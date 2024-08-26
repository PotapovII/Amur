//---------------------------------------------------------------------------
//                          ПРОЕКТ  "МКЭ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//           кодировка : 27.11.2022 Потапов И.И. (c# => c#)
//---------------------------------------------------------------------------
namespace GeometryLib.Vector
{
    using System;
    using System.Text;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    /// <summary>
    /// Представляет четырехмерный вещественный вектор с полями двойной точности.
    /// </summary>
    [Serializable]
    public struct Vector4 : IEquatable<Vector4>, IFormattable
    {
        /// <summary>
        /// Координата X вектора.
        /// </summary>
        public double X;
        /// <summary>
        /// Координата Y вектора.
        /// </summary>
        public double Y;
        /// <summary>
        /// Координата Z вектора.
        /// </summary>
        public double Z;
        /// <summary>
        /// Координата W вектора.
        /// </summary>
        public double W;
        /// <summary>
        /// Получает вектор, четыре элемента которого равны нулю.
        /// Вектор, четыре элемента которого равны нулю 
        /// (то есть он возвращает вектор (значение 0,0,0,0).
        /// </summary>
        public static Vector4 Zero
        {
            get
            {
                return default(Vector4);
            }
        }
        /// <summary>
        /// Возвращается вектор, четыре элемента которого равны единице.
        /// </summary>
        public static Vector4 One
        {
            get
            {
                return new Vector4(1f, 1f, 1f, 1f);
            }
        }
        /// <summary>
        /// Возвращается вектор (1,0,0,0).
        /// </summary>
        public static Vector4 UnitX
        {
            get
            {
                return new Vector4(1f, 0f, 0f, 0f);
            }
        }
        /// <summary>
        /// Возвращается вектор (0,1,0,0).
        /// </summary>
        public static Vector4 UnitY
        {
            get
            {
                return new Vector4(0f, 1f, 0f, 0f);
            }
        }
        /// <summary>
        /// Возвращается вектор (0,0,1,0).
        /// </summary>
        public static Vector4 UnitZ
        {
            get
            {
                return new Vector4(0f, 0f, 1f, 0f);
            }
        }
        /// <summary>
        /// Возвращается вектор (0,0,0,1).
        /// </summary>
        public static Vector4 UnitW
        {
            get
            {
                return new Vector4(0f, 0f, 0f, 1f);
            }
        }
        /// <summary>
        /// Возвращает хэш-код данного экземпляра.
        /// </summary>
        public override int GetHashCode()
        {
            return (X.GetHashCode() ^ 
                    Y.GetHashCode() ^ 
                    Z.GetHashCode() ^ 
                    W.GetHashCode()).GetHashCode();
        }
        /// <summary>
        /// Возвращает значение, указывающее, равен ли данный экземпляр указанному объекту.
        /// </summary>
        /// <param name="obj">        
        /// Объект для сравнения с текущим экземпляром
        /// </param>
        /// <returns>
        /// Возврат: Значение true, если объект obj равен текущему экземпляру; в противном случае
        ///  — значение false. Если значением параметра obj является null, метод возвращает false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            if (!(obj is Vector4))
            {
                return false;
            }
            return Equals((Vector4)obj);
        }
        /// <summary>
        /// Возвращает строковое представление текущего экземпляра, 
        /// используя форматирование по умолчанию.
        /// </summary>
        /// <returns>Строковое представление текущего экземпляра.</returns>
        public override string ToString()
        {
            return ToString("G", CultureInfo.CurrentCulture);
        }
        /// <summary>
        /// Возвращает строковое представление текущего экземпляра, 
        /// используя заданную строку форматирования для 
        /// форматирования отдельных элементов.
        /// </summary>
        /// <param name="format">
        /// Строка стандартного или настраиваемого числового формата, 
        /// определяющая формат отдельных элементов
        /// </param>
        /// <returns>
        /// Строковое представление текущего экземпляра.
        /// </returns>
        public string ToString(string format)
        {
            return ToString(format, CultureInfo.CurrentCulture);
        }
        //
        // Сводка:
        //     Возвращает строковое представление текущего экземпляра, используя заданную строку
        //     форматирования для форматирования отдельных элементов и заданный поставщик формата
        //     для указания форматирования, определяемого языком и региональными параметрами.
        //
        // Параметры:
        //   format:
        //     Строка стандартного или настраиваемого числового формата, определяющая формат
        //     отдельных элементов.
        //
        //   formatProvider:
        //     Поставщик формата, предоставляющий сведения о форматировании для определенного
        //     языка и региональных параметров.
        //
        // Возврат:
        //     Строковое представление текущего экземпляра.
        
