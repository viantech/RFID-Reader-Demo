using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GatewayForm
{
    public partial class Zigbee_pop : Form
    {
        public string ip_add;
        public string port;
        public string PanID = String.Empty;
        public string Device;
        public string Channel;
        public int Length;
        public int Timeout;

        public Zigbee_pop()
        {
            InitializeComponent();
        }
        
        private void update_btn_Click(object sender, EventArgs e)
        {
            ip_add = this.ZigbeeIP_tx.Text;
            port = this.ZigbeePort_tx.Text;
            PanID = this.PanID_tx.Text;
            Device = this.DeviceID_tx.Text;
            Channel = this.ZigbeeChannel_tx.Text;
            Length = int.Parse(this.Zmax_len_tx.Text);
            Timeout = int.Parse(this.Ztimeout_tx.Text);
            this.Close();
        }

        private void close_btn_Click(object sender, EventArgs e)
        {
            this.PanID = String.Empty;
            this.Close();
        }

        private void Zigbee_pop_Activated(object sender, EventArgs e)
        {
            this.ZigbeeIP_tx.Text = ip_add;
            this.ZigbeePort_tx.Text = port;
        }
    }
}
