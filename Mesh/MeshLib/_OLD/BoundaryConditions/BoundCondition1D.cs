namespace MeshLib
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using CommonLib;
    /// <summary>
    /// OO: Класс для задания типа граничных условий
    /// </summary>
    [Category("Граничные условия")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Serializable]
    public class BoundCondition1D
    {
        /// <summary>
        /// Тип граничных условий на границе
        /// </summary>
        [DisplayName("Тип граничных условий")]
        [TypeConverter(typeof(EnumTypeConverter))]
        public TypeBoundCond typeBC { get => tbc; set { tbc = value; } }
        TypeBoundCond tbc;
        /// <summary>
        /// Значение граничного условия на границе
        /// </summary>
        [DisplayName("Значение на границе")]
        //[Category("Граничные условия")]
        public double valueBC { get => val; set { val = value; } }
        double val;
        public BoundCondition1D(TypeBoundCond mark, double v = 0)
        {
            tbc = mark;
            val = v;
        }
        public BoundCondition1D(double v)
        {
            tbc = TypeBoundCond.Dirichlet;
            val = v;
        }
        /// <summary>
        /// Представление в виде строки
        /// </summary>
        public override string ToString()
        {
            string[] lines = { "Дирихле", "Нейман", "Периодические" };
            return lines[(int)tbc] + " " + val.ToString("F5");
        }
    }

    /// <summary>
    /// Класс для правильного отображения BoundCondition1D в таблице свойств
    /// TypeConverter для Enum, преобразовывающий Enum к строке с учетом атрибута Description
    /// </summary>
    class EnumTypeConverter : EnumConverter
    {
        private Type _enumType;
        /// <summary>Инициализирует экземпляр</summary>/// <param name="type">тип Enum</param>
        public EnumTypeConverter(Type type) : base(type)
        {
            _enumType = type;
        }
        public override bool CanConvertTo(ITypeDescriptorContext context,
          Type destType)
        {
            return destType == typeof(string);
        }
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture,
          object value, Type destType)
        {
            FieldInfo fi = _enumType.GetField(Enum.GetName(_enumType, value));
            DescriptionAttribute dna =
              (DescriptionAttribute)Attribute.GetCustomAttribute(
                fi, typeof(DescriptionAttribute));

            if (dna != null)
                return dna.Description;
            else
                return value.ToString();
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context,
          Type srcType)
        {
            return srcType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context,
          CultureInfo culture, object value)
        {
            foreach (FieldInfo fi in _enumType.GetFields())
            {
                DescriptionAttribute dna =
                  (DescriptionAttribute)Attribute.GetCustomAttribute(
                    fi, typeof(DescriptionAttribute));

                if ((dna != null) && ((string)value == dna.Description))
                    return Enum.Parse(_enumType, fi.Name);
            }
            return Enum.Parse(_enumType, (string)value);
        }
    }

}
