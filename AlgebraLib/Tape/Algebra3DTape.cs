//---------------------------------------------------------------------------
//   Реализация библиотеки методов решения систем алгебраических уравнений
//                     - (C) Copyright 2019 -
//                       ALL RIGHT RESERVED
//                    разработка: Потапов И.И.
//                      01.08.2021 Потапов И.И. 
//---------------------------------------------------------------------------
namespace AlgebraLib
{
    using MemLogLib;
    using System;
    using System.Linq;
    using CommonLib;
    /// <summary>
    ///  ОО: Класс AlgebraBandGauss для решения системы линейных алгебраических уравнений 
    ///  методом Гаусса для несимметричной ленточной матрицы системы 
    /// </summary>
    public class Algebra3DTape : Algebra
    {
        /// <summary>
        /// вектор диагонали
        /// </summary>
        double[] C;
        /// <summary>
        /// вектор правой части
        /// </summary>
        double[] R;
        double[] X;
        double[][] Matrix;


        /// <summary>
        /// Выделение памяти под систему
        /// </summary>
        public Algebra3DTape(uint NN) : base(new AlgebraResultDirect(1, 1), NN)
        {
            name = "Tri-Diagonal прогонка";
            SetAlgebra(result, NN);
        }
        public override void SetAlgebra(IAlgebraResult res, uint NN)
        {
            try
            {
                if (NN != FN)
                    base.SetAlgebra(res, NN);
                result.Name = name;

                MEM.Alloc((int)FN, ref C);
                MEM.Alloc((int)FN, ref R);
                MEM.Alloc((int)FN, ref X);
                MEM.Alloc2D<double>((int)FN, 3, ref Matrix);
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
                C[i] = 0;
                R[i] = 0;
                X[i] = 0;
                Right[i] = 0;
            }
        }
        /// <summary>
        /// Сборка ГМЖ
        /// </summary>
        public override void AddToMatrix(double[][] LMartix, uint[] Adress)
        {
            int Size = LMartix.Length;
            // ф-я сборки глобальной матрицы жесткости
            int i, j, nv, nh;
            for (i = 0; i < Size; i++)
            {
                nv = (int)Adress[i];
                for (j = 0; j < Size; j++)
                {
                    nh = (int)Adress[j] - nv + 1;
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
                nh = (int)Adress[j] - nv + 1;
                if (nh >= 0)
                    Matrix[nv][nh] += ColElems[j];
            }
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
            int i = (int)IndexRow;
            for (int j = 0; j < FN; j++)
            {
                int jp = j - i + 1;
                if (jp < 3 && jp > -1)
                    ColElems[j] = Matrix[i][jp];
            }
            R = Right[IndexRow];
        }
        /// <summary>
        /// Удовлетворение ГУ приближенное
        /// </summary>
        public override void BoundConditions(double[] Conditions, uint[] Adress)
        {
            //if (false)
            //{
            //    // тоже работает
            //    double Ves = 1.0e30;
            //    uint ad, curKnot;
            //    for (curKnot = 0; curKnot < Adress.Length; curKnot++)
            //    {
            //        ad = Adress[curKnot];   // адрес граничной неизвестной
            //        Right[ad] = Conditions[curKnot] * Ves;
            //        Matrix[ad][FHL] = Ves;
            //    }
            //}
            //else
            {
                int ad, curKnot;
                for (curKnot = 0; curKnot < Adress.Length; curKnot++)
                {
                    ad = (int)Adress[curKnot];   // адрес граничной неизвестной
                    int top = ad - 1 > 0 ? (ad - 1) : 0;
                    int bot = ad + 1 < FN - 1 ? ad + 1 : (int)FN - 1;
                    for (int i = top; i < bot; i++)
                    {
                        int jp = ad - i + 1;
                        Right[i] -= Matrix[i][jp] * Conditions[curKnot];
                        Matrix[i][jp] = 0;
                    }
                    for (int i = 0; i < 3; i++)
                        Matrix[ad][i] = 0;
                    Matrix[ad][1] = 1;
                    Right[ad] = Conditions[curKnot];
                }
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
            int ad, curKnot;
            for (curKnot = 0; curKnot < Adress.Length; curKnot++)
            {
                ad = (int)Adress[curKnot];   // адрес граничной неизвестной
                int top = ad - 1 > 0 ? (ad - 1) : 0;
                int bot = ad + 1 < FN - 1 ? ad + 1 : (int)FN - 1;
                for (int i = top; i < bot; i++)
                {
                    int jp = ad - i + 1;
                    Right[i] -= Matrix[i][jp] * Conditions;
                    Matrix[i][jp] = 0;
                }
                for (int i = 0; i < 3; i++)
                    Matrix[ad][i] = 0;
                Matrix[ad][1] = 1;
                Right[ad] = Conditions;
            }
        }

        /// <summary>
        /// Ленточная матрица A, умножается на вектор-столбец X. 
        /// Результат возвращается в массиве AX.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="AX"></param>
        public void band_mult_col(double[] X, ref double[] AX)
        {
            int k, begin, end;
            for (int i = 0; i < FN; i++)
            {
                begin = i > 1 ? 0 : 1;
                end = (int)(i < FN - 1 ? 2 : 1);
                k = i - 1;
                AX[i] = 0.0;
                for (int j = begin; j <= end; j++)
                    AX[i] += Matrix[i][j] * X[j + k];
            }
        }
        /// <summary>
        /// Операция определения невязки R = Matrix X - Right
        /// </summary>
        public override void getResidual(ref double[] R, double[] X, int IsRight)
        {
            MEM.Alloc<double>((int)FN, ref R);
            band_mult_col(X, ref R);
            for (int i = 0; i < FN; i++)
                R[i] -= IsRight * Right[i];
        }
        /// <summary>
        /// нормировка системы для малых значений матрицы и правой части
        /// </summary>
        protected override void SystemNormalization()
        {
            
            double valNorm,v;
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
                    v = (int)Math.Abs(Math.Log10(maxM));
                    valNorm = Math.Pow(10, v + 2);
                    for (int i = 0; i < FN; i++)
                    {
                        Right[i] *= valNorm;
                        for (int j = 0; j < 3; j++)
                            Matrix[i][j] = valNorm * Matrix[i][j];
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
            int flag = -1;
            int imax = C.Length;
            C[0] = Matrix[0][1];
            R[0] = Right[0];
            // прямой ход для вычисления коэффициентов С и R 
            for (int i = 1; i < imax; i++)
            {
                C[i] = Matrix[i][1] - Matrix[i][0] * Matrix[i - 1][2] / C[i - 1];
                R[i] = Right[i] - flag * Matrix[i][0] * R[i - 1] / C[i - 1];
            }
            // обратный ход
            X[imax - 1] = R[imax - 1] / C[imax - 1];
            for (int i = imax - 2; i > -1; i--)
                X[i] = (R[i] - flag * X[i + 1] * Matrix[i][2]) / C[i];
        }
        /// <summary>
        /// Клонирование объекта
        /// </summary>
        /// <returns></returns>
        public override IAlgebra Clone()
        {
            return new Algebra3DTape(FN);
        }
        /// <summary>
        /// Вывод САУ на КОНСОЛЬ
        /// </summary>
        /// <param name="flag">количество знаков мантисы</param>
        /// <param name="color">длина цветового блока</param>
        public override void Print(int flag = 0, int color = 1)
        {
            double V = 0;
            Console.WriteLine("Matrix");
            for (int i = 0; i < FN; i++)
            {
                for (int j = 0; j < FN; j++)
                {
                    int jp = j - i + 1;
                    if (jp < 3 && jp > -1)
                        Console.Write(" " + Matrix[i][jp].ToString("F4"));
                    else
                        Console.Write(" " + V.ToString("F4"));
                }
                Console.WriteLine();
            }
            Console.WriteLine("Right");
            for (int i = 0; i < FN; i++)
                Console.Write(" " + Right[i].ToString("F4"));
            Console.WriteLine();
        }
    }
}

