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
        /// Расход
        /// </summary>
        double QRiver = 0;
        string NameGP = "", X = "", Y = "";
        double Zerro = 0, height = 0;
        const string Ext_RvY = ".rvy";
        const string Ext_Crf = ".crf";
        public FCrossSection(double[] s, double[] Deapth, 
                             double[] Vx, double[] Vy,
                             double[] Vn, double[] Vt, string Data)
        {
            InitializeComponent();
            this.Data = Data;
            tb_Data.Text = Data;
            tb_DataEnd.Text = Data;
            string filter = "(*" + Ext_RvY + ")|*" + Ext_RvY + "| ";
                   filter += "(*" + Ext_Crf + ")|*" + Ext_Crf + "| ";
            filter += " All files (*.*)|*.*";
            saveFileDialog1.Filter = filter;
            ChangGP_WL();
            // геометрия дна
            Geometry = new DigFunction(s, Deapth, "Геометрия створа");
            // скорости на сободной поверхности потока
            VelocityX = new DigFunction(s, Vn, "Нормальные скорости на сободной поверхности потока");
            VelocityY = new DigFunction(s, Vt, "Радиальные скорости на сободной поверхности потока");
            crossFunctions = new IDigFunction[5] { Geometry, WaterLevels, FlowRate, VelocityX, VelocityY };
        }
        public void ChangGP_WL()
        {
            ConnectDB.PlaceInfo(placeID, ref NameGP, ref X, ref Y, ref Zerro, ref height);
            tb2.Text = placeID.ToString();
            tb21.Text = NameGP;
            tb_latitudeArea.Text = X;
            tb_longitude.Text = Y;
            tb_BaltikLvl.Text = Zerro.ToString("F4");
            WLRiver = ConnectDB.WaterLevelData(Data, placeID);
            QRiver = riverFlow(WLRiver);
            tb_RF.Text = QRiver.ToString("F1");
            tb_RiverLvl.Text = (Zerro + WLRiver / 100).ToString("F2");
            // Заглушка сделать запрос за период если он есть с получением уровней и расходов
            double[] t = { 0, 100000 };
            double[] wl = { WLRiver, WLRiver };
            double[] qr = { QRiver, QRiver };
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
                                for (int i = 0; i < 3; i++)
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
