//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 24.12.2024 Потапов И.И.
//---------------------------------------------------------------------------
//                      Сбока No Property River Lib
//              с убранными из наследников Property классами 
//---------------------------------------------------------------------------
namespace NPRiverLib.ABaseTask
{
    using System;
    using System.IO;

    using MemLogLib;
    using GeometryLib;
    
    using CommonLib;
    using CommonLib.IO;
    using CommonLib.ChannelProcess;
        
    using NPRiverLib.IO;
    using CommonLib.Function;

    /// <summary>
    ///            Базовый тип заглушек для задач гидродинамики
    ///  ОО: Определение класса  ARiverAnyEmpty - предоставления полей 
    ///               придонных напряжений и давлений, 
    ///             для решения задавчи донных деформаций
    /// </summary>
    /// <typeparam name="TParam">параметры задачи</typeparam>
    [Serializable]
    public abstract class ARiverAnyEmpty<TParam> :
        APRiver<TParam> where TParam : class, ITProperty<TParam>
    {
        /// <summary>
        /// Поле напряжений dz
        /// </summary>
        public double[] tauX = null;
        /// <summary>
        /// Поле напряжений dy
        /// </summary>
        public double[] tauY = null;
        /// <summary>
        /// Поле напряжений dy
        /// </summary>
        public double[] P = null;
        /// <summary>
        /// расход взвешенных наносов по фракциям
        /// </summary>
        public double[][] CS = null;
        /// <summary>
        /// координаты дна по оси z
        /// </summary>
        public double[] zeta0 = null;
        ///// <summary>
        ///// КЭ сетка
        ///// </summary>
        //public override IMesh Mesh() => mesh;
        /// <summary>
        /// Имена файлов с данными для задачи гидродинамики
        /// </summary>
        public override ITaskFileNames taskFileNemes()
        {
            ITaskFileNames fn = new TaskFileNames(GetFormater());
            fn.NameCPParams = "NameCPParams.txt";
            fn.NameBLParams = "NameBLParams.txt";
            fn.NameRSParams = "NameRSParams.txt";
            fn.NameRData = "NameRData.txt";
            return fn;
        }
        /// <summary>
        /// Получить шероховатость дна
        /// </summary>
        /// <param name="zeta"></param>
        public void GetRoughness(ref double[] Roughness)
        {
            Roughness = null;
        }

        public ARiverAnyEmpty(TParam p, TypeTask typeTask) : base(p, typeTask) { }
        /// <summary>
        /// Установка КЭ сетки и решателя задачи
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="algebra">решатель задачи</param>
        public override void Set(IMesh mesh, IAlgebra algebra = null)
        {
            base.Set(mesh, algebra);
        }
        /// <summary>
        /// Установка новых отметок дна
        /// </summary>
        /// <param name="zeta"></param>
        public override void SetZeta(double[] zeta, EBedErosion bedErosion)
        {
            Erosion = bedErosion;
            if (zeta != null) MEM.Copy(ref zeta0, zeta);
        }
        /// <summary>
        /// Расчет полей глубины и скоростей на текущем шаге по времени
        /// </summary>
        public override void SolverStep()
        {
            time += dtime;
        }
        #region методы предстартовой подготовки задачи
        /// <summary>
        /// Загрузка задачи иp форматного файла
        /// </summary>
        /// <param name="file">имя файла</param>
        public override void LoadData(StreamReader file) { }
        /// <summary>
        /// Загрузка задачи иp форматного файла
        /// </summary>
        /// <param name="file">имя файла</param>
        public override void LoadData(IDigFunction[] crossFunctions = null) { }
        /// <summary>
        /// Конфигурация задачи по умолчанию (тестовые задачи)
        /// </summary>
        /// <param name="testTaskID">номер задачи по умолчанию</param>
        public override void DefaultCalculationDomain(uint testTaskID = 0) { }
        #endregion
        /// <summary>
        /// Получить отметки дна
        /// </summary>
        /// <param name="zeta"></param>
        public override void GetZeta(ref double[] zeta)
        {
            zeta = this.zeta0;
        }
        /// <summary>
        /// Создает экземпляр класса конвертера для 
        /// загрузки и сохранения задачи не в бинарном формате
        /// </summary>
        /// <returns></returns>
        public override IOFormater<IRiver> GetFormater()
        {
            return new TaskReaderEmptyAll();
        }
    }
}
