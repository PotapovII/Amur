//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 04.03.2024 Потапов И.И.
//---------------------------------------------------------------------------
//                      Сбока No Property River Lib
//              с убранными из наследников Property классами 
//---------------------------------------------------------------------------
namespace MeshGeneratorsLib.StripGenerator
{
    using System;

    using MeshLib;
    using MemLogLib;

    using CommonLib;
    using CommonLib.Mesh;

    /// <summary>
    /// ОО: Фронтальный генератор КЭ трехузловой сетки для русла реки в 
    /// створе с заданной геометрией дна и уровнем свободной поверхности
    /// </summary>
    [Serializable]
    public class CrossStripMeshGeneratorTri: ACrossStripMeshGenerator
    {
        /// <summary>
        /// Тип формы канала в створе потока
        /// </summary>
        SСhannelForms channelSectionForms;
        /// <summary>
        /// Создаваемая сетка
        /// </summary>
        TriMesh mesh = null;
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="MAXElem">максимальное количество КЭ сетки</param>
        /// <param name="MAXKnot">максимальное количество узлов сетки</param>
        public CrossStripMeshGeneratorTri(CrossStripMeshOption Option) 
            : base(Option)
        {
            this.channelSectionForms = Option.channelSectionForms;
        }
        /// <summary>
        /// // расчет количества конечных элементов
        /// </summary>
        /// <returns>количество конечных элементов</returns>
        public uint CalkCountElements()
        {
            CountElements = 0;
            for (int i = 0; i < Map.Count - 1; i++)
            {
                if (Map.map1D[i] == 1)
                {
                    //  i 0 ----- i+1 0
                    //     \       |
                    //        \    |
                    //           \ |   
                    //          i+1 1
                    CountElements++;
                }
                else if (Map.map1D[i + 1] == 1)
                {
                    //  i 0 ------i+1 0
                    //  |        /
                    //  |     /  
                    //  |  /      
                    //  i 1
                    CountElements++;
                }
                else
                {
                    uint Nmin = Math.Min(Map.map1D[i], Map.map1D[i + 1]) - 1;
                    for (int j = 0; j < Nmin; j++)
                    {
                        //  ij ----- i+1 j
                        //  |  \    1  |
                        //  |     \    |
                        //  |  2     \ |   
                        //  ij+1---- i+1j+1
                            CountElements++;
                            CountElements++;
                    }
                    int flag = (int)Map.map1D[i] - (int)Map.map1D[i + 1];
                    if (flag > 0)
                    {
                        //  ij ------i+1 j
                        //  |        /
                        //  |     /  
                        //  |  /      
                        //  ij+1
                        CountElements++;
                    }
                    if (flag < 0)
                    {
                        //  ij ----- i+1 j
                        //     \       |
                        //        \    |
                        //           \ |   
                        //          i+1j+1
                        CountElements++;
                    }
                }
            }
            return CountElements;
        }

