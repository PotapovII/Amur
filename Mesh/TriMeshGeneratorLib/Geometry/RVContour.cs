namespace TriMeshGeneratorLib
{
    /// <summary>
    /// Определяет класс контура, который вычисляет и сохраняет CountNodes и сегменты, 
    /// определяющие контурные линии для RVMeshIrregular.
    /// </summary>
    class RVContour
    {
        /// <summary>
        /// связанный список CountNodes/linked list of CountNodes
        /// </summary>
        protected RVList nodesList = null;
        /// <summary>
        /// связанный список сегментов контура/linked list of contour segments
        /// </summary>
        protected RVList segmentsList = null;
        /// <summary>
        /// минимальное значение/minimum value
        /// </summary>
        protected double aMin;
        /// <summary>
        /// максимальное значение/maximum value
        /// </summary>
        protected double aMax;
        /// <summary>
        /// минимальное значение контура/minimum contour value
        /// </summary>
        protected double cMin;
        /// <summary>
        /// максимальное значение контура/maximum contour value
        /// </summary>
        protected double cMax;
        /// <summary>
        /// приращение контура / contour increment
        /// </summary>
        protected double contourIncriment;
        /// <summary>
        /// = 1, если требуется только нулевой контур/= 1 if only zero contour desired
        /// </summary>
        protected int onlyZero;
        /// <summary>
        /// количество контурных линий/number of contour lines
        /// </summary>
        protected int nC = 0;
        /// <summary>
        /// индекс параметра какую переменную окантовать/which variable to contoured
        /// </summary>
        protected int index;
        /// <summary>
        /// оконтуренный RVMeshIrregular / the RVMeshIrregular to be contoured
        /// </summary>
        protected RVMeshIrregular contourTIN;

        public RVContour(RVMeshIrregular contourTIN, double increment = 1, int index = 1)
        {
            this.contourTIN = contourTIN;
            if (increment > 0.0)
            {
                contourIncriment = increment;
                onlyZero = 0;
            }
            else
            {
                contourIncriment = -increment;
                onlyZero = 1;
            }
            this.index = index;
            InitMinMas();
            CreateMap();
        }
        /// <summary>
        /// Вычисление ограничений - минимальных и максимальных значений 
        /// </summary>
        protected void InitMinMas()
        {
            RVNode nP = contourTIN.firstNode;
            double p = nP.GetPapam(index);
            double max = p;
            double min = p;

            if (onlyZero == 1)
            {
                cMin = contourIncriment;
                cMax = cMin + 1.0;
                contourIncriment = 2.0;
                // Сохранение фактических минимальных и максимальных значений 
                // Store actual min and max values
                aMin = cMin;
                aMax = cMax;
            }
            else
            {
                while (nP != null)
                {
                    p = nP.GetPapam(index);
                    if (p > max)
                        max = p;
                    if (p < min)
                        min = p;
                    nP = contourTIN.NextNode;
                }
                // Сохраняем фактические минимальные и максимальные значения
                aMin = min;
                aMax = max;
                cMin = ((int)min / contourIncriment) * contourIncriment + 0.00001;
                cMax = ((int)max / contourIncriment + 1) * contourIncriment;
            }
        }
        /// <summary>
        /// Создание карты
        /// </summary>
        protected void CreateMap()
        {
            RVNode nP1 = null, nP2 = null;
            RVSegment sP = null;
            double cVal, tMin, tMax, vl;

            RVTriangle triangle = contourTIN.FirstTriElements;
            while (triangle != null)
            {
                if (triangle.Status == StatusFlag.Activate)
                {
                    tMin = cMax;
                    tMax = cMin;
                    for (int i = 0; i < 3; i++)
                    {
                        vl = (triangle.GetNode(i)).GetPapam(index);
                        if (vl > tMax)
                            tMax = vl;
                        if (vl < tMin)
                            tMin = vl;
                    }
                    tMin = ((int)tMin / contourIncriment - 1) * contourIncriment + 0.00001;
                    if ((index == 3) && (tMin < 0.0))
                        tMin = 0.00001;
                    //			tMax = ((int) tMax/contourIncriment + 2) * contourIncriment;
                    if (onlyZero == 1)
                    {
                        tMin = cMin;
                        tMax = cMax;
                    }
                    cVal = tMin;
                    while (cVal < tMax)
                    {
                        if (GetContourSegment(triangle, cVal, sP, nP1, nP2) == 2)
                        {
                            //					nodesList.Add(nP1);
                            //					nodesList.Add(nP2);
                            //					segmentsList.Add(sP);
                        }
                        cVal += contourIncriment;
                    }
                }
                triangle = contourTIN.NextTriElements;
            }
            segmentsList.BuildIndex();
            nodesList.BuildIndex();
        }

        protected int GetContourSegment(RVTriangle triangle, double cVal, RVSegment sPP, RVNode nPP1, RVNode nPP2)
        {
            int n = 0;
            RVSegment tSeg1 = new RVSegment(1, triangle.GetNode(0), triangle.GetNode(1));
            RVSegment tSeg2 = new RVSegment(2, triangle.GetNode(1), triangle.GetNode(2));
            RVSegment tSeg3 = new RVSegment(3, triangle.GetNode(2), triangle.GetNode(0));
            double v1 = (triangle.GetNode(0)).GetPapam(index);
            double v2 = (triangle.GetNode(1)).GetPapam(index);
            double v3 = (triangle.GetNode(2)).GetPapam(index);

            if ((v1 == v2) && (v2 == v3))
            {
                return 0;
            }

            if (((cVal >= v1) && (cVal <= v2)) || ((cVal <= v1) && (cVal >= v2)))
            {
                nPP1 = SetCNode(tSeg1, cVal);
                n += 1;
            }
            if (((cVal >= v2) && (cVal <= v3)) || ((cVal <= v2) && (cVal >= v3)))
            {
                if (n == 0)
                    nPP1 = SetCNode(tSeg2, cVal);
                else
                    nPP2 = SetCNode(tSeg2, cVal);
                n += 1;
            }
            if (n == 0)
            {
                return 0;
            }
            if (n == 1)
            {
                nPP2 = SetCNode(tSeg3, cVal);
                n += 1;
            }

            RVSegment sP = new RVSegment(1, nPP1, nPP2);
            segmentsList.Add(sP);
            sPP = sP;

            return 2;
        }

        protected RVNode SetCNode(RVSegment sP, double cVal)
        {
            double v1 = (sP.GetNode(0)).GetPapam(index);
            double v2 = (sP.GetNode(1)).GetPapam(index);
            double r, x, y;

            if (v2 > v1)
                r = (cVal - v1) / (v2 - v1);
            else if (v2 < v1)
                r = 1.0 - (cVal - v2) / (v1 - v2);
            else
                r = 0.0;

            x = (sP.GetNode(0)).X * (1 - r) + (sP.GetNode(1)).X * r;
            y = (sP.GetNode(0)).Y * (1 - r) + (sP.GetNode(1)).Y * r;

            RVNode nP = new RVNode(1, x, y, cVal);
            nodesList.Add(nP);
            return nP;
        }

        //public RVSegment firstCSeg()
        //{
        //    return (RVSegment)segmentsList.FirstItem();
        //}
        //public RVSegment nextCSeg()
        //{
        //    return (RVSegment)segmentsList.NextItem();
        //}
        //public double getAMin() { return (aMin); }     // JDS 10/97
        //public double getAMax() { return (aMax); }     // JDS 10/97
        //public double getCMin() { return (cMin); }
        //public double getCMax() { return (cMax); }
        //public double getCInc() { return (contourIncriment); }
    }
}
