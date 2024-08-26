//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//---------------------------------------------------------------------------
//       исходный код решателя создан П.С.Тимош, аспирант, 2021-2022
//          (Вычислительный центр Дальневосточного отделения РАН)
//---------------------------------------------------------------------------
//               кодировка адаптации : 15.10.2022 Потапов И.И.
//---------------------------------------------------------------------------
namespace RiverLib
{
    using System;
    using MemLogLib;
    using CommonLib;
    using CommonLib.IO;
    using MeshLib;
    using RiverLib.IO;
    using GeometryLib.Vector;
    using CommonLib.EConverter;
    using CommonLib.ChannelProcess;
    using CommonLib.Delegate;
    using GeometryLib;


    /// <summary>
    ///  ОО: Определение класса RiverSW_FCT - для расчета полей расходов, 
    ///  скорости, глубин и напряжений в речном потоке
    /// </summary>
    [Serializable]
    public class GasDynamics_FCT : RiverSW_FCTParams, IRiver
    {
        /// <summary>
        /// Готовность задачи к расчету
        /// </summary>
        public bool TaskReady() => true;
        #region поля задачи
        /// <summary>
        /// Симплекс сетка области для отрисовки поляй задачи
        /// </summary>
        RectangleMesh mesh = null;
        /// <summary>
        /// Метод установки НУ задачи
        /// </summary>
        public SimpleProcedure initialCondition { get; set; } = null;
        /// <summary>
        /// Метод установки ГУ задачи
        /// </summary>
        public SimpleProcedure borderCondition { get; set; } = null;

        /// <summary>
        /// Скорость по оси X
        /// </summary>
        protected double[,] u;
        /// <summary>
        /// Скорость по оси Y
        /// </summary>
        protected double[,] v;
        /// <summary>
        /// Глубина водного канала
        /// </summary>
        protected double[,] p;
        /// <summary>
        /// Уровень дна канала
        /// </summary>
        protected double[,] rho;
        /// <summary>
        /// расход по х
        /// </summary>
        protected double[,] T;
        /// <summary>
		/// Область решения
		/// </summary>
        protected bool[,] regionSolver;
        /// <summary>
		/// Область входной границы
		/// </summary>
        protected bool[,] inputBoundary;
        /// <summary>
        /// Вектор потоков
        /// </summary>
        Vector4[,] Q;
        /// <summary>
        /// Дельта Q для FCT
        /// </summary>
        Vector4[,] DxQ, DyQ;
        /// <summary>
        /// дивергентный вектор
        /// </summary>
        Vector4[,] F, G;
        /// <summary>
        /// Диффузия решения
        /// </summary>
        Vector4[,] fD, gD;
        /// <summary>
        /// Антидиффузия решения
        /// </summary>
        Vector4[,] fAD, gAD;
        /// <summary>
        /// Параметр расхода воды
        /// </summary>
        protected const double Qh = 1.01f;
        /// <summary>
        /// Параметры сглаживания разрыва в полях задачи
        /// </summary>
        private const double ETA = 1.0f / 6;
        private const double ET1 = 1.0f / 3;
        private const double ET2 = -1.0f / 6;

        /// <summary>
        /// отношение удельных теплоемкостей
        /// </summary>
        double Gamma = 1.4;
        /// <summary>
        /// отношение давлений p1/p2
        /// </summary>
        double PressureRate = 2.5;
        double DUM, RHD, DIM, UD;
        double GMM, GMP, GMR;
        /// <summary>
        /// скорость скачка 
        /// </summary>
        double SpeedJamp;
        uint IndxJamp;
        /// <summary>
        /// положение скачка в начальный момент времени
        /// </summary>
        double JampCoord0 = 0.1f;
        /// <summary>
        /// шаг сетки по х и у
        /// </summary>
        protected double dx, dy;
        #endregion

        #region IRiver
        /// <summary>
        /// Флаг определяющий необходимость расчета взвешенных наносов
        /// </summary>
        public EBedErosion GetBedErosion() => Erosion;
        public EBedErosion Erosion;

