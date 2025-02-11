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
    using MemLogLib;
    using System;
    /// <summary>
    /// Функция стенки по Волков К.Н. 2007. Теплофизика и аэромеханика, 2007, том 14. № 1
    /// </summary>
    [Serializable]
    public class WallFunctionVolkov : AWallFunction
    {
        protected double ExpKB;
        protected double B = 6.3;
        public WallFunctionVolkov(double B = 5.6)
        {
            this.B = B;
            ExpKB = Math.Exp(-kappa * B);
        }
        /// <summary>
        /// Расчет касательного напряжения 
        /// </summary>
        /// <param name="Ux">скорость в ближайшем узле</param>
        /// <param name="y">растояние до узла</param>
        /// <param name="tau"></param>
        /// <param name="y_plus"></param>
        /// <param name="u_plus"></param>
        public override void Tau_wall(ref double y_plus, ref double u_plus, ref double tau, double Ux, double y, double ks = 0)
        {
            double u;
            // сохранение знака
            double S = Math.Sign(Ux);
            Ux = Math.Abs(Ux);
            double Re = Ux * y / nu;
            u = Math.Sqrt(Re);
            // вычисление yp по методу касательных Ньютона
            for (int i = 0; i < 8; i++)
            {
                double u2 = u * u;
                double FF = ExpKB * (1.0 + kappa * u + 0.5 * kappa2 * u2 + kappa3 * u2 * u / 6.0) + Re / u - u;
                double logFF = Math.Log(FF);
                double du = (u - B - logFF * bkappa) / (1.0 - (ExpKB * (kappa + kappa2 * u + kappa3 * u2 / 2.0) - Re / u2 - 1.0) / FF * bkappa);
                u = u - du;
                if (Math.Abs(du) < MEM.Error8)
                    break;
            }
            u_plus = u;
            y_plus = Re / u;
            double Utau = y_plus * nu / y;
            tau = S * rho * Utau * Utau;
        }
        /// <summary>
        /// Пример работы и тест
        /// </summary>
        public override void tau_wall_test()
        {
            IWallFunction fw = new WallFunctionVolkov();
            double Ux = 0.1;
            double y = 0.1;
            double tau = 0;
            double y_plus = 0;
            double u_plus = 0;
            double ctau = 0.0234516615211507;
            double cy_plus = 484.269155750712;
            double cu_plus = 20.6496735983485;

            fw.Tau_wall(ref tau, ref y_plus, ref u_plus, Ux, y);
            Console.WriteLine("U = {0} y = {1} Re = {2}", y, Ux, Ux * y / SPhysics.nu);
            Console.WriteLine("tau = {0} y_plus = {1} u_plus = {2}", tau, y_plus, u_plus);
            // Контроль
            Console.WriteLine("Контрольные данные");
            Console.WriteLine("tau = {0} y_plus = {1} u_plus = {2}", ctau, cy_plus, cu_plus);

            Ux = 0.12;
            y = 0.005;
            ctau = 0.071552008349074;
            cy_plus = 42.2942101087944;
            cu_plus = 14.1863389446595;

            fw.Tau_wall(ref tau, ref y_plus, ref u_plus, Ux, y);
            Console.WriteLine("U = {0} y = {1} Re = {2}", y, Ux, Ux * y / SPhysics.nu);
            Console.WriteLine("tau = {0} y_plus = {1} u_plus = {2}", tau, y_plus, u_plus);
            Console.WriteLine("Контрольные данные");
            Console.WriteLine("tau = {0} y_plus = {1} u_plus = {2}", ctau, cy_plus, cu_plus);

            Ux = 0.02;
            y = 0.003;
            ctau = 0.00733948735668917;
            cy_plus = 8.1274464753822;
            cu_plus = 7.38239251181023;
            fw.Tau_wall(ref tau, ref y_plus, ref u_plus, Ux, y);
            Console.WriteLine("U = {0} y = {1} Re = {2}", y, Ux, Ux * y / SPhysics.nu);
            Console.WriteLine("tau = {0} y_plus = {1} u_plus = {2}", tau, y_plus, u_plus);
            Console.WriteLine("Контрольные данные");
            Console.WriteLine("tau = {0} y_plus = {1} u_plus = {2}", ctau, cy_plus, cu_plus);
            Console.Read();
        }

    }
}
