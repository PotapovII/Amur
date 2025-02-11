////---------------------------------------------------------------------------
////                          ПРОЕКТ  "DISER"
////                  создано  :   9.03.2007 Потапов И.И.
////---------------------------------------------------------------------------
////                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
////            перенесено с правкой : 06.12.2020 Потапов И.И. 
////            создание родителя : 21.02.2022 Потапов И.И. 
////---------------------------------------------------------------------------
////               Реверс в NPRiverLib 09.02.2024 Потапов И.И.
////---------------------------------------------------------------------------
////                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
////                         проектировщик:
////                           Потапов И.И.
////---------------------------------------------------------------------------
////                 кодировка : 09.02.2024 Потапов И.И.
////---------------------------------------------------------------------------
////                      Сбока No Property River Lib
////              с убранными из наследников Property классами 
////---------------------------------------------------------------------------
//namespace NPRiverLib.APRiver1YD
//{
//    using MemLogLib;
//    using AlgebraLib;
//    using GeometryLib;
//    using NPRiverLib.IO;

//    using CommonLib;
//    using CommonLib.IO;
//    using CommonLib.Mesh;
//    using CommonLib.Geometry;
//    using CommonLib.Physics;
//    using CommonLib.Delegate;
//    using CommonLib.Function;
//    using CommonLib.ChannelProcess;

//    using System;
//    using System.Linq;

//    using MeshLib.Wrappers;
//    using NPRiverLib.ABaseTask;
//    using FEMTasksLib.FESimpleTask;
//    using NPRiverLib.APRiver1YD.Params;
//    using MeshGeneratorsLib.StripGenerator;
//    /// <summary>
//    ///  ОО: Определение класса TriSroSecRiverTask1YD - расчет полей скорости, вязкости 
//    ///       и напряжений в живом сечении потока методом КЭ (симплекс элементы)
//    /// </summary>
//    [Serializable]
//    public class TriRoseRiverTask1YD : APRiverFEM1YD
//    {
//        #region Локальные поля и методы задачи Розовского
//        /// <summary>
//        /// Параметр релаксации для заадчи вихрь - функция тока
//        /// </summary>
//        protected double w = 0.3;
//        /// <summary>
//        /// Количество узлов по дну
//        /// </summary>
//        protected int NN = 100;
//        /// <summary>
//        /// Индекс створа
//        /// </summary>
//        protected bool BoundaryG2_UxChecked = true;
//        protected bool smoothingChecked = true;
//        protected int[] bKnotsU = null;
//        protected int[] bKnotsV = null;
//        protected double[] mBC_V = null;
//        protected double[] mBC_U = null;
//        protected double[] mbU = null;
//        protected double[] mbV = null;
//        protected uint[] mAdressU = null;
//        protected double[] YRose2 = { -0.16, 0, 0.12, 0.4, 0.65, 0.8, 0.95, 1.2, 1.48, 1.6, 1.76 };
//        protected double[] ZRose2 = { 0.204, 0.1401, 0.1, 0.028625, 0, 0, 0, 0.028625, 0.1, 0.1401, 0.204 };
//        protected double WL = 0.14;
//        protected IDigFunction VelosityUx = null;
//        protected IDigFunction VelosityUy = null;
//        /// <summary>
//        /// Вычисление скорости Ux
//        /// </summary>
//        protected Function<double, double> CalkU = null;
//        #endregion
//        protected RiverSedimentTri_1Y riverSediment = null;
//        /// <summary>
//        /// Концентрация частиц
//        /// </summary>
//        protected double[] СonSediment;
//        /// <summary>
//        /// Задача для расчета полей гидродинамики в створе канала
//        /// </summary>
//        protected RiverCrossVortexPhiTri taskVortexPhi = null;
//        /// <summary>
//        /// Конструктор
//        /// </summary>
//        public TriRoseRiverTask1YD() : this(new RSCrossParams()) { }
//        /// <summary>
//        /// Конструктор
//        /// </summary>
//        public TriRoseRiverTask1YD(RSCrossParams p) : base(p)
//        {
//            Version = "TriRoseRiverTask1YD 20.06.2024";
//            name = "Поток в створе канала Розовского 1YD";
//        }

