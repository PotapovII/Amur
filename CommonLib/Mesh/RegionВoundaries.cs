////---------------------------------------------------------------------------
////                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
////                         проектировщик:
////                           Потапов И.И.
////---------------------------------------------------------------------------
////                 кодировка : 11.12.2020 Потапов И.И.
////---------------------------------------------------------------------------
//namespace MeshLib
//{
//    using MemLogLib;
//    using System;
//    using System.Drawing;
//    /// <summary>
//    /// ОО: Границы региона 
//    /// </summary>
//    [Serializable]
//    public struct RectangleWorld
//    {
//        public float Left;
//        public float Right;
//        public float Bottom;
//        public float Top;
//        /// <summary>
//        /// Ширина региона
//        /// </summary>
//        public float Width
//        {
//            get => this.Right - this.Left; 
//        }
//        /// <summary>
//        /// Высота региона
//        /// </summary>
//        public float Height
//        {
//            get => this.Top - this.Bottom; 
//        }
//        /// <summary>
//        /// Максимальный линейный размер региона
//        /// </summary>
//        public float MaxScale
//        {
//            get => Math.Max(Width, Height);
//        }
//        public RectangleWorld(PointF a, PointF b)
//        {
//            Left = Math.Min(a.X, b.X);
//            Right = Math.Max(a.X, b.X);
//            Bottom = Math.Min(a.Y, b.Y);
//            Top = Math.Max(a.Y, b.Y);
//        }
//        public RectangleWorld(float left=0, float right=0, float bottom=0, float top=0)
//        {
//            this.Left = left;
//            this.Right = right;
//            this.Bottom = bottom;
//            this.Top = top;
//        }
//        public RectangleWorld(double left, double right, double bottom, double top)
//        {
//            this.Left = (float)left;
//            this.Right = (float)right;
//            this.Bottom = (float)bottom;
//            this.Top = (float)top;
//        }

//        public void AddRegion(RectangleWorld b)
//        {
//            Left = Math.Min(Left, b.Left);
//            Right = Math.Max(Right, b.Right);
//            Bottom = Math.Min(Bottom, b.Bottom);
//            Top = Math.Max(Top, b.Top);
//        }

//        public void AddRegion(PointF p)
//        {
//            Left = Math.Min(Left, p.X);
//            Right = Math.Max(Right, p.X);
//            Bottom = Math.Min(Bottom, p.Y);
//            Top = Math.Max(Top, p.Y);
//        }

//        /// <summary>
//        /// Расширение региона при смешивании
//        /// </summary>
//        /// <param name="a"></param>
//        /// <param name="b"></param>
//        /// <returns></returns>
//        public static RectangleWorld Extension(ref RectangleWorld a, ref RectangleWorld b)
//        {
//            if (b.Left == b.Right && b.Bottom == b.Top)
//                return a;
//            else
//                return new RectangleWorld(
//                Math.Min(a.Left, b.Left),
//                Math.Max(a.Right, b.Right),
//                Math.Min(a.Bottom, b.Bottom),
//                Math.Max(a.Top, b.Top));
//        }
//        public void Center(ref PointF center)
//        {
//            center.X = (Left + Right) / 2;
//            center.Y = (Bottom + Top) / 2;
//        }

//        public static bool operator ==(RectangleWorld a, RectangleWorld b)
//        {
//            int merror = 4;
//            bool r = MEM.Equals(a.Left, b.Left, merror) &&
//                     MEM.Equals(a.Right, b.Right, merror) &&
//                     MEM.Equals(a.Top, b.Top, merror) &&
//                     MEM.Equals(a.Bottom, b.Bottom, merror);
//            return r;
//        }
//        public static bool operator !=(RectangleWorld a, RectangleWorld b)
//        {
//            if (a == b)
//                return false;
//            else
//                return true;
//        }

//    }
//}
