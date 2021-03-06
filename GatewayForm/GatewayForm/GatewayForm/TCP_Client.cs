﻿using System;
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
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
    }

    //public delegate void SocketReceivedHandler(string msg);

    public class TCP_Client
    {
        private Socket tcp_client;

        /// <summary>
        /// Byte Data of Frame Format
        /// </summary>
        private byte[] result_data_byte = new byte[0];
        public bool connect_ok = false;
        
        // ManualResetEvent instances signal completion.
        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);
        //Ping connection
        //private System.Timers.Timer pingTimer = new System.Timers.Timer() { Interval = 10000 };
        private ManualResetEvent waiter = new ManualResetEvent(false);
        // Event Handler
        public event SocketReceivedHandler MessageReceived;
        public event SocketReceivedHandler ConfigMessage;
        public event SocketReceivedHandler Log_Msg;
        private volatile bool start_enable = false;
        public bool alive = true;
        private static int retry_count = 3;

        // The response from the remote device.

        public TCP_Client()
        {
            // TODO: Complete member initialization
            //this.form1 = form1;
        }

        private void Log_Raise(string log_str)
        {
            SocketReceivedHandler logmsg = Log_Msg;
            if (logmsg != null)
                logmsg(log_str);
        }

        private void Cmd_Raise(string cmd_str)
        {
            SocketReceivedHandler cmd_msg = ConfigMessage;
            if (cmd_msg != null)
                cmd_msg(cmd_str);
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
                            Log_Raise("Re-connect");
                            pingTimer.Interval = 5000;
                            alive = true;
                        }
                    }
                    else
                    {
                        alive = false;
                        Log_Raise("No Connection");
                        pingTimer.Interval = 10000;
                    }
                    ((AutoResetEvent)rev.UserState).Set();
                };*/
                #endregion

                // Connect to a remote device.
                // Establish the remote endpoint for the socket.
                IPEndPoint remoteEP;
                IPAddress ipAddress;
                //if (ip_server.Contains("local"))
                //{
                //IPHostEntry ipHostInfo = Dns.Resolve();
                //IPAddress ipAddress = ipHostInfo.AddressList[0];
                //remoteEP = new IPEndPoint(ipAddress, port);
                //}
                //else
                //{
                ipAddress = IPAddress.Parse(ip_server);
                remoteEP = new IPEndPoint(ipAddress, port);
                //}

                // Create a TCP/IP socket.
                tcp_client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                tcp_client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                // Connect to the remote endpoint.
                tcp_client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), tcp_client);
                if (connectDone.WaitOne(5000))
                {
                    //Send Connection Request
                    Send_ConnectionRequest();

                    //Receive the response status
                    Receive_Command_Handler(CM.COMMAND.CONNECTION_REQUEST_CMD);

                    //Get Gateway Configuration
                    Get_Command_Send(CM.COMMAND.GET_CONFIGURATION_CMD);
                    Receive_Command_Handler(CM.COMMAND.GET_CONFIGURATION_CMD);
                    /*pingTimer.Elapsed += (sender, e) =>
                        {
                            //pingsender.SendAsync(ipAddress, 120, icmp_test, options, waiter);
                            //waiter.WaitOne();
                        };*/
                    //pingTimer.Start();
                }
                else
                {
                    throw new SocketException();
                }
            }
            catch (IOException e)
            {
                MessageBox.Show(e.ToString());
            }
            catch (SocketException ex)
            {
                //pingTimer.Stop();
                //receiveDone.Set();
                MessageBox.Show(ex.ToString());
                //this.Free();
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
            catch (SocketException)
            {
                //connect_ok = false;
                //connectDone.Set();
                //MessageBox.Show(e.ToString()); //2
            }
            catch (ObjectDisposedException)
            {
                Log_Raise("Abort due to close");
            }
        }

        #endregion

        #region Send

        public bool IsConnected
        {
            get
            {
                try
                {
                    if (tcp_client != null && tcp_client.Connected)
                    {
                        StateTCPClient state = new StateTCPClient();
                        state.workSocket = tcp_client;
                        alive = false;

                        // Begin receiving the data from the remote device.
                        tcp_client.BeginReceive(state.buffer, 0, StateTCPClient.BufferSize, 0,
                            new AsyncCallback(Keep_Alive_Callback), state);
                        waiter.WaitOne(900);
                        waiter.Reset();
                        if (alive == false)
                        {
                            Log_Raise("No Connection");
                            //pingTimer.Interval = 5000;
                            return false;
                        }
                        else
                        {
                            return true;
                            //pingTimer.Interval = 10000;
                        }
                        // Detect if client disconnected
                        /*if (tcp_client.Poll(0, SelectMode.SelectRead))
                        {
                            byte[] buff = new byte[1];
                            if (tcp_client.Receive(buff, SocketFlags.Peek) == 0)
                            {
                                // Client disconnected
                                return false;
                            }
                            else
                            {
                                return true;
                            }
                        }*/
                    }
                    else
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }

        private void Keep_Alive_Callback(IAsyncResult ar)
        {
            try
            {
                StateTCPClient state = (StateTCPClient)ar.AsyncState;
                Socket client = state.workSocket;
                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    byte[] meta_sub = CM.Decode_SubFrame(state.buffer, bytesRead);
                    byte asaack = CM.Decode_Frame_ACK((byte)CM.COMMAND.STOP_OPERATION_CMD, meta_sub);
                    if (0x00 == asaack)
                    {
                        alive = true;
                        if (!this.IsConnected)
                            Log_Raise("Re-Connect.");
                        receiveDone.Set();
                    }
                    else
                    {
                        Log_Raise("Error");
                    }
                }
            }
            catch (SocketException e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        /// <summary>
        /// Send byte raw of frame data 
        /// </summary>
        /// <param name="b_frame"></param> byte array of sending frame
        private void SendFrame(byte[] b_frame)
        {
            try
            {
                // Begin sending the packet to the remote device.
                if (tcp_client.Connected)
                {
                    tcp_client.BeginSend(b_frame, 0, b_frame.Length, 0, new AsyncCallback(SendCallback), tcp_client);
                    sendDone.WaitOne();
                    sendDone.Reset();
                }
                else
                    connect_ok = false;
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.ToString());
            }
            catch (SocketException ex)
            {
                sendDone.Set();
                MessageBox.Show(ex.ToString()); //3
            }
            catch (ObjectDisposedException)
            {
                Log_Raise("Abort due to close");
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Complete sending the data to the remote device.
                if (((Socket)ar.AsyncState).Connected)
                {
                    ((Socket)ar.AsyncState).EndSend(ar);

                    // Signal that all bytes have been sent.
                    sendDone.Set();
                }
                else
                    connect_ok = false;
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
                Log_Raise("Abort due to close");
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
            if (command == CM.COMMAND.STOP_OPERATION_CMD)
                start_enable = false;
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
            Send_Packets(sub_fmt_byte);
        }

        public void Set_Command_Send_Bytes(CM.COMMAND command, byte[] user_bytes)
        {
            CM.FrameFormat fmt_set = new CM.FrameFormat();
            fmt_set.command = (byte)command;
            fmt_set.length = (ushort)((ushort)CM.LENGTH.FRAME_NON_DATA + user_bytes.Length);
            fmt_set.metal_data = user_bytes;

            /* Byte data of Frame Format*/
            byte[] sub_fmt_byte = CM.Encode_Frame(fmt_set);
            Send_Packets(sub_fmt_byte);
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
                connect_ok = true;
                Log_Raise("Accepted!");
                //MessageBox.Show("Accepted!");
            }
            else
            {
                if (retry_count > 0)
                {
                    MessageBox.Show("Fail request connection\n Retry!");
                    result_data_byte = new byte[0];
                    retry_count--;

                    Send_ConnectionRequest();
                    Receive_Command_Handler(CM.COMMAND.CONNECTION_REQUEST_CMD);
                }
                else
                {
                    Log_Raise("Retry Failed. Closed Socket.");
                    retry_count = 3;
                    tcp_client.Close();

                    // Chua retry button duoc. form1.ConnectSocket_btn.Text = "Connect";
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
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                StateTCPClient state = (StateTCPClient)ar.AsyncState;
                Socket client = state.workSocket;
                //Thread.Sleep(1);

                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);
                if (bytesRead > 0)
                {
                    byte[] meta_sub = CM.Decode_SubFrame(state.buffer, bytesRead);
                    if (meta_sub != null)
                    {
                        Array.Resize(ref result_data_byte, result_data_byte.Length + meta_sub.Length);
                        Buffer.BlockCopy(meta_sub, 0, result_data_byte, result_data_byte.Length - meta_sub.Length, meta_sub.Length);
                        if (0x01 == state.buffer[bytesRead - 2])
                            receiveDone.Set();
                        else
                            client.BeginReceive(state.buffer, 0, StateTCPClient.BufferSize, 0,
                            new AsyncCallback(Receive_Command_Callback), state);
                    }
                    else
                    {
                        MessageBox.Show("Wrong Message");
                    }
                }
            }
            catch (IOException e)
            {
                MessageBox.Show(e.ToString());
            }
            catch (SocketException se)
            {
                MessageBox.Show(se.ToString());
            }
            catch (ObjectDisposedException)
            {
                Log_Raise("Abort due to close");
            }
        }

        /// <summary>
        /// Receive Command Data Response.
        /// </summary>
        /// <param name="command_type"></param>
        public void Receive_Command_Handler(CM.COMMAND command_type)
        {
            // Create the state object.
            StateTCPClient state = new StateTCPClient();
            state.workSocket = tcp_client;

            // Begin receiving the data from the remote device.
            if (tcp_client.Connected)
            {
                tcp_client.BeginReceive(state.buffer, 0, StateTCPClient.BufferSize, 0, new AsyncCallback(Receive_Command_Callback), state);
                if (receiveDone.WaitOne(2000))
                {
                    receiveDone.Reset();

                    // After received all packet
                    if (result_data_byte != null)
                    {
                        byte info_ack;
                        string data_response = null;
                        byte[] byte_bits = null;
                        switch (command_type)
                        {
                            /* connection request */
                            case CM.COMMAND.CONNECTION_REQUEST_CMD:
                                Request_Connection_Handler(CM.Decode_Frame((byte)CM.COMMAND.CONNECTION_REQUEST_CMD, result_data_byte));
                                break;
                            /* configuration */
                            case CM.COMMAND.GET_CONFIGURATION_CMD:
                                data_response = CM.Get_Data(CM.Decode_Frame((byte)CM.COMMAND.GET_CONFIGURATION_CMD, result_data_byte));
                                if (data_response.Length > 0)
                                    Cmd_Raise(data_response);
                                break;
                            case CM.COMMAND.SET_CONFIGURATION_CMD:
                                info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.SET_CONFIGURATION_CMD, result_data_byte);
                                if (0x00 == info_ack)
                                    Log_Raise("Set GW Config done");
                                else
                                    Log_Raise("Failed set Config");
                                break;
                            /* RFID configuration */
                            case CM.COMMAND.GET_RFID_CONFIGURATION_CMD:
                                data_response = CM.Get_Data(CM.Decode_Frame((byte)CM.COMMAND.GET_RFID_CONFIGURATION_CMD, result_data_byte));
                                if (data_response.Length > 0)
                                    Cmd_Raise(data_response);
                                MessageBox.Show(data_response);
                                break;
                            case CM.COMMAND.SET_RFID_CONFIGURATION_CMD:
                                info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.SET_RFID_CONFIGURATION_CMD, result_data_byte);
                                if (0x00 == info_ack)
                                    Log_Raise("Set RFID done");
                                else
                                    Log_Raise("Failed set RFID");
                                break;
                            /* Port Properties */
                            case CM.COMMAND.GET_PORT_PROPERTIES_CMD:
                                data_response = CM.Get_Data(CM.Decode_Frame((byte)CM.COMMAND.GET_PORT_PROPERTIES_CMD, result_data_byte));
                                if (data_response.Length > 0)
                                    Cmd_Raise(data_response);
                                MessageBox.Show(data_response);
                                break;
                            case CM.COMMAND.SET_PORT_PROPERTIES_CMD:
                                info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.SET_PORT_PROPERTIES_CMD, result_data_byte);
                                if (0x00 == info_ack)
                                    Log_Raise("Set connection done");
                                else
                                    Log_Raise("Failed set connection");
                                break;
                            case CM.COMMAND.DIS_CONNECT_CMD:
                                info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.DIS_CONNECT_CMD, result_data_byte);
                                if (0x00 != info_ack)
                                    MessageBox.Show("Failed disconnect");
                                break;
                            /* start operate */
                            case CM.COMMAND.START_OPERATION_CMD:
                                info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.START_OPERATION_CMD, result_data_byte);
                                if (0x00 == info_ack)
                                {
                                    start_enable = true;
                                    //pingTimer.Stop();
                                    Log_Raise("Inventory Mode");
                                    Receive_Data_Handler();
                                }
                                else
                                    MessageBox.Show("Failed start operation");
                                break;
                            //Power RFID
                            case CM.COMMAND.GET_POWER_CMD:
                                byte_bits = CM.Decode_Frame((byte)CM.COMMAND.GET_POWER_CMD, result_data_byte);
                                if (byte_bits != null)
                                {
                                    if (0x00 == byte_bits[0])
                                        Cmd_Raise("Power RFID\n" + byte_bits[1].ToString() + "\n");
                                    else
                                        Log_Msg("Fail get power");
                                }
                                break;
                            case CM.COMMAND.SET_POWER_CMD:
                                info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.SET_POWER_CMD, result_data_byte);
                                if (0x00 == info_ack)
                                    Log_Raise("Set Power done");
                                else
                                    Log_Raise("Failed Set Power");
                                break;
                            //Region Configuration
                            case CM.COMMAND.GET_REGION_CMD:
                                byte_bits = CM.Decode_Frame((byte)CM.COMMAND.GET_REGION_CMD, result_data_byte);
                                if (byte_bits != null)
                                {
                                    if (0x00 == byte_bits[0])
                                        Cmd_Raise("Region RFID\n" + byte_bits[1].ToString() + "\n");
                                    else
                                        Log_Msg("Fail get region");
                                }
                                break;
                            case CM.COMMAND.SET_REGION_CMD:
                                info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.SET_REGION_CMD, result_data_byte);
                                if (0x00 == info_ack)
                                    Log_Raise("Set Region done");
                                else
                                    Log_Raise("Failed set region");
                                break;
                            //Power Mode Configuration
                            case CM.COMMAND.GET_POWER_MODE_CMD:
                                byte_bits = CM.Decode_Frame((byte)CM.COMMAND.GET_POWER_MODE_CMD, result_data_byte);
                                if (byte_bits != null)
                                {
                                    if (0x00 == byte_bits[0])
                                        Cmd_Raise("Power Mode RFID\n" + byte_bits[1].ToString() + "\n");
                                    else
                                        Log_Msg("Fail get power mode");
                                }
                                break;
                            case CM.COMMAND.SET_POWER_MODE_CMD:
                                info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.SET_POWER_MODE_CMD, result_data_byte);
                                if (0x00 == info_ack)
                                    Log_Raise("Set Power Mode done");
                                else
                                    Log_Raise("Failed set power mode");
                                break;
                            // Change Connection Type
                            case CM.COMMAND.SET_CONN_TYPE_CMD:
                                data_response = CM.Get_Data(CM.Decode_Frame((byte)CM.COMMAND.SET_CONN_TYPE_CMD, result_data_byte));
                                if (data_response.Length > 0)
                                    Cmd_Raise(data_response);
                                break;
                            case CM.COMMAND.GET_BLF_CMD:
                                byte_bits = CM.Decode_Frame((byte)CM.COMMAND.GET_BLF_CMD, result_data_byte);
                                if (byte_bits != null)
                                {
                                    if (0x00 == byte_bits[0])
                                        Cmd_Raise("BLF Setting\n" + byte_bits[1].ToString() + "\n");
                                    else
                                        Log_Msg("Fail get power");
                                }
                                break;
                            case CM.COMMAND.SET_BLF_CMD:
                                info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.SET_BLF_CMD, result_data_byte);
                                if (0x00 == info_ack)
                                    Log_Raise("Set BLF done");
                                else
                                    Log_Raise("Failed BLF");
                                break;
                            default:
                                break;
                        }

                        result_data_byte = new byte[0];
                    }
                }
                else
                {
                    receiveDone.Reset();
                    result_data_byte = new byte[0];
                }
            }
            else
                connect_ok = false;
        }

        private void Receive_Data_Callback(IAsyncResult ar)
        {
            try
            {
                StateTCPClient state = (StateTCPClient)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.
                Thread.Sleep(1);
                int bytesRead = client.EndReceive(ar);
                if (bytesRead > 0)
                {
                    byte[] meta_sub = CM.Decode_SubFrame(state.buffer, bytesRead);
                    if (meta_sub != null)
                    {
                        Array.Resize(ref result_data_byte, result_data_byte.Length + meta_sub.Length);
                        Buffer.BlockCopy(meta_sub, 0, result_data_byte, result_data_byte.Length - meta_sub.Length, meta_sub.Length);
                    }

                    if (0x01 != state.buffer[bytesRead - 2])
                        client.BeginReceive(state.buffer, 0, StateTCPClient.BufferSize, 0,
                        new AsyncCallback(Receive_Data_Callback), state);
                    else
                    {
                        if (result_data_byte != null)
                        {
                            if (!start_enable)
                            {
                                byte stop_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.STOP_OPERATION_CMD, result_data_byte);
                                if (0x00 == stop_ack)
                                {
                                    Log_Raise("Stop operate");
                                    //pingTimer.Start();
                                }
                                else
                                    MessageBox.Show("Failed stop operate");
                                result_data_byte = new byte[0];
                            }
                            else
                            {
                                /* TAG ID */
                                var messageReceived = MessageReceived;
                                byte[] byte_user = CM.Decode_Frame((byte)CM.COMMAND.REQUEST_TAG_ID_CMD, result_data_byte);

                                if (byte_user != null)
                                {
                                    if (messageReceived != null)
                                        messageReceived(Encoding.ASCII.GetString(byte_user, 0, byte_user.Length));
                                }

                                result_data_byte = new byte[0];
                                Receive_Data_Handler();
                            }
                        }
                    }
                }
            }
            catch (IOException e)
            {
                MessageBox.Show(e.ToString());
            }
            catch (ObjectDisposedException)
            {
                Log_Raise("Abort connection");
            }
        }

        /// <summary>
        /// Receive Command Data Tag ID.
        /// </summary>
        /// <param name="command_type"></param>
        public void Receive_Data_Handler()
        {
            // Create the state object.
            StateTCPClient state = new StateTCPClient();
            state.workSocket = tcp_client;

            // Begin receiving the data from the remote device.
            if (tcp_client.Connected)
                tcp_client.BeginReceive(state.buffer, 0, StateTCPClient.BufferSize, 0,
                    new AsyncCallback(Receive_Data_Callback), state);
            else
                connect_ok = false;
        }

        #endregion
        /// <summary>
        /// Free TCP resource
        /// </summary>
        public void Free()
        {
            sendDone.Close();
            receiveDone.Close();
            connectDone.Close();
            tcp_client.Shutdown(SocketShutdown.Both);
            tcp_client.Disconnect(true);
            tcp_client.Close();
            connect_ok = false;
            //pingTimer.Stop();
            Log_Raise("Disconnected");
        }

        ~TCP_Client()
        {
            // Release the socket.

        }

    }

}
