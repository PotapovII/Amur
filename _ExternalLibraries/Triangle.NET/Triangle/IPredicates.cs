// -----------------------------------------------------------------------
// <copyright file="IPredicates.cs">
// Triangle.NET code by Christian Woltering, http://triangle.codeplex.com/
// </copyright>
// -----------------------------------------------------------------------

namespace TriangleNet
{
    using TriangleNet.Geometry;
    /// <summary>
    /// Original Triangle code by Jonathan Richard Shewchuk
    /// Адаптивные, точные арифметические геометрические предикаты.
    /// </summary>
    public interface IPredicates
    {
        /// <summary>
        /// Возвращает положительное значение, если точки pa, pb и pc расположены против 
        /// часовой стрелки; отрицательное значение, если они происходят по часовой стрелке; 
        /// и ноль, если они коллинеарны.
        /// </summary>
        double CounterClockwise(Point a, Point b, Point c);
        /// <summary>
        /// Возвращает положительное значение, если точка pd лежит внутри окружности, 
        /// проходящей через pa, pb и pc; отрицательное значение, если она лежит снаружи; 
        /// и ноль, если четыре точки являются соокружными.
        /// </summary>
        double InCircle(Point a, Point b, Point c, Point p);
        /// <summary>
        /// Найдите центр описанной окружности треугольника.
        /// </summary>
        /// <param name="org">Точка треугольника.</param>
        /// <param name="dest">Точка треугольника.</param>
        /// <param name="apex">Точка треугольника.</param>
        /// <param name="xi">Относительная координата нового местоположения.</param>
        /// <param name="eta">Относительная координата нового местоположения.</param>
        /// <returns>Return a positive value if the point pd lies inside the circle passing through 
        /// pa, pb, and pc; a negative value if it lies outside; and zero if the four points 
        /// are cocircular.</returns>
        /// <returns>Возвращает положительное значение, если точка pd лежит внутри окружности,
        /// проходящей через pa, pb и pc; отрицательное значение, если она лежит снаружи; 
        /// и ноль, если четыре точки являются соокружными.</returns>
        Point FindCircumcenter(Point org, Point dest, Point apex, ref double xi, ref double eta);
        /// <summary>
        /// Найдите центр описанной окружности треугольника.
        /// </summary>
        /// <param name="org">Точка треугольника.</param>
        /// <param name="dest">Точка треугольника.</param>
        /// <param name="apex">Точка треугольника.</param>
        /// <param name="xi">Относительная координата нового местоположения.</param>
        /// <param name="eta">Относительная координата нового местоположения.</param>
        /// <param name="offconstant">Константа смещения центра.</param>
        /// Результат возвращается как в координатах x-y, 
        /// так и в координатах xi-eta (барицентрических). 
        /// Система координат xi-eta определяется в терминах треугольника: 
        /// начало треугольника является началом системы координат; пункт 
        /// назначения треугольника — одна единица по оси xi; а вершина 
        /// треугольника — одна единица по оси eta. 
        /// Эта процедура также возвращает квадрат длины самого короткого ребра треугольника.
        Point FindCircumcenter(Point org, Point dest, Point apex, ref double xi, ref double eta,
            double offconstant);
    }
}
