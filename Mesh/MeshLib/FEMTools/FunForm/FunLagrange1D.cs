//---------------------------------------------------------------------------
//                       ПРОЕКТ  "RiverLib"
//              Перенос на C# : 28.03.2024  Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshLib.FEMTools.FunForm
{
    using CommonLib;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// ОО:  Функции Лагранжа 1 
    /// </summary>
    public class FunLagrange1D : IFunLagrange1D
    {
        protected double L;
        protected double[] x;
        public FunLagrange1D(double[] x)
        {
            this.x = x;
            L = x[1] - x[0];
        }
        /// <summary>
        /// вычисление значений функций формы
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double CalkForm(double xp, double Ua, double Ub, double Uc = 0)
        {
            double N2 = (xp - x[0]) / L;
            double N1 = 1 - N2;
            return Ua * N1 + Ub * N2;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double CalkForm(double xp, double[] U)
        {
            double N1 = (xp - x[0]) / L;
            double N0 = 1 - N1;
            return U[0] * N0 + U[1] * N1;
        }
    }
}
