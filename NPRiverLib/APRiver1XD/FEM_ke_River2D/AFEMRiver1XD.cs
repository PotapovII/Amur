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
namespace NPRiverLib.APRiver_1XD
{
    using MeshLib;
    using MemLogLib;
    using GeometryLib;

    using CommonLib;
    using CommonLib.Function;
    using CommonLib.ChannelProcess;

    using NPRiverLib.ABaseTask;
    using NPRiverLib.APRiver1YD.Params;

    using System;
    using System.IO;
    using System.Collections.Generic;
    using CommonLib.Mesh;
    using MeshLib.Wrappers;
    using static alglib;


    /// <summary>
    ///             Базовый тип для створовых русловых задач
    ///  ОО: Определение класса  APRiver1YD - расчет полей скорости, вязкости 
    ///           и напряжений в живом сечении потока (створе)
    /// </summary>    
    /// <typeparam name="TParam">параметры задачи</typeparam>
    [Serializable]
    public abstract class AFEMRiver1XD<TParam> :
        APRiver<TParam> where TParam : class, ITProperty<TParam>
    {
        #region Физические параметры
        /// <summary>
        /// Турбулентная вязкость
        /// </summary>
        protected double eddyViscosityConst = 1.1;
        /// <summary>
        /// коэффициент Прандтля для вязкости
        /// </summary>
        protected double sigma = 1;
        #endregion

        #region Искомые поля створа без вторичных потоков
        /// <summary>
        /// Поле вязкости текущее и с предыдущей итерации
        /// </summary>
        public double[] eddyViscosity;
        /// <summary>
        /// Поле напряжений T_xz
        /// </summary>
        public double[] TauZ;
        /// <summary>
        /// Поле напряжений dy
        /// </summary>
        public double[] TauY;
        /// <summary>
        /// Поле придонных напряжений 
        /// </summary>
        public double[] tau;
        /// <summary>
        /// координаты придонных напряжений (центры КЭ)
        /// </summary>
        public double[] xtau;
        /// <summary>
        /// сечения створа
        /// </summary>
        protected int[][] riverGates;
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
        #endregion

        #region Инструменты и функции
        /// <summary>
        /// Обертка для КЭ сетки
        /// </summary>
        protected IMeshWrapper wMesh;
        /// <summary>
        /// Скорость на входе/выходе
        /// </summary>
        public IDigFunction[] BCVelosity = null;
        /// <summary>
        /// Функция тока на входе/выходе
        /// </summary>
        public IDigFunction[] bcPhi = null;
        /// <summary>
        /// Параметры задачи зависимые от времени
        /// </summary>
        protected List<TaskEvolution> evolution = new List<TaskEvolution>();
        #endregion
        public AFEMRiver1XD(TParam p) : base(p, TypeTask.streamX1D) { }

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
            fn.NameRData = "NameCrossRData.rvy";
            return fn;
        }
        #region методы предстартовой подготовки задачи

        /// <summary>
        /// Установка адаптеров для КЭ сетки и алгебры
        /// </summary>
        /// <param name="_mesh">сетка</param>
        /// <param name="algebra">решатель</param>
        public override void Set(IMesh mesh, IAlgebra algebra = null)
        {
            base.Set(mesh, algebra);
            wMesh = new MeshWrapperTri(mesh);
        }
        /// <summary>
        /// Чтение данных задачи из файла
        /// </summary>
        /// <param name="file">имя файла</param>
        public override void LoadData(StreamReader file)
        {
            BCVelosity = new DigFunction[2];
            // геометрия дна
            BCVelosity[0] = new DigFunction();
            // свободная поверхность
            BCVelosity[1] = new DigFunction();
            evolution.Clear();
            // геометрия дна
            BCVelosity[0].Load(file);
            // свободная поверхность
            BCVelosity[1].Load(file);
            // готовность задачи
            eTaskReady = ETaskStatus.LoadAreaData;
        }
        /// <summary>
        /// Загрузка задачи из тестовых данных
        /// </summary>
        /// <param name="file">имя файла</param>
        public override void LoadData(IDigFunction[] BCVelosity)
        {
            evolution.Clear();
            // геометрия дна
            this.BCVelosity = BCVelosity;
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
            MEM.Copy(ref bottom_y, zeta);
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
        /// <summary>
        /// Создать расчетную область
        /// </summary>
        /// <param name="right"></param>
        /// <param name="left"></param>
        protected abstract void CreateCalculationDomain();

        protected abstract double[] TausToVols(in double[] x, in double[] y, bool spline = false);
        #endregion
    }
}
