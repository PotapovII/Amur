//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 03.03.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshGeneratorsLib.StripGenerator
{
    using CommonLib;
    using GeometryLib;
    /// <summary>
    /// Генераторы сеток для живого сечения створа
    /// </summary>
    public interface IStripMeshGenerator
    {
        /// <summary>
        /// Опции для генерации Ленточной КЭ сетки 
        /// </summary>
        CrossStripMeshOption Option { get; }
        /// <summary>
        /// Правая береговая точка
        /// </summary>
        HKnot Right();
        /// <summary>
        /// Левая береговая точка
        /// </summary>
        HKnot Left();
        /// <summary>
        /// флаг левой берега
        /// </summary>
        bool DryLeft();
        /// <summary>
        /// флаг правого берега
        /// </summary>
        bool DryRight();
        /// <summary>
        /// Создает сетку в области живого сечения
        /// </summary>
        /// <param name="WetBed">смоченный периметр</param>
        /// <param name="WaterLevel">уровень свободной поверхности</param>
        /// <param name="xx">координаты дна по Х</param>
        /// <param name="yy">координаты дна по Y</param>
        /// <param name="Count">Количество узлов по дну</param>
        /// <returns>КЭ сетка</returns>
        IMesh CreateMesh(ref double WetBed, ref int[][] riverGates, double WaterLevel, double[] xx, double[] yy, int Count = 0);
        /// <summary>
        /// интерполяция дна
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        double Zeta(double arg);
    }
}
