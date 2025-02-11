//---------------------------------------------------------------------------
//   Реализация библиотеки методов решения систем алгебраических уравнений
//                     - (C) Copyright 2021 -
//                       ALL RIGHT RESERVED
//                    разработка: Потапов И.И.
//---------------------------------------------------------------------------
//                    " Класс HAlgGausStr "
//                   23.10.98 Потапов И.И. C++
//---------------------------------------------------------------------------
//                            Потапов И.И.
//          HGaussAlgebraStr (C++) ==> SparseAlgebraGaussStr (C#)
//                              19.04.2021 
//---------------------------------------------------------------------------
namespace AlgebraLib
{
    using System;
    using CommonLib;
    /// <summary>
    /// ОО: Класс решатель САУ методом Гауса по строкам
    /// с хранением матрицы в формате CRS
    /// и поддержкой интерфейса алгебры для работы с КЭ задачами
    /// </summary>
    public class SparseAlgebraGaussStr : SparseAlgebra
    {
        #region Рабочии переменные

        #endregion
        public SparseAlgebraGaussStr(uint FN) : base(FN)
        {
            name = "Метод Гаусса для матрицы в формате CRS";
            result.Name = name;
        }
        /// <summary>
        /// Решение СЛУ GMRES методом
        /// </summary>
        protected override void solve(ref double[] X)
        {
            //double c;
            //// сообщение о проделанной  работе :)
            //// FSentReportMessage("прямой ход исключения по Гауссу");
            //double maxMatrix = 0;
            //for (int i = 0; i < FN; i++)
            //    for (int j = 0; j < FN; j++)
            //        if (maxMatrix < Math.Abs(Matrix[i].Row[j].Elem))
            //            maxMatrix = Math.Abs(Matrix[i].Row[j].Elem);
            //// Решение СЛУ
            //uint numSel = 1;
            //double a;
            //uint lll = 0;
            //for (uint i = 0; i < FN; i++)
            //{
            //    // сообщение о проделанной  работе :)
            //    //FSendRateMessage(i);
            //    // выборочное исключение по столбцам для поиска мах элемента
            //    uint maxElem = 0;
            //    uint maxIndex = i;
            //    uint nN = numSel + i; if (nN > FN) nN = FN;
            //    // цикл поиска ведущего элемента с
            //    // исключением по столбцу
            //    for (uint sl = i; sl < nN; sl++)
            //    {
            //        //if (sl == 27 || i == 27)
            //        //{
            //        //    lll = (uint)(sl + 33.0 / nN);
            //        //    i = i;
            //        //}
            //        //lll--;
            //        for (uint j = 0; j < i; j++)
            //        {
            //            if (Math.Abs(Matrix[sl][j]) < 1.e - 12) continue;
            //            a = Matrix[sl][j] / Matrix[j][j];
            //            for (uint k = j; k < FN; k++) Matrix[sl][k] -= Matrix[j][k] * a;
            //            Right[sl] -= Right[j] * a;
            //        }
            //        // анализ ведущей строки
            //        if ((Math.Abs(Matrix[i][i]) / maxMatrix) > 1.e - 8) break;
            //        else
            //        {
            //            if (Math.Abs(Matrix[sl][i]) / maxMatrix > 0.0001)
            //            {  // достаточный элемент найден
            //                RealVector Old = Matrix[i];
            //                Matrix[i] = Matrix[sl];
            //                Matrix[sl] = Old;
            //                Old.clear();
            //                //double *aOld = Matrix[i]; Matrix[i] = Matrix[sl];  Matrix[sl] = aOld;
            //                double bOld = Right[i]; Right[i] = Right[sl]; Right[sl] = bOld;
            //                break;
            //            }
            //            else
            //            {
            //                if (maxElem < Math.Abs(Matrix[sl][i]))
            //                {
            //                    maxElem = Math.Abs(Matrix[sl][i]); maxIndex = sl;
            //                }
            //                if (sl == nN - 1)
            //                {
            //                    if (maxElem > 1e-10)
            //                    {
            //                        RealVector Old = Matrix[i];
            //                        Matrix[i] = Matrix[sl];
            //                        Matrix[sl] = Old;
            //                        Old.clear();
            //                        //double *aOld = Matrix[i]; Matrix[i] = Matrix[maxIndex];  Matrix[maxIndex] = aOld;
            //                        double bOld = Right[i]; Right[i] = Right[maxIndex]; Right[maxIndex] = bOld;
            //                        break;
            //                    }
            //                    else
            //                    {
            //                        if (nN < FN) nN = FN; // расширение границы поиска
            //                        else
            //                        {
            //                            // ShowMessage("Матрица вырождена !");
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //    // Отладочная информация
            //    // AddTestInfoOne(i, a);
            //}
            //// сообщение о проделанной  работе :)
            ////FSentReportMessage("обратный ход исключения по Гауссу");
            ////FSendRateMessage(0);
            //for (uint i = FN - 1; i > -1; i--)
            //{
            //    // сообщение о проделанной  работе :)
            //    //FSendRateMessage(FN - i);
            //    a = Right[i];
            //    for (uint j = i + 1; j < (uint)FN; j++) a -= Matrix[i][j] * X[j];
            //    X[i] = a / Matrix[i][i];
            //    // Отладочная информация
            //    // AddTestInfoTwo(i, X);
            //}
        }
        /// <summary>
        /// клонирование объекта
        /// </summary>
        /// <returns></returns>
        public override IAlgebra Clone()
        {
            return new SparseAlgebraGaussStr(this.FN);
        }
        /// <summary>
        /// Вывод САУ на КОНСОЛЬ
        /// </summary>
        /// <param name="flag">количество знаков мантисы</param>
        /// <param name="color">длина цветового блока</param>
        public override void Print(int flag = 0, int color = 1)
        {
            if (flag == 0)
            {
                Console.WriteLine("Matrix");
                for (int i = 0; i < N; i++)
                {
                    double[] buf = new double[N];
                    SparseRow.DeCompress(Matrix[i], ref buf);
                    for (int j = 0; j < N; j++)
                        Console.Write(" " + buf[j].ToString("F4"));
                    Console.WriteLine();
                }
                Console.WriteLine("Right");
                for (int i = 0; i < N; i++)
                    Console.Write(" " + Right[i].ToString("F4"));
                Console.WriteLine();
            }
            if (flag == 1)
            {
                Console.WriteLine("Matrix");
                for (int i = 0; i < N; i++)
                {
                    for (int j = 0; j < Matrix[i].Count; j++)
                        Console.Write("   " + Matrix[i][j].Knot.ToString() + " : " + Matrix[i].Row[j].Elem.ToString("F4"));
                    Console.WriteLine();
                }
                Console.WriteLine("Right");
                for (int i = 0; i < N; i++)
                    Console.Write(" " + Right[i].ToString("F4"));
                Console.WriteLine();
            }
        }
        public static void Test1()
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
            uint N = 150;
            uint NE = N - 1;
            BC[1] = NE;
            double[] X = new double[N];
            SparseAlgebraGMRES algebra = new SparseAlgebraGMRES(N);

            for (uint i = 0; i < NE; i++)
            {
                ad[0] = i; ad[1] = i + 1;
                algebra.AddToMatrix(m, ad);
                algebra.AddToRight(B, ad);
                // algebra.Print();
            }

            adb[0] = 0; adb[1] = NE;
            uint[] adb0 = { 0 };
            uint[] adb1 = { NE };
            double[] BC0 = { 0 };
            double[] BC1 = { NE };
            algebra.BoundConditions(BC0, adb0);
            // algebra.Print();
            algebra.BoundConditions(BC1, adb1);
            //algebra.Print();
            algebra.Solve(ref X);
            for (int j = 0; j < N; j++)
                Console.Write(" " + X[j].ToString("F4"));
            Console.Read();
        }
        //public static void Main()
        //{
        //    Test1();
        //}
    }
}
