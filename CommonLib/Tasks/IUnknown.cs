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
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2021 -
//                       ALL RIGHT RESERVED
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                          25 08.22
//---------------------------------------------------------------------------
//                     рефакт. 11 02.24
//---------------------------------------------------------------------------
namespace CommonLib
{
    /// <summary>
    /// Описание неизвестной
    /// </summary>
    public interface IUnknown
    {
        ///// <summary>
        ///// Значения в узлах сетки
        ///// </summary>
        double[] ValuesX { get; }
        ///// <summary>
        ///// Значения в узлах сетки
        ///// </summary>
        double[] ValuesY { get; }
        ///// <summary>
        ///// Значения в узлах сетки
        ///// </summary>
        int Dimention { get; }
        /// <summary>
        /// Название неизвестных задачи
        /// </summary>
        string NameUnknow { get; }
        /// <summary>
        /// Тип неизвестной задачи (меняется во времени или нет) - необходимо наальное условие или нет
        /// </summary>
        bool TypeUnknow { get;  }
        /// <summary>
        /// Апроксимация неизвестной задачи ????
        /// </summary>
        TypeFunForm ApproxUnknow { get; set; }
    }
}
