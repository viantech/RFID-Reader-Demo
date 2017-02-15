using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;
using System.Data.SQLite;
using System.IO;
namespace GatewayForm
{
    class LogIOData
    {
        public String SQLtablename= Properties.Resources.DATABASE_TABLENAME;
        public String dbname = Properties.Resources.DATABASE_NAME;
        public String currentDBPath = "";
        public String currentExelPath="";

        private object misValue = System.Reflection.Missing.Value;
        private SQLiteConnection sqlConnection = null;
        private List<String[]> dataList = new List<String[]>();
        /* private SqlConnection connectSQL=null;
         private SqlCommand cmdSQL=null;
         private SqlDataAdapter adapterSQL=null;
         private DataTable dtable=null;
         Excel.Range chartRange=null;*/
        private bool _flag_TableSql = false;
        public delegate void LoadDataToTablefromDBbrowser(String[] mgs);
        public event LoadDataToTablefromDBbrowser loadData2Table;
        public LogIOData() { }
        public void CreateDBFolder_currentDirectory()
        {
            string _currentPath = System.IO.Directory.GetCurrentDirectory();
            _currentPath += Properties.Resources.SELDAT_DATABASE;
            if (!File.Exists(_currentPath)) {
                var folder = Directory.CreateDirectory(_currentPath);
            }
            currentDBPath = _currentPath+"\\";
        }
        public void CreateExcelFolder_currentDirectory()
        {
            string _currentExcelPath = System.IO.Directory.GetCurrentDirectory();
            _currentExcelPath += Properties.Resources.SELDAT_EXCEL_DATASTORE;
            if (!File.Exists(_currentExcelPath))
            {
                var folder = Directory.CreateDirectory(_currentExcelPath);
            }
            currentExelPath = _currentExcelPath + "\\";
        }
        public Boolean CreateDBTable()
        {
            try
            {
                string sqlcmd;
                CreateDBFolder_currentDirectory(); // create folder for databse if not exist
                string currentdbpath = currentDBPath + dbname;
               // MessageBox.Show(currentdbpath);
                if (!File.Exists(currentdbpath))
                {
                    SQLiteConnection.CreateFile(currentdbpath);
                    sqlConnection = new SQLiteConnection("Data Source=" + currentdbpath + ";Version=3");
                    sqlcmd = "CREATE TABLE " + SQLtablename + " (TAG varchar(50), ANT varchar(50),RSSI varchar(50), CODE varchar(50),DATE varchar(20))";
                    sqlConnection.Open();
                    SQLiteCommand cmd = new SQLiteCommand(sqlcmd, sqlConnection);
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    sqlConnection = new SQLiteConnection("Data Source=" + currentdbpath + ";Version=3");
                    sqlConnection.Open();
                }
                _flag_TableSql = true;
            
            }
            catch {
                MessageBox.Show("Log ERROR");
            }
            return true;

        }
        public void InsertData2Sql(String[] mgs)
        {        
            if (_flag_TableSql)
            {
                if (mgs.Length > 0)
                {
                    for (int i = 0; i < mgs.Length; i++)
                    {
                        string[] cells;
                        cells = mgs[i].Split(new string[] { "\t" }, StringSplitOptions.None);
                        string sqlcmd = "INSERT INTO "+ SQLtablename + " (TAG,ANT,RSSI,CODE,DATE) values ('" + cells[0] + "','"+ cells[1] + "','"+ cells[2] + "','"+ cells[3] + "','"+ cells[4] + "')";
                        SQLiteCommand cmd = new SQLiteCommand(sqlcmd, sqlConnection);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
        public void AddRow2Sql(String[] cells)
        {
            if (_flag_TableSql)
            {
                string sqlcmd = "INSERT INTO " + SQLtablename + " (TAG,ANT,RSSI,CODE,DATE) values ('" + cells[0] + "','" + cells[1] + "','" + cells[2] + "','" + cells[3] + "','" + cells[4] + "')";
                SQLiteCommand cmd = new SQLiteCommand(sqlcmd, sqlConnection);
                cmd.ExecuteNonQuery();
            }
        }
        public void SearchDataINSql(String field, String inf)
        {
            try
            {
                string sqlcmd = "SELECT TAG,ANT,RSSI,CODE,DATE FROM " + SQLtablename + " WHERE " + field + "='" + inf + "'";
                MessageBox.Show(sqlcmd);
                SQLiteCommand cmd = new SQLiteCommand(sqlcmd, sqlConnection);
                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    String[] mgs = new String[5];
                    mgs[0] = reader["TAG"].ToString();
                    mgs[1] = reader["ANT"].ToString();
                    mgs[2] = reader["RSSI"].ToString();
                    mgs[3] = reader["CODE"].ToString();
                    mgs[4] = reader["DATE"].ToString();
                    loadData2Table(mgs);
                    dataList.Add(mgs); 
                }
            }
            catch
            {
                MessageBox.Show(Properties.Resources.ERROR_SEARCH_DATABASE);
            }
        }
        private void closeSql()
        {
            if(_flag_TableSql)
            {
                sqlConnection.Close();
            }
        }
        private void CreateSQLite()
        {
            try
            {

            }
            catch { }
        }
        public void DownloadExelFile()
        {
            Excel.Application xlApp;
            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet;
            object misValue = System.Reflection.Missing.Value;
            Excel.Range chartRange;
            xlApp = new Excel.Application();
            xlWorkBook = xlApp.Workbooks.Add(misValue);
            xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);
            #region Create a cell titles

            xlWorkSheet.Cells[2, 1] = "TAG";
            xlWorkSheet.Cells[2, 2] = "ANTENNA";
            xlWorkSheet.Cells[2, 3] = "RSSI";
            xlWorkSheet.Cells[2, 4] = "CODE";
            xlWorkSheet.Cells[2, 5] = "DATE/TIME";
            #endregion
            if (dataList.Count>0)
            {
                long endRow = 3; // save endrow num
                foreach (String[] cells in dataList)
                {
                    xlWorkSheet.Cells[endRow, 1] = cells[0]; // Tag
                    xlWorkSheet.Cells[endRow, 2] = cells[1]; // antenna
                    xlWorkSheet.Cells[endRow, 3] = cells[2]; // rssi
                    xlWorkSheet.Cells[endRow, 4] = cells[3]; // code
                    xlWorkSheet.Cells[endRow, 5] = cells[4]; // date/time
                    endRow++;
                }
             }
            xlWorkSheet.Rows.RowHeight = 25;
            xlWorkSheet.StandardWidth = 13;
            xlWorkSheet.Rows.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
            xlWorkSheet.get_Range("A1", "E1").Merge(false);

            chartRange = xlWorkSheet.get_Range("A1", "E1");
            chartRange.FormulaR1C1 = Properties.Resources.SELDAT_EXCEL_TITLE;
            chartRange.HorizontalAlignment = 3;
            chartRange.VerticalAlignment = 3;
            chartRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Yellow);
            chartRange.Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Red);
            chartRange.Font.Size = 20;
            chartRange = xlWorkSheet.get_Range("A2", "A2");
            chartRange.BorderAround(Excel.XlLineStyle.xlContinuous, Excel.XlBorderWeight.xlMedium, Excel.XlColorIndex.xlColorIndexAutomatic, Excel.XlColorIndex.xlColorIndexAutomatic);
            chartRange = xlWorkSheet.get_Range("B2", "B2");
            chartRange.BorderAround(Excel.XlLineStyle.xlContinuous, Excel.XlBorderWeight.xlMedium, Excel.XlColorIndex.xlColorIndexAutomatic, Excel.XlColorIndex.xlColorIndexAutomatic);
            chartRange = xlWorkSheet.get_Range("C2", "C2");
            chartRange.BorderAround(Excel.XlLineStyle.xlContinuous, Excel.XlBorderWeight.xlMedium, Excel.XlColorIndex.xlColorIndexAutomatic, Excel.XlColorIndex.xlColorIndexAutomatic);
            chartRange = xlWorkSheet.get_Range("D2", "D2");
            chartRange.BorderAround(Excel.XlLineStyle.xlContinuous, Excel.XlBorderWeight.xlMedium, Excel.XlColorIndex.xlColorIndexAutomatic, Excel.XlColorIndex.xlColorIndexAutomatic);
            chartRange = xlWorkSheet.get_Range("E2", "E2");
            CreateExcelFolder_currentDirectory();
            xlWorkBook.SaveAs(currentExelPath + CreateNameExelFile() + ".xls", Excel.XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
            xlWorkBook.Close(true, misValue, misValue);
            xlApp.Quit();
            releaseObject(xlApp);
            releaseObject(xlWorkBook);
            releaseObject(xlWorkSheet);
        }
        private string CreateNameExelFile()
        {
            DateTime date = DateTime.Now;
            return "" + date.ToString("yy") + date.ToString("MM") + date.ToString("dd") + "_" + date.ToString("hh") + "'" + date.ToString("mm") + "''" + date.ToString("ss") + "_SELDAT";
        }
        private string GetDateTime()
        {
            DateTime date = DateTime.Now;
            return "" + date.ToString("yy") + date.ToString("MM") + date.ToString("dd") + "_" + date.ToString("hh") + "'" + date.ToString("mm") + "''" + date.ToString("ss");
        }
        public String [] getData()
        {
            String[] data=new String[100];
            return data;
        }
        private string getnameTable()
        {
            DateTime date = DateTime.Now;
            return "" + date.ToString("yy") + date.ToString("MM") + date.ToString("dd") + "_" + date.ToString("hh") + "'" + date.ToString("mm") + "''" + date.ToString("ss") + "seldatGW";
        }
        private void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                MessageBox.Show("Unable to release the Object " + ex.ToString());
            }
            finally
            {
                GC.Collect();
            }
        }
    }
}
