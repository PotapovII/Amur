namespace GeometryLib.Geometry
{
    using System;
    using System.Collections.Generic;

    using CommonLib.Areas;
    using CommonLib.Geometry;
    using GeometryLib.Predicates;
    /// <summary>
    /// Контур области
    /// </summary>
    public class HContour
    {
        /// <summary>
        /// Тип контура 0 - оболочка 1,3,4,... - дырки
        /// </summary>
        protected int typeHill;
        /// <summary>
        /// Флаг типа контура = true если контур выпуклый.
        /// </summary>
        protected  bool convexHill;
        /// <summary>
        /// Получает или задает список точек, составляющих контур.
        /// </summary>
        public List<HKnot> Points { get; set; }
        /// <summary>
        /// Инициализирует новый экземпляр класса 
        /// </summary>
        /// <param name="points">Точки, составляющие контур</param>
        /// <param name="convexHill">Флаг типа контура = true если контур выпуклый.</param>
        /// <param name="typeHill">Тип контура 0 - оболочка 1,3,4,... - дырки</param>
        public HContour(IEnumerable<HKnot> points, bool convexHill,int typeHill = 0)
        {
            AddPoints(points);
            this.convexHill = convexHill;
            this.typeHill = typeHill;
        }
        /// <summary>
        /// Инициализирует новый экземпляр класса с новой маркировкой граней
        /// </summary>
        /// <param name="points"></param>
        /// <param name="Markers"></param>
        /// <param name="convexHill"></param>
        /// <param name="typeHill"></param>
        public HContour(List<HKnot> points, List<int> Markers, bool convexHill = true, int typeHill = 0)
        {
            AddPoints(points);
            this.convexHill = convexHill;
            this.typeHill = typeHill;
            for (int i = 0; i < Points.Count; i++)
                Points[i].marker = Markers[i];
        }
        public HContour(HContour co)
        {
            AddPoints(co.Points);
            this.convexHill = co.convexHill;
            this.typeHill = co.typeHill;
        }
        public List<IHEdge> GetSegments()
        {
            var segments = new List<IHEdge>();
            var p = this.Points;
            int count = p.Count - 1;
            for (int i = 0; i < count; i++)
            {
                // Добавьте сегменты к полигону.
                segments.Add(new HEdge(p[i].marker, p[i], p[i + 1]));
            }
            // Закройте контур.
            segments.Add(new HEdge(p[count].marker, p[count], p[0]));
            return segments;
        }
        /// <summary>
        /// Добавить список точек
        /// </summary>
        /// <param name="points"></param>
        private void AddPoints(IEnumerable<HKnot> points)
        {
            this.Points = new List<HKnot>(points);
            int count = Points.Count - 1;
            // Проверьте, равна ли первая вершина последней вершине.
            if (Points[0] == Points[count])
            {
                Points.RemoveAt(count);
            }
        }
        /// <summary>
        /// Возвращает true, если заданная точка находится 
        /// внутри многоугольника, или false, если это не так.
        /// </summary>
        /// <param name="point">Точка для проверки.</param>
        /// <param name="poly">Многоугольник (список точек контура).</param>
        /// ПРЕДУПРЕЖДЕНИЕ: Если точка находится точно на краю многоугольника, то функция
        /// может возвращать значение true или false.
        /// <returns></returns>
        public bool Contains(double x, double y)
        {
            bool inside = false;
            int count = Points.Count;
            for (int i = 0, j = count - 1; i < count; i++)
            {
                if (((Points[i].y < y && Points[j].y >= y) ||
                     (Points[j].y < y && Points[i].y >= y)) &&
                     (Points[i].x <= x || Points[j].x <= x))
                {
                    inside ^= (Points[i].x + (y - Points[i].y) / (Points[j].y - Points[i].y) * (Points[j].x - Points[i].x) < x);
                }
                j = i;
            }
            return inside;
        }
        /// <summary>
        /// Проверьте, находится ли заданная точка внутри прямоугольника.
        /// </summary>
        public bool Contains(IHPoint pt)
        {
            return Contains(pt.X, pt.Y);
        }
    }

    /// <summary>
    /// Контур области
    /// </summary>
    public class LContour : HContour
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HContour" /> class.
        /// </summary>
        /// <param name="points">The points that make up the contour.</param>
        /// <param name="marker">HContour marker.</param>
        /// <param name="convexHill">The hole is convexHill.</param>
        /// <краткое описание>
        /// Инициализирует новый экземпляр класса <см. cref="HContour" />.
        /// </краткое содержание>
        /// <имя параметра="точки">Точки, составляющие контур.</параметр>
        /// <имя параметра="маркер">Контурный маркер.</параметр>
        /// <имя параметра="выпуклый">Отверстие выпуклое.</параметр>
        public LContour(IEnumerable<HKnot> points, bool convexHill, int marker) :
            base(points, convexHill, marker)
        { }

        /// <summary>
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
        public HPoint FindInteriorPoint(int limit = 5, double eps = 2e-5)
        {
            if (convexHill == true)
            {
                int count = this.Points.Count;
                var point = new HPoint(0.0, 0.0);
                for (int i = 0; i < count; i++)
                {
                    point.x += this.Points[i].x;
                    point.y += this.Points[i].y;
                }
                // Если контур выпуклый, используйте его центр тяжести.
                point.x /= count;
                point.y /= count;
                return point;
            }
            return FindPointInPolygon(this.Points, limit, eps);
        }
        #region Helper methods
        /// <summary>
        /// Найди точку на полигоне
        /// </summary>
        /// <param name="contour">Контур</param>
        /// <param name="limit">кол-во итераций</param>
        /// <param name="eps">точность</param>
        /// <returns></returns>
        public static HPoint FindPointInPolygon(List<HKnot> contour, int limit, double eps)
        {
            var bounds = new HQuad();
            bounds.Extension(contour);

            int length = contour.Count;

            var test = new HPoint();

            HPoint a, b, c; // Текущие угловые точки.

            double bx, by;
            double dx, dy;
            double h;

            var predicates = new HPredicatesDelone();

            a = contour[0];
            b = contour[1];

            for (int i = 0; i < length; i++)
            {
                c = contour[(i + 2) % length];

                // Corner point.
                bx = b.x;
                by = b.y;

                // ПРИМЕЧАНИЕ: если бы мы знали, что точки контура расположены в порядке против часовой стрелки,
                // мы могли бы пропустить вогнутые углы и выполнять поиск только в одном направлении.
                h = predicates.CounterClockwise(a, b, c);

                if (Math.Abs(h) < eps)
                {
                    // Точки почти линейны. Используйте перпендикулярное направление.
                    dx = (c.y - a.y) / 2;
                    dy = (a.x - c.x) / 2;
                }
                else
                {
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
                    // Поиск в направлении.
                    test.x = bx + dx * h;
                    test.y = by + dy * h;

                    if (bounds.Contains(test) && IsPointInPolygon(test, contour))
                    {
                        return test;
                    }
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
        /// Возвращает true, если заданная точка находится 
        /// внутри многоугольника, или false, если это не так.
        /// </summary>
        /// <param name="point">Точка для проверки.</param>
        /// <param name="poly">Многоугольник (список точек контура).</param>
        /// ПРЕДУПРЕЖДЕНИЕ: Если точка находится точно на краю многоугольника, то функция
        /// может возвращать значение true или false.
        /// <returns></returns>
        private static bool IsPointInPolygon(IHPoint point, List<HKnot> poly)
        {
            bool inside = false;
            double x = point.X;
            double y = point.Y;
            int count = poly.Count;
            for (int i = 0, j = count - 1; i < count; i++)
            {
                if (((poly[i].y < y && poly[j].y >= y) || 
                     (poly[j].y < y && poly[i].y >= y)) &&
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
