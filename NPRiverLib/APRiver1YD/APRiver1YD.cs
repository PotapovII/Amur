//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 09.02.2024 Потапов И.И.
//---------------------------------------------------------------------------
//                      Сбока No Property River Lib
//              с убранными из наследников Property классами 
//---------------------------------------------------------------------------
namespace NPRiverLib.APRiver1YD
{
    using MeshLib;
    using MemLogLib;
    using GeometryLib;

    using CommonLib;
    using CommonLib.Physics;
    using CommonLib.Function;
    using CommonLib.ChannelProcess;

    using NPRiverLib.ATask;
    using NPRiverLib.APRiver1YD.Params;

    using System;
    using System.IO;
    using System.Collections.Generic;
    /// <summary>
    ///             Базовый тип для створовых русловых задач
    ///  ОО: Определение класса  APRiver1YD - расчет полей скорости, вязкости 
    ///           и напряжений в живом сечении потока (створе)
    /// </summary>    
    /// <typeparam name="TParam">параметры задачи</typeparam>
    [Serializable]
    public abstract class APRiver1YD<TParam> : 
        APRiver<TParam> where TParam : class, ITProperty<TParam>
    {
        #region Физические параметры
        /// <summary>
        /// Турбулентная вязкость
        /// </summary>
        protected double eddyViscosityConst = 1.1;
        /// <summary>
        /// кинематическая вязкость воды
        /// </summary>
        protected double mu = SPhysics.mu;
        /// <summary>
        /// коэффициент Кармана
        /// </summary>
        protected double kappa = SPhysics.kappa_w;
        /// <summary>
        /// коэффициент Прандтля для вязкости
        /// </summary>
        protected double sigma = 1;
        #endregion

        #region Искомые поля створа без вторичных потоков
        /// <summary>
        /// Поле скорости
        /// </summary>
        public double[] Ux;
        /// <summary>
        /// Поле вязкости текущее и с предыдущей итерации
        /// </summary>
        public double[] eddyViscosity, eddyViscosity0;
        /// <summary>
        /// Поле напряжений T_xz
        /// </summary>
        public double[] TauZ;
        /// <summary>
        /// Поле напряжений dy
        /// </summary>
        public double[] TauY;
        /// <summary>
        /// Поле напряжений - модуль
        /// </summary>
        public double[] tau;
        #endregion

        #region Локальные переменные
        /// <summary>
        /// координаты дна по оси х
        /// </summary>
        protected double[] bottom_x;
        /// <summary>
        /// координаты дна по оси у
        /// </summary>
        protected double[] bottom_y;
        /// <summary>
        /// точка правого уреза воды
        /// </summary>
        protected HKnot right;
        /// <summary>
        /// точка левого уреза воды
        /// </summary>
        protected HKnot left;
        /// <summary>
        /// текущий расход потока
        /// </summary>
        protected double riverFlowRate;
        /// <summary>
        /// текущий расчетный расход потока 
        /// </summary>
        protected double riverFlowRateCalk;
        /// <summary>
        /// текущий уровень свободной поверхности
        /// </summary>
        protected double waterLevel;
        #endregion

        #region Инструменты и функции
        /// <summary>
        ///  начальная геометрия русла
        /// </summary>
        protected IDigFunction Geometry;
        /// <summary>
        /// уровни(нь) свободной поверхности потока
        /// </summary>
        protected IDigFunction WaterLevels;
        /// <summary>
        /// расход потока
        /// </summary>
        protected IDigFunction FlowRate;
        /// <summary>
        /// Параметры задачи зависимые от времени
        /// </summary>
        protected List<TaskEvolution> evolution = new List<TaskEvolution>();
        #endregion
        public APRiver1YD(TParam p) : base(p, TypeTask.streamY1D) { }

        #region Локальные методы
        /// <summary>
        /// Имена файлов с данными для задачи в створе
        /// </summary>
        public override ITaskFileNames taskFileNemes()
        {
            ITaskFileNames fn = new TaskFileNames(GetFormater());
            fn.NameCPParams = "NameCPParams.txt";
            fn.NameBLParams = "NameBLParams.txt";
            fn.NameRSParams = "NameRSParamsCross.txt";
            fn.NameRData = "NameCrossRData.txt";
            return fn;
        }
        #region методы предстартовой подготовки задачи
        /// <summary>
        /// Чтение данных задачи из файла
        /// </summary>
        /// <param name="file">имя файла</param>
        public override void LoadData(StreamReader file)
        {
            // геометрия дна
            Geometry = new DigFunction();
            // свободная поверхность
            WaterLevels = new DigFunction();
            // расход потока
            FlowRate = new DigFunction();
            //
            evolution.Clear();
            // геометрия дна
            Geometry.Load(file);
            // свободная поверхность
            WaterLevels.Load(file);
            // расход потока
            FlowRate.Load(file);
            // инициализация задачи
            InitTask();
            // готовность задачи
            eTaskReady = ETaskStatus.LoadAreaData;
        }
        /// <summary>
        /// Конфигурация задачи по умолчанию (тестовые задачи)
        /// </summary>
        /// <param name="testTaskID">номер задачи по умолчанию</param>
        public override void DefaultCalculationDomain(uint testTaskID = 0)
        {

        }
        #endregion
        #endregion

        #region Переопределение абстрактные методы IRiver, ITask ...
        /// <summary>
        /// Получение полей придонных касательных напряжений и давления/свободной 
        /// поверхности по контексту задачи усредненных на конечных элементах
        /// </summary>
        /// <param name="tauX">придонное касательное напряжение по х</param>
        /// <param name="tauY">придонное касательное напряжение по у</param>
        /// <param name="P">придонных давления/свободная поверхности потока, по контексту задачи</param>
        public override void GetTau(ref double[] _tauX, ref double[] _tauY, ref double[] P,
                ref double[][] CS, StressesFlag sf = StressesFlag.Nod)
        {
            _tauX = tau;
        }
        /// <summary>
        /// Расчет полей глубины и скоростей на текущем шаге по времени
        /// </summary>
        /// <summary>
        /// Получить отметки дна
        /// </summary>
        /// <param name="zeta"></param>
        public override void GetZeta(ref double[] zeta)
        {
            MEM.MemCopy(ref zeta, bottom_y);
        }
        /// <summary>
        /// Установка новых отметок дна
        /// </summary>
        /// <param name="zeta">отметки дна</param>
        /// <param name="bedErosion">флаг генерация сетки при размывах дна</param>
        public override void SetZeta(double[] zeta, EBedErosion bedErosion)
        {
            Erosion = bedErosion;
            bottom_y = zeta;
        }
        /// <summary>
        ///  Сетка для расчета донных деформаций
        /// </summary>
        public override IMesh BedMesh()
        {
            return new TwoMesh(bottom_x, bottom_y);
        }
        #endregion

        #region локальные абстрактные методы
        protected abstract void InitTask();
        /// <summary>
        /// Создать расчетную область
        /// </summary>
        /// <param name="right"></param>
        /// <param name="left"></param>
        protected abstract void CreateCalculationDomain();
        /// <summary>
        /// Изменение уравня свободной поверхности
        /// </summary>
        /// <returns></returns>
        protected abstract bool CalkWaterLevel();
        protected abstract double[] TausToVols(in double[] x, in double[] y);
        #endregion
    }
}
