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
    using System;
    using MemLogLib;
    using CommonLib.Geometry;
    /// <summary>
    /// Расширение точки 2 флагами (маркер и тип маркеа)
    /// </summary>
    [Serializable]
    public class HKnot : HPoint
    {
        public int type = 0;
        public int typeEx = 0;
        public HKnot() : base() { type = 0; typeEx = 0; }
        public HKnot(double xx, double yy, int type = 0, int typeEx = 0) : base(xx, yy)
        {
            this.type = type;
            this.typeEx = typeEx;
        }
        public HKnot(HKnot k) : base(k) 
        { 
            type = k.type;
            typeEx = k.typeEx;
        }
        public HKnot(HPoint p, int type = 0, int typeEx = 0) : base(p) 
        { 
            this.type = type;
            this.typeEx = typeEx;
        }
        public static new HKnot Parse(string str)
        {
            string[] ls = (str.Trim()).Split(' ');
            HKnot knot = new HKnot(double.Parse(ls[0], MEM.formatter),
                                   double.Parse(ls[1], MEM.formatter), 
                                   int.Parse(ls[2]), int.Parse(ls[3]));
            return knot;
        }
        public override HPoint Clone()
        {
            return new HKnot(this);
        }
        /// <summary>
        /// Создает копию объекта
        /// </summary>
        /// <returns></returns>
        public override IHPoint IClone() => new HKnot(this);
    }
}
