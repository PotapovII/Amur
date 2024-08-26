//---------------------------------------------------------------------------
//                     ПРОЕКТ  "RiverSolver"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//               кодировка : 04.06.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace RiverLib.River2D.RiverMesh
{
    using MemLogLib;
    using System;
    /// <summary>
    /// ОО: Линия для определения значений на элементах контура расчетной области
    /// путем линейной интерполяции значений 
    /// </summary>
    [Serializable]
    public class BoundSegmentRiver
    {
        /// <summary>
        /// код линии
        /// </summary>
        public int ID = 0;
        /// <summary>
        /// значение гу глубина на границе
        /// </summary>
        public double Hn = 0;  // p0
        /// <summary>
        /// значение гу расход на границе
        /// </summary>
        public double Qn = 0; // p1
        /// <summary>
        /// Интеграл по КЭ сегмента вида sum_belem( fac H^(5/3) L_belem )
        /// </summary>
        public double Dcoef = 0; // p2
        /// <summary>
        /// средняя глубина на ГКЭ/средняя глубина без слоя льда на ГКЭ
        /// </summary>
        public double H = 0; // p3
        /// <summary>
        /// средняя отметка дна на ГКЭ
        /// </summary>
        public double zeta = 0; // p4
        /// <summary>
        /// расстояние между 0 и 1 узлом грани
        /// </summary>
        public double Length; // { get => p5; set => p5 = value; }
        /// <summary>
        /// флаг типпа граничных условий
        /// </summary>
        public int boundCondType = 0;
        /// <summary>
        /// номер узла в начале линии
        /// </summary>
        public int startnode = 0;
        /// <summary>
        /// номер узла в конце линии
        /// </summary>
        public int endnode = 0;
        public BoundSegmentRiver() { }
        public BoundSegmentRiver(BoundSegmentRiver s)
        {
            ID = s.ID;
            Hn = s.Hn;
            Qn = s.Qn;
            boundCondType = s.boundCondType;
            startnode = s.startnode;
            endnode = s.endnode;
        }
        public static BoundSegmentRiver Parse(string[] lines)
        {
            BoundSegmentRiver segment = new BoundSegmentRiver();
            segment.ID = int.Parse(lines[0].Trim());
            segment.startnode = int.Parse(lines[1].Trim());
            segment.endnode = int.Parse(lines[2].Trim());
            segment.boundCondType = int.Parse(lines[3].Trim());
            segment.Hn = double.Parse(lines[4].Trim(), MEM.formatter);
            segment.Qn = double.Parse(lines[5].Trim(), MEM.formatter);
            return segment;
        }
        public static BoundSegmentRiver Parse(string line)
        {
            string[] lines = (line.Trim()).Split(' ');
            return Parse(lines);
        }
        public string ToString(string format = "F6")
        {
            string segment = ID.ToString() + "\t" +
                             startnode.ToString() + "\t" +
                             endnode.ToString() + "\t" +
                             boundCondType.ToString() + "\t" +
                             Hn.ToString(format) + "\t" +
                             Qn.ToString(format);
            return segment;
        }

        public string ToStringCDG(string format = "F6")
        {
            string segment = (ID+1).ToString() + "\t" +
                             boundCondType.ToString() + "\t" +
                             Hn.ToString(format) + "\t" +
                             Qn.ToString(format) + "\t" +
                             (startnode+1).ToString() + "\t" +
                             (endnode+1).ToString();
            return segment;
        }
    }
}
