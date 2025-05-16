using CommonLib.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDelaunayGenerator.Boundary
{
    /// <summary>
    /// Контейнер-генератор множества областей, представляющих границу
    /// </summary>
    public class BoundaryContainer
    {
        List<BoundaryHill> boundaries = new List<BoundaryHill>();
        public BoundaryHill this[int boundId] => boundaries[boundId];
        /// <summary>
        /// Количество границ
        /// </summary>
        public int Count => boundaries.Count;
        /// <summary>
        /// Все граничные узлы
        /// </summary>
        private IHPoint[] _allBounaryKnots = null;
        /// <summary>
        /// Получить все граничные точки
        /// </summary>
        public IHPoint[] AllBoundaryKnots
        {
            get
            {
                if (_allBounaryKnots == null)
                    this.IntializeContainer();
                return _allBounaryKnots;
            }
        }
        protected readonly IGeneratorBase generator;

        /// <summary>
        /// Контейнер для границ области триангуляции
        /// </summary>
        /// <param name="basePoints">точки области триангуляции</param>
        public BoundaryContainer(IGeneratorBase generator)
        {
            this.generator = generator;
        }
        /// <summary>
        /// Смещение по количеству узлов в общем массиве узлов для конкретной границы. <br/>
        /// Для первой границы смещение будет 0, для 2-ой границы смещение будет 0 + количество узлов в первой границе и т.д.
        /// </summary>
        /// <param name="boundId"></param>
        /// <returns></returns>
        public int GetBoundaryOffset(int boundId)
        {
            if (boundId > boundaries.Count - 1 || boundId < 0)
                throw new ArgumentException($"{nameof(boundId)} вышел за пределы индексации.");

            int offset = 0;
            for (int i = 0; i < boundId; i++)
            {
                offset += boundaries[i].Length;
            }
            return offset;
        }


        public void Add(IHPoint[] vertexes)
        {
            BoundaryHill boundary = new BoundaryHill(vertexes, generator);
            boundaries.Add(boundary);
        }
        //генератор
        public IEnumerator<BoundaryHill> GetEnumerator()
        {
            for (int i = 0; i < boundaries.Count; i++)
            {
                yield return boundaries[i];
            }
        }
        /// <summary>
        /// Получить индекс границы, которой принадлежит точка
        /// </summary>
        /// <param name="point"></param>
        /// <returns>-1 если точка не принадлежит границе</returns>
        public int BoundaryIndex(IHPoint point)
        {
            for (int i = 0; i < boundaries.Count; ++i)
            {
                if (Array.IndexOf(boundaries[i].Points, point) != -1)
                    return i;
            }
            return -1;

        }
        protected void IntializeContainer()
        {
            List<IHPoint> boundaryPoints = new List<IHPoint>(2);
            foreach (BoundaryHill boundary in boundaries)
            {
                boundary.Initialize(generator);
                boundaryPoints.AddRange(boundary.Points);
            }
            _allBounaryKnots = boundaryPoints.ToArray();
        }
    }

}
