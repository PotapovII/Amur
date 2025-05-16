//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//---------------------------------------------------------------------------
//                 кодировка : 02.02.2025 Потапов И.И.
//---------------------------------------------------------------------------
namespace NPRiverLib.APRiver1YD
{
    using System;
    using System.IO;
    using System.Linq;

    using MemLogLib;
    using AlgebraLib;
    using GeometryLib;

    using NPRiverLib.IO;
    using NPRiverLib.APRiver1YD.Params;

    using CommonLib;
    using CommonLib.IO;
    using CommonLib.Mesh;
    using CommonLib.Physics;
    using CommonLib.BedLoad;
    using CommonLib.Function;
    using CommonLib.EddyViscosity;
    using CommonLib.ChannelProcess;

    using EddyViscosityLib;
    using MeshGeneratorsLib.StripGenerator;
    using FEMTasksLib.FEMTasks.VortexStream;

    /// <summary>
    ///  ОО: Определение класса TriSroSecRiverTask1YD - расчет полей скорости, вязкости 
    ///       и напряжений в живом сечении потока методом КЭ (симплекс элементы)
    /// </summary>
    [Serializable]
    public class TriSecRiver_1YD : APRiverFEM1YD
    {
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
        /// Правая часть
        /// </summary>
        public double[] Q;
        /// <summary>
        /// узлы на контуре
        /// </summary>
        protected uint[] bknotsUx = null;
        /// <summary>
        /// Нормальная скорость на WL
        /// </summary>
        protected double[] bUx = null;
        /// <summary>
        /// Нормальная скорость на WL
        /// </summary>
        public IDigFunction VelosityUx = null;
        /// <summary>
        /// Радиальная скорость на WL
        /// </summary>
        public IDigFunction VelosityUy = null;

        /// <summary>
        /// Задача для расчета взвешенных наносов
        /// </summary>
        protected ReynoldsConcentrationTri taskCon;
        /// <summary>
        /// Задача для расчета нормальной скорости потока
        /// </summary>
        protected ReynoldsTransportTri taskUx;
        /// <summary>
        /// Задача для расчета вторичных скоростей в створе
        /// </summary>
        protected ReynoldsVortexStream1YDTri taskPV;
        /// <summary>
        /// Задача для расчета турбулентной вязкости потока
        /// </summary>
        protected IEddyViscosityTri taskViscosity;
        /// <summary>
        /// Интегральный расход влекомых наносов спроецированный на дно канала
        /// </summary>
        protected IDigFunction fGcon = null;
        /// <summary>
        /// Конструктор
        /// </summary>
        public TriSecRiver_1YD() : this(new RSCrossParams()) { }
        /// <summary>
        /// Конструктор
        /// </summary>
        public TriSecRiver_1YD(RSCrossParams p) : base(p)
        {
            Version = "TriSecRiver_1YD 02.02.2025";
            name = "Полный поток в створе канала  (КЭ)";
        }


