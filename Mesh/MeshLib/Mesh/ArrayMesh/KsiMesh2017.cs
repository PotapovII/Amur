//---------------------------------------------------------------------------
//                 Разработка кода : Снигур К.С.
//                         релиз 26 06 2017 
//---------------------------------------------------------------------------
//         интеграция в RiverLib 13 08 2022 : Потапов И.И.
//---------------------------------------------------------------------------
//      Убраны массивы ГУ из всей иерархи сеток : 01.06.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using MemLogLib;
    using CommonLib;

    [Serializable]
    public class KsiMesh2017 : ComplecsMesh
    {
        /// <summary>
        /// идентификатор структуры сетки: 0 - треугольная, 1 - смешанная, 2 - четырехугольная
        /// </summary>
        public int StructureIndex;

        #region Дополнение полей
        /// <summary>
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


        /// <summary>
        /// Узлы левой границы
        /// </summary>
        public int[] LeftKnots;
        /// <summary>
        /// Узлы правой границы
        /// </summary>
        public int[] RightKnots;
        /// <summary>
        /// Узлы верхней границы
        /// </summary>
        public int[] TopKnots;
        /// Узлы нижней границы
        /// </summary>
        public int[] BottomKnots;
        /// <summary>
        /// [][0] - левые граничные узлы, [][1..n] - связные с ним узлы, не лежащие на границе
        /// </summary>
        public int[][] CPLeft;
        /// <summary>
        /// [][0] - правые граничные узлы, [][1..n] - связные с ним узлы, не лежащие на границе
        /// </summary>
        public int[][] CPRight;
        /// <summary>
        /// [][0] - верхние граничные узлы, [][1..n] - связные с ним узлы, не лежащие на границе
        /// </summary>
        public int[][] CPTop;
        /// <summary>
        /// [][0] - нижние граничные узлы, [][1..n] - связные с ним узлы, не лежащие на границе
        /// </summary>
        public int[][] CPBottom;
        /// <summary>
        /// массив - граничные треугольники [1..4] для внутреннего алгоритма
        /// </summary>
        int[] BottomTriangles = null;
        /// <summary>
        /// упрощеная версия хранения граничных треугольников у дна
        /// </summary>
        public int[] BTriangles = null;
        /// <summary>
        /// массив связности окружающих дно точек в глобальной нумерации
        /// </summary>
        public int[] CBottom = null;
        /// <summary>
        /// массив - граничные треугольники [1..4] для внутреннего алгоритма
        /// </summary>
        int[] TopTriangles = null;
        /// <summary>
        /// упрощеная версия хранения граничных треугольников у дна
        /// </summary>
        public int[] TTriangles = null;
        /// <summary>
        /// массив связности окружающих дно точек в глобальной нумерации
        /// </summary>
        public int[] CTop = null;
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
        //
        public double[] Sx, Sy;
        /// <summary>
        /// Шаблон перенумерации (для решулярной сетки)
        /// </summary>
        int[] Numbers = null;
        /// <summary>
        /// величина строки для хранения связных узлов с одним узлом
        /// </summary>
        int max_len = 10;
        public string error = "";
        #endregion
        #region Поля
        /// <summary>
        /// Количество узлов правой границы 
        /// </summary>
        public int CountRight;
        /// <summary>
        /// Количество узлов верхней границы 
        /// </summary>
        public int CountTop;
        /// <summary>
        /// Количество узлов нижней границы 
        /// </summary>
        public int CountBottom;
        /// <summary>
        /// Количество узлов левой границы 
        /// </summary>
        public int CountLeft;
        #endregion
        //
        #region Методы
        public KsiMesh2017() : base() { }

        public KsiMesh2017(KsiMesh2017 m) : base(m)
        {
            CountLeft = m.CountLeft;
            CountRight = m.CountRight;
            CountTop = m.CountTop;
            CountBottom = m.CountBottom;

            MEM.MemCopy(ref CVolumes, m.CVolumes);
            MEM.MemCopy(ref LeftKnots, m.LeftKnots);
            MEM.MemCopy(ref TopKnots, m.TopKnots);
            MEM.MemCopy(ref RightKnots, m.RightKnots);
            MEM.MemCopy(ref BottomKnots, m.BottomKnots);

            MEM.MemCopy(ref CPLeft, m.CPLeft);
            MEM.MemCopy(ref CPRight, m.CPRight);
            MEM.MemCopy(ref CPTop, m.CPTop);
            MEM.MemCopy(ref CPBottom, m.CPBottom);
            return;
            //MEM.MemCopy(ref BottomTriangles, m.BottomTriangles);
            //MEM.MemCopy(ref BTriangles, m.BTriangles);
            //MEM.MemCopy(ref CBottom, m.CBottom);
            //MEM.MemCopy(ref Xc, m.Xc);
            //MEM.MemCopy(ref Yc, m.Yc);
            //return;

            //MEM.MemCopy(ref Ycr, m.Ycr);
            //MEM.MemCopy(ref Ycr, m.Ycr);

            //MEM.MemCopy(ref S, m.S);
            //MEM.MemCopy(ref S0, m.S0);

            //MEM.MemCopy(ref Lk, m.Lk);
            //MEM.MemCopy(ref Nx, m.Nx);
            //MEM.MemCopy(ref Ny, m.Ny);

            //MEM.MemCopy(ref Lx10, m.Lx10);
            //MEM.MemCopy(ref Lx32, m.Lx32);
            //MEM.MemCopy(ref Ly01, m.Ly01);
            //MEM.MemCopy(ref Ly23, m.Ly23);

            //MEM.MemCopy(ref Alpha, m.Alpha);
            //MEM.MemCopy(ref P1, m.P1);
            //MEM.MemCopy(ref Sk, m.Sk);

            //MEM.MemCopy(ref Sx, m.Sx);
            //MEM.MemCopy(ref Sy, m.Sy);
        }

        public KsiMesh2017(ComplecsMesh m) : base(m)
        {
            List<int>[] bknots = new List<int>[4];
            for (int i = 0; i < bknots.Length; i++)
                bknots[i] = new List<int>();
            for (int i = 0; i < m.CountBoundKnots; i++)
            {
                int mark = m.BoundKnotsMark[i];
                bknots[mark].Add(m.BoundKnots[i]);
            }
            CountBottom = bknots[0].Count;
            CountRight = bknots[1].Count;
            CountTop = bknots[2].Count;
            CountLeft = bknots[3].Count;
            BottomKnots = bknots[0].ToArray();
            RightKnots = bknots[1].ToArray();
            TopKnots = bknots[2].ToArray();
            LeftKnots = bknots[3].ToArray();
            //
            TriangleGeometryCalculation();
            //MakeWallFuncStructure(false);
        }

        /// <summary>
        /// Клонирование объекта сетки
        /// </summary>
        public override IMesh Clone()
        {
            return new KsiMesh2017(this);
        }
        internal void TransportCVstructure(KsiMesh2017 prevMesh)
        {
            if (prevMesh != null)
            {
                this.CVolumes = prevMesh.CVolumes;
                this.BTriangles = prevMesh.BTriangles;
                this.TTriangles = prevMesh.TTriangles;
                this.CBottom = prevMesh.CBottom;
                this.CTop = prevMesh.CTop;
                this.CPRight = prevMesh.CPRight;
                this.CPTop = prevMesh.CPTop;
                this.CPBottom = prevMesh.CPBottom;
            }
        }


        /// <summary>
        /// Выделение памяти
        /// </summary>
        void Initializing()
        {

            CVolumes = new int[CountKnots][];
            for (int i = 0; i < CountKnots; i++)
            {
                CVolumes[i] = new int[max_len];// можно поставить больше, если область будет сильно сложная или будет сильное сгущение сетки
                CVolumes[i][0] = i;//первый элемент - это центральных узел
                for (int j = 1; j < max_len; j++)
                    CVolumes[i][j] = -1;
            }
            BottomTriangles = new int[CountBottom * 5];
            for (int i = 0; i < BottomTriangles.Length; i++)
                BottomTriangles[i] = -1;
            TopTriangles = new int[CountBottom * 5];
            for (int i = 0; i < TopTriangles.Length; i++)
                TopTriangles[i] = -1;
            //
            CPRight = new int[CountRight - 2][];
            CPTop = new int[CountTop - 2][];
            //
            Xc = new double[CountElements];
            Yc = new double[CountElements];

        }
        public int BandWidth()
        {
            int width = 0;
            int min = 0, max = 0;
            for (int fe = 0; fe < CountElements; fe++)
            {
                uint[] Knots = AreaElems[fe];
                max = (int)Knots.Max();
                min = (int)Knots.Min();
                width = Math.Max(width, max - min);
            }
            return width + 1;
        }
        public int GetTriangle(double x, double y)
        {
            for (int k = 0; k < AreaElems.Length; k++)
            {
                uint[] Knots = AreaElems[k];
                double[] Xk = { CoordsX[Knots[0]], CoordsX[Knots[1]], CoordsX[Knots[2]] };
                double[] Yk = { CoordsY[Knots[0]], CoordsY[Knots[1]], CoordsY[Knots[2]] };
                //
                if ((x > Xk.Min()) && (x < Xk.Max()))
                {
                    if ((y > Yk.Min()) && (y < Yk.Max()))
                    {
                        return k;
                    }

                }
            }
            return -1;
        }


        public void TriangleGeometryCalculation(bool structureChanged = true)
        {
            // Инициализация массивов
            if (structureChanged)
                Initializing();
            //формирование масивов связности КО и граничных узлов
            // CVolumes, CPTop, CPRight, BTriangles, CBottom
            if (structureChanged)
                VolumeCommunicate();
            else
            {
                Xc = new double[CountElements];
                Yc = new double[CountElements];
            }
            //
            #region Геометрия КО
            //
            for (int fe = 0; fe < CountElements; fe++)
            {
                uint[] Knots = AreaElems[fe];
                //вычисляем цетры масс треугольников
                Xc[fe] = (CoordsX[Knots[0]] + CoordsX[Knots[1]] + CoordsX[Knots[2]]) / 3.0f;
                Yc[fe] = (CoordsY[Knots[0]] + CoordsY[Knots[1]] + CoordsY[Knots[2]]) / 3.0f;
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
                    int p1 = SharedGrane(p0, v1, v2);
                    //находим точку пересечения грани с узлом и грани КО
                    double x0 = CoordsX[p0]; double y0 = CoordsY[p0]; double x1 = CoordsX[p1]; double y1 = CoordsY[p1];
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
                    P1[p0][j] = SharedGrane(p0, v1, v2);
                    //координаты рассматриваемого контура
                    double x0 = CoordsX[p0]; double y0 = CoordsY[p0]; double x1 = CoordsX[P1[p0][j]]; double y1 = CoordsY[P1[p0][j]];
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
            for (int i = 0; i < CountElements; i++)
            {
                //и номера его вершин
                uint Lm1 = AreaElems[i][0];
                uint Lm2 = AreaElems[i][1];
                uint Lm3 = AreaElems[i][2];
                // нахождение площади треугольника
                double LSk = ((CoordsX[Lm2] - CoordsX[Lm1]) * (CoordsY[Lm3] - CoordsY[Lm1]) - (CoordsX[Lm3] - CoordsX[Lm1]) * (CoordsY[Lm2] - CoordsY[Lm1])) / 2.0;
                //lock (lockThis)
                Sk[i] = LSk;
                // расчитываем геометрию элемента 
                // производные dL/dx и dL/dy
                double Lb1 = (CoordsY[Lm2] - CoordsY[Lm3]) / (2 * LSk);
                double Lb2 = (CoordsY[Lm3] - CoordsY[Lm1]) / (2 * LSk);
                double Lb3 = (CoordsY[Lm1] - CoordsY[Lm2]) / (2 * LSk);
                double Lc1 = (CoordsX[Lm3] - CoordsX[Lm2]) / (2 * LSk);
                double Lc2 = (CoordsX[Lm1] - CoordsX[Lm3]) / (2 * LSk);
                double Lc3 = (CoordsX[Lm2] - CoordsX[Lm1]) / (2 * LSk);
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
            int end = BottomKnots.Length - 1;
            for (int i = 0; i < BTriangles.Length; i++)
            {
                Knot = AreaElems[BTriangles[i]];
                ch = 0;
                for (int j = 0; j < BottomKnots.Length; j++)
                {
                    if (Knot[0] == BottomKnots[j])
                    {
                        ch++;
                        idx = j;
                    }
                    if (Knot[1] == BottomKnots[j])
                    {
                        ch++;
                        idx = j;
                    }
                    if (Knot[2] == BottomKnots[j])
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

        //public void TriangleGeometryCalculation()
        //{
        //    // Инициализация массивов
        //    Initializing();
        //    //формирование масивов связности КО и граничных узлов
        //    // CVolumes, CPTop, CPRight, BTriangles, CBottom
        //    VolumeCommunicate();
        //    //
        //    #region Геометрия КО
        //    //
        //    for (int fe = 0; fe < CountElements; fe++)
        //    {
        //        uint[] Knots = AreaElems[fe];
        //        //вычисляем цетры масс треугольников
        //        Xc[fe] = (CoordsX[Knots[0]] + CoordsX[Knots[1]] + CoordsX[Knots[2]]) / 3.0f;
        //        Yc[fe] = (CoordsY[Knots[0]] + CoordsY[Knots[1]] + CoordsY[Knots[2]]) / 3.0f;
        //    }
        //    //
        //    int CVlength = CVolumes.Length;
        //    Xcr = new double[CVlength][];
        //    Ycr = new double[CVlength][];
        //    //цикл по внутренним КО
        //    for (int i = 0; i < CVlength; i++)
        //    {
        //        int jj = CVolumes[i].Length - 1;//количество КО, связанных с данным узлом
        //        //массив точек пересечения граней КЭ и КО
        //        Xcr[i] = new double[jj];
        //        Ycr[i] = new double[jj];
        //        //текущий внутренний узел
        //        int p0 = CVolumes[i][0];
        //        //
        //        for (int j = 0; j < jj; j++)
        //        {
        //            //сосоедние элементы
        //            int v1 = CVolumes[i][(j + 1) % jj + 1];
        //            int v2 = CVolumes[i][j + 1];
        //            //вторая точка общей грани
        //            int p1 = SharedGrane(p0, v1, v2);
        //            //находим точку пересечения грани с узлом и грани КО
        //            double x0 = CoordsX[p0]; double y0 = CoordsY[p0]; double x1 = CoordsX[p1]; double y1 = CoordsY[p1];
        //            double x2 = Xc[v1]; double y2 = Yc[v1]; double x3 = Xc[v2]; double y3 = Yc[v2];
        //            double s1 = ((y2 - y3) * (x2 - x0) + (x3 - x2) * (y2 - y0)) / ((y2 - y3) * (x1 - x0) + (x3 - x2) * (y1 - y0));
        //            Xcr[i][j] = x0 + s1 * (x1 - x0);
        //            Ycr[i][j] = y0 + s1 * (y1 - y0);
        //        }
        //    }
        //    //

        //    Lx10 = new double[CVlength][];
        //    Lx32 = new double[CVlength][];
        //    Ly01 = new double[CVlength][];
        //    Ly23 = new double[CVlength][];
        //    S = new double[CVlength][];
        //    S0 = new double[CVlength];
        //    Alpha = new double[CVlength][];
        //    Lk = new double[CVlength][];

        //    P1 = new int[CVlength][];
        //    Nx = new double[CVlength][];
        //    Ny = new double[CVlength][];

        //    for (int i = 0; i < CVlength; i++)
        //    {

        //        int p0 = CVolumes[i][0];
        //        int jj = CVolumes[i].Length - 1;//количество КО, связанных с данным узлом
        //        Lx10[i] = new double[jj + 1];
        //        Lx32[i] = new double[jj + 1];
        //        Ly01[i] = new double[jj + 1];
        //        Ly23[i] = new double[jj + 1];
        //        Alpha[i] = new double[jj + 1];
        //        S[i] = new double[jj + 1];
        //        Lk[i] = new double[jj + 1];
        //        P1[i] = new int[jj + 1];
        //        Nx[i] = new double[jj + 1];
        //        Ny[i] = new double[jj + 1];
        //        // заполнение массивов геометрии КО
        //        for (int j = 0; j < jj; j++)
        //        {
        //            int v1 = CVolumes[i][(j + 1) % jj + 1];
        //            int v2 = CVolumes[i][j + 1];
        //            //вторая точка общей грани
        //            P1[i][j] = SharedGrane(p0, v1, v2);
        //            //координаты рассматриваемого контура
        //            double x0 = CoordsX[p0]; double y0 = CoordsY[p0]; double x1 = CoordsX[P1[i][j]]; double y1 = CoordsY[P1[i][j]];
        //            double x2 = Xc[v1]; double y2 = Yc[v1]; double x3 = Xc[v2]; double y3 = Yc[v2];
        //            //длины плеч конечно-разностного аналога
        //            double lx10 = x1 - x0;
        //            double lx32 = x3 - x2;
        //            double ly01 = y0 - y1;
        //            double ly23 = y2 - y3;
        //            Lx10[i][j] = lx10;
        //            Lx32[i][j] = lx32;
        //            Ly01[i][j] = ly01;
        //            Ly23[i][j] = ly23;
        //            // координыты точки пересечения грани и прямой, соединяющей центры масс треугольников
        //            double xcr = Xcr[i][j]; double ycr = Ycr[i][j];
        //            // коэффициент пересечения грани и прямой, соединяющей центры масс треугольников
        //            Alpha[i][j] = Math.Abs((x0 - xcr) * ly23 + (y0 - ycr) * lx32) / (Math.Abs((x0 - xcr) * ly23 + (y0 - ycr) * lx32) + Math.Abs((xcr - x1) * ly23 + (ycr - y1) * lx32));
        //            //площадь разностного аналога (не треугольника!)
        //            S[i][j] = (lx10 * ly23 + lx32 * -ly01) / 2.0;
        //            //Вся площадь ячейки КО
        //            S0[i] += ((x3 - x0) * (y2 - y0) - (x2 - x0) * (y3 - y0)) / 2.0f;
        //            //if (S0[i] < 0)
        //            //    S0[i] = S0[i];
        //            //длина текущего фрагмента внешнего контера КО
        //            Lk[i][j] = Math.Sqrt(lx32 * lx32 + ly23 * ly23);
        //            //внешняя нормаль к грани КО (контуру КО)
        //            Nx[i][j] = ly23 / Math.Sqrt(lx32 * lx32 + ly23 * ly23);
        //            Ny[i][j] = lx32 / Math.Sqrt(lx32 * lx32 + ly23 * ly23);
        //            //
        //        }
        //    }
        //    #endregion
        //    //
        //    #region Геометрия КЭ
        //    b1 = new double[CountElements];
        //    b2 = new double[CountElements];
        //    b3 = new double[CountElements];
        //    c1 = new double[CountElements];
        //    c2 = new double[CountElements];
        //    c3 = new double[CountElements];
        //    Sk = new double[CountElements];
        //    //
        //    //OrderablePartitioner<Tuple<int, int>> rangePartitioner = Partitioner.Create(0, Mesh.CountElem);
        //    //Parallel.ForEach(rangePartitioner,
        //    //        (range, loopState) =>
        //    //        {
        //    //            for (int fe = range.Item1; fe < range.Item2; fe++)
        //    //{
        //    for (int i = 0; i < CountElements; i++)
        //    {
        //        //и номера его вершин
        //        uint Lm1 = AreaElems[i][0];
        //        uint Lm2 = AreaElems[i][1];
        //        uint Lm3 = AreaElems[i][2];
        //        // нахождение площади треугольника
        //        double LSk = ((CoordsX[Lm2] - CoordsX[Lm1]) * (CoordsY[Lm3] - CoordsY[Lm1]) - (CoordsX[Lm3] - CoordsX[Lm1]) * (CoordsY[Lm2] - CoordsY[Lm1])) / 2.0;
        //        //lock (lockThis)
        //        Sk[i] = LSk;
        //        // расчитываем геометрию элемента 
        //        // производные dL/dx и dL/dy
        //        double Lb1 = (CoordsY[Lm2] - CoordsY[Lm3]) / (2 * LSk);
        //        double Lb2 = (CoordsY[Lm3] - CoordsY[Lm1]) / (2 * LSk);
        //        double Lb3 = (CoordsY[Lm1] - CoordsY[Lm2]) / (2 * LSk);
        //        double Lc1 = (CoordsX[Lm3] - CoordsX[Lm2]) / (2 * LSk);
        //        double Lc2 = (CoordsX[Lm1] - CoordsX[Lm3]) / (2 * LSk);
        //        double Lc3 = (CoordsX[Lm2] - CoordsX[Lm1]) / (2 * LSk);
        //        // записывем производные L по х и y в массивы 
        //        b1[i] = Lb1;
        //        b2[i] = Lb2;
        //        b3[i] = Lb3;
        //        c1[i] = Lc1;
        //        c2[i] = Lc2;
        //        c3[i] = Lc3;
        //    }
        //    //}
        //    //заполняю массив касательных для массива приграничных нижних треугольников
        //    Sx = new double[BTriangles.Length];
        //    Sy = new double[BTriangles.Length];
        //    uint[] Knot = null;
        //    int ch = 0, idx = 0;
        //    int end = BottomKnots.Length - 1;
        //    for (int i = 0; i < BTriangles.Length; i++)
        //    {
        //        Knot = AreaElems[BTriangles[i]];
        //        ch = 0;
        //        for (int j = 0; j < BottomKnots.Length; j++)
        //        {
        //            if (Knot[0] == BottomKnots[j])
        //            {
        //                ch++;
        //                idx = j;
        //            }
        //            if (Knot[1] == BottomKnots[j])
        //            {
        //                ch++;
        //                idx = j;
        //            }
        //            if (Knot[2] == BottomKnots[j])
        //            {
        //                ch++;
        //                idx = j;
        //            }
        //            if (ch == 2)
        //                break;
        //        }
        //        double[] SxSy = new double[2];
        //        //если треугольник не лежит на дне
        //        if (ch == 1)
        //        {
        //            if (idx == end)
        //                SxSy = ShearLine(idx - 1, idx);
        //            else if (idx == 0)
        //                SxSy = ShearLine(idx, idx + 1);
        //            else
        //                SxSy = ShearLine(idx - 1, idx + 1);

        //        }
        //        //если треугольник лежит на дне
        //        if (ch == 2)
        //        {
        //            if (idx != 0)
        //                SxSy = ShearLine(idx - 1, idx);

        //            else
        //            {
        //                ch = ch;
        //            }
        //        }
        //        Sx[i] = SxSy[0];
        //        Sy[i] = SxSy[1];
        //    }
        //    #endregion
        //}

        /// <summary>
        /// Метод перенумерации сетки
        /// </summary>
        public void Renumberation()
        {
            int CountKnot = CoordsX.Length;
            //if (Numbers==null)
            //    Numbers = RenumberationTemplate();
            //
            //приводим нумерацию снизу вверх от левого нижнего угла
            //к виду нумерации слева направо от левого верхнего узла
            Numbers = new int[CountKnot];
            int ch = 0;
            for (int i = 0; i < CountLeft; i++)
                for (int j = 0; j < CountBottom; j++)
                    Numbers[ch++] = CountLeft * (j + 1) - 1 - i;

            double[] tmpX = new double[CountKnot];
            for (int i = 0; i < CountKnot; i++)
                tmpX[i] = CoordsX[i];
            //
            double[] tmpY = new double[CountKnot];
            for (int i = 0; i < CountKnot; i++)
                tmpY[i] = CoordsY[i];
            //
            for (int i = 0; i < CountKnot; i++)
            {
                int n = Numbers[i];
                CoordsX[n] = tmpX[i];
                CoordsY[n] = tmpY[i];
            }
            tmpX = null;
            tmpY = null;
            // изменяем нумерацию для всей остальной структуры
            StructureChanging(Numbers);
            Numbers = null;


        }

        /// <summary>
        /// перенумерация неструктурированной сетки
        /// </summary>
        /// <returns></returns>
        private int[] RenumberationTemplate()
        {
            // перенумерация
            int CountKnot = CoordsX.Length;
            int[] Numbers = new int[CountKnot];
            ////////
            int ix, iy;
            //
            List<int>[,] XMap = new List<int>[CountKnot, CountKnot];
            for (ix = 0; ix < CountKnot; ix++) // по Х
                for (iy = 0; iy < CountKnot; iy++) // по CoordsY
                    XMap[ix, iy] = new List<int>();
            // Подготовка контейнера
            double MaxX = CoordsX.Max();
            double MinX = CoordsX.Min();
            double MaxY = CoordsY.Max();
            double MinY = CoordsY.Min();
            //
            double dx = (MaxX - MinX) / ((double)CountKnot - 1);
            double dy = (MaxY - MinY) / ((double)CountKnot - 1);
            // хеширование узлов
            for (int i = 0; i < CountKnot; i++)
            {
                //VMeshMapKnot *Knot = &Coords[ i ];
                ix = (int)((CoordsX[i] - MinX) / dx);
                iy = (int)((CoordsY[i] - MinY) / dy);
                XMap[ix, iy].Add(i);
            }
            // Новые нумера узлов
            Numbers = new int[CountKnot];
            int NewIndex = 0;
            // Получение новый номеров узлов    
            for (ix = 0; ix < CountKnot; ix++) // по Х
            {
                for (iy = 0; iy < CountKnot; iy++) // по CoordsY
                {
                    int CountX = XMap[ix, iy].Count;
                    for (int i = 0; i < CountX; i++) // по CoordsY
                    {
                        int Old = XMap[ix, iy][i];
                        Numbers[Old] = NewIndex;
                        NewIndex++;
                    }
                }
            }
            return Numbers;
        }
        private void StructureChanging(int[] NewNumb)
        {
            // **************** Создание нового массива обхода ******************
            // перебор по всем КЭ второй сетки
            for (int i = 0; i < AreaElems.Length; i++)
            {
                // перенумерация
                for (int j = 0; j < AreaElems[i].Length; j++)
                {
                    uint old = AreaElems[i][j];
                    AreaElems[i][j] = (uint)NewNumb[old];
                    //uint ew = AreaElems[i][j];
                }
            }
            //****************  Граничные узлы  ***********************
            if (LeftKnots != null)
                for (int i = 0; i < LeftKnots.Length; i++)
                {
                    //BoundKnots[i].Knot = NewNumb[BoundKnots[i].Knot];
                    int old = LeftKnots[i];
                    LeftKnots[i] = NewNumb[old];
                    int ew = LeftKnots[i];
                    //
                    int old2 = RightKnots[i];
                    RightKnots[i] = NewNumb[old2];
                    int ew1 = RightKnots[i];
                }
            //
            if (BottomKnots != null)
                for (int i = 0; i < BottomKnots.Length; i++)
                {
                    //BoundKnots[i].Knot = NewNumb[BoundKnots[i].Knot];
                    int old = BottomKnots[i];
                    BottomKnots[i] = NewNumb[old];
                    int ew = BottomKnots[i];
                    //
                    int old2 = TopKnots[i];
                    TopKnots[i] = NewNumb[old2];
                    int ew1 = TopKnots[i];
                }
        }
        //
        /// <summary>
        /// метод формирует таблицу связности узла и окружающих его элементов
        /// </summary>
        private void VolumeCommunicate()
        {
            try
            {
                bool F_in;
                //формируется неcтруктурированная таблица связности
                for (int fe = 0; fe < CountElements; fe++)
                {
                    // получение узлов треугольника
                    uint[] Knots = AreaElems[fe];
                    for (int i = 0; i < 3; i++)
                    {
                        // флаг наполненности массива
                        F_in = false;
                        for (int j = 1; j < max_len; j++)
                        {
                            if (CVolumes[Knots[i]][j] == -1)
                            {
                                //вписываем номер КЭ в доступную ячейку
                                CVolumes[Knots[i]][j] = fe;
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
                int kk1 = 0;
                int kk2 = 0;
                int kB = 0, kT = 1;
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
                    uint[] VKnots = AreaElems[CVolumes[i][1]];
                    int j = 0;
                    for (j = 0; j < 3; j++)
                    {
                        //находим положение центрального узла
                        if (VKnots[j] == CKnot)
                            break;
                    }
                    //тогда вершина контура против часовой стрелки будет через один от нее
                    uint PrevK = VKnots[(j + 2) % 3];
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
                            int curT = CVolumes[i][j];
                            //вершины треугольника
                            VKnots = AreaElems[curT];
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
                                    PrevK = VKnots[(k + 1) % 3];
                                    flag = true;
                                    break;
                                }
                            }
                            // если нашли треугольник с PrevK
                            if (flag)
                                break;
                        }

                    }
                    #endregion
                    //
                    #region Формирование листов для ГУ
                    int sh = -1;
                    // формирование листа нижних узлов
                    for (int l = 0; l < CountBottom; l++)
                    {
                        // если текущий рассматриваемый узел принадлежит нижней границе
                        if (CKnot == BottomKnots[l])
                        {
                            // запоминаем его номер в массиве узлов нижней границы
                            sh = l;
                            break;
                        }
                    }
                    // если узел принадлежит нижней границе
                    if (sh != -1)
                    {
                        //if ((sh < 1) || (sh >= NBottom - 1))
                        //{
                        //    continue;
                        //    //если узел граничный, то аппроксимируем его через ближайший граничный верхний и нижний узел
                        //}
                        //BottomVolumeList[kk1].Add(CKnot);//первый - граничный узел
                        //
                        // записываем все треугольники, содержащие граничный узел, в массив
                        for (int k = 1; k < CVolumes[i].Length; k++)
                            BottomTriangles[kB++] = CVolumes[i][k];
                        //
                        //kk1++;
                    }
                    //
                    sh = -1;
                    //формирование листа верхних узлов
                    for (int l = 0; l < CountTop; l++)
                    {
                        // если текущий рассматриваемый узел принадлежит верхней границе
                        if (CKnot == TopKnots[l])
                        {
                            // запоминаем его номер в массиве узлов верхней границы
                            sh = l;
                            break;
                        }
                    }
                    // если узел принадлежит верхней границе
                    if (sh != -1)
                    {
                        if ((sh < 1) || (sh >= CountTop - 1))
                        {
                            continue;
                            //если узел граничный, то аппроксимируем его через ближайший граничный верхний и нижний узел
                        }
                        //// если узел принадлежит только верхней границе
                        #region Комментарии
                        //// заполняется строка массива значениями (-1)
                        //CPTop[kk1] = new int[7];
                        //for (int k = 0; k < CPTop[kk1].Length; k++)
                        //    CPTop[kk1][k] = -1;
                        ////
                        //CPTop[kk1][0] = CKnot;//первый - граничный узел
                        ////
                        //kT = 1;
                        //// прогон по треугольникам с данной вершиной
                        //for (int k = 0; k < CVolumes[i].Length - 1; k++)
                        //{
                        //    // узлы текущего треугольника
                        //    VKnots = AreaElems[CVolumes[i][k + 1]];
                        //    bool flag = false;
                        //    //если еще одна из вершин принадлежит границе, не включаем эти вершины в список
                        //    for (int m = 0; m < 3; m++)
                        //    {
                        //        if ((VKnots[m] == TopKnots[(sh - 1)]) || (VKnots[m] == TopKnots[sh + 1]))
                        //        {
                        //            flag = true;
                        //            break;
                        //        }
                        //    }
                        //    //если на границе только одна вершина треугольника, включаем остальные вершины в список
                        //    if (flag == false)
                        //    {
                        //        for (int m = 0; m < 3; m++)
                        //        {
                        //            if (VKnots[m] != CKnot)
                        //                CPTop[kk1][kT++] = VKnots[m];
                        //        }
                        //    }
                        //}
                        ////если после поиска не оказалось треугольника, не лежащего на границе
                        //// то добавляем в список третий узел этого треугольника, который не лежит на границе
                        ////        .
                        ////       /|\ 
                        ////      / | \
                        ////     /  |  \
                        ////    .---.---.
                        //if (kT == 1)
                        //{
                        //    for (int m = 0; m < 3; m++)
                        //    {
                        //        if ((VKnots[m] == TopKnots[(sh - 1)]) || (VKnots[m] == TopKnots[sh + 1]))
                        //        {
                        //            if (VKnots[(m + 1) % 3] != CPTop[kk1][0])
                        //                CPTop[kk1][kT++] = VKnots[(m + 1) % 3];
                        //            if (VKnots[(m + 2) % 3] != CPTop[kk1][0])
                        //                CPTop[kk1][kT++] = VKnots[(m + 2) % 3];
                        //            break;
                        //        }
                        //    }
                        //}
                        ////
                        //kk1++;
                        #endregion
                        // добавляем его в массив CPTop
                        AddPoints(ref CPTop, ref TopKnots, ref kk1, i, CKnot, sh);
                        // записываем все треугольники, содержащие граничный узел, в массив
                        for (int k = 1; k < CVolumes[i].Length; k++)
                            TopTriangles[kT++] = CVolumes[i][k];
                    }
                    //
                    sh = -1;
                    //формирование листа правых улов
                    for (int l = 0; l < CountRight; l++)
                    {
                        // если текущий рассматриваемый узел принадлежит правой границе
                        if (CKnot == RightKnots[l])
                        {
                            // запоминаем его номер в массиве узлов правой границы
                            sh = l;
                            break;
                        }
                    }
                    // если узел принадлежит правой границе
                    if (sh != -1)
                    {
                        if ((sh < 1) || (sh >= CountRight - 1))
                        {
                            continue;
                            //если узел граничный, то аппроксимируем его через ближайший граничный верхний и нижний узел
                        }
                        #region Комментарии
                        //// строка заполняется (-1)
                        //CPRight[kk2] = new int[7];
                        //for (int k = 0; k < 7; k++)
                        //    CPRight[kk2][k] = -1;
                        ////
                        //CPRight[kk2][0] = CKnot;//первый - граничный узел
                        ////
                        // kR = 1;
                        //// прогон по треугольникам с данной вершиной
                        //for (int k = 0; k < CVolumes[i].Length - 1; k++)
                        //{
                        //    // узлы текущего треугльника
                        //    VKnots = AreaElems[CVolumes[i][k + 1]];
                        //    bool flag = false;
                        //    //если еще одна из вершин принадлежит границе, не включаем эти вершины в список
                        //    for (int m = 0; m < 3; m++)
                        //    {
                        //        if ((VKnots[m] == RightKnots[(sh - 1)]) || (VKnots[m] == RightKnots[sh + 1]))
                        //        {
                        //            flag = true;
                        //            break;
                        //        }
                        //    }
                        //    //если на границе только одна вершина треугольника, включаем остальные вершины в список
                        //    if (flag == false)
                        //    {
                        //        for (int m = 0; m < 3; m++)
                        //        {
                        //            if (VKnots[m] != CKnot)
                        //                CPRight[kk2][kR++] = VKnots[m];
                        //        }
                        //    }
                        //}
                        //// если после поиска не оказалось треугольника, не лежащего на границе
                        //// то добавляем в список третий узел этого треугольника, который не лежит на границе
                        ////        .
                        ////       /|\ 
                        ////      / | \
                        ////     /  |  \
                        ////    .___.___.
                        //if (kR == 1)
                        //{
                        //    for (int m = 0; m < 3; m++)
                        //    {
                        //        if ((VKnots[m] == RightKnots[(sh - 1)]) || (VKnots[m] == RightKnots[sh + 1]))
                        //        {
                        //            if (VKnots[(m + 1) % 3] != CPRight[kk2][0])
                        //                CPRight[kk2][kR++] = VKnots[(m + 1) % 3];
                        //            if (VKnots[(m + 2) % 3] != CPRight[kk2][0])
                        //                CPRight[kk2][kR++] = VKnots[(m + 2) % 3];
                        //            break;
                        //        }
                        //    }
                        //}
                        ////
                        //kk2++;
                        #endregion
                        // если узел принадлежит только правой границе
                        // то добавляем его в массив CPRight
                        AddPoints(ref CPRight, ref RightKnots, ref kk2, i, CKnot, sh);
                    }
                    #endregion
                }
                #region Сортировка значений листа связности, массивов ГУ
                // сортирую и убираю повторяющиеся элементы в листе верхней границы
                SortCP(ref CPTop);
                // сортирую и убираю повторяющиеся элементы в листе правой границы
                SortCP(ref CPRight);
                //
                #region Формирование массивов для ниженго ГУ
                //убираю повторяющиеся треугольники и узлы в листах для нижнего ГУ 
                //формирую массив связности глобальной нумерации узлов нижнего ГУ и локальной для СЛАУ
                //формирую упрощенный массив хранения граничных треугольников
                //
                //убираю повторяющиеся треугольники в листе для нижнего ГУ
                List<int> tmpKnots = new List<int>();
                uint[] tmpK = null;
                int sch = 0;
                //сортировка значений
                Array.Sort(BottomTriangles, 0, kB);
                //
                for (int m = 1; m < kB; m++)
                {
                    if (BottomTriangles[m] == BottomTriangles[m - 1])
                    {
                        //удаляю повторяющиеся треугольники
                        BottomTriangles[m - 1] = -1;
                        sch++;
                        continue;
                    }
                    //добавляю все узлы текущего треугольника во временный список узлов
                    tmpK = AreaElems[BottomTriangles[m]];
                    tmpKnots.Add((int)tmpK[0]); tmpKnots.Add((int)tmpK[1]); tmpKnots.Add((int)tmpK[2]);
                }
                //
                BTriangles = Array.FindAll(BottomTriangles, i => i != -1);
                //
                //сортирую и убираю повторяющиеся узлы в листе вершин граничных треугольников нижней границы
                tmpKnots.Sort();
                for (int n = 0; n < tmpKnots.Count - 1; n++)
                {
                    if (tmpKnots[n] == tmpKnots[n + 1])
                    {
                        tmpKnots.RemoveAt(n + 1);
                        n--;
                    }
                }
                // создаю массив вершин для нижнего ГУ
                CBottom = new int[tmpKnots.Count];//глобальные номера
                CBottom = tmpKnots.ToArray();
                sch = 0;
                tmpKnots.Clear();
                //сортировка значений
                Array.Sort(TopTriangles, 0, kT);
                //
                for (int m = 1; m < kT; m++)
                {
                    if (TopTriangles[m] == TopTriangles[m - 1])
                    {
                        //удаляю повторяющиеся треугольники
                        TopTriangles[m - 1] = -1;
                        sch++;
                        continue;
                    }
                    //добавляю все узлы текущего треугольника во временный список узлов
                    tmpK = AreaElems[TopTriangles[m]];
                    tmpKnots.Add((int)tmpK[0]); tmpKnots.Add((int)tmpK[1]); tmpKnots.Add((int)tmpK[2]);
                }
                //
                TTriangles = Array.FindAll(TopTriangles, i => i != -1);
                //
                //сортирую и убираю повторяющиеся узлы в листе вершин граничных треугольников нижней границы
                tmpKnots.Sort();
                for (int n = 0; n < tmpKnots.Count - 1; n++)
                {
                    if (tmpKnots[n] == tmpKnots[n + 1])
                    {
                        tmpKnots.RemoveAt(n + 1);
                        n--;
                    }
                }
                // создаю массив вершин для нижнего ГУ
                CTop = new int[tmpKnots.Count];//глобальные номера
                CTop = tmpKnots.ToArray();
                sch = 0;
                #endregion
                //убираю граничные узлы из массива связности КО---???
                int knot = 0;
                for (int i = 0; i < CountKnots; i++)
                {
                    for (int j = 0; j < CountLeft; j++)
                    {
                        knot = LeftKnots[j];
                        if (CVolumes[i][0] == knot)
                        {
                            CVolumes[i][0] = -1; sch++;
                            break;
                        }
                    }
                    for (int j = 0; j < CountRight; j++)
                    {
                        knot = RightKnots[j];
                        if (CVolumes[i][0] == knot)
                        {
                            CVolumes[i][0] = -1; sch++;
                            break;
                        }
                    }
                    //
                    for (int j = 0; j < CountBottom; j++)
                    {
                        knot = BottomKnots[j];
                        if (CVolumes[i][0] == knot)
                        {
                            CVolumes[i][0] = -1; sch++;
                            break;
                        }
                    }
                    for (int j = 0; j < CountTop; j++)
                    {
                        knot = TopKnots[j];
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
                #endregion
            }
            catch (Exception ex)
            {
                error = error + "VolumeCommunicate " + ex.Message;
            }
        }
        /// <summary>
        /// Заполняет массив связности КО без узлов, в которых задано WallFunc и массив узлов WallKnots
        /// </summary>
        /// <param name="surf_flag"></param>
        public void MakeWallFuncStructure(bool surf_flag)
        {
            int key = 0;
            int ch = 0, p0 = 0, ch_wall = 0, idx=0;
            try
            {
                if (!surf_flag)
                {
                    CV2 = new int[CVolumes.Length - 2 * (CountBottom - 2)][];
                    CV_WallKnots = new int[2 * (CountBottom - 2)][];
                    CV_WallKnotsDistance = new double[2 * (CountBottom - 2)];
                    //
                    BWallDistance = new double[CountBottom];

                    TWallDistance = new double[CountTop];
                    for (int i = 0; i < CVolumes.Length; i++)
                    {
                        key = i;
                        p0 = CVolumes[i][0];
                        // нижняя граница
                        if ((p0 - 1) % CountLeft == 0)
                        {
                            CV_WallKnots[ch_wall] = CVolumes[i];
                            CV_WallKnotsDistance[ch_wall] = GetNormalDistanceBottom(p0);
                            idx = (p0 - 1) / CountLeft;
                            BWallDistance[idx] = CV_WallKnotsDistance[ch_wall];
                            ch_wall++;
                            continue;
                        }
                        // верхняя граница
                        if ((p0 + 2) % CountLeft == 0)
                        {
                            CV_WallKnots[ch_wall] = CVolumes[i];
                            CV_WallKnotsDistance[ch_wall] = GetNormalDistanceTop(p0);
                            //
                            TWallDistance[(p0 + 2) / CountLeft - 1] = CV_WallKnotsDistance[ch_wall];
                            ch_wall++;
                            continue;
                        }

                        CV2[ch] = CVolumes[i];
                        ch++;

                    }
                    TWallDistance[0] = Math.Sqrt((CoordsY[LeftKnots[0]] - CoordsY[LeftKnots[1]]) * (CoordsY[LeftKnots[0]] - CoordsY[LeftKnots[1]]) + (CoordsX[LeftKnots[0]] - CoordsX[LeftKnots[1]]) * (CoordsX[LeftKnots[0]] - CoordsX[LeftKnots[1]]));
                    TWallDistance[CountTop - 1] = Math.Sqrt((CoordsY[TopKnots[0]] - CoordsY[TopKnots[0] - 1]) * (CoordsY[TopKnots[0]] - CoordsY[TopKnots[0] - 1]) + (CoordsX[TopKnots[0]] - CoordsX[TopKnots[0] - 1]) * (CoordsX[TopKnots[0]] - CoordsX[TopKnots[0] - 1]));

                }
                else
                {
                    CV2 = new int[CVolumes.Length - (CountBottom - 2)][];
                    CV_WallKnots = new int[CountBottom - 2][];
                    CV_WallKnotsDistance = new double[CountBottom - 2];
                    //
                    BWallDistance = new double[CountBottom];
                    for (int i = 0; i < CVolumes.Length; i++)
                    {
                        p0 = CVolumes[i][0];
                        if ((p0 - 1) % CountLeft != 0)
                        {
                            CV2[ch] = CVolumes[i];
                            ch++;
                        }
                        else
                        {
                            CV_WallKnots[ch_wall] = CVolumes[i];
                            CV_WallKnotsDistance[ch_wall] = GetNormalDistanceBottom(p0);
                            //
                            BWallDistance[(p0 - 1) / CountLeft] = CV_WallKnotsDistance[ch_wall];
                            ch_wall++;
                            continue;
                        }

                    }
                }
                BWallDistance[0] = Math.Sqrt((CoordsY[1] - CoordsY[0]) * (CoordsY[1] - CoordsY[0]) + (CoordsX[1] - CoordsX[0]) * (CoordsX[1] - CoordsX[0]));
                BWallDistance[CountBottom - 1] = Math.Sqrt((CoordsY[RightKnots[1]] - CoordsY[RightKnots[0]]) * (CoordsY[RightKnots[1]] - CoordsY[RightKnots[0]]) + (CoordsX[RightKnots[1]] - CoordsX[RightKnots[0]]) * (CoordsX[RightKnots[1]] - CoordsX[RightKnots[0]]));
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
        }

        ///// <summary>
        ///// Заполняет массив связности КО без узлов, в которых задано WallFunc и массив узлов WallKnots
        ///// </summary>
        ///// <param name="surf_flag"></param>
        //public void MakeWallFuncStructure(bool surf_flag)
        //{
        //    try
        //    {
        //        if (!surf_flag)
        //        {
        //            CV2 = new int[CVolumes.Length - 2 * (CountBottom - 2)][];
        //            CV_WallKnots = new int[2 * (CountBottom - 2)][];
        //            CV_WallKnotsDistance = new double[2 * (CountBottom - 2)];
        //            //
        //            BWallDistance = new double[CountBottom];
        //            TWallDistance = new double[CountTop];
        //            int ch = 0, p0 = 0, ch_wall = 0;
        //            for (int i = 0; i < CVolumes.Length; i++)
        //            {
        //                p0 = CVolumes[i][0];
        //                // нижняя граница
        //                if ((p0 - 1) % CountLeft == 0)
        //                {
        //                    CV_WallKnots[ch_wall] = CVolumes[i];
        //                    CV_WallKnotsDistance[ch_wall] = GetNormalDistanceBottom(p0);
        //                    //
        //                    BWallDistance[(p0 - 1) / CountLeft] = CV_WallKnotsDistance[ch_wall];
        //                    ch_wall++;
        //                    continue;
        //                }
        //                // верхняя граница
        //                if ((p0 + 2) % CountLeft == 0)
        //                {
        //                    CV_WallKnots[ch_wall] = CVolumes[i];
        //                    CV_WallKnotsDistance[ch_wall] = GetNormalDistanceTop(p0);
        //                    //
        //                    TWallDistance[(p0 + 2) / CountLeft - 1] = CV_WallKnotsDistance[ch_wall];
        //                    ch_wall++;
        //                    continue;
        //                }

        //                CV2[ch] = CVolumes[i];
        //                ch++;

        //            }
        //            TWallDistance[0] = Math.Sqrt((CoordsY[LeftKnots[0]] - CoordsY[LeftKnots[1]]) *
        //                                         (CoordsY[LeftKnots[0]] - CoordsY[LeftKnots[1]]) +
        //                                         (CoordsX[LeftKnots[0]] - CoordsX[LeftKnots[1]]) *
        //                                         (CoordsX[LeftKnots[0]] - CoordsX[LeftKnots[1]]));
        //            TWallDistance[CountTop - 1] = Math.Sqrt((CoordsY[TopKnots[0]] - CoordsY[TopKnots[0] - 1]) *
        //                                                    (CoordsY[TopKnots[0]] - CoordsY[TopKnots[0] - 1]) +
        //                                                    (CoordsX[TopKnots[0]] - CoordsX[TopKnots[0] - 1]) *
        //                                                    (CoordsX[TopKnots[0]] - CoordsX[TopKnots[0] - 1]));

        //        }
        //        else
        //        {
        //            CV2 = new int[CVolumes.Length - (CountBottom - 2)][];
        //            CV_WallKnots = new int[CountBottom - 2][];
        //            CV_WallKnotsDistance = new double[CountBottom - 2];
        //            //
        //            BWallDistance = new double[CountBottom];
        //            int ch = 0, p0 = 0, ch_wall = 0;
        //            for (int i = 0; i < CVolumes.Length; i++)
        //            {
        //                p0 = CVolumes[i][0];
        //                if ((p0 - 1) % CountLeft != 0)
        //                {
        //                    CV2[ch] = CVolumes[i];
        //                    ch++;
        //                }
        //                else
        //                {
        //                    CV_WallKnots[ch_wall] = CVolumes[i];
        //                    CV_WallKnotsDistance[ch_wall] = GetNormalDistanceBottom(p0);
        //                    //
        //                    BWallDistance[(p0 - 1) / CountLeft] = CV_WallKnotsDistance[ch_wall];
        //                    ch_wall++;
        //                    continue;
        //                }

        //            }
        //        }
        //        BWallDistance[0] = Math.Sqrt((CoordsY[1] - CoordsY[0]) * (CoordsY[1] - CoordsY[0]) + (CoordsX[1] - CoordsX[0]) * (CoordsX[1] - CoordsX[0]));
        //        BWallDistance[CountBottom - 1] = Math.Sqrt((CoordsY[RightKnots[1]] - CoordsY[RightKnots[0]]) * (CoordsY[RightKnots[1]] - CoordsY[RightKnots[0]]) + (CoordsX[RightKnots[1]] - CoordsX[RightKnots[0]]) * (CoordsX[RightKnots[1]] - CoordsX[RightKnots[0]]));
        //    }
        //    catch(Exception ex)
        //    {
        //        Logger.Instance.Exception(ex);
        //    }
        //}

        /// <summary>
        /// метод находит длину нормали от пристенночной точки ко дну
        /// </summary>
        /// <param name="p0">пристеночная точка (не лежит на границе)</param>
        /// <returns></returns>
        public double GetNormalDistanceBottom(int p0)
        {
            double n_length0 = 100000, n_lengthMin = 100000;
            //
            double Xm = CoordsX[p0], Ym = CoordsY[p0], Xf = 0, Yf = 0;
            // регулярная сетка, определяем порядковый номер в массиве
            int n = p0 / CountLeft, knot0 = 0, knot1 = 0;
            double X0 = 0, X1 = 0, Y0 = 0, Y1 = 0, nx = 0, ny = 0, k = 0, nydnx = 0;
            double max = 0, min = 0;
            for (int i = n - 10; i < n + 10; i++)
            {
                if (i <= 0 || i >= CountBottom)
                    continue;
                knot0 = BottomKnots[i - 1];
                knot1 = BottomKnots[i];
                X0 = CoordsX[knot0]; X1 = CoordsX[knot1];
                Y0 = CoordsY[knot0]; Y1 = CoordsY[knot1];
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
            //double l = Math.Sqrt((Xm - CoordsX[p0 + 1]) * (Xm - CoordsX[p0 + 1]) + (Ym - CoordsY[p0 + 1]) * (Ym - CoordsY[p0 + 1]));
            double l = Ym - CoordsY[(p0 / CountLeft) * CountLeft];
            return Math.Min(n_lengthMin, l);
        }
        //
        public double GetNormalDistanceTop(int p0)
        {
            double n_length0 = 100000, n_lengthMin = 100000;
            //
            double Xm = CoordsX[p0], Ym = CoordsY[p0], Xf = 0, Yf = 0;
            // регулярная сетка, определяем порядковый номер в массиве
            int n = CountTop - (p0 + 1) / CountLeft, knot0 = 0, knot1 = 0;
            double X0 = 0, X1 = 0, Y0 = 0, Y1 = 0, nx = 0, ny = 0, k = 0, nydnx = 0;
            double max = 0, min = 0;
            for (int i = n - 10; i < n + 10; i++)
            {
                if (i < 0 || i >= (CountTop - 1))
                    continue;
                knot0 = TopKnots[i + 1];
                knot1 = TopKnots[i];
                X0 = CoordsX[knot0]; X1 = CoordsX[knot1];
                Y0 = CoordsY[knot0]; Y1 = CoordsY[knot1];
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
        /// <summary>
        /// Метод добавляет в масив CP узлы, имеющие общую грань с CKnot и не лежащие на грнице BoundaryBottom
        /// </summary>
        /// <param name="CP">массив граничных связных узлов</param>
        /// <param name="BoundaryKnot">массив соответствующих граничных точек</param>
        /// <param name="kk2">номер в массиве CP для записи</param>
        /// <param name="i"> глобальный номер (перебор по CVolumes)</param>
        /// <param name="CKnot">текущий рассматриваемый узел </param>
        /// <param name="sh">номер текущего узла в массиве BoundaryKnot</param>
        private void AddPoints(ref int[][] CP, ref int[] BoundaryKnot, ref int kk2, int i, int CKnot, int sh)
        {
            // строка заполняется (-1)
            CP[kk2] = new int[7];
            for (int k = 0; k < 7; k++)
                CP[kk2][k] = -1;
            //
            CP[kk2][0] = CKnot;//первый - граничный узел
            //
            int kR = 1;
            uint[] VKnots = new uint[3];
            // прогон по треугольникам с данной вершиной
            for (int k = 0; k < CVolumes[i].Length - 1; k++)
            {
                // узлы текущего треугльника
                VKnots = AreaElems[CVolumes[i][k + 1]];
                bool flag = false;
                //если еще одна из вершин принадлежит границе, не включаем эти вершины в список
                for (int m = 0; m < 3; m++)
                {
                    if ((VKnots[m] == BoundaryKnot[(sh - 1)]) || (VKnots[m] == BoundaryKnot[sh + 1]))
                    {
                        flag = true;
                        break;
                    }
                }
                //если на границе только одна вершина треугольника, включаем остальные вершины в список
                if (flag == false)
                {
                    for (int m = 0; m < 3; m++)
                    {
                        if (VKnots[m] != CKnot)
                            CP[kk2][kR++] = (int)VKnots[m];
                    }
                }
            }
            // если после поиска не оказалось треугольника, не лежащего на границе
            // то добавляем в список третий узел этого треугольника, который не лежит на границе
            //        .
            //       /|\ 
            //      / | \
            //     /  |  \
            //    .___.___.
            if (kR == 1)
            {
                for (int m = 0; m < 3; m++)
                {
                    if ((VKnots[m] == BoundaryKnot[(sh - 1)]) || (VKnots[m] == BoundaryKnot[sh + 1]))
                    {
                        if (VKnots[(m + 1) % 3] != CP[kk2][0])
                            CP[kk2][kR++] = (int)VKnots[(m + 1) % 3];
                        if (VKnots[(m + 2) % 3] != CP[kk2][0])
                            CP[kk2][kR++] = (int)VKnots[(m + 2) % 3];
                        break;
                    }
                }
            }
            //
            kk2++;
        }
        /// <summary>
        /// Сортировка значений в списке CP свзяности граничного узла с другими узлами, с которыми он образует грань
        /// </summary>
        /// <param name="CP">неотсортированный список связности граничных узлов и его граней</param>
        private void SortCP(ref int[][] CP)
        {
            // сортирую и убираю повторяющиеся элементы в листе границы
            for (int m = 0; m < CP.Length; m++)
            {
                //-------------------------------------------------???  можно сделать сортировку по убыванию, выборку, а затем reverse, но еще дольше будет.
                Array.Sort(CP[m], 1, CP[m].Length - 1);
                // перенос строки CPTop в буфер tmp
                int[] tmp = new int[7]; int pp = 1;
                tmp[0] = CP[m][0];
                //
                for (int n = 1; n < CP[m].Length; n++)
                {
                    // узлы без повторений копируются в tmp
                    if (CP[m][n] != -1)
                        if (CP[m][n - 1] != CP[m][n])
                            tmp[pp++] = CP[m][n];
                }
                // переопределяется размер CPTop
                CP[m] = new int[pp];
                // переписываются с CPTop вершины без повторений
                for (int k = 0; k < pp; k++)
                    CP[m][k] = tmp[k];
            }
        }

        private int SharedGrane(int p0, int v1, int v2)
        {
            uint p1 = (uint)p0;
            //
            uint[] knotsV1 = AreaElems[v1];
            uint[] knotsV2 = AreaElems[v2];
            //
            for (int k = 0; k < 3; k++)
            {
                if (knotsV1[0] == knotsV2[k])
                    p1 = knotsV1[0];
                else if (knotsV1[1] == knotsV2[k])
                    p1 = knotsV1[1];
                else if (knotsV1[2] == knotsV2[k])
                    p1 = knotsV1[2];
                if (p1 != p0)
                    break;
            }
            return (int)p1;
        }
        //
        private double[] ShearLine(int idx_a, int idx_b)
        {
            int a = BottomKnots[idx_a];
            int b = BottomKnots[idx_b];
            double ss = Math.Sqrt((CoordsX[b] - CoordsX[a]) * (CoordsX[b] - CoordsX[a]) + (CoordsY[b] - CoordsY[a]) * (CoordsY[b] - CoordsY[a]));
            double[] SxSy = new double[2];
            SxSy[0] = 1.0 / ss * (CoordsX[b] - CoordsX[a]);
            SxSy[1] = 1.0 / ss * (CoordsY[b] - CoordsY[a]);
            return SxSy;
        }
        /// <summary>
        /// поблочное соединение равных четырехугольных сеток, образующее четырехугольник (четырехугольник + четырехугольник)
        /// </summary>
        /// <param name="NMesh"></param>
        public void Add(KsiMesh2017 NMesh)
        {
            double Eps = 0.00001; // точность float 6-7 знаков после запятой
            //
            // создание временного массива для хранения
            int[] Conform = new int[NMesh.CountKnots];
            bool[] Check = new bool[NMesh.CountKnots];
            int aPoint, bPoint, idx = 0;
            for (int i = 0; i < NMesh.CountKnots; i++)
            {
                Check[i] = true; Conform[i] = i;
            }
            #region Попарное сравнивнение левых граней с правыми и верхние с нижними двух сеток
            //определение общей границы левая-правая, правая-левая
            bool LR = false, RL = false, TB = false, BT = false;
            for (int i = 0; i < CountLeft; i++)
            {
                aPoint = LeftKnots[i];
                for (int j = 0; j < NMesh.CountRight; j++)
                {
                    bPoint = NMesh.RightKnots[j];
                    //
                    if (Math.Abs(CoordsX[aPoint] - NMesh.CoordsX[bPoint]) < Eps
                        && Math.Abs(CoordsY[aPoint] - NMesh.CoordsY[bPoint]) < Eps)
                    {
                        Conform[bPoint] = aPoint;
                        Check[bPoint] = false;
                        idx++;
                        break;
                    }
                    if (idx > 3)
                        LR = true;
                }
            }
            if (idx < 2)
            {
                for (int i = 0; i < CountRight; i++)
                {
                    aPoint = RightKnots[i];
                    for (int j = 0; j < NMesh.CountLeft; j++)
                    {
                        bPoint = NMesh.LeftKnots[j];
                        //
                        if (Math.Abs(CoordsX[aPoint] - NMesh.CoordsX[bPoint]) < Eps
                            && Math.Abs(CoordsY[aPoint] - NMesh.CoordsY[bPoint]) < Eps)
                        {
                            Conform[bPoint] = aPoint;
                            Check[bPoint] = false;
                            idx++;
                            break;
                        }
                        if (idx > 3)
                            RL = true;
                    }
                }
            }
            if (idx < 2)
            {
                for (int i = 0; i < CountTop; i++)
                {
                    aPoint = TopKnots[i];
                    for (int j = 0; j < NMesh.CountBottom; j++)
                    {
                        bPoint = NMesh.BottomKnots[j];
                        //
                        if (Math.Abs(CoordsX[aPoint] - NMesh.CoordsX[bPoint]) < Eps
                            && Math.Abs(CoordsY[aPoint] - NMesh.CoordsY[bPoint]) < Eps)
                        {
                            Conform[bPoint] = aPoint;
                            Check[bPoint] = false;
                            idx++;
                            break;
                        }
                        if (idx > 3)
                            TB = true;
                    }
                }
            }
            if (idx < 2)
            {
                for (int i = 0; i < CountBottom; i++)
                {
                    aPoint = BottomKnots[i];
                    for (int j = 0; j < NMesh.CountTop; j++)
                    {
                        bPoint = NMesh.TopKnots[j];
                        //
                        if (Math.Abs(CoordsX[aPoint] - NMesh.CoordsX[bPoint]) < Eps
                            && Math.Abs(CoordsY[aPoint] - NMesh.CoordsY[bPoint]) < Eps)
                        {
                            Conform[bPoint] = aPoint;
                            Check[bPoint] = false;
                            idx++;
                            break;
                        }
                        if (idx > 3)
                            BT = true;
                    }
                }
            }
            #endregion
            //если нет общих граней, то выходим из метода
            if (idx < 2)
                return;
            //
            //Если сетка соединяется по горизонтали, то меняем нумерацию
            // чтобы она шла построчно по слитой сетке, а не по секторам
            int[] ConformL = new int[CountKnots]; // отображение для левой сетки
            int NumberCount = 0;
            // граничные КО и КЭ находятся по результирующей сетке
            //
            #region Формирование массивов граничных узлов и перенумерация структуры
            if (LR || RL)
            {
                KsiMesh2017 MeshLeft, MeshRight;
                if (LR)
                {
                    MeshLeft = NMesh;
                    MeshRight = this;
                }
                else
                {
                    MeshLeft = this;
                    MeshRight = NMesh;
                }
                //
                // нумерация левой половины сетки
                int Nx = CountBottom + NMesh.CountBottom - 1;
                int ch = 0;
                for (int i = 0; i < CountLeft; i++)
                    for (int j = 0; j < CountBottom; j++)
                    {
                        ConformL[ch++] = Nx * i + j;
                        NumberCount++;
                    }
                //применение отображения к структуре элементов и граничных узлов
                Conforming(MeshLeft, ConformL);
                //нумерация правой половины сетки
                ch = 0;
                for (int i = 0; i < NMesh.CountLeft; i++)
                    for (int j = 0; j < NMesh.CountBottom; j++)
                    {
                        if (Check[ch] == true)
                        {
                            Conform[ch] = Nx * i + CountBottom + j - 1;
                            NumberCount++;
                        }
                        else
                        {
                            Conform[ch] = ConformL[Conform[ch]];
                        }
                        ch++;
                    }
                //применение отображения к структуре элементов и граничных узлов
                Conforming(MeshRight, Conform);
                //формирование массивов левой и правой границы
                for (int i = 0; i < CountLeft; i++)
                {
                    LeftKnots[i] = MeshLeft.LeftKnots[i];
                    RightKnots[i] = MeshRight.RightKnots[i];
                }
                // формирование массивов узлов нижней и верхней границы
                int[] tmpBottom = new int[MeshLeft.CountBottom + MeshRight.CountBottom - 1];
                int[] tmpTop = new int[MeshLeft.CountTop + MeshRight.CountTop - 1];
                for (int i = 0; i < MeshLeft.CountBottom; i++)
                {
                    tmpBottom[i] = MeshLeft.BottomKnots[i];
                }
                int e = MeshLeft.CountBottom;
                for (int i = 1; i < MeshRight.CountBottom; i++)
                {
                    tmpBottom[e++] = MeshRight.BottomKnots[i];
                }
                for (int i = 0; i < MeshRight.CountTop; i++)
                {
                    tmpTop[i] = MeshRight.TopKnots[i];
                }
                e = MeshRight.CountTop;
                for (int i = 1; i < MeshLeft.CountTop; i++)
                {
                    tmpTop[e++] = MeshLeft.TopKnots[i];
                }
                this.BottomKnots = tmpBottom;
                this.CountBottom = tmpBottom.Length;
                this.TopKnots = tmpTop;
                this.CountTop = tmpTop.Length;
                //
                // перенумерация координат
                double[] tmpX = new double[NumberCount];
                double[] tmpY = new double[NumberCount];
                for (int i = 0; i < CountKnots; i++)
                {
                    tmpX[ConformL[i]] = MeshLeft.CoordsX[i];
                    tmpY[ConformL[i]] = MeshLeft.CoordsY[i];
                }
                int kn = CountKnots;
                for (uint i = 0; i < NMesh.CountKnots; i++)
                    if (Check[i] == true)
                    {
                        tmpX[Conform[i]] = MeshRight.CoordsX[i];
                        tmpY[Conform[i]] = MeshRight.CoordsY[i];
                    }
                CoordsX = tmpX; CoordsY = tmpY;
            }
            //если верхняя и нижняя границы совпадают
            if (TB || BT)
            {
                KsiMesh2017 MeshTop, MeshBottom;
                if (BT)
                {
                    MeshTop = this;
                    MeshBottom = NMesh;
                }
                else
                {
                    MeshTop = NMesh;
                    MeshBottom = this;
                }
                //
                NumberCount = 0;
                // отображение для верхней сетки
                for (uint i = 0; i < MeshTop.CountKnots; i++)
                    ConformL[i] = NumberCount++;
                //применение отображения к структуре элементов и граничных узлов
                Conforming(MeshTop, ConformL);
                // отображение для нижней сетки
                for (uint i = 0; i < MeshBottom.CountKnots; i++)
                {
                    if (Check[i] == true)
                    {
                        Conform[i] = NumberCount;
                        NumberCount++;
                    }
                    else
                    {
                        Conform[i] = ConformL[Conform[i]];
                    }
                }
                // применение отображения к структуре элементов и граничных узлов
                Conforming(MeshBottom, Conform);
                // формирование массивов узлов верхней и нижней границы
                for (int i = 0; i < CountTop; i++)
                {
                    TopKnots[i] = MeshTop.TopKnots[i];
                    BottomKnots[i] = MeshBottom.BottomKnots[i];
                }
                // формирование массивов узлов левой и правой границы
                int[] tmpLeft = new int[MeshTop.CountLeft + MeshBottom.CountLeft - 1];
                int[] tmpRight = new int[MeshTop.CountRight + MeshBottom.CountRight - 1];
                for (int i = 0; i < MeshTop.CountLeft; i++)
                {
                    tmpLeft[i] = MeshTop.LeftKnots[i];
                }
                int e = MeshTop.CountLeft;
                for (int i = 1; i < MeshBottom.CountLeft; i++)
                {
                    tmpLeft[e++] = MeshBottom.LeftKnots[i];
                }
                for (int i = 0; i < MeshBottom.CountRight; i++)
                {
                    tmpRight[i] = MeshBottom.RightKnots[i];
                }
                e = MeshBottom.CountRight;
                for (int i = 1; i < MeshTop.CountRight; i++)
                {
                    tmpRight[e++] = MeshTop.RightKnots[i];
                }
                this.LeftKnots = tmpLeft;
                this.CountLeft = tmpLeft.Length;
                this.RightKnots = tmpRight;
                this.CountRight = tmpRight.Length;
                //
                // перенумерация координат
                double[] tmpX = new double[NumberCount];
                double[] tmpY = new double[NumberCount];
                for (int i = 0; i < CountKnots; i++)
                {
                    tmpX[i] = MeshTop.CoordsX[i];
                    tmpY[i] = MeshTop.CoordsY[i];
                }
                int kn = CountKnots;
                for (uint i = 0; i < NMesh.CountKnots; i++)
                    if (Check[i] == true)
                    {
                        tmpX[kn] = MeshBottom.CoordsX[i];
                        tmpY[kn++] = MeshBottom.CoordsY[i];
                    }
                CoordsX = tmpX; CoordsY = tmpY;
            }
            #endregion
            //
            //расширение массива конечных элементов
            int fe = 0;
            uint[][] tmpAreaElems = new uint[CountElements + NMesh.CountElements][];
            for (uint i = 0; i < AreaElems.Length; i++)
            {
                tmpAreaElems[fe] = new uint[3];
                tmpAreaElems[fe][0] = AreaElems[i][0];
                tmpAreaElems[fe][1] = AreaElems[i][1];
                tmpAreaElems[fe++][2] = AreaElems[i][2];
            }
            for (uint i = 0; i < NMesh.AreaElems.Length; i++)
            {
                tmpAreaElems[fe] = new uint[3];
                tmpAreaElems[fe][0] = NMesh.AreaElems[i][0];
                tmpAreaElems[fe][1] = NMesh.AreaElems[i][1];
                tmpAreaElems[fe++][2] = NMesh.AreaElems[i][2];
            }
            AreaElems = tmpAreaElems;
        }
        /// <summary>
        /// Метод переопределяет нумерацию одной их сливаемых сеток
        /// </summary>
        /// <param name="NMesh">Сетка, в которой нужно поменять нумерацию</param>
        /// <param name="Conform">Массив нумерации, которая должна бытьт в итоге на сетке NMesh</param>
        private static void Conforming(KsiMesh2017 NMesh, int[] Conform)
        {
            // перебор по всем КЭ второй сетки и исправление номеров узлов
            int CountTwoFE = NMesh.AreaElems.Length;
            for (uint i = 0; i < CountTwoFE; i++)
                for (int j = 0; j < 3; j++)
                    NMesh.AreaElems[i][j] = (uint)Conform[NMesh.AreaElems[i][j]];
            //
            //перенумерация граничных узлов
            for (int i = 0; i < NMesh.CountLeft; i++)
                NMesh.LeftKnots[i] = Conform[NMesh.LeftKnots[i]];
            for (uint i = 0; i < NMesh.CountBottom; i++)
                NMesh.BottomKnots[i] = Conform[NMesh.BottomKnots[i]];
            for (int i = 0; i < NMesh.CountRight; i++)
                NMesh.RightKnots[i] = Conform[NMesh.RightKnots[i]];
            for (int i = 0; i < NMesh.CountTop; i++)
                NMesh.TopKnots[i] = Conform[NMesh.TopKnots[i]];
        }
        //
        #endregion

    }
}
