//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 11.08.2021 Потапов И.И.
//---------------------------------------------------------------------------
//             обновление и чистка : 01.01.2024 Потапов И.И.
//---------------------------------------------------------------------------
using CommonLib.Mesh.RVData;

namespace TriMeshGeneratorLib
{
    /// <summary>
    /// Генератор КЭ сетки через рекурсивное квадратное окно
    /// </summary>
    public class RVQuadrantGenerator
    {
        public static double MINDIST = 0.0001;
        /// <summary>
        /// 4 subquadrants ne, nw, sw, se
        /// подобласти квадрата
        /// </summary>
        protected RVQuadrantGenerator[] subQuadrant = new RVQuadrantGenerator[4];
        /// <summary>
        /// голова/хвост списка узлов
        /// </summary>
        protected RVNode firstNode, LastNode;
        /// <summary>
        /// счетчик узлов
        /// </summary>
        protected int CountNodes;
        /// <summary>
        /// счетчик треугольников
        /// </summary>
        protected int CountTriangles;
        /// <summary>
        /// счетчик граничных сегментов
        /// </summary>
        protected int CountmpBoundarySegmentsList;
        /// <summary>
        /// голова/хвост списка треугольников
        /// </summary>
        protected RVTriangle firstTriangle, lastTriangle;
        /// <summary>
        /// Дуга на основе вершин треугольника
        /// </summary>
        public RVTriangle BaseArc { get => baseArc; set => baseArc = value; }
        /// <summary>
        /// 
        /// </summary>
        protected RVTriangle baseArc;
        /// <summary>
        /// рут треугольников
        /// </summary>
        /// <returns></returns>
        public RVTriangle FirstTriangle => firstTriangle; 
        /// <summary>
        /// ящик границы области
        /// </summary>
        protected RVBox boundBox;
        /// <summary>
        /// Конструктор с пределами границы области
        /// </summary>
        /// <param name="limits"></param>
        public RVQuadrantGenerator(RVBox limits)
        {
            subQuadrant[0] = null;
            subQuadrant[1] = null;
            subQuadrant[2] = null;
            subQuadrant[3] = null;
            CountNodes = CountTriangles = 0;
            firstNode = LastNode = null;
            firstTriangle = lastTriangle = null;
            baseArc = null;
            boundBox = limits;
         }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        public RVQuadrantGenerator(double x1, double y1, double x2, double y2)
        {
            subQuadrant[0] = null;
            subQuadrant[1] = null;
            subQuadrant[2] = null;
            subQuadrant[3] = null;
            CountNodes = CountTriangles = 0;
            firstNode = LastNode = null;
            firstTriangle = lastTriangle = null;
            baseArc = null;
            boundBox = new RVBox( x1,  y1,  x2,  y2);
        }
        /// <summary>
        /// рут узлов
        /// </summary>
        /// <returns></returns>
        public RVNode FirstNode => firstNode; 

