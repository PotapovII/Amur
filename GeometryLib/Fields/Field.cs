//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                    создание поля Field1D
//                 кодировка : 14.12.2020 Потапов И.И.
//---------------------------------------------------------------------------
//                      создание иерархии полей
//                 кодировка : 13.03.2022 Потапов И.И.
//---------------------------------------------------------------------------
namespace GeometryLib
{
    using MemLogLib;
    using System;
    /// <summary>
    /// ОО: Поле название поля и массив(ы) данных поля в узлах сетки
    /// </summary>
    [Serializable]
    abstract public class Field<T> : IField
    {
        /// <summary>
        /// Название поля
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Контекстный флаг используется при отображении поля
        /// </summary>
        //[NonSerialized]
        public bool Check { get; set; }
        /// <summary>
        /// Контекстный флаг используется при отображении поля
        /// </summary>
        //[NonSerialized]
        public uint idx { get; set; }
        /// <summary>
        /// <summary>
        /// арнумент функции или первая компонента векторного поля
        /// </summary>
        public T[] Values;
        /// <summary>
        /// индексатор
        /// </summary>
        public T this[int index]
        {
            get => Values[(int)index];
            set => Values[(int)index] = value;
        }
        public T this[uint index]
        {
            get => Values[(int)index];
            set => Values[(int)index] = value;
        }
        public Field(string Name = "", T[] V = null)
        {
            this.Name = Name;
            if (V != null)
                this.Values = MEM.Copy(this.Values, V);
        }
        public Field(string Name, int Count)
        {
            this.Name = Name;
            MEM.Alloc(Count, ref Values);
        }
        public Field(Field<T> p)
        {
            Name = p.Name;
            this.Values = MEM.Copy(this.Values, p.Values);
        }
        public void GetValue(ref T[] V)
        {
            V = MEM.Copy(V, Values);
        }
        public void GetValue(ref object[] V)
        {
            GetValue(ref V);
        }
        public void SetValue(T[] V)
        {
            Values = MEM.Copy(Values, V);
        }
        public virtual int Dimention { get; }
    }
}

