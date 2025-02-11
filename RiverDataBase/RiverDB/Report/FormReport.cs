//---------------------------------------------------------------------------
//                   ПРОЕКТ  "Русловые процессы"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 26.11.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace RiverDB.Report
{
    using System;
    using System.Data;
    using System.Windows.Forms;
    using System.Collections.Generic;

    using MeshLib;
    using RenderLib;
    using CommonLib;
    using ConnectLib;
    using RiverDB.FormsDB;
    using GeometryLib.Vector;
    using System.Linq;

    public partial class FormReportWL : Form
    {
        string TName = "Experiment";
        Catalogue book = new Catalogue("place", true);
        int id = 1;
        public FormReportWL()
        {
            InitializeComponent();
            tb2.Text = id.ToString();
            tb21.Text = ConnectDB.GetName("place", id);
            pnRF.Visible = true;
        }
        
        private void bt1_Click(object sender, EventArgs e)
        {
            book.ShowDialog();
            tb2.Text = book.RowID;
            tb21.Text = book.RowName;
            switch(tb2.Text.Trim())
            {
                case "1":
                    pnRF.Visible = true;
                    rbAmur.Visible = true;
                    rbPemza.Visible = true;
                    rbMad.Visible = true;
                    break;
                case "3":
                    pnRF.Visible = true;
                    rbAmur.Visible = false;
                    rbPemza.Visible = false;
                    rbMad.Visible = false;
                    break;
                default:
                    pnRF.Visible = false;
                    break;
            }
            cListBoxDates.Items.Clear();
        }

        private void btLoadRateWL_Click(object sender, EventArgs e)
        {
            GetDataFilter();
        }
        /// <summary>
        /// Получить списаок дат работы экспедиций
        /// </summary>
        protected void GetDataFilter()
        {
            try
            {
                string sql = "select DISTINCT datepart(yyyy, [experiment_datetime]) as [year] "
                            + "from[Experiment] where [place_id] = '"+ tb2.Text.Trim() + "' order by[year]";
                DataTable mapTable = ConnectDB.GetDataTable(sql, TName);
                cListBoxDates.Items.Clear();
                foreach (DataRow dr in mapTable.Rows)
                    cListBoxDates.Items.Add(dr["year"]);
                for (int i = 0; i < cListBoxDates.Items.Count; i++)
                    cListBoxDates.SetItemChecked(i, cbAllData.Checked);
            }
            catch (Exception ex)
            {
            }
        }

        private void cbAllData_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < cListBoxDates.Items.Count; i++)
                cListBoxDates.SetItemChecked(i, cbAllData.Checked);
        }
        
        /// <summary>
        /// Вычисление графиков уровней по гидропостам в различные годы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btWaterlevel_Click(object sender, EventArgs e)
        {
            ISavePoint sp = new SavePoint();
            for (int i = 0; i < cListBoxDates.Items.Count; i++)
            {
                CheckState flag = cListBoxDates.GetItemCheckState(i);
                if (flag == CheckState.Checked)
                {
                    string year = cListBoxDates.Items[i].ToString();
                    List<Vector2> fun = GetWaterLevel(year);
                    sp.AddCurve(year, (fun.Select(x => x.X)).ToArray(), (fun.Select(x => x.Y)).ToArray());
                }
            }
            FVCurves form = new FVCurves(sp);
            form.Text = "График уровней реки Амур г.п.: " + tb21.Text;
            form.Show();
        }
       
        private void btRiverFlow_Click(object sender, EventArgs e)
        {
            ISavePoint sp = new SavePoint();
            IRiverFlow rf = null;
            switch (tb2.Text.Trim())
            {
                case "1":
                    {
                        if (rbAmur.Checked == true)
                            rf = new RiverFlowAmur();
                        else
                        {
                            if (rbPemza.Checked == true)
                                rf = new RiverFlowPemza();
                            else
                                rf = new RiverFlowMad();
                        }
                    }
                    break;
                case "3":
                    rf = new RiverFlowKomsomolskNaAmure();
                    break;
                default:
                    break;
            }
            if (rf == null)
            {
                MessageBox.Show("Функция связи для расхода не опредедлена");
                return;
            }
            for (int i = 0; i < cListBoxDates.Items.Count; i++)
            {
                CheckState flag = cListBoxDates.GetItemCheckState(i);
                if (flag == CheckState.Checked)
                {
                    string year = cListBoxDates.Items[i].ToString();
                    List<Vector2> fun = GetWaterLevel(year);
                    // Пересчет уровней в расход по переходной кривной
                    for(int j = 0; j < fun.Count; j++)
                    {
                        Vector2 p = fun[j];
                        p.Y = rf.GetRiverFlow(p.Y);
                        // по Н.Н. Бортин, В.M. Mилаев, А.М. Горчаков 2020 DOI: 10.35567/1999-4508-2020-2-5
                        //double x = p.Y;
                        // p.Y = 0.025 * x * x + 26.4793 * x + 7170.74;
                        fun[j] = p;
                    }
                    sp.AddCurve(year, (fun.Select(x => x.X)).ToArray(), (fun.Select(x => x.Y)).ToArray());
                }
            }
            FVCurves form = new FVCurves(sp);
            form.Text = rf.RiverName + ", Расходы м^3/c" ;
            form.Show();
        }
        /// <summary>
        /// Получить 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        protected List<Vector2> GetWaterLevel(string year, bool MiddleWaterLevel = false)
        {
            string RowID = tb2.Text;
            string sql = "SELECT [experiment_id],[place_id],[experiment_datetime]" +
                          ",[experiment_waterlevel],[experiment_MiddleWaterLevel] " +
                          " FROM[RiverDB].[dbo].[Experiment] " +
                          " WHERE datepart(yyyy, [experiment_datetime]) = '" + year +
                          "' AND[place_id] = '" + RowID.Trim() + "'";
            DataTable mapTable = ConnectDB.GetDataTable(sql, TName);
            List<Vector2> fun = new List<Vector2>();
            double waterlevel;
            foreach (DataRow dr in mapTable.Rows)
            {
                if (MiddleWaterLevel == true)
                    waterlevel = (double)dr["experiment_MiddleWaterLevel"];
                else
                    waterlevel = (double)dr["experiment_waterlevel"];
                DateTime now = (DateTime)dr["experiment_datetime"];
                int day = now.DayOfYear;
                Vector2 d = new Vector2(day, waterlevel);
                fun.Add(d);
            }
            fun.Sort();
            return fun;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
