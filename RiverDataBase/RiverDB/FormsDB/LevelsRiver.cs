//---------------------------------------------------------------------------
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//        кодировка : 04.10.2020 Потапов И.И. & Потапов Д.И.
//---------------------------------------------------------------------------
namespace RiverDB.FormsDB
{
    using System;
    using System.Data;
    using System.Linq;
    //using Excel = Microsoft.Office.Interop.Excel;
    using System.Windows.Forms;
    using ConnectLib;

    public partial class LevelsRiver : Form
    {
        //public Microsoft.Office.Interop.Excel.Application excelapp = new Microsoft.Office.Interop.Excel.Application();
        LevelsRiverUp form;
        public string TName;
        public string VName;
        int[] sizeColum;
        public LevelsRiver(string TName)
        {
            this.TName = TName;
            InitializeComponent();
            DataGridSettings.DataGridViewSetup(ref dataGridView1);
        }

        private void insertButton_Click(object sender, EventArgs e)
        {
            form = new LevelsRiverUp();
            form.TName = TName;
            form.Text = TName + " Set";
            if (form.ShowDialog() == DialogResult.OK)
                LevelsRiver_Load(sender, e);
        }
        private void updateButton_Click(object sender, EventArgs e)
        {
            try
            {
                int id = Convert.ToInt32(this.dataGridView1.CurrentRow.Cells[0].Value);
                if (id > 0)
                {
                    form = new LevelsRiverUp(id);
                    form.TName = TName;
                    form.Text = TName + " Set";
                    if (form.ShowDialog() == DialogResult.OK)
                        LevelsRiver_Load(sender, e);
                }
            }
            catch(Exception ex)
            {
                textBox1.Text = ex.Message;
            }
        }
        private void LevelsRiver_Load(object sender, EventArgs e)
        {
            this.Text = TName;
            sizeColum = new int[] { 10, 35, 25, 15, 15 };
            string strAccessSelect = "exec SP_" + TName + "_SEL";
            dataGridView1.DataSource = ConnectDB.GetDataTable(strAccessSelect, TName);
            LevelsRiver_Resize(sender, e);
            this.Text = "Форма списка речных глубин по гидропостам";
        }

        private void LevelsRiver_Resize(object sender, EventArgs e)
        {
            double sum = sizeColum.Sum();
            double AllWidth = dataGridView1.Width;
            for (int i = 0; i < sizeColum.Length; i++)
                dataGridView1.Columns[i].Width = (int)(sizeColum[i] * AllWidth / sum);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != null)
                CFValue(sender, e);
        }
        private void findButton_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != null)
                CFValue(sender, e);
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
            //    excelworksheet.Cells[i + 1, 1].Value = (string)dataGridView1.Rows[i].Cells[0].Value.ToString();//i + 1;
            //    excelworksheet.Cells[i + 1, 2].Value = (string)dataGridView1.Rows[i].Cells[1].Value.ToString();
            //    excelworksheet.Cells[i + 1, 3].Value = (string)dataGridView1.Rows[i].Cells[2].Value.ToString();
            //    excelworksheet.Cells[i + 1, 4].Value = (string)dataGridView1.Rows[i].Cells[3].Value.ToString();
            //    excelworksheet.Cells[i + 1, 5].Value = (string)dataGridView1.Rows[i].Cells[4].Value.ToString();
            //}
        }

        public void CFValue(object sender, EventArgs e)
        {
            char[] charsToTrim = { ' ' };
            string pole = textBox1.Text.Trim(charsToTrim);
            string strAccessSelect =
             "select [experiment_ID] as [код], [place_name] as [участок]," +
             "[experiment_datetime] as [дата и время], [experiment_waterlevel] as" +
             " [уровень воды],[experiment_middlewaterlevel] as [норма] " +
             " from dbo.view_experiment where place_name LIKE @POLE ";
            DataTable mapTable = ConnectDB.SelectTableBystrKey(strAccessSelect, TName, pole);
            dataGridView1.DataSource = mapTable;
            LevelsRiver_Resize(sender, e);
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(this.dataGridView1.CurrentRow.Cells[0].Value);
            if (id > 0)
            {
                ConnectDB.DeleteRow(TName, id);
                LevelsRiver_Resize(sender, e);
            }
        }
    }
}
