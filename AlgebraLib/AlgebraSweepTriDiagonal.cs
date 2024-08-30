//---------------------------------------------------------------------------
//   Реализация библиотеки методов решения систем алгебраических уравнений
//                     - (C) Copyright 2015 -
//                       ALL RIGHT RESERVED
//                    разработка: Потапов И.И.
//                            01.12.15
//---------------------------------------------------------------------------
namespace AlgebraLib
{
    using MemLogLib;
    using System;
    using CommonLib;
    /// <summary>
    /// Tri-Diagonal Matrix Algorithm
    /// Прогонка для трехдиагональных матриц
    /// </summary>
    public class AlgebraSweepTriDiagonal : Algebra
    {
        /// <summary>
        /// вектор диагонали
        /// </summary>
        double[] C = null;
        /// <summary>
        /// вектор правой части
        /// </summary>
        double[] R = null;
        /// <summary>
        /// Исходные вектора техдиагональной СЛАУ
        /// </summary>
        double[] AW;
        double[] AP;
        double[] AE;
        /// Учет инверсии знаков в коэффициентах схемы 
        /// метода контрольных объемов,
        /// для симметричных КО системы :
        /// flag=-1   Ap Xp = sum_k (Ak Xk) + b 
        /// классическая трехдиагональная СЛАУ  :
        /// flag= 1   Ap Xp + sum_k (Ak Xk) + b = 0 
        int flag = -1;
        /// <summary>
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="imax">Значение i для последнего КО по Х</param>
        /// <param name="jmax">Значение j для последнего КО по Y</param>
        public AlgebraSweepTriDiagonal(uint Count, int flag = -1) : base(new AlgebraResult(), Count)
        {
            name = "Метод прогонки для трехдиагональных матриц";
            SetAlgebra(result, Count);
        }
        /// <summary>
        /// нормировка системы для малых значений матрицы и правой части
        /// </summary>
        protected override void SystemNormalization()
        {
            // номировка для данного солвера не реализована
        }
        public override void SetAlgebra(IAlgebraResult res, uint NN)
        {
            try
            {
                if (NN != FN)
                    base.SetAlgebra(res, NN);
                result.Name = name;
                MEM.AllocClear(FN, ref C);
                MEM.AllocClear(FN, ref R);
                MEM.AllocClear(FN, ref AW);
                MEM.AllocClear(FN, ref AE);
                MEM.AllocClear(FN, ref AP);
            }
            catch (Exception ex)
            {
                result.errorType = ErrorType.outOfMemory;
                result.Message = ex.Message;
            }
        }

