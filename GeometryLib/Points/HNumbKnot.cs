using CommonLib.Geometry;
using System;
//---------------------------------------------------------------------------
//                          ПРОЕКТ  "H?"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                кодировка : 2.10.2000 Потапов И.И. (c++)
//---------------------------------------------------------------------------
//                          ПРОЕКТ  "МКЭ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//           кодировка : 25.12.2020 Потапов И.И. (c++=> c#)
//---------------------------------------------------------------------------
namespace GeometryLib
{
    [Serializable]
    public class HNumbKnot : HKnot
    {
        public int number = 0;
        public HNumbKnot() : base() { number = 0; }
        public HNumbKnot(double xx, double yy, int type, int number)
            : base(xx, yy, type)
        {
            this.number = number;
        }
        public HNumbKnot(HNumbKnot k) : base(k) { number = k.number; }

        public static new HNumbKnot Parse(string str)
        {
            string[] ls = (str.Trim()).Split(' ');
            HNumbKnot knot = new HNumbKnot(double.Parse(ls[0]), double.Parse(ls[1]), int.Parse(ls[2]), int.Parse(ls[3]));
            return knot;
        }
        public override HPoint Clone()
        {
            return new HNumbKnot(this);
        }
        /// <summary>
        /// Создает копию объекта
        /// </summary>
        /// <returns></returns>
        public override IHPoint IClone() => new HNumbKnot(this);

    }
}
