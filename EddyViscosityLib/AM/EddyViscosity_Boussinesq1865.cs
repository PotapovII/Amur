//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 07.02.2025 Потапов И.И.
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
    /// Профиль турбулентной вязкости Буссинеск 1865
    /// </summary>
    [Serializable]
    public class EddyViscosity_Boussinesq1865 : AlgebraEddyViscosityTri
    {
        double[] Vy = null;
        /// <summary>
        /// Конструктор 
        /// </summary>
        public EddyViscosity_Boussinesq1865(ETurbViscType eTurbViscType, BEddyViscosityParam p, TypeTask tt)
            : base(eTurbViscType, p, tt)
        {
        }
        public override void SolveTask(ref double[] eddyViscosity)
        {
            try
            {
                MEM.Alloc(mesh.CountKnots, ref eddyViscosity);
                double mu_t0;
                double a = 22;
                if (typeTask == TypeTask.streamY1D)
                {
                    double Area = wMesh.GetArea();
                    double Bottom = wMesh.GetBottom();
                    double H0 = Area / Bottom;
                    double mCs = SPhysics.PHYS.Cs(H0);
                    double U0;
                    U0 = Math.Sqrt(SPhysics.GRAV * H0 * J) * mCs;
                    mu_t0 = rho_w * U0 * H0 * Math.Sqrt(GRAV) / (2 * a * mCs);
                }
                else
                {
                    double[] Y = mesh.GetCoords(1);
                    double H = Y.Max() - Y.Min();
                    double mCs = SPhysics.PHYS.Cs(H);
                    double U0 = Vy.Max() * mCs;
                    mu_t0 = rho_w * U0 * H * Math.Sqrt(GRAV) / (2 * a * mCs);
                }
                for (int node = 0; node < mesh.CountKnots; node++)
                    eddyViscosity[node] = mu_t0 + mu;
            }
            catch (Exception ee)
            {
                Console.WriteLine("ee = " + ee.ToString() + " " + ee.Message);
            }
        }
        /// <summary>
        /// Определение турбулентной вязкости по модели Буссинеска 1865
        /// </summary>
        public override void SolveTask(ref double[] eddyViscosity, double[] Ux, double[] Vy, double[] Vz, double[] Phi, double[] Vortex, double dt)
        {
            this.Vy = Vy;
            SolveTask(ref eddyViscosity);
        }
    }
}
