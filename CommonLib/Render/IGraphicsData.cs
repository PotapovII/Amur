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
//                 кодировка : 14.12.2020 Потапов И.И.
//---------------------------------------------------------------------------
namespace CommonLib
{
    using System.Collections.Generic;
    /// <summary>
    /// ОО. Класс контейнер для отрисовки кривых
    /// </summary>
    public interface IGraphicsData
    {
        /// <summary>
        /// Количество кривых
        /// </summary>
        int Count { get; }
        /// <summary>
        /// Сложение объеков GraphicsData
        /// </summary>
        void Add(IGraphicsData ig);
        /// <summary>
        /// Очистка данных по кривым
        /// </summary>
        void Clear(TypeGraphicsCurve TGraphicsCurve = TypeGraphicsCurve.TimeCurve);
        /// <summary>
        /// Имена кривых
        /// </summary>
        /// <returns></returns>
        List<string> GraphicNames();
        /// <summary>
        /// Имена групп кривых
        /// </summary>
        /// <returns></returns>
        List<string> GraphicGroupNames();
        /// <summary>
        /// Добавление кривой
        /// </summary>
        /// <param name="e"></param>
        void Add(IGraphicsCurve e);
        /// <summary>
        /// Добавление кривой
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        void AddCurve(string Name, double[] x, double[] y);
        /// <summary>
        /// Удаление кривой по имени
        /// </summary>
        /// <param name="Name">Название поля</param>
        void RemoveCurve(string Name);
        /// <summary>
        /// Получить подмножество IGraphicsData с кривыми заданного типа 
        /// в активных подгруппах
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IGraphicsData GetSubIGraphicsData(TypeGraphicsCurve TGraphicsCurve, List<string> filter);
    }
}