        public string ToString(string format, IFormatProvider formatProvider)
        {
            StringBuilder stringBuilder = new StringBuilder();
            string numberGroupSeparator = NumberFormatInfo.GetInstance(formatProvider).NumberGroupSeparator;
            stringBuilder.Append('<');
            stringBuilder.Append(X.ToString(format, formatProvider));
            stringBuilder.Append(numberGroupSeparator);
            stringBuilder.Append(' ');
            stringBuilder.Append(Y.ToString(format, formatProvider));
            stringBuilder.Append(numberGroupSeparator);
            stringBuilder.Append(' ');
            stringBuilder.Append(Z.ToString(format, formatProvider));
            stringBuilder.Append(numberGroupSeparator);
            stringBuilder.Append(' ');
            stringBuilder.Append(W.ToString(format, formatProvider));
            stringBuilder.Append('>');
            return stringBuilder.ToString();
        }

        //
        // Сводка:
        //     Возвращает длину данного объекта вектора.
        //
        // Возврат:
        //     Длина вектора.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Length()
        {
            //if (Vector.IsHardwareAccelerated)
            //{
            //    double num = Dot(this, this);
            //    return (double)Math.Sqrt(num);
            //}
            double num2 = X * X + Y * Y + Z * Z + W * W;
            return (double)Math.Sqrt(num2);
        }

        //
        // Сводка:
        //     Возвращает длину вектора в квадрате.
        //
        // Возврат:
        //     Длина вектора в квадрате.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double LengthSquared()
        {
            //if (Vector.IsHardwareAccelerated)
            //{
            //    return Dot(this, this);
            //}
            return X * X + Y * Y + Z * Z + W * W;
        }

        //
        // Сводка:
        //     Вычисляет евклидово расстояние между двумя заданными точками.
        //
        // Параметры:
        //   value1:
        //     Первая точка.
        //
        //   value2:
        //     Вторая точка.
        //
        // Возврат:
        //     Расстояние.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Distance(Vector4 value1, Vector4 value2)
        {
            //if (Vector.IsHardwareAccelerated)
            //{
            //    Vector4 vector = value1 - value2;
            //    double num = Dot(vector, vector);
            //    return (double)Math.Sqrt(num);
            //}
            double num2 = value1.X - value2.X;
            double num3 = value1.Y - value2.Y;
            double num4 = value1.Z - value2.Z;
            double num5 = value1.W - value2.W;
            double num6 = num2 * num2 + num3 * num3 + num4 * num4 + num5 * num5;
            return (double)Math.Sqrt(num6);
        }

        //
        // Сводка:
        //     Возвращает квадрат евклидова расстояния между двумя заданными точками.
        //
        // Параметры:
        //   value1:
        //     Первая точка.
        //
        //   value2:
        //     Вторая точка.
        //
        // Возврат:
        //     Квадрат расстояния.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double DistanceSquared(Vector4 value1, Vector4 value2)
        {
            //if (Vector.IsHardwareAccelerated)
            //{
            //    Vector4 vector = value1 - value2;
            //    return Dot(vector, vector);
            //}
            double num = value1.X - value2.X;
            double num2 = value1.Y - value2.Y;
            double num3 = value1.Z - value2.Z;
            double num4 = value1.W - value2.W;
            return num * num + num2 * num2 + num3 * num3 + num4 * num4;
        }

