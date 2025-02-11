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
    using System.Collections.Generic;

    using MemLogLib;
    using CommonLib;
    using CommonLib.IO;
    using CommonLib.Function;
    using NPRiverLib.APRiver1YD;
    using GeometryLib;
    using NPRiverLib.APRiver1YD.Params;
    using CommonLib.EddyViscosity;
    using CommonLib.Physics;

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
                            {
                                // создание и чтение свойств задачи                
                                RSCrossParams p = new RSCrossParams()
                                {
                                    J = 0.006,
                                    turbViscTypeA = ETurbViscType.Leo_C_van_Rijn1984,
                                    turbViscTypeB = ETurbViscType.Leo_C_van_Rijn1984,
                                    сrossAlgebra = CrossAlgebra.TapeGauss,
                                    taskVariant = TaskVariant.WaterLevelFun,
                                    bcTypeVortex = BCTypeVortex.VortexAllCalk,
                                    typeEddyViscosity = ECalkDynamicSpeed.u_start_U,
                                    velocityOnWL = true,
                                    axisSymmetry = 0,
                                    CountKnots = 200,
                                    CountBLKnots = 300,
                                    NLine = 100,
                                    SigmaTask = 0,
                                    RadiusMin = 4.2,
                                    ReTask = 0,
                                    theta = 0.5
                                };
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
                                if (testID == 1) // Створ 15
                                {
                                    
                                    double[] Y_U15 = { 0, 0.1, 0.3, 0.55, 0.8, 1.05, 1.2, 1.5, 1.6 };
                                    double[] U15 = { 0, 0.243, 0.365, 0.38, 0.38, 0.365, 0.345, 0.186, 0 };
                                    VelosityUx = new DigFunction(Y_U15, U15, "Створ");
                                    double[] Y_V15 = { 0, 0.55, 0.8, 1.05, 1.2, 1.6 };
                                    double[] V15 = { 0, 0.03, 0.028, 0.028, 0.038, 0 };
                                    VelosityUy = new DigFunction(Y_V15, V15, "Створ");
                                }
                                if (testID == 2) // Створ 18
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
                                if (testID == 3) // Створ 21
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
                        case 4: // Канал Вим ван Балена
                        case 5:
                        case 6:
                            {
                                // создание и чтение свойств задачи                
                                RSCrossParams p = new RSCrossParams()
                                {
                                    J = 0.006,
                                    turbViscTypeA = ETurbViscType.Leo_C_van_Rijn1984,
                                    turbViscTypeB = ETurbViscType.Leo_C_van_Rijn1984,
                                    сrossAlgebra = CrossAlgebra.TapeGauss,
                                    taskVariant = TaskVariant.WaterLevelFun,
                                    bcTypeVortex = BCTypeVortex.VortexAllCalk,
                                    typeEddyViscosity = ECalkDynamicSpeed.u_start_U,
                                    velocityOnWL = true,
                                    axisSymmetry = 0,
                                    CountKnots = 200,
                                    CountBLKnots = 300,
                                    NLine = 100,
                                    SigmaTask = 0,
                                    RadiusMin = 4.2,
                                    ReTask = 0,
                                    theta = 0.5
                                };
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
                                if (testID == 4) // Створ 29 градусов
                                {

                                }
                            }

                            break;
                        case 7: // Парабола левая
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
            list.Add("Канал Розовского, 15 створ");  // 1
            list.Add("Канал Розовского, 18 створ");
            list.Add("Канал Розовского, 21 створ");
            list.Add("Канал Вим ван Балена, створ 29 гр."); // 4
            list.Add("Канал Вим ван Балена, створ 60 гр.");
            list.Add("Канал Вим ван Балена, створ 145 гр.");
            list.Add("Прямоугольный канал (тест)");
            list.Add("Парабола (тест)");
            list.Add("Парабола левая (тест)");
            return list;
        }
    }
}