        /// <summary>
        /// Наименование задачи
        /// </summary>
        public string Name { get => "Газовая динамика CD FCT"; }
        /// <summary>
        /// Тип задачи используется для выбора совместимых подзадач
        /// </summary>
        public TypeTask typeTask { get => TypeTask.streamXY2D; }
        /// <summary>
        /// версия дата последних изменений интерфейса задачи
        /// </summary>
        public string VersionData() => "River2D 27.11.2022";
        /// <summary>
        /// граничные условия
        /// </summary>
        public virtual IBoundaryConditions BoundCondition() => null;
        /// <summary>
        /// Имена файлов с данными для задачи гидродинамики при загрузке
        /// по умолчанию
        /// </summary>
        public ITaskFileNames taskFileNemes()
        {
            ITaskFileNames fn = new TaskFileNames();
            fn.NameCPParams = "NameCPParams.txt";
            fn.NameBLParams = "NameBLParams.txt";
            fn.NameRSParams = "NameRSParams.txt";
            fn.NameRData = "NameRData.txt";
            fn.NameEXT = "(*.tsk)|*.tsk|";
            fn.NameEXTImport = "(*.txt)|*.txt|";
            return fn;
        }
        /// <summary>
        ///  Сетка для расчета донных деформаций
        /// </summary>
        public IMesh BedMesh()
        {
            return null;
        }
        /// <summary>
        /// КЭ сетка
        /// </summary>
        public IMesh Mesh() => mesh.Clone();
        /// <summary>
        /// Искомые поля задачи которые позволяют сформировать на сетке расчетной области краевые условия задачи
        /// </summary>
        public IUnknown[] Unknowns() => unknowns;
        protected IUnknown[] unknowns =
        {
            new Unknown("Скорость х", null, TypeFunForm.Form_2D_Triangle_L1),
            new Unknown("Скорость у", null, TypeFunForm.Form_2D_Triangle_L1),
            new Unknown("Плотность", null, TypeFunForm.Form_2D_Triangle_L1),
            new Unknown("Давление", null, TypeFunForm.Form_2D_Triangle_L1),
            new Unknown("Энергия", null, TypeFunForm.Form_2D_Triangle_L1)
        };
        /// <summary>
        /// Расчет полей глубины и скоростей на текущем шаге по времени
        /// </summary>
        public void SolverStep()
        {

            //dtime = dtEvaluate();

            // диффузия потоков на входной границе
            for (uint i = 0; i < Nx; i++)
            {
                for (int j = 0; j < Ny; j++)
                {
                    if (inputBoundary[i, j])
                    {
                        var alpha = dtime * (u[i, j] + u[i, j + 1]) / dx;
                        var nu = ETA + ET1 * alpha * alpha;
                        fD[i, j] = nu * (Q[i, j + 1] - Q[i, j]);
                        var epsilon = dtime * ((v[i, j] + v[i + 1, j]) / dy);
                        var lambda = ETA + ET1 * epsilon * epsilon;
                        gD[i, j] = lambda * (Q[i + 1, j] - Q[i, j]);
                    }
                }
            }

            // цикл по внутренним узлам области
            for (uint i = 1; i < Nx - 1; ++i)
            {
                for (uint j = 1; j < Ny - 1; ++j)
                {
                    // диффузия потоков
                    if (regionSolver[i, j])
                    {
                        var alpha = dtime * (u[i, j] + u[i + 1, j]) / dx;
                        var epsilon = dtime * (v[i, j] + v[i, j + 1]) / dy;
                        var nu = ETA + ET1 * alpha * alpha;
                        var lambda = ETA + ET1 * epsilon * epsilon;
                        fD[i, j] = nu * (Q[i + 1, j] - Q[i, j]);
                        gD[i, j] = lambda * (Q[i, j + 1] - Q[i, j]);

                        var Fx = (F[i + 1, j] - F[i - 1, j]) / dx;
                        var Gy = (G[i, j + 1] - G[i, j - 1]) / dy;

                        // Схема центральных разностей
                        Q[i, j] = Q[i, j] - 0.5f * dtime * (Fx + Gy);
                    }
                }
            } // по пространству

            // расчет движения фронта
            FCT();

            // Пересчет граничных условий
            borderCondition();
            //borderConditionX();
            //borderConditionXFrontOblique();
            //borderConditionY();
            //borderConditionRotatedTo45();

            // буфферизация искомых функций
            for (uint i = 0; i < Nx; ++i)
            {
                for (uint j = 0; j < Ny; ++j)
                {
                    rho[i, j] = Q[i, j].W;
                    u[i, j] = Q[i, j].X / Q[i, j].W;
                    v[i, j] = Q[i, j].Y / Q[i, j].W;
                    p[i, j] = (Q[i, j].Z - 0.5f * (u[i, j] * Q[i, j].X + v[i, j] * Q[i, j].Y)) * Gamma * GMM;
                    T[i, j] = p[i, j] / rho[i, j];
                }
            }

            // Обновление дивергентных векторов
            for (uint i = 0; i < Nx; ++i)
            {
                for (uint j = 0; j < Ny; j++)
                {
                    F[i, j].W = Q[i, j].X;
                    F[i, j].X = p[i, j] / Gamma + u[i, j] * Q[i, j].X;
                    F[i, j].Y = rho[i, j] * u[i, j] * v[i, j];
                    F[i, j].Z = (p[i, j] / GMM + 0.5f * (Q[i, j].X * u[i, j] + Q[i, j].Y * v[i, j])) * u[i, j];

                    G[i, j].W = Q[i, j].Y;
                    G[i, j].X = rho[i, j] * v[i, j] * u[i, j];
                    G[i, j].Y = p[i, j] / Gamma + v[i, j] * Q[i, j].Y;
                    G[i, j].Z = (p[i, j] / GMM + 0.5f * (Q[i, j].X * u[i, j] + Q[i, j].Y * v[i, j])) * v[i, j];
                }
            }
        }
        /// <summary>
        /// Чтение входных данных задачи из файла
        /// геометрия канала по умолчанию
        /// эволюция расходов/уровней
        /// </summary>
        /// <param name="p"></param>
        public bool LoadData(string fileName)
        {
            return false;
        }
        /// <summary>
        /// Установка объектоа КЭ сетки и алгебры
        /// </summary>
        /// <param name="_mesh"></param>
        /// <param name="algebra"></param>
        public void Set(IMesh mesh, IAlgebra algebra = null)
        {

        }
        /// <summary>
        /// Установка новых отметок дна
        /// </summary>
        /// <param name="zeta"></param>
        public void SetZeta(double[] zeta, EBedErosion bedErosion)
        {
            Erosion = bedErosion;
        }
        /// <summary>
        /// Получить отметки дна
        /// </summary>
        /// <param name="zeta"></param>
        public void GetZeta(ref double[] zeta)
        {

        }

        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public IRiver Clone()
        {
            return new GasDynamics_FCT(this);
        }
        /// <summary>
        /// Создает экземпляр класса конвертера
        /// </summary>
        /// <returns></returns>
        public IOFormater<IRiver> GetFormater()
        {
            return new RiverFormatReader2DTri();
        }
        double[] b_rho = null, b_p = null, b_T = null, b_u = null, b_v = null;
        /// <summary>
        /// Формирование данных для отрисовки связанных с сеткой IMesh со сороны задачи
        /// </summary>
        /// <param name="sp">контейнер данных</param>
        public void AddMeshPolesForGraphics(ISavePoint sp)
        {
            Get(ref b_rho, ref b_p, ref b_T, ref b_u, ref b_v);
            sp.Add("Скорость ", b_u, b_v);
            sp.Add("Скорость x", b_u);
            sp.Add("Скорость y", b_v);
            sp.Add("Давление", b_rho);
            sp.Add("Плотность", b_rho);
            sp.Add("Энергия", b_T);
        }

