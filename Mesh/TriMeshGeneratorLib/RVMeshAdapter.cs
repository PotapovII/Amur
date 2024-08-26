namespace TriMeshGeneratorLib
{
    using System;
    using System.Collections.Generic;
    using CommonLib;
    using CommonLib.Mesh;
    using CommonLib.Mesh.RVData;
    using MeshLib;

    public class RVMeshAdapter
    {
        public static IMesh getMesh(RVMeshIrregular theMesh)
        {
            TriMesh mesh = new TriMesh();
            RVTriangle currentFElement = theMesh.FirstTriElements;
            int eCount = 0;
            while (currentFElement != null)
            {
                if (currentFElement.Status == StatusFlag.Activate)
                    eCount++;
                currentFElement = theMesh.NextTriElements;
            }
            mesh.AreaElems = new TriElement[eCount];
            int elem = 0;
            currentFElement = theMesh.FirstTriElements;
            while (currentFElement != null)
            {
                if (currentFElement.Status == StatusFlag.Activate)
                {
                    uint n0 = (uint)currentFElement.GetNode(0).ID;
                    uint n1 = (uint)currentFElement.GetNode(1).ID;
                    uint n2 = (uint)currentFElement.GetNode(2).ID;
                    mesh.AreaElems[elem] = new TriElement(n0, n1, n2);
                    elem++;
                }
                currentFElement = theMesh.NextTriElements;
            }
            elem = 0;
            mesh.BoundElems = new TwoElement[theMesh.CountBoundarySegments];
            mesh.BoundElementsMark = new int[theMesh.CountBoundarySegments];
            RVBoundary currentBoundFElement = (RVBoundary)theMesh.FirstBoundarySegment;
            while (currentBoundFElement != null)
            {
                uint n0 = (uint)currentBoundFElement.GetNode(0).ID;
                uint n1 = (uint)currentBoundFElement.GetNode(1).ID;
                mesh.BoundElementsMark[elem] = currentBoundFElement.ID;
                mesh.BoundElems[elem++] = new TwoElement(n0, n1);
                currentBoundFElement = (RVBoundary)theMesh.NextBoundarySegment;
            }
            int nod = 0;
            int idx = 0;
            List<int> BoundKnots = new List<int>();
            List<int> BoundKnotsMark = new List<int>();
            mesh.CoordsX = new double[theMesh.CountNodes];
            mesh.CoordsY = new double[theMesh.CountNodes];
            RVNodeShallowWater elemNodes = (RVNodeShallowWater)theMesh.firstNode;
            while (elemNodes != null)
            {
                mesh.CoordsX[nod] = elemNodes.Xo;
                mesh.CoordsY[nod] = elemNodes.Yo;
                nod++;
                // граничные узлы
                if (elemNodes.BoundNodeFlag == BoundaryNodeFlag.boundaryNode)
                {
                    BoundKnots.Add(elemNodes.ID);
                    BoundKnotsMark.Add(1);
                    idx++;
                }
                elemNodes = (RVNodeShallowWater)theMesh.NextNode;
            }
            mesh.BoundKnots = BoundKnots.ToArray();
            mesh.BoundKnotsMark = BoundKnotsMark.ToArray();
            return mesh;
        }
        /// <summary>
        /// Фронтальный перенумератор сетки
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static IMesh MeshFrontRenumberation(RVMeshIrregular theMesh, ref double[][] values, Direction direction = Direction.toRight)
        {
            TriMesh mesh = new TriMesh();
            int ix, iy, jy;
            //// узлы КЭ сетки
            //ICollection<Vertex> Vertices = mesh.Vertices;
            //// FE
            //ICollection<RVTriangle> Triangles = mesh.Triangles;
            //// BFE
            //IEnumerable<Edge> Edges = mesh.Edges;
            
            RVTriangle currentFElement = theMesh.FirstTriElements;
            // количество элементов
            int CountElems = 0;
            while (currentFElement != null)
            {
                if (currentFElement.Status == StatusFlag.Activate)
                    CountElems++;
                currentFElement = theMesh.NextTriElements;
            }
            mesh.AreaElems = new TriElement[CountElems];


            double MaxX = double.MinValue;
            double MinX = double.MaxValue;
            double MaxY = double.MinValue;
            double MinY = double.MaxValue;
            int Count = 0;
            int CountBoundKnots = 0;
            int MaxID = 0;
            RVNodeShallowWater elemNodes = (RVNodeShallowWater)theMesh.firstNode;
            while (elemNodes != null)
            {
                double X = elemNodes.Xo;
                double Y = elemNodes.Yo;
                if (X > MaxX) MaxX = X;
                if (X < MinX) MinX = X;
                if (Y > MaxY) MaxY = Y;
                if (Y < MinY) MinY = Y;
                if (MaxID < elemNodes.ID) MaxID = elemNodes.ID;
                Count++;
                // граничные узлы
                if (elemNodes.BoundNodeFlag == BoundaryNodeFlag.boundaryNode)
                    CountBoundKnots++;
                elemNodes = (RVNodeShallowWater)theMesh.NextNode;
            }

            int CountKnots = (int)(Math.Sqrt(Count)) + 2;
            List<int>[,] XMap = new List<int>[CountKnots, CountKnots];
            for (int i = 0; i < CountKnots; i++)
                for (int j = 0; j < CountKnots; j++)
                    XMap[i, j] = new List<int>();

            mesh.BoundKnots = new int[CountBoundKnots];
            mesh.BoundKnotsMark = new int[CountBoundKnots];
            // шаги хеширования
            double dx = (MaxX - MinX) / (CountKnots - 1);
            double dy = (MaxY - MinY) / (CountKnots - 1);
            // хеширование узлов
            elemNodes = (RVNodeShallowWater)theMesh.firstNode;
            while (elemNodes != null)
            {
                double X = elemNodes.Xo;
                double Y = elemNodes.Yo;
                ix = (int)((X - MinX) / dx);
                iy = (int)((Y - MinY) / dy);
                XMap[ix, iy].Add(elemNodes.ID);
                elemNodes = (RVNodeShallowWater)theMesh.NextNode;
            }
            // Новые нумера узлов
            int[] NewNumber = new int[MaxID + 1];
            for (uint i = 0; i < CountKnots; i++)
                NewNumber[i] = -1;
            int NewIndex = 0;
            switch (direction)
            {
                case Direction.toRight:
                    {
                        // Получение новый номеров узлов
                        for (ix = 0; ix < CountKnots; ix++) // по Х
                        {
                            for (iy = 0; iy < CountKnots; iy++) // по Y
                            {
                                int CountX = XMap[ix, iy].Count;
                                for (int i = 0; i < CountX; i++) // по узлам в хеше
                                {
                                    int Old = XMap[ix, iy][i];
                                    NewNumber[Old] = NewIndex;
                                    NewIndex++;
                                }
                            }
                        }
                        break;
                    }
                case Direction.toLeft:
                    {
                        // Получение новый номеров узлов
                        int iix;
                        for (ix = 0; ix < CountKnots; ix++) // по Х
                        {
                            iix = CountKnots - ix - 1;
                            for (iy = 0; iy < CountKnots; iy++) // по Y
                            {
                                int CountX = XMap[iix, iy].Count;
                                for (int i = 0; i < CountX; i++) // по узлам в хеше
                                {
                                    int Old = XMap[iix, iy][i];
                                    NewNumber[Old] = NewIndex;
                                    NewIndex++;
                                }
                            }
                        }
                        break;
                    }
                case Direction.toDown:
                    {
                        for (iy = 0; iy < CountKnots; iy++) // по Y
                        {
                            for (ix = 0; ix < CountKnots; ix++) // по Х
                            {
                                int CountX = XMap[ix, iy].Count;
                                for (int i = 0; i < CountX; i++) // по Y
                                {
                                    int Old = XMap[ix, iy][i];
                                    NewNumber[Old] = NewIndex;
                                    NewIndex++;
                                }
                            }
                        }
                        break;
                    }
                case Direction.toUp:
                    {
                        for (iy = 0; iy < CountKnots; iy++) // по Y
                        {
                            jy = CountKnots - iy - 1;
                            for (ix = 0; ix < CountKnots; ix++) // по Х
                            {
                                int CountX = XMap[ix, jy].Count;
                                for (int i = 0; i < CountX; i++) // по Y
                                {
                                    int Old = XMap[ix, jy][i];
                                    NewNumber[Old] = NewIndex;
                                    NewIndex++;
                                }
                            }
                        }
                        break;
                    }
            }
            // **************** Создание нового массива координат ******************
            mesh.CoordsX = new double[Count];
            mesh.CoordsY = new double[Count];
            MemLogLib.MEM.Alloc(4, Count, ref values);
            int idx = 0;
            elemNodes = (RVNodeShallowWater)theMesh.firstNode;
            while (elemNodes != null)
            {
                int OldKnot = elemNodes.ID;
                int NewKnot = NewNumber[OldKnot];
                // координаты
                mesh.CoordsX[NewKnot] = elemNodes.Xo; 
                mesh.CoordsY[NewKnot] = elemNodes.Yo;
                values[0][NewKnot] = elemNodes.Z;
                values[1][NewKnot] = elemNodes.Depth;
                values[2][NewKnot] = elemNodes.Qx;
                values[3][NewKnot] = elemNodes.Qy;
                // граничные узлы
                if (elemNodes.BoundNodeFlag == BoundaryNodeFlag.boundaryNode)
                {
                    mesh.BoundKnots[idx] = NewKnot;
                    mesh.BoundKnotsMark[idx] = 1;
                    idx++;
                }
                // Console.WriteLine(" id {0} x {1} y {2} label {3}", e.ID, e.X, e.Y, e.Label);
                elemNodes = (RVNodeShallowWater)theMesh.NextNode;
            }
            // **************** Создание нового массива обхода ******************
            mesh.AreaElems = new TriElement[CountElems];
            //for (int i = 0; i < mesh.AreaElems.Length; i++)
            //    mesh.AreaElems[i] = new int[3];
            // перезапись треугольников
            int fe = 0;
            currentFElement = theMesh.FirstTriElements;
            while (currentFElement != null)
            {
                if (currentFElement.Status == StatusFlag.Activate)
                {
                    uint OldKnot = (uint)currentFElement.GetNode(0).ID;
                    int NewKnot = NewNumber[OldKnot];
                    mesh.AreaElems[fe].Vertex1 = (uint)NewKnot;

                    OldKnot = (uint)currentFElement.GetNode(1).ID;
                    NewKnot = NewNumber[OldKnot];
                    mesh.AreaElems[fe].Vertex2 = (uint)NewKnot;

                    OldKnot = (uint)currentFElement.GetNode(2).ID;
                    NewKnot = NewNumber[OldKnot];
                    mesh.AreaElems[fe].Vertex3 = (uint)NewKnot;

                    fe++;
                }
                currentFElement = theMesh.NextTriElements;
            }
            // **************** Создание нового массива граничных элементов ******************
            mesh.BoundElems = new TwoElement[theMesh.CountBoundarySegments];
            mesh.BoundElementsMark = new int[theMesh.CountBoundarySegments];
            //for (int i = 0; i < mesh.BoundElems.Length; i++)
            //    mesh.BoundElems[i] = new int[2];
            int be = 0;
            RVBoundary currentBoundFElement = (RVBoundary)theMesh.FirstBoundarySegment;
            while (currentBoundFElement != null)
            {
                int OldKnot = currentBoundFElement.GetNode(0).ID;
                int NewKnot0 = NewNumber[OldKnot];
                OldKnot = currentBoundFElement.GetNode(1).ID;
                int NewKnot1 = NewNumber[OldKnot];
                mesh.BoundElems[be].Vertex1 = (uint)NewKnot0;
                mesh.BoundElems[be].Vertex2 = (uint)NewKnot1;
                mesh.BoundElementsMark[be] = 1;
                be++;
                currentBoundFElement = (RVBoundary)theMesh.NextBoundarySegment;
            }
            return mesh;
        }
    }
}
