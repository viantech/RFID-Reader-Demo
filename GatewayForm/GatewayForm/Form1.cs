using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CM = GatewayForm.Common;
using System.Threading;
using System.IO;

namespace GatewayForm
{

    public partial class Form1 : Form
    {

        Communication com_type;
        Zigbee_pop zigbee_form;
        Wifi_pop wifi_form;
        Tcp_pop tcp_form;
        Serial_pop serial_form;
        StringBuilder gateway_config = new StringBuilder();
        byte read_write_bit;
        //string messageLoghandle;
        LogIOData plog; // Declare Database Table
        string[] GW_Format = new string[16] {
                 "Seldatinc gateway configuration=\n",
                 "Gateway serial={0}\n",
                 "Hardware version={0}\n",
                 "Software version={0}\n",
                 "Connection support={0}\n",
                 "Connection using={0}\n",
                 "Audio support={0}\n",
                 "Audio output level={0}dB\n",
                 "Led support={0}\n",
                 "Pallet pattern support={0}\n",
                 "Pallet pattern={0}\n",
                 "Offline mode={0}\n",
                 "RFID API Support LLRP={0}\n",
                 "Message queue time interval={0}\n",
                 "Stack light support={0}\n",
                 "Stack light GPIO={0}"
        };
        public Form1()
        {
            InitializeComponent();
            ConnType_cbx.SelectedIndex = 0;
            zigbee_form = new Zigbee_pop();
            wifi_form = new Wifi_pop();
            tcp_form = new Tcp_pop();
            serial_form = new Serial_pop();
            // create contructor for class LogIOData
            if (plog == null)
            {
                plog = new LogIOData();
                plog.loadData2Table += LoadDatatoTablefromDBbrowser;
                plog.CreateDBTable();
            }

        }
        /*public void startcmdprocess(CM.COMMAND CMD)
        {
            Thread pThreadCmd = new Thread(() => cmdprocess(CMD));
            pThreadCmd.Start();
        }
        public void cmdprocess(CM.COMMAND CMD)
        {
            this.Invoke((MethodInvoker)delegate
            {

                switch (CMD)
                {
                    case CM.COMMAND.CONNECTION_REQUEST_CMD:
                        com_type.Config_Msg += GetConfig_Handler;
                        com_type.Log_Msg += Log_Handler;
                        //com_type.Connect();
                        if (com_type.getflagAccepted())
                            Connected_Behavior();
                        else
                            Log_lb.Text = "Idle";
                        break;
                    case CM.COMMAND.SET_CONN_TYPE_CMD:
                        com_type.Get_Command_Send(CM.COMMAND.DIS_CONNECT_CMD);
                        while (!com_type.getflagConnected_TCPIP()) ;
                        com_type.Close();
                        com_type.Config_Msg -= GetConfig_Handler;
                        com_type.Log_Msg -= Log_Handler;
                        Disconnect_Behavior();
                        com_type.setflagConnected_TCPIP(false);
                        ConnType_cbx.SelectedIndex = Change_conntype_cbx.SelectedIndex;
                        break;
                    case CM.COMMAND.DIS_CONNECT_CMD:
                        com_type.Get_Command_Send(CM.COMMAND.DIS_CONNECT_CMD);
                        while (!com_type.getflagConnected_TCPIP()) ;
                        com_type.Close();
                        com_type.Config_Msg -= GetConfig_Handler;
                        com_type.Log_Msg -= Log_Handler;
                        Disconnect_Behavior();
                        com_type.setflagConnected_TCPIP(false);
                        break;
                    case CM.COMMAND.GET_POWER_CMD:
                        com_type.Get_Command_Power(CM.COMMAND.GET_POWER_CMD, 0);
                        while (!com_type.getflagConnected_TCPIP()) ;
                        com_type.Get_Command_Send(CM.COMMAND.GET_POWER_MODE_CMD);
                        while (!com_type.getflagConnected_TCPIP()) ;
                        com_type.Get_Command_Send(CM.COMMAND.GET_REGION_CMD);
                        while (!com_type.getflagConnected_TCPIP()) ;
                        break;
                    default:
                        break;

                }
            });
        }*/
        private void Connect_btn_Click(object sender, EventArgs e)
        {

            try
            {
                switch (ConnType_cbx.SelectedIndex)
                {
                    //zigbee
                    case 0:
                        if (Connect_btn.Text == "Connect")
                        {
                            com_type = new Communication(CM.TYPECONNECT.HDR_ZIGBEE);
                            com_type.Config_Msg += GetConfig_Handler;
                            com_type.Log_Msg += Log_Handler;
                            com_type.Connect(zigbee_form.ip_add, int.Parse(zigbee_form.port));
                            Connected_Behavior();
                        }
                        else
                        {
                            //startcmdprocess(CM.COMMAND.DIS_CONNECT_CMD);
                            com_type.Get_Command_Send(CM.COMMAND.DIS_CONNECT_CMD);
                            com_type.Close();
                            com_type.Config_Msg -= GetConfig_Handler;
                            com_type.Log_Msg -= Log_Handler;
                            Disconnect_Behavior();
                        }
                        break;
                    //wifi
                    case 1:
                        if (Connect_btn.Text == "Connect")
                        {
                            Log_lb.Text = "Connecting ...";
                            com_type = new Communication(CM.TYPECONNECT.HDR_WIFI);
                            //startcmdprocess(CM.COMMAND.CONNECTION_REQUEST_CMD);
                            com_type.Config_Msg += GetConfig_Handler;
                            com_type.Log_Msg += Log_Handler;
                            com_type.Connect(wifi_form.address, int.Parse(wifi_form.port));
                            if (com_type.getflagConnected_TCPIP())
                                Connected_Behavior();
                            else
                            {
                                Log_lb.Text = "Idle";
                                com_type.Config_Msg -= GetConfig_Handler;
                                com_type.Log_Msg -= Log_Handler;
                            }
                        }
                        else
                        {
                            //startcmdprocess(CM.COMMAND.DIS_CONNECT_CMD);
                            com_type.Get_Command_Send(CM.COMMAND.DIS_CONNECT_CMD);
                            com_type.Receive_Command_Handler(CM.COMMAND.DIS_CONNECT_CMD);
                            com_type.Close();
                            com_type.Config_Msg -= GetConfig_Handler;
                            com_type.Log_Msg -= Log_Handler;
                            Disconnect_Behavior();
                        }
                        break;
                    //bluetooth
                    case 2:
                        break;
                    //Ethernet
                    case 3:
                        if (Connect_btn.Text == "Connect")
                        {
                            Log_lb.Text = "Connecting ...";
                            com_type = new Communication(CM.TYPECONNECT.HDR_ETHERNET);
                            //startcmdprocess(CM.COMMAND.CONNECTION_REQUEST_CMD);
                            com_type.Config_Msg += GetConfig_Handler;
                            com_type.Log_Msg += Log_Handler;
                            com_type.Connect(tcp_form.address, int.Parse(tcp_form.port));
                            if (com_type.getflagConnected_TCPIP())
                                Connected_Behavior();
                            else
                            {
                                Log_lb.Text = "Idle";
                                com_type.Config_Msg -= GetConfig_Handler;
                                com_type.Log_Msg -= Log_Handler;
                            }
                        }
                        else
                        {
                            //startcmdprocess(CM.COMMAND.DIS_CONNECT_CMD);
                            com_type.Get_Command_Send(CM.COMMAND.DIS_CONNECT_CMD);
                            com_type.Receive_Command_Handler(CM.COMMAND.DIS_CONNECT_CMD);
                            com_type.Close();
                            com_type.Config_Msg -= GetConfig_Handler;
                            com_type.Log_Msg -= Log_Handler;
                            Disconnect_Behavior();
                        }
                        break;
                    //RS485
                    case 4:
                        break;
                    default:
                        break;
                }
            }
            catch { }
        }

