//---------------------------------------------------------------------------
//                         проектировщик:
//                           Потапов И.И.
//                  - (C) Copyright 2000-2003
//                      ALL RIGHT RESERVED
//---------------------------------------------------------------------------
//                 кодировка : 1.02.2003 Потапов И.И.
//               ПРОЕКТ  "MixTasker' на базе "DISER"
//---------------------------------------------------------------------------
//  Определение класса HBaseMeshGener - абстрактного класса
//  определяющего интерфейс генераторов базисной конечно элементной сетки
//---------------------------------------------------------------------------
//              Перенос на C# : 05.03.2021  Потапов И.И.
//                     упрощенный вариант, убрана
//          вся механика связанная со стыковкой подобластей, 
//      и классы описания подобластей с сигментированными границами 
//                      убран 4 порядок сетки 
//---------------------------------------------------------------------------
namespace MeshGeneratorsLib
{
    using CommonLib;
    using MeshLib;
    using System.Collections.Generic;
    using System.Linq;
    using GeometryLib;
    public class HBaseMeshGener //: IMeshGenerator
    {
        ///// <summary>
        ///// флаг генерации конечно - элементной сетки со средним диаметром разбиения
        ///// </summary>
        //uint FlagMidle;
        ///// <summary>
        ///// Средний диаметр разбиения области
        ///// </summary>
        //double MidleDFE;
        /// <summary>
        /// Реалаксация ортогонализации
        /// </summary>
        double RelaxMeshOrthogonality = 0.1;
        ///// <summary>
        ///// тип КЭ сетки
        ///// </summary>
        //TypeMesh TypeMesh;
        /// <summary>
        /// Метод генерации конечно элементной сетки
        /// </summary>
        // int MeshMethod = 1;
        //// Перенумерация узлов в базовой КЭ сетке
        //uint ReNumberation;
        //// Запуск окна генерации КЭ сетки
        //bool StartForm;
        //// Индекс модификатора генерации КЭ сетки (зависит от вида генератора)
        //uint ModMesh;
        /// <summary>
        /// макс. количество ребер в подобласти
        /// </summary>
        protected const uint NRib = 4;
        protected const uint NCfg = 3;
        /// <summary>
        /// Описание подобластей
        /// </summary>
        HSubAreaMap Area;
        /// <summary>
        /// Сетка
        /// </summary>
        ComplecsMesh mesh = null;
        #region данные для генерации КЭ сетки в одной подобласти
        /// <summary>
        /// карта номеров узлов
        /// </summary>
        int[][] Map;
        /// <summary>
        /// максимальный размер карты по х и у
        /// </summary>
        int MaxNodeX, MaxNodeY;
        /// <summary>
        /// порядок КЭ, используемых при генерации базисной КЭ сетки
        /// </summary>
        TypeRangeMesh meshRange;
        /// <summary>
        /// конфигуратор карт
        /// </summary>
        int[][] CfgMap;
        /// <summary>
        /// количество узлов на границах подобласти
        /// </summary>
        int[] KnotRibs = new int[NRib];
        /// <summary>
        /// количество КЭ - в на границах подобласти
        /// </summary>
        int[] FElemRibs = new int[NRib];
        /// <summary>
        /// граничные сегменты после разбиения и ориентировки
        /// </summary>
        HMapSegment[] RealSegmRibs = new HMapSegment[NRib];
        // количество ребер
        int CRibs;
        /// <summary>
        /// количество узлов на границах подобласти
        /// </summary>
        List<uint> SegmentKnots = new List<uint>();
        /// <summary>
        /// левая граница области
        /// </summary>
        List<int> Left = new List<int>();
        /// <summary>
        /// правая граница области
        /// </summary>
        List<int> Right = new List<int>();
        // карта координат узлов
        VMapKnot[][] pMap;
        #endregion 
        public HBaseMeshGener(HSubAreaMap Area)
        {
            SetSubAres(Area);
        }
        public void SetSubAres(HSubAreaMap Area)
        {
            this.Area = Area;
            this.meshRange = Area.meshData.meshRange;
            mesh = new ComplecsMesh();
            CfgMap = new int[NRib][];
            for (int i = 0; i < 4; i++)
                CfgMap[i] = new int[4];
        }
        public IMesh CreateMesh()
        {
            Run();
            return mesh;
        }
        //  Генерация б.к.э. сетки
        void Run()
        {
            // поиск корректного расположения узлов базовой сетки
            // на границах сегментов и вычисление их координат
            LookingCorrSegmKnot();
            //// установка данных для генерации в подобласти
            //// и определение ориентации сегментов подобласти
            OrientationSegment();
            //// генерация КЭ сетки в подобласти
            DoMesh();
            //// сложение подобластей
            //Mesh[0] += SMesh;
            // вычисление принадлежности граничных КЭ к КЭ области
            // Mesh.CalkFEForBound();
            // Перенумерация сетки
            //Mesh.Renumberation(TData->ReNumberation );
        }
        void DoMesh()
        {
            // подготовка массивов и данных
            SetMapsForRun();
            // вычисление координат узловых точек и свойств
            // определенных в них
            FindCoords();
            // вычисление основных массивов связности сетки
            BuildMesh();
        }
        //  Генерация координат узлов б.к.э. сетки
        public void FindCoords()
        {
            HDiffCalcCoords FCoords = new HDiffCalcCoords(MaxNodeX, MaxNodeY, Left, Right, pMap, RelaxMeshOrthogonality);
            FCoords.Solve();
            // ===================================================
            // Запись полученных координат и параметров
            // ===================================================
            List<double> x = new List<double>();
            List<double> y = new List<double>();
            for (int i = 0; i < MaxNodeY; i++)
                for (int j = Left[i]; j < Right[i]; j++)
                {
                    x.Add(pMap[i][j].x);
                    y.Add(pMap[i][j].y);
                }
            mesh.CoordsX = x.ToArray();
            mesh.CoordsY = y.ToArray();
        }

