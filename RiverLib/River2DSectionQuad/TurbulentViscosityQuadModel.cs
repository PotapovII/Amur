//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 19.05.2023 Потапов И.И.
//---------------------------------------------------------------------------
namespace RiverLib
{
    using System;
    using System.ComponentModel;

    [Serializable]
    public enum TurbulentViscosityQuadModelOld
    {
        ViscosityConst = 0,
        ViscosityWolfgangRodi = 1,
        Viscosity2DXY = 2,
        ViscositySA0 = 3,
        ViscositySA1 = 4,
        ViscositySA2 = 5,
        [Description("Модель Спаларта - Алмараса")]
        SpalartAllmarasStandard,
        SpalartAllmarasStandardQ,
        SpalartAllmarasNoft2,
        SpalartAllmarasRough,
        /// <summary>
        /// 
        /// </summary>
        [Description("Модель Рея - Агорвала")]
        WrayAgarwal2018,
        WrayAgarwal2018Q,
        SecundovNut92,
        SecundovNut92Q,
        /// <summary>
        /// модель турбулентности Ни–Коважного
        /// </summary>
        Nee_Kovasznay_TaskSolve_0,
        /// <summary>
        /// постоянная правая часть 2 u0 kappa / h
        /// </summary>
        Potapovs_01,
        Potapovs_01Q,
        /// <summary>
        /// постоянная правая часть 2 u0 kappa / h c сосдувом <^>
        /// </summary>
        Potapovs_02,
        Potapovs_02Q,
        /// <summary>
        /// пораболическая правая часть с вихревой генерацией вязкости
        /// </summary>
        Potapovs_03,
        Potapovs_03Q,
        /// <summary>
        /// пораболическая правая часть с квадратичной вязкости
        /// </summary>
        Potapovs_04,
        Potapovs_04Q
    }

}