        private void Connected_Behavior()
        {
            ConnType_cbx.Enabled = false;
            Connect_btn.Text = "Disconnect";
            status_btn.BackColor = Color.Blue;
            status_lb.Text = "Active";
            status_lb.ForeColor = Color.DarkBlue;
        }
        private void Disconnect_Behavior()
        {
            Connect_btn.Text = "Connect";
            status_btn.BackColor = Color.Red;
            status_lb.Text = "Inactive";
            status_lb.ForeColor = SystemColors.ControlDark;
            ConnType_cbx.Enabled = true;
        }
        private void GetConfig_Handler(string config_msg)
        {
            string[] config_str = config_msg.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (config_str[0] == "NAK")
                Log_Handler("Failed Get Config");
            else if (config_str[0] == "Seldatinc gateway configuration=")
            {
                //Gateway serial
                SetControl(Gateway_ID_lb, config_str[1].Substring(config_str[1].IndexOf("=") + 1));
                SetControl(Gateway_ID_tx, config_str[1].Substring(config_str[1].IndexOf("=") + 1));
                //Hardware version
                SetControl(HW_Verrsion_tx, config_str[2].Substring(config_str[2].IndexOf("=") + 1));
                //Software version
                SetControl(SW_Version_tx, config_str[3].Substring(config_str[3].IndexOf("=") + 1));
                //Connection support
                SetControl(ConnectionList_ck, config_str[4].Substring(config_str[4].IndexOf("=") + 1));
                //Audio support
                SetControl(AudioSupport_cbx, config_str[6].Substring(config_str[6].IndexOf("=") + 1));
                SetControl(AudioVolume_trb, config_str[7].Substring(config_str[7].IndexOf("=") + 1));
                //Pallet ID
                SetControl(PatternID_tx, config_str[10].Substring(config_str[10].IndexOf("=") + 1));
                //Message queue time interval
                SetControl(MessageInterval_tx, config_str[13].Substring(config_str[13].IndexOf("=") + 1));
                //Stack Light Support
                if (config_str[14].Contains("yes"))
                    SetControl(StackLight_ckb, "yes");
                else SetControl(StackLight_ckb, "no");
                //Stack Light GPO
                if (config_str[15].Contains("gpo1"))
                    SetControl(GPO1_ckb, "yes");
                else SetControl(GPO1_ckb, "no");
                if (config_str[15].Contains("gpo3"))
                    SetControl(GPO3_ckb, "yes");
                else SetControl(GPO3_ckb, "no");
                if (config_str[15].Contains("gpo4"))
                    SetControl(GPO4_ckb, "yes");
                else SetControl(GPO4_ckb, "no");
                if (config_str[15].Contains("gpo2"))
                    SetControl(GPO2_ckb, "yes");
                else SetControl(GPO2_ckb, "no");
                if (config_str[15].Contains("gpo0"))
                    SetControl(GPO0_ckb, "yes");
                else SetControl(GPO0_ckb, "no");
                //Led support
                if (config_str[8].Contains("yes"))
                    SetControl(LED_Support_ckb, "yes");
                else SetControl(LED_Support_ckb, "no");
                //Pallet support
                if (config_str[9].Contains("yes"))
                    SetControl(PalletSupport_cbx, "yes");
                else SetControl(PalletSupport_cbx, "no");
                //Offline mode
                if (config_str[11].Contains("yes"))
                    SetControl(Offline_ckb, "yes");
                else SetControl(Offline_ckb, "no");
                //RFID API Support
                if (config_str[12].Contains("yes"))
                    SetControl(RFID_API_ckb, "yes");
                else SetControl(RFID_API_ckb, "no");
                Log_Handler("Get GW Config done");
            }
            else if (config_str[0] == "Power RFID")
            {
                if (read_write_bit == 0)
                { SetControl(trackBar2, config_str[1]); Log_Handler("Get Read Power done"); }
                else
                { SetControl(trackBar3, config_str[1]); Log_Handler("Get Write Power done"); }

            }
            else if (config_str[0] == "Region RFID")
            {
                SetControl(region_lst, config_str[1]);
                Log_Handler("Get Region done");
            }
            else if (config_str[0] == "Power Mode RFID")
            {
                SetControl(power_mode_cbx, config_str[1]);
                Log_Handler("Get Power Mode done");
            }
            else if (config_str[0].Contains("= {"))
            {
                MessageBox.Show(config_msg);
                /*if (ConnType_cbx.SelectedIndex == 3)
                    startcmdprocess(CM.COMMAND.SET_CONN_TYPE_CMD);
                else
                {*/
                com_type.Get_Command_Send(CM.COMMAND.DIS_CONNECT_CMD);
                com_type.Receive_Command_Handler(CM.COMMAND.DIS_CONNECT_CMD);
                com_type.Close();
                com_type.Config_Msg -= GetConfig_Handler;
                com_type.Log_Msg -= Log_Handler;
                Disconnect_Behavior();
                ConnType_cbx.SelectedIndex = Change_conntype_cbx.SelectedIndex;
                //}
            }
            else if (config_str[0] == "BLF Setting")
            {
                if (couting == 0)
                {
                    if (config_str[1] == "250")
                        SetControl(freq_cbx, "0");
                    else if (config_str[1] == "320")
                        SetControl(freq_cbx, "1");
                    else
                        SetControl(freq_cbx, "2");
                    couting++;
                }
                else if (couting == 1)
                {
                    SetControl(coding_cbx, config_str[1]);
                    couting++;
                }
                else
                {
                    SetControl(tari_cbx, config_str[1]);
                    couting = 0;
                }

            }
            else
                Log_Handler("Get command not defined");
        }
        int couting = 0;
        private void Log_Handler(string log_msg)
        {
            this.Invoke((MethodInvoker)delegate
            {
                Log_lb.Text = log_msg;
                //messageLoghandle = log_msg;
                if (Log_lb.Text != "Inventory Mode")
                {
                    ptimer_loghandle.Interval = 3000;
                    ptimer_loghandle.Start();
                }
            });
        }

