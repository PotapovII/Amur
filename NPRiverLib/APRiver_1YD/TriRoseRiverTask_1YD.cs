//---------------------------------------------------------------------------
//                          ПРОЕКТ  "DISER"
//                  создано  :   9.03.2007 Потапов И.И.
//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//            перенесено с правкой : 06.12.2020 Потапов И.И. 
//            создание родителя : 21.02.2022 Потапов И.И. 
//---------------------------------------------------------------------------
//               Реверс в NPRiverLib 09.02.2024 Потапов И.И.
//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 09.02.2024 Потапов И.И.
//---------------------------------------------------------------------------
//                      Сбока No Property River Lib
//              с убранными из наследников Property классами 
//---------------------------------------------------------------------------
namespace NPRiverLib.APRiver_1YD
{
    using MemLogLib;
    using AlgebraLib;
    using GeometryLib;

    using NPRiverLib.IO;

    using CommonLib;
    using CommonLib.IO;
    using CommonLib.Points;

    using System;
    using System.Linq;

    using FEMTasksLib.FESimpleTask;
    using NPRiverLib.APRiver_1YD.Params;
    using MeshGeneratorsLib.StripGenerator;
    using MeshLib.CArea;
    using CommonLib.Delegate;
    using CommonLib.Function;
    using MeshLib;
    using CommonLib.Physics;

    /// <summary>
    ///  ОО: Определение класса TriSroSecRiverTask_1YD - расчет полей скорости, вязкости 
    ///       и напряжений в живом сечении потока методом КЭ (симплекс элементы)
    /// </summary>
    [Serializable]
    public class TriRoseRiverTask_1YD : APRiverFEM_1YD
    {

        #region Локальные поля и методы задачи Розовского
        /// <summary>
        /// Параметр релаксации для заадчи вихрь - функция тока
        /// </summary>
        double w = 0.3;
        /// <summary>
        /// Средний радиус канала
        /// </summary>
        double R_midle = 5;
        /// <summary>
        /// Флаг осесимментричности
        /// </summary>
        int Ring = 1;
        /// <summary>
        /// Количество узлов по дну
        /// </summary>
        int NN = 100;
        /// <summary>
        /// Индекс створа
        /// </summary>
        int CrossNamberIndex = 0;
        bool BoundaryG2_UxChecked = true;
        bool smoothingChecked = true;
        int[] bKnotsU = null;
        int[] bKnotsV = null;
        double[] mBC_V = null;
        double[] mBC_U = null;
        double[] mbU = null;
        double[] mbV = null;
        uint[] mAdressU = null;
        double[] YRose2 = { 0, 0.11965, 0.405625, 0.634625, 0.804625, 0.974625, 1.203625, 1.489625, 1.60925 };
        double[] ZRose2 = { 0.1401, 0.1, 0.028625, 0, 0, 0, 0.028625, 0.1, 0.1401 };
        double WL = 0.140;

        IDigFunction VelosityUx = null;
        IDigFunction VelosityUy = null;
        /// <summary>
        /// Вычисление скорости Ux
        /// </summary>
        Function<double, double> CalkU = null;

        #endregion

