using CommonLib;
using MeshLib;
using TestDelaunayGenerator.Areas;

namespace TestDelaunayGenerator
{
    /// <summary>
    /// Запись для логов триангуляции
    /// </summary>
    public class TriangulationLog
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="area">исходное область, может содержать границуё</param>
        /// <param name="secondsFilterPoints">Время предварительной фильтрации</param>
        /// <param name="secondsGenerate">Время генерации триангуляции Делоне</param>
        /// <param name="secondsFilter">Время фильтрации треугольников</param>
        /// <param name="usePointFilter">true - используется только метод заражения треугольников. Фильтрация точек отключена</param>
        public TriangulationLog(AreaBase area, IMesh mesh, double secondsFilterPoints, double secondsGenerate, double secondsFilter, bool usePointFilter = true)
        {
            PointsBaseBeforeCnt = area.Points.Length;
            HasBoundary = !(area.BoundaryContainer is null);
            PointsBoundaryCnt = 0;
            if (HasBoundary)
            {
                PointsBoundaryCnt = area.BoundaryContainer.AllBoundaryKnots.Length;
                this.BoundaryCount = area.BoundaryContainer.Count;
                foreach (var boundary in area.BoundaryContainer)
                    BoundaryVertexCnt += boundary.Vertexes.Length;
            }
            AreaType = area.Name;

            this.TrianglesCnt = mesh.CountElements;
            this.PointsBaseAfterCnt = mesh.CountKnots - PointsBoundaryCnt;

            this.SecondsFilterPoints = secondsFilterPoints;
            this.SecondsGenerate = secondsGenerate;
            this.SecondsFilter = secondsFilter;
            this.UsePointFilter = usePointFilter;
        }

        /// <summary>
        /// количество исходных узлов
        /// </summary>
        public int PointsBaseBeforeCnt { get; set; }
        
        /// <summary>
        /// количество граничных узлов
        /// </summary>
        public int PointsBoundaryCnt { get; set; } = 0;

        /// <summary>
        /// Количество ограниченных областей
        /// </summary>
        public int BoundaryCount { get; set; }

        /// <summary>
        /// Общее количество вершин, образующих ограниченные области
        /// </summary>
        public int BoundaryVertexCnt { get; set; } = 0;

        /// <summary>
        /// Количество исходных узлов и количество граничных узлов до генерации сетки
        /// </summary>
        public int PointsBeforeCnt => PointsBaseBeforeCnt + PointsBoundaryCnt;

        /// <summary>
        /// Количество узлов, оставшихся после генерации, за вычетом граничных узлов
        /// </summary>
        public int PointsBaseAfterCnt { get; set; }


        /// <summary>
        /// Общее количество узлов, оставшихся после генерации с учетом граничных узлов
        /// </summary>
        public int PointsAfterCnt => PointsBaseAfterCnt + PointsBoundaryCnt;

        /// <summary>
        /// Количество треугольников
        /// </summary>
        public int TrianglesCnt { get; set; }

        /// <summary>
        /// имеется ли граница. true - граница задана
        /// </summary>
        public bool HasBoundary { get; set; } = false;

        /// <summary>
        /// Тип области
        /// </summary>
        public string AreaType { get;set; }

        /// <summary>
        /// Время предварительной фильтрации
        /// </summary>
        public double SecondsFilterPoints { get; set; } = 0;

        /// <summary>
        /// Время генерации триангуляции
        /// </summary>
        public double SecondsGenerate { get; set; }

        /// <summary>
        /// Время фильтрации треугольников
        /// </summary>
        public double SecondsFilter { get; set; }

        /// <summary>
        /// Общее затраченное время
        /// </summary>
        public double SecondsTotal => SecondsFilterPoints + SecondsGenerate + SecondsFilter;


        /// <summary>
        /// True - использовать предварительную фильтрацию исходного множества точек,
        /// оставляя только те, что гарантированно войдут в триангуляцию Делоне
        /// </summary>
        public bool UsePointFilter { get; set; } = true;

        public override string ToString()
        {
            string s = $"Область:{AreaType}; " +
            $"граница:{HasBoundary}; " +
            $"колво граница(шт):{PointsBoundaryCnt}; " +
            $"колво огран областей(шт):{BoundaryCount}; " +
            $"колво вершин огран областей(шт):{BoundaryVertexCnt}; " +
            $"узлы до(шт):{PointsBaseBeforeCnt}; " +
            $"общее до(шт):{PointsBeforeCnt}; " +
            $"узлы после(шт):{PointsBaseAfterCnt}; " +
            $"общее после(шт):{PointsAfterCnt}; " +
            $"треугольники(шт):{TrianglesCnt}; " +
            $"предв. фильтр.(сек):{SecondsFilterPoints}; " +
            $"генерация(сек):{SecondsGenerate}; " +
            $"фильтрация(сек):{SecondsFilter}; " +
            $"общее(сек):{SecondsTotal};" +
            $"предварительный фильтр:{UsePointFilter}";
            return s;
        }
    }
}
