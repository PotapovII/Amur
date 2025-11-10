namespace AlgebraLib.Sparse.CRS
{
    // для LLM
    using System;
    public class _SparseMatrixCSR
    {
        /// <summary>
        /// Указатели на начало каждой строки в массиве значений
        /// </summary>
        public int[] RowPointers;
        /// <summary>
        ///  Индексы столбцов для соответствующих ненулевых элементов из values
        /// </summary>
        public int[] ColumnIndices;
        /// <summary>
        /// Массив содержащий все ненулевые значения матрицы.
        /// </summary>
        public double[] Values;
        /// <summary>
        /// Количество строк матрицы
        /// </summary>
        public int N;
        public _SparseMatrixCSR(_SparseMatrixCSR crs)
        {
            RowPointers = crs.RowPointers;
            ColumnIndices = crs.ColumnIndices;
            Values = crs.Values;
            N = crs.N;
        }
        public _SparseMatrixCSR(int[] rowPointers, int[] columnIndices, double[] values)
        {
            if (rowPointers == null || columnIndices == null || values == null)
                throw new ArgumentNullException();
            RowPointers = rowPointers;
            ColumnIndices = columnIndices;
            Values = values;
            N = RowPointers.Length - 1;
        }
        public double GetElement(int row, int col)
        {
            for (int k = RowPointers[row]; k < RowPointers[row + 1]; ++k)
            {
                if (ColumnIndices[k] == col)
                    return Values[k];
            }
            return 0;
        }
        public void Multiply(double[] vector, ref double[] result)
        {
            result = new double[N];
            for (int i = 0; i < N; i++)
            {
                for (int k = RowPointers[i]; k < RowPointers[i + 1]; k++)
                    result[i] += Values[k] * vector[ColumnIndices[k]];
            }
        }
        public void MultiplyRowVector(double[] rowVector, ref double[] result)
        {
            result = new double[N];
            for (int i = 0; i < N; i++)
            {
                for (int k = RowPointers[i]; k < RowPointers[i + 1]; k++)
                    result[ColumnIndices[k]] += rowVector[i] * Values[k];
            }
        }
        public static _SparseMatrixCSR InitializeIdentity(int N = 1)
        {
            double[] values = new double[N];
            int[] colindices = new int[N];
            int[] rowPointers = new int[N + 1];
            for (int i = 0; i < N; i++)
            {
                rowPointers[i] = i;
                values[i] = 1;
                colindices[i] = i;
            }
            rowPointers[N] = N;
            return new _SparseMatrixCSR(rowPointers, colindices, values);
        }
    }
    public class _SparseMatrixCSC
    {
        public int[] ColumnPointers;
        public int[] RowIndices;
        public double[] Values;
        public int N;
        public _SparseMatrixCSC(_SparseMatrixCSC csc)
        {
            ColumnPointers = csc.ColumnPointers;
            RowIndices = csc.RowIndices;
            Values = csc.Values;
            N = csc.N;
        }
        /// <summary>
        /// Конструктор с параметрами
        /// </summary>
        public _SparseMatrixCSC(int[] columnPointers, int[] rowIndices, double[] values)
        {
            ColumnPointers = columnPointers;
            RowIndices = rowIndices;
            Values = values;
            N = ColumnPointers.Length - 1;
        }
        /// <summary>
        /// Конструктор на основе матрицы SparseMatrixCSR
        /// </summary>
        public _SparseMatrixCSC(_SparseMatrixCSR csr)
        {
            if (csr == null) throw new ArgumentNullException(nameof(csr));
            N = csr.N;
            int totalNonZeros = csr.Values.Length;
            int[] columnCounts = new int[N];
            for (int i = 0; i < totalNonZeros; i++)
                columnCounts[csr.ColumnIndices[i]]++;
            ColumnPointers = new int[N + 1];
            ColumnPointers[0] = 0;
            for (int i = 1; i <= N; i++)
                ColumnPointers[i] = ColumnPointers[i - 1] + columnCounts[i - 1];
            RowIndices = new int[totalNonZeros];
            Values = new double[totalNonZeros];
            int[] offset = new int[N];
            for (int row = 0; row < N; row++)
            {
                for (int k = csr.RowPointers[row]; k < csr.RowPointers[row + 1]; k++)
                {
                    int col = csr.ColumnIndices[k];
                    int posInCSC = ColumnPointers[col] + offset[col];
                    RowIndices[posInCSC] = row;
                    Values[posInCSC] = csr.Values[k];
                    offset[col]++;
                }
            }
        }
        public void MultiplyRowVector(double[] rowVector, ref double[] result)
        {
            result = new double[N];
            for (int j = 0; j < N; j++)
            {
                for (int k = ColumnPointers[j]; k < ColumnPointers[j + 1]; k++)
                    result[j] += rowVector[RowIndices[k]] * Values[k];
            }
        }
        public void Multiply(double[] vector, ref double[] result)
        {
            result = new double[N];
            for (int j = 0; j < N; j++)
            {
                for (int k = ColumnPointers[j]; k < ColumnPointers[j + 1]; k++)
                    result[RowIndices[k]] += Values[k] * vector[j];
            }
        }
        public static _SparseMatrixCSC InitializeIdentity(int N = 1)
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
            return new _SparseMatrixCSC(сolumnPointers, rowIndices, values);
        }
    }
}
