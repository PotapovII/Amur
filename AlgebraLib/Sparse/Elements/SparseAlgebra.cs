//---------------------------------------------------------------------------
//   Реализация библиотеки методов решения систем алгебраических уравнений
//                     - (C) Copyright 2021 -
//                       ALL RIGHT RESERVED
//                    разработка: Потапов И.И.
//---------------------------------------------------------------------------
//                        Потапов И.И.
//              member-функции класса HPackAlgebra
//                  Last Edit Data: 25.6.2001
//---------------------------------------------------------------------------
//                        Потапов И.И.
//          HPackAlgebra (C++) ==> SparseAlgebra (C#)
//                       18.04.2021 
//---------------------------------------------------------------------------
namespace AlgebraLib
{
    using MemLogLib;
    using CommonLib;
    using System;
    using System.Linq;
    using System.Collections.Generic;

    /// <summary>
    /// ОО: Класс матрицы в формате CRS
    /// с поддержкой интерфейса алгебры для работы с КЭ задачами
    /// </summary>
    public abstract class SparseAlgebra : Algebra
    {
        /// <summary>
        /// плотно упакованная матрица системы
        /// </summary>
        public List<SparseRow> Matrix;
        /// <summary>
        /// Флаг работы с матрицами предобуславливания
        /// </summary>
        protected bool isPrecond;
        /// <summary>
        /// Матрица предобуславливателя ILU(1)
        /// </summary>
        public List<SparseRow> ILU;
        /// <summary>
        /// Индексы профиля матрицы для расчета матриц предобуславливания
        /// </summary>
        public List<SparseColIndex> MatrixColIndex;
        public int[] diagonalRow;
        public int[] diagonalCol;
        public double[] Y;

        public SparseAlgebra(uint FN = 1, bool isPrecond = false)
        {
            this.isPrecond = isPrecond;
            AlgebraResultIterative res = new AlgebraResultIterative();
            res.isPrecond = isPrecond;
            SetAlgebra(res, FN);
        }
        public override void SetAlgebra(IAlgebraResult res, uint FN)
        {
            try
            {
                base.SetAlgebra(res, FN);
                Matrix = new List<SparseRow>((int)FN);
                for (int i = 0; i < FN; i++)
                    Matrix.Add(new SparseRow());
                if (isPrecond == true)
                {
                    Y = new double[FN];
                    diagonalRow = new int[FN];
                    diagonalCol = new int[FN];
                    MatrixColIndex = new List<SparseColIndex>((int)FN);
                    ILU = new List<SparseRow>();
                    for (int i = 0; i < FN; i++)
                    {
                        MatrixColIndex.Add(new SparseColIndex());
                    }
                }
            }
            catch (Exception ex)
            {
                result.errorType = ErrorType.outOfMemory;
                result.Message = ex.Message;
            }
        }
        /// <summary>
        /// Очистка матрицы и правой части
        /// </summary>
        public override void Clear()
        {
            for (int i = 0; i < N; i++)
            {
                Matrix[i].Row.Clear();
                Right[i] = 0;
            }
            if (isPrecond == true)
            {
                for (int i = 0; i < ILU.Count; i++)
                    ILU[i].Row.Clear();
                for (int i = 0; i < N; i++)
                {
                    MatrixColIndex[i].Clear();
                }
            }
        }
        #region Методы для вычисления матрицы предобуславливания
        /// <summary>
        /// Построение образа неулевых индексов по колонкам матрицы
        /// </summary>
        public void CreateMatrixColIndex()
        {
            for (int row = 0; row < N; row++)
                for (int j = 0; j < Matrix[row].Row.Count; j++)
                {
                    int col = Matrix[row].Row[j].Knot;
                    MatrixColIndex[col].Col.Add(new SparseElementIndex(row, j));
                    if (row == col)
                    {
                        diagonalRow[row] = j;
                        diagonalCol[col] = MatrixColIndex[col].Col.Count - 1;
                    }
                }
        }
        /// <summary>
        /// Умножение строки матрицы на ее столбец c ограничениями
        /// </summary>
        /// <param name="rowIndex">индекс строки матрицы</param>
        /// <param name="colIndex">индекс столбеца матрицы</param>
        /// <returns>результат</returns>
        /// <param name="rowCount">ограничение суммирования по строкам матрицы</param>
        /// <param name="colCount">ограничение суммирования по столбцам матрицы</param>
        /// <returns></returns>
        public double MultMatrix(SparseRow a, SparseColIndex b, int rowCount, int colCount)
        {
            int col = 0, row = 0;
            double sum = 0;
            while (col < a.Count && row < b.Count)
            {
                int posx = a[col].Knot;
                int posy = b[row].Knot;

                if (posx >= colCount)
                    break;
                if (posy >= rowCount)
                    break;

                if (posx == posy)
                {
                    sum += a[col].Elem * ILU[posy].Row[b[row].j].Elem;
                    col++;
                    row++;
                }
                else if (posx < posy)
                {
                    col++;
                }
                else //if (posx > posy)
                {
                    row++;
                }
            }
            return sum;
        }

