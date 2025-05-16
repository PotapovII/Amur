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

    using CommonLib.Physics;
    using CommonLib.EddyViscosity;
    using CommonLib;
    using System.Linq;
    using MemLogLib;

    /// <summary>
    /// Профиль турбулентной вязкости Караушев 1977
    /// </summary>
    [Serializable]
    public class EddyViscosity_Karaushev1977 : AlgebraEddyViscosityTri
    {
        /// <summary>
        /// Конструктор 
        /// </summary>
        public EddyViscosity_Karaushev1977(ETurbViscType eTurbViscType, BEddyViscosityParam p, TypeTask tt)
            : base(eTurbViscType, p, tt){}
        /// <summary>
        /// Определение турбулентной вязкости по модели Караушева 1977
        /// </summary>
        public override void SolveTask(ref double[] eddyViscosity, double[] Ux, double[] Vy, double[] Vz, double[] Phi, double[] Vortex, double dt)
        {
            try
            {
                double Area = wMesh.GetArea();
                double Bottom = wMesh.GetBottom();
                double R0 = Area / Bottom;
                double Q = wMesh.RiverFlowRate(Ux, ref Area);
                double U0 = Q / Area;
                double mCs = SPhysics.PHYS.Cs(R0) * Math.Sqrt(GRAV);
                double U1 = Math.Sqrt(GRAV * R0 * J);
                double mM = 0.7 * mCs + 1.92 * Math.Sqrt(GRAV);
                double sU = Ux.Sum();
                if (Ux != null && Vy != null && Vz != null && sU>0)
                {
                    for (int node = 0; node < mesh.CountKnots; node++)
                    {
                        double mU = Math.Sqrt(Ux[node] * Ux[node] + Vy[node] * Vy[node] + Vz[node] * Vz[node]);
                        double mu_t0 = rho_w * (mU / U0) * U1 * GRAV * R0 / (mM * mCs);
                        eddyViscosity[node] = mu_t0 + mu;
                    }
                }
                else
                {
                    if( MEM.Equals(sU,0)==true)
                        for (int node = 0; node < mesh.CountKnots; node++)
                        {
                            double mu_t0 = rho_w * U1 * GRAV * R0 / (mM * mCs);
                            eddyViscosity[node] = mu_t0 + mu;
                        }
                    else
                        for (int node = 0; node < mesh.CountKnots; node++)
                        {
                            double mu_t0 = rho_w * (Ux[node] / U0) * U1 * GRAV * R0 / (mM * mCs);
                            eddyViscosity[node] = mu_t0 + mu;
                        }
                }
            }
            catch (Exception ee)
            {
                Console.WriteLine("ee = " + ee.ToString() + " " + ee.Message);
            }
        }
    }
}
