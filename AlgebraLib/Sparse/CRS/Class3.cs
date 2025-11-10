using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgebraLib.Sparse.CRS
{
    using System;

    public class ILU1
    {
        public static void Test()
        {
            // Пример матрицы и вектора правой части
            double[,] A = {
            { 4, -2, 1, 0 },
            { -2, 4, -2, 1 },
            { 1, -2, 4, -2 },
            { 0, 1, -2,  4 }
            };
            //double[] b = { 11, -16, 17 };
            //double[] X = { 1, 1, 1 };
            double[] b = { 3, 1, 1, 3 };
            //double[] X = { 2, 2, 2 };
            //double[] b = { 6, 0, 6 };

            // Разложение ILU(1)
            var (L, U) = IncompleteLUDecomposition(A);

            Console.WriteLine("Матрица A:");
            PrintMatrix(A);
            Console.WriteLine("Матрица L:");
            PrintMatrix(L);
            Console.WriteLine("Матрица U:");
            PrintMatrix(U);
            
            // Решение СЛАУ с использованием ILU предобуславливания
            double[] x = SolveLU(L, U, b);
            Console.WriteLine("Правая часть:");
            PrintVector(b);
            Console.WriteLine("Решение x:");
            PrintVector(x);
        }

        public static (double[,], double[,]) IncompleteLUDecomposition(double[,] A)
        {
            int n = A.GetLength(0);
            double[,] L = new double[n, n];
            double[,] U = new double[n, n];

            for (int i = 0; i < n; i++)
            {
                U[i, i] = A[i, i];

                for (int j = i + 1; j < n; j++)
                {
                    L[j, i] = A[j, i] / U[i, i]; // Значения для L

                    for (int k = i + 1; k <= j; k++)
                    {
                        A[j, k] -= L[j, i] * U[i, k]; // Обновление A
                    }
                }

                for (int j = i; j < n; j++)
                {
                    U[i, j] = A[i, j]; // Значения для U
                }
            }

            // Установка 1 на диагонали L
            for (int i = 0; i < n; i++)
            {
                L[i, i] = 1;
            }

            return (L, U);
        }

        public static double[] SolveLU(double[,] L, double[,] U, double[] b)
        {
            int n = b.Length;
            double[] y = new double[n];
            double[] x = new double[n];

            // Прямой ход: L * y = b
            for (int i = 0; i < n; i++)
            {
                y[i] = b[i];
                for (int j = 0; j < i; j++)
                {
                    y[i] -= L[i, j] * y[j];
                }
            }

            // Обратный ход: U * x = y
            for (int i = n - 1; i >= 0; i--)
            {
                x[i] = y[i];
                for (int j = i + 1; j < n; j++)
                {
                    x[i] -= U[i, j] * x[j];
                }
                x[i] /= U[i, i]; // Делим на диагональный элемент
            }

            return x;
        }

        public static void PrintMatrix(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Console.Write($"{matrix[i, j]:F2} ");
                }
                Console.WriteLine();
            }
        }

        public static void PrintVector(double[] vector)
        {
            foreach (var value in vector)
            {
                Console.Write($"{value:F2} ");
            }
            Console.WriteLine();
        }
    }
}
