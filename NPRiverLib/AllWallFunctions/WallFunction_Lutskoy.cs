//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 12.06.2020 Потапов И.И.
//---------------------------------------------------------------------------
//                      Сбока No Property River Lib
//              с убранными из наследников Property классами 
//---------------------------------------------------------------------------
//               Реверс в NPRiverLib 16.02.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace NPRiverLib.AllWallFunctions
{
    using System;
    [Serializable]
    public class WallFunction_Lutskoy
    {
        /// <summary>
        /// параметр Кармана
        /// </summary>
        public static double kappa = 0.4;
        /// <summary>
        /// параметр трения (безразмерной шероховатости)
        /// </summary>
        public static double E_wall = 9.8;
        /// <summary>
        /// параметр сшифки логарифмического слоя с
        /// переходным слоем
        /// </summary>
        public static double C = -73.50481;
        /// <summary>
        /// кинематическая вязкость воды
        /// </summary>
        public static double nu = 1E-6;
        /// <summary>
        /// плотность воды
        /// </summary>
        public static double rho = 1000;
        /// <summary>
        /// Расчет касательного напряжения (логарифмический слой)
        /// </summary>
        /// <param name="Ux">скорость в ближайшем узле</param>
        /// <param name="y">растояние до узла</param>
        /// <returns>полученное касательное напряжение на стенке</returns>
        public static void Tau(double Ux, double y,ref double tau, ref double yplus)
        {
            double up;
            double Re_yp;
            // сохранение знака
            double S = Math.Sign(Ux);
            Ux = Math.Abs(Ux);
            double Re_bottom = Ux * y / nu;
            double yp = 60;
            // вычисление yp по методу касательных Ньютона
            for (int i = 0; i < 5; i++)
            {
                up = Math.Log(E_wall * yp) / kappa;
                Re_yp = yp * (Math.Log(E_wall * yp) - 1) / kappa + C;
                yp = yp + (Re_bottom - Re_yp) / up;
            }
            double Utau = yp * nu / y;
            tau = S * rho * Utau * Utau;
            // Console.WriteLine(" F = {0},  F0 = {1} yp = {2}, tau ={3}", F, F0, yp, tau);
            yplus = yp;
        }

        /// <summary>
        /// Расчет касательного напряжения (логарифмический слой)
        /// </summary>
        /// <param name="Ux">скорость в ближайшем узле</param>
        /// <param name="y">растояние до узла</param>
        /// <returns>полученное касательное напряжение на стенке</returns>
        public static void Tau_LutskySeverin(double Ux, double y, ref double tau, ref double yplus)
        {
            double up;
            double Re_yp;
            // сохранение знака
            double S = Math.Sign(Ux);
            Ux = Math.Abs(Ux);
            double Re_bottom = Ux * y / nu;
            double yp = 60;
            // вычисление yp по методу касательных Ньютона
            for (int i = 0; i < 5; i++)
            {
                up = Math.Log(E_wall * yp) / kappa;
                Re_yp = yp * (Math.Log(E_wall * yp) - 1) / kappa + C;
                yp = yp + (Re_bottom - Re_yp) / up;
            }
            double Utau = yp * nu / y;
            tau = S * rho * Utau * Utau;
            // Console.WriteLine(" F = {0},  F0 = {1} yp = {2}, tau ={3}", F, F0, yp, tau);
            yplus = yp;
        }

        /// <summary>
        /// Тестовый метод 
        /// </summary>
        //public static void Main()
        //{
        //    double Ux = 0.5;
        //    double y=0.01;
        //    double tau = Tau(Ux, y);
        //    Console.WriteLine(" Ux = {0},  y = {1} tau = {2}", Ux, y, tau);
        //}
    }
}
