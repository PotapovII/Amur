////---------------------------------------------------------------------------
////              Реализация библиотеки для моделирования 
////               гидродинамических и русловых процессов
////                      - (C) Copyright 2021 -
////                       ALL RIGHT RESERVED
////                        ПРОЕКТ "BLLib"
////---------------------------------------------------------------------------
////                   разработка: Потапов И.И.
////                          12.04.21
////---------------------------------------------------------------------------
//namespace BLLib
//{
//    using AlgebraLib;
//    using CommonLib;
//    using MemLogLib;
//    using System;
//    /// <summary>
//    /// ОО: Абстрактный класс для задач деформируемого дна c однородным донным материалом
//    /// Решение задачи МКЭ
//    /// </summary>
//    [Serializable]
//    public abstract class ABedLoadTask : BedLoadParams, IBedLoadTask
//    {
//        /// <summary>
//        /// Название файла параметров задачи
//        /// </summary>
//        public string NameBLParams() => "NameBLParams.txt";
//        /// <summary>
//        /// текущее время расчета 
//        /// </summary>
//        public double time { get; set; }
//        /// <summary>
//        /// шаг по времени
//        /// </summary>
//        public double dtime { get; set; }
//        /// <summary>
//        /// наименование задачи
//        /// </summary>
//        public string Name { get => name; }
//        protected string name;
//        /// <summary>
//        /// Тип задачи используется для выбора совместимых подзадач
//        /// </summary>
//        public TypeTask typeTask { get => _typeTask; }
//        protected TypeTask _typeTask;
//        /// <summary>
//        /// версия дата последних изменений интерфейса задачи
//        /// </summary>
//        public string VersionData() => "ABedLoadTask 25.09.2021"; 
//        ///// <summary>
//        ///// Модель влекомого движения донного матеиала
//        ///// </summary>
//        //public TypeBLModel blm { get => blm; }
//        //protected TypeBLModel blm;
//        /// <summary>
//        /// Сетка решателя
//        /// </summary>
//        public IMesh Mesh { get => mesh; }
//        protected IMesh mesh;
//        /// <summary>
//        /// Алгебра задачи
//        /// </summary>
//        public IAlgebra Algebra() => algebra;
//        [NonSerialized]
//        protected IAlgebra algebra = null;
//        /// <summary>
//        /// Для правой части
//        /// </summary>
//        [NonSerialized]
//        protected IAlgebra Ralgebra = null;
//        /// <summary>
//        /// Текщий уровень дна
//        /// </summary>
//        public double[] CZeta { get => Zeta0; }
//        /// <summary>
//        /// массив донных отметок на предыдущем слое по времени
//        /// </summary>
//        protected double[] Zeta0 = null;
//        /// <summary>
//        /// массив придонного давления
//        /// </summary>
//        public double[] P = null;
//        /// <summary>
//        /// Флаг отладки
//        /// </summary>
//        public int debug = 0;
//        /// <summary>
//        /// Поле сообщений о состоянии задачи
//        /// </summary>
//        public string Message = "Ok";
//        /// <summary>
//        /// Параметр схемы по времени
//        /// </summary>
//        public double theta = 0.5;
//        /// <summary>
//        /// текущая итерация по времени 
//        /// </summary>
//        public int CountTime = 0;
//        /// <summary>
//        /// относительная точность при вычислении 
//        /// изменения донной поверхности
//        /// </summary>
//        protected double eZeta = 0.000001;
//        ///// <summary>
//        ///// множитель для приведения придонного давления к напору
//        ///// </summary>
//        //double gamma;
//        #region Переменные для работы с КЭ аналогом
//        /// <summary>
//        /// Узлы КЭ
//        /// </summary>
//        protected uint[] knots = null;
//        /// <summary>
//        /// координаты узлов КЭ
//        /// </summary>
//        protected double[] x = null;
//        protected double[] y = null;
//        /// <summary>
//        /// локальная матрица часть СЛАУ
//        /// </summary>
//        protected double[][] LaplMatrix = null;
//        /// <summary>
//        /// локальная матрица часть СЛАУ
//        /// </summary>
//        protected double[][] RMatrix = null;
//        /// <summary>
//        /// локальная правая часть СЛАУ
//        /// </summary>
//        protected double[] LocalRight = null;
//        /// <summary>
//        /// адреса ГУ
//        /// </summary>
//        protected uint[] adressBound = null;
//        /// <summary>
//        /// Вспомогательная правая часть
//        /// </summary>
//        protected double[] MRight = null;
//        #endregion
//        #region Служебные переменные
//        /// <summary>
//        ///  косинус гамма - косинус угола между 
//        ///  нормалью к дну и вертикальной осью
//        /// </summary>
//        protected double[] CosGamma = null;
//        protected double[] tau = null;
//        protected double[] G0 = null;
//        protected double[] A = null;
//        protected double[] B = null;
//        protected double[] C = null;
//        protected double[] D = null;
//        protected double[] ps = null;
//        protected int cu;
//        protected double[] a;
//        protected double[] b;
//        protected double dz, dx, dp;
//        protected double mtau, chi;
//        #endregion
//        /// <summary>
//        /// Конструктор по умолчанию/тестовый
//        /// </summary>
//        public ABedLoadTask(BedLoadParams p) : base(p)
//        {
//            InitBedLoad();
//            time = 0;
//        }
//        public void ReInitBedLoad(BedLoadParams p)
//        {
//            SetParams(p);
//            InitBedLoad();
//            time = 0;
//        }
//        /// <summary>
//        /// Готовность задачи к расчету
//        /// </summary>
//        protected bool taskReady = false;
//        /// <summary>
//        /// Готовность задачи к расчету
//        /// </summary>
//        public bool TaskReady()
//        {
//            return taskReady;
//        }
//        /// <summary>
//        /// Установка текущей геометрии расчетной области
//        /// </summary>
//        /// <param name="mesh">Сетка расчетной области</param>
//        /// <param name="Zeta0">начальный уровень дна</param>
//        public virtual void SetTask(IMesh mesh, double[] Zeta0)
//        {
//            taskReady = false;
//            this.mesh = mesh;
//            this.Zeta0 = Zeta0;
//            int Count = mesh.CountElements;

