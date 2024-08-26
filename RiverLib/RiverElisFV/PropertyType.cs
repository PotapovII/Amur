//---------------------------------------------------------------------------
//                 Разработка кода : Снигур К.С.
//                         релиз 26 06 2017 
//---------------------------------------------------------------------------
//         интеграция в RiverLib 13 08 2022 : Потапов И.И.
//---------------------------------------------------------------------------
namespace RiverLib
{
    using System;
    using System.ComponentModel;
    /// <summary>
    /// Тип вычислений
    /// </summary>
    [Serializable]
    public enum PropertyType
    {
        [Description("Однопоточный")]
        singleThreaded = 0,
        [Description("Многопоточный")]
        multithreaded
    }

    /// <summary>
    /// Тип вычислительного устройства/технологии CPU || GPU (OpenCL or CUDA)
    /// </summary>
    [Serializable]
    public enum TypePU
    {
        [Description("Процессор")]
        Cpu = 0,
        [Description("GPU CUDA")]
        Gpu_Cuda,
        [Description("GPU OpenCL")]
        Gpu_OpenCL
    }

    /// <summary>
    /// Метод вычисления функции стенки
    /// </summary>
    [Serializable]
    public enum WallFunctionType
    {
        /// <summary>
        /// шероховатая стенка по Луцкому
        /// </summary>
        [Description("Шероховатая стенка по Луцкому")]
        roughWall_Lutsk = 0,
        /// <summary>
        /// гладкая стенка по Снегиреву
        /// </summary>
        [Description("Гладкая стенка по Снегиреву")]
        smoothWall_Snegirev,
        /// <summary>
        /// гладкая стенка по Луцкому
        /// </summary>
        [Description("Гладкая стенка по Луцкому")]
        smoothWall_Lutsk,
        /// <summary>
        /// гладкая стенка по Волкову 
        /// </summary>
        [Description("Гладкая стенка по Волкову ")]
        smoothWall_Volkov
        /// <summary>
        /// гладкая стенка по Волкову Ньютон
        /// </summary>
        //,smoothWall_VolkovNewton
    }

    /// <summary>
    /// Тип турбулентной модели
    /// </summary>
    [Serializable]
    public enum TurbulentModelType   
    {
        [Description("Модель т. К-Е")]
        Model_k_e,
        [Description("Модель т. К-W")]
        Model_k_w,
        [Description("Модель т. К-Е c пристеночной функцией по Жлуткову")]
        Model_k_eG,
        [Description("Модель ньютоновской жидкости")]
        Model_Newton
    }

    /// <summary>
    /// метод нахождения размерного касательного напряжения на дне
    /// </summary>
    [Serializable]
    public enum ModeTauCalklType
    {
        [Description("Через ленту придонных КЭ для tauXY")]
        method0,
        [Description("Нормальные конечные разности")]
        method1,
        [Description("Через ленту придонных КЭ для тензора Tau")]
        method2
    }

    /// <summary>
    /// Вид профиля скорости на входе в расчетную область
    /// </summary>
    [Serializable]
    public enum VelocityInProfile
    {
        [Description("Порабола заданная через расход")]
        PorabolicProfile = 0,
        [Description("Степенной профиль заданный при N=20")]
        PowerProfile = 1,
        [Description("Профиль заднный функцией")]
        FunctionProfile = 2
    }
}