        /// <summary>
        /// Декомпозиция матрицы на A => ILU
        /// </summary>
        public void DecompositionPrecondMatrix()
        {
            int i=0, ii, j, e, row_i, row_i0, j0;
            try
            {
                // копируем матрицу Matrix в ILU
                for (i = 0; i < FN; i++)
                    ILU.Add(new SparseRow(Matrix[i]));
                double sum;
                for (i = 0; i < FN; i++)
                {
                    // получаем строку ILU
                    SparseRow a = ILU[i];
                    // получаем индекс диагонального элемента матрицы
                    // копируем все элементы строки от диагонали ко конца строки
                    // в столбец
                    for (j = diagonalRow[i]; j < a.Count; j++)
                    {
                        SparseColIndex b = MatrixColIndex[a.Row[j].Knot];
                        sum = MultMatrix(a, b, i, i);
                        ILU[i].Row[j].Elem = Matrix[i].Row[j].Elem - sum;
                    }
                    //  индексы i столбца
                    SparseColIndex ci = MatrixColIndex[i];
                    ii = diagonalCol[i];
                    row_i0 = ci[ii].Knot;
                    j0 = ci[ii].j;
                    double aii = 1 / ILU[row_i0].Row[j0].Elem;
                    for (e = diagonalCol[i] + 1; e < ci.Count; e++)
                    {
                        row_i = ci[e].Knot;
                        j = ci[e].j;
                        SparseRow aj = ILU[row_i];
                        sum = MultMatrix(aj, ci, i, i);
                        ILU[row_i].Row[j].Elem = aii * (Matrix[row_i].Row[j].Elem - sum);
                    }
                    //Print("Matrix ILU", ILU);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(i);
            }
        }

        /// <summary>
        ///  умножение вектора ошибки на матрицу r' = M^-1 r
        ///  M^-1 = ILU
        /// </summary>
        /// <param name="X"></param>
        public void CalkErrorPrecond(double[] R, ref double[] PR)
        {
            double t;
            MEM.AllocClear(FN, ref Y);
            for (int i = 0; i < FN; i++)
            {
                double sum = 0;
                SparseRow a = ILU[i];
                for (int j = 0; j < diagonalRow[i]; j++)
                {
                    int k = a.Row[j].Knot;
                    sum += a.Row[j].Elem * Y[k];
                }
                Y[i] = R[i] - sum;
            }
            // find solution of Ux = y
            for (int i = (int)FN - 1; i >= 0; i--)
            {
                SparseRow a = ILU[i];
                int jd = diagonalRow[i];
                t = 1.0 / a.Row[jd].Elem;
                double sum = 0;
                for (int j = diagonalRow[i] + 1; j < a.Count; j++)
                {
                    int k = a.Row[j].Knot;
                    sum += a.Row[j].Elem * PR[k];
                }
                PR[i] = t * (Y[i] - sum);
            }
        }
        /// <summary>
        /// Построение образа неулевых индексов по колонкам матрицы
        /// </summary>
        /// <param name="R"></param>
        /// <param name="PR"></param>
        public void TP(double[] R, ref double[] PR)
        {
            if (isPrecond == true)
            {
                // Построение образа неулевых индексов по колонкам матрицы
                CreateMatrixColIndex();
                // расчет матрицы предобуславливателя
                DecompositionPrecondMatrix();
                Print("Matrix ILU", ILU);
                CalkErrorPrecond(R, ref PR);
            }
        }

        #endregion

        /// <summary>
        /// нормировка системы для малых значений матрицы и правой части
        /// </summary>
        protected override void SystemNormalization()
        {
            // максимальное значение в правой части
            double maxR = Right.Max(x => Math.Abs(x));
            // максимальное значение в матрице
            double maxM = Matrix.Max(x => Math.Abs(x.MaxRow()));
            if (maxM > maxR)
            {
                if (maxM < 1 && maxM > 0)
                {
                    double v = (int)Math.Abs(Math.Log10(maxM));
                    double valNorm = Math.Pow(10, v + 2);
                    for (int i = 0; i < FN; i++)
                    {
                        Right[i] *= valNorm;
                        Matrix[i] *= valNorm;
                    }
                }
            }
        }
        /// <summary>
        /// Решение САУ
        /// </summary>
        /// <param name="X">Вектор в который возвращается результат решения СЛАУ</param>
        public override void Solve(ref double[] X)
        {
            MEM.Alloc<double>((int)FN, ref X);
            // нормировка системы для малых значений матрицы и правой части
            SystemNormalization();
            if (isPrecond == true)
            {
                // Построение образа неулевых индексов по колонкам матрицы
                CreateMatrixColIndex();
                // расчет матрицы предобуславливателя
                DecompositionPrecondMatrix();
            }
            SetBCondition(ref X);         // X => X0 - начальное условие
            base.Solve(ref X);
        }
        /// <summary>
        /// Формирование матрицы системы
        /// </summary>
        /// <param name="LMartix">Локальная матрица</param>
        /// <param name="Adress">Глабальные адреса</param>
        public override void AddToMatrix(double[][] LMartix, uint[] Adress)
        {
            for (int a = 0; a < Adress.Length; a++)
            {
                int ash = (int)Adress[a];
                Matrix[ash].Add(LMartix[a], Adress);
            }
        }
        /// <summary>
        /// Сборка САУ по строкам (не для всех решателей)
        /// </summary>
        /// <param name="ColElems">Коэффициенты строки системы</param>
        /// <param name="ColAdress">Адреса коэффицентов</param>
        /// <param name="IndexRow">Индекс формируемой строки системы</param>
        /// <param name="Right">Значение правой части строки</param>
        public override void AddStringSystem(double[] ColElems, uint[] ColAdress, uint IndexRow, double R)
        {
            Matrix[(int)IndexRow].Row.Clear();
            Matrix[(int)IndexRow].Add(ColElems, ColAdress);
            Right[IndexRow] += R;
        }
        /// <summary>
        /// Получить строку (не для всех решателей)
        /// </summary>
        /// <param name="IndexRow">Индекс получемой строки системы</param>
        /// <param name="ColElems">Коэффициенты строки системы</param>
        /// <param name="ColAdress">Адреса коэффицентов</param>
        /// <param name="R">Значение правой части</param>
        public override void GetStringSystem(uint IndexRow, ref double[] ColElems, ref double R)
        {
            MEM.Alloc(FN, ref ColElems);
            Matrix[(int)IndexRow].DeCompress(ref ColElems);
            R = Right[IndexRow];
        }
        /// <summary>
        /// Удовлетворение ГУ
        /// </summary>
        public override void BoundConditions(double[] Conditions, uint[] Adress)
        {
            for (int i = 0; i < Adress.Length; i++)
            {
                int a = (int)Adress[i];
                Matrix[a].Row.Clear();
                Matrix[a].Add(new SparseElement(1.0, a));
                Right[a] = Conditions[i];
            }
        }
        /// <summary>
        /// Удовлетворение ГУ (с накопительными вызовами)
        /// </summary>
        /// <param name="Condition">Значения неизвестных по адресам</param>
        /// <param name="Adress">Глабальные адреса</param>
        public override void BoundConditions(double Condition, uint[] Adress)
        {
            for (int i = 0; i < Adress.Length; i++)
            {
                int a = (int)Adress[i];
                Matrix[a].Row.Clear();
                Matrix[a].Row.Add(new SparseElement(1.0, a));
                Right[a] = Condition;
            }
        }
        /// <summary>
        /// Установка граничных условий
        /// </summary>
        /// <param name="Value"></param>
        protected void SetBCondition(ref double[] Value)
        {
            for (int i = 0; i < N; i++)
            {
                if (Matrix[i].Count == 1)
                    Value[i] = Right[i] / Matrix[i].Row[0].Elem;
            }
        }

        /// <summary>
        /// Операция определения невязки R = Matrix X - Right
        /// </summary>
        /// <param name="R">результат</param>
        /// <param name="X">умножаемый вектор</param>
        /// <param name="IsRight">знак операции = +/- 1</param>
        public override void getResidual(ref double[] R, double[] X, int IsRight = 1)
        {
            MEM.Alloc((int)FN, ref R);
            for (int i = 0; i < Matrix.Count; i++)
            {
                double sum = 0;
                for (int j = 0; j < Matrix[i].Row.Count; j++)
                {
                    int k = Matrix[i].Row[j].Knot;
                    sum += Matrix[i].Row[j].Elem * X[k];
                }
                R[i] = sum;
            }
        }

        public int CheckSYS(string mes = "")
        {
            if (Debug != true) return -1;
            Console.WriteLine("щаг отладки " + mes);
            int ai = -1; int aj = -1;
            for (int i = 0; i < N; i++)
            {
                if (double.IsNaN(Right[i]) == true || double.IsInfinity(Right[i]) == true)
                {
                    Console.WriteLine(" i =" + i.ToString());
                }
            }
            for (ai = 0; ai < N; ai++)
            {
                for (aj = 0; aj < Matrix[ai].Count; aj++)
                {
                    if (double.IsNaN(Matrix[ai].Row[aj].Elem) == true
                        || double.IsInfinity(Matrix[ai].Row[aj].Elem))
                        Console.WriteLine(" i = {0}, j = {1}", ai, aj);
                }
            }
            Console.WriteLine("матрица проверена");
            ai = -1; aj = -1;
            return -1;
        }
        /// <summary>
        /// Вывод САУ на КОНСОЛЬ
        /// </summary>
        /// <param name="flag">количество знаков мантисы</param>
        /// <param name="color">длина цветового блока</param>
        public override void Print(int flag = 0, int color = 1)
        {
            if (flag == 0)
            {
                Console.WriteLine("Matrix");
                for (int i = 0; i < N; i++)
                {
                    double[] buf = new double[N];
                    SparseRow.DeCompress(Matrix[i], ref buf);
                    Console.Write("{0} ", i);
                    for (int j = 0; j < N; j++)
                        Console.Write(" " + buf[j].ToString("F4"));
                    Console.WriteLine();
                }
                Console.WriteLine("Right");
                for (int i = 0; i < N; i++)
                    Console.Write(" " + Right[i].ToString("F4"));
                Console.WriteLine();
            }
            if (flag == 1)
            {
                Console.WriteLine("Matrix");
                for (int i = 0; i < N; i++)
                {
                    if (Matrix[i].Count == 0)
                        Console.WriteLine("Строка {0} пустая!", i);
                    for (int j = 0; j < Matrix[i].Count; j++)
                        Console.Write(" " + Matrix[i][j].Elem.ToString("F4"));
                    Console.WriteLine();
                }
                Console.WriteLine("Right");
                for (int i = 0; i < N; i++)
                    Console.Write(" " + Right[i].ToString("F4"));
                Console.WriteLine();
            }
            if (flag == 2)
            {
                Console.WriteLine("Matrix");
                for (int i = 0; i < N; i++)
                {
                    for (int j = 0; j < Matrix[i].Count; j++)
                        Console.Write("   " + Matrix[i][j].Knot.ToString() + " : " + Matrix[i].Row[j].Elem.ToString("F4"));
                    Console.WriteLine();
                }
                Console.WriteLine("Right");
                for (int i = 0; i < N; i++)
                    Console.Write(" " + Right[i].ToString("F4"));
                Console.WriteLine();
            }

        }
        public void Print(string name, List<SparseRow> Matrix)
        {
            Console.WriteLine(name);
            for (int i = 0; i < N; i++)
            {
                double[] buf = new double[N];
                SparseRow.DeCompress(Matrix[i], ref buf);
                Console.Write("{0} ", i);
                for (int j = 0; j < N; j++)
                    Console.Write(" " + buf[j].ToString("F5"));
                Console.WriteLine();
            }
        }

    }
}
