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
        public int Length;
        public int Timeout;

        public Tcp_pop()
        {
            InitializeComponent();
            if(automatic)
            {
                this.dhcp_btn.Checked = automatic;
            }
            else
            {
                this.static_btn.Checked = automatic;
                this.Subnet_tx.Enabled = true;
                this.gateway_tx.Enabled = true;
            }
        }

        private void update_btn_Click(object sender, EventArgs e)
        {
            address = this.TcpIP_tx.Text;
            port = this.TcpPort_tx.Text;
            automatic = this.dhcp_btn.Checked;
            Length = int.Parse(this.Tcp_len_tx.Text);
            Timeout = int.Parse(this.TcpTimeout_tx.Text);
            this.Close();
        }

        private void close_btn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dhcp_btn_CheckedChanged(object sender, EventArgs e)
        {
            this.Subnet_tx.Enabled = false;
            this.gateway_tx.Enabled = false;
        }

        private void static_btn_CheckedChanged(object sender, EventArgs e)
        {
            this.Subnet_tx.Enabled = true;
            this.gateway_tx.Enabled = true;
        }

        private void groupBox4_Enter(object sender, EventArgs e)
        {

        }
    }
}
