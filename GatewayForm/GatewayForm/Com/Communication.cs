using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CM = GatewayForm.Common;
using System.Threading;
namespace GatewayForm
{
    public class Communication
    {
        private CM.TYPECONNECT type;
        private SocketClient zigbee;
        private TCP_Client tcp;
        //public event CM.SocketReceivedHandler TagID_Msg;
        //public event CM.SocketReceivedHandler Config_Msg;
        //public event CM.SocketReceivedHandler Log_Msg;
        //TcpipConnection pTcpipClient;
        public Communication(CM.TYPECONNECT type_connect)
        {
            this.type = type_connect;
            SelectType();
        }

        private void SelectType()
        {
            //Select new connection
            switch (type)
            {
                case CM.TYPECONNECT.HDR_ZIGBEE:
                    zigbee = new SocketClient();
                    break;
                case CM.TYPECONNECT.HDR_WIFI:
                    tcp = new TCP_Client();
                    break;
                case CM.TYPECONNECT.HDR_BLUETOOTH:
                    break;
                case CM.TYPECONNECT.HDR_ETHERNET:
                    //pTcpipClient = new TcpipConnection();
                    tcp = new TCP_Client();
                    break;
                case CM.TYPECONNECT.HDR_RS232:
                    break;
                default:
                    break;
            }
        }
        public Boolean getflagConnected_TCPIP()
        {
            return tcp.connect_ok;
        }
        public void setflagConnected_TCPIP(Boolean v)
        {
            tcp.connect_ok = v;
        }

        public void Connect(string ip_addr, int port)
        {
            switch (type)
            {
                case CM.TYPECONNECT.HDR_ZIGBEE:

                    //zigbee.Connect(ip_addr, port);
                    break;
                case CM.TYPECONNECT.HDR_WIFI:
                    tcp.InitClient(ip_addr, port);
                    if (!getflagConnected_TCPIP())
                    {
                        
                    }
                    /*if (pTcpipClient != null)
                    {
                        pTcpipClient.CreateSocketConnection(ip_addr, port);
                        if (pTcpipClient.isconnected)
                        {
                            pTcpipClient.ConfigMessage += passed_config;
                            pTcpipClient.MessageReceived += passed_event;
                            pTcpipClient.Log_Msg += passed_log;
                            //pTcpipClient.flag_arriveddata = recv_flag;
                            pTcpipClient.Send_ConnectionRequest();
                        }
                    }*/
                    break;
                case CM.TYPECONNECT.HDR_BLUETOOTH:
                    break;
                case CM.TYPECONNECT.HDR_ETHERNET:
                    tcp.InitClient(ip_addr, port);
                    if (!getflagConnected_TCPIP())
                    {
                       
                    }
                    break;
                case CM.TYPECONNECT.HDR_RS232:
                    break;
                default:
                    break;
            }
        }
        public void Close()
        {
            switch (type)
            {
                case CM.TYPECONNECT.HDR_ZIGBEE:
                    zigbee = null;
                    break;
                case CM.TYPECONNECT.HDR_WIFI:
                    tcp.Free();
                    break;
                case CM.TYPECONNECT.HDR_BLUETOOTH:
                    break;
                case CM.TYPECONNECT.HDR_ETHERNET:
                    tcp.Free();
                    /*pp.close();
                    pTcpipClient.MessageReceived -= passed_event; //chu y
                    pTcpipClient.ConfigMessage -= passed_config;
                    pTcpipClient.Log_Msg -= passed_log;*/
                    break;
                case CM.TYPECONNECT.HDR_RS232:
                    break;
                default:
                    break;
            }
        }

        public void Get_Command_Send(CM.COMMAND command_type)
        {
            switch (type)
            {
                case CM.TYPECONNECT.HDR_ZIGBEE:
                    if (zigbee != null)
                        zigbee.Get_Command_Send(command_type);
                    break;
                case CM.TYPECONNECT.HDR_WIFI:
                    if (tcp != null)
                    {
                        if (getflagConnected_TCPIP())
                            tcp.Get_Command_Send(command_type);
                        else
                            tcp.Free();
                    }
                    break;
                case CM.TYPECONNECT.HDR_BLUETOOTH:
                    break;
                case CM.TYPECONNECT.HDR_ETHERNET:
                    if (tcp != null)
                    {
                        if (getflagConnected_TCPIP())
                            tcp.Get_Command_Send(command_type);
                        else
                            tcp.Free();
                    }
                    /*if (pTcpipClient != null)
                    {
                        if(pTcpipClient.isconnected)
                            pTcpipClient.Get_Command_Send(command_type);
                    }*/
                    break;
                case CM.TYPECONNECT.HDR_RS232:
                    break;
                default:
                    break;
            }
        }

        public void Get_Command_Power(CM.COMMAND command_type, byte option_mode)
        {
            switch (type)
            {
                case CM.TYPECONNECT.HDR_ZIGBEE:
                    if (zigbee != null)
                        zigbee.Get_Command_Power(command_type, option_mode);
                    break;
                case CM.TYPECONNECT.HDR_WIFI:
                    if (tcp != null)
                    {
                        if (getflagConnected_TCPIP())
                            tcp.Get_Command_Power(command_type, option_mode);
                        else
                            tcp.Free();
                    }
                    break;
                case CM.TYPECONNECT.HDR_BLUETOOTH:
                    break;
                case CM.TYPECONNECT.HDR_ETHERNET:
                    if (tcp != null)
                    {
                        if (getflagConnected_TCPIP())
                            tcp.Get_Command_Power(command_type, option_mode);
                        else
                            tcp.Free();
                    }
                    /*if (pTcpipClient != null)
                    {
                        if (pTcpipClient.isconnected)
                            pTcpipClient.Get_Command_Power(command_type, option_mode);
                    }*/
                    break;
                case CM.TYPECONNECT.HDR_RS232:
                    break;
                default:
                    break;
            }
        }

