//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 17.06.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace GeometryLib
{
    using CommonLib.Function;
    using CommonLib.Geometry;
    using MemLogLib;
    using System;
    using System.IO;
    /// <summary>
    /// ОО: Класс - для загрузки начальной геометрии русла в створе реки
    /// </summary>
    [Serializable]
    public class DigFunction : AbDigFunction
    {
        /// <summary>
        /// загрузка начальной геометрии из файла
        /// </summary>
        /// <param name="file"></param>
        public override void Load(StreamReader file)
        {
            x0.Clear();
            y0.Clear();
            string line = file.ReadLine();
            name = line;
            for (line = file.ReadLine(); line != null; line = file.ReadLine())
            {
                if (line.Trim() == "#")
                    break;
                string[] sline = line.Split(' ');
                double x = double.Parse(sline[0].Trim(), MEM.formatter);
                double y = double.Parse(sline[1].Trim(), MEM.formatter);
                x0.Add(x);
                y0.Add(y);
            }
        }
        /// <summary>
        /// Сохранение данных в файл
        /// </summary>
        /// <param name="file"></param>
        public override void Save(StreamWriter file) 
        {
            file.WriteLine(name);
            for (int i=0; i<x0.Count; i++)
                file.WriteLine("{0} {1}", x0[i], y0[i]);
            file.WriteLine("#");
        }

        public DigFunction(string name = "функция по точкам")
        : base(SmoothnessFunction.linear, false)
        { }

        public DigFunction(string name, SmoothnessFunction sn, bool parametricOk)
                :base(sn, parametricOk)
        {
            if (name == "")
                name = "функция по точкам";
        }

        public DigFunction(double[] x, double[] y, string name = "")
                : base(SmoothnessFunction.linear, false)
        {
            if (name == "")
                name = "функция по точкам";
            SetFunctionData(x, y, name);
        }

        public DigFunction(DigFunctionPolynom p) : base(p.Smoothness, false)
        {
            name = p.Name;
            x0.Clear();
            y0.Clear();
            x0.AddRange(p.x0.ToArray());
            y0.AddRange(p.y0.ToArray());
        }

        public DigFunction(HPoint[] p, string name = "")
         : base(SmoothnessFunction.linear, false)
        {
            this.name = name;
            x0.Clear();
            y0.Clear();
            for (int i = 0; i < p.Length; i++)
            {
                x0.Add(p[i].X);
                y0.Add(p[i].Y);
            }
        }


        public DigFunction(double L, string name = "") : this(L, 0, name) {}
        
        public DigFunction(double L,double V, string name = "")
        {
            if (name == "")
                name = "функция по точкам";
            Add(0, V);
            Add(L, V);
        }
    }
}