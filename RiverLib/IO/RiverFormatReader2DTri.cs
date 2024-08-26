//---------------------------------------------------------------------------
//                   ПРОЕКТ  "Русловые процессы"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 16.11.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace RiverLib.IO
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;

    using MeshLib;
    using MemLogLib;
    using CommonLib;
    using CommonLib.IO;
    using CommonLib.Geometry;
    using RiverLib.River2D;

    [Serializable]
    public class RiverFormatReader2DTri : ATaskFormat<IRiver>
    {
        /// <summary>
        /// Поддержка внешних форматов загрузки
        /// </summary>
        public override bool SupportImport { get; }
        /// <summary>
        /// Поддержка внешних форматов сохранения
        /// </summary>
        public override bool SupportExport { get; }

        public RiverFormatReader2DTri()
        {
            extString = new List<string>() { ".txt" };
            SupportImport = true;
            SupportExport = true;
        }
        /// <summary>
        /// Прочитать файл, содержащий речную задачу из старого River2D формата данных cdg.
        /// </summary>
        /// <param name="filename">Путь к файлу для чтения..</param>
        /// <returns>Экземпляр интерфейса<see cref="IMesh" /> interface.</returns>
        public override void Read(string filename, ref IRiver river, uint testID = 0)
        {
            RiverEmptyXY2D river2D = river as RiverEmptyXY2D;
            if (river2D == null)
                throw new NotSupportedException("Не возможно импортировать выбранный формат задачи RiverEmptyXY2D == null");
            if (IsSupported(filename) == true)
            {
                try
                {
                    TypeBoundCond bottom1D = TypeBoundCond.Dirichlet;
                    TypeBoundCond right1D = TypeBoundCond.Dirichlet;
                    TypeBoundCond top1D = TypeBoundCond.Dirichlet;
                    TypeBoundCond left1D = TypeBoundCond.Dirichlet;
                    double ValueBottom=0, ValueLeft=0, ValueRight=0, ValueTop=0;
                    
                    string filenameTaxkEx = "dataTask.tsk";
                    string filenameTaxk = Path.GetDirectoryName(filename)+"\\"+ filenameTaxkEx;
                    // файл расширения задачи формирует граничные условия
                    if (Exists(filenameTaxk) == true)
                    {
                        string[] data = GetAllLines(filenameTaxk);
                        bottom1D = (TypeBoundCond)LOG.GetInt(data[0]);
                        right1D = (TypeBoundCond)LOG.GetInt(data[1]);
                        top1D = (TypeBoundCond)LOG.GetInt(data[2]);
                        left1D = (TypeBoundCond)LOG.GetInt(data[3]);
                        ValueBottom = LOG.GetDouble(data[4]);
                        ValueRight = LOG.GetDouble(data[5]);
                        ValueTop = LOG.GetDouble(data[6]);
                        ValueLeft = LOG.GetDouble(data[7]);
                    }
                    double[] VAlueBC = { ValueBottom, ValueRight, ValueTop , ValueLeft };
                    TriMesh mesh = new TriMesh();
                    using (StreamReader file = new StreamReader(filename))
                    {
                        string line = file.ReadLine();
                        int CountElements = int.Parse(line.Trim('\t'));
                        MEM.Alloc(CountElements, ref mesh.AreaElems);
                        for (int i = 0; i < CountElements; i++)
                        {
                            line = file.ReadLine().Trim();
                            string[] slines = line.Split(',', '(', ')', ' ', '\t');
                            mesh.AreaElems[i].Vertex1 = uint.Parse(slines[0]);
                            mesh.AreaElems[i].Vertex2 = uint.Parse(slines[1]);
                            mesh.AreaElems[i].Vertex3 = uint.Parse(slines[2]);
                        }
                        line = file.ReadLine();
                        int CountKnots = int.Parse(line.Trim('\t'));

                        MEM.Alloc(CountKnots, ref mesh.CoordsX);
                        MEM.Alloc(CountKnots, ref mesh.CoordsY);
                        MEM.Alloc(CountKnots, ref river2D.zeta0);
                        MEM.Alloc(CountKnots, ref river2D.tauX);
                        MEM.Alloc(CountKnots, ref river2D.tauY);
                        MEM.Alloc(CountKnots, ref river2D.P);

                        for (int i = 0; i < CountKnots; i++)
                        {
                            line = file.ReadLine().Trim('(', ')', '\t');
                            string[] slines = LOG.Split(line);

                            //mesh.CoordsX[i] = double.Parse(slines[0], formatter);
                            //mesh.CoordsY[i] = double.Parse(slines[1], formatter);
                            //river2D.zeta0[i] = double.Parse(slines[2], formatter);

                            mesh.CoordsX[i] = double.Parse(slines[0], MEM.formatter);
                            mesh.CoordsY[i] = double.Parse(slines[2], MEM.formatter);
                            river2D.zeta0[i] = double.Parse(slines[1], MEM.formatter);
                        }
                        // исключение плохих КЭ
                        uint[] kn = { 0, 0, 0 };
                        double[] xx = { 0, 0, 0 };
                        double[] yy = { 0, 0, 0 };
                        List<TriElement> goodElems = new List<TriElement>();
                        int CoundFL = 0;
                        double S;
                        int flag;
                        using (StreamWriter fileRep = new StreamWriter("ZeroElements.txt"))
                        {
                            for (uint elem = 0; elem < mesh.CountElements; elem++)
                            {
                                flag = 0;
                                // получить узлы КЭ
                                mesh.ElementKnots(elem, ref kn);
                                // координаты и площадь
                                mesh.GetElemCoords(elem, ref xx, ref yy);
                                if (xx[0] == xx[1] && xx[1] == xx[2])
                                    flag = 1;
                                if (yy[0] == yy[1] && yy[1] == yy[2])
                                    flag += 2;
                                // площадь
                                S = mesh.ElemSquare(elem);
                                if (S < 0)
                                {
                                    uint Buff = mesh.AreaElems[elem].Vertex1;
                                    mesh.AreaElems[elem].Vertex1 = mesh.AreaElems[elem].Vertex2;
                                    mesh.AreaElems[elem].Vertex2 = Buff;
                                }
                                if (S != 0 && flag == 0)
                                {
                                    TriElement e = mesh.AreaElems[elem];
                                    goodElems.Add(e);
                                }
                                else
                                {
                                    fileRep.WriteLine("Площадь FE = {0:F10} Index FE {1}", S, elem);
                                    Console.WriteLine("S = {0:F10} Index FE {1} flag = {2} ", S, elem, flag);
                                    CoundFL++;
                                }
                            }
                            fileRep.WriteLine("Плохих элементов {0}", CoundFL);
                            Console.WriteLine("Плохих элементов {0}", CoundFL);
                            fileRep.Close();
                        }
                        mesh.AreaElems = goodElems.ToArray();

                        for (int i = 0; i < CountKnots; i++)
                        {
                            line = file.ReadLine();
                            string[] slines = LOG.Split(line);
                            river2D.tauX[i] = double.Parse(slines[0], MEM.formatter);
                        }
                        for (int i = 0; i < CountKnots; i++)
                        {
                            line = file.ReadLine();
                            string[] slines = LOG.Split(line);
                            river2D.tauY[i] = double.Parse(slines[0], MEM.formatter);
                        }
                        for (int i = 0; i < CountKnots; i++)
                        {
                            line = file.ReadLine();
                            string[] slines = LOG.Split(line);
                            river2D.P[i] = double.Parse(slines[0], MEM.formatter);
                        }

                        double[] x = mesh.GetCoords(0);
                        double[] y = mesh.GetCoords(1);
                        double maxX = x.Max();
                        double minX = x.Min();
                        double maxY = y.Max();
                        double minY = y.Min();
                        List<int> knotB = new List<int>();
                        List<int> knotR = new List<int>();
                        List<int> knotT = new List<int>();
                        List<int> knotL = new List<int>();
                        List<int> FB = new List<int>();
                        List<int> FR = new List<int>();
                        List<int> FT = new List<int>();
                        List<int> FL = new List<int>();
                        List<TypeBoundCond> TFB = new List<TypeBoundCond>();
                        List<TypeBoundCond> TFR = new List<TypeBoundCond>();
                        List<TypeBoundCond> TFT = new List<TypeBoundCond>();
                        List<TypeBoundCond> TFL = new List<TypeBoundCond>();

                        for (int i = 0; i < y.Length; i++)
                            if (MEM.Equals(y[i], minY) == true)
                                knotB.Add(i); FB.Add(0); TFB.Add(TypeBoundCond.Neumann);
                        knotB.Sort();
                        for (int i = 0; i < y.Length; i++)
                            if (MEM.Equals(x[i], maxX) == true)
                                knotR.Add(i); FR.Add(1); TFR.Add(TypeBoundCond.Dirichlet);
                        knotR.Sort();
                        for (int i = 0; i < y.Length; i++)
                            if (MEM.Equals(y[i], maxY) == true)
                                knotT.Add(i); FT.Add(2); TFT.Add(TypeBoundCond.Neumann);
                        knotT.Sort();
                        for (int i = 0; i < y.Length; i++)
                            if (MEM.Equals(x[i], minX) == true)
                                knotL.Add(i); FL.Add(3); TFL.Add(TypeBoundCond.Dirichlet);
                        knotL.Sort();
                        List<int> knots = new List<int>();
                        List<int> FLG = new List<int>();
                        List<TypeBoundCond> TLG = new List<TypeBoundCond>();

                        knots.AddRange(knotB);
                        knots.AddRange(knotR);
                        knots.AddRange(knotT);
                        knots.AddRange(knotL);

                        FLG.AddRange(FB);
                        FLG.AddRange(FR);
                        FLG.AddRange(FT);
                        FLG.AddRange(FL);

                        TLG.AddRange(TFB);
                        TLG.AddRange(TFR);
                        TLG.AddRange(TFT);
                        TLG.AddRange(TFL);

                        mesh.BoundKnots = knots.ToArray();
                        mesh.BoundKnotsMark = FLG.ToArray();

                        Dictionary<string, AFacet> dictionary = new Dictionary<string, AFacet>();
                        int cu = 3;
                        int faceid = 0;
                        int boundaryFacetsMark = 0;

                        for (int eID = 0; eID < mesh.AreaElems.Length; eID++)
                        {
                            HPoint[] vertex = new HPoint[cu];
                            for (int vID = 0; vID < cu; vID++)
                            {
                                int nodeA = (int)mesh.AreaElems[eID][vID];
                                int nodeB = (int)mesh.AreaElems[eID][(vID + 1) % cu];
                                vertex[vID] = new HPoint(mesh.CoordsX[nodeA], mesh.CoordsY[nodeA]);
                                string faceеHash = AFacet.GetFaceHash(nodeA, nodeB);
                                if (dictionary.ContainsKey(faceеHash) == false)
                                {
                                    AFacet facet = new AFacet(faceid, nodeA, nodeB, eID, boundaryFacetsMark);
                                    dictionary.Add(faceеHash, facet);
                                    faceid++;
                                }
                                else
                                {
                                    AFacet facet = dictionary[faceеHash];
                                    facet.BoundaryFacetsMark = -1;
                                    facet.NbElem = eID;
                                }
                            }
                        }
                        List<AFacet> flist = new List<AFacet>();
                        foreach (var pair in dictionary)
                        {
                            AFacet facet = pair.Value;
                            double xa = x[facet.Pointid1];
                            double ya = y[facet.Pointid1];
                            double xb = x[facet.Pointid2];
                            double yb = y[facet.Pointid2];
                            if (MEM.Equals(xa, minX) && MEM.Equals(xb, minX) && facet.NbElem == -1)
                            {
                                facet.BoundaryFacetsMark = 3;
                                flist.Add(facet);
                            }
                            if (MEM.Equals(xa, maxX) && MEM.Equals(xb, maxX) && facet.NbElem == -1)
                            {
                                facet.BoundaryFacetsMark = 1;
                                flist.Add(facet);
                            }
                            if (MEM.Equals(ya, minY) && MEM.Equals(yb, minY) && facet.NbElem == -1)
                            {
                                facet.BoundaryFacetsMark = 0;
                                flist.Add(facet);
                            }
                            if (MEM.Equals(ya, maxY) && MEM.Equals(yb, maxY) && facet.NbElem == -1)
                            {
                                facet.BoundaryFacetsMark = 2;
                                flist.Add(facet);
                            }
                        }

                        int CountBoundElements = flist.Count;
                        MEM.Alloc(CountBoundElements, ref mesh.BoundElems);
                        MEM.Alloc(CountBoundElements, ref mesh.BoundElementsMark);

                        int be = 0;
                        for (int i = 0; i < flist.Count; i++)
                        {
                            mesh.BoundElems[be].Vertex1 = (uint)flist[i].Pointid1;
                            mesh.BoundElems[be].Vertex2 = (uint)flist[i].Pointid2;
                            mesh.BoundElementsMark[be++] = flist[i].BoundaryFacetsMark;

                        }
                    }
                    river2D.Set(mesh, null);
                    BoundaryConditionsVar bc = new BoundaryConditionsVar(mesh);
                    bc.SetValue(VAlueBC, VAlueBC);
                    river2D.BoundCondition1D = bc;
                    //for (int i = 0; i < VAlueBC.Length; i++)
                    //{
                    //    bc.ValueNeu[i] = VAlueBC[i];
                    //    bc.ValueDir[i] = VAlueBC[i];
                    //}


                    double minTauX = river2D.tauX.Min();
                    double maxTauX = river2D.tauX.Max();
                    if (Math.Abs(maxTauX) > Math.Abs(minTauX))
                    {
                        Logger.Instance.Info("Поля напряжений не требуют инверсии");
                        for (int i = 0; i < mesh.CountKnots; i++)
                        {
                            river2D.tauX[i] = river2D.tauX[i];
                            river2D.tauY[i] = river2D.tauY[i];
                        }
                    }
                    else
                    {
                        Logger.Instance.Info("Для полей напряжений выполнена инверсия");
                        for (int i = 0; i < mesh.CountKnots; i++)
                        {
                            river2D.tauX[i] = -river2D.tauX[i];
                            river2D.tauY[i] = -river2D.tauY[i];
                            river2D.zeta0[i] = 0;
                            //river2D.tauX[i] = river2D.tauX[i];
                            //river2D.tauY[i] = river2D.tauY[i];
                        }
                    }
                    river = river2D;

                }
                catch (Exception ex)
                {
                    Logger.Instance.Info("Формат файла не корректен: " + ex.Message);
                    river = new RiverEmptyXY2D(new RiverStreamParams());
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
            River2D river2D = river as River2D;
            if (river2D == null)
                throw new NotSupportedException("Не возможно сохранить выбранный формат задачи river2D == null");
            string ext = Path.GetExtension(filename);
            if (ext == ".txt")
            {
                IMesh mesh = river2D.Mesh();
                TriElement[] elems = mesh.GetAreaElems();
                TwoElement[] belems = mesh.GetBoundElems();
                int[] bflags = mesh.GetBElementsBCMark();
                int[] bknots = mesh.GetBoundKnots();
                int[] bflagknots = mesh.GetBoundKnotsMark();
                double[] x = mesh.GetCoords(0);
                double[] y = mesh.GetCoords(1);

                using (StreamWriter file = new StreamWriter(filename))
                {
                    
                    file.WriteLine(elems.Length);
                    for (int i = 0; i < mesh.CountElements; i++)
                    {

                        file.WriteLine(elems[i].Vertex1.ToString() + " " +
                                       elems[i].Vertex2.ToString() + " " +
                                       elems[i].Vertex3.ToString());
                    }
                    file.WriteLine(mesh.CountKnots);
                    for (int i = 0; i < mesh.CountKnots; i++)
                    {
                        file.WriteLine("{0} {1}", x[i], y[i]);
                    }
                    file.WriteLine(mesh.CountBoundElements);
                    for (int i = 0; i < mesh.CountBoundElements; i++)
                    {
                        file.WriteLine(belems[i].Vertex1.ToString() + " " +
                                       belems[i].Vertex2.ToString() + " " +
                                       bflags[i].ToString());
                    }
                    file.WriteLine(bknots.Length);
                    for (int i = 0; i < bknots.Length; i++)
                    {
                        file.WriteLine("{0} {1}",
                        bknots[i],
                        bflagknots[i]);
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
