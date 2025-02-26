// -----------------------------------------------------------------------
// Джонатана Ричарда Шевчука http://www.cs.cmu.edu/~quake/triangle.html
// и Кристиана Уолтеринга, http://triangle.codeplex.com/
//              Адаптация кода : 20.02.2025 Потапов И.И.
// -----------------------------------------------------------------------
namespace GeometryLib.Aalgorithms
{
    using System;
    using CommonLib.Geometry;
    /// <summary>
    /// ОО: Класс для сортировки массиа точек, поддерживает 
    /// быструю сортировку      Sort(...)
    /// и сортировку разрезами  Alternate(...)
    /// </summary>
    [Serializable]
    public class SorterForIHPoint
    {
        private const int RANDOM_SEED = 57113;
        Random rand;
        IHPoint[] points;

        SorterForIHPoint(IHPoint[] points, int seed)
        {
            this.points = points;
            this.rand = new Random(seed);
        }
        /// <summary>
        /// Сортирует заданный массив точек 
        /// </summary>
        /// <param name="array">Массив точек</param>
        /// <param name="seed">Случайное начальное значение, используемое для поворота.</param>
        public static void Sort(IHPoint[] array, int seed = RANDOM_SEED)
        {
            var qs = new SorterForIHPoint(array, seed);
            qs.QuickSort(0, array.Length - 1);
        }
        /// <summary>
        /// Наложить чередующиеся разрезы на заданный массив вершин.
        /// </summary>
        /// <param name="array">Массив точек.</param>
        /// <param name="length">Количество вершин для сортировки.</param>
        /// <param name="seed">Случайное начальное значение, используемое для поворота.</param>
        public static void Alternate(IHPoint[] array, int length, int seed = RANDOM_SEED)
        {
            var qs = new SorterForIHPoint(array, seed);
            int divider = length >> 1;
            // Пересортируйте массив вершин, чтобы учесть чередующиеся разрезы.
            if (length - divider >= 2)
            {
                if (divider >= 2)
                {
                    qs.AlternateAxes(0, divider - 1, 1);
                }
                qs.AlternateAxes(divider, length - 1, 1);
            }
        }

        #region Quicksort

        /// <summary>
        /// Отсортируйте массив вершин по координатам .Y.X, 
        /// используя координату.Y в качестве дополнительного ключа в диапазоне
        /// </summary>
        /// <param name="left">номер левой точки</param>
        /// <param name="right">номер правой точки</param>
        /// <remarks>
        /// Использует быструю сортировку. Время выбрано случайным образом. 
        /// Нет, я не допустил ни одной из обычных ошибок при быстрой сортировке.
        /// </remarks>
        private void QuickSort(int left, int right)
        {
            int oleft = left;
            int oright = right;
            int arraysize = right - left + 1;
            int pivot;
            double pivotx, pivoty;
            IHPoint temp;

            var array = this.points;

            if (arraysize < 32)
            {
                // Сортировка по вставке
                for (int i = left + 1; i <= right; i++)
                {
                    var a = array[i];
                    int j = i - 1;
                    while (j >= left && (array[j].X > a.X || (array[j].X == a.X && array[j].Y > a.Y)))
                    {
                        array[j + 1] = array[j];
                        j--;
                    }
                    array[j + 1] = a;
                }

                return;
            }

            // Выберите случайную точку поворота, чтобы разделить массив.
            pivot = rand.Next(left, right);
            pivotx = array[pivot].X;
            pivoty = array[pivot].Y;
            // Разделить массив.
            left--;
            right++;
            while (left < right)
            {
                // Найдите вершину, у которой координата .X слишком велика для левой стороны.
                do
                {
                    left++;
                }
                while ((left <= right) && ((array[left].X < pivotx) ||
                    ((array[left].X == pivotx) && (array[left].Y < pivoty))));
                // Найдите вершину, у которой координата .X слишком мала для правой.
                do
                {
                    right--;
                }
                while ((left <= right) && ((array[right].X > pivotx) ||
                    ((array[right].X == pivotx) && (array[right].Y > pivoty))));

                if (left < right)
                {
                    // Поменяйте местами левую и правую вершины.
                    temp = array[left];
                    array[left] = array[right];
                    array[right] = temp;
                }
            }

            if (left > oleft)
            {
                // Рекурсивно отсортируйте левое подмножество.
                QuickSort(oleft, left);
            }

            if (oright > right + 1)
            {
                // Рекурсивно отсортируйте нужное подмножество.
                QuickSort(right + 1, oright);
            }
        }

        #endregion

        #region Чередуйте поперечный разрез по осям.

        /// <summary>
        /// Сортирует вершины в соответствии с алгоритмом "разделяй и властвуй" с 
        /// помощью чередующихся разрезов.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="axis"></param>
        /// <remarks>
        /// Разбиения .Y.X-координата, если ось == 0; .Y.Y-координата, если ось == 1. 
        /// В базовом случае подмножества, содержащие только две или три вершины, 
        /// всегда сортируются.Y.X-координата.
        /// </remarks>
        private void AlternateAxes(int left, int right, int axis)
        {
            int size = right - left + 1;
            int divider = size >> 1;

            if (size <= 3)
            {
                // Рекурсивный базовый вариант: подмножества из двух или трех вершин будут
                // обрабатываться специальным образом и всегда должны быть отсортированы .Y.X-координата.
                axis = 0;
            }

            // Перегородка с горизонтальным или вертикальным разрезом.
            if (axis == 0)
            {
                VertexMedianX(left, right, left + divider);
            }
            else
            {
                VertexMedianY(left, right, left + divider);
            }

            // Рекурсивно разбивает подмножества перекрестным разрезом.
            if (size - divider >= 2)
            {
                if (divider >= 2)
                {
                    AlternateAxes(left, left + divider - 1, 1 - axis);
                }

                AlternateAxes(left + divider, right, 1 - axis);
            }
        }

