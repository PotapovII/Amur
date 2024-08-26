namespace RiverDB.Convertors
{
    using System;
    using System.IO;
    using System.Data;
    using System.Reflection;
    using System.Windows.Forms;
    using System.Collections.Generic;
    using ConnectLib;

    public partial class ConvertForm : Form
    {
        string FileEXT = "";
        string FName = "";
        /// <summary>
        /// буффер gpx
        /// </summary>
        List<IStandartSQLCommand> list = new List<IStandartSQLCommand>();
        /// <summary>
        /// Таблица точек 
        /// </summary>
        string TName = "knot";

        public ConvertForm()
        {
            InitializeComponent();
            openFileDialog1.InitialDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            openFileDialog1.Filter = "файл наблюдения gpx (*.gpx)|*.gpx|" +
                                     "файл наблюдения Excel (*.xls)|*.xls|" +
                                     "файл наблюдения dat (*.dat)|*.dat|" +
                                     "All files (*.*)|*.*";
        }

        private void ConvertForm_Load(object sender, EventArgs e)
        {
            BtLoadData();
            ForBtLoadData();
        }

        private void BtLoadData()
        {
            string strSelect = "SELECT CAST(knot.knot_datetime AS DATE) as mydate FROM knot"
                            + " GROUP BY CAST(knot.knot_datetime AS DATE) ORDER BY mydate";
            DataTable mapTable = ConnectDB.GetDataTable(strSelect, TName);
            cListBoxDates.Items.Clear();
            foreach (DataRow dr in mapTable.Rows)
                cListBoxDates.Items.Add(dr["mydate"]);
            btClear.Enabled = true;
        }
        private void ForBtLoadData()
        {
            string strSelectGPX = "SELECT CAST(forknot.forknot_datetime AS DATE) as mydate FROM forknot"
                            + " GROUP BY CAST(forknot.forknot_datetime AS DATE) ORDER BY mydate";
            DataTable mapTableGPX = ConnectDB.GetDataTable(strSelectGPX, TName);
            cListBoxDatesGPX.Items.Clear();
            foreach (DataRow dr in mapTableGPX.Rows)
                cListBoxDatesGPX.Items.Add(dr["mydate"]);
            btClear.Enabled = true;
        }
        /// <summary>
        /// Загрузка данных
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btInsertData_Click(object sender, EventArgs e)
        {
            if (list.Count > 0)
            {
                string result = "";
                int Count = 0;

                if (FileEXT == ".xls" || FileEXT == ".XLS")
                {
                    if (ConnectDB.DoListCommand(list, ref Count, ref result) == true)
                    {
                        Console.WriteLine("Данные добавлены в базу данных");
                        BtLoadData();
                    }
                }
                if (FileEXT == ".gpx" || FileEXT == ".GPX")
                {
                    string strCom = "update[dbo].knot set knot_latitude = forknot_latitude," +
                    "knot_longitude = forknot_longitude, knot_datetime = " +
                    "forknot_datetime,knot_fulldepth = forknot_fulldepth, knot_temperature =" +
                    "forknot_temperature from knot, forknot where knot_datetime = forknot_datetime";
                    if (ConnectDB.DoListCommand(list, ref Count, ref result) == true)
                    {
                        ConnectDB.DoSqlTransactionCommand(strCom);
                        ForBtLoadData();
                    }
                }
                if (FileEXT == ".dat")
                {
                    if (ConnectDB.DoListCommand(list, ref Count, ref result) == true)
                    {
                        // присвоить текст комманды SQL по вставке узла
                        ExtStandartOld knots = (ExtStandartOld)list[0];
                        DateTime dataTime = knots.dataTime.Date;
                        string SdataTime = dataTime.ToString("yyyy-MM-dd HH:mm:ss:fff");
                        string seeWaterleve = "SELECT experiment_waterlevel, experiment_datetime FROM experiment" +
                        " where CAST(experiment_datetime AS DATE)  IN('" + SdataTime + "')";
                        DataTable table = ConnectDB.GetDataTable(seeWaterleve, "experiment");
                        if (table.Rows.Count == 0)
                        {
                            double cut = 100 * (knots.depth - knots.cutDepth);
                            string sInsert = "insert into experiment "
                            + "(place_id, experiment_datetime, experiment_waterlevel,experiment_MiddleWaterLevel)"
                            + "values (1,'" + SdataTime + "'," + cut.ToString() + "," + cut.ToString() + ")";
                            ConnectDB.SQLCommandDo(sInsert);
                            BtLoadData();
                            BtLoadData();
                        }
                    }
                }
                tsLab.Text = result;
                tsLab.Text = "В базу данных добавлено " + Count.ToString()
                        + " записей из " + list.Count.ToString() + " , файл: dat";
                lbExp.Items.Clear();
                list.Clear();
            }
        }
        private void btCansel_Click(object sender, EventArgs e)
        {
            Close();
        }
        /// <summary>
        /// выполняеем сборку мусора - не синхронизированные данных
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btClear_Click(object sender, EventArgs e)
        {
            string strClearErr = "delete from knot where knot.knot_id in " +
                "( select knot.knot_id from knot where CAST(knot.knot_longitude AS int) > "+ tbFilter.Text.Trim() + ")";
            string result = ConnectDB.DoSqlTransactionCommand(strClearErr);
            if (result == "")
                Console.WriteLine("Удаление не синхронизированных данных добавленых в базу данных");
            else
                Console.WriteLine(result);
        }
        /// <summary>
        /// Загрузка данных различных форматов
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btOpenData_Click(object sender, EventArgs e)
        {
            lbExp.Items.Clear();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // имя файла
                string FileName = openFileDialog1.FileName;
                FName = Path.GetFileName(FileName);
                // расширение файла
                FileEXT = Path.GetExtension(FileName).ToLower();
                try
                {
                    switch (FileEXT)
                    {
                        case ".gpx":
                            list = Convert_GPX_to_Standart.LoadGPX(FileName);
                            break;
                        case ".xls":
                            list = Convert_GPX_to_Standart.LoadExcel(FileName);
                            break;
                        case ".dat":
                            list = Convert_GPX_to_Standart.LoadDat(FileName);
                            break;
                    }
                    foreach (var p in list)
                        lbExp.Items.Add(((StandartGPX)p).ToString());
                    if (list.Count > 0)
                        btInsertData.Enabled = true;
                    tsLab.Text = "Загружен файл " + FName + " содержащий " + lbExp.Items.Count.ToString() + " записей";
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    if (list == null)
                        list = new List<IStandartSQLCommand>();
                }
            }
        }
    }
}