        /// <summary>
        /// <summary>
        /// Очистка матрицы и правой части
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            for (int i = 0; i < FN; i++)
            {
                C[i] = 0;
                R[i] = 0;
                AP[i] = 0;
                AE[i] = 0;
                AW[i] = 0;
            }
        }
        /// <summary>
        /// Формирование матрицы системы
        /// </summary>
        /// <param name="LMartix">Локальная матрица</param>
        /// <param name="Adress">Глабальные адреса</param>
        public override void AddToMatrix(double[][] LMartix, uint[] Adress)
        {
            uint ai = Adress[0];
            AP[ai] += LMartix[0][0];
            AE[ai] += LMartix[0][1];
            AW[ai + 1] += LMartix[1][0];
            AP[ai + 1] += LMartix[1][1];
        }
        /// <summary>
        /// // Сборка ГПЧ
        /// </summary>
        public override void AddToRight(double[] LRight, uint[] Adress)
        {
            uint ai = Adress[0];
            Right[ai] += LRight[0];
            Right[ai + 1] += LRight[1];
        }
        /// <summary>
        /// Установка исходных данных для задачи 
        /// </summary>
        /// <param name="Aw">Коэффициенты схемы - запад</param>
        /// <param name="Ap">Коэффициенты схемы - центр</param>
        /// <param name="Ae">Коэффициенты схемы - восток</param>
        /// <param name="sc">Правая часть системы</param>
        /// <param name="X">Решение задачи</param>
        public void AddStringSystem(double[] AW, double[] AP, double[] AE, double[] Right)
        {
            this.AE = AE;
            this.AP = AP;
            this.AW = AW;
            this.Right = Right;
        }
        public override void AddStringSystem(double[] ColElems, uint[] ColAdress, uint IndexRow, double R)
        {
            this.AW[IndexRow] = ColElems[0];
            this.AP[IndexRow] = ColElems[0];
            this.AE[IndexRow] = ColElems[0];
            this.Right[IndexRow] = R;
        }
        /// <summary>
        /// Удовлетворение ГУ (с накопительными вызовами)
        /// </summary>
        /// <param name="Condition">Значения неизвестных по адресам</param>
        /// <param name="Adress">Глабальные адреса</param>
        public override void BoundConditions(double[] Conditions, uint[] Adress)
        {
            for (int i = 0; i < Adress.Length; i++)
            {
                uint idx = Adress[i];
                if (idx == 0)
                {
                    AP[0] = 1;
                    AE[0] = 0;
                    Right[0] = Conditions[i];
                }
                else
                if (idx == AP.Length - 1)
                {
                    AP[AP.Length - 1] = 1;
                    AW[AP.Length - 1] = 0;
                    Right[AP.Length - 1] = Conditions[i];
                }
                else
                {
                    AE[idx] = 0;
                    AP[idx] = 1;
                    AW[idx] = 0;
                    Right[idx] = Conditions[i];
                }
            }
        }
        /// <summary>
        /// Удовлетворение ГУ (с накопительными вызовами)
        /// </summary>
        /// <param name="Condition">Значения неизвестных по адресам</param>
        /// <param name="Adress">Глабальные адреса</param>
        public override void BoundConditions(double Conditions, uint[] Adress)
        {
            for (int i = 0; i < Adress.Length; i++)
            {
                uint idx = Adress[i];
                if (idx == 0)
                {
                    AP[0] = 1;
                    AE[0] = 0;
                    Right[0] = Conditions;
                }
                else
                if (idx == AP.Length - 1)
                {
                    AP[AP.Length - 1] = 1;
                    AW[AP.Length - 1] = 0;
                    Right[AP.Length - 1] = Conditions;
                }
                else
                {
                    AE[idx] = 0;
                    AP[idx] = 1;
                    AW[idx] = 0;
                    Right[idx] = Conditions;
                }
            }
        }
        /// <summary>
        /// Операция определения невязки R = Matrix X - Right
        /// </summary>
        /// <param name="R">результат</param>
        /// <param name="X">умножаемый вектор</param>
        /// <param name="IsRight">знак операции = +/- 1</param>
        public override void getResidual(ref double[] R, double[] X, int IsRight = 1)
        {
            for (int i = 0; i < FN; i++)
            {
                if (i == 0)
                    R[0] = AP[0] * X[0] + AE[0] * X[1];
                else
                if (i == FN - 1)
                    R[i] = AW[i] * X[i - 1] + AP[i] * X[i];
                else
                    R[i] = AW[i] * X[i - 1] + AP[i] * X[i] + AE[i] * X[i + 1];
            }
        }
        /// <summary>
        /// Вывод САУ на КОНСОЛЬ
        /// </summary>
        public override void Print(int flag = 0)
        {
            double[,] Matrix = new double[Right.Length, Right.Length];

            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                    Matrix[i, j] = 0;
            for (int i = 0; i < N; i++)
            {
                if (i == 0)
                {
                    Matrix[0, 0] = AP[0]; Matrix[0, 1] = AE[0];
                }
                else
                if (i == N - 1)
                {
                    Matrix[N - 1, N - 1] = AP[N - 1];
                    Matrix[N - 1, N - 2] = AW[N - 1];
                }
                else
                {
                    Matrix[i, i + 1] = AE[i];
                    Matrix[i, i] = AP[i];
                    Matrix[i, i - 1] = AW[i];
                }
            }
            Console.WriteLine("Matrix");
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                    Console.Write(" " + Matrix[i, j].ToString("F4"));
                Console.WriteLine();
            }
            Console.WriteLine("Right");
            for (int i = 0; i < N; i++)
                Console.Write(" " + Right[i].ToString("F4"));
            Console.WriteLine();
        }
        /// <summary>
        /// Метод решения задачи по алгоритму 
        /// трехдиагональной матричной прогонки
        /// </summary>
        protected override void solve(ref double[] X)
        {
            if (X == null)
                X = new double[FN];
            int imax = C.Length;
            C[0] = AP[0];
            R[0] = Right[0];
            // прямой ход для вычисления коэффициентов С и R 
            for (int i = 1; i < imax; i++)
            {
                C[i] = AP[i] - AW[i] * AE[i - 1] / C[i - 1];
                R[i] = Right[i] - flag * AW[i] * R[i - 1] / C[i - 1];
            }
            // обратный ход
            X[imax - 1] = R[imax - 1] / C[imax - 1];
            for (int i = imax - 2; i > -1; i--)
                X[i] = (R[i] - flag * X[i + 1] * AE[i]) / C[i];

            Result.X = X;
        }
        /// <summary>
        /// Клонирование объекта
        /// </summary>
        /// <returns></returns>
        public override IAlgebra Clone()
        {
            return new AlgebraSweepTriDiagonal(FN, flag);
        }
    }
}
