﻿namespace MeshLib
{
    using AlgebraLib;
    using MemLogLib;
    using GeometryLib;
    using System;
    using CommonLib.Function;

    /// <summary>
    /// Сетка в четырехугольнике с деформируемыми сторонами
    /// </summary>
    [Serializable]
    public class QMesh
    {
        /// <summary>
        /// Движение узлов сетки по горизонтальным границам
        /// </summary>
        public bool topBottom = true;
        /// <summary>
        /// Движение узлов сетки по горизонтальным границам
        /// </summary>
        public bool leftRight = false;
        /// <summary>
        /// функция дна
        /// </summary>
        IFunction1D Function = null;
        /// <summary>
        /// длина области
        /// </summary>
        public double L;
        /// <summary>
        /// высота области
        /// </summary>
        public double H;
        /// <summary>
        /// узлов по х
        /// </summary>
        public int NX;
        /// <summary>
        /// узлов по у
        /// </summary>
        public int NY;
        /// <summary>
        /// Координаты Х узлов сетки
        /// </summary>
        public double[][] xx = null;
        /// <summary>
        /// Координаты У узлов сетки
        /// </summary>
        public double[][] yy = null;
        #region коэффициенты схемы
        protected double[][] Ae;
        protected double[][] Aw;
        protected double[][] An;
        protected double[][] As;
        protected double[][] Ap;
        protected double[][] Sx;
        protected double[][] Sy;
        double Q, P;
        double[] bottomX = null;
        double[] bottomY = null;
        double[] topX = null;
        double[] topY = null;

        [NonSerialized]
        ITPSolver pSolve = null;

        double relax = 0.3;
        double RelaxOrto = 0.9;
        double dx;
        double dy;
        int In;
        int Out;
        int count;
        #endregion
        /// <summary>
        /// Установка функции дна
        /// </summary>
        /// <param name="fun"></param>
        public void SetFunction(IDigFunction fun)
        {
            Function = fun;
        }
        /// <summary>
        /// Сетка
        /// </summary>
        /// <param name="Nx"></param>
        /// <param name="Ny"></param>
        public QMesh(int Nx, int Ny, double Q = 0, double P = 0)
        {
            NX = Nx;
            NY = Ny;
            this.Q = Q;
            this.P = P;
            MEM.Alloc2D(NX, NY, ref xx);
            MEM.Alloc2D(NX, NY, ref yy);
        }
        /// <summary>
        /// Инициализация сетки
        /// </summary>
        /// <param name="L"></param>
        /// <param name="H"></param>
        /// <param name="Q"></param>
        /// <param name="P"></param>
        public void InitQMesh(double L, double H, double Q, double P, 
            bool topBottom, bool leftRight,
            bool revers = true, 
            IDigFunction function = null)
        {
            double[] bx = null;
            double[] by = null;
            this.L = L;
            this.H = H;
            this.Q = Q;
            this.P = P;
            this.topBottom = topBottom;
            this.leftRight = leftRight;
            if (function != null)
                Function = function;
            if (Function == null)
                Function = new DigFunction(L);

            this.NX = xx.Length;
            this.NY = xx[0].Length;
            int nx = NX - 1;
            int ny = NY - 1;
            LineStretch t = new LineStretch(NX, Q, P);
            double[] ss = t.GetCoords(0, H);
            // установка граничных условий                
            for (int i = 0; i < NX; i++)
            {
                // слева
                xx[i][0] = 0;
                // справа
                xx[i][ny] = L;
            }
            double stepX = L / ny;
            for (int i = 0; i < NY; i++)
            {
                double x = i * stepX;
                // верх
                xx[0][i] = x;
                // дно
                xx[nx][i] = x;
            }
            for (int i = 0; i < NY; i++)
                for (int j = 0; j < NX; j++)
                {
                    double s = ((double)j) / nx;
                    xx[j][i] = (1 - s) * xx[0][i] + s * xx[nx][i];
                }
            if (revers == true)
            {
                // установка граничных условий                
                for (int i = 0; i < NX; i++)
                {
                    // слева
                    yy[i][0] = ss[i];
                    // справа
                    yy[i][ny] = ss[i];
                }
                Function.GetFunctionData(ref bx, ref by, NY);
                for (int i = 0; i < NY; i++)
                {
                    yy[0][i] = H - by[i];
                    yy[nx][i] = by[i];
                }
                for (int i = 0; i < NY; i++)
                    for (int j = 0; j < NX; j++)
                    {
                        double s = ((double)j) / nx;
                        yy[j][i] = (1 - s) * yy[0][i] + s * yy[nx][i];
                    }
            }
            else
            {
                // установка граничных условий                
                for (int i = 0; i < NX; i++)
                {
                    // слева
                    yy[nx - i][ny] = ss[i];
                    // справа
                    yy[nx - i][0] = ss[i];
                }
                Function.GetFunctionData(ref bx, ref by, NY);
                for (int i = 0; i < NY; i++)
                {
                    yy[0][i] = H - by[i];
                    yy[nx][i] = by[i];
                }
                for (int i = 0; i < NY; i++)
                {
                    for (int j = 0; j < NX; j++)
                    {
                        double s = ((double)j) / nx;
                        yy[j][i] = (1 - s) * yy[0][i] + s * yy[nx][i];
                    }
                }
            }
        }
        /// <summary>
        /// Устанвка координат границ у квадрата
        /// </summary>
        /// <param name="functions"></param>
        public void InitQMesh(IDigFunction[] functions)
        {
            double[] bx = null;
            double[] by = null;
            this.NX = xx.Length;
            this.NY = xx[0].Length;
            int nx = NX - 1;
            int ny = NY - 1;
            // низ
            functions[0].GetFunctionData(ref bx, ref by, NY);
            for (int i = 0; i < NY; i++)
            {
                xx[nx][i] = bx[i];
                yy[nx][i] = by[i];
            }
            // слева
            functions[1].GetFunctionData(ref bx, ref by, NX);
            for (int i = 0; i < NX; i++)
            {
                xx[nx - i][ny] = bx[i];
                yy[nx - i][ny] = by[i];
            }
            // верх
            functions[2].GetFunctionData(ref bx, ref by, NY);
            for (int i = 0; i < NY; i++)
            {
                xx[0][i] = bx[i];
                yy[0][i] = by[i];
            }
            functions[3].GetFunctionData(ref bx, ref by, NX);
            // установка граничных условий                
            for (int i = 0; i < NX; i++)
            {
                // справа
                yy[nx - i][0] = bx[i];
                yy[nx - i][0] = bx[i];
            }
            for (int i = 0; i < NY; i++)
                for (int j = 0; j < NX; j++)
                {
                    double s = ((double)j) / nx;
                    yy[j][i] = (1 - s) * yy[0][i] + s * yy[nx][i];
                }
            for (int i = 0; i < NY; i++)
                for (int j = 0; j < NX; j++)
                {
                    double s = ((double)j) / nx;
                    xx[j][i] = (1 - s) * xx[0][i] + s * xx[nx][i];
                }
        }

        /// <summary>
        /// Определения индексов сетки для центральной области с деформируемым дном
        /// </summary>
        /// <param name="count"></param>
        /// <param name="In"></param>
        /// <param name="Out"></param>
        /// <param name="Len1"></param>
        /// <param name="Len2"></param>
        /// <param name="getX0"></param>
        public void GetZetaArea(ref int count, ref int In, ref int Out, double Len1, double Len2, bool getX0)
        {
            if (count == 0)
            {
                // Определяем индекс начала зоны размыва
                if (getX0 == false)
                {
                    for (int i = 0; i < NY; i++)
                        if (Len1 <= xx[NX - 1][i]) { In = i; break; }
                }
                else
                    In = 0;
                for (int i = In; i < NY; i++)
                    if (Len1 + Len2 <= xx[NX - 1][i]) { Out = i > NY ? NY : i; break; }
                // Определяем количество узлов размываемого дна
                count = Out - In + 1;
                this.In = In;
                this.Out = Out;
                this.count = count;
            }
        }
        public void ReGetZetaArea(ref int count, ref int In, ref int Out, double Len1, double Len2)
        {
            // Определяем индекс начала зоны размыва
            for (int i = 0; i < NY; i++)
                if (Len1 <= xx[NX - 1][i]) { In = i; break; }
            for (int i = In; i < NY; i++)
                if (Len1 + Len2 <= xx[NX - 1][i]) { Out = i > NY ? NY : i; break; }
            // Определяем количество узлов размываемого дна
            count = Out - In + 1;
            this.In = In;
            this.Out = Out;
            this.count = count;
        }
        /// <summary>
        /// Вычисление координат узлов сетки методом 2D прогонки
        /// </summary>
        public void CalkXY(int Count = 5)
        {
            // массивы координат сетки в
            // вспомогательной системе координат 
            //  ^ x |
            //  |   |
            //  |   V (i)
            //  |
            //  *-----------> x (j)
            // взятой из старого кода 
            this.NX = xx.Length;
            this.NY = xx[0].Length;
            MEM.Alloc2D(NX, NY, ref Ae);
            MEM.Alloc2D(NX, NY, ref Aw);
            MEM.Alloc2D(NX, NY, ref An);
            MEM.Alloc2D(NX, NY, ref As);
            MEM.Alloc2D(NX, NY, ref Ap);
            MEM.Alloc2D(NX, NY, ref Sx);
            MEM.Alloc2D(NX, NY, ref Sy);
            if (pSolve == null)
                pSolve = new TPSolver(NX, NY);
            // Сохранение информации о форме дна
            GetBottom();
            // Количество итераций
            for (int index = 0; index < Count; index++)
            {
                for (int i = 1; i < NX - 1; i++)
                {
                    for (int j = 1; j < NY - 1; j++)
                    {
                        double xe = xx[i][j + 1];
                        double xw = xx[i][j - 1];
                        double xs = xx[i + 1][j];
                        double xn = xx[i - 1][j];
                        double xen = xx[i + 1][j - 1];
                        double xwn = xx[i + 1][j + 1];
                        double xes = xx[i - 1][j - 1];
                        double xws = xx[i - 1][j + 1];
                        double yen = yy[i + 1][j - 1];
                        double ywn = yy[i + 1][j + 1];
                        double yes = yy[i - 1][j - 1];
                        double yws = yy[i - 1][j + 1];
                        double ye = yy[i][j + 1];
                        double yw = yy[i][j - 1];
                        double ys = yy[i + 1][j];
                        double yn = yy[i - 1][j];
                        // g22
                        double Alpha = 0.25 * ((xn - xs) * (xn - xs) + (yn - ys) * (yn - ys));
                        // g12
                        double Betta = RelaxOrto * 0.25 * ((xe - xw) * (xn - xs) + (ye - yw) * (yn - ys));
                        // g11
                        double Gamma = 0.25 * ((xe - xw) * (xe - xw) + (ye - yw) * (ye - yw));
                        // определитель метрического тензора
                        double Delta = 0.0625 * ((xe - xw) * (yn - ys) - (xn - xs) * (ye - yw));

                        Ae[i][j] = Alpha;
                        Aw[i][j] = Alpha;
                        An[i][j] = Gamma;
                        As[i][j] = Gamma;
                        Ap[i][j] = 2 * (Alpha + Gamma);
                        Sx[i][j] = -0.5 * Betta * (xen - xwn - xes + xws) +
                                     0.5 * Delta * (xe - xw) * P +
                                     0.5 * Delta * (xn - xs) * Q;
                        Sy[i][j] = -0.5 * Betta * (yen - ywn - yes + yws) +
                                     0.5 * Delta * (ye - yw) * P +
                                     0.5 * Delta * (yn - ys) * Q;
                    }
                }
                pSolve.OnTDMASolver(1, 1, NX - 1, NY - 1, Ae, Aw, An, As, Ap, Sx, null, ref xx, 1);
                pSolve.OnTDMASolver(1, 1, NX - 1, NY - 1, Ae, Aw, An, As, Ap, Sx, null, ref yy, 1);
                // движение узлов на донной поверхности
                MoveBotton();
            }
            // коррекция
        }
        /// <summary>
        /// Вычисление координат узлов сетки методом Зейделя
        /// </summary>
        public void CalkXYI(int Count = 5, IFunction1D botton = null)
        {
            try
            {
                int ii, jj;
                double err = 0.000001;
                double dx = 0, maxErr = 0;
                double modX = Math.Abs(xx[0][0] - xx[xx.Length - 1][xx[0].Length - 1]);
                double modY = Math.Abs(yy[0][0] - yy[xx.Length - 1][xx[0].Length - 1]);
                double mod = 0.5 * (modX + modY);
                if (Count == 0)
                    Count = 6 * NX * NY;
                double xp, xe, xw, xs, xn;
                double yp, ye, yw, ys, yn;
                double xen, xwn, xes, xws;
                double yen, ywn, yes, yws;
                double Ap, Ig, Alpha, Betta, Gamma, Delta;
                double RelaxOrto = 0.9;
                //double Q = meshQ;
                //double P = meshP;
                double Tay = 0.15;
                double oldX, oldY,newX,newY;
                int index = 0;

                // Сохранение информации о форме дна
                GetBottom(botton);
                for (index = 0; index < Count; index++)
                {
                    maxErr = 0;

                    for (int i = 1; i < xx.Length - 1; i++)
                        for (int j = 1; j < xx[0].Length - 1; j++)
                        {
                            xp = xx[i][j];
                            xe = xx[i][j + 1];
                            xw = xx[i][j - 1];
                            xs = xx[i + 1][j];
                            xn = xx[i - 1][j];

                            xen = xx[i + 1][j - 1];
                            xwn = xx[i + 1][j + 1];
                            xes = xx[i - 1][j - 1];
                            xws = xx[i - 1][j + 1];

                            yen = yy[i + 1][j - 1];
                            ywn = yy[i + 1][j + 1];
                            yes = yy[i - 1][j - 1];
                            yws = yy[i - 1][j + 1];

                            yp = yy[i][j];
                            ye = yy[i][j + 1];
                            yw = yy[i][j - 1];
                            ys = yy[i + 1][j];
                            yn = yy[i - 1][j];

                            Alpha = 1;
                            Betta = 0;
                            Gamma = 1;
                            Delta = 0;
                            RelaxOrto = 0.5;
                            // g22
                            Alpha = 0.25 * ((xn - xs) * (xn - xs) + (yn - ys) * (yn - ys));
                            // g12
                            Betta = RelaxOrto * 0.25 * ((xe - xw) * (xn - xs) + (ye - yw) * (yn - ys));
                            // g11
                            Gamma = 0.25 * ((xe - xw) * (xe - xw) + (ye - yw) * (ye - yw));
                            // определитель метрического тензора
                            Delta = 0.625 * ((xe - xw) * (yn - ys) - (xn - xs) * (ye - yw));

                            Ig = Alpha + Gamma;
                            Ap = 1.0 / (2 * Ig);

                            xp = Ap * (Alpha * (xw + xe) + Gamma * (xn + xs) -
                                                        0.5 * Betta * (xen - xwn - xes + xws) +
                                                        0.5 * Delta * (xe - xw) * P +
                                                        0.5 * Delta * (xn - xs) * Q);

                            yp = Ap * (Alpha * (yw + ye) + Gamma * (yn + ys) -
                                                        0.5 * Betta * (yen - ywn - yes + yws) +
                                                        0.5 * Delta * (ye - yw) * P +
                                                        0.5 * Delta * (yn - ys) * Q);

                            oldX = xx[i][j];
                            oldY = yy[i][j];

                            newX = (1 - Tay) * xx[i][j] + Tay * xp;
                            newY = (1 - Tay) * yy[i][j] + Tay * yp;

                            //if (double.IsInfinity(newX) == true || double.IsNaN(newX) == true ||
                            //    double.IsInfinity(newY) == true || double.IsNaN(newY) == true)
                            //{
                            //    Console.WriteLine("Ups QMesh.CalkXYI()");
                            //    break;
                            //}

                            xx[i][j] = newX;
                            yy[i][j] = newY;


                            dx = Math.Max( Math.Abs(oldX - xx[i][j]), Math.Abs(oldY - yy[i][j]) );
                            if (dx > maxErr)
                            {
                                maxErr = dx;
                                ii = i; jj = j;
                                //Logger.Instance.Info("xx [" + ii.ToString() + ", " + jj.ToString() + "]=  " + xx[i][j].ToString("F4"));
                                //Logger.Instance.Info("yy [" + ii.ToString() + ", " + jj.ToString() + "]=  " + yy[i][j].ToString("F4"));
                            }
                        }
                    // движение узлов на донной поверхности
                    //ERR.INF_NAN("xx QMesh.CalkXYI()", xx);
                    //ERR.INF_NAN("yy QMesh.CalkXYI()", yy);
                    MoveBotton(botton);
                    //ERR.INF_NAN("xx QMesh.CalkXYI()", xx);
                    //ERR.INF_NAN("yy QMesh.CalkXYI()", yy);
                    if (maxErr / mod < err)
                        break;
                }
                //ERR.INF_NAN("xx QMesh.CalkXYI()", xx);
                //ERR.INF_NAN("yy QMesh.CalkXYI()", yy);
            }
            catch (Exception e)
            {
                Logger.Instance.Info(e.Message);
            }
        }
        /// <summary>
        /// сохранение информации о форме дна
        /// </summary>
        public void GetBottom(IFunction1D botton = null)
        {
            dx = Math.Abs(xx[0][1] - xx[0][0]);
            dy = Math.Abs(yy[1][0] - yy[0][0]);
            relax = Math.Min(dx / dy, dy / dx);
            // дно
            MEM.AllocClear(NY, ref bottomX);
            MEM.AllocClear(NY, ref bottomY);
            MEM.AllocClear(NY, ref topX);
            MEM.AllocClear(NY, ref topY);
            if (botton == null)
                for (int j = 0; j < NY; j++)
                {
                    bottomX[j] = xx[NX - 1][j];
                    bottomY[j] = yy[NX - 1][j];
                }
            else
                for (int j = 0; j < NY; j++)
                {
                    bottomX[j] = xx[NX - 1][j];
                    bottomY[j] = botton.FunctionValue(bottomX[j]);
                }
            for (int j = 0; j < NY; j++)
            {
                topX[j] = xx[0][j];
                topY[j] = yy[0][j];
            }
        }


        /// <summary>
        /// движение узлов вдоль донной поверхности
        /// </summary>
        //public double MoveBotton()
        //{
        //    int jj = 1;
        //    double dX = xx[NX - 1][1] - xx[NX - 1][0];
        //    double dY = yy[NX - 1][1] - yy[NX - 1][0];
        //    double dL = Math.Sqrt(dX * dX + dY * dY);
        //    double alpha = 0.05;
        //    double maxDelta = alpha * dL;
        //    double mDelta = 0;
        //    int start = In == 0 ? 1 : In;
        //    //int startY = In == 0 ? 1 : In;
        //    int i, j;
        //    int bed = NX - 1;
            
        //    for (int m = 0; m < 1; m++)
        //    {
        //        if (topBottom == true)
        //        {
        //            // низ
        //            for (j = 1; j < NY - 1; j++)
        //            {
        //                // -------------------- низ ------------------------------
        //                // касательная к границе
        //                double tauX = xx[bed][j + 1] - xx[bed][j - 1];
        //                double tauY = yy[bed][j + 1] - yy[bed][j - 1];

        //                double tauXe = xx[bed][j]     - xx[bed][j - 1];
        //                double tauYe = yy[bed][j + 1] - yy[bed][j];
        //                double tauXw = xx[bed][j + 1] - xx[bed][j];
        //                double tauYw = yy[bed][j]     - yy[bed][j - 1];

        //                double ntauE = Math.Sqrt(tauXe * tauXe + tauYe * tauYe);
        //                double ntauW = Math.Sqrt(tauXw * tauXw + tauYw * tauYw);

        //                double minTau = Math.Min(ntauE, ntauW);

        //                // нормали 
        //                double normXe = - tauYe;
        //                double normYe = tauXe;
        //                double normXw = -tauYw;
        //                double normYw = tauXw;


        //                // Острый угол не перемещаем
        //                //if (Math.Sign(normXe) != Math.Sign(normXw)) 
        //                //    continue;

        //                // длина касательной 
        //                double ntau = Math.Sqrt(tauX * tauX + tauY * tauY);
        //                // единичная касательная к границе
        //                double ntauX = tauX / ntau;
        //                double ntauY = tauY / ntau;
        //                // вектор сеточной кривой выходящий из узла границы
        //                double VectX = xx[bed - 1][j] - xx[bed][j];
        //                double VectY = yy[bed - 1][j] - yy[bed][j];
        //                // скалярное произведение Vect на единичный вектор касательной
        //                double nVect = Math.Sqrt(VectX * ntauX + VectY * ntauY);

        //                minTau = Math.Min(minTau, nVect);
        //                double delta = relax * minTau;

                        

        //                double x = xx[bed][j] + delta;

        //                //if (0.99 * xx[bed][j + 1] > x && xx[bed][j - 1] * 1.01 < x)
        //                //    xx[NX - 1][j] = x;


        //                if (xx[bed][j + 1] > x && xx[bed][j - 1] < x)
        //                    xx[bed][j] = x;
        //                else
        //                {
        //                    if (xx[bed][j + 1] <= x)
        //                        xx[bed][j] = xx[bed][j + 1];
        //                    if (xx[bed][j - 1] >= x)
        //                        xx[bed][j] = xx[bed][j - 1];
        //                }


        //                // if (start <= j)
        //                {
        //                    // Квадратичная интерполяция геометрии донной поверхности
        //                    for (int ii = jj; ii < NX - 2; ii++)
        //                    {
        //                        // ii-1         x   ii               ii+1
        //                        // * -----------+-- * -------------- *
        //                        if (bottomX[ii] >= x)
        //                        {
        //                            double x0 = bottomX[ii - 1];
        //                            double x1 = bottomX[ii];
        //                            double x2 = bottomX[ii + 1];
        //                            double z0 = bottomY[ii - 1];
        //                            double z1 = bottomY[ii];
        //                            double z2 = bottomY[ii + 1];
        //                            double A0 = (x0 - x1) * (x0 - x2);
        //                            double A1 = (x1 - x0) * (x1 - x2);
        //                            double A2 = (x2 - x0) * (x2 - x1);

        //                            double N0 = (x - x1) * (x - x2) / A0;
        //                            double N1 = (x - x0) * (x - x2) / A1;
        //                            double N2 = (x - x0) * (x - x1) / A2;
        //                            // релаксация 
        //                            yy[bed][j] = (N0 * z0 + N1 * z1 + N2 * z2);
                                    
        //                         //   jj = ii > 1 ? ii - 1 : ii;

        //                          //  ERR.INF_NAN("yy", yy[NX - 1][j]);
        //                            break;
        //                        }
        //                    }
        //                }

                        

        //                // -------------------- верх ------------------------------
        //                // касательная к границе
        //                tauX = xx[0][j + 1] - xx[0][j - 1];
        //                tauY = yy[0][j + 1] - yy[0][j - 1];
        //                // длина касательной 
        //                ntau = Math.Sqrt(tauX * tauX + tauY * tauY);
        //                // единичная касательная к границе
        //                ntauX = tauX / ntau;
        //                ntauY = tauY / ntau;
        //                // вектор сеточной кривой выходящий из узла границы
        //                VectX = xx[1][j] - xx[0][j];
        //                VectY = yy[1][j] - yy[0][j];
        //                // скалярное произведение Vect на единичный вектор касательной
        //                delta = relax * (VectX * ntauX + VectY * ntauY);
        //                if (delta > maxDelta)
        //                    delta = maxDelta;
        //                if (mDelta < delta)
        //                    mDelta = delta;
        //                x = xx[0][j] + delta;
        //                xx[0][j] = x;
        //                // Квадратичная интерполяция геометрии донной поверхности
        //                for (int ii = jj; ii < NX - 2; ii++)
        //                {
        //                    if (topX[ii] >= x)
        //                    {
        //                        double x0 = topX[ii - 1];
        //                        double x1 = topX[ii];
        //                        double x2 = topX[ii + 1];
        //                        double z0 = topY[ii - 1];
        //                        double z1 = topY[ii];
        //                        double z2 = topY[ii + 1];
        //                        double N0 = (x - x1) * (x - x2) / ((x0 - x1) * (x0 - x2));
        //                        double N1 = (x - x0) * (x - x2) / ((x1 - x0) * (x1 - x2));
        //                        double N2 = (x - x0) * (x - x1) / ((x2 - x0) * (x2 - x1));
        //                        // релаксация 
        //                        yy[0][j] = (N0 * z0 + N1 * z1 + N2 * z2);
        //                        jj = ii > 1 ? ii - 1 : ii;
                                
        //                        ERR.INF_NAN("yy", yy[0][j]);
        //                        break;
        //                    }
        //                }
        //            }
        //        }
        //    }


        //    for (int m = 0; m < 1; m++)
        //    {
        //        if (leftRight == true)
        //        {
        //            // низ
        //            for (j = start; j < NY - 1; j++)
        //            {
        //                // Левая сторона
        //                for (i = 1; i < NX - 1; i++)
        //                {
        //                    // единичная касательная к границе
        //                    double ntauX = 0; // ntau;
        //                    double ntauY = -1; // ntau;
        //                                       // вектор сеточной кривой выходящий из узла границы
        //                    double VectX = xx[i][1] - xx[i][0];
        //                    double VectY = yy[i][1] - yy[i][0];
        //                    // скалярное произведение Vect на единичный вектор касательной
        //                    double delta = VectX * ntauX + VectY * ntauY;
        //                    yy[i][0] = yy[i][0] - relax * delta;
        //                }

        //                // Правая сторона
        //                for (i = 1; i < NY - 1; i++)
        //                {
        //                    // единичная касательная к границе
        //                    double ntauX = 0; // ntau;
        //                    double ntauY = 1; // ntau;
        //                                      // вектор сеточной кривой выходящий из узла границы
        //                    double VectX = xx[i][NX - 2] - xx[i][NX - 1];
        //                    double VectY = yy[i][NX - 2] - yy[i][NX - 1];
        //                    // скалярное произведение Vect на единичный вектор касательной
        //                    double delta = VectX * ntauX + VectY * ntauY;
        //                    yy[i][NX - 1] = yy[i][NX - 1] + relax * delta;
        //                }
        //                //ERR.INF_NAN("yy", yy[i][0]);
        //                //ERR.INF_NAN("yy", yy[i][NX - 1]);
        //            }
        //        }
        //    }

        //    return mDelta;
        //}

        /// <summary>
        /// движение узлов вдоль донной поверхности
        /// </summary>
        public double MoveBotton(IFunction1D botton = null)
        {
            int jj = 1;
            double dX = xx[NX - 1][1] - xx[NX - 1][0];
            double dY = yy[NX - 1][1] - yy[NX - 1][0];
            double dL = Math.Sqrt(dX * dX + dY * dY);
            double alpha = 0.05;
            double maxDelta = alpha * dL;
            double mDelta = 0;
            int start = In == 0 ? 1 : In;
            //int startY = In == 0 ? 1 : In;
            int i, j;
            for (int m = 0; m < 1; m++)
            {
                if (topBottom == true)
                {
                    // низ
                    for (j = 1; j < NY - 1; j++)
                    {
                        // -------------------- низ ------------------------------
                        // касательная к границе
                        double tauX = xx[NX - 1][j + 1] - xx[NX - 1][j - 1];
                        double tauY = yy[NX - 1][j + 1] - yy[NX - 1][j - 1];
                        // длина касательной 
                        double ntau = Math.Sqrt(tauX * tauX + tauY * tauY);
                        // единичная касательная к границе
                        double ntauX = tauX / ntau;
                        double ntauY = tauY / ntau;
                        // вектор сеточной кривой выходящий из узла границы
                        double VectX = xx[NX - 2][j] - xx[NX - 1][j];
                        double VectY = yy[NX - 2][j] - yy[NX - 1][j];
                        // скалярное произведение Vect на единичный вектор касательной
                        double delta = relax * (VectX * ntauX + VectY * ntauY);
                        if (Math.Abs(delta) > maxDelta)
                            delta = Math.Sign(delta) * maxDelta;
                        if (mDelta < delta)
                            mDelta = delta;

                        double x = xx[NX - 1][j] + delta;
                        if (xx[NX - 1][j + 1] > x && xx[NX - 1][j - 1] < x)
                            xx[NX - 1][j] = x;
                        else
                        {
                            if (xx[NX - 1][j + 1] <= x)
                                xx[NX - 1][j] = xx[NX - 1][j + 1];
                            if (xx[NX - 1][j - 1] >= x)
                                xx[NX - 1][j] = xx[NX - 1][j - 1];
                        }
                        if (start <= j)
                        {
                            if (botton == null)
                            {
                                // Квадратичная интерполяция геометрии донной поверхности
                                for (int ii = jj; ii < NX - 2; ii++)
                                {
                                    if (bottomX[ii] >= x)
                                    {
                                        double x0 = bottomX[ii - 1];
                                        double x1 = bottomX[ii];
                                        double x2 = bottomX[ii + 1];
                                        double z0 = bottomY[ii - 1];
                                        double z1 = bottomY[ii];
                                        double z2 = bottomY[ii + 1];
                                        double N0 = (x - x1) * (x - x2) / ((x0 - x1) * (x0 - x2));
                                        double N1 = (x - x0) * (x - x2) / ((x1 - x0) * (x1 - x2));
                                        double N2 = (x - x0) * (x - x1) / ((x2 - x0) * (x2 - x1));
                                        // релаксация 
                                        yy[NX - 1][j] = (N0 * z0 + N1 * z1 + N2 * z2);
                                        jj = ii > 1 ? ii - 1 : ii;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                for (int ii = jj; ii < NX - 2; ii++)
                                {
                                    yy[NX - 1][j] = botton.FunctionValue(x);
                                }
                            }
                        }
                        // -------------------- верх ------------------------------
                        // касательная к границе
                        tauX = xx[0][j + 1] - xx[0][j - 1];
                        tauY = yy[0][j + 1] - yy[0][j - 1];
                        // длина касательной 
                        ntau = Math.Sqrt(tauX * tauX + tauY * tauY);
                        // единичная касательная к границе
                        ntauX = tauX / ntau;
                        ntauY = tauY / ntau;
                        // вектор сеточной кривой выходящий из узла границы
                        VectX = xx[1][j] - xx[0][j];
                        VectY = yy[1][j] - yy[0][j];
                        // скалярное произведение Vect на единичный вектор касательной
                        delta = relax * (VectX * ntauX + VectY * ntauY);
                        if (delta > maxDelta)
                            delta = maxDelta;
                        if (mDelta < delta)
                            mDelta = delta;
                        x = xx[0][j] + delta;
                        xx[0][j] = x;
                        // Квадратичная интерполяция геометрии донной поверхности
                        for (int ii = jj; ii < NX - 2; ii++)
                        {
                            if (topX[ii] >= x)
                            {
                                double x0 = topX[ii - 1];
                                double x1 = topX[ii];
                                double x2 = topX[ii + 1];
                                double z0 = topY[ii - 1];
                                double z1 = topY[ii];
                                double z2 = topY[ii + 1];
                                double N0 = (x - x1) * (x - x2) / ((x0 - x1) * (x0 - x2));
                                double N1 = (x - x0) * (x - x2) / ((x1 - x0) * (x1 - x2));
                                double N2 = (x - x0) * (x - x1) / ((x2 - x0) * (x2 - x1));
                                // релаксация 
                                yy[0][j] = (N0 * z0 + N1 * z1 + N2 * z2);
                                jj = ii > 1 ? ii - 1 : ii;
                                break;
                            }
                        }
                    }
                }
                if (leftRight == true)
                {
                    // низ
                    for (j = start; j < NY - 1; j++)
                    {
                        // Левая сторона
                        for (i = 1; i < NX - 1; i++)
                        {
                            // единичная касательная к границе
                            double ntauX = 0; // ntau;
                            double ntauY = -1; // ntau;
                                               // вектор сеточной кривой выходящий из узла границы
                            double VectX = xx[i][1] - xx[i][0];
                            double VectY = yy[i][1] - yy[i][0];
                            // скалярное произведение Vect на единичный вектор касательной
                            double delta = VectX * ntauX + VectY * ntauY;
                            yy[i][0] = yy[i][0] - relax * delta;
                        }

                        // Правая сторона
                        for (i = 1; i < NY - 1; i++)
                        {
                            // единичная касательная к границе
                            double ntauX = 0; // ntau;
                            double ntauY = 1; // ntau;
                                              // вектор сеточной кривой выходящий из узла границы
                            double VectX = xx[i][NX - 2] - xx[i][NX - 1];
                            double VectY = yy[i][NX - 2] - yy[i][NX - 1];
                            // скалярное произведение Vect на единичный вектор касательной
                            double delta = VectX * ntauX + VectY * ntauY;
                            yy[i][NX - 1] = yy[i][NX - 1] + relax * delta;
                        }
                    }
                }
            }
            return mDelta;
        }


        /// <summary>
        /// Получение новой границы области и формирование сетки
        /// </summary>
        /// <param name="Zeta"></param>
        public void CreateNewQMesh(double[] Zeta, IFunction1D botton, int NCoord = 100)
        {
            int N = yy.Length - 1;
            if (botton == null)
            {
                for (int i = 0; i < count; i++)
                    yy[N][i + In] = Math.Abs(Zeta[i]) < MEM.Error9 ? 0 : Zeta[i];
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    yy[N][i + In] = botton.FunctionValue(xx[N][i + In]);
                }
            }
            // формирование граничных узлов с волнистым дном
            CalkXYI(NCoord, botton);
        }
        /// <summary>
        /// Получение текущих донных отметок
        /// </summary>
        /// <param name="Zeta"></param>
        public void GetBed(ref double[] Zeta)
        {
            int N = yy.Length - 1;
            MEM.Alloc<double>(count, ref Zeta, "Zeta");
            for (int i = 0; i < count; i++)
                Zeta[i] = yy[N][i + In];
        }
    }
}