//---------------------------------------------------------------------------
//                  Разработка кода : Снигур К.С.
//                         релиз 26 06 2017 
//---------------------------------------------------------------------------
//         интеграция в RiverLib 13 08 2022 : Потапов И.И.
//---------------------------------------------------------------------------
//                      Сбока No Property River Lib
//              с убранными из наследников Property классами 
//                              23 08 2024
//---------------------------------------------------------------------------
namespace NPRiverLib.APRiver1XD.KGD_River2D
{
    using System;
    using System.IO;
    using System.ComponentModel;

    using MemLogLib;
    using CommonLib.EConverter;
    using CommonLib;

    [Serializable]
    public class RGDParameters1XD : ITProperty<RGDParameters1XD>
    {
        #region  Параметры русла
        /// <summary>
        /// Расход жидкости на входе
        /// </summary>
        [DisplayName("Расход потока")] 
        [Description("Расход потока, м2/с")]
        [Category("Параметры русла")]
        public double Q { get; set; }
        /// <summary>
        /// Средний уклон русла
        /// </summary>
        [DisplayName("Средний уклон русла")] 
        [Description("Средний уклон русла")]
        [Category("Параметры русла")]
        public double J { get; set; }
        /// <summary>
        /// число Рейнольдса
        /// </summary>
        [DisplayName("Число Рейнольдса")]
        [Description("Число Рейнольдса")]
        [Category("Параметры русла")]
        public double Re { get; set; }

        #endregion  
        #region  Параметры КГД метода
        /// <summary>
        /// штраф неразрывности в давлении
        /// </summary>
        [DisplayName("alpha_n")] 
        [Description("Релаксация неразрывности")]
        [Category("Параметры КГД метода")]
        public double alpha_n { get; set; }
        /// <summary>
        /// штраф регуляризационной части в давлении
        /// </summary>
        [DisplayName("alpha_r")]
        [Description("Релаксация регуляризации")]
        [Category("Параметры КГД метода")]
        public double alpha_r { get; set; }
        /// <summary>
        /// Параметр регуляризации
        /// </summary>
        [DisplayName("tau")]
        [Description("Параметр регуляризации, с")]
        [Category("Параметры КГД метода")]
        public double tau { get; set; }

        [DisplayName("HydroIteration")]
        [Description("Количество внутренних итераций по гидродинамики")]
        [Category("Прочее")]
        public int iter
        {
            get 
            { 
                return Convert.ToInt32(time_b / dt_local); 
            }
        }
        /// <summary>
        /// Временной шаг для расчета гидродинамки
        /// </summary>
        [DisplayName("dt_local")]
        [Description("Локальный шаг по времени, с")]
        [Category("Алгоритм")]
        public double dt_local { get; set; }
        /// <summary>
        /// Релаксация давления
        /// </summary>
        [DisplayName("Relaxation P")] 
        [Description("Релаксация давления")]
        [Category("Алгоритм")]
        public double relaxP { get; set; }
        /// <summary>
        /// Максимальная допустимая погрешность по давлению
        /// </summary>
        [DisplayName("ErrorP")]
        [Description("Погрешность по давлению")]
        [Category("Алгоритм")]
        public double errP { get; set; }
        /// <summary>
        /// Свободная поверхность (есть или нет)
        /// </summary>
        
        [DisplayName("Свободная поверхность")] 
        [Description("Свободная поверхность (есть или нет)")]
        [Category("Алгоритм")]
        [TypeConverter(typeof(BooleanTypeConverterYN))]
        public bool surf_flag { get; set; }


        [DisplayName("delta")]
        [Description("Коэффициент поступления кинетической энергии через границу выхода")]
        [Category("Алгоритм")]
        public double delta { get; set; }

        /// <summary>
        /// Метод вычисления функции стенки
        /// </summary>
        [DisplayName("Функция стенки")]
        [Description("Метод вычисления функции стенки")]
        [Category("Управление моделью")]
        [TypeConverter(typeof(MyEnumConverter))]
        public WallFunctionType wallFunctionType { get; set; }
        /// <summary>
        /// Тип турбулентной модели
        /// </summary>
        [DisplayName("Тип турбулентной модели")]
        [Description("Тип турбулентной модели")]
        [Category("Управление моделью")]
        [TypeConverter(typeof(MyEnumConverter))]
        public TurbulentModelType turbulentModelType { get; set; }
        /// <summary>
        /// Метод вычисления функции стенки
        /// </summary>
        [DisplayName("Тип вычислений")]
        [Description("Тип вычислений")]
        [Category("Управление моделью")]
        [TypeConverter(typeof(MyEnumConverter))]
        public CheckMultiThreading calculationType { get; set; }


        [DisplayName("Решатель"), Description("Решатель")]
        [Category("Управление моделью")]
        [TypeConverter(typeof(MyEnumConverter))]
        public TypePU typePU { get; set; }


