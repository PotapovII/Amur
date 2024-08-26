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
//   Реализация библиотеки методов решения систем алгебраических уравнений
//                     - (C) Copyright 2021 -
//                       ALL RIGHT RESERVED
//                   разработка: Потапов И.И.
//                          15.02.2021 
//---------------------------------------------------------------------------
namespace CommonLib
{
    using System;
    public enum ErrorType
    {
        /// <summary>
        /// ошибок нет
        /// </summary>
        notError = 0,
        /// <summary>
        /// переполнение переменных
        /// </summary>
        variableOverflow,
        /// <summary>
        /// деление на ноль
        /// </summary>
        divisionByZero,
        /// <summary>
        /// стагнация сходимости
        /// </summary>
        convergenceStagnation,
        /// <summary>
        /// нехватка памяти
        /// </summary>
        outOfMemory,
        /// <summary>
        /// другие проблемы
        /// </summary>
        methodCannotSolveSuchSLAEs,
        /// <summary>
        /// другие проблемы
        /// </summary>
        other
    }

    /// <summary>
    /// Результат решения задачи
    /// </summary>
    public interface IAlgebraResult
    {
        /// <summary>
        /// Время работы метода
        /// </summary>
        TimeSpan ts { get; }
        /// <summary>
        /// Время работы метода в строковом формате
        /// </summary>
        string elapsedTime { get; }
        /// <summary>
        /// Порядок системы
        /// </summary>
        uint N { get; set; }
        /// <summary>
        /// Имя метода
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Сообщение о решении
        /// </summary>
        string Message { get; set; }
        /// <summary>
        /// тип ошибки
        /// </summary>
        ErrorType errorType { get; set; }
        /// <summary>
        /// решение СЛАУ
        /// </summary>
        double[] X { get; set; }
        /// <summary>
        /// число обусловленности
        /// </summary>
        double conditionality { get; set; }
        void Start();
        void Stop();
        void Print();
    }
}
