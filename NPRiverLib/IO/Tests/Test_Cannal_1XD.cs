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
    using System.Collections.Generic;

    using MemLogLib;
    using GeometryLib;

    using CommonLib;
    using CommonLib.Function;
    using CommonLib.EddyViscosity;

    using MeshLib;
    using MeshAdapterLib;
    using MeshGeneratorsLib.SPIN;
    using MeshGeneratorsLib.StripGenerator;

    using NPRiverLib.APRiver_1XD;
    using NPRiverLib.APRiver_1XD.River2D_FVM_ke;
    using CommonLib.BedLoad;

    /// <summary>
    /// Список тестов для задачи в створе
    /// </summary>
    public class Test_Cannal_1XD
    {
        /// <summary>
        /// Названия тестов для задачи в створе
        /// </summary>
        /// <returns></returns>
        public static List<string> GetTestsName()
        {
            List<string> list = new List<string>();
            list.Add("Загрузка по умолчанию");          // 0
            list.Add("Канал с выбранной нативной геометрией"); // 1

            list.Add("Прямой канал - шель (Стокс)");        // 2
            list.Add("Прямой канал - шель (Рейнольдс)");    // 3

            list.Add("Канал с уступом (Стокс)");     // 4
            list.Add("Канал с уступом (Рейнольдс)"); // 5

            list.Add("Канал с плавным уступом (Стокс)");     // 6
            list.Add("Канал с плавным уступом (Рейнольдс)"); // 7

            list.Add("Взвешенная струя (Стокс)");      // 8
            list.Add("Взвешенная струя (Рейнольдс)");  // 9

            list.Add("Канал с дыркой (нс Стокс)");      // 10
            list.Add("Канал с дыркой (ст Рейнольдс)");  // 11

            list.Add("Прямой канал - русло (Стокс)");        // 12
            list.Add("Прямой канал - русло (Рейнольдс)");    // 13
            return list;
        }
        public static void GetTest(ref IRiver river, uint testTaskID = 0)
        {
            int[][] riverGates = null;
            TriFEMRiver_1XD river_1XD = river as TriFEMRiver_1XD;
            if (river_1XD == null)
                throw new NotSupportedException("Не возможно загрузить выбранный объект задачи river_1YD == null");
            GetTest(testTaskID, out FEMParams_1XD p, out IDigFunction[] bcPhi,
                                out IDigFunction[] BCVelosity, out IMesh mesh, out riverGates);
            river_1XD.bcPhi = bcPhi;
            // Заглушка по скоростям
            river_1XD.SetParams(p);
            river_1XD.Set(mesh, null);
            river_1XD.LoadData(BCVelosity);
            river = river_1XD;
        }

        public static void GetTest(uint testTaskID, 
            out FEMParams_1XD p,
            out IDigFunction[] bcPhi,
            out IDigFunction[] BCVelosity,
            out IMesh mesh,
            out int[][] riverGates
            )
        {
            riverGates = null;
            // создание и чтение свойств задачи                
            p = new FEMParams_1XD()
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
                bcTypeOnWL = RoofCondition.adhesion,
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
                turbViscType = ETurbViscType.EddyViscosityConst,
                // Флаг для расчета взвешенных наносов 
                CalkConcentration = BCalkConcentration.NeCalkConcentration
            };
            if (testTaskID % 2 == 1) // стационарные у-я Рейнольдса 
                p.ReTask = 1;
            
            bcPhi = new DigFunctionPolynom[2];
            TriMesh triMesh = null;

            switch (testTaskID)
            {
                // Прямой канал- шель
                case 2:
                case 3:
                    try
                    {
                        CreateMesh.GetRectangleTriMesh(ref triMesh, p.FE_X, p.FE_Y, p.Lx, p.Ly, 1);
                        
                        double y0 = 0, Phi_0 = 0;
                        double y1 = p.Ly / 3, Phi_H3 = 14.0 / 81 * p.V2_inlet * p.Ly;
                        double y2 = 2 * p.Ly / 3, Phi_2H3 = 40.0 / 81 * p.V2_inlet * p.Ly;
                        double y3 = p.Ly, Phi_H = 2.0 / 3.0 * p.V2_inlet * p.Ly;
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
                case 4: // Канал с уступом
                case 5:
                    try
                    {
                        p.Len1 = 1;
                        p.Len2 = 1;
                        p.Wen1 = 4;
                        p.Wen2 = 36;
                        p.V2_inlet = 1;
                        double H1 = p.Len1;
                        double H2 = p.Len2;
                        double H = H1 + H2;
                        double L1 = p.Wen1;
                        double L2 = p.Wen2;
                        double L = L1 + L2;
                        double U0 = p.V2_inlet;
                        double U1 = U0 * H2 / H;

                        double y0 = H1;
                        double Phi_0 = 0;

                        double y1 = H1 + H2 / 3;
                        double Phi_H3 = 14.0 / 81 * U0 * H2;

                        double y2 = H1 + 2 * H2 / 3;
                        double Phi_2H3 = 40.0 / 81 * U0 * H2;

                        double y3 = H;
                        double Phi_H = 2.0 / 3.0 * U0 * H2;
                        // функция тока на входе
                        bcPhi[0] = new DigFunctionPolynom(y0, Phi_0, y1, Phi_H3, y2, Phi_2H3, y3, Phi_H);

                        y0 = 0;
                        Phi_0 = 0;

                        y1 = H / 3;
                        Phi_H3 = 14.0 / 81 * U1 * H;

                        y2 = 2.0 * H / 3;
                        Phi_2H3 = 40.0 / 81 * U1 * H;

                        y3 = H;
                        Phi_H = 2.0 / 3.0 * U1 * H;

                        // функция тока на выходе
                        bcPhi[1] = new DigFunctionPolynom(y0, Phi_0, y1, Phi_H3, y2, Phi_2H3, y3, Phi_H);

                        StepGenerator sg = new StepGenerator();
                        int Nx1 = 20;
                        int Nx2 = 120;
                        int Ny1 = 20;
                        int Ny2 = 20;
                        sg.Set(Nx1, Nx2, Ny1, Ny2, H1, H2, L1, L2);

                        sg.GetMesh(ref triMesh);
                        
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    break;
                case 6: // Канал с плавнм уступом
                case 7:
                    try
                    {
                        p.Len1 = 1;
                        p.Len2 = 1;
                        p.Wen1 = 4;
                        p.Wen2 = 4;
                        p.Wen3 = 32;
                        p.V2_inlet = 1;
                        p.mu_const = 2;

                        double H1 = p.Len1;
                        double H2 = p.Len2;
                        double H = H1 + H2;
                        double L1 = p.Wen1;
                        double L2 = p.Wen2;
                        double L3 = p.Wen3;
                        double L = L1 + L2 + L3;
                        double U0 = p.V2_inlet;
                        double U1 = U0 * H2 / H;

                        double y0 = H1;
                        double Phi_0 = 0;

                        double y1 = H1 + H2 / 3;
                        double Phi_H3 = 14.0 / 81 * U0 * H2;

                        double y2 = H1 + 2 * H2 / 3;
                        double Phi_2H3 = 40.0 / 81 * U0 * H2;

                        double y3 = H;
                        double Phi_H = 2.0 / 3.0 * U0 * H2;
                        // функция тока на входе
                        bcPhi[0] = new DigFunctionPolynom(y0, Phi_0, y1, Phi_H3, y2, Phi_2H3, y3, Phi_H);

                        y0 = 0;
                        Phi_0 = 0;

                        y1 = H / 3;
                        Phi_H3 = 14.0 / 81 * U1 * H;

                        y2 = 2.0 * H / 3;
                        Phi_2H3 = 40.0 / 81 * U1 * H;

                        y3 = H;
                        Phi_H = 2.0 / 3.0 * U1 * H;
                        // функция тока на выходе
                        bcPhi[1] = new DigFunctionPolynom(y0, Phi_0, y1, Phi_H3, y2, Phi_2H3, y3, Phi_H);

                        IStripMeshGenerator sg = new CrossStripMeshGenerator();
                        sg.Option.markerArea = SimpleMarkerArea.boxCrossSection;
                        double WetBed = 0;
                        int Nx = 1000;
                        double[] xx = null;
                        double[] yy = null;
                        MEM.Alloc(Nx, ref xx);
                        MEM.Alloc(Nx, ref yy);
                        IDigFunction Geometry;
                        Geometry = new FunctionСhannelStep(Nx, L, L1, L2, 0, H1);
                        Geometry.GetFunctionData(ref xx, ref yy, Nx);
                        mesh = sg.CreateMesh(ref WetBed, ref riverGates, H, xx, yy, Nx);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    break;
                case 8: // Канал с взыешенной струей
                case 9:
                    try
                    {
                        CreateMesh.GetRectangleTriMesh(ref triMesh, p.FE_X, p.FE_Y, p.Lx, p.Ly, 1);
                        double y0 = 0, Phi_0 = 0;
                        double y1 = p.Ly / 3, Phi_H3 = 14.0 / 81 * p.V2_inlet * p.Ly;
                        double y2 = 2 * p.Ly / 3, Phi_2H3 = 40.0 / 81 * p.V2_inlet * p.Ly;
                        double y3 = p.Ly, Phi_H = 2.0 / 3.0 * p.V2_inlet * p.Ly;
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
                case 10: // Канал с дыркой
                case 11:
                    try
                    {
                        CreateMesh.GetRectangleTriMesh(ref triMesh, p.FE_X, p.FE_Y, p.Lx, p.Ly, 1);
                        double y0 = 0, Phi_0 = 0;
                        double y1 = p.Ly / 3, Phi_H3 = 14.0 / 81 * p.V2_inlet * p.Ly;
                        double y2 = 2 * p.Ly / 3, Phi_2H3 = 40.0 / 81 * p.V2_inlet * p.Ly;
                        double y3 = p.Ly, Phi_H = 2.0 / 3.0 * p.V2_inlet * p.Ly;
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
                // Прямой канал - русло
                case 12:
                case 13:
                    try
                    {
                        // Количество КЭ для давления по Х
                        p.FE_X = 280;
                        // p.Wen1 = 1.0;
                        // Глубина 2 участка
                        p.Wen2 = 100.0;
                        // Глубина 3 участка
                        //p.Wen3 = 1.0;

                        p.bcTypeOnWL = RoofCondition.slip;
                        p.CalkConcentration = BCalkConcentration.DirCalkConcentration;
                        //p.CalkConcentration = BCalkConcentration.NeCalkConcentration;

                        CreateMesh.GetRectangleTriMesh(ref triMesh, p.FE_X, p.FE_Y, p.Lx, p.Ly, 1);
                        double y0 = 0, Phi_0 = 0;
                        double y1 = p.Ly / 3, Phi_H3 = 8.0 / 81 * p.V2_inlet * p.Ly;
                        double y2 = 2 * p.Ly / 3, Phi_2H3 = 28.0 / 81 * p.V2_inlet * p.Ly;
                        double y3 = p.Ly, Phi_H = 2.0 / 3.0 * p.V2_inlet * p.Ly;
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
            }
            mesh = triMesh;
            // Заглушка по скоростям
            BCVelosity = new IDigFunction[2];
        }
    }
}
