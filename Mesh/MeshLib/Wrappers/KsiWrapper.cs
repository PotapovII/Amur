//---------------------------------------------------------------------------
//                     Прототип: код Снигур К.С.
//                         релиз 26 06 2017 
//---------------------------------------------------------------------------
//                    19 11 2024 : Королева К.С
//---------------------------------------------------------------------------
namespace MeshLib.Wrappers
{
    using System;
    using MemLogLib;
    using System.Collections.Generic;
    
    [Serializable]
    public class KsiWrapper
    {
        /// <summary>
        /// 
        /// </summary>
        KsiMesh Mesh;
        //
        int CountElements = 0, CountKnots = 0;
        //
        double[] X, Y;
        /// Таблица связности [узел] [окружающие его треугольники] 
        /// </summary>
        public int[][] CVolumes;
        /// Лист связности внутренних КО, за исключением граничных узлов и узлов, где ставится WallFunc
        /// </summary>
        public int[][] CV2 = null;
        /// <summary>
        /// Лист связности КО, где ставится WallFunc (ближайшие узлы к границе)
        /// </summary>
        public int[][] CV_WallKnots = null;
        /// <summary>
        /// расстояние от пристеночного узла до дна по нормали
        /// </summary>
        public double[] CV_WallKnotsDistance = null;
        public double[] BWallDistance = null, TWallDistance = null;
        //
        /// <summary>
        /// граничные треугольники у дна, хранящиеся в номеров виде узлов
        /// </summary>
        public uint[][] BTrianglesKnots = null;
        //
        /// <summary>
        /// граничные труегольники у свободной поверхности, хранящиеся в виде номеров узлов
        /// </summary>
        public uint[][] TTrianglesKnots = null;
        /// <summary>
        /// граничные треугольники у дна, хранящтеся в виде номеров элементов
        /// </summary>
        public uint[] BTriangles = null;
        /// <summary>
        /// граничные треугольниуи у свободной поверхносии, хрянящиеся в виде номеров элементов
        /// </summary>
        public uint[] TTriangles = null;
        //
        /// <summary>
        /// массив связности окружающих дно точек в глобальной нумерации
        /// </summary>
        public uint[] CBottom = null;
        /// <summary>
        /// массив связности окружающих дно точек в глобальной нумерации
        /// </summary>
        public uint[] CTop = null;
        // геометрия КО
        /// <summary>
        /// координаты цетров масс треугольников
        /// </summary>
        public double[] Xc, Yc;
        /// <summary>
        /// координаты точек пересечения граней контура КО с гранями КЭ
        /// </summary>
        public double[][] Xcr, Ycr;
        /// <summary>
        /// площадь разностного аналога
        /// </summary>
        public double[][] S;
        /// <summary>
        /// площадь ячейки КО
        /// </summary>
        public double[] S0;
        /// <summary>
        /// длина фрагмента контура КО
        /// </summary>
        public double[][] Lk;
        /// <summary>
        /// x-координата внешней нормали к контуру КО
        /// </summary>
        public double[][] Nx;
        /// <summary>
        /// y-координата внешней нормали к контуру КО
        /// </summary>
        public double[][] Ny;
        /// <summary>
        /// длины плеч конечно-разностного аналога
        /// </summary>
        public double[][] Lx10, Lx32, Ly01, Ly23;
        /// <summary>
        /// Коэффициент определения точки пересечения (Xcr;Ycr)
        /// </summary>
        public double[][] Alpha;
        /// <summary>
        /// Значение второй вершины общей грани КО
        /// </summary>
        public int[][] P1;
        /// <summary>
        /// Геометрия конечных элементов для матрицы жесткости
        /// </summary>
        public double[] b1, b2, b3, c1, c2, c3;
        /// <summary>
        /// Площадь конечных элементов
        /// </summary>
        public double[] Sk;
        /// <summary>
        /// величина строки для хранения связных узлов с одним узлом
        /// </summary>
        int max_len = 10;
        public string error = "";
        //
        public double[] Sx, Sy;
        public KsiWrapper(KsiMesh mesh)
        {
            this.Mesh = mesh;
            this.X = mesh.X;
            this.Y = mesh.Y;
            CountElements = mesh.CountElements;
            CountKnots = mesh.CountKnots;
            //
            uint[][] m = GetBoundaryElements();
            BTriangles = m[0];
            TTriangles = m[1];
        }

