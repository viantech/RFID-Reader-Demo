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
    public partial class Wifi_pop : Form
    {
        public string ssid_name = string.Empty;
        public string passwd;
        public string address;
        //public string port;
        public string netmask;
        public string gateway;
        public bool automatic = true;
        //public int Length;
        //public int Timeout;
        public Wifi_pop()
        {
            InitializeComponent();
            this.Icon = Properties.Resources.favicon__1_;
            if (automatic)
            {
                this.dhcp_btn.Checked = automatic;
            }
            else
            {
                this.static_btn.Checked = automatic;
                this.Subnet_tx.Enabled = true;
                this.gateway_tx.Enabled = true;
                this.address_tx.Enabled = true;
            }
        }

        private void update_btn_Click(object sender, EventArgs e)
        {
            ssid_name = this.ssid_name_tx.Text;
            passwd = this.password_tx.Text;
            address = this.address_tx.Text;
            //port = this.port_tx.Text;
            automatic = this.dhcp_btn.Checked;
            netmask = this.Subnet_tx.Text;
            gateway = this.gateway_tx.Text;
            //Length = int.Parse(this.Tcp_len_tx.Text);
            //Timeout = int.Parse(this.TcpTimeout_tx.Text);
            this.Close();
        }

        private void close_btn_Click(object sender, EventArgs e)
        {
            ssid_name = String.Empty;
            this.Close();
        }

        private void dhcp_btn_CheckedChanged(object sender, EventArgs e)
        {
            this.Subnet_tx.Enabled = false;
            this.gateway_tx.Enabled = false;
            this.address_tx.Enabled = false;
        }

        private void static_btn_CheckedChanged(object sender, EventArgs e)
        {
            this.Subnet_tx.Enabled = true;
            this.gateway_tx.Enabled = true;
            this.address_tx.Enabled = true;
        }

        private void Wifi_pop_Activated(object sender, EventArgs e)
        {
            this.address_tx.Text = address;
            //this.port_tx.Text = port;
        }

    }
}
