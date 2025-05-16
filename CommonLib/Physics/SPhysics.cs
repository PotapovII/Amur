#region License
/*
Copyright (c) 2019 - 2024  Potapov Igor Ivanovich, Khabarovsk

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion
//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2024 -
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                           07.01.24
//---------------------------------------------------------------------------
namespace CommonLib.Physics
{
    using System;
    using System.IO;
    using System.ComponentModel;

    using MemLogLib;
    using CommonLib.EConverter;
    using CommonLib.Delegate;
    using CommonLib.Mesh;
    using System.Linq;
    using CommonLib.Function;
    using System.Xml.Linq;
    using CommonLib.EddyViscosity;


    /// <summary>
    /// Турбулентная вязкость 
    /// </summary>
    /// <param name="mu_t"></param>
    /// <param name="wMesh"></param>
    /// <param name="typeEddyViscosity">способ расчета динамической скорости</param>
    /// <param name="U">контекстное поле</param>
    /// <param name="J">уклон русла</param>
    public delegate void CalkTurbVisc(ref double[] mu_t, TypeTask typeTask, IMWRiver wMesh, 
                                       ECalkDynamicSpeed typeEddyViscosity, double[] U,  double J = 0);
    /// <summary>
    /// ОО: Источник физических параметров русловых задач (патерн одиночка)
    /// </summary>
    public sealed class SPhysics : IPropertyTask
    {
        #region УПРАВЛЕНИЕ источником SPhysics и реализация интерфейса IPropertyTask
        /// <summary>
        /// ссылка для блокировки
        /// </summary>
        public static string FileName = "PhysicsData.riv";
        /// <summary>
        /// Чтение параметров задачи из файла (FileName) для локальной настройки 
        /// </summary>
        public void LoadParams(string fileName)
        {
            FileName = fileName;
            Load();
        }
        /// <summary>
        /// Чтение параметров задачи из файла (FileName) для локальной настройки 
        /// </summary>
        public void Load()
        {
            if (FileName != "")
            {
                using (StreamReader file = new StreamReader(FileName))
                {
                    try
                    {
                        Cp = LOG.GetDouble(file.ReadLine());
                        rho_s = LOG.GetDouble(file.ReadLine());
                        phi = LOG.GetDouble(file.ReadLine());
                        d50 = LOG.GetDouble(file.ReadLine());
                        epsilon = LOG.GetDouble(file.ReadLine());
                        kappa = LOG.GetDouble(file.ReadLine());
                        cx = LOG.GetDouble(file.ReadLine());
                        f = LOG.GetDouble(file.ReadLine());
                        ks = LOG.GetDouble(file.ReadLine());
                        mu_t = LOG.GetDouble(file.ReadLine());
                        K_filtr = LOG.GetDouble(file.ReadLine());
                        Water_f = LOG.GetDouble(file.ReadLine());

                        сritTauType = (ECritTauType)LOG.GetInt(file.ReadLine());
                        avtoPhi = LOG.GetBool(file.ReadLine());
                        avtoKappa = LOG.GetBool(file.ReadLine());

                        wsType = (EWsType)LOG.GetInt(file.ReadLine());
                        particleForms = (ParticleForms)LOG.GetInt(file.ReadLine());
                        сbType = (ECbType)LOG.GetInt(file.ReadLine());
                        turbViscType = (ETurbViscType)LOG.GetInt(file.ReadLine());
                    }
                    catch (Exception ee)
                    {
                        Logger.Instance.Exception(ee);
                        Logger.Instance.Info("Не согласованность версии программы и файла данных");
                        Logger.Instance.Info("Использованы параметры по умолчанию");
                    }
                    // Пересчет зависимых параметров
                    file.Close();
                }
            }
        }
        /// <summary>
        /// Установка свойств задачи
        /// </summary>
        /// <param name="p"></param>
        public void SetParams(object p)
        {
            SetParams((SPhysics)p);
        }
        /// <summary>
        /// установка параметров
        /// </summary>
        /// <param name="ps"></param>
        public void SetParams(SPhysics ps)
        {
            Cp = ps.Cp;
            rho_s = ps.rho_s;
            phi = ps.phi;
            d50 = ps.d50;
            epsilon = ps.epsilon;
            kappa = ps.kappa;
            cx = ps.cx;
            f = ps.f;
            ks = ps.ks;
            mu_t = ps.mu_t;
            K_filtr = ps.K_filtr;
            Water_f = ps.Water_f;
            wsType = ps.wsType;
            particleForms = ps.particleForms;
            сritTauType = ps.сritTauType;
            avtoPhi = ps.avtoPhi;
            avtoKappa = ps.avtoKappa;
            InitBedLoad();
        }
        /// <summary>
        /// свойств задачи
        /// </summary>
        /// <param name="p"></param>
        public object GetParams()
        {
            return PHYS;
        }
        #endregion

        #region Служебные поля
        /// <summary>
        /// Синглетон 
        /// </summary>
        private static SPhysics instance = null;
        /// <summary>
        /// ссылка для блокировки
        /// </summary>
        private static object syncRoot = new Object();
        #endregion

        #region Константы
        /// <summary>
        /// Ускорение свободного падения [м/с²]
        /// </summary>
        public const float GRAV = 9.80665f; // м/с²
        /// <summary>
        /// Плотность воды
        /// </summary>
        public const float rho_w = 1000f; // кг/м^3
        /// <summary>
        /// плотность льда rho_ice
        /// </summary>
        public const double rho_ise = 920; // кг/м^3
        /// <summary>
        ///  кинематическая вязкость воды
        /// </summary>
        public const double nu = 1e-6;
        /// <summary>
        ///  вязкость воды
        /// </summary>
        public const double mu = 1e-3;
        /// <summary>
        /// параметр Кармана для чистой воды
        /// </summary>
        public const double kappa_w = 0.41;

        #endregion

        #region Настройка физической модели модели
        /// <summary>
        /// Вычисление угла внутреннего трения через диаметр донного матрериала
        /// </summary>
        [DisplayName("Вычисление phi(d50)")]
        [Description("Вычисление угла внутреннего трения")]
        [Category("Настройка физической модели")]
        [TypeConverter(typeof(BooleanTypeConverterYN))]
        public bool avtoPhi { get; set; }
        /// <summary>
        /// Вычисление угла параметра Кармана через концентрацию f и пористость epsilon
        /// </summary>
        [DisplayName("Вычисление kappa(f,eps)")]
        [Description("Вычисление параметра Кармана")]
        [Category("Настройка физической модели")]
        [TypeConverter(typeof(BooleanTypeConverterYN))]
        public bool avtoKappa { get; set; }
        /// <summary>
        /// Вычисление угла параметра Кармана через концентрацию f и пористость epsilon
        /// </summary>
        [DisplayName("Гидравлическая крупность Ws")]
        [Description("Флаг выбора формулы для вычисления " +
                            "гидравлической крупности Ws")]
        [Category("Настройка физической модели")]
        [TypeConverter(typeof(MyEnumConverter))]
        public EWsType wsType { get; set; }
        [DisplayName("Форма частиц")]
        [Description("Флаг формы частиц для модели расчета гидравлической крупности Ws")]
        [Category("Настройка физической модели")]
        [TypeConverter(typeof(MyEnumConverter))]
        public ParticleForms particleForms { get; set; }
        /// <summary>
        /// Флаг для модели алгебраической турбулентной вязкости
        /// </summary>
        [DisplayName("Тип алгебраической модели турбулентности")]
        [Description("Флаг выбора формулы для вычисления" +
                            "турбулентной вязкости")]
        [Category("Настройка физической модели")]
        public ETurbViscType turbViscType
        {
            get => _turbViscType;
            set { _turbViscType = value; SetTurbViscModel(); }
        }
        ETurbViscType _turbViscType;
        /// <summary>
        /// Вычисление придонной концентрации
        /// </summary>
        [DisplayName("Придонная концентрация Cb")]
        [Description("Флаг выбора формулы для вычисления " +
            "               придонной концентрации Cb")]
        [Category("Настройка физической модели")]
        [TypeConverter(typeof(MyEnumConverter))]
        public ECbType сbType { get; set; }
        /// <summary>
        /// Флаг для модели расчета критических напряжений
        /// </summary>
        [DisplayName("Критическое напряжение")]
        [Description("Флаг выбора формулы для вычисления придонного" +
                "критического напряжения tau0")]
        [Category("Настройка физической модели")]
        [TypeConverter(typeof(MyEnumConverter))]
        public ECritTauType сritTauType { get; set; }
        #endregion

        #region Физические параметры
        /// <summary>
        /// плотность частиц, кг/м^3
        /// </summary>
        [DisplayName("Плотность частиц (кг/м^3)")]
        [Category("Физика")]
        public double rho_s { get; set; }
        /// <summary>
        /// угол внутреннего трения донных частиц
        /// </summary>
        [DisplayName("Угол внутреннего трения")]
        [Category("Физика")]
        public double phi { get; set; }
        /// <summary>
        /// параметр Кармана
        /// </summary>
        [DisplayName("Параметр Кармана kappa")]
        [Category("Физика")]
        public double kappa { get; set; }
        /// <summary>
        /// диаметр частиц
        /// </summary>
        [DisplayName("Средний диаметр частиц d50")]
        [Category("Физика")]
        public double d50 { get; set; }
        /// <summary>
        /// коэффициент лобового столкновения частиц
        /// </summary>
        [DisplayName("Коэффициент лобового столкновения cx")]
        [Category("Физика")]
        public double cx { get; set; }
        /// <summary>
        /// коэффициент пористости дна
        /// </summary>
        [DisplayName("Коэффициент пористости дна eps")]
        [Category("Физика")]
        public double epsilon { get; set; }
        /// <summary>
        /// концентрация частиц в активном слое
        /// </summary>
        [DisplayName("Концентрация частиц в активном слое f")]
        [Category("Физика")]
        public double f { get; set; }
        /// <summary>
        /// Шероховатость
        /// </summary>
        [DisplayName("Высота донной шероховатости ks")]
        [Category("Физика")]
        public double ks { get; set; }
        /// <summary>
        /// Турбулентная вихревая вязкость потока (постоянная)
        /// </summary>
        [DisplayName("Турбулентная вязкость потока mu_t")]
        [Category("Физика")]
        public double mu_t { get; set; }
        /// <summary>
        /// Коэффициент фильтрации грунта
        /// </summary>
        [DisplayName("Коэффициент фильтрации грунта K_filtr")]
        [Category("Физика")]
        public double K_filtr { get; set; }

        /// <summary>
        /// Коэффициент водонасыщенности
        /// </summary>
        [DisplayName("Коэффициент водонасыщенности Water_f")]
        [Category("Физика")]
        public double Water_f { get; set; }
        /// <summary>
        /// Удельная теплоемкость потока 
        /// </summary>
        [DisplayName("Удельная теплоемкость")]
        [Description("Удельная теплоемкость потока , Дж/К")]
        [Category("Физика")]
        public double Cp { get; set; }
        /// <summary>
        /// Удельная теплоемкость потока 
        /// </summary>
        [DisplayName("Критические напряжения на ровном дне")]
        [Category("Физика")]
        public double tau0 { get; set; } = 0;
        /// <summary>
        /// Удельная теплоемкость потока 
        /// </summary>
        [DisplayName("Критические Шильдс")]
        [Category("Физика")]
        public double theta0 { get; set; } = 0;
        #endregion
        /// <summary>
        /// Придонная концентрация взвешенных наносов
        /// </summary>
        public Function<double, double> Cb;
        /// <summary>
        /// Расчет турбулентной вязкости
        /// </summary>
        public CalkTurbVisc calkTurbVisc;
        #region Вычисляемые параметры задачи
        /// <summary>
        /// безразмерный диаметр частиц
        /// </summary>
        public double Db, Db_03;
        /// <summary>
        /// безразмерный диаметр частиц Db^3
        /// </summary>
        public double db;
        /// <summary>
        /// гидравлическая крупность частицы
        /// </summary>
        public double Ws;
        /// <summary>
        /// множитель для приведения придонного давления к напору
        /// </summary>
        public double gamma;
        /// <summary>
        /// тангенс угла phi
        /// </summary>
        public double tanphi;
        /// <summary>
        /// относительная плотность
        /// </summary>
        public double rho_b;
        /// <summary>
        /// константа расхода влекомых наносов
        /// </summary>
        public double G1;
        /// <summary>
        /// параметр стратификации активного слоя, 
        /// в котором переносятся донные частицы
        /// </summary>
        public double s;
        /// <summary>
        /// коэффициент сухого трения
        /// </summary>
        public double Fa0;
        /// <summary>
        /// коэффициент сухого трения
        /// </summary>
        public double normaTheta;
        /// <summary>
        /// Критическое число Рауза
        /// </summary>
        public double RaC = 8;
        /// <summary>
        /// масштаб микро скорости
        /// </summary>
        /// <returns></returns>
        public double Vd;
        /// <summary>
        /// Максимальная концентрация частиц на дне
        /// </summary>
        public double C0;
        ///// <summary>
        ///// Коэффициент шероховатости по Манингу
        ///// </summary>
        //public double Maning;
        /// <summary>
        /// Коэффициент гидравлического сопротивления потоку
        /// </summary>
        public double Lambda;
        /// <summary>
        /// Критическая динамическая скорость
        /// </summary>
        public double u_cr;
        /// <summary>
        /// Константа формы частиц Гришанина для определения Ws
        /// </summary>
        double Const_Grishanin = 1.2;
        /// <summary>
        /// Пересчет зависимых параметров задачи
        /// </summary>
        private void InitBedLoad()
        {
            gamma = 1.0 / (rho_w * GRAV);
            // относительная плотность
            rho_b = (rho_s - rho_w) / rho_w;
            // параметр стратификации активного слоя, 
            // в котором переносятся донные частицы
            s = f * rho_b;
            // Критическая динамическая скорость
            u_cr = rho_b * d50 * d50 * GRAV / (18 * nu);
            // тангенс угла внешнего откоса
            if (avtoPhi == true)
                tanphi = 1.15 * Math.Pow(d50, 1.0 / 7);
            else
                tanphi = Math.Tan(phi / 180 * Math.PI);
            if (avtoKappa == true)
            {
                kappa = 0.22 * (1 + s);
                kappa = WR.FormatCut(kappa);
            }
            // сухое трение
            Fa0 = tanphi * (rho_s - rho_w) * GRAV;
            // 
            normaTheta = (rho_s - rho_w) * GRAV * d50;
            // Флаг формы частиц для модели расчета гидравлической крупности
            switch (particleForms)
            {
                case ParticleForms.Spherical:
                    Const_Grishanin = 1.2;
                    break;
                case ParticleForms.Polyhedral:
                    Const_Grishanin = 1.05;
                    break;
                case ParticleForms.Flattened:
                    Const_Grishanin = 0.75;
                    break;
                case ParticleForms.Lamellar:
                    Const_Grishanin = 0.5;
                    break;
            }
            // критические напряжения на ровном дне
            switch (сritTauType)
            {
                case ECritTauType.Petrov1991:
                    tau0 = 9.0 / 8.0 * kappa * kappa * d50 * Fa0 / cx;
                    break;
                case ECritTauType.Petrov2001:
                    tau0 = 9.0 / 8.0 * kappa * kappa * d50 * Fa0 / cx;
                    break;
                default:
                    tau0 = 9.0 / 8.0 * kappa * kappa * d50 * Fa0 / cx;
                    break;
            }
            theta0 = tau0 / normaTheta;

            C0 = 1 - epsilon;
            // константа расхода влекомых наносов
            G1 = 4.0 / (3.0 * kappa * Math.Sqrt(rho_w) * Fa0 * C0);
            // безразмерный диаметр частиц
            db = rho_b * GRAV * (d50 / nu) * (d50 / nu) * d50;
            //Db = d50 * Math.Pow(rho_b * GRAV / (nu * nu), 1.0 / 3);
            Db = Math.Pow(db, 1.0 / 3);
            // масштаб микро скорости
            Vd = nu / d50;
            Lambda = 0.02;
            // модели расчета гидравлической крупности
            switch (wsType)
            {
                case EWsType.Arkhangelsky: Ws = Ws_Arkhangelsky(); break;
                case EWsType.Grishanin: Ws = Ws_Grishanin(); break;
                case EWsType.Goncharova: Ws = Ws_Goncharova(); break;
                case EWsType.Ibade_Zade: Ws = Ws_Ibade_Zade(); break;
                case EWsType.Ruby: Ws = Ws_Ruby(); break;
                case EWsType.Sha: Ws = Ws_Sha(); break;
                case EWsType.Van_Rijn: Ws = Ws_Van_Rijn(); break;
                default: Ws = Ws_Van_Rijn(); break;
            }
            // модели расчета придонной концентрации
            switch (сbType)
            {
                case ECbType.Potapov_2024:
                    Cb = Cb_PotapovII_2024;
                    break;
                case ECbType.Einstein_1950:
                    Cb = Cb_Einstein_1950;
                    break;
                case ECbType.Van_Rijn_Leo_1984:
                    Cb = Cb_van_Rijn_Leo_1984;
                    break;
                case ECbType.Van_Rijn_Leo_1986:
                    Cb = Cb_van_Rijn_Leo_1986;
                    break;
                case ECbType.Smith_McLean_1977:
                    Cb = Cb_Smith_McLean_1977;
                    break;
                case ECbType.Engelund_Freds0e_1976:
                    Cb = Cb_Engelund_Freds0e_1976;
                    break;
            }
            SetTurbViscModel();
        }

        /// Пересчет зависимых параметров задачи
        /// </summary>
        public void GetLocalBedLoadParams(double roughness, ref double u_cr, ref double tau0,
                                    ref double tanphi, ref double Fa0, ref double theta0)
        {
            double d50 = 0.05 * roughness;
            // Критическая динамическая скорость
            u_cr = rho_b * d50 * d50 * GRAV / (18 * nu);
            // тангенс угла внешнего откоса
            if (avtoPhi == true)
                tanphi = 1.15 * Math.Pow(d50, 1.0 / 7);
            else
                tanphi = Math.Tan(phi / 180 * Math.PI);
            // сухое трение
            Fa0 = tanphi * (rho_s - rho_w) * GRAV;
            // 
            tau0 = 9.0 / 8.0 * kappa * kappa * d50 * Fa0 / cx;
            normaTheta = (rho_s - rho_w) * GRAV * d50;
            theta0 = tau0 / normaTheta;
        }

        /// Пересчет зависимых параметров задачи
        /// </summary>
        public void GetLocalBedLoadParams(double roughness, ref double tau0, ref double tanphi, ref double G1)
        {
            double d50 = 0.005 * roughness;
            //double d50 = Convert_ks_to_d50(roughness, H)
            // тангенс угла внешнего откоса
            if (avtoPhi == true)
                tanphi = 1.15 * Math.Pow(d50, 1.0 / 7);
            else
                tanphi = Math.Tan(phi / 180 * Math.PI);
            tau0 = 9.0 / 8.0 * kappa * kappa * d50 * Fa0 / cx;
            Fa0 = tanphi * (rho_s - rho_w) * GRAV;
            // константа расхода влекомых наносов
            G1 = 4.0 / (3.0 * kappa * Math.Sqrt(rho_w) * Fa0 * C0);
        }


        /// <summary>
        /// модели алгебраической турбулентной вязкости
        /// </summary>
        void SetTurbViscModel()
        {
            switch (_turbViscType)
            {
                case ETurbViscType.Boussinesq1865:
                    calkTurbVisc = calkTurbVisc_Boussinesq1865;
                    break;
                case ETurbViscType.Karaushev1977:
                    calkTurbVisc = calkTurbVisc_Karaushev1977;
                    break;
                case ETurbViscType.Prandtl1934:
                    calkTurbVisc = calkTurbVisc_Prandtl1934;
                    break;
                case ETurbViscType.Velikanov1948:
                    calkTurbVisc = calkTurbVisc_Velikanov1948;
                    break;
                case ETurbViscType.VanDriest1956:
                    calkTurbVisc = calkTurbVisc_VanDriest1956;
                    break;
                case ETurbViscType.Absi_2012:
                    calkTurbVisc = calkTurbVisc_Absi_2012;
                    break;
                case ETurbViscType.Absi_2019:
                    calkTurbVisc = calkTurbVisc_Absi_2019;
                    break;
                case ETurbViscType.Leo_C_van_Rijn1984:
                    calkTurbVisc = calkTurbVisc_Leo_van_Rijn1984;
                    break;
                case ETurbViscType.GLS_1995:
                    calkTurbVisc = calkTurbVisc_GLS_1995;
                    break;
                case ETurbViscType.Les_Smagorinsky_Lilly_1996:
                    calkTurbVisc = calkTurbVisc_Smagorinsky_Lilly_1996;
                    break;
                case ETurbViscType.Derek_G_Goring_and_K_1997:
                    calkTurbVisc = calkTurbVisc_Derek_G_Goring_and_K_1997;
                    break;
                case ETurbViscType.PotapobII_2024:
                    calkTurbVisc = calkTurbVisc_PotapovII_2024;
                    break;
            }
        }
        #endregion
        private SPhysics()
        {
            FileName = "";
            //Cp = 1.0;  // воздух
            Cp = 4183.0; // вода
            rho_s = 2650;
            phi = 30;
            //
            //d50 = 0.00071;
            //d50 = 0.00022;
            d50 = 0.0005;
            // 
            epsilon = 0.35;
            kappa = 0.25;
            cx = 0.5;
            f = 0.1;
            //ks = 0.1;
            ks = 0.15; // Бетон
            //ks = 0.00328; // Бетон
            mu_t = 0.1;
            // зависит от d50
            K_filtr = 25 / (24 * 3600);
            Water_f = 0.5;
            

            сbType = ECbType.Van_Rijn_Leo_1984;
            wsType = EWsType.Goncharova;
            particleForms = ParticleForms.Flattened;

            сritTauType = ECritTauType.Petrov2001;
            _turbViscType = ETurbViscType.Boussinesq1865;

            avtoPhi = true;
            avtoKappa = true;
            Load();
            InitBedLoad();
        }
        /// <summary>
        /// ссылка на объект
        /// </summary>
        public static SPhysics PHYS //Instance
        {
            get
            {
                lock (syncRoot)
                {
                    if (instance == null)
                    {

                        if (instance == null)
                            instance = new SPhysics();
                    }
                    return instance;
                }
            }
        }

        /// <summary>
        /// Вычисление  коэффициента Шези !!!! доработать со ссылками
        /// </summary>
        /// <param name="us"></param>
        /// <returns></returns>
        public double Cs(double H)
        {
            if (H / ks > 1)
                return 5.75 * Math.Log10(12 * H / ks);
            else
                return 5.75 * Math.Log10(12);
        }
        /// <summary>
        /// Расчет шероховатости по среднему диаметру песка в диапазоне
        /// 0.0002 м < d50 < 0.003 м
        /// </summary>
        /// <param name="d50"></param>
        /// <param name="H"></param>
        /// <returns></returns>
        public double Convert_d50_to_ks(double d50, double H)
        {
            double A = 3.25;
            double B = 12;
            return B * H / Math.Exp(A * kappa_w * Math.Pow(H / d50, 6));
        }
        /// <summary>
        /// Расчет среднего диаметра песка по шероховатости в диапазоне
        /// 0.0002 м < d50 < 0.01 м
        /// 0.05 < ks < 0.8
        /// </summary>
        /// <param name="d50"></param>
        /// <param name="H"></param>
        /// <returns></returns>
        public double Convert_ks_to_d50(double d50, double H)
        {
            double A = 3.25;
            double B = 12;
            return H * Math.Pow(((A * kappa_w) / Math.Log(B * H / ks)), 6);
        }



        /// <summary>
        /// Вычисление числа Рауза
        /// </summary>
        /// <param name="us"></param>
        /// <returns></returns>
        public double Ra(double us)
        {
            return Ws / (kappa * us + MEM.Error12);
        }

        #region Вычисление гидравлической крупности по различным моделям
        /// <summary>
        /// гидравлическая крупность по формуле Руби (1933)
        /// </summary>
        /// <returns></returns>
        public double Ws_Ruby()
        {
            if (Db < 38500)
                return 6 * (Math.Sqrt(1 + Db / 54) - 1) * Vd;
            else
                return 1.05 * Math.Sqrt(Db) / rho_b * Vd;
        }
        /// <summary>
        /// гидравлическая крупность по формуле Гончарова (1962)
        /// </summary>
        /// <returns></returns>
        public double Ws_Goncharova()
        {
            if (Db < 15)
                return 1.05 * Db / 18.0 * Vd;
            else
                return 1.37 * Math.Sqrt(Db) * Vd;
        }
        /// <summary>
        /// гидравлическая крупность по формуле Sha (1954)
        /// </summary>
        /// <returns></returns>
        public double Ws_Sha()
        {
            if (Db < 5)
                return Db / 34.0 * Vd;
            else
                //if(Db > 38500)
                return 1.14 * Math.Sqrt(Db) * Vd;
            //else
            //    return Db * Math.Exp(Math.Sqrt(39 - (Math.Log(Db)-5.777)* (Math.Log(Db) - 5.777) ) - 3.79 ) * Vd;
        }
        /// <summary>
        /// гидравлическая крупность по формуле Ibade-Zade (1992)
        /// </summary>
        /// <returns></returns>
        public double Ws_Ibade_Zade()
        {
            if (Db < 16)
                return Db / 24.0 * Vd;
            else
                return 1.068 * Math.Sqrt(Db) * Vd;
        }
        /// <summary>
        /// гидравлическая крупность по формуле Ibade-Zade (1992)
        /// </summary>
        /// <returns></returns>
        public double Ws_Van_Rijn()
        {
            if (Db < 5)
                return Db / 18.0 * Vd;
            else
                //if (Db > 5000)
                return 1.1 * Math.Sqrt(Db) * Vd;
            //else
            //    return 10* ( Math.Sqrt(1 + (10e-6) * Db * Db * Db) - 1) * Vd;
        }
        /// <summary>
        /// гидравлическая крупность по формуле Гришанина К В (1979)
        /// </summary>
        /// <returns></returns>
        public double Ws_Grishanin()
        {
            return Const_Grishanin * Math.Sqrt(Db) * Vd;
        }
        /// <summary>
        /// гидравлическая крупность по таблице Архангельского Б.В.(1935)
        /// при температуре 15 С
        /// </summary>
        /// <returns></returns>
        public double Ws_Arkhangelsky()
        {
            double[] d_50 = { 0.0001, 0.0002, 0.0003, 0.0004, 0.00050, 0.00075, 0.001, 0.0035 };
            double[] Ws_a = { 0.0057, 0.0170, 0.0321, 0.0461, 0.0567, 0.0858, 0.1115, 0.254 };
            IFunction1D WW = new DigFunction1D("A", d_50, Ws_a);
            return WW.FunctionValue(d50);
        }
        public double Ws_Sokolov()
        {
            // до 0.3 мм по Архангельскому Б.В.
            // Источник Шамов Г.И. Речные наносы 1959 ст 64
            IFunction1D WW = null;
            switch (particleForms)
            {
                case ParticleForms.Spherical: // округленная форма частиц
                    {
                        double[] d_50 = { 0.0001, 0.0002, 0.0003, 0.0004, 0.0005, 0.0007, 0.0009, 0.00125, 0.00225, 0.00275, 0.0035 };
                        double[] Ws_a = { 0.008,  0.021,  0.0321, 0.051,  0.06,   0.0111, 0.0125, 0.148,   0.214,   0.2211,  0.254 };
                        WW = new DigFunction1D("A", d_50, Ws_a);
                    }
                    break;
                case ParticleForms.Polyhedral: // многогранная форма частиц
                    {
                        double[] d_50 = { 0.0001, 0.0002, 0.0003, 0.0004, 0.0007, 0.0009, 0.00125, 0.00225, 0.00275, 0.0035 };
                        double[] Ws_a = { 0.0057, 0.0170, 0.0321, 0.047,  0.0097, 0.0118, 0.131,   0.196,   0.1996,  0.229 };
                        WW = new DigFunction1D("A", d_50, Ws_a);
                    }
                    break;
                case ParticleForms.Flattened: // удлиненная форма частиц
                    {
                        double[] d_50 = { 0.0001, 0.0002, 0.0003, 0.0005, 0.0007, 0.0009, 0.00125, 0.00225, 0.00275, 0.0035 };
                        double[] Ws_a = { 0.0057, 0.021, 0.0321, 0.061,  0.0105, 0.0115, 0.133,   0.167,   0.19,    0.193 };
                        WW = new DigFunction1D("A", d_50, Ws_a);
                    }
                    break;
                case ParticleForms.Lamellar: // пластинчатая форма частиц
                    {
                        double[] d_50 = { 0.0001, 0.0002, 0.0003, 0.0004, 0.0007, 0.0009, 0.00125, 0.00225, 0.00275, 0.0035 };
                        double[] Ws_a = { 0.0057, 0.0170, 0.0321, 0.054,  0.07,   0.087,  0.096,   0.123,   0.1305,  0.146  };
                        WW = new DigFunction1D("A", d_50, Ws_a);
                    }
                    break;
            }
            return WW.FunctionValue(d50);
        }

        #endregion
        /// <summary>
        /// Таблица удельного сцепления песков разной крупности
        /// </summary>
        /// <param name="d"></param>
        /// <param name="eps"></param>
        /// <returns></returns>
        public static double GetAdhesion(double d, double eps = 0.45)
        {
            if (eps > 0.75) 
            {
                return 0;
            }
            if (eps < 0.5 && eps <= 0.55)
            {
                if (d <= 0.0025) return 0;
                if (d > 0.0025 && d > 0.002) return 0.0;
                if (d > 0.002 &&  d > 0.001) return 0.01;
                if (d > 0.001 &&  d > 0.0005) return 0.02;
                if (d > 0.0005 && d > 0.00025) return 0.04;
                if (d > 0.00025 && d > 0.0001) return 0.06;
                if (d > 0.00025 && d > 0.0001) return 0.08;
            }
            if (eps < 0.45 && eps <= 0.5)
            {
                if (d <= 0.0025) return 0.02;
                if (d > 0.0025 && d > 0.002) return 0.3;
                if (d > 0.002 && d > 0.001) return 0.06;
                if (d > 0.001 && d > 0.0005) return 0.08;
                if (d > 0.0005 && d > 0.00025) return 0.010;
                if (d > 0.00025 && d > 0.0001) return 0.012;
                if (d > 0.00025 && d > 0.0001) return 0.016;
            }
            if (eps < 0.35 && eps <= 0.45)
            {
                if (d <= 0.0025) return 0.04;
                if (d > 0.0025 && d > 0.002) return 0.6;
                if (d > 0.002 && d > 0.001) return 0.08;
                if (d > 0.001 && d > 0.0005) return 0.012;
                if (d > 0.0005 && d > 0.00025) return 0.016;
                if (d > 0.00025 && d > 0.0001) return 0.020;
                if (d > 0.00025 && d > 0.0001) return 0.025;
            }
            return 0;
        }

        #region Вычисление придонной концентрации
    /// <summary>
    /// Вычисление придонной концентрации Потапов И И (2024)
    /// </summary>
    /// <returns></returns>
    double A = 1.0 / 22.5;
        double B = 1.0 / 110;
        public double Cb_PotapovII_2024(double tau)
        {
            double theta = Math.Abs(tau / SPhysics.PHYS.normaTheta);
            if (theta < theta0) return 0;
            double Cb = A * (theta - Math.Abs(theta0)) / (kappa * tanphi);
            return Cb;
        }
        public double Cb_PotapovII_2024_11(double tau)
        {
            double xi = (Math.Abs(tau) - tau0) / tau0;
            if (xi < 0) return 0;
            double Cb = B * xi / (kappa * tanphi);
            return Cb;
        }
        public double Cb_PotapovII_2024_06(double tau)
        {
            double u_star = Math.Sqrt( tau / SPhysics.rho_w );
            if (u_cr > u_star) return 0;
            
            double theta = Math.Abs(tau / SPhysics.PHYS.normaTheta);
            if (theta < theta0) return 0;
            double A = 1.0 / 22.5;
            double Cb = A * (theta - Math.Abs(theta0)) / (kappa * tanphi);
            return Cb;
        }
        /// <summary>
        /// Вычисление придонной концентрации Эйнштейн (1950) 
        /// Эйнштейн определил придонную концентрацию взвешенных отложений 
        /// на высоте двух диаметров a = 2 * d50          /// </summary>
        /// <returns></returns>
        public double Cb_Einstein_1950(double tau)
        {
            double theta = Math.Abs(tau / normaTheta);
            double dtheta = theta - theta0;
            if (dtheta < 0) return 0;
            double theta05 = Math.Sqrt(theta);
            double p = 1.0 / Math.Pow(1 + Math.Pow(Math.PI / 6 * tanphi / dtheta, 6), 0.25);
            double Phi = 5 * p * (theta05 - 0.7 * Math.Sqrt(theta0));
            double Cb = Phi / (23.2 * theta05);
            return Cb;
        }
        /// <summary>
        /// Вычисление придонной концентрации Ван Рейн (1984)
        /// </summary>
        /// <returns></returns>
        public double Cb_van_Rijn_Leo_1984(double tau)
        {
            double xi = (Math.Abs(tau) - tau0) / tau0;
            if (xi < 0) return 0;
            double Cb = 0.18 * C0 * xi / Db;
            return Cb;
        }
        /// <summary>
        /// Вычисление придонной концентрации Ван Рейн (1986)
        /// </summary>
        /// <returns></returns>
        public double Cb_van_Rijn_Leo_1986(double tau)
        {
            double xi = (Math.Abs(tau) - tau0) / tau0;
            if (xi < 0) return 0;
            double Cb = 0.015 * xi * Math.Sqrt(xi) / Math.Pow(Db, 0.3);
            return Cb;
        }

        /// <summary>
        /// Вычисление придонной концентрации Smith and McLean  (1977)
        /// </summary>
        /// <returns></returns>
        const double gamma_sm = 0.0024;
        public double Cb_Smith_McLean_1977(double tau)
        {
            double xi = (Math.Abs(tau) - tau0) / tau0;
            if (xi < 0) return 0;
            double Cb = C0 * gamma_sm * xi / (1 + gamma_sm * xi);
            return Cb;
        }
        /// <summary>
        /// Энгелунд и Фредс0е (1976) 
        /// </summary>
        /// <returns></returns>
        public double Cb_Engelund_Freds0e_1976(double tau)
        {
            double xi = (Math.Abs(tau) - tau0) / tau0;
            if (xi < 0) return 0;
            double chi = tau / tau0;
            double p = 1.0 / Math.Pow(1 + Math.Pow(Math.PI / 6 * tanphi / xi, 6), 0.25);
            double lambda = Math.Sqrt((xi - Math.PI / 6 * p * tanphi) / (0.027 * (rho_s / rho_w) * chi));
            double Cb = C0 / (1 + 1 / lambda);
            return Cb;
        }
        #endregion

        #region Вычисление динамической скорости в створе канала
        public double DynamicSpeedCrossJ(IMeshWrapper wm, double J)
        {
            double Area = wm.GetArea();
            double Bottom = wm.GetBottom();
            double HydrodynamicRadius = Area / Bottom;
            double dynamicSpeed = Math.Sqrt(GRAV * HydrodynamicRadius * J);
            return dynamicSpeed;
        }

        public double DynamicSpeedCrossKs(IMeshWrapper wm, double[] U)
        {
            double Area = wm.GetArea();
            double Bottom = wm.GetBottom();
            double HydrodynamicRadius = Area / Bottom;
            double dynamicSpeed = U.Max() * kappa_w / Math.Log(HydrodynamicRadius / ks);
            return dynamicSpeed;
        }
        /// <summary>
        /// Вычисление по расходу
        /// </summary>
        /// <param name="wm"></param>
        /// <param name="U"></param>
        /// <returns></returns>
        public double DynamicSpeedCrossCs(IMeshWrapper wm, double[] U)
        {
            double Area = 0;
            // Расход
            double FlowRate = wm.RiverFlowRate(U, ref Area);
            double Bottom = wm.GetBottom();
            double HydrodynamicRadius = Area / Bottom;
            // Безразмерный Шези
            double U_midle = FlowRate / Area;
            double mCs = Cs(HydrodynamicRadius);
            double dynamicSpeed = Math.Sqrt(mCs) * U_midle;
            return dynamicSpeed;
        }
        #endregion

        #region Вычисление алгебраической турбулентной вязкости (сделать расчет u_star вариативным !!!)

        /// <summary>
        /// Профиль турбулентной вязкости Буссинеск 1865
        /// </summary>
        
        public void calkTurbVisc_Boussinesq1865(ref double[] mu_t, TypeTask typeTask,
            IMWRiver wMesh, ECalkDynamicSpeed typeEddyViscosity, double[] Vx, double J = 0)
        {
            IMesh mesh = wMesh.GetMesh();
            double mu_t0;
            double a = 22;
            if (typeTask == TypeTask.streamY1D)
            {
                double Area = wMesh.GetArea();
                double Bottom = wMesh.GetBottom();
                double H0 = Area / Bottom;
                double mCs = Cs(H0);
                double Q = wMesh.RiverFlowRate(Vx, ref Area);
                double U0;
                //if (typeEddyViscosity == ECalkDynamicSpeed.u_start_U && Q > 0)
                //    U0 = Q / Area;
                //else
                    U0 = Math.Sqrt(SPhysics.GRAV * H0 * J) * mCs;
                
                    
                mu_t0 = rho_w * U0 * H0 * Math.Sqrt(GRAV) / (2 * a * mCs);
            }
            else
            {
                double[] Y = mesh.GetCoords(1);
                double H = Y.Max() - Y.Min();
                double mCs = Cs(H);
                double U0 = Vx.Sum() / Vx.Length;
                mu_t0 = rho_w * U0 * H * Math.Sqrt(GRAV) / (2 * a * mCs);
            }
            for (int node = 0; node < mesh.CountKnots; node++)
                mu_t[node] = mu_t0 + mu;
        }
        /// <summary>
        /// Профиль турбулентной вязкости Караушев 1977
        /// </summary>
        public void calkTurbVisc_Karaushev1977(ref double[] mu_t, TypeTask typeTask,
            IMWRiver wMesh,ECalkDynamicSpeed typeEddyViscosity, double[] Vx, double J = 0)
        {
            IMesh mesh = wMesh.GetMesh();
            double[] Hp = wMesh.GetHp();
            IMeshWrapper wm = (IMeshWrapper)wMesh;
            double Area = wm.GetArea();
            double Bottom = wm.GetBottom();
            double R0 = Area / Bottom;
            double Q = wm.RiverFlowRate(Vx, ref Area);
            double U0 = Q / Area;
            double mCs = Cs(R0) * Math.Sqrt(GRAV);
            //double U2 = mCs * Math.Sqrt(R0 * J);
            double U1 = Math.Sqrt(GRAV * R0 * J);
            double mM = 0.7 * mCs + 1.92 * Math.Sqrt(GRAV);
            //double mA = rho_w * GRAV * R0 * U1 / (mM * mCs);
            for (int node = 0; node < mesh.CountKnots; node++)
            {
                double mu_t0 = rho_w * (Vx[node] / U0) * U1 * GRAV * R0 / (mM * mCs);
                mu_t[node] = mu_t0 + mu;
            }
        }
        /// <summary>
        /// "Профиль турбулентной вязкости Прандтль 1934
        /// </summary>
        public void calkTurbVisc_Prandtl1934(ref double[] mu_t, TypeTask typeTask,
            IMWRiver wMesh, ECalkDynamicSpeed typeEddyViscosity, double[] Vx, double J = 0)
        {
            IMesh mesh = wMesh.GetMesh();
            double[] Hp = wMesh.GetHp();
            double[] Distance = wMesh.GetDistance();

            double u_star = 0;
            if (typeEddyViscosity != ECalkDynamicSpeed.u_start_J)
                u_star = Vx.Max() * kappa_w / Math.Log(Hp.Max() / ks);

            for (int node = 0; node < mesh.CountKnots; node++)
            {
                if (typeEddyViscosity == ECalkDynamicSpeed.u_start_J && J > MEM.Error7)
                      u_star = Math.Sqrt(GRAV * Hp[node] * J);
                double mu_t0 = 0;
                if (MEM.Equals(Math.Abs(Hp[node]), 0) == false)
                {
                    double xi = Distance[node] / Hp[node];
                    double F = Math.Max(0.0, (1 - xi) * xi);
                    mu_t0 = rho_w * kappa_w * u_star * Hp[node] * F;
                }
                mu_t[node] = mu_t0 + mu;
            }
        }
        /// <summary>
        /// Профиль турбулентной вязкости Великанова 1948
        /// </summary>
        public void calkTurbVisc_Velikanov1948(ref double[] mu_t, TypeTask typeTask,
            IMWRiver wMesh, ECalkDynamicSpeed typeEddyViscosity, double[] Vx, double J = 0)
        {
            IMesh mesh = wMesh.GetMesh();
            double[] Hp = wMesh.GetHp();
            double[] Distance = wMesh.GetDistance();
            IMeshWrapper wm = (IMeshWrapper)wMesh;
            double Area = 0;
            double Q = wm.RiverFlowRate(Vx, ref Area);
            double U0 = Q / Area;
            double U_max = Vx.Max();
            double u_star;
            for (int node = 0; node < mesh.CountKnots; node++)
            {
                if (typeEddyViscosity == ECalkDynamicSpeed.u_start_J && J > MEM.Error7)
                    u_star = Math.Sqrt(GRAV * Hp[node] * J);
                else
                    u_star = (U_max - U0) * kappa_w;
                double mu_t0 = 0;
                if (Math.Abs(Hp[node]) > MEM.Error4)
                {
                    double xi = Distance[node] / Hp[node];
                    double ks_b = d50 / Hp[node];
                    double F = Math.Max(0.0, (1 - xi) * (xi + ks_b));
                    mu_t0 = rho_w * kappa_w * u_star * Hp[node] * F;
                }
                mu_t[node] = mu_t0 + mu;
            }
        }
        /// <summary>
        /// Профиль турбулентной вязкости Великанова 1948 с ядром
        /// </summary>
        public void calkTurbVisc_Leo_van_Rijn1984(ref double[] mu_t, TypeTask typeTask,
            IMWRiver wMesh, ECalkDynamicSpeed typeEddyViscosity, double[] Vx, double J = 0)
        {
            IMesh mesh = wMesh.GetMesh();
            double[] Hp = wMesh.GetHp();
            double[] Distance = wMesh.GetDistance();
            

            if (Vx.Sum() == 0 || typeEddyViscosity == ECalkDynamicSpeed.u_start_J ||
                typeEddyViscosity == ECalkDynamicSpeed.u_start_M)
            {
                IMeshWrapper wm = (IMeshWrapper)wMesh;
                double Area = 0;
                double Q = wm.RiverFlowRate(Vx, ref Area);
                double U0 = Q / Area;
                double U_max = Vx.Max();
                double u_star = (U_max - U0) * kappa_w;
                for (int node = 0; node < mesh.CountKnots; node++)
                {
                    if (J > MEM.Error7 || typeEddyViscosity == ECalkDynamicSpeed.u_start_J)
                        u_star = Math.Sqrt(GRAV * Hp[node] * J);
                    
                    double mu_t0 = 0;
                    if (Math.Abs(Hp[node]) > MEM.Error4)
                    {
                        double xi = Distance[node] / Hp[node];
                        xi = Math.Min(xi, 0.5);
                        double ks_b = d50 / Hp[node];
                        double F = Math.Max(0.0, (1 - xi) * (xi + ks_b));
                        mu_t0 = rho_w * kappa_w * u_star * Hp[node] * F;
                    }
                    mu_t[node] = mu_t0 + mu;
                }
            }
            else
            {
                IMWCrossSection wm = (IMWCrossSection)wMesh;
                double[] Us = null;
                wm.CalkBoundary_U_star(Vx, ref Us);
                for (int node = 0; node < mesh.CountKnots; node++)
                {
                    double mu_t0 = 0;
                    double xi = Distance[node] / Hp[node];
                    if (xi > 0)
                    {
                        double u_star = Us[node];
                        if (Math.Abs(Hp[node]) > MEM.Error3 && u_star > 0)
                        {
                            xi = Math.Max(0, Math.Min(xi, 0.5));
                            double F = Math.Max(0.0, (1 - xi) * xi);
                            mu_t0 = rho_w * kappa_w * u_star * Hp[node] * F;
                        }
                    }
                    mu_t[node] = mu_t0 + mu;
                }
            }
        }

        public double Get_U_star_J(IMWDistance wMesh, double J)
        {
            IMeshWrapper wm = (IMeshWrapper)wMesh;
            double Area = wm.GetArea();
            double Bottom = wm.GetBottom();
            double R0 = Area / Bottom;
            double u_star = Math.Sqrt(GRAV * R0 * J);
            return u_star;
        }
        public double Get_U_star_Vx(IMWDistance wMesh, double[] Vx)
        {
            IMeshWrapper wm = (IMeshWrapper)wMesh;
            double Area = wm.GetArea();
            double Q = wm.RiverFlowRate(Vx, ref Area);
            double U0 = Q / Area;
            double U_max = Vx.Max();
            double u_star = (U_max - U0) * kappa_w;
            return u_star;
        }
        IMWCrossSection wm = null;
        /// <summary>
        /// Профиль турбулентной вязкости Рафик Абси 2012
        /// </summary>
        public void calkTurbVisc_Absi_2012(ref double[] mu_t, TypeTask typeTask,
            IMWRiver wMesh, ECalkDynamicSpeed typeEddyViscosity, double[] Vx, double J = 0)
        {
            try
            {
                IMesh mesh = wMesh.GetMesh();
                double[] Hp = wMesh.GetHp();
                double[] Distance = wMesh.GetDistance();
                double sVx = Vx.Sum();

                if (MEM.Equals(sVx, 0) == true ||
                     typeEddyViscosity == ECalkDynamicSpeed.u_start_J ||
                     typeEddyViscosity == ECalkDynamicSpeed.u_start_M ||
                     wMesh as IMWCrossSection == null)
                {
                    double u_star = 0;
                    if (typeEddyViscosity != ECalkDynamicSpeed.u_start_J && sVx > 0)
                    {
                        double mHp = Hp.Max() / d50;
                        if (mHp > 1)
                            u_star = Vx.Max() * kappa_w / Math.Log(mHp);
                    }
                    for (int node = 0; node < mesh.CountKnots; node++)
                    {
                        if (typeEddyViscosity == ECalkDynamicSpeed.u_start_J && J > MEM.Error7 ||
                            MEM.Equals(sVx, 0) == true)
                        {
                            if (Hp[node] > MEM.Error4)
                                u_star = Math.Sqrt(GRAV * Hp[node] * J);
                            else
                                u_star = 0;
                        }
                        mu_t[node] = GetMuAbsi_2012(Hp[node], Distance[node], u_star);
                    }
                }
                else
                {
                    double[] Us = null;
                    ((IMWCrossSection)wMesh).CalkBoundary_U_star(Vx, ref Us);
                    for (int node = 0; node < mesh.CountKnots; node++)
                        mu_t[node] = GetMuAbsi_2012(Hp[node], Distance[node], Us[node]);
                }
                //if (ERR.INF_NAN("Mu", mu_t) == false)
                //    throw new Exception("Ups");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public double GetMuAbsi_2012(double H, double D, double u_star)
        {
            double mu_t0 = 0;
            if (MEM.Equals(Math.Abs(H), 0) == false && u_star > MEM.Error5)
            {
                double xi = D / H;
                if (xi > MEM.Error5 && MEM.Equals(Math.Abs(u_star), 0) == false)
                {
                    double Re_star = H * u_star / nu;
                    double C1 = 1000;
                    double Z = 0.46 * Re_star - 5.98;
                    if (Z > MEM.Error2)
                    {
                        C1 = Re_star / Z;
                        double Ca = 0;
                        if (Math.Abs(0.46 * Re_star - 5.98) > MEM.Error8)
                            Ca = Math.Exp(-(0.34 * Re_star - 11.5) / Z);
                        if (Ca > 50)
                            Ca = Math.Exp(-Math.Abs((0.34 * Re_star - 11.5) / Z));
                        mu_t0 = rho_w * u_star * D * Ca * Math.Exp(-C1 * xi);
                    }
                }
            }
            return mu_t0 + mu;
        }
        
        /// <summary>
        /// Профиль турбулентной вязкости Рафик Абси 2019
        /// </summary>
        public void calkTurbVisc_Absi_2019(ref double[] mu_t, TypeTask typeTask,
            IMWRiver wMesh, ECalkDynamicSpeed typeEddyViscosity, double[] Vx, double J = 0)
        {
            IMesh mesh = wMesh.GetMesh();
            double[] Hp = wMesh.GetHp();
            double[] Distance = wMesh.GetDistance();
            double sVx = Vx.Sum();
            double mHp = Hp.Max();
            double bHp = mHp / d50;
            double u_star = 0;

            if (MEM.Equals(sVx, 0) == true || 
                typeEddyViscosity == ECalkDynamicSpeed.u_start_J ||
                typeEddyViscosity == ECalkDynamicSpeed.u_start_M ||
                wMesh as IMWCrossSection == null)
            {
                if (typeEddyViscosity != ECalkDynamicSpeed.u_start_J && sVx > 0 ||
                    MEM.Equals(sVx, 0) != true)
                {
                    if (bHp > 1)
                        u_star = sVx * kappa_w / Math.Log(bHp);
                }
                else
                {
                    u_star = Math.Sqrt(GRAV * mHp);
                }
                for (int node = 0; node < mesh.CountKnots; node++)
                {
                    if (typeEddyViscosity == ECalkDynamicSpeed.u_start_J && J > MEM.Error7)
                    {
                        if (Hp[node] > MEM.Error4)
                            u_star = Math.Sqrt(GRAV * Hp[node] * J);
                        else
                            u_star = 0;
                    }
                    mu_t[node] = GetMuAbsi_2019(Hp[node], Distance[node], u_star);
                }
                //if (typeEddyViscosity != ECalkDynamicSpeed.u_start_J && sVx > 0)
                //{
                //    if (mHp > 1)
                //        u_star = Vx.Max() * kappa_w / Math.Log(mHp);
                //}
                //for (int node = 0; node < mesh.CountKnots; node++)
                //{
                //    if (typeEddyViscosity == ECalkDynamicSpeed.u_start_J && J > MEM.Error7 && Hp[node] > MEM.Error4)
                //    {
                //            u_star = Math.Sqrt(GRAV * Hp[node] * J);
                //            mu_t[node] = GetMuAbsi_2019(Hp[node], Distance[node], u_star);
                //    }
                //    else
                //        mu_t[node] = mu;

                //}
            }
            else
            {
                double[] Us = null;
                ((IMWCrossSection)wMesh).CalkBoundary_U_star(Vx, ref Us);
                for (int node = 0; node < mesh.CountKnots; node++)
                    mu_t[node] = GetMuAbsi_2019(Hp[node], Distance[node], Us[node]);
            }
        }
        public double GetMuAbsi_2019(double H, double D, double u_star)
        {
            double mu_t0 = 0;
            if (u_star > MEM.Error5)
            {
                double xi = D / H;
                //xi = Math.Min(0.5, xi);
                if (xi > MEM.Error5)
                {
                    double Bf = 6;
                    double Re_star = H * u_star / nu;
                    double C1 = 1000;
                    double Z = 0.46 * Re_star - 5.98;
                    if (Z > MEM.Error2)
                    {
                        C1 = Re_star / Z;
                        double Ca = Math.Exp(-(0.34 * Re_star - 11.5) / Z);
                        if (H > MEM.Error4)
                        {
                            mu_t0 = rho_w * u_star * D * Ca * Math.Exp(-C1 * xi) * (1 - Math.Exp(-Bf * Math.Max(0, 1 - xi)));
                        }
                    }
                }
            }
            return  mu_t0 + mu;
        }
        /// <summary>
        /// Профиль турбулентной вязкости ванн Дрист 1956
        /// </summary>
        public void calkTurbVisc_VanDriest1956(ref double[] mu_t, TypeTask typeTask,
            IMWRiver wMesh, ECalkDynamicSpeed typeEddyViscosity, double[] Vx, double J = 0)
        {
            double A_vd = 26;
            IMesh mesh = wMesh.GetMesh();
            double[] Hp = wMesh.GetHp();
            double[] Distance = wMesh.GetDistance();

            IMWCrossSection wm = (IMWCrossSection)wMesh;
            if (Vx.Sum() == 0 || typeEddyViscosity == ECalkDynamicSpeed.u_start_J ||
                                 typeEddyViscosity == ECalkDynamicSpeed.u_start_M)
            {
                double u_star = 0;
                if (typeEddyViscosity != ECalkDynamicSpeed.u_start_J)
                    u_star = Vx.Max() * kappa_w / Math.Log(Hp.Max() / d50);

                for (int node = 0; node < mesh.CountKnots; node++)
                {
                    if (typeEddyViscosity == ECalkDynamicSpeed.u_start_J && J > MEM.Error7)
                    {
                        if (Hp[node] > MEM.Error4)
                            u_star = Math.Sqrt(GRAV * Hp[node] * J);
                        else
                            u_star = 0;
                    }
                    double z = Distance[node];
                    double zplus = u_star * z / nu;
                    double mu_t0 = rho_w * u_star * kappa_w * z * (1 - Math.Exp(-zplus / A_vd));
                    mu_t[node] = mu_t0 + mu;
                }
            }
            else
            {
                double[] Us = null;
                wm.CalkBoundary_U_star(Vx, ref Us);
                for (int node = 0; node < mesh.CountKnots; node++)
                {
                    double u_star = Us[node];
                    double z = Distance[node];
                    double zplus = u_star * z / nu;
                    double mu_t0 = rho_w * u_star * kappa_w * z * (1 - Math.Exp(-zplus / A_vd));
                    mu_t[node] = mu_t0 + mu;
                }
            }
        }


        /// <summary>
        /// Двухслойная модель GLS 1995 : А. В. Гарбарук, Ю. В. Лапин, М. X. Стрелец
        /// </summary>
        void calkTurbVisc_GLS_1995(ref double[] mu_t, TypeTask typeTask,
            IMWRiver wMesh, ECalkDynamicSpeed typeEddyViscosity, double[] Vx, double J = 0)
        {
            double z1 = d50;
            IMesh mesh = wMesh.GetMesh();
            double[] Hp = wMesh.GetHp();
            double[] Distance = wMesh.GetDistance();
            IMWCrossSection wm = (IMWCrossSection)wMesh;
            if (Vx.Sum() == 0 || typeEddyViscosity == ECalkDynamicSpeed.u_start_J ||
                                 typeEddyViscosity == ECalkDynamicSpeed.u_start_M)
            {
                double u_star = 0;
                if (typeEddyViscosity != ECalkDynamicSpeed.u_start_J)
                    u_star = Vx.Max() * kappa_w / Math.Log(Hp.Max() / d50);
                // шероховатость
                for (int node = 0; node < mesh.CountKnots; node++)
                {
                    if (typeEddyViscosity == ECalkDynamicSpeed.u_start_J && J > MEM.Error7)
                        u_star = Math.Sqrt(GRAV * Hp[node] * J);
                    double z = Distance[node];
                    double h = Hp[node];
                    mu_t[node] = GLS_1995(z, h, u_star, z1);
                }
            }
            else
            {
                double[] Us = null;
                wm.CalkBoundary_U_star(Vx, ref Us);
                for (int node = 0; node < mesh.CountKnots; node++)
                {
                    double u_star = Us[node];
                    double z = Distance[node];
                    double h = Hp[node];
                    mu_t[node] = GLS_1995(z, h, u_star, z1);
                }
            }
        }
        /// <summary>
        /// Двухслойная модель GLS 1995 : А. В. Гарбарук, Ю. В. Лапин, М. X. Стрелец
        /// </summary>
        /// <param name="z"></param>
        /// <param name="h"></param>
        /// <param name="u_star"></param>
        /// <param name="z1"></param>
        /// <returns></returns>
        double GLS_1995(double z, double h, double u_star, double z1)
        {
            double A_vd = 13;
            double xi = z / h;
            double xi3 = xi * xi * xi;
            double zplus = u_star * z / nu;
            // поправка типа ван Дриста
            double VD0 = (1 - Math.Exp(-zplus / A_vd));
            double VD = VD0 * VD0 * VD0;
            // модель турбулентнтной вязкости во внутренней области
            double nu_t_in = kappa * u_star * z * VD + nu;
            // толщина пограничного слоя
            double delta0 = -(2 * z1 * Math.Log((z1 + h) / z1) - 2 * z1 * Math.Log(2) + z1 - h) / Math.Log((z1 + h) / z1);
            // коэффициент перемежаемости Клебанова 
            double gamma0 = 1.0 / (1 + 5.5 * (xi3 * xi3));
            // модель турбулентнтной вязкости во внешней области
            double nu_t_out = kappa * u_star * delta0 * gamma0 + nu;
            // турбулентная вязкость
            double Mu_t = rho_w * Math.Min(nu_t_in, nu_t_out);
            return Mu_t;
        }

        /// <summary>
        /// Модель Смагоринского-Лилли 0.17 < Cs< 0.21 (Lilly, 1996)
        /// </summary>
        /// <param name="mu_t"></param>
        /// <param name="typeTask"></param>
        /// <param name="wMesh"></param>
        /// <param name="typeEddyViscosity"></param>
        /// <param name="Vx"></param>
        /// <param name="J"></param>
        public void calkTurbVisc_Smagorinsky_Lilly_1996(ref double[] mu_t, TypeTask typeTask,
            IMWRiver wMesh, ECalkDynamicSpeed typeEddyViscosity, double[] E2, double J = 0)
        {
            //double Csm = 0.2;
            //double Csm2 = Csm * Csm;
            double Csm2 = 0.04;
            IMWCrossSection wm = (IMWCrossSection)wMesh;
            IMesh mesh = wMesh.GetMesh();
            // площади Ко в окрестности узла
            double[] Se = wm.GetElemS();
            double K = 0.000125/Se.Max();
            Csm2 *= K;
            // шероховатость
            for (int node = 0; node < mesh.CountKnots; node++)
            {
                double mu_tn = rho_w * Csm2 * Se[node] * E2[node];
                mu_tn = Math.Max(mu_tn, 1e-4);
                mu_t[node] = mu_tn + mu;
            }
        }

        /// <summary>
        /// Модель Derek G.Goring, Jeremy M.Walsh, Peter Rutschmann & Jürg Trösch
        /// Модель Дерек Г.Горинг, Джереми М. Уолш, Питер Ратчманн и Юрг Треш 1997
        /// </summary>
        /// <param name = "mu_t" ></ param >
        /// < param name= "typeTask" ></ param >
        /// < param name= "wMesh" ></ param >
        /// < param name= "typeEddyViscosity" ></ param >
        /// < param name= "Vx" ></ param >
        /// < param name= "J" ></ param >
        public void calkTurbVisc_Derek_G_Goring_and_K_1997(ref double[] mu_t, TypeTask typeTask,
            IMWRiver wMesh, ECalkDynamicSpeed typeEddyViscosity, double[] E2, double J = 0)
        {
            IMesh mesh = wMesh.GetMesh();
            double[] Hp = wMesh.GetHp();
            double[] Distance = wMesh.GetDistance();
            // шероховатость
            for (int node = 0; node < mesh.CountKnots; node++)
            {
                double z = Distance[node];
                double lm2 = kappa_w * z * kappa_w * z;// * z * Math.Max(0, 1 - z / h);
                double mu_tn = rho_w * lm2 * E2[node];
                mu_t[node] = mu_tn + mu;
            }
        }
        /// <summary>
        /// Модель Потапова И И. 2024
        /// </summary>
        public void calkTurbVisc_PotapovII_2024(ref double[] mu_t, TypeTask typeTask,
            IMWRiver wMesh, ECalkDynamicSpeed typeEddyViscosity, double[] E2, double J = 0)
        {
            IMesh mesh = wMesh.GetMesh();
            double[] Distance = wMesh.GetDistance();
            // шероховатость
            for (int node = 0; node < mesh.CountKnots; node++)
            {
                double z = Distance[node];
                double lm2 = kappa_w * z * kappa_w * z;
                double mu_tn = rho_w * lm2 * E2[node];
                mu_t[node] = mu_tn + mu;
            }
        }

        #endregion

    }
}
