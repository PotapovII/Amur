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
//                 реф. кодировка : 30.03.2025 Потапов И.И.
//                      настройка на Option
//---------------------------------------------------------------------------
namespace MeshGeneratorsLib.StripGenerator
{
    using System;

    using MeshLib;
    using MemLogLib;
    using CommonLib;
    using System.Linq;

    /// <summary>
    /// ОО: Фронтальный генератор КЭ трехузловой сетки для русла реки в 
    /// створе с заданной геометрией дна и уровнем свободной поверхности
    /// </summary>
    [Serializable]
    public class StripMeshGenerator : ACrossStripMeshGenerator
    {
        CrossStripMapBr Map;
        /// <summary>
        /// Создаваемая сетка
        /// </summary>
        ComplecsMesh mesh = null;
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="MAXElem">максимальное количество КЭ сетки</param>
        /// <param name="MAXKnot">максимальное количество узлов сетки</param>
        public StripMeshGenerator(CrossStripMeshOption Option)
            : base(Option)
        {
        }
        public StripMeshGenerator() : this(new CrossStripMeshOption()) { }
        /// <summary>
        /// // расчет количества конечных элементов
        /// </summary>
        /// <returns>количество конечных элементов</returns>
        public uint CalkCountElements()
        {
            CountElements = 0;
            uint CountEl3 = 0;
            uint CountEl4 = 0;
            for (int i = 0; i < Map.Count - 1; i++)
            {
                if (Map.map1D[i] == 1)
                {
                    CountElements++;
                    CountEl3++;
                }
                else if (Map.map1D[i + 1] == 1)
                {
                    CountElements++;
                    CountEl3++;
                }
                else
                {
                    if (Option.typeMesh == TypeMesh.Triangle)
                    {
                        uint Nmin = Math.Min(Map.map1D[i], Map.map1D[i + 1]) - 1;
                        CountElements += 2 * Nmin;
                        CountEl3 += 2 * Nmin;
                        int flag = Math.Abs((int)Map.map1D[i] - (int)Map.map1D[i + 1]);
                        CountElements += (uint)flag;
                        CountEl3 += (uint)flag;
                    }
                    else
                    {
                        uint Nmin = Math.Min(Map.map1D[i], Map.map1D[i + 1]) - 1;
                        CountElements += Nmin;
                        CountEl4 += Nmin;
                        int flag = Math.Abs((int)Map.map1D[i] - (int)Map.map1D[i + 1]);
                        CountElements += (uint)flag;
                        CountEl3 += (uint)flag;
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
                // расчет конечных элементов
                CalkBedFunction(ref WetBed, WaterLevel, xx, yy);
                if (Count == 0)
                    Count = CountBed;
                double ymin = yy.Min();
                Map = new CrossStripMapBr(Option.BoundaryClose);
                Map.CreateMap(spline, WaterLevel, ymin, Count, width, left, right, dryLeft, dryRight);

                uint[] map1D = Map.map1D;
                uint[][] map = Map.map;
                double[][] mapZ = Map.mapZ;
                double[][] mapY = Map.mapY;
                double dy = Map.dy;
                double y0 = Map.y0;
                Count = Map.Count;
                uint CountKnots = Map.CountKnots;
                uint CountElements = CalkCountElements();
                int CountBoundKnots = 2 * Count - 2;
                int iL = 0;
                int iR = Count - 1;
                uint CountR = map1D[iR];
                uint CountL = map1D[iL];
                uint CountInR = CountR > 2 ? CountR - 2 : 0;
                uint CountInL = CountL > 2 ? CountL - 2 : 0;
                if (CountInR > 0)
                    CountBoundKnots += (int)CountInR + 1;
                if (CountInL > 0)
                    CountBoundKnots += (int)CountInL + 1;
                int CountBoundElems = CountBoundKnots;
                
                int WallR = 1;
                int WallL = 3;
                switch ( Option.markerArea)
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
                mesh = new ComplecsMesh();
                mesh.tRangeMesh = TypeRangeMesh.mRange1;
                mesh.tMesh = TypeMesh.MixMesh;
                MEM.Alloc(CountKnots, ref mesh.CoordsX);
                MEM.Alloc(CountKnots, ref mesh.CoordsY);
                MEM.Alloc(CountElements, ref mesh.AreaElems);
                MEM.Alloc(CountElements, ref mesh.AreaElemsFFType);
                MEM.Alloc(CountBoundKnots, ref mesh.BoundKnots);
                MEM.Alloc(CountBoundKnots, ref mesh.BoundKnotsMark);
                MEM.Alloc(CountBoundElems, ref mesh.BoundElementsMark);
                MEM.Alloc(CountBoundElems, ref mesh.BoundElems);
                // вычисление массивов координат
                CountKnots = 0;
                riverGates = new int[Count][];
                for (int i = 0; i < Count; i++)
                {
                    double x = y0 + dy * i;
                    riverGates[i] = new int[map1D[i]];
                    for (int j = 0; j < map1D[i]; j++)
                    {
                        mesh.CoordsX[CountKnots] = mapY[i][j];
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
                        mesh.AreaElems[CountElements] = new uint[3]
                        {
                            map[i][0], map[i+1][1], map[i+1][0]
                        };
                        mesh.AreaElemsFFType[CountElements] = TypeFunForm.Form_2D_TriangleAnalitic_L1;
                        CountElements++;
                    }
                    else if (map1D[i + 1] == 1)
                    {
                        //  i 0 ------i+1 0
                        //  |        /
                        //  |     /  
                        //  |  /      
                        //  i 1
                        mesh.AreaElems[CountElements] = new uint[3]
                        {
                            map[i][0], map[i][1], map[i + 1][0]
                        };
                        mesh.AreaElemsFFType[CountElements] = TypeFunForm.Form_2D_TriangleAnalitic_L1;
                        CountElements++;
                    }
                    else
                    {
                        uint Nmin = Math.Min(map1D[i], map1D[i + 1]) - 1;
                        for (int j = 0; j < Nmin; j++)
                        {

                            if (Option.typeMesh == TypeMesh.Triangle)
                            {

                                double dx1 = mesh.CoordsX[map[i][j]] - mesh.CoordsX[map[i + 1][j + 1]];
                                double dy1 = mesh.CoordsY[map[i][j]] - mesh.CoordsY[map[i + 1][j + 1]];
                                double L1 = dx1 * dx1 + dy1 * dy1;

                                double dx2 = mesh.CoordsX[map[i + 1][j]] - mesh.CoordsX[map[i][j + 1]];
                                double dy2 = mesh.CoordsY[map[i + 1][j]] - mesh.CoordsY[map[i][j + 1]];
                                double L2 = dx2 * dx2 + dy2 * dy2;

                                if (L1 <= 0.95 * L2)// || i > Count / 2)
                                {
                                    //  ij ----- i+1 j
                                    //  |  \    2  |
                                    //  |     \    |
                                    //  |  1     \ |   
                                    //  ij+1---- i+1j+1
                                    mesh.AreaElems[CountElements] = new uint[3]
                                    {
                                        map[i][j], map[i+1][j+1], map[i+1][j]
                                    };
                                    mesh.AreaElemsFFType[CountElements] = TypeFunForm.Form_2D_TriangleAnalitic_L1;
                                    CountElements++;
                                    mesh.AreaElems[CountElements] = new uint[3]
                                    {
                                        map[i][j], map[i][j+1], map[i+1][j+1]
                                    };
                                    mesh.AreaElemsFFType[CountElements] = TypeFunForm.Form_2D_TriangleAnalitic_L1;
                                    CountElements++;
                                }
                                else
                                {
                                    //  ij ----- i+1 j
                                    //  |  1    /  |
                                    //  |     /    |
                                    //  |  /     2 |   
                                    //  ij+1---- i+1j+1
                                    mesh.AreaElems[CountElements] = new uint[3]
                                    {
                                        map[i][j], map[i][j+1], map[i+1][j]
                                    };
                                    mesh.AreaElemsFFType[CountElements] = TypeFunForm.Form_2D_TriangleAnalitic_L1;
                                    CountElements++;
                                    mesh.AreaElems[CountElements] = new uint[3]
                                    {
                                        map[i][j+1], map[i+1][j+1], map[i+1][j]
                                    };
                                    mesh.AreaElemsFFType[CountElements] = TypeFunForm.Form_2D_TriangleAnalitic_L1;
                                    CountElements++;
                                }
                            }
                            else
                            {
                                //  ij ----- i+1 j
                                //  |  0     3 |
                                //  |          |
                                //  |  1     2 |   
                                //  ij+1---- i+1j+1
                                mesh.AreaElems[CountElements] = new uint[4]
                                {
                                     map[i][j], map[i][j+1], map[i+1][j+1], map[i+1][j]
                                };
                                mesh.AreaElemsFFType[CountElements] = TypeFunForm.Form_2D_Rectangle_L1;
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
                            mesh.AreaElems[CountElements] = new uint[3]
                            {
                                 map[i][j], map[i][j+1], map[i+1][j]
                            };
                            mesh.AreaElemsFFType[CountElements] = TypeFunForm.Form_2D_TriangleAnalitic_L1;
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
                            mesh.AreaElems[CountElements] = new uint[3]
                            {
                                map[i][j], map[i+1][j+1], map[i+1][j]
                            };
                            mesh.AreaElemsFFType[CountElements] = TypeFunForm.Form_2D_TriangleAnalitic_L1;
                            CountElements++;
                        }
                    }
                }
                CountKnots = 0;
                // свободная поверхность
                for (int i = 1; i < Count - 1; i++)
                {
                    mesh.BoundKnots[CountKnots] = (int)map[i][0];
                    mesh.BoundKnotsMark[CountKnots] = 2;
                    CountKnots++;
                }
                //  Левая стенка канала
                for (int j = 0; j < CountL - 1; j++)
                {
                    mesh.BoundKnots[CountKnots] = (int)map[iL][j];
                    mesh.BoundKnotsMark[CountKnots] = WallL;
                    CountKnots++;
                }
                // дно канала
                for (int i = 0; i < Count; i++)
                {
                    mesh.BoundKnots[CountKnots] = (int)map[i][map1D[i] - 1];
                    mesh.BoundKnotsMark[CountKnots] = 0;
                    CountKnots++;
                }
                // Правая стенка канала
                for (int j = (int)CountR - 2; j > -1; j--)
                {
                    mesh.BoundKnots[CountKnots] = (int)map[iR][j];
                    mesh.BoundKnotsMark[CountKnots] = WallR;
                    CountKnots++;
                }
                int belem = 0;
                // свободная поверхность
                for (int i = 0; i < Count - 1; i++)
                {
                    mesh.BoundElems[belem] = new uint[2]
                    {
                         map[i][0], map[i+1][0]
                    };
                    mesh.BoundElementsMark[belem] = 2;
                    belem++;
                }
                //  Левая стенка канала
                for (int j = 0; j < CountL - 1; j++)
                {
                    mesh.BoundElems[belem] = new uint[2]
                    {
                         map[iL][j], map[iL][j+1]
                    };
                    mesh.BoundElementsMark[belem] = WallL;
                    belem++;
                }
                // дно канала
                for (int i = 0; i < Count - 1; i++)
                {
                    mesh.BoundElems[belem] = new uint[2]
                    {
                         map[i][map1D[i] - 1], map[i+1][map1D[i+1] - 1]
                    };
                    mesh.BoundElementsMark[belem] = 0;
                    belem++;
                }
                // Правая стенка канала
                for (int j = (int)CountR - 2; j > -1; j--)
                {
                    mesh.BoundElems[belem] = new uint[2]
                    {
                         map[iR][j], map[iR][j+1]
                    };
                    mesh.BoundElementsMark[belem] = WallR;
                    belem++;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
                Logger.Instance.Info("ОШИБКА: Проверте согласованность данных по геометрии створа");
            }
            return mesh;
        }
    }
}

