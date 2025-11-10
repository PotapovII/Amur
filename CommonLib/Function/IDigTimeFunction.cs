#region License
/*
Copyright (c) 2019 - 2025  Potapov Igor Ivanovich, Khabarovsk

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
//                 кодировка : 15.07.2025 Потапов И.И.
//---------------------------------------------------------------------------
using System;
using System.IO;

namespace CommonLib.Function
{
    /// <summary>
    /// ОО: Данные для функциональных зависимостей
    /// временная функция
    /// </summary>
    public interface IDigTimeFunction
    {
        /// <summary>
        /// Имя функции/данных
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Загрузка данных из файлов
        /// </summary>
        /// <param name="file"></param>
        void Load(StreamReader file);
        /// <summary>
        /// Сохранение данных в файл
        /// </summary>
        /// <param name="file"></param>
        void Save(StreamWriter file);
        /// <summary>
        /// Получение интерполированного (линейно) не сглаженного значения данных 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        double FunctionValue(DateTime x);
        /// <summary>
        /// Создание цифровой функции с аргументом 
        /// в секундах flag = 0,  минутах flag = 1, часах flag = 2, днях flag = 3
        /// </summary>
        /// <returns></returns>
        /// <param name="flag"></param>
        /// <returns></returns>
        IDigFunction CreateDigFunction(int flag = 0);
    }
}
