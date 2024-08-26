using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
namespace RiverLib
{
    [Serializable]
    [TypeConverter(typeof(EnumConverter))]
    public enum TypeBoundaryCondition 
    {
        /// <summary>
        /// Первого рода - задана функция
        /// </summary>
        [Description("Дирихле = value")]
        Dirichlet,
        /// <summary>
        /// Первого рода - однородные
        /// </summary>
        [Description("Дирихле = 0")]
        Dirichlet0,
        /// <summary>
        /// Второго рода - задан поток
        /// </summary>
        [Description("Нейман = value")]
        Neumann,
        /// <summary>
        /// Второго рода - однородные
        /// </summary>
        [Description("Нейман = 0")]
        Neumann0,
        /// <summary>
        /// Второго рода -  = alpha (Fun - Fun_inf)^beta
        /// </summary>
        [Description("Нейман  = alpha (Fun - Fun_inf)^beta")]
        Neumann1,
    }

    //public interface IBoundaryCondition
    //{
    //    int[] BordersLabels(TypeBoundaryCondition type);
    //    void BordersCondition(string Name,int label, ref double[] bc, ref uint[] bcIndex);
    //    void AreaFunction(string Name, ref double[] Fun);
    //}
}
