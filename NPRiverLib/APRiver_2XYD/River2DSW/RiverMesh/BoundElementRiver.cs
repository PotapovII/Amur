//---------------------------------------------------------------------------
//                     ПРОЕКТ  "RiverSolver"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//               кодировка : 03.06.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace NPRiverLib.APRiver_2XYD.River2DSW
{
    using CommonLib;
    using MemLogLib;
    using System;
    /// <summary>
    /// ОО: Класс речной конечный элементы
    /// </summary>
    [Serializable]
    public class BoundElementRiver
    {
        /// <summary>
        /// Номер ГЭ
        /// </summary>
        public int ID;
        /// <summary>
        /// Номер родителя
        /// </summary>
        public int elementID;
        /// <summary>
        ///  номер граничного сегмента
        /// </summary>
        public int segmentID;
        /// <summary>
        /// свободная поверхность на границе
        /// </summary>
        public double Eta { get => p[0]; set => p[0] = value; }
        /// <summary>
        /// нормальный расход на границе
        /// </summary>
        public double Qn { get => p[1]; set => p[1] = value; }
        /// <summary>
        /// касательный поток к границе
        /// </summary>
        public double Qt { get => p[2]; set => p[2] = value; }
        /// <summary>
        /// средняя глубина потока на ГЭ, для dryFlag = 1 || (the belp is dry)  = 0
        /// </summary>
        public double H { get => p[3]; set => p[3] = value; }
        /// <summary>
        /// средне элеметнаый уровень дна
        /// </summary>
        public double Zeta { get => p[4]; set => p[4] = value; }
        /// <summary>
        /// расстояние между 0 и 1 узлом грани
        /// </summary>
        public double Length { get => p[5]; set => p[5] = value; }
        /// <summary>
        /// Параметры КЭ
        /// </summary>
        protected double[] p = new double[7];
        /// <summary>
        /// Узел 1
        /// </summary>
        public uint Vertex1;// { get => nodes.Vertex1; set => nodes.Vertex1 = value; }
        /// <summary>
        /// Узел 2
        /// </summary>
        public uint Vertex2;// { get => nodes.Vertex2; set => nodes.Vertex2 = value; }
        /// <summary>
        /// Узлы граничного КЭ
        /// </summary>
        public TwoElement nodes { get => new TwoElement(Vertex1, Vertex2); }

        /// <summary>
        //    switch (boundCondType)
        //    {
        //        case 0:            elem  segment
        //            bcc[0] = 0.0;  H      H
        //            bcc[1] = 1.0;  Qx     Qn
        //            bcc[2] = 1.0;  Qy     Qt
        //            break;
        //        case 1:
        //            bcc[0] = 0.0;
        //            bcc[1] = 1.0;
        //            bcc[2] = 1.0;
        //            break;
        //        case 2:
        //            bcc[0] = 1.0;
        //            bcc[1] = 1.0;
        //            bcc[2] = 1.0;
        //            break;
        //        case 3:
        //            bcc[0] = 1.0;
        //            bcc[1] = 0.0;
        //            bcc[2] = 0.0;
        //            break;
        //        case 4:
        //            bcc[0] = 0.0;
        //            bcc[1] = 0.0;
        //            bcc[2] = 0.0;
        //            break;
        //        case 5:
        //            bcc[0] = 0.0;
        //            bcc[1] = 0.0;
        //            bcc[2] = 0.0;
        //            break;
        //    }
        /// </summary>
        
        /// <summary>
        /// Тип граничного условия
        /// </summary>
        public int boundCondType;

        public void Clear()
        {
            for (int j = 0; j < p.Length; j++)
                p[j] = 0.0;
        }
        public BoundElementRiver()
        {
            ID = 0;
            p = new double[7];
        }
        public BoundElementRiver(BoundElementRiver e)
        {
            Vertex1 = e.Vertex1;
            Vertex2 = e.Vertex2;
            segmentID = e.segmentID;
            elementID = e.elementID;
            boundCondType = e.boundCondType;
            MEM.MemCopy<double>(ref p, e.p);
        }
        public static BoundElementRiver Parse(string[] lines)
        {
            BoundElementRiver BoundElems = new BoundElementRiver();
            BoundElems.ID = int.Parse(lines[0].Trim());
            BoundElems.boundCondType = int.Parse(lines[1].Trim());
            BoundElems.Vertex1 = uint.Parse(lines[2].Trim());
            BoundElems.Vertex2 = uint.Parse(lines[3].Trim());
            BoundElems.Eta = double.Parse(lines[4].Trim(), MEM.formatter);
            BoundElems.Qn = double.Parse(lines[5].Trim(),   MEM.formatter);
            BoundElems.Qt = double.Parse(lines[6].Trim(),   MEM.formatter);
            
            return BoundElems;
        }
        public static BoundElementRiver Parse(string line)
        {
            string[] lines = (line.Trim()).Split(' ','\t');
            return Parse(lines);
        }
        /// <summary>
        /// Вывод полей в строку для граничного элемента
        /// </summary>

        public string ToString(string format = "F6")
        {
            string BE = ID.ToString() + "\t" +
                        boundCondType.ToString() + "\t" +
                        Vertex1.ToString() + "\t" +
                        Vertex2.ToString() + "\t" +
                        Eta.ToString(format) + "\t" +
                        Qn.ToString(format) + "\t" +
                        Qt.ToString(format);

            return BE;
        }
        /// <summary>
        /// Вывод полей в строку для граничного элемента
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public string ToStringCDG(string format = "F6")
        {
            string BE = (ID+1).ToString() + "\t" + "111\t111\t" +
                        (Vertex1+1).ToString() + "\t" +
                        (Vertex2+1).ToString() + "\t";
            // выход сверхкритический режим    
            if (boundCondType == 1)
                // вход докритический режим    
                BE += "0\t" + Qn.ToString(format) + "\t" + Qt.ToString(format) + "\t0\t0\t0\t0\t1\t0\t0";
            else if (boundCondType == 2)
                // вход сверхкритический режим
                BE += Eta.ToString(format) + "\t" + Qn.ToString(format) + "\t" + Qt.ToString(format) + "\t0\t0\t0\t0\t5\t0\t0";
            else if (boundCondType == 3)
                // выход докритический режим
                BE += Eta.ToString(format) + "\t0\t0\t0\t0\t0\t0\t3\t0\t0";
            else if (boundCondType == 0 || boundCondType == 4 || boundCondType == 5)
                BE += "0\t0\t0\t0\t0\t0\t0\t0\t0\t0";
            return BE;
        }
        /// <summary>
        /// Вывод полей в строку для граничного элемента
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public string ToStringBED(string format = "F6")
        {
            string BE = (ID + 1).ToString() + "\t" + 
                        (Vertex1 + 1).ToString() + "\t" +
                        (Vertex2 + 1).ToString() + "\t0\t0";
            return BE;
        }
        /// <summary>
        /// Сортировка граничных элементов по возрастанию индекса
        /// </summary>
        /// <param name="belems"></param>
        public static void Sort(ref BoundElementRiver[] belems, int CountKnots)
        {
            try
            {
                BoundElementRiver[] tmpA = new BoundElementRiver[CountKnots];
                BoundElementRiver[] tmpB = new BoundElementRiver[belems.Length];
                for (int i = 0; i < belems.Length; i++)
                {
                    BoundElementRiver be = belems[i];
                    tmpA[be.Vertex1] = be;
                }
                tmpB[0] = tmpA[0];
                tmpB[0].ID = 0;
                for (int i = 1; i < belems.Length; i++)
                {
                    tmpB[i] = tmpA[tmpB[i - 1].Vertex2];
                    tmpB[i].ID = i;
                }
                belems = tmpB;
            }
            catch(Exception ex)
            {
                string s = ex.Message;
            }
        }
    }
}
