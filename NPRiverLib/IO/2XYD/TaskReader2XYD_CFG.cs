//---------------------------------------------------------------------------
//                   ПРОЕКТ  "Русловые процессы"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 22.04.2021 Потапов И.И.
//                          формат cdg 
//---------------------------------------------------------------------------
//              кодировка : 04.01.2024 Потапов И.И.
//              добавлены( форматы mrf, bed )
//---------------------------------------------------------------------------
namespace NPRiverLib.IO
{
    using System;
    using System.IO;
    using System.Collections.Generic;

    using MemLogLib;
    using CommonLib;
    using CommonLib.IO;
    using GeometryLib;
    using NPRiverLib.APRiver2XYD.River2DSW;

    [Serializable]
    public class TaskReader2XYD_CFG : ATaskFormat<IRiver>
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
        public TaskReader2XYD_CFG()
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
            TriRiverSWE2XYD river2D = river as TriRiverSWE2XYD;
            if (river2D == null)
                throw new NotSupportedException("Не возможно сохранить выбранный формат задачи river2D == null");
            try
            {
                using (StreamWriter file = new StreamWriter(filename))
                {
                    ParamsRiver2XYD Params = (ParamsRiver2XYD)river2D.GetParams();
                    TriRiverMesh mesh = (TriRiverMesh)river2D.Mesh();

                    file.WriteLine(" RVTransient analysis = 1");
                    file.WriteLine(" Mesh type = 1");
                    file.WriteLine(" Number of Time Steps = 1");
                    file.WriteLine(" Delta t Acceleration Factor = " + Params.dtTrend);
                    file.WriteLine(" Time = " + river2D.time.ToString());
                    file.WriteLine(" Delta t = " + river2D.dtime.ToString());
                    file.WriteLine(" Theta = " + Params.theta.ToString());
                    file.WriteLine(" UW = " + Params.UpWindCoeff.ToString());
                    file.WriteLine(" Eddy Viscosity Bed Shear Parameter = " + Params.turbulentVisCoeff.ToString());
                    file.WriteLine(" Latitude  = " + Params.latitudeArea.ToString());
                    file.WriteLine(" Groundwater Storativity = " + Params.droundWaterCoeff.ToString());
                    file.WriteLine(" Diffusive wave Solution = 0 zero for fully dynamic only");
                    file.WriteLine(" UW Jacobian terms included = 0  zero for not included");
                    file.WriteLine(" Plot Code = 2 zero for xsec one for contour two for velocity and three for threeD");
                    file.WriteLine(" RVTransient Boundary Condition = 0  zero for Steady BCs");
                    file.WriteLine(" Maximum Number of Iterations = 9");
                    file.WriteLine(" Small Depths Occur = 1 zero for no small depth calculations");
                    file.WriteLine(" Jacobian Terms included = 1 zero for not included");
                    file.WriteLine(" Eddy Viscosity Constant = 0");
                    file.WriteLine(" Minimum Depth for Groundwater Flow Calculation = " + Params.H_minGroundWater.ToString());
                    file.WriteLine(" Eddy Viscosity Horizontal Shear Paramter = 0");
                    file.WriteLine(" Groundwater Transmissivity = " + Params.filtrСoeff.ToString());
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
                    file.WriteLine(" Number of Nodes = " + mesh.CountKnots.ToString());
                    file.WriteLine(" Number of Elements = " + mesh.CountElements.ToString());
                    file.WriteLine(" Number of  Boundary Elements = " + mesh.CountBoundElements.ToString());
                    file.WriteLine(" Number of  Boundary Segments = " + mesh.CountSegment.ToString());

                    file.WriteLine("\n\n Node Information ");
                    file.WriteLine("\n Node #, Coordinates, Parameters, Variables \n");

                    // запись узловых полей
                    for (int i = 0; i < mesh.CountKnots; i++)
                        file.WriteLine(mesh.nodes[i].ToStringCDG());
                    file.WriteLine("\n Element Information");
                    file.WriteLine("\n Element #, vtype, gtype, CountNodes\n\n");
                    // запись КЭ 
                    for (int i = 0; i < mesh.CountElements; i++)
                        file.WriteLine(mesh.AreaElems[i].ToStringCDG());
                    file.WriteLine("\n Boundary Element #, vtype, gtype, CountNodes, boundary condition codes\n");
                    // запись ГКЭ 
                    for (int i = 0; i < mesh.CountBoundElements; i++)
                        file.WriteLine(mesh.BoundElems[i].ToStringCDG());
                    file.WriteLine("\n Boundary Seg #,Boundary type,stage,QT,start node #,end node #\n\n");
                    // запись сегментов
                    for (int i = 0; i < mesh.CountSegment; i++)
                        file.WriteLine(mesh.boundSegment[i].ToStringCDG());
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
            TriRiverSWE2XYD river2D = river as TriRiverSWE2XYD;
            if (river2D == null)
                throw new NotSupportedException("Не возможно сохранить выбранный формат задачи river2D == null");
            try
            {
                using (StreamWriter file = new StreamWriter(filename))
                {
                    ParamsRiver2XYD Params = (ParamsRiver2XYD)river2D.GetParams();
                    TriRiverMesh mesh = (TriRiverMesh)river2D.Mesh();

                    // запись узловых полей
                    for (int i = 0; i < mesh.CountKnots; i++)
                        file.WriteLine( mesh.nodes[i].ToStringBED());
                    file.WriteLine("\n no more nodes.");
                    file.WriteLine("\n no more breakline segments.\n\n");
                    // запись ГКЭ 
                    for (int i = 0; i < mesh.CountBoundElements; i++)
                        file.WriteLine(mesh.BoundElems[i].ToStringBED());
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
            TriRiverSWE2XYD river2D = river as TriRiverSWE2XYD;
            if (river2D == null)
                throw new NotSupportedException("Не возможно сохранить выбранный формат задачи river2D == null");
                try
                {
                ParamsRiver2XYD Params = (ParamsRiver2XYD)river2D.GetParams();
                TriRiverMesh mesh = (TriRiverMesh)river2D.Mesh();
                using (StreamWriter file = new StreamWriter(filename))
                    {
                        file.WriteLine("Версия формата ver_mdf = 1.0");
                        file.WriteLine("Коэффициент ускорения шага по времени: dtTrend = {0}", Params.dtTrend);
                        file.WriteLine("Начальное время расчета time = {0}", river2D.time);
                        file.WriteLine("Шаг по времени dtime = {0}", river2D.dtime);
                        file.WriteLine("Параметр неявности расчетной схемы по времени theta = {0}", Params.theta);
                        file.WriteLine("Параметр веса SUPG UpWindCoeff = {0}", Params.UpWindCoeff);
                        file.WriteLine("Параметр вихревой вязкости turbulentVisCoeff = {0}", Params.turbulentVisCoeff);
                        file.WriteLine("Широта в градусах latitudeArea = {0}", Params.latitudeArea);
                        // русло
                        file.WriteLine("Параметр насыщаемость русла droundWaterCoeff = {0}", Params.droundWaterCoeff);
                        file.WriteLine("Минимальная глубина для расчета расхода грунтовых вод H_minGroundWater = {0}", Params.H_minGroundWater);
                        file.WriteLine("Коэффициент фильтрации подземных вод filtrСoeff = {0}", Params.filtrСoeff);
                        // размерность
                        file.WriteLine("Количество узлов CountKnots = {0}", mesh.CountKnots);
                        file.WriteLine("Количество КЭ CountElements = {0}", mesh.CountElements);
                        file.WriteLine("Количество ГКЭ CountBoundElements = {0}", mesh.CountBoundElements);
                        file.WriteLine("Количество сегментов активных границ CountSegment = {0}", mesh.CountSegment);
                        // запись узловых полей
                        for (int i = 0; i < mesh.CountKnots; i++)
                            file.WriteLine(mesh.nodes[i].ToString());
                        // запись КЭ 
                        for (int i = 0; i < mesh.CountElements; i++)
                            file.WriteLine(mesh.AreaElems[i].ToString());
                        // запись ГКЭ 
                        for (int i = 0; i < mesh.CountBoundElements; i++)
                            file.WriteLine(mesh.BoundElems[i].ToString());
                        // запись сегментов
                        for (int i = 0; i <mesh.CountSegment; i++)
                            file.WriteLine(mesh.boundSegment[i].ToString());
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
            TriRiverSWE2XYD river2D = river as TriRiverSWE2XYD;
            if (river2D == null)
                throw new NotSupportedException("Не возможно загрузить выбранный объект задачи river2D == null");
            if (IsSupported(filename) == true)
            {
                ParamsRiver2XYD Params = new ParamsRiver2XYD();
                TriRiverMesh mesh = new TriRiverMesh();
                river2D = new TriRiverSWE2XYD();
                try
                {
                    using (StreamReader file = new StreamReader(filename))
                    {
                        int i;
                        var value = GetDouble(file);   // Версия
                        Params.dtTrend = GetDouble(file);
                        Params.time = GetDouble(file);
                        Params.dtime = GetDouble(file);
                        Params.theta = GetDouble(file);
                        Params.UpWindCoeff = GetDouble(file);
                        Params.turbulentVisCoeff = GetDouble(file);
                        Params.latitudeArea = GetDouble(file);
                        Params.droundWaterCoeff = GetDouble(file);
                        Params.H_minGroundWater = GetDouble(file);
                        Params.filtrСoeff = GetDouble(file);
                        int Count = GetInt(file);
                        int CountElements = GetInt(file);
                        int CountBoundElements = GetInt(file);
                        int CountBoundSegments = GetInt(file);
                        river2D.SetParams(Params);

                        // выделение памяти под узловые поля
                        MEM.Alloc(Count, ref mesh.nodes);
                        // чтение узловых полей
                        for (i = 0; i < Count; i++)
                        {
                            string[] lines = GetLines(file, 8);
                            if (lines == null) break;
                            mesh.nodes[i] = RiverNode.Parse(lines, i);
                        }
                        // выделение памяти под КЭ и поля
                        MEM.Alloc(CountElements, ref mesh.AreaElems);
                        // чтение КЭ полей
                        for (i = 0; i < CountElements; i++)
                        {
                            mesh.AreaElems[i] = new TriElementRiver();
                            string[] lines = GetLines(file, 3);
                            mesh.AreaElems[i] = TriElementRiver.Parse(lines);
                        }
                        // выделение памяти под граничные КЭ и поля
                        MEM.Alloc(CountBoundElements, ref mesh.BoundElems);
                        for (i = 0; i < CountBoundElements; i++)
                        {
                            string[] lines = GetLines(file,7);
                            if (lines == null) break;
                            mesh.BoundElems[i] = BoundElementRiver.Parse(lines);
                        }
                        // выделение памяти под граничные сегменты
                        MEM.Alloc(CountBoundSegments, ref mesh.boundSegment);
                        for (i = 0; i < CountBoundSegments; i++)
                        {
                            mesh.boundSegment[i] = new BoundSegmentRiver();
                            string[] lines = GetLines(file, 6);
                            mesh.boundSegment[i] = BoundSegmentRiver.Parse(lines);
                        }
                        
                        file.Close();
                    }
                    // Перенумерация
                    mesh.RenumberingID();
                    // Настройка граничных условий и ссылок в КЭ
                    // цикл узлам граничных КЭ 
                    for (int i = 0; i < mesh.BoundElems.Length; i++)
                    {
                        mesh.nodes[mesh.BoundElems[i].Vertex1].fxc = FixedFlag.fixednode;
                        mesh.nodes[mesh.BoundElems[i].Vertex2].fxc = FixedFlag.fixednode;
                    }
                    Logger.Instance.Info(" Number of unknowns = " + (mesh.CountKnots * 3).ToString());
                    river2D.Set(mesh);
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
            TriRiverSWE2XYD river2D = river as TriRiverSWE2XYD;
            if (river2D == null)
                throw new NotSupportedException("Не возможно pfuhepbnm выбранный формат задачи river2D == null");
            if (IsSupported(filename) == true)
            {
                ParamsRiver2XYD Params = new ParamsRiver2XYD();
                TriRiverMesh mesh = new TriRiverMesh();
                river2D = new TriRiverSWE2XYD();
                try
                {
                    using (StreamReader file = new StreamReader(filename))
                    {
                        object value = GetDouble(file);   // не нужная переменная
                        var value1 = GetInt(file);          // не нужная переменная
                        var value2 = GetInt(file);          // не нужная переменная
                        Params.dtTrend = GetDouble(file);
                        Params.time = GetDouble(file);
                        Params.dtime = GetDouble(file);
                        Params.theta = GetDouble(file);
                        Params.UpWindCoeff = GetDouble(file);
                        Params.turbulentVisCoeff = GetDouble(file);
                        Params.latitudeArea = GetDouble(file);
                        Params.droundWaterCoeff = GetDouble(file);
                        var value3 = GetInt(file);          // не нужная переменная
                        var value4 = GetInt(file);          // не нужная переменная
                        var value5 = GetInt(file);          // не нужная переменная
                        var value6 = GetInt(file);          // не нужная переменная
                        var value7 = GetInt(file);          // не нужная переменная
                        var value8 = GetInt(file);          // не нужная переменная
                        var value9 = GetInt(file);          // не нужная переменная
                        var value10 = GetInt(file);          // не нужная переменная
                        Params.H_minGroundWater = GetDouble(file);
                        var value11 = GetDouble(file);          // не нужная переменная
                        Params.filtrСoeff = GetDouble(file);
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
                        
                        river2D.SetParams(Params);

                        // выделение памяти под узловые поля
                        mesh.nodes = new RiverNode[Count];
                        // чтение узловых полей
                        for (i = 0; i < Count; i++)
                        {
                            mesh.nodes[i] = new RiverNode();
                            string[] lines = GetLines(file, 8);
                            if (lines == null)
                                break;
                            int idx = 0;
                            mesh.nodes[i].n = int.Parse(lines[idx++].Trim());
                            mesh.nodes[i].i = i;
                            string s = lines[idx].Trim();
                            if (s == "x" || s == "s")
                            {
                                if (s == "x")
                                    mesh.nodes[i].fxc = FixedFlag.fixednode;   // x  фиксированный узел
                                else
                                    mesh.nodes[i].fxc = FixedFlag.sliding;  // s sliding - скользящий узел (по границе/ или линии)
                                mesh.nodes[i].fxc = 0;
                                idx++;
                            }
                            else
                                mesh.nodes[i].fxc = 0; // плавающий узел
                            mesh.nodes[i].X = double.Parse(lines[idx++].Trim(), MEM.formatter);
                            mesh.nodes[i].Y = double.Parse(lines[idx++].Trim(), MEM.formatter);
                            mesh.nodes[i].zeta = double.Parse(lines[idx++].Trim(), MEM.formatter);
                            //river2D.mesh.nodes[i].zeta0 = river2D.mesh.nodes[i].zeta;
                            mesh.nodes[i].ks = double.Parse(lines[idx++].Trim(), MEM.formatter);
                            idx++;
                            //river2D.mesh.nodes[i].curvature = double.Parse(lines[idx++].Trim(), formatter);
                            mesh.nodes[i].h = double.Parse(lines[idx++].Trim(), MEM.formatter);
                            mesh.nodes[i].qx = double.Parse(lines[idx++].Trim(), MEM.formatter);
                            mesh.nodes[i].qy = double.Parse(lines[idx++].Trim(), MEM.formatter);
                        }
                        // выделение памяти под КЭ и поля
                        mesh.AreaElems = new TriElementRiver[CountElements];
                        // чтение КЭ полей
                        for (i = 0; i < CountElements; i++)
                        {
                            mesh.AreaElems[i] = new TriElementRiver();
                            string[] lines = GetLines(file, 9);
                            mesh.AreaElems[i].ID = int.Parse(lines[0].Trim());
                            int typeFFV = int.Parse(lines[1].Trim());  // не нужная переменная
                            int typeFFL = int.Parse(lines[2].Trim());  // не нужная переменная
                                                                       // сдвиг на 1 нумерация узлов идет с нуля
                            mesh.AreaElems[i].Vertex1 = uint.Parse(lines[3].Trim());
                            mesh.AreaElems[i].Vertex2 = uint.Parse(lines[4].Trim());
                            mesh.AreaElems[i].Vertex3 = uint.Parse(lines[5].Trim());
                        }
                        // выделение памяти под граничные КЭ и поля
                        mesh.BoundElems = new BoundElementRiver[CountBoundElements];
                        for (i = 0; i < CountBoundElements; i++)
                        {
                            mesh.BoundElems[i] = new BoundElementRiver();
                            string[] lines = GetLines(file, 15);
                            mesh.BoundElems[i].ID = int.Parse(lines[0].Trim());
                            int typeBFV = int.Parse(lines[1].Trim());   // не нужная переменная
                            int typeBFL = int.Parse(lines[2].Trim());   // не нужная переменная

                            mesh.BoundElems[i].Vertex1 = uint.Parse(lines[3].Trim());
                            mesh.BoundElems[i].Vertex2 = uint.Parse(lines[4].Trim());
                            mesh.BoundElems[i].Eta = double.Parse(lines[5].Trim(), MEM.formatter);
                            mesh.BoundElems[i].Qn = double.Parse(lines[6].Trim(), MEM.formatter);
                            mesh.BoundElems[i].Qt = double.Parse(lines[7].Trim(), MEM.formatter);
                            //river2D.mesh.BoundElems[i].H = double.Parse(lines[8].Trim(), formatter);
                            //river2D.mesh.BoundElems[i].Zeta = double.Parse(lines[9].Trim(), formatter);
                            //river2D.mesh.BoundElems[i].Length = double.Parse(lines[10].Trim(), formatter);
                           // river2D.mesh.BoundElems[i].p[6] = double.Parse(lines[11].Trim(), formatter);
                            mesh.BoundElems[i].boundCondType = int.Parse(lines[12].Trim());
                            //river2D.mesh.BoundElems[i].bcs[1] = int.Parse(lines[13].Trim());
                            //river2D.mesh.BoundElems[i].bcs[2] = int.Parse(lines[14].Trim());
                        }
                        // заголовок    
                        string[] headlines = GetLines(file, 6);
                        // выделение памяти под граничные сегменты
                        mesh.boundSegment = new BoundSegmentRiver[CountBoundSegments];
                        for (i = 0; i < CountBoundSegments; i++)
                        {
                            mesh.boundSegment[i] = new BoundSegmentRiver();
                            string[] lines = GetLines(file, 6);
                            mesh.boundSegment[i].ID = int.Parse(lines[0].Trim());
                            mesh.boundSegment[i].boundCondType = int.Parse(lines[1].Trim());
                            mesh.boundSegment[i].Eta = double.Parse(lines[2].Trim(), MEM.formatter);
                            mesh.boundSegment[i].Qn = double.Parse(lines[3].Trim(), MEM.formatter);
                            mesh.boundSegment[i].startnode = int.Parse(lines[4].Trim());
                            mesh.boundSegment[i].endnode = int.Parse(lines[5].Trim());
                        }
                        file.Close();
                    }
                    // Перенумерация
                    mesh.RenumberingID();
                    // Настройка граничных условий и ссылок в КЭ
                    // цикл узлам граничных КЭ 
                    for (int i = 0; i < mesh.BoundElems.Length; i++)
                    {
                        mesh.nodes[mesh.BoundElems[i].Vertex1].fxc = FixedFlag.fixednode;
                        mesh.nodes[mesh.BoundElems[i].Vertex2].fxc = FixedFlag.fixednode;
                    }
                    Logger.Instance.Info(" Number of unknowns = " + (mesh.CountKnots * 3).ToString());
                    river2D.Set(mesh);
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
    }
}
