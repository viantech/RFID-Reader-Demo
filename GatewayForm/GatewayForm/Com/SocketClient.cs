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
using CM = GatewayForm.Common;

namespace GatewayForm
{
    

    public class ZigbeeClient
    {
        // Size of receive buffer.
        public const int ZigbeeSize = 2048;
        // Receive buffer.
        public byte[] buffer = new byte[ZigbeeSize];
        public byte[] result_byte_frame = new byte[0];
        public byte[] raw_read_byte = new byte[0];

    }

    public class SocketClient
    {
        private static byte[] zigbee_header_byte = Encoding.ASCII.GetBytes(Properties.Resources.ZIGBEE_SEND 
                                                                  + Properties.Resources.ADDRESS_NODE + "=");
        private static byte[] zigbee_binary = Encoding.ASCII.GetBytes(Properties.Resources.ZIGBEE_BINARY);
        private static byte[] addr_node = Encoding.ASCII.GetBytes(Properties.Resources.ADDRESS_NODE);
        private static byte[] ucast_header = Encoding.ASCII.GetBytes(Properties.Resources.UCAST_HEADER);
        
        /*private static byte[] seq_header = Encoding.ASCII.GetBytes("SEQ");
        private static byte[] ack_header = Encoding.ASCII.GetBytes("ACK");
        private static byte[] nack_header = Encoding.ASCII.GetBytes("NACK");
        private static byte[] error_header = Encoding.ASCII.GetBytes("ERROR");*/
        //private byte[] result_byte_frame = new byte[0];
        //private string respose;
        private static int retry_count = 3;
        public bool connect_ok = false;
        //private volatile bool start_enable = false;
        private TcpClient client;
        private NetworkStream stream;
        //private Socket client_socket;

        private AutoResetEvent connectDone = new AutoResetEvent(false);
        private AutoResetEvent sendDone = new AutoResetEvent(false);
        private AutoResetEvent receive_ACKDone = new AutoResetEvent(false);
        public AutoResetEvent receiveDone = new AutoResetEvent(false);
        public event SocketReceivedHandler TagID_Msg;
        public event SocketReceivedHandler Config_Msg;
        public SocketClient()
        {
        }

