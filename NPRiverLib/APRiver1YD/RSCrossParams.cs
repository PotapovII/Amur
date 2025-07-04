﻿//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 12.12.2020 Потапов И.И.
//---------------------------------------------------------------------------
//               Реверс в NPRiverLib 09.02.2024 Потапов И.И.
//---------------------------------------------------------------------------
//                ++ кодировка :  03.02.2025 Потапов И.И.
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
    using CommonLib.EddyViscosity;
    using MeshGeneratorsLib.StripGenerator;
    using CommonLib.BedLoad;

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
    /// Тип решателя
    /// </summary>
    [Serializable]
    public enum BCWLVelocity
    {
        /// <summary>
        /// Скорости на свободной поверхности не заданны
        /// </summary>
        [Description("Скорости на свободной поверхности не заданны")]
        NoWLVelocity = 0,
        /// <summary>
        /// На свободной поверхности заданны 
        /// скорости Ux и Uy
        /// </summary>
        [Description("Скорости свободной поверхности заданны")]
        AllVelocity = 1,
        /// <summary>
        /// На свободной поверхности заданна только 
        /// скорость Ux (окружная)
        /// </summary>
        [Description("На свободной поверхности заданна только скорость Ux (окружная)")]
        onlyVelocityUx = 2,
        /// <summary>
        /// На свободной поверхности заданна только 
        /// скорость Uy (радиальная)
        /// </summary>
        [Description("На свободной поверхности заданна только скорость Uy (радиальная)")]
        onlyVelocityUy = 3
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
        /// Постоянная вихревая вязкость
        /// </summary>
        [DisplayName("Постоянная вихревая вязкость")]
        [Category("Задача")]
        public double mu_const { get; set; }
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
        ///// <summary>
        ///// Тип граничныого условия для вихря
        ///// </summary>
        //[DisplayName("Тип граничныого условия для вихря")]
        //[Category("Задача")]
        //[TypeConverter(typeof(MyEnumConverter))]
        //public BCTypeVortex bcTypeVortex { get; set; }
        /// <summary>
        /// Скорость Ux на поверхности потока задана
        /// </summary>
        [DisplayName("Определение cкорость Ux и Vy на поверхности потока")]
        [Category("Задача")]
        [TypeConverter(typeof(MyEnumConverter))]
        public BCWLVelocity velocityOnWL { get; set; }
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
        /// Тип ленточного герератора КЭ сетки для створовых задач
        /// </summary>
        [DisplayName("тип ленточного герератора КЭ сетки для створовых задач")]
        [Category("Сетка")]
        [TypeConverter(typeof(MyEnumConverter))]
        public StripGenMeshType typeMeshGenerator { get; set; }

        /// <summary>
        /// Скорость Ux на поверхности потока задана
        /// </summary>
        [DisplayName("Метод расчета динамической скорости на стенках канала")]
        [Category("Задача")]
        [TypeConverter(typeof(MyEnumConverter))]
        public ECalkDynamicSpeed typeEddyViscosity { get; set; }

        #region + 03 02 2025 ОО: Параметры для класса TriSecRiver_1YD 
        /// <summary>
        /// Количество итераций по нелинейности на текущем шаге по времени
        /// </summary>
        [DisplayName("Количество итераций по нелинейности на текущем шаге по времени")]
        [Category("Алгоритм")]
        public int NLine { get; set; }
        /// <summary>
        /// тип задачи 0 - плоская 1 - цилиндрическая
        /// </summary>
        [DisplayName("тип задачи 0 - плоская 1 - цилиндрическая")]
        [Category("Задача")]
        public int SigmaTask { get; set; }
        /// <summary>
        /// начальынй радиус закругления канала по выпуклому берегу
        /// </summary>
        [DisplayName("начальынй радиус закругления канала по выпуклому берегу")]
        [Category("Задача")]
        public double RadiusMin { get; set; }
        /// <summary>
        /// Тип гидродинамики 0 - нестационарные 1 - стационарные у-я Рейнольдса 2 - Стокс
        /// нестационар/стационар
        /// </summary>
        [DisplayName("Тип гидродинамики 0 - нестационарные 1 - стационарные у-я Рейнольдса 2 - Стокс")]
        [Category("Задача")]
        public int ReTask { get; set; }
        
        [DisplayName("Параметр неявности схемы при шаге по времени")]
        [Category("Задача")]
        public double theta { get; set; }
        #endregion

        #region + 19 03 2025 ОО: Параметры для обратного контроля файла задачи 
        /// <summary>
        /// + 19 03 2025 ОО: Параметры для обратного контроля 
        /// файла задачи в окне создания редактирования файла
        /// </summary>
        [DisplayName("Начальная дата расчета")]
        [Category("Задача")]
        public string DataStart { get; set; } = "01.01.2000";
        [DisplayName("Конечная дата расчета")]
        [Category("Задача")]
        public string DataEnd { get; set; } = "01.01.2000";
        #endregion

        #region + 01 05 2025 ОО: Флаг для расчета задачи взвешенных наносов
        [TypeConverter(typeof(MyEnumConverter))]
        [DisplayName("Расчет взвешенных наносов")]
        [Description("Флаг для расчета взвешенных наносов Да - true, Нет -false")]
        [Category("Задача")]
        public BCalkConcentration CalkConcentration { get; set; } = BCalkConcentration.NeCalkConcentration;
        #endregion
        /// <summary>
        /// Нужен для менеджера задач / настройки по умолчанию
        /// </summary>
        public RSCrossParams()
        {
            SetDef();
        }
        /// <summary>
        /// Нужен определения настроек тестовых задач
        /// </summary>
        public RSCrossParams(double J, double RadiusMin, double theta, int axisSymmetry, int CountKnots, 
            int CountBLKnots, BCWLVelocity velocityOnWL, int NLine, int SigmaTask, int ReTask, 
            ECalkDynamicSpeed typeEddyViscosity, ETurbViscType turbViscTypeA,
            ETurbViscType turbViscTypeB, CrossAlgebra сrossAlgebra, 
            TaskVariant taskVariant, BCTypeVortex bcTypeVortex, StripGenMeshType typeMeshGenerator,double  mu_const,
            string DataStart = "", string DataEnd="", 
            BCalkConcentration CalkConcentration = BCalkConcentration.NotCalkConcentration)
        {
            this.J = J;
            this.mu_const = mu_const;
            this.RadiusMin = RadiusMin;
            this.theta = theta;
            this.axisSymmetry = axisSymmetry;
            this.CountKnots = CountKnots;
            this.CountBLKnots = CountBLKnots;
            this.velocityOnWL = velocityOnWL;
            this.NLine = NLine;
            this.SigmaTask = SigmaTask;
            this.ReTask = ReTask;
            this.typeEddyViscosity = typeEddyViscosity;
            this.turbViscTypeA = turbViscTypeA;
            this.turbViscTypeB = turbViscTypeB;
            this.сrossAlgebra = сrossAlgebra;
            this.taskVariant = taskVariant;
            //this.bcTypeVortex = bcTypeVortex;
            this.typeMeshGenerator = typeMeshGenerator; 
            this.DataStart = DataStart;
            this.DataEnd = DataEnd;
            this.CalkConcentration = CalkConcentration;
        }
        protected virtual void SetDef()
        {
            J = 0.001;
            mu_const = 1;
            turbViscTypeA = ETurbViscType.Leo_C_van_Rijn1984;
            turbViscTypeB = ETurbViscType.Leo_C_van_Rijn1984;
            сrossAlgebra = CrossAlgebra.TapeGauss;
            taskVariant = TaskVariant.WaterLevelFun;
            //bcTypeVortex = BCTypeVortex.VortexAllCalk;
            typeEddyViscosity = ECalkDynamicSpeed.u_start_U;
            typeMeshGenerator = StripGenMeshType.StripMeshGenerator_3;
            velocityOnWL = BCWLVelocity.AllVelocity;
            axisSymmetry = 0;
            CountKnots = 400;
            CountBLKnots = 600;
            NLine = 100;
            SigmaTask = 0;
            RadiusMin = 100.0;
            ReTask = 0;
            theta = 0.5;
            CalkConcentration = BCalkConcentration.NotCalkConcentration;
        }
        /// <summary>
        /// Нужен для копирования свойств с интерфейса
        /// </summary>
        /// <param name="p"></param>
        public RSCrossParams(RSCrossParams p)
        {
            J = p.J;
            mu_const = p.mu_const;
            CountKnots = p.CountKnots;
            CountBLKnots = p.CountBLKnots;
            axisSymmetry = p.axisSymmetry;
            taskVariant = p.taskVariant;
            turbViscTypeA = p.turbViscTypeA;
            turbViscTypeB = p.turbViscTypeB;
            сrossAlgebra = p.сrossAlgebra;
           // bcTypeVortex = p.bcTypeVortex;
            velocityOnWL = p.velocityOnWL;
            typeEddyViscosity = p.typeEddyViscosity;
            typeMeshGenerator = p.typeMeshGenerator;
            // + 03 02 2025
            NLine = p.NLine;
            SigmaTask = p.SigmaTask;
            RadiusMin = p.RadiusMin;
            ReTask = p.ReTask;
            theta = p.theta;
            DataStart = p.DataStart;
            DataEnd = p.DataEnd;
            // + 01 05 2025
            CalkConcentration = p.CalkConcentration;
        }
        /// <summary>
        /// Нужен для чтение параметров задачи из файла
        /// </summary>
        /// <param name="file"></param>
        public virtual void Load(StreamReader file)
        {
            string ver = "";
            try
            {
                ver = file.ReadLine();
                J = LOG.GetDouble(file.ReadLine());
                turbViscTypeA = (ETurbViscType)LOG.GetInt(file.ReadLine());
                turbViscTypeB = (ETurbViscType)LOG.GetInt(file.ReadLine());
                typeEddyViscosity = (ECalkDynamicSpeed)LOG.GetInt(file.ReadLine());
                сrossAlgebra = (CrossAlgebra)LOG.GetInt(file.ReadLine());
                taskVariant = (TaskVariant)LOG.GetInt(file.ReadLine());
                // для совместимости - 23 04 2025
                LOG.GetInt(file.ReadLine());
                typeMeshGenerator =(StripGenMeshType)LOG.GetInt(file.ReadLine());
                axisSymmetry = LOG.GetInt(file.ReadLine());
                CountKnots = LOG.GetInt(file.ReadLine());
                CountBLKnots = LOG.GetInt(file.ReadLine());
                velocityOnWL = (BCWLVelocity)LOG.GetInt(file.ReadLine());
                // + 03 02 2025
                NLine = LOG.GetInt(file.ReadLine());
                SigmaTask = LOG.GetInt(file.ReadLine());
                ReTask = LOG.GetInt(file.ReadLine());
                RadiusMin = LOG.GetDouble(file.ReadLine());
                theta = LOG.GetDouble(file.ReadLine());
                // +  25 02 2025
                mu_const = LOG.GetDouble(file.ReadLine());
                DataStart = LOG.GetString(file.ReadLine());
                DataEnd = LOG.GetString(file.ReadLine());
                // + 01 05 2025
                CalkConcentration = (BCalkConcentration) LOG.GetInt(file.ReadLine());
            }
            catch (Exception)
            {
                Logger.Instance.Info("Файл параметров не синхронизирован, использована версия "+ver);
                SetDef();
            }
        }

 
        /// <summary>
        /// Нужен для чтение параметров задачи из файла
        /// </summary>
        /// <param name="file"></param>
        public virtual void SaveA(StreamWriter file)
        {
            string ver = "";
            try
            {
                file.WriteLine("Версия 0.1\t\t\tфайла данных для створовой задачи");
                file.WriteLine("J {0}             : уклон свободной поверхности", J);
                file.WriteLine("turbViscTypeA {0}     : тип окружной турбулентной модели",(int)turbViscTypeA);
                file.WriteLine("turbViscTypeB {0}     : тип турбулентной модели в створе", (int)turbViscTypeB);
                file.WriteLine("typeEddyViscosity {0} : метод расчета динамической скорости на стенках канала", (int)typeEddyViscosity);
                file.WriteLine("сrossAlgebra {0}      : алгебра задачи", (int)сrossAlgebra);
                file.WriteLine("taskVariant {0}       : вариант задачи", (int)taskVariant);
                file.WriteLine("bcTypeVortex 2      : тип граничного условия на свободной поверхности створа"); // для совместимости
                file.WriteLine("typeMeshGenerator {0} : тип ленточного герератора КЭ сетки для створовых задач", (int)typeMeshGenerator);
                file.WriteLine("axisSymmetry {0}      : ось симметрии 1 стенка 0", axisSymmetry);
                file.WriteLine("CountKnots {0}      : количество узлов по смоченному периметру", CountKnots);
                file.WriteLine("CountBLKnots {0}    : количество узлов по контуру дна", CountBLKnots);
                file.WriteLine("velocityOnWL {0}    : cкорости Ux, Uy на поверхности потока 0 - не задана, 1 - заданна, 2 - только Ux, 3 - только Uy,", (int)velocityOnWL);
                // + 03 02 2025
                file.WriteLine("NLine {0}           : количество итераций по нелинейности на текущем шаге по времени", NLine);
                file.WriteLine("SigmaTask {0}         : тип задачи 0 - плоская 1 - цилиндрическая", SigmaTask);
                file.WriteLine("ReTask {0}            : тип решаемых уравнений Рейнольдса НС 0, Рейнольдса 1, Стокса 2, Только Ux <2", ReTask);
                file.WriteLine("RadiusMin {0}       : радиус закругления канала по выпуклому берегу", RadiusMin);
                file.WriteLine("theta {0}           : параметр неявности схемы при шаге по времени", theta);
                // +  25 02 2025
                file.WriteLine("mu_const {0}           : постоянная вихревая вязкость", mu_const);
                // +  19 03 2025
                file.WriteLine("DataStart " + DataStart);
                file.WriteLine("DataEnd " + DataEnd);
                // + 01 05 2025
                file.WriteLine("CalkConcentration {0}           : Флаг для расчета взвешенных наносов Да -true, Нет - false", (int)CalkConcentration);
            }
            catch (Exception)
            {
                Logger.Instance.Info("Файл параметров не синхронизирован, использована версия " + ver);
                SetDef();
            }
        }

        /// <summary>
        /// Нужен для работы конструктора копирования при работе с интерфецсом шаблона свойств
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public RSCrossParams Clone(RSCrossParams p)
        {
            return new RSCrossParams(p);
        }
    }
}
