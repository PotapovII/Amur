namespace MeshAdapterLib
{
    using TriangleNet.Meshing;
    using TriangleNet.Meshing.Algorithm;

    public class QualityMeshNetOptions
    {
        public int CountSmooth;
        public bool RefineMode;
        public bool Refine;
        public SweepLine sweepLine = null;
        public ConstraintOptions options = null;
        public QualityOptions quality = null;
        public QualityMeshNetOptions()
        {
            options = new ConstraintOptions();
            quality = new QualityOptions();
            CountSmooth = 0;
            RefineMode = false;
            Refine = false;
        }
        public QualityMeshNetOptions(QualityMeshNetOptions op) 
        { 
            options = op.options;
            quality = op.quality;
            CountSmooth = op.CountSmooth;
            RefineMode = op.RefineMode;
            Refine = op.Refine;
        }
    }
}
