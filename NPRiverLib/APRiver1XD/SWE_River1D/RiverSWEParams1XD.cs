//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 23.02.2021 Потапов И.И.
//---------------------------------------------------------------------------
//               Реверс в NPRiverLib 22.07.2024 Потапов И.И.
//---------------------------------------------------------------------------


namespace NPRiverLib.APRiver_1XD.River1DSW
{
    using CommonLib;
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
    public class RiverSWEParams1XD : ITProperty<RiverSWEParams1XD>
    {
        public RiverSWEParams1XD Clone(RiverSWEParams1XD p)
        {
            return new RiverSWEParams1XD(p);
        }
        /// <summary>
        /// уклон свободной поверхности потока
        /// </summary>
        [DisplayName("Уклон свободной поверхности")]
        [Category("Гидрология")]
        public double J { get; set; }
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
        [DisplayName("Скорость на входе")]
        [Category("Граничные условия")]
        public double U0 { get; set; }
        /// <summary>
        /// глубина потокана выходе
        /// </summary>
        [DisplayName("Глубина на выходе")]
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
        /// <summary>
        /// количество узлов по дну реки
        /// </summary>
        [DisplayName("количество узлов по смоченному периметру")]
        [Category("Сетка")]
        public int CountKnots { get; set; }

        public RiverSWEParams1XD() : base()
        {
            J = 0.0001;
            Maning = 0.2;
            Lambda = 0.01;
            typeTau = TypeSWETau.Analytics;
            U0 = 1;
            H0 = 1;
            typeBCInlet = 0;
            typeBCOut = 1;
            CountKnots = 100;
        }
        public RiverSWEParams1XD(RiverSWEParams1XD p)
        {
            SetParams(p);
        }
        /// <summary>
        /// Установка свойств задачи
        /// </summary>
        /// <param name="p"></param>
        public void SetParams(object p)
        {
            RiverSWEParams1XD pp = p as RiverSWEParams1XD;
            SetParams(pp);
        }
        public void SetParams(RiverSWEParams1XD p)
        {
            J = p.J;
            Maning = p.Maning;
            Lambda = p.Lambda;
            typeTau = p.typeTau;
            U0 = p.U0;
            H0 = p.H0;
            typeBCInlet = p.typeBCInlet;
            typeBCOut = p.typeBCOut;
            CountKnots = p.CountKnots;
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
            this.J = LOG.GetDouble(file.ReadLine());
            this.Maning = LOG.GetDouble(file.ReadLine());
            this.Lambda = LOG.GetDouble(file.ReadLine());
            this.typeTau = (TypeSWETau)LOG.GetInt(file.ReadLine());
            this.U0 = LOG.GetDouble(file.ReadLine());
            this.H0 = LOG.GetDouble(file.ReadLine());
            this.typeBCInlet = LOG.GetInt(file.ReadLine());
            this.typeBCOut = LOG.GetInt(file.ReadLine());
            this.CountKnots = LOG.GetInt(file.ReadLine());
        }
    }
}
