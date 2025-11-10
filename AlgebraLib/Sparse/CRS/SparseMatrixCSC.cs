//---------------------------------------------------------------------------
//   Реализация библиотеки методов решения систем алгебраических уравнений
//                    разработка: Потапов И.И.
//                      20.07.2025
//---------------------------------------------------------------------------
namespace AlgebraLib.Sparse.CRS
{
    using MemLogLib;
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;

    public class SparseMatrixCSC
    {
        OrderablePartitioner<Tuple<int, int>> OrdPart = null;
        /// <summary>
        /// Начало каждого столбца в массиве RowIndices / Values
        /// </summary>
        public int[] ColumnPointers;
        /// <summary>
        /// Индексы строк для значений ненулевых элементов (в порядке сжатия по столбцам)
        /// </summary>
        public int[] RowIndices;
        /// <summary>
        /// Значения элементов матрицы, сохраненные в порядке CSC
        /// </summary>
        public double[] Values;
        /// <summary>
        /// Размер квадратной матрицы
        /// </summary>
        public int N;
        /// <summary>
        /// Конструктор копирования
        /// </summary>
        public SparseMatrixCSC(SparseMatrixCSC csc)
        {
            ColumnPointers = csc.ColumnPointers;
            RowIndices = csc.RowIndices;
            Values = csc.Values;
            N = csc.N;
            OrdPart = Partitioner.Create(0, N);
        }
        /// <summary>
        /// Конструктор с параметрами
        /// </summary>
        /// <param name="columnPointers"></param>
        /// <param name="rowIndices"></param>
        /// <param name="values"></param>
        /// <param name="n"></param>
        public SparseMatrixCSC(int[] columnPointers, int[] rowIndices, double[] values)
        {
            ColumnPointers = columnPointers;
            RowIndices = rowIndices;
            Values = values;
            N = ColumnPointers.Length - 1;
            OrdPart = Partitioner.Create(0, N);
        }
        /// <summary>
        /// Конструктор на основе матрицы SparseMatrixCSR
        /// </summary>
        public SparseMatrixCSC(SparseMatrixCSR csr)
        {
            if (csr == null) throw new ArgumentNullException(nameof(csr));
            N = csr.N;
            int totalNonZeros = csr.Values.Length;
            // Шаг 1: Подсчет количества элементов в каждом столбце
            int[] columnCounts = new int[N];
            for (int i = 0; i < totalNonZeros; i++)
                columnCounts[csr.ColumnIndices[i]]++;
            // Шаг 2: Построение ColumnPointers как префиксные суммы для индексов в RowIndices
            ColumnPointers = new int[N + 1];
            ColumnPointers[0] = 0;
            for (int i = 1; i <= N; i++)
                ColumnPointers[i] = ColumnPointers[i - 1] + columnCounts[i - 1];
            // Шаг 3: Инициализация RowIndices и Values
            RowIndices = new int[totalNonZeros];
            Values = new double[totalNonZeros];
            // Счетчик для заполнения элементов по столбцам
            int[] offset = new int[N];
            // Шаг 4: Заполнение CSC матрицы, перебирая все строки в CSR
            for (int row = 0; row < N; row++)
            {
                for (int k = csr.RowPointers[row]; k < csr.RowPointers[row + 1]; k++)
                {
                    int col = csr.ColumnIndices[k];
                    // Позиция для записи в RowIndices и Values
                    int posInCSC = ColumnPointers[col] + offset[col];
                    RowIndices[posInCSC] = row;
                    Values[posInCSC] = csr.Values[k];
                    offset[col]++;
                }
            }
            OrdPart = Partitioner.Create(0, N);
        }

        public static SparseMatrixCSC InitializeIdentity(int N = 1)
        {
            double[] values = new double[N];
            int[] rowIndices = new int[N]; 
            int[] сolumnPointers = new int[N + 1];
            for (int i = 0; i < N; i++)
            {
                сolumnPointers[i] = i;
                values[i] = 1;
                rowIndices[i] = i;
            }
            сolumnPointers[N] = N;
            return new SparseMatrixCSC(сolumnPointers, rowIndices, values);
        }
        
