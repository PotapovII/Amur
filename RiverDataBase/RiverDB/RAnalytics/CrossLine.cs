//---------------------------------------------------------------------------
//                   ПРОЕКТ  "Русловые процессы"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 13.08.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace RiverDB.RAnalytics
{
    using System.Collections.Generic;
    /// <summary>
    /// Класс описывающий речной створ
    /// </summary>
    public class CrossLine
    {
        public string Name;
        /// <summary>
        /// координаты створа
        /// </summary>
        public double xa;
        public double ya;
        public double xb;
        public double yb;
        /// <summary>
        /// список точек описывающих дно
        /// </summary>
        public List<RiverPoint> points = new List<RiverPoint>();
        public CrossLine(string Name, double xa, double ya, double xb, double yb)
        {
            this.Name = Name;
            this.xa = xa;
            this.ya = ya;
            this.xb = xb;
            this.yb = yb;
        }
        public void Add(RiverPoint p)
        {
            points.Add(p);
        }
        public void Clear()
        {
            points.Clear();
        }
        /// <summary>
        /// Конвертация в строку
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name + ":" + xa.ToString() + ":" + ya.ToString() + ":" + xb.ToString() + ":" + yb.ToString();
        }
        /// <summary>
        /// Конвертация из строки / без защиты от д...
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static CrossLine Parse(string line)
        {
            string[] lines = line.Split(':');
            CrossLine cl = new CrossLine(
            lines[0],
            double.Parse(lines[1]),
            double.Parse(lines[2]),
            double.Parse(lines[3]),
            double.Parse(lines[4]));
            return cl;
        }
    }

}
