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
//                   - (C) Copyright 
//                      Потапов И.И.
//                       06.04.2021
//---------------------------------------------------------------------------
namespace FEMTasksLib
{
    using FEMTasksLib.FEMTasks.Utils;
    using MemLogLib;
    using System;

    /// <summary>
    ///  ОО: Решатель для задачи Ламе на трехузловой сетке
    /// </summary>
    [Serializable]
    public class LameTaskTri : AFETask
    {
        const int cs = 2;
        const int cu = 3;
        //Градиенты от функций форм
        double[] a = new double[cu];
        double[] dNdx = new double[cu];

        /// <summary>
        /// Модуль сдвига
        /// </summary>
        private double Mu;
        /// <summary>
        /// Коэффициет Пуассона
        /// </summary>
        private double Nu;

        private double Q;
        private int indexBC;

        public LameTaskTri(double Mu, double Nu, double Q, int indexBC)
        {
            this.Mu = Mu;
            this.Nu = Nu;
            this.Q = Q;
            this.indexBC = indexBC;
            InitLocal(cu, cs);
        }
        public override void SolveTask(ref double[] result)
        {
            MEM.Alloc<double>((int)algebra.N, ref result);
            algebra.Clear();
            double vm = 2 * (1 - Nu) / (1 - 2 * Nu);
            double gm = 2 * Nu / (1 - 2 * Nu);
            int li, lj;
            for (uint elem = 0; elem < mesh.CountElements; elem++)
            {
                // получить узлы КЭ
                mesh.ElementKnots(elem, ref knots);
                // координаты и площадь
                mesh.GetElemCoords(elem, ref x, ref y);
                // площадь
                double S = mesh.ElemSquare(elem);
                double MuS = Mu / (4 * S);
                a[0] = (y[1] - y[2]);
                dNdx[0] = (x[2] - x[1]);
                a[1] = (y[2] - y[0]);
                dNdx[1] = (x[0] - x[2]);
                a[2] = (y[0] - y[1]);
                dNdx[2] = (x[1] - x[0]);
                // вычисление ЛЖМ для задачи Ламе
                for (int ai = 0; ai < cu; ai++)
                {
                    li = cs * ai;
                    for (int aj = 0; aj < cu; aj++)
                    {
                        lj = cs * aj;
                        LaplMatrix[li][lj] = MuS * (vm * a[ai] * a[aj] + dNdx[ai] * dNdx[aj]);
                        LaplMatrix[li][lj + 1] = MuS * (gm * a[ai] * dNdx[aj] + dNdx[ai] * a[aj]);
                        LaplMatrix[li + 1][lj] = MuS * (gm * dNdx[ai] * a[aj] + a[ai] * dNdx[aj]);
                        LaplMatrix[li + 1][lj + 1] = MuS * (vm * dNdx[ai] * dNdx[aj] + a[ai] * a[aj]);
                    }
                }
                // получкемк адресов 
                FEMUtils.GetAdress(knots, ref adressBound);
                // добавление вновь сформированной ЛЖМ в ГМЖ
                algebra.AddToMatrix(LaplMatrix, adressBound);
                // вычисление ЛПЧ от объемных сил
                for (int ai = 0; ai < cu; ai++)
                {
                    li = cs * ai;
                    LocalRight[li] = 0;                 // горизонтальная объемня нагрузка
                    LocalRight[li + 1] = Q * S / 3;     // вертикальная объемня нагрузка
                }
                // добавление вновь сформированной ЛПЧ в ГПЧ
                algebra.AddToRight(LocalRight, adressBound);
            }
            //algebra.Print();
            // получить граничные узлы
            uint[] bound = mesh.GetBoundKnotsByMarker(indexBC);
            // получить адреса
            FEMUtils.GetAdress(bound, ref adressBound);
            // установить ГУ
            algebra.BoundConditions(0.0, adressBound);
            //algebra.Print();
            // решение
            algebra.Solve(ref result);
            //algebra.Print();
        }
    }
}



