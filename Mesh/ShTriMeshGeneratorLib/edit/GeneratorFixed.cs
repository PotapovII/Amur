namespace ShTriMeshGeneratorLib
{
    using System;
    using CommonLib.Geometry;
    /// <summary>
    /// Является генератором границы области с фиксированным количеством точек на каждом ребре
    /// </summary>
    public class GeneratorFixed : IGeneratorBase
    {
        protected IHPoint[] points;
        public IHPoint[] Points => points;

        protected int[] vertexIds;
        public int[] BaseVertexIds() => vertexIds;
        /// <summary>
        /// Фиксированное количество точек на каждой границе
        /// </summary>
        protected int fixedByEdge = 0;
        /// <summary>
        /// Генератор границы с фиксированным количеством точек на каждом ребре
        /// </summary>
        /// <param name="fixedByEdge">количество точек на каждой границе, больше 0</param>
        /// <exception cref="ArgumentException"></exception>
        public GeneratorFixed(int fixedByEdge = 5)
        {
            if (fixedByEdge < 0)
                throw new ArgumentException($"недопустимое значение Для " +
                                 $"{nameof(fixedByEdge)} ({fixedByEdge})");
            this.fixedByEdge = fixedByEdge;
        }
        /// <summary>
        /// Сгенерировать границу для заданной области
        /// </summary>
        /// <param name="boundary">область, в которой требуется сгенерировать границу</param>
        /// <returns>Множество граничных точек, с учетом вершин области</returns>
        public IHPoint[] Generate(BorderTmp boundary)
        {
            //количество ребер по сути равно количеству вершин.
            //Общее количество точек на всех границах,
            //т.е. сумма точек на всех ребрах + сами вершины
            int allPointsAmount = boundary.BaseVertexes.Length * (1 + fixedByEdge);
            IHPoint[] boundaryPoints = new IHPoint[allPointsAmount];

            //количество опорных вершин
            int boundLength = boundary.BaseVertexes.Length;
            //текущий свободный индекс
            int curId = 0;
            //инициализация массива индексов опорных вершин
            vertexIds = new int[boundLength];
            for (int i = 0; i < boundLength; i++)
            {
                //сохраняем индекс опорной вершины
                vertexIds[i] = curId;
                //добавляем начальное ребро
                boundaryPoints[curId % boundaryPoints.Length] = boundary.BaseVertexes[i % boundLength];
                curId++;

                //получаем длину ребра, образованного двумя вершинами
                (IHPoint v1, IHPoint v2) =
                    (boundary.BaseVertexes[i % boundLength], boundary.BaseVertexes[(i + 1) % boundLength]);
                int pointsByEdge = (this.fixedByEdge + 1);
                double intervalX = (v2.X - v1.X) / pointsByEdge;
                double intervalY = (v2.Y - v1.Y) / pointsByEdge;
                //добавляем точки в промежутке между вершинами ребер
                for (int j = 1; j <= fixedByEdge; j++)
                {
                    boundaryPoints[curId] = new HPoint(v1.X + intervalX * j, v1.Y + intervalY * j);
                    curId++;
                }
            }

            points = boundaryPoints;
            return points;
        }
    }
}
