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
    using System.Collections.Generic;
    using CommonLib.Mesh.RVData;

    // Basic Triangular Irregular Network Class 
    // Can be subclassed for various FEM meshes based on linear triangles.
    // Класс базовой треугольной нерегулярной сети
    // Можно разделить на подклассы для различных сеток МКЭ на основе линейных треугольников.
    public class RVMeshIrregular  // TIN
    {

        /// <summary>
        /// связанный список узлов
        /// </summary>
        public RVList nodesList = new RVList();
        /// <summary>
        /// связанный список элементов (треугольников)
        /// </summary>
        protected RVList triElementsList = new RVList();
        /// <summary>
        /// связанный список сегментов ребер треугольников
        /// </summary>
        public RVList edgesList = new RVList();
        /// <summary>
        /// связанный список граничных элементов
        /// </summary>
        protected RVList boundarySegmentsList = new RVList();
        /// <summary>
        /// связанный список сегментов (структурные линии)
        /// </summary>
        public RVList breakLinesList = new RVList();

        #region Временные массивы
        /// <summary>
        /// связанный список временных узлов
        /// </summary>
        protected RVList tmpNodesList = new RVList();
        /// <summary>
        /// связанный список временных граничных сегментов
        /// </summary>
        protected RVList tmpBoundarySegmentsList = new RVList();
        /// <summary>
        /// связанный список временных сегментов пространственных объектов (структурных линий)
        /// </summary>
        protected RVList tmpBreakLinesList = new RVList();
        /// <summary>
        /// связанный список узлов, которые были удалены
        /// </summary>
        protected RVList deadNodesList = new RVList();
        #endregion
        /// <summary>
        /// Прямоугольник включающий в себя все узлы
        /// </summary>
        protected RVBox limits;
        /// <summary>
        /// указатель на описание физической задачи
        /// </summary>
        protected IRVPhysics physics;
        /// <summary>
        /// текущий номер узла при генерации
        /// </summary>
        protected int serialNum;
        public RVMeshIrregular(IRVPhysics physics)
        {
            this.physics = physics;
            serialNum = 0;
        }
        /// <summary>
        /// Физика задачи
        /// </summary>
        public IRVPhysics GetPhysics => physics;
        /// <summary>
        /// Количество узлов
        /// </summary>
        public int CountNodes => nodesList.Count; 
        /// <summary>
        /// Количество граничных сегментов
        /// </summary>
        public int CountBoundarySegments => boundarySegmentsList.Count;
        /// <summary>
        /// диапазон генератора случайных чисел 0 - RAND_MAX
        /// </summary>
        int RAND_MAX = int.MaxValue;
        /// <summary>
        /// генератор случайных чисел
        /// </summary>
        protected Random random = new Random();
        /// <summary>
        /// Получить ссылку на первый граничный сегмент
        /// </summary>
        /// <returns></returns>
        public RVSegment FirstBoundarySegment=>(RVSegment)boundarySegmentsList.FirstItem(); 
        /// <summary>
        /// Получить ссылку на следующий граничный сегмент
        /// </summary>
        /// <returns></returns>
        public RVSegment NextBoundarySegment => (RVSegment)boundarySegmentsList.NextItem(); 
        public RVSegment FirstBreakLine =>(RVSegment)breakLinesList.FirstItem();
        public RVSegment NextBreakLinesList=>(RVSegment)breakLinesList.NextItem(); 
        public RVSegment FirstTmpBreakLinesList => (RVSegment)tmpBreakLinesList.FirstItem(); 
        public RVSegment NextTmpBreakLinesList => (RVSegment)tmpBreakLinesList.NextItem(); 
        public RVNode firstNode => (RVNode)nodesList.FirstItem(); 
        public RVNode NextNode => (RVNode)nodesList.NextItem(); 
        public RVTriangle FirstTriElements =>(RVTriangle)triElementsList.FirstItem(); 
        public RVTriangle NextTriElements =>(RVTriangle)triElementsList.NextItem(); 

        public RVNode GetNodeByID(int ID) { return (RVNode)nodesList.Get(ID); }
        public void AddNode(RVNode node) { nodesList.Add(node); }
        public void AddBoundarySegment(RVSegment bSegment) { boundarySegmentsList.Add(bSegment); }

        /// <summary>
        ///  чтение структурных линий
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public int ReadBreakLinesList(StreamReader file)
        {
            RVSegment segP;
            string[] lines;
            for (lines = RVcdgIO.GetLines(file); lines != null; lines = RVcdgIO.GetLines(file))
            {
                List<string> list = new List<string>();
                foreach (var s in lines)
                    if (s != "")
                        list.Add(s);
                lines = list.ToArray();
                int ID = int.Parse(lines[0]);
                int n_1 = int.Parse(lines[1]);
                int n_2 = int.Parse(lines[2]);
                RVNode nodeP1 = (RVNode)nodesList.GetIndexItem(n_1);
                RVNode nodeP2 = (RVNode)nodesList.GetIndexItem(n_2);
                segP = new RVSegment(ID, nodeP1, nodeP2);
                AddFeatureSeg(segP, breakLinesList);
            }
            return breakLinesList.Count;
        }
        /// <summary>
        ///  запись структурных линий
        /// </summary>
        public void WriteBreakLinesList(StreamWriter file)
        {
            for (int i = 1; i <= breakLinesList.Count; i++)
            {
                var elem = breakLinesList.GetIndexItem(i) as RVElement;
                elem.Write(file, elem);
            }
            file.WriteLine("\nno more breakline segments.");
        }

        public RVTriangle WhichTriHasNode(RVNode nodeP)
        {
            RVTriangle triangle;
            triangle = nodeP.TriangleNodeOwner;
            if (triangle != null)
            {
                triElementsList.SetCurrentItem(triangle);
            }
            return triangle;
        }
        /// <summary>
        /// Получить новый узел (для 1 потока)
        /// </summary>
        /// <returns></returns>
        public int GetNextNumber()
        {
            serialNum += 1;
            return serialNum;
        }
        public RVSegment AddFeatureSeg(RVSegment newfsegp, RVList fList)
        {
            int result = CheckFeatureSeg(newfsegp);
            if (result != -1)
            {
                fList.Add(newfsegp);
                return newfsegp;
            }
            else
            {
                newfsegp = null;
                return null;
            }
        }
        public int CheckFeatureSeg(RVSegment newfsegp)
        {
            int i1 = 1, i2 = 1;
            if ((newfsegp.GetNode(0).Fixed == RVFixedNodeFlag.deletedNode) ||
                (newfsegp.GetNode(1).Fixed == RVFixedNodeFlag.deletedNode))
            {
                return -1;
            }
            RVSegment fsegp = FirstBreakLine;
            while ((fsegp != null) && (fsegp != newfsegp))
            {
                i1 = newfsegp.intersect(fsegp);
                i2 = fsegp.intersect(newfsegp);
                if ((i1 == -1) && (i2 == -1))
                    return -1;
                fsegp = NextBreakLinesList;
            }
            fsegp = FirstTmpBreakLinesList;
            while ((fsegp != null) && (fsegp != newfsegp))
            {
                i1 = newfsegp.intersect(fsegp);
                i2 = fsegp.intersect(newfsegp);
                if ((i1 == -1) && (i2 == -1))
                    return -1;
                fsegp = NextTmpBreakLinesList;
            }
            fsegp = FirstBoundarySegment;
            while ((fsegp != null) && (fsegp != newfsegp))
            {
                i1 = newfsegp.intersect(fsegp);
                i2 = fsegp.intersect(newfsegp);
                if ((i1 == -1) && (i2 == -1))
                    return -1;
                fsegp = NextBoundarySegment;
            }
            return 1;
        }
        /// <summary>
        /// Триангуляция
        /// </summary>
        /// <returns></returns>
        public int Triangulate()
        {
            if (nodesList.Count < 3)
                return (0);
            // delete old triangles
            RVTriangle triangle = (RVTriangle)triElementsList.FirstItem();
            // RVCdgIOut.put_elm(triangle);
            while (triangle != null)
            {
                triElementsList.RemoveCurrentItem();
                triangle = (RVTriangle)triElementsList.FirstItem();
            }
            CheckDupNodes();
            GetLimits();
            // рекурсивный генератор сетки
            RVQuadrantGenerator triGenerator = new RVQuadrantGenerator(limits);

            RVNode[] nodesListist = new RVNode[nodesList.Count];
            

            int inode = 0;

            double xn, yn, zn;
            // массивы для псевдослучайного микро смещения координат узлов
            double[] dxx = {0.0000001, 0.0000005, 0.0000002, 0.0000006, 0.0000002,
                            0.0000007, 0.0000003, 0.0000001, 0.0000004, 0.0000009,
                            0.0000000, 0.0000003, 0.0000006, 0.0000004, 0.0000008 };
            double[] dyy = { 0.0000005, 0.0000002, 0.0000006, 0.0000002, 0.0000004,
                             0.0000007, 0.0000003, 0.0000001, 0.0000001, 0.0000004,
                             0.0000009, 0.0000000, 0.0000003, 0.0000006,  0.0000008 };

            //double[] dxx1 = {0.0000003, 0.0000002, 0.0000001, 0.0000004, 0.0000007,
            //                0.0000001, 0.0000005, 0.0000002, 0.0000001, 0.0000006,
            //                0.0000002, 0.0000007, 0.0000002, 0.0000007, 0.0000001 };
            //double[] dyy1 = { 0.0000002, 0.0000004, 0.0000003, 0.0000005, 0.0000001,
            //                 0.0000009, 0.0000004, 0.0000003, 0.0000002, 0.0000001,
            //                 0.0000006, 0.0000007, 0.0000003, 0.0000004,  0.0000005 };
            int idd = 0;
            RVNode node = firstNode;
            while (node != null)
            {
                nodesList.Remove(node);
                if (node.Fixed != RVFixedNodeFlag.deletedNode)
                {
                    nodesListist[inode] = node;
                    inode++;
                    // resets coordinates to prevent accumulated drift
                    // cбрасывает координаты для предотвращения накопления дрейфа
                    // x = xo; y = yo;
                    node.RestoreLocation();
                    // тряска координат
                    xn = node.X + dxx[idd];
                    yn = node.Y + dyy[idd];
                    idd++;
                    idd = idd % dxx.Length;
                    zn = node.Z;
                    node.Assignt(xn, yn, zn);
                    triGenerator.AddNode(node);
                }
                else
                {
                    deadNodesList.Add(node);
                }
                node = firstNode;
            }

            int Count = triGenerator.TriangulateNodes();

            for (int ii = 0; ii < inode; ii++)
            {
                // Console.WriteLine("N = {0}, x = {1:F5} y = {2:F5}", nodesListist[ii].getn(), nodesListist[ii].X, nodesListist[ii].Y);
                nodesList.Add(nodesListist[ii]);
            }
            nodesListist = null;

            RVTriangle NextTriangle;
            triangle = triGenerator.FirstTriangle;
            while (triangle != null)
            {
                NextTriangle = triangle.NextTriangle;
                // RVCdgIOut.put_elm(triangle,true,-1);
                triElementsList.Add(triangle);
                triangle = NextTriangle;
            }

            //for(RVTriangle nextArcP = )
            //public RVTriangle tP0() { return (RVTriangle)adj[0]; }
            //public RVTriangle tP1() { return (RVTriangle)adj[1]; }
            //public RVTriangle tP2() { return (RVTriangle)adj[2]; }
            //public override void SetOwner(int i, RVElement ap) { if (i < 3) adj[i] = ap; }
            RVTriangle arcP = triGenerator.BaseArc;
            RVTriangle nextArcP;
            arcP.tP1.SetOwner(0, null);
            while (arcP != null)
            {
                nextArcP = arcP.tP0;
                arcP.tP2.SetOwner(arcP.tsn, null);
                arcP = nextArcP;
            }
            triGenerator = null;

            for (int i = 1; i <= triElementsList.Count; i++)
            {           //	Renumber triangles
                triangle = (RVTriangle)triElementsList.GetIndexItem(i);
                triangle.ID = triangle.Index + 1;
                for (int j = 0; j < 3; j++)
                    triangle.GetNode(j).TriangleNodeOwner = triangle;
            }

            return triElementsList.Count;
        }

        public int CheckDupNodes()
        {
            // ищем размеры области RVBox limits 
            GetLimits();
            RVQuadrantGenerator triGenerator = new RVQuadrantGenerator(limits);
            // выделяем буферный массив
            RVNode[] nodesListist = new RVNode[nodesList.Count];
            // первый узел
            RVNode node = firstNode;
            // бежим по узла удаляя их из списка и занося в буферные массив и список
            int inode = 0;
            while (node != null)
            {
                nodesList.Remove(node);
                nodesListist[inode] = node;
                inode++;
                triGenerator.AddNode(node);
                node = firstNode;
            }
            // получаем количество дупликатов в 
            int Count = triGenerator.checkDuplicateNodes();
            // копируем из списка буфера в список сетки
            for (int ii = 0; ii < inode; ii++)
            {
                nodesList.Add(nodesListist[ii]);
            }
            nodesListist = null;
            triGenerator = null;
            return triElementsList.Count;
        }
        /// <summary>
        /// добавление нового узла в треугольник
        /// </summary>
        /// <param name="nodeP"></param>
        /// <param name="triangle"></param>
        public void InsertNewNode(RVNode nodeP, RVTriangle triangle)
        {
            RVNode nP1, nP2, nP3;
            RVTriangle tP1, tP2, tP3;
            RVElement element;
            RVList triStack = new RVList();
            int index;
            // Сохранить оригинальные узлы
            nP1 = triangle.GetNode(0);                         //	Save original tNodes
            nP2 = triangle.GetNode(1);
            nP3 = triangle.GetNode(2);

            tP1 = triangle;
            // Убрать существующий элемент из сетки
            triElementsList.SetCurrentItem(triangle);                      //	Take existing element out of mesh
            // Начать работу над тремя новыми
            triElementsList.RemoveCurrentItem();                       //  Start working on three new ones
            // Положите их в стек для проверки позже
            tP1.SetNode(0, nodeP);                         //	Put them on the stack for checking later
            triStack.Push(tP1);
            tP2 = new RVTriangle(triElementsList.Count + 2, nodeP, nP3, nP1);
            triStack.Push(tP2);
            tP3 = new RVTriangle(triElementsList.Count + 3, nodeP, nP1, nP2);
            triStack.Push(tP3);

            tP2.SetOwner(0, tP1.GetOwner(1));
            tP2.SetOwner(1, tP3);
            tP2.SetOwner(2, tP1);

            tP3.SetOwner(0, tP1.GetOwner(2));
            tP3.SetOwner(1, tP1);
            tP3.SetOwner(2, tP2);

            tP1.SetOwner(1, tP2);
            tP1.SetOwner(2, tP3);

            if ((element = tP2.GetOwner(0)) != null)
            {
                index = element.reflectAdj(tP1);
                element.SetOwner(index, tP2);
            }

            if ((element = tP3.GetOwner(0)) != null)
            {
                index = element.reflectAdj(tP1);
                element.SetOwner(index, tP3);
            }

            while ((tP1 = (RVTriangle)triStack.Pop()) != null)
            {   // while any tringles on stack
                if ((element = tP1.GetOwner(0)) != null)
                {               // if not adjacent to nothing
                    if (element.Count() == 3)
                    {                       // if adjacent to a triangle (could be a boundary segment)
                        tP2 = (RVTriangle)element;
                        if (tP2.InsideC(nodeP) >= 0)
                        {           //	If not Delauney
                            triElementsList.SetCurrentItem(tP2);
                            triElementsList.RemoveCurrentItem();           // Take second triangle out for now
                            SwapDiagonal(tP1, tP2);             //	We need to swap
                            triStack.Push(tP1);                 //  New triangles need to be checked
                            triStack.Push(tP2);                 //	afterward so put both on stack
                        }
                        else
                        {
                            triElementsList.Insert(tP1);
                            for (int i = 0; i < 3; i++)
                            {
                                tP1.GetNode(i).TriangleNodeOwner = tP1;
                            }
                        }
                    }
                    else
                    {
                        triElementsList.Insert(tP1);           // RVTriangle good so accept back into mesh
                        for (int i = 0; i < 3; i++)
                        {
                            tP1.GetNode(i).TriangleNodeOwner = tP1;
                        }
                    }
                }
                else
                {
                    triElementsList.Insert(tP1);
                    for (int i = 0; i < 3; i++)
                    {
                        tP1.GetNode(i).TriangleNodeOwner = tP1;
                    }
                }
            }
        }
        /// <summary>
        /// Генерация треугольников по готовым вершинам - рекурсивная функция
        ///                         triangulateCons
        /// </summary>
        /// <returns></returns>
        public int GeneratingTriangularMesh()
        {
            int nelms;
            if (nodesList.Count < 3)
                return (0);
            nelms = Triangulate();
            if (boundarySegmentsList.Count < 3)
                return nelms;

            RVTriangle triangle1, triangle2, boundaryTriangle, triangle, triangleAdjSegments;
            RVElement adjElement;
            RVSegment nextBoundarySegment;
            RVSegment boundarySegment = FirstBoundarySegment;
            triangleAdjSegments = null;
            while (boundarySegment != null)
            {
                boundarySegment.GetNode(0).BoundNodeFlag = BoundaryNodeFlag.boundaryNode;
                // int notGood = 0;
                // Обрабатываем граничные сегменты
                while ((boundaryTriangle = whichTriHasSeg(boundarySegment, triangleAdjSegments)) == null)
                {
                    triangle1 = GetFirstTriElementsListOnSeg(boundarySegment);
                    triangle2 = GetSecondTriOnSeg(triangle1, boundarySegment);
                    SwapDiagonal(triangle1, triangle2);
                }
                InsertBoundarySegment(boundarySegment, boundaryTriangle);
                for (int side = 0; side < 2; side++)
                {
                    triangle1 = (RVTriangle)boundarySegment.GetOwner(side);
                    if (triangle1 != null)
                    {
                        for (int adj = 0; adj < 3; adj++)
                        {
                            adjElement = triangle1.GetOwner(adj);
                            if (adjElement != null && adjElement != boundarySegment)
                            {
                                if (adjElement.Count() == 3)
                                {
                                    triangle2 = (RVTriangle)adjElement;
                                    if (triangle1.Area() <= 0.0 || triangle2.Area() <= 0.0)
                                    {
                                        SwapDiagonal(triangle1, triangle2);
                                    }
                                }
                            }
                        }
                    }
                }
                nextBoundarySegment = NextBoundarySegment;
                triangleAdjSegments = null;
                if (nextBoundarySegment != null)
                {
                    if (nextBoundarySegment.GetNode(0) == boundarySegment.GetNode(1))
                    {
                        triangleAdjSegments = (RVTriangle)boundarySegment.GetOwner(0);
                    }
                }
                boundarySegment = nextBoundarySegment;
            }
            /*
				boundarySegment = FirstBoundarySegment();
				while(boundarySegment != null) 
                {
					boundaryTriangle = whichTriHasSeg(boundarySegment);
					InsertBoundarySegment(boundarySegment,boundaryTriangle);
					for(int side=0;side<2;side++)
                    {
						triangle1 = (RVTriangle) boundarySegment.GetOwner(side);
						if(triangle1 != null)
                        {
							for(int adj=0;adj<3;adj++)
                            {
								adjElement = triangle1.GetOwner(adj);
								if(adjElement != null && adjElement != boundarySegment)
                                {
									if(adjElement.Count() == 3) 
                                    {
										triangle2 = (RVTriangle) adjElement;
										if(triangle1.Area()<=0.0 || triangle2.Area()<=0.0)
                                        {
											SwapDiagonal(triangle1,triangle2);
										}
									}
								}
							}
						}
					}
					boundarySegment = NextBoundarySegment();
				}
			*/
            // Деактивируем (но сохраним) треугольники, которые находятся за пределами границы
            boundarySegment = (RVSegment)boundarySegmentsList.FirstItem();
            while (boundarySegment != null)
            {
                if ((adjElement = boundarySegment.GetOwner(1)) != null)
                    adjElement.Deactivate();
                boundarySegment = (RVSegment)boundarySegmentsList.NextItem();
            }
            // Ловим внешние треугольники без ребер на границе
            triangle = (RVTriangle)triElementsList.FirstItem();
            while (triangle != null)
            {
                if  ((triangle.GetOwner(0) == null) ||
                     (triangle.GetOwner(1) == null) ||
                     (triangle.GetOwner(2) == null))
                            triangle.Deactivate();
                else if ((triangle.GetOwner(0).Status == StatusFlag.NotActive) ||
                          (triangle.GetOwner(1).Status == StatusFlag.NotActive) ||
                          (triangle.GetOwner(2).Status == StatusFlag.NotActive))
                                triangle.Deactivate();
                
                if (triangle.Status == StatusFlag.Activate)
                {
                    for (int j = 0; j < 3; j++)
                        triangle.GetNode(j).TriangleNodeOwner = triangle;
                }
                triangle = (RVTriangle)triElementsList.NextItem();
            }
            // Перенумеровать треугольники
            for (int i = 1; i <= triElementsList.Count; i++)
            {
                triangle = (RVTriangle)triElementsList.GetIndexItem(i);
                triangle.ID = triangle.Index + 1;
            }
            // Перенумеровать узлы 
            RVNode node;
            for (int i = 1; i <= nodesList.Count; i++)
            {
                node = (RVNode)nodesList.GetIndexItem(i);
                node.ID = node.Index + 1;
            }
            return triElementsList.Count;
        }
        /// <summary>
        /// сборка новых граней для треугольников
        /// </summary>
        /// <returns></returns>
        public int GeneratingEdges()
        {
            // Удалить старые грани 
            RVSegment edges = (RVSegment)edgesList.FirstItem();      
            while (edges != null)
            {
                edgesList.RemoveCurrentItem();
                edges = (RVSegment)edgesList.FirstItem();
            }
            // Удалите ссылки на грани в треугольниках
            for (RVTriangle triangle = FirstTriElements; triangle != null; triangle = NextTriElements)
            {
                triangle.SetEdge(0, null);
                triangle.SetEdge(1, null);
                triangle.SetEdge(2, null);
            }
            int n1, n2, indexEdges, ne = 0;
            RVTriangle element;
            RVElement aeP;

            for(RVTriangle triangle = FirstTriElements; triangle != null; triangle = NextTriElements)
            {
                if (triangle.Status == StatusFlag.Activate)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (triangle.GetEdge(i) == null)
                        {
                            ne++;
                            n1 = (i + 1) % 3;
                            n2 = (i + 2) % 3;
                            edges = new RVSegment(ne, triangle.GetNode(n1), triangle.GetNode(n2));
                            edges.SetOwner(0, triangle);
                            triangle.SetEdge(i, edges);
                            aeP = triangle.GetOwner(i);
                            if (aeP != null)
                            {
                                if (aeP.Count() == 3)
                                {
                                    element = (RVTriangle)aeP;
                                    indexEdges = element.reflectAdj(triangle);
                                    edges.SetOwner(1, element);
                                    element.SetEdge(indexEdges, edges);
                                }
                            }
                            edgesList.Add(edges);
                        }
                    }
                }
            }
            
            return ne;
        }

        public RVTriangle GetFirstTriElementsListOnSeg(RVSegment segment)
        {
            RVNode node = new RVNode(1);
            // безразмерный шаг для интерполяции вдоль сегмента
            double ds = 0.01;
            // создает новый узел в позиции pos = ds вдоль сегмента и интерполирует
            segment.LocateNodeAndInterpolation(ds, node);
            // поиск треугольника для нового узла
            RVTriangle triangle = WhichTriangle(node);
            while (triangle.Contains(segment.GetNode(0)) != triangle)
            {
                ds *= 0.1;
                segment.LocateNodeAndInterpolation(ds, node);
                triangle = WhichTriangle(node);
                //if (triangle == null)
                //    return null;
            }
            return triangle;
        }

        public RVTriangle GetSecondTriOnSeg(RVTriangle triangle, RVSegment segment)
        {
            int index = triangle.GetNodeIndex(segment.GetNode(0));
            RVElement element = triangle.GetOwner(index);
            RVTriangle t = element as RVTriangle;
            return t;
        }
        /// <summary>
        /// вставка грани/сегмента в треугольник 
        /// </summary>
        /// <param name="bSegment"></param>
        /// <param name="triangle"></param>
        public void InsertBoundarySegment(RVSegment bSegment, RVTriangle triangle)
        {
            //	Rotate the element indices so that
            //	the tail of the boundary segment is at 0
            // Поверните индексы элементов так, чтобы
            // хвост граничного сегмента находится в 0
            RVElement element;
            RVNode bNode = bSegment.GetNode(0);             
            if (bNode == triangle.GetNode(1))
            {               
                triangle.SetNode(1, triangle.GetNode(2));
                triangle.SetNode(2, triangle.GetNode(0));
                triangle.SetNode(0, bNode);
                element = triangle.GetOwner(1);
                triangle.SetOwner(1, triangle.GetOwner(2));
                triangle.SetOwner(2, triangle.GetOwner(0));
                triangle.SetOwner(0, element);
            }
            else 
                if (bNode == triangle.GetNode(2))
                {
                    triangle.SetNode(2, triangle.GetNode(1));
                    triangle.SetNode(1, triangle.GetNode(0));
                    triangle.SetNode(0, bNode);
                    element = triangle.GetOwner(2);
                    triangle.SetOwner(2, triangle.GetOwner(1));
                    triangle.SetOwner(1, triangle.GetOwner(0));
                    triangle.SetOwner(0, element);
                }
            //	triangles, 0 is to left (Inside) of segment
            // треугольники, 0 слева (внутри) сегмента
            element = triangle.GetOwner(2);
            bSegment.SetOwner(0, triangle);
            bSegment.SetOwner(1, element);
            triangle.SetOwner(2, bSegment);                            
            if (element != null)
                element.SetOwner(element.reflectAdj(triangle), bSegment);
            return;
        }

        public void SwapDiagonal(RVTriangle tP1, RVTriangle tP2)
        {
            RVNode nP0, nP1, nP2, nP3;
            RVElement pP1, pP2, pP3;
            // Найдите число общих ребер во втором треугольнике
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

        public RVSegment ThreadFeature(RVSegment fsegp, RVList segmentsListist)
        {
            RVNode n = new RVNode(1);
            fsegp.LocateNodeAndInterpolation(0.001, n);
            RVTriangle triangle;
            if ((triangle = WhichTriangle(n)) == null)
            {
                return (RVSegment)segmentsListist.NextItem();
            }
            if ((triangle.Contains(fsegp)) == triangle)
            {
                return (RVSegment)segmentsListist.NextItem();
            }
            RVSegment revfseg = new RVSegment(1, fsegp.GetNode(1), fsegp.GetNode(0));
            if ((triangle.Contains(revfseg)) == triangle)
            {
                return (RVSegment)segmentsListist.NextItem();
            }
            int nindex = triangle.GetNodeIndex(fsegp.GetNode(0));
            if (nindex < 0)
                return (RVSegment)segmentsListist.NextItem();
            nindex += 1;
            if (nindex > 2)
                nindex = 0;
            RVNode on1 = triangle.GetNode(nindex);
            nindex += 1;
            if (nindex > 2)
                nindex = 0;
            RVNode on2 = triangle.GetNode(nindex);
            RVSegment opSeg = new RVSegment(1, on1, on2);
            double Distance = fsegp.intersectd(opSeg);
            if ((Distance > 0.0) && (Distance < 1.0))
            {
                bisectSeg(fsegp, segmentsListist, Distance);
                if ((triangle = WhichTriangle(fsegp.GetNode(1))) != null)
                {
                    InsertNewNode(fsegp.GetNode(1), triangle);
                }
            }
            return (RVSegment)segmentsListist.NextItem();
        }
        public double TriCenterDz(RVTriangle triangle)
        {
            RVNode tCNode = new RVNode(100);
            RVNode tinNode = new RVNode(101);
            triangle.LocateNodeAtCenter(tCNode);
            RVTriangle tinTri;
            double dz = 0.0;
            if ((tinTri = WhichTriangle(tCNode)) != null)
            {
                tinTri.LocateNodeAndInterpolation(tCNode.X, tCNode.Y, tinNode);
                dz = tCNode.Z - tinNode.Z;
            }
            return dz;
        }

        public RVBox GetLimits(double margin = 0.01)
        {
            // первый узел
            RVNode node = (RVNode)nodesList.FirstItem();
            if (node != null)
            {
                limits.x1 = node.X;
                limits.y1 = node.Y;
                limits.x2 = node.X;
                limits.y2 = node.Y;
                node = (RVNode)nodesList.NextItem();
            }
            // ищем минимах бокс
            while (node != null)
            {
                if (node.X < limits.x1)
                    limits.x1 = node.X;
                if (node.X > limits.x2)
                    limits.x2 = node.X;
                if (node.Y < limits.y1)
                    limits.y1 = node.Y;
                if (node.Y > limits.y2)
                    limits.y2 = node.Y;
                node = (RVNode)nodesList.NextItem();
            }
            // ищем интервал области по х
            double dx = limits.x2 - limits.x1;
            // и по у
            double dy = limits.y2 - limits.y1;
            // расширяем коробку
            limits.x1 -= margin * dx;
            limits.x2 += margin * dx;
            limits.y1 -= margin * dy;
            limits.y2 += margin * dy;
            return limits;
        }
        int[] check = null;
        /// <summary>
        /// поиск треугольника по узлу
        /// </summary>
        /// <param name="nodeP"></param>
        /// <returns></returns>
        public RVTriangle WhichTriangle(RVNode nodeP)
        {
            RVElement element = null, triangle;
            triangle = (RVElement)triElementsList.CurrentItem();
            if (triangle == null)
                return null;

            MemLogLib.MEM.Alloc(triElementsList.Count, ref check);
            while (true)
            {
                element = triangle.Inside(nodeP);
                if ((element == triangle) || (element == null))
                    break;

                if (check[element.ID] > 2)
                {
                    Console.WriteLine("Пойман цикл при поиске");
                    element = null;
                    break;
                }
                check[element.ID] += 1;

                triangle = element;
                // ЕСЛИ МНОГИЕ УЗЛЫ УДАЛЯЮТСЯ В ГРАНИЦЕ, ОНА ЛОМАЕТСЯ !!
                if (element.Count() == 3) // IF MANY NODES ARE DELETED IN BOUNDARY, IT CRASHES!!
                    triElementsList.SetCurrentItem(element);
            }
            if (element != null)
            {
                triElementsList.SetCurrentItem(element);
            }
            return (RVTriangle)element;
        }

        RVTriangle TriangleListForNode(RVNode node, RVTriangle sTP)
        {
            RVTriangle tPf;
            if (sTP == null)
                tPf = node.TriangleNodeOwner;
            else
                tPf = sTP;

            RVTriangle tPs = tPf;
            RVTriangle tP1;
            RVNode nP1;
            int index = 0;
            int hitEdge = 0;
            RVElement aEP;
            if (tPs != null)
            {
                tPs.prevTriangle = tPs.NextTriangle = null;
                do
                {
                    for (int i = 0; i < 3; i++)
                    {
                        nP1 = tPs.GetNode(i);
                        if (nP1 == node)
                        {
                            index = i - 1;
                            if (index == -1)
                                index = 2;
                            break;
                        }
                    }
                    aEP = tPs.GetOwner(index);
                    if (aEP != null)
                    {
                        if (aEP.Count() == 3)
                        {
                            tP1 = (RVTriangle)aEP;
                            if (tP1 != tPf)
                            {
                                tPs.NextTriangle = tP1;
                                tP1.prevTriangle = tPs;
                                tP1.NextTriangle = null;
                            }
                            tPs = tP1;
                        }
                        else
                            hitEdge = 1;
                    }
                    else
                        hitEdge = 1;
                } while ((tPs != tPf) && (hitEdge == 0));
                if (hitEdge == 1)
                {
                    tPs = tPf;
                    hitEdge = 0;
                    do
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            nP1 = tPs.GetNode(i);
                            if (nP1 == node)
                            {
                                index = i + 1;
                                if (index == 3)
                                    index = 0;
                                break;
                            }
                        }
                        aEP = tPs.GetOwner(index);
                        if (aEP != null)
                        {
                            if (aEP.Count() == 3)
                            {
                                tP1 = (RVTriangle)aEP;
                                tPs.prevTriangle = tP1;
                                tP1.NextTriangle = tPs;
                                tP1.prevTriangle = null;
                                tPs = tP1;
                            }
                            else
                                hitEdge = 1;
                        }
                        else
                            hitEdge = 1;
                    } while (hitEdge == 0);
                    tPf = tPs;
                }
            }

            return tPf;
        }

        public RVTriangle whichTriHasSeg(RVSegment segment, RVTriangle sTP)
        {
            RVTriangle triangle = TriangleListForNode(segment.GetNode(0), sTP);
            while (triangle != null)
            {
                if (triangle.Contains(segment) == triangle)
                    break;
                triangle = triangle.NextTriangle;
            }
            return triangle;
        }

        public RVSegment bisectSeg(RVSegment segP, RVList segmentsList, double Distance = 0.5)
        {
            RVNode nodeP;
            RVSegment nSegP;

            if (Distance > 0.99)
                Distance = 0.99;
            if (Distance < 0.01)
                Distance = 0.01;
            nodeP = physics.CreateNewNode(GetNextNumber());
            if ((segP.GetNode(0).BoundNodeFlag == BoundaryNodeFlag.boundaryNode) &&
                    (segP.GetNode(1).BoundNodeFlag == BoundaryNodeFlag.boundaryNode))
                nodeP.BoundNodeFlag = BoundaryNodeFlag.boundaryNode;
            if ((segP.GetNode(0).Fixed == RVFixedNodeFlag.fixedNode)
                 || (segP.GetNode(0).Fixed == RVFixedNodeFlag.slidingNode))
                nodeP.Fixed = RVFixedNodeFlag.slidingNode;
            nodesList.Add(nodeP);
            segP.LocateNodeAndInterpolation(Distance, nodeP);
            nSegP = physics.CreateNewSegment(segmentsList.Count + 1,
                    nodeP, segP.GetNode(1), segP);
            segP.SetNode(1, nodeP);
            segmentsList.SetCurrentItem(segP);
            segmentsList.Insert(nSegP);

            RVSegment sp = (RVSegment)segmentsList.FirstItem();
            if (sp == segP)
                return sp;
            while (sp.Next != segP)
                sp = (RVSegment)segmentsList.NextItem();
            return sp;
        }


        //	Puts one RVNode into the RVMeshIrregular (from physics spec).
        //	Requires a previously triangulated data RVMeshIrregular, bedMesh,
        //	which masks inactive areas and provides interpolation data.
        //	Resulting nodeis in tmpNodesList which can be later accepted into nodesList or rejected.
        //	The number of CountNodes created are returned.
        public RVNode AddOneNode(double x, double y, RVMeshIrregular boundTIN, RVMeshIrregular bedMesh)
        {
            int nGoodNodes = 0;
            int nodeNum = GetNextNumber();
            RVTriangle triangle, tBP;
            RVNode node;

            node = physics.CreateNewNode(nodeNum, x, y);
            if ((triangle = bedMesh.WhichTriangle(node)) != null)
            {
                if ((tBP = boundTIN.WhichTriangle(node)) != null)
                {
                    if (tBP.Status == StatusFlag.Activate)
                    {
                        triangle.LocateNodeAndInterpolation(x, y, node);
                        nGoodNodes += 1;
                        nodeNum += 1;
                        tmpNodesList.Add(node);
                    }
                    else
                    {
                        node = null;
                    }
                }
                else
                {
                    node = null;
                }
            }
            else
            {
                node = null;
            }
            return node;
        }

        public RVNode AddOneNode(double x, double y)
        {
            int nodeNum = GetNextNumber();
            RVNode node;
            node = physics.CreateNewNode(nodeNum, x, y);
            tmpNodesList.Add(node);
            return node;
        }

        /// <summary>
        /// Добавлена строка для проверки нулевого nsegP. Если да, то возвращает 2.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public int RemoveNode(RVNode node)
        {
            if (node.Fixed == RVFixedNodeFlag.slidingNode)
            {
                RVSegment segP = FirstBoundarySegment;
                RVSegment nsegP;
                while (segP != null)
                {
                    if (segP.GetNode(1) == node)
                    {
                        nsegP = (RVSegment)segP.Next;
                        if (nsegP == null) return 2;    //	Added line.  JDS
                        nsegP.SetNode(0, segP.GetNode(0));
                        boundarySegmentsList.Remove(segP);
                        segP = null;
                        node.BoundNodeFlag = BoundaryNodeFlag.internalNode;
                        break;
                    }
                    segP = NextBoundarySegment;
                }
                segP = FirstBreakLine;
                while (segP != null)
                {
                    if (segP.GetNode(1) == node)
                    {
                        nsegP = (RVSegment)segP.Next;
                        nsegP.SetNode(0, segP.GetNode(0));
                        breakLinesList.Remove(segP);
                        segP = null;
                        break;
                    }
                    segP = NextBreakLinesList;
                }
            }
            if ((node.Fixed == RVFixedNodeFlag.floatingNode) || (node.Fixed == RVFixedNodeFlag.slidingNode))
            {
                node.Fixed = RVFixedNodeFlag.deletedNode;
                return 1;
            }
            else
                return 0;
        }


        /// <summary>
        /// сглаживание сетки
        /// </summary>
        /// <param name="nTimes"></param>
        /// <param name="bedMesh"></param>
        /// <param name="bias"></param>
        /// <returns></returns>
        public double SmoothMesh(int nTimes, RVMeshIrregular bedMesh, double bias = 0.0)
        {
            RVNode node;
            RVTriangle triangle, triangleA, triangleB;
            RVNode node1;
            double a, sumx, sumy, suma, xN, yN, dx, dy, lx, ly, d;
            double[] xNew;
            double[] yNew;

            int j, index = 0, iin = 0;
            bias = Math.Sqrt(bias);

                
            xNew = new double[nodesList.Count];
            yNew = new double[nodesList.Count];

            for (int i = 0; i < nTimes; i++)
            {
                node = (RVNode)nodesList.FirstItem();

                while (node != null)
                {
                    xNew[iin] = node.X;
                    yNew[iin] = node.Y;
                    if ((node.Fixed == RVFixedNodeFlag.floatingNode) && (node.BoundNodeFlag == BoundaryNodeFlag.internalNode))
                    {
                        if ((triangle = WhichTriHasNode(node)) != null)
                        {
                            if (triangle.Status == StatusFlag.NotActive)
                            { // node has slipped outside boundary
                                node.Fixed = RVFixedNodeFlag.deletedNode;
                                xNew[iin] = node.X;
                                yNew[iin] = node.Y;
                            }
                            else
                            {
                                sumx = sumy = suma = 0.0;
                                triangleA = triangle;
                                do
                                {
                                    a = ((1.0 - bias) + bias * Math.Abs(bedMesh.TriCenterDz(triangleA))) *
                                        Math.Sqrt(Math.Abs(triangleA.Area())); // triangleA.Area();
                                    suma += 4.0 * a;
                                    sumx += node.X * a;
                                    sumy += node.Y * a;
                                    for (j = 0; j < 3; j++)
                                    {
                                        node1 = triangleA.GetNode(j);
                                        sumx += node1.X * a;
                                        sumy += node1.Y * a;
                                        if (node1 == node)
                                            index = j;
                                    }
                                    index -= 1;
                                    if (index == -1)
                                        index = 2;
                                    triangleA = (RVTriangle)triangleA.GetOwner(index);
                                } 
                                while (triangleA != triangle);
                                xNew[iin] = (node.X + sumx / suma) * 0.5;
                                yNew[iin] = (node.Y + sumy / suma) * 0.5;
                            }
                        }
                    }
                    node = (RVNode)nodesList.NextItem();
                    iin += 1;
                }

                RVSegment breakLine = FirstBreakLine;
                RVSegment breakLineA;
                while (breakLine != null)
                {
                    node = breakLine.GetNode(1);
                    if (node.Fixed == RVFixedNodeFlag.slidingNode)
                    {
                        triangle = WhichTriHasNode(node);
                        sumx = sumy = suma = 0.0;
                        triangleA = triangle;
                        do
                        {
                            a = ((1.0 - bias) + bias * Math.Abs(bedMesh.TriCenterDz(triangleA))) *
                                Math.Sqrt(Math.Abs(triangleA.Area())); // triangleA.Area();
                            suma += 4.0 * a;
                            sumx += node.X * a;
                            sumy += node.Y * a;
                            for (j = 0; j < 3; j++)
                            {
                                node1 = triangleA.GetNode(j);
                                sumx += node1.X * a;
                                sumy += node1.Y * a;
                                if (node1 == node)
                                    index = j;
                            }
                            index -= 1;
                            if (index == -1)
                                index = 2;
                            triangleA = (RVTriangle)triangleA.GetOwner(index);
                        }
                        while (triangleA != triangle);
                        xN = (node.X + sumx / suma) * 0.5;
                        yN = (node.Y + sumy / suma) * 0.5;
                        if ((breakLineA = (RVSegment)breakLine.Next) != null)
                        {
                            if (breakLineA.GetNode(0) == node)
                            {
                                dx = xN - breakLine.GetNode(0).X;
                                dy = yN - breakLine.GetNode(0).Y;
                                lx = breakLineA.GetNode(1).X - breakLine.GetNode(0).X;
                                ly = breakLineA.GetNode(1).Y - breakLine.GetNode(0).Y;
                                d = (dx * lx + dy * ly) / (lx * lx + ly * ly);
                                if (d < 0.33333)
                                    d = 0.33333;
                                else 
                                    if (d > 0.66667)
                                        d = 0.66667;
                                xN = breakLine.GetNode(0).X + d * lx;
                                yN = breakLine.GetNode(0).Y + d * ly;
                                node.Init(xN, yN, 100.0);
                                if ((triangleB = bedMesh.WhichTriangle(node)) != null)
                                    triangleB.LocateNodeAndInterpolation(xN, yN, node);
                            }
                        }
                    }
                    breakLine = NextBreakLinesList;
                }

                RVSegment boundarySegment = FirstBoundarySegment;
                RVElement element;
                while (boundarySegment != null)
                {
                    node = boundarySegment.GetNode(1);
                    if (node.Fixed == RVFixedNodeFlag.slidingNode)
                    {
                        triangle = (RVTriangle)boundarySegment.GetOwner(0);
                        sumx = sumy = suma = 0.0;
                        triangleA = triangle;
                        do
                        {
                            a = Math.Sqrt(Math.Abs(triangleA.Area()));
                            suma += 3.0 * a;
                            for (j = 0; j < 3; j++)
                            {
                                node1 = triangleA.GetNode(j);
                                sumx += node1.X * a;
                                sumy += node1.Y * a;
                                if (node1 == node)
                                    index = j;
                            }
                            index -= 1;
                            if (index == -1)
                                index = 2;
                            element = triangleA.GetOwner(index);
                            if (element.Count() == 3)
                                triangleA = (RVTriangle)element;
                            else
                                break;
                        } 
                        while (triangleA != triangle);
                        xN = (node.X + sumx / suma) * 0.5;
                        yN = (node.Y + sumy / suma) * 0.5;
                        breakLineA = (RVSegment)boundarySegment.Next;
                        dx = xN - boundarySegment.GetNode(0).X;
                        dy = yN - boundarySegment.GetNode(0).Y;
                        lx = breakLineA.GetNode(1).X - boundarySegment.GetNode(0).X;
                        ly = breakLineA.GetNode(1).Y - boundarySegment.GetNode(0).Y;
                        d = (dx * lx + dy * ly) / (lx * lx + ly * ly);
                        if (d < 0.33333)
                            d = 0.33333;
                        else if (d > 0.66667)
                            d = 0.66667;
                        xN = boundarySegment.GetNode(0).X + d * lx;
                        yN = boundarySegment.GetNode(0).Y + d * ly;
                        node.Init(xN, yN, 100.0);
                        triangleB = bedMesh.WhichTriangle(node);
                        if (triangleB != null)
                            triangleB.LocateNodeAndInterpolation(xN, yN, node);
                    }
                    boundarySegment = NextBoundarySegment;
                }

                node = firstNode;
                iin = 0;
                while (node != null)
                {
                    if (node.Fixed == RVFixedNodeFlag.floatingNode)
                    {
                        node.Init(xNew[iin], yNew[iin], 100.0);
                        if ((triangleB = bedMesh.WhichTriangle(node)) != null)
                            triangleB.LocateNodeAndInterpolation(xNew[iin], yNew[iin], node);
                    }
                    node = NextNode;
                    iin += 1;
                }
            }

            return 1.0;
        }

        #region Zerro Не используемые методы

        public int Nfsegs() { return breakLinesList.Count; }
        public int Nelms() { return triElementsList.Count; }
        public int Nedges() { return edgesList.Count; }
        public int NTnodes() { return tmpNodesList.Count; }
        public int NTfsegs() { return tmpBreakLinesList.Count; }
        public RVNode firstTNode() { return (RVNode)tmpNodesList.FirstItem(); }
        public RVNode nextTNode() { return (RVNode)tmpNodesList.NextItem(); }
        public void insertNode(RVNode node) { nodesList.Insert(node); }
        public void setCurrentNode(RVNode node) { nodesList.SetCurrentItem(node); }
        public RVSegment firstEdge() { return (RVSegment)edgesList.FirstItem(); }
        public RVSegment nextEdge() { return (RVSegment)edgesList.NextItem(); }
        public int checkDupNode(RVNode pnP)
        {
            int dupFlag = 0;
            RVNode node = firstNode;
            while (node != null)
            {
                if (Math.Abs(node.X - pnP.X) < 0.00001)
                {
                    if (Math.Abs(node.Y - pnP.Y) < 0.00001)
                    {
                        dupFlag = -1;
                        break;
                    }
                }
                node = NextNode;
            }
            return dupFlag;
        }

        //	New function added 5/2001, JDS
        //	This function checks all CountNodes to see if they still belong in the 
        //	dataset after a boundary redefine.  I originally tried the boundary
        //	redefine letting the system handle the CountNodes, but there was a problem 
        //	removing the first node from the list that resulted in a ghost node.
        //  Добавлена новая функция 5/2001, JDS. Эта функция проверяет все CountNodes, 
        //  чтобы убедиться, что они по-прежнему принадлежат набору данных после 
        //  переопределения границы. Первоначально я пытался переопределить границу, 
        //  позволяя системе обрабатывать CountNodes, но возникла проблема с удалением 
        //  первого узла из списка, что привело к появлению узла-призрака.
        public void CheckAllNodes()
        {
            RVTriangle triangle;
            RVNode node = (RVNode)nodesList.FirstItem();
            nodesList.SetCurrentItem(node);
            while (node != null)
            {
                triangle = WhichTriHasNode(node);
                if (triangle == null || (triangle.Status == StatusFlag.NotActive))
                {
                    node.Fixed = RVFixedNodeFlag.floatingNode;
                    RemoveNode(node);
                    nodesList.RemoveCurrentItem();
                    node = (RVNode)nodesList.CurrentItem();
                }
                else
                {
                    node = (RVNode)nodesList.NextItem();
                }
            }
        }

        public int CheckDupNodeTri(RVNode pnP, RVTriangle triangle)
        {
            //int dupFlag = 0;
            RVNode node;
            for (int i = 0; i < 3; i++)
            {
                node = triangle.GetNode(i);
                if (Math.Abs(node.X - pnP.X) < 0.00001)
                {
                    if (Math.Abs(node.Y - pnP.Y) < 0.00001)
                    {
                        return (-1);
                    }
                }
            }
            return (0);
        }

        public void checkAllFeatureSegs()
        {
            RVTriangle triP1;
            RVTriangle triP2;
            RVSegment segP = FirstBreakLine;
            while (segP != null)
            {
                triP1 = WhichTriangle(segP.GetNode(0));
                triP2 = WhichTriangle(segP.GetNode(1));
                if ((triP1 == null) || (triP1.Status == StatusFlag.NotActive) ||
                     (triP2 == null) || (triP2.Status == StatusFlag.NotActive) ||
                    (CheckFeatureSeg(segP) == -1))
                {
                    breakLinesList.RemoveCurrentItem();
                    segP = (RVSegment)breakLinesList.CurrentItem();
                }
                else
                    segP = NextBreakLinesList;
            }
            segP = FirstTmpBreakLinesList;
            while (segP != null)
            {
                triP1 = WhichTriangle(segP.GetNode(0));
                triP2 = WhichTriangle(segP.GetNode(1));
                if ((triP1 == null) || (triP1.Status == StatusFlag.NotActive) ||
                     (triP2 == null) || (triP2.Status == StatusFlag.NotActive) ||
                     (CheckFeatureSeg(segP) == -1))
                {
                    tmpBreakLinesList.RemoveCurrentItem();
                    segP = (RVSegment)tmpBreakLinesList.CurrentItem();
                }
                else
                    segP = NextTmpBreakLinesList;
            }
            return;
        }
        public RVSegment addTFeatureSeg(RVSegment newfsegp)
        {
            return AddFeatureSeg(newfsegp, tmpBreakLinesList);
        }
        public void removeFSeg(RVSegment fSegP)
        {
            breakLinesList.Remove(fSegP);
        }
        public void removeTFSeg(RVSegment fSegP)
        {
            tmpBreakLinesList.Remove(fSegP);
        }
        public int readBoundSegs(StreamReader file)
        {
            //			RVSegment segP;
            //			RVNode nodeP1;
            //			RVNode nodeP2;
            //			char c = 'a';
            //			int name, n1, n2;

            //			//	while (is)
            //			//	{
            //			//		if (!(is >> name))
            //			//			break;
            //			//		if (!(is >> n1))
            //			//			break;
            //			//		nodeP1 = (RVNode*)nodesList.n(n1);
            //			//		if (!(is >> n2))
            //			//			break;
            //			//		nodeP2 = (RVNode*)nodesList.n(n2);

            //			//		segP = physics.ReadSegment(name, nodeP1, nodeP2,is);
            //			//		if (segP.getn() >= 0)
            //			//			boundarySegmentsList.Add(segP);
            //			//	}
            //			//is.clear();
            //			//	while ((c != '.') && is)
            //			//	{
            //			//	is.get(c);
            //			//		cout << c;
            //			//	}
            return boundarySegmentsList.Count;
        }

        public double MeshQuality(RVTriangle thisOne)
        {
            double minQual = 1.0;
            thisOne = null;
            double elmQual;
            for (int i = 1; i <= triElementsList.Count; i++)
            {
                if (((RVTriangle)triElementsList.GetIndexItem(i)).Status == StatusFlag.Activate)
                {
                    elmQual = ((RVTriangle)triElementsList.GetIndexItem(i)).Quality();
                    if (elmQual < minQual)
                    {
                        minQual = elmQual;
                        thisOne = (RVTriangle)triElementsList.GetIndexItem(i);
                    }
                }
            }
            return minQual;
        }

        public int boundaryRefine(double minQual)
        {
            int[] index = { 0, 0, 0 };
            int count = 0, ind;
            double[] length = new double[3];
            RVTriangle triangle;

            triangle = FirstTriElements;
            while (triangle != null)
            {
                index[0] = index[1] = index[2] = 0;
                if ((triangle.Status == StatusFlag.Activate) && (triangle.Quality() < minQual))
                {
                    if (((triangle.GetOwner(0)).Count()) == 2)
                        index[0] = 1;
                    if (((triangle.GetOwner(1)).Count()) == 2)
                        index[1] = 1;
                    if (((triangle.GetOwner(2)).Count()) == 2)
                        index[2] = 1;
                    if ((index[0] + index[1] + index[2]) == 1)
                    {
                        length[0] = (triangle.GetNode(1)).Distance(triangle.GetNode(2));
                        length[1] = (triangle.GetNode(2)).Distance(triangle.GetNode(0));
                        length[2] = (triangle.GetNode(0)).Distance(triangle.GetNode(1));
                        if ((length[0] > length[1]) && (length[0] > length[2]))
                            ind = 0;
                        else if ((length[1] > length[0]) && (length[1] > length[2]))
                            ind = 1;
                        else
                            ind = 2;
                        if (index[ind] == 1)
                        {
                            bisectSeg((RVSegment)triangle.GetOwner(ind), boundarySegmentsList);
                            count++;
                        }
                    }
                }
                triangle = NextTriElements;
            }
            return count;
        }

        
        /// <summary>
        /// this is for Dump nodal CSV file SK November 2008
        /// это для Dump узлового CSV-файла SK Ноябрь 2008 г.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="index"></param>
        void dumpREADCOFile(StreamWriter file, int index)
        {
            //		RVNode* nodeP;
            //		double prevyval, yval, xval, diff, z0, zmax, xoffset, yoffset;
            //		int numcols, numrows, j5, colcounter, nzp1, iz;
            //		os.precision(6);
            //		os.setf(ios::scientific, ios::floatfield);

            //		//get the number of columns and rows
            //		prevyval = 0;
            //		for (int i = 1; i <= nodesList.Count(); i++)
            //		{
            //			nodeP = (RVNode*)nodesList.i(i);
            //			yval = nodeP.Yo;  //y
            //			diff = fabs(prevyval - yval);
            //			if (prevyval != 0 && diff > 0.1) break;
            //			prevyval = yval;
            //		}
            //		//---------------
            //		numcols = i - 1;
            //		numrows = nodesList.Count() / numcols;
            //		colcounter = 0;
            //		nzp1 = int(sedp.variables[19] + 1);  //sedp.variables[19] = number of cells in z
            //		zmax = sedp.variables[18];
            //		xoffset = sedp.variables[20];
            //		yoffset = sedp.variables[21];
            //		os << numcols << "," << numrows << "," << nzp1 << "\n";
            //		for (iz = 0; iz <= nzp1; iz++)
            //		{
            //			j5 = 0;
            //			colcounter = 0;
            //			for (j5 = 0; j5 < numcols; j5++)
            //			{
            //				for (int i = 1 + j5; i <= numrows * numcols; i += numcols)
            //				{
            //					nodeP = (RVNode*)nodesList.i(i);
            //					os << nodeP.Xo - xoffset << " ";   //x
            //					if (i == numrows * numcols)
            //					{
            //						os << "\n";
            //						colcounter = 0;
            //						break;
            //					}
            //					colcounter++;
            //					if (colcounter == 5)
            //					{
            //						os << "\n";
            //						colcounter = 0;
            //					}
            //				}
            //			}
            //			for (j5 = 0; j5 < numcols; j5++)
            //			{
            //				for (int i = 1 + j5; i <= numrows * numcols; i += numcols)
            //				{
            //					nodeP = (RVNode*)nodesList.i(i);
            //					os << nodeP.Yo - yoffset << " ";   //y
            //					if (i == numrows * numcols)
            //					{   // if this is the end of ny
            //						os << "\n";
            //						colcounter = 0;
            //						break;
            //					}
            //					colcounter++;
            //					if (colcounter == 5)
            //					{
            //						os << "\n";
            //						colcounter = 0;
            //					}
            //				}
            //			}
            //			for (j5 = 0; j5 < numcols; j5++)
            //			{
            //				for (int i = 1 + j5; i <= numrows * numcols; i += numcols)
            //				{
            //					nodeP = (RVNode*)nodesList.i(i);
            //					z0 = nodeP.GetPapam(1);
            //					if (sedp.variables[18] <= 0) zmax = z0 + fabs(sedp.variables[18]); // 
            //					os << iz * (zmax - z0) / nzp1 + z0 << ",";   //z
            //					if (i == numrows * numcols)
            //					{
            //						os << "\n";
            //						colcounter = 0;
            //						break;
            //					}
            //					colcounter++;
            //					if (colcounter == 5)
            //					{
            //						os << "\n";
            //						colcounter = 0;
            //					}
            //				}
            //			}
            //		}
        }

        //	Fills RVMeshIrregular with uniformly spaced, equilaterally arranged Nodes (from physics spec).
        //	Spacing is approximate (goal) Distance between CountNodes.
        //	Theta (in degrees, 0 to 90) is the angle of the pattern to the x direction.
        //	dLimits is a RVBox defining the Area to be filled.
        //	Requires a previously triangulated data RVMeshIrregular, bedMesh,
        //	which masks inactive areas and provides interpolation data.
        //	Resulting set of CountNodes are in tmpNodesList which can be later accepted into nodesList or rejected.
        //	The number of CountNodes created are returned.
        // Заполняет RVMeshIrregular равномерно расположенными, равносторонними узлами (из спецификации физики).
        // Интервал - это приблизительное (целевое) расстояние между CountNodes.
        // Тета (в градусах от 0 до 90) - это угол рисунка к направлению x.
        // dLimits - это поле, определяющее область для заполнения.
        // Требуются предварительно триангулированные данные RVMeshIrregular, bedMesh,
        // который маскирует неактивные области и предоставляет данные интерполяции.
        // Результирующий набор CountNodes находится в tmpNodesList, который позже может быть принят в nodesList или отклонен.
        // Возвращается количество созданных CountNodes.
        public int fillNodesUniform(double spacing, double theta,
            RVBox dLimits, RVMeshIrregular boundTIN, RVMeshIrregular bedMesh, RVMeshIrregular bound2TIN)
        {
            double r, s, x, y, r0;
            int nGoodNodes = 0;
            int nodeNum = GetNextNumber();
            RVTriangle triangle, tBP;
            RVNode node;
            if ((theta < 0.0) || (theta > 90.0))
                return 0;

            double ct = Math.Cos(theta / 45.0 * Math.Atan(1.0));
            double st = Math.Sin(theta / 45.0 * Math.Atan(1.0));
            double rmax = (dLimits.x2 - dLimits.x1) * ct + (dLimits.y2 - dLimits.y1) * st;
            double smax = (dLimits.x2 - dLimits.x1) * st + (dLimits.y2 - dLimits.y1) * ct;
            double x0 = dLimits.x1 + (dLimits.x2 - dLimits.x1) * (1.0 - ct * ct);
            double y0 = dLimits.y1 - (dLimits.x2 - dLimits.x1) * ct * st;
            double dr = spacing;
            double ds = spacing * Math.Sqrt(3.0) / 2.0;
            int nr = (int)(rmax / dr + 1.5);
            dr = rmax / nr;
            int ns = (int)(smax / ds + 1.5);
            ds = smax / ns;

            for (int j = 1; j < ns; j++)
            {
                s = j * ds;
                r0 = ((j % 2) - 1) * dr / 2.0;
                for (int i = 1; i < nr; i++)
                {
                    r = r0 + i * dr;
                    x = x0 + r * ct - s * st + (random.Next(RAND_MAX) % 500) / 100000.0;
                    y = y0 + r * st + s * ct + (random.Next(RAND_MAX) % 500) / 100000.0;
                    node = physics.CreateNewNode(nodeNum, x, y);

                    if ((triangle = bedMesh.WhichTriangle(node)) != null)
                    {
                        if ((tBP = boundTIN.WhichTriangle(node)) != null)
                        {
                            if (tBP.Status == StatusFlag.Activate)
                            {
                                if ((tBP = bound2TIN.WhichTriangle(node)) != null)
                                {
                                    if (tBP.Status == StatusFlag.Activate)
                                    {
                                        triangle.LocateNodeAndInterpolation(x, y, node);
                                        nGoodNodes += 1;
                                        nodeNum += 1;
                                        tmpNodesList.Add(node);
                                    }
                                    else
                                        node = null;
                                }
                                else
                                    node = null;
                            }
                            else
                                node = null;
                        }
                        else
                            node = null;
                    }
                    else
                        node = null;
                }
            }
            return nGoodNodes;
        }


        //	Fills RVMeshIrregular with equilaterally arranged Nodes (from physics spec) in an expanding circular pattern.
        //	The spacing is set analagous to to a point source potential flow net.
        //	Spacing is approximate (goal) Distance between CountNodes.
        //	Theta1 (in degrees, 0 to 360) is the start angle of the pattern from the x direction.
        //	Theta2 (in degrees, 0 to 360) is the finish angle of the pattern from the x direction.
        //	Requires a previously triangulated data RVMeshIrregular, bedMesh,
        //	which masks inactive areas and provides interpolation data.
        //	Resulting set of CountNodes are in tmpNodesList which can be later accepted into nodesList or rejected.
        //	The number of CountNodes created are returned.
        public int fillNodesSource(double x0, double y0, double r0, int nRays,
                                         double theta0, RVMeshIrregular boundTIN, RVMeshIrregular bedMesh)
        {
            double r, rmax, theta, deltheta, fac, x, y;
            int j;
            int nGoodNodes = 0;
            int nodeNum = GetNextNumber();
            RVBox dLimits;
            RVTriangle triangle, tBP;
            RVNode node;


            deltheta = 8.0 * Math.Atan(1.0) / nRays;
            theta0 *= 8.0 * Math.Atan(1.0) / 360.0;
            //	fac = (nRays + sqrt(3.0)/2.)/(nRays - sqrt(3.0)/2.);

            fac = Math.Exp(deltheta / Math.Sqrt(2.0));

            dLimits = bedMesh.GetLimits();
            if (Math.Abs(x0 - dLimits.x1) > Math.Abs(x0 - dLimits.x2))
                x = Math.Abs(x0 - dLimits.x1);
            else
                x = Math.Abs(x0 - dLimits.x2);
            if (Math.Abs(y0 - dLimits.y1) > Math.Abs(y0 - dLimits.y2))
                y = Math.Abs(y0 - dLimits.y1);
            else
                y = Math.Abs(y0 - dLimits.y2);
            rmax = Math.Sqrt(x * x + y * y);
            r = r0;
            j = 0;

            while (r < rmax)
            {
                for (int i = 0; i < nRays; i++)
                {
                    theta = theta0 + (i + (j % 2) * 0.5) * deltheta + 0.001;
                    x = x0 + r * Math.Cos(theta);
                    y = y0 + r * Math.Sin(theta);
                    node = physics.CreateNewNode(nodeNum, x, y);

                    if ((triangle = bedMesh.WhichTriangle(node)) != null)
                    {
                        if ((tBP = boundTIN.WhichTriangle(node)) != null)
                        {
                            if (tBP.Status == StatusFlag.Activate)
                            {
                                triangle.LocateNodeAndInterpolation(x, y, node);
                                nGoodNodes += 1;
                                nodeNum += 1;
                                tmpNodesList.Add(node);
                            }
                            else
                                node = null;
                        }
                        else
                            node = null;
                    }
                    else
                        node = null;
                }
                r *= fac;
                j++;
            }
            return nGoodNodes;
        }

        //	Refines a RVMeshIrregular by placing a new node in the centre of each existing triangle
        //	within the boundTIN region.
        //	Requires a previously triangulated data RVMeshIrregular, bedMesh,
        //	which masks inactive areas and provides interpolation data.
        //	Resulting set of CountNodes are in tmpNodesList which can be later accepted into nodesList or rejected.
        //	The number of CountNodes created are returned.
        public int refineRegion(RVMeshIrregular boundTIN, RVMeshIrregular bedMesh)
        {
            int nGoodNodes = 0;
            int nodeNum = GetNextNumber();
            RVTriangle triangle, tBP, eTP;
            RVNode cNode = new RVNode(), node;

            eTP = FirstTriElements;
            while (eTP != null)
            {
                if (eTP.Status == StatusFlag.Activate)
                {
                    eTP.LocateNodeAtCenter(cNode);
                    node = physics.CreateNewNode(nodeNum, cNode.X, cNode.Y);
                    if ((triangle = bedMesh.WhichTriangle(node)) != null)
                    {
                        if ((tBP = boundTIN.WhichTriangle(node)) != null)
                        {
                            if (tBP.Status == StatusFlag.Activate)
                            {
                                triangle.LocateNodeAndInterpolation(node);
                                nGoodNodes += 1;
                                nodeNum += 1;
                                tmpNodesList.Add(node);
                            }
                            else
                                node = null;
                        }
                        else
                            node = null;
                    }
                    else
                        node = null;
                }
                eTP = NextTriElements;
            }
            return nGoodNodes;
        }

        /// <summary>
        /// New function added 3/2001, JDS
        ///	Same as refineRegion but refines only large elevation difference triangles.
        ///
        ///	Refines a RVMeshIrregular by placing a new node in the centre of each existing triangle
        ///	within the boundTIN region.
        ///	Requires a previously triangulated data RVMeshIrregular, bedMesh,
        ///	which masks inactive areas and provides interpolation data.
        ///	Resulting set of CountNodes are in tmpNodesList which can be later accepted into nodesList or rejected.
        ///	The number of CountNodes created are returned.
        ///	Добавлена новая функция 3/2001, JDS
        /// То же, что и RefineRegion, но уточняет только большие треугольники перепада высот.
        /// Уточняет RVMeshIrregular, помещая новый узел в центр каждого существующего треугольника
        /// в пределах области boundTIN.
        /// Требуются предварительно триангулированные данные RVMeshIrregular, bedMesh,
        /// который маскирует неактивные области и предоставляет данные интерполяции.
        /// Результирующий набор CountNodes находится в tmpNodesList, который позже может быть принят в nodeList или отклонен.
        /// Возвращается количество созданных CountNodes.
        /// </summary>
        /// <param name="boundTIN"></param>
        /// <param name="bedMesh"></param>
        /// <param name="minDz"></param>
        /// <returns></returns>
        public int refineRegionYellow(RVMeshIrregular boundTIN, RVMeshIrregular bedMesh, double minDz)
        {
            int nGoodNodes = 0;
            int nodeNum = GetNextNumber();
            RVTriangle triangle, tBP, eTP;
            RVNode cNode = new RVNode(), node;

            eTP = FirstTriElements;
            while (eTP != null)
            {
                if ((eTP.Status == StatusFlag.Activate) && Math.Abs(bedMesh.TriCenterDz(eTP)) > minDz)
                {
                    eTP.LocateNodeAtCenter(cNode);
                    node = physics.CreateNewNode(nodeNum, cNode.X, cNode.Y);
                    if ((triangle = bedMesh.WhichTriangle(node)) != null)
                    {
                        if ((tBP = boundTIN.WhichTriangle(node)) != null)
                        {
                            if (tBP.Status == StatusFlag.Activate)
                            {
                                triangle.LocateNodeAndInterpolation(node);
                                nGoodNodes += 1;
                                nodeNum += 1;
                                tmpNodesList.Add(node);
                            }
                            else
                                node = null;
                        }
                        else
                            node = null;
                    }
                    else
                        node = null;
                }
                eTP = NextTriElements;
            }
            return nGoodNodes;
        }

        //	Generates new boundary CountNodes and segments from physics spec.
        //	Outline is provided by boundTIN.
        //	Spacing is approximate (goal) Distance between new CountNodes.
        //	Resulting set of CountNodes are in tmpNodesList which can be later accepted into nodesList or rejected.
        //	Resulting boundary segments are in tmpBoundarySegmentsList. Accepting segements automatically accepts CountNodes.
        //	The number of segments (= total number of CountNodes on boundary) created are returned.
        
        // Создает новые граничные CountNodes и сегменты из физических спецификаций.
        // Контур предоставляется boundTIN.
        // Интервал - это приблизительное (целевое) расстояние между новыми CountNodes.
        // Результирующий набор CountNodes находится в tmpNodesList, который позже может быть принят в список узлов или отклонен.
        // Результирующие граничные сегменты находятся в tmpBoundarySegmentsList. Принятие сегментов автоматически принимает CountNodes.
        // Количество созданных сегментов (= общее количество CountNodes на границе) возвращается.
        public int defineBoundary(double spacing, RVMeshIrregular boundTIN, RVMeshIrregular bedMesh)
        {
            int i, nNewSegs;
            RVSegment bSegment = (RVSegment)boundTIN.FirstBoundarySegment;
            RVNode startLoopP;
            RVSegment nSegP = null;
            RVNode nodeP1 = null;
            RVNode nodeP2 = null;
            RVNode startNodeP = null;
            RVTriangle triangle;

            while (bSegment != null)
            {
                startLoopP = bSegment.GetNode(0);
                nodeP1 = physics.CreateNewNode(GetNextNumber());
                startNodeP = nodeP1;
                nodeP1.Fixed = RVFixedNodeFlag.fixedNode;
                tmpNodesList.Add(nodeP1);
                bSegment.LocateNodeAndInterpolation(0.0, nodeP1);
                triangle = bedMesh.WhichTriangle(nodeP1);
                if (triangle != null)
                    triangle.LocateNodeAndInterpolation(nodeP1);
                while (bSegment != null)
                {
                    nNewSegs = 1 + (int)(bSegment.length() / spacing);
                    for (i = 1; i <= nNewSegs; i++)
                    {
                        nodeP2 = physics.CreateNewNode(GetNextNumber());
                        nodeP2.Fixed = RVFixedNodeFlag.slidingNode;
                        tmpNodesList.Add(nodeP2);
                        bSegment.LocateNodeAndInterpolation((1.0 * i / nNewSegs), nodeP2);
                        triangle = bedMesh.WhichTriangle(nodeP2);
                        if (triangle != null)
                            triangle.LocateNodeAndInterpolation(nodeP2);
                        nSegP = physics.CreateNewSegment(tmpBoundarySegmentsList.Count + 1, nodeP1, nodeP2, bSegment);
                        tmpBoundarySegmentsList.Add(nSegP);
                        nodeP1 = nodeP2;
                    }
                    nodeP1.Fixed = RVFixedNodeFlag.fixedNode;
                    if (bSegment.GetNode(1) == startLoopP)
                    {
                        bSegment = (RVSegment)boundTIN.NextBoundarySegment;
                        break;
                    }
                    bSegment = (RVSegment)boundTIN.NextBoundarySegment;
                }
                nSegP.SetNode(1, startNodeP);
                tmpNodesList.RemoveCurrentItem();
                nodeP2 = null;
            }
            return tmpBoundarySegmentsList.Count;
        }

        //	Generates new boundary CountNodes and segments from physics spec.
        //	Outline is provided by boundTIN, starting with firstSP, which is presumably a new loop.
        //	Spacing is approximate (goal) Distance between new CountNodes.
        //	Resulting set of CountNodes are in tmpNodesList which can be later accepted into nodesList or rejected.
        //	Resulting boundary segments are in tmpBoundarySegmentsList. Accepting segements automatically accepts CountNodes.
        //	The number of segments (= total number of CountNodes on boundary) created are returned.
        // Создает новые граничные CountNodes и сегменты из физических спецификаций.
        // Схема предоставляется boundTIN, начиная с firstSP, что предположительно является новым циклом.
        // Интервал - это приблизительное (целевое) расстояние между новыми CountNodes.
        // Результирующий набор CountNodes находится в tmpNodesList, который позже может быть принят в nodesList или отклонен.
        // Результирующие граничные сегменты находятся в tmpBoundarySegmentsList. Принятие сегментов автоматически принимает CountNodes.
        // Количество созданных сегментов (= общее количество CountNodes на границе) возвращается.
        public int defineNewBoundLoop(double spacing, RVMeshIrregular boundTIN, RVMeshIrregular bedMesh, RVSegment firstSP)
        {
            int i, nNewSegs;
            RVSegment bSegment = firstSP;
            RVNode startLoopP;
            RVSegment nSegP = null;
            RVNode nodeP1 = null;
            RVNode nodeP2 = null;
            RVNode startNodeP = null;
            RVTriangle triangle;

            while (bSegment != null)
            {
                startLoopP = bSegment.GetNode(0);
                nodeP1 = physics.CreateNewNode(GetNextNumber());
                startNodeP = nodeP1;
                nodeP1.Fixed = RVFixedNodeFlag.fixedNode;
                tmpNodesList.Add(nodeP1);
                bSegment.LocateNodeAndInterpolation(0.0, nodeP1);
                triangle = bedMesh.WhichTriangle(nodeP1);
                if (triangle != null)
                    triangle.LocateNodeAndInterpolation(nodeP1);
                while (bSegment != null)
                {
                    nNewSegs = 1 + (int)(bSegment.length() / spacing);
                    for (i = 1; i <= nNewSegs; i++)
                    {
                        nodeP2 = physics.CreateNewNode(GetNextNumber());
                        nodeP2.Fixed = RVFixedNodeFlag.slidingNode;
                        tmpNodesList.Add(nodeP2);
                        bSegment.LocateNodeAndInterpolation((1.0 * i / nNewSegs), nodeP2);
                        triangle = bedMesh.WhichTriangle(nodeP2);
                        if (triangle != null)
                            triangle.LocateNodeAndInterpolation(nodeP2);
                        nSegP = physics.CreateNewSegment(tmpBoundarySegmentsList.Count + 1, nodeP1, nodeP2, bSegment);
                        tmpBoundarySegmentsList.Add(nSegP);
                        nodeP1 = nodeP2;
                    }
                    nodeP1.Fixed = RVFixedNodeFlag.fixedNode;
                    if (bSegment.GetNode(1) == startLoopP)
                    {
                        bSegment = (RVSegment)bSegment.Next;
                        break;
                    }
                    bSegment = (RVSegment)bSegment.Next;
                }
                nSegP.SetNode(1, startNodeP);
                tmpNodesList.RemoveCurrentItem();
                nodeP2 = null;
            }

            return tmpBoundarySegmentsList.Count;
        }

        public int orderNodesRCM()
        {
            int ii, i, j, index = 0, count = 0;
            RVElement aElP;
            RVTriangle element, triangle;
            RVNode nP1, mnP, tnP, node;

            for (ii = 1; ii <= triElementsList.Count; ii++)
            {           //	Renumber triangles
                triangle = (RVTriangle)triElementsList.GetIndexItem(ii);
                triangle.ID = triangle.Index  + 1;
            }
            //	Set RVNode indices to zero
            node = firstNode;
            while (node != null)
            {
                node.Index = 0;
                node.ID = 0;
                node = NextNode;
            }
            //	Set index for each node to number of connecting elements
            triangle = FirstTriElements;
            while (triangle != null)
            {
                if (triangle.Status == StatusFlag.Activate)
                {
                    for (i = 0; i < 3; i++)
                    {
                        node = triangle.GetNode(i);
                        node.Index = node.Index - 1;
                    }
                }
                triangle = NextTriElements;
            }
            //	Initialize process
            RVList tList = new RVList();
            RVList aList = new RVList();

            node = firstNode;
            if (node != null)
            {
                nodesList.Remove(node);
                tList.Add(node);
                node.ID = 1;
                count++;
            }

            //	Main ordering loop, nodesListist to tempList

            while (node != null)
            {

                //	Get List of adjacent CountNodes

                if ((triangle = WhichTriHasNode(node)) != null)
                {
                    element = triangle;
                    do
                    {
                        for (j = 0; j < 3; j++)
                        {
                            nP1 = element.GetNode(j);
                            if (nP1 == node)
                                index = j;
                        }
                        index += 1;
                        if (index == 3)
                            index = 0;
                        aElP = element.GetOwner(index);
                        if (aElP == null)
                            break;
                        if (aElP.Count() == 2)
                            break;
                        element = (RVTriangle)aElP;
                    } while (element != triangle);
                    triangle = element;
                    do
                    {
                        for (j = 0; j < 3; j++)
                        {
                            nP1 = element.GetNode(j);
                            if (nP1 == node)
                                index = j;
                            else
                            {
                                if (nP1.ID == 0)
                                {
                                    nodesList.Remove(nP1);
                                    aList.Add(nP1);
                                    nP1.ID= 1;
                                }
                            }
                        }
                        index -= 1;
                        if (index == -1)
                            index = 2;
                        aElP = element.GetOwner(index);
                        if (aElP == null)
                            break;
                        if (aElP.Count() == 2)
                            break;
                        element = (RVTriangle)aElP;
                    } while (element != triangle);
                }

                // Put in tempList in order of increasing degree

                while (aList.Count > 0)
                {
                    tnP = (RVNode)aList.FirstItem();
                    mnP = tnP;
                    while (tnP != null)
                    {
                        if (tnP.Index > mnP.Index)
                            mnP = tnP;
                        tnP = (RVNode)aList.NextItem();
                    }
                    aList.Remove(mnP);
                    tList.Add(mnP);
                    count++;
                }

                node = (RVNode)node.Next;
            }

            //	transfer back to nodesListist in reverse order

            while (tList.Count > 0)
            {
                nodesList.Push(tList.Pop());
            }

            node = (RVNode)nodesList.FirstItem();
            int nn = 1;
            while (node != null)
            {
                node.ID = nn;
                nn++;
                node = (RVNode)node.Next;
            }

            return count;

        }

        public int doFeatures(RVMeshIrregular dataTINP)
        {
            int startNum, nTimes = 0, errcode = 1;
            RVSegment sP, lastSP = null;
            RVSegment fP;

            //	checkAllFeatureSegs();            //		check feature segs for consistentency

            do
            {
                startNum = nodesList.Count;            // Thread feature lines through
                sP = FirstBreakLine;                       //	the mesh, inserting additional
                fP = sP;
                lastSP = sP;
                while (sP != null)
                {
                    sP = ThreadFeature(sP, breakLinesList);
                    if (sP == lastSP)
                        nTimes += 1;
                    else
                        nTimes = 0;
                    if (nTimes > 4)
                    {
                        errcode = -sP.ID;
                        sP = (RVSegment)breakLinesList.NextItem();
                        break;
                    }
                    if (sP == fP)
                        break;
                    lastSP = sP;
                }
                if (errcode < 0)
                    break;
            } while (nodesList.Count != startNum);

            RVTriangle triangle;
            int i;
            for (i = 1; i <= triElementsList.Count; i++)
            {           //	Renumber triangles
                triangle = (RVTriangle)triElementsList.GetIndexItem(i);
                triangle.ID = triangle.Index + 1;
            }

            /*	RVNode* node;
				for(i =1;i<= nodesList.Count();i++){			//	Renumber CountNodes
					node = (RVNode *) nodesList.i(i);
					node.setn((node.getIndex())+1);
				}
			*/
            return errcode;
        }
               

        public int updateMesh(RVMeshIrregular bedMesh)
        {
            int err = 0;

            RVTriangle dtP;
            RVNode node = firstNode;
            while (node != null)
            {
                if ((dtP = bedMesh.WhichTriangle(node)) != null)
                    dtP.LocateNodeAndInterpolation(node.X, node.Y, node);
                else
                {
                    err = -1;
                    break;
                }
                node = NextNode;
            }
            return err;
        }


        public double getMinPar(int nPar)
        {
            RVNode node;
            double min;
            node = (RVNode)nodesList.FirstItem();
            if (node != null)
                min = node.GetPapam(nPar);
            else
                return 0.0;
            while (node != null)
            {
                if (node.GetPapam(nPar) < min)
                    min = node.GetPapam(nPar);
                node = (RVNode)nodesList.NextItem();
            }
            return min;
        }

        public double getMaxPar(int nPar)
        {
            RVNode node;
            double max;
            node = (RVNode)nodesList.FirstItem();
            if (node != null)
                max = node.GetPapam(nPar);
            else
                return 0.0;
            while (node != null)
            {
                if (node.GetPapam(nPar) > max)
                    max = node.GetPapam(nPar);
                node = (RVNode)nodesList.NextItem();
            }
            return max;
        }


        public void scratchTriangle()
        {
            RVTriangle triangle;
            RVElement pP;
            int ind;

            triangle = (RVTriangle)triElementsList.CurrentItem();
            for (int i = 0; i < 3; i++)
            {
                pP = triangle.GetOwner(i);
                if (pP != null)
                {
                    ind = pP.reflectAdj(triangle);
                    if (ind >= 0)
                        pP.SetOwner(ind, null);
                }
            }
            triElementsList.RemoveCurrentItem();
            triangle = null;
        }

        //	New function added 4/2001, JDS
        //	Same as bisectSeg but specific to boundary segments.  This function
        //	handles the operation when the user chooses to manually bisect
        //	a boundary segment.
        public RVSegment bisectBSeg(RVSegment segP, double Distance = 0.5)
        {
            RVNode nodeP;
            RVSegment nSegP;
            if (Distance > 0.99) Distance = 0.99;
            if (Distance < 0.01) Distance = 0.01;
            nodeP = physics.CreateNewNode(GetNextNumber());
            nodeP.BoundNodeFlag = BoundaryNodeFlag.boundaryNode;
            nodeP.Fixed = RVFixedNodeFlag.fixedNode;
            nodesList.Add(nodeP);
            segP.LocateNodeAndInterpolation(Distance, nodeP);
            nSegP = physics.CreateNewSegment(boundarySegmentsList.Count + 1,
                    nodeP, segP.GetNode(1), segP);
            segP.SetNode(1, nodeP);
            boundarySegmentsList.SetCurrentItem(segP);
            boundarySegmentsList.Insert(nSegP);
            RVSegment sp = (RVSegment)boundarySegmentsList.FirstItem();
            if (sp == segP)
                return sp;
            while (sp.Next != segP)
                sp = (RVSegment)boundarySegmentsList.NextItem();
            return sp;
        }

        public void acceptNodes()
        {
            nodesList.AddRange(tmpNodesList);
            tmpNodesList.Clear();
            RVNode node = firstNode;
            int i = 1;
            while (node != null)
            {
                node.ID = i++;
                node = NextNode;
            }
        }

        public void clearBsegs()
        {
            RVSegment sP = (RVSegment)boundarySegmentsList.FirstItem();
            while (sP != null)
            {
                boundarySegmentsList.RemoveCurrentItem();
                sP = (RVSegment)boundarySegmentsList.FirstItem();
            }
        }

        public void rejectNodes()
        {
            RVNode node = (RVNode)tmpNodesList.FirstItem();
            while (node != null)
            {
                tmpNodesList.RemoveCurrentItem();
                node = (RVNode)tmpNodesList.FirstItem();
            }
        }

        public void acceptBsegs()
        {
            RVSegment sP = (RVSegment)boundarySegmentsList.FirstItem();
            while (sP != null)
            {
                boundarySegmentsList.RemoveCurrentItem();
                sP = (RVSegment)boundarySegmentsList.FirstItem();
            }
            boundarySegmentsList.AddRange(tmpBoundarySegmentsList);
            tmpBoundarySegmentsList.Clear();
        }

        public void acceptNewBsegs()
        {
            boundarySegmentsList.AddRange(tmpBoundarySegmentsList);
            tmpBoundarySegmentsList.Clear();
        }

        public void rejectBsegs()
        {
            RVNode node = (RVNode)tmpNodesList.FirstItem();
            while (node != null)
            {
                tmpNodesList.RemoveCurrentItem();
                node = (RVNode)tmpNodesList.FirstItem();
            }
            RVSegment sP = (RVSegment)tmpBoundarySegmentsList.FirstItem();
            while (sP != null)
            {
                tmpBoundarySegmentsList.RemoveCurrentItem();
                sP = (RVSegment)tmpBoundarySegmentsList.FirstItem();
            }
        }

        public void acceptFsegs()
        {
            breakLinesList.AddRange(tmpBreakLinesList);
            tmpBreakLinesList.Clear();
        }

        public void rejectFsegs()
        {
            RVSegment sP = (RVSegment)tmpBreakLinesList.FirstItem();
            while (sP != null)
            {
                tmpBreakLinesList.RemoveCurrentItem();
                sP = (RVSegment)tmpBreakLinesList.FirstItem();
            }
        }

        //	Puts one RVNode into the RVMeshIrregular (from physics spec).
        //  Then joins that node to the current node to form a new boundary segment.
        //	Requires a previously triangulated data RVMeshIrregular, bedMesh,
        //	which masks inactive areas and provides interpolation data.
        //	Resulting node is in tmpNodesList which can be later accepted into nodesList or rejected.
        //	The segment created is returned.
        //  If something went wrong, null is returned.
        // Помещает один узел в RVMeshIrregular (из спецификации физики).
        // Затем присоединяет этот узел к текущему узлу, чтобы сформировать новый сегмент границы.
        // Требуются предварительно триангулированные данные RVMeshIrregular, bedMesh,
        // который маскирует неактивные области и предоставляет данные интерполяции.
        // Результирующий узел находится в tmpNodesList, который позже может быть принят в nodesList или отклонен.
        // Созданный сегмент возвращается.
        // Если что-то пошло не так, возвращается null.
        public RVSegment defNewBound(double x, double y, RVMeshIrregular bedMesh)
        {
            RVSegment nSegP = null;
            RVNode pNP = (RVNode)tmpNodesList.LastItem();
            RVNode newNP = AddOneNode(x, y, bedMesh, bedMesh);
            if (newNP != null)
            {
                nSegP = physics.CreateNewSegment(boundarySegmentsList.Count + 1, pNP, newNP, null);
                if (CheckFeatureSeg(nSegP) == 1)
                    boundarySegmentsList.Add(nSegP);
                else
                {
                    nSegP = null;
                    tmpNodesList.RemoveCurrentItem();
                    newNP = null;
                    tmpNodesList.SetCurrentItem(pNP);
                    nSegP = null;
                }
            }
            return nSegP;
        }

        public RVSegment defNewBound(double x, double y)
        {
            RVSegment nSegP = null;
            RVNode pNP = (RVNode)tmpNodesList.LastItem();
            RVNode newNP = AddOneNode(x, y);
            if (newNP != null)
            {
                nSegP = physics.CreateNewSegment(boundarySegmentsList.Count + 1, pNP, newNP, null);
                if (CheckFeatureSeg(nSegP) == 1)
                    boundarySegmentsList.Add(nSegP);
                else
                {
                    nSegP = null;
                    tmpNodesList.RemoveCurrentItem();
                    newNP = null;
                    tmpNodesList.SetCurrentItem(pNP);
                    nSegP = null;
                }
            }
            return nSegP;
        }

        // Joins the last node to the first node to close the boundary loop.
        //	Requires a previously triangulated data RVMeshIrregular, bedMesh,
        //	which masks inactive areas and provides interpolation data.
        //	The segment created is returned.
        // If something went wrong, null is returned.
        public RVSegment closeBound(RVMeshIrregular bedMesh)
        {
            RVSegment nSegP = null;
            RVNode pNP = (RVNode)tmpNodesList.LastItem();
            RVNode newNP = FirstBoundarySegment.GetNode(0);
            if (newNP != null)
            {
                nSegP = physics.CreateNewSegment(boundarySegmentsList.Count + 1, pNP, newNP, null);
                if (CheckFeatureSeg(nSegP) == 1)
                    boundarySegmentsList.Add(nSegP);
                else
                    nSegP = null;
            }
            return nSegP;
        }

        public RVSegment closeBound()
        {
            RVSegment nSegP = null;
            RVNode pNP = (RVNode)tmpNodesList.LastItem();
            RVNode newNP = FirstBoundarySegment.GetNode(0);
            if (newNP != null)
            {
                nSegP = physics.CreateNewSegment(boundarySegmentsList.Count + 1, pNP, newNP, null);
                if (CheckFeatureSeg(nSegP) == 1)
                    boundarySegmentsList.Add(nSegP);
                else
                    nSegP = null;
            }
            return nSegP;
        }

        // Joins the last node to the first node to close the boundary loop.
        //	Requires a previously triangulated data RVMeshIrregular, bedMesh,
        //	which masks inactive areas and provides interpolation data.
        //	The segment created is returned.
        // If something went wrong, null is returned.
        // Присоединяет последний узел к первому, чтобы замкнуть граничный цикл.
        // Требуются предварительно триангулированные данные RVMeshIrregular, bedMesh,
        // который маскирует неактивные области и предоставляет данные интерполяции.
        // Созданный сегмент возвращается.
        // Если что-то пошло не так, возвращается null.
        public RVSegment closeBoundLoop(RVNode firstNP, RVMeshIrregular bedMesh)
        {
            RVSegment nSegP = null;
            RVNode pNP = (RVNode)tmpNodesList.LastItem();
            if (firstNP != null)
            {
                nSegP = physics.CreateNewSegment(boundarySegmentsList.Count + 1, pNP, firstNP, null);
                boundarySegmentsList.Add(nSegP);
            }
            return nSegP;
        }


        #endregion

    }
}