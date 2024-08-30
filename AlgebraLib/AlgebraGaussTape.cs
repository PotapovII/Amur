//---------------------------------------------------------------------------
//   Реализация библиотеки методов решения систем алгебраических уравнений
//                     - (C) Copyright 2019 -
//                       ALL RIGHT RESERVED
//                    разработка: Потапов И.И.
//---------------------------------------------------------------------------
//      Источник кода: (кусок листинга с кодом FORTRAN 77:)
//                  (моя стажировка ЛПИ, 1987)
//---------------------------------------------------------------------------
//                       ПРОЕКТ  "shell"
//    кодировка : 15.09.1988 Потапов И.И. перенесен с FORTRAN 77 на PL/2 
//---------------------------------------------------------------------------
//                  ПРОЕКТ "solid rocket engine"
//    кодировка : 15.09.1991 Потапов И.И. перенесен с PL/2 на C++
//---------------------------------------------------------------------------
//                      ПРОЕКТ  "RiverProfile"
//    кодировка :   06.12.2006 Потапов И.И. перенесен с C++ на C#
//---------------------------------------------------------------------------
//                  ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//      правка  :   06.12.2019 Потапов И.И. перенесен с C#
//---------------------------------------------------------------------------
//                  ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//             31.07.2021 Потапов И.И. перенесен с C#
//                добавлен AlgebraResult Result
//---------------------------------------------------------------------------
namespace AlgebraLib
{
    using MemLogLib;
    using System;
    using System.Linq;
    using CommonLib;
    /// <summary>
    ///  Определение класса AlgebraGaussTape 
    ///  класс AlgebraGaussTape для решения системы линейных алгебраических уравнений 
    ///  методом Гаусса для симметричной ленточной матрицы системы 
    ///  В случае решения одномерной задачи линейными КЭ имеет
    ///  эффективность метода прогонки (в некотором смысле эквивалентна ему)
    /// </summary>
    public class AlgebraGaussTape : Algebra
    {
        /// <summary>
        /// глобальная матрица жесткости (ГМЖ)
        /// </summary>
        protected double[][] Matrix;
        /// <summary>
        /// Ширина ленты ГМЖ
        /// </summary>
        protected uint FH;
        /// <summary>
        /// порядок СЛУ
        /// </summary>
        /// <summary>
        /// Выделение памяти под систему
        /// </summary>
        public AlgebraGaussTape(uint NN, uint FH) : base(new AlgebraResultDirect(0, (int)FH), NN)
        {
            name = "Метод Гаусса для симметричной летночной матрицы";
            this.FH = FH;
            SetAlgebra(result, NN);
        }

