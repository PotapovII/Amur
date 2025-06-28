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
        /// <summary>
        /// Фильтр синхронизации данных по долготе >
        /// </summary>
        string Filter = "190";
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
            openFileDialog1.FileName = "file";
            openFileDialog1.Filter = "файл наблюдения Excel (*.xls)|*.xls|" +
                                     "файл наблюдения gpx (*.gpx)|*.gpx|" +
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
            string sql = "SELECT CAST(knot.knot_datetime AS DATE) as mydate FROM knot"
                       + " GROUP BY CAST(knot.knot_datetime AS DATE) ORDER BY mydate";
            DataTable mapTable = ConnectDB.GetDataTable(sql, TName);
            cListBoxDates.Items.Clear();
            foreach (DataRow dr in mapTable.Rows)
                cListBoxDates.Items.Add(dr["mydate"]);
            ts_ClearBadData.Enabled = true;
        }
        private void ForBtLoadData()
        {
            string sql = "SELECT CAST(forknot.forknot_datetime AS DATE) as mydate FROM forknot"
                       + " GROUP BY CAST(forknot.forknot_datetime AS DATE) ORDER BY mydate";
            DataTable mapTableGPX = ConnectDB.GetDataTable(sql, TName);
            cListBoxDatesGPX.Items.Clear();
            foreach (DataRow dr in mapTableGPX.Rows)
                cListBoxDatesGPX.Items.Add(dr["mydate"]);
            ts_ClearBadData.Enabled = true;
        }

        private void insertButton_Click(object sender, EventArgs e)
        {
            loadDBToolStripMenuItem_Click(sender, e);
        }

        private void loadDBToolStripMenuItem_Click(object sender, EventArgs e)
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
            convertDBToolStripMenuItem_Click(sender, e);
        }



        private void convertDBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (list.Count > 0)
            {
                string result = "";
                int Count = 0;

                if (FileEXT == ".xls" || FileEXT == ".XLS")
                {
                    // Добавление записей в базу данных
                    if (ConnectDB.DoListCommand(list, TypeCommand.insert, ref Count, ref result) == true)
                    {
                        Console.WriteLine("Данные добавлены в базу данных");
                        // отрисовка
                        BtLoadData();
                    }
                }
                if (FileEXT == ".gpx" || FileEXT == ".GPX")
                {
                    // Добавление записей в базу данных
                    if (ConnectDB.DoListCommand(list, TypeCommand.insert, ref Count, ref result) == true)
                    {
                        string strCom = "update[dbo].knot set knot_latitude = forknot_latitude," +
                        "knot_longitude = forknot_longitude, knot_datetime = " +
                        "forknot_datetime,knot_fulldepth = forknot_fulldepth, knot_temperature =" +
                        "forknot_temperature from knot, forknot where knot_datetime = forknot_datetime," +
                        "knot_N = forknot_latitude, knot_E = forknot_longitude";
                        // если успешно, обновляем поля
                        ConnectDB.DoSqlTransactionCommand(strCom);
                        // отрисовка
                        ForBtLoadData();
                    }
                }
                if (FileEXT == ".dat")
                {
                    if (ConnectDB.DoListCommand(list, TypeCommand.insert, ref Count, ref result) == true)
                    {
                        // присвоить текст комманды SQL по вставке узла
                        ExtStandartOld knots = (ExtStandartOld)list[0];
                        DateTime dataTime = knots.dataTime.Date;
                        string SdataTime = dataTime.ToString("yyyy-MM-dd HH:mm:ss:fff");
                        string seeWaterleve = "SELECT experiment_waterlevel, " +
                            "experiment_datetime FROM experiment" +
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

        private void updateButton_Click(object sender, EventArgs e)
        {
            Sin();
        }
        private void Sin()
        {
            string strCom = "update[dbo].knot set knot_latitude = forknot_latitude," +
            "knot_longitude = forknot_longitude, knot_datetime = " +
            "forknot_datetime,knot_fulldepth = forknot_fulldepth, knot_temperature =" +
            "forknot_temperature from knot, forknot where knot_datetime = forknot_datetime," +
            "knot_N = forknot_latitude, knot_E = forknot_longitude";
            // если успешно, обновляем поля
            ConnectDB.DoSqlTransactionCommand(strCom);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
        /// <summary>
        /// Отрисовка помощи
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string[] helps = File.ReadAllLines(@"ConvertorIn\HelpConverter.txt");
            lbExp.Items.Clear();
            foreach (string help in helps)
                lbExp.Items.Add(help);
        }
        /// <summary>
        ///  выполняеем сборку мусора - не синхронизированные данных
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ts_ClearBadData_Click(object sender, EventArgs e)
        {
            string strClearErr = "delete from knot where knot.knot_id in " +
                "( select knot.knot_id from knot where CAST(knot.knot_longitude AS int) > " + Filter + ")";
            string result = ConnectDB.DoSqlTransactionCommand(strClearErr);
            if (result == "")
                Console.WriteLine("Удаление не синхронизированных данных добавленых в базу данных");
            else
                Console.WriteLine(result);
        }

        #region Старые методы
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
                    if (ConnectDB.DoListCommand(list, TypeCommand.insert, ref Count, ref result) == true)
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
                    if (ConnectDB.DoListCommand(list, TypeCommand.insert, ref Count, ref result) == true)
                    {
                        ConnectDB.DoSqlTransactionCommand(strCom);
                        ForBtLoadData();
                    }
                }
                if (FileEXT == ".dat")
                {
                    if (ConnectDB.DoListCommand(list, TypeCommand.insert, ref Count, ref result) == true)
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

        #endregion

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            string filter = " ( ";
            for (int i = 0; i < cListBoxDatesGPX.Items.Count; i++)
            {
                CheckState flag = cListBoxDatesGPX.GetItemCheckState(i);
                if (flag == CheckState.Checked)
                {
                    string d = cListBoxDatesGPX.Items[i].ToString();
                    DateTime dataA = DateTime.Parse(d);
                    string FL = dataA.ToString("yyyy-MM-dd ");
                    filter += "'" + FL + "',";
                }
            }
            filter += " '2100.01.01' ) ";
            string place_id = "1"; // Хабаровск
            string strSelect = "SELECT knot_id, knot_latitude, knot_longitude, knot_fulldepth, knot_depth, knot_temperature," +
                            " knot_speed, knot_course, knot_datetime, CAST(knot.knot_datetime AS DATE) as DTime," +
                            " experiment_waterlevel" +
                            " FROM knot, experiment where  experiment.place_id = " + place_id + " and " +
                            " CAST(knot.knot_datetime AS DATE) = CAST(experiment_datetime AS DATE) " +
                            " and CAST(knot.knot_datetime AS DATE) IN " + filter;
            DataTable pTable = ConnectDB.GetDataTable(strSelect, TName);
            List<IStandartSQLCommand> uList = new List<IStandartSQLCommand>();
            foreach (DataRow dr in pTable.Rows)
            {
                int ID = Convert.ToInt32(dr["knot_id"]);
                double depth = Convert.ToDouble(dr["knot_fulldepth"]);
                double Hg = Convert.ToDouble(dr["experiment_waterlevel"]) / 100.0;
                // Срезка глубин
                double sDepth = depth - Hg;
                ExtStandart point = new ExtStandart();
                point.ID = ID;
                point.depth = depth;
                point.sDepth = sDepth;
                uList.Add(point);
            }
            int Count = 0;
            string result = "";
            if (ConnectDB.DoListCommand(uList, TypeCommand.updateDeapth, ref Count, ref result) == true)
            {
                ForBtLoadData();
            }
        }
        /// <summary>
        /// Внесение макрера скорости для точек
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsb_VelocityMark_Click(object sender, EventArgs e)
        {
            string filter = openFileDialog1.Filter;
            openFileDialog1.Filter = "файл наблюдения Excel (*.xls)|*.xls|" +
                                     "All files (*.*)|*.*";
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
                    if (FileEXT == ".xls")
                    {
                        List<IStandartSQLCommand> listMarkUpdate = Convert_GPX_to_Standart.LoadExcel(FileName);
                        foreach (var p in listMarkUpdate)
                            lbExp.Items.Add(((StandartGPX)p).ToString());
                        tsLab.Text = "Загружен файл наблюдений за поверхностной скоростью " + FName + 
                            " содержащий " + lbExp.Items.Count.ToString() + " записей";
                        int Count = 0;
                        string result = "";
                        if (ConnectDB.DoListCommand(listMarkUpdate, TypeCommand.updateMark, ref Count, ref result) == true)
                        {
                            ForBtLoadData();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    if (list == null)
                        list = new List<IStandartSQLCommand>();
                }
            }
            openFileDialog1.Filter = filter;
        }
    }
}
