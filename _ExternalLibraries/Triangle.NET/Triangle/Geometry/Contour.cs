// -----------------------------------------------------------------------
// <copyright file="Contour.cs" company="">
// Triangle.NET code by Christian Woltering, http://triangle.codeplex.com/
// </copyright>
// -----------------------------------------------------------------------

namespace TriangleNet.Geometry
{
    using System;
    using System.Collections.Generic;

    public class ContourOld
    {
        int marker;

        bool convex;

        /// <summary>
        /// Gets or sets the list of points making up the contour.
        /// </summary>
        public List<Vertex> Points { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Contour" /> class.
        /// </summary>
        /// <param name="points">The points that make up the contour.</param>
        public ContourOld(IEnumerable<Vertex> points)
            : this(points, 0, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Contour" /> class.
        /// </summary>
        /// <param name="points">The points that make up the contour.</param>
        /// <param name="marker">Contour marker.</param>
        public ContourOld(IEnumerable<Vertex> points, int marker)
            : this(points, marker, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Contour" /> class.
        /// </summary>
        /// <param name="points">The points that make up the contour.</param>
        /// <param name="marker">Contour marker.</param>
        /// <param name="convex">The hole is convex.</param>
        public ContourOld(IEnumerable<Vertex> points, int marker, bool convex)
        {
            AddPoints(points);

            this.marker = marker;
            this.convex = convex;
        }

        public List<ISegment> GetSegments()
        {
            var segments = new List<ISegment>();

            var p = this.Points;

            int count = p.Count - 1;

            for (int i = 0; i < count; i++)
            {
                // Add segments to polygon.
                segments.Add(new Segment(p[i], p[i + 1], marker));
            }

            // Close the contour.
            segments.Add(new Segment(p[count], p[0], marker));

            return segments;
        }

        /// <summary>
        /// Try to find a point inside the contour.
        /// </summary>
        /// <param name="limit">The number of iterations on each segment (default = 5).</param>
        /// <param name="eps">Threshold for co-linear points (default = 2e-5).</param>
        /// <returns>Point inside the contour</returns>
        /// <exception cref="Exception">Throws if no point could be found.</exception>
        /// <remarks>
        /// For each corner (index i) of the contour, the 3 points with indices i-1, i and i+1
        /// are considered and a search on the line through the corner vertex is started (either
        /// on the bisecting line, or, if <see cref="IPredicates.CounterClockwise"/> is less than
        /// eps, on the perpendicular line.
        /// A given number of points will be tested (limit), while the distance to the contour
        /// boundary will be reduced in each iteration (with a factor 1 / 2^i, i = 1 ... limit).
        /// </remarks>
        public Point FindInteriorPoint(int limit = 5, double eps = 2e-5)
        {
            if (convex)
            {
                int count = this.Points.Count;

                var point = new Point(0.0, 0.0);

                for (int i = 0; i < count; i++)
                {
                    point.x += this.Points[i].x;
                    point.y += this.Points[i].y;
                }

                // If the contour is convex, use its Centroid.
                point.x /= count;
                point.y /= count;

                return point;
            }

            return FindPointInPolygon(this.Points, limit, eps);
        }

        private void AddPoints(IEnumerable<Vertex> points)
        {
            this.Points = new List<Vertex>(points);

            int count = Points.Count - 1;

            // Check if first vertex equals last vertex.
            if (Points[0] == Points[count])
            {
                Points.RemoveAt(count);
            }
        }

        #region Helper methods

        private static Point FindPointInPolygon(List<Vertex> contour, int limit, double eps)
        {
            var bounds = new Rectangle();
            bounds.Expand(contour);

            int length = contour.Count;

            var test = new Point();

            Point a, b, c; // Current corner points.

            double bx, by;
            double dx, dy;
            double h;

            var predicates = new RobustPredicates();

            a = contour[0];
            b = contour[1];

            for (int i = 0; i < length; i++)
            {
                c = contour[(i + 2) % length];

                // Corner point.
                bx = b.x;
                by = b.y;

                // NOTE: if we knew the contour points were in counterclockwise order, we
                // could skip concave corners and search only in one direction.

                h = predicates.CounterClockwise(a, b, c);

                if (Math.Abs(h) < eps)
                {
                    // Points are nearly co-linear. Use perpendicular direction.
                    dx = (c.y - a.y) / 2;
                    dy = (a.x - c.x) / 2;
                }
                else
                {
                    // Direction [midpoint(a-c) -> corner point]
                    dx = (a.x + c.x) / 2 - bx;
                    dy = (a.y + c.y) / 2 - by;
                }

                // Move around the contour.
                a = b;
                b = c;

                h = 1.0;

                for (int j = 0; j < limit; j++)
                {
                    // Search in direction.
                    test.x = bx + dx * h;
                    test.y = by + dy * h;

                    if (bounds.Contains(test) && IsPointInPolygon(test, contour))
                    {
                        return test;
                    }

                    // Search in opposite direction (see NOTE above).
                    test.x = bx - dx * h;
                    test.y = by - dy * h;

                    if (bounds.Contains(test) && IsPointInPolygon(test, contour))
                    {
                        return test;
                    }

                    h = h / 2;
                }
            }

            throw new Exception();
        }

        /// <summary>
        /// Return true if the given point is inside the polygon, or false if it is not.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <param name="poly">The polygon (list of contour points).</param>
        /// <returns></returns>
        /// <remarks>
        /// WARNING: If the point is exactly on the edge of the polygon, then the function
        /// may return true or false.
        /// 
        /// See http://alienryderflex.com/polygon/
        /// </remarks>
        private static bool IsPointInPolygon(Point point, List<Vertex> poly)
        {
            bool inside = false;

            double x = point.x;
            double y = point.y;

            int count = poly.Count;

            for (int i = 0, j = count - 1; i < count; i++)
            {
                if (((poly[i].y < y && poly[j].y >= y) || (poly[j].y < y && poly[i].y >= y))
                    && (poly[i].x <= x || poly[j].x <= x))
                {
                    inside ^= (poly[i].x + (y - poly[i].y) / (poly[j].y - poly[i].y) * (poly[j].x - poly[i].x) < x);
                }

                j = i;
            }

            return inside;
        }

        #endregion
    }


    public class Contour
    {
        //int marker;

        bool convex;

        /// <summary>
        /// Gets or sets the list of points making up the contour.
        /// Получает или задает список точек, составляющих контур.
        /// </summary>
        public List<Vertex> Points { get; set; }
        /// <summary>
        /// Макркера контура
        /// </summary>
        public List<int> Markers { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Contour" /> class.
        /// Инициализирует новый экземпляр класса <see cref = "Contour" />.
        /// </summary>
        /// <param name="points">Точки, составляющие контур. 
        /// The points that make up the contour.</param>
        public Contour(IEnumerable<Vertex> points)
            : this(points, 0, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Contour" /> class.
        /// </summary>
        /// <param name="points">The points that make up the contour.</param>
        /// <param name="marker">Contour marker.</param>
        public Contour(IEnumerable<Vertex> points, int marker)
            : this(points, marker, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Contour" /> class.
        /// </summary>
        /// <param name="points">The points that make up the contour.</param>
        /// <param name="marker">Contour marker.</param>
        /// <param name="convex">The hole is convex.</param>
        /// <краткое описание>
        /// Инициализирует новый экземпляр класса <см. cref="Contour" />.
        /// </краткое содержание>
        /// <имя параметра="точки">Точки, составляющие контур.</параметр>
        /// <имя параметра="маркер">Контурный маркер.</параметр>
        /// <имя параметра="выпуклый">Отверстие выпуклое.</параметр>
        public Contour(IEnumerable<Vertex> points, int marker, bool convex)
        {
            AddPoints(points);
            this.convex = convex;
            Markers = new List<int>();
            for (int i = 0; i < Points.Count; i++)
                Markers.Add(marker);
        }

        public Contour(List<Vertex> Points, List<int> Markers, bool convex = true)
        {
            this.Points = Points;
            this.Markers = Markers;
            this.convex = convex;
        }

        public List<ISegment> GetSegments()
        {
            var segments = new List<ISegment>();
            var p = this.Points;
            int count = p.Count - 1;
            for (int i = 0; i < count; i++)
            {
                // Add segments to polygon.
                segments.Add(new Segment(p[i], p[i + 1], Markers[i]));
            }
            // Close the contour.
            segments.Add(new Segment(p[count], p[0], Markers[count]));
            return segments;
        }

        /// <summary>
        /// Try to find a point inside the contour.
        /// </summary>
        /// <param name="limit">The number of iterations on each segment (default = 5).</param>
        /// <param name="eps">Threshold for co-linear points (default = 2e-5).</param>
        /// <returns>Point inside the contour</returns>
        /// <exception cref="Exception">Throws if no point could be found.</exception>
        /// <remarks>
        /// For each corner (index i) of the contour, the 3 points with indices i-1, i and i+1
        /// are considered and a search on the line through the corner vertex is started (either
        /// on the bisecting line, or, if <see cref="IPredicates.CounterClockwise"/> is less than
        /// eps, on the perpendicular line.
        /// A given number of points will be tested (limit), while the distance to the contour
        /// boundary will be reduced in each iteration (with a factor 1 / 2^i, i = 1 ... limit).
        /// Попробуйте найти точку внутри контура.
        /// </summary>
        /// <param name = "limit"> Количество итераций в каждом сегменте (по умолчанию = 5). </param>
        /// <param name = "eps"> Порог для коллинеарных точек (по умолчанию = 2e-5). </param>
        /// <returns> Точка внутри контура </returns>
        /// <exception cref = "Exception"> Выбрасывается, если точка не может быть найдена. </exception>
        /// <примечания>
        /// Для каждого угла (индекс i) контура, 3 точки с индексами i-1, i и i + 1
        /// рассматриваются и запускается поиск по линии, проходящей через вершину угла (либо
        /// на биссектрисе, или, если <см. cref = "IPredicates.CounterClockwise" /> меньше, чем
        /// eps на перпендикулярной линии.
        /// Будет проверено заданное количество точек (ограничение), при этом расстояние до контура
        /// граница будет уменьшаться на каждой итерации (с коэффициентом 1/2 ^ i, i = 1 ... limit).
        /// </remarks>
        public Point FindInteriorPoint(int limit = 5, double eps = 2e-5)
        {
            if (convex)
            {
                int count = this.Points.Count;

                var point = new Point(0.0, 0.0);

                for (int i = 0; i < count; i++)
                {
                    point.x += this.Points[i].x;
                    point.y += this.Points[i].y;
                }

                // If the contour is convex, use its Centroid.
                point.x /= count;
                point.y /= count;

                return point;
            }

            return FindPointInPolygon(this.Points, limit, eps);
        }

        private void AddPoints(IEnumerable<Vertex> points)
        {
            this.Points = new List<Vertex>(points);

            int count = Points.Count - 1;

            // Check if first vertex equals last vertex.
            if (Points[0] == Points[count])
            {
                Points.RemoveAt(count);
            }
        }

        #region Helper methods
        /// <summary>
        /// Найди точку на полигоне
        /// </summary>
        /// <param name="contour">Контур</param>
        /// <param name="limit">кол-во итераций</param>
        /// <param name="eps">точность</param>
        /// <returns></returns>
        public static Point FindPointInPolygon(List<Vertex> contour, int limit, double eps)
        {
            var bounds = new Rectangle();
            bounds.Expand(contour);

            int length = contour.Count;

            var test = new Point();

            Point a, b, c; // Current corner points.

            double bx, by;
            double dx, dy;
            double h;

            var predicates = new RobustPredicates();

            a = contour[0];
            b = contour[1];

            for (int i = 0; i < length; i++)
            {
                c = contour[(i + 2) % length];

                // Corner point.
                bx = b.x;
                by = b.y;

                // NOTE: if we knew the contour points were in counterclockwise order, we
                // could skip concave corners and search only in one direction.
                // ПРИМЕЧАНИЕ: если бы мы знали, что точки контура расположены в порядке против часовой стрелки,
                // мы могли бы пропустить вогнутые углы и выполнять поиск только в одном направлении.

                h = predicates.CounterClockwise(a, b, c);

                if (Math.Abs(h) < eps)
                {
                    // Points are nearly co-linear. Use perpendicular direction.
                    // Точки почти линейны. Используйте перпендикулярное направление.
                    dx = (c.y - a.y) / 2;
                    dy = (a.x - c.x) / 2;
                }
                else
                {
                    // Direction [midpoint(a-c) -> corner point]
                    // Направление [средняя точка(a-c) -> угловая точка]
                    dx = (a.x + c.x) / 2 - bx;
                    dy = (a.y + c.y) / 2 - by;
                }

                // Move around the contour.
                // Перемещение по контуру.
                a = b;
                b = c;

                h = 1.0;

                for (int j = 0; j < limit; j++)
                {
                    // Search in direction.
                    // Поиск в направлении.
                    test.x = bx + dx * h;
                    test.y = by + dy * h;

                    if (bounds.Contains(test) && IsPointInPolygon(test, contour))
                    {
                        return test;
                    }

                    // Search in opposite direction (see NOTE above).
                    // Поиск в противоположном направлении (см. Примечание выше).
                    test.x = bx - dx * h;
                    test.y = by - dy * h;

                    if (bounds.Contains(test) && IsPointInPolygon(test, contour))
                    {
                        return test;
                    }

                    h = h / 2;
                }
            }

            throw new Exception();
        }

        /// <summary>
        /// Return true if the given point is inside the polygon, or false if it is not.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <param name="poly">The polygon (list of contour points).</param>
        /// <returns></returns>
        /// <remarks>
        /// WARNING: If the point is exactly on the edge of the polygon, then the function
        /// may return true or false.
        /// 
        /// See http://alienryderflex.com/polygon/
        /// </remarks>

        /// <краткое описание>
        /// Возвращает true, если заданная точка находится внутри многоугольника, или false, если это не так.
        /// </краткое содержание>
        /// <имя параметра="точка">Точка для проверки.</параметр>
        /// <имя параметра="poly">Многоугольник (список точек контура).</параметр>
        /// <возвращает></возвращает>
        /// <примечания>
        /// ПРЕДУПРЕЖДЕНИЕ: Если точка находится точно на краю многоугольника, то функция
        /// может возвращать значение true или false.
        /// </примечания>
        private static bool IsPointInPolygon(Point point, List<Vertex> poly)
        {
            bool inside = false;
            double x = point.x;
            double y = point.y;
            int count = poly.Count;
            for (int i = 0, j = count - 1; i < count; i++)
            {
                if (((poly[i].y < y && poly[j].y >= y) || (poly[j].y < y && poly[i].y >= y)) &&
                     (poly[i].x <= x || poly[j].x <= x))
                {
                    inside ^= (poly[i].x + (y - poly[i].y) / (poly[j].y - poly[i].y) * (poly[j].x - poly[i].x) < x);
                }
                j = i;
            }
            return inside;
        }
        #endregion
    }

}