        protected void Get(ref double[] b_rho, ref double[] b_p, ref double[] b_T, ref double[] b_u, ref double[] b_v)
        {
            int CountKnots = mesh.CountKnots;
            MEM.Alloc(CountKnots, ref b_rho);
            MEM.Alloc(CountKnots, ref b_p);
            MEM.Alloc(CountKnots, ref b_T);
            MEM.Alloc(CountKnots, ref b_u);
            MEM.Alloc(CountKnots, ref b_v);
            int k = 0;
            for (uint i = 0; i < Nx; i++)
            {
                for (uint j = 0; j < Ny; j++)
                {
                    b_u[k] = u[i, j];
                    b_v[k] = v[i, j];
                    b_rho[k] = rho[i, j];
                    b_p[k] = p[i, j];
                    b_T[k] = T[i, j];
                    k++;
                }
            }
        }

        /// <summary>
        /// Получение полей придонных касательных напряжений и давления/свободной поверхности по контексту задачи
        /// усредненных на конечных элементах
        /// </summary>
        /// <param name="tauX">придонное касательное напряжение по х</param>
        /// <param name="tauY">придонное касательное напряжение по у</param>
        /// <param name="P">придонных давления/свободная поверхности потока, по контексту задачи</param>
        public void GetTau(ref double[] tauX, ref double[] tauY, ref double[] P, ref double[][] CS, StressesFlag sf = StressesFlag.Nod)
        {
            int CountKnots = mesh.CountKnots;
            MEM.Alloc(CountKnots, ref tauX);
            MEM.Alloc(CountKnots, ref tauY);
            MEM.Alloc(CountKnots, ref P);
            // ...
        }
        #endregion

        /// <summary>
        /// установка параметров
        /// </summary>
        /// <param name="p"></param>
        public new void SetParams(object p)
        {
            base.SetParams(p);
            InitTask();
        }

        protected void InitTask()
        {
            int[] BCMark = new int[] { 0, 1, 2, 3 };
            TypeBoundCond[] BCType = new TypeBoundCond[]
            {
                TypeBoundCond.Dirichlet,
                TypeBoundCond.Neumann,
                TypeBoundCond.Dirichlet,
                TypeBoundCond.Neumann
            };
            switch (TaskIndex)
            {
                case FCTTaskIndex.ConditionX:
                    initialCondition = initialConditionX;
                    borderCondition = borderConditionX;
                    break;
                case FCTTaskIndex.ConditionY:
                    initialCondition = initialConditionY;
                    borderCondition = borderConditionY;
                    break;
                case FCTTaskIndex.CylinderDecay:
                    // Растекание водного столба
                    initialCondition = initialConditionRotatedTo45;
                    borderCondition = borderConditionRotatedTo45;
                    break;
                    //case FCTTaskIndex.Trapeziform_X:
                    //    // Начальные условия для потока вдоль X трапециевидной формы
                    //    initialCondition = initialConditionX_Trapeziform;
                    //    // Граничные условия для потока вдоль X трапециевидной формы
                    //    borderCondition = borderConditionX_Trapeziform;
                    //    break;
                    //case FCTTaskIndex.Trapeziform_Y:
                    //    initialCondition = initialConditionY_Trapeziform;
                    //    borderCondition = borderConditionY_Trapeziform;
                    //    break;
                    //case FCTTaskIndex.ParabGradient_SW:
                    //    initialCondition = initialConditionX_ParabolicGradientForm_SW;
                    //    borderCondition = borderConditionX_ParabolicGradientForm_SW;
                    //    break;
                    //case FCTTaskIndex.ParabGradient:
                    //    initialCondition = initialConditionX_ParabolicGradientForm;
                    //    borderCondition = borderConditionX_ParabolicGradientForm;
                    //    break;
                    //case FCTTaskIndex.Parab_SW:
                    //    initialCondition = initialConditionX_Parabolicform_SW;
                    //    borderCondition = borderConditionX_Parabolicform_SW;
                    //    break;
                    //case FCTTaskIndex.Parabolic:
                    //    // Начальные условия для потока вдоль X параболической формы
                    //    initialCondition = initialConditionX_Parabolicform;
                    //    // Граничные условия для потока вдоль X параболической формы
                    //    borderCondition = borderConditionX_Parabolicform;
                    //    break;
                    //case FCTTaskIndex.Vform_X:
                    //    // Начальные условия для потока вдоль X V-образной формы
                    //    initialCondition = initialConditionX_Vform;
                    //    // Граничные условия для потока вдоль X V-образной формы
                    //    borderCondition = borderConditionX_Vform;
                    //    break;
                    //case FCTTaskIndex.Dike_Х:
                    //    // Начальные условия для задачи прорыва плотины при потоке вдоль X
                    //    initialCondition = initialConditionX_Dike;
                    //    // Граничные условия для задачи прорыва плотины при потоке потока вдоль X
                    //    borderCondition = borderConditionX_Dike;
                    //    break;
            }
            memoryAllocation(BCMark);
            initialization();
        }

