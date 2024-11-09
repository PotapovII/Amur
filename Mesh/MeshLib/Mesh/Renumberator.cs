//---------------------------------------------------------------------------
//                          ПРОЕКТ  "H?V"
//                         проектировщик:
//                  Фронтальная перенумерация 2D сетки
//                           Потапов И.И.
//                    - (C) Copyright 2000
//                      ALL RIGHT RESERVED
//---------------------------------------------------------------------------
//                 кодировка : 15.03.2001 Потапов И.И.
//                         ПРОЕКТ  "DISER"
//                 правка  :   9.03.2002 Потапов И.И.
//---------------------------------------------------------------------------
//                       ПРОЕКТ  "MeshLib"
//     перенос кода с C++ на C#, адаптация к рабочим классам сетки
//                      17.04.2021 Потапов И.И.
//---------------------------------------------------------------------------
//                  добавлена ветка TriRiverMesh 
//                      01.07.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshLib
{
    using CommonLib;
    using CommonLib.Mesh;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// ОО: Фронтальный перенумератор 2D сетки
    /// </summary>
    [Serializable]
    public class Renumberator : IRenumberator
    {
        // Новые нумера узлов
        int[] NewNumber = null;
        /// <summary>
        /// Метод фронтальной перенумерации 2D сетки
        /// </summary>
        /// <param name="mesh">сетка</param>
        /// <param name="direction">направление фронта</param>
        /// <returns></returns>
        public void FrontRenumberation(ref IMesh mesh,  Direction direction = Direction.toRight)
        {
            if (mesh as TwoMesh != null) return;
            int ix, iy, jy;
            // узлы КЭ сетки
            double[] xx = mesh.GetCoords(0);
            double[] yy = mesh.GetCoords(1);
            int CountKnots = xx.Length;
            double[] x = new double[CountKnots];
            double[] y = new double[CountKnots];
            for (int i = 0; i < CountKnots; i++)
            {
                x[i] = xx[i];
                y[i] = yy[i];
            }
            double MaxX = double.MinValue;
            double MinX = double.MaxValue;
            double MaxY = double.MinValue;
            double MinY = double.MaxValue;
            int CountBoundKnots = 0;
            // Подготовка контейнера
            for (int i = 0; i < CountKnots; i++)
            {
                if (x[i] > MaxX) MaxX = x[i];
                if (x[i] < MinX) MinX = x[i];
                if (y[i] > MaxY) MaxY = y[i];
                if (y[i] < MinY) MinY = y[i];
                CountBoundKnots++;
            }
            int CountHech = 2 * (int)Math.Sqrt(CountKnots);
            List<int>[,] XMap = new List<int>[CountHech, CountHech];
            for (int i = 0; i < CountHech; i++)
                for (int j = 0; j < CountHech; j++)
                    XMap[i, j] = new List<int>();
            // шаги хеширования
            double dx = (MaxX - MinX) / (CountHech - 1);
            double dy = (MaxY - MinY) / (CountHech - 1);
            // хеширование узлов
            for (int i = 0; i < CountKnots; i++)
            {
                ix = (int)((x[i] - MinX) / dx);
                iy = (int)((y[i] - MinY) / dy);
                XMap[ix, iy].Add(i);
            }
            // Новые нумера узлов
            NewNumber = new int[CountKnots];
            for (uint i = 0; i < CountKnots; i++)
                NewNumber[i] = -1;
            int NewIndex = 0;
            switch (direction)
            {
                case Direction.toRight:
                    {
                        // Получение новый номеров узлов
                        for (ix = 0; ix < CountHech; ix++) // по Х
                        {
                            for (iy = 0; iy < CountHech; iy++) // по Y
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
                        for (ix = 0; ix < CountHech; ix++) // по Х
                        {
                            iix = CountHech - ix - 1;
                            for (iy = 0; iy < CountHech; iy++) // по Y
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
                        for (iy = 0; iy < CountHech; iy++) // по Y
                        {
                            for (ix = 0; ix < CountHech; ix++) // по Х
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
                        for (iy = 0; iy < CountHech; iy++) // по Y
                        {
                            jy = CountHech - iy - 1;
                            for (ix = 0; ix < CountHech; ix++) // по Х
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
            // **********************************
            if (mesh as TriMesh != null)
            {
                TriMesh BedMesh = (TriMesh)mesh;
                // Перестановка координат
                for (int i = 0; i < CountKnots; i++)
                {
                    int NewKnot = NewNumber[i];
                    // новые координаты
                    BedMesh.CoordsX[NewKnot] = x[i];
                    BedMesh.CoordsY[NewKnot] = y[i];
                    //BedMesh.CoordsX[i] = x[NewKnot];
                    //BedMesh.CoordsY[i] = y[NewKnot];
                }
                // граничные узлы (флаги не меняются)
                for (int i = 0; i < BedMesh.BoundKnots.Length; i++)
                {
                    int oldKnot = BedMesh.BoundKnots[i];
                    BedMesh.BoundKnots[i] = NewNumber[oldKnot];
                }
                // Создание нового массива обхода 
                for (int fe = 0; fe < BedMesh.CountElements; fe++)
                {
                    TriElement elem = BedMesh.AreaElems[fe];
                    BedMesh.AreaElems[fe].Vertex1 = (uint)NewNumber[elem.Vertex1];
                    BedMesh.AreaElems[fe].Vertex2 = (uint)NewNumber[elem.Vertex2];
                    BedMesh.AreaElems[fe].Vertex3 = (uint)NewNumber[elem.Vertex3];
                }
                // **************** Создание нового массива граничных элементов ******************
                for (int be = 0; be < BedMesh.CountBoundElements; be++)
                {
                    TwoElement elem = BedMesh.BoundElems[be];
                    BedMesh.BoundElems[be].Vertex1 = (uint)NewNumber[elem.Vertex1];
                    BedMesh.BoundElems[be].Vertex2 = (uint)NewNumber[elem.Vertex2];
                }
                mesh = (CommonLib.IMesh)BedMesh;
                return;
            }
            if (mesh as ComplecsMesh != null)
            {
                ComplecsMesh BedMesh = (ComplecsMesh)mesh;
                // Перестановка координат
                for (int i = 0; i < CountKnots; i++)
                {
                    int NewKnot = NewNumber[i];
                    // новые координаты
                    BedMesh.CoordsX[NewKnot] = x[i];
                    BedMesh.CoordsY[NewKnot] = y[i];
                }
                // граничные узлы (флаги не меняются)
                for (int i = 0; i < BedMesh.BoundKnots.Length; i++)
                {
                    int oldKnot = BedMesh.BoundKnots[i];
                    BedMesh.BoundKnots[i] = NewNumber[oldKnot];
                }
                // Создание нового массива обхода 
                uint[] elem = null;
                for (uint fe = 0; fe < BedMesh.CountElements; fe++)
                {
                    BedMesh.ElementKnots(fe, ref elem);
                    for (uint nod = 0; nod < elem.Length; nod++)
                        BedMesh.AreaElems[fe][nod] = (uint)NewNumber[elem[nod]];
                }
                // **************** Создание нового массива граничных элементов ******************
                for (uint be = 0; be < BedMesh.CountBoundElements; be++)
                {
                    BedMesh.ElementBoundKnots(be, ref elem);
                    for (uint nod = 0; nod < elem.Length; nod++)
                        BedMesh.BoundElems[be][nod] = (uint)NewNumber[elem[nod]];
                }
                mesh = (IMesh)BedMesh;
            }
            {

            }
        }
        /// <summary>
        /// Перестановка данных при перенумерации сетки, 
        /// для скалярных полей привязанных к сетке
        /// </summary>
        /// <param name="mas"></param>
        public void RenumberationData(ref double[] mas)
        {
            if (NewNumber == null)
                throw new Exception("Перенумерация не проводилась");
            if (mas.Length != NewNumber.Length)
                throw new Exception("Перенумерация сетки не согласована с данным массивом по количеству узлов");
            // граничные узлы (флаги не меняются)
            double[] b = new double[mas.Length];
            for (int i = 0; i < mas.Length; i++)
                b[i] = mas[i];
            for (int i = 0; i < mas.Length; i++)
            {
                int NewKnot = NewNumber[i];
                mas[NewKnot] = b[i];
            }
        }
    }

}
