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
//                      - (C) Copyright 2024 -
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                           07.01.24
//---------------------------------------------------------------------------
namespace CommonLib.Physics
{
    using System;
    using System.ComponentModel;
    /// <summary>
    /// Флаг для модели расчета придонной концентрации
    /// </summary>
    [Serializable]
    public enum CbType
    {
        /// <summary>
        /// Модель Потапова
        /// </summary>
        [Description("Придонная концентрация по Потапову 24")]
        Potapov_2024 = 0,
        /// <summary>
        /// Модель Эйнштейна
        /// </summary>
        [Description("Придонная концентрация по Эйнштейну 50")]
        Einstein_1950,
        /// <summary>
        /// Модель Ван Рейна 84
        /// </summary>
        [Description("Придонная концентрация по Ван Рейну 84")]
        Van_Rijn_Leo_1984,
        /// <summary>
        /// Модель Ван Рейна 86
        /// </summary>
        [Description("Придонная концентрация по Ван Рейну 86")]
        Van_Rijn_Leo_1986,
        /// <summary>
        /// Модель Смит и Маклин
        /// </summary>
        [Description("Придонная концентрация по Смиту и Маклину 77")]
        Smith_McLean_1977,
        /// <summary>
        /// Модель Смит и Маклин
        /// </summary>
        [Description("Придонная концентрация по Энгелунду и Фредс0е 76")]
        Engelund_Freds0e_1976
    }
}