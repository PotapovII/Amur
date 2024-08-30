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
    using System.Drawing;
    using System.Collections.Generic;
    /// <summary>
    /// ОО: Границы региона 
    /// </summary>
    [Serializable]
    public struct RectangleWorld
    {
        public float Left;
        public float Right;
        public float Bottom;
        public float Top;
        /// <summary>
        /// Ширина региона
        /// </summary>
        public float Width
        {
            get => this.Right - this.Left;
        }
        /// <summary>
        /// Высота региона
        /// </summary>
        public float Height
        {
            get => this.Top - this.Bottom;
        }
        /// <summary>
        /// Максимальный линейный размер региона
        /// </summary>
        public float MaxScale
        {
            get => Math.Max(Width, Height);
        }
        public RectangleWorld(IHPoint a, IHPoint b)
        {
            Left = Math.Min((float)a.X, (float)b.X);
            Right = Math.Max((float)a.X, (float)b.X);
            Bottom = Math.Min((float)a.Y, (float)b.Y);
            Top = Math.Max((float)a.Y, (float)b.Y);
        }
        public RectangleWorld(PointF a, PointF b)
        {
            Left = Math.Min(a.X, b.X);
            Right = Math.Max(a.X, b.X);
            Bottom = Math.Min(a.Y, b.Y);
            Top = Math.Max(a.Y, b.Y);
        }
        public RectangleWorld(float left = 0, float right = 0, float bottom = 0, float top = 0)
        {
            this.Left = left;
            this.Right = right;
            this.Bottom = bottom;
            this.Top = top;
        }
        public RectangleWorld(double left, double right, double bottom, double top)
        {
            this.Left = (float)left;
            this.Right = (float)right;
            this.Bottom = (float)bottom;
            this.Top = (float)top;
        }
        /// <summary>
        /// Расширение региона при смешивании
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static RectangleWorld Extension(ref RectangleWorld a, ref RectangleWorld b)
        {
            if (b.Left == b.Right && b.Bottom == b.Top)
                return a;
            else
            {
                if (a.Left == a.Right && a.Bottom == a.Top)
                    return b;
                else
                    return new RectangleWorld(
                        Math.Min(a.Left, b.Left),
                        Math.Max(a.Right, b.Right),
                        Math.Min(a.Bottom, b.Bottom),
                        Math.Max(a.Top, b.Top));
            }
                
        }

        /// <summary>
        /// Расширение области, для включения в даннуой точки.
        /// </summary>
        /// <param name="p"></param>
        public void Extension(PointF p)
        {
            Left = Math.Min(Left, p.X);
            Right = Math.Max(Right, p.X);
            Bottom = Math.Min(Bottom, p.Y);
            Top = Math.Max(Top, p.Y);
        }

        /// <summary>
        /// Расширение области, для включения в даннуой точки.
        /// </summary>
        /// <param name="p"></param>

        public void Extension(RectangleWorld p)
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
            Left = Math.Min(Left, (float)p.X);
            Bottom = Math.Min(Bottom, (float)p.Y);
            Right = Math.Max(Right, (float)p.X);
            Top = Math.Max(Top, (float)p.Y);
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
        public void Center(ref PointF center)
        {
            center.X = (Left + Right) / 2;
            center.Y = (Bottom + Top) / 2;
        }

        public static bool operator ==(RectangleWorld a, RectangleWorld b)
        {
            int merror = 4;
            bool r = MEM.Equals(a.Left, b.Left, merror) &&
                     MEM.Equals(a.Right, b.Right, merror) &&
                     MEM.Equals(a.Top, b.Top, merror) &&
                     MEM.Equals(a.Bottom, b.Bottom, merror);
            return r;
        }
        public static bool operator !=(RectangleWorld a, RectangleWorld b)
        {
            if (a == b)
                return false;
            else
                return true;
        }
    }
}
