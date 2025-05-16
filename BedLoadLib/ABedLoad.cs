//---------------------------------------------------------------------------
//                 Реализация библиотеки для моделирования 
//                  гидродинамических и русловых процессов
//---------------------------------------------------------------------------
//                Модуль BedLoadLib для расчета донных деформаций 
//                 (учет движения только влекомых наносов)
//         по русловой модели Петрова А.Г. и Потапова И.И. от 2014 г.
//                              Потапов И.И.
//                               27.05.23
//---------------------------------------------------------------------------
//                      рефакторинг : Потапов И.И.
//                              27.04.25
//---------------------------------------------------------------------------
namespace BedLoadLib
{
    using MemLogLib;
    using CommonLib;
    using CommonLib.Physics;
    using CommonLib.Function;
    using CommonLib.ChannelProcess;

    using System;
    using System.Collections.Generic;
    using CommonLib.BedLoad;

    /// <summary>
    /// Абстактный класс задачи
    /// </summary>
    /// <typeparam name="TParam">Параметры задачи</typeparam>
    [Serializable]
    public abstract class ABedLoad<TParam> where TParam : class, ITProperty<TParam>
    {
        #region Константы 
        /// <summary>
        /// Плотность воды
        /// </summary>
        public double rho_w = SPhysics.rho_w;
        /// <summary>
        /// Плотность песка
        /// </summary>
        public double rho_s = SPhysics.PHYS.rho_s;
        /// <summary>
        /// Относительная плотность (rho_s - rho_w)/rho_w
        /// </summary>
        public double rho_b = SPhysics.PHYS.rho_b;
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
        /// Параметр Кармана водо песчанной смеси
        /// </summary>
        public double kappa = SPhysics.PHYS.kappa;
        /// <summary>
        /// Ускорение свободного падения [м/с²]
        /// </summary>
        public double GRAV = SPhysics.GRAV;
        
        #endregion
        /// <summary>
        /// Поле сообщений о состоянии задачи
        /// </summary>
        public string Message = "Ok";
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
            if (eTaskReady < ETaskStatus.LoadParamsData)
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
        /// Граничные условия задачи
        /// </summary>
        protected IBoundaryConditions BConditions = null;
        /// <summary>
        /// Количество расчетных узлов для дна
        /// </summary>
        protected int Count;
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
        /// Алгебра задачи
        /// </summary>
        public IAlgebra Algebra() => algebra;
        [NonSerialized]
        protected IAlgebra algebra = null;
        /// <summary>
        /// Лавинное обрушение
        /// </summary>
        public IAvalanche GetAvalanche() => avalanche;
        /// <summary>
        /// Лавинное обрушение
        /// </summary>
        public IAvalanche avalanche;
        /// <summary>
        /// Искомые поля задачи которые позволяют сформировать на 
        /// сетке расчетной области краевые условия задачи
        /// </summary>
        public IUnknown[] Unknowns() => unknowns.ToArray();
        protected List<IUnknown> unknowns = new List<IUnknown>();
        /// <summary>
        /// Получить граничные условия для задачи донных деформаций
        /// </summary>
        /// <returns></returns>
        public IBoundaryConditions BoundCondition() => boundaryConditions;
        protected IBoundaryConditions boundaryConditions = null;
        #endregion

        #region Рабочие массивы
        /// <summary>
        /// Флаг для определения сухого-мокрого дна
        /// </summary>
        public int[] DryWet = null;
        /// <summary>
        /// массив донных отметок
        /// </summary>
        public double[] Zeta = null;
        /// <summary>
        /// массив донных отметок на предыдущем слое по времени
        /// </summary>
        public double[] Zeta0 = null;
        /// <summary>
        /// массив производных для уровня донных отметок 
        /// </summary>
        public double[] dZeta = null;
        /// <summary>
        /// массив придонных касательнывх напряжений
        /// </summary>
        public double[] tau = null;
        /// <summary>
        /// массив придонного давления
        /// </summary>
        public double[] P = null;

        /// <summary>
        /// Массив цифровых функций передаваемых
        /// </summary>
        IDigFunction[] crossFunctions = null;
        /// <summary>
        /// шероховатость дна
        /// </summary>
        public double[] Roughness = null;
        /// <summary>
        /// Удельное сцепление песка/грунта
        /// of sand
        /// </summary>
        public double[] Adhesion = null;
        /// <summary>
        /// Удельное сцепление песка/грунта
        /// of sand
        /// </summary>
        public bool AdhesionFlag = false;
        #endregion

