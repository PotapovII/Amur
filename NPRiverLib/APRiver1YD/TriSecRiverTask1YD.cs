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
//                 кодировка : 29.12.2024 Потапов И.И.
//                      Сбока No Property River Lib
//              с убранными из наследников Property классами 
//---------------------------------------------------------------------------
namespace NPRiverLib.APRiver1YD
{
    using System;
    using System.IO;

    using MemLogLib;
    using AlgebraLib;
    using GeometryLib;

    using NPRiverLib.IO;
    using NPRiverLib.ABaseTask;
    using NPRiverLib.APRiver1YD.Params;

    using MeshLib.Wrappers;
    using MeshGeneratorsLib.StripGenerator;

    using FEMTasksLib.FESimpleTask;

    using CommonLib;
    using CommonLib.IO;
    using CommonLib.Mesh;
    using CommonLib.Physics;
    using CommonLib.ChannelProcess;
    using CommonLib.EddyViscosity;


    /// <summary>
    ///  ОО: Определение класса TriSroSecRiverTask1YD - расчет полей скорости, вязкости 
    ///       и напряжений в живом сечении потока методом КЭ (симплекс элементы)
    /// </summary>
    [Serializable]
    public class TriSecRiverTask1YD : APRiverFEM1YD
    {
        double[] eddyViscosity0;
        /// <summary>
        /// Конструктор
        /// </summary>
        public TriSecRiverTask1YD() : this(new RSCrossParams()) { }
        /// <summary>
        /// Конструктор
        /// </summary>
        public TriSecRiverTask1YD(RSCrossParams p) : base(p)
        {
            Version = "TriSroSecRiverTask 21.02.2022";
            name = "Поток в створе канала  (КЭ)";
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
                    CreateCalculationDomain();
                // расчет гидрадинамики  (скоростей потока)
                MEM.Relax(ref eddyViscosity0, eddyViscosity, 0.3);
                if (eTaskReady == ETaskStatus.TaskReady)
                {
                    flagErr++;
                    uint[] bc = mesh.GetBoundKnotsByMarker(0);
                    // вычисление скорости
                    taskPoisson.FEPoissonTask(ref Ux, eddyViscosity, bc, null, Q);
                    FlagStartMu = true;
                    flagErr++;
                    // расчет  придонных касательных напряжений на дне
                    tau = TausToVols(in bottom_x, in bottom_y);
                    flagErr++;
                    MEM.Copy(ref eddyViscosity0, eddyViscosity);
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

        public override void DefaultCalculationDomain(uint testTaskID = 0)
        {
            CreateCalculationDomain();
        }

        public override void LoadData(StreamReader file)
        {
            base.LoadData(file);
            CreateCalculationDomain();
        }
        new IMWRiver wMesh = null;
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
            right = meshGenerator.Right();
            left = meshGenerator.Left();
            // получение ширины ленты для алгебры
            int WidthMatrix = (int)mesh.GetWidthMatrix();
            // TO DO подумать о реклонировании algebra если размер матрицы не поменялся 
            algebra = new AlgebraGaussTape((uint)mesh.CountKnots, (uint)WidthMatrix);

            MEM.Alloc(mesh.CountKnots, ref eddyViscosity, "eddyViscosity");
            MEM.Alloc(mesh.CountKnots, ref Ux, "Ux");
            // память под напряжения в области
            MEM.Alloc(mesh.CountKnots, ref TauY, "TauY");
            MEM.Alloc(mesh.CountKnots, ref TauZ, "TauZ");
            MEM.Alloc(Params.CountKnots, ref tau, "tau");
            unknowns.Clear();
            unknowns.Add(new Unknown("Скорость Ux", Ux, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new CalkPapams("Турб. вязкость", eddyViscosity, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new CalkPapams("Поле напряжений T_xy", TauY, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new CalkPapams("Поле напряжений T_xz", TauZ, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new CalkPapams("Поле напряжений", TauY, TauZ, TypeFunForm.Form_2D_Rectangle_L1));
            if (taskPoisson == null)
                taskPoisson = new FEPoissonTaskTri(mesh, algebra);
            else
                taskPoisson.SetTask(mesh, algebra);
            wMesh = new MWRiver(mesh);

            SPhysics.PHYS.turbViscType = ETurbViscType.Absi_2012;
            SPhysics.PHYS.calkTurbVisc(ref eddyViscosity, typeTask, (IMWRiver)wMesh, Params.typeEddyViscosity, Ux, Params.J);
            if(eddyViscosity0 == null)
                MEM.Copy(ref eddyViscosity0, eddyViscosity);
            if (eddyViscosity0.Length != eddyViscosity.Length)
                MEM.Copy(ref eddyViscosity0, eddyViscosity);

            eTaskReady = ETaskStatus.TaskReady;
        }
        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public override IRiver Clone()
        {
            return new TriSecRiverTask1YD(Params);
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
