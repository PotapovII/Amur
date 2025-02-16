namespace MeshGeneratorsLib.StripGenerator
{
    using System;
    /// <summary>
    /// Признак типа границы области
    /// </summary>
    [Flags]
    public enum BoundaryCrossSectionType
    {
        RiverBottom     = 0b_0000_0000,  // 0
        RightBankRiver  = 0b_0000_0001,  // 1
        RiverSurface    = 0b_0000_0010,  // 2
        LeftBankRiver   = 0b_0000_0100,  // 4
        AxisSymmetry    = 0b_0000_1000,  // 8
        InputBoundary   = 0b_0001_0000,  // 16
        OutputBoundary  = 0b_0010_0000,  // 32
        SlidingBoundary = 0b_0100_0000,  // 64
        WettedPerimeter = RiverBottom | RightBankRiver | LeftBankRiver
    }
}
