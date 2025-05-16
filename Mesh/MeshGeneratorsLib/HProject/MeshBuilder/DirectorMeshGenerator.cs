//---------------------------------------------------------------------------
//                         проектировщик:
//                           Потапов И.И.
//                  - (C) Copyright 2000-2003
//                      ALL RIGHT RESERVED
//---------------------------------------------------------------------------
//                 кодировка : 1.02.2003 Потапов И.И.
//               ПРОЕКТ  "MixTasker' на базе "DISER"
//---------------------------------------------------------------------------
//        Перенос на C#, вариант от : 05.02.2022  Потапов И.И.
//    реализация генерации базисной КЭ сетки без поддержки полей свойств
//                         убран 4 порядок сетки 
//---------------------------------------------------------------------------
namespace MeshGeneratorsLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CommonLib;
    using CommonLib.Mesh;
    using MeshLib;
    using MemLogLib;
    using GeometryLib;
    using MeshGeneratorsLib.Renumberation;

    /// <summary>
    /// ОО: Директор - строит КЭ сетку выбранным строителем/ми
    /// </summary>
    public class DirectorMeshGenerator : MeshBuilderArgs
    {
        protected const uint NRib = 4;
        protected const uint NCfg = 3;
        /// <summary>
        /// тип геометрии конечных элементов используемых при генерации КЭ сетки
        /// </summary>
        protected TypeMesh typeMesh = TypeMesh.MixMesh;
        /// <summary>
        /// информация об области разбиения
        /// </summary>
        protected HInfoTaskArea InfoMesh;
        /// <summary>
        /// количество подобластей
        /// </summary>
        protected int CountAreas;
        /// <summary>
        /// количество узлов на границах подобласти
        /// </summary>
        protected int[] KnotRibs = { 0, 0, 0, 0 };
        /// <summary>
        /// количество КЭ - в на границах подобласти
        /// </summary>
        protected int[] FElemRibs = { 0, 0, 0, 0 };
        /// <summary>
        /// количество ребер
        /// </summary>
        protected int CRibs;

        /// <summary>
        /// данные о генерации ->> в свойства генератора
        /// </summary>
        public HMeshParams meshData;
        /// <summary>
        /// Описания подобластей расчетной оббласти
        /// </summary>
        public IHTaskMap mapMesh;
        /// <summary>
        /// cтроителm сетки
        /// </summary>
        IMeshBuilder meshBuilder = null;

        IFERenumberator renumberator = null;
        /// <summary>
        /// Установка построителя сетки
        /// </summary>
        /// <param name="builder"></param>
        public void Set(IMeshBuilder meshBuilder)
        {
            this.meshBuilder = meshBuilder;
        }
        public DirectorMeshGenerator()
        {
            for (int i = 0; i < 4; i++)
                RealSegmRibs[i] = new HMapSegment();
        }
        public DirectorMeshGenerator(IMeshBuilder meshBuilder, HMeshParams meshData, IHTaskMap mapMesh,
            IFERenumberator renumberator) : this()
        {
            this.mapMesh = mapMesh;
            this.meshData = meshData;
            this.renumberator = renumberator;
            InfoMesh = new HInfoTaskArea();
            // генерация КЭ сетки в подобласти
            if (meshBuilder == null)
                this.meshBuilder = Clone(meshData.meshMethod);
            else
                this.meshBuilder = meshBuilder;
        }
        public IFEMesh Create()
        {
            IFEMesh Mesh = null;
            try
            {
                // Приведение области к сегментному описанию
                InfoMesh.Set(mapMesh);
                // Количество подобластей
                CountAreas = InfoMesh.NumAreas.Count;
                // Запуск кансоли
                // получение типпа и порядка для генерации КЭ сетки
                //MeshType = TData->MeshType;
                meshRange = meshData.meshRange;
                meshType = meshData.meshType;
                // очистка БКЭ  сетки
                Mesh = new FEMesh();
                // поиск корректного расположения узлов базовой сетки
                // на границах сегментов и вычисление их координат
                LookingCorrSegmKnot();
                // трассировка
                Logger.Instance.Tracer("Создание сетки в " + CountAreas + " подобластях");
                // цикл по подобластям
                for (area = 0; area < CountAreas; area++)
                {
                    flag = 0;
                    // установка данных для генерации в подобласти
                    // и определение ориентации сегментов подобласти
                    // OrientationSegment(area);
                    // Подготовка данных для генерации сетки в текущей подобласти
                    CRibs = InfoMesh.GetRealSegmRibs(area, meshRange, ref KnotRibs,
                        ref FElemRibs, ref RealSegmRibs);
                    // подготовка узловых карт области (определение размерностей для текущей подобласти)
                    SetMapsForRun();
                    // создание сетки подобласти
                    IFEMesh SMesh = new FEMesh(meshData.CountParams, meshData.meshRange);
                    // настройка билдера
                    meshBuilder.Set(this, SMesh);
                    #region построение сетки 
                    // вычисление координат
                    meshBuilder.BuilderCoords();
                    // Определение массива граничных узлов
                    meshBuilder.BuilderBNods();
                    // Определение массива граничных конечных элементов
                    meshBuilder.BuilderBElement();
                    // Определение массива конечных элементов в подобласти
                    meshBuilder.BuilderAElement();
                    // Определение массива параметров сетки
                    meshBuilder.BuilderParams();
                    #endregion
                    SMesh = meshBuilder.GetFEMesh();
                    Logger.Instance.Tracer("Создание сетки в " + area + " подобласти");
                    // сложение подобластей
                    Mesh.Add(SMesh);
                }
                // Перенумерация сетки
                if (meshData.reNumberation == 1 && renumberator != null)
                {
                    IFEMesh NewMesh = null;
                    renumberator.FrontRenumberation(ref NewMesh, Mesh, meshData.direction);
                    Mesh = NewMesh;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
                Logger.Instance.Error("Ошибка при генерации КЭ сетки", "IFEMesh.CreateFEMesh()");
                Mesh = null;
            }
            return Mesh;
        }
        /// <summary>
        /// Преобразователь типа треугольной сетки 
        /// от типа TriangleNet.Mesh mesh к типу CommonLib.IMesh
        /// с фронтальной перенумерацией сетки 
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public void FrontRenumberation(ref IFEMesh NewMesh, IFEMesh mesh, Direction direction = Direction.toRight)
        {
            int ix, iy, jy;
            double MaxX = double.MinValue;
            double MinX = double.MaxValue;
            double MaxY = double.MinValue;
            double MinY = double.MaxValue;
            mesh.MinMax(0, ref MinX, ref MaxX);
            mesh.MinMax(1, ref MinY, ref MaxY);
            double[] X = mesh.GetCoords(0);
            double[] Y = mesh.GetCoords(1);
            // Подготовка контейнера хеш таблицы
            int CountKnots = mesh.CountKnots;
            int CountHash = CountKnots;
            if (CountKnots>1000)
                CountHash = (int)Math.Sqrt(CountKnots);
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
            NewMesh = new FEMesh(mesh);
            for (int i = 0; i < CountKnots; i++)
            {
                int OldKnot = i;
                int NewKnot = NewNumber[OldKnot];
                // координаты
                NewMesh.CoordsX[NewKnot] = X[i];
                NewMesh.CoordsY[NewKnot] = Y[i];
            }
            for (int i = 0; i < NewMesh.BNods.Length; i++)
            {
                int OldID = NewMesh.BNods[i].ID;
                NewMesh.BNods[i].ID = NewNumber[OldID];
                //Console.WriteLine("{2} old {0} new {1}", OldID, NewNumber[OldID], i);
            }
            for (int e = 0; e < NewMesh.CountElements; e++)
            {
                for (int i = 0; i < NewMesh.AreaElems[e].Length; i++)
                {
                    int OldID = NewMesh.AreaElems[e][i].ID;
                    NewMesh.AreaElems[e][i].ID = NewNumber[OldID];
                }
            }
            for (int e = 0; e < NewMesh.CountBoundElements; e++)
            {
                for (int i = 0; i < NewMesh.BoundElems[e].Length; i++)
                {
                    int OldID = NewMesh.BoundElems[e][i].ID;
                    NewMesh.BoundElems[e][i].ID = NewNumber[OldID];
                }
            }
        }

        /// <summary>
        /// поиск корректного расположения узлов базовой сетки на границах сегментов
        /// </summary>
        protected void LookingCorrSegmKnot()
        {
            try
            {
                int MaxKnot = 0;
                int MinKnot = 0;
                InfoMesh.SegmentNods.Clear();
                // Если флаг разбиения - средний диаметр
                if (meshData.flagMidle == true)
                    InfoMesh.SetMidleDiam(meshData.diametrFE.x);
                //  Разбиение сегментов на узлы
                InfoMesh.GetMaxKnot(meshRange, ref MaxKnot, ref MinKnot);
                // анализ полученного разбиения
                if (meshData.meshType == TypeMesh.Rectangle)
                {
                    int maxSegmenOnRib = 1;
                    // цикл по пдобластям
                    for (int area = 0; area < CountAreas; area++)
                    {
                        int CountRib = InfoMesh.NumAreas[area].NumRibs.Count;
                        // цикл по ребрам подобласти
                        for (int rib = 0; rib < CountRib; rib++)
                        {
                            int CountSegm = InfoMesh.NumAreas[area].NumRibs[rib].IndexSegments.Count;
                            if (maxSegmenOnRib < CountSegm)
                                maxSegmenOnRib = CountSegm;
                        }
                    }
                    int CMinKnot = MinKnot / maxSegmenOnRib;
                    int ElemMin = (int)(CMinKnot /(int)meshRange)+1;
                    int[] CountSeg = new int[maxSegmenOnRib];
                    for(int s = 0; s < maxSegmenOnRib; s++)
                        CountSeg[s] = (maxSegmenOnRib - s) * ElemMin * (int)meshRange + 1;
                    for (int area = 0; area < CountAreas; area++)
                    {
                        int CountRib = InfoMesh.NumAreas[area].NumRibs.Count;
                        // цикл по ребрам подобласти
                        for (int rib = 0; rib < CountRib; rib++)
                        {
                            int CountSegm = InfoMesh.NumAreas[area].NumRibs[rib].IndexSegments.Count;
                            for (int sg = 0; sg < CountSegm; sg++)
                            {
                                int IndexSegm = InfoMesh.NumAreas[area].NumRibs[rib].IndexSegments[sg];
                                InfoMesh.SegmentNods[IndexSegm] = CountSeg[CountSegm-1];
                                //RNods += InfoMesh.SegmentNods[IndexSegm].ID;
                            }
                        }
                    }
                        

                    //int MediaKnot = (MaxKnot + MinKnot) / 2;
                    //MediaKnot /= (int)meshRange;
                    //if (MediaKnot == 0)
                    //    MediaKnot = (int)meshRange + 1;
                    //else
                    //    MediaKnot = (int)meshRange * MediaKnot + 1;
                    //for (int Segm = 0; Segm < InfoMesh.SegmentNods.Count; Segm++)
                    //    InfoMesh.SegmentNods[Segm] = MediaKnot;
                    //InfoMesh.SegmentNods[Segm].ID = MediaKnot;
                }
                else // смешанная сетка конечных элементов
                {
                    for (uint i = 0; i < (MaxKnot - MinKnot) / (uint)meshRange; i++)
                    {
                        bool KeyBreak = true;
                        // цикл по подобластям
                        for (int area = 0; area < CountAreas; area++)
                        {
                            int CountRib = InfoMesh.NumAreas[area].NumRibs.Count;
                            // Отсечка изолиний
                            if (CountRib == 1) continue;
                            // количество узлов на ребрах подобласти
                            int[] Nods = new int[CountRib];
                            // цикл по ребрам подобласти
                            for (int rib = 0; rib < CountRib; rib++)
                            {
                                int RNods = 0;
                                int CountSegm = InfoMesh.NumAreas[area].NumRibs[rib].IndexSegments.Count;
                                for (int sg = 0; sg < CountSegm; sg++)
                                {
                                    int IndexSegm = InfoMesh.NumAreas[area].NumRibs[rib].IndexSegments[sg];
                                    RNods += InfoMesh.SegmentNods[IndexSegm];
                                    //RNods += InfoMesh.SegmentNods[IndexSegm].ID;
                                }
                                // определение количества узлов на ребре подобласти
                                Nods[rib] = RNods - CountSegm;
                            }
                            // для четырехгранной подобласти
                            if (CountRib == 4)
                            {
                                for (int rib = 0; rib < CountRib; rib++)
                                {
                                    int VKnots = Nods[rib] + Nods[(rib + 1) % CountRib];
                                    int MKnots = Nods[(rib + 2) % CountRib];
                                    if (VKnots <= MKnots)
                                    {
                                        // Введение поправки
                                        int Idx = InfoMesh.NumAreas[area].NumRibs[(rib + 2) % CountRib].Increment();
                                        int IndexSegm = InfoMesh.NumAreas[area].NumRibs[(rib + 2) % CountRib].IndexSegments[Idx];
                                        InfoMesh.SegmentNods[IndexSegm] -= (int)meshRange;
                                        //InfoMesh.SegmentNods[IndexSegm].ID -= (int)meshRange;
                                        if (Nods[rib] < Nods[(rib + 1) % CountRib])
                                        {
                                            Idx = InfoMesh.NumAreas[area].NumRibs[rib].Increment();
                                            IndexSegm = InfoMesh.NumAreas[area].NumRibs[rib].IndexSegments[Idx];
                                            InfoMesh.SegmentNods[IndexSegm] += (int)meshRange;
                                            //InfoMesh.SegmentNods[IndexSegm].ID += (int)meshRange;
                                        }
                                        else
                                        {
                                            Idx = InfoMesh.NumAreas[area].NumRibs[(rib + 1) % CountRib].Increment();
                                            IndexSegm = InfoMesh.NumAreas[area].NumRibs[(rib + 1) % CountRib].IndexSegments[Idx];
                                            InfoMesh.SegmentNods[IndexSegm] += (int)meshRange;
                                            //InfoMesh.SegmentNods[IndexSegm].ID += (int)meshRange;
                                        }
                                        KeyBreak = false;
                                    }
                                }
                            }
                        }
                        if (KeyBreak == true) break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
                Logger.Instance.Error("Ошибка в генераторе КЭ сетки", "LookingCorrSegmKnot");
            }
        }
        
        /// <summary>
        /// подготовка узловых карт области (определение размерностей для текущей подобласти)
        /// </summary>
        protected void SetMapsForRun()
        {
            Left.Clear();
            Right.Clear();
            //for (int i = 0; i < NRib; i++)
            //    for (int j = 1; j < NCfg; j++)
            //        CfgMap[i][j] = 0;
            
            // доопределение рабочих параметров
            //for (int i = 0; i < NRib; i++)
            //    CfgMap[i][0] = KnotRibs[i];
            //---------------------------------
            // определение характерных размеров карт
            //         X 
            //    |----> - направления обхода в циклах
            //    |
            //  Y V
            //   |---N2---|      MaxNodeX = max(N1,N3)
            //   |        |      MaxNodeY = max(N0,N2)
            //   N3       N1
            //   |        |
            //   |---N0---|
            //
            // максимальное количество узлов по Y
            //MaxNodeY = Math.Max(CfgMap[1][0], CfgMap[3][0]);
            MaxNodeY = Math.Max(KnotRibs[1], KnotRibs[3]);
            // максимальное количество узлов по Х
            MaxNodeX = Math.Max(KnotRibs[0], KnotRibs[2]);
            // выделение памяти по карты
            MEM.Alloc2D(MaxNodeY, MaxNodeX, ref pMap);
            MEM.Alloc2D(MaxNodeY, MaxNodeX, ref Map);
            for (int i = 0; i < MaxNodeY; i++)
                for (int j = 0; j < MaxNodeX; j++)
                    Map[i][j] = -1;
            //  доопределение конфигурации 3-x или 4-x сегментной
            //          конечно-элементной карты
            //
            //          |--- количество узлов на k - й грани
            //          |              |--- сдвижка начала k- й грани в КЭ карте по
            //          |              |    направлению предыдущей грани
            //          |              |         |--- недовыбранные узлы на текущей
            //          |              |         |               k- й грани
            //          V              V         V
            //  CfgMap[MaxNodeX[k] , start[k], end[k]]
            //
            // определение количество недовыбранных узлов на гранях
            
            int[] MapEnd = { 0, 0, 0, 0 };
            MapEnd[0] = MaxNodeX - KnotRibs[0];
            MapEnd[1] = MaxNodeY - KnotRibs[1];
            MapEnd[2] = MaxNodeX - KnotRibs[2];
            MapEnd[3] = MaxNodeY - KnotRibs[3];

            //CfgMap[0][2] = MaxNodeX - CfgMap[0][0];
            //CfgMap[1][2] = MaxNodeY - CfgMap[1][0];
            //CfgMap[2][2] = MaxNodeX - CfgMap[2][0];
            //CfgMap[3][2] = MaxNodeY - CfgMap[3][0];
            // определение характерных размеров карт
            //         X 
            //    |----> - направления обхода в циклах
            //    |
            //  Y V
            //   |---N2---|      MaxNodeX = max(N1,N3)
            //   |        |      MaxNodeY = max(N0,N2)
            //   N3       N1
            //   |        |
            //   |---N0---|
            //
            int[] MapStart = { 0, 0, 0, 0 };
            MapStart[0] = MapEnd[3];
            MapStart[1] = MapEnd[0];
            MapStart[2] = MapEnd[1];
            MapStart[3] = MapEnd[2];

            //CfgMap[0][1] = CfgMap[3][2];
            //CfgMap[1][1] = CfgMap[0][2];
            //CfgMap[2][1] = CfgMap[1][2];
            //CfgMap[3][1] = CfgMap[2][2];
            // определение левой и правой границы КЭ карты
            for (int i = 0; i < MaxNodeY; i++)
            {
                // левая боковая стенка
                int lft = 0;
                int rht = MaxNodeX;
                // верх
                if ((MapStart[3] - i) > 0)
                    lft = MapEnd[2] - i;
                // низ
                if ((i - KnotRibs[3] + 1) > 0)
                    lft = i - KnotRibs[3] + 1;
                //
                if (MapStart[2] - i > 0)
                    rht = MaxNodeX - (MapStart[2] - i);   // верх
                if (i - (MaxNodeY - MapEnd[0]) >= 0)
                    rht = MaxNodeX - (i - (MaxNodeY - MapEnd[0])) - 1;     // низ
                                                                              //
                Left.Add(lft);
                Right.Add(rht);
            }
            // формирование КЭ карты
            int Num = 0;
            for (int i = 0; i < MaxNodeY; i++)
                for (int j = Left[i]; j < Right[i]; j++)
                    Map[i][j] = Num++;
            //
            // int NumPoHInt=Num;
            //
            // формирование ГУ для координатной карты pMap
            //
            // инициализируем "координатную" КЭ карту
            VMapKnot KnotInit = RealSegmRibs[0].Knots[0];
            for (int i = 0; i < MaxNodeY; i++)
                for (int m = 0; m < MaxNodeX; m++)
                    pMap[i][m] = new VMapKnot(KnotInit);
            // прописываем граничные условия
            for (int rib = 0; rib < NRib; rib++) // цикл по ребрам
            {
                for (int knot = 0; knot < KnotRibs[rib]; knot++)
                {
                    int i = Calk_I(rib, knot);
                    int j = Calk_J(rib, knot);
                    VMapKnot Ka = pMap[i][j];
                    VMapKnot Kb = RealSegmRibs[rib].Knots[knot];
                    //pMap[i][j] = Kb.SelectBC(Ka);
                    pMap[i][j] = new VMapKnot(Kb);
                    pMap[i][j].marker = RealSegmRibs[rib].Knots[knot].marker;
                    pMap[i][j].typeEx = (int)RealSegmRibs[rib].Knots[knot].typeEx;
                }
            }
        }
        /// <summary>
        /// формирование узловых карт ...
        /// </summary>
        /// <param name="rib"></param>
        /// <param name="knot"></param>
        /// <returns></returns>
        int Calk_I(int rib, int knot)
        {
            int index = 0;
            // определение характерных размеров карт
            //
            //  |--> - направления обхода в циклах
            //  V
            //   |----2---|
            //   |        |
            //   3   rib  1
            //   |        |
            //   *---0----|
            //
            //   |---N3---|      MaxNodeX = max(N2,N4)
            //   |        |      MaxNodeY = max(N1,N3)
            //   N4       N2
            //   |        |
            //   |---N1---|
            //
            switch (rib)
            {
                case 0:
                    for (int i = MaxNodeY - 1; i > -1; i--)
                        if (Map[i][knot] > -1)
                        {
                            index = i;
                            break;
                        }
                    break;
                case 1:
                    index = MaxNodeY - 1 - knot;
                    break;
                case 2:
                    for (int i = 0; i < MaxNodeY; i++)
                        if (Map[i][MaxNodeX - knot - 1] > -1)
                        {
                            index = i;
                            break;
                        }
                    break;
                case 3:
                    index = knot;
                    break;
            }
            return index;
        }
        //---------------------------------------------------------------------------
        /// <summary>
        /// формирование узловых карт ...
        /// </summary>
        /// <param name="rib"></param>
        /// <param name="knot"></param>
        /// <returns></returns>
        int Calk_J(int rib, int knot)
        {
            int index = 0;
            switch (rib)
            {
                case 0:
                    index = knot;
                    break;
                case 1:
                    for (int i = MaxNodeX - 1; i > -1; i--)
                        if (Map[MaxNodeY - knot - 1][i] > -1)
                        {
                            index = i; break;
                        }
                    break;
                case 2:
                    index = MaxNodeX - knot - 1;
                    break;
                case 3:
                    for (int i = 0; i < MaxNodeX; i++)
                        if (Map[knot][i] > -1)
                        {
                            index = i;
                            break;
                        }
                    break;
            }
            return index;
        }
        /// <summary>
        /// Список строителей
        /// </summary>
        /// <returns></returns>
        public List<TaskMetka> GetBuilderNames()
        {
            return (new ManagerMeshBuilder()).GetStreamName();
        }
        /// <summary>
        /// получить строителя
        /// </summary>
        public IMeshBuilder Clone(int id)
        {
            return (new ManagerMeshBuilder()).Clone(id);
        }
    }
}
