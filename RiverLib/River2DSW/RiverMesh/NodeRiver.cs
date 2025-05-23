﻿////---------------------------------------------------------------------------
////                     ПРОЕКТ  "RiverSolver"
////                         проектировщик:
////                           Потапов И.И.
////---------------------------------------------------------------------------
////               кодировка : 02.06.2021 Потапов И.И.
////---------------------------------------------------------------------------
////               кодировка : 27.12.2023 Потапов И.И.  оптимизация и читка
////---------------------------------------------------------------------------
//namespace RiverLib.River2D.RiverMesh
//{
//    using MemLogLib;
//    using System;

//    enum fixedFlag 
//    { 
//        /// <summary>
//        /// Свободный узел
//        /// </summary>
//        floating = 0, 
//        fixednode = 1, 
//        sliding =2 
//    };
//    /// <summary>
//    /// Узел КЭ сетки для задач речной гидродинамики
//    /// </summary>
//    [Serializable]
//    public class RiverNode
//    {
//        /// <summary>
//        /// номер узла
//        /// </summary>
//        public int n;
//        /// <summary>
//        /// индекс узла
//        /// </summary>
//        public int i;
//        /// <summary>
//        /// тип фиксации 
//        /// </summary>
//        public int fxc;
//        /// <summary>
//        ///  номер граничного сегмента
//        /// </summary>
//        public int segmentID = -1;
//        /// <summary>
//        /// Координата Х узла
//        /// </summary>
//        public double X { get => x[0]; set => x[0] = value; }
//        /// <summary>
//        /// Координата Y узла
//        /// </summary>
//        public double Y { get => x[1]; set => x[1] = value; }
//        /// <summary>
//        /// x[0] = x, x[1] = y    control/dims
//        /// </summary>
//        public double[] x = new double[2];
//        #region Расчетные поля
//        /// <summary>
//        /// глубина
//        /// </summary>
//        public double h { get => u[0]; set => u[0] = value; }
//        /// <summary>
//        /// расход по х
//        /// </summary>
//        public double qx { get => u[1]; set => u[1] = value; }
//        /// <summary>
//        /// расход по у
//        /// </summary>
//        public double qy { get => u[2]; set => u[2] = value; }
//        /// <summary>
//        /// u[0] = h, u[1] = qx, u[2] = qy    (time)  control/vars
//        /// </summary>
//        public double[] u = new double[3];
//        /// <summary>
//        /// поля на (time - 1) (предыдущей) итерации
//        /// </summary>
//        public double h0 { get => uo[0]; set => uo[0] = value; }
//        /// <summary>
//        /// расход по х
//        /// </summary>
//        public double qx0 { get => uo[1]; set => uo[1] = value; }
//        /// <summary>
//        /// расход по у
//        /// </summary>
//        public double qy0 { get => uo[2]; set => uo[2] = value; }
//        /// <summary>
//        /// u[0] = h, u[1] = qx, u[2] = qy  (time - 1)
//        /// </summary>
//        public double[] uo = new double[3];
//        /// <summary>
//        /// поля на (time - 2) итерации
//        /// </summary>
//        public double h00 { get => uoo[0]; set => uoo[0] = value; }
//        /// <summary>
//        /// расход по х
//        /// </summary>
//        public double qx00 { get => uoo[1]; set => uoo[1] = value; }
//        /// <summary>
//        /// расход по у
//        /// </summary>
//        public double qy00 { get => uoo[2]; set => uoo[2] = value; }
//        /// <summary>
//        /// поля на пред предыдущей итерации
//        /// u[0] = h, u[1] = qx, u[2] = qy  (time - 2) 
//        /// </summary>
//        protected double[] uoo = new double[3];
//        #endregion 
//        /// <summary>
//        /// отметка дна
//        /// </summary>
//        public double zeta { get => p[0]; set => p[0] = value; }
//        /// <summary>
//        /// шероховатость дна
//        /// </summary>
//        public double ks { get => p[1]; set => p[1] = value; }
//        /// <summary>
//        ///  p[0] = z (дно), 
//        ///  p[1] = ks; - roughness/шероховатость дна  
//        /// </summary>
//        protected double[] p = new double[3];
//        public double Hice { get => ice[0]; set => ice[0] = value; }
//        public double KsIce { get => ice[1]; set => ice[1] = value; }
//        /// <summary>
//        /// ice[0] - ice thickness/ толщина льда  
//        /// ice[1] - ice roughness/шероховатость льда 
//        /// </summary>
//        protected double[] ice = new double[3];
//        public double h_ise { get => ice[0]; set => ice[0] = value; }
//        public double ks_ise { get => ice[1]; set => ice[1] = value; }
//        /// <summary>
//        /// глубина без льда
//        /// </summary>
//        public double hd { get => ud[0]; set => ud[0] = value; }
//        /// <summary>
//        /// скорость по х
//        /// </summary>
//        public double udx { get => ud[1]; set => ud[1] = value; }
//        /// <summary>
//        /// скорость по у
//        /// </summary>
//        public double udy { get => ud[2]; set => ud[2] = value; }
//        /// <summary>
//        /// поля h udx udy
//        /// </summary>
//        protected double[] ud = new double[4];

