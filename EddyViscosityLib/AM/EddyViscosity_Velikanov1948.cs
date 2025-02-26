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
    using CommonLib.Physics;
    using CommonLib.EddyViscosity;
    
    using MemLogLib;
    /// <summary>
    /// Профиль турбулентной вязкости Великанова 1948
    /// </summary>
    [Serializable]
    public class EddyViscosity_Velikanov1948 : AEddyViscosityDistance
    {
        /// <summary>
        /// Конструктор 
        /// </summary>
        public EddyViscosity_Velikanov1948(ETurbViscType eTurbViscType, BEddyViscosityParam p)
            : base(eTurbViscType, p)
        {
        }
        /// <summary>
        /// Определение турбулентной вязкости по модели Великанова 1948
        /// </summary>
        public override void SolveTask(ref double[] eddyViscosity, double[] Ux, double[] Vy, double[] Vz, double[] Phi, double[] Vortex, double dt)
        {
            try
            {
                double u_star;
                switch (Params.u_start)
                {
                    case ECalkDynamicSpeed.u_start_J:
                    case ECalkDynamicSpeed.u_start_M:
                        {
                            for (int node = 0; node < mesh.CountKnots; node++)
                            {
                                double mu_t0 = 0;
                                u_star = Math.Sqrt(GRAV * Hp[node] * J);
                                if (Math.Abs(Hp[node]) > MEM.Error4)
                                {
                                    double xi = Distance[node] / Hp[node];
                                    double ks_b = SPhysics.PHYS.d50 / Hp[node];
                                    double F = Math.Max(0.0, (1 - xi) * (xi + ks_b));
                                    mu_t0 = rho_w * kappa_w * u_star * Hp[node] * F;
                                }
                                eddyViscosity[node] = mu_t0 + mu;
                            }
                        }
                        break;
                    case ECalkDynamicSpeed.u_start_U:
                        {
                            double Area = 0;
                            double Q = wMesh.RiverFlowRate(Ux, ref Area);
                            double U0 = Q / Area;
                            double U_max = Ux.Max();
                            u_star = (U_max - U0) * kappa_w;
                            for (int node = 0; node < mesh.CountKnots; node++)
                            {
                                double mu_t0 = 0;
                                if (Math.Abs(Hp[node]) > MEM.Error4)
                                {
                                    double xi = Distance[node] / Hp[node];
                                    double ks_b = SPhysics.PHYS.d50 / Hp[node];
                                    double F = Math.Max(0.0, (1 - xi) * (xi + ks_b));
                                    mu_t0 = rho_w * kappa_w * u_star * Hp[node] * F;
                                }
                                eddyViscosity[node] = mu_t0 + mu;
                            }
                        }
                        break;
                }
            }
            catch (Exception ee)
            {
                Console.WriteLine("ee = " + ee.ToString() + " " + ee.Message);
            }
        }
    }

}