        #region Переопределение абстрактные методы IRiver, ITask ...
        /// <summary>
        /// Получение полей придонных касательных напряжений и давления/свободной 
        /// поверхности по контексту задачи усредненных на конечных элементах
        /// </summary>
        /// <param name="tauX">придонное касательное напряжение по х</param>
        /// <param name="tauY">придонное касательное напряжение по у</param>
        /// <param name="P">придонных давления/свободная поверхности потока, по контексту задачи</param>
        public override void GetTau(ref double[] _tauX, ref double[] _tauY, ref double[] P,
                ref double[][] CS, StressesFlag sf = StressesFlag.Nod)
        {
            _tauX = tau;
            if ((Erosion == EBedErosion.SedimentErosion ||
                 Erosion == EBedErosion.BedLoadAndSedimentErosion) &&
                 Params.CalkConcentration != BCalkConcentration.NotCalkConcentration)
            {
                if (Params.CalkConcentration != BCalkConcentration.NotCalkConcentration )
                {
                    // массив концентраций в узлах донно - береговой поверхности
                    MEM.Alloc(1, bottom_x.Length, ref CS);
                    // пробегаем по граничным узлам и записываем для них Ty, Tz 
                    for (int i = 0; i < bottom_x.Length; i++)
                        CS[0][i] = fGcon.FunctionValue(bottom_x[i]);
                }
            }
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
                double waterLevel0 = waterLevel;
                CalkWaterLevel();
                flagErr = 1;
                // определение расчетной области потока и построение КЭ сетки
                if (MEM.Equals(waterLevel0, waterLevel, 0.001) != true ||
                    Erosion != EBedErosion.NoBedErosion)
                    UpDateDomain();

                if (eTaskReady == ETaskStatus.TaskReady)
                {
                    flagErr = 2;
                    // расчет осевой скорости потока
                    taskUx.SolveTaskUx(ref Ux, eddyViscosity, Phi);
                    flagErr = 3;
                    // расчет вторичных потоков в створе
                    switch (Params.ReTask)
                    {
                        case 0: // Навье - Стокс / Рейнольдс не стационар
                            taskPV.SolveReynoldsTask(ref Phi, ref Vortex, eddyViscosity, Ux, dtime);
                            break;
                        case 1: // Навье - Стокс / Рейнольдс стационар
                            taskPV.SolveReynoldsTask(ref Phi, ref Vortex, eddyViscosity, Ux);
                            break;
                        case 2: // Стокс
                            taskPV.SolveStokesTask(ref Phi, ref Vortex, eddyViscosity, Ux);
                            break;
                        default:
                            break;
                    }
                    FlagStartMu = true;
                    flagErr = 4;
                    // расчет  придонных касательных напряжений на дне
                    CalkTauBed(ref tau);
                    MEM.Copy(ref eddyViscosity0, eddyViscosity);
                    flagErr = 5;
                    if (Params.CalkConcentration != BCalkConcentration.NotCalkConcentration)
                    {
                        taskCon.SolveTaskConcentS(ref concentration, eddyViscosity, Phi, tau, (int)Params.CalkConcentration);
                        for (int i =0; i< concentration.Length; i++)
                            Gcon[i] = concentration[i] * taskPV.Vy[i];
                        flagErr = 6;
                        double[] X = mesh.GetCoords(0);
                        double[] Y = mesh.GetCoords(1);
                        MEM.Alloc(riverGates.Length, ref xCon);
                        MEM.Alloc(riverGates.Length, ref Con);
                        for (int i = 0; i < riverGates.Length; i++)
                        {
                            xCon[i] = X[riverGates[i][0]];
                            if (riverGates[i].Length == 1)
                                Con[i] = 0;
                            else
                            {
                                Con[i] = 0;
                                for (int j = 0; j < riverGates[i].Length - 1; j++)
                                {
                                    double h = Math.Abs(Y[riverGates[i][j]] - Y[riverGates[i][j + 1]]);
                                    Con[i] += h * (Gcon[riverGates[i][j]] + Gcon[riverGates[i][j + 1]]);
                                }
                            }
                        }
                        fGcon = new DigFunction(xCon, Con, "Взвешенный расход наносов");
                    }
                    flagErr = 7;
                    // вычисление вязкости потока
                    taskViscosity.SolveTask(ref eddyViscosity, Ux, taskPV.Vy, taskPV.Vz, Phi, Vortex, dtime);
                    MEM.Relax(ref eddyViscosity, eddyViscosity0);
                    flagErr = 8;
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
            CreateCalculationDomain();
        }

        /// <summary>
        /// Формирование данных для отрисовки связанных с сеткой IMesh со сороны задачи
        /// </summary>
        /// <param name="sp">контейнер данных</param>
        public override void AddMeshPolesForGraphics(ISavePoint sp)
        {
            base.AddMeshPolesForGraphics(sp);
            if (Params.CalkConcentration != BCalkConcentration.NotCalkConcentration && fGcon != null)
                sp.AddCurve(fGcon);

            if (VelosityUx != null)
                sp.AddCurve(VelosityUx);
            if (VelosityUy != null)
                sp.AddCurve(VelosityUy);
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
        #endregion
        /// <summary>
        /// Создать расчетную область
        /// </summary>
        /// <param name="right"></param>
        /// <param name="left"></param>
        protected override void CreateCalculationDomain()
        {
            // генерация сетки
            bool axisOfSymmetry = Params.axisSymmetry == 1 ? true : false;
            CrossStripMeshOption op = new CrossStripMeshOption();
            op.AxisOfSymmetry = axisOfSymmetry;

            if (meshGenerator == null)
                meshGenerator = SMGManager.GetMeshGenerator(Params.typeMeshGenerator, op);
            mesh = meshGenerator.CreateMesh(ref WetBed, ref riverGates, waterLevel, bottom_x, bottom_y);
            right = meshGenerator.Right();
            left = meshGenerator.Left();
            MEM.Alloc(mesh.CountKnots, ref eddyViscosity, "eddyViscosity");
            MEM.Alloc(mesh.CountKnots, ref Ux, "Ux");
            MEM.Alloc(mesh.CountKnots, ref Phi, "Phi");
            MEM.Alloc(mesh.CountKnots, ref Vortex, "Vortex");
            // память под напряжения в области
            MEM.Alloc(mesh.CountKnots, ref TauY, "TauY");
            MEM.Alloc(mesh.CountKnots, ref TauZ, "TauZ");
            MEM.Alloc(Params.CountKnots, ref tau, "tau");
            // получение ширины ленты для алгебры
            int WidthMatrix = (int)mesh.GetWidthMatrix();
            // TO DO подумать о реклонировании algebra если размер матрицы не поменялся 
            algebra = new AlgebraLUTape((uint)mesh.CountKnots, WidthMatrix, WidthMatrix);
            algebra2 = new AlgebraLUTape((uint)(2 * mesh.CountKnots), 2 * WidthMatrix, 2 * WidthMatrix);
            
            if (Params.CalkConcentration != BCalkConcentration.NotCalkConcentration)
            {
                MEM.Alloc(mesh.CountKnots, ref concentration, "concentration");
                MEM.Alloc(mesh.CountKnots, ref Gcon, "concentration");
                taskCon = new ReynoldsConcentrationTri(1, 0, typeTask);
                taskCon.SetTask(mesh, algebra, wMesh);
            }
            // создание общего враппера сетки для задачи
            Set(mesh, algebra);
            if (taskUx == null)
            {
                switch(Params.velocityOnWL)
                {
                    case BCWLVelocity.AllVelocity:
                        taskUx = new ReynoldsTransportTri(Params.J, Params.RadiusMin, Params.SigmaTask, VelosityUx);
                        taskPV = new ReynoldsVortexStream1YDTri(Params.NLine, Params.SigmaTask, Params.RadiusMin, VelosityUy, Params.theta);
                        break;
                    case BCWLVelocity.onlyVelocityUx:
                        taskUx = new ReynoldsTransportTri(Params.J, Params.RadiusMin, Params.SigmaTask, VelosityUx);
                        taskPV = new ReynoldsVortexStream1YDTri(Params.NLine, Params.SigmaTask, Params.RadiusMin, null, Params.theta);
                        break;
                    case BCWLVelocity.onlyVelocityUy:
                        taskUx = new ReynoldsTransportTri(Params.J, Params.RadiusMin, Params.SigmaTask, null);
                        taskPV = new ReynoldsVortexStream1YDTri(Params.NLine, Params.SigmaTask, Params.RadiusMin, VelosityUy, Params.theta);
                        break;
                    case BCWLVelocity.NoWLVelocity:
                        taskUx = new ReynoldsTransportTri(Params.J, Params.RadiusMin, Params.SigmaTask, null);
                        taskPV = new ReynoldsVortexStream1YDTri(Params.NLine, Params.SigmaTask, Params.RadiusMin, null, Params.theta);
                        break;
                }
                BEddyViscosityParam p = new BEddyViscosityParam(1, Params.SigmaTask, Params.J, Params.RadiusMin, 
                                    SСhannelForms.halfPorabolic, ECalkDynamicSpeed.u_start_J, Params.mu_const);

                // вычисление начальной вихревой вязкости потока по алгебраической модели Leo_C_van_Rijn1984
                if (MEM.Equals(eddyViscosity.Sum(), 0) == true)
                {
                    taskViscosity = MuManager.Get(ETurbViscType.EddyViscosityConst, p, TypeTask.streamY1D);
                    taskViscosity.SetTask(mesh, algebra, wMesh);
                    taskViscosity.SolveTask(ref eddyViscosity, Ux, null, null, Phi, Vortex, dtime);
                }
                // вычисление начальной вихревой вязкости потока
                taskViscosity = MuManager.Get(Params.turbViscTypeA, p, TypeTask.streamY1D);
                
                //EddyViscosityUpdate();
            }
            if (taskViscosity.Cet_cs() == 1)
                taskViscosity.SetTask(mesh, algebra, wMesh);
            else
                taskViscosity.SetTask(mesh, algebra2, wMesh);
            taskUx.SetTask(mesh, algebra, wMesh);
            taskPV.SetTask(mesh, algebra2, wMesh);

            taskViscosity.SolveTask(ref eddyViscosity, Ux, taskPV.Vy, taskPV.Vz, Phi, Vortex, dtime);
            //taskViscosity.SolveTask(ref eddyViscosity, Ux, null, null, Phi, Vortex, 0);

            unknowns.Clear();
            unknowns.Add(new Unknown("Скорость Ux", Ux, TypeFunForm.Form_2D_Rectangle_L1));
            //switch (Params.ReTask)
            //{
            //    case 0:
            //    case 1:
            //    case 2:
            //        taskPV.SolveStokesTask(ref Phi, ref Vortex, eddyViscosity, Ux);
            //        break;
            //    default:
            //        break;
            //}
            if (Params.ReTask <= 2)
            {
                unknowns.Add(new Unknown("Скорость Vy", taskPV.Vy, TypeFunForm.Form_2D_Rectangle_L1));
                unknowns.Add(new Unknown("Скорость Vz", taskPV.Vz, TypeFunForm.Form_2D_Rectangle_L1));
                unknowns.Add(new Unknown("Скорость V", taskPV.Vy, taskPV.Vz, TypeFunForm.Form_2D_Rectangle_L1));
                unknowns.Add(new Unknown("Функция тока", taskPV.Phi, TypeFunForm.Form_2D_Rectangle_L1));
                unknowns.Add(new Unknown("Вихрь", taskPV.Vortex, TypeFunForm.Form_2D_Rectangle_L1));
                if (Params.CalkConcentration != BCalkConcentration.NotCalkConcentration)
                {
                    unknowns.Add(new Unknown("Концентрация", concentration, TypeFunForm.Form_2D_Rectangle_L1));
                    unknowns.Add(new Unknown("Расход взвешенных наносов", Gcon, TypeFunForm.Form_2D_Rectangle_L1));
                }
            }
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
            try
            {
                // генерация сетки
                bool axisOfSymmetry = Params.axisSymmetry == 1 ? true : false;
                CrossStripMeshOption op = new CrossStripMeshOption();
                op.AxisOfSymmetry = axisOfSymmetry;
                if (meshGenerator == null)
                    meshGenerator = new HStripMeshGenerator(op);
                mesh = meshGenerator.CreateMesh(ref WetBed, ref riverGates, waterLevel, bottom_x, bottom_y);
                right = meshGenerator.Right();
                left = meshGenerator.Left();
                // получение ширины ленты для алгебры
                int WidthMatrix = (int)mesh.GetWidthMatrix();
                // TO DO подумать о реклонировании algebra если размер матрицы не поменялся 
                algebra = new AlgebraLUTape((uint)mesh.CountKnots, WidthMatrix, WidthMatrix);
                algebra2 = new AlgebraLUTape((uint)(2 * mesh.CountKnots), 2 * WidthMatrix, 2 * WidthMatrix);
                // создание общего враппера сетки для задачи
                Set(mesh, algebra);

                if (Params.CalkConcentration != BCalkConcentration.NotCalkConcentration)
                {
                    MEM.Alloc(mesh.CountKnots, ref concentration, "concentration");
                    MEM.Alloc(mesh.CountKnots, ref Gcon, "concentration");
                    taskCon = new ReynoldsConcentrationTri(1, 0, typeTask);
                    taskCon.SetTask(mesh, algebra, wMesh);
                }

                if (taskViscosity.Cet_cs() == 1)
                    taskViscosity.SetTask(mesh, algebra, wMesh);
                else
                    taskViscosity.SetTask(mesh, algebra2, wMesh);
                taskUx.SetTask(mesh, algebra, wMesh);
                taskPV.SetTask(mesh, algebra2, wMesh);

                if (mesh.CountKnots != Ux.Length)
                {
                    MEM.Alloc(mesh.CountKnots, ref eddyViscosity, "eddyViscosity");
                    BEddyViscosityParam p = new BEddyViscosityParam(1, Params.SigmaTask,
                                Params.J, Params.RadiusMin, SСhannelForms.halfPorabolic);
                    IEddyViscosityTri tv = MuManager.Get(ETurbViscType.Leo_C_van_Rijn1984, p, TypeTask.streamY1D);
                    tv.SetTask(mesh, algebra, wMesh);
                    tv.SolveTask(ref eddyViscosity, Ux, Phi, null, null, Vortex, dtime);

                    MEM.Alloc(mesh.CountKnots, ref Ux, "Ux");
                    MEM.Alloc(mesh.CountKnots, ref Phi, "Phi");
                    MEM.Alloc(mesh.CountKnots, ref Vortex, "Vortex");
                    // память под напряжения в области
                    MEM.Alloc(mesh.CountKnots, ref TauY, "TauY");
                    MEM.Alloc(mesh.CountKnots, ref TauZ, "TauZ");
                    MEM.Alloc(Params.CountKnots, ref tau, "tau");

                    if (eddyViscosity0 == null)
                        MEM.Copy(ref eddyViscosity0, eddyViscosity);
                    if (eddyViscosity0.Length != eddyViscosity.Length)
                        MEM.Copy(ref eddyViscosity0, eddyViscosity);

                    unknowns.Clear();
                    unknowns.Add(new Unknown("Скорость Ux", Ux, TypeFunForm.Form_2D_Rectangle_L1));
                    unknowns.Add(new Unknown("Скорость Vy", taskPV.Vy, TypeFunForm.Form_2D_Rectangle_L1));
                    unknowns.Add(new Unknown("Скорость Vz", taskPV.Vz, TypeFunForm.Form_2D_Rectangle_L1));
                    unknowns.Add(new Unknown("Скорость V", taskPV.Vy, taskPV.Vz, TypeFunForm.Form_2D_Rectangle_L1));
                    unknowns.Add(new Unknown("Функция тока", taskPV.Phi, TypeFunForm.Form_2D_Rectangle_L1));
                    unknowns.Add(new Unknown("Вихрь", taskPV.Vortex, TypeFunForm.Form_2D_Rectangle_L1));
                    if (Params.CalkConcentration != BCalkConcentration.NotCalkConcentration)
                    {
                        unknowns.Add(new Unknown("Концентрация", concentration, TypeFunForm.Form_2D_Rectangle_L1));
                        unknowns.Add(new Unknown("Расход взвешенных наносов", Gcon, TypeFunForm.Form_2D_Rectangle_L1));
                    }

                    unknowns.Add(new CalkPapams("Турб. вязкость", eddyViscosity, TypeFunForm.Form_2D_Rectangle_L1));
                    unknowns.Add(new CalkPapams("Поле напряжений T_xy", TauY, TypeFunForm.Form_2D_Rectangle_L1));
                    unknowns.Add(new CalkPapams("Поле напряжений T_xz", TauZ, TypeFunForm.Form_2D_Rectangle_L1));
                    unknowns.Add(new CalkPapams("Поле напряжений", TauY, TauZ, TypeFunForm.Form_2D_Rectangle_L1));
                }
                eTaskReady = ETaskStatus.TaskReady;
            }
            catch(Exception ex)
            {
                Logger.Instance.Error(ex.Message, "TriSecRiver_1YD.UpDateDomain");
            }
        }
        /// <summary>
        /// Загрузка геометрии области из файла
        /// </summary>
        /// <param name="file"></param>
        public override void LoadData(StreamReader file)
        {
            // геометрия дна
            Geometry = new DigFunction();
            // свободная поверхность
            WaterLevels = new DigFunction();
            // расход потока
            FlowRate = new DigFunction();
            // Нормальная скорость на WL
            VelosityUx = new DigFunction();
            // Радиальная скорость на WL
            VelosityUy = new DigFunction();
            // шероховатость дна
            Roughness = new DigFunction();
            //
            evolution.Clear();
            // геометрия дна
            Geometry.Load(file);
            // свободная поверхность
            WaterLevels.Load(file);
            // расход потока
            FlowRate.Load(file);
            // Нормальная скорость на WL
            VelosityUx.Load(file);
            // Радиальная скорость на WL
            VelosityUy.Load(file);
            // шероховатость дна
            Roughness.Load(file);
            // инициализация задачи
            InitTask();
            // готовность задачи
            eTaskReady = ETaskStatus.LoadAreaData;
            crossFunctions = new IDigFunction[5]
            {
                Geometry, WaterLevels, FlowRate, VelosityUx, VelosityUy
            };
            CreateCalculationDomain();
        }
        /// <summary>
        /// Загрузка задачи из тестовых данных
        /// </summary>
        /// <param name="file">имя файла</param>
        public override void LoadData(IDigFunction[] crossFunctions)
        {
            evolution.Clear();
            // геометрия дна
            Geometry = crossFunctions[0];
            // свободная поверхность
            WaterLevels = crossFunctions[1];
            // расход потока
            FlowRate = crossFunctions[2];
            // Нормальная скорость на WL
            VelosityUx = crossFunctions[3];
            // Радиальная скорость на WL
            VelosityUy = crossFunctions[4];
            // шероховатость дна
            Roughness = crossFunctions[5];
            // инициализация задачи
            InitTask();
            // готовность задачи
            eTaskReady = ETaskStatus.LoadAreaData;
            this.crossFunctions = crossFunctions;
            CreateCalculationDomain();
        }
        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public override IRiver Clone()
        {
            return new TriSecRiver_1YD(Params);
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
