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
    /// Тип узла
    /// </summary>
    [Serializable]
    public enum FixedFlag 
    { 
        /// <summary>
        /// Свободный узел
        /// </summary>
        floating = 0, 
        /// <summary>
        /// Фиксированный узел
        /// </summary>
        fixednode = 1, 
        /// <summary>
        /// Скользящий вдоль по грани границы узел
        /// </summary>
        sliding = 2 
    };
    /// <summary>
    /// Узел КЭ сетки для задач речной гидродинамики
    /// </summary>
    [Serializable]
    public class BedRiverNode : IBedRiverNode
    {
        /// <summary>
        /// Координата Х узла
        /// </summary>
        public double X { get => x[0]; set => x[0] = value; }
        /// <summary>
        /// Координата Y узла
        /// </summary>
        public double Y { get => x[1]; set => x[1] = value; }
        /// <summary>
        /// x[0] = x, x[1] = y    control/dims
        /// </summary>
        public double[] x = new double[2];
        /// <summary>
        /// отметка дна
        /// </summary>
        public double zeta { get => p[0]; set => p[0] = value; }
        /// <summary>
        /// шероховатость дна
        /// </summary>
        public double ks { get => p[1]; set => p[1] = value; }
        /// <summary>
        ///  p[0] = z (дно), 
        ///  p[1] = ks; - roughness/шероховатость дна  
        /// </summary>
        protected double[] p = new double[2];

        public BedRiverNode() { }

        public BedRiverNode(double x, double y, double zeta, double ks) 
        {
            this.x[0] = x;
            this.x[1] = y;
            this.zeta = zeta;
            this.ks = ks;
        }
        public BedRiverNode(BedRiverNode node)
        {
            MEM.MemCopy<double>(ref x, node.x);
            MEM.MemCopy<double>(ref p, node.p);
        }
        public static BedRiverNode Parse(string[] lines)
        {
            BedRiverNode node = new BedRiverNode();
            node.X = double.Parse(lines[1].Trim(), MEM.formatter);
            node.Y = double.Parse(lines[2].Trim(), MEM.formatter);
            node.zeta = double.Parse(lines[3].Trim(), MEM.formatter);
            node.ks = double.Parse(lines[4].Trim(), MEM.formatter);
            return node;
        }
        public static BedRiverNode Parse(string line)
        {
            string[] lines = (line.Trim()).Split(' ');
            return Parse(lines);
        }
        /// <summary>
        /// Для mrf формата
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public string ToString(string format = "F6")
        {
            string node = X.ToString(format) + "\t" +
                          Y.ToString(format) + "\t" +
                          zeta.ToString(format) + "\t" +
                          ks.ToString(format);
            return node;
        }
        /// <summary>
        /// Формат для файла *.bed
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public string ToStringBED(int ID, string format = "F6")
        {
            string node = ID.ToString() + "\t" +
                          X.ToString(format) + "\t" +
                          Y.ToString(format) + "\t" +
                          zeta.ToString(format) + "\t" +
                          ks.ToString(format);
            return node;
        }
        public int CompareTo(object obj)
        {
            BedRiverNode a = obj as BedRiverNode;
            if (a != null)
            {
                if (zeta < a.zeta)
                    return -1;
                if (zeta > a.zeta)
                    return 1;
                return 0;
            }
            else
                throw new Exception("ошибка приведения типа");
        }
        /// <summary>
        /// Создает копию объекта
        /// </summary>
        /// <returns></returns>
        public virtual IHPoint IClone() => new BedRiverNode(this);
    }
}
