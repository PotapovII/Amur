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
    using System.IO;
    /// <summaryint
    /// интерфейс для класса BCListPoints.
    /// </summaryint
    public class BCListPoints : RVListPoints
    {
        public BCListPoints(StreamReader f):base()
        {
        }
    }
    /// <summary>
    /// Граничный сегмент (ГС)
    /// </summary>
    public class RVFlowBoundary : RVItem
    {
        /// <summary>
        /// Код граничных условий
        /// </summary>
        protected int bcCode;
        /// <summary>
        /// Значение величины в начале ГС
        /// </summary>
        protected double bcValue;
        /// <summary>
        /// Значение величины в конце ГС
        /// </summary>
        protected double bcValue2;
        /// <summary>
        /// Начальный уровень ГС
        /// </summary>
        protected RVNode startNode;
        /// <summary>
        /// Конечный уровень ГС
        /// </summary>
        protected RVNode endNode;
        protected int transCode;
        protected bool ratingCurve;
        protected string bcFilePathName;
        protected RVListPoints transLst;
        public RVFlowBoundary(int name = 1, RVNode start = null, RVNode end = null, int code = 0,
            double value = 0, double value2 = 0, int tCode = 0, string path = "") : base(name)
        {
            startNode = start;
            endNode = end;
            ID = code;
            bcValue = value;
            bcValue2 = value2;
            transCode = tCode;
            bcFilePathName = path;
            transLst = null;
            if (bcFilePathName != "")
            {
                if (bcFilePathName[bcFilePathName.Length - 1] == 'r')
                    ratingCurve = true;
                else
                    ratingCurve = false;
            }
            else
                ratingCurve = false;
        }

        public RVNode getStartNode() { return startNode; }
        public void setStartNode(RVNode start) { startNode = start; }
        public RVNode getEndNode() { return endNode; }
        public void setEndNode(RVNode end) { endNode = end; }
        public int getTransCode() { return transCode; }
        public double getBcValue() { return bcValue; }
        public double getBcValue2() { return bcValue2; }
        public string getFilePath() { return bcFilePathName; }
        public void loadTransLst(StreamReader f)
        {
            transLst = new BCListPoints(f);
        }
        public int getBcCode() { return bcCode; }
        public void setBcCode(int code) { bcCode = code; }

        public void deleteTransLst() { transLst = null; }
        public double getInterpValue(double timeOrQ) { return (transLst.Interpolation(timeOrQ)); }
        public void setFilePath(string path) { bcFilePathName = path; }
        public bool getYesNoRatingCurve() { return ratingCurve; }
        public void setYesNoRatingCurve(bool yesNoCurve) { ratingCurve = yesNoCurve; }
        public RVListPoints gettransLst() { return transLst; }
        public void setBcValue2(double value2) { bcValue2 = value2; }
        public void SetBcValue(double value) { bcValue = value; }
        public void setTransCode(int trans) { transCode = trans; }
    }
}
