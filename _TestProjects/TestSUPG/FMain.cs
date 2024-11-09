namespace TestSUPG
{
    using MeshLib;
    using RenderLib;
    using GeometryLib;
    using MeshGeneratorsLib;

    using CommonLib;
    using CommonLib.Mesh;
    using CommonLib.Geometry;

    using System;
    using MemLogLib;
    using AlgebraLib;
    using System.Windows.Forms;
    using System.Collections.Generic;
    using MeshGeneratorsLib.StripGenerator;
    using FEMTasksLib.FESimpleTask;
    using MeshLib.Wrappers;
    using CommonLib.Physics;
    using System.Linq;
    using MeshLib.FEMTools.FunForm;
    using CommonLib.Function;
    using MeshGeneratorsLib.Renumberation;

    public partial class FMain : Form
    {
        IMesh mesh = null;

        double H = 0;
        double L = 0;
        double diametrFE,dx,dy;
        double[] x = null;
        double[] y = null;
        double WL = 1.7;

        double[] YRose2 = { 0, 0.11965, 0.405625, 0.634625, 0.804625, 0.974625, 1.203625, 1.489625, 1.60925 };
        double[] ZRose2 = { 0.1401, 0.1, 0.028625, 0, 0, 0, 0.028625, 0.1, 0.1401 };
        IDigFunction Geometry = null;
        public FMain()
        {
            InitializeComponent();
            lb_Ring.SelectedIndex = 1;
            listBoxRange.SelectedIndex = 0;
            listBoxTypeMesh.SelectedIndex = 0;
            listBoxFrontRen.SelectedIndex = 0;
            listBoxTasks.SelectedIndex = 5;
            listBoxArea.SelectedIndex = 1;
            listBoxRiverTask.SelectedIndex = 7;
            listBoxAMu.SelectedIndex = 5;
            lb_VortexBC_G2.SelectedIndex = 1;
            lb_VetrexTurbTask.SelectedIndex = 1;
            lb_CrossNamber.SelectedIndex = 3;
            lb_Algebra.SelectedIndex = 0;
            ls_Type__U_star.SelectedIndex = 0;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            GetMesh();
        }

        protected void GetHLD()
        {
            H = double.Parse(textBoxH.Text, MEM.formatter);
            L = double.Parse(textBoxL.Text, MEM.formatter);
            double PFE = double.Parse(textBoxDiam.Text, MEM.formatter);
            diametrFE = Math.Min(L, H) / 100 * PFE;
        }

        protected void QUAD_Mesh()
        {
            TypeMesh[] meshTypes = { TypeMesh.MixMesh, TypeMesh.Triangle, TypeMesh.Rectangle };
            TypeMesh meshType = meshTypes[listBoxTypeMesh.SelectedIndex];
            TypeRangeMesh MeshRange = (TypeRangeMesh)(listBoxRange.SelectedIndex + 1);
            //int meshMethod = 0;
            int meshMethod = 1;
            int reNumberation = cb_renamber.Checked == true ? 1 : 0;
            double RelaxMeshOrthogonality = 0.1;
            int CountParams = 0;
            bool flagMidle = true;

            Direction direction = (Direction)listBoxFrontRen.SelectedIndex;

            // Данные для сетки
            HMeshParams meshData = new HMeshParams(meshType, MeshRange, meshMethod,
                      new HPoint(diametrFE, diametrFE), RelaxMeshOrthogonality, reNumberation,
                      direction, CountParams, flagMidle);
            HMapSegment[] mapSegment = new HMapSegment[4];
            // количество параметров на границе (задан 1)
            double[] param0 = { 5, 5 };
            double[] param1 = { 2, 2 };
            double[] param2 = { 3, 3 };
            double[] param3 = { 4, 6 };
            // определение вершин
            VMapKnot p0 = new VMapKnot(0, 0, param0);
            VMapKnot p1 = new VMapKnot(L, 0, param1);
            VMapKnot p2 = new VMapKnot(L, H, param2);
            VMapKnot p3 = new VMapKnot(0, H, param3);
            // определение ребер
            List<VMapKnot> facet0 = new List<VMapKnot>() { new VMapKnot(p0), new VMapKnot(p1) };
            List<VMapKnot> facet1 = new List<VMapKnot>() { new VMapKnot(p1), new VMapKnot(p2) };
            List<VMapKnot> facet2 = new List<VMapKnot>() { new VMapKnot(p2), new VMapKnot(p3) };
            List<VMapKnot> facet3 = new List<VMapKnot>() { new VMapKnot(p3), new VMapKnot(p0) };

            // определение сегментов
            int ID = 0; int MarkBC = 0; 
            mapSegment[0] = new HMapSegment(facet0, ID, MarkBC);
            ID = 1; MarkBC = 1; 
            mapSegment[1] = new HMapSegment(facet1, ID, MarkBC);
            ID = 2; MarkBC = 2; 
            mapSegment[2] = new HMapSegment(facet2, ID, MarkBC);
            ID = 3; MarkBC = 3; 
            mapSegment[3] = new HMapSegment(facet3, ID, MarkBC);

            // определение область для генерации КЭ сетки
            HMapSubArea subArea = new HMapSubArea();
            // определение граней подобласть для генерации КЭ сетки
            HMapFacet[] facet = new HMapFacet[4];
            //
            for (int id = 0; id < 4; id++)
            {
                facet[id] = new HMapFacet(mapSegment[id]);
                subArea.Add(facet[id]);
            }
            IHTaskMap mapMesh = new HTaskMap(subArea);
            mapMesh.Name = "Simple";
            //mapMesh.Add(subArea);
            IFERenumberator Renumberator = new FERenumberator();
            DirectorMeshGenerator mg = new DirectorMeshGenerator(null, meshData, mapMesh, Renumberator);
            // генерация КЭ сетки
            IFEMesh feMesh = mg.Create();
            mesh = feMesh;
            ShowMesh();
        }

        protected void ShowMesh()
        {
            if (mesh != null && checkBoxView.Checked == true)
            {
            //    IMWCross wMesh = new MWCrossTri(mesh, SСhannelForms.boxCrossSection, 2);
                SavePoint data = new SavePoint();
                data.SetSavePoint(0, mesh);
                double[] xx = mesh.GetCoords(0);
                double[] yy = mesh.GetCoords(1);
                data.Add("Координата Х", xx);
                data.Add("Координата Y", yy);
                data.Add("Координаты ХY", xx, yy);
                //data.Add("Distance", wMesh.GetDistance());
                //data.Add("Hp", wMesh.GetHp());
                GraphicsCurve curves = new GraphicsCurve("Дно");
                for (int i = 0; i < xx.Length; i++)
                    curves.Add(xx[i], yy[i]);
                GraphicsData gd = new GraphicsData();
                gd.Add(curves);
                data.SetGraphicsData(gd);
                Form form = new ViForm(data);
                form.Show();
            }
        }

        public void CreateGeometry(ref double WL)
        {
            WL = 1.7;
            double A = 2;
            int NN = (int)(L / diametrFE) + 1;
            //double A = double.Parse(textBoxA.Text);
            int idx = listBoxArea.SelectedIndex - 1;
            dx = L / (NN - 1);
            x = new double[NN];
            y = new double[NN];

            if (idx == 0)
            {
                for (int i = 0; i < NN; i++)
                {
                    double xi = i * dx;
                    x[i] = xi;
                    y[i] = A * (xi/L * (xi/L - 1) / 0.25 + 1);
                }
            }
            if (idx == 1) // ЛБ
            {
                dx = dx / 2;
                for (int i = 0; i < NN; i++)
                {
                    double xi = i * dx;
                    x[i] = xi;
                    y[i] = A * (xi / L * (xi / L - 1) / 0.25 + 1);
                }
            }
            if (idx == 2) // // Канал Розовского
            {
                double[] Y = { 0,     0.1,  0.15,  0.35, 0.6, 0.775, 0.95, 1.225, 1.5,  1.725,  1.95,  2.0,  2.1 };
                double[] Z = { 0.17, 0.17, 0.145, 0.069,   0,     0,    0,     0,   0,  0.069, 0.145, 0.17, 0.17 };
                Geometry = new DigFunction(Y, Z, "Створ");
                // получение отметок дна
                Geometry.GetFunctionData(ref x, ref y, NN);
                WL = 0.145;
            }
            if (idx == 3) // Канал Розовского 1
            {
                //double[] Y = { 0,     0.11965, 0.405625, 0.634625, 0.804625, 0.974625, 1.203625, 1.489625, 1.60925 };
                //double[] Z = { 0.1401, 0.1,     0.028625,  0,        0,        0,        0.028625,  0.1,      0.1401 };
                //double[] Y = { 0, 0.3, 0.586, 0.815, 1.835, 2.064, 2.35, 2.65 };
                //double[] Z = { 0.200, 0.100, 0.028625, 0.0, 0.0, 0.028625, 0.1, 0.2 };
                Geometry = new DigFunction(YRose2, ZRose2, "Створ");
                // получение отметок дна
                Geometry.GetFunctionData(ref x, ref y, NN);
                WL = 0.140;
            }
            //double[] Roz1_Y = { 0, 0.3, 0.586, 0.815, 1.835, 2.064, 2.35, 2.65 };
            //double[] Roz1_Z = { 0.200, 0.100, 0.028625, 0.0, 0.0, 0.028625, 0.1, 0.2 };
            //R_midle = 5;            //double[] Roz1_Y = { 0, 0.3, 0.586, 0.815, 1.835, 2.064, 2.35, 2.65 };
            //double[] Roz1_Z = { 0.200, 0.100, 0.028625, 0.0, 0.0, 0.028625, 0.1, 0.2 };
            //R_midle = 5;
        }

        protected void River_Mesh()
        {
            double GR = 0;
            double WL = 0;
            CreateGeometry(ref WL);
            if( listBoxArea.SelectedIndex == 1 )
            {
                if(listBoxTypeMesh.SelectedIndex == 0)
                {
                    CrossStripMeshGenerator sg = new CrossStripMeshGenerator(TypeMesh.MixMesh);
                    mesh = (TriMesh)((ComplecsMesh)sg.CreateMesh(ref GR, WL, x, y));
                }
                else
                {
                    HStripMeshGeneratorTri sg = new HStripMeshGeneratorTri();
                    mesh = (TriMesh)sg.CreateMesh(ref GR, WL, x, y);
                }
            }
            else
            {
                HStripMeshGeneratorTri sg = new HStripMeshGeneratorTri();
                mesh = (TriMesh)sg.CreateMesh(ref GR, WL, x, y);
            }
            ShowMesh();
        }

        public double CalkU(double y)
        {
            double U1 = 1.213524255 * Math.Sqrt(-.7043851125e-18 * Math.Exp(24.9 * y) - .1716750000 * Math.Exp(-1.092708634 * y) + .1716750000);
            if (double.IsNaN(U1) == true) 
                return 0;
            return U1;
        }


        private void button2_Click(object sender, EventArgs e)
        {
            double[] mPhi = null;
            double[] mVortex = null;
            double[] mVx = null;
            double[] mVy = null;
            double[] mVz = null;
            double[] tauX = null;
            double[] tauY = null;
            double[] tauZ = null;
            int[] bKnots = null;
            double[] mBC_V = null;
            double[] mBC_U = null;
            double[] meddyViscosity = null;

            double[] mQ = null;
            
            uint[] bcVortexAdress;
            double[] bcVortexValue;

            IAlgebra algebra = null;
            GetMesh();
            if (mesh == null) return;

            double Mu = double.Parse(tb_Mu.Text, MEM.formatter);
            double J = double.Parse(tb_J.Text, MEM.formatter);

            double Ux = double.Parse(tb_Ux.Text, MEM.formatter);
            double Uy = double.Parse(tb_Uy.Text, MEM.formatter);
            double Psi = double.Parse(tb_Psi.Text, MEM.formatter);
            double Wertex = double.Parse(tb_Wertex.Text, MEM.formatter);

            double bUx = double.Parse(tb_B_Ux.Text, MEM.formatter);
            double bUy = double.Parse(tb_B_Uy.Text, MEM.formatter);
            double bPsi = double.Parse(tb_B_Psi.Text, MEM.formatter);
            double bWertex = double.Parse(tb_B_Wertex.Text, MEM.formatter);

            MEM.Alloc(mesh.CountKnots, ref mPhi, 0);
            MEM.Alloc(mesh.CountKnots, ref mVortex, 0);
            MEM.Alloc(mesh.CountKnots, ref mVx, 0);
            MEM.Alloc(mesh.CountKnots, ref mVy, 0);
            MEM.Alloc(mesh.CountKnots, ref mVz, 0);
            
            MEM.Alloc(mesh.CountKnots, ref bKnots, -1);
            MEM.Alloc(mesh.CountKnots, ref mBC_U, 0);
            MEM.Alloc(mesh.CountKnots, ref mBC_V, 0);

            MEM.Alloc(mesh.CountKnots, ref tauX, 0);
            MEM.Alloc(mesh.CountKnots, ref tauY, 0);
            MEM.Alloc(mesh.CountKnots, ref tauZ, 0);

            MEM.Alloc(mesh.CountKnots, ref meddyViscosity, Mu);
            double Q = SPhysics.GRAV * SPhysics.rho_w * J;
            MEM.Alloc(mesh.CountKnots, ref mQ, Q);
            uint[] mbc = null;
            double[] mbv = null;
            List<double> bc_Ux = new List<double>();
            List<double> bc_Uy = new List<double>();
            List<double> bc_Psi = new List<double>();
            List<double> bc_Wertex = new List<double>();
            List<uint> idx_Ux = new List<uint>();
            List<uint> idx_Uy = new List<uint>();
            List<uint> idx_Psi = new List<uint>();
            List<uint> idx_Wertex = new List<uint>();
            List<uint> idx_Bed = new List<uint>();
            /// <summary>
            /// Массив меток  для граничных узловых точек
            /// </summary>
            int[] bknots = mesh.GetBoundKnots();
            int[] Marks = mesh.GetBoundKnotsMark();

            double x_min = 0, x_max = 0;
            mesh.MinMax(0, ref x_min, ref x_max);
            double[] X = mesh.GetCoords(0);
            double[] xx = { x_min, 0.5*(x_min+x_max), x_max };
            IFunLagrange1D ff = new FunLagrange2D(xx);
            ECalkDynamicSpeed typeEddyViscosity = (ECalkDynamicSpeed)ls_Type__U_star.SelectedIndex;

            #region Канал Розовского
            IDigFunction VelosityUx = null;
            IDigFunction VelosityUy = null;
            double R_midle;
            /// <summary>
            /// Массив меток  для граничных узловых точек
            /// </summary>
            #endregion

            if (listBoxArea.SelectedIndex == 3)
            {
                // Канал Розовского 
                double[] Y = { 0.0, 0.1, 0.15, 0.35, 0.6, 0.775, 0.95, 1.225, 1.5, 1.725, 1.95, 2.0, 2.1 };
                double[] U = { 0.0, 0.0, 0, 0.008, 0.011, 0.01, 0.016, 0.024, 0.024, 0.0, 0.0, 0.0, 0.0 };
                double[] V = { 0.0, 0.0, 0, 0.008, 0.011, 0.01, 0.016, 0.024, 0.024, 0.0, 0.0, 0.0, 0.0 };
                VelosityUx = new DigFunction(Y, U, "Створ");
                VelosityUy = new DigFunction(Y, V, "Створ");
                R_midle = 2;
            }
            else
            {
                #region Канал Розовского 1
                R_midle = 5 - 1.325;
                double[] YB = { 0, 0.11965, 0.405625, 0.634625, 0.804625, 0.974625, 1.203625, 1.489625, 1.60925 };
                double[] ZB = { 0.14,       0.1,      0.28625,  0,        0,        0.28625,  0.1,      0.14 };
                
                switch (lb_CrossNamber.SelectedIndex)
                {
                    case 0: // Створ 15
                        {
                            //double[] U15 = { 0, 0.28, 0.375, 0.38, 0.38, 0.373, 0.355, 0.2, 0 };
                            // нормированные к расходу скорости
                            double[] U15 = { 0, 0.2577, 0.381, 0.391, 0.391, 0.3845, 0.366, 0.206, 0 };
                            double[] V15 = { 0, 0.004,  0.02,  0.028, 0.028, 0.027,  0.031, 0.01, 0 };
                            VelosityUx = new DigFunction(YB, U15, "Створ");
                            VelosityUy = new DigFunction(YB, V15, "Створ");
                        }
                        break;
                    case 1: // Створ 18
                        {
                            //double[] U18 = { 0, 0.20, 0.33, 0.38, 0.40, 0.40, 0.38, 0.26, 0 };
                            // нормированные к расходу скорости
                            double[] U18 = { 0, 0.2039, 0.336, 0.387, 0.4078, 0.4078, 0.387, 0.2651, 0 };
                            double[] V18 = { 0, 0.005,  0.03,  0.039, 0.041,  0.04,   0.035, 0.015,  0 };
                            VelosityUx = new DigFunction(YB, U18, "Створ");
                            VelosityUy = new DigFunction(YB, V18, "Створ");
                        }
                        break;
                    case 2: // Створ 21
                        {
                            //double[] U21 = { 0, 0.16, 0.31, 0.38, 0.40, 0.42, 0.44, 0.37, 0 };
                            // нормированные к расходу скорости
                            double[] U21 = { 0, 0.1563, 0.303, 0.371, 0.391, 0.410, 0.43, 0.361, 0 };
                            double[] V21 = { 0, 0.005, 0.013, 0.02, 0.021, 0.02, 0.015, 0.01, 0 };
                            VelosityUx = new DigFunction(YB, U21, "Створ");
                            VelosityUy = new DigFunction(YB, V21, "Створ");
                        }
                        break;
                    case 3: // Створ 26
                        {
                            // double[] U26 = { 0, 0.15, 0.29, 0.35, 0.38, 0.40, 0.43, 0.33, 0 };
                            // нормированные к расходу скорости
                            double[] U26 = { 0, 0.1539, 0.2977, 0.359, 0.39, 0.41, 0.441, 0.3387, 0 };
                            double[] V26 = { 0, 0.003, 0.011, 0.015, 0.018, 0.016, 0.013, 0.01, 0 };
                            VelosityUx = new DigFunction(YB, U26, "Створ");
                            VelosityUy = new DigFunction(YB, V26, "Створ");
                        }
                        break;
                }
                
                #endregion
            }

            for (int i = 0; i < Marks.Length; i++)
            {
                int Mark = Marks[i];
                if (Mark == 0) // Дно
                {
                    if (cb_B_Ux.Checked == true)
                    {
                        idx_Ux.Add((uint)bknots[i]);
                        bc_Ux.Add(bUx);
                    }
                    if (cb_B_Uy.Checked == true)
                    {
                        idx_Uy.Add((uint)bknots[i]);
                        bc_Uy.Add(bUy);
                    }
                    if (cb_B_Psi.Checked == true)
                    {
                        idx_Psi.Add((uint)bknots[i]);
                        bc_Psi.Add(bPsi);
                    }
                    if (cb_B_Wertex.Checked == true)
                    {
                        idx_Wertex.Add((uint)bknots[i]);
                        bc_Wertex.Add(bWertex);
                    }
                    idx_Bed.Add((uint)bknots[i]);
                }
            }

            for (int i = 0; i < Marks.Length; i++)
            {
                int Mark = Marks[i];
                if (Mark == 2) // свободная поверхность
                {
                    int idx = bknots[i];
                    if (cb_Ux.Checked == true)
                    {
                        idx_Ux.Add((uint)bknots[i]);
                        bc_Ux.Add(Ux);
                    }
                    if (cb_Uy.Checked == true)
                    {
                        //idx_Uy.Add((uint)bknots[i]);
                        //double x = X[idx];
                        //double U = ff.CalkForm(x, 0, Uy, 0);
                        //bc_Uy.Add(U);
                    }
                    if (cb_Psi.Checked == true)
                    {
                        idx_Psi.Add((uint)bknots[i]);
                        bc_Psi.Add(Psi);
                    }
                    if (cb_Wertex.Checked == true)
                    {
                        idx_Wertex.Add((uint)bknots[i]);
                        bc_Wertex.Add(Wertex);
                    }
                }
            }

      

            TwoElement[] belems = mesh.GetBoundElems();
            int[] bMarks = mesh.GetBElementsBCMark();

            for (uint be = 0; be < bMarks.Length; be++)
            {
                int Mark = bMarks[be];
                if (Mark == 2) // свободная поверхность
                {
                    
                    if (cb_Uy.Checked == true)
                    {
                        // Канал Розовского
                        if (listBoxArea.SelectedIndex == 3 || listBoxArea.SelectedIndex == 4)
                        {
                            idx_Uy.Add(be);
                            uint idx1 = belems[be].Vertex1;
                            double x1 = X[idx1];
                            uint idx2 = belems[be].Vertex2;
                            double x2 = X[idx2];
                            double U1, U2;
                            
                            if (lb_CrossNamber.SelectedIndex == 3)
                            {
                                U1 = CalkU(x1);
                                U2 = CalkU(x2);
                            }
                            else
                            {
                                U1 = VelosityUx.FunctionValue(x1);
                                U2 = VelosityUx.FunctionValue(x2);
                            }
                            if (cbBoundaryG2_Ux.Checked == true)
                            {
                                mBC_U[idx1] = U1;
                                mBC_U[idx2] = U2;
                                bc_Ux.Add((U1 + U2) * 0.5);
                            }
                            else
                            {
                                mBC_U[idx1] = 0;
                                mBC_U[idx2] = 0;
                                bc_Ux.Add(0);
                            }

                            double V1 = VelosityUy.FunctionValue(x1);
                            double V2 = VelosityUy.FunctionValue(x2);
                            if (cbBoundaryG2_Uy.Checked == true)
                            {
                                mBC_V[idx1] = V1;
                                mBC_V[idx2] = V2;
                                bc_Uy.Add((V1 + V2) * 0.5);
                            }
                            else
                            {
                                mBC_V[idx1] = 0;
                                mBC_V[idx2] = 0;
                                bc_Uy.Add(0);
                            }
                        }
                        else
                        {
                            idx_Uy.Add(be);
                            uint idx1 = belems[be].Vertex1;
                            double x1 = X[idx1];
                            double U1 = ff.CalkForm(x1, 0, Uy, 0);
                            mBC_V[idx1] = U1;
                            uint idx2 = belems[be].Vertex2;
                            double x2 = X[idx2];
                            double U2 = ff.CalkForm(x2, 0, Uy, 0);
                            mBC_V[idx2] = U2;
                            bc_Uy.Add((U1 + U2) * 0.5);
                            bc_Ux.Add(0);
                        }
                    }
                }
            }
            int Ring = (int)lb_Ring.SelectedIndex;
            //IMeshWrapper wMesh = new MeshWrapperTri(mesh);
            // IMWCross wMesh = new MWCrossTri(mesh, 2);
            IMWCrossSection wMesh = null;
            if (listBoxArea.SelectedIndex == 0)
                return;
            if (listBoxArea.SelectedIndex == 1 || 
                listBoxArea.SelectedIndex == 3 ||
                listBoxArea.SelectedIndex == 4)
                wMesh = new MWCrossSectionTri(mesh, R_midle, Ring, false);
            if (listBoxArea.SelectedIndex == 2)
                wMesh = new MWCrossSectionTri(mesh, R_midle, Ring, true);
            
            //algebra = new SparseAlgebraBeCG((uint)mesh.CountKnots, true);
            //algebra = new AlgebraGauss((uint)mesh.CountKnots);
            //if (listBoxArea.SelectedIndex == 3)
            //    algebra = new AlgebraGauss((uint)mesh.CountKnots);
            //else
            if(lb_Algebra.SelectedIndex == 0)
            {
                uint NH = mesh.GetWidthMatrix();
                algebra = new AlgebraLUTape((uint)mesh.CountKnots, (int)NH + 1, (int)NH + 1);
            }
            else
                algebra = new SparseAlgebraBeCG((uint)mesh.CountKnots, false);

            
            bc_Ux.Clear();
            bc_Ux.AddRange(mBC_U);

            mbc = idx_Ux.ToArray();
            mbv = bc_Ux.ToArray();
            TypeTask typeTask = TypeTask.streamY1D;
            // Определение вязкости
            SPhysics.PHYS.turbViscType = (ETurbViscType)listBoxAMu.SelectedIndex;
            switch ( listBoxAMu.SelectedIndex )
            {
                case 0:
                    {
                        MEM.Alloc(mesh.CountKnots, ref mQ, Q);
                        CFEPoissonTaskTri taskV = new CFEPoissonTaskTri(wMesh, algebra, typeTask);
                        taskV.PoissonTask(ref mVx, meddyViscosity, mbc, mbv, mQ);
                        SPhysics.PHYS.calkTurbVisc_Boussinesq1865(ref meddyViscosity, typeTask, wMesh, typeEddyViscosity, mVx, J);
                    }
                    break;
                case 1:
                    {
                        MEM.Alloc(mesh.CountKnots, ref mQ, Q);
                        CFEPoissonTaskTri taskV = new CFEPoissonTaskTri(wMesh, algebra, typeTask);
                        taskV.PoissonTask(ref mVx, meddyViscosity, mbc, mbv, mQ);
                        SPhysics.PHYS.calkTurbVisc_Karaushev1977(ref meddyViscosity, typeTask, wMesh, typeEddyViscosity, mVx, J);
                    }
                    break;
                case 2:
                    SPhysics.PHYS.calkTurbVisc_Prandtl1934(ref meddyViscosity, typeTask, wMesh, typeEddyViscosity, mVx, J);
                    break;
                case 3:
                    SPhysics.PHYS.calkTurbVisc_Velikanov1948(ref meddyViscosity, typeTask, wMesh, typeEddyViscosity, mVx, J);
                    break;
                case 4:
                    SPhysics.PHYS.calkTurbVisc_Absi_2012(ref meddyViscosity, typeTask, wMesh, typeEddyViscosity, mVx, J);
                    break;
                case 5:
                    SPhysics.PHYS.calkTurbVisc_Absi_2019(ref meddyViscosity, typeTask, wMesh, typeEddyViscosity, mVx, J);
                    break;
            }
            
                        

            int VortexBC_G2 = lb_VortexBC_G2.SelectedIndex;

            int VetrexTurbTask = lb_VetrexTurbTask.SelectedIndex;

            int index = listBoxRiverTask.SelectedIndex;

            bool flagUx = cbBoundaryG2_Ux.Checked;
            
            

            switch (index)
            {
                case 0: // Расчет потока Ux в створе канала
                    {
                        mbc = idx_Ux.ToArray();
                        mbv = bc_Ux.ToArray();
                        CFEPoissonTaskTri task = new CFEPoissonTaskTri(wMesh, algebra, typeTask);
                        task.PoissonTask(ref mVx, meddyViscosity, mbc, mbv, mQ);
                        Show(mVx, null, null, null, null, null, null, null);
                    }
                    break;
                case 1: // Расчет вихря Vortex в створе канала
                    {
                        mbc = idx_Wertex.ToArray();
                        mbv = bc_Wertex.ToArray();
                        CTransportEquationsTri task = new CTransportEquationsTri(wMesh, algebra, typeTask);
                        task.TransportEquationsTaskSUPG(ref mVortex, meddyViscosity, mVx, mVy, mbc, mbv, mQ);
                        Show(null, mVx, mVy, null, null, null, null, mVortex);
                    }
                    break;
                case 2: // Решение задачи вихрь -функции тока в створе канала
                    {
                        bcVortexAdress = idx_Wertex.ToArray();
                        bcVortexValue = bc_Wertex.ToArray();
                        FEVortexPhi task = new FEVortexPhi(mesh, algebra);
                        task.CalkVortexPhi_BCVortex(ref mPhi, ref mVortex, ref mVx, ref mVy, 
                                                    meddyViscosity, mQ, bcVortexAdress, bcVortexValue);
                        Show(null, mVx, mVy, null, null, null, mPhi, mVortex);
                    }
                    break;
                 case 3: // Решение задачи вихрь -функции тока в створе канала + расчет потока Ux в створе канала
                    {
                        MEM.Alloc(mesh.CountKnots, ref mQ, 0);
                        // Решение задачи вихрь - функции тока в створе канала
                        bcVortexAdress = idx_Wertex.ToArray();
                        bcVortexValue = bc_Wertex.ToArray();
                        CFEVortexPhiTri task = new CFEVortexPhiTri(wMesh, algebra, typeTask);
                        task.CalkVortexPhi_BCVortex(ref mPhi, ref mVortex, ref mVy, ref mVz, 
                                                    meddyViscosity, mQ, bcVortexAdress, bcVortexValue);
                        // Расчет потока Ux в створе канала
                        mbc = idx_Ux.ToArray();
                        mbv = bc_Ux.ToArray();
                        MEM.Alloc(mesh.CountKnots, ref mQ, Q);
                        CTransportEquationsTri taskUx = new CTransportEquationsTri(wMesh, algebra, typeTask);
                        taskUx.TransportEquationsTaskSUPG(ref mVx, meddyViscosity, mVy, mVz, mbc, mbv, mQ);
                        taskUx.CalkTauInterpolation(ref tauY, ref tauZ, mVx, meddyViscosity);

                        double[] mDistance = wMesh.GetDistance();
                        ShowCFD(mVx, mVy, mVz, meddyViscosity, tauY, tauZ, mPhi, mVortex, mDistance);
                        
                    }
                    break;
                case 4: // Решение задачи вихрь - функции тока в створе канала
                    {
                        bcVortexAdress = idx_Wertex.ToArray();
                        bcVortexValue = bc_Wertex.ToArray();
                        CFETurbulVortexPhiTri task = new CFETurbulVortexPhiTri(wMesh, algebra, TypeTask.streamY1D);
                        MEM.Alloc(mesh.CountKnots, ref mQ, Q);
                        task.CalkVortexPhi_BCVortex(ref mPhi, ref mVortex, ref mVx, ref mVy, ref mVz,
                                                    ref tauY, ref tauZ, ref meddyViscosity,
                                                    mQ, bcVortexAdress, bcVortexValue, typeEddyViscosity);
                        double[] mDistance = wMesh.GetDistance();
                        ShowCFD(mVx, mVy, mVz, meddyViscosity, null, tauY, tauZ, mPhi, mVortex, mDistance);
                    }
                    break;
                case 5: // Решение задачи вихрь - функции тока в створе канала
                    {
                        CFETurbulVortexPhiTri task = new CFETurbulVortexPhiTri(wMesh, algebra, TypeTask.streamY1D);
                        MEM.Alloc(mesh.CountKnots, ref mQ, Q);
                        task.CalkVortexPhi_BCVelocity(ref mPhi, ref mVortex, ref mVx, ref mVy, ref mVz,  ref meddyViscosity, 
                            ref tauY, ref tauZ, mQ, idx_Uy.ToArray(), bc_Uy.ToArray(), typeEddyViscosity);

                        double[] mDistance = wMesh.GetDistance();
                        ShowCFD(mVx, mVy, mVz, meddyViscosity, mBC_V, tauY, tauZ, mPhi, mVortex, mDistance);
                    }
                    break;
                case 6: // Решение задачи вихрь - функции тока в створе канала
                    {
                        
                        RiverSectionVortexPhiTri task = new RiverSectionVortexPhiTri(wMesh, algebra, TypeTask.streamY1D);
                        MEM.Alloc(mesh.CountKnots, ref mQ, Q);
                        task.CalkVortexPhi_BCVelocity(ref mPhi, ref mVortex, ref mVx, ref mVy, ref mVz, ref meddyViscosity,
                            ref tauY, ref tauZ, mQ, bc_Uy.ToArray(), VortexBC_G2, VetrexTurbTask, typeEddyViscosity);

                        double[] mDistance = wMesh.GetDistance();
                        ShowCFD(mVx, mVy, mVz, meddyViscosity, mBC_V, tauY, tauZ, mPhi, mVortex, mDistance);
                    }
                    break;
                case 7: // Решение задачи вихрь - функции тока в створе канала
                    {
                        double[] tauXY = null;
                        double[] tauXZ = null;
                        double[] tauYY = null;
                        double[] tauYZ = null;
                        double[] tauZZ = null;
                        double w = 0.3;
                        RiverCrossVortexPhiTri task = new RiverCrossVortexPhiTri(wMesh, algebra, TypeTask.streamY1D, w);
                        MEM.Alloc(mesh.CountKnots, ref mQ, Q);

                        task.CalkVortexPhi_BCVelocity(ref mPhi, ref mVortex, ref mVx, ref mVy, ref mVz, ref meddyViscosity,
                            ref tauXY, ref tauXZ, ref tauYY, ref tauYZ, ref tauZZ, bc_Ux.ToArray(), bc_Uy.ToArray(), 
                            mQ, VetrexTurbTask, flagUx, VortexBC_G2, typeEddyViscosity);

                        double[] mDistance = wMesh.GetDistance();

                        SavePoint data = new SavePoint();
                        data.SetSavePoint(0, mesh);

                        double[] x = mesh.GetCoords(0);
                        double[] y = mesh.GetCoords(1);

                        data.Add("Скорость Vx", mVx);
                        data.Add("Скорость Vy", mVy);
                        data.Add("Скорость Vz", mVz);
                        data.Add("Скорость", mVy, mVz);
                        data.Add("Турбулентная вязкость", meddyViscosity);

                        data.Add("Phi", mPhi);
                        data.Add("mVortex", mVortex);
                        data.Add("tauXY", tauXY);
                        data.Add("tauXZ", tauXZ);
                        data.Add("TauX", tauXY, tauXZ);

                        data.Add("tauYY", tauYY);
                        data.Add("tauYZ", tauYZ);
                        data.Add("tauZZ", tauZZ);

                        if (mDistance != null)
                            data.Add("mDistance", mDistance);

                        data.Add("Координата Х", x);
                        data.Add("Координата Y", y);

                        Form form = new ViForm(data);
                        form.Text = listBoxAMu.Items[listBoxAMu.SelectedIndex].ToString()
                        +" "+lb_CrossNamber.Items[lb_CrossNamber.SelectedIndex].ToString();
                        form.Show();
                    }
                    break;
                case 8: // Решение задачи вихрь - функции тока в створе канала
                    {
                        double[] tauXY = null;
                        double[] tauXZ = null;
                        double[] tauYY = null;
                        double[] tauYZ = null;
                        double[] tauZZ = null;
                        double w = 0.3;
                        RiverCrossVortexPhiTri task = new RiverCrossVortexPhiTri(wMesh, algebra, TypeTask.streamY1D, w);
                        MEM.Alloc(mesh.CountKnots, ref mQ, Q);

                        task.CalkVortexPhi_Plane(ref mPhi, ref mVortex, ref mVx, ref mVy, ref mVz, ref meddyViscosity,
                            ref tauXY, ref tauXZ, ref tauYY, ref tauYZ, ref tauZZ, bc_Uy.ToArray(), J, 
                            VetrexTurbTask, typeEddyViscosity);

                        double[] mDistance = wMesh.GetDistance();

                        SavePoint data = new SavePoint();
                        data.SetSavePoint(0, mesh);

                        double[] x = mesh.GetCoords(0);
                        double[] y = mesh.GetCoords(1);

                        data.Add("Скорость Vx", mVx);
                        data.Add("Скорость Vy", mVy);
                        data.Add("Скорость Vz", mVz);
                        data.Add("Скорость", mVy, mVz);
                        data.Add("Турбулентная вязкость", meddyViscosity);

                        data.Add("Phi", mPhi);
                        data.Add("mVortex", mVortex);
                        data.Add("tauXY", tauXY);
                        data.Add("tauXZ", tauXZ);
                        data.Add("TauX", tauXY, tauXZ);

                        data.Add("tauYY", tauYY);
                        data.Add("tauYZ", tauYZ);
                        data.Add("tauZZ", tauZZ);

                        if (mDistance != null)
                            data.Add("mDistance", mDistance);

                        data.Add("Координата Х", x);
                        data.Add("Координата Y", y);

                        Form form = new ViForm(data);
                        form.Text = listBoxAMu.Items[listBoxAMu.SelectedIndex].ToString()
                        + " " + lb_CrossNamber.Items[lb_CrossNamber.SelectedIndex].ToString();
                        form.Show();
                    }
                    break;
            }
        }

        /// <summary>
        /// генерация КЭ СЕТКИ в тестовой области
        /// </summary>
        void GetMesh()
        {
            GetHLD();
            // meshData
            if( listBoxArea.SelectedIndex == 0)
                QUAD_Mesh();
            else
                River_Mesh();
        }

    

        private void btCalk_Click(object sender, EventArgs e)
        {
            double[] mT = null;
            double[] meddyViscosity = null;
            double[] mVx = null;
            double[] mVy = null;
            double[] mQ = null;

            uint[] bcAdressPhi;
            double[] bcValuePhi;

            IAlgebra algebra = null;
            GetMesh();
            if (mesh == null) return;

            double Mu = double.Parse(tb_Lambda.Text, MEM.formatter);
            double Q = double.Parse(tb_Q.Text, MEM.formatter);
            double U = double.Parse(tb_U.Text, MEM.formatter);
            double V = double.Parse(tb_V.Text, MEM.formatter);
            double bcL = double.Parse(tb_L.Text, MEM.formatter);
            double bcR = double.Parse(tb_R.Text, MEM.formatter);
            double bcT = double.Parse(tb_T.Text, MEM.formatter);
            double bcB = double.Parse(tb_B.Text, MEM.formatter);

            tb_Pe_u.Text = (U * L / Mu).ToString();
            tb_Pe_v.Text = (V * H / Mu).ToString();
            tb_Amin.Text = (Math.Sqrt((U * U + V * V) * mesh.ElemSquare(0)) / Mu).ToString();

            MEM.Alloc(mesh.CountKnots, ref mT, 0);
            MEM.Alloc(mesh.CountKnots, ref meddyViscosity, Mu);
            MEM.Alloc(mesh.CountKnots, ref mVx, U);
            MEM.Alloc(mesh.CountKnots, ref mVy, V);
            MEM.Alloc(mesh.CountKnots, ref mQ, Q);

            uint[] mbc = null;
            double[] mbv = null;
            List<uint> idx = new List<uint>();
            List<double> bc = new List<double>();

            /// <summary>
            /// Массив меток  для граничных узловых точек
            /// </summary>
            int[] bknots = mesh.GetBoundKnots();
            int[] Marks = mesh.GetBoundKnotsMark();
            for (int i = 0; i < Marks.Length; i++)
            {
                int Mark = Marks[i];
                if (cb_L.Checked == true && Mark == 3)
                {
                    idx.Add((uint)bknots[i]);
                    bc.Add(bcL);
                }
                else if (cb_R.Checked == true && Mark == 1)
                {
                    idx.Add((uint)bknots[i]);
                    bc.Add(bcR);
                }
                else if (cb_T.Checked == true && Mark == 2)
                {
                    idx.Add((uint)bknots[i]);
                    bc.Add(bcT);
                }
                else if (cb_B.Checked == true && Mark == 0)
                {
                    idx.Add((uint)bknots[i]);
                    bc.Add(bcB);
                }
            }
            mbc = idx.ToArray();
            mbv = bc.ToArray();

            List<uint> idxPhi = new List<uint>();
            List<double> bcPhi = new List<double>();

            double[] X = mesh.GetCoords(0);
            double[] Y = mesh.GetCoords(1);
            double Ymin = Y.Min();
            double Ymax = Y.Max();
            double Hmax = Ymax - Ymin;
            double Umax = Q / Hmax;
            for (int i = 0; i < Marks.Length; i++)
            {
                int Mark = Marks[i];
                if (Mark == 0)
                {
                    idxPhi.Add((uint)bknots[i]);
                    bcPhi.Add(0);
                }
                //if (Mark == 3 || Mark == 1)
                if (Mark == 3 )
                {
                    idxPhi.Add((uint)bknots[i]);
                    double z = Y[bknots[i]];
                    double phi =  Umax * z * z * (z / (3.0 * H) - 1) / H;
                    bcPhi.Add(phi);
                }
                if (Mark == 2)
                {
                    idxPhi.Add((uint)bknots[i]);
                    double z = Y[bknots[i]];
                    double phi = - Umax * H * (2.0 / 3.0);
                    bcPhi.Add(phi);
                }
            }
            bcAdressPhi = idxPhi.ToArray();
            bcValuePhi = bcPhi.ToArray();   

            algebra = new AlgebraGauss((uint)mesh.CountKnots);
            //FEMPoissonTask

            IMeshWrapper wMesh = new MeshWrapperTri(mesh);
            TypeTask typeTask = TypeTask.streamX1D;
            int index = listBoxTasks.SelectedIndex;
            SPhysics.PHYS.turbViscType = (ETurbViscType)listBoxAMu.SelectedIndex;
            ECalkDynamicSpeed typeEddyViscosity = (ECalkDynamicSpeed)ls_Type__U_star.SelectedIndex;
            switch (index)
            {
                case 0:
                    {
                        CFEPoissonTaskTri task = new CFEPoissonTaskTri(wMesh, algebra, typeTask);
                        task.PoissonTask(ref mT, meddyViscosity, mbc, mbv, mQ);
                    }
                    break;
                case 1:
                    {
                        CTransportEquationsTri task = new CTransportEquationsTri(wMesh, algebra, typeTask);
                        //FETransportEquationsTri task = new FETransportEquationsTri(mesh, algebra);
                        task.TransportEquationsTask(ref mT, meddyViscosity, mVx, mVy, mbc, mbv, mQ);
                    }
                    break;
                case 2:
                    {
                        CTransportEquationsTri task = new CTransportEquationsTri(wMesh, algebra, typeTask);
                        //FETransportEquationsTri task = new FETransportEquationsTri(mesh, algebra);
                        task.TransportEquationsTaskSUPG(ref mT, meddyViscosity, mVx, mVy, mbc, mbv, mQ);
                    }
                    break;
                case 3:
                    {
                        FETransportEquationsTask task = new FETransportEquationsTask(mesh, algebra);
                        task.CFETransportEquationsTask(ref mT, meddyViscosity, mVx, mVy, mbc, mbv, mQ);
                    }
                    break;
                case 4:
                    {
                        FETransportEquationsTask task = new FETransportEquationsTask(mesh, algebra);
                        task.FETransportEquationsTaskSUPG(ref mT, meddyViscosity, mVx, mVy, mbc, mbv, mQ);
                    }
                    break;
                case 5:
                    {
                        IMWDistance wM = new MWDistanceTri(wMesh);
                        double[] mVortex = null;
                        double[] mPhi = null;
                        CFETurbulVortexPhiTri task = new CFETurbulVortexPhiTri(wM, algebra, TypeTask.streamX1D );
                        MEM.Alloc(mesh.CountKnots, ref mQ, Q);
                        task.CalkVortexPhi_StreamBCPhi(ref mPhi, ref mVortex, ref mVx, ref mVy, 
                          ref meddyViscosity, bcAdressPhi, bcValuePhi, typeEddyViscosity);

                        double[] mDistance = wM.GetDistance();
                        ShowCFD(null, mVx, mVy, meddyViscosity, null, null, null, mPhi, mVortex, mDistance);
                        return;
                    }
                    break;
                case 6:
                    {
                        //FEMPoissonTask task = new FEMPoissonTask(mesh, algebra);
                        //task.FEPoissonTask(ref mT, meddyViscosity, mbc, mbv, mQ);
                    }
                    break;
            }
            SavePoint data = new SavePoint();
            data.SetSavePoint(0, mesh);

            double[] x = mesh.GetCoords(0);
            double[] y = mesh.GetCoords(1);

            data.Add("Температура", mT);
            data.Add("Скорость U", mVx);
            data.Add("Скорость V", mVy);
            data.Add("Скорость", mVx, mVy);
            data.Add("Координата Х", x);
            data.Add("Координата Y", y);

            Form form = new ViForm(data);

            Show(mT, mVx, mVy, null, null, null, null, null);

        }

        public void Show(double[] mT, double[] mVx, double[] mVy, double[] Ux, 
            double[] Uy, double[] Uz, double[] Psi, double[] Wer)
        {
            SavePoint data = new SavePoint();
            data.SetSavePoint(0, mesh);

            double[] x = mesh.GetCoords(0);
            double[] y = mesh.GetCoords(1);

            if(mT != null)
                data.Add("Температура", mT);
            if (mVx != null)
                data.Add("Скорость U", mVx);
            if (mVy != null)
                data.Add("Скорость V", mVy);
            if (mVx != null && mVy != null)
                data.Add("Скорость", mVx, mVy);
            if (Ux != null)
                data.Add("Скорость Ux", Ux);
            if (Uy != null)
                data.Add("Скорость Uy", Uy);
            if (Uz != null)
                data.Add("Скорость Uz", Uz);
            if (Psi != null)
                data.Add("Psi", Psi);
            if (Wer != null)
                data.Add("Wertex", Wer);

            data.Add("Координата Х", x);
            data.Add("Координата Y", y);

            Form form = new ViForm(data);
            form.Show();
        }


        public void ShowCFD(double[] mVx, double[] mVy, double[] mVz, double[] meddyViscosity,
            double[] TauXX, double[] TauXY, double[] TauXZ, double[] Psi, double[] mVortex, double[] mDistance=null)
        {
            SavePoint data = new SavePoint();
            data.SetSavePoint(0, mesh);

            double[] x = mesh.GetCoords(0);
            double[] y = mesh.GetCoords(1);

            if (mVx != null)
                data.Add("Скорость Vx", mVx);
            if (mVy != null)
                data.Add("Скорость Vy", mVy);
            if (mVz != null)
                data.Add("Скорость Vz", mVz);
            if (mVy != null && mVz != null)
                data.Add("Скорость", mVy, mVz);
            if (meddyViscosity != null)
                data.Add("Турбулентная вязкость", meddyViscosity);
            
            if (Psi != null)
                data.Add("Psi", Psi);
            if (mVortex != null)
                data.Add("mVortex", mVortex);
            if (TauXX != null)
                data.Add("tauXX", TauXX);
            if (TauXY != null)
                data.Add("TauXY", TauXY);
            if (TauXZ != null)
                data.Add("TauXZ", TauXZ);
            if (TauXZ != null && TauXY != null)
                data.Add("TauX", TauXY, TauXZ);
            
            if (mDistance != null)
                data.Add("mDistance", mDistance);

            data.Add("Координата Х", x);
            data.Add("Координата Y", y);

            Form form = new ViForm(data);
            form.Show();
        }


    }
}
