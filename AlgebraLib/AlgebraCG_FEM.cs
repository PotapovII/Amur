//---------------------------------------------------------------------------
//   Реализация библиотеки методов решения систем алгебраических уравнений
//                     - (C) Copyright 2021 -
//                       ALL RIGHT RESERVED
//                    разработка: Потапов И.И.
//---------------------------------------------------------------------------
//                 Home: HConjuageGradient
//                  - (C) Copyright 2002
//                        Потапов И.И.
//                         30.05.02
//---------------------------------------------------------------------------
//          Решение САУ методом сопряженных градиентов
//---------------------------------------------------------------------------
//                  C++ => C#   15.04.2021
//          HConjuageGradient = > AlgebraConjGrad
//---------------------------------------------------------------------------
namespace AlgebraLib
{
    using MemLogLib;
    using System;
    using CommonLib;
    /// <summary>
    /// ОО: Хранение ЛМЖ и глобальных адресов КЭ
    /// </summary>
    public class HLMatrix
    {
        public uint Count { get => (uint)adress.Length; }

        public uint[] adress = null;
        public double[][] LMartix = null;
        public HLMatrix(double[][] LMartix, uint[] adress)
        {
            this.adress = new uint[adress.Length];
            this.LMartix = new double[adress.Length][];
            for (int i = 0; i < adress.Length; i++)
            {
                this.adress[i] = adress[i];
                this.LMartix[i] = new double[adress.Length];
                for (int j = 0; j < adress.Length; j++)
                    this.LMartix[i][j] = LMartix[i][j];
            }
        }
    }
    /// <summary>
    /// Реализация МСГ 
    /// </summary>
    public class AlgebraCG_FEM : Algebra
    {
        /// <summary>
        /// <summary>
        /// Очистка матрицы и правой части
        /// </summary>
        public override void Clear()
        {
            base.Clear();
        }
        protected uint CountElem;
        protected int CountMatrix = 0;
        protected HLMatrix[] Matrixs;    // матрица системы
        protected double[] R;           // вектор ошибки
        protected double[] P;           // сопряженный вектор
        protected double[] AP;          // произведение сопряженного вектора на матрицу
        protected bool Sumetry;             // флаг приведения матрицы системы к симметричному виду
                                            // Граничные
        protected double[] BnConditions;
        protected uint[] BnAdress;
        protected short[] CkBoundAdress;

        public AlgebraCG_FEM(uint N, uint CountElem)
        {
            this.CountElem = CountElem;
            name = "Метод сопряженных градиентов для КЭ матрицы в виде ЛМЖ";
            SetAlgebra(new AlgebraResultIterative(), N);
        }
        /// <summary>
        /// Выделение рабочих массивов
        /// </summary>
        /// <param name="N">размерность СЛАУ</param>
        public override void SetAlgebra(IAlgebraResult res, uint NN)
        {
            try
            {
                if (NN != FN)
                    base.SetAlgebra(new AlgebraResultIterative(), NN);
                result.Name = name;
                CountMatrix = 0;
                Matrixs = new HLMatrix[CountElem];
                MEM.AllocClear(FN, ref P);
                MEM.AllocClear(FN, ref R);
                MEM.AllocClear(FN, ref AP);
            }
            catch (Exception ex)
            {
                result.errorType = ErrorType.outOfMemory;
                result.Message = ex.Message;
            }
        }
        /// <summary>
        /// Умножение вектора на КЭ матрицы
        /// </summary>
        /// <param name="X">результат</param>
        /// <param name="Value">умножаемый вектор</param>
        /// <returns></returns>
        public double[] getResidual(double[] X, double[] Value, int IsRight = 1)
        {
            for (int i = 0; i < X.Length; i++)
                X[i] = 0;

            //Parallel.For(0, Matrixs.Length, i =>
            //{
            //    uint Count = Matrixs[i].Count;
            //    for (uint a = 0; a < Count; a++)
            //    {
            //        uint aa = Matrixs[i].adress[a];
            //        if (CkBoundAdress[aa] == 1) continue;
            //        for (uint b = 0; b < Count; b++)
            //        {
            //            uint bb = Matrixs[i].adress[b];
            //            X[aa] += Matrixs[i].LMartix[a][b] * Value[bb];
            //        }
            //    }
            //});

            for (int i = 0; i < Matrixs.Length; i++)
            {
                uint Count = Matrixs[i].Count;
                for (uint a = 0; a < Count; a++)
                {
                    uint aa = Matrixs[i].adress[a];
                    if (CkBoundAdress[aa] == 1) continue;
                    for (uint b = 0; b < Count; b++)
                    {
                        uint bb = Matrixs[i].adress[b];
                        X[aa] += Matrixs[i].LMartix[a][b] * Value[bb];
                    }
                }
            }
            return X;
        }


