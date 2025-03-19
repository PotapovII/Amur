//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//---------------------------------------------------------------------------
//                 кодировка : 09.03.2025 Потапов И.И.
//---------------------------------------------------------------------------
namespace NPRiverLib.APRiver_1XD
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;

    using CommonLib;
    using CommonLib.IO;
    using CommonLib.Mesh;
    using CommonLib.Physics;
    using CommonLib.Geometry;
    using CommonLib.Function;
    using CommonLib.EddyViscosity;
    using CommonLib.ChannelProcess;

    using MeshGeneratorsLib;
    using MeshGeneratorsLib.IO;
    using MeshGeneratorsLib.Renumberation;

    using MemLogLib;
    using AlgebraLib;
    using GeometryLib;

    using NPRiverLib.IO;
    using NPRiverLib.APRiver_1XD.River2D_FVM_ke;

    using EddyViscosityLib;

    using FEMTasksLib.FEMTasks.VortexStream;
    using MeshLib;
    using MeshAdapterLib;
    using System.Reflection;
    using RenderLib;
    using System.Windows.Forms;
    using MeshGeneratorsLib.SPIN;


    /// <summary>
    ///  ОО: Определение класса TriFEMRiver_1XD - расчет полей скорости, вязкости 
    ///       и напряжений в живом сечении потока методом КЭ (симплекс элементы)
    /// </summary>
    [Serializable]
    public class TriFEMRiver_1XD : APRiverFEM1XD
    {
        /// <summary>
        /// индекс задачи по умолчанию
        /// </summary>
        uint testTaskID = 0;
        /// <summary>
        /// Максимальное количество итераций по нелинейности на текущем шаге по времени
        /// </summary>
        protected int NoLineMax = 20;
        /// <summary>
        /// Алгебра для КЭ векторной задачи
        /// </summary>
        [NonSerialized]
        protected IAlgebra algebra2 = null;
        /// <summary>
        /// тип задачи 0 - плоская 1 - цилиндрическая
        /// </summary>
        protected int SigmaTask;
        /// <summary>
        /// радиус изгиба русла
        /// </summary>
        protected double RadiusMin;
        /// <summary>
        /// Функция тока
        /// </summary>
        public double[] Phi;
        /// <summary>
        /// Функция вихря
        /// </summary>
        public double[] Vortex;
        /// <summary>
        /// Вязкость на предыдущем слое по времени
        /// </summary>
        protected double[] eddyViscosity0;
        /// <summary>
        /// узлы на контуре
        /// </summary>
        protected uint[] bknotsUx = null;
        /// <summary>
        /// Нормальная скорость на WL
        /// </summary>
        protected double[] bUx = null;
        /// <summary>
        /// Задача для расчета вторичных скоростей в створе
        /// </summary>
        protected ReynoldsVortexStream2XDTri taskPV;
        /// <summary>
        /// Задача для расчета турбулентной вязкости потока
        /// </summary>
        protected IEddyViscosityTri taskViscosity;
        /// <summary>
        /// Конструктор
        /// </summary>
        public TriFEMRiver_1XD() : this(new FEMParams_1XD()) { }
        /// <summary>
        /// Конструктор
        /// </summary>
        public TriFEMRiver_1XD(FEMParams_1XD p) : base(p)
        {
            Version = "TriFEMRiver_1XD 09.09.2025";
            name = "Поток в канале (КЭ)";
        }
        /// <summary>
        /// Расчет полей глубины и скоростей на текущем шаге по времени
        /// </summary>
        public override void SolverStep()
        {
            int flagErr = 0;
            try
            {
                // расчет уровня свободной поверхности реки
                flagErr=1;
                // определение расчетной области потока и построение КЭ сетки
                if (Erosion != EBedErosion.NoBedErosion)
                    UpDateDomain();
                if (eTaskReady == ETaskStatus.TaskReady)
                {
                    flagErr=2;
                    // расчет вторичных потоков в створе
                    switch (Params.ReTask)
                    {
                        case 0:
                            taskPV.SolveReynoldsTask(ref Phi, ref Vortex, eddyViscosity, dtime);
                            break;
                        case 1:
                            taskPV.SolveReynoldsTask(ref Phi, ref Vortex, eddyViscosity);
                            break;
                        case 2:
                            taskPV.SolveStokesTask(ref Phi, ref Vortex, eddyViscosity);
                            break;
                    }
                    FlagStartMu = true;
                    flagErr=3;
                    // расчет  придонных касательных напряжений на дне
                    tau = TausToVols(in bottom_x, in bottom_y);
                    flagErr=4;
                    MEM.Copy(ref eddyViscosity0, eddyViscosity);
                    flagErr=5;
                    // вычисление вязкости потока
                    taskViscosity.SolveTask(ref eddyViscosity, null, taskPV.Vy, taskPV.Vz, Phi, Vortex, dtime);
                    flagErr = 6;
                    MEM.Relax(ref eddyViscosity, eddyViscosity0);
                    time += dtime;
                }
            }
            catch (Exception ex)
            {
                string ErrorName = RiverError.ErrorName(flagErr);
                Logger.Instance.Error(ErrorName, "ASectionalRiverTask");
                Logger.Instance.Exception(ex);
            }
        }
        /// <summary>
        /// Создать расчетную область
        /// </summary>
        /// <param name="right"></param>
        /// <param name="left"></param>
        public override void DefaultCalculationDomain(uint testTaskID = 0)
        {
            // генерация сетки
            MeshCreateor(testTaskID);
            CreateCalculationDomain();
        }

        /// <summary>
        /// Формирование данных для отрисовки связанных с сеткой IMesh со сороны задачи
        /// </summary>
        /// <param name="sp">контейнер данных</param>
        public override void AddMeshPolesForGraphics(ISavePoint sp)
        {
            base.AddMeshPolesForGraphics(sp);
            var vm = taskViscosity as AEddyViscosityKETri;
            if (vm != null)
            {
                sp.Add("Кин. энергия турб.", vm.Ken);
                sp.Add("Диссипация турб. эн.", vm.Eps);
            }
            var vm1 = taskViscosity as AEddyViscosity_SA_Tri;
            if (vm1 != null)
            {
                sp.Add("турб. вязкость прив.", vm1.Mut);
                sp.Add("xi", vm1.xi);
                sp.Add("fv1", vm1.fv1);
                sp.Add("fv2", vm1.fv2);
                sp.Add("ft2", vm1.ft2);
                sp.Add("mQ_mut", vm1.mQ_mut);
                
                sp.Add("дистанция.", vm1.distance);
            }
            var vm2 = taskViscosity as AEddyViscosityDistance;
            if (vm2 != null)
            {
                sp.Add("дистанция.", vm2.Distance);
                sp.Add("Глубина приведенная.", vm2.Hp);
            }
        }
        /// <summary>
        /// генерация сетки
        /// </summary>
        protected void MeshCreateor(uint testTaskID)
        {
            // создание и чтение свойств задачи                
            FEMParams_1XD p = new FEMParams_1XD()
            {
                // Количество КЭ для давления по Х
                FE_X = 80,
                // Количество КЭ для давления по Х
                FE_Y = 30,
                // Тип формы дна
                typeBedForm = TypeBedForm.PlaneForm,
                // Амплитуда донной поверхности
                bottomWaveAmplitude = 0,
                // Количество донных волн
                wavePeriod = 1,
                // Количество инераций по движению узлов границы
                CountBoundaryMove = 1,
                // Длина водотока на 1 участке (вход потока)
                Len1 = 0.5,
                // Длина водотока на 3 участке (центр)
                Len2 = 0.5,
                // Длина водотока на 3 участке (истечение)
                Len3 = 0.5,
                // Глубина водотока 1 придонный участок
                Wen1 = 1.0,
                // Глубина 2 участка
                Wen2 = 3.0,
                // Глубина 3 участка
                Wen3 = 1.0,
                // Расчет концентрации вместо температуры true == да
                TemperOrConcentration = false,
                // концентрация  в 1 слое
                t1 = 0,
                // концентрация  в 2 слое
                t2 = 0.1,
                // концентрация  в 3 слое
                t3 = 0,
                // Скорость в 1 придонном слое
                V1_inlet = 0,
                // Скорость в 2 придонном слое
                V2_inlet = 1,
                // Скорость в 3 придонном слое
                V3_inlet = 0,
                // Граничные условия для скоростей на верхней границе области
                bcIndex = RoofCondition.slip,
                // типы задачи по входной струе
                typeStreamTask = TypeStreamTask.StreamFromShield,
                // 
                typeMAlgebra = TypeMAlgebra.CGD_Algorithm,
                // Растояние струи от стенки
                LV = 0,
                // Смешение струи от стенки
                shiftV = false,
                // Максимальное количество итераций по нелинейности
                NonLinearIterations = 10,
                // Число Прандтля для уравнения теплопроводности
                TaskIndex = 0,
                // Деформаяй дна от x = 0 (Да) со второго участуа (Нет)
                bedLoadStart_X0 = false,
                // Деформаяй дна только от положительных напряжений (Да)
                // с учетом зон рецеркуляции (Нет)"
                bedLoadTauPlus = true,
                // Струя сформирована в области (Да) только на входе (Нет)
                streamInsBoundary = true,
                // Подсос на границе втекания (Да) только через сопла (Нет)
                velocityInsBoundary = true,
                // Движение узлов сетки по горизонтальным границам
                topBottom = true,
                // Движение узлов сетки по вертикальным границам
                leftRight = false,
                // Фильтрация на обвал дна
                localFilterBLMesh = true,
                // Расчет напряжений на всем дне
                AllBedForce = true,
                // Тип гидродинамики 0 - нестационарные 1 - стационарные у-я Рейнольдса 2 - Стокс
                ReTask = 2,
                // Количество итераций по нелинейности на текущем шаге по времени
                NLine = 10,
                // Параметр неявности схемы при шаге по времениПараметр неявности схемы при шаге по времени
                theta = 0.5,
                // Постоянная вихревая вязкость
                mu_const = 4,
                // модель турбулентной вязкости
                turbViscType = ETurbViscType.EddyViscosityConst
            };
            switch (testTaskID)
            {
                case 1:
                case 2:
                case 3:
                    try
                    {
                        if (testTaskID == 2) // стационарные у-я Рейнольдса 
                            p.ReTask = 1;
                        if (testTaskID == 3) // нестационарные у - я Рейнольдса
                            p.ReTask = 0;
                        SetParams(p);

                        TriMesh triMesh = null;
                        CreateMesh.GetRectangleTriMesh(ref triMesh, Params.FE_X, Params.FE_Y, p.Lx, p.Ly, 1);
                        mesh = triMesh;

                        BCVelosity = new DigFunctionPolynom[2];
                        // скорость на входе
                        BCVelosity[0] = new DigFunctionPolynom(0, 0, p.Ly / 2, p.V2_inlet, p.Ly, 0);
                        // скорость на выходе
                        BCVelosity[1] = new DigFunctionPolynom(0, 0, p.Ly / 2, p.V2_inlet, p.Ly, 0);

                        double y0 = 0,              Phi_0   = 0;
                        double y1 = p.Ly / 3,       Phi_H3  = 14.0 / 81 * p.V2_inlet * p.Ly;
                        double y2 = 2 * p.Ly / 3,   Phi_2H3 = 40.0 / 81 * p.V2_inlet * p.Ly;
                        double y3 = p.Ly,           Phi_H   = 2.0 / 3.0 * p.V2_inlet * p.Ly;
                        bcPhi = new DigFunctionPolynom[2];
                        // функция тока на входе
                        bcPhi[0] = new DigFunctionPolynom(y0, Phi_0, y1, Phi_H3, y2, Phi_2H3, y3, Phi_H);
                        // функция тока на выходе
                        bcPhi[1] = new DigFunctionPolynom(y0, Phi_0, y1, Phi_H3, y2, Phi_2H3, y3, Phi_H);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    break;
                case 4:
                case 5:
                case 6:
                    try
                    {
                        if (testTaskID == 5) // стационарные у-я Рейнольдса 
                            p.ReTask = 1;
                        if (testTaskID == 6) // нестационарные у - я Рейнольдса
                            p.ReTask = 0;
                        p.Len1 = 1;
                        p.Len2 = 1;
                        p.Wen1 = 4;
                        p.Wen2 = 36;
                        p.V2_inlet = 1;
                        SetParams(p);
                        double H1 = p.Len1;
                        double H2 = p.Len2;
                        double H = H1 + H2;
                        double L1 = p.Wen1;
                        double L2 = p.Wen2;
                        double L = L1 + L2;
                        double U0 = p.V2_inlet;
                        double U1 = U0*H2/H;

                        bcPhi = new DigFunctionPolynom[2];

                        double y0 = H1,                 Phi_0 = 0;
                        double y1 = H1 + H2 / 3,        Phi_H3 = 14.0 / 81 * U0 * H2;
                        double y2 = H1 + 2 * H2 / 3,    Phi_2H3 = 40.0 / 81 * U0 * H2;
                        double y3 = H,                  Phi_H = 2.0 / 3.0 * U0 * H2;
                        // функция тока на входе
                        bcPhi[0] = new DigFunctionPolynom(y0, Phi_0, y1, Phi_H3, y2, Phi_2H3, y3, Phi_H);

                        y0 = 0;             Phi_0 = 0;
                        y1 = H / 3;         Phi_H3 = 14.0 / 81 * U1 * H;
                        y2 = 2.0 * H / 3;   Phi_2H3 = 40.0 / 81 * U1 * H;
                        y3 = H;             Phi_H = 2.0 / 3.0 * U1 * H;

                        // функция тока на выходе
                        bcPhi[1] = new DigFunctionPolynom(y0, Phi_0, y1, Phi_H3, y2, Phi_2H3, y3, Phi_H);

                        StepGenerator sg = new StepGenerator();
                        //int Nx1 = 60;
                        //int Nx2 = 360;
                        //int Ny1 = 60;
                        //int Ny2 = 60;
                        int Nx1 = 20;
                        int Nx2 = 120;
                        int Ny1 = 20;
                        int Ny2 = 20;
                        sg.Set(Nx1, Nx2, Ny1, Ny2, H1, H2, L1, L2);
                        //ComplecsMesh cmesh = null;
                        TriMesh cmesh = null; 
                        sg.GetMesh(ref cmesh);
                        mesh = cmesh;

                        //double diametrFE = 0.2;
                        //// meshData
                        //TypeMesh[] meshTypes = { TypeMesh.MixMesh, TypeMesh.Triangle, TypeMesh.Rectangle };
                        //TypeMesh meshType = meshTypes[1];
                        //TypeRangeMesh MeshRange = (TypeRangeMesh)(1);
                        //int meshMethod = 0;
                        //int reNumberation = 1;
                        //double RelaxMeshOrthogonality = 0.2;
                        //int CountParams = 0;
                        //bool flagMidle = true;
                        //Direction direction = 0;
                        //IFERenumberator renumberator = ARenumberator.GetRenumberator(0);
                        //// Данные для сетки
                        //HMeshParams meshData = new HMeshParams(meshType, MeshRange, meshMethod,
                        //          new HPoint(diametrFE, diametrFE), RelaxMeshOrthogonality, reNumberation,
                        //          direction, CountParams, flagMidle);


                        //double[] param = { 1 };
                        ////
                        ////  4-------------(3)-------------3      ---  ---
                        ////  |                             |       |    |
                        ////  |                             |       |    |
                        //// (4)             [1]           (2)      H2   |
                        ////  |                             |       |    H
                        ////  |                             |       |    |     
                        ////  0-------(0)---------1---(1)---2      ---   |
                        ////                      |         |       |    |
                        ////  ^ Y                (5)   [2] (7)      H1   |
                        ////  |                   |         |       |    |
                        ////  0===> X             5---(6)---6      ---  --- 
                        ////
                        ////  |----------L1-------|----L2---|
                        ////  |---------------L-------------|
                        //VMapKnot p0 = new VMapKnot(0, H1, param);
                        //VMapKnot p1 = new VMapKnot(L1, H1, param);
                        //VMapKnot p2 = new VMapKnot(L, H1, param);
                        //VMapKnot p3 = new VMapKnot(L, H, param);
                        //VMapKnot p4 = new VMapKnot(0, H, param);
                        //VMapKnot p5 = new VMapKnot(L1, 0, param);
                        //VMapKnot p6 = new VMapKnot(L, 0, param);
                        //// количество параметров на границе (задан 1)
                        //List<VMapKnot> nods0 = new List<VMapKnot>() { new VMapKnot(p0), new VMapKnot(p1) };
                        //List<VMapKnot> nods1 = new List<VMapKnot>() { new VMapKnot(p1), new VMapKnot(p2) };
                        //List<VMapKnot> nods2 = new List<VMapKnot>() { new VMapKnot(p2), new VMapKnot(p3) };
                        //List<VMapKnot> nods3 = new List<VMapKnot>() { new VMapKnot(p3), new VMapKnot(p4) };
                        //List<VMapKnot> nods4 = new List<VMapKnot>() { new VMapKnot(p4), new VMapKnot(p0) };
                        //List<VMapKnot> nods5 = new List<VMapKnot>() { new VMapKnot(p1), new VMapKnot(p5) };
                        //List<VMapKnot> nods6 = new List<VMapKnot>() { new VMapKnot(p5), new VMapKnot(p6) };
                        //List<VMapKnot> nods7 = new List<VMapKnot>() { new VMapKnot(p6), new VMapKnot(p2) };

                        //int ID = 0;
                        //int markBC0 = 0;
                        //int markBC1 = 1;
                        //int markBC2 = 2;
                        //int markBC3 = 3;
                        //int markBC4 = 4;

                        //HMapSegment seg0 = new HMapSegment(nods0, ID, markBC0);
                        //HMapSegment seg1 = new HMapSegment(nods1, 1, markBC4);
                        //HMapSegment seg2 = new HMapSegment(nods2, 2, markBC1);
                        //HMapSegment seg3 = new HMapSegment(nods3, 3, markBC2);
                        //HMapSegment seg4 = new HMapSegment(nods4, 4, markBC3);
                        //HMapSegment seg5 = new HMapSegment(nods5, 5, markBC0);
                        //HMapSegment seg6 = new HMapSegment(nods6, 6, markBC0);
                        //HMapSegment seg7 = new HMapSegment(nods7, 7, markBC1);

                        ////  4-------------(3)-------------3      ---  ---
                        ////  |                             |       |    |
                        ////  |                             |       |    |
                        //// (4)             [1]           (2)      H2   |
                        ////  |                             |       |    H
                        ////  |                             |       |    |     
                        ////  0-------(0)---------1---(1)---2      ---   |
                        ////                      |         |       |    |
                        ////  ^ Y                (5)   [2] (7)      H1   |
                        ////  |                   |         |       |    |
                        ////  0===> X             5---(6)---6      ---  --- 
                        ////
                        ////  |----------L1-------|----L2---|
                        ////  |---------------L-------------|
                        //HMapSubArea subArea0 = new HMapSubArea(0);
                        //HMapFacet[] facet = new HMapFacet[4];
                        //facet[0] = new HMapFacet(seg0);
                        //facet[0].Add(seg1);
                        //facet[1] = new HMapFacet(seg2);
                        //facet[2] = new HMapFacet(seg3);
                        //facet[3] = new HMapFacet(seg4);
                        //for (int i = 0; i < 4; i++)
                        //    subArea0.Add(facet[i]);
                        ////  4-------------(3)-------------3      ---  ---
                        ////  |                             |       |    |
                        ////  |                             |       |    |
                        //// (4)             [1]           (2)      H2   |
                        ////  |                             |       |    H
                        ////  |                             |       |    |     
                        ////  0-------(0)---------1---(1)---2      ---   |
                        ////                      |         |       |    |
                        ////  ^ Y                (5)   [2] (7)      H1   |
                        ////  |                   |         |       |    |
                        ////  0===> X             5---(6)---6      ---  --- 
                        ////
                        ////  |----------L1-------|----L2---|
                        ////  |---------------L-------------|
                        ////
                        //HMapSubArea subArea1 = new HMapSubArea(1);
                        //facet[0] = new HMapFacet(seg6);
                        //facet[1] = new HMapFacet(seg7);
                        //facet[2] = new HMapFacet(seg1);
                        //facet[3] = new HMapFacet(seg5);
                        //for (int i = 0; i < 4; i++)
                        //    subArea1.Add(facet[i]);

                        //IHTaskMap mapMesh = new HTaskMap("ustup");
                        //mapMesh.Add(subArea0);
                        //mapMesh.Add(subArea1);

                        ////FormatFileTaskMap ftmap = new FormatFileTaskMap();
                        ////ftmap.Write(mapMesh, mapMesh.Name + ".tmap");


                        //IMeshBuilder meshBuilder = null; // new DiffMeshBuilder(meshData.RelaxMeshOrthogonality);

                        //DirectorMeshGenerator mg = new DirectorMeshGenerator(meshBuilder, meshData, mapMesh, renumberator);
                        //// генерация КЭ сетки
                        //IMesh feMesh = mg.Create();

                        //mesh = feMesh;

                        ////SavePoint data = new SavePoint();
                        ////data.SetSavePoint(0, mesh);
                        ////double[] x = mesh.GetCoords(0);
                        ////double[] y = mesh.GetCoords(1);

                        ////data.Add("Координата Х", x);
                        ////data.Add("Координата Y", y);
                        ////data.Add("Координаты ХY", x, y);
                        ////GraphicsCurve curves = new GraphicsCurve();
                        ////for (int i = 0; i < x.Length; i++)
                        ////    curves.Add(x[i], y[i]);
                        ////GraphicsData gd = new GraphicsData();
                        ////gd.Add(curves);
                        ////data.SetGraphicsData(gd);
                        ////Form form = new ViForm(data);
                        ////form.Show();




                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                    break;
            }

        }
        /// <summary>
        /// Создать расчетную область
        /// </summary>
        /// <param name="right"></param>
        /// <param name="left"></param>
        protected override void CreateCalculationDomain()
        {

            MEM.Alloc(mesh.CountKnots, ref eddyViscosity, "eddyViscosity");
            MEM.Alloc(mesh.CountKnots, ref Phi, "Phi");
            MEM.Alloc(mesh.CountKnots, ref Vortex, "Vortex");
            // память под напряжения в области
            MEM.Alloc(mesh.CountKnots, ref TauY, "TauY");
            MEM.Alloc(mesh.CountKnots, ref TauZ, "TauZ");

            uint[] bedKnots = mesh.GetBoundKnotsByMarker(0);
            MEM.Alloc(bedKnots.Length, ref bottom_x, "TauY");
            MEM.Alloc(bedKnots.Length, ref bottom_y, "TauZ");

            double[] X = mesh.GetCoords(0);
            double[] Y = mesh.GetCoords(1);
            for (int i = 0; i < bedKnots.Length; i++)
            {
                bottom_x[i] = X[bedKnots[i]];
                bottom_y[i] = Y[bedKnots[i]];
            }
            // получение ширины ленты для алгебры
            int WidthMatrix = (int)mesh.GetWidthMatrix();
            // TO DO подумать о реклонировании algebra если размер матрицы не поменялся 
            algebra = new AlgebraLUTape((uint)mesh.CountKnots, WidthMatrix, WidthMatrix);
            algebra2 = new AlgebraLUTape((uint)(2 * mesh.CountKnots), 2 * WidthMatrix, 2 * WidthMatrix);

            //algebra = new SparseAlgebraCG(N, true);
            //algebra = new SparseAlgebraGMRES_P((uint)(mesh.CountKnots), 30, true);
            //algebra2 = new SparseAlgebraGMRES_P((uint)(2 * mesh.CountKnots), 30, true);

            // создание общего враппера сетки для задачи
            Set(mesh, algebra);
            if (taskPV == null)
            {
                taskPV = new ReynoldsVortexStream2XDTri(Params.NLine, BCVelosity, bcPhi, Params.theta);
                BEddyViscosityParam p = new BEddyViscosityParam(Params.NLine,0,0,0, 
                    SСhannelForms.boxCrossSection, ECalkDynamicSpeed.u_start_J, Params.mu_const);
                // вычисление начальной вихревой вязкости потока по алгебраической модели Leo_C_van_Rijn1984
                if (MEM.Equals(eddyViscosity.Sum(), 0) == true)
                {
                    taskViscosity = MuManager.Get(ETurbViscType.EddyViscosityConst, p, TypeTask.streamX1D);
                    taskViscosity.SetTask(mesh, algebra, wMesh);
                    taskViscosity.SolveTask(ref eddyViscosity, null, taskPV.Vy, taskPV.Vz, Phi, Vortex, dtime);
                }
                // вычисление начальной вихревой вязкости потока
                taskViscosity = MuManager.Get(Params.turbViscType, p, TypeTask.streamX1D);
            }
            if (taskViscosity.Cet_cs() == 1)
                taskViscosity.SetTask(mesh, algebra, wMesh);
            else
                taskViscosity.SetTask(mesh, algebra2, wMesh);
            taskPV.SetTask(mesh, algebra2, wMesh);

            unknowns.Clear();
            
            unknowns.Add(new Unknown("Скорость Vy", taskPV.Vy, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new Unknown("Скорость Vz", taskPV.Vz, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new Unknown("Скорость V", taskPV.Vy, taskPV.Vz, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new Unknown("Функция тока", taskPV.Phi, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new Unknown("Вихрь", taskPV.Vortex, TypeFunForm.Form_2D_Rectangle_L1));

            unknowns.Add(new CalkPapams("Турб. вязкость", eddyViscosity, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new CalkPapams("Поле напряжений T_xy", TauY, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new CalkPapams("Поле напряжений T_xz", TauZ, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new CalkPapams("Поле напряжений", TauY, TauZ, TypeFunForm.Form_2D_Rectangle_L1));

            if (eddyViscosity0 == null)
                MEM.Copy(ref eddyViscosity0, eddyViscosity);
            if (eddyViscosity0.Length != eddyViscosity.Length)
                MEM.Copy(ref eddyViscosity0, eddyViscosity);

            eTaskReady = ETaskStatus.TaskReady;
        }
        /// <summary>
        /// Обновить расчетную область
        /// </summary>
        /// <param name="right"></param>
        /// <param name="left"></param>
        protected void UpDateDomain()
        {
            // генерация сетки
            MeshCreateor(testTaskID); 
            // получение ширины ленты для алгебры
            int WidthMatrix = (int)mesh.GetWidthMatrix();
            // TO DO подумать о реклонировании algebra если размер матрицы не поменялся 
            algebra = new AlgebraLUTape((uint)mesh.CountKnots, WidthMatrix, WidthMatrix);
            algebra2 = new AlgebraLUTape((uint)(2 * mesh.CountKnots), 2 * WidthMatrix, 2 * WidthMatrix);
            // создание общего враппера сетки для задачи
            Set(mesh, algebra);

            if (taskViscosity.Cet_cs() == 1)
                taskViscosity.SetTask(mesh, algebra, wMesh);
            else
                taskViscosity.SetTask(mesh, algebra2, wMesh);
            taskPV.SetTask(mesh, algebra2, wMesh);

            if (mesh.CountKnots != Phi.Length)
            {
                MEM.Alloc(mesh.CountKnots, ref eddyViscosity, "eddyViscosity");
                BEddyViscosityParam p = new BEddyViscosityParam(Params.NLine, 0, 0, 0,
                SСhannelForms.boxCrossSection, ECalkDynamicSpeed.u_start_J, Params.mu_const);
                IEddyViscosityTri tv = MuManager.Get(ETurbViscType.Leo_C_van_Rijn1984, p, TypeTask.streamX1D);
                
                tv.SetTask(mesh, algebra, wMesh);
                tv.SolveTask(ref eddyViscosity, null, Phi, null, null, Vortex, dtime);

                MEM.Alloc(mesh.CountKnots, ref Phi, "Phi");
                MEM.Alloc(mesh.CountKnots, ref Vortex, "Vortex");
                // память под напряжения в области
                MEM.Alloc(mesh.CountKnots, ref TauY, "TauY");
                MEM.Alloc(mesh.CountKnots, ref TauZ, "TauZ");

                if (eddyViscosity0 == null)
                    MEM.Copy(ref eddyViscosity0, eddyViscosity);
                if (eddyViscosity0.Length != eddyViscosity.Length)
                    MEM.Copy(ref eddyViscosity0, eddyViscosity);

                unknowns.Clear();
                unknowns.Add(new Unknown("Скорость Vy", taskPV.Vy, TypeFunForm.Form_2D_Rectangle_L1));
                unknowns.Add(new Unknown("Скорость Vz", taskPV.Vz, TypeFunForm.Form_2D_Rectangle_L1));
                unknowns.Add(new Unknown("Скорость V", taskPV.Vy, taskPV.Vz, TypeFunForm.Form_2D_Rectangle_L1));
                unknowns.Add(new Unknown("Функция тока", taskPV.Phi, TypeFunForm.Form_2D_Rectangle_L1));
                unknowns.Add(new Unknown("Вихрь", taskPV.Vortex, TypeFunForm.Form_2D_Rectangle_L1));

                unknowns.Add(new CalkPapams("Турб. вязкость", eddyViscosity, TypeFunForm.Form_2D_Rectangle_L1));
                unknowns.Add(new CalkPapams("Поле напряжений T_xy", TauY, TypeFunForm.Form_2D_Rectangle_L1));
                unknowns.Add(new CalkPapams("Поле напряжений T_xz", TauZ, TypeFunForm.Form_2D_Rectangle_L1));
                unknowns.Add(new CalkPapams("Поле напряжений", TauY, TauZ, TypeFunForm.Form_2D_Rectangle_L1));
            }
            eTaskReady = ETaskStatus.TaskReady;
        }
        /// <summary>
        /// Загрузка геометрии области из файла
        /// </summary>
        /// <param name="file"></param>
        public override void LoadData(StreamReader file)
        {
            base.LoadData(file);
            CreateCalculationDomain();
            // готовность задачи
            eTaskReady = ETaskStatus.LoadAreaData;
        }
        /// <summary>
        /// Загрузка задачи из тестовых данных
        /// </summary>
        /// <param name="file">имя файла</param>
        public override void LoadData(IDigFunction[] BCVelosity)
        {
            base .LoadData(BCVelosity);
            CreateCalculationDomain();
        }
        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public override IRiver Clone()
        {
            return new TriFEMRiver_1XD(Params);
        }
        /// <summary>
        /// Создает экземпляр класса конвертера
        /// </summary>
        /// <returns></returns>
        public override IOFormater<IRiver> GetFormater()
        {
            return new TaskReaderFEM_1XD();
        }
    }
}
