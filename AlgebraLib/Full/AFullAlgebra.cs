//---------------------------------------------------------------------------
//                     - (C) Copyright 2019 -
//                       ALL RIGHT RESERVED
//                            11 08 2019        
//                    реализация: Потапов И.И.
//---------------------------------------------------------------------------
//                     - (C) Copyright 2019 -
//                       ALL RIGHT RESERVED
//                            01 08 2021        
//                       правка: Потапов И.И.
//---------------------------------------------------------------------------
namespace AlgebraLib
{
    using CommonLib;
    using MemLogLib;
    using System;
    using System.Linq;

    /// <summary>
    /// ОО: Абстрактный класс для СЛАУ исполльзующих полную прямоугольную матрицц
    /// </summary>
    public abstract class AFullAlgebra : Algebra
    {
        /// <summary>
        /// матрица системы
        /// </summary>
        protected double[][] Matrix;
        public AFullAlgebra(AlgebraResult a = null, uint NN = 1)
        {
            SetAlgebra(a, NN);
        }
        /// <summary>
        /// Решение САУ
        /// </summary>
        /// <param name="X">Вектор в который возвращается результат решения СЛАУ</param>
        public override void Solve(ref double[] X)
        {
            SystemNormalization();
            base.Solve(ref X);
        }
        /// <summary>
        /// нормировка системы для малых значений матрицы и правой части
        /// </summary>
        protected override void SystemNormalization()
        {
            // максимальное значение в правой части
            double maxR = Right.Max(x => Math.Abs(x));
            // максимальное по модулю значение в матрице
            double maxM = Matrix[0][0];
            for (int i = 0; i < FN; i++)
                maxM = Math.Max(maxM, Matrix[i].Max(x => Math.Abs(x)));
            if (maxM > maxR)
            {
                if (maxM < 1 && maxM > 0)
                {
                    double v = (int)Math.Abs(Math.Log10(maxM));
                    double valNorm = Math.Pow(10, v + 2);
                    for (int i = 0; i < FN; i++)
                    {
                        Right[i] *= valNorm;
                        for (int j = 0; j < FN; j++)
                            Matrix[i][j] *= valNorm;
                    }
                }
            }
        }
        /// <summary>
        /// [1] Определение ранга САУ
        /// </summary>
        /// <param name="N">Порядок системы</param>
        public override void SetAlgebra(IAlgebraResult a = null, uint NN = 1)
        {
            try
            {
                base.SetAlgebra(a, NN);
                MEM.Alloc2D<double>((int)FN, (int)FN, ref Matrix);
            }
            catch (Exception ex)
            {
                result.errorType = ErrorType.outOfMemory;
                result.Message = ex.Message;
            }
        }
        /// <summary>
        /// Очистка системы
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            for (int i = 0; i < FN; i++)
                for (int j = 0; j < FN; j++)
                    Matrix[i][j] = 0;
        }
        /// <summary>
        ///  Формирование матрицы системы
        /// </summary>
        /// <param name="LMartix">Локальная матрица жесткости КЭ аналога</param>
        /// <param name="Adress">Глобальные адреса матрицы связности для текущего конечного элемента</param>
        public override void AddToMatrix(double[][] LMartix/*[in]*/, uint[] Adress/*[in]*/)
        {
            for (int a = 0; a < Adress.Length; a++)
                for (int b = 0; b < Adress.Length; b++)
                    Matrix[Adress[a]][Adress[b]] += LMartix[a][b];
        }
        /// <summary>
        /// Выполнение граничных условий
        /// </summary>
        /// <param name="Conditions">Значения переменных</param>
        /// <param name="Adress">Адреса переменных с граничными условиями</param>
        public override void BoundConditions(double[] Conditions, uint[] Adress)
        {
            SystemNormalization();
            uint ad, curKnot;
            int Count = Adress.Length;
            for (curKnot = 0; curKnot < Count; curKnot++)
            {
                ad = Adress[curKnot];   // адрес граничной неизвестной
                for (int i = 0; i < FN; i++)
                {
                    // Обрабатываем столбец
                    Right[i] -= Matrix[i][ad] * Conditions[curKnot];
                    Matrix[i][ad] = 0;
                    // стираем строку системы
                    Matrix[ad][i] = 0;
                }
                // ставим 1 на главной диагонали
                Matrix[ad][ad] = 1.0;
                // ставим значение ГУ по адресу (ad) в правую чать
                Right[ad] = Conditions[curKnot];
            }
        }
        /// <summary>
        /// Выполнение граничных условий
        /// </summary>
        /// <param name="Conditions">Значения переменных</param>
        /// <param name="Adress">Адреса переменных с граничными условиями</param>
        public override void BoundConditions(double Conditions, uint[] Adress)
        {
            SystemNormalization();
            uint ad, curKnot;
            int Count = Adress.Length;
            for (curKnot = 0; curKnot < Count; curKnot++)
            {
                ad = Adress[curKnot];   // адрес граничной неизвестной
                for (int i = 0; i < FN; i++)
                {
                    // Обрабатываем столбец
                    Right[i] -= Matrix[i][ad] * Conditions;
                    Matrix[i][ad] = 0;
                    // стираем строку системы
                    Matrix[ad][i] = 0;
                }
                // ставим 1 на главной диагонали
                Matrix[ad][ad] = 1.0;
                // ставим значение ГУ по адресу (ad) в правую чать
                Right[ad] = Conditions;
            }
        }
        /// <summary>
        /// Сборка САУ по строкам !!!
        /// </summary>
        /// <param name="ColElems">Коэффициенты системы</param>
        /// <param name="ColAdress">Адреса коэффицентов</param>
        /// <param name="IndexRow">Индекс формируемой строки системы</param>
        /// <param name="R">Значение правой части строки</param>
        public override void AddStringSystem(double[] ColElems, uint[] ColAdress, uint IndexRow, double R)
        {
            for (int i = 0; i < ColAdress.Length; i++)
                Matrix[IndexRow][ColAdress[i]] = ColElems[i];
            Right[IndexRow] = R;
        }
        /// <summary>
        /// Получить строку (не для всех решателей)
        /// </summary>
        /// <param name="IndexRow">Индекс получемой строки системы</param>
        /// <param name="ColElems">Коэффициенты строки системы</param>
        /// <param name="R">Значение правой части</param>
        public override void GetStringSystem(uint IndexRow, ref double[] ColElems, ref double R)
        {
            MEM.Alloc(FN, ref ColElems);
            for (int j = 0; j < FN; j++)
                ColElems[j] = Matrix[IndexRow][j];
            R = Right[IndexRow];
        }
        /// <summary>
        /// Умножение вектора на матрицу
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Q"></param>
        protected void matrixVectorMult(double[] X, ref double[] Q)
        {
            for (int i = 0; i < FN; i++)
            {
                Q[i] = 0;
                for (int idx = 0; idx < Matrix[i].Length; idx++)
                    Q[i] += Matrix[i][idx] * X[idx];
            }
        }

