//---------------------------------------------------------------------------
//      Класс TPSolver предназначен для решения САУ - прогонкой 
//                              Потапов И.И.
//                        - (C) Copyright 2015 -
//                          ALL RIGHT RESERVED
//                               31.07.15
//---------------------------------------------------------------------------
//   Реализация библиотеки для решения задачи турбулентного теплообмена
//                   методом контрольного объема
//---------------------------------------------------------------------------
namespace AlgebraLib
{

    /// <summary>
    /// Солвер
    /// </summary>
    public interface ITPSolver
    {
        /// <summary>
        /// Метод техдиагональной матричной прогонки 
        /// </summary>
        bool OnTDMASolver(int ist, int jst, int imax, int jmax,
             double[][] Ae, double[][] Aw, double[][] An, double[][] As, double[][] Ap,
             double[][] sc, BCond bc, ref double[][] X, int CountIter = 1);
    }
}