        public GasDynamics_FCT(RiverSW_FCTParams p) : base(p)
        {
            InitTask();
        }

        /// <summary>
        /// Функция для выделения памяти под массивы
        /// </summary>
        private void memoryAllocation(int[] BCMark)
        {
            mesh = new RectangleMesh(Lx, Ly, Nx, Ny, BCMark);
            dx = mesh.dx;
            dy = mesh.dy;
            // Выделение памяти под рабочии массивы
            MEM.Alloc2D(Nx, Ny, ref u);
            MEM.Alloc2D(Nx, Ny, ref v);
            MEM.Alloc2D(Nx, Ny, ref rho);
            MEM.Alloc2D(Nx, Ny, ref p);
            MEM.Alloc2D(Nx, Ny, ref T);
            // Вектора потоков
            MEM.Alloc2D(Nx, Ny, ref Q);
            MEM.Alloc2D(Nx, Ny, ref DxQ);
            MEM.Alloc2D(Nx, Ny, ref DyQ);
            // дивергентный вектор
            MEM.Alloc2D(Nx, Ny, ref F);
            MEM.Alloc2D(Nx, Ny, ref G);
            // Дифузия и антидифузия дивергентного вектора
            MEM.Alloc2D(Nx, Ny, ref fD);
            MEM.Alloc2D(Nx, Ny, ref gD);
            MEM.Alloc2D(Nx, Ny, ref fAD);
            MEM.Alloc2D(Nx, Ny, ref gAD);

            // Выделение памяти под вспомогательные массивы
            MEM.Alloc2D(Nx, Ny, ref regionSolver);
            MEM.Alloc2D(Nx, Ny, ref inputBoundary);
        }


        /// <summary>
        /// Инициализация начальных значений
        /// </summary>
        public void initialization()
        {
            initialCondition();
            GMM = Gamma - 1;
            GMP = Gamma + 1;
            GMR = GMM / GMP;

            SpeedJamp = 0.5f * (GMM + GMP * PressureRate) / Gamma;
            SpeedJamp = Math.Sqrt(SpeedJamp);

            for (uint i = 0; i < Nx; ++i)
            {
                for (uint j = 0; j < Ny; j++)
                {
                    Q[i, j].W = rho[i, j];
                    Q[i, j].X = rho[i, j] * u[i, j];
                    Q[i, j].Y = rho[i, j] * v[i, j];
                    Q[i, j].Z = p[i, j] / Gamma / GMM + 0.5f * rho[i, j] * (u[i, j] * u[i, j] + v[i, j] * v[i, j]);

                    F[i, j].W = Q[i, j].X;
                    F[i, j].X = p[i, j] / Gamma + u[i, j] * Q[i, j].X;
                    F[i, j].Y = rho[i, j] * u[i, j] * v[i, j];
                    F[i, j].Z = (p[i, j] / GMM + 0.5f * rho[i, j] * (u[i, j] * u[i, j] + v[i, j] * v[i, j])) * u[i, j];

                    G[i, j].W = Q[i, j].Y;
                    G[i, j].X = rho[i, j] * v[i, j] * u[i, j];
                    G[i, j].Y = p[i, j] / Gamma + v[i, j] * Q[i, j].Y;
                    G[i, j].Z = (p[i, j] / GMM + 0.5f * rho[i, j] * (u[i, j] * u[i, j] + v[i, j] * v[i, j])) * v[i, j];
                }
            }
        }