        /// <summary>
        /// Умножение вектора на КЭ матрицы
        /// </summary>
        /// <param name="X">результат</param>
        /// <param name="Value">умножаемый вектор</param>
        /// <returns></returns>
        public override void getResidual(ref double[] X, double[] Value, int IsRight = 1)
        {
            for (int i = 0; i < X.Length; i++)
                X[i] = 0;
            for (int i = 0; i < Matrixs.Length; i++)
            {
                uint Count = Matrixs[i].Count;
                for (uint a = 0; a < Count; a++)
                {
                    uint aa = Matrixs[i].adress[a];
                    if (CkBoundAdress[aa] == 1) continue;
                    for (uint b = 0; b < Count; b++)
                    {
                        uint bb = Matrixs[i].adress[b];
                        X[aa] += Matrixs[i].LMartix[a][b] * Value[bb];
                    }
                }
            }
        }
        //
        protected void SetCondition(ref double[] Value)
        {
            for (int i = 0; i < BnConditions.Length; i++)
                Value[BnAdress[i]] = BnConditions[i];
        }
        protected double[] SumVector(double[] Value, double Al, double[] X)
        {
            //Parallel.For(0, FN, i =>
            //{
            //    X[i] += Al * Value[i];
            //});
            for (uint i = 0; i < FN; i++)
                X[i] += Al * Value[i];
            return X;
        }
        protected bool CalcX(double[] X, double Al)
        {
            double ck = 0;
            double ax = 0;
            for (int i = 0; i < FN; i++)
            {
                X[i] += Al * P[i];
                if (ck < Math.Abs(Al * P[i]))
                    ck = Math.Abs(Al * P[i]);
                if (ax < Math.Abs(X[i]))
                    ax = Math.Abs(X[i]);
            }
            if (ax > 0)
            {
                double ratioError = ck / ax;
                ((AlgebraResultIterative)result).ratioError = ratioError;
                if (ratioError < EPS)
                    return true;
            }
            else
               if (ck < EPS)
                return true;
            return false;
        }
        /// <summary>
        /// Формирование матрицы системы
        /// </summary>
        /// <param name="LMartix">Локальная матрица</param>
        /// <param name="Adress">Глабальные адреса</param>
        public override void AddToMatrix(double[][] LMartix, uint[] Adress)
        {
            HLMatrix Elem = new HLMatrix(LMartix, Adress);
            Matrixs[CountMatrix++] = Elem;
        }
        /// <summary>
        /// Формирование САУ по строкам
        /// </summary>
        /// <param name="ColElems"></param>
        /// <param name="ColAdress"></param>
        /// <param name="IndexRow"></param>
        /// <param name="R"></param>
        public override void AddStringSystem(double[] ColElems, uint[] ColAdress, uint IndexRow, double R)
        {
            result.Message = "Форма хранения матрицы не позволяет выполнять ее по строчное формирование";
            result.errorType = ErrorType.methodCannotSolveSuchSLAEs;
            throw new Exception("Метод не поддерживается для данной реализации");
        }
        /// <summary>
        /// Получить строку (не для всех решателей)
        /// </summary>
        /// <param name="IndexRow">Индекс получемой строки системы</param>
        /// <param name="ColElems">Коэффициенты строки системы</param>
        /// <param name="R">Значение правой части</param>
        public override void GetStringSystem(uint IndexRow, ref double[] ColElems, ref double R)
        {
            result.Message = "Форма хранения матрицы не позволяет выполнять ее по строчное формирование";
            result.errorType = ErrorType.methodCannotSolveSuchSLAEs;
            throw new Exception("метод GetStringSystem не реализован для класса");
        }
        /// <summary>
        /// Удовлетворение ГУ (с накопительными вызовами)
        /// </summary>
        public override void BoundConditions(double[] Conditions, uint[] Adress)
        {
            if (BnConditions == null)
            {
                BnConditions = new double[Adress.Length];
                BnAdress = new uint[Adress.Length];
                CkBoundAdress = new short[FN];
                for (uint curKnot = 0; curKnot < Adress.Length; curKnot++)
                {
                    BnConditions[curKnot] = Conditions[curKnot];
                    BnAdress[curKnot] = Adress[curKnot];
                    CkBoundAdress[Adress[curKnot]] = 1;
                }
            }
            else
            {
                int bCount = BnConditions.Length;
                double[] tmpBnConditions = new double[bCount + Adress.Length];
                uint[] tmpBnAdress = new uint[bCount + Adress.Length];
                for (uint curKnot = 0; curKnot < BnConditions.Length; curKnot++)
                {
                    tmpBnConditions[curKnot] = BnConditions[curKnot];
                    tmpBnAdress[curKnot] = BnAdress[curKnot];
                }
                for (uint curKnot = 0; curKnot < Adress.Length; curKnot++)
                {
                    tmpBnConditions[bCount + curKnot] = Conditions[curKnot];
                    tmpBnAdress[bCount + curKnot] = Adress[curKnot];
                    CkBoundAdress[Adress[curKnot]] = 1;
                }
                BnConditions = tmpBnConditions;
                BnAdress = tmpBnAdress;
            }
        }
        /// <summary>
        /// Выполнение ГУ
        /// </summary>
        public override void BoundConditions(double Conditions, uint[] Adress)
        {
            BnConditions = new double[Adress.Length];
            BnAdress = new uint[Adress.Length];
            CkBoundAdress = new short[FN];
            for (long curKnot = 0; curKnot < Adress.Length; curKnot++)
            {
                BnConditions[curKnot] = Conditions;
                BnAdress[curKnot] = Adress[curKnot];
                CkBoundAdress[Adress[curKnot]] = 1;
            }
        }
        /// <summary>
        /// нормировка системы для малых значений матрицы и правой части
        /// </summary>
        protected override void SystemNormalization()
        {
            // номировка для данного солвера не реализована
        }
        /// <summary>
        /// Решение САУ
        /// </summary>
        /// <param name="X">решение задачи</param>
        protected override void solve(ref double[] X)
        {
            double Alpha, Betta = 0, Ro = 0;
            // начальное приближение
            P = new double[FN];
            R = new double[FN];
            // граничные условия
            SetCondition(ref X);         // X0
            SetCondition(ref P);         // X0
            R = getResidual(R, P);  // R = A*X0
            R = SumVector(Right, -1, R); // R0 = A*X0 - F           // p
            // начальное приближение
            for (uint i = 0; i < FN; i++)
                P[i] = R[i];            // P0 = R0
            // цикл по сходимости
            int iters = 0;
            for (iters = 0; iters < 1000000000; iters++)
            {
                AP = getResidual(AP, P); // AP = A * P
                                         // граничные условия
                                         // AP && BCond
                for (int i = 0; i < BnConditions.Length; i++)
                    AP[BnAdress[i]] = 0;
                // минимизация ошибки решения по текущему направлению
                Ro = MultyVector(R, R);
                Alpha = -Ro / MultyVector(P, AP);
                // Модификация вектора решения и вектора ошибки
                // Проверка сходимости
                if (CalcX(X, Alpha) == true)
                    break;
                R = SumVector(AP, Alpha, R);
                // расчет нового сопряженного направления
                Betta = MultyVector(R, R) / Ro;
                //if (Betta < 0.0001 * FN) break;
                for (int i = 0; i < FN; i++)
                    P[i] = R[i] + Betta * P[i];
            }
            ((AlgebraResultIterative)result).Iterations = iters;
            ((AlgebraResultIterative)result).ratioError = Betta;
            ((AlgebraResultIterative)result).Error0_L2 = Ro;
            Result.X = X;

        }
        /// <summary>
        /// Клонирование объекта
        /// </summary>
        /// <returns></returns>
        public override IAlgebra Clone()
        {
            return new AlgebraCG_FEM(FN, CountElem);
        }
        /// <summary>
        /// Вывод САУ на КОНСОЛЬ
        /// </summary>
        /// <param name="flag">количество знаков мантисы</param>
        /// <param name="color">длина цветового блока</param>
        public override void Print(int flag = 0, int color = 1)
        {
            Console.WriteLine("Matrix");
            for (int i = 0; i < N; i++)
            {
                Console.WriteLine("А нету!!!");
                Console.WriteLine();
            }
            Console.WriteLine("Right");
            for (int i = 0; i < N; i++)
                Console.Write(" " + Right[i].ToString("F4"));
            Console.WriteLine();
        }
        public static void Test()
        {
            double[] m0 = { 1, -1 };
            double[] m1 = { -1, 1 };
            double[][] m = new double[2][];
            m[0] = m0;
            m[1] = m1;
            double[] B = { 0, 0 };
            double[] BC = { 0, 4 };

            uint[] ad = { 0, 0 };
            uint[] adb = { 0, 0 };
            uint N = 300;
            uint NE = N - 1;
            BC[1] = NE;
            double[] X = new double[N];
            AlgebraCG_FEM algebra = new AlgebraCG_FEM(N, NE);

            for (uint i = 0; i < NE; i++)
            {
                ad[0] = i; ad[1] = i + 1;
                algebra.AddToMatrix(m, ad);
                algebra.AddToRight(B, ad);
            }
            adb[0] = 0; adb[1] = NE;
            uint[] adb0 = { 0 };
            uint[] adb1 = { NE };
            double[] BC0 = { 0 };
            double[] BC1 = { NE };
            algebra.BoundConditions(BC0, adb0);
            algebra.BoundConditions(BC1, adb1);
            algebra.Solve(ref X);
            for (int j = 0; j < N; j++)
                Console.Write(" " + X[j].ToString("F4"));
            Console.Read();
        }
    }
}
