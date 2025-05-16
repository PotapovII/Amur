namespace TestDelaunayGenerator.Areas
{
    using System;
    using CommonLib.Geometry;

    /// <summary>
    /// Генератор обычной квадратной сетки
    /// </summary>
    public class GridArea : AreaBase
    {
        double maxValue;
        public GridArea(int count = 10_000, double maxValue = 1)
            :base("Сетка", count)
        {
            this.maxValue = maxValue;
            Initialize();
        }

        public override void Initialize()
        {
            if (Points != null)
                return;
            // массивы для псевдослучайного микро смещения координат узлов
            double[] dxx = {0.00000001, 0.00000005, 0.00000002, 0.00000006, 0.00000002,
                            0.00000007, 0.00000003, 0.00000001, 0.00000004, 0.00000009,
                            0.00000000, 0.00000003, 0.00000006, 0.00000004, 0.00000008 };
            double[] dyy = { 0.00000005, 0.00000002, 0.00000006, 0.00000002, 0.00000004,
                             0.00000007, 0.00000003, 0.00000001, 0.00000001, 0.00000004,
                             0.00000009, 0.00000000, 0.00000003, 0.00000006,  0.00000008 };
            int idd = 0;

            int sqrtCnt = (int)Math.Sqrt(Count);
            double coordOffset = maxValue / sqrtCnt;
            Points = new IHPoint[sqrtCnt*sqrtCnt];
            int indexer = 0;
            for (int i = 0; i < sqrtCnt; i++)
                for (int j = 0; j < sqrtCnt; j++)
                {
                    Points[indexer++] = new HPoint(i * coordOffset + dxx[idd], j * coordOffset + dyy[idd]);
                    idd++;
                    idd = idd % dxx.Length;
                }
            IHPoint[] variable;
            if (BoundaryContainer != null)
                variable = BoundaryContainer.AllBoundaryKnots;
        }
    }
}
