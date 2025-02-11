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
    /// Двухслойная модель GLS 1995 : А. В. Гарбарук, Ю. В. Лапин, М. X. Стрелец
    /// </summary>
    [Serializable]
    public class EddyViscosity_GLS_1995 : AEddyViscosityDistance
    {
        /// <summary>
        /// Конструктор 
        /// </summary>
        public EddyViscosity_GLS_1995(ETurbViscType eTurbViscType, BEddyViscosityParam p)
            : base(eTurbViscType, p)
        {
        }
        /// <summary>
        /// Определение турбулентной вязкости по модели 
        /// GLS 1995 : А. В. Гарбарук, Ю. В. Лапин, М. X. Стрелец
        /// </summary>
        public override void SolveTask(ref double[] eddyViscosity, double[] Ux, double[] Vy, double[] Vz, double[] Phi, double[] Vortex, double dt)
        {
            try
            {
                double d50 = SPhysics.PHYS.d50;
                IMWCrossSection wm = (IMWCrossSection)wMesh;
                if (Ux.Sum() == 0 || Params.u_start == ECalkDynamicSpeed.u_start_J ||
                                     Params.u_start == ECalkDynamicSpeed.u_start_M)
                {
                    double u_star = 0;
                    if (Params.u_start != ECalkDynamicSpeed.u_start_J)
                        u_star = Ux.Max() * kappa_w / Math.Log(Hp.Max() / d50);
                    // шероховатость
                    for (int node = 0; node < mesh.CountKnots; node++)
                    {
                        if (Params.u_start == ECalkDynamicSpeed.u_start_J && J > MEM.Error7)
                            u_star = Math.Sqrt(GRAV * Hp[node] * J);
                        double z = Distance[node];
                        double h = Hp[node];
                        eddyViscosity[node] = GLS_1995(z, h, u_star, d50);
                    }
                }
                else
                {
                    double[] Us = null;
                    wm.CalkBoundary_U_star(Ux, ref Us);
                    for (int node = 0; node < mesh.CountKnots; node++)
                    {
                        double u_star = Us[node];
                        double z = Distance[node];
                        double h = Hp[node];
                        eddyViscosity[node] = GLS_1995(z, h, u_star, d50);
                    }
                }
            }
            catch (Exception ee)
            {
                Console.WriteLine("ee = " + ee.ToString() + " " + ee.Message);
            }
        }
        /// <summary>
        /// Двухслойная модель GLS 1995 : А. В. Гарбарук, Ю. В. Лапин, М. X. Стрелец
        /// </summary>
        /// <param name="z"></param>
        /// <param name="h"></param>
        /// <param name="u_star"></param>
        /// <param name="z1"></param>
        /// <returns></returns>
        double GLS_1995(double z, double h, double u_star, double z1)
        {
            double A_vd = 13;
            double xi = z / h;
            double xi3 = xi * xi * xi;
            double zplus = u_star * z / nu;
            // поправка типа ван Дриста
            double VD0 = (1 - Math.Exp(-zplus / A_vd));
            double VD = VD0 * VD0 * VD0;
            // модель турбулентнтной вязкости во внутренней области
            double nu_t_in = kappa_w * u_star * z * VD + nu;
            // толщина пограничного слоя
            double delta0 = -(2 * z1 * Math.Log((z1 + h) / z1) - 2 * z1 * Math.Log(2) + z1 - h) / Math.Log((z1 + h) / z1);
            // коэффициент перемежаемости Клебанова 
            double gamma0 = 1.0 / (1 + 5.5 * (xi3 * xi3));
            // модель турбулентнтной вязкости во внешней области
            double nu_t_out = kappa_w * u_star * delta0 * gamma0 + nu;
            // турбулентная вязкость
            double Mu_t = rho_w * Math.Min(nu_t_in, nu_t_out);
            return Mu_t;
        }

    }

}