        /// <summary>
        /// добавление узла в список
        /// </summary>
        /// <param name="newNode"></param>
        public void AddNode(RVNode newNode)
        {
            if (newNode != null)
            {
                if (firstNode == null)
                    firstNode = newNode;
                else
                    LastNode.Next = newNode;
                LastNode = newNode;
                newNode.Next= null;
                CountNodes += 1;
            }
            return;
        }
        /// <summary>
        /// добавление треугольника в список
        /// </summary>
        /// <param name="newTri"></param>
        public void AppendTriangle(RVTriangle newTri)
        {
            if (newTri != null)
            {
                if (firstTriangle == null)
                {
                    firstTriangle = newTri;
                    newTri.prevTriangle = null;

                }
                else
                {
                    lastTriangle.NextTriangle = newTri;
                    newTri.prevTriangle = lastTriangle;
                }
                newTri.NextTriangle = null;
                lastTriangle = newTri;
                CountTriangles += 1;
            }
            return;
        }
        /// <summary>
        /// удаление треугольника из списка
        /// </summary>
        /// <param name="tP"></param>
        public void DeleteTriangle(RVTriangle tP)
        {
            if (tP.NextTriangle != null)
                tP.NextTriangle.prevTriangle = tP.prevTriangle;
            else
                lastTriangle = tP.prevTriangle;

            if (tP.prevTriangle != null)
                tP.prevTriangle.NextTriangle = tP.NextTriangle;
            else
                firstTriangle = tP.NextTriangle;

            tP.NextTriangle = null;
            tP.prevTriangle = null;
            CountTriangles += 1;
            return;
        }
        /// <summary>
        ///  Рекурсивный метод триангуляции
        /// </summary>
        /// <returns></returns>
        public int TriangulateNodes()
        {
            if (CountNodes == 0)
                baseArc = null;  // Делаем дугу так, чтобы она никуда не указывала

            if (CountNodes == 1)
            {
                // Создаем дугу, указывающую на узел
                RVNode np1 = firstNode;
                RVTriangle arc1 = new RVTriangle(1, np1, np1, null);
                arc1.SetOwner(0, arc1);
                arc1.SetOwner(1, arc1);
                baseArc = arc1;
                // RVCdgIOut.put_elm(firstTriangle, true, CountNodes);
            }
            if (CountNodes == 2)
            {
                RVNode np1 = firstNode;
                RVNode np2 = (RVNode)np1.Next;

                /*		double minDist = 0.0001;		// Check for duplicate CountNodes
						if(np1.Distance(np2) < minDist) {
							delete np2;
							np2 = null;
							np1.setNextOne(null);
							LastNode = np1;
							CountNodes--;
						}
						if(CountNodes == 2){
				*/                                    // Make a double arc between 2 CountNodes
                RVTriangle arc1 = new RVTriangle(1, np1, np2, null);
                RVTriangle arc2 = new RVTriangle(1, np2, np1, null);
                arc1.SetOwner(0, arc2);
                arc1.SetOwner(1, arc2);
                arc1.SetOwner(2, arc2);

                arc2.SetOwner(0, arc1);
                arc2.SetOwner(1, arc1);
                arc2.SetOwner(2, arc1);
                baseArc = arc1;
                //		}
                // RVCdgIOut.put_elm(firstTriangle, true, CountNodes);
            }

            if (CountNodes == 3)
            {
                RVNode np1 = firstNode;
                RVNode np2 = (RVNode)np1.Next;
                RVNode np3 = (RVNode)np2.Next;
                // Создаем треугольник, окруженный дугами
                RVTriangle tp = new RVTriangle(1, np1, np2, np3);
                firstTriangle = lastTriangle = tp;
                tp.NextTriangle = tp.prevTriangle = null;
                // считаем площадь треугольника
                double area = tp.Area();
                if (area < 0.0)
                {
                    // если площадь треугольника отрицательная меняем местами узлы 1 и 2
                    tp.SetNode(1, np3);
                    tp.SetNode(2, np2);
                }
                // сопряженные треугольники без третьего узла
                RVTriangle arc0 = new RVTriangle(1, tp.GetNode(0), tp.GetNode(1), null);
                RVTriangle arc1 = new RVTriangle(1, tp.GetNode(1), tp.GetNode(2), null);
                RVTriangle arc2 = new RVTriangle(1, tp.GetNode(2), tp.GetNode(0), null);
                // добавляем ссылки на сопряженные треугольники 
                tp.SetOwner(0, arc1);
                tp.SetOwner(1, arc2);
                tp.SetOwner(2, arc0);
                // добавляем сопряженным треугольникам ссылки на основной треугольник 
                arc0.SetOwner(0, arc1);
                arc0.SetOwner(1, arc2);
                arc0.SetOwner(2, tp);

                arc1.SetOwner(0, arc2);
                arc1.SetOwner(1, arc0);
                arc1.SetOwner(2, tp);

                arc2.SetOwner(0, arc0);
                arc2.SetOwner(1, arc1);
                arc2.SetOwner(2, tp);
                baseArc = arc1;
            }
            if (CountNodes > 3)
            {
                #region Создание суб областей - ректанглов и добавление в них узлов
                // Подразделить
                double ym = (boundBox.y1 + boundBox.y2) / 2.0;
                double xm = (boundBox.x1 + boundBox.x2) / 2.0;
                subQuadrant[0] = new RVQuadrantGenerator(xm, ym, boundBox.x2, boundBox.y2);
                subQuadrant[1] = new RVQuadrantGenerator(boundBox.x1, ym, xm, boundBox.y2);
                subQuadrant[2] = new RVQuadrantGenerator(boundBox.x1, boundBox.y1, xm, ym);
                subQuadrant[3] = new RVQuadrantGenerator(xm, boundBox.y1, boundBox.x2, ym);
                // Распределение узлов в подобласти (процесс рекурсивный)
                // цикл по узлам
                RVNode node = firstNode;
                RVNode NextNode;
                while (node != null)
                {
                    NextNode = (RVNode)node.Next;
                    if (node.Y >= ym)
                    {
                        if (node.X >= xm)
                            subQuadrant[0].AddNode(node);
                        else
                            subQuadrant[1].AddNode(node);
                    }
                    else
                    {
                        if (node.X < xm)
                            subQuadrant[2].AddNode(node);
                        else
                            subQuadrant[3].AddNode(node);
                    }
                    node = NextNode;
                }
                #endregion
                CountNodes = 0;
                firstNode = LastNode = null;
                // Рекурсивная триангуляция для подобластей
                for (int i = 0; i < 4; i++)
                {
                    subQuadrant[i].TriangulateNodes();
                }
                #region  добавления узлов и треугольников из списка i - го субквадранта в список квадранта
                // добавления узлов и треугольников из списка 0 - го субквадранта в список квадранта
                int NNE = RecoverSubQuadInfo(0);
                RVTriangle bArcE = subQuadrant[0].BaseArc;
                // добавления узлов и треугольников из списка 1 - го субквадранта в список квадранта
                int NNW = RecoverSubQuadInfo(1);
                RVTriangle bArcW = subQuadrant[1].BaseArc;
                
                RVTriangle bArcN;
                int NNN = NNE + NNW;
                if (NNE == 0)
                    bArcN = bArcW;
                else if (NNW == 0)
                    bArcN = bArcE;
                else
                    bArcN = StitchLR(bArcW, bArcE);

                NNW = RecoverSubQuadInfo(2);
                bArcW = subQuadrant[2].BaseArc;
                NNE = RecoverSubQuadInfo(3);
                bArcE = subQuadrant[3].BaseArc;
               
                RVTriangle bArcS;
                int NNS = NNE + NNW;
                if (NNE == 0)
                    bArcS = bArcW;
                else if (NNW == 0)
                    bArcS = bArcE;
                else
                    bArcS = StitchLR(bArcW, bArcE);

                int NN = NNN + NNS;
                if (NNN == 0)
                    baseArc = bArcS;
                else if (NNS == 0)
                    baseArc = bArcN;
                else
                    baseArc = StitchLR(bArcN, bArcS);
                #endregion
            }
            for (int i = 0; i < 4; i++)
            {
                subQuadrant[i] = null;
            }

            return CountNodes;
        }
        /// <summary>
        /// восстановление информации о SubQuads
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        int RecoverSubQuadInfo(int i)
        {
            // добавления узлов из внутреннего списка субквадранта в списка квадранта
            RVNode nP = subQuadrant[i].FirstNode;
            RVNode nextNP;
            while (nP != null)
            {
                nextNP = (RVNode)nP.Next;
                AddNode(nP);
                nP = nextNP;
            }
            // добавления треугольников из внутреннего списка субквадранта в списка квадранта
            RVTriangle tP = subQuadrant[i].FirstTriangle;
            RVTriangle nextTP;
            while (tP != null)
            {
                nextTP = tP.NextTriangle;
                AppendTriangle(tP);
                tP = nextTP;
            }
            // количество узлов в субквадранте 
            return subQuadrant[i].CountNodes;
        }
        /// <summary>
        /// Склейка двух трангуляций
        /// </summary>
        /// <param name="bArcL">трангуляция А</param>
        /// <param name="bArcR">трангуляция Б</param>
        /// <returns></returns>
        RVTriangle StitchLR(RVTriangle bArcL, RVTriangle bArcR)
        {
            RVTriangle topArcL = bArcL;
            RVTriangle topArcR = bArcR;
            RVTriangle topArc = new RVTriangle(1, topArcR.node0, topArcL.node1, null);
            int doneFlag = -1;
            while (doneFlag < 1)
            {
                doneFlag = 1;
                if (topArcL.node0 != topArcL.node1)
                {
                    topArc.SetNode(2, topArcL.node0);
                    if (topArc.Area() <= 0.0)
                    {
                        topArcL = topArcL.tP1;
                        topArc.SetNode(1, topArcL.node1);
                        doneFlag = -1;
                    }
                    else
                    {
                        topArc.SetNode(2, topArcL.tP0.node1);
                        if (topArc.Area() < 0.0)
                        {
                            topArcL = topArcL.tP0;
                            topArc.SetNode(1, topArcL.node1);
                            doneFlag = -1;
                        }
                    }
                }
                if (topArcR.node0 != topArcR.node1)
                {
                    topArc.SetNode(2, topArcR.tP1.node0);
                    if (topArc.Area() < 0.0)
                    {
                        topArcR = topArcR.tP1;
                        topArc.SetNode(0, topArcR.node0);
                        doneFlag = -1;
                    }
                    else
                    {
                        topArc.SetNode(2, topArcR.node1);
                        if (topArc.Area() <= 0.0)
                        {
                            topArcR = topArcR.tP0;
                            topArc.SetNode(0, topArcR.node0);
                            doneFlag = -1;
                        }
                    }
                }
            }

            topArc.SetNode(2, null);
            topArc.SetOwner(0, topArcL.tP0);
            topArc.SetOwner(1, topArcR.tP1);

            topArcL.tP0.SetOwner(1, topArc);
            topArcR.tP1.SetOwner(0, topArc);
            RVTriangle botArc = new RVTriangle(1, topArcL.node1, topArcR.node0, null);
            topArc.SetOwner(2, botArc);
            botArc.SetOwner(2, topArc);
            botArc.SetOwner(0, topArcR);
            botArc.SetOwner(1, topArcL);
            topArcL.SetOwner(0, botArc);
            topArcR.SetOwner(1, botArc);
            RVTriangle cwArc = topArcL;
            RVTriangle ccwArc = topArcR;

            int snode = 0;
            if (topArcR.node0 == topArcR.node1)
            {
                topArc.SetOwner(1, botArc);
                botArc.SetOwner(0, topArc);
                ccwArc = topArc;
                topArcR = null;
                snode += 1;
            }
            if (topArcL.node0 == topArcL.node1)
            {
                topArc.SetOwner(0, botArc);
                botArc.SetOwner(1, topArc);
                cwArc = topArc;
                topArcL = null;
                snode += 2;
            }
            if (snode == 3)
                return topArc;

            RVNode nP1 = ccwArc.node1;
            RVNode nP2 = botArc.node1;
            RVNode nP3 = botArc.node0;
            RVNode nP4 = cwArc.node0;

            double areaL, areaR;
            int goLeft;
            RVTriangle tP = new RVTriangle();

            while (true)
            {
                areaL = areaR = -1.0;
                goLeft = 0;
                tP = new RVTriangle(1, nP4, nP2, nP3);
                if (snode != 2)
                {
                    areaL = tP.Area();
                }
                tP.SetNode(0, nP1);
                if (snode != 1)
                {
                    areaR = tP.Area();
                }
                if ((areaL <= 0.0) && (areaR <= 0.0))
                {
                    tP = null;
                    break;
                }
                if ((areaL > 0.0) && (areaR <= 0.0))
                {
                    goLeft = 1;
                }
                if ((areaL <= 0.0) && (areaR > 0.0))
                {
                    goLeft = -1;
                }
                if ((areaL > 0.0) && (areaR > 0.0))
                {
                    if (tP.InsideC(nP4) >= 0.0)
                    {
                        goLeft = 1;
                    }
                    else
                    {
                        goLeft = -1;
                    }
                }
                if (goLeft > 0)
                {
                    tP.SetNode(0, nP4);
                    tP.SetOwner(0, botArc.GetOwner(2));
                    botArc.GetOwner(2).SetOwner(botArc.tsn, tP);
                    tP.SetOwner(1, cwArc.GetOwner(2));
                    if (cwArc.GetOwner(2) != cwArc)
                    {
                        cwArc.GetOwner(2).SetOwner(cwArc.tsn, tP);
                    }
                    else
                    {
                        cwArc.tP1.SetOwner(2, tP);
                    }
                    tP.SetOwner(2, botArc);
                    botArc.SetOwner(2, tP);
                    botArc.SetOwner(1, cwArc.tP1);
                    botArc.SetNode(0, cwArc.node0);
                    cwArc.tP1.SetOwner(0, botArc);
                    cwArc = null;
                    Delaunay(tP);
                    cwArc = botArc.tP1;
                    nP3 = botArc.node0;
                    nP4 = cwArc.node0;
                }
                else
                {
                    tP.SetOwner(0, botArc.GetOwner(2));
                    botArc.GetOwner(2).SetOwner(botArc.tsn, tP);
                    tP.SetOwner(2, ccwArc.GetOwner(2));
                    if (ccwArc.GetOwner(2) != ccwArc)
                    {
                        ccwArc.GetOwner(2).SetOwner(ccwArc.tsn, tP);
                    }
                    else
                    {
                        ccwArc.tP0.SetOwner(2, tP);
                    }
                    tP.SetOwner(1, botArc);
                    botArc.SetOwner(2, tP);
                    botArc.SetOwner(0, ccwArc.tP0);
                    botArc.SetNode(1, ccwArc.GetNode(1));
                    ccwArc.tP0.SetOwner(1, botArc);
                    ccwArc = null;
                    Delaunay(tP);
                    ccwArc = botArc.tP0;
                    nP2 = botArc.node1;
                    nP1 = ccwArc.node1;
                }
            }

            return topArc;
        }
        /// <summary>
        /// Делоне
        /// </summary>
        /// <param name="tP"></param>
        public void Delaunay(RVTriangle tP)
        {
            int index, changed = 0;
            RVTriangle element = new RVTriangle();
            RVTriangle NextTriangle = new RVTriangle();
            // стек трехугольников
            RVTriangle cStack = null;
            tP.Deactivate();
            cStack = PushTriangle(cStack, tP);
            while (cStack != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    changed = 0;
                    element = (RVTriangle)cStack.GetOwner(i);
                    if (element != null)
                    {
                        index = element.reflectAdj(cStack);
                        if (cStack.InsideC(element.GetNode(index)) > 0.0)
                        {
                            SwapDiagonal(cStack, element);
                            if (element.Status != StatusFlag.NotActive)
                            {
                                element.Deactivate();
                                DeleteTriangle(element);
                                cStack = PushTriangle(cStack, element);
                            }
                            changed = 1;
                            break;
                        }
                    }
                }
                if (changed == 0)
                {
                    NextTriangle = cStack.NextTriangle;
                    AppendTriangle(cStack);
                    cStack.Activate();
                    cStack = NextTriangle;
                }
            }
            return;
        }
        /// <summary>
        /// Добавление теугольника в двусвязный стек
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="tP"></param>
        /// <returns></returns>
        public RVTriangle PushTriangle(RVTriangle stack, RVTriangle tP)
        {
            if (tP != null)
            {
                tP.NextTriangle = stack;
                if (stack != null)
                    stack.prevTriangle = tP;
                return tP;
            }
            else
            {
                return stack;
            }
        }