        /// <summary>
        /// Реализация алгоритма FCT на решении для текущего слоя по времени.
        /// </summary>
        public void FCT()
        {
            // антидиффузия потоков на входной границе
            for (uint i = 0; i < Nx; i++)
            {
                for (int j = 0; j < Ny; j++)
                {
                    if (inputBoundary[i, j])
                    {
                        var alpha = dtime * (u[i, j] + u[i, j + 1]) / dx;
                        var mu = ETA + 0.25f * ET2 * alpha * alpha;
                        fAD[i, j] = mu * (Q[i, j + 1] - Q[i, 0]);
                        var epsilon = dtime * (v[i, j] + v[i + 1, j]) / dy;
                        var kappa = ETA + 0.25f * ET2 * epsilon * epsilon;
                        gAD[i, j] = kappa * (Q[i + 1, j] - Q[i, j]);
                    }
                }
            }

            // расчет антидифузионного потока
            for (uint i = 1; i < Nx - 1; ++i)
            {
                for (uint j = 1; j < Ny - 1; j++)
                {
                    if (regionSolver[i, j])
                    {
                        var alpha = dtime * (u[i, j] + u[i + 1, j]) / dx;
                        var epsilon = dtime * (v[i, j] + v[i, j + 1]) / dy;
                        var mu = ETA + ET2 * alpha * alpha;
                        var kappa = ETA + ET2 * epsilon * epsilon;

                        fAD[i, j] = mu * (Q[i + 1, j] - Q[i, j]);
                        gAD[i, j] = kappa * (Q[i, j + 1] - Q[i, j]);
                        Q[i, j] = Q[i, j] + fD[i, j] - fD[i - 1, j] + gD[i, j] - gD[i, j - 1];
                        DxQ[i - 1, j] = Q[i, j] - Q[i - 1, j];
                        DyQ[i, j - 1] = Q[i, j] - Q[i, j - 1];
                    }
                }
            }

            for (uint i = 0; i < Nx; i++)
            {
                DyQ[i, Ny - 2] = Q[i, Ny - 1] - Q[i, Ny - 2];
            }
            for (uint j = 0; j < Ny; j++)
            {
                DxQ[Nx - 2, j] = Q[Nx - 1, j] - Q[Nx - 2, j];
            }

            // Ограничение антидиффузионных членов
            for (uint i = 1; i < Nx - 1; ++i)
            {
                for (uint j = 1; j < Ny - 1; j++)
                {
                    if (regionSolver[i, j])
                    {
                        fAD[i, j].W = limiter(fAD[i, j].W, DxQ[i - 1, j].W, DxQ[i + 1, j].W);
                        fAD[i, j].X = limiter(fAD[i, j].X, DxQ[i - 1, j].X, DxQ[i + 1, j].X);
                        fAD[i, j].Y = limiter(fAD[i, j].Y, DxQ[i - 1, j].Y, DxQ[i + 1, j].Y);
                        fAD[i, j].Z = limiter(fAD[i, j].Z, DxQ[i - 1, j].Z, DxQ[i + 1, j].Z);

                        gAD[i, j].W = limiter(gAD[i, j].W, DyQ[i, j - 1].W, DyQ[i, j + 1].W);
                        gAD[i, j].X = limiter(gAD[i, j].X, DyQ[i, j - 1].X, DyQ[i, j + 1].X);
                        gAD[i, j].Y = limiter(gAD[i, j].Y, DyQ[i, j - 1].Y, DyQ[i, j + 1].Y);
                        gAD[i, j].Z = limiter(gAD[i, j].Z, DyQ[i, j - 1].Z, DyQ[i, j + 1].Z);

                        // учет антидиффузии в решении
                        Q[i, j] = Q[i, j] - (fAD[i, j] - fAD[i - 1, j] + gAD[i, j] - gAD[i, j - 1]);
                    }
                }
            }
        }


        // Ограничитель функций
        double limiter(double fAD, double DQM, double DQA)
        {
            var s = Math.Sign(fAD);
            fAD = Math.Abs(fAD);
            DUM = s * DQM;
            fAD = Math.Min(fAD, DUM);
            DUM = s * DQA;
            fAD = Math.Min(fAD, DUM);
            fAD = Math.Max(fAD, 0);
            fAD = s * fAD;
            return fAD;
        }

        // ---------------------------------------------------------------------------------------
        // Тестовые НУ и ГУ
        // ---------------------------------------------------------------------------------------
        #region Начальные и граничные условия

        /// <summary>
        /// Начальные условия для потока вдоль X
        /// </summary>
        private void initialConditionX()
        {
            JampCoord0 = Lx / 2;
            IndxJamp = (uint)(JampCoord0 / dx);

            DUM = GMR + PressureRate;
            RHD = DUM / (1.0f + GMR * PressureRate);
            DIM = Math.Sqrt(2 * Gamma / GMP / DUM);
            // начальная скорость
            UD = (PressureRate - 1) * DIM / Gamma;

            // область до скачка
            for (uint i = 0; i < IndxJamp; i++)
            {
                for (uint j = 0; j < Ny; j++)
                {
                    u[i, j] = UD;
                    v[i, j] = 0f;
                    rho[i, j] = RHD;
                    p[i, j] = PressureRate;
                    T[i, j] = p[i, j] / rho[i, j];
                }
            }

            // область после скачка
            for (uint i = IndxJamp; i < Nx; i++)
            {
                for (uint j = 0; j < Ny; j++)
                {
                    u[i, j] = 0;
                    v[i, j] = 0;
                    rho[i, j] = 1;
                    p[i, j] = 1;
                    T[i, j] = 1;
                }
            }

            for (uint i = 1; i < Nx - 1; i++)
            {
                for (uint j = 1; j < Ny - 1; j++)
                {
                    regionSolver[i, j] = true;
                }
            }
            for (int j = 0; j < Ny - 1; j++)
            {
                inputBoundary[0, j] = true;
            }
        } // initialConditionX() 

