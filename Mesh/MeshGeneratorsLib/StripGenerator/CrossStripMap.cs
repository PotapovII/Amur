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
        public CrossStripMap() { }
        public CrossStripMap(CrossStripMap m)
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
        }
        /// <summary>
        /// Вычисление узловой карты створа
        /// </summary>
        /// <param name="spline">функция дна</param>
        /// <param name="WaterLevel">отметка свободной поверхности потока</param>
        /// <param name="dy">шаг сетки по створу</param>
        /// <param name="y0">левая координата створа</param>
        /// <param name="Count">количество узлов по дну</param>
        /// <param name="CountH">максимальное количество узлов по глубине</param>
        public void CreateMap(TSpline spline, double WaterLevel, double dy, double y0, int Count, int CountH)
        {
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
                if ((i == 0 || i == Count - 1) && h < 0.0001)
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
                    if (n < 3)
                    {
                        if(i == 1 || i == Count-2)
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
        }
    }
}
