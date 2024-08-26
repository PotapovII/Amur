//---------------------------------------------------------------------------
//                   ПРОЕКТ  "Русловые процессы"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 03.09.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace RiverLib.IO
{
    using CommonLib.IO;
    using CommonLib;
    using MemLogLib;
    using System;
    using System.IO;
    using System.Collections.Generic;

    /// <summary>
    /// ОО: Работа с задачей об деформации створа реки
    /// </summary>
    [Serializable]
    public class RiverFormatReader1DSection : ATaskFormat<IRiver>
    {
        /// <summary>
        /// Поддержка внешних форматов загрузки
        /// </summary>
        public override bool SupportImport { get; }
        /// <summary>
        /// Поддержка внешних форматов сохранения
        /// </summary>
        public override bool SupportExport { get; }
        public RiverFormatReader1DSection()
        {
            SupportImport = false;
            SupportExport = false;
            extString = new List<string>() { ".crf" };
        }
        /// <summary>
        /// Прочитать файл, содержащий речную задачу из старого River2D формата данных cdg.
        /// </summary>
        /// <param name="filename">Путь к файлу для чтения..</param>
        /// <returns>Экземпляр интерфейса<see cref="IMesh" /> interface.</returns>
        public override void Read(string filename, ref IRiver river, uint testID = 0)
        {
            RiverStreamTask river1D = river as RiverStreamTask;
            if (river1D == null)
                throw new NotSupportedException("Не возможно открыть выбранный формат задачи river == null");
            string ext = Path.GetExtension(filename);
            if (ext == ".crf" || ext == ".CRF")
            {
                river1D = new RiverStreamTask(new RiverStreamParams());
                try
                {
                    using (StreamReader file = new StreamReader(filename))
                    {
                        //    object value = GetDouble(file);   // не нужная переменная
                        //    var value1 = GetInt(file);          // не нужная переменная
                        //    var value2 = GetInt(file);          // не нужная переменная
                        //    river2D.dtTrend = GetDouble(file);
                        //    river2D.time = GetDouble(file);
                        //    river2D.dtime = GetDouble(file);
                        //    river2D.theta = GetDouble(file);
                        //    river2D.UpWindCoeff = GetDouble(file);
                        //    river2D.turbulentVisCoeff = GetDouble(file);
                        //    river2D.latitudeArea = GetDouble(file);
                        //    river2D.droundWaterCoeff = GetDouble(file);
                        //    var value3 = GetInt(file);          // не нужная переменная
                        //    var value4 = GetInt(file);          // не нужная переменная
                        //    var value5 = GetInt(file);          // не нужная переменная
                        //    var value6 = GetInt(file);          // не нужная переменная
                        //    var value7 = GetInt(file);          // не нужная переменная
                        //    var value8 = GetInt(file);          // не нужная переменная
                        //    var value9 = GetInt(file);          // не нужная переменная
                        //    var value10 = GetInt(file);          // не нужная переменная
                        //    river2D.H_minGroundWater = GetDouble(file);
                        //    var value11 = GetDouble(file);          // не нужная переменная
                        //    river2D.filtrСoeff = GetDouble(file);
                        //    var value12 = GetInt(file);  // не нужная переменная
                        //    var value13 = GetInt(file);  // не нужная переменная
                        //    int[] Keqns = new int[12];
                        //    GetLine(file);
                        //    int i = 0;
                        //    for (int k = 0; k < 3; k++)
                        //    {
                        //        string line = file.ReadLine().Trim();
                        //        string[] sline = line.Split(' ', '\t');
                        //        for (int m = 0; m < sline.Length; m++)
                        //            if (sline[m] != "")
                        //                Keqns[i++] = int.Parse(sline[0].Trim());

                        //    }
                        //    var value14 = GetInt(file);  // не нужная переменная
                        //    var value15 = GetInt(file);  // не нужная переменная
                        //    int Count = GetInt(file);
                        //    int CountElements = GetInt(file);
                        //    int CountBoundElements = GetInt(file);
                        //    int CountBoundSegments = GetInt(file);
                        //    if (river2D.mesh == null)
                        //        river2D.mesh = new TriRiverMesh();
                        //    // выделение памяти под узловые поля
                        //    river2D.mesh.nodes = new RiverNode[Count];
                        //    // чтение узловых полей
                        //    for (i = 0; i < Count; i++)
                        //    {
                        //        river2D.mesh.nodes[i] = new RiverNode();
                        //        string[] lines = GetLines(file, 8);
                        //        if (lines == null)
                        //            break;
                        //        int idx = 0;
                        //        river2D.mesh.nodes[i].n = int.Parse(lines[idx++].Trim());
                        //        river2D.mesh.nodes[i].i = i;
                        //        string s = lines[idx].Trim();
                        //        if (s == "x" || s == "s")
                        //        {
                        //            //if (s == "x")
                        //            //    mesh.nodes[i].fxc = 1;  // x  фиксированный узел
                        //            //else
                        //            //    mesh.nodes[i].fxc = 2;  // s sliding - скользящий узел (по границе/ или линии)
                        //            river2D.mesh.nodes[i].fxc = 0;
                        //            idx++;
                        //        }
                        //        else
                        //            river2D.mesh.nodes[i].fxc = 0; // плавающий узел
                        //        river2D.mesh.nodes[i].X = double.Parse(lines[idx++].Trim(), formatter);
                        //        river2D.mesh.nodes[i].Y = double.Parse(lines[idx++].Trim(), formatter);
                        //        river2D.mesh.nodes[i].zeta = double.Parse(lines[idx++].Trim(), formatter);
                        //        river2D.mesh.nodes[i].zeta0 = river2D.mesh.nodes[i].zeta;
                        //        river2D.mesh.nodes[i].ks = double.Parse(lines[idx++].Trim(), formatter);
                        //        river2D.mesh.nodes[i].curvature = double.Parse(lines[idx++].Trim(), formatter);
                        //        river2D.mesh.nodes[i].h = double.Parse(lines[idx++].Trim(), formatter);
                        //        river2D.mesh.nodes[i].qx = double.Parse(lines[idx++].Trim(), formatter);
                        //        river2D.mesh.nodes[i].qy = double.Parse(lines[idx++].Trim(), formatter);
                        //    }
                        //    // выделение памяти под КЭ и поля
                        //    river2D.mesh.AreaElems = new TriElementRiver[CountElements];
                        //    // чтение КЭ полей
                        //    for (i = 0; i < CountElements; i++)
                        //    {
                        //        river2D.mesh.AreaElems[i] = new TriElementRiver();
                        //        string[] lines = GetLines(file, 9);
                        //        river2D.mesh.AreaElems[i].ID = int.Parse(lines[0].Trim());
                        //        int typeFFV = int.Parse(lines[1].Trim());  // не нужная переменная
                        //        int typeFFL = int.Parse(lines[2].Trim());  // не нужная переменная
                        //                                                   // сдвиг на 1 нумерация узлов идет с нуля
                        //        river2D.mesh.AreaElems[i].Vertex1 = uint.Parse(lines[3].Trim());
                        //        river2D.mesh.AreaElems[i].Vertex2 = uint.Parse(lines[4].Trim());
                        //        river2D.mesh.AreaElems[i].Vertex3 = uint.Parse(lines[5].Trim());
                        //    }
                        //    // выделение памяти под граничные КЭ и поля
                        //    river2D.mesh.BoundElems = new BoundElementRiver[CountBoundElements];
                        //    for (i = 0; i < CountBoundElements; i++)
                        //    {
                        //        river2D.mesh.BoundElems[i] = new BoundElementRiver();
                        //        string[] lines = GetLines(file, 15);
                        //        river2D.mesh.BoundElems[i].ID = int.Parse(lines[0].Trim());
                        //        int typeBFV = int.Parse(lines[1].Trim());   // не нужная переменная
                        //        int typeBFL = int.Parse(lines[2].Trim());   // не нужная переменная

                        //        river2D.mesh.BoundElems[i].Vertex1 = uint.Parse(lines[3].Trim());
                        //        river2D.mesh.BoundElems[i].Vertex2 = uint.Parse(lines[4].Trim());
                        //        river2D.mesh.BoundElems[i].p[0] = double.Parse(lines[5].Trim(), formatter);
                        //        river2D.mesh.BoundElems[i].p[1] = double.Parse(lines[6].Trim(), formatter);
                        //        river2D.mesh.BoundElems[i].p[2] = double.Parse(lines[7].Trim(), formatter);
                        //        river2D.mesh.BoundElems[i].p[3] = double.Parse(lines[8].Trim(), formatter);
                        //        river2D.mesh.BoundElems[i].p[4] = double.Parse(lines[9].Trim(), formatter);
                        //        river2D.mesh.BoundElems[i].p[5] = double.Parse(lines[10].Trim(), formatter);
                        //        river2D.mesh.BoundElems[i].p[6] = double.Parse(lines[11].Trim(), formatter);
                        //        river2D.mesh.BoundElems[i].bcs[0] = int.Parse(lines[12].Trim());
                        //        river2D.mesh.BoundElems[i].bcs[1] = int.Parse(lines[13].Trim());
                        //        river2D.mesh.BoundElems[i].bcs[2] = int.Parse(lines[14].Trim());
                        //    }
                        //    // заголовок    
                        //    string[] headlines = GetLines(file, 6);
                        //    // выделение памяти под граничные сегменты
                        //    river2D.mesh.boundSegment = new BoundSegmentRiver[CountBoundSegments];
                        //    for (i = 0; i < CountBoundSegments; i++)
                        //    {
                        //        river2D.mesh.boundSegment[i] = new BoundSegmentRiver();
                        //        string[] lines = GetLines(file, 6);
                        //        river2D.mesh.boundSegment[i].ID = int.Parse(lines[0].Trim());
                        //        river2D.mesh.boundSegment[i].boundCondType = int.Parse(lines[1].Trim());
                        //        river2D.mesh.boundSegment[i].Hn = double.Parse(lines[2].Trim(), formatter);
                        //        river2D.mesh.boundSegment[i].Qn = double.Parse(lines[3].Trim(), formatter);
                        //        river2D.mesh.boundSegment[i].startnode = int.Parse(lines[4].Trim());
                        //        river2D.mesh.boundSegment[i].endnode = int.Parse(lines[5].Trim());
                        //    }
                        file.Close();
                    }
                    //// Перенумерация
                    //river2D.mesh.RenumberingID();
                    //// Настройка граничных условий и ссылок в КЭ
                    //// цикл узлам граничных КЭ 
                    //for (int i = 0; i < river2D.mesh.BoundElems.Length; i++)
                    //{
                    //    river2D.mesh.nodes[river2D.mesh.BoundElems[i].Vertex1].fxc = 1;
                    //    river2D.mesh.nodes[river2D.mesh.BoundElems[i].Vertex2].fxc = 1;
                    //}
                    //Logger.Instance.Info(" Number of unknowns = " + (river2D.mesh.CountKnots * 3).ToString());
                    //river2D.Set(river2D.mesh);
                    //// обновление скоростей
                    //river2D.RollbackToOldValues();
                    //river2D.updateVelocities();
                    river = river1D;
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
        /// Сохраняем сетку на диск.
        /// </summary>
        /// <param name="mesh">Экземпляр интерфейса<see cref="IMesh" /> interface.</param>
        /// <param name="filename">Путь к файлу для сохранения.</param>
        public override void Write(IRiver river, string filename)
        {
            RiverStreamTask river2D = river as RiverStreamTask;
            if (river2D == null)
                throw new NotSupportedException("Не возможно сохранить выбранный формат задачи river2D == null");
            string ext = Path.GetExtension(filename);
            if (ext == ".crf" || ext == ".CRF")
            {
                using (StreamWriter file = new StreamWriter(filename))
                {
                    //file.WriteLine(mesh.CountElements); 
                    //for (int i = 0; i < mesh.CountElements; i++)
                    //{
                    //    file.WriteLine(mesh.AreaElems[i].Vertex1.ToString() + " " +
                    //                   mesh.AreaElems[i].Vertex2.ToString() + " " +
                    //                   mesh.AreaElems[i].Vertex3.ToString());
                    //}
                    //file.WriteLine(mesh.CountKnots);
                    //for (int i = 0; i < mesh.CountKnots; i++)
                    //{
                    //    file.WriteLine("{0} {1}",
                    //    mesh.CoordsX[i],
                    //    mesh.CoordsY[i]);
                    //}
                    //file.WriteLine(mesh.CountBoundElements);
                    //for (int i = 0; i < mesh.CountBoundElements; i++)
                    //{
                    //    file.WriteLine(mesh.BoundElems[i].Vertex1.ToString() + " " +
                    //                   mesh.BoundElems[i].Vertex2.ToString() + " " +
                    //                   mesh.BoundElementsMark[i].ToString());
                    //}
                    //file.WriteLine(mesh.BoundKnots.Length);
                    //for (int i = 0; i < mesh.BoundKnots.Length; i++)
                    //{
                    //    file.WriteLine("{0} {1}",
                    //    mesh.BoundKnots[i],
                    //    mesh.BoundKnotsMark[i]);

                    file.Close();
                }
                return;
            }
            throw new NotSupportedException("Не возможно сохранить выбранный формат сетки mesh == null");
        }
        /// <summary>
        /// Создает список тестовых задач для загрузчика по умолчанию
        /// </summary>
        /// <returns></returns>
        public override List<string> GetTestsName()
        {
            List<string> strings = new List<string>();
            strings.Add("Основная задача - тестовая");
            return strings;
        }
    }
}
