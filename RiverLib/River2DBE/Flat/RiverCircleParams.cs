////---------------------------------------------------------------------------
////                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
////                         проектировщик:
////                           Потапов И.И.
////---------------------------------------------------------------------------
////                 кодировка : 26.04.2021 Потапов И.И.
////---------------------------------------------------------------------------
//namespace RiverLib
//{
//    using System;
//    using System.ComponentModel;
//    /// <summary>
//    ///  ОО: Параметры для класса RiverStreamTask 
//    /// </summary>
//    [Serializable]
//    public class RiverCircleParams
//    {
//        /// <summary>
//        /// Ширина области
//        /// </summary>
//        [DisplayName("Ширина области м")]
//        [Category("Расчетная область")]
//        public double LW { get; set; }
//        /// <summary>
//        /// Высота области
//        /// </summary>
//        [DisplayName("Высота области м")]
//        [Category("Расчетная область")]
//        public double HW { get; set; }
//        /// <summary>
//        /// Координата х для цилиндра
//        /// </summary>
//        [DisplayName("Координата х для цилиндра м")]
//        [Category("Расчетная область")]
//        public double XC { get; set; }
//        /// <summary>
//        /// Координата y для цилиндра
//        /// </summary>
//        [DisplayName("Координата y для цилиндра м")]
//        [Category("Расчетная область")]
//        public double YC { get; set; }
//        /// <summary>
//        /// Радиус цилиндра
//        /// </summary>
//        [DisplayName("Радиус цилиндра м")]
//        [Category("Расчетная область")]
//        public double RC { get; set; }
//        /// <summary>
//        /// Скорость набегания
//        /// </summary>
//        [DisplayName("Скорость набегания м/с")]
//        [Category("Физика")]
//        public double U_inf { get; set; }
//        /// <summary>
//        /// Узлов по дну
//        /// </summary>
//        [DisplayName("Узлов по дну")]
//        [Category("Алгоритм")]
//        public int NW { get; set; }
//        /// <summary>
//        /// Узлов по цилиндру
//        /// </summary>
//        [DisplayName("Узлов по цилиндру")]
//        [Category("Алгоритм")]
//        public int NB { get; set; }
//        /// <summary>
//        /// Плотность потока
//        /// </summary>
//        [DisplayName("Плотность потока")]
//        [Category("Физика")]
//        public double Rho { get; set; }
//        /// <summary>
//        /// Коэффциент гидравлического сопротивления
//        /// </summary>
//        [DisplayName("Коэффциент гидравлического сопротивления")]
//        [Category("Физика")]
//        public double Lambda { get; set; }
//        /// <summary>
//        /// Параметр сглаживания функции скорости
//        /// </summary>
//        [DisplayName("Параметр сглаживания")]
//        [Category("Физика")]
//        public int Flexibility { get; set; }
//        /// <summary>
//        /// Параметр регуляризации функции скорости
//        /// </summary>
//        [DisplayName("Параметр регуляризации")]
//        [Category("Физика")]
//        public int Hardness { get; set; }
//        public RiverCircleParams()
//        {
//            this.Rho = 1000;
//            this.Lambda = 0.02;
//            this.RC = 0.05;    // радиус цилиндра
//            double D = RC * 2; // диаметр цилиндра
//            this.LW = 60 * D;    // длина размываемой области
//            this.HW = 4 * D;     // высота канала
//            this.XC = 20 * D;  // координата х для цилиндра
//            this.YC = RC + 0.1 * D; // координата y для цилиндра
//            this.U_inf = 0.5;  // скорость набегания потока
//            // узлы
//            this.NW = 1250;
//            this.NB = 250;
//            // 
//            this.Hardness = 1;
//            this.Flexibility = 700;
//        }
//        public RiverCircleParams(RiverCircleParams p)
//        {
//            this.Rho = p.Rho;
//            this.Lambda = p.Lambda;
//            this.LW = p.LW;
//            this.HW = p.HW;
//            this.XC = p.XC;
//            this.YC = p.YC;
//            this.RC = p.RC;
//            this.U_inf = p.U_inf;
//            this.NW = p.NW;
//            this.NB = p.NB;
//            this.Hardness = p.Hardness;
//            this.Flexibility = p.Flexibility;
//        }
//    }
//}
