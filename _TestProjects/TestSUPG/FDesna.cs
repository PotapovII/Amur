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
    using GeometryLib.Vector;

    public partial class FDesna : Form
    {
        IMesh mesh = null;

        double diametrFE;
        double R_midle = 5; //- 0.8;
        /// <summary>
        /// Створ 1
        /// </summary>
        double[] YDesna1 = { -1, 0,  4.0, 10.5, 19.0, 27.0, 34.5, 42.5, 53.5, 60.5, 65,  70.5, 80.5, 89.5, 98.5, 109.5, 122,  135.5, 147,  148.5, 157.5, 158.5 };
        double[] HDesna1 = { -1, 0, 1.55,  3.3, 3.3,  3.6,  3.65,  3.7, 3.75, 3.4,  3.3, 3.4,  3.45, 3,    3,    2.95,  2.55, 2.8,   2.05, 1.4,       0,  -1   };


        double[] YAmur1 = { 8.07850757125003, 155.39594753188, 302.71338749251, 450.03082745314, 597.34826741377, 744.6657073744, 891.98314733503, 1039.30058729566, 1186.61802725629, 1333.93546721692 };
        double[] HAmur1 = { -0.00597974425919433, 0.0402060537833953, 0.0260593290249211, 0.04007607290018, 0.101653114708262, 0.202543622249814, 0.339754922566874, 0.495670267215276, 0.921772719589645, -0.23748311347325 };

        double[] YAmur2 = { 8.07850757125003, 155.39594753188, 302.71338749251, 450.03082745314, 597.34826741377, 744.6657073744, 891.98314733503, 1039.30058729566, 1186.61802725629, 1333.93546721692 };
        double[] HAmur2 = { -0.00597974425919433, 0.0402060537833953, 0.0260593290249211, 0.04007607290018, 0.101653114708262, 0.202543622249814, 0.339754922566874, 0.495670267215276, 0.921772719589645, -0.23748311347325 };
        /// <summary>
        /// Створ 3
        /// </summary>
        double[] YDesna3 = { -26.5, 27.5, 28.5,  33, 34,  38.0, 41.5, 45.0, 50.0, 53.5, 58, 64.5, 68.0, 72.0, 77.0, 83.0, 88.0, 90.0, 99,  103.0, 109, 118.5, 124.5, 127.5, 131.0, 141.5, 145,  151, 157, 159.5, 160.5};
        double[] HDesna3 = { -1, 0,   0.30, 0.6, 0.8, 1.2,  1.6,  1.8,  1.9,  1.8,   2, 2.4,  2.6,  2.8,  3.4,  3.4,  4.0,  3.8,  5.0, 5.2,   5.2, 6.1,   7.6,   6.0,   6.0,    4.8,  5.25, 3.2, 2.2, 0,     -1 };
        /// <summary>
        /// Створ 4
        /// </summary>
        double[] YDesna4 = { 28, 29, 39, 43.5, 48.5, 52.0, 56.0, 63.5, 69.5, 74.0, 78.5, 83.5, 89.0, 93.0, 99.5, 104.5, 113, 120.0, 127,  130.0, 134, 139.5, 146.5, 151, 155, 160, 161 };
        double[] HDesna4 = { -1,  0, 0.55, 0.8, 1.45, 1.5, 2.2,  2.8,  3.15, 3.6,  4.0,  4.0,  4.25, 4.6,  4.7,  5.0,   5.4, 6.6,   6.05, 5.95,  6.1, 6.0,   5.6,   3.0, 1.2,  0,  -1 };

        //double[] YDesnaT= {  -1, 0,  50.0, 150,  160, 161 };
        //double[] YDesnaT = { -1, 0, 10.0, 150,    160, 161 };
        const double L = 10;
        double[] YDesnaT = { -1, 0,  1.0, L, L+1, L+2 };
        //double[] HDesnaT = { -1, 0,    6.6,   5.6,    0, -1 };
        double[] HDesnaT = { -1, 0,  1,  1,  0, -1 };


        double WL = 0.140;
        IDigFunction Geometry;
        int SelectedIndexSave = 0;
        public FDesna()
        {
            InitializeComponent();
            lb_Ring.SelectedIndex = 1;
            listBoxAMu.SelectedIndex = 11;
            lb_VortexBC_G2.SelectedIndex = 2;
            SelectedIndexSave = lb_VortexBC_G2.SelectedIndex; 
            lb_CrossNamber.SelectedIndex = 1;
            lb_Algebra.SelectedIndex = 0;
            lb_MeshGen.SelectedIndex = 0;
            ls_Type__U_star.SelectedIndex = 0;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            River_Mesh();
        }

        /// <summary>
        /// Расчет донных отметок по глубинам и создание функции геометрии створа 
        /// </summary>
        /// <param name="Y"></param>
        /// <param name="H"></param>
        /// <returns></returns>
        public DigFunction Create(double[] Y, double[] H)
        {
            //if(cb_Revers.Checked == true)
            //{
            //    double maxY = Y.Max();
            //    Y.Reverse();
            //    for (int i = 0; i < Y.Length; i++)
            //        Y[i] = maxY - Y[i];
            //    H.Reverse();
            //}
            double[] Z = new double[Y.Length];
            WL = H.Max();
            for (int i = 0; i < Y.Length; i++)
                Z[i] = WL - H[i];
            return new DigFunction(Y, Z, "Створ");
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
            switch(lb_CrossNamber.SelectedIndex)
            {
                case 0: 
                    Geometry = Create(YDesna1, HDesna1);
                    break;
                case 1:
                    GEM.NormMas(ref YAmur1,10);
                    double ww = GEM.NormMaxMas(ref HAmur1);
                    WL = ww;
                    Geometry = Create(YAmur1, HAmur1);
                    break;
                case 2:
                    Geometry = Create(YDesna3, HDesna3);
                    break;
                case 3:
                    Geometry = Create(YDesna4, HDesna4);
                    break;
                case 4:
                case 5:
                    Geometry = Create(YDesnaT, HDesnaT);
                    break;
            }
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
            //WL = 3;
            mesh = sg.CreateMesh(ref GR, WL, x, y);
            ShowMesh();
        }
        protected void ShowMesh()
        {
            if (mesh != null && checkBoxView.Checked == true)
            {
                IMWCross wMesh = new MWCrossTri(mesh, SСhannelForms.porabolicСhannelSection, 2);
                SavePoint data = new SavePoint();
                data.SetSavePoint(0, mesh);
                double[] xx = mesh.GetCoords(0);
                double[] yy = mesh.GetCoords(1);
                data.Add("Distance", wMesh.GetDistance());
                data.Add("Hp", wMesh.GetHp());
                data.Add("Координата Х", xx);
                data.Add("Координата Y", yy);
                data.Add("Координаты ХY", xx, yy);
                DateTime dateTime = DateTime.Now;
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
            double Rr = double.Parse(tb_R.Text, MEM.formatter);
            double Qw = double.Parse(tb_Q.Text, MEM.formatter);
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
            #region Река Десна (Розовский)
            // выбор граничных условий створа
            switch (lb_CrossNamber.SelectedIndex)
            {
                case 0: // Створ 1 Десна
                    {
                        double[] YDesna1S = { 0, 19.0, 42.5, 65, 89.5, 109.5, 135.5, 157.5 };
                        double[] YDesna1U = { 0, 0.64, 0.72, 0.7, 0.55, 0.4, 0.3, 0 };
                        double[] YDesna1V = { 0, -0.02, 0.02, -0.01, -0.05, -0.04, -0.03, 0 };
                        VelosityUx = new DigFunction(YDesna1S, YDesna1U, "Створ");
                        VelosityUy = new DigFunction(YDesna1S, YDesna1V, "Створ");
                        R_midle = Rr - YDesna1S[YDesna1S.Length - 1] / 2;
                    }
                    break;
                case 1: // Створ Амура o. Б.У.-o.Kaб.
                    {
                        Rr = 1000;
                        double[] YAmurS = { 14.9612858994447, 160.141012734915, 305.320739570386, 450.500466405857, 595.680193241328, 740.859920076798, 886.039646912269, 1031.21937374774, 1176.39910058321, 1319.0, 1321.57882741868 };
                        double[] YAmurU = { 0.270986989473428, 0.391481823084925, 0.504930159975303, 0.614403229678924, 0.764885412362173, 0.772160811366355, 0.772286288792793, 0.822947815893047, 0.932657482572942, 0.8, 0 };
                        double[] YAmurV = { 0.435043291868031, 0.599031560976853, 0.746860855492965, 0.851404132892535, 0.988678259569121, 0.838052424893014, 0.750298893937259, 0.670376134562141, 0.503998658845548, 0.300 };
                        double LR = GEM.NormMas(ref YAmurS,10);

                        VelosityUx = new DigFunction(YAmurS, YAmurU, "Створ");
                        VelosityUy = new DigFunction(YAmurS, YAmurV, "Створ");
                        R_midle = Rr/LR - (YAmurS[YAmurS.Length - 1] - YAmurS[0]) / 2;
                    }
                    break;
                case 2: // Створ 3 Десна
                    {
                        double[] YDesna3 = { -26.5, 27.5, 28.5, 33, 34, 38.0, 41.5, 45.0, 50.0, 53.5, 58, 64.5, 68.0, 72.0, 77.0, 83.0, 88.0, 90.0, 99, 103.0, 109, 118.5, 124.5, 127.5, 131.0, 141.5, 145, 151, 157, 159.5, 160.5 };
                        double[] HDesna3 = { -1,    0,    0.30, 0.6, 0.8, 1.2, 1.6, 1.8, 1.9, 1.8, 2, 2.4, 2.6, 2.8, 3.4, 3.4, 4.0, 3.8, 5.0, 5.2, 5.2, 6.1, 7.6, 6.0, 6.0, 4.8, 5.25, 3.2, 2.2, 0, -1 };

                        double[] YDesna3S = { 0, 28.5, 50.0, 72.0, 90.0, 109, 127.5, 145, 157.5, 159.5 };
                        double[] YDesna3U = { 0, 0.35, 0.41, 0.47, 0.52, 0.55, 0.55, 0.55, 0.45, 0 };
                        //double[] YDesna3V = { 0, 0.02, 0.06, 0.06, 0.7, 0.08, 0.04, 0 };
                        double[] YDesna3V = { 0, 0.016, 0.02, 0.03, 0.06, 0.7, 0.08, 0.04, 0.03, 0 };
                        VelosityUx = new DigFunction(YDesna3S, YDesna3U, "Створ");
                        VelosityUy = new DigFunction(YDesna3S, YDesna3V, "Створ");
                        R_midle = Rr - (YDesna3S[YDesna3S.Length - 1] - YDesna3S[0]) / 2;
                    }
                    break;
                case 3: // Створ 4 Десна
                    {
                        double[] YDesna4S = { 0, 52.0, 69.5, 99.5, 120, 139.5, 160.0 };
                        double[] YDesna4U = { 0, 0.4, 0.4, 0.45, 0.5, 0.47, 0 };
                        double[] YDesna4V = { 0, 0.02, 0.06, 0.04, 0.05, 0.04, 0 };
                        //double[] YDesna3V = { 0, 0.05, 0.05, 0.05, 0.05, 0.05, 0 };
                        VelosityUx = new DigFunction(YDesna4S, YDesna4U, "Створ");
                        VelosityUy = new DigFunction(YDesna4S, YDesna4V, "Створ");
                        R_midle = Rr - (YDesna4S[YDesna4S.Length - 1] - YDesna4S[0]) / 2;
                    }
                    break;
                case 4: // Створ 5 Десна
                    {

                    }
                    break;
                case 5: // Тест
                    {
                        //double[] YDesnaT = { -1, 0, 10.0, 150, 160, 161 };
                        //YDesnaT = { -1, 0,  1.0,  15,  16, 17 };
                        double[] YDesnaU = { 0, 0, 1.5,  1.5,  0, 0 };
                        //double[] YDesnaV = { 0, 0.1, 0.1,  0.1,  0.1, 0.1,   0.1, 0.1,   0 };
                        double[] YDesnaV = { 0, 0, 0.01, 0.01, 0, 0 };
                        VelosityUx = new DigFunction(YDesnaT, YDesnaU, "Створ");
                        VelosityUy = new DigFunction(YDesnaT, YDesnaV, "Створ");
                        R_midle = Rr - (YDesnaT[YDesnaT.Length - 1] - YDesnaT[0]) / 2;
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
                        bKnotsU[idx1] = 1;
                        bKnotsU[idx2] = 1;
                        mBC_U[idx1] = VelosityUx.FunctionValue(x1);
                        mBC_U[idx2] = VelosityUx.FunctionValue(x2);
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

            RiverCrossVortexPhiTri task = new RiverCrossVortexPhiTri(wMesh, algebra, TypeTask.streamY1D, w);
            
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
            sp.Add("R_Phi", task.tmpRPhi);
            sp.Add("mVortex", mVortex);
            sp.Add("tauXY", tauXY);
            sp.Add("tauXZ", tauXZ);
            sp.Add("TauX", tauXY, tauXZ);

            sp.Add("tauYY", tauYY);
            sp.Add("tauYZ", tauYZ);
            sp.Add("tauZZ", tauZZ);
            sp.Add("Radius", RR);

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
