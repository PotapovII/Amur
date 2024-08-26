//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 23.02.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace RiverLib
{
    using CommonLib.EConverter;
    using MemLogLib;
    using System;
    using System.ComponentModel;
    using System.IO;

    /// <summary>
    /// Тип формулы для расчета донных напряжений
    /// </summary>
    [Serializable]
    public enum TypeSWETau
    {
        /// <summary>
        /// Дарси
        /// </summary>
        [Description("Формула Дарси")]
        Darcy = 0,
        /// <summary>
        /// Шези по Манингу
        /// </summary>
        [Description("Формула Манингу")]
        Maning,
        /// <summary>
        /// Шези по ...
        /// </summary>
        [Description("Формула Манингу1")]
        Shezi_1,
        /// <summary>
        /// аналитика
        /// </summary>
        [Description("Формула Манингу2")]
        Analytics
    }
    /// <summary>
    ///  ОО: Параметры для класса RiverKGDShallowWater1D 
    /// </summary>
    [Serializable]
    public class River1DSWParams : RiverStreamParams
    {
        /// <summary>
        /// Коэффициент гидравлического сопротивления потоку
        /// </summary>
        [DisplayName("Гидравлическое сопротивление")]
        [Category("Гидрология")]
        public double Lambda { get; set; }
        /// <summary>
        /// Коэффициент шероховатости по Манингу
        /// </summary>
        [DisplayName("Шероховатость по Манингу")]
        [Category("Гидрология")]
        public double Maning { get; set; }
        /// <summary>
        /// Метод расчета придонных напряжений
        /// </summary>
        [DisplayName("Метод расчета напряжений")]
        [Category("Гидрология")]
        [TypeConverter(typeof(MyEnumConverter))]
        public TypeSWETau typeTau { get; set; }
        /// <summary>
        /// скорость потока на входе в область
        /// </summary>
        [DisplayName("Скорость притока")]
        [Category("Граничные условия")]
        public double U0 { get; set; }
        /// <summary>
        /// глубина потокана выходе
        /// </summary>
        [DisplayName("Скорость стока")]
        [Category("Граничные условия")]
        public double H0 { get; set; }
        /// <summary>
        /// тип граничных условий на входе
        /// </summary>
        [DisplayName("Тип граничных условий")]
        [Category("Граничные условия")]
        public int typeBCInlet { get; set; }
        /// <summary>
        /// тип граничных условий на входе
        /// </summary>
        [DisplayName("Тип граничных условий")]
        [Category("Граничные условия")]
        public int typeBCOut { get; set; }

        public River1DSWParams() : base()
        {
            Maning = 0.2;
            Lambda = 0.01;
            typeTau = TypeSWETau.Analytics;
            CountKnots = 100;
            typeBCInlet = 0;
            typeBCOut = 1;
            U0 = 1;
            H0 = 1;
        }
        public River1DSWParams(River1DSWParams p)
        {
            SetParams(p);
        }
        /// <summary>
        /// Установка свойств задачи
        /// </summary>
        /// <param name="p"></param>
        public void SetParams(object p)
        {
            River1DSWParams pp = p as River1DSWParams;
            SetParams(pp);
        }
        public void SetParams(River1DSWParams p)
        {
            base.Set(p);
            U0 = p.U0;
            H0 = p.H0;
            Maning = p.Maning;
            Lambda = p.Lambda;
            typeTau = p.typeTau;
            typeBCInlet = p.typeBCInlet;
            typeBCOut = p.typeBCOut;
            CountKnots = p.CountKnots;
            J = p.J;
        }
        public override object GetParams()
        {
            return this;
        }
        public override void LoadParams(string fileName = "")
        {
            string message = "Файл парамеров задачи - доные деформации - не обнаружен";
            WR.LoadParams(Load, message, fileName);
        }
        /// <summary>
        /// Чтение параметров задачи из файла
        /// </summary>
        /// <param name="file"></param>
        public override void Load(StreamReader file)
        {
            base.Load(file);
            this.Lambda = LOG.GetDouble(file.ReadLine());
            this.Maning = LOG.GetDouble(file.ReadLine());
            this.typeTau = (TypeSWETau)LOG.GetInt(file.ReadLine());
            this.H0 = LOG.GetDouble(file.ReadLine());
            this.U0 = LOG.GetDouble(file.ReadLine());
            this.typeBCInlet = LOG.GetInt(file.ReadLine());
            this.typeBCOut = LOG.GetInt(file.ReadLine());
        }
    }

}
