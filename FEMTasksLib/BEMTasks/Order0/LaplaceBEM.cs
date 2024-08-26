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
//          Класс для решения задачи Лапласса МГЭ
//                 Потапов И.И.  12 05 2021
//---------------------------------------------------------------------------
namespace FEMTasksLib
{
    using AlgebraLib;
    using CommonLib;
    using MemLogLib;
    using System;

    /// <summary>
    /// ОО: Класс решатель: задача Пуассона (гидродинамика) 
    /// прямым методом граничных элементов
    /// </summary>
    [Serializable]
    public class LaplaceBEM : ABEMTask
    {
        /// <summary>
        /// Матрица потенциала по границе
        /// </summary>
        double[][] FS;
        /// <summary>
        /// Матрица скорости по границе
        /// </summary>
        double[][] GS;
        /// <summary>
        /// значение на границе (функции или потока)
        /// </summary>
        public double[] bValue = null;
        uint[] C = null;
        double[][] Matrix = null;
        double[] Right = null;
        double[] beU = null;
        double[] bePhi = null;
        double[] BL = null;
        public bool[] cKnot = null;
        /// <summary>
        /// Вязкость потока
        /// </summary>
        public double Mu = 0.65;
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="mesh">сетка области</param>
        public LaplaceBEM(IMesh mesh, IAlgebra algebra, double Mu, BoundLabel[] boundLabels) : base(mesh, algebra, boundLabels)
        {
            this.Mu = Mu;
            // this.bValue = bValue;
            if (boundLabels == null)
            {
                this.boundLabels = new BoundLabel[4];
                boundLabels[0] = new BoundLabel(0, 1, 0);
                boundLabels[1] = new BoundLabel(1, 0, 1);
                boundLabels[2] = new BoundLabel(2, 0, 0);
                boundLabels[3] = new BoundLabel(3, 0, 1);
            }
            this.boundLabels = boundLabels;
            // значение на границе (функции или потока)
            Init();
        }
        public override void SetTask(IMesh mesh, IAlgebra algebra)
        {
            base.SetTask(mesh, algebra);
            Init();
            Alpha = -1.0 / (2.0 * Math.PI * Mu);
        }
        protected void Init()
        {
            // и результат вектор
            MEM.Alloc<uint>(N, ref C);
            // Инициация матриц
            MEM.Alloc2DClear(N, ref FS);
            MEM.Alloc2DClear(N, ref GS);
        }
        /// <summary>
        /// Решение задачи дискретизации задачи и ее рещателя
        /// </summary>
        /// <param name="result">результат решения</param>
        public override void SolveTask(ref double[] result)
        {
            double xc, yc;
            // Инициация 
            MEM.AllocClear(N, ref result);
            MEM.AllocClear(N, ref Right);

            MEM.Alloc2DClear(N, ref Matrix);
            MEM.Alloc2DClear(N, ref FS);
            MEM.Alloc2DClear(N, ref GS);

            MEM.AllocClear(N, ref beU);
            MEM.AllocClear(N, ref bePhi);

            MEM.AllocClear(mesh.CountKnots, ref BL);
            MEM.AllocClear(mesh.CountKnots, ref Phi);
            MEM.AllocClear(mesh.CountKnots, ref U);
            cKnot = new bool[mesh.CountKnots];
            // Вычисление матриц Fs и Gs
            #region Расчет матриц жесткости
            // цикл по формированию матриц жесткости ГЭ 
            for (int i = 0; i < N; i++) // !!!!!!!!!!
            {
                // координаты точки ГЭ наблюдения
                GetMidleXY(i, out xc, out yc);
                // 
                for (int j = 0; j < N; j++)
                {
                    // Узлы ГЭ влияния
                    uint jdxA = belems[j].Vertex1;
                    uint jdxB = belems[j].Vertex2;
                    if (i == j) // расчет самовлияния источника
                    {
                        // полуразмер ГЭ
                        double h2 = mesh.GetBoundElemLength((uint)i) / 2.0;
                        FS[i][j] = -0.5;
                        GS[i][j] = 2 * Alpha * h2 * (Math.Log(h2 / Radius0) - 1);
                    }
                    else
                    {
                        // вычисление влияния со стороны других элементов
                        // расчет геометрии  (углов, расстояний, размера)
                        GetRelation(xc, yc, jdxA, jdxB, out thetaA, out thetaB, out rA, out rB, out h);
                        // расчет матрицы Грина (fs)
                        FS[i][j] = (thetaB - thetaA) / (2 * Math.PI);
                        // расчет матрицы потенциалов (fs)
                        double gs = GreenBoundElem(thetaA, thetaB, rA, rB, h);
                        GS[i][j] = gs;
                    }
                }
            }
            #endregion
            // ------------------------------------------------------------
            #region Формирование матрицы системы и ее правой части
            for (uint i = 0; i < N; i++)
                C[i] = i;
            // Формирование САУ и вычисление правой части
            for (uint i = 0; i < N; i++)
            {
                for (uint j = 0; j < N; j++)
                {
                    /////////////////////////////////////////
                    //
                    //   FS phi + GS u = GA chi
                    //
                    /////////////////////////////////////////
                    int type = mesh.GetBoundElementMarker(j);
                    //  значение ГУ на граничном элементе
                    BoundLabel bc = boundLabels[type];
                    double addValue = 0;
                    // тип ГЭ
                    if (bc.TypeBoundCond == 0)
                    {
                        //   FS phi = GA chi - GS u
                        // потенциал
                        addValue = FS[i][j] * bc.Value;
                        Matrix[i][j] = -1.0 * GS[i][j];
                    }
                    else
                    {
                        //   GS u = GA chi - FS phi
                        // поток
                        addValue = -1.0 * GS[i][j] * bc.Value;
                        Matrix[i][j] = FS[i][j];
                    }
                    // коэффициент правой части 
                    Right[i] -= addValue;
                }
            }
            //Console.WriteLine();
            //Console.WriteLine(" mesh.GetBoundElementMarker ");
            //for (uint i = 0; i < N; i++)
            //   Console.Write(" " + mesh.GetBoundElementMarker(i).ToString("F4"));
            //Console.WriteLine();

            //Console.WriteLine();
            //Console.WriteLine(" mesh.GetBoundElementMarker ");
            //for (uint i = 0; i < N; i++)
            //{
            //    int type = mesh.GetBoundElementMarker(i);
            //    if (type == 0)
            //        Console.Write(" 0");
            //    else
            //        Console.Write(" 1");
            //}
            //Console.WriteLine();

            //Console.WriteLine(" GS ");
            //for (int i = 0; i < N; i++)
            //{
            //    for (int j = 0; j < N; j++)
            //        Console.Write(" " + GS[i][j].ToString("F4"));
            //    Console.WriteLine();
            //}
            //Console.WriteLine(" FS ");
            //for (int i = 0; i < N; i++)
            //{
            //    for (int j = 0; j < N; j++)
            //        Console.Write(" " + FS[i][j].ToString("F4"));
            //    Console.WriteLine();
            //}
            //Console.WriteLine(" GA ");
            //for (int i = 0; i < N; i++)
            //{
            //    for (int j = 0; j < M; j++)
            //        Console.Write(" " + GA[i][j].ToString("F4"));
            //    Console.WriteLine();
            //}

            //Console.WriteLine(" Matrix ");
            //for (int i = 0; i < N; i++)
            //{
            //    for (int j = 0; j < N; j++)
            //        Console.Write(" " + Matrix[i][j].ToString("F4"));
            //    Console.WriteLine();
            //}
            //Console.WriteLine(" Right ");
            //for (int j = 0; j < N; j++)
            //    Console.Write(" " + Right[j].ToString("F4"));


            #endregion
            // формируем систему
            for (uint i = 0; i < N; i++)
                algebra.AddStringSystem(Matrix[i], C, i, Right[i]);


            // algebra.Print();

            algebra.Solve(ref result);

            //Console.WriteLine();
            //Console.WriteLine(" result ");
            //for (int j = 0; j < N; j++)
            //    Console.Write(" " + result[j].ToString("F4"));

            //double vvv = 0;
            //Console.WriteLine();
            //Console.WriteLine(" U ");
            //for (uint j = 0; j < N; j++)
            //{
            //    int type = mesh.GetBoundElementMarker(j);
            //    if (type == 0)
            //        Console.Write(" " + vvv.ToString("F4"));
            //    else
            //        Console.Write(" " + result[j].ToString("F4"));
            //}
            //Console.WriteLine();
            //Console.WriteLine(" Phi ");
            //for (uint j = 0; j < N; j++)
            //{
            //    int type = mesh.GetBoundElementMarker(j);
            //    if (type == 0)
            //        Console.Write(" " + result[j].ToString("F4"));
            //    else
            //        Console.Write(" " + vvv.ToString("F4"));
            //}

            for (uint k = 0; k < mesh.CountKnots; k++)
                cKnot[k] = false;
            for (uint be = 0; be < N; be++)
            {
                double L = mesh.GetBoundElemLength(be);
                // Узлы ГЭ наблюдения
                uint idxA = belems[be].Vertex1;
                uint idxB = belems[be].Vertex2;

                BL[idxA] += L / 2;
                BL[idxB] += L / 2;

                int type = mesh.GetBoundElementMarker(be);
                BoundLabel bc = boundLabels[type];
                if (bc.TypeBoundCond == 0)
                {
                    // потенциал
                    beU[be] = bc.Value;
                    bePhi[be] = result[be];
                }
                else
                {
                    // поток
                    beU[be] = result[be];
                    bePhi[be] = bc.Value;
                }
                U[idxA] += beU[be] * L / 2;
                U[idxB] += beU[be] * L / 2;
                Phi[idxA] += bePhi[be] * L / 2;
                Phi[idxB] += bePhi[be] * L / 2;
                cKnot[idxA] = true;
                cKnot[idxB] = true;
            }
            for (uint k = 0; k < U.Length; k++)
            {
                if (cKnot[k] == true)
                {
                    U[k] /= BL[k];
                    Phi[k] /= BL[k];
                }
                else
                {
                    //  Phi[k] = CalkPhi(k, bePhi, beU);
                }
            }
        }
    }
}
