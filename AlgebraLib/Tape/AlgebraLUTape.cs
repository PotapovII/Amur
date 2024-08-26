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
    public class AlgebraLUTape : Algebra
    {
        /// <summary>
        /// глобальная матрица жесткости (ГМЖ)
        /// </summary>
        protected double[][] Matrix;
        /// <summary>
        /// Поддиагональные элементы множителя L для LU разложения
        /// в компактной форме возвращаются в массиве 
        /// </summary>
        protected double[][] LMatrix;
        /// <summary>
        ///  вектор перестановок
        /// </summary>
        protected int[] p;
        /// <summary>
        /// Ширина ленты ГМЖ
        /// </summary>
        protected int FH { get { return (FHL + FHR + 1); } }
        /// <summary>
        /// Шириной нижней полуленты 
        /// </summary>
        protected int FHL;
        /// <summary>
        /// Шириной верхней полуленты 
        /// </summary>
        protected int FHR;
        /// <summary>
        /// Выделение памяти под систему
        /// </summary>
        public AlgebraLUTape(uint NN, int FHL, int FHR) : base(new AlgebraResultDirect(FHL, FHR), NN)
        {
            name = "Метод LU разложения с выбором ведущего элемента для летночной матрицы";
            this.FHL = FHL;
            this.FHR = FHR;
            SetAlgebra(result, NN);
        }
        public override void SetAlgebra(IAlgebraResult res, uint NN)
        {
            try
            {
                if (NN != FN)
                    base.SetAlgebra(res, NN);
                result.Name = name;
                MEM.Alloc2D<double>((int)FN, FH, ref Matrix);
                MEM.Alloc2D<double>((int)FN, FHL, ref LMatrix);
                MEM.Alloc<int>((int)FN, ref p);
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
            // ф-я сборки глобальной матрицы жесткости
            int i, j, nv, nh;
            for (i = 0; i < Size; i++)
            {
                nv = (int)Adress[i];
                for (j = 0; j < Size; j++)
                {
                    nh = (int)Adress[j] - nv + FHL;
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
                nh = (int)Adress[j] - nv + FHL;
                if (nh >= 0)
                    Matrix[nv][nh] += ColElems[j];
            }
            Right[IndexRow] = R;
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
                    int top = ad - FHR > 0 ? (ad - FHR) : 0;
                    int bot = ad + FHL < FN - 1 ? ad + FHL : (int)FN - 1;
                    for (int i = top; i < bot; i++)
                    {
                        int jp = ad - i + FHL;
                        Right[i] -= Matrix[i][jp] * Conditions[curKnot];
                        Matrix[i][jp] = 0;
                    }
                    for (int i = 0; i < FH; i++)
                        Matrix[ad][i] = 0;
                    Matrix[ad][FHL] = 1;
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
                int top = ad - FHR > 0 ? (ad - FHR) : 0;
                int bot = ad + FHL < FN - 1 ? ad + FHL : (int)FN - 1;
                for (int i = top; i < bot; i++)
                {
                    int jp = ad - i + FHL;
                    Right[i] -= Matrix[i][jp] * Conditions;
                    Matrix[i][jp] = 0;
                }
                for (int i = 0; i < FH; i++)
                    Matrix[ad][i] = 0;
                Matrix[ad][FHL] = 1;
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
                begin = i > FHL ? 0 : FHL - i;
                end = (int)(i < FN - FHR ? FHL + FHR : FN + FHL - i - 1);
                k = i - FHL;
                AX[i] = 0.0;
                for (int j = begin; j <= end; j++)
                    AX[i] += Matrix[i][j] * X[j + k];
            }
        }

        /// <summary>
        /// Функция находит LU - разложение PA = LU ленточной квадратной матрицы Matrix порядка FN 
        /// с шириной нижней полуленты FHL и шириной верхней полуленты FHR. 
        /// Для хранения матрицы используется компактная схема. 
        /// Элементы матрицы хранятся в массиве Matrix размера [FN , (FHL+FHR+1)]. 
        /// Используется схема выбора главного элемента по столбцу. 
        /// Элементы множителя U возвращаются в компактной форме в массиве Matrix. 
        /// Поддиагональные элементы множителя L в компактной форме возвращаются в массиве 
        /// LMatrix размера [FN,  FHL]. 
        /// Диагональные элементы матрицы LMatrix равны 1. 
        /// Матрица перестановок P представлена вектором перестановок p:
        /// на j-й итерации j-я строка была переставлена с p[j]-й.
        /// На выходе sgn - определитель матрицы P.
        /// </summary>
        /// <param name="sgn"></param>
        public void LU_Diccomposition(ref int sgn)
        {
            int i, j, k, l, s;
            double dum;
            // Перестраиваем матрицу                                                        
            l = FHL;
            for (i = 0; i < FHL; i++)
            {
                for (j = FHL - i; j < FH; j++)
                    Matrix[i][j - l] = Matrix[i][j];
                l--;
                for (j = FH - l - 1; j < FH; j++)
                    Matrix[i][j] = 0.0;
            }

            sgn = 1;
            l = FHL;

            // Для каждой строки...                                                       
            for (k = 0; k < FN; k++)
            {
                dum = Matrix[k][0]; /* Matrix(k, 0) */
                s = k;
                if (l < FN) l++;

                // Ищем главный элемент
                for (j = k + 1; j < l; j++)
                {
                    if (Math.Abs(Matrix[j][0]) > Math.Abs(dum)) /* Matrix(j, 0) */
                    {
                        dum = Matrix[j][0];
                        s = j;
                    }
                }
                p[k] = s;

                // Переставляем строки
                if (s != k)
                {
                    sgn = -sgn;
                    for (j = 0; j < FH; j++)
                    {
                        dum = Matrix[k][j];
                        Matrix[k][j] = Matrix[s][j];
                        Matrix[s][j] = dum;
                    }
                }

                // Гауссово исключение
                for (i = k + 1; i < l; i++)
                {
                    dum = Matrix[i][0] / Matrix[k][0];
                    LMatrix[k][i - k - 1] = dum;
                    for (j = 1; j < FH; j++)
                        Matrix[i][j - 1] = Matrix[i][j] - dum * Matrix[k][j];
                    Matrix[i][FH - 1] = 0.0;
                }
            }
        }
        public void band_solve(ref double[] X)
        {
            int i, k, l;
            double dum;
            l = FHL;
            // Прямая подстановка
            for (k = 0; k < FN; k++)
            {
                i = p[k];
                if (i != k)
                {
                    dum = Right[k];
                    Right[k] = Right[i];
                    Right[i] = dum;
                }
                if (l < FN) l++;
                for (i = k + 1; i < l; i++)
                    Right[i] -= LMatrix[k][i - k - 1] * Right[k];
            }
            // Обратная подстановка
            l = 1;
            for (i = (int)FN; i > 0; i--)
            {
                dum = Right[i - 1];
                for (k = 1; k < l; k++)
                    dum -= Matrix[i - 1][k] * Right[k + i - 1];
                Right[i - 1] = dum / Matrix[i - 1][0];  /* Matrix(i - 1, 0) */
                if (l < FH) l++;
            }
            for (i = 0; i < FN; i++)
                X[i] = Right[i];
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
            int sgn = 0;
            LU_Diccomposition(ref sgn);
            band_solve(ref X);
        }
        /// <summary>
        /// Клонирование объекта
        /// </summary>
        /// <returns></returns>
        public override IAlgebra Clone()
        {
            return new AlgebraLUTape(FN, FHL, FHR);
        }
        /// <summary>
        /// Вывод САУ на КОНСОЛЬ
        /// </summary>
        public override void Print(int flag = 0)
        {
            double V = 0;
            int dFH = FH;
            Console.WriteLine("Matrix");
            for (int i = 0; i < FN; i++)
            {
                for (int j = 0; j < FN; j++)
                {
                    int jp = j - i + FHL;
                    if (jp < FH && jp > -1)
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

