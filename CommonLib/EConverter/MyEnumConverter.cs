#region License
/*
Copyright (c) 2019 - 2024  Potapov Igor Ivanovich, Khabarovsk

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion
//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2021 -
//                       ALL RIGHT RESERVED
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                          21.08.22
//---------------------------------------------------------------------------
namespace CommonLib.EConverter
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    /// <summary>
    /// Используем для руссификации пропертигрида 
    /// при использовании перечислений
    /// </summary>
    public class MyEnumConverter : EnumConverter
    {
        private Type type;

        public MyEnumConverter(Type type)
            : base(type)
        {
            this.type = type;
        }

        public override object ConvertTo(ITypeDescriptorContext context,
            CultureInfo culture, object value, Type destType)
        {
            FieldInfo field = type.GetField(Enum.GetName(type, value));
            DescriptionAttribute desAttribute =
              (DescriptionAttribute)Attribute.GetCustomAttribute(
                field, typeof(DescriptionAttribute));

            if (desAttribute != null)
                return desAttribute.Description;
            else
                return value.ToString();
        }
        public override object ConvertFrom(ITypeDescriptorContext context,
            CultureInfo culture, object value)
        {
            foreach (FieldInfo field in type.GetFields())
            {
                DescriptionAttribute desAttribute =
                  (DescriptionAttribute)Attribute.GetCustomAttribute(
                    field, typeof(DescriptionAttribute));

                if ((desAttribute != null) && ((string)value == desAttribute.Description))
                    return Enum.Parse(type, field.Name);
            }
            return Enum.Parse(type, (string)value);
        }
    }
}
