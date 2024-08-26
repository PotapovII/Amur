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
////---------------------------------------------------------------------------
////                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
////                         проектировщик:
////                           Потапов И.И.
////---------------------------------------------------------------------------
////                 кодировка : 09.10.2023 Потапов И.И.
////---------------------------------------------------------------------------
//namespace CommonLib
//{
//    using System;
//    /// <summary>
//    /// ОО: Отсылка данных для отрисовки
//    /// </summary>
//    /// <param name="a"></param>
//    [Serializable]
//    public delegate void SendParamCloud(ISaveCloud a);
//    /// <summary>
//    /// Адаптер точки сохранения для облака данных 
//    /// </summary>
//    public interface ISaveCloud
//    {
//        /// <summary>
//        /// Облако данных - множество геометрических точкек со свойствами привязанными к ним
//        /// </summary>
//        ICloud cloud { get; }
//        /// <summary>
//        /// Время сохранения
//        /// </summary>
//        double time { get; }
//        /// <summary>
//        /// Установка времени и сетки кадра, и (или) графиков
//        /// </summary>
//        /// <param name="time"></param>
//        /// <param name="mesh"></param>
//        /// <param name="gd"></param>
//        void SetSaveCloud(double time, ICloud mesh);
//        /// <summary>
//        /// Добавление поля привязанного к узлам сетки
//        /// </summary>
//        /// <param name="Name">Название поля</param>
//        /// <param name="Value">Значение поля в узлах сетки</param>
//        void Add(string Name, double[] Value);
//        /// <summary>
//        /// Добавление векторного поля привязанного к узлам сетки
//        /// </summary>
//        /// <param name="Name">Название поля</param>
//        /// <param name="Vx">Аргументы функции поля</param>
//        /// <param name="Vy">Значение функции поля</param>
//        void Add(string Name, double[] Vx, double[] Vy);
//        /// <summary>
//        /// Сериализация sp
//        /// </summary>
//        /// <param name="sp">точка сохранения</param>
//        /// <param name="NameSave">путь,имя файла</param>
//        void SerializableSavePoint(ISaveCloud sp, string NameSave);
//        /// <summary>
//        /// Загрузка точки сохранения
//        /// </summary>
//        /// <param name="FileName">путь,имя файла</param>
//        /// <returns></returns>
//        ISaveCloud LoadSaveCloud(string FileName);
//    }
//}