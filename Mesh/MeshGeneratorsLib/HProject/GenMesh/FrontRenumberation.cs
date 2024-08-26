using System;
using System.Collections.Generic;
using System.Linq;
using CommonLib;
using CommonLib.Mesh;
using MemLogLib;

namespace MeshGeneratorsLib
{
    class MeshAdapter
    {/// <summary>
     /// Фронтальный перенумератор сетки
     /// </summary>
     /// <param name="mesh"></param>
     /// <param name="direction"></param>
     /// <returns></returns>
        public static void Renumberation(ref IMesh bmesh, Direction direction = Direction.toRight)
        {
        }
        public static void Renumberation(ref IFEMesh bmesh, Direction direction = Direction.toRight)
        {
        }
        //public static void Renumberation(ref IMesh bmesh, IMesh mesh, Direction direction = Direction.toRight)
        //{
        //    IMesh BedMesh = new TriMesh();
        //    int ix, iy, jy;
        //    // узлы КЭ сетки
        //    ICollection<Vertex> Vertices = mesh.Vertices;
        //    // FE
        //    ICollection<Triangle> Triangles = mesh.Triangles;
        //    // BFE
        //    IEnumerable<Edge> Edges = mesh.Edges;

        //    int CountKnots = (int)(Math.Sqrt(Vertices.Count)) + 2;
        //    List<int>[,] XMap = new List<int>[CountKnots, CountKnots];
        //    for (int i = 0; i < CountKnots; i++)
        //        for (int j = 0; j < CountKnots; j++)
        //            XMap[i, j] = new List<int>();

        //    double MaxX = double.MinValue;
        //    double MinX = double.MaxValue;
        //    double MaxY = double.MinValue;
        //    double MinY = double.MaxValue;
        //    int CountBoundKnots = 0;
        //    int MaxID = 0;
        //    // Подготовка контейнера
        //    foreach (var e in Vertices)
        //    {
        //        if (e.X > MaxX) MaxX = e.X;
        //        if (e.X < MinX) MinX = e.X;
        //        if (e.Y > MaxY) MaxY = e.Y;
        //        if (e.Y < MinY) MinY = e.Y;
        //        if (MaxID < e.ID) MaxID = e.ID;
        //        if (e.Label > 0)
        //            CountBoundKnots++;
        //    }
        //    BedMesh.BoundKnots = new int[CountBoundKnots];
        //    BedMesh.BoundKnotsMark = new int[CountBoundKnots];
        //    // шаги хеширования
        //    double dx = (MaxX - MinX) / (CountKnots - 1);
        //    double dy = (MaxY - MinY) / (CountKnots - 1);
        //    // хеширование узлов
        //    foreach (var e in Vertices)
        //    {
        //        ix = (int)((e.X - MinX) / dx);
        //        iy = (int)((e.Y - MinY) / dy);
        //        XMap[ix, iy].Add(e.ID);
        //    }
        //    // Новые нумера узлов
        //    int[] NewNumber = new int[MaxID + 1];
        //    for (uint i = 0; i < CountKnots; i++)
        //        NewNumber[i] = -1;
        //    int NewIndex = 0;
        //    switch (direction)
        //    {
        //        case Direction.toRight:
        //            {
        //                // Получение новый номеров узлов
        //                for (ix = 0; ix < CountKnots; ix++) // по Х
        //                {
        //                    for (iy = 0; iy < CountKnots; iy++) // по Y
        //                    {
        //                        int CountX = XMap[ix, iy].Count();
        //                        for (int i = 0; i < CountX; i++) // по узлам в хеше
        //                        {
        //                            int Old = XMap[ix, iy][i];
        //                            NewNumber[Old] = NewIndex;
        //                            NewIndex++;
        //                        }
        //                    }
        //                }
        //                break;
        //            }
        //        case Direction.toLeft:
        //            {
        //                // Получение новый номеров узлов
        //                int iix;
        //                for (ix = 0; ix < CountKnots; ix++) // по Х
        //                {
        //                    iix = CountKnots - ix - 1;
        //                    for (iy = 0; iy < CountKnots; iy++) // по Y
        //                    {
        //                        int CountX = XMap[iix, iy].Count();
        //                        for (int i = 0; i < CountX; i++) // по узлам в хеше
        //                        {
        //                            int Old = XMap[iix, iy][i];
        //                            NewNumber[Old] = NewIndex;
        //                            NewIndex++;
        //                        }
        //                    }
        //                }
        //                break;
        //            }
        //        case Direction.toDown:
        //            {
        //                for (iy = 0; iy < CountKnots; iy++) // по Y
        //                {
        //                    for (ix = 0; ix < CountKnots; ix++) // по Х
        //                    {
        //                        int CountX = XMap[ix, iy].Count();
        //                        for (int i = 0; i < CountX; i++) // по Y
        //                        {
        //                            int Old = XMap[ix, iy][i];
        //                            NewNumber[Old] = NewIndex;
        //                            NewIndex++;
        //                        }
        //                    }
        //                }
        //                break;
        //            }
        //        case Direction.toUp:
        //            {
        //                for (iy = 0; iy < CountKnots; iy++) // по Y
        //                {
        //                    jy = CountKnots - iy - 1;
        //                    for (ix = 0; ix < CountKnots; ix++) // по Х
        //                    {
        //                        int CountX = XMap[ix, jy].Count();
        //                        for (int i = 0; i < CountX; i++) // по Y
        //                        {
        //                            int Old = XMap[ix, jy][i];
        //                            NewNumber[Old] = NewIndex;
        //                            NewIndex++;
        //                        }
        //                    }
        //                }
        //                break;
        //            }
        //    }
        //    // **************** Создание нового массива координат ******************
        //    BedMesh.CoordsX = new double[Vertices.Count];
        //    BedMesh.CoordsY = new double[Vertices.Count];
        //    int idx = 0;
        //    foreach (var e in Vertices)
        //    {
        //        int OldKnot = e.ID;
        //        int NewKnot = NewNumber[OldKnot];
        //        // координаты
        //        BedMesh.CoordsX[NewKnot] = e.X;
        //        BedMesh.CoordsY[NewKnot] = e.Y;
        //        // граничные узлы
        //        if (e.Label > 0)
        //        {
        //            BedMesh.BoundKnots[idx] = NewKnot;
        //            BedMesh.BoundKnotsMark[idx] = e.Label;
        //            idx++;
        //        }
        //        // Console.WriteLine(" id {0} x {1} y {2} label {3}", e.ID, e.X, e.Y, e.Label);
        //    }
        //    // **************** Создание нового массива обхода ******************
        //    BedMesh.AreaElems = new TriElement[Triangles.Count];
        //    //for (int i = 0; i < BedMesh.AreaElems.Length; i++)
        //    //    BedMesh.AreaElems[i] = new int[3];
        //    // перезапись треугольников
        //    int fe = 0;
        //    foreach (var e in Triangles)
        //    {
        //        int OldKnot = e.GetVertexID(0);
        //        int NewKnot = NewNumber[OldKnot];
        //        BedMesh.AreaElems[fe].Vertex1 = (uint)NewKnot;