        /// <summary>
        /// Граничные условия для потока вдоль X
        /// </summary>
        private void borderConditionX()
        {
            DUM = GMR + PressureRate;
            DIM = Math.Sqrt(2 * Gamma / GMP / DUM);
            var rho = DUM / (1.0f + GMR * PressureRate);
            var u = (PressureRate - 1) * DIM / Gamma;
            var v = 0.0f;
            var p = PressureRate;

            for (uint i = 1; i < Nx; i++)
            {
                Q[i, 0] = Q[i, 1];
                Q[i, Ny - 1] = Q[i, Ny - 2];
            }
            for (uint j = 0; j < Ny; j++)
            {
                Q[0, j].W = rho;
                Q[0, j].X = rho * u;
                Q[0, j].Y = rho * v;
                Q[0, j].Z = p / Gamma / GMM + 0.5f * rho * (u * u + v * v);
            }
        } // void borderConditionX()

        /// <summary>
        /// Начальные условия для потока вдоль X
        /// </summary>
        private void initialConditionXFrontOblique()
        {
            IndxJamp = (uint)(JampCoord0 / dx);
            var Xd = IndxJamp;
            DUM = GMR + PressureRate;
            RHD = DUM / (1.0f + GMR * PressureRate);
            DIM = Math.Sqrt(2 * Gamma / GMP / DUM);
            // начальная скорость
            UD = (PressureRate - 1) * DIM / Gamma;

            // область до скачка
            for (uint i = 0; i < IndxJamp; i++)
            {
                for (uint j = 0; j < Ny; j++)
                {
                    u[i, j] = UD;
                    v[i, j] = 0f;
                    rho[i, j] = RHD;
                    p[i, j] = PressureRate;
                    T[i, j] = p[i, j] / rho[i, j];
                }
            }

            for (uint i = IndxJamp; i < IndxJamp + Xd; i++)
            {
                for (uint j = 0; j < Ny; j++)
                {
                    if (j <= Ny - Ny / Xd * (i - IndxJamp))
                    {
                        u[i, j] = UD;
                        v[i, j] = 0f;
                        rho[i, j] = RHD;
                        p[i, j] = PressureRate;
                        T[i, j] = p[i, j] / rho[i, j];
                    }
                    else
                    {
                        u[i, j] = 0;
                        v[i, j] = 0;
                        rho[i, j] = 1;
                        p[i, j] = 1;
                        T[i, j] = 1;
                    }
                }
            }

            // область после скачка
            for (uint i = IndxJamp + Xd; i < Nx; i++)
            {
                for (uint j = 0; j < Ny; j++)
                {
                    u[i, j] = 0;
                    v[i, j] = 0;
                    rho[i, j] = 1;
                    p[i, j] = 1;
                    T[i, j] = 1;
                }
            }

            for (uint i = 1; i < Nx - 1; i++)
            {
                for (uint j = 1; j < Ny - 1; j++)
                {
                    regionSolver[i, j] = true;
                }
            }
            for (int j = 0; j < Ny - 1; j++)
            {
                inputBoundary[0, j] = true;
            }
        } // initialConditionXFrontOblique() 

        /// <summary>
        /// Граничные условия для потока вдоль X
        /// </summary>
        private void borderConditionXFrontOblique()
        {
            DUM = GMR + PressureRate;
            DIM = Math.Sqrt(2 * Gamma / GMP / DUM);
            var rho = DUM / (1.0f + GMR * PressureRate);
            var u = (PressureRate - 1) * DIM / Gamma;
            var v = 0.0f;
            var p = PressureRate;

            for (uint i = 1; i < Nx; i++)
            {
                Q[i, 0] = Q[i, 1];
                Q[i, Ny - 1] = Q[i, Ny - 2];
            }
            for (uint j = 0; j < Ny; j++)
            {
                Q[0, j].W = rho;
                Q[0, j].X = rho * u;
                Q[0, j].Y = rho * v;
                Q[0, j].Z = p / Gamma / GMM + 0.5f * rho * (u * u + v * v);
            }
        } // void borderConditionXFrontOblique()

        /// <summary>
        /// Начальные условия для потока вдоль Y
        /// </summary>
        private void initialConditionY()
        {
            IndxJamp = (uint)(JampCoord0 / dy);

            DUM = GMR + PressureRate;
            RHD = DUM / (1.0f + GMR * PressureRate);
            DIM = Math.Sqrt(2 * Gamma / GMP / DUM);
            // начальная скорость
            UD = (PressureRate - 1) * DIM / Gamma;
            // область до скачка
            for (uint j = 0; j < IndxJamp; j++)
            {
                for (uint i = 0; i < Nx; i++)
                {
                    u[i, j] = 0f;
                    v[i, j] = UD;
                    rho[i, j] = RHD;
                    p[i, j] = PressureRate;
                    T[i, j] = p[i, j] / rho[i, j];
                }
            }

            // область после скачка
            for (uint j = IndxJamp; j < Ny; j++)
            {
                for (uint i = 0; i < Nx; i++)
                {
                    u[i, j] = 0;
                    v[i, j] = 0;
                    rho[i, j] = 1;
                    p[i, j] = 1;
                    T[i, j] = 1;
                }
            }

            for (uint i = 1; i < Nx - 1; i++)
            {
                for (uint j = 1; j < Ny - 1; j++)
                {
                    regionSolver[i, j] = true;
                }
            }
            for (int i = 0; i < Nx - 1; i++)
            {
                inputBoundary[i, 0] = true;
            }
        } // initialConditionY()

