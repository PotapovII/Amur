using CommonLib.Geometry;
using MemLogLib;
using System;
using System.Collections.Generic;

namespace GeometryLib.Aalgorithms
{
    public class Duplicates
    {
        /// <summary>
        /// Удаление дубликатов 
        /// </summary>
        /// <param name="mas"></param>
        public static void RemoveDuplicates(ref int[] mas)
        {
            Array.Sort(mas);
            int k = 0;
            for (int i = 1; i < mas.Length; i++)
            {
                if (mas[k] != mas[i])
                    k++;
                mas[k] = mas[i];
            }
            for (int i = k + 1; i < mas.Length; i++)
                mas[i] = -1;
        }
        public static void RemoveDuplicatesShort(ref int[] mas)
        {
            Array.Sort(mas);
            int[] tmp = new int[mas.Length];
            tmp[0] = mas[0];
            int k = 0;
            for (int i = 1; i < mas.Length; i++)
            {
                if (tmp[k] != mas[i])
                    k++;
                tmp[k] = mas[i];
            }
            MEM.Alloc(k+1, ref mas);
            for (int i = 0; i < mas.Length; i++)
            {
                mas[i] = tmp[i];
            }
        }
        /// <summary>
        /// Удаление дубликатов ((
        /// </summary>
        /// <param name="mas"></param>
        public static void RemoveDuplicates(ref HPoint[] mas) 
        {
            // Сортировка по X и Y
            Array.Sort(mas);
            //for (int i = 0; i < mas.Length; i++)
            //    Console.Write(mas[i].ToString());
            //Console.WriteLine();
            HPoint[] tmp = new HPoint[mas.Length];
            tmp[0] = mas[0];
            int k = 0;
            for (int i = 1; i < mas.Length; i++)
            {
                if (HPoint.Equals(tmp[k], mas[i]) == true)
                    k++;
                tmp[k] = mas[i];
            }
            MEM.Alloc(k+1,ref mas);
            for (int i = 0; i < mas.Length; i++)
            {
                mas[i] = tmp[i];
            }
        }
    }
}
