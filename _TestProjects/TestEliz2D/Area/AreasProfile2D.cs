
namespace TestEliz2D
{
    using System;
    using MemLogLib;
    using System.ComponentModel;
    

    public class AreasProfile2D : SimpleAreaProfile
    {
        //                             -->
        // XYTop   ________________________________________________________________
        //        | topLayer   |                topLayer           |  topLayer     |
        //        |            |                                   |               |
        //   XYt  |------------|-----------------------------------|---------------|
        // ||     |            |                                   |               |  
        // \/     |            |                                   |               |  ||
        //        |            |                                   |               |  \/
        //   XYb  | -----------|-----------------------------------|---------------|
        //        |bottomLayer |     bottomLayer                   | bottomLayer   |
        //XYBottom ________________________________________________________________
        //                                                   
        //                                   ->
        //

        /// <summary>
        /// X-координаты первой промежуточной границы  
        /// </summary>
        public double[] Xt;
        /// <summary>
        /// Y-координаты первой промежуточной границы
        /// </summary>
        public double[] Yt;
        /// <summary>
        /// X-координаты второй промежуточной границы  
        /// </summary>
        public double[] Xb;
        /// <summary>
        /// Y-координаты второй промежуточной границы
        /// </summary>
        public double[] Yb;
        /// <summary>
        /// Толщина слоя сгущения на верхней границе расчетной области
        /// </summary>
        protected double topLayer = 0;
        /// <summary>
        /// Толщина слоя сгущения на нижней границе расчетной области
        /// </summary>
        protected double bottomLayer = 0;
        public AreasProfile2D()
        { }
        /// <summary>
        /// Получение Всей области едиными массивами XBottom, YBottom, XTop, YTop, если все сегменты генерируются одним методом
        /// </summary>
        /// <param name="L1">Длина области установления на входе</param>
        /// <param name="L2">Длина срединной области</param>
        /// <param name="L3">Длина области установления на выходе</param>
        /// <param name="H">Глубина области</param>
        /// <param name="Nx">Количество узлов по дну</param>
        /// <param name="absBed">Тип геомтерии срединной части</param>
        /// <param name="bottomLayer">Толщина слоя сгущения сетки на дне</param>
        /// <param name="topLayer">Толщина слоя сгущения на свободной поверхности</param>
        public AreasProfile2D(double L1, double L2, double L3, double H, int Nx, AbsBed absBed, double bottomLayer = 0, double topLayer = 0)
        {
            this.Nx = Nx;
            this.bottomLayer = bottomLayer;
            this.topLayer = topLayer;
            //
            double dx = (L1 + L2 + L3) / (Nx - 1);
            int Nx1 = (int)(L1 / dx) + 1;
            int Nx2 = (int)(L2 / dx) + 1;
            int Nx3 = (int)(L3 / dx) + 1;
            // ошибка округдения компенсируется за счет средней части
            if (Nx1 + Nx2 + Nx3 != (Nx + 2))
                Nx2 += Nx + 2 - Nx1 - Nx2 - Nx3;
            //
            double[][] XTopsAll = new double[3][];
            double[][] YTopsAll = new double[3][];
            double[][] XBottomsAll = new double[3][];
            double[][] YBottomsAll = new double[3][];
            //
            GenerateRectangleArea(ref XBottomsAll[0], ref YBottomsAll[0], ref XTopsAll[0], ref YTopsAll[0], Nx1, 0, L1, H);
            GenerateMiddleArea(ref XBottomsAll[1], ref YBottomsAll[1], ref XTopsAll[1], ref YTopsAll[1], Nx2, L1, L2, H, absBed);
            GenerateRectangleArea(ref XBottomsAll[2], ref YBottomsAll[2], ref XTopsAll[2], ref YTopsAll[2], Nx3, (L1 + L2), L3, H);
            //
            MEM.Alloc(Nx, ref XBottom);
            MEM.Alloc(Nx, ref YBottom);
            MEM.Alloc(Nx, ref XTop);
            MEM.Alloc(Nx, ref YTop);
            //
            XBottomsAll[0].CopyTo(XBottom, 0);
            XBottomsAll[1].CopyTo(XBottom, Nx1 - 1);
            XBottomsAll[2].CopyTo(XBottom, Nx1 + Nx2 - 2);
            //
            YBottomsAll[0].CopyTo(YBottom, 0);
            YBottomsAll[1].CopyTo(YBottom, Nx1 - 1);
            YBottomsAll[2].CopyTo(YBottom, Nx1 + Nx2 - 2);
            //
            XTopsAll[0].CopyTo(XTop, 0);
            XTopsAll[1].CopyTo(XTop, Nx1 - 1);
            XTopsAll[2].CopyTo(XTop, Nx1 + Nx2 - 2);
            //
            YTopsAll[0].CopyTo(YTop, 0);
            YTopsAll[1].CopyTo(YTop, Nx1 - 1);
            YTopsAll[2].CopyTo(YTop, Nx1 + Nx2 - 2);
            //
            GenerateMiddleSurfaces();
        }
        //
        public SimpleAreaProfile[] GetSimpleAreas()
        {
            SimpleAreaProfile[] simpleAreaProfiles = new SimpleAreaProfile[3];
            if (topLayer!=0)
                simpleAreaProfiles[0] = new SimpleAreaProfile(Xt, Yt, XTop, YTop);
            simpleAreaProfiles[1] = new SimpleAreaProfile(Xb, Yb, Xt, Yt);
            if (bottomLayer!=0)
                simpleAreaProfiles[2] = new SimpleAreaProfile(XBottom, YBottom, Xb, Yb);
            return simpleAreaProfiles;
        }
        /// <summary>
        /// расчет координат промежуточных поверхностей
        /// </summary>
        /// <param name="Nx">количество узлов по Х</param>
        /// <param name="TopLayer">Толщина верхнего подслоя</param>
        /// <param name="BottomLayer">Толщина нижнейго подслоя</param>
        public void GenerateMiddleSurfaces()
        {
            //
            if (bottomLayer != 0)
            {
                Xb = new double[Nx];
                Yb = new double[Nx];
                for (int i = 0; i < Nx; i++)
                {
                    Xb[i] = XBottom[i];
                    Yb[i] = YBottom[i] + bottomLayer;
                }
            }
            else
            {
                Xb = XBottom;
                Yb = YBottom;
            }
            //
            if (topLayer != 0)
            {
                Xt = new double[Nx];
                Yt = new double[Nx];
                for (int i = 0; i < Nx; i++)
                {
                    Xt[i] = XTop[i];
                    Yt[i] = YTop[i] - topLayer;
                }
            }
            else
            {
                Xt = XTop;
                Yt = YTop;
            }
        }
        public void GenerateRectangleArea(ref double[] _XBottom, ref double[] _YBottom, ref double[] _XTop, ref double[] _YTop, int Nx, double X0, double L, double H)
        {
            MEM.Alloc(Nx, ref _XBottom);
            MEM.Alloc(Nx, ref _YBottom);
            MEM.Alloc(Nx, ref _XTop);
            MEM.Alloc(Nx, ref _YTop);
            double dx = L / (Nx - 1);
            //
            for (int i = 0; i < Nx; i++)
            {
                _XBottom[i] = X0 + i * dx;
                _YBottom[i] = 0;
                //
                _XTop[i] = X0 + i * dx;
                _YTop[i] = H;
            }
        }
        //
        public void GenerateMiddleArea(ref double[] _XBottom, ref double[] _YBottom, ref double[] _XTop, ref double[] _YTop, int Nx, double X0, double L, double H, AbsBed BedType)
        {
            double dx = L / (Nx - 1);
            //
            BedType.GenerateBed(ref _XBottom, ref _YBottom, Nx, X0, L);
            //
            MEM.Alloc(Nx, ref _XTop);
            MEM.Alloc(Nx, ref _YTop);
            //
            for (int i = 0; i < Nx; i++)
            {
                _XTop[i] = X0 + i * dx;
                _YTop[i] = H;
            }
        }
    }
    //
    public class SimpleAreaProfile
    {
        public int Nx;
        /// <summary>
        /// X-координаты точек верхней границы  
        /// </summary>
        public double[] XTop;
        /// <summary>
        /// Y-координаты точек верхней границы
        /// </summary>
        public double[] YTop;
        /// <summary>
        /// X-координаты точек нижней границы  
        /// </summary>
        public double[] XBottom;
        /// <summary>
        /// Y-координаты точек нижней границы
        /// </summary>
        public double[] YBottom;
        //
        public SimpleAreaProfile()
        { }
        public SimpleAreaProfile(double[] XBottom, double[] YBottom, double[] XTop, double[] YTop)
        {
            this.XTop = XTop;
            this.YTop = YTop;
            this.XBottom = XBottom;
            this.YBottom = YBottom;
            this.Nx = XBottom.Length;
        }
    }
    //
    public abstract class AbsBed
    {
        protected AbsBedFormParam bp;
        public abstract string Name
        {
            get;
        }
        //
        public AbsBed() { }
        //
        public AbsBed(AbsBedFormParam bp)
        {
            this.bp = bp;
        }
        ///// <summary>
        /// Генератор координат дна
        /// </summary>
        /// <param name="X">массив Х координат</param>
        /// <param name="Y">массив Y координат</param>
        /// <param name="Nx">количество узлов по Х</param>
        /// <param name="X0">точка начала серединной части дна</param>
        /// <param name="L">длина серердинной части дна</param>
        public abstract void GenerateBed(ref double[] X, ref double[] Y, int Nx, double X0, double L);

