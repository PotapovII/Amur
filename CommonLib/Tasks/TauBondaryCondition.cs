using System;
using System.ComponentModel;
namespace CommonLib.Tasks
{
    /// <summary>
    /// Касательные граничные условия на границе области
    /// </summary>
    [Serializable]
    public enum TauBondaryCondition
    {
        [Description("Скольжение")]
        slip = 0,
        [Description("Прилипание")]
        adhesion = 1
    }
}