        /// <summary>
        /// Граничные условия для потока вдоль Y
        /// </summary>
        private void borderConditionY()
        {
            DUM = GMR + PressureRate;
            DIM = Math.Sqrt(2 * Gamma / GMP / DUM);
            var rho = DUM / (1.0f + GMR * PressureRate);
            var v = (PressureRate - 1) * DIM / Gamma;
            var u = 0.0f;
            var p = PressureRate;

            for (uint j = 1; j < Nx; j++)
            {
                Q[0, j] = Q[1, j];
                Q[Nx - 1, j] = Q[Nx - 2, j];
            }
            for (uint i = 0; i < Ny; i++)
            {
                Q[i, 0].W = rho;
                Q[i, 0].X = rho * u;
                Q[i, 0].Y = rho * v;
                Q[i, 0].Z = p / Gamma / GMM + 0.5f * rho * (u * u + v * v);
            }
        } // void borderConditionY()

        /// <summary>
        /// Начальные условия для потока с углом под 45 градусов.
        /// </summary>
        private void initialCondition45()
        {
            // область после скачка
            for (uint i = 0; i < Nx; i++)
            {
                for (uint j = 0; j < Ny; j++)
                {
                    u[i, j] = 0;
                    v[i, j] = 0;
                    rho[i, j] = 1;
                    p[i, j] = 1;
                    T[i, j] = 1;
                }
            }

            var IndxJampX = (uint)(JampCoord0 / dx);
            var IndxJampY = (uint)(JampCoord0 / dy);

            DUM = GMR + PressureRate;
            RHD = DUM / (1.0f + GMR * PressureRate);
            DIM = Math.Sqrt(2 * Gamma / GMP / DUM);
            // начальная скорость
            var UD = (PressureRate - 1) * DIM / Gamma * (double)Math.Cos(Math.PI / 4);
            var VD = (PressureRate - 1) * DIM / Gamma * (double)Math.Sin(Math.PI / 4);
            // область до скачка
            for (uint i = 0; i < IndxJampX; i++)
            {
                for (uint j = 0; j < IndxJampY - i; j++)
                {
                    u[i, j] = UD;
                    v[i, j] = VD;
                    rho[i, j] = RHD;
                    p[i, j] = PressureRate;
                    T[i, j] = p[i, j] / rho[i, j];
                }
            }
        }

        /// <summary>
        /// Начальные условия для прямоугольной области потока вдоль X
        /// </summary>
        private void initialConditionSquareAlongX()
        {
            IndxJamp = (uint)(JampCoord0 / dx);

            DUM = GMR + PressureRate;
            RHD = DUM / (1.0f + GMR * PressureRate);
            DIM = Math.Sqrt(2 * Gamma / GMP / DUM);
            // начальная скорость
            UD = (PressureRate - 1) * DIM / Gamma;

            // область вне скачка
            for (uint i = 0; i < Nx; i++)
            {
                for (uint j = 0; j < Ny; j++)
                {
                    u[i, j] = 0;
                    v[i, j] = 0;
                    rho[i, j] = 1;
                    p[i, j] = 1;
                    T[i, j] = 1;
                }
            }

            // область скачка
            for (uint i = IndxJamp - IndxJamp / 2; i < IndxJamp + IndxJamp / 2; i++)
            {
                for (int j = Ny / 4; j < 3 * Ny / 4; j++)
                {
                    u[i, j] = UD;
                    v[i, j] = 0f;
                    rho[i, j] = RHD;
                    p[i, j] = PressureRate;
                    T[i, j] = p[i, j] / rho[i, j];
                }
            }
        }

        /// <summary>
        /// Начальные условия для прямоугольной области потока вдоль X
        /// </summary>
        private void initialConditionPartialFlowAlongX()
        {
            IndxJamp = (uint)(JampCoord0 / dx);

            DUM = GMR + PressureRate;
            RHD = DUM / (1.0f + GMR * PressureRate);
            DIM = Math.Sqrt(2 * Gamma / GMP / DUM);
            // начальная скорость
            UD = (PressureRate - 1) * DIM / Gamma;

            // область вне скачка
            for (uint i = 0; i < Nx; i++)
            {
                for (uint j = 0; j < Ny; j++)
                {
                    u[i, j] = 0;
                    v[i, j] = 0;
                    rho[i, j] = 1;
                    p[i, j] = 1;
                    T[i, j] = 1;
                }
            }

            var CountYNodesQ = (Ny - 1) / 4;
            // область скачка
            for (uint i = 0; i < IndxJamp; i++)
            {
                for (int j = CountYNodesQ; j <= 3 * CountYNodesQ; j++)
                {
                    u[i, j] = UD;
                    v[i, j] = 0f;
                    rho[i, j] = RHD;
                    p[i, j] = PressureRate;
                    T[i, j] = p[i, j] / rho[i, j];
                }
            }
        }

