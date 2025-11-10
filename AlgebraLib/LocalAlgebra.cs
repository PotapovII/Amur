
namespace AlgebraLib
{
    using System;
    using System.Linq;

    public static class LocalAlgebra
    {
        public static void HouseholderDecomposition(ref double[,] A)
        {
            int N = A.GetLength(0);
            double[] Sx = new double[N];
            // DO  I = 1, N-1
            for (int i = 0; i < N; i++)
            {
                // SX - временный массив для вычислений
                double[] sx = new double[N];

                // Вычисляем SX(K)
                for (int k = i; k < N; ++k)
                {
                    sx[k] = A[N - 1, i] * A[N - 1, k]; //A(N,I)*A(N,K), N->N-1
                }

                // Вычисляем SX(K) + A(J, I) * A(J, K) для J < N-1
                for (int j = N - 2; j >= i; --j) // J=N-1 to I, -1. N->N
                {
                    for (int k = i; k < N; ++k) // DO K = I, N
                    { 
                        sx[k] += A[j, i] * A[j, k];   //A(J,I)*A(J,K)
                    }
                }

                double alpha = Math.Sqrt(sx[i]); // SQRT(SX(I))

                if (A[i, i] != 0)  // A(I,I).NE.0.
                {
                    double beta = 1.0 / alpha;    // BETA = 1./ALPHA
                    for (int j = i + 1; j < N; ++j) //J=I+1 to N
                    {
                        A[j, i] *= beta;  // A(J,I)=A(J,I)*BETA
                    }

                    sx[i] = A[i, i] * beta + Math.Sign(A[i, i]); //SX(I) = A(I,I)*BETA+SIGN(1.,A(I,I))
                    A[i, i] = alpha; // A(I,I)=ALPHA

                    double g = 1.0 / Math.Abs(sx[i]);  //G=1./ABS(SX(I))

                    for (int k = i + 1; k < N; ++k)
                    {
                        sx[k] = sx[k] * beta * g + A[i, k] * Math.Sign(sx[i]); // SX(K)=SX(K)*BETA*G+SIGN(A(I,K),SX(I))
                        A[i, k] = A[i, k] + sx[k] * sx[i];  // A(I,K) = A(I,K)+SX(K)*SX(I)

                        for (int j = i + 1; j < N; ++j)
                        {
                            A[j, k] -= A[j, i] * sx[k]; // A (J,K) = A(J,K)-A(J,I)*SX(K)
                        }
                    }
                }
                else
                {
                    if (alpha != 0)
                    {
                        double beta = 1.0 / alpha;  // BETA = 1./ALPHA

                        for (int j = i + 1; j < N; ++j) //J=I+1 to N
                        {
                            A[j, i] *= beta;   // A(J,I)=A(J,I)*BETA
                        }

                        sx[i] = -1.0; // SX(I) = -1.
                        A[i, i] = alpha;  // A(I,I)=ALPHA

                        double g = 1.0; //G=1.! 1/gamma

                        for (int k = i + 1; k < N; ++k)
                        {
                            sx[k] = sx[k] * beta * g + A[i, k] * Math.Sign(sx[i]);// SX(K)=SX(K)*BETA*G+SIGN(A(I,K),SX(I))
                            A[i, k] = A[i, k] + sx[k] * sx[i];  // A(I,K) = A(I,K)+SX(K)*SX(I)

                            for (int j = i + 1; j < N; ++j)
                            {
                                A[j, k] -= A[j, i] * sx[k]; // A (J,K) = A(J,K)-A(J,I)*SX(K)
                            }
                        }
                    }
                    else
                    {
                        sx[i] = 1.0; // SX(I)=1
                        double g = 2.0;  //G=2.

                        for (int k = i + 1; k < N; ++k)
                        {
                            sx[k] = 2.0;   // SX(K)=2.
                            A[i, k] = A[i, k] - sx[k];  //A(I,K) = A(I,K)-SX(K)
                        }
                    }
                }
            }
        }

