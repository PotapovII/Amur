// -----------------------------------------------------------------------
// <copyright file="Polygon.cs" company="">
// Triangle.NET code by Christian Woltering, http://triangle.codeplex.com/
// </copyright>
// -----------------------------------------------------------------------

namespace TriangleNet.Geometry
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A polygon represented as a planar straight line graph.
    /// </summary>
    public class Polygon : IPolygon
    {
        /// <summary>
        /// Вершины области
        /// </summary>
        List<Vertex> points;
        /// <summary>
        /// Маркеры дырок для внутренних контуров
        /// </summary>
        List<Point> holes;
        /// <summary>
        /// Под/Области с границами
        /// </summary>
        List<RegionPointer> regions;
        /// <summary>
        /// Границы области внешнии и внутрение
        /// </summary>
        List<ISegment> segments;

        /// <inheritdoc />
        public List<Vertex> Points
        {
            get { return points; }
        }
        /// <inheritdoc />
        public List<Point> Holes
        {
            get { return holes; }
        }
        /// <inheritdoc />
        public List<RegionPointer> Regions
        {
            get { return regions; }
        }

        /// <inheritdoc />
        public List<ISegment> Segments
        {
            get { return segments; }
        }

        /// <inheritdoc />
        public bool HasPointMarkers { get; set; }

        /// <inheritdoc />
        public bool HasSegmentMarkers { get; set; }

        /// <inheritdoc />
        public int Count
        {
            get { return points.Count; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Polygon" /> class.
        /// </summary>
        public Polygon()
            : this(3, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Polygon" /> class.
        /// </summary>
        /// <param name="capacity">The default capacity for the points list.</param>
        public Polygon(int capacity)
            : this(3, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Polygon" /> class.
        /// Инициализирует новый экземпляр
        /// </summary>
        /// <param name="capacity">The default capacity for the points list.</param>
        /// <param name="markers">Use point and segment markers.</param>
        public Polygon(int capacity, bool markers)
        {
            points = new List<Vertex>(capacity);
            holes = new List<Point>();
            regions = new List<RegionPointer>();

            segments = new List<ISegment>();

            HasPointMarkers = markers;
            HasSegmentMarkers = markers;
        }
        /// <summary>
        /// Конструктор копирования
        /// </summary>
        /// <param name="p"></param>
        public Polygon(Polygon p)
        {
            points = new List<Vertex>();
            holes = new List<Point>();
            regions = new List<RegionPointer>();
            segments = new List<ISegment>();
            if(p.points != null)
                points.AddRange(p.points);
            if(p.holes != null)
                holes.AddRange(p.holes);
            if(p.regions != null)
                regions.AddRange(p.regions);
            if(p.segments != null)
                segments.AddRange(p.segments);

            HasPointMarkers = p.HasPointMarkers;
            HasSegmentMarkers = p.HasSegmentMarkers;
        }

        public Polygon(IPolygon p) : this((Polygon)p) { }



        [Obsolete("Use polygon.Add(contour) method instead.")]
        public void AddContour(IEnumerable<Vertex> points, int marker = 0,
            bool hole = false, bool convex = false)
        {
            this.Add(new Contour(points, marker, convex), hole);
        }

        [Obsolete("Use polygon.Add(contour) method instead.")]
        public void AddContour(IEnumerable<Vertex> points, int marker, Point hole)
        {
            this.Add(new Contour(points, marker), hole);
        }
        /// <summary>
        /// Вычислите границы многоугольника.
        /// </summary>
        /// <returns></returns>
        public Rectangle Bounds()
        {
            var bounds = new Rectangle();
            bounds.Expand(this.points);
            return bounds;
        }

        /// <summary>
        /// Add a vertex to the polygon.
        /// </summary>
        /// <param name="vertex">The vertex to insert.</param>
        public void Add(Vertex vertex)
        {
            this.points.Add(vertex);
        }

        /// <summary>
        /// Add a segment to the polygon.
        /// </summary>
        /// <param name="segment">The segment to insert.</param>
        /// <param name="insert">If true, both endpoints will be added to the points list.</param>
        /// Добавляем сегмент к многоугольнику.
        /// <param name="segment">Сегмент для вставки.</param>
        /// <param name="insert">Если true, обе конечные точки будут добавлены в список точек.</param>
        public void Add(ISegment segment, bool insert = false)
        {
            this.segments.Add(segment);

            if (insert)
            {
                this.points.Add(segment.GetVertex(0));
                this.points.Add(segment.GetVertex(1));
            }
        }

        /// <summary>
        /// Add a segment to the polygon.
        /// </summary>
        /// <param name="segment">The segment to insert.</param>
        /// <param name="index">The index of the segment endpoint to add to the points list (must be 0 or 1).</param>
        /// Добавляем сегмент к многоугольнику.
        /// <param name="segment">Сегмент для вставки.</param>
        /// <param name="index">Индекс конечной точки сегмента, добавляемого в список точек (должен быть 0 или 1).</param>
        public void Add(ISegment segment, int index)
        {
            this.segments.Add(segment);

            this.points.Add(segment.GetVertex(index));
        }

        /// <summary>
        /// Add a contour to the polygon.
        /// </summary>
        /// <param name="contour">The contour to insert.</param>
        /// <param name="hole">Treat contour as a hole.</param>
        public void Add(Contour contour, bool hole = false)
        {
            if (hole)
            {
                this.Add(contour, contour.FindInteriorPoint());
            }
            else
            {
                this.points.AddRange(contour.Points);
                this.segments.AddRange(contour.GetSegments());
            }
        }

        /// <summary>
        /// Add a contour to the polygon.
        /// </summary>
        /// <param name="contour">The contour to insert.</param>
        /// <param name="hole">Point inside the contour, making it a hole.</param>
        public void Add(Contour contour, Point hole)
        {
            this.points.AddRange(contour.Points);
            this.segments.AddRange(contour.GetSegments());

            this.holes.Add(hole);
        }
    }
}
