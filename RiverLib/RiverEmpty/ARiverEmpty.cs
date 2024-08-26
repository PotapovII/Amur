//---------------------------------------------------------------------------
//                          ПРОЕКТ  "DISER"
//              создано  :   17.09.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace RiverLib
{
    using System;
    using System.IO;
    using MeshLib;
    using CommonLib;
    using CommonLib.IO;
    using MemLogLib;
    using CommonLib.EConverter;
    using CommonLib.ChannelProcess;
    using GeometryLib;

    /// <summary>
    ///  ОО: Определение класса ARiverEmpty - заглушки задачи для 
    ///  расчета полей скорости и напряжений в речном потоке
    /// </summary>
    [Serializable]
    public class ARiverEmpty : RiverStreamParams, IRiver
    {
        /// <summary>
        /// Готовность задачи к расчету
        /// </summary>
        public bool TaskReady() => true;

        public IBoundaryConditions BoundCondition1D = null;
        /// <summary>
        /// Флаг определяющий необходимость расчета взвешенных наносов
        /// </summary>
        public EBedErosion GetBedErosion() => Erosion;
        public EBedErosion Erosion;
        /// <summary>
        /// Получить граничные условия для задачи донных деформаций
        /// </summary>
        /// <returns></returns>
        public virtual IBoundaryConditions BoundCondition() => BoundCondition1D;
        /// <summary>
        /// Искомые поля задачи которые позволяют сформировать на сетке расчетной области краевые условия задачи
        /// </summary>
        public IUnknown[] Unknowns() => unknowns;
        protected IUnknown[] unknowns = { new Unknown("Cкорость потока", null, TypeFunForm.Form_2D_Rectangle_L1) };
        /// <summary>
        /// Имена файлов с данными для задачи гидродинамики
        /// </summary>
        public virtual ITaskFileNames taskFileNemes()
        {
            ITaskFileNames fn = new TaskFileNames();
            fn.NameCPParams = "NameCPParams.txt";
            fn.NameBLParams = "NameBLParams.txt";
            fn.NameRSParams = "NameRSParams.txt";
            fn.NameRData = "NameRData.txt";
            fn.NameEXT = "(*.tsk)|*.tsk|";
            return fn;
        }
        /// <summary       
        /// Тип задачи используется для выбора совместимых подзадач
        /// </summary>
        public TypeTask typeTask { get => _typeTask; }
        protected TypeTask _typeTask;
        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public virtual IRiver Clone()
        {
            return null;
        }
        #region Свойства
        /// <summary>
        /// Наименование задачи
        /// </summary>
        public virtual string Name { get => "прокси гидрадинамика"; }
        /// <summary>
        /// версия дата последних изменений интерфейса задачи
        /// </summary>
        public virtual string VersionData() => "RiverEmpty1D 17.09.2021"; 
        /// <summary>
        /// Текущее время
        /// </summary>
        public double time { get; set; }
        /// <summary>
        /// Текущий шаг по времени
        /// </summary>
        public double dtime { get; set; }
        #endregion
        /// <summary>
        /// КЭ сетка
        /// </summary>
        public IMesh Mesh() => mesh;
        /// <summary>
        /// КЭ сетка
        /// </summary>
        protected IMesh mesh = null;
        /// <summary>
        ///  Сетка для расчета донных деформаций
        /// </summary>
        public virtual IMesh BedMesh() { return new TwoMesh(x, y); }
        /// <summary>
        /// Алгебра для КЭ задачи
        /// </summary>
        [NonSerialized]
        private IAlgebra algebra = null;
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
        protected double[] x = null;
        /// <summary>
        /// координаты дна по оси у
        /// </summary>
        protected double[] y = null;
        /// <summary>
        /// координаты дна по оси z
        /// </summary>
        public double[] zeta0 = null;

        /// <summary>
        /// Установка свойств задачи
        /// </summary>
        /// <param name="p"></param>
        public void SetParams(object p)
        {
            Set((RiverStreamParams)p);
        }

        public ARiverEmpty(RiverStreamParams p) : base(p)
        {
            x = new double[2] { 0, 1 };
            y = new double[2] { 0, 0 };
        }

        /// <summary>
        /// Загрузка данных задачи
        /// </summary>
        //public virtual void LoadTaskData(string fileName)
        //{
        //    string message = "Файл данных задачи не обнаружен";
        //    WR.LoadParams(LoadData, message, fileName);
        //}

        public  virtual bool LoadData(string fileName)
        {
            string message = "Файл данных задачи не обнаружен";
            return WR.LoadParams(LoadData, message, fileName);
        }
        /// <summary>
        /// Чтение входных данных задачи из файла
        /// геометрия канала
        /// эволюция расходов/уровней
        /// </summary>
        /// <param name="p"></param>
        public virtual void LoadData(StreamReader file)
        {

        }

        /// <summary>
        /// Установка адаптеров для КЭ сетки и алгебры
        /// </summary>
        /// <param name="_mesh"></param>
        /// <param name="algebra"></param>
        public virtual void Set(IMesh mesh, IAlgebra algebra = null)
        {
            this.mesh = mesh;
            this.algebra = algebra;
            BoundCondition1D = new BoundaryConditionsVar(mesh);
            //BoundaryConditionsVar BC = new BoundaryConditionsVar(mesh);
            //int CountMark = BC.CountMark;
            //for (int i = 0; i < CountMark; i++)
            //{
            //    BC.ValueDir[i] = 0;
            //    BC.ValueNeu[i] = 0;
            //}
            //BoundCondition1D = BC;
        }
        /// <summary>
        /// Установка новых отметок дна
        /// </summary>
        /// <param name="zeta"></param>
        public virtual void SetZeta(double[] zeta, EBedErosion bedErosion)
        {
            Erosion = bedErosion;
        }
        /// <summary>
        /// Получить отметки дна
        /// </summary>
        /// <param name="zeta"></param>
        public void GetZeta(ref double[] zeta)
        {
            zeta = this.zeta0;
        }
        /// <summary>
        /// Расчет полей глубины и скоростей на текущем шаге по времени
        /// </summary>
        public void SolverStep()
        {
            time += dtime;
        }
        /// <summary>
        /// Формирование данных для отрисовки связанных с сеткой IMesh со сороны задачи
        /// </summary>
        /// <param name="sp">контейнер данных</param>
        public virtual void AddMeshPolesForGraphics(ISavePoint sp)
        {
        }
        /// <summary>
        /// Получение полей придонных касательных напряжений и давления/свободной поверхности по контексту задачи
        /// усредненных на конечных элементах
        /// </summary>
        /// <param name="tauX">придонное касательное напряжение по х</param>
        /// <param name="tauY">придонное касательное напряжение по у</param>
        /// <param name="P">придонных давления/свободная поверхности потока, по контексту задачи</param>
        public virtual void GetTau(ref double[] tauX, ref double[] tauY, ref double[] P, ref double[][] CS, StressesFlag sf = StressesFlag.Nod)
        {
            tauX = this.tauX;
            tauY = this.tauY;
            P = this.P;
        }
        /// <summary>
        /// Создает экземпляр класса конвертера
        /// </summary>
        /// <returns></returns>
        public virtual IOFormater<IRiver> GetFormater()
        {
            return new ProxyTaskFormat<IRiver>();
        }
    }
}
