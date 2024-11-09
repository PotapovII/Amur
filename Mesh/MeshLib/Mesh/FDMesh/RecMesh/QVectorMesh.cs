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
    ///    
    ///         H0       H1    H2     Y (j)
    ///    *-----------*---*------*------->
    /// L0 |     Ny0   |               .
    ///    |           |               .
    ///    |  Nx0      |               .
    ///    |           | Скорость      .  
    ///    *. . . . . .*---*------*   ANx
    /// L1 |           . | .      |    .
    ///    *           . | .      *    .
    /// L2 |  Nx       . V .      |    .
    ///    *           .   .      *    .
    /// L3 |           .   .      |    .
    ///    *-----------*---*------*    .
    ///    |            
    ///    |           Ny
    ///    V x (i)
    /// 
    ///    |---->  y(j) 
    ///    |
    ///    V x (i)
    /// </summary>
    public class QVectorMesh : IQuadMesh
    {
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
        /// Длина нижний широкой области
        /// </summary>
        double L;
        /// <summary>
        /// Вычота области 
        /// </summary>
        double H;
        /// <summary>
        /// Длина области первой части
        /// </summary>
        double L0;
        /// <summary>
        /// Длина области второй части
        /// </summary>
        double L1;
        /// <summary>
        /// Длина области третьей части
        /// </summary>
        double L2;
        /// <summary>
        /// Длина области четвертой части
        /// </summary>
        double L3;
        /// <summary>
        /// Длина L1 + L2 + L3
        /// </summary>
        double L13;
        /// <summary>
        /// Высота придонной части
        /// </summary>
        double H0;
        /// <summary>
        /// Высота центральной части
        /// </summary>
        double H1;
        /// <summary>
        /// Высота приповерхностной части
        /// </summary>
        double H2;
        /// <summary>
        /// Шаг сетки по Х
        /// </summary>
        double dx;
        /// <summary>
        /// Шаг сетки по Y
        /// </summary>
        double dy;
        /// <summary>
        /// Узлов в длинной части
        /// </summary>
        int Nx0, ANx, eNx;
        int Nx;
        int Ny0, Ny;
        /// <summary>
        /// Вектор координат
        /// </summary>
        Vector2[][] p;
        /// <summary>
        /// Профиль входной стенки
        /// </summary>
        int[] profX = null;

        #region коэффициенты схемы
        protected double[][] Ae;
        protected double[][] Aw;
        protected double[][] An;
        protected double[][] As;
        protected double[][] Ap;
        protected double[][] Sx;
        protected double[][] Sy;
        double Q, P;
        Vector2[] bottom = null;
        Vector2[] top = null;

        [NonSerialized]
        ITPSolver pSolve = null;
        double relax = 0.3;
        double RelaxOrto = 0.9;
        int In;
        int Out;
        int count;
        #endregion
        public QVectorMesh(double L0, double L1, double L2, double L3,
            double H0, double H1, double H2, int Nx, int Ny, double Q = 0, double P = 0)
        {
            this.L0 = L0;
            this.L1 = L1;
            this.L2 = L2;
            this.L3 = L3;
            this.H0 = H0;
            this.H1 = H1;
            this.H2 = H2;
            this.Nx = Nx;
            this.Ny = Ny;
            this.Q = Q;
            this.P = P;
            L13 = L1 + L2 + L3;
            H = H0 + H1 + H2;
            dx = L13 / (Nx - 1);
            dy = H / (Ny - 1);
            relax = Math.Min(dx / dy, dy / dx);
            Nx0 = (int)(L0 / dx);
            L0 = dx * Nx0;
            L = L0 + L13;
            ANx = Nx + Nx0;
            eNx = ANx - 1;
            MEM.Alloc(ANx, Ny, ref p);
            MEM.Alloc(Ny, ref profX);
            for (int j = 0; j < Ny; j++)
            {
                double y = j * dy;
                if (y > H0)
                    profX[j] = Nx0;
                else
                    profX[j] = 0;
            }
            Ny0 = (int)(H0 / dy) + 1;
        }
        /// <summary>
        /// Установка функции дна
        /// </summary>
        /// <param name="fun"></param>
        public void SetFunction(IDigFunction fun)
        {
            Function = fun;
        }
        /// <summary>
        /// Инициализация сетки
        /// </summary>
        /// <param name="topBottom"></param>
        /// <param name="leftRight"></param>
        /// <param name="function"></param>
        public void InitQMesh(bool topBottom, bool leftRight, IDigFunction function = null)
        {
            double[] bx = null;
            double[] by = null;
            this.topBottom = topBottom;
            this.leftRight = leftRight;
            if (function != null)
                Function = function;
            if (Function == null)
                Function = new DigFunction(L);
            // установка граничных условий                
            for (int j = 0; j < Ny; j++)
            {
                double y = j * dy;
                for (int i = profX[j]; i < ANx; i++)
                {
                    double x = i * dx;
                    // Дно
                    p[i][j].X = x;
                    p[i][j].Y = y;
                    // WL
                    p[i][j].X = x;
                    p[i][j].Y = y;
                }
            }
            // установка граничных условий
            LineStretch t = new LineStretch(Ny0, Q, P);
            double[] ss = t.GetCoords(0, H0);
            LOG.Print(" Y0 ", Vector2.GetArrayY(p), 3);
            for (int j = 0; j < Ny0; j++)
            {
                // слева
                p[0][j].Y = ss[Ny0 - j-1];
                // справа
                p[eNx][j].Y = ss[Ny0 -j-1];
            }
            LOG.Print(" Y1 ", Vector2.GetArrayY(p), 3);
            Function.GetFunctionData(ref bx, ref by, ANx);
            for (int i = 0; i < ANx; i++)
            {
                p[i][0].Y = by[i];
            }
            LOG.Print(" Y2 ", Vector2.GetArrayY(p), 3);
            for (int j = 1; j < Ny0-1; j++)
            {
                for (int i = 0; i < ANx; i++)
                {
                    double s = ((double)j) / (Ny0 - 1);
                    p[i][j].Y = (1 - s) * p[i][0].Y + s * p[i][Ny0-1].Y;
                }
            }
            LOG.Print(" Y3 ", Vector2.GetArrayY(p), 3);
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
                    for (int i = 0; i < ANx; i++)
                        if (xStart <= p[i][0].X) { In = i; break; }
                }
                else
                    In = 0;
                for (int i = In; i < ANx; i++)
                    if (xEnd <= p[i][0].X) { Out = i > ANx ? ANx : i; break; }
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
                double modY = H;
                if (Count == 0)
                    Count = 6 * Nx * Ny;
                double xp, xe, xw, xs, xn;
                double yp, ye, yw, ys, yn;
                double xen, xwn, xes, xws;
                double yen, ywn, yes, yws;
                double Ap, Ig, Alpha, Betta, Gamma, Delta;
                double RelaxOrto = 0.9;
                //double Q = meshQ;
                //double P = meshP;
                double Tay = 0.15;
                Vector2 old;
                int index = 0;
                // Сохранение информации о форме дна
                GetBottom(botton);
                for (index = 0; index < Count; index++)
                {
                    maxErr = 0;
                    for (int i = 1; i < p.Length - 1; i++)
                    {
                        for (int j = 1; j < Ny0 - 1; j++)
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
                            old = p[i][j];

                            p[i][j].X = (1 - Tay) * p[i][j].X + Tay * xp;
                            p[i][j].Y = (1 - Tay) * p[i][j].Y + Tay * yp;

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
                    if (maxErr / H < err)
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
            MEM.Alloc(ANx, ref top, "GetBottom::top");
            MEM.Alloc(ANx, ref bottom, "GetBottom::bottom");
            // WL
            MEM.Copy(top, p[Ny0]);
            // дно
            MEM.Copy(bottom, p[0]);
            if (fZeta == null)
                for (int i = 0; i < ANx; i++)
                    bottom[i].Y = fZeta.FunctionValue(bottom[i].X);
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
                    for (i = 1; i < eNx; i++)
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

                        if (p[i + 1][0].X > x && p[i - 1][0].X < x)
                            p[i][0].X = x;
                        else
                        {
                            if (p[i + 1][0].X <= x)
                                p[i][0].X = p[i + 1][0].X - 0.1 * ntau;
                            if (p[i - 1][0].X >= x)
                                p[i][0].X = p[i - 1][0].X + 0.1 * ntau;
                        }
                        if (start <= i)
                        {
                            if (botton == null)
                            {
                                // Квадратичная интерполяция геометрии донной поверхности
                                for (int ii = jj; ii < ANx - 2; ii++)
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
                                for (int ii = jj; ii < ANx - 2; ii++)
                                {
                                    p[i][0].Y = botton.FunctionValue(x);
                                }
                            }
                        }
                    }
                }
                if (leftRight == true)
                {
                    // Левая сторона
                    for (j = 1; j < Ny0; j++)
                    {
                        // единичная касательная к границе
                        Vector2 ntau = new Vector2(0, -1);
                        // вектор сеточной кривой выходящий из узла границы
                        // касательная к границе
                        // вектор сеточной кривой выходящий из узла границы
                        Vector2 Vect = p[1][j] - p[0][j];
                        // скалярное произведение Vect на единичный вектор касательной
                        double delta = relax * Vector2.Dot(Vect, ntau);
                        p[0][j].Y = p[0][j].Y - relax * delta;
                    }
                    
                    // Правая сторона
                    for (i = 1; i < Ny0 - 1; i++)
                    {
                        // единичная касательная к границе
                        Vector2 ntau = new Vector2(0, 1);
                        // вектор сеточной кривой выходящий из узла границы
                        Vector2 Vect = p[ANx-2][j] - p[eNx][j];
                        // скалярное произведение Vect на единичный вектор касательной
                        double delta = relax * Vector2.Dot(Vect, ntau);
                        p[eNx][j].Y = p[eNx][j].Y + relax * delta;
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


        public void Print()
        {
            LOG.Print(" X ", Vector2.GetArrayX(p), 3);
            LOG.Print(" Y ", Vector2.GetArrayY(p), 3);
        }
    }
}