//        /// <summary>
//        /// Обертка для работы с КЭ сеткой и вычисления алгебраической турбулентной вязкости
//        /// </summary>
//        protected IMWCross wMesh = null;

//        #region Локальные методы
//        /// <summary>
//        /// Инициализация задачи
//        /// </summary>
//        protected override void InitTask()
//        {
//            double g = SPhysics.GRAV;
//            double rho_w = SPhysics.rho_w;
//            Q = rho_w * g * Params.J;
//            // получение отметок дна
//            Geometry.GetFunctionData(ref bottom_x, ref bottom_y, Params.CountBLKnots);
//            // начальный уровень свободной поверхности
//            waterLevel = WaterLevels.FunctionValue(0);
//            // начальный расход потока
//            riverFlowRate = FlowRate.FunctionValue(0);
//        }

//        #endregion 

        
//        #region Искомые поля
//        /// <summary>
//        /// Поле скорости
//        /// </summary>
//        public double[] Uy;
//        /// <summary>
//        /// Правая часть
//        /// </summary>
//        public double[] mQ = null;
//        /// <summary>
//        /// Поле скорости
//        /// </summary>
//        public double[] Uz;
//        /// <summary>
//        /// Поле напряжений T_zz
//        /// </summary>
//        public double[] TauZZ;
//        /// <summary>
//        /// Поле напряжений T_yy
//        /// </summary>
//        public double[] TauYY;
//        /// <summary>
//        /// Поле напряжений T_yz
//        /// </summary>
//        public double[] TauYZ;
//        /// <summary>
//        /// Поле вихря
//        /// </summary>
//        public double[] Vortex;
//        /// <summary>
//        /// Поле функции тока
//        /// </summary>
//        public double[] Phi;

//        #endregion

//        #region Локальные методы задачи Розовского
//        /// <summary>
//        /// Скорость в створе 15
//        /// </summary>
//        /// <param name="y"></param>
//        /// <returns></returns>
//        double CalkU15(double y)
//        {
//            double V =
//                    -0.4823745080e-2 * Math.Exp(2.230789734 * y)
//                   - 0.1668512549 * Math.Exp(-3.659361162 * y) + 0.1716750000;
//            return 1.096569401 * Math.Sqrt(Math.Max(0, V));
//        }
//        /// <summary>
//        /// Скорость в створе 15
//        /// </summary>
//        /// <param name="y"></param>
//        /// <returns></returns>
//        double CalkU18(double y)
//        {
//            double V = -0.7744248317e-2 * Math.Exp(1.513732350 * y) -
//                0.1639307517 * Math.Exp(-.4148312511 * y) + 0.1716750000;
//            return 2.457935972 * Math.Sqrt(Math.Max(0, V));
//        }
//        /// <summary>
//        /// Скорость в створе 15
//        /// </summary>
//        /// <param name="y"></param>
//        /// <returns></returns>
//        double CalkU26(double y)
//        {
//            double V = -0.3580698312e-5 * Math.Exp(5.606508095 * y)
//                - .1716714194 * Math.Exp(-.1120026006 * y) + .1716750000;
//            return 3.285684719 * Math.Sqrt(Math.Max(0, V));
//        }

//        #endregion

//        /// <summary>
//        /// Создать расчетную область
//        /// </summary>
//        public void DefaultCalculationDomain()
//        {
//            CreateCalculationDomain();
//            eTaskReady = ETaskStatus.TaskReady;
//        }

//        /// <summary>
//        /// Создать расчетную область
//        /// </summary>
//        /// <param name="right"></param>
//        /// <param name="left"></param>
//        protected override void CreateCalculationDomain()
//        {
//            meshGenerator = new HStripMeshGeneratorTri();
//            //meshGenerator = new CrossStripMeshGenerator(TypeMesh.Triangle);
            



//            WetBed = 0;
//            waterLevel = 0.14;
//            // генерация сетки
//            switch (Params.crossSectionNamber)
//            {
//                case 0: // Створ 15
//                    {
//                        // 17 04 24
//                        double[] Y_U15 = { 0, 0.1, 0.3, 0.55, 0.8, 1.05, 1.2, 1.5, 1.6 };
//                        double[] U15 = { 0, 0.243, 0.365, 0.38, 0.38, 0.365, 0.345, 0.186, 0 };

