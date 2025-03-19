//---------------------------------------------------------------------------
//                  - (C) Copyright 2024
//                        Потапов И.И.
//                         21.12.24
//---------------------------------------------------------------------------
// Тестовая задача Стокса/Навье-Стокса в переменных Phi,Vortex
// две степени свободы в узле, с "точными" граничными условиями для Vortex
//---------------------------------------------------------------------------
namespace TestSUPG
{
    using System;
    using System.Linq;
    using System.Windows.Forms;

    using CommonLib;
    using CommonLib.Mesh;
    using CommonLib.Physics;
    using CommonLib.Function;
    using CommonLib.EddyViscosity;

    using MeshLib;
    using MemLogLib;
    using RenderLib;
    using AlgebraLib;
    using GeometryLib;
    using FEMTasksLib;
    using EddyViscosityLib;
    using MeshLib.Wrappers;
    using MeshGeneratorsLib.StripGenerator;
    using FEMTasksLib.FEMTasks.VortexStream;
    using System.IO;


    public partial class FAmur : Form
    {
        double J = 0;
        double Time;

        
        IDigFunction WaterLevels = null;    
        IDigFunction riverQ = null;

        int[] indexsBC = null;
        /// <summary>
        /// Окружная скорость
        /// </summary>
        double[] Ux = null;
        /// <summary>
        /// Вихревая вязкость
        /// </summary>
        double[] eddyViscosity = null;
        double Vy_top=1;
        double Mut = 1;
        double H = 0;
        double BL = 0;
        double WL = 0;
        double diametrFE;
        double dt;
        double Qw;
        double Q;
        /// <summary>
        /// Потоковая скорость на свободной поверхности
        /// </summary>
        IDigFunction funU = null;
        /// <summary>
        /// Радиальная скорость на свободной поверхности
        /// </summary>
        IDigFunction funV = null;
        /// <summary>
        /// тип задачи 0 - плоская 1 - цилиндрическая
        /// </summary>
        int SigmaTask;
        /// <summary>
        /// радиус изгиба русла
        /// </summary>
        double RadiusMin;
        /// <summary>
        /// Итераций по нелинейности
        /// </summary>
        int NoLine;
        /// <summary>
        /// Внешнии итераций по нелинейности
        /// </summary>
        int NoLineMax;
        IMesh mesh = null;
        bool AxisOfSymmetry = false;
        /// <summary>
        /// Геометрич створа
        /// </summary>
        IDigFunction Geometry;
        /// <summary>
        /// Скорость на WL
        /// </summary>
        IDigFunction VelocityUx = null;
        /// <summary>
        /// Индекс турбуленной модели
        /// </summary>
        int IndexTurbModel;
        /// <summary>
        /// Тип алгебраической турбулентной модели
        /// </summary>
        ETurbViscType typeAlgebraTurbModel;


        const string Ext_RvY = ".rvy";
        const string Ext_Crf = ".crf";

