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
        public CrossStripMap Map = null;

        protected uint CountElements = 0;
        /// <summary>
        /// Конструктор
        /// </summary>
        public ACrossStripMeshGenerator(CrossStripMeshOption Option) 
            : base(Option) 
        {
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
            double ymin = yy.Min();
            Map = new CrossStripMap(Option.BoundaryClose);
            Map.CreateMap(spline, WaterLevel, ymin, Count, width, left, right, dryLeft, dryRight);
        }
    }
}

