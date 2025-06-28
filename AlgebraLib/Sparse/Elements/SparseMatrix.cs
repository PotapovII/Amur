//---------------------------------------------------------------------------
//   Реализация библиотеки методов решения систем алгебраических уравнений
//                    разработка: Потапов И.И.
//                      30.05.2025
//---------------------------------------------------------------------------
namespace AlgebraLib.Sparse.River
{
    using System;
    using System.Linq;
    using MemLogLib;
    public class SparseMatrix 
    {
        /// <summary>
        /// порядок СЛУ
        /// </summary>
        public uint N { get { return (uint)FN; } set { FN = (int)value; } }
        /// <summary>
        /// порядок СЛУ
        /// </summary>
        protected int FN;
        /// <summary>
        /// Строки СЛАУ
        /// </summary>
        protected SparseRow[] Rows;
        /// <summary>
        /// хеш индексы
        /// </summary>
        protected int[] indexArray = null;
        public SparseRow Row(int i) { return Rows[i]; }
        public SparseMatrix(SparseMatrix a)
        {
            FN = a.FN;
            Rows = new SparseRow[FN];
            for (int i = 0; i < FN; i++)
                Rows[i] = new SparseRow(a.Rows[i]);
            MEM.Copy(ref indexArray, a.indexArray);
        }
        public SparseMatrix(int numRows, int rowSize)
        {
            FN = numRows;
            Rows = new SparseRow[FN];
            MEM.Alloc(FN, ref indexArray);
            for (int i = 0; i < FN; i++)
            {
                Rows[i] = new SparseRow(i, rowSize, FN);
                indexArray[i] = -1;
            }
        }
        /// <summary>
        /// Определить макимальное значение в матрице
        /// </summary>
        /// <returns></returns>
        public double Max()
        {
            return  Rows.Max(x => Math.Abs(x.MaxRow()));
        }
        /// <summary>
        /// Нормализация строки
        /// </summary>
        /// <param name="valNorm"></param>
        /// <param name="BC"></param>
        public void Normalization(int numRows, double valNorm)
        {
            Rows[numRows] *= valNorm;
        }
        public SparseMatrix(SparseMatrix K, int numRows)
        {
            FN = K.FN;
            Rows = new SparseRow[FN];
            MEM.Alloc(FN, ref indexArray);
            for (int i = 0; i < FN; i++)
            {
                Rows[i] = new SparseRow(K.Rows[i]);
                indexArray[i] = -1;
            }
        }
        public void AddToMatrix(int[] rownums, int[] colnums, double[][] sMatrix)
        {
            SparseRow newRow = new SparseRow(rownums[0], colnums.Length, FN);
            for (int i = 0; i < colnums.Length; i++)
                newRow.SetZeroValue(colnums[i]);
            
            for (int i = 0; i < rownums.Length; i++)
            {
                newRow.RowNumber = rownums[i];
                for (int j = 0; j < colnums.Length; j++)
                {
                    newRow.SetValue(j, colnums[j], sMatrix[i][j]);
                }
                Rows[rownums[i]].AddRange(1.0, newRow,ref indexArray);
            }
        }
        public  void AddToMatrix(double[][] LMartix, uint[] Adress)
        {
            for (int i = 0; i < Adress.Length; i++)
            {
                SparseRow newRow = new SparseRow((int)Adress[i], Adress.Length, FN);
                for (int j = 0; j < Adress.Length; j++)
                {
                    if (MEM.Equals(LMartix[i][j], 0, MEM.Error12) == false)
                        newRow.AddValue((int)Adress[j], LMartix[i][j]);
                }
                Rows[Adress[i]].AddRange(1.0, newRow, ref indexArray);
            }
        }

        public void AddStringSystem(double[] ColElems, uint[] ColAdress, uint IndexRow)
        {
            SparseRow newRow = new SparseRow((int)IndexRow, ColAdress.Length, FN);
            for (int i = 0; i < ColAdress.Length; i++)
                newRow.SetZeroValue((int)ColAdress[i]);
            newRow.RowNumber = (int)IndexRow;
            for (int j = 0; j < ColAdress.Length; j++)
                newRow.SetValue(j, (int)ColAdress[j], ColElems[j]);
            Rows[IndexRow].AddRange(1.0, newRow, ref indexArray);
        }

