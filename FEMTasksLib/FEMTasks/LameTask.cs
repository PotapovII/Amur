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
//                      Проект "Home" dNdy++
//                  - (C) Copyright 2003
//                        Потапов И.И.
//                         14.11.03
//---------------------------------------------------------------------------
//         сильно, упрощенный перенос с dNdy++ на dNdy#
//                 нет адресных таблиц ...
//                      Потапов И.И.
//                      07.04.2021
//---------------------------------------------------------------------------
// ToDo: я уже подзабыл, что полиномы Лагранжа не айс, 
//      придется востанавливать адресные таблицы ((
//---------------------------------------------------------------------------
namespace FEMTasksLib
{
    using CommonLib;
    using MemLogLib;
    using MeshLib;
    using System;

    /// <summary>
    ///  ОО: Решатель для задачи Ламе (с численным интегрированием, на произвольной сетке)
    /// </summary>
    [Serializable]
    public class LameTask : AFETask
    {
        /// <summary>
        /// количество степеней свободы в узле
        /// </summary>
        private int cs = 2;
        /// <summary>
        /// Модуль сдвига
        /// </summary>
        private double Mu;
        /// <summary>
        /// Коэффициет Пуассона
        /// </summary>
        private double Nu;
        /// <summary>
        /// объемные силы
        /// </summary>
        private double Q;

        private int indexBC;

        public LameTask(double Mu, double Nu, double Q, int indexBC)
        {
            this.Mu = Mu;
            this.Nu = Nu;
            this.Q = Q;
            this.indexBC = indexBC;
        }

        public override void SolveTask(ref double[] result)
        {
            MEM.Alloc<double>((int)algebra.N, ref result);
            algebra.Clear();

            double MuS = Mu;
            double vm = 2 * (1 - Nu) / (1 - 2 * Nu);
            double gm = 2 * Nu / (1 - 2 * Nu);

            for (uint elem = 0; elem < mesh.CountElements; elem++)
            {
                // получить узлы КЭ
                TypeFunForm typeff = mesh.ElementKnots(elem, ref knots);
                if (FunFormHelp.CheckFF(typeff) == 0)
                    pIntegration = IPointsA;
                else
                    pIntegration = IPointsB;
                AbFunForm ff = FunFormsManager.CreateKernel(typeff);
                int cu = knots.Length;
                InitLocal(cu, cs);
                //Координаты и площадь
                mesh.GetElemCoords(elem, ref x, ref y);
                // установка координат узлов
                ff.SetGeoCoords(x, y);
                // цикл по точкам интегрирования
                for (int pi = 0; pi < pIntegration.weight.Length; pi++)
                {
                    ff.CalkDiffForm(pIntegration.xi[pi], pIntegration.eta[pi]);

                    double DWJ = ff.DetJ * pIntegration.weight[pi];

                    int li, lj;
                    // Вычисление ЛЖМ для задачи Ламе
                    for (int ai = 0; ai < cu; ai++)
                    {
                        li = cs * ai;
                        for (int aj = 0; aj < cu; aj++)
                        {
                            lj = cs * aj;
                            LaplMatrix[li][lj] += MuS * (vm * ff.DN_x[ai] * ff.DN_x[aj] + ff.DN_y[ai] * ff.DN_y[aj]) * DWJ;
                            LaplMatrix[li][lj + 1] += MuS * (gm * ff.DN_x[ai] * ff.DN_y[aj] + ff.DN_y[ai] * ff.DN_x[aj]) * DWJ;
                            LaplMatrix[li + 1][lj] += MuS * (gm * ff.DN_y[ai] * ff.DN_x[aj] + ff.DN_x[ai] * ff.DN_y[aj]) * DWJ;
                            LaplMatrix[li + 1][lj + 1] += MuS * (vm * ff.DN_y[ai] * ff.DN_y[aj] + ff.DN_x[ai] * ff.DN_x[aj]) * DWJ;
                        }
                    }

                    ff.CalkForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                    // Вычисление ЛПЧ от объемных сил
                    for (int ai = 0; ai < cu; ai++)
                    {
                        li = cs * ai;
                        LocalRight[li] = 0;                           // горизонтальная объемня нагрузка
                        LocalRight[li + 1] += Q * ff.N[ai] * DWJ;     // вертикальная объемня нагрузка
                    }
                }

                // получкемк адресов 
                GetAdress(knots, ref adressBound, cs);
                // добавление вновь сформированной ЛЖМ в ГМЖ
                algebra.AddToMatrix(LaplMatrix, adressBound);
                // добавление вновь сформированной ЛПЧ в ГПЧ
                algebra.AddToRight(LocalRight, adressBound);
            }
            //algebra.Print();
            // получить граничные узлы
            uint[] bound = mesh.GetBoundKnotsByMarker(indexBC);
            // получить адреса
            GetAdress(bound, ref adressBound, cs);
            // установить ГУ
            algebra.BoundConditions(0.0, adressBound);
            //algebra.Print();
            // решение
            algebra.Solve(ref result);
            //algebra.Print();
        }
    }
}



