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
    /// Профиль турбулентной вязкости Рафик Абси 2012
    /// </summary>
    [Serializable]
    public class EddyViscosity_Absi_2012 : AEddyViscosityDistance
    {
        /// <summary>
        /// Конструктор 
        /// </summary>
        public EddyViscosity_Absi_2012(ETurbViscType eTurbViscType, BEddyViscosityParam p, TypeTask tt)
            : base(eTurbViscType, p, tt)
        {
        }
        /// <summary>
        /// Определение турбулентной вязкости по модели Рафика Абси 2012
        /// </summary>
        public override void SolveTask(ref double[] eddyViscosity, double[] Ux, double[] Vy, double[] Vz, double[] Phi, double[] Vortex, double dt)
        {
            try
            {
                switch (Params.u_start)
                {
                    case ECalkDynamicSpeed.u_start_J:
                    case ECalkDynamicSpeed.u_start_M:
                        {
                            double u_star = 0;
                            double mHp = Hp.Max() / SPhysics.PHYS.d50;
                            for (int node = 0; node < mesh.CountKnots; node++)
                            {
                                u_star = 0;
                                if (Hp[node] > MEM.Error4)
                                    u_star = Math.Sqrt(GRAV * Hp[node] * J);
                                eddyViscosity[node] = GetMuAbsi_2012(Hp[node], Distance[node], u_star);
                            }
                        }
                        break;
                    case ECalkDynamicSpeed.u_start_U:
                        {
                            double sVx = Ux.Sum();
                            double mHp = Hp.Max() / SPhysics.PHYS.d50;
                            double u_star = Ux.Max() * kappa_w / Math.Log(mHp);
                            {
                                double[] Us = null;
                                ((IMWCrossSection)wMesh).CalkBoundary_U_star(Ux, ref Us);
                                for (int node = 0; node < mesh.CountKnots; node++)
                                    eddyViscosity[node] = GetMuAbsi_2012(Hp[node], Distance[node], Us[node]);
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
        public double GetMuAbsi_2012(double H, double D, double u_star)
        {
            double mu_t0 = 0;
            if (MEM.Equals(Math.Abs(H), 0) == false && u_star > MEM.Error5)
            {
                double xi = D / H;
                if (xi > MEM.Error5 && MEM.Equals(Math.Abs(u_star), 0) == false)
                {
                    double Re_star = H * u_star / nu;
                    double C1 = 1000;
                    double Z = 0.46 * Re_star - 5.98;
                    if (Z > MEM.Error2)
                    {
                        C1 = Re_star / Z;
                        double Ca = 0;
                        if (Math.Abs(0.46 * Re_star - 5.98) > MEM.Error8)
                            Ca = Math.Exp(-(0.34 * Re_star - 11.5) / Z);
                        if (Ca > 50)
                            Ca = Math.Exp(-Math.Abs((0.34 * Re_star - 11.5) / Z));
                        mu_t0 = rho_w * u_star * D * Ca * Math.Exp(-C1 * xi);
                    }
                }
            }
            return mu_t0 + mu;
        }
    }
}