        public static explicit operator AbsBed(Type v)
        {
            throw new NotImplementedException();
        }
        //

    }

    public class FlatBed : AbsBed
    {    // /////////////////////////////////////////не переименовывать классы Bed и Param!!!! от имени зависит положение в листбоксе контрола
        public override string Name
        { get { return "Плоское дно"; } }
        //
        public FlatBed() { }
        public FlatBed(AbsBedFormParam bp) : base(bp)
        { }
        public override void GenerateBed(ref double[] X, ref double[] Y, int Nx, double X0, double L)
        {
            MEM.Alloc(Nx, ref X);
            MEM.Alloc(Nx, ref Y);
            double dx = L / (Nx - 1);
            //
            for (int i = 0; i < Nx; i++)
            {
                X[i] = X0 + i * dx;
                Y[i] = 0;
            }
        }

    }

    public class SinBed : AbsBed
    {    // /////////////////////////////////////////не переименовывать классы Bed и Param!!!! от имени зависит положение в листбоксе контрола
        /// <summary>
        /// Параметры функции Y=A*Sin(B * (x+C))+ D
        /// </summary>
        double A, C, D;
        int B;
        //
        public override string Name
        { get { return "Синусоидальное дно"; } }
        public SinBed() { }

        public SinBed(AbsBedFormParam bp)
        {
            SinBedParam p = (SinBedParam)bp;
            A = p.A;
            B = p.B;
            C = p.C;
            D = p.D;
        }
        public SinBed(double Amplitude, int PickCount, double PhaseShift, double VerticalShift)
        {
            A = Amplitude;
            B = PickCount;
            C = PhaseShift;
            D = VerticalShift;
        }
        //
        public override void GenerateBed(ref double[] X, ref double[] Y, int Nx, double X0, double L)
        {
            MEM.Alloc(Nx, ref X);
            MEM.Alloc(Nx, ref Y);
            double dx = L / (Nx - 1);
            //
            for (int i = 0; i < Nx; i++)
            {
                X[i] = X0 + i * dx;
                Y[i] = A * Math.Sin(Math.PI / 2.0 * B * (i * dx + C)) + D;
            }

        }

    }
    //
    public class CovernBed : AbsBed
    {    // /////////////////////////////////////////не переименовывать классы Bed и Param!!!! от имени зависит положение в листбоксе контрола
        /// <summary>
        ///    
        //  -----.                 .
        //   h   |\               /|
        //       | \             / |
        // ------|  \___________/  |
        //       |  |           |  |
        //       <-> <---------> <->
        //        L1    L2       L3
        //
        /// </summary>
        double L1, L2, L3, h;
        public override string Name
        {
            get { return "Каверна"; }
        }
        public CovernBed() { }
        //
        public CovernBed(AbsBedFormParam bp)
        {
            CovernBedParam p = (CovernBedParam)bp;
            this.L1 = p.L1;
            this.L2 = p.L2;
            this.L3 = p.L3;
            this.h = p.h;
        }
        /// <summary>
        /// параметры каверны
        /// </summary>
        /// <param name="L1">проекция длины левого склона на ось Х</param>
        /// <param name="L2">длина донной части</param>
        /// <param name="L3">проекция длины правого склона на ось Х</param>
        /// <param name="h">глубина каверны</param>
        public CovernBed(double L1, double L2, double L3, double h)
        {
            this.L1 = L1;
            this.L2 = L2;
            this.L3 = L3;
            this.h = h;
        }