//        public RiverNode() { }
//        public RiverNode(RiverNode node)
//        {
//            n = node.n;
//            i = node.i;
//            fxc = node.fxc;
//            segmentID = node.segmentID;
//            MEM.MemCopy<double>(ref x, node.x);
//            MEM.MemCopy<double>(ref u, node.u);
//            MEM.MemCopy<double>(ref p, node.p);
//            MEM.MemCopy<double>(ref ice, node.ice);
//            MEM.MemCopy<double>(ref uo, node.uo);
//            MEM.MemCopy<double>(ref uoo, node.uoo);
//        }
//        public static RiverNode Parse(string[] lines, int index = 0)
//        {
//            RiverNode node = new RiverNode();
//            node.n = int.Parse(lines[0].Trim());
//            node.i = index;
//            node.X = double.Parse(lines[1].Trim(), MEM.formatter);
//            node.Y = double.Parse(lines[2].Trim(), MEM.formatter);
//            node.zeta = double.Parse(lines[3].Trim(), MEM.formatter);
//            node.ks = double.Parse(lines[4].Trim(), MEM.formatter);
//            node.h = double.Parse(lines[5].Trim(), MEM.formatter);
//            node.qx = double.Parse(lines[6].Trim(), MEM.formatter);
//            node.qy = double.Parse(lines[7].Trim(), MEM.formatter);
//            node.fxc = 0; // плавающий узел
//            return node;
//        }
//        public static RiverNode Parse(string line, int index = 0)
//        {
//            string[] lines = (line.Trim()).Split(' ');
//            return Parse(lines, index);
//        }
//        public string ToString(string format = "F6")
//        {
//            string node = n.ToString() + "\t" +
//                          X.ToString(format) + "\t" +
//                          Y.ToString(format) + "\t" +
//                          zeta.ToString(format) + "\t" +
//                          ks.ToString(format) + "\t" +
//                          h.ToString(format) + "\t" +
//                          qx.ToString(format) + "\t" +
//                          qy.ToString(format);
//            return node;
//        }
//        /// <summary>
//        /// Формат для файла *.cdg
//        /// </summary>
//        /// <param name="format"></param>
//        /// <returns></returns>
//        public string ToStringCDG(string format = "F6")
//        {
//            int sliding = 1;
//            int fixednode = 2;
//            string FXC = " ";
//            if (fxc == sliding)
//                FXC = "s ";
//            else if (fxc == fixednode)
//                FXC = "x ";
//            string node = (n+1).ToString() + "\t" +
//                          FXC + "\t" +
//                          X.ToString(format) + "\t" +
//                          Y.ToString(format) + "\t" +
//                          zeta.ToString(format) + "\t" +
//                          ks.ToString(format) + "\t" + "10 \t" +
//                          h.ToString(format) + "\t" +
//                          qx.ToString(format) + "\t" +
//                          qy.ToString(format);
//            return node;
//        }
//        /// <summary>
//        /// Формат для файла *.bed
//        /// </summary>
//        /// <param name="format"></param>
//        /// <returns></returns>
//        public string ToStringBED(string format = "F6")
//        {
//            string node = (n + 1).ToString() + "\t" +
//                          X.ToString(format) + "\t" +
//                          Y.ToString(format) + "\t" +
//                          zeta.ToString(format) + "\t" +
//                          ks.ToString(format);
//            return node;
//        }
//    }
//}
