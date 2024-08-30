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
//           кодировка : 01.02.2024 Потапов И.И. (HPoint => FPoint)
//---------------------------------------------------------------------------
namespace CommonLib.Points
{
    using System;
    using MemLogLib;
    using System.Drawing;
    using System.Diagnostics;
    /// <summary>
    /// Определение класса FPoint Точка в (2) мерной системе координат
    /// </summary>
    [Serializable]
    [DebuggerDisplay("ID {ID} [{X}, {Y}]")]
    public class FPoint : IFPoint, IComparable, IEquatable<FPoint>
    {
        /// <summary>
        /// Координата по х
        /// </summary>
        public float x;
        /// <summary>
        /// Координата по y
        /// </summary>
        public float y;
        /// <summary>
        /// Координата по х
        /// </summary>
        public float X { get => x; set => x = value; }
        /// <summary>
        /// Координата по y
        /// </summary>
        public float Y { get => y; set => y = value; }

        public FPoint()
        {
            x = 0;
            y = 0;
        }
        public FPoint(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
        public FPoint(IFPoint a)
        {
            x = a.X;
            y = a.Y;
        }
        public FPoint(FPoint a)
        {
            x = a.x;
            y = a.y;
        }

        public virtual FPoint Clone()
        {
            return new FPoint(x, y);
        }
        /// <summary>
        /// Итератор
        /// </summary>
        /// <param name="i"></param>
        public float this[int i]
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
        int CompareTo(FPoint other)
        {
            return x.CompareTo(other.x);
        }
        public int CompareTo(object obj)
        {
            FPoint other = obj as FPoint;
            if (1e9f * x > y && 1e9f * other.x > other.y)
            {
                float a = 1e9f * x + y;
                float b = 1e9f * other.x + other.y;
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
        public static bool Equals(FPoint a, FPoint b)
        {
            if (Math.Abs(a.x - b.x) < MEM.Error7 && Math.Abs(a.y - b.y) < MEM.Error7)
                return true;
            else
                return false;
        }
        public bool Equals(FPoint p)
        {
            return Equals(this, p);
        }

        public virtual string ToString(string format = "F4")
        {
            return x.ToString(format) + " " + y.ToString(format) + " ";
        }

        public static FPoint Parse(string str)
        {
            string[] ls = (str.Trim()).Split(' ');
            FPoint knot = new FPoint(float.Parse(ls[0], MEM.formatter), float.Parse(ls[1], MEM.formatter));
            return knot;
        }

        public static implicit operator PointF(FPoint p)
        {
            return new PointF((float)p.x, (float)p.y);
        }

        public static implicit operator FPoint(PointF p)
        {
            return new FPoint((float)p.X, (float)p.Y);
        }

        public static float Length(float x1, float y1, float x2, float y2)
        {
            return (float)Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        }
        public static float Length(FPoint a, FPoint b)
        {
            return (float)Math.Sqrt((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y));
        }
        // расстояние мж точкой текущей точкой и точкой b
        private float LengthAB(FPoint b)
        {
            return (float)Math.Sqrt((x - b.x) * (x - b.x) + (y - b.y) * (y - b.y));
        }
        /// <summary>
        /// длина радиус вектора для точки а
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public float Scalar(FPoint a)
        {
            return (float)Math.Sqrt(x * a.x + y * a.y);
        }
        /// <summary>
        /// векторное произведение
        /// </summary>
        /// <param name="Vector1">Вектор 1</param>
        /// <param name="Vector2">Вектор 2</param>
        /// <returns></returns>
        public static float Cross(FPoint Vector1, FPoint Vector2)
        {
            return Vector1.x * Vector2.y - Vector1.y * Vector2.x;
        }
        /// <summary>
        /// норма или длинна вектора  |v|   ||v|| 
        /// </summary>
        /// <returns></returns>
        public float Length()
        {
            return (float)Math.Sqrt(x * x + y * y);
        }
        /// <summary>
        /// нахождение единичного вектора направление которого совпадает 
        /// с направлением текущего
        /// </summary>
        /// <returns></returns>
        public FPoint GetNormalize()
        {
            FPoint result = new FPoint();
            float length = Length();
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
        public FPoint GetNormalize(ref float length)
        {
            FPoint result = new FPoint();
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
            float length = Length();
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
        public FPoint GetOrt()
        {
            FPoint result = new FPoint();
            float length = Length();
            if (length >= MEM.Error8)
            {
                result.x = y / length;
                result.y = -x / length;
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
        public static FPoint operator *(float scal, FPoint b)
        {
            return new FPoint(scal * b.x, scal * b.y);
        }
        public static float operator *(FPoint a, FPoint b)
        {
            return (a.x * b.x + a.y * b.y);
        }
        public static FPoint operator *(FPoint b, float a)
        {
            return new FPoint(a * b.x, a * b.y);
        }
        public static FPoint operator +(FPoint a, FPoint b)
        {
            return new FPoint(a.x + b.x, a.y + b.y);
        }
        public static FPoint operator +(float a, FPoint b)
        {
            return new FPoint(a + b.x, a + b.y);
        }
        public static FPoint operator +(FPoint b, float a)
        {
            return new FPoint(a + b.x, a + b.y);
        }

        public static FPoint operator -(FPoint b)
        {
            return new FPoint(-b.x, -b.y);
        }
        public static FPoint operator -(FPoint a, FPoint b)
        {
            return new FPoint(a.x - b.x, a.y - b.y);
        }
        public static FPoint operator -(float a, FPoint b)
        {
            return new FPoint(a - b.x, a - b.y);
        }
        public static FPoint operator -(FPoint b, float a)
        {
            return new FPoint(b.x - a, b.y - a);
        }
        public static FPoint operator /(FPoint a, FPoint b)
        {
            return new FPoint(a.x / b.x, a.y / b.y);
        }
        public static FPoint operator /(float a, FPoint b)
        {
            return new FPoint(a / b.x, a / b.y);
        }
        public static FPoint operator /(FPoint b, float a)
        {
            return new FPoint(b.x / a, b.y / a);
        }
    }
}
