// -----------------------------------------------------------------------
// <copyright file="IPolygon.cs" company="">
// Triangle.NET code by Christian Woltering, http://triangle.codeplex.com/
// </copyright>
// -----------------------------------------------------------------------

namespace TriangleNet.Geometry
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Polygon interface.
    /// Интерфейс полигона
    /// </summary>
    public interface IPolygon
    {
        /// <summary>
        /// Gets the vertices of the polygon.
        /// Получает вершины многоугольника.
        /// </summary>
        List<Vertex> Points { get; }

        /// <summary>
        /// Gets the segments of the polygon.
        /// Получает сегменты многоугольника.
        /// </summary>
        List<ISegment> Segments { get; }

        /// <summary>
        /// Gets a list of points defining the holes of the polygon.
        /// Получает список точек, определяющих отверстия многоугольника.
        /// </summary>
        List<Point> Holes { get; }

        /// <summary>
        /// Gets a list of pointers defining the regions of the polygon.
        /// Получает список указателей, определяющих области многоугольника.
        /// </summary>
        List<RegionPointer> Regions { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the vertices have marks or not.
        /// Получает или задает значение, указывающее, есть ли у вершин метки или нет.
        /// </summary>
        bool HasPointMarkers { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the segments have marks or not.
        /// Получает или задает значение, указывающее, есть ли у сегментов метки или нет.
        /// </summary>
        bool HasSegmentMarkers { get; set; }

        // Вместо этого используйте метод polygon.Add(contour).
        [Obsolete("Use polygon.Add(contour) method instead.")]
        void AddContour(IEnumerable<Vertex> points, int marker, bool hole, bool convex);

        // Вместо этого используйте метод polygon.Add(contour).
        [Obsolete("Use polygon.Add(contour) method instead.")]
        void AddContour(IEnumerable<Vertex> points, int marker, Point hole);

        /// <summary>
        /// Compute the bounds of the polygon.
        /// Вычислите границы многоугольника.
        /// </summary>
        /// <returns>Rectangle defining an axis-aligned bounding box.</returns>
        Rectangle Bounds();

        /// <summary>
        /// Add a vertex to the polygon.
        /// Добавьте вершину в многоугольник.
        /// </summary>
        /// <param name="vertex">The vertex to insert.</param>
        void Add(Vertex vertex);

        /// <summary>
        /// Add a segment to the polygon.
        /// Добавьте сегмент к многоугольнику.
        /// </summary>
        /// <param name="segment">Сегмент для вставки. The segment to insert.</param>
        /// <param name="insert">If true, both endpoints will be added to the points list.
        /// Если это правда, обе конечные точки будут добавлены в список точек.</param>
        void Add(ISegment segment, bool insert = false);

        /// <summary>
        /// Add a segment to the polygon.
        /// </summary>
        /// <param name="segment">The segment to insert.</param>
        /// <param name="index">The index of the segment endpoint to add to the points list (must be 0 or 1).</param>
        void Add(ISegment segment, int index);

        /// <summary>
        /// Add a contour to the polygon.
        /// </summary>
        /// <param name="contour">The contour to insert.</param>
        /// <param name="hole">Treat contour as a hole.</param>
        void Add(Contour contour, bool hole = false);

        /// <summary>
        /// Add a contour to the polygon.
        /// </summary>
        /// <param name="contour">The contour to insert.</param>
        /// <param name="hole">Point inside the contour, making it a hole.</param>
        void Add(Contour contour, Point hole);
    }
}
