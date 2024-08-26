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
//                 кодировка : 21.12.2020 Потапов И.И.
//---------------------------------------------------------------------------
namespace CommonLib.DrvGraphics
{
    using CommonLib.Geometry;
    using System.Drawing;
    using System.Runtime.InteropServices;
    /// <summary>
    /// Структура TriVertex содержит информацию положении о цвете точки.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TriVertex
    {
        /// <summary>
        /// Экранная координата x левого верхнего угла прямоугольника
        /// </summary>
        public int x;
        /// <summary>
        /// Экранная координата y левого верхнего угла прямоугольника
        /// </summary>
        public int y;
        /// <summary>
        /// Информация о красном цвете в точке x, y
        /// </summary>
        public ushort Red;
        /// <summary>
        /// Информация о зеленом цвете в точке  x, y
        /// </summary>
        public ushort Green;
        /// <summary>
        /// Информация о синим цвете в точке  x, y
        /// </summary>
        public ushort Blue;
        /// <summary>
        /// альфа не используется в 
        /// </summary>
        public ushort Alpha;

        public TriVertex(int x, int y, ushort red, ushort green, ushort blue, ushort alpha = 0)
        {
            this.x = x;
            this.y = y;
            this.Red = red;
            this.Green = green;
            this.Blue = blue;
            this.Alpha = alpha;
        }

        public void SetTriVertex(ref PointF p, ref Color c)
        {
            this.x = (int)p.X;
            this.y = (int)p.Y;
            SetColor(in c);
        }
        public void SetColor(in Color c)
        {
            // конверт byte к ushort (сдвиг на 8 битов
            this.Red = (ushort)(c.R << 8);
            this.Green = (ushort)(c.G << 8);
            this.Blue = (ushort)(c.B << 8);
            this.Alpha = (ushort)(c.A << 8);
        }
        public void SetTriVertex(ref IFPoint p, ref Color c)
        {
            this.x = (int)p.X;
            this.y = (int)p.Y;
            SetColor(in c);
        }
        public void SetTriVertex(ref IHPoint p, ref Color c)
        {
            this.x = (int)p.X;
            this.y = (int)p.Y;
            SetColor(in c);
        }
    }
}
