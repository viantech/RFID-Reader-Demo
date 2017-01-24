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
//using System.Net.NetworkInformation;
//using System.Timers;
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
    }

    public delegate void SocketReceivedHandler(string msg);

    public class Eth_Client
    {
        // private Socket tcp_client;

        /// <summary>
        /// Byte Data of Frame Format
        /// </summary>
        public bool connect_ok = false;
        public event SocketReceivedHandler TagID_Msg;
        public event SocketReceivedHandler Config_Msg;

        //private Thread p_read;
        //Ping connection
        //private System.Timers.Timer pingTimer = new System.Timers.Timer() { Interval = 10000 };
        //private AutoResetEvent waiter = new AutoResetEvent(false);
        //private bool File_Mode = false;
        //private bool Muti_packets = false;
        // Event Handler
        //public bool recv_flag = false;
        //public bool alive = true;
        private static int retry_count = 3;
        // ManualResetEvent instances signal completion.
        private AutoResetEvent connectDone = new AutoResetEvent(false);
        private AutoResetEvent sendDone = new AutoResetEvent(false);
        public AutoResetEvent receiveDone = new AutoResetEvent(false);
        private ManualResetEvent send_part = new ManualResetEvent(false);
        // The response from the remote device.
        private List<byte> result_command_bytes = new List<byte>();
        private List<byte> raw_buffer = new List<byte>();
        private TcpClient client;
        private NetworkStream stream;
        //private CancellationTokenSource cts = new CancellationTokenSource();
        public Eth_Client()
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
                client = new TcpClient();
                // Create a TCP/IP socket.
                client.Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                // Connect to the remote endpoint.
                client.Client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client.Client);
                if (connectDone.WaitOne(5000))
                {
                    CM.SetKeepAliveValues(client.Client, true, 2000, 1000);
                    connect_ok = true;
                    stream = client.GetStream();
                    Receive_Command_Handler();
                    Send_ConnectionRequest();
                    /*pingTimer.Elapsed += (sender, e) =>
                        {
                            //pingsender.SendAsync(ipAddress, 120, icmp_test, options, waiter);
                            //waiter.WaitOne();
                        };*/
                    //pingTimer.Start();
                }
                else
                {
                    MessageBox.Show("Socket Connect Timeout", "Error Socket", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (SocketException)
            {
                MessageBox.Show("Socket Exception", "Error Socket", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            catch (SocketException)
            {
                //connectDone.Set();
                MessageBox.Show("Connect Exception", "Error Socket", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            try
            {
                if (!client.Client.Connected)
                {
                    // Connection is terminated, either by force or willingly
                    MessageBox.Show("Transmit Error", "Socket Terminate", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    connect_ok = false;
                }
                else
                {

                    stream.BeginWrite(b_frame, 0, b_frame.Length, new AsyncCallback(SendCallback), null);
                    sendDone.WaitOne(2000);
                    sendDone.Reset();
                }
            }
            catch (SocketException)
            {
                MessageBox.Show("Send Exception", "Error Socket", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (ObjectDisposedException)
            {
                CM.Log_Raise("Abort due to close");
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Check connection
                if (!client.Client.Connected)
                {
                    // Connection is terminated, either by force or willingly
                    MessageBox.Show("Transmiting Error", "Socket Terminate", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    connect_ok = false;
                }
                else
                {
                    // Complete sending the data to the remote device.
                    stream.EndWrite(ar);

                    // Signal that all bytes have been sent.
                    sendDone.Set();
                }
            }
            catch (SocketException)
            {
                MessageBox.Show("Sending Exception", "Error Socket", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            //Part 0 (Info)
            send_part.Reset();
            Set_Command_Send(CM.COMMAND.FIRMWARE_UPDATE_CMD, info);
            if (send_part.WaitOne(3000))
            {
                send_part.Reset();
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
                    percent = (ushort)((ip + 1) * (100 / num_part));
                    if (ip == num_part - 1)
                    {
                        if (len_last_part > 0)
                            len_part = (ushort)len_last_part;
                        percent = 100;
                    }
                    part_byte = new byte[len_part];
                    Buffer.BlockCopy(file_stream, id, part_byte, 0, len_part);
                    id += len_part;
                    CM.FrameFormat fmt_set = new CM.FrameFormat();
                    fmt_set.command = (byte)CM.COMMAND.FIRMWARE_UPDATE_CMD;
                    fmt_set.length = (ushort)((ushort)CM.LENGTH.FRAME_NON_DATA + len_part);
                    fmt_set.metal_data = part_byte;
                    /* Byte data of Frame Format*/
                    byte[] sub_fmt_byte = CM.Encode_Frame(fmt_set);
                    Send_Packets(sub_fmt_byte);
                    if (!send_part.WaitOne(4000))
                    {
                        MessageBox.Show("Time out! Failed to update firmware.", "Sending Part Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    }
                    else
                    {
                        send_part.Reset();
                        Cmd_Raise("Update FW\n" + percent.ToString() + "\n");
                    }
                }
            }
            else
                MessageBox.Show("Wrong File Info", "Sending Part 0 Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            if (CM.COMMAND.GET_CONFIGURATION_CMD == command)
            {
                receiveDone.Reset();
                //Get Gateway Configuration
                Get_Command_Send(CM.COMMAND.GET_CONFIGURATION_CMD);
                receiveDone.WaitOne(2000);
                receiveDone.Reset();
                //Antena
                Get_Command_Send(CM.COMMAND.ANTENA_CMD);
                receiveDone.WaitOne(2000);
            }
            else if (CM.COMMAND.GET_BLF_CMD == command)
            {
                receiveDone.Reset();
                Get_Command_Power(CM.COMMAND.GET_BLF_CMD, 0);
                receiveDone.WaitOne(2000);
                receiveDone.Reset();
                Get_Command_Power(CM.COMMAND.GET_BLF_CMD, 1);
                receiveDone.WaitOne(2000);
                receiveDone.Reset();
                Get_Command_Power(CM.COMMAND.GET_BLF_CMD, 2);
                receiveDone.WaitOne(2000);
            }
            else if (CM.COMMAND.GET_POWER_CMD == command)
            {
                receiveDone.Reset();
                Get_Command_Power(CM.COMMAND.GET_POWER_CMD, 0);
                receiveDone.WaitOne(2000);
                receiveDone.Reset();
                Get_Command_Power(CM.COMMAND.GET_POWER_CMD, 1);
                receiveDone.WaitOne(2000);
            }
            else if (CM.COMMAND.DIS_CONNECT_CMD == command)
            {
                receiveDone.Reset();
                Get_Command_Send(CM.COMMAND.DIS_CONNECT_CMD);
                receiveDone.WaitOne(2000);
                Free();
            }
            else if (CM.COMMAND.GET_RFID_CONFIGURATION_CMD == command)
            {
                receiveDone.Reset();
                Get_Command_Send(CM.COMMAND.GET_RFID_CONFIGURATION_CMD);
                receiveDone.WaitOne(2000);
            }
            else if (CM.COMMAND.GET_READ_POWER_PORT_CMD == command)
            {
                receiveDone.Reset();
                Get_Command_Send(CM.COMMAND.GET_READ_POWER_PORT_CMD);
                receiveDone.WaitOne(2000);
                receiveDone.Reset();
                Get_Command_Send(CM.COMMAND.GET_WRITE_POWER_PORT_CMD);
                receiveDone.WaitOne(2000);
            }
            else if (CM.COMMAND.GET_POWER_MODE_CMD == command)
            {
                receiveDone.Reset();
                Get_Command_Power(CM.COMMAND.GET_BLF_CMD, 0);
                receiveDone.WaitOne(2000);
                receiveDone.Reset();
                Get_Command_Power(CM.COMMAND.GET_BLF_CMD, 1);
                receiveDone.WaitOne(2000);
                receiveDone.Reset();
                Get_Command_Power(CM.COMMAND.GET_BLF_CMD, 2);
                receiveDone.WaitOne(2000);
                receiveDone.Reset();
                Get_Command_Power(CM.COMMAND.GET_POWER_CMD, 0);
                receiveDone.WaitOne(2000);
                receiveDone.Reset();
                Get_Command_Power(CM.COMMAND.GET_POWER_CMD, 1);
                receiveDone.WaitOne(2000);
                receiveDone.Reset();
                Get_Command_Send(CM.COMMAND.GET_POWER_MODE_CMD);
                receiveDone.WaitOne(2000);
            }
            else if (CM.COMMAND.CONNECTION_REQUEST_CMD == command)
            {
                receiveDone.Reset();
                Send_ConnectionRequest();
                receiveDone.WaitOne(2000);
            }
            else
            {

            }
            return 1;
        }

        #endregion

        #region Received

        private static int IndexOfByte(byte[] arrayToSearchThrough, byte patternToFind)
        {

            for (int i = 0; i < arrayToSearchThrough.Length - 1; i++)
            {
                bool found = true;
                //for (int j = 0; j < patternToFind.Length; j++)
                //{
                if (arrayToSearchThrough[i] != patternToFind)
                {
                    found = false;
                    break;
                }
                //}
                if (found)
                {
                    return i;
                }
            }
            return -1;
        }
        /// <summary>
        /// Handler Request Connection response
        /// </summary>
        /// <param name="byte_receive"></param>
        private void Request_Connection_Handler(byte[] byte_receive)
        {
            if (0x00 == byte_receive[0])
            {
                CM.Log_Raise("Accepted!\n");
                Start_Command_Process(CM.COMMAND.GET_CONFIGURATION_CMD);
            }
            else
            {
                if (retry_count > 0)
                {
                    MessageBox.Show("Fail request connection\n Retry!", "Request Connection Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        private void Data_Receive_Handler(byte[] command_bytes)
        {
            byte info_ack;
            string data_response = String.Empty;
            byte[] byte_bits = null;
            if (command_bytes != null && command_bytes.Length > 0)
            {
                switch ((CM.COMMAND)command_bytes[0])
                {
                    /* configuration */
                    case CM.COMMAND.GET_CONFIGURATION_CMD:
                        data_response = CM.Get_Data(CM.Decode_Frame((byte)CM.COMMAND.GET_CONFIGURATION_CMD, command_bytes));
                        if (data_response.Length > 0)
                            Cmd_Raise(data_response);
                        break;
                    case CM.COMMAND.SET_CONFIGURATION_CMD:
                        info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.SET_CONFIGURATION_CMD, command_bytes);
                        if (0x00 == info_ack)
                            CM.Log_Raise("Set Reader Config done");
                        else
                            MessageBox.Show("Configure Reader Failed", "Reader Configuration", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    /* RFID configuration */
                    case CM.COMMAND.GET_RFID_CONFIGURATION_CMD:
                        data_response = CM.Get_Data(CM.Decode_Frame((byte)CM.COMMAND.GET_RFID_CONFIGURATION_CMD, command_bytes));
                        if (data_response.Length > 0)
                            Cmd_Raise(data_response);
                        //MessageBox.Show(data_response);
                        break;
                    case CM.COMMAND.SET_RFID_CONFIGURATION_CMD:
                        info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.SET_RFID_CONFIGURATION_CMD, command_bytes);
                        if (0x00 == info_ack)
                            CM.Log_Raise("Set RFID done");
                        else
                            MessageBox.Show("Configure RFID Failed", "RFID Configuration", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    /* Port Properties */
                    case CM.COMMAND.GET_PORT_PROPERTIES_CMD:
                        data_response = CM.Get_Data(CM.Decode_Frame((byte)CM.COMMAND.GET_PORT_PROPERTIES_CMD, command_bytes));
                        if (data_response.Length > 0)
                        {
                            Cmd_Raise(data_response);
                            MessageBox.Show(data_response, "Port Property", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        break;
                    case CM.COMMAND.SET_PORT_PROPERTIES_CMD:
                        info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.SET_PORT_PROPERTIES_CMD, command_bytes);
                        if (0x00 == info_ack)
                        {
                            Cmd_Raise("Set Port\n");
                            CM.Log_Raise("Set port type done");
                        }
                        else
                            MessageBox.Show("Configure Port Failed", "Port Property", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    case CM.COMMAND.DIS_CONNECT_CMD:
                        info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.DIS_CONNECT_CMD, command_bytes);
                        if (0x00 != info_ack)
                            MessageBox.Show("Disconnect Command Failed", "Disconnect Command", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    /* start operate */
                    case CM.COMMAND.START_OPERATION_CMD:
                        info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.START_OPERATION_CMD, command_bytes);
                        if (0x00 == info_ack)
                            CM.Log_Raise("Inventory Mode");
                        else
                            MessageBox.Show("Start Operation Failed", "Start Inventory", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    /* stop operate */
                    case CM.COMMAND.STOP_OPERATION_CMD:
                        info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.STOP_OPERATION_CMD, command_bytes);
                        if (0x00 == info_ack)
                            CM.Log_Raise("Stop Inventory");
                        else
                            MessageBox.Show("Stop Operation Failed", "Stop Inventory", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    /*Tag ID */
                    case CM.COMMAND.REQUEST_TAG_ID_CMD:
                        byte_bits = CM.Decode_Frame((byte)CM.COMMAND.REQUEST_TAG_ID_CMD, command_bytes);
                        if (byte_bits != null && byte_bits.Length > 0)
                            TagID_Raise(Encoding.ASCII.GetString(byte_bits, 0, byte_bits.Length));
                        break;
                    //Power RFID
                    case CM.COMMAND.GET_POWER_CMD:
                        byte_bits = CM.Decode_Frame((byte)CM.COMMAND.GET_POWER_CMD, command_bytes);
                        if (byte_bits != null)
                        {
                            if (0x00 == byte_bits[0])
                                Cmd_Raise("Power RFID\n" + byte_bits[1].ToString() + "\n");
                            else
                                MessageBox.Show("Get Power Failed", "Global Power Information", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        break;
                    case CM.COMMAND.SET_POWER_CMD:
                        info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.SET_POWER_CMD, command_bytes);
                        if (0x00 == info_ack)
                            CM.Log_Raise("Set Power done");
                        else
                            MessageBox.Show("Setting Power Failed", "Global Power Information", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    //Region Configuration
                    case CM.COMMAND.GET_REGION_CMD:
                        byte_bits = CM.Decode_Frame((byte)CM.COMMAND.GET_REGION_CMD, command_bytes);
                        if (byte_bits != null)
                        {
                            if (0x00 == byte_bits[0])
                                Cmd_Raise("Region RFID\n" + byte_bits[1].ToString() + "\n");
                            else
                                MessageBox.Show("Get Region Failed", "Region Support", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        break;
                    case CM.COMMAND.SET_REGION_CMD:
                        info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.SET_REGION_CMD, command_bytes);
                        if (0x00 == info_ack)
                            CM.Log_Raise("Set Region done");
                        else
                            MessageBox.Show("Setting Region Failed", "Region Support", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    //Power Mode Configuration
                    case CM.COMMAND.GET_POWER_MODE_CMD:
                        byte_bits = CM.Decode_Frame((byte)CM.COMMAND.GET_POWER_MODE_CMD, command_bytes);
                        if (byte_bits != null)
                        {
                            if (0x00 == byte_bits[0])
                                Cmd_Raise("Power Mode RFID\n" + byte_bits[1].ToString() + "\n");
                            else
                                MessageBox.Show("Get Power Mode Failed", "Power Mode", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        break;
                    case CM.COMMAND.SET_POWER_MODE_CMD:
                        info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.SET_POWER_MODE_CMD, command_bytes);
                        if (0x00 == info_ack)
                            CM.Log_Raise("Set Power Mode done");
                        else
                            MessageBox.Show("Setting Power Mode Failed", "Power Mode", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    // Change Connection Type
                    case CM.COMMAND.SET_CONN_TYPE_CMD:
                        data_response = CM.Get_Data(CM.Decode_Frame((byte)CM.COMMAND.SET_CONN_TYPE_CMD, command_bytes));
                        if (data_response.Length > 0)
                            Cmd_Raise("Change Protocol\n" + data_response + "\n");
                        break;
                    case CM.COMMAND.GET_BLF_CMD:
                        byte_bits = CM.Decode_Frame((byte)CM.COMMAND.GET_BLF_CMD, command_bytes);
                        if (byte_bits != null)
                        {
                            if (0x00 == byte_bits[0])
                                Cmd_Raise("BLF Setting\n" + byte_bits[1].ToString() + "\n");
                            else
                                CM.Log_Raise("Fail Get BLF");
                        }
                        break;
                    case CM.COMMAND.SET_BLF_CMD:
                        info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.SET_BLF_CMD, command_bytes);
                        if (0x00 == info_ack)
                            CM.Log_Raise("Set BLF done");
                        else
                            CM.Log_Raise("Failed BLF");
                        break;
                    case CM.COMMAND.REBOOT_CMD:
                        info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.REBOOT_CMD, command_bytes);
                        if (0x00 == info_ack)
                            CM.Log_Raise("Rebooting ...");
                        else
                            MessageBox.Show("Error\nReboot Failed", "Reboot Command", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    case CM.COMMAND.ANTENA_CMD:
                        data_response = CM.Get_Data(CM.Decode_Frame((byte)CM.COMMAND.ANTENA_CMD, command_bytes));
                        if (data_response != null)
                            Cmd_Raise("Antena RFID\n" + data_response + "\n");
                        break;
                    case CM.COMMAND.SET_PLAN_CMD:
                        info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.SET_PLAN_CMD, command_bytes);
                        if (0x00 == info_ack)
                            CM.Log_Raise("Set Plan done");
                        else
                            MessageBox.Show("Setting Read Plan Failed", "Read Plan Configuration", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    case CM.COMMAND.GET_PLAN_CMD:
                        data_response = CM.Get_Data(CM.Decode_Frame((byte)CM.COMMAND.GET_PLAN_CMD, command_bytes));
                        if (data_response.Length > 0)
                            Cmd_Raise("Get Plan\n" + data_response + "\n");
                        break;
                    case CM.COMMAND.SETTING_SENSOR_CMD:
                        info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.SETTING_SENSOR_CMD, command_bytes);
                        if (0x00 == info_ack)
                            CM.Log_Raise("Configure Sensor done");
                        else
                            MessageBox.Show("Sensor Configuration Failed", "Sensor Setting", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    case CM.COMMAND.FIRMWARE_UPDATE_CMD:
                        info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.FIRMWARE_UPDATE_CMD, command_bytes);
                        if (0x00 == info_ack)
                            send_part.Set();
                        else
                            CM.Log_Raise("Failed Part sent.");
                        break;
                    case CM.COMMAND.GET_READ_POWER_PORT_CMD:
                        data_response = CM.Get_Data(CM.Decode_Frame((byte)CM.COMMAND.GET_READ_POWER_PORT_CMD, command_bytes));
                        if (data_response.Length > 0)
                            Cmd_Raise("Port Power\n" + data_response + "\n");
                        break;
                    case CM.COMMAND.GET_WRITE_POWER_PORT_CMD:
                        data_response = CM.Get_Data(CM.Decode_Frame((byte)CM.COMMAND.GET_WRITE_POWER_PORT_CMD, command_bytes));
                        if (data_response.Length > 0)
                            Cmd_Raise("Port Power\n" + data_response + "\n");
                        break;
                    case CM.COMMAND.SET_READ_POWER_PORT_CMD:
                        info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.SET_READ_POWER_PORT_CMD, command_bytes);
                        if (0x00 == info_ack)
                            CM.Log_Raise("Set Port Read Power done");
                        else
                            MessageBox.Show("Setting Read Power Port", "Antena Read Power", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    case CM.COMMAND.SET_WRITE_POWER_PORT_CMD:
                        info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.SET_WRITE_POWER_PORT_CMD, command_bytes);
                        if (0x00 == info_ack)
                            CM.Log_Raise("Set Port Write Power done");
                        else
                            MessageBox.Show("Setting Write Power Port", "Antena Write Power", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    default:
                        break;
                }
            }
        }

        public void Cmd_Raise(string cmd_str)
        {
            SocketReceivedHandler cmd_msg = Config_Msg;
            if (cmd_msg != null)
                cmd_msg(cmd_str);
        }
        public void TagID_Raise(string ID_str)
        {
            SocketReceivedHandler messageReceived = TagID_Msg;
            //if (messageReceived != null)
            messageReceived(ID_str);
        }
        /// <summary>
        /// Receive Command Data Response.
        /// </summary>
        /// <param name="command_type"></param>

        public bool IsConnected
        {
            get
            {
                return this.client != null && connect_ok;
            }
        }

        private void Receive_Command_Handler()
        {
            Task task_read = Task.Run(() =>
                {
                    int byteRead;
                    int length = 0;

                    byte[] buffer = new byte[1024];

                    while (this.IsConnected)
                    {
                        try
                        {
                            if (this.stream.Read(buffer, 0, 1) > 0)
                            {
                                if (buffer[0] == (byte)CM.HEADER.RESP_PACKET_HDR)
                                {
                                    if (this.stream.Read(buffer, 1, 2) > 0)
                                    {
                                        length = (buffer[1] << 8) + buffer[2];
                                        if (length > 1023)
                                            CM.Log_Raise("Wrong length");
                                        else
                                        {
                                            int desired_len = length - 2;
                                            int idx = 3;
                                            while ((byteRead = this.stream.Read(buffer, idx, desired_len)) > 0)
                                            {

                                                desired_len = desired_len - byteRead;
                                                idx += byteRead;
                                                if (desired_len == 0)
                                                    break;
                                            }
                                            Command_Sync(buffer, length + 1);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show("Server closed connection", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                Free();
                                break;
                            }
                        }
                        catch (IOException ex)
                        {
                            if (connect_ok)
                            {
                                if (ex.InnerException.Message.Contains("A connection attempt"))
                                {
                                    Cmd_Raise("Keep Alive Timeout\n");
                                }
                                else
                                {
                                    Free();
                                    MessageBox.Show("Connection Close", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                            else
                            {
                                if (ex.InnerException.Message.Contains("blocking operation"))
                                    length = -1;
                            }
                        }
                        catch (ObjectDisposedException)
                        {
                            if (connect_ok)
                                CM.Log_Raise("Abort due to close");
                        }
                        catch (ThreadAbortException)
                        {
                            MessageBox.Show("Thread Aborted Exception", "Thread Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                );

        }

        private void Command_Sync(byte[] SubFrame, int bytesRead)
        {
            try
            {
                if (0x01 != (SubFrame[bytesRead - 3] << 8) + SubFrame[bytesRead - 2])
                {
                    byte[] part_meta_sub = CM.Decode_SubFrame(SubFrame, bytesRead);
                    if (part_meta_sub != null)
                        result_command_bytes.AddRange(part_meta_sub);
                    else
                    {
                        result_command_bytes.Clear();
                        CM.Log_Raise("Wrong full frame");
                    }
                }
                else
                {
                    byte[] last_meta_sub = CM.Decode_SubFrame(SubFrame, bytesRead);
                    if (last_meta_sub != null)
                    {
                        receiveDone.Set();
                        if (result_command_bytes.Count > 0)
                        {
                            result_command_bytes.AddRange(last_meta_sub);
                            if ((byte)CM.COMMAND.REQUEST_TAG_ID_CMD == result_command_bytes[0])
                            {
                                byte[] byte_bits = CM.Decode_Frame((byte)CM.COMMAND.REQUEST_TAG_ID_CMD, result_command_bytes.ToArray());
                                result_command_bytes.Clear();
                                //if (byte_bits != null && byte_bits.Length > 0)
                                TagID_Raise(Encoding.ASCII.GetString(byte_bits, 0, byte_bits.Length));
                            }
                            else if ((byte)CM.COMMAND.GET_RFID_CONFIGURATION_CMD == result_command_bytes[0])
                            {
                                string data_response = CM.Get_Data(CM.Decode_Frame((byte)CM.COMMAND.GET_RFID_CONFIGURATION_CMD, result_command_bytes.ToArray()));
                                result_command_bytes.Clear();
                                Cmd_Raise(data_response);
                            }
                            else
                            {
                                result_command_bytes.Clear();
                                CM.Log_Raise("Warning catch header" + last_meta_sub[0].ToString());
                            }

                        }
                        else
                        {
                            if ((byte)CM.COMMAND.CONNECTION_REQUEST_CMD == last_meta_sub[0])
                                Request_Connection_Handler(CM.Decode_Frame((byte)CM.COMMAND.CONNECTION_REQUEST_CMD, last_meta_sub));
                            else
                                Data_Receive_Handler(last_meta_sub);
                        }

                    }
                    else
                    {
                        CM.Log_Raise("Wrong Last Frame");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error Command Handler", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                this.stream.Close();
                this.client.Close();
                CM.Log_Raise("Disconnected");
            }
            catch (SocketException se)
            {
                MessageBox.Show(se.ToString(), "Error Socket", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (ObjectDisposedException e)
            {
                MessageBox.Show(e.ToString(), "Error Disposed Object", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }

}
