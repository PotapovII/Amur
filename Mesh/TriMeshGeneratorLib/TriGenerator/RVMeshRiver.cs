//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 12.08.2021 Потапов И.И.
//---------------------------------------------------------------------------
//             обновление и чистка : 01.01.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace TriMeshGeneratorLib
{
    using System;
    using System.IO;
    using CommonLib.Mesh.RVData;
    using MemLogLib;
    enum FEMProblem
    {
        psiSolution = 0,
        phiSolution
    };

    /// <summary>
    /// Сетка для задачи о речном потоке (HabitatTIN)
    /// </summary>
    public class RVMeshRiver : RVMeshIrregular
    {
        protected static string F6 = "F6";
        protected static string F8 = "F8";
        protected double wsElevOut = 0, wsElevIn = 0;
        protected RVBoundary outSegP = new RVBoundary();
        protected RVBoundary inSegP = new RVBoundary();
        /// <summary>
        /// Версия формата данных sdg
        /// </summary>
        public const string VersionLMesh = "Ver 18.11.2021";
        /// <summary>
        /// Количество узлов
        /// </summary>
        protected new int CountNodes;
        /// <summary>
        /// Количество элементов
        /// </summary>
        protected int CountElements;
        /// <summary>
        /// Количество граничных элементов
        /// </summary>
        protected int CountBoundElements;
        /// <summary>
        /// Количество граничных сегментов
        /// </summary>
        protected int CountBoundSegs;
        /// <summary>
        /// Список граничных сегментов с неоднородными ГУ на границе (потоки и шлубины)
        /// </summary>
        public RVList FlowBoundList = new RVList();

        //private RVList BElMtxL = null;
        
        //private RVList ElMtxL = null;

        private FEMProblem FEMProb = FEMProblem.psiSolution;
        public RVMeshRiver(IRVPhysics theProb) : base(theProb) { }

        public RVNodeHabitat getBoundaryNode(RVMeshIrregular meshTIN, RVNodeHabitat hNP, double boundaryValue, int flag, int parChoice)
        {
            double dx, dy, d, dxLimit, dyLimit, dLimit;
            double xPath, yPath;
            double v1, v2, r;

            RVNodeHabitat hNPath, hNBoundNode;
            RVNodeHabitat[] intersectNodes = new RVNodeHabitat[3];
            RVSegment segPathP;
            RVSegment[] elementSegs = new RVSegment[3];
            RVTriangle triangle, pathTriP, checkTriP;
            // создайте путь, по которому нужно следовать,
            // чтобы получить нижний / верхний граничный узел
            RVBox limits = meshTIN.GetLimits();
            // RVBox limits1 = RVBox.CreateRVBoxLimits(meshTIN);
            // использует направление расхода для определения пути 
            // uses direction of discharge intensity to get direction of path
            dx = hNP.GetPapam(4);  
            dy = hNP.GetPapam(5);
            d = Math.Sqrt(dx * dx + dy * dy);
            dxLimit = limits.x2 - limits.x1;
            dyLimit = limits.y2 - limits.y1;
            dLimit = Math.Sqrt(dxLimit * dxLimit + dyLimit * dyLimit);
            // to get path to lower boundary node
            // выбор пути к нижнему граничному узлу
            if (flag == 0)      
            {
                xPath =  (dLimit / d) * dy + hNP.X;
                yPath = -(dLimit / d) * dx + hNP.Y;
            }
            else
            {
                // to get path to upper boundary node
                // выбор пути к верхнему граничному узлу
                xPath = -(dLimit / d) * dy + hNP.X;
                yPath =  (dLimit / d) * dx + hNP.Y;
            }

            hNPath = (RVNodeHabitat)physics.CreateNewNode(1, xPath, yPath);
            segPathP = new RVSegment(1, hNP, hNPath);
            pathTriP = meshTIN.WhichTriangle(hNPath);
            // check to see if boundary node is in the current triangle,
            // if not get triangle with boundary node
            // проверьте, находится ли граничный узел в текущем треугольнике,
            // если нет, получите треугольник с граничным узлом
            triangle = meshTIN.WhichTriangle(hNP);

            while (true)//(-1)
            {
                checkTriP = findTriWithBoundaryNode(triangle, segPathP, boundaryValue, parChoice);
                if (triangle != checkTriP)
                    triangle = checkTriP;
                else
                    break;
            }

            //with correct triangle, get boundary node

            //find CountNodes where path intersects the correct triangle
            int j;
            for (j = 0; j < 3; j++)
                intersectNodes[j] = null;

            for (int k = 0; k < 3; k++)
            {
                int i = k + 1;
                if (i == 3) i = 0;
                j = k + 2;
                if (j == 3) j = 0;
                if (j == 4) j = 1;
                elementSegs[k] = new RVSegment(1, triangle.GetNode(i), triangle.GetNode(j));
            }
            for (int i = 0; i < 3; i++)
            {
                r = elementSegs[i].intersectd(segPathP);
                if ((r > 0) && (r < 1.0))
                {
                    intersectNodes[i] = (RVNodeHabitat)physics.CreateNewNode(1, 100, 100);
                    elementSegs[i].LocateNodeAndInterpolation(r, intersectNodes[i]);
                }
            }

            //reset path to extend only across the correct triangle

            for (int i = 0; i < 3; i++)
            {
                j = i + 1;
                if (j == 3) j = 0;
                if ((intersectNodes[i] != null) && (intersectNodes[j] != null))
                {
                    segPathP.SetNode(0, intersectNodes[i]);
                    segPathP.SetNode(1, intersectNodes[j]);
                }
            }

            //interpolate the boundary node within the correct triangle

            v1 = segPathP.GetNode(0).GetPapam(parChoice);
            v2 = segPathP.GetNode(1).GetPapam(parChoice);

            if (v2 > v1)
                r = (boundaryValue - v1) / (v2 - v1);
            else if (v2 < v1)
                r = 1.0 - (boundaryValue - v2) / (v1 - v2);
            else
                r = 0.0;

            hNBoundNode = (RVNodeHabitat)physics.CreateNewNode(1, 100, 100);
            segPathP.LocateNodeAndInterpolation(r, hNBoundNode);

            for (int i = 0; i < 3; i++)
            {
                elementSegs[i] = null;
                intersectNodes[i] = null;
            }
            return hNBoundNode;
        }

        public RVTriangle findTriWithBoundaryNode(RVTriangle triangle, RVSegment segPathP, double boundaryValue, int parChoice)
        {
            RVSegment[] elementSegs = new RVSegment[3];
            RVNodeHabitat[] intersectNodes = new RVNodeHabitat[3];
            RVNode movePath, hNPath;
            double r, Distance;

            Distance = 10e10;
            int j, k;
            for (j = 0; j < 3; j++)
                intersectNodes[j] = null;

            //find CountNodes where path intersects the triangle triangle

            //first create the segments of the triangle

            for (k = 0; k < 3; k++)
            {
                int i = k + 1;
                if (i == 3) i = 0;
                j = k + 2;
                if (j == 3) j = 0;
                if (j == 4) j = 1;
                elementSegs[k] = new RVSegment(1, triangle.GetNode(i), triangle.GetNode(j));
            }

            //check to see if intersection is at a node, if so move the path slightly to avoid this
            //this might not always work...need to think about

            for (int i = 0; i < 3; i++)
            {
                r = elementSegs[i].intersectd(segPathP);
                if ((r == 0) || (r == 1.0))
                {
                    movePath = segPathP.GetNode(0);
                    movePath.Init(movePath.X + 0.0001, movePath.Y + 0.0001, 100);
                    movePath = segPathP.GetNode(1);
                    movePath.Init(movePath.X + 0.0001, movePath.Y + 0.0001, 100);
                }
            }

            //find intersection points - assumes there are 2

            for (int i = 0; i < 3; i++)
            {
                r = elementSegs[i].intersectd(segPathP);
                if ((r > 0) && (r < 1.0))
                {
                    intersectNodes[i] = (RVNodeHabitat)physics.CreateNewNode(1, 100, 100);
                    elementSegs[i].LocateNodeAndInterpolation(r, intersectNodes[i]);
                }
            }

            //find the next triangle in along the path 
            //based the Distance of the point from hNPath which is the user input point 

            j = -1;
            hNPath = segPathP.GetNode(1);
            for (int i = 0; i < 3; i++) //find which node is closest to hNPath
            {
                if (intersectNodes[i] != null)
                {
                    if (Math.Abs(intersectNodes[i].Distance(hNPath)) < Distance)
                    {
                        Distance = Math.Abs(intersectNodes[i].Distance(hNPath));
                        j = i;
                    }
                }
            }
            k = 0;
            for (int i = 0; i < 3; i++) //find which CountNodes is farthest from hNPath
                                        //which will be the node that is not j
                                        //and not null
            {
                if ((intersectNodes[i] != null) && (i != j))
                    k = i;
            }

            double diffj, diffk, diffjk;

            diffj = Math.Abs(intersectNodes[j].GetPapam(parChoice) - boundaryValue);
            diffk = Math.Abs(intersectNodes[k].GetPapam(parChoice) - boundaryValue);
            diffjk = Math.Abs(intersectNodes[j].GetPapam(parChoice) - intersectNodes[k].GetPapam(parChoice));

            if ((diffj <= diffjk) && (diffk <= diffjk))
            {
                //leave triangle as is
            }
            else
                triangle = (RVTriangle)triangle.GetOwner(j);

            for (int i = 0; i < 3; i++)
            {
                elementSegs[i] = null;
                intersectNodes[i] = null;
            }
            return triangle;
        }

        public RVTriangle mapStreamLine(RVMeshIrregular meshTIN, RVTriangle triangle, RVNodeHabitat hNPStart, RVNodeHabitat hNPEnd, double boundaryValue, int flag, int parChoice)
        {
            RVNodeHabitat[] intersectNodes = new RVNodeHabitat[3];
            RVNodeHabitat lastNode;
            RVSegment[] elementSegs = new RVSegment[3];
            RVBoundary boundSeg;
            int segNum;

            //if the current triangle triangle Contains the end point then append the end point
            //and return the triangle

            if (triangle == meshTIN.WhichTriangle(hNPEnd))
            {
                lastNode = (RVNodeHabitat)nodesList.LastItem();
                AddNode(hNPEnd);
                if (boundarySegmentsList.FirstItem() == null) segNum = 1;
                else segNum = boundarySegmentsList.Count + 1;
                boundSeg = new RVBoundary(segNum, lastNode, hNPEnd, 0, 0);
                AddBoundarySegment(boundSeg);
                return triangle;
            }

            //if not, get the next node and segment along the streamline to append to the list

            else
            {
                int j;
                for (j = 0; j < 3; j++)
                    intersectNodes[j] = null;

                for (int k = 0; k < 3; k++)
                {
                    int i = k + 1;
                    if (i == 3) i = 0;
                    j = k + 2;
                    if (j == 3) j = 0;
                    if (j == 4) j = 1;
                    elementSegs[k] = new RVSegment(1, triangle.GetNode(i), triangle.GetNode(j));
                }

                //find CountNodes where the streamline intersects the triangle triangle

                for (int i = 0; i < 3; i++)
                {
                    double parValue1, parValue2, r;

                    parValue1 = elementSegs[i].GetNode(0).GetPapam(parChoice);
                    parValue2 = elementSegs[i].GetNode(1).GetPapam(parChoice);

                    //provisions for the case when the streamline intersects exactly
                    //at a node - simply move the streamline to avoid the intersection at
                    //a node
                    //not sure how well this works...need to think about

                    if (parValue1 == boundaryValue || parValue2 == boundaryValue)
                    {
                        if (flag == 0) boundaryValue = boundaryValue + 0.0001;
                        if (flag == 1) boundaryValue = boundaryValue - 0.0001;
                    }

                    if (((parValue1 > boundaryValue) && (parValue2 < boundaryValue)) || ((parValue1 < boundaryValue) && (parValue2 > boundaryValue)))
                    {
                        if (parValue2 > parValue1)
                            r = (boundaryValue - parValue1) / (parValue2 - parValue1);
                        else if (parValue2 < parValue1)
                            r = 1.0 - (boundaryValue - parValue2) / (parValue1 - parValue2);
                        else r = 0.0;

                        intersectNodes[i] = (RVNodeHabitat)physics.CreateNewNode(1, 100, 100);
                        elementSegs[i].LocateNodeAndInterpolation(r, intersectNodes[i]);
                    }
                }

                //determine which of the two intersection CountNodes is the next node to appended

                for (int i = 0; i < 3; i++)
                {
                    j = i + 1;
                    if (j == 3) j = 0;
                    if ((intersectNodes[i] != null) && (intersectNodes[j] != null))
                    {

                        if (triangle == meshTIN.WhichTriangle(hNPStart))
                        {
                            double distance1, distance2;
                            distance1 = intersectNodes[i].Distance(hNPEnd);
                            distance2 = intersectNodes[j].Distance(hNPEnd);
                            if (distance1 > distance2)
                            {
                                lastNode = (RVNodeHabitat)nodesList.LastItem();
                                AddNode(intersectNodes[j]);
                                if (boundarySegmentsList.FirstItem() == null) segNum = 1;
                                else segNum = boundarySegmentsList.Count + 1;
                                boundSeg = new RVBoundary(segNum, lastNode, intersectNodes[j], 0, 0);
                                AddBoundarySegment(boundSeg);
                                intersectNodes[i] = null;
                                triangle = (RVTriangle)triangle.GetOwner(j);
                            }
                            else
                            {
                                lastNode = (RVNodeHabitat)nodesList.LastItem();
                                AddNode(intersectNodes[i]);
                                if (boundarySegmentsList.FirstItem() == null) segNum = 1;
                                else segNum = boundarySegmentsList.Count + 1;
                                boundSeg = new RVBoundary(segNum, lastNode, intersectNodes[i], 0, 0);
                                AddBoundarySegment(boundSeg);
                                intersectNodes[j] = null;
                                triangle = (RVTriangle)triangle.GetOwner(i);
                            }
                        }

                        else
                        {
                            double xDir = intersectNodes[j].X - intersectNodes[i].X;
                            double yDir = intersectNodes[j].Y - intersectNodes[i].Y;
                            double qxAvg = (intersectNodes[j].GetPapam(4) + intersectNodes[i].GetPapam(4)) / 2;
                            double qyAvg = (intersectNodes[j].GetPapam(5) + intersectNodes[i].GetPapam(5)) / 2;
                            double dotProd = xDir * qxAvg + yDir * qyAvg;

                            if (dotProd > 0)
                            {
                                if (flag == 0)
                                {
                                    lastNode = (RVNodeHabitat)nodesList.LastItem();
                                    AddNode(intersectNodes[j]);
                                    if (boundarySegmentsList.FirstItem() == null) segNum = 1;
                                    else segNum = boundarySegmentsList.Count + 1;
                                    boundSeg = new RVBoundary(segNum, lastNode, intersectNodes[j], 0, 0);
                                    AddBoundarySegment(boundSeg);
                                    intersectNodes[i] = null;
                                    triangle = (RVTriangle)triangle.GetOwner(j);
                                }
                                else
                                {
                                    lastNode = (RVNodeHabitat)nodesList.LastItem();
                                    AddNode(intersectNodes[i]);
                                    if (boundarySegmentsList.FirstItem() == null) segNum = 1;
                                    else segNum = boundarySegmentsList.Count + 1;
                                    boundSeg = new RVBoundary(segNum, lastNode, intersectNodes[i], 0, 0);
                                    AddBoundarySegment(boundSeg);
                                    intersectNodes[j] = null;
                                    triangle = (RVTriangle)triangle.GetOwner(i);

                                }
                            }
                            else
                            {
                                if (flag == 0)
                                {
                                    lastNode = (RVNodeHabitat)nodesList.LastItem();
                                    AddNode(intersectNodes[i]);
                                    if (boundarySegmentsList.FirstItem() == null)
                                        segNum = 1;
                                    else segNum = boundarySegmentsList.Count + 1;
                                    boundSeg = new RVBoundary(segNum, lastNode, intersectNodes[i], 0, 0);
                                    AddBoundarySegment(boundSeg);
                                    intersectNodes[j] = null;
                                    triangle = (RVTriangle)triangle.GetOwner(i);
                                }
                                else
                                {

                                    lastNode = (RVNodeHabitat)nodesList.LastItem();
                                    AddNode(intersectNodes[j]);
                                    if (boundarySegmentsList.FirstItem() == null)
                                        segNum = 1;
                                    else segNum = boundarySegmentsList.Count + 1;
                                    boundSeg = new RVBoundary(segNum, lastNode, intersectNodes[j], 0, 0);
                                    AddBoundarySegment(boundSeg);
                                    intersectNodes[i] = null;
                                    triangle = (RVTriangle)triangle.GetOwner(j);
                                }
                            }
                        }
                    }
                }

            }

            for (int i = 0; i < 3; i++)
            {
                elementSegs[i] = null;
            }

            return triangle;
        }

        public void resampleBoundary(double spacing)
        {
            RVNodeHabitat hNP1 = null, hNP2 = null, hNP3 = null, hNP4 = null, tempHNP1, tempHNP2, newHNP = null, newStartHNP, lastNodeP, oldNodeP;
            RVBoundary boundSeg;
            double lowerArc = 0, upperArc = 0, noOfNodesExact;
            RVSegment tempSeg;
            double remainder, segmentsListength, incrementLength, r, wse, dischargeIntensity;
            int noOfNodes, noOfNodesAppended, segNum;

            //get the corner CountNodes of the boundary

            hNP1 = (RVNodeHabitat)nodesList.FirstItem();
            hNP4 = (RVNodeHabitat)nodesList.LastItem();

            for (int i = 1; i <= boundarySegmentsList.Count; i++)
            {
                boundSeg = (RVBoundary)boundarySegmentsList.GetIndexItem(i);
                if (boundSeg.getBcCode() == 3)
                {
                    hNP2 = (RVNodeHabitat)boundSeg.GetNode(0);
                    hNP3 = (RVNodeHabitat)boundSeg.GetNode(1);
                }

            }

            //delete the boundary segments in the list, will create a new ones

            while (boundarySegmentsList.Count > 0)
            {
                boundSeg = (RVBoundary)boundarySegmentsList.Pop();
                boundSeg = null;
            }

            //find the length of the upper and lower streamlines

            tempHNP1 = hNP1;
            nodesList.SetCurrentItem(tempHNP1);
            tempHNP2 = (RVNodeHabitat)nodesList.NextItem();

            while (tempHNP1 != hNP2)
            {
                lowerArc += tempHNP1.Distance(tempHNP2);
                tempHNP1 = tempHNP2;
                nodesList.SetCurrentItem(tempHNP1);
                tempHNP2 = (RVNodeHabitat)nodesList.NextItem();
            }

            tempHNP1 = hNP3;
            nodesList.SetCurrentItem(tempHNP1);
            tempHNP2 = (RVNodeHabitat)nodesList.NextItem();

            while (tempHNP1 != hNP4)
            {
                upperArc += tempHNP1.Distance(tempHNP2);
                tempHNP1 = tempHNP2;
                nodesList.SetCurrentItem(tempHNP1);
                tempHNP2 = (RVNodeHabitat)nodesList.NextItem();
            }

            //finding the exact spacing between the CountNodes for the lower streamline
            //based on the user defined spacing

            noOfNodesExact = lowerArc / spacing;
            noOfNodes = (int)(noOfNodesExact);
            spacing = lowerArc / noOfNodes;

            //setting the first node at the beginning of the lower streamline 
            //identical to hNP1 except that it is fixed
            //and initiating some variables for the while loop

            tempHNP1 = hNP1;
            nodesList.SetCurrentItem(tempHNP1);
            tempHNP2 = (RVNodeHabitat)nodesList.NextItem();
            tempSeg = new RVSegment(1, tempHNP1, tempHNP2);
            newStartHNP = (RVNodeHabitat)physics.CreateNewNode(1, 100, 100);
            tempSeg.LocateNodeAndInterpolation(0, newStartHNP);
            newStartHNP.Fixed = RVFixedNodeFlag.fixedNode;
            AddNode(newStartHNP);

            incrementLength = spacing;
            remainder = 0;
            noOfNodesAppended = 0;

            //looping through all of the CountNodes in the lower streamline, appending 
            //new CountNodes at the new spacing and boundary segments between the CountNodes

            while (tempHNP1 != hNP2)
            {
                tempSeg.SetNode(0, tempHNP1);
                tempSeg.SetNode(1, tempHNP2);
                segmentsListength = tempSeg.length();

                while (segmentsListength >= incrementLength)
                {
                    if (noOfNodesAppended == (noOfNodes - 1)) break;
                    r = incrementLength / segmentsListength;
                    newHNP = (RVNodeHabitat)physics.CreateNewNode(1, 100, 100);
                    tempSeg.LocateNodeAndInterpolation(r, newHNP);
                    newHNP.Fixed = RVFixedNodeFlag.fixedNode;
                    lastNodeP = (RVNodeHabitat)nodesList.LastItem();
                    AddNode(newHNP);
                    noOfNodesAppended += 1;
                    if (boundarySegmentsList.FirstItem() == null) segNum = 1;
                    else segNum = boundarySegmentsList.Count + 1;
                    boundSeg = new RVBoundary(segNum, lastNodeP, newHNP, 0, 0);
                    AddBoundarySegment(boundSeg);
                    incrementLength += spacing;

                }
                remainder = incrementLength - segmentsListength;
                incrementLength = remainder;

                tempHNP1 = tempHNP2;
                nodesList.SetCurrentItem(tempHNP1);
                tempHNP2 = (RVNodeHabitat)nodesList.NextItem();
            }

            //appending a node at the end of the lower streamline - same as hNP2 but fixed
            //appending the last boundary segment in the streamline

            lastNodeP = newHNP;
            tempSeg = new RVSegment(1, tempHNP1, tempHNP2);
            newHNP = (RVNodeHabitat)physics.CreateNewNode(1, 100, 100);
            tempSeg.LocateNodeAndInterpolation(0, newHNP);  //may want to delete tempSeg when done with it
            newHNP.Fixed = RVFixedNodeFlag.fixedNode;
            AddNode(newHNP);
            segNum = boundarySegmentsList.Count + 1;
            boundSeg = new RVBoundary(segNum, lastNodeP, newHNP, 0, 0);
            AddBoundarySegment(boundSeg);

            //appending the first node in the upper streamline (starting at the downstream end)
            //same as hNP3 and appending the outflow boundary segment

            lastNodeP = newHNP;
            newHNP = (RVNodeHabitat)physics.CreateNewNode(1, 100, 100);
            tempSeg.LocateNodeAndInterpolation(1, newHNP);
            newHNP.Fixed = RVFixedNodeFlag.fixedNode;
            AddNode(newHNP);
            segNum = boundarySegmentsList.Count + 1;
            wse = (lastNodeP.GetPapam(6) + newHNP.GetPapam(6)) / 2;
            boundSeg = new RVBoundary(segNum, lastNodeP, newHNP, 3, wse);
            AddBoundarySegment(boundSeg);

            //finding the exact spacing between the CountNodes for the upper streamline
            //based on the user defined spacing

            noOfNodesExact = upperArc / spacing;
            noOfNodes = (int)(noOfNodesExact);
            spacing = upperArc / noOfNodes;

            //initiating some variables for the while loop

            tempHNP1 = hNP3;
            nodesList.SetCurrentItem(tempHNP1);
            tempHNP2 = (RVNodeHabitat)nodesList.NextItem();
            tempSeg = new RVSegment(1, tempHNP1, tempHNP2);

            incrementLength = spacing;
            remainder = 0;
            noOfNodesAppended = 0;

            while (tempHNP1 != hNP4)
            {
                tempSeg.SetNode(0, tempHNP1);
                tempSeg.SetNode(1, tempHNP2);
                segmentsListength = tempSeg.length();

                while (segmentsListength >= incrementLength)
                {
                    if (noOfNodesAppended == (noOfNodes - 1)) break;
                    r = incrementLength / segmentsListength;
                    newHNP = (RVNodeHabitat)physics.CreateNewNode(1, 100, 100);
                    tempSeg.LocateNodeAndInterpolation(r, newHNP);
                    newHNP.Fixed = RVFixedNodeFlag.fixedNode;
                    lastNodeP = (RVNodeHabitat)nodesList.LastItem();
                    AddNode(newHNP);
                    noOfNodesAppended += 1;
                    if (boundarySegmentsList.FirstItem() == null) segNum = 1;
                    else segNum = boundarySegmentsList.Count + 1;
                    boundSeg = new RVBoundary(segNum, lastNodeP, newHNP, 0, 0);
                    AddBoundarySegment(boundSeg);
                    incrementLength += spacing;

                }
                remainder = incrementLength - segmentsListength;
                incrementLength = remainder;

                tempHNP1 = tempHNP2;
                nodesList.SetCurrentItem(tempHNP1);
                tempHNP2 = (RVNodeHabitat)nodesList.NextItem();
            }

            //appending a node at the end of the upper streamline - same as hNP4 but fixed
            //appending the last boundary segment in the streamline

            lastNodeP = newHNP;
            tempSeg = new RVSegment(1, tempHNP1, tempHNP2);
            newHNP = (RVNodeHabitat)physics.CreateNewNode(1, 100, 100);
            tempSeg.LocateNodeAndInterpolation(0, newHNP);
            newHNP.Fixed = RVFixedNodeFlag.fixedNode;
            AddNode(newHNP);
            segNum = boundarySegmentsList.Count + 1;
            boundSeg = new RVBoundary(segNum, lastNodeP, newHNP, 0, 0);
            AddBoundarySegment(boundSeg);

            //appending the outflow boundary segment

            dischargeIntensity = (newHNP.GetPapam(15) - newStartHNP.GetPapam(15)) / (newHNP.Distance(newStartHNP));
            segNum = boundarySegmentsList.Count + 1;
            boundSeg = new RVBoundary(segNum, newHNP, newStartHNP, 1, dischargeIntensity);
            AddBoundarySegment(boundSeg);

            oldNodeP = (RVNodeHabitat)nodesList.FirstItem();

            while (oldNodeP != null)
            {
                if (oldNodeP.Fixed != RVFixedNodeFlag.fixedNode)
                {
                    nodesList.Remove(oldNodeP);
                    oldNodeP = (RVNodeHabitat)nodesList.FirstItem();
                }
                else
                    oldNodeP = (RVNodeHabitat)nodesList.NextItem();
            }
        }

        public void mapBoundaryStreamLine(RVMeshIrregular meshTIN, RVNodeHabitat hNUSBound, RVNodeHabitat hNDSBound, int flag)
        {
            double dx, dy, d, dxLimit, dyLimit, dLimit, r1, r2, wse, Distance;
            double xPath, yPath;
            RVSegment USsegPathP, DSsegPathP;
            RVBoundary bsP, USbsP, DSbsP, nextbsP, newbsP;
            RVNodeHabitat hNUSPath, hNDSPath, hNstartP, hNendP, nP, lastNodeP;
            int segNum;

            RVBox limits = meshTIN.GetLimits();
            dxLimit = limits.x2 - limits.x1;
            dyLimit = limits.y2 - limits.y1;
            dLimit = Math.Sqrt(dxLimit * dxLimit + dyLimit * dyLimit);

            //get path for upstream end

            dx = hNUSBound.GetPapam(4);  // uses direction of discharge intensity to get direction of path
            dy = hNUSBound.GetPapam(5);
            d = Math.Sqrt(dx * dx + dy * dy);

            if (flag == 0)      //to get path to lower boundary node
            {
                xPath = (dLimit / d) * dy + hNUSBound.X;
                yPath = -(dLimit / d) * dx + hNUSBound.Y;
            }
            else                // to get path to upper boundary node
            {
                xPath = -(dLimit / d) * dy + hNUSBound.X;
                yPath = (dLimit / d) * dx + hNUSBound.Y;
            }

            hNUSPath = (RVNodeHabitat)physics.CreateNewNode(1, xPath, yPath);
            USsegPathP = new RVSegment(1, hNUSBound, hNUSPath);

            //get path for downstream end

            dx = hNDSBound.GetPapam(4);  // uses direction of discharge intensity to get direction of path
            dy = hNDSBound.GetPapam(5);
            d = Math.Sqrt(dx * dx + dy * dy);

            if (flag == 0)      //to get path to lower boundary node
            {
                xPath = (dLimit / d) * dy + hNDSBound.X;
                yPath = -(dLimit / d) * dx + hNDSBound.Y;
            }
            else                // to get path to upper boundary node
            {
                xPath = -(dLimit / d) * dy + hNDSBound.X;
                yPath = (dLimit / d) * dx + hNDSBound.Y;
            }

            hNDSPath = (RVNodeHabitat)physics.CreateNewNode(1, xPath, yPath);
            DSsegPathP = new RVSegment(1, hNDSBound, hNDSPath);

            //find the boundary element that is intersected by the upstream path
            //the one that is first intersected (closest)

            bsP = (RVBoundary)meshTIN.FirstBoundarySegment;
            USbsP = bsP;

            Distance = 10e10;

            while (bsP != null)
            {
                r1 = bsP.intersectd(USsegPathP);
                r2 = USsegPathP.intersectd(bsP);

                if ((r1 >= 0) && (r1 <= 1.0) && (r2 >= 0) && (r2 <= 1.0))
                    if ((bsP.GetNode(0).Distance(USsegPathP.GetNode(0))) < Distance)
                    {
                        Distance = bsP.GetNode(0).Distance(USsegPathP.GetNode(0));
                        USbsP = bsP;
                    }
                bsP = (RVBoundary)meshTIN.NextBoundarySegment;

            }

            //find the boundary element that is intersected by the downstream path
            //the one that is first intersected (closest)

            bsP = (RVBoundary)meshTIN.FirstBoundarySegment;
            DSbsP = bsP;

            Distance = 10e10;

            while (bsP != null)
            {
                r1 = bsP.intersectd(DSsegPathP);
                r2 = DSsegPathP.intersectd(bsP);

                if ((r1 >= 0) && (r1 <= 1.0) && (r2 >= 0) && (r2 <= 1.0))
                    if ((bsP.GetNode(0).Distance(DSsegPathP.GetNode(0))) < Distance)
                    {
                        Distance = bsP.GetNode(0).Distance(DSsegPathP.GetNode(0));
                        DSbsP = bsP;
                    }
                bsP = (RVBoundary)meshTIN.NextBoundarySegment;

            }

            //getting the starting and end CountNodes in the psi = 0 streamline

            if (flag == 0)
            {
                hNstartP = (RVNodeHabitat)physics.CreateNewNode(1, 100, 100);
                r1 = USbsP.intersectd(USsegPathP);
                USbsP.LocateNodeAndInterpolation(r1, hNstartP);
                hNendP = (RVNodeHabitat)physics.CreateNewNode(1, 100, 100);
                r1 = DSbsP.intersectd(DSsegPathP);
                DSbsP.LocateNodeAndInterpolation(r1, hNendP);
            }

            //getting the starting and end CountNodes int the psi = Qtotal streamline

            else
            {
                hNstartP = (RVNodeHabitat)physics.CreateNewNode(1, 100, 100);
                r1 = DSbsP.intersectd(DSsegPathP);
                DSbsP.LocateNodeAndInterpolation(r1, hNstartP);
                hNendP = (RVNodeHabitat)physics.CreateNewNode(1, 100, 100);
                r1 = USbsP.intersectd(USsegPathP);
                USbsP.LocateNodeAndInterpolation(r1, hNendP);
            }

            //mapping the streamline that is along the right boundary

            if (flag == 0)
            {
                AddNode(hNstartP);
                bsP = USbsP;

                while (bsP != DSbsP)
                {
                    lastNodeP = (RVNodeHabitat)nodesList.LastItem();
                    nP = (RVNodeHabitat)physics.CreateNewNode(1, 100, 100);
                    bsP.LocateNodeAndInterpolation(1, nP);
                    AddNode(nP);
                    if (boundarySegmentsList.FirstItem() == null) segNum = 1;
                    else segNum = boundarySegmentsList.Count + 1;
                    newbsP = new RVBoundary(segNum, lastNodeP, nP, 0, 0);
                    AddBoundarySegment(newbsP);
                    nextbsP = (RVBoundary)meshTIN.NextBoundarySegment;
                    if (nextbsP == null)
                        nextbsP = (RVBoundary)meshTIN.FirstBoundarySegment;
                    if (nextbsP.GetNode(0) == bsP.GetNode(1))
                        bsP = nextbsP;
                    else
                    {
                        while (nextbsP.GetNode(0) != bsP.GetNode(1))
                        {
                            nextbsP = (RVBoundary)meshTIN.NextBoundarySegment;
                            if (nextbsP == null)
                                nextbsP = (RVBoundary)meshTIN.FirstBoundarySegment;
                        }
                        bsP = nextbsP;
                    }
                }
            }
            // mapping the streamline that is along left boundary
            else
            {
                lastNodeP = (RVNodeHabitat)nodesList.LastItem();
                AddNode(hNstartP);
                wse = (lastNodeP.GetPapam(6) + hNstartP.GetPapam(6)) / 2;
                if (boundarySegmentsList.FirstItem() == null) segNum = 1;
                else segNum = boundarySegmentsList.Count + 1;
                bsP = new RVBoundary(segNum, lastNodeP, hNstartP, 3, wse);
                AddBoundarySegment(bsP);
                bsP = DSbsP;
                while (bsP != USbsP)
                {
                    lastNodeP = (RVNodeHabitat)nodesList.LastItem();
                    nP = (RVNodeHabitat)physics.CreateNewNode(1, 100, 100);
                    bsP.LocateNodeAndInterpolation(1, nP);
                    AddNode(nP);
                    if (boundarySegmentsList.FirstItem() == null) segNum = 1;
                    else segNum = boundarySegmentsList.Count + 1;
                    newbsP = new RVBoundary(segNum, lastNodeP, nP, 0, 0);
                    AddBoundarySegment(newbsP);
                    nextbsP = (RVBoundary)meshTIN.NextBoundarySegment;
                    if (nextbsP == null)
                        nextbsP = (RVBoundary)meshTIN.FirstBoundarySegment;
                    if (nextbsP.GetNode(0) == bsP.GetNode(1))
                        bsP = nextbsP;
                    else
                    {
                        while (nextbsP.GetNode(0) != bsP.GetNode(1))
                        {
                            nextbsP = (RVBoundary)meshTIN.NextBoundarySegment;
                            if (nextbsP == null)
                                nextbsP = (RVBoundary)meshTIN.FirstBoundarySegment;
                        }
                        bsP = nextbsP;
                    }
                }
            }
            //appending last node and boundary element in streamline
            lastNodeP = (RVNodeHabitat)nodesList.LastItem();
            AddNode(hNendP);
            if (boundarySegmentsList.FirstItem() == null) segNum = 1;
            else segNum = boundarySegmentsList.Count + 1;
            bsP = new RVBoundary(segNum, lastNodeP, hNendP, 0, 0);
            AddBoundarySegment(bsP);
        }

        public RVTriangle findTriAndNodeWhenVelDirCannot(RVMeshIrregular meshTIN, RVTriangle triangle, double boundaryValue, int flag, int parChoice)
        {
            RVNodeHabitat[] intersectNodes = new RVNodeHabitat[3];
            RVNodeHabitat lastNode;
            RVBoundary lastSeg;
            RVSegment[] elementSegs = new RVSegment[3];
            RVBoundary boundSeg;
            int segNum, i, j, k;

            //velocity vector has led us astray so last node and segment are not what we want
            //last node is essentially the same as the second last node and last segment is 
            //segment between these CountNodes
            //so first want to delete these from the lists

            lastNode = (RVNodeHabitat)nodesList.LastItem();
            nodesList.SetCurrentItem(lastNode);
            nodesList.RemoveCurrentItem();

            lastSeg = (RVBoundary)boundarySegmentsList.LastItem();
            boundarySegmentsList.SetCurrentItem(lastSeg);
            boundarySegmentsList.RemoveCurrentItem();

            //if not, get the next node and segment along the streamline to append to the list

            for (j = 0; j < 3; j++)
                intersectNodes[j] = null;

            for (k = 0; k < 3; k++)
            {
                i = k + 1;
                if (i == 3) i = 0;
                j = k + 2;
                if (j == 3) j = 0;
                if (j == 4) j = 1;
                elementSegs[k] = new RVSegment(1, triangle.GetNode(i), triangle.GetNode(j));
            }

            //find CountNodes where the streamline intersects the triangle triangle

            for (i = 0; i < 3; i++)
            {
                double parValue1, parValue2, r;

                parValue1 = elementSegs[i].GetNode(0).GetPapam(parChoice);
                parValue2 = elementSegs[i].GetNode(1).GetPapam(parChoice);

                //provisions for the case when the streamline intersects exactly
                //at a node - simply move the streamline to avoid the intersection at
                //a node

                if (parValue1 == boundaryValue || parValue2 == boundaryValue)
                {
                    if (flag == 0) boundaryValue = boundaryValue + 0.0001;
                    if (flag == 1) boundaryValue = boundaryValue - 0.0001;
                }

                if (((parValue1 > boundaryValue) && (parValue2 < boundaryValue)) || ((parValue1 < boundaryValue) && (parValue2 > boundaryValue)))
                {
                    if (parValue2 > parValue1)
                        r = (boundaryValue - parValue1) / (parValue2 - parValue1);
                    else if (parValue2 < parValue1)
                        r = 1.0 - (boundaryValue - parValue2) / (parValue1 - parValue2);
                    else r = 0.0;

                    intersectNodes[i] = (RVNodeHabitat)physics.CreateNewNode(1, 100, 100);
                    elementSegs[i].LocateNodeAndInterpolation(r, intersectNodes[i]);
                }
            }

            //determine which of the two intersection CountNodes is the next node to appended
            //opposite to what it was when I called mapStreamLine

            for (i = 0; i < 3; i++)
            {
                j = i + 1;
                if (j == 3) j = 0;
                if ((intersectNodes[i] != null) && (intersectNodes[j] != null))
                {
                    double xDir = intersectNodes[j].X - intersectNodes[i].X;
                    double yDir = intersectNodes[j].Y - intersectNodes[i].Y;
                    double qxAvg = (intersectNodes[j].GetPapam(4) + intersectNodes[i].GetPapam(4)) / 2;
                    double qyAvg = (intersectNodes[j].GetPapam(5) + intersectNodes[i].GetPapam(5)) / 2;
                    double dotProd = xDir * qxAvg + yDir * qyAvg;
                    if (dotProd < 0)
                    {
                        if (flag == 0)
                        {
                            lastNode = (RVNodeHabitat)nodesList.LastItem();
                            AddNode(intersectNodes[j]);
                            if (boundarySegmentsList.FirstItem() == null) segNum = 1;
                            else segNum = boundarySegmentsList.Count + 1;
                            boundSeg = new RVBoundary(segNum, lastNode, intersectNodes[j], 0, 0);
                            AddBoundarySegment(boundSeg);
                            intersectNodes[i] = null;
                            triangle = (RVTriangle)triangle.GetOwner(j);
                        }
                        else
                        {
                            lastNode = (RVNodeHabitat)nodesList.LastItem();
                            AddNode(intersectNodes[i]);
                            if (boundarySegmentsList.FirstItem() == null) segNum = 1;
                            else segNum = boundarySegmentsList.Count + 1;
                            boundSeg = new RVBoundary(segNum, lastNode, intersectNodes[i], 0, 0);
                            AddBoundarySegment(boundSeg);
                            intersectNodes[j] = null;
                            triangle = (RVTriangle)triangle.GetOwner(i);

                        }
                    }
                    else
                    {
                        if (flag == 0)
                        {
                            lastNode = (RVNodeHabitat)nodesList.LastItem();
                            AddNode(intersectNodes[i]);
                            if (boundarySegmentsList.FirstItem() == null) segNum = 1;
                            else segNum = boundarySegmentsList.Count + 1;
                            boundSeg = new RVBoundary(segNum, lastNode, intersectNodes[i], 0, 0);
                            AddBoundarySegment(boundSeg);
                            intersectNodes[j] = null;
                            triangle = (RVTriangle)triangle.GetOwner(i);
                        }
                        else
                        {
                            lastNode = (RVNodeHabitat)nodesList.LastItem();
                            AddNode(intersectNodes[j]);
                            if (boundarySegmentsList.FirstItem() == null) segNum = 1;
                            else segNum = boundarySegmentsList.Count + 1;
                            boundSeg = new RVBoundary(segNum, lastNode, intersectNodes[j], 0, 0);
                            AddBoundarySegment(boundSeg);
                            intersectNodes[i] = null;
                            triangle = (RVTriangle)triangle.GetOwner(j);
                        }
                    }

                }
            }


            for (i = 0; i < 3; i++)
            {
                elementSegs[i] = null;
            }

            return triangle;
        }

        // ======================================================================================
        //         WR
        // ======================================================================================
        #region  методы загрузки сетки формат cdg
        /// <summary>
        /// Чтение основных списков сетки
        /// </summary>
        /// <param name="file"></param>
        /// <param name="rvControl"></param>
        public void ReadBodyCDG(StreamReader file, RVControl rvControl)
        {
            this.CountNodes = rvControl.CountNodes;
            this.CountElements = rvControl.CountElements;
            this.CountBoundElements = rvControl.CountBoundElements;
            this.CountBoundSegs = rvControl.CountBoundSegs;
            for (int i = 0; i < CountNodes; i++)
                GetNode(file);
            for (int j = 0; j < CountElements; j++)
                getElm(file);
            for (int k = 0; k < CountBoundElements; k++)
                getBElm(file);
            for (int l = 0; l < CountBoundSegs; l++)
                getBSeg(file);
            ReadBreakLinesList(file);
        }
        /// <summary>
        /// Чтение узлов
        /// </summary>
        /// <param name="f"></param>
        public void GetNode(StreamReader f)
        {
            string[] lines = RVcdgIO.GetLines(f);
            if (lines == null) return;
            int idx = 0;
            int n = int.Parse(lines[idx++].Trim());
            string s = lines[idx].Trim();
            RVFixedNodeFlag fx = RVFixedNodeFlag.floatingNode;
            if (s == "x" || s == "s")
            {
                if (s == "x")
                    fx = RVFixedNodeFlag.fixedNode;   // x  фиксированный узел
                else
                    fx = RVFixedNodeFlag.slidingNode;  // s slidingNode - скользящий узел (по границе/ или линии)
                idx++;
            }
            double x = double.Parse(lines[idx++].Trim(), MEM.formatter);
            double y = double.Parse(lines[idx++].Trim(), MEM.formatter);
            double z = double.Parse(lines[idx++].Trim(), MEM.formatter);

            double k = double.Parse(lines[idx++].Trim(), MEM.formatter);
            double weight = double.Parse(lines[idx++].Trim(), MEM.formatter);
            double d = double.Parse(lines[idx++].Trim(), MEM.formatter);
            double qx = double.Parse(lines[idx++].Trim(), MEM.formatter);
            double qy = double.Parse(lines[idx++].Trim(), MEM.formatter);

            RVNodeHabitat nP = new RVNodeHabitat((RVHabitatPhysics)GetPhysics, n, x, y, z, k, weight, d, qx, qy);
            if (fx == RVFixedNodeFlag.fixedNode)
                nP.Fixed = RVFixedNodeFlag.fixedNode;
            if (fx == RVFixedNodeFlag.slidingNode)
                nP.Fixed = RVFixedNodeFlag.slidingNode;
            AddNode(nP);
            return;
        }
        public void getBElm(StreamReader f)
        {
            string[] lines = RVcdgIO.GetLines(f);
            if (lines == null) return;
            int n = int.Parse(lines[0].Trim());
            int typeBFV = int.Parse(lines[1].Trim());
            int typeBFL = int.Parse(lines[2].Trim());
            int nn1 = int.Parse(lines[3].Trim());
            int nn2 = int.Parse(lines[4].Trim());
            double p1 = double.Parse(lines[5].Trim(), MEM.formatter);
            double p2 = double.Parse(lines[6].Trim(), MEM.formatter);
            int c1 = int.Parse(lines[12].Trim());
            RVNode n1P = GetNodeByID(nn1);
            RVNode n2P = GetNodeByID(nn2);
            double bValue = 0.0;
            if (c1 == 1)
                bValue = p2;
            else if (c1 == 3)
                bValue = p1;
            RVBoundary bSeg = new RVBoundary(n, n1P, n2P, c1, bValue);
            AddBoundarySegment(bSeg);
            return;
        }
        public void getElm(StreamReader f)
        {
            string[] lines = RVcdgIO.GetLines(f);
            if(lines == null) return;
            int n = int.Parse(lines[0].Trim());
            int typeFFV = int.Parse(lines[1].Trim());
            int typeFFL = int.Parse(lines[2].Trim());
            // сдвиг на 1 нумерация узлов идет с нуля
            uint nn1 = uint.Parse(lines[3].Trim());
            uint nn2 = uint.Parse(lines[4].Trim());
            uint nn3 = uint.Parse(lines[5].Trim());
            return;
        }
        public void getBSeg(StreamReader f)
        {
            string[] lines = RVcdgIO.GetLines(f);
            if (lines == null) return;
            int n = int.Parse(lines[0].Trim());
            int code = int.Parse(lines[1].Trim());
            double p0 = double.Parse(lines[2].Trim(), MEM.formatter);
            double p1 = double.Parse(lines[3].Trim(), MEM.formatter);
            int startnode = int.Parse(lines[4].Trim());
            int endnode = int.Parse(lines[5].Trim());
            string filePath = "";
            if (lines.Length == 7)
                filePath = lines[6].Trim();
            int transCode;
            if (filePath != "")
                transCode = 0;
            else
                transCode = 1;
            RVNode start = GetNodeByID(startnode);
            RVNode end = GetNodeByID(endnode);
            RVFlowBoundary flowBP = new RVFlowBoundary(n, start, end, code, p0, p1, transCode, filePath);
            FlowBoundList.Add(flowBP);
            SetBElementFlowBound(flowBP, start, end);
            if (filePath != "")
                using (StreamReader file = new StreamReader(filePath))
                {
                    if (file != null)
                        flowBP.loadTransLst(file);
                }
        }

        public RVFlowBoundary FirstFBound => (RVFlowBoundary)FlowBoundList.FirstItem(); 
        public RVFlowBoundary lastFBound() { return (RVFlowBoundary)FlowBoundList.LastItem(); }
        public RVFlowBoundary nextFBound() { return (RVFlowBoundary)FlowBoundList.NextItem(); }
        public void appendFBound(RVFlowBoundary fBP)
        {
            FlowBoundList.Add(fBP);
            FlowBoundList.BuildIndex();
            CountBoundSegs++;
        }
        public void removeFBound(RVFlowBoundary fBP)
        {
            FlowBoundList.Remove(fBP);
            FlowBoundList.BuildIndex();
            CountBoundSegs--;
            fBP = FirstFBound;
            int i = 1;
            while (fBP != null)
            {
                fBP.ID = i;
                i++;
                fBP = nextFBound();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void BuildFlowBoundaries()
        {
            int Nseg = 0, oldBcCode = 0, count = 0;
            double sum = 0.0;
            RVBoundary bSegment, startBP = new RVBoundary(), oldBP = new RVBoundary();
            RVFlowBoundary fBP;

            bSegment = (RVBoundary)FirstBoundarySegment;
            while (bSegment != null)
            {
                if (bSegment.getBcCode() != oldBcCode)
                {
                    if (oldBcCode == 1)
                    {
                        Nseg += 1;
                        fBP = new RVFlowBoundary(Nseg, startBP.GetNode(0), oldBP.GetNode(1), 1, 0, sum);
                        FlowBoundList.Add(fBP);
                        CountBoundSegs++;
                    }
                    if (oldBcCode == 3)
                    {
                        Nseg += 1;
                        fBP = new RVFlowBoundary(Nseg, startBP.GetNode(0), oldBP.GetNode(1), 3, startBP.BcValue, 0);
                        FlowBoundList.Add(fBP);
                        CountBoundSegs++;
                    }
                    if (oldBcCode == 5)
                    {
                        Nseg += 1;
                        fBP = new RVFlowBoundary(Nseg, startBP.GetNode(0), oldBP.GetNode(1), 5, startBP.BcValue, startBP.BcValueTwo);
                        FlowBoundList.Add(fBP);
                        CountBoundSegs++;
                    }
                    oldBcCode = bSegment.getBcCode();
                    startBP = bSegment;
                    count = 0;
                    sum = 0.0;
                }
                count += 1;
                sum += bSegment.BcValue * bSegment.length();
                oldBP = bSegment;
                bSegment = (RVBoundary)NextBoundarySegment;
            }
            if (oldBcCode == 1)
            {
                Nseg += 1;
                fBP = new RVFlowBoundary(Nseg, startBP.GetNode(0), oldBP.GetNode(1), 1, 0, sum);
                FlowBoundList.Add(fBP);
                CountBoundSegs++;
            }
            if (oldBcCode == 3)
            {
                Nseg += 1;
                fBP = new RVFlowBoundary(Nseg, startBP.GetNode(0), oldBP.GetNode(1), 3, startBP.BcValue, 0);
                FlowBoundList.Add(fBP);
                CountBoundSegs++;
            }
            if (oldBcCode == 5)
            {
                Nseg += 1;
                fBP = new RVFlowBoundary(Nseg, startBP.GetNode(0), oldBP.GetNode(1), 5, startBP.BcValue, startBP.BcValueTwo);
                FlowBoundList.Add(fBP);
                CountBoundSegs++;
            }
        }
        public void SetBElementFlowBound(RVFlowBoundary fBP, RVNode startNP, RVNode endNP)
        {
            RVBoundary segP;
            segP = (RVBoundary)FirstBoundarySegment;
            if (segP == null)
                return;
            while (segP.GetNode(0) != startNP)
            {
                segP = (RVBoundary)NextBoundarySegment;
                if (segP == null)
                    segP = (RVBoundary)FirstBoundarySegment;
            }
            segP.FlowBound = fBP;
            while (segP.GetNode(1) != endNP)
            {
                segP = (RVBoundary)NextBoundarySegment;
                if (segP == null)
                    segP = (RVBoundary)FirstBoundarySegment;
                segP.FlowBound = fBP;
            }
            return;
        }
        #endregion
        
        #region  новые методы загрузки сетки формат msh
        /// <summary>
        /// чтение формата sdg
        /// </summary>
        /// <param name="file"></param>
        public void ReadLMesh_sdg(StreamReader file)
        {
            string version = file.ReadLine();
            if (VersionLMesh == version)
            {
                CountNodes = RVcdgIO.GetInt(file);
                CountBoundElements = RVcdgIO.GetInt(file);
                CountBoundSegs = RVcdgIO.GetInt(file);
                // Чтение узлов
                for (int i = 0; i < CountNodes; i++)
                    ReadNode(file);
                for (int k = 0; k < CountBoundElements; k++)
                    ReadBElm(file);
                for (int l = 0; l < CountBoundSegs; l++)
                    ReadBSegments(file);
                ReadBreakLinesList(file);
            }
            else
                Logger.Instance.Info("не согласованность версий данных: " + VersionLMesh + " " + version);
        }
        /// <summary>
        /// Чтение узлов в формате sdg
        /// </summary>
        /// <param name="f"></param>
        public void ReadNode(StreamReader f)
        {
            string[] lines = RVcdgIO.GetLines(f);
            if (lines == null) return;
            int idx = 0;
            int n = int.Parse(lines[idx++].Trim());
            string s = lines[idx].Trim();
            RVFixedNodeFlag fx = RVFixedNodeFlag.floatingNode;
            if (s == "x" || s == "s")
            {
                if (s == "x")
                    fx = RVFixedNodeFlag.fixedNode;   // x  фиксированный узел
                else
                    fx = RVFixedNodeFlag.slidingNode;  // s slidingNode - скользящий узел (по границе/ или линии)
                idx++;
            }
            double x = double.Parse(lines[idx++].Trim(), MEM.formatter);
            double y = double.Parse(lines[idx++].Trim(), MEM.formatter);
            double z = double.Parse(lines[idx++].Trim(), MEM.formatter);

            double ks = double.Parse(lines[idx++].Trim(), MEM.formatter);
            double depth = double.Parse(lines[idx++].Trim(), MEM.formatter);
            double qx = double.Parse(lines[idx++].Trim(), MEM.formatter);
            double qy = double.Parse(lines[idx++].Trim(), MEM.formatter);

            RVNodeHabitat nP = new RVNodeHabitat((RVHabitatPhysics)GetPhysics, n, x, y, z, ks, 5.0, depth, qx, qy);
            if (fx == RVFixedNodeFlag.fixedNode)
                nP.Fixed = RVFixedNodeFlag.fixedNode;
            if (fx == RVFixedNodeFlag.slidingNode)
                nP.Fixed = RVFixedNodeFlag.slidingNode;
            AddNode(nP);
            return;
        }
        /// <summary>
        /// чтение граничных элементов в формате sdg
        /// </summary>
        /// <param name="f"></param>
        public void ReadBElm(StreamReader f)
        {
            string[] lines = RVcdgIO.GetLines(f);
            if (lines == null) return;
            int n = int.Parse(lines[0].Trim());
            int nn1 = int.Parse(lines[1].Trim());
            int nn2 = int.Parse(lines[2].Trim());
            // значения
            double p1 = double.Parse(lines[3].Trim(), MEM.formatter);
            double p2 = double.Parse(lines[4].Trim(), MEM.formatter);
            // флаг
            int c1 = int.Parse(lines[5].Trim());
            RVNode n1P = GetNodeByID(nn1);
            RVNode n2P = GetNodeByID(nn2);
            double bValue = 0.0;
            if (c1 == 1)
                bValue = p2;
            else if (c1 == 3)
                bValue = p1;
            RVBoundary bSeg = new RVBoundary(n, n1P, n2P, c1, bValue);
            AddBoundarySegment(bSeg);
            return;
        }
        /// <summary>
        /// чтение граничных сегментов в формате sdg
        /// </summary>
        /// <param name="f"></param>
        public void ReadBSegments(StreamReader f)
        {
            string[] lines = RVcdgIO.GetLines(f);
            if (lines == null) return;
            int n = int.Parse(lines[0].Trim());
            int code = int.Parse(lines[1].Trim());
            double p0 = double.Parse(lines[2].Trim(), MEM.formatter);
            double p1 = double.Parse(lines[3].Trim(), MEM.formatter);
            int startnode = int.Parse(lines[4].Trim());
            int endnode = int.Parse(lines[5].Trim());
            string filePath = "";
            if (lines.Length == 7)
                filePath = lines[6].Trim();
            int transCode;
            if (filePath != "")
                transCode = 0;
            else
                transCode = 1;
            RVNode start = GetNodeByID(startnode);
            RVNode end = GetNodeByID(endnode);
            RVFlowBoundary flowBP = new RVFlowBoundary(n, start, end, code, p0, p1, transCode, filePath);
            FlowBoundList.Add(flowBP);
            SetBElementFlowBound(flowBP, start, end);
            if (filePath != "")
                using (StreamReader file = new StreamReader(filePath))
                {
                    if (file != null)
                        flowBP.loadTransLst(file);
                }
        }
        protected double GetWsElev()
        {
            int countOut = 0;
            double elOut = 0.0;
            RVBoundary bSegment = (RVBoundary)FirstBoundarySegment;
            while (bSegment != null)
            {
                if ((bSegment.getBcCode() == 3) || (bSegment.getBcCode() == 5))
                {
                    outSegP = bSegment;
                    countOut += 1;
                    elOut += bSegment.BcValue;
                }
                else if (bSegment.getBcCode() == 1)
                {
                    inSegP = bSegment;
                }
                bSegment = (RVBoundary)NextBoundarySegment;
            }
            if (countOut > 0)
                return elOut / countOut;
            else
                return 0.0;
        }
        /// <summary>
        /// Запись в формате sdg
        /// </summary>
        /// <param name="file"></param>
        public void writeLMesh_sdg(StreamWriter file)
        {
            file.WriteLine("Ver 18.11.2021");
            file.WriteLine("Count of Nodes = " + CountNodes.ToString());
            RVTriangle currentFElement = FirstTriElements;
            int eCount = 0;
            while (currentFElement != null)
            {
                if (currentFElement.Status == StatusFlag.Activate)
                    eCount += 1; ;
                currentFElement = NextTriElements;
            }
            // file.WriteLine("Number of Elements = " + eCount.ToString());
            file.WriteLine("Count of Boundary Elements = " + CountBoundarySegments.ToString());
            file.WriteLine("Count of Boundary Segments = " + CountBoundSegs.ToString());
            //
            file.WriteLine("Node ID, flag, Coordinates: X Y Z, Parameter: ks, Variables: dipth qx qy");
            wsElevOut = GetWsElev();
            RVNodeShallowWater elemNodes = (RVNodeShallowWater)firstNode;
            while (elemNodes != null)
            {
                writeNode(file, elemNodes);
                elemNodes = (RVNodeShallowWater)NextNode;
            }
            file.WriteLine("Boundary RVElement ID, Nodes n1 n2, boundary condition val1 val2 flag");
            RVBoundary currentBoundFElement = (RVBoundary)FirstBoundarySegment;
            while (currentBoundFElement != null)
            {
                writeBElm(file, currentBoundFElement);
                currentBoundFElement = (RVBoundary)NextBoundarySegment;
            }
            file.WriteLine("Boundary RVSegment ID,Boundary type, va1, val2, start node ID,end node ID");
            writeBSegs(file);
            WriteBreakLinesList(file);
            return;
        }
        /// <summary>
        /// Запись узла в формате sdg
        /// </summary>
        /// <param name="file"></param>
        /// <param name="nP"></param>
        void writeNode(StreamWriter file, RVNodeShallowWater nP)
        {
            file.Write(nP.ID.ToString() + "  ");     // Number nods
            if (nP.Fixed == RVFixedNodeFlag.fixedNode)    // flag
                file.Write(" x ");
            else if (nP.Fixed == RVFixedNodeFlag.slidingNode)
                file.Write(" s ");
            else
                file.Write("   ");
            file.Write(nP.Xo.ToString(F8) + "  ");  // x
            file.Write(nP.Yo.ToString(F8) + "  ");  // y
            file.Write(nP.Z.ToString(F6) + "  ");  // z
            file.Write(nP.Ks.ToString(F6) + "  ");  // ks
            // depth qx qy
            string zerro = 0.ToString(F6);
            if (wsElevOut <= 0.0)
            {
                file.Write(wsElevIn.ToString(F6));
                file.Write(zerro);
                file.Write(zerro);
            }
            else if (wsElevIn > 0.0)
            {
                double dout = Math.Abs(outSegP.whichSide(nP)) / outSegP.length();
                double din = Math.Abs(inSegP.whichSide(nP)) / inSegP.length();
                double r = dout / (din + dout);
                file.Write((r * wsElevIn + (1 - r) * wsElevOut - (nP.Z)).ToString(F6) + "  ");
                file.Write(zerro + " " + zerro);
            }
            else
            {
                file.Write(nP.Depth.ToString(F6) + "  ");    // depth
                file.Write(nP.GetPapam(4).ToString(F6) + "  "); // qx
                file.Write(nP.GetPapam(5).ToString(F6) + "  "); // qy
            }
            file.WriteLine();
        }
        void writeBElm(StreamWriter file, RVBoundary bSegment)
        {
            // номер узла
            file.Write(bSegment.ID.ToString() + "  ");
            // первый узел
            file.Write(bSegment.GetNode(0).ID.ToString() + "  ");
            // второй узел
            file.Write(bSegment.GetNode(1).ID.ToString() + "  ");
            double a = 0, b = 0;
            int c = 0;
            if (bSegment.getBcCode() == 1)
            { 
                b = bSegment.BcValue; c = 1; 
            }
            else if (bSegment.getBcCode() == 3)
            {
                a = bSegment.BcValue; c = 3;
            }
            else if (bSegment.getBcCode() == 5)
            {
                a = bSegment.BcValue; b = bSegment.BcValueTwo; c = 5;
            }
            file.Write(a.ToString(F6) + " " + b.ToString(F6) + " " + c.ToString());
            file.WriteLine();
        }
        void writeBSegs(StreamWriter file)
        {
            RVFlowBoundary fBP = FirstFBound;
            while (fBP != null)
            {
                file.Write(fBP.ID.ToString() + "  ");
                file.Write(fBP.ID.ToString() + "  ");
                file.Write(fBP.getBcValue().ToString(F6) + "  ");
                file.Write(fBP.getBcValue2().ToString(F6) + "  ");
                file.Write((((RVNode)fBP.getStartNode()).ID).ToString() + "  ");
                file.Write(((RVNode)fBP.getEndNode()).ID.ToString() + "  ");
                if (fBP.getTransCode() == 0)
                    file.Write("  ");
                else
                    file.Write("  " + fBP.getFilePath().ToString() + "  ");
                file.WriteLine();
                fBP = nextFBound();
            }
        }
        #endregion


        #region методы расширения / для редактора сетки
        //	Refines a RVMeshRiver by placing a new node in the centre of each triangle
        //	that adheres to a particular critieria 
        //	Requires a previously triangulated bedMesh (bed data)
        //	which masks inactive areas and provides interpolation data.
        //	Resulting set of CountNodes are in tmpNodesList which can be later accepted into nodesList or rejected.
        //	The number of CountNodes created are returned.
        /// <summary>
        /// Уточняет RVMeshRiver, помещая новый узел в центр каждого треугольника
        /// который соответствует определенным критериям
        /// Требуется предварительно триангулированный bedMesh (bed data)
        /// который маскирует неактивные области и предоставляет данные интерполяции.
        /// Результирующий набор CountNodes находится в tmpNodesList, который позже может быть принят в nodesList или отклонен.
        /// Возвращается количество созданных CountNodes.
        /// </summary>
        /// <param name="boundTIN"></param>
        /// <param name="bedMesh"></param>
        /// <param name="lowerlimit"></param>
        /// <param name="upperlimit"></param>
        /// <param name="parNum"></param>
        /// <param name="valueOrChange"></param>
        /// <returns></returns>
        public int AutoRefineHabitatRegion(RVMeshIrregular boundTIN, RVMeshIrregular bedMesh, double lowerlimit, double upperlimit, int parNum, int valueOrChange)
        {
            int nGoodNodes = 0;
            int nodeNum = GetNextNumber();
            RVTriangle tP, tBP, eTP, eltP;
            RVElement elP;
            RVNodeShallowWater cNode = new RVNodeShallowWater();
            RVNodeHabitat nP;
            double value0, value1, value2, average;
            double dz;

            eTP = FirstTriElements;  //meshP
            while (eTP != null)
            {
                if (eTP.Status == StatusFlag.Activate)
                {
                    value0 = eTP.GetNode(0).GetPapam(parNum);
                    value1 = eTP.GetNode(1).GetPapam(parNum);
                    value2 = eTP.GetNode(2).GetPapam(parNum);

                    switch (valueOrChange)
                    {

                        case 0:
                            {
                                average = (value0 + value1 + value2) / 3.0;
                                if ((lowerlimit < average) && (average < upperlimit))
                                {
                                    eTP.SetRefineOn();
                                    for (int i = 0; i < 3; i++)
                                    {
                                        elP = eTP.GetOwner(i);
                                        if (elP != null && elP.Count() == 3)
                                        {
                                            eltP = (RVTriangle)elP;
                                            eltP.SetRefineOn();
                                        }
                                    }
                                }
                                break;
                            }

                        case 1: //the change in
                            {
                                if (((lowerlimit < Math.Abs(value0 - value1)) && (Math.Abs(value0 - value1) < upperlimit))
                                    || ((lowerlimit < Math.Abs(value1 - value2)) && (Math.Abs(value1 - value2) < upperlimit))
                                    || ((lowerlimit < Math.Abs(value2 - value0)) && (Math.Abs(value2 - value0) < upperlimit)))
                                {
                                    eTP.SetRefineOn();
                                    for (int i = 0; i < 3; i++)
                                    {
                                        elP = eTP.GetOwner(i);
                                        if (elP != null && elP.Count() == 3)
                                        {
                                            eltP = (RVTriangle)elP;
                                            eltP.SetRefineOn();
                                        }
                                    }
                                }
                                break;
                            }

                        case 2: //the change in
                            {
                                if (((lowerlimit < value0) && (value0 < upperlimit))
                                    || ((lowerlimit < value1) && (value1 < upperlimit))
                                    || ((lowerlimit < value2) && (value2 < upperlimit)))
                                {
                                    eTP.SetRefineOn();
                                }
                                break;
                            }
                    }
                }
                eTP = NextTriElements;
            }
            eTP = FirstTriElements;  //meshP
            while (eTP != null)
            {
                if (eTP.IsRefineOn == 1)
                {
                    nP = (RVNodeHabitat)physics.CreateNewNode(nodeNum);
                    eTP.LocateNodeAtCenter(nP); //interpolates a new node based on mesh
                    cNode.Init(nP.X, nP.Y, nP.Z); //assigns the shallownode the same x and y
                    if ((tP = bedMesh.WhichTriangle(nP)) != null)
                    {
                        tBP = eTP;
                        if (boundTIN != null)
                        {
                            tBP = boundTIN.WhichTriangle(nP);
                        }
                        if (tBP != null)
                        {
                            if (tBP.Status == StatusFlag.Activate)
                            {
                                tP.LocateNodeAndInterpolation(cNode);
                                nP.Ks=cNode.Ks;
                                dz = cNode.Z - nP.Z;
                                nP.Z = nP.Z + dz;
                                nP.Depth = nP.Depth - dz;
                                nGoodNodes += 1;
                                nodeNum += 1;
                                tmpNodesList.Add(nP);
                            }
                            else
                                nP = null;
                        }
                        else
                            nP = null;
                    }
                    else
                        nP = null;
                }
                eTP.SetRefineOff();
                eTP = NextTriElements;
            }
            return nGoodNodes;
        }

        public int deletePrimaryMeshNode(RVNode nP)
        {
            RVList tempbreakLinesList = new RVList(); //temporary brealine list
            RVSegment segP;
            RVNode bN, eN;
            if ((nP.Fixed == RVFixedNodeFlag.fixedNode) || (nP.Fixed == RVFixedNodeFlag.slidingNode))
            {
                segP = FirstBreakLine;

                //remove all breakline segments that include node nP
                //and put on a temporary list

                while (segP != null)
                {
                    if ((segP.GetNode(0) == nP) || (segP.GetNode(1) == nP))
                    {
                        breakLinesList.RemoveCurrentItem();
                        tempbreakLinesList.Add(segP);
                        segP = (RVSegment)breakLinesList.CurrentItem();
                    }
                    else
                        segP = NextBreakLinesList;
                }

                //fix all of the CountNodes that are connected to node nP
                //by breakline segments
                //then delete the breakline segments

                segP = (RVSegment)tempbreakLinesList.FirstItem();
                while (segP != null)
                {
                    bN = segP.GetNode(0);
                    eN = segP.GetNode(1);
                    if (bN == nP) eN.Fixed = RVFixedNodeFlag.fixedNode;
                    else bN.Fixed = RVFixedNodeFlag.fixedNode;
                    segP = (RVSegment)tempbreakLinesList.NextItem();
                }
                tempbreakLinesList.Clear();

                //Set node nP to slidingNode so that it can be deletedNode

                nP.Fixed = RVFixedNodeFlag.slidingNode;
            }
            if ((nP.Fixed == RVFixedNodeFlag.floatingNode) || (nP.Fixed == RVFixedNodeFlag.slidingNode))
            {
                nP.Fixed = RVFixedNodeFlag.deletedNode;
                return 1;
            }
            else
                return 0;
        }

        public RVNodeHabitat removeNode(RVNode nP)
        {
            RVNodeHabitat currentNode;
            currentNode = (RVNodeHabitat)nodesList.Remove(nP);
            return currentNode;
        }

        //public void buildPsiMatrices()
        //{
        //	RVTriangle triangle;
        //	RVBoundary bSegP;
        //	PsiElementMtx elMP, BelMP;

        //	//create the triangular element matrices

        //	triangle = FirstTriElements();

        //	while (triangle != null)
        //	{
        //		if (triangle.Status() ==  StatusFlag.Activate)
        //		{
        //			elMP = new PsiElementMtx(triangle.getn(), triangle, 3);
        //			ElMtxL.Add(elMP);
        //		}
        //		triangle = NextTriElements();
        //	}

        //	//create the linear element matrices (boundary elements)

        //	bSegP = (RVBoundary)FirstBoundarySegment();

        //	while (bSegP != null)
        //	{
        //		if (bSegP.getBcCode() == 0)
        //		{
        //			BelMP = new PsiElementMtx(bSegP.getn(), bSegP, 2);
        //			BElMtxL.Add(BelMP);
        //		}
        //		bSegP = (RVBoundary)NextBoundarySegment();
        //	}

        //}
        //public void buildPhiMatrices(double wsElevIn)
        //{
        //	RVTriangle triangle;
        //	RVBoundary bSegP;
        //	PhiElementMtx elMP, BelMP;

        //	//create the triangular element matrices

        //	triangle = FirstTriElements();

        //	while (triangle != null)
        //	{
        //		if (triangle.Status() ==  StatusFlag.Activate)
        //		{
        //			elMP = new PhiElementMtx(triangle.getn(), triangle, 3, wsElevIn);
        //			ElMtxL.Add(elMP);
        //		}
        //		triangle = NextTriElements();
        //	}

        //	//create the linear element matrices (boundary elements)

        //	bSegP = (RVBoundary)FirstBoundarySegment();

        //	while (bSegP != null)
        //	{
        //		if (bSegP.getBcCode() == 1 || bSegP.getBcCode() == 3 || bSegP.getBcCode() == 5)
        //		{
        //			BelMP = new PhiElementMtx(bSegP.getn(), bSegP, 2, wsElevIn);
        //			BElMtxL.Add(BelMP);
        //		}
        //		bSegP = (RVBoundary)NextBoundarySegment();
        //	}

        //}
        //public void destroyMatrices()
        //{

        //	ElementMtx elMP, BelMP;

        //	//delete the element matrices and vectors once solution is obtained

        //	//delete the triangular element matrices

        //	elMP = (ElementMtx)ElMtxL.FirstItem();
        //	while (elMP != null)
        //	{
        //		ElMtxL.RemoveCurrentItem();
        //		elMP = (ElementMtx)ElMtxL.FirstItem();
        //	}

        //	//delete the linear element matrices (boundary elements)

        //	BelMP = (ElementMtx)BElMtxL.FirstItem();
        //	while (BelMP != null)
        //	{
        //		BElMtxL.RemoveCurrentItem();
        //		BelMP = (ElementMtx)BElMtxL.FirstItem();
        //	}
        //}
        //public void calculatePsi(RVNodeHabitat startnP)
        //{
        //	//setAllPsi(startnP);
        //	//buildPsiMatrices();
        //	//FEMProb = FEMProblem.psiSolution;
        //	//Solver psiSolver = new Solver(this);
        //	//psiSolver.preconditionedCGs(ElMtxL, BElMtxL, 1);

        //	//// Store psi in backend Dennis Hodge June 18 2009
        //	//RVNodeHabitat nP = (RVNodeHabitat)FirstBoundarySegment().GetNode(0);
        //	//node elemNodes = River.gp.gp.Nodes;
        //	//while (nP.getPrevOne() != null) 
        //	//{ 
        //	//	nP = (RVNodeHabitat)nP.getPrevOne();
        //	//}
        //	//while (nP != null) {
        //	//	elemNodes.CumDischarge = nP.getPsi();
        //	//	elemNodes = elemNodes.nextnp;
        //	//	nP = (RVNodeHabitat*)nP.getNextOne();
        //	//}
        //	//destroyMatrices();
        //}

        public void calculatePhi(double wsElevIn)
        {
            //buildPhiMatrices(wsElevIn);
            //FEMProb = phiSolution;
            //Solver phiSolver = new Solver(this);
            //phiSolver.preconditionedCGs(ElMtxL, BElMtxL, 1);
            //destroyMatrices();
        }
        public void initSolutionVector(double[] X)
        {
            if (FEMProb == FEMProblem.psiSolution)
            {
                RVNodeHabitat hNP;
                int i = 0;
                hNP = (RVNodeHabitat)firstNode;

                while (hNP != null)
                {
                    X[i] = hNP.Psi;
                    i++;
                    hNP = (RVNodeHabitat)NextNode;
                }
            }
            if (FEMProb == FEMProblem.phiSolution)
            {
                RVNodeHabitat hNP;
                int i = 0;
                hNP = (RVNodeHabitat)firstNode;

                while (hNP != null)
                {
                    X[i] = hNP.GetPapam(6);
                    i++;
                    hNP = (RVNodeHabitat)NextNode;
                }
            }
        }

        public void updateSolToNodes(double[] X)
        {
            if (FEMProb == FEMProblem.psiSolution)
            {
                RVNodeHabitat hNP;
                int i = 0;
                hNP = (RVNodeHabitat)firstNode;

                while (hNP != null)
                {
                    hNP.Psi = X[i];
                    i++;
                    hNP = (RVNodeHabitat)NextNode;
                }
            }
            if (FEMProb == FEMProblem.phiSolution)
            {
                RVNodeHabitat hNP;
                int i = 0;
                hNP = (RVNodeHabitat)firstNode;

                while (hNP != null)

                {
                    hNP.SetFlow(X[i] - hNP.GetPapam(1), 0, 0);
                    hNP.SetVandF();
                    i++;
                    hNP = (RVNodeHabitat)NextNode;
                }
            }
        }
        //	Puts one RVNode into the RVMeshIrregular (from physics spec).
        //	Requires a three previously triangulated RVMeshIrregular.
        //	boundTIN is used when the new node is being added within a user defined boundary - eg region refine
        //	Bed elevation and roughness of the new node are interpolated from the bedTIN
        //	while the other parameters (qx, qy, water surface elevation) are interpolated from 
        //	the meshTIN.
        //	The esulting node is added to tmpNodesList and  can be later accepted into nodesList or rejected.

        // Помещает один узел в RVMeshIrregular (из спецификации физики).
        // Требуется три предварительно триангулированных RVMeshIrregular.
        // boundTIN используется, когда новый узел добавляется в пределах определенной пользователем границы - например, уточнение области
        // Высота кровати и шероховатость нового узла интерполируются из bedTIN
        // в то время как остальные параметры (qx, qy, высота поверхности воды) интерполируются из сеткаRVMeshIrregular.
        // Результирующий узел добавляется в tmpNodesList и может быть позже принят в nodesList или отклонен.
        public RVNodeHabitat insertOneFloatingNode(double x, double y, RVMeshIrregular boundTIN, RVMeshIrregular bedTIN, RVMeshIrregular meshTIN)
        {
            int nGoodNodes = 0;
            int nodeNum = GetNextNumber();
            RVTriangle tBedP, tBoundP, tMeshP, triangle;
            RVNodeHabitat nP;
            RVNodeShallowWater sN = new RVNodeShallowWater();
            double dz;

            nP = (RVNodeHabitat)physics.CreateNewNode(nodeNum, x, y);
            if ((tBedP = bedTIN.WhichTriangle(nP)) != null)
            {
                if ((tMeshP = meshTIN.WhichTriangle(nP)) != null)
                {
                    if ((tBoundP = boundTIN.WhichTriangle(nP)) != null)
                    {
                        if (tBoundP.Status == StatusFlag.Activate)
                        {
                            tMeshP.LocateNodeAndInterpolation(x, y, nP);
                            sN.Init(nP.X, nP.Y, nP.Z);
                            triangle = bedTIN.WhichTriangle(sN);
                            if (triangle != null)
                            {
                                triangle.LocateNodeAndInterpolation(sN);
                                nP.Ks = sN.Ks;
                                dz = sN.Z - nP.Z;
                                nP.Z = nP.Z + dz;
                                nP.Depth= nP.Depth - dz;
                            }

                            nGoodNodes += 1;
                            nodeNum += 1;
                            tmpNodesList.Add(nP);
                        }
                        else
                        {
                            nP = null;
                        }
                    }
                    else
                    {
                        nP = null;
                    }
                }
                else
                {
                    nP = null;
                }
            }
            else
            {
                nP = null;
            }

            return nP;
        }
        public double smoothHabitatMesh(int nTimes, RVMeshIrregular bedMesh, double bias = 0.0)
        {
            RVNodeHabitat nP;
            RVNodeShallowWater sN = new RVNodeShallowWater();
            RVTriangle tP, element, dtP;
            RVNode nP1;
            double a, sumx, sumy, suma, xN, yN, dx, dy, lx, ly, d, dz;
            double[] xNew, yNew;
            int j, index = 0, iin = 0;

            bias = Math.Sqrt(bias);

            xNew = new double[nodesList.Count];
            yNew = new double[nodesList.Count];

            for (int i = 0; i < nTimes; i++)
            {
                nP = (RVNodeHabitat)nodesList.FirstItem();

                while (nP != null)
                {
                    xNew[iin] = nP.X;
                    yNew[iin] = nP.Y;
                    if ((nP.Fixed == RVFixedNodeFlag.floatingNode) && (nP.BoundNodeFlag == BoundaryNodeFlag.internalNode))
                    {
                        if ((tP = WhichTriHasNode(nP)) != null)
                        {
                            sumx = sumy = suma = 0.0;
                            element = tP;
                            do
                            {
                                a = ((1.0 - bias) + bias * Math.Abs(bedMesh.TriCenterDz(element))) * Math.Sqrt(Math.Abs(element.Area())); // element.Area();
                                suma += 4.0 * a;
                                sumx += nP.X * a;
                                sumy += nP.Y * a;
                                for (j = 0; j < 3; j++)
                                {
                                    nP1 = element.GetNode(j);
                                    sumx += nP1.X * a;
                                    sumy += nP1.Y * a;
                                    if (nP1 == nP)
                                        index = j;
                                }
                                index -= 1;
                                if (index == -1)
                                    index = 2;
                                element = (RVTriangle)element.GetOwner(index);
                            } while (element != tP);
                            xNew[iin] = (nP.X + sumx / suma) * 0.5;
                            yNew[iin] = (nP.Y + sumy / suma) * 0.5;
                        }
                    }
                    nP = (RVNodeHabitat)nodesList.NextItem();
                    iin += 1;
                }

                RVSegment fsP = FirstBreakLine;
                RVSegment nsP;
                while (fsP != null)
                {
                    nP = (RVNodeHabitat)fsP.GetNode(1);
                    if (nP.Fixed == RVFixedNodeFlag.slidingNode)
                    {
                        tP = WhichTriHasNode(nP);
                        sumx = sumy = suma = 0.0;
                        element = tP;
                        do
                        {
                            a = ((1.0 - bias) + bias * Math.Abs(bedMesh.TriCenterDz(element))) * Math.Sqrt(Math.Abs(element.Area())); // element.Area();
                            suma += 4.0 * a;
                            sumx += nP.X * a;
                            sumy += nP.Y * a;
                            for (j = 0; j < 3; j++)
                            {
                                nP1 = element.GetNode(j);
                                sumx += nP1.X * a;
                                sumy += nP1.Y * a;
                                if (nP1 == nP)
                                    index = j;
                            }
                            index -= 1;
                            if (index == -1)
                                index = 2;
                            element = (RVTriangle)element.GetOwner(index);
                        } while (element != tP);
                        xN = (nP.X + sumx / suma) * 0.5;
                        yN = (nP.Y + sumy / suma) * 0.5;
                        if ((nsP = (RVSegment)fsP.Next) != null)
                        {
                            if (nsP.GetNode(0) == nP)
                            {
                                dx = xN - fsP.GetNode(0).X;
                                dy = yN - fsP.GetNode(0).Y;
                                lx = nsP.GetNode(1).X - fsP.GetNode(0).X;
                                ly = nsP.GetNode(1).Y - fsP.GetNode(0).Y;
                                d = (dx * lx + dy * ly) / (lx * lx + ly * ly);
                                if (d < 0.33333)
                                    d = 0.33333;
                                else if (d > 0.66667)
                                    d = 0.66667;
                                xN = fsP.GetNode(0).X + d * lx;
                                yN = fsP.GetNode(0).Y + d * ly;

                                sN.Init(xN, yN, 100.0);
                                dtP = bedMesh.WhichTriangle(sN);
                                if (dtP != null)
                                {
                                    dtP.LocateNodeAndInterpolation(sN);
                                    dz = sN.Z - nP.Z;
                                    nP.Init(xN, yN, nP.Z + dz);
                                    nP.Ks = sN.Ks;
                                    nP.Depth = nP.Depth - dz;
                                }

                            }
                        }
                    }
                    fsP = NextBreakLinesList;
                }

                RVSegment bsP = FirstBoundarySegment;
                RVElement elP;
                while (bsP != null)
                {
                    nP = (RVNodeHabitat)bsP.GetNode(1);
                    if (nP.Fixed == RVFixedNodeFlag.slidingNode)
                    {
                        tP = (RVTriangle)bsP.GetOwner(0);
                        sumx = sumy = suma = 0.0;
                        element = tP;
                        do
                        {
                            a = Math.Sqrt(Math.Abs(element.Area()));
                            suma += 3.0 * a;
                            for (j = 0; j < 3; j++)
                            {
                                nP1 = element.GetNode(j);
                                sumx += nP1.X * a;
                                sumy += nP1.Y * a;
                                if (nP1 == nP)
                                    index = j;
                            }
                            index -= 1;
                            if (index == -1)
                                index = 2;
                            elP = element.GetOwner(index);
                            if (elP.Count() == 3)
                                element = (RVTriangle)elP;
                            else
                                break;
                        } while (element != tP);
                        xN = (nP.X + sumx / suma) * 0.5;
                        yN = (nP.Y + sumy / suma) * 0.5;
                        nsP = (RVSegment)bsP.Next;
                        dx = xN - bsP.GetNode(0).X;
                        dy = yN - bsP.GetNode(0).Y;
                        lx = nsP.GetNode(1).X - bsP.GetNode(0).X;
                        ly = nsP.GetNode(1).Y - bsP.GetNode(0).Y;
                        d = (dx * lx + dy * ly) / (lx * lx + ly * ly);
                        if (d < 0.33333)
                            d = 0.33333;
                        else if (d > 0.66667)
                            d = 0.66667;
                        xN = bsP.GetNode(0).X + d * lx;
                        yN = bsP.GetNode(0).Y + d * ly;

                        sN.Init(xN, yN, 100.0);
                        dtP = bedMesh.WhichTriangle(sN);
                        if (dtP != null)
                        {
                            dtP.LocateNodeAndInterpolation(sN);
                            dz = sN.Z - nP.Z;
                            nP.Init(xN, yN, nP.Z + dz);
                            nP.Ks = sN.Ks;
                            nP.Depth = nP.Depth - dz;
                        }

                    }
                    bsP = NextBoundarySegment;
                }

                nP = (RVNodeHabitat)firstNode;
                iin = 0;
                while (nP != null)
                {
                    if (nP.Fixed == RVFixedNodeFlag.floatingNode)
                    {

                        sN.Init(xNew[iin], yNew[iin], 100.0);
                        dtP = bedMesh.WhichTriangle(sN);
                        if (dtP != null)
                        {
                            dtP.LocateNodeAndInterpolation(sN);
                            dz = sN.Z - nP.Z;
                            nP.Init(xNew[iin], yNew[iin], nP.Z + dz);
                            nP.Ks = sN.Ks;
                            nP.Depth = nP.Depth - dz;
                        }

                    }
                    nP = (RVNodeHabitat)NextNode;
                    iin += 1;
                }
            }
            xNew = null;
            yNew = null;
            return 1.0;
        }

        //	Refines a RVMeshRiver by placing a new node in the centre of each existing triangle
        //	within the boundTIN region.
        //	Requires a previously triangulated bedMesh, bedMesh,
        //	which masks inactive areas and provides interpolation data.
        //	Resulting set of CountNodes are in tmpNodesList which can be later accepted into nodesList or rejected.
        //	The number of CountNodes created are returned.
        /// <summary>
        /// Уточняет RVMeshRiver, помещая новый узел в центр каждого существующего треугольника
        /// внутри области boundTIN.
        /// Требуется предварительно триангулированная кровать BedMesh, bedMesh,
        /// который маскирует неактивные области и предоставляет данные интерполяции.
        /// Результирующий набор CountNodes находится в tmpNodesList, который позже может быть принят в nodesList или отклонен.
        /// Возвращается количество созданных CountNodes.
        /// </summary>
        /// <param name="boundTIN"></param>
        /// <param name="bedMesh"></param>
        /// <returns></returns>
        public int refineHabitatRegion(RVMeshIrregular boundTIN, RVMeshIrregular bedMesh)
        {
            int nGoodNodes = 0;
            int nodeNum = GetNextNumber();
            RVTriangle tP, tBP, eTP;
            RVNodeShallowWater cNode = new RVNodeShallowWater();
            RVNodeHabitat nP;
            double dz;

            eTP = FirstTriElements;  //meshP
            while (eTP != null)
            {
                if (eTP.Status == StatusFlag.Activate)
                {
                    nP = (RVNodeHabitat)physics.CreateNewNode(nodeNum);
                    eTP.LocateNodeAtCenter(nP); //interpolates a new node based on mesh
                    cNode.Init(nP.X, nP.Y, nP.Z); //assigns the shallownode the same x and y
                    if ((tP = bedMesh.WhichTriangle(nP)) != null)
                    {
                        if ((tBP = boundTIN.WhichTriangle(nP)) != null)
                        {
                            if (tBP.Status == StatusFlag.Activate)
                            {
                                tP.LocateNodeAndInterpolation(cNode);
                                nP.Ks = cNode.Ks;
                                dz = cNode.Z - nP.Z;
                                nP.Z = nP.Z + dz;
                                nP.Depth = nP.Depth - dz;
                                nGoodNodes += 1;
                                nodeNum += 1;
                                tmpNodesList.Add(nP);
                            }
                            else
                                nP = null;
                        }
                        else
                            nP = null;
                    }
                    else
                        nP = null;
                }
                eTP = NextTriElements;
            }
            return nGoodNodes;
        }

        public int ExtractBoundary(RVMeshIrregular meshTIN, 
                    RVNodeHabitat hNUSBound, RVNodeHabitat hNDSBound, 
                    double leftBound, double rightBound, double spacing, 
                    int leftBoundType, int rightBoundType)
        {
            RVNodeHabitat hNP1, hNP2, hNP3, hNP4;
            RVTriangle triangle, checkTriP, prevTriP;
            RVBoundary boundSegDS, boundSegUS;
            int segNum;
            double wse; //downstream water surface elevation
            double dischargeIntensity;

            if (rightBoundType == 2)  //right boundary chosen along computation boundary
            {
                mapBoundaryStreamLine(meshTIN, hNUSBound, hNDSBound, 0);
                hNP1 = (RVNodeHabitat)firstNode;
                hNP2 = (RVNodeHabitat)nodesList.LastItem();
            }
            else if (rightBoundType == 0) //right boundary chosen along a streamline
            {
                hNP1 = getBoundaryNode(meshTIN, hNUSBound, rightBound, 0, 15);
                AddNode(hNP1);

                hNP2 = getBoundaryNode(meshTIN, hNDSBound, rightBound, 0, 15);

                triangle = meshTIN.WhichTriangle(hNP1);
                prevTriP = null; //used to check if mapping is oscillating between triangles
                while (true) //(-1)
                {
                    checkTriP = mapStreamLine(meshTIN, triangle, hNP1, hNP2, rightBound, 0, 15);
                    if (triangle == checkTriP)
                        break;
                    if (checkTriP == prevTriP)
                        checkTriP = findTriAndNodeWhenVelDirCannot(meshTIN, triangle, rightBound, 0, 15);
                    prevTriP = triangle;
                    triangle = checkTriP;
                }

            }
            else    //right boundary chosen along a depth contour
            {
                hNP1 = getBoundaryNode(meshTIN, hNUSBound, rightBound, 0, 3);
                AddNode(hNP1);

                hNP2 = getBoundaryNode(meshTIN, hNDSBound, rightBound, 0, 3);

                triangle = meshTIN.WhichTriangle(hNP1);
                prevTriP = null; //used to check if mapping is oscillating between triangles
                while (true) //(-1)
                {
                    checkTriP = mapStreamLine(meshTIN, triangle, hNP1, hNP2, rightBound, 0, 3);
                    if (triangle == checkTriP) break;
                    if (checkTriP == prevTriP)
                        checkTriP = findTriAndNodeWhenVelDirCannot(meshTIN, triangle, rightBound, 0, 3);
                    prevTriP = triangle;
                    triangle = checkTriP;
                }
            }

            if (leftBoundType == 2) //left boundary chosen along computation boundary
            {

                mapBoundaryStreamLine(meshTIN, hNUSBound, hNDSBound, 1);
                hNP4 = (RVNodeHabitat)nodesList.LastItem();
            }

            else if (leftBoundType == 0) //left boundary chosen along a streamline
            {
                hNP3 = getBoundaryNode(meshTIN, hNDSBound, leftBound, 1, 15);
                AddNode(hNP3);
                wse = (hNP2.GetPapam(6) + hNP3.GetPapam(6)) / 2;
                segNum = boundarySegmentsList.Count + 1;
                boundSegDS = new RVBoundary(segNum, hNP2, hNP3, 3, wse);
                AddBoundarySegment(boundSegDS);


                hNP4 = getBoundaryNode(meshTIN, hNUSBound, leftBound, 1, 15);

                triangle = meshTIN.WhichTriangle(hNP3);
                prevTriP = null;
                while (true) //(-1)
                {
                    checkTriP = mapStreamLine(meshTIN, triangle, hNP3, hNP4, leftBound, 1, 15);
                    if (triangle == checkTriP) break;
                    if (checkTriP == prevTriP)
                        checkTriP = findTriAndNodeWhenVelDirCannot(meshTIN, triangle, leftBound, 1, 15);
                    prevTriP = triangle;
                    triangle = checkTriP;
                }
            }
            else    //left boundary chosen along a depth contour
            {
                hNP3 = getBoundaryNode(meshTIN, hNDSBound, leftBound, 1, 3);
                AddNode(hNP3);
                wse = (hNP2.GetPapam(6) + hNP3.GetPapam(6)) / 2;
                segNum = boundarySegmentsList.Count + 1;
                boundSegDS = new RVBoundary(segNum, hNP2, hNP3, 3, wse);
                AddBoundarySegment(boundSegDS);


                hNP4 = getBoundaryNode(meshTIN, hNUSBound, leftBound, 1, 3);

                triangle = meshTIN.WhichTriangle(hNP3);
                prevTriP = null;
                while (true) //(-1)
                {
                    checkTriP = mapStreamLine(meshTIN, triangle, hNP3, hNP4, leftBound, 1, 3);
                    if (triangle == checkTriP) break;
                    if (checkTriP == prevTriP)
                        checkTriP = findTriAndNodeWhenVelDirCannot(meshTIN, triangle, leftBound, 1, 3);
                    prevTriP = triangle;
                    triangle = checkTriP;
                }
            }

            dischargeIntensity = (leftBound - rightBound) / (hNP4.Distance(hNP1));
            segNum = boundarySegmentsList.Count + 1;
            boundSegUS = new RVBoundary(segNum, hNP4, hNP1, 1, dischargeIntensity);
            AddBoundarySegment(boundSegUS);

            resampleBoundary(spacing);

            return 1;
        }

        public int checkNoOfInflowSegs()
        {
            RVBoundary bsP;
            int prevBcCode = 0, currentBcCode, Nseg = 0;
            bsP = (RVBoundary)FirstBoundarySegment;
            while (bsP != null)
            {
                currentBcCode = bsP.getBcCode();
                if (currentBcCode != prevBcCode)
                {
                    if (currentBcCode == 1)
                    {
                        Nseg += 1;
                    }
                    prevBcCode = currentBcCode;
                }
                bsP = (RVBoundary)NextBoundarySegment;
            }
            return Nseg;
        }

        public RVSegment setBoundaryPsi(RVNodeHabitat startnP)
        {
            RVBoundary startbsP = null;
            //	RVNodeHabitat nP1, nP2, firstnP;
            //	int prevBcCode, currentBcCode;
            //	double qx, qy, dx, dy, discharge, d, QinTotal = 0, QoutTotal = 0, erTotal, erPercent;
            //	node elemNodes = River.gp.Nodes[0];

            //	// Calculate the error between inflow and outflow to be applied to the outflow
            //	// Also resetting the psi values along the boundary to 0
            //	bsP = (RVBoundary)FirstBoundarySegment();
            //	nP1 = (RVNodeHabitat)bsP.GetNode(0);
            //	nP1.setPsi(0);

            //	while (bsP != null)
            //	{
            //		nP1 = (RVNodeHabitat)bsP.GetNode(0);
            //		nP2 = (RVNodeHabitat)bsP.GetNode(1);
            //		nP2.setPsi(0);

            //		if (bsP.getBcCode() == 1)
            //		{
            //			dx = nP2.X - nP1.X;
            //			dy = nP2.Y - nP1.Y;
            //			d = Math.Abs(Math.Sqrt(dx * dx + dy * dy));
            //			discharge = bsP.getBcValue() * d;
            //			QinTotal += discharge;
            //		}
            //		if (bsP.getBcCode() == 3 || bsP.getBcCode() == 5)
            //		{
            //			dx = nP2.X - nP1.X;
            //			dy = nP2.Y - nP1.Y;
            //			qx = (nP1.GetPapam(4) + nP2.GetPapam(4)) / 2;
            //			qy = (nP1.GetPapam(5) + nP2.GetPapam(5)) / 2;
            //			discharge = qx * dy - qy * dx;
            //			QoutTotal += discharge;
            //		}

            //		bsP = (RVBoundary)NextBoundarySegment();
            //	}

            //	erTotal = QinTotal - QoutTotal;

            //	//check to indicate to the user if inflow/outflow error is too large

            //	erPercent = erTotal / QinTotal;

            //	//if (erPercent > 0.10)
            //	//	AfxMessageBox("Note: The solution may not have yet reached steady state.\n\n"
            //	//				  "The net outflow is more than 10 percent of the total inflow.\n"
            //	//				  "As a result, the calculated cumulative discharge may\nnot be very accurate.");

            //	// Finding the starting node at a corner where an inflow boundary element 
            //	// meets a noflow boundary element

            //	bsP = (RVBoundary)FirstBoundarySegment();
            //	startbsP = bsP;

            //	firstnP = (RVNodeHabitat)bsP.GetNode(0);  //firstnP is first node in the list defining the external boundary

            //	if (startnP == null) // for the case of 1 inflow
            //	{
            //		do
            //		{
            //			prevBcCode = bsP.getBcCode();
            //			bsP = (RVBoundary)NextBoundarySegment();
            //			currentBcCode = bsP.getBcCode();
            //			if (prevBcCode == 1 && currentBcCode == 0)
            //				startbsP = bsP;

            //		} while (bsP.GetNode(1) != firstnP);
            //	}
            //	else // multiple inflows - starting node startnP input by user 
            //		 // this loop finds the starting boundary segment based on the starting node
            //	{
            //		while (bsP != null)
            //		{
            //			if (bsP.GetNode(0) == startnP)
            //				startbsP = bsP;
            //			bsP = (RVBoundary)NextBoundarySegment();
            //		}
            //	}

            //	//Using the new starting point cycle through the boundary
            //	//element list and set the stream function value - previously
            //	//initialized to zero throughout the entire domain

            //	bsP = startbsP;
            //	boundarySegmentsList.SetCurrentItem(bsP); // had to add this line so that bsP is the "current"
            //							   // item on the list, without this line it may not be

            //	startnP = (RVNodeHabitat)startbsP.GetNode(0);   //startnP is the starting node for setting the boundary psi values
            //	do
            //	{
            //		nP1 = (RVNodeHabitat)bsP.GetNode(0);
            //		nP2 = (RVNodeHabitat)bsP.GetNode(1);

            //		if (bsP.getBcCode() == 0)
            //		{
            //			nP2.setPsi(nP1.getPsi());

            //		}
            //		if (bsP.getBcCode() == 1)
            //		{
            //			dx = nP2.X - nP1.X;
            //			dy = nP2.Y - nP1.Y;
            //			d = Math.Abs(Math.Sqrt(dx * dx + dy * dy));
            //			discharge = bsP.getBcValue() * d;
            //			nP2.setPsi(nP1.getPsi() - discharge);


            //		}
            //		if (bsP.getBcCode() == 3 || bsP.getBcCode() == 5)
            //		{
            //			dx = nP2.X - nP1.X;
            //			dy = nP2.Y - nP1.Y;
            //			qx = (nP1.GetPapam(4) + nP2.GetPapam(4)) / 2;
            //			qy = (nP1.GetPapam(5) + nP2.GetPapam(5)) / 2;
            //			discharge = (qx * dy - qy * dx) * (erTotal / QoutTotal + 1);
            //			nP2.setPsi(nP1.getPsi() + discharge);

            //		}
            //		//this if else is here so that we skip the internal boundaries

            //		if (firstnP != startnP && bsP.GetNode(1) == firstnP)
            //			bsP = (RVBoundary)FirstBoundarySegment();  //need to check for firstnP == startnP or get caught in loop 
            //		else
            //		{
            //			bsP = (RVBoundary)NextBoundarySegment();
            //			elemNodes = elemNodes.nextnp;
            //			if (bsP == null)
            //				bsP = (RVBoundary)FirstBoundarySegment();
            //		}

            //	} 
            //	while (bsP.GetNode(1) != startnP);
            //	boundarySegmentsList.SetCurrentItem(startbsP);
            return startbsP;
        }


        //public void setBElementFlowBound(RVFlowBoundary fBP, RVNode startNP, RVNode endNP)
        //{
        //    RVBoundary segP;
        //    segP = (RVBoundary)FirstBoundarySegment;
        //    if (segP == null)
        //        return;
        //    while (segP.GetNode(0) != startNP)
        //    {
        //        segP = (RVBoundary)NextBoundarySegment;
        //        if (segP == null)
        //            segP = (RVBoundary)FirstBoundarySegment;
        //    }
        //    segP.setFlowBound(fBP);
        //    while (segP.GetNode(1) != endNP)
        //    {
        //        segP = (RVBoundary)NextBoundarySegment;
        //        if (segP == null)
        //            segP = (RVBoundary)FirstBoundarySegment;
        //        segP.setFlowBound(fBP);
        //    }
        //    return;
        //}

        public void setAllPsi(RVNodeHabitat startnP)
        {
            //	RVBoundary startbsP, bsP;
            //	RVNodeHabitat startbnP, nP, nP1;
            //	RVElement aElP;
            //	RVTriangle element, tP;
            //	RVList tList = new RVList(), aList = new RVList();
            //	double sum;
            //	int count = 0;
            //	node elemNodes = River.gp.Nodes[0];

            //	int j, index=0;

            //	double qx, qy, dx, dy, discharge;

            //	//aList is for CountNodes with psi set that have not 
            //	//been checked for adjacent CountNodes boundary or internalNode CountNodes

            //	//tList is for CountNodes that have psi set and have been checked for
            //	//adjacent CountNodes

            //	//before starting...create an array of pointers and store a pointer
            //	//to each node which will be used to restore the node ordering after
            //	//sweeping through the domain to set the psi values
            //	//also setting n for all CountNodes to 0 as this value is used as a flag
            //	//to see whether or not a node has been removed from nodesList
            //	//n = 0 . still on nodesList
            //	//n = 1 . no longer on nodesList

            //	RVNode[] nodesListist = new RVNode[nodesList.Count()];
            //	RVNode nodeP = firstNode();
            //	int inode = 0;

            //	while (nodeP != null)
            //	{
            //		nodesListist[inode] = nodeP;
            //		nodeP.setn(0);
            //		inode++;
            //		nodeP = NextNode();
            //	}

            //	startbsP = (RVBoundary)setBoundaryPsi(startnP);

            //	//cycle through all EXTERIOR boundary CountNodes and put on the aList.

            //	bsP = (RVBoundary)FirstBoundarySegment();
            //	startbnP = (RVNodeHabitat)bsP.GetNode(0);

            //	while (true)// (-1)								
            //	{
            //		nP = (RVNodeHabitat)bsP.GetNode(0);
            //		nodesList.Remove(nP);
            //		aList.Add(nP);
            //		nP.setn(1);
            //		if (bsP.GetNode(1) == startbnP) break;
            //		bsP = (RVBoundary)NextBoundarySegment();
            //	}

            //	//need to reorder tList so that startbnP is at the top of the list

            //	startbnP = (RVNodeHabitat)startbsP.GetNode(0);
            //	nP = (RVNodeHabitat)aList.FirstItem();

            //	while (nP != startbnP)
            //	{
            //		aList.Remove(nP);
            //		aList.Add(nP);
            //		nP = (RVNodeHabitat)aList.FirstItem();
            //	}

            //	//cycle through aList which starts as only boundary CountNodes
            //	//start by popping the top item off the list and pushing the adjacent
            //	//CountNodes onto the aList once their psi value has been set
            //	//and pushing the node that has been popped of onto the tList.

            //	nP = (RVNodeHabitat)aList.FirstItem();
            //	tList.Clear();

            //	while (nP != null)
            //	{

            //		tList.Add(aList.Pop());  //had to change to Add from 
            //										//Push because Push cannot
            //										//cannot set previous item which 
            //										//causes Remove to crash
            //		if ((tP = WhichTriHasNode(nP)) != null)
            //		{
            //			element = tP;
            //			do
            //			{
            //				for (j = 0; j < 3; j++)
            //				{
            //					nP1 = (RVNodeHabitat)element.GetNode(j);
            //					if (nP1 == nP)
            //						index = j;
            //				}
            //				index += 1;
            //				if (index == 3)
            //					index = 0;
            //				aElP = element.GetOwner(index);
            //				if (aElP == null) //added because in orderNodesRCM
            //					break;       //added because in orderNodesRCM
            //				if (aElP.Count() == 2)
            //					break;
            //				element = (RVTriangle)aElP;
            //			} while (element != tP);
            //			tP = element;
            //			do
            //			{
            //				for (j = 0; j < 3; j++)
            //				{
            //					nP1 = (RVNodeHabitat)element.GetNode(j);
            //					if (nP1 == nP)
            //						index = j;
            //					else
            //					{
            //						if (nP1.getn() == 0)
            //						{
            //							dx = nP1.X - nP.X;
            //							dy = nP1.Y - nP.Y;
            //							qx = (nP1.GetPapam(4) + nP.GetPapam(4)) / 2;
            //							qy = (nP1.GetPapam(5) + nP.GetPapam(5)) / 2;
            //							discharge = (qx * dy - qy * dx);
            //							//	if (nP1.GetPapam(3) < 0)
            //							//		discharge = 0;
            //							nP1.setPsi(nP.getPsi() + discharge);
            //							nodesList.Remove(nP1);
            //							aList.Push(nP1);
            //							nP1.setn(1);

            //						}
            //					}
            //				}
            //				index -= 1;
            //				if (index == -1)
            //					index = 2;
            //				aElP = element.GetOwner(index);
            //				if (aElP == null) //added because in orderNodesRCM
            //					break;       //added because in orderNodesRCM	
            //				if (aElP.Count() == 2)
            //					break;
            //				element = (RVTriangle)aElP;

            //			} while (element != tP);
            //		}


            //		nP = (RVNodeHabitat)aList.FirstItem();
            //	}

            //	//restore the node ordering that existed before
            //	//the sweep using the array of node pointers

            //	for (int ii = 0; ii < inode; ii++)
            //	{
            //		tList.Remove(nodesListist[ii]);
            //		nodesList.Add(nodesListist[ii]);
            //	}

            //	nodesListist = null;

            //	//reset n for each node according to
            //	//the node ordering

            //	nP = (RVNodeHabitat)nodesList.FirstItem();
            //	int nn = 1;
            //	while (nP != null)
            //	{
            //		nP.setn(nn);
            //		nn++;
            //		nP = (RVNodeHabitat)nP.getNextOne();
            //	}

            //	//Now that psi has been calculated everywhere, go back, average the psi
            //	//values for each internal boundary and reset each value to the average

            //	bsP = (RVBoundary)FirstBoundarySegment();
            //	startbnP = (RVNodeHabitat)bsP.GetNode(0);
            //	//loop to get to the first internal boundary
            //	do
            //	{
            //		bsP = (RVBoundary)NextBoundarySegment();
            //	} while (bsP.GetNode(1) != startbnP);

            //	boundarySegmentsList.SetCurrentItem(bsP);
            //	bsP = (RVBoundary)boundarySegmentsList.NextItem();
            //	if (bsP != null)
            //		nP = (RVNodeHabitat)bsP.GetNode(0);

            //	while (bsP != null)
            //	{
            //		startbnP = (RVNodeHabitat)bsP.GetNode(0);
            //		startbsP = bsP;
            //		sum = 0;
            //		count = 0;
            //		//loop through internal boundary first time
            //		//to sum up the psi values
            //		while (true)// (-1)
            //		{
            //			nP = (RVNodeHabitat)bsP.GetNode(0);
            //			sum += nP.getPsi();
            //			count += 1;
            //			if (bsP.GetNode(1) == startbnP) break;
            //			bsP = (RVBoundary)NextBoundarySegment();
            //		}
            //		double psi = sum / count;
            //		nP = (RVNodeHabitat)startbsP.GetNode(0);
            //		bsP = startbsP;
            //		boundarySegmentsList.SetCurrentItem(bsP);
            //		//loop through again to reset the psi values
            //		while (true)// (-1)
            //		{
            //			nP = (RVNodeHabitat)bsP.GetNode(0);
            //			nP.setPsi(psi);
            //			if (bsP.GetNode(1) == startbnP) break;
            //			bsP = (RVBoundary)NextBoundarySegment();
            //		}
            //		//go to the next internal boundary if any...
            //		bsP = (RVBoundary)NextBoundarySegment();
            //	}
        }
        #endregion
    }
}