        public override void SetAlgebra(IAlgebraResult res, uint NN)
        {
            try
            {
                if (NN != FN)
                    base.SetAlgebra(res, NN);
                result.Name = name;
                MEM.Alloc2D<double>((int)FN, (int)FH, ref Matrix);
            }
            catch (Exception ex)
            {
                result.errorType = ErrorType.outOfMemory;
                result.Message = ex.Message;
            }
        }
        /// <summary>
        /// Очистка матрицы и правой части
        /// </summary>
        public override void Clear()
        {
            for (int i = 0; i < FN; i++)
            {
                for (int j = 0; j < FH; j++)
                    Matrix[i][j] = 0;
                Right[i] = 0;
            }
        }
        /// <summary>
        /// Сборка ГМЖ
        /// </summary>
        public override void AddToMatrix(double[][] LMartix, uint[] Adress)
        {
            int Size = LMartix.Length;
            // ф-я сборки глобальной симетричной матрицы жесткости
            int i, j, nv, nh;
            for (i = 0; i < Size; i++)
            {
                nv = (int)Adress[i];
                for (j = 0; j < Size; j++)
                {
                    nh = (int)Adress[j] - nv;
                    if (nh >= 0)
                        Matrix[nv][nh] += LMartix[i][j];
                }
            }
        }
        /// <summary>
        /// Сборка САУ по строкам
        /// </summary>
        /// <param name="ColElems">Коэффициенты системы</param>
        /// <param name="ColAdress">Адреса коэффицентов</param>
        /// <param name="IndexRow">Индекс формируемой строки системы</param>
        /// <param name="Right">Значение правой части строки</param>
        public override void AddStringSystem(double[] ColElems, uint[] Adress, uint IndexRow, double R)
        {
            int nh;
            int nv = (int)IndexRow;
            for (int j = 0; j < Adress.Length; j++)
            {
                nh = (int)Adress[j] - nv;
                if (nh >= 0)
                    Matrix[nv][nh] += ColElems[j];
            }
            Right[IndexRow] = R;
        }
        /// <summary>
        /// Удовлетворение ГУ
        /// </summary>
        public override void BoundConditions(double[] Conditions, uint[] Adress)
        {
            SystemNormalization();
            double Ves = 1.0e30;
            uint ad, curKnot;
            for (curKnot = 0; curKnot < Adress.Length; curKnot++)
            {
                ad = Adress[curKnot];   // адрес граничной неизвестной
                Right[ad] = Conditions[curKnot] * Ves;
                Matrix[ad][0] = Ves;
            }
        }
        /// <summary>
        /// Выполнение ГУ
        /// </summary>
        /// <param name="Conditions"></param>
        /// <param name="Adress"></param>
        /// <param name="Count"></param>
        public override void BoundConditions(double Conditions, uint[] Adress)
        {
            SystemNormalization();
            double Ves = 1.0e30;
            uint ad, curKnot;
            for (curKnot = 0; curKnot < Adress.Length; curKnot++)
            {
                ad = Adress[curKnot];   // адрес граничной неизвестной
                Right[ad] = Conditions * Ves;
                Matrix[ad][0] = Ves;
            }
        }
        /// <summary>
        /// Операция определения невязки R = Matrix X - Right
        /// </summary>
        public override void getResidual(ref double[] R, double[] X, int IsRight)
        {
            if (R == null)
                R = new double[FN];
            //  ф-я перемножения симметричной ленточной матрицы на столбец
            //  Matrix - матрица системы, H - ширина ленты, FN - порядок матрицы
            int m;
            double s;
            int FNN = (int)FN;
            int FHH = (int)FH;
            for (int i = 0; i < FN; i++)
            {
                s = 0;
                // столбец
                if (i > 0)
                {
                    if (i < FHH)
                        m = i + 1;
                    else
                        m = FHH;
                    for (int k = 1; k < m; k++)
                        s += Matrix[i - k][k] * X[i - k];
                }
                if (FNN - i < FHH)
                    m = FNN - i;
                else
                    m = FHH;
                // строка
                for (uint k = 0; k < m; k++)
                    s += Matrix[i][k] * X[k + i];
                R[i] = s - IsRight * Right[i];
            }
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
                        for (int j = 0; j < FH; j++)
                            Matrix[i][j] *= valNorm;
                    }
                }
            }
        }
        public override void Solve(ref double[] X)
        {
            SystemNormalization();
            base.Solve(ref X);
        }
        /// <summary>
        /// Решение СЛУ
        /// </summary>
        protected override void solve(ref double[] X)
        {
            //int Key = 0; // Флаг для могократного обратного хода (не используется в данной версии)
            int FNN = (int)FN;
            int FHH = (int)FH;
            MEM.Alloc(FNN, ref X, "X");
            double c, ai;
            int i;
            int m, j, l, k;
            //if(Key==0)
            {
                /* ------ прямой ход -------- */
                for (i = 0; i < FN; i++)
                {
                    ai = Matrix[i][0];
                    m = FHH; if (FN - i < FH) 
                        m = FNN - i;
                    for (j = 1; j < m; j++)
                    {
                        c = Matrix[i][j] / ai;
                        l = i + j;
                        for (k = 0; k < m - j; k++)
                        {
                            Matrix[l][k] -= c * Matrix[i][k + j];
                        }
                    }
                }
            }
            // повторный прямой ход
            for (i = 0; i < FN; i++)
            {
                ai = Matrix[i][0];
                if (FN - i < FH) m = FNN - i;
                else m = FHH;
                for (j = 1; j < m; j++)
                {
                    c = Matrix[i][j] / ai;
                    l = i + j;
                    Right[l] -= c * Right[i];
                }
            }
            // Обратный ход
            for (i = FNN - 1; i >= 0; i--)
            {
                ai = Matrix[i][0];
                if (FN - i < FH)
                    m = FNN - i;
                else
                    m = FHH;
                c = 0;
                for (j = 1; j < m; j++)
                    c += Matrix[i][j] * X[i + j];

                X[i] = (Right[i] - c) / ai;
            }
            Result.X = X;
        }

        /// <summary>
        /// Клонирование объекта
        /// </summary>
        /// <returns></returns>
        public override IAlgebra Clone()
        {
            return new AlgebraGaussTape(FN, FH);
        }
        /// <summary>
        /// Вывод САУ на КОНСОЛЬ
        /// </summary>
        public override void Print(int flag = 0)
        {
            for (int i = 0; i < FN; i++)
            {
                Console.WriteLine("Matrix");
                for (int j = 0; j < FH; j++)
                    Console.Write(" " + Matrix[i][j].ToString("F4"));
                Console.WriteLine();
            }
            Console.WriteLine("Right");
            for (int i = 0; i < FN; i++)
                Console.Write(" " + Right[i].ToString("F4"));
            Console.WriteLine();
        }
    }
}

