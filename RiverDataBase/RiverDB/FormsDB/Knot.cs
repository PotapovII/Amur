//---------------------------------------------------------------------------
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//        кодировка : 03.10.2020 Потапов И.И. & Потапов Д.И.
//---------------------------------------------------------------------------
namespace RiverDB.FormsDB
{
    using System;
    using System.Data;
    using System.Linq;
    using ConnectLib;
    using System.Windows.Forms;

    public partial class Knot : Form
    {
        KnotUp form;
        // public Microsoft.Office.Interop.Excel.Application excelapp = new Microsoft.Office.Interop.Excel.Application();
        public string TName;
        public string VName;
        int[] sizeColum;
        public Knot(string TName)
        {
            this.TName = TName;
            InitializeComponent();
            DataGridSettings.DataGridViewSetup(ref dataGridView1);
        }

        private void Knot_Load(object sender, EventArgs e)
        {
            sizeColum = new int[] { 15, 20, 20, 20, 20, 20, 20, 20, 20  };
            string strAccessSelect = "exec SP_" + TName + "_SEL";
            dataGridView1.DataSource = ConnectDB.GetDataTable(strAccessSelect, TName);
            Knot_Resize(sender, e);
            this.Text = "Форма списка узлов опорной сети";
        }

        private void Knot_Resize(object sender, EventArgs e)
        {
            double sum = sizeColum.Sum();
            double AllWidth = dataGridView1.Width;
            for (int i = 0; i < sizeColum.Length; i++)
                dataGridView1.Columns[i].Width = (int)(sizeColum[i] * AllWidth / sum);
        }

        private void insertButton_Click(object sender, EventArgs e)
        {
            form = new KnotUp(TName);
            form.Text = TName + " Set";
            if (form.ShowDialog() == DialogResult.OK)
                Knot_Load(sender, e);
        }

        private void updateButton_Click(object sender, EventArgs e)
        {
            if (this.dataGridView1.CurrentRow != null)
            {
                int id = Convert.ToInt32(this.dataGridView1.CurrentRow.Cells[0].Value);
                if (id > 0)
                {
                    form = new KnotUp(TName, id);
                    form.Text = TName + " Set";
                    if (form.ShowDialog() == DialogResult.OK)
                        Knot_Load(sender, e);
                }
            }
        }
        private void findButton_Click(object sender, EventArgs e)
        {
            CFValue(sender, e);
        }
        public void CFValue(object sender, EventArgs e)
        {
            string FDT1 = dateTimePicker1.Value.ToString("yyyy-MM-dd");//("yyyy-MM-dd");
            string FDT2 = dateTimePicker2.Value.ToString("yyyy-MM-dd");//("yyyy-MM-dd");
            string strAccessSelect =
            "SELECT " +
            "[knot_ID] as [код], " +
            "[knot_fulldepth] as [глубина], " +
            "[knot_step] as [шаг]," +
            "[knot_speed] as [скорость]," +
            "[knot_course] as [курс]," +
            "[knot_datetime] as [дата и время]," +
            "[knot_latitude] as [широта]," +
            "[knot_longitude] as [долгота]," +
            "[knot_temperature] as [температура] " +
            "from dbo.knot where knot_datetime > '" + FDT1 + "' and knot_datetime < '" + FDT2 + "'";
            DataTable mapTable = ConnectDB.GetDataTable(strAccessSelect, TName);
            dataGridView1.DataSource = mapTable;
            Knot_Resize(sender, e);
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(this.dataGridView1.CurrentRow.Cells[0].Value);
            if (id > 0)
            {
                ConnectDB.DeleteRow(TName, id);
                Knot_Load(sender, e);
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
            //excelworksheet.Cells[1, 1].Value = dataGridView1.Columns[0].HeaderText;
            //excelworksheet.Cells[1, 2].Value = dataGridView1.Columns[1].HeaderText;
            //excelworksheet.Cells[1, 3].Value = dataGridView1.Columns[2].HeaderText;
            //excelworksheet.Cells[1, 4].Value = dataGridView1.Columns[3].HeaderText;
            //excelworksheet.Cells[1, 5].Value = dataGridView1.Columns[4].HeaderText;
            //excelworksheet.Cells[1, 6].Value = dataGridView1.Columns[5].HeaderText;
            //excelworksheet.Cells[1, 7].Value = dataGridView1.Columns[6].HeaderText;
            //excelworksheet.Cells[1, 8].Value = dataGridView1.Columns[7].HeaderText;
            //excelworksheet.Cells[1, 9].Value = dataGridView1.Columns[8].HeaderText;
            //excelworksheet.Cells[1, 10].Value = dataGridView1.Columns[9].HeaderText;
            //excelworksheet.Cells[1, 11].Value = dataGridView1.Columns[10].HeaderText;
            //for (int i = 0; i < dataGridView1.RowCount; i++)
            //{
            //    excelworksheet.Cells[i + 2, 1].Value = i + 1;
            //    excelworksheet.Cells[i + 2, 2].Value = dataGridView1.Rows[i].Cells[1].Value.ToString();
            //    excelworksheet.Cells[i + 2, 3].Value = dataGridView1.Rows[i].Cells[2].Value.ToString();
            //    excelworksheet.Cells[i + 2, 4].Value = dataGridView1.Rows[i].Cells[3].Value.ToString();
            //    excelworksheet.Cells[i + 2, 5].Value = dataGridView1.Rows[i].Cells[4].Value.ToString();
            //    excelworksheet.Cells[i + 2, 6].Value = dataGridView1.Rows[i].Cells[5].Value.ToString();
            //    excelworksheet.Cells[i + 2, 7].Value = dataGridView1.Rows[i].Cells[6].Value.ToString();
            //    excelworksheet.Cells[i + 2, 8].Value = dataGridView1.Rows[i].Cells[7].Value.ToString();
            //    excelworksheet.Cells[i + 2, 9].Value = dataGridView1.Rows[i].Cells[8].Value.ToString();
            //    excelworksheet.Cells[i + 2, 10].Value = (string)dataGridView1.Rows[i].Cells[9].Value.ToString();
            //    excelworksheet.Cells[i + 2, 11].Value = (string)dataGridView1.Rows[i].Cells[10].Value.ToString();
            //}
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CFValue(sender, e);
        }
    }
}
