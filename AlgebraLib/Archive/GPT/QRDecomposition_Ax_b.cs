using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgebraLib.Archive.GPT
{
    using System;
    /// <summary>
    /// Рассмотрим решение задачи по шагам: сначала реализуем 
    /// QR-разложение матрицы с помощью отражений Хаусхолдера, 
    /// затем используем данное разложение для решения системы
    /// линейных алгебраических уравнений ( Ax = b ) на языке C#.
    /// </summary>
    public class QRDecomposition_Ax_b
    {
        /// <summary>
        /// 1. QR-разложение методом отражений Хаусхолдера
        /// На каждом шаге к i-му столбцу применяется матрица отражения 
        /// (матрица Хаусхолдера), обнуляющая все элементы ниже диагонального. 
        /// Таким образом, ( A ) раскладывается в ( QR ), где ( Q ) — ортогональная, ( R ) — верхне-треугольная.
        /// 2. Решение СЛАУ Пусть(A = QR), получим(QRx = b) ⇒ (Rx = Q^T b ).
        /// Решаем треугольную систему обратной подстановкой.
        /// </summary>
        /// <param name="A"></param>
        /// <param name="Q"></param>
        /// <param name="R"></param>
        public static void HouseholderQR(double[,] A, out double[,] Q, out double[,] R)
        {
            int m = A.GetLength(0);
            int n = A.GetLength(1);
            R = (double[,])A.Clone();
            Q = CreateIdentity(m);

            for (int k = 0; k < Math.Min(m, n); k++)
            {
                // Построение вектора отражения
                double norm = 0;
                for (int i = k; i < m; i++)
                    norm += R[i, k] * R[i, k];
                norm = Math.Sqrt(norm);

                if (norm == 0) continue; // Пропустить итерацию

                double[] v = new double[m];
                for (int i = 0; i < m; i++)
                    v[i] = (i < k) ? 0 : R[i, k];
                v[k] += (R[k, k] >= 0) ? norm : -norm;

                // Нормализация вектора v
                double vnorm = 0;
                for (int i = k; i < m; i++)
                    vnorm += v[i] * v[i];
                vnorm = Math.Sqrt(vnorm);

                if (vnorm == 0) continue;
                for (int i = k; i < m; i++)
                    v[i] /= vnorm;

                // Обновление R = H * R
                for (int j = k; j < n; j++)
                {
                    double s = 0;
                    for (int i = k; i < m; i++)
                        s += v[i] * R[i, j];
                    for (int i = k; i < m; i++)
                        R[i, j] -= 2 * v[i] * s;
                }

                // Обновление Q = Q * H^T (H — симметрична)
                for (int i = 0; i < m; i++)
                {
                    double s = 0;
                    for (int j = k; j < m; j++)
                        s += v[j] * Q[i, j];
                    for (int j = k; j < m; j++)
                        Q[i, j] -= 2 * v[j] * s;
                }
            }

            // Q получена транспонированной (Q^T), транспонируем
            Q = Transpose(Q);
        }

        public static double[] Solve(double[,] A, double[] b)
        {
            HouseholderQR(A, out var Q, out var R);
            double[] y = MultiplyMatrixVector(Transpose(Q), b);
            return BackSubstitution(R, y);
        }

        // Обратная подстановка для верхней треугольной матрицы
        public static double[] BackSubstitution(double[,] R, double[] y)
        {
            int n = R.GetLength(1);
            double[] x = new double[n];
            for (int i = n - 1; i >= 0; i--)
            {
                double s = y[i];
                for (int j = i + 1; j < n; j++)
                    s -= R[i, j] * x[j];
                x[i] = s / R[i, i];
            }
            return x;
        }

        // Утилиты для работы с матрицами и векторами
        static double[,] CreateIdentity(int n)
        {
            double[,] I = new double[n, n];
            for (int i = 0; i < n; i++)
                I[i, i] = 1.0;
            return I;
        }

        public static double[,] Transpose(double[,] A)
        {
            int m = A.GetLength(0);
            int n = A.GetLength(1);
            double[,] AT = new double[n, m];
            for (int i = 0; i < m; i++)
                for (int j = 0; j < n; j++)
                    AT[j, i] = A[i, j];
            return AT;
        }

        public static double[] MultiplyMatrixVector(double[,] A, double[] x)
        {
            int m = A.GetLength(0), n = A.GetLength(1);
            if (n != x.Length) throw new ArgumentException("Matrix and vector size mismatch");
            double[] b = new double[m];
            for (int i = 0; i < m; i++)
                for (int j = 0; j < n; j++)
                    b[i] += A[i, j] * x[j];
            return b;
        }
    }



}
