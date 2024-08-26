//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                      создание иерархии полей
//                 кодировка : 13.03.2022 Потапов И.И.
//---------------------------------------------------------------------------

namespace GeometryLib
{
    using System;
    using GeometryLib.Vector;
    using MemLogLib;
    [Serializable]
    public class Field2D : Field<Vector2>
    {
        public Field2D(string Name, int Count) : base(Name, Count) { }
        public Field2D(string Name, Vector2[] V) : base(Name, V) { }
        public Field2D(Field2D p) : base(p) { }
        public Field2D(string Name, double[] Vx, double[] Vy) : base(Name)
        {
            MEM.Alloc<Vector2>(Vx.Length, ref Values);
            for (int i = 0; i < Values.Length; i++)
                Values[i] = new Vector2((float)Vx[i], (float)Vy[i]);
        }
        public void GetValue(ref double[] X, ref double[] Y)
        {
            MEM.Alloc(Values.Length, ref X);
            MEM.Alloc(Values.Length, ref Y);
            for (int i = 0; i < Values.Length; i++)
            {
                X[i] = Values[i].X;
                Y[i] = Values[i].Y;
            }
        }
        public override int Dimention => 2;
    }
}
