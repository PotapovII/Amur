//                          ПРОЕКТ  "МКЭ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//           кодировка : 15.10.2022 Потапов И.И. (c++=> c#)
//---------------------------------------------------------------------------
namespace GeometryLib.Vector
{
    using System;
    using System.Text;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using MemLogLib;
    using CommonLib.Geometry;

    /// <summary>
    /// Представляет трехмерный вещественный вектор с полями двойной точности.
    /// </summary>
    [Serializable]
    public struct Vector3 : IEquatable<Vector3>, IFormattable, IDPoint
    {
        /// <summary>
        /// Координата X вектора.
        /// </summary>
        public double X { get; set; }
        /// <summary>
        /// Координата Y вектора.
        /// </summary>
        public double Y { get; set; }
        /// <summary>
        /// Координата Z вектора.
        /// </summary>
        public double Z { get; set; }
        /// <summary>
        /// Создает копию объекта
        /// </summary>
        /// <returns></returns>
        public IHPoint IClone() { return new Vector3(X, Y, Z); }
        public int CompareTo(object obj)
        {
            return ((Vector3)obj).X.CompareTo(X);
        }
        /// <summary>
        /// Значение, присваиваемое всем трем элементам.
        /// </summary>
        /// <param name="value"></param>
        public Vector3(double value) : this(value, value, value){}
        public Vector3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public Vector3(IHPoint p)
        {
            X = p.X;
            Y = p.Y;
            Z = 0;
        }
        public static Vector3 Zero => default(Vector3);
        /// <summary>
        /// Вектор, три элемента которого равны единице (то есть он возвращает вектор (1,1,1).
        /// </summary>
        public static Vector3 One => new Vector3(1f, 1f, 1f);
        /// <summary>
        /// Вектор (1,0,0).
        /// </summary>
        public static Vector3 UnitX => new Vector3(1f, 0f, 0f);
        /// <summary>
        /// Вектор (0,1,0).
        /// </summary>
        public static Vector3 UnitY => new Vector3(0f, 1f, 0f);
        /// <summary>
        ///  Вектор (0,0,1).
        /// </summary>
        public static Vector3 UnitZ => new Vector3(0, 0, 1);

        public override bool Equals(object obj) {
            if (base.Equals(obj)) {
                return this == (Vector3)obj;
            } else {
                return false;
            }
        }

        public override int GetHashCode() {
            return (X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode()).GetHashCode();
        }

