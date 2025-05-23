﻿#region License
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
    using MemLogLib;
    using System;

    /// <summary>
    ///  ОО: Решатель для задачи Пуассона на трехузловой сетке
    /// </summary>
    [Serializable]
    public class PoissonTaskTri : AFETask
    {
        public double[] U = null;
        private double Mu;
        private double Q;
        private int indexBC;
        private int[] indexsBC = null;
        //Градиенты от функций форм
        const int cu = 3;
        double[] b = new double[cu];
        double[] c = new double[cu];
        public PoissonTaskTri(double Mu, double Q, int indexBC)
        {
            this.Mu = Mu;
            this.Q = Q;
            this.indexBC = indexBC;
        }
        public PoissonTaskTri(double Mu, double Q, int[] indexsBC)
        {
            this.Mu = Mu;
            this.Q = Q;
            this.indexsBC = indexsBC;
        }
        public void Test()
        {
            double[] result = null;
            SolveTask(ref result);
            U = result;
        }

        public override void SolveTask(ref double[] result)
        {
            MEM.Alloc<double>(mesh.CountKnots, ref result);
            algebra.Clear();
            InitLocal(cu, 1);
            for (uint elem = 0; elem < mesh.CountElements; elem++)
            {
                // получить узлы КЭ
                mesh.ElementKnots(elem, ref knots);
                // координаты и площадь
                mesh.GetElemCoords(elem, ref x, ref y);
                // площадь
                double S = mesh.ElemSquare(elem);
                b[0] = (y[1] - y[2]);
                c[0] = (x[2] - x[1]);
                b[1] = (y[2] - y[0]);
                c[1] = (x[0] - x[2]);
                b[2] = (y[0] - y[1]);
                c[2] = (x[1] - x[0]);
                // вычисление ЛЖМ
                for (int ai = 0; ai < cu; ai++)
                    for (int aj = 0; aj < cu; aj++)
                        LaplMatrix[ai][aj] = Mu * (b[ai] * b[aj] + c[ai] * c[aj]) / (4 * S);
                // добавление вновь сформированной ЛЖМ в ГМЖ
                algebra.AddToMatrix(LaplMatrix, knots);
                // вычисление ЛПЧ
                for (int j = 0; j < cu; j++)
                    LocalRight[j] = Q * S / 3;
                // добавление вновь сформированной ЛПЧ в ГПЧ
                algebra.AddToRight(LocalRight, knots);
            }
            //algebra.Print();
            // удовлетворение ГУ
            if (indexsBC == null)
            {
                uint[] bound = mesh.GetBoundKnotsByMarker(indexBC);
                algebra.BoundConditions(0.0, bound);
            }
            else
            {
                for (int i = 0; i < indexsBC.Length; i++)
                {
                    uint[] bound = mesh.GetBoundKnotsByMarker(indexsBC[i]);
                    algebra.BoundConditions(0.0, bound);
                }
            }
            //algebra.Print();
            algebra.Solve(ref result);
            //algebra.Print();
        }
    }
}