        public void BoundConditions(uint[] Adress)
        {
            for (int i = 0; i < Adress.Length; i++)
            {
                Rows[Adress[i]].Clear();
                Rows[Adress[i]].Add(new SparseElement(1.0, (int)Adress[i]));
            }
        }

        public void GetStringSystem(uint IndexRow, ref double[] ColElems)
        {
            MEM.AllocClear(N, ref ColElems);
            Rows[IndexRow].DeCompress(ref ColElems);
        }
        public double Value(int row, int column)
        {
            if (row <= FN)
                return Rows[row].Value(column);
            else
                return 0.0;
        }
        public virtual void Clear()
        {
            for (int i = 0; i < FN; i++)
                Rows[i].Clear();
        }
        /// <summary>
        /// Разложение LU
        /// </summary>
        public void LUDecompose()
        {
            for (int iPivot = 0; iPivot < FN; iPivot++)
            {
                SparseRow pivotRow = Rows[iPivot];
                for (int index = 1; index < pivotRow.Count; index++)
                {
                    int iTarget = pivotRow.ColumnByIndex(index);
                    if (iTarget > iPivot)
                        Rows[iTarget].ReduceRow(pivotRow, indexArray);
                }
            }
        }
        /// <summary>
        /// умножение вектора ошибки на матрицу r' = M^-1 r
        /// где M^-1 = ILU
        /// </summary>
        /// <param name="rhs"></param>
        public void Precondition(double[] rhs)
        {
            for (int i = 1; i < FN; i++)
                Rows[i].ScalarLeftSubRow(rhs);
            for (int i = FN - 1; i >= 0; i--)
                Rows[i].ScalarRightSubRow(rhs);
        }
        /// <summary>
        /// Умножение вектора Vector на матрицу M справа  Result = M * Vector
        /// </summary>
        public void MatrixCRS_Mult(ref double[] Result, double[] Vector)
        {
            for (int i = 0; i < FN; i++)
                Result[i] = Rows[i].RowDot(Vector);
        }
        /// <summary>
        /// Умножение вектора Vector на матрицу M справа  Result = Vector * M  
        /// </summary>
        public void Mult_MatrixCRS(ref double[] result, double[] Vector)
        {
            SparseRow currentRow;
            int currentRowSize;
            int currentColumnIndex, currentRowIndex;
            double currentValue;
            for (int i = 0; i < FN; i++)
                result[i] = 0.0;
            for (int i = 0; i < FN; i++)
            {
                currentRow = Row(i);
                currentRowSize = currentRow.Count;
                for (int j = 0; j < currentRowSize; j++)
                {
                    currentColumnIndex = currentRow.ColumnByIndex(j);
                    currentValue = currentRow.ValueByIndex(j);
                    currentRowIndex = currentRow.RowNumber;
                    result[currentRowIndex] += currentValue * Vector[currentColumnIndex];
                }
            }
        }

        public virtual void Print(int pack = 0,string str = "Матрица распокованная (метод Value)")
        {
            if (str == "")
                Console.WriteLine("Матрица распокованная (метод Value)");
            else
                Console.WriteLine(str);
            for (int i = 0; i < FN; ++i)
            {
                for (int j = 0; j < FN; ++j)
                {
                    double e = Value(i, j);
                    Console.Write(" " + e.ToString("F6"));
                }
                Console.WriteLine();
            }
            if (pack > 0)
            {
                Console.WriteLine("Матрица упакованая");
                for (int i = 0; i < FN; ++i)
                {
                    Console.Write("номер строки " + Rows[i].RowNumber.ToString());
                    for (int j = 0; j < Rows[i].Count; ++j)
                    {
                        int ce = Rows[i].ColumnByIndex(j);
                        double ee = Rows[i].ValueByIndex(j);
                        Console.Write(" col " + ce.ToString() + " " + ee.ToString("F6"));
                    }
                    Console.WriteLine();
                }
            }
            if (pack > 1)
            {
                Console.WriteLine("Матрица распокованная");
                for (int i = 0; i < FN; ++i)
                {
                    for (int j = 0; j < FN; ++j)
                    {
                        double e = Rows[i].Value(j);
                        Console.Write(" " + e.ToString("F6"));
                    }
                    Console.WriteLine();
                }
            }
        }
    }
}