        /// <summary>
        /// метод возвращает список номеров ТРЕУГОЛЬНИКОВ, у которых
        /// хотя бы 1 узел которых лежит на нижней или верхней границе
        /// нужно для аппроксимации по Гауссу придонных касательных напряжений
        /// </summary>
        /// <returns>двумерный массив mass,
        /// mass[0] - номера треугольников, лежащих на дне,
        /// mass[1] - номера треугольников, лежащих на свободной поверхности
        /// </returns>
        public uint[][] GetBoundaryElements()
        {
            uint[][] mass = new uint[2][];
            //
            double x_max = 0, x_min = 0, y_max = 0, y_min = 0;
            Mesh.MinMax(0, ref x_min, ref x_max);
            Mesh.MinMax(1, ref y_min, ref y_max);
            //
            List<uint> BottomTriangles = new List<uint>();
            List<uint> TopTriangles = new List<uint>();
            uint[] knots = new uint[3];
            for (uint i = 0; i < Mesh.AreaElems.Length; i++)
            {
                knots = Mesh.AreaElems[i];
                if ((Mesh.Y[knots[0]] == y_min) || (Mesh.Y[knots[1]] == y_min) || (Mesh.Y[knots[2]] == y_min))
                    BottomTriangles.Add(i);
                if ((Mesh.Y[knots[0]] == y_max) || (Mesh.Y[knots[1]] == y_max) || (Mesh.Y[knots[2]] == y_max))
                    TopTriangles.Add(i);
            }
            BottomTriangles.Sort();
            TopTriangles.Sort();
            mass[0] = new uint[BottomTriangles.Count];
            mass[1] = new uint[TopTriangles.Count];
            mass[0] = BottomTriangles.ToArray();
            mass[1] = TopTriangles.ToArray();
            return mass;
        }
        //
        protected void Initializing()
        {
            MEM.Alloc2D(CountKnots, max_len, ref CVolumes);
            //
            for (int i = 0; i < CountKnots; i++)
            {
                CVolumes[i][0] = i; //первый элемент - это центральных узел
                for (int j = 1; j < max_len; j++)
                    CVolumes[i][j] = -1;
            }
        }
        /// <summary>
        /// Генерация вспомогательных данных сетки о ее геометрии
        /// </summary>
        /// <param name="structureChanged"></param>
        public void TriangleGeometryCalculation(bool structureChanged = true)
        {

            // Инициализация массивов
            if (structureChanged)
                Initializing();
            //формирование масивов связности КО и граничных узлов
            // CVolumes, CPTop, CPRight, BTriangles, CBottom
            if (structureChanged)
                VolumeCommunicate();

            Xc = new double[CountElements];
            Yc = new double[CountElements];

            //
            #region Геометрия КО
            //
            for (uint fe = 0; fe < CountElements; fe++)
            {
                uint[] Knots = new uint[3];
                Mesh.ElementKnots(fe, ref Knots);
                //вычисляем цетры масс треугольников
                Xc[fe] = (X[Knots[0]] + X[Knots[1]] + X[Knots[2]]) / 3.0f;
                Yc[fe] = (Y[Knots[0]] + Y[Knots[1]] + Y[Knots[2]]) / 3.0f;
            }
            //
            int CVlength = CVolumes.Length;
            Xcr = new double[CountElements][];
            Ycr = new double[CountElements][];
            //цикл по внутренним КО
            for (int i = 0; i < CVlength; i++)
            {
                int jj = CVolumes[i].Length - 1;//количество КО, связанных с данным узлом
                //текущий внутренний узел
                int p0 = CVolumes[i][0];
                //массив точек пересечения граней КЭ и КО
                Xcr[p0] = new double[jj];
                Ycr[p0] = new double[jj];
                //
                for (int j = 0; j < jj; j++)
                {
                    //соcедние элементы
                    int v1 = CVolumes[i][(j + 1) % jj + 1];
                    int v2 = CVolumes[i][j + 1];
                    //вторая точка общей грани
                    int p1 = SharedGrane(p0, (uint)v1, (uint)v2);
                    //находим точку пересечения грани с узлом и грани КО
                    //находим точку пересечения грани с узлом и грани КО
                    double x0 = X[p0]; double y0 = Y[p0]; double x1 = X[p1]; double y1 = Y[p1];
                    double x2 = Xc[v1]; double y2 = Yc[v1]; double x3 = Xc[v2]; double y3 = Yc[v2];
                    double s1 = ((y2 - y3) * (x2 - x0) + (x3 - x2) * (y2 - y0)) / ((y2 - y3) * (x1 - x0) + (x3 - x2) * (y1 - y0));
                    Xcr[p0][j] = x0 + s1 * (x1 - x0);
                    Ycr[p0][j] = y0 + s1 * (y1 - y0);

                }
            }
            //
            Lx10 = new double[CountKnots][];
            Lx32 = new double[CountKnots][];
            Ly01 = new double[CountKnots][];
            Ly23 = new double[CountKnots][];
            S = new double[CountKnots][];
            S0 = new double[CountKnots];
            Alpha = new double[CountKnots][];
            Lk = new double[CountKnots][];

            P1 = new int[CountKnots][];
            Nx = new double[CountKnots][];
            Ny = new double[CountKnots][];

            for (int i = 0; i < CVlength; i++)
            {

                int p0 = CVolumes[i][0];
                int jj = CVolumes[i].Length - 1;//количество КО, связанных с данным узлом
                Lx10[p0] = new double[jj + 1];
                Lx32[p0] = new double[jj + 1];
                Ly01[p0] = new double[jj + 1];
                Ly23[p0] = new double[jj + 1];
                Alpha[p0] = new double[jj + 1];
                S[p0] = new double[jj + 1];
                Lk[p0] = new double[jj + 1];
                P1[p0] = new int[jj + 1];
                Nx[p0] = new double[jj + 1];
                Ny[p0] = new double[jj + 1];
                // заполнение массивов геометрии КО
                for (int j = 0; j < jj; j++)
                {
                    int v1 = CVolumes[i][(j + 1) % jj + 1];
                    int v2 = CVolumes[i][j + 1];
                    //вторая точка общей грани
                    P1[p0][j] = SharedGrane(p0, (uint)v1, (uint)v2);
                    //координаты рассматриваемого контура
                    double x0 = X[p0]; double y0 = Y[p0]; double x1 = X[P1[p0][j]]; double y1 = Y[P1[p0][j]];
                    double x2 = Xc[v1]; double y2 = Yc[v1]; double x3 = Xc[v2]; double y3 = Yc[v2];
                    //длины плеч конечно-разностного аналога
                    double lx10 = x1 - x0;
                    double lx32 = x3 - x2;
                    double ly01 = y0 - y1;
                    double ly23 = y2 - y3;
                    Lx10[p0][j] = lx10;
                    Lx32[p0][j] = lx32;
                    Ly01[p0][j] = ly01;
                    Ly23[p0][j] = ly23;
                    // координыты точки пересечения грани и прямой, соединяющей центры масс треугольников
                    double xcr = Xcr[p0][j]; double ycr = Ycr[p0][j];
                    // коэффициент пересечения грани и прямой, соединяющей центры масс треугольников
                    Alpha[p0][j] = Math.Abs((x0 - xcr) * ly23 + (y0 - ycr) * lx32) / (Math.Abs((x0 - xcr) * ly23 + (y0 - ycr) * lx32) + Math.Abs((xcr - x1) * ly23 + (ycr - y1) * lx32));
                    //площадь разностного аналога (не треугольника!)
                    S[p0][j] = (lx10 * ly23 + lx32 * -ly01) / 2.0;
                    //Вся площадь ячейки КО
                    S0[p0] += ((x3 - x0) * (y2 - y0) - (x2 - x0) * (y3 - y0)) / 2.0f;
                    //if (S0[i] < 0)
                    //    S0[i] = S0[i];
                    //длина текущего фрагмента внешнего контера КО
                    Lk[p0][j] = Math.Sqrt(lx32 * lx32 + ly23 * ly23);
                    //внешняя нормаль к грани КО (контуру КО)
                    Nx[p0][j] = ly23 / Math.Sqrt(lx32 * lx32 + ly23 * ly23);
                    Ny[p0][j] = lx32 / Math.Sqrt(lx32 * lx32 + ly23 * ly23);
                    //
                }
            }
            #endregion
            //
            #region Геометрия КЭ
            b1 = new double[CountElements];
            b2 = new double[CountElements];
            b3 = new double[CountElements];
            c1 = new double[CountElements];
            c2 = new double[CountElements];
            c3 = new double[CountElements];
            Sk = new double[CountElements];
            //
            //OrderablePartitioner<Tuple<int, int>> rangePartitioner = Partitioner.Create(0, Mesh.CountElem);
            //Parallel.ForEach(rangePartitioner,
            //        (range, loopState) =>
            //        {
            //            for (int fe = range.Item1; fe < range.Item2; fe++)
            //{
            for (uint i = 0; i < CountElements; i++)
            {
                uint[] Knots = new uint[3];
                Mesh.ElementKnots(i, ref Knots);
                //и номера его вершин
                uint Lm1 = Knots[0];
                uint Lm2 = Knots[1];
                uint Lm3 = Knots[2];
                // нахождение площади треугольника
                double LSk = ((X[Lm2] - X[Lm1]) * (Y[Lm3] - Y[Lm1]) - (X[Lm3] - X[Lm1]) * (Y[Lm2] - Y[Lm1])) / 2.0;
                //lock (lockThis)
                Sk[i] = LSk;
                // расчитываем геометрию элемента 
                // производные dL/dx и dL/dy
                double Lb1 = (Y[Lm2] - Y[Lm3]) / (2 * LSk);
                double Lb2 = (Y[Lm3] - Y[Lm1]) / (2 * LSk);
                double Lb3 = (Y[Lm1] - Y[Lm2]) / (2 * LSk);
                double Lc1 = (X[Lm3] - X[Lm2]) / (2 * LSk);
                double Lc2 = (X[Lm1] - X[Lm3]) / (2 * LSk);
                double Lc3 = (X[Lm2] - X[Lm1]) / (2 * LSk);
                // записывем производные L по х и y в массивы 
                b1[i] = Lb1;
                b2[i] = Lb2;
                b3[i] = Lb3;
                c1[i] = Lc1;
                c2[i] = Lc2;
                c3[i] = Lc3;
            }
            //}
            //заполняю массив касательных для массива приграничных нижних треугольников
            Sx = new double[BTriangles.Length];
            Sy = new double[BTriangles.Length];
            uint[] Knot = null;
            int ch = 0, idx = 0;
            int end = Mesh.BottomKnots.Length - 1;
            for (int i = 0; i < BTriangles.Length; i++)
            {
                Knot = Mesh.AreaElems[BTriangles[i]];
                ch = 0;
                for (int j = 0; j < Mesh.BottomKnots.Length; j++)
                {
                    if (Knot[0] == Mesh.BottomKnots[j])
                    {
                        ch++;
                        idx = j;
                    }
                    if (Knot[1] == Mesh.BottomKnots[j])
                    {
                        ch++;
                        idx = j;
                    }
                    if (Knot[2] == Mesh.BottomKnots[j])
                    {
                        ch++;
                        idx = j;
                    }
                    if (ch == 2)
                        break;
                }
                double[] SxSy = new double[2];
                //если треугольник не лежит на дне
                if (ch == 1)
                {
                    if (idx == end)
                        SxSy = ShearLine(idx - 1, idx);
                    else if (idx == 0)
                        SxSy = ShearLine(idx, idx + 1);
                    else
                        SxSy = ShearLine(idx - 1, idx + 1);

                }
                //если треугольник лежит на дне
                if (ch == 2)
                {
                    if (idx != 0)
                        SxSy = ShearLine(idx - 1, idx);

                    //else
                    //{
                    //    ch = ch;
                    //}
                }
                Sx[i] = SxSy[0];
                Sy[i] = SxSy[1];
            }
            #endregion
        }
        /// <summary>
        /// метод, который формирует массив связности узла 
        /// и окружающих его против часовой стрелки треугольков
        /// оставила для сохранени явозможности разбивать 
        /// четырехугольники на треугольники по кратчайшей диагонали
        /// </summary>
        private void VolumeCommunicate()
        {
            try
            {
                bool F_in;
                //формируется неcтруктурированная таблица связности
                for (uint fe = 0; fe < CountElements; fe++)
                {
                    // получение узлов треугольника
                    uint[] Knots = new uint[3];
                    Mesh.ElementKnots(fe, ref Knots);
                    for (int i = 0; i < 3; i++)
                    {
                        // флаг наполненности массива
                        F_in = false;
                        for (int j = 1; j < max_len; j++)
                        {
                            if (CVolumes[Knots[i]][j] == -1)
                            {
                                //вписываем номер КЭ в доступную ячейку
                                CVolumes[Knots[i]][j] = (int)fe;
                                F_in = true;
                                break;
                            }
                            //
                        }
                        // если некуда вписать номер КЭ, то расширяем массив и вписываем
                        if (F_in == false)
                        {
                            // 1 test
                            // если элементов вокруг точки больше, чем 10, то расширяем строку на 3
                            uint pt = Knots[i];
                            int len = CVolumes[pt].Length;
                            int[] tmp = new int[len];
                            //буфер
                            for (int k = 0; k < len; k++)
                                tmp[k] = CVolumes[pt][k];
                            //копируем обратно
                            CVolumes[pt] = new int[len + 3];
                            for (int k = 0; k < len; k++)
                                CVolumes[pt][k] = tmp[k];
                            // три новых ячейки
                            CVolumes[pt][len] = -1; CVolumes[pt][len + 1] = -1; CVolumes[pt][len + 2] = -1;
                            tmp = null;
                            max_len += 3;
                            //
                            i--;
                        }
                    }
                }
                //чищу все незанятые ячейки
                int[] ss;
                for (int i = 0; i < CountKnots; i++)
                {
                    ss = new int[max_len];
                    for (int j = 0; j < CVolumes[i].Length; j++)
                    {
                        if (CVolumes[i][j] != -1)
                        {
                            ss[j] = CVolumes[i][j];
                            continue;
                        }
                        CVolumes[i] = new int[j];
                        break;
                    }
                    for (int j = 0; j < CVolumes[i].Length; j++)
                        CVolumes[i][j] = ss[j];
                }
                //
                for (int i = 0; i < CountKnots; i++)
                {
                    #region Формирование неструктурированного листа связности КО (сортировка)
                    // центральный узел, вокруг которого строится контур КО
                    int CKnot = CVolumes[i][0];
                    //сначала рассматриваем первый треугольник, в который входит этот узел, 
                    //находим первую вершину контура против часовой стрелки
                    //
                    //вершины треугольника
                    uint[] VKnots = new uint[3];
                    Mesh.ElementKnots((uint)CVolumes[i][1], ref VKnots);
                    int j = 0;
                    for (j = 0; j < 3; j++)
                    {
                        //находим положение центрального узла
                        if (VKnots[j] == CKnot)
                            break;
                    }
                    //тогда вершина контура против часовой стрелки будет через один от нее
                    int PrevK = (int)VKnots[(j + 2) % 3];
                    VKnots = new uint[3];
                    //следующие треугольники сортируем отталкиваясь от PrevK точки, формируя контур
                    int ch = 2;
                    for (int l = 2; l < CVolumes[i].Length - 1; l++)
                    {
                        bool flag = false;
                        //ищем следующий треугольник в цепи
                        for (j = ch; j < CVolumes[i].Length; j++)
                        {
                            //текущий треугольник
                            //вершины треугольника
                            Mesh.ElementKnots((uint)CVolumes[i][j], ref VKnots);
                            for (int k = 0; k < 3; k++)
                            {
                                //находим положение текущего узла контура PrevK
                                if (VKnots[k] == PrevK)
                                {
                                    //меняем местами треугольники
                                    int buf = CVolumes[i][j]; // треугольник ij
                                    CVolumes[i][j] = CVolumes[i][ch]; // меняем на треугольник с точкой PrevK
                                    CVolumes[i][ch++] = buf; // треугольник ij ставим на место треугольника с PrevK
                                    //тогда следующая вершина контура против часовой стрелки будет через один от нее
                                    PrevK = (int)VKnots[(k + 1) % 3];
                                    flag = true;
                                    break;
                                }
                            }
                            // если нашли треугольник с PrevK
                            if (flag)
                                break;
                        }

                    }
                }
                #endregion
                //
                //убираю граничные узлы из массива связности КО---???
                int knot = 0, sch = 0;
                for (int i = 0; i < CountKnots; i++)
                {
                    for (int j = 0; j < Mesh.CountLeft; j++)
                    {
                        knot = Mesh.LeftKnots[j];
                        if (CVolumes[i][0] == knot)
                        {
                            CVolumes[i][0] = -1; sch++;
                            break;
                        }
                    }
                    for (int j = 0; j < Mesh.CountRight; j++)
                    {
                        knot = Mesh.RightKnots[j];
                        if (CVolumes[i][0] == knot)
                        {
                            CVolumes[i][0] = -1; sch++;
                            break;
                        }
                    }
                    //
                    for (int j = 0; j < Mesh.CountBottom; j++)
                    {
                        knot = Mesh.BottomKnots[j];
                        if (CVolumes[i][0] == knot)
                        {
                            CVolumes[i][0] = -1; sch++;
                            break;
                        }
                    }
                    for (int j = 0; j < Mesh.CountTop; j++)
                    {
                        knot = Mesh.TopKnots[j];
                        if (CVolumes[i][0] == knot)
                        {
                            CVolumes[i][0] = -1; sch++;
                            break;
                        }
                    }
                }
                //
                int[][] CVolumes_new = new int[CVolumes.Length - sch][];
                //
                sch = 0;
                for (int i = 0; i < CountKnots; i++)
                {
                    if (CVolumes[i][0] != -1)
                    {
                        CVolumes_new[sch] = new int[CVolumes[i].Length];
                        //
                        for (int k = 0; k < CVolumes[i].Length; k++)
                            CVolumes_new[sch][k] = CVolumes[i][k];
                        sch++;
                    }
                }
                CVolumes = CVolumes_new;
                //формирую массив граничных узлов и узлов, образующих грань с узлом на границе
                // для моей фронтальной нумерации для дна это граничный узел  и граничный узел +1
                CBottom = new uint[Mesh.CountBottom * 2];
                sch = 0;
                //
                for (uint i = 0; i < Mesh.CountBottom; i++)
                {
                    CBottom[sch++] = (uint)Mesh.BottomKnots[i];
                    CBottom[sch++] = (uint)Mesh.BottomKnots[i] + 1;
                }
                //для свободной поверхности - это граничный узел и граничный узел -1
                CTop = new uint[Mesh.CountBottom * 2];
                sch = 0;
                //
                for (uint i = 0; i < Mesh.CountBottom; i++)
                {
                    CTop[sch++] = (uint)Mesh.TopKnots[i];
                    CTop[sch++] = (uint)Mesh.TopKnots[i] - 1;
                }
            }
            catch (Exception ex)
            {
                error = error + "Mesh.VolumeCommunicate " + ex.Message;
            }
        }
        //
        public void MakeWallFuncStructure(bool surf_flag)
        {
            if (!surf_flag)
            {
                CV2 = new int[CVolumes.Length - 2 * (Mesh.CountBottom - 2)][];
                CV_WallKnots = new int[2 * (Mesh.CountBottom - 2)][];
                //CV_WallKnotsDistance = new double[2 * (Mesh.CountBottom - 2)];
                //
                //BWallDistance = new double[Mesh.CountBottom];
                //TWallDistance = new double[Mesh.CountTop];
                //
                MEM.Alloc(2 * (Mesh.CountBottom - 2), ref CV_WallKnotsDistance);
                MEM.Alloc(Mesh.CountBottom, ref BWallDistance);
                MEM.Alloc(Mesh.CountTop, ref TWallDistance);
                //

                int ch = 0, p0 = 0, ch_wall = 0;
                for (int i = 0; i < CVolumes.Length; i++)
                {
                    p0 = CVolumes[i][0];
                    // нижняя граница
                    if ((p0 - 1) % Mesh.CountLeft == 0)
                    {
                        CV_WallKnots[ch_wall] = CVolumes[i];
                        CV_WallKnotsDistance[ch_wall] = GetNormalDistanceBottom(p0);
                        //
                        BWallDistance[(p0 - 1) / Mesh.CountLeft] = CV_WallKnotsDistance[ch_wall];
                        ch_wall++;
                        continue;
                    }
                    // верхняя граница
                    if ((p0 + 2) % Mesh.CountLeft == 0)
                    {
                        CV_WallKnots[ch_wall] = CVolumes[i];
                        CV_WallKnotsDistance[ch_wall] = GetNormalDistanceTop(p0);
                        //
                        TWallDistance[(p0 + 2) / Mesh.CountLeft - 1] = CV_WallKnotsDistance[ch_wall];
                        ch_wall++;
                        continue;
                    }

                    CV2[ch] = CVolumes[i];
                    ch++;

                }
                int LK0 = Mesh.LeftKnots[0], LK1 = Mesh.LeftKnots[1];
                int TK0 = Mesh.TopKnots[0];
                TWallDistance[0] = Math.Sqrt((Y[LK0] - Y[LK1]) * (Y[LK0] - Y[LK1]) + (X[LK0] - X[LK1]) * (X[LK0] - X[LK1]));
                TWallDistance[Mesh.CountTop - 1] = Math.Sqrt((Y[TK0] - Y[TK0 - 1]) * (Y[TK0] - Y[TK0 - 1]) + (X[TK0] - X[TK0 - 1]) * (X[TK0] - X[TK0 - 1]));

            }
            else
            {
                CV2 = new int[CVolumes.Length - (Mesh.CountBottom - 2)][];
                CV_WallKnots = new int[Mesh.CountBottom - 2][];
                CV_WallKnotsDistance = new double[Mesh.CountBottom - 2];
                //
                BWallDistance = new double[Mesh.CountBottom];
                int ch = 0, p0 = 0, ch_wall = 0;
                for (int i = 0; i < CVolumes.Length; i++)
                {
                    p0 = CVolumes[i][0];
                    if ((p0 - 1) % Mesh.CountLeft != 0)
                    {
                        CV2[ch] = CVolumes[i];
                        ch++;
                    }
                    else
                    {
                        CV_WallKnots[ch_wall] = CVolumes[i];
                        CV_WallKnotsDistance[ch_wall] = GetNormalDistanceBottom(p0);
                        //
                        int idx = (p0 - 1) / Mesh.CountLeft;
                        BWallDistance[idx] = CV_WallKnotsDistance[ch_wall];
                        ch_wall++;
                        continue;
                    }

                }
            }
            int RK0 = Mesh.RightKnots[0], RK1 = Mesh.RightKnots[1];
            BWallDistance[0] = Math.Sqrt((Y[1] - Y[0]) * (Y[1] - Y[0]) + (X[1] - X[0]) * (X[1] - X[0]));
            BWallDistance[Mesh.CountBottom - 1] = Math.Sqrt((Y[RK1] - Y[RK0]) * (Y[RK1] - Y[RK0]) + (X[RK1] - X[RK0]) * (X[RK1] - X[RK0]));
        }
        /// <summary>
        /// метод находит длину нормали от пристенночной точки ко дну
        /// </summary>
        /// <param name="p0">пристеночная точка (не лежит на границе)</param>
        /// <returns></returns>
        public double GetNormalDistanceBottom(int p0)
        {
            double n_length0 = 100000, n_lengthMin = 100000;
            //
            double Xm = X[p0], Ym = Y[p0], Xf = 0, Yf = 0;
            // регулярная сетка, определяем порядковый номер в массиве
            int n = p0 / Mesh.CountLeft, knot0 = 0, knot1 = 0;
            double X0 = 0, X1 = 0, Y0 = 0, Y1 = 0, nx = 0, ny = 0, k = 0, nydnx = 0;
            double max = 0, min = 0;
            for (int i = n - 10; i < n + 10; i++)
            {
                if (i <= 0 || i >= Mesh.CountBottom)
                    continue;
                knot0 = Mesh.BottomKnots[i - 1];
                knot1 = Mesh.BottomKnots[i];
                X0 = X[knot0]; X1 = X[knot1];
                Y0 = Y[knot0]; Y1 = Y[knot1];
                // работа с первой границей
                // длина отрезка дна
                k = Math.Sqrt((X1 - X0) * (X1 - X0) + (Y1 - Y0) * (Y1 - Y0));
                // нормаль к отрезку дна
                nx = (Y1 - Y0) / k;
                ny = (X0 - X1) / k;
                // если ровное дно
                if (Math.Abs(nx) < 1.0e-6)
                {
                    // точка пересечения отрезка дна с нормалью из пристеночного узла
                    Xf = Xm;
                    Yf = Y1;
                }
                // если не ровное дно
                else
                {
                    k = (Y1 - Y0) / (X1 - X0);
                    nydnx = ny / nx;
                    // точка пересечения отрезка дна с нормалью из пристеночного узла
                    Xf = (Ym - Y0 - nydnx * Xm + k * X0) / (k - nydnx);
                    Yf = Ym + nydnx * (Xf - Xm);
                }
                //проверяем, попадает ли точка персечения на рассматриваемый отрезок
                max = Math.Max(X0, X1);
                min = Math.Min(X0, X1);
                if (Xf >= min && Xf <= max)
                    n_length0 = Math.Sqrt((Xm - Xf) * (Xm - Xf) + (Ym - Yf) * (Ym - Yf));
                // сохраняем минимальное значение
                //if (n_length0!=0)
                n_lengthMin = Math.Min(n_length0, n_lengthMin);
            }
            //
            //double l = Math.Sqrt((Xm - X[p0 + 1]) * (Xm - X[p0 + 1]) + (Ym - Y[p0 + 1]) * (Ym - Y[p0 + 1]));
            double l = Ym - Y[(p0 / Mesh.CountLeft) * Mesh.CountLeft];
            return Math.Min(n_lengthMin, l);
        }
        public double GetNormalDistanceTop(int p0)
        {
            double n_length0 = 100000, n_lengthMin = 100000;
            //
            double Xm = X[p0], Ym = Y[p0], Xf = 0, Yf = 0;
            // регулярная сетка, определяем порядковый номер в массиве
            int n = Mesh.CountTop - (p0 + 1) / Mesh.CountLeft, knot0 = 0, knot1 = 0;
            double X0 = 0, X1 = 0, Y0 = 0, Y1 = 0, nx = 0, ny = 0, k = 0, nydnx = 0;
            double max = 0, min = 0;
            for (int i = n - 10; i < n + 10; i++)
            {
                if (i < 0 || i >= (Mesh.CountTop - 1))
                    continue;
                knot0 = Mesh.TopKnots[i + 1];
                knot1 = Mesh.TopKnots[i];
                X0 = X[knot0]; X1 = X[knot1];
                Y0 = Y[knot0]; Y1 = Y[knot1];
                // работа с первой границей
                k = Math.Sqrt((X1 - X0) * (X1 - X0) + (Y1 - Y0) * (Y1 - Y0));
                nx = (Y0 - Y1) / k;
                ny = (X1 - X0) / k;
                if (nx == 0)
                {
                    Xf = Xm;
                    Yf = Y1;
                }
                else
                {
                    k = (Y1 - Y0) / (X1 - X0);
                    nydnx = ny / nx;
                    //
                    Xf = (Ym - Y0 - nydnx * Xm + k * X0) / (k - nydnx);
                    Yf = Ym + nydnx * (Xf - Xm);
                }
                //
                max = Math.Max(X0, X1);
                min = Math.Min(X0, X1);
                if (Xf >= min && Xf <= max)
                    n_length0 = Math.Sqrt((Xm - Xf) * (Xm - Xf) + (Ym - Yf) * (Ym - Yf));
                //
                n_lengthMin = Math.Min(n_length0, n_lengthMin);
            }
            //
            return n_lengthMin;
        }

