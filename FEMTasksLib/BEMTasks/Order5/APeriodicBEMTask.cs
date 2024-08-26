#region License
/*
Copyright (c) 2019 - 2024  Potapov Igor Ivanovich, Khabarovsk

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion
//---------------------------------------------------------------------------
//             Класс для решения задачи кавитационного обтекания 
//                      тел вращения и плоских тел
//                              Потапов И.И.
//                        - () Copyright 2014 -
//                          ALL RIGHT RESERVED
//                              07.10.14
//---------------------------------------------------------------------------
//     Реализация для библиотеки решающей задачу внешнего обтекания тел 
//       периодическим методом граничных элементов высокого порядка
//---------------------------------------------------------------------------
namespace FEMTasksLib
{
    using AlgebraLib;
    using CommonLib;
    using MeshLib;
    using System;
    [Serializable]
    public abstract class APeriodicBEMTask : BEMStreamParams, IFEMTask
    {
        /// <summary>
        /// Флаг отладки
        /// </summary>
        public int Debug { get; set; }
        /// <summary>
        /// Сетка решателя
        /// </summary>
        public IMesh Mesh { get => mesh; }
        protected TwoMesh mesh;
        /// <summary>
        /// Алгебра задачи
        /// </summary>
        public IAlgebra Algebra { get => algebra; }
        protected IAlgebra algebra;
        /// <summary>
        /// <summary>
        /// Касательная скорость обтекания контура
        /// </summary>
        protected double[] V = null;
        /// <summary>
        /// <summary>
        /// расширенная касательная скорость обтекания контура
        /// </summary>
        protected double[] V1 = null;
        /// <summary>
        /// Параметрическая геометрия контура 
        /// </summary>
        protected double[] fx = null;
        protected double[] fy = null;
        /// <summary>
        /// Параметрическое задание геометрии для Х координат эллипса
        /// </summary>
        public PSpline GeometriaX = null;
        /// <summary>
        /// Параметрическое задание геометрии для Y координат эллипса
        /// </summary>
        public PSpline GeometriaY = null;
        /// <summary>
        /// Коэффициенты ядра для вычисления функции Грина
        /// </summary>
        protected double[] Betta = null;
        protected double[] dXdZeta = null;
        protected double[] dYdZeta = null;
        /// <summary>
        /// Матрица задачи
        /// </summary>
        protected double[,] Matrix = null;
        /// <summary>
        /// приращение вдоль кривой контура (натуральная координата контура)
        /// </summary>
        protected double[] detJ = null;
        /// <summary>
        /// Количество узлов в контуре
        /// </summary>
        protected int Count = 0;
        /// <summary>
        /// Количество узлов на расчетном в контуре
        /// в случае осевой симметрии
        /// </summary>
        protected int NS = 0;
        // Массив коеф. строки
        protected double[] line = null;
        // Массив адресов строки
        protected uint[] C = null;

        public APeriodicBEMTask(BEMStreamParams p, IMesh mesh, IAlgebra algebra) : base(p)
        {
            SetTask(mesh, algebra);
        }
        /// <summary>
        /// Установка сетки задачи и решателя СЛУ
        /// </summary>
        /// <param name="mesh">сетки задачи</param>
        /// <param name="algebra">решатель СЛУ</param>
        public virtual void SetTask(IMesh mesh, IAlgebra algebra)
        {
            this.mesh = mesh as TwoMesh;
            if (mesh == null)
                throw new Exception("Не согласованость по КЭ сетке, необходим объект TwoMesh");
            this.algebra = algebra;
            //
            fx = mesh.GetCoords(0);
            fy = mesh.GetCoords(1);
            Count = mesh.CountKnots;
            // приращение вдоль кривой
            detJ = new double[Count];
            // Формирование правых частей
            V1 = new double[Count];
            GeometriaX = new PSpline(fx);
            GeometriaY = new PSpline(fy);
            dXdZeta = GeometriaX.DiffSplineFunction();
            dYdZeta = GeometriaY.DiffSplineFunction();
            // приращение вдоль кривой
            for (int i = 0; i < Count; i++)
                detJ[i] = Math.Sqrt(dXdZeta[i] * dXdZeta[i] + dYdZeta[i] * dYdZeta[i]);
        }
        #region Методы для для получения дискретного аналога гранично-элементной задачи
        /// <summary>
        /// Вычисление бета вектора
        /// </summary>
        /// <param name="N">Количество узлов</param>
        /// <returns>Массив коэффициентов  Betta</returns>
        protected double[] CalkBetta(int N)
        {
            int M = (int)(N / 2) - 1;
            Betta = new double[N];
            double Alpha = 0;
            double log2 = Math.Log(2.0);
            double ztak = -1.0;
            double Sum;

            for (int k = 0; k < N; k++)
            {
                ztak = -1 * ztak;
                Sum = 0;
                if (k == 0)
                {
                    for (int m = 1; m <= M; m++)
                        Sum += 1.0 / m;
                    Betta[0] += -(log2 + ztak / N + Sum);
                }
                else
                {
                    for (int m = 1; m <= M; m++)
                        Sum += Math.Cos(2.0 * Math.PI * k * m / N) / m;
                    Alpha = -(log2 + ztak / N + Sum);
                    double sn = Math.Sin(k * Math.PI / N);
                    double ln = Math.Log(Math.Abs(sn));
                    Betta[k] += -ln + Alpha;
                }
            }
            return Betta;
        }
        #endregion

        /// <summary>
        /// Решение задачи дискретизации задачи и ее рещателя
        /// </summary>
        /// <param name="result">результат решения</param>
        public abstract void SolveTask(ref double[] result);
        ///// <summary>
        ///// Вычисление функции тока в координатах rm, zm
        ///// </summary>
        ///// <param name="x">осевая кордината</param>
        ///// <param name="y">радиальная кордината</param>
        ///// <returns>функция тока</returns>
        public abstract double CalkPhi(double x, double y);
        /// <summary>
        /// Вычисление расширенной матрицы системы для решения 
        ///            внешней осесимметричной задачи
        /// </summary>
        /// <param name="N">Порядок системы</param>
        /// <param name="Betta">Коэффициенты матрицы Betta</param>
        /// <returns>H</returns>
        public abstract double[,] CalkMatrix(int Ni, int N, double[] Betta);

    }
}
