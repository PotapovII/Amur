namespace AlgebraLib
{
    // ПРИМЕР генерации на чат gpt куча ошибок )    

    using MemLogLib;
    using System;
    using System.Linq;
    /// <summary>
    /// Метод GMRES (Generalized Minimal Residual) — это итерационный метод для решения 
    /// систем линейных уравнений, особенно когда матрица разреженная или плохо обусловленная. 
    /// Он основан на минимизации остатка на каждом шаге итерации.
    /// Вот пример реализации метода GMRES на C#:
    /// Пояснения к коду:
    /// Этот код является базовой реализацией метода GMRES.
    /// В реальных приложениях могут быть использованы более сложные техники, 
    /// такие как предобуславливание, для улучшения сходимости.
    /// </summary>
    public class GMRES_0
    {
        /// <summary>
        /// Метод Solve — основной метод, который принимает матрицу A, вектор правой части b, 
        /// размер подпространства m, а также параметры tol и maxIter для контроля точности и 
        /// максимального количества итераций.
        /// </summary>
        /// <param name="A"></param>
        /// <param name="b"></param>
        /// <param name="m"></param>
        /// <param name="tol"></param>
        /// <param name="maxIter"></param>
        /// <returns></returns>
        public static double[] Solve(double[,] A, double[] b, int m, double tol = 1e-10, int maxIter = 1000)
        {
            int n = b.Length;
            double[] x = new double[n]; // Начальное приближение
            double[] r = new double[n]; // Остаток
            double[] v = new double[n]; // Векторы
            double[] v1 = new double[n]; // Векторы
            double[,] H = new double[m + 1, m]; // Гессиан
            double[] e1 = new double[m + 1]; // Стандартный базисный вектор
            e1[0] = 1.0;

            for (int iter = 0; iter < maxIter; iter++)
            {
                // Вычисление остатка — на каждом шаге мы вычисляем остаток r и нормируем его.
                // 1. Вычисляем остаток r = b - Ax
                Multiply(A, x, r);
                for (int i = 0; i < n; i++)
                    r[i] = b[i] - r[i];

                // 2. Нормируем остаток
                double beta = Norm(r);
                if (beta < tol)
                    break;

                for (int i = 0; i < n; i++)
                    v[i] = r[i] / beta;

                // 3. Заполняем матрицу H и векторы v
                for (int j = 0; j < m; j++)
                {
                    // v[j] = A * v[j]
                    Multiply(A, v, v);

                    // Ортогонализируем v[j] относительно предыдущих векторов
                    // Ортогонализация — на каждом шаге мы ортогонализируем новый вектор v
                    // относительно предыдущих векторов, используя метод Грама - Шмидта.
                    for (int i = 0; i <= j; i++)
                    {
                        H[i, j] = DotProduct(v, v);
                        for (int k = 0; k < n; k++)
                            v[k] -= H[i, j] * v[k];
                    }

                    H[j + 1, j] = Norm(v);
                    if (H[j + 1, j] < tol)
                        break;

                    for (int k = 0; k < n; k++)
                        v[k] /= H[j + 1, j];
                }
                e1[0] = beta;
                // Решение малой системы — решаем систему H* y = beta * e1,
                // где H — верхняя треугольная матрица, а beta — норма остатка.
                // 4. Решаем малую систему H * y = beta * e1
                //LOG.Print("H", H);
                double[] y = SolveUpperTriangular(H, e1, m);
                //LOG.Print("y", y);
                // 5. Обновляем решение x
                // Обновление решения — обновляем текущее решение x на основе найденного вектора y.
                for (int i = 0; i < m; i++)
                    x[i] += y[i];
            }

            return x;
        }

        private static void Multiply(double[,] A, double[] x, double[] result)
        {
            int rows = A.GetLength(0);
            int cols = A.GetLength(1);
            for (int i = 0; i < rows; i++)
            {
                result[i] = 0;
                for (int j = 0; j < cols; j++)
                    result[i] += A[i, j] * x[j];
            }
        }

        private static double DotProduct(double[] a, double[] b)
        {
            return a.Zip(b, (x, y) => x * y).Sum();
        }

        private static double Norm(double[] x)
        {
            return Math.Sqrt(DotProduct(x, x));
        }

        private static double[] SolveUpperTriangular(double[,] U, double[] b, int n)
        {
            double[] x = new double[n];
            for (int i = n - 1; i >= 0; i--)
            {
                x[i] = b[i];
                for (int j = i + 1; j < n; j++)
                    x[i] -= U[i, j] * x[j];
                x[i] /= U[i, i];
            }
            return x;
        }
    }
}
