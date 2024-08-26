//---------------------------------------------------------------------------
//                         ПРОЕКТ  "RiverDB"
//                          проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 20.10.2023 Потапов И.И. 
//---------------------------------------------------------------------------
namespace GeometryLib
{
    using System;
    using CommonLib.Geometry;
    using GeometryLib.Vector;
    using MemLogLib;
    /// <summary>
    /// Узел для облака данных
    /// </summary>
    [Serializable]
    public class CloudKnot : HPoint
    {
        /// <summary>
        /// Свойства в узлах (состояние контексное) от задачи
        /// </summary>
        public double[] Attributes = null;
        /// <summary>
        /// Маркер узла (состояние контексное) от задачи
        /// </summary>
        public int mark;

        public CloudKnot() : base() { Attributes = new double[1]; mark = 0; }
        public CloudKnot(double xx, double yy, double[] atr, int mark = 0)
            : base(xx, yy)
        {
            this.mark = mark;
            MEM.MemCopy(ref Attributes, atr);
        }
        public CloudKnot(HPoint coord, HPoint atrib, int mark = 0)
            : base(coord)
        {
            this.mark = mark;
            Attributes = new double[] { atrib.x, atrib.y };
        }
        public CloudKnot(HPoint coord, double[] Atrs, int mark = 0)
            : base(coord)
        {
            this.mark = mark;
            MEM.MemCopy(ref Attributes, Atrs);
        }
        public CloudKnot(Vector2 coord, Vector2 atrib, int mark = 0)
            : base(coord.X, coord.Y)
        {
            this.mark = mark;
            Attributes = new double[] { atrib.X, atrib.Y };
        }
        public CloudKnot(CloudKnot p) : base(p)
        {
            this.mark = p.mark;
            MEM.MemCopy(ref Attributes, p.Attributes);
        }

        public override string ToString(string Filter = "F6")
        {
            string s = " " + x.ToString("F15") +
                       " " + y.ToString("F15") +
                      " " + mark.ToString();
            for (int j = 0; j < Attributes.Length; j++)
                s += " " + Attributes[j].ToString(Filter);
            return s;
        }
        public override string ToString()
        {
            string s = " " + x.ToString("F15") +
                       " " + y.ToString("F15") +
                      " " + mark.ToString();
            for (int j = 0; j < Attributes.Length; j++)
                s += " " + Attributes[j].ToString("F4");
            return s;
        }

        public new static CloudKnot Parse(string line)
        {
            string[] mas = (line.Trim()).Split(' ');
            double xx = double.Parse(mas[0], MEM.formatter);
            double yy = double.Parse(mas[1], MEM.formatter);
            int _mark = int.Parse(mas[2]);
            int count = mas.Length - 3;
            count = count <= 0 ? 1 : count;
            double[] v = new double[count];
            for (int j = 3; j < mas.Length; j++)
                v[j - 3] = double.Parse(mas[j], MEM.formatter);
            CloudKnot p = new CloudKnot(xx, yy, v, _mark);
            return p;
        }

        /// <summary>
        /// Ключ для словарей
        /// </summary>
        /// <returns></returns>
        public string GetHash()
        {
            return x.ToString(WR.format5) + y.ToString(WR.format5);
        }
        public override HPoint Clone()
        {
            return new CloudKnot(this);
        }
        /// <summary>
        /// Создает копию объекта
        /// </summary>
        /// <returns></returns>
        public override IHPoint IClone() => new CloudKnot(this);

        /// <summary>
        /// Интерполирует CloudKnot точку между точками A и B
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="s">параметр интерполяции изменяется от 0 до 1</param>
        /// <returns></returns>
        public static CloudKnot Interpolation(CloudKnot A, CloudKnot B, double s, int mark = 0)
        {
            double N1 = 1 - s;
            double N2 = s;
            double x = A.x * N1 + B.x * N2;
            double y = A.y * N1 + B.y * N2;
            CloudKnot V = new CloudKnot(x, y, new double[A.Attributes.Length], mark);
            for (int k = 0; k < A.Attributes.Length; k++)
                V.Attributes[k] = A.Attributes[k] * N1 + B.Attributes[k] * N2;
            return V;
        }
    }
}