//                        double[] Y_V15 = { 0, 0.55, 0.8, 1.05, 1.2, 1.6 };
//                        double[] V15 = { 0, 0.03, 0.028, 0.028, 0.038, 0 };

//                        VelosityUx = new DigFunction(Y_U15, U15, "Створ");
//                        //VelosityUy = new DigFunction(YRose2, V15, "Створ");
//                        VelosityUy = new DigFunction(Y_V15, V15, "Створ");
//                        CalkU = CalkU15;
//                    }
//                    break;
//                case 1: // Створ 18
//                    {
//                        // 17 04 24
//                        double[] Y_U18 = { 0, 0.15, 0.4, 0.6, 0.8, 1.07, 1.25, 1.4, 1.6 };
//                        double[] U18 = { 0, 0.2, 0.32, 0.38, 0.4, 0.4, 0.38, 0.26, 0 };

//                        double[] Y_V18 = { 0, 0.4, 0.6, 0.8, 1.07, 1.25, 1.6 };
//                        double[] V18 = { 0, 0.028, 0.039, 0.041, 0.039, 0.032, 0 };

//                        VelosityUx = new DigFunction(Y_U18, U18, "Створ");
//                        //VelosityUy = new DigFunction(YRose2, V18, "Створ");
//                        VelosityUy = new DigFunction(Y_V18, V18, "Створ");
//                        CalkU = CalkU18;
//                    }
//                    break;
//                case 2: // Створ 21
//                    {
//                        // 17 04 24
//                        double[] Y_U21 = { 0, 0.17, 0.5, 0.8, 1.13, 1.45, 1.6 };
//                        double[] U21 = { 0, 0.16, 0.35, 0.4, 0.45, 0.37, 0 };

//                        double[] Y_V21 = { 0, 0.5, 0.8, 1.13, 1.6 };
//                        double[] V21 = { 0, 0.022, 0.021, 0.021, 0 };

//                        VelosityUx = new DigFunction(Y_U21, U21, "Створ");
//                        //VelosityUy = new DigFunction(YRose2, V21, "Створ");
//                        VelosityUy = new DigFunction(Y_V21, V21, "Створ");
//                        CalkU = CalkU26;
//                    }
//                    break;
//                case 3: // Створ 26
//                    {
//                        // double[] U26 = { 0, 0.15, 0.29, 0.35, 0.38, 0.40, 0.43, 0.33, 0 };
//                        // нормированные к расходу скорости
//                        double[] U26 = { 0, 0.1539, 0.2977, 0.359, 0.39, 0.41, 0.441, 0.3387, 0 };
//                        double[] V26 = { 0, 0.003, 0.011, 0.015, 0.018, 0.016, 0.013, 0.01, 0 };
//                        VelosityUx = new DigFunction(YRose2, U26, "Створ");
//                        VelosityUy = new DigFunction(YRose2, V26, "Створ");
//                        CalkU = CalkU26;
//                    }
//                    break;
//                case 4: // Тест
//                    {
//                        // double[] U26 = { 0, 0.15, 0.29, 0.35, 0.38, 0.40, 0.43, 0.33, 0 };
//                        // нормированные к расходу скорости
//                        double[] U26 = { 0, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0 };
//                        double[] V26 = { 0, 0.05, 0.05, 0.05, 0.05, 0.05, 0.05, 0.05, 0 };
//                        VelosityUx = new DigFunction(YRose2, U26, "Створ");
//                        VelosityUy = new DigFunction(YRose2, V26, "Створ");
//                    }
//                    break;
//            }

//            // геометрия дна
//            if (Params.crossFormGeometry == CrossFormGeometry.Trapezoidal)
//                Geometry = new DigFunction(YRose2, ZRose2, "Створ");
//            else
//                Geometry = new FunctionСhannelRose();

//            // свободная поверхность
//            WaterLevels = new DigFunction(YRose2[YRose2.Length-1], waterLevel, "Створ");
//            // расход потока
//            double Flow = 0.054;
//            FlowRate = new DigFunction(3600, Flow, "Створ");
            
//            InitTask();
//            evolution.Clear();
//            NN = Params.CountKnots;
//            // получение отметок дна
//            Geometry.GetFunctionData(ref bottom_x, ref bottom_y, NN);

