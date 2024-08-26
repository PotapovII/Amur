#region License
/*
Copyright (c) 2019 - 2024  Potapov Igor Ivanovich, Khabarovsk

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion
//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 22.12.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace CommonLib.Areas
{
    using System;
    using System.Collections.Generic;
    /// <summary>
    /// Метка границы (список сегментов)
    /// </summary>
    public interface IMBoundary : IComparable<IMBoundary>
    {
        /// <summary>
        /// Индекс метки границы
        /// </summary>
        int ID { get; set; }
        /// <summary>
        /// наименование фигуры
        /// </summary>
        string FiguraName { get; set; }
        /// <summary>
        /// Индекс фигуры
        /// </summary>
        int FID { get; set; }
        /// <summary>
        /// список индексов сегментов
        /// </summary>
        List<int> SegmentIndex { get; set; }
        /// <summary>
        /// Имя метки границы
        /// </summary>
        string Name { get; set; }
    }
}
