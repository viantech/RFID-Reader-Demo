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
        List<string> list_plan_name = new List<string>();
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
            CM.ConfigMessage += GetConfig_Handler;
            CM.Log_Msg += Log_Handler;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (com_type != null && com_type.getflagConnected_TCPIP())
                com_type.Close();
            zigbee_form.Dispose();
            wifi_form.Dispose();
            tcp_form.Dispose();
            serial_form.Dispose();
            CM.ConfigMessage -= GetConfig_Handler;
            CM.Log_Msg -= Log_Handler;
            plog.loadData2Table -= LoadDatatoTablefromDBbrowser;
        }
        private void startcmdprocess(CM.COMMAND CMD)
        {
            Thread pThreadCmd = new Thread(() => cmdprocess(CMD));
            pThreadCmd.Start();
        }
        private void cmdprocess(CM.COMMAND CMD)
        {
            this.Invoke((MethodInvoker)delegate
            {
                switch (CMD)
                {
                    case CM.COMMAND.DIS_CONNECT_CMD:
                        com_type.Get_Command_Send(CM.COMMAND.DIS_CONNECT_CMD);
                        //com_type.waitflagRevTCP();
                        com_type.Close();
                        Disconnect_Behavior();
                        break;
                    case CM.COMMAND.SET_CONN_TYPE_CMD:
                        if (Change_conntype_cbx.SelectedIndex == 1)
                            com_type.Get_Command_Power(CM.COMMAND.REBOOT_CMD, 2);
                        else
                            com_type.Get_Command_Power(CM.COMMAND.REBOOT_CMD, 1);
                        com_type.waitflagRevTCP();
                        com_type.Close();
                        Disconnect_Behavior();
                        ConnType_cbx.SelectedIndex = Change_conntype_cbx.SelectedIndex;
                        break;
                    case CM.COMMAND.SET_POWER_CMD:
                        byte[] power_bytes = new byte[2];
                        power_bytes[0] = 0;
                        power_bytes[1] = (byte)trackBar2.Value;
                        com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_POWER_CMD, power_bytes);
                        com_type.waitflagRevTCP();

                        power_bytes[0] = 1;
                        power_bytes[1] = (byte)trackBar3.Value;
                        com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_POWER_CMD, power_bytes);
                        com_type.waitflagRevTCP();
                        break;
                    case CM.COMMAND.GET_POWER_MODE_CMD:
                        com_type.Get_Command_Send(CM.COMMAND.GET_POWER_MODE_CMD);
                        com_type.waitflagRevTCP();
                        break;
                    case CM.COMMAND.SET_BLF_CMD:
                        byte[] freq_bytes = new byte[2];
                        freq_bytes[0] = 0;
                        freq_bytes[1] = (byte)(2 * freq_cbx.SelectedIndex);
                        com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_BLF_CMD, freq_bytes);
                        com_type.waitflagRevTCP();

                        freq_bytes[0] = 1;
                        freq_bytes[1] = (byte)(coding_cbx.SelectedIndex);
                        com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_BLF_CMD, freq_bytes);
                        com_type.waitflagRevTCP();

                        freq_bytes[0] = 2;
                        freq_bytes[1] = (byte)(tari_cbx.SelectedIndex);
                        com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_BLF_CMD, freq_bytes);
                        com_type.waitflagRevTCP();
                        break;
                    case CM.COMMAND.CHECK_READER_STT_CMD:
                        com_type.Close();
                        Disconnect_Behavior();
                        DialogResult result = MessageBox.Show("The connection closed. Please check your connection.\nIf you want re-connect, Click \"Yes\"",
                                                              "Confirmation", MessageBoxButtons.YesNo);
                        if (result == DialogResult.Yes)
                        {
                            Connect_btn.PerformClick();
                        }
                        else
                        {
                            //no...
                        }
                        break;
                    default:
                        break;

                }
            });
        }
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
                            Log_lb.Text = "Connecting ...";
                            com_type = new Communication(CM.TYPECONNECT.HDR_ZIGBEE);

                            com_type.Connect(zigbee_form.ip_add, int.Parse(zigbee_form.port));
                            Connected_Behavior();
                        }
                        else
                        {
                            com_type.Get_Command_Send(CM.COMMAND.DIS_CONNECT_CMD);
                            com_type.Close();
                            Disconnect_Behavior();
                        }
                        break;
                    //wifi
                    case 1:
                        if (Connect_btn.Text == "Connect")
                        {
                            Log_lb.Text = "Connecting ...";
                            com_type = new Communication(CM.TYPECONNECT.HDR_WIFI);
                            com_type.Connect(wifi_form.address, int.Parse(wifi_form.port));
                            if (com_type.getflagConnected_TCPIP())
                                Connected_Behavior();
                            else
                            {
                                Log_lb.Text = "Idle";
                            }
                        }
                        else
                        {
                            startcmdprocess(CM.COMMAND.DIS_CONNECT_CMD);
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
                            com_type.Connect(tcp_form.address, int.Parse(tcp_form.port));
                            if (com_type.getflagConnected_TCPIP())
                                Connected_Behavior();
                            else
                            {
                                Log_lb.Text = "Idle";
                            }
                        }
                        else
                        {
                            startcmdprocess(CM.COMMAND.DIS_CONNECT_CMD);
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
            ViewConn_btn.Text = "View Port";
            ViewConn_btn.FlatStyle = FlatStyle.Flat;
        }
        private void Disconnect_Behavior()
        {
            Connect_btn.Text = "Connect";
            status_btn.BackColor = Color.Red;
            status_lb.Text = "Inactive";
            status_lb.ForeColor = SystemColors.ControlDark;
            ConnType_cbx.Enabled = true;
            ViewConn_btn.Text = "Setting";
            ViewConn_btn.FlatStyle = FlatStyle.Standard;
        }

        int couting = 0;
        private void GetConfig_Handler(string config_msg)
        {
            string[] config_str = config_msg.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            if (config_str[0] == "BLF Setting")
            {
                if (couting == 0)
                {
                    if (config_str[1] == "0")
                        SetControl(freq_cbx, "0");
                    else if (config_str[1] == "2")
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
                    Log_Handler("Get BLF done");
                }
            }
            else if (config_str[0] == "Power RFID")
            {
                if (couting == 0)
                {
                    SetControl(trackBar2, config_str[1]);
                    Log_Handler("Get Read Power done");
                    couting++;
                }
                else
                {
                    SetControl(trackBar3, config_str[1]);
                    Log_Handler("Get Write Power done");
                    couting = 0;
                }
            }
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
            else if (config_str[0] == "Antena RFID")
            {
                if (config_str[1].IndexOf('1') != -1)
                    SetControl(Ant1_ckb, "yes");
                else
                    SetControl(Ant1_ckb, "no");
                if (config_str[1].IndexOf('2') != -1)
                    SetControl(Ant2_ckb, "yes");
                else
                    SetControl(Ant2_ckb, "no");
                if (config_str[1].IndexOf('3') != -1)
                    SetControl(Ant3_ckb, "yes");
                else
                    SetControl(Ant3_ckb, "no");
                if (config_str[1].IndexOf('4') != -1)
                    SetControl(Ant4_ckb, "yes");
                else
                    SetControl(Ant4_ckb, "no");
            }
            else if (config_str[0] == "Power Mode RFID")
            {
                SetControl(power_mode_cbx, config_str[1]);
                Log_Handler("Get Power Mode done");
            }
            else if (config_str[0] == "Region RFID")
            {
                SetControl(region_lst, config_str[1]);
                Log_Handler("Get Region done");
            }
            else if (config_str[0].IndexOf('/') == 0)
            {
                //freq
                if (config_str[16].IndexOf("250", 15) != -1)
                    SetControl(freq_cbx, "0");
                else if (config_str[16].IndexOf("320", 15) != -1)
                    SetControl(freq_cbx, "1");
                else
                    SetControl(freq_cbx, "2");
                //coding
                if (config_str[13].IndexOf('0', 15) != -1)
                    SetControl(coding_cbx, "0");
                else if (config_str[13].IndexOf('2', 15) != -1)
                    SetControl(coding_cbx, "1");
                else if (config_str[13].IndexOf('4', 15) != -1)
                    SetControl(coding_cbx, "2");
                else if (config_str[13].IndexOf('8', 15) != -1)
                    SetControl(coding_cbx, "3");
                //tari
                if (config_str[17].IndexOf("6_25", 15) != -1)
                    SetControl(tari_cbx, "2");
                else if (config_str[17].IndexOf("12_5", 15) != -1)
                    SetControl(tari_cbx, "1");
                else
                    SetControl(tari_cbx, "0");
                //readpower
                SetControl(trackBar2, config_str[23].Substring(config_str[23].IndexOf("=") + 1, 2));
                //readpower
                SetControl(trackBar3, config_str[24].Substring(config_str[24].IndexOf("=") + 1, 2));
                //power mode
                if (config_str[4].IndexOf("FULL") != -1)
                    SetControl(power_mode_cbx, "0");
                else if (config_str[4].IndexOf("MIN") != -1)
                    SetControl(power_mode_cbx, "1");
                else if (config_str[4].IndexOf("MED") != -1)
                    SetControl(power_mode_cbx, "2");
                else if (config_str[4].IndexOf("MAX") != -1)
                    SetControl(power_mode_cbx, "3");
                else
                    SetControl(power_mode_cbx, "4");
            }
            else if (config_str[0].IndexOf("= {") != -1)
            {
                MessageBox.Show(config_msg);
                startcmdprocess(CM.COMMAND.SET_CONN_TYPE_CMD);
            }
            else if (config_str[0] == "Update FW")
            {
                SetControl(progressBar1, config_str[1]);
            }
            else if (config_str[0] == "Keep Alive Timeout")
            {
                startcmdprocess(CM.COMMAND.CHECK_READER_STT_CMD);
            }
            else if (config_str[0] == "NAK")
                Log_Handler("Failed Get Config");
            else
                Log_Handler("Get command not defined");
        }

        private void Log_Handler(string log_msg)
        {
            this.Invoke((MethodInvoker)delegate
            {
                Log_lb.Text = log_msg;
                if (log_msg == "Inventory Mode")
                {
                    Stop_Behavior();
                }
                else if (log_msg == "Stop Inventory")
                {
                    Start_Behavior();
                    ptimer_loghandle.Interval = 2000;
                    ptimer_loghandle.Start();
                }
                else
                {
                    ptimer_loghandle.Interval = 2500;
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
                if (control is TextBox || control is Label)
                    control.Text = config_tx;
                else if (control is ComboBox)
                    (control as ComboBox).SelectedIndex = int.Parse(config_tx);
                else if (control is TrackBar)
                {
                    if ((control as TrackBar).Name.Contains("trackBar"))
                    {
                        (control as TrackBar).Value = int.Parse(config_tx);
                    }
                    else if ((control as TrackBar).Name == "AudioVolume_trb")
                    {
                        (control as TrackBar).Value = int.Parse(config_tx.Remove(config_tx.Length - 2));
                    }
                }
                else if (control is CheckBox)
                {
                    if ("yes" == config_tx)
                        (control as CheckBox).Checked = true;
                    else
                        (control as CheckBox).Checked = false;
                }
                else if (control is CheckedListBox)
                {
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
                }
                else if (control is Button)
                {
                    if ("Failed" == config_tx)
                        (control as Button).BackColor = Color.Red;
                    else
                        (control as Button).BackColor = Color.Blue;
                }
                else if (control is RadioButton)
                {
                    if ("yes" == config_tx)
                        (control as RadioButton).Checked = true;
                    else
                        (control as RadioButton).Checked = false;
                }
                else if (control is ProgressBar)
                {
                    (control as ProgressBar).Value = int.Parse(config_tx);
                }
                else
                {
                    MessageBox.Show("New type control");
                }
                #region sub
                /*switch (control.GetType().Name)
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
                }*/
                #endregion
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
                SetControl(No_Tag_lb, rows.Length.ToString());
            }
        }

        private void Start_Operate_btn_Click(object sender, EventArgs e)
        {
            if (Start_Operate_btn.Text == "Start inventory")
            {
                CM.MessageReceived += Read_handler;
                com_type.Get_Command_Send(CM.COMMAND.START_OPERATION_CMD);

            }
            else
            {
                CM.MessageReceived -= Read_handler;
                com_type.Get_Command_Send(CM.COMMAND.STOP_OPERATION_CMD);
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
                foreach (CheckBox GPOs_ckb in flowLayoutPanel2.Controls)
                {
                    if (GPOs_ckb.Checked)
                        GPO_sets += GPOs_ckb.Text.ToLower() + ",";
                }
                gateway_config.AppendFormat(GW_Format[15], GPO_sets.Remove(GPO_sets.Length - 1));
                com_type.Set_Command_Send(CM.COMMAND.SET_CONFIGURATION_CMD, gateway_config.ToString());
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!");
                Disconnect_Behavior();
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
                /*com_type.Get_Command_Power(CM.COMMAND.GET_POWER_CMD, 0);
                com_type.waitflagRevTCP();
                com_type.Get_Command_Power(CM.COMMAND.GET_POWER_CMD, 1);
                com_type.waitflagRevTCP();*/
                com_type.StartCmd_Process(CM.COMMAND.GET_POWER_CMD);
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
                /*byte[] power_bytes = new byte[2];
                power_bytes[0] = 0;
                power_bytes[1] = (byte)trackBar2.Value;
                com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_POWER_CMD, power_bytes);
                com_type.waitflagRevTCP();
                power_bytes[0] = 1;
                power_bytes[1] = (byte)trackBar3.Value;
                com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_POWER_CMD, power_bytes);
                com_type.waitflagRevTCP();*/
                startcmdprocess(CM.COMMAND.SET_POWER_CMD);
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
                //com_type.Get_Command_Send(CM.COMMAND.GET_POWER_MODE_CMD);
                startcmdprocess(CM.COMMAND.GET_POWER_MODE_CMD);
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
                    if (com_type.getflagConnected_TCPIP())
                    {
                        com_type.StartCmd_Process(CM.COMMAND.GET_RFID_CONFIGURATION_CMD);
                        for (int i = 1; i <= 4; i++)
                        {
                            if ((flowLayoutPanel1.Controls[i - 1] as CheckBox).Checked)
                            {
                                Antena_cbx.Items.Add("ANT" + i.ToString());
                                (flowLayoutPanel3.Controls[i - 1] as CheckBox).CheckState = CheckState.Checked;
                                (flowLayoutPanel3.Controls[i - 1] as CheckBox).Enabled = true;
                            }
                            else
                            {
                                (flowLayoutPanel3.Controls[i - 1] as CheckBox).Enabled = false;
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Connection was disconnected\nPlease connect again!");
                        Disconnect_Behavior();
                    }
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

        private void Set_RFID_btn_Click(object sender, EventArgs e)
        {

        }
        private void ptimer_loghandle_Tick_1(object sender, EventArgs e)
        {
            if (Log_lb.Text == "Disconnected" || Log_lb.Text == "Abort due to close")
            {
                Log_lb.Text = "Idle";
                if (status_lb.Text == "Active")
                    Disconnect_Behavior();
            }

            else
            {
                Log_lb.Text = "Ready!";
            }
            ptimer_loghandle.Stop();
        }

        private void get_protocol_btn_Click(object sender, EventArgs e)
        {
            if (com_type.getflagConnected_TCPIP())
            {
                /*com_type.Get_Command_Power(CM.COMMAND.GET_BLF_CMD, 0);
                com_type.waitflagRevTCP();
                com_type.Get_Command_Power(CM.COMMAND.GET_BLF_CMD, 1);
                com_type.waitflagRevTCP();
                com_type.Get_Command_Power(CM.COMMAND.GET_BLF_CMD, 2);
                com_type.waitflagRevTCP();*/
                com_type.StartCmd_Process(CM.COMMAND.GET_BLF_CMD);
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!");
                Disconnect_Behavior();
            }
        }

        private void set_protocol_btn_Click(object sender, EventArgs e)
        {
            if (com_type.getflagConnected_TCPIP())
            {
                /*byte[] freq_bytes = new byte[2];
                freq_bytes[0] = 0;
                freq_bytes[1] = (byte)(2 * freq_cbx.SelectedIndex);
                com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_BLF_CMD, freq_bytes);
                com_type.waitflagRevTCP();

                freq_bytes[0] = 1;
                freq_bytes[1] = (byte)(coding_cbx.SelectedIndex);
                com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_BLF_CMD, freq_bytes);
                com_type.waitflagRevTCP();

                freq_bytes[0] = 2;
                freq_bytes[1] = (byte)(tari_cbx.SelectedIndex);
                com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_BLF_CMD, freq_bytes);
                com_type.waitflagRevTCP();*/
                startcmdprocess(CM.COMMAND.SET_BLF_CMD);
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!");
                Disconnect_Behavior();
            }
        }

        private void set_port_btn_Click(object sender, EventArgs e)
        {
            switch (Change_conntype_cbx.SelectedIndex)
            {
                case 0:
                    if (!String.IsNullOrEmpty(zigbee_form.PanID))
                    {
                        String zigbee_config = String.Empty;
                        zigbee_config = "gateway_zigbee_configure = {\nbaudrate = 115200"
                                        + "\nport_name = /dev/ttyO4"
                                        + "\ntimeout = 50"
                                        + "\nmax_packet_length = 73"
                                        + "\nchannel = 25"
                                        + "\npanid = 38CE"
                                        + "\nepid = "
                                        + "\ndeviceid = "
                                        + "\n}";

                        MessageBox.Show(zigbee_config);
                        byte[] sd = Encoding.ASCII.GetBytes(zigbee_config);
                        byte[] newArray = new byte[sd.Length + 1];
                        sd.CopyTo(newArray, 1);
                        newArray[0] = 0;
                        com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_PORT_PROPERTIES_CMD, newArray);
                    }
                    else
                        MessageBox.Show("Zigbee Config not confirm");
                    break;
                case 1:
                    if (!String.IsNullOrEmpty(wifi_form.ssid_name))
                    {
                        String wifi_config = String.Empty;
                        if (wifi_form.automatic)
                            wifi_config = "gateway_wifi_configure = {\nssid =" + wifi_form.ssid_name
                                          + "\npsk=" + wifi_form.passwd
                                          + "\ninet=dhcp\n}";
                        else
                            wifi_config = "gateway_wifi_configure = {\nssid =" + wifi_form.ssid_name
                                          + "\npsk=" + wifi_form.passwd
                                          + "\ninet=static"
                                          + "\naddress=" + wifi_form.address
                                          + "\nnetmask=" + wifi_form.netmask
                                          + "\ngateway=" + wifi_form.gateway
                                          + "\n}";
                        MessageBox.Show(wifi_config);
                        byte[] sd = Encoding.ASCII.GetBytes(wifi_config);
                        byte[] newArray = new byte[sd.Length + 1];
                        sd.CopyTo(newArray, 1);
                        newArray[0] = 1;
                        com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_PORT_PROPERTIES_CMD, newArray);
                    }
                    else
                        MessageBox.Show("Wifi Config not confirm");
                    break;
                case 2:
                    break;
                case 3:
                    if (!String.IsNullOrEmpty(tcp_form.address))
                    {
                        String tcp_config = String.Empty;
                        if (wifi_form.automatic)
                            tcp_config = "gateway_tcp_configure = {\nipaddress =" + tcp_form.address
                                          + "\nhostname =" + Gateway_ID_tx.Text
                                          + "\nport =" + tcp_form.port
                                          + "\ntimeout=" + tcp_form.Timeout
                                          + "\nmax_packet_length=" + tcp_form.Length
                                          + "\n}";
                        else
                            tcp_config = "gateway_tcp_configure = {\nipaddress =" + tcp_form.address
                                          + "\nhostname =" + Gateway_ID_tx.Text
                                          + "\nport =" + tcp_form.port
                                          + "\ntimeout=" + tcp_form.Timeout
                                          + "\nmax_packet_length=" + tcp_form.Length
                                          + "\nnetmask=" + tcp_form.netmask
                                          + "\ngateway=" + tcp_form.gateway
                                          + "\n}";
                        MessageBox.Show(tcp_config);
                        byte[] sd = Encoding.ASCII.GetBytes(tcp_config);
                        byte[] newArray = new byte[sd.Length + 1];
                        sd.CopyTo(newArray, 1);
                        newArray[0] = 3;
                        com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_PORT_PROPERTIES_CMD, newArray);
                    }
                    else
                        MessageBox.Show("Ethernet config not confirm");
                    break;
                case 4:
                    break;
                default:
                    break;
            }
        }

        private void Change_conntype_cbx_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (Change_conntype_cbx.SelectedIndex)
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
        private string New_Node()
        {
            if (list_plan_name.Count == 0)
            {
                list_plan_name.Add("Plan1");
                return "Plan1";
            }
            else
            {
                for (int idx = 0; idx < list_plan_name.Count + 1; idx++)
                {
                    if (!list_plan_name.Contains("Plan" + (idx + 1).ToString()))
                    {
                        list_plan_name.Add("Plan" + (idx + 1).ToString());
                        break;
                    }
                }
                return list_plan_name[list_plan_name.Count - 1];
            }
        }

        private void Add_plan_btn_Click(object sender, EventArgs e)
        {
            //TreeNode node;
            //Plan_Node nodeplan = new Plan_Node();
            //nodeplan.name = New_Node();
            //node = new TreeNode();
            treeView1.Nodes.Add(New_Node());
        }

        private void treeView1_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            list_plan_name.Add(e.Label);
        }

        private void treeView1_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            list_plan_name.Remove(e.Node.Text);
        }

        private void Remove_plan_btn_Click(object sender, EventArgs e)
        {
            list_plan_name.Remove(treeView1.SelectedNode.Text);
            treeView1.Nodes.Remove(treeView1.SelectedNode);
        }

        private void update_fw_btn_Click(object sender, EventArgs e)
        {
            OpenFileDialog firware_file = new OpenFileDialog();
            firware_file.Filter = "Bin file (*.*)|*.*";
            firware_file.FilterIndex = 1;
            firware_file.Multiselect = false;
            firware_file.RestoreDirectory = true;
            //DCM_file.InitialDirectory = DCM_file_tx.Text;
            if (firware_file.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FileInfo fileinfo = new FileInfo(firware_file.FileName);
                byte[] bytesFile = System.IO.File.ReadAllBytes(firware_file.FileName);
                string info_file = "[" + fileinfo.Name + "]" + "[" + fileinfo.Length.ToString() + "]";
                Log_Handler("Sending ...");
                progressBar1.Value = 0;
                com_type.Update_File(bytesFile, info_file);
                //com_type.waitflagRevTCP();
            }
        }
    }
}
