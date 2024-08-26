//---------------------------------------------------------------------------
//  Класс BCond предназначен выполнения граничных условий при решении САУ - прогонкой 
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
    using System;
    [Serializable]
    /// <summary>
    /// Граничные условия
    /// </summary>
    public class BCond
    {
        /// <summary>
        /// Индекс южной границы 0
        /// </summary>
        public static int S_index = 0;
        /// <summary>
        /// Индекс северной границы 1
        /// </summary>
        public static int N_index = 1;
        /// <summary>
        /// Индекс западной границы 2
        /// </summary>
        public static int W_index = 2;
        /// <summary>
        /// Индекс восточной границы 3
        /// </summary>
        public static int E_index = 3;
        public int[][] BC = null;
        public double[][] Value = null;
        public BCond() { }
        public BCond(int imax, int jmax)
        {
            BC = new int[4][];
            BC[0] = new int[imax];
            BC[1] = new int[imax];
            BC[2] = new int[jmax];
            BC[3] = new int[jmax];
            Value = new double[4][];
            Value[0] = new double[imax];
            Value[1] = new double[imax];
            Value[2] = new double[jmax];
            Value[3] = new double[jmax];
        }
        /// <summary>
        /// Установка граничных условий (полный контур) по существующему полю X
        /// </summary>
        public void SetBCondition(int ist, int jst, int imax, int jmax, ref double[][] X)
        {
            int i, j;
            imax++;
            jmax++;
            ist--;
            jst--;
            // Расчет вертикальных коэффициентов As[i][j], An[i][j]
            //              1      imax  
            //i=0,j=0|-------------|--> i x
            //       |      S      |
            //       |             |
            //       | 0         0 |
            //    W  |             | E    Ae[i][j] = Aw[i+1][j] - flow
            //       |             |      An[i][j] = As[i][j+1] - flow
            //       |      N      |
            //       |-------------|
            //       | y    1  
            //       V j          
            j = jst;
            for (i = ist; i < imax; i++)
            {
                BC[BCond.S_index][i] = 1;
                Value[BCond.S_index][i] = X[i][j];
            }
            j = jmax - 1;
            for (i = ist; i < imax; i++)
            {
                BC[BCond.N_index][i] = 1;
                Value[BCond.N_index][i] = X[i][j];
            }
            i = ist;
            for (j = jst; j < jmax; j++)
            {
                BC[BCond.W_index][j] = 1;
                Value[BCond.W_index][j] = X[i][j];
            }
            i = imax - 1;
            for (j = jst; j < jmax; j++)
            {
                BC[BCond.E_index][j] = 1;
                Value[BCond.E_index][j] = X[i][j];
            }
        }
    }
}
