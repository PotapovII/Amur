//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 11.08.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace TriMeshGeneratorLib.Advance
{
    public class RivBoundary : RivEdge
    {
        public int bcCode;
        /// <summary>
        /// Значение граничного условия
        /// </summary>
        public double BcValue { get => bcValue; set => bcValue = value; }
        protected double bcValue = 0;

        public double BcValueTwo { get => bcValue2; set => bcValue2 = value; }
        protected double bcValue2 = 0;

        public RVFlowBoundary FlowBound { get => flowBound; set => flowBound = value; }
        protected RVFlowBoundary flowBound = null;

        public RivBoundary(int num = 1, RivNode nP1 = null, RivNode nP2 = null,
                             int c = 0, double v = 0.0) : base(num, nP1, nP2)
        {
            bcCode = c;
            bcValue = v;
            flowBound = null;
        }
    }
}
