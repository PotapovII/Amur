//---------------------------------------------------------------------------
//                   ПРОЕКТ  "Русловые процессы"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 23.04.2025 Потапов И.И.
//---------------------------------------------------------------------------
namespace NPRiverLib.IO._1XD.Tests
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    using MemLogLib;
    using GeometryLib;

    using CommonLib;
    using CommonLib.Physics;
    using CommonLib.Function;
    using CommonLib.EddyViscosity;

    using MeshGeneratorsLib.StripGenerator;

    using NPRiverLib.APRiver1YD;
    using NPRiverLib.APRiver1YD.Params;
    using CommonLib.BedLoad;

    /// <summary>
    /// Список тестов для задачи в створе
    /// </summary>
    public class Test_CS_1YD
    {
        /// <summary>
        /// Названия тестов для задачи в створе
        /// </summary>
        /// <returns></returns>
        public static List<string> GetTestsName()
        {
            List<string> list = new List<string>();
            list.Add("Загрузка по умолчанию");       // 0
            list.Add("Канал с выбранной нативной геометрией"); // 3
            list.Add("Канал Розовского, 15 створ (нс Рейнольдс)");  // 1
            list.Add("Канал Розовского, 15 створ (ст Рейнольдс)"); // 2 
            list.Add("Канал Розовского, 18 створ");
            list.Add("Канал Розовского, 21 створ");
            list.Add("Канал Вим ван Балена, створ 29 гр."); // 6
            list.Add("Канал Вим ван Балена, створ 60 гр.");
            list.Add("Канал Вим ван Балена, створ 145 гр.");
            list.Add("Река Десна, створ 1 (ст Рейнольдс)"); // 9
            list.Add("Река Десна, створ 2 (ст Рейнольдс)");
            list.Add("Река Десна, створ 3 (ст Рейнольдс)");
            list.Add("Река Десна, створ 4 (ст Рейнольдс)");
            list.Add("Река Десна, створ 5 (ст Рейнольдс)");
            list.Add("Каверна Гиа, У. Гиа, К. Н.(нс Рейнольдс)"); // 13
            list.Add("Каверна Гиа, У. Гиа, К. Н.(ст Рейнольдс)"); // 
            list.Add("Каверна Гиа, У. Гиа, К. Н.(ст Стокс)"); // 
            list.Add("Канал Розовского п., 15 створ (нс Рейнольдс)");  // 16
            list.Add("Канал Розовского п., 15 створ (ст Рейнольдс)");
            list.Add("Канал Розовского п., плский поток (нс Рейнольдс)");
            list.Add("Канал Розовского п., плский поток (ст Рейнольдс)");
            return list;
        }

        public static DigFunction Create(double[] Y, double[] H)
        {
            double[] Z = new double[Y.Length];
            double WL = H.Max();
            for (int i = 0; i < Y.Length; i++)
                Z[i] = WL - H[i];
            return new DigFunction(Y, Z, "Створ");
        }
        public static void GetTest(ref IRiver river, uint testID = 0)
        {
            int SigmaTask = 1;
            ETurbViscType turbViscType;
            turbViscType = ETurbViscType.Leo_C_van_Rijn1984;
            //turbViscType = ETurbViscType.Boussinesq1865;
            //turbViscType = ETurbViscType.Leo_C_van_Rijn1984;
            //turbViscType = ETurbViscType.Leo_C_van_Rijn1984;
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
                IDigFunction Roughness = null;
                switch (testID)
                {
                    case 2: // Канал Розовского
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
                                typeMeshGenerator = StripGenMeshType.StripMeshGenerator_0,
                                typeEddyViscosity = ECalkDynamicSpeed.u_start_U,
                                velocityOnWL = BCWLVelocity.AllVelocity,
                                axisSymmetry = 0,
                                CountKnots = 300,
                                CountBLKnots = 340,
                                NLine = 100,
                                SigmaTask = SigmaTask,
                                RadiusMin = 4.2,
                                ReTask = 0,
                                theta = 0.5,
                                CalkConcentration = BCalkConcentration.DirCalkConcentration
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
                            // шероховатость дна
                            double ks0 = 0.1;
                            double[] Roug = { ks0, ks0 };
                            Roughness = new DigFunction(WLy, Roug, "Шероховатость дна");

                            // расход
                            double Q = 0.1; ;
                            double[] timeArg = { 0, 1000 };
                            double[] q = { Q, Q };
                            FlowRate = new DigFunction(timeArg, q, "расход");
                            if (testID == 2 || testID == 3) // Створ 15
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
                            turbViscType = ETurbViscType.Boussinesq1865;
                            // создание и чтение свойств задачи                
                            RSCrossParams p = new RSCrossParams()
                            {
                                J = 0.001,
                                turbViscTypeA = turbViscType,
                                turbViscTypeB = turbViscType,
                                сrossAlgebra = CrossAlgebra.TapeGauss,
                                taskVariant = TaskVariant.WaterLevelFun,
                                typeMeshGenerator = StripGenMeshType.StripMeshGenerator_3,
                                typeEddyViscosity = ECalkDynamicSpeed.u_start_U,
                                velocityOnWL = BCWLVelocity.AllVelocity,
                                axisSymmetry = 1,
                                CountKnots = 100,
                                CountBLKnots = 150,
                                NLine = 10,
                                SigmaTask = SigmaTask,
                                RadiusMin = 3.85, // = 4.1 - 0.25 
                                ReTask = 1,
                                theta = 0.5,
                                mu_const = 0.01,
                                // Флаг для расчета взвешенных наносов 
                                CalkConcentration = BCalkConcentration.DirCalkConcentration
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

                            // шероховатость дна
                            double ks0 = 0.1;
                            double[] Roug = { ks0, ks0 };
                            Roughness = new DigFunction(WLy, Roug, "Шероховатость дна");

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
                    case 9:  // Река Десна створ 1
                    case 10: // 2
                    case 11: // 3
                    case 12: // 4
                    case 13: // 5
                        {
                            // расход
                            double Q = 184.0; 
                            //turbViscType = ETurbViscType.PotapobII_2024;
                            turbViscType = ETurbViscType.VanDriest1956;
                            SigmaTask = 1;
                            // создание и чтение свойств задачи                
                            RSCrossParams p = new RSCrossParams()
                            {
                                J = 0.0000274,
                                turbViscTypeA = turbViscType,
                                turbViscTypeB = turbViscType,
                                сrossAlgebra = CrossAlgebra.TapeGauss,
                                taskVariant = TaskVariant.WaterLevelFun,
                                typeMeshGenerator = StripGenMeshType.StripMeshGenerator_3,
                                typeEddyViscosity = ECalkDynamicSpeed.u_start_U,
                                velocityOnWL = BCWLVelocity.AllVelocity,
                                axisSymmetry = 1,
                                CountKnots = 350,
                                CountBLKnots = 400,
                                NLine = 10,
                                SigmaTask = SigmaTask,
                                RadiusMin = 320,
                                ReTask = 1,
                                theta = 0.5,
                                CalkConcentration = BCalkConcentration.DirCalkConcentration
                            };
                            river_1YD.SetParams(p);
                            if (testID == 9) // Створ 1
                            {
                                
                                                   // Створ 1
                                                   //double[] YDesna1 = { -1, 0, 4.0, 10.5, 19.0, 27.0, 34.5, 42.5, 53.5, 60.5, 65, 70.5, 80.5, 89.5, 98.5, 109.5, 122, 135.5, 147, 148.5, 157.5, 158.5 };
                                                   //double[] HDesna1 = { -1, 0, 1.55, 3.3, 3.3, 3.6, 3.65, 3.7, 3.75, 3.4, 3.3, 3.4, 3.45, 3, 3, 2.95, 2.55, 2.8, 2.05, 1.4, 0, -1 };
                                double[] YDesna1 = { -1, 0, 4.0, 10.5, 19.0, 27.0, 34.5, 42.5, 53.0, 60.0, 65, 70.5, 80.5, 89.5, 98.5, 109.5, 122, 135.5, 147, 148.5, 157.5, 158.5 };
                                double[] HDesna1 = { -1, 0, 1.55, 3.3, 3.3, 3.6, 3.65, 3.7, 3.75, 3.4, 3.3, 3.4, 3.45, 3, 3, 2.95, 2.55, 2.8, 2.05, 1.4, 0, -1 };

                                p.RadiusMin = 400 - YDesna1[YDesna1.Length - 2]/2;

                                Geometry = Create(YDesna1, HDesna1);
                                // свободная поверхность
                                double WL = 3.75;
                                double[] WLy = { YDesna1[0], YDesna1[YDesna1.Length - 1] };
                                double[] WLz = { WL, WL };
                                WaterLevels = new DigFunction(WLy, WLz, "свободная поверхность");
                                // шероховатость дна
                                double ks0 = 0.1;
                                double[] Roug = { ks0, ks0 };
                                Roughness = new DigFunction(WLy, Roug, "Шероховатость дна");

                                double[] timeArg = { 0, 1000 };
                                double[] q = { Q, Q };
                                FlowRate = new DigFunction(timeArg, q, "расход");

                                double[] YDesna1S = { 0, 19.0, 42.5, 65, 89.5, 109.5, 135.5, 157.5 };
                                double[] YDesna1U = { 0, 0.63, 0.72, 0.73, 0.55, 0.43, 0.31, 0 };
                                double[] YDesna1V = { 0, 0.050813008, 0.052845528, 0.028455285, 0.008130081, 0.014227642, 0.008130081, 0 };
                                //double[] YDesna1V = { 0, -0.029, 0.018, -0.014, -0.047, -0.036, -0.033, 0 };
                                VelosityUx = new DigFunction(YDesna1S, YDesna1U, "Створ");
                                VelosityUy = new DigFunction(YDesna1S, YDesna1V, "Створ");
                            }   // Створ 1
                            else
                            if (testID == 10) // Створ 2
                            {
                                //double[] YDesna4 = { 28, 29, 39, 43.5, 48.5, 52.0, 56.0, 63.5, 69.5, 74.0, 78.5, 83.5, 89.0, 93.0, 99.5, 104.5, 113, 120.0, 127, 130.0, 134, 139.5, 146.5, 151, 155, 160, 161 };
                                //double[] HDesna4 = { -1, 0, 0.55, 0.8, 1.45, 1.5, 2.2, 2.8, 3.15, 3.6, 4.0, 4.0, 4.25, 4.6, 4.7, 5.0, 5.4, 6.6, 6.05, 5.95, 6.1, 6.0, 5.6, 3.0, 1.2, 0, -1 };
                                double[] YDesna2 = { -1, 0, 10.5, 20.5, 24, 32, 44, 49, 56.5, 58, 64.5, 74, 79, 82, 87, 92, 98.5, 106, 109.5, 112, 117.5, 132, 134, 135 };
                                double[] HDesna2 = { -1, 0, 0.3, 0.45, 0.8, 1.2, 1.7, 1.95, 2.4, 2.3, 2.8, 4.1, 4.4, 4.4, 4.8, 5.9, 6.5, 7.5, 8.0, 8.5, 7.4, 4, 0, -1 };

                                Geometry = Create(YDesna2, HDesna2);
                                // свободная поверхность
                                double WL = HDesna2.Max();
                                double[] WLy = { YDesna2[0], YDesna2[YDesna2.Length - 1] };
                                double[] WLz = { WL, WL };
                                WaterLevels = new DigFunction(WLy, WLz, "свободная поверхность");
                                // шероховатость дна
                                double ks0 = 0.1;
                                double[] Roug = { ks0, ks0 };
                                Roughness = new DigFunction(WLy, Roug, "Шероховатость дна");

                                // расход
                                
                                double[] timeArg = { 0, 1000 };
                                double[] q = { Q, Q };
                                FlowRate = new DigFunction(timeArg, q, "расход");

                                //double[] YDesna4S = { 0, 52.0, 69.5, 99.5, 120, 139.5, 160.0 };
                                //double[] YDesna4U = { 0, 0.4, 0.4, 0.45, 0.5, 0.47, 0 };
                                //double[] YDesna4V = { 0, 0.02, 0.06, 0.04, 0.05, 0.04, 0 };

                                double[] YDesna2S = { 0, 32, 58, 79, 98.5, 135 };
                                double[] YDesna2U = { 0, 0.3125, 0.45, 0.48, 0.5446, 0 };
                                double[] YDesna2V = { 0, -0.0125, 0.039, 0.059, 0.064, 0 };

                                VelosityUx = new DigFunction(YDesna2S, YDesna2U, "Створ");
                                VelosityUy = new DigFunction(YDesna2S, YDesna2V, "Створ");
                            } // Створ 2
                            else
                            if (testID == 11) // Створ 3
                            {
                                //double[] YDesna4 = { 28, 29, 39, 43.5, 48.5, 52.0, 56.0, 63.5, 69.5, 74.0, 78.5, 83.5, 89.0, 93.0, 99.5, 104.5, 113, 120.0, 127, 130.0, 134, 139.5, 146.5, 151, 155, 160, 161 };
                                //double[] HDesna4 = { -1, 0, 0.55, 0.8, 1.45, 1.5, 2.2, 2.8, 3.15, 3.6, 4.0, 4.0, 4.25, 4.6, 4.7, 5.0, 5.4, 6.6, 6.05, 5.95, 6.1, 6.0, 5.6, 3.0, 1.2, 0, -1 };

                                double[] YDesna3 = { -26.5, 27.5, 28.5, 33, 34, 38.0, 41.5, 45.0, 50.0, 53.5, 58, 64.5, 68.0, 72.0, 77.0, 83.0, 88.0, 90.0, 99, 103.0, 109, 118.5, 124.5, 127.5, 131.0, 141.5, 145, 151, 157, 159.5, 160.5 };
                                double[] HDesna3 = { -1, 0, 0.30, 0.6, 0.8, 1.2, 1.6, 1.8, 1.9, 1.8, 2, 2.4, 2.6, 2.8, 3.4, 3.4, 4.0, 3.8, 5.0, 5.2, 5.2, 6.1, 7.6, 6.0, 6.0, 4.8, 5.25, 3.2, 2.2, 0, -1 };

                                Geometry = Create(YDesna3, HDesna3);
                                // свободная поверхность
                                double WL = HDesna3.Max();
                                double[] WLy = { YDesna3[0], YDesna3[YDesna3.Length - 1] };
                                double[] WLz = { WL, WL };
                                WaterLevels = new DigFunction(WLy, WLz, "свободная поверхность");
                                // шероховатость дна
                                double ks0 = 0.1;
                                double[] Roug = { ks0, ks0 };
                                Roughness = new DigFunction(WLy, Roug, "Шероховатость дна");

                                // расход
                                
                                double[] timeArg = { 0, 1000 };
                                double[] q = { Q, Q };
                                FlowRate = new DigFunction(timeArg, q, "расход");

                                //double[] YDesna4S = { 0, 52.0, 69.5, 99.5, 120, 139.5, 160.0 };
                                //double[] YDesna4U = { 0, 0.4, 0.4, 0.45, 0.5, 0.47, 0 };
                                //double[] YDesna4V = { 0, 0.02, 0.06, 0.04, 0.05, 0.04, 0 };

                                double[] YDesna3S = { 0, 50.0, 72.0, 90.0, 109, 127.5, 145, 159.5 };
                                double[] YDesna3U = { 0, 0.27, 0.467, 0.52, 0.47, 0.54, 0.548, 0 };
                                double[] YDesna3V = { 0, 0.022, 0.035, 0.074, 0.074, 0.041, 0.037, 0 };

                                VelosityUx = new DigFunction(YDesna3S, YDesna3U, "Створ");
                                VelosityUy = new DigFunction(YDesna3S, YDesna3V, "Створ");
                            } // Створ 3
                            else
                            if (testID == 12)    // Створ 4
                            {
                                //double[] YDesna4 = { 28, 29, 39, 43.5, 48.5, 52.0, 56.0, 63.5, 69.5, 74.0, 78.5, 83.5, 89.0, 93.0, 99.5, 104.5, 113, 120.0, 127, 130.0, 134, 139.5, 146.5, 151, 155, 160, 161 };
                                //double[] HDesna4 = { -1, 0, 0.55, 0.8, 1.45, 1.5, 2.2, 2.8, 3.15, 3.6, 4.0, 4.0, 4.25, 4.6, 4.7, 5.0, 5.4, 6.6, 6.05, 5.95, 6.1, 6.0, 5.6, 3.0, 1.2, 0, -1 };

                                double[] YDesna4 = { -1, 0, 5, 10, 14.5, 19.5, 23, 27, 34.5, 40.5, 45, 49.5, 54.5, 60, 64, 70.5, 75.5, 84, 91, 98, 101, 105, 110.5, 117.5, 122, 126, 131, 132 };
                                double[] HDesna4 = { -1, 0, 0.55, 0.8, 1.45, 1.4, 1.5, 2.2, 2.8, 3.15, 3.6, 4.0, 4.0, 4.25, 4.6, 4.7, 5.0, 5.4, 6.6, 6.05, 5.95, 6.1, 6.0, 5.6, 3.0, 1.2, 0, -1 };

                                Geometry = Create(YDesna4, HDesna4);
                                // свободная поверхность
                                double WL = HDesna4.Max();
                                double[] WLy = { YDesna4[0], YDesna4[YDesna4.Length - 1] };
                                double[] WLz = { WL, WL };
                                WaterLevels = new DigFunction(WLy, WLz, "свободная поверхность");
                                // шероховатость дна
                                double ks0 = 0.1;
                                double[] Roug = { ks0, ks0 };
                                Roughness = new DigFunction(WLy, Roug, "Шероховатость дна");

                                // расход
                                
                                double[] timeArg = { 0, 1000 };
                                double[] q = { Q, Q };
                                FlowRate = new DigFunction(timeArg, q, "расход");

                                //double[] YDesna4S = { 0, 52.0, 69.5, 99.5, 120, 139.5, 160.0 };
                                //double[] YDesna4U = { 0, 0.4, 0.4, 0.45, 0.5, 0.47, 0 };
                                //double[] YDesna4V = { 0, 0.02, 0.06, 0.04, 0.05, 0.04, 0 };

                                double[] YDesna4S = { 0, 23, 45, 70.5, 91, 110.5, 132 };
                                double[] YDesna4U = { 0, 0.41, 0.4, 0.45, 0.53, 0.46, 0 };
                                double[] YDesna4V = { 0, 0.02432, 0.05405, 0.03243, 0.05405, 0.03784, 0 };

                                VelosityUx = new DigFunction(YDesna4S, YDesna4U, "Створ");
                                VelosityUy = new DigFunction(YDesna4S, YDesna4V, "Створ");
                            } // Створ 4
                            else
                            if (testID == 13)    // Створ 5
                            {
                                //double[] YDesna4 = { 28, 29, 39, 43.5, 48.5, 52.0, 56.0, 63.5, 69.5, 74.0, 78.5, 83.5, 89.0, 93.0, 99.5, 104.5, 113, 120.0, 127, 130.0, 134, 139.5, 146.5, 151, 155, 160, 161 };
                                //double[] HDesna4 = { -1, 0, 0.55, 0.8, 1.45, 1.5, 2.2, 2.8, 3.15, 3.6, 4.0, 4.0, 4.25, 4.6, 4.7, 5.0, 5.4, 6.6, 6.05, 5.95, 6.1, 6.0, 5.6, 3.0, 1.2, 0, -1 };

                                double[] YDesna5 = { -1, 0, 4, 6, 8, 10.5, 13, 17, 19.5, 25.5, 30, 35, 39.5, 45.5, 51, 56, 58, 61, 65, 73, 79.5, 86.5, 92.5, 102, 104, 111.5, 115.5, 118, 120 };
                                double[] HDesna5 = { -1, 0, 0.3, 0.8, 1.2, 1.6, 1.9, 2.2, 2.6, 3.9, 3.4, 3.2, 3.8, 4.2, 4.2, 4.6, 4.15, 4.6, 4.4, 5.2, 5, 5.2, 5.4, 6.5, 5.4, 3.9, 1.4, 0, -1 };

                                Geometry = Create(YDesna5, HDesna5);
                                // свободная поверхность
                                double WL = HDesna5.Max();
                                double[] WLy = { YDesna5[0], YDesna5[YDesna5.Length - 1] };
                                double[] WLz = { WL, WL };
                                WaterLevels = new DigFunction(WLy, WLz, "свободная поверхность");
                                // шероховатость дна
                                double ks0 = 0.1;
                                double[] Roug = { ks0, ks0 };
                                Roughness = new DigFunction(WLy, Roug, "Шероховатость дна");

                                // расход
                                
                                double[] timeArg = { 0, 1000 };
                                double[] q = { Q, Q };
                                FlowRate = new DigFunction(timeArg, q, "расход");

                                //double[] YDesna4S = { 0, 52.0, 69.5, 99.5, 120, 139.5, 160.0 };
                                //double[] YDesna4U = { 0, 0.4, 0.4, 0.45, 0.5, 0.47, 0 };
                                //double[] YDesna4V = { 0, 0.02, 0.06, 0.04, 0.05, 0.04, 0 };

                                double[] YDesna5S = { 0, 13, 25.5, 35, 58, 79.5, 102, 120 };
                                double[] YDesna5U = { 0, 0.2275, 0.3, 0.405, 0.535, 0.59, 0.5625, 0 };
                                double[] YDesna5V = { 0, 0.0175, 0.0275, 0.0225, 0.03, 0.015, 0, 0 };

                                VelosityUx = new DigFunction(YDesna5S, YDesna5U, "Створ");
                                VelosityUy = new DigFunction(YDesna5S, YDesna5V, "Створ");
                            } // Створ 5

                        }
                        break;
                    case 14: // Каверна для Гиа, У. Гиа, К. Н.
                    case 15:
                    case 16:
                        {
                            turbViscType = ETurbViscType.EddyViscosityConst;
                            // создание и чтение свойств задачи                
                            RSCrossParams p = new RSCrossParams()
                            {
                                J = 0.0000274,
                                turbViscTypeA = turbViscType,
                                turbViscTypeB = turbViscType,
                                сrossAlgebra = CrossAlgebra.TapeGauss,
                                taskVariant = TaskVariant.WaterLevelFun,
                                typeMeshGenerator = StripGenMeshType.StripMeshGenerator_3,
                                typeEddyViscosity = ECalkDynamicSpeed.u_start_U,
                                velocityOnWL = BCWLVelocity.AllVelocity,
                                axisSymmetry = 1,
                                CountKnots = 100,
                                CountBLKnots = 110,
                                NLine = 20,
                                SigmaTask = 0, //SigmaTask,
                                RadiusMin = 3.85,
                                ReTask = 0,
                                theta = 0.5,
                                CalkConcentration = BCalkConcentration.DirCalkConcentration
                            };
                            if (testID == 15)
                                p.ReTask = 1;
                            if (testID == 16)
                                p.ReTask = 2;
                            river_1YD.SetParams(p);
                            double WL = 1; // два дюйма
                            double H = 0.5; // ширина канал
                            double[] ox = { 0, WL };
                            double[] oy = { -H, -H };
                            Geometry = new DigFunction(ox, oy, "Дно");
                            // свободная поверхность
                            double[] WLy = { 0, WL };
                            double[] WLz = { H, H };
                            WaterLevels = new DigFunction(WLy, WLz, "свободная поверхность");
                            // шероховатость дна
                            double ks0 = 0.1;
                            double[] Roug = { ks0, ks0 };
                            Roughness = new DigFunction(WLy, Roug, "Шероховатость дна");

                            // расход
                            double Q = 0.1; ;
                            double[] timeArg = { 0, 1000 };
                            double[] q = { Q, Q };
                            FlowRate = new DigFunction(timeArg, q, "расход");
                            double[] Y29 = { 0, WL };
                            double[] U29 = { 0, 0 };
                            VelosityUx = new DigFunction(Y29, U29, "Створ U29");
                            double[] V29 = { 1.0, 1.0 };
                            VelosityUy = new DigFunction(Y29, V29, "Створ V29");
                        }
                        break;
                    case 17: // Парабола Розовского
                    case 18:
                    case 19: // Парабола Розовского плоский поток
                    case 20:
                        {
                            if (testID == 17 || testID == 1)
                                turbViscType = ETurbViscType.Karaushev1977;
                            else
                                turbViscType = ETurbViscType.EddyViscosityConst;
                            // создание и чтение свойств задачи                
                            RSCrossParams p = new RSCrossParams()
                            {
                                J = 0.0000274,
                                turbViscTypeA = turbViscType,
                                turbViscTypeB = turbViscType,
                                сrossAlgebra = CrossAlgebra.TapeGauss,
                                taskVariant = TaskVariant.WaterLevelFun,
                                typeEddyViscosity = ECalkDynamicSpeed.u_start_U,
                                velocityOnWL = BCWLVelocity.AllVelocity,
                                axisSymmetry = 1,
                                CountKnots = 300,
                                CountBLKnots = 350,
                                NLine = 20,
                                SigmaTask = 1, //SigmaTask,
                                RadiusMin = 3.85,
                                ReTask = 0,
                                theta = 0.5,
                                mu_const = 0.01,
                                CalkConcentration = BCalkConcentration.DirCalkConcentration
                            };
                            if (testID == 17 || testID == 19)
                                p.ReTask = 0;
                            else
                                p.ReTask = 1;
                            double WL = 0.140;
                            double Ux = 0.5;
                            double Uy = 0.1;
                            if (testID == 17 || testID == 18)
                            {
                                WL = 0.140;
                                SigmaTask = 1;
                            }
                            else
                            {
                                WL = 0.2;
                                SigmaTask = 0;
                            }
                            river_1YD.SetParams(p);
                            int Ny = 20;
                            double[] xx = null;
                            double[] yy = null;
                            //// Дно створа 15 Розовского
                            //Geometry = new DigFunction(xx, yy, "Дно створа 15");
                            if (testID == 17 || testID == 18)
                                Geometry = new FunctionСhannelRose();
                            else
                                Geometry = new FunctionСhannel(Ny, 0.5, 1.0);

                            Geometry.GetFunctionData(ref xx, ref yy, Ny);
                            // свободная поверхность
                            double[] WLy = { xx[0], xx[xx.Length - 1] };
                            double[] WLz = { WL, WL };
                            WaterLevels = new DigFunction(WLy, WLz, "свободная поверхность");
                            // шероховатость дна
                            double ks0 = 0.1;
                            double[] Roug = { ks0, ks0 };
                            Roughness = new DigFunction(WLy, Roug, "Шероховатость дна");

                            // расход
                            double Q = 0.1; ;
                            double[] timeArg = { 0, 1000 };
                            double[] q = { Q, Q };
                            FlowRate = new DigFunction(timeArg, q, "расход");
                            if (testID == 17 || testID == 18)
                            {
                                // Створ 15
                                double[] Y_U15 = { 0, 0.1, 0.3, 0.55, 0.8, 1.05, 1.2, 1.5, 1.6 };
                                double[] U15 = { 0, 0.243, 0.365, 0.38, 0.38, 0.365, 0.345, 0.186, 0 };
                                VelosityUx = new DigFunction(Y_U15, U15, "Створ");
                                double[] Y_V15 = { 0, 0.55, 0.8, 1.05, 1.2, 1.6 };
                                double[] V15 = { 0, 0.03, 0.028, 0.028, 0.038, 0 };
                                VelosityUy = new DigFunction(Y_V15, V15, "Створ");
                            }
                            else
                            {
                                double[] U26 = null;
                                double[] V26 = null;
                                MEM.VAlloc(Ny, Ux, ref U26);
                                MEM.VAlloc(Ny, Uy, ref V26);
                                VelosityUx = new DigFunction(xx, U26, "Створ");
                                VelosityUy = new DigFunction(xx, V26, "Створ");
                            }
                        }
                        break;

                }
                IDigFunction[] crossFunctions = new IDigFunction[6]
                {
                        Geometry, WaterLevels, FlowRate, VelosityUx, VelosityUy, Roughness
                };
                river_1YD.LoadData(crossFunctions);
            }
        }
    }
}