        public FAmur()
        {
            InitializeComponent();
            lb_CrossNamber.SelectedIndex = 0;
            lb_MeshGen.SelectedIndex = 0;
            lb_Algebra.SelectedIndex = 1;
            listBoxAMu.SelectedIndex = 1;
            lb_Tasks.SelectedIndex = 2;
            lb_Vz.SelectedIndex = 0;
            lb_TurbModels.SelectedIndex = 6;
            lb_TurbModels.SelectedIndex = 5;

            string filter = "(*" + Ext_RvY + ")|*" + Ext_RvY + "| ";
            filter += "(*" + Ext_Crf + ")|*" + Ext_Crf + "| ";
            filter += " All files (*.*)|*.*";
            openFileDialog1.Filter = filter;
        }
        private void lb_Tasks_SelectedIndexChanged(object sender, EventArgs e)
        {
            Tasks();
        }
        void Tasks()
        {
            switch(lb_Tasks.SelectedIndex)
            {
                    case 1:
                        tb_WL.Text = "1";
                        tb_H.Text = "0.5";
                        lb_CrossNamber.SelectedIndex=3;
                        lb_MeshGen.SelectedIndex = 3;
                        tb_Vy_top.Text = "0.1";
                        tb_Mu.Text = "1";
                    break;
                    case 2:
                        tb_WL.Text = "1";
                        tb_H.Text = "0.5";
                        lb_CrossNamber.SelectedIndex = 3;
                        lb_MeshGen.SelectedIndex = 3;
                        tb_Vy_top.Text = "1";
                        tb_Mu.Text = "1";
                    break;
                    case 3:
                    tb_WL.Text = "0.5";
                    tb_H.Text = "0.25";
                    lb_CrossNamber.SelectedIndex = 3;
                    lb_MeshGen.SelectedIndex = 3;
                    tb_Vy_top.Text = "0.2";
                    tb_Mu.Text = "1";
                    break;
                case 4:
                    tb_WL.Text = "0.5";
                    tb_H.Text = "0.25";
                    lb_CrossNamber.SelectedIndex = 3;
                    lb_MeshGen.SelectedIndex = 3;
                    tb_Vy_top.Text = "2";
                    tb_Mu.Text = "1";
                    break;
                case 5:
                    tb_WL.Text = "1";
                    tb_H.Text = "0.5";
                    lb_CrossNamber.SelectedIndex = 3;
                    lb_MeshGen.SelectedIndex = 3;
                    tb_Vy_top.Text = "10";
                    tb_Mu.Text = "1";
                    break;
                case 6:
                    tb_WL.Text = "100";
                    tb_H.Text = "5";
                    lb_CrossNamber.SelectedIndex = 0;
                    lb_MeshGen.SelectedIndex = 0;
                    tb_Vy_top.Text = "1.0";
                    tb_Mu.Text = "1";
                    break;
                case 7:
                    tb_WL.Text = "100";
                    tb_H.Text = "5";
                    lb_CrossNamber.SelectedIndex = 4;
                    lb_MeshGen.SelectedIndex = 0;
                    tb_Vy_top.Text = "1.0";
                    tb_Mu.Text = "1";
                    break;
                case 8:
                    tb_WL.Text = "100";
                    tb_H.Text = "5";
                    lb_CrossNamber.SelectedIndex = 1;
                    lb_MeshGen.SelectedIndex = 0;
                    tb_Vy_top.Text = "1.0";
                    tb_Mu.Text = "1";
                    break;
                case 9:
                    tb_WL.Text = "100";
                    tb_H.Text = "5";
                    lb_CrossNamber.SelectedIndex = 5;
                    lb_MeshGen.SelectedIndex = 0;
                    tb_Vy_top.Text = "1.0";
                    tb_Mu.Text = "1";
                    break;
                case 0:
                default:
                    tb_WL.Text = "100";
                    tb_H.Text = "5.5";
                    lb_CrossNamber.SelectedIndex = 0;
                    lb_MeshGen.SelectedIndex = 0;
                    tb_Vy_top.Text = "0.1";
                    tb_Mu.Text = "1";
                    break;
            }
        }
        private void bt_CalkUx_Click(object sender, EventArgs e)
        {
            CreateMesh();
            PoissonTaskTri task = null;
            if (indexsBC == null)
                task = new PoissonTaskTri(Mut, Q, 0);
            else
                task = new PoissonTaskTri(Mut, Q, indexsBC);
            StartTask(task, 0);
        }

        private void btCalk_Click(object sender, EventArgs e)
        {
            if(SigmaTask == 0)
                CreateMesh();
            else
                bt_CalkUx_Click(sender, e);
            VortexStreamTri task = new VortexStreamTri(Vy_top, Mut, NoLine, 
                SigmaTask, RadiusMin, Ux);
            StartTask(task, 0);
        }
        private void btCalkNS_Click(object sender, EventArgs e)
        {
            CreateMesh();
            VortexStreamTri task = new VortexStreamTri(Vy_top, Mut, NoLineMax,
                SigmaTask, RadiusMin, Ux);
            StartTask(task, 1);
        }
        private void btCalkNNS_Click(object sender, EventArgs e)
        {
            CreateMesh();
            VortexStreamTri task = new NSVortexStreamTri(Vy_top, Mut, dt, Time, NoLine,
                SigmaTask, RadiusMin, Ux);
            StartTask(task, 2);
        }
        private void btCalkNS_NR_Click(object sender, EventArgs e)
        {
            CreateMesh();
            VortexStreamTri task = new VortexStreamTri(Vy_top, Mut, NoLine,
                SigmaTask, RadiusMin, Ux);
            StartTask(task, 3);
        }
        private void btCalkReS_Click(object sender, EventArgs e)
        {
            bt_CalkUx_Click(sender, e);
            VortexStreamTri task = new VortexStreamTri(Vy_top, eddyViscosity, 
                NoLineMax, SigmaTask, RadiusMin, Ux);
            StartTask(task, 4);
        }
        private void btCalkReN_Click(object sender, EventArgs e)
        {
            bt_CalkUx_Click(sender, e);
            NSVortexStreamTri task = new NSVortexStreamTri(Vy_top, eddyViscosity, 
                dt, Time, NoLine, SigmaTask, RadiusMin, Ux);
            StartTask(task, 5);
        }

