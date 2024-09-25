//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 10.08.2021 Потапов И.И.
//---------------------------------------------------------------------------
//             обновление и чистка : 01.01.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace TriMeshGeneratorLib
{
    using CommonLib.Mesh.RVData;
    using System;
    using System.Globalization;
    using System.IO;
    class RVCdgIOut
    {
        /// <summary>
        /// Загружаемая сетка задачи
        /// </summary>
        protected RVMeshIrregular theMesh = null;
        /// <summary>
        /// Граничные сегменты на которых определены потоки/глубина
        /// </summary>
        protected RVList FlowBoundList = new RVList();
        protected RVBoundary outSegP = new RVBoundary();
        protected RVBoundary inSegP = new RVBoundary();

        public static string F4 = "F4";
        public static string F8 = "F8";
        /// <summary>
        /// Шапка cdg файла
        /// </summary>
        protected RVControl rvControl = new RVControl();
        protected RVTransient tvals = new RVTransient();
        
        protected double wsElevOut = 0, wsElevIn = 0;
        NumberFormatInfo formatter = new NumberFormatInfo { NumberDecimalSeparator = "." };
        public RVCdgIOut(RVMeshIrregular mesh, double elIn= -100.0)
        {
            theMesh = mesh;
            wsElevIn = elIn;
            rvControl.trans = 1;
            rvControl.meshtype = 1;
            tvals.nsteps = 1;
            tvals.dtfac = 1.0;
            tvals.time = 0.0;
            tvals.dtime = 10.0;
            tvals.theta = 1.0;
            tvals.UW = 0.5;
            tvals.DIF = 0.5;
            tvals.epsilon1 = 0;
            tvals.epsilon3 = 0;
            tvals.latitude = 0.0;
            tvals.S = 1.0;
            tvals.diffusivewave = 0;
            tvals.uwJ = 0.0;
            tvals.plotcode = 2;
            tvals.transbcs = 0;
            tvals.maxitnum = 9;   // changed from 9 to 100 steve kwan 29 april 2010 
            tvals.smallH = 1;
            tvals.JE = 1;
            tvals.minH = 0.01;
            tvals.gwH = 0.01;
            tvals.GWD = 0.1;
            tvals.T = 0.1;
            rvControl.dims = 2;
            rvControl.CountVars = 3;
            for (int ii = 0; ii < ((rvControl.CountVars + 1) * rvControl.CountVars); ii++)
                rvControl.Keqns[ii] = 1;
            rvControl.knotParams = 3;
            rvControl.bparams = 7;
            rvControl.CountNodes = mesh.CountNodes;
            rvControl.CountElements = 0;
            rvControl.CountBoundElements = 0;
            rvControl.CountBoundSegs = mesh.CountBoundarySegments; 
            FlowBoundList = ((RVMeshRiver)theMesh).FlowBoundList;
        }
        protected double GetWsElev()
        {
            int countOut = 0;
            double elOut = 0.0;
            RVBoundary bSegment = (RVBoundary)theMesh.FirstBoundarySegment;
            while (bSegment != null)
            {
                if ((bSegment.bcCode == 3) || (bSegment.bcCode == 5))
                {
                    outSegP = bSegment;
                    countOut += 1;
                    elOut += bSegment.BcValue;
                }
                else if (bSegment.bcCode == 1)
                {
                    inSegP = bSegment;
                }
                bSegment = (RVBoundary)theMesh.NextBoundarySegment;
            }
            if (countOut > 0)
                return elOut / countOut;
            else
                return 0.0;
        }
        public RVFlowBoundary FirstFBound() { return (RVFlowBoundary)FlowBoundList.FirstItem(); }
        public RVFlowBoundary lastFBound() { return (RVFlowBoundary)FlowBoundList.LastItem(); }
        public RVFlowBoundary nextFBound() { return (RVFlowBoundary)FlowBoundList.NextItem(); }

        protected int GetInt(StreamReader file)
        {
            int val = 0;
            string line = file.ReadLine().Trim();
            string[] sline = line.Split(' ');
            for (int i = 0; i < sline.Length; i++)
            {
                if (sline[i].Trim() == "=")
                {
                    val = int.Parse(sline[i + 1].Trim());
                    break;
                }
            }
            return val;
        }
        protected double GetDouble(StreamReader file)
        {
            double val = 0;
            string line = file.ReadLine().Trim();
            string[] sline = line.Split(' ');
            for (int i = 0; i < sline.Length; i++)
            {
                if (sline[i].Trim() == "=")
                {
                    val = double.Parse(sline[i + 1].Trim(), formatter);
                    break;
                }
            }
            return val;
        }
        protected void ReadRVControl(StreamReader f)
        {
            rvControl.trans = GetInt(f);
            rvControl.meshtype = GetInt(f);
            if (rvControl.trans == 1)
            {
                tvals.nsteps = GetInt(f);
                tvals.dtfac = GetDouble(f);
                tvals.time = GetDouble(f);
                tvals.dtime = GetDouble(f);
                tvals.theta = GetDouble(f);
                tvals.UW = GetDouble(f);
                tvals.DIF = GetDouble(f);
                tvals.latitude = GetDouble(f);
                tvals.S = GetDouble(f);
                tvals.diffusivewave = GetInt(f);
                tvals.uwJ = GetDouble(f);
                tvals.plotcode = GetInt(f);
                tvals.transbcs = GetInt(f);
                tvals.maxitnum = GetInt(f);
                tvals.smallH = GetInt(f);
                tvals.JE = GetInt(f);
                //      tvals.minH = GetDouble(f);
                tvals.epsilon1 = GetDouble(f);
                tvals.gwH = GetDouble(f);
                //      tvals.GWD = GetDouble(f);
                tvals.epsilon3 = GetDouble(f);
                tvals.T = GetDouble(f);
            }
            rvControl.dims = GetInt(f);
            rvControl.CountVars = GetInt(f);
            rvControl.Keqns = new int[(rvControl.CountVars + 1) * rvControl.CountVars];
            f.ReadLine();
            int i = 0;
            for (int k = 0; k < rvControl.CountVars; k++)
            {
                string line = f.ReadLine().Trim();
                string[] sline = line.Split(' ', '\t');
                for (int m = 0; m < sline.Length; m++)
                    if (sline[m] != "")
                        rvControl.Keqns[i++] = int.Parse(sline[0].Trim());

            }
            rvControl.knotParams = GetInt(f);
            rvControl.bparams = GetInt(f);
            rvControl.CountNodes = GetInt(f);
            rvControl.CountElements = GetInt(f);
            rvControl.CountBoundElements = GetInt(f);
            rvControl.CountBoundSegs = GetInt(f);
        }
        protected void WriteRVControl(StreamWriter file)
        {
            file.WriteLine(" RVTransient analysis = " + rvControl.trans.ToString());
            file.WriteLine(" Mesh type = " + rvControl.meshtype.ToString());
            file.WriteLine(" Number of Time Steps = " + tvals.nsteps.ToString());
            file.WriteLine(" Delta t Acceleration Factor = " + tvals.dtfac.ToString());
            file.WriteLine(" Time = " + tvals.time.ToString());
            file.WriteLine(" Delta t = " + tvals.dtime.ToString());
            file.WriteLine(" Theta = " + tvals.theta.ToString());
            file.WriteLine(" UW = " + tvals.UW.ToString());
            file.WriteLine(" Eddy Viscosity Bed Shear Parameter = " + tvals.DIF.ToString());
            file.WriteLine(" Latitude  = " + tvals.latitude.ToString());
            file.WriteLine(" Groundwater Storativity = " + tvals.S.ToString());
            file.WriteLine(" Diffusive wave Solution = " + tvals.diffusivewave.ToString() + " zero for fully dynamic only");
            file.WriteLine(" UW Jacobian terms included = " + tvals.uwJ.ToString() + " zero for not included");
            file.WriteLine(" Plot Code = " + tvals.plotcode.ToString() + " zero for xsec one for contour two for velocity and three for threeD");
            file.WriteLine(" RVTransient Boundary Condition = " + tvals.transbcs.ToString() + " zero for Steady BCs");
            file.WriteLine(" Maximum Number of Iterations = " + tvals.maxitnum.ToString());
            file.WriteLine(" Small Depths Occur = " + tvals.smallH.ToString() + " zero for no small depth calculations");
            file.WriteLine(" Jacobian Terms included = " + tvals.JE.ToString() + " zero for not included");
            file.WriteLine(" Eddy Viscosity Constant = " + tvals.epsilon1.ToString());
            file.WriteLine(" Minimum Depth for Groundwater Flow Calculation = " + tvals.gwH.ToString());
            file.WriteLine(" Eddy Viscosity Horizontal Shear Paramter = " + tvals.epsilon3.ToString());
            file.WriteLine(" Groundwater Transmissivity = " + tvals.T.ToString());
            file.WriteLine(" Dimensions = " + rvControl.dims.ToString());
            file.WriteLine(" Number of Variables = " + rvControl.CountVars.ToString());
            file.WriteLine(" [K] governing equation numbers");
            for (int i = 0; i < 3; i++)
            {
                for (int ii = 0; ii < 3; ii++)
                    file.Write(" 1");
                file.WriteLine();
            }
            // rvControl.knotParams = GetInt(f);
            file.WriteLine(" Number of Parameters = " + rvControl.knotParams.ToString());
            // rvControl.bparams = GetInt(f);
            file.WriteLine(" Number of Boundary Parameters = " + rvControl.bparams.ToString());
            // rvControl.CountNodes = GetInt(f);
            file.WriteLine(" Number of Nodes = " + theMesh.CountNodes.ToString());
            // rvControl.CountElements = GetInt(f);
            RVTriangle currentFElement = theMesh.FirstTriElements;
            int CountElements = 0;
            while (currentFElement != null)
            {
                if (currentFElement.Status == StatusFlag.Activate)
                    CountElements += 1; ;
                currentFElement = theMesh.NextTriElements;
            }
            file.WriteLine(" Number of Elements = " + CountElements.ToString());
            // rvControl.CountBoundElements = GetInt(f);
            file.WriteLine(" Number of  Boundary Elements = " + theMesh.CountBoundarySegments.ToString());
            // rvControl.CountBoundSegs = GetInt(f);
            file.WriteLine(" Number of  Boundary Segments = " + rvControl.CountBoundSegs.ToString());
        }

        //protected int GetInt(ifstream& f);
        //protected double GetDouble(ifstream& f);
        /// <summary>
        /// output
        /// </summary>
        /// <param name="file"></param>
        public void WriteFileCDG(StreamWriter file)
        {
            RVNodeShallowWater elemNodes;
            RVTriangle currentFElement;
            RVBoundary currentBoundFElement;
            WriteRVControl(file);
            file.WriteLine(" RVNode Information ");
            file.WriteLine(" RVNode #, Coordinates, Parameters, Variables");
            wsElevOut = GetWsElev();
            
            elemNodes = (RVNodeShallowWater)theMesh.firstNode;
            while (elemNodes != null)
            {
                WriteNodes(file, elemNodes);
                elemNodes = (RVNodeShallowWater)theMesh.NextNode;
            }

            file.WriteLine(" RVElement Information ");
            file.WriteLine(" RVElement #, vtype, gtype, CountNodes");
            currentFElement = theMesh.FirstTriElements;
            while (currentFElement != null)
            {
                WriteElems(file, currentFElement);
                currentFElement = theMesh.NextTriElements;
            }

            file.WriteLine(" Boundary RVElement #, vtype, gtype, CountNodes, boundary condition codes");
            currentBoundFElement = (RVBoundary)theMesh.FirstBoundarySegment;
            while (currentBoundFElement != null)
            {
                WriteBoundElems(file, currentBoundFElement);
                currentBoundFElement = (RVBoundary)theMesh.NextBoundarySegment;
            }

            file.WriteLine(" Boundary Seg #,Boundary type,stage,QT,start node #,end node #");
            WriteBoundSegms(file);

            theMesh.WriteBreakLinesList(file);

            return;
        }
        /// <summary>
        /// put_node
        /// </summary>
        /// <param name="file"></param>
        /// <param name="nP"></param>
        void WriteNodes(StreamWriter file, RVNodeShallowWater nP)
        {
            file.Write(nP.ID.ToString() + "  ");
            if (nP.Fixed == RVFixedNodeFlag.fixedNode)
                file.Write(" x ");
            else if (nP.Fixed == RVFixedNodeFlag.slidingNode)
                file.Write(" s ");
            else
                file.Write("   ");

            file.Write(nP.Xo.ToString(F8) + "  ");
            file.Write(nP.Yo.ToString(F8) + "  ");
            file.Write(nP.Z.ToString(F4) + "  ");
            file.Write(nP.Ks.ToString(F4) + "  ");
            file.Write(" 10 ");
            if (wsElevOut <= 0.0)
            {
                file.Write(wsElevIn.ToString(F4));
                file.Write(" 0.0 ");
                file.Write(" 0.0 ");
            }
            else if (wsElevIn > 0.0)
            {
                double dout = Math.Abs(outSegP.whichSide(nP)) / outSegP.length();
                double din = Math.Abs(inSegP.whichSide(nP)) / inSegP.length();
                double r = dout / (din + dout);
                file.Write((r * wsElevIn + (1 - r) * wsElevOut - (nP.Z)).ToString(F4) + "  ");
                file.Write("  0.0  0.0 ");
            }
            else
            {
                file.Write(nP.Depth.ToString(F4) + "  ");    // depth
                file.Write(nP.GetPapam(4).ToString(F4) + "  "); // qx
                file.Write(nP.GetPapam(5).ToString(F4) + "  "); // qy
            }
            file.WriteLine();
        }

        void WriteElems(StreamWriter file, RVTriangle tP)
        {
            if (tP.Status == StatusFlag.Activate)
            {
                file.Write(tP.ID.ToString() + "  ");
                file.Write(" 210  210 ");
                file.Write(tP.GetNode(0).ID.ToString() + "  ");
                file.Write(tP.GetNode(1).ID.ToString() + "  ");
                file.Write(tP.GetNode(2).ID.ToString() + "  ");
                file.Write(" 0.0  0.0  0.0  ");
                file.WriteLine("Activate");
            }
            else
            {
                file.Write(tP.ID.ToString() + "  ");
                file.Write(" 210  210 ");
                file.Write(tP.GetNode(0).ID.ToString() + "  ");
                file.Write(tP.GetNode(1).ID.ToString() + "  ");
                file.Write(tP.GetNode(2).ID.ToString() + "  ");
                file.Write(" 0.0  0.0  0.0  ");
                file.WriteLine("noActive");
            }
        }

        static int CountPrint = 0;
        /// <summary>
        /// put_elm
        /// </summary>
        /// <param name="tP"></param>
        /// <param name="Flag"></param>
        /// <param name="Count"></param>
        public static void WriteElems(RVTriangle tP, bool Flag = true, int Count = 0)
        {
            if (Flag == true)
                CountPrint++;
            else
                CountPrint = 0;
            if (tP == null)
            {
                Console.WriteLine("Ссылка пуста");
                return;
            }
            else
            {
                Console.WriteLine(" Трехугольник " + tP.ID.ToString() + "  ");
                Console.WriteLine();
                Console.Write(tP.GetNode(0).ID.ToString() + "  ");
                Console.Write(tP.GetNode(1).ID.ToString() + "  ");
                Console.Write(tP.GetNode(2).ID.ToString() + "  ");
                Console.Write(tP.Area().ToString("F5") + "  ");
                Console.WriteLine();
                RVTriangle t0 = tP.tP0;
                if (t0 == null) Console.WriteLine("Ссылка пуста");
                else
                {
                    int i = 0;
                    Console.WriteLine();
                    Console.WriteLine("суб Трехугольник 1" + t0.ID.ToString() + "  ");
                    var v = t0.GetNode(0);
                    if (v == null) Console.WriteLine("Узел пуст");
                    else { Console.Write(v.ID.ToString() + "  "); i++; }
                    v = t0.GetNode(1);
                    if (v == null) Console.WriteLine("Узел пуст");
                    else { Console.Write(v.ID.ToString() + "  "); i++; }
                    v = t0.GetNode(2);
                    if (v == null) Console.WriteLine("Узел пуст");
                    else { Console.Write(v.ID.ToString() + "  "); i++; }
                    if (i == 3)
                        Console.Write(t0.Area().ToString("F5") + "  ");
                    Console.WriteLine("граничный КЭ");
                }
                t0 = tP.tP1;
                if (t0 == null) Console.WriteLine("Ссылка пуста");
                else
                {
                    int i = 0;
                    Console.WriteLine("суб Трехугольник 2" + t0.ID.ToString() + "  ");
                    Console.WriteLine();
                    var v = t0.GetNode(0);
                    if (v == null) Console.WriteLine("Узел пуст");
                    else { Console.Write(v.ID.ToString() + "  "); i++; }
                    v = t0.GetNode(1);
                    if (v == null) Console.WriteLine("Узел пуст");
                    else { Console.Write(v.ID.ToString() + "  "); i++; }
                    v = t0.GetNode(2);
                    if (v == null) Console.WriteLine("Узел пуст");
                    else { Console.Write(v.ID.ToString() + "  "); i++; }
                    if (i == 3)
                        Console.Write(t0.Area().ToString("F5") + "  ");
                    Console.WriteLine("граничный КЭ");
                }
                t0 = tP.tP2;
                if (t0 == null) Console.WriteLine("Ссылка пуста");
                else
                {
                    int i = 0;
                    Console.WriteLine("суб Трехугольник 3" + t0.ID.ToString() + "  ");
                    Console.WriteLine();
                    var v = t0.GetNode(0);
                    if (v == null) Console.WriteLine("Узел пуст");
                    else { Console.Write(v.ID.ToString() + "  "); i++; }
                    v = t0.GetNode(1);
                    if (v == null) Console.WriteLine("Узел пуст");
                    else { Console.Write(v.ID.ToString() + "  "); i++; }
                    v = t0.GetNode(2);
                    if (v == null) Console.WriteLine("Узел пуст");
                    else { Console.Write(v.ID.ToString() + "  "); i++; }
                    if (i == 3)
                        Console.Write(t0.Area().ToString("F5") + "  ");
                    Console.WriteLine("граничный КЭ");
                }

            }
            Console.WriteLine();
        }
        /// <summary>
        /// put_bsegs
        /// </summary>
        /// <param name="file"></param>
        /// <param name="bSegment"></param>
        void WriteBoundElems(StreamWriter file, RVBoundary bSegment)
        {
            file.Write(bSegment.ID.ToString() + "  ");
            file.Write("111  111  ");
            file.Write(bSegment.GetNode(0).ID.ToString() + "  ");
            file.Write(bSegment.GetNode(1).ID.ToString() + "  ");
            if (bSegment.getBcCode() == 0)
                file.Write(" 0\t0\t0\t0\t0\t0\t0\t0\t0\t0 ");
            else if (bSegment.getBcCode() == 1)
                file.Write(" 0 " + (bSegment.BcValue).ToString(F4) + " 0\t0\t0\t0\t0\t1\t0\t0 ");
            else if (bSegment.getBcCode() == 3)
                file.Write(bSegment.BcValue + " 0\t0\t0\t0\t0\t0\t3\t0\t0 ");
            else if (bSegment.getBcCode() == 5)
                file.Write(bSegment.BcValue.ToString(F4) + " " + bSegment.BcValueTwo.ToString(F4) + " 0\t0\t0\t0\t0\t5\t0\t0 ");
            file.WriteLine();
        }
        /// <summary>
        /// WriteBoundElems
        /// </summary>
        /// <param name="file"></param>
        void WriteBoundSegms(StreamWriter file)
        {
            RVFlowBoundary fBP = FirstFBound();
            while (fBP != null)
            {
                file.Write(fBP.ID.ToString() + "  ");
                file.Write(fBP.getBcCode().ToString() + "  ");
                file.Write(fBP.getBcValue().ToString() + "  ");
                file.Write(fBP.getBcValue2().ToString() + "  ");
                file.Write((((RVNode)fBP.getStartNode()).ID).ToString() + "  ");
                file.Write(((RVNode)fBP.getEndNode()).ID.ToString() + "  ");
                if (fBP.getTransCode() == 0)
                    file.Write("  ");
                else
                    file.Write("  " + fBP.getFilePath().ToString() + "  ");
                file.WriteLine();
                fBP = nextFBound();
            }
        }
        /// <summary>
        /// Чтение cdg файла
        /// </summary>
        public void ReadFileCDG(StreamReader file)
        {
            ReadRVControl(file);
            ((RVMeshRiver)theMesh).ReadBodyCDG(file, rvControl);
            FlowBoundList = ((RVMeshRiver)theMesh).FlowBoundList;
        }

        #region Функционал задачи - для тестов и здесь не использован
        public double gettheta() { return tvals.theta; }
        public double getUW() { return tvals.UW; }
        public double getDIF() { return tvals.DIF; }
        public double getgwH() { return tvals.gwH; }
        public double getT() { return tvals.T; }
        public double getS() { return tvals.S; }
        public double getEpsilon1() { return tvals.epsilon1; }
        public double getEpsilon3() { return tvals.epsilon3; }
        public int NFlowBounds() { return rvControl.CountBoundSegs; }

        protected int num_bsegs()
        {
            int Nseg = 0, oldBcCode = 0;//, count;
                                        //double sum = 0.0;

            RVBoundary bSegment = (RVBoundary)theMesh.FirstBoundarySegment;
            while (bSegment != null)
            {
                if (bSegment.ID != oldBcCode)
                {
                    if (oldBcCode > 0)
                    {
                        Nseg += 1;
                    }
                    oldBcCode = bSegment.ID;
                }
                bSegment = (RVBoundary)theMesh.NextBoundarySegment;
            }
            if (oldBcCode > 0)
            {
                Nseg += 1;
            }
            return Nseg;
        }

        public void setUW(double upwinding)
        {
            tvals.UW = upwinding;
        }
        public void setDIF(double diffusivity)
        {
            tvals.DIF = diffusivity;
        }
        public void setgwH(double gwHeight)
        {
            tvals.gwH = gwHeight;
        }
        public void setT(double transmissivity)
        {
            tvals.T = transmissivity;
        }
        public void setS(double storativity)
        {
            tvals.S = storativity;
        }
        public void setEpsilon1(double Epsilon1)
        {
            tvals.epsilon1 = Epsilon1;
        }
        public void setEpsilon3(double Epsilon3)
        {
            tvals.epsilon3 = Epsilon3;
        }

        public void appendFBound(RVFlowBoundary fBP)
        {
            FlowBoundList.Add(fBP);
            FlowBoundList.BuildIndex();
            rvControl.CountBoundSegs++;
        }
        public void removeFBound(RVFlowBoundary fBP)
        {
            FlowBoundList.Remove(fBP);
            FlowBoundList.BuildIndex();
            rvControl.CountBoundSegs--;
            fBP = FirstFBound();
            int i = 1;
            while (fBP != null)
            {
                fBP.ID = i;
                i++;
                fBP = nextFBound();
            }
        }

        public void SetBElementFlowBound(RVFlowBoundary fBP, RVNode startNP, RVNode endNP)
        {
            RVBoundary segP;
            segP = (RVBoundary)theMesh.FirstBoundarySegment;
            if (segP == null)
                return;
            while (segP.GetNode(0) != startNP)
            {
                segP = (RVBoundary)theMesh.NextBoundarySegment;
                if (segP == null)
                    segP = (RVBoundary)theMesh.FirstBoundarySegment;
            }
            segP.FlowBound = fBP;
            while (segP.GetNode(1) != endNP)
            {
                segP = (RVBoundary)theMesh.NextBoundarySegment;
                if (segP == null)
                    segP = (RVBoundary)theMesh.FirstBoundarySegment;
                segP.FlowBound = fBP;
            }
            return;
        }
        /// <summary>
        /// Не используется генератором
        /// </summary>
        public void BuildFlowBoundaries()
        {
            int Nseg = 0, oldBcCode = 0, count = 0;
            double sum = 0.0;
            RVBoundary bSegment;
            RVBoundary startBP = new RVBoundary();
            RVBoundary oldBP = new RVBoundary();
            RVFlowBoundary fBP;
            bSegment = (RVBoundary)theMesh.FirstBoundarySegment;
            while (bSegment != null)
            {
                if (bSegment.getBcCode() != oldBcCode)
                {
                    if (oldBcCode == 1)
                    {
                        Nseg += 1;
                        fBP = new RVFlowBoundary(Nseg, startBP.GetNode(0), oldBP.GetNode(1), 1, 0, sum);
                        FlowBoundList.Add(fBP);
                        rvControl.CountBoundSegs++;
                    }
                    if (oldBcCode == 3)
                    {
                        Nseg += 1;
                        fBP = new RVFlowBoundary(Nseg, startBP.GetNode(0), oldBP.GetNode(1), 3, startBP.BcValue, 0);
                        FlowBoundList.Add(fBP);
                        rvControl.CountBoundSegs++;
                    }
                    if (oldBcCode == 5)
                    {
                        Nseg += 1;
                        fBP = new RVFlowBoundary(Nseg, startBP.GetNode(0), oldBP.GetNode(1), 5, startBP.BcValue, startBP.BcValueTwo);
                        FlowBoundList.Add(fBP);
                        rvControl.CountBoundSegs++;
                    }
                    oldBcCode = bSegment.ID;
                    startBP = bSegment;
                    count = 0;
                    sum = 0.0;
                }
                count += 1;
                sum += bSegment.BcValue * bSegment.length();
                oldBP = bSegment;
                bSegment = (RVBoundary)theMesh.NextBoundarySegment;
            }
            if (oldBcCode == 1)
            {
                Nseg += 1;
                fBP = new RVFlowBoundary(Nseg, startBP.GetNode(0), oldBP.GetNode(1), 1, 0, sum);
                FlowBoundList.Add(fBP);
                rvControl.CountBoundSegs++;
            }
            if (oldBcCode == 3)
            {
                Nseg += 1;
                fBP = new RVFlowBoundary(Nseg, startBP.GetNode(0), oldBP.GetNode(1), 3, startBP.BcValue, 0);
                FlowBoundList.Add(fBP);
                rvControl.CountBoundSegs++;
            }
            if (oldBcCode == 5)
            {
                Nseg += 1;
                fBP = new RVFlowBoundary(Nseg, startBP.GetNode(0), oldBP.GetNode(1), 5, startBP.BcValue, startBP.BcValueTwo);
                FlowBoundList.Add(fBP);
                rvControl.CountBoundSegs++;
            }
        }

        public void sett(double time)
        {
            tvals.time = time;
        }
        public void setdt(double deltat)
        {
            tvals.dtime = deltat;
        }
        public void setmaxitnum(int maxitnum)
        {
            tvals.maxitnum = maxitnum;
        }
        public void settheta(double theta)
        {
            tvals.theta = theta;
        }
        //protected void getElm(StreamReader f)
        //{
        //    string[] lines = GetLines(f, 9);
        //    int n = int.Parse(lines[0].Trim());
        //    int typeFFV = int.Parse(lines[1].Trim());
        //    int typeFFL = int.Parse(lines[2].Trim());
        //    // сдвиг на 1 нумерация узлов идет с нуля
        //    uint nn1 = uint.Parse(lines[3].Trim());
        //    uint nn2 = uint.Parse(lines[4].Trim());
        //    uint nn3 = uint.Parse(lines[5].Trim());
        //    return;
        //}

        //protected void GetNode(StreamReader f)
        //{
        //    string[] lines = GetLines(f, 8);
        //    if (lines == null) return;
        //    int idx = 0;
        //    int n = int.Parse(lines[idx++].Trim());
        //    string s = lines[idx].Trim();
        //    RVFixedNodeFlag fx = RVFixedNodeFlag.floatingNode;
        //    if (s == "x" || s == "s")
        //    {
        //        if (s == "x")
        //            fx = RVFixedNodeFlag.fixedNode;   // x  фиксированный узел
        //        else
        //            fx = RVFixedNodeFlag.slidingNode;  // s slidingNode - скользящий узел (по границе/ или линии)
        //        idx++;
        //    }
        //    double x = double.Parse(lines[idx++].Trim(), formatter);
        //    double y = double.Parse(lines[idx++].Trim(), formatter);
        //    double z = double.Parse(lines[idx++].Trim(), formatter);

        //    double k = double.Parse(lines[idx++].Trim(), formatter);
        //    double weight = double.Parse(lines[idx++].Trim(), formatter);
        //    double d = double.Parse(lines[idx++].Trim(), formatter);
        //    double qx = double.Parse(lines[idx++].Trim(), formatter);
        //    double qy = double.Parse(lines[idx++].Trim(), formatter);

        //    RVNodeHabitat nP = new RVNodeHabitat((RVHabitatPhysics)theMesh.GetPhysics(), n, x, y, z, k, 5.0, d, qx, qy);
        //    if (fx == RVFixedNodeFlag.fixedNode)
        //        nP.RVFixedNodeFlag.fixedNode;
        //    if (fx == RVFixedNodeFlag.slidingNode)
        //        nP.Fixed = RVFixedNodeFlag.slidingNode;
        //    theMesh.AddNode(nP);
        //    return;
        //}

        //protected void getBSeg(StreamReader f)
        //{
        //    string[] lines = GetLines(f, 6);
        //    int n = int.Parse(lines[0].Trim());
        //    int code = int.Parse(lines[1].Trim());
        //    double p0 = double.Parse(lines[2].Trim(), formatter);
        //    double p1 = double.Parse(lines[3].Trim(), formatter);
        //    int startnode = int.Parse(lines[4].Trim());
        //    int endnode = int.Parse(lines[5].Trim());
        //    string filePath = "";
        //    if (lines.Length == 7)
        //        filePath = lines[6].Trim();
        //    int transCode;
        //    if (filePath != "")
        //        transCode = 0;
        //    else
        //        transCode = 1;
        //    RVNode start = theMesh.GetNodeByID(startnode);
        //    RVNode end = theMesh.GetNodeByID(endnode);
        //    RVFlowBoundary flowBP = new RVFlowBoundary(n, start, end, code, p0, p1, transCode, filePath);
        //    FlowBoundList.Add(flowBP);
        //    SetBElementFlowBound(flowBP, start, end);
        //    if (filePath != "")
        //        using (StreamReader file = new StreamReader(filePath))
        //        {
        //            if (file != null)
        //                flowBP.loadTransLst(file);
        //        }
        //}

        //protected void getBElm(StreamReader f)
        //{
        //    string[] lines = GetLines(f, 15);
        //    int n = int.Parse(lines[0].Trim());
        //    int typeBFV = int.Parse(lines[1].Trim());
        //    int typeBFL = int.Parse(lines[2].Trim());

        //    int nn1 = int.Parse(lines[3].Trim());
        //    int nn2 = int.Parse(lines[4].Trim());
        //    double p1 = double.Parse(lines[5].Trim(), formatter);
        //    double p2 = double.Parse(lines[6].Trim(), formatter);
        //    int c1 = int.Parse(lines[12].Trim());
        //    RVNode n1P = theMesh.GetNodeByID(nn1);
        //    RVNode n2P = theMesh.GetNodeByID(nn2);
        //    double bValue = 0.0;
        //    if (c1 == 1)
        //        bValue = p2;
        //    else if (c1 == 3)
        //        bValue = p1;
        //    RVBoundary bSeg = new RVBoundary(n, n1P, n2P, c1, bValue);
        //    theMesh.AddBoundarySegment(bSeg);
        //    return;
        //}

        ///// <summary>
        ///// читать сегменты функций
        ///// </summary>
        ///// <param name="file"></param>
        ///// <returns></returns>
        //public int ReadBreakLinesList(StreamReader file)
        //{
        //    RVSegment segP;
        //    RVNode nodeP1;
        //    RVNode nodeP2;
        //    for (; ; )
        //    {
        //        string[] lines = GetLines(file, 3);
        //        if (lines == null)
        //            break;
        //        if (lines.Length != 3)
        //            break;
        //        int name = int.Parse(lines[0].Trim());
        //        int n1 = int.Parse(lines[0].Trim());
        //        int n2 = int.Parse(lines[0].Trim());
        //        nodeP1 = (RVNode)theMesh.nodesList.n(n1);
        //        nodeP2 = (RVNode)theMesh.nodesList.n(n2);
        //        segP = new RVSegment(name, nodeP1, nodeP2);
        //        theMesh.AddFeatureSeg(segP, theMesh.breakLinesList);
        //    }
        //    return theMesh.breakLinesList.Count();
        //}

        #endregion
    }
}