        /// <summary>
        /// Алгоритм упорядоченной статистики, почти. Перетасовывает массив вершин таким образом, что
        /// первые "медианные" вершины лексикографически располагаются перед остальными вершинами.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="median"></param>
        /// <remarks>
        /// Uses the.X-coordinate as the primary key. Very similar to the QuickSort()
        /// procedure, but runs in randomized linear time.
        /// Использует координату .X в качестве первичного ключа. 
        /// Очень похоже на процедуру быстрой QuickSort(), но 
        /// выполняется за случайное линейное время.
        /// </remarks>
        private void VertexMedianX(int left, int right, int median)
        {
            int arraysize = right - left + 1;
            int oleft = left, oright = right;
            int pivot;
            double pivot1, pivot2;
            IHPoint temp;

            var array = this.points;

            if (arraysize == 2)
            {
                // Рекурсивный базовый вариант.
                if ((array[left].X > array[right].X) ||
                    ((array[left].X == array[right].X) &&
                     (array[left].Y > array[right].Y)))
                {
                    temp = array[right];
                    array[right] = array[left];
                    array[left] = temp;
                }
                return;
            }

            // Выберите случайную точку поворота, чтобы разделить массив.
            pivot = rand.Next(left, right);
            pivot1 = array[pivot].X;
            pivot2 = array[pivot].Y;

            left--;
            right++;
            while (left < right)
            {
                // Найдите вершину, у которой координата .X слишком велика для левой стороны.
                do
                {
                    left++;
                }
                while ((left <= right) && ((array[left].X < pivot1) ||
                    ((array[left].X == pivot1) && (array[left].Y < pivot2))));

                // Найдите вершину, у которой координата .X слишком мала для правой.
                do
                {
                    right--;
                }
                while ((left <= right) && ((array[right].X > pivot1) ||
                    ((array[right].X == pivot1) && (array[right].Y > pivot2))));

                if (left < right)
                {
                    // Поменяйте местами левую и правую вершины.
                    temp = array[left];
                    array[left] = array[right];
                    array[right] = temp;
                }
            }

            // В отличие от функции vertexsort(), не более одного из следующих условий является истинным.
            if (left > median)
            {
                // Рекурсивно перетасуйте левое подмножество.
                VertexMedianX(oleft, left - 1, median);
            }

            if (right < median - 1)
            {
                // Рекурсивно перетасуйте нужное подмножество.
                VertexMedianX(right + 1, oright, median);
            }
        }

        /// <summary>
        /// Алгоритм упорядоченной статистики, почти.  Перетасовывает массив вершин таким образом, что
        /// первые "медианные" вершины лексикографически располагаются перед остальными вершинами.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="median"></param>
        /// <remarks>
        /// Использует координату .Y в качестве первичного ключа. 
        /// Очень похоже на процедуру быстрой QuickSort(), но 
        /// выполняется за случайное линейное время.
        /// </remarks>
        private void VertexMedianY(int left, int right, int median)
        {
            int arraysize = right - left + 1;
            int oleft = left, oright = right;
            int pivot;
            double pivot1, pivot2;
            IHPoint temp;

            var array = this.points;

            if (arraysize == 2)
            {
                // Рекурсивный базовый вариант.
                if ((array[left].Y > array[right].Y) ||
                    ((array[left].Y == array[right].Y) &&
                     (array[left].X > array[right].X)))
                {
                    temp = array[right];
                    array[right] = array[left];
                    array[left] = temp;
                }
                return;
            }

            // Выберите случайную точку поворота, чтобы разделить массив.
            pivot = rand.Next(left, right);
            pivot1 = array[pivot].Y;
            pivot2 = array[pivot].X;

            left--;
            right++;
            while (left < right)
            {
                // Найдите вершину, у которой координата .X слишком велика для левой стороны.
                do
                {
                    left++;
                }
                while ((left <= right) && ((array[left].Y < pivot1) ||
                    ((array[left].Y == pivot1) && (array[left].X < pivot2))));

                // Найдите вершину, у которой координата .X слишком мала для правой.
                do
                {
                    right--;
                }
                while ((left <= right) && ((array[right].Y > pivot1) ||
                    ((array[right].Y == pivot1) && (array[right].X > pivot2))));

                if (left < right)
                {
                    // Поменяйте местами левую и правую вершины.
                    temp = array[left];
                    array[left] = array[right];
                    array[right] = temp;
                }
            }

            // В отличие от функции быстрой QuickSort(), верно не более одного из следующих условий.
            if (left > median)
            {
                // Рекурсивно перетасуйте левое подмножество.
                VertexMedianY(oleft, left - 1, median);
            }

            if (right < median - 1)
            {
                // Рекурсивно перетасуйте нужное подмножество.
                VertexMedianY(right + 1, oright, median);
            }
        }

        #endregion
    }
}
