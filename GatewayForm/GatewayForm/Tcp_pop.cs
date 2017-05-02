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
    public partial class Tcp_pop : Form
    {
        public string address;
        public string port;
        public bool automatic = true;
        public string Length;
        public string Timeout;
        public string netmask;
        public string gateway = String.Empty;

        public Tcp_pop()
        {
            InitializeComponent();
            this.Icon = Properties.Resources.favicon__1_;
            if(automatic)
            {
                this.dhcp_btn.Checked = automatic;
            }
            else
            {
                this.static_btn.Checked = automatic;
                this.Subnet_tx.Enabled = true;
                this.gateway_tx.Enabled = true;
                this.TcpIP_tx.Enabled = true;
                this.TcpPort_tx.Enabled = true;
            }
        }

        private void update_btn_Click(object sender, EventArgs e)
        {
            address = this.TcpIP_tx.Text;
            port = this.TcpPort_tx.Text;
            automatic = this.dhcp_btn.Checked;
            Length = this.Tcp_len_tx.Text;
            Timeout = this.TcpTimeout_tx.Text;
            netmask = this.Subnet_tx.Text;
            gateway = this.gateway_tx.Text;
            this.Close();
        }

        private void close_btn_Click(object sender, EventArgs e)
        {
            this.gateway = String.Empty;
            this.Close();
        }

        private void dhcp_btn_CheckedChanged(object sender, EventArgs e)
        {
            this.Subnet_tx.Enabled = false;
            this.gateway_tx.Enabled = false;
            this.TcpIP_tx.Enabled = false;
            this.TcpPort_tx.Enabled = false;
        }

        private void static_btn_CheckedChanged(object sender, EventArgs e)
        {
            this.Subnet_tx.Enabled = true;
            this.gateway_tx.Enabled = true;
            this.TcpIP_tx.Enabled = true;
            this.TcpPort_tx.Enabled = true;
        }

        private void Tcp_pop_Activated(object sender, EventArgs e)
        {
            this.TcpIP_tx.Text = address;
            //this.TcpPort_tx.Text = port;
        }

    }
}
