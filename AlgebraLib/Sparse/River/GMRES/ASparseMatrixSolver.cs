//---------------------------------------------------------------------------
//   Реализация библиотеки методов решения систем алгебраических уравнений
//                    разработка: Потапов И.И.
//                      03.06.2025
//---------------------------------------------------------------------------
namespace AlgebraLib.Sparse.River
{
    using CommonLib;
    using MemLogLib;
    using System;
    using System.Linq;

    public abstract class ASparseMatrixSolver : Algebra, IAlgebra
    {
        /// <summary>
        /// Основная матрица решателя
        /// </summary>
        protected SparseMatrix SMatrix;

        #region Конструкторы
        public ASparseMatrixSolver(uint FN, int rowSize) : base(FN)
        {
            SMatrix = new SparseMatrix((int)FN, rowSize);
            SetAlgebra(new AlgebraResultIterative(), FN);
        }
        public ASparseMatrixSolver(SparseMatrix matrix) : base(matrix.N)
        {
            SMatrix = new SparseMatrix(matrix);
            SetAlgebra(new AlgebraResultIterative(), SMatrix.N);
        }
        public ASparseMatrixSolver(ASparseMatrixSolver algebra) : base(algebra.N)
        {
            SetAlgebra(new AlgebraResultIterative(), algebra.N);
            SMatrix = new SparseMatrix(algebra.SMatrix);
            MEM.Copy(ref Right, algebra.Right);
        }
        #endregion 

        #region IAlgebra
        /// <summary>
        /// Очистка матрицы и правой части
        /// </summary>
        public override void Clear()
        {
            SMatrix.Clear();
            base.Clear();
        }
        /// <summary>
        /// Сборка ГМЖ
        /// </summary>
        public override void AddToMatrix(double[][] LMartix, uint[] Adress)
        {
            SMatrix.AddToMatrix(LMartix, Adress);
        }
        /// <summary>
        /// Сборка САУ по строкам (не для всех решателей)
        /// </summary>
        /// <param name="ColElems">Коэффициенты строки системы</param>
        /// <param name="ColAdress">Адреса коэффицентов</param>
        /// <param name="IndexRow">Индекс формируемой строки системы</param>
        /// <param name="Right">Значение правой части</param>
        public override void AddStringSystem(double[] ColElems, uint[] ColAdress, uint IndexRow, double R)
        {
            SMatrix.AddStringSystem(ColElems, ColAdress, IndexRow);
            Right[IndexRow] = R;
        }
        /// <summary>
        /// Получить строку (не для всех решателей)
        /// </summary>
        /// <param name="IndexRow">Индекс получемой строки системы</param>
        /// <param name="ColElems">Коэффициенты строки системы</param>
        /// <param name="BetaE1">Значение правой части</param>
        public override void GetStringSystem(uint IndexRow, ref double[] ColElems, ref double R)
        {
            SMatrix.GetStringSystem(IndexRow, ref ColElems);
            R = Right[IndexRow];
        }
        /// <summary>
        /// Удовлетворение ГУ
        /// </summary>
        public override void BoundConditions(double[] Conditions, uint[] Adress)
        {
            SMatrix.BoundConditions(Adress);
            for (int i = 0; i < Adress.Length; i++)
                Right[Adress[i]] = Conditions[i];
        }
        /// <summary>
        /// Выполнение ГУ
        /// </summary>
        public override void BoundConditions(double Conditions, uint[] Adress)
        {
            SMatrix.BoundConditions(Adress);
            for (int i = 0; i < Adress.Length; i++)
                Right[Adress[i]] = Conditions;
        }
        /// <summary>
        /// Операция определения невязки BetaE1 = Matrix X - Right
        /// </summary>
        /// <param name="BetaE1">результат</param>
        /// <param name="X">умножаемый вектор</param>
        /// <param name="IsRight">знак операции = +/- 1</param>
        public override void GetResidual(ref double[] Result, double[] X, int IsRight = 1)
        {
            SMatrix.MatrixCRS_Mult(ref Result, X);
            for (int i = 0; i < SMatrix.N; i++)
                Result[i] = Result[i] - Right[i];
        }
        /// <summary>
        /// Решение СЛУ
        /// </summary>
        public override void Solve(ref double[] X)
        {
            MEM.Alloc<double>((int)FN, ref X);
            base.Solve(ref X);
        }
        /// <summary>
        /// нормировка системы для малых значений матрицы и правой части
        /// </summary>
        protected override void SystemNormalization()
        {
            // максимальное значение в правой части
            double maxR = Right.Max(x => Math.Abs(x));
            // максимальное значение в матрице
            double maxM = SMatrix.Max();
            if (maxM > maxR)
            {
                if (maxM < 1 && maxM > 0)
                {
                    double v = (int)Math.Abs(Math.Log10(maxM));
                    double valNorm = Math.Pow(10, v + 2);
                    for (int i = 0; i < FN; i++)
                    {
                        Right[i] *= valNorm;
                        SMatrix.Normalization(i, valNorm);
                    }
                }
            }
        }
        #endregion
    }
}
