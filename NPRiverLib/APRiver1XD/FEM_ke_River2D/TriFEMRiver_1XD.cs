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

    using CommonLib;
    using CommonLib.IO;
    using CommonLib.Mesh;
    using CommonLib.Physics;
    using CommonLib.Function;
    using CommonLib.EddyViscosity;
    using CommonLib.ChannelProcess;

    using MemLogLib;
    using AlgebraLib;
    using GeometryLib;
    using EddyViscosityLib;

    using NPRiverLib.IO;
    using FEMTasksLib.FEMTasks.VortexStream;
    using NPRiverLib.IO._1XD.Tests;
    using CommonLib.BedLoad;
    using static alglib;

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
        /// Концентрация взвешенных наносов
        /// </summary>
        protected double[] concentration = null;
        /// <summary>
        /// По узловой расход взвешенных наносов
        /// </summary>
        protected double[] Gcon = null;
        protected double[] xCon = null;
        protected double[] Con = null;
        /// <summary>
        /// Задача для расчета взвешенных наносов
        /// </summary>
        protected ReynoldsConcentrationTri taskCon;
        /// <summary>
        /// Задача для расчета поля скоростей вихря и функции тока
        /// </summary>
        protected ReynoldsVortexStream1XDTri taskPV;
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
                    MEM.Relax(ref eddyViscosity, eddyViscosity0);
                    flagErr = 6;
                    if(Params.CalkConcentration != BCalkConcentration.NotCalkConcentration)
                    {
                        if (taskCon == null)
                        {
                            MEM.Alloc(mesh.CountKnots, ref concentration, "concentration");
                            MEM.Alloc(mesh.CountKnots, ref Gcon, "concentration");
                            taskCon = new ReynoldsConcentrationTri(1, 0, typeTask);
                            taskCon.SetTask(mesh, algebra, wMesh);
                        }
                        taskCon.SolveTaskConcentS(ref concentration, eddyViscosity, Phi, tau, (int)Params.CalkConcentration);
                        //double[] X = mesh.GetCoords(0);
                        //double[] Y = mesh.GetCoords(1);
                        //MEM.Alloc(riverGates.Length, ref xCon);
                        //MEM.Alloc(riverGates.Length, ref Con);
                        //for (int i = 0; i < riverGates.Length; i++)
                        //{
                        //    xCon[i] = X[riverGates[i][0]];
                        //    if (riverGates[i].Length == 1)
                        //        Con[i] = 0;
                        //    else
                        //    {
                        //        Con[i] = 0;
                        //        for (int j = 0; j < riverGates[i].Length - 1; j++)
                        //        {
                        //            double h = Math.Abs(Y[riverGates[i][j]] - Y[riverGates[i][j + 1]]);
                        //            Con[i] += h * (Gcon[riverGates[i][j]] + Gcon[riverGates[i][j + 1]]);
                        //        }
                        //    }
                        //}
                        //fGcon = new DigFunction(xCon, Con, "Взвешенный расход наносов");
                    }    
                    flagErr = 7;
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
        /// Расчет касательных напряжений на дне канала
        /// </summary>
        /// <param name="mesh">сетка</param>
        /// <param name="TauZ">напражения на сетке</param>
        /// <param name="TauY">напражения на сетке</param>
        /// <param name="xv">координаты Х узлов КО</param>
        /// <param name="yv">координаты У узлов КО</param>
        protected override double[] TausToVols(in double[] xv, in double[] yv, bool elements = false)
        {
            uint i1;
            uint i2;
            int CountElVortexWL = 0;
            for (uint belem = 0; belem < mesh.CountBoundElements; belem++)
            {
                int mark = mesh.GetBoundElementMarker(belem);
                if (mark == 2)
                    CountElVortexWL++;
            }
            // массив касательных напряжений
            MEM.Alloc(CountElVortexWL, ref tau);
            MEM.Alloc(CountElVortexWL, ref xtau);
            CountElVortexWL = -1;
            TwoElement[] beKnots = mesh.GetBoundElems();
            int k = 0;
            double[] X = mesh.GetCoords(0);
            for (uint belem = 0; belem < mesh.CountBoundElements; belem++)
            {
                int mark = mesh.GetBoundElementMarker(belem);
                if (mark == 0)
                {
                    i1 = beKnots[belem].Vertex1;
                    i2 = beKnots[belem].Vertex2;
                    xtau[k] = 0.5 * (X[i1] + X[i2]);
                    tau[k++] = -0.5 * (eddyViscosity[i1] * Vortex[i1]
                                     + eddyViscosity[i2] * Vortex[i2]);
                }
            }
            return tau;
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

            if (Params.CalkConcentration != BCalkConcentration.NotCalkConcentration)
            {
                sp.Add("Концентрация взвешенных наносов", concentration);

            }
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
            Test_Cannal_PW_1XD.GetTest(testTaskID, out FEMParams_1XD p, out IDigFunction[] bcPhi,
                     out IDigFunction[] BCVelosity, out IMesh mesh, out riverGates);
            // Заглушка по скоростям
            SetParams(p);
            Set(mesh, null);
            this.bcPhi = bcPhi;
            LoadData(BCVelosity);
        }
        /// <summary>
        /// Установка решателя
        /// </summary>
        protected void InitAlgebra()
        {
            bool isPrecond = true;
            
            switch (Params.typeAlgebra)
            {
                case TypeAlgebra.GMRES_P_Sparce:
                    uint MK = 25;
                    uint MaxIters = 100;
                    algebra = new SparseAlgebraGMRES_P((uint)mesh.CountKnots, MK, isPrecond, MaxIters);
                    algebra2 = new SparseAlgebraGMRES_P((uint)(2 * mesh.CountKnots), MK, isPrecond, MaxIters);
                    break;
                case TypeAlgebra.BeCGM_Sparce:
                    algebra = new SparseAlgebraBeCG((uint)mesh.CountKnots, isPrecond);
                    algebra2 = new SparseAlgebraBeCG((uint)(2 * mesh.CountKnots), isPrecond);
                    break;
                default:
                    // получение ширины ленты для алгебры
                    int WidthMatrix = (int)mesh.GetWidthMatrix();
                    algebra = new AlgebraLUTape((uint)mesh.CountKnots, WidthMatrix, WidthMatrix);
                    algebra2 = new AlgebraLUTape((uint)(2 * mesh.CountKnots), 2 * WidthMatrix, 2 * WidthMatrix);
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
            // Установка решателя
            InitAlgebra();

            if (Params.CalkConcentration != BCalkConcentration.NotCalkConcentration)
            {
                MEM.Alloc(mesh.CountKnots, ref concentration, "concentration");
                taskCon = new ReynoldsConcentrationTri(1, 0, typeTask);
                taskCon.SetTask(mesh, algebra, wMesh);
            }
            // создание общего враппера сетки для задачи
            Set(mesh, algebra);
            if (taskPV == null)
            {
                int outBC = 0;
                if (Params.outBC == TypeBoundCond.Neumann0) 
                    outBC = 1;
                taskPV = new ReynoldsVortexStream1XDTri(Params.NLine,(int) Params.bcTypeOnWL, outBC, BCVelosity, bcPhi, Params.theta);
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
            if (Params.CalkConcentration != BCalkConcentration.NotCalkConcentration)
                unknowns.Add(new Unknown("Концентрация", concentration, TypeFunForm.Form_2D_Rectangle_L1));

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

            // Установка решателя
            InitAlgebra();
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
