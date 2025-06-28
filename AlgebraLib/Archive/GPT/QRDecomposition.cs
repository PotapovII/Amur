namespace AlgebraLib.Archive.GPT
{
    using AlgebraLib.Utils;
    using MemLogLib;
    using System;
    /// <summary>
    /// Алгоритм QR-разложения с применением отражений Хаусхолдера — один 
    /// из наиболее устойчивых и быстрых способов вычисления QR-разложения матрицы. 
    /// Суть метода — поочередно на каждом шаге превращать очередной столбец матрицы
    /// (вниз по диагонали) векторами отражения в вектор с нулями ниже диагонали, 
    /// после чего рекурсивно обрабатывать "нижний подблок".
    /// Результатом служит ортогональная матрица Q и верхнетреугольная R, такие что A = QR.

    // Пошаговый план реализации
    // Пройтись по столбцам j = 0 до min(m, n) - 1, где m — строки, n — столбцы матрицы A.
    // Для каждого столбца j сформировать вектор Хаусхолдера, отражающий столбец к кратному e_j.
    // Обновить A: применить отражение к оставшейся части матрицы(справа и вниз от текущей позиции).
    // Сохранить данные о Хаусхолдерах для последующего построения Q при необходимости.
    /// </summary>
    public class QRDecomposition
    {
        public static void DecomposeQR(double[][] A, out double[][] Q, out double[][] R)
        {
            int m = A.Length;
            int n = A[0].Length;
            Q = null;
            double[] v = null;
            MEM.Alloc(m,ref v);
            MEM.Alloc2DIdentity(m, ref Q);
            double[][] H = null;
            for (int k = 0; k < Math.Min(m, n); k++)
            {
                // 1. Вычисляем вектор Хаусхолдера для столбца k
                double sum = 0.0;
                for (int i = 0; i < m - k; i++)
                    sum += A[k + i][k] * A[k + i][k];
                double normx = Math.Sqrt(sum);
                if (normx == 0.0)
                    continue;
                double alpha = A[k][k] >= 0 ? -normx : normx;
                for (int i = 0; i < m - k; i++)
                    v[i] = A[k + i][k];
                v[0] -= alpha;
                sum = 0.0;
                for (int i = 0; i < m - k; i++)
                    sum += v[i] * v[i];
                double vnorm = Math.Sqrt(sum);
                for (int i = 0; i < m - k; i++)
                    v[i] /= vnorm;
                // 2. Применяем отражение к подматрице A[k:m, k:n]
                for (int j = k; j < n; j++)
                {
                    double dot = 0.0;
                    for (int i = 0; i < m - k; i++)
                        dot += v[i] * A[k + i][ j];

                    for (int i = 0; i < m - k; i++)
                        A[k + i][ j] -= 2 * v[i] * dot;
                }
                // 3. Чтобы построить Q, накапливаем произведение Хаусхолдеров
                MEM.Alloc2DIdentity(m, ref H);
                for (int i = 0; i < m - k; i++)
                    for (int j = 0; j < m - k; j++)
                        H[k + i][k + j] -= 2 * v[i] * v[j];
                Q = MultiplyMatrices(Q, H);
            }
            R = null;
            MEM.Alloc2D(m, n, ref R);
            // R — верхний треугольник A
            for (int i = 0; i < m; i++)
                for (int j = 0; j < n; j++)
                    R[i][j] = (i <= j) ? A[i][j] : 0.0;
        }

        public static void Decompose(double[,] A, out double[,] Q, out double[,] R)
        {
            int m = A.GetLength(0);
            int n = A.GetLength(1);
            Q = CreateIdentityMatrix(m);
            double[,] Awork = (double[,])A.Clone();

            for (int k = 0; k < Math.Min(m, n); k++)
            {
                // 1. Вычисляем вектор Хаусхолдера для столбца k
                double[] x = new double[m - k];
                for (int i = 0; i < x.Length; i++)
                    x[i] = Awork[k + i, k];

                double normx = EuclideanNorm(x);
                if (normx == 0.0)
                    continue;

                double alpha = x[0] >= 0 ? -normx : normx;
                double[] v = (double[])x.Clone();
                v[0] -= alpha;
                double vnorm = EuclideanNorm(v);
                for (int i = 0; i < v.Length; i++)
                    v[i] /= vnorm;

                // 2. Применяем отражение к подматрице Awork[k:m, k:n]
                for (int j = k; j < n; j++)
                {
                    double dot = 0.0;
                    for (int i = 0; i < v.Length; i++)
                        dot += v[i] * Awork[k + i, j];

                    for (int i = 0; i < v.Length; i++)
                        Awork[k + i, j] -= 2 * v[i] * dot;
                }

                // 3. Чтобы построить Q, накапливаем произведение Хаусхолдеров
                double[,] H = CreateIdentityMatrix(m);
                for (int i = 0; i < v.Length; i++)
                    for (int j = 0; j < v.Length; j++)
                        H[k + i, k + j] -= 2 * v[i] * v[j];
                Q = MultiplyMatrices(Q, H);
            }

            // R — верхний треугольник Awork
            R = new double[m, n];
            for (int i = 0; i < m; i++)
                for (int j = 0; j < n; j++)
                    R[i, j] = (i <= j) ? Awork[i, j] : 0.0;
        }

        static double EuclideanNorm(double[] x)
        {
            double sum = 0.0;
            foreach (var xi in x) sum += xi * xi;
            return Math.Sqrt(sum);
        }

        static double[,] CreateIdentityMatrix(int size)
        {
            double[,] I = new double[size, size];
            for (int i = 0; i < size; i++)
                I[i, i] = 1.0;
            return I;
        }

        static double[,] MultiplyMatrices(double[,] A, double[,] B)
        {
            int m = A.GetLength(0);
            int n = B.GetLength(1);
            int p = A.GetLength(1);
            double[,] C = new double[m, n];
            for (int i = 0; i < m; i++)
                for (int j = 0; j < n; j++)
                    for (int k = 0; k < p; k++)
                        C[i, j] += A[i, k] * B[k, j];
            return C;
        }
        static double[][] MultiplyMatrices(double[][] A, double[][] B)
        {
            int m = A.Length;
            int n = B[0].Length;
            int p = A[0].Length;
            double[][] C = null;
            MEM.Alloc2D(m, n, ref C);
            for (int i = 0; i < m; i++)
                for (int j = 0; j < n; j++)
                    for (int k = 0; k < p; k++)
                        C[i][j] += A[i][k] * B[k][j];
            return C;
        }
    }

}
