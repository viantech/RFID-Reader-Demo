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
using System.Security.AccessControl;
using System.Net.NetworkInformation;

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
        ScanIP scanIP_form;
        LogIOData plog; // Declare Database Table
        string anten_list = String.Empty;

        List<string> list_plan_name = new List<string>();
        Plan_Node.Plan_Root all_plans = new Plan_Node.Plan_Root();
        List<string> list_cell_0 = new List<string>();
        //List<string> list_same = new List<string>();
        Dictionary<int, int> antena_read_power_list = new Dictionary<int, int>();
        Dictionary<int, int> antena_write_power_list = new Dictionary<int, int>();
        //string[] RFID_fixed = new string[5];

        public Form1()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            //this.MaximizeBox = false;
            ConnType_cbx.SelectedIndex = (int)CM.TYPECONNECT.HDR_ETHERNET;
            zigbee_form = new Zigbee_pop();
            wifi_form = new Wifi_pop();
            tcp_form = new Tcp_pop();
            serial_form = new Serial_pop();
            connect_form = new setup_connect();
            scanIP_form = new ScanIP();
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            set_gpo_btn.BringToFront();
            get_gpi_btn.BringToFront();
            get_anten_btn.BringToFront();
            // create contructor for class LogIOData
            if (plog == null)
            {

                plog = new LogIOData();
                plog.loadData2Table += LoadDatatoTablefromDBbrowser;
                plog.CreateDBTable();

            }
            CM.Log_Msg += Log_Handler;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (com_type != null && com_type.getflagConnected_TCPIP())
                    com_type.Close();
                zigbee_form.Dispose();
                wifi_form.Dispose();
                tcp_form.Dispose();
                serial_form.Dispose();
                //CM.ConfigMessage -= GetConfig_Handler;
                CM.Log_Msg -= Log_Handler;
                plog.loadData2Table -= LoadDatatoTablefromDBbrowser;
            }
            catch
            { ;}

        }
        
        private void startcmdprocess(CM.COMMAND CMD)
        {
            Thread pThreadCmd = new Thread(() => cmdprocess(CMD));
            pThreadCmd.Start();
        }
        private void cmdprocess(CM.COMMAND CMD)
        {
            this.BeginInvoke((MethodInvoker)delegate
            {
                switch (CMD)
                {
                    case CM.COMMAND.DIS_CONNECT_CMD:
                        com_type.resetflag();
                        com_type.Get_Command_Send(CM.COMMAND.DIS_CONNECT_CMD);
                        com_type.waitflagRevTCP();
                        com_type.Close();
                        Disconnect_Behavior();
                        com_type.Config_Msg -= GetConfig_Handler;
                        break;
                    case CM.COMMAND.SET_CONN_TYPE_CMD:
                        byte[] conn_type_byte = new byte[1];
                        if ((int)CM.TYPECONNECT.HDR_ZIGBEE == Change_conntype_cbx.SelectedIndex )
                            //conn_type_byte[0] = (byte)0;
                            com_type.Get_Command_Power(CM.COMMAND.SET_CONN_TYPE_CMD, 0);
                        else
                            com_type.Get_Command_Power(CM.COMMAND.SET_CONN_TYPE_CMD, (byte)(Change_conntype_cbx.SelectedIndex + 1));
                            //conn_type_byte[0] = (byte)(Change_conntype_cbx.SelectedIndex + 1);
                        //com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_CONN_TYPE_CMD, conn_type_byte);
                        break;
                    case CM.COMMAND.SET_PLAN_CMD:
                        com_type.resetflag();
                        com_type.Get_Command_Power(CM.COMMAND.SET_POWER_MODE_CMD, (byte)power_mode_cbx.SelectedIndex);
                        com_type.waitflagRevTCP();
                        com_type.resetflag();
                        com_type.Get_Command_Power(CM.COMMAND.SET_REGION_CMD, (byte)region_lst.SelectedIndex);
                        com_type.waitflagRevTCP();
                        com_type.resetflag();
                        com_type.Set_Command_Send(CM.COMMAND.SET_PLAN_CMD, Plans_ToString(all_plans));
                        com_type.waitflagRevTCP();
                        break;
                    case CM.COMMAND.REBOOT_CMD:
                        com_type.resetflag();
                        if ((int)CM.TYPECONNECT.HDR_WIFI == Change_conntype_cbx.SelectedIndex)
                            com_type.Get_Command_Power(CM.COMMAND.REBOOT_CMD, 2);
                        else
                            com_type.Get_Command_Power(CM.COMMAND.REBOOT_CMD, 1);
                        com_type.waitflagRevTCP();
                        com_type.Close();
                        Disconnect_Behavior();
                        com_type.Config_Msg -= GetConfig_Handler;
                        ///ConnType_cbx.SelectedIndex = Change_conntype_cbx.SelectedIndex;
                        break;
                    case CM.COMMAND.SET_POWER_CMD:
                        com_type.resetflag();
                        byte[] power_bytes = new byte[2];
                        power_bytes[0] = 0;
                        power_bytes[1] = (byte)trackBar2.Value;
                        com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_POWER_CMD, power_bytes);
                        com_type.waitflagRevTCP();
                        com_type.resetflag();
                        power_bytes[0] = 1;
                        power_bytes[1] = (byte)trackBar3.Value;
                        com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_POWER_CMD, power_bytes);
                        com_type.waitflagRevTCP();
                        break;
                    case CM.COMMAND.GET_POWER_MODE_CMD:
                        com_type.resetflag();
                        com_type.Get_Command_Send(CM.COMMAND.GET_POWER_MODE_CMD);
                        com_type.waitflagRevTCP();
                        break;
                    case CM.COMMAND.GET_BLF_CMD:
                        com_type.resetflag();
                        com_type.Get_Command_Power(CM.COMMAND.GET_BLF_CMD, 0);
                        com_type.waitflagRevTCP();
                        com_type.resetflag();
                        com_type.Get_Command_Power(CM.COMMAND.GET_BLF_CMD, 1);
                        com_type.waitflagRevTCP();
                        com_type.resetflag();
                        com_type.Get_Command_Power(CM.COMMAND.GET_BLF_CMD, 2);
                        com_type.waitflagRevTCP();
                        break;
                    case CM.COMMAND.SET_BLF_CMD:
                        com_type.resetflag();
                        byte[] freq_bytes = new byte[2];
                        com_type.resetflag();
                        freq_bytes[0] = 0;
                        freq_bytes[1] = (byte)(2 * freq_cbx.SelectedIndex);
                        com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_BLF_CMD, freq_bytes);
                        com_type.waitflagRevTCP();

                        com_type.resetflag();
                        freq_bytes[0] = 1;
                        freq_bytes[1] = (byte)(coding_cbx.SelectedIndex);
                        com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_BLF_CMD, freq_bytes);
                        com_type.waitflagRevTCP();

                        com_type.resetflag();
                        freq_bytes[0] = 2;
                        freq_bytes[1] = (byte)(tari_cbx.SelectedIndex);
                        com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_BLF_CMD, freq_bytes);
                        com_type.waitflagRevTCP();
                        break;
                    case CM.COMMAND.SET_TAG_CONNECTION_CMD:
                        com_type.resetflag();
                        byte[] sd = Encoding.ASCII.GetBytes(Conver_Q());
                        byte[] newArray = new byte[sd.Length + 1];
                        sd.CopyTo(newArray, 1);
                        newArray[0] = 0;
                        com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_TAG_CONNECTION_CMD, newArray);
                        com_type.waitflagRevTCP();

                        com_type.resetflag();
                        sd = Encoding.ASCII.GetBytes(Session_cbx.Text);
                        newArray = new byte[sd.Length + 1];
                        sd.CopyTo(newArray, 1);
                        newArray[0] = 1;
                        com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_TAG_CONNECTION_CMD, newArray);
                        com_type.waitflagRevTCP();

                        com_type.resetflag();
                        sd = Encoding.ASCII.GetBytes(target_cbx.Text);
                        newArray = new byte[sd.Length + 1];
                        sd.CopyTo(newArray, 1);
                        newArray[0] = 2;
                        com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_TAG_CONNECTION_CMD, newArray);
                        com_type.waitflagRevTCP();
                        break;
                    case CM.COMMAND.CHECK_READER_STT_CMD:
                        com_type.Close();
                        Disconnect_Behavior();
                        com_type.Config_Msg -= GetConfig_Handler;
                        DialogResult result = MessageBox.Show("The connection closed. Please check your connection.\nIf you want re-connect, Click \"Yes\"",
                                                              "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (result == DialogResult.Yes)
                        {
                            Connect_btn.PerformClick();
                        }
                        else
                        {
                            //no...
                        }
                        break;
                    case CM.COMMAND.SET_READ_POWER_PORT_CMD:
                        com_type.resetflag();
                        com_type.Set_Command_Send(CM.COMMAND.SET_READ_POWER_PORT_CMD, Port_Power_ToString(antena_read_power_list));
                        com_type.waitflagRevTCP();
                        com_type.resetflag();
                        com_type.Set_Command_Send(CM.COMMAND.SET_WRITE_POWER_PORT_CMD, Port_Power_ToString(antena_write_power_list));
                        com_type.waitflagRevTCP();
                        break;
                    case CM.COMMAND.SET_WRITE_POWER_PORT_CMD:
                        antena_read_power_list.Clear();
                        antena_write_power_list.Clear();
                        Populate_Antena_Power();
                        com_type.resetflag();
                        com_type.Set_Command_Send(CM.COMMAND.SET_READ_POWER_PORT_CMD, "[[1,0],[2,0],[3,0],[4,0]]");
                        com_type.waitflagRevTCP();
                        com_type.resetflag();
                        com_type.Set_Command_Send(CM.COMMAND.SET_WRITE_POWER_PORT_CMD, "[[1,0],[2,0],[3,0],[4,0]]");
                        com_type.waitflagRevTCP();
                        break;
                    case CM.COMMAND.SETTING_SENSOR_CMD:
                        string sensor_data = String.Empty;
                        if (Sensor_EN_ckb.Checked)
                        {
                            if (String.IsNullOrEmpty(timeout_sensor_tx.Text)
                                || (int.Parse(timeout_sensor_tx.Text) == 0))
                            {
                                MessageBox.Show("Watching Timeout must greater than 0", "Sensor Configuration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                            else
                                sensor_data += "True,";
                        }
                        else
                        {
                            if (String.IsNullOrEmpty(time_on_tx.Text) || String.IsNullOrEmpty(time_off_tx.Text)
                                 || (int.Parse(time_on_tx.Text) == 0) || (int.Parse(time_off_tx.Text) == 0))
                            {
                                MessageBox.Show("Time onn/off must greater than 0", "Sensor Configuration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                            else
                                sensor_data += "False,";
                        }

                        sensor_data += time_on_tx.Text + "," + time_off_tx.Text + ",";// +read_sensor_ckb.Text + "," + timeout_sensor_tx.Text;
                        if (read_sensor_cb.Checked)
                            sensor_data += "True,";
                        else
                            sensor_data += "False,";
                        sensor_data += timeout_sensor_tx.Text;
                        if (com_type != null && com_type.getflagConnected_TCPIP())
                        {
                            com_type.resetflag();
                            com_type.Set_Command_Send(CM.COMMAND.SETTING_SENSOR_CMD, sensor_data);
                            com_type.waitflagRevTCP();
                            com_type.resetflag();
                            com_type.Set_Command_Send(CM.COMMAND.SET_SEND_NULL_EPC_CMD, sensor_data = (sendnull_ckb.Checked) ? "1" : "0");
                            com_type.waitflagRevTCP();
                        }
                        else
                        {
                            MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Disconnect_Behavior();
                        }
                        break;
                    case CM.COMMAND.SET_GPO_VALUE_CMD:
                        foreach (CheckBox gpo_ckb in flowLayoutPanel2.Controls.OfType<CheckBox>())
                        {
                            com_type.resetflag();
                            if (gpo_ckb.Checked)
                                com_type.Set_Command_Send(CM.COMMAND.SET_GPO_VALUE_CMD, gpo_ckb.Text.ToLower() + ":on");
                            else
                                com_type.Set_Command_Send(CM.COMMAND.SET_GPO_VALUE_CMD, gpo_ckb.Text.ToLower() + ":off");
                            com_type.waitflagRevTCP();
                        }
                        Log_Handler("Set GPO done");
                        break;
                    case CM.COMMAND.REQUEST_TAG_ID_CMD:
                        Start_Operate_btn.Enabled = false;
                        this.dataGridView1.Rows.Clear();
                        com_type.TagID_Msg += Read_handler;
                        com_type.resetflag();
                        com_type.Get_Command_Send(CM.COMMAND.REQUEST_TAG_ID_CMD);
                        com_type.waitflagRevTCP();
                        com_type.TagID_Msg -= Read_handler;
                        Start_Operate_btn.Enabled = true;
                        break;
                    case CM.COMMAND.PING_TO_HOST_CMD:
                        //status_led.Image = global::GatewayForm.Properties.Resources.blind_led2;
                        //Thread.Sleep(3000);
                        //status_led.Image = global::GatewayForm.Properties.Resources.green_led2;
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
                    case (int)CM.TYPECONNECT.HDR_ZIGBEE:
                        if (Connect_btn.Text == "Connect")
                        {
                            Log_lb.Text = "Connecting ...";
                            com_type = new Communication(CM.TYPECONNECT.HDR_ZIGBEE);
                            if (!String.IsNullOrEmpty(connect_form.address))
                            {
                                com_type.Config_Msg += GetConfig_Handler;
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
                                    com_type.Config_Msg -= GetConfig_Handler;
                                }
                            }
                            else
                            {
                                MessageBox.Show("Please config Zigbee property", "Setup Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                Log_lb.Text = "Idle";
                            }
                        }
                        else
                        {
                            com_type.Get_Command_Send(CM.COMMAND.DIS_CONNECT_CMD);
                            com_type.Close();
                            Disconnect_Behavior();
                            com_type.Config_Msg -= GetConfig_Handler;
                        }
                        break;
                    //wifi
                    case (int)CM.TYPECONNECT.HDR_WIFI:
                        if (Connect_btn.Text == "Connect")
                        {
                            Log_lb.Text = "Connecting ...";
                            com_type = new Communication(CM.TYPECONNECT.HDR_WIFI);
                            if (!String.IsNullOrEmpty(connect_form.address))
                            {
                                com_type.Config_Msg += GetConfig_Handler;
                                com_type.Connect(connect_form.address, int.Parse(connect_form.port));
                                if (com_type != null && com_type.getflagConnected_TCPIP())
                                {
                                    Connected_Behavior();
                                    wifi_form.address = connect_form.address;
                                    //wifi_form.port = connect_form.port;
                                }
                                else
                                {
                                    Log_lb.Text = "Idle";
                                    com_type.Config_Msg -= GetConfig_Handler;
                                }
                            }
                            else
                            {
                                MessageBox.Show("Please config Wifi property", "Setup Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                Log_lb.Text = "Idle";
                            }
                        }
                        else
                        {
                            startcmdprocess(CM.COMMAND.DIS_CONNECT_CMD);
                        }
                        break;
                    //bluetooth
                    case (int)CM.TYPECONNECT.HDR_BLUETOOTH:
                        break;
                    //Ethernet
                    case (int)CM.TYPECONNECT.HDR_ETHERNET:
                        if (Connect_btn.Text == "Connect")
                        {
                            Log_lb.Text = "Connecting ...";
                            com_type = new Communication(CM.TYPECONNECT.HDR_ETHERNET);
                            if (!String.IsNullOrEmpty(connect_form.address))
                            {
                                com_type.Config_Msg += GetConfig_Handler;
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
                                    com_type.Config_Msg -= GetConfig_Handler;
                                }
                            }
                            else
                            {
                                MessageBox.Show("Please config Ethernet property", "Setup Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                Log_lb.Text = "Idle";
                            }
                        }
                        else
                        {
                            startcmdprocess(CM.COMMAND.DIS_CONNECT_CMD);
                        }
                        break;
                    //RS485
                    case (int)CM.TYPECONNECT.HDR_RS232:
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Connect Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        int couting = 0;
        private void GetConfig_Handler(string config_msg)
        {
            string[] config_str = config_msg.Split(new string[] { "\n" }, StringSplitOptions.None);

            /*if (config_str[0] == "Update FW")
            {
                this.Invoke((MethodInvoker)delegate
                {
                    if ("100" == config_str[1])
                    {
                        com_type.Close();
                        Disconnect_Behavior();
                        com_type.Config_Msg -= GetConfig_Handler;
                        MessageBox.Show("Update Firmware complete!\nDisconnect to start new update version.", "New Firmware", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        progressBar1.PerformStep();
                        Log_lb.Text = "Updating " + config_str[1] + "% ...";
                    }
                });
            }*/
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
                Load_GW_Config(config_str);
            }
            else if (config_str[0] == "Antena RFID")
            {
                this.Invoke((MethodInvoker)delegate
                {
                    anten_list = String.Empty;
                    if (config_str[1].IndexOf('1') != -1)
                    {
                        Ant1_ckb.Checked = true;
                        anten_list += "1,";
                        Ant1_plan_ckb.Enabled = true;
                    }
                    else
                    {
                        Ant1_ckb.Checked = false;
                        Ant1_plan_ckb.Enabled = false;
                    }
                    if (config_str[1].IndexOf('2') != -1)
                    {
                        Ant2_ckb.Checked = true;
                        anten_list += "2,";
                        Ant2_plan_ckb.Enabled = true;
                    }
                    else
                    {
                        Ant2_ckb.Checked = false;
                        Ant2_plan_ckb.Enabled = false;
                    }
                    if (config_str[1].IndexOf('3') != -1)
                    {
                        Ant3_ckb.Checked = true;
                        anten_list += "3,";
                        Ant3_plan_ckb.Enabled = true;
                    }
                    else
                    {
                        Ant3_ckb.Checked = false;
                        Ant3_plan_ckb.Enabled = false;
                    }
                    if (config_str[1].IndexOf('4') != -1)
                    {
                        Ant4_ckb.Checked = true;
                        anten_list += "4,";
                        Ant4_plan_ckb.Enabled = true;
                    }
                    else
                    {
                        Ant4_ckb.Checked = false;
                        Ant4_plan_ckb.Enabled = false;
                    }
                    if (!String.IsNullOrEmpty(anten_list))
                    {
                        anten_list.Remove(anten_list.Length - 1);
                        Start_Operate_btn.Enabled = true;
                    }
                    else
                    {
                        MessageBox.Show("No Antena Detected. Start/stop Inventory can not work until found at least one antena", "Warning Antena", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        Start_Operate_btn.Enabled = false;
                    }
                });
            }
            else if (config_str[0] == "Get GPI")
            {
                //string[] gpio_status = config_str[1].Split(new string[] { " " }, StringSplitOptions.None);
                this.Invoke((MethodInvoker)delegate
                {
                    for (int i = 0; i < 5; i++)
                    {
                        if (config_str[1][i] == '1')
                            (flowLayoutPanel5.Controls[i] as CheckBox).Checked = true;
                        else
                            (flowLayoutPanel5.Controls[i] as CheckBox).Checked = false;
                    }
                });
                Log_Handler("Get GPI done");
            }
            else if (config_str[0] == "Tag Setting")
            {
                if (couting == 0)
                {
                    if (config_str[1].IndexOf("Dynamic") != -1)
                    {
                        SetControl(dynamic_Q_rbtn, "yes");
                    }
                    else
                    {
                        SetControl(static_Q_rbtn, "yes");
                        SetControl(trackBar5, config_str[1].Substring(config_str[1].IndexOf('(') + 1).TrimEnd(')'));
                    }
                    SetControl(progressBar1, "35");
                    couting++;
                }
                else if (couting == 1)
                {
                    //char idx_session = config_str[1][1];
                    SetControl(Session_cbx, config_str[1][1].ToString());
                    SetControl(progressBar1, "70");
                    couting++;
                }
                else
                {
                    if (config_str[1] == "AB")
                        SetControl(target_cbx, "1");
                    else
                        SetControl(target_cbx, "0");
                    couting = 0;
                    SetControl(progressBar1, "100");
                    Log_Handler("Get Tag Connection done");
                    Enable_RFID();
                }
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
                try
                {
                    list_plan_name.Clear();
                    all_plans.plan_list.Clear();
                    all_plans = LoadReadPlans(config_str[1]);
                    PopulateTreeView(all_plans);
                }
                catch (Exception)
                {
                    MessageBox.Show("Wrong Plan Format. Please click \"+\" button to reset Read Plan Format", "Warning Read Plan", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    get_plan_btn.Enabled = false;
                    Title_TreeView();
                    Add_plan_btn.Focus();
                }
            }
            else if (config_str[0][0] == '/')
            {
                this.Invoke((MethodInvoker)delegate
                {
                    //freq
                    if (config_str[16].IndexOf("250", 15) != -1)
                        freq_cbx.SelectedIndex = 0;
                    else if (config_str[16].IndexOf("320", 15) != -1)
                        freq_cbx.SelectedIndex = 1;
                    else
                        freq_cbx.SelectedIndex = 2;
                    //coding
                    if (config_str[13].IndexOf('0', 15) != -1)
                        coding_cbx.SelectedIndex = 0;
                    else if (config_str[13].IndexOf('2', 15) != -1)
                        coding_cbx.SelectedIndex = 1;
                    else if (config_str[13].IndexOf('4', 15) != -1)
                        coding_cbx.SelectedIndex = 2;
                    else if (config_str[13].IndexOf('8', 15) != -1)
                        coding_cbx.SelectedIndex = 3;
                    //tari
                    if (config_str[17].IndexOf("6_25", 15) != -1)
                        tari_cbx.SelectedIndex = 2;
                    else if (config_str[17].IndexOf("12_5", 15) != -1)
                        tari_cbx.SelectedIndex = 1;
                    else
                        tari_cbx.SelectedIndex = 0;
                    //Q
                    if (config_str[12].IndexOf("Static") != -1)
                    {
                        static_Q_rbtn.Checked = true;
                        //trackBar5.Enabled = true;
                        trackBar5.Value = int.Parse(config_str[12].Substring(config_str[12].IndexOf('(') + 1).TrimEnd(')'));
                    }
                    else
                        dynamic_Q_rbtn.Checked = true;
                    //Session
                    if (config_str[14].IndexOf("S0") != -1)
                         Session_cbx.SelectedIndex = 0;
                    else if (config_str[14].IndexOf("S1") != -1)
                        Session_cbx.SelectedIndex = 1;
                    else if (config_str[14].IndexOf("S2") != -1)
                        Session_cbx.SelectedIndex = 2;
                    else
                        Session_cbx.SelectedIndex = 3;
                    //Target
                    if (config_str[15].IndexOf("AB") != -1)
                        target_cbx.SelectedIndex = 1;
                    else
                        target_cbx.SelectedIndex = 0;
                    
                    //readpower
                    trackBar2.Value = int.Parse(config_str[23].Substring(24, config_str[23].Length - 26));
                    //readpower
                    trackBar3.Value = int.Parse(config_str[24].Substring(25, config_str[24].Length - 27));
                    //power mode
                    if (config_str[4].IndexOf("FULL") != -1)
                        power_mode_cbx.SelectedIndex = 0;
                    else if (config_str[4].IndexOf("MIN") != -1)
                        power_mode_cbx.SelectedIndex = 1;
                    else if (config_str[4].IndexOf("MED") != -1)
                        power_mode_cbx.SelectedIndex = 2;
                    else if (config_str[4].IndexOf("MAX") != -1)
                        power_mode_cbx.SelectedIndex = 3;
                    else
                        power_mode_cbx.SelectedIndex = 4;
                    //Antena Read Power List
                    Load_ReadPort_Power(config_str[21].Substring(config_str[21].IndexOf('=') + 1));

                    //Antena Write Power List
                    Load_WritePort_Power(config_str[22].Substring(config_str[22].IndexOf('=') + 1));
                    Populate_Antena_Power();
                    //Read Plan
                    try
                    {
                        list_plan_name.Clear();
                        all_plans.plan_list.Clear();
                        all_plans = LoadReadPlans(config_str[20].Substring(config_str[20].IndexOf("Simple")));
                        PopulateTreeView(all_plans);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Wrong Plan Format. Please click \"+\" button to reset Read Plan Format", "Warning Read Plan", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        get_plan_btn.Enabled = false;
                        Title_TreeView();
                        Add_plan_btn.Focus();
                    }
                    /*Array.Clear(RFID_fixed, 0, RFID_fixed.Length);
                    for (int i = 0; i < 4; i++)
                        RFID_fixed[0] += config_str[i] + "\n";
                    for (int i = 5; i < 12; i++)
                        RFID_fixed[1] += config_str[i] + "\n";
                    for (int i = 18; i < 20; i++)
                        RFID_fixed[2] += config_str[i] + "\n";
                    for (int i = 25; i < 32; i++)
                        RFID_fixed[3] += config_str[i] + "\n";
                    for (int i = 33; i < 39; i++)
                        RFID_fixed[4] += config_str[i] + "\n";*/
                    Log_Handler("Get RFID Configuration done");
                });
            }
            else if (config_str[0] == "Port Power")
            {
                this.Invoke((MethodInvoker)delegate
                {
                    if (couting == 0)
                    {
                        Load_ReadPort_Power(config_str[1]);
                        Log_Handler("Get Port Read Power done");
                        SetControl(progressBar1, "50");
                        couting++;
                    }
                    else
                    {
                        Load_WritePort_Power(config_str[1]);
                        Log_Handler("Get Port Write Power done");
                        SetControl(progressBar1, "100");
                        Enable_RFID();
                        Populate_Antena_Power();
                        couting = 0;
                        read_port_ckb.Checked = false;
                        write_port_ckb.Checked = false;
                        trackBar1.Enabled = false;
                        trackBar4.Enabled = false;
                    }
                });
            }
            else if (config_str[0] == "Keep Alive Timeout")
            {
                startcmdprocess(CM.COMMAND.CHECK_READER_STT_CMD);
            }
            else if (config_str[0] == "Pinged")
            {
                
                //startcmdprocess(CM.COMMAND.PING_TO_HOST_CMD);
                this.Invoke((MethodInvoker)delegate
                {
                    pinged = true;
                    status_led.Image = global::GatewayForm.Properties.Resources.blind_led2;
                });
            }
            else if (config_str[0] == "Set Port")
            {
                startcmdprocess(CM.COMMAND.SET_CONN_TYPE_CMD);
            }
            else if (config_str[0] == "Change Protocol")
            {
                //MessageBox.Show(config_str[1], "New Protocol Port Property", MessageBoxButtons.OK);
                startcmdprocess(CM.COMMAND.REBOOT_CMD);
            }
            else if (config_str[0] == "NAK")
                MessageBox.Show("Get Configuration Failed", "Error Get Command", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
                MessageBox.Show("Get Command not defined", "Error Get Command", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void Load_ReadPort_Power(string read_format)
        {
            antena_read_power_list.Clear();
            if (read_format == "[]")
            {
                if (anten_list.IndexOf('1') != -1)
                    antena_read_power_list.Add(1, 100 * trackBar2.Value);
                if (anten_list.IndexOf('2') != -1)
                    antena_read_power_list.Add(2, 100 * trackBar2.Value);
                if (anten_list.IndexOf('3') != -1)
                    antena_read_power_list.Add(3, 100 * trackBar2.Value);
                if (anten_list.IndexOf('4') != -1)
                    antena_read_power_list.Add(4, 100 * trackBar2.Value);
            }
            else
            {
                int anten_index;
                if ((anten_list.IndexOf('1') != -1) && ((anten_index = read_format.IndexOf("[1,")) != -1))
                    antena_read_power_list.Add(1, Cut_String(read_format, anten_index + 3));
                if ((anten_list.IndexOf('2') != -1) && ((anten_index = read_format.IndexOf("[2,")) != -1))
                    antena_read_power_list.Add(2, Cut_String(read_format, anten_index + 3));
                if ((anten_list.IndexOf('3') != -1) && ((anten_index = read_format.IndexOf("[3,")) != -1))
                    antena_read_power_list.Add(3, Cut_String(read_format, anten_index + 3));
                if ((anten_list.IndexOf('4') != -1) && ((anten_index = read_format.IndexOf("[4,")) != -1))
                    antena_read_power_list.Add(4, Cut_String(read_format, anten_index + 3));
            }
        }

        private void Load_WritePort_Power(string write_format)
        {
            antena_write_power_list.Clear();
            if (write_format == "[]")
            {
                if (anten_list.IndexOf('1') != -1)
                    antena_write_power_list.Add(1, 100 * trackBar3.Value);
                if (anten_list.IndexOf('2') != -1)               
                    antena_write_power_list.Add(2, 100 * trackBar3.Value);
                if (anten_list.IndexOf('3') != -1)               
                    antena_write_power_list.Add(3, 100 * trackBar3.Value);
                if (anten_list.IndexOf('4') != -1)               
                    antena_write_power_list.Add(4, 100 * trackBar3.Value);
            }
            else
            {
                int anten_index;
                if ((anten_list.IndexOf('1') != -1) && ((anten_index = write_format.IndexOf("[1,")) != -1))
                    antena_write_power_list.Add(1, Cut_String(write_format, anten_index + 3));
                if ((anten_list.IndexOf('2') != -1) && ((anten_index = write_format.IndexOf("[2,")) != -1))
                    antena_write_power_list.Add(2, Cut_String(write_format, anten_index + 3));
                if ((anten_list.IndexOf('3') != -1) && ((anten_index = write_format.IndexOf("[3,")) != -1))
                    antena_write_power_list.Add(3, Cut_String(write_format, anten_index + 3));
                if ((anten_list.IndexOf('4') != -1) && ((anten_index = write_format.IndexOf("[4,")) != -1))
                    antena_write_power_list.Add(4, Cut_String(write_format, anten_index + 3));
            }
        }

        private string Port_Power_ToString(Dictionary<int, int> power_list)
        {
            string port_power = "[";
            if (power_list.Count > 0)
            {
                foreach (KeyValuePair<int, int> port in power_list)
                    port_power += "[" + port.Key.ToString() + "," + port.Value.ToString() + "],";
                port_power = port_power.Remove(port_power.Length - 1, 1) + "]";
            }
            else
                port_power += "]";
            return port_power;
        }

        private int Cut_String(string format, int index)
        {
            string svalue = String.Empty;
            int value;
            for (int inext = index; inext < format.Length; inext++)
            {
                if (format[inext] == ']')
                    break;
                else
                    svalue += format[inext];
            }
            if (!int.TryParse(svalue, out value))
                value = 3000;
            return value;
        }

        private void Populate_Antena_Power()
        {
            this.Invoke((MethodInvoker)delegate
            {
                Antena_cbx.Items.Clear();
                Antena_cbx.Text = "ANTx";
                if (antena_read_power_list.Count > 0) 
                {
                    foreach (KeyValuePair<int, int> antena in antena_read_power_list)
                        Antena_cbx.Items.Add("ANT" + antena.Key.ToString());
                    //trackBar1.Enabled = true;
                    //trackBar4.Enabled = true;
                }
                else
                {
                    read_port_ckb.Checked = false;
                    write_port_ckb.Checked = false;
                    trackBar1.Enabled = false;
                    trackBar4.Enabled = false;
                }
            });
        }

        private void Load_GW_Config(string[] config_str)
        {
            this.Invoke((MethodInvoker)delegate
            {
                //Gateway serial
                Gateway_ID_lb.Text = config_str[1].Substring(config_str[1].IndexOf("=") + 1);
                Gateway_ID_tx.Text = config_str[1].Substring(config_str[1].IndexOf("=") + 1);
                //Hardware version
                HW_Verrsion_tx.Text = config_str[2].Substring(config_str[2].IndexOf("=") + 1);
                //Software version
                SW_Version_tx.Text = config_str[3].Substring(config_str[3].IndexOf("=") + 1);
                //Connection support
                /*for (int i = 0; i < ConnectionList_ck.Items.Count; i++)
                    ConnectionList_ck.SetItemCheckState(i, CheckState.Checked);

                if (!config_str[4].Contains("Zigbee"))
                    ConnectionList_ck.SetItemCheckState(0, CheckState.Unchecked);
                if (!config_str[4].Contains("Wifi"))
                    ConnectionList_ck.SetItemCheckState(1, CheckState.Unchecked);
                if (!config_str[4].Contains("Bluetooth"))
                    ConnectionList_ck.SetItemCheckState(2, CheckState.Unchecked);
                if (!config_str[4].Contains("Ethernet"))
                    ConnectionList_ck.SetItemCheckState(3, CheckState.Unchecked);
                if (!config_str[4].Contains("RS485"))
                    ConnectionList_ck.SetItemCheckState(4, CheckState.Unchecked);*/
                //Audio support
                setCheckBox(config_str[6], AudioSupport_cbx);
                AudioVolume_trb.Value = int.Parse(config_str[7].Substring(19, config_str[7].Length - 21));
                //Led support
                setCheckBox(config_str[8], LED_Support_ckb);
                //Pallet support
                setCheckBox(config_str[9], PalletSupport_cbx);
                //Pallet ID
                if (!config_str[10].Contains(','))
                {
                    Mode_pallet_pattern_cbx.SelectedIndex = 0;
                    PatternID_tx.Text = config_str[10].Substring(config_str[10].IndexOf("=") + 1);
                }
                else
                {
                    Mode_pallet_pattern_cbx.SelectedIndex = 1;
                    if (config_str[10].Contains("true"))
                        Invert_ckb.Checked = true;
                    else Invert_ckb.Checked = false;
                    string[] mode_field = config_str[10].Split(new string[] { "," }, StringSplitOptions.None);
                    if (mode_field[1] == "TMR_GEN2_BANK_EPC")
                        bank_cbx.SelectedIndex = 0;
                    else if (mode_field[1] == "TMR_GEN2_BANK_USER")
                        bank_cbx.SelectedIndex = 1;
                    else bank_cbx.SelectedIndex = 2;
                    start_bit_tx.Text = mode_field[2];
                    bit_length_tx.Text = mode_field[3];
                    PatternID_tx.Text = mode_field[4];
                }
                //Offline mode
                setCheckBox(config_str[11], Offline_ckb);
                //RFID API Support
                setCheckBox(config_str[12], RFID_API_ckb);
                //Message queue time interval
                MessageInterval_tx.Text = config_str[13].Substring(config_str[13].IndexOf("=") + 1);
                //Stack Light Support
                setCheckBox(config_str[14], StackLight_ckb);
                //Stack Light GPO
                if (config_str[15].Contains("gpo0"))
                    GPO0_ckb.Checked = true;
                else GPO0_ckb.Checked = false;
                if (config_str[15].Contains("gpo1"))
                    GPO1_ckb.Checked = true;
                else GPO1_ckb.Checked = false;
                if (config_str[15].Contains("gpo2"))
                    GPO2_ckb.Checked = true;
                else GPO2_ckb.Checked = false;
                if (config_str[15].Contains("gpo3"))
                    GPO3_ckb.Checked = true;
                else GPO3_ckb.Checked = false;
                if (config_str[15].Contains("gpo4"))
                    GPO4_ckb.Checked = true;
                else GPO4_ckb.Checked = false;

                //Read Sensor
                setCheckBox(config_str[16], Sensor_EN_ckb);
                time_off_tx.Text = config_str[17].Substring(config_str[17].IndexOf("=") + 1);
                time_on_tx.Text = config_str[18].Substring(config_str[18].IndexOf("=") + 1);
                setCheckBox(config_str[19], read_sensor_cb);
                timeout_sensor_tx.Text = config_str[20].Substring(config_str[20].IndexOf("=") + 1);
                setCheckBox(config_str[21], sendnull_ckb);
                Log_Handler("Get GW Config done");
            });
        }

        private void Log_Handler(string log_msg)
        {
            this.Invoke((MethodInvoker)delegate
            {
                Log_lb.Text = log_msg;
                if (log_msg == "Inventory Mode")
                {
                    Stop_Behavior();
                    timer1.Interval = 1000;
                    timer1.Start();
                    //ping_timer.Start();
                }
                else if (log_msg == "Stop Inventory")
                {
                    Start_Behavior();
                    ptimer_loghandle.Interval = 500;
                    ptimer_loghandle.Start();
                }
                else if (log_msg[0] == 'U')
                {
                    progressBar1.PerformStep();
                    if (log_msg[9] == '0')
                    {
                        com_type.Close();
                        Disconnect_Behavior();
                        com_type.Config_Msg -= GetConfig_Handler;
                        MessageBox.Show("Update Firmware complete!\nDisconnect to start new update version.", "New Firmware", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
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
            if ("Inventory Mode" == Log_lb.Text)
            {
                ptimer_loghandle.Stop();
                MessageBox.Show("Start/Stop Inventory not working properly!\nPlease check Antena connection\nDisconnect to Gateway", "Inventory Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                startcmdprocess(CM.COMMAND.DIS_CONNECT_CMD);
                Start_Behavior();
                Start_Operate_btn.Enabled = false;
            }
            else if (Log_lb.Text == "Disconnected" || Log_lb.Text == "Abort due to close")
            {
                ptimer_loghandle.Stop();
                Log_lb.Text = "Idle";
                progressBar1.Value = 0;
                progressBar1.Visible = false;
                if (status_lb.Text == "Active")
                    Disconnect_Behavior();
            }
            else if ("Getting Protocol ..." == Log_lb.Text)
            {
                ptimer_loghandle.Stop();
                DialogResult result = MessageBox.Show("Getting BLF info not complete\nPlease click \"Yes\" for retry", "Warning Protocol Configuration", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    //progressBar1.Value = 0;
                    get_protocol_btn.Enabled = true;
                    get_protocol_btn.PerformClick();
                }
                else
                {
                    MessageBox.Show("The BLF components configuration might not right", "Warning Protocol Configuration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Enable_RFID();
                    Log_lb.Text = "Failed Protocol";
                    progressBar1.Value = 0;
                    progressBar1.Visible = false;
                }
            }
            else if ("Getting Power ..." == Log_lb.Text)
            {
                ptimer_loghandle.Stop();
                DialogResult result = MessageBox.Show("Getting Power info not complete\nPlease click \"Yes\" for retry", "Warning Power Configuration", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    //progressBar1.Value = 0;
                    get_power_btn.Enabled = true;
                    get_power_btn.PerformClick();
                }
                else
                {
                    MessageBox.Show("The Read/Write Power value might not right", "Warning Power Configuration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Enable_RFID();
                    Log_lb.Text = "Failed Power";
                    progressBar1.Value = 0;
                    progressBar1.Visible = false;
                }
            }
            else if ("Getting Port Power ..." == Log_lb.Text)
            {
                ptimer_loghandle.Stop();
                DialogResult result = MessageBox.Show("Getting Power Port List not complete\nPlease click \"Yes\" for retry", "Warning Power Configuration", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == System.Windows.Forms.DialogResult.Yes)
                {

                    //progressBar1.Value = 0;
                    get_pw_antena_btn.Enabled = true;
                    get_pw_antena_btn.PerformClick();
                }
                else
                {
                    MessageBox.Show("The Power Port value might not right", "Warning Power Port Configuration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Enable_RFID();
                    Log_lb.Text = "Failed Port Power";
                    progressBar1.Value = 0;
                    progressBar1.Visible = false;
                }
            }
            else if ("Getting Tag Contention ..." == Log_lb.Text)
            {
                ptimer_loghandle.Stop();
                DialogResult result = MessageBox.Show("Getting Tag Contention not complete\nPlease click \"Yes\" for retry", "Warning Tag Contention", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == System.Windows.Forms.DialogResult.Yes)
                {

                    //progressBar1.Value = 0;
                    get_tag_connection_btn.Enabled = true;
                    get_tag_connection_btn.PerformClick();
                }
                else
                {
                    MessageBox.Show("Tag Contention value might not right", "Warning Tag Contention Configuration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Enable_RFID();
                    Log_lb.Text = "Failed Tag Contention";
                    progressBar1.Value = 0;
                    progressBar1.Visible = false;
                }
            }
            else
            {
                ptimer_loghandle.Stop();
                progressBar1.Value = 0;
                progressBar1.Visible = false;
                Log_lb.Text = "Ready!";
                //Enable_RFID();
            }
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
                {
                    try
                    {
                        (control as ComboBox).SelectedIndex = int.Parse(config_tx);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        MessageBox.Show("Combox is out of range", "Combox Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else if (control is ProgressBar)
                {
                    (control as ProgressBar).Value = int.Parse(config_tx);
                }
                else if (control is TrackBar)
                {
                    try
                    {
                        (control as TrackBar).Value = int.Parse(config_tx);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        MessageBox.Show("Power value is out of range", "TrackBar Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else if (control is CheckBox)
                {
                    if ("yes" == config_tx)
                        (control as CheckBox).Checked = true;
                    else
                        (control as CheckBox).Checked = false;
                }
                /*else if (control is CheckedListBox)
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
                }*/
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
                    MessageBox.Show("New type control", "Warrning", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            }
        }

        private void Read_handler(string msg)
        {
            //ping_timer.Stop();
            string[] read_rows = msg.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            read_rows = read_rows.Skip(5).ToArray();
            SetData(this.dataGridView1, read_rows);
            SqlAddRowData(read_rows);
            SetControl(No_Tag_lb, read_rows.Length.ToString());
            //ping_timer.Start();
        }
        private static int CheckRowExist(String Check_Cell_0, List<string> Tag_List)
        {
            for (int irow = 0; irow < Tag_List.Count; irow++)
            {
                if (Tag_List[irow] == Check_Cell_0)
                    return irow;
            }
            return -1;
        }

        private delegate void SetTextDelegate(DataGridView table, String[] rows);
        
        private void SetData(DataGridView table, String[] read_rows)
        {
            if (table.InvokeRequired)
            {
                table.Invoke(new SetTextDelegate(SetData), table, read_rows);
            }
            else
            {
                table.Rows.Clear();
                string[] cells = new string[5];
                for (int iread = 0; iread < read_rows.Length; iread++)
                {
                    cells = read_rows[iread].Split(new string[] { "\t" }, StringSplitOptions.None);
                    table.Rows.Add(cells[0], cells[1], cells[2], cells[3], cells[4]);
                }
            }
        }

        private void SetText(DataGridView table, String[] read_rows)
        {
            if (table.InvokeRequired)
            {
                table.Invoke(new SetTextDelegate(SetText), table, read_rows);
            }
            else
            {
                string[] cells = new string[5];
                int lenght_not_add = table.Rows.Count;
                if (lenght_not_add > 0)
                {
                    //update and add new data
                    for (int iread = 0; iread < read_rows.Length; iread++)
                    {
                        cells = read_rows[iread].Split(new string[] { "\t" }, StringSplitOptions.None);
                        int index = CheckRowExist(cells[0], list_cell_0);
                        if (index != -1)
                        {
                            table[1, index].Value = cells[1];
                            table[2, index].Value = cells[2];
                            table[3, index].Value = cells[3];
                            table[4, index].Value = cells[4];
                            table.Rows[index].ReadOnly = true; //mark the update
                        }
                        else
                        {
                            table.Rows.Add(cells[0], cells[1], cells[2], cells[3], cells[4]);
                            //SqlAddRowData(cells);
                            list_cell_0.Add(cells[0]);
                        }
                    }
                    //light off row which is not in read data
                    for (int irow = 0; irow < lenght_not_add; irow++)
                    {
                        DataGridViewRow row = table.Rows[irow];
                        if (row.ReadOnly)
                        {
                            row.DefaultCellStyle.BackColor = Color.White;
                            row.ReadOnly = false;
                        }
                        else
                            row.DefaultCellStyle.BackColor = Color.Gray;
                    }
                }
                else
                {
                    for (int iread = 0; iread < read_rows.Length; iread++)
                    {
                        cells = read_rows[iread].Split(new string[] { "\t" }, StringSplitOptions.None);
                        table.Rows.Add(cells[0], cells[1], cells[2], cells[3], cells[4]);
                        //SqlAddRowData(cells);
                        list_cell_0.Add(cells[0]);
                    }
                }

            }
        }

        private void Start_Operate_btn_Click(object sender, EventArgs e)
        {
            if (com_type != null && com_type.getflagConnected_TCPIP())
            {
                if (Start_Operate_btn.Text == "Start inventory")
                {
                    Start_Operate_btn.Enabled = false;
                    com_type.TagID_Msg += Read_handler;
                    com_type.Get_Command_Send(CM.COMMAND.START_OPERATION_CMD);
                }
                else
                {
                    Start_Operate_btn.Enabled = false;
                    com_type.Get_Command_Send(CM.COMMAND.STOP_OPERATION_CMD);
                    com_type.TagID_Msg -= Read_handler;
                    list_cell_0.Clear();
                    int time_out = int.Parse(time_on_tx.Text) + int.Parse(time_off_tx.Text);
                    if (time_out > 5000)
                        ptimer_loghandle.Interval = time_out + 1000;
                    else
                        ptimer_loghandle.Interval = 5000;
                    ptimer_loghandle.Start();
                }
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Disconnect_Behavior();
            }

        }

        private void Connected_Behavior()
        {
            ConnType_cbx.Enabled = false;
            Connect_btn.Text = "Disconnect";
            status_led.Image = global::GatewayForm.Properties.Resources.green_led2;
            status_lb.Text = "Active";
            status_lb.ForeColor = Color.DarkBlue;
            //ViewConn_btn.Text = "View Port";
            //ViewConn_btn.FlatStyle = FlatStyle.Flat;
        }

        private void Disconnect_Behavior()
        {
            //Data
            list_plan_name.Clear();
            all_plans.plan_list.Clear();
            list_cell_0.Clear();
            antena_read_power_list.Clear();
            antena_write_power_list.Clear();
            //Display
            Connect_btn.Text = "Connect";
            status_led.Image = global::GatewayForm.Properties.Resources.red_led2;
            status_lb.Text = "Inactive";
            status_lb.ForeColor = SystemColors.ControlDark;
            ConnType_cbx.Enabled = true;
            //ViewConn_btn.Text = "Server IP";
            //ViewConn_btn.FlatStyle = FlatStyle.Standard;
            Start_Operate_btn.Enabled = false;
            if (Start_Operate_btn.Text == "Stop inventory")
                Start_Behavior();
            //Clear GW Config
            foreach (CheckBox com_clear in flowLayoutPanel2.Controls.OfType<CheckBox>())
            {
                (com_clear as CheckBox).Checked = false;
            }
            foreach (CheckBox com_clear in flowLayoutPanel1.Controls.OfType<CheckBox>())
            {
                (com_clear as CheckBox).Checked = false;
            }
            foreach (CheckBox com_clear in flowLayoutPanel5.Controls.OfType<CheckBox>())
            {
                (com_clear as CheckBox).Checked = false;
            }
            HW_Verrsion_tx.Text = String.Empty;
            SW_Version_tx.Text = String.Empty;
            Gateway_ID_lb.Text = String.Empty;
            Gateway_ID_tx.Text = String.Empty;
            MessageInterval_tx.Text = String.Empty;
            AudioVolume_trb.Value = 0;
            AudioSupport_cbx.Checked = false;
            Sensor_EN_ckb.Checked = false;
            read_sensor_cb.Checked = false;
            PalletSupport_cbx.Checked = false;
            LED_Support_ckb.Checked = false;
            Offline_ckb.Checked = false;
            RFID_API_ckb.Checked = false;
            StackLight_ckb.Checked = false;
            sendnull_ckb.Checked = false;
            sendNullTrigger_ckb.Checked = false;

            //RIFD
            foreach (ComboBox comm_clear in this.groupBox8.Controls.OfType<ComboBox>())
            {
                comm_clear.SelectedIndex = 0;
            }
            region_lst.SelectedIndex = 1;

            Title_TreeView();
            foreach (TextBox comm_clear in this.groupBox16.Controls.OfType<TextBox>())
            {
                comm_clear.Text = String.Empty;
            }
            foreach (RadioButton comm_clear in this.groupBox16.Controls.OfType<RadioButton>())
            {
                if (comm_clear.Checked)
                    comm_clear.Checked = false;
            }
            weight_tx.Text = String.Empty;
            foreach (CheckBox com_clear in flowLayoutPanel3.Controls)
            {
                (com_clear as CheckBox).Checked = false;
            }
            Connect_btn.Enabled = true;
            Antena_cbx.Items.Clear();
            trackBar2.Value = 5;
            trackBar3.Value = 5;
            trackBar1.Value = 5;
            trackBar4.Value = 5;
            write_port_ckb.Checked = false;
            read_port_ckb.Checked = false;
            trackBar1.Enabled = false;
            trackBar4.Enabled = false;
        }

        private void Stop_Behavior()
        {
            Start_Operate_btn.Text = "Stop inventory";
            //GW Config
            //set_gpo_btn.Enabled = false;
            //get_gpi_btn.Enabled = false;
            Set_GW_Config_btn.Enabled = false;
            Get_GW_Config_btn.Enabled = false;
            Get_RFID_btn.Enabled = false;
            Set_RFID_btn.Enabled = false;
            Connect_btn.Enabled = false;
            update_fw_btn.Enabled = false;
            set_port_btn.Enabled = false;
            get_anten_btn.Enabled = false;
            Set_sensor_btn.Enabled = false;
            speak_btn.Enabled = false;
            //RFID 
            Block_RFID_Tab();
            Start_Operate_btn.Enabled = true;
        }

        private void Start_Behavior()
        {
            timer1.Stop();
            status_led.Image = global::GatewayForm.Properties.Resources.green_led2;
            //ping_timer.Stop();
            sec = 0;
            time_duration_lb.Text = "00:00:00";
            Start_Operate_btn.Text = "Start inventory";
            //GW Config
            get_gpi_btn.Enabled = true;
            set_gpo_btn.Enabled = true;
            Set_GW_Config_btn.Enabled = true;
            Get_GW_Config_btn.Enabled = true;
            Get_RFID_btn.Enabled = true;
            Set_RFID_btn.Enabled = true;
            Connect_btn.Enabled = true;
            update_fw_btn.Enabled = true;
            set_port_btn.Enabled = true;
            get_anten_btn.Enabled = true;
            Set_sensor_btn.Enabled = true;
            speak_btn.Enabled = true;
            //RFID
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
            foreach (Button comm_btn in this.groupBox11.Controls.OfType<Button>())
            {
                comm_btn.Enabled = false;
            }
            foreach (Button comm_btn in this.groupBox17.Controls.OfType<Button>())
            {
                comm_btn.Enabled = false;
            }
            foreach (Button comm_btn in this.groupBox20.Controls.OfType<Button>())
            {
                comm_btn.Enabled = false;
            }
            foreach (Button comm_btn in this.groupBox21.Controls.OfType<Button>())
            {
                comm_btn.Enabled = false;
            }
            foreach (Control comm_btn in this.groupBox14.Controls)
            {
                comm_btn.Enabled = false;
            }
            Get_RFID_btn.Enabled = false;
            Set_RFID_btn.Enabled = false;
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
                foreach (Button comm_btn in this.groupBox11.Controls.OfType<Button>())
                {
                    comm_btn.Enabled = true;
                }
                foreach (Button comm_btn in this.groupBox9.Controls.OfType<Button>())
                {
                    comm_btn.Enabled = true;
                }
                foreach (Button comm_btn in this.groupBox17.Controls.OfType<Button>())
                {
                    comm_btn.Enabled = true;
                }
                foreach (Button comm_btn in this.groupBox20.Controls.OfType<Button>())
                {
                    comm_btn.Enabled = true;
                }
                foreach (Button comm_btn in this.groupBox21.Controls.OfType<Button>())
                {
                    comm_btn.Enabled = true;
                }
                foreach (Control comm_btn in this.groupBox14.Controls)
                {
                    comm_btn.Enabled = true;
                }
                Get_RFID_btn.Enabled = true;
                Set_RFID_btn.Enabled = true;
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
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Disconnect_Behavior();
            }
        }

        private void Set_GW_Config_btn_Click(object sender, EventArgs e)
        {
            if (com_type != null && com_type.getflagConnected_TCPIP())
            {
                string[] GW_Format = new string[22] {
                 "Seldatinc gateway configuration=\n",
                 "Gateway serial={0}\n",
                 "Hardware version={0}\n",
                 "Software version={0}\n",
                 "Connection support=Zigbee,Wifi,Bluetooth,Ethernet,RS485\n",
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
                 "Stack light GPIO={0}\n",
                 "Sensor Support={0}\n",
                 "AsyncTimeOff={0}\n",
                 "AsyncTimeOn={0}\n",
                 "ReadContinous={0}\n",
                 "SensorWatchingTimeout={0}\n",
                 "SendNullEPC={0}\n",
                };

                StringBuilder gateway_config = new StringBuilder();
                gateway_config.Append(GW_Format[0]);
                gateway_config.AppendFormat(GW_Format[1], Gateway_ID_tx.Text);
                gateway_config.AppendFormat(GW_Format[2], HW_Verrsion_tx.Text);
                gateway_config.AppendFormat(GW_Format[3], SW_Version_tx.Text);
                string connections = String.Empty;
                /*foreach (var item_conn in ConnectionList_ck.CheckedItems)
                    connections += item_conn.ToString() + ",";*/

                gateway_config.Append(GW_Format[4]);
                gateway_config.AppendFormat(GW_Format[5], ConnType_cbx.Text);

                gateway_config.AppendFormat(GW_Format[6], convertCheckBox(AudioSupport_cbx));
                gateway_config.AppendFormat(GW_Format[7], AudioVolume_trb.Value.ToString());
                gateway_config.AppendFormat(GW_Format[8], convertCheckBox(LED_Support_ckb));
                gateway_config.AppendFormat(GW_Format[9], convertCheckBox(PalletSupport_cbx));
                if (Mode_pallet_pattern_cbx.SelectedIndex == 0)
                    gateway_config.AppendFormat(GW_Format[10], PatternID_tx.Text);
                else
                {
                    string pallet_str = String.Empty;
                    if (Invert_ckb.Checked)
                        pallet_str += "true,";
                    else
                        pallet_str += "false,";
                    if (bank_cbx.SelectedIndex == 0)
                        pallet_str += "TMR_GEN2_BANK_EPC,";
                    else if (bank_cbx.SelectedIndex == 1)
                        pallet_str += "TMR_GEN2_BANK_USER,";
                    else pallet_str += "TMR_GEN2_BANK_TID,";
                    pallet_str += start_bit_tx.Text + ",";
                    pallet_str += bit_length_tx.Text + ",";
                    pallet_str += PatternID_tx.Text;
                    gateway_config.AppendFormat(GW_Format[10], pallet_str);
                }
                gateway_config.AppendFormat(GW_Format[11], convertCheckBox(Offline_ckb));
                gateway_config.AppendFormat(GW_Format[12], convertCheckBox(RFID_API_ckb));
                gateway_config.AppendFormat(GW_Format[13], MessageInterval_tx.Text);
                gateway_config.AppendFormat(GW_Format[14], convertCheckBox(StackLight_ckb));
                string GPO_sets = String.Empty;
                foreach (CheckBox GPOs_ckb in flowLayoutPanel2.Controls.OfType<CheckBox>())
                {
                    if (GPOs_ckb.Checked)
                        GPO_sets += GPOs_ckb.Text.ToLower() + ",";
                }
                gateway_config.AppendFormat(GW_Format[15], GPO_sets.Remove(GPO_sets.Length - 1));
                gateway_config.AppendFormat(GW_Format[16], convertCheckBox(Sensor_EN_ckb));
                gateway_config.AppendFormat(GW_Format[17], time_off_tx.Text);
                gateway_config.AppendFormat(GW_Format[18], time_on_tx.Text);
                gateway_config.AppendFormat(GW_Format[19], convertCheckBox(read_sensor_cb));
                gateway_config.AppendFormat(GW_Format[20], timeout_sensor_tx.Text);
                gateway_config.AppendFormat(GW_Format[21], convertCheckBox(sendnull_ckb));
                com_type.Set_Command_Send(CM.COMMAND.SET_CONFIGURATION_CMD, gateway_config.ToString());
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Disconnect_Behavior();
            }
        }

        private void ViewConn_btn_Click(object sender, EventArgs e)
        {
            connect_form.ShowDialog();
        }

        private void Get_RFID_btn_Click(object sender, EventArgs e)
        {
            if (com_type != null && com_type.getflagConnected_TCPIP())
            {
                com_type.Get_Command_Send(CM.COMMAND.GET_RFID_CONFIGURATION_CMD);
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Disconnect_Behavior();
            }
        }

        private void ConnType_cbx_SelectedIndexChanged(object sender, EventArgs e)
        {
            connect_form.ShowDialog();
        }

        private void get_power_btn_Click(object sender, EventArgs e)
        {
            if (com_type != null && com_type.getflagConnected_TCPIP())
            {
                /*com_type.Get_Command_Power(CM.COMMAND.GET_POWER_CMD, 0);
                com_type.waitflagRevTCP();
                com_type.Get_Command_Power(CM.COMMAND.GET_POWER_CMD, 1);
                com_type.waitflagRevTCP();*/
                couting = 0;
                progressBar1.Visible = true;
                progressBar1.Value = 0;
                Log_lb.Text = "Getting Power ...";
                Block_RFID_Tab();
                com_type.StartCmd_Process(CM.COMMAND.GET_POWER_CMD);
                ptimer_loghandle.Interval = 4000;
                ptimer_loghandle.Start();
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Disconnect_Behavior();
            }
        }

        private void set_power_btn_Click(object sender, EventArgs e)
        {
            if (com_type != null && com_type.getflagConnected_TCPIP())
            {
                startcmdprocess(CM.COMMAND.SET_POWER_CMD);
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Disconnect_Behavior();
            }
        }

        private void set_region_btn_Click(object sender, EventArgs e)
        {
            if (com_type != null && com_type.getflagConnected_TCPIP())
            {
                //byte[] region_byte = new byte[1];
                //region_byte[0] = (byte)region_lst.SelectedIndex;
                //com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_REGION_CMD, region_byte);
                com_type.Get_Command_Power(CM.COMMAND.SET_REGION_CMD, (byte)region_lst.SelectedIndex);
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Disconnect_Behavior();
            }
        }

        private void set_power_mode_btn_Click(object sender, EventArgs e)
        {
            if (com_type != null && com_type.getflagConnected_TCPIP())
            {
                //byte[] pw_mode_byte = new byte[1];
                //pw_mode_byte[0] = (byte)power_mode_cbx.SelectedIndex;
                //com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_POWER_MODE_CMD, pw_mode_byte);
                com_type.Get_Command_Power(CM.COMMAND.SET_POWER_MODE_CMD, (byte)power_mode_cbx.SelectedIndex);
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                progressBar1.Visible = true;
                progressBar1.Value = 0;
                Log_lb.Text = "Getting Protocol ...";
                Block_RFID_Tab();
                ptimer_loghandle.Interval = 5000;
                ptimer_loghandle.Start();
                com_type.StartCmd_Process(CM.COMMAND.GET_BLF_CMD);
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Disconnect_Behavior();
            }
        }

        private void set_protocol_btn_Click(object sender, EventArgs e)
        {
            if (com_type != null && com_type.getflagConnected_TCPIP())
            {
                startcmdprocess(CM.COMMAND.SET_BLF_CMD);
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Disconnect_Behavior();
            }
        }

        private void AudioSupport_cbx_CheckedChanged(object sender, EventArgs e)
        {
            if (!AudioSupport_cbx.Checked)
            {
                AudioVolume_trb.Enabled = false;
                AudioSupport_cbx.Font = new Font(AudioSupport_cbx.Font, FontStyle.Italic);
            }
            else
            {
                AudioSupport_cbx.Font = new Font(AudioSupport_cbx.Font, FontStyle.Bold);
                AudioVolume_trb.Enabled = true;
            }
        }

        private void PalletSupport_cbx_CheckedChanged(object sender, EventArgs e)
        {
            if (PalletSupport_cbx.Checked)
            {
                PalletSupport_cbx.Font = new Font(PalletSupport_cbx.Font, FontStyle.Bold);
                Mode_pallet_pattern_cbx.Enabled = true;
                mask_pallet_id_lb.Enabled = true;
                Invert_ckb.Enabled = true;
                bit_length_tx.Enabled = true;
                bit_length_lb.Enabled = true;
                PatternID_tx.Enabled = true;
                bank_lb.Enabled = true;
                start_bit_tx.Enabled = true;
                start_bit_lb.Enabled = true;
                bank_cbx.Enabled = true;
            }
            else
            {
                PalletSupport_cbx.Font = new Font(PalletSupport_cbx.Font, FontStyle.Italic);
                Mode_pallet_pattern_cbx.Enabled = false;
                mask_pallet_id_lb.Enabled = false;
                Invert_ckb.Enabled = false;
                bit_length_tx.Enabled = false;
                bit_length_lb.Enabled = false;
                PatternID_tx.Enabled = false;
                bank_lb.Enabled = false;
                start_bit_tx.Enabled = false;
                start_bit_lb.Enabled = false;
                bank_cbx.Enabled = false;
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
                    }
                    else
                    {
                        MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Disconnect_Behavior();
                    }
                }
            }
        }
        private void LoadDatatoTablefromDBbrowser(String[] mgs)
        {
            Table_dbbrowser_datagrid.Rows.Add(mgs[0], mgs[1], mgs[2], mgs[3], mgs[4]);
        }

        private void btn_downloadtabletoExcel_Click(object sender, EventArgs e)
        {
            if (plog != null)
            {

                Thread p = new Thread(() => loadExeldata());
                p.Start();
            }
        }
        public void loadExeldata()
        {
            this.Invoke((MethodInvoker)delegate
            {
                plog.DownloadExelFile();
            });

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

                Thread p = new Thread(() => searchdata(field, data));
                p.Start();

            }
        }
        public void searchdata(String field, String data)
        {
            this.Invoke((MethodInvoker)delegate
            {
                plog.SearchDataINSql(field, data);
            });

        }

        public void SqlAddRowData(String[] rows)
        {
            if (plog != null)
            {
                Thread p = new Thread(() => insertdata(rows));
                p.Start();

            }
        }
        public void insertdata(String[] rows)
        {
            this.Invoke((MethodInvoker)delegate
            {
                plog.InsertData2Sql(rows);
            });
        }
        private void updateProgress(int percent)
        {
            Thread thread_update = new Thread(() =>
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        progressBar1.Value = percent;
                    });
                }
            );
            thread_update.Start();
        }
        private void Set_RFID_btn_Click(object sender, EventArgs e)
        {
            if (com_type.getflagConnected_TCPIP())
            {
                //String RFID_Config = String.Empty;
                /*StringBuilder rfid_config = new StringBuilder();
                rfid_config.Append(RFID_fixed[0]);
                rfid_config.Append("/reader/powerMode=" + power_mode_cbx.Text.Substring(0, power_mode_cbx.Text.IndexOf(" ") + 1) + "\n");
                rfid_config.Append(RFID_fixed[1]);
                rfid_config.Append("/reader/gen2/q=" + Conver_Q() + "\n");
                rfid_config.Append("/reader/gen2/tagEncoding=" + coding_cbx.Text + "\n");
                rfid_config.Append("/reader/gen2/session=" + Session_cbx.Text + "\n");
                rfid_config.Append("/reader/gen2/target=" + target_cbx.Text + "\n");
                rfid_config.Append("/reader/gen2/BLF=LINK" + freq_cbx.Text + "KHZ\n");
                rfid_config.Append("/reader/gen2/tari=TARI_" + Convert_Tari(tari_cbx.Text) + "US\n");
                rfid_config.Append(RFID_fixed[2]);
                //rfid_config.Append("/reader/read/asyncOffTime=" + time_off_tx + "\n");
                //rfid_config.Append("/reader/read/asyncOnTime=" + time_on_tx + "\n");
                rfid_config.Append("/reader/read/plan=" + Plans_ToString(all_plans) + "\n");
                rfid_config.Append("/reader/radio/portReadPowerList=" + Port_Power_ToString(antena_read_power_list) + "\n");
                rfid_config.Append("/reader/radio/portWritePowerList=" + Port_Power_ToString(antena_write_power_list) + "\n");
                rfid_config.Append("/reader/radio/readPower=" + trackBar2.Value + "00\n");
                rfid_config.Append("/reader/radio/writePower=" + trackBar3.Value + "00\n");
                rfid_config.Append(RFID_fixed[3]);
                rfid_config.Append("/reader/region/id=" + region_lst.SelectedIndex + "\n");
                rfid_config.Append(RFID_fixed[4]);
                /*RFID_Config = 
                    //"/reader/baudRate=115200\n/reader/probeBaudRates=[9600,115200,921600,19200,38400,57600,230400,460800]\n"
                    //+ "/reader/commandTimeout=1000\n/reader/transportTimeout=1000\n"
                              RFID_fixed[0]
                              + "/reader/powerMode=" + power_mode_cbx.Text + "\n"
                    //+ "/reader/antenna/checkPort=false\n"
                    //+ "/reader/antenna/portSwitchGpos=[]\nreader/antenna/settlingTimeList=[]\n"
                    //+ "/reader/antenna/txRxMap=[[1,1,1],[2,2,2],[3,3,3],[4,4,4]]\n"
                    //+ "/reader/gpio/inputList=[1,2,3,4]\n/reader/gpio/outputList=[]\n"
                    //+ "/reader/gen2/accessPassword=0\n/reader/gen2/q=DynamicQ\n"
                              + RFID_fixed[1]
                              + "/reader/gen2/q=" + Conver_Q() + "\n"
                              + "/reader/gen2/tagEncoding=" + coding_cbx.Text + "\n"
                              + "/reader/gen2/session=" + Session_cbx.Text +"\n"
                              + "/reader/gen2/target=" + target_cbx.Text + "\n"
                              + "/reader/gen2/BLF=LINK" + freq_cbx.Text + "KHZ\n"
                              + "/reader/gen2/tari=TARI_" + Convert_Tari(tari_cbx.Text) + "US\n"
                              + "/reader/read/asyncOffTime=" + time_off_tx + "\n"
                              + "/reader/read/asyncOnTime=" + time_on_tx + "\n"
                              + "/reader/read/plan=" + Plans_ToString(all_plans) + "\n"
                              + "/reader/radio/portReadPowerList=" + Port_Power_ToString(antena_read_power_list) + "\n"
                              + "/reader/radio/portWritePowerList=" + Port_Power_ToString(antena_write_power_list) + "\n"
                              + "/reader/radio/readPower=" + trackBar2.Value + "00\n"
                              + "/reader/radio/writePower=" + trackBar3.Value + "00\n"
                              + RFID_fixed[2]
                    //+ "/reader/tagReadData/recordHighestRssi=false\n/reader/tagReadData/reportRssiInDbm=true\n"
                    //+ "/reader/tagReadData/uniqueByAntenna=true\n/reader/tagReadData/uniqueByData=false\n"
                    //+ "/reader/tagop/protocol=GEN2\n"
                    //+ "/reader/region/hopTable=[918250,923250,913250,905250,923750,912750,918750,926250,921250,905750,915250,904750,911250,916750,926750,921750,913750,925250,910750,916250,922750,904250,917250,909750,903750,911750,906250,919750,927250,922250,907250,920750,909250,925750,920250,914750,908750,924750,915750,910250,903250,908250,919250,924250,914250,902750,907750,917750,906750,912250]\n"
                    //+ "/reader/region/hopTime=375\n"
                              + "/reader/region/id=" + region_lst.SelectedIndex + "\n"
                              + RFID_fixed[3];
                    //+ "/reader/status/antennaEnable=false\n/reader/status/frequencyEnable=false\n/reader/status/temperatureEnable=false\n"
                    //+ "/reader/tagReadData/enableReadFilter=true\n/reader/tagReadData/readFilterTimeout=0\n/reader/tagReadData/uniqueByProtocol=true\n";
                */
                //com_type.Set_Command_Send(CM.COMMAND.SET_RFID_CONFIGURATION_CMD, rfid_config.ToString());
                progressBar1.Visible = true;
                progressBar1.Value = 0;
                Log_lb.Text = "Setting RFID...";
                Block_RFID_Tab();
                Thread blf_process = new Thread(() => cmdprocess(CM.COMMAND.SET_BLF_CMD));
                blf_process.Start();
                blf_process.Join();
                //startcmdprocess(CM.COMMAND.SET_BLF_CMD); //3
                //pThreadCmd.Join(10000);
                updateProgress(25);
                Thread tag_process = new Thread(() => cmdprocess(CM.COMMAND.SET_TAG_CONNECTION_CMD));
                tag_process.Start();
                tag_process.Join();
                //startcmdprocess(CM.COMMAND.SET_TAG_CONNECTION_CMD);//3
                //pThreadCmd.Join(7000);
                updateProgress(50);
                Thread power_process = new Thread(() => cmdprocess(CM.COMMAND.SET_POWER_CMD));
                power_process.Start();
                power_process.Join();
                //startcmdprocess(CM.COMMAND.SET_POWER_CMD);//2
                //pThreadCmd.Join(4000);
                updateProgress(65);
                Thread port_process = new Thread(() => cmdprocess(CM.COMMAND.SET_READ_POWER_PORT_CMD));
                port_process.Start();
                port_process.Join();
                //startcmdprocess(CM.COMMAND.SET_READ_POWER_PORT_CMD);//2
                //pThreadCmd.Join(4000);
                updateProgress(80);
                Thread gernale = new Thread(() => cmdprocess(CM.COMMAND.SET_PLAN_CMD));
                gernale.Start();
                gernale.Join();
                //startcmdprocess(CM.COMMAND.SET_PLAN_CMD);//3
                //pThreadCmd.Join();
                updateProgress(100);
                
                //something wrong here
                Enable_RFID();
                progressBar1.Visible = false;
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Disconnect_Behavior();
            }
        }

        private string Conver_Q()
        {
            if (dynamic_Q_rbtn.Checked)
            {
                return "DynamicQ";
            }
            else
            {
                return "StaticQ(" + trackBar5.Value.ToString() + ")";
            }
        }
        private string Convert_Tari(string tari)
        {
            StringBuilder sb = new StringBuilder(tari);
            int idx = tari.IndexOf('.');
            if (idx != -1)
            {
                sb[idx] = '_';
                return sb.ToString();
            }
            else
            {
                return sb.ToString();
            }
        }

        private void set_port_btn_Click(object sender, EventArgs e)
        {
            switch (Change_conntype_cbx.SelectedIndex)
            {
                case (int)CM.TYPECONNECT.HDR_ZIGBEE:
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

                        DialogResult result = MessageBox.Show(zigbee_config, "Confirm Zigbee Port", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (result == DialogResult.Yes)
                        {
                            byte[] sd = Encoding.ASCII.GetBytes(zigbee_config);
                            byte[] newArray = new byte[sd.Length + 1];
                            sd.CopyTo(newArray, 1);
                            newArray[0] = 0;
                            com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_PORT_PROPERTIES_CMD, newArray);
                        }
                        else
                        {
                            MessageBox.Show("Zigbee Port Property not set", "Cancel Confirm", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                        MessageBox.Show("Please config Zigbee Port", "Warning Zigbee Configuration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case (int)CM.TYPECONNECT.HDR_WIFI:
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

                        DialogResult result = MessageBox.Show(wifi_config, "Confirm Wifi Port", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (result == DialogResult.Yes)
                        {
                            byte[] sd = Encoding.ASCII.GetBytes(wifi_config);
                            byte[] newArray = new byte[sd.Length + 1];
                            sd.CopyTo(newArray, 1);
                            newArray[0] = 1;
                            com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_PORT_PROPERTIES_CMD, newArray);
                        }
                        else
                        {
                            MessageBox.Show("Wifi Port Property not set", "Cancel Confirm", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                        MessageBox.Show("Please configure Wifi Port", "Warning Wifi Configuration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case (int)CM.TYPECONNECT.HDR_BLUETOOTH:
                    break;
                case (int)CM.TYPECONNECT.HDR_ETHERNET:
                    if (!String.IsNullOrEmpty(tcp_form.gateway))
                    {
                        String tcp_config = String.Empty;
                        if (tcp_form.automatic)
                            tcp_config = "gateway_tcp_configure = {\nipaddress =" + tcp_form.address
                                          + "\nhostname =" + Gateway_ID_tx.Text
                                          + "\nport =" + tcp_form.port
                                          + "\ntimeout=" + tcp_form.Timeout
                                          + "\nmax_packet_length=" + tcp_form.Length
                                          + "\ndhcp=true"
                                          + "\n}";
                        else
                            tcp_config = "gateway_tcp_configure = {\nipaddress =" + tcp_form.address
                                          + "\nhostname =" + Gateway_ID_tx.Text
                                          + "\nport =" + tcp_form.port
                                          + "\ntimeout=" + tcp_form.Timeout
                                          + "\nmax_packet_length=" + tcp_form.Length
                                          + "\ndhcp=false"
                                          + "\nnetmask=" + tcp_form.netmask
                                          + "\ngateway=" + tcp_form.gateway
                                          + "\n}";

                        DialogResult result = MessageBox.Show(tcp_config, "Confirm Ethernet Port", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (result == DialogResult.Yes)
                        {
                            byte[] sd = Encoding.ASCII.GetBytes(tcp_config);
                            byte[] newArray = new byte[sd.Length + 1];
                            sd.CopyTo(newArray, 1);
                            newArray[0] = 3;
                            com_type.Set_Command_Send_Bytes(CM.COMMAND.SET_PORT_PROPERTIES_CMD, newArray);
                        }
                        else
                        {
                            MessageBox.Show("Ethernet Port Property not set", "Cancel Confirm", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                        MessageBox.Show("Please configure Ethernet Port", "Warning Ethernet Configuration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case (int)CM.TYPECONNECT.HDR_RS232:
                    break;
                default:
                    break;
            }
        }

        private void Change_conntype_cbx_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (Change_conntype_cbx.SelectedIndex)
            {
                case (int)CM.TYPECONNECT.HDR_ZIGBEE:
                    zigbee_form.ShowDialog();
                    break;
                case (int)CM.TYPECONNECT.HDR_WIFI:
                    wifi_form.ShowDialog();
                    break;
                case (int)CM.TYPECONNECT.HDR_BLUETOOTH:
                    break;
                case (int)CM.TYPECONNECT.HDR_ETHERNET:
                    tcp_form.ShowDialog();
                    break;
                case (int)CM.TYPECONNECT.HDR_RS232:
                    serial_form.ShowDialog();
                    break;
                default:
                    break;
            }
        }

        private void update_fw_btn_Click(object sender, EventArgs e)
        {
            if (com_type != null && com_type.getflagConnected_TCPIP())
            {
                OpenFileDialog firware_file = new OpenFileDialog();
                firware_file.Filter = "Binary File (*.bin)|*.bin|All files (*.*)|*.*";
                firware_file.FilterIndex = 1;
                firware_file.Multiselect = false;
                firware_file.RestoreDirectory = true;
                //DCM_file.InitialDirectory = DCM_file_tx.Text;
                if (firware_file.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    DialogResult result = MessageBox.Show("File Selected:\n" + firware_file.FileName + "\nAre you sure to upload this firmware?\nPlease click \"Yes\" for confirmation",
                                                                  "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        FileInfo fileinfo = new FileInfo(firware_file.FileName);
                        byte[] bytesFile = System.IO.File.ReadAllBytes(firware_file.FileName);
                        string info_file = "[" + fileinfo.Name + "]" + "[" + fileinfo.Length.ToString() + "]";
                        progressBar1.Visible = true;
                        progressBar1.Value = 0;
                        UInt16 num_part = (ushort)(bytesFile.Length / (ushort)CM.LENGTH.CHUNK_SIZE_FILE);
                        progressBar1.Step = 100 / num_part;
                        Log_Handler("Starting Update");
                        com_type.Update_File(bytesFile, info_file);
                    }
                    else
                    {
                        //no...
                    }

                }
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Disconnect_Behavior();
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
            Plan_Node.Plan_Struct nodeplan = new Plan_Node.Plan_Struct(New_NamePlan(), anten_list);
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
                Plan_Node.Plan_Struct theplan;

                theplan = new Plan_Node.Plan_Struct(New_NamePlan(), field[1].TrimEnd(']'));
                theplan.weight = field[6].Substring(0, field[6].Length - 1);

                //theplan.antena = field[1].TrimEnd(']');
                theplan.type = FILTER.EPC;
                if (theplan.type == FILTER.EPC)
                {
                    theplan.EPC = field[3].Substring(field[3].IndexOf('=') + 1).TrimEnd(']');
                }
                root.plan_list.Add(theplan);
            }
            else
            {
                for (int num_plan = 0; num_plan < field.Length / 6; num_plan++)
                {
                    Plan_Node.Plan_Struct theplan = new Plan_Node.Plan_Struct(New_NamePlan(), field[6 * num_plan + 1].TrimEnd(']'));
                    //theplan.antena = field[6 * num_plan + 1].TrimEnd(']');
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

        private void Title_TreeView()
        {
            treeView1.Nodes.Clear();
            TreeNode node_lable = new TreeNode("[Plans]");
            treeView1.Nodes.Add(node_lable);
        }

        private void PopulateTreeView(Plan_Node.Plan_Root root_node)
        {
            this.Invoke((MethodInvoker)delegate
            {
                Title_TreeView();
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
                all_plans.plan_list.RemoveAt(next - 1);
                treeView1.Nodes.Remove(treeView1.SelectedNode);
                treeView1.SelectedNode = treeView1.Nodes[next - 1];
                treeView1.Focus();
            }
            else
            {
                MessageBox.Show("At least must exist one simple plan", "Warning Read Plan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            if ((Ant1_plan_ckb.Enabled) && (plan.antena.IndexOf('1') != -1))
                Ant1_plan_ckb.Checked = true;
            else
                Ant1_plan_ckb.Checked = false;

            if ((Ant2_plan_ckb.Enabled) && (plan.antena.IndexOf('2') != -1))
                Ant2_plan_ckb.Checked = true;
            else
                Ant2_plan_ckb.Checked = false;

            if ((Ant3_plan_ckb.Enabled) && (plan.antena.IndexOf('3') != -1))
                Ant3_plan_ckb.Checked = true;
            else
                Ant3_plan_ckb.Checked = false;

            if ((Ant4_plan_ckb.Enabled) && (plan.antena.IndexOf('4') != -1))
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
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            {
                MessageBox.Show("The weight field can not blank", "Warning Read Plan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                weight_tx.Focus();
            }
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
            if (anten_str.Length > 0)
                return anten_str.Remove(anten_str.Length - 1);
            else return String.Empty;
        }

        private string Plans_ToString(Plan_Node.Plan_Root plans)
        {
            StringBuilder plan_string = new StringBuilder();
            string[] simple_plan_format = new string[5] {
            "SimpleReadPlan:[Antennas=[{0}],",
            "Protocol=GEN2,",
            "Filter=TagData:[EPC={0}],",
            "Op=null,",
            "UseFastSearch=true,Weight={0}]"
            };

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
                if (all_plans.plan_list.Count > 0)
                {
                    com_type.Set_Command_Send(CM.COMMAND.SET_PLAN_CMD, Plans_ToString(all_plans));
                    if (get_plan_btn.Enabled == false)
                        get_plan_btn.Enabled = true;
                }
                else
                {
                    MessageBox.Show("Please click \"+\" button to add one simple plan", "Warning Read Plan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Disconnect_Behavior();
            }
        }
        #endregion

        private void Sensor_EN_ckb_CheckedChanged(object sender, EventArgs e)
        {
            if (Sensor_EN_ckb.Checked)
            {
                //time_on_tx.Text = "0";
                //time_off_tx.Text = "0";
                //time_off_tx.Enabled = false;
                //time_on_tx.Enabled = false;
                //read_sensor_cb.Enabled = false;
                Sensor_EN_ckb.Font = new System.Drawing.Font(Sensor_EN_ckb.Font, FontStyle.Bold);
                timeout_sensor_tx.Enabled = true;
            }
            else
            {
                //time_off_tx.Enabled = true;
                //time_on_tx.Enabled = true;
                //read_sensor_cb.Enabled = true;
                Sensor_EN_ckb.Font = new System.Drawing.Font(Sensor_EN_ckb.Font, FontStyle.Italic);
                timeout_sensor_tx.Enabled = false;
            }
        }

        private void Set_sensor_btn_Click(object sender, EventArgs e)
        {
            startcmdprocess(CM.COMMAND.SETTING_SENSOR_CMD);
        }

        private void Mode_pallet_pattern_cbx_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (0 == Mode_pallet_pattern_cbx.SelectedIndex)
            {
                panel2.Enabled = false;
                //mask_pallet_id_lb.Location = new Point(5, 65);
                //mask_pallet_id_lb.Text = "Pallet Pattern ID:";
            }
            else
            {
                panel2.Enabled = true;
                //mask_pallet_id_lb.Location = new Point(38, 65);
                //mask_pallet_id_lb.Text = "Mask:";
            }
        }

        private void PatternID_tx_KeyPress(object sender, KeyPressEventArgs e)
        {
            HexString_Allow_KeyPress(e);
        }

        private void time_on_tx_KeyPress(object sender, KeyPressEventArgs e)
        {
            Number_Allow_KeyPress(e);
        }

        private void time_off_tx_KeyPress(object sender, KeyPressEventArgs e)
        {
            Number_Allow_KeyPress(e);
        }

        private void get_anten_btn_Click(object sender, EventArgs e)
        {
            if (com_type != null && com_type.getflagConnected_TCPIP())
            {
                com_type.Get_Command_Send(CM.COMMAND.ANTENA_CMD);
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Disconnect_Behavior();
            }
        }

        private void Ant1_plan_ckb_MouseClick(object sender, MouseEventArgs e)
        {
            /*if (String.IsNullOrEmpty(Antena_ToString()))
            {
              MessageBox.Show("Firstly please make sure at least one antena is selected", "Antena Read Plan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
              Ant1_plan_ckb.Checked = true;
            }
            else
            {*/
                if ((treeView1.SelectedNode != null) && (treeView1.SelectedNode.Index > 0))
                    all_plans.plan_list[treeView1.SelectedNode.Index - 1].antena = Antena_ToString();
                else
                    MessageBox.Show("Please select one Plan to update antena for plan", "Plan Selection", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            //}
        }

        private void Ant2_plan_ckb_MouseClick(object sender, MouseEventArgs e)
        {
            /*if (String.IsNullOrEmpty(Antena_ToString()))
            {
              MessageBox.Show("Firstly please make sure at least one antena is selected", "Antena Read Plan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
              Ant2_plan_ckb.Checked = true;
            }
            else
            {*/
                if ((treeView1.SelectedNode != null) && (treeView1.SelectedNode.Index > 0))
                    all_plans.plan_list[treeView1.SelectedNode.Index - 1].antena = Antena_ToString();
                else
                    MessageBox.Show("Please select one Plan to update antena for plan", "Plan Selection", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            //}
        }

        private void Ant3_plan_ckb_MouseClick(object sender, MouseEventArgs e)
        {
            /*if (String.IsNullOrEmpty(Antena_ToString()))
            {
              MessageBox.Show("Firstly please make sure at least one antena is selected", "Antena Read Plan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
              Ant3_plan_ckb.Checked = true;
            }
            else
            {*/
                if ((treeView1.SelectedNode != null) && (treeView1.SelectedNode.Index > 0))
                    all_plans.plan_list[treeView1.SelectedNode.Index - 1].antena = Antena_ToString();
                else
                    MessageBox.Show("Please select one Plan to update antena for plan", "Plan Selection", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            //}
        }

        private void Ant4_plan_ckb_MouseClick(object sender, MouseEventArgs e)
        {
            /*if (String.IsNullOrEmpty(Antena_ToString()))
            {
                MessageBox.Show("Firstly please make sure at least one antena is selected", "Antena Read Plan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Ant4_plan_ckb.Checked = true;
            }
            else
            {*/
                if ((treeView1.SelectedNode != null) && (treeView1.SelectedNode.Index > 0))
                    all_plans.plan_list[treeView1.SelectedNode.Index - 1].antena = Antena_ToString();
                else
                    MessageBox.Show("Please select one Plan to update antena for plan", "Plan Selection", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            //}
        }

        private void Invert_ckb_CheckedChanged(object sender, EventArgs e)
        {
            if (Invert_ckb.Checked)
                PatternID_tx.Font = new Font(PatternID_tx.Font, FontStyle.Strikeout);
            else
                PatternID_tx.Font = new Font(PatternID_tx.Font, FontStyle.Bold | FontStyle.Italic);
        }

        private void scan_ip_btn_Click(object sender, EventArgs e)
        {

            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                Task_Scan();
            else
                MessageBox.Show("The computer not connect to local network.\nPlease check connection", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


        private void Task_Scan()
        {
            IPInterfaceProperties ipProps;
            scanIP_form.netcard_List = new List<string>();
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (((nic.OperationalStatus == OperationalStatus.Up) && (nic.NetworkInterfaceType != NetworkInterfaceType.Tunnel))
                    &&
                    (nic.NetworkInterfaceType != NetworkInterfaceType.Loopback))
                {
                    ipProps = nic.GetIPProperties();

                    foreach (UnicastIPAddressInformation uipProps in ipProps.UnicastAddresses)
                    {
                        if (uipProps.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            int subnet_pos = 0;
                            byte[] ipAddr = new byte[4];
                            byte[] subnet = new byte[4];
                            ipAddr = uipProps.Address.GetAddressBytes();
                            subnet = uipProps.IPv4Mask.GetAddressBytes();
                            for (int i = 0; i < 4; i++)
                            {
                                ipAddr[i] &= subnet[i];
                                for (int ibit = 0; ibit < 8; ibit++)
                                {
                                    if (IsBitSet(subnet[i], ibit))
                                        subnet_pos++;
                                    else
                                        break;
                                }
                            }
                            string ip = string.Format("{0}.{1}.{2}.{3}", ipAddr[0], ipAddr[1], ipAddr[2], ipAddr[3]);
                            scanIP_form.netcard_List.Add(nic.Name + "\t" + ip + "\t" + subnet_pos.ToString());
                            break;
                        }
                    }
                }
            }
            try
            {
                var result = scanIP_form.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    Properties.Settings.Default.save_address = scanIP_form.connect_ip;
                    if (ConnType_cbx.SelectedIndex != (int)CM.TYPECONNECT.HDR_ETHERNET)
                        ConnType_cbx.SelectedIndex = (int)CM.TYPECONNECT.HDR_ETHERNET;
                    else
                        connect_form.ShowDialog();
                }
                else
                {
                    scanIP_form.connect_ip = String.Empty;
                    connect_form.address = String.Empty;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Scan IP Form", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private static bool IsBitSet(byte b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }

        private void Antena_cbx_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (antena_read_power_list.Count > 0)
            {
                ComboBox comboBox = (ComboBox)sender;
                string select = comboBox.Text;
                if (select == "ANT1")
                {
                    if (antena_read_power_list.ContainsKey(1))
                    {
                        read_port_ckb.Checked = true;
                        trackBar1.Value = antena_read_power_list[1] / 100; 
                    }
                    else
                        read_port_ckb.Checked = false;
                    if (antena_write_power_list.ContainsKey(1))
                    {
                        write_port_ckb.Checked = true;
                        trackBar4.Value = antena_write_power_list[1] / 100; 
                    }
                    else
                        write_port_ckb.Checked = false;
                }
                else if (select == "ANT2")
                {
                    if (antena_read_power_list.ContainsKey(2))
                    {
                        read_port_ckb.Checked = true;
                        trackBar1.Value = antena_read_power_list[2] / 100;
                    }
                    else
                        read_port_ckb.Checked = false;
                    if (antena_write_power_list.ContainsKey(2))
                    {
                        write_port_ckb.Checked = true;
                        trackBar4.Value = antena_write_power_list[2] / 100;
                    }
                    else
                        write_port_ckb.Checked = false;
                }
                else if (select == "ANT3")
                {
                    if (antena_read_power_list.ContainsKey(3))
                    {
                        read_port_ckb.Checked = true;
                        trackBar1.Value = antena_read_power_list[3] / 100;
                    }
                    else
                        read_port_ckb.Checked = false;
                    if (antena_write_power_list.ContainsKey(3))
                    {
                        write_port_ckb.Checked = true;
                        trackBar4.Value = antena_write_power_list[3] / 100;
                    }
                    else
                        write_port_ckb.Checked = false;
                }
                else if (select == "ANT4")
                {
                    if (antena_read_power_list.ContainsKey(4))
                    {
                        read_port_ckb.Checked = true;
                        trackBar1.Value = antena_read_power_list[4] / 100;
                    }
                    else
                        read_port_ckb.Checked = false;
                    if (antena_write_power_list.ContainsKey(4))
                    {
                        write_port_ckb.Checked = true;
                        trackBar4.Value = antena_write_power_list[4] / 100;
                    }
                    else
                        write_port_ckb.Checked = false;
                }
                else
                {
                    trackBar1.Value = 30;
                    trackBar4.Value = 30;
                }
            }
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            read_power_port_lb.Text = trackBar1.Value.ToString();
            if (antena_read_power_list.Count > 0)
            {
                if (Antena_cbx.Text.Length == 0)
                    return;
                string select_port = this.Antena_cbx.Text.Substring(Antena_cbx.Text.Length - 1, 1);
                int port;
                if (int.TryParse(select_port, out port))
                {
                    if (antena_read_power_list.ContainsKey(port))
                        antena_read_power_list[port] = 100 * trackBar1.Value;
                }
                /*if (select_text == "ANT1")
                {
                    if (antena_read_power_list.ContainsKey(1))
                        antena_read_power_list[1] = 100 * trackBar1.Value;
                }
                else if (select_text == "ANT2")
                {
                    if (antena_read_power_list.ContainsKey(2))
                        antena_read_power_list[2] = 100 * trackBar1.Value;
                }
                else if (select_text == "ANT3")
                {
                    if (antena_read_power_list.ContainsKey(3))
                        antena_read_power_list[3] = 100 * trackBar1.Value;
                }
                else if (select_text == "ANT4")
                {
                    if (antena_read_power_list.ContainsKey(4))
                        antena_read_power_list[4] = 100 * trackBar1.Value;
                }*/
                else
                    MessageBox.Show("Please select specific Antena Port to update Read Power", "Read Power Port List", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void trackBar4_ValueChanged(object sender, EventArgs e)
        {
            write_power_port_lb.Text = trackBar4.Value.ToString();
            if (antena_write_power_list.Count > 0)
            {
                if (Antena_cbx.Text.Length == 0)
                    return;
                string select_port = this.Antena_cbx.Text.Substring(Antena_cbx.Text.Length - 1, 1);
                int port;
                if (int.TryParse(select_port, out port))
                {
                    if (antena_write_power_list.ContainsKey(port))
                        antena_write_power_list[port] = 100 * trackBar4.Value;
                }
                /*if (select_text == "ANT1")
                {
                    if (antena_write_power_list.ContainsKey(1))
                        antena_write_power_list[1] = 100 * trackBar4.Value;
                }
                else if (select_text == "ANT2")
                {
                    if (antena_write_power_list.ContainsKey(2))
                        antena_write_power_list[2] = 100 * trackBar4.Value;
                }
                else if (select_text == "ANT3")
                {
                    if (antena_write_power_list.ContainsKey(3))
                        antena_write_power_list[3] = 100 * trackBar4.Value;
                }
                else if (select_text == "ANT4")
                {
                    if (antena_write_power_list.ContainsKey(4))
                        antena_write_power_list[4] = 100 * trackBar4.Value;
                }*/
                else
                    MessageBox.Show("Please select specific Antena Port to update Write Power", "Write Power Port List", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void get_pw_antena_btn_Click(object sender, EventArgs e)
        {
            if (com_type != null && com_type.getflagConnected_TCPIP())
            {
                couting = 0;
                progressBar1.Visible = true;
                progressBar1.Value = 0;
                trackBar4.Value = 5;
                trackBar1.Value = 5;
                Log_lb.Text = "Getting Port Power ...";
                Block_RFID_Tab();
                com_type.StartCmd_Process(CM.COMMAND.GET_READ_POWER_PORT_CMD);
                ptimer_loghandle.Interval = 5000;
                ptimer_loghandle.Start();
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Disconnect_Behavior();
            }
        }

        private void set_pw_antena_btn_Click(object sender, EventArgs e)
        {
            if (com_type != null && com_type.getflagConnected_TCPIP())
            {

                startcmdprocess(CM.COMMAND.SET_READ_POWER_PORT_CMD);
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Disconnect_Behavior();
            }
        }

        private void Number_Allow_KeyPress(KeyPressEventArgs e)
        {
            char c = e.KeyChar;
            if (!((c >= '0' && c <= '9') || (c == (char)Keys.Back) || (c == (char)Keys.Delete)))
            {
                e.Handled = true;
            }
        }

        private void HexString_Allow_KeyPress(KeyPressEventArgs e)
        {
            char c = e.KeyChar;
            if (c >= 'a' && c <= 'f')
                c = Char.ToUpper(c);
            if (!((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c == (char)Keys.Back) || (c == (char)Keys.Delete)))
            {
                e.Handled = true;
            }
        }

        private void MessageInterval_tx_KeyPress(object sender, KeyPressEventArgs e)
        {
            Number_Allow_KeyPress(e);
        }

        private void start_bit_tx_KeyPress(object sender, KeyPressEventArgs e)
        {
            Number_Allow_KeyPress(e);
        }

        private void bit_length_tx_KeyPress(object sender, KeyPressEventArgs e)
        {
            Number_Allow_KeyPress(e);
        }

        private void timeout_sensor_tx_KeyPress(object sender, KeyPressEventArgs e)
        {
            Number_Allow_KeyPress(e);
        }

        private void weight_tx_KeyPress(object sender, KeyPressEventArgs e)
        {
            Number_Allow_KeyPress(e);
        }

        private void EPC_filter_tx_KeyPress(object sender, KeyPressEventArgs e)
        {
            HexString_Allow_KeyPress(e);
        }

        private void Memory_filter_tx_KeyPress(object sender, KeyPressEventArgs e)
        {
            HexString_Allow_KeyPress(e);
        }

        private void TID_filter_tx_KeyPress(object sender, KeyPressEventArgs e)
        {
            HexString_Allow_KeyPress(e);
        }

        private void loadProfileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (com_type != null && com_type.getflagConnected_TCPIP())
            {
                OpenFileDialog profile_file = new OpenFileDialog();
                profile_file.Filter = "Profile (*.ini)|*.ini|All files (*.*)|*.*";
                profile_file.Title = "Load profile";
                profile_file.FilterIndex = 1;
                profile_file.Multiselect = false;
                profile_file.RestoreDirectory = true;
                if (profile_file.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    //clear uncheck
                    PalletSupport_cbx.Checked = false;
                    LED_Support_ckb.Checked = false;
                    Offline_ckb.Checked = false;
                    RFID_API_ckb.Checked = false;
                    StackLight_ckb.Checked = false;

                    string[] lines = System.IO.File.ReadAllLines(profile_file.FileName);
                    //Pallet support
                    PatternID_tx.Text = lines[0].Substring(lines[0].IndexOf('=') + 1);
                    setCheckBox(lines[0], PalletSupport_cbx);
                    //if (lines[0].Contains("yes"))
                    Mode_pallet_pattern_cbx.SelectedIndex = 0;
                    //sensor enable
                    setCheckBox(lines[1], Sensor_EN_ckb);
                    //read contiuously
                    setCheckBox(lines[2], read_sensor_cb);
                    //time on
                    time_on_tx.Text = lines[3].Substring(lines[3].IndexOf('=') + 1);
                    //time off
                    time_off_tx.Text = lines[4].Substring(lines[4].IndexOf('=') + 1);
                    //watching timeout
                    timeout_sensor_tx.Text = lines[5].Substring(lines[5].IndexOf('=') + 1);
                    //led support
                    setCheckBox(lines[6], LED_Support_ckb);
                    //stacklight support
                    setCheckBox(lines[7], StackLight_ckb);
                    //Speaker Support
                    setCheckBox(lines[8], AudioSupport_cbx);
                    //BLF
                    freq_cbx.SelectedIndex = freq_cbx.FindStringExact(lines[9].Substring(lines[9].IndexOf('=') + 1));
                    //coding
                    coding_cbx.SelectedIndex = coding_cbx.FindStringExact(lines[10].Substring(lines[10].IndexOf('=') + 1));
                    //tari
                    tari_cbx.SelectedIndex = tari_cbx.FindStringExact(lines[11].Substring(lines[11].IndexOf('=') + 1));
                    //region
                    region_lst.SelectedIndex = int.Parse(lines[12].Substring(lines[12].IndexOf('=') + 1));
                    //read power
                    trackBar2.Value = int.Parse(lines[13].Substring(lines[13].IndexOf('=') + 1));
                    //power mode
                    power_mode_cbx.SelectedIndex = power_mode_cbx.FindStringExact(lines[14].Substring(lines[14].IndexOf('=') + 1));
                    //Send Null on No Tag
                    setCheckBox(lines[15], sendnull_ckb);
                    if (lines[16].Contains("yes")) //actually yes
                    {
                        list_plan_name.Clear();
                        all_plans.plan_list.Clear();
                        all_plans = LoadReadPlans(lines[16].Substring(lines[16].IndexOf("Simple")));
                        PopulateTreeView(all_plans);
                    }
                    else
                    {
                        list_plan_name.Clear();
                        all_plans.plan_list.Clear();
                        Title_TreeView();
                    }
                }
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Disconnect_Behavior();
            }
        }

        private void saveProfileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (com_type != null && com_type.getflagConnected_TCPIP())
            {
                if (all_plans.plan_list.Count == 0)
                {
                    com_type.resetflag();
                    com_type.Get_Command_Send(CM.COMMAND.GET_PLAN_CMD);
                    com_type.waitflagRevTCP();
                }
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.Filter = "Profile|*.ini";
                saveFileDialog1.Title = "Save as Profile";
                saveFileDialog1.DefaultExt = "ini";
                saveFileDialog1.RestoreDirectory = true;
                string[] profile_format = new string[17] {
                    "Pattern support:{0}={1}",
                    "Sensor support:{0}",
                    "Read continue:{0}",
                    "Time on={0}",
                    "Time off={0}",
                    "Watching timeout={0}",
                    "Led support:{0}",
                    "Stacklight support:{0}",
                    "Speaker support:{0}",
                    "BLF={0}",
                    "Coding={0}",
                    "Tari={0}",
                    "Region={0}",
                    "Read power={0}",
                    "Power Mode={0}",
                    "Send Null on No Tag:{0}",
                    "Read Plan:{0}{1}",
                };
                if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {

                    StringBuilder content = new StringBuilder();
                    content.AppendFormat(profile_format[0], convertCheckBox(PalletSupport_cbx), PatternID_tx.Text).Append(Environment.NewLine);
                    content.AppendFormat(profile_format[1], convertCheckBox(Sensor_EN_ckb)).Append(Environment.NewLine);
                    content.AppendFormat(profile_format[2], convertCheckBox(read_sensor_cb)).Append(Environment.NewLine);
                    content.AppendFormat(profile_format[3], time_on_tx.Text).Append(Environment.NewLine);
                    content.AppendFormat(profile_format[4], time_off_tx.Text).Append(Environment.NewLine);
                    content.AppendFormat(profile_format[5], timeout_sensor_tx.Text).Append(Environment.NewLine);
                    content.AppendFormat(profile_format[6], convertCheckBox(LED_Support_ckb)).Append(Environment.NewLine);
                    content.AppendFormat(profile_format[7], convertCheckBox(StackLight_ckb)).Append(Environment.NewLine);
                    content.AppendFormat(profile_format[8], convertCheckBox(AudioSupport_cbx)).Append(Environment.NewLine);
                    content.AppendFormat(profile_format[9], freq_cbx.Text).Append(Environment.NewLine);
                    content.AppendFormat(profile_format[10], coding_cbx.Text).Append(Environment.NewLine);
                    content.AppendFormat(profile_format[11], tari_cbx.Text).Append(Environment.NewLine);
                    content.AppendFormat(profile_format[12], region_lst.SelectedIndex.ToString()).Append(Environment.NewLine);
                    content.AppendFormat(profile_format[13], trackBar2.Value.ToString()).Append(Environment.NewLine);
                    content.AppendFormat(profile_format[14], power_mode_cbx.Text).Append(Environment.NewLine);
                    content.AppendFormat(profile_format[15], convertCheckBox(sendnull_ckb)).Append(Environment.NewLine);
                    if (all_plans.plan_list.Count > 0)
                        content.AppendFormat(profile_format[16], "yes=", Plans_ToString(all_plans));
                    else
                    {
                        //MessageBox.Show("Enable Read Contiuously only work when Reader Profile have at least two Read Plan\nPlease check Read Plan in RFID Configuration Tab.", "Forklift Behaviour", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        content.AppendFormat(profile_format[16], "no", String.Empty);
                    }
                    System.IO.File.WriteAllText(saveFileDialog1.FileName, content.ToString(), Encoding.ASCII);
                }
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Disconnect_Behavior();
            }
        }
        private string convertCheckBox(CheckBox checkbox)
        {
            if (checkbox.Checked)
                return "yes";
            else
                return "no";
        }

        private void setCheckBox(string str, CheckBox checkbox)
        {
            if (str.Contains("yes"))
                checkbox.Checked = true;
            else
                checkbox.Checked = false;
        }

        /*private string Plan_Profile(Plan_Node.Plan_Root plans)
        {
            StringBuilder plan_string = new StringBuilder();
            if (all_plans.plan_list.Count == 1)
            {
                plan_string.Append("[" + all_plans.plan_list[0].name + "=");
                plan_string.AppendFormat(simple_plan_format[0], all_plans.plan_list[0].antena);
                plan_string.AppendFormat(simple_plan_format[1]);
                plan_string.AppendFormat(simple_plan_format[2], all_plans.plan_list[0].EPC);
                plan_string.AppendFormat(simple_plan_format[3]);
                plan_string.AppendFormat(simple_plan_format[4], all_plans.plan_list[0].weight);
                plan_string.Append("]");
            }
            else
            {
                for (int ix = 0; ix < all_plans.plan_list.Count; ix++)
                {
                    plan_string.Append("[" + all_plans.plan_list[ix].name + "=");
                    plan_string.AppendFormat(simple_plan_format[0], all_plans.plan_list[ix].antena);
                    plan_string.AppendFormat(simple_plan_format[1]);
                    plan_string.AppendFormat(simple_plan_format[2], all_plans.plan_list[ix].EPC);
                    plan_string.AppendFormat(simple_plan_format[3]);
                    plan_string.AppendFormat(simple_plan_format[4], all_plans.plan_list[ix].weight);
                    plan_string.Append("];");
                }
                plan_string.Remove(plan_string.Length - 1, 1);
            }
            return plan_string.ToString();
        }*/

        //TimeSpan time_count = new TimeSpan();
        double sec = 0;
        bool pinged = false;
        private void timer1_Tick(object sender, EventArgs e)
        {
            sec++;
            TimeSpan time = TimeSpan.FromSeconds(sec);
            this.Invoke((MethodInvoker)delegate
            {
                time_duration_lb.Text = time.ToString(@"hh\:mm\:ss");
                if (pinged)
                {
                    status_led.Image = global::GatewayForm.Properties.Resources.green_led2;
                    pinged = false;
                }
            });
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void loadProfileToolStripMenuItem_MouseEnter(object sender, EventArgs e)
        {
            if (com_type != null && com_type.getflagConnected_TCPIP())
            {
                ToolStripMenuItem item = sender as ToolStripMenuItem;
                item.ForeColor = Color.DarkBlue;
                Font oldfont = item.Font;
                item.Font = new Font(oldfont, oldfont.Style | FontStyle.Bold);
            }
        }

        private void loadProfileToolStripMenuItem_MouseLeave(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            item.ForeColor = SystemColors.ControlText;
            Font oldfont = item.Font;
            item.Font = new Font(oldfont, oldfont.Style & FontStyle.Regular);
        }

        private void saveProfileToolStripMenuItem_MouseEnter(object sender, EventArgs e)
        {
            if (com_type != null && com_type.getflagConnected_TCPIP())
            {
                ToolStripMenuItem item = sender as ToolStripMenuItem;
                item.ForeColor = Color.DarkBlue;
                Font oldfont = item.Font;
                item.Font = new Font(oldfont, oldfont.Style | FontStyle.Bold);
            }
        }

        private void saveProfileToolStripMenuItem_MouseLeave(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            item.ForeColor = SystemColors.ControlText;
            Font oldfont = item.Font;
            item.Font = new Font(oldfont, oldfont.Style & FontStyle.Regular);
        }

        private void set_gpo_btn_Click(object sender, EventArgs e)
        {
            if (com_type != null && com_type.getflagConnected_TCPIP())
            {
                startcmdprocess(CM.COMMAND.SET_GPO_VALUE_CMD);
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Disconnect_Behavior();
            }
        }

        private void get_gpi_btn_Click(object sender, EventArgs e)
        {
            if (com_type != null && com_type.getflagConnected_TCPIP())
            {
                 com_type.Get_Command_Send(CM.COMMAND.GET_GPIO_STATUS_CMD);
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Disconnect_Behavior();
            }
        }

        private void read_sensor_cb_CheckedChanged(object sender, EventArgs e)
        {
            HighlightCheckBox(sender as CheckBox);
        }

        private void HighlightCheckBox(CheckBox checkbox)
        {
            if (checkbox.Checked)
                checkbox.Font = new Font(checkbox.Font, FontStyle.Bold);
            else
                checkbox.Font = new Font(checkbox.Font, FontStyle.Italic);
        }

        private void sendnull_ckb_CheckedChanged(object sender, EventArgs e)
        {
            HighlightCheckBox(sender as CheckBox);
        }

        private void sendNullTrigger_ckb_CheckedChanged(object sender, EventArgs e)
        {
            HighlightCheckBox(sender as CheckBox);
        }

        private void LED_Support_ckb_CheckedChanged(object sender, EventArgs e)
        {
            HighlightCheckBox(sender as CheckBox);
        }

        private void Offline_ckb_CheckedChanged(object sender, EventArgs e)
        {
            HighlightCheckBox(sender as CheckBox);
        }

        private void StackLight_ckb_CheckedChanged(object sender, EventArgs e)
        {
            HighlightCheckBox(sender as CheckBox);
        }

        private void RFID_API_ckb_CheckedChanged(object sender, EventArgs e)
        {
            HighlightCheckBox(sender as CheckBox);
        }

        private void speak_btn_Click(object sender, EventArgs e)
        {
            if (com_type != null && com_type.getflagConnected_TCPIP())
            {
                com_type.Set_Command_Send(CM.COMMAND.TEXT_TO_SPEECH_CMD, text_to_speak_tx.Text);
                com_type.waitflagRevTCP(5000);
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Disconnect_Behavior();
            }
        }

        private void static_Q_rbtn_CheckedChanged(object sender, EventArgs e)
        {
            if (static_Q_rbtn.Enabled)
                trackBar5.Enabled = true;
            else
            {
                trackBar5.Value = 0;
                trackBar5.Enabled = false;
            }
        }

        private void trackBar5_ValueChanged(object sender, EventArgs e)
        {
            static_value_lb.Text = trackBar5.Value.ToString();
        }

        private void dynamic_Q_rbtn_CheckedChanged(object sender, EventArgs e)
        {
            if (dynamic_Q_rbtn.Checked)
            {
                trackBar5.Value = 0;
                trackBar5.Enabled = false;
            }
            else
                trackBar5.Enabled = true;
        }

        private void get_tag_connection_btn_Click(object sender, EventArgs e)
        {
            if (com_type != null && com_type.getflagConnected_TCPIP())
            {
                couting = 0;
                progressBar1.Visible = true;
                progressBar1.Value = 0;
                Log_lb.Text = "Getting Tag Contention ...";
                Block_RFID_Tab();
                ptimer_loghandle.Interval = 6000;
                ptimer_loghandle.Start();
                com_type.StartCmd_Process(CM.COMMAND.GET_TAG_CONNECTION_CMD);
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Disconnect_Behavior();
            }
        }

        private void set_tag_connection_btn_Click(object sender, EventArgs e)
        {
            if (com_type != null && com_type.getflagConnected_TCPIP())
            {
                startcmdprocess(CM.COMMAND.SET_TAG_CONNECTION_CMD);
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Disconnect_Behavior();
            }
        }

        private void clear_AntenaPort_btn_Click(object sender, EventArgs e)
        {
            if (com_type != null && com_type.getflagConnected_TCPIP())
            {
                trackBar4.Value = 5;
                trackBar1.Value = 5;
                startcmdprocess(CM.COMMAND.SET_WRITE_POWER_PORT_CMD);
            }
            else
            {
                MessageBox.Show("Connection was disconnected\nPlease connect again!", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Disconnect_Behavior();
            }
        }

        private void read_port_ckb_CheckedChanged(object sender, EventArgs e)
        {
            if ((Antena_cbx.Text.Length == 0) || (Antena_cbx.Text == "ANTx"))
            {
                read_port_ckb.Checked = false;
                return;
            }
            string select_port = this.Antena_cbx.Text.Substring(Antena_cbx.Text.Length - 1, 1);
            int port; 
            if (read_port_ckb.Checked)
            {
                if (int.TryParse(select_port, out port))
                {
                    trackBar1.Enabled = true;
                    if (antena_read_power_list.ContainsKey(port))
                        trackBar1.Value = antena_read_power_list[port] / 100;
                    else
                        antena_read_power_list.Add(port, 100 * trackBar1.Value);
                }
                else
                    MessageBox.Show("Please select specific Antena Port to update Read Power", "Read Power Port List", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                if (int.TryParse(select_port, out port))
                {
                    if (antena_read_power_list.ContainsKey(port))
                        antena_read_power_list.Remove(port);
                    trackBar1.Enabled = false;
                }
                else
                    MessageBox.Show("Please select specific Antena Port to Remove Read Power", "Read Power Port List", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void write_port_ckb_CheckedChanged(object sender, EventArgs e)
        {
            if ((Antena_cbx.Text.Length == 0) || (Antena_cbx.Text == "ANTx"))
            {
                write_port_ckb.Checked = false;
                return;
            }
            string select_port = this.Antena_cbx.Text.Substring(Antena_cbx.Text.Length - 1, 1);
            int port;
            if (write_port_ckb.Checked)
            {
                if (int.TryParse(select_port, out port))
                {
                    trackBar4.Enabled = true;
                    if (antena_write_power_list.ContainsKey(port))
                        trackBar4.Value = antena_write_power_list[port] / 100;
                    else
                        antena_write_power_list.Add(port, 100 * trackBar4.Value);
                }
                else
                    MessageBox.Show("Please select specific Antena Port to update Write Power", "Write Power Port List", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                if (int.TryParse(select_port, out port))
                {
                    if (antena_write_power_list.ContainsKey(port))
                        antena_write_power_list.Remove(port);
                    trackBar4.Enabled = false;
                }
                else
                    MessageBox.Show("Please select specific Antena Port to Remove Write Power", "Write Power Port List", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void request_TagID_btn_Click(object sender, EventArgs e)
        {
            //startcmdprocess(CM.COMMAND.REQUEST_TAG_ID_CMD);
            com_type.Get_Command_Send(CM.COMMAND.STOP_OPERATION_CMD);
        }
    }
}
