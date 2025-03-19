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
    /// <summary>
    /// Профиль турбулентной вязкости Прандтль 1934
    /// </summary>
    [Serializable]
    public class EddyViscosity_Prandtl1934 : AEddyViscosityDistance
    {
        /// <summary>
        /// Конструктор 
        /// </summary>
        public EddyViscosity_Prandtl1934(ETurbViscType eTurbViscType, BEddyViscosityParam p, TypeTask tt)
            : base(eTurbViscType, p, tt)
        {
        }
        /// <summary>
        /// Определение турбулентной вязкости по модели Прандтля 1934
        /// </summary>
        public override void SolveTask(ref double[] eddyViscosity, double[] Ux, double[] Vy, double[] Vz, double[] Phi, double[] Vortex, double dt)
        {
            try
            {
                IMesh mesh = wMesh.GetMesh();
                double u_star = 0;
                if (Params.u_start == ECalkDynamicSpeed.u_start_U)
                {
                    u_star = Ux.Max() * kappa_w / Math.Log(Hp.Max() / SPhysics.PHYS.ks);
                    for (int node = 0; node < mesh.CountKnots; node++)
                    {
                        double mu_t0 = 0;
                        if (MEM.Equals(Math.Abs(Hp[node]), 0) == false)
                        {
                            double xi = Distance[node] / Hp[node];
                            double F = Math.Max(0.0, (1 - xi) * xi);
                            mu_t0 = rho_w * kappa_w * u_star * Hp[node] * F;
                        }
                        eddyViscosity[node] = mu_t0 + mu;
                    }
                }
                else
                {
                    for (int node = 0; node < mesh.CountKnots; node++)
                    {
                        u_star = Math.Sqrt(GRAV * Hp[node] * J);
                        double mu_t0 = 0;
                        if (MEM.Equals(Math.Abs(Hp[node]), 0) == false)
                        {
                            double xi = Distance[node] / Hp[node];
                            double F = Math.Max(0.0, (1 - xi) * xi);
                            mu_t0 = rho_w * kappa_w * u_star * Hp[node] * F;
                        }
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