        void SetMapsForRun()
        {
            //int NRib = Area.Contur.Length;
            //// конфигуратор карт
            //int[][] CfgMap = new int[NRib][];
            // доопределение рабочих параметров
            for (int i = 0; i < NRib; i++)
                CfgMap[i][0] = KnotRibs[i];
            //---------------------------------
            // определение характерных размеров карт
            //
            //  |--> - направления обхода в циклах
            //  V
            //   |---N3---|      MaxNodeX = max(N2,N4)
            //   |        |      MaxNodeY = max(N1,N3)
            //   N4       N2
            //   |        |
            //   |---N1---|
            //
            // максимальное количество узлов по Y
            if (CfgMap[1][0] < CfgMap[3][0])
                MaxNodeY = CfgMap[3][0];
            else
                MaxNodeY = CfgMap[1][0];
            // максимальное количество узлов по Х
            if (CfgMap[0][0] < CfgMap[2][0])
                MaxNodeX = CfgMap[2][0];
            else
                MaxNodeX = CfgMap[0][0];
            // выделение памяти по карты
            pMap = new VMapKnot[MaxNodeY][];
            for (int i = 0; i < MaxNodeY; i++)
                pMap[i] = new VMapKnot[MaxNodeX];
            Map = new int[MaxNodeY][];
            for (int i = 0; i < MaxNodeY; i++)
            {
                Map[i] = new int[MaxNodeX];
                for (int j = 0; j < MaxNodeX; j++)
                    Map[i][j] = -1;
            }
            //  доопределение конфигурации 3-x или 4-x сегментной
            //          конечно-элементной карты
            //
            //          |--- количество узлов на k - й грани
            //          |               |--- сдвижка начала k- й грани в КЭ карте по
            //          |               |      направлению предыдущей грани
            //          |               |        |--- недовыбранные узлы на текущей
            //          |               |        |               k- й грани
            //          V               V        V
            //  CfgMap[MaxNodeX(k) , start(k), end(k)]
            //
            // определение количество недовыбранных узлов на гранях
            CfgMap[0][2] = MaxNodeX - CfgMap[0][0];
            CfgMap[1][2] = MaxNodeY - CfgMap[1][0];
            CfgMap[2][2] = MaxNodeX - CfgMap[2][0];
            CfgMap[3][2] = MaxNodeY - CfgMap[3][0];
            //
            CfgMap[0][1] = CfgMap[3][2];
            CfgMap[1][1] = CfgMap[0][2];
            CfgMap[2][1] = CfgMap[1][2];
            CfgMap[3][1] = CfgMap[2][2];
            Left.Clear();
            Right.Clear();
            // определение индексов левой и правой границы КЭ карты
            for (int i = 0; i < MaxNodeY; i++)
            {
                // левая боковая стенка
                int lft = 0;
                int rht = MaxNodeX;
                // верх
                if ((CfgMap[3][1] - i) > 0)
                    lft = CfgMap[2][2] - i;
                // низ
                if ((i - CfgMap[3][0] + 1) > 0)
                    lft = i - CfgMap[3][0] + 1;
                //
                if (CfgMap[2][1] - i > 0)
                    rht = MaxNodeX - (CfgMap[2][1] - i);   // верх
                if (i - (MaxNodeY - CfgMap[0][2]) >= 0)
                    rht = MaxNodeX - (i - (MaxNodeY - CfgMap[0][2])) - 1;     // низ
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
                for (int knot = 0; knot < CfgMap[rib][0]; knot++)
                {
                    int i = Calk_I(rib, knot);
                    int j = Calk_J(rib, knot);
                    VMapKnot Ka = pMap[i][j];
                    VMapKnot Kb = RealSegmRibs[rib].Knots[knot];
                    pMap[i][j] = Kb.SelectBC(Ka);
                }
            }
            //for (int i = 0; i < MaxNodeY; i++)
            //{
            //    for (int m = 0; m < MaxNodeX; m++)
            //        Console.Write(" "+pMap[i][m].x.ToString());
            //    Console.WriteLine();
            //}
            //Console.WriteLine();
            //for (int i = 0; i < MaxNodeY; i++)
            //{
            //    for (int m = 0; m < MaxNodeX; m++)
            //        Console.Write(" " + pMap[i][m].y.ToString());
            //    Console.WriteLine();
            //}
        }
        // вычисление узлов границ
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
        void LookingCorrSegmKnot()
        {
            try
            {
                SegmentKnots.Clear();
                //// цикл по подобластям
                //for (uint area = 0; area < CountAreas; area++)
                //{
                //    HInt CountRib = InfoMesh.NumAreas[area].NumRibs.size();
                //  Разбиение сегментов на узлы
                uint Knots = Area.ContsKnots[0];
                SegmentKnots.Add(Knots);
                uint MaxKnot = Knots;
                uint MinKnot = Knots;
                for (uint Segm = 1; Segm < Area.Contur.Length; Segm++)
                {
                    Knots = Area.ContsKnots[Segm];
                    SegmentKnots.Add(Knots);
                    if (MinKnot > Knots) MinKnot = Knots;
                    if (MaxKnot < Knots) MaxKnot = Knots;
                }

                // анализ полученного разбиения
                if (Area.Contur.Length == 3)
                {
                    uint MediaKnot = (MaxKnot + MinKnot) / 2;
                    for (int Segm = 0; Segm < SegmentKnots.Count; Segm++)
                        SegmentKnots[Segm] = MediaKnot;
                }
                else
                {
                    //int M, N, m, n;
                    // уиел по поджатию граней
                    for (uint i = 0; i < (MaxKnot - MinKnot) / (uint)meshRange; i++)
                    {
                        bool KeyBreak = true;
                        // цикл по подобластям
                        // uint CountAreas;
                        //for (uint area = 0; area < CountAreas; area++)
                        //{
                        // количество узлов на ребрах подобласти
                        uint[] Nods = new uint[4];
                        // цикл по ребрам подобласти
                        for (int rib = 0; rib < Area.Contur.Length; rib++)
                        {
                            Nods[rib] = Area.ContsKnots[rib];
                        }
                        // для четырехгранной подобласти
                        for (int rib = 0; rib < 4; rib++)
                        {
                            // суммарное количество опорных узловых точек для текуших граней
                            uint VKnots = Nods[rib] + Nods[(rib + 1) % 4];
                            // количество на противолежащем сегменте
                            uint MKnots = Nods[(rib + 2) % 4];
                            if (VKnots <= MKnots)
                            {
                                if (Nods[rib] < Nods[(rib + 1) % 4])
                                {
                                    //uint Idx = InfoMesh.NumAreas[area].NumRibs[rib].Increment();
                                    //uint IndexSegm = InfoMesh.NumAreas[area].NumRibs[rib].IndexSegments[Idx];
                                    SegmentKnots[rib] += (uint)meshRange;
                                }
                                else
                                {
                                    //uint Idx = InfoMesh.NumAreas[area].NumRibs[(rib + 1) % 4].Increment();
                                    //uint IndexSegm = InfoMesh.NumAreas[area].NumRibs[(rib + 1) % 4].IndexSegments[Idx];
                                    SegmentKnots[rib + 1] += (uint)meshRange;
                                }
                                // сейчас только одна подобласть и нет нужды 
                                // согласовывать количество узорв на стаках сегментов
                                // 2003 Введение поправки 
                                //uint Idx =       InfoMesh.NumAreas[area].NumRibs[(rib + 2) % 4].Increment();
                                //uint IndexSegm = InfoMesh.NumAreas[area].NumRibs[(rib + 2) % 4].IndexSegments[Idx];
                                //SegmentKnots[IndexSegm] -= (uint)meshRange;
                                //if (Nods[rib] < Nods[(rib + 1) % 4])
                                //{
                                //    uint Idx = InfoMesh.NumAreas[area].NumRibs[rib].Increment();
                                //    uint IndexSegm = InfoMesh.NumAreas[area].NumRibs[rib].IndexSegments[Idx];
                                //    SegmentKnots[IndexSegm] += (uint)meshRange;
                                //}
                                //else
                                //{
                                //    uint Idx = InfoMesh.NumAreas[area].NumRibs[(rib + 1) % 4].Increment();
                                //    uint IndexSegm = InfoMesh.NumAreas[area].NumRibs[(rib + 1) % 4].IndexSegments[Idx];
                                //    SegmentKnots[IndexSegm] += (uint)meshRange;
                                //}
                                KeyBreak = false;
                            }
                        }
                        //}
                        if (KeyBreak == true) break;
                    }
                }
                //}
            }
            catch // (Exception aa)
            {
                //ShowMessage("Ошибка в генераторе КЭ сетки, метод: LookingCorrSegmKnot!");
            }
        }
        void OrientationSegment()
        {
            int CountRib = Area.Contur.Length;
            // 2021 только одна подобласть 
            for (int rib = 0; rib < CountRib; rib++)
            {
                // количество узлов на границах подобласти
                KnotRibs[rib] = (int)SegmentKnots[rib];
                // количество КЭ - в на границах подобласти
                FElemRibs[rib] = (int)(SegmentKnots[rib] - 1) / (int)meshRange;
                // граничные сегменты после разбиения и ориентировки
                RealSegmRibs[rib] = Area.Contur[rib].Stretching(KnotRibs[rib]);
            }
            // 2003
            // Подготовка данных для генерации сетки в текущей подобласти
            // очистка
            //for (uint i = 0; i < points.Length; i++)
            //{
            //    KnotRibs[i] = 0;           // количество узлов на границах подобласти
            //    FElemRibs[i] = 0;          // количество КЭ - в на границах подобласти
            //    RealSegmRibs[i].Clear(); // граничные сегменты после разбиения и ориентировки
            //}
            //uint CountRib = InfoMesh.NumAreas[area].NumRibs.size();
            //for (uint rib = 0; rib < CountRib; rib++)
            //{
            //    // количество узлов на границах подобласти
            //    KnotRibs[rib] = 0;
            //    uint CountSegm = InfoMesh.NumAreas[area].NumRibs[rib].IndexSegments.size();
            //    for (uint sg = 0; sg < CountSegm; sg++)
            //    {
            //        // индекс сегмента на грани
            //        uint IndexSegm = InfoMesh.NumAreas[area].NumRibs[rib].IndexSegments[sg];
            //        // количество узлов на грани
            //        uint RealKnots = SegmentKnots[IndexSegm];
            //        // сумарное количество узлов на грани
            //        KnotRibs[rib] += RealKnots;
            //        // суммарный (составной) сегмент для всей грани
            //        // с заданным количестом узлов
            //        RealSegmRibs[rib] = RealSegmRibs[rib] + InfoMesh.BaseSegments[IndexSegm].Stretching(RealKnots);
            //    }
            //    // количество КЭ - в на границах подобласти
            //    FElemRibs[rib] = (KnotRibs[rib] - CountSegm) / ((uint)meshRange);
            //    KnotRibs[rib] -= CountSegm - 1;
            //    // граничные сегменты после разбиения и ориентировки
            //}
            // Определение последовательности обхода координат КЭ по сегментам
            //  |--------|c
            //  |        |
            //  |        |   if(a!=b && a!=c) то a - начало сегмента
            //  |________|
            //  a         b
            //    заполняем массив - vectors
            VMapKnot a, b, c;
            for (int i = 0; i < CountRib; i++)
            {
                // координаты начала 1 - сегмента
                a = RealSegmRibs[i].Knots[0];
                // координаты начала и конца 2 - сегмента
                int Next = (i + 1) % CountRib;
                b = RealSegmRibs[Next].Knots[0];
                c = RealSegmRibs[Next].Knots[RealSegmRibs[Next].Knots.Count() - 1];
                if (!((a != b) && (a != c)))
                    RealSegmRibs[i].Reverse();
            }
            CRibs = CountRib;
        }
        /// <summary>
        /// формирование объекта   ( Mesh )
        /// </summary>
        /// <param name="area"></param>
        void BuildMesh()
        {
            // формирование массива соответствий
            int ii = 0, jj = 0;
            TypeFunForm ElemType = 0, DElemType = 0;
            List<int> Elem = new List<int>();
            List<int> DElem = new List<int>();
            List<int[]> Elements = new List<int[]>();
            List<TypeFunForm> TypeElements = new List<TypeFunForm>();
            // Определение массива граничных узлов
            CalcBoundKnot();
            mesh.tRangeMesh = meshRange;
            switch (meshRange) // порядок КЭ базы
            {
                case TypeRangeMesh.mRange1: // 1 порядок базовой сетки
                    {
                        // формирование массива соответствий
                        for (int i = 0; i < MaxNodeY - 1; i++)
                        {
                            for (int j = 0; j < MaxNodeX - 1; j++)
                            {
                                // создаем текущий элемент (или два :))
                                Elem.Clear();
                                DElem.Clear();
                                //HElemFE Elem, DElem;
                                // определяем тип элемента
                                int keyFE = TestFE(i, j, ref ii, ref jj);
                                switch (keyFE)
                                {
                                    case 0: // центр
                                        if (Area.meshData.meshType == TypeMesh.Rectangle ||
                                            Area.meshData.meshType == TypeMesh.MixMesh)
                                        //TypeMesh == mtTetrangleMesh || TypeMesh == mtMixMesh)
                                        {
                                            Elem.Add(Map[i + 1][j]);
                                            Elem.Add(Map[i + 1][j + 1]);
                                            Elem.Add(Map[i][j + 1]);
                                            Elem.Add(Map[i][j]);
                                            ElemType = TypeFunForm.Form_2D_Rectangle_L1;
                                        }
                                        else
                                        {
                                            if ((i % 2 == 1 && j % 2 == 0) || (i % 2 == 0 && j % 2 == 1))
                                            {
                                                // левый нижний трехугольный КЭ
                                                Elem.Add(Map[i + 1][j + 1]);
                                                Elem.Add(Map[i][j + 1]);
                                                Elem.Add(Map[i][j]);
                                                // правый верхний трехугольный КЭ
                                                DElem.Add(Map[i + 1][j]);
                                                DElem.Add(Map[i + 1][j + 1]);
                                                DElem.Add(Map[i][j]);
                                            }
                                            else
                                            {
                                                // правый нижний трехугольный КЭ
                                                Elem.Add(Map[i + 1][j]);
                                                Elem.Add(Map[i][j + 1]);
                                                Elem.Add(Map[i][j]);
                                                // левый верхний трехугольник
                                                DElem.Add(Map[i + 1][j]);
                                                DElem.Add(Map[i + 1][j + 1]);
                                                DElem.Add(Map[i][j + 1]);
                                            }
                                            ElemType = TypeFunForm.Form_2D_Triangle_L1;
                                            DElemType = TypeFunForm.Form_2D_Triangle_L1;
                                        }
                                        break;
                                    case 1: // 3 узловой КЭ слева внизу
                                        Elem.Add(Map[i + 1][j + 1]);
                                        Elem.Add(Map[i][j + 1]);
                                        Elem.Add(Map[i][j]);
                                        DElemType = TypeFunForm.Form_2D_Triangle_L1;
                                        break;
                                    case 2: // 3 узловой КЭ справа внизу
                                        Elem.Add(Map[i + 1][j]);
                                        Elem.Add(Map[i][j + 1]);
                                        Elem.Add(Map[i][j]);
                                        DElemType = TypeFunForm.Form_2D_Triangle_L1;
                                        break;
                                    case 3: // 3 узловой КЭ справа вверху
                                        Elem.Add(Map[i + 1][j]);
                                        Elem.Add(Map[i + 1][j + 1]);
                                        Elem.Add(Map[i][j]);
                                        DElemType = TypeFunForm.Form_2D_Triangle_L1;
                                        break;
                                    case 4: // 3 узловой КЭ слева вверху
                                        Elem.Add(Map[i + 1][j]);
                                        Elem.Add(Map[i + 1][j + 1]);
                                        Elem.Add(Map[i][j + 1]);
                                        DElemType = TypeFunForm.Form_2D_Triangle_L1;
                                        break;
                                }
                                // добавляем элемент в сетку

                                if (Elem.Count > 0)
                                {
                                    if (Elem[0] != -1 && Elem[1] != -1 || Elem[2] != -1)
                                    {
                                        Elements.Add(Elem.ToArray());
                                        TypeElements.Add(ElemType);
                                    }
                                }

                                // если определен парный КЭ добавляем и его
                                if (DElem.Count > 0)
                                {
                                    if (DElem[0] != -1 && DElem[1] != -1 || DElem[2] != -1)
                                    {
                                        Elements.Add(DElem.ToArray());
                                        TypeElements.Add(DElemType);
                                    }
                                }
                            }
                        }
                        //
                        mesh.AreaElems = new uint[Elements.Count][];
                        mesh.AreaElemsFFType = TypeElements.ToArray<TypeFunForm>();
                        for (int ik = 0; ik < mesh.AreaElems.Length; ik++)
                        {
                            mesh.AreaElems[ik] = new uint[Elements[ik].Length];
                            for (int i = 0; i < mesh.AreaElems[ik].Length; i++)
                                mesh.AreaElems[ik][i] = (uint)Elements[ik][i];
                        }
                        //// формирование граничных конечных элементов
                        //// количество граничных узлов
                        //int CountBKnot = mesh.BoundKnots.Length;
                        //// количество граничных КЭ - в
                        //int CountBFE = CountBKnot - 1;
                        //mesh.BoundElems = new uint[CountBFE][];
                        //mesh.BoundType = new uint[CountBFE];
                        //for (int ik = 0; ik < CountBFE; ik++)
                        //{
                        //    Elem.Clear();
                        //    mesh.BoundElems[ik] = new uint[2];
                        //    mesh.BoundElems[ik][0] = (uint)mesh.BoundKnots[ik];
                        //    mesh.BoundElems[ik][1] = (uint)mesh.BoundKnots[(ik + 1) % CountBKnot];
                        //    mesh.BoundType[ik] = (uint)TypeFunForm.Form_1D_L1;
                        //}
                    }
                    break;
                case TypeRangeMesh.mRange2: // 2 порядок
                    {

                        for (int i = 0; i < (MaxNodeY - 1) / 2; i++)
                        {
                            for (int j = 0; j < (MaxNodeX - 1) / 2; j++)
                            {
                                // создаем текущий элемент (или два :))
                                Elem.Clear();
                                DElem.Clear();
                                int keyFE = TestFE(i, j, ref ii, ref jj);
                                switch (keyFE)
                                {
                                    case -1:
                                        // пустой КЭ
                                        break;
                                    case 0: // центр - четырехугольный КЭ
                                        {
                                            if (Area.meshData.meshType == TypeMesh.Rectangle ||
                                            Area.meshData.meshType == TypeMesh.MixMesh)
                                            //    if (TypeMesh == mtTetrangleMesh || TypeMesh == mtMixMesh)
                                            {
                                                Elem.Add(Map[ii + 1][jj - 1]);
                                                Elem.Add(Map[ii + 1][jj]);
                                                Elem.Add(Map[ii + 1][jj + 1]);
                                                Elem.Add(Map[ii][jj + 1]);
                                                Elem.Add(Map[ii - 1][jj + 1]);
                                                Elem.Add(Map[ii - 1][jj]);
                                                Elem.Add(Map[ii - 1][jj - 1]);
                                                Elem.Add(Map[ii][jj - 1]);
                                                Elem.Add(Map[ii][jj]);
                                                ElemType = TypeFunForm.Form_2D_Rectangle_L2;
                                            }
                                            else
                                            {
                                                if ((i % 2 == 1 && j % 2 == 0) || (i % 2 == 0 && j % 2 == 1))
                                                {
                                                    // левый нижний трехугольный КЭ
                                                    Elem.Add(Map[ii - 1][jj - 1]);
                                                    Elem.Add(Map[ii][jj]);
                                                    Elem.Add(Map[ii + 1][jj + 1]);
                                                    Elem.Add(Map[ii][jj + 1]);
                                                    Elem.Add(Map[ii - 1][jj + 1]);
                                                    Elem.Add(Map[ii - 1][jj]);
                                                    // правый верхний трехугольный КЭ
                                                    DElem.Add(Map[ii + 1][jj - 1]);
                                                    DElem.Add(Map[ii + 1][jj]);
                                                    DElem.Add(Map[ii + 1][jj + 1]);
                                                    DElem.Add(Map[ii][jj]);
                                                    DElem.Add(Map[ii - 1][jj - 1]);
                                                    DElem.Add(Map[ii][jj - 1]);
                                                }
                                                else
                                                {
                                                    // правый нижний трехугольный КЭ
                                                    Elem.Add(Map[ii + 1][jj - 1]);
                                                    Elem.Add(Map[ii][jj]);
                                                    Elem.Add(Map[ii - 1][jj + 1]);
                                                    Elem.Add(Map[ii - 1][jj]);
                                                    Elem.Add(Map[ii - 1][jj - 1]);
                                                    Elem.Add(Map[ii][jj - 1]);
                                                    // левый верхний трехугольник
                                                    DElem.Add(Map[ii + 1][jj - 1]);
                                                    DElem.Add(Map[ii + 1][jj]);
                                                    DElem.Add(Map[ii + 1][jj + 1]);
                                                    DElem.Add(Map[ii][jj + 1]);
                                                    DElem.Add(Map[ii - 1][jj + 1]);
                                                    DElem.Add(Map[ii][jj]);
                                                }
                                                ElemType = TypeFunForm.Form_2D_Triangle_L2;
                                                DElemType = TypeFunForm.Form_2D_Triangle_L2;
                                            }
                                        }
                                        break;
                                    case 1: // левый нижний трехугольный КЭ
                                        {
                                            Elem.Add(Map[ii - 1][jj - 1]);
                                            Elem.Add(Map[ii][jj]);
                                            Elem.Add(Map[ii + 1][jj + 1]);
                                            Elem.Add(Map[ii][jj + 1]);
                                            Elem.Add(Map[ii - 1][jj + 1]);
                                            Elem.Add(Map[ii - 1][jj]);
                                            ElemType = TypeFunForm.Form_2D_Triangle_L2;
                                        }
                                        break;
                                    case 2: // правый нижний трехугольный КЭ
                                        {
                                            Elem.Add(Map[ii + 1][jj - 1]);
                                            Elem.Add(Map[ii][jj]);
                                            Elem.Add(Map[ii - 1][jj + 1]);
                                            Elem.Add(Map[ii - 1][jj]);
                                            Elem.Add(Map[ii - 1][jj - 1]);
                                            Elem.Add(Map[ii][jj - 1]);
                                            ElemType = TypeFunForm.Form_2D_Triangle_L2;
                                        }
                                        break;
                                    case 3: // правый верхний трехугольный КЭ
                                        {
                                            Elem.Add(Map[ii + 1][jj - 1]);
                                            Elem.Add(Map[ii + 1][jj]);
                                            Elem.Add(Map[ii + 1][jj + 1]);
                                            Elem.Add(Map[ii][jj]);
                                            Elem.Add(Map[ii - 1][jj - 1]);
                                            Elem.Add(Map[ii][jj - 1]);
                                            ElemType = TypeFunForm.Form_2D_Triangle_L2;
                                            break;
                                        }
                                    case 4: // левый верхний трехугольник
                                        {
                                            Elem.Add(Map[ii + 1][jj - 1]);
                                            Elem.Add(Map[ii + 1][jj]);
                                            Elem.Add(Map[ii + 1][jj + 1]);
                                            Elem.Add(Map[ii][jj + 1]);
                                            Elem.Add(Map[ii - 1][jj + 1]);
                                            Elem.Add(Map[ii][jj]);
                                            ElemType = TypeFunForm.Form_2D_Triangle_L2;
                                        }
                                        break;
                                }
                                // добавляем элемент в сетку
                                if (Elem.Count > 0)
                                {
                                    Elements.Add(Elem.ToArray());
                                    TypeElements.Add(ElemType);
                                }
                                // если определен парный КЭ добавляем и его
                                if (DElem.Count > 0)
                                {
                                    Elements.Add(DElem.ToArray());
                                    TypeElements.Add(DElemType);
                                }
                            }
                        }
                        mesh.AreaElems = new uint[Elements.Count][];
                        mesh.AreaElemsFFType = TypeElements.ToArray<TypeFunForm>();
                        for (int ik = 0; ik < mesh.AreaElems.Length; ik++)
                        {
                            mesh.AreaElems[ik] = new uint[Elements[ik].Length];
                            for (int i = 0; i < mesh.AreaElems[ik].Length; i++)
                                mesh.AreaElems[ik][i] = (uint)Elements[ik][i];
                        }
                        //// формирование граничных конечных элементов
                        //// количество граничных узлов
                        //int CountBKnot = mesh.BoundKnots.Length;
                        //// количество граничных КЭ - в
                        //int CountBFE = (CountBKnot) / 2;
                        //mesh.BoundElems = new uint[CountBFE][];
                        //mesh.BoundType = new uint[CountBFE];

                        //for (int ik = 0; ik < CountBFE; ik++)
                        //{
                        //    int IdxElem1 = 2 * ik;
                        //    int IdxElem2 = 2 * ik + 1;
                        //    int IdxElem3 = (2 * ik + 2) % CountBKnot;
                        //    mesh.BoundElems[ik] = new uint[3];
                        //    mesh.BoundElems[ik][0] = (uint)mesh.BoundKnots[IdxElem1];
                        //    mesh.BoundElems[ik][1] = (uint)mesh.BoundKnots[IdxElem2];
                        //    mesh.BoundElems[ik][2] = (uint)mesh.BoundKnots[IdxElem3];
                        //    mesh.BoundType[ik] = (uint)TypeFunForm.Form_1D_L2;
                        //}
                    }
                    break;
                case TypeRangeMesh.mRange3: // 3 порядок
                    {
                        // формирование массива соответствий
                        for (int i = 0; i < (MaxNodeY - 1) / 3; i++)
                        {
                            for (int j = 0; j < (MaxNodeX - 1) / 3; j++)
                            {
                                // создаем текущий элемент (или два :))
                                Elem.Clear();
                                DElem.Clear();
                                int keyFE = TestFE(i, j, ref ii, ref jj);
                                switch (keyFE)
                                {
                                    case -1:
                                        // пустой КЭ
                                        break;
                                    case 0: // центр - четырехугольный КЭ
                                        {
                                            if (Area.meshData.meshType == TypeMesh.Rectangle ||
                                            Area.meshData.meshType == TypeMesh.MixMesh)
                                            //if (TypeMesh == mtTetrangleMesh || TypeMesh == mtMixMesh)
                                            {
                                                Elem.Add(Map[ii + 2][jj - 1]);
                                                Elem.Add(Map[ii + 2][jj]);
                                                Elem.Add(Map[ii + 2][jj + 1]);
                                                Elem.Add(Map[ii + 2][jj + 2]);

                                                Elem.Add(Map[ii + 1][jj + 2]);
                                                Elem.Add(Map[ii][jj + 2]);
                                                Elem.Add(Map[ii - 1][jj + 2]);
                                                Elem.Add(Map[ii - 1][jj + 1]);

                                                Elem.Add(Map[ii - 1][jj]);
                                                Elem.Add(Map[ii - 1][jj - 1]);
                                                Elem.Add(Map[ii][jj - 1]);
                                                Elem.Add(Map[ii + 1][jj - 1]);

                                                Elem.Add(Map[ii + 1][jj]);
                                                Elem.Add(Map[ii + 1][jj + 1]);
                                                Elem.Add(Map[ii][jj + 1]);
                                                Elem.Add(Map[ii][jj]);
                                                ElemType = TypeFunForm.Form_2D_Rectangle_L3;
                                            }
                                            else
                                            {
                                                if ((i % 2 == 1 && j % 2 == 0) || (i % 2 == 0 && j % 2 == 1))
                                                {
                                                    // левый нижний трехугольный КЭ
                                                    Elem.Add(Map[ii - 1][jj - 1]);
                                                    Elem.Add(Map[ii][jj]);
                                                    Elem.Add(Map[ii + 1][jj + 1]);
                                                    Elem.Add(Map[ii + 2][jj + 2]);

                                                    Elem.Add(Map[ii + 1][jj + 2]);
                                                    Elem.Add(Map[ii][jj + 2]);
                                                    Elem.Add(Map[ii - 1][jj + 2]);
                                                    Elem.Add(Map[ii - 1][jj + 1]);

                                                    Elem.Add(Map[ii - 1][jj]);
                                                    Elem.Add(Map[ii][jj + 1]);
                                                    // правый верхний трехугольный КЭ
                                                    DElem.Add(Map[ii + 2][jj - 1]);
                                                    DElem.Add(Map[ii + 2][jj]);
                                                    DElem.Add(Map[ii + 2][jj + 1]);
                                                    DElem.Add(Map[ii + 2][jj + 2]);

                                                    DElem.Add(Map[ii + 1][jj + 1]);
                                                    DElem.Add(Map[ii][jj]);
                                                    DElem.Add(Map[ii - 1][jj - 1]);
                                                    DElem.Add(Map[ii][jj - 1]);

                                                    DElem.Add(Map[ii + 1][jj - 1]);
                                                    DElem.Add(Map[ii + 1][jj]);
                                                }
                                                else
                                                {
                                                    // правый нижний трехугольный КЭ
                                                    Elem.Add(Map[ii + 2][jj - 1]);
                                                    Elem.Add(Map[ii + 1][jj]);
                                                    Elem.Add(Map[ii][jj + 1]);
                                                    Elem.Add(Map[ii - 1][jj + 2]);

                                                    Elem.Add(Map[ii - 1][jj + 1]);
                                                    Elem.Add(Map[ii - 1][jj]);
                                                    Elem.Add(Map[ii - 1][jj - 1]);
                                                    Elem.Add(Map[ii][jj - 1]);

                                                    Elem.Add(Map[ii + 1][jj - 1]);
                                                    Elem.Add(Map[ii][jj]);
                                                    // левый верхний трехугольник
                                                    DElem.Add(Map[ii + 2][jj - 1]);
                                                    DElem.Add(Map[ii + 2][jj]);
                                                    DElem.Add(Map[ii + 2][jj + 1]);
                                                    DElem.Add(Map[ii + 2][jj + 2]);

                                                    DElem.Add(Map[ii + 1][jj + 2]);
                                                    DElem.Add(Map[ii][jj + 2]);
                                                    DElem.Add(Map[ii - 1][jj + 2]);
                                                    DElem.Add(Map[ii][jj + 1]);

                                                    DElem.Add(Map[ii + 1][jj]);
                                                    DElem.Add(Map[ii + 1][jj + 1]);
                                                }
                                                ElemType = TypeFunForm.Form_2D_Triangle_L3;
                                                DElemType = TypeFunForm.Form_2D_Triangle_L3;
                                            }
                                        }
                                        break;
                                    case 1:
                                        {
                                            // левый нижний трехугольный КЭ
                                            Elem.Add(Map[ii - 1][jj - 1]);
                                            Elem.Add(Map[ii][jj]);
                                            Elem.Add(Map[ii + 1][jj + 1]);
                                            Elem.Add(Map[ii + 2][jj + 2]);

                                            Elem.Add(Map[ii + 1][jj + 2]);
                                            Elem.Add(Map[ii][jj + 2]);
                                            Elem.Add(Map[ii - 1][jj + 2]);
                                            Elem.Add(Map[ii - 1][jj + 1]);

                                            Elem.Add(Map[ii - 1][jj]);
                                            Elem.Add(Map[ii][jj + 1]);
                                            ElemType = TypeFunForm.Form_2D_Triangle_L3;

                                        }
                                        break;
                                    case 2:
                                        {
                                            // правый нижний трехугольный КЭ
                                            Elem.Add(Map[ii + 2][jj - 1]);
                                            Elem.Add(Map[ii + 1][jj]);
                                            Elem.Add(Map[ii][jj + 1]);
                                            Elem.Add(Map[ii - 1][jj + 2]);

                                            Elem.Add(Map[ii - 1][jj + 1]);
                                            Elem.Add(Map[ii - 1][jj]);
                                            Elem.Add(Map[ii - 1][jj - 1]);
                                            Elem.Add(Map[ii][jj - 1]);

                                            Elem.Add(Map[ii + 1][jj - 1]);
                                            Elem.Add(Map[ii][jj]);
                                            ElemType = TypeFunForm.Form_2D_Triangle_L3;
                                        }
                                        break;
                                    case 3:
                                        {
                                            // правый верхний трехугольный КЭ
                                            Elem.Add(Map[ii + 2][jj - 1]);
                                            Elem.Add(Map[ii + 2][jj]);
                                            Elem.Add(Map[ii + 2][jj + 1]);
                                            Elem.Add(Map[ii + 2][jj + 2]);

                                            Elem.Add(Map[ii + 1][jj + 1]);
                                            Elem.Add(Map[ii][jj]);
                                            Elem.Add(Map[ii - 1][jj - 1]);
                                            Elem.Add(Map[ii][jj - 1]);

                                            Elem.Add(Map[ii + 1][jj - 1]);
                                            Elem.Add(Map[ii + 1][jj]);
                                            ElemType = TypeFunForm.Form_2D_Triangle_L3;
                                            break;
                                        }
                                    case 4:
                                        {
                                            // левый верхний трехугольник
                                            Elem.Add(Map[ii + 2][jj - 1]);
                                            Elem.Add(Map[ii + 2][jj]);
                                            Elem.Add(Map[ii + 2][jj + 1]);
                                            Elem.Add(Map[ii + 2][jj + 2]);

                                            Elem.Add(Map[ii + 1][jj + 2]);
                                            Elem.Add(Map[ii][jj + 2]);
                                            Elem.Add(Map[ii - 1][jj + 2]);
                                            Elem.Add(Map[ii][jj + 1]);

                                            Elem.Add(Map[ii + 1][jj]);
                                            Elem.Add(Map[ii + 1][jj + 1]);
                                            ElemType = TypeFunForm.Form_2D_Triangle_L3;
                                        }
                                        break;
                                }
                                // добавляем элемент в сетку
                                if (Elem.Count > 0)
                                {
                                    Elements.Add(Elem.ToArray());
                                    TypeElements.Add(ElemType);
                                }
                                // если определен парный КЭ добавляем и его
                                if (DElem.Count > 0)
                                {
                                    Elements.Add(DElem.ToArray());
                                    TypeElements.Add(DElemType);
                                }
                            }
                        }
                        mesh.AreaElems = new uint[Elements.Count][];
                        mesh.AreaElemsFFType = TypeElements.ToArray<TypeFunForm>();
                        for (int ik = 0; ik < mesh.AreaElems.Length; ik++)
                        {
                            mesh.AreaElems[ik] = new uint[Elements[ik].Length];
                            for (int i = 0; i < mesh.AreaElems[ik].Length; i++)
                                mesh.AreaElems[ik][i] = (uint)Elements[ik][i];
                        }
                        //// формирование граничных конечных элементов
                        //// количество граничных узлов
                        //int CountBKnot = mesh.BoundKnots.Length;
                        //// количество граничных КЭ - в
                        //int CountBFE = CountBKnot / 3;
                        //mesh.BoundElems = new uint[CountBFE][];
                        //    mesh.BoundType = new uint[CountBFE];
                        //for (int ik = 0; ik < CountBFE; ik++)
                        //{
                        //    int IdxElem1 = 3 * ik;
                        //    int IdxElem2 = 3 * ik + 1;
                        //    int IdxElem3 = 3 * ik + 2;
                        //    int IdxElem4 = (3 * ik + 3) % CountBKnot;

                        //    mesh.BoundElems[ik] = new uint[4];
                        //    mesh.BoundElems[ik][0] = (uint)mesh.BoundKnots[IdxElem1];
                        //    mesh.BoundElems[ik][1] = (uint)mesh.BoundKnots[IdxElem2];
                        //    mesh.BoundElems[ik][2] = (uint)mesh.BoundKnots[IdxElem3];
                        //    mesh.BoundElems[ik][3] = (uint)mesh.BoundKnots[IdxElem4];
                        //    mesh.BoundType[ik] = (uint)TypeFunForm.Form_1D_L3;
                        //}
                    }
                    break;
            }
            //for (int ik = 0; ik < mesh.AreaElems.Length; ik++)
            //{
            //    Console.Write(" " + ik.ToString());
            //    for (int i = 0; i < mesh.AreaElems[ik].Length; i++)
            //        Console.Write(" " + mesh.AreaElems[ik][i].ToString());
            //    Console.WriteLine();
            //}
        }
        /// <summary>
        /// Определение массива граничных узлов
        /// </summary>
        void CalcBoundKnot()
        {
            List<int> B = new List<int>();
            List<int> R = new List<int>();
            List<int> T = new List<int>();
            List<int> L = new List<int>();
            // количество граничных узлов
            int CountBN = Right[0] - Left[0] +
                          Right[MaxNodeY - 1] - Left[MaxNodeY - 1] +
                          Right.Count + Left.Count;

            mesh.BoundKnots = new int[CountBN];
            mesh.BoundKnotsMark = new int[CountBN];
            uint n = 0;
            // узлы границы против часовой стрелки
            // низ
            int i = MaxNodeY - 1;
            for (int j = Left[i]; j < Right[i]; j++)
            {
                mesh.BoundKnots[n] = Map[i][j];
                B.Add(mesh.BoundKnots[n]);
                mesh.BoundKnotsMark[n++] = pMap[i][j].marker; //    RealSegmRibs[0].MarkBC;
                // Console.Write(" " + mesh.BoundKnots[n].ToString());
            }
            // Console.WriteLine();
            // справа
            for (i = Right.Count - 1; i > -1; i--)
            {
                mesh.BoundKnots[n] = Map[i][Right[i] - 1];
                R.Add(mesh.BoundKnots[n]);
                mesh.BoundKnotsMark[n++] = pMap[i][Right[i] - 1].marker; // RealSegmRibs[1].MarkBC;
            }
            // Console.WriteLine();
            // верх
            i = 0;
            for (int j = Right[i] - 1; j > Left[i] - 1; j--)
            {
                mesh.BoundKnots[n] = Map[i][j];
                T.Add(mesh.BoundKnots[n]);
                mesh.BoundKnotsMark[n++] = pMap[i][j].marker; ;
                // Console.Write(" " + mesh.BoundKnots[n].ToString());
            }
            // Console.WriteLine();
            // слева
            for (i = 0; i < Left.Count; i++)
            {
                mesh.BoundKnots[n] = Map[i][Left[i]];
                L.Add(mesh.BoundKnots[n]);
                mesh.BoundKnotsMark[n++] = pMap[i][Left[i]].marker;
                // Console.Write(" " + mesh.BoundKnots[n].ToString());
            }
            // Console.WriteLine();
            uint ID = 0;
            if (meshRange == TypeRangeMesh.mRange1)
                ID = (uint)TypeFunForm.Form_1D_L1;
            if (meshRange == TypeRangeMesh.mRange2)
                ID = (uint)TypeFunForm.Form_1D_L2;
            if (meshRange == TypeRangeMesh.mRange3)
                ID = (uint)TypeFunForm.Form_1D_L3;
            int cs = (int)meshRange;
            // количество граничных КЭ - в
            int CountBFE = (B.Count + R.Count + T.Count + L.Count - 4) / (int)meshRange;
            mesh.BoundElems = new uint[CountBFE][];
            mesh.BoundElementsMark = new int[CountBFE];
            int elem = 0;
            CreateBE(B.ToArray(), cs, ID, ref elem, 0);
            CreateBE(R.ToArray(), cs, ID, ref elem, 1);
            CreateBE(T.ToArray(), cs, ID, ref elem, 2);
            CreateBE(L.ToArray(), cs, ID, ref elem, 3);
            // Console.WriteLine();
        }
        public void CreateBE(int[] M, int cs, uint ID, ref int elem, int SID)
        {
            int Count = (M.Length - 1) / (int)meshRange;
            for (int be = 0; be < Count; be++)
            {
                // Console.WriteLine();
                mesh.BoundElems[elem] = new uint[cs + 1];
                for (int s = 0; s < cs + 1; s++)
                {
                    mesh.BoundElems[elem][s] = (uint)M[be * cs + s];
                    // Console.Write(" " + mesh.BoundElems[elem][s].ToString());
                }
                mesh.BoundElementsMark[elem] = RealSegmRibs[SID].MarkBC[elem];
                elem++;
            }
        }



