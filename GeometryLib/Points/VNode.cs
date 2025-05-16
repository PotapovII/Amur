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
//    public class VNode : HKnot
//    {
//        /// <summary>
//        /// номер узла
//        /// </summary>
//        public int ID { get => number; set => number = value; }
//        /// <summary>
//        /// флаг группы
//        /// </summary>
//        public int Flag { get => marker; set => marker = value; }
//        /// <summary>
//        /// 
//        /// </summary>
//        public double Value { get => value; set => this.value = value; }

//        public double value = 0;
//        public int number = 0;
//        public VNode() : base() { number = 0; }
//        public VNode(double xx, double yy, double value, int marker, int number)
//            : base(xx, yy, marker)
//        {
//            this.value = value;
//            this.number = number;
//        }
//        public VNode(VNode k) : base(k) 
//        { 
//            number = k.number;
//            this.value = k.value;
//            this.number = k.number;
//        }

//        public static new VNode Parse(string str)
//        {
//            string[] ls = (str.Trim()).Split(' ');
//            VNode knot = new VNode(double.Parse(ls[0], MEM.formatter), double.Parse(ls[1], MEM.formatter),
//                                    double.Parse(ls[2], MEM.formatter), int.Parse(ls[3]), int.Parse(ls[4]));
//            return knot;
//        }
//        public override HPoint Clone()
//        {
//            return new VNode(this);
//        }
//        /// <summary>
//        /// Создает копию объекта
//        /// </summary>
//        /// <returns></returns>
//        public override IHPoint IClone() => new VNode(this);
//    }
//}
