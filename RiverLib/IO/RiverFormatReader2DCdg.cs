//---------------------------------------------------------------------------
//                   ПРОЕКТ  "Русловые процессы"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 22.04.2021 Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 04.01.2024 Потапов И.И.
//              добавлены( форматы cdg, mrf, bed )
//---------------------------------------------------------------------------
namespace RiverLib.IO
{
    using System;
    using System.IO;
    using System.Collections.Generic;

    using MemLogLib;
    using CommonLib;
    using CommonLib.IO;
    using RiverLib.River2D;
    using RiverLib.River2D.RiverMesh;
    using GeometryLib;

    [Serializable]
    public class RiverFormatReader2DCdg : ATaskFormat<IRiver>
    {
        /// <summary>
        /// Поддержка внешних форматов загрузки
        /// </summary>
        public override bool SupportImport { get; }
        /// <summary>
        /// Поддержка внешних форматов сохранения
        /// </summary>
        public override bool SupportExport { get; }

        const string Ext_cdg = ".cdg";
        const string Ext_mrf = ".mrf";
        const string Ext_bed = ".bed";
        public RiverFormatReader2DCdg()
        {
            extString = new List<string>() { Ext_cdg, Ext_mrf, Ext_bed };
            SupportImport = true;
            SupportExport = true;
        }
        public override void Read(string filename, ref IRiver river, uint testID = 0)
        {
            // расширение файла
            string FileEXT = Path.GetExtension(filename).ToLower();
            switch (FileEXT)
            {
                case Ext_cdg:
                    Import_cdg(filename, ref river);
                    break;
                case Ext_mrf:
                    Import_mrf(filename, ref river);
                    break;
                case Ext_bed:
                    Import_bed(filename, ref river);
                    break;
            }
        }
        /// <summary>
        /// Сохраняем сетку на диск.
        /// </summary>
        /// <param name="mesh">Экземпляр интерфейса<see cref="IMesh" /> interface.</param>
        /// <param name="filename">Путь к файлу для сохранения.</param>
        public override void Write(IRiver river, string filename)
        {
            // расширение файла
            string FileEXT = Path.GetExtension(filename).ToLower();
            switch (FileEXT)
            {
                case Ext_cdg:
                    Write_cdg(river, filename);
                    break;
                case Ext_mrf:
                    Write_mrf(river, filename);
                    break;
                case Ext_bed:
                    Write_bed(river, filename);
                    break;
            }
        }
        public void Write_cdg(IRiver river, string filename)
        {
            River2D river2D = river as River2D;
            if (river2D == null)
                throw new NotSupportedException("Не возможно сохранить выбранный формат задачи river2D == null");
            try
            {
                using (StreamWriter file = new StreamWriter(filename))
                {
                    file.WriteLine(" RVTransient analysis = 1");
                    file.WriteLine(" Mesh type = 1");
                    file.WriteLine(" Number of Time Steps = 1");
                    file.WriteLine(" Delta t Acceleration Factor = " + river2D.dtTrend);
                    file.WriteLine(" Time = " + river2D.time.ToString());
                    file.WriteLine(" Delta t = " + river2D.dtime.ToString());
                    file.WriteLine(" Theta = " + river2D.theta.ToString());
                    file.WriteLine(" UW = " + river2D.UpWindCoeff.ToString());
                    file.WriteLine(" Eddy Viscosity Bed Shear Parameter = " + river2D.turbulentVisCoeff.ToString());
                    file.WriteLine(" Latitude  = " + river2D.latitudeArea.ToString());
                    file.WriteLine(" Groundwater Storativity = " + river2D.droundWaterCoeff.ToString());
                    file.WriteLine(" Diffusive wave Solution = 0 zero for fully dynamic only");
                    file.WriteLine(" UW Jacobian terms included = 0  zero for not included");
                    file.WriteLine(" Plot Code = 2 zero for xsec one for contour two for velocity and three for threeD");
                    file.WriteLine(" RVTransient Boundary Condition = 0  zero for Steady BCs");
                    file.WriteLine(" Maximum Number of Iterations = 9");
                    file.WriteLine(" Small Depths Occur = 1 zero for no small depth calculations");
                    file.WriteLine(" Jacobian Terms included = 1 zero for not included");
                    file.WriteLine(" Eddy Viscosity Constant = 0");
                    file.WriteLine(" Minimum Depth for Groundwater Flow Calculation = " + river2D.H_minGroundWater.ToString());
                    file.WriteLine(" Eddy Viscosity Horizontal Shear Paramter = 0");
                    file.WriteLine(" Groundwater Transmissivity = " + river2D.filtrСoeff.ToString());
                    file.WriteLine(" Dimensions = 2");
                    file.WriteLine(" Number of Variables = 3");
                    file.WriteLine(" [K] governing equation numbers");
                    for (int i = 0; i < 3; i++)
                    {
                        for (int ii = 0; ii < 4; ii++)
                            file.Write(" 1");
                        file.WriteLine();
                    }
                    file.WriteLine(" Number of Parameters = 3");
                    file.WriteLine(" Number of Boundary Parameters = 7");
                    file.WriteLine(" Number of Nodes = " + river2D.mesh.CountKnots.ToString());
                    file.WriteLine(" Number of Elements = " + river2D.mesh.CountElements.ToString());
                    file.WriteLine(" Number of  Boundary Elements = " + river2D.mesh.CountBoundElements.ToString());
                    file.WriteLine(" Number of  Boundary Segments = " + river2D.mesh.CountSegment.ToString());

                    file.WriteLine("\n\n Node Information ");
                    file.WriteLine("\n Node #, Coordinates, Parameters, Variables \n");

                    // запись узловых полей
                    for (int i = 0; i < river2D.mesh.CountKnots; i++)
                        file.WriteLine(river2D.mesh.nodes[i].ToStringCDG());
                    file.WriteLine("\n Element Information");
                    file.WriteLine("\n Element #, vtype, gtype, CountNodes\n\n");
                    // запись КЭ 
                    for (int i = 0; i < river2D.mesh.CountElements; i++)
                        file.WriteLine(river2D.mesh.AreaElems[i].ToStringCDG());
                    file.WriteLine("\n Boundary Element #, vtype, gtype, CountNodes, boundary condition codes\n");
                    // запись ГКЭ 
                    for (int i = 0; i < river2D.mesh.CountBoundElements; i++)
                        file.WriteLine(river2D.mesh.BoundElems[i].ToStringCDG());
                    file.WriteLine("\n Boundary Seg #,Boundary type,stage,QT,start node #,end node #\n\n");
                    // запись сегментов
                    for (int i = 0; i < river2D.mesh.CountSegment; i++)
                        file.WriteLine(river2D.mesh.boundSegment[i].ToStringCDG());
                    file.WriteLine("\nno more breakline segments.\n\n");

                    file.Close();

                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Info(ex.Message);
            }
            return;
        }
        public void Write_bed(IRiver river, string filename)
        {
            River2D river2D = river as River2D;
            if (river2D == null)
                throw new NotSupportedException("Не возможно сохранить выбранный формат задачи river2D == null");
            try
            {
                using (StreamWriter file = new StreamWriter(filename))
                {
                    // запись узловых полей
                    for (int i = 0; i < river2D.mesh.CountKnots; i++)
                        file.WriteLine(river2D.mesh.nodes[i].ToStringBED());
                    file.WriteLine("\n no more nodes.");
                    file.WriteLine("\n no more breakline segments.\n\n");
                    // запись ГКЭ 
                    for (int i = 0; i < river2D.mesh.CountBoundElements; i++)
                        file.WriteLine(river2D.mesh.BoundElems[i].ToStringBED());
                    file.WriteLine("\nno more boundary segments.\n\n");
                    file.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Info(ex.Message);
            }
            return;
        }
        public void Write_mrf(IRiver river, string filename)
        {
            River2D river2D = river as River2D;
            if (river2D == null)
                throw new NotSupportedException("Не возможно сохранить выбранный формат задачи river2D == null");
                try
                {
                    using (StreamWriter file = new StreamWriter(filename))
                    {
                        file.WriteLine("Версия формата ver_mdf = 1.0");
                        file.WriteLine("Коэффициент ускорения шага по времени: dtTrend = {0}", river2D.dtTrend);
                        file.WriteLine("Начальное время расчета time = {0}", river2D.time);
                        file.WriteLine("Шаг по времени dtime = {0}", river2D.dtime);
                        file.WriteLine("Параметр неявности расчетной схемы по времени theta = {0}", river2D.theta);
                        file.WriteLine("Параметр веса SUPG UpWindCoeff = {0}", river2D.UpWindCoeff);
                        file.WriteLine("Параметр вихревой вязкости turbulentVisCoeff = {0}", river2D.turbulentVisCoeff);
                        file.WriteLine("Широта в градусах latitudeArea = {0}", river2D.latitudeArea);
                        // русло
                        file.WriteLine("Параметр насыщаемость русла droundWaterCoeff = {0}", river2D.droundWaterCoeff);
                        file.WriteLine("Минимальная глубина для расчета расхода грунтовых вод H_minGroundWater = {0}", river2D.H_minGroundWater);
                        file.WriteLine("Коэффициент фильтрации подземных вод filtrСoeff = {0}", river2D.filtrСoeff);
                        // размерность
                        file.WriteLine("Количество узлов CountKnots = {0}", river2D.mesh.CountKnots);
                        file.WriteLine("Количество КЭ CountElements = {0}", river2D.mesh.CountElements);
                        file.WriteLine("Количество ГКЭ CountBoundElements = {0}", river2D.mesh.CountBoundElements);
                        file.WriteLine("Количество сегментов активных границ CountSegment = {0}", river2D.mesh.CountSegment);
                        // запись узловых полей
                        for (int i = 0; i < river2D.mesh.CountKnots; i++)
                            file.WriteLine(river2D.mesh.nodes[i].ToString());
                        // запись КЭ 
                        for (int i = 0; i < river2D.mesh.CountElements; i++)
                            file.WriteLine(river2D.mesh.AreaElems[i].ToString());
                        // запись ГКЭ 
                        for (int i = 0; i < river2D.mesh.CountBoundElements; i++)
                            file.WriteLine(river2D.mesh.BoundElems[i].ToString());
                        // запись сегментов
                        for (int i = 0; i < river2D.mesh.CountSegment; i++)
                            file.WriteLine(river2D.mesh.boundSegment[i].ToString());
                        file.Close();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.Info(ex.Message);
                }
                return;
        }
        /// <summary>
        /// Прочитать файл, мой речной формат данных
        /// </summary>
        /// <param name="filename">Путь к файлу для чтения..</param>
        /// <returns>Экземпляр интерфейса<see cref="IMesh" /> interface.</returns>
        public void Import_mrf(string filename, ref IRiver river)
        {
            River2D river2D = river as River2D;
            if (river2D == null)
                throw new NotSupportedException("Не возможно загрузить выбранный объект задачи river2D == null");
            if (IsSupported(filename) == true)
            {
                river2D = new River2D();
                try
                {
                    using (StreamReader file = new StreamReader(filename))
                    {
                        int i;
                        var value = GetDouble(file);   // Версия
                        river2D.dtTrend = GetDouble(file);
                        river2D.time = GetDouble(file);
                        river2D.dtime = GetDouble(file);
                        river2D.theta = GetDouble(file);
                        river2D.UpWindCoeff = GetDouble(file);
                        river2D.turbulentVisCoeff = GetDouble(file);
                        river2D.latitudeArea = GetDouble(file);
                        river2D.droundWaterCoeff = GetDouble(file);
                        river2D.H_minGroundWater = GetDouble(file);
                        river2D.filtrСoeff = GetDouble(file);
                        int Count = GetInt(file);
                        int CountElements = GetInt(file);
                        int CountBoundElements = GetInt(file);
                        int CountBoundSegments = GetInt(file);
                        if (river2D.mesh == null)
                            river2D.mesh = new TriRiverMesh();
                        // выделение памяти под узловые поля
                        MEM.Alloc(Count, ref river2D.mesh.nodes);
                        // чтение узловых полей
                        for (i = 0; i < Count; i++)
                        {
                            string[] lines = GetLines(file, 8);
                            if (lines == null) break;
                            river2D.mesh.nodes[i] = RiverNode.Parse(lines, i);
                        }
                        // выделение памяти под КЭ и поля
                        MEM.Alloc(CountElements, ref river2D.mesh.AreaElems);
                        // чтение КЭ полей
                        for (i = 0; i < CountElements; i++)
                        {
                            river2D.mesh.AreaElems[i] = new TriElementRiver();
                            string[] lines = GetLines(file, 3);
                            river2D.mesh.AreaElems[i] = TriElementRiver.Parse(lines);
                        }
                        // выделение памяти под граничные КЭ и поля
                        MEM.Alloc(CountBoundElements, ref river2D.mesh.BoundElems);
                        for (i = 0; i < CountBoundElements; i++)
                        {
                            string[] lines = GetLines(file,7);
                            if (lines == null) break;
                            river2D.mesh.BoundElems[i] = BoundElementRiver.Parse(lines);
                        }
                        // выделение памяти под граничные сегменты
                        MEM.Alloc(CountBoundSegments, ref river2D.mesh.boundSegment);
                        for (i = 0; i < CountBoundSegments; i++)
                        {
                            river2D.mesh.boundSegment[i] = new BoundSegmentRiver();
                            string[] lines = GetLines(file, 6);
                            river2D.mesh.boundSegment[i] = BoundSegmentRiver.Parse(lines);
                        }
                        file.Close();
                    }
                    // Перенумерация
                    river2D.mesh.RenumberingID();
                    // Настройка граничных условий и ссылок в КЭ
                    // цикл узлам граничных КЭ 
                    for (int i = 0; i < river2D.mesh.BoundElems.Length; i++)
                    {
                        river2D.mesh.nodes[river2D.mesh.BoundElems[i].Vertex1].fxc = FixedFlag.fixednode;
                        river2D.mesh.nodes[river2D.mesh.BoundElems[i].Vertex2].fxc = FixedFlag.fixednode;
                    }
                    Logger.Instance.Info(" Number of unknowns = " + (river2D.mesh.CountKnots * 3).ToString());
                    river2D.Set(river2D.mesh);
                    // обновление скоростей
                    river2D.RollbackToOldValues();
                    river2D.UpdateVelocities();
                    river = river2D;
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
        /// Речной формат данных
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="river"></param>
        public void Import_bed(string filename, ref IRiver river)
        {

        }
        /// <summary>
        /// Прочитать файл, содержащий речную задачу из старого River2D формата данных cdg.
        /// </summary>
        /// <param name="filename">Путь к файлу для чтения..</param>
        /// <returns>Экземпляр интерфейса<see cref="IMesh" /> interface.</returns>
        public void Import_cdg(string filename, ref IRiver river)
        {
            River2D river2D = river as River2D;
            if (river2D == null)
                throw new NotSupportedException("Не возможно pfuhepbnm выбранный формат задачи river2D == null");
            if (IsSupported(filename) == true)
            {
                river2D = new River2D();
                try
                {
                    using (StreamReader file = new StreamReader(filename))
                    {
                        object value = GetDouble(file);   // не нужная переменная
                        var value1 = GetInt(file);          // не нужная переменная
                        var value2 = GetInt(file);          // не нужная переменная
                        river2D.dtTrend = GetDouble(file);
                        river2D.time = GetDouble(file);
                        river2D.dtime = GetDouble(file);
                        river2D.theta = GetDouble(file);
                        river2D.UpWindCoeff = GetDouble(file);
                        river2D.turbulentVisCoeff = GetDouble(file);
                        river2D.latitudeArea = GetDouble(file);
                        river2D.droundWaterCoeff = GetDouble(file);
                        var value3 = GetInt(file);          // не нужная переменная
                        var value4 = GetInt(file);          // не нужная переменная
                        var value5 = GetInt(file);          // не нужная переменная
                        var value6 = GetInt(file);          // не нужная переменная
                        var value7 = GetInt(file);          // не нужная переменная
                        var value8 = GetInt(file);          // не нужная переменная
                        var value9 = GetInt(file);          // не нужная переменная
                        var value10 = GetInt(file);          // не нужная переменная
                        river2D.H_minGroundWater = GetDouble(file);
                        var value11 = GetDouble(file);          // не нужная переменная
                        river2D.filtrСoeff = GetDouble(file);
                        var value12 = GetInt(file);  // не нужная переменная
                        var value13 = GetInt(file);  // не нужная переменная
                        int[] Keqns = new int[12];
                        GetLine(file);
                        int i = 0;
                        for (int k = 0; k < 3; k++)
                        {
                            string line = file.ReadLine().Trim();
                            string[] sline = line.Split(' ', '\t');
                            for (int m = 0; m < sline.Length; m++)
                                if (sline[m] != "")
                                    Keqns[i++] = int.Parse(sline[0].Trim());

                        }
                        var value14 = GetInt(file);  // не нужная переменная
                        var value15 = GetInt(file);  // не нужная переменная
                        int Count = GetInt(file);
                        int CountElements = GetInt(file);
                        int CountBoundElements = GetInt(file);
                        int CountBoundSegments = GetInt(file);
                        if (river2D.mesh == null)
                            river2D.mesh = new TriRiverMesh();
                        // выделение памяти под узловые поля
                        river2D.mesh.nodes = new RiverNode[Count];
                        // чтение узловых полей
                        for (i = 0; i < Count; i++)
                        {
                            river2D.mesh.nodes[i] = new RiverNode();
                            string[] lines = GetLines(file, 8);
                            if (lines == null)
                                break;
                            int idx = 0;
                            river2D.mesh.nodes[i].n = int.Parse(lines[idx++].Trim());
                            river2D.mesh.nodes[i].i = i;
                            string s = lines[idx].Trim();
                            if (s == "x" || s == "s")
                            {
                                //if (s == "x")
                                //    mesh.nodes[i].fxc = 1;  // x  фиксированный узел
                                //else
                                //    mesh.nodes[i].fxc = 2;  // s sliding - скользящий узел (по границе/ или линии)
                                river2D.mesh.nodes[i].fxc = 0;
                                idx++;
                            }
                            else
                                river2D.mesh.nodes[i].fxc = 0; // плавающий узел
                            river2D.mesh.nodes[i].X = double.Parse(lines[idx++].Trim(), MEM.formatter);
                            river2D.mesh.nodes[i].Y = double.Parse(lines[idx++].Trim(), MEM.formatter);
                            river2D.mesh.nodes[i].zeta = double.Parse(lines[idx++].Trim(), MEM.formatter);
                            //river2D.mesh.nodes[i].zeta0 = river2D.mesh.nodes[i].zeta;
                            river2D.mesh.nodes[i].ks = double.Parse(lines[idx++].Trim(), MEM.formatter);
                            idx++;
                            //river2D.mesh.nodes[i].curvature = double.Parse(lines[idx++].Trim(), formatter);
                            river2D.mesh.nodes[i].h = double.Parse(lines[idx++].Trim(), MEM.formatter);
                            river2D.mesh.nodes[i].qx = double.Parse(lines[idx++].Trim(), MEM.formatter);
                            river2D.mesh.nodes[i].qy = double.Parse(lines[idx++].Trim(), MEM.formatter);
                        }
                        // выделение памяти под КЭ и поля
                        river2D.mesh.AreaElems = new TriElementRiver[CountElements];
                        // чтение КЭ полей
                        for (i = 0; i < CountElements; i++)
                        {
                            river2D.mesh.AreaElems[i] = new TriElementRiver();
                            string[] lines = GetLines(file, 9);
                            river2D.mesh.AreaElems[i].ID = int.Parse(lines[0].Trim());
                            int typeFFV = int.Parse(lines[1].Trim());  // не нужная переменная
                            int typeFFL = int.Parse(lines[2].Trim());  // не нужная переменная
                                                                       // сдвиг на 1 нумерация узлов идет с нуля
                            river2D.mesh.AreaElems[i].Vertex1 = uint.Parse(lines[3].Trim());
                            river2D.mesh.AreaElems[i].Vertex2 = uint.Parse(lines[4].Trim());
                            river2D.mesh.AreaElems[i].Vertex3 = uint.Parse(lines[5].Trim());
                        }
                        // выделение памяти под граничные КЭ и поля
                        river2D.mesh.BoundElems = new BoundElementRiver[CountBoundElements];
                        for (i = 0; i < CountBoundElements; i++)
                        {
                            river2D.mesh.BoundElems[i] = new BoundElementRiver();
                            string[] lines = GetLines(file, 15);
                            river2D.mesh.BoundElems[i].ID = int.Parse(lines[0].Trim());
                            int typeBFV = int.Parse(lines[1].Trim());   // не нужная переменная
                            int typeBFL = int.Parse(lines[2].Trim());   // не нужная переменная

                            river2D.mesh.BoundElems[i].Vertex1 = uint.Parse(lines[3].Trim());
                            river2D.mesh.BoundElems[i].Vertex2 = uint.Parse(lines[4].Trim());
                            river2D.mesh.BoundElems[i].Eta = double.Parse(lines[5].Trim(), MEM.formatter);
                            river2D.mesh.BoundElems[i].Qn = double.Parse(lines[6].Trim(), MEM.formatter);
                            river2D.mesh.BoundElems[i].Qt = double.Parse(lines[7].Trim(), MEM.formatter);
                            //river2D.mesh.BoundElems[i].H = double.Parse(lines[8].Trim(), formatter);
                            //river2D.mesh.BoundElems[i].Zeta = double.Parse(lines[9].Trim(), formatter);
                            //river2D.mesh.BoundElems[i].Length = double.Parse(lines[10].Trim(), formatter);
                           // river2D.mesh.BoundElems[i].p[6] = double.Parse(lines[11].Trim(), formatter);
                            river2D.mesh.BoundElems[i].boundCondType = int.Parse(lines[12].Trim());
                            //river2D.mesh.BoundElems[i].bcs[1] = int.Parse(lines[13].Trim());
                            //river2D.mesh.BoundElems[i].bcs[2] = int.Parse(lines[14].Trim());
                        }
                        // заголовок    
                        string[] headlines = GetLines(file, 6);
                        // выделение памяти под граничные сегменты
                        river2D.mesh.boundSegment = new BoundSegmentRiver[CountBoundSegments];
                        for (i = 0; i < CountBoundSegments; i++)
                        {
                            river2D.mesh.boundSegment[i] = new BoundSegmentRiver();
                            string[] lines = GetLines(file, 6);
                            river2D.mesh.boundSegment[i].ID = int.Parse(lines[0].Trim());
                            river2D.mesh.boundSegment[i].boundCondType = int.Parse(lines[1].Trim());
                            river2D.mesh.boundSegment[i].Hn = double.Parse(lines[2].Trim(), MEM.formatter);
                            river2D.mesh.boundSegment[i].Qn = double.Parse(lines[3].Trim(), MEM.formatter);
                            river2D.mesh.boundSegment[i].startnode = int.Parse(lines[4].Trim());
                            river2D.mesh.boundSegment[i].endnode = int.Parse(lines[5].Trim());
                        }
                        file.Close();
                    }
                    // Перенумерация
                    river2D.mesh.RenumberingID();
                    // Настройка граничных условий и ссылок в КЭ
                    // цикл узлам граничных КЭ 
                    for (int i = 0; i < river2D.mesh.BoundElems.Length; i++)
                    {
                        river2D.mesh.nodes[river2D.mesh.BoundElems[i].Vertex1].fxc = FixedFlag.fixednode;
                        river2D.mesh.nodes[river2D.mesh.BoundElems[i].Vertex2].fxc = FixedFlag.fixednode;
                    }
                    Logger.Instance.Info(" Number of unknowns = " + (river2D.mesh.CountKnots * 3).ToString());
                    river2D.Set(river2D.mesh);
                    // обновление скоростей
                    river2D.RollbackToOldValues();
                    river2D.UpdateVelocities();
                    river = river2D;
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
            List<string> strings = new List<string>();
            strings.Add("Задача не имеет тестовых");
            return strings;
        }
    }
}
