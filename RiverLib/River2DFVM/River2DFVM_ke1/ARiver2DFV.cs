﻿//---------------------------------------------------------------------------
//               Класс CFDTask предназначен для запуска на решение 
//                      задачи турбулентного теплообмена
//                              Потапов И.И.
//                        - (C) Copyright 2015 -
//                          ALL RIGHT RESERVED
//                               31.07.15
//---------------------------------------------------------------------------
//   Реализация библиотеки для решения задачи турбулентного теплообмена
//                  методом контрольного объема
//---------------------------------------------------------------------------
//                  добавлена иерархия параметров 12.03.2021              
//---------------------------------------------------------------------------
//   добавлена адаптация вывода результатов в форматах библиотек 
//                              MeshLib, RenderLib;
//                                  16.03.2021
//---------------------------------------------------------------------------
namespace RiverLib.Patankar
{

    using System;
    using System.IO;
    using System.Linq;

    using CommonLib;
    using CommonLib.IO;
    using CommonLib.Physics;
    using CommonLib.Function;
    using CommonLib.ChannelProcess;

    using MeshLib;
    using MemLogLib;
    using AlgebraLib;
    using RiverLib.AllWallFunctions;

    /// <summary>
    /// ОО: Реализация решателя для задачи о расчете профильного турбулентного потока 
    /// в формулировке RANS k-e методом контрольных объемов
    /// </summary>
    [Serializable]
    public abstract class ARiver2DFV : IRiver
    {
        #region Свойства
        protected Stream2FVParams Params = new Stream2FVParams();
        /// <summary>
        /// Установка свойств задачи
        /// </summary>
        /// <param name="p"></param>
        //public void SetParams(object p)
        //{
        //    Stream2FVParams pp = p as Stream2FVParams;
        //    InitTask();
        //}
        /// <summary>
        /// установка параметров
        /// </summary>
        /// <param name="p"></param>
        public void SetParams(object p)
        {
            Params.SetParams((Stream2FVParams)p);
            InitTask();
        }
        public object GetParams()
        {
            return Params;
        }
        /// <summary>
        /// Чтение параметров задачи
        /// </summary>
        public void LoadParams(string fileName = "")
        {
            string message = "Файл парамеров задачи - доные деформации - не обнаружен";
            WR.LoadParams(Load, message, fileName);
            OnInitialData();
        }
        /// <summary>
        /// Чтение параметров задачи из файла
        /// </summary>
        /// <param name="file"></param>
        public void Load(StreamReader file)
        {
            Params = new Stream2FVParams();
            Params.Load(file);
        }
        #endregion


        #region Свойства

        protected double rho_w = SPhysics.kappa_w;
        protected double mu = SPhysics.mu;
        protected double nu = SPhysics.nu;
        protected double kappa_w = SPhysics.kappa_w;
        public double smax;

        /// <summary>
        /// Флаг определяющий необходимость расчета взвешенных наносов
        /// </summary>
        public BedErosion GetBedErosion() => Erosion;
        public BedErosion Erosion;

        /// <summary>
        /// Имена файлов с данными для задачи гидродинамики
        /// </summary>
        public virtual ITaskFileNames taskFileNemes()
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
        /// Текущее время
        /// </summary>
        public double time { get; set; }
        /// <summary>
        /// Текущий шаг по времени
        /// </summary>
        public double dtime { get => dt; set { dt = value; } }
        public double dt;
        /// <summary>
        /// Наименование задачи
        /// </summary>
        public string Name { get => name; }
        protected string name;
        /// <summary       
        /// Тип задачи используется для выбора совместимых подзадач
        /// </summary>
        public TypeTask typeTask { get => TypeTask.streamX1D; }
        /// <summary>
        /// версия дата последних изменений интерфейса задачи
        /// </summary>
        public string VersionData() => "ARiver2DFV 01.09.2021"; 

