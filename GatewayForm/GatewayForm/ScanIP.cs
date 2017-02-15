using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IPAddressControlLib;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

namespace GatewayForm
{
    public partial class ScanIP : Form
    {
        private IPAddressControlLib.IPAddressControl ipAddressControl1;
        //private IPAddressControlLib.IPAddressControl ipAddressControl2;
        public List<string> netcard_List;
        string[] netcard_current;
        int start = byte.MinValue;
        int end = byte.MaxValue;

        List<KeyValuePair<string, string>> computerList = new List<KeyValuePair<string, string>>();
        long completedCounter = 0;
        long total_host = 0;
        List<IPAddress> alivehost = new List<IPAddress>();
        object listLock = new object();
        public string connect_ip {get;set;} 
        //public string connect_ip { get { return String.Empty; } ; private set}

        public ScanIP()
        {
            InitializeComponent();
        }

        private string[] Decode_Info_NetCard(int index)
        {
            return netcard_List.ElementAt(index).ToString().Split(new string[] { "\t" }, StringSplitOptions.None);
        }

        private void network_card_cbx_SelectedIndexChanged(object sender, EventArgs e)
        {
            netcard_current = Decode_Info_NetCard(network_card_cbx.SelectedIndex);
            ipAddressControl1.Text = netcard_current[1];

            byte[] mask = new byte[4];
            mask = ipAddressControl1.GetAddressBytes();
            if (netcard_current[2] == "16")
                ipAddressControl2.IPAddress = new IPAddress(new byte[4] { mask[0], mask[1], 255, 255 });
            else if (netcard_current[2] == "24")
                ipAddressControl2.IPAddress = new IPAddress(new byte[4] { mask[0], mask[1], mask[2], 255 });
        }

        private void GetFieldStart(object sender, FieldChangedEventArgs e)
        {
            if (netcard_current != null)
            {
                if (!String.IsNullOrEmpty(e.Text))
                {
                    if (!int.TryParse(e.Text, out this.start))
                        this.start = 1;
                }
            }
        }

        private void GetFieldEnd(object sender, FieldChangedEventArgs e)
        {
            if (netcard_current != null)
            {
                if (!String.IsNullOrEmpty(e.Text))
                {
                    if (!int.TryParse(e.Text, out this.end))
                        this.end = byte.MaxValue;
                }
            }
        }

        private void ipAddressControl1_Click(object sender, EventArgs e)
        {
            if (netcard_current[2] == "16")
                ipAddressControl1.SetFieldFocus(2);
            else if (netcard_current[2] == "24")
                ipAddressControl1.SetFieldFocus(3);
            else
                network_card_cbx.Focus();
        }

        private void ipAddressControl2_Click(object sender, EventArgs e)
        {
            if (netcard_current[2] == "16")
                ipAddressControl2.SetFieldFocus(2);
            else if (netcard_current[2] == "24")
                ipAddressControl2.SetFieldFocus(3);
            else
                network_card_cbx.Focus();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            computerList.Clear();
            alivehost.Clear();
            completedCounter = 0;
            total_host = 0;
            dataGridView1.Rows.Clear();
            //int task = await Task.Run(() => ScanIPsNetwork(ipAddressControl1.GetAddressBytes(), netcard_current[2], start, end));
            Thread pscanIP = new Thread(() => ScanIPsNetwork(ipAddressControl1.GetAddressBytes(), netcard_current[2], start, end));
            pscanIP.Start();
        }

        private delegate void SetBarDelegate(int Value, bool max_or_value);
        private void SetBar(int Value, bool max_or_value)
        {
            if (this.progressBar1.InvokeRequired)
            {
                SetBarDelegate bar = new SetBarDelegate(SetBar);
                this.Invoke(bar, new object[] { Value, max_or_value });
            }
            else
            {
                if (max_or_value)
                    this.progressBar1.Maximum = Value;
                else
                    this.progressBar1.Value = Value;
            }
        }

