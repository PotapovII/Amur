using CommonLib.Geometry;
using System;
using System.Threading.Tasks;

namespace GeometryLib.Aalgorithms
{
    /// <summary>
    /// Простые методы сортировки
    /// </summary>
    public class SortArray
    {
        #region Быстрая сортировка для массивов int
        /// <summary>
        /// быстрая сортировка точек по расстоянию от центра окружности исходного треугольника
        /// </summary>
        /// <param name="ids">индекс сортируемой точки</param>
        /// <param name="dists">дистанции от центра до сортируемой точки</param>
        /// <param name="left">начальный номер узла сортируемых массивов</param>
        /// <param name="right">конечный номер узла сортируемых массивов</param></param>
        /// Quicksort(ids, dists, 0, points.Length - 1);
        public static void Quicksort(int[] ids, double[] dists, int left, int right)
        {
            if (right - left <= 20)
            {
                for (var i = left + 1; i <= right; i++)
                {
                    var temp = ids[i];
                    var tempDist = dists[temp];
                    var j = i - 1;
                    while (j >= left && dists[ids[j]] > tempDist) 
                        ids[j + 1] = ids[j--];
                    ids[j + 1] = temp;
                }
            }
            else
            {
                var median = (left + right) >> 1;
                var i = left + 1;
                var j = right;
                Swap(ids, median, i);
                if (dists[ids[left]] > dists[ids[right]]) 
                    Swap(ids, left, right);
                if (dists[ids[i]] > dists[ids[right]]) 
                    Swap(ids, i, right);
                if (dists[ids[left]] > dists[ids[i]]) 
                    Swap(ids, left, i);

                var temp = ids[i];
                var tempDist = dists[temp];
                while (true)
                {
                    do i++; while (dists[ids[i]] < tempDist);
                    do j--; while (dists[ids[j]] > tempDist);
                    if (j < i) break;
                    Swap(ids, i, j);
                }
                ids[left + 1] = ids[j];
                ids[j] = temp;

                if (right - i + 1 >= j - left)
                {
                    Quicksort(ids, dists, i, right);
                    Quicksort(ids, dists, left, j - 1);
                }
                else
                {
                    Quicksort(ids, dists, left, j - 1);
                    Quicksort(ids, dists, i, right);
                }
            }
        }
        private static void Swap(int[] arr, int i, int j)
        {
            var tmp = arr[i];
            arr[i] = arr[j];
            arr[j] = tmp;
        }
        #endregion 

        #region Быстрая сортировка для массивов int
        /// <summary>
        /// Быстрая сортировка
        /// </summary>
        /// <param name="array">сортируемый массив</param>
        /// <returns>отсортированный массив</returns>
        protected static int[] Quicksort(int[] array, int leftIndex, int rightIndex)
        {
            var i = leftIndex;
            var j = rightIndex;
            var pivot = array[leftIndex];
            while (i <= j)
            {
                while (array[i] < pivot)
                {
                    i++;
                }
                while (array[j] > pivot)
                {
                    j--;
                }
                if (i <= j)
                {
                    int temp = array[i];
                    array[i] = array[j];
                    array[j] = temp;
                    i++;
                    j--;
                }
            }
            if (leftIndex < j)
                Quicksort(array, leftIndex, j);
            if (i < rightIndex)
                Quicksort(array, i, rightIndex);
            return array;
        }
        #endregion

        #region Быстрая сортировка для массивов int
        public static void QuickSort(ref int[] arr, bool parrallel = true)
        {
            if (parrallel == true)
                QuickSort(arr, 0, arr.Length - 1);
            else
                arr = Quicksort(arr, 0, arr.Length - 1);
        }

        private static void QuickSort(int[] arr, int left, int right)
        {
            if (left < right)
            {
                int pivotIndex = Partition(arr, left, right);
                Parallel.Invoke(
                    () => QuickSort(arr, left, pivotIndex - 1),
                    () => QuickSort(arr, pivotIndex + 1, right)
                );
            }
        }

        private static int Partition(int[] arr, int left, int right)
        {
            int pivot = arr[right];
            int i = left - 1;

            for (int j = left; j < right; j++)
            {
                if (arr[j] < pivot)
                {
                    i++;
                    Swap(arr, i, j);
                }
            }

            Swap(arr, i + 1, right);
            return i + 1;
        }

        //static void Swap(int[] arr, int i, int j)
        //{
        //    int temp = arr[i];
        //    arr[i] = arr[j];
        //    arr[j] = temp;
        //}
        #endregion