        /// <summary>
        /// Умножает вектор-строку на матрицу слева (x * A)
        /// </summary>
        public double[] MultiplyRowVector(double[] rowVector)
        {
            var result = new double[N];
            for (int j = 0; j < N; j++)
            {
                for (int k = ColumnPointers[j]; k < ColumnPointers[j + 1]; k++)
                    result[j] += rowVector[RowIndices[k]] * Values[k];
            }
            return result;
        }

        /// <summary>
        /// Умножает вектор-строку на матрицу слева, передаёт результат по ссылке (x * A)
        /// </summary>
        public void MultiplyRowVector(double[] rowVector, ref double[] result)
        {
            MEM.Alloc(N, ref result, 0);
            for (int j = 0; j < N; j++)
            {
                for (int k = ColumnPointers[j]; k < ColumnPointers[j + 1]; k++)
                    result[j] += rowVector[RowIndices[k]] * Values[k];
            }
        }
        /// <summary>
        /// Умножает вектор-строку на матрицу слева, передаёт результат по ссылке (x * A)
        /// </summary>
        public void PMultiplyRowVector(double[] rowVector, ref double[] Result)
        {
            MEM.Alloc(N, ref Result, 0);
            double[] result = Result;
            Parallel.ForEach(OrdPart, (range, loopState) =>
            {
                for (int j = range.Item1; j < range.Item2; j++)
                {
                    for (int k = ColumnPointers[j]; k < ColumnPointers[j + 1]; k++)
                        result[j] += rowVector[RowIndices[k]] * Values[k];
                }
            });
            Result = result;
        }
      
        /// <summary>
        /// Умножает матрицу на вектор-столбец (A * y)
        /// </summary>
        public double[] Multiply(double[] vector)
        {
            var result = new double[N];
            for (int j = 0; j < N; j++)
            {
                for (int k = ColumnPointers[j]; k < ColumnPointers[j + 1]; k++)
                    result[RowIndices[k]] += Values[k] * vector[j];
            }
            return result;
        }
        /// <summary>
        /// Умножает матрицу на вектор-столбец, передаёт результат по ссылке (A * y)
        /// </summary>
        public void Multiply(double[] vector, ref double[] result)
        {
            MEM.Alloc(N, ref result, 0);
            for (int j = 0; j < N; j++)
            {
                for (int k = ColumnPointers[j]; k < ColumnPointers[j + 1]; k++)
                    result[RowIndices[k]] += Values[k] * vector[j];
            }
        }

        public void Print()
        {
            Console.WriteLine("SparseMatrixCSC");
            double[,] buff = new double[N, N];
            for (int j = 0; j < N; j++)
            {
                for (int k = ColumnPointers[j]; k < ColumnPointers[j + 1]; k++)
                    buff[RowIndices[k], j] = Values[k];
            }
            LOG.Print(" rowMatrix ", buff);
        }
        public static void Test()
        {
            SparseMatrixCSR e = SparseMatrixCSR.InitializeIdentity(5);
            e.Print("eMatrix");

            // |1, 2, 0|
            // |0, 4, 0|
            // |0, 2, 6|
            double[] V = { 1, 2, 4, 2, 6 };
            int[] COL_INDEX = { 0, 1, 1, 1, 2 };
            int[] ROW_INDEX = { 0, 2, 3, 5 };
            SparseMatrixCSR _matrix = new SparseMatrixCSR(ROW_INDEX, COL_INDEX, V);
            SparseMatrixCSC matrix = new SparseMatrixCSC(_matrix);
            matrix.Print();
            double[] X = { 1, 2, 3 };
            double[] Res = null;
            matrix.MultiplyRowVector(X, ref Res);
            LOG.Print(" X ", X);
            LOG.Print(" Res ", Res);
            Res = matrix.MultiplyRowVector(X);
            LOG.Print(" X ", X);
            LOG.Print(" Res ", Res);
            matrix.PMultiplyRowVector(X, ref Res);
            LOG.Print(" X ", X);
            LOG.Print(" Res ", Res);
            matrix.Multiply(X, ref Res);
            LOG.Print(" X ", X);
            LOG.Print(" Res ", Res);
            Res = matrix.Multiply(X);
            LOG.Print(" X ", X);
            LOG.Print(" Res ", Res);
        }
    }

}
