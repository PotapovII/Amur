//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 25.07.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace NPRiverLib.RiverEmpty
{
    using System;
    using System.IO;

    using MeshLib;
    using MemLogLib;

    using CommonLib;
    using CommonLib.IO;
    using CommonLib.ChannelProcess;
    
    using NPRiverLib.ATask;
    /// <summary>
    ///  ОО: Определение класса ARiverBaseEmpty - заглушки для задачи 
    ///  расчета полей скорости и напряжений в речном потоке
    /// </summary>
    [Serializable]
    public abstract class ARiverBaseEmpty : APRiver<EmptyParams> , IRiver
    {
        public ARiverBaseEmpty(TypeTask tt, EmptyParams p) : base(p, tt)
        {
        }
        public ARiverBaseEmpty(TypeTask tt) : base(new EmptyParams(), tt)
        {
        }
        /// <summary>
        /// Создает экземпляр класса конвертера
        /// </summary>
        /// <returns></returns>
        public override IOFormater<IRiver> GetFormater()
        {
            return new ProxyTaskFormat<IRiver>();
        }
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
            fn.NameEXT = "(*.tsk)|*.tsk|";
            return fn;
        }
        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public override IRiver Clone()
        {
            return null;
        }
        /// <summary>
        ///  Сетка для расчета донных деформаций
        /// </summary>
        public override IMesh BedMesh() { return new TwoMesh(x, y); }
        /// <summary>
        /// Поле напряжений dz
        /// </summary>
        public double[] tauX  = null;
        /// <summary>
        /// Поле напряжений dy
        /// </summary>
        public double[] tauY = null;
        /// <summary>
        /// Поле напряжений dy
        /// </summary>
        public double[] P = null;
        /// <summary>
        /// координаты дна по оси х
        /// </summary>
        protected double[] x = { 0, 1 };
        /// <summary>
        /// координаты дна по оси у
        /// </summary>
        protected double[] y = { 0, 0 };
        /// <summary>
        /// координаты дна по оси z
        /// </summary>
        public double[] zeta0 = null;
        public  virtual bool LoadData(string fileName)
        {
            string message = "Файл данных задачи не обнаружен";
            return WR.LoadParams(LoadData, message, fileName);
        }
        #region методы предстартовой подготовки задачи
        /// <summary>
        /// Загрузка задачи иp форматного файла
        /// </summary>
        /// <param name="file">имя файла</param>
        public override void LoadData(StreamReader file) { }
        /// <summary>
        /// Конфигурация задачи по умолчанию (тестовые задачи)
        /// </summary>
        /// <param name="testTaskID">номер задачи по умолчанию</param>
        public override void DefaultCalculationDomain(uint testTaskID = 0) 
        {
        }
        #endregion 
        /// <summary>
        /// Установка новых отметок дна
        /// </summary>
        /// <param name="zeta"></param>
        public override void SetZeta(double[] zeta, EBedErosion bedErosion)
        {
            Erosion = bedErosion;
        }
        /// <summary>
        /// Получить отметки дна
        /// </summary>
        /// <param name="zeta"></param>
        public override void GetZeta(ref double[] zeta)
        {
            zeta = this.zeta0;
        }
        /// <summary>
        /// Расчет полей глубины и скоростей на текущем шаге по времени
        /// </summary>
        public override void SolverStep()
        {
            time += dtime;
        }
        /// <summary>
        /// Формирование данных для отрисовки связанных с сеткой IMesh со сороны задачи
        /// </summary>
        /// <param name="sp">контейнер данных</param>
        public override void AddMeshPolesForGraphics(ISavePoint sp)
        {
        }
        /// <summary>
        /// Получение полей придонных касательных напряжений и давления/свободной поверхности по контексту задачи
        /// усредненных на конечных элементах
        /// </summary>
        /// <param name="tauX">придонное касательное напряжение по х</param>
        /// <param name="tauY">придонное касательное напряжение по у</param>
        /// <param name="P">придонных давления/свободная поверхности потока, по контексту задачи</param>
        public override void GetTau(ref double[] tauX, ref double[] tauY, ref double[] P, ref double[][] CS, StressesFlag sf = StressesFlag.Nod)
        {
            tauX = this.tauX;
            tauY = this.tauY;
            P = this.P;
        }
    }
}
