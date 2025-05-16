//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 03.02.2025 Потапов И.И.
//---------------------------------------------------------------------------
namespace EddyViscosityLib
{
    using System;
    using System.Collections.Generic;

    using MemLogLib;
    using CommonLib;
    using CommonLib.Mesh;
    using CommonLib.Physics;
    using CommonLib.EddyViscosity;
    using FEMTasksLib.FEMTasks.VortexStream;
    using MemLogLib.Reflection;

    //public interface ITurbViscType
    //{
    //    /// <summary>
    //    /// Тип модели
    //    /// </summary>
    //    ETurbViscType turbViscType { get; set; }
    //}

    /// <summary>
    /// Базовый класс для расчета вихревой вязкости
    /// </summary>
    [Serializable]
    public abstract class AbsEddyViscosityTri<TParam>: AWRAP_FETaskTri 
         where TParam : class, ITProperty<TParam> //, ITurbViscType
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
        protected ETaskStatus eTaskReady;
        /// <summary>
        /// Тип модели
        /// </summary>

        public ETurbViscType turbViscType { get=> eTurbViscType; set=> eTurbViscType= value; }
        protected ETurbViscType eTurbViscType;
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
        // 
        public void SetParams(TParam p)
        {
            Params = p.Clone(p);
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
            eTaskReady = ETaskStatus.LoadParamsData;
        }
        #endregion
        /// <summary>
        /// Тип системы координат 0 - плоскя, 1 - цилиндрическая, 2 - осесимметричная 
        /// </summary>
        public int Sigma { get; set; } = 0;
        /// <summary>
        /// Вихревая вязкость
        /// </summary>
        public double[] eddyViscosity = null;
        /// <summary>
        /// Размерность узла задачи, зависит от модели т.в.
        /// </summary>
        public int Cet_cs() => cs;
        /// <summary>
        /// Тип задачи используется для выбора совместимых подзадач
        /// </summary>
        public TypeTask typeTask { get; }
        /// Шаг по времени
        /// </summary>
        public double dtime { get; set; }
        /// <summary>
        /// Текущее время выполнения задачи
        /// </summary>
        public double time { get; set; }
        /// <summary>
        /// Наименование задачи
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="p">Параметры</param>
        /// <param name="tt">Тип задачи</param>
        public AbsEddyViscosityTri(ETurbViscType eTurbViscType, TParam p, TypeTask tt)
        {
            eTaskReady = ETaskStatus.TaskNoReady;
            this.eTurbViscType = eTurbViscType;
            Name = REFL.GetEnumDescription(eTurbViscType);
            typeTask = tt;
            SetParams(p);
        }
        /// <summary>
        /// Готовность задачи к расчету
        /// </summary>
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
        /// <summary>
        /// версия дата последних изменений интерфейса задачи
        /// </summary>
        public string VersionData() => Version;
        protected string Version = "";
        /// <summary>
        /// Искомые поля задачи которые позволяют сформировать на 
        /// сетке расчетной области краевые условия задачи
        /// </summary>
        public IUnknown[] Unknowns() => unknowns.ToArray();
        protected List<IUnknown> unknowns = new List<IUnknown>();
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="algebra"></param>
        /// <param name="wMesh"></param>
        public override void SetTask(IMesh mesh, IAlgebra algebra, IMeshWrapper wMesh = null)
        {
            MEM.Alloc(mesh.CountKnots, ref eddyViscosity);
            base.SetTask(mesh, algebra, wMesh);
        }
        /// <summary>
        /// Расчет вихревой вязкости 
        /// </summary>
        public abstract void SolveTask(ref double[] eddyViscosity, double[] Ux, double[] Vy, double[] Vz, double[] Phi, double[] Vortex, double dt);
    }
}
