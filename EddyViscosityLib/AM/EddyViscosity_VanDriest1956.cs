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
    using MeshLib.Wrappers;

    /// <summary>
    /// Алгебраическая модель расчета вихревой вязкости ванн Дрист 1956
    /// </summary>
    [Serializable]
    public class EddyViscosity_VanDriest1956 : AEddyViscosityDistance
    {
        /// <summary>
        /// Конструктор 
        /// </summary>
        public EddyViscosity_VanDriest1956(ETurbViscType eTurbViscType, BEddyViscosityParam p, TypeTask tt)
            : base(eTurbViscType, p, tt)
        {
        }
        /// <summary>
        /// Определение турбулентной вязкости по модели ванн Дриста 1956
        /// </summary>
        public override void SolveTask(ref double[] eddyViscosity, double[] Ux, double[] Vy, double[] Vz, double[] Phi, double[] Vortex, double dt)
        {
            try
            {
                double A_vd = 26;
                IMWCrossSection wm = (IMWCrossSection)wMesh;
                if (Ux.Sum() == 0 || Params.u_start == ECalkDynamicSpeed.u_start_J ||
                                     Params.u_start == ECalkDynamicSpeed.u_start_M)
                {
                    double u_star = 0;
                    if (Params.u_start != ECalkDynamicSpeed.u_start_J)
                        u_star = Ux.Max() * kappa_w / Math.Log(Hp.Max() / SPhysics.PHYS.d50);

                    for (int node = 0; node < mesh.CountKnots; node++)
                    {
                        if (Params.u_start == ECalkDynamicSpeed.u_start_J && J > MEM.Error7)
                        {
                            if (Hp[node] > MEM.Error4)
                                u_star = Math.Sqrt(GRAV * Hp[node] * J);
                            else
                                u_star = 0;
                        }
                        double z = Distance[node];
                        double zplus = u_star * z / nu;
                        double mu_t0 = rho_w * u_star * kappa_w * z * (1 - Math.Exp(-zplus / A_vd));
                        eddyViscosity[node] = mu_t0 + mu;
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
                        double zplus = u_star * z / nu;
                        double mu_t0 = rho_w * u_star * kappa_w * z * (1 - Math.Exp(-zplus / A_vd));
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