        public void Connect(string ip, int port)
        {
            try
            {
                IPEndPoint remoteEP;
                IPAddress ipAddress = Dns.GetHostAddresses(ip)[0];
                remoteEP = new IPEndPoint(ipAddress, port);
                client = new TcpClient();
                client.Client.BeginConnect(remoteEP, OnConnect, client.Client);
                if (connectDone.WaitOne(3000))
                {
                    stream = client.GetStream();
                    connect_ok = true;
                    //client_socket = client.Client;
                    Receive_Handler();
                    Send_ConnectionRequest();
                }
                else
                    MessageBox.Show("Socket Connect Timeout", "Error Socket", MessageBoxButtons.OK);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        private void OnConnect(IAsyncResult ar)
        {
            try
            {
                using (TcpClient tcpClient = (TcpClient)ar.AsyncState)
                {
                    tcpClient.EndConnect(ar);
                    // Signal that the connection has been made.
                    connectDone.Set();
                }
            }
            catch (Exception e)
            {
                if (connect_ok)
                    MessageBox.Show(e.ToString());
            }
        }

        #region Send

        public void Send_Binary(byte[] sub_fmt_byte)
        {
            if (client.Client.Connected)
            {
                byte[] len_byte = Encoding.ASCII.GetBytes(sub_fmt_byte.Length.ToString("X2"));
                byte[] concat = new byte[zigbee_binary.Length + len_byte.Length + addr_node.Length + 2];
                Buffer.BlockCopy(zigbee_binary, 0, concat, 0, zigbee_binary.Length);
                Buffer.BlockCopy(len_byte, 0, concat, zigbee_binary.Length, len_byte.Length);
                concat[zigbee_binary.Length + len_byte.Length] = 0x2C;
                Buffer.BlockCopy(addr_node, 0, concat, zigbee_binary.Length + len_byte.Length + 1, addr_node.Length);
                concat[zigbee_binary.Length + len_byte.Length + addr_node.Length + 1] = 0x0D;
                stream.BeginWrite(concat, 0, concat.Length, OnWriteComplete, null);

                stream.BeginWrite(sub_fmt_byte, 0, sub_fmt_byte.Length, OnWriteComplete, null);
            }
            else
            {
                MessageBox.Show("Send Stream close");
                connect_ok = false;
            }
        }

        public void SendAsync(byte[] bytesSend)
        {
            byte[] zigbee_frame = Encode_Zigbee(bytesSend);
            stream.BeginWrite(zigbee_frame, 0, zigbee_frame.Length, OnWriteComplete, null);
            //Console.WriteLine("Len:{0}-{1}", zigbee_frame.Length, ByteArrayToHexString(zigbee_frame));
            sendDone.WaitOne();
            sendDone.Reset();
        }

        private void OnWriteComplete(IAsyncResult ar)
        {
            try
            {
                // Complete sending the data to the remote device.
                stream.EndWrite(ar);
                //CM.Log_Raise(String.Format("Sent {0} bytes to server.", bytesSent));

                // Signal that all bytes have been sent.
                sendDone.Set();
            }
            catch (Exception e)
            {
                if (connect_ok)
                    MessageBox.Show(e.ToString());
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
            Send_Binary(byte_req);
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
            Send_Binary(byte_get_cmd);
        }

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
            Send_Binary(byte_get_cmd);
        }

        public void Set_Command_Send(CM.COMMAND command, String user_data)
        {
            CM.FrameFormat fmt_set = new CM.FrameFormat();
            fmt_set.command = (byte)command;
            byte[] user_byte = Encoding.ASCII.GetBytes(user_data);
            fmt_set.metal_data = user_byte;
            fmt_set.length = (ushort)((ushort)CM.LENGTH.FRAME_NON_DATA + user_byte.Length);

            /* Byte data of Frame Format*/
            byte[] sub_fmt_byte = CM.Encode_Frame(fmt_set);
            Send_Bytes(sub_fmt_byte);
        }

        public void Set_Command_Send_Bytes(CM.COMMAND command, byte[] user_bytes)
        {
            CM.FrameFormat fmt_set = new CM.FrameFormat();
            fmt_set.command = (byte)command;
            fmt_set.length = (ushort)((ushort)CM.LENGTH.FRAME_NON_DATA + user_bytes.Length);
            fmt_set.metal_data = user_bytes;

            /* Byte data of Frame Format*/
            byte[] sub_fmt_byte = CM.Encode_Frame(fmt_set);
            Send_Bytes(sub_fmt_byte);
        }

        private void Send_Bytes(byte[] frame_data_byte)
        {
            UInt16 num_packet, last_packet_byte, len_transmit;
            UInt16 idx, len_byte_fmt;
            idx = 0;
            len_byte_fmt = (ushort)frame_data_byte.Length;
            num_packet = (ushort)(len_byte_fmt / ((ushort)CM.LENGTH.MAX_SIZE_ZIGBEE_META_DATA));
            last_packet_byte = (ushort)(len_byte_fmt % ((ushort)CM.LENGTH.MAX_SIZE_ZIGBEE_META_DATA));
            if (last_packet_byte > 0)
                num_packet += 1;
            len_transmit = (ushort)CM.LENGTH.MAX_SIZE_ZIGBEE_META_DATA; //72 - 6
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
                Send_Binary(byte_tcp);
                if (i < num_packet -1)
                {
                    receive_ACKDone.WaitOne(2000);
                }
            }
        }

        /// <summary>
        /// Encode byte of frame data with Zigbee Header
        /// </summary>
        /// <param name="b_frame"></param> byte array of sending frame

        private static byte[] Encode_Zigbee(byte[] sub_fmt_byte)
        {
            byte[] concat = new byte[zigbee_header_byte.Length + sub_fmt_byte.Length + 1];
            Buffer.BlockCopy(zigbee_header_byte, 0, concat, 0, zigbee_header_byte.Length);
            Buffer.BlockCopy(sub_fmt_byte, 0, concat, zigbee_header_byte.Length, sub_fmt_byte.Length);
            concat[zigbee_header_byte.Length + sub_fmt_byte.Length] = 0x0D;
            return concat;
        }

        public async void Start_Process(CM.COMMAND cmd)
        {
            int task = await Task.Run(() => Command_Process(cmd));
        }

        private int Command_Process(CM.COMMAND command)
        {
            switch (command)
            {
                case CM.COMMAND.CONNECTION_REQUEST_CMD:
                    receiveDone.WaitOne(2000);
                    Send_ConnectionRequest();
                    receiveDone.WaitOne(2000);
                    break;
                case CM.COMMAND.GET_CONFIGURATION_CMD:
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

        #region Request Connection Command

        private void Request_Connection_Handler(byte[] byte_receive)
        {
            if (0x00 == byte_receive[0])
            {
                CM.Log_Raise("Accepted!");
                Start_Process(CM.COMMAND.GET_CONFIGURATION_CMD);
            }
            else
            {
                if (retry_count > 0)
                {
                    MessageBox.Show("Fail request connection\n Retry!");
                    retry_count--;
                    Start_Process(CM.COMMAND.CONNECTION_REQUEST_CMD);
                }
                else
                {
                    CM.Log_Raise("Retry Failed. Closed Socket.");
                    connect_ok = false;
                    retry_count = 3;
                    Free();
                    return;
                }
            }
        }

        #endregion

        private static string ByteArrayToHexString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2} ", b);
            return hex.ToString();
        }

        private static bool Match_Bytes(byte[] haystack, byte[] needle, int start)
        {
            if (needle.Length > haystack.Length)
                return false;
            else
            {
                for (int i = 0; i < needle.Length; i++)
                {
                    if (needle[i] != haystack[i + start])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static int IndexOfByte(byte[] arrayToSearchThrough, byte[] patternToFind)
        {
            if (patternToFind.Length > arrayToSearchThrough.Length)
                return -1;
            for (int i = 0; i < arrayToSearchThrough.Length - patternToFind.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < patternToFind.Length; j++)
                {
                    if (arrayToSearchThrough[i + j] != patternToFind[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Receive Command Data Response.
        /// </summary>
        /// <param name="command_type"></param>

        #region Read Async

        private void Receive_Handler()
        {
            if (client.Client.Connected)
            {
                ZigbeeClient state = new ZigbeeClient();

                stream.BeginRead(state.buffer, 0, ZigbeeClient.ZigbeeSize, OnRead, state);
            }
            else
            {
                MessageBox.Show("Stream Receive close ");
                connect_ok = false;
            }
        }

        private void OnRead(IAsyncResult ar)
        {
            int bytesRead;
            ZigbeeClient state = (ZigbeeClient)ar.AsyncState;
            try
            {
                bytesRead = stream.EndRead(ar);
                if (bytesRead > 0)
                {
                    if (state.buffer[bytesRead - 3] == 0x4B && state.buffer[bytesRead - 2] == 0x0d && state.buffer[bytesRead - 1] == 0x0a)
                    {
                        Receive_Handler();
                        receive_ACKDone.Set();
                        return;
                    }
                    Array.Resize(ref state.raw_read_byte, state.raw_read_byte.Length + bytesRead);
                    Buffer.BlockCopy(state.buffer, 0, state.raw_read_byte, state.raw_read_byte.Length - bytesRead, bytesRead);
                    if (bytesRead > 4)
                    {
                        if (state.buffer[bytesRead - 4] == 0x01)
                        {
                            if (state.buffer[bytesRead - 2] == 0x0d && state.buffer[bytesRead - 1] == 0x0a)
                            {
                                int found_ucast = IndexOfByte(state.raw_read_byte, ucast_header) - 2;
                                state.raw_read_byte = state.raw_read_byte.Skip(found_ucast).ToArray();
                                UInt16 num_ucast, last_ucast, len_block;
                                UInt16 idx, len_stream;
                                idx = 0;
                                len_stream = (ushort)state.raw_read_byte.Length;
                                num_ucast = (ushort)(len_stream / (ushort)CM.LENGTH.MAX_SIZE_ZIGBEE_BLOCK);
                                last_ucast = (ushort)(len_stream % (ushort)CM.LENGTH.MAX_SIZE_ZIGBEE_BLOCK);
                                if (last_ucast > 0)
                                    num_ucast += 1;
                                len_block = (ushort)CM.LENGTH.MAX_SIZE_ZIGBEE_BLOCK;
                                for (int i = 0; i < num_ucast; i++)
                                {
                                    if (i == num_ucast - 1)
                                    {
                                        if (last_ucast > 0)
                                            len_block = last_ucast;
                                    }
                                    byte[] subframe_block = new byte[len_block - 30];
                                    Buffer.BlockCopy(state.raw_read_byte, idx + 28, subframe_block, 0, subframe_block.Length);
                                    idx += len_block;
                                    byte[] meta_sub = CM.Decode_SubFrame(subframe_block, subframe_block.Length);
                                    if (meta_sub != null)
                                    {
                                        Array.Resize(ref state.result_byte_frame, state.result_byte_frame.Length + meta_sub.Length);
                                        Buffer.BlockCopy(meta_sub, 0, state.result_byte_frame, state.result_byte_frame.Length - meta_sub.Length, meta_sub.Length);
                                    }
                                    else
                                    {
                                        MessageBox.Show("Wrong Sub Frame Packet");
                                        Receive_Handler();
                                    }

                                }
                                if (state.result_byte_frame != null)
                                {
                                    receiveDone.Set();
                                    if ((byte)CM.COMMAND.CONNECTION_REQUEST_CMD == state.result_byte_frame[0])
                                        Request_Connection_Handler(CM.Decode_Frame((byte)CM.COMMAND.CONNECTION_REQUEST_CMD, state.result_byte_frame));
                                    else Data_Receive_Handler(state.result_byte_frame);

                                    Receive_Handler();
                                }
                                else
                                {
                                    Receive_Handler();
                                }
                            }
                            else stream.BeginRead(state.buffer, 0, ZigbeeClient.ZigbeeSize, OnRead, state); //make sure the last packet
                        }
                        else stream.BeginRead(state.buffer, 0, ZigbeeClient.ZigbeeSize, OnRead, state); // not the last packet
                    }
                    else stream.BeginRead(state.buffer, 0, ZigbeeClient.ZigbeeSize, OnRead, state); // read < 4bytes
                }
                else
                {
                    MessageBox.Show("Zigbee is down");
                    Free();
                }

            }
            catch (SocketException se)
            {
                if (connect_ok)
                    MessageBox.Show(se.ToString()); 
            }
            catch (ObjectDisposedException)
            {
                if (connect_ok)
                    CM.Log_Raise("Abort connection");
            }
        }

        private void Data_Receive_Handler(byte[] command_bytes)
        {
            byte info_ack;
            string data_response = null;
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
                            MessageBox.Show("Failed Reader Set Config");
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
                            MessageBox.Show("Failed Set RFID Configuration");
                        break;
                    /* Port Properties */
                    case CM.COMMAND.GET_PORT_PROPERTIES_CMD:
                        data_response = CM.Get_Data(CM.Decode_Frame((byte)CM.COMMAND.GET_PORT_PROPERTIES_CMD, command_bytes));
                        if (data_response.Length > 0)
                        {
                            Cmd_Raise(data_response);
                            MessageBox.Show(data_response);
                        }
                        break;
                    case CM.COMMAND.SET_PORT_PROPERTIES_CMD:
                        info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.SET_PORT_PROPERTIES_CMD, command_bytes);
                        if (0x00 == info_ack)
                            CM.Log_Raise("Set connection done");
                        else
                            MessageBox.Show("Failed Set Connection");
                        break;
                    case CM.COMMAND.DIS_CONNECT_CMD:
                        info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.DIS_CONNECT_CMD, command_bytes);
                        if (0x00 != info_ack)
                            MessageBox.Show("Failed Disconnect");
                        break;
                    /* start operate */
                    case CM.COMMAND.START_OPERATION_CMD:
                        info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.START_OPERATION_CMD, command_bytes);
                        if (0x00 == info_ack)
                        {
                            CM.Log_Raise("Inventory Mode");
                        }
                        else
                            MessageBox.Show("Failed Start Operation");
                        break;
                    /* stop operate */
                    case CM.COMMAND.STOP_OPERATION_CMD:
                        info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.STOP_OPERATION_CMD, command_bytes);
                        if (0x00 == info_ack)
                        {
                            CM.Log_Raise("Stop Inventory");
                        }
                        else
                            MessageBox.Show("Failed Stop Operation!");
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
                                MessageBox.Show("Fail Get Power");
                        }
                        break;
                    case CM.COMMAND.SET_POWER_CMD:
                        info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.SET_POWER_CMD, command_bytes);
                        if (0x00 == info_ack)
                            CM.Log_Raise("Set Power done");
                        else
                            MessageBox.Show("Failed Set Power");
                        break;
                    //Region Configuration
                    case CM.COMMAND.GET_REGION_CMD:
                        byte_bits = CM.Decode_Frame((byte)CM.COMMAND.GET_REGION_CMD, command_bytes);
                        if (byte_bits != null)
                        {
                            if (0x00 == byte_bits[0])
                                Cmd_Raise("Region RFID\n" + byte_bits[1].ToString() + "\n");
                            else
                                MessageBox.Show("Fail Get Region");
                        }
                        break;
                    case CM.COMMAND.SET_REGION_CMD:
                        info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.SET_REGION_CMD, command_bytes);
                        if (0x00 == info_ack)
                            CM.Log_Raise("Set Region done");
                        else
                            MessageBox.Show("Failed Set Region");
                        break;
                    //Power Mode Configuration
                    case CM.COMMAND.GET_POWER_MODE_CMD:
                        byte_bits = CM.Decode_Frame((byte)CM.COMMAND.GET_POWER_MODE_CMD, command_bytes);
                        if (byte_bits != null)
                        {
                            if (0x00 == byte_bits[0])
                                Cmd_Raise("Power Mode RFID\n" + byte_bits[1].ToString() + "\n");
                            else
                                MessageBox.Show("Fail Get Power Mode");
                        }
                        break;
                    case CM.COMMAND.SET_POWER_MODE_CMD:
                        info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.SET_POWER_MODE_CMD, command_bytes);
                        if (0x00 == info_ack)
                            CM.Log_Raise("Set Power Mode done");
                        else
                            MessageBox.Show("Failed Set Power Mode");
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
                            MessageBox.Show("Failed Reboot");
                        break;
                    case CM.COMMAND.ANTENA_CMD:
                        data_response = CM.Get_Data(CM.Decode_Frame((byte)CM.COMMAND.ANTENA_CMD, command_bytes));
                        if (data_response.Length > 0)
                            Cmd_Raise("Antena RFID\n" + data_response + "\n");
                        break;
                    case CM.COMMAND.SET_PLAN_CMD:
                        info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.SET_PLAN_CMD, command_bytes);
                        if (0x00 == info_ack)
                            CM.Log_Raise("Set Plan done");
                        else
                            MessageBox.Show("Failed Set Plan");
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
                            MessageBox.Show("Failed Congfigure Sensor");
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
            if (messageReceived != null)
                messageReceived(ID_str);
        }

        #endregion

        #region Free resource
        public void Free()
        {
            try
            {
                connect_ok = false;
                stream.Close();
                //client_socket.Close();
                client.Close();
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
        /*/// <summary>
        /// Free TCP resource
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
                    //tcp_client.Shutdown(SocketShutdown.Both);//try catch here
                    tcp_client.Close();
                    stream.Dispose();
                }
            }
            IsDisposed = true;
        }

        ~SocketClient()
        {
            // Release the socket.
            Dispose(false);
        }*/
        ~SocketClient()
        {
            
        }
        #endregion

    }
}