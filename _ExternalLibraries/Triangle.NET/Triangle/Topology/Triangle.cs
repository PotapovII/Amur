// -----------------------------------------------------------------------
// <copyright file="Triangle.cs" company="">
// Original Triangle code by Jonathan Richard Shewchuk, http://www.cs.cmu.edu/~quake/triangle.html
// Triangle.NET code by Christian Woltering, http://triangle.codeplex.com/
// </copyright>
// -----------------------------------------------------------------------

namespace TriangleNet.Topology
{
    using System;
    using TriangleNet.Geometry;

    /// <summary>
    /// Треугольник.
    /// </summary>
    public class Triangle : ITriangle
    {
        /// <summary>
        /// Хэш для словаря. Будет установлен экземпляром сетки.
        /// </summary>
        internal int hash;
        /// <summary>
        /// Идентификатор используется только для вывода сетки.
        /// </summary>
        internal int id;
        /// <summary>
        /// Инициализируем три ближайших сопряженных треугольника как «внешнее пространство».
        /// </summary>
        internal Otri[] neighbors;
        
        /// <summary>
        /// вершины треугольника
        /// </summary>
        public Vertex[] vertices;
        /// <summary>
        /// 
        /// </summary>
        internal Osub[] subsegs;
        internal int label;
        internal double area;
        /// <summary>
        /// Метрка на треугольнике
        /// </summary>
        internal bool infected;

        /// <summary>
        /// Инициализирует новый экземпляр класса Triangle
        /// </summary>
        public Triangle()
        {
            // Три NULL-вершины.
            vertices = new Vertex[3];

            // Инициализируйте три соседних подсегмента, чтобы они были вездесущими.
            subsegs = new Osub[3];

            // Initialize the three adjoining triangles to be "outer space".
            // Инициализируем три соседних треугольника как «внешнее пространство».
            neighbors = new Otri[3];

            // area = -1.0;
        }

        #region Public properties

        /// <summary>
        /// Gets or sets the triangle id.
        /// </summary>
        public int ID
        {
            get { return this.id; }
            set { this.id = value; }
        }

        /// <summary>
        /// Region ID the triangle belongs to.
        /// </summary>
        public int Label
        {
            get { return this.label; }
            set { this.label = value; }
        }

        /// <summary>
        /// Gets the triangle area constraint.
        /// </summary>
        public double Area
        {
            get { return this.area; }
            set { this.area = value; }
        }

        /// <summary>
        /// Gets the specified corners vertex.
        /// </summary>
        public Vertex GetVertex(int index)
        {
            return this.vertices[index]; // TODO: Check range?
        }

        public int GetVertexID(int index)
        {
            return this.vertices[index].id;
        }

        /// <summary>
        /// Gets a triangles' neighbor.
        /// </summary>
        /// <param name="index">The neighbor index (0, 1 or 2).</param>
        /// <returns>The neigbbor opposite of vertex with given index.</returns>
        public ITriangle GetNeighbor(int index)
        {
            return neighbors[index].tri.hash == MeshNet.DUMMY ? null : neighbors[index].tri;
        }

        /// <inheritdoc />
        public int GetNeighborID(int index)
        {
            return neighbors[index].tri.hash == MeshNet.DUMMY ? -1 : neighbors[index].tri.id;
        }

        /// <summary>
        /// Gets a triangles segment.
        /// </summary>
        /// <param name="index">The vertex index (0, 1 or 2).</param>
        /// <returns>The segment opposite of vertex with given index.</returns>
        public ISegment GetSegment(int index)
        {
            return subsegs[index].seg.hash == MeshNet.DUMMY ? null : subsegs[index].seg;
        }

        #endregion

        public override int GetHashCode()
        {
            return this.hash;
        }

        public override string ToString()
        {
            return String.Format("TID {0}", hash);
        }
    }
}
