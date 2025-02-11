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
    ///  Модель Потапова И И. 2024
    /// </summary>
    [Serializable]
    public class EddyViscosity_PotapovII_2024 : AEddyViscosityDistance
    {
        СontinuumMechanicsTri MECH = null;
        /// <summary>
        /// Конструктор 
        /// </summary>
        public EddyViscosity_PotapovII_2024(ETurbViscType eTurbViscType, BEddyViscosityParam p)
            : base(eTurbViscType, p)
        {
        }
        public override void SetTask(IMesh mesh, IAlgebra algebra, IMeshWrapper wMesh = null)
        {
            base.SetTask(mesh, algebra, wMesh);
            MECH = new СontinuumMechanicsTri(wMesh, Params.SigmaTask, Params.RadiusMin);
        }

        /// <summary>
        ///  Модель Потапова И И. 2024
        /// </summary>
        public override void SolveTask(ref double[] eddyViscosity, double[]Ux, double[] Vy, double[] Vz, double[] Phi, double[] Vortex, double dt)
        {
            try
            {
                double[][] tDef = null;
                double[] E2 = null;
                MECH.Calk_tensor_deformations(ref tDef, ref E2, Ux, Vy, Vz);
                for (int node = 0; node < mesh.CountKnots; node++)
                {
                    double z = Distance[node];
                    double lm2 = kappa_w * z * kappa_w * z;
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
