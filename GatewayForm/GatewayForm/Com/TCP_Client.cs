using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using System.Timers;
using CM = GatewayForm.Common;

namespace GatewayForm
{
    // State object for receiving data from remote device.
    public class StateTCPClient
    {
        // Client socket.
        //public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        //public byte[] result_data_byte = new byte[0];
        public byte[] result_data_byte = new byte[0];
    }

    //public delegate void SocketReceivedHandler(string msg);

    public class TCP_Client
    {
        private Socket tcp_client;

        /// <summary>
        /// Byte Data of Frame Format
        /// </summary>
        public bool connect_ok = false;

        // ManualResetEvent instances signal completion.
        private AutoResetEvent connectDone = new AutoResetEvent(false);
        private AutoResetEvent sendDone = new AutoResetEvent(false);
        public AutoResetEvent receiveDone = new AutoResetEvent(false);
        //Ping connection
        //private System.Timers.Timer pingTimer = new System.Timers.Timer() { Interval = 10000 };
        //private AutoResetEvent waiter = new AutoResetEvent(false);
        private bool File_Mode = false;
        // Event Handler
        //public bool recv_flag = false;
        //public bool alive = true;
        private static int retry_count = 3;

        // The response from the remote device.

        public TCP_Client()
        {
            // TODO: Complete member initialization
            //this.form1 = form1;
        }

        #region Connect

