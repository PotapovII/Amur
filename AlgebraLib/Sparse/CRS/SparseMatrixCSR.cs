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
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    /// <summary>
    /// Массивы для хранения разреженной матрицы в формате CSR (Compressed Sparse Row)
    /// Реализованы методы умножения с временной сложностью O(nnz), где nnz - количество 
    /// ненулевых элементов (в отличие от O(n³) для плотной матрицы)
    /// </summary>
    public class SparseMatrixCSR
    {
        OrderablePartitioner<Tuple<int, int>> OrdPart = null;
        /// <summary>
        /// Указатели на начало каждой строки в массиве значений
        /// Массив размера Rows + 1, где rowPointers[i] указывает на индекс в массиве values, 
        /// с которого начинается i-ая строка.
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
        /// <summary>
        /// Конструктор копирования
        /// </summary>
        public SparseMatrixCSR(SparseMatrixCSR crs)
        {
            RowPointers = crs.RowPointers;
            ColumnIndices = crs.ColumnIndices;
            Values = crs.Values;
            N = crs.N;
            OrdPart = Partitioner.Create(0, N);
        }
        /// <summary>
        /// Конструктор с параметрами
        /// </summary>
        public SparseMatrixCSR(int[] rowPointers, int[] columnIndices, double[] values)
        {
            if (rowPointers == null || columnIndices == null || values == null)
                throw new ArgumentNullException();
            RowPointers = rowPointers;
            ColumnIndices = columnIndices;
            Values = values;
            N = RowPointers.Length - 1;
            OrdPart = Partitioner.Create(0, N);
            if (RowPointers.Length < 2) return;
            for (int i = 0; i < N; i++)
            {
                if (RowPointers[i] > RowPointers[i + 1])
                    throw new ArgumentException("Неверный порядок row pointers.");
            }
        }

        #region Расширение
        /// <summary>
        /// Создание единичной матрицы
        /// </summary>
        /// <returns></returns>
        public static SparseMatrixCSR InitializeIdentity(int N = 1)
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
            SparseMatrixCSR m = new SparseMatrixCSR(rowPointers, colindices, values);
            return m;
        }

        /// <summary>
        /// метод для получения элемента матрицы по индексам
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetElement(int row, int col)
        {
            for (int k = RowPointers[row]; k < RowPointers[row + 1]; ++k)
                if (ColumnIndices[k] == col)
                    return Values[k];
            return 0;
        }
        public int GetIndex(int row, int col)
        {
            for (int k = RowPointers[row]; k < RowPointers[row + 1]; ++k)
                if (ColumnIndices[k] == col)
                    return k;
            return -1;
        }

        /// <summary>
        /// Получить верхнюю треугольную матрицу c диагональными членами
        /// </summary>
        /// <returns>Верхняя треугольная матрицы c диагональными членами</returns>
        public SparseMatrixCSR GetUpperTriangular()
        {
            int[] rowIndices = new int[N + 1];
            List<int> colIndices = new List<int>(Values.Length);
            List<double> values = new List<double>(Values.Length);
            int nnz = 0; 
            for (int i = 0; i < N; i++)
            {
                // Добавить текущее количество ненулевых
                // элементов в следующей строке.
                rowIndices[i] = nnz;
                for (int j = RowPointers[i]; j < RowPointers[i + 1]; j++)
                {
                    // если индекс столбца больше или равен
                    // текущему индексу строки, добавляем элемент
                    if (ColumnIndices[j] >= i)
                    {
                        nnz++;
                        colIndices.Add(ColumnIndices[j]);
                        values.Add(Values[j]);
                    }
                }
            }
            // Добавление последнего элемента, указывающего на конец списка индексов столбцов и значений
            rowIndices[N] = nnz;
            return new SparseMatrixCSR(rowIndices, colIndices.ToArray(), values.ToArray());
        }
        /// <summary>
        /// Получить нижнюю треугольную матрицу 
        /// </summary>
        /// <returns>Нижняя треугольная матрицы</returns>
        public SparseMatrixCSR GetLowerTriangular(bool diag = true)
        {
            int[] rowIndices = new int[N + 1];
            List<int> colIndices = new List<int>(Values.Length);
            List<double> values = new List<double>(Values.Length);
            int nnz = 0;
            for (int i = 0; i < N; i++)
            {
                rowIndices[i] = nnz;
                for (int j = RowPointers[i]; j < RowPointers[i + 1]; j++)
                {
                    // если индекс столбца меньше или равен
                    // текущему индексу строки, добавляем элемент
                    bool flag = false;
                    if (diag == true)
                        flag = ColumnIndices[j] <= i;
                    else
                        flag = ColumnIndices[j] < i;
                    if (flag == true) 
                    {
                        nnz++;
                        colIndices.Add(ColumnIndices[j]);
                        if (ColumnIndices[j] == i)
                            values.Add(1);
                        else
                            values.Add(Values[j]);
                    }
                }
            }
            // Добавление последнего элемента, указывающего на
            // конец списка индексов столбцов и значений
            rowIndices[N] = nnz;
            return new SparseMatrixCSR(rowIndices.ToArray(), colIndices.ToArray(), values.ToArray());
        }



        public void GetUnPackRow(int row, ref double[] value)
        {
            for (int k = RowPointers[row]; k < RowPointers[row + 1]; ++k)
                value[ColumnIndices[k]] = Values[k];
        }
        public void ClearUnPackRow(int row, ref double[] value)
        {
            for (int k = RowPointers[row]; k < RowPointers[row + 1]; ++k)
                value[ColumnIndices[k]] = 0;
        }
        #endregion

        /// <summary>
        /// Умножает матрицу на вектор-столбец (A * x)
        /// </summary>
        /// <param name="vector">вектор столбец умножаемый на матрицу</param>
        /// <param name="result">вектор столбец - результат</param>
        public double[] Multiply(double[] vector)
        {
            var result = new double[N];
            for (int i = 0; i < N; i++)
            {
                for (int k = RowPointers[i]; k < RowPointers[i + 1]; k++)
                    result[i] += Values[k] * vector[ColumnIndices[k]];
            }
            return result;
        }
        /// <summary>
        /// Умножает матрицу на вектор-столбец (A * x)
        /// </summary>
        /// <param name="vector">вектор столбец умножаемый на матрицу</param>
        /// <param name="result">вектор столбец - результат</param>
        public void Multiply(double[] vector, ref double[] result)
        {
            MEM.Alloc(N, ref result, 0);
            for (int i = 0; i < N; i++)
            {
                for (int k = RowPointers[i]; k < RowPointers[i + 1]; k++)
                    result[i] += Values[k] * vector[ColumnIndices[k]];
            }
        }
        /// <summary>
        /// Умножает матрицу на вектор-столбец (A * x)
        /// Поскольку каждая строка независимо обрабатывается, их можно параллелить по строкам.
        /// </summary>
        /// <param name="vector">вектор столбец умножаемый на матрицу</param>
        /// <param name="result">вектор столбец - результат</param>
        public void PMultiply(double[] vector, ref double[] Result)
        {
            MEM.Alloc(N, ref Result, 0);
            double[] result = Result;
            Parallel.ForEach(OrdPart, (range, loopState) =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    for (int k = RowPointers[i]; k < RowPointers[i + 1]; k++)
                        result[i] += Values[k] * vector[ColumnIndices[k]];
                }
            });
            Result = result;
        }
        /// Если у вас часто возникает необходимость умножать на вектор-строку, 
        /// нужно перейти к хранению матрицы в формате CSC (Compressed Sparse Column), 
        /// т.е. использовать класс SparseMatrixCSC
        /// где информация группируется по столбцам. Это снизит время выполнения операции 
        /// row_vector * matrix. Однако для разового использования текущий метод эффективен 
        /// и прост в реализации.
        /// <param name="vector">вектор столбец умножаемый на матрицу</param>
        /// <param name="result">вектор столбец - результат</param>
        public void MultiplyRowVector(double[] rowVector, ref double[] result)
        {
            MEM.Alloc(N, ref result, 0);
            for (int i = 0; i < N; i++)
            {
                for (int k = RowPointers[i]; k < RowPointers[i + 1]; k++)
                    result[ColumnIndices[k]] += rowVector[i] * Values[k];
            }
        }
        /// Если у вас часто возникает необходимость умножать на вектор-строку, 
        /// можно рассмотреть хранение матрицы в формате CSC (Compressed Sparse Column), 
        /// где информация группируется по столбцам. Это снизит время выполнения операции 
        /// row_vector * matrix. Однако для разового использования текущий метод эффективен 
        /// и прост в реализации.
        /// <param name="vector">вектор столбец умножаемый на матрицу</param>
        /// <param name="result">вектор столбец - результат</param>
        public double[] MultiplyRowVector(double[] rowVector)
        {
            var result = new double[N];
            for (int i = 0; i < N; i++)
            {
                for (int k = RowPointers[i]; k < RowPointers[i + 1]; k++)
                    result[ColumnIndices[k]] += rowVector[i] * Values[k];
            }
            return result;
        }
        /// <summary>
        /// Использовать Parallel.For по i (строкам), но при записи в 
        /// result[ColumnIndices[k]] нужно синхронизировать доступ.        
        /// Это можно сделать через lock или Interlocked.Add, если тип 
        /// результата double поддерживает атомарные операции. 
        /// Однако double – это 64-битный тип, и в C# нет встроенной 
        /// атомарной операции сложения для double. 
        /// Можно использовать lock на объекте.
        /// </summary>
        /// <param name="vector">вектор столбец умножаемый на матрицу</param>
        /// <param name="result">вектор столбец - результат</param>
        public void PMultiplyRowVector(double[] rowVector, ref double[] Result)
        {
            MEM.Alloc(N, ref Result, 0);
            var result = Result;
            Parallel.ForEach(OrdPart, (range, loopState) =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    for (int k = RowPointers[i]; k < RowPointers[i + 1]; k++)
                    {
                        lock (result)
                        {
                            result[ColumnIndices[k]] += rowVector[i] * Values[k];
                        }
                    }
                }
            });
            Result = result;
        }
        public void Print()
        {
            Console.WriteLine("SparseMatrixCSR");
            double[] buff = new double[N];
            for (int i = 0; i < N; i++)
            {
                for (int k = RowPointers[i]; k < RowPointers[i + 1]; k++)
                    buff[ColumnIndices[k]] = Values[k];
                LOG.Print(i.ToString() + " row: ", buff);
                for (int k = RowPointers[i]; k < RowPointers[i + 1]; k++)
                    buff[ColumnIndices[k]] = 0;
            }
        }
        public void Print(string Name)
        {
            Console.WriteLine(Name);
            double[] buff = new double[N];
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                    Console.Write(GetElement(i, j).ToString("F4") + "  ");
                Console.WriteLine();
            }
        }
        public void PrintCSR(string Name="CSR")
        {
            Console.WriteLine(Name);
            LOG.Print("RowPointers",RowPointers);
            LOG.Print("ColumnIndices",ColumnIndices);
            LOG.Print("Values",Values);
        }

        public static void Test()
        {
            // |1, 2, 0|
            // |0, 4, 0|
            // |0, 2, 6|
            //double[] V = { 1, 2, 4, 2, 6 };
            //int[] COL_INDEX = { 0, 1, 1, 1, 2 };
            //int[] ROW_INDEX = { 0, 2, 3, 5 };
            //SparseMatrixCSR matrix = new SparseMatrixCSR(ROW_INDEX, COL_INDEX, V);
            //matrix.Print();
            //double[] X = { 1, 2, 3 };
            //double[] Res = null;
            //matrix.Multiply(X, ref Res);
            //LOG.Print(" X ", X);
            //LOG.Print(" Res ", Res );
            //Res = matrix.Multiply(X);
            //LOG.Print(" X ", X);
            //LOG.Print(" Res ", Res);
            //matrix.PMultiply(X, ref Res);
            //LOG.Print(" X ", X);
            //LOG.Print(" Res ", Res);
            //matrix.MultiplyRowVector(X, ref Res);
            //LOG.Print(" X ", X);
            //LOG.Print(" Res ", Res);
            //Res = matrix.MultiplyRowVector(X);
            //LOG.Print(" X ", X);
            //LOG.Print(" Res ", Res);
            // |1, 2, 0|
            // |-1, 4, 1|
            // |0, 2, 6|
            double[] V2 = { 1, 2, -1, 4, 1, 2, 6 };
            int[] COL_INDEX2 = { 0, 1, 0, 1, 2, 1, 2 };
            int[] ROW_INDEX2 = { 0, 2, 5, 7 };
            SparseMatrixCSR matrix2 = new SparseMatrixCSR(ROW_INDEX2, COL_INDEX2, V2);
            matrix2.Print("matrix");
            matrix2.PrintCSR("matrix" + "CSR");
            SparseMatrixCSR um = matrix2.GetUpperTriangular();
            um.Print("UMatrix");
            um.PrintCSR("UMatrix" + "CSR");
            SparseMatrixCSR lm = matrix2.GetLowerTriangular();
            lm.Print("LMatrix");
            lm.PrintCSR("LMatrix" + "CSR");
            SparseMatrixCSR lm1 = matrix2.GetLowerTriangular(false);
            lm1.Print("LMatrix");
            lm1.PrintCSR("LMatrix" + "CSR");

            SparseMatrixCSR e = SparseMatrixCSR.InitializeIdentity(5);
            e.Print("eMatrix");

        }
    }
}