        public RVTriangle PopTriangle(RVTriangle stack)
        {
            return stack.NextTriangle;
        }

        public void SwapDiagonal(RVTriangle tP1, RVTriangle tP2)
        {
            RVNode nP0;
            RVNode nP1;
            RVNode nP2;
            RVNode nP3;
            RVElement pP1;
            RVElement pP2;
            RVElement pP3;

            int index = tP2.reflectAdj(tP1);       // Find number of common edge in second triangle
            int indexp = index + 1;
            if (indexp == 3)                            // Next edge number
                indexp = 0;
            int indexm = index - 1;
            if (indexm == -1)                       // Previous edge number
                indexm = 2;

            int index1 = tP1.reflectAdj(tP2);      // Find number of common edge in first triangle
            int index1p = index1 + 1;
            if (index1p == 3)                           // Next edge number
                index1p = 0;
            int index1m = index1 - 1;
            if (index1m == -1)                      // Previous edge number
                index1m = 2;

            nP0 = tP1.GetNode(index1);                 // The node that started it all
            nP1 = tP1.GetNode(index1p);
            nP2 = tP1.GetNode(index1m);                    // Save 4 CountNodes in quadrilateral
            nP3 = tP2.GetNode(index);

            pP1 = tP1.GetOwner(index1m);
            if (pP1 != null)
            {
                index = pP1.reflectAdj(tP1);       // Adjust pointers from adjacent triangles
                pP1.SetOwner(index, tP2);
            }

            pP2 = tP2.GetOwner(indexm);
            if (pP2 != null)
            {
                index = pP2.reflectAdj(tP2);
                pP2.SetOwner(index, tP1);
            }

            pP3 = tP1.GetOwner(index1p);

            tP1.SetNode(0, nP0);
            tP1.SetNode(1, nP3);
            tP1.SetNode(2, nP2);
            tP1.SetOwner(0, pP2);    // Setup new triangle 1
            tP1.SetOwner(1, pP3);
            tP1.SetOwner(2, tP2);

            tP2.SetNode(0, nP0);
            tP2.SetNode(1, nP1);
            tP2.SetNode(2, nP3);
            tP2.SetOwner(0, tP2.GetOwner(indexp));   // Set up new triangle 2
            tP2.SetOwner(1, tP1);
            tP2.SetOwner(2, pP1);
        }