        public override void GenerateBed(ref double[] X, ref double[] Y, int Nx, double X0, double L = 0)
        {
            MEM.Alloc(Nx, ref X);
            MEM.Alloc(Nx, ref Y);
            L = L1 + L2 + L3;
            double dx = L / (Nx - 1);
            int N1 = (int)(L1 / dx);
            int N2 = (int)(L2 / dx);
            int N3 = (int)(L3 / dx);
            // коррекция ошибки округления за счет средней части каверны
            if ((N1 + N2 + N3) != Nx)
            {
                N2 += Nx - (N1 + N2 + N3);
            }
            //
            double dy1 = h / (N1 - 1);
            double dy3 = h / (N3 - 1);
            // левый склон
            for (int i = 0; i < N1; i++)
            {
                X[i] = X0 + i * dx;
                Y[i] = 0 - i * dy1;
            }
            //дно каверны
            X0 = X[N1 - 1];
            double Y0 = Y[N1 - 1];
            for (int i = 0; i < N2; i++)
            {
                X[N1 + i] = X0 + (i + 1) * dx;
                Y[N1 + i] = Y0;
            }
            X0 = X[N1 + N2 - 1];
            //правый склон
            for (int i = 0; i < N3; i++)
            {
                X[N1 + N2 + i] = X0 + (i + 1) * dx;
                Y[N1 + N2 + i] = Y0 + i * dy3;
            }
        }
    }
    //
    public class AssymerticalDuneBedKwoll : AbsBed
    {    // /////////////////////////////////////////не переименовывать классы Bed и Param!!!! от имени зависит положение в листбоксе контрола

