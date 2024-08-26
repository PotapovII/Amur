//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2021 -
//                       ALL RIGHT RESERVED
//---------------------------------------------------------------------------
//                      09.10.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshAdapterLib
{
    using MeshLib;
    using MemLogLib;
    using CommonLib;
    using GeometryLib;
    
    using System;
    using System.IO;
    using System.Linq;
    using System.Globalization;
    using System.Collections.Generic;
    using CommonLib.Geometry;

    /// <summary>
    /// ОО: Загрузка сетки и данных
    /// </summary>
    public class MeshAdapter2D
    {
        public static NumberFormatInfo formatter = new NumberFormatInfo { NumberDecimalSeparator = "." };
        /// <summary>
        /// Загрузка сетки КЭ, координат вершин и дна, придонных касательных напряжений и давления
        /// </summary>
        /// <param name="FileName">имя файла</param>
        /// <param name="mesh">сетка</param>
        /// <param name="Zeta0">дно</param>
        /// <param name="tauX">придонное касательное напряжение по Х</param>
        /// <param name="tauY">придонное касательное напряжение по Х</param>
        /// <param name="P">придонное давление</param>
        public void LoadData(string FileName, ref TriMesh mesh, ref double[] Zeta0, ref double[] tauX, ref double[] tauY, ref double[] P)
        {
            try
            {
                //string filename = "TaskData.tsk";
                mesh = new TriMesh();
                using (StreamReader file = new StreamReader(FileName))
                {
                    //FiltrFileReadLine file = new FiltrFileReadLine(File);
                    string line = file.ReadLine();
                    int Count = int.Parse(line.Trim('\t'));
                    mesh.AreaElems = new TriElement[Count];
                    for (int i = 0; i < Count; i++)
                    {
                        line = file.ReadLine().Trim();
                        string[] slines = line.Split(',', '(', ')', ' ', '\t');
                        mesh.AreaElems[i].Vertex1 = uint.Parse(slines[0]);
                        mesh.AreaElems[i].Vertex2 = uint.Parse(slines[1]);
                        mesh.AreaElems[i].Vertex3 = uint.Parse(slines[2]);
                    }
                    line = file.ReadLine();
                    int CountCoord = int.Parse(line.Trim('\t'));
                    Zeta0 = new double[CountCoord];
                    mesh.CoordsX = new double[CountCoord];
                    mesh.CoordsY = new double[CountCoord];
                    for (int i = 0; i < CountCoord; i++)
                    {
                        line = file.ReadLine().Trim('(', ')', '\t');
                        //string[] slines = line.Split(' ', '\t');
                        string[] slines = LOG.Split(line);
                        mesh.CoordsX[i] = double.Parse(slines[0], formatter);
                        mesh.CoordsY[i] = double.Parse(slines[1], formatter);
                        Zeta0[i] = double.Parse(slines[2], formatter);
                        //Zeta0[i] = 0; double.Parse(slines[2], formatter);
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
                            if(S<0)
                            {
                                uint Buff = mesh.AreaElems[elem].Vertex1;
                                mesh.AreaElems[elem].Vertex1 = mesh.AreaElems[elem].Vertex2;
                                mesh.AreaElems[elem].Vertex2 = Buff;
                                //S = mesh.ElemSquare(elem);
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

                    tauX = new double[CountCoord];
                    for (int i = 0; i < CountCoord; i++)
                    {
                        line = file.ReadLine();
                        string[] slines = LOG.Split(line);
                        tauX[i] = double.Parse(slines[0], formatter);
                    }
             
                    tauY = new double[CountCoord];
                    for (int i = 0; i < CountCoord; i++)
                    {
                        line = file.ReadLine();
                        string[] slines = LOG.Split(line);
                        tauY[i] = double.Parse(slines[0], formatter);
                    }
                    P = new double[CountCoord];
                    for (int i = 0; i < CountCoord; i++)
                    {
                        line = file.ReadLine();
                        string[] slines = LOG.Split(line);
                        P[i] = double.Parse(slines[0], formatter);
                    }
                    double maxTauX = tauX.Max();
                    double minTauX = tauX.Min();
                    double maxTauY = tauY.Max();
                    double minTauY = tauY.Min();
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
                        if (MEM.Equals(y[i],minY) == true)
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
                    TypeBoundCond left1D = TypeBoundCond.Dirichlet;
                    TypeBoundCond right1D = TypeBoundCond.Dirichlet;
                    TypeBoundCond boundaryType = TypeBoundCond.Neumann;
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
                                //HPoint pa = new HPoint(mesh.CoordsX[nodeA], mesh.CoordsY[nodeA]);
                                //HPoint pb = new HPoint(mesh.CoordsX[nodeB], mesh.CoordsY[nodeB]);
                                //if ((Math.Abs(pa.y - minY) < 0.00001 && Math.Abs(pb.y - minY) < 0.00001) ||
                                //    (Math.Abs(pa.y - maxY) < 0.00001 && Math.Abs(pb.y - maxY) < 0.00001))
                                //{
                                //    boundaryType = TypeBoundCond.Neumann;
                                //    boundaryFacetsMark = 0;
                                //}
                                //else
                                //{
                                //    boundaryFacetsMark = 1;
                                //    boundaryType = TypeBoundCond.Dirichlet;
                                //}
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
                        if (MEM.Equals(xa,minX) && MEM.Equals(xb, minX) && facet.NbElem == -1)
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
                    int CountBE = flist.Count;
                    MEM.Alloc(CountBE, ref mesh.BoundElems);
                    MEM.Alloc(CountBE, ref mesh.BoundElementsMark);
                    int be = 0;
                    for (int i = 0; i < flist.Count; i++)
                    {
                            mesh.BoundElems[be].Vertex1 = (uint)flist[i].Pointid1;
                            mesh.BoundElems[be].Vertex2 = (uint)flist[i].Pointid2;
                            mesh.BoundElementsMark[be++] = flist[i].BoundaryFacetsMark;
                        
                    }



                    //AFacet[] Facets = new AFacet[dictionary.Count];
                    //foreach (var pair in dictionary)
                    //{
                    //    AFacet facet = pair.Value;
                    //    Facets[facet.id] = facet;
                    //}
                    //int CountBE = Facets.Where(xy => xy.nbElem == -1).Count();
                    //MEM.Alloc(CountBE, ref mesh.BoundElems);
                    //MEM.Alloc(CountBE, ref mesh.BoundElementsType);
                    //MEM.Alloc(CountBE, ref mesh.BoundElementsMark);
                    //int be = 0;
                    //for(int i = 0; i < Facets.Length; i++)
                    //{
                    //    if (Facets[i].nbElem == -1)
                    //    {
                    //        mesh.BoundElems[be].Vertex1 = (uint)Facets[i].pointid1;
                    //        mesh.BoundElems[be].Vertex2 = (uint)Facets[i].pointid2;
                    //        mesh.BoundElementsType[be] = Facets[i].boundaryType;
                    //        mesh.BoundElementsMark[be++] = Facets[i].boundaryFacetsMark;
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Info("Формат файла не корректен: " + ex.Message);
            }

        }

        /// <summary>
        /// Сохранение сетки КЭ, координат вершин и дна,
        /// </summary>
        /// <param name="FileName">имя файла</param>
        /// <param name="mesh">сетка</param>
        /// <param name="Zeta0">дно</param>
        /// <param name="tauX">придонное касательное напряжение по Х</param>
        /// <param name="tauY">придонное касательное напряжение по Х</param>
        /// <param name="P">придонное давление</param>
        public void SaveData(string FileName, TriMesh mesh, double[] Zeta0, double[] tauX, double[] tauY, double[] P)
        {
            using (StreamWriter file = new StreamWriter(FileName))
            {
                file.WriteLine(mesh.CountElements);
                for (int i = 0; i < mesh.CountElements; i++)
                {
                    file.WriteLine(mesh.AreaElems[i].Vertex1.ToString() + " " +
                                   mesh.AreaElems[i].Vertex2.ToString() + " " +
                                   mesh.AreaElems[i].Vertex3.ToString());
                }
                file.WriteLine(mesh.CountKnots);
                for (int i = 0; i < mesh.CountKnots; i++)
                {
                    file.WriteLine("{0:F8} {1:F8} {2:F8}",
                    mesh.CoordsX[i],
                    mesh.CoordsY[i],
                    Zeta0[i]);
                }
                for (int i = 0; i < mesh.CountKnots; i++)
                    file.WriteLine("{0:F8}", tauX[i]);
                for (int i = 0; i < mesh.CountKnots; i++)
                    file.WriteLine("{0:F8}", tauY[i]);
                for (int i = 0; i < mesh.CountKnots; i++)
                    file.WriteLine("{0:F8}", P[i]);
                file.Close();
            }
        }
        /// <summary>
        /// Сохранение координат вершин
        /// </summary>
        /// <param name="FileName">имя файла</param>
        /// <param name="mesh">сетка</param>
        /// <param name="Zeta0">дно</param>
        public void SaveDataZeta(string FileName, TriMesh mesh, double[] Zeta0)
        {
            using (StreamWriter file = new StreamWriter(FileName))
            {
                file.WriteLine(mesh.CountKnots);
                for (int i = 0; i < mesh.CountKnots; i++)
                {
                    file.WriteLine("{0:F8} {1:F8} {2:F8}",
                    mesh.CoordsX[i],
                    mesh.CoordsY[i],
                    Zeta0[i]);
                }
                file.Close();
            }
        }
        /// <summary>
        /// Печатаем сетку на консоль
        /// </summary>
        /// <param name="mesh">Экземпляр интерфейса<see cref="IMesh" /> interface.</param>
        public void WriteTriMesh(TriMesh mesh)
        {
            if (mesh == null)
                throw new NotSupportedException("Не показать выбранный формат сетки mesh == null");

            for (int i = 0; i < mesh.CountElements; i++)
            {
                Console.WriteLine(mesh.AreaElems[i].Vertex1.ToString() + " " +
                               mesh.AreaElems[i].Vertex2.ToString() + " " +
                               mesh.AreaElems[i].Vertex3.ToString());
            }
            Console.WriteLine(mesh.CountKnots);
            for (int i = 0; i < mesh.CountKnots; i++)
            {
                Console.WriteLine("{0} {1}",
                mesh.CoordsX[i],
                mesh.CoordsY[i]);
            }
            Console.WriteLine(mesh.CountBoundElements);
            for (int i = 0; i < mesh.CountBoundElements; i++)
            {
                Console.WriteLine(mesh.BoundElems[i].Vertex1.ToString() + " " +
                               mesh.BoundElems[i].Vertex2.ToString() + " " +
                               mesh.BoundElementsMark[i].ToString());
            }
            Console.WriteLine(mesh.BoundKnots.Length);
            for (int i = 0; i < mesh.BoundKnots.Length; i++)
            {
                Console.WriteLine("{0} {1}",
                mesh.BoundKnots[i],
                mesh.BoundKnotsMark[i]);
            }
        }

        // ======================================================
        //       Система координат        Обход узлов
        //     dy                                     i
        //   |---|----------> Y j      -------------> 0 j
        //   | dx                      -------------> 1 j 
        //   |---                      -------------> 2 j
        //   |
        //   |
        //   |
        //   V X  i
        // ======================================================
        /// <summary>
        /// Генерация триангуляционной КЭ сетки в прямоугольной области
        /// </summary>
        /// <param name="mesh">результат</param>
        /// <param name="Nx">узлов по Х</param>
        /// <param name="Ny">узлов по У</param>
        /// <param name="dx">шаг по Х</param>
        /// <param name="dy">шаг по У</param>
        /// <param name="Flag">признаки границ для ГУ</param>
        public void GetRectangleMesh(ref TriMesh mesh, int Nx, int Ny, double dx, double dy, int[] Flag = null)
        {
            int[] LFlag = { 0, 1, 1, 0 };
            if (Flag == null)
                Flag = LFlag;

            mesh = new TriMesh();
            int counter = 2 * (Nx - 1) + 2 * (Ny - 1);
            int CountNodes = Nx * Ny;
            int CountElems = 2 * (Nx - 1) * (Ny - 1);

            mesh.AreaElems = new TriElement[CountElems];

            mesh.BoundElems = new TwoElement[counter];
            mesh.BoundElementsMark = new int[counter];

            mesh.BoundKnots = new int[counter];
            mesh.BoundKnotsMark = new int[counter];

            mesh.CoordsX = new double[CountNodes];
            mesh.CoordsY = new double[CountNodes];

            uint[,] map = new uint[Nx, Ny];

            uint k = 0;
            for (uint i = 0; i < Nx; i++)
            {
                double xm = i * dx;
                for (int j = 0; j < Ny; j++)
                {
                    double ym = dy * j;
                    mesh.CoordsX[k] = xm;
                    mesh.CoordsY[k] = ym;
                    map[i, j] = k++;
                }
            }
            //for (int j = 0; j < Ny; j++)
            //{
            //    for (uint i = 0; i < Nx; i++)
            //    {
            //        double ym = j * dx;
            //        double xm = i * dy;

            //        mesh.CoordsX[k] = xm;
            //        mesh.CoordsY[k] = ym;
            //        map[i, j] = k++;
            //    }
            //}




            int elem = 0;
            for (int i = 0; i < Nx - 1; i++)
            {
                for (int j = 0; j < Ny - 1; j++)
                {
                    mesh.AreaElems[elem] = new TriElement(map[i, j], map[i + 1, j], map[i + 1, j + 1]);
                    elem++;
                    mesh.AreaElems[elem] = new TriElement(map[i + 1, j + 1], map[i, j + 1], map[i, j]);
                    elem++;
                }
            }
            k = 0;


            // низ
            for (int i = 0; i < Ny - 1; i++)
            {
                mesh.BoundElems[k] = new TwoElement(map[Nx - 1, i], map[Nx - 1, i + 1]);
                mesh.BoundElementsMark[k] = Flag[0];
                // задана функция
                mesh.BoundKnotsMark[k] = Flag[0];
                mesh.BoundKnots[k++] = (int)map[Nx - 1, i];
            }
            // правая сторона
            for (int i = 0; i < Nx - 1; i++)
            {
                mesh.BoundElems[k] = new TwoElement(map[Nx - 1 - i, Ny - 1], map[Nx - 2 - i, Ny - 1]);
                mesh.BoundElementsMark[k] = Flag[1];
                // задана производная
                mesh.BoundKnotsMark[k] = Flag[1];
                mesh.BoundKnots[k++] = (int)map[Nx - 1 - i, Ny - 1];
            }
            // верх
            for (int i = 0; i < Ny - 1; i++)
            {
                mesh.BoundElems[k] = new TwoElement(map[0, Ny - i - 1], map[0, Ny - i - 2]);
                mesh.BoundElementsMark[k] = Flag[2];
                // задана производная
                mesh.BoundKnotsMark[k] = Flag[2];
                mesh.BoundKnots[k++] = (int)map[0, Ny - i - 1];
            }
            // левая сторона
            for (int i = 0; i < Nx - 1; i++)
            {
                mesh.BoundElems[k] = new TwoElement(map[i, 0], map[i + 1, 0]);
                mesh.BoundElementsMark[k] = Flag[3];
                // задана функция
                mesh.BoundKnotsMark[k] = Flag[3];
                mesh.BoundKnots[k++] = (int)map[i, 0];
            }

        }
       
        //public class Rebro
        //{
        //    public uint a;
        //    public uint b;
        //    public uint elem;
        //    public uint Count;
        //    public Rebro(uint A, uint B, uint elem)
        //    {
        //        a = (A > B) ? B : A;
        //        b = (A < B) ? B : A;
        //        this.elem = elem;
        //        Count = 1;
        //    }
        //    public static bool operator ==(Rebro ra, Rebro rb)
        //    {
        //        return ra.a == rb.a && ra.b == rb.b;
        //    }
        //    public static bool operator !=(Rebro ra, Rebro rb)
        //    {
        //        return ra.a != rb.a || ra.b != rb.b;
        //    }
        //}
        //public class Rebros
        //{
        //    List<Rebro> rebs = new List<Rebro>();
        //    public void Add(uint a, uint b, uint elem)
        //    {
        //        Rebro r = new Rebro(a, b, elem);
        //        bool flag = false;
        //        foreach (var rb in rebs)
        //        {
        //            if (rb == r)
        //            {
        //                rb.Count++;
        //                if (rb.Count > 2)
        //                {
        //                    Console.WriteLine(" elem1 = {0} ребро {1} - {2} и elem1 = {3} ребро {4} - {5}",
        //                        rb.elem, rb.a, rb.b, r.elem, r.a, r.b);
        //                }
        //                flag = true;
        //            }
        //        }
        //        if (flag == false)
        //            rebs.Add(r);
        //    }
        //    public void Report()
        //    {
        //        int k = 0;
        //        using (StreamWriter fileRep = new StreamWriter("breb.txt"))
        //        {
        //            foreach (var rb in rebs)
        //            {
        //                if (rb.Count > 2)
        //                {
        //                    fileRep.WriteLine(" elem1 = {0} ребро {1} - {2} Count {3} k =",
        //                        rb.elem, rb.a, rb.b, rb.Count, k);
        //                }
        //                k++;
        //            }
        //        }
        //    }
        //}
    }
}
