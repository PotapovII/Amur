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
    using System.Runtime.CompilerServices;
    using System.Diagnostics;
    using CommonLib.Mesh;

    /// <summary>
    /// Расширение точки 2D ее идентификатором
    /// </summary>
    [Serializable]
    [DebuggerDisplay("ID {ID} [{X}, {Y}]")]
    public class SKnot : HPoint
    {
        public static Direction direction = Direction.toRight;
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
        #region IComparable
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new int CompareTo(HPoint other)
        {
            switch (direction)
            {
                case Direction.toRight:
                    if (1e10 * (x + 1) > y && 1e10 * (other.x + 1) > other.y)
                    {
                        double a = 1e10 * (x + 1) + 1e2 * (y + 1);
                        double b = 1e10 * (other.x + 1) + 1e2 * (other.y + 1);
                        return a.CompareTo(b);
                    }
                    else
                        return x.CompareTo(other.x);
                case Direction.toLeft:
                    if (1e10 * (x + 1) > y && 1e10 * (other.x + 1) > other.y)
                    {
                        double a = 1e10 * (x + 1) + 1e2 * (y + 1);
                        double b = 1e10 * (other.x + 1) + 1e2 * (other.y + 1);
                        return b.CompareTo(a);
                    }
                    else
                        return x.CompareTo(other.x);
                case Direction.toUp:
                    if (1e10 * (y + 1) > x && 1e10 * (other.y + 1) > other.x)
                    {
                        double a = 1e10 * (y + 1) + 1e2 * (x + 1);
                        double b = 1e10 * (other.y + 1) + 1e2 * (other.x + 1);
                        return a.CompareTo(b);
                    }
                    else
                        return x.CompareTo(other.x);
                case Direction.toDown:
                    if (1e10 * (x + 1) > y && 1e10 * (other.x + 1) > other.y)
                    {
                        double a = 1e10 * (y + 1) + 1e2 * (x + 1);
                        double b = 1e10 * (other.y + 1) + 1e2 * (other.x + 1);
                        return b.CompareTo(a);
                    }
                    else
                        return x.CompareTo(other.x);
            }
            return x.CompareTo(other.x);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int CompareTo(object obj)
        {
            HPoint other = obj as HPoint;
            return CompareTo(other);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new int GetHashCode()
        {
            int hash = 19;
            hash = hash * 31 + x.GetHashCode();
            hash = hash * 31 + y.GetHashCode();

            return hash;
        }
        #endregion

        /// <summary>
        /// Создает копию объекта
        /// </summary>
        /// <returns></returns>
        public override IHPoint IClone() => new SKnot(this);
    }
}
