//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 11.08.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace TriMeshGeneratorLib.Advance
{ 
    using CommonLib.Mesh.RVData;
    using System.Collections.Generic;

    /// <summary>
    /// Генератор КЭ сетки через рекурсивное квадратное окно
    /// </summary>
    public class RivQuadrantGenerator
    {
        public static double MINDIST = 0.0001;
        /// <summary>
        /// 4 subquadrants ne, nw, sw, se
        /// подобласти квадрата
        /// </summary>
        protected RivQuadrantGenerator[] subQuadrant = new RivQuadrantGenerator[4];
        /// <summary>
        /// список узлов
        /// </summary>
        public List<RivNode> QNodes = new List<RivNode> ();
        /// <summary>
        /// список треугольников
        /// </summary>
        public List<RivTriangle> QTriangles = new List<RivTriangle>();
        /// <summary>
        /// Дуга на основе вершин треугольника
        /// </summary>
        public RivTriangle BaseArc;
        /// <summary>
        /// ящик границы области
        /// </summary>
        protected RVBox boundBox;
        /// <summary>
        /// Конструктор с пределами границы области
        /// </summary>
        /// <param name="limits"></param>
        public RivQuadrantGenerator(RVBox limits)
        {
            subQuadrant[0] = null;
            subQuadrant[1] = null;
            subQuadrant[2] = null;
            subQuadrant[3] = null;
            BaseArc = null;
            boundBox = limits;
        }
        /// <summary>
        ///  Рекурсивный метод построения триангуляции по заданным вершинам
        /// </summary>
        /// <returns></returns>
        public int TriangulateNodes()
        {
            if (QNodes.Count == 0)
                BaseArc = null;  // Делаем дугу так, чтобы она никуда не указывала
            if (QNodes.Count == 3)
            {
                RivNode np1 = QNodes[0];
                RivNode np2 = QNodes[1];
                RivNode np3 = QNodes[2];
                // Check for duplicate QNodes.Count
                double minDist = 0.0001;        
                if (np1.Distance(np2) < minDist)
                {
                    QNodes.Remove(np2);
                }
                if (np1.Distance(np3) < minDist)
                {
                    QNodes.Remove(np3);
                }
                if ((np2 != null) && (np1 != null))
                {
                    if (np2.Distance(np3) < minDist)
                        QNodes.Remove(np3);
                }
                if (QNodes.Count == 3)
                {
                    // Создаем треугольник, окруженный дугами
                    // Make a triangle surrounded by arcs
                    RivTriangle tp = new RivTriangle(1, np1, np2, np3);
                    QTriangles.Add(tp);
                    //firstTriangle = lastTriangle = tp;
                    //tp.NextTriangle = tp.prevTriangle = null;
                    // считаем площадь треугольника
                    double area = tp.Area();
                    if (area < 0.0)
                    {
                        // если площадь треугольника отрицательная меняем местами узлы 1 и 2
                        tp.SetNode(1, np3);
                        tp.SetNode(2, np2);
                    }
                    // сопряженные треугольники без третьего узла
                    RivTriangle arc0 = new RivTriangle(1, tp.GetNode(0), tp.GetNode(1), null);
                    RivTriangle arc1 = new RivTriangle(1, tp.GetNode(1), tp.GetNode(2), null);
                    RivTriangle arc2 = new RivTriangle(1, tp.GetNode(2), tp.GetNode(0), null);
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
                    BaseArc = arc1;
                }
                // RVCdgIOut.put_elm(firstTriangle,true, QNodes.Count);
            }
            if (QNodes.Count == 2)
            {
                RivNode np1 = QNodes[0];
                RivNode np2 = QNodes[1];
                // Check for duplicate QNodes.Count
                double minDist = 0.0001;        
                if (np1.Distance(np2) < minDist)
                {
                    QNodes.Remove(np2);
                }
                if (QNodes.Count == 2)
                {
                    // Make a double arc between 2 QNodes.Count
                    RivTriangle arc1 = new RivTriangle(1, np1, np2, null);
                    RivTriangle arc2 = new RivTriangle(1, np2, np1, null);
                    arc1.SetOwner(0, arc2);
                    arc1.SetOwner(1, arc2);
                    arc1.SetOwner(2, arc2);

                    arc2.SetOwner(0, arc1);
                    arc2.SetOwner(1, arc1);
                    arc2.SetOwner(2, arc1);
                    BaseArc = arc1;
                }
                // RVCdgIOut.put_elm(firstTriangle, true, QNodes.Count);
            }
            if (QNodes.Count == 1)
            {                           // Make an arc to point to node
                RivNode np1 = QNodes[0];
                RivTriangle arc1 = new RivTriangle(1, np1, np1, null);
                arc1.SetOwner(0, arc1);
                arc1.SetOwner(1, arc1);
                BaseArc = arc1;
                // RVCdgIOut.put_elm(firstTriangle, true, QNodes.Count);
            }
            if (QNodes.Count > 3)
            {
                // Подразделить
                double ym = (boundBox.y1 + boundBox.y2) / 2.0;
                double xm = (boundBox.x1 + boundBox.x2) / 2.0;
                RVBox qBox;
                // NE subquadrant  // прямоугоьник - подобласть СВ
                qBox = new RVBox(xm, ym, boundBox.x2, boundBox.y2);
                //qBox.x1 = xm; qBox.x2 = boundBox.x2;
                //qBox.y1 = ym; qBox.y2 = boundBox.y2;
                subQuadrant[0] = new RivQuadrantGenerator(qBox);
                // NW subquadrant // прямоугоьник - подобласть  СЗ
                //qBox.x1 = boundBox.x1; qBox.x2 = xm;
                //qBox.y1 = ym; qBox.y2 = boundBox.y2;
                qBox = new RVBox(boundBox.x1, ym, xm, boundBox.y2);
                subQuadrant[1] = new RivQuadrantGenerator(qBox);
                // SW subquadrant // прямоугоьник - подобласть  ЮЗ
                //qBox.x1 = boundBox.x1; qBox.x2 = xm;
                //qBox.y1 = boundBox.y1; qBox.y2 = ym;
                qBox = new RVBox(boundBox.x1, boundBox.y1, xm, ym);
                subQuadrant[2] = new RivQuadrantGenerator(qBox);
                // SE subquadrant  // прямоугоьник - подобласть ЮВ
                //qBox.x1 = xm; qBox.x2 = boundBox.x2;
                //qBox.y1 = boundBox.y1; qBox.y2 = ym;
                qBox = new RVBox(xm, boundBox.y1, boundBox.x2, ym);
                subQuadrant[3] = new RivQuadrantGenerator(qBox);
                // Распределение узлов в подобласти (процесс рекурсивный)цикл по узлам
                for (int i = 0; i < QNodes.Count; i++)
                {
                    RivNode node = QNodes[i];
                    if (node.Y >= ym)
                    {
                        if (node.X >= xm)
                            subQuadrant[0].QNodes.Add(node);
                        else
                            subQuadrant[1].QNodes.Add(node);
                    }
                    else
                    {
                        if (node.X < xm)
                            subQuadrant[2].QNodes.Add(node);
                        else
                            subQuadrant[3].QNodes.Add(node);
                    }
                }
                QNodes.Clear();
                // Рекурсивная триангуляция для подобластей
                for (int i = 0; i < 4; i++)
                {
                    subQuadrant[i].TriangulateNodes();
                }

                int NNE = RecoverSubQuadInfo(0);
                RivTriangle bArcE = subQuadrant[0].BaseArc;
                int NNW = RecoverSubQuadInfo(1);
                RivTriangle bArcW = subQuadrant[1].BaseArc;
                RivTriangle bArcN;
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
                RivTriangle bArcS;
                int NNS = NNE + NNW;
                if (NNE == 0)
                    bArcS = bArcW;
                else if (NNW == 0)
                    bArcS = bArcE;
                else
                    bArcS = StitchLR(bArcW, bArcE);
                int NN = NNN + NNS;
                if (NNN == 0)
                    BaseArc = bArcS;
                else if (NNS == 0)
                    BaseArc = bArcN;
                else
                    BaseArc = StitchLR(bArcN, bArcS);
                // RVCdgIOut.put_elm(firstTriangle, true, QNodes.Count);
            }
            for (int i = 0; i < 4; i++)
                subQuadrant[i] = null;

            return QNodes.Count;
        }
        /// <summary>
        /// восстановление информации о SubQuads
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        int RecoverSubQuadInfo(int i)
        {
            // добавления узлов из внутреннего списка субквадранта в списка квадранта
            foreach (RivNode node in subQuadrant[i].QNodes)
                QNodes.Add(node);
            //// добавления узлов из внутреннего списка субквадранта в списка квадранта
            //RivNode nextNP;
            //RivNode nP = subQuadrant[i].QNodes[0];
            //while (nP != null)
            //{
            //    nextNP = (RivNode)nP.Next;
            //    AddNode(nP);
            //    nP = nextNP;
            //}
            // добавления треугольников из внутреннего списка субквадранта в списка квадранта
            foreach (RivTriangle triangle in subQuadrant[i].QTriangles)
                QTriangles.Add(triangle);
            //RivTriangle tP = subQuadrant[i].FirstTriangle;
            //RivTriangle nextTP;
            //while (tP != null)
            //{
            //    nextTP = tP.NextTriangle;
            //    AppendTriangle(tP);
            //    tP = nextTP;
            //}
            // количество узлов в субквадранте 
            //return subQuadrant[i].QNodes.Count;
            // количество узлов в субквадранте 
            return subQuadrant[i].QNodes.Count;
        }

        RivTriangle StitchLR(RivTriangle bArcL, RivTriangle bArcR)
        {
            RivTriangle topArcL = bArcL;
            RivTriangle topArcR = bArcR;
            RivTriangle topArc = new RivTriangle(1, topArcR.node0, topArcL.node1, null);
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
            RivTriangle botArc = new RivTriangle(1, topArcL.node1, topArcR.node0, null);
            topArc.SetOwner(2, botArc);
            botArc.SetOwner(2, topArc);
            botArc.SetOwner(0, topArcR);
            botArc.SetOwner(1, topArcL);
            topArcL.SetOwner(0, botArc);
            topArcR.SetOwner(1, botArc);
            RivTriangle cwArc = topArcL;
            RivTriangle ccwArc = topArcR;

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

            RivNode nP1 = ccwArc.node1;
            RivNode nP2 = botArc.node1;
            RivNode nP3 = botArc.node0;
            RivNode nP4 = cwArc.node0;

            double areaL, areaR;
            int goLeft;
            RivTriangle tP = new RivTriangle();

            while (true)
            {
                areaL = areaR = -1.0;
                goLeft = 0;
                tP = new RivTriangle(1, nP4, nP2, nP3);
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
        public void Delaunay(RivTriangle tP)
        {
            int index, changed = 0;
            RivTriangle element = new RivTriangle();
            RivTriangle NextTriangle = new RivTriangle();
            // стек трехугольников
            RivTriangle cStack = null;
            tP.Deactivate();
            cStack = PushTriangle(cStack, tP);
            while (cStack != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    changed = 0;
                    element = (RivTriangle)cStack.GetOwner(i);
                    if (element != null)
                    {
                        index = element.reflectAdj(cStack);
                        if (cStack.InsideC(element.GetNode(index)) > 0.0)
                        {
                            SwapDiagonal(cStack, element);
                            if (element.Status != StatusFlag.NotActive)
                            {
                                element.Deactivate();
                                QTriangles.Remove(element);
                                //DeleteTriangle(element);
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
                    QTriangles.Add(cStack);
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
        public RivTriangle PushTriangle(RivTriangle stack, RivTriangle tP)
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

        public RivTriangle PopTriangle(RivTriangle stack)
        {
            return stack.NextTriangle;
        }

        public void SwapDiagonal(RivTriangle tP1, RivTriangle tP2)
        {
            RivNode nP0;
            RivNode nP1;
            RivNode nP2;
            RivNode nP3;
            ARivElement pP1;
            ARivElement pP2;
            ARivElement pP3;

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
            nP2 = tP1.GetNode(index1m);                    // Save 4 QNodes.Count in quadrilateral
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

            if (QNodes.Count == 0 || QNodes.Count == 1)
            {
                return QNodes.Count;
            }

            if (QNodes.Count == 2)
            {
                RivNode np1 = QNodes[0];
                RivNode np2 = QNodes[1];

                if (np1.Distance(np2) < MINDIST)
                {
                    np2.Fixed = RVFixedNodeFlag.deletedNode;
                }
                return QNodes.Count;
            }

            if (QNodes.Count > 2)
            {
                if (boundBox.y2 - boundBox.y1 < minDist)
                {
                    if (boundBox.x2 - boundBox.x1 < minDist)
                    {
                        foreach (RivNode elemNodes in QNodes)
                        {
                            elemNodes.Fixed = RVFixedNodeFlag.deletedNode;
                        }
                        return QNodes.Count;
                    }
                }
                double ym = (boundBox.y1 + boundBox.y2) / 2.0;
                double xm = (boundBox.x1 + boundBox.x2) / 2.0;
                RVBox qBox;                               // NE subquadrant
                qBox.x1 = xm; qBox.x2 = boundBox.x2;
                qBox.y1 = ym; qBox.y2 = boundBox.y2;
                subQuadrant[0] = new RivQuadrantGenerator(qBox);
                // NW subquadrant
                qBox.x1 = boundBox.x1; qBox.x2 = xm;
                qBox.y1 = ym; qBox.y2 = boundBox.y2;
                subQuadrant[1] = new RivQuadrantGenerator(qBox);
                // SW subquadrant
                qBox.x1 = boundBox.x1; qBox.x2 = xm;
                qBox.y1 = boundBox.y1; qBox.y2 = ym;
                subQuadrant[2] = new RivQuadrantGenerator(qBox);
                // SE subquadrant
                qBox.x1 = xm; qBox.x2 = boundBox.x2;
                qBox.y1 = boundBox.y1; qBox.y2 = ym;
                subQuadrant[3] = new RivQuadrantGenerator(qBox);
                // Distribute Nodes
                //RivNode nP = firstNode;
                //RivNode NextNode;
                foreach (RivNode nP in QNodes)
                // while (nP != null)
                {
                    // NextNode = (RivNode)nP.Next;
                    if (nP.Y >= ym)
                    {
                        if (nP.X >= xm)
                        {
                            subQuadrant[0].QNodes.Add(nP);
                        }
                        else
                        {
                            subQuadrant[1].QNodes.Add(nP);
                        }
                    }
                    else
                    {
                        if (nP.X < xm)
                            subQuadrant[2].QNodes.Add(nP);
                        else
                            subQuadrant[3].QNodes.Add(nP);
                    }
                }
                QNodes.Clear();
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
            return QNodes.Count;
        }
    }
}
