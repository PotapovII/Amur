
namespace RiverDB.ConvertorOut
{
    using System;
    using System.Linq;
    using System.Windows.Forms;
    using System.Collections.Generic;

    using MemLogLib;
    using ConnectLib;
    using GeometryLib;
    using TriangleNet;
    using MeshAdapterLib;
    using RiverDB.Report;
    using RiverDB.FormsDB;

    using CommonLib;
    using CommonLib.IO;
    using CommonLib.Mesh;
    using CommonLib.Geometry;

    using GeometryLib.Locators;
    using NPRiverLib.APRiver2XYD.River2DSW;
    using TriangleNet.Geometry;
    using MeshLib;
    using RenderLib;

    public partial class ExportMRF : Form
    {
        List<SegmentInfo> segInfo;
        MeshNet meshRiver;
        double[] x;
        double[] y;
        IFEMesh bmesh = null;
        /// <summary>
        /// Атрибуты узлов
        /// </summary>
        double[][] values = null;
        List<int> selected = new List<int>();
        Catalogue book = new Catalogue("place", true);
        /// <summary>
        /// Уровнень реки по гидропосту
        /// </summary>
        double WLRiver = 0;
        /// <summary>
        /// Расход
        /// </summary>
        double QRiver = 0;
        /// <summary>
        /// Дата изменения
        /// </summary>
        string Data;
        
        int placeID = 1;
        public ExportMRF(MeshNet meshRiver, List<SegmentInfo> segInfo, string  Data)
        {
            InitializeComponent();
            this.Data = Data;
            tb_Data.Text = Data;
            this.segInfo = segInfo;
            this.meshRiver = meshRiver;
            string ext = "*.mrf";
            string filter = "(*" + ext + ")|*" + ext + "| ";
            filter += " All files (*.*)|*.*";
            saveFileDialog1.Filter = filter;
            Init();
            listBox1.SelectedIndex = 0;
            ChangGP_WL();
        }
        public void ChangGP_WL()
        {
            string Name = "", X = "", Y = "";
            double Zerro = 0, height = 0;
            ConnectDB.PlaceInfo(placeID, ref Name, ref X, ref Y, ref Zerro, ref height);
            tb2.Text = placeID.ToString();
            tb21.Text = Name;
            tb_latitudeArea.Text = X;
            tb_longitude.Text = Y;
            tb_BaltikLvl.Text = Zerro.ToString("F4");
            WLRiver = ConnectDB.WaterLevelData(Data, placeID);
            QRiver = riverFlow(WLRiver);
            tb_RF.Text = QRiver.ToString("F1");
            tb_RiverLvl.Text = (Zerro + WLRiver / 100).ToString("F2");
        }
        private void bt1_Click(object sender, EventArgs e)
        {
            book.ShowDialog();
            tb2.Text = book.RowID;
            tb21.Text = book.RowName;
            placeID = int.Parse(book.RowID.Trim());
            ChangGP_WL();
            if (tb2.Text.Trim() == "1")
                pnRF.Visible = true;
            else
                pnRF.Visible = false;
        }
        private double riverFlow(double H)
        {
            IRiverFlow rf = new RiverFlowZerro();
            if (tb2.Text.Trim() == "1")
            {
                if (rbAmur.Checked == true)
                    rf = new RiverFlowAmur();
                else
                {
                    if (rbPemza.Checked == true)
                        rf = new RiverFlowPemza();
                    else
                        rf = new RiverFlowMad();
                }
            }
            if (tb2.Text.Trim() == "3")
                rf = new RiverFlowKomsomolskNaAmure();
            return rf.GetRiverFlow(H);
        }
        private void rbAmur_CheckedChanged(object sender, EventArgs e)
        {
            ChangGP_WL();
        }
        protected void Init()
        {
            MeshAdapter.ConvertFrontRenumberationAndCutting(ref bmesh, ref values, meshRiver, Direction.toRight);
            x = bmesh.GetCoords(0);
            y = bmesh.GetCoords(1);
            string[] Names = { " Береговой сегмент реки", 
                                "Приток потока в расчетную область",
                                "Исток потока из расчетной области" };
            string name="";
            for(int s = 0; s < segInfo.Count; s++)
            {
                var seg = segInfo[s];
                name = Names[seg.type - 1];
                listBox1.Items.Add(name);
                selected.Add(0);
            }
        }
        /// <summary>
        /// Определение граничных КЭ по сегменту
        /// </summary>
        public void GetBConditionID(ref BoundElementRiver[] BoundElems)
        {
            for (int be = 0; be < BoundElems.Length; be++)
                BoundElems[be].boundCondType = 0;

            LineLocator Locator = new LineLocator();
            foreach (SegmentInfo s in segInfo)
            {
                List<BoundElementRiver> set = new List<BoundElementRiver>();
                Locator.Set(s.pA, s.pB);
                for (int be = 0; be < BoundElems.Length; be++)
                {
                    uint Vertex1 = BoundElems[be].Vertex1;
                    uint Vertex2 = BoundElems[be].Vertex2;
                    IHPoint p1 = new HPoint(x[Vertex1], y[Vertex1]);
                    IHPoint p2 = new HPoint(x[Vertex2], y[Vertex2]);
                    bool flag1 = Locator.IsLiesOn(p1);
                    bool flag2 = Locator.IsLiesOn(p2);
                    if ( flag1 == true && flag2 == true)
                    {
                        BoundElems[be].segmentID = s.ID;
                        BoundElems[be].boundCondType = s.boundCondType();
                        BoundElems[be].Qn = s.Qn;
                        BoundElems[be].Qt = s.Qt;
                        BoundElems[be].Eta = s.Eta;
                        set.Add(BoundElems[be]);
                    }
                }
                BoundElementRiver[] bElems = set.ToArray();
                if (bElems[0].Vertex2 == bElems[1].Vertex1)
                {
                    s.startID = (int)bElems[0].Vertex1;
                    s.endID = (int)bElems[bElems.Length-1].Vertex2;
                    s.BEstartID = (int)bElems[0].ID;
                    s.BEendID = (int)bElems[bElems.Length - 1].ID;
                }
                else
                {
                    s.startID = (int)bElems[bElems.Length- 1].Vertex1;
                    s.endID = (int)bElems[0].Vertex2;
                    s.BEstartID = (int)bElems[bElems.Length - 1].ID;
                    s.BEendID = (int)bElems[0].ID;
                }
            }
        }