        public int checkDuplicateNodes()
        {
            double minDist = 0.0001;

            if (CountNodes == 0 || CountNodes == 1)
            {
                return CountNodes;
            }

            if (CountNodes == 2)
            {
                RVNode np1 = firstNode;
                RVNode np2 = (RVNode)np1.Next;

                if (np1.Distance(np2) < MINDIST)
                {
                    np2.Fixed = RVFixedNodeFlag.deletedNode;
                }
                return CountNodes;
            }

            if (CountNodes > 2)
            {
                if (boundBox.y2 - boundBox.y1 < minDist)
                {
                    if (boundBox.x2 - boundBox.x1 < minDist)
                    {
                        RVNode elemNodes = firstNode;
                        while (elemNodes != null)
                        {
                            elemNodes.Fixed = RVFixedNodeFlag.deletedNode;
                            elemNodes = (RVNode)elemNodes.Next;
                        }
                        return CountNodes;
                    }
                }

                double ym = (boundBox.y1 + boundBox.y2) / 2.0;
                double xm = (boundBox.x1 + boundBox.x2) / 2.0;

                RVBox qBox;                               // NE subquadrant
                qBox.x1 = xm; qBox.x2 = boundBox.x2;
                qBox.y1 = ym; qBox.y2 = boundBox.y2;
                subQuadrant[0] = new RVQuadrantGenerator(qBox);
                // NW subquadrant
                qBox.x1 = boundBox.x1; qBox.x2 = xm;
                qBox.y1 = ym; qBox.y2 = boundBox.y2;
                subQuadrant[1] = new RVQuadrantGenerator(qBox);
                // SW subquadrant
                qBox.x1 = boundBox.x1; qBox.x2 = xm;
                qBox.y1 = boundBox.y1; qBox.y2 = ym;
                subQuadrant[2] = new RVQuadrantGenerator(qBox);
                // SE subquadrant
                qBox.x1 = xm; qBox.x2 = boundBox.x2;
                qBox.y1 = boundBox.y1; qBox.y2 = ym;
                subQuadrant[3] = new RVQuadrantGenerator(qBox);
                // Distribute Nodes
                RVNode nP = firstNode;
                RVNode NextNode;
                while (nP != null)
                {
                    NextNode = (RVNode)nP.Next;
                    if (nP.Y >= ym)
                    {
                        if (nP.X >= xm)
                        {
                            subQuadrant[0].AddNode(nP);
                        }
                        else
                        {
                            subQuadrant[1].AddNode(nP);
                        }
                    }
                    else
                    {
                        if (nP.X < xm)
                            subQuadrant[2].AddNode(nP);
                        else
                            subQuadrant[3].AddNode(nP);
                    }
                    nP = NextNode;
                }
                CountNodes = 0;
                firstNode = LastNode = null;
                // Recursion
                for (int i = 0; i < 4; i++)
                {
                    subQuadrant[i].checkDuplicateNodes();
                }
                for (int i = 0; i < 4; i++)
                {
                    RecoverSubQuadInfo(i);
                    subQuadrant[i] = null;
                }
            }
            return CountNodes;
        }
    }
}