        private void btCalkRiverCrossA_Click(object sender, EventArgs e)
        {
            CreateMesh();
            RiverCrossSteramTri task = new RiverCrossSteramTri(Vy_top, J, Mut,
                    null, dt, Time, NoLine, NoLineMax, typeAlgebraTurbModel, SigmaTask, RadiusMin);
             StartTask(task, 0);
        }
        private void btCalkRiverCrossKE_Click(object sender, EventArgs e)
        {
            CreateMesh();
            int idxVT = lb_TurbModels.SelectedIndex;
            RiverCrossSteramDiffTri task = new RiverCrossSteramDiffTri(Vy_top, J, Mut,
            null, dt, Time, NoLine, NoLineMax, idxVT, typeAlgebraTurbModel, SigmaTask, RadiusMin);
            StartTask(task, 0);
        }

        private void btCalkRiverCrossSA_Click(object sender, EventArgs e)
        {
            lb_TurbModels.SelectedIndex = 5;
            CreateMesh();
            RiverCrossSteramDiffTri task = new RiverCrossSteramDiffTri(Vy_top, J, Mut,
                    null, dt, Time, NoLine, NoLineMax, IndexTurbModel, 
                    typeAlgebraTurbModel, SigmaTask, RadiusMin);
            StartTask(task, 0);
        }
        private void btCalkRiverCrossSA_n_Click(object sender, EventArgs e)
        {
            lb_TurbModels.SelectedIndex = 6;
            CreateMesh();
            RiverCrossSteramDiffTri task = new RiverCrossSteramDiffTri(Vy_top, J, Mut, null, 
                dt, Time, NoLine, NoLineMax, IndexTurbModel, 
                typeAlgebraTurbModel, SigmaTask, RadiusMin);
            StartTask(task, 1);
        }
        public IAlgebra SetAlgebra(uint CountU, int cs)
        {
            IAlgebra algebra = null;
            switch (lb_Algebra.SelectedIndex)
            {
                case 0:
                    algebra = new AlgebraGauss(CountU);
                    break;
                case 1:
                    {
                        uint NH = mesh.GetWidthMatrix();
                        algebra = new AlgebraLUTape(CountU, (int)(NH) * cs, (int)(NH) * cs);
                    }
                    break;
                case 2:
                    algebra = new SparseAlgebraBeCG(CountU, false);
                    break;
            }
            return algebra;
        }


        public IDigFunction CalkVelosityUy()
        {
            IDigFunction VelosityUy = null;
            // Радиальная/боковая скорости на WL
            switch (lb_Vz.SelectedIndex)
            {
                case 0:
                    VelosityUy = null;
                    break;
                case 1:
                    {
                        double[] X = mesh.GetCoords(0);
                        double[] ox = { X.Min(), X.Max() };
                        double[] oy = { Vy_top, Vy_top };
                        VelosityUy = new DigFunction(ox, oy, "Боковая скорость");
                    }
                    break;
                case 2:
                    {
                        double[] X = mesh.GetCoords(0);
                        double[] ox = { X.Min(), X.Max() };
                        double[] oy = { -Vy_top, -Vy_top };
                        VelosityUy = new DigFunction(ox, oy, "Боковая скорость");
                    }
                    break;
                case 3:
                    {
                        double[] X = mesh.GetCoords(0);
                        double wX = X.Min();
                        double eX = X.Max();
                        double pX = 0.5 * (eX + wX);
                        double dx = 0.005 * (eX - wX);
                        double[] ox = { wX, pX - dx, pX + dx, eX };
                        double[] oy = { -Vy_top, -Vy_top, Vy_top, Vy_top };
                        VelosityUy = new DigFunction(ox, oy, "Боковая скорость");
                    }
                    break;
                case 4:
                    {
                        double[] X = mesh.GetCoords(0);
                        double wX = X.Min();
                        double eX = X.Max();
                        double L = eX - wX;
                        int N = 10;
                        double dx = L / (N - 1);
                        double[] ox = null;
                        double[] oy = null;
                        MEM.Alloc(N, ref ox);
                        MEM.Alloc(N, ref oy);
                        for (int i = 0; i < N; i++)
                        {
                            ox[i] = i * dx;
                            oy[i] = -4 * Vy_top * ox[i] * (ox[i] - L) / (L * L);
                        }
                        VelosityUy = new DigFunction(ox, oy, "Боковая скорость");
                    }
                    break;
            }
            return VelosityUy;
        }