        /// <summary>
        /// расчетный период времени, сек 
        /// </summary>
        protected double T;
        protected double dx, dp;
        protected double mtau, chi;
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="p">Параметры</param>
        /// <param name="tt">Тип задачи</param>
        public ABedLoad(TParam p, TypeTask tt)
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
        #region методы предстартовой подготовки задачи
        /// <summary>
        /// Загрузка полей задачи из форматного файла
        /// </summary>
        /// <param name="file">имя файла</param>
        public abstract void LoadData(IDigFunction[] crossFunctions = null);
        #endregion
        #region абстрактные методы 
        /// <summary>
        /// Установка текущего шага по времени
        /// </summary>
        /// <param name="dtime"></param>
        public void SetDTime(double dtime)
        {
            this.dtime = dtime;
        }
        /// <summary>
        /// Установка текущей геометрии расчетной области
        /// </summary>
        /// <param name="mesh">Сетка расчетной области</param>
        /// <param name="Zeta0">начальный уровень дна</param>
        /// <param name="Roughness">шероховатость дна</param>
        /// <param name="BConditions">граничные условия, 
        /// количество в обзем случае определяется через маркеры 
        /// границ сетеи</param>
        public virtual void SetTask(IMesh mesh, double[] Zeta0, double[] Roughness, IBoundaryConditions BConditions)
        {
            eTaskReady = ETaskStatus.CreateMesh;
            this.mesh = mesh;
            this.Zeta0 = Zeta0;
            this.Roughness = Roughness;
            this.Count = mesh.CountKnots;
            this.BConditions = BConditions;
            if (AdhesionFlag == true)
            {
                if (Roughness != null)
                {
                    MEM.Alloc(Zeta0.Length, ref Adhesion);
                    for (int i = 0; i < Zeta0.Length; i++)
                    {
                        double d = Roughness[i] * 0.05;
                        Adhesion[i] = SPhysics.GetAdhesion(d);
                    }
                }
            }
        }

        protected void GigFuntion(IDigFunction[] crossFunctions)
        {
            foreach (var df in crossFunctions)
            {
                if(df.Name == "Roughness")
                {
                    var X = mesh.GetCoords(0);
                    MEM.Alloc(X.Length, ref Roughness);
                    for (int i = 0; i < Roughness.Length; i++)
                    {
                        Roughness[i] = df.FunctionValue(X[i]);
                    }
                }
                if (df.Name == "Adhesion")
                {
                    var X = mesh.GetCoords(0);
                    MEM.Alloc(X.Length, ref Adhesion);
                    for (int i = 0; i < Adhesion.Length; i++)
                    {
                        Adhesion[i] = df.FunctionValue(X[i]);
                    }
                }
            }
        }
        /// <summary>
        /// Вычисление изменений формы донной поверхности 
        /// на одном шаге по времени по модели 
        /// Петрова А.Г. и Потапова И.И. 2014
        /// Реализация решателя - методом контрольных объемов,
        /// Патанкар (неявный дискретный аналог ст 40 ф.3.40 - 3.41)
        /// Коэффициенты донной подвижности, определяются 
        /// как среднее гармонические величины         
        /// </summary>
        /// <param name="Zeta">>возвращаемая форма дна на n+1 итерации</param>
        /// <param name="tau">придонное касательное напряжение</param>
        /// <param name="P">придонное давление</param>
        /// <param name="GloverFlory">флаг расчета критической глуьины по Гловеру и Флори</param>
        public abstract void CalkZetaFDM(ref double[] Zeta, double[] tauX, double[] tauY = null, double[] P = null, double[][] CS = null);
        /// <summary>
        /// Вычисление текущих расходов и их градиентов для построения графиков
        /// </summary>
        /// <param name="Gs">возвращаемые расходы (по механизмам движения)</param>
        /// <param name="dGs">возвращаемые градиенты расходы (по механизмам движения)</param>
        /// <param name="tau">придонное касательное напряжение</param>
        /// <param name="P">придонное давление</param>
        public abstract void Calk_Gs(ref double[][] Gs, ref double[][] dGs, double[] tau, double[] P = null);
        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public abstract IBedLoadTask Clone();
        /// <summary>
        /// Название файла параметров задачи
        /// </summary>
        public abstract string NameBLParams();
        #endregion
    }
}
