//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 04.03.2024 Потапов И.И.
//---------------------------------------------------------------------------
//                      Сбока No Property River Lib
//              с убранными из наследников Property классами 
//---------------------------------------------------------------------------
namespace MeshGeneratorsLib.StripGenerator
{
    using System;
    using System.Linq;
    /// <summary>
    /// ОО: Фронтальный генератор КЭ трехузловой сетки для русла реки в 
    /// створе с заданной геометрией дна и уровнем свободной поверхности
    /// </summary>
    [Serializable]
    public abstract class ACrossStripMeshGenerator : AStripMeshGenerator
    {
        protected int beginLeft = 0;
        protected int beginRight = 0;
        protected int CountBed = 0;

        public CrossStripMap Map = null;

        protected uint CountElements = 0;
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="MAXElem">максимальное количество КЭ сетки</param>
        /// <param name="MAXKnot">максимальное количество узлов сетки</param>
        public ACrossStripMeshGenerator():base(){ }
        
        public virtual void CalkBedFunction(ref double WetBed, double WaterLevel, double[] xx, double[] yy)
        {
            int beginLeft;
            int beginRight;
            // Поиск береговых точек створа
            LookingBoundary(WaterLevel, xx, yy, out beginLeft, out beginRight);
            // Расчет характеристик живого сечения створа
            CreateBedWet(ref WetBed, WaterLevel, xx, yy, beginLeft, beginRight);
            // шаг сетки по свободной поверхности
            // количество элементов
            CountBed = beginRight - beginLeft + 1;
        }
        /// <summary>
        /// Вычисляет карту КЭ сетки канала
        /// </summary>
        /// <param name="WaterLevel">уровень свободной поверхности</param>
        /// <param name="xx">координаты дна по Х</param>
        /// <param name="yy">координаты дна по Y</param>
        /// <param name="Count">Количество узлов по дну</param>
        public virtual void CreateMap(double WaterLevel, double[] xx, double[] yy, int Count, ref double WetBed)
        {
            CalkBedFunction(ref WetBed, WaterLevel, xx, yy);
            if (Count == 0)
                Count = CountBed;
            double dy = width / (Count - 1);
            double y0 = left.X;
            // глубина максимальная
            double H = WaterLevel - yy.Min();
            int CountH = (int)(H / dy) + 1;
            if (CountH < 5)
                throw new Exception("Сетка вырождена по напрявлению Z");
            Map = new CrossStripMap();
            Map.CreateMap(spline, WaterLevel, dy, y0, Count, CountH);
        }
    }
}