        /// <summary>
        /// можно создать инкрементный массив зависящий от range и код будет "проще" !
        /// </summary>
        /// <param name="i">КЭ</param>
        /// <param name="j">КЭ</param>
        /// <param name="ii">центр КЭ в мапе</param>
        /// <param name="jj">центр КЭ в мапе</param>
        /// <returns></returns>
        int TestFE(int i, int j, ref int ii, ref int jj)
        {
            int Type = 0;
            // определение типа КЭ
            switch (meshRange)
            {
                case TypeRangeMesh.mRange1:
                    ii = i;
                    jj = j;
                    if (Map[ii][jj] == -1 && Map[ii][jj + 1] == -1 &&
                       Map[ii + 1][jj] == -1 && Map[ii + 1][jj + 1] == -1) Type = -1;
                    else
                    if (Map[ii][jj] != -1 && Map[ii][jj + 1] != -1 &&
                       Map[ii + 1][jj] != -1 && Map[ii + 1][jj + 1] != -1) Type = 0;
                    else
                    if (Map[ii][jj] != -1 && Map[ii][jj + 1] != -1 &&       //  -|
                       Map[ii + 1][jj] == -1 && Map[ii + 1][jj + 1] != -1) Type = 1; //   |
                    else
                    if (Map[ii][jj] != -1 && Map[ii][jj + 1] != -1 &&       //  |-
                       Map[ii + 1][jj] != -1 && Map[ii + 1][jj + 1] == -1) Type = 2; //  |
                    else
                    if (Map[ii][jj] != -1 && Map[ii][jj + 1] == -1 &&       //  |
                       Map[ii + 1][jj] != -1 && Map[ii + 1][jj + 1] != -1) Type = 3; //  |_
                    else
                    if (Map[ii][jj] == -1 && Map[ii][jj + 1] != -1 &&       //   |
                       Map[ii + 1][jj] != -1 && Map[ii + 1][jj + 1] != -1) Type = 4; //  _|
                    break;
                case TypeRangeMesh.mRange2:
                    ii = 2 * i + 1;
                    jj = 2 * j + 1;
                    if (Map[ii][jj] == -1) Type = -1;
                    else
                    if (Map[ii - 1][jj - 1] == -1 && Map[ii - 1][jj + 1] == -1 &&
                       Map[ii + 1][jj - 1] == -1 && Map[ii + 1][jj + 1] == -1) Type = -1;
                    else
                    if (Map[ii - 1][jj - 1] != -1 && Map[ii - 1][jj + 1] != -1 &&
                       Map[ii + 1][jj - 1] != -1 && Map[ii + 1][jj + 1] != -1) Type = 0;
                    else
                    if (Map[ii - 1][jj - 1] != -1 && Map[ii - 1][jj + 1] != -1 &&       //  -|
                       Map[ii + 1][jj - 1] == -1 && Map[ii + 1][jj + 1] != -1) Type = 1; //   |
                    else
                    if (Map[ii - 1][jj - 1] != -1 && Map[ii - 1][jj + 1] != -1 &&       //  |-
                       Map[ii + 1][jj - 1] != -1 && Map[ii + 1][jj + 1] == -1) Type = 2; //  |
                    else
                    if (Map[ii - 1][jj - 1] != -1 && Map[ii - 1][jj + 1] == -1 &&       //  |
                       Map[ii + 1][jj - 1] != -1 && Map[ii + 1][jj + 1] != -1) Type = 3; //  |_
                    else
                    if (Map[ii - 1][jj - 1] == -1 && Map[ii - 1][jj + 1] != -1 &&       //   |
                       Map[ii + 1][jj - 1] != -1 && Map[ii + 1][jj + 1] != -1) Type = 4; //  _|
                    break;
                case TypeRangeMesh.mRange3:
                    ii = 3 * i + 1;
                    jj = 3 * j + 1;
                    if (Map[ii][jj] == -1 && Map[ii][jj + 1] == -1 &&
                       Map[ii + 1][jj] == -1 && Map[ii + 1][jj + 1] == -1) Type = -1;
                    else
                    if (Map[ii - 1][jj - 1] != -1 && Map[ii - 1][jj + 2] != -1 &&
                       Map[ii + 2][jj - 1] != -1 && Map[ii + 2][jj + 2] != -1) Type = 0;
                    else
                    if (Map[ii - 1][jj - 1] != -1 && Map[ii - 1][jj + 2] != -1 &&       //  -|
                       Map[ii + 2][jj - 1] == -1 && Map[ii + 2][jj + 2] != -1) Type = 1; //   |
                    else
                    if (Map[ii - 1][jj - 1] != -1 && Map[ii - 1][jj + 2] != -1 &&       //  |-
                       Map[ii + 2][jj - 1] != -1 && Map[ii + 2][jj + 2] == -1) Type = 2; //  |
                    else
                    if (Map[ii - 1][jj - 1] != -1 && Map[ii - 1][jj + 2] == -1 &&       //  |
                       Map[ii + 2][jj - 1] != -1 && Map[ii + 2][jj + 2] != -1) Type = 3; //  |_
                    else
                    if (Map[ii - 1][jj - 1] == -1 && Map[ii - 1][jj + 2] != -1 &&       //   |
                       Map[ii + 2][jj - 1] != -1 && Map[ii + 2][jj + 2] != -1) Type = 4; //  _|
                    break;
                    //case TypeRangeMesh.mRange4:
                    //    ii = 4 * i + 2;
                    //    jj = 4 * j + 2;
                    //    if (Map[ii][jj] == -1) Type = -1;
                    //    else
                    //    if (Map[ii - 2][jj - 2] == -1 && Map[ii - 2][jj + 2] == -1 &&
                    //       Map[ii + 2][jj - 2] == -1 && Map[ii + 2][jj + 2] == -1) Type = -1;
                    //    else
                    //    if (Map[ii - 2][jj - 2] != -1 && Map[ii - 2][jj + 2] != -1 &&
                    //       Map[ii + 2][jj - 2] != -1 && Map[ii + 2][jj + 2] != -1) Type = 0;
                    //    else
                    //    if (Map[ii - 2][jj - 2] != -1 && Map[ii - 2][jj + 2] != -1 &&       //  -|
                    //       Map[ii + 2][jj - 2] == -1 && Map[ii + 2][jj + 2] != -1) Type = 1; //   |
                    //    else
                    //    if (Map[ii - 2][jj - 2] != -1 && Map[ii - 2][jj + 2] != -1 &&       //  |-
                    //       Map[ii + 2][jj - 2] != -1 && Map[ii + 2][jj + 2] == -1) Type = 2; //  |
                    //    else
                    //    if (Map[ii - 2][jj - 2] != -1 && Map[ii - 2][jj + 2] == -1 &&       //  |
                    //       Map[ii + 2][jj - 2] != -1 && Map[ii + 2][jj + 2] != -1) Type = 3; //  |_
                    //    else
                    //    if (Map[ii - 2][jj - 2] == -1 && Map[ii - 2][jj + 2] != -1 &&       //   |
                    //       Map[ii + 2][jj - 2] != -1 && Map[ii + 2][jj + 2] != -1) Type = 4; //  _|
                    //    break;
            }
            return Type;
        }

        //int BoundKnotDef(int i, int j)
        //{
        //    // определение сигнатуры карты
        //    if (i < 0 || i > MaxNodeY - 1 || j < 0 || j > MaxNodeX - 1) 
        //        return -1;
        //    return Map[i][j];
        //}
    }
}