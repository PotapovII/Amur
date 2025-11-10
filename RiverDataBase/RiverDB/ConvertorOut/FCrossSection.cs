//---------------------------------------------------------------------------
//                   ПРОЕКТ  "Русловые процессы"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 01.02.2025 Потапов И.И.
//---------------------------------------------------------------------------
namespace RiverDB.ConvertorOut
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;

    using MemLogLib;
    using ConnectLib;

    using CommonLib;
    using CommonLib.IO;
    using CommonLib.Function;

    using GeometryLib;

    using RiverDB.Report;
    using RiverDB.FormsDB;

    using NPRiverLib.APRiver1YD.Params;

    using HelpLib;
    using RenderLib;
    using RenderLib.PDG;

    using MeshLib;
    using MeshGeneratorsLib.StripGenerator;
    using CommonLib.Geometry;
    using GeometryLib.Geometry;
    using CommonLib.Physics;
    using NPRiverLib.APRiver1YD;

    public partial class FCrossSection : Form
    {
        /// <summary>
        /// Данные о створе
        /// </summary>
        CrossSectionData csData = new CrossSectionData();
        /// <summary>
        /// Вспомогательная функция срезанных отметок
        /// </summary>
        protected IDigFunction CSWaterLevels;
        /// <summary>
        /// Справочник гидропостов
        /// </summary>
        Catalogue book = new Catalogue("place", true);
        /// <summary>
        /// дата замера в формате строки
        /// </summary>
        string Data;
        /// <summary>
        /// Номер гидропоста
        /// </summary>
        int placeID = 1;
        /// <summary>
        /// Уровнень реки по гидропосту
        /// </summary>
        double WLRiver = 0;
        /// <summary>
        /// Отметка свободной поверхности по гидропосту в метрах по балтийской системе
        /// </summary>
        double WL = 0;
        /// <summary>
        /// Расход
        /// </summary>
        double QRiver = 0;
        /// <summary>
        /// название гидропоста
        /// </summary>
        string NameGP = "", X = "", Y = "";
        /// <summary>
        /// 0 графика гидропоста по балтийской ситеме
        /// </summary>
        double Zerro = 0;
        /// <summary>
        /// отметка поймы по гидропосту по балтийской ситеме
        /// </summary>
        double floodplain = 0;

        const string Ext_RvY = ".rvy";
        const string Ext_Crf = ".crf";
        double[] Zeta = null;
        double[] ks = null;
        double[] Uy = null;
        double[] Ux = null;
        double[] sx = null;

        double[] t = null;
        double[] wl = null;
        double[] qr = null;
        /// <summary>
        /// Сдвиг дна на берегах
        /// </summary>
        double delta = 0.1;
        double ks0 = 0.1;
        FVCurves formCL = new FVCurves();
        FVCurves formTM = new FVCurves();
        /// <summary>
        /// Линия створа основания
        /// </summary>
        HLine crossLine;
        /// <summary>
        /// титул формата CRF
        /// </summary>
        CRF_Title crfTitle = new CRF_Title();
        RSCrossParams p = new RSCrossParams();
        //BedLoadParams1YD bp = new BedLoadParams1YD();
        public FCrossSection(IHLine crossLine, double[] s, double[] Depth, double[] Vn, double[] Vt, string Data)
        {
            this.crossLine = new HLine((HPoint)crossLine.A, (HPoint)crossLine.B);
            InitializeComponent();
            Text = "Создание задачи в створе русла";
            string filter = "(*" + Ext_Crf + ")|*" + Ext_Crf + "| ";
            filter += "(*" + Ext_RvY + ")|*" + Ext_RvY + "| ";
            filter += " All files (*.*)|*.*";
            saveFileDialog1.Filter = filter;
            crfTitle.Set(s, Depth, Vn, Vt);
            SetFCrossSection(s, Depth, Vn, Vt, Data, Data);
            tbMessage.Text = "Создание нового створа";
        }
        public void SetFCrossSection(double[] s, double[] Depth, double[] Vn, double[] Vt, string Data, string DataEnd)
        {
            var names = SMGManager.GetNames();
            cbMeshGenerator.Items.AddRange(names.ToArray());
            cbMeshGenerator.SelectedIndex = 2;
            this.Data = Data;
            tb_Data.Text = Data;
            tb_DataEnd.Text = DataEnd;
            SetData(s, Depth, Vn, Vt);
        }

        public FCrossSection(string FileName)
        {
            Text = "Редактирование задачи в створе русла";
            InitializeComponent();
            LoadCRF(FileName);
        }
        private void btLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            string filter = "(*" + Ext_Crf + ")|*" + Ext_Crf + "| ";
            filter += " All files (*.*)|*.*";
            ofd.Filter = filter;
            if (ofd.ShowDialog() == DialogResult.OK)
                LoadCRF(ofd.FileName);
            tbMessage.Text = "Загружен стврор из файла :" + Path.GetFileName(ofd.FileName);
        }
        public void LoadCRF(string FileName)
        {
            try
            {
                using (StreamReader file = new StreamReader(FileName))
                {
                    p.Load(file);
                    pGridTask.SelectedObject = p;
                    pGridTask.Update();
                    csData = new CrossSectionData();
                    csData.Load(file);
                    #region ++ 02 09 2025
                    if (csData.TanPhi.Count == 2) cbTF.Checked = false; else cbTF.Checked = true;
                    tb_tf.Text = (180 * Math.Atan(csData.TanPhi.Ymax) / Math.PI).ToString("F2");
                    if (csData.Tau_Adhesion.Count == 2) cbTau_s.Checked = false; else cbTau_s.Checked = true;
                    tb_tau_s.Text = csData.Tau_Adhesion.Ymax.ToString("F4");
                    #endregion
                    #region Расширение формата
                    crfTitle = new CRF_Title();
                    crfTitle.Load(file);
                    string lineA = file.ReadLine();
                    string lineB = file.ReadLine();
                    crossLine = new HLine(HPoint.Parse(lineA), HPoint.Parse(lineB));
                    SetFCrossSection(crfTitle.s, crfTitle.Depth, crfTitle.Vn, crfTitle.Vt, p.DataStart, p.DataEnd);// crfTitle.start.ToString(), crfTitle.end.ToString());
                    SetLocRiver(crfTitle.idx);
                    lbKs.Items.Clear();
                    for (int ii = 0; ii < crfTitle.values.Count; ii++)
                    {
                        var (s0, s1, ks) = crfTitle.values[ii];
                        string line = s0.ToString("F4") + " " + s1.ToString("F4") + " " + ks.ToString("F6");
                        lbKs.Items.Add(line);
                        SetRoughness();
                    }
                    tb_Data.Text = p.DataStart;
                    tb_DataEnd.Text = p.DataEnd;
                    ChangeData();
                    #endregion
                    file.Close();
                    cbMeshGenerator.SelectedIndex = (int)p.typeMeshGenerator;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Info(ex.Message);
            }
        }

        private void bt_CreateTaskFile_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.Filter = "файл задачи  (*.crf)|*.crf| Все файлы (*.*)|*.*";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    using (StreamWriter file = new StreamWriter(saveFileDialog1.FileName))
                    {
                        ChangeData();
                        UpDate();
                        string FileEXT = Path.GetExtension(saveFileDialog1.FileName).ToLower();
                        switch (FileEXT)
                        {
                            case Ext_RvY:
                                file.WriteLine(Data);
                                file.WriteLine(NameGP);
                                csData.Save(file);
                                //for (int i = 0; i < crossFunctions.Length; i++)
                                //    crossFunctions[i].Save(file);
                                break;
                            case Ext_Crf:
                                // параметры задачи
                                p = (RSCrossParams)pGridTask.SelectedObject;
                                if (cbRetask.Checked == false)
                                    p.ReTask = 3;
                                p.DataStart = tb_Data.Text;
                                p.DataEnd = tb_DataEnd.Text;
                                p.typeMeshGenerator = (StripGenMeshType)cbMeshGenerator.SelectedIndex;
                                p.SaveA(file);
                                // функции задачи
                                csData.Save(file);
                                // титул задачи
                                int idx = LocRiver();
                                crfTitle.SetTitle(placeID, idx);
                                crfTitle.Clrar();
                                for (int ii = 0; ii < lbKs.Items.Count; ii++)
                                {
                                    string line = lbKs.Items[ii].ToString();
                                    string[] lines = line.Split(' ');
                                    double s0 = double.Parse(lines[0]);
                                    double s1 = double.Parse(lines[1]);
                                    double ks = double.Parse(lines[2]);
                                    crfTitle.AddKs(s0, s1, ks);
                                }
                                crfTitle.Save(file);
                                // координаты створа
                                file.WriteLine(((HPoint)crossLine.A).ToString());
                                file.WriteLine(((HPoint)crossLine.B).ToString());

                                break;
                        }
                        file.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Info(ex.Message);
            }
        }


        private void SetData(double[] s, double[] Depth, double[] Vn, double[] Vt)
        {
            // Расширение на 2 узла для интерполяции берегов 
            MEM.Alloc(Depth.Length + 4, ref Zeta);
            MEM.Alloc(Zeta.Length, ref ks);
            MEM.Alloc(Zeta.Length, ref Ux);
            MEM.Alloc(Zeta.Length, ref Uy);
            MEM.Alloc(Zeta.Length, ref sx);
            ChangGP_WL();
            //    геометрия дна
            //     A ---                                              floodplain + 1  возвышение над поймой
            //          ----
            //               ---- B  ------------------------------   floodplain       пойма   
            //                        \
            //                            \
            //                                 C ---------------------- WL 
            //                                    \
            //                                       \
            //                                          \        
            //                                            \ ----------  дно
            //     |---- Ls -----|---- L1 ----|                                 
            // Zeta[0]        Zeta[1]       Zeta[2] ...                            
            double xMax = s.Max();
            double xMin = s.Min();
            double Ls = 0.1 * (xMax - xMin);
            // текущий уровень WL по гидропосту
            double waterLevel = Zerro + 0.01 * WLRiver;
            // возвышение над уровнем поймы
            double H1 = floodplain - waterLevel;
            // ширина участка между отметками от WL до  floodplain (уклон 30 градусов) + 1 метр
            double L1 = 2 * H1 + 1; 
            //double L1 = H1;
            // если вода вышла на пойму
            if (L1 > Ls) Ls = 2 * L1;

            //Zeta[0] = floodplain + 5;
            //Zeta[1] = floodplain;
            //Zeta[Zeta.Length - 2] = floodplain;
            //Zeta[Zeta.Length - 1] = floodplain + 5;
            Zeta[0] = floodplain;
            Zeta[1] = floodplain;
            Zeta[Zeta.Length - 2] = floodplain;
            Zeta[Zeta.Length - 1] = floodplain;

            double[] wx = new double[2];
            if (s[0] < xMax)
            {
                sx[0] = xMin - Ls;
                sx[1] = xMin - L1;

                sx[sx.Length - 2] = xMax + L1;
                sx[sx.Length - 1] = xMax + Ls;
                wx[0] = sx[0];
                wx[1] = sx[sx.Length - 1];

            }
            else
            {
                sx[0] = xMax + Ls;
                sx[1] = xMax + L1;

                sx[sx.Length - 2] = xMin - L1;
                sx[sx.Length - 1] = xMin - Ls;
                wx[0] = sx[0];
                wx[1] = sx[sx.Length - 1];

            }
            double[] wH = { waterLevel, waterLevel };
            CSWaterLevels = new DigFunction(wx, wH, "Свободная поверхность");


            tbS0.Text = sx[0].ToString("F4");
            tbSL.Text = sx[sx.Length - 1].ToString("F4");
            tbks.Text = ks0.ToString("F4");
            string line = tbS0.Text + " " + tbSL.Text + " " + tbks.Text;
            lbKs.Items.Add(line);

            for (int i = 0; i < Depth.Length; i++)
            {
                sx[i + 2] = s[i];
                // вычислени отметок дна по глубинам,
                // отметоки гидпопоста и текуше глубины по гидропосту
                Zeta[i + 2] = waterLevel - Depth[i];
                // перевод размерности из км/час в м/с
                Ux[i + 2] = Vn[i] / 3.6;
                Uy[i + 2] = Vt[i] / 3.6;
            }

            for (int i = 0; i < ks.Length; i++)
                ks[i] = ks0;
            csData.Geometry = new DigFunction(sx, Zeta, "Геометрия створа");
            csData.Roughness = new DigFunction(sx, ks, "Шероховатость дна");
            // скорости на сободной поверхности потока
            csData.VelocityX = new DigFunction(sx, Ux, "Нормальные скорости на сободной поверхности потока");
            csData.VelocityY = new DigFunction(sx, Uy, "Радиальные скорости на сободной поверхности потока");
            // скорости на сободной поверхности потока
            UpDate();
            //if (cbTF.Checked == true)
            //{
            //    double tmpGr = double.Parse(tb_tf.Text, MEM.formatter);
            //    double tmpTf = Math.Tan(Math.PI * tmpGr / 180);
            //    double[] xx = { sx[0], sx[1], sx[2], sx[sx.Length - 3], sx[sx.Length - 2], sx[sx.Length - 1] };
            //    double[] tf = { tmpTf, tmpTf, SPhysics.PHYS.tanphi, SPhysics.PHYS.tanphi, tmpTf, tmpTf };
            //    csData.TanPhi = new DigFunction(xx, tf, "Угол естественного откоса");
            //}
            //else
            //{
            //    double[] xx = { sx[0], sx[sx.Length - 1] };
            //    double[] tf = { SPhysics.PHYS.tanphi, SPhysics.PHYS.tanphi };
            //    csData.TanPhi = new DigFunction(xx, tf, "Угол естественного откоса");
            //}
            //if (cbTau_s.Checked == true)
            //{
            //    double tmp = double.Parse(tb_tau_s.Text, MEM.formatter);
            //    double[] xx = { sx[0], sx[1], sx[2], sx[sx.Length - 3], sx[sx.Length - 2], sx[sx.Length - 1] };
            //    double[] cc = { tmp, tmp, 0, 0, tmp, tmp };
            //    csData.Tau_s = new DigFunction(xx, cc, "Напряжние удельного сцепления");
            //}
            //else
            //{
            //    double[] xx = { sx[0], sx[sx.Length - 1] };
            //    double[] cc = { 0, 0 };
            //    csData.Tau_s = new DigFunction(xx, cc, "Напряжние удельного сцепления");
            //}
            //crossFunctions = new IDigFunction[8]
            //{ Geometry, WaterLevels, FlowRate, VelocityX, VelocityY, Roughness, TanPhi, Tau_s };
            p.DataStart = tb_Data.Text;
            p.DataEnd = tb_DataEnd.Text;
            pGridTask.SelectedObject = p;
            int splitterPositionCP = pGridTask.GetInternalLabelWidth();
        }
        public void UpDate()
        {
            // скорости на сободной поверхности потока
            if (cbTF.Checked == true)
            {
                double tmpGr = double.Parse(tb_tf.Text, MEM.formatter);
                double tmpTf = Math.Tan(Math.PI * tmpGr / 180);
                double[] xx = { sx[0], sx[1], sx[2], sx[sx.Length - 3], sx[sx.Length - 2], sx[sx.Length - 1] };
                double[] tf = { tmpTf, tmpTf, SPhysics.PHYS.tanphi, SPhysics.PHYS.tanphi, tmpTf, tmpTf };
                csData.TanPhi = new DigFunction(xx, tf, "Угол естественного откоса");
            }
            else
            {
                double[] xx = { sx[0], sx[sx.Length - 1] };
                double[] tf = { SPhysics.PHYS.tanphi, SPhysics.PHYS.tanphi };
                csData.TanPhi = new DigFunction(xx, tf, "Угол естественного откоса");
            }
            if (cbTau_s.Checked == true)
            {
                double tmp = double.Parse(tb_tau_s.Text, MEM.formatter);
                double[] xx = { sx[0], sx[1], sx[2], sx[sx.Length - 3], sx[sx.Length - 2], sx[sx.Length - 1] };
                double[] cc = { tmp, tmp, 0, 0, tmp, tmp };
                csData.Tau_Adhesion = new DigFunction(xx, cc, "Напряжние удельного сцепления");
            }
            else
            {
                double[] xx = { sx[0], sx[sx.Length - 1] };
                double[] cc = { 0, 0 };
                csData.Tau_Adhesion = new DigFunction(xx, cc, "Напряжние удельного сцепления");
            }
            csData.UpDate();
        }


        private void SetplaceID(int placeID)
        {
            ConnectDB.PlaceInfo(placeID, ref NameGP, ref X, ref Y, ref Zerro, ref floodplain);
            tb2.Text = placeID.ToString();
            tb21.Text = NameGP;
            tb_latitudeArea.Text = X;
            tb_longitude.Text = Y;
            tb_BaltikLvl.Text = Zerro.ToString("F4");
        }

        /// <summary>
        /// Определение времени расчета, уровня свободной поверхности, и расхода
        /// </summary>
        public void ChangGP_WL()
        {
            SetplaceID(placeID);
            // WLRiver хранится в БД в сантиметрах
            WLRiver = ConnectDB.WaterLevelData(Data, placeID);
            QRiver = riverFlow(WLRiver);
            tb_RF.Text = QRiver.ToString("F1");
            WL = Zerro + 0.01 * WLRiver;
            tb_RiverLvl.Text = WL.ToString("F2");
            // если дата начала и конца расчетного периода совподают
            // ставим текущий уровень свободной поверхности, и расхода
            if (tb_DataEnd.Text.Trim() == tb_Data.Text.Trim())
            {
                // Текуший период с получением уровня и расхода
                t = new double[2] { 0, 100000 };
                wl = new double[2] { WL, WL };
                qr = new double[2] { QRiver, QRiver };
            }
            else
            {
                string DataEnd = "";
                // Запрос за период с получением уровней и расходов
                try
                {
                    DataEnd = tb_DataEnd.Text;
                    ConnectDB.WaterLevelsData(Zerro, tb_Data.Text, tb_DataEnd.Text, ref t, ref wl, placeID);
                    MEM.Alloc(wl.Length, ref qr);
                    for (int i = 0; i < wl.Length; i++)
                        qr[i] = riverFlow(wl[i]);
                }
                catch
                {
                    // Текуший период с получением уровня и расхода
                    t = new double[2] { 0, 100000 };
                    wl = new double[2] { WL, WL };
                    qr = new double[2] { QRiver, QRiver };
                    Console.WriteLine("Формат даты " + DataEnd + "не верен!");
                }
            }
            // свободная поверхность
            csData.WaterLevels = new DigFunction(t, wl, "Эволюция свободной поверхности");
            // расход потока
            csData.FlowRate = new DigFunction(t, qr, "Эволюция расхода в створе");
            csData.UpDate();
        }
        private double riverFlow(double H)
        {
            IRiverFlow rf = new RiverFlowZerro();
            if (tb2.Text.Trim() == "1")
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
            if (tb2.Text.Trim() == "3")
                rf = new RiverFlowKomsomolskNaAmure();
            return rf.GetRiverFlow(H);
        }

        private void btClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ISavePoint sp = new SavePoint("Створ");
            sp.AddCurve(csData.Geometry);
            sp.AddCurve(csData.VelocityX);
            sp.AddCurve(csData.VelocityY);
            sp.AddCurve(csData.Roughness);
            if (CSWaterLevels != null)
                sp.AddCurve(CSWaterLevels);

            if (formCL.CloseDO == true)
            {
                formCL = new FVCurves(sp);
                formCL.Show();
            }
            else
            {
                try
                {
                    formCL.SetSavePoint(sp);
                    formCL.Show();
                }
                catch
                {
                    formCL = new FVCurves(sp);
                    formCL.Show();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ISavePoint sp = new SavePoint("Створ");
            if (csData.WaterLevels.Count > 2)
            {
                string name = "";
                csData.WaterLevels.GetFunctionData(ref name, ref t, ref wl);
                csData.FlowRate.GetFunctionData(ref name, ref t, ref qr);
                for (int i = 0; i < t.Length; i++)
                {
                    wl[i] = wl[i] * 100;
                    t[i] = t[i] / 86400;
                }
                sp.AddCurve(new DigFunction(t, wl, "Эволюция свободной поверхности"));
                sp.AddCurve(new DigFunction(t, qr, "Эволюция расхода в створе"));
            }
            else
            {
                sp.AddCurve(csData.WaterLevels);
                sp.AddCurve(csData.FlowRate);
            }
            formTM = new FVCurves(sp);
            formTM.Show();
        }

        private void btAdd_Click(object sender, EventArgs e)
        {
            string line = tbS0.Text + " " + tbSL.Text + " " + tbks.Text;
            if (lbKs.Items.Contains(line) == false)
            {
                lbKs.Items.Add(line);
                SetRoughness();
            }
        }

        private void btEdit_Click(object sender, EventArgs e)
        {
            string line;
            if (lbKs.SelectedIndex == 0)
                line = sx[0].ToString("F4") + " " + sx[sx.Length - 1].ToString("F4") + " " + tbks.Text;
            else
                line = tbS0.Text + " " + tbSL.Text + " " + tbks.Text;
            if (lbKs.Items.Contains(line) == false)
            {
                lbKs.Items[lbKs.SelectedIndex] = line;
                SetRoughness();
            }
        }

        private void btDel_Click(object sender, EventArgs e)
        {
            if (lbKs.SelectedIndex != -1)
            {
                object obj = lbKs.Items[lbKs.SelectedIndex];
                lbKs.Items.Remove(obj);
                if (lbKs.Items.Count == 0)
                {
                    tbS0.Text = sx[0].ToString("F4");
                    tbSL.Text = sx[sx.Length - 1].ToString("F4");
                    tbks.Text = ks0.ToString("F4");
                    string line = tbS0.Text + " " + tbSL.Text + " " + tbks.Text;
                    lbKs.Items.Add(line);

                }
                SetRoughness();
            }
        }
        private void SetRoughness()
        {
            for (int i = 0; i < lbKs.Items.Count; i++)
            {
                string line = lbKs.Items[i].ToString();
                string[] lines = line.Split(' ');
                double x0 = double.Parse(lines[0]);
                double xL = double.Parse(lines[1]);
                double kss = double.Parse(lines[2]);
                for (int j = 0; j < sx.Length; j++)
                {
                    if (sx[j] >= x0 && sx[j] <= xL)
                        ks[j] = kss;
                }
            }
            csData.Roughness = new DigFunction(sx, ks, "Шероховатость дна");
            csData.UpDate();
        }

        private void lbKs_DoubleClick(object sender, EventArgs e)
        {
            string line = lbKs.Items[lbKs.SelectedIndex].ToString();
            string[] lines = line.Split(' ');
            tbS0.Text = lines[0];
            tbSL.Text = lines[1];
            tbks.Text = lines[2];
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form form = new HelpForm(0);
            form.Show();
        }

        private void bt1_Click(object sender, EventArgs e)
        {
            book.ShowDialog();
            tb2.Text = book.RowID;
            tb21.Text = book.RowName;
            placeID = int.Parse(book.RowID.Trim());
            ChangGP_WL();
            if (tb2.Text.Trim() == "1")
                pnRF.Visible = true;
            else
                pnRF.Visible = false;
        }

        private void btCSMesh_Click(object sender, EventArgs e)
        {
            try
            {
                /// координаты дна по оси х
                double[] bottom_x = null;
                /// координаты дна по оси у
                double[] bottom_y = null;
                /// <summary>
                /// Длина смоченного периметра
                /// </summary>
                double WetBed = 0;
                p = (RSCrossParams)pGridTask.SelectedObject;
                // получение отметок дна
                csData.Geometry.GetFunctionData(ref bottom_x, ref bottom_y, p.CountBLKnots);
                // начальный уровень свободной поверхности
                var waterLevel = csData.WaterLevels.FunctionValue(0);
                // начальный расход потока
                var riverFlowRate = csData.FlowRate.FunctionValue(0);

                bool axisOfSymmetry = p.axisSymmetry == 1 ? true : false;
                CrossStripMeshOption op = new CrossStripMeshOption();
                op.AxisOfSymmetry = axisOfSymmetry;
                var meshGenerator = SMGManager.GetMeshGenerator(p.typeMeshGenerator, op);
                /// сечения створа
                int[][] riverGates = null;
                IMesh mesh = meshGenerator.CreateMesh(ref WetBed, ref riverGates, waterLevel, bottom_x, bottom_y);
                ShowMesh(mesh, csData.crossFunctions);
            }
            catch (Exception ex)
            {
                Logger.Instance.Info(ex.Message);
            }
        }
        /// <summary>
        /// Отобразить сетку в отдельном окне
        /// </summary>
        /// <param name="meshRiver"></param>
        public void ShowMesh(IMesh mesh, IDigFunction[] curves = null)
        {
            if (mesh != null)
            {
                SavePoint data = new SavePoint("Триангуляция в контуре по точкам наблюдения");
                data.SetSavePoint(0, mesh);
                double[] x = mesh.GetCoords(0);
                double[] y = mesh.GetCoords(1);
                data.Add("Координата Х", x);
                data.Add("Координата Y", y);
                if (curves != null)
                {
                    for (int i = 0; i < curves.Length; i++)
                        if (curves[i] != null)
                            data.AddCurve(curves[i]);
                }
                Form form = new ViForm(data);
                form.Show();
            }
        }

        int LocRiver()
        {
            if (rbAmur.Checked == true)
                return 0;
            else
                if (rbPemza.Checked == true)
                return 1;
            else
                return 2;
        }

        private void btChangeData_Click(object sender, EventArgs e)
        {
            ChangeData();
        }
        private void ChangeData()
        {
            if (tb_DataEnd.Text.Trim() != tb_Data.Text.Trim())
            {
                ChangGP_WL();
                csData.UpDate();
            }
        }

        void SetLocRiver(int idx)
        {
            if (idx == 0) rbAmur.Checked = true;
            if (idx == 1) rbPemza.Checked = true;
            if (idx == 2) rbMad.Checked = true;
        }
        private void rbAmur_CheckedChanged(object sender, EventArgs e)
        {
            ChangGP_WL();
            csData.UpDate();
        }
    }
}
