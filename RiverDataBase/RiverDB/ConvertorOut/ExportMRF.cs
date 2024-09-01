namespace RiverDB.ConvertorOut
{
    using CommonLib;
    using CommonLib.Mesh;
    using MemLogLib;
    using GeometryLib;
    using TriangleNet;
    using MeshAdapterLib;

    using System;
    using System.IO;
    using System.Windows.Forms;
    using System.Collections.Generic;
    using System.Linq;
    using CommonLib.IO;
    using GeometryLib.Locators;
    using CommonLib.Geometry;
    using NPRiverLib.APRiver_2XYD.River2DSW;
    using NPRiverLib.IO;
    using ConnectLib;

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
        public ExportMRF(MeshNet meshRiver, List<SegmentInfo> segInfo,int placeID = 1)
        {
            InitializeComponent();
            this.segInfo = segInfo;
            this.meshRiver = meshRiver;
            string ext = "*.mrf";
            string filter = "(*" + ext + ")|*" + ext + "| ";
            filter += " All files (*.*)|*.*";
            saveFileDialog1.Filter = filter;
            Init();
            listBox1.SelectedIndex = 0;
            // Ноль графика - отметка репера по Балтийской системе
            double hr = ConnectDB.WaterLevelGP(placeID);
            tb_BaltikLvl.Text = hr.ToString("F4");
        }

        protected void Init()
        {
            MeshAdapter.ConvertFrontRenumberationAndCutting(ref bmesh, ref values, meshRiver, Direction.toRight);
            x = bmesh.GetCoords(0);
            y = bmesh.GetCoords(1);
            //GetBConditionID(bmesh);

            string[] Names = { " Береговой сегмент реки", " Вход реки в область", " Выход реки из области" };
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

        public ParamsRiver_2XYD GetParams()
        {
            ParamsRiver_2XYD Params = new ParamsRiver_2XYD();
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
            TriRiverMesh meshRiver = new TriRiverMesh();
            try
            {
                int i;
                // размерность
                int Count = bmesh.CountKnots;
                int CountElements = bmesh.CountElements;
                int CountBoundElements = bmesh.CountBoundElements;
                int CountBoundSegments = segInfo.Count;
                // выделение памяти под узловые поля
                MEM.Alloc(Count, ref meshRiver.nodes);
                IFElement[] elems = bmesh.AreaElems;
                double BaltikLvl = double.Parse(tb_BaltikLvl.Text.Trim(), MEM.formatter);
                // чтение узловых полей
                for (i = 0; i < x.Length; i++)
                {
                    RiverNode node = new RiverNode();
                    node.n = i;
                    node.i = i;
                    node.X = x[i];
                    node.Y = y[i];
                    node.zeta = BaltikLvl - values[0][i];
                    node.ks = double.Parse(tb_Ks.Text.Trim(), MEM.formatter);
                    node.h = values[0][i];
                    node.qx = 0;
                    node.qy = 0;
                    node.fxc = 0; // плавающий узел
                    meshRiver.nodes[i] = node;
                }
                // выделение памяти под КЭ и поля
                MEM.Alloc(CountElements, ref meshRiver.AreaElems);
                // чтение КЭ полей
                for (i = 0; i < CountElements; i++)
                {
                    IFElement el = bmesh.AreaElems[i];
                    TriElementRiver elem = new TriElementRiver(i,
                        (uint)el.Nods[0].ID, (uint)el.Nods[1].ID, (uint)el.Nods[2].ID);
                    meshRiver.AreaElems[i] = elem;
                }
                // выделение памяти под граничные КЭ и поля
                MEM.Alloc(CountBoundElements, ref meshRiver.BoundElems);
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
                    meshRiver.BoundElems[i] = BoundElems;
                }
                // Сортировка ГКЭ для получения связности
                BoundElementRiver.Sort(ref meshRiver.BoundElems, Count);
                // Привязка
                //GetBConditionID(ref meshRiver.BoundElems);
                /// <summary>
                /// Определение граничных КЭ по сегменту
                /// </summary>
                //public void GetBConditionID(ref BoundElementRiver[] BoundElems)
                {
                    for (int be = 0; be < meshRiver.BoundElems.Length; be++)
                        meshRiver.BoundElems[be].boundCondType = 0;

                    LineLocator Locator = new LineLocator();
                    foreach (SegmentInfo s in segInfo)
                    {
                        List<BoundElementRiver> set = new List<BoundElementRiver>();
                        Locator.Set(s.pA, s.pB);
                        for (int be = 0; be < meshRiver.BoundElems.Length; be++)
                        {
                            uint Vertex1 = meshRiver.BoundElems[be].Vertex1;
                            uint Vertex2 = meshRiver.BoundElems[be].Vertex2;
                            IHPoint p1 = new HPoint(x[Vertex1], y[Vertex1]);
                            IHPoint p2 = new HPoint(x[Vertex2], y[Vertex2]);
                            bool flag1 = Locator.IsLiesOn(p1);
                            bool flag2 = Locator.IsLiesOn(p2);
                            if (flag1 == true && flag2 == true)
                            {
                                meshRiver.BoundElems[be].segmentID = s.ID;
                                meshRiver.BoundElems[be].boundCondType = s.boundCondType();
                                meshRiver.BoundElems[be].Qn = s.Qn;
                                meshRiver.BoundElems[be].Qt = s.Qt;
                                meshRiver.BoundElems[be].Eta = s.Eta;
                                set.Add(meshRiver.BoundElems[be]);
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
                MEM.Alloc(CountBoundSegments, ref meshRiver.boundSegment);
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
                    meshRiver.boundSegment[i] = segment;
                }
                // Определение граничных узлов 
                for (i = 0; i < meshRiver.BoundElems.Length; i++)
                {
                    uint Vertex1 = meshRiver.BoundElems[i].Vertex1;
                    // скользящий узел
                    meshRiver.nodes[Vertex1].fxc = FixedFlag.sliding;
                    IHPoint pA = new HPoint(x[Vertex1], y[Vertex1]);
                    for (int j = 0; j < segInfo.Count; j++)
                    {
                        if (MEM.Equals(segInfo[j].pA, pA) == true)
                        {
                            // фиксированный узел - вершина полигона
                            meshRiver.nodes[Vertex1].fxc = FixedFlag.fixednode;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Info(ex.Message);
            }
            return meshRiver;
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
                #region Old
                //int i;
                //river2D.dtTrend = double.Parse(tb_Version.Text, MEM.formatter);
                //river2D.time = double.Parse(tb_time.Text, MEM.formatter);
                //river2D.dtime = double.Parse(tb_dtime.Text, MEM.formatter);
                //river2D.theta = double.Parse(tb_theta.Text, MEM.formatter);
                //river2D.UpWindCoeff = double.Parse(tb_UpWindCoeff.Text, MEM.formatter);
                //river2D.turbulentVisCoeff = double.Parse(tb_turbulentVisCoeff.Text, MEM.formatter);
                //river2D.latitudeArea = double.Parse(tb_latitudeArea.Text, MEM.formatter);
                //// русло
                //river2D.droundWaterCoeff = double.Parse(tb_droundWaterCoeff.Text, MEM.formatter);
                //river2D.H_minGroundWater = double.Parse(tb_minGroundWater.Text, MEM.formatter);
                //river2D.filtrСoeff = double.Parse(tb_filtrСoeff.Text, MEM.formatter);
                //// размерность

                //int Count = bmesh.CountKnots;
                //int CountElements = bmesh.CountElements;
                //int CountBoundElements = bmesh.CountBoundElements;
                //int CountBoundSegments = segInfo.Count;
                //if (river2D.meshRiver == null)
                //    river2D.meshRiver = new TriRiverMesh();
                //// выделение памяти под узловые поля
                //MEM.Alloc(Count, ref river2D.meshRiver.nodes);
                //IFElement[] elems = bmesh.AreaElems;

                //double BaltikLvl = double.Parse(tb_BaltikLvl.Text.Trim(), MEM.formatter);
                //// чтение узловых полей
                //for (i = 0; i < x.Length; i++)
                //{
                //    RiverNode node = new RiverNode();
                //    node.n = i;
                //    node.i = i;
                //    node.X = x[i];
                //    node.Y = y[i];
                //    node.zeta = BaltikLvl - values[0][i];
                //    node.ks = double.Parse(tb_Ks.Text.Trim(), MEM.formatter);
                //    node.h = values[0][i];
                //    node.qx = 0;
                //    node.qy = 0;
                //    node.fxc = 0; // плавающий узел
                //    river2D.meshRiver.nodes[i] = node;
                //}
                //// выделение памяти под КЭ и поля
                //MEM.Alloc(CountElements, ref river2D.meshRiver.AreaElems);
                //// чтение КЭ полей
                //for (i = 0; i < CountElements; i++)
                //{
                //    IFElement el = bmesh.AreaElems[i];
                //    TriElementRiver elem = new TriElementRiver(i,
                //        (uint)el.Nods[0].ID, (uint)el.Nods[1].ID, (uint)el.Nods[2].ID);
                //    river2D.meshRiver.AreaElems[i] = elem;
                //}
                //// выделение памяти под граничные КЭ и поля
                //MEM.Alloc(CountBoundElements, ref river2D.meshRiver.BoundElems);
                //for (i = 0; i < CountBoundElements; i++)
                //{
                //    IFElement el = bmesh.BoundElems[i];
                //    BoundElementRiver BoundElems = new BoundElementRiver();
                //    BoundElems.ID = i;
                //    BoundElems.boundCondType = 0;
                //    BoundElems.Vertex1 = (uint)el.Nods[0].ID;
                //    BoundElems.Vertex2 = (uint)el.Nods[1].ID;
                //    BoundElems.Eta = 0;
                //    BoundElems.Qn = 0;
                //    BoundElems.Qt = 0;
                //    river2D.meshRiver.BoundElems[i] = BoundElems;
                //}
                //// Сортировка ГКЭ для получения связности
                //BoundElementRiver.Sort(ref river2D.meshRiver.BoundElems, Count);
                //// Привязка
                //GetBConditionID(ref river2D.meshRiver.BoundElems);
                //// Привязка ГЭ к КЭ
                //// выделение памяти под граничные сегменты
                //MEM.Alloc(CountBoundSegments, ref river2D.meshRiver.boundSegment);
                //for (i = 0; i < CountBoundSegments; i++)
                //{
                //    SegmentInfo si = segInfo[i];
                //    BoundSegmentRiver segment = new BoundSegmentRiver();
                //    segment.ID = i;
                //    segment.startnode = si.startID;
                //    segment.endnode = si.endID;
                //    segment.boundCondType = si.boundCondType();
                //    segment.Hn = si.Eta;
                //    segment.Qn = si.Qn;
                //    river2D.meshRiver.boundSegment[i] = segment;
                //}
                //// Определение граничных узлов 
                //for (i = 0; i < river2D.meshRiver.BoundElems.Length; i++)
                //{
                //    uint Vertex1 = river2D.meshRiver.BoundElems[i].Vertex1;
                //    // скользящий узел
                //    river2D.meshRiver.nodes[Vertex1].fxc = 1;
                //    IHPoint pA = new HPoint(x[Vertex1], y[Vertex1]);
                //    for (int j=0; j<segInfo.Count; j++)
                //    {
                //        if( MEM.Equals(segInfo[j].pA , pA) == true )
                //        {
                //            // фиксированный узел - вершина полигона
                //            river2D.meshRiver.nodes[Vertex1].fxc = 2;
                //        }    
                //    }
                //}
                #endregion 
                // Конвертировать сетку IFEMesh Получить речную сетку в TriRiverMesh
                TriRiverMesh riverMesh = ConvertIFEMeshToRiver2DMesh(bmesh);
                // Получить параметры задачи
                ParamsRiver_2XYD Params = GetParams();
                // Создать задачу
                TriRiverSWE_2XYD river2D = new TriRiverSWE_2XYD(Params);
                //river2D.Set(riverMesh);
                //river2D.FindNeighbour();
                river2D.SetNeighbour(riverMesh);
                IOFormater<IRiver> loader = river2D.GetFormater();
                string tmpFilter = saveFileDialog1.Filter;
                saveFileDialog1.Filter = loader.FilterSD;
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    loader.Write(river2D, saveFileDialog1.FileName);
                saveFileDialog1.Filter = tmpFilter;
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
            if("Выход" == lines[1].Trim())
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

        private void cbCritic_CheckedChanged(object sender, EventArgs e)
        {
          //  listBox1_SelectedIndexChanged(sender, e);
        }
    }
}
