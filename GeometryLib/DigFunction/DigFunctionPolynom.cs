//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 09.03.2025 Потапов И.И.
//---------------------------------------------------------------------------
namespace GeometryLib
{
    using System;
    using System.IO;

    using MemLogLib;
    using CommonLib.Function;
    using CommonLib.Geometry;

    /// <summary>
    /// ОО: Класс - для загрузки начальной геометрии русла в створе реки
    /// </summary>
    [Serializable]
    public class DigFunctionPolynom : AbDigFunction
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
            switch (x0.Count)
            {
                case 2:
                    Smoothness = SmoothnessFunction.polynom1;
                    break;
                case 3:
                    Smoothness = SmoothnessFunction.polynom2;
                    break;
                case 4:
                    Smoothness = SmoothnessFunction.polynom3;
                    break;
                default:
                    throw new NotImplementedException();
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

        public DigFunctionPolynom(IHPoint a, IHPoint b)
            : this(a.X, a.Y, b.X, b.Y) { }

        public DigFunctionPolynom(IHPoint a, IHPoint b, IHPoint c)
            : this(a.X, a.Y, b.X, b.Y, c.X, c.Y) { }

        public DigFunctionPolynom(IHPoint a, IHPoint b, IHPoint c, IHPoint d)
            : this(a.X, a.Y, b.X, b.Y, c.X, c.Y, d.X, d.Y) { }

        public DigFunctionPolynom(double xa, double ya, double xb, double yb)
        : base(SmoothnessFunction.polynom1, false)
        {
            x0.Add(xa);
            y0.Add(ya);
            x0.Add(xb);
            y0.Add(yb);
        }
        public DigFunctionPolynom(double xa, double ya, double xb, double yb, double xc, double yc)
        : base(SmoothnessFunction.polynom2, false)
        {
            x0.Add(xa);
            y0.Add(ya);
            x0.Add(xb);
            y0.Add(yb);
            x0.Add(xc);
            y0.Add(yc);
        }
        public DigFunctionPolynom(double xa, double ya, double xb, double yb, double xc, double yc, double xd, double yd)
        : base(SmoothnessFunction.polynom3, false)
        {
            x0.Add(xa);
            y0.Add(ya);
            x0.Add(xb);
            y0.Add(yb);
            x0.Add(xc);
            y0.Add(yc);
            x0.Add(xd);
            y0.Add(yd);
        }
        public DigFunctionPolynom(DigFunctionPolynom p)
        : base(p.Smoothness, false)
        {
            name = p.Name;
            x0.Clear(); 
            y0.Clear();
            x0.AddRange(p.x0.ToArray());
            y0.AddRange(p.y0.ToArray());
        }
        public DigFunctionPolynom(double[] x, double[] y, string name = "")
                : base(SmoothnessFunction.linear, false)
        {
            switch (x.Length)
            {
                case 2:
                    Smoothness = SmoothnessFunction.polynom1;
                    SetFunctionData(x, y, "линия");
                    break;
                case 3:
                    Smoothness = SmoothnessFunction.polynom2;
                    SetFunctionData(x, y, "парабола");
                    break;
                case 4:
                    Smoothness = SmoothnessFunction.polynom3;
                    SetFunctionData(x, y, "кубическая парабола");
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        public DigFunctionPolynom(IHPoint[] ps, string name = "")
                : base(SmoothnessFunction.linear, false)
        {
            switch (ps.Length)
            {
                case 2:
                    Smoothness = SmoothnessFunction.polynom1;
                    break;
                case 3:
                    Smoothness = SmoothnessFunction.polynom2;
                    break;
                case 4:
                    Smoothness = SmoothnessFunction.polynom3;
                    break;
                default:
                    throw new NotImplementedException();
            }
            Clear();
            for (int i = 0; i < ps.Length; i++)
                Add(ps[i].X, ps[i].Y);
        }
        /// <summary>
        /// Получение не сглаженного значения данных по линейной интерполяции
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public override double FunctionValue(double xx)
        {
            switch (Smoothness)
            {
                case SmoothnessFunction.polynom1:
                    {
                        double L1 = (xx - x0[0]) / (x0[1] - x0[0]);
                        double L0 = 1 - L1;
                        return L0 * y0[0] + L1 * y0[1];
                    }
                case SmoothnessFunction.polynom2:
                    {
                        double L0 = (xx - x0[1]) * (xx - x0[2]) / ((x0[0] - x0[1]) * (x0[0] - x0[2]));
                        double L1 = (xx - x0[0]) * (xx - x0[2]) / ((x0[1] - x0[0]) * (x0[1] - x0[2]));
                        double L2 = (xx - x0[0]) * (xx - x0[1]) / ((x0[2] - x0[0]) * (x0[2] - x0[1]));
                        return L0 * y0[0] + L1 * y0[1] + L2 * y0[2];
                    }
                case SmoothnessFunction.polynom3:
                    {
                        double L0 = (xx - x0[1]) * (xx - x0[2]) * (xx - x0[3]) / ((x0[0] - x0[1]) * (x0[0] - x0[2]) * (x0[0] - x0[3]));
                        double L1 = (xx - x0[0]) * (xx - x0[2]) * (xx - x0[3]) / ((x0[1] - x0[0]) * (x0[1] - x0[2]) * (x0[1] - x0[3]));
                        double L2 = (xx - x0[0]) * (xx - x0[1]) * (xx - x0[3]) / ((x0[2] - x0[0]) * (x0[2] - x0[1]) * (x0[2] - x0[3]));
                        double L3 = (xx - x0[0]) * (xx - x0[1]) * (xx - x0[2]) / ((x0[3] - x0[0]) * (x0[3] - x0[1]) * (x0[3] - x0[2]));
                        return L0 * y0[0] + L1 * y0[1] + L2 * y0[2] + L3 * y0[3];
                    }
                default:
                    throw new NotImplementedException();
            }
        }
        /// <summary>
        /// Получаем начальную геометрию русла для заданной дискретизации дна
        /// </summary>
        /// <param name="x">координаты Х</param>
        /// <param name="y">координаты У</param>
        /// <param name="Count">количество узлов для русла</param>
        public override void GetFunctionData(ref double[] x, ref double[] y,
                                         int Count = 10, bool revers = false)
        {
            MEM.Alloc<double>(Count, ref x);
            MEM.Alloc<double>(Count, ref y);
            double L = DMath.LengthCurve(x0.ToArray(), y0.ToArray());
            double ds = L / (Count - 1);
            double L0, L1, L2, L3;
            double nF0, nF1, nF2, nF3;
            switch (Smoothness)
            {
                case SmoothnessFunction.polynom1:
                    {
                        nF0 = x0[1] - x0[0];
                        for (int k = 0; k < Count; k++)
                        {
                            double xx = k * ds;
                            L1 = (xx - x0[0]) / nF0;
                            L0 = 1 - L1;
                            x[k] = xx;
                            y[k] = L0 * y0[0] + L1 * y0[1];
                        }
                    }
                    break;
                case SmoothnessFunction.polynom2:
                    {
                        nF0 = (x0[0] - x0[1]) * (x0[0] - x0[2]);
                        nF1 = (x0[1] - x0[0]) * (x0[1] - x0[2]);
                        nF2 = (x0[2] - x0[0]) * (x0[2] - x0[1]);
                        for (int k = 0; k < Count; k++)
                        {
                            double xx = k * ds;
                            x[k] = xx;
                            L0 = (xx - x0[1]) * (xx - x0[2]) / nF0;
                            L1 = (xx - x0[0]) * (xx - x0[2]) / nF1;
                            L2 = (xx - x0[0]) * (xx - x0[1]) / nF2;
                            y[k] = L0 * y0[0] +L1 * y0[1] + L2 * y0[2];
                        }
                    }
                    break;
                case SmoothnessFunction.polynom3:
                    {
                        nF0 = (x0[0] - x0[1]) * (x0[0] - x0[2]) * (x0[0] - x0[3]);
                        nF1 = (x0[1] - x0[0]) * (x0[1] - x0[2]) * (x0[1] - x0[3]);
                        nF2 = (x0[2] - x0[0]) * (x0[2] - x0[1]) * (x0[2] - x0[3]);
                        nF3 = (x0[3] - x0[0]) * (x0[3] - x0[1]) * (x0[3] - x0[2]);
                        for (int k = 0; k < Count; k++)
                        {
                            double xx = k * ds;
                            x[k] = xx;
                            L0 = (xx - x0[1]) * (xx - x0[2]) * (xx - x0[3]) / nF0;
                            L1 = (xx - x0[0]) * (xx - x0[2]) * (xx - x0[3]) / nF1;
                            L2 = (xx - x0[0]) * (xx - x0[1]) * (xx - x0[3]) / nF2;
                            L3 = (xx - x0[0]) * (xx - x0[1]) * (xx - x0[2]) / nF3;
                            y[k] = L0 * y0[0] + L1 * y0[1] + L2 * y0[2] + L3 * y0[3];
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
            if (revers == true)
                MEM.Reverse(ref y);
        }
    }
}