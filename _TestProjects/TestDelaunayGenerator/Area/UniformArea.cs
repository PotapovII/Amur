namespace TestDelaunayGenerator.Areas
{
    using System;
    using CommonLib.Geometry;
    /// <summary>
    /// Равномерное распределение
    /// </summary>
    public class UniformArea : AreaBase
    {
        double min = 0;
        double max = 1;
        Random rnd = new Random();

        public UniformArea(string Name, int count = 10_000,
                double valueMin = 0, double valueMax = 1)
            : base(Name, count)
        {
            this.min = valueMin;
            this.max = valueMax;
        }
        public UniformArea(int count = 10_000, double valueMin = 0, double valueMax = 1)
            : this("Равномерное распределение", count, valueMin, valueMax)
        {
            Initialize();
        }

        public override void Initialize()
        {
            if (Points != null)
                return;
            Points = new IHPoint[Count];

            for (int i = 0; i < Points.Length; i++)
            {
                Points[i] = new HPoint(Next(), Next());
            }
            IHPoint[] variable;
            if (BoundaryContainer != null)
                variable = BoundaryContainer.AllBoundaryKnots;
        }

        double Next()
        {
            return min + rnd.NextDouble() * (max - min);
        }
    }
}