        /// <summary>
        /// Создает сетку в области
        /// </summary>
        /// <param name="WetBed">смоченный периметр</param>
        /// <param name="WaterLevel">уровень свободной поверхности</param>
        /// <param name="xx">координаты дна по Х</param>
        /// <param name="yy">координаты дна по Y</param>
        /// <param name="Count">Количество узлов по дну</param>
        /// <returns></returns>
        public override IMesh CreateMesh(ref double WetBed, ref int[][] riverGates, double WaterLevel, double[] xx, double[] yy, int Count = 0)
        {
            try
            {
                CreateMap(WaterLevel, xx, yy, Count, ref WetBed);

                uint[] map1D = Map.map1D;
                uint[][] map = Map.map;
                double[][] mapZ = Map.mapZ;
                double dy = Map.dy;
                double y0 = Map.y0;
                Count = Map.Count;
                uint CountKnots = Map.CountKnots;
                uint CountElements = CalkCountElements();
                int CountBoundKnots = 2 * Count - 2;
                if (channelSectionForms == SСhannelForms.trapezoid)
                {
                    CountBoundKnots = CountBoundKnots + (int)map1D[0] + (int)map1D[Count - 1];
                }
                mesh = new TriMesh();
                mesh.tRangeMesh = TypeRangeMesh.mRange1;
                mesh.tMesh = TypeMesh.MixMesh;
                MEM.Alloc(CountKnots, ref mesh.CoordsX);
                MEM.Alloc(CountKnots, ref mesh.CoordsY);
                MEM.Alloc(CountElements, ref mesh.AreaElems);
                MEM.Alloc(CountBoundKnots, ref mesh.BoundKnots);
                MEM.Alloc(CountBoundKnots, ref mesh.BoundKnotsMark);
                MEM.Alloc(CountBoundKnots, ref mesh.BoundElementsMark);
                MEM.Alloc(CountBoundKnots, ref mesh.BoundElems);
                int WallR = 1;
                int WallL = 3;
                switch (Option.markerArea)
                {
                    case SimpleMarkerArea.crossSectionRiver:
                    case SimpleMarkerArea.crossSectionTrapezoid:
                        WallR = 0;
                        WallL = 0;
                        break;
                    case SimpleMarkerArea.crossSectionRiverLeft:
                        WallR = 0;
                        break;
                    case SimpleMarkerArea.crossSectionRiverRight:
                        WallL = 0;
                        break;
                }
                // вычисление массивов координат
                CountKnots = 0;
                riverGates = new int[Count][];
                for (int i = 0; i < Count; i++)
                {
                    double y = y0 + dy * i;
                    riverGates[i] = new int[map1D[i]];
                    for (int j = 0; j < map1D[i]; j++)
                    {
                        mesh.CoordsX[CountKnots] = y;
                        mesh.CoordsY[CountKnots] = mapZ[i][j];
                        riverGates[i][j] = (int)CountKnots;
                        CountKnots++;
                    }
                }
                // вычисление массивов обхода
                CountElements = 0;
                for (int i = 0; i < Count - 1; i++)
                {
                    if (map1D[i] == 1)
                    {
                        //  i 0 ----- i+1 0
                        //     \       |
                        //        \    |
                        //           \ |   
                        //          i+1 1
                        mesh.AreaElems[CountElements].Vertex1 = map[i][0];
                        mesh.AreaElems[CountElements].Vertex2 = map[i + 1][1];
                        mesh.AreaElems[CountElements].Vertex3 = map[i + 1][0];
                        CountElements++;
                    }
                    else if (map1D[i + 1] == 1)
                    {
                        //  i 0 ------i+1 0
                        //  |        /
                        //  |     /  
                        //  |  /      
                        //  i 1
                        mesh.AreaElems[CountElements].Vertex1 = map[i][0];
                        mesh.AreaElems[CountElements].Vertex2 = map[i][1];
                        mesh.AreaElems[CountElements].Vertex3 = map[i + 1][0];
                        CountElements++;
                    }
                    else
                    {
                        uint Nmin = Math.Min(map1D[i], map1D[i + 1]) - 1;
                        for (int j = 0; j < Nmin; j++)
                        {
                            //  ij ----- i+1 j
                            //  |  \    1  |
                            //  |     \    |
                            //  |  2     \ |   
                            //  ij+1---- i+1j+1
                            double dx1 = mesh.CoordsX[map[i][j]] - mesh.CoordsX[map[i + 1][j + 1]];
                            double dy1 = mesh.CoordsY[map[i][j]] - mesh.CoordsY[map[i + 1][j + 1]];
                            double L1 = dx1 * dx1 + dy1 * dy1;

                            double dx2 = mesh.CoordsX[map[i+1][j]] - mesh.CoordsX[map[i][j + 1]];
                            double dy2 = mesh.CoordsY[map[i+1][j]] - mesh.CoordsY[map[i][j + 1]];
                            double L2 = dx2 * dx2 + dy2 * dy2;
                            if (L1 <= 0.99*L2)
                            {
                                mesh.AreaElems[CountElements].Vertex1 = map[i][j];
                                mesh.AreaElems[CountElements].Vertex2 = map[i + 1][j + 1];
                                mesh.AreaElems[CountElements].Vertex3 = map[i + 1][j];
                                CountElements++;

                                mesh.AreaElems[CountElements].Vertex1 = map[i][j];
                                mesh.AreaElems[CountElements].Vertex2 = map[i][j + 1];
                                mesh.AreaElems[CountElements].Vertex3 = map[i + 1][j + 1];
                                CountElements++;
                            }
                            else
                            {
                                //  ij ----- i+1 j
                                //  |  1    /  |
                                //  |     /    |
                                //  |  /     2 |   
                                //  ij+1---- i+1j+1
                                mesh.AreaElems[CountElements].Vertex1 = map[i][j];
                                mesh.AreaElems[CountElements].Vertex2 = map[i][j + 1];
                                mesh.AreaElems[CountElements].Vertex3 = map[i + 1][j];
                                CountElements++;

                                mesh.AreaElems[CountElements].Vertex1 = map[i][j +1];
                                mesh.AreaElems[CountElements].Vertex2 = map[i + 1][j + 1];
                                mesh.AreaElems[CountElements].Vertex3 = map[i + 1][j];
                                CountElements++;
                            }
                        }
                        int flag = (int)map1D[i] - (int)map1D[i + 1];
                        if (flag > 0)
                        {
                            //  ij ------i+1 j
                            //  |        /
                            //  |     /  
                            //  |  /      
                            //  ij+1
                            uint j = map1D[i] - 2;
                            mesh.AreaElems[CountElements].Vertex1 = map[i][j];
                            mesh.AreaElems[CountElements].Vertex2 = map[i][j + 1];
                            mesh.AreaElems[CountElements].Vertex3 = map[i + 1][j];
                            CountElements++;
                        }
                        if (flag < 0)
                        {
                            //  ij ----- i+1 j
                            //     \       |
                            //        \    |
                            //           \ |   
                            //          i+1j+1
                            uint j = map1D[i + 1] - 2;
                            mesh.AreaElems[CountElements].Vertex1 = map[i][j];
                            mesh.AreaElems[CountElements].Vertex2 = map[i + 1][j + 1];
                            mesh.AreaElems[CountElements].Vertex3 = map[i + 1][j];
                            CountElements++;
                        }
                    }
                }
                CountKnots = 0;
                // дно канала
                for (int i = 0; i < Count; i++)
                {
                    mesh.BoundKnots[CountKnots] = (int)map[i][map1D[i] - 1];
                    mesh.BoundKnotsMark[CountKnots] = 0;
                    CountKnots++;
                }
                // свободная поверхность
                for (int i = 1; i < Count - 1; i++)
                {
                    mesh.BoundKnots[CountKnots] = (int)map[i][0];
                    mesh.BoundKnotsMark[CountKnots] = 2;
                    CountKnots++;
                }
                if (channelSectionForms == SСhannelForms.trapezoid)
                {
                    // левая сторона
                    for (int i = 0; i < map1D[0]; i++)
                    {
                        mesh.BoundKnots[CountKnots] = (int)map[0][i];
                        mesh.BoundKnotsMark[CountKnots] = 3;
                        CountKnots++;
                    }
                    // правая сторона
                    for (int i = 0; i < map1D[Count - 1]; i++)
                    {
                        mesh.BoundKnots[CountKnots] = (int)map[Count - 1][i];
                        mesh.BoundKnotsMark[CountKnots] = 1;
                        CountKnots++;
                    }
                }

                // дно канала
                int belem = 0;
                for (int i = 0; i < Count - 1; i++)
                {
                    mesh.BoundElems[belem].Vertex1 = map[i][map1D[i] - 1];
                    mesh.BoundElems[belem].Vertex2 = map[i + 1][map1D[i + 1] - 1];
                    mesh.BoundElementsMark[belem] = 0;
                    belem++;
                }
                // свободная поверхность
                for (int i = 0; i < Count - 1; i++)
                {
                    mesh.BoundElems[belem].Vertex1 = map[i][0];
                    mesh.BoundElems[belem].Vertex2 = map[i + 1][0];
                    mesh.BoundElementsMark[belem] = 2;
                    belem++;
                }

                if (channelSectionForms == SСhannelForms.trapezoid)
                {
                    // левая сторона
                    for (int i = 0; i < map1D[0]-1; i++)
                    {
                        mesh.BoundElems[belem].Vertex1 = map[0][i];
                        mesh.BoundElems[belem].Vertex2 = map[0][i+1];
                        mesh.BoundElementsMark[belem] = 3;
                        belem++;
                    }
                    // правая сторона
                    for (int i = 0; i < map1D[Count - 1]-1; i++)
                    {
                        mesh.BoundElems[belem].Vertex1 = map[Count - 1][i];
                        mesh.BoundElems[belem].Vertex2 = map[Count - 1][i+1];
                        mesh.BoundElementsMark[belem] = 1;
                        belem++;
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.Instance.Exception(ex);
                Logger.Instance.Info("ОШИБКА: Проверте согласованность данных по геометрии створа");
            }
            return mesh;
        }
    }
}

