namespace RenderEditLib
{
    using CommonLib.Mesh;
    using TriangleNet.Meshing;
    /// <summary>
    /// Опции герерации сетки в контуре
    /// </summary>
    public class OptionsGenMesh
    {
        /// <summary>
        /// минимальный угол
        /// </summary>
        public double MinimumAngle { get; set; }
        /// <summary>
        /// максимальный угол
        /// </summary>
        public double MaximumAngle { get; set; }
        /// <summary>
        /// максимальная площадь треугольника
        /// </summary>
        public double MaximumArea { get; set; }
        /// <summary>
        /// запрещает 1, не запрещает 0 создавать 
        /// узлы на границах контура
        /// </summary>
        public int SegmentSplitting { get; set; }
        /// <summary>
        /// количество циклов сглаживания
        /// </summary>
        public int CountSmooth { get; set; }
        /// <summary>
        /// создать контур
        /// </summary>
        public bool CreateContur { get; set; }
        /// <summary>
        /// создать трианг. Делоне
        /// </summary>
        public bool ConformingDelaunay { get; set; }
        /// <summary>
        /// направление перенумерации сетки
        /// </summary>
        public Direction DirectionRenumber { get; set; }
        /// <summary>
        /// перенумерация сетки
        /// </summary>
        public bool RenumberMesh { get; set; }

        public OptionsGenMesh()
        {
            MinimumAngle = 30;
            MaximumAngle = 160;
            MaximumArea = 1;
            SegmentSplitting = 0;
            CountSmooth = 2;
            CreateContur = true;
            ConformingDelaunay = true;
            DirectionRenumber = Direction.toRight;
            RenumberMesh = true;
        }
        public OptionsGenMesh(OptionsGenMesh p)
        {
            MinimumAngle = p.MaximumArea;
            MaximumAngle = p.MaximumAngle;
            MaximumArea = p.MaximumArea;
            SegmentSplitting = p.SegmentSplitting;
            CountSmooth = p.CountSmooth;
            CreateContur = p.CreateContur;
            ConformingDelaunay = p.ConformingDelaunay;
            DirectionRenumber = p.DirectionRenumber;
            RenumberMesh = p.RenumberMesh;
        }
        public void GetOptionsGenMesh(ref ConstraintOptions options, ref QualityOptions quality)
        {
            if (options == null) options = new ConstraintOptions();
            if (quality == null) quality = new QualityOptions();
            // создать контур
            options.Convex = CreateContur;
            // создать трианг. Делоне
            options.ConformingDelaunay = ConformingDelaunay;
            // создавать узлы на границах ?
            options.SegmentSplitting = SegmentSplitting;

            quality.MinimumAngle = MinimumAngle;
            quality.MaximumAngle = MaximumAngle;
            quality.MaximumArea = MaximumArea;
        }
    }
}
