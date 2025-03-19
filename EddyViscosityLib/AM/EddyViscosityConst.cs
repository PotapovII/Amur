//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 22.02.2025 Потапов И.И.
//---------------------------------------------------------------------------
namespace EddyViscosityLib
{
    using System;
    using System.Linq;

    using CommonLib;
    using CommonLib.Physics;
    using CommonLib.EddyViscosity;
    
    using MemLogLib;
    /// <summary>
    /// Постоянная турбулентная вязкость
    /// </summary>
    [Serializable]
    public class EddyViscosityConst : AlgebraEddyViscosityTri
    {
        /// <summary>
        /// Конструктор 
        /// </summary>
        public EddyViscosityConst(ETurbViscType eTurbViscType, BEddyViscosityParam p, TypeTask tt)
            : base(eTurbViscType, p, tt)
        {
        }
        public override void SolveTask(ref double[] eddyViscosity)
        {
            MEM.Alloc(mesh.CountKnots, ref eddyViscosity);
            for (int node = 0; node < mesh.CountKnots; node++)
                eddyViscosity[node] = Params.mu_const;
        }
        /// <summary>
        /// Определение турбулентной вязкости по модели Буссинеска 1865
        /// </summary>
        public override void SolveTask(ref double[] eddyViscosity, double[] Ux, double[] Vy, double[] Vz, double[] Phi, double[] Vortex, double dt)
        {
            SolveTask(ref eddyViscosity);
        }
    }
}
