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
//                 кодировка : 11.12.2020 Потапов И.И.
//---------------------------------------------------------------------------
namespace CommonLib.Areas
{
    using MemLogLib;
    using CommonLib.Geometry;

    using System;
    using System.Collections.Generic;
    /// <summary>
    /// ОО: Границы региона 
    /// </summary>
    [Serializable]
    public struct HQuad
    {
        public double Left;
        public double Right;
        public double Bottom;
        public double Top;
        /// <summary>
        /// Ширина региона
        /// </summary>
        public double Width
        {
            get => this.Right - this.Left;
        }
        /// <summary>
        /// Высота региона
        /// </summary>
        public double Height
        {
            get => this.Top - this.Bottom;
        }
        /// <summary>
        /// Максимальный линейный размер региона
        /// </summary>
        public double MaxScale
        {
            get => Math.Max(Width, Height);
        }
        public HQuad(IHPoint a, IHPoint b)
        {
            Left = Math.Min((double)a.X, (double)b.X);
            Right = Math.Max((double)a.X, (double)b.X);
            Bottom = Math.Min((double)a.Y, (double)b.Y);
            Top = Math.Max((double)a.Y, (double)b.Y);
        }

        public HQuad(double left = 0, double right = 0, double bottom = 0, double top = 0)
        {
            this.Left = left;
            this.Right = right;
            this.Bottom = bottom;
            this.Top = top;
        }
        /// <summary>
        /// Расширение региона при смешивании
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static HQuad Extension(ref HQuad a, ref HQuad b)
        {
            if (b.Left == b.Right && b.Bottom == b.Top)
                return a;
            else
            {
                if (a.Left == a.Right && a.Bottom == a.Top)
                    return b;
                else
                    return new HQuad(
                        Math.Min(a.Left, b.Left),
                        Math.Max(a.Right, b.Right),
                        Math.Min(a.Bottom, b.Bottom),
                        Math.Max(a.Top, b.Top));
            }
                
        }
        /// <summary>
        /// Расширение области, для включения в область данной точки.
        /// </summary>
        /// <param name="p"></param>

        public void Extension(HQuad p)
        {
            Left = Math.Min(Left, p.Left);
            Right = Math.Max(Right, p.Right);
            Bottom = Math.Min(Bottom, p.Bottom);
            Top = Math.Max(Top, p.Top);
        }

        /// <summary>
        /// Расширение области, для включения в даннуой точки.
        /// </summary>
        /// <param name="p"></param>
        public void Extension(IHPoint p)
        {
            Left = Math.Min(Left, (double)p.X);
            Bottom = Math.Min(Bottom, (double)p.Y);
            Right = Math.Max(Right, (double)p.X);
            Top = Math.Max(Top, (double)p.Y);
        }
        /// <summary>
        /// Проверьте, находится ли заданная точка внутри прямоугольника.
        /// </summary>
        public bool Contains(double x, double y)
        {
            return ((x >= Left) && (x <= Right) && (y >= Bottom) && (y <= Top));
        }

        /// <summary>
        /// Проверьте, находится ли заданная точка внутри прямоугольника.
        /// </summary>
        public bool Contains(IHPoint pt)
        {
            return Contains(pt.X, pt.Y);
        }

        /// <summary>
        /// Проверьте, содержит ли этот прямоугольник другой прямоугольник.
        /// </summary>
        public bool Contains(HQuad other)
        {
            return (Left <= other.Left && other.Right <= Right
                && Bottom <= other.Bottom && other.Top <= Top);
        }


        /// <summary>
        /// Расширение области, для включения в облака точек.
        /// </summary>
        public void Extension(IEnumerable<IHPoint> points)
        {
            foreach (var p in points)
                Extension(p);
        }
        /// <summary>
        /// Центр области
        /// </summary>
        /// <param name="center"></param>
        public void Center(ref IHPoint center)
        {
            center.X = (Left + Right) / 2;
            center.Y = (Bottom + Top) / 2;
        }

        public static bool operator ==(HQuad a, HQuad b)
        {
            int merror = 4;
            bool r = MEM.Equals(a.Left, b.Left, merror) &&
                     MEM.Equals(a.Right, b.Right, merror) &&
                     MEM.Equals(a.Top, b.Top, merror) &&
                     MEM.Equals(a.Bottom, b.Bottom, merror);
            return r;
        }
        public static bool operator !=(HQuad a, HQuad b)
        {
            if (a == b)
                return false;
            else
                return true;
        }
    }
}
