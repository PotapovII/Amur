//---------------------------------------------------------------------------
//     Реализация класса реализующего решение системы алгебраических уравнений
//              методом Гаусса с выбором ведущего элемента в блоке
//                              Потапов И.И.
//                        - (C) Copyright 2011 -
//                          ALL RIGHT RESERVED
//                               28.07.11
//---------------------------------------------------------------------------
//                 Источник код С++ HAlgebra проект MixTasker
//                              Потапов И.И.
//                        - (C) Copyright 2001 -
//                          ALL RIGHT RESERVED
//                              28.05.02
//---------------------------------------------------------------------------
//   Реализация библиотеки методов решения систем алгебраических уравнений
//                     - (C) Copyright 2019 -
//                       ALL RIGHT RESERVED
//                    разработка: Потапов И.И.
//---------------------------------------------------------------------------
//                     - (C) Copyright 2019 -
//                       ALL RIGHT RESERVED
//                            01 08 2021        
//                       правка: Потапов И.И.
//---------------------------------------------------------------------------

namespace AlgebraLib
{
    using System;
    using MemLogLib;
    using CommonLib;
    /// <summary>
    /// ОО: Метод Гаусса для решения САУ по столбцам, 
    /// прямоугольная матрица, выбор гл. элемента
    /// </summary>
    public class AlgebraGauss : AFullAlgebra
    {

        public AlgebraGauss(uint NN) : base(new AlgebraResult(), NN)
        {
            name = "Метод Гаусса с выбором ведущего элемента для полной матрицы";
            result.Name = name;
        }
        /// <summary>
        /// [6] Решение САУ
        /// </summary>
        /// <param name="X">Вектор в который возвращается результат решения СЛАУ</param>
        protected override void solve(ref double[] X)
        {
            if (X == null)
                X = new double[FN];
            double Error = MEM.Error10;
            // сообщение о проделанной  работе :)
            // FSentReportMessage("прямой ход исключения по Гауссу");
            int i;
            int k, tmp;
            double c;
            for (i = 0; i < FN; i++)
            {
                // сообщение о проделанной  работе :)
                // FSendRateMessage(i);
                // поиск максимального индекса в столбце
                c = Math.Abs(Matrix[i][i]); tmp = i;
                for (int j = i + 1; j < FN; j++)
                {
                    if (Math.Abs(Matrix[j][i]) > c)
                    {
                        c = Math.Abs(Matrix[j][i]);
                        tmp = j;
                    }
                }
                // если найден коэф. больше чем на главной диагонали то
                if (tmp > i)
                {
                    for (k = 0; k < FN; k++)  // сохраняем tmp - ю строку
                        X[k] = Matrix[tmp][k];
                    for (k = 0; k < FN; k++)  // заменяем tmp - ю строку на i - ю
                        Matrix[tmp][k] = Matrix[i][k];
                    for (k = 0; k < FN; k++)  // заменяем tmp - ю строку на i - ю
                        Matrix[i][k] = X[k];
                    // изменяем правую часть
                    c = Right[tmp]; Right[tmp] = Right[i]; Right[i] = c;
                }
                // Балансировка ведомой САУ
                //if(i%17==1) Calibrate(i,c);
                // проводим прямой ход исключения по Гауссу
                for (int j = i + 1; j < FN; j++)
                {
                    if (Math.Abs(Matrix[i][i]) < Error)
                    {
                        // MessageBox("Матрица вырождена !");
                        // throw new Exception();
                        return;
                    }
                    c = Matrix[j][i] / Matrix[i][i];
                    if (Math.Abs(c) > Error)
                    {
                        for (k = i; k < FN; k++)
                            Matrix[j][k] = Matrix[j][k] - c * Matrix[i][k];
                        Right[j] = Right[j] - c * Right[i];
                    }
                }
            }
            //Print();
            // выполняем обратный ход
            // FSentReportMessage("обратный ход исключения по Гауссу");
            for (i = (int)FN - 1; i > -1; i--)
            {
                c = Right[i];
                for (int j = i + 1; j < FN; j++)
                    c -= Matrix[i][j] * X[j];
                X[i] = c / Matrix[i][i];
            }
            Result.X = X;
        }
        public override IAlgebra Clone()
        {
            return new AlgebraGauss(N);
        }
    }
}
