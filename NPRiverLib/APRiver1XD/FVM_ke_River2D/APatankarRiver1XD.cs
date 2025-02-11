//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 16.02.2024 Потапов И.И.
//---------------------------------------------------------------------------
//                      Сбока No Property River Lib
//              с убранными из наследников Property классами 
//---------------------------------------------------------------------------

namespace NPRiverLib.APRiver_1XD.River2D_FVM_ke
{
    using System;
    using System.IO;
    using System.Linq;

    using CommonLib;
    using CommonLib.IO;
    using CommonLib.FDM;
    using CommonLib.Physics;
    using CommonLib.Function;
    using CommonLib.ChannelProcess;

    using MeshLib;
    using MemLogLib;
    using AlgebraLib;
    using GeometryLib;
    using NPRiverLib.AllWallFunctions;
    using NPRiverLib.IO;
    using NPRiverLib.ABaseTask;
    

    [Serializable]
    public abstract class APatankarRiver1XD : APRiver1XD<PatankarParams1XD>, IRiver
    {
        #region Константы модели
        /// <summary>
        /// Удельная теплоемкость потока 
        /// </summary>
        public double Cp = SPhysics.PHYS.Cp;
        /// <summary>
        /// Гидравлическая крупность
        /// </summary>
        public double Ws = SPhysics.PHYS.Ws;

        #region Параметры стандартной модели k-e
        public const double Cmu = 0.09;
        public const double c1 = 1.44;
        public const double c2 = 1.92;
        /// <summary>
        /// Число Прандтля для уравнения теплопроводности?  Prandtl = Пекле/Рейнольдс
        /// </summary>
        public const double Prandtl = 0.7;
        /// <summary>
        /// Число Прандтля для уравнения концентрации наносов
        /// </summary>
        public const double PrandtlС = 0.8;
        /// <summary>
        /// Число Прандтля для уравнения теплопроводности
        /// </summary>
        public const double Prandtl_T = 0.9;
        /// <summary>
        /// Число Прандтля для уравнения кин. энергии турбулентности
        /// </summary>
        public const double Prandtl_Kin = 1.0;
        /// <summary>
        /// Число Прандтля для уравнения дисспации кин. энергии турбулентности
        /// </summary>
        public const double Prandtl_dis = 1.3;
        /// <summary>
        /// Параметр для турбулентной кинетической энергии на входе в область
        /// Параметр k на входе в область
        /// </summary>
        public const double Coeff_k = 0.0005;
        /// <summary>
        /// Параметр e на входе в область
        /// </summary>
        public const double Coeff_e = 0.1;
        /// <summary>
        /// Параметр шероховатости стенки
        /// </summary>
        public const double E_wall = 9.8;
        /// <summary>
        /// Отношение чисел Прандтля
        /// </summary>
        public const double prprt = Prandtl / Prandtl_T;

        public double pfn = 9.0 * (prprt - 1.0) / Math.Sqrt(Math.Sqrt(prprt));
        public double cmu4 = Math.Sqrt(Math.Sqrt(Cmu));
        #endregion
        /// <summary>
        /// Невязка уравнения неразрывности  
        /// </summary>
        public double smax = 0;
        #endregion

        #region Поля
        public double[] Zeta0 = null;
        public double[] tau = null;
        public double[] theta = null;
        public double[] Cb_zeta = null;

        public double[] Pb = null;
        public int In;
        public int Out;
        public int count = 0;
        public double zeta_in = 0;
        public double zeta_out = 0;
        public double[] relax = new double[10];
        /// <summary>
        /// Растояние до стеки
        /// </summary>
        protected double[] xplus;
        // Массивы дискретных аналогов задачи        
        protected double[][] Ae;
        protected double[][] Aw;
        protected double[][] An;
        protected double[][] As;
        protected double[][] Ap;
        protected double[][] Ap0;
        protected double[][] sc;
        protected double[][] sp;
        protected double[][] du;
        protected double[][] dv;
        protected double[][] uu;
        protected double[][] vv;
        /// <summary>
        /// расход турбулентного потока на входе в область
        /// </summary>
	    protected double flowin;
        /// <summary>
        /// Координаты х для цетров контрольных объемов
        /// </summary>
        public double[][] x = null;
        /// <summary>
        /// Координаты y для цетров контрольных объемов
        /// </summary>
        public double[][] y = null;
        /// <summary>
        /// Координаты узловых точек для скорости u
        /// </summary>
        public double[][] xu = null;
        /// <summary>
        /// Координаты узловых точек для скорости v
        /// </summary>
        public double[][] yv = null;
        /// <summary>
        /// Расстояние между узловыми точками для скорости u
        /// или ширина контрольного объема
        /// </summary>
        protected double[][] Hx;
        /// <summary>
        /// Расстояние между узловыми точками для скорости v
        /// или высота контрольного объема
        /// </summary>
        protected double[][] Hy;
        /// <summary>
        /// Расстояние между центрами контрольных объемов по х
        /// </summary>
        protected double[][] Dx;
        /// <summary>
        /// Расстояние между центрами контрольных объемов по у
        /// </summary>
        protected double[][] Dy;
        [NonSerialized]
        protected double[] data = null;
        /// <summary>
        /// объект для расчета координт сетки в четырехугольной расчетной области
        /// </summary>
        public IQuadMesh qmesh = null;
        /// <summary>
        /// Нижния граница стенки струи
        /// </summary>
        protected int bottomTube = 0;
        /// <summary>
        /// Верхняя граница стенки струи
        /// </summary>
        protected int topTube = 0;
        /// <summary>
        /// Длина струи
        /// </summary>
        protected int jdxTube = 0;

        /// <summary>
        /// FlagStartMesh - первая генерация сетки true
        /// </summary>
        protected bool FlagStartMesh = true;
        protected double Q = 0;
        protected double P = 0;

