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
    /// <summary>
    /// Базисная конечно - элементная сетка
    /// </summary>
    public interface IFEMesh : IMesh
    {
        /// <summary>
        /// id пообласти 
        /// </summary>
        int ID { get; set; }
        /// <summary>
        /// Конечные элементы сетки
        /// </summary>
        IFElement[] AreaElems { get; set; }
        /// <summary>
        /// граничные элементы сетки
        /// </summary>
        IFElement[] BoundElems { get; set; }
        /// <summary>
        /// граничные узлы секи
        /// </summary>
        IFENods[] BNods { get; set; }
        /// <summary>
        /// Получить параметры сетки
        /// </summary>
        /// <returns></returns>
        double[][] GetParams();
        /// <summary>
        /// Координаты узлов по Х
        /// </summary>
        double[] CoordsX { get; set; }
        /// <summary>
        /// Координаты узлов по Y
        /// </summary>
        double[] CoordsY { get; set; }
        /// <summary>
        /// Координаты узловых точек и параметров определенных в них
        /// </summary>
        double[][] Params { get; set; }
        /// <summary>
        /// Установка новой сетки в текущую
        /// </summary>
        /// <param name="m"></param>
        void Set(IFEMesh m);
        /// <summary>
        /// Установка новой сетки в текущую
        /// </summary>
        /// <param name="m"></param>
        void Set(IMesh m);
        /// <summary>
        /// добавить сетку к текущей
        /// </summary>
        /// <param name="mesh"></param>
        void Add(IFEMesh mesh);
        /// <summary>
        /// взять сетку для подобласти
        /// </summary>
        /// <param name="mesh"></param>
        IFEMesh Get(int index);
        /// <summary>
        /// взять сетку с размерностью d - 1 для участка границы 
        /// </summary>
        /// <param name="mesh"></param>
        IFEMesh GetBoundaryMesh(int index);
        /// <summary>
        /// взять сетку с размерностью d - 1 для участка границы 
        /// </summary>
        /// <param name="mesh"></param>
        IMesh GetBoundaryTwoMesh(int index);
        /// <summary>
        /// Получить координаты узлов КЭ
        /// </summary>
        /// <param name="knots"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        void ElemXY(uint[] knots, ref double[] X, ref double[] Y);
    }
}
