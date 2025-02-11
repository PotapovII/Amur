//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 14.01.2025 Потапов И.И.
//---------------------------------------------------------------------------
namespace EddyViscosityLib
{
    using System;
    using System.Linq;

    using CommonLib;
    using CommonLib.Mesh;
    using CommonLib.Physics;
    using CommonLib.EddyViscosity;
    
    using MemLogLib;
    using MeshLib.Wrappers;

    ///// <summary>
    ///// Алгебраическая модель расчета вихревой вязкости
    ///// </summary>
    //[Serializable]
    //public class AlgebraEddyViscosityTri : AbsEddyViscosityTri<BEddyViscosityParam>, IEddyViscosityTri
    //{
    //    /// <summary>
    //    /// Алгебраическая модель
    //    /// </summary>
    //    protected AlgebraEddyViscosityTri task;
    //    /// <summary>
    //    /// Осевая скорость створа/канала
    //    /// </summary>
    //    protected double[] Ux;
    //    /// <summary>
    //    /// Уклон свободной поверхности потока
    //    /// </summary>
    //    protected double J;
    //    /// <summary>
    //    /// Перегруженный метод
    //    /// </summary>
    //    /// <returns></returns>
    //    public new IMesh Mesh()=>mesh;
    //    /// <summary>
    //    /// Врапер
    //    /// </summary>
    //    /// <returns></returns>
    //    public virtual IMWDistance WMesh()
    //    {
    //        var wm = wMesh as IMWDistance;
    //        return wm;
    //    }
    //    /// <summary>
    //    /// Конструктор 
    //    /// </summary>
    //    public AlgebraEddyViscosityTri(ETurbViscType eTurbViscType, BEddyViscosityParam p)
    //        : base(eTurbViscType, p, TypeTask.streamY1D)
    //    {
    //        this.eTurbViscType = eTurbViscType;
    //        this.J = p.J;
    //    }
    //    public override void SetTask(IMesh mesh, IAlgebra algebra, IMeshWrapper wMesh = null)
    //    {
    //        base.SetTask(mesh, algebra, wMesh);
    //        if (wMesh as MWCrossSectionTri != null)
    //            this.wMesh = wMesh as MWCrossSectionTri;
    //        else
    //            this.wMesh = new MWCrossSectionTri(mesh, 1, 0, false);
    //        MEM.Alloc(mesh.CountKnots, ref eddyViscosity);
    //    }
    //    /// <summary>
    //    /// Решение задачи
    //    /// </summary>
    //    /// <param name="result">результат решения</param>
    //    public virtual void SolveTask(ref double[] eddyViscosity, double[] Ux, double J)
    //    {
    //        this.Ux = Ux;
    //        this.J = J;
    //        SolveTask(ref eddyViscosity);
    //    }
    //    public override void SolveTask(ref double[] eddyViscosity, double[] Ux, double[] Vy, double[] Vz, double[] Phi, double[] Vortex, double dt)
    //    {
    //        this.Ux = Ux;
    //        // Определение вязкости
    //        if (eTurbViscType == ETurbViscType.Boussinesq1865)
    //        {
    //            task = new EddyViscosity_Boussinesq1865(eTurbViscType, Params);
    //            task.SetTask(mesh, algebra, wMesh);
    //            task.SolveTask(ref eddyViscosity, Ux, Vy, Vz, Phi, Vortex, dt);
    //        }
    //        else
    //        // Определение вязкости
    //        if (eTurbViscType == ETurbViscType.Leo_C_van_Rijn1984)
    //        {
    //            task = new EddyViscosity_Leo_van_Rijn1984(eTurbViscType, Params);
    //            task.SetTask(mesh, algebra, wMesh);
    //            task.SolveTask(ref eddyViscosity, Ux, Vy, Vz, Phi, Vortex, dt);
    //        }
    //        else
    //        {
    //            SolveTask(ref eddyViscosity);
    //        }
    //    }
    //    public override void SolveTask(ref double[] eddyViscosity)
    //    {
    //        SPhysics.PHYS.turbViscType = eTurbViscType;
    //        SPhysics.PHYS.calkTurbVisc(ref eddyViscosity, TypeTask.streamY1D, (IMWRiver)wMesh, ECalkDynamicSpeed.u_start_U, Ux, J);
    //        this.eddyViscosity = eddyViscosity;
    //    }
    //}
    /// <summary>
    /// Алгебраическая модель расчета вихревой вязкости
    /// </summary>
    [Serializable]
    public abstract class AlgebraEddyViscosityTri : AbsEddyViscosityTri<BEddyViscosityParam>, IEddyViscosityTri
    {
        /// <summary>
        /// Осевая скорость створа/канала
        /// </summary>
        protected double[] Ux;
        /// <summary>
        /// Уклон свободной поверхности потока
        /// </summary>
        protected double J;
        /// <summary>
        /// Конструктор 
        /// </summary>
        public AlgebraEddyViscosityTri(ETurbViscType eTurbViscType, BEddyViscosityParam p)
            : base(eTurbViscType, p, TypeTask.streamY1D)
        {
            this.eTurbViscType = eTurbViscType;
            this.J = p.J;
        }
        /// <summary>
        /// Решение задачи
        /// </summary>
        /// <param name="result">результат решения</param>
        public virtual void SolveTask(ref double[] eddyViscosity, double[] Ux, double J)
        {
            this.Ux = Ux;
            this.J = J;
            SolveTask(ref eddyViscosity);
        }
        public override void SolveTask(ref double[] eddyViscosity, double[] Ux, double[] Vy, double[] Vz, double[] Phi, double[] Vortex, double dt)
        {
            this.Ux = Ux;
            //// Определение вязкости
            //if (eTurbViscType == ETurbViscType.Boussinesq1865)
            //{
            //    task = new EddyViscosity_Boussinesq1865(eTurbViscType, Params);
            //    task.SetTask(mesh, algebra, wMesh);
            //    task.SolveTask(ref eddyViscosity, Ux, Vy, Vz, Phi, Vortex, dt);
            //}
            //else
            //// Определение вязкости
            //if (eTurbViscType == ETurbViscType.Leo_C_van_Rijn1984)
            //{
            //    task = new EddyViscosity_Leo_van_Rijn1984(eTurbViscType, Params);
            //    task.SetTask(mesh, algebra, wMesh);
            //    task.SolveTask(ref eddyViscosity, Ux, Vy, Vz, Phi, Vortex, dt);
            //}
            //else
            //{
            //    SolveTask(ref eddyViscosity);
            //}
        }
        public override void SolveTask(ref double[] eddyViscosity)
        {
            //SPhysics.PHYS.turbViscType = eTurbViscType;
            //SPhysics.PHYS.calkTurbVisc(ref eddyViscosity, TypeTask.streamY1D, (IMWRiver)wMesh, ECalkDynamicSpeed.u_start_U, Ux, J);
            //this.eddyViscosity = eddyViscosity;
        }
    }
}
