//---------------------------------------------------------------------------
//                   ПРОЕКТ  "Русловые процессы"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 01.02.2025 Потапов И.И.
//---------------------------------------------------------------------------
namespace RiverDB.ConvertorOut
{
    using MemLogLib;
    using ConnectLib;
    using GeometryLib;
    using CommonLib.Function;

    using RiverDB.Report;
    using RiverDB.FormsDB;

    using System;
    using System.IO;
    using System.Windows.Forms;
    using RenderLib;
    using MeshLib;
    using CommonLib;
    using System.Threading.Tasks;
    using NPRiverLib.APRiver1YD.Params;
    using System.Reflection;
    using RenderLib.PDG;
    using System.Linq;
    using MeshGeneratorsLib.StripGenerator;
    using HelpLib;

    public partial class FCrossSection : Form
    {
        /// <summary>
        ///  начальная геометрия русла
        /// </summary>
        protected IDigFunction Geometry;
        /// <summary>
        /// уровни(нь) свободной поверхности потока во времени
        /// </summary>
        protected IDigFunction WaterLevels;
        /// <summary>
        /// расход потока во времени
        /// </summary>
        protected IDigFunction FlowRate;
        /// <summary>
        /// скорости на сободной поверхности потока
        /// </summary>
        protected IDigFunction VelocityX;
        /// <summary>
        /// радиальные скорости на сободной поверхности потока
        /// </summary>
        protected IDigFunction VelocityY;
        /// <summary>
        /// шероховатость дна
        /// </summary>
        protected IDigFunction Roughness;
        /// <summary>
        /// Данные о створе
        /// </summary>
        public IDigFunction[] crossFunctions = null;
        /// <summary>
        /// 
        /// </summary>
        Catalogue book = new Catalogue("place", true);
        string Data;
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
        /// <summary>
        /// Сдвиг дна на берегах
        /// </summary>
        double delta = 0.1;
        double ks0 = 0.1;
        FVCurves formCL = new FVCurves();
        FVCurves formTM = new FVCurves();
        public FCrossSection(double[] s, double[] Depth,
                             double[] Vx, double[] Vy,
                             double[] Vn, double[] Vt, string Data)
        {
            InitializeComponent();
            var names = SMGManager.GetNames();
            cbMeshGenerator.Items.AddRange(names.ToArray());
            cbMeshGenerator.SelectedIndex = 2;
            this.Data = Data;
            tb_Data.Text = Data;
            tb_DataEnd.Text = Data;
            string filter = "(*" + Ext_Crf + ")|*" + Ext_Crf + "| ";
            filter += "(*" + Ext_RvY + ")|*" + Ext_RvY + "| ";
            filter += " All files (*.*)|*.*";
            saveFileDialog1.Filter = filter;
            SetData(s, Depth, Vn, Vt);
        }
        private void btLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            string filter = "(*" + Ext_Crf + ")|*" + Ext_Crf + "| ";
            filter += " All files (*.*)|*.*";
            ofd.Filter = filter;
            try
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    using (StreamReader  file = new StreamReader(saveFileDialog1.FileName))
                    {
                        RSCrossParams p = new RSCrossParams();
                        p.Load(file);
                        for (int i = 0; i < crossFunctions.Length; i++)
                            crossFunctions[i].Load(file);
                        tb_Data.Text = p.DataStart;
                        tb_DataEnd.Text = p.DataEnd;
                        if (p.DataStart != p.DataEnd)
                            cbDataEnd.Checked = true;
                        cbMeshGenerator.SelectedIndex = (int)p.typeMeshGenerator;
                        file.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Info(ex.Message);
            }
            //SetData(s, Depth, Vn, Vt);
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
            // геометрия дна
            
