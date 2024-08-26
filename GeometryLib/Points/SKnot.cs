//---------------------------------------------------------------------------
//                          ПРОЕКТ  "H?"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                       11.06.2024 Потапов И.И. 
//---------------------------------------------------------------------------
namespace GeometryLib
{
    using System;
    using MemLogLib;
    using CommonLib.Geometry;
    /// <summary>
    /// Расширение точки 2 ее идентификатором
    /// </summary>
    [Serializable]
    public class SKnot : HPoint
    {
        /// <summary>
        /// Номер точки
        /// </summary>
        public int ID;
        public SKnot() : base() { ID = 0; }
        public SKnot(double xx, double yy, int ID = 0) : base(xx, yy)
        {
            this.ID = ID;
        }
        public SKnot(SKnot k) : base(k) 
        { 
            ID = k.ID;
        }
        public SKnot(IHPoint p, int ID = 0) : base(p) 
        { 
            this.ID = ID;
        }
        public static new SKnot Parse(string str)
        {
            string[] ls = (str.Trim()).Split(' ');
            SKnot knot = new SKnot(double.Parse(ls[0], MEM.formatter),
                                   double.Parse(ls[1], MEM.formatter), 
                                   int.Parse(ls[2]));
            return knot;
        }
        public override HPoint Clone()
        {
            return new SKnot(this);
        }
        /// <summary>
        /// Создает копию объекта
        /// </summary>
        /// <returns></returns>
        public override IHPoint IClone() => new SKnot(this);
    }
}
