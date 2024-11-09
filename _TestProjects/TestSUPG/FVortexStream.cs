namespace TestSUPG
{
    using System;
    using System.Linq;
    using System.Windows.Forms;

    using CommonLib;
    using CommonLib.Mesh;
    using CommonLib.Physics;
    using CommonLib.Delegate;
    using CommonLib.Function;

    using MeshLib;
    using MeshLib.Wrappers;

    using RenderLib;
    using MemLogLib;
    using AlgebraLib;
    using GeometryLib;
    using FEMTasksLib.FESimpleTask;
    using MeshGeneratorsLib.StripGenerator;

    public partial class FVortexStream : Form
    {
        IMesh mesh = null;

        double H = 0;
        double L = 0;
        double diametrFE;
        double R_midle = 5; //- 0.8;
        //double[] YRose2 = { 0,      0.11965, 0.405625, 0.634625, 0.804625, 0.974625, 1.203625, 1.489625, 1.60925 };
        //double[] ZRose2 = { 0.1401, 0.1,     0.028625, 0, 0, 0, 0.028625, 0.1, 0.1401 };
        double[] YRose2 = { -0.16, 0,      0.12, 0.4,      0.65, 0.8, 0.95, 1.2,      1.48, 1.6 , 1.76 };
        double[] ZRose2 = { 0.204, 0.1401, 0.1,  0.028625, 0,      0,    0, 0.028625, 0.1,  0.1401, 0.204 };
        double WL = 0.140;
        IDigFunction Geometry;
        int SelectedIndexSave = 0;
        public FVortexStream()
        {
            InitializeComponent();
            lb_Ring.SelectedIndex = 1;
            listBoxAMu.SelectedIndex = 11;
            lb_VortexBC_G2.SelectedIndex = 3;
            SelectedIndexSave = lb_VortexBC_G2.SelectedIndex; 
            lb_CrossNamber.SelectedIndex = 3;
            lb_Algebra.SelectedIndex = 0;
            lb_Geometry.SelectedIndex = 0;
            lb_MeshGen.SelectedIndex = 0;
            ls_Type__U_star.SelectedIndex = 0;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            River_Mesh();
        }

        /// <summary>
        /// генерация КЭ СЕТКИ в тестовой области
        /// </summary>

        protected void River_Mesh()
        {
            double L = 1;
            double PFE = double.Parse(textBoxDiam.Text, MEM.formatter);
            diametrFE = L / 100 * PFE;
            double[] x = null;
            double[] y = null;
            double GR = 0;
            int NN = (int)(10 / diametrFE) + 1;
            // получение отметок дна
            if(lb_Geometry.SelectedIndex == 0)
                Geometry = new DigFunction(YRose2, ZRose2, "Створ");
            else
                Geometry = new FunctionСhannelRose();
            Geometry.GetFunctionData(ref x, ref y, NN);
            //HStripMeshGeneratorTri sg = new HStripMeshGeneratorTri();
            //CrossStripMeshGeneratorTri sg = new CrossStripMeshGeneratorTri();
            //mesh = sg.CreateMesh(ref GR, WL, x, y);
            IStripMeshGenerator sg = null;
            switch (lb_MeshGen.SelectedIndex)
            {
                case 0:
                    sg = new HStripMeshGeneratorTri();
                    break;
                case 1:
                    sg = new CrossStripMeshGeneratorTri();
                    break;
                case 2:
                    sg = new CrossStripMeshGenerator(TypeMesh.Triangle);
                    break;
            }
            mesh = sg.CreateMesh(ref GR, WL, x, y);
            ShowMesh();
        }

        /// <summary>
        /// Скорость в створе 15
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        double CalkU15(double y)
        {
            double V =
                    -0.4823745080e-2 * Math.Exp(2.230789734 * y)
                   - 0.1668512549 * Math.Exp(-3.659361162 * y) + 0.1716750000;
            return 1.096569401 * Math.Sqrt(Math.Max(0, V));
        }
        /// <summary>
        /// Скорость в створе 15
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        double CalkU18(double y)
        {
            double V = -0.7744248317e-2 * Math.Exp(1.513732350 * y) -
                0.1639307517 * Math.Exp(-.4148312511 * y) + 0.1716750000;
            return 2.457935972 * Math.Sqrt(Math.Max(0, V));
        }
        /// <summary>
        /// Скорость в створе 15
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        double CalkU26(double y)
        {
            double V = -0.3580698312e-5 * Math.Exp(5.606508095 * y)
                - .1716714194 * Math.Exp(-.1120026006 * y) + .1716750000;
            return 3.285684719 * Math.Sqrt(Math.Max(0, V));
        }

        protected void ShowMesh()
        {
            if (mesh != null && checkBoxView.Checked == true)
            {
                IMWDistance wMesh = new MWCrossTri(mesh, SСhannelForms.porabolicСhannelSection, 2);
                SavePoint data = new SavePoint();
                data.SetSavePoint(0, mesh);
                double[] xx = mesh.GetCoords(0);
                double[] yy = mesh.GetCoords(1);
                data.Add("Distance", wMesh.GetDistance());
                data.Add("Hp", wMesh.GetHp());
                data.Add("Координата Х", xx);
                data.Add("Координата Y", yy);
                data.Add("Координаты ХY", xx, yy);
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
        private void button2_Click(object sender, EventArgs e)
        {
            double[] mQ = null;
            double[] mPhi = null;
            double[] mVortex = null;
            double[] mVx = null;
            double[] mVy = null;
            double[] mVz = null;
            int[] bKnotsU = null;
            int[] bKnotsV = null;
            double[] mBC_V = null;
            double[] mBC_U = null;
            double[] meddyViscosity = null;
            double[] tauXY = null;
            double[] tauXZ = null;
            double[] tauYY = null;
            double[] tauYZ = null;
            double[] tauZZ = null;

            River_Mesh();

            if (mesh == null) return;

            double w = double.Parse(tb_w.Text, MEM.formatter);
            double J = double.Parse(tb_J.Text, MEM.formatter);
            double Q = SPhysics.GRAV * SPhysics.rho_w * J;
            int Ring = (int)lb_Ring.SelectedIndex;


            MEM.Alloc(mesh.CountKnots, ref bKnotsU, -1);
            MEM.Alloc(mesh.CountKnots, ref bKnotsV, -1);
            MEM.Alloc(mesh.CountKnots, ref mBC_U, 0);
            MEM.Alloc(mesh.CountKnots, ref mBC_V, 0);
            MEM.Alloc(mesh.CountKnots, ref mQ, Q);
            MEM.Alloc(mesh.CountKnots, ref meddyViscosity, 0.3);
            

            IDigFunction VelosityUx = null;
            IDigFunction VelosityUy = null;
            #region Канал Розовского 1
            R_midle = 5 - YRose2[YRose2.Length-1]/2;

            Function<double, double> CalkU = null;

            double[] YRose = { 0, 0.05,  0.3,   0.55, 0.80,  1.05,  1.3,   1.55,  1.6 };
            //double[] U15 =   { 0, 0.243, 0.365, 0.38, 0.38, 0.365, 0.345, 0.186, 0 };
            //double[] U18 =   { 0, 0.2,   0.32,  0.38, 0.40, 0.40,  0.38,  0.26,  0 };
            //double[] U21 =   { 0, 0.16,  0.25,  0.36, 0.40, 0.43, 0.45, 0.36, 0 };
            double[] U25 =   { 0, 0.15,  0.24,  0.31, 0.36, 0.45, 0.43, 0.33, 0 };
            // выбор граничных условий створа
            switch (lb_CrossNamber.SelectedIndex)
            {
                case 0: // Створ 15
                    {
                        // 17 04 24
                        double[] Y_U15 = { 0, 0.1, 0.3, 0.55, 0.8, 1.05, 1.2, 1.5, 1.6 };
                        double[] U15 = { 0, 0.243, 0.365, 0.38, 0.38, 0.365, 0.345, 0.186, 0 };
                        
                        double[] Y_V15 = { 0, 0.55,  0.8,   1.05,  1.2,    1.6 };
                        double[] V15 =   { 0, 0.03,  0.028, 0.028, 0.038,  0 };

                        VelosityUx = new DigFunction(Y_U15, U15, "Створ");
                        //VelosityUy = new DigFunction(YRose2, V15, "Створ");
                        VelosityUy = new DigFunction(Y_V15, V15, "Створ");
                        CalkU = CalkU15;
                    }
                    break;
                case 1: // Створ 18
                    {
                        // 17 04 24
                        double[] Y_U18 = { 0, 0.15, 0.4,  0.6,  0.8, 1.07, 1.25, 1.4, 1.6 };
                        double[] U18   = { 0, 0.2,  0.32, 0.38, 0.4, 0.4,  0.38, 0.26, 0 };

                        double[] Y_V18 = { 0, 0.4, 0.6, 0.8, 1.07, 1.25, 1.6 };
                        double[] V18 = { 0, 0.028, 0.039, 0.041, 0.039, 0.032, 0 };

                        VelosityUx = new DigFunction(Y_U18, U18, "Створ");
                        //VelosityUy = new DigFunction(YRose2, V18, "Створ");
                        VelosityUy = new DigFunction(Y_V18, V18, "Створ");
                        CalkU = CalkU18;
                    }
                    break;
                case 2: // Створ 21
                    {
                        // 17 04 24
                        double[] Y_U21 = { 0, 0.17, 0.5,  0.8, 1.13, 1.45, 1.6 };
                        double[] U21   = { 0, 0.16, 0.35, 0.4, 0.45, 0.37, 0 };

                        double[] Y_V21 = { 0, 0.5,   0.8,   1.13,  1.6 };
                        double[] V21  =  { 0, 0.022, 0.021, 0.021, 0 };

                        VelosityUx = new DigFunction(Y_U21, U21, "Створ");
                        //VelosityUy = new DigFunction(YRose2, V21, "Створ");
                        VelosityUy = new DigFunction(Y_V21, V21, "Створ");
                        CalkU = CalkU26;
                    }
                    break;
                case 3: // Тест
                    {
                        // double[] U26 = { 0, 0.15, 0.29, 0.35, 0.38, 0.40, 0.43, 0.33, 0 };
                        // нормированные к расходу скорости
                        
                        double[] U26 =    {  0,    0.5,  0.5,  0.5,  0.5,  0.5,  0.5,  0.5,  0.5,  0.5, 0 };
                        double[] V26 =    {  0 ,   0.05, 0.05, 0.05, 0.05, 0.05, 0.05, 0.05, 0.05, 0.05, 0 };
                        VelosityUx = new DigFunction(YRose2, U26, "Створ");
                        VelosityUy = new DigFunction(YRose2, V26, "Створ");
                    }
                    break;
            }

            #endregion
            double[] X = mesh.GetCoords(0);
            TwoElement[] belems = mesh.GetBoundElems();
            int[] bMarks = mesh.GetBElementsBCMark();

            for (uint be = 0; be < bMarks.Length; be++)
            {
                int Mark = bMarks[be];
                uint idx1 = belems[be].Vertex1;
                double x1 = X[idx1];
                uint idx2 = belems[be].Vertex2;
                double x2 = X[idx2];
                if (Mark == 0) // дно канала
                {
                    bKnotsU[idx1] = 1;
                    mBC_U[idx1] = 0;
                    bKnotsU[idx2] = 1;
                    mBC_U[idx2] = 0;
                }
                if (Mark == 2) // свободная поверхность
                {
                    if (cbBoundaryG2_Ux.Checked == true)
                    {
                        //double U1 = VelosityUx.FunctionValue(x1);
                        //double U2 = VelosityUx.FunctionValue(x2);
                        //Console.WriteLine(" U1 {0:F4}  U2 {0:F4} i1 {2}  i2 {3}", U1, U2, idx1, idx2);
                        bKnotsU[idx1] = 1;
                        bKnotsU[idx2] = 1;
                        if(cb_smoothing.Checked == true && CalkU!=null)
                        {
                            mBC_U[idx1] = CalkU(x1);
                            mBC_U[idx2] = CalkU(x2);
                        }
                        else
                        {
                            mBC_U[idx1] = VelosityUx.FunctionValue(x1);
                            mBC_U[idx2] = VelosityUx.FunctionValue(x2);
                        }
                    }
                    bKnotsV[idx1] = 1;
                    mBC_V[idx1] = VelosityUy.FunctionValue(x1);
                    bKnotsV[idx2] = 1;
                    mBC_V[idx2] = VelosityUy.FunctionValue(x2);
                }
            }

            double[] mbU = null;
            uint[] mAdressU = null;
            double[] mbV = null;
            int CountU = bKnotsU.Sum(xx => xx > 0 ? xx : 0);
            int CountV = bKnotsV.Sum(xx => xx > 0 ? xx : 0);
            MEM.Alloc(CountU, ref mbU, 0);
            MEM.Alloc(CountU, ref mAdressU);
            MEM.Alloc(CountV, ref mbV, 0);
            int k = 0;
            for(uint i = 0; i < mBC_U.Length; i++)
            {
                if (bKnotsU[i] == 1)
                {
                    mAdressU[k] = i;
                    mbU[k++] = mBC_U[i];
                }
            }
            k = 0;
            for (int i = 0; i < mBC_V.Length; i++)
            {
                if (bKnotsV[i] == 1)
                    mbV[k++] = mBC_V[i];
            }
            IMWCrossSection wMesh = new MWCrossSectionTri(mesh, R_midle, Ring, false);
            // Определение вязкости
            SPhysics.PHYS.turbViscType = (ETurbViscType)listBoxAMu.SelectedIndex;

            IAlgebra algebra = null;
            if (lb_Algebra.SelectedIndex == 0)
            {
                uint NH = mesh.GetWidthMatrix();
                algebra = new AlgebraLUTape((uint)mesh.CountKnots, (int)NH + 1, (int)NH + 1);
            }
            else
                algebra = new SparseAlgebraBeCG((uint)mesh.CountKnots, false);

            ECalkDynamicSpeed typeEddyViscosity = (ECalkDynamicSpeed)ls_Type__U_star.SelectedIndex;

            RiverCrossVortexPhiTri task = new RiverCrossVortexPhiTri((IMWDistance)wMesh, algebra, TypeTask.streamY1D, w);
            
            int flagIndexUy = lb_VortexBC_G2.SelectedIndex;
            int VortexBC_G2 = lb_VortexBC_G2.SelectedIndex;
            bool flagUx = cbBoundaryG2_Ux.Checked;
            bool flagLes = cb_Mu_YZ.Checked;
            double[] eddyViscosityUx = null;
            int idxMu2 = listBoxAMu2.SelectedIndex;
            double[] RR = null;
            task.CalkVortexStream(
                // Искомые поля
                ref mPhi, ref mVortex, ref mVx, ref mVy, ref mVz, ref eddyViscosityUx, ref meddyViscosity,
                ref tauXY, ref tauXZ, ref tauYY, ref tauYZ, ref tauZZ, ref RR,
                // Граничные условия для потоковой скорости и боковой скорости на свободной поверхности
                mbU, mAdressU, mbV, mQ, flagIndexUy, flagUx, VortexBC_G2, typeEddyViscosity, flagLes, idxMu2);

            double[] mDistance = wMesh.GetDistance();
            double[] Hp = wMesh.GetHp();

            string NameMuModel = listBoxAMu.Items[listBoxAMu.SelectedIndex].ToString();
            string NameCross = lb_CrossNamber.Items[lb_CrossNamber.SelectedIndex].ToString();
            string NameBC = lb_VortexBC_G2.Items[lb_VortexBC_G2.SelectedIndex].ToString();
            string NameTT = lb_Ring.Items[lb_Ring.SelectedIndex].ToString();
            string NameUs = (ls_Type__U_star.Items[ls_Type__U_star.SelectedIndex].ToString()).Substring(0, 2);
            string spName = NameMuModel + " " + NameCross + " " + NameBC + " " + NameUs + " " + NameTT;


            SavePoint sp = new SavePoint(spName);
            sp.SetSavePoint(0, mesh);

            double[] x = mesh.GetCoords(0);
            double[] y = mesh.GetCoords(1);

            sp.Add("Скорость Vx", mVx);
            sp.Add("Скорость Vy", mVy);
            sp.Add("Скорость Vz", mVz);
            sp.Add("Скорость", mVy, mVz);
            sp.Add("Турбулентная вязкость по X", eddyViscosityUx);
            sp.Add("Турбулентная вязкость по YZ", meddyViscosity);

            sp.Add("Phi", mPhi);
            sp.Add("mVortex", mVortex);
            sp.Add("tauXY", tauXY);
            sp.Add("tauXZ", tauXZ);
            sp.Add("TauX", tauXY, tauXZ);

            sp.Add("tauYY", tauYY);
            sp.Add("tauYZ", tauYZ);
            sp.Add("tauZZ", tauZZ);

            if (mDistance != null)
                sp.Add("mDistance", mDistance);
            if (mDistance != null)
                sp.Add("Эф. глубина", Hp);

            sp.Add("Координата Х", x);
            sp.Add("Координата Y", y);

            // кривые
            double[] by = null;
            double[] bz = null;
            double[] Us = null;
            uint[] ba = wMesh.GetBoundaryBedAdress();
            wMesh.BedCoords(ref by, ref bz);
            wMesh.CalkBoundary_U_star(mVx, ref Us);
            sp.AddCurve("Динамическая скорость", by, Us);
            double UsJ = SPhysics.PHYS.Get_U_star_J(wMesh, J);
            for (int i = 0; i < ba.Length; i++)
                Us[i] = UsJ;
            sp.AddCurve("Динамическая скорость J", by, Us);
            double UsU = SPhysics.PHYS.Get_U_star_Vx(wMesh, mVx);
            for (int i = 0; i < ba.Length; i++)
                Us[i] = UsU;
            sp.AddCurve("Динамическая скорость Ux", by, Us);

            for (int i = 0; i < ba.Length; i++)
                Us[i] = mVy[ba[i]];
            sp.AddCurve("Придонная скорость Vy", by, Us);

            Form form = new ViForm(sp);
            form.Text = sp.Name;
            form.Show();

        }
        
        private void lb_Ring_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lb_Ring.SelectedIndex == 0)
            {
                SelectedIndexSave = lb_VortexBC_G2.SelectedIndex;
                lb_VortexBC_G2.SelectedIndex = 2;
                lb_VortexBC_G2.Enabled = false;
            }
            else
            {
                lb_VortexBC_G2.SelectedIndex = SelectedIndexSave;
                lb_VortexBC_G2.Enabled = true;
            }
        }

        private void listBoxAMu_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cb_Mu_YZ.Checked == false)
                listBoxAMu2.SelectedIndex = listBoxAMu.SelectedIndex;
        }
    }
}
