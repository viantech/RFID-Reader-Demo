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
        setup_connect connect_form;
        
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
        string[] simple_plan_format = new string[5] {
            "SimpleReadPlan:[Antennas=[{0}],",
            "Protocol=GEN2,",
            "Filter=TagData:[EPC={0}],",
            "Op=null,",
            "UseFastSearch=true,Weight={0}]"
        };
        List<string> list_plan_name = new List<string>();
        Plan_Node.Plan_Root all_plans = new Plan_Node.Plan_Root();
        //ManualResetEvent oSignalEvent = new ManualResetEvent(false);
        public Form1()
        {
            InitializeComponent();
            ConnType_cbx.SelectedIndex = 0;
            zigbee_form = new Zigbee_pop();
            wifi_form = new Wifi_pop();
            tcp_form = new Tcp_pop();
            serial_form = new Serial_pop();
            connect_form = new setup_connect();
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            //status_led.Image = global::GatewayForm.Properties.Resources.red;
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
                        com_type.waitflagRevTCP();
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
                    case CM.COMMAND.GET_BLF_CMD:
                        com_type.Get_Command_Power(CM.COMMAND.GET_BLF_CMD, 0);
                        com_type.waitflagRevTCP();
                        com_type.Get_Command_Power(CM.COMMAND.GET_BLF_CMD, 1);
                        com_type.waitflagRevTCP();
                        com_type.Get_Command_Power(CM.COMMAND.GET_BLF_CMD, 2);
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
                            if (!String.IsNullOrEmpty(connect_form.address))
                            {
                                com_type.Connect(connect_form.address, int.Parse(connect_form.port));
                                if (com_type != null && com_type.getflagConnected_TCPIP())
                                { 
                                    Connected_Behavior();
                                    zigbee_form.ip_add = connect_form.address;
                                    zigbee_form.port = connect_form.port;
                                }

                                else
                                {
                                    Log_lb.Text = "Idle";
                                }
                            }
                            else
                            {
                                MessageBox.Show("Please config Zigbee property", "Setup Error", MessageBoxButtons.OK);
                                Log_lb.Text = "Idle";
                            }
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
                            if (!String.IsNullOrEmpty(connect_form.address))
                            {
                                com_type.Connect(connect_form.address, int.Parse(connect_form.port));
                                if (com_type != null && com_type.getflagConnected_TCPIP())
                                { 
                                    Connected_Behavior();
                                    wifi_form.address = connect_form.address;
                                    wifi_form.port = connect_form.port;
                                }
                                else
                                {
                                    Log_lb.Text = "Idle";
                                }
                            }
                            else
                            {
                                MessageBox.Show("Please config Wifi property", "Setup Error", MessageBoxButtons.OK);
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
                            if (!String.IsNullOrEmpty(connect_form.address))
                            {
                                com_type.Connect(connect_form.address, int.Parse(connect_form.port));
                                if (com_type != null && com_type.getflagConnected_TCPIP())
                                {
                                    Connected_Behavior();
                                    tcp_form.address = connect_form.address;
                                    tcp_form.port = connect_form.port;
                                }
                                else
                                {
                                    Log_lb.Text = "Idle";
                                }
                            }
                            else
                            {
                                MessageBox.Show("Please config Ethernet property", "Setup Error", MessageBoxButtons.OK);
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
            catch (Exception ex) 
            {
                MessageBox.Show(ex.ToString());
            }
        }

        int couting = 0;
        private void GetConfig_Handler(string config_msg)
        {
            string[] config_str = config_msg.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            if (config_str[0] == "Update FW")
            {
                Log_Handler("Updating " + config_str[1] + "% ...");
                SetControl(progressBar1, config_str[1]);
            }
            else if (config_str[0] == "BLF Setting")
            {
                if (couting == 0)
                {
                    if (config_str[1] == "0")
                        SetControl(freq_cbx, "0");
                    else if (config_str[1] == "2")
                        SetControl(freq_cbx, "1");
                    else
                        SetControl(freq_cbx, "2");
                    SetControl(progressBar1, "35");
                    couting++;
                }
                else if (couting == 1)
                {
                    SetControl(coding_cbx, config_str[1]);
                    SetControl(progressBar1, "70");
                    couting++;
                }
                else
                {
                    SetControl(tari_cbx, config_str[1]);
                    couting = 0;
                    SetControl(progressBar1, "100");
                    Log_Handler("Get BLF done");
                    Enable_RFID();
                }
            }
            else if (config_str[0] == "Power RFID")
            {
                if (couting == 0)
                {
                    SetControl(trackBar2, config_str[1]);
                    SetControl(progressBar1, "50");
                    couting++;
                }
                else
                {
                    SetControl(trackBar3, config_str[1]);
                    Log_Handler("Get Global Power done");
                    SetControl(progressBar1, "100");
                    couting = 0;
                    Enable_RFID();
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
            else if (config_str[0] == "Get Plan")
            {
                list_plan_name.Clear();
                all_plans.plan_list.Clear();
                all_plans = LoadReadPlans(config_str[1]);
                PopulateTreeView(all_plans);
            }
            else if (config_str[0] == "Keep Alive Timeout")
            {
                startcmdprocess(CM.COMMAND.CHECK_READER_STT_CMD);
            }
            else if (config_str[0] == "Change Protocol")
            {
                MessageBox.Show(config_str[1],"New Protocol Port Property",MessageBoxButtons.OK);
                startcmdprocess(CM.COMMAND.SET_CONN_TYPE_CMD);
            }
            else if (config_str[0][0] == '/')
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
                SetControl(trackBar2, config_str[23].Substring(config_str[23].IndexOf("=") + 1, config_str[23].Length - 3 - config_str[23].IndexOf("=")));
                //readpower
                SetControl(trackBar3, config_str[24].Substring(config_str[24].IndexOf("=") + 1, config_str[24].Length - 3 - config_str[24].IndexOf("=")));
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
                //Read Plan
                //int id_muti = config_str[20].IndexOf("Multi");
                list_plan_name.Clear();
                all_plans.plan_list.Clear();
                all_plans = LoadReadPlans(config_str[20].Substring(config_str[20].IndexOf("Simple")));
                PopulateTreeView(all_plans);
            }
            else if (config_str[0] == "NAK")
                MessageBox.Show("Failed to get Configuration", "Error Get Command", MessageBoxButtons.OK);
            else
                MessageBox.Show("Get Command not defined", "Error Get Command", MessageBoxButtons.OK);
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
                    ptimer_loghandle.Interval = 500;
                    ptimer_loghandle.Start();
                }
                else
                {
                    ptimer_loghandle.Interval = 1500;
                    ptimer_loghandle.Start();
                }
            });
        }

        private void ptimer_loghandle_Tick_1(object sender, EventArgs e)
        {
            if (Log_lb.Text == "Disconnected" || Log_lb.Text == "Abort due to close")
            {
                Log_lb.Text = "Idle";
                progressBar1.Value = 0;
                if (status_lb.Text == "Active")
                    Disconnect_Behavior();
            }
            else if ("Inventory Mode" == Log_lb.Text)
            {
                startcmdprocess(CM.COMMAND.DIS_CONNECT_CMD);
                Start_Behavior();
                Start_Operate_btn.Enabled = false;
                MessageBox.Show("Start/Stop Inventory not working properly!\nPlease check Antena connection\nDisconnect to Gateway", "Inventory Error");
            }
            else if ("Getting Protocol ..." == Log_lb.Text)
            {
                ptimer_loghandle.Stop();
                DialogResult result = MessageBox.Show("Get BLF info not complete\nPlease click \"Yes\" for retry", "Warning Protocol Configuration", MessageBoxButtons.YesNo);
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    progressBar1.Value = 0;
                    get_protocol_btn.Enabled = true;
                    get_protocol_btn.PerformClick();
                }
                else
                {
                    MessageBox.Show("The BLF components show might not right", "Error Protocol Configuration");
                    Enable_RFID();
                    Log_lb.Text = "Failed Protocol";
                    progressBar1.Value = 0;
                }
            }
            else if ("Getting Power ..." == Log_lb.Text)
            {
                ptimer_loghandle.Stop();
                DialogResult result = MessageBox.Show("Get Power info not complete\nPlease click \"Yes\" for retry", "Warning Power Configuration", MessageBoxButtons.YesNo);
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    progressBar1.Value = 0;
                    get_power_btn.Enabled = true;
                    get_power_btn.PerformClick();
                }
                else
                {
                    MessageBox.Show("The Read/Write power value show might not right", "Error Power Configuration");
                    Enable_RFID();
                    Log_lb.Text = "Failed Power";
                    progressBar1.Value = 0;
                }
            }
            else
            {
                progressBar1.Value = 0;
                Log_lb.Text = "Ready!";
                //Enable_RFID();
            }
            //Start_Behavior();
            ptimer_loghandle.Stop();
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
                if (control is ComboBox)
                {
                    try
                    {
                        (control as ComboBox).SelectedIndex = int.Parse(config_tx);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        MessageBox.Show("Combox is out of range");
                    }
                }
                else if (control is ProgressBar)
                {
                    (control as ProgressBar).Value = int.Parse(config_tx);
                }
                else if (control is TrackBar)
                {
                    if ((control as TrackBar).Name.Contains("trackBar"))
                    {
                        try
                        {
                            (control as TrackBar).Value = int.Parse(config_tx);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            MessageBox.Show("Power value is out of range");
                        }
                    }
                    else if ((control as TrackBar).Name == "AudioVolume_trb")
                    {
                        try
                        {
                            (control as TrackBar).Value = int.Parse(config_tx.Remove(config_tx.Length - 2));
                        }
                        catch (IndexOutOfRangeException)
                        {
                            MessageBox.Show("Audio value is out of range");
                        }
                    }
                }
                else if (control is TextBox || control is Label)
                    control.Text = config_tx;
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
                else
                {
                    MessageBox.Show("New type control", "Warrning", MessageBoxButtons.OK);
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
                SetControl(No_Tag_lb, rows.Length.ToString());
            }
        }

        private void Start_Operate_btn_Click(object sender, EventArgs e)
        {
            if (com_type != null && com_type.getflagConnected_TCPIP())
            {
                if (Start_Operate_btn.Text == "Start inventory")
                {
                    Start_Operate_btn.Enabled = false;
                    CM.MessageReceived += Read_handler;
                    com_type.Get_Command_Send(CM.COMMAND.START_OPERATION_CMD);
                }
                else
                {
                    Start_Operate_btn.Enabled = false;
                    CM.MessageReceived -= Read_handler;
                    com_type.Get_Command_Send(CM.COMMAND.STOP_OPERATION_CMD);
                    ptimer_loghandle.Interval = 5000;
                    ptimer_loghandle.Start();
                }
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK);
                Disconnect_Behavior();
            }
            
        }

        private void Connected_Behavior()
        {
            ConnType_cbx.Enabled = false;
            Connect_btn.Text = "Disconnect";
            status_led.Image = global::GatewayForm.Properties.Resources.green_led;
            status_lb.Text = "Active";
            status_lb.ForeColor = Color.DarkBlue;
            ViewConn_btn.Text = "View Port";
            ViewConn_btn.FlatStyle = FlatStyle.Flat;
            Start_Operate_btn.Enabled = true;
        }

        private void Disconnect_Behavior()
        {
            Connect_btn.Text = "Connect";
            status_led.Image = global::GatewayForm.Properties.Resources.red_led;
            status_lb.Text = "Inactive";
            status_lb.ForeColor = SystemColors.ControlDark;
            ConnType_cbx.Enabled = true;
            ViewConn_btn.Text = "Setting";
            ViewConn_btn.FlatStyle = FlatStyle.Standard;
            Start_Operate_btn.Enabled = false;
            //Clear GW Config
            foreach(CheckBox com_clear in flowLayoutPanel2.Controls)
            {
                (com_clear as CheckBox).Checked = false;
            }
            foreach(CheckBox com_clear in flowLayoutPanel1.Controls)
            {
                (com_clear as CheckBox).Checked = false;
            }
            PalletSupport_cbx.Checked = false;
            for (int conn = 0; conn < ConnectionList_ck.Items.Count; conn++)
                ConnectionList_ck.SetItemChecked(conn, false);
            LED_Support_ckb.Checked = false;
            Offline_ckb.Checked = false;
            RFID_API_ckb.Checked = false;
            StackLight_ckb.Checked = false;
            HW_Verrsion_tx.Text = String.Empty;
            SW_Version_tx.Text = String.Empty;
            Gateway_ID_lb.Text = String.Empty;
            Gateway_ID_tx.Text = String.Empty;
            MessageInterval_tx.Text = String.Empty;
            //RIFD
            foreach (ComboBox comm_clear in this.groupBox8.Controls.OfType<ComboBox>())
            {
                comm_clear.SelectedIndex = 0;
            }
            region_lst.SelectedIndex = 1;
            foreach (TrackBar comm_clear in this.groupBox9.Controls.OfType<TrackBar>())
            {
                comm_clear.Value = 5;
            }
            list_plan_name.Clear();
            all_plans.plan_list.Clear();
            treeView1.Nodes.Clear();
            TreeNode node_lable = new TreeNode("[Plans]");
            treeView1.Nodes.Add(node_lable);
            foreach (CheckBox com_clear in flowLayoutPanel3.Controls)
            {
                (com_clear as CheckBox).Checked = false;
            }
        }

        private void Stop_Behavior()
        {
            Start_Operate_btn.Text = "Stop inventory";
            //GW Config
            Set_GW_Config_btn.Enabled = false;
            Get_GW_Config_btn.Enabled = false;
            set_newconn_btn.Enabled = false;
            Get_RFID_btn.Enabled = false;
            Set_RFID_btn.Enabled = false;
            Connect_btn.Enabled = false;
            update_fw_btn.Enabled = false;
            set_port_btn.Enabled = false;
            //RFID 
            Block_RFID_Tab();
            Start_Operate_btn.Enabled = true;
        }
       
        private void Start_Behavior()
        {
            Start_Operate_btn.Text = "Start inventory";
            //GW Config
            Set_GW_Config_btn.Enabled = true;
            Get_GW_Config_btn.Enabled = true;
            set_newconn_btn.Enabled = true;
            Get_RFID_btn.Enabled = true;
            Set_RFID_btn.Enabled = true;
            Connect_btn.Enabled = true;
            update_fw_btn.Enabled = true;
            set_port_btn.Enabled = true;

            Enable_RFID();
            this.dataGridView1.Rows.Clear();
            this.No_Tag_lb.Text = "0";
            Start_Operate_btn.Enabled = true;
        }

        //RFID View
        private void Block_RFID_Tab()
        {
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
            foreach (Control comm_btn in this.groupBox14.Controls)
            {
                comm_btn.Enabled = false;
            }
            //Start_Operate_btn.Enabled = false;
        }

        private void Enable_RFID()
        {
            this.Invoke((MethodInvoker)delegate
            {
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
                foreach (Control comm_btn in this.groupBox14.Controls)
                {
                    comm_btn.Enabled = true;
                }
                //Start_Operate_btn.Enabled = true;
            });
        }

        private void Get_GW_Config_btn_Click(object sender, EventArgs e)
        {
            if (com_type != null && com_type.getflagConnected_TCPIP())
            {
                com_type.Get_Command_Send(CM.COMMAND.GET_CONFIGURATION_CMD);
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK);
                Disconnect_Behavior();
            }
        }

        private void Set_GW_Config_btn_Click(object sender, EventArgs e)
        {
            if (com_type != null && com_type.getflagConnected_TCPIP())
            {
                StringBuilder gateway_config = new StringBuilder();
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
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK);
                Disconnect_Behavior();
            }
        }

        private void ViewConn_btn_Click(object sender, EventArgs e)
        {
            if (ViewConn_btn.Text == "Setting")
            {
                connect_form.ShowDialog();
            }
            else
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
        }

        private void Get_RFID_btn_Click(object sender, EventArgs e)
        {
            if (com_type != null && com_type.getflagConnected_TCPIP())
            {
                com_type.Get_Command_Send(CM.COMMAND.GET_RFID_CONFIGURATION_CMD);
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK);
                Disconnect_Behavior();
            }
        }

        private void ConnType_cbx_SelectedIndexChanged(object sender, EventArgs e)
        {
            connect_form.ShowDialog();
            /*switch (ConnType_cbx.SelectedIndex)
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
            }*/
        }

        private void get_power_btn_Click(object sender, EventArgs e)
        {
            if (com_type != null && com_type.getflagConnected_TCPIP())
            {
                /*com_type.Get_Command_Power(CM.COMMAND.GET_POWER_CMD, 0);
                com_type.waitflagRevTCP();
                com_type.Get_Command_Power(CM.COMMAND.GET_POWER_CMD, 1);
                com_type.waitflagRevTCP();*/
                Log_lb.Text = "Getting Power ...";
                couting = 0;
                Block_RFID_Tab();
                com_type.StartCmd_Process(CM.COMMAND.GET_POWER_CMD);
                ptimer_loghandle.Interval = 4000;
                ptimer_loghandle.Start();
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK);
                Disconnect_Behavior();
            }
        }

        private void set_power_btn_Click(object sender, EventArgs e)
        {
            if (com_type != null && com_type.getflagConnected_TCPIP())
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
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK);
                Disconnect_Behavior();
            }
        }

        private void get_region_btn_Click(object sender, EventArgs e)
        {
            if (com_type != null && com_type.getflagConnected_TCPIP())
            {
                com_type.Get_Command_Send(CM.COMMAND.GET_REGION_CMD);
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK);
                Disconnect_Behavior();
            }
        }

        private void set_region_btn_Click(object sender, EventArgs e)
        {
            if (com_type != null && com_type.getflagConnected_TCPIP())
            {
                byte[] region_byte = new byte[1];
                region_byte[0] = (byte)region_lst.SelectedIndex;
                com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_REGION_CMD, region_byte);
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK);
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
            if (com_type != null && com_type.getflagConnected_TCPIP())
            {
                //com_type.Get_Command_Send(CM.COMMAND.GET_POWER_MODE_CMD);
                startcmdprocess(CM.COMMAND.GET_POWER_MODE_CMD);
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK);
                Disconnect_Behavior();
            }
        }

        private void set_power_mode_btn_Click(object sender, EventArgs e)
        {
            if (com_type != null && com_type.getflagConnected_TCPIP())
            {
                byte[] pw_mode_byte = new byte[1];
                pw_mode_byte[0] = (byte)power_mode_cbx.SelectedIndex;
                com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_POWER_MODE_CMD, pw_mode_byte);
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK);
                Disconnect_Behavior();
            }
        }

        private void get_protocol_btn_Click(object sender, EventArgs e)
        {
            if (com_type != null && com_type.getflagConnected_TCPIP())
            {
                /*com_type.Get_Command_Power(CM.COMMAND.GET_BLF_CMD, 0);
                oSignalEvent.Reset();
                oSignalEvent.WaitOne(2000);
                com_type.Get_Command_Power(CM.COMMAND.GET_BLF_CMD, 1);
                oSignalEvent.Reset();
                oSignalEvent.WaitOne(2000);
                com_type.Get_Command_Power(CM.COMMAND.GET_BLF_CMD, 2);
                oSignalEvent.Reset();
                oSignalEvent.WaitOne(2000);*/
                couting = 0;
                Log_lb.Text = "Getting Protocol ...";
                Block_RFID_Tab();
                com_type.StartCmd_Process(CM.COMMAND.GET_BLF_CMD);
                ptimer_loghandle.Interval = 5000;
                ptimer_loghandle.Start();
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK);
                Disconnect_Behavior();
            }
        }

        private void set_protocol_btn_Click(object sender, EventArgs e)
        {
            if (com_type != null && com_type.getflagConnected_TCPIP())
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
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK);
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
            if (com_type != null && com_type.getflagConnected_TCPIP())
            {
                byte[] conn_type_byte = new byte[1];
                conn_type_byte[0] = (byte)Change_conntype_cbx.SelectedIndex;
                com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_CONN_TYPE_CMD, conn_type_byte);
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK);
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
                                //(flowLayoutPanel3.Controls[i - 1] as CheckBox).CheckState = CheckState.Checked;
                                //(flowLayoutPanel3.Controls[i - 1] as CheckBox).Enabled = true;
                            }
                            else
                            {
                                //(flowLayoutPanel3.Controls[i - 1] as CheckBox).Enabled = false;
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK);
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

                        MessageBox.Show(zigbee_config,"Zigbee Port Property", MessageBoxButtons.OK);
                        byte[] sd = Encoding.ASCII.GetBytes(zigbee_config);
                        byte[] newArray = new byte[sd.Length + 1];
                        sd.CopyTo(newArray, 1);
                        newArray[0] = 0;
                        com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_PORT_PROPERTIES_CMD, newArray);
                    }
                    else
                        MessageBox.Show("Please config Zigbee Port", "Warning Zigbee Configuration",MessageBoxButtons.OK);
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
                        MessageBox.Show(wifi_config, "Wifi Port Property", MessageBoxButtons.OK);
                        byte[] sd = Encoding.ASCII.GetBytes(wifi_config);
                        byte[] newArray = new byte[sd.Length + 1];
                        sd.CopyTo(newArray, 1);
                        newArray[0] = 1;
                        com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_PORT_PROPERTIES_CMD, newArray);
                    }
                    else
                        MessageBox.Show("Please config Wifi Port", "Warning Wifi Configuration", MessageBoxButtons.OK);
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
                        MessageBox.Show(tcp_config, "Ethernet Port Property", MessageBoxButtons.OK);
                        byte[] sd = Encoding.ASCII.GetBytes(tcp_config);
                        byte[] newArray = new byte[sd.Length + 1];
                        sd.CopyTo(newArray, 1);
                        newArray[0] = 3;
                        com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_PORT_PROPERTIES_CMD, newArray);
                    }
                    else
                        MessageBox.Show("Please config Ethernet Port", "Warning Ethernet Configuration", MessageBoxButtons.OK);
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
                DialogResult result = MessageBox.Show("File Selected:\n" + firware_file.FileName + "\nAre you sure to upload this firmware?\nPlease click \"Yes\" for confirmation",
                                                              "Confirmation", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    FileInfo fileinfo = new FileInfo(firware_file.FileName);
                    byte[] bytesFile = System.IO.File.ReadAllBytes(firware_file.FileName);
                    string info_file = "[" + fileinfo.Name + "]" + "[" + fileinfo.Length.ToString() + "]";
                    Log_lb.Text = "Start Send File";
                    progressBar1.Value = 0;
                    com_type.Update_File(bytesFile, info_file);
                }
                else
                {
                    //no...
                }

            }
        }
        #region Get Plan
        private string New_NamePlan()
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
            TreeNode node;
            Plan_Node.Plan_Struct nodeplan = new Plan_Node.Plan_Struct(New_NamePlan());
            node = new TreeNode(nodeplan.name);
            node.Tag = nodeplan;
            treeView1.Nodes.Add(node);
            all_plans.plan_list.Add(nodeplan);
        }

        private Plan_Node.Plan_Root LoadReadPlans(string plan_str)
        {
            Plan_Node.Plan_Root root = new Plan_Node.Plan_Root();
            string[] seperators = new string[] { "SimpleReadPlan:[Antennas=[", ",Protocol=", ",Filter=", ",Op=", ",UseFastSearch=", ",Weight=" };
            string[] field = plan_str.Split(seperators, StringSplitOptions.None);
            if (field.Length < 12)
            {
                Plan_Node.Plan_Struct theplan = new Plan_Node.Plan_Struct(New_NamePlan());
                theplan.antena = field[1].TrimEnd(']');
                theplan.type = FILTER.EPC;
                if (theplan.type == FILTER.EPC)
                {
                    theplan.EPC = field[3].Substring(field[3].IndexOf('=') + 1).TrimEnd(']');
                }
                theplan.weight = field[6].Substring(0, field[6].Length - 1);
                root.plan_list.Add(theplan);
            }
            else
            {
                for (int num_plan = 0; num_plan < field.Length / 6; num_plan++)
                {
                    Plan_Node.Plan_Struct theplan = new Plan_Node.Plan_Struct(New_NamePlan());
                    theplan.antena = field[6 * num_plan + 1].TrimEnd(']');
                    theplan.type = FILTER.EPC;
                    if (theplan.type == FILTER.EPC)
                    {
                        theplan.EPC = field[6 * num_plan + 3].Substring(field[6 * num_plan + 3].IndexOf('=') + 1).TrimEnd(']');
                    }
                    theplan.weight = field[6 * num_plan + 6].Substring(0, field[6 * num_plan + 6].Length - 2);
                    root.plan_list.Add(theplan);
                }
            }
            return root;
        }
        
        private void PopulateTreeView(Plan_Node.Plan_Root root_node)
        {
            this.Invoke((MethodInvoker)delegate
            {
                treeView1.Nodes.Clear();
                TreeNode node_lable = new TreeNode("[Plans]");
                treeView1.Nodes.Add(node_lable);
                for (int ix = 0; ix < root_node.plan_list.Count; ix++)
                {
                    Plan_Node.Plan_Struct plan = root_node.plan_list[ix];
                    TreeNode node = new TreeNode(plan.name);
                    node.Tag = plan;
                    treeView1.Nodes.Add(node);
                }
            });
        }

        private void Remove_plan_btn_Click(object sender, EventArgs e)
        {
            if (treeView1.Nodes.Count > 2)
            {
                int next = treeView1.SelectedNode.Index;
                list_plan_name.Remove(treeView1.SelectedNode.Text);
                treeView1.Nodes.Remove(treeView1.SelectedNode);
                all_plans.plan_list.RemoveAt(treeView1.SelectedNode.Index - 1);
                treeView1.SelectedNode = treeView1.Nodes[next - 1];
                treeView1.Focus();
            }
            else
            {
                MessageBox.Show("At least must exist one simple plan","Warning Simple Plan",MessageBoxButtons.OK);
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Index == 0)
                treeView1.SelectedNode = e.Node.NextNode;
            else
            {
                DisplayReadPlan(e.Node);
            }
        }
        private void DisplayReadPlan(TreeNode plan_node)
        {
            Plan_Node.Plan_Struct plan = (Plan_Node.Plan_Struct)plan_node.Tag;
            if (plan.type == FILTER.EPC)
            {
                EPC_rbtn.Checked = true;
                EPC_filter_tx.Text = plan.EPC;
            }
            else if (plan.type == FILTER.User)
            {
                Mem_rbtn.Checked = true;
                Memory_filter_tx.Text = plan.EPC;
            }
            else
            {
                tid_rbtn.Checked = true;
                TID_filter_tx.Text = plan.EPC;
            }
            if (plan.antena.IndexOf('1') != -1)
                Ant1_plan_ckb.Checked = true;
            else
                Ant1_plan_ckb.Checked = false;

            if (plan.antena.IndexOf('2') != -1)
                Ant2_plan_ckb.Checked = true;
            else
                Ant2_plan_ckb.Checked = false;

            if (plan.antena.IndexOf('3') != -1)
                Ant3_plan_ckb.Checked = true;
            else
                Ant3_plan_ckb.Checked = false;

            if (plan.antena.IndexOf('4') != -1)
                Ant4_plan_ckb.Checked = true;
            else
                Ant4_plan_ckb.Checked = false;
            weight_tx.Text = plan.weight;
        }

        private void get_plan_btn_Click(object sender, EventArgs e)
        {
            if (com_type != null && com_type.getflagConnected_TCPIP())
            {
                com_type.Get_Command_Send(CM.COMMAND.GET_PLAN_CMD);
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK);
                Disconnect_Behavior();
            }
        }
        #endregion

        #region Set Plan

        private void treeView1_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Label != null)
            {
                list_plan_name[treeView1.SelectedNode.Index - 1] = e.Label;
                all_plans.plan_list[treeView1.SelectedNode.Index - 1].name = e.Label;
            }
        }

        private void EPC_rbtn_CheckedChanged(object sender, EventArgs e)
        {
            if (EPC_rbtn.Checked == true)
            {
                EPC_filter_tx.Enabled = true;
                Memory_filter_tx.Enabled = false;
                TID_filter_tx.Enabled = false;
                if (all_plans.plan_list.Count > 0)
                {
                    all_plans.plan_list[treeView1.SelectedNode.Index - 1].type = FILTER.EPC;
                }
            }
        }

        private void Mem_rbtn_CheckedChanged(object sender, EventArgs e)
        {
            if (Mem_rbtn.Checked == true)
            {
                EPC_filter_tx.Enabled = false;
                Memory_filter_tx.Enabled = true;
                TID_filter_tx.Enabled = false;
                if (all_plans.plan_list.Count > 0)
                {
                    all_plans.plan_list[treeView1.SelectedNode.Index - 1].type = FILTER.User;
                }
            }
        }

        private void tid_rbtn_CheckedChanged(object sender, EventArgs e)
        {
            if (tid_rbtn.Checked == true)
            {
                EPC_filter_tx.Enabled = false;
                Memory_filter_tx.Enabled = false;
                TID_filter_tx.Enabled = true;
                if (all_plans.plan_list.Count > 0)
                {
                    all_plans.plan_list[treeView1.SelectedNode.Index - 1].type = FILTER.TID;
                }
            }
        }

        private void EPC_filter_tx_Leave(object sender, EventArgs e)
        {
            if (EPC_rbtn.Checked)
            {
                if (all_plans.plan_list.Count > 0)
                    all_plans.plan_list[treeView1.SelectedNode.Index - 1].EPC = EPC_filter_tx.Text;
            }
        }
       
        private void Memory_filter_tx_Leave(object sender, EventArgs e)
        {
            if (Mem_rbtn.Checked)
            {
                if (all_plans.plan_list.Count > 0)
                    all_plans.plan_list[treeView1.SelectedNode.Index - 1].EPC = Memory_filter_tx.Text;
            }
        }

        private void TID_filter_tx_Leave(object sender, EventArgs e)
        {
            if (tid_rbtn.Checked)
            {
                if (all_plans.plan_list.Count > 0)
                    all_plans.plan_list[treeView1.SelectedNode.Index - 1].EPC = TID_filter_tx.Text;
            }
        }

        private void weight_tx_Leave(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(weight_tx.Text))
            {
                if (all_plans.plan_list.Count > 0)
                    all_plans.plan_list[treeView1.SelectedNode.Index - 1].weight = weight_tx.Text;
            }
            else
                MessageBox.Show("The weight field can not blank", "Error Keypress", MessageBoxButtons.OK);
        }

        private string Antena_ToString()
        {
            string anten_str = String.Empty;
            if (Ant1_plan_ckb.Checked)
                anten_str += "1,";
            if (Ant2_plan_ckb.Checked)
                anten_str += "2,";
            if (Ant3_plan_ckb.Checked)
                anten_str += "3,";
            if (Ant4_plan_ckb.Checked)
                anten_str += "4,";
            return anten_str.Remove(anten_str.Length - 1);
        }

        private void flowLayoutPanel3_Leave(object sender, EventArgs e)
        {
            if (all_plans.plan_list.Count > 0)
                all_plans.plan_list[treeView1.SelectedNode.Index - 1].antena = Antena_ToString();
        }

        private string Plans_ToString(Plan_Node.Plan_Root plans)
        {
            StringBuilder plan_string = new StringBuilder();
            if (all_plans.plan_list.Count == 1)
            {
                plan_string.AppendFormat(simple_plan_format[0], all_plans.plan_list[0].antena);
                plan_string.AppendFormat(simple_plan_format[1]);
                plan_string.AppendFormat(simple_plan_format[2], all_plans.plan_list[0].EPC);
                plan_string.AppendFormat(simple_plan_format[3]);
                plan_string.AppendFormat(simple_plan_format[4], all_plans.plan_list[0].weight);
            }
            else
            {
                plan_string.Append("MultiReadPlan:[");
                for (int ix = 0; ix < all_plans.plan_list.Count; ix++)
                {
                    plan_string.AppendFormat(simple_plan_format[0], all_plans.plan_list[ix].antena);
                    plan_string.AppendFormat(simple_plan_format[1]);
                    plan_string.AppendFormat(simple_plan_format[2], all_plans.plan_list[ix].EPC);
                    plan_string.AppendFormat(simple_plan_format[3]);
                    plan_string.AppendFormat(simple_plan_format[4], all_plans.plan_list[ix].weight);
                    plan_string.Append(",");
                }
                plan_string.Remove(plan_string.Length - 1, 1);
                plan_string.Append("]");
            }
            return plan_string.ToString();
        }

        private void set_plan_btn_Click(object sender, EventArgs e)
        {
            if (com_type != null && com_type.getflagConnected_TCPIP())
            {
                com_type.Set_Command_Send(CM.COMMAND.SET_PLAN_CMD, Plans_ToString(all_plans));
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK);
                Disconnect_Behavior();
            }
        }
        #endregion

        private void Sensor_EN_ckb_CheckedChanged(object sender, EventArgs e)
        {
            if(Sensor_EN_ckb.Checked)
            {
                reader_type_lb.Text = "Gateway/Conveyor";
                time_on_tx.Text = "0";
                time_off_tx.Text = "0";
                read_sensor_ckb.SelectedIndex = 0;
                time_off_tx.Enabled = false;
                time_on_tx.Enabled = false;
                read_sensor_ckb.Enabled = false;
            }
            else
            {
                time_off_tx.Enabled = true;
                time_on_tx.Enabled = true;
                read_sensor_ckb.Enabled = true;
                reader_type_lb.Text = "Forklift";
            }
        }

        private void Set_sensor_btn_Click(object sender, EventArgs e)
        {
            string sensor_data = String.Empty;
            if (Sensor_EN_ckb.Checked)
                sensor_data += "True,";
            else
                sensor_data += "False,";
            sensor_data += time_on_tx.Text + "," + time_off_tx.Text + "," + read_sensor_ckb.Text;
            if (com_type != null && com_type.getflagConnected_TCPIP())
            {
                com_type.Set_Command_Send(CM.COMMAND.SETTING_SENSOR_CMD, sensor_data);
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK);
                Disconnect_Behavior();
            }
        }
    }
}
