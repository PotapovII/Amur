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
    using CommonLib.EddyViscosity;

    public partial class FDitrich : Form
    {
        IMesh mesh = null;

        double diametrFE;
        double R_midle = 9; //- 0.8;

        double[] YDitrich14 = { -1,0,    0.13,    0.27,    0.34,    0.47,    0.59,    0.64,    0.71,    0.94,    1.12,    1.25,    1.44,    1.8,    2.08,    2.38,    2.54,    2.81,    2.96,    3.1, 3.27,    3.44,    3.6, 3.77,    3.93,    4.06,    4.27,    4.5, 4.71,    4.85,    4.92,    4.922,   4.93,    4.94,    4.945,   4.948,   4.95,    4.955,   4.96,    4.97,    4.98,    4.99,    5, 5.01 };
        double[] HDitrich14 = { -1, 0,   0.24,    0.47,    0.55,    0.63,    0.67,    0.68,    0.68,    0.62,    0.59,    0.57,    0.55,    0.53,    0.51,    0.485,   0.47,    0.43,    0.415,   0.405,   0.4, 0.395,   0.393,   0.387,   0.38,    0.376,   0.37,    0.363,   0.355,   0.35,    0.33,    0.3, 0.26,    0.21,    0.18,    0.14,    0.11,    0.08,    0.055,   0.045,   0.025,   0.012,   0, -1 };

        const double L = 10;

        double WL = 0.140;
        IDigFunction Geometry;
        int SelectedIndexSave = 0;
        public FDitrich()
        {
            InitializeComponent();
            lb_Ring.SelectedIndex = 1;
            listBoxAMu.SelectedIndex = 11;
            lb_VortexBC_G2.SelectedIndex = 2;
            SelectedIndexSave = lb_VortexBC_G2.SelectedIndex;
            lb_CrossNamber.SelectedIndex = 0;
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
            switch (lb_CrossNamber.SelectedIndex)
            {
                case 0:
                    Geometry = Create(YDitrich14, HDitrich14);
                    break;
                //case 3:
                //    Geometry = Create(YDesna4, HDesna4);
                //    break;
                //case 5:
                //    Geometry = Create(YDesnaT, HDesnaT);
                //    break;
            }
            Geometry.GetFunctionData(ref x, ref y, NN);
            //HStripMeshGeneratorTri sg = new HStripMeshGeneratorTri();
            //CrossStripMeshGeneratorTri sg = new CrossStripMeshGeneratorTri();
            //mesh = sg.CreateMesh(ref GR, WL, x, y);
            IStripMeshGenerator sg = null;
            CrossStripMeshOption op = new CrossStripMeshOption();
            switch (lb_MeshGen.SelectedIndex)
            {
                case 0:
                    sg = new HStripMeshGeneratorTri(op);
                    break;
                case 1:
                    sg = new CrossStripMeshGeneratorTri(op);
                    break;
                case 2:
                    sg = new CrossStripMeshGenerator(op);
                    break;
            }
            //WL = 3;
            int[][] riverGates = null;
            mesh = sg.CreateMesh(ref GR, ref riverGates, WL, x, y);

            ShowMesh();
        }
        protected void ShowMesh()
        {
            if (mesh != null && checkBoxView.Checked == true)
            {
                IMWCross wMesh = new MWCrossTri(mesh, SСhannelForms.porabolic, 2);
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
            #region Река Muddy Creek (Dierich,81)

            // выбор граничных условий створа
            switch (lb_CrossNamber.SelectedIndex)
            {
                case 0: // Створ 1 Десна
                    {
                        double[] YDitrich14SU = {0,     0.72,   0.93,   1.04,   1.32,   1.76,   2.21,   3.34,   4.25,   4.29, 5.0 };
                        double[] YDitrich14U = { 0,     0.4,    0.45,   0.5,    0.55,   0.6,    0.65,   0.7,    0.7,    0.7,    0 };
                        //
                        double[] YDitrich14SV = {0,     0.96,   1.99,   3.6,    3.87,   4.59,   0};
                        double[] YDitrich14V = { 0,     0.05,   0.1,    0.1,    0.05,   0,      0 };
                        //
                        VelosityUx = new DigFunction(YDitrich14SU, YDitrich14U, "Створ14");
                        VelosityUy = new DigFunction(YDitrich14SV, YDitrich14V, "Створ14");
                        R_midle = Rr - YDitrich14SU[YDitrich14SU.Length - 1] / 2;
                    }
                    break;
                case 1: // Створ 2 Десна
                    {
                    }
                    break;
                case 2: // Створ 3 Десна
                    {
                    }
                    break;
                case 3: // Створ 4 Десна
                    {
                       
                    }
                    break;
                case 4: // Створ 5 Десна
                    {

                    }
                    break;
                case 5: // Тест
                    {
                        
                        
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
                        //if(cb_smoothing.Checked == true && CalkU!=null)
                        //{
                        //    mBC_U[idx1] = CalkU(x1);
                        //    mBC_U[idx2] = CalkU(x2);
                        //}
                        //else
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
            for (uint i = 0; i < mBC_U.Length; i++)
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