        public void StartTask(PoissonTaskTri task, int id)
        {
            try
            {
                int cs = 1;
                uint CountU = (uint)(cs * mesh.CountKnots);
                IAlgebra algebra = SetAlgebra(CountU, cs);

                task.SetTask(mesh, algebra);

                Diagnostician diagnostic = null;
                if (id == 0) diagnostic = new Diagnostician(algebra.Name, task.Test);
                diagnostic.Test();

                MEM.Alloc(mesh.CountKnots, ref eddyViscosity);

                IMWCrossSection wMesh = new MWCrossSectionTri(mesh, 1, 0, false);
                // Определение вязкости
                SPhysics.PHYS.turbViscType = (ETurbViscType)listBoxAMu.SelectedIndex;
                SPhysics.PHYS.calkTurbVisc(ref eddyViscosity, TypeTask.streamY1D, (IMWRiver)wMesh, ECalkDynamicSpeed.u_start_U, task.U, J);


                SavePoint sp = new SavePoint("Тест задачи: вихрь - функция тока в cs = 2");
                sp.SetSavePoint(0, mesh);

                double[] x = mesh.GetCoords(0);
                double[] y = mesh.GetCoords(1);
                Ux = task.U;
                sp.Add("U", task.U);
                sp.Add("Вихревая вязкость", eddyViscosity);
                sp.Add("Координата Х", x);
                sp.Add("Координата Y", y);

                Form form = new ViForm(sp);
                form.Text = sp.Name;
                form.Show();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void StartTask(VortexStreamTri task, int id)
        {
            try
            {
                task.VelosityUy = CalkVelosityUy();

                int cs = 2;
                uint CountU = (uint)(cs * mesh.CountKnots);
                IAlgebra algebra = SetAlgebra(CountU, cs);
                task.SetTask(mesh, algebra);

                double[] phi = null;
                double[] vortex = null;
                MEM.Alloc(mesh.CountKnots, ref phi);
                MEM.Alloc(mesh.CountKnots, ref vortex);

                //double[] resulat = null;
                //task.SolveTask(ref resulat);
                Diagnostician diagnostic = null;
                if (id == 0) diagnostic = new Diagnostician(algebra.Name, task.Solve);
                else
                if (id == 1) diagnostic = new Diagnostician(algebra.Name, task.SolveTaskNS);
                else 
                if (id == 2) diagnostic = new Diagnostician(algebra.Name, task.Solve);
                else
                if (id == 3) diagnostic = new Diagnostician(algebra.Name, task.SolveTaskNR);
                else
                if (id == 4) diagnostic = new Diagnostician(algebra.Name, task.SolveTaskRe);
                else
                if (id == 5) diagnostic = new Diagnostician(algebra.Name, task.Solve);

                diagnostic.Test();


                SavePoint sp = new SavePoint("Тест задачи: вихрь - функция тока в cs = 2");
                sp.SetSavePoint(0, mesh);

                double[] x = mesh.GetCoords(0);
                double[] y = mesh.GetCoords(1);

                sp.Add("Phi", task.Phi);
                sp.Add("Vortex", task.Vortex);
                sp.Add("Вихревая вязкость", task.eddyViscosity);
                sp.Add("Vy", task.Vy);
                sp.Add("Vz", task.Vz);
                sp.Add("V", task.Vy, task.Vz);

                sp.Add("Координата Х", x);
                sp.Add("Координата Y", y);

                Form form = new ViForm(sp);
                form.Text = sp.Name;
                form.Show();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void StartTask(ARiverCrossSteramTri task, int id)
        {
            try
            {
                task.VelosityUy = CalkVelosityUy();
                int cs = 1;
                uint CountU = (uint)(cs * mesh.CountKnots);
                IAlgebra algebra = SetAlgebra(CountU, cs);
                cs = 2;
                CountU = (uint)(cs * mesh.CountKnots);
                IAlgebra algebra2 = SetAlgebra(CountU, cs);
                task.SetTask(mesh, algebra, algebra2);
                double[] phi = null;
                double[] vortex = null;
                MEM.Alloc(mesh.CountKnots, ref phi);
                MEM.Alloc(mesh.CountKnots, ref vortex);
              
                Diagnostician diagnostic = null;
                if (id == 0) diagnostic = new Diagnostician(algebra.Name, task.Solve);
                else
                if (id == 1) diagnostic = new Diagnostician(algebra.Name, task.SolveTime);

                diagnostic.Test();

                SavePoint sp = new SavePoint("Тест, вихревая вязкость SA, кавкрна (вихрь - функция тока в cs = 2" +
                    "Время расчета = " + tb_Time.Text + " c");
                sp.SetSavePoint(0, mesh);

                double[] x = mesh.GetCoords(0);
                double[] y = mesh.GetCoords(1);

                sp.Add("Ux", task.taskUx.Ux);
                sp.Add("Phi", task.taskPV.Phi);
                sp.Add("Vortex", task.taskPV.Vortex);
                sp.Add("Вихревая вязкость", task.eddyViscosity);
                sp.Add("Vy", task.taskPV.Vy);
                sp.Add("Vz", task.taskPV.Vz);
                sp.Add("V", task.taskPV.Vy, task.taskPV.Vz);

                var taskKE = task as RiverCrossSteramDiffTri; 
                if (taskKE != null)
                {
                    var vm = taskKE.taskViscosity as AEddyViscosityKETri;
                    if (vm != null)
                    {
                        sp.Add("Кин. энергия турб.", vm.Ken);
                        sp.Add("Диссипация турб. эн.", vm.Eps);
                    }
                    var vm1 = taskKE.taskViscosity as AEddyViscosity_SA_Tri;
                    if (vm1 != null)
                    {
                        sp.Add("турб. вязкость прив.", vm1.Mut);
                        sp.Add("xi.", vm1.xi);
                        sp.Add("fv1.", vm1.fv1);
                        sp.Add("fv2.", vm1.fv2);
                        sp.Add("ft2.", vm1.ft2);
                        sp.Add("дистанция.", vm1.distance);
                    }
                }
                sp.Add("Координата Х", x);
                sp.Add("Координата Y", y);

                Form form = new ViForm(sp);
                form.Text = sp.Name;
                form.Show();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void lb_CrossNamber_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (lb_CrossNamber.SelectedIndex)
            {
                case 0:
                case 1:
                case 2:
                    lb_WL.Visible=true;
                    lb_BL.Visible=true;
                    lb_H.Visible=true;
                    tb_WL.Visible=true;
                    tb_BL.Visible=true;
                    tb_H.Visible=true;
                    break;
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                    lb_WL.Visible = true;
                    lb_H.Visible = true;
                    lb_BL.Visible = false;
                    tb_WL.Visible = true;
                    tb_H.Visible = true;
                    tb_BL.Visible = false;
                    break;
                case 8:
                    lb_WL.Visible = false;
                    lb_H.Visible = false;
                    lb_BL.Visible = false;
                    tb_WL.Visible = false;
                    tb_H.Visible = false;
                    tb_BL.Visible = false;
                    break;
            }
        }

        #region Тесты сеточных генераторов
        private void btCreateMesh_Click(object sender, EventArgs e)
        {
            CreateMesh();
            ShowMesh();
        }
        void CreateMesh()
        {
            try
            {
                // общие параметры задач
                dt = double.Parse(tb_dt.Text, MEM.formatter);
                J = double.Parse(tb_J.Text, MEM.formatter);
                Qw = double.Parse(tb_Q.Text, MEM.formatter);
                Q = SPhysics.GRAV * SPhysics.rho_w * J;
                SigmaTask = rb_SigmaTask.Checked == true ? 0 : 1;
                RadiusMin = double.Parse(tb_R.Text, MEM.formatter);
                Vy_top = double.Parse(tb_Vy_top.Text, MEM.formatter);
                dt = double.Parse(tb_dt.Text, MEM.formatter);
                Time = double.Parse(tb_Time.Text, MEM.formatter);
                NoLine = int.Parse(tb_NoLine.Text, MEM.formatter);
                NoLineMax = int.Parse(tb_NoLineMax.Text, MEM.formatter);
                IndexTurbModel = lb_TurbModels.SelectedIndex;
                typeAlgebraTurbModel = (ETurbViscType)listBoxAMu.SelectedIndex;

                WL = double.Parse(tb_WL.Text, MEM.formatter);
                BL = double.Parse(tb_BL.Text, MEM.formatter);
                H = double.Parse(tb_H.Text, MEM.formatter);
                Mut = double.Parse(tb_Mu.Text, MEM.formatter);
                
                AxisOfSymmetry = cb_AxisOfSymmetry.Checked;
                Console.WriteLine("Re = " + (1000 * Vy_top * H / SPhysics.mu).ToString("F4"));
                double WetBed = 0;
                double WaterLevel = H;
                double PFE = double.Parse(textBoxDiam.Text, MEM.formatter);
                diametrFE = Math.Min(WL, H) / 100 * PFE;

                double Re = SPhysics.rho_w * Vy_top * 2 * H/SPhysics.mu;
                tb_Re.Text = Re.ToString();
                double Ret = SPhysics.rho_w * Vy_top * 2 * H / Mut;
                tb_Ret.Text = Ret.ToString();

                double dtC = 0.5 * diametrFE / Vy_top;
                tb_dtC.Text = dtC.ToString("F6");

                int Ny = (int)(WL / diametrFE) + 1;
                double[] xx = null;
                double[] yy = null;
                MEM.Alloc(Ny, ref xx);
                MEM.Alloc(Ny, ref yy);
                IStripMeshGenerator sg = null;
                switch (lb_MeshGen.SelectedIndex)
                {
                    case 0:
                        sg = new HStripMeshGeneratorTri(AxisOfSymmetry);
                        break;
                    case 1:
                        sg = new HStripMeshGenerator(AxisOfSymmetry);
                        break;
                    case 2:
                        sg = new CrossStripMeshGeneratorTri(AxisOfSymmetry);
                        break;
                    case 3:
                        sg = new CrossStripMeshGenerator(AxisOfSymmetry, TypeMesh.Triangle);
                        break;
                }
                switch (lb_CrossNamber.SelectedIndex)
                {
                    case 0: // трапеция
                        {
                            indexsBC = null;
                            double dL = (WL - BL) / 2;
                            double[] ox = { -2*dL, 0, dL, dL + BL, WL, WL + 2*dL };
                            double dx = (ox[ox.Length - 1] - ox[0]) / (Ny - 1);
                            double[] oy = {  2*H, 0,-H,       -H, 0,   2*H };
                            IDigFunction fun = new DigFunction(ox, oy, "Дно");
                            for (int i = 0; i < xx.Length; i++)
                            {
                                double x = ox[0] + dx * i;
                                xx[i] = x;
                                yy[i] = fun.FunctionValue(x);
                            }
                        }
                        break;
                    case 1: // левая трапеция
                        {
                            double dL = WL - BL;
                            double[] ox = { -2* dL, 0, dL, WL };
                            double dx = (ox[ox.Length - 1] - ox[0]) / (Ny - 1);
                            double[] oy = { 2*H, 0, -H, -H };
                            IDigFunction fun = new DigFunction(ox, oy, "Дно");
                            for (int i = 0; i < xx.Length; i++)
                            {
                                double x = ox[0] + dx * i;
                                xx[i] = x;
                                yy[i] = fun.FunctionValue(x);
                            }
                            if (cb_AxisOfSymmetry.Checked == false)
                                indexsBC = new int[2] { 0, 1 };
                            else
                                indexsBC = null;
                        }
                        break;
                    case 2: // правая трапеция
                        {
                            double dL = WL - BL;
                            double[] ox = { 0, BL, WL, WL + 2 *dL };
                            double dx = (ox[ox.Length - 1] - ox[0]) / (Ny - 1);
                            double[] oy = { -H, -H, 0, 2 * H };
                            IDigFunction fun = new DigFunction(ox, oy, "Дно");
                            for (int i = 0; i < xx.Length; i++)
                            {
                                double x = ox[0] + dx * i;
                                xx[i] = x;
                                yy[i] = fun.FunctionValue(x);
                            }
                            if (cb_AxisOfSymmetry.Checked == false)
                                indexsBC = new int[2] { 0, 3 };
                            else
                                indexsBC = null;
                        }
                        break;
                    case 3: // Короб
                        {
                            double[] ox = { 0, WL };
                            double dx = (ox[ox.Length - 1] - ox[0]) / (Ny - 1);
                            double[] oy = { -H, -H };
                            IDigFunction fun = new DigFunction(ox, oy, "Дно");
                            for (int i = 0; i < xx.Length; i++)
                            {
                                double x = ox[0] + dx * i;
                                xx[i] = x;
                                yy[i] = fun.FunctionValue(x);
                            }
                            if (cb_AxisOfSymmetry.Checked == false)
                                indexsBC = new int[3] { 0, 1, 3 };
                            else
                                indexsBC = new int[2] { 0, 3 };
                        }
                        break;
                    case 4: // Парабола
                        {
                            indexsBC = null;
                            double dx = 2 * WL / (Ny - 1);
                            double a = WL / 2;
                            double b = a * a / H;
                            for (int i = 0; i < xx.Length; i++)
                            {
                                double x = -WL + dx * i;
                                xx[i] = x;
                                yy[i] = (x - a) * (x + a) / b;
                            }
                        }
                        break;

                    case 5: // Левая парабола
                        {
                            double dx = 2 * WL / (Ny - 1);
                            double a = WL;
                            double b = a * a / H;
                            for (int i = 0; i < xx.Length; i++)
                            {
                                double x = -2*WL + dx * i;
                                xx[i] = x;
                                yy[i] = (x - a) * (x + a) / b;
                            }
                            if (cb_AxisOfSymmetry.Checked == false)
                                indexsBC = new int[2] { 0, 1 };
                            else
                                indexsBC = null;
                        }
                        break;
                    case 6: // Правая парабола
                        {
                            double dx = 2 * WL / (Ny - 1);
                            double a = WL;
                            double b = a * a / H;
                            for (int i = 0; i < xx.Length; i++)
                            {
                                double x = dx * (i - 1);
                                xx[i] = x;
                                yy[i] = (x - a) * (x + a) / b;
                            }
                            if (cb_AxisOfSymmetry.Checked == false)
                                indexsBC = new int[2] { 0, 3 };
                            else
                                indexsBC = null;
                        }
                        break;
                    case 7:
                        {
                            Geometry = new FunctionСhannel(Ny, H, WL);
                            Geometry.GetFunctionData(ref xx, ref yy, Ny);
                        }
                        break;
                    case 8:
                        {
                            if (Geometry == null)
                                rb_SigmaTask_CheckedChanged(null, null);
                            WaterLevel = 0;// WaterLevels.FunctionValue(0);
                            double xa = Geometry.Xmin;
                            double dx = Geometry.Length / (Ny - 1);
                            for (int i = 0; i < xx.Length; i++)
                            {
                                double x = xa + dx * i;
                                xx[i] = x;
                                yy[i] = - (Geometry.FunctionValue(x) + 1);
                            }
                        }
                        break;
                }
                mesh = sg.CreateMesh(ref WetBed, WaterLevel, xx, yy, Ny);
            }
            catch (Exception ex) 
            { 
                Console.WriteLine(ex.Message);
            }
        }
        protected void ShowMesh()
        {
            if (mesh != null && checkBoxView.Checked == true)
            {
                SavePoint data = new SavePoint();
                data.SetSavePoint(0, mesh);
                double[] xx = mesh.GetCoords(0);
                double[] yy = mesh.GetCoords(1);
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


        #endregion
        private void rb_SigmaTask_CheckedChanged(object sender, EventArgs e)
        {
            SigmaTask = rb_SigmaTask.Checked == true ? 0 : 1;
        }

        private void bt_Open_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                using (StreamReader file = new StreamReader(openFileDialog1.FileName))
                {
                    // геометрия дна
                    Geometry = new DigFunction();
                    // свободная поверхность
                    WaterLevels = new DigFunction();
                    // расход потока
                    funV = new DigFunction();
                    funU = new DigFunction();
                    riverQ = new DigFunction();
                    Console.WriteLine(" дата " + file.ReadLine());
                    Console.WriteLine(" место " + file.ReadLine());
                    // геометрия дна
                    Geometry.Load(file);
                    // свободная поверхность
                    WaterLevels.Load(file);
                    // расход потока
                    riverQ.Load(file);
                    // Нормальная скорость на WL
                    funU = new DigFunction();
                    // Радиальная скорость на WL
                    funV = new DigFunction();
                    file.Close();
                }
            }
        }
    }
}