//            CreateMesh();
//        }

//        protected void BC(bool BoundaryG2_UxChecked, bool smoothingChecked,
//                          ref int[] bKnotsU, ref int[] bKnotsV, 
//                          ref double[] mBC_U, ref double[] mBC_V,
//                          ref double[] mbU, ref double[] mbV)
//        {
            

//            double[] X = mesh.GetCoords(0);
//            TwoElement[] belems = mesh.GetBoundElems();
//            int[] bMarks = mesh.GetBElementsBCMark();

//            for (uint be = 0; be < bMarks.Length; be++)
//            {
//                int Mark = bMarks[be];
//                uint idx1 = belems[be].Vertex1;
//                double x1 = X[idx1];
//                uint idx2 = belems[be].Vertex2;
//                double x2 = X[idx2];
//                if (Mark == 0) // дно канала
//                {
//                    bKnotsU[idx1] = 1;
//                    mBC_U[idx1] = 0;
//                    bKnotsU[idx2] = 1;
//                    mBC_U[idx2] = 0;
//                }
//                if (Mark == 2) // свободная поверхность
//                {
//                    if (BoundaryG2_UxChecked == true)
//                    {
//                        //double U1 = VelosityUx.FunctionValue(x1);
//                        //double U2 = VelosityUx.FunctionValue(x2);
//                        //Console.WriteLine(" U1 {0:F4}  U2 {0:F4} i1 {2}  i2 {3}", U1, U2, idx1, idx2);
//                        bKnotsU[idx1] = 1;
//                        bKnotsU[idx2] = 1;
//                        if (smoothingChecked == true && CalkU != null)
//                        {
//                            mBC_U[idx1] = CalkU(x1);
//                            mBC_U[idx2] = CalkU(x2);
//                        }
//                        else
//                        {
//                            mBC_U[idx1] = VelosityUx.FunctionValue(x1);
//                            mBC_U[idx2] = VelosityUx.FunctionValue(x2);
//                        }
//                    }
//                    bKnotsV[idx1] = 1;
//                    mBC_V[idx1] = VelosityUy.FunctionValue(x1);
//                    bKnotsV[idx2] = 1;
//                    mBC_V[idx2] = VelosityUy.FunctionValue(x2);
//                }
//            }
            
//            int CountU = bKnotsU.Sum(xx => xx > 0 ? xx : 0);
//            int CountV = bKnotsV.Sum(xx => xx > 0 ? xx : 0);
//            MEM.Alloc(CountU, ref mbU, 0);
//            MEM.Alloc(CountU, ref mAdressU);
//            MEM.Alloc(CountV, ref mbV, 0);
//            int k = 0;
//            for (uint i = 0; i < mBC_U.Length; i++)
//            {
//                if (bKnotsU[i] == 1)
//                {
//                    mAdressU[k] = i;
//                    mbU[k++] = mBC_U[i];
//                }
//            }
//            k = 0;
//            for (int i = 0; i < mBC_V.Length; i++)
//            {
//                if (bKnotsV[i] == 1)
//                    mbV[k++] = mBC_V[i];
//            }
//        }
//        /// <summary>
//        /// Создание расчетной области и инициализация задачи
//        /// </summary>
//        protected void CreateMesh()
//        {
//            mesh = meshGenerator.CreateMesh(ref WetBed, waterLevel, bottom_x, bottom_y);

//           /// mesh = (TriMesh)((ComplecsMesh)meshGenerator.CreateMesh(ref WetBed, waterLevel, bottom_x, bottom_y));

//            right = meshGenerator.Right();
//            left = meshGenerator.Left();

//            double R_midle = Params.midleRadius - YRose2[YRose2.Length - 1] / 2;
//            int cst = (int)Params.crossSectionType;

//            wMesh = new MWCrossSectionTri(mesh, R_midle, cst, 
//                        false, SСhannelForms.porabolic);

//            // TO DO подумать о реклонировании algebra если размер матрицы не поменялся 
//            if (Params.сrossAlgebra ==  CrossAlgebra.TapeGauss)
//            {
//                // получение ширины ленты для алгебры
//                int NH = (int)mesh.GetWidthMatrix() + 1;
//                algebra = new AlgebraLUTape((uint)mesh.CountKnots, NH, NH);
//            }
//            else
//                algebra = new SparseAlgebraBeCG((uint)mesh.CountKnots, false);

