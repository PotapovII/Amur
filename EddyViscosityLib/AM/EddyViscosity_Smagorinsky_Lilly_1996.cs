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
    using CommonLib.EddyViscosity;
    using CommonLib.Mesh;
    using FEMTasksLib;

    /// <summary>
    /// Модель Смагоринского-Лилли 0.17 < Cs< 0.21 (Lilly, 1996)
    /// </summary>
    [Serializable]
    public class EddyViscosity_Smagorinsky_Lilly_1996 : AlgebraEddyViscosityTri
    {
        СontinuumMechanicsTri MECH = null;
        /// <summary>
        /// Конструктор 
        /// </summary>
        public EddyViscosity_Smagorinsky_Lilly_1996(ETurbViscType eTurbViscType, BEddyViscosityParam p, TypeTask tt)
            : base(eTurbViscType, p, tt)
        {
        }
        public override void SetTask(IMesh mesh, IAlgebra algebra, IMeshWrapper wMesh = null)
        {
            base.SetTask(mesh, algebra, wMesh);
            MECH = new СontinuumMechanicsTri(wMesh, Params.SigmaTask, Params.RadiusMin);
        }
        /// <summary>
        /// Модель Смагоринского-Лилли 0.17 < Cs< 0.21 (Lilly, 1996)
        /// </summary>
        public override void SolveTask(ref double[] eddyViscosity, double[] Ux, double[] Vy, double[] Vz, double[] Phi, double[] Vortex, double dt)
        {
            try
            {
                double[][] tDef = null;
                double[] E2 = null;
                MECH.Calk_tensor_deformations(ref tDef, ref E2, Ux, Vy, Vz);
                double Csm2 = 0.04;
                // площади Ко в окрестности узла
                double[] Se = wMesh.GetElemS();
                double K = 0.000125 / Se.Max();
                Csm2 *= K;
                // шероховатость
                for (int node = 0; node < mesh.CountKnots; node++)
                {
                    double mu_tn = rho_w * Csm2 * Se[node] * E2[node];
                    mu_tn = Math.Max(mu_tn, 1e-4);
                    eddyViscosity[node] = mu_tn + mu;
                }
            }
            catch (Exception ee)
            {
                Console.WriteLine("ee = " + ee.ToString() + " " + ee.Message);
            }
        }
    }
}
