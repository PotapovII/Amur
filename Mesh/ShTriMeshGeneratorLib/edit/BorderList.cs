namespace ShTriMeshGeneratorLib
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    using CommonLib.Geometry;

    // TO DO Сделать общий контейнер границ !!!
    /// <summary>
    /// Контейнер для границ области. 
    /// Хранит и управляет внешней и внутренними границами 
    /// </summary>
    public class BorderList
    {
        /// <summary>
        /// Внешняя граница
        /// </summary>
        public BorderTmp OuterBoundary;
        /// <summary>
        /// Внутренние границы
        /// </summary>
        public List<BorderTmp> InnerBoundaries = new List<BorderTmp>();
        /// <summary>
        /// Все граничные точки
        /// </summary>
        protected IHPoint[] allBoundaryPoints;
        // TODO удалить/изменить
        public int Count => 1 + this.InnerBoundaries.Count;
        /// <summary>
        /// Все граничные точки
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public IHPoint[] AllBoundaryPoints
        {
            get
            {
                //если массив ранее был инициализирован, то возвращаем его
                if (this.allBoundaryPoints != null)
                    return this.allBoundaryPoints;
                //если внешней границы не задано, то поднимаем исключение
                if (this.OuterBoundary is null)
                    throw new ArgumentNullException($"Внешняя граница не задана");
                //количество всех точек
                int pointsCnt = this.OuterBoundary.Points.Length +
                    InnerBoundaries.Sum(x => x.Points.Length);
                // заполняем массив точек
                allBoundaryPoints = new IHPoint[pointsCnt];
                // точки внешней границы
                OuterBoundary.Points.CopyTo(allBoundaryPoints, 0);
                // точки внутренних границ
                if (this.InnerBoundaries.Count > 0)
                {
                    //текущее смещение по точкам
                    //учитываем уже размещенные количество точек внешней границы
                    int pointsOffset = this.OuterBoundary.Points.Length;
                    foreach (var innerBound in InnerBoundaries)
                    {
                        innerBound.Points.CopyTo(allBoundaryPoints, pointsOffset);
                        pointsOffset += innerBound.Points.Length;
                    }
                }

                //возврат массива точек
                return this.allBoundaryPoints;
            }
        }

        //TODO поправить индексацию, а также индексацию в методе GetBoundaryOffset
        public BorderTmp this[int boundId]
        {
            get
            {
                if (boundId == 0)
                    return this.OuterBoundary;

                if (boundId - 1 > this.InnerBoundaries.Count - 1)
                    throw new ArgumentException($"{nameof(boundId)} вышел за пределены");

                return this.InnerBoundaries[boundId - 1];
            }
        }

        #region Добавление границ
        /// <summary>
        /// Добавить внутреннюю оболочку
        /// </summary>
        /// <param name="boundary">внутренняя граница</param>
        /// <exception cref="ArgumentNullException">boundary null</exception>
        public void AddInnerBoundary(BorderTmp boundary)
        {
            if (boundary is null)
                throw new ArgumentNullException($"{nameof(boundary)} не может быть null!");
            this.InnerBoundaries.Add(boundary);
        }

        /// <summary>
        /// Добавить внутреннюю оболочку на основе опорных вершин
        /// </summary>
        /// <param name="baseVertexes">опорные вершины, образующие форму границы</param>
        /// <param name="generator">правила генерации точек на ребрах границы</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddInnerBoundary(IHPoint[] baseVertexes, IGeneratorBase generator)
        {
            //проверка на null
            if (baseVertexes is null || baseVertexes.Length == 0)
                throw new ArgumentNullException($"{nameof(baseVertexes)} null или пуст");
            if (generator is null)
                throw new ArgumentNullException($"{nameof(generator)} не может быть null");


            //создаем объект границы и добавляем в коллекцию через метод
            var boundary = new BorderTmp(baseVertexes, generator);
            this.AddInnerBoundary(boundary);
        }


        /// <summary>
        /// Замена внешней границы
        /// </summary>
        /// <param name="boundary">внешняя граница</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void LReplaceOuterBoundary(BorderTmp boundary)
        {
            if (boundary is null)
                throw new ArgumentNullException($"{nameof(boundary)} не может быть null!");

            this.OuterBoundary = boundary;
        }

        /// <summary>
        /// Замена внешней границы
        /// </summary>
        /// <param name="baseVertexes">опорные вершины, образующие форму границы</param>
        /// <param name="generator">правила генерации точек на ребрах границы</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void ReplaceOuterBoundary(IHPoint[] baseVertexes, IGeneratorBase generator)
        {
            //проверка на null
            if (baseVertexes is null || baseVertexes.Length == 0)
                throw new ArgumentNullException($"{nameof(baseVertexes)} null или пуст");
            if (generator is null)
                throw new ArgumentNullException($"{nameof(generator)} не может быть null");

            //создаем объект границы и заменяем через метод
            var boundary = new BorderTmp(baseVertexes, generator);
            this.LReplaceOuterBoundary(boundary);
        }
        #endregion

        //генератор
        public IEnumerator<BorderTmp> GetEnumerator()
        {
            yield return this.OuterBoundary;
            for (int i = 0; i < this.InnerBoundaries.Count; i++)
            {
                yield return InnerBoundaries[i];
            }
        }




        /// <summary>
        /// Смещение по количеству узлов в общем массиве узлов для конкретной границы. <br/>
        /// Для первой границы смещение будет 0, для 2-ой границы смещение будет 0 + количество узлов в первой границе и т.д.
        /// </summary>
        /// <param name="boundId"></param>
        /// <returns></returns>
        public int GetBoundaryOffset(int boundId)
        {
            //смещение по внешней оболочке
            if (boundId == 0)
                return 0;

            //проверка на корректный индекс границы
            //для внутренних границ индексация начинается с 1
            if (1 + this.InnerBoundaries.Count - 1 < boundId || boundId < 0)
                throw new ArgumentException($"{nameof(boundId)} вышел за пределы индексации.");

            //смещение по внешней оболочке
            int offset = this.OuterBoundary.Points.Length;
            //отсчитываем по внутренних оболочкам
            for (int i = 0; i < boundId - 1; i++)
            {
                offset += this.InnerBoundaries[i].Points.Length;
            }
            return offset;
        }

        /// <summary>
        /// Проверяет, принадлежит ли ребро (start, end) какой-либо границе
        /// </summary>
        /// <param name="start">Индекс начальной точки ребра</param>
        /// <param name="end">Индекс конечной точки ребра</param>
        /// <param name="offset">Смещение индексов точек в общем массиве</param>
        /// <returns>True, если ребро принадлежит границе</returns>
        public bool IsBoundaryEdge(int start, int end, int offset)
        {
            foreach (var boundary in this)
            {
                foreach (var edge in boundary.BaseBoundaryEdges)
                {
                    int edgeStart = Array.IndexOf(boundary.Points, edge.A) + offset;
                    int edgeEnd = Array.IndexOf(boundary.Points, edge.B) + offset;
                    if ((start == edgeStart && end == edgeEnd) || (start == edgeEnd && end == edgeStart))
                        return true;
                }
                offset += boundary.Points.Length;
            }
            return false;
        }
    }

}
