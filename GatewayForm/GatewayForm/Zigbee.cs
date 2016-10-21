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
    public class StateZigbee
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 102;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        //public StringBuilder sb = new StringBuilder();
    }

    public delegate void ZigbeeReceivedHandler(string msg);

    public class Zigbee : IDisposable
    {
        public Socket zigbee_conn;
        /// <summary>
        /// Byte Data of Frame Format
        /// </summary>
        private List<byte> byte_List = new List<byte>();
        // The port number for the remote device.
        private static byte[] zigbee_header_byte = Encoding.ASCII.GetBytes("AT+UCAST:000D6F000C45D3B9=");
        private static byte[] seq_header = Encoding.ASCII.GetBytes("SEQ");
        private static byte[] ack_header = Encoding.ASCII.GetBytes("ACK");
        private static byte[] nack_header = Encoding.ASCII.GetBytes("NACK");
        private static byte[] ucast_header = Encoding.ASCII.GetBytes("UCAST");
        // ManualResetEvent instances signal completion.                             
        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);
        // Event Handler
        //public event ZigbeeReceivedHandler MessageReceived;
        //private string respose;
        public bool IsDead { get; set; }
        private bool IsDisposed = false;

        // The response from the remote device.
        private Form1 form1;

        public Zigbee(Form1 form1)
        {
            this.form1 = form1;
            this.IsDead = false;
        }

        #region Esblish connection
        /// <summary>
        /// Esblablish connection to Gateway server
        /// </summary>
        /// <param name="ip_server"></param> IP address of server in string format
        /// <param name="port"></param> remote port default 5000
        public void InitClient(string ip_server, int port)
        {
            // Connect to a remote device.
            // Establish the remote endpoint for the socket.
            IPAddress ipAddress = IPAddress.Parse(ip_server);
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

            // Create a TCP/IP socket.
            zigbee_conn = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Connect to the remote endpoint.
            zigbee_conn.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), zigbee_conn);
            connectDone.WaitOne();
            Send_ConnectionRequest();
            Receive_Command_Handler(CM.COMMAND.CONNECTION_REQUEST_CMD);
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

                // Complete the connection.
                client.EndConnect(ar);

                //form1.SetLog(String.Format("Socket connected to {0}", client.RemoteEndPoint.ToString()));
                // Signal that the connection has been made.
                connectDone.Set();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        #endregion

        #region API Encode Data to byte stream

        /// <summary>
        /// Calculate Checksum for byte data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private byte Chcksum(byte[] data, int length)
        {
            byte sum = 0;
            for (int i = 0; i < length; i++)
                sum += data[i];
            sum = (byte)(~sum + 1);

            return sum;
        }

        /// <summary>
        /// Encoding tcp packet to byte streaming
        /// </summary>
        /// <param name="sub_fmt"></param>
        /// <returns></returns>
        private byte[] Encode_SubFrame(CM.SubFrameFormat sub_fmt)
        {
            byte[] send_bytes = new byte[sub_fmt.length + 1];
            send_bytes[0] = sub_fmt.header;
            send_bytes[1] = (byte)((sub_fmt.length >> 8) & 0xff);
            send_bytes[2] = (byte)(sub_fmt.length & 0xff);
            if (sub_fmt.length > 5)
                Buffer.BlockCopy(sub_fmt.metal_data, 0, send_bytes, 3, sub_fmt.length - 5);
            send_bytes[sub_fmt.length - 2] = (byte)((sub_fmt.truncate >> 8) & 0xff);
            send_bytes[sub_fmt.length - 1] = (byte)(sub_fmt.truncate & 0xff);
            send_bytes[sub_fmt.length] = Chcksum(send_bytes.Skip(1).ToArray(), sub_fmt.length - 1);
            return send_bytes;
        }

        /// <summary>
        /// Encoding user data to byte with Frame Format
        /// </summary>
        /// <param name="fmt"></param>
        /// <returns></returns>
        private byte[] Encode_Frame(CM.FrameFormat fmt)
        {
            byte[] byte_frame = new byte[fmt.length];
            byte_frame[0] = fmt.command;
            byte_frame[1] = (byte)((fmt.length >> 8) & 0xff);
            byte_frame[2] = (byte)(fmt.length & 0xff);
            if (fmt.length > 4)
                Buffer.BlockCopy(fmt.metal_data, 0, byte_frame, 3, fmt.length - 4);
            byte_frame[fmt.length - 1] = Chcksum(byte_frame, fmt.length - 1);
            return byte_frame;
        }
        #endregion

        #region Send Command
        /// <summary>
        /// Send byte raw of frame data with Zigbee Header
        /// </summary>
        /// <param name="b_frame"></param> byte array of sending frame
        public void SendFrame(byte[] b_frame)
        {
            byte[] zigbee_frame = Encode_Zigbee(b_frame);
            // Begin sending the packet to the remote device.
            zigbee_conn.BeginSend(zigbee_frame, 0, zigbee_frame.Length, 0, new AsyncCallback(SendCallback), zigbee_conn);
            sendDone.WaitOne();
        }

        private static byte[] Encode_Zigbee(byte[] sub_fmt_byte)
        {
            byte[] concat = new byte[zigbee_header_byte.Length + sub_fmt_byte.Length + 1];
            Buffer.BlockCopy(zigbee_header_byte, 0, concat, 0, zigbee_header_byte.Length);
            Buffer.BlockCopy(sub_fmt_byte, 0, concat, zigbee_header_byte.Length, sub_fmt_byte.Length);
            concat[zigbee_header_byte.Length + sub_fmt_byte.Length] = 0x0D;
            return concat;
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
            sub_fmt_send.metal_data = Encode_Frame(req_conn_fmt);

            sub_fmt_send.truncate = 0x01;

            byte[] byte_req = Encode_SubFrame(sub_fmt_send);

            // Begin sending Connection Request Command.
            SendFrame(byte_req);
            sendDone.Reset();
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

            sub_fmt_get.metal_data = Encode_Frame(fmt_get);
            sub_fmt_get.header = (byte)CM.HEADER.PACKET_HDR;
            sub_fmt_get.length = (ushort)CM.LENGTH.SUB_FRAME_NON_DATA;
            sub_fmt_get.truncate = 0x01;

            byte[] byte_get_cmd = Encode_SubFrame(sub_fmt_get);
            SendFrame(byte_get_cmd);
            sendDone.Reset();
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
            byte[] sub_fmt_byte = Encode_Frame(fmt_set);
            Send_Packet(sub_fmt_byte);
        }

        public void Send_Packet(byte[] frame_data_byte)
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
                byte[] byte_tcp = Encode_SubFrame(sub_fmt_send);
                SendFrame(byte_tcp);
                sendDone.Reset();
                idx += len_transmit;
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
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

        #region Request Connection Command

        static int retry_count = 3;
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
                    //sendDone.WaitOne();
                    Receive_Command_Handler(CM.COMMAND.CONNECTION_REQUEST_CMD); 
                    receiveDone.Reset();
                }
                else
                {
                    //form1.SetLog("Retry Failed. Closed Socket.");
                    retry_count = 3;
                    zigbee_conn.Close();

                    // Chua retry button duoc. form1.ConnectSocket_btn.Text = "Connect";
                    return;
                }
            }
        }

        #endregion

        #region Get Data Command
        private string Get_Data(byte[] byte_data)
        {
            string info_data;
            if (0x00 == byte_data[0])
            {
                //MessageBox.Show("ACK. Get info");
                info_data = Encoding.ASCII.GetString(byte_data.Skip(1).ToArray(), 0, byte_data.Length - 1);
            }
            else
            {
                info_data = "NAK";
            }
            return info_data;
        }
        #endregion

        #region API Decode byte stream receive to meta data
        /// <summary>
        /// Decoding the SubFrameFormat is sent via TCP connection.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="len"></param>
        /// <returns>Meta Data bytes</returns>
        private byte[] Decode_SubFrame(byte[] buffer, ref ushort packet)
        {
            CM.SubFrameFormat sub_fmt = new CM.SubFrameFormat();
            int datalen_subfmt;
            /* Sub Frame Format*/
            // Header ignore
            sub_fmt.header = buffer[0];
            if (buffer[0] != (byte)CM.HEADER.RESP_PACKET_HDR)
            {
                //form1.SetLog("The TCP packet not send by RFID Gateway");
                return null;
            }
            //Length
            sub_fmt.length = (ushort)(buffer[2] + (buffer[1] << 8)); // deduce 1 byte header
            datalen_subfmt = sub_fmt.length - 5;
            if (sub_fmt.length != buffer.Length - 1)
            {
                //form1.SetLog("Buffer overflow/Error read length");
                return null;
            }

            //Check CRC
            sub_fmt.checksum = buffer[sub_fmt.length];
            if (sub_fmt.checksum != Chcksum(buffer.Skip(1).ToArray(), sub_fmt.length - 1))
            {
                //form1.SetLog("Error CRC Packet ");
                return null;
            }

            /*Data of TCP packet is part of Frame Format*/
            sub_fmt.metal_data = new byte[datalen_subfmt];
            Buffer.BlockCopy(buffer, 3, sub_fmt.metal_data, 0, datalen_subfmt);
            //Number of TCP packet be slpited in meta data
            sub_fmt.truncate = (ushort)(buffer[sub_fmt.length - 1] + (buffer[sub_fmt.length - 2] << 8));
            packet = sub_fmt.truncate;

            //return sub_fmt.metal_data;
            return sub_fmt.metal_data;
        }
        /// <summary>
        /// Decoding FrameFormat is slpit by RFID gateway
        /// </summary>
        /// <param name="meta_sub"></param>
        /// <returns></returns>
        private byte[] Decode_Frame(byte command_option)
        {
            CM.FrameFormat fmt = new CM.FrameFormat();
            int datalen_fmt;
            byte[] meta_sub = byte_List.ToArray();
            /* Frame Format*/
            //Command
            fmt.command = meta_sub[0];
            if (fmt.command != command_option)
            {
                //form1.SetLog("Error Command Frame Format");
                return null;
            }

            //Length
            fmt.length = (ushort)(meta_sub[2] + (meta_sub[1] << 8));
            if (fmt.length != meta_sub.Length)
            {
                //form1.SetLog("Wrong User Data Length");
                return null;
            }

            //CRC
            fmt.checksum = meta_sub[fmt.length - 1];
            if (fmt.checksum != Chcksum(meta_sub, fmt.length - 1))
            {
                //form1.SetLog("Error CRC Data User");
                return null;
            }

            /* Data Frame Format*/
            datalen_fmt = fmt.length - 4;
            fmt.metal_data = new byte[datalen_fmt];
            Buffer.BlockCopy(meta_sub, 3, fmt.metal_data, 0, datalen_fmt);
            byte_List.Clear();
            return fmt.metal_data;
        }

        private byte Decode_Frame_ACK(byte command_option)
        {
            CM.FrameFormat fmt = new CM.FrameFormat();
            byte[] meta_sub = byte_List.ToArray();
            /* Frame Format*/
            //Command
            fmt.command = meta_sub[0];
            if (fmt.command != command_option)
            {
                //form1.SetLog("Error Command Frame Format");
                return 0;
            }

            //Length
            fmt.length = (ushort)(meta_sub[2] + (meta_sub[1] << 8));
            if (fmt.length != 5)
            {
                //form1.SetLog("Wrong User Data Length");
                return 0;
            }

            //CRC
            fmt.checksum = meta_sub[4];
            if (fmt.checksum != Chcksum(meta_sub, fmt.length - 1))
            {
                //form1.SetLog("Error CRC Data User");
                return 0;
            }

            byte_List.Clear();
            return meta_sub[3];
        }
        #endregion

        #region Read Handler

        public static string ByteArrayToString(byte[] ba)
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
        //static UInt16 no_pack;
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
                StateZigbee state = (StateZigbee)ar.AsyncState;
                Socket client = state.workSocket;
                
                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);

                Console.WriteLine("len: {0} - {1}", bytesRead, ByteArrayToString(state.buffer));
                if (bytesRead > 0)
                {
                    /*
                    if(Match_Bytes(state.buffer, ucast_header, 2))
                    {
                        byte[] zigbee_data = new byte[bytesRead - 30];
                        Buffer.BlockCopy(state.buffer, 28, zigbee_data, 0, bytesRead - 30);
                        byte[] meta_sub = Decode_SubFrame(zigbee_data, ref no_pack);
                        if (meta_sub == null)
                            MessageBox.Show("Error Sub Frame Format");
                        else
                        {
                            byte_List.AddRange(meta_sub);
                        }
                        if(no_pack != 1)
                        {
                            client.BeginReceive(state.buffer, 0, StateZigbee.BufferSize, 0,
                            new AsyncCallback(Receive_Command_Callback), state);
                        }
                        else
                        {
                            receiveDone.Set();
                        }
                    }*/
                        client.BeginReceive(state.buffer, 0, StateZigbee.BufferSize, 0,
                        new AsyncCallback(Receive_Command_Callback), state);
                }
                    receiveDone.Set();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        /// <summary>
        /// Receive Command Data Response.
        /// </summary>
        /// <param name="command_type"></param>
        public void Receive_Command_Handler(CM.COMMAND command_type)
        {
            // Create the state object.
            StateZigbee state = new StateZigbee();
            state.workSocket = zigbee_conn;

            // Begin receiving the data from the remote device.
            zigbee_conn.BeginReceive(state.buffer, 0, StateZigbee.BufferSize, 0,
                new AsyncCallback(Receive_Command_Callback), state);
            receiveDone.WaitOne();
            receiveDone.Reset();
        }

        #endregion

        /// <summary>
        /// Free TCP resource
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            IsDead = true;
            //form1.SetLog("Socket disconneted");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {

                if (disposing)
                {
                    zigbee_conn.Shutdown(SocketShutdown.Both);//try catch here
                    zigbee_conn.Close();
                }
                IsDisposed = true;
            }
        }

        ~Zigbee()
        {
            // Release the socket.
            Dispose(false);
        }

    }
}