        /// <summary>
        /// Операция определения невязки R = Matrix X - Right
        /// </summary>
        /// <param name="R">невязка</param>
        /// <param name="X">решение</param>
        /// <param name="IsRight"></param>
        public override void getResidual(ref double[] R, double[] X, int IsRight = 1)
        {
            if (R == null)
                R = new double[FN];
            for (uint i = 0; i < FN; i++)
            {
                R[i] = 0;
                for (uint j = 0; j < FN; j++)
                    R[i] += Matrix[i][j] * X[j];
                R[i] -= IsRight * Right[i];
            }
        }
        // [10]
        /// <summary>
        /// Умножение коэффициента C на ведомою подстроку и правую часть
        /// </summary>
        /// <param name="k"></param>
        /// <param name="C"></param>
        protected void Calibrate(int k, double C)
        {
            double CC = 1 / C;
            for (int i = k; i < FN; i++)
            {
                for (int j = k; j < FN; j++) Matrix[i][j] *= CC;
                Right[i] *= CC;
            }
        }
        /// <summary>
        /// Вывод САУ на КОНСОЛЬ
        /// </summary>
        /// <param name="flag">количество знаков мантисы</param>
        /// <param name="color">длина цветового блока</param>
        public override void Print(int flag = 0, int color = 1)
        {
            LOG.Print("Matrix", Matrix, flag, color);
            LOG.Print("Right", Right, flag);
        }
    }
}