        int CountDunes;
        int LeeAngle = 0;
        public override string Name => "Ассиметричные дюны Kwoll";
        public AssymerticalDuneBedKwoll()
        {
        }
        //
        public AssymerticalDuneBedKwoll(AbsBedFormParam bp)
        {
            AssymerticalDuneBedKwollParam p = (AssymerticalDuneBedKwollParam)bp;
            CountDunes = p.CountDunes;
            LeeAngle = p.LeeAngle;
        }
        /// <summary>
        /// Дюнное дно по работе Kwoll 2016
        /// </summary>
        /// <param name="countDunes">количество дюн (каждая по 0.9 м)</param>
        /// <param name="leeAngle">угол подветренного склона</param>
        public AssymerticalDuneBedKwoll(int countDunes, int leeAngle)
        {
            CountDunes = countDunes;
            LeeAngle = leeAngle;
        }

        public override void GenerateBed(ref double[] X, ref double[] Y, int Nx, double X0, double L = 0)
        {
            MEM.Alloc(Nx, ref X);
            MEM.Alloc(Nx, ref Y);
            L = CountDunes * 0.9;
            double dx = L / (Nx - 1);
            int PointsCount = CountDunes * 3 + 1;
            double[] y_all = { 0.03, 0.025, 0 };
            double[] x = new double[PointsCount];
            double[] y = new double[PointsCount];
            double[] x_angle = null;
            switch (LeeAngle)
            {
                case 10:
                    {
                        x_angle = new[] { 0.6164, 0.1418, 0.1418 };
                        break;
                    }
                case 20:
                    {
                        x_angle = new[] { 0.7626, 0.0687, 0.0687 };
                        break;
                    }
                case 30:
                    {
                        x_angle = new[] { 0.8134, 0.0433, 0.0433 };
                        break;
                    }


            }
            for (int i = 1; i < PointsCount; i++)
            {
                x[i] = x[i - 1] + x_angle[(i + 2) % 3];
                y[i] = y_all[(i + 2) % 3];

            }
            //
            alglib.spline1dinterpolant d;
            //
            alglib.spline1dbuildlinear(x, y, out d);
            double xl = 0;
            for (int i = 0; i < Nx; i++)
            {
                xl = i * dx;
                Y[i] = alglib.spline1dcalc(d, xl);
                X[i] = xl + X0;
            }
        }

    }
    //
    public abstract class AbsBedFormParam
    {    // /////////////////////////////////////////не переименовывать классы Bed и Param!!!! от имени зависит положение в листбоксе контрола
        public abstract string Name
        {
            get;
        }
    }
    //
    public class FlatBedParam : AbsBedFormParam
    {    // /////////////////////////////////////////не переименовывать классы Bed и Param!!!! от имени зависит положение в листбоксе контрола
        public override string Name => "Плоское дно";
    }
    //
    public class SinBedParam : AbsBedFormParam
    {    // /////////////////////////////////////////не переименовывать классы Bed и Param!!!! от имени зависит положение в листбоксе контрола
        public override string Name => "Синусоидальное дно";
        /// <summary>
        /// Параметры функции Y=A*Sin(B * (x+C))+ D
        /// </summary>
        double _A=1, _C=0, _D = 0;
        int _B=3;
        //
        [Description("Амплитуда"), Category("Параметры средней части дна"), DisplayName("A")]
        public double A
        {
            get { return _A; }
            set { _A = value; }
        }
        //
        [Description("Количество пиков"), Category("Параметры средней части дна"), DisplayName("B")]
        public int B
        {
            get { return _B; }
            set { _B = value; }
        }
        //
        [Description("Фазовый сдвиг"), Category("Параметры средней части дна"), DisplayName("C")]
        public double C
        {
            get { return _C; }
            set { _C = value; }
        }
        //
        [Description("Вертикальный сдвиг"), Category("Параметры средней части дна"), DisplayName("D")]
        public double D
        {
            get { return _D; }
            set { _D = value; }
        }
        //
    }
    //
    public class CovernBedParam : AbsBedFormParam
    {    // /////////////////////////////////////////не переименовывать классы Bed и Param!!!! от имени зависит положение в листбоксе контрола
        public override string Name => "Каверна";
        // <summary>
        ///    
        //  -----.                 .
        //   h   |\               /|
        //       | \             / |
        // ------|  \___________/  |
        //       |  |           |  |
        //       <-> <---------> <->
        //        L1    L2       L3
        //
        /// </summary>
        double _L1=0.1, _L2=0.3, _L3=0.1, _h=0.1;
        //
        [Description("Длина подветр. склона"), Category("Параметры средней части дна"), DisplayName("L1'\\'")]
        public double L1
        {
            get { return _L1; }
            set { _L1 = value; }
        }
        //
        [Description("Длина дна каврены"), Category("Параметры средней части дна"), DisplayName("L2'_'")]
        public double L2
        {
            get { return _L2; }
            set { _L2 = value; }
        }
        //
        [Description("Длина напорн. склона"), Category("Параметры средней части дна"), DisplayName("L3'/'")]
        public double L3
        {
            get { return _L3; }
            set { _L3 = value; }
        }
        //
        [Description("Глубина каверны"), Category("Параметры средней части дна"), DisplayName("h")]
        public double h
        {
            get { return _h; }
            set { _h = value; }
        }
    }

    public class AssymerticalDuneBedKwollParam : AbsBedFormParam
    {    // /////////////////////////////////////////не переименовывать классы Bed и Param!!!! от имени зависит положение в листбоксе контрола
        public override string Name => "Дюны Kwoll";
        //
        int _CountDunes=6;
        int _LeeAngle = 10;
        //
        [Description("Количество дюн"), Category("Параметры средней части дна"), DisplayName("DuneCount")]
        public int CountDunes
        {
            get { return _CountDunes; }
            set { _CountDunes = value; }
        }
        //
        [Description("Угол подветренного склона (может быть только 10, 20, 30)"), Category("Параметры средней части дна"), DisplayName("LeeAngle")]
        public int LeeAngle
        {
            get { return _LeeAngle; }
            set { _LeeAngle = value; }
        }
    }
}


    


