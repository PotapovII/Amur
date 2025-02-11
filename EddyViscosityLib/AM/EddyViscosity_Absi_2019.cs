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
    /// Профиль турбулентной вязкости Рафик Абси 2019
    /// </summary>
    [Serializable]
    public class EddyViscosity_Absi_2019 : AEddyViscosityDistance
    {
        /// <summary>
        /// Конструктор 
        /// </summary>
        public EddyViscosity_Absi_2019(ETurbViscType eTurbViscType, BEddyViscosityParam p)
            : base(eTurbViscType, p)
        {
        }
        /// <summary>
        /// Профиль турбулентной вязкости Рафик Абси 2019
        /// </summary>
        public override void SolveTask(ref double[] eddyViscosity, double[] Ux, double[] Vy, double[] Vz, double[] Phi, double[] Vortex, double dt)
        {
            try
            {
                double sVx = Ux.Sum();
                double mHp = Hp.Max();
                double bHp = mHp / SPhysics.PHYS.d50;
                double u_star = 0;

                if (Params.u_start == ECalkDynamicSpeed.u_start_J ||
                    Params.u_start == ECalkDynamicSpeed.u_start_M ||
                    wMesh as IMWCrossSection == null)
                {
                    if (Params.u_start != ECalkDynamicSpeed.u_start_J && sVx > 0 ||
                        MEM.Equals(sVx, 0) != true)
                    {
                        if (bHp > 1)
                            u_star = sVx * kappa_w / Math.Log(bHp);
                    }
                    else
                    {
                        u_star = Math.Sqrt(GRAV * mHp);
                    }
                    for (int node = 0; node < mesh.CountKnots; node++)
                    {
                        if (Params.u_start == ECalkDynamicSpeed.u_start_J && J > MEM.Error7)
                        {
                            if (Hp[node] > MEM.Error4)
                                u_star = Math.Sqrt(GRAV * Hp[node] * J);
                            else
                                u_star = 0;
                        }
                        eddyViscosity[node] = GetMuAbsi_2019(Hp[node], Distance[node], u_star);
                    }
                }
                else
                {
                    double[] Us = null;
                    ((IMWCrossSection)wMesh).CalkBoundary_U_star(Ux, ref Us);
                    for (int node = 0; node < mesh.CountKnots; node++)
                        eddyViscosity[node] = GetMuAbsi_2019(Hp[node], Distance[node], Us[node]);
                }
            }
            catch (Exception ee)
            {
                Console.WriteLine("ee = " + ee.ToString() + " " + ee.Message);
            }
        }
        /// <summary>
        /// Профиль турбулентной вязкости Рафик Абси 2019
        /// </summary>
        public double GetMuAbsi_2019(double H, double D, double u_star)
        {
            double mu_t0 = 0;
            if (u_star > MEM.Error5)
            {
                double xi = D / H;
                //xi = Math.Min(0.5, xi);
                if (xi > MEM.Error5)
                {
                    double Bf = 6;
                    double Re_star = H * u_star / nu;
                    double C1 = 1000;
                    double Z = 0.46 * Re_star - 5.98;
                    if (Z > MEM.Error2)
                    {
                        C1 = Re_star / Z;
                        double Ca = Math.Exp(-(0.34 * Re_star - 11.5) / Z);
                        if (H > MEM.Error4)
                        {
                            mu_t0 = rho_w * u_star * D * Ca * Math.Exp(-C1 * xi) * (1 - Math.Exp(-Bf * Math.Max(0, 1 - xi)));
                        }
                    }
                }
            }
            return mu_t0 + mu;
        }

    }
}
