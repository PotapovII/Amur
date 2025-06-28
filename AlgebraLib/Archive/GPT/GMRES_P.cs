namespace AlgebraLib.Archive.GPT
{
    using System;
    using System.Threading.Tasks;

    class GMRES_P
    {
        // Норма вектора (евклидова)
        static double Norm(double[] v)
        {
            double sum = 0;
            Parallel.For(0, v.Length, () => 0.0, (i, state, localSum) =>
            {
                return localSum + v[i] * v[i];
            },
            localSum => { lock (v) sum += localSum; });
            return Math.Sqrt(sum);
        }

        // Скалярное произведение
        static double Dot(double[] v1, double[] v2)
        {
            double sum = 0;
            Parallel.For(0, v1.Length, () => 0.0, (i, state, localSum) =>
            {
                return localSum + v1[i] * v2[i];
            },
            localSum => { lock (v1) sum += localSum; });
            return sum;
        }

        // Матрица-Вектор - ключ к производительности. Можно параллелить по строкам.
        static double[] MatVec(double[][] A, double[] x)
        {
            int n = A.Length;
            double[] result = new double[n];
            Parallel.For(0, n, i =>
            {
                double sum = 0;
                for (int j = 0; j < x.Length; ++j)
                    sum += A[i][j] * x[j];
                result[i] = sum;
            });
            return result;
        }

        // Основной GMRES (без рестартов!)
        public static double[] Solve(double[][] A, double[] b, int m, double tol = 1e-8, int maxIter = 100)
        {
            int n = b.Length;
            double[] x = new double[n];
            double[] r = new double[n];
            double[] Ax = MatVec(A, x);

            Parallel.For(0, n, i => r[i] = b[i] - Ax[i]);
            double beta = Norm(r);

            if (beta < tol)
                return x;

            // Базис подпространства Крылова
            double[][] V = new double[m + 1][];
            for (int i = 0; i < m + 1; ++i)
                V[i] = new double[n];

            // Хессенбергова матрица
            double[][] H = new double[m + 1][];
            for (int i = 0; i < m + 1; ++i)
                H[i] = new double[m];

            double[] e1 = new double[m + 1];
            e1[0] = beta;

            // Первая векторная колонка
            for (int i = 0; i < n; ++i)
                V[0][i] = r[i] / beta;

            for (int k = 0; k < m; ++k)
            {
                // 1. w = A * V[k]
                double[] w = MatVec(A, V[k]);

                // 2. Ортогонализация по Граму-Шмидту
                for (int j = 0; j <= k; ++j)
                {
                    H[j][k] = Dot(w, V[j]);
                    // w -= H[j,k] * V[j]
                    for (int i = 0; i < n; ++i)
                        w[i] -= H[j][k] * V[j][i];
                }
                H[k + 1][k] = Norm(w);

                if (H[k + 1][k] > 1e-14)
                {
                    for (int i = 0; i < n; ++i)
                        V[k + 1][i] = w[i] / H[k + 1][k];
                }
                else
                {
                    // w кратен V[k]; Крыловское подпространство выродилось
                    break;
                }

                // Минимизация невязки можно делать через QR-факторизацию H,
                // но тут для простоты опустим итерационный контроль сходимости
            }

            // Вычисление y из H y = e1
            // Можно использовать least squares (SVD или QR-разложение)
            // Для простоты тут - псевдообратная (можно расширять)

            double[] y = LeastSquaresSolve(H, e1, m);

            // x = x0 + V * y
            for (int j = 0; j < m; ++j)
                for (int i = 0; i < n; ++i)
                    x[i] += V[j][i] * y[j];

            return x;
        }

        // Решение системы наименьших квадратов для H y = e
        // (Быстро, без оптимизации; для больших m используйте специализированные библиотеки)
        static double[] LeastSquaresSolve(double[][] H, double[] e, int m)
        {
            // Простая реализация без QR - только для маленьких m
            double[,] mat = new double[m, m];
            double[] rhs = new double[m];
            for (int i = 0; i < m; ++i)
            {
                for (int j = 0; j < m; ++j)
                    mat[i, j] = H[i][j];
                rhs[i] = e[i];
            }
            // Простой решатель Гаусса
            return GaussianElimination(mat, rhs);
        }

        static double[] GaussianElimination(double[,] A, double[] b)
        {
            int n = b.Length;
            double[] x = new double[n];
            // Только пример! Без выбора главного элемента!
            for (int k = 0; k < n; ++k)
            {
                for (int i = k + 1; i < n; ++i)
                {
                    double factor = A[i, k] / A[k, k];
                    for (int j = k; j < n; ++j)
                        A[i, j] -= factor * A[k, j];
                    b[i] -= factor * b[k];
                }
            }
            for (int i = n - 1; i >= 0; --i)
            {
                x[i] = b[i];
                for (int j = i + 1; j < n; ++j)
                    x[i] -= A[i, j] * x[j];
                x[i] /= A[i, i];
            }
            return x;
        }
    }

    // Пример использования:
    // double[][] A = ...; // Ваша матрица
    // double[] b = ...; // правый столбец
    // var solution = GMRES.Solve(A, b, m: 30, tol: 1e-8);

}
