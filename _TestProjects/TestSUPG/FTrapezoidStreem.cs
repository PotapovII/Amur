namespace TestSUPG
{
    using MemLogLib;
    using CommonLib;
    using CommonLib.Mesh;

    using MeshLib;
    using MeshLib.Wrappers;
    using MeshGeneratorsLib.StripGenerator;

    using RenderLib;

    using System;
    using System.Windows.Forms;
    using CommonLib.Physics;
    using CommonLib.Function;
    using AlgebraLib;
    using FEMTasksLib.FESimpleTask;
    using System.Linq;
    using GeometryLib;
    using CommonLib.EddyViscosity;

    public partial class FTrapezoidStreem : Form
    {
        double WL = 0;
        double HL = 0;
        double HR = 0;
        double L = 0;
        double J = 0;
        double R0 = 0;
        double diametrFE, dx, dy;
        IMesh mesh = null;


        protected void QUAD_Mesh()
        {
            // 
            //                    HL     WL
            // |-----------|-------------|------> z
            // |          /--------------|
            // |         /---------------|
            // |       /-----------------|
            // |     |-------------------|
            // |     |-------------------|
            // V R ---\------------------|
            // |        \----------------|
            // |        /----------------|
            // |       /-----------------|
            // |      /------------------|
            // |     /-------------------|
            // |    /--------------------|
            // |             HR
            // |    
            // V y    


            WL = double.Parse(textBoxW.Text, MEM.formatter);
            HL = double.Parse(textBoxHL.Text, MEM.formatter);
            HR = double.Parse(textBoxHR.Text, MEM.formatter);
            L = double.Parse(textBoxL.Text, MEM.formatter);
            double H = Math.Max(HL, HR);
            double WetBed = 0;
            double WaterLevel = H;
            double PFE = double.Parse(textBoxDiam.Text, MEM.formatter);
            diametrFE = Math.Min(L, H) / 100 * PFE;
            
            CrossStripMeshOption op = new CrossStripMeshOption();
            op.channelSectionForms = SСhannelForms.trapezoid;
            switch (lb_AreaGeom.SelectedIndex)
            {
                case 0:
                    {
                        double[] xx = { 0, L };
                        double[] yy = { WL - HL, WL - HR };
                        int Ny = (int)(L / diametrFE) + 1;
                        
                        IStripMeshGenerator sg = new CrossStripMeshGeneratorTri(op);
                        int[][] riverGates = null;
                        mesh = sg.CreateMesh(ref WetBed, ref riverGates, WaterLevel, xx, yy, Ny);

                    }
                    break;
                case 1:
                    {
                        double[] xx = { 0, L / 3, 2 * L / 3, L };
                        double[] yy = { WL - HL, WL - 0.6 * HL, WL - 1.2 * HL, WL - HR };
                        int Ny = (int)(L / diametrFE) + 1;
                        IStripMeshGenerator sg = new CrossStripMeshGeneratorTri(op);
                        int[][] riverGates = null;
                        mesh = sg.CreateMesh(ref WetBed, ref riverGates, WaterLevel, xx, yy, Ny);

                    }
                    break;
                case 2:
                    {
                        double[] xx = { 0, L / 3, 2 * L / 3, L };
                        double[] yy = { WL - HL, WL -  1.5*HL, WL - 0.6 * HR, WL - HR };
                        int Ny = (int)(L / diametrFE) + 1;
                        IStripMeshGenerator sg = new CrossStripMeshGeneratorTri(op);
                        int[][] riverGates = null;
                        mesh = sg.CreateMesh(ref WetBed, ref riverGates, WaterLevel, xx, yy, Ny);
                    }
                    break;
                case 3:
                    {
                        double[] xx = { 0, L / 3, 2 * L / 3, L };
                        double[] yy = { -WL, WL - 1.5 * HL, WL - 0.6 * HR, -WL };
                        int Ny = (int)(L / diametrFE) + 1;
                        IStripMeshGenerator sg = new CrossStripMeshGeneratorTri(op);
                        int[][] riverGates = null;
                        mesh = sg.CreateMesh(ref WetBed, ref riverGates, WaterLevel, xx, yy, Ny);
                    }
                    break;
            }
            ShowMesh();
        }

        protected void ShowMesh()
        {
            if (mesh != null && checkBoxView.Checked == true)
            {
                IMWCross wMesh = new MWCrossTri(mesh, SСhannelForms.trapezoid, 2);
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


        private void button3_Click(object sender, EventArgs e)
        {
            QUAD_Mesh();
        }

        int SelectedIndexSave = 0;
        public FTrapezoidStreem()
        {
            InitializeComponent();
            lb_Ring.SelectedIndex = 1;
            listBoxAMu.SelectedIndex = 11;
            listBoxAMu2.SelectedIndex = listBoxAMu.SelectedIndex;
            lb_VortexBC_G2.SelectedIndex = 2;
            
            SelectedIndexSave = lb_VortexBC_G2.SelectedIndex;
            lb_CrossNamber.SelectedIndex = 0;
            lb_Algebra.SelectedIndex = 0;
            lb_Renumberator.SelectedIndex = 0;
            ls_Type__U_star.SelectedIndex = 0;
            lb_AreaGeom.SelectedIndex = 0;
        }

        private void btStart_Click(object sender, EventArgs e)
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

            QUAD_Mesh();

            if (mesh == null) return;
            double w = double.Parse(tb_w.Text, MEM.formatter);
            double J = double.Parse(tb_J.Text, MEM.formatter);
            double R0 = double.Parse(tb_R0.Text, MEM.formatter);
            double R_min = R0 - L / 2;
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

            double[] BoxY = { 0, 0.05 * L, 0.95 * L, L };
            double[] U15 = { 0, 0.2, 0.2, 0.0 };
            double[] V15 = { 0, 0.03, 0.03, 0.0 };

            // выбор граничных условий створа
            switch (lb_CrossNamber.SelectedIndex)
            {
                case 0: // Створ 29
                    {
                        double[] Y29 = { 0, 0.023, 0.035, 0.06, 0.1, 0.171, 0.25, 0.33, 0.4, 0.44, 0.465, 0.485, 0.5 };
                        double[] U29 = { 0, 0.1863, 0.1911, 0.2176, 0.252, 0.29, 0.2922, 0.2848, 0.2815, 0.2547, 0.2204, 0.2009, 0 };
                        double[] V29 = { 0, 0.00458, 0.01115, 0.01494, 0.01992, 0.02015, 0.0259, 0.02613, 0.01922, -0.00088, -0.01871, -0.01352, 0 };
                        VelosityUx = new DigFunction(Y29, U29, "Створ U29");
                        VelosityUy = new DigFunction(Y29, V29, "Створ V29");
                    }
                    break;
                case 1: // Створ 60
                    {
                        double[] Y60 = { 0, 0.023, 0.035, 0.06, 0.1, 0.17, 0.25, 0.33, 0.4, 0.44, 0.465, 0.485, 0.5 };
                        double[] U60 = { 0, 0.157, 0.185, 0.208, 0.2332, 0.2573, 0.279, 0.2885, 0.2977, 0.2732, 0.2363, 0.2079, 0 };
                        double[] V60 = { 0, 0.00476, 0.01156, 0.01472, 0.01752, 0.01639, 0.02066, 0.02341, 0.01076, -0.0005, -0.01106, -0.01384, 0 };
                        VelosityUx = new DigFunction(Y60, U60, "Створ U60");
                        VelosityUy = new DigFunction(Y60, V60, "Створ V60");
                    }
                    break;
                case 2: // Створ 135
                    {
                        double[] Y145 = { 0, 0.023, 0.035, 0.06, 0.1, 0.17, 0.25, 0.33, 0.4, 0.44, 0.465, 0.485, 0.5 };
                        double[] U145 = { 0, 0.1613, 0.1847, 0.2072, 0.2357, 0.2642, 0.2847, 0.3014, 0.3266, 0.3239, 0.2823, 0.2522, 0 };
                        double[] V145 = { 0, 0.00519, 0.01092, 0.01491, 0.01774, 0.0201, 0.02062, 0.02124, 0.00954, -0.00176, -0.0167, -0.02049, 0 };
                        VelosityUx = new DigFunction(Y145, U145, "Створ U145");
                        VelosityUy = new DigFunction(Y145, V145, "Створ V145");
                    }
                    break;
                case 3: // тест 1 - контроль граничных условий / 1 вихрь  
                    {
                        double[] Y135 = { 0, 0.05, 0.10, 0.15, 0.20, 0.25, 0.30, 0.35, 0.40, 0.45, 0.50 };
                        double[] U135 = { 0, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0 };
                        double[] V135 = { 0, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02, 0 };
                        VelosityUx = new DigFunction(Y135, U135, "Створ U135");
                        VelosityUy = new DigFunction(Y135, V135, "Створ V135");
                    }
                    break;
                case 4: // тест 2 - контроль граничных условий / 2 вихря
                    {
                        double[] Y135 = { 0, 0.05, 0.10, 0.15, 0.20, 0.25, 0.30, 0.35, 0.40, 0.45, 0.50 };
                        double[] U135 = { 0, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0 };
                        double[] V135 = { 0, -0.02, -0.02, -0.02, -0.02, 0.0, 0.02, 0.02, 0.02, 0.02, 0 };
                        VelosityUx = new DigFunction(Y135, U135, "Створ U135");
                        VelosityUy = new DigFunction(Y135, V135, "Створ V135");
                    }
                    break;
            }
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
                if (Mark == 0 || Mark == 1 || Mark == 3) // дно канала
                {
                    bKnotsU[idx1] = 1;
                    mBC_U[idx1] = 0;
                    bKnotsU[idx2] = 1;
                    mBC_U[idx2] = 0;
                }
                if (Mark == 2) // свободная поверхность
                {
                    bKnotsU[idx1] = 1;
                    bKnotsU[idx2] = 1;
                    bKnotsV[idx1] = 1;
                    bKnotsV[idx2] = 1;

                    mBC_U[idx1] = VelosityUx.FunctionValue(x1);
                    mBC_U[idx2] = VelosityUx.FunctionValue(x2);

                    mBC_V[idx1] = VelosityUy.FunctionValue(x1);
                    mBC_V[idx2] = VelosityUy.FunctionValue(x2);
                }
            }
            double[] mbU = null;
            double[] mbV = null;
            uint[] mAdressU = null;
            int CountU = bKnotsU.Sum(xx => xx == 1 ? xx : 0);
            int CountV = bKnotsV.Sum(xx => xx == 1 ? xx : 0);
            MEM.Alloc(CountU, ref mbU, 0);
            MEM.Alloc(CountV, ref mbV, 0);
            MEM.Alloc(CountU, ref mAdressU);
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
            
            IMWCrossSection wMesh = new MWCrossSectionTri(mesh, R_min, Ring,
                                                      false, SСhannelForms.trapezoid);
            // Определение наальной вязкости
            SPhysics.PHYS.turbViscType = (ETurbViscType)listBoxAMu.SelectedIndex;


            ECalkDynamicSpeed typeEddyViscosity = (ECalkDynamicSpeed)ls_Type__U_star.SelectedIndex;

            IAlgebra algebra = null;
            if (lb_Algebra.SelectedIndex == 0)
            {
                uint NH = mesh.GetWidthMatrix();
                algebra = new AlgebraLUTape((uint)mesh.CountKnots, (int)NH + 1, (int)NH + 1);
            }
            else
                algebra = new SparseAlgebraBeCG((uint)mesh.CountKnots, false);

            RiverCrossVortexPhiTri task = new RiverCrossVortexPhiTri(wMesh, algebra, TypeTask.streamY1D, w);

            int flagIndexUy = lb_VortexBC_G2.SelectedIndex;
            int VortexBC_G2 = lb_VortexBC_G2.SelectedIndex;
            bool flagUx = cbBoundaryG2_Ux.Checked;
            bool flagLes = cb_Mu_YZ.Checked;
            int idxMu2 = listBoxAMu2.SelectedIndex;

            double[] eddyViscosityUx = null;
            double[] RR = null;
            task.CalkVortexStream(
                // Искомые поля
                ref mPhi, ref mVortex, ref mVx, ref mVy, ref mVz, ref eddyViscosityUx, ref meddyViscosity,
                ref tauXY, ref tauXZ, ref tauYY, ref tauYZ, ref tauZZ, ref RR,
                // Граничные условия для потоковой скорости и боковой скорости на свободной поверхности
                mbU, mAdressU, mbV, mQ, flagIndexUy, flagUx, VortexBC_G2, typeEddyViscosity, flagLes, idxMu2);

            double[] mDistance = wMesh.GetDistance();

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

    }
}