//            taskVortexPhi = new RiverCrossVortexPhiTri(wMesh, algebra, TypeTask.streamY1D, w);

//            riverSediment = new RiverSedimentTri_1Y(wMesh, algebra, TypeTask.streamY1D);

//            if (taskPoisson == null)
//                taskPoisson = new FEPoissonTaskTri(mesh, algebra);
//            else
//                taskPoisson.SetTask(mesh, algebra);

//            MEM.Alloc(mesh.CountKnots, ref eddyViscosity, 0.3);
//            MEM.Alloc(mesh.CountKnots, ref Ux, "Ux");
//            MEM.Alloc(mesh.CountKnots, ref Uy, "Uy");
//            MEM.Alloc(mesh.CountKnots, ref Uz, "Uz");
//            // память под напряжения в области
//            MEM.Alloc(mesh.CountKnots, ref TauY, "TauY");
//            MEM.Alloc(mesh.CountKnots, ref TauZ, "TauZ");

//            MEM.Alloc(mesh.CountKnots, ref TauYY, "TauYY");
//            MEM.Alloc(mesh.CountKnots, ref TauZZ, "TauZZ");
//            MEM.Alloc(mesh.CountKnots, ref TauYZ, "TauYZ");

//            MEM.Alloc(mesh.CountKnots, ref Vortex, "Vortex");
//            MEM.Alloc(mesh.CountKnots, ref Phi, "Phi");

//            MEM.Alloc(Params.CountKnots, ref tau, "tau");

//            MEM.Alloc(mesh.CountKnots, ref mQ, Q);

//            unknowns.Clear();
//            unknowns.Add(new Unknown("Скорость Ux", Ux, TypeFunForm.Form_2D_Rectangle_L1));
//            unknowns.Add(new Unknown("Поле вихря Vortex", Vortex, TypeFunForm.Form_2D_Rectangle_L1));
//            unknowns.Add(new Unknown("Поле тока Phi", Phi, TypeFunForm.Form_2D_Rectangle_L1));
//            unknowns.Add(new Unknown("Турб. вязкость", eddyViscosity, TypeFunForm.Form_2D_Rectangle_L1));
//            unknowns.Add(new Unknown("Скорость Uy", Uy, TypeFunForm.Form_2D_Rectangle_L1));
//            unknowns.Add(new Unknown("Скорость Uz", Uz, TypeFunForm.Form_2D_Rectangle_L1));
//            unknowns.Add(new CalkPapams("Поле скорость", Uy, Uz, TypeFunForm.Form_2D_Rectangle_L1));
//            unknowns.Add(new CalkPapams("Поле напряжений T_xy", TauY, TypeFunForm.Form_2D_Rectangle_L1));
//            unknowns.Add(new CalkPapams("Поле напряжений T_xz", TauZ, TypeFunForm.Form_2D_Rectangle_L1));
//            unknowns.Add(new CalkPapams("Поле напряжений T_yy", TauYY, TypeFunForm.Form_2D_Rectangle_L1));
//            unknowns.Add(new CalkPapams("Поле напряжений T_zz", TauZZ, TypeFunForm.Form_2D_Rectangle_L1));
//            unknowns.Add(new CalkPapams("Поле напряжений T_yz", TauYZ, TypeFunForm.Form_2D_Rectangle_L1));
//            unknowns.Add(new CalkPapams("Поле напряжений", TauY, TauZ, TypeFunForm.Form_2D_Rectangle_L1));

//            MEM.Alloc(mesh.CountKnots, ref bKnotsU, -1);
//            MEM.Alloc(mesh.CountKnots, ref bKnotsV, -1);
//            MEM.Alloc(mesh.CountKnots, ref mBC_U, 0);
//            MEM.Alloc(mesh.CountKnots, ref mBC_V, 0);

