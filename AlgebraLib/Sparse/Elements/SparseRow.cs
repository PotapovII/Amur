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
using MemLogLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AlgebraLib
{
    /// <summary>
    /// ОО: Строка разреженной матрицы формате CRS
    /// </summary>
    public class SparseRow
    {
        public static double DEPS = 10e-10;
        /// <summary>
        /// Упакованная строка
        /// </summary>
        public List<SparseElement> Row;
        /// <summary>
        /// Размер упакованной строки
        /// </summary>
        public int Count { get => Row.Count; }
        /// <summary>
        /// индексатор
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public SparseElement this[int index]
        {
            get
            {
                return Row[(int)index];
            }
            set
            {
                Row[(int)index] = value;
            }
        }
        /// <summary>
        /// Максимальное значение в строке
        /// </summary>
        /// <returns></returns>
        public double MaxRow()
        {
            return Row.Max(x => Math.Abs(x.Elem));
        }

        public SparseRow()
        {
            Row = new List<SparseElement>();
        }
        public SparseRow(SparseRow sp)
        {
            Row = new List<SparseElement>();
            foreach (var e in sp.Row)
                Row.Add(new SparseElement(e));
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
                int posx = mas[i].Knot;
                int posy = Row[j].Knot;

                if (posx == posy)
                {
                    tRow.Add(new SparseElement(mas[i].Elem + Row[j].Elem, posx));
                    i++;
                    j++;
                }
                else
                    if (posx < posy)
                {
                    tRow.Add(new SparseElement(mas[i].Elem, posx));
                    i++;
                }
                else //if (posx > posy)
                {
                    tRow.Add(new SparseElement(Row[j].Elem, posx));
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
                    posx = row[i].Knot;// (int)knots[i];
                if (j == Row.Count)
                    posy = -1;
                else
                    posy = Row[j].Knot;
                if (posx == -1 && posy == -1)
                    break;
                if (posy == -1)
                {
                    tRow.Add(new SparseElement(row[i].Elem, posx));
                    i++;
                }
                else
                {
                    if (posx == -1)
                    {
                        tRow.Add(new SparseElement(Row[j].Elem, posy));
                        j++;
                    }
                    else
                    if (posx == posy)
                    {
                        val = row[i].Elem + Row[j].Elem;
                        if (Math.Abs(val) > DEPS)
                            tRow.Add(new SparseElement(val, posx));
                        i++;
                        j++;
                    }
                    else
                        if (posx < posy)
                    {
                        tRow.Add(new SparseElement(row[i].Elem, posx));
                        i++;
                    }
                    else
                    {
                        tRow.Add(new SparseElement(Row[j].Elem, posy));
                        j++;
                    }
                }
            }
            Row = tRow;
            for (int ai = 0; ai < Row.Count; ai++)
            {
                if (double.IsNaN(Row[ai].Elem) == true || double.IsInfinity(Row[ai].Elem) == true)
                    throw new Exception("Косяку в SparseRow.Add() index = " + ai.ToString());
            }
        }
        /// <summary>
        ///   Создает плотный вектор.
        /// </summary>
        public void DeCompress(ref double[] result)
        {
            for (int j = 0; j < Row.Count; j++)
                result[Row[j].Knot] += Row[j].Elem;
        }
        /// <summary>
        /// Сложение двух строк
        /// </summary>
        public static SparseRow operator +(SparseRow a, SparseRow b)
        {
            SparseRow tRow = new SparseRow();
            int i = 0, j = 0;
            double val;
            while (i < a.Row.Count && j < b.Row.Count)
            {
                int posx = a[i].Knot;
                int posy = b[j].Knot;

                if (posx == posy)
                {
                    val = a[i].Elem + b[j].Elem;
                    if (Math.Abs(val) > DEPS)
                        tRow.Row.Add(new SparseElement(val, posx));
                    i++;
                    j++;
                }
                else
                    if (posx < posy)
                {
                    tRow.Row.Add(new SparseElement(a[i].Elem, posx));
                    i++;
                }
                else //if (posx > posy)
                {
                    tRow.Row.Add(new SparseElement(b[j].Elem, posx));
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
                int posx = a[i].Knot;
                int posy = b[j].Knot;

                if (posx == posy)
                {
                    val = a[i].Elem - b[j].Elem;
                    if (Math.Abs(val) > DEPS)
                        tRow.Row.Add(new SparseElement(val, posx));
                    i++;
                    j++;
                }
                else
                    if (posx < posy)
                {
                    tRow.Row.Add(new SparseElement(a[i].Elem, posx));
                    i++;
                }
                else //if (posx > posy)
                {
                    tRow.Row.Add(new SparseElement(-b[j].Elem, posx));
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
                int posx = a[i].Knot;
                int posy = b[j].Knot;

                if (posx == posy)
                {
                    sum += a[i].Elem * b[j].Elem;
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
                e.Elem *= a;
            return b;
        }
        /// <summary>
        /// Скалярное произведение числа на строку (справа)
        /// </summary>
        public static SparseRow operator *(SparseRow b, double a)
        {
            foreach (var e in b.Row)
                e.Elem *= a;
            return b;
        }
        // <summary>
        /// Скалярное произведение числа на строку (слева)
        /// </summary>
        public static double operator *(double[] a, SparseRow b)
        {
            double sum = 0;
            foreach (var e in b.Row)
                sum += e.Elem * a[e.Knot];
            return sum;
        }

        // <summary>
        /// Скалярное произведение числа на строку (слева)
        /// </summary>
        public static double operator *(SparseRow b, double[] a)
        {
            double sum = 0;
            foreach (var e in b.Row)
                sum += e.Elem * a[e.Knot];
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
                result[a[j].Knot] += a[j].Elem;
            return result;
        }
        /// <summary>
        ///   Добавляет разреженный вектор в плотный вектор.
        /// </summary>
        public static void DeCompress(SparseRow a, ref double[] result, int coef = 1)
        {
            for (int j = 0; j < a.Row.Count; j++)
                result[a[j].Knot] += a[j].Elem * coef;
        }
        /// <summary>
        ///  Собирает разреженный вектор из плотного вектора.
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
