////---------------------------------------------------------------------------
////                         проектировщик:
////                           Потапов И.И.
////---------------------------------------------------------------------------
////              кодировка : 02.07.2022 Потапов И.И. 
////---------------------------------------------------------------------------
//namespace GeometryLib
//{
//    using System;
//    using CommonLib.Geometry;
//    using MemLogLib;
//    [Serializable]
//    public class VKnot : HPoint
//    {
//        /// <summary>
//        ///  Флаг
//        /// </summary>
//        public int marker = 0;
//        /// <summary>
//        /// Значение в узле
//        /// </summary>
//        public double value = 0;
//        public VKnot() : base() { marker = 0; }
//        public VKnot(double xx, double yy, double value, int marker = 0) : base(xx, yy)
//        {
//            this.marker = marker;
//            this.value = value;
//        }
//        public VKnot(VKnot k) : base(k)
//        {
//            marker = k.marker;
//            this.value = k.value;
//        }
//        public VKnot(HPoint p, double value, int marker = 0) : base(p)
//        {
//            this.marker = marker;
//            this.value = value;
//        }
//        public static new VKnot Parse(string str)
//        {
//            string[] ls = (str.Trim()).Split(' ');
//            VKnot VKnot = new VKnot(double.Parse(ls[0], MEM.formatter), double.Parse(ls[1], MEM.formatter),
//                                    double.Parse(ls[2], MEM.formatter), int.Parse(ls[3]));
//            return VKnot;
//        }
//        public override HPoint Clone()
//        {
//            return new VKnot(this);
//        }
//        /// <summary>
//        /// Создает копию объекта
//        /// </summary>
//        /// <returns></returns>
//        public override IHPoint IClone() => new VKnot(this);
//    }
//}
