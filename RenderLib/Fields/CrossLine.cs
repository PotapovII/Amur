//namespace RenderLib.Fields
//{
//    using CommonLib.Geometry;
//    /// <summary>
//    /// Класс описывающий речной створ
//    /// </summary>
//    public class CrossLine : IHLine 
//    {
//        /// <summary>
//        /// Начало линии
//        /// </summary>
//        public IHPoint A
//        {
//            get => new HPoint(xa, ya);
//            set { xa = value.X; ya = value.Y; }
//        }
//        /// <summary>
//        /// Конец линии
//        /// </summary>
//        public IHPoint B 
//        { 
//            get => new HPoint(xb, yb);
//            set { xb = value.X; yb = value.Y; }
//        }
//        public string Name;
//        /// <summary>
//        /// координаты створа
//        /// </summary>
//        double xa;
//        double ya;
//        double xb;
//        double yb;
        
//        public CrossLine(string Name, double xa, double ya, double xb, double yb)
//        {
//            this.Name = Name;
//            this.xa = xa;
//            this.ya = ya;
//            this.xb = xb;
//            this.yb = yb;
//        }
//        public CrossLine(IHPoint a, IHPoint b)
//        {
//            A = new HPoint(a);
//            B = new HPoint(b);
//        }
//        /// <summary>
//        /// Конвертация в строку
//        /// </summary>
//        /// <returns></returns>
//        public override string ToString()
//        {
//            return Name + ":" + xa.ToString() + ":" + ya.ToString() + ":" + xb.ToString() + ":" + yb.ToString();
//        }
//        /// <summary>
//        /// Конвертация из строки / без защиты от д...
//        /// </summary>
//        /// <param name="line"></param>
//        /// <returns></returns>
//        public static CrossLine Parse(string line)
//        {
//            string[] lines = line.Split(':');
//            CrossLine cl = new CrossLine(
//            lines[0],
//            double.Parse(lines[1]),
//            double.Parse(lines[2]),
//            double.Parse(lines[3]),
//            double.Parse(lines[4]));
//            return cl;
//        }
//    }

//}