//            if (this.algebra == null)
//                this.algebra = new SparseAlgebraCG((uint)mesh.CountKnots);

//            if (Ralgebra == null)
//                this.Ralgebra = this.algebra.Clone();

//            MEM.Alloc<double>(Count, ref A);
//            MEM.Alloc<double>(Count, ref B);
//            MEM.Alloc<double>(Count, ref C);
//            MEM.Alloc<double>(Count, ref D);
//            MEM.Alloc<double>(Count, ref G0);

//            MEM.Alloc<double>(Count, ref CosGamma);
//            MEM.Alloc<double>(Count, ref tau);
//            MEM.Alloc<double>(Count, ref ps);
//            taskReady = true;
//        }
//        /// <summary>
//        /// создание/очистка ЛМЖ и ЛПЧ ...
//        /// </summary>
//        /// <param name="cu">количество неизвестных</param>
//        public virtual void InitLocal(int cu, int cs = 1)
//        {
//            MEM.Alloc<double>(cu, ref x);
//            MEM.Alloc<double>(cu, ref y);
//            MEM.Alloc<uint>(cu, ref knots);
//            // с учетом степеней свободы
//            int Count = cu * cs;
//            MEM.AllocClear(Count, ref LocalRight);
//            MEM.Alloc2DClear(Count, ref LaplMatrix);
//            MEM.Alloc2DClear(Count, ref RMatrix);
//        }
//        /// <summary>
//        /// Формирование данных для отрисовки связанных с сеткой IMesh со сороны задачи
//        /// </summary>
//        /// <param name="sp">контейнер данных</param>
//        public abstract void AddMeshPolesForGraphics(ISavePoint sp);
//        /// <summary>
//        /// Создает экземпляр класса
//        /// </summary>
//        /// <returns></returns>
//        public abstract IBedLoadTask Clone();
//        ///  /// <summary>
//        /// Вычисление текущих расходов и их градиентов для построения графиков
//        /// </summary>
//        /// <param name="Gs">возвращаемые расходы (по механизмам движения)</param>
//        /// <param name="dGs">возвращаемые градиенты расходы (по механизмам движения)</param>
//        /// <param name="tau">придонное касательное напряжение</param>
//        /// <param name="P">придонное давление</param>
//        public abstract void Calk_Gs(ref double[][] Gs, ref double[][] dGs, double[] tau, double[] P = null);
//        public abstract void CalkZetaFDM(ref double[] Zeta, double[] tauX, double[] tauY, double[] P = null);
//    }
//}
