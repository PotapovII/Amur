namespace TestDelaunayGenerator.Boundary
{
    using System;
    using CommonLib.Geometry;
    /// <summary>
    /// Является генератором границы области с фиксированным количеством точек на каждом ребре
    /// </summary>
    public class GeneratorFixed : IGeneratorBase
    {
        /// <summary>
        /// Фиксированное количество точек на каждой границе
        /// </summary>
        protected int fixedByEdge = 0;
        /// <summary>
        /// Генератор границы с фиксированным количеством точек на каждом ребре
        /// </summary>
        /// <param name="fixedByEdge">количество точек на каждой границе, больше 0</param>
        /// <exception cref="ArgumentException"></exception>
        public GeneratorFixed(int fixedByEdge = 30)
        {
            if (fixedByEdge < 0)
                throw new ArgumentException($"недопустимое значение Для {nameof(fixedByEdge)} ({fixedByEdge})");
            this.fixedByEdge = fixedByEdge;
        }

        /// <summary>
        /// Сгенерировать границу для заданной области
        /// </summary>
        /// <param name="boundary">область, в которой требуется сгенерировать границу</param>
        /// <returns>Множество граничных точек, с учетом вершин области</returns>
        public IHPoint[] Generate(BoundaryHill boundary)
        {
            //количество ребер по сути равно количеству вершин.
            //Общее количество точек на всех границах,
            //т.е. сумма точек на всех ребрах + сами вершины
            int allPointsAmount = boundary.Vertexes.Length * (1 + fixedByEdge);
            IHPoint[] boundaryPoints = new IHPoint[allPointsAmount];

            int boundLength = boundary.Vertexes.Length;
            int curId = 0;
            for (int i = 0; i < boundLength; i++)
            {
                //добавляем начальное ребро
                boundaryPoints[curId % boundaryPoints.Length] = boundary.Vertexes[i % boundLength];
                curId++;

                //получаем длину ребра, образованного двумя вершинами
                (IHPoint v1, IHPoint v2) =
                    (boundary.Vertexes[i % boundLength], boundary.Vertexes[(i + 1) % boundLength]);
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

            return boundaryPoints;
        }
    }

}
