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
namespace NPRiverLib.APRiver1YD
{
    using MemLogLib;
    using AlgebraLib;
    using GeometryLib;

    using NPRiverLib.IO;

    using CommonLib;
    using CommonLib.IO;

    using System;

    using FEMTasksLib.FESimpleTask;
    using NPRiverLib.APRiver1YD.Params;
    using MeshGeneratorsLib.StripGenerator;
    using MeshLib.Wrappers;
    using NPRiverLib.ATask;
    using CommonLib.ChannelProcess;
    using CommonLib.Physics;
    using System.Collections.Generic;
    using CommonLib.Mesh;

    /// <summary>
    ///  ОО: Определение класса TriSroSecRiverTask1YD - расчет полей скорости, вязкости 
    ///       и напряжений в живом сечении потока методом КЭ (симплекс элементы)
    /// </summary>
    [Serializable]
    public class TriSroSecRiverTask1YD : APRiverFEM1YD
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public TriSroSecRiverTask1YD() : this(new RSCrossParams()) { }
        /// <summary>
        /// Конструктор
        /// </summary>
        public TriSroSecRiverTask1YD(RSCrossParams p) : base(p)
        {
            Version = "TriSroSecRiverTask 21.02.2022";
            name = "Поток в створе канала (КЭ)";
        }
        /// <summary>
        /// Расчет полей глубины и скоростей на текущем шаге по времени
        /// </summary>
        public override void SolverStep()
        {
            int flagErr = 0;
            try
            {
                if (eTaskReady < ETaskStatus.CreateMesh)
                {
                    CreateCalculationDomain();
                    // готовность задачи
                    eTaskReady = ETaskStatus.TaskReady;
                }
                if (eTaskReady == ETaskStatus.TaskReady)
                {
                    // расчет уровня свободной поверхности реки
                    double waterLevel0 = waterLevel;
                    CalkWaterLevel();
                    flagErr++;
                    // определение расчетной области потока и построение КЭ сетки
                    if (MEM.Equals(waterLevel0, waterLevel, 0.001) != true || Erosion != EBedErosion.NoBedErosion)
                        CreateCalculationDomain();
                    flagErr++;
                    // расчет гидрадинамики  (скоростей потока)
                    SPhysics.PHYS.turbViscType = ETurbViscType.Leo_C_van_Rijn1984;
                    SPhysics.PHYS.calkTurbVisc(ref eddyViscosity, typeTask, (IMWCrossSection)wMesh, Params.typeEddyViscosity, Ux, Params.J);
                    uint[] bc = mesh.GetBoundKnotsByMarker(0);
                    // вычисление скорости
                    taskPoisson.FEPoissonTask(ref Ux, eddyViscosity, bc, null, Q);
                    FlagStartMu = true;
                    flagErr++;
                    // расчет  придонных касательных напряжений на дне
                    tau = TausToVols(in bottom_x, in bottom_y);
                    flagErr++;
                    // сохранение данных в начальный момент времени
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
        protected override void CreateCalculationDomain()
        {
            // генерация сетки
            if (meshGenerator == null)
                //meshGenerator = new HStripMeshGeneratorTri();
                meshGenerator = new HStripMeshGenerator();
            mesh = meshGenerator.CreateMesh(ref WetBed, waterLevel, bottom_x, bottom_y);
            right = meshGenerator.Right();
            left = meshGenerator.Left();
            // получение ширины ленты для алгебры
            int WidthMatrix = (int)mesh.GetWidthMatrix();
            // TO DO подумать о реклонировании algebra если размер матрицы не поменялся 
            algebra = new AlgebraGaussTape((uint)mesh.CountKnots, (uint)WidthMatrix);

            MEM.Alloc(mesh.CountKnots, ref eddyViscosity, "eddyViscosity");
            MEM.Alloc(mesh.CountKnots, ref eddyViscosity0, "eddyViscosity");
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

            wMesh = new MWCrossTri(mesh);
        }
        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public override IRiver Clone()
        {
            return new TriSroSecRiverTask1YD(Params);
        }
        /// <summary>
        /// Создает экземпляр класса конвертера
        /// </summary>
        /// <returns></returns>
        public override IOFormater<IRiver> GetFormater()
        {
            return new TaskReader1YD_RvY();
        }
        /// <summary>
        /// Создает список тестовых задач для загрузчика по умолчанию
        /// </summary>
        /// <returns></returns>
        //public override List<string> GetTestsName()
        //{
        //    List<string> strings = new List<string>();
        //    strings.Add("Основная задача - тестовая");
        //    return strings;
        //}
    }
}
