
//namespace MeshLib.Wrappers
//{
//    using CommonLib;
//    using CommonLib.Tasks;
//    using MemLogLib;
//    using System;

//    [Serializable]
//    public class BCValues : IBCValues
//    {
//        public double[] Values { get => values; } 
//        protected double[] values = new double[1];
//        public BCValues(double[] values)
//        {
//            this.values = values;
//        }
//        public BCValues(double value)
//        {
//            values[0] = value;
//        }
//        public BCValues(IBCValues v)
//        {
//            values = MEM.Copy(values, v.Values);
//        }
//        public void Mult(double scal)
//        {
//            for(int i=0; i<values.Length; i++)
//                values[i] *= scal;
//        }
//        public void Sum(IBCValues bv)
//        {
//            for (int i = 0; i < values.Length; i++)
//                values[i] += bv.Values[i];
//        }
//    }
//}
