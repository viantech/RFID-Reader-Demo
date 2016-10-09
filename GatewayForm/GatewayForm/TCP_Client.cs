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
    // State object for receiving data from remote device.
    public class StateTCPClient
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Command type
        //public CM.COMMAND type;
    }

    //public delegate void SocketReceivedHandler(string msg);

    public class TCP_Client : IDisposable
    {
        public Socket tcp_client;
        /// <summary>
        /// Byte Data of Frame Format
        /// </summary>
        private byte[] result_data_byte = new byte[0];
        // The port number for the remote device.
        private const int defaultport = 5000;

        // ManualResetEvent instances signal completion.
        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);
        // Event Handler
        public event SocketReceivedHandler MessageReceived;
        public event SocketReceivedHandler ConfigMessage;
        private string respose;
        public bool IsDead { get; set; }
        private bool IsDisposed = false;

        // The response from the remote device.

        public TCP_Client()
        {
            // TODO: Complete member initialization
            //this.form1 = form1;
            this.IsDead = false;
        }

        #region Establish connection
        /// <summary>
        /// Esblablish connection to Gateway server
        /// </summary>
        /// <param name="ip_server"></param> IP address of server in string format
        /// <param name="port"></param> remote port default 5000
        public void InitClient(string ip_server, int port = defaultport)
        {
            // Connect to a remote device.
            // Establish the remote endpoint for the socket.
            IPAddress ipAddress = IPAddress.Parse(ip_server);
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

            // Create a TCP/IP socket.
            tcp_client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Connect to the remote endpoint.
            tcp_client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), tcp_client);
            connectDone.WaitOne();

            //Send Connection Request
            Send_ConnectionRequest();

            //Receive the response status
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

                Console.Write(String.Format("Socket connected to {0}", client.RemoteEndPoint.ToString()));
                // Signal that the connection has been made.
                connectDone.Set();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        #endregion

        #region Send Command
        /// <summary>
        /// Send byte raw of frame data 
        /// </summary>
        /// <param name="b_frame"></param> byte array of sending frame
        private void SendFrame(byte[] b_frame)
        {
            //byte[] frame_to_buffer;
            //frame_to_buffer = getBytes(frame);
            // Begin sending the packet to the remote device.
            tcp_client.BeginSend(b_frame, 0, b_frame.Length, 0, new AsyncCallback(SendCallback), tcp_client);
            sendDone.WaitOne();
            sendDone.Reset();
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
            Send_Packet(sub_fmt_byte);
        }

        private void Send_Packet(byte[] frame_data_byte)
        {
            UInt16 num_packet, last_packet_byte, len_transmit;
            UInt16 idx, len_byte_fmt;
            idx = 0;
            len_byte_fmt = (ushort)frame_data_byte.Length;
            num_packet = (ushort)(len_byte_fmt / ((ushort)CM.LENGTH.MAX_SIZE_TCP_META_DATA));
            last_packet_byte = (ushort)(len_byte_fmt % ((ushort)CM.LENGTH.MAX_SIZE_TCP_META_DATA));
            if (last_packet_byte > 0)
                num_packet += 1;
            len_transmit = (ushort)((ushort)CM.LENGTH.MAX_SIZE_TCP_META_DATA + (ushort)CM.LENGTH.SIZE_SUB_FRAME_APPEND); //1024
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
                idx += len_transmit;
                sub_fmt_send.truncate = (ushort)(num_packet - i);
                byte[] byte_tcp = CM.Encode_SubFrame(sub_fmt_send);
                SendFrame(byte_tcp);
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
                MessageBox.Show("Accepted!");
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
                }
                else
                {
                    //form1.SetLog("Retry Failed. Closed Socket.");
                    retry_count = 3;
                    tcp_client.Close();

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

        #region Read Handler
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

                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    byte[] meta_sub = CM.Decode_SubFrame(state.buffer, bytesRead);
                    Array.Resize(ref result_data_byte, result_data_byte.Length + meta_sub.Length);
                    Buffer.BlockCopy(meta_sub, 0, result_data_byte, result_data_byte.Length - meta_sub.Length, meta_sub.Length);
                    if (0x01 == state.buffer[bytesRead - 2])
                    {
                        receiveDone.Set();
                    }
                    else
                    {
                        client.BeginReceive(state.buffer, 0, StateTCPClient.BufferSize, 0,
                        new AsyncCallback(Receive_Command_Callback), state);
                    }
                }
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
            StateTCPClient state = new StateTCPClient();
            state.workSocket = tcp_client;

            // Begin receiving the data from the remote device.
            tcp_client.BeginReceive(state.buffer, 0, StateTCPClient.BufferSize, 0,
                new AsyncCallback(Receive_Command_Callback), state);
            receiveDone.WaitOne();
            receiveDone.Reset();

            // After received all packet
            byte info_ack;
            string data_response;
            SocketReceivedHandler command_rev;
            switch (command_type)
            {
                /* connection request */
                case CM.COMMAND.CONNECTION_REQUEST_CMD:
                    Request_Connection_Handler(CM.Decode_Frame((byte)CM.COMMAND.CONNECTION_REQUEST_CMD, result_data_byte));
                    break;
                /* configuration */
                case CM.COMMAND.GET_CONFIGURATION_CMD:
                    data_response = Get_Data(CM.Decode_Frame((byte)CM.COMMAND.GET_CONFIGURATION_CMD, result_data_byte));
                    command_rev = ConfigMessage;
                    if (command_rev != null)
                        command_rev(data_response);
                    break;
                case CM.COMMAND.SET_CONFIGURATION_CMD:
                    info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.SET_CONFIGURATION_CMD, result_data_byte);
                    if (0x00 == info_ack)
                        MessageBox.Show("Set Config success");
                    else
                        MessageBox.Show("Failed set Config");
                    break;
                /* RFID configuration */
                case CM.COMMAND.GET_RFID_CONFIGURATION_CMD:
                    data_response = Get_Data(CM.Decode_Frame((byte)CM.COMMAND.GET_RFID_CONFIGURATION_CMD, result_data_byte));
                    command_rev = ConfigMessage;
                    if (command_rev != null)
                        command_rev(data_response);
                    break;
                case CM.COMMAND.SET_RFID_CONFIGURATION_CMD:
                    info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.SET_RFID_CONFIGURATION_CMD, result_data_byte);
                    if (0x00 == info_ack)
                        MessageBox.Show("Set RFID successfull");
                    else
                        MessageBox.Show("Failed set RFID");
                    break;
                /* Port Properties */
                case CM.COMMAND.GET_PORT_PROPERTIES_CMD:
                    data_response = Get_Data(CM.Decode_Frame((byte)CM.COMMAND.GET_PORT_PROPERTIES_CMD, result_data_byte));
                    command_rev = ConfigMessage;
                    if (command_rev != null)
                        command_rev(data_response);
                    break;
                case CM.COMMAND.SET_PORT_PROPERTIES_CMD:
                    info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.SET_PORT_PROPERTIES_CMD, result_data_byte);
                    if (0x00 == info_ack)
                        MessageBox.Show("Set connection successfull");
                    else
                        MessageBox.Show("Failed set connection");
                    break;
                /* start operate */
                case CM.COMMAND.START_OPERATION_CMD:
                    info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.START_OPERATION_CMD, result_data_byte);
                    if (0x00 == info_ack)
                    {
                        MessageBox.Show("Starting read TAG ID");
                        Receive_Data_Handler();
                    }
                    else
                        MessageBox.Show("Failed start operation");
                    break;
                /* stop operate */
                case CM.COMMAND.STOP_OPERATION_CMD:
                    info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.STOP_OPERATION_CMD, result_data_byte);
                    if (0x00 == info_ack)
                        MessageBox.Show("Stop operate");
                    else
                        MessageBox.Show("Failed stop operate");
                    break;
                default:
                    break;
            }

            result_data_byte = new byte[0];
        }
        
        private void Receive_Data_Callback(IAsyncResult ar)
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
                    Array.Resize(ref result_data_byte, result_data_byte.Length + meta_sub.Length);
                    Buffer.BlockCopy(meta_sub, 0, result_data_byte, result_data_byte.Length - meta_sub.Length, meta_sub.Length);
                }
                if (bytesRead == StateTCPClient.BufferSize)
                {
                    client.BeginReceive(state.buffer, 0, StateTCPClient.BufferSize, 0,
                    new AsyncCallback(Receive_Data_Callback), state);
                }
                else
                {
                    /* TAG ID */
                    var messageReceived = MessageReceived;
                    byte[] byte_user = CM.Decode_Frame((byte)CM.COMMAND.REQUEST_TAG_ID_CMD, result_data_byte);
                    respose = Encoding.ASCII.GetString(byte_user, 0, byte_user.Length);
                    if (!String.IsNullOrEmpty(respose))
                        messageReceived(respose);
                    result_data_byte = new byte[0];
                    Receive_Data_Handler();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
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
            tcp_client.BeginReceive(state.buffer, 0, StateTCPClient.BufferSize, 0,
                new AsyncCallback(Receive_Data_Callback), state);
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
                    tcp_client.Shutdown(SocketShutdown.Both);//try catch here
                    tcp_client.Close();
                    receiveDone.Close();
                }
                IsDisposed = true;
            }
        }

        ~TCP_Client()
        {
            // Release the socket.
            Dispose(false);
        }

    }

}
