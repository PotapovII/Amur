//---------------------------------------------------------------------------
//                     ПРОЕКТ  "RiverSolver"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//               кодировка : 02.06.2021 Потапов И.И.
//---------------------------------------------------------------------------
//               кодировка : 27.12.2023 Потапов И.И.  оптимизация и читка
//---------------------------------------------------------------------------
//           + BedRiverNode + MeshRiverNode : 17.08.2024 Потапов И.И.  
//---------------------------------------------------------------------------
namespace GeometryLib
{
    using CommonLib.Geometry;
    using MemLogLib;
    using System;
    /// <summary>
    /// Узел КЭ сетки для задач речной гидродинамики
    /// </summary>
    [Serializable]
    public class MeshRiverNode : BedRiverNode, IMeshRiverNode
    {
        #region Расчетные поля
        /// <summary>
        /// глубина
        /// </summary>
        public double h { get => u[0]; set => u[0] = value; }
        /// <summary>
        /// расход по х
        /// </summary>
        public double qx { get => u[1]; set => u[1] = value; }
        /// <summary>
        /// расход по у
        /// </summary>
        public double qy { get => u[2]; set => u[2] = value; }
        /// <summary>
        /// u[0] = h, u[1] = qx, u[2] = qy    (time)  control/vars
        /// </summary>
        public double[] u = new double[3];
        #endregion 

        public MeshRiverNode() { }
        public MeshRiverNode(MeshRiverNode node):base(node) 
        {
            MEM.MemCopy<double>(ref u, node.u);
        }
        public new static MeshRiverNode Parse(string[] lines)
        {
            MeshRiverNode node = new MeshRiverNode();
            node.X = double.Parse(lines[1].Trim(), MEM.formatter);
            node.Y = double.Parse(lines[2].Trim(), MEM.formatter);
            node.zeta = double.Parse(lines[3].Trim(), MEM.formatter);
            node.ks = double.Parse(lines[4].Trim(), MEM.formatter);
            node.h = double.Parse(lines[5].Trim(), MEM.formatter);
            node.qx = double.Parse(lines[6].Trim(), MEM.formatter);
            node.qy = double.Parse(lines[7].Trim(), MEM.formatter);
            return node;
        }
        public new static MeshRiverNode Parse(string line)
        {
            string[] lines = (line.Trim()).Split(' ');
            return Parse(lines);
        }
        /// <summary>
        /// Для mrf формата
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public new string ToString(string format = "F6")
        {
            string node = X.ToString(format) + "\t" +
                          Y.ToString(format) + "\t" +
                          zeta.ToString(format) + "\t" +
                          ks.ToString(format) + "\t" +
                          h.ToString(format) + "\t" +
                          qx.ToString(format) + "\t" +
                          qy.ToString(format);
            return node;
        }
        /// <summary>
        /// Для mrf формата
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public string ToString(int ID, string format = "F6")
        {
            string node = ID.ToString() + "\t" + ToString(format);
            return node;
        }
        /// <summary>
        /// Создает копию объекта
        /// </summary>
        /// <returns></returns>
        public override IHPoint IClone() => new MeshRiverNode(this);
    }
}
