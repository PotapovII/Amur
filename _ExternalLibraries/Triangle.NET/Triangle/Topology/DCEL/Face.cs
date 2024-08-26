// -----------------------------------------------------------------------
// <copyright file="Face.cs">
// Triangle.NET code by Christian Woltering, http://triangle.codeplex.com/
// </copyright>
// -----------------------------------------------------------------------

namespace TriangleNet.Topology.DCEL
{
    using System.Collections.Generic;
    using TriangleNet.Geometry;

    /// <summary>
    /// A face of DCEL mesh.
    /// Грань сетки DCEL.
    /// </summary>
    public class Face
    {
        #region Статическая инициализация грани сетки из внешнего пространства Static initialization of "Outer Space" face  

        public static readonly Face Empty;

        static Face()
        {
            Empty = new Face(null);
            Empty.id = -1;
        }

        #endregion
        /// <summary>
        /// идентификатор грани
        /// </summary>
        internal int id;
        /// <summary>
        /// метка грани
        /// </summary>
        internal int mark;
        /// <summary>
        /// Генератор этой грани (для диаграммы Вороного)
        /// </summary>
        internal Point generator;
        /// <summary>
        /// Полуребро, соединенное с гранью.
        /// </summary>
        internal HalfEdge edge;
        /// <summary>
        /// Флаг указывающий, ограничена ли грань (для диаграммы Вороного).
        /// </summary>
        internal bool bounded;

        /// <summary>
        /// Gets or sets the face id.
        /// Получает или задает идентификатор грани.
        /// </summary>
        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        /// <summary>
        /// Gets or sets a half-edge connected to the face.
        /// Получает или задает полуребро, соединенное с гранью.
        /// </summary>
        public HalfEdge Edge
        {
            get { return edge; }
            set { edge = value; }
        }

        /// <summary>
        /// Gets or sets a value, indicating if the face is bounded (for Voronoi diagram).
        /// Получает или задает значение, указывающее, ограничена ли грань (для диаграммы Вороного).
        /// </summary>
        public bool Bounded
        {
            get { return bounded; }
            set { bounded = value; }
        }

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="Face" /> class.
        /// </summary>
        /// <param name="generator">Генератор этой грани (для диаграммы Вороного)</param>
        public Face(Point generator)
            : this(generator, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Face" /> class.
        /// </summary>
        /// <param name="generator">Генератор этой грани (для диаграммы Вороного)</param>
        /// <param name="edge">Полуребро, соединенное с этой гранью.</param>
        public Face(Point generator, HalfEdge edge)
        {
            this.generator = generator;
            this.edge = edge;
            this.bounded = true;

            if (generator != null)
            {
                this.id = generator.ID;
            }
        }

        /// <summary>
        /// Enumerates all half-edges of the face boundary.
        /// Перечисляет все полуребра границы грани.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<HalfEdge> EnumerateEdges()
        {
            var edge = this.Edge;
            int first = edge.ID;

            do
            {
                yield return edge;

                edge = edge.Next;
            } while (edge.ID != first);
        }

        public override string ToString()
        {
            return string.Format("F-ID {0}", id);
        }
    }
}
