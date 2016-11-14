using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using CM = GatewayForm.Common;
using System.Windows.Forms;

namespace GatewayForm
{
    class TcpipConnection
    {
        //public String _ipserver="";
        //public int _port = 5000;
        private Socket psocketClient;
        private Thread pReceiveThreading;
        private Thread pRFIDprocess;
        public Boolean isconnected = false;
        public Boolean flag_received = false;
        public Boolean flag_arriveddata = false;
        private byte[] result_data_byte = new byte[0];
        public event SocketReceivedHandler MessageReceived;
        public event SocketReceivedHandler ConfigMessage;
        public event SocketReceivedHandler Log_Msg;
        static int retry_count = 3;
        int stepCmd_RFID = 1;

        private void Cmd_Raise(string cmd_str)
        {
            SocketReceivedHandler cmd_msg = ConfigMessage;
            if (cmd_msg != null)
                cmd_msg(cmd_str);
        }
        private void Log_Raise(string log_str)
        {
            SocketReceivedHandler logmsg = Log_Msg;
            if (logmsg != null)
                logmsg(log_str);
        }
        public TcpipConnection()
        {
            //_ipserver = ip;
            //_port = port;
            
        }
        public bool CreateSocketConnection(string ip_addr, int port)
        {
            isconnected = false;
            flag_received = false;
            try
            {

                if (psocketClient == null)
                {
                    IPAddress ipAddress = IPAddress.Parse(ip_addr);
                    IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
                    psocketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    psocketClient.Connect(remoteEP);
                    pReceiveThreading = new Thread(RecievedData);
                    pReceiveThreading.Start();
                    isconnected = true;
                    flag_received = true;
                }
                return true;
            }
            catch {
                MessageBox.Show("Error Open Socket");
                return false;
            }
        }
        public void close()
        {
            if(isconnected)
            {
                isconnected = false;
                flag_received = false;
                psocketClient.Close();
                pReceiveThreading.Abort();
                pReceiveThreading = null;
                psocketClient = null;

            }
        }
        public void RecievedData()
        {
            while (flag_received)
            {
                bool flag_correctdata = false;
                
                while (true)
                {
                    try
                    {
                        byte[] packet = new byte[1024];
                        int byteCount = psocketClient.Receive(packet);
                        /*foreach(byte d in packet)
                        {
                            Console.WriteLine("bb ="+(int)d);
                        }*/
                        byte[] meta_sub = CM.Decode_SubFrame(packet, byteCount);
                        if (meta_sub != null)
                        {
                            Array.Resize(ref result_data_byte, result_data_byte.Length + meta_sub.Length);
                            Buffer.BlockCopy(meta_sub, 0, result_data_byte, result_data_byte.Length - meta_sub.Length, meta_sub.Length);
                            //Console.WriteLine("bb =" + byteCount + "" +packet[byteCount - 2]);
                            if (packet[byteCount - 2] == 1)
                            {
                                flag_correctdata = true;
                                flag_arriveddata = true;
                                break;
                            }
                        }
                        else
                        {
                            MessageBox.Show("[ERROR] Message Received!");
                            break;
                        }
                    }
                    catch {
                        MessageBox.Show("Socket close");
                        break;
                    }
                }
                if (flag_correctdata)
                {
                   Data_Handler((CM.COMMAND)result_data_byte[0]);
                }
              
            }
        }
        public void startRFIDprocess()
        {
            if(pRFIDprocess!=null)
            {   try
                {
                    pRFIDprocess.Abort();
                }
                catch { }
               pRFIDprocess = null;
            }
            pRFIDprocess = new Thread(RFIDProcessing);
            pRFIDprocess.Start();
        }
        private void RFIDProcessing()
        {
            while(true)
            {
                switch (stepCmd_RFID)
                {
                    case 1:
                        Get_Command_Power(CM.COMMAND.GET_POWER_CMD, 0);
                        stepCmd_RFID = 2;
                        break;
                    case 2:
                        if (flag_arriveddata)
                        {
                            stepCmd_RFID = 3;
                            flag_arriveddata = false;
                        }
                        break;
                    case 3:
                        Get_Command_Send(CM.COMMAND.GET_POWER_MODE_CMD);
                        stepCmd_RFID = 4;
                        break;
                    case 4:
                        if (flag_arriveddata)
                        {
                            stepCmd_RFID = 5;
                            flag_arriveddata = false;
                        }
                        break;
                    case 5:
                        Get_Command_Send(CM.COMMAND.GET_REGION_CMD);
                        stepCmd_RFID = 6;
                        break;
                    case 6:
                        if (flag_arriveddata)
                        {
                            stepCmd_RFID = 7;
                            flag_arriveddata = false;
                        }
                        break;
                    default:
                        break;
                }
                if (stepCmd_RFID == 7)
                {
                    stepCmd_RFID = 1;
                    break;
                }
                Thread.Sleep(100);

            }
        }
        
