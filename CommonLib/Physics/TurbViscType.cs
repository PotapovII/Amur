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
namespace CommonLib.Physics
{
    using System;
    using System.ComponentModel;
    /// <summary>
    /// Флаг для модели алгебраической турбулентной вязкости
    /// </summary>
    [Serializable]
    public enum TurbViscType
    {
        /// <summary>
        /// Профиль турбулентной вязкости Буссинеск 1865
        /// </summary>
        [Description("Профиль турбулентной вязкости Буссинеск 1865")]
        Boussinesq1865 = 0,
        /// <summary>
        /// Профиль турбулентной вязкости Караушев 1977
        /// </summary>
        [Description("Профиль турбулентной вязкости Караушев 1977")]
        Karaushev1977 = 1,
        /// <summary>
        /// "Профиль турбулентной вязкости Прандтль 1934
        /// </summary>
        [Description("Профиль турбулентной вязкости Прандтль 1934")]
        Prandtl1934 = 2,
        /// <summary>
        /// Профиль турбулентной вязкости Великанова 1948
        /// </summary>
        [Description("Профиль турбулентной вязкости Великанова 1948")]
        Velikanov1948 = 3,
        /// <summary>
        /// Профиль турбулентной вязкости Рафик Абси 2019"
        /// </summary>
        [Description("Профиль турбулентной вязкости Рафик Абси 2019")]
        Absi_2012 = 4,
        /// <summary>
        /// Профиль турбулентной вязкости Рафик Абси 2012"
        /// </summary>
        [Description("Профиль турбулентной вязкости Рафик Абси 2012")]
        Absi_2019 = 5,
        /// <summary>
        /// "Профиль турбулентной вязкости Leo C. van Rijn 1984
        /// </summary>
        [Description("Профиль турбулентной вязкости Лео К. ван Рейн 1984")]
        Leo_C_van_Rijn1984_C = 6,
        /// <summary>
        /// "Профиль турбулентной вязкости Leo C. van Rijn 1984
        /// </summary>
        [Description("Профиль турбулентной вязкости Лео К. ван Рейн 1984")]
        Leo_C_van_Rijn1984_V = 7,
        /// <summary>
        /// Профиль турбулентной вязкости ванн Дриста 1956
        /// </summary>
        [Description("Профиль турбулентной вязкости ванн Дриста 1956")]
        VanDriest1956 = 8,
        /// <summary>
        /// Не используем алгебраическую модель
        /// </summary>
        [Description("Не используем алгебраическую модель")]
        NotUsingAlgebaraModel = 9
    }
}
