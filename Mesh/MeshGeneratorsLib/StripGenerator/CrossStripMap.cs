//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 25.06.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshGeneratorsLib.StripGenerator
{
    using System;
    using MemLogLib;
    using GeometryLib;

    /// <summary>
    /// ОО: Используется генераторами сетки в створе русла для построения КЭ сетки общего вида 1 порядока
    /// Используется для вычисление разостных вертикальных операций на сетке
    /// </summary>
    [Serializable]
    public class CrossStripMap
    {
        /// <summary>
        /// Способ замыкания крутого берега
        /// </summary>
        public int BoundaryClose;
        /// <summary>
        /// количество узлов по вертикали
        /// </summary>
        public uint[] map1D = null;
        /// <summary>
        /// карта узлов 2D сетки
        /// </summary>
        public uint[][] map = null;
        /// <summary>
        /// вертикальная координата узлов в карте
        /// </summary>
        public double[][] mapZ = null;
        /// <summary>
        /// шаг сетки по створу
        /// </summary>
        public double dy = 0;
        /// <summary>
        /// левая координата створа
        /// </summary>
        public double y0 = 0;
        /// <summary>
        /// количество узлов в сетке
        /// </summary>
        public uint CountKnots = 0;
        /// <summary>
        /// количество узлов по дну
        /// </summary>
        public int Count;
        /// <summary>
        /// максимальное количество узлов по глубине
        /// </summary>
        public int CountH;
        /// <summary>
        /// отметка свободной поверхности потока
        /// </summary>
        public double WaterLevel;
        public CrossStripMap(int BoundaryClose = 0) 
        {
            this.BoundaryClose = BoundaryClose;
        }
        public CrossStripMap(CrossStripMap m, int BoundaryClose = 0)
        {
            this.map1D = m.map1D;
            this.map = m.map;
            this.mapZ = m.mapZ;
            this.dy = m.dy;
            this.y0 = m.y0;
            this.CountKnots = m.CountKnots;
            this.Count = m.Count;
            this.CountH = m.CountH;
            this.WaterLevel = m.WaterLevel;
            this.BoundaryClose = m.BoundaryClose;
        }
        /// <summary>
        /// Вычисление узловой карты створа
        /// </summary>
        /// <param name="spline">функция дна</param>
        /// <param name="WaterLevel">отметка свободной поверхности потока</param>
        /// <param name="ymin">минимальная отменка дна</param>
        /// <param name="Count">количество узлов по дну</param>
        /// <param name="width">щирина створа</param>
        /// <param name="left">Левая береговая точка</param>
        /// <param name="right">Правая береговая точка</param>
        /// <exception cref="Exception"></exception>

        public void CreateMap(TSpline spline, double WaterLevel, double ymin,
            int Count, double width, HKnot left, HKnot right, bool dryLeft, bool dryRight)
        {
            // шаг сетки по створу
            double dy = width / (Count - 1);
            // левая координата створа
            double y0 = left.X;
            // глубина максимальная
            double H = WaterLevel - ymin;
            // максимальное количество узлов по глубине
            int CountH = (int)(H / dy) + 1;
            if (CountH < 5)
                CountH = 5;
                //throw new Exception("Сетка вырождена по напрявлению Z");
            this.dy = dy;
            this.y0 = y0;
            this.Count = Count;
            this.CountH = CountH;
            this.WaterLevel = WaterLevel;
            MEM.Alloc(Count, ref map1D);
            MEM.Alloc(Count, CountH, ref map);
            MEM.Alloc(Count, CountH, ref mapZ);
            CountKnots = 0;

            for (int i = 0; i < Count; i++)
            {
                // координата текущего столбца
                double y = y0 + dy * i;
                // вычисляем уровень дна
                double zeta = spline.Value(y);
                // находим глубину 
                double h = WaterLevel - zeta;
                if(dryLeft == true && i == 0)
                    map1D[i] = 1;
                else
                if (dryRight == true && i == Count - 1)
                    map1D[i] = 1;
                else
                {
                    // находим количество узлов на вертикали
                    uint n = (uint)Math.Abs(h / dy) + 1;
                    if (n == 1) n = 2;
                    if (i == 0)
                        map1D[i] = n;
                    else
                    {
                        if (map1D[i - 1] == n)
                            map1D[i] = n;
                        else
                            if (map1D[i - 1] > n)
                            map1D[i] = map1D[i - 1] - 1;
                        else
                            map1D[i] = map1D[i - 1] + 1;
                    }
                }
            }
            switch (BoundaryClose)
            {
                case 0: // обратное сглаживание - артефактная сетка на крутых берегах
                    if (Math.Abs(map1D[Count - 1] - map1D[Count - 2]) > 1)
                    {
                        // цикл коррекции
                        for (int i = Count - 1; i > 0; i--)
                        {
                            int dn = (int)map1D[i] - (int)map1D[i - 1];
                            if (Math.Abs(dn) > 1)
                            {
                                if (dn > 0)
                                    map1D[i - 1] = map1D[i] - 1;
                                else
                                    map1D[i - 1] = map1D[i] + 1;
                            }

                        }
                    }
                    break;
                case 1:
                    if (Math.Abs(map1D[Count - 1] - map1D[Count - 2]) > 2)
                    {
                        // цикл коррекции
                        for (int i = Count - 1; i > 0; i--)
                        {
                            int dn = (int)map1D[i] - (int)map1D[i - 1];
                            if (Math.Abs(dn) > 2)
                            {
                                if (dn > 0)
                                    map1D[i - 1] = map1D[i] - 2;
                                else
                                    map1D[i - 1] = map1D[i] + 2;
                            }

                        }
                    }
                    break;
            }

            for (int i = 0; i < Count; i++)
            {
                // координата текущего столбца
                double y = y0 + dy * i;
                // вычисляем уровень дна
                double zeta = spline.Value(y);
                // находим глубину 
                double h = WaterLevel - zeta;

                // находим количество узлов на вертикали
                uint n = map1D[i];
                if (n == 1)
                {
                    mapZ[i][0] = WaterLevel;
                    map[i][0] = CountKnots;
                    CountKnots++;
                }
                else
                {
                    double dh = h / (n - 1);
                    for (int j = 0; j < n; j++)
                    {
                        mapZ[i][j] = WaterLevel - dh * j;
                        map[i][j] = CountKnots;
                        CountKnots++;
                    }
                }
            }
        }


        public void CreateMapOld(TSpline spline, double WaterLevel, double ymin,
            int Count, double width, HKnot left, HKnot right)
        {
            // шаг сетки по створу
            double dy = width / (Count - 1);
            // левая координата створа
            double y0 = left.X;
            // глубина максимальная
            double H = WaterLevel - ymin;
            // максимальное количество узлов по глубине
            int CountH = (int)(H / dy) + 1;
            if (CountH < 5)
                throw new Exception("Сетка вырождена по напрявлению Z");
            this.dy = dy;
            this.y0 = y0;
            this.Count = Count;
            this.CountH = CountH;
            this.WaterLevel = WaterLevel;
            MEM.Alloc(Count, ref map1D);
            MEM.Alloc(Count, CountH, ref map);
            MEM.Alloc(Count, CountH, ref mapZ);
            CountKnots = 0;
            for (int i = 0; i < Count; i++)
            {
                // координата текущего столбца
                double y = y0 + dy * i;
                // вычисляем уровень дна
                double zeta = spline.Value(y);
                // находим глубину 
                double h = WaterLevel - zeta;
                
                if ((i == 0 || i == Count - 1) && h < MEM.Error5)
                {
                    // берега
                    map1D[i] = 1;
                    mapZ[i][0] = WaterLevel;
                    map[i][0] = CountKnots;
                    CountKnots++;
                }
                else
                {
                    // находим количество узлов на вертикали
                    uint n = (uint)Math.Abs(h / dy) + 1;
                    uint nOld;
                    if (i == 0)
                        nOld = n;
                    else
                        nOld = map1D[i-1];
                    if( n == nOld || n == nOld-1 || n == nOld+1 )
                    {
                        map1D[i] = n;
                    }
                    else
                    {
                        if(n > nOld) n = nOld + 1;
                        if(n < nOld) n = nOld - 1;
                    }
                    if (n < 3)
                    {
                        if (i == 1 || i == Count - 2)
                            n = 2;
                        else
                            n = 3;
                    }
                    map1D[i] = n;
                    double dh = h / (n - 1);
                    for (int j = 0; j < n; j++)
                    {
                        mapZ[i][j] = WaterLevel - dh * j;
                        map[i][j] = CountKnots;
                        CountKnots++;
                    }
                }
            }
          //  LOG.Print("map", map);
        }
    }
}