        private void Request_Connection_Handler(byte[] byte_receive)
        {
            if (0x00 == byte_receive[0])
            {
                Log_Raise("Accepted!");
                Get_Command_Send(CM.COMMAND.GET_CONFIGURATION_CMD);
            }
            else
            {
                if (retry_count > 0)
                {
                    MessageBox.Show("Fail request connection\n Retry!");
                    result_data_byte = new byte[0];
                    retry_count--;

                    Send_ConnectionRequest();
                }
                else
                {
                    Log_Raise("Retry Failed. Closed Socket.");
                    retry_count = 3;
                    close();

                    // Chua retry button duoc. form1.ConnectSocket_btn.Text = "Connect";
                    return;
                }
            }
        }
        private void Data_Handler(CM.COMMAND command_option)
        {
            byte info_ack = new byte();
            string data_response = null;
            switch (command_option)
            {
                /* connection request */
                case CM.COMMAND.CONNECTION_REQUEST_CMD:
                    Request_Connection_Handler(CM.Decode_Frame((byte)command_option, result_data_byte));
                    break;
                /* configuration */
                case CM.COMMAND.GET_CONFIGURATION_CMD:
                    data_response = CM.Get_Data(CM.Decode_Frame((byte)command_option, result_data_byte));
                    Cmd_Raise(data_response);
                    break;
                case CM.COMMAND.SET_CONFIGURATION_CMD:
                    info_ack = CM.Decode_Frame_ACK((byte)command_option, result_data_byte);
                    if (0x00 == info_ack)
                        Log_Raise("Set GW Config done");
                    else
                        Log_Raise("Failed set Config");
                    break;
                /* RFID configuration */
                case CM.COMMAND.GET_RFID_CONFIGURATION_CMD:
                    data_response = CM.Get_Data(CM.Decode_Frame((byte)command_option, result_data_byte));
                    Cmd_Raise(data_response);
                    MessageBox.Show(data_response);
                    break;
                case CM.COMMAND.SET_RFID_CONFIGURATION_CMD:
                    info_ack = CM.Decode_Frame_ACK((byte)command_option, result_data_byte);
                    if (0x00 == info_ack)
                        Log_Raise("Set RFID done");
                    else
                        Log_Raise("Failed set RFID");
                    break;
                /* Port Properties */
                case CM.COMMAND.GET_PORT_PROPERTIES_CMD:
                    data_response = CM.Get_Data(CM.Decode_Frame((byte)command_option, result_data_byte));
                    Cmd_Raise(data_response);
                    MessageBox.Show(data_response);
                    break;
                case CM.COMMAND.SET_PORT_PROPERTIES_CMD:
                    info_ack = CM.Decode_Frame_ACK((byte)command_option, result_data_byte);
                    if (0x00 == info_ack)
                        Log_Raise("Set connection done");
                    else
                        Log_Raise("Failed set connection");
                    break;
                case CM.COMMAND.DIS_CONNECT_CMD:
                    info_ack = CM.Decode_Frame_ACK((byte)command_option, result_data_byte);
                    if (0x00 != info_ack)
                        MessageBox.Show("Failed disconnect");
                    else
                    {
                        Log_Raise("Disconnected");
                        close();
                    }
                    break;
                /* start operate */
                case CM.COMMAND.START_OPERATION_CMD:
                    info_ack = CM.Decode_Frame_ACK((byte)command_option, result_data_byte);
                    if (0x00 == info_ack)
                    {
                        //start_enable = true;
                        //pingTimer.Stop();
                        Log_Raise("Inventory Mode");
                    }
                    else
                        MessageBox.Show("Failed start operation");
                    break;
                /* TAG ID */
                case CM.COMMAND.REQUEST_TAG_ID_CMD:
                    
                    var messageReceived = MessageReceived;
                    byte[] byte_user = CM.Decode_Frame((byte)CM.COMMAND.REQUEST_TAG_ID_CMD, result_data_byte);

                    if (messageReceived != null)
                        messageReceived(Encoding.ASCII.GetString(byte_user, 0, byte_user.Length));
                    break;
                /* stop operate */
                case CM.COMMAND.STOP_OPERATION_CMD:
                    info_ack = CM.Decode_Frame_ACK((byte)CM.COMMAND.STOP_OPERATION_CMD, result_data_byte);
                    if (0x00 == info_ack)
                    {
                        Log_Raise("Stop operate");
                    }
                    else
                        MessageBox.Show("Failed stop operation!");
                    break;
                //Power RFID
                case CM.COMMAND.GET_POWER_CMD:
                    byte[] power_bits = CM.Decode_Frame((byte)command_option, result_data_byte);
                    if (0x00 == power_bits[0])
                        Cmd_Raise("Power RFID\n" + power_bits[1].ToString() + "\n");
                    else
                        Log_Msg("Fail get power");
                    break;
                case CM.COMMAND.SET_POWER_CMD:
                    info_ack = CM.Decode_Frame_ACK((byte)command_option, result_data_byte);
                    if (0x00 == info_ack)
                        Log_Raise("Set Power done");
                    else
                        Log_Raise("Failed Set Power");
                    break;
                //Region Configuration
                case CM.COMMAND.GET_REGION_CMD:
                    byte[] region_bits = CM.Decode_Frame((byte)command_option, result_data_byte);
                    if (0x00 == region_bits[0])
                        Cmd_Raise("Region RFID\n" + region_bits[1].ToString() + "\n");
                    else
                        Log_Msg("Fail get region");
                    break;
                case CM.COMMAND.SET_REGION_CMD:
                    info_ack = CM.Decode_Frame_ACK((byte)command_option, result_data_byte);
                    if (0x00 == info_ack)
                        Log_Raise("Set Region done");
                    else
                        Log_Raise("Failed set region");
                    break;
                //Power Mode Configuration
                case CM.COMMAND.GET_POWER_MODE_CMD:
                    byte[] pw_mode_bits = CM.Decode_Frame((byte)command_option, result_data_byte);
                    if (0x00 == pw_mode_bits[0])
                        Cmd_Raise("Power Mode RFID\n" + pw_mode_bits[1].ToString() + "\n");
                    else
                        Log_Msg("Fail get power mode");
                    break;
                case CM.COMMAND.SET_POWER_MODE_CMD:
                    info_ack = CM.Decode_Frame_ACK((byte)command_option, result_data_byte);
                    if (0x00 == info_ack)
                        Log_Raise("Set Power Mode done");
                    else
                        Log_Raise("Failed set power mode");
                    break;
                // Change Connection Type
                case CM.COMMAND.SET_CONN_TYPE_CMD:
                    data_response = CM.Get_Data(CM.Decode_Frame((byte)command_option, result_data_byte));
                    Cmd_Raise(data_response);
                    break;
                default:
                    break;
            }
            //Array.Clear(result_data_byte,0, result_data_byte.Length);
            result_data_byte = new byte[0];
            flag_arriveddata = false;
        }
        public void SendPacket(byte [] packet)
        {
            if(psocketClient!=null)
            {
                if(isconnected)
                {
                    //Console.WriteLine("sending");
                    psocketClient.Send(packet);
                }
            }
        }
        public void Send_ConnectionRequest()
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
            SendPacket(byte_req);
        }

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
            SendPacket(byte_get_cmd);
        }

        public void Set_Command_Send(CM.COMMAND command, String user_data)
        {
            CM.FrameFormat fmt_set = new CM.FrameFormat();
            CM.SubFrameFormat sub_fmt_get = new CM.SubFrameFormat();
            fmt_set.command = (byte)command;
            byte[] user_byte = Encoding.ASCII.GetBytes(user_data);
            fmt_set.length = (ushort)((ushort)CM.LENGTH.FRAME_NON_DATA + user_byte.Length);
            fmt_set.metal_data = user_byte;
            
            /* Byte data of Frame Format*/
            byte[] sub_fmt_byte = CM.Encode_Frame(fmt_set);
            
            sub_fmt_get.header = (byte)CM.HEADER.PACKET_HDR;
            sub_fmt_get.length = (ushort)(5 + sub_fmt_byte.Length);
            sub_fmt_get.metal_data = sub_fmt_byte;
            sub_fmt_get.truncate = 0x01;
            byte[] byte_set_cmd = CM.Encode_SubFrame(sub_fmt_get);
            SendPacket(byte_set_cmd);
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
            SendPacket(byte_get_cmd);
        }
        public void Set_Command_Send_Bytes(CM.COMMAND command, byte[] user_bytes)
        {
            CM.SubFrameFormat sub_fmt_get = new CM.SubFrameFormat();
            CM.FrameFormat fmt_set = new CM.FrameFormat();
            fmt_set.command = (byte)command;
            fmt_set.length = (ushort)((ushort)CM.LENGTH.FRAME_NON_DATA + user_bytes.Length);
            fmt_set.metal_data = user_bytes;

            /* Byte data of Frame Format*/
            byte[] sub_fmt_byte = CM.Encode_Frame(fmt_set);

            sub_fmt_get.header = (byte)CM.HEADER.PACKET_HDR;
            sub_fmt_get.length = (ushort)(5 + sub_fmt_byte.Length);
            sub_fmt_get.metal_data = sub_fmt_byte;
            sub_fmt_get.truncate = 0x01;
            byte[] byte_set_cmd = CM.Encode_SubFrame(sub_fmt_get);
            SendPacket(byte_set_cmd);
        }
    }
}
