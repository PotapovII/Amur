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
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                          02.05.2025
//---------------------------------------------------------------------------
namespace CommonLib.BedLoad
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Тип граничныого условия для уравнения концентрации
    /// </summary>
    [Serializable]
    public enum BCalkConcentration
    {
        /// <summary>
        /// Концентрация не считается
        /// </summary>
        [Description("Концентрация не считается")]
        NotCalkConcentration = 0,
        /// <summary>
        /// Концентрация считается с условиями Дирехле на дне
        /// </summary>
        [Description("Концентрация считается с условиями Дирехле на дне")]
        DirCalkConcentration = 1,
        /// <summary>
        /// Концентрация считается с условиями Неймана на дне
        /// </summary>
        [Description("Концентрация считается с условиями Неймана на дне")]
        NeCalkConcentration = 2,
    }
}