        //        OldKnot = e.GetVertexID(1);
        //        NewKnot = NewNumber[OldKnot];
        //        BedMesh.AreaElems[fe].Vertex2 = (uint)NewKnot;

        //        OldKnot = e.GetVertexID(2);
        //        NewKnot = NewNumber[OldKnot];
        //        BedMesh.AreaElems[fe].Vertex3 = (uint)NewKnot;
        //        //for (int j = 0; j < 3; j++)
        //        //{
        //        //    int OldKnot = e.GetVertexID(j);
        //        //    int NewKnot = NewNumber[OldKnot];
        //        //    BedMesh.AreaElems[fe][j] = NewKnot;
        //        //}
        //        fe++;
        //    }
        //    // **************** Создание нового массива граничных элементов ******************
        //    BedMesh.BoundElems = new TwoElement[Edges.Count()];
        //    BedMesh.BoundElementsMark = new int[Edges.Count()];

        //    //for (int i = 0; i < BedMesh.BoundElems.Length; i++)
        //    //    BedMesh.BoundElems[i] = new int[2];
        //    int be = 0;
        //    foreach (var e in Edges)
        //    {
        //        int OldKnot = e.P0;
        //        int NewKnot0 = NewNumber[OldKnot];
        //        OldKnot = e.P1;
        //        int NewKnot1 = NewNumber[OldKnot];
        //        BedMesh.BoundElems[be].Vertex1 = (uint)NewKnot0;
        //        BedMesh.BoundElems[be].Vertex2 = (uint)NewKnot1;
        //        BedMesh.BoundElementsMark[be] = e.Label;
        //        be++;
        //    }
        //    bmesh = BedMesh;
        //}
    }
}
