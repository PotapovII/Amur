//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 09.11.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshLib.RecMesh
{
    using System;

    using MemLogLib;
    using AlgebraLib;
    using CommonLib.Function;

    using GeometryLib;
    using GeometryLib.Vector;
    using CommonLib.FDM;

    /// <summary>
    /// Расчет турбулентных потоков в области c уступом
    ///                 mark=3   
    ///                   Г4
    ///           H0       H1    H2     Y (j)
    ///      *-----------*---*------*------->
    ///   L0 |     Ny0   |               .
    ///      |           |               .
    ///mark=0|  Nx0      |               .
    /// Г1   |           | Скорость   Г3 .  
    ///      *. . . . . .*---*------*   ANx
    ///   L1 |           . | .      |    .
    ///      *           . | .      *    .  mark=2
    ///   L2 |  LNx       . V .      |    .
    ///      *           .   .      *    .
    ///   L3 |           .   .      |    .
    ///      *-----------*---*------*    .
    ///      |        Г2    mark=1
    ///      |           Ny
    ///      V x (i)
    /// 
    ///    |---->  y(j) 
    ///    |
    ///    V x (i)
    /// </summary>
    public class QVectorMesh : IQuadMesh
    {
        /// <summary>
        /// Нижния граница стенки струи
        /// </summary>
        protected int bottomTube = 0;
        /// <summary>
        /// Длина струи
        /// </summary>
        protected int jdxTube = 0;
        /// <summary>
        /// Движение узлов сетки по дну
        /// </summary>
        public bool topBottom { get; set; }
        /// <summary>
        /// Движение узлов сетки по вертикальными границам
        /// </summary>
        public bool leftRight { get; set; }
        /// <summary>
        /// функция дна
        /// </summary>
        public IFunction1D Function { get; set; }
        /// <summary>
        /// Длина водотока на 1 участке (вход потока)
        /// </summary>
        public double Len1 { get; set; }
        /// <summary>
        /// Длина водотока на 3 участке (центр)
        /// </summary>
        public double Len2 { get; set; }
        /// <summary>
        /// Длина водотока на 3 участке (истечение)
        /// </summary>
        public double Len3 { get; set; }
        /// <summary>
        /// Глубина водотока 1 придонный участок
        /// </summary>
        public double Wen1 { get; set; }
        /// <summary>
        /// Глубина 2 участка
        /// </summary>
        public double Wen2 { get; set; }
        /// <summary>
        /// Глубина 3 участка
        /// </summary>
        public double Wen3 { get; set; }
        /// <summary>
        /// Профиль границы втекания
        /// </summary>
        public int[] startX { get; set; }
        /// <summary>
        /// Профиль границы втекания
        /// </summary>
        public int[] startY { get; set; }
        /// <summary>
        /// Профиль границы втекания
        /// </summary>
        public int[] endX { get; set; }
        /// <summary>
        /// Профиль границы втекания
        /// </summary>
        public int[] endY { get; set; }
        /// <summary>
        /// Ширина расчетной области  -> по X
        /// </summary>
        public double Lx => Wen1 + Wen2 + Wen3; 
        /// <summary>
        /// Высота расчетной области -> по Y
        /// </summary>
        public double Ly => Len1 + Len2 + Len3;
        /// <summary>
        /// Шаг сетки по Х
        /// </summary>
        double dx;
        /// <summary>
        /// Шаг сетки по Y
        /// </summary>
        double dy;
        /// <summary>
        /// Узлов по Х в широкой части канала (задаются)
        /// </summary>
        int Nx;
        /// <summary>
        /// Узлов по Y в широкой части канала (задаются)
        /// </summary>
        int Ny;
        /// <summary>
        /// Массив векторов искомых координат в узлах сетки
        /// </summary>
        Vector2[][] p;

        int imax;
        int jmax;

        #region коэффициенты схемы
        //protected double[][] Ae;
        //protected double[][] Aw;
        //protected double[][] An;
        //protected double[][] As;
        //protected double[][] Ap;
        //protected double[][] Sx;
        //protected double[][] Sy;
        double Q, P;
        /// <summary>
        /// координаты крышки или WL
        /// </summary>
        Vector2[] bottom = null;
        /// <summary>
        /// координаты дна канала
        /// </summary>
        Vector2[] top = null;

        //[NonSerialized]
        //ITPSolver pSolve = null;
        double relax = 0.3;
        /// <summary>
        /// Требования на ортогональность сетки
        /// </summary>
        double RelaxOrto = 0.6;
        /// <summary>
        /// 
        /// </summary>
        const double Tay = 0.15, mTay = 1 - Tay;
        /// <summary>
        /// Начальный узел изменяемой донной поверхности П
        /// </summary>
        int In;
        int Out;
        int count;
        #endregion
        public QVectorMesh(double Len1, double Len2, double Len3,
                           double Wen1, double Wen2, double Wen3, 
                           int Nx, int Ny, double Q = 0, double P = 0)
        {
            this.Len1 = Len1;
            this.Len2 = Len2;
            this.Len3 = Len3;
            this.Wen1 = Wen1;
            this.Wen2 = Wen2;
            this.Wen3 = Wen3;
            this.Nx = Nx;
            this.Ny = Ny;
            this.Q = Q;
            this.P = P;
            imax = Nx - 1;
            jmax = Ny - 1;
            Init();
        }

        protected void Init()
        {
           
            dx = Lx / (Nx - 1);
            dy = Ly / (Ny - 1);
            relax = Math.Min(dx / dy, dy / dx);

            //MEM.Alloc(Nx, Ny, ref p);
            //MEM.Alloc(Ny, ref profX);
            //for (int j = 0; j < Ny; j++)
            //{
            //    double y = j * dy;
            //    if (y > H0)
            //        profX[j] = Nx0;
            //    else
            //        profX[j] = 0;
            //}
            //Ny0 = (int)(H0 / dy) + 1;
        }

        /// <summary>
        /// Установка функции дна
        /// </summary>
        /// <param name="fun"></param>
        public void SetFunction(IDigFunction fun)
        {
            Function = fun;
        }

        public void InitQMesh(double L, double H, bool topBottom, bool leftRight, IDigFunction function = null)
        {
            //double aX = this.L / L;
            //double aY = this.H / H;
            //this.L0 = L0 / aX;
            //this.L1 = L1 / aX;
            //this.L2 = L2 / aX;
            //this.L3 = L3 / aX;
            //this.H0 = H0 / aY;
            //this.H1 = H1 / aY;
            //this.H2 = H2 / aY;
            Init();
            InitQMesh(topBottom,leftRight,function);
        }

        /// <summary>
        /// Инициализация сетки
        /// </summary>
        /// <param name="topBottom"></param>
        /// <param name="leftRight"></param>
        /// <param name="function"></param>
        public void InitQMesh(bool topBottom, bool leftRight, IDigFunction function = null)
        {
            //double[] bx = null;
            //double[] by = null;
            //this.topBottom = topBottom;
            //this.leftRight = leftRight;
            //if (function != null)
            //    Function = function;
            //if (Function == null)
            //    Function = new DigFunction(L);
            //// установка граничных условий                
            //for (int j = 0; j < Ny; j++)
            //{
            //    double y = j * dy;
            //    for (int i = profX[j]; i < Nx; i++)
            //    {
            //        double x = i * dx;
            //        // Дно
            //        p[i][j].X = x;
            //        p[i][j].Y = y;
            //        // WL
            //        p[i][j].X = x;
            //        p[i][j].Y = y;
            //    }
            //}
            //// установка граничных условий
            //LineStretch t = new LineStretch(Ny0, Q, P);
            //double[] ss = t.GetCoords(0, H0);
            //LOG.Print(" Y0 ", Vector2.GetArrayY(p), 3);
            //for (int j = 0; j < Ny0; j++)
            //{
            //    // слева
            //    p[0][j].Y = ss[Ny0 - j-1];
            //    // справа
            //    p[eNx][j].Y = ss[Ny0 -j-1];
            //}
            //LOG.Print(" Y1 ", Vector2.GetArrayY(p), 3);
            //Function.GetFunctionData(ref bx, ref by, Nx);
            //for (int i = 0; i < Nx; i++)
            //{
            //    p[i][0].Y = by[i];
            //}
            //LOG.Print(" Y2 ", Vector2.GetArrayY(p), 3);
            //for (int j = 1; j < Ny0-1; j++)
            //{
            //    for (int i = 0; i < Nx; i++)
            //    {
            //        double s = ((double)j) / (Ny0 - 1);
            //        p[i][j].Y = (1 - s) * p[i][0].Y + s * p[i][Ny0-1].Y;
            //    }
            //}
            //LOG.Print(" Y3 ", Vector2.GetArrayY(p), 3);
        }
        /// <summary>
        /// Определения индексов сетки для центральной области с деформируемым дном
        /// </summary>
        /// <param name="count">Количество узлов в деформируемой области</param>
        /// <param name="In">начальный узел области деформации</param>
        /// <param name="Out">конечный узел области деформации</param>
        /// <param name="xStart">левая граница области деформации</param>
        /// <param name="xEnd">правая граница области деформации</param>
        /// <param name="getX0">флаг учета левой границы</param>
        public void GetZetaArea(ref int count, ref int In, ref int Out,
                                double xStart, double xEnd, bool getX0)
        {
            if (count == 0)
            {
                // Определяем индекс начала зоны размыва
                if (getX0 == false)
                {
                    for (int i = 0; i < Nx; i++)
                        if (xStart <= p[i][0].X) { In = i; break; }
                }
                else
                    In = 0;
                for (int i = In; i < Nx; i++)
                    if (xEnd <= p[i][0].X) { Out = i > Nx ? Nx : i; break; }
                // Определяем количество узлов размываемого дна
                count = Out - In + 1;
                this.In = In;
                this.Out = Out;
                this.count = count;
            }
        }
        /// <summary>
        /// Вычисление координат узлов сетки методом Зейделя
        /// </summary>
        public void CalkXYI(int Count = 5, IFunction1D botton = null)
        {
            try
            {
                int ii, jj;
                double err = MEM.Error6;
                double dV = 0, maxErr = 0;
                double modY = Ly;
                if (Count == 0)
                    Count = 6 * Nx * Ny;
                double dXew, dXns, dYew, dYns;
                double xp, xe, xw, xs, xn;
                double yp, ye, yw, ys, yn;
                double xen, xwn, xes, xws;
                double yen, ywn, yes, yws;
                double Ap, Ig, Alpha, Betta, Gamma, Delta;
                //double Q = meshQ;
                //double P = meshP;
                Vector2 old;
                int index = 0;
                // Сохранение информации о форме дна
                GetBottom(botton);
                for (index = 0; index < Count; index++)
                {
                    maxErr = 0;
                    for (int i = 1; i < p.Length - 1; i++)
                    {
                        for (int j = startY[i] + 1; j < endY[i] - 1; j++)
                        {
                            xp = p[i][j].X;
                            xe = p[i + 1][j].X;
                            xw = p[i - 1][j].X;
                            xs = p[i][j - 1].X;
                            xn = p[i][j + 1].X;

                            xen = p[i + 1][j + 1].X;
                            xwn = p[i - 1][j + 1].X;
                            xes = p[i + 1][j - 1].X;
                            xws = p[i - 1][j - 1].X;

                            yen = p[i + 1][j + 1].Y;
                            ywn = p[i - 1][j + 1].Y;
                            yes = p[i + 1][j - 1].Y;
                            yws = p[i - 1][j - 1].Y;

                            yp = p[i][j].Y;
                            ye = p[i + 1][j].Y;
                            yw = p[i - 1][j].Y;
                            ys = p[i][j - 1].Y;
                            yn = p[i][j + 1].Y;

                            dXns = xn - xs;
                            dYns = yn - ys;
                            dXew = xe - xw;
                            dYew = ye - yw;

                            Alpha = 1;
                            Betta = 0;
                            Gamma = 1;
                            Delta = 0;
                            // g22
                            Alpha = 0.25 * (dXns * dXns + dYns * dYns);
                            // g12
                            Betta = RelaxOrto * 0.25 * (dXew * dXns + dYew * dYns);
                            // g11
                            Gamma = 0.25 * (dXew * dXew + dYew * dYew);
                            // определитель метрического тензора
                            Delta = 0.625 * (dXew * dYns - dXns * dYew);

                            Ig = Alpha + Gamma;

                            Ap = 1.0 / (2 * Ig);

                            xp = Ap * (Alpha * (xw + xe) + Gamma * (xn + xs) -
                                                        0.5 * Betta * (xen - xwn - xes + xws) +
                                                        0.5 * Delta * dXew * P +
                                                        0.5 * Delta * dXns * Q);

                            yp = Ap * (Alpha * (yw + ye) + Gamma * (yn + ys) -
                                                        0.5 * Betta * (yen - ywn - yes + yws) +
                                                        0.5 * Delta * dYew * P +
                                                        0.5 * Delta * dYns * Q);
                            old = p[i][j];

                            //p[i][j].X = (1 - Tay) * p[i][j].X + Tay * xp;
                            //p[i][j].Y = (1 - Tay) * p[i][j].Y + Tay * yp;
                            p[i][j].X = mTay * p[i][j].X + Tay * xp;
                            p[i][j].Y = mTay * p[i][j].Y + Tay * yp;

                            dV = Math.Max(Math.Abs(old.X - p[i][j].X), Math.Abs(old.Y - p[i][j].Y));

                            if (dV > maxErr)
                            {
                                maxErr = dV;
                                ii = i; jj = j;
                                //Logger.Instance.Info("xx [" + ii.ToString() + ", " + jj.ToString() + "]=  " + xx[i][j].ToString("F4"));
                                //Logger.Instance.Info("yy [" + ii.ToString() + ", " + jj.ToString() + "]=  " + yy[i][j].ToString("F4"));
                            }
                        }
                    }
                    // движение узлов на донной поверхности
                    //ERR.INF_NAN("xx ReverseQMesh.CalkXYI()", xx);
                    //ERR.INF_NAN("yy ReverseQMesh.CalkXYI()", yy);
                    MoveBotton(botton);
                    //ERR.INF_NAN("xx ReverseQMesh.CalkXYI()", xx);
                    //ERR.INF_NAN("yy ReverseQMesh.CalkXYI()", yy);
                    if (maxErr / Ly < err)
                        break;
                }
                //ERR.INF_NAN("xx ReverseQMesh.CalkXYI()", xx);
                //ERR.INF_NAN("yy ReverseQMesh.CalkXYI()", yy);
            }
            catch (Exception e)
            {
                Logger.Instance.Info(e.Message);
            }
        }
        /// <summary>
        /// сохранение информации о форме дна
        /// </summary>
        public void GetBottom(IFunction1D fZeta = null)
        {
            //MEM.Alloc(Nx, ref top, "GetBottom::top");
            //MEM.Alloc(Nx, ref bottom, "GetBottom::bottom");
            //// WL
            //MEM.Copy(top, p[Ny0]);
            //// дно
            //MEM.Copy(bottom, p[0]);
            //if (fZeta == null)
            //    for (int i = 0; i < Nx; i++)
            //        bottom[i].Y = fZeta.FunctionValue(bottom[i].X);
        }
        /// <summary>
        /// движение узлов вдоль донной поверхности
        /// </summary>
        public double MoveBotton(IFunction1D botton = null)
        {
            int jj = 1;
            Vector2 dS = p[1][0] - p[0][0];
            double dL = dS.Length();
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
                    // Дно
                    for (i = 1; i < (Wen1/dx); i++)
                    {
                        // -------------------- низ ------------------------------
                        // касательная к границе
                        Vector2 tau = p[i + 1][0] - p[i - 1][0];
                        // длина касательной 
                        double ntau = tau.Length();
                        // единичная касательная к границе
                        Vector2 etau = tau / ntau;
                        // вектор сеточной кривой выходящий из узла границы
                        Vector2 Vect = p[i][1] - p[i][0];
                        // скалярное произведение Vect на единичный вектор касательной
                        double delta = relax * Vector2.Dot(Vect, etau);
                        if (Math.Abs(delta) > maxDelta)
                            delta = Math.Sign(delta) * maxDelta;
                        if (mDelta < delta)
                            mDelta = delta;

                        double x = p[i][0].X + delta;

                        if (p[i + 1][0].X > 1.1 * x && p[i - 1][0].X < 0.9 *x)
                            p[i][0].X = x;
                        else
                        {
                            if (p[i + 1][0].X <= x)
                                p[i][0].X = p[i + 1][0].X - 0.1 * ntau / 2;
                            if (p[i - 1][0].X >= x)
                                p[i][0].X = p[i - 1][0].X + 0.1 * ntau / 2;
                        }
                        if (start <= i)
                        {
                            if (botton == null)
                            {
                                // Квадратичная интерполяция геометрии донной поверхности
                                for (int ii = jj; ii < Nx - 2; ii++)
                                {
                                    if (bottom[ii].X >= x)
                                    {
                                        double x0 = bottom[ii - 1].X;
                                        double x1 = bottom[ii].X;
                                        double x2 = bottom[ii + 1].X;
                                        double z0 = bottom[ii - 1].Y;
                                        double z1 = bottom[ii].Y;
                                        double z2 = bottom[ii + 1].Y;
                                        double N0 = (x - x1) * (x - x2) / ((x0 - x1) * (x0 - x2));
                                        double N1 = (x - x0) * (x - x2) / ((x1 - x0) * (x1 - x2));
                                        double N2 = (x - x0) * (x - x1) / ((x2 - x0) * (x2 - x1));
                                        // релаксация 
                                        p[i][0].Y = (N0 * z0 + N1 * z1 + N2 * z2);
                                        jj = ii > 1 ? ii - 1 : ii;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                for (int ii = jj; ii < Nx - 2; ii++)
                                {
                                    p[i][0].Y = botton.FunctionValue(x);
                                }
                            }
                        }
                    }
                }
                if (leftRight == true)
                {
                    //// Левая сторона
                    //for (j = 1; j < Ny; j++)
                    //{
                    //    // единичная касательная к границе
                    //    Vector2 ntau = new Vector2(0, -1);
                    //    // вектор сеточной кривой выходящий из узла границы
                    //    // касательная к границе
                    //    // вектор сеточной кривой выходящий из узла границы
                    //    Vector2 Vect = p[1][j] - p[0][j];
                    //    // скалярное произведение Vect на единичный вектор касательной
                    //    double delta = relax * Vector2.Dot(Vect, ntau);
                    //    p[0][j].Y = p[0][j].Y - relax * delta;
                    //}
                    
                    //// Правая сторона
                    //for (i = 1; i < Ny - 1; i++)
                    //{
                    //    // единичная касательная к границе
                    //    Vector2 ntau = new Vector2(0, 1);
                    //    // вектор сеточной кривой выходящий из узла границы
                    //    Vector2 Vect = p[Nx-2][j] - p[eNx][j];
                    //    // скалярное произведение Vect на единичный вектор касательной
                    //    double delta = relax * Vector2.Dot(Vect, ntau);
                    //    p[eNx][j].Y = p[eNx][j].Y + relax * delta;
                    //}
                }
            }
            return mDelta;
        }
        /// <summary>
        /// Получение новой границы области и формирование сетки
        /// </summary>
        /// <param name="Zeta"></param>
        public void CreateNewQMesh(double[] Zeta, int bottomTube, int jdxTube, IFunction1D botton, int NCoord = 100)
        {
            this.bottomTube = bottomTube;
            this.jdxTube = jdxTube;
            if (botton == null)
            {
                for (int i = 0; i < count; i++)
                    p[i + In][0].Y = Math.Abs(Zeta[i]) < MEM.Error9 ? 0 : Zeta[i];
            }
            else
            {
                for (int i = 0; i < count; i++)
                    p[i + In][0].Y = botton.FunctionValue(p[i + In][0].X);
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
            MEM.Alloc<double>(count, ref Zeta, "Zeta");
            for (int i = 0; i < count; i++)
                Zeta[i] = p[i + In][0].Y;
        }

        /// <summary>
        /// Конверсия генерируемой сетки в формат КО сетки
        /// </summary>
        /// <param name="xu">координаты узловых точек для скорости u</param>
        /// <param name="yv">координаты узловых точек для скорости v</param>
        /// <param name="x">центры ячеек сетки по X</param>
        /// <param name="y">центры ячеек сетки по Y</param>
        /// <param name="Hx">расстояние между узловыми точеками для скорости u</param>
        /// <param name="Hy">расстояние между узловыми точеками для скорости v</param>
        /// <param name="Dx">расстояние между центрами контрольных объемов по х</param>
        /// <param name="Dy">расстояние между центрами контрольных объемов по у</param>
        public void ConvertMeshToMesh(ref double[][] xu, ref double[][] yv,
                                    ref double[][] x, ref double[][] y, 
                                    ref double[][] Hx, ref double[][] Hy,
                                    ref double[][] Dx, ref double[][] Dy)
        {
            try
            {
                // конвертация сетки
                #region стенка справа
                // координаты узловых точек для скорости u
                //for (int i = 1; i < NY; i++)
                //    for (int j = 1; j < NX; j++)
                //        xu[i][j] = 0.5 * (yy[i][j] + yy[i][j - 1]);
                for (int i = 1; i < Nx; i++)
                    for (int j = 1; j < Ny; j++)
                        xu[i][j] = 0.5 * (p[i][j].X + p[i][j - 1].X);

                // координаты узловых точек для скорости v
                //for (int i = 0; i < NY; i++)
                //    for (int j = 1; j < NX; j++)
                //        yv[i][j] = 0.5 * (xx[i][j] + xx[i][j - 1]);
                for (int i = 0; i < Nx; i++)
                    for (int j = 1; j < Ny; j++)
                        yv[i][j] = 0.5 * (p[i][j].Y + p[i][j - 1].Y);


                // центры ячеек сетки
                for (int i = 1; i < Nx; i++)
                    for (int j = 1; j < Ny; j++)
                    {
                        x[i][j] = 0.25 * (p[i - 1][j - 1].X + p[i - 1][j].X + p[i][j - 1].X + p[i][j].X);
                        y[i][j] = 0.25 * (p[i - 1][j - 1].Y + p[i - 1][j].Y + p[i][j - 1].Y + p[i][j].Y);
                    }
                //y[0][0] = xx[0][0];
                //y[NY][0] = xx[NY - 1][0];
                //y[0][NX] = xx[0][NX - 1];
                //y[NY][NX] = xx[NY - 1][NX - 1];

                y[0][0] = p[0][0].Y;
                y[Nx][0] = p[Nx - 1][0].Y;
                y[0][Ny] = p[0][Ny - 1].Y;
                y[Nx][Ny] = p[Nx - 1][Ny - 1].Y;

                //x[0][0] = yy[0][0];
                //x[NY][0] = yy[NY - 1][0];
                //x[0][NX] = yy[0][NX - 1];
                //x[NY][NX] = yy[NY - 1][NX - 1];

                x[0][0] = p[0][0].X;
                x[Nx][0] = p[Nx - 1][0].X;
                x[0][Ny] = p[0][Ny - 1].X;
                x[Nx][Ny] = p[Nx - 1][Ny - 1].X;


                //for (int i = 1; i < NY; i++)
                //{
                //    y[i][0] = 0.5 * (xx[i][0] + xx[i - 1][0]);
                //    y[i][NX] = 0.5 * (xx[i][NX - 1] + xx[i - 1][NX - 1]);

                //    x[i][0] = 0.5 * (yy[i][0] + yy[i - 1][0]);
                //    x[i][NX] = 0.5 * (yy[i][NX - 1] + yy[i - 1][NX - 1]);
                //}
                for (int i = 1; i < Nx; i++)
                {
                    y[i][0] = 0.5 * (p[i][0].Y + p[i - 1][0].Y);
                    y[i][Ny] = 0.5 * (p[i][Ny - 1] .Y+ p[i - 1][Ny - 1].Y);

                    x[i][0] = 0.5 * (p[i][0].X + p[i - 1][0].X);
                    x[i][Ny] = 0.5 * (p[i][Ny - 1].X + p[i - 1][Ny - 1].X);
                }

                for (int j = 1; j < Ny; j++)
                {
                    y[0][j]  = 0.5 * (p[0][j].Y + p[0][j - 1].Y);
                    y[Nx][j] = 0.5 * (p[Nx - 1][j].Y + p[Nx - 1][j - 1].Y);

                    x[0][j] = 0.5 * (p[0][j].X + p[0][j - 1].X);
                    x[Nx][j] = 0.5 * (p[Nx - 1][j].X + p[Nx - 1][j - 1].X);
                }

                //for (int i = 0; i < x.Length / 2; i++)
                //{
                //    double[] buf = x[i];
                //    x[i] = x[x.Length - 1 - i];
                //    x[x.Length - 1 - i] = buf;
                //}
                //// Переворот для у
                //MEM.MReverseOrder(y);

                for (int i = 0; i < imax - 1; i++)
                {
                    for (int j = 0; j < jmax; j++)
                    {
                        // расстояние между узловыми точеками для скорости u
                        Hx[i + 1][j] = Math.Sqrt(
                            (p[i][j].Y - p[i + 1][j].Y) * (p[i][j].Y - p[i + 1][j].Y) +
                            (p[i][j].X - p[i + 1][j].X) * (p[i][j].X - p[i + 1][j].X));
                    }
                }
                // Разворот
                //int Length = Hx.Length - 1;
                //for (int i = 1; i < Length / 2; i++)
                //{
                //    double[] buf = Hx[i];
                //    Hx[i] = Hx[Hx.Length - i];
                //    Hx[Hx.Length - i] = buf;
                //}

                //for (int i = 0; i < Nx - 1; i++)
                //{
                //    for (int j = 0; j < jmax - 1; j++)
                //    {
                //        // расстояние между узловыми точеками для скорости v
                //        Hy[i][j + 1] = Math.Sqrt(
                //              (xx[i][j] - xx[i][(j + 1)]) * (xx[i][j] - xx[i][(j + 1)]) +
                //              (yy[i][j] - yy[i][(j + 1)]) * (yy[i][j] - yy[i][(j + 1)]));
                //    }
                //}
                for (int i = 0; i < Nx - 1; i++)
                {
                    for (int j = 0; j < jmax - 1; j++)
                    {
                        // расстояние между узловыми точеками для скорости v
                        Hy[i][j + 1] = Math.Sqrt(
                              (p[i][j].Y - p[i][(j + 1)].Y) * (p[i][j].Y - p[i][(j + 1)].Y) +
                              (p[i][j].X - p[i][(j + 1)].X) * (p[i][j].X - p[i][(j + 1)].Y));
                    }
                }
                // Разворот
                //Length = Hy.Length - 1;
                //for (int i = 0; i < Length / 2; i++)
                //{
                //    double[] buf = Hy[i];
                //    Hy[i] = Hy[Hx.Length - i - 2];
                //    Hy[Hx.Length - i - 2] = buf;
                //}
                for (int i = 0; i < imax; i++)
                {
                    for (int j = 0; j < jmax + 1; j++)
                    {
                        // расстояние между центрами контрольных объемов по х
                        Dx[i + 1][j] = Math.Sqrt(
                                 (x[i][j] - x[i + 1][j]) * (x[i][j] - x[i + 1][j]) +
                                 (y[i][j] - y[i + 1][j]) * (y[i][j] - y[i + 1][j]));
                    }
                }
                for (int i = 0; i < Nx; i++)
                {
                    for (int j = 0; j < jmax; j++)
                    {
                        // расстояние между центрами контрольных объемов по у
                        Dy[i][j + 1] = Math.Sqrt(
                                (x[i][j] - x[i][j + 1]) * (x[i][j] - x[i][j + 1]) +
                                (y[i][j] - y[i][j + 1]) * (y[i][j] - y[i][j + 1]));
                    }
                }
                #endregion
                //  OnOutputTest("output_Cos.txt");
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
        }

        /// <summary>
        /// Конверсия генерируемой сетки в формат КО сетки
        /// </summary>
        /// <param name="xu">координаты узловых точек для скорости u</param>
        /// <param name="yv">координаты узловых точек для скорости v</param>
        /// <param name="x">центры ячеек сетки по X</param>
        /// <param name="y">центры ячеек сетки по Y</param>
        /// <param name="Hx">расстояние между узловыми точеками для скорости u</param>
        /// <param name="Hy">расстояние между узловыми точеками для скорости v</param>
        /// <param name="Dx">расстояние между центрами контрольных объемов по х</param>
        /// <param name="Dy">расстояние между центрами контрольных объемов по у</param>
        public void StartGeometryMesh(ref double[][] xu, ref double[][] yv,
                               ref double[][] x, ref double[][] y,
                               ref double[][] Hx, ref double[][] Hy,
                               ref double[][] Dx, ref double[][] Dy)
        {

        }

        public void Print()
        {
            LOG.Print(" X ", Vector2.GetArrayX(p), 3);
            LOG.Print(" Y ", Vector2.GetArrayY(p), 3);
        }
    }
}
