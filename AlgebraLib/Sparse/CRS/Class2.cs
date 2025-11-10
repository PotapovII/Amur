using MemLogLib;
using System;

namespace AlgebraLib.Sparse.CRS
{
    public class CSRMatrix
    {
        public int[] RowPointers { get; private set; }
        public int[] ColumnIndices { get; private set; }
        public double[] Values { get; private set; }
        public int Rows { get; private set; }
        public int NonZeros { get; private set; }

        public CSRMatrix(int[] rowPointers, int[] columnIndices, double[] values)
        {
            Rows = rowPointers.Length - 1;
            RowPointers = rowPointers;
            ColumnIndices = columnIndices;
            Values = values;
            NonZeros = values.Length;
        }
        public void Print()
        {
            Console.WriteLine("SparseMatrixCSR");
            double[] buff = new double[Rows];
            for (int i = 0; i < Rows; i++)
            {
                for (int k = RowPointers[i]; k < RowPointers[i + 1]; k++)
                    buff[ColumnIndices[k]] = Values[k];
                LOG.Print(i.ToString() + " row: ", buff);
                for (int k = RowPointers[i]; k < RowPointers[i + 1]; k++)
                    buff[ColumnIndices[k]] = 0;
            }
        }
    }

    public class ILUPreconditioner
    {
        public CSRMatrix L;
        public CSRMatrix U;

        public ILUPreconditioner(CSRMatrix A)
        {
            Factorize(A);
        }

        private void Factorize(CSRMatrix A)
        {
            int n = A.Rows;

            // Инициализация матриц L и U
            int[] rowPointersL = new int[n + 1];
            int[] rowPointersU = new int[n + 1];
            // Максимальная оценка числа ненулевых элементов (примитивная)
            int maxNNZ = A.NonZeros;
            int[] columnIndicesL = new int[maxNNZ];
            double[] valuesL = new double[maxNNZ];
            int[] columnIndicesU = new int[maxNNZ];
            double[] valuesU = new double[maxNNZ];

            int currentL = 0, currentU = 0;

            // ILU(k) разложение
            for (int i = 0; i < n; i++)
            {
                for (int j = A.RowPointers[i]; j < A.RowPointers[i + 1]; j++)
                {
                    int aCol = A.ColumnIndices[j];
                    double aVal = A.Values[j];
                    if (aCol < i) // для матрицы L
                    {
                        int lIndex = FindIndex(columnIndicesL, 0, currentL, aCol);
                        double sumL = 0.0;
                        if (lIndex != -1)
                        {
                            sumL = valuesL[lIndex];
                        }
                        valuesL[currentL] = aVal - sumL;
                        columnIndicesL[currentL] = aCol;
                        currentL++;
                    }
                    else 
                        if (aCol == i) // для матрицы U
                        {
                            valuesU[currentU] = aVal;
                            columnIndicesU[currentU] = aCol;
                            currentU++;
                        }
                        else // для матрицы U
                        {
                            int uIndex = FindIndex(columnIndicesU, 0, currentU, aCol);
                            double sumU = 0.0;
                            if (uIndex != -1)
                            {
                                sumU = valuesU[uIndex];
                            }
                            valuesU[currentU] = aVal - sumU;
                            columnIndicesU[currentU] = aCol;
                            currentU++;
                        }
                }
                rowPointersL[i + 1] = currentL;
                rowPointersU[i + 1] = currentU;
            }

            L = new CSRMatrix(rowPointersL, columnIndicesL, valuesL);
            U = new CSRMatrix(rowPointersU, columnIndicesU, valuesU);
        }

        private int FindIndex(int[] columnIndices, int start, int end, int target)
        {
            for (int i = start; i < end; i++)
            {
                if (columnIndices[i] == target)
                {
                    return i;
                }
            }
            return -1; // not found
        }

        // Метод для применения предобуславливателя к вектору
        public double[] Apply(double[] x)
        {
            int n = L.Rows; // Предполагаем, что L.Rows возвращает количество строк в матрице L

            // Шаг 1: Вычисляем y = L^-1 * x
            double[] y = new double[n];
            for (int i = 0; i < n; i++)
            {
                y[i] = x[i];
                for (int j = L.RowPointers[i]; j < L.RowPointers[i + 1]; j++)
                {
                    int lCol = L.ColumnIndices[j]; // Столбец элемента L
                    y[i] -= L.Values[j] * y[lCol]; // Вычитаем влияние предыдущих y
                }
                // Никакого деления, так как L имеет 1 на диагонали
            }

            // Шаг 2: Вычисляем z = U^-1 * y
            double[] z = new double[n];
            for (int i = n - 1; i >= 0; i--) // Исправлено
            {
                z[i] = y[i];
                for (int j = U.RowPointers[i]; j < U.RowPointers[i + 1]; j++)
                {
                    int uCol = U.ColumnIndices[j]; // Столбец элемента U
                    z[i] -= U.Values[j] * z[uCol]; // Вычитаем влияние следующих z
                }
                // Делаем деление на диагональный элемент U
                z[i] /= U.Values[U.RowPointers[i] + (i - U.RowPointers[i])]; // Положение диагонального элемента U
            }

            return z;
        }

        public static void Test()
        {
            // |1, 2, 0|
            // |-1, 4, 1|
            // |0, 2, 6|
            //     | 1 |
            // X = | 1 |
            //     | 1 |  
            //     | 3 |
            // B = | 4 |
            //     | 8 |
            double[] V2 = { 1, 2, -1, 4, 1, 2, 6 };
            int[] COL_INDEX2 = { 0, 1, 0, 1, 2, 1, 2 };
            int[] ROW_INDEX2 = { 0, 2, 5, 7 };
            double[] B = { 3, 4, 8 };
            CSRMatrix matrix = new CSRMatrix(ROW_INDEX2, COL_INDEX2, V2);
            matrix.Print();

            ILUPreconditioner pc = new ILUPreconditioner(matrix);
            pc.L.Print();
            pc.U.Print();

            double[] X = pc.Apply(B);
            LOG.Print("X", X);
        }
    }
}
