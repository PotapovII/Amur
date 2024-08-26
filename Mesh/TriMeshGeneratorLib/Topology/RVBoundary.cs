//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 11.08.2021 Потапов И.И.
//---------------------------------------------------------------------------
//             обновление и чистка : 01.01.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace TriMeshGeneratorLib
{
    public class RVBoundary : RVSegment
    {
        //public int flagCondition;
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

        public RVBoundary(int num = 1, RVNode nP1 = null, RVNode nP2 = null,
                             int c = 0, double v = 0.0) : base(num, nP1, nP2)
        {
            bcCode = c;
            bcValue = v;
            flowBound = null;
        }
        
        
        //public void setBcValue2(double d) { bcValue2 = d; }
        //public double getBcValue2() { return bcValue2; }
        //public void setFlowBound(RVFlowBoundary fBP) { flowBound = fBP; }
        //public RVFlowBoundary getFlowBound() { return flowBound; }
    }
}