//            BC(BoundaryG2_UxChecked, smoothingChecked,
//                          ref bKnotsU, ref bKnotsV,
//                          ref mBC_U, ref mBC_V,
//                          ref mbU, ref mbV);
//        }
//        /// <summary>
//        /// Получение полей придонных касательных напряжений и давления/свободной 
//        /// поверхности по контексту задачи усредненных на конечных элементах
//        /// </summary>
//        /// <param name="tauX">придонное касательное напряжение по х</param>
//        /// <param name="tauY">придонное касательное напряжение по у</param>
//        /// <param name="P">придонных давления/свободная поверхности потока, по контексту задачи</param>
//        public override void GetTau(ref double[] _tauX, ref double[] _tauY, ref double[] P,
//                ref double[][] CS, StressesFlag sf = StressesFlag.Nod)
//        {
//            _tauX = tau;
//            if (Erosion == EBedErosion.SedimentErosion ||
//                Erosion == EBedErosion.BedLoadAndSedimentErosion)
//            {
//                if (CS == null) CS = new double[1][];
//                GetCS(ref CS[0]);
//            }
//        }
//        /// <summary>
//        /// Интеграл концентации по глубине канала 
//        /// </summary>
//        /// <param name="CS"></param>
//        protected void GetCS(ref double[] CS)
//        {
//            //MEM.Alloc(Ny - 1, ref CS);
//            //for (int j = 0; j < Ny - 1; j++)
//            //{
//            //    CS[j] = 0;
//            //    for (int i = 0; i < Nx - 1; i++)
//            //        CS[j] += v[i][j] * t[i][j] * Hx[i][j];
//            //}
//        }
//        public override void SolverStep()
//        {
//            int flagErr = 0;
//            try
//            {
//                    // расчет уровня свободной поверхности реки
//                    bool watelLevelChange = CalkWaterLevel();
//                    flagErr++;
//                    // определение расчетной области потока и построение КЭ сетки
//                    if (watelLevelChange == true || Erosion != EBedErosion.NoBedErosion)
//                        CreateMesh();
//                    flagErr++;
//                    // расчет гидрадинамики  (скоростей потока)
//                    int VortexBC_G2 = (int) Params.bcTypeVortex;
//                    SPhysics.PHYS.turbViscType =  Params.turbViscTypeA;

//                    bool flagLes = false;
//                    double[] eddyViscosityUx = null;
//                    double[] RR = null;
//                    // Искомые поля
//                    taskVortexPhi.CalkVortexStream(
//                    ref Phi,  ref Vortex, ref Ux, ref Uy, ref Uz, ref eddyViscosityUx, ref eddyViscosity, 
//                    ref TauY, ref TauZ, ref TauYY, ref TauYZ, ref TauZZ, ref RR,
//                    // Граничные условия для потоковой скорости и боковой скорости на свободной поверхности
//                    mbU, mAdressU, mbV, mQ, VortexBC_G2, Params.velocityOnWL, VortexBC_G2, Params.typeEddyViscosity, 
//                    flagLes,(int)Params.turbViscTypeB);


//                    flagErr++;
//                    // расчет  придонных касательных напряжений на дне
//                    tau = TausToVols(in bottom_x, in bottom_y);
//                    flagErr++;