        private delegate void SetStatusDelegate(string text);
        private void SetLog(string text)
        {
            if (this.status_scanning_lb.InvokeRequired)
            {
                SetStatusDelegate label = new SetStatusDelegate(SetLog);
                this.Invoke(label, new object[] { text });
            }
            else
            {
                this.status_scanning_lb.Text = text;
            }
        }

        private delegate void SetGridVIewDelegate(string IP, string hostname);
        private void SetGrid(string IP, string hostname)
        {
            if (this.status_scanning_lb.InvokeRequired)
            {
                SetGridVIewDelegate grid = new SetGridVIewDelegate(SetGrid);
                this.Invoke(grid, new object[] { IP, hostname });
            }
            else
            {
                this.dataGridView1.Rows.Add(IP, hostname);
            }
        }

        private void ScanIPsNetwork(byte[] ipaddr, string subnet, int startIP, int endIP)
        {
            if (subnet == "24")
            {
                total_host = (endIP - startIP + 1);
                SetBar((int)total_host, true);

                for (int isub24 = startIP; isub24 <= endIP; isub24++)
                {
                    ipaddr.SetValue((byte)isub24, 3);
                    IPAddress scanIP = new IPAddress(ipaddr);
                    PingHost(scanIP);
                }
                do
                    Thread.Sleep(0);
                while (Interlocked.Read(ref completedCounter) < (endIP - startIP + 1));
                completedCounter = 0;
            }
            else if (subnet == "16")
            {
                if (255 == endIP)
                {
                    System.Windows.Forms.DialogResult result = MessageBox.Show("It will take a very long time to scan IP.\nClick \"Yes\" to limit the range (set the third octes less than 255).\nClick \"No\" to continue scanning hosts.", 
                        "Huge Range Scan IP", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == System.Windows.Forms.DialogResult.Yes)
                    {
                        //this.ipAddressControl2.Focus();
                        return;
                    }
                }
                total_host = 254*(endIP - startIP + 1);
                SetBar((int)total_host, true);

                for (int isub16 = startIP; isub16 <= endIP; isub16++)
                {
                    ipaddr.SetValue((byte)isub16, 2);
                    for (int isub24 = 1; isub24 < byte.MaxValue; isub24++)
                    {
                        ipaddr.SetValue((byte)isub24, 3);
                        IPAddress scanIP = new IPAddress(ipaddr);
                        PingHost(scanIP);
                    }
                }
                do
                    Thread.Sleep(0);
                while (Interlocked.Read(ref completedCounter) < (endIP - startIP + 1) * 254);
                completedCounter = 0;
            }
            SetBar(0, false);
            SetLog("Resolving Host Name ...");

            foreach (IPAddress ipAddress in alivehost)
            {
                Dns.BeginGetHostEntry(ipAddress, new AsyncCallback(RequestHostAddress), ipAddress);
            }
            do
                Thread.Sleep(0);
            while (Interlocked.Read(ref completedCounter) < alivehost.Count);
            completedCounter = 0;
            ListOutComputer();
        }

        private void PingHost(IPAddress hostIP)
        {
            ThreadPool.QueueUserWorkItem(delegate(object state)
            {
                Ping pingsender = new Ping();
                if (pingsender.Send(hostIP, 100).Status == IPStatus.Success)
                    lock (listLock) alivehost.Add(hostIP);
                Interlocked.Increment(ref completedCounter);
                double ratio = (double)completedCounter / total_host;
                SetLog("Scanning:  " + Math.Round(100*ratio).ToString() + "%  (" + completedCounter.ToString() + "/" + total_host.ToString() + ")");
                SetBar((int)(completedCounter), false);
            });
        }

        private void ListOutComputer()
        {
            this.Invoke((MethodInvoker)delegate
            {
                foreach (KeyValuePair<string, string> host in computerList)
                    this.dataGridView1.Rows.Add(host.Key, host.Value);
                    //SetGrid(host.Key, host.Value);
            });
            Thread found = new Thread(() => SetLog("Found " + alivehost.Count.ToString() + " hosts"));
            found.Start();
        }

