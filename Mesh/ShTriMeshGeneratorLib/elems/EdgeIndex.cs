namespace ShTriMeshGeneratorLib
{
    /// <summary>
    /// Структура вершины с адресами по типу двухстороннего списка 
    /// для замкнутого контура границ
    /// </summary>
    public struct EdgeIndex
    {
        /// <summary>
        /// Создание ребра на основе индексов вершин
        /// </summary>
        /// <param name="adjacent1">1-ая соседняя вершина</param>
        /// <param name="adjacent2">2-ая соседняя вершина</param>
        /// <param name="boundaryId">Индекс граничного контура (границы), которой принадлежит точка</param>
        public EdgeIndex(int PointID, int adjacent1, int adjacent2, int boundaryId = 0)
        {
            this.PointID = PointID;
            this.adjacent1 = adjacent1;
            this.adjacent2 = adjacent2;
            this.BoundaryID = boundaryId;
        }
        /// <summary>
        /// Индекс текущей вершины
        /// </summary>
        public int PointID;
        /// <summary>
        /// Индекс 1-ой соседней вершины с <see cref="PointID"/>
        /// </summary>
        public int adjacent1;
        /// <summary>
        /// Индекс 2-ой соседней вершины с <see cref="PointID"/>
        /// </summary>
        public int adjacent2;
        /// <summary>
        /// Проверка на индексы соседних вершин 
        /// </summary>
        public bool Contains(int edgeIdEnd)
        {
            return adjacent1 == edgeIdEnd || adjacent2 == edgeIdEnd;
        }
        /// <summary>
        /// Индекс граничного контура (границы), которой принадлежит <see cref="PointID"/>
        /// </summary>
        public int BoundaryID;
    }
}
