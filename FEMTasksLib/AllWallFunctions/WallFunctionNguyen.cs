//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//               Реверс в NPRiverLib 20.01.2025 Потапов И.И.
//---------------------------------------------------------------------------
namespace FEMTasksLib.AllWallFunctions
{
    using CommonLib.Physics;
    using GeometryLib;
    using MemLogLib;
    using System;
    /// <summary>
    /// Н.Ш. Нгуен 2021
    /// «Закон стенки» с учетом шероховатости
    /// </summary>
    [Serializable]
    public class WallFunctionNguyen : AWallFunction
    {
        public double u0 = 0;
        protected double ks, Ux, y, tau, y_plus;
        protected double ks_p;
        protected double ur_p;
        protected double y0_p;
        protected double us_p;
        protected double us_pp;

        public string ToString(string F = "F4")
        {
            string str = "Ux = " + Ux.ToString(F) + " " +
                         "y = " + y.ToString(F) + " " +
                         "ks = " + ks.ToString(F) + " " +
                         "tau = " + tau.ToString(F) + " " +
                         "y_plus = " + y_plus.ToString(F) + " ";
            return str;
        }

        public WallFunctionNguyen() : base()
        {

        }

        /// <summary>
        /// Н.Ш. Нгуен «закон стенки» с учетом шероховатости
        /// </summary>
        public override void Tau_wall(ref double y_plus, ref double u_plus, ref double tau, double Ux, double y, double ks = 0)
        {
            this.tau = tau;
            this.Ux = Ux;
            this.ks = ks;
            this.y = y;
            // сохранение знака
            double S = Math.Sign(Ux);
            Ux = Math.Abs(Ux);
            try
            {
                if (CalkR(0.000001) * CalkR(10000) < 0 && mu > 0)
                {
                    y_plus = DMath.RootBisectionMethod(CalkR, 0.0000001, 100000, MEM.Error6);
                    tau = rho * u0 * u0 * S;
                }
                else
                {
                    y_plus = 0;
                    tau = 0;
                    return;
                }
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.Message);
                y_plus = 0;
                tau = 0;
                return;
            }
            this.y_plus = y_plus;
        }
        protected double CalkR(double y_p)
        {
            u0 = y_p * mu / rho / y;
            double ks_p = ks * u0 * rho / mu;
            double ur_p = Ux / u0;
            double y0_p = (0.202 * ks_p + 10.1) * Math.Tanh(Math.Pow(ks_p / 90, 0.55));
            double us_p = Up(y0_p);
            double us_pp = Up(y0_p + y_p);
            double R = ur_p - us_pp + us_p;
            return R;
        }
        /// <summary>
        /// профиль скорости Рейхардта для гладкой пластины
        /// </summary>
        /// <param name="y_p"></param>
        /// <returns></returns>
        protected double Up(double y_p)
        {
            double u_p = Math.Log(1 + kappa * y_p) / kappa +
                         7.8 * (1 - Math.Exp(-y_p / 11) - y_p / 11 * Math.Exp(-0.33 * y_p));
            return u_p;
        }

        public override void tau_wall_test()
        {
            WallFunctionNguyen fw = new WallFunctionNguyen();
            double nu = 0.000001;

            double rho = SPhysics.rho_w;
            double g = SPhysics.GRAV;
            double H = 0.555;
            double J = 0.00036;
            double kappa = SPhysics.kappa_w;
            double ks = 0.0;
            ks = 0.23 / SPhysics.rho_w;
            ks = 0.8 / SPhysics.rho_w;
            //ks = 50.0/1000;

            double C = 5.5;
            double Mu = rho * nu;
            double Tau = rho * g * H * J;
            double u0 = Math.Sqrt(Tau / rho);
            double y = 0.02;
            double Tau_y = rho * g * (H - y) * J;
            double ks_plus = ks * u0 / nu;
            double Y_plus = y * u0 / nu;
            double Ux = u0 / kappa * Math.Log(Y_plus) + C * u0 - u0 / kappa * Math.Log(1 + ks_plus / Math.Exp(3.25 * kappa));
            Console.WriteLine("Ux = {4} Y_plus = {0}, ks_plus = {1} u0 = {2} Tau = {3}, Tau_y = {5}", Y_plus, ks_plus, u0, Tau, Ux, Tau_y);
            double tau = 0;
            double y_plus = 1;
            //fw.Tau_wall(Ux, y, ks, Mu, ref tau, ref y_plus);
            //fw.Tau_wall(ref tau, ref y_plus, ref u_plus, Ux, y);

            Console.WriteLine(fw.ToString("F7") + " u0 = " + fw.u0.ToString("F5"));
        }
    }

}
