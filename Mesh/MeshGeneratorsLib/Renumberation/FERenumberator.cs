#region License
/*
Copyright (c) 2019 - 2024  Potapov Igor Ivanovich, Khabarovsk

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion
//---------------------------------------------------------------------------
//                          ПРОЕКТ  "МКЭ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 27.11.2021 Потапов И.И.
//---------------------------------------------------------------------------
//                 обобщение : 11.06.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshGeneratorsLib.Renumberation
{
    using System.Linq;
    using System.Collections.Generic;

    using MeshLib;
    using MemLogLib;
    using CommonLib;
    using CommonLib.Mesh;
    public class FERenumberator : ARenumberator
    {
        /// <summary>
        /// Фронтальный перенумератор сетки с квадратным хешированием
        /// Жаден до памяти!!!
        /// </summary>
        /// <param name="NewMesh"></param>
        /// <param name="mesh"></param>
        /// <param name="direction"></param>
        public override void FrontRenumberation(ref IFEMesh NewMesh, IFEMesh mesh, Direction direction = Direction.toRight)
        {
            Set(mesh);
            int ix, iy, jy;
            double MaxX = double.MinValue;
            double MinX = double.MaxValue;
            double MaxY = double.MinValue;
            double MinY = double.MaxValue;
            mesh.MinMax(0, ref MinX, ref MaxX);
            mesh.MinMax(1, ref MinY, ref MaxY);
            int CountHash = CountKnots;
            List<int>[,] XMap = new List<int>[CountHash, CountHash];
            for (int i = 0; i < XMap.GetLength(0); i++)
                for (int j = 0; j < XMap.GetLength(1); j++)
                    XMap[i, j] = new List<int>();
            // шаги хеширования

            double dx = (MaxX - MinX) / (XMap.GetLength(0) - 1);
            double dy = (MaxY - MinY) / (XMap.GetLength(0) - 1);

            // хеширование узлов
            for (int i = 0; i < CountKnots; i++)
            {
                ix = (int)((X[i] - MinX) / dx);
                iy = (int)((Y[i] - MinY) / dy);
                XMap[ix, iy].Add(i);
            }
            // Новые нумера узлов
            int[] NewNumber = null;
            MEM.VAlloc(CountKnots + 1, -1, ref NewNumber);
            int NewIndex = 0;
            switch (direction)
            {
                case Direction.toRight:
                    {
                        // Получение новый номеров узлов
                        for (ix = 0; ix < XMap.GetLength(0); ix++) // по Х
                        {
                            for (iy = 0; iy < XMap.GetLength(1); iy++) // по Y
                            {
                                int CountX = XMap[ix, iy].Count();
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
                        for (ix = 0; ix < CountHash; ix++) // по Х
                        {
                            iix = CountHash - ix - 1;
                            for (iy = 0; iy < CountHash; iy++) // по Y
                            {
                                int CountX = XMap[iix, iy].Count();
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
                        for (iy = 0; iy < CountHash; iy++) // по Y
                        {
                            for (ix = 0; ix < CountHash; ix++) // по Х
                            {
                                int CountX = XMap[ix, iy].Count();
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
                        for (iy = 0; iy < CountHash; iy++) // по Y
                        {
                            jy = CountHash - iy - 1;
                            for (ix = 0; ix < CountHash; ix++) // по Х
                            {
                                int CountX = XMap[ix, jy].Count();
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
            DoRenumberation(ref NewMesh, NewNumber, direction);
            //NewMesh = new FEMesh(mesh);
            //for (int i = 0; i < CountKnots; i++)
            //{
            //    int OldKnot = i;
            //    int NewKnot = NewNumber[OldKnot];
            //    // координаты
            //    NewMesh.CoordsX[NewKnot] = X[i];
            //    NewMesh.CoordsY[NewKnot] = Y[i];
            //}
            //for (int i = 0; i < NewMesh.BNods.Length; i++)
            //{
            //    int OldID = NewMesh.BNods[i].ID;
            //    NewMesh.BNods[i].ID = NewNumber[OldID];
            //    //Console.WriteLine("{2} old {0} new {1}", OldID, NewNumber[OldID], i);
            //}
            //for (int e = 0; e < NewMesh.CountElements; e++)
            //{
            //    for (int i = 0; i < NewMesh.AreaElems[e].Length; i++)
            //    {
            //        int OldID = NewMesh.AreaElems[e][i].ID;
            //        NewMesh.AreaElems[e][i].ID = NewNumber[OldID];
            //    }
            //}
            //for (int e = 0; e < NewMesh.CountBoundElements; e++)
            //{
            //    for (int i = 0; i < NewMesh.BoundElems[e].Length; i++)
            //    {
            //        int OldID = NewMesh.BoundElems[e][i].ID;
            //        NewMesh.BoundElems[e][i].ID = NewNumber[OldID];
            //    }
            //}
        }
    }
}
