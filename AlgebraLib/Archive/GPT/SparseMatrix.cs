namespace AlgebraLib
{
    using System;
    using System.Collections.Generic;
    /// <summary>
    /// Класс для хранения разреженной матрицы с использованием словаря, 
    /// что позволяет эффективно хранить только ненулевые элементы.
    /// </summary>
    public class SparseMatrixDemo
    {
        private Dictionary<int, Dictionary<int, double>> matrix;

        public SparseMatrixDemo()
        {
            matrix = new Dictionary<int, Dictionary<int, double>>();
        }

        public void AddValue(int row, int col, double value)
        {
            if (!matrix.ContainsKey(row))
            {
                matrix[row] = new Dictionary<int, double>();
            }
            matrix[row][col] = value;
        }

        public double GetValue(int row, int col)
        {
            if (matrix.ContainsKey(row) && matrix[row].ContainsKey(col))
            {
                return matrix[row][col];
            }
            return 0.0; // Значение по умолчанию для отсутствующих элементов
        }

        public int RowCount => matrix.Count;

        public IEnumerable<KeyValuePair<int, Dictionary<int, double>>> Rows => matrix;
    }
    /// <summary>
    /// Основной класс для реализации метода GMRES. Он принимает разреженную матрицу, вектор правой части и параметры для итераций.
    /// </summary>
    public class SparseGMRES
    {
        private SparseMatrixDemo matrix;
        private double[] b;
        private int maxIterations;
        private double tolerance;

        public SparseGMRES(SparseMatrixDemo matrix, double[] b, int maxIterations = 100, double tolerance = 1e-10)
        {
            this.matrix = matrix;
            this.b = b;
            this.maxIterations = maxIterations;
            this.tolerance = tolerance;
        }

        public double[] Solve(double[] x0)
        {
            int n = matrix.RowCount;
            double[] x = (double[])x0.Clone();
            double[] r = Subtract(b, Multiply(matrix, x));
            double beta = Norm(r);
            double[] v0 = Normalize(r);
            double[][] V = new double[n][];
            double[] h = new double[n];
            double[] e1 = new double[n];
            e1[0] = 1.0;

            for (int k = 0; k < maxIterations; k++)
            {
                V[k] = v0;

                // Arnoldi процесс
                for (int j = 0; j < k; j++)
                {
                    h[j] = Dot(V[j], Multiply(matrix, V[k]));
                    for (int i = 0; i < n; i++)
                    {
                        V[k][i] -= h[j] * V[j][i];
                    }
                }

                h[k] = Norm(V[k]);
                if (h[k] < tolerance) break;

                for (int i = 0; i < n; i++)
                {
                    V[k][i] /= h[k];
                }

                // Решение маленькой системы
                // Здесь следует реализовать решение системы H * y = beta * e1
                // ...
            }

            return x; // Возвращаем решение
        }
        /// <summary>
        ///  Умножает разреженную матрицу на вектор.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="vector"></param>
        /// <returns></returns>
        private double[] Multiply(SparseMatrixDemo matrix, double[] vector)
        {
            double[] result = new double[matrix.RowCount];
            foreach (var row in matrix.Rows)
            {
                int rowIndex = row.Key;
                foreach (var col in row.Value)
                {
                    result[rowIndex] += col.Value * vector[col.Key];
                }
            }
            return result;
        }
        /// <summary>
        /// Вычитает два вектора.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private double[] Subtract(double[] a, double[] b)
        {
            double[] result = new double[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                result[i] = a[i] - b[i];
            }
            return result;
        }
        /// <summary>
        /// Вычисляет норму вектора.
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        private double Norm(double[] vector)
        {
            double sum = 0.0;
            for (int i = 0; i < vector.Length; i++)
            {
                sum += vector[i] * vector[i];
            }
            return Math.Sqrt(sum);
        }
        /// <summary>
        /// Нормализует вектор.
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        private double[] Normalize(double[] vector)
        {
            double norm = Norm(vector);
            double[] result = new double[vector.Length];
            for (int i = 0; i < vector.Length; i++)
            {
                result[i] = vector[i] / norm;
            }
            return result;
        }
        /// <summary>
        ///  Вычисляет скалярное произведение двух векторов.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private double Dot(double[] a, double[] b)
        {
            double result = 0.0;
            for (int i = 0; i < a.Length; i++)
            {
                result += a[i] * b[i];
            }
            return result;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            SparseMatrixDemo matrix = new SparseMatrixDemo();
            // Заполнение матрицы здесь
            double[] b = { /* Вектор правой части */ };
            double[] x0 = new double[b.Length]; // Начальное приближение

            SparseGMRES solver = new SparseGMRES(matrix, b);
            double[] solution = solver.Solve(x0);

            Console.WriteLine("Решение:");
            foreach (var value in solution)
            {
                Console.WriteLine(value);
            }
        }
    }

}
