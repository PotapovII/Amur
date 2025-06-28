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
//                 кодировка : 30.09.2024 Потапов И.И.
//---------------------------------------------------------------------------
//                  + 
//                 кодировка : 29.03.2025 Потапов И.И.
//---------------------------------------------------------------------------
namespace CommonLib
{
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    /// <summary>
    /// структура треугольника, содержащая 3 его вершины и флаг
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Triangles
    {
        /// <summary>
        /// Первый узел треугольника
        /// </summary>
        public int i;
        /// <summary>
        /// Второй узел треугольника
        /// </summary>
        public int j;
        /// <summary>
        /// Третий узел треугольника
        /// </summary>
        public int k;
        /// <summary>
        /// Флаг, обозначающий атрибуты тройки вершин (вхождение в область и т.п.)
        /// flag = 1 - принадлежит области, 0 - не принадлежит области
        /// </summary>
        public int flag;

        public Triangles(int vertex1, int vertex2, int vertex3, int flag = 0)
        {
            i = vertex1;
            j = vertex2;
            k = vertex3;
            this.flag = flag;
        }
        /// <summary>
        /// Получить вершину треугольника из тройки
        /// </summary>
        /// <param name="index">индекс вершины треугольника (относительно самого треугольника, внутренний индекс) [0..2]. <br/>
        /// Если передать другое число, то результатом будет вершины по внутреннему индексу 2
        /// </param>
        /// <returns>Индекс вершины треугольника в общем массиве точек</returns>
        public int this[int index]
        {
            get => index == 0 ? i : index == 1 ? j : k;
            set 
            { 
                if (index == 0) i = value; 
                else 
                    if (index == 1) 
                        j = value; 
                    else 
                        k = value; 
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (int i, int j, int k) Get() => (i, j, k);

        /// <summary>
        /// Преобразовать в треугольник
        /// </summary>
        public TriElement GetTriElement => new TriElement((uint)i, (uint)j, (uint)k);
        /// <summary>
        /// Строка
        /// </summary>
        /// <returns></returns>
        public new string ToString()
        {
            return i.ToString() + " " +
                   j.ToString() + " " +
                   k.ToString() + " " +
                   flag.ToString() + ": ";
        }
    }
}