        public static void TestQRh()
        {
            double[,] matrix = {
                {2, 1, 1},
                {1, -1, 0},
                {0, 1, 2}
            };

            HouseholderDecomposition(ref matrix);
            double[,] transformedMatrix = matrix;

            Console.WriteLine("Transformed Matrix:");
            for (int i = 0; i < transformedMatrix.GetLength(0); ++i)
            {
                for (int j = 0; j < transformedMatrix.GetLength(1); ++j)
                {
                    Console.Write(transformedMatrix[i, j] + " ");
                }
                Console.WriteLine();
            }
        }

        public static double[,] qrDecomposition(double[,] A)
        {
            int n = A.GetLength(0);
            double[,] QR = new double[n, n];
            double[,] R = new double[n, n];

            for (int k = 0; k < n - 1; ++k)
            {
                double[] e = new double[n];
                for (int i = 0; i < n; ++i)
                    e[i] = 0.0;
                e[k] = 1.0;
                for (int i = 0; i < n; ++i)
                {
                    for (int j = 0; j <= k - 1; ++j)
                    {
                        double result = 0.0;
                        for (int ii = 0; ii < n; ++ii)
                            result += A[ii,k] * QR[j,ii];
                        double alpha = -QR[j, i] * result;
                        e[i] += alpha * QR[j, i];
                    }
                }
                double norm = 0.0;
                for (int i = 0; i < n; ++i)
                    norm += Math.Pow(e[i], 2);
                norm = Math.Sqrt(norm);
                for (int i = 0; i < n; ++i)
                {
                    e[i] /= norm;
                    QR[k, i] = e[i];
                }
            }
            return QR;
        }

        private static double[] solve(double[,] QR, double[] b)
        {
            int n = QR.GetLength(0);
            double[] x = new double[n];

            for (int i = 0; i < n; ++i)
                x[i] = 0.0;

            for (int i = n - 1; i >= 0; --i)
            {
                double sum = 0.0;

                for (int j = i + 1; j < n; ++j)
                    sum += QR[i, j] * x[j];

                x[i] = (b[i] - sum) / QR[i, i];
            }

            return x;
        }

        public static double[] SolveQRLinearEquations(double[,] A, double[] b)
        {
            double[,] QR = qrDecomposition(A);
            return solve(QR, b);
        }
        public static void Test()
        {
            double[,] A = new double[2, 2] 
                { { 2.0, 1.0 }, 
                  { 1.0, 3.0 } };
            double[] b = new double[2] { 3, 4 };


            double[] result = LocalAlgebra.SolveQRLinearEquations(A, b);

            foreach (double value in result)
                Console.WriteLine(value);
        }
    }


    public class QRSolver1
    {
        /// <summary>
        /// Решает систему линейных уравнений Ax = b методом QR-разложения.
        /// </summary>
        /// <param name="A">Квадратная матрица коэффициентов (n x n).</param>
        /// <param name="b">Вектор правой части (n x 1).</param>
        /// <returns>Вектор решения x, или null в случае ошибки.</returns>
        public static double[] SolveQR(double[,] A, double[] b)
        {
            int n = A.GetLength(0);

            // Проверка размерности матрицы и вектора
            if (A.GetLength(1) != n || b.Length != n)
            {
                Console.WriteLine("Ошибка: Несоответствие размерностей матрицы и вектора.");
                return null;
            }

            // Создание матриц Q и R
            double[,] Q = new double[n, n];
            double[,] R = new double[n, n];

            // QR-разложение методом Грама-Шмидта
            for (int j = 0; j < n; ++j)
            {
                R[j, j] = A[j, j]; // Диагональные элементы R инициализируются как диагональные элементы A.

                // Вычисление столбца Q для текущего столбца A
                for (int i = 0; i < n; ++i)
                {
                    Q[i, j] = A[i, j]; // Инициализация столбца Q
                }
                double projection = 0;
                // Ортогонализация столбцов Q
                for (int k = 0; k < j; ++k)
                {
                    projection = 0.0;
                    for (int i = 0; i < n; ++i)
                    {
                        projection += Q[i, k] * A[i, j];
                    }

                    // Вычитание проекции на предыдущий ортогональный вектор
                    for (int i = 0; i < n; ++i)
                    {
                        Q[i, j] -= projection * Q[i, k];
                    }
                }


                // Нормализация столбца Q
                double norm = 0.0;
                for (int i = 0; i < n; ++i)
                {
                    norm += Q[i, j] * Q[i, j];
                }
                norm = Math.Sqrt(norm);

                if (norm == 0.0) // Обработка случая нулевого вектора
                {
                    Console.WriteLine("Ошибка: Матрица A сингулярна (столбец Q стал нулевым).");
                    return null;
                }

                for (int i = 0; i < n; ++i)
                {
                    Q[i, j] /= norm;
                }


                // Вычисление элемента R
                R[j, j] = projection; //projection вычислялось в цикле ортогонализации
            }



            // Решение системы Rx = Qtb методом подстановки вперед
            double[] x = new double[n];
            double[] Qtb = new double[n];

            // Вычисление Qtb
            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < n; ++j)
                {
                    Qtb[i] += Q[i, j] * b[j];
                }
            }

