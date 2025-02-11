//---------------------------------------------------------------------------
//                       ПРОЕКТ  "RiverLib"
//              Перенос на C# : 28.03.2024  Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshLib.FEMTools.FunForm
{
    using CommonLib;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// ОО:  Функции Лагранжа 2 
    /// </summary>
    public class FunLagrange2D : IFunLagrange1D
    {
        protected double L1, L2, L3;
        protected double[] x;
        public FunLagrange2D(double[] x)
        {
            this.x = x;
            L1 = (x[0] - x[1]) * (x[0] - x[2]);
            L2 = (x[1] - x[0]) * (x[1] - x[2]);
            L3 = (x[2] - x[0]) * (x[2] - x[1]);
        }
        /// <summary>
        /// вычисление значений функций формы
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double CalkForm(double xp, double Ua, double Ub, double Uc = 0)
        {
            double N1 = (xp - x[1]) * (xp - x[2]) / L1;
            double N2 = (xp - x[0]) * (xp - x[2]) / L2;
            double N3 = (xp - x[0]) * (xp - x[1]) / L3;
            return Ua * N1 + Ub * N2 + Uc * N3;
        }
        /// <summary>
        /// вычисление значений функций формы
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double CalkForm(double xp, double[] U)
        {
            double N1 = (xp - x[1]) * (xp - x[2]) / (x[0] - x[1]) / (x[0] - x[2]);
            double N2 = (xp - x[0]) * (xp - x[2]) / (x[1] - x[0]) / (x[1] - x[2]);
            double N3 = (xp - x[0]) * (xp - x[1]) / (x[2] - x[0]) / (x[2] - x[1]);
            return U[0] * N1 + U[1] * N2 + U[2] * N3;
        }
    }
}
