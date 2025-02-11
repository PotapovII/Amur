//---------------------------------------------------------------------------
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//        кодировка : 01.10.2020 Потапов И.И. & Потапов Д.И.
//---------------------------------------------------------------------------
namespace RiverDB.FormsDB
{
    using System;
    using System.Data;
    using ConnectLib;
    using System.Windows.Forms;

    public partial class Catalogue : Form
    {
        //public Microsoft.Office.Interop.Excel.Application excelapp = new Microsoft.Office.Interop.Excel.Application();
        public string RowID = "";
        public string RowName = "";
        public string TName;
        public bool FMode = true;
        public Catalogue(string tablename, bool Mode = false)
        {
            InitializeComponent();
            TName = tablename;
            DataGridSettings.DataGridViewSetup(ref dataGridView1);
        }
        /// <summary>
        /// Получить первую запись таблицы
        /// </summary>
        /// <param name="RowID"></param>
        /// <param name="RowName"></param>
        public void GetFirstValue(ref string RowID,ref string RowName)
        {
            string strAccessSelect = "exec SP_" + TName + "_SEL";
            dataGridView1.DataSource = ConnectDB.GetDataTable(strAccessSelect, TName);
            int id = Convert.ToInt32(this.dataGridView1.CurrentRow.Cells[0].Value);
            if (id > 0)
            {
                RowID = dataGridView1.CurrentRow.Cells[0].Value.ToString();
                RowName = (string)dataGridView1.CurrentRow.Cells[1].Value;
            }
        }


        private void Catalogue_Load(object sender, EventArgs e)
        {
            string strAccessSelect = "exec SP_" + TName + "_SEL";
            dataGridView1.DataSource = ConnectDB.GetDataTable(strAccessSelect, TName);
            Catalogue_Resize(sender, e);
            this.Text = "Форма справочника гидропостов";
        }

        private void Catalogue_Resize(object sender, EventArgs e)
        {
            int width = 50;
            dataGridView1.Columns[0].Width = width;   // ID
            dataGridView1.Columns[1].Width = dataGridView1.Width - width;  // Name
            dataGridView1.Columns[0].Name = "Код"; dataGridView1.Columns[1].Name = "Наименование";
        }

        private void insertButton_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != null)
            {
                string strCom = "exec SP_" + TName + "_ADD '" + textBox1.Text + "'";
                ConnectDB.SQLCommandDo(strCom);
                Catalogue_Load(sender, e);
            }
        }

        private void updateButton_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != null)
            {
                int id = Convert.ToInt32(this.dataGridView1.CurrentRow.Cells[0].Value);
                if (id < 0) return;
                string strCom = "exec SP_" + TName + "_UPD " + id.ToString() + ",'" + textBox1.Text + "'";
                ConnectDB.SQLCommandDo(strCom);
                Catalogue_Load(sender, e);
            }
        }

        private void findButton_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != null)
            {
                char[] charsToTrim = { ' ' };
                string pole = textBox1.Text.Trim(charsToTrim);
                string strCommand = "SELECT " + TName + "_ID as [Код]," + TName +
                    "_Name as [Наименование] FROM " + TName + " WHERE " + TName + "_Name LIKE @POLE";
                DataTable mapTable = ConnectDB.SelectTableBystrKey(strCommand, TName, pole);
                dataGridView1.DataSource = mapTable;
                Catalogue_Resize(sender, e);
            }
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(this.dataGridView1.CurrentRow.Cells[0].Value);
            if (id < 0) return;
            string strCom = "exec SP_" + TName + "_DEL " + id.ToString();
            ConnectDB.SQLCommandDo(strCom);
            Catalogue_Load(sender, e);
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
            //}
        }

        private void button1_Click(object sender, EventArgs e)
        {
            findButton_Click(sender, e);
        }

        private void dataGridView1_DoubleClick(object sender, EventArgs e)
        {
            if (FMode)
            {
                RowID = dataGridView1.CurrentRow.Cells[0].Value.ToString();
                RowName = (string)dataGridView1.CurrentRow.Cells[1].Value;
                Close();
            }
        }
    }
}
