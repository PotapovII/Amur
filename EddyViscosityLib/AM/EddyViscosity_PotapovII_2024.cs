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
        double h;
        /// <summary>
        /// Конструктор 
        /// </summary>
        public EddyViscosity_PotapovII_2024(ETurbViscType eTurbViscType, BEddyViscosityParam p, TypeTask tt)
            : base(eTurbViscType, p, tt)
        {
        }
        public override void SetTask(IMesh mesh, IAlgebra algebra, IMeshWrapper wMesh = null)
        {
            base.SetTask(mesh, algebra, wMesh);
            MECH = new СontinuumMechanicsTri(wMesh, Params.SigmaTask, Params.RadiusMin);
            double min = 0;
            double max = 0;
            mesh.MinMax(1,ref min,ref max);
            h = (max - min)/2;
        }


        /// <summary>
        ///  Модель Потапова И И. 2025
        /// </summary>
        public override void SolveTask(ref double[] eddyViscosity, double[] Ux, double[] Vy, double[] Vz, double[] Phi, double[] Vortex, double dt)
        {
            try
            {
                double[][] tDef = null;
                double[] E2 = null;
                MECH.Calk_tensor_deformations(ref tDef, ref E2, Ux, Vy, Vz);
                double mu_h = rho_w * kappa_w * h * kappa_w * h;
                for (int node = 0; node < mesh.CountKnots; node++)
                {
                    double mu_tn = mu_h * E2[node];
                    eddyViscosity[node] = mu_tn + mu;
                }
            }
            catch (Exception ee)
            {
                Console.WriteLine("ee = " + ee.ToString() + " " + ee.Message);
            }
        }

        /// <summary>
        /// Расчет вихревой вязкости 
        /// </summary>
        /// <param name="eddyViscosity">вихревая вязкость</param>
        /// <param name="Ux">осевая/окружная/осевая/скорость потока</param>
        /// <param name="dt">шаг по времени</param>
        public override void SolveTask(ref double[] eddyViscosity, double[] Ux, double dt)
        {
            throw new NotImplementedException("Модель вязкость не поддерживается для одномерного потока");
        }
    }

}