        /// <summary>
        /// Определение индексов границ для граничных КЭ по сегменту
        /// </summary>
        //public void GetBConditionID_OLD(BoundElementRiver[] BoundElems)
        //{
        //    foreach (SegmentInfo s in segInfo)
        //    {
        //        for (int i = 0; i < BoundElems.Length; i++)
        //        {
        //            uint Vertex1 = BoundElems[i].Vertex1;
        //            HPoint p = new HPoint(x[Vertex1], y[Vertex1]);
        //            HPoint ps = s.pA;

        //            if (HPoint.Equals(p, ps))
        //            {
        //                s.startID = (int)Vertex1;
        //                s.BEstartID = i;
        //                break;
        //            }
        //        }
        //        for (int i = 0; i < BoundElems.Length; i++)
        //        {
        //            uint Vertex2 = BoundElems[i].Vertex2;
        //            HPoint p = new HPoint(x[Vertex2], y[Vertex2]);
        //            HPoint ps = s.pB;
        //            if (HPoint.Equals(p, ps))
        //            {
        //                s.endID = (int)Vertex2;
        //                s.BEendID = i;
        //                break;
        //            }
        //        }
        //        for (int be = s.BEstartID; be <= s.BEendID; be++)
        //        {
        //            BoundElems[be].segmentID = s.ID;
        //            BoundElems[be].boundCondType = s.boundCondType();
        //            BoundElems[be].Qn = s.Qn;
        //            BoundElems[be].Qt = s.Qt;
        //            BoundElems[be].Eta = s.Eta;
        //        }
        //    }
        //}

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        public ParamsRiver2XYD GetParams()
        {
            ParamsRiver2XYD Params = new ParamsRiver2XYD();
            try
            {
                int i;
                Params.dtTrend = double.Parse(tb_Version.Text, MEM.formatter);
                Params.time = double.Parse(tb_time.Text, MEM.formatter);
                Params.dtime = double.Parse(tb_dtime.Text, MEM.formatter);
                Params.theta = double.Parse(tb_theta.Text, MEM.formatter);
                Params.UpWindCoeff = double.Parse(tb_UpWindCoeff.Text, MEM.formatter);
                Params.turbulentVisCoeff = double.Parse(tb_turbulentVisCoeff.Text, MEM.formatter);
                Params.latitudeArea = double.Parse(tb_latitudeArea.Text, MEM.formatter);
                // русло
                Params.droundWaterCoeff = double.Parse(tb_droundWaterCoeff.Text, MEM.formatter);
                Params.H_minGroundWater = double.Parse(tb_minGroundWater.Text, MEM.formatter);
                Params.filtrСoeff = double.Parse(tb_filtrСoeff.Text, MEM.formatter);
            }
            catch (Exception ex)
            {
                Logger.Instance.Info(ex.Message);
            }
            return Params;
        }
        public TriRiverMesh ConvertIFEMeshToRiver2DMesh(IFEMesh bmesh)
        {
            TriRiverMesh mesh = new TriRiverMesh();
            try
            {
                int i;
                // размерность
                int Count = bmesh.CountKnots;
                int CountElements = bmesh.CountElements;
                int CountBoundElements = bmesh.CountBoundElements;
                int CountBoundSegments = segInfo.Count;
                // выделение памяти под узловые поля
                MEM.Alloc(Count, ref mesh.nodes);
                IFElement[] elems = bmesh.AreaElems;
                double BaltikLvl = double.Parse(tb_BaltikLvl.Text.Trim(), MEM.formatter);
                // чтение узловых полей
                Vertex[] point =  meshRiver.Vertices.ToArray();
                for (i = 0; i < x.Length; i++)
                {
                    RiverNode node = new RiverNode();
                    node.n = i;
                    node.i = i;
                    node.X = x[i];
                    node.Y = y[i];
                    node.zeta = BaltikLvl - values[AtrCK.idx_H][i];
                    if(cb_Ise.Checked == true)
                        node.h_ise = double.Parse(tb_Ise.Text.Trim(), MEM.formatter);
                    else
                        node.h_ise = values[AtrCK.idx_Ice][i]; 
                    if (cb_Ks.Checked == true)
                        node.ks = double.Parse(tb_Ks.Text.Trim(), MEM.formatter);
                    else
                        node.ks = values[AtrCK.idx_ks][i];
                    node.h = values[AtrCK.idx_H][i];
                    node.qx = 0;
                    node.qy = 0;
                    node.fxc = 0; // узел в области
                    mesh.nodes[i] = node;
                }
                // выделение памяти под КЭ и поля
                MEM.Alloc(CountElements, ref mesh.AreaElems);
                // чтение КЭ полей
                for (i = 0; i < CountElements; i++)
                {
                    IFElement el = bmesh.AreaElems[i];
                    TriElementRiver elem = new TriElementRiver(i,
                        (uint)el.Nods[0].ID, (uint)el.Nods[1].ID, (uint)el.Nods[2].ID);
                    mesh.AreaElems[i] = elem;
                }
                // выделение памяти под граничные КЭ и поля
                MEM.Alloc(CountBoundElements, ref mesh.BoundElems);
                for (i = 0; i < CountBoundElements; i++)
                {
                    IFElement el = bmesh.BoundElems[i];
                    BoundElementRiver BoundElems = new BoundElementRiver();
                    BoundElems.ID = i;
                    BoundElems.boundCondType = 0;
                    BoundElems.Vertex1 = (uint)el.Nods[0].ID;
                    BoundElems.Vertex2 = (uint)el.Nods[1].ID;
                    BoundElems.Eta = 0;
                    BoundElems.Qn = 0;
                    BoundElems.Qt = 0;
                    mesh.BoundElems[i] = BoundElems;
                }
                // Сортировка ГКЭ для получения связности
                BoundElementRiver.Sort(ref mesh.BoundElems, Count);
                // Привязка
                //GetBConditionID(ref mesh.BoundElems);
                /// <summary>
                /// Определение граничных КЭ по сегменту
                /// </summary>
                //public void GetBConditionID(ref BoundElementRiver[] BoundElems)
                {
                    for (int be = 0; be < mesh.BoundElems.Length; be++)
                        mesh.BoundElems[be].boundCondType = 0;

                    LineLocator Locator = new LineLocator();
                    foreach (SegmentInfo s in segInfo)
                    {
                        List<BoundElementRiver> set = new List<BoundElementRiver>();
                        Locator.Set(s.pA, s.pB);
                        for (int be = 0; be < mesh.BoundElems.Length; be++)
                        {
                            uint Vertex1 = mesh.BoundElems[be].Vertex1;
                            uint Vertex2 = mesh.BoundElems[be].Vertex2;
                            IHPoint p1 = new HPoint(x[Vertex1], y[Vertex1]);
                            IHPoint p2 = new HPoint(x[Vertex2], y[Vertex2]);
                            bool flag1 = Locator.IsLiesOn(p1);
                            bool flag2 = Locator.IsLiesOn(p2);
                            if (flag1 == true && flag2 == true)
                            {
                                mesh.BoundElems[be].segmentID = s.ID;
                                mesh.BoundElems[be].boundCondType = s.boundCondType();
                                mesh.BoundElems[be].Qn = s.Qn;
                                mesh.BoundElems[be].Qt = s.Qt;
                                mesh.BoundElems[be].Eta = s.Eta;
                                set.Add(mesh.BoundElems[be]);
                            }
                        }
                        BoundElementRiver[] bElems = set.ToArray();
                        if (bElems[0].Vertex2 == bElems[1].Vertex1)
                        {
                            s.startID = (int)bElems[0].Vertex1;
                            s.endID = (int)bElems[bElems.Length - 1].Vertex2;
                            s.BEstartID = (int)bElems[0].ID;
                            s.BEendID = (int)bElems[bElems.Length - 1].ID;
                        }
                        else
                        {
                            s.startID = (int)bElems[bElems.Length - 1].Vertex1;
                            s.endID = (int)bElems[0].Vertex2;
                            s.BEstartID = (int)bElems[bElems.Length - 1].ID;
                            s.BEendID = (int)bElems[0].ID;
                        }
                    }
                }

                // Привязка ГЭ к КЭ
                // выделение памяти под граничные сегменты
                MEM.Alloc(CountBoundSegments, ref mesh.boundSegment);
                for (i = 0; i < CountBoundSegments; i++)
                {
                    SegmentInfo si = segInfo[i];
                    BoundSegmentRiver segment = new BoundSegmentRiver();
                    segment.ID = i;
                    segment.startnode = si.startID;
                    segment.endnode = si.endID;
                    segment.boundCondType = si.boundCondType();
                    segment.Eta = si.Eta;
                    segment.Qn = si.Qn;
                    mesh.boundSegment[i] = segment;
                }
                // Определение граничных узлов 
                for (i = 0; i < mesh.BoundElems.Length; i++)
                {
                    uint Vertex1 = mesh.BoundElems[i].Vertex1;
                    // скользящий узел
                    mesh.nodes[Vertex1].fxc = FixedFlag.sliding;
                    IHPoint pA = new HPoint(x[Vertex1], y[Vertex1]);
                    for (int j = 0; j < segInfo.Count; j++)
                    {
                        if (MEM.Equals(segInfo[j].pA, pA) == true)
                        {
                            // фиксированный узел - вершина полигона
                            mesh.nodes[Vertex1].fxc = FixedFlag.fixednode;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Info(ex.Message);
            }
            return mesh;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (selected.Sum() != selected.Count)
            {
                string str = "";
                for (int i = 0; i < selected.Count; i++)
                {
                    if (selected[i] == 0)
                        str += i.ToString() + " ";
                }
                MessageBox.Show("Не определены ГУ задачи для сегментов: " + str, "Ошибка");
                return;
            }
            try
            {
                // Конвертировать сетку IFEMesh Получить речную сетку в TriRiverMesh
                TriRiverMesh riverMesh = ConvertIFEMeshToRiver2DMesh(bmesh);
                // Получить параметры задачи
                ParamsRiver2XYD Params = GetParams();
                // Создать задачу
                TriRiverSWE2XYD river2D = new TriRiverSWE2XYD(Params);
                //river2D.Set(riverMesh);
                //river2D.FindNeighbour();
                river2D.SetNeighbour(riverMesh);
                IOFormater<IRiver> loader = river2D.GetFormater();
                string tmpFilter = saveFileDialog1.Filter;
                saveFileDialog1.Filter = loader.FilterSD;
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    loader.Write(river2D, saveFileDialog1.FileName);
                saveFileDialog1.Filter = tmpFilter;

                if(cb_Control.Checked == true)
                {
                    ISavePoint sp = new SavePoint();
                    sp.SetSavePoint(0, riverMesh, null);
                    river2D.AddMeshPolesForGraphics(sp);
                    Form vform = new ViForm(sp);
                    vform.Show();
                }

            }
            catch (Exception ex)
            {
                Logger.Instance.Info(ex.Message);
            }
        }

        private void bt_Load_Click(object sender, EventArgs e)
        {
            try
            {
                // Получить параметры задачи
                ParamsRiver2XYD Params = GetParams();
                // Создать задачу
                IRiver river2D = new TriRiverSWE2XYD(Params);
                // Создать загрузчик
                IOFormater<IRiver> loader = river2D.GetFormater();
                OpenFileDialog form = new OpenFileDialog();
                form.Filter = loader.FilterSD;
                if (form.ShowDialog() == DialogResult.OK)
                    loader.Read(form.FileName, ref river2D);
                ISavePoint sp = new SavePoint();
                IMesh riverMesh = river2D.Mesh();
                sp.SetSavePoint(0, riverMesh, null);
                river2D.AddMeshPolesForGraphics(sp);
                Form vform = new ViForm(sp);
                vform.Show();
            }
            catch (Exception ex)
            {
                Logger.Instance.Info(ex.Message);
            }

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SegmentInfo s = segInfo[listBox1.SelectedIndex];
            tb_Start.Text = s.startID.ToString();
            tb_End.Text = s.endID.ToString();
            tb_Qn.Text = s.Qn.ToString("F6");
            tb_Qt.Text = s.Qt.ToString("F6");
            tb_Eta.Text = s.Eta.ToString("F6");
            cbCritic.Checked = s.CriticalFlowRegime;
            string str = listBox1.Items[listBox1.SelectedIndex].ToString();
            string[] lines = str.Split(' ');
            if("Исток" == lines[0].Trim())
            {
                if(cbCritic.Checked == false)
                    tb_Eta.Enabled = true;
                else
                    tb_Eta.Enabled = false;
                tb_Qn.Enabled = false;
                tb_Qt.Enabled = false;
            }
            else
            {
                if (cbCritic.Checked == false)
                    tb_Eta.Enabled = false; 
                else
                    tb_Eta.Enabled = true;
                tb_Qn.Enabled = true;
                tb_Qt.Enabled = true;
            }
        }
        private void btSetSegment_Click(object sender, EventArgs e)
        {
            int saveIdx = listBox1.SelectedIndex;
            if (saveIdx > -1)
            {
                selected[saveIdx] = 1;
                segInfo[saveIdx].Qn = double.Parse(tb_Qn.Text, MEM.formatter);
                segInfo[saveIdx].Qt = double.Parse(tb_Qt.Text, MEM.formatter);
                segInfo[saveIdx].Eta = double.Parse(tb_Eta.Text, MEM.formatter);
                segInfo[saveIdx].CriticalFlowRegime = cbCritic.Checked;
            }
        }

        private void btSetWL_Click(object sender, EventArgs e)
        {
            if (tb_Eta.Enabled == true)
            {
                tb_Eta.Text = tb_RiverLvl.Text;
            }
        }

        private void tbQn_Click(object sender, EventArgs e)
        {
            if (tb_Qn.Enabled == true)
            {
                tb_Qn.Text = tb_RF.Text;
            }
        }
    }
}
