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
    public partial class Serial_pop : Form
    {
        public string portname;
        public int baud;
        public Serial_pop()
        {
            InitializeComponent();
            this.Icon = Properties.Resources.favicon__1_;
        }

        private void update_btn_Click(object sender, EventArgs e)
        {
            portname = this.portname_tx.Text;
            baud = int.Parse(this.baudrate_tx.Text);
            this.Close();
        }

        private void close_btn_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
