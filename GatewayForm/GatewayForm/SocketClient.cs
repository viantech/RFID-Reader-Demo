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
    public delegate void SocketReceivedHandler(string msg);

    public class ZigbeeClient
    {
        // Size of receive buffer.
        public const int ZigbeeSize = 2048;
        // Receive buffer.
        public byte[] buffer = new byte[ZigbeeSize];
        
    }

    public class SocketClient
    {
        private static byte[] zigbee_header_byte = Encoding.ASCII.GetBytes("AT+UCAST:000D6F000C45D3B9=");
        private static byte[] seq_header = Encoding.ASCII.GetBytes("SEQ");
        private static byte[] ack_header = Encoding.ASCII.GetBytes("ACK");
        private static byte[] nack_header = Encoding.ASCII.GetBytes("NACK");
        private static byte[] ucast_header = Encoding.ASCII.GetBytes("UCAST");
        private static byte[] error_header = Encoding.ASCII.GetBytes("ERROR");
        private byte[] result_byte_frame = new byte[0];
        private byte[] raw_read_byte = new byte[0];
        private volatile bool start_enable = false;
        private string respose;
        private static int retry_count = 3;

        private TcpClient client;
        private NetworkStream stream;
        private Socket client_socket;

        private AsyncCallback callbackRead;
        private AsyncCallback callbackWrite;

        public event SocketReceivedHandler TagID_Received;
        public event SocketReceivedHandler Get_Configuration;
        public event SocketReceivedHandler Log_Msg;

        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);
        public SocketClient()
        {
        }

        public void Connect(string ip, int port)
        {
            client = new TcpClient(ip, port);
            stream = client.GetStream();
            client_socket = client.Client;
            client_socket.ReceiveTimeout = 2000;
            callbackRead = new AsyncCallback(OnReadComplete);
            callbackWrite = new AsyncCallback(OnWriteComplete);
            Send_ConnectionRequest();
            //Receive_Command_Handler(CM.COMMAND.CONNECTION_REQUEST_CMD);
            ReadAsync(CM.COMMAND.CONNECTION_REQUEST_CMD);
        }

        #region Send
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
            SendAsync(byte_req);
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
            SendAsync(byte_get_cmd);
        }

        public void Set_Command_Send(CM.COMMAND command, String user_data)
        {
            CM.FrameFormat fmt_set = new CM.FrameFormat();
            fmt_set.command = (byte)command;
            fmt_set.length = (ushort)CM.LENGTH.FRAME_NON_DATA;
            byte[] user_byte = Encoding.ASCII.GetBytes(user_data);
            fmt_set.length += (ushort)user_byte.Length;
            fmt_set.metal_data = user_byte;

            /* Byte data of Frame Format*/
            byte[] sub_fmt_byte = CM.Encode_Frame(fmt_set);
            SendAsync(sub_fmt_byte);
        }

        private void Send_Packets(byte[] frame_data_byte)
        {
            UInt16 num_packet, last_packet_byte, len_transmit;
            UInt16 idx, len_byte_fmt;
            idx = 0;
            len_byte_fmt = (ushort)frame_data_byte.Length;
            num_packet = (ushort)(len_byte_fmt / ((ushort)CM.LENGTH.MAX_SIZE_ZIGBEE_META_DATA));
            last_packet_byte = (ushort)(len_byte_fmt % ((ushort)CM.LENGTH.MAX_SIZE_ZIGBEE_META_DATA));
            if (last_packet_byte > 0)
                num_packet += 1;
            len_transmit = (ushort)((ushort)CM.LENGTH.MAX_SIZE_ZIGBEE_META_DATA + (ushort)CM.LENGTH.SIZE_SUB_FRAME_APPEND); //72
            for (int i = 0; i < num_packet; i++)
            {
                if (i == (num_packet - 1))
                {
                    if (last_packet_byte > 0)
                        len_transmit = (ushort)(last_packet_byte + (ushort)CM.LENGTH.SIZE_SUB_FRAME_APPEND);
                }
                CM.SubFrameFormat sub_fmt_send = new CM.SubFrameFormat();
                sub_fmt_send.header = (byte)CM.HEADER.PACKET_HDR;
                sub_fmt_send.length = len_transmit;
                Buffer.BlockCopy(frame_data_byte, idx, sub_fmt_send.metal_data, 0, (int)len_transmit);
                sub_fmt_send.truncate = (ushort)(num_packet - i);
                byte[] byte_tcp = CM.Encode_SubFrame(sub_fmt_send);
                SendAsync(byte_tcp);
                idx += len_transmit;
            }
        }

        /// <summary>
        /// Send byte raw of frame data with Zigbee Header
        /// </summary>
        /// <param name="b_frame"></param> byte array of sending frame
        private void SendFrame(byte[] b_frame)
        {
            byte[] zigbee_frame = Encode_Zigbee(b_frame);
            // Begin sending the packet to the remote device.
            //stream.Write(zigbee_frame, 0, zigbee_frame.Length);
            //stream.Flush();
        }

        private static byte[] Encode_Zigbee(byte[] sub_fmt_byte)
        {
            byte[] concat = new byte[zigbee_header_byte.Length + sub_fmt_byte.Length + 1];
            Buffer.BlockCopy(zigbee_header_byte, 0, concat, 0, zigbee_header_byte.Length);
            Buffer.BlockCopy(sub_fmt_byte, 0, concat, zigbee_header_byte.Length, sub_fmt_byte.Length);
            concat[zigbee_header_byte.Length + sub_fmt_byte.Length] = 0x0D;
            return concat;
        }
        #endregion

        #region Receive

        #region Request Connection Command

        private void Request_Connection_Handler(byte[] byte_receive)
        {
            //ACK and Status
            //if (0x00 == byte_resp[1])
            //form1.SetLog("Join Ready State.");

            if (0x00 == byte_receive[0])
            {
                MessageBox.Show("ACK.Accepted!");
            }
            //form1.SetLog("Receive ACK.\nCommand Accpected!");
            else
            {
                //form1.SetLog("Receive NAK.\nRetry!!!");
                if (retry_count > 1)
                {
                    retry_count--;
                    Send_ConnectionRequest();
                    ReadAsync(CM.COMMAND.CONNECTION_REQUEST_CMD);
                }
                else
                {
                    //form1.SetLog("Retry Failed. Closed Socket.");
                    retry_count = 3;
                    client.Close();

                    // Chua retry button duoc. form1.ConnectSocket_btn.Text = "Connect";
                    return;
                }
            }
        }

        #endregion

        #region Read Handler

        private static string ByteArrayToHexString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2} ", b);
            return hex.ToString();
        }

        static bool Match_Bytes(byte[] haystack, byte[] needle, int start)
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
        
        /// <summary>
        /// After received all packets. Decode the byte array into metal data.
        /// </summary>
        /// <param name="command_type"></param>
        private void Data_Handler(CM.COMMAND command_type)
        {
            byte info_ack;
            string data_response = null;
            SocketReceivedHandler command_rev;
            //______________________________________________//
            switch (command_type)
            {
                /* connection request */
                case CM.COMMAND.CONNECTION_REQUEST_CMD:
                    Request_Connection_Handler(CM.Decode_Frame((byte)CM.COMMAND.CONNECTION_REQUEST_CMD, result_byte_frame));
                    break;
                /* configuration */
                case CM.COMMAND.GET_CONFIGURATION_CMD:
                    data_response = CM.Get_Data(CM.Decode_Frame((byte)CM.COMMAND.GET_CONFIGURATION_CMD, result_byte_frame));
                    command_rev = Get_Configuration;
                    if (command_rev != null)
                        command_rev(data_response);
                    break;
                case CM.COMMAND.SET_CONFIGURATION_CMD:
                    info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.SET_CONFIGURATION_CMD, result_byte_frame);
                    if (0x00 == info_ack)
                        MessageBox.Show("Set Config success");
                    else
                        MessageBox.Show("Failed set Config");
                    break;
                /* RFID configuration */
                case CM.COMMAND.GET_RFID_CONFIGURATION_CMD:
                    data_response = CM.Get_Data(CM.Decode_Frame((byte)CM.COMMAND.GET_RFID_CONFIGURATION_CMD, result_byte_frame));
                    command_rev = Get_Configuration;
                    if (command_rev != null)
                        command_rev(data_response);
                    MessageBox.Show(data_response);
                    break;
                case CM.COMMAND.SET_RFID_CONFIGURATION_CMD:
                    info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.SET_RFID_CONFIGURATION_CMD, result_byte_frame);
                    if (0x00 == info_ack)
                        MessageBox.Show("Set RFID successfull");
                    else
                        MessageBox.Show("Failed set RFID");
                    break;
                /* Port Properties */
                case CM.COMMAND.GET_PORT_PROPERTIES_CMD:
                    data_response = CM.Get_Data(CM.Decode_Frame((byte)CM.COMMAND.GET_PORT_PROPERTIES_CMD, result_byte_frame));
                    command_rev = Get_Configuration;
                    if (command_rev != null)
                        command_rev(data_response);
                    MessageBox.Show(data_response);
                    break;
                case CM.COMMAND.SET_PORT_PROPERTIES_CMD:
                    info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.SET_PORT_PROPERTIES_CMD, result_byte_frame);
                    if (0x00 == info_ack)
                        MessageBox.Show("Set connection successfull");
                    else
                        MessageBox.Show("Failed set connection");
                    break;
                /* start operate */
                case CM.COMMAND.START_OPERATION_CMD:
                    info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.START_OPERATION_CMD, result_byte_frame);
                    if (0x00 == info_ack)
                    {
                        MessageBox.Show("Starting read Tag ID");
                        start_enable = true;
                        Receive_TagID_Handler();
                    }
                    else
                        MessageBox.Show("Failed start operation!");
                    break;
                /* stop operate */
                case CM.COMMAND.STOP_OPERATION_CMD:
                    info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.STOP_OPERATION_CMD, result_byte_frame);
                    if (0x00 == info_ack)
                    {
                        MessageBox.Show("Stop operate");
                        start_enable = false;
                    }
                    else
                        MessageBox.Show("Failed stop operation!");
                    break;
                default:
                    break;
            }
        }

        private void Receive_TagID_Handler()
        {
            ZigbeeClient state = new ZigbeeClient();

            stream.BeginRead(state.buffer, 0, ZigbeeClient.ZigbeeSize, OnReadIDComplete, state);
        }
        
        private void OnReadIDComplete(IAsyncResult ar)
        {
            int bytesRead;
            ZigbeeClient state = (ZigbeeClient)ar.AsyncState;
            try
            {
                bytesRead = stream.EndRead(ar);
                if (bytesRead > 0)
                {
                    Array.Resize(ref raw_read_byte, raw_read_byte.Length + bytesRead);
                    Buffer.BlockCopy(state.buffer, 0, raw_read_byte, raw_read_byte.Length - bytesRead, bytesRead);
                    
                    if (bytesRead > 4)
                    {
                        if (state.buffer[bytesRead - 4] == 0x01)
                        {
                            if (state.buffer[bytesRead - 2] == 0x0d && state.buffer[bytesRead - 1] == 0x0a)
                            {
                                //byte[] data = raw_read_byte.ToArray();
                                UInt16 num_ucast, last_ucast, len_block;
                                UInt16 idx, len_stream;
                                idx = 0;
                                len_stream = (ushort)raw_read_byte.Length;
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
                                    Buffer.BlockCopy(raw_read_byte, idx + 28, subframe_block, 0, subframe_block.Length);
                                    idx += len_block;
                                    byte[] meta_sub = CM.Decode_SubFrame(subframe_block, subframe_block.Length);
                                    Array.Resize(ref result_byte_frame, result_byte_frame.Length + meta_sub.Length);
                                    Buffer.BlockCopy(meta_sub, 0, result_byte_frame, result_byte_frame.Length - meta_sub.Length, meta_sub.Length);
                                }
                                /* TAG ID */
                                var messageReceived = TagID_Received;
                                byte[] byte_user = CM.Decode_Frame((byte)CM.COMMAND.REQUEST_TAG_ID_CMD, result_byte_frame);
                                respose = Encoding.ASCII.GetString(byte_user, 0, byte_user.Length);
                                if (!String.IsNullOrEmpty(respose))
                                    messageReceived(respose);
                                raw_read_byte = new byte[0];
                                result_byte_frame = new byte[0];
                                Receive_TagID_Handler();
                            }
                        }
                    }
                }
                stream.BeginRead(state.buffer, 0, ZigbeeClient.ZigbeeSize, OnReadIDComplete, state);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void Log_Raise(string log_str)
        {
            var logmsg = Log_Msg;
            if (logmsg != null)
                logmsg(log_str);
        }
        
        #endregion

        #region Read Async
        public void ReadAsync(CM.COMMAND type_command)
        {
            ZigbeeClient state = new ZigbeeClient();

            stream.BeginRead(state.buffer, 0, ZigbeeClient.ZigbeeSize, OnReadComplete, state);
            receiveDone.WaitOne();
            receiveDone.Reset();

            int found_ucast = IndexOfByte(raw_read_byte, ucast_header) - 2;
            if (found_ucast > 0)
            {
                byte[] data = raw_read_byte.Skip(found_ucast).ToArray();
                UInt16 num_ucast, last_ucast, len_block;
                UInt16 idx, len_stream;
                idx = 0;
                len_stream = (ushort)data.Length;
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
                    Buffer.BlockCopy(data, idx + 28, subframe_block, 0, subframe_block.Length);
                    idx += len_block;
                    byte[] meta_sub = CM.Decode_SubFrame(subframe_block, subframe_block.Length);
                    Array.Resize(ref result_byte_frame, result_byte_frame.Length + meta_sub.Length);
                    Buffer.BlockCopy(meta_sub, 0, result_byte_frame, result_byte_frame.Length - meta_sub.Length, meta_sub.Length);
                }
                Data_Handler(type_command);
                result_byte_frame = new byte[0];
                raw_read_byte = new byte[0];
            }
        }

        private void OnReadComplete(IAsyncResult ar)
        {
            int bytesRead;
            ZigbeeClient state = (ZigbeeClient)ar.AsyncState;
            try
            {
                lock (stream)
                {
                    bytesRead = stream.EndRead(ar);
                }
                if (bytesRead < 1)
                    return;
                else
                {
                    Array.Resize(ref raw_read_byte, raw_read_byte.Length + bytesRead);
                    Buffer.BlockCopy(state.buffer, 0, raw_read_byte, raw_read_byte.Length - bytesRead, bytesRead);

                    if (bytesRead > 4)
                    {
                        if (state.buffer[bytesRead - 4] == 0x01)
                        {
                            if (state.buffer[bytesRead - 2] == 0x0d && state.buffer[bytesRead - 1] == 0x0a)
                            {
                                receiveDone.Set();
                            }
                            else
                                stream.BeginRead(state.buffer, 0, ZigbeeClient.ZigbeeSize, OnReadComplete, state);
                        }
                        else
                            stream.BeginRead(state.buffer, 0, ZigbeeClient.ZigbeeSize, OnReadComplete, state);
                    }
                    else
                        stream.BeginRead(state.buffer, 0, ZigbeeClient.ZigbeeSize, OnReadComplete, state);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        public void SendAsync(byte[] bytesSend)
        {
            byte[] zigbee_frame = Encode_Zigbee(bytesSend);
            stream.BeginWrite(zigbee_frame, 0, zigbee_frame.Length, OnWriteComplete, null);
            sendDone.WaitOne();
            sendDone.Reset();

        }
        private void OnWriteComplete(IAsyncResult ar)
        {
            try
            {
                // Complete sending the data to the remote device.
                stream.EndWrite(ar);
                //form1.SetLog(String.Format("Sent {0} bytes to server.", bytesSent));

                // Signal that all bytes have been sent.
                sendDone.Set();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        #endregion

        #endregion

        #region Free resource
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
            stream.Dispose();
            client.Close();
        }
        #endregion

    }
}