            // Подстановка вперед
            for (int i = n - 1; i >= 0; --i)
            {
                x[i] = Qtb[i];
                for (int j = i + 1; j < n; ++j)
                {
                    x[i] -= R[i, j] * x[j];
                }
                x[i] /= R[i, i];
            }

            return x;
        }


        public static void Test()
        {
            // Пример использования:
            double[,] A = {
            { 12, -5,  -6 },
            {-4,   7,  -4 },
            {-6,  -3,  11 }
        };

            double[] b = { 1, -1, 2 };

            double[] x = SolveQR(A, b);

            if (x != null)
            {
                Console.WriteLine("Решение:");
                for (int i = 0; i < x.Length; ++i)
                {
                    Console.WriteLine($"x[{i}] = {x[i]}");
                }
            }
        }

    }



    public class QRSolver
    {
        /// <summary>
        /// Решает систему линейных уравнений Ax = b методом QR-разложения.
        /// </summary>
        /// <param name="A">Квадратная матрица коэффициентов (n x n).</param>
        /// <param name="b">Вектор правой части (n x 1).</param>
        /// <returns>Вектор решения x, или null в случае ошибки.</returns>
        public static double[] SolveQR1(double[,] A, double[] b)
        {
            int n = A.GetLength(0);

            // Проверка размерности матрицы и вектора
            if (A.GetLength(1) != n || b.Length != n)
            {
                Console.WriteLine("Ошибка: Несоответствие размерностей матрицы и вектора.");
                return null;
            }

            // Создание матриц Q и R
            double[,] Q = new double[n, n];
            double[,] R = new double[n, n];

            // QR-разложение методом Грама-Шмидта
            for (int j = 0; j < n; ++j)
            {
                R[j, j] = A[j, j]; // Диагональные элементы R инициализируются как диагональные элементы A.

                // Вычисление столбца Q для текущего столбца A
                for (int i = 0; i < n; ++i)
                {
                    Q[i, j] = A[i, j]; // Инициализация столбца Q
                }

                // Ортогонализация столбцов Q
                double projection=0;
                for (int k = 0; k < j; ++k)
                {
                    projection = 0.0;
                    for (int i = 0; i < n; ++i)
                    {
                        projection += Q[i, k] * A[i, j];
                    }

                    // Вычитание проекции на предыдущий ортогональный вектор
                    for (int i = 0; i < n; ++i)
                    {
                        Q[i, j] -= projection * Q[i, k];
                    }
                }


                // Нормализация столбца Q
                double norm = 0.0;
                for (int i = 0; i < n; ++i)
                {
                    norm += Q[i, j] * Q[i, j];
                }
                norm = Math.Sqrt(norm);

                if (norm == 0.0) // Обработка случая нулевого вектора
                {
                    Console.WriteLine("Ошибка: Матрица A сингулярна (столбец Q стал нулевым).");
                    return null;
                }

                for (int i = 0; i < n; ++i)
                {
                    Q[i, j] /= norm;
                }


                //// Вычисление элемента R с использованием проекции ДО нормализации.
                //R[j, j] = projection;  //Теперь записываем в диагональный элемент R[j,j]

                // Вычисление элемента R с использованием нормализованных значений из Q.
                R[j, j] = Math.Sqrt(norm); // Диагональный элемент равен норме столбца Q
                for (int i = 0; i < j; ++i)
                {
                    double sum = 0;
                    for (int k = 0; k < n; ++k)
                    {
                        sum += Q[k, i] * Q[k, j];
                    }

                    R[i, j] = sum;
                }

            }



            // Решение системы Rx = Qtb методом подстановки вперед
            double[] x = new double[n];
            double[] Qtb = new double[n];

            // Вычисление Qtb
            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < n; ++j)
                {
                    Qtb[i] += Q[i, j] * b[j];
                }
            }

            // Подстановка вперед
            for (int i = n - 1; i >= 0; --i)
            {
                x[i] = Qtb[i];
                for (int j = i + 1; j < n; ++j)
                {
                    x[i] -= R[i, j] * x[j];
                }
                x[i] /= R[i, i];
            }

            return x;
        }

        public static double[] SolveQR(double[,] A, double[] b)
        {
            int n = A.GetLength(0);

            // Проверка размерности матрицы и вектора
            if (A.GetLength(1) != n || b.Length != n)
            {
                Console.WriteLine("Ошибка: Несоответствие размерностей матрицы и вектора.");
                return null;
            }

            // Создание матриц Q и R
            double[,] Q = new double[n, n];
            double[,] R = new double[n, n];

            // QR-разложение методом Грама-Шмидта
            for (int j = 0; j < n; ++j)
            {
                R[j, j] = A[j, j]; // Диагональные элементы R инициализируются как диагональные элементы A.

                // Вычисление столбца Q для текущего столбца A
                for (int i = 0; i < n; ++i)
                {
                    Q[i, j] = A[i, j]; // Инициализация столбца Q
                }

                // Ортогонализация столбцов Q
                double projection = 0;
                for (int k = 0; k < j; ++k)
                {
                    projection = 0.0;
                    for (int i = 0; i < n; ++i)
                    {
                        projection += Q[i, k] * A[i, j];
                    }

                    // Вычитание проекции на предыдущий ортогональный вектор
                    for (int i = 0; i < n; ++i)
                    {
                        Q[i, j] -= projection * Q[i, k];
                    }
                }


                // Нормализация столбца Q
                double norm = 0.0;
                for (int i = 0; i < n; ++i)
                {
                    norm += Q[i, j] * Q[i, j];
                }
                norm = Math.Sqrt(norm);

                if (norm == 0.0) // Обработка случая нулевого вектора
                {
                    Console.WriteLine("Ошибка: Матрица A сингулярна (столбец Q стал нулевым).");
                    return null;
                }

                for (int i = 0; i < n; ++i)
                {
                    Q[i, j] /= norm;
                }


                // Вычисление элемента R с использованием проекции ДО нормализации.  Этот кусок закомментирован, а правильный расчет ниже.

                // Вычисление элемента R с использованием нормализованных значений из Q.
                R[j, j] = Math.Sqrt(norm); // Диагональный элемент равен норме столбца Q
                for (int i = 0; i < j; ++i)
                {
                    double sum = 0;
                    for (int k = 0; k < n; ++k)
                    {
                        sum += Q[k, i] * Q[k, j];
                    }

                    R[i, j] = sum;
                }

            }



            // Решение системы Rx = Qtb методом подстановки вперед
            double[] x = new double[n];
            double[] Qtb = new double[n];

            // Вычисление Qtb
            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < n; ++j)
                {
                    Qtb[i] += Q[i, j] * b[j];
                }
            }

            // Подстановка вперед
            for (int i = n - 1; i >= 0; --i)
            {
                x[i] = Qtb[i];
                for (int j = i + 1; j < n; ++j)
                {
                    x[i] -= R[i, j] * x[j];
                }
                x[i] /= R[i, i];
            }

            return x;
        }


        public static void Test()
        {
            // Пример использования:
            //double[,] A = {
            //{ 12, -5,  -6 },
            //{-4,   7,  -4 },
            //{-6,  -3,  11 }};

            double[,] A = {
            { 2, -1,  0 },
            {-1,  2,  -1 },
            {0,  -1,  2 }};

            double[] b = { 1, 0, 1 };

            double[] x = SolveQR(A, b);

            if (x != null)
            {
                Console.WriteLine("Решение:");
                for (int i = 0; i < x.Length; ++i)
                {
                    Console.WriteLine($"x[{i}] = {x[i]}");
                }
            }
        }

    }


}
