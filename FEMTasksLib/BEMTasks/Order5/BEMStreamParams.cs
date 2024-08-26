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
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 12.12.2020 Потапов И.И.
//---------------------------------------------------------------------------
namespace FEMTasksLib
{
    using System;
    using System.ComponentModel;
    /// <summary>
    ///  ОО: Параметры для класса ГЭ
    /// </summary>
    [Serializable]
    public class BEMStreamParams
    {
        /// <summary>
        /// Длина расчетной области
        /// </summary>
        [DisplayName("Длина расчетной области")]
        [Category("Задача")]
        protected double L { get; set; }
        /// <summary>
        /// Высота расчетной области
        /// </summary>
        [DisplayName("Высота расчетной области")]
        [Category("Задача")]
        protected double H { get; set; }
        /// <summary>
        /// Координата цилиндра х
        /// </summary>
        [DisplayName("Координата цилиндра х")]
        [Category("Задача")]
        protected double Xc { get; set; }
        /// <summary>
        /// Координата цилиндра y
        /// </summary>
        [DisplayName("Координата цилиндра y")]
        [Category("Задача")]
        protected double Yc { get; set; }
        /// <summary>
        /// Радиус цилиндра
        /// </summary>
        [DisplayName("Радиус цилиндра")]
        [Category("Задача")]
        protected double R { get; set; }
        /// <summary>
        /// Скорость набегания по х на бесконечности
        /// </summary>
        [DisplayName("Скорость набегания по х на бесконечности")]
        [Category("Задача")]
        protected double Vx0 { get; set; }
        /// <summary>
        /// Скорость набегания по y на бесконечности
        /// </summary>
        [DisplayName("Скорость набегания по y на бесконечности")]
        [Category("Задача")]
        protected double Vy0 { get; set; }
        public BEMStreamParams()
        {
            L = 10;
            H = 5;
            Xc = 5;
            Yc = 0.6;
            R = 1;
            Vx0 = 1;
            Vy0 = 1;
        }
        public BEMStreamParams(BEMStreamParams p)
        {
            L = p.L;
            H = p.H;
            Xc = p.Xc;
            Yc = p.Yc;
            R = p.R;
            Vx0 = p.Vx0;
            Vy0 = p.Vy0;
        }
    }
}