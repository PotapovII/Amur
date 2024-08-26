//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                    - (C) Copyright 2015 -
//                       ALL RIGHT RESERVED
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                          01.12.15
//---------------------------------------------------------------------------
namespace BLLib
{
    using CommonLib;
    using MeshLib;
    /// <summary>
    /// Tri-Diagonal Matrix Algorithm
    /// Прогонка для трехдиагональных матриц
    /// </summary>
    class Solver
    {
        /// <summary>
        /// вектор диагонали
        /// </summary>
        double[] C;
        /// <summary>
        /// вектор правой части
        /// </summary>
        double[] R;
        /// <summary>
        /// Исходные вектора техдиагональной СЛАУ
        /// </summary>
        double[] AW;
        double[] AP;
        double[] AE;
        double[] F;
        double[] X;
        /// <summary>
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="imax">Значение i для последнего КО по Х</param>
        /// <param name="jmax">Значение j для последнего КО по Y</param>
        public Solver(int Count)
        {
            C = new double[Count];
            R = new double[Count];
        }
        /// <summary>
        /// Установка исходных данных для задачи 
        /// </summary>
        /// <param name="Aw">Коэффициенты схемы - запад</param>
        /// <param name="Ap">Коэффициенты схемы - центр</param>
        /// <param name="Ae">Коэффициенты схемы - восток</param>
        /// <param name="sc">Правая часть системы</param>
        /// <param name="X">Решение задачи</param>
        public void SetSystem(double[] AW, double[] AP, double[] AE,
                              double[] F, double[] X)
        {
            this.AE = AE;
            this.AP = AP;
            this.AW = AW;
            this.F = F;
            this.X = X;
        }
        /// <summary>
        /// Задание граничных условий
        /// </summary>
        /// <param name="tau">придонное касательное напряжение</param>
        /// <param name="P">придонное давление</param>
        /// </summary>
        public void CalkBCondition(BoundCondition1D Inlet, BoundCondition1D Outlet)
        {
            // граничные условыия на входе
            if (Inlet.typeBC == TypeBoundCond.Dirichlet)
            {
                AP[0] = 1;
                AE[0] = 0;
                F[0] = Inlet.valueBC;
            }
            if (Outlet.typeBC == TypeBoundCond.Dirichlet)
            {
                AP[X.Length - 1] = 1;
                AW[X.Length - 1] = 0;
                F[X.Length - 1] = Outlet.valueBC;
            }
        }
        /// <summary>
        /// Метод решения задачи по алгоритму 
        /// трехдиагональной матричной прогонки
        /// </summary>
        /// <param name="flag">
        /// Учет инверсии знаков в коэффициентах схемы 
        /// метода контрольных объемов,
        /// для симметричных КО системы :
        /// flag=-1   Ap Xp = sum_k (Ak Xk) + b 
        /// классическая трехдиагональная СЛАУ  :
        /// flag= 1   Ap Xp + sum_k (Ak Xk) + b = 0 
        /// </param>
        /// <returns></returns>
        public double[] SolveSystem(int flag = -1)
        {
            int imax = C.Length;
            C[0] = AP[0];
            R[0] = F[0];
            // прямой ход для вычисления коэффициентов С и R 
            for (int i = 1; i < imax; i++)
            {
                C[i] = AP[i] - AW[i] * AE[i - 1] / C[i - 1];
                R[i] = F[i] - flag * AW[i] * R[i - 1] / C[i - 1];
            }
            // обратный ход
            X[imax - 1] = R[imax - 1] / C[imax - 1];
            for (int i = imax - 2; i > -1; i--)
                X[i] = (R[i] - flag * X[i + 1] * AE[i]) / C[i];
            return X;
        }
    }
}
