using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GatewayForm
{
    public class Tag_Struct
    {
        public string EPC { get; set; }
        public string Read_Antena { get; set; }
        public string RSSI { get; set; }
        public string Read_Count { get; set; }
        public string Date { get; set; }
        public Tag_Struct (string EPC, string antena, string rssi, string read_count, string date)
        {
            this.EPC = EPC;
            this.Read_Antena = antena;
            this.RSSI = rssi;
            this.Read_Count = read_count;
            this.Date = date;
        }
        public DataGridViewRow To_Row()
        {
            DataGridViewRow row = new DataGridViewRow();
            row.Cells[0].Value = EPC;
            row.Cells[1].Value = Read_Antena;
            row.Cells[2].Value = RSSI;
            row.Cells[3].Value = Read_Count;
            row.Cells[4].Value = Date;
            return row;
        }
    }
}