        public void Set_Command_Send(CM.COMMAND command_type, String user_str)
        {
            switch (type)
            {
                case CM.TYPECONNECT.HDR_ZIGBEE:
                    if (zigbee != null)
                        zigbee.Set_Command_Send(command_type, user_str);
                    break;
                case CM.TYPECONNECT.HDR_WIFI:
                    if (tcp != null)
                    {
                        if (getflagConnected_TCPIP())
                            tcp.Set_Command_Send(command_type, user_str);
                        else
                            tcp.Free();
                    }
                    break;
                case CM.TYPECONNECT.HDR_BLUETOOTH:
                    break;
                case CM.TYPECONNECT.HDR_ETHERNET:
                    if (tcp != null)
                    {
                        if (getflagConnected_TCPIP())
                            tcp.Set_Command_Send(command_type, user_str);
                        else
                            tcp.Free();
                    }
                    /*if (pTcpipClient != null)
                    {
                        if (pTcpipClient.isconnected)
                            pTcpipClient.Set_Command_Send(command_type, user_str);
                    }*/
                    break;
                case CM.TYPECONNECT.HDR_RS232:
                    break;
                default:
                    break;
            }
        }
        public void Set_Command_Send_Bytes(CM.COMMAND command_type, byte[] user_bytes)
        {
            switch (type)
            {
                case CM.TYPECONNECT.HDR_ZIGBEE:
                    if (zigbee != null)
                        zigbee.Set_Command_Send_Bytes(command_type, user_bytes);
                    break;
                case CM.TYPECONNECT.HDR_WIFI:
                    if (tcp != null)
                    {
                        if (getflagConnected_TCPIP())
                            tcp.Set_Command_Send_Bytes(command_type, user_bytes);
                        else
                            tcp.Free();
                    }
                    break;
                case CM.TYPECONNECT.HDR_BLUETOOTH:
                    break;
                case CM.TYPECONNECT.HDR_ETHERNET:
                    if (tcp != null)
                    {
                        if (getflagConnected_TCPIP())
                            tcp.Set_Command_Send_Bytes(command_type, user_bytes);
                        else
                            tcp.Free();
                    }
                    /*if (pTcpipClient != null)
                    {
                        if (pTcpipClient.isconnected)
                            pTcpipClient.Set_Command_Send_Bytes(command_type, user_bytes);
                    }*/
                    break;
                case CM.TYPECONNECT.HDR_RS232:
                    break;
                default:
                    break;
            }
        }

        public void Receive_Command_Handler(CM.COMMAND command_type)
        {
            switch (type)
            {
                case CM.TYPECONNECT.HDR_ZIGBEE:
                    if (zigbee != null)
                        zigbee.ReadAsync(command_type);
                    break;
                case CM.TYPECONNECT.HDR_WIFI:
                    if (tcp != null)
                    {
                        if (getflagConnected_TCPIP())
                            tcp.Receive_Command_Handler(command_type);
                        else
                            tcp.Free();
                    }
                    break;
                case CM.TYPECONNECT.HDR_BLUETOOTH:
                    break;
                case CM.TYPECONNECT.HDR_ETHERNET:
                    if (tcp != null)
                    {
                        if (getflagConnected_TCPIP())
                            tcp.Receive_Command_Handler(command_type);
                        else
                            tcp.Free();
                    }
                    //tcp.Receive_Command_Handler(command_type);
                    break;
                case CM.TYPECONNECT.HDR_RS232:
                    break;
                default:
                    break;
            }
        }

        /*public void RFID_Process()
        {
            switch (type)
            {
                case CM.TYPECONNECT.HDR_ZIGBEE:
                    //zigbee.ReadAsync(command_type);
                    break;
                case CM.TYPECONNECT.HDR_WIFI:
                    if (pTcpipClient != null)
                    {
                        if (pTcpipClient.isconnected)
                            pTcpipClient.startRFIDprocess();
                    }
                    break;
                case CM.TYPECONNECT.HDR_BLUETOOTH:
                    break;
                case CM.TYPECONNECT.HDR_ETHERNET:
                    if (pTcpipClient != null)
                    {
                        if (pTcpipClient.isconnected)
                            pTcpipClient.startRFIDprocess();
                    }
                    break;
                case CM.TYPECONNECT.HDR_RS232:
                    break;
                default:
                    break;
            }
        }

        private void passed_event(string str_event)
        {
            CM.SocketReceivedHandler recv_msg = TagID_Msg;
            if (recv_msg != null)
                recv_msg(str_event);
        }
        private void passed_config(string config_str)
        {
            CM.SocketReceivedHandler get_msg = Config_Msg;
            if (get_msg != null)
                get_msg(config_str);
        }
        private void passed_log(string log_str)
        {
            CM.SocketReceivedHandler get_log = Log_Msg;
            if (get_log != null)
                get_log(log_str);
        }*/
    }
}