        //
        // Сводка:
        //     Возвращает вектор с тем же направлением, что и заданный вектор, но с длиной равной
        //     единице.
        //
        // Параметры:
        //   vector:
        //     Нормализуемый вектор.
        //
        // Возврат:
        //     Нормализованный вектор.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Normalize(Vector4 vector)
        {
            //if (Vector.IsHardwareAccelerated)
            //{
            //    double num = vector.Length();
            //    return vector / num;
            //}

            double num2 = vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z + vector.W * vector.W;
            double num3 = 1f / (double)Math.Sqrt(num2);
            return new Vector4(vector.X * num3, vector.Y * num3, vector.Z * num3, vector.W * num3);
        }

        //
        // Сводка:
        //     Ограничивает минимальное и максимальное значение вектора.
        //
        // Параметры:
        //   value1:
        //     Ограничиваемый вектор.
        //
        //   min:
        //     Минимальное значение.
        //
        //   max:
        //     Максимальное значение.
        //
        // Возврат:
        //     Ограниченный вектор.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Clamp(Vector4 value1, Vector4 min, Vector4 max)
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
            double w = value1.W;
            w = ((w > max.W) ? max.W : w);
            w = ((w < min.W) ? min.W : w);
            return new Vector4(x, y, z, w);
        }

        //
        // Сводка:
        //     Выполняет линейную интерполяцию между двумя векторами на основе заданного взвешивания.
        //
        // Параметры:
        //   value1:
        //     Первый вектор.
        //
        //   value2:
        //     Второй вектор.
        //
        //   amount:
        //     Значение от 0 до 1, указывающее вес value2.
        //
        // Возврат:
        //     Интерполированный вектор.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Lerp(Vector4 value1, Vector4 value2, double amount)
        {
            return new Vector4(value1.X + (value2.X - value1.X) * amount, value1.Y + (value2.Y - value1.Y) * amount, value1.Z + (value2.Z - value1.Z) * amount, value1.W + (value2.W - value1.W) * amount);
        }

        ////
        //// Сводка:
        ////     Преобразует двухмерный вектор посредством заданной матрицы 4x4.
        ////
        //// Параметры:
        ////   position:
        ////     Преобразуемый вектор.
        ////
        ////   matrix:
        ////     Матрица преобразования.
        ////
        //// Возврат:
        ////     Преобразованный вектор.
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static Vector4 Transform(Vector2 position, Matrix4x4 matrix)
        //{
        //    return new Vector4(position.X * matrix.M11 + position.Y * matrix.M21 + matrix.M41, position.X * matrix.M12 + position.Y * matrix.M22 + matrix.M42, position.X * matrix.M13 + position.Y * matrix.M23 + matrix.M43, position.X * matrix.M14 + position.Y * matrix.M24 + matrix.M44);
        //}

        ////
        //// Сводка:
        ////     Преобразует трехмерный вектор посредством заданной матрицы 4x4.
        ////
        //// Параметры:
        ////   position:
        ////     Преобразуемый вектор.
        ////
        ////   matrix:
        ////     Матрица преобразования.
        ////
        //// Возврат:
        ////     Преобразованный вектор.
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static Vector4 Transform(Vector3 position, Matrix4x4 matrix)
        //{
        //    return new Vector4(position.X * matrix.M11 + position.Y * matrix.M21 + position.Z * matrix.M31 + matrix.M41, position.X * matrix.M12 + position.Y * matrix.M22 + position.Z * matrix.M32 + matrix.M42, position.X * matrix.M13 + position.Y * matrix.M23 + position.Z * matrix.M33 + matrix.M43, position.X * matrix.M14 + position.Y * matrix.M24 + position.Z * matrix.M34 + matrix.M44);
        //}

        ////
        //// Сводка:
        ////     Преобразует четырехмерный вектор посредством заданной матрицы 4x4.
        ////
        //// Параметры:
        ////   vector:
        ////     Преобразуемый вектор.
        ////
        ////   matrix:
        ////     Матрица преобразования.
        ////
        //// Возврат:
        ////     Преобразованный вектор.
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static Vector4 Transform(Vector4 vector, Matrix4x4 matrix)
        //{
        //    return new Vector4(vector.X * matrix.M11 + vector.Y * matrix.M21 + vector.Z * matrix.M31 + vector.W * matrix.M41, vector.X * matrix.M12 + vector.Y * matrix.M22 + vector.Z * matrix.M32 + vector.W * matrix.M42, vector.X * matrix.M13 + vector.Y * matrix.M23 + vector.Z * matrix.M33 + vector.W * matrix.M43, vector.X * matrix.M14 + vector.Y * matrix.M24 + vector.Z * matrix.M34 + vector.W * matrix.M44);
        //}

        ////
        //// Сводка:
        ////     Преобразует двухмерный вектор посредством заданного значения поворота кватерниона.
        ////
        //// Параметры:
        ////   value:
        ////     Поворачиваемый вектор.
        ////
        ////   rotation:
        ////     Применяемый поворот.
        ////
        //// Возврат:
        ////     Преобразованный вектор.
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static Vector4 Transform(Vector2 value, Quaternion rotation)
        //{
        //    double num = rotation.X + rotation.X;
        //    double num2 = rotation.Y + rotation.Y;
        //    double num3 = rotation.Z + rotation.Z;
        //    double num4 = rotation.W * num;
        //    double num5 = rotation.W * num2;
        //    double num6 = rotation.W * num3;
        //    double num7 = rotation.X * num;
        //    double num8 = rotation.X * num2;
        //    double num9 = rotation.X * num3;
        //    double num10 = rotation.Y * num2;
        //    double num11 = rotation.Y * num3;
        //    double num12 = rotation.Z * num3;
        //    return new Vector4(value.X * (1f - num10 - num12) + value.Y * (num8 - num6), value.X * (num8 + num6) + value.Y * (1f - num7 - num12), value.X * (num9 - num5) + value.Y * (num11 + num4), 1f);
        //}

        ////
        //// Сводка:
        ////     Преобразует трехмерный вектор посредством заданного значения поворота кватерниона.
        ////
        //// Параметры:
        ////   value:
        ////     Поворачиваемый вектор.
        ////
        ////   rotation:
        ////     Применяемый поворот.
        ////
        //// Возврат:
        ////     Преобразованный вектор.
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static Vector4 Transform(Vector3 value, Quaternion rotation)
        //{
        //    double num = rotation.X + rotation.X;
        //    double num2 = rotation.Y + rotation.Y;
        //    double num3 = rotation.Z + rotation.Z;
        //    double num4 = rotation.W * num;
        //    double num5 = rotation.W * num2;
        //    double num6 = rotation.W * num3;
        //    double num7 = rotation.X * num;
        //    double num8 = rotation.X * num2;
        //    double num9 = rotation.X * num3;
        //    double num10 = rotation.Y * num2;
        //    double num11 = rotation.Y * num3;
        //    double num12 = rotation.Z * num3;
        //    return new Vector4(value.X * (1f - num10 - num12) + value.Y * (num8 - num6) + value.Z * (num9 + num5), value.X * (num8 + num6) + value.Y * (1f - num7 - num12) + value.Z * (num11 - num4), value.X * (num9 - num5) + value.Y * (num11 + num4) + value.Z * (1f - num7 - num10), 1f);
        //}

        ////
        //// Сводка:
        ////     Преобразует четырехмерный вектор посредством заданного значения поворота кватерниона.
        ////
        //// Параметры:
        ////   value:
        ////     Поворачиваемый вектор.
        ////
        ////   rotation:
        ////     Применяемый поворот.
        ////
        //// Возврат:
        ////     Преобразованный вектор.
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static Vector4 Transform(Vector4 value, Quaternion rotation)
        //{
        //    double num = rotation.X + rotation.X;
        //    double num2 = rotation.Y + rotation.Y;
        //    double num3 = rotation.Z + rotation.Z;
        //    double num4 = rotation.W * num;
        //    double num5 = rotation.W * num2;
        //    double num6 = rotation.W * num3;
        //    double num7 = rotation.X * num;
        //    double num8 = rotation.X * num2;
        //    double num9 = rotation.X * num3;
        //    double num10 = rotation.Y * num2;
        //    double num11 = rotation.Y * num3;
        //    double num12 = rotation.Z * num3;
        //    return new Vector4(value.X * (1f - num10 - num12) + value.Y * (num8 - num6) + value.Z * (num9 + num5), value.X * (num8 + num6) + value.Y * (1f - num7 - num12) + value.Z * (num11 - num4), value.X * (num9 - num5) + value.Y * (num11 + num4) + value.Z * (1f - num7 - num10), value.W);
        //}

        //
        // Сводка:
        //     Складывает два вектора.
        //
        // Параметры:
        //   left:
        //     Первый складываемый вектор.
        //
        //   right:
        //     Второй складываемый вектор.
        //
        // Возврат:
        //     Суммарный вектор.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Add(Vector4 left, Vector4 right)
        {
            return left + right;
        }

        //
        // Сводка:
        //     Вычитает второй вектор из первого.
        //
        // Параметры:
        //   left:
        //     Первый вектор.
        //
        //   right:
        //     Второй вектор.
        //
        // Возврат:
        //     Вектор отличия.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Subtract(Vector4 left, Vector4 right)
        {
            return left - right;
        }

        //
        // Сводка:
        //     Возвращает новый вектор, значения которого являются произведением каждой пары
        //     элементов в двух заданных векторах.
        //
        // Параметры:
        //   left:
        //     Первый вектор.
        //
        //   right:
        //     Второй вектор.
        //
        // Возврат:
        //     Поэлементный вектор произведения.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Multiply(Vector4 left, Vector4 right)
        {
            return left * right;
        }

        //
        // Сводка:
        //     Умножает вектор на заданный скаляр.
        //
        // Параметры:
        //   left:
        //     Умножаемый вектор.
        //
        //   right:
        //     Скалярное значение.
        //
        // Возврат:
        //     Масштабированный вектор.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Multiply(Vector4 left, double right)
        {
            return left * new Vector4(right, right, right, right);
        }

        //
        // Сводка:
        //     Умножает скалярное значение на заданный вектор.
        //
        // Параметры:
        //   left:
        //     Масштабированное значение.
        //
        //   right:
        //     Вектор.
        //
        // Возврат:
        //     Масштабированный вектор.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Multiply(double left, Vector4 right)
        {
            return new Vector4(left, left, left, left) * right;
        }

        //
        // Сводка:
        //     Делит первый вектор на второй.
        //
        // Параметры:
        //   left:
        //     Первый вектор.
        //
        //   right:
        //     Второй вектор.
        //
        // Возврат:
        //     Вектор, полученный в результате деления.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Divide(Vector4 left, Vector4 right)
        {
            return left / right;
        }

        //
        // Сводка:
        //     Делит заданный вектор на указанное скалярное значение.
        //
        // Параметры:
        //   left:
        //     Вектор.
        //
        //   divisor:
        //     Скалярное значение.
        //
        // Возврат:
        //     Вектор, полученный в результате деления.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Divide(Vector4 left, double divisor)
        {
            return left / divisor;
        }

        //
        // Сводка:
        //     Преобразует заданный вектор в отрицательный.
        //
        // Параметры:
        //   value:
        //     Вектор, преобразуемый в отрицательный.
        //
        // Возврат:
        //     Вектор, преобразованный в отрицательный.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Negate(Vector4 value)
        {
            return -value;
        }

        //
        // Сводка:
        //     Создает новый объект System.Numerics.Vector4 с четырьмя элементами, имеющими
        //     одинаковое значение.
        //
        // Параметры:
        //   value:
        //     Значение, присваиваемое всем четырем элементам.
        public Vector4(double value)
            : this(value, value, value, value)
        {
        }

        //
        // Сводка:
        //     Создает вектор, элементы которого имеют заданные значения.
        //
        // Параметры:
        //   x:
        //     Значение, присваиваемое полю System.Numerics.Vector4.X.
        //
        //   y:
        //     Значение, присваиваемое полю System.Numerics.Vector4.Y.
        //
        //   z:
        //     Значение, присваиваемое полю System.Numerics.Vector4.Z.
        //
        //   w:
        //     Значение, присваиваемое полю System.Numerics.Vector4.W.
        public Vector4(double x, double y, double z, double w)
        {
            W = w;
            X = x;
            Y = y;
            Z = z;
        }

        //
        // Сводка:
        //     Создает новый объект System.Numerics.Vector4 на основе заданного объекта System.Numerics.Vector2
        //     и координат Z и W.
        //
        // Параметры:
        //   value:
        //     Вектор, который необходимо использовать для координат X и Y.
        //
        //   z:
        //     Координата Z.
        //
        //   w:
        //     Координата W.
        public Vector4(Vector2 value, double z, double w)
        {
            X = value.X;
            Y = value.Y;
            Z = z;
            W = w;
        }

        //
        // Сводка:
        //     Строит новый объект System.Numerics.Vector4 на основе заданного объекта System.Numerics.Vector3
        //     и координаты W.
        //
        // Параметры:
        //   value:
        //     Вектор, который необходимо использовать для координат X, Y и Z.
        //
        //   w:
        //     Координата W.
        public Vector4(Vector3 value, double w)
        {
            X = value.X;
            Y = value.Y;
            Z = value.Z;
            W = w;
        }

        //
        // Сводка:
        //     Копирует элементы вектора в заданный массив.
        //
        // Параметры:
        //   array:
        //     Массив назначения.
        //
        // Исключения:
        //   T:System.ArgumentNullException:
        //     Свойство array имеет значение null.
        //
        //   T:System.ArgumentException:
        //     Количество элементов в текущем экземпляре больше, чем в массиве.
        //
        //   T:System.RankException:
        //     Массив array является многомерным.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(double[] array)
        {
            CopyTo(array, 0);
        }

        //
        // Сводка:
        //     Копирует элементы вектора в заданный массив, начиная с указанной позиции индекса.
        //
        // Параметры:
        //   array:
        //     Массив назначения.
        //
        //   index:
        //     Индекс, с которого начинается копирование первого элемента вектора.
        //
        // Исключения:
        //   T:System.ArgumentNullException:
        //     Свойство array имеет значение null.
        //
        //   T:System.ArgumentException:
        //     Количество элементов в текущем экземпляре больше, чем в массиве.
        //
        //   T:System.ArgumentOutOfRangeException:
        //     Значение параметра index меньше нуля. -или- Длина параметра index больше или
        //     равна длине массива.
        //
        //   T:System.RankException:
        //     Массив array является многомерным.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(double[] array, int index)
        {
            if (array == null)
            {
                throw new NullReferenceException("Arg_NullArgumentNullRef");
            }

            if (index < 0 || index >= array.Length)
            {
                throw new ArgumentOutOfRangeException("Arg_ArgumentOutOfRangeException " + index.ToString());
            }

            if (array.Length - index < 4)
            {
                throw new ArgumentException("Arg_ElementsInSourceIsGreaterThanDestination "+ index.ToString());
            }
            array[index] = X;
            array[index + 1] = Y;
            array[index + 2] = Z;
            array[index + 3] = W;
        }

        //
        // Сводка:
        //     Возвращает значение, указывающее, равен ли данный экземпляр другому вектору.
        //
        // Параметры:
        //   other:
        //     Другой вектор.
        //
        // Возврат:
        //     Значение true, если два вектора равны; в противном случае — значение false.
        public bool Equals(Vector4 other)
        {
            if (X == other.X && Y == other.Y && Z == other.Z)
            {
                return W == other.W;
            }

            return false;
        }

        //
        // Сводка:
        //     Возвращает скалярное произведение двух векторов.
        //
        // Параметры:
        //   vector1:
        //     Первый вектор.
        //
        //   vector2:
        //     Второй вектор.
        //
        // Возврат:
        //     Скалярное произведение.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Dot(Vector4 vector1, Vector4 vector2)
        {
            return vector1.X * vector2.X + vector1.Y * vector2.Y + vector1.Z * vector2.Z + vector1.W * vector2.W;
        }

        //
        // Сводка:
        //     Возвращает вектор, элементы которого являются минимальными значениями каждой
        //     пары элементов в двух заданных векторах.
        //
        // Параметры:
        //   value1:
        //     Первый вектор.
        //
        //   value2:
        //     Второй вектор.
        //
        // Возврат:
        //     Вектор, приведенный к минимуму.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Min(Vector4 value1, Vector4 value2)
        {
            return new Vector4((value1.X < value2.X) ? value1.X : value2.X, (value1.Y < value2.Y) ? value1.Y : value2.Y, (value1.Z < value2.Z) ? value1.Z : value2.Z, (value1.W < value2.W) ? value1.W : value2.W);
        }

        //
        // Сводка:
        //     Возвращает вектор, элементы которого являются максимальными значениями каждой
        //     пары элементов в двух заданных векторах.
        //
        // Параметры:
        //   value1:
        //     Первый вектор.
        //
        //   value2:
        //     Второй вектор.
        //
        // Возврат:
        //     Вектор, приведенный к максимуму.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Max(Vector4 value1, Vector4 value2)
        {
            return new Vector4((value1.X > value2.X) ? value1.X : value2.X, (value1.Y > value2.Y) ? value1.Y : value2.Y, (value1.Z > value2.Z) ? value1.Z : value2.Z, (value1.W > value2.W) ? value1.W : value2.W);
        }

        //
        // Сводка:
        //     Возвращает вектор, элементы которого являются абсолютными значениями каждого
        //     из элементов заданного вектора.
        //
        // Параметры:
        //   value:
        //     Вектор.
        //
        // Возврат:
        //     Вектор абсолютного значения.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Abs(Vector4 value)
        {
            return new Vector4(Math.Abs(value.X), Math.Abs(value.Y), Math.Abs(value.Z), Math.Abs(value.W));
        }


        /// <summary>
        /// Возвращает  вектор, содержащий знак компонент вектора 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Sign(Vector4 value)
        {
            return new Vector4(Math.Sign(value.X), Math.Sign(value.Y), Math.Sign(value.Z), Math.Sign(value.W));
        }

        //
        // Сводка:
        //     Возвращает вектор, элементы которого являются квадратным корнем каждого из элементов
        //     заданного вектора.
        //
        // Параметры:
        //   value:
        //     Вектор.
        //
        // Возврат:
        //     Вектор квадратного корня.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 SquareRoot(Vector4 value)
        {
            return new Vector4((double)Math.Sqrt(value.X), (double)Math.Sqrt(value.Y), (double)Math.Sqrt(value.Z), (double)Math.Sqrt(value.W));
        }

        //
        // Сводка:
        //     Складывает два вектора.
        //
        // Параметры:
        //   left:
        //     Первый складываемый вектор.
        //
        //   right:
        //     Второй складываемый вектор.
        //
        // Возврат:
        //     Суммарный вектор.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator +(Vector4 left, Vector4 right)
        {
            return new Vector4(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
        }

        //
        // Сводка:
        //     Вычитает второй вектор из первого.
        //
        // Параметры:
        //   left:
        //     Первый вектор.
        //
        //   right:
        //     Второй вектор.
        //
        // Возврат:
        //     Вектор, полученный в результате вычитания right из left.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator -(Vector4 left, Vector4 right)
        {
            return new Vector4(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
        }

        //
        // Сводка:
        //     Возвращает новый вектор, значения которого являются произведением каждой пары
        //     элементов в двух заданных векторах.
        //
        // Параметры:
        //   left:
        //     Первый вектор.
        //
        //   right:
        //     Второй вектор.
        //
        // Возврат:
        //     Поэлементный вектор произведения.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator *(Vector4 left, Vector4 right)
        {
            return new Vector4(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
        }

        //
        // Сводка:
        //     Умножает заданный вектор на указанное скалярное значение.
        //
        // Параметры:
        //   left:
        //     Вектор.
        //
        //   right:
        //     Скалярное значение.
        //
        // Возврат:
        //     Масштабированный вектор.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator *(Vector4 left, double right)
        {
            return left * new Vector4(right);
        }

        //
        // Сводка:
        //     Умножает скалярное значение на заданный вектор.
        //
        // Параметры:
        //   left:
        //     Вектор.
        //
        //   right:
        //     Скалярное значение.
        //
        // Возврат:
        //     Масштабированный вектор.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator *(double left, Vector4 right)
        {
            return new Vector4(left) * right;
        }

        //
        // Сводка:
        //     Делит первый вектор на второй.
        //
        // Параметры:
        //   left:
        //     Первый вектор.
        //
        //   right:
        //     Второй вектор.
        //
        // Возврат:
        //     Вектор, полученный в результате деления left на right.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator /(Vector4 left, Vector4 right)
        {
            return new Vector4(left.X / right.X, left.Y / right.Y, left.Z / right.Z, left.W / right.W);
        }

        //
        // Сводка:
        //     Делит заданный вектор на указанное скалярное значение.
        //
        // Параметры:
        //   value1:
        //     Вектор.
        //
        //   value2:
        //     Скалярное значение.
        //
        // Возврат:
        //     Результат деления.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator /(Vector4 value1, double value2)
        {
            double num = 1f / value2;
            return new Vector4(value1.X * num, value1.Y * num, value1.Z * num, value1.W * num);
        }

        //
        // Сводка:
        //     Преобразует заданный вектор в отрицательный.
        //
        // Параметры:
        //   value:
        //     Вектор, преобразуемый в отрицательный.
        //
        // Возврат:
        //     Вектор, преобразованный в отрицательный.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator -(Vector4 value)
        {
            return Zero - value;
        }

        //
        // Сводка:
        //     Возвращает значение, указывающее, равна ли каждая пара элементов в двух заданных
        //     векторах.
        //
        // Параметры:
        //   left:
        //     Первый сравниваемый вектор.
        //
        //   right:
        //     Второй сравниваемый вектор.
        //
        // Возврат:
        //     Значение true, если left и right равны; в противном случае — значение false.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Vector4 left, Vector4 right)
        {
            return left.Equals(right);
        }

        //
        // Сводка:
        //     Возвращает значение, указывающее на неравенство двух заданных векторов.
        //
        // Параметры:
        //   left:
        //     Первый сравниваемый вектор.
        //
        //   right:
        //     Второй сравниваемый вектор.
        //
        // Возврат:
        //     Значение true, если left и right не равны друг другу; в противном случае — значение
        //     false.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Vector4 left, Vector4 right)
        {
            return !(left == right);
        }
    }
}