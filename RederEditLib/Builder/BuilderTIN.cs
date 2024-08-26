namespace RenderEditLib
{
    using System.IO;
    using CommonLib.Areas;
    using CommonLib.Mesh.RVData;
    using GeometryLib.Areas;
    using TriMeshGeneratorLib;
    
    class BuilderTIN
    {
        IMFigura fig;
        public BuilderTIN(IMFigura fig)
        {
            this.fig = fig;
        }
        /// <summary>
        /// Создание связности и условий для генерации сетки
        /// </summary>
        /// <param name="physics"></param>
        /// <returns></returns>
        public RVMeshRiver Create(RVHabitatPhysics physics)
        {
            RVMeshRiver hmesh = new RVMeshRiver(physics);

            int CountNodes = fig.Count;
            int CountBoundElements = fig.Count;
            int CountBoundSegs = fig.Count; ;
            // Чтение узлов
            for (int i = 0; i < CountNodes; i++)
            {
                IMPoint node = fig.GetPoint(i);
                int n = i;
                double x = node.Point.X;
                double y = node.Point.Y;

                double z = 10;
                double ks = 0.01;
                double chi = 10.0;
                double depth = 1;
                double qx = 0;
                double qy = 0;

                RVNodeHabitat hNode = new RVNodeHabitat(physics, n, x, y, z, ks, chi, depth, qx, qy);
                hNode.Fixed = RVFixedNodeFlag.fixedNode;
                hmesh.AddNode(hNode);
            }
            //
            for (int k = 0; k < CountBoundElements; k++)
            {
                IMSegment segmennt = fig.GetSegment(k);
                int n = k;
                int nodeA = segmennt.nodeA;
                int nodeB = segmennt.nodeB;

                // значения на границе
                double p1 = 1;
                double p2 = 2;
                // флаг условий на границе
                int c1 = 1;

                RVNode n1P = hmesh.GetNodeByID(nodeA);
                RVNode n2P = hmesh.GetNodeByID(nodeB);
                double bValue = 0.0;
                if (c1 == 1)
                    bValue = p2;
                else if (c1 == 3)
                    bValue = p1;
                RVBoundary segment = new RVBoundary(n, n1P, n2P, c1, bValue);
                hmesh.AddBoundarySegment(segment);
            }

            for (int l = 0; l < CountBoundSegs; l++)
            {
                IMSegment segmennt = fig.GetSegment(l);
                int nodeA = segmennt.nodeA;
                int nodeB = segmennt.nodeB;
                //   значения
                int n = l;
                int code = 0;   // тип границы
                double pA = 1;  // значение в A
                double pB = 1;  // значение в B
                string filePath = "";
                int transCode;
                if (filePath != "")
                    transCode = 0;
                else
                    transCode = 1;
                RVNode start = hmesh.GetNodeByID(nodeA);
                RVNode end = hmesh.GetNodeByID(nodeB);
                RVFlowBoundary flowBP = new RVFlowBoundary(n, start, end, code, pA, pB, transCode, filePath);
                hmesh.FlowBoundList.Add(flowBP);
                hmesh.SetBElementFlowBound(flowBP, start, end);
            }

            string name = Path.GetFileNameWithoutExtension("test") + ".sdg";
            using (StreamWriter fsave = new StreamWriter(name))
            {
                hmesh.writeLMesh_sdg(fsave);
                fsave.Close();
            }
            return hmesh;
        }
        ///// <summary>
        ///// Чтение узлов в формате sdg
        ///// </summary>
        ///// <param name="f"></param>
        //public void readNode(StreamReader f)
        //{
        //    string[] lines = RL.GetLines(f);
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
        //    double x = double.Parse(lines[idx++].Trim(), MEM.formatter);
        //    double y = double.Parse(lines[idx++].Trim(), MEM.formatter);
        //    double z = double.Parse(lines[idx++].Trim(), MEM.formatter);

        //    double ks = double.Parse(lines[idx++].Trim(), MEM.formatter);
        //    double depth = double.Parse(lines[idx++].Trim(), MEM.formatter);
        //    double qx = double.Parse(lines[idx++].Trim(), MEM.formatter);
        //    double qy = double.Parse(lines[idx++].Trim(), MEM.formatter);

        //    RVNodeHabitat nP = new RVNodeHabitat((RVHabitatPhysics)GetPhysics(), n, x, y, z, ks, 5.0, depth, qx, qy);
        //    if (fx == RVFixedNodeFlag.fixedNode)
        //        nP.Fixed = RVFixedNodeFlag.fixedNode;
        //    if (fx == RVFixedNodeFlag.slidingNode)
        //        nP.Fixed = RVFixedNodeFlag.slidingNode;
        //    AddNode(nP);
        //    return;
        //}
        ///// <summary>
        ///// чтение граничных элементов в формате sdg
        ///// </summary>
        ///// <param name="f"></param>
        //public void readBElm(StreamReader f)
        //{
        //    string[] lines = RL.GetLines(f);
        //    if (lines == null) return;
        //    int n = int.Parse(lines[0].Trim());
        //    int nn1 = int.Parse(lines[1].Trim());
        //    int nn2 = int.Parse(lines[2].Trim());
        //    // значения
        //    double p1 = double.Parse(lines[3].Trim(), MEM.formatter);
        //    double p2 = double.Parse(lines[4].Trim(), MEM.formatter);
        //    // флаг
        //    int c1 = int.Parse(lines[5].Trim());
        //    RVNode n1P = GetNodeByID(nn1);
        //    RVNode n2P = GetNodeByID(nn2);
        //    double bValue = 0.0;
        //    if (c1 == 1)
        //        bValue = p2;
        //    else if (c1 == 3)
        //        bValue = p1;
        //    RVBoundary bSeg = new RVBoundary(n, n1P, n2P, c1, bValue);
        //    AddBoundarySegment(bSeg);
        //    return;
        //}
        ///// <summary>
        ///// чтение граничных сегментов в формате sdg
        ///// </summary>
        ///// <param name="f"></param>
        //public void readBSegments(StreamReader f)
        //{
        //    string[] lines = RL.GetLines(f);
        //    if (lines == null) return;
        //    int n = int.Parse(lines[0].Trim());
        //    int code = int.Parse(lines[1].Trim());
        //    double p0 = double.Parse(lines[2].Trim(), MEM.formatter);
        //    double p1 = double.Parse(lines[3].Trim(), MEM.formatter);
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
        //    RVNode start = GetNodeByID(startnode);
        //    RVNode end = GetNodeByID(endnode);
        //    RVFlowBoundary flowBP = new RVFlowBoundary(n, start, end, code, p0, p1, transCode, filePath);
        //    flowBoundL.Add(flowBP);
        //    setBElementFlowBound(flowBP, start, end);
        //    if (filePath != "")
        //        using (StreamReader file = new StreamReader(filePath))
        //        {
        //            if (file != null)
        //                flowBP.loadTransLst(file);
        //        }
        //}

    }
}
