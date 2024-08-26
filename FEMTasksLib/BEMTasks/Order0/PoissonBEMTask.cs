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
//               Класс для решения задачи Пуассона МГЭ
//             (C) 30.12.12 Бондаренко Б.В., Потапов И.И.
//---------------------------------------------------------------------------
///              адаптация и развитие в проекте   
///                 Потапов И.И.  09 05 2021
//---------------------------------------------------------------------------
namespace FEMTasksLib
{
    using AlgebraLib;
    using CommonLib;
    using MemLogLib;
    using System;
    using System.Linq;

    /// <summary>
    /// ОО: Класс решатель: задача Пуассона (гидродинамика) 
    /// прямым методом граничных элементов
    /// </summary>
    [Serializable]
    public class PoissonBEMTask : ABEMTask
    {
        public string errorMessage = "Ok";
        #region Параметры задачи
        /// <summary>
        /// коэффициент объемной силы rho*J*g
        /// </summary>
        public double Psi = 176.58;
        /// <summary>
        /// Вязкость потока
        /// </summary>
        public double Mu = 0.65;
        /// <summary>
        /// Способ вычисления правой части
        /// </summary>
        public bool TypeQ = true;
        /// <summary>
        /// Матрица потенциала по границе
        /// </summary>
        double[][] FS;
        /// <summary>
        /// Матрица скорости по границе
        /// </summary>
        double[][] GS;
        /// <summary>
        /// Матрица объемных сил в области
        /// </summary>
        double[][] GA;