        [DisplayName("Вид профиля скорости"), 
        Description("Вид профиля скорости на входе в расчетную область")]
        [Category("Управление моделью")]
        [TypeConverter(typeof(MyEnumConverter))]
        public VelocityInProfile velocityInProfile { get; set; }
        /// <summary>
        /// метод нахождения размерного касательного напряжения на дне
        /// </summary>
        [DisplayName("Method calk Tau")] 
        [Description("Метод расчета напряжения на дне")]
        [Category("Управление моделью")]
        [TypeConverter(typeof(MyEnumConverter))]
        public ModeTauCalklType IndexMethod { get; set; }


        /// <summary>
        /// Время размыва дна = время расчета подзадачи гидродинамики
        /// </summary>
        public double time_b;

        #endregion

        public RGDParameters1XD()
        {
            Re = 723.5f;
            alpha_n = 0.005f;
            alpha_r = 0.005f;
            dt_local = 0.001f;
            time_b = 0.25f;
            tau = 0.0001f;
            Q = 0.0369f;
            relaxP = 1;
            errP = 0.001f;
            surf_flag = false;
            IndexMethod =  ModeTauCalklType.method2;
            wallFunctionType = WallFunctionType.smoothWall_Volkov;
            turbulentModelType = TurbulentModelType.Model_k_w;
            delta = 0.05;
            typePU = TypePU.Cpu;
            J = 0.00132;
            velocityInProfile = VelocityInProfile.PorabolicProfile;
        }
        public RGDParameters1XD(RGDParameters1XD p)
        {
            SetParams(p);
        }
        public void SetParams(RGDParameters1XD p)
        {
            if (p == null) p = new RGDParameters1XD();
            Re = p.Re;
            alpha_n = p.alpha_n;
            alpha_r = p.alpha_r;
            dt_local = p.dt_local;
            time_b = p.time_b;
            tau = p.tau;
            Q = p.Q;
            relaxP = p.relaxP;
            errP = p.errP;
            surf_flag = p.surf_flag;
            IndexMethod = p.IndexMethod;
            delta = p.delta;
            typePU = p.typePU;
            J = p.J;
            wallFunctionType = p.wallFunctionType;
            turbulentModelType = p.turbulentModelType;
            calculationType = p.calculationType;
            velocityInProfile = p.velocityInProfile;
        }
        /// <summary>
        /// свойств задачи
        /// </summary>
        /// <param name="p"></param>
        public object GetParams() { return this; }
        /// <summary>
        /// Установка свойств задачи
        /// </summary>
        /// <param name="p"></param>
        public void SetParams(object p)
        {
            SetParams((RGDParameters1XD)p);
        }
        /// <summary>
        /// Чтение параметров задачи из файла
        /// </summary>
        /// <param name="file"></param>
        public void LoadParams(string fileName = "")
        {
            string message = "Файл парамеров задачи - доные деформации - не обнаружен";
            WR.LoadParams(Load, message, fileName);
        }
        /// <summary>
        /// Чтение параметров задачи из файла
        /// </summary>
        /// <param name="file"></param>
        public virtual void Load(StreamReader file)
        {
            try
            {
                Re = LOG.GetDouble(file.ReadLine());
                alpha_n = LOG.GetDouble(file.ReadLine());
                alpha_r = LOG.GetDouble(file.ReadLine());
                dt_local = LOG.GetDouble(file.ReadLine());
                time_b = LOG.GetDouble(file.ReadLine());
                tau = LOG.GetDouble(file.ReadLine());
                Q = LOG.GetDouble(file.ReadLine());
                relaxP = LOG.GetDouble(file.ReadLine());
                errP = LOG.GetDouble(file.ReadLine());
                delta = LOG.GetDouble(file.ReadLine());
                J = LOG.GetDouble(file.ReadLine());
                IndexMethod =  (ModeTauCalklType)LOG.GetInt(file.ReadLine());
                surf_flag = LOG.GetBool(file.ReadLine());
                typePU = (TypePU)LOG.GetInt(file.ReadLine());
                wallFunctionType = (WallFunctionType)LOG.GetInt(file.ReadLine());
                turbulentModelType = (TurbulentModelType)LOG.GetInt(file.ReadLine());
                calculationType = (CheckMultiThreading)LOG.GetInt(file.ReadLine());
                velocityInProfile = (VelocityInProfile)LOG.GetInt(file.ReadLine());
            }
            catch (Exception ee)
            {
                Logger.Instance.Exception(ee);
                Logger.Instance.Info("Не согласованность версии программы и файла данных");
                Logger.Instance.Info("Использованы параметры по умолчанию");
            }
            // Пересчет зависимых параметров
            InitParams();
        }
        public virtual void InitParams() { }
        public RGDParameters1XD Clone(RGDParameters1XD p)
        {
            return new RGDParameters1XD(p);
        }
    }
}
