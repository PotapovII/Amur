//---------------------------------------------------------------------------
//                   ПРОЕКТ  "Русловые процессы"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 22.08.2028 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshGeneratorsLib.IO
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    
    using GeometryLib;
    using CommonLib;
    using CommonLib.IO;
    using MemLogLib;
    [Serializable]
    public class FormatFileTaskMap : ATaskFormat<IHTaskMap>
    {
        /// <summary>
        /// Поддержка внешних форматов загрузки
        /// </summary>
        public override bool SupportImport { get; }
        /// <summary>
        /// Поддержка внешних форматов сохранения
        /// </summary>
        public override bool SupportExport { get; }

        public static string path = @"..\\TData\";
        public FormatFileTaskMap()
        {
            SupportImport = true;
            SupportExport = true;
            extString = new List<string>() { ".tmap" };
        }

        protected void GetStartEndIndex(string[] Filter, int lookrow, out int rowstart, out int rowend)
        {
            rowstart = lookrow;
            for (; lookrow < Filter.Length; )
            {
                string line = Filter[lookrow];
                if (line.StartsWith("##"))
                    break;
                if (line.Trim() == "")
                    break;
                if (line.StartsWith("#"))
                {
                    rowstart = ++lookrow;
                    continue;
                }
                lookrow++;
            }
            rowend = lookrow;
        }

        /// <summary>
        /// Прочитать файл, содержащий речную задачу из старого River2D формата данных cdg.
        /// </summary>
        /// <param name="filename">Путь к файлу для чтения..</param>
        /// <returns>Экземпляр интерфейса<see cref="IMesh" /> interface.</returns>
        public override void Read(string filename, ref IHTaskMap taskMap, uint testID = 0)
        {
            if (IsSupported(path + filename) == true)
            {
                int rowstart=0, rowend=0;
                taskMap = new HTaskMap();
                try
                {
                    string[] lines = File.ReadAllLines(path + filename);
                    List<VMapKnot> knots = new List<VMapKnot>();
                    List<HMapSegment> segments = new List<HMapSegment>();
                    List<HMapFacet> facets = new List<HMapFacet>();
                    List<HMapSubArea> subareas = new List<HMapSubArea>();

                    if (lines[0].Trim() != "##Version 1.0")
                        throw new NotSupportedException("Не совместимость версий'" + filename + "' file.");
                    int lookrow = 2;
                    // ##Points
                    GetStartEndIndex(lines, lookrow, out rowstart, out rowend);
                    for (int i = rowstart; i < rowend; i++)
                        knots.Add(VMapKnot.Parse(lines[i]));
                    // ##Segment
                    GetStartEndIndex(lines, rowend+1, out rowstart, out rowend);
                    for (int i = rowstart; i < rowend; i++)
                    {
                        string[] mas = (lines[i].Trim()).Split(' ');
                        int ID = int.Parse(mas[0]);
                        List<VMapKnot> nods = new List<VMapKnot>();
                        List<int> markBC = new List<int>();
                        List<TypeBoundCond> typeBC = new List<TypeBoundCond>();
                        int Count = (mas.Length - 1) / 3;
                        int j = 1;
                        for (int n = 0; n < Count; n++)
                        {
                            int MarkBC = int.Parse(mas[0 + j]);
                            int TypeBC = int.Parse(mas[1 + j]);
                            int adress = int.Parse(mas[2 + j]);
                            markBC.Add(MarkBC);
                            typeBC.Add((TypeBoundCond)TypeBC);
                            nods.Add(new VMapKnot(knots[adress]));
                            j += 3;
                        }
                        segments.Add(new HMapSegment(nods, markBC.ToArray(),  ID));
                    }
                    // ##Facet
                    GetStartEndIndex(lines, rowend + 1, out rowstart, out rowend);
                    for (int i = rowstart; i < rowend; i++)
                    {
                        string[] mas = (lines[i].Trim()).Split(' ');
                        int ID = int.Parse(mas[0]);
                        HMapFacet facet = new HMapFacet();
                       // facet.ID = ID;
                        for (int j = 1; j < mas.Length; j++)
                        {
                            int adress = int.Parse(mas[j]);
                            facet.Add(new HMapSegment(segments[adress]));
                        }
                        facets.Add(facet);
                    }
                    // ##SubArea
                    GetStartEndIndex(lines, rowend + 1, out rowstart, out rowend);
                    for (int i = rowstart; i < rowend; i++)
                    {
                        string[] mas = (lines[i].Trim()).Split(' ');
                        int ID = int.Parse(mas[0]);
                        int SubType = int.Parse(mas[1]);
                        HMapSubArea subArea = new HMapSubArea(ID, SubType);
                        for (int j = 2; j < mas.Length; j++)
                        {
                            int adress = int.Parse(mas[j]);
                            subArea.Add(new HMapFacet(facets[adress]));
                        }
                        subareas.Add(subArea);
                    }
                    // ##SubArea
                    foreach (HMapSubArea s in subareas)
                        taskMap.Add(s);
                    GetStartEndIndex(lines, rowend + 1, out rowstart, out rowend);
                    string[] masE = (lines[rowstart].Trim()).Split(' ');
                    taskMap.Name = masE[0];
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
        public override void Write(IHTaskMap taskMap, string filename)
        {
            //HTaskMap map = tMap as HTaskMap;
            //if (map == null)
            //    throw new NotSupportedException("Не возможно сохранить выбранный формат задачи map == null");
            string ext = Path.GetExtension(path + filename);
            if (ext == ".tmap")
            {
                Dictionary<string,int> dictionaryID = new Dictionary<string,int>();
                Dictionary<string,VMapKnot> dictionary = new Dictionary<string, VMapKnot>();
                Dictionary<string, HMapSegment> dictionarySeg = new Dictionary<string, HMapSegment>();
                Dictionary<string, HMapFacet> dictionaryFac = new Dictionary<string, HMapFacet>();
                string pathFilename = path + filename;
                using (StreamWriter file = new StreamWriter(pathFilename))
                {
                    int IDF = 0;
                    // формируем словарь уникальных узлов
                    int knotID = 0;
                    int segID = 0;
                    int subAreaID = 0;
                    List<HMapSubArea> subareas = taskMap.GetAreaMaps();
                    foreach (HMapSubArea s in subareas)
                    {
                        s.ID = subAreaID; subAreaID++;
                        List<HMapFacet> Facets = s.Facets;
                        foreach (HMapFacet f in Facets)
                        {
                            string hesh = f.GetHash();
                            if (!dictionaryFac.ContainsKey(hesh))
                            {
                                f.ID = IDF; IDF++;
                                dictionaryFac.Add(hesh, f);
                            }
                            List<HMapSegment> Segments = f.Segments;
                            foreach (HMapSegment seg in Segments)
                            {
                                string shesh = seg.GetHash();
                                if (!dictionarySeg.ContainsKey(shesh))
                                {
                                    seg.ID = segID; segID++;
                                    dictionarySeg.Add(shesh, seg);
                                }
                                VMapKnot[] Knots = seg.Knots;
                                for (int i = 0; i < Knots.Length; i++)
                                {
                                    VMapKnot Knot = Knots[i];
                                    string hash = Knot.GetHash();
                                    if (!dictionary.ContainsKey(hash))
                                    {
                                        dictionary.Add(hash, Knot);
                                        dictionaryID.Add(hash, knotID);
                                        knotID++;
                                    }
                                }
                            }
                        }
                    }

                    file.WriteLine("##Version 1.0");
                    file.WriteLine("##Points :: format: int(PointsID) double(x) double(y) int(flag0) int(flag1) double(R) double[](Params)");
                    foreach (var pair in dictionary)
                    {
                        string hesh = pair.Key;
                        VMapKnot point = pair.Value;
                        int KnotID = dictionaryID[hesh];
                        file.WriteLine(KnotID.ToString() + point.ToString("F5"));
                    }
                    file.WriteLine("##Segment :: format: int(SegmentID) int[](MarkBC) int[](TypeBC) int[](PointsID)");

                    List<HMapSegment> segments = new List<HMapSegment>();
                    foreach (var pair in dictionarySeg)
                        segments.Add(pair.Value);
                    segments.Sort();
                    for (int ID = 0; ID < segments.Count; ID++)
                    //foreach (var pair in dictionarySeg)
                    {
                        //int ID = pair.Key;
                        HMapSegment seg = segments[ID]; //pair.Value;
                        string s = ID.ToString();
                        for (int j = 0; j < seg.Knots.Length; j++)
                        {
                            s += " " + seg.MarkBC[j].ToString();
                            string hesh = seg.Knots[j].GetHash();
                            int key = dictionaryID[hesh];
                            s += " " + key.ToString();
                        }
                        file.WriteLine(s);
                    }
                    file.WriteLine("##Facet :: format: int(FacetID) int[](SegmentID)");
                    foreach (var pair in dictionaryFac)
                    {
                        HMapFacet facet = pair.Value;
                        string s = facet.ID.ToString();
                        for (int j = 0; j < facet.Segments.Count; j++)
                        {
                            int key = facet.Segments[j].ID;
                            s += " " + key.ToString();
                        }
                        file.WriteLine(s);
                    }
                    file.WriteLine("##SubArea :: format: int(SubAreaID) int[](FacetID)");
                    foreach (HMapSubArea subArea in subareas)
                    {
                        string s = subArea.ID.ToString();
                        s += " " + subArea.SubType.ToString();
                        foreach (HMapFacet facet in subArea.Facets)
                            s += " " + facet.ID.ToString();
                        file.WriteLine(s);
                    }
                    file.WriteLine("##Area :: format: string(NameArea) int[] (SubAreaID)");
                    {
                        string s = taskMap.Name;
                        foreach (HMapSubArea subArea in subareas)
                            s += " " + subArea.ID.ToString();
                        file.WriteLine(s);
                    }
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