        #region Сортировка слиянием для IHPoint
        /// <summary>
        /// метод для слияния массивов
        /// </summary>
        /// <param name="array"></param>
        /// <param name="lowIndex"></param>
        /// <param name="middleIndex"></param>
        /// <param name="highIndex"></param>
        protected static void Merge(IHPoint[] array, int lowIndex, int middleIndex, int highIndex)
        {
            var left = lowIndex;
            var right = middleIndex + 1;
            var tempArray = new IHPoint[highIndex - lowIndex + 1];
            var index = 0;
            while ((left <= middleIndex) && (right <= highIndex))
            {
                //if (array[left] < array[right])
                if (array[left].CompareTo(array[right]) < 1)
                {
                    tempArray[index] = array[left];
                    left++;
                }
                else
                {
                    tempArray[index] = array[right];
                    right++;
                }

                index++;
            }

            for (var i = left; i <= middleIndex; i++)
            {
                tempArray[index] = array[i];
                index++;
            }

            for (var i = right; i <= highIndex; i++)
            {
                tempArray[index] = array[i];
                index++;
            }

            for (var i = 0; i < tempArray.Length; i++)
            {
                array[lowIndex + i] = tempArray[i];
            }
        }
        /// <summary>
        /// сортировка слиянием
        /// </summary>
        /// <param name="array"></param>
        /// <param name="lowIndex"></param>
        /// <param name="highIndex"></param>
        /// <returns></returns>
        protected static IHPoint[] MergeSort(IHPoint[] array, int lowIndex, int highIndex)
        {
            if (lowIndex < highIndex)
            {
                var middleIndex = (lowIndex + highIndex) / 2;
                MergeSort(array, lowIndex, middleIndex);
                MergeSort(array, middleIndex + 1, highIndex);
                Merge(array, lowIndex, middleIndex, highIndex);
            }
            return array;
        }
        /// <summary>
        /// Сортировка слиянием
        /// </summary>
        /// <param name="array">сортируемый массив</param>
        /// <returns>отсортированный массив</returns>
        public static IHPoint[] MergeSort(IHPoint[] array)
        {
            return MergeSort(array, 0, array.Length - 1);
        }
        #endregion

        #region Сортировка слиянием для массивов int
        /// <summary>
        /// метод для слияния массивов
        /// </summary>
        /// <param name="array"></param>
        /// <param name="lowIndex"></param>
        /// <param name="middleIndex"></param>
        /// <param name="highIndex"></param>
        protected static void Merge(int[] array, int lowIndex, int middleIndex, int highIndex)
        {
            var left = lowIndex;
            var right = middleIndex + 1;
            var tempArray = new int[highIndex - lowIndex + 1];
            var index = 0;

            while ((left <= middleIndex) && (right <= highIndex))
            {
                if (array[left] < array[right])
                {
                    tempArray[index] = array[left];
                    left++;
                }
                else
                {
                    tempArray[index] = array[right];
                    right++;
                }

                index++;
            }

            for (var i = left; i <= middleIndex; i++)
            {
                tempArray[index] = array[i];
                index++;
            }

            for (var i = right; i <= highIndex; i++)
            {
                tempArray[index] = array[i];
                index++;
            }

            for (var i = 0; i < tempArray.Length; i++)
            {
                array[lowIndex + i] = tempArray[i];
            }
        }
        /// <summary>
        /// сортировка слиянием
        /// </summary>
        /// <param name="array"></param>
        /// <param name="lowIndex"></param>
        /// <param name="highIndex"></param>
        /// <returns></returns>
        protected static int[] MergeSort(int[] array, int lowIndex, int highIndex)
        {
            if (lowIndex < highIndex)
            {
                var middleIndex = (lowIndex + highIndex) / 2;
                MergeSort(array, lowIndex, middleIndex);
                MergeSort(array, middleIndex + 1, highIndex);
                Merge(array, lowIndex, middleIndex, highIndex);
            }
            return array;
        }
        /// <summary>
        /// Запуск сортировки
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static int[] MergeSort(int[] array)
        {
            return MergeSort(array, 0, array.Length - 1);
        }

        #endregion

        #region Прямая сортировка 
        /// <summary>
        /// Сортировка слиянием
        /// </summary>
        /// <param name="array">сортируемый массив</param>
        /// <returns>отсортированный массив</returns>
        public static void Sort<T>(T[] array) where T : IComparable
        {
            for (int i = 0; i < array.Length; i++)
            {
                T tmp = array[i];
                int idx = i;
                for (int j = i + 1; j < array.Length; j++)
                {
                    if (tmp.CompareTo(array[j]) < 1)
                    {
                        tmp = array[i];
                        idx = i;
                    }
                }
                Swap(ref array[idx], ref array[i]);
            }
        }

        private static void Swap<T>(ref T a, ref T b)
        {
            T tmp = a;
            a = b;
            b = tmp;
        }

        #endregion
    }
}
