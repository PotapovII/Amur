using System;

namespace RiverLib
{
    /// <summary>
    /// Типы донных форм
    /// </summary>
    [Serializable]
    public enum TypeBedForm
    {
        PlaneForm = 0,
        L1_L2sin_L3
        //LxOver3_sinL2,
        //L1Null_L2sin_L3Null
    }
    /// <summary>
    /// типы спец. рашателей задачи алгебры
    /// </summary>
    [Serializable]
    public enum TypeMAlgebra
    {
        TriDiagMat_Algorithm = 0,
        CGD_Algorithm,
        CGD_ParrallelAlgorithm
    }

    /// <summary>
    /// Граничные условия на верхней границе области
    /// </summary>
    [Serializable]
    public enum TauBondaryCondition
    {
        slip = 0,
        adhesion = 1
    }

}