        /// <summary>
        /// Количество контрольных объемов + 1 по направлению X 
        /// (количество узлов по i)
        /// </summary>
        protected int Nx;
        /// <summary>
        /// Количество контрольных объемов + 1 по направлению Y
        /// (количество узлов по j)
        /// </summary>
        protected int Ny;
        /// <summary>
        /// Количество контрольных объемов + 1 по направлению X 
        /// (количество узлов по i)
        /// </summary>
        public int imax;
        /// <summary>
        /// Количество контрольных объемов + 1 по направлению Y
        /// (количество узлов по j)
        /// </summary>
        public int jmax;
        /// <summary>
        /// Горизонтальная скорость
        /// </summary>
        public double[][] u;
        /// <summary>
        /// Вертикальная скорость
        /// </summary>
        public double[][] v;
        /// <summary>
        /// Давление
        /// </summary>
        public double[][] p;
        /// <summary>
        /// Поправка поля давления 
        /// </summary>
        public double[][] pc;
        /// <summary>
        /// Температура/концентрация наносов
        /// </summary>
        public double[][] t;
        /// <summary>
        /// Температура
        /// </summary>
        public double[][] phi;
        /// <summary>
        /// Плотность
        /// </summary>
        protected double[][] rho;
        /// <summary>
        /// Вязкость турбулентная
        /// </summary>
        public double[][] mut;
        /// <summary>
        /// Кинетическая энергия турбулентности
        /// </summary>
        public double[][] tke;
        /// <summary>
        /// Диссипация турбулентности
        /// </summary>
        public double[][] dis;
        /// <summary>
        /// Генерация энергии турбулентности
        /// </summary>
        public double[][] gen;
        /// <summary>
        /// Правая часть
        /// </summary>
        protected double[][] gam;
        /// <summary>
        /// Массив для индексации полей задачи
        /// </summary>
        protected double[][][] F;
        /// <summary>
        /// Файл для тестового вывода полей
        /// </summary>
        protected StreamWriter outFile = null;
        /// <summary>
        /// Решатель алгебраических уравнений - методом прогонки  
        /// использует Tri-Diagonal Matrix Algorithm
        /// </summary>
        [NonSerialized]
        protected ITPSolver pSolve = null;
        /// <summary>
        /// Решатель алгебраических уравнений 
        /// </summary>
        [NonSerialized]
        protected ITPSolver pSolveR = null;

        [NonSerialized]
        protected IFunction1D ZetaFuntion0 = null;

        public BCond bcU;
        public BCond bcV;
        public BCond bcP;
        public BCond bcT;
        public BCond bcK;
        public BCond bcE;
        public BCond bcPhi;
        public BCond[] bcF;
        #endregion

        public APatankarRiver1XD(PatankarParams1XD p) : base(p) { }

        #region методы предстартовой подготовки задачи
        /// <summary>
        /// Загрузка задачи иp форматного файла
        /// </summary>
        /// <param name="file">имя файла</param>

