using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgebraLib.Sparse.CRS1
{
    using System;

    public class ILUPreconditioner
    {
        private int size; // Размерность матрицы
        private double[] L_values; // Значения L
        private int[] L_rowPtr; // Указатели на строки L
        private int[] L_colIdx; // Индексы столбцов L

        private double[] U_values; // Значения U
        private int[] U_rowPtr; // Указатели на строки U
        private int[] U_colIdx; // Индексы столбцов U

        public ILUPreconditioner(double[] values, int[] rowPtr, int[] colIdx, int n)
        {
            this.size = n;
            Factorization(values, rowPtr, colIdx);
        }

        private void Factorization(double[] values, int[] rowPtr, int[] colIdx)
        {
            // Инициализация L и U
            L_rowPtr = new int[size + 1];
            L_colIdx = new int[size * size]; // Может потребоваться пересмотреть размер
            L_values = new double[size * size]; // Может потребоваться пересмотреть размер
            U_rowPtr = new int[size + 1];
            U_colIdx = new int[size * size]; // Может потребоваться пересмотреть размер
            U_values = new double[size * size]; // Может потребоваться пересмотреть размер

            for (int i = 0; i < size; i++)
            {
                L_rowPtr[i] = i; // Инициализация указателей строк
                U_rowPtr[i] = i; // Инициализация указателей строк
            }
            L_rowPtr[size] = size;
            U_rowPtr[size] = size;

            for (int i = 0; i < size; i++)
            {
                // Для каждой строки i находим ненулевые элементы
                for (int j = rowPtr[i]; j < rowPtr[i + 1]; j++)
                {
                    int col = colIdx[j];
                    double val = values[j];

                    // ILU(1) будет изменять только ненулевые элементы
                    if (Math.Abs(val) < 1e-12)
                        continue;

                    // Записываем элементы в U
                    U_values[j] = val;

                    if (col < i)
                    {
                        L_colIdx[L_rowPtr[i]] = col;
                        L_values[L_rowPtr[i]] = val;
                        L_rowPtr[i]++;
                    }
                    else if (col == i)
                    {
                        // Диагональные элементы
                        U_colIdx[U_rowPtr[i]] = col;
                        U_values[U_rowPtr[i]] = val;
                        U_rowPtr[i]++;
                    }
                }
            }
        }

        public double[] Solve(double[] b)
        {
            // Решение системы L * y = b
            double[] y = new double[size];
            for (int i = 0; i < size; i++)
            {
                y[i] = b[i];
                for (int j = L_rowPtr[i]; j < L_rowPtr[i + 1]; j++)
                {
                    y[i] -= L_values[j] * y[L_colIdx[j]];
                }
                y[i] /= L_values[L_rowPtr[i] + i];
            }

            // Решение системы U * x = y
            double[] x = new double[size];
            for (int i = size - 1; i >= 0; i--)
            {
                x[i] = y[i];
                for (int j = U_rowPtr[i]; j < U_rowPtr[i + 1]; j++)
                {
                    x[i] -= U_values[j] * x[U_colIdx[j]];
                }
                x[i] /= U_values[U_rowPtr[i] + i];
            }

            return x;
        }
    }
    public class GMRES
    {
        public static double[] Solve(double[] values, int[] rowPtr, int[] colIdx, double[] b, 
                                     int maxIter = 100, double tol = 1e-10)
        {
            int size = b.Length;
            double[] x = new double[size]; // Начальное приближение
            var preconditioner = new ILUPreconditioner(values, rowPtr, colIdx, size);

            for (int iter = 0; iter < maxIter; iter++)
            {
                // Вычисляем остаток r = b - Ax
                double[] r0 = new double[size];
                for (int i = 0; i < size; i++)
                {
                    r0[i] = b[i];
                    for (int j = rowPtr[i]; j < rowPtr[i + 1]; j++)
                    {
                        r0[i] -= values[j] * x[colIdx[j]];
                    }
                }

                // Применяем предобуславливатель
                double[] z = preconditioner.Solve(r0);

                // Начинаем GMRES
                int m = 10; // Размер подпространства
                double[][] V = new double[m + 1][];
                double[] H = new double[(m + 1) * m];
                double[] S = new double[m + 1];
                double[] y = new double[m];

                // Инициализация
                V[0] = Normalized(z);
                for (int i = 0; i < m; i++)
                {
                    V[i + 1] = new double[size];
                }

                // Arnoldi Process
                for (int j = 0; j < m; j++)
                {
                    // V[j] — это вектор базиса
                    // A * V[j]
                    double[] AVj = ApplyMatrix(values, rowPtr, colIdx, V[j]);

                    // Грамм-Шмидт
                    for (int i = 0; i <= j; i++)
                    {
                        H[i + j * (m + 1)] = InnerProduct(AVj, V[i]);
                        for (int k = 0; k < size; k++)
                        {
                            AVj[k] -= H[i + j * (m + 1)] * V[i][k];
                        }
                    }

                    H[j + 1 + j * (m + 1)] = Norm(AVj);
                    if (H[j + 1 + j * (m + 1)] < 1e-12)
                    {
                        // Завершение, если вектор равен нулю
                        break;
                    }

                    V[j + 1] = Normalized(AVj);

                    // Обновление системы
                    for (int i = 0; i <= j; i++)
                    {
                        S[i] = 0;
                    }
                    S[j] = Norm(r0);

                    // Решение системы H * y = S
                    for (int i = j; i >= 0; i--)
                    {
                        for (int k = i + 1; k <= j; k++)
                        {
                            S[i] -= H[i + k * (m + 1)] * y[k];
                        }
                        y[i] = S[i] / H[i + i * (m + 1)];
                    }

                    // Обновление x
                    for (int k = 0; k <= j; k++)
                    {
                        for (int i = 0; i < size; i++)
                        {
                            x[i] += y[k] * V[k][i];
                        }
                    }

                    // Проверка на сходимость
                    double residualNorm = Norm(r0);
                    if (residualNorm < tol)
                    {
                        break;
                    }
                }
            }

            return x;
        }

        private static double[] ApplyMatrix(double[] values, int[] rowPtr, int[] colIdx, double[] x)
        {
            int size = rowPtr.Length - 1;
            double[] result = new double[size];

            for (int i = 0; i < size; i++)
            {
                for (int j = rowPtr[i]; j < rowPtr[i + 1]; j++)
                {
                    result[i] += values[j] * x[colIdx[j]];
                }
            }

            return result;
        }

        private static double[] Normalized(double[] vector)
        {
            double norm = Norm(vector);
            double[] result = new double[vector.Length];
            for (int i = 0; i < vector.Length; i++)
            {
                result[i] = vector[i] / norm;
            }
            return result;
        }

        private static double Norm(double[] vector)
        {
            double sum = 0;
            for (int i = 0; i < vector.Length; i++)
            {
                sum += vector[i] * vector[i];
            }
            return Math.Sqrt(sum);
        }

        private static double InnerProduct(double[] a, double[] b)
        {
            double sum = 0;
            for (int i = 0; i < a.Length; i++)
            {
                sum += a[i] * b[i];
            }
            return sum;
        }

        public static void Test()
        {
            // Пример матрицы: 3x3
            double[] values = { 4, 1, -1, 1, 3, 2, 0, -1, -1 };
            int[] rowPtr = { 0, 3, 6, 9 };
            int[] colIdx = { 0, 1, 2, 0, 1, 2, 1, 2, 0 };
            double[] b = { 3, 7, -2 };

            // Решение системы
            double[] solution = GMRES.Solve(values, rowPtr, colIdx, b);

            // Вывод результата
            Console.WriteLine("Решение:");
            for (int i = 0; i < solution.Length; i++)
            {
                Console.WriteLine($"x[{i}] = {solution[i]:F6}");
            }
        }
    }
}