            double xMax = s.Max();
            double xMin = s.Min();
            double Ls = 0.1 * (xMax - xMin);
            double mZeta = Zerro + 0.01 * WLRiver;
            double H1 = floodplain - mZeta;
            double L1 = 2 * H1;
            double dFp = 1;
            if (floodplain + 1 - mZeta < 0)
                dFp = mZeta + 1 - floodplain;
            if (L1 > Ls) Ls = 2 *L1;
            Zeta[0] = floodplain + 1;
            Zeta[1] = floodplain;
            Zeta[Zeta.Length - 2] = floodplain;
            Zeta[Zeta.Length - 1] = floodplain + dFp;
            sx[0] = xMin - Ls;
            sx[1] = xMin - L1;
            sx[sx.Length - 2] = xMax + L1;
            sx[sx.Length - 1] = xMax + Ls;
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
                Zeta[i + 2] = mZeta - Depth[i];
                // перевод размерности из км/час в м/с
                Ux[i + 2] = Vn[i] / 3.6;
                Uy[i + 2] = Vt[i] / 3.6;
            }
            for (int i = 0; i < ks.Length; i++)
                ks[i] = ks0;
            Geometry = new DigFunction(sx, Zeta, "Геометрия створа");
            Roughness = new DigFunction(sx, ks, "Шероховатость дна");
            // скорости на сободной поверхности потока
            VelocityX = new DigFunction(sx, Ux, "Нормальные скорости на сободной поверхности потока");
            VelocityY = new DigFunction(sx, Uy, "Радиальные скорости на сободной поверхности потока");
            // скорости на сободной поверхности потока
            crossFunctions = new IDigFunction[6] { Geometry, WaterLevels, FlowRate, VelocityX, VelocityY, Roughness };
            RSCrossParams p = new RSCrossParams();
            p.DataStart = tb_Data.Text;
            p.DataEnd = tb_DataEnd.Text;
            pGridTask.SelectedObject = p;
            int splitterPositionCP = pGridTask.GetInternalLabelWidth();
        }

        public void ChangGP_WL()
        {
            ConnectDB.PlaceInfo(placeID, ref NameGP, ref X, ref Y, ref Zerro, ref floodplain);
            tb2.Text = placeID.ToString();
            tb21.Text = NameGP;
            tb_latitudeArea.Text = X;
            tb_longitude.Text = Y;
            tb_BaltikLvl.Text = Zerro.ToString("F4");
            double[] t = null;
            double[] wl = null;
            double[] qr = null;
            if (tb_DataEnd.Text.Trim() == tb_Data.Text.Trim())
            {
                WLRiver = ConnectDB.WaterLevelData(Data, placeID);
                QRiver = riverFlow(WLRiver);
                tb_RF.Text = QRiver.ToString("F1");
                WL = Zerro + 0.01 * WLRiver;
                tb_RiverLvl.Text = WL.ToString("F2");
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
                    DateTime now = DateTime.Parse(DataEnd);
                    ConnectDB.WaterLevelsData(Zerro, tb_Data.Text, tb_DataEnd.Text, ref t, ref wl, placeID);
                    MEM.Alloc(wl.Length, ref qr);
                    for (int i = 0; i < wl.Length; i++)
                        qr[i] = riverFlow(wl[i]);
                }
                catch 
                {
                    WLRiver = ConnectDB.WaterLevelData(Data, placeID);
                    QRiver = riverFlow(WLRiver);
                    tb_RF.Text = QRiver.ToString("F1");
                    WL = Zerro + 0.01 * WLRiver;
                    tb_RiverLvl.Text = WL.ToString("F2");
                    // Текуший период с получением уровня и расхода
                    t = new double[2] { 0, 100000 };
                    wl = new double[2] { WL, WL };
                    qr = new double[2] { QRiver, QRiver };
                    Console.WriteLine("Формат даты " + DataEnd + "не верен!");
                }

            }
            // свободная поверхность
            WaterLevels = new DigFunction(t, wl, "Эволюция свободной поверхности");
            // расход потока
            FlowRate = new DigFunction(t, qr, "Эволюция расхода в створе");
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

        private void cbDataEnd_CheckedChanged(object sender, EventArgs e)
        {
            tb_DataEnd.Enabled = cbDataEnd.Checked;
            if (cbDataEnd.Checked == false && tb_DataEnd.Text.Trim() != tb_Data.Text.Trim())
            {
                ChangGP_WL();
                crossFunctions = new IDigFunction[6] { Geometry, WaterLevels, FlowRate, VelocityX, VelocityY, Roughness };
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ISavePoint sp = new SavePoint("Створ");
            sp.AddCurve(Geometry);
            sp.AddCurve(VelocityX);
            sp.AddCurve(VelocityY);
            sp.AddCurve(Roughness);
            if (formCL.CloseDO == true)
            {
                formCL = new FVCurves(sp);
                formCL.Show();
            }
            else
            {
                formCL.SetSavePoint(sp);
                formCL.Show();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ISavePoint sp = new SavePoint("Створ");
            sp.AddCurve(WaterLevels);
            sp.AddCurve(FlowRate);
            if (formTM.CloseDO == true)
            {
                formTM = new FVCurves(sp);
                formTM.Show();
            }
            else
            {
                formTM.SetSavePoint(sp);
                formTM.Show();
            }
        }

        private void btAdd_Click(object sender, EventArgs e)
        {
            string line = tbS0.Text + " " + tbSL.Text + " " + tbks.Text;
            if(lbKs.Items.Contains(line) == false)
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
                if(lbKs.Items.Count == 0)
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
            Roughness = new DigFunction(sx, ks, "Шероховатость дна");
            crossFunctions[5] = Roughness;
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



        private void rbAmur_CheckedChanged(object sender, EventArgs e)
        {
            ChangGP_WL();
            crossFunctions = new IDigFunction[6] { Geometry, WaterLevels, FlowRate, VelocityX, VelocityY, Roughness };
        }

        private void bt_CreateTaskFile_Click(object sender, EventArgs e)
        {
            try
            {
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    using (StreamWriter file = new StreamWriter(saveFileDialog1.FileName))
                    {
                        string FileEXT = Path.GetExtension(saveFileDialog1.FileName).ToLower();
                        switch (FileEXT)
                        {
                            case Ext_RvY:
                                file.WriteLine(Data);
                                file.WriteLine(NameGP);
                                for (int i = 0; i < crossFunctions.Length; i++)
                                    crossFunctions[i].Save(file);
                                break;
                            case Ext_Crf:
                                RSCrossParams p = (RSCrossParams)pGridTask.SelectedObject;
                                if (cbRetask.Checked == false)
                                    p.ReTask = 3;
                                p.DataStart = tb_Data.Text;
                                p.DataEnd = tb_DataEnd.Text;
                                p.typeMeshGenerator = (StripGenMeshType)cbMeshGenerator.SelectedIndex;
                                p.SaveA(file);
                                for (int i = 0; i < crossFunctions.Length; i++)
                                    crossFunctions[i].Save(file);
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
    }
}