        public override void LoadData(StreamReader file) { }
        /// <summary>
        /// Загрузка задачи иp форматного файла
        /// </summary>
        /// <param name="file">имя файла</param>
        public override void LoadData(IDigFunction[] crossFunctions = null) { }
        /// <summary>
        /// Конфигурация задачи по умолчанию (тестовые задачи)
        /// </summary>
        /// <param name="testTaskID">номер задачи по умолчанию</param>
        public override void DefaultCalculationDomain(uint testTaskID = 0)
        {
            //          1 (W) вток    
            //i=0,j=0|-------------|--> j y  jmax
            //       |             |
            //       |             |
            //   S   | 0    _    0 |   N (верх)
            // Дно   |    _| |_    |     
            //       |    \   /    |     
            //       |     \ /     |
            //  imax |------V------| 
            //       | x    1  исток
            //       V i    E          
            // Координатная система решателя
            //             (S) дно    
            //i=0,j=0|-------------|--> j y  jmax
            //       |             |
            //       |    ----|\   |      
            //   W   |   |      |  | E 
            // Вток  |    ----|/   | исток    
            //       |             |     
            //       |             |     
            //  imax |-------------| 
            //       | x     (верх)
            //       V i    N        

            imax = Params.FV_X + 1;
            jmax = Params.FV_Y + 1;
            Nx = imax + 1;
            Ny = jmax + 1;

            MEM.Alloc2D<double>(Nx, Ny, ref x);
            MEM.Alloc2D<double>(Nx, Ny, ref xu);
            MEM.Alloc2D<double>(Nx, Ny, ref Dx);

            MEM.Alloc2D<double>(imax, Ny, ref Hx);

            MEM.Alloc<double>(jmax, ref theta);
            MEM.Alloc<double>(jmax, ref Cb_zeta);

            MEM.Alloc2D<double>(Nx, Ny, ref y);
            MEM.Alloc2D<double>(Nx, Ny, ref yv);
            MEM.Alloc2D<double>(Nx, Ny, ref Dy);
            MEM.Alloc2D<double>(Nx, jmax, ref Hy);

            MEM.Alloc2D<double>(Nx, Ny, ref Ae);
            MEM.Alloc2D<double>(Nx, Ny, ref Aw);
            MEM.Alloc2D<double>(Nx, Ny, ref An);
            MEM.Alloc2D<double>(Nx, Ny, ref As);
            MEM.Alloc2D<double>(Nx, Ny, ref Ap);
            MEM.Alloc2D<double>(Nx, Ny, ref Ap0);
            MEM.Alloc2D<double>(Nx, Ny, ref sc);
            MEM.Alloc2D<double>(Nx, Ny, ref sp);

            MEM.Alloc2D<double>(Nx, Ny, ref du);
            MEM.Alloc2D<double>(Nx, Ny, ref dv);

            MEM.Alloc2D<double>(Nx, Ny, ref uu);
            MEM.Alloc2D<double>(Nx, Ny, ref vv);

            MEM.Alloc<double>(Ny, ref xplus);
            xplus = new double[Ny];

            bcU = new BCond(Nx, Ny);
            bcV = new BCond(Nx, Ny);
            bcP = new BCond(Nx, Ny);
            bcT = new BCond(Nx, Ny);
            bcK = new BCond(Nx, Ny);
            bcE = new BCond(Nx, Ny);
            bcPhi = new BCond(Nx, Ny);

            bcF = new BCond[7] { bcU, bcV, bcP, bcT, bcK, bcE, bcPhi };

            MEM.Alloc2D<double>(Nx, Ny, ref u);
            MEM.Alloc2D<double>(Nx, Ny, ref v);
            MEM.Alloc2D<double>(Nx, Ny, ref p);
            MEM.Alloc2D<double>(Nx, Ny, ref pc);
            MEM.Alloc2D<double>(Nx, Ny, ref t);
            MEM.Alloc2D<double>(Nx, Ny, ref rho);
            MEM.Alloc2D<double>(Nx, Ny, ref mut);
            MEM.Alloc2D<double>(Nx, Ny, ref phi);


            MEM.Alloc2D<double>(Nx, Ny, ref tke);
            MEM.Alloc2D<double>(Nx, Ny, ref dis);
            MEM.Alloc2D<double>(Nx, Ny, ref gam);
            MEM.Alloc2D<double>(Nx, Ny, ref gen);

            // Индексация полей задачи
            F = new double[7][][];
            F[0] = u;
            F[1] = v;
            F[2] = pc;
            F[3] = t;
            F[4] = tke;
            F[5] = dis;
            F[6] = phi;

            unknowns.Add(new Unknown2D("Осредненная скорость х", u, TypeFunForm.Form_2D_Triangle_L1));
            unknowns.Add(new Unknown2D("Осредненная скорость у", v, TypeFunForm.Form_2D_Triangle_L1));
            unknowns.Add(new CalkPapams2D("Модуль скорости", u, v, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new Unknown2D("Давление", p, TypeFunForm.Form_2D_Triangle_L1));
            unknowns.Add(new Unknown2D("Поправка давления", pc, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new Unknown2D("Кинетичесая энергия турбулентности", tke, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new Unknown2D("Диссипация кинетической энергии турбулентности", dis, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new CalkPapams2D("Вихревая вязкость", mut, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new Unknown2D("Температура потока", t, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new Unknown2D("Концентрация в потоке", t, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new CalkPapams2D("Функция тока", phi, TypeFunForm.Form_2D_Rectangle_L1));

            // Коэффициенты реласксации модели
            relax[0] = 0.5f; // u-velocity
            relax[1] = 0.5f; // v-velocity
            relax[2] = 0.8f; // p_correction
            relax[3] = 1.0f; // temperature
            relax[4] = 0.4f; // turb kin energy
            relax[5] = 0.4f; // dissipation of turb kin energy
            relax[6] = 0.5f; // gamma

            //qmesh = new ReverseQMesh(Nx - 1, Ny - 1);
            //qmesh.InitQMesh(Params.Ly, Params.Lx, Q, P, Params.topBottom, Params.leftRight);


            //if (Params.GeometryForm == TypeGeometryForm.RectangleForm)
            //{
                qmesh = new ReverseQMesh(Nx - 1, Ny - 1);
                ((ReverseQMesh)qmesh).InitQMesh(Params.Ly, Params.Lx, Q, P, Params.topBottom, Params.leftRight);
            //}
            //else
            //{
            //    double Q = 0;
            //    double P = 0;
            //    qmesh = new QVectorMesh(Params.Len1, Params.Len2, Params.Len3,
            //                Params.Wen1, Params.Wen2, Params.Wen3, Nx - 1, Ny - 1, Q, P);
            //}

            if (Params.typeBedForm == TypeBedForm.PlaneForm)
            {
                // прямоугольник с плоским дном
                qmesh.StartGeometryMesh(ref xu, ref yv, ref x, ref y,
                 ref Hx, ref Hy, ref Dx, ref Dy);
                //OnGridCalculationRectangle();
            }
            else
            {
                // прямоугольник с профильным дном
                qmesh.CalkXYI(100 * imax * jmax);
                ConvertMeshToMesh();
                //qmesh.ConvertMeshToMesh(ref xu, ref yv, ref x, ref y, ref Hx, ref Hy, ref Dx, ref Dy);
            }
            eTaskReady = ETaskStatus.CreateMesh;
            // граничные условия
            OnInitialData();
            // выделение решателей
            InitTask();
            eTaskReady = ETaskStatus.TaskReady;
        }
        /// <summary>
        /// Конверсися генерируемой сетки в расчетной области в формат КО сетки
        /// </summary>
        public void ConvertMeshToMesh()
        {
            try
            {
                // массивы координат сетки в
                // вспомогательной системе координат 
                //  ^ x |
                //  |   |
                //  |   V (i)
                //  |
                //  *-----------> x (j)
                // взятой из старого кода 
                double[][] xx = ((ReverseQMesh)qmesh).xx;
                double[][] yy = ((ReverseQMesh)qmesh).yy;
                int NY = ((ReverseQMesh)qmesh).NX;
                int NX = ((ReverseQMesh)qmesh).NY;
                // конвертация сетки
                #region стенка справа

                // координаты узловых точек для скорости u
                for (int i = 1; i < NY; i++)
                    for (int j = 1; j < NX; j++)
                        xu[i][j] = 0.5 * (yy[i][j] + yy[i][j - 1]);

                // координаты узловых точек для скорости v
                for (int i = 0; i < NY; i++)
                    for (int j = 1; j < NX; j++)
                        yv[i][j] = 0.5 * (xx[i][j] + xx[i][j - 1]);

                for (int i = 1; i < NY; i++)
                    for (int j = 1; j < NX; j++)
                    {
                        x[i][j] = 0.25 * (yy[i - 1][j - 1] + yy[i - 1][j] + yy[i][j - 1] + yy[i][j]);
                        y[i][j] = 0.25 * (xx[i - 1][j - 1] + xx[i - 1][j] + xx[i][j - 1] + xx[i][j]);
                    }

                y[0][0] = xx[0][0];
                y[NY][0] = xx[NY - 1][0];
                y[0][NX] = xx[0][NX - 1];
                y[NY][NX] = xx[NY - 1][NX - 1];

                x[0][0] = yy[0][0];
                x[NY][0] = yy[NY - 1][0];
                x[0][NX] = yy[0][NX - 1];
                x[NY][NX] = yy[NY - 1][NX - 1];

                for (int i = 1; i < NY; i++)
                {
                    y[i][0] = 0.5 * (xx[i][0] + xx[i - 1][0]);
                    y[i][NX] = 0.5 * (xx[i][NX - 1] + xx[i - 1][NX - 1]);

                    x[i][0] = 0.5 * (yy[i][0] + yy[i - 1][0]);
                    x[i][NX] = 0.5 * (yy[i][NX - 1] + yy[i - 1][NX - 1]);
                }

                for (int j = 1; j < NX; j++)
                {
                    y[0][j] = 0.5 * (xx[0][j] + xx[0][j - 1]);
                    y[NY][j] = 0.5 * (xx[NY - 1][j] + xx[NY - 1][j - 1]);

                    x[0][j] = 0.5 * (yy[0][j] + yy[0][j - 1]);
                    x[NY][j] = 0.5 * (yy[NY - 1][j] + yy[NY - 1][j - 1]);
                }

                for (int i = 0; i < x.Length / 2; i++)
                {
                    double[] buf = x[i];
                    x[i] = x[x.Length - 1 - i];
                    x[x.Length - 1 - i] = buf;
                }

                // Переворот для у
                MEM.MReverseOrder(y);

                for (int i = 0; i < imax - 1; i++)
                {
                    for (int j = 0; j < jmax; j++)
                    {
                        // расстояние между узловыми точеками для скорости u
                        Hx[i + 1][j] = Math.Sqrt(
                            (xx[i][j] - xx[i + 1][j]) * (xx[i][j] - xx[i + 1][j]) +
                            (yy[i][j] - yy[i + 1][j]) * (yy[i][j] - yy[i + 1][j]));
                    }
                }
                // Разворот
                int Length = Hx.Length - 1;
                for (int i = 1; i < Length / 2; i++)
                {
                    double[] buf = Hx[i];
                    Hx[i] = Hx[Hx.Length - i];
                    Hx[Hx.Length - i] = buf;
                }

                for (int i = 0; i < Nx - 1; i++)
                {
                    for (int j = 0; j < jmax - 1; j++)
                    {
                        // расстояние между узловыми точеками для скорости v
                        Hy[i][j + 1] = Math.Sqrt(
                              (xx[i][j] - xx[i][(j + 1)]) * (xx[i][j] - xx[i][(j + 1)]) +
                              (yy[i][j] - yy[i][(j + 1)]) * (yy[i][j] - yy[i][(j + 1)]));
                    }
                }
                // Разворот
                Length = Hy.Length - 1;
                for (int i = 0; i < Length / 2; i++)
                {
                    double[] buf = Hy[i];
                    Hy[i] = Hy[Hx.Length - i - 2];
                    Hy[Hx.Length - i - 2] = buf;
                }

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

        #endregion
        /// <summary>
        /// Чтение параметров задачи
        /// </summary>
        public override void LoadParams(string fileName = "")
        {
            base.LoadParams(fileName);
            OnInitialData();
        }

        #region Свойства

        /// <summary>
        /// Имена файлов с данными для задачи гидродинамики
        /// </summary>
        public override ITaskFileNames taskFileNemes()
        {
            ITaskFileNames fn = new TaskFileNames()
            {
                NameCPParams = "NameCPParams.txt",
                NameBLParams = "NameBLParams.txt",
                NameRSParams = "NameRiverFVParams.txt",
                NameRData = "NameRData.txt",
                NameEXT = "(*.tsk)|*.tsk|"
            };
            return fn;
        }

        /// <summary>
        ///  Сетка для расчета донных деформаций
        /// </summary>
        protected double[] zX, zY;
        public override IMesh BedMesh()
        {
            GetBottom(ref zX, ref zY);
            return new TwoMesh(zX, zY);
        }
        /// <summary>
        /// Получаем сетку приведенную к TriMesh
        /// </summary>
        public override IMesh Mesh() => GetMesh();
        /// <summary>
        /// Установка КЭ сетки и решателя задачи
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="algebra">решатель задачи</param>
        public override void Set(IMesh mesh, IAlgebra a = null)
        {
            base.Set(mesh, algebra);

        }
        #endregion

        #region методы IRiver2D
        /// <summary>
        /// Предстартовая подготовка задачи
        /// </summary>
        public void InitTask()
        {
            if (Params.typeMAlgebra == TypeMAlgebra.TriDiagMat_Algorithm)
                pSolve = new TPSolver(imax, jmax);
            if (Params.typeMAlgebra == TypeMAlgebra.CGD_Algorithm)
                pSolve = new PAlgebraCG(imax, jmax);
            if (Params.typeMAlgebra == TypeMAlgebra.CGD_ParrallelAlgorithm)
                pSolve = new ParrallelPAlgebraCG(imax, jmax);
            pSolveR = new ParrallelPAlgebraCG(imax, jmax);
        }

        /// <summary>
        /// Установка новых отметок дна
        /// </summary>
        /// <param name="zeta"></param>
        public override void SetZeta(double[] zeta, EBedErosion bedErosion)
        {
            Erosion = bedErosion;

            //if (u == null)
            //    DefaultCalculationDomain();
            for (int i = 0; i < 6; i++)
                if (zeta[i] > 0.001)
                    zeta[i] = 0.001;

            if (zeta != null )
            {
                // FlagStartMesh - первая генерация сетки
                // bedErosion - генерация сетки при размывах дна
                if (FlagStartMesh == true || bedErosion != EBedErosion.NoBedErosion)
                {
                    // Геометрия дна в начальный момент времени
                    if (FlagStartMesh == true)
                    {
                        CreateStaticBedForms(ref zeta);
                        // Получение новой границы области и формирование сетки
                        qmesh.CreateNewQMesh(zeta, 0, 0, ZetaFuntion0, Params.MaxCoordIters);
                        FlagStartMesh = false;
                    }
                    else
                    {
                        qmesh.CreateNewQMesh(zeta, bottomTube, jdxTube, null, Params.MaxCoordIters);
                    }
                    // конвертация ReverseQMesh в сетку задачи
                    ConvertMeshToMesh();
                    //qmesh.ConvertMeshToMesh(ref xu, ref yv, ref x, ref y, ref Hx, ref Hy,ref Dx, ref Dy);

                    MEM.MemCopy(ref Zeta0, zeta);
                }
            }
            else
            {
                // Получение текущих донных отметок если zeta == null
                // при старте задачи (в данной задаче начальная генерация
                // сетки области и дна выполняется после выделения памяти
                // в методе DefaultCalculationDomain
                qmesh.GetBed(ref zeta);
            }
        }

        /// <summary>
        /// Расчет волны на границе
        /// </summary>
        /// <param name="x"></param>
        /// <param name="L"></param>
        /// <param name="bottomWaveAmplitude"></param>
        /// <param name="T"></param>
        /// <returns></returns>
        public void CreateStaticBedForms(ref double[] zeta)
        {
            // Определение индексов границ для расчета донных деформаций
            qmesh.GetZetaArea(ref count, ref In, ref Out, Params.Len1, Params.Len2, Params.bedLoadStart_X0);
            MEM.Alloc(count, ref zX);
            MEM.Alloc(count, ref zeta);
            // Геометрия дна в начальный момент времени
            switch (Params.typeBedForm)
            {
                case TypeBedForm.PlaneForm:
                    {
                        //ZetaFuntion0 = new BedSinLen2(Params.Len1, Params.Len2, Params.Len3,
                        //        0, Params.wavePeriod);
                        for (int j = 0; j < count; j++)
                            zeta[j] = 0;
                        break;
                    }
                case TypeBedForm.L1_L2sin_L3:
                    {
                        ZetaFuntion0 = new BedSinLen2(Params.Len1, Params.Len2, Params.Len3,
                                Params.bottomWaveAmplitude, Params.wavePeriod);
                        for (int j = 0; j < count; j++)
                        {
                            double y = zX[j];
                            zeta[j] = ZetaFuntion0.FunctionValue(y);
                        }
                        break;
                    }
            }
        }


        /// <summary>
        /// Получить отметки дна
        /// </summary>
        /// <param name="zeta"></param>
        public override void GetZeta(ref double[] zeta)
        {
            if (u == null)
                DefaultCalculationDomain();
            GetBottom(ref zX, ref zY);
            zeta = zY;
        }
        /// <summary>
        /// Расчет полей глубины и скоростей на текущем шаге по времени
        /// </summary>
        public override void SolverStep()
        {
            if (pSolve == null)
                InitTask();
            // Цикл итераций по нелинейности
            for (int iter = 0; iter < Params.NonLinearIterations; iter++)
            {
                // Цикл по подзадачам u, v, p', k, 
                for (int IndexTask = 0; IndexTask < Params.nfmax; IndexTask++)
                {
                    if (IndexTask != 3 ||
                        (IndexTask == 3 && Params.flatTermoTask == true) ||
                        ((IndexTask == 3 && Params.TemperOrConcentration == true) &&
                        (Erosion == EBedErosion.BedLoadAndSedimentErosion ||
                         Erosion == EBedErosion.SedimentErosion)))
                        CalkCoef(pSolve, pSolveR, time, IndexTask);
                }
                // Коррекция вычисляяемых граничных условий
                OnBoundCalculate();
                double cec = ErrPc();
                if (cec < MEM.Error3)
                    break;
            }
        }
        /// <summary>
        /// Формирование данных для отрисовки связанных с сеткой IMesh со сороны задачи
        /// </summary>
        /// <param name="sp">контейнер данных</param>
        public override void AddMeshPolesForGraphics(ISavePoint sp)
        {
            double[] data = null;
            double[] data1 = null;

            GetValue1D(p, ref data);
            sp.Add("Давление", data);

            GetValue1D(pc, ref data);
            sp.Add("Поправка давления", data);

            GetValue1D(u, ref data);
            sp.Add("Скорость Vx", data);

            GetValue1D(v, ref data1);
            sp.Add("Скорость Vy", data1);

            sp.Add("Скорость V", data, data1);

            GetValue1D(mut, ref data);
            sp.Add("Вязкость турбулентная", data);

            if (Params.TemperOrConcentration == false)
            {
                if (Params.flatTermoTask == true)
                {
                    GetValue1D(t, ref data);
                    sp.Add("Температура", data);
                }
            }
            else
            {
                GetValue1D(t, ref data);
                sp.Add("Концентрация", data);
            }

            GetValue1D(gen, ref data);
            sp.Add("Генерация энергии турбулентности", data);

            GetValue1D(tke, ref data);
            sp.Add("Кинетическая ЭТ", data);

            GetValue1D(phi, ref data);
            sp.Add("Функция тока", data);

            GetValue1D(dis, ref data);
            sp.Add("Диссипация ТКЭ", data);


            double[] P = null;
            double[] Yp = null;
            double[] zX1 = null;
            double[] zY1 = null;
            GetBottom(ref zX1, ref zY1, Params.AllBedForce);

            GetForce(ref tau, ref P, ref Yp, Params.AllBedForce);

            GetCS(ref data);
            sp.AddCurve("Концентрация осредненная по глубине ", zX1, data);

            sp.AddCurve("Напряжения на дне", zX1, tau);
            for (int i = 0; i < tau.Length; i++)
                tau[i] = Math.Sqrt(Math.Abs(tau[i]) / SPhysics.rho_w);
            sp.AddCurve("Динамическая скорость", zX1, tau);
            for (int i = 0; i < tau.Length; i++)
            {
                if (tau[i] <= 0.5*MEM.Error2) 
                    tau[i] = 0;
                else
                    tau[i] = SPhysics.PHYS.Ws/ (tau[i] * SPhysics.PHYS.kappa);
            }
            sp.AddCurve("число Рауза", zX1, tau);
            sp.AddCurve("Число шильдса на дне", zX1, theta);
            sp.AddCurve("Концентрация на дне", zX1, Cb_zeta);

            sp.AddCurve("Давление на дне", zX1, P);
            sp.AddCurve("Y+", zX1, Yp);
            sp.AddCurve("Отметки дна", zX1, zY1);
            

        }

        /// <summary>
        /// Получение полей придонных касательных напряжений и давления/свободной поверхности по контексту задачи
        /// усредненных на конечных элементах
        /// </summary>
        /// <param name="tauX">придонное касательное напряжение по х</param>
        /// <param name="tauY">придонное касательное напряжение по у</param>
        /// <param name="P">придонных давления/свободная поверхности потока, по контексту задачи</param>
        public override void GetTau(ref double[] tauX, ref double[] tauY, ref double[] P, ref double[][] CS, StressesFlag sf = StressesFlag.Element)
        {
            tauY = null;
            double[] yp = null;
            GetForce(ref tauX, ref P, ref yp);
            if ((Erosion == EBedErosion.SedimentErosion ||
                 Erosion == EBedErosion.BedLoadAndSedimentErosion) && Params.TemperOrConcentration == true)
            {
                if (CS == null) CS = new double[1][];
                GetCS(ref CS[0]);
            }
        }
        /// <summary>
        /// Интеграл концентации по глубине канала 
        /// </summary>
        /// <param name="CS"></param>
        protected void GetCS(ref double[] CS)
        {
            MEM.Alloc(Ny - 1, ref CS);
            for (int j = 0; j < Ny - 1; j++)
            {
                CS[j] = 0;
                for (int i = 0; i < Nx - 1; i++)
                    CS[j] += v[i][j] * t[i][j] * Hx[i][j];
            }
        }
        /// <summary>
        /// Создает экземпляр класса конвертера
        /// </summary>
        /// <returns></returns>
        public override IOFormater<IRiver> GetFormater()
        {
            return new TaskReader_Patankar1XD();
        }
        #endregion

        #region Расчет коэффициентов
        /// <summary>
        /// Инициализация данных (праметров мат модели),
        /// коэффициентов релаксации схемы,
        /// начальных и граничных условий задачи,
        /// расчет массового расхода потока
        /// </summary>
        public abstract void OnInitialData();
        /// <summary>
        /// Расчет дискретных аналогов и их коэффициентов
        /// </summary>
        /// <param name="pSolve"></param>
        /// <param name="time"></param>
        /// <param name="IndexTask"></param>
        public abstract void CalkCoef(ITPSolver pSolve, ITPSolver pSolveR, double time, int IndexTask);
        /// <summary>
        /// Расчет коэффициентов диффузии для текущей задачи (турбулентной вязкости, диффузии ...) 
        /// </summary>
        /// <param name="IndexTask"></param>
        protected abstract void OnLookingGama(int IndexTask);
        /// <summary>
        /// Расчет правой части уравнения
        /// </summary>
        /// <param name="ist">смещение по сетки по i</param>
        /// <param name="jst">смещение по сетки по j</param>
        /// <param name="IndexTask"></param>
        protected abstract void OnSource(int ist, int jst, int IndexTask);
        /// <summary>
        /// Получение координат дна zX,zY придонных касательных напряжений и давления
        /// </summary>
       // public abstract void GetForce(ref double[] tau, ref double[] Pb, ref double[] Yplus, bool AllBedForce = false);
        #endregion

        /// <summary>
        /// Получение координат дна zX,zY ? придонных касательных напряжений и давления
        /// </summary>
        public virtual void GetForce(ref double[] _tau, ref double[] Pb, ref double[] Yplus, bool _AllBedForce = false)
        {
            int start, end;
            if (_AllBedForce == true)
            {
                start = 0;
                end = y[0].Length - 2;
                //end = qmesh.NY - 1;
                MEM.Alloc<double>(end, ref tau);
                MEM.Alloc<double>(end, ref Yplus);
            }
            else
            {
                // Определение индексов границ для расчета донных деформаций
                qmesh.GetZetaArea(ref count, ref In, ref Out, Params.Len1, Params.Len2, Params.bedLoadStart_X0);
                MEM.Alloc<double>(count - 1, ref tau);
                MEM.Alloc<double>(count - 1, ref Yplus);
                start = In;
                end = count + In - 1;
            }
            // Настройка 
            WallFunction_Lutskoy.E_wall = E_wall;
            WallFunction_Lutskoy.nu = nu;
            WallFunction_Lutskoy.rho = rho_w;
            // расчет напряжений
            int k = 0;
            double Tau = 0, yplus = 0;

            double alpha_zeta;// Dx[1][j] / SPhysics.PHYS.d50;

            for (int j = start; j < end; j++)
            {
                // средняя скорость на интервале
                double V = 0.5 * (v[1][j] + v[1][j + 1]);
                double dxw = 0.5 * (x[2][j] + x[1][j]) - x[0][j];
                double dxe = 0.5 * (x[2][j + 1] + x[1][j + 1]) - x[0][j + 1];
                double dyw = y[1][j] - y[0][j];
                double dye = y[1][j + 1] - y[0][j + 1];
                double dx = 0.5 * (dxw + dxe); // растояние до центра 2 контр. объема
                double dy = 0.5 * (dyw + dye);
                double Y = Math.Sqrt(dx * dx + dy * dy);
                WallFunction_Lutskoy.Tau(V, Y, ref Tau, ref yplus);
                alpha_zeta = dx / SPhysics.PHYS.d50;
                if (k < tau.Length)
                {
                    tau[k] = Tau;
                    theta[k] = tau[k] / SPhysics.PHYS.normaTheta;
                    //Cb_zeta[k] = SPhysics.PHYS.Cb(Tau);
                    //if ( Math.Abs( theta[k] ) > SPhysics.PHYS.theta0)
                    //    Cb_zeta[k] = Math.Abs(theta[k]) / (alpha_zeta * SPhysics.PHYS.tanphi);
                    //else
                    //    Cb_zeta[k] = 0;
                    Cb_zeta[k] = SPhysics.PHYS.Cb(Tau);
                    Yplus[k] = yplus;
                    k++;
                }
                else
                {
                    Console.WriteLine("Необходимо удлинение массива tau");
                }
            }
            k = 0;
            if (_AllBedForce == true)
            {
                //MEM.Alloc<double>(qmesh.NY, ref Pb);
                //for (int j = 0; j < qmesh.NY; j++)
                MEM.Alloc<double>(p[0].Length-1, ref Pb);
                for (int j = 0; j < Pb.Length; j++)
                {
                    // среднее давление в узле
                    Pb[k] = p[0][j];
                    k++;
                }
            }
            else
            {
                MEM.Alloc<double>(count, ref Pb);
                for (int j = In; j < count + In; j++)
                {
                    // среднее давление в узле
                    Pb[k] = p[0][j];
                    k++;
                }
            }
            // отсечение зон рециркуляции
            if (Params.bedLoadTauPlus == true)
            {
                for (int j = 0; j < tau.Length; j++)
                    if (tau[j]<0)
                        tau[j]=0;
            }
            MEM.MemCopy(ref _tau, tau);
            
        }
        /// <summary>
        /// Коррекция вычисляяемых граничных условий
        /// </summary>
        public void OnBoundCalculate()
        {
            double factor = 1;
            double vmin = 0;
            int i, j;
            if (Params.shiftV == false)
            {
                // условия установления для скорости на границе входа
                // с подавлением обратных потоков 
                for (i = 1; i < imax; i++)
                {
                    if (v[i][jmax - 1] < 0)
                        vmin = Math.Max(vmin, -v[i][jmax - 1]);
                }
                for (i = 1; i < imax; i++)
                    v[i][jmax] = (v[i][jmax - 1] + vmin) * factor;
            }
            // подсос потока на входной границе
            if (Params.velocityInsBoundary == true)
            {
                double hh = 2 * Dx[1][1];
                // Инициализация данных
                for (i = 1; i < imax; i++)
                {
                    double bx = x[i][0];
                    if ((Params.Wen1 >= bx && Params.V1_inlet < MEM.Error10) ||
                        (Params.Wen2 + Params.Wen1 > bx &&
                        Params.Wen1 <= bx && Params.V2_inlet < MEM.Error10) ||
                        (Params.Wen3 + Params.Wen2 + Params.Wen1 + hh > bx &&
                        Params.Wen2 + Params.Wen1 <= bx && Params.V3_inlet < MEM.Error10))
                    {
                        if (Params.Wen1 >= bx && Params.Wen1 - hh < bx)
                        {
                            v[i][1] = v[i][2] = 0;
                            v[i][0] = v[i][2] = 0;
                            tke[i][0] = tke[i][2] = 0;
                            dis[i][0] = dis[i][2] = 0;
                            mut[i][0] = mut[i][2] = 0;
                        }
                        else
                        {
                            if (Params.Wen2 + Params.Wen1 <= bx &&
                                Params.Wen2 + Params.Wen1 + hh > bx)
                            {
                                v[i][1] = v[i][2] = 0;
                                v[i][0] = v[i][2] = 0;
                                
                                tke[i][0] = tke[i][2] = 0;
                                dis[i][0] = dis[i][2] = 0;
                                mut[i][0] = mut[i][2] = 0;
                            }
                            else
                            {
                                if (Params.shiftV == false)
                                {
                                    v[i][1] = Math.Max(0, v[i][2]);
                                    v[i][0] = Math.Max(0, v[i][2]);
                                }
                                else
                                {
                                    v[i][1] = v[i][2] = 0;
                                    v[i][0] = v[i][2] = 0;
                                }
                                tke[i][0] = tke[i][2];
                                dis[i][0] = dis[i][2];
                                mut[i][0] = mut[i][2];
                            }
                        }
                    }
                }
            }
            // условия установления 
            for (i = 0; i < imax + 1; i++)
            {
                tke[i][jmax - 1] = tke[i][jmax - 2];
                t[i][jmax - 1] = t[i][jmax - 2];
                dis[i][jmax - 1] = dis[i][jmax - 2];
                mut[i][jmax - 1] = mut[i][jmax - 2];
                gen[i][jmax - 1] = gen[i][jmax - 2];
                tke[i][jmax] = tke[i][jmax - 2];
                t[i][jmax] = t[i][jmax - 2];
                dis[i][jmax] = dis[i][jmax - 2];
                mut[i][jmax] = mut[i][jmax - 2];
                gen[i][jmax] = gen[i][jmax - 2];
                gen[i][0] = gen[i][1];
            }
            // условия установления на крышке 
            for (j = 0; j < jmax + 1; j++)
            {
                v[imax][j] = v[imax - 1][j];
                tke[imax][j] = tke[imax - 1][j];
                t[imax][j] = t[imax - 1][j];
                dis[0][j] = dis[1][j];
                dis[imax][j] = dis[imax - 1][j];
                gen[imax][j] = gen[imax - 1][j];
            }
            // Экстраполяция функции тока на выходе из области
            for (i = 1; i < Nx - 1; i++)
                phi[i][jmax] = phi[i][jmax - 1];
            // Экстраполяция давления на дне области
            for (j = 1; j < jmax; j++)
                p[0][j] = (p[1][j] * (Dx[1][j] + Dx[2][j]) - p[2][j] * Dx[1][j]) / Dx[2][j];
            // Экстраполяция давления на крышке области
            for (j = 1; j < jmax; j++)
                p[imax][j] = (p[imax - 1][j] * (Dx[imax - 1][j] + Dx[imax][j]) - p[imax - 2][j] * Dx[imax][j]) / Dx[imax - 1][j];
            // Экстраполяция давления на входе в область
            for (i = 1; i < imax; i++)
                p[i][0] = (p[i][1] * (Dy[i][1] + Dy[i][2]) - p[i][2] * Dy[i][1]) / Dy[i][2];
            // Экстраполяция давления на выходе из области
            for (i = 1; i < imax; i++)
                p[i][jmax] = (p[i][jmax - 1] * (Dy[i][jmax - 1] + Dy[i][jmax]) - p[i][jmax - 2] * Dy[i][jmax]) / Dy[i][jmax - 1];
            // Экстраполяция давления в углы области
            p[0][0] = p[1][0] + p[0][1] - p[1][1];
            p[imax][0] = p[imax - 1][0] + p[imax][1] - p[imax - 1][1];
            p[imax][jmax] = p[imax - 1][jmax] + p[imax][jmax - 1] - p[imax - 1][jmax - 1];
            // Нормировка давления к нулю в узле p[ipref][jpref] - Дирехле в 1 узле
            int ipref = 0;
            int jpref = 0;
            double pref = p[ipref][jpref];
            for (i = 0; i < imax + 1; i++)
                for (j = 0; j < jmax + 1; j++)
                    p[i][j] = p[i][j] - pref;

            // условия установления на дне
            if (Params.TemperOrConcentration == true)
            {
                double[] P = null;
                double[] Yp = null;
                GetForce(ref tau, ref P, ref Yp, Params.AllBedForce);
                // условия установления на дне
                for (j = 1; j < tau.Length; j++)
                {
                    t[0][j] = SPhysics.PHYS.Cb(tau[j]);
                    t[1][j] = t[0][j];
                }
                t[0][jmax - 1] = t[0][jmax - 2];
                t[1][jmax - 1] = t[1][jmax - 2];
                t[0][jmax] = t[0][jmax - 1];
                t[1][jmax] = t[1][jmax - 1];
            }
        }
        /// <summary>
        /// Максимальное значение поправки давления
        /// </summary>
        /// <returns></returns>
        public double ErrPc()
        {
            int ist = 1;
            int jst = 1;
            double errpc = 0;
            double cpc;
            for (int i = ist; i < imax; i++)
            {
                for (int j = jst; j < jmax; j++)
                {
                    cpc = Math.Abs(pc[i][j]);
                    if (cpc > errpc)
                        errpc = cpc;
                }
            }
            return errpc;
        }
        //
        public void StatisticaYplus()
        {
            int i, j;
            double y0 = Dx[1][1];
            double maxYp = xplus.Max();
            double minYp = xplus[1];
            double gamMin = gam[0][1];
            double gamMax = gam[0][1];
            for (j = 2; j < jmax + 1; j++)
            {
                if (minYp > xplus[j])
                    minYp = xplus[j];
                if (gamMin > gam[0][j])
                    gamMin = gam[0][j];
                if (gamMax < gam[0][j])
                    gamMax = gam[0][j];
            }
            double gamMidle = gamMin;
            double gamSumMidle = gamMin;
            int s = 0;
            for (i = 0; i < imax + 1; i++)
                for (j = 0; j < jmax + 1; j++)
                {
                    if (gamMidle < gam[i][j])
                        gamMidle = gam[i][j];
                    gamSumMidle += gamMidle;
                    s++;
                }
            gamSumMidle /= s;
        }

        /// <summary>
        /// Расчет коэффиентов Aw, As, ... 
        /// по схеме Патанкара со степенным законом
        /// </summary>
        /// <param name="Diff">диффузионный поток</param>
        /// <param name="Flow">конвективный поток</param>
        /// <returns>расчетный коэффициент</returns>
        protected double DifFlow(double Diff, double Flow)
        {
            if (Flow == 0.0f)
                return Diff;
            double temp = Diff - 0.1 * Math.Abs(Flow);
            if (temp <= 0.0f)
                return 0.0f;
            temp = temp / Diff;
            return (Diff * temp * temp * temp * temp * temp);
        }

        #region Методы сетки
        public void GetValue1D(double[][] value, ref double[] data)
        {
            int NL = value.Length * value[0].Length;
            MEM.Alloc<double>(NL, ref data);
            int co = 0;
            for (int i = 0; i < value.Length; i++)
                for (int j = 0; j < value[0].Length; j++)
                    data[co++] = value[i][j];
        }
        /// <summary>
        /// Получение координат дна zX,zY ? придонных касательных напряжений и давления
        /// </summary>
        public void GetBottom(ref double[] zX, ref double[] zY, bool _AllBedForce = false)
        {
            if (_AllBedForce == true)
            {
                MEM.Alloc<double>(y[0].Length, ref zX);
                MEM.Alloc<double>(y[0].Length, ref zY);
                for (int j = 0; j < zX.Length; j++)
                {
                    zX[j] = y[0][j];
                    zY[j] = x[0][j];
                }
            }
            else
            {
                int k;
                // Определение индексов границ для расчета донных деформаций
                qmesh.GetZetaArea(ref count, ref In, ref Out, Params.Len1, Params.Len2, Params.bedLoadStart_X0);
                MEM.Alloc<double>(count, ref zX);
                MEM.Alloc<double>(count, ref zY);
                for (int j = 0; j < count; j++)
                {
                    k = j + In;
                    zX[j] = y[0][k];
                    zY[j] = x[0][k];
                }
            }
        }
        /// <summary>
        /// Получить образ симплекс сетки для модуля графики
        /// </summary>
        /// <returns></returns>
        public TriMesh GetMesh()
        {
            TriMesh mesh = new TriMesh();
            int NY = x.Length;
            int NX = x[0].Length;
            uint[][] map = new uint[NY][];
            uint co = 0;
            for (int i = 0; i < NY; i++)
            {
                map[i] = new uint[NX];
                for (int j = 0; j < NX; j++)
                    map[i][j] = co++;
            }
            int CountElems = 2 * (NX - 1) * (NY - 1);
            int CountBElems = 2 * (NX - 1 + NY - 1);
            int CountKnots = NX * NY;
            mesh.AreaElems = new TriElement[CountElems];
            mesh.BoundElems = new TwoElement[CountBElems];
            mesh.BoundElementsMark = new int[CountBElems];
            
            mesh.BoundKnots = new int[CountBElems];
            mesh.BoundKnotsMark = new int[CountBElems];
            
            mesh.CoordsX = new double[CountKnots];
            mesh.CoordsY = new double[CountKnots];
            // Элементы
            co = 0;
            for (int i = 0; i < NY - 1; i++)
                for (int j = 0; j < NX - 1; j++)
                {
                    mesh.AreaElems[co++] = new TriElement(map[i][j], map[i + 1][j], map[i + 1][j + 1]);
                    mesh.AreaElems[co++] = new TriElement(map[i][j], map[i + 1][j + 1], map[i][j + 1]);
                }
            // Элементы на границе
            co = 0;
            uint kn = 0;
            for (int i = 0; i < NY - 1; i++)
            {
                mesh.BoundElementsMark[co] = 0;
                mesh.BoundElems[co++] = new TwoElement(map[i][0], map[i + 1][0]);
                mesh.BoundElementsMark[co] = 2;
                mesh.BoundElems[co++] = new TwoElement(map[i][NX - 1], map[i + 1][NX - 1]);
                mesh.BoundKnots[kn++] = (int)map[i][0];
                mesh.BoundKnots[kn++] = (int)map[i + 1][NX - 1];
            }
            for (int j = 0; j < NX - 1; j++)
            {
                mesh.BoundElementsMark[co] = 1;
                mesh.BoundElems[co++] = new TwoElement(map[0][j], map[0][j + 1]);
                mesh.BoundElementsMark[co] = 3;
                mesh.BoundElems[co++] = new TwoElement(map[NY - 1][j], map[NY - 1][j + 1]);
                mesh.BoundKnots[kn++] = (int)map[0][j + 1];
                mesh.BoundKnots[kn++] = (int)map[NY - 1][j];
            }
            // Узлы
            co = 0;
            for (int i = 0; i < NY; i++)
                for (int j = 0; j < NX; j++)
                {
                    mesh.CoordsX[co] = x[i][j];
                    mesh.CoordsY[co] = y[i][j];
                    co++;
                }
            return mesh;
        }
        #endregion
    }
}
