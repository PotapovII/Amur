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
    using CommonLib;
    using CommonLib.EddyViscosity;
    using CommonLib.Mesh;
    using FEMTasksLib;

    /// <summary>
    /// Модель Derek G.Goring, Jeremy M.Walsh, Peter Rutschmann & Jürg Trösch
    /// Модель Дерек Г.Горинг, Джереми М. Уолш, Питер Ратчманн и Юрг Треш 1997
    /// </summary>
    [Serializable]
    public class EddyViscosity_Derek_G_Goring_and_K_1997 : AEddyViscosityDistance
    {
        СontinuumMechanicsTri MECH = null;
        /// <summary>
        /// Конструктор 
        /// </summary>
        public EddyViscosity_Derek_G_Goring_and_K_1997(ETurbViscType eTurbViscType, BEddyViscosityParam p, TypeTask tt)
            : base(eTurbViscType, p, tt)
        {
        }
        public override void SetTask(IMesh mesh, IAlgebra algebra, IMeshWrapper wMesh = null)
        {
            base.SetTask(mesh, algebra, wMesh);
            MECH = new СontinuumMechanicsTri(wMesh, Params.SigmaTask, Params.RadiusMin);
        }
        /// <summary>
        /// Модель Derek G.Goring, Jeremy M.Walsh, Peter Rutschmann & Jürg Trösch
        /// Модель Дерек Г.Горинг, Джереми М. Уолш, Питер Ратчманн и Юрг Треш 1997
        /// </summary>
        public override void SolveTask(ref double[] eddyViscosity, double[] Ux, double[] Vy, double[] Vz, double[] Phi, double[] Vortex, double dt)
        {
            try
            {
                double[][] tDef = null;
                double[] E2 = null;
                MECH.Calk_tensor_deformations(ref tDef, ref E2, Ux, Vy, Vz);

                // шероховатость
                for (int node = 0; node < mesh.CountKnots; node++)
                {
                    double z = Distance[node];
                    double lm2 = kappa_w * z * kappa_w * z;// * z * Math.Max(0, 1 - z / h);
                    double mu_tn = rho_w * lm2 * E2[node];
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
