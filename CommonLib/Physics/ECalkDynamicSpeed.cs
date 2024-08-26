namespace CommonLib.Physics
{
    using System;
    using System.ComponentModel;
    /// <summary>
    /// метод расчета динамической скорости на стенках канала
    /// </summary>
    [Serializable]
    public enum ECalkDynamicSpeed
    {
        /// <summary>
        /// Расчет динамической скорости по уклону и глубине
        /// </summary>
        [Description("Расчет динамической скорости по уклону и глубине")]
        u_start_J = 0,
        /// <summary>
        /// Гидростатический расчет динамической скорости
        /// </summary>
        [Description("Гидростатический расчет динамической скорости")]
        u_start_M,
        /// <summary>
        /// Расчет динамической скорости по придонной скорости
        /// </summary>
        [Description("Расчет динамической скорости по придонной скорости")]
        u_start_U,
        /// <summary>
        /// расчет динамической скорости по пристеночной турбулентной энергии
        /// </summary>
        [Description("расчет динамической скорости по пристеночной турбулентной энергии")]
        u_start_Ke
    }

}
