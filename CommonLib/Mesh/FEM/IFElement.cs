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
//                          ПРОЕКТ  "МКЭ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 11.01.2022 Потапов И.И.
//---------------------------------------------------------------------------
namespace CommonLib
{
    using System;
    /// <summary>
    /// Элемент
    /// </summary>
    public interface IFElement : IEquatable<IFElement>
    {
        /// <summary>
        /// номер элемента
        /// </summary>
        int ID { get; set; }
        /// <summary>
        /// Метка границы 
        /// </summary>
        int MarkBC { get; set; }
        /// <summary>
        /// Количество узлов на элементе
        /// </summary>
        int Length { get; }
        /// <summary>
        /// узлы конечного элемента
        /// </summary>
        IFENods[] Nods { get; set; }
        /// <summary>
        /// Тип функции формы основного КЭ 
        /// </summary>
        TypeFunForm TFunForm { get; }
        /// <summary>
        /// индексатор
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        IFENods this[int index] { get; set; }
        /// <summary>
        /// узлы элемента
        /// </summary>
        /// <param name="knots"></param>
        void ElementKnots(ref uint[] knots);
        /// <summary>
        /// Чтение объекта из строки
        /// </summary>
        IFElement Parse(string line);
    }
}
