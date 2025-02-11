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

    using MeshLib.Wrappers;
    using MeshGeneratorsLib.StripGenerator;

    using CommonLib;
    using CommonLib.IO;
    using CommonLib.Mesh;
    using CommonLib.Function;
    using CommonLib.EddyViscosity;
    using CommonLib.ChannelProcess;

    using EddyViscosityLib;
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
        /// Обертка для КЭ сетки
        /// </summary>
        protected IMeshWrapper wMesh;
        /// <summary>
        /// Задача для расчета нормальной скорости потока
        /// </summary>
        protected ReynoldsTransportTri taskUx;
        /// <summary>
        /// Задача для расчета вторичных скоростей в створе
        /// </summary>
        protected ReynoldsVortexStreamTri taskPV;
        /// <summary>
        /// Задача для расчета турбулентной вязкости потока
        /// </summary>
        protected IEddyViscosityTri taskViscosity;
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
                flagErr++;
                // определение расчетной области потока и построение КЭ сетки
                if (MEM.Equals(waterLevel0, waterLevel, 0.001) != true ||
                    Erosion != EBedErosion.NoBedErosion)
                    UpDateDomain();
                if (eTaskReady == ETaskStatus.TaskReady)
                {
                    flagErr++;
                    // расчет осевой скорости потока
                    taskUx.SolveTaskUx(ref Ux, eddyViscosity, Phi);
                    flagErr++;
                    // расчет вторичных потоков в створе
                    taskPV.SolveTaskRe(ref Phi, ref Vortex, eddyViscosity, Ux, dtime);
                    FlagStartMu = true;
                    flagErr++;
                    // расчет  придонных касательных напряжений на дне
                    tau = TausToVols(in bottom_x, in bottom_y);
                    flagErr++;
                    MEM.Copy(ref eddyViscosity0, eddyViscosity);
                    flagErr++;
                    // вычисление вязкости потока
                    taskViscosity.SolveTask(ref eddyViscosity, Ux, taskPV.Vy, taskPV.Vz, Phi, Vortex, dtime);
                    MEM.Relax(ref eddyViscosity, eddyViscosity0);
                    flagErr++;
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
        /// Создать расчетную область
        /// </summary>
        /// <param name="right"></param>
        /// <param name="left"></param>
        protected override void CreateCalculationDomain()
        {
            // генерация сетки
            bool axisOfSymmetry = Params.axisSymmetry == 1 ? true : false;
            if (meshGenerator == null)
                meshGenerator = new HStripMeshGenerator(axisOfSymmetry);
            //meshGenerator = new HStripMeshGenerator();
            mesh = meshGenerator.CreateMesh(ref WetBed, waterLevel, bottom_x, bottom_y);
            //TestMesh.Show(mesh);
            right = meshGenerator.Right();
            left = meshGenerator.Left();
            wMesh = new MeshWrapperTri(mesh);
            MEM.Alloc(mesh.CountKnots, ref eddyViscosity, "eddyViscosity");
            MEM.Alloc(mesh.CountKnots, ref Ux, "Ux");
            MEM.Alloc(mesh.CountKnots, ref Phi, "Phi");
            MEM.Alloc(mesh.CountKnots, ref Vortex, "Vortex");
            // память под напряжения в области
            MEM.Alloc(mesh.CountKnots, ref TauY, "TauY");
            MEM.Alloc(mesh.CountKnots, ref TauZ, "TauZ");
            MEM.Alloc(Params.CountKnots, ref tau, "tau");

            if (taskUx == null)
            {
                taskUx = new ReynoldsTransportTri(Params.J, Params.RadiusMin, Params.SigmaTask, VelosityUx);
                taskPV = new ReynoldsVortexStreamTri(Params.NLine, Params.SigmaTask, Params.RadiusMin, VelosityUy, Params.theta);
                BEddyViscosityParam p = new BEddyViscosityParam(1, Params.SigmaTask, Params.J, Params.RadiusMin, SСhannelForms.halfPorabolic);
                // вычисление начальной вихревой вязкости потока по алгебраической модели Leo_C_van_Rijn1984
                if (MEM.Equals(eddyViscosity.Sum(), 0) == true)
                {
                    taskViscosity = MuManager.Get(ETurbViscType.Boussinesq1865, p);
                    taskViscosity.SetTask(mesh, algebra, wMesh);
                    taskViscosity.SolveTask(ref eddyViscosity, Ux, null, null, Phi, Vortex, dtime);
                }
                // вычисление начальной вихревой вязкости потока
                taskViscosity = MuManager.Get(Params.turbViscTypeA, p);
            }
            // получение ширины ленты для алгебры
            int WidthMatrix = (int)mesh.GetWidthMatrix();
            // TO DO подумать о реклонировании algebra если размер матрицы не поменялся 
            algebra = new AlgebraLUTape((uint)mesh.CountKnots, WidthMatrix, WidthMatrix);
            algebra2 = new AlgebraLUTape((uint)(2 * mesh.CountKnots), 2 * WidthMatrix, 2 * WidthMatrix);

            if (taskViscosity.Cet_cs() == 1)
                taskViscosity.SetTask(mesh, algebra, wMesh);
            else
                taskViscosity.SetTask(mesh, algebra2, wMesh);
            taskUx.SetTask(mesh, algebra, wMesh);
            taskPV.SetTask(mesh, algebra2, wMesh);

            unknowns.Clear();
            unknowns.Add(new Unknown("Скорость Ux", Ux, TypeFunForm.Form_2D_Rectangle_L1));
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
            bool axisOfSymmetry = Params.axisSymmetry == 1 ? true : false;
            if (meshGenerator == null)
                meshGenerator = new HStripMeshGenerator(axisOfSymmetry);
            //meshGenerator = new HStripMeshGenerator();
            mesh = meshGenerator.CreateMesh(ref WetBed, waterLevel, bottom_x, bottom_y);
            right = meshGenerator.Right();
            left = meshGenerator.Left();
            wMesh = new MeshWrapperTri(mesh);
            // получение ширины ленты для алгебры
            int WidthMatrix = (int)mesh.GetWidthMatrix();
            // TO DO подумать о реклонировании algebra если размер матрицы не поменялся 
            algebra = new AlgebraLUTape((uint)mesh.CountKnots, WidthMatrix, WidthMatrix);
            algebra2 = new AlgebraLUTape((uint)(2 * mesh.CountKnots), 2 * WidthMatrix, 2 * WidthMatrix);

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
                IEddyViscosityTri tv = MuManager.Get(ETurbViscType.Leo_C_van_Rijn1984, p);
                tv.SetTask(mesh, algebra, wMesh);
                tv.SolveTask(ref eddyViscosity, Ux, Phi, null, null, Vortex, dtime);

                MEM.Alloc(mesh.CountKnots, ref Ux, "Ux");
                MEM.Alloc(mesh.CountKnots, ref Ux, "Vy");
                MEM.Alloc(mesh.CountKnots, ref Ux, "Vz");
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
