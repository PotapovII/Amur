//---------------------------------------------------------------------------
//                     ПРОЕКТ  "RiverSolver"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//               кодировка : 02.06.2021 Потапов И.И.
//---------------------------------------------------------------------------
//               кодировка : 27.12.2023 Потапов И.И.  оптимизация и читка
//---------------------------------------------------------------------------
namespace RiverLib.River2D.RiverMesh
{
    using CommonLib;
    using MemLogLib;
    using System;
    /// <summary>
    /// ОО: Класс речной конечный элемент
    /// </summary>
    [Serializable]
    public class TriElementRiver
    {
        /// <summary>
        /// Номер ГЭ
        /// </summary>
        public int ID;
        /// <summary>
        /// Узлы КЭ
        /// </summary>
        public uint Vertex1; 
        public uint Vertex2; 
        public uint Vertex3;
        /// <summary>
        /// Узлы КЭ
        /// </summary>
        public TriElement nodes { get { return new TriElement(Vertex1, Vertex2, Vertex3);  }  }
        /// <summary>
        /// Какая из локальных матриц 
        /// </summary>
        public double[,] matrices = new double[9, 9];
        /// <summary>
        /// правая часть
        /// </summary>
        public double[] FEn = new double[9];
        /// <summary>
        /// управляющие коды (флаги) 
        /// флаг для отметки точек интегрирования попадающих на берег
        /// </summary>
        public int[] gpcode = new int[9];

        public TriElementRiver()
        {
            ID = 0;
            matrices = new double[9, 9];
            FEn = new double[9];
            gpcode = new int[9];
        }
        public TriElementRiver(int ID, uint Vertex1, uint Vertex2, uint Vertex3)
        { 
            this.ID = ID;
            this.Vertex1 = Vertex1;
            this.Vertex2 = Vertex2;
            this.Vertex3 = Vertex3;
        }
        public TriElementRiver(TriElementRiver e)
        {
            ID = e.ID;
            Vertex1 = e.Vertex1;
            Vertex2 = e.Vertex2;
            Vertex3 = e.Vertex3;
            MEM.MemCopy<int>(ref gpcode, e.gpcode);
            MEM.MemCopy<double>(ref matrices, e.matrices);
            MEM.MemCopy<double>(ref FEn, e.FEn);
        }
        public static TriElementRiver Parse(string[] lines)
        {
            TriElementRiver element = new TriElementRiver();
            element.ID = int.Parse(lines[0].Trim());
            element.Vertex1 = uint.Parse(lines[1].Trim());
            element.Vertex2 = uint.Parse(lines[2].Trim());
            element.Vertex3 = uint.Parse(lines[3].Trim());
            return element;
        }
        public static TriElementRiver Parse(string line)
        {
            string[] lines = (line.Trim()).Split(' ');
            return Parse(lines);
        }
        public new string ToString()
        {
            string element = ID.ToString() + "\t" +
                Vertex1.ToString() + "\t" +
                Vertex2.ToString() + "\t" +
                Vertex3.ToString();
            return element;
        }
        public string ToStringCDG()
        {
            string element = (ID+1).ToString() + "\t" + "210\t210\t" +
                (Vertex1+1).ToString() + "\t" +
                (Vertex2+1).ToString() + "\t" +
                (Vertex3+1).ToString() + "\t0.0\t0.0\t0.0";
            return element;
        }
    }
}
