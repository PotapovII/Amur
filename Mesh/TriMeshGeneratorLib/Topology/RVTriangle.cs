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
    using System;
    /// <summary>
    /// Трехугольник
    /// </summary>
    public class RVTriangle : RVElement
    {
        /// <summary>
        /// узлы треугольника
        /// </summary>
        protected RVNode[] Nodes = new RVNode[3];
        /// <summary>
        /// соседние сопряженные треугольники
        /// </summary>
        protected RVElement[] AdjacentTriangle = new RVElement[3];
        /// <summary>
        /// сегменты грани треугольника
        /// </summary>
        protected RVSegment[] Edges = new RVSegment[3];
        /// <summary>
        /// быть уточненным?
        /// </summary>
        protected int toBeRefined;
        /// <summary>
        /// площадь  треугольника
        /// </summary>
        protected double area;
        // next prev ? в списке?
        public RVTriangle NextTriangle, prevTriangle;
        public RVTriangle(int id = 1, RVNode FirstNodeP = null, RVNode secondNodeP = null, RVNode thirdNodeP = null) : base(id)
        {
            Nodes[0] = FirstNodeP;
            Nodes[1] = secondNodeP;
            Nodes[2] = thirdNodeP;
            AdjacentTriangle[0] = null;
            AdjacentTriangle[1] = null;
            AdjacentTriangle[2] = null;
            Edges[0] = null;
            Edges[1] = null;
            Edges[2] = null;
            toBeRefined = 0;
        }
        public override RVNode[] GetNodes() { return Nodes; }
        public override RVNode GetNode(int i) { return Nodes[i]; }
        public override void SetNode(int i, RVNode elemNodes)
        { 
            if (i < 3) 
                Nodes[i] = elemNodes; 
        }
        public virtual RVSegment GetEdge(int i) { return Edges[i]; }
        public virtual void SetEdge(int i, RVSegment ep) { if (i < 3) Edges[i] = ep; }
        public override int GetNodeIndex(RVNode nP)
        {
            for (int i = 0; i < Count(); i++)
                if (nP == Nodes[i])
                    return i;
            return -1;
        }
        public override RVElement[] GetOwners =>AdjacentTriangle; 
        public override RVElement GetOwner(int i) { return AdjacentTriangle[i]; }
        public RVTriangle tP0 => (RVTriangle)AdjacentTriangle[0]; 
        public RVTriangle tP1 => (RVTriangle)AdjacentTriangle[1]; 
        public RVTriangle tP2 => (RVTriangle)AdjacentTriangle[2]; 
        public RVNode node0 => Nodes[0]; 
        public RVNode node1 => Nodes[1]; 
        public RVNode node2 => Nodes[2]; 
        public void SetRefineOn() { toBeRefined = 1; }
        public void SetRefineOff() { toBeRefined = 0; }
        public int IsRefineOn => toBeRefined; 
        public int tsn => AdjacentTriangle[2].reflectAdj(this); 
        /// <summary>
        /// добавляем ссылки на сопряженные треугольники 
        /// </summary>
        /// <param name="i">номер элемента</param>
        /// <param name="ap">сопряженный элемент</param>
        public override void SetOwner(int i, RVElement ap) 
        { 
            if (i < 3) 
                AdjacentTriangle[i] = ap; 
        }
        /// <summary>
        /// Количество эдементов
        /// </summary>
        /// <returns></returns>
        public override int Count() { return 3; }
        /// <summary>
        /// количество владельцев
        /// </summary>
        /// <returns></returns>
        public override int CountOwners() { return 3; }
        /// <summary>
        /// треугольник
        /// </summary>
        /// <returns></returns>
        public override string NameType() { return "RVTriangle"; }
        /// <summary>
        /// площадь треугольника
        /// </summary>
        /// <returns></returns>
        public double Area()
        {
            if ((Nodes[0] != null) && (Nodes[1] != null) && (Nodes[2] != null))
            {
                area = ((Nodes[1].X - Nodes[0].X) * (Nodes[2].Y - Nodes[0].Y)
                  - (Nodes[2].X - Nodes[0].X) * (Nodes[1].Y - Nodes[0].Y)) / 2.0;
                return area;
            }
            else
                return -1.0;
        }
        public double TribAreas(double[] an)
        {
            if ((Nodes[0] != null) && (Nodes[1] != null) && (Nodes[2] != null))
            {
                double x1 = Nodes[0].X;
                double y1 = Nodes[0].Y;
                double x2 = Nodes[1].X;
                double y2 = Nodes[1].Y;
                double x3 = Nodes[2].X;
                double y3 = Nodes[2].Y;
                double X21 = x2 - x1;
                double X32 = x3 - x2;
                double X13 = x1 - x3;
                double Y21 = y2 - y1;
                double Y32 = y3 - y2;
                double Y13 = y1 - y3;
                double d1 = -X13 * X21 - Y13 * Y21;
                double d2 = -X21 * X32 - Y21 * Y32;
                double d3 = -X32 * X13 - Y32 * Y13;
                double c1 = d2 * d3;
                double c2 = d3 * d1;
                double c3 = d1 * d2;
                double c = c1 + c2 + c3;
                double xc = ((c2 + c3) * x1 + (c3 + c1) * x2 + (c1 + c2) * x3) / (2.0 * c);
                double yc = ((c2 + c3) * y1 + (c3 + c1) * y2 + (c1 + c2) * y3) / (2.0 * c);
                RVNode cNode = new RVNode(100, xc, yc);
                if (Inside(cNode) == this)
                {
                    RVTriangle t1 = new RVTriangle(1, Nodes[0], cNode, Nodes[2]);
                    RVTriangle t2 = new RVTriangle(2, Nodes[1], cNode, Nodes[0]);
                    RVTriangle t3 = new RVTriangle(3, Nodes[2], cNode, Nodes[1]);
                    an[0] = (t1.Area() + t2.Area()) / 2.0;
                    an[1] = (t2.Area() + t3.Area()) / 2.0;
                    an[2] = (t3.Area() + t1.Area()) / 2.0;
                }
                else
                {
                    RVSegment l1 = new RVSegment(1, Nodes[0], Nodes[1]);
                    RVSegment l2 = new RVSegment(2, Nodes[1], Nodes[2]);
                    RVSegment l3 = new RVSegment(3, Nodes[2], Nodes[0]);
                    if ((l1.length() > l2.length()) && (l1.length() > l3.length()))
                    {
                        RVNode bisectNode = new RVNode(100, 100, 100);
                        l3.LocateNodeAndInterpolation(0.5, bisectNode);
                        RVSegment bisectSeg = new RVSegment(1, cNode, bisectNode);
                        double r = l1.intersectd(bisectSeg);
                        RVNode intersectNode = new RVNode(100, 100, 100);
                        l1.LocateNodeAndInterpolation(r, intersectNode);
                        RVTriangle tri = new RVTriangle(1, intersectNode, bisectNode, Nodes[0]);
                        an[0] = tri.Area();

                        l2.LocateNodeAndInterpolation(0.5, bisectNode);
                        bisectSeg.SetNode(1, bisectNode);
                        r = l2.intersectd(bisectSeg);
                        l1.LocateNodeAndInterpolation(r, intersectNode);
                        tri.SetNode(1, Nodes[1]);
                        tri.SetNode(2, bisectNode);
                        an[1] = tri.Area();

                        an[2] = this.Area() - an[0] - an[1];
                    }
                    else if (l2.length() > l3.length())
                    {
                        RVNode bisectNode = new RVNode(100, 100, 100);
                        l1.LocateNodeAndInterpolation(0.5, bisectNode);
                        RVSegment bisectSeg = new RVSegment(1, cNode, bisectNode);
                        double r = l2.intersectd(bisectSeg);
                        RVNode intersectNode = new RVNode(100, 100, 100);
                        l2.LocateNodeAndInterpolation(r, intersectNode);
                        RVTriangle tri = new RVTriangle(1, intersectNode, bisectNode, Nodes[1]);
                        an[1] = tri.Area();

                        l3.LocateNodeAndInterpolation(0.5, bisectNode);
                        bisectSeg.SetNode(1, bisectNode);
                        r = l2.intersectd(bisectSeg);
                        l2.LocateNodeAndInterpolation(r, intersectNode);
                        tri.SetNode(1, Nodes[2]);
                        tri.SetNode(2, bisectNode);
                        an[2] = tri.Area();

                        an[0] = this.Area() - an[1] - an[2];
                    }
                    else
                    {
                        RVNode bisectNode = new RVNode(100, 100, 100);
                        l2.LocateNodeAndInterpolation(0.5, bisectNode);
                        RVSegment bisectSeg = new RVSegment(1, cNode, bisectNode);
                        double r = l3.intersectd(bisectSeg);
                        RVNode intersectNode = new RVNode(100, 100, 100);
                        l3.LocateNodeAndInterpolation(r, intersectNode);
                        RVTriangle tri = new RVTriangle(1, intersectNode, bisectNode, Nodes[2]);
                        an[2] = tri.Area();

                        l1.LocateNodeAndInterpolation(0.5, bisectNode);
                        bisectSeg.SetNode(1, bisectNode);
                        r = l3.intersectd(bisectSeg);
                        l3.LocateNodeAndInterpolation(r, intersectNode);
                        tri.SetNode(1, Nodes[0]);
                        tri.SetNode(2, bisectNode);
                        an[0] = tri.Area();

                        an[1] = this.Area() - an[0] - an[2];
                    }
                }

                return (an[0] + an[1] + an[2]);
            }
            else
                return -1.0;
        }
        /// <summary>
        /// Критерий качества треугольника
        /// </summary>
        /// <returns></returns>
        public double Quality()
        {
            if ((Nodes[0] != null) && (Nodes[1] != null) && (Nodes[2] != null))
            {
                double X21 = Nodes[1].X - Nodes[0].X;
                double X32 = Nodes[2].X - Nodes[1].X;
                double X13 = Nodes[0].X - Nodes[2].X;
                double Y21 = Nodes[1].Y - Nodes[0].Y;
                double Y32 = Nodes[2].Y - Nodes[1].Y;
                double Y13 = Nodes[0].Y - Nodes[2].Y;
                double d1 = -X13 * X21 - Y13 * Y21;
                double d2 = -X21 * X32 - Y21 * Y32;
                double d3 = -X32 * X13 - Y32 * Y13;
                double c = d2 * d3 + d3 * d1 + d1 * d2;
                if (c == 0.0)
                    return 0.0;
                double r = Math.Sqrt((d1 + d2) * (d2 + d3) * (d3 + d1) / c) / 2.0;
                double q = 2.0 * Area() / (3.0 * r * r);
                return q;
            }
            return 0.0;
        }
        /// <summary>
        /// указатель на себя, если узел внутри, в противном случае - на следующий треугольник
        /// </summary>
        /// <param name="otherNodeP"></param>
        /// <returns></returns>
        public override RVElement Inside(RVNode otherNodeP)
        {
            RVSegment testSeg1 = new RVSegment(1, Nodes[0], Nodes[1]);
            RVSegment testSeg2 = new RVSegment(1, Nodes[1], Nodes[2]);
            RVSegment testSeg3 = new RVSegment(1, Nodes[2], Nodes[0]);

            if (otherNodeP != null)
            {
                if (testSeg1.whichSide(otherNodeP) < -0.0000001)
                {
                    return AdjacentTriangle[2];
                }
                if (testSeg2.whichSide(otherNodeP) < -0.0000001)
                {
                    return AdjacentTriangle[0];
                }
                if (testSeg3.whichSide(otherNodeP) < -0.0000001)
                {
                    return AdjacentTriangle[1];
                }
                return this;
            }
            else
                return null;
        }
        /// <summary>
        /// pointer to self if nodeP is one the tri Nodes, otherwise to next triangle
        /// указатель на себя, если nodeP является одним из трех узлов, 
        /// в противном случае - на следующий треугольник
        /// </summary>
        /// <param name="nodeP"></param>
        /// <returns></returns>
        public RVTriangle Contains(RVNode nodeP)
        {
            if (nodeP != null)
            {
                if ((Nodes[0] == nodeP) || (Nodes[1] == nodeP) || (Nodes[2] == nodeP))
                {
                    return this;
                }
                // ищем треугольник с количеством узлов меньше 3
                RVElement aP = Inside(nodeP);
                while (aP.Count() != 3)
                {
                    aP = aP.Inside(nodeP);
                    if (aP == null) return null;
                }    
                    
                return (RVTriangle)aP;
            }
            return null;
        }
        /// <summary>
        /// pointer to self if nodeP is one the tri Nodes, otherwise to next triangle
        /// указатель на себя, если nodeP является одним из трех узлов, 
        /// в противном случае - на следующий треугольник
        /// </summary>
        /// <param name="nodeP"></param>
        /// <returns></returns>
        public RVTriangle ActContains(RVNode nodeP)
        {
            if (nodeP != null)
            {
                int index = GetNodeIndex(nodeP);
                if (index < 0)
                {
                    RVElement aP = Inside(nodeP);
                    if (aP != null)
                    {
                        while (aP.Count() != 3)
                        {
                            aP = aP.Inside(nodeP);
                            if ((aP == null) || (aP == this))
                                return null;
                        }
                        return (RVTriangle)aP;
                    }
                    else
                        return null;
                }
                if (status == StatusFlag.Activate)
                    return this;
            }
            return null;
        }
        /// <summary>
        /// pointer to self if segment Inside, otherwise to next triangle
        /// указатель на себя, если сегмент внутри, иначе на следующий треугольник
        /// </summary>
        /// <param name="segP"></param>
        /// <returns></returns>
        public RVTriangle Contains(RVSegment segP)
        {
            if (segP != null)
            {
                if ((Nodes[0] == segP.GetNode(0)) &&
                    (Nodes[1] == segP.GetNode(1)))
                {
                    return this;
                }
                if ((Nodes[1] == segP.GetNode(0)) &&
                    (Nodes[2] == segP.GetNode(1)))
                {
                    return this;
                }
                if ((Nodes[2] == segP.GetNode(0)) &&
                    (Nodes[0] == segP.GetNode(1)))
                {
                    return this;
                }
            }

            return null;
        }
        /// <summary>
        /// pointer to adjacent triangle which Contains node
        /// указатель на соседний треугольник, который содержит узел
        /// </summary>
        /// <param name="nP1"></param>
        /// <returns></returns>
        RVTriangle AdjContaining(RVNode nP1)
        {
            RVElement pP;
            RVTriangle tP;

            for (int i = 0; i < 3; i++)
            {
                pP = AdjacentTriangle[i];
                if (pP != null)
                    if (pP.Count() == 3)
                    {
                        tP = (RVTriangle)pP;
                        if (tP.GetNodeIndex(nP1) >= 0)
                            return tP;
                    }
            }
            return null;
        }
        /// <summary>
        /// positive if Inside of circumcircle
        /// положительный, если внутри описанной окружности
        /// </summary>
        /// <param name="otherNodeP"></param>
        /// <returns></returns>
        public double InsideC(RVNode otherNodeP)
        {
            double xp, yp, x1, y1, x2, y2, x3, y3, numr, numi, denr, deni;

            if (otherNodeP != null)
            {
                xp = otherNodeP.X;
                yp = otherNodeP.Y;
            }
            else
                return -1.0;

            x1 = Nodes[0].X;
            y1 = Nodes[0].Y;
            x2 = Nodes[1].X;
            y2 = Nodes[1].Y;
            x3 = Nodes[2].X;
            y3 = Nodes[2].Y;

            numr = (x2 - x3) * (xp - x1) - (y2 - y3) * (yp - y1);
            numi = (x2 - x3) * (yp - y1) + (y2 - y3) * (xp - x1);
            denr = (x2 - x1) * (xp - x3) - (y2 - y1) * (yp - y3);
            deni = (x2 - x1) * (yp - y3) + (y2 - y1) * (xp - x3);

            return denr * numi - deni * numr;
        }
        /// <summary>
        /// locates a node at x, y and interpolates
        /// находит узел в x, y и интерполирует
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="nP"></param>
        /// <returns></returns>
        public RVNode LocateNodeAndInterpolation(double x, double y, RVNode nP)
        {
            double[] weights = new double[3];
            double X21 = Nodes[1].X - Nodes[0].X;
            double X31 = Nodes[2].X - Nodes[0].X;
            double Y21 = Nodes[1].Y - Nodes[0].Y;
            double Y31 = Nodes[2].Y - Nodes[0].Y;
            double X = x - Nodes[0].X;
            double Y = y - Nodes[0].Y;
            double det = X21 * Y31 - X31 * Y21;
            double r = (Y31 * X - X31 * Y) / det;
            double s = (-Y21 * X + X21 * Y) / det;
            double t = 1.0 - r - s;

            weights[0] = t;
            weights[1] = r;
            weights[2] = s;

            nP.Interpolation(3, Nodes, weights);

            return nP;
        }
        /// <summary>
        /// 	locates a node at triangle center and interpolates
        /// 	находит узел в центре треугольника и интерполирует
        /// </summary>
        /// <param name="nP"></param>
        /// <returns></returns>
        public RVNode LocateNodeAtCenter(RVNode nP)
        {
            double[] weights = new double[3];
            double r = 1.0 / 3.0;
            double s = r;
            double t = r;

            weights[0] = t;
            weights[1] = r;
            weights[2] = s;

            nP.Interpolation(3, Nodes, weights);

            return nP;
        }
        /// <summary>
        /// interpolates a node which already has x, y set
        /// интерполирует узел, для которого уже установлены x, y
        /// </summary>
        /// <param name="nP"></param>
        /// <returns></returns>

        public RVNode LocateNodeAndInterpolation(RVNode nP)
        {
            double[] weights = new double[3];
            double X21 = Nodes[1].X - Nodes[0].X;
            double X31 = Nodes[2].X - Nodes[0].X;
            double Y21 = Nodes[1].Y - Nodes[0].Y;
            double Y31 = Nodes[2].Y - Nodes[0].Y;
            double X = nP.X - Nodes[0].X;
            double Y = nP.Y - Nodes[0].Y;
            double det = X21 * Y31 - X31 * Y21;
            double r = (Y31 * X - X31 * Y) / det;
            double s = (-Y21 * X + X21 * Y) / det;
            double t = 1.0 - r - s;

            weights[0] = t;
            weights[1] = r;
            weights[2] = s;

            nP.Interpolation(3, Nodes, weights);

            return nP;
        }
        /// <summary>
        /// returns the triangle node closest to the given node
        /// возвращает узел треугольника, ближайший к данному узлу
        /// </summary>
        /// <param name="nP"></param>
        /// <returns></returns>

        public RVNode GetClosestNode(RVNode nP)
        {
            RVNode theNodeP = Nodes[0];
            //int iNode;
            //RVElement adjtElP;
            double Distance;
            double minDistance = nP.Distance(theNodeP);
            for (int i = 1; i <= 2; i++)
            {
                Distance = nP.Distance(Nodes[i]);
                if (Distance < minDistance)
                {
                    minDistance = Distance;
                    theNodeP = Nodes[i];
                }
            }
            return theNodeP;
        }
    }
}