        public void InitClient(string ip_server, int port)
        {
            try
            {
                #region Ping ICMP
                /*
                byte[] icmp_test = Encoding.ASCII.GetBytes("xxxx");
                PingOptions options = new PingOptions(64, true);
                Ping pingsender = new Ping();
                options.DontFragment = true;
                pingsender.PingCompleted += (pingSource, rev) =>
                {
                    if (rev.Cancelled)
                    {
                        Console.WriteLine("Ping canceled.");

                        // Let the main thread resume. 
                        // UserToken is the AutoResetEvent object that the main thread 
                        // is waiting for.
                        ((AutoResetEvent)rev.UserState).Set();
                    }

                    // If an error occurred, display the exception to the user.
                    if (rev.Error != null)
                    {
                        Console.WriteLine("Ping failed:");
                        Console.WriteLine(rev.Error.ToString());

                        // Let the main thread resume. 
                        ((AutoResetEvent)rev.UserState).Set();
                    }

                    if (rev.Reply.Status == IPStatus.Success)
                    {
                        if (alive == false)
                        {
                            CM.Log_Raise("Re-connect");
                            pingTimer.Interval = 5000;
                            alive = true;
                        }
                    }
                    else
                    {
                        alive = false;
                        CM.Log_Raise("No Connection");
                        pingTimer.Interval = 10000;
                    }
                    ((AutoResetEvent)rev.UserState).Set();
                };*/
                #endregion

                // Connect to a remote device.
                IPEndPoint remoteEP;
                IPAddress ipAddress = Dns.GetHostAddresses(ip_server)[0];
                remoteEP = new IPEndPoint(ipAddress, port);

                // Create a TCP/IP socket.
                tcp_client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                tcp_client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                // Connect to the remote endpoint.
                tcp_client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), tcp_client);
                if (connectDone.WaitOne(5000))
                {
                    CM.SetKeepAliveValues(tcp_client, true, 2000, 1000);
                    connect_ok = true;
                    Receive_Command_Handler();

                    Start_Command_Process(CM.COMMAND.CONNECTION_REQUEST_CMD);
                    /*pingTimer.Elapsed += (sender, e) =>
                        {
                            //pingsender.SendAsync(ipAddress, 120, icmp_test, options, waiter);
                            //waiter.WaitOne();
                        };*/
                    //pingTimer.Start();
                }
                else
                {
                    MessageBox.Show("Error Socket");
                }
            }
            catch (IOException e)
            {
                MessageBox.Show(e.ToString());
            }
            catch (SocketException ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// Handle starting connection
        /// </summary>
        /// <param name="ar"></param>
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;
                if (client.Connected)
                {
                    // Complete the connection.
                    client.EndConnect(ar);

                    // Signal that the connection has been made.
                    connectDone.Set();
                }
            }
            catch (IOException e)
            {
                MessageBox.Show(e.ToString());
            }
            catch (SocketException se)
            {
                //connectDone.Set();
                MessageBox.Show(se.ToString()); //2
            }
            catch (ObjectDisposedException)
            {
                CM.Log_Raise("Abort due to close");
            }
        }

        #endregion

        #region Send

        /// <summary>
        /// Send byte raw of frame data 
        /// </summary>
        /// <param name="b_frame"></param> byte array of sending frame
        private void SendFrame(byte[] b_frame)
        {
            if (!tcp_client.Connected)
            {
                // Connection is terminated, either by force or willingly
                MessageBox.Show("Error transmit because socket is terminated");
                connect_ok = false;
            }
            else
            {
                tcp_client.BeginSend(b_frame, 0, b_frame.Length, 0, new AsyncCallback(SendCallback), tcp_client);
                sendDone.WaitOne();
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Check connection
                if (!tcp_client.Connected)
                {
                    // Connection is terminated, either by force or willingly
                    MessageBox.Show("Sending error because socket is terminated");
                    connect_ok = false;
                }
                else
                {
                    // Complete sending the data to the remote device.
                    ((Socket)ar.AsyncState).EndSend(ar);

                    // Signal that all bytes have been sent.
                    sendDone.Set();
                }
            }
            catch (IOException e)
            {
                MessageBox.Show(e.ToString());
            }
            catch (SocketException e)
            {
                MessageBox.Show(e.ToString()); //4
            }
            catch (ObjectDisposedException)
            {
                CM.Log_Raise("Abort due to close");
            }
        }

        /// <summary>
        /// Send connection request command
        /// </summary>
        private void Send_ConnectionRequest()
        {
            // Encoding follow Sub Frame Format
            CM.SubFrameFormat sub_fmt_send = new CM.SubFrameFormat();
            sub_fmt_send.header = (byte)CM.HEADER.PACKET_HDR; //header hex: E5
            //4bytes (meta) + 2bytes(truncate) + 1byte(cheksum) + 2bytes(lenth)
            sub_fmt_send.length = (ushort)CM.LENGTH.SUB_FRAME_NON_DATA;

            // connection request command frame format
            CM.FrameFormat req_conn_fmt = new CM.FrameFormat();
            req_conn_fmt.command = (byte)CM.COMMAND.CONNECTION_REQUEST_CMD;
            req_conn_fmt.length = (ushort)CM.LENGTH.FRAME_NON_DATA;
            sub_fmt_send.metal_data = CM.Encode_Frame(req_conn_fmt);

            sub_fmt_send.truncate = 0x01;

            byte[] byte_req = CM.Encode_SubFrame(sub_fmt_send);
            // Begin sending Connection Request Command.
            SendFrame(byte_req);
        }

        /// <summary>
        /// Send Non user data Command. 
        /// Get Configuration data.
        /// Start/stop operation.
        /// </summary>
        /// <param name="command"></param>
        public void Get_Command_Send(CM.COMMAND command)
        {
            CM.SubFrameFormat sub_fmt_get = new CM.SubFrameFormat();
            CM.FrameFormat fmt_get = new CM.FrameFormat();
            fmt_get.command = (byte)command;
            fmt_get.length = (ushort)CM.LENGTH.FRAME_NON_DATA;

            sub_fmt_get.metal_data = CM.Encode_Frame(fmt_get);
            sub_fmt_get.header = (byte)CM.HEADER.PACKET_HDR;
            sub_fmt_get.length = (ushort)CM.LENGTH.SUB_FRAME_NON_DATA;
            sub_fmt_get.truncate = 0x01;

            byte[] byte_get_cmd = CM.Encode_SubFrame(sub_fmt_get);
            SendFrame(byte_get_cmd);
        }
        /// <summary>
        /// Send Get Power Configuration Command.
        /// </summary>
        /// <param name="command"></param>
        public void Get_Command_Power(CM.COMMAND command, byte power_mode)
        {
            CM.SubFrameFormat sub_fmt_get = new CM.SubFrameFormat();
            CM.FrameFormat fmt_get = new CM.FrameFormat();
            fmt_get.command = (byte)command;
            fmt_get.length = (ushort)CM.LENGTH.FRAME_NON_DATA + 1;
            fmt_get.metal_data = new byte[1];
            fmt_get.metal_data[0] = power_mode;

            sub_fmt_get.metal_data = CM.Encode_Frame(fmt_get);
            sub_fmt_get.header = (byte)CM.HEADER.PACKET_HDR;
            sub_fmt_get.length = (ushort)CM.LENGTH.SUB_FRAME_NON_DATA + 1;
            sub_fmt_get.truncate = 0x01;

            byte[] byte_get_cmd = CM.Encode_SubFrame(sub_fmt_get);
            SendFrame(byte_get_cmd);
        }
        /// <summary>
        /// Send Set Config data
        /// </summary>
        /// <param name="command"></param>
        /// <param name="user_data"></param>
        public void Set_Command_Send(CM.COMMAND command, String user_data)
        {
            CM.FrameFormat fmt_set = new CM.FrameFormat();
            fmt_set.command = (byte)command;
            byte[] user_byte = Encoding.ASCII.GetBytes(user_data);
            fmt_set.length = (ushort)((ushort)CM.LENGTH.FRAME_NON_DATA + user_byte.Length);
            fmt_set.metal_data = user_byte;

            /* Byte data of Frame Format*/
            byte[] sub_fmt_byte = CM.Encode_Frame(fmt_set);
            Send_Bytes_Stream(sub_fmt_byte);
        }

        public void Set_Command_Send_Bytes(CM.COMMAND command, byte[] user_bytes)
        {
            CM.FrameFormat fmt_set = new CM.FrameFormat();
            fmt_set.command = (byte)command;
            fmt_set.length = (ushort)((ushort)CM.LENGTH.FRAME_NON_DATA + user_bytes.Length);
            fmt_set.metal_data = user_bytes;

            /* Byte data of Frame Format*/
            byte[] sub_fmt_byte = CM.Encode_Frame(fmt_set);
            Send_Bytes_Stream(sub_fmt_byte);
        }

        private void Send_Bytes_Stream(byte[] frame_data_byte)
        {
            UInt16 num_packet, last_packet_byte, len_transmit;
            UInt16 idx, len_byte_fmt;
            idx = 0;
            len_byte_fmt = (ushort)frame_data_byte.Length;
            num_packet = (ushort)(len_byte_fmt / ((ushort)CM.LENGTH.MAX_SIZE_TCP_META_DATA));
            last_packet_byte = (ushort)(len_byte_fmt % ((ushort)CM.LENGTH.MAX_SIZE_TCP_META_DATA));
            if (last_packet_byte > 0)
                num_packet += 1;
            len_transmit = (ushort)CM.LENGTH.MAX_SIZE_TCP_META_DATA; //1024 - 6
            for (int i = 0; i < num_packet; i++)
            {
                if (i == num_packet - 1)
                {
                    if (last_packet_byte > 0)
                        len_transmit = (ushort)last_packet_byte;
                }
                CM.SubFrameFormat sub_fmt_send = new CM.SubFrameFormat();
                sub_fmt_send.header = (byte)CM.HEADER.PACKET_HDR;
                sub_fmt_send.length = (ushort)(len_transmit + 5);
                sub_fmt_send.metal_data = new byte[len_transmit];
                Buffer.BlockCopy(frame_data_byte, idx, sub_fmt_send.metal_data, 0, len_transmit);
                idx += len_transmit;
                sub_fmt_send.truncate = (ushort)(num_packet - i);
                byte[] byte_tcp = CM.Encode_SubFrame(sub_fmt_send);
                SendFrame(byte_tcp);
            }
        }

        public void Send_File(byte[] file_stream, string info)
        {
            File_Mode = true;
            //Part 0 (Info)
            Set_Command_Send(CM.COMMAND.FIRMWARE_UPDATE_CMD, info);
            if (receiveDone.WaitOne(3000))
            {
                //Part file
                byte[] part_byte;
                UInt16 num_part, len_last_part, len_part, percent;
                int id = 0;
                len_part = (ushort)CM.LENGTH.CHUNK_SIZE_FILE;
                num_part = (ushort)(file_stream.Length / len_part);
                len_last_part = (ushort)(file_stream.Length % len_part);
                if (len_last_part > 0)
                    num_part += 1;
                for (int ip = 0; ip < num_part; ip++)
                {
                    if (ip == num_part - 1)
                    {
                        if (len_last_part > 0)
                            len_part = (ushort)len_last_part;
                    }
                    part_byte = new byte[len_part];
                    Buffer.BlockCopy(file_stream, id, part_byte, 0, len_part);
                    id += len_part;
                    CM.FrameFormat fmt_set = new CM.FrameFormat();
                    fmt_set.command = (byte)CM.COMMAND.FIRMWARE_UPDATE_CMD;
                    fmt_set.length = (ushort)((ushort)CM.LENGTH.FRAME_NON_DATA + len_part);
                    fmt_set.metal_data = part_byte;
                    percent = (ushort)((ip + 1) * (100 / num_part));
                    /* Byte data of Frame Format*/
                    byte[] sub_fmt_byte = CM.Encode_Frame(fmt_set);
                    Send_Packets(sub_fmt_byte);
                    if (!receiveDone.WaitOne(7000))
                    {
                        CM.Log_Raise("Failed to update.");
                        break;
                    }
                    else
                        CM.Cmd_Raise("Update FW\n" + percent.ToString() + "\n");
                }
            }
            else
                MessageBox.Show("Wrong File Info");
            File_Mode = false;
        }

        private void Send_Packets(byte[] frame_data_byte)
        {
            UInt16 num_packet, last_packet_byte, len_transmit;
            UInt16 idx, len_byte_fmt;
            idx = 0;
            len_byte_fmt = (ushort)frame_data_byte.Length;
            num_packet = (ushort)(len_byte_fmt / ((ushort)CM.LENGTH.MAX_SIZE_TCP_META_DATA));
            last_packet_byte = (ushort)(len_byte_fmt % ((ushort)CM.LENGTH.MAX_SIZE_TCP_META_DATA));
            if (last_packet_byte > 0)
                num_packet += 1;
            len_transmit = (ushort)CM.LENGTH.MAX_SIZE_TCP_META_DATA; //1024 - 6
            for (int i = 0; i < num_packet; i++)
            {
                if (i == num_packet - 1)
                {
                    if (last_packet_byte > 0)
                        len_transmit = (ushort)last_packet_byte;
                }
                CM.SubFrameFormat sub_fmt_send = new CM.SubFrameFormat();
                sub_fmt_send.header = (byte)CM.HEADER.PACKET_HDR;
                sub_fmt_send.length = (ushort)(len_transmit + 5);
                sub_fmt_send.metal_data = new byte[len_transmit];
                Buffer.BlockCopy(frame_data_byte, idx, sub_fmt_send.metal_data, 0, len_transmit);
                idx += len_transmit;
                sub_fmt_send.truncate = (ushort)(num_packet - i);

                byte[] byte_tcp = CM.Encode_SubFrame(sub_fmt_send);
                SendFrame(byte_tcp);
            }
        }

        public async void Start_Command_Process(CM.COMMAND cmd)
        {
            int task = await Task.Run(() => Command_Process(cmd));
        }

        private int Command_Process(CM.COMMAND command)
        {
            switch (command)
            {
                case CM.COMMAND.CONNECTION_REQUEST_CMD:
                    Send_ConnectionRequest();
                    receiveDone.WaitOne(2000);
                    //Get Gateway Configuration
                    Get_Command_Send(CM.COMMAND.GET_CONFIGURATION_CMD);
                    receiveDone.WaitOne(2000);
                    //Antena
                    Get_Command_Send(CM.COMMAND.ANTENA_CMD);
                    receiveDone.WaitOne(2000);
                    break;
                case CM.COMMAND.GET_POWER_CMD:
                    Get_Command_Power(CM.COMMAND.GET_POWER_CMD, 0);
                    receiveDone.WaitOne(2000);
                    Get_Command_Power(CM.COMMAND.GET_POWER_CMD, 1);
                    receiveDone.WaitOne(2000);
                    break;
                case CM.COMMAND.GET_POWER_MODE_CMD:
                    Get_Command_Power(CM.COMMAND.GET_BLF_CMD, 0);
                    receiveDone.WaitOne(2000);
                    Get_Command_Power(CM.COMMAND.GET_BLF_CMD, 1);
                    receiveDone.WaitOne(2000);
                    Get_Command_Power(CM.COMMAND.GET_BLF_CMD, 2);
                    receiveDone.WaitOne(2000);
                    receiveDone.Reset();
                    Get_Command_Power(CM.COMMAND.GET_POWER_CMD, 0);
                    receiveDone.WaitOne(2000);
                    Get_Command_Power(CM.COMMAND.GET_POWER_CMD, 1);
                    receiveDone.WaitOne(2000);
                    Get_Command_Send(CM.COMMAND.GET_POWER_MODE_CMD);
                    receiveDone.WaitOne(2000);
                    break;
                case CM.COMMAND.GET_BLF_CMD:
                    Get_Command_Power(CM.COMMAND.GET_BLF_CMD, 0);
                    receiveDone.WaitOne(2000);
                    Get_Command_Power(CM.COMMAND.GET_BLF_CMD, 1);
                    receiveDone.WaitOne(2000);
                    Get_Command_Power(CM.COMMAND.GET_BLF_CMD, 2);
                    receiveDone.WaitOne(2000);
                    break;
                case CM.COMMAND.GET_RFID_CONFIGURATION_CMD:
                    Get_Command_Send(CM.COMMAND.GET_RFID_CONFIGURATION_CMD);
                    receiveDone.WaitOne(2000);
                    break;
                case CM.COMMAND.DIS_CONNECT_CMD:
                    Get_Command_Send(CM.COMMAND.DIS_CONNECT_CMD);
                    receiveDone.WaitOne(2000);
                    Free();
                    break;
                default:
                    break;
            }
            return 1;
        }

        #endregion

        #region Received

        /// <summary>
        /// Handler Request Connection response
        /// </summary>
        /// <param name="byte_receive"></param>
        private void Request_Connection_Handler(byte[] byte_receive)
        {
            if (0x00 == byte_receive[0])
            {
                CM.Log_Raise("Accepted!");
            }
            else
            {
                if (retry_count > 0)
                {
                    MessageBox.Show("Fail request connection\n Retry!");
                    retry_count--;
                    Start_Command_Process(CM.COMMAND.CONNECTION_REQUEST_CMD);
                }
                else
                {
                    CM.Log_Raise("Retry Failed. Closed Socket.");
                    retry_count = 3;
                    Free();
                    return;
                }
            }
        }

        /// <summary>
        /// Start when have incomming tcp
        /// </summary>
        /// <param name="ar"></param>
        private void Receive_Command_Callback(IAsyncResult ar)
        {
            try
            {
                StateTCPClient state = (StateTCPClient)ar.AsyncState;
                //Socket client = state.workSocket;

                // Read data from the remote device.
                int bytesRead = tcp_client.EndReceive(ar);
                if (bytesRead != 0)
                {
                    byte[] meta_sub = CM.Decode_SubFrame(state.buffer, bytesRead);
                    if (meta_sub != null)
                    {
                        if (File_Mode)
                        {
                            byte ack_file = CM.Decode_Frame_ACK((byte)CM.COMMAND.FIRMWARE_UPDATE_CMD, meta_sub);
                            if (0x00 == ack_file)
                                receiveDone.Set();
                            else
                                MessageBox.Show("Failed to update. Please try again!");
                            Receive_Command_Handler();
                        }
                        else
                        {
                            Array.Resize(ref state.result_data_byte, state.result_data_byte.Length + meta_sub.Length);
                            Buffer.BlockCopy(meta_sub, 0, state.result_data_byte, state.result_data_byte.Length - meta_sub.Length, meta_sub.Length);
                            if (0x01 != state.buffer[bytesRead - 2])
                                tcp_client.BeginReceive(state.buffer, 0, StateTCPClient.BufferSize, 0,
                                   new AsyncCallback(Receive_Command_Callback), state);
                            else
                            {
                                if ((byte)CM.COMMAND.CONNECTION_REQUEST_CMD == state.result_data_byte[0])
                                    Request_Connection_Handler(CM.Decode_Frame((byte)CM.COMMAND.CONNECTION_REQUEST_CMD, state.result_data_byte));

                                else
                                    CM.Data_Receive_Handler(state.result_data_byte);

                                receiveDone.Set();
                                Receive_Command_Handler();
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Wrong Sub Frame Packet");
                        Receive_Command_Handler();
                    }
                }
                else
                {
                    MessageBox.Show("Server closed\nNo more data will be sent.");
                    Free();
                }
            }
            catch (SocketException se)
            {
                if (connect_ok)
                {
                    MessageBox.Show(se.ToString());
                    if (se.ErrorCode == 10061)
                        CM.Cmd_Raise("Keep Alive Timeout\n");
                }
            }
            catch (IOException e)
            {
                MessageBox.Show(e.ToString());
            }
            catch (ObjectDisposedException)
            {
                if (connect_ok)
                    CM.Log_Raise("Abort due to close");
            }
        }

        /// <summary>
        /// Receive Command Data Response.
        /// </summary>
        /// <param name="command_type"></param>
        private void Receive_Command_Handler()
        {
            if (tcp_client.Connected)
            {
                // Create the state object.
                StateTCPClient state = new StateTCPClient();

                // Begin receiving the data from the remote device.
                tcp_client.BeginReceive(state.buffer, 0, StateTCPClient.BufferSize, 0,
                    new AsyncCallback(Receive_Command_Callback), state);
            }
            else
            {
                MessageBox.Show("Receive error because socket is terminated");
                connect_ok = false;
            }

        }

        #endregion
        /// <summary>
        /// Free TCP resource
        /// </summary>
        public void Free()
        {
            try
            {
                connect_ok = false;
                tcp_client.Shutdown(SocketShutdown.Both);
                //tcp_client.Disconnect(true);
                tcp_client.Close();
                //pingTimer.Stop();
                CM.Log_Raise("Disconnected");
            }
            catch (SocketException se)
            {
                MessageBox.Show(se.ToString());
            }
            catch (ObjectDisposedException e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        ~TCP_Client()
        {
            // Release the socket.

        }

    }

}
