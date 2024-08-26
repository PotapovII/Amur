//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 11.08.2024 Потапов И.И.
//---------------------------------------------------------------------------
using CommonLib.Geometry;
using System.Collections.Generic;

namespace TriMeshGeneratorLib.Advance
{
    /// <summary>
    /// Определяет класс контура, который вычисляет и сохраняет CountNodes и сегменты, 
    /// определяющие контурные линии для RVMeshIrregular.
    /// </summary>
    class RivContour
    {
        /// <summary>
        /// связанный список CountNodes/linked list of CountNodes
        /// </summary>
        protected List<IDPoint> nodesList = new List<IDPoint>();
        /// <summary>
        /// связанный список сегментов контура/linked list of contour segments
        /// </summary>
        protected List<RivEdge> segmentsList = new List<RivEdge>();
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
        protected RivMeshIrregular contourTIN;

        public RivContour(RivMeshIrregular contourTIN, double increment = 1, int index = 1)
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
            List<RivNode> nodesList = contourTIN.nodesList;
            double p = nodesList[0].GetPapam(index);
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
                for(int i = 1; i < nodesList.Count; i++)
                { 
                    p = nodesList[i].GetPapam(index);
                    if (p > max)
                        max = p;
                    if (p < min)
                        min = p;
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
            RivNode nP1 = null, nP2 = null;
            RivEdge sP = null;
            double cVal, tMin, tMax, vl;
            List<RivTriangle> triElementsList = contourTIN.triElementsList;
            foreach(RivTriangle triangle in triElementsList)
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
                            //nodesList.Add(nP1);
                            //nodesList.Add(nP2);
                            //segmentsList.Add(sP);
                        }
                        cVal += contourIncriment;
                    }
                }
            }
            //segmentsList.BuildIndex();
            //nodesList.BuildIndex();
        }

        protected int GetContourSegment(RivTriangle triangle, double cVal, RivEdge sPP, RivNode nPP1, RivNode nPP2)
        {
            int n = 0;
            RivEdge tSeg1 = new RivEdge(1, triangle.GetNode(0), triangle.GetNode(1));
            RivEdge tSeg2 = new RivEdge(2, triangle.GetNode(1), triangle.GetNode(2));
            RivEdge tSeg3 = new RivEdge(3, triangle.GetNode(2), triangle.GetNode(0));
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

            RivEdge sP = new RivEdge(1, nPP1, nPP2);
            segmentsList.Add(sP);
            sPP = sP;

            return 2;
        }

        protected RivNode SetCNode(RivEdge sP, double cVal)
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

            RivNode nP = new RivNode(1, x, y, cVal);
            nodesList.Add(nP);
            return nP;
        }

        //public RivEdge firstCSeg()
        //{
        //    return (RivEdge)segmentsList.FirstItem();
        //}
        //public RivEdge nextCSeg()
        //{
        //    return (RivEdge)segmentsList.NextItem();
        //}
        //public double getAMin() { return (aMin); }     // JDS 10/97
        //public double getAMax() { return (aMax); }     // JDS 10/97
        //public double getCMin() { return (cMin); }
        //public double getCMax() { return (cMax); }
        //public double getCInc() { return (contourIncriment); }
    }
}
