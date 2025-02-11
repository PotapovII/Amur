#region License
/*
Copyright (c) 2019 - 2024  Potapov Igor Ivanovich, Khabarovsk

Permission is hereby granted, free of charge, to any person
obtaining dNdx copy of this software and associated documentation
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
//                  - (C) Copyright 2025
//                        Потапов И.И.
//                         04.02.25
//---------------------------------------------------------------------------
// Не стационарная задача Рейнольдса в переменных Phi,Vortex
// две степени свободы в узле, со слабыми граничными условиями для Vortex
//---------------------------------------------------------------------------
namespace FEMTasksLib.FEMTasks.VortexStream
{
    using System;
    using MemLogLib;
    using CommonLib;
    using CommonLib.Mesh;
    using CommonLib.Physics;
    using CommonLib.Function;

    /// <summary>
    ///  ОО: Решатель для задачи Рейнольдс на трехузловой сетке
    /// </summary>
    [Serializable]
    public class ReynoldsVortexStreamTri : VortexStreamTri
    {
        /// <summary>
        /// Параметр схемы по времени
        /// </summary>
        protected double theta = 0.5;
        /// <summary>
        /// Итераций по нелинейности на текущем шаге по времени
        /// </summary>
        protected int NLine;
        /// <summary>
        /// Для правой части
        /// </summary>
        [NonSerialized]
        protected IAlgebra Ralgebra;
        /// <summary>
        /// Правая часть
        /// </summary>
        protected double[] MRight = null;
        /// <summary>
        /// локальная матрица часть СЛАУ
        /// </summary>
        private double[][] RMatrix = null;
        #region Поля результатов
        /// <summary>
        /// Решение 
        /// </summary>
        protected double[] result = null;
        /// <summary>
        /// Предыдущее решение по нелинейности
        /// </summary>
        protected double[] result_cur = null;
        #endregion
        public ReynoldsVortexStreamTri(int NLine, int SigmaTask, double RadiusMin, IDigFunction VelosityUy, double theta = 0.5) 
            : base(VelosityUy, NLine, SigmaTask, RadiusMin)
        {
            this.theta = theta;
            this.NLine = NLine;
            flagStart = true;
        }
        public override void SetTask(IMesh mesh, IAlgebra algebra, IMeshWrapper wMesh = null)
        {
            if(this.mesh == null)
                flagStart = true;
            else
            {
                if( this.mesh.CountKnots != mesh.CountKnots ||
                    this.mesh.CountElements != mesh.CountElements ||
                    this.mesh.CountBoundKnots != mesh.CountBoundKnots)
                    MEM.Alloc(cs * mesh.CountKnots, ref result_old);
                flagStart = true;
            }
            base.SetTask(mesh, algebra, wMesh);
            MEM.Alloc(cs * mesh.CountKnots, ref MRight);
            MEM.Alloc(cs * mesh.CountKnots, ref result_cur);
            Ralgebra = algebra.Clone();
        }
        /// <summary>
        /// Решение одного шага по времени для задачи Рейнольдса
        /// TO DO доделать решатель для цилиндрического русла
        /// </summary>
        /// <param name="result"></param>
        public void SolveTaskRe(ref double[] _Phi, ref double[] _Vortex,  double[] eddyViscosity, double[] Ux, double dt)
        {
            uint ee = 0;
            uint testflag = 0;
            MEM.Alloc(mesh.CountKnots, ref _Phi);
            MEM.Alloc(mesh.CountKnots, ref _Vortex);
            try
            {
                MEM.MemCopy(ref result_cur, result_old);
                // итераций по нелинейности на текущем шаге по времени
                for (int nIdx = 0; nIdx < NLine; nIdx++)
                {
                    if (flagStart == true)
                    {
                        // На первом шаге решаем задачу Стокса
                        base.SolveTask(ref result_old);
                        flagStart = false;
                        MEM.MemCopy(ref result, result_old);
                        MEM.MemCopy(ref result_cur, result_old);
                        for (int i = 0; i < mesh.CountKnots; i++)
                        {
                            Phi[i] = result_cur[2 * i];
                            _Phi[i] = Phi[i];
                            Vortex[i] = result_cur[2 * i + 1];
                            _Vortex[i] = Vortex[i];
                        }
                    }
                    else
                    {
                        testflag = 0;
                        // расчет функции тока и функции вихря
                        Console.WriteLine("Phi & Vortex: шаг по нелинейности: " + nIdx.ToString());
                        MEM.Alloc((int)algebra.N, ref result);
                        algebra.Clear();
                        Ralgebra.Clear();
                        int li, lj;
                        int Count = cu * cs;
                        for (uint elem = 0; elem < mesh.CountElements; elem++)
                        {
                            ee = elem;
                            // локальная матрица часть СЛАУ
                            MEM.Alloc2DClear(Count, ref LaplMatrix);
                            MEM.Alloc2DClear(Count, ref RMatrix);
                            double[] b = dNdx[elem];
                            double[] c = dNdy[elem];
                            uint i0 = eKnots[elem].Vertex1;
                            uint i1 = eKnots[elem].Vertex2;
                            uint i2 = eKnots[elem].Vertex3;
                            // получить узлы КЭ
                            uint[] knots = { i0, i1, i2 };
                            double Se = S[elem];
                            // Скорости через phi 
                            double eVx = c[0] * Phi[i0] + c[1] * Phi[i1] + c[2] * Phi[i2];
                            double eVy = -(b[0] * Phi[i0] + b[1] * Phi[i1] + b[2] * Phi[i2]);
                            double mV = Math.Sqrt(eVx * eVx + eVy * eVy);
                            double Hk = Math.Sqrt(Se) / 1.4;
                            double Pe = SPhysics.rho_w * mV * Hk / (2 * mu);
                            double Ax = 0;
                            double Ay = 0;
                            if (MEM.Equals(mV, 0) != true)
                            {
                                Ax = eVx / mV;
                                Ay = eVy / mV;
                            }
                            double eMu = (eddyViscosity[i0] + eddyViscosity[i1] + eddyViscosity[i2]) / 3.0;
                            // вычисление ЛЖМ для задачи Рейнольдса
                            for (int ai = 0; ai < cu; ai++)
                            {
                                double L_SUPG = omega * (Hx[elem] * Ax * b[ai] + Hy[elem] * Ay * c[ai]);
                                li = cs * ai;
                                double Lb = (1 / 3.0 + L_SUPG);
                                double La = rho_w * Lb;

                                for (int aj = 0; aj < cu; aj++)
                                {
                                    lj = cs * aj;
                                    // матрица масс
                                    double M_ab = MM[ai][aj] * Se;
                                    // матрица жосткости - вязкие члены плоские
                                    double K_ab = (b[ai] * b[aj] + c[ai] * c[aj]) * Se;

                                    // конверктивные члены - скорости
                                    double C_agb = La * ((c[0] * b[aj] - b[0] * c[aj]) * Phi[i0] +
                                                                 (c[1] * b[aj] - b[1] * c[aj]) * Phi[i1] +
                                                                 (c[2] * b[aj] - b[2] * c[aj]) * Phi[i2]) * Se;
                                    // конверктивные члены - вязкости
                                    double D_agb = Lb * ((b[0] * b[aj] - c[0] * c[aj]) * eddyViscosity[i0] +
                                                         (b[1] * b[aj] - c[1] * c[aj]) * eddyViscosity[i1] +
                                                         (b[2] * b[aj] - c[2] * c[aj]) * eddyViscosity[i2]) * Se;

                                    //LaplMatrix[li][lj] = (1 - theta) * K_ab;
                                    //LaplMatrix[li][lj + 1] = -(1 - theta) * M_ab;
                                    //LaplMatrix[li + 1][lj] = 0;
                                    //LaplMatrix[li + 1][lj + 1] = rho_w * M_ab + dt * (1 - theta) * (C_agb - D_agb + eMu * K_ab);

                                    //RMatrix[li][lj] = -K_ab * theta;
                                    //RMatrix[li][lj + 1] = M_ab * theta;
                                    //RMatrix[li + 1][lj] = 0;
                                    //RMatrix[li + 1][lj + 1] = rho_w * M_ab - dt * theta * (C_agb - D_agb + eMu * K_ab);

                                    LaplMatrix[li][lj] = K_ab;
                                    LaplMatrix[li][lj + 1] = - M_ab;
                                    LaplMatrix[li + 1][lj] = 0;
                                    LaplMatrix[li + 1][lj + 1] = rho_w * M_ab + dt * (1 - theta) * (C_agb - D_agb + eMu * K_ab);

                                    RMatrix[li][lj] = 0;
                                    RMatrix[li][lj + 1] = 0;
                                    RMatrix[li + 1][lj] = 0;
                                    RMatrix[li + 1][lj + 1] = rho_w * M_ab - dt * theta * (C_agb - D_agb + eMu * K_ab);

                                }
                            }
                            // получем значения адресов неизвестных
                            GetAdress(knots, ref adressBound);
                            // добавление вновь сформированной ЛЖМ в ГМЖ
                            algebra.AddToMatrix(LaplMatrix, adressBound);
                            // добавление вновь сформированной ЛЖМ в ГМЖ
                            Ralgebra.AddToMatrix(RMatrix, adressBound);
                        }
                        testflag = 1;
                        // Расчет
                        Ralgebra.getResidual(ref MRight, result_old, 0);
                        testflag = 2;
                        algebra.CopyRight(MRight);
                        testflag = 3;
                        // Выполнение граничных условий для функции вихря
                        VortexBC();
                        testflag = 4;
                        // Выполнение граничных условий для функции тока
                        algebra.BoundConditions(0, bcIndex);
                        testflag = 5;
                        //algebra.Print();
                        // решение
                        algebra.Solve(ref result);
                        testflag = 6;
                        for (int i = 0; i < result.Length; i++)
                            result_cur[i] = relax * result[i] + (1 - relax) * result_cur[i];
                        for (int i = 0; i < mesh.CountKnots; i++)
                        {
                            Phi[i] = result_cur[2 * i];
                            _Phi[i] = Phi[i];
                            Vortex[i] = result_cur[2 * i + 1];
                            _Vortex[i] = Vortex[i];
                        }
                        testflag = 7;
                        //************************************************************************************************************************
                        // Считаем невязку
                        //************************************************************************************************************************
                        double epsVortex = 0.0;
                        double normVortex = 0.0;

                        for (int i = 0; i < result.Length; i++)
                        {
                            normVortex += result_cur[i] * result_cur[i];
                            epsVortex += (result_cur[i] - result[i]) * (result_cur[i] - result[i]);
                        }
                        double residual = Math.Sqrt(epsVortex / normVortex);
                        Console.WriteLine("n {0} residual {1}", nIdx, residual);
                        if (residual < MEM.Error5)
                            break;
                        MEM.MemCopy(ref result_cur, result);
                    }
                }
                // расчет поля скорости
                Console.WriteLine("Vy ~ Vz:");
                CalcVelosityPlane(Phi, ref Vy, ref Vz);
                MEM.MemCopy(ref result_old, result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ee = " + ee.ToString() + " " + ex.Message);
            }

        }

        /// <summary>
        /// Решение одного шага по времени для задачи Рейнольдса
        /// TO DO доделать решатель для цилиндрического русла
        /// </summary>
        /// <param name="result"></param>
        public void SolveTaskRe_R(ref double[] _Phi, ref double[] _Vortex, double[] eddyViscosity, double[] Ux, double dt)
        {
            uint ee = 0;
            uint testflag = 0;
            MEM.Alloc(mesh.CountKnots, ref _Phi);
            MEM.Alloc(mesh.CountKnots, ref _Vortex);
            try
            {
                MEM.MemCopy(ref result_cur, result_old);
                // итераций по нелинейности на текущем шаге по времени
                for (int nIdx = 0; nIdx < NLine; nIdx++)
                {
                    if (flagStart == true)
                    {
                        // На первом шаге решаем задачу Стокса
                        base.SolveTask(ref result_old);
                        flagStart = false;
                        MEM.MemCopy(ref result, result_old);
                        MEM.MemCopy(ref result_cur, result_old);
                        for (int i = 0; i < mesh.CountKnots; i++)
                        {
                            Phi[i] = result_cur[2 * i];
                            _Phi[i] = Phi[i];
                            Vortex[i] = result_cur[2 * i + 1];
                            _Vortex[i] = Vortex[i];
                        }
                    }
                    else
                    {
                        testflag = 0;
                        // расчет функции тока и функции вихря
                        Console.WriteLine("Phi & Vortex: шаг по нелинейности: " + nIdx.ToString());
                        MEM.Alloc((int)algebra.N, ref result);
                        algebra.Clear();
                        Ralgebra.Clear();
                        int li, lj;
                        int Count = cu * cs;
                        for (uint elem = 0; elem < mesh.CountElements; elem++)
                        {
                            ee = elem;
                            // локальная матрица часть СЛАУ
                            MEM.Alloc2DClear(Count, ref LaplMatrix);
                            MEM.Alloc2DClear(Count, ref RMatrix);
                            double[] b = dNdx[elem];
                            double[] c = dNdy[elem];
                            uint i0 = eKnots[elem].Vertex1;
                            uint i1 = eKnots[elem].Vertex2;
                            uint i2 = eKnots[elem].Vertex3;
                            // получить узлы КЭ
                            uint[] knots = { i0, i1, i2 };
                            double Se = S[elem];
                            double Radius = 1;
                            if (SigmaTask == 1)
                            {
                                Radius = RadiusMin + (X[i0] + X[i1] + X[i2]) / 3.0;
                            }
                            // Скорости через phi 
                            double eVx = (c[0] * Phi[i0] + c[1] * Phi[i1] + c[2] * Phi[i2]) / Radius;
                            double eVy = -(b[0] * Phi[i0] + b[1] * Phi[i1] + b[2] * Phi[i2]) / Radius;
                            double mV = Math.Sqrt(eVx * eVx + eVy * eVy);
                            double Hk = Math.Sqrt(Se) / 1.4;
                            double Pe = SPhysics.rho_w * mV * Hk / (2 * mu);
                            double Ax = 0;
                            double Ay = 0;
                            if (MEM.Equals(mV, 0) != true)
                            {
                                Ax = eVx / mV;
                                Ay = eVy / mV;
                            }
                            double eMu = (eddyViscosity[i0] + eddyViscosity[i1] + eddyViscosity[i2]) / 3.0;
                            double Se3 = Se / 3;
                            double eUx = (Ux[i0] + Ux[i1] + Ux[i2]) / 3;
                            double Ux2 = SigmaTask * eUx * eUx;

                            // вычисление ЛЖМ для задачи Рейнольдса
                            for (int ai = 0; ai < cu; ai++)
                            {
                                li = cs * ai;
                                double L_SUPG = omega * (Hx[elem] * Ax * b[ai] + Hy[elem] * Ay * c[ai]);
                                double Lb = (1 / 3.0 + L_SUPG);
                                double La = rho_w * Lb;
                                double F_a = Lb * Se;
                                double F_b = F_a;
                                for (int aj = 0; aj < cu; aj++)
                                {
                                    lj = cs * aj;
                                    // матрица масс
                                    double M_ab = MM[ai][aj] * Se;
                                    double _m_ab = M_ab + L_SUPG * Se3;
                                    double _M_ab = rho_w * _m_ab;
                                    // матрица жосткости - вязкие члены плоские
                                    double K_ab = (b[ai] * b[aj] + c[ai] * c[aj]) * Se;

                                    // конверктивные члены - скорости
                                    double C_agb = La * ((c[0] * b[aj] - b[0] * c[aj]) * Phi[i0] +
                                                         (c[1] * b[aj] - b[1] * c[aj]) * Phi[i1] +
                                                         (c[2] * b[aj] - b[2] * c[aj]) * Phi[i2]) * Se;
                                    // конверктивные члены - вязкости
                                    double D_agb = Lb * ((b[0] * b[aj] - c[0] * c[aj]) * eddyViscosity[i0] +
                                                         (b[1] * b[aj] - c[1] * c[aj]) * eddyViscosity[i1] +
                                                         (b[2] * b[aj] - c[2] * c[aj]) * eddyViscosity[i2]) * Se;

                                    double sWR_ab = SigmaTask * (eMu * b[ai] * F_b / Radius + eVy * _M_ab / Radius);

                                    //LaplMatrix[li][lj]     =   (1 - theta) * (K_ab + SigmaTask * F_a*b[aj]/Radius );
                                    //LaplMatrix[li][lj + 1] = - (1 - theta) * _m_ab * Radius;
                                    //LaplMatrix[li + 1][lj] = 0;
                                    //LaplMatrix[li + 1][lj + 1] = _M_ab + dt * (1 - theta) * (C_agb / Radius - D_agb + eMu * K_ab + sWR_ab);

                                    //RMatrix[li][lj] =    - theta * (K_ab + SigmaTask * F_a * b[aj] / Radius);
                                    //RMatrix[li][lj + 1] =  theta * _m_ab;
                                    //RMatrix[li + 1][lj] =  0;
                                    //RMatrix[li + 1][lj + 1] = _M_ab - dt * theta * (C_agb / Radius - D_agb + eMu * K_ab + sWR_ab);


                                    LaplMatrix[li][lj] = K_ab + SigmaTask * F_a * b[aj] / Radius;
                                    LaplMatrix[li][lj + 1] = -_m_ab * Radius;
                                    LaplMatrix[li + 1][lj] = 0;
                                    LaplMatrix[li + 1][lj + 1] = _M_ab + dt * (1 - theta) * (C_agb / Radius - D_agb + eMu * K_ab + sWR_ab);

                                    RMatrix[li][lj] = 0;
                                    RMatrix[li][lj + 1] = 0;
                                    RMatrix[li + 1][lj] = 0;
                                    RMatrix[li + 1][lj + 1] = _M_ab - dt * theta * (C_agb / Radius - D_agb + eMu * K_ab + sWR_ab);

                                }
                                LocalRight[li] = 0;
                                LocalRight[li + 1] = rho_w * c[ai] * Se3 * Ux2 / Radius;
                            }
                            // получем значения адресов неизвестных
                            GetAdress(knots, ref adressBound);
                            // добавление вновь сформированной ЛЖМ в ГМЖ
                            algebra.AddToMatrix(LaplMatrix, adressBound);
                            // добавление вновь сформированной ЛЖМ в ГМЖ
                            Ralgebra.AddToMatrix(RMatrix, adressBound);
                        }
                        testflag = 1;
                        // Расчет
                        Ralgebra.getResidual(ref MRight, result_old, 0);
                        testflag = 2;
                        algebra.CopyRight(MRight);
                        testflag = 3;
                        // Выполнение граничных условий для функции вихря
                        VortexBC();
                        testflag = 4;
                        // Выполнение граничных условий для функции тока
                        algebra.BoundConditions(0, bcIndex);
                        testflag = 5;
                        //algebra.Print();
                        // решение
                        algebra.Solve(ref result);
                        testflag = 6;
                        for (int i = 0; i < result.Length; i++)
                            result_cur[i] = relax * result[i] + (1 - relax) * result_cur[i];
                        for (int i = 0; i < mesh.CountKnots; i++)
                        {
                            Phi[i] = result_cur[2 * i];
                            _Phi[i] = Phi[i];
                            Vortex[i] = result_cur[2 * i + 1];
                            _Vortex[i] = Vortex[i];
                        }
                        testflag = 7;
                        //************************************************************************************************************************
                        // Считаем невязку
                        //************************************************************************************************************************
                        double epsVortex = 0.0;
                        double normVortex = 0.0;

                        for (int i = 0; i < result.Length; i++)
                        {
                            normVortex += result_cur[i] * result_cur[i];
                            epsVortex += (result_cur[i] - result[i]) * (result_cur[i] - result[i]);
                        }
                        double residual = Math.Sqrt(epsVortex / normVortex);
                        Console.WriteLine("n {0} residual {1}", nIdx, residual);
                        if (residual < MEM.Error5)
                            break;
                        MEM.MemCopy(ref result_cur, result);
                    }
                }
                // расчет поля скорости
                Console.WriteLine("Vy ~ Vz:");
                CalcVelosityPlane(Phi, ref Vy, ref Vz);
                MEM.MemCopy(ref result_old, result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ee = " + ee.ToString() + " " + ex.Message);
            }

        }

    }
}



