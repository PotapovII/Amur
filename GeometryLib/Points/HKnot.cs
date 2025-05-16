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
        /// <summary>
        /// Маркер точки, заначение по контексту
        /// </summary>
        public int marker = 0;
        /// <summary>
        /// дополнительный маркер точки, заначение по контексту
        /// </summary>
        public int typeEx = 0;
        public HKnot() : base() { marker = 0; typeEx = 0; }
        public HKnot(IHPoint p, int marker = 0, int typeEx = 0) : base(p.X, p.Y)
        {
            this.marker = marker;
            this.typeEx = typeEx;
        }

        public HKnot(double xx, double yy, int marker = 0, int typeEx = 0) : base(xx, yy)
        {
            this.marker = marker;
            this.typeEx = typeEx;
        }
        public HKnot(HKnot k) : base(k) 
        { 
            marker = k.marker;
            typeEx = k.typeEx;
        }
        public HKnot(HPoint p, int marker = 0, int typeEx = 0) : base(p) 
        { 
            this.marker = marker;
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
