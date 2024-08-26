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
//                  Библиотека  метода граничных элементов
//                              Потапов И.И.
//                        - () Copyright 2014 -
//                          ALL RIGHT RESERVED
//                              07.10.14
//---------------------------------------------------------------------------
//                            Потапов И.И.
//                              07.04.14
//---------------------------------------------------------------------------
using CommonLib;
using System;

namespace FEMTasksLib
{
    /// <summary>
    ///  ОО: Интерфейс решателя простых линейных ГЭ задач.
    ///  Расширяет интерфейс конечно элементных задачи 
    /// </summary>
    public interface IBEMTask : IFEMTask
    {
        /// <summary>
        /// Получить потонциальное поле 
        /// </summary>
        /// <returns></returns>
        double[] GetPhi();
        /// <summary>
        /// Получить поле потока
        /// </summary>
        /// <returns></returns>
        double[] GetU();
        /// <summary>
        /// Функция Грина для двух граничыных элементов i и j
        /// </summary>
        double GreenBound(int i, int j);
        /// <summary>
        /// Нормальная производная от функции Грина для двух граничыных элементов i и j
        /// Интеграл от функции грина int_{Delta S}( F(x^p,xi^q) ) dS формула (3.53) 
        /// </summary>
        double DiffGreenBound(int i, int j);
    }
}