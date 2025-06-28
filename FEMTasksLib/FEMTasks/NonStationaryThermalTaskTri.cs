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
//                    Проект "Home" dNdy++
//                  - (C) Copyright 2003
//                        Потапов И.И.
//                         05.03.2004
//---------------------------------------------------------------------------
//    сильно, упрощенный перенос VTimeStreamHeat с dNdy++ на dNdy#
//         нет конвективных членов и SUPG стабилизации,
//           нет нелинейности и расчета Якобиана ...   
//                 нет адресных таблиц ...
//                      Потапов И.И.
//                      07.04.2021
//---------------------------------------------------------------------------
//                  Пример реализации
//---------------------------------------------------------------------------
namespace FEMTasksLib
{
    using AlgebraLib;
    using CommonLib;
    using MemLogLib;

    /// <summary>
    ///  ОО: Решатель для не стационарной задачи теплопроводности
    ///  с постоянными коэффициентами
    /// </summary>
    public class NonStationaryThermalTaskTri : AFETask
    {
        /// <summary>
        /// Флаг сборки рекурсивной системы
        /// </summary>
        protected bool flagStart = false;
        /// <summary>
        /// Для правой части
        /// </summary>
        protected IAlgebra Ralgebra;
        private double[] MRight = null;
        /// <summary>
        /// локальная матрица часть СЛАУ
        /// </summary>
        private double[][] RMatrix = null;
        /// <summary>
        /// Параметр схемы по времени
        /// </summary>
        private double theta;
        /// <summary>
        /// Шаг по времени
        /// </summary>
        private double dt;
        /// <summary>
        /// Температурапроводность
        /// </summary>
        private double Mu;
        /// <summary>
        /// Источник
        /// </summary>
        private double Q;
        /// <summary>
        /// Демо индекс границы
        /// </summary>
        private int indexBC;
        //Градиенты от функций форм
        const int cu = 3;
        double[] a = new double[cu];
        double[] dNdx = new double[cu];
        /// <summary>
        /// Матрица масс для КЭ
        /// </summary>
        double[,] MM = { { 2, 1, 1 }, { 1, 2, 1 }, { 1, 1, 2 } };
        double T0, TL, TR;
        public NonStationaryThermalTaskTri(double Mu, double Q, double T0, double TL, double TR,
            double theta, double dt, int indexBC)
        {
            this.Mu = Mu;
            this.theta = theta;
            this.dt = dt;
            this.T0 = T0;
            this.TL = TL;
            this.TR = TR;
            this.Q = Q;
            this.indexBC = indexBC;
        }
        public override void SetTask(IMesh mesh, IAlgebra algebra)
        {
            base.SetTask(mesh, algebra);
            this.Ralgebra = algebra.Clone();
        }
        public override void InitLocal(int cu, int cs = 1)
        {
            base.InitLocal(cu, cs);
            int Count = cu * cs;
            MEM.Alloc2DClear(Count, ref RMatrix);
        }
        public override void SolveTask(ref double[] result)
        {
            if (flagStart == false || result == null)
            {
                MEM.Alloc<double>(mesh.CountKnots, ref result);
                // установка начальных условий
                for (int i = 0; i < result.Length; i++)
                    result[i] = T0;
                flagStart = true;
            }
            algebra.Clear();
            Ralgebra.Clear();
            InitLocal(cu);
            for (uint elem = 0; elem < mesh.CountElements; elem++)
            {
                // получить узлы КЭ
                mesh.ElementKnots(elem, ref knots);
                // Получить координаты узлов
                mesh.GetElemCoords(elem, ref x, ref y);
                // установка координат узлов в функции формы
                double S = mesh.ElemSquare(elem);
                a[0] = (y[1] - y[2]);
                dNdx[0] = (x[2] - x[1]);
                a[1] = (y[2] - y[0]);
                dNdx[1] = (x[0] - x[2]);
                a[2] = (y[0] - y[1]);
                dNdx[2] = (x[1] - x[0]);
                for (int ai = 0; ai < cu; ai++)
                    for (int aj = 0; aj < cu; aj++)
                    {
                        // Матрица жесткости (теплопроводности)
                        double MatrixK = Mu * (a[ai] * a[aj] + dNdx[ai] * dNdx[aj]);
                        // Матрица масс
                        double MatrixM = MM[ai, aj] * S / 12.0 / dt;
                        LaplMatrix[ai][aj] += MatrixM + theta * MatrixK;
                        RMatrix[ai][aj] += MatrixM - (1 - theta) * MatrixK;
                    }
                // Вычисление ЛПЧ
                for (int ai = 0; ai < cu; ai++)
                    LocalRight[ai] += Q * S / 3;
                // добавление вновь сформированной ЛЖМ в ГМЖ
                algebra.AddToMatrix(LaplMatrix, knots);
                // добавление вновь сформированной ЛЖМ в ГМЖ
                Ralgebra.AddToMatrix(RMatrix, knots);
                // добавление вновь сформированной ЛПЧ в ГПЧ
                algebra.AddToRight(LocalRight, knots);
            }
            Ralgebra.GetResidual(ref MRight, result, 0);
            algebra.CopyRight(MRight);
            //Удовлетворение ГУ
            uint[] bound = mesh.GetBoundKnotsByMarker(indexBC);
            algebra.BoundConditions(TL, bound);
            bound = mesh.GetBoundKnotsByMarker((indexBC + 2) % 4);
            algebra.BoundConditions(TR, bound);
            algebra.Solve(ref result);
        }
    }
}



