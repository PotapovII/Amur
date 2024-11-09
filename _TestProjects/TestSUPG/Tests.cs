namespace TestSUPG
{
    using AlgebraLib;

    using CommonLib;
    using CommonLib.Mesh;
    using CommonLib.Physics;
    using CommonLib.Geometry;

    using MemLogLib;
    using GeometryLib;

    using MeshLib;
    using MeshLib.Wrappers;
    using MeshGeneratorsLib;
    using MeshGeneratorsLib.Renumberation;

    using FEMTasksLib.FESimpleTask;

    using RenderLib;

    using System;
    using System.Windows.Forms;
    using System.Collections.Generic;
    public partial class Tests : Form
    {
        // КЭ сетка
        IMesh mesh = null;
        double H = 0;
        double L = 0;
        double diametrFE;
        public Tests()
        {
            InitializeComponent();
      
            listBoxRange.SelectedIndex = 0;
            listBoxTypeMesh.SelectedIndex = 1;
            listBoxFrontRen.SelectedIndex = 0;
            listBoxTasks.SelectedIndex = 2;
            listBoxArea.SelectedIndex = 0;
            lb_Algebra.SelectedIndex = 0;
            ls_Type__U_star.SelectedIndex = 0;
            listBoxAMu.SelectedIndex = 0;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            GetMesh();
        }
        /// <summary>
        /// генерация КЭ СЕТКИ в тестовой области
        /// </summary>
        void GetMesh()
        {
            GetHLD();
            QUAD_Mesh();
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

            double Mu = double.Parse(tb_Mu0.Text, MEM.formatter);
            double dt = double.Parse(tb_dt.Text, MEM.formatter);
            double Q = double.Parse(tb_Q.Text, MEM.formatter);
            double U = double.Parse(tb_U.Text, MEM.formatter);
            double V = double.Parse(tb_V.Text, MEM.formatter);
            //double bcL = double.Parse(tb_L.Text, MEM.formatter);
            //double bcR = double.Parse(tb_R.Text, MEM.formatter);
            //double bcT = double.Parse(tb_T.Text, MEM.formatter);
            //double bcB = double.Parse(tb_B.Text, MEM.formatter);

            MEM.Alloc(mesh.CountKnots, ref mT, 0);
            MEM.Alloc(mesh.CountKnots, ref meddyViscosity, Mu);
            MEM.Alloc(mesh.CountKnots, ref mVx);
            MEM.Alloc(mesh.CountKnots, ref mVy);
            MEM.Alloc(mesh.CountKnots, ref mQ, Q);

            ///// <summary>
            ///// Массив меток  для граничных узловых точек
            ///// </summary>
            int[] bknots = mesh.GetBoundKnots();
            int[] Marks = mesh.GetBoundKnotsMark();

            List<uint> idxWL = new List<uint>();
            List<uint> idxWaLL = new List<uint>();
            List<uint> idxBed = new List<uint>();
            List<uint> idxIN = new List<uint>();
            List<uint> idxOUT = new List<uint>();

            List<uint> idxPhi = new List<uint>();
            List<double> bcPhi = new List<double>();
            //   ^ z(y)      Phi = Q
            //   |           mark = 2          
            //   |----------------------------|
            //   |                            |
            //   |                            |
            //   |  mark = 3                  |  mark = 1
            //   |                            |
            //   |          mark = 0          |
            //   |----------------------------|----> y (x)
            //              Phi = 0
            // ----------------------------------------------
            //  Канал u := U*z*(2*H-z)/H^2
            //        phi := (1-1/3*z/H)*U*z^2/H
            //  Щель  u := 4*U*z*(H-z)/H^2
            //        phi := -4/3*U/H^2*z^3+2*U/H*z^2
            // ----------------------------------------------
            double[] X = mesh.GetCoords(0);
            double[] Y = mesh.GetCoords(1);
            
            double Umax = Q / H;
            for (int i = 0; i < Marks.Length; i++)
            {
                int Mark = Marks[i];
                if (Mark == 0)
                {
                    //  Phi = 0
                    idxPhi.Add((uint)bknots[i]);
                    idxWaLL.Add((uint)bknots[i]);
                    idxBed.Add((uint)bknots[i]);
                    bcPhi.Add(0);
                }
                if (Mark == 1)
                {
                    idxPhi.Add((uint)bknots[i]);
                    idxOUT.Add((uint)bknots[i]);
                    idxWaLL.Add((uint)bknots[i]);
                    double z = Y[bknots[i]];
                    //double phi = 2 * Umax * z * z / H * (1 - 2.0 / 3 * z / H);
                    double phi = (1 - 1.0 / 3 * z / H) * U * z * z / H;
                    bcPhi.Add(phi);
                }
                if (Mark == 3)
                {
                    idxPhi.Add((uint)bknots[i]);
                    idxIN.Add((uint)bknots[i]);
                    idxWaLL.Add((uint)bknots[i]);
                    double z = Y[bknots[i]];
                    //double phi = 2 * Umax * z * z / H * (1 - 2.0 / 3 * z / H);
                    double phi = (1 - 1.0 / 3 * z / H) * U * z * z / H;
                    bcPhi.Add(phi);
                }
                if (Mark == 2)
                {
                    idxPhi.Add((uint)bknots[i]);
                    idxWL.Add((uint)bknots[i]);
                    double phi =  Umax * H * (2.0 / 3.0);
                    bcPhi.Add(phi);
                }
            }
            bcAdressPhi = idxPhi.ToArray();
            bcValuePhi = bcPhi.ToArray();
            uint[] bcAdress_WL = idxWL.ToArray();
            uint[] bcAdress_Bed = idxBed.ToArray();
            uint[] bcAdress_WaLL = idxWaLL.ToArray();
            uint[] bcAdress_IN = idxIN.ToArray();
            uint[] bcAdress_OUT = idxOUT.ToArray();
            double[] bcVy = null;
            MEM.Alloc(bcAdress_WL.Length, ref bcVy, V);

            algebra = new AlgebraGauss((uint)mesh.CountKnots);
            SPhysics.PHYS.turbViscType = (ETurbViscType)listBoxAMu.SelectedIndex;
            ECalkDynamicSpeed typeEddyViscosity = (ECalkDynamicSpeed)ls_Type__U_star.SelectedIndex;
            bool velLocal = cbVelocity.Checked;
            double[] mVortex = null;
            double[] mPhi = null;
            switch (listBoxTasks.SelectedIndex)
            {
                // функции тока в проточном канале
                case 0:
                    {
                        IMWDistance wM = new MWDistanceTri(mesh);
                        CFETurbulVortexPhiTri task = new CFETurbulVortexPhiTri(wM, algebra, TypeTask.streamX1D);
                        task.CalkVortexPhi_StreamBCPhi(ref mPhi, ref mVortex, ref mVx, ref mVy,
                          ref meddyViscosity, bcAdressPhi, bcValuePhi, typeEddyViscosity);
                        SHOW.ShowCFD(mesh, null, mVx, mVy, meddyViscosity, null, null, null, mPhi, mVortex, null);
                    }
                break;
                case 1:
                    {
                        IMWDistance wM = new MWDistanceTri(mesh);
                        CFETurbulVortexPhiTri task = new CFETurbulVortexPhiTri(wM, algebra, TypeTask.streamX1D);
                        task.CalkVortexPhi_Mazo(ref mPhi, ref mVortex, ref mVx, ref mVy,
                          ref meddyViscosity, bcAdressPhi, bcValuePhi, bcAdress_WL, bcAdress_Bed, bcAdress_IN, bcAdress_OUT, dt);
                        SHOW.ShowCFD(mesh, null, mVx, mVy, meddyViscosity, null, null, null, mPhi, mVortex, null);
                    }
                    break;
                case 2:
                    {
                        double R_min = 0; int Ring = 0;
                        bool stoks = cbStocks.Checked;
                        IMWCrossSection wM = new MWCrossSectionTri(mesh, R_min, Ring, false, SСhannelForms.boxCrossSection);
                        CFETurbulVortexPhiTri task = new CFETurbulVortexPhiTri(wM, algebra, TypeTask.streamY1D);
                        
                        task.CalkVortexPhiCavern(ref mPhi, ref mVortex, ref mVx, ref mVy, ref meddyViscosity,
                            bcVy, bcAdressPhi, bcValuePhi, bcAdress_WL, bcAdress_Bed, bcAdress_IN, bcAdress_OUT, dt, velLocal, stoks);

                        SHOW.ShowCFD(mesh, null, mVx, mVy, meddyViscosity, null, null, null, mPhi, mVortex, null);
                    }
                    break;
                case 3:
                    {
                        //double R_min = 0; int Ring = 0;
                        //IMWCrossSection wM = new MWCrossSectionTri(mesh, R_min, Ring, false, SСhannelForms.boxCrossSection);
                        //CFETurbulVortexPhiTri task = new CFETurbulVortexPhiTri(wM, algebra, TypeTask.streamY1D);
                        
                        //task.CalkVortexPhiCavern_Mazo(ref mPhi, ref mVortex, ref mVx, ref mVy, ref meddyViscosity,
                        //    bcVy, bcAdressPhi, bcValuePhi, bcAdress_WL, bcAdress_Bed, bcAdress_IN, bcAdress_OUT, dt);

                        //SHOW.ShowCFD(mesh, null, mVx, mVy, meddyViscosity, null, null, null, mPhi, mVortex, null);
                    }
                    break;
                case 99:
                    {
                        //IMWDistance wM = new MWDistanceTri(mesh);
                        //CFETurbulVortexPhiTri task = new CFETurbulVortexPhiTri(wM, algebra, TypeTask.streamX1D);
                        //task.CalkVortexPhiCavern(ref mPhi, ref mVortex, ref mVx, ref mVy, ref meddyViscosity,
                        //    bcVy, bcAdressPhi, bcValuePhi, bcAdress_WL, bcAdress_Bed, bcAdress_IN, bcAdress_OUT, dt);
                        //SHOW.ShowCFD(mesh, null, mVx, mVy, meddyViscosity, null, null, null, mPhi, mVortex, null);
                    }
                    break;
            }
        }
    }
}