        /// <summary>
        /// Начальные условия для области, повернутой на 45 градусов
        /// </summary>
        private void initialConditionRotatedTo45()
        {
            //var IndxXJump = (uint)(JampCoord0 / dx);
            var IndxYJump = (uint)(JampCoord0 / dy);

            DUM = GMR + PressureRate;
            RHD = DUM / (1.0f + GMR * PressureRate);
            DIM = Math.Sqrt(2 * Gamma / GMP / DUM);
            UD = (PressureRate - 1) * DIM / Gamma;

            var X0 = Nx / 2;
            var Y0 = Ny / 2;

            var cos = (double)Math.Cos(Math.PI / 4);
            var sin = (double)Math.Sin(Math.PI / 4);

            for (int i = 0; i < Nx; i++)
            {
                for (int j = 0; j < Ny; j++)
                {
                    var lb = j >= Y0 - i;
                    var lt = j <= Y0 + i;
                    var rb = j >= i - Y0;
                    var rt = j <= Y0 + IndxYJump - 1 - i;
                    if ((lb && lt) && (rb && rt))
                    {
                        // область скачка
                        u[i, j] = UD * cos;
                        v[i, j] = UD * sin;
                        rho[i, j] = RHD;
                        p[i, j] = PressureRate;
                        T[i, j] = p[i, j] / rho[i, j];
                    }
                    else
                    {
                        // область скачка
                        u[i, j] = 0f;
                        v[i, j] = 0f;
                        rho[i, j] = 1f;
                        p[i, j] = 1f;
                        T[i, j] = 1f;
                    }
                }
            }
            for (int i = 0; i < Nx; i++)
            {
                for (int j = 0; j < Ny; j++)
                {
                    var lb = j > Y0 - i;
                    var lt = j < Y0 + i;
                    var rb = j > i - Y0;
                    var rt = j < Y0 + Ny - 1 - i;
                    if ((lb && lt) && (rb && rt))
                    {
                        // область скачка
                        regionSolver[i, j] = true;
                    }
                }
            }
            for (uint i = 0; i <= X0; i++)
            {
                inputBoundary[i, Y0 - i] = true;
            }

        } // initialConditionRotatedTo45()

        /// <summary>
        /// Граничные условия для области, повернутой на 45 градусов
        /// </summary>
        private void borderConditionRotatedTo45()
        {
            uint X0 = (uint)Nx / 2;
            uint Y0 = (uint)Ny / 2;
            var cos = Math.Cos(Math.PI / 4);
            var sin = Math.Sin(Math.PI / 4);

            DUM = GMR + PressureRate;
            DIM = Math.Sqrt(2 * Gamma / GMP / DUM);
            UD = (PressureRate - 1) * DIM / Gamma;
            var u = UD * cos;
            var v = UD * sin;
            var rho = DUM / (1.0f + GMR * PressureRate);
            var p = PressureRate;

            //for (uint i = 0; i <= X0; i++) {
            //    Q[i, Y0 + i] = Q[i + 1, Y0 + i];
            //}
            //for (uint i = X0 + 1; i < Nx - 1; i++) {
            //    Q[i, i - Y0] = Q[i, i - Y0+1];
            //}

            for (uint i = 0; i < X0; i++)
            {
                Q[i, i + Y0] = Q[i + 1, Y0 + i - 1];
            }
            for (uint i = X0; i < Nx - 1; i++)
            {
                Q[i, i - Y0] = Q[i - 1, i - Y0 + 1];
            }

            for (uint i = 0; i <= X0; i++)
            {
                Q[i, Y0 - i].W = rho;
                Q[i, Y0 - i].X = rho * u;
                Q[i, Y0 - i].Y = rho * v;
                Q[i, Y0 - i].Z = p / Gamma / GMM + 0.5f * rho * (u * u + v * v);
            }
        } // void borderConditionRotatedTo45()

        /// <summary>
        /// Начальные условия нулевые
        /// </summary>
        private void initialConditionEmpty()
        {
            for (uint i = 0; i < Nx; i++)
            {
                for (uint j = 0; j < Ny; j++)
                {
                    u[i, j] = 0f;
                    v[i, j] = 0f;
                    rho[i, j] = 1f;
                    p[i, j] = 1f;
                    T[i, j] = 1f;
                }
            }
        }

        /// <summary>
        /// Граничные условия
        /// </summary>
        private void borderCondition0()
        {
            for (int i = 0; i < Nx; ++i)
            {
                Q[i, 0] = Q[i, 1];
                Q[i, Ny - 1] = Q[i, Ny - 2];
            }
            for (int j = 0; j < Ny; ++j)
            {
                Q[0, j] = Q[1, j];
                Q[Nx - 1, j] = Q[Nx - 2, j];
            }
            for (int j = 0; j < Ny; ++j)
            {
                Q[0, j] = Q[1, j];
                Q[Nx - 1, j] = Q[Nx - 2, j];
            }
        }


        #endregion



    }
}
