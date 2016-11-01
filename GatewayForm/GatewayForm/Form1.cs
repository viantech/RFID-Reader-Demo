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
        string[] GW_Format = new string[16] {
                 "Seldatinc gateway configuration:\n",
                 "Gateway serial:{0}\n",
                 "Hardware version:{0}\n",
                 "Software version:{0}\n",
                 "Connection support:{0}\n",
                 "Connection using:{0}\n",
                 "Audio support:{0}\n",
                 "Audio output level:{0}\n",
                 "Led support:yes\n",
                 "Pallet pattern support:{0}\n",
                 "Pallet pattern:{0}\n",
                 "Offline mode:{0}\n",
                 "RFID API Support LLRP:{0}\n",
                 "Message queue time interval:{0}\n",
                 "Stack light support:{0}\n",
                 "Stacl light GPIO:{0}"
        };
        public Form1()
        {
            InitializeComponent();
            ConnType_cbx.SelectedIndex = 0;
            zigbee_form = new Zigbee_pop();
            wifi_form = new Wifi_pop();
            tcp_form = new Tcp_pop();
            serial_form = new Serial_pop();
        }

        private void Connect_btn_Click(object sender, EventArgs e)
        {
            switch (ConnType_cbx.SelectedIndex)
            {
                //zigbee
                case 0:
                    if (Connect_btn.Text == "Connect")
                    {
                        com_type = new Communication(CM.TYPECONNECT.HDR_ZIGBEE);
                        com_type.Config_Msg += new SocketReceivedHandler(GetConfig_Handler);
                        com_type.Log_Msg += new SocketReceivedHandler(Log_Handler);
                        com_type.Connect(zigbee_form.ip_add, int.Parse(zigbee_form.port));
                        Connected_Behavior();
                    }
                    else if (Connect_btn.Text == "Disconnect")
                    {
                        com_type.Get_Command_Send(CM.COMMAND.DIS_CONNECT_CMD);
                        com_type.Receive_Command_Handler(CM.COMMAND.DIS_CONNECT_CMD);
                        com_type.Close();
                        Disconnect_Behavior();
                    }
                    break;
                //wifi
                case 1: MessageBox.Show("Still develop");
                    break;
                //bluetooth
                case 2:
                    break;
                //Ethernet
                case 3:
                    if (Connect_btn.Text == "Connect")
                    {
                        com_type = new Communication(CM.TYPECONNECT.HDR_ETHERNET);
                        com_type.Config_Msg += new SocketReceivedHandler(GetConfig_Handler);
                        com_type.Log_Msg += new SocketReceivedHandler(Log_Handler);
                        com_type.Connect(tcp_form.address, int.Parse(tcp_form.port));
                        if (com_type.connect_ok)
                            Connected_Behavior();
                    }
                    else if (Connect_btn.Text == "Disconnect")
                    {
                        com_type.Get_Command_Send(CM.COMMAND.DIS_CONNECT_CMD);
                        com_type.Receive_Command_Handler(CM.COMMAND.DIS_CONNECT_CMD);
                        com_type.Close();
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
            if (config_str[0].Contains("Seldatinc gateway "))
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
                SetControl(AudioSupport_rbtn, config_str[6].Substring(config_str[6].IndexOf("=") + 1));
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
                if (config_str[15].Contains("gpo0"))
                    SetControl(GPO0_ckb, "yes");
                else SetControl(GPO0_ckb, "no");
                if (config_str[15].Contains("gpo1"))
                    SetControl(GPO1_ckb, "yes");
                else SetControl(GPO1_ckb, "no");
                if (config_str[15].Contains("gpo2"))
                    SetControl(GPO2_ckb, "yes");
                else SetControl(GPO2_ckb, "no");
                if (config_str[15].Contains("gpo3"))
                    SetControl(GPO3_ckb, "yes");
                else SetControl(GPO3_ckb, "no");
                if (config_str[15].Contains("gpo4"))
                    SetControl(GPO4_ckb, "yes");
                else SetControl(GPO4_ckb, "no");
                //Led support
                if (config_str[8].Contains("yes"))
                    SetControl(LED_Support_ckb, "yes");
                else SetControl(LED_Support_ckb, "no");
                //Pallet support
                if (config_str[9].Contains("yes"))
                    SetControl(PalletSupport_rbtn, "yes");
                else SetControl(PalletSupport_rbtn, "no");
                //Offline mode
                if (config_str[11].Contains("yes"))
                    SetControl(Offline_ckb, "yes");
                else SetControl(Offline_ckb, "no");
                //RFID API Support
                if (config_str[12].Contains("yes"))
                    SetControl(RFID_API_ckb, "yes");
                else SetControl(RFID_API_ckb, "no");
            }
            else if (config_str[0].Contains("Power RFID"))
            {
                SetControl(trackBar2, config_str[1].Substring(config_str[1].IndexOf("=") + 1));
            }
            else if (config_str[0].Contains("Region RFID"))
            {
                SetControl(region_lst, config_str[1].Substring(config_str[1].IndexOf("=") + 1));
            }
        }

        private void Log_Handler(string log_msg)
        {
            SetControl(Log_lb, log_msg);
            if (log_msg == "No Connection")
            {
                SetControl(status_lb, "Inactive");
                SetControl(status_btn, "Failed");
            }
            else if (log_msg == "Connected")
            {
                SetControl(status_lb, "Active");
                SetControl(status_btn, "True");
            }
        }

        private delegate void SetConfigDelegate(Control control, string config_tx);
        public void SetControl(Control control, string config_tx)
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
                        if ((control as TrackBar).Name == "trackBar2")
                        {
                            (control as TrackBar).Value = int.Parse(config_tx) / 100;
                        }
                        else if ((control as TrackBar).Name == "AudioVolume_trb")
                        {
                            (control as TrackBar).Value = int.Parse(config_tx);
                        }
                        break;

                    case "Button":
                        if ("Failed" == config_tx)
                            (control as Button).BackColor = Color.Red;
                        else
                            (control as Button).BackColor = Color.Blue;
                        break;
                    case "ComboBox":
                        (control as ComboBox).SelectedIndex = int.Parse(config_tx);
                        break;
                    default:
                        break;
                }

            }
        }
        private void Read_handler(string msg)
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
                rows = rows.Skip(2).ToArray();

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
                com_type.TagID_Msg += new SocketReceivedHandler(Read_handler);
                com_type.Get_Command_Send(CM.COMMAND.START_OPERATION_CMD);
                com_type.Receive_Command_Handler(CM.COMMAND.START_OPERATION_CMD);
                Start_Operate_btn.Text = "Stop inventory";
            }
            else if (Start_Operate_btn.Text == "Stop inventory")
            {
                com_type.TagID_Msg -= new SocketReceivedHandler(Read_handler);
                com_type.Get_Command_Send(CM.COMMAND.STOP_OPERATION_CMD);
                Start_Operate_btn.Text = "Start inventory";
                this.dataGridView1.Rows.Clear();
            }
        }

        private void Get_GW_Config_btn_Click(object sender, EventArgs e)
        {
            com_type.Get_Command_Send(CM.COMMAND.GET_CONFIGURATION_CMD);
            com_type.Receive_Command_Handler(CM.COMMAND.GET_CONFIGURATION_CMD);
        }

        private void Set_GW_Config_btn_Click(object sender, EventArgs e)
        {
            gateway_config.Append(GW_Format[0]);
            gateway_config.AppendFormat(GW_Format[1], Gateway_ID_tx.Text);
            gateway_config.AppendFormat(GW_Format[2], HW_Verrsion_tx.Text);
            gateway_config.AppendFormat(GW_Format[3], SW_Version_tx.Text);
            string connections = String.Empty;
            foreach (var item_conn in ConnectionList_ck.CheckedItems)
                connections += item_conn.ToString() + ",";
            gateway_config.AppendFormat(GW_Format[4], connections.Remove(connections.Length - 1));
            gateway_config.AppendFormat(GW_Format[5], ConnType_cbx.SelectedItem.ToString());
            if (AudioSupport_rbtn.Checked)
                gateway_config.AppendFormat(GW_Format[6], "yes");
            else gateway_config.AppendFormat(GW_Format[6], "no");
            gateway_config.AppendFormat(GW_Format[7] + "dB", AudioVolume_trb.Value.ToString());
            if (LED_Support_ckb.Checked)
                gateway_config.AppendFormat(GW_Format[8], "yes");
            else gateway_config.AppendFormat(GW_Format[8], "no");
            if (PalletSupport_rbtn.Checked)
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
            foreach (CheckBox GPOs_ckb in groupBox7.Controls)
            {
                if (GPOs_ckb.Checked)
                    GPO_sets += GPOs_ckb.Text.ToLower() + ",";
            }
            gateway_config.AppendFormat(GW_Format[15], GPO_sets.Remove(GPO_sets.Length - 1));
            com_type.Set_Command_Send(CM.COMMAND.SET_CONFIGURATION_CMD, gateway_config.ToString());
            com_type.Receive_Command_Handler(CM.COMMAND.SET_CONFIGURATION_CMD);
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
            com_type.Get_Command_Send(CM.COMMAND.GET_RFID_CONFIGURATION_CMD);
            com_type.Receive_Command_Handler(CM.COMMAND.GET_RFID_CONFIGURATION_CMD);
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

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            read_power_lb.Text = trackBar2.Value.ToString();
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            write_power_lb.Text = trackBar3.Value.ToString();
        }

        private void AudioVolume_trb_Scroll(object sender, EventArgs e)
        {
            Audio_val.Text = AudioVolume_trb.Value.ToString();
        }

        private void get_power_btn_Click(object sender, EventArgs e)
        {
            com_type.Get_Command_Send(CM.COMMAND.GET_POWER_CMD);
            com_type.Receive_Command_Handler(CM.COMMAND.GET_POWER_CMD);
        }

        private void set_power_btn_Click(object sender, EventArgs e)
        {
            string powerconfig = "/reader/radio/readPower = " + 100 * trackBar2.Value;
            com_type.Set_Command_Send(CM.COMMAND.SET_POWER_CMD, powerconfig);
            com_type.Receive_Command_Handler(CM.COMMAND.SET_POWER_CMD);
        }

        private void get_region_btn_Click(object sender, EventArgs e)
        {
            com_type.Get_Command_Send(CM.COMMAND.GET_REGION_CMD);
            com_type.Receive_Command_Handler(CM.COMMAND.GET_REGION_CMD);
        }

        private void set_region_btn_Click(object sender, EventArgs e)
        {
            string regionconfig = "/reader/region/hopTable = 0" + region_lst.SelectedIndex.ToString();
            com_type.Set_Command_Send(CM.COMMAND.SET_POWER_CMD, regionconfig);
            com_type.Receive_Command_Handler(CM.COMMAND.SET_POWER_CMD);
        }

    }
}
