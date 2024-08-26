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
//                          06.01.24
//---------------------------------------------------------------------------
namespace CommonLib.ChannelProcess
{
    using System;
    using System.ComponentModel;
    /// <summary>
    /// Флаг определяющий модель изменений донной воверхности
    /// </summary>        
    [Serializable]
    public enum EBedErosion
    {
        /// <summary>
        /// Размыв отсутствует
        /// </summary>
        [Description("Дно неподвижно")]
        NoBedErosion = 0,
        /// <summary>
        /// Размыв - только влекомые наносы
        /// </summary>
        [Description("Размыв дна, движение влекомых наносов")]
        BedLoadErosion,
        /// <summary>
        /// Размыв - только взыешенные наносы
        /// </summary>
        [Description("Размыв дна, движение взвешенных наносов")]
        SedimentErosion,
        /// <summary>
        /// Размыв - учет влекомых и взыешенных наносы
        /// </summary>
        [Description("Размыв дна, движение наносов")]
        BedLoadAndSedimentErosion
    }
}
