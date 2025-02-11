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
    /// Флаг для модели расчета гидравлической крупности
    /// </summary>
    [Serializable]
    public enum EWsType
    {
        /// <summary>
        /// Гидравлическая крупность по Архангельскому
        /// </summary>
        [Description("по Архангельскому")]
        Arkhangelsky = 0,
        /// <summary>
        /// Гидравлическая крупность по Горчарову
        /// </summary>
        [Description("по Горчарову")]
        Goncharova,
        /// <summary>
        /// Гидравлическая крупность по Гришанину
        /// </summary>
        [Description("по Гришанину")]
        Grishanin,
        /// <summary>
        /// Гидравлическая крупность по Руби
        /// </summary>
        [Description("по Руби")]
        Ruby,
        /// <summary>
        /// Гидравлическая крупность по Ибн - Заде
        /// </summary>
        [Description("по Ибн - Заде")]
        Ibade_Zade,
        /// <summary>
        /// Гидравлическая крупность по Ван Рейну
        /// </summary>
        [Description("по Ван Рейну")]
        Van_Rijn,
        /// <summary>
        /// Гидравлическая крупность по Ша
        /// </summary>
        [Description("по Ша")]
        Sha
    }
    /// <summary>
    /// Флаг формы частиц для модели расчета гидравлической крупности
    /// </summary>
    [Serializable]
    public enum ParticleForms
    {
        /// <summary>
        /// Округленные частицы  (шарообразные) 1.2
        /// </summary>
        [Description("Округленные частицы")]
        Spherical = 0,
        /// <summary>
        /// Многогранные частицы  1
        /// </summary>
        [Description("Многогранные частицы")]
        Polyhedral,
        /// <summary>
        /// Гидравлическая частицы (эллипсоидальные) 3/4 = 0.75
        /// </summary>
        [Description("Уплощенные частицы")]  
        Flattened,
        /// <summary>
        /// Пластинчатые частицы (уплощенные) 0.5
        /// </summary>
        [Description("Пластинчатые частицы")]
        Lamellar
    }


}
