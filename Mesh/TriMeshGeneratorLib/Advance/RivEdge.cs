//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 11.08.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace TriMeshGeneratorLib.Advance
{
    /// <summary>
    ///то базовый класс, определяющий отрезок прямой
    /// между двумя точками. Точки определяются как из класса RivNode.
    /// Предоставляется базовая геометрическая функциональность.
    /// Создание подкласса сегмента для одномерных элементов.
    /// Инициировано: 24 сентября 1995 г.
    /// Последняя редакция: 24 сентября 1995 г.
    /// </summary>
    public class RivEdge : ARivElement
    { 
        /// <summary>
        /// Список узлов определяющих сегмент (сторону) треугольника и другое
        /// </summary>
        protected RivNode[] Nodes = new RivNode[2];
        /// <summary>
        /// Сопряженные с сегменом треугольники которым принадлежит данная сторона
        /// </summary>
        protected ARivElement[] ElementOwner = new ARivElement[2];
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="FirstNodeP"></param>
        /// <param name="secondNodeP"></param>
        public RivEdge(int name = 1, RivNode FirstNodeP = null, RivNode secondNodeP = null) : base(name)
        {
            Nodes[0] = FirstNodeP;
            Nodes[1] = secondNodeP;
            ElementOwner[0] = null;
            ElementOwner[1] = null;
        }
        public override RivNode[] GetNodes()
        {
            return Nodes;
        }
        public override RivNode GetNode(int i)
        {
            return Nodes[i];
        }
        public override void SetNode(int i, RivNode elemNodes)
        {
            if (i < 2)
                Nodes[i] = elemNodes;
        }

        public override int GetNodeIndex(RivNode nP)
        {
            for (int i = 0; i < Count(); i++)
            {
                if (nP == Nodes[i])
                    return i;
            }
            return -1;
        }
        public override ARivElement[] GetOwners => ElementOwner;
        
        public override ARivElement GetOwner(int i)
        {
            return ElementOwner[i];
        }
        public override void SetOwner(int i, ARivElement ap)
        {
            if (i < 2)
                ElementOwner[i] = ap;
        }
        public override int Count()
        {
            return 2;
        }
        public override int CountOwners()
        {
            return 2;
        }
        public override string NameType()
        {
            return "RivEdge";
        }
        public virtual int getBcCode() { return 0; }

        public double length()
        {
            if ((Nodes[0] != null) && (Nodes[1] != null))
                return Nodes[0].Distance(Nodes[1]);
            else
                return -1.0;
        }
        /// <summary>
        /// positive is to the left of the segment
        /// Определение направления номали для сегмента
        /// положительное - слева от сегмента 
        /// 
        /// </summary>
        /// <param name="otherNodeP"></param>
        /// <returns></returns>
        public double whichSide(RivNode otherNodeP)
        {
            if (otherNodeP != null)
                return (otherNodeP.X - Nodes[0].X) * (Nodes[0].Y - Nodes[1].Y)
                     + (otherNodeP.Y - Nodes[0].Y) * (Nodes[1].X - Nodes[0].X);
            else
                return -1.0;
        }
        /// <summary>
        /// расстояние от середины сегмента до узла
        /// </summary>
        /// <param name="otherNodeP"></param>
        /// <returns></returns>
        public double midDistance(RivNode otherNodeP)
        {
            if (otherNodeP != null)
            {
                RivNode midPoint = new RivNode(0, (Nodes[0].X + Nodes[1].X) / 2.0,
                                           (Nodes[0].Y + Nodes[1].Y) / 2.0, 100.0);
                return midPoint.Distance(otherNodeP);
            }
            else
                return -1.0;
        }
        /// <summary>
        /// just a pass through for searches
        /// узел лежит справа или с лева от сегмента
        /// </summary>
        /// <param name="otherNodeP"></param>
        /// <returns></returns>
        public override ARivElement Inside(RivNode otherNodeP)
        {
            if (otherNodeP != null)
            {
                if (whichSide(otherNodeP) > 0.0) // с лево
                    return ElementOwner[0]; // левый треугольник от сегмента
                else
                    return ElementOwner[1]; // правый треугольник от сегмента
            }
            else
                return null;
        }
        /// <summary>
        /// creates a new node at pos along the segment and interpolates
        /// создает новый узел в позиции pos вдоль сегмента и интерполирует
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="nP"></param>
        public void LocateNodeAndInterpolation(double pos, RivNode nP)
        {
            double[] weights = new double[2];

            if (pos <= 0.0)
            {
                weights[0] = 1.0;
                weights[1] = 0.0;
            }
            else
            if (pos >= 1.0)
            {
                weights[0] = 0.0;
                weights[1] = 1.0;
            }
            else
            {
                weights[0] = 1.0 - pos;
                weights[1] = pos;
            }
            nP.Interpolation(Nodes, weights);
        }
        /// <summary>
        /// returns 1 if no intersection, 0 if special, -1 if intersecting
        /// </summary>
        /// <param name="otherSegP"></param>
        /// <returns></returns>
        public int intersect(RivEdge otherSegP)
        {
            RivNode nP1 = this.GetNode(0);
            RivNode nP2 = this.GetNode(1);
            RivNode oP1 = otherSegP.GetNode(0);
            RivNode oP2 = otherSegP.GetNode(1);

            if ((nP1 == oP1) || (nP1 == oP2) || (nP2 == oP1) || (nP2 == oP2))
                return 0;

            double ny = oP2.X - oP1.X;
            double nx = oP1.Y - oP2.Y;

            double num = nx * (oP1.X - nP1.X) + ny * (oP1.Y - nP1.Y);
            double den = nx * (nP2.X - nP1.X) + ny * (nP2.Y - nP1.Y);

            if (den == 0.0)
                return 0;

            double r = num / den;
            if ((r >= 0.0) && (r <= 1.0))
                return -1;

            return 1;
        }

        /// <summary>
        /// returns Distance to intersection, -1.0 if no intersecting
        /// </summary>
        /// <param name="otherSegP"></param>
        /// <returns></returns>
        public double intersectd(RivEdge otherSegP)
        {
            RivNode nP1 = this.GetNode(0);
            RivNode nP2 = this.GetNode(1);
            RivNode oP1 = otherSegP.GetNode(0);
            RivNode oP2 = otherSegP.GetNode(1);

            if (((nP1 == oP1) && (nP2 == oP2)) || ((nP1 == oP2) && (nP2 == oP1)))
                return -1.0;
            if ((nP1 == oP1) || (nP1 == oP2) || (nP2 == oP1) || (nP2 == oP2))
                return 0.0;

            double ny = oP2.X - oP1.X;
            double nx = oP1.Y - oP2.Y;

            double num = nx * (oP1.X - nP1.X) + ny * (oP1.Y - nP1.Y);
            double den = nx * (nP2.X - nP1.X) + ny * (nP2.Y - nP1.Y);

            if (den == 0.0)
                return 0.0;

            double r = num / den;
            if ((r >= 0.0) && (r <= 1.0))
                return r;

            return -1.0;
        }
    }
}
