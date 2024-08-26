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
namespace FEMTasksLib
{
    using CommonLib;
    using MemLogLib;
    using MeshLib;
    using System;

    /// <summary>
    ///  ОО: Решатель для не стационарной задачи теплопроводности
    ///  с постоянными коэффициентами
    /// </summary>
    [Serializable]
    public class NonStationaryThermalTask : AFETask
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

        double T0, TL, TR;
        public NonStationaryThermalTask(double Mu, double Q, double T0, double TL, double TR,
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
            for (uint elem = 0; elem < mesh.CountElements; elem++)
            {
                // получить узлы КЭ
                TypeFunForm typeff = mesh.ElementKnots(elem, ref knots);
                // определить тип объекта для численного интегрирования 
                if (FunFormHelp.CheckFF(typeff) == 0)
                    pIntegration = IPointsA;
                else
                    pIntegration = IPointsB;
                // получить функции формы
                AbFunForm ff = FunFormsManager.CreateKernel(typeff);
                int cu = knots.Length;
                InitLocal(cu);
                // Получить координаты узлов
                mesh.GetElemCoords(elem, ref x, ref y);
                // установка координат узлов в функции формы
                ff.SetGeoCoords(x, y);
                // цикл по точкам интегрирования
                for (int pi = 0; pi < pIntegration.weight.Length; pi++)
                {
                    // вычисление глоб. производных от функции формы
                    ff.CalkDiffForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                    ff.CalkForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                    double DWJ = ff.DetJ * pIntegration.weight[pi];

                    for (int ai = 0; ai < cu; ai++)
                        for (int aj = 0; aj < cu; aj++)
                        {
                            // Лапласс
                            double Difuz = Mu * (ff.DN_x[ai] * ff.DN_x[aj] + ff.DN_y[ai] * ff.DN_y[aj]);
                            // Матрица масс
                            double MassTime = ff.N[ai] * ff.N[aj] / dt;
                            LaplMatrix[ai][aj] += (MassTime + theta * Difuz) * DWJ;
                            RMatrix[ai][aj] += (MassTime - (1 - theta) * Difuz) * DWJ;
                        }
                    // Вычисление ЛПЧ
                    for (int ai = 0; ai < cu; ai++)
                        LocalRight[ai] += Q * ff.N[ai] * DWJ;
                }
                // добавление вновь сформированной ЛЖМ в ГМЖ
                algebra.AddToMatrix(LaplMatrix, knots);
                // добавление вновь сформированной ЛЖМ в ГМЖ
                Ralgebra.AddToMatrix(RMatrix, knots);
                // добавление вновь сформированной ЛПЧ в ГПЧ
                algebra.AddToRight(LocalRight, knots);
            }
            Ralgebra.getResidual(ref MRight, result, 0);
            algebra.CopyRight(MRight);
            //Удовлетворение ГУ
            uint[] bound = mesh.GetBoundKnotsByMarker(indexBC);
            algebra.BoundConditions(TL, bound);
            bound = mesh.GetBoundKnotsByMarker((indexBC + 2) % 4);
            algebra.BoundConditions(TR, bound);

            //algebra.Print();
            algebra.Solve(ref result);

        }
    }
}



