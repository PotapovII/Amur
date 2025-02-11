//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 10.02.2024 Потапов И.И.
//---------------------------------------------------------------------------
//                      Сбока No Property River Lib
//              с убранными из наследников Property классами 
//---------------------------------------------------------------------------
namespace GeometryLib
{
    using System;
    using CommonLib;
    using MemLogLib;

    /// <summary>
    /// Склалярная неизвестная
    /// </summary>
    [Serializable]
    public class AUnknown : IUnknown
    {
        ///// <summary>
        ///// Значения в узлах сетки
        ///// </summary>
        public double[] ValuesX { get; }
        ///// <summary>
        ///// Значения в узлах сетки
        ///// </summary>
        public double[] ValuesY { get; }
        ///// <summary>
        ///// Значения в узлах сетки
        ///// </summary>
        public int Dimention 
        { 
            get 
            { 
                if( ValuesX!=null && ValuesY!=null) 
                    return 2; 
                else    
                    return 1; 
            } 
        }
        /// <summary>
        /// Название неизвестных задачи
        /// </summary>
        public string NameUnknow { get; }
        /// <summary>
        /// Тип неизвестной задачи (меняется во времени или нет) - необходимо наальное условие или нет
        /// </summary>
        public bool TypeUnknow { get; }
        /// <summary>
        /// Апроксимация неизвестной задачи 
        /// </summary>
        public TypeFunForm ApproxUnknow { get; set; }

        public AUnknown(string nameUnknow,double [] mas, bool typeUnknow, TypeFunForm approxUnknow)
        {
            this.NameUnknow = nameUnknow;
            this.ValuesX = mas;
            this.ValuesY = null;
            TypeUnknow = typeUnknow;
            ApproxUnknow = approxUnknow;
        }
        public AUnknown(string nameUnknow, double[] masX, double[] masY, bool typeUnknow, TypeFunForm approxUnknow) :
            this(nameUnknow, masX, typeUnknow, approxUnknow)
        {
            this.ValuesY = masY;
        }
        public AUnknown(AUnknown p)
        {
            this.NameUnknow = p.NameUnknow;
            this.ValuesX = p.ValuesX;
            this.ValuesY = p.ValuesY;
            TypeUnknow = p.TypeUnknow;
            ApproxUnknow = p.ApproxUnknow;
        }
    }
    /// <summary>
    /// Матричная неизвестная
    /// </summary>
    [Serializable]
    public class AUnknown2D : IUnknown
    {
        ///// <summary>
        ///// Значения в узлах сетки
        ///// </summary>
        public double[] ValuesX => GetValue1D(masX);
        public double[][] masX;
        ///// <summary>
        ///// Значения в узлах сетки
        ///// </summary>
        public double[] ValuesY => GetValue1D(masY);
        public double[][] masY;
        ///// <summary>
        ///// Значения в узлах сетки
        ///// </summary>
        public int Dimention
        {
            get
            {
                if (ValuesX != null && ValuesY != null)
                    return 2;
                else
                    return 1;
            }
        }
        /// <summary>
        /// Название неизвестных задачи
        /// </summary>
        public string NameUnknow { get; }
        /// <summary>
        /// Тип неизвестной задачи (меняется во времени или нет) - необходимо наальное условие или нет
        /// </summary>
        public bool TypeUnknow { get; }
        /// <summary>
        /// Апроксимация неизвестной задачи 
        /// </summary>
        public TypeFunForm ApproxUnknow { get; set; }

        public AUnknown2D(string nameUnknow, double[][] masX, bool typeUnknow, TypeFunForm approxUnknow)
        {
            this.NameUnknow = nameUnknow;
            this.masX = masX;
            this.masY = null;
            TypeUnknow = typeUnknow;
            ApproxUnknow = approxUnknow;
        }
        public AUnknown2D(string nameUnknow, double[][] masX, double[][] masY, bool typeUnknow, TypeFunForm approxUnknow) :
            this(nameUnknow, masX, typeUnknow, approxUnknow)
        {
            this.masY = masY;
        }
        public AUnknown2D(AUnknown2D p)
        {
            this.NameUnknow = p.NameUnknow;
            this.masX = p.masX;
            this.masY = p.masY;
            TypeUnknow = p.TypeUnknow;
            ApproxUnknow = p.ApproxUnknow;
        }
        public double[] GetValue1D(double[][] value)
        {
            double[] data = null;
            int NL = value.Length * value[0].Length;
            MEM.Alloc<double>(NL, ref data);
            int co = 0;
            for (int i = 0; i < value.Length; i++)
                for (int j = 0; j < value[0].Length; j++)
                    data[co++] = value[i][j];
            return data;
        }
    }
    /// <summary>
    /// Склалярная неизвестная
    /// </summary>
    [Serializable]
    public class Unknown : AUnknown
    {
        public Unknown(string nameUnknow, double[] mas) :
            base(nameUnknow, mas, true, TypeFunForm.Form_2D_Rectangle_L1) { }

        public Unknown(string nameUnknow, double[] mas, TypeFunForm approxUnknow) :
            base(nameUnknow, mas, true, approxUnknow) { }

        public Unknown(string nameUnknow, double[] masX, double[] masY,  TypeFunForm approxUnknow) :
            base(nameUnknow, masX, masY, true, approxUnknow) { }
        public Unknown(Unknown p) : base(p) { }
    }
    /// <summary>
    /// Склалярная неизвестная
    /// </summary>
    [Serializable]
    public class Unknown2D : AUnknown2D
    {
        public Unknown2D(string nameUnknow, double[][] mas, TypeFunForm approxUnknow) :
            base(nameUnknow, mas, true, approxUnknow) { }
        public Unknown2D(string nameUnknow, double[][] masX, double[][] masY, TypeFunForm approxUnknow) :
            base(nameUnknow, masX, masY, true, approxUnknow) { }
        public Unknown2D(Unknown2D p) : base(p) { }
    }

    /// <summary>
    /// Склалярное вычисляемое поле
    /// </summary>
    [Serializable]
    public class CalkPapams : AUnknown
    {
        public CalkPapams(string nameUnknow, double[] mas) :
            base(nameUnknow, mas, false, TypeFunForm.Form_2D_Rectangle_L1) { }
        public CalkPapams(string nameUnknow, double[] mas, TypeFunForm approxUnknow = TypeFunForm.Form_2D_Triangle_L1) :
            base(nameUnknow, mas, false, approxUnknow) { }
        public CalkPapams(string nameUnknow, double[] masX, double[] masY, TypeFunForm approxUnknow) :
            base(nameUnknow, masX, masY, false, approxUnknow) { }
        public CalkPapams(string nameUnknow, double[] masX, double[] masY) :
            base(nameUnknow, masX, masY, false, TypeFunForm.Form_2D_Rectangle_L1) { }
        public CalkPapams(Unknown p) : base(p) { }
    }
    /// <summary>
    /// Склалярное вычисляемое поле
    /// </summary>
    [Serializable]
    public class CalkPapams2D : AUnknown2D
    {
        public CalkPapams2D(string nameUnknow, double[][] mas, TypeFunForm approxUnknow) :
            base(nameUnknow, mas, false, approxUnknow) { }
        public CalkPapams2D(string nameUnknow, double[][] masX, double[][] masY, TypeFunForm approxUnknow) :
            base(nameUnknow, masX, masY, false, approxUnknow) { }
        public CalkPapams2D(Unknown2D p) : base(p) { }
    }
}
