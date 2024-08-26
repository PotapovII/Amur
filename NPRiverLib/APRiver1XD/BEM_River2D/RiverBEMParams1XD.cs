//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 26.04.2021 Потапов И.И.
//---------------------------------------------------------------------------
//               Реверс в NPRiverLib 24.07.2024 Потапов И.И.
//---------------------------------------------------------------------------

namespace NPRiverLib.APRiver1XD.BEM_River2D
{
    using MemLogLib;
    using CommonLib;
    
    using System;
    using System.IO;
    using System.ComponentModel;
    /// <summary>
    ///  ОО: Параметры для класса RiverStreamTask 
    /// </summary>
    [Serializable]
    public class RiverBEMParams1XD : ITProperty<RiverBEMParams1XD>
    {
        public RiverBEMParams1XD Clone(RiverBEMParams1XD p)
        {
            return new RiverBEMParams1XD(p);
        }
        /// <summary>
        /// Ширина области
        /// </summary>
        [DisplayName("Выделение под области")]
        [Category("Расчетная область")]
        public bool SelectAreaMesh { get; set; }
        /// <summary>
        /// Ширина области
        /// </summary>
        [DisplayName("Ширина окна области м")]
        [Category("Расчетная область")]
        public double LPhi { get; set; }
        /// <summary>
        /// Ширина области
        /// </summary>
        [DisplayName("Высота окна области м")]
        [Category("Расчетная область")]
        public double HPhi { get; set; }
        /// <summary>
        /// Ширина области
        /// </summary>
        [DisplayName("Ширина области м")]
        [Category("Расчетная область")]
        public double LW { get; set; }
        /// <summary>
        /// Высота области
        /// </summary>
        [DisplayName("Высота области м")]
        [Category("Расчетная область")]
        public double HW { get; set; }
        /// <summary>
        /// Координата х для цилиндра
        /// </summary>
        [DisplayName("Координата х для цилиндра м")]
        [Category("Расчетная область")]
        public double XC { get; set; }
        /// <summary>
        /// Координата y для цилиндра
        /// </summary>
        [DisplayName("Координата y для цилиндра м")]
        [Category("Расчетная область")]
        public double YC { get; set; }
        /// <summary>
        /// Радиус цилиндра
        /// </summary>
        [DisplayName("Радиус цилиндра м")]
        [Category("Расчетная область")]
        public double RC { get; set; }
        /// <summary>
        /// Эллипс - след 
        /// </summary>
        [DisplayName("Эллипс - след")]
        [Category("Расчетная область")]
        public double EllipsRC { get; set; }
        /// <summary>
        /// Скорость набегания
        /// </summary>
        [DisplayName("Скорость набегания м/с")]
        [Category("Физика")]
        public double U_inf { get; set; }
        /// <summary>
        /// Узлов по дну
        /// </summary>
        [DisplayName("Узлов по дну")]
        [Category("Алгоритм")]
        public int NW { get; set; }
        /// <summary>
        /// Узлов по цилиндру
        /// </summary>
        [DisplayName("Узлов по цилиндру")]
        [Category("Алгоритм")]
        public int NB { get; set; }
        /// <summary>
        /// Расчет циркуляции потока
        /// </summary>
        [DisplayName("Расчет циркуляции потока")]
        [Category("Расчет циркуляции потока")]
        public bool flagGamma { get; set; }
        /// <summary>
        /// Степень сгущения сетки на цилиндре
        /// </summary>
        [DisplayName("Параметр циркуляции")]
        [Category("Расчет циркуляции потока")]
        public double AlphaGamma { get; set; }
        ///// <summary>
        ///// Коэффциент гидравлического сопротивления
        ///// </summary>
        //[DisplayName("Коэффциент гидравлического сопротивления")]
        //[Category("Физика")]
        //public double Lambda { get; set; }
        /// <summary>
        /// Параметр сглаживания функции скорости
        /// </summary>
        [DisplayName("Параметр сглаживания")]
        [Category("Физика")]
        public int Flexibility { get; set; }
        /// <summary>
        /// Параметр регуляризации функции скорости
        /// </summary>
        [DisplayName("Параметр регуляризации")]
        [Category("Физика")]
        public int Hardness { get; set; }
        /// <summary>
        /// Параметр регуляризации функции скорости
        /// </summary>
        [DisplayName("Сглаживания скорости")]
        [Category("Физика")]
        public bool FlagFlexibility { get; set; }

        /// <summary>
        /// Флаг типа сетки
        /// true - равномерая
        /// false 0 - сгущение на цилиндр
        /// </summary>
        [DisplayName("Флаг типа сетки")]
        [Category("Расчетная область")]
        public bool MeshType { get; set; }
        /// <summary>
        /// Степень сгущения сетки на дне
        /// </summary>
        [DisplayName("Степень сгущения сетки на дне")]
        [Category("Расчетная область")]
        public double AmpBottom { get; set; }
        /// <summary>
        /// Степень сгущения сетки на цилиндре
        /// </summary>
        [DisplayName("Степень сгущения сетки на цилиндре")]
        [Category("Расчетная область")]
        public double AmpCircle { get; set; }
        /// <summary>
        /// Степень сгущения сетки на цилиндре
        /// </summary>
        [DisplayName("Растояния в долях диаметра между дном и цилиндром")]
        [Category("Расчетная область")]
        public double AlphaH { get; set; }
        /// <summary>
        /// Угол разворота хвоста
        /// </summary>
        [DisplayName("Угол разворота хвоста")]
        [Category("Угол разворота хвоста")]
        public double Alpha { get; set; }

