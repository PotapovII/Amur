//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 07.02.2024 Потапов И.И.
//---------------------------------------------------------------------------
//                      Сбока No Property River Lib
//              с убранными из наследников Property классами 
//---------------------------------------------------------------------------
namespace NPRiverLib.ABaseTask
{
    using MemLogLib;
    using CommonLib;
    using CommonLib.IO;
    using CommonLib.ChannelProcess;
    
    using System;
    using System.IO;
    using System.Collections.Generic;
    using CommonLib.Physics;
    using CommonLib.Function;

    /// <summary>
    /// Абстактный класс задачи
    /// </summary>
    /// <typeparam name="TParam">Параметры задачи</typeparam>
    [Serializable]
    public abstract class APRiver<TParam> where TParam : class, ITProperty<TParam>
    {
        #region Константы 
        /// <summary>
        /// Плотность потока
        /// </summary>
        public double rho_w = SPhysics.rho_w;
        /// <summary>
        /// Кинематическая вязкость потока
        /// </summary>
        public double nu = SPhysics.nu;
        /// <summary>
        /// Кинематическая вязкость потока
        /// </summary>
        public double mu = SPhysics.mu;
        /// <summary>
        /// Параметр Кармана
        /// </summary>
        public double kappa_w = SPhysics.kappa_w;
        /// <summary>
        /// Ускорение свободного падения [м/с²]
        /// </summary>
        public double GRAV = SPhysics.GRAV;
        #endregion
        /// <summary>
        ///  Готовность задачи к расчету
        /// </summary>
        protected ETaskStatus eTaskReady = ETaskStatus.TaskNoReady;

        #region Параметры задачи
        /// <summary>
        /// Параметры задачи
        /// </summary>
        protected TParam Params = null;
        /// <summary>
        /// Установка свойств задачи
        /// </summary>
        /// <param name="p"></param>
        public void SetParams(object p)
        {
            TParam pp = (TParam)p;
            SetParams(pp);
        }
        public void SetParams(TParam p)
        {
            Params = p.Clone(p);
            if(eTaskReady < ETaskStatus.LoadParamsData)
                eTaskReady = ETaskStatus.LoadParamsData;
        }
        public object GetParams()
        {
            return Params;
        }
        public virtual void LoadParams(string fileName = "")
        {
            string message = "Файл парамеров задачи - доные деформации - не обнаружен";
            WR.LoadParams(Params.Load, message, fileName);
            if (eTaskReady < ETaskStatus.LoadParamsData)
                eTaskReady = ETaskStatus.LoadParamsData;
        }
        #endregion

        #region Информация о задаче
        /// <summary>
        /// Готовность задачи к расчету
        /// </summary>
        public bool TaskReady()
        {
            if (eTaskReady == ETaskStatus.TaskReady)
                return true;
            else
                return false;
        }
        public ETaskStatus _TaskReady() => eTaskReady;
        /// <summary>
        /// Флаг определяющий необходимость расчета взвешенных наносов
        /// </summary>
        public EBedErosion GetBedErosion() => Erosion;
        protected EBedErosion Erosion = EBedErosion.NoBedErosion;
        /// <summary>
        /// Наименование задачи
        /// </summary>
        public string Name => name;
        protected string name = "";
        /// <summary>
        /// Тип задачи используется для выбора совместимых подзадач 
        /// </summary>
        public TypeTask typeTask { get; }
        /// <summary>
        /// версия дата последних изменений интерфейса задачи
        /// </summary>
        public string VersionData() => Version;
        protected string Version = "";
        #endregion

