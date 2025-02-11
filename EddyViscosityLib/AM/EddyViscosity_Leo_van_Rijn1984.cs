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

    using MemLogLib;

    using CommonLib.Physics;
    using CommonLib.EddyViscosity;
    /// <summary>
    /// Определение турбулентной вязкости по модели Leo_van_Rijn1984
    /// </summary>
    [Serializable]
    public class EddyViscosity_Leo_van_Rijn1984 : AEddyViscosityDistance
    {
        /// <summary>
        /// Конструктор 
        /// </summary>
        public EddyViscosity_Leo_van_Rijn1984(ETurbViscType eTurbViscType, BEddyViscosityParam p)
            : base(eTurbViscType, p)
        {
        }
        /// <summary>
        /// Определение турбулентной вязкости по модели Leo_van_Rijn1984
        /// </summary>
        public override void SolveTask(ref double[] eddyViscosity, double[] Ux, double[] Vy, double[] Vz, double[] Phi, double[] Vortex, double dt)
        {
            try
            {
                switch (Params.u_start)
                {
                    case ECalkDynamicSpeed.u_start_J:
                        {
                            // Ширина живого сечения
                            double Width = wMesh.GetBottom();
                            double Area = wMesh.GetArea();
                            double HR = Area / Width;
                            double u_star = Math.Sqrt(GRAV * HR);
                            double mu_t0 = 0;
                            double Hmax = Hp.Max();
                            for (int node = 0; node < mesh.CountKnots; node++)
                            {
                                u_star = Math.Sqrt(GRAV * Hp[node] * J);
                                if (Math.Abs(Hp[node]) > MEM.Error4)
                                {
                                    //double xi = Distance[node] / Hp[node];
                                    double xi = Distance[node] / Hmax;
                                    xi = Math.Min(xi, 0.5);
                                    double ks_b = SPhysics.PHYS.d50 / Hp[node];
                                    double F = Math.Max(0.0, (1 - xi) * (xi + ks_b));
                                    mu_t0 = rho_w * kappa_w * u_star * Hp[node] * F;
                                }
                                else
                                    mu_t0 = rho_w * kappa_w * u_star * SPhysics.PHYS.d50;
                                eddyViscosity[node] = mu_t0 + mu;
                            }
                        }
                        break;
                    case ECalkDynamicSpeed.u_start_M:
                        {
                            double Width = wMesh.GetBottom();
                            double Area = wMesh.GetArea();
                            double HR = Area / Width;
                            double u_star = Math.Sqrt(GRAV * HR);
                            double mu_t0;
                            for (int node = 0; node < mesh.CountKnots; node++)
                            {
                                if (Math.Abs(Hp[node]) > MEM.Error3)
                                {
                                    double xi = Distance[node] / Hp[node];
                                    xi = Math.Min(xi, 0.5);
                                    double ks_b = SPhysics.PHYS.d50 / Hp[node];
                                    double F = Math.Max(0.0, (1 - xi) * (xi + ks_b));
                                    mu_t0 = rho_w * kappa_w * u_star * Hp[node] * F;
                                }
                                else
                                {
                                    mu_t0 = rho_w * kappa_w * u_star * SPhysics.PHYS.d50;
                                }
                                eddyViscosity[node] = mu_t0 + mu;
                            }
                        }
                        break;
                    case ECalkDynamicSpeed.u_start_U:
                        {
                            //if (wMesh as MWCrossSectionTri != null)
                            //    this.wMesh = wMesh as MWCrossSectionTri;
                            //else
                            //    this.wMesh = new MWCrossSectionTri(mesh, 1, 0, false);
                            //IMWCrossSection wmc = (IMWCrossSection)wMesh;
                            //double[] Us = null;
                            //wmc.CalkBoundary_U_star(Ux, ref Us);
                            //double mu_t0;
                            //for (int node = 0; node < mesh.CountKnots; node++)
                            //{

                            //    double xi = Distance[node] / Hp[node];
                            //    if (xi > 0)
                            //    {
                            //        double u_star = Us[node];
                            //        if (Math.Abs(Hp[node]) > MEM.Error3 && u_star > 0)
                            //        {
                            //            xi = Math.Max(0, Math.Min(xi, 0.5));
                            //            double F = Math.Max(0.0, (1 - xi) * xi);
                            //            mu_t0 = rho_w * kappa_w * u_star * Hp[node] * F;
                            //        }
                            //        else
                            //        {
                            //            mu_t0 = rho_w * kappa_w * u_star * SPhysics.PHYS.d50;
                            //        }
                            //    }
                            //    else
                            //        mu_t0 = 0;
                            //    eddyViscosity[node] = mu_t0 + mu;
                            //}
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
