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
//                 кодировка : 25.11.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace CommonLib.Areas
{
    using System.Collections.Generic;
    using System.Drawing;
    /// <summary>
    /// Расчетная область
    /// </summary>
    public interface IMArea
    {
        /// <summary>
        /// Количество фигур
        /// </summary>
        int Count { get; }
        /// <summary>
        /// Количество подобластей
        /// </summary>
        int CountSubAreas { get; }
        /// <summary>
        /// Количество дырок
        /// </summary>
        int CountHoles { get; }
        /// <summary>
        /// Список фигур
        /// </summary>
        List<IMFigura> Figures { get; }
        /// <summary>
        /// Метки границ
        /// </summary>
        List<IMBoundary> BoundMark { get; }
        /// <summary>
        /// Взять/дать фигуру 
        /// </summary>
        IMFigura this[int index] {get;set;}
        /// <summary>
        /// удаление меток при удалении фигуры
        /// </summary>
        void RemoveBoundMark(string FigName);
        /// <summary>
        /// текущий интекс метки границ
        /// </summary>
        int SelectIndexBoundMark { get; set; }
        /// <summary>
        /// Получить индекс фигуры по имени метки
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        int GetFigIndexByMarkName(string name);
        /// <summary>
        /// Получить метку по имени метки
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IMBoundary GetMBoundaryByMarkName(string name);
        /// <summary>
        ///  Взять активную метку
        /// </summary>
        /// <returns></returns>
        IMBoundary GetSelection();
        /// <summary>
        /// Установка индекса активной метки
        /// </summary>
        /// <param name="index"></param>
        void SetSelectIndex(int index);
        /// <summary>
        /// Список фигур
        /// </summary>
        List<string> Names { get; }
        /// <summary>
        /// Взять фигуру
        /// </summary>
        IMFigura Get(int index);
        /// <summary>
        /// Добавить фигуру
        /// </summary>
        /// <param name="point"></param>
        void Add(IMFigura fig);
        /// <summary>
        /// Убрать фигуру
        /// </summary>
        /// <param name="point"></param>
        void Remove(IMFigura fig);
        /// <summary>
        /// установить статус фигур
        /// </summary>
        void SetFigureStatus(FigureStatus status);
        /// <summary>
        /// установить типы фигур
        /// </summary>
        void SetFigureTypes(FigureType type);
        /// <summary>
        /// Получить регион
        /// </summary>
        RectangleWorld GetRegion();
        /// <summary>
        /// Установить регион
        /// </summary>
        /// <param name="a">левая нижняя точка</param>
        /// <param name="b">правая верхняя точка</param>
        void SetRegion(PointF a, PointF b);
        /// <summary>
        /// Удалить фигуры
        /// </summary>
        void Clear();
        /// <summary>
        /// Создание новой метки границ для текущей фигуры
        /// </summary>
        /// <param name="FID">номер фигуры</param>
        /// <param name="idx">индексы границ</param>
        void AddBoundMark(int FID, int[] idx);
        /// <summary>
        /// Контур области определен
        /// </summary>
        /// <returns></returns>
        bool Ready();
    }
}
