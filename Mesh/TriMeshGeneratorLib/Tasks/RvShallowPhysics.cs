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
    /// <summary>
    /// Задача мелкой воды (Shallow)
    /// </summary>
    public class RVShallowPhysics : IRVPhysics
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
        public RVShallowPhysics() : base()
        {
            minDepth = MIN_DEPTH;
        }

        public virtual RVNode CreateNewNode(int n = 1, double x = 100.0,
                double y = 100.0, double z = 100.0)
        {
            return new RVNodeShallowWater(n, x, y, z, 0.1);
        }
        public virtual RVSegment CreateNewSegment(int n = 1, RVNode nP1 = null,
            RVNode nP2 = null, RVSegment segP = null)
        {
            RVBoundary bSeg, obSeg;

            bSeg = new RVBoundary(n, nP1, nP2);
            if (segP != null)
            {
                obSeg = (RVBoundary)segP;
                bSeg.bcCode = obSeg.bcCode;
                bSeg.BcValue = obSeg.BcValue;
            }
            return bSeg;
        }
        public virtual RVNode ReadNode(StreamReader file)
        {
            return new RVNodeShallowWater(file);
        }
        public virtual RVSegment ReadSegment(int n, RVNode nP1, RVNode nP2, StreamReader file)
        {
            RVBoundary nBSeg;
            //int code;
            //double value;

            nBSeg = new RVBoundary(n, nP1, nP2);

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
        public virtual string NameType => "RVShallowPhysics"; 
    }
}
