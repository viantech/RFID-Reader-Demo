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
    public partial class SearchForm_Data : Form
    {
        public delegate void LoadDataSearch(String field, String data);
        public event LoadDataSearch loaddatasearch;
        public SearchForm_Data()
        {
            InitializeComponent();
           
        }
        private void btn_searchTag_Click(object sender, EventArgs e)
        {
            loaddatasearch("TAG", txt_search_tag.Text);
        }

        private void btn_searchdate_Click(object sender, EventArgs e)
        {
            loaddatasearch("TAG", txt_search_date.Text);
        }
    }
}
