namespace CommonLib.IO
{
    using System.IO;
    using System.Collections.Generic;

    using MemLogLib;
    using CommonLib.Geometry;

    /// <summary>
    /// Хранение титула формата CRF необходимого для редактора формата CRF 
    /// </summary>
    public class CRF_Title
    {
        /// <summary>
        /// индекс гидропоста
        /// </summary>
        public int placeID = 1;
        /// <summary>
        /// индекс русла
        /// </summary>
        public int idx = 0;
        /// <summary>
        /// Дата начала расчета
        /// </summary>
        //public DateTime start;
        ///// <summary>
        ///// флаг
        ///// </summary>
        //public int endFlag = 0;
        ///// <summary>
        ///// Дата завершения расчета
        ///// </summary>
        //public DateTime end;
        ///// <summary>
        ///// индекс генератора сетки
        ///// </summary>
        //public int idxMesh = 0;

        public double[] s = null;
        public double[] Depth = null;
        public double[] Vn = null;
        public double[] Vt = null;
        /// <summary>
        /// бронь руслав
        /// </summary>
        public List<(double s0, double s1, double ks)> values = new List<(double, double, double)>();
        public CRF_Title(){}
        public CRF_Title(int placeID, int idx)//, int endFlag, int idxMesh, DateTime start, DateTime end)
        {
            SetTitle(placeID, idx);//, endFlag, idxMesh, start, end);
        }
        public void SetTitle(int placeID, int idx)//, int endFlag, int idxMesh, DateTime start, DateTime end)
        {
            this.placeID = placeID;
            this.idx = idx;
            //this.endFlag = endFlag;
            //this.idxMesh = idxMesh;
            //this.start = start;
            //this.end = end;
        }
        //public CRF_Title(int placeID, int idx, int endFlag, int idxMesh, DateTime start, DateTime end,
        //        double[] s, double[] Depth, double[] Vn, double[] Vt): this(
        //            placeID, idx)//, endFlag, idxMesh, start, end)
        //{
        //    Set(s, Depth, Vn, Vt);
        //}
        public void Set(double[] s, double[] Depth, double[] Vn, double[] Vt)
        {
            this.s = s;
            this.Depth = Depth;
            this.Vn = Vn;
            this.Vt = Vt;
        }

        public void AddKs(double s0, double s1, double ks)
        {
            values.Add((s0, s1, ks));
        }
        public void Clrar() => values.Clear();

        public void Load(StreamReader file)
        {
            placeID = LOG.GetInt(file.ReadLine());
            idx = LOG.GetInt(file.ReadLine());
            //idxMesh = LOG.GetInt(file.ReadLine());
            //endFlag = LOG.GetInt(file.ReadLine());
            //start = LOG.GetDateTime(file.ReadLine());
            //end = LOG.GetDateTime(file.ReadLine());
            int Count = LOG.GetInt(file.ReadLine());
            for (int i=0; i<Count;i++)
            {
                string line = file.ReadLine();
                if (line.Trim() == "#")
                    break;
                string[] sline = line.Split('\"', ' ', '\'');
                double s0 = double.Parse(sline[0].Trim(), MEM.formatter);
                double s1 = double.Parse(sline[1].Trim(), MEM.formatter);
                double ks = double.Parse(sline[2].Trim(), MEM.formatter);
                values.Add((s0, s1, ks));
            }
            LOG.LoadMasLine(file,ref s);
            LOG.LoadMasLine(file, ref Depth);
            LOG.LoadMasLine(file, ref Vn);
            LOG.LoadMasLine(file, ref Vt);
        }
        public void Save(StreamWriter file)
        {
            file.WriteLine("placeID {0}     : индекс гидропоста", placeID);
            file.WriteLine("LocRiver {0}    : индекс русла", idx);
            //file.WriteLine("IndexGMesh {0}    : индекс генератора сетки", idxMesh);
            //file.WriteLine("endFlag {0}    : индекс генератора сетки", endFlag);
            //file.WriteLine("StartData {0}    : индекс генератора сетки", start);
            //file.WriteLine("EndData {0}    : индекс генератора сетки", end);
            file.WriteLine("CountKs {0}    : количество учаткаов брони", values.Count);
            for (int i = 0; i < values.Count; i++)
                file.WriteLine("{0} {1} {2}", values[i].s0, values[i].s1, values[i].ks);
            LOG.SaveMasLine(file, s, 10);
            LOG.SaveMasLine(file, Depth, 6);
            LOG.SaveMasLine(file, Vn, 4);
            LOG.SaveMasLine(file, Vt, 4);
        }
    }
}
