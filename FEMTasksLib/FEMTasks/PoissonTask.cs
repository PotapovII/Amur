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
//                         14.11.03
//---------------------------------------------------------------------------
//         сильно, упрощенный перенос с dNdy++ на dNdy#
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
    ///  ОО: Решатель для задачи Пуассона (с численным интегрированием, на произвольной сетке)
    /// </summary>
    [Serializable]
    public class PoissonTask : AFETask
    {
        private double Mu;
        private double Q;
        private int indexBC;
        public PoissonTask(double Mu, double Q, int indexBC)
        {
            this.Mu = Mu;
            this.Q = Q;
            this.indexBC = indexBC;
        }
        public override void SolveTask(ref double[] result)
        {
            MEM.Alloc<double>(mesh.CountKnots, ref result, "result");
            algebra.Clear();

            for (uint elem = 0; elem < mesh.CountElements; elem++)
            {
                // получить узлы КЭ
                TypeFunForm typeff = mesh.ElementKnots(elem, ref knots);
                int cu = knots.Length;
                InitLocal(cu, 1);
                //Координаты и площадь
                mesh.GetElemCoords(elem, ref x, ref y);

                if (FunFormHelp.CheckFF(typeff) == 0)
                    pIntegration = IPointsA;
                else
                    pIntegration = IPointsB;
                AbFunForm ff = FunFormsManager.CreateKernel(typeff);
                // установка координат узлов
                ff.SetGeoCoords(x, y);
                // цикл по точкам интегрирования
                for (int pi = 0; pi < pIntegration.weight.Length; pi++)
                {
                    ff.CalkDiffForm(pIntegration.xi[pi], pIntegration.eta[pi]);

                    double DWJ = ff.DetJ * pIntegration.weight[pi];
                    for (int ai = 0; ai < cu; ai++)
                        for (int aj = 0; aj < cu; aj++)
                            LaplMatrix[ai][aj] += Mu * (ff.DN_x[ai] * ff.DN_x[aj] + ff.DN_y[ai] * ff.DN_y[aj]) * DWJ;

                    ff.CalkForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                    // Вычисление ЛПЧ
                    for (int ai = 0; ai < cu; ai++)
                        LocalRight[ai] += Q * ff.N[ai] * DWJ;
                }

                // добавление вновь сформированной ЛЖМ в ГМЖ
                algebra.AddToMatrix(LaplMatrix, knots);
                // добавление вновь сформированной ЛПЧ в ГПЧ
                algebra.AddToRight(LocalRight, knots);
            }
            //algebra.Print();
            //Удовлетворение ГУ
            uint[] bound = mesh.GetBoundKnotsByMarker(indexBC);
            algebra.BoundConditions(0.0, bound);
            //algebra.Print();
            algebra.Solve(ref result);
            //algebra.Print();
        }
    }
}