        private void RequestHostAddress(IAsyncResult iAr)
        {
            IPAddress ipaddr = (IPAddress)iAr.AsyncState;
            IPHostEntry host = null;
            try
            {
                host = Dns.EndGetHostEntry(iAr);
            }
            catch (System.Net.Sockets.SocketException)
            {
                computerList.Add(new KeyValuePair<string, string>(ipaddr.ToString(), "could not resolve host name"));
            }
            if (host != null)
                computerList.Add(new KeyValuePair<string, string>(ipaddr.ToString(), host.HostName));
            Interlocked.Increment(ref completedCounter);
            return;
        }

        private void ScanIP_Load(object sender, EventArgs e)
        {
            if (network_card_cbx.Items.Count > 0)
                network_card_cbx.Items.Clear();
            if (netcard_List.Count > 0)
            {
                string[] first_netcard = Decode_Info_NetCard(0);
                network_card_cbx.Text = first_netcard[0];
                ipAddressControl1.Text = first_netcard[1];
                byte[] mask = new byte[4];
                mask = ipAddressControl1.GetAddressBytes();
                if (first_netcard[2] == "16")
                    ipAddressControl2.IPAddress = new IPAddress(new byte[4]{mask[0],mask[1],255,255});
                else if (first_netcard[2] == "24")
                    ipAddressControl2.IPAddress = new IPAddress(new byte[4] { mask[0], mask[1], mask[2], 255 });
                for (int icard = 0; icard < netcard_List.Count; icard++)
                    network_card_cbx.Items.Add(Decode_Info_NetCard(icard)[0]);

                netcard_current = first_netcard;
            }
        }

        private void ipAddressControl1_KeyDown(object sender, KeyEventArgs e)
        {
            this.ipAddressControl1.FieldChangedEvent +=
               new System.EventHandler<IPAddressControlLib.FieldChangedEventArgs>(this.GetFieldStart);
        }

        private void ipAddressControl1_KeyUp(object sender, KeyEventArgs e)
        {
            this.ipAddressControl1.FieldChangedEvent -=
               new System.EventHandler<IPAddressControlLib.FieldChangedEventArgs>(this.GetFieldStart);
        }

        private void ipAddressControl2_KeyDown(object sender, KeyEventArgs e)
        {
            this.ipAddressControl2.FieldChangedEvent +=
                new System.EventHandler<IPAddressControlLib.FieldChangedEventArgs>(this.GetFieldEnd);
        }

        private void ipAddressControl2_KeyUp(object sender, KeyEventArgs e)
        {
            this.ipAddressControl2.FieldChangedEvent -=
                new System.EventHandler<IPAddressControlLib.FieldChangedEventArgs>(this.GetFieldEnd);
        }

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            //var dataGridView = sender as DataGridView;
            if (dataGridView1.Rows[e.RowIndex].Selected)
            {
                if ((dataGridView1[e.ColumnIndex, e.RowIndex].Value != null) && (!String.IsNullOrEmpty(dataGridView1[e.ColumnIndex, e.RowIndex].Value.ToString())))
                {
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold | FontStyle.Italic);
                    // edit: to change the background color:
                    e.CellStyle.SelectionBackColor = Color.Coral;
                    this.connect_ip = dataGridView1[0, e.RowIndex].Value.ToString();
                    button2.Enabled = true;
                }
                else
                {
                    button2.Enabled = false;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ipAddressControl1_Enter(object sender, EventArgs e)
        {
            if (netcard_current[2] == "16")
                ipAddressControl1.SetFieldFocus(2);
            else if (netcard_current[2] == "24")
                ipAddressControl1.SetFieldFocus(3);
            else
                network_card_cbx.Focus();
        }

        private void ipAddressControl2_Enter(object sender, EventArgs e)
        {
            if (netcard_current[2] == "16")
                ipAddressControl2.SetFieldFocus(2);
            else if (netcard_current[2] == "24")
                ipAddressControl2.SetFieldFocus(3);
            else
                network_card_cbx.Focus();
        }
    }
}
