//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 11.08.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace TriMeshGeneratorLib.Advance
{
    using System.IO;
    /// <summary>
    /// Задача мелкой воды (Shallow)
    /// </summary>
    public class RivPhysics : IRivPhysics
    {
        /// <summary>
        /// минимальная глубина
        /// </summary>
        protected static double MIN_DEPTH = 0.02;
        /// <summary>
        /// Минимальная глубина потока
        /// </summary>
        public double MinDepth { get => minDepth; set => minDepth = value; }
        protected double minDepth;
        public RivPhysics() : base()
        {
            minDepth = MIN_DEPTH;
        }

        public virtual RivNode CreateNewNode(int n = 1, double x = 100.0,
                double y = 100.0, double z = 100.0)
        {
            return new RivNodeSW(n, x, y, z, 0.1);
        }
        public virtual RivEdge CreateNewSegment(int n = 1, RivNode nP1 = null,
            RivNode nP2 = null, RivEdge segP = null)
        {
            RivBoundary bSeg, obSeg;

            bSeg = new RivBoundary(n, nP1, nP2);
            if (segP != null)
            {
                obSeg = (RivBoundary)segP;
                bSeg.bcCode = obSeg.bcCode;
                bSeg.BcValue = obSeg.BcValue;
            }
            return bSeg;
        }
        public virtual RivNode ReadNode(StreamReader file)
        {
            return new RivNodeSW(file);
        }
        public virtual RivEdge ReadSegment(int n, RivNode nP1, RivNode nP2, StreamReader file)
        {
            RivBoundary nBSeg;
            //int code;
            //double value;

            nBSeg = new RivBoundary(n, nP1, nP2);

            //if ((is intint code))
            //	nBSeg.setBcCode(code);
            //else
            //	nBSeg.setn(-890);

            //if ((is intint value))
            //	nBSeg.SetBcValue(value);
            //else
            //	nBSeg.setn(-891);

            return (nBSeg);
        }
        public virtual int CountVars => 3; 
        public virtual int CountParams => 2; 
        public virtual string NameType => "RivPhysics"; 
    }
}