        private int SharedGrane(int p0, uint v1, uint v2)
        {
            int p1 = p0;
            //
            uint[] knotsV1 = new uint[3];
            Mesh.ElementKnots(v1, ref knotsV1);
            //
            uint[] knotsV2 = new uint[3];
            Mesh.ElementKnots(v2, ref knotsV2);
            //
            for (int k = 0; k < 3; k++)
            {
                if (knotsV1[0] == knotsV2[k])
                    p1 = (int)knotsV1[0];
                else if (knotsV1[1] == knotsV2[k])
                    p1 = (int)knotsV1[1];
                else if (knotsV1[2] == knotsV2[k])
                    p1 = (int)knotsV1[2];
                if (p1 != p0)
                    break;
            }
            return p1;
        }
        private double[] ShearLine(int idx_a, int idx_b)
        {
            int a = Mesh.BottomKnots[idx_a];
            int b = Mesh.BottomKnots[idx_b];
            double ss = Math.Sqrt((X[b] - X[a]) * (X[b] - X[a]) + (Y[b] - Y[a]) * (Y[b] - Y[a]));
            double[] SxSy = new double[2];
            SxSy[0] = 1.0 / ss * (X[b] - X[a]);
            SxSy[1] = 1.0 / ss * (Y[b] - Y[a]);
            return SxSy;
        }

        public void TransportCVstructure(KsiWrapper prevWrapper)
        {
            if (prevWrapper != null)
            {
                this.CVolumes = prevWrapper.CVolumes;
                this.BTriangles = prevWrapper.BTriangles;
                this.TTriangles = prevWrapper.TTriangles;
            }
        }
    }

}
