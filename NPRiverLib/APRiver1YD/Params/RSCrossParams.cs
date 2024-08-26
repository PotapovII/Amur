//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 12.12.2020 Потапов И.И.
//---------------------------------------------------------------------------
//               Реверс в NPRiverLib 09.02.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace NPRiverLib.APRiver1YD.Params
{
    using System;
    using System.IO;
    using System.ComponentModel;

    using MemLogLib;
    using CommonLib;
    using CommonLib.EConverter;
    using CommonLib.Physics;

    /// <summary>
    /// Тип решателя
    /// </summary>
    [Serializable]
    public enum CrossAlgebra
    {
        /// <summary>
        /// Ленточный не симметрияный метод Гаусса
        /// </summary>
        [Description("метод Гаусса - ленточный")]
        TapeGauss = 0,
        /// <summary>
        /// Метод би сопряженных градиентов
        /// </summary>
        [Description("метод би сопряженных градиентов")] 
        BeCGrad = 1
    }

    /// <summary>
    /// Тип задачи
    /// </summary>
    [Serializable]
    public enum CrossTypeTask
    {
        /// <summary>
        /// Ленточный не симметрияный метод Гаусса
        /// </summary>
        [Description("плоская задача")]
        FlatProblem = 0,
        /// <summary>
        /// Метод би сопряженных градиентов
        /// </summary>
        [Description("цилиндрическая задача")]
        CylindricalProblem = 1
    }
    /// <summary>
    /// Тип геомерии створа
    /// </summary>
    [Serializable]
    public enum CrossFormGeometry
    {
        /// <summary>
        /// Трапециидальный створ
        /// </summary>
        [Description("трапециидальный")]
        Trapezoidal = 0,
        /// <summary>
        /// Параболический створ
        /// </summary>
        [Description("параболический")]
        Parabolic = 1
    }
    /// <summary>
    /// Тип граничныого условия для вихря
    /// </summary>
    [Serializable]
    public enum BCTypeVortex
    {
        /// <summary>
        /// вихрь на контуре равен нулю
        /// </summary>
        [Description("вихрь на контуре равен нулю")]
        VortexZero = 0,
        /// <summary>
        /// вихрь на поверхность потока равен нулю
        /// </summary>
        [Description("вихрь на поверхность потока равен нулю")]
        VortexZeroWL = 1,
        /// <summary>
        /// вихрь считаем на поверхности и дне потока 
        /// </summary>
        [Description("вихрь считаем на поверхности и дне потока")]
        VortexAllCalk = 2,
        /// <summary>
        /// вихрь на поверхность потока равен нулю
        /// расчет вихря на поверхность потока 
        /// </summary>
        [Description("вихрь на поверхность потока равен нулю, на дне нет")]
        VortexZeroBed = 3
    }

    /// <summary>
    ///  ОО: Параметры для класса RiverStreamTask 
    /// </summary>
    [Serializable]
    public class RSCrossParams : ITProperty<RSCrossParams>
    {
        /// <summary>
        /// уклон свободной поверхности потока
        /// </summary>
        [DisplayName("Уклон свободной поверхности")]
        [Category("Задача")]
        public double J { get; set; }
        /// <summary>
        /// Средний радиус
        /// </summary>
        [DisplayName("Средний радиус закругления канала")]
        [Category("Задача")]
        public double midleRadius { get; set; }
        /// <summary>
        /// модель турбулентной вязкости
        /// </summary>
        [DisplayName("Y модель турбулентной вязкости")]
        [Category("Задача")]
        [TypeConverter(typeof(MyEnumConverter))]
        public ETurbViscType turbViscTypeA { get; set; }
        /// <summary>
        /// модель турбулентной вязкости
        /// </summary>
        [DisplayName("X модель турбулентной вязкости")]
        [Category("Задача")]
        [TypeConverter(typeof(MyEnumConverter))]
        public ETurbViscType turbViscTypeB { get; set; }
        /// <summary>
        /// метод решения алгебраической системе
        /// </summary>
        [DisplayName("метод решения алгебраической системе")]
        [Category("Алгоритм")]
        [TypeConverter(typeof(MyEnumConverter))]
        public CrossAlgebra сrossAlgebra { get; set; }
        /// <summary>
        /// Вариант задачи: задан расход/свободная поверхность
        /// </summary>
        [DisplayName("Вариант задачи")]
        [Category("Задача")]
        [TypeConverter(typeof(MyEnumConverter))]
        public TaskVariant taskVariant { get; set; }
        /// <summary>
        /// Тип геомерии створа
        /// </summary>
        [DisplayName("Тип геомерии створа")]
        [Category("Задача")]
        [TypeConverter(typeof(MyEnumConverter))]
        public CrossFormGeometry crossFormGeometry { get; set; }
        /// <summary>
        /// Тип граничныого условия для вихря
        /// </summary>
        [DisplayName("Тип граничныого условия для вихря")]
        [Category("Задача")]
        [TypeConverter(typeof(MyEnumConverter))]
        public BCTypeVortex bcTypeVortex { get; set; }
        /// <summary>
        /// Наличие оси симметрии канала
        /// </summary>
        [DisplayName("Ось симметрии 1 стенка 0")]
        [Category("Задача")]
        public int axisSymmetry { get; set; }
        /// <summary>
        /// количество узлов по дну реки
        /// </summary>
        [DisplayName("количество узлов по смоченному периметру")]
        [Category("Сетка")]
        public int CountKnots { get; set; }
        /// <summary>
        /// количество узлов в области
        /// </summary>
        [DisplayName("количество узлов по контуру дна")]
        [Category("Сетка")]
        public int CountBLKnots { get; set; }
        /// <summary>
        /// Номер створа в вариантных задачах
        /// </summary>
        [DisplayName("Номер створа в вариантных задачах")]
        [Category("Задача")]
        public int crossSectionNamber { get; set; }
        /// <summary>
        /// количество узлов в области
        /// </summary>
        [DisplayName("Тип Задачи, 0 плоская; 1 изгиб русла")]
        [Category("Задача")]
        [TypeConverter(typeof(MyEnumConverter))]
        public CrossTypeTask crossSectionType { get; set; }
        /// <summary>
        /// Скорость Ux на поверхности потока задана
        /// </summary>
        [DisplayName("Скорость Ux на поверхности потока задана")]
        [Category("Задача")]
        [TypeConverter(typeof(BooleanTypeConverterYN))]
        public bool velocityOnWL { get; set; }
        /// <summary>
        /// Скорость Ux на поверхности потока задана
        /// </summary>
        [DisplayName("Метод расчета динамической скорости на стенках канала")]
        [Category("Задача")]
        [TypeConverter(typeof(BooleanTypeConverterYN))]
        public ECalkDynamicSpeed typeEddyViscosity { get; set; }
        public RSCrossParams()
        {
            SetDef();
        }
        protected void SetDef()
        {
            J = 0.001;
            turbViscTypeA = ETurbViscType.Leo_C_van_Rijn1984;
            turbViscTypeB = ETurbViscType.Leo_C_van_Rijn1984;
            сrossAlgebra = CrossAlgebra.TapeGauss;
            taskVariant = TaskVariant.WaterLevelFun;
            bcTypeVortex = BCTypeVortex.VortexAllCalk;
            crossFormGeometry = CrossFormGeometry.Trapezoidal;
            typeEddyViscosity = ECalkDynamicSpeed.u_start_U;
            velocityOnWL = true;
            axisSymmetry = 0;
            CountKnots = 400;
            CountBLKnots = 600;
            crossSectionNamber = 0;
            crossSectionType = CrossTypeTask.CylindricalProblem;
            midleRadius = 5.0;
        }

        public RSCrossParams(RSCrossParams p)
        {
            Set(p);
        }
        public virtual void Set(RSCrossParams p)
        {
            J = p.J;
            CountKnots = p.CountKnots;
            CountBLKnots = p.CountBLKnots;
            axisSymmetry = p.axisSymmetry;
            taskVariant = p.taskVariant;
            turbViscTypeA = p.turbViscTypeA;
            turbViscTypeB = p.turbViscTypeB;
            сrossAlgebra = p.сrossAlgebra;
            crossSectionNamber = p.crossSectionNamber;
            crossSectionType = p.crossSectionType;
            midleRadius = p.midleRadius;
            bcTypeVortex = p.bcTypeVortex;
            velocityOnWL = p.velocityOnWL;
            crossFormGeometry = p.crossFormGeometry;
            typeEddyViscosity = p.typeEddyViscosity;
        }

        /// <summary>
        /// свойств задачи
        /// </summary>
        /// <param name="p"></param>
        public virtual object GetParams() { return this; }
        /// <summary>
        /// Чтение параметров задачи из файла
        /// </summary>
        /// <param name="file"></param>
        public virtual void Load(StreamReader file)
        {
            try
            {
                J = LOG.GetDouble(file.ReadLine());
                turbViscTypeA = (ETurbViscType)LOG.GetInt(file.ReadLine());
                turbViscTypeB = (ETurbViscType)LOG.GetInt(file.ReadLine());
                typeEddyViscosity = (ECalkDynamicSpeed)LOG.GetInt(file.ReadLine());
                сrossAlgebra = (CrossAlgebra)LOG.GetInt(file.ReadLine());
                taskVariant = (TaskVariant)LOG.GetInt(file.ReadLine());
                bcTypeVortex = (BCTypeVortex)LOG.GetDouble(file.ReadLine());
                crossFormGeometry = (CrossFormGeometry)LOG.GetInt(file.ReadLine());
                axisSymmetry = LOG.GetInt(file.ReadLine());
                CountKnots = LOG.GetInt(file.ReadLine());
                CountBLKnots = LOG.GetInt(file.ReadLine());
                crossSectionNamber = LOG.GetInt(file.ReadLine());
                crossSectionType = (CrossTypeTask)LOG.GetInt(file.ReadLine());
                midleRadius = LOG.GetDouble(file.ReadLine());
                velocityOnWL = LOG.GetBool(file.ReadLine());
                
            }
            catch(Exception)
            {
                Logger.Instance.Info("Файл параметров не синхронизирован, использованы параметры по умолчанию");
                SetDef();
            }
        }
        /// <summary>
        /// Запись
        /// </summary>
        /// <param name="file"></param>
        public virtual void Save(StreamReader file)
        {
        }
        public virtual void LoadParams(string fileName)
        {
            string message = "Файл парамеров задачи - доные деформации - не обнаружен";
            WR.LoadParams(Load, message, fileName);
        }
        public RSCrossParams Clone(RSCrossParams p)
        {
            return new RSCrossParams(p);
        }
        public void SetParams(object p)
        {
            Set((RSCrossParams)p);
        }
    }
}
