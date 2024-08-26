namespace CommonLib.Physics
{
    using System;
    using System.ComponentModel;
    /// <summary>
    /// Дифференциальыне модели турбулентности с 1 уравнением
    /// </summary>
    [Serializable]
    public enum ETurbViscType_1D
    {
        /// <summary>
        /// Модель Спаларта - Алмараса стандарт
        /// </summary>
        [Description("Модель Спаларта - Алмараса стандарт")]
        SpalartAllmarasStandard,
        /// <summary>
        /// Модель Рея - Агорвала
        /// </summary>
        [Description("Модель Рея - Агорвала")]
        WrayAgarwal2018,
        /// <summary>
        /// Модель Рея - Агорвала
        /// </summary>
        [Description("Модель Секундова 92")]
        SecundovNut92,
        /// <summary>
        /// модель турбулентности Ни–Коважного
        /// </summary>
        [Description("Модель Ни–Коважного")]
        Nee_Kovasznay_TaskSolve_0,
        /// <summary>
        /// Модель Потапова 23 постоянная правая часть 2 u0 kappa / h
        /// </summary>
        [Description("Модель Потапова 23")]
        Potapovs_23,
    }
}
