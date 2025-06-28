//---------------------------------------------------------------------------
//   Реализация библиотеки методов решения систем алгебраических уравнений
//                     - (C) Copyright 2021 -
//                       ALL RIGHT RESERVED
//                    разработка: Потапов И.И.
//                          25.06.2001
//---------------------------------------------------------------------------
//                        Потапов И.И.
//                   - (C) Copyright 2001 -
//                     ALL RIGHT RESERVED
//              member-функции класса HPackVector
//                  Last Edit Data: 25.6.2001
//---------------------------------------------------------------------------
//                        Потапов И.И.
//          HPackVector (C++) ==> SparseRow (C#)
//                       18.04.2021 
//---------------------------------------------------------------------------
//                        Потапов И.И.
//              добавление поля индекс строки RowNumber
//                       30.05.2025 
//---------------------------------------------------------------------------
namespace AlgebraLib
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    using MemLogLib;
    using System.Net;

    /// <summary>
    /// ОО: Строка разреженной матрицы формате CRS
    /// </summary>
    public class SparseRow
    {
        static double DEPS = MEM.Error10;
        /// <summary>
        /// Упакованная строка
        /// </summary>
        public List<SparseElement> Row;
        /// <summary>
        /// Индекс строки
        /// </summary>
        public int RowNumber;
        #region Свойства
        /// <summary>
        /// Количество не нулевых элементов
        /// </summary>
        public int Count { get => Row.Count; }
        /// <summary>
        /// индексатор
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public SparseElement this[int index]
        {
            get => Row[(int)index];
            set => Row[(int)index] = value;
        }
        #endregion

        public SparseRow()
        {
            RowNumber = 0;
            Row = new List<SparseElement>();
        }
        public SparseRow(SparseRow sp)
        {
            RowNumber = sp.RowNumber;
            Row = new List<SparseElement>();
            foreach (var e in sp.Row)
                Row.Add(new SparseElement(e));
        }
        #region Расширение функционала в связи добавлением поля RowNumber
        //public SparseRow(SparseRow currentRow)
        //{
        //    RowNumber = currentRow.RowNumber;
        //    Row = new List<SparseElement>();
        //    Row.AddRange(currentRow.Row);
        //}
        public SparseRow(int RowNumber, int initialSize, int maxColNumber)
        {
            this.RowNumber = RowNumber;
            Row = new List<SparseElement>(initialSize);
            SetZeroValue(RowNumber);
        }
        /// <summary>
        /// Получит значение по позиции в списке
        /// </summary>
        public double ValueByIndex(int index)
        {
            return Row[index].Value;
        }
        /// <summary>
        /// Получит индекс столбца по позиции в списке
        /// </summary>
        /// <param name="index">позиция столбца в списке</param>
        /// <returns></returns>
        public int ColumnByIndex(int index)
        {
            return Row[index].IndexColumn;
        }
        /// <summary>
        /// Добавить 0 элемент в строку 
        /// </summary>
        /// <param name="ncolumn"></param>
        public void SetZeroValue(int ncolumn)
        {
            int newFlag = 1;
            for (int i = 0; i < Row.Count; i++)
            {
                if (ncolumn == Row[i].IndexColumn)
                {
                    Row[i].Value = 0.0;
                    newFlag = 0;
                    break;
                }
            }
            if (newFlag != 0)
            {
                SparseElement newEntry = new SparseElement(0.0, ncolumn);
                Row.Add(newEntry);
            }
        }
        /// <summary>
        /// Добавить Value элемент в строку 
        /// </summary>
        /// <param name="ncolumn"></param>
        public void AddValue(int ncolumn, double Value)
        {
            if (MEM.Equals(Value, 0, MEM.Error10) == false)
                Row.Add(new SparseElement(Value, ncolumn));
        }
        /// <summary>
        /// Получить значение по индексу столбца, линейный поиск (
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public double Value(int ncolumn)
        {
            for (int i = 0; i < Row.Count; i++)
                if (ncolumn == Row[i].IndexColumn)
                    return Row[i].Value;
            return 0;
        }
        /// <summary>
        /// Установить по адресу index элемент Value в столбец ncolumn
        /// </summary>
        /// <param name="index"></param>
        /// <param name="ncolumn"></param>
        /// <param name="Value"></param>
        public void SetValue(int index, int ncolumn, double Value)
        {
            if (index < Count)
            {
                Row[index].IndexColumn = ncolumn;
                Row[index].Value = Value;
            }
        }
        /// <summary>
        /// Сложение строк
        /// </summary>
        /// <param name="factor">масштабный коэффициент</param>
        /// <param name="pivotRow">добавляемая строка</param>
        /// <param name="indexArray">хеш масив</param>
        public void AddRange(double factor, SparseRow pivotRow,ref int[] indexArray)
        {
            for (int i = 0; i < Count; i++)
                indexArray[Row[i].IndexColumn] = i;

            for (int j = 0; j < pivotRow.Count; j++)
            {
                int IndexColumn = pivotRow.Row[j].IndexColumn;
                int targetIndex = indexArray[IndexColumn];
                if (targetIndex > -1)
                    Row[targetIndex].Value += factor * pivotRow.Row[j].Value;
                else
                    if (MEM.Equals(pivotRow.Row[j].Value, 0, MEM.Error12) == false)
                {
                    SparseElement fillEntry = new 
                        SparseElement(pivotRow.Row[j].Value, pivotRow.Row[j].IndexColumn);
                    Row.Add(fillEntry);
                }
            }
            for (int i = 0; i < Count; i++)
                indexArray[Row[i].IndexColumn] = -1;
        }
        /// <summary>
        /// вычитание строки
        /// </summary>
        /// <param name="pivotRow"></param>
        /// <param name="indexArray"></param>
        public void ReduceRow(SparseRow pivotRow, int[] indexArray)
        {
            SparseElement pivotEntry = pivotRow.Row[0];

            for (int i = 0; i < Count; i++)
                indexArray[Row[i].IndexColumn] = i;

            if (indexArray[pivotEntry.IndexColumn] > -1)
            {
                int targetIndexI = indexArray[pivotRow.Row[0].IndexColumn];
                if (MEM.Equals(pivotEntry.Value, 0, MEM.Error10) == true)
                {
                    Console.WriteLine("Value {0} i:{1} j:{2}", pivotEntry.Value, pivotRow.RowNumber, pivotEntry.IndexColumn);
                }
                double factor = Row[targetIndexI].Value / pivotEntry.Value;
                Row[targetIndexI].Value = factor;

                for (int j = 1; j < pivotRow.Count; j++)
                {
                    if (pivotRow.Row[j].IndexColumn > pivotRow.RowNumber)
                    {
                        int targetIndexJ = indexArray[pivotRow.Row[j].IndexColumn];
                        if (targetIndexJ > -1)
                        {
                            double Value = Row[targetIndexJ].Value - factor * pivotRow.Row[j].Value;
                            if (MEM.Equals(Value, 0, MEM.Error10) == true)
                            {
                                indexArray[Row[targetIndexJ].IndexColumn] = -1;
                                Row.Remove(Row[targetIndexJ]);
                            }
                            else
                                Row[targetIndexJ].Value = Value;
                        }
                    }
                }
            }
            for (int i = 0; i < Count; i++)
                indexArray[Row[i].IndexColumn] = -1;
        }
        /// <summary>
        /// Умножение столбца rhs на строку из нижней треугольной матрицы
        /// и вычитание результата из него
        /// A = LU,      rhs -= rhs * L
        /// </summary>
        /// <param name="rhs"></param>
        public void ScalarLeftSubRow(double[] rhs)
        {
            double sum = rhs[RowNumber];
            for (int i = 0; i < Row.Count; i++)
                if (Row[i].IndexColumn < RowNumber)
                    sum -= Row[i].Value * rhs[Row[i].IndexColumn];
            rhs[RowNumber] = sum;
        }
        /// <summary>
        /// Умножение столбца rhs на строку из верхней треугольной матрицы
        /// и вычитание результата из него
        /// A = LU,      rhs -= rhs * U
        /// </summary>
        /// <param name="rhs"></param>
        public void ScalarRightSubRow(double[] rhs)
        {
            double sum = rhs[RowNumber];
            for (int i = 0; i < Row.Count; i++)
                if (Row[i].IndexColumn > RowNumber)
                    sum -= Row[i].Value * rhs[Row[i].IndexColumn];
            rhs[RowNumber] = sum / Row[0].Value;
        }
        /// <summary>
        /// Скалярное умножение полной строки vector на плотную строку CRS
        /// </summary>
        public double RowDot(double[] vector)
        {
            double sum = 0.0;
            for (int i = 0; i < Row.Count; i++)
                sum += Row[i].Value * vector[Row[i].IndexColumn];
            return sum;
        }
        public void Clear()
        {
            Row.Clear();
        }
        #endregion
        /// <summary>
        /// Максимальное значение в строке
        /// </summary>
        /// <returns></returns>
        public double MaxRow()
        {
            return Row.Max(x => Math.Abs(x.Value));
        }

       

        /// <summary>
        /// добавление нового значения элемента
        /// </summary>
        /// <param name="element"></param>
        public void Add(SparseElement element)
        {
            Row.Add(element);
        }
        /// <summary>
        /// добавление элементов в строку
        /// </summary>
        /// <param name="element"></param>
        public void Add(SparseElement[] mas)
        {
            List<SparseElement> tRow = new List<SparseElement>();
            Array.Sort(mas);
            int i = 0, j = 0;
            while (i < mas.Length && j < Row.Count)
            {
                int posx = mas[i].IndexColumn;
                int posy = Row[j].IndexColumn;

                if (posx == posy)
                {
                    tRow.Add(new SparseElement(mas[i].Value + Row[j].Value, posx));
                    i++;
                    j++;
                }
                else
                    if (posx < posy)
                {
                    tRow.Add(new SparseElement(mas[i].Value, posx));
                    i++;
                }
                else //if (posx > posy)
                {
                    tRow.Add(new SparseElement(Row[j].Value, posx));
                    j++;
                }
            }
            Row = tRow;
            Row.Sort();
        }
        /// <summary>
        /// добавление элементов в строку
        /// </summary>
        /// <param name="mas">елемент</param>
        /// <param name="knots">индекс колонки</param>
        public void Add(double[] mas, uint[] knots)
        {
            List<SparseElement> row = new List<SparseElement>();
            for (int k = 0; k < knots.Length; k++)
            {
                if(MEM.Equals(mas[k], 0, MEM.Error10) == false)
                    row.Add(new SparseElement(mas[k], (int)knots[k]));
            }
            row.Sort();
            //
            int i = 0, j = 0;
            int posx;
            int posy;
            double val;
            int SCount = row.Count + Row.Count;
            List<SparseElement> tRow = new List<SparseElement>();
            for (int k = 0; k < SCount; k++)
            {
                if (i == row.Count)
                    posx = -1;
                else
                    posx = row[i].IndexColumn;// (int)knots[i];
                if (j == Row.Count)
                    posy = -1;
                else
                    posy = Row[j].IndexColumn;
                if (posx == -1 && posy == -1)
                    break;
                if (posy == -1)
                {
                    tRow.Add(new SparseElement(row[i].Value, posx));
                    i++;
                }
                else
                {
                    if (posx == -1)
                    {
                        tRow.Add(new SparseElement(Row[j].Value, posy));
                        j++;
                    }
                    else
                    if (posx == posy)
                    {
                        val = row[i].Value + Row[j].Value;
                        if (Math.Abs(val) > DEPS)
                            tRow.Add(new SparseElement(val, posx));
                        i++;
                        j++;
                    }
                    else
                        if (posx < posy)
                    {
                        tRow.Add(new SparseElement(row[i].Value, posx));
                        i++;
                    }
                    else
                    {
                        tRow.Add(new SparseElement(Row[j].Value, posy));
                        j++;
                    }
                }
            }
            Row = tRow;
            for (int ai = 0; ai < Row.Count; ai++)
            {
                if (double.IsNaN(Row[ai].Value) == true || double.IsInfinity(Row[ai].Value) == true)
                    throw new Exception("Косяку в SparseRow.Add() index = " + ai.ToString());
            }
        }
        /// <summary>
        ///   Создает плотный вектор.
        /// </summary>
        public void DeCompress(ref double[] result)
        {
            for (int j = 0; j < Row.Count; j++)
                result[Row[j].IndexColumn] += Row[j].Value;
        }
        /// <summary>
        ///   Добавляет разреженный вектор в плотный вектор.
        /// </summary>
        public static void DeCompress(SparseRow a, ref double[] result, int coef = 1)
        {
            for (int j = 0; j < a.Row.Count; j++)
                result[a[j].IndexColumn] += a[j].Value * coef;
        }

        /// <summary>
        ///  Собирает разреженный вектор из плотного вектора.
        /// <summary>
        /// Сложение двух строк
        /// </summary>
        public static SparseRow operator + (SparseRow a, SparseRow b)
        {
            SparseRow tRow = new SparseRow();
            int i = 0, j = 0;
            double val;
            while (i < a.Row.Count && j < b.Row.Count)
            {
                int posx = a[i].IndexColumn;
                int posy = b[j].IndexColumn;

                if (posx == posy)
                {
                    val = a[i].Value + b[j].Value;
                    if (Math.Abs(val) > DEPS)
                        tRow.Row.Add(new SparseElement(val, posx));
                    i++;
                    j++;
                }
                else
                    if (posx < posy)
                {
                    tRow.Row.Add(new SparseElement(a[i].Value, posx));
                    i++;
                }
                else //if (posx > posy)
                {
                    tRow.Row.Add(new SparseElement(b[j].Value, posx));
                    j++;
                }
            }
            return tRow;
        }
        /// <summary>
        /// Вычитание строки b из строки a
        /// </summary>
        public static SparseRow operator -(SparseRow a, SparseRow b)
        {
            SparseRow tRow = new SparseRow();
            int i = 0, j = 0;
            double val;
            while (i < a.Count && j < b.Count)
            {
                int posx = a[i].IndexColumn;
                int posy = b[j].IndexColumn;

                if (posx == posy)
                {
                    val = a[i].Value - b[j].Value;
                    if (Math.Abs(val) > DEPS)
                        tRow.Row.Add(new SparseElement(val, posx));
                    i++;
                    j++;
                }
                else
                    if (posx < posy)
                {
                    tRow.Row.Add(new SparseElement(a[i].Value, posx));
                    i++;
                }
                else //if (posx > posy)
                {
                    tRow.Row.Add(new SparseElement(-b[j].Value, posx));
                    j++;
                }
            }
            return tRow;
        }
        /// <summary>
        /// Скалярное произведение строк
        /// </summary>
        public static double operator *(SparseRow a, SparseRow b)
        {
            int i = 0, j = 0;
            double sum = 0;
            while (i < a.Count && j < b.Count)
            {
                int posx = a[i].IndexColumn;
                int posy = b[j].IndexColumn;

                if (posx == posy)
                {
                    sum += a[i].Value * b[j].Value;
                    i++;
                    j++;
                }
                else if (posx < posy)
                {
                    i++;
                }
                else //if (posx > posy)
                {
                    j++;
                }
            }
            return sum;
        }
        /// <summary>
        /// Скалярное произведение числа на строку (слева)
        /// </summary>
        public static SparseRow operator *(double a, SparseRow b)
        {
            foreach (var e in b.Row)
                e.Value *= a;
            return b;
        }
        /// <summary>
        /// Скалярное произведение числа на строку (справа)
        /// </summary>
        public static SparseRow operator *(SparseRow b, double a)
        {
            foreach (var e in b.Row)
                e.Value *= a;
            return b;
        }
        // <summary>
        /// Скалярное произведение числа на строку (слева)
        /// </summary>
        public static double operator *(double[] a, SparseRow b)
        {
            double sum = 0;
            foreach (var e in b.Row)
                sum += e.Value * a[e.IndexColumn];
            return sum;
        }
        // <summary>
        /// Скалярное произведение числа на строку (слева)
        /// </summary>
        public static double operator *(SparseRow b, double[] a)
        {
            double sum = 0;
            foreach (var e in b.Row)
                sum += e.Value * a[e.IndexColumn];
            return sum;
        }
        /// <summary>
        /// Перестановка строк
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static void Swap(ref SparseRow a, ref SparseRow b)
        {
            SparseRow tmp = a; a = b; b = tmp;
        }
        /// <summary>
        ///   Добавляет разреженный вектор к плотному вектору.
        /// </summary>
        /// 
        public static double[] Add(SparseRow a, double[] b, ref double[] result)
        {
            for (int j = 0; j < b.Length; j++)
                result[j] = b[j];
            for (int j = 0; j < a.Count; j++)
                result[a[j].IndexColumn] += a[j].Value;
            return result;
        }
        /// </summary>
        public static void Compress(double[] mas, ref SparseRow a)
        {
            a.Row.Clear();
            for (int j = 0; j < mas.Length; j++)
            {
                if (Math.Abs(mas[j]) > DEPS)
                    a.Add(new SparseElement(mas[j], j));
            }
        }
        public void Print()
        {
            int N = 10;
            double[] buf = new double[12];
            SparseRow.DeCompress(this, ref buf);
            for (int j = 0; j < N; j++)
                Console.Write(" " + buf[j].ToString("F4"));
            Console.WriteLine();
        }
    }
}
