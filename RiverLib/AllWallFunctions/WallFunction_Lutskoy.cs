namespace RiverLib.AllWallFunctions
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
        /// <param name="U">скорость в ближайшем узле</param>
        /// <param name="y">растояние до узла</param>
        /// <returns>полученное касательное напряжение на стенке</returns>
        public static void Tau(double U, double y,ref double tau, ref double yplus)
        {
            double up;
            double Re_yp;
            // сохранение знака
            double S = Math.Sign(U);
            U = Math.Abs(U);
            double Re_bottom = U * y / nu;
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
        /// <param name="U">скорость в ближайшем узле</param>
        /// <param name="y">растояние до узла</param>
        /// <returns>полученное касательное напряжение на стенке</returns>
        public static void Tau_LutskySeverin(double U, double y, ref double tau, ref double yplus)
        {
            double up;
            double Re_yp;
            // сохранение знака
            double S = Math.Sign(U);
            U = Math.Abs(U);
            double Re_bottom = U * y / nu;
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
        //    double U = 0.5;
        //    double y=0.01;
        //    double tau = Tau(U, y);
        //    Console.WriteLine(" U = {0},  y = {1} tau = {2}", U, y, tau);
        //}
    }

}
