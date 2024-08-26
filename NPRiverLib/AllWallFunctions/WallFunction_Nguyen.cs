//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 07.05.2021 Потапов И.И.
//---------------------------------------------------------------------------
//                      Сбока No Property River Lib
//              с убранными из наследников Property классами 
//---------------------------------------------------------------------------
//               Реверс в NPRiverLib 16.02.2024 Потапов И.И.
//---------------------------------------------------------------------------

namespace NPRiverLib.AllWallFunctions
{
    using GeometryLib;
    using MemLogLib;
    using System;
    /// <summary>
    /// Н.Ш. Нгуен 2021
    /// «Закон стенки» с учетом шероховатости
    /// </summary>
    public class WallFunction_Nguyen
    {
        /// <summary>
        /// параметр Кармана
        /// </summary>
        public double kappa = 0.41;
        /// <summary>
        /// плотность воды
        /// </summary>
        public static double rho = 1000;
        public double u0 = 0;
        protected double ks, Mu, Ux, Y, tau, yplus;
        protected double ks_p;
        protected double ur_p;
        protected double y0_p;
        protected double us_p;
        protected double us_pp;

        public string ToString(string F = "F4")
        {
            string str = "Ux = " + Ux.ToString(F) + " " +
                         "Y = " + Y.ToString(F) + " " +
                         "ks = " + ks.ToString(F) + " " +
                         "tau = " + tau.ToString(F) + " " +
                         "yplus = " + yplus.ToString(F) + " ";
            return str;
        }
        /// <summary>
        /// Н.Ш. Нгуен
        /// «закон стенки» с учетом шероховатости
        /// </summary>
        /// <param name="Ux"></param>
        /// <param name="y"></param>
        /// <param name="tau"></param>
        /// <param name="yplus"></param>
        public void Tau_Nguyen(double Ux, double Y, double ks, double Mu, ref double _tau, ref double _yplus)
        {
            this.Mu = Mu;
            this.Ux = Ux;
            this.Y = Y;
            this.ks = ks;
            // сохранение знака
            double S = Math.Sign(Ux);
            Ux = Math.Abs(Ux);
            try
            {
                if (CalkR(0.000001) * CalkR(10000) < 0 && Mu > 0)
                {
                    yplus = DMath.RootBisectionMethod(CalkR, 0.0000001, 100000, MEM.Error6);
                    // Console.WriteLine(" F = {0},  F0 = {1} yp = {2}, tau ={3}", F, F0, yp, tau);
                    tau = rho * u0 * u0 * S;
                }
                else
                {
                    _yplus = 0;
                    _tau = 0;
                    return;
                }
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.Message);
                _yplus = 0;
                _tau = 0;
                return;
            }
            _yplus = yplus;
            _tau = tau;
        }
        protected double CalkR(double y_p)
        {
            u0 = y_p * Mu / rho / Y;
            double ks_p = ks * u0 * rho / Mu;
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

        public static void Test_WallFunction_Nguyen()
        {
            WallFunction_Nguyen fw = new WallFunction_Nguyen();
            double nu = 0.000001;

            double rho = 1000;
            double g = 9.81;
            double H = 0.555;
            double J = 0.00036;
            double kappa = 0.41;
            double ks = 0.0;
            ks = 0.23 / 1000;
            ks = 0.8 / 1000;
            //ks = 50.0/1000;

            double C = 5.5;
            double Mu = rho * nu;
            double Tau = rho * g * H * J;
            double u0 = Math.Sqrt(Tau / rho);
            double Y = 0.02;
            double Tau_y = rho * g * (H - Y) * J;
            double ks_plus = ks * u0 / nu;
            double Y_plus = Y * u0 / nu;
            double Ux = u0 / kappa * Math.Log(Y_plus) + C * u0 - u0 / kappa * Math.Log(1 + ks_plus / Math.Exp(3.25 * kappa));
            Console.WriteLine("Ux = {4} Y_plus = {0}, ks_plus = {1} u0 = {2} Tau = {3}, Tau_y = {5}", Y_plus, ks_plus, u0, Tau, Ux, Tau_y);
            double tau = 0;
            double yplus = 1;
            fw.Tau_Nguyen(Ux, Y, ks, Mu, ref tau, ref yplus);

            Console.WriteLine(fw.ToString("F7") + " u0 = " + fw.u0.ToString("F5"));
        }
    }
}
