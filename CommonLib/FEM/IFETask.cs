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
//                    Проект "Home" c++
//              Интерфейс задачи VBaseLineTask
//                  - (C) Copyright 2003
//                        Потапов И.И.
//                         14.11.03
//---------------------------------------------------------------------------
//    сильно, упрощенный перенос VBaseLineTask с c++ на c#,
//    не реализована концепция адресных таблиц необходтмых
//      для произвольной аппроксимации полей задачи над
//                     опорной сеткой,
//          не реализована концепция стека задач и 
//    буферов обмена полями и параметрами между задачами
//                       Потапов И.И.
//                        07.04.2021
//---------------------------------------------------------------------------
//  Для демонстрации простых тестовых FEM реализаций задач Пуассона, Ламе...
//---------------------------------------------------------------------------
namespace CommonLib
{
    /// <summary>
    ///  ОО: Интерфейс решателя простых линейных КЭ задач.
    ///  Реализует патерн стратегия для сетки задачи 
    ///  и метода решения системы алгебраических уравнений
    /// </summary>
    public interface IFEMTask : IBaseFEMTask
    {
        /// <summary>
        /// Решение задачи дискретизации задачи и ее рещателя
        /// </summary>
        /// <param name="result">результат решения</param>
        void SolveTask(ref double[] result);
    }
}