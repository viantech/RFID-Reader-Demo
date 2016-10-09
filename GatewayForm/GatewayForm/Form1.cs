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
        public Form1()
        {
            InitializeComponent();
        }
        
        Communication com_type;
        private void Connect_btn_Click(object sender, EventArgs e)
        {
            switch (ConnType_cbx.SelectedIndex)
            {
                //zigbee
                case 0:
                    if (Connect_btn.Text == "Connect")
                    {
                        com_type = new Communication(CM.TYPECONNECT.HDR_ZIGBEE);
                        com_type.Connect(IP_textbox.Text, int.Parse(Port_textbox.Text));
                        com_type.Config_Msg += new SocketReceivedHandler(GetConfig_Handler);
                        com_type.Log_Msg += new SocketReceivedHandler(Log_Handler);
                        Connected_Behavior();
                    }
                    else if (Connect_btn.Text == "Disconnect")
                    {
                        com_type.Close();
                        Disconnect_Behavior();
                    }
                    break;
                //wifi
                case 1: MessageBox.Show("qa1");
                    break;
                //bluetooth
                case 2:
                    break;
                //Ethernet
                case 3:
                    if (Connect_btn.Text == "Connect")
                    {
                        com_type = new Communication(CM.TYPECONNECT.HDR_ETHERNET);
                        com_type.Connect(IP_textbox.Text, int.Parse(Port_textbox.Text));
                        com_type.Config_Msg += new SocketReceivedHandler(GetConfig_Handler);
                        com_type.Log_Msg += new SocketReceivedHandler(Log_Handler);
                        Connected_Behavior();
                    }
                    else if (Connect_btn.Text == "Disconnect")
                    {
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
            string [] config_str = config_msg.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (config_str[0].Contains("Seldatinc gateway "))
            {
                //Gateway serial
                SetControl(Gateway_ID_lb, config_str[1].Substring(config_str[1].IndexOf(":") + 1));
                SetControl(Gateway_ID_tx, config_str[1].Substring(config_str[1].IndexOf(":") + 1));
                //Hardware version
                SetControl(HW_Verrsion_tx, config_str[2].Substring(config_str[2].IndexOf(":") + 1));
                //Software version
                SetControl(SW_Version_tx, config_str[3].Substring(config_str[3].IndexOf(":") + 1));
                //Connection support
                SetControl(ConnectionList_ck, config_str[4].Substring(config_str[4].IndexOf(":") + 1));
                //Audio support
                SetControl(AudioSupport_rbtn, config_str[6].Substring(config_str[6].IndexOf(":") + 1));
                SetControl(AudioVolume_trb, config_str[7].Substring(config_str[7].IndexOf(":") + 1));
                //Pallet ID
                SetControl(PatternID_tx, config_str[10].Substring(config_str[10].IndexOf(":") + 1));
                //Message queue time interval
                SetControl(MessageInterval_tx, config_str[13].Substring(config_str[13].IndexOf(":") + 1));
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
        }

        private void Log_Handler(string log_msg)
        {
            SetControl(Log_lb, log_msg);
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
                if (control is TextBox || control is Label)
                    control.Text = config_tx;

                if (control is CheckBox)
                {
                    if ("yes" == config_tx)
                        (control as CheckBox).Checked = true;
                    else
                        (control as CheckBox).Checked = false;
                }

                if (control is RadioButton)
                {
                    if ("yes" == config_tx)
                        (control as RadioButton).Checked = true;
                    else
                        (control as RadioButton).Checked = false;
                }

                if (control is CheckedListBox)
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

                if (control is TrackBar)
                {
                    (control as TrackBar).Value = 100;
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
                rows = rows.Skip(1).ToArray();

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
                com_type.Receive_Command_Handler(CM.COMMAND.STOP_OPERATION_CMD);
                Start_Operate_btn.Text = "Start inventory";
            }
        }

        private void Get_GW_Config_btn_Click(object sender, EventArgs e)
        {
            com_type.Get_Command_Send(CM.COMMAND.GET_CONFIGURATION_CMD);
            com_type.Receive_Command_Handler(CM.COMMAND.GET_CONFIGURATION_CMD);
        }

        private void Set_GW_Config_btn_Click(object sender, EventArgs e)
        {
            string gateway_info = "quang";
            com_type.Set_Command_Send(CM.COMMAND.SET_CONFIGURATION_CMD, gateway_info);
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

        }

        private void Get_RFID_btn_Click(object sender, EventArgs e)
        {
            com_type.Get_Command_Send(CM.COMMAND.GET_RFID_CONFIGURATION_CMD);
            com_type.Receive_Command_Handler(CM.COMMAND.GET_RFID_CONFIGURATION_CMD);
        }


    }
}
