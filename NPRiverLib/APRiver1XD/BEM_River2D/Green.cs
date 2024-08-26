//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//            перенесено с правкой : 26.04.2021 Потапов И.И. 
//---------------------------------------------------------------------------
//                      выделен 26.02.2022 Потапов И.И. 
//---------------------------------------------------------------------------
//               Реверс в NPRiverLib 24.07.2024 Потапов И.И.
//---------------------------------------------------------------------------

namespace NPRiverLib.APRiver1XD.BEM_River2D
{
    using System;
    /// <summary>
    /// Функция Грина для 
    /// </summary>
    [Serializable]
    public class Green
    {
        /// <summary>
        /// длина дна
        /// </summary>
        double L;
        double[] fxA;
        double[] fyA;
        double[] fxB;
        double[] fyB;
        double[] detJA;
        double TwoL, Alpha, Log2;
        public void Set(double[] fxA, double[] fyA, double[] fxB, double[] fyB, double[] detJA, double L)
        {
            this.fxA = fxA;
            this.fyA = fyA;
            this.detJA = detJA;
            this.fxB = fxB;
            this.fyB = fyB;
            this.L = L;
            TwoL = 2.0 / L;
            Alpha = Math.PI / L;
            Log2 = Math.Log(2);
        }
        /// <summary>
        /// Расчет значения функции Грина от источника с узлом i в узле j
        /// </summary>
        public double Get(int i, int j)
        {
            if (i == j && fxA.Length == fxB.Length)
            {
                return Math.Log(detJA[i] * TwoL);
            }
            else
            {
                // Вычисление проекций по Х
                double dx = fxA[j] - fxB[i];
                // Вычисление проекций по Y
                double dy = fyA[j] - fyB[i];
                double a = Math.Sinh(Alpha * dy);
                double b = Math.Sin(Alpha * dx);
                double Gij = 0.5 * Math.Log(a * a + b * b) + Log2;
                return Gij;
            }
        }

        /// <summary>
        /// Расчет значения функции Грина от источника с узлом i в узле j
        /// </summary>
        public double Get(double x, double y, int j)
        {
            // Вычисление проекций по Х
            double dx = fxA[j] - x;
            // Вычисление проекций по Y
            double dy = fyA[j] - y;
            double a = Math.Sinh(Alpha * dy);
            double b = Math.Sin(Alpha * dx);
            double Gij = 0.5 * Math.Log(a * a + b * b) + Log2;
            return Gij;
        }
    }
}