        /// <summary>
        /// Дисперсия начальной коверны
        /// </summary>
        [DisplayName("Дисперсия начальной коверны")]
        [Category("Расчетная область")]
        public double AlphaCowern { get; set; }
        /// <summary>
        /// Амплитуда начальной коверны
        /// </summary>
        [DisplayName("Угол разворота хвоста")]
        [Category("Расчетная область")]
        public double AMPCowern { get; set; }
        public RiverBEMParams1XD()
        {
            this.Alpha = 5;         // наклон хвоста
            this.AlphaH = 0.05;
            this.RC = 0.05;       // радиус цилиндра
            this.U_inf = 0.6;      // скорость набегания потока
            this.flagGamma = false; // Расчет циркуляции потока
            this.AlphaGamma = 15;
            this.FlagFlexibility = true;
            this.Hardness = 1;
            this.Flexibility = 700;
            this.MeshType = false;
            this.AmpBottom = 0.9;
            this.AmpCircle = -0.7;
            this.NW = 128;// 256;
            this.NB = 64;// 128;

            CalkParams();
        }
        protected void CalkParams()
        {
            this.EllipsRC = 1.5 * RC;
            double D = RC * 2;   // диаметр цилиндра
            this.LW = 30 * D;     // длина размываемой области
            this.HW = 4 * D;     // высота канала (не используется)
            this.XC = LW / 2.0;    // координата х для цилиндра
            this.YC = RC + AlphaH * D; // координата y для цилиндра
            this.AlphaCowern = 0.05 * D;
            this.AMPCowern = 5 / D;
            this.LPhi = 3 * D + 2 * EllipsRC;
            this.HPhi = 1.5 * D;
        }
        public RiverBEMParams1XD(RiverBEMParams1XD p)
        {
            SetParams(p);
        }
        public void SetParams(RiverBEMParams1XD p)
        {
            this.LW = p.LW;
            this.HW = p.HW;
            this.XC = p.XC;
            this.YC = p.YC;
            this.RC = p.RC;
            this.U_inf = p.U_inf;
            this.NW = p.NW;
            this.NB = p.NB;
            this.Hardness = p.Hardness;
            this.Flexibility = p.Flexibility;
            this.FlagFlexibility = p.FlagFlexibility;
            this.MeshType = p.MeshType;
            this.AmpBottom = p.AmpBottom;
            this.AmpCircle = p.AmpCircle;
            this.AlphaH = p.AlphaH;
            this.EllipsRC = p.EllipsRC;
            this.Alpha = p.Alpha;
            this.flagGamma = p.flagGamma;
            this.AlphaGamma = p.AlphaGamma;
            this.LPhi = p.LPhi;
            this.HPhi = p.HPhi;
            this.SelectAreaMesh = p.SelectAreaMesh;
            this.AlphaCowern = p.AlphaCowern;
            this.AMPCowern = p.AMPCowern;
        }

        /// <summary>
        /// Установка свойств задачи
        /// </summary>
        /// <param name="p"></param>
        public void SetParams(object p)
        {
            RiverBEMParams1XD pp = p as RiverBEMParams1XD;
            SetParams(pp);
        }
        
        public object GetParams()
        {
            return this;
        }
        public void LoadParams(string fileName = "")
        {
            string message = "Файл парамеров задачи - доные деформации - не обнаружен";
            WR.LoadParams(Load, message, fileName);
        }
        /// <summary>
        /// Чтение параметров задачи из файла
        /// </summary>
        /// <param name="file"></param>
        public void Load(StreamReader file)
        {
            this.NW = LOG.GetInt(file.ReadLine());
            this.NB = LOG.GetInt(file.ReadLine());
            this.AmpBottom = LOG.GetDouble(file.ReadLine());
            this.AmpCircle = LOG.GetDouble(file.ReadLine());
            this.Alpha = LOG.GetDouble(file.ReadLine());  // наклон хвоста
            this.AlphaH = LOG.GetDouble(file.ReadLine());
            this.RC = LOG.GetDouble(file.ReadLine());         // радиус цилиндра
            this.U_inf = LOG.GetDouble(file.ReadLine());       // скорость набегания потока
            
            this.Flexibility = LOG.GetInt(file.ReadLine());
            this.FlagFlexibility = LOG.GetBool(file.ReadLine());
            this.AlphaGamma = LOG.GetDouble(file.ReadLine());
            this.flagGamma = LOG.GetBool(file.ReadLine()); // Расчет циркуляции потока

            this.Hardness = LOG.GetInt(file.ReadLine());
            this.MeshType = LOG.GetBool(file.ReadLine());
            
            CalkParams();
        }
    }
}
