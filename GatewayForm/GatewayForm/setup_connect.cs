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
    public partial class setup_connect : Form
    {
        public string address = String.Empty;
        public string port;

        public setup_connect()
        {
            InitializeComponent();
        }

        private void ok_btn_Click(object sender, EventArgs e)
        {
            address = this.address_IP_tx.Text;
            port = this.port_IP_tx.Text;
            Properties.Settings.Default.save_address = address_IP_tx.Text;
            Properties.Settings.Default.save_port = port_IP_tx.Text;
            Properties.Settings.Default.Save();
            this.Close();
        }

        private void Cancel_btn_Click(object sender, EventArgs e)
        {
            address = String.Empty;
            this.Close();
        }

        private void setup_connect_Activated(object sender, EventArgs e)
        {
            address_IP_tx.Text = Properties.Settings.Default.save_address;
            port_IP_tx.Text = Properties.Settings.Default.save_port;
            address_IP_tx.Select(address_IP_tx.Text.Length, 0);
            address_IP_tx.Focus();
        }
    }
}