//                    unknowns.Clear();
//                    unknowns.Add(new Unknown("Скорость Ux", Ux));
//                    unknowns.Add(new Unknown("Поле вихря Vortex", Vortex));
//                    unknowns.Add(new Unknown("Поле тока Phi", Phi));
//                    unknowns.Add(new Unknown("Турб. вязкость_X ", eddyViscosityUx));
//                    unknowns.Add(new Unknown("Турб. вязкость_YZ", eddyViscosity));
//                    unknowns.Add(new Unknown("Скорость Uy", Uy));
//                    unknowns.Add(new Unknown("Скорость Uz", Uz));
//                    unknowns.Add(new CalkPapams("Поле скорости", Uy, Uz));
//                    if(RR!=null) unknowns.Add(new CalkPapams("Radius", RR));
//                    unknowns.Add(new CalkPapams("Поле напряжений T_xy", TauY));
//                    unknowns.Add(new CalkPapams("Поле напряжений T_xz", TauZ));
//                    unknowns.Add(new CalkPapams("Поле напряжений T_yy", TauYY));
//                    unknowns.Add(new CalkPapams("Поле напряжений T_zz", TauZZ));
//                    unknowns.Add(new CalkPapams("Поле напряжений T_yz", TauYZ));
//                    unknowns.Add(new CalkPapams("Поле напряжений", TauY, TauZ));
//                    if (Erosion == EBedErosion.SedimentErosion || Erosion == EBedErosion.BedLoadAndSedimentErosion)
//                    {
//                        if (riverSediment == null)
//                            riverSediment = new RiverSedimentTri_1Y(wMesh, algebra, TypeTask.streamY1D);
//                        riverSediment.CalkСonSediment(ref СonSediment, Ux, Uy, Uz, eddyViscosity, tau);
//                        unknowns.Add(new CalkPapams("Концентрация", СonSediment));
//                    }
//                    // сохранение данных в начальный момент времени
//                    flagErr++;
//                    time += dtime;
//            }
//            catch (Exception ex)
//            {
//                string ErrorName = RiverError.ErrorName(flagErr);
//                Logger.Instance.Error(ErrorName, "ASectionalRiverTask");
//                Logger.Instance.Exception(ex);
//            }
//        }
//        /// <summary>
//        /// Формирование данных для отрисовки связанных с сеткой IMesh со сороны задачи
//        /// </summary>
//        /// <param name="sp">контейнер данных</param>
//        public override void AddMeshPolesForGraphics(ISavePoint sp)
//        {
//            base.AddMeshPolesForGraphics(sp);
//            // кривые 
//            // дно - берег
//            sp.AddCurve("Русловой профиль", bottom_x, bottom_y);
//            //if (left != null && right != null)
//            //{
//            //    double[] xwl = { left.x, right.x };
//            //    double[] ywl = { left.y, right.y };
//            //    // свободная поверхность
//            //    sp.AddCurve("Свободная поверхность", xwl, ywl);
//            //    Scan();
//            //}
//            if (evolution.Count > 1)
//            {
//                double[] times = (from arg in evolution select arg.time).ToArray();
//                double[] wls = (from arg in evolution select arg.waterLevel).ToArray();
//                sp.AddCurve("Эв.св.поверхности", times, wls, TypeGraphicsCurve.TimeCurve);
//                double[] mus = (from arg in evolution select arg.eddyViscosityConst).ToArray();
//                sp.AddCurve("Вязкость", times, mus, TypeGraphicsCurve.TimeCurve);
//                double[] tm = (from arg in evolution select arg.tauMax).ToArray();
//                sp.AddCurve("Tau максимум", times, tm, TypeGraphicsCurve.TimeCurve);
//                tm = (from arg in evolution select arg.tauMid).ToArray();
//                sp.AddCurve("Tau средние", times, tm, TypeGraphicsCurve.TimeCurve);
//                double[] gr = (from arg in evolution select arg.WetBed).ToArray();
//                sp.AddCurve("Гидравл. радиус", times, gr, TypeGraphicsCurve.TimeCurve);
//                double[] ar = (from arg in evolution select arg.Area).ToArray();
//                sp.AddCurve("Площадь сечения", times, ar, TypeGraphicsCurve.TimeCurve);
//                double[] rfr = (from arg in evolution select arg.riverFlowRate).ToArray();
//                sp.AddCurve("Расход потока", times, rfr, TypeGraphicsCurve.TimeCurve);
//                rfr = (from arg in evolution select arg.riverFlowRateCalk).ToArray();
//                sp.AddCurve("Текущий расчетный расход потока", times, rfr, TypeGraphicsCurve.TimeCurve);
//            }
//        }

//        /// <summary>
//        /// Создает экземпляр класса
//        /// </summary>
//        /// <returns></returns>
//        public override IRiver Clone()
//        {
//            return new TriRoseRiverTask1YD(Params);
//        }
//        /// <summary>
//        /// Создает экземпляр класса конвертера
//        /// </summary>
//        /// <returns></returns>
//        public override IOFormater<IRiver> GetFormater()
//        {
//            return new TaskReader1YD_Rose();
//        }
//        /// <summary>
//        /// Создает список тестовых задач для загрузчика по умолчанию
//        /// </summary>
//        /// <returns></returns>
//        //public override List<string> GetTestsName()
//        //{
//        //    List<string> strings = new List<string>();
//        //    strings.Add("Основная задача - тестовая");
//        //    return strings;
//        //}
//    }
//}