        /// <summary>
        ///  Сетка для расчета донных деформаций
        /// </summary>
        protected double[] zX, zY;
        public IMesh BedMesh()
        {
            GetBottom(ref zX, ref zY);
            return new TwoMesh(zX, zY);
        }
        /// <summary>
        /// Получаем сетку приведенную к TriMesh
        /// </summary>
        public IMesh Mesh() => GetMesh();
        /// <summary>
        /// Искомые поля задачи которые позволяют сформировать на сетке расчетной области краевые условия задачи
        /// </summary>
        public IUnknown[] Unknowns() => unknowns;
        protected IUnknown[] unknowns = { new SUnknown("Осредненная скорость х", true, TypeFunForm.Form_2D_Rectangle_L1),
                                          new SUnknown("Осредненная скорость у", true, TypeFunForm.Form_2D_Rectangle_L1),
                                          new SUnknown("Давление ", true, TypeFunForm.Form_2D_Rectangle_L1),
                                          new SUnknown("Поправка давления", true, TypeFunForm.Form_2D_Rectangle_L1),
                                          new SUnknown("Кинетичесая энергия турбулентности", true, TypeFunForm.Form_2D_Rectangle_L1),
                                          new SUnknown("Диссипация кинетической энергии турбулентности", true, TypeFunForm.Form_2D_Rectangle_L1),
                                          new SUnknown("Температура потока", true, TypeFunForm.Form_2D_Rectangle_L1) };

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
        public QMesh qmesh = null;
        /// <summary>
        /// FlagStartMesh - первая генерация сетки true
        /// </summary>
        protected bool FlagStartMesh = true;
        protected double Q = 0;
        protected double P = 0;
        public bool meshGeom;
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
        /// Температура
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
        #region методы IRiver2D
        /// <summary>
        /// Получить граничные условия для задачи донных деформаций
        /// </summary>
        /// <returns></returns>
        public IBoundaryConditions BoundCondition { get; set; } = null;

