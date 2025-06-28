//---------------------------------------------------------------------------
//   Реализация библиотеки методов решения систем алгебраических уравнений
//                     - (C) Copyright 2021 -
//                       ALL RIGHT RESERVED
//                    разработка: Потапов И.И.
//---------------------------------------------------------------------------
//                        Потапов И.И.
//                       25.06.2001 C++
//---------------------------------------------------------------------------
//                  перенос с C++ ==> C#
//                       18.04.2021    
//---------------------------------------------------------------------------
//               Изменение имен полей класса
//                       31.05.2025
//---------------------------------------------------------------------------
using System;

namespace AlgebraLib
{
    /// <summary>
    /// ОО: Используется для хранения разреженных матриц формате CRS
    /// </summary>
    public class SparseElement : IComparable<SparseElement>
    {
        /// <summary>
        /// значение ненулевого элемента в строке
        /// </summary>
        public double Value;
        /// <summary>
        /// Индекс столбца (j) (ненулевого элемента в строке)
        /// </summary>
        public int IndexColumn;
        public SparseElement() { }
        public SparseElement(SparseElement e)
        {
            Value = e.Value; IndexColumn = e.IndexColumn;
        }
        public SparseElement(double Value, int IndexColumn)
        {
            this.Value = Value;
            this.IndexColumn = IndexColumn;
        }
        public static bool operator <=(SparseElement a, SparseElement b)
        {
            return a.IndexColumn <= b.IndexColumn;
        }
        public static bool operator >=(SparseElement a, SparseElement b)
        {
            return a.IndexColumn >= b.IndexColumn;
        }
        public int CompareTo(SparseElement b)
        {
            if (IndexColumn > b.IndexColumn)
                return 1;
            else if (IndexColumn < b.IndexColumn)
                return -1;
            else
                return 0;
        }
    }
}