        /// <summary>
        ///  Возвращает строковое представление текущего экземпляра, используя заданную строку
        ///  форматирования для форматирования отдельных элементов.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString("G", CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Возвращает строковое представление текущего экземпляра, используя заданную строку
        ///  форматирования для форматирования отдельных элементов.
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public string ToString(string format)
        {
            return ToString(format, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Возвращает строковое представление текущего экземпляра, используя заданную строку
        /// форматирования для форматирования отдельных элементов и заданный поставщик формата
        /// для указания форматирования, определяемого языком и региональными параметрами.
        /// </summary>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            StringBuilder stringBuilder = new StringBuilder();
            string numberGroupSeparator = NumberFormatInfo.GetInstance(formatProvider).NumberGroupSeparator;
            stringBuilder.Append('<');
            stringBuilder.Append(((IFormattable)X).ToString(format, formatProvider));
            stringBuilder.Append(numberGroupSeparator);
            stringBuilder.Append(' ');
            stringBuilder.Append(((IFormattable)Y).ToString(format, formatProvider));
            stringBuilder.Append(numberGroupSeparator);
            stringBuilder.Append(' ');
            stringBuilder.Append(((IFormattable)Z).ToString(format, formatProvider));
            stringBuilder.Append('>');
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Возвращает длину данного объекта вектора.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Length()
        {
            double num2 = X * X + Y * Y + Z * Z;
            return (double)Math.Sqrt(num2);
        }

        /// <summary>
        /// Возвращает длину вектора в квадрате.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double LengthSquared()
        {
            return X * X + Y * Y + Z * Z;
        }

        /// <summary>
        /// Вычисляет евклидово расстояние между двумя заданными точками.
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Distance(Vector3 value1, Vector3 value2)
        {
            double num2 = value1.X - value2.X;
            double num3 = value1.Y - value2.Y;
            double num4 = value1.Z - value2.Z;
            double num5 = num2 * num2 + num3 * num3 + num4 * num4;
            return (double)Math.Sqrt(num5);
        }

        /// <summary>
        /// Возвращает квадрат евклидова расстояния между двумя заданными точками.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double DistanceSquared(Vector3 value1, Vector3 value2)
        {
            double num = value1.X - value2.X;
            double num2 = value1.Y - value2.Y;
            double num3 = value1.Z - value2.Z;
            return num * num + num2 * num2 + num3 * num3;
        }

        /// <summary>
        /// Возвращает вектор с тем же направлением, 
        /// что и заданный вектор, но с длиной равной единице.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Normalize(Vector3 value)
        {
            double num2 = value.X * value.X + value.Y * value.Y + value.Z * value.Z;
            double num3 = (double)Math.Sqrt(num2);
            return new Vector3(value.X / num3, value.Y / num3, value.Z / num3);
        }

        /// <summary>
        /// Вычисляет векторное произведение двух векторов.
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Cross(Vector3 vector1, Vector3 vector2)
        {
            return new Vector3(vector1.Y * vector2.Z - vector1.Z * vector2.Y, vector1.Z * vector2.X - vector1.X * vector2.Z, vector1.X * vector2.Y - vector1.Y * vector2.X);
        }

        /// <summary>
        /// Возвращает отражение вектора от поверхности, которая имеет заданную нормаль.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="normal"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Reflect(Vector3 vector, Vector3 normal)
        {
            double num2 = vector.X * normal.X + vector.Y * normal.Y + vector.Z * normal.Z;
            double num3 = normal.X * num2 * 2f;
            double num4 = normal.Y * num2 * 2f;
            double num5 = normal.Z * num2 * 2f;
            return new Vector3(vector.X - num3, vector.Y - num4, vector.Z - num5);
        }

        /// <summary>
        /// Ограничивает минимальное и максимальное значение вектора.
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Clamp(Vector3 value1, Vector3 min, Vector3 max)
        {
            double x = value1.X;
            x = ((x > max.X) ? max.X : x);
            x = ((x < min.X) ? min.X : x);
            double y = value1.Y;
            y = ((y > max.Y) ? max.Y : y);
            y = ((y < min.Y) ? min.Y : y);
            double z = value1.Z;
            z = ((z > max.Z) ? max.Z : z);
            z = ((z < min.Z) ? min.Z : z);
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Выполняет линейную интерполяцию между двумя векторами на основе заданного взвешивания.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Lerp(Vector3 value1, Vector3 value2, double amount)
        {
            return new Vector3(value1.X + (value2.X - value1.X) * amount, value1.Y + (value2.Y - value1.Y) * amount, value1.Z + (value2.Z - value1.Z) * amount);
        }

        /// <summary>
        /// Складывает два вектора.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Add(Vector3 left, Vector3 right)
        {
            return left + right;
        }
        /// <summary>
        /// Вычитает второй вектор из первого.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Subtract(Vector3 left, Vector3 right)
        {
            return left - right;
        }
        /// <summary>
        ///  Возвращает новый вектор, значения которого являются произведением каждой пары
        ///  элементов в двух заданных векторах.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Multiply(Vector3 left, Vector3 right)
        {
            return left * right;
        }
        /// <summary>
        /// Умножает вектор на заданный скаляр.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Multiply(Vector3 left, double right)
        {
            return left * right;
        }

        /// <summary>
        /// Умножает скалярное значение на заданный вектор.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Multiply(double left, Vector3 right)
        {
            return left * right;
        }

        /// <summary>
        /// Делит первый вектор на второй.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Divide(Vector3 left, Vector3 right)
        {
            return left / right;
        }

        //
        /// <summary>
        /// Делит заданный вектор на указанное скалярное значение.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="divisor"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Divide(Vector3 left, double divisor)
        {
            return left / divisor;
        }
        /// <summary>
        /// Преобразует заданный вектор в отрицательный.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Negate(Vector3 value)
        {
            return -value;
        }
        /// <summary>
        /// Скалярное произведение.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Dot(Vector3 vector1, Vector3 vector2)
        {
            return vector1.X * vector2.X + vector1.Y * vector2.Y + vector1.Z * vector2.Z;
        }
        /// <summary>
        /// Копирует элементы вектора в заданный массив.
        /// </summary>
        /// <param name="array"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(double[] array)
        {
            X = array[0];
            Y = array[1];
            Z = array[2];
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns>
        /// Значение true, если объект obj равен текущему экземпляру; в противном случае
        /// — значение false. Если значением параметра obj является null, метод возвращает
        /// false.
        /// </returns>
        public bool Equals(Vector3 other)
        {
            if (MEM.Equals(X,other.X) && MEM.Equals(Y,other.Y) )
                return MEM.Equals(Z, other.Z);
            else
                return false;
        }

        /// <summary>
        /// Возвращает вектор, элементы которого являются минимальными значениями каждой
        /// пары элементов в двух заданных векторах.
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        public static Vector3 Min(Vector3 value1, Vector3 value2)
        {
            return new Vector3((value1.X < value2.X) ? value1.X : value2.X, (value1.Y < value2.Y) ? value1.Y : value2.Y, (value1.Z < value2.Z) ? value1.Z : value2.Z);
        }

        /// <summary>
        /// Возвращает вектор, элементы которого являются максимальными значениями каждой
        /// пары элементов в двух заданных векторах.
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Max(Vector3 value1, Vector3 value2)
        {
            return new Vector3((value1.X > value2.X) ? value1.X : value2.X, (value1.Y > value2.Y) ? value1.Y : value2.Y, (value1.Z > value2.Z) ? value1.Z : value2.Z);
        }
        /// <summary>
        ///  Возвращает вектор, элементы которого являются абсолютными значениями каждого 
        ///  из элементов заданного вектора.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Abs(Vector3 value)
        {
            return new Vector3(Math.Abs(value.X), Math.Abs(value.Y), Math.Abs(value.Z));
        }

        /// <summary>
        /// Возвращает  вектор, содержащий знак компонент вектора 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Sign(Vector3 value)
        {
            return new Vector3(Math.Sign(value.X), Math.Sign(value.Y), Math.Sign(value.Z));
        }


        /// <summary>
        /// Возвращает вектор, элементы которого являются квадратным корнем 
        /// каждого из элементов заданного вектора.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 SquareRoot(Vector3 value)
        {
            return new Vector3(Math.Sqrt(value.X), Math.Sqrt(value.Y), Math.Sqrt(value.Z));
        }



        /// <summary>
        /// Складывает два вектора.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        //     Суммарный вектор.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator +(Vector3 left, Vector3 right)
        {
            return new Vector3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }
        /// <summary>
        /// Вектор, полученный в результате вычитания right из left.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator -(Vector3 left, Vector3 right)
        {
            return new Vector3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }
        /// <summary>
        /// Поэлементный вектор произведения.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(Vector3 left, Vector3 right)
        {
            return new Vector3(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
        }
        /// <summary>
        /// Умножает заданный вектор на указанное скалярное значение.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(Vector3 left, double right)
        {
            return left * new Vector3(right);
        }
        /// <summary>
        /// Умножает скалярное значение на заданный вектор.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(double left, Vector3 right)
        {
            return new Vector3(left) * right;
        }
        /// <summary>
        /// Делит первый вектор на второй.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator /(Vector3 left, Vector3 right)
        {
            return new Vector3(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
        }
        /// <summary>
        /// Делит заданный вектор на указанное скалярное значение.
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator /(Vector3 value1, double value2)
        {
            double num = 1.0 / value2;
            return new Vector3(value1.X * num, value1.Y * num, value1.Z * num);
        }
        /// <summary>
        /// Преобразует заданный вектор в отрицательный.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator -(Vector3 value)
        {
            return Zero - value;
        }
        /// <summary>
        /// Возвращает значение, указывающее, равна ли каждая пара элементов в двух заданных векторах.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Vector3 left, Vector3 right)
        {
            return !MEM.Equals(left.X, right.X) && !MEM.Equals(left.Y, right.Y) && !MEM.Equals(left.Z, right.Z);
        }
        /// <summary>
        /// Возвращает значение, указывающее на неравенство двух заданных векторов.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Vector3 left, Vector3 right)
        {
            return !MEM.Equals(left.X, right.X) && !MEM.Equals(left.Y, right.Y) && !MEM.Equals(left.Z, right.Z);
        }

    }
}