        /// <summary>
        /// конструктор
        /// </summary>
        /// <param name="ps"></param>
        public ARiver2DFV()
        {
            Set(new Stream2FVParams());
        }
        /// <summary>
        /// конструктор
        /// </summary>
        /// <param name="ps"></param>
        public ARiver2DFV(Stream2FVParams ps)
        {
            Set(ps);
        }
        /// <summary>
        /// Установка параметров
        /// </summary>
        /// <param name="ps">параметры</param>
        public virtual void Set(Stream2FVParams ps)
        {
            SetParams(ps);
            imax = ps.FV_X + 1;
            jmax = ps.FV_Y + 1;
            Nx = imax + 1;
            Ny = jmax + 1;
            Params.wavePeriod = ps.wavePeriod;
            Params.bottomWaveAmplitude = ps.bottomWaveAmplitude;
            //x = new double[Nx][];
            //xu = new double[Nx][];
            //Dx = new double[Nx][];
            //for (int i = 0; i < Nx; i++)
            //{
            //    x[i] = new double[Ny];
            //    xu[i] = new double[Ny];
            //    Dx[i] = new double[Ny];
            //}
            //Hx = new double[imax][];
            //for (int i = 0; i < imax; i++)
            //    Hx[i] = new double[Ny];

            MEM.Alloc2D<double>(Nx, Ny, ref x);
            MEM.Alloc2D<double>(Nx, Ny, ref xu);
            MEM.Alloc2D<double>(Nx, Ny, ref Dx);

            MEM.Alloc2D<double>(imax, Ny, ref Hx);

            MEM.Alloc<double>(jmax, ref theta);
            MEM.Alloc<double>(jmax, ref Cb_zeta);
            

            //y = new double[Nx][];
            //yv = new double[Nx][];
            //Hy = new double[Nx][];
            //Dy = new double[Nx][];
            //for (int i = 0; i < Nx; i++)
            //{
            //    y[i] = new double[Ny];
            //    yv[i] = new double[Ny];
            //    Dy[i] = new double[Ny];
            //    Hy[i] = new double[jmax];
            //}
            MEM.Alloc2D<double>(Nx, Ny, ref y);
            MEM.Alloc2D<double>(Nx, Ny, ref yv);
            MEM.Alloc2D<double>(Nx, Ny, ref Dy);
            MEM.Alloc2D<double>(Nx, jmax, ref Hy);

            if (Params.typeBedForm == TypeBedForm.PlaneForm)
                meshGeom = false;
            else
                meshGeom = true;

            OnGridCalculation(meshGeom);

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


            MEM.Alloc2D<double>(Nx, Ny, ref mut);
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
            // Коэффициенты реласксации модели
            relax[0] = 0.5f; // u-velocity
            relax[1] = 0.5f; // v-velocity
            relax[2] = 0.8f; // p_correction
            relax[3] = 1.0f; // temperature
            relax[4] = 0.4f; // turb kin energy
            relax[5] = 0.4f; // dissipation of turb kin energy
            relax[6] = 0.5f; // gamma
            // граничные условия
            OnInitialData();
            // выделение решателей
            InitTask();
        }
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
        /// Установка адаптеров для КЭ сетки и алгебры
        /// </summary>
        /// <param name="_mesh"></param>
        /// <param name="algebra"></param>
        public void Set(IMesh mesh, IAlgebra algebra = null) { }
        /// <summary>
        /// Установка новых отметок дна
        /// </summary>
        /// <param name="zeta"></param>
        public void SetZeta(double[] zeta, BedErosion bedErosion)
        {
            Erosion = bedErosion;
            if (zeta != null)
            {
                // FlagStartMesh - первая генерация сетки
                // bedErosion - генерация сетки при размывах дна
                if (FlagStartMesh == true || bedErosion != BedErosion.NoBedErosion )
                {
                    // Геометрия дна в начальный момент времени
                    if (FlagStartMesh == true)
                    {
                        CreateStaticBedForms(ref zeta);
                        // Получение новой границы области и формирование сетки
                        qmesh.CreateNewQMesh(zeta, ZetaFuntion0, Params.MaxCoordIters);
                        FlagStartMesh = false;
                    }
                    else
                        qmesh.CreateNewQMesh(zeta, null, Params.MaxCoordIters);
                    // конвертация QMesh в сетку задачи
                    ConvertMeshToMesh();
                    MEM.MemCopy(ref Zeta0, zeta);
                }
            }
            else
            {
                // Получение текущих донных отметок
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
            int k;
            // Определение индексов границ для расчета донных деформаций
            qmesh.GetZetaArea(ref count, ref In, ref Out, Params.Len1, Params.Len2);
            MEM.Alloc<double>(count, ref zX);
            // Геометрия дна в начальный момент времени
            switch (Params.typeBedForm)
            {
                case TypeBedForm.PlaneForm:
                    {
                        for (int j = 0; j < count; j++)
                            zeta[j] = 0;
                        break;
                    }
                case TypeBedForm.L1_L2sin_L3:
                    {
                        if (ZetaFuntion0 == null)
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
        public void GetZeta(ref double[] zeta)
        {
            GetBottom(ref zX, ref zY);
            zeta = zY;
        }
        /// <summary>
        /// Расчет полей глубины и скоростей на текущем шаге по времени
        /// </summary>
        public void SolverStep()
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
                        (IndexTask == 3 && Params.TemperOrConcentration == true))
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
        public void AddMeshPolesForGraphics(ISavePoint sp)
        {
            double[] data = null;
            double[] data1 = null;


            GetValue1D(p, ref data);
            sp.Add("Давление", data);

            GetValue1D(pc,ref data);
            sp.Add("Поправка давления", data);

            GetValue1D(u, ref data);
            sp.Add("Скорость U", data);

            GetValue1D(v, ref data1);
            sp.Add("Скорость V", data1);

            sp.Add("Поле скорости", data, data1);

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
        public void GetTau(ref double[] tauX, ref double[] tauY, ref double[] P, ref double[][] CS, StressesFlag sf = StressesFlag.Element)
        {
            tauY = null;
            double[] yp = null;
            GetForce(ref tauX, ref P, ref yp);
            if ((Erosion == BedErosion.SedimentErosion ||
                 Erosion == BedErosion.BedLoadAndSedimentErosion) && Params.TemperOrConcentration == true)
            {
                if(CS == null) CS = new double[1][];
                GetCS(ref CS[0]);
            }
        }

        protected void GetCS(ref double[] CS)
        {
            MEM.Alloc(Ny-1, ref CS);
            for (int j = 0; j < Ny-1; j++)
            {
                CS[j] = 0;
                for (int i = 0; i < Nx-1; i++)
                    CS[j] += v[i][j] * t[i][j] * Hx[i][j];
            }
        }


        /// <summary>
        /// Загрузка геометрии дна
        /// </summary>
        /// <param name="fileName"></param>
        public virtual void LoadData(string fileName) { }
        /// <summary>
        /// Создает экземпляр класса конвертера
        /// </summary>
        /// <returns></returns>
        public virtual IOFormater<IRiver> GetFormater()
        {
            return new ProxyTaskFormat<IRiver>();
        }

        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public abstract IRiver Clone();
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
                end = qmesh.NY - 1;
                MEM.Alloc<double>(end, ref tau);
                MEM.Alloc<double>(end, ref Yplus);
            }
            else
            {
                // Определение индексов границ для расчета донных деформаций
                qmesh.GetZetaArea(ref count, ref In, ref Out, Params.Len1, Params.Len2);
                MEM.Alloc<double>(count - 1, ref tau);
                MEM.Alloc<double>(count - 1, ref Yplus);
                start = In;
                end = count + In - 1;
            }
            // Настройка 
            WallFunction_Lutskoy.E_wall = Params.E_wall;
            WallFunction_Lutskoy.nu = Params.nu;
            WallFunction_Lutskoy.rho = Params.rho_w;
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
                MEM.Alloc<double>(qmesh.NY, ref Pb);
                for (int j = 0; j < qmesh.NY; j++)
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
            for (i = 1; i < imax; i++)
            {
                if (v[i][jmax - 1] < 0)
                    vmin = Math.Max(vmin, -v[i][jmax - 1]);
            }
            for (i = 1; i < imax; i++)
                v[i][jmax] = (v[i][jmax - 1] + vmin) * factor;
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
            // Экстраполяция давления на границе области
            for (j = 1; j < jmax; j++)
            {
                p[0][j] = (p[1][j] * (Dx[1][j] + Dx[2][j]) - p[2][j] * Dx[1][j]) / Dx[2][j];
                p[imax][j] = (p[imax - 1][j] * (Dx[imax - 1][j] + Dx[imax][j]) - p[imax - 2][j] * Dx[imax][j]) / Dx[imax - 1][j];
            }
            for (i = 1; i < Nx - 1; i++)
                phi[i][jmax] = phi[i][jmax - 1];

            for (i = 1; i < imax; i++)
            {
                p[i][0] = (p[i][1] * (Dy[i][1] + Dy[i][2]) - p[i][2] * Dy[i][1]) / Dy[i][2];
                p[i][jmax] = (p[i][jmax - 1] * (Dy[i][jmax - 1] + Dy[i][jmax]) - p[i][jmax - 2] * Dy[i][jmax]) / Dy[i][jmax - 1];
            }
            p[0][0] = p[1][0] + p[0][1] - p[1][1];
            p[imax][0] = p[imax - 1][0] + p[imax][1] - p[imax - 1][1];
            p[imax][jmax] = p[imax - 1][jmax] + p[imax][jmax - 1] - p[imax - 1][jmax - 1];
            int ipref = 0;
            int jpref = 0;
            double pref = p[ipref][jpref];

            for (i = 0; i < imax + 1; i++)
                for (j = 0; j < jmax + 1; j++)
                    p[i][j] = p[i][j] - pref;

            if(Params.TemperOrConcentration == true)
            {
                double[] P = null;
                double[] Yp = null;
                GetForce(ref tau, ref P, ref Yp, Params.AllBedForce);
                // условия установления на дне
                for (j = 1; j < jmax ; j++)
                {
                    t[0][j] = Cb_zeta[j]; 
                    t[1][j] = t[0][j];
                }
                t[0][jmax-1] = Cb_zeta[jmax - 2];
                t[1][jmax-1] = t[0][jmax - 2];
                t[0][jmax] = Cb_zeta[jmax-1];
                t[1][jmax] = t[0][jmax-1];
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
        /// <summary>
        /// Конвертер поля из 2-х мерного в 1-й мерный массив
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        //public double[] GetValue1D(double[][] value)
        //{
        //    int NL = value.Length * value[0].Length;
        //    double[] data = null;
        //    MEM.Alloc<double>(NL, ref data);
        //    int co = 0;
        //    for (int i = 0; i < value.Length; i++)
        //        for (int j = 0; j < value[0].Length; j++)
        //            data[co++] = value[i][j];
        //    return data;
        //}
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
        /// Генерация начальной геометрии области
        /// </summary>
        /// <param name="meshGeom"></param>
        public void OnGridCalculation(bool meshGeom = false)
        {
            if (qmesh == null)
                qmesh = new QMesh(Nx - 1, Ny - 1);
            qmesh.InitQMesh(Params.Ly, Params.Lx, Q, P, Params.topBottom, Params.leftRight);
            if (meshGeom == false)
                OnGridCalculationRectangle();
            else
            {
                qmesh.CalkXYI(100*imax*jmax);
                ConvertMeshToMesh();
            }
        }
        /// <summary>
        /// Расчет координат сетки задачи в прямоугольнике 
        /// </summary>
        public void OnGridCalculationRectangle()
        {
            int i, j;
            double dx = Params.Lx / (imax - 1);
            for (j = 0; j < Ny; j++)
            {
                x[0][j] = xu[1][j] = 0;
            }
            for (i = 1; i < imax; i++)
            {
                for (j = 0; j < Ny; j++)
                {
                    // координаты узловых точек для скорости u
                    xu[i + 1][j] = xu[i][j] + dx;
                    // x - координата цетра контрольного объема 
                    x[i][j] = (xu[i + 1][j] + xu[i][j]) / 2;
                    // расстояние между центрами контрольных объемов по х
                    Dx[i][j] = x[i][j] - x[i - 1][j];
                    // расстояние между узловыми точеками для скорости u
                    Hx[i][j] = xu[i + 1][j] - xu[i][j];
                }
            }
            for (j = 0; j < Ny; j++)
            {
                x[imax][j] = xu[imax][j];
                Dx[imax][j] = x[imax][j] - x[imax - 1][j];
            }
            double dy = Params.Ly / (jmax - 1);
            for (i = 0; i < Nx; i++)
            {
                y[i][0] = yv[i][1] = 0;
            }
            for (i = 0; i < Nx; i++)
            {
                for (j = 1; j < jmax; j++)
                {
                    // координаты узловых точек для скорости v
                    yv[i][j + 1] = yv[i][j] + dy;
                    // y - координата цетра контрольного объема
                    y[i][j] = (yv[i][j + 1] + yv[i][j]) / 2;
                    // расстояние между центрами контрольных объемов по у
                    Dy[i][j] = y[i][j] - y[i][j - 1];
                    // расстояние между узловыми точеками для скорости v
                    Hy[i][j] = yv[i][j + 1] - yv[i][j];
                }
                y[i][jmax] = yv[i][jmax];
                Dy[i][jmax] = y[i][jmax] - y[i][jmax - 1];
            }
        }
        /// <summary>
        /// Получение координат дна zX,zY ? придонных касательных напряжений и давления
        /// </summary>
        public void GetBottom(ref double[] zX, ref double[] zY, bool _AllBedForce = false)
        {
            if (_AllBedForce == true)
            {
                MEM.Alloc<double>(qmesh.NY, ref zX);
                MEM.Alloc<double>(qmesh.NY, ref zY);
                for (int j = 0; j < qmesh.NY; j++)
                {
                    zX[j] = y[0][j];
                    zY[j] = x[0][j];
                }
            }
            else
            {
                int k;
                // Определение индексов границ для расчета донных деформаций
                qmesh.GetZetaArea(ref count, ref In, ref Out, Params.Len1, Params.Len2);
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
                double[][] xx = qmesh.xx;
                double[][] yy = qmesh.yy;
                int NY = qmesh.NX;
                int NX = qmesh.NY;
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
            catch(Exception ex)
            {
                Logger.Instance.Exception(ex);
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
            mesh.BoundElementsType = new TypeBoundCond[CountBElems]; 
            mesh.BoundKnots = new int[CountBElems];
            mesh.BoundKnotsMark = new int[CountBElems];
            mesh.BoundKnotsType = new TypeBoundCond[CountBElems];
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
