using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CM = GatewayForm.Common;

namespace GatewayForm
{
    public class Communication
    {
        private CM.TYPECONNECT type;
        private SocketClient zigbee;
        private TCP_Client tcp;
        public bool connect_ok = true;
        //public delegate void ReceivedHandler(string msg);
        public event SocketReceivedHandler TagID_Msg;
        public event SocketReceivedHandler Config_Msg;
        public event SocketReceivedHandler Log_Msg;
        public Communication (CM.TYPECONNECT type_connect)
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
                    break;
                case CM.TYPECONNECT.HDR_BLUETOOTH:
                    break;
                case CM.TYPECONNECT.HDR_ETHERNET:
                    tcp = new TCP_Client();
                    break;
                case CM.TYPECONNECT.HDR_RS232:
                    break;
                default:
                    break;
            }
        }

        public void Connect (string ip, int port)
        {
            switch (type)
            {
                case CM.TYPECONNECT.HDR_ZIGBEE:
                    zigbee.TagID_Received += new SocketReceivedHandler(passed_event);
                    zigbee.Get_Configuration += new SocketReceivedHandler(passed_config);
                    zigbee.Log_Msg += new SocketReceivedHandler(passed_log);
                    zigbee.Connect(ip, port);
                    break;
                case CM.TYPECONNECT.HDR_WIFI:
                    break;
                case CM.TYPECONNECT.HDR_BLUETOOTH:
                    break;
                case CM.TYPECONNECT.HDR_ETHERNET:
                    tcp.MessageReceived += new SocketReceivedHandler(passed_event); //chu y
                    tcp.ConfigMessage += new SocketReceivedHandler(passed_config);
                    tcp.Log_Msg += new SocketReceivedHandler(passed_log);
                    tcp.InitClient(ip, port);
                    this.connect_ok = tcp.connect_ok;
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
                    break;
                case CM.TYPECONNECT.HDR_BLUETOOTH:
                    break;
                case CM.TYPECONNECT.HDR_ETHERNET:
                    tcp.Free();
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
                    zigbee.Get_Command_Send(command_type);
                    break;
                case CM.TYPECONNECT.HDR_WIFI:
                    break;
                case CM.TYPECONNECT.HDR_BLUETOOTH:
                    break;
                case CM.TYPECONNECT.HDR_ETHERNET:
                    tcp.Get_Command_Send(command_type);
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
                    zigbee.Set_Command_Send(command_type, user_str);
                    break;
                case CM.TYPECONNECT.HDR_WIFI:
                    break;
                case CM.TYPECONNECT.HDR_BLUETOOTH:
                    break;
                case CM.TYPECONNECT.HDR_ETHERNET:
                    tcp.Set_Command_Send(command_type, user_str);
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
                    zigbee.ReadAsync(command_type);
                    break;
                case CM.TYPECONNECT.HDR_WIFI:
                    break;
                case CM.TYPECONNECT.HDR_BLUETOOTH:
                    break;
                case CM.TYPECONNECT.HDR_ETHERNET:
                    tcp.Receive_Command_Handler(command_type);
                    break;
                case CM.TYPECONNECT.HDR_RS232:
                    break;
                default:
                    break;
            }
        }

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
        private void passed_log(string log_str)
        {
            SocketReceivedHandler get_log = Log_Msg;
            if (get_log != null)
                get_log(log_str);
        }
    }
}
