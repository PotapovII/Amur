namespace AlgebraLib.Archive.GPT
{
    using System;
    /// <summary>
    /// Пример ниже написан с расчетом на наглядность и расширяемость, 
    /// без жёсткой оптимизации под sparse, но с местом под замену операций 
    /// над векторами и матрицами вашими нужными реализациями.
    /// </summary>
    public class GMRES_1
    {
        // Функция для основной итерации GMRES
        // A — функция умножения матрицы на вектор
        // b — правый вектор
        // x — начальное приближение
        // restart — переинициализация пространства Крылова (число итераций до рестарта)
        // tol — точность выхода
        // maxIter — максимальное число итераций
        public static void Solve(Func<double[], double[]> A, double[] b,
            double[] x, int restart = 30, double tol = 1e-6, int maxIter = 1000)
        {
            int n = b.Length;
            double[] r = new double[n];
            double[] v = new double[n];
            double beta, resid;
            double[][] V = new double[restart + 1][];
            double[,] H = new double[restart + 1, restart];

            for (int i = 0; i < n; i++)
                r[i] = b[i] - A(x)[i];

            beta = Norm(r);
            resid = beta;
            if (resid < tol)
                return;

            int iter = 0;

            while (iter < maxIter)
            {
                // Начальная ортонормализация
                for (int i = 0; i < n; i++)
                    v[i] = r[i] / beta;
                V[0] = (double[])v.Clone();

                double[] g = new double[restart + 1];
                g[0] = beta;
                for (int j = 0; j < restart && iter < maxIter; j++, iter++)
                {
                    // Арнольди. Формируем новые V и H.
                    var w = A(V[j]);
                    for (int i = 0; i <= j; i++)
                    {
                        H[i, j] = Dot(w, V[i]);
                        for (int k = 0; k < n; k++)
                            w[k] -= H[i, j] * V[i][k];
                    }
                    H[j + 1, j] = Norm(w);
                    if (H[j + 1, j] != 0)
                    {
                        var vnew = new double[n];
                        for (int k = 0; k < n; k++)
                            vnew[k] = w[k] / H[j + 1, j];
                        V[j + 1] = vnew;
                    }
                    // Решаем min||g - H y||. Приведём H к верхнетреугольному виду Гивенсом (можно улучшить QR).
                    // Здесь для простоты сделано вручную обратной подстановкой.
                    // Собираем уравнение размером (j+2)×(j+1)
                    double[][] Hsub = new double[j + 2][];
                    for (int row = 0; row < j + 2; row++)
                    {
                        Hsub[row] = new double[j + 1];
                        for (int col = 0; col < j + 1; col++)
                            Hsub[row][col] = H[row, col];
                    }
                    double[] gsub = new double[j + 2];
                    Array.Copy(g, gsub, j + 2);
                    // Решаем Least Squares (можно встроить QR, но упрощено):
                    var y = LeastSquares(Hsub, gsub);

                    // Обновляем x
                    Array.Clear(x, 0, x.Length); // можно ускорить
                    for (int k = 0; k < y.Length; k++)
                        for (int m = 0; m < n; m++)
                            x[m] += V[k][m] * y[k];

                    // Новая невязка
                    r = A(x);
                    for (int m = 0; m < n; m++)
                        r[m] = b[m] - r[m];
                    resid = Norm(r);

                    if (resid < tol)
                        return;
                }

                // Restart: новое начальное приближение
                for (int m = 0; m < n; m++)
                    r[m] = b[m] - A(x)[m];
                beta = Norm(r);
                resid = beta;
                if (resid < tol)
                    return;
            }
        }


        // 
        static double[] Mult(double[][] A, double[] v)
        {
            double[] R = new double[A.Length];
            for (int i = 0; i < A.Length; i++)
            {
                R[i] = 0;
                for (int j = 0; j < A[i].Length; j++)
                    R[i] += A[i][j] * v[i];
            }
            return R;
        }

        // Евклидова норма вектора
        static double Norm(double[] v)
        {
            double sum = 0;
            for (int i = 0; i < v.Length; i++)
                sum += v[i] * v[i];
            return Math.Sqrt(sum);
        }

        // Скалярное произведение
        static double Dot(double[] a, double[] b)
        {
            double s = 0;
            for (int i = 0; i < a.Length; i++)
                s += a[i] * b[i];
            return s;
        }

        // Решает задачу min ||A x - b|| (в лоб, по сути, через обратную подстановку)
        static double[] LeastSquares(double[][] A, double[] b)
        {
            // Использовать специализированное QR-разложение или SVD для надёжности.
            // Здесь просто решение для маленьких H.
            var ATA = new double[A[0].Length, A[0].Length];
            var ATb = new double[A[0].Length];

            for (int i = 0; i < A[0].Length; i++)
                for (int j = 0; j < A[0].Length; j++)
                    for (int k = 0; k < A.Length; k++)
                        ATA[i, j] += A[k][i] * A[k][j];

            for (int i = 0; i < A[0].Length; i++)
                for (int k = 0; k < A.Length; k++)
                    ATb[i] += A[k][i] * b[k];

            return GaussianElimination(ATA, ATb);
        }

        // Решение СЛАУ методом Гаусса
        static double[] GaussianElimination(double[,] A, double[] b)
        {
            int n = b.Length;
            double[] x = new double[n];
            double[,] M = (double[,])A.Clone();
            double[] v = (double[])b.Clone();

            for (int k = 0; k < n; k++)
            {
                // прямой ход
                double max = Math.Abs(M[k, k]);
                int row = k;
                for (int i = k + 1; i < n; i++)
                {
                    if (Math.Abs(M[i, k]) > max)
                    {
                        max = Math.Abs(M[i, k]);
                        row = i;
                    }
                }
                if (row != k)
                {
                    for (int j = 0; j < n; j++)
                    {
                        double tmp = M[k, j];
                        M[k, j] = M[row, j];
                        M[row, j] = tmp;
                    }
                    double tmp2 = v[k];
                    v[k] = v[row];
                    v[row] = tmp2;
                }

                for (int i = k + 1; i < n; i++)
                {
                    double f = M[i, k] / M[k, k];
                    for (int j = k; j < n; j++)
                        M[i, j] -= f * M[k, j];
                    v[i] -= f * v[k];
                }
            }

            // обратный ход
            for (int i = n - 1; i >= 0; i--)
            {
                x[i] = v[i];
                for (int j = i + 1; j < n; j++)
                    x[i] -= M[i, j] * x[j];
                x[i] /= M[i, i];
            }

            return x;
        }
    }

}
