﻿using System;
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
        public event SocketReceivedHandler TagID_Msg;
        public event SocketReceivedHandler Config_Msg;

        private CM.TYPECONNECT type;
        private SocketClient zigbee;
        private Eth_Client tcp;
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
                    tcp = new Eth_Client();
                    break;
                case CM.TYPECONNECT.HDR_BLUETOOTH:
                    break;
                case CM.TYPECONNECT.HDR_ETHERNET:
                    //pTcpipClient = new TcpipConnection();
                    tcp = new Eth_Client();
                    break;
                case CM.TYPECONNECT.HDR_RS232:
                    break;
                default:
                    break;
            }
        }
        public Boolean getflagConnected_TCPIP()
        {
            if (tcp != null)
                return tcp.connect_ok;
            else if (zigbee != null)
                return zigbee.connect_ok;
            else return false;
        }
        public void resetflag()
        {
            if (tcp != null)
                tcp.receiveDone.Reset();
            else if (zigbee != null)
                zigbee.receiveDone.Reset();
            else
                return;
        }
        public Boolean waitflagRevTCP(int time = 2000)
        {
            if (tcp != null)
                return tcp.receiveDone.WaitOne(time);
            else if (zigbee != null)
                return zigbee.receiveDone.WaitOne(time);
            else
                return false;
        }

        public void Connect(string ip_addr, int port)
        {
            switch (type)
            {
                case CM.TYPECONNECT.HDR_ZIGBEE:
                    zigbee.Connect(ip_addr, port);
                    zigbee.Config_Msg += passed_config;
                    zigbee.TagID_Msg += passed_event;
                    break;
                case CM.TYPECONNECT.HDR_WIFI:
                    tcp.InitClient(ip_addr, port);
                    tcp.Config_Msg += passed_config;
                    tcp.TagID_Msg += passed_event;
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
                    tcp.Config_Msg += passed_config;
                    tcp.TagID_Msg += passed_event;
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
                    zigbee.Free();
                    zigbee.Config_Msg -= passed_config;
                    zigbee.TagID_Msg -= passed_event;
                    zigbee = null;
                    break;
                case CM.TYPECONNECT.HDR_WIFI:
                    tcp.Free();
                    tcp.Config_Msg -= passed_config;
                    tcp.TagID_Msg -= passed_event;
                    tcp = null;
                    break;
                case CM.TYPECONNECT.HDR_BLUETOOTH:
                    break;
                case CM.TYPECONNECT.HDR_ETHERNET:
                    tcp.Free();
                    tcp.Config_Msg -= passed_config;
                    tcp.TagID_Msg -= passed_event;
                    tcp = null;
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
        public void StartCmd_Process(CM.COMMAND command_type)
        {
            switch (type)
            {
                case CM.TYPECONNECT.HDR_ZIGBEE:
                    if (zigbee != null)
                    {
                        zigbee.Start_Process(command_type);
                    }
                    break;
                case CM.TYPECONNECT.HDR_WIFI:
                    if (tcp != null)
                    {
                        if (getflagConnected_TCPIP())
                            tcp.Start_Command_Process(command_type);
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
                            tcp.Start_Command_Process(command_type);
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
        public void Update_File(byte[] file_bytes, string info)
        {
            switch (type)
            {
                case CM.TYPECONNECT.HDR_ZIGBEE:
                    if (zigbee != null)
                    {
                        MessageBox.Show("We not support update firmware in Zigbee protocol");
                    }
                    break;
                case CM.TYPECONNECT.HDR_WIFI:
                    if (tcp != null)
                    {
                        if (getflagConnected_TCPIP())
                            tcp.Send_File(file_bytes, info);
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
                            tcp.Send_File(file_bytes, info);
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
        /*public void Receive_Command_Handler(CM.COMMAND command_type)
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

        public void RFID_Process()
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
        }*/

        private void passed_event(string str_event)
        {
            SocketReceivedHandler recv_msg = TagID_Msg;
            if (recv_msg != null)
                recv_msg(str_event);
        }
        private void passed_config(string config_str)
        {
            SocketReceivedHandler get_msg = Config_Msg;
            if (get_msg != null)
                get_msg(config_str);
        }
        /*private void passed_log(string log_str)
        {
            CM.SocketReceivedHandler get_log = Log_Msg;
            if (get_log != null)
                get_log(log_str);
        }*/
    }
}
