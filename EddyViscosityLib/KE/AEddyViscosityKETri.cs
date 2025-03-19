
//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 14.01.2025 Потапов И.И.
//---------------------------------------------------------------------------
namespace EddyViscosityLib
{
    using CommonLib;
    using CommonLib.Mesh;

    using System;
    using MemLogLib;
    using MeshLib.Wrappers;
    using CommonLib.EddyViscosity;

    /// <summary>
    /// Базовый класс для моделей k - e
    /// </summary>
    [Serializable]
    public abstract class AEddyViscosityKETri : ADiffEddyViscosityTri
    {
        /// <summary>
        /// Кинетическая энергия
        /// </summary>
        public double[] Ken;
        protected double[] Ken_old;
        /// <summary>
        /// Диссипация кинетической энергии
        /// </summary>
        public double[] Eps;
        protected double[] Eps_old;
        /// <summary>
        /// Обертка для сетки позволяет получить список граничных элементов и их связь с КЭ
        /// </summary>
        protected MWDLinkTri mWDLinkTri = null;

        #region Константы модели
        protected const double Sigma_k = 1;
        protected const double Sigma_e = 1.3;
        protected const double C_e1 = 1.44;
        protected const double C_e2 = 1.92;
        protected const double C_mu = 0.09;
        protected double cmu4 = Math.Sqrt(Math.Sqrt(C_mu));
        protected double Eps_wall = 9.8;
        #endregion
        public AEddyViscosityKETri(ETurbViscType eTurbViscType, TypeTask tt, int nLine) 
            : base(eTurbViscType, tt, nLine) { }
        public override void SetTask(IMesh mesh, IAlgebra algebra, IMeshWrapper wMesh = null)
        {
            base.SetTask(mesh, algebra, wMesh);
            MEM.Alloc(mesh.CountKnots, ref Ken);
            MEM.Alloc(mesh.CountKnots, ref Ken_old);
            MEM.Alloc(mesh.CountKnots, ref Eps);
            MEM.Alloc(mesh.CountKnots, ref Eps_old);
            mWDLinkTri = new MWDLinkTri(mesh);
        }
    }

}
