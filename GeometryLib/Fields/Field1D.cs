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

    [Serializable]
    public class Field1D : Field<double>
    {
        public Field1D(string Name, int Count) : base(Name, Count) { }
        public Field1D(string Name, double[] V) : base(Name, V) { }
        public Field1D(Field1D p) : base(p) { }
        
        public override int Dimention => 1;
    }
}