        uint[] C = null;
        double[][] Matrix = null;
        double[] Right = null;
        double[] beU = null;
        double[] bePhi = null;
        double[] BL = null;
        public bool[] cKnot = null;
        #endregion
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="mesh">сетка области</param>
        public PoissonBEMTask(IMesh mesh, IAlgebra algebra, BoundLabel[] boundLabels = null, double Mu = 0.65, double Psi = 176.58) : base(mesh, algebra, boundLabels)
        {
            this.Mu = Mu;
            this.Psi = Psi;
            // значение на границе (функции или потока)
            if (boundLabels == null)
            {
                boundLabels = new BoundLabel[4];
                boundLabels[0] = new BoundLabel(0, 0, 0);
                boundLabels[1] = new BoundLabel(1, 1, 0);
                boundLabels[2] = new BoundLabel(2, 1, 0);
                boundLabels[3] = new BoundLabel(3, 0, 0);
            }
            this.boundLabels = boundLabels;
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
            MEM.Alloc2DClear(N, M, ref GA);
        }
        /// <summary>
        /// Решение задачи дискретизации задачи и ее рещателя
        /// </summary>
        /// <param name="result">результат решения</param>
        public override void SolveTask(ref double[] result)
        {
            try
            {
                double xc, yc;
                // Инициация 
                MEM.AllocClear(N, ref result);
                MEM.AllocClear(N, ref Right);

                MEM.Alloc2DClear(N, ref Matrix);
                MEM.Alloc2DClear(N, ref FS);
                MEM.Alloc2DClear(N, ref GS);
                MEM.Alloc2DClear(N, ref GA);

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
                        else // вычисление влияния со стороны других элементов
                        {
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
                #region Вычисление матрицы (Ga) для расчета правой части системы
                // цикл по ГЭ
                double[] phiQ = { 0, 0, 0 };
                uint[] jdxGA = { 0, 0, 0 };
                uint[] jdxGB = { 0, 0, 0 };
                if (TypeQ)
                {
                    #region Новая редакция
                    // цикл по граничным элементам
                    for (int i = 0; i < N; i++)
                    {
                        // Координаты центра ГЭ наблюдения
                        GetMidleXY(i, out xc, out yc);
                        // Цикл по КЭ в области
                        for (int elem = 0; elem < M; elem++)
                        {
                            // получить узлы КЭ
                            mesh.ElementKnots((uint)elem, ref knots);

                            jdxGA[0] = knots[0];
                            jdxGB[0] = knots[1];
                            jdxGA[1] = knots[1];
                            jdxGB[1] = knots[2];
                            jdxGA[2] = knots[2];
                            jdxGB[2] = knots[0];

                            // Вычисляем потенциал действующий на ГЭ наблюдения со стороны каждой грани элемента
                            for (int knot = 0; knot < 3; knot++)
                            {
                                // расчет геометрии  (углов, расстояний, размера)
                                GetRelation(xc, yc, jdxGA[knot], jdxGB[knot], out thetaA, out thetaB, out rA, out rB, out h);
                                // расчет потенциала грани (fs)
                                phiQ[knot] = GreenAreaElem(thetaA, thetaB, rA, rB, h);
                                // коррекция коэффициентов    
                                phiQ[knot] = Math.Abs(phiQ[knot]) > MEM.Error6 ? phiQ[knot] : 0;
                            }
                            // Вычисляем вклад каждой грани текущего КЭ на суммарный потенциал от объемного источника (суперпозиция)
                            for (int knot = 0; knot < 3; knot++)
                            {
                                double X, Y;
                                // если узел ГЭ не принадлежит элементу в области, то вычисляем потонциал как
                                if (!PointInTriangle(i, elem))
                                {
                                    uint jdxA = jdxGA[knot];            // узел связанный с точкой наблюдения - луч
                                    uint jdxAS = jdxGA[(knot + 1) % 3];  // первый узел грани
                                    uint jdxBS = jdxGB[(knot + 1) % 3];  // второй узел грани
                                    // определяем точку пересечения луча наблюдения с граню треугольника
                                    int res = GetRaySegmentIntersection(xc, yc, x[jdxA], y[jdxA], x[jdxAS], y[jdxAS], x[jdxBS], y[jdxBS], out X, out Y);
                                    //int res = mesh.Crossing(xc, yc, mesh.x[jdxA], mesh.y[jdxA], mesh.x[jdxAS], mesh.y[jdxAS], mesh.x[jdxBS], mesh.y[jdxBS], out X, out Y);
                                    if (res == 1)
                                    {
                                        double Length1 = LengthElem(xc, yc, x[jdxA], y[jdxA]);
                                        double Length2 = LengthElem(xc, yc, X, Y);
                                        // сложение и вычитание секторов определяющих потенциал
                                        if (Length1 > Length2)
                                            phiQ[(knot + 1) % 3] *= -1.0;
                                        else
                                        {
                                            phiQ[knot] *= -1.0;
                                            phiQ[(knot + 2) % 3] *= -1.0;
                                        }
                                        break;
                                    }
                                }
                            }
                            // заносим результат в матрицу объемного влияния 
                            GA[i][elem] = phiQ.Sum();
                        }
                    }
                    #endregion
                }
                else
                {
                    #region Старая редакция
                    //// цикл по ГЭ
                    for (int i = 0; i < N; i++)
                    {
                        //// Узлы ГЭ наблюдения
                        //int idxA = mesh.BoundryKnots[i];
                        //int idxB = mesh.BoundryKnots[(i + 1) % N];
                        // Узлы ГЭ наблюдения
                        uint idxA = belems[i].Vertex1;
                        uint idxB = belems[i].Vertex2;
                        //Node node = _boundry[i].MiddleNod;
                        // Цикл по КЭ в области
                        for (int elem = 0; elem < M; elem++)
                        {
                            uint jdxA_G1, jdxB_G1, jdxA_G2, jdxB_G2, jdxA_G3, jdxB_G3;
                            //    AreaElement el = _area[j];
                            //    GA[i][j] = GetGa(node, el);
                            //private double GetGa(Node node, AreaElement areaElement)
                            // получить узлы КЭ
                            mesh.ElementKnots((uint)elem, ref knots);
                            jdxA_G1 = knots[0];
                            jdxB_G1 = knots[1];
                            jdxA_G2 = knots[1];
                            jdxB_G2 = knots[2];
                            jdxA_G3 = knots[2];
                            jdxB_G3 = knots[0];

                            //mesh.GetGanElems(elem, 0, out jdxA_G1, out jdxB_G1);
                            //mesh.GetGanElems(elem, 1, out jdxA_G2, out jdxB_G2);
                            //mesh.GetGanElems(elem, 2, out jdxA_G3, out jdxB_G3);
                            GetMidleXY(i, out xc, out yc);
                            // расчет геометрии  (углов, расстояний, размера)
                            GetRelation(xc, yc, jdxA_G1, jdxB_G1, out thetaA, out thetaB, out rA, out rB, out h);
                            //GetRelation(i, jdxA_G1, jdxB_G1, out thetaA, out thetaB, out rA, out rB, out h);
                            // расчет потенциала грани (fs)
                            double g1 = GreenAreaElem(thetaA, thetaB, rA, rB, h);
                            // расчет геометрии  (углов, расстояний, размера)
                            GetRelation(xc, yc, jdxA_G2, jdxB_G2, out thetaA, out thetaB, out rA, out rB, out h);
                            // расчет потенциала грани (fs)
                            double g2 = GreenAreaElem(thetaA, thetaB, rA, rB, h);
                            // расчет геометрии  (углов, расстояний, размера)
                            GetRelation(xc, yc, jdxA_G3, jdxB_G3, out thetaA, out thetaB, out rA, out rB, out h);
                            // расчет потенциала грани (fs)
                            double g3 = GreenAreaElem(thetaA, thetaB, rA, rB, h);

                            g1 = Math.Abs(g1) > MEM.Error6 ? g1 : 0;
                            g2 = Math.Abs(g2) > MEM.Error6 ? g2 : 0;
                            g3 = Math.Abs(g3) > MEM.Error6 ? g3 : 0;

                            //double xc = (x[idxA] + x[idxB]) / 2.0;
                            //double yc = (y[idxA] + y[idxB]) / 2.0;

                            double X, Y;

                            if (!PointInTriangle(i, elem))
                            {
                                double x1 = x[jdxA_G3];
                                double y1 = y[jdxA_G3];

                                double sx1 = x[jdxA_G1];
                                double sy1 = y[jdxA_G1];
                                double sx2 = x[jdxB_G1];
                                double sy2 = y[jdxB_G1];
                                int res = GetRaySegmentIntersection(xc, yc, x1, y1, sx1, sy1, sx2, sy2, out X, out Y);
                                if (res == 1)
                                {
                                    double Length1 = LengthElem(xc, yc, x[jdxA_G3], y[jdxA_G3]);
                                    double Length2 = LengthElem(xc, yc, X, Y);
                                    if (Length1 > Length2)
                                        g1 *= -1.0;
                                    else
                                    {
                                        g2 *= -1.0;
                                        g3 *= -1.0;
                                    }
                                }
                                else
                                {
                                    res = GetRaySegmentIntersection(xc, yc, x[jdxA_G1], y[jdxA_G1], x[jdxA_G2], y[jdxA_G2], x[jdxB_G2], y[jdxB_G2], out X, out Y);
                                    if (res == 1)
                                    {
                                        double Length1 = LengthElem(xc, yc, x[jdxA_G1], y[jdxA_G1]);
                                        double Length2 = LengthElem(xc, yc, X, Y);

                                        if (Length1 > Length2)
                                            g2 *= -1.0;
                                        else
                                        {
                                            g1 *= -1.0;
                                            g3 *= -1.0;
                                        }
                                    }
                                    else
                                    {
                                        res = GetRaySegmentIntersection(xc, yc, x[jdxA_G2], y[jdxA_G2], x[jdxA_G3], y[jdxA_G3], x[jdxB_G3], y[jdxB_G3], out X, out Y);
                                        if (res == 1)
                                        {
                                            double Length1 = LengthElem(xc, yc, x[jdxA_G2], y[jdxA_G2]);
                                            double Length2 = LengthElem(xc, yc, X, Y);
                                            if (Length1 > Length2)
                                                g3 *= -1.0;
                                            else
                                            {
                                                g1 *= -1.0;
                                                g2 *= -1.0;
                                            }
                                        }
                                    }
                                }
                            }
                            GA[i][elem] = g1 + g2 + g3;
                        }
                    }
                    #endregion
                }
                #endregion
                // ------------------------------------------------------------
                #region Формирование матрицы системы и ее правой части
                for (uint i = 0; i < N; i++)
                    C[i] = i;
                // Формирование САУ и вычисление правой части
                for (uint i = 0; i < N; i++)
                    for (uint j = 0; j < N; j++)
                    {
                        int type = mesh.GetBoundElementMarker(j);
                        //  значение ГУ на граничном элементе
                        BoundLabel bc = boundLabels[type];
                        double addValue = 0;
                        // тип ГЭ
                        if (bc.TypeBoundCond == 0)
                        {
                            // потенциал
                            addValue = FS[i][j] * bc.Value;
                            Matrix[i][j] = -1.0 * GS[i][j];
                        }
                        else
                        {
                            // поток
                            addValue = -1.0 * GS[i][j] * bc.Value;
                            Matrix[i][j] = FS[i][j];
                        }
                        // коэффициент правой части 
                        Right[i] -= addValue;
                    }
                // Учет источников внутри области
                for (int i = 0; i < N; i++)
                    for (int elem = 0; elem < M; elem++)
                        Right[i] -= Psi * GA[i][elem];

                if (Debug > 0)
                {
                    Console.WriteLine();
                    Console.WriteLine(" mesh.GetBoundElementMarker ");
                    for (uint i = 0; i < N; i++)
                        Console.Write(" " + mesh.GetBoundElementMarker(i).ToString("F4"));
                    Console.WriteLine();

                    Console.WriteLine();
                    Console.WriteLine(" mesh.GetBoundElementMarker ");
                    for (uint i = 0; i < N; i++)
                    {
                        int type = mesh.GetBoundElementMarker(i);
                        if (type == 0)
                            Console.Write(" 0");
                        else
                            Console.Write(" 1");
                    }
                    Console.WriteLine();

                    Console.WriteLine(" GS ");
                    for (int i = 0; i < N; i++)
                    {
                        for (int j = 0; j < N; j++)
                            Console.Write(" " + GS[i][j].ToString("F4"));
                        Console.WriteLine();
                    }
                    Console.WriteLine(" FS ");
                    for (int i = 0; i < N; i++)
                    {
                        for (int j = 0; j < N; j++)
                            Console.Write(" " + FS[i][j].ToString("F4"));
                        Console.WriteLine();
                    }
                    Console.WriteLine(" GA ");
                    for (int i = 0; i < N; i++)
                    {
                        for (int j = 0; j < M; j++)
                            Console.Write(" " + GA[i][j].ToString("F4"));
                        Console.WriteLine();
                    }

                    Console.WriteLine(" Matrix ");
                    for (int i = 0; i < N; i++)
                    {
                        for (int j = 0; j < N; j++)
                            Console.Write(" " + Matrix[i][j].ToString("F4"));
                        Console.WriteLine();
                    }
                    Console.WriteLine(" Right ");
                    for (int j = 0; j < N; j++)
                        Console.Write(" " + Right[j].ToString("F4"));
                }

                #endregion
                // формируем систему
                for (uint i = 0; i < N; i++)
                    algebra.AddStringSystem(Matrix[i], C, i, Right[i]);

                if (Debug > 0)
                    algebra.Print();

                algebra.Solve(ref result);

                if (Debug > 0)
                {
                    Console.WriteLine();
                    Console.WriteLine(" result ");
                    for (int j = 0; j < N; j++)
                        Console.Write(" " + result[j].ToString("F4"));

                    // double vvv = 0;
                    Console.WriteLine();
                    Console.WriteLine(" U ");
                    for (uint j = 0; j < N; j++)
                    {
                        int type = mesh.GetBoundElementMarker(j);
                        //  значение ГУ на граничном элементе
                        BoundLabel bc = boundLabels[type];
                        // тип ГЭ
                        if (bc.TypeBoundCond == 0)
                            Console.Write(" " + bc.Value);
                        else
                            Console.Write(" " + result[j].ToString("F4"));

                    }
                    Console.WriteLine();
                    Console.WriteLine(" Phi ");
                    for (uint j = 0; j < N; j++)
                    {
                        int type = mesh.GetBoundElementMarker(j);
                        //  значение ГУ на граничном элементе
                        BoundLabel bc = boundLabels[type];
                        // тип ГЭ
                        if (bc.TypeBoundCond == 0)
                            Console.Write(" " + result[j].ToString("F4"));
                        else
                            Console.Write(" " + bc.Value);
                    }
                }

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
                    // тип ГЭ
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
                }
                //for (uint k = 0; k < cKnot.Length; k++)
                //{
                //    if( cKnot[k] == false )
                //        Phi[k] = CalkPhi(k);
                //}
            }
            catch (Exception ee)
            {
                errorMessage += "Class: GSolverBEM, Method: Solver, Error: " + ee.Message;
            }
        }


    }
}