        private delegate void SetConfigDelegate(Control control, string config_tx);
        private void SetControl(Control control, string config_tx)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new SetConfigDelegate(SetControl), control, config_tx);
            }
            else
            {
                switch (control.GetType().Name)
                {
                    case "TextBox":
                        control.Text = config_tx;
                        break;

                    case "Label":
                        control.Text = config_tx;
                        break;

                    case "CheckBox":
                        if ("yes" == config_tx)
                            (control as CheckBox).Checked = true;
                        else
                            (control as CheckBox).Checked = false;
                        break;

                    case "RadioButton":
                        if ("yes" == config_tx)
                            (control as RadioButton).Checked = true;
                        else
                            (control as RadioButton).Checked = false;
                        break;

                    case "CheckedListBox":
                        for (int i = 0; i < (control as CheckedListBox).Items.Count; i++)
                            ConnectionList_ck.SetItemCheckState(i, CheckState.Checked);

                        if (!config_tx.Contains("Zigbee"))
                            (control as CheckedListBox).SetItemCheckState(0, CheckState.Unchecked);
                        if (!config_tx.Contains("Wifi"))
                            (control as CheckedListBox).SetItemCheckState(1, CheckState.Unchecked);
                        if (!config_tx.Contains("Bluetooth"))
                            (control as CheckedListBox).SetItemCheckState(2, CheckState.Unchecked);
                        if (!config_tx.Contains("Ethernet"))
                            (control as CheckedListBox).SetItemCheckState(3, CheckState.Unchecked);
                        if (!config_tx.Contains("RS485"))
                            (control as CheckedListBox).SetItemCheckState(4, CheckState.Unchecked);
                        break;

                    case "TrackBar":
                        if ((control as TrackBar).Name.Contains("trackBar"))
                        {
                            (control as TrackBar).Value = int.Parse(config_tx);
                        }
                        else if ((control as TrackBar).Name == "AudioVolume_trb")
                        {
                            (control as TrackBar).Value = int.Parse(config_tx.Remove(config_tx.Length - 2));
                        }
                        break;

                    case "Button":
                        if ("Failed" == config_tx)
                            (control as Button).BackColor = Color.Red;
                        else
                            (control as Button).BackColor = Color.Blue;
                        break;
                    case "ComboBox":
                        //if (control.Name == "region_lst")
                        (control as ComboBox).SelectedIndex = int.Parse(config_tx);
                        //else if (control.Name == "power_mode_cbx")
                        break;
                    default:
                        break;
                }

            }
        }
        private void Read_handler(string msg)// open and save
        {
            SetText(this.dataGridView1, msg);
        }

        private delegate void SetTextDelegate(DataGridView table, string text);
        private void SetText(DataGridView table, string text)
        {
            if (table.InvokeRequired)
            {
                table.Invoke(new SetTextDelegate(SetText), table, text);
            }
            else
            {
                string[] rows;
                table.Rows.Clear();
                //string[] seperators = new string[] { "EPC:", "ANT:", "RSSI:", "Read Count:", "Date:" };
                rows = text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

                rows = rows.Skip(5).ToArray();
                plog.InsertData2Sql(rows);
                for (int i = 0; i < rows.Length; i++)
                {
                    string[] cells;
                    cells = rows[i].Split(new string[] { "\t" }, StringSplitOptions.None);
                    table.Rows.Add(cells[0], cells[1], cells[2], cells[3], cells[4]);
                }

            }
        }

        private void Start_Operate_btn_Click(object sender, EventArgs e)
        {
            if (Start_Operate_btn.Text == "Start inventory")
            {
                com_type.TagID_Msg += Read_handler;
                com_type.Get_Command_Send(CM.COMMAND.START_OPERATION_CMD);
                com_type.Receive_Command_Handler(CM.COMMAND.START_OPERATION_CMD);
                Stop_Behavior();
            }
            else
            {
                com_type.TagID_Msg -= Read_handler;
                com_type.Get_Command_Send(CM.COMMAND.STOP_OPERATION_CMD);
                Start_Behavior();
            }
        }

        private void Stop_Behavior()
        {
            Start_Operate_btn.Text = "Stop inventory";

            Set_GW_Config_btn.Enabled = false;
            Get_GW_Config_btn.Enabled = false;
            set_newconn_btn.Enabled = false;
            Get_RFID_btn.Enabled = false;
            Set_RFID_btn.Enabled = false;
            Connect_btn.Enabled = false;

            foreach (Button comm_btn in this.groupBox8.Controls.OfType<Button>())
            {
                comm_btn.Enabled = false;
            }
            foreach (Button comm_btn in this.groupBox9.Controls.OfType<Button>())
            {
                comm_btn.Enabled = false;
            }
            foreach (Button comm_btn in this.groupBox11.Controls.OfType<Button>())
            {
                comm_btn.Enabled = false;
            }
        }

        private void Start_Behavior()
        {
            Start_Operate_btn.Text = "Start inventory";

            Set_GW_Config_btn.Enabled = true;
            Get_GW_Config_btn.Enabled = true;
            set_newconn_btn.Enabled = true;
            Get_RFID_btn.Enabled = true;
            Set_RFID_btn.Enabled = true;
            Connect_btn.Enabled = true;

            foreach (Button comm_btn in this.groupBox8.Controls.OfType<Button>())
            {
                comm_btn.Enabled = true;
            }
            foreach (Button comm_btn in this.groupBox9.Controls.OfType<Button>())
            {
                comm_btn.Enabled = true;
            }
            foreach (Button comm_btn in this.groupBox11.Controls.OfType<Button>())
            {
                comm_btn.Enabled = true;
            }
            this.dataGridView1.Rows.Clear();
            this.No_Tag_lb.Text = "0";
        }

        private void Get_GW_Config_btn_Click(object sender, EventArgs e)
        {
            if (com_type.getflagConnected_TCPIP())
            {
                com_type.Get_Command_Send(CM.COMMAND.GET_CONFIGURATION_CMD);
                com_type.Receive_Command_Handler(CM.COMMAND.GET_CONFIGURATION_CMD);
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!");
                Disconnect_Behavior();
            }
        }

        private void Set_GW_Config_btn_Click(object sender, EventArgs e)
        {
            if (com_type.getflagConnected_TCPIP())
            {
                gateway_config.Clear();
                gateway_config.Append(GW_Format[0]);
                gateway_config.AppendFormat(GW_Format[1], Gateway_ID_tx.Text);
                gateway_config.AppendFormat(GW_Format[2], HW_Verrsion_tx.Text);
                gateway_config.AppendFormat(GW_Format[3], SW_Version_tx.Text);
                string connections = String.Empty;
                foreach (var item_conn in ConnectionList_ck.CheckedItems)
                    connections += item_conn.ToString() + ",";
                gateway_config.AppendFormat(GW_Format[4], connections.Remove(connections.Length - 1));
                gateway_config.AppendFormat(GW_Format[5], ConnType_cbx.SelectedItem.ToString());
                if (AudioSupport_cbx.Checked)
                    gateway_config.AppendFormat(GW_Format[6], "yes");
                else gateway_config.AppendFormat(GW_Format[6], "no");
                gateway_config.AppendFormat(GW_Format[7], AudioVolume_trb.Value.ToString());
                if (LED_Support_ckb.Checked)
                    gateway_config.AppendFormat(GW_Format[8], "yes");
                else gateway_config.AppendFormat(GW_Format[8], "no");
                if (PalletSupport_cbx.Checked)
                    gateway_config.AppendFormat(GW_Format[9], "yes");
                else gateway_config.AppendFormat(GW_Format[9], "no");
                gateway_config.AppendFormat(GW_Format[10], PatternID_tx.Text);
                if (Offline_ckb.Checked)
                    gateway_config.AppendFormat(GW_Format[11], "yes");
                else gateway_config.AppendFormat(GW_Format[11], "no");
                if (RFID_API_ckb.Checked)
                    gateway_config.AppendFormat(GW_Format[12], "yes");
                else gateway_config.AppendFormat(GW_Format[12], "no");
                gateway_config.AppendFormat(GW_Format[13], MessageInterval_tx.Text);
                if (StackLight_ckb.Checked)
                    gateway_config.AppendFormat(GW_Format[14], "yes");
                else gateway_config.AppendFormat(GW_Format[14], "no");
                string GPO_sets = String.Empty;
                foreach (CheckBox GPOs_ckb in groupBox7.Controls.OfType<CheckBox>())
                {
                    if (GPOs_ckb.Checked)
                        GPO_sets += GPOs_ckb.Text.ToLower() + ",";
                }
                gateway_config.AppendFormat(GW_Format[15], GPO_sets.Remove(GPO_sets.Length - 1));
                com_type.Set_Command_Send(CM.COMMAND.SET_CONFIGURATION_CMD, gateway_config.ToString());
                com_type.Receive_Command_Handler(CM.COMMAND.SET_CONFIGURATION_CMD);
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!");
                Disconnect_Behavior();
            }
        }

        private void dataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            No_Tag_lb.Text = this.dataGridView1.RowCount.ToString();
            if (this.dataGridView1.Rows.Count > 0)
            {
                switch (int.Parse(this.dataGridView1.Rows[this.dataGridView1.Rows.Count - 1].Cells[1].Value.ToString()))
                {
                    case 1: ANT1_Rbtn.Checked = true;
                        break;
                    case 2: ANT2_Rbtn.Checked = true;
                        break;
                    case 3: ANT3_Rbtn.Checked = true;
                        break;
                    case 4: ANT4_Rbtn.Checked = true;
                        break;
                    default:
                        break;
                }
            }
        }

        private void ViewConn_btn_Click(object sender, EventArgs e)
        {
            switch (ConnType_cbx.SelectedIndex)
            {
                case 0:
                    zigbee_form.ShowDialog();
                    break;
                case 1:
                    wifi_form.ShowDialog();
                    break;
                case 2:
                    break;
                case 3:
                    tcp_form.ShowDialog();
                    break;
                case 4:
                    serial_form.ShowDialog();
                    break;
                default:
                    break;
            }
        }

        private void Get_RFID_btn_Click(object sender, EventArgs e)
        {
            if (com_type.getflagConnected_TCPIP())
            {
                com_type.Get_Command_Send(CM.COMMAND.GET_RFID_CONFIGURATION_CMD);
                com_type.Receive_Command_Handler(CM.COMMAND.GET_RFID_CONFIGURATION_CMD);
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!");
                Disconnect_Behavior();
            }
        }

        private void ConnType_cbx_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (ConnType_cbx.SelectedIndex)
            {
                case 0:
                    zigbee_form.ShowDialog();
                    break;
                case 1:
                    wifi_form.ShowDialog();
                    break;
                case 2:
                    break;
                case 3:
                    tcp_form.ShowDialog();
                    break;
                case 4:
                    serial_form.ShowDialog();
                    break;
                default:
                    break;
            }
        }

        private void get_power_btn_Click(object sender, EventArgs e)
        {
            if (com_type.getflagConnected_TCPIP())
            {
                read_write_bit = 0;
                com_type.Get_Command_Power(CM.COMMAND.GET_POWER_CMD, read_write_bit);
                com_type.Receive_Command_Handler(CM.COMMAND.GET_POWER_CMD);
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!");
                Disconnect_Behavior();
            }
        }

        private void set_power_btn_Click(object sender, EventArgs e)
        {
            if (com_type.getflagConnected_TCPIP())
            {
                //Log_lb.Text = "Sending...";
                byte[] power_bytes = new byte[2];
                power_bytes[0] = 0;
                power_bytes[1] = (byte)trackBar2.Value;
                com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_POWER_CMD, power_bytes);
                com_type.Receive_Command_Handler(CM.COMMAND.SET_POWER_CMD);
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!");
                Disconnect_Behavior();
            }
        }

        private void get_write_power_btn_Click(object sender, EventArgs e)
        {
            if (com_type.getflagConnected_TCPIP())
            {
                read_write_bit = 1;
                com_type.Get_Command_Power(CM.COMMAND.GET_POWER_CMD, read_write_bit);
                com_type.Receive_Command_Handler(CM.COMMAND.GET_POWER_CMD);
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!");
                Disconnect_Behavior();
            }
        }

        private void set_write_power_btn_Click(object sender, EventArgs e)
        {
            if (com_type.getflagConnected_TCPIP())
            {
                byte[] power_bytes = new byte[2];
                power_bytes[0] = 1;
                power_bytes[1] = (byte)trackBar3.Value;
                com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_POWER_CMD, power_bytes);
                com_type.Receive_Command_Handler(CM.COMMAND.SET_POWER_CMD);
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!");
                Disconnect_Behavior();
            }
        }

        private void get_region_btn_Click(object sender, EventArgs e)
        {
            if (com_type.getflagConnected_TCPIP())
            {
                com_type.Get_Command_Send(CM.COMMAND.GET_REGION_CMD);
                com_type.Receive_Command_Handler(CM.COMMAND.GET_REGION_CMD);
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!");
                Disconnect_Behavior();
            }
        }

        private void set_region_btn_Click(object sender, EventArgs e)
        {
            if (com_type.getflagConnected_TCPIP())
            {
                byte[] region_byte = new byte[1];
                region_byte[0] = (byte)region_lst.SelectedIndex;
                com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_REGION_CMD, region_byte);
                com_type.Receive_Command_Handler(CM.COMMAND.SET_REGION_CMD);
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!");
                Disconnect_Behavior();
            }
        }

        private void trackBar2_ValueChanged(object sender, EventArgs e)
        {
            read_power_lb.Text = trackBar2.Value.ToString();
        }

        private void trackBar3_ValueChanged(object sender, EventArgs e)
        {
            write_power_lb.Text = trackBar3.Value.ToString();
        }

        private void AudioVolume_trb_ValueChanged(object sender, EventArgs e)
        {
            Audio_val.Text = AudioVolume_trb.Value.ToString();
        }

        private void get_power_mode_btn_Click(object sender, EventArgs e)
        {
            if (com_type.getflagConnected_TCPIP())
            {
                com_type.Get_Command_Send(CM.COMMAND.GET_POWER_MODE_CMD);
                com_type.Receive_Command_Handler(CM.COMMAND.GET_POWER_MODE_CMD);
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!");
                Disconnect_Behavior();
            }
        }

        private void set_power_mode_btn_Click(object sender, EventArgs e)
        {
            if (com_type.getflagConnected_TCPIP())
            {
                byte[] pw_mode_byte = new byte[1];
                pw_mode_byte[0] = (byte)power_mode_cbx.SelectedIndex;
                com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_POWER_MODE_CMD, pw_mode_byte);
                com_type.Receive_Command_Handler(CM.COMMAND.SET_POWER_MODE_CMD);
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!");
                Disconnect_Behavior();
            }
        }

        private void AudioSupport_cbx_CheckedChanged(object sender, EventArgs e)
        {
            if (!AudioSupport_cbx.Checked)
                AudioVolume_trb.Enabled = false;
            else
                AudioVolume_trb.Enabled = true;
        }

        private void PalletSupport_cbx_CheckedChanged(object sender, EventArgs e)
        {
            if (!PalletSupport_cbx.Checked)
                PatternID_tx.Enabled = false;
            else
                PatternID_tx.Enabled = true;
        }

        private void set_newconn_btn_Click(object sender, EventArgs e)
        {
            if (com_type.getflagConnected_TCPIP())
            {
                byte[] conn_type_byte = new byte[1];
                conn_type_byte[0] = (byte)Change_conntype_cbx.SelectedIndex;
                com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_CONN_TYPE_CMD, conn_type_byte);
                com_type.Receive_Command_Handler(CM.COMMAND.SET_CONN_TYPE_CMD);
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!");
                Disconnect_Behavior();
            }
        }

        private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (tabControl1.SelectedIndex == 2)
            {
                if (trackBar2.Value == 5 && Start_Operate_btn.Text == "Start inventory" && Connect_btn.Text == "Disconnect")
                {
                    //com_type.RFID_Process();
                    /*if (ConnType_cbx.SelectedIndex == 3)
                        startcmdprocess(CM.COMMAND.GET_POWER_CMD);
                    else //if (ConnType_cbx.SelectedIndex == 1)
                    {*/
                    if (com_type.getflagConnected_TCPIP())
                    {
                        read_write_bit = 0;
                        com_type.Get_Command_Power(CM.COMMAND.GET_POWER_CMD, read_write_bit);
                        com_type.Receive_Command_Handler(CM.COMMAND.GET_POWER_CMD);

                        read_write_bit = 1;
                        com_type.Get_Command_Power(CM.COMMAND.GET_POWER_CMD, read_write_bit);
                        com_type.Receive_Command_Handler(CM.COMMAND.GET_POWER_CMD);

                        com_type.Get_Command_Send(CM.COMMAND.GET_POWER_MODE_CMD);
                        com_type.Receive_Command_Handler(CM.COMMAND.GET_POWER_MODE_CMD);
                    }
                    else
                    {
                        MessageBox.Show("Connection was disconnected\nPlease connect again!");
                        Disconnect_Behavior();
                    }
                    //com_type.Get_Command_Send(CM.COMMAND.GET_REGION_CMD);
                    //com_type.Receive_Command_Handler(CM.COMMAND.GET_REGION_CMD);
                    //}
                }
            }
        }
        private void LoadDatatoTablefromDBbrowser(String[] mgs)
        {
            Table_dbbrowser_datagrid.Rows.Add(mgs[0], mgs[1], mgs[2], mgs[3], mgs[4]);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string startupPath = System.IO.Directory.GetCurrentDirectory();
            MessageBox.Show(startupPath + Properties.Resources.SELDAT_DATABASE);
            //  var folder = Directory.CreateDirectory("E://luatga"); 
            // returns a DirectoryInfo object
            // FolderBrowserDialog st_currentpath = new FolderBrowserDialog();
            // st_currentpath.ShowDialog();
            //txt_dbbrowser_selectedpath.Text = st_currentpath.SelectedPath;
            //MessageBox.Show(pp.SelectedPath);

        }
        private void chk_dbbrowser_currentpath_CheckedChanged_1(object sender, EventArgs e)
        {

        }
        private void btn_downloadtabletoExcel_Click(object sender, EventArgs e)
        {
            if (plog != null)
            {
                plog.DownloadExelFile();
            }
        }

        private void btn_dbbrowser_search_Click(object sender, EventArgs e)
        {
            if (plog != null)
            {

                SearchForm_Data psearchform = new SearchForm_Data();
                psearchform.loaddatasearch += LoadDataSearch;
                psearchform.Show();

            }
        }
        public void LoadDataSearch(String field, String data)
        {
            if (plog != null)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    plog.SearchDataINSql(field, data);
                });
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void Set_RFID_btn_Click(object sender, EventArgs e)
        {

        }
        private void ptimer_loghandle_Tick_1(object sender, EventArgs e)
        {
            //SetControl(Log_lb, messageLoghandle);
            //Log_lb.Text = messageLoghandle;
            /*if (messageLoghandle == "No Connection")
            {
                SetControl(status_lb, "Inactive");
                SetControl(status_btn, "Failed");
            }
            else if (messageLoghandle == "Connected")
            {
                SetControl(status_lb, "Active");
                SetControl(status_btn, "True");
            }*/
            if (Start_Operate_btn.Text == "Stop inventory")
                Log_lb.Text = "Inventory Mode";
            if (Log_lb.Text == "Disconnected")
            {
                Log_lb.Text = "Idle";
                if (status_lb.Text == "Active")
                    Disconnect_Behavior();
            }
            else
                Log_lb.Text = "Ready!";
            ptimer_loghandle.Stop();
        }

        private void get_protocol_btn_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("In Development.\nWait for the next version.");
            com_type.Get_Command_Power(CM.COMMAND.GET_BLF_CMD, 0);
            com_type.Receive_Command_Handler(CM.COMMAND.GET_BLF_CMD);

            com_type.Get_Command_Power(CM.COMMAND.GET_BLF_CMD, 1);
            com_type.Receive_Command_Handler(CM.COMMAND.GET_BLF_CMD);

            com_type.Get_Command_Power(CM.COMMAND.GET_BLF_CMD, 2);
            com_type.Receive_Command_Handler(CM.COMMAND.GET_BLF_CMD);
        }

        private void set_protocol_btn_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("In Development.\nWait for the next version.");
            byte[] freq_bytes = new byte[2];
            freq_bytes[0] = 0;
            freq_bytes[1] = (byte)(2 * freq_cbx.SelectedIndex);
            com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_BLF_CMD, freq_bytes);
            com_type.Receive_Command_Handler(CM.COMMAND.SET_BLF_CMD);

            //freq_bytes = new byte[2];
            freq_bytes[0] = 1;
            freq_bytes[1] = (byte)(coding_cbx.SelectedIndex);
            com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_BLF_CMD, freq_bytes);
            com_type.Receive_Command_Handler(CM.COMMAND.SET_BLF_CMD);

            freq_bytes[0] = 2;
            freq_bytes[1] = (byte)(tari_cbx.SelectedIndex);
            com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_BLF_CMD, freq_bytes);
            com_type.Receive_Command_Handler(CM.COMMAND.SET_BLF_CMD);
        }
    }
}