        RiverCrossVortexPhiTri taskVortexPhi = null;
        /// <summary>
        /// Конструктор
        /// </summary>
        public TriRoseRiverTask_1YD() : this(new RSCrossParams()) { }
        /// <summary>
        /// Конструктор
        /// </summary>
        public TriRoseRiverTask_1YD(RSCrossParams p) : base(p)
        {
            Version = "TriSroSecRiverTask 21.02.2022";
            name = "Поток в створе канала Розовского";
        }
        /// <summary>
        /// Расчет касательных напряжений на дне канала
        /// </summary>
        /// <param name="mesh">сетка</param>
        /// <param name="TauZ">напражения на сетке</param>
        /// <param name="TauY">напражения на сетке</param>
        /// <param name="xv">координаты Х узлов КО</param>
        /// <param name="yv">координаты У узлов КО</param>
        protected override double[] TausToVols(in double[] xv, in double[] yv)
        {
            // расчет напряжений Txy  Txz
            taskU.SolveTaus(ref TauY, ref TauZ, U, eddyViscosity);
            // граничные узлы на нижней границе области
            uint[] bounds = mesh.GetBoundKnotsByMarker(0);
            // количество узлов на нижней границе области
            TSpline tauSplineZ = new TSpline();
            TSpline tauSplineY = new TSpline();
            MEM.Alloc(bounds.Length, ref tauY);
            MEM.Alloc(bounds.Length, ref tauZ);
            MEM.Alloc(bounds.Length, ref Coord);
            // пробегаем по граничным узлам и записываем для них Ty, Tz T
            double[] xx = mesh.GetCoords(0);
            for (int i = 0; i < bounds.Length - 1; i++)
            {
                tauZ[i] = 0.5 * (TauZ[bounds[i]] + TauZ[bounds[i + 1]]);
                tauY[i] = 0.5 * (TauY[bounds[i]] + TauY[bounds[i + 1]]);
                Coord[i] = 0.5 * (xx[bounds[i]] + xx[bounds[i + 1]]);
            }
            double left = xx[bounds[0]];
            double right = xx[bounds[bounds.Length - 1]];
            // формируем сплайны напряжений в натуральной координате
            tauSplineZ.Set(tauZ, Coord, (uint)bounds.Length);
            tauSplineY.Set(tauY, Coord, (uint)bounds.Length);
            // массив касательных напряжений
            MEM.Alloc(xv.Length - 1, ref tau);
            for (int i = 0; i < tau.Length; i++)
            {
                double xtau = 0.5 * (xv[i] + xv[i + 1]);
                if (xtau < left || right < xtau)
                    tau[i] = 0;
                else
                {
                    double L = HPoint.Length(xv[i + 1], yv[i + 1], xv[i], yv[i]);
                    double CosG = (xv[i + 1] - xv[i]) / L;
                    double SinG = (yv[i + 1] - yv[i]) / L;
                    tau[i] = tauSplineZ.Value(xtau) * CosG +
                             tauSplineY.Value(xtau) * SinG;
                    if (double.IsNaN(tau[i]) == true)
                        throw new Exception("Mesh for RiverStreamTask");
                }
            }
            // Сдвиговые напряжения максимум
            tauMax = tau.Max();
            /// Сдвиговые напряжения средние
            tauMid = tau.Sum() / tau.Length;
            return tau;
        }
        #region Искомые поля
        /// <summary>
        /// Поле скорости
        /// </summary>
        public double[] Uy;
        /// <summary>
        /// Правая часть
        /// </summary>
        public double[] mQ = null;
        /// <summary>
        /// Поле скорости
        /// </summary>
        public double[] Uz;
        /// <summary>
        /// Поле напряжений T_zz
        /// </summary>
        public double[] TauZZ;
        /// <summary>
        /// Поле напряжений T_yy
        /// </summary>
        public double[] TauYY;
        /// <summary>
        /// Поле напряжений T_yz
        /// </summary>
        public double[] TauYZ;
        /// <summary>
        /// Поле вихря
        /// </summary>
        public double[] Vortex;
        /// <summary>
        /// Поле функции тока
        /// </summary>
        public double[] Phi;

        #endregion

        #region Локальные методы задачи Розовского
        /// <summary>
        /// Скорость в створе 15
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        double CalkU15(double y)
        {
            double V =
                    -0.4823745080e-2 * Math.Exp(2.230789734 * y)
                   - 0.1668512549 * Math.Exp(-3.659361162 * y) + 0.1716750000;
            return 1.096569401 * Math.Sqrt(Math.Max(0, V));
        }
        /// <summary>
        /// Скорость в створе 15
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        double CalkU18(double y)
        {
            double V = -0.7744248317e-2 * Math.Exp(1.513732350 * y) -
                0.1639307517 * Math.Exp(-.4148312511 * y) + 0.1716750000;
            return 2.457935972 * Math.Sqrt(Math.Max(0, V));
        }
        /// <summary>
        /// Скорость в створе 15
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        double CalkU26(double y)
        {
            double V = -0.3580698312e-5 * Math.Exp(5.606508095 * y)
                - .1716714194 * Math.Exp(-.1120026006 * y) + .1716750000;
            return 3.285684719 * Math.Sqrt(Math.Max(0, V));
        }

