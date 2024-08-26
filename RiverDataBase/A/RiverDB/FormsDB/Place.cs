using ConnectLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RiverDB.FormsDB
{
    public partial class Place : Form
    {
        //public static string SConnection = MainForm.SConnection;//string SConnection = Settings.connectpath;
        PlaceUp form;
        //public Microsoft.Office.Interop.Excel.Application excelapp = new Microsoft.Office.Interop.Excel.Application();
        public string TName;
        public string VName;
        int[] sizeColum;
        public Place(string TName)
        {
            this.TName = TName;
            InitializeComponent();
            DataGridSettings.DataGridViewSetup(ref dataGridView1);
        }

        private void insertButton_Click(object sender, EventArgs e)
        {
            form = new PlaceUp(TName);
            form.Text = TName + " Set";
            if (form.ShowDialog() == DialogResult.OK)
                Place_Load(sender, e);
        }

        private void updateButton_Click(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(this.dataGridView1.CurrentRow.Cells[0].Value);
            if (id > 0)
            {
                form = new PlaceUp(TName,id);
                form.Text = TName + " Set";
                if (form.ShowDialog() == DialogResult.OK)
                    Place_Load(sender, e);
            }
        }

        private void findButton_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != null)
            {
                char[] charsToTrim = { ' ' };
                string pole = textBox1.Text.Trim(charsToTrim);
                string strAccessSelect =
                    "select[place_id] as[код],[place_name]as[название],[place_x]as[x-координата],[place_y]as[y-координата]," +
                    "[place_height]as[отметка репера],[place_nullheight]as[положение нуля]" +
                    "from dbo.place where place_Name LIKE @POLE ";
                DataTable mapTable = ConnectDB.SelectTableBystrKey(strAccessSelect, TName, pole);
                dataGridView1.DataSource = mapTable;
                Place_Resize(sender, e);
                dataGridView1.RowHeadersVisible = false;
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            }
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(this.dataGridView1.CurrentRow.Cells[0].Value);
            if (id > 0)
            {
                ConnectDB.DeleteRow(TName, id);
                Place_Load(sender, e);
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            //excelapp = new Excel.Application();
            //Excel.Workbook excelworkbook;
            //excelworkbook = excelapp.Workbooks.Add();
            //Excel.Worksheet excelworksheet;
            //excelworksheet = (Excel.Worksheet)excelworkbook.Worksheets.get_Item(1);
            //excelapp.SheetsInNewWorkbook = 1;
            //excelapp.Workbooks.Add(Type.Missing);
            //excelapp.Visible = true;
            //excelworksheet.Activate();
            //for (int i = 0; i < dataGridView1.RowCount; i++)
            //{
            //    excelworksheet.Cells[i + 1, 1].Value = i + 1;
            //    excelworksheet.Cells[i + 1, 2].Value = (string)dataGridView1.Rows[i].Cells[1].Value.ToString();
            //    excelworksheet.Cells[i + 1, 3].Value = (string)dataGridView1.Rows[i].Cells[2].Value.ToString();
            //    excelworksheet.Cells[i + 1, 4].Value = (string)dataGridView1.Rows[i].Cells[3].Value.ToString();
            //    excelworksheet.Cells[i + 1, 5].Value = (string)dataGridView1.Rows[i].Cells[4].Value.ToString();
            //    excelworksheet.Cells[i + 1, 6].Value = (string)dataGridView1.Rows[i].Cells[5].Value.ToString();
            //    excelworksheet.Cells[i + 1, 7].Value = (string)dataGridView1.Rows[i].Cells[6].Value.ToString();
            //    excelworksheet.Cells[i + 1, 8].Value = (string)dataGridView1.Rows[i].Cells[7].Value.ToString();
            //    excelworksheet.Cells[i + 1, 9].Value = (string)dataGridView1.Rows[i].Cells[8].Value.ToString();
            //}
        }

        private void button1_Click(object sender, EventArgs e)
        {
            findButton_Click(sender, e);
        }

        private void Place_Load(object sender, EventArgs e)
        {
            this.Text = TName;
            string strAccessSelect = " ";
            sizeColum = new int[] { 10, 30, 15, 15, 15, 15 };
            strAccessSelect = "exec SP_" + TName + "_SEL";
            dataGridView1.DataSource = ConnectDB.GetDataTable(strAccessSelect, TName);
            Place_Resize(sender, e);
            this.Text = "Форма характеристик гидропостов";
        }

        private void Place_Resize(object sender, EventArgs e)
        {
            double sum = sizeColum.Sum();
            double AllWidth = dataGridView1.Width;
            for (int i = 0; i < sizeColum.Length; i++)
                dataGridView1.Columns[i].Width = (int)(sizeColum[i] * AllWidth / sum);
        }
    }
}
