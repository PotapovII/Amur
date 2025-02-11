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
//               Источник код С++ HAlgebra проект MixTasker
//                      - (C) Copyright 2002 -
//                       ALL RIGHT RESERVED
//                   разработка: Потапов И.И.
//                          28.05.02
//---------------------------------------------------------------------------
//     Адаптер классов реализующих решение систем алгебраических уравнений
//                      - (C) Copyright 2011 -
//                       ALL RIGHT RESERVED
//                   разработка: Потапов И.И.
//                            28.07.11
//---------------------------------------------------------------------------
//   Реализация библиотеки методов решения систем алгебраических уравнений
//                     - (C) Copyright 2021 -
//                       ALL RIGHT RESERVED
//                   разработка: Потапов И.И.
//                          15.02.2021 
//---------------------------------------------------------------------------
namespace CommonLib
{
    /// <summary>
    /// ОО: Общий интерфейс Алгебры
    /// </summary>
    public interface IAlgebra
    {
        /// <summary>
        /// Информация о полученном результате
        /// </summary>
        IAlgebraResult Result { get; }
        /// <summary>
        /// Название метода
        /// </summary>
        string Name { get; }
        /// <summary>
        /// порядок СЛУ
        /// </summary>
        uint N { get; }
        /// <summary>
        /// <summary>
        /// Очистка матрицы и правой части
        /// </summary>
        void Clear();
        /// <summary>
        /// Сборка ГМЖ
        /// </summary>
        void AddToMatrix(double[][] LMartix, uint[] Adress);
        /// <summary>
        /// // Сборка ГПЧ
        /// </summary>
        void AddToRight(double[] LRight, uint[] Adress);
        /// <summary>
        /// Сборка САУ по строкам (не для всех решателей)
        /// </summary>
        /// <param name="ColElems">Коэффициенты строки системы</param>
        /// <param name="ColAdress">Адреса коэффицентов</param>
        /// <param name="IndexRow">Индекс формируемой строки системы</param>
        /// <param name="Right">Значение правой части</param>
        void AddStringSystem(double[] ColElems, uint[] ColAdress, uint IndexRow, double R);
        /// <summary>
        /// Получить строку (не для всех решателей)
        /// </summary>
        /// <param name="IndexRow">Индекс получемой строки системы</param>
        /// <param name="ColElems">Коэффициенты строки системы</param>
        /// <param name="R">Значение правой части</param>
        void GetStringSystem(uint IndexRow, ref double[] ColElems, ref double R);
        /// <summary>
        /// Добавление в правую часть
        /// </summary>
        void CopyRight(double[] CRight);
        /// <summary>
        /// Получение правой части СЛАУ
        /// </summary>
        /// <param name="CRight"></param>
        void GetRight(ref double[] CRight);
        /// <summary>
        /// Удовлетворение ГУ
        /// </summary>
        void BoundConditions(double[] Conditions, uint[] Adress);
        /// <summary>
        /// Выполнение ГУ
        /// </summary>
        void BoundConditions(double Conditions, uint[] Adress);
        /// <summary>
        /// Операция определения невязки R = Matrix X - Right
        /// </summary>
        /// <param name="R">результат</param>
        /// <param name="X">умножаемый вектор</param>
        /// <param name="IsRight">знак операции = +/- 1</param>
        void getResidual(ref double[] R, double[] X, int IsRight = 1);
        /// <summary>
        /// Решение СЛУ
        /// </summary>
        void Solve(ref double[] X);
        /// <summary>
        /// Клонирование объекта
        /// </summary>
        /// <param name="N">порядок системы</param>
        /// <returns></returns>
        IAlgebra Clone();
        /// <summary>
        /// Вывод САУ на КОНСОЛЬ
        /// </summary>
        /// <param name="flag">количество знаков мантисы</param>
        /// <param name="color">длина цветового блока</param>
        void Print(int flag = 0, int color = 1);
    }
}