        #endregion

        /// <summary>
        /// Создать расчетную область
        /// </summary>
        /// <param name="right"></param>
        /// <param name="left"></param>
        protected override void CreateCalculationDomain()
        {
            // генерация сетки
            switch (CrossNamberIndex)
            {
                case 0: // Створ 15
                    {
                        //double[] U15 = { 0, 0.28, 0.375, 0.38, 0.38, 0.373, 0.355, 0.2, 0 };
                        // нормированные к расходу скорости
                        double[] U15 = { 0, 0.2577, 0.381, 0.391, 0.391, 0.3845, 0.366, 0.206, 0 };
                        double[] V15 = { 0, 0.004, 0.02, 0.028, 0.028, 0.027, 0.031, 0.01, 0 };
                        VelosityUx = new DigFunction(YRose2, U15, "Створ");
                        VelosityUy = new DigFunction(YRose2, V15, "Створ");
                        CalkU = CalkU15;
                    }
                    break;
                case 1: // Створ 18
                    {
                        //double[] U18 = { 0, 0.20, 0.33, 0.38, 0.40, 0.40, 0.38, 0.26, 0 };
                        // нормированные к расходу скорости
                        double[] U18 = { 0, 0.2039, 0.336, 0.387, 0.4078, 0.4078, 0.387, 0.2651, 0 };
                        double[] V18 = { 0, 0.005, 0.03, 0.039, 0.041, 0.04, 0.035, 0.015, 0 };
                        VelosityUx = new DigFunction(YRose2, U18, "Створ");
                        VelosityUy = new DigFunction(YRose2, V18, "Створ");
                        CalkU = CalkU18;
                    }
                    break;
                case 2: // Створ 21
                    {
                        //double[] U21 = { 0, 0.16, 0.31, 0.38, 0.40, 0.42, 0.44, 0.37, 0 };
                        // нормированные к расходу скорости
                        double[] U21 = { 0, 0.1563, 0.303, 0.371, 0.391, 0.410, 0.43, 0.361, 0 };
                        double[] V21 = { 0, 0.005, 0.013, 0.02, 0.021, 0.02, 0.015, 0.01, 0 };
                        VelosityUx = new DigFunction(YRose2, U21, "Створ");
                        VelosityUy = new DigFunction(YRose2, V21, "Створ");
                        CalkU = CalkU26;
                    }
                    break;
                case 3: // Створ 26
                    {
                        // double[] U26 = { 0, 0.15, 0.29, 0.35, 0.38, 0.40, 0.43, 0.33, 0 };
                        // нормированные к расходу скорости
                        double[] U26 = { 0, 0.1539, 0.2977, 0.359, 0.39, 0.41, 0.441, 0.3387, 0 };
                        double[] V26 = { 0, 0.003, 0.011, 0.015, 0.018, 0.016, 0.013, 0.01, 0 };
                        VelosityUx = new DigFunction(YRose2, U26, "Створ");
                        VelosityUy = new DigFunction(YRose2, V26, "Створ");
                        CalkU = CalkU26;
                    }
                    break;
                case 4: // Тест
                    {
                        // double[] U26 = { 0, 0.15, 0.29, 0.35, 0.38, 0.40, 0.43, 0.33, 0 };
                        // нормированные к расходу скорости
                        double[] U26 = { 0, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0 };
                        double[] V26 = { 0, 0.05, 0.05, 0.05, 0.05, 0.05, 0.05, 0.05, 0 };
                        VelosityUx = new DigFunction(YRose2, U26, "Створ");
                        VelosityUy = new DigFunction(YRose2, V26, "Створ");
                    }
                    break;
            }

            IDigFunction Geometry = new DigFunction(YRose2, ZRose2, "Створ");
            
            // получение отметок дна
            
            Geometry.GetFunctionData(ref bottom_x, ref bottom_y, NN);

            meshGenerator = new HStripMeshGeneratorTri();

            mesh = meshGenerator.CreateMesh(ref WetBed, waterLevel, bottom_x, bottom_y);

            wMesh = new MeshWrapperСhannelSectionCFGTri(mesh, Geometry, waterLevel, R_midle, Ring, false);
            
            right = meshGenerator.Right();
            left = meshGenerator.Left();
            // получение ширины ленты для алгебры
            int WidthMatrix = (int)mesh.GetWidthMatrix();
            // TO DO подумать о реклонировании algebra если размер матрицы не поменялся 
            algebra = new AlgebraGaussTape((uint)mesh.CountKnots, (uint)WidthMatrix);

            MEM.Alloc(mesh.CountKnots, ref eddyViscosity, SPhysics.mu );
            MEM.Alloc(mesh.CountKnots, ref eddyViscosity0, SPhysics.mu);
            MEM.Alloc(mesh.CountKnots, ref U, "U");
            MEM.Alloc(mesh.CountKnots, ref Uy, "Uy");
            MEM.Alloc(mesh.CountKnots, ref Uz, "Uz");
            // память под напряжения в области
            MEM.Alloc(mesh.CountKnots, ref TauY, "TauY");
            MEM.Alloc(mesh.CountKnots, ref TauZ, "TauZ");
            
            MEM.Alloc(mesh.CountKnots, ref TauYY, "TauYY");
            MEM.Alloc(mesh.CountKnots, ref TauZZ, "TauZZ");
            MEM.Alloc(mesh.CountKnots, ref TauYZ, "TauYZ");
            
            MEM.Alloc(mesh.CountKnots, ref Vortex, "Vortex");
            MEM.Alloc(mesh.CountKnots, ref Phi, "Phi");

            MEM.Alloc(Params.CountKnots, ref tau, "tau");

            MEM.Alloc(mesh.CountKnots, ref mQ, Q);

            unknowns.Clear();
            unknowns.Add(new Unknown("Скорость Ux", U, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new Unknown("Поле вихря Vortex", Vortex, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new Unknown("Поле тока Phi", Phi, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new Unknown("Турб. вязкость", eddyViscosity, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new Unknown("Скорость Uy", Uy, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new Unknown("Скорость Uz", Uz, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new CalkPapams("Поле скорость", Uy, Uz, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new CalkPapams("Поле напряжений T_xy", TauY, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new CalkPapams("Поле напряжений T_xz", TauZ, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new CalkPapams("Поле напряжений T_yy", TauYY, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new CalkPapams("Поле напряжений T_zz", TauZZ, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new CalkPapams("Поле напряжений T_yz", TauYZ, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new CalkPapams("Поле напряжений", TauY, TauZ, TypeFunForm.Form_2D_Rectangle_L1));


            taskVortexPhi = new RiverCrossVortexPhiTri(wMesh, algebra, TypeTask.streamY1D, w);

            if (taskU == null)
                taskU = new FEPoissonTaskTri(mesh, algebra);
            else
                taskU.SetTask(mesh, algebra);


            MEM.Alloc(mesh.CountKnots, ref bKnotsU, -1);
            MEM.Alloc(mesh.CountKnots, ref bKnotsV, -1);
            MEM.Alloc(mesh.CountKnots, ref mBC_U, 0);
            MEM.Alloc(mesh.CountKnots, ref mBC_V, 0);

            BC(BoundaryG2_UxChecked, smoothingChecked,
                          ref bKnotsU, ref bKnotsV,
                          ref mBC_U, ref mBC_V,
                          ref mbU, ref mbV);
        }

        protected void BC(bool BoundaryG2_UxChecked, bool smoothingChecked,
                          ref int[] bKnotsU, ref int[] bKnotsV, 
                          ref double[] mBC_U, ref double[] mBC_V,
                          ref double[] mbU, ref double[] mbV)
        {
            

            double[] X = mesh.GetCoords(0);
            TwoElement[] belems = mesh.GetBoundElems();
            int[] bMarks = mesh.GetBElementsBCMark();

            for (uint be = 0; be < bMarks.Length; be++)
            {
                int Mark = bMarks[be];
                uint idx1 = belems[be].Vertex1;
                double x1 = X[idx1];
                uint idx2 = belems[be].Vertex2;
                double x2 = X[idx2];
                if (Mark == 0) // дно канала
                {
                    bKnotsU[idx1] = 1;
                    mBC_U[idx1] = 0;
                    bKnotsU[idx2] = 1;
                    mBC_U[idx2] = 0;
                }
                if (Mark == 2) // свободная поверхность
                {
                    if (BoundaryG2_UxChecked == true)
                    {
                        //double U1 = VelosityUx.FunctionValue(x1);
                        //double U2 = VelosityUx.FunctionValue(x2);
                        //Console.WriteLine(" U1 {0:F4}  U2 {0:F4} i1 {2}  i2 {3}", U1, U2, idx1, idx2);
                        bKnotsU[idx1] = 1;
                        bKnotsU[idx2] = 1;
                        if (smoothingChecked == true && CalkU != null)
                        {
                            mBC_U[idx1] = CalkU(x1);
                            mBC_U[idx2] = CalkU(x2);
                        }
                        else
                        {
                            mBC_U[idx1] = VelosityUx.FunctionValue(x1);
                            mBC_U[idx2] = VelosityUx.FunctionValue(x2);
                        }
                    }
                    bKnotsV[idx1] = 1;
                    mBC_V[idx1] = VelosityUy.FunctionValue(x1);
                    bKnotsV[idx2] = 1;
                    mBC_V[idx2] = VelosityUy.FunctionValue(x2);
                }
            }
            
            int CountU = bKnotsU.Sum(xx => xx > 0 ? xx : 0);
            int CountV = bKnotsV.Sum(xx => xx > 0 ? xx : 0);
            MEM.Alloc(CountU, ref mbU, 0);
            MEM.Alloc(CountU, ref mAdressU);
            MEM.Alloc(CountV, ref mbV, 0);
            int k = 0;
            for (uint i = 0; i < mBC_U.Length; i++)
            {
                if (bKnotsU[i] == 1)
                {
                    mAdressU[k] = i;
                    mbU[k++] = mBC_U[i];
                }
            }
            k = 0;
            for (int i = 0; i < mBC_V.Length; i++)
            {
                if (bKnotsV[i] == 1)
                    mbV[k++] = mBC_V[i];
            }
        }

        protected override void SolveVelosity()
        {
            bool flagUx = true;
            int VortexBC_G2 = 2;

            SPhysics.PHYS.turbViscType = TurbViscType.Leo_C_van_Rijn1984_C;
            taskVortexPhi.CalkVortexStream(
            // Искомые поля
            ref Phi, ref Vortex, ref U, ref Uy, ref Uz, ref eddyViscosity,
            ref TauY, ref TauZ, ref TauYY, ref TauYZ, ref TauZZ,
            // Граничные условия для потоковой скорости и боковой скорости на свободной поверхности
            mbU, mAdressU, mbV,
            mQ, R_midle, Ring,
            VortexBC_G2, flagUx, VortexBC_G2);
        }

        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public override IRiver Clone()
        {
            return new TriRoseRiverTask_1YD(Params);
        }
        /// <summary>
        /// Создает экземпляр класса конвертера
        /// </summary>
        /// <returns></returns>
        public override IOFormater<IRiver> GetFormater()
        {
            return new TaskReader1YD_RvY();
        }
    }
}
