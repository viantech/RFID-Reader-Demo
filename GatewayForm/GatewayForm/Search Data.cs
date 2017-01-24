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
            int day = txt_search_date.Value.Day;
            int month = txt_search_date.Value.Month;
            int year = txt_search_date.Value.Year;
            int hour = txt_search_date.Value.Hour;
            int minute = txt_search_date.Value.Minute;

            String value = year + "-" + month + "-" + day + " " + hour + ":" + minute;


            MessageBox.Show(value);
            loaddatasearch("DATE",value);
        }

        private void txt_search_date_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