        #region Общие поля задач
        /// <summary>
        /// Текущее время
        /// </summary>
        public double time { get; set; }
        /// <summary>
        /// Текущий шаг по времени
        /// </summary>
        public double dtime { get; set; }
        /// <summary>
        /// КЭ сетка
        /// </summary>
        public virtual IMesh Mesh() => mesh;
        protected IMesh mesh = null;
        /// <summary>
        /// Алгебра для КЭ задачи
        /// </summary>
        [NonSerialized]
        protected IAlgebra algebra = null;
        /// <summary>
        /// Искомые поля задачи которые позволяют сформировать на 
        /// сетке расчетной области краевые условия задачи
        /// </summary>
        public  IUnknown[] Unknowns() => unknowns.ToArray();
        protected  List<IUnknown> unknowns = new List<IUnknown>();
        /// <summary>
        /// Получить граничные условия для задачи донных деформаций
        /// </summary>
        /// <returns></returns>
        public IBoundaryConditions BoundCondition() => boundaryConditions;
        protected IBoundaryConditions boundaryConditions = null;
        #endregion

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="p">Параметры</param>
        /// <param name="tt">Тип задачи</param>
        public APRiver(TParam p, TypeTask tt)
        {
            typeTask = tt;
            SetParams(p);
        }
        /// <summary>
        /// Установка адаптеров для КЭ сетки и алгебры
        /// </summary>
        /// <param name="_mesh">сетка</param>
        /// <param name="algebra">решатель</param>
        public virtual void Set(IMesh mesh, IAlgebra algebra = null)
        {
            this.mesh = mesh;
            if (algebra != null)
                this.algebra = algebra;
        }
        /// <summary>
        /// Формирование данных для отрисовки связанных с сеткой IMesh со сороны задачи
        /// </summary>
        /// <param name="sp">контейнер данных</param>
        public virtual void AddMeshPolesForGraphics(ISavePoint sp)
        {
            // поля на сетке
            foreach (var fi in unknowns)
            {
                if (fi.Dimention == 1)
                    sp.Add(fi.NameUnknow, fi.ValuesX);
                else
                    sp.Add(fi.NameUnknow, fi.ValuesX, fi.ValuesY);
            }
        }

        #region абстрактные методы 
        #region методы предстартовой подготовки задачи
        /// <summary>
        /// Загрузка задачи иp форматного файла
        /// </summary>
        /// <param name="file">имя файла</param>
        public abstract void LoadData(StreamReader file);
        /// <summary>
        /// Загрузка задачи иp форматного файла
        /// </summary>
        /// <param name="file">имя файла</param>
        public abstract void LoadData(IDigFunction[] crossFunctions = null);
        /// <summary>
        /// Конфигурация задачи по умолчанию (тестовые задачи)
        /// </summary>
        /// <param name="testTaskID">номер задачи по умолчанию</param>
        public abstract void DefaultCalculationDomain(uint testTaskID = 0);
        #endregion 
        /// <summary>
        /// Имена файлов с данными для задачи гидродинамики
        /// </summary>
        public abstract ITaskFileNames taskFileNemes();
        /// <summary>
        /// Получение полей придонных касательных напряжений и давления/свободной поверхности по контексту задачи
        /// усредненных на конечных элементах
        /// </summary>
        /// <param name="tauX">придонное касательное напряжение по х</param>
        /// <param name="tauY">придонное касательное напряжение по у</param>
        /// <param name="P">придонных давления/свободная поверхности потока, по контексту задачи</param>
        /// <param name="CS">расход взвешенных наносов по фракциям</param>
        /// <param name="sf">тип данных по КЭ или в узлахб по умолчанию в узлах</param>
        public abstract void GetTau(ref double[] _tauX, ref double[] _tauY,
            ref double[] P, ref double[][] CS, StressesFlag sf = StressesFlag.Nod);
        /// <summary>
        /// Расчет полей глубины и скоростей на текущем шаге по времени
        /// </summary>
        /// <summary>
        /// Получить отметки дна
        /// </summary>
        /// <param name="zeta"></param>
        public abstract void GetZeta(ref double[] zeta);
        /// <summary>
        /// Установка новых отметок дна
        /// </summary>
        /// <param name="zeta">отметки дна</param>
        /// <param name="bedErosion">флаг генерация сетки при размывах дна</param>
        public abstract void SetZeta(double[] zeta, EBedErosion bedErosion);
        /// <summary>
        ///  Сетка для расчета донных деформаций
        /// </summary>
        public abstract IMesh BedMesh();
        /// <summary>
        /// Расчет полей глубины и скоростей на текущем шаге по времени
        /// </summary>
        public abstract void SolverStep();
        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public abstract IRiver Clone();
        /// <summary>
        /// Создает экземпляр класса конвертера
        /// </summary>
        public abstract IOFormater<IRiver> GetFormater();
        /// <summary>
        /// Создает список тестовых задач для загрузчика по умолчанию
        /// </summary>
        /// <returns></returns>
        public virtual List<string> GetTestsName()
        {
            List<string> strings = new List<string>();
            strings.Add("Основная задача - тестовая");
            return strings;
        }
        #endregion

    }
}
