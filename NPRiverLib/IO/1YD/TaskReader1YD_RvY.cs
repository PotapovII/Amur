//---------------------------------------------------------------------------
//                   ПРОЕКТ  "Русловые процессы"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 03.03.2024 Потапов И.И.
//                форматы ( RvX ) для створа реки
//---------------------------------------------------------------------------
namespace NPRiverLib.IO
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;

    
    using CommonLib;
    using CommonLib.IO;
    using CommonLib.Physics;
    using CommonLib.Function;
    using CommonLib.EddyViscosity;

    using MemLogLib;
    using GeometryLib;
    using NPRiverLib.APRiver1YD;
    using NPRiverLib.APRiver1YD.Params;
    using MeshGeneratorsLib.StripGenerator;
    

    [Serializable]
    public class TaskReader1YD_RvY : ATaskFormat<IRiver>
    {
        /// <summary>
        /// Поддержка внешних форматов загрузки
        /// </summary>
        public override bool SupportImport { get; }
        /// <summary>
        /// Поддержка внешних форматов сохранения
        /// </summary>
        public override bool SupportExport { get; }

        const string Ext_RvY = ".rvy";
        const string Ext_Crf = ".crf";
        public TaskReader1YD_RvY()
        {
            extString = new List<string>() { Ext_RvY, Ext_Crf };
            SupportImport = true;
            SupportExport = true;
        }
        public override void Read(string filename, ref IRiver river, uint testID = 0)
        {
            // расширение файла
            if (testID == 0) // Загрузка по умолчанию
            {
                string FileEXT = Path.GetExtension(filename).ToLower();
                switch (FileEXT)
                {
                    case Ext_RvY:
                        Read_RvY(filename, ref river);
                        break;
                    case Ext_Crf:
                        Read_Crf(filename, ref river);
                        break;
                }
            }
            else
            {
                int SigmaTask = 1;
                ETurbViscType turbViscType;
                //turbViscType = ETurbViscType.Karaushev1977;
                //turbViscType = ETurbViscType.Boussinesq1865;
                //turbViscType = ETurbViscType.Leo_C_van_Rijn1984;
                turbViscType = ETurbViscType.PotapobII_2024;
                APRiverFEM1YD river_1YD = river as APRiverFEM1YD;
                if (river_1YD == null)
                    throw new NotSupportedException("Не возможно загрузить выбранный объект задачи в формате *.RvY, river_1YD == null");
                if (river_1YD as TriSroSecRiverTask1YD != null)
                    river_1YD = river as TriSroSecRiverTask1YD;
                if (river_1YD as TriSecRiverTask1YD != null)
                    river_1YD = river as TriSecRiverTask1YD;
                if (river_1YD as TriSecRiverTask1YBase != null)
                    river_1YD = river as TriSecRiverTask1YBase;
                if (river_1YD as TriSecRiver_1YD != null)
                    river_1YD = river as TriSecRiver_1YD;
                if (river_1YD != null)
                {
                    IDigFunction Geometry = null;
                    IDigFunction WaterLevels = null;
                    IDigFunction FlowRate = null;
                    IDigFunction VelosityUx = null;
                    IDigFunction VelosityUy = null;
                    switch (testID)
                    {
                        case 1: // Канал Розовского
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                            {
                                // создание и чтение свойств задачи                
                                RSCrossParams p = new RSCrossParams()
                                {
                                    J = 0.006,
                                    turbViscTypeA = turbViscType,
                                    turbViscTypeB = turbViscType,
                                    сrossAlgebra = CrossAlgebra.TapeGauss,
                                    taskVariant = TaskVariant.WaterLevelFun,
                                    bcTypeVortex = BCTypeVortex.VortexAllCalk,
                                    typeMeshGenerator = StripGenMeshType.StripMeshGenerator_0,
                                    typeEddyViscosity = ECalkDynamicSpeed.u_start_U,
                                    velocityOnWL = true,
                                    axisSymmetry = 0,
                                    CountKnots = 250,
                                    CountBLKnots = 300,
                                    NLine = 100,
                                    SigmaTask = SigmaTask,
                                    RadiusMin = 4.2,
                                    ReTask = 0,
                                    theta = 0.5
                                };
                                if (testID == 2)
                                    p.ReTask = 1;
                                if (testID == 3)
                                    p.ReTask = 2;
                                //p.velocityOnWL = false;
                                river_1YD.SetParams(p);
                                // Дно створа 18 Розовского
                                double[] YRose2 = { -0.16, 0, 0.12, 0.4, 0.65, 0.8, 0.95, 1.2, 1.48, 1.6, 1.76 };
                                double[] ZRose2 = { 0.204, 0.1401, 0.1, 0.028625, 0, 0, 0, 0.028625, 0.1, 0.1401, 0.204 };
                                Geometry = new DigFunction(YRose2, ZRose2, "Дно створа 18");
                                // свободная поверхность
                                double WL = 0.140;
                                double[] WLy = { YRose2[0], YRose2[YRose2.Length - 1] };
                                double[] WLz = { WL, WL };
                                WaterLevels = new DigFunction(WLy, WLz, "свободная поверхность");
                                // расход
                                double Q = 0.1; ;
                                double[] timeArg = { 0, 1000 };
                                double[] q = { Q, Q };
                                FlowRate = new DigFunction(timeArg, q, "расход");
                                if (testID == 1 || testID == 2 || testID == 3) // Створ 15
                                {
                                    
                                    double[] Y_U15 = { 0, 0.1, 0.3, 0.55, 0.8, 1.05, 1.2, 1.5, 1.6 };
                                    double[] U15 = { 0, 0.243, 0.365, 0.38, 0.38, 0.365, 0.345, 0.186, 0 };
                                    VelosityUx = new DigFunction(Y_U15, U15, "Створ");
                                    double[] Y_V15 = { 0, 0.55, 0.8, 1.05, 1.2, 1.6 };
                                    double[] V15 = { 0, 0.03, 0.028, 0.028, 0.038, 0 };
                                    VelosityUy = new DigFunction(Y_V15, V15, "Створ");
                                }
                                if (testID == 4) // Створ 18
                                {
                                    // скорость окружная
                                    double[] Y_U18 = { 0, 0.15, 0.4, 0.6, 0.8, 1.07, 1.25, 1.4, 1.6 };
                                    double[] U18 = { 0, 0.2, 0.32, 0.38, 0.4, 0.4, 0.38, 0.26, 0 };
                                    VelosityUx = new DigFunction(Y_U18, U18, "Ux");
                                    // скорость радиальная
                                    double[] Y_V18 = { 0, 0.4, 0.6, 0.8, 1.07, 1.25, 1.6 };
                                    double[] V18 = { 0, 0.028, 0.039, 0.041, 0.039, 0.032, 0 };
                                    VelosityUy = new DigFunction(Y_V18, V18, "Vy");
                                }
                                if (testID == 5) // Створ 21
                                {
                                    double[] Y_U21 = { 0, 0.17, 0.5, 0.8, 1.13, 1.45, 1.6 };
                                    double[] U21 = { 0, 0.16, 0.35, 0.4, 0.45, 0.37, 0 };
                                    VelosityUx = new DigFunction(Y_U21, U21, "Створ");
                                    double[] Y_V21 = { 0, 0.5, 0.8, 1.13, 1.6 };
                                    double[] V21 = { 0, 0.022, 0.021, 0.021, 0 };
                                    VelosityUy = new DigFunction(Y_V21, V21, "Створ");
                                }
                            }
                            break;
                        case 6: // Канал Вим ван Балена
                        case 7:
                        case 8:
                            {
                                // создание и чтение свойств задачи                
                                RSCrossParams p = new RSCrossParams()
                                {
                                    J = 0.006,
                                    turbViscTypeA = turbViscType,
                                    turbViscTypeB = turbViscType,
                                    сrossAlgebra = CrossAlgebra.TapeGauss,
                                    taskVariant = TaskVariant.WaterLevelFun,
                                    bcTypeVortex = BCTypeVortex.VortexAllCalk,
                                    typeMeshGenerator = StripGenMeshType.StripMeshGenerator_3,
                                    typeEddyViscosity = ECalkDynamicSpeed.u_start_U,
                                    velocityOnWL = true,
                                    axisSymmetry = 1,
                                    CountKnots = 200,
                                    CountBLKnots = 300,
                                    NLine = 100,
                                    SigmaTask = SigmaTask,
                                    RadiusMin = 3.85,
                                    ReTask = 0,
                                    theta = 0.5
                                };
                                river_1YD.SetParams(p);
                                // Дно Канал Вим ван Балена
                                double WL = 0.052; // два дюйма
                                double W = 0.5; // ширина канал
                                                // а
                                double[] ox = { 0, W };
                                double[] oy = { 0, 0 };
                                int Ny = 10;
                                double[] xx = null;
                                double[] yy = null;
                                MEM.Alloc(Ny, ref xx);
                                MEM.Alloc(Ny, ref yy);
                                double dx = (ox[1] - ox[0]) / (Ny - 1);
                                IDigFunction fun = new DigFunction(ox, oy, "Дно");
                                
                                for (int i = 0; i < xx.Length; i++)
                                {
                                    double x = ox[0] + dx * i;
                                    xx[i] = x;
                                    yy[i] = fun.FunctionValue(x);
                                }
                                Geometry = new DigFunction(xx, yy, "Дно");
                                // свободная поверхность
                                double[] WLy = { 0, W };
                                double[] WLz = { WL, WL };
                                WaterLevels = new DigFunction(WLy, WLz, "свободная поверхность");
                                // расход
                                double Q = 0.1; ;
                                double[] timeArg = { 0, 1000 };
                                double[] q = { Q, Q };
                                FlowRate = new DigFunction(timeArg, q, "расход");
                                if (testID == 6) // Створ 29 градусов
                                {
                                    double[] Y29 = { 0, 0.023, 0.035, 0.06, 0.1, 0.171, 0.25, 0.33, 0.4, 0.44, 0.465, 0.485, 0.5 };
                                    double[] U29 = { 0, 0.1863, 0.1911, 0.2176, 0.252, 0.29, 0.2922, 0.2848, 0.2815, 0.2547, 0.2204, 0.2009, 0 };
                                    double[] V29 = { 0, 0.00458, 0.01115, 0.01494, 0.01992, 0.02015, 0.0259, 0.02613, 0.01922, -0.00088, -0.01871, -0.01352, 0 };
                                    VelosityUx = new DigFunction(Y29, U29, "Створ U29");
                                    VelosityUy = new DigFunction(Y29, V29, "Створ V29");
                                }
                                if (testID == 7) // Створ 60 градусов
                                {
                                    double[] Y60 = { 0, 0.023, 0.035, 0.06, 0.1, 0.17, 0.25, 0.33, 0.4, 0.44, 0.465, 0.485, 0.5 };
                                    double[] U60 = { 0, 0.157, 0.185, 0.208, 0.2332, 0.2573, 0.279, 0.2885, 0.2977, 0.2732, 0.2363, 0.2079, 0 };
                                    double[] V60 = { 0, 0.00476, 0.01156, 0.01472, 0.01752, 0.01639, 0.02066, 0.02341, 0.01076, -0.0005, -0.01106, -0.01384, 0 };
                                    VelosityUx = new DigFunction(Y60, U60, "Створ U60");
                                    VelosityUy = new DigFunction(Y60, V60, "Створ V60");
                                }
                                if (testID == 8) // Створ 135 градусов
                                {
                                    double[] Y145 = { 0, 0.023, 0.035, 0.06, 0.1, 0.17, 0.25, 0.33, 0.4, 0.44, 0.465, 0.485, 0.5 };
                                    double[] U145 = { 0, 0.1613, 0.1847, 0.2072, 0.2357, 0.2642, 0.2847, 0.3014, 0.3266, 0.3239, 0.2823, 0.2522, 0 };
                                    double[] V145 = { 0, 0.00519, 0.01092, 0.01491, 0.01774, 0.0201, 0.02062, 0.02124, 0.00954, -0.00176, -0.0167, -0.02049, 0 };
                                    VelosityUx = new DigFunction(Y145, U145, "Створ U145");
                                    VelosityUy = new DigFunction(Y145, V145, "Створ V145");
                                }
                            }
                            break;
                        case 9: // Река Десна
                        case 10:
                        case 11:
                        case 12:
                            {
                                // создание и чтение свойств задачи                
                                RSCrossParams p = new RSCrossParams()
                                {
                                    J = 0.0000274,
                                    turbViscTypeA = turbViscType,
                                    turbViscTypeB = turbViscType,
                                    сrossAlgebra = CrossAlgebra.TapeGauss,
                                    taskVariant = TaskVariant.WaterLevelFun,
                                    bcTypeVortex = BCTypeVortex.VortexAllCalk,
                                    typeMeshGenerator = StripGenMeshType.StripMeshGenerator_3,
                                    typeEddyViscosity = ECalkDynamicSpeed.u_start_U,
                                    velocityOnWL = true,
                                    axisSymmetry = 1,
                                    CountKnots = 500,
                                    CountBLKnots = 600,
                                    NLine = 20,
                                    SigmaTask = SigmaTask,
                                    RadiusMin = 3.85,
                                    ReTask = 0,
                                    theta = 0.5
                                };
                                if (testID == 11)
                                    p.ReTask = 1;
                                if (testID == 12)
                                    p.ReTask = 2;
                                
                                river_1YD.SetParams(p);

                                if (testID == 9)
                                {
                                    p.RadiusMin = 320; // 400
                                                       // Створ 1
                                    double[] YDesna1 = { -1, 0, 4.0, 10.5, 19.0, 27.0, 34.5, 42.5, 53.5, 60.5, 65, 70.5, 80.5, 89.5, 98.5, 109.5, 122, 135.5, 147, 148.5, 157.5, 158.5 };
                                    double[] HDesna1 = { -1, 0, 1.55, 3.3, 3.3, 3.6, 3.65, 3.7, 3.75, 3.4, 3.3, 3.4, 3.45, 3, 3, 2.95, 2.55, 2.8, 2.05, 1.4, 0, -1 };
                                    Geometry = Create(YDesna1, HDesna1);
                                    // свободная поверхность
                                    double WL = 3.75;
                                    double[] WLy = { YDesna1[0], YDesna1[YDesna1.Length - 1] };
                                    double[] WLz = { WL, WL };
                                    WaterLevels = new DigFunction(WLy, WLz, "свободная поверхность");
                                    // расход
                                    double Q = 184.0; ;
                                    double[] timeArg = { 0, 1000 };
                                    double[] q = { Q, Q };
                                    FlowRate = new DigFunction(timeArg, q, "расход");

                                    double[] YDesna1S = { 0, 19.0, 42.5, 65, 89.5, 109.5, 135.5, 157.5 };
                                    double[] YDesna1U = { 0, 0.64, 0.72, 0.7, 0.55, 0.4, 0.3, 0 };
                                    VelosityUx = new DigFunction(YDesna1S, YDesna1U, "Створ");
                                    double[] YDesna1V = { 0, -0.02, 0.02, -0.01, -0.05, -0.04, -0.03, 0 };
                                    VelosityUy = new DigFunction(YDesna1S, YDesna1V, "Створ");
                                }
                                else
                                if (testID == 10 || testID == 11 || testID == 12) // Створ 4
                                {
                                    double[] YDesna4 = { 28, 29, 39, 43.5, 48.5, 52.0, 56.0, 63.5, 69.5, 74.0, 78.5, 83.5, 89.0, 93.0, 99.5, 104.5, 113, 120.0, 127, 130.0, 134, 139.5, 146.5, 151, 155, 160, 161 };
                                    double[] HDesna4 = { -1, 0, 0.55, 0.8, 1.45, 1.5, 2.2, 2.8, 3.15, 3.6, 4.0, 4.0, 4.25, 4.6, 4.7, 5.0, 5.4, 6.6, 6.05, 5.95, 6.1, 6.0, 5.6, 3.0, 1.2, 0, -1 };
                                    Geometry = Create(YDesna4, HDesna4);
                                    // свободная поверхность
                                    double WL = HDesna4.Max();
                                    double[] WLy = { YDesna4[0], YDesna4[YDesna4.Length - 1] };
                                    double[] WLz = { WL, WL };
                                    WaterLevels = new DigFunction(WLy, WLz, "свободная поверхность");
                                    // расход
                                    double Q = 0.1; ;
                                    double[] timeArg = { 0, 1000 };
                                    double[] q = { Q, Q };
                                    FlowRate = new DigFunction(timeArg, q, "расход");

                                    double[] YDesna4S = { 0, 52.0, 69.5, 99.5, 120, 139.5, 160.0 };
                                    double[] YDesna4U = { 0, 0.4, 0.4, 0.45, 0.5, 0.47, 0 };
                                    VelosityUx = new DigFunction(YDesna4S, YDesna4U, "Створ");
                                    double[] YDesna4V = { 0, 0.02, 0.06, 0.04, 0.05, 0.04, 0 };
                                    VelosityUy = new DigFunction(YDesna4S, YDesna4V, "Створ");
                                }
                            }
                            break;
                        case 13: // Каверна
                        case 14:
                        case 15:
                            {
                                //turbViscType = ETurbViscType.Boussinesq1865;
                                turbViscType = ETurbViscType.Karaushev1977;
                                // создание и чтение свойств задачи                
                                RSCrossParams p = new RSCrossParams()
                                {
                                    J = 0.0000274,
                                    turbViscTypeA = turbViscType,
                                    turbViscTypeB = turbViscType,
                                    сrossAlgebra = CrossAlgebra.TapeGauss,
                                    taskVariant = TaskVariant.WaterLevelFun,
                                    bcTypeVortex = BCTypeVortex.VortexAllCalk,
                                    typeMeshGenerator = StripGenMeshType.StripMeshGenerator_3,
                                    typeEddyViscosity = ECalkDynamicSpeed.u_start_U,
                                    velocityOnWL = true,
                                    axisSymmetry = 1,
                                    CountKnots = 30,
                                    CountBLKnots = 40,
                                    NLine = 20,
                                    SigmaTask = 0, //SigmaTask,
                                    RadiusMin = 3.85,
                                    ReTask = 0,
                                    theta = 0.5
                                };
                                if (testID == 14)
                                    p.ReTask = 1;
                                if (testID == 15)
                                    p.ReTask = 2;
                                river_1YD.SetParams(p);
                                // Дно Канал Вим ван Балена
                                double WL = 1; // два дюйма
                                double H = 0.5; // ширина канал
                                double[] ox = { 0, WL };
                                double[] oy = { -H, -H };
                                Geometry = new DigFunction(ox, oy, "Дно");
                                // свободная поверхность
                                double[] WLy = { 0, WL };
                                double[] WLz = { H, H };
                                WaterLevels = new DigFunction(WLy, WLz, "свободная поверхность");
                                // расход
                                double Q = 0.1; ;
                                double[] timeArg = { 0, 1000 };
                                double[] q = { Q, Q };
                                FlowRate = new DigFunction(timeArg, q, "расход");
                                double[] Y29 = { 0, WL };
                                double[] U29 = { 0, 0 };
                                VelosityUx = new DigFunction(Y29, U29, "Створ U29");
                                double[] V29 = { 0, 0.1 };
                                VelosityUy = new DigFunction(Y29, V29, "Створ V29");
                            }
                            break;
                        case 16:
                        case 17:
                            {
                                turbViscType = ETurbViscType.Karaushev1977;
                                // создание и чтение свойств задачи                
                                RSCrossParams p = new RSCrossParams()
                                {
                                    J = 0.0000274,
                                    turbViscTypeA = turbViscType,
                                    turbViscTypeB = turbViscType,
                                    сrossAlgebra = CrossAlgebra.TapeGauss,
                                    taskVariant = TaskVariant.WaterLevelFun,
                                    bcTypeVortex = BCTypeVortex.VortexAllCalk,
                                    typeMeshGenerator = StripGenMeshType.StripMeshGenerator_3,
                                    typeEddyViscosity = ECalkDynamicSpeed.u_start_U,
                                    velocityOnWL = true,
                                    axisSymmetry = 1,
                                    CountKnots = 400,
                                    CountBLKnots = 450,
                                    NLine = 20,
                                    SigmaTask = 0, //SigmaTask,
                                    RadiusMin = 3.85,
                                    ReTask = 0,
                                    theta = 0.5
                                };
                                if (testID == 14)
                                    p.ReTask = 1;
                                if (testID == 15)
                                    p.ReTask = 2;
                                river_1YD.SetParams(p);
                                int Ny = 20;
                                double[] xx = null;
                                double[] yy = null;
                                //// Дно створа 15 Розовского
                                //Geometry = new DigFunction(xx, yy, "Дно створа 15");
                                Geometry = new FunctionСhannelRose();
                                Geometry.GetFunctionData(ref xx, ref yy, Ny);
                                // свободная поверхность
                                double WL = 0.140;
                                double[] WLy = { xx[0], xx[xx.Length - 1] };
                                double[] WLz = { WL, WL };
                                WaterLevels = new DigFunction(WLy, WLz, "свободная поверхность");
                                // расход
                                double Q = 0.1; ;
                                double[] timeArg = { 0, 1000 };
                                double[] q = { Q, Q };
                                FlowRate = new DigFunction(timeArg, q, "расход");
                                // Створ 15
                                double[] Y_U15 = { 0, 0.1, 0.3, 0.55, 0.8, 1.05, 1.2, 1.5, 1.6 };
                                double[] U15 = { 0, 0.243, 0.365, 0.38, 0.38, 0.365, 0.345, 0.186, 0 };
                                VelosityUx = new DigFunction(Y_U15, U15, "Створ");
                                double[] Y_V15 = { 0, 0.55, 0.8, 1.05, 1.2, 1.6 };
                                double[] V15 = { 0, 0.03, 0.028, 0.028, 0.038, 0 };
                                VelosityUy = new DigFunction(Y_V15, V15, "Створ");
                            }
                            break;
                    }
                    IDigFunction[] crossFunctions = new IDigFunction[5]
                    {
                        Geometry, WaterLevels, FlowRate, VelosityUx, VelosityUy
                    };
                    river_1YD.LoadData(crossFunctions);
                }
            }
        }
        public DigFunction Create(double[] Y, double[] H)
        {
            double[] Z = new double[Y.Length];
            double         WL = H.Max();
            for (int i = 0; i < Y.Length; i++)
                Z[i] = WL - H[i];
            return new DigFunction(Y, Z, "Створ");
        }
        /// <summary>
        /// Сохраняем сетку на диск речной формат данных 1DX для створа
        /// </summary>
        /// <param name="mesh">Экземпляр интерфейса<see cref="IMesh" /> interface.</param>
        /// <param name="filename">Путь к файлу для сохранения.</param>
        public override void Write(IRiver river, string filename)
        {
            // расширение файла
            string FileEXT = Path.GetExtension(filename).ToLower();
            switch (FileEXT)
            {
                case Ext_RvY:
                    Write_RvY(river, filename);
                    break;
                case Ext_Crf:
                    Write_Crf(river, filename);
                    break;
            }
        }
        public void Write_Crf(IRiver river, string filename)
        {
            APRiverFEM1YD river_1YD = river as APRiverFEM1YD;
            if (river_1YD == null)
                throw new NotSupportedException("Не возможно сохранить выбранный объект задачи в формате *.RvY, river_1YD == null");
            try
            {
                using (StreamWriter file = new StreamWriter(filename))
                {
                    IDigFunction[] crossFunctions = river_1YD.crossFunctions;
                    for (int i = 0; i < crossFunctions.Length; i++)
                        crossFunctions[i].Save(file);
                    file.Close();
                }
                river = river_1YD;
            }
            catch (Exception ex)
            {
                Logger.Instance.Info(ex.Message);
            }
        }
        public void Write_RvY(IRiver river, string filename)
        {
            APRiverFEM1YD river_1YD = river as APRiverFEM1YD;
            if (river_1YD == null)
                throw new NotSupportedException("Не возможно сохранить выбранный объект задачи в формате *.RvY, river_1YD == null");
            try
            {
                using (StreamWriter file = new StreamWriter(filename))
                {
                    IDigFunction[] crossFunctions = river_1YD.crossFunctions;
                    for (int i=0; i< crossFunctions.Length; i++)
                        crossFunctions[i].Save(file);
                    file.Close();
                }
                river = river_1YD;
            }
            catch (Exception ex)
            {
                Logger.Instance.Info(ex.Message);
            }
        }
        /// <summary>
        /// Прочитать файл, речной формат данных 1DX для створа
        /// </summary>
        /// <param name="filename">Путь к файлу для чтения..</param>
        /// <returns>Экземпляр интерфейса<see cref="IMesh" /> interface.</returns>
        public void Read_RvY(string filename, ref IRiver river)
        {
            filename = WR.path + filename;
            APRiverFEM1YD river_1YD = river as APRiverFEM1YD;
            if (river_1YD == null)
                throw new NotSupportedException("Не возможно загрузить выбранный объект задачи в формате *.RvY, river_1YD == null");
            if (IsSupported(filename) == true)
            {
                try
                {
                    using (StreamReader file = new StreamReader(filename))
                    {
                        if(river_1YD as TriSroSecRiverTask1YD !=null)
                            river_1YD = river as TriSroSecRiverTask1YD;
                        if (river_1YD as TriSecRiverTask1YD != null)
                            river_1YD = river as TriSecRiverTask1YD;
                        if (river_1YD as TriSecRiverTask1YBase != null)
                            river_1YD = river as TriSecRiverTask1YBase;
                        if (river_1YD as TriSecRiver_1YD != null)
                            river_1YD = river as TriSecRiver_1YD;
                        river_1YD.LoadData(file);
                        file.Close();
                    }
                    river = river_1YD;
                }
                catch (Exception ex)
                {
                    Logger.Instance.Info(ex.Message);
                }
            }
            else
                throw new NotSupportedException("Could not load '" + filename + "' file.");
        }

        /// <summary>
        /// Прочитать файл, речной формат данных 1DX для створа
        /// </summary>
        /// <param name="filename">Путь к файлу для чтения..</param>
        /// <returns>Экземпляр интерфейса<see cref="IMesh" /> interface.</returns>
        public void Read_Crf(string filename, ref IRiver river)
        {
            string pfilename;
            if (filename[1] != ':')
                pfilename = WR.path + filename;
            else
                pfilename = filename;
            APRiverFEM1YD river_1YD = river as APRiverFEM1YD;
            if (river_1YD == null)
                throw new NotSupportedException("Не возможно загрузить выбранный объект задачи river_1YD == null");
            if (IsSupported(pfilename) == true)
            {
                try
                {
                    using (StreamReader file = new StreamReader(pfilename))
                    {
                        // создание и чтение свойств задачи                
                        RSCrossParams p = new RSCrossParams();
                        p.Load(file);
                        river_1YD.SetParams(p);
                        // геометрия дна
                        IDigFunction Geometry = new DigFunction();
                        // свободная поверхность
                        IDigFunction WaterLevels = new DigFunction();
                        // расход потока
                        IDigFunction FlowRate = new DigFunction();
                        // Нормальная скорость на WL
                        IDigFunction VelosityUx = new DigFunction();
                        // Радиальная скорость на WL
                        IDigFunction VelosityUy = new DigFunction();
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
                        IDigFunction[] crossFunctions = new IDigFunction[5]
                        {
                            Geometry, WaterLevels, FlowRate, VelosityUx, VelosityUy
                        };
                        river_1YD.LoadData(crossFunctions);
                        file.Close();
                    }
                    river = river_1YD;
                }
                catch (Exception ex)
                {
                    Logger.Instance.Info(ex.Message);
                }
            }
            else
                throw new NotSupportedException("Could not load '" + filename + "' file.");
        }
        /// <summary>
        /// Создает список тестовых задач для загрузчика по умолчанию
        /// </summary>
        /// <returns></returns>
        public override List<string> GetTestsName()
        {
            List<string> list = new List<string>();
            list.Add("Загрузка по умолчанию");       // 0
            list.Add("Канал Розовского, 15 створ (нс Рейнольдс)");  // 1
            list.Add("Канал Розовского, 15 створ (ст Рейнольдс)");  
            list.Add("Канал Розовского, 15 створ (ст Стокс)");  
            list.Add("Канал Розовского, 18 створ");
            list.Add("Канал Розовского, 21 створ");
            list.Add("Канал Вим ван Балена, створ 29 гр."); // 6
            list.Add("Канал Вим ван Балена, створ 60 гр.");
            list.Add("Канал Вим ван Балена, створ 145 гр.");
            list.Add("Река Десна, створ 1"); // 9
            list.Add("Река Десна, створ 4 (нс Рейнольдс)");
            list.Add("Река Десна, створ 4 (ст Рейнольдс)");
            list.Add("Река Десна, створ 4 (ст Стокс)");
            list.Add("Прямоугольная каверна (нс Рейнольдс)"); // 13
            list.Add("Прямоугольная каверна (ст Рейнольдс)"); // 
            list.Add("Прямоугольная каверна (ст Стокс)"); // 
            list.Add("Канал Розовского п., 15 створ (нс Рейнольдс)");  // 16
            list.Add("Канал Розовского п., 15 створ (ст Рейнольдс)");  
            return list;
        }
    }
}
