//---------------------------------------------------------------------------
//                          ПРОЕКТ  "МКЭ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//        кодировка : 27.11.2022 Потапов И.И. (c# => c#)
//---------------------------------------------------------------------------
//                        12.11.2024 : IHPoint
//---------------------------------------------------------------------------
namespace GeometryLib.Vector
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    using CommonLib.Geometry;
    /// <summary>
    /// Представляет двухмерный вещественный вектор с полями двойной точности.
    /// </summary>
    [Serializable]
    public struct Vector2 : IEquatable<Vector2>, IFormattable, IHPoint
    {
        /// <summary>
        /// Координата X вектора.
        /// </summary>
        public double X { get; set; }
        /// <summary>
        ///  Координата Y вектора.
        /// </summary>
        public double Y { get; set; }
        /// <summary>
        /// Создает копию объекта
        /// </summary>
        /// <returns></returns>
        public IHPoint IClone() { return new Vector2(X, Y); }

        public int CompareTo(object obj)
        { 
            return ((Vector2)obj).X.CompareTo(X); 
        }

        public static double[] GetArrayX(Vector2[] a)
        {
            return (a.Select(x => x.X)).ToArray();
        }
        public static double[] GetArrayY(Vector2[] a)
        {
            return (a.Select(x => x.Y)).ToArray();
        }

        public static double[][] GetArrayX(Vector2[][] a)
        {
            double[][] res = new double[a.Length][];
            for (int i = 0; i < a.Length; i++)
                res[i] = (a[i].Select(x => x.X)).ToArray();
            return res;
        }
        public static double[][] GetArrayY(Vector2[][] a)
        {
            double[][] res = new double[a.Length][];
            for (int i = 0; i < a.Length; i++)
                res[i] = (a[i].Select(x => x.Y)).ToArray();
            return res;
        }

        public Vector2(IHPoint p)
        {
            X = p.X;
            Y = p.Y;
        }
        /// <summary>
        /// Возвращает вектор, два элемента которого равны нулю.
        /// </summary>
        public static Vector2 Zero
        {
            get
            {
                return default(Vector2);
            }
        }
        /// <summary>
        /// Получает вектор, два элемента которого равны единице.
        /// </summary>
        public static Vector2 One
        {
            get
            {
                return new Vector2(1f, 1f);
            }
        }
        /// <summary>
        /// Получает вектор (1,0).
        /// </summary>
        public static Vector2 UnitX
        {
            get
            {
                return new Vector2(1f, 0f);
            }
        }
        /// <summary>
        /// Получает вектор (0,1).
        /// </summary>
        public static Vector2 UnitY
        {
            get
            {
                return new Vector2(0f, 1f);
            }
        }
        /// <summary>
        /// Возвращает хэш-код данного экземпляра.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (X.GetHashCode() ^ Y.GetHashCode()).GetHashCode();
        }
        /// <summary>
        /// Возвращает значение, указывающее, равен ли данный экземпляр указанному объекту.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (!(obj is Vector2))
            {
                return false;
            }
            return Equals((Vector2)obj);
        }
        //
        // Сводка:
        //     Возвращает строковое представление текущего экземпляра, используя форматирование
        //     по умолчанию.
        //
        // Возврат:
        //     Строковое представление текущего экземпляра.
        public override string ToString()
        {
            return ToString("G", CultureInfo.CurrentCulture);
        }
        //
        // Сводка:
        //     Возвращает строковое представление текущего экземпляра, используя заданную строку
        //     форматирования для форматирования отдельных элементов.
        //
        // Параметры:
        //   format:
        //     Строка стандартного или настраиваемого числового формата, определяющая формат
        //     отдельных элементов.
        //
        // Возврат:
        //     Строковое представление текущего экземпляра.
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
            stringBuilder.Append('>');
            return stringBuilder.ToString();
        }
        //
        // Сводка:
        //     Возвращает длину вектора.
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
            double num2 = X * X + Y * Y;
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
            return X * X + Y * Y;
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
        public static double Distance(Vector2 value1, Vector2 value2)
        {
            //if (Vector.IsHardwareAccelerated)
            //{
            //    Vector2 vector = value1 - value2;
            //    double num = Dot(vector, vector);
            //    return (double)Math.Sqrt(num);
            //}

            double num2 = value1.X - value2.X;
            double num3 = value1.Y - value2.Y;
            double num4 = num2 * num2 + num3 * num3;
            return (double)Math.Sqrt(num4);
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
        public static double DistanceSquared(Vector2 value1, Vector2 value2)
        {
            //if (Vector.IsHardwareAccelerated)
            //{
            //    Vector2 vector = value1 - value2;
            //    return Dot(vector, vector);
            //}
            double num = value1.X - value2.X;
            double num2 = value1.Y - value2.Y;
            return num * num + num2 * num2;
        }
        //
        // Сводка:
        //     Возвращает вектор с тем же направлением, что и заданный вектор, но с длиной равной
        //     единице.
        //
        // Параметры:
        //   value:
        //     Нормализуемый вектор.
        //
        // Возврат:
        //     Нормализованный вектор.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Normalize(Vector2 value)
        {
            //if (Vector.IsHardwareAccelerated)
            //{
            //    double num = value.Length();
            //    return value / num;
            //}
            double num2 = value.X * value.X + value.Y * value.Y;
            double num3 = 1f / (double)Math.Sqrt(num2);
            return new Vector2(value.X * num3, value.Y * num3);
        }
        //
        // Сводка:
        //     Возвращает правый ортогональный вектор 
        //
        // Возврат:
        //     Ортогональный вектор.
        /// <summary>
        /// Получить правый ортогональный вектор 
        /// </summary>
        /// <returns>Ортогональный вектор</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 GetOrtogonalRight()
        {
            return new Vector2(Y, -X);
        }
        //
        // Сводка:
        //     Возвращает левый ортогональный вектор 
        //
        // Возврат:
        //     Ортогональный вектор.
        /// <summary>
        /// Получить левый ортогональный вектор 
        /// </summary>
        /// <returns>Ортогональный вектор</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 GetOrtogonalLeft()
        {
            return new Vector2(-Y, X);
        }
        //
        // Сводка:
        //     Возвращает отражение вектора от поверхности, которая имеет заданную нормаль.
        //
        // Параметры:
        //   vector:
        //     Исходный вектор.
        //
        //   normal:
        //     Нормаль отражаемой поверхности.
        //
        // Возврат:
        //     Отраженный вектор.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Reflect(Vector2 vector, Vector2 normal)
        {
            //if (Vector.IsHardwareAccelerated)
            //{
            //    double num = Dot(vector, normal);
            //    return vector - 2f * num * normal;
            //}

            double num2 = vector.X * normal.X + vector.Y * normal.Y;
            return new Vector2(vector.X - 2f * num2 * normal.X, vector.Y - 2f * num2 * normal.Y);
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
        public static Vector2 Clamp(Vector2 value1, Vector2 min, Vector2 max)
        {
            double x = value1.X;
            x = ((x > max.X) ? max.X : x);
            x = ((x < min.X) ? min.X : x);
            double y = value1.Y;
            y = ((y > max.Y) ? max.Y : y);
            y = ((y < min.Y) ? min.Y : y);
            return new Vector2(x, y);
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
        public static Vector2 Lerp(Vector2 value1, Vector2 value2, double amount)
        {
            return new Vector2(value1.X + (value2.X - value1.X) * amount, 
                               value1.Y + (value2.Y - value1.Y) * amount);
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
        public static Vector2 Add(Vector2 left, Vector2 right)
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
        public static Vector2 Subtract(Vector2 left, Vector2 right)
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
        public static Vector2 Multiply(Vector2 left, Vector2 right)
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
        public static Vector2 Multiply(Vector2 left, double right)
        {
            return left * right;
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
        public static Vector2 Multiply(double left, Vector2 right)
        {
            return left * right;
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
        public static Vector2 Divide(Vector2 left, Vector2 right)
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
        public static Vector2 Divide(Vector2 left, double divisor)
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
        public static Vector2 Negate(Vector2 value)
        {
            return -value;
        }

        //
        // Сводка:
        //     Создает новый объект System.Numerics.Vector2 с двумя элементами, имеющими одинаковое
        //     значение.
        //
        // Параметры:
        //   value:
        //     Значение, присваиваемое обоим элементам.
        public Vector2(double value)
            : this(value, value)
        {
        }

        //
        // Сводка:
        //     Создает вектор, элементы которого имеют заданные значения.
        //
        // Параметры:
        //   x:
        //     Значение, присваиваемое полю System.Numerics.Vector2.X.
        //
        //   y:
        //     Значение, присваиваемое полю System.Numerics.Vector2.Y.
        public Vector2(double x, double y)
        {
            X = x;
            Y = y;
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

            if (array.Length - index < 2)
            {
                throw new ArgumentException("Arg_ElementsInSourceIsGreaterThanDestination " + index.ToString());
            }
            array[index] = X;
            array[index + 1] = Y;
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
        public bool Equals(Vector2 other)
        {
            if (X == other.X)
            {
                return Y == other.Y;
            }
            return false;
        }

        //
        // Сводка:
        //     Возвращает скалярное произведение двух векторов.
        //
        // Параметры:
        //   value1:
        //     Первый вектор.
        //
        //   value2:
        //     Второй вектор.
        //
        // Возврат:
        //     Скалярное произведение.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Dot(Vector2 value1, Vector2 value2)
        {
            return value1.X * value2.X + value1.Y * value2.Y;
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
        public static Vector2 Min(Vector2 value1, Vector2 value2)
        {
            return new Vector2((value1.X < value2.X) ? value1.X : value2.X, (value1.Y < value2.Y) ? value1.Y : value2.Y);
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
        public static Vector2 Max(Vector2 value1, Vector2 value2)
        {
            return new Vector2((value1.X > value2.X) ? value1.X : value2.X, (value1.Y > value2.Y) ? value1.Y : value2.Y);
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
        public static Vector2 Abs(Vector2 value)
        {
            return new Vector2(Math.Abs(value.X), Math.Abs(value.Y));
        }

        /// <summary>
        /// Возвращает  вектор, содержащий знак компонент вектора 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Sign(Vector3 value)
        {
            return new Vector2(Math.Sign(value.X), Math.Sign(value.Y));
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
        public static Vector2 SquareRoot(Vector2 value)
        {
            return new Vector2((double)Math.Sqrt(value.X), (double)Math.Sqrt(value.Y));
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
        public static Vector2 operator +(Vector2 left, Vector2 right)
        {
            return new Vector2(left.X + right.X, left.Y + right.Y);
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
        public static Vector2 operator -(Vector2 left, Vector2 right)
        {
            return new Vector2(left.X - right.X, left.Y - right.Y);
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
        public static Vector2 operator *(Vector2 left, Vector2 right)
        {
            return new Vector2(left.X * right.X, left.Y * right.Y);
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
        public static Vector2 operator *(double left, Vector2 right)
        {
            return new Vector2(left, left) * right;
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
        public static Vector2 operator *(Vector2 left, double right)
        {
            return left * new Vector2(right, right);
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
        public static Vector2 operator /(Vector2 left, Vector2 right)
        {
            return new Vector2(left.X / right.X, left.Y / right.Y);
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
        public static Vector2 operator /(Vector2 value1, double value2)
        {
            double num = 1f / value2;
            return new Vector2(value1.X * num, value1.Y * num);
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
        public static Vector2 operator -(Vector2 value)
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
        public static bool operator ==(Vector2 left, Vector2 right)
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
        public static bool operator !=(Vector2 left, Vector2 right)
        {
            return !(left == right);
        }
    }

}
