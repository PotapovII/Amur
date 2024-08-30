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
//                          ПРОЕКТ  "H?"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                кодировка : 2.10.2000 Потапов И.И. (c++)
//---------------------------------------------------------------------------
//                          ПРОЕКТ  "МКЭ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//           кодировка : 25.12.2020 Потапов И.И. (c++=> c#)
//---------------------------------------------------------------------------
namespace CommonLib.Points
{
    using System;
    using MemLogLib;
    using System.Drawing;
    using System.Diagnostics;
    /// <summary>
    /// Определение класса HPoint Точка в (2) мерной системе координат
    /// </summary>
    [Serializable]
    [DebuggerDisplay("ID {ID} [{X}, {Y}]")]
    public class HPoint : IHPoint, IComparable, IEquatable<HPoint>
    {
        /// <summary>
        /// Координата по х
        /// </summary>
        public double x;
        /// <summary>
        /// Координата по y
        /// </summary>
        public double y;
        /// <summary>
        /// Координата по х
        /// </summary>
        public double X { get => x; set => x = value; }
        /// <summary>
        /// Координата по y
        /// </summary>
        public double Y { get => y; set => y = value; }

        public HPoint()
        {
            x = 0;
            y = 0;
        }
        public HPoint(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
        public HPoint(IHPoint a)
        {
            x = a.X;
            y = a.Y;
        }
        public HPoint(HPoint a)
        {
            x = a.x;
            y = a.y;
        }

        public virtual HPoint Clone()
        {
            return new HPoint(x, y);
        }
        /// <summary>
        /// Итератор
        /// </summary>
        /// <param name="i"></param>
        public double this[int i]
        {
            get
            {
                if (i >= 2)
                    throw new Exception("bad vector index");
                if (i == 0)
                    return x;
                else if (i == 1)
                    return y;
                return 0;
            }
            set
            {
                if (i >= 2)
                    throw new Exception("bad vector index");
                if (i == 0)
                    x = value;
                else if (i == 1)
                    y = value;
            }
        }

        #region IComparable
        int CompareTo(HPoint other)
        {
            return x.CompareTo(other.x);
        }
        public int CompareTo(object obj)
        {
            HPoint other = obj as HPoint;
            if (1e10 * x > y && 1e10 * other.x > other.y)
            {
                double a = 1e10 * x + y;
                double b = 1e10 * other.x + other.y;
                return a.CompareTo(b);
            }
            else
                return x.CompareTo(other.x);
        }
        public override int GetHashCode()
        {
            int hash = 19;
            hash = hash * 31 + x.GetHashCode();
            hash = hash * 31 + y.GetHashCode();

            return hash;
        }
        #endregion

        /// <summary>
        /// Сравнение вещественных чисел
        /// </summary>
        public static bool Equals(HPoint a, HPoint b)
        {
            if (Math.Abs(a.x - b.x) < MEM.Error7 && Math.Abs(a.y - b.y) < MEM.Error7)
                return true;
            else
                return false;
        }
        public bool Equals(HPoint p)
        {
            return Equals(this, p);
        }

        public virtual string ToString(string format = "F4")
        {
            return x.ToString(format) + " " + y.ToString(format) + " ";
        }
        

        public static HPoint Parse(string str)
        {
            string[] ls = (str.Trim()).Split(' ');
            HPoint knot = new HPoint(double.Parse(ls[0], MEM.formatter), double.Parse(ls[1], MEM.formatter));
            return knot;
        }

        public static implicit operator PointF(HPoint p)
        {
            return new PointF((float)p.x, (float)p.y);
        }

        public static implicit operator HPoint(PointF p)
        {
            return new HPoint((float)p.X, (float)p.Y);
        }

        public static double Length(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        }
        public static double Length2(double x1, double y1, double x2, double y2)
        {
            return (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);
        }

        public static double Length(HPoint a, HPoint b)
        {
            return Math.Sqrt((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y));
        }
        public static double Length2(HPoint a, HPoint b)
        {
            return (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y);
        }
        // расстояние мж точкой текущей точкой и точкой b
        private double LengthAB(HPoint b)
        {
            return Math.Sqrt((x - b.x) * (x - b.x) + (y - b.y) * (y - b.y));
        }
        /// <summary>
        /// длина радиус вектора для точки а
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public double Scalar(HPoint a)
        {
            return Math.Sqrt(x * a.x + y * a.y);
        }
        /// <summary>
        /// векторное произведение
        /// </summary>
        /// <param name="Vector1">Вектор 1</param>
        /// <param name="Vector2">Вектор 2</param>
        /// <returns></returns>
        public static double Cross(HPoint Vector1, HPoint Vector2)
        {
            return Vector1.x * Vector2.y - Vector1.y * Vector2.x;
        }
        /// <summary>
        /// норма или длинна вектора  |v|   ||v|| 
        /// </summary>
        /// <returns></returns>
        public double Length()
        {
            return Math.Sqrt(x * x + y * y);
        }
        /// <summary>
        /// нахождение единичного вектора направление которого совпадает 
        /// с направлением текущего
        /// </summary>
        /// <returns></returns>
        public HPoint GetNormalize()
        {
            HPoint result = new HPoint();
            double length = Length();
            if (length >= MEM.Error8)
            {
                result.x = x / length;
                result.y = y / length;
            }
            else
            {
                result.x = 0;
                result.y = 0;
                x = 0;
                y = 0;
            }
            return result;
        }
        /// <summary>
        /// нахождение единичного вектора направление которого совпадает 
        /// с направлением текущего
        /// </summary>
        /// <returns></returns>
        public HPoint GetNormalize(ref double length)
        {
            HPoint result = new HPoint();
            length = Length();
            if (length >= MEM.Error8)
            {
                result.x = x / length;
                result.y = y / length;
            }
            else
            {
                result.x = 0;
                result.y = 0;
                x = 0;
                y = 0;
            }
            return result;
        }
        /// <summary>
        /// нахождение единичного вектора направление которого совпадает 
        /// с направлением текущего
        /// </summary>
        public void Normalize()
        {
            double length = Length();
            if (length >= MEM.Error8)
            {
                x = x / length;
                y = y / length;
            }
            else
            {
                x = 0;
                y = 0;
            }
        }
        /// <summary>
        /// Получить орт к вектору
        /// </summary>
        /// <returns></returns>
        public HPoint GetOrt()
        {
            HPoint result = new HPoint();
            double length = Length();
            if (length >= MEM.Error8)
            {
                result.x =   y / length;
                result.y = - x / length;
            }
            else
            {
                result.x = 0;
                result.y = 0;
                x = 0;
                y = 0;
            }
            return result;
        }
        /// <summary>
        /// Получить внешний (правый) ортогональный вектор 
        /// </summary>
        /// <returns></returns>
        public HPoint GetOrtogonalRight()
        {
            return new HPoint(y, -x);
        }
        /// <summary>
        /// Получить внутренний (левый) ортогональный вектор 
        /// </summary>
        /// <returns></returns>
        public HPoint GetOrtogonalLeft()
        {
            return new HPoint(-y, x);
        }

        public static HPoint operator *(double scal, HPoint b)
        {
            return new HPoint(scal * b.x, scal * b.y);
        }
        public static double operator *(HPoint a, HPoint b)
        {
            return (a.x * b.x + a.y * b.y);
        }
        public static HPoint operator *(HPoint b, double a)
        {
            return new HPoint(a * b.x, a * b.y);
        }
        public static HPoint operator +(HPoint a, HPoint b)
        {
            return new HPoint(a.x + b.x, a.y + b.y);
        }
        public static HPoint operator +(double a, HPoint b)
        {
            return new HPoint(a + b.x, a + b.y);
        }
        public static HPoint operator +(HPoint b, double a)
        {
            return new HPoint(a + b.x, a + b.y);
        }

        public static HPoint operator -(HPoint b)
        {
            return new HPoint(-b.x, -b.y);
        }
        public static HPoint operator -(HPoint a, HPoint b)
        {
            return new HPoint(a.x - b.x, a.y - b.y);
        }
        public static HPoint operator -(double a, HPoint b)
        {
            return new HPoint(a - b.x, a - b.y);
        }
        public static HPoint operator -(HPoint b, double a)
        {
            return new HPoint(b.x - a, b.y - a);
        }
        public static HPoint operator /(HPoint a, HPoint b)
        {
            return new HPoint(a.x / b.x, a.y / b.y);
        }
        public static HPoint operator /(double a, HPoint b)
        {
            return new HPoint(a / b.x, a / b.y);
        }
        public static HPoint operator /(HPoint b, double a)
        {
            return new HPoint(b.x / a, b.y / a);
        }
    }
}
