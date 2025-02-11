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
//                  - (C) Copyright 2024
//                        Потапов И.И.
//                         07.01.25
//---------------------------------------------------------------------------
//                         04.01.2025
// Тестовая не стационарная задача Навье - Стокса в переменных Phi,Vortex
// две степени свободы в узле, со слабыми граничными условиями для Vortex
//---------------------------------------------------------------------------
namespace FEMTasksLib.FEMTasks.VortexStream
{
    using MemLogLib;
    using CommonLib;

    using System;
    using CommonLib.Physics;
    using CommonLib.Mesh;

    /// <summary>
    ///  ОО: Решатель для задачи Ламе на трехузловой сетке
    /// </summary>
    [Serializable]
    public class NSVortexStreamTri : VortexStreamTri
    {
        /// <summary>
        /// Параметр схемы по времени
        /// </summary>
        protected double theta = 0.5;
        /// <summary>
        /// Шаг по времени
        /// </summary>
        protected double dt;
        /// <summary>
        /// Расчетное время
        /// </summary>
        protected double Time;
        /// <summary>
        /// Итераций по нелинейности на текущем шаге по времени
        /// </summary>
        protected int NLine;
        /// <summary>
        /// Для правой части
        /// </summary>
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
        /// Предыдущее решение по нелинейности
        /// </summary>
        protected double[] result_cur = null;
        /// <summary>
        /// Функция тока
        /// </summary>
        protected double[] Phi_old;
        /// <summary>
        /// Тип задачи уравнения Рейнольдса или уравнения Навье-Стокса 
        /// </summary>
        protected bool ReTask;
        #endregion
        public NSVortexStreamTri(double V, double[] eddyViscosity, double dt, 
        double Time, int NLine, int SigmaTask, double RadiusMin, double[] Ux = null) 
            : base(V, eddyViscosity, 1, SigmaTask, RadiusMin, Ux)
        {
            this.dt = dt;
            this.Time = Time;
            this.NLine = NLine;
            ReTask = true;
        }
        public NSVortexStreamTri(double V, double mu, double dt, double Time, 
        int NLine, int SigmaTask, double RadiusMin, double[] Ux = null) 
            : base(V, mu, 1, SigmaTask, RadiusMin, Ux)
        {
            this.V = V;
            this.dt = dt;
            this.Time = Time;
            this.NLine = NLine;
            ReTask = false;
        }
        public override void SetTask(IMesh mesh, IAlgebra algebra, IMeshWrapper wMesh = null)
        {
            base.SetTask(mesh, algebra, wMesh);
            MEM.Alloc(mesh.CountKnots, ref Phi_old);
            MEM.Alloc(cs * mesh.CountKnots, ref MRight);
            MEM.Alloc(cs * mesh.CountKnots, ref result_cur);
            Ralgebra = algebra.Clone();
        }
        /// <summary>
        /// Метод для диагностики
        /// </summary>
        public override void Solve()
        {
            int NT = (int)(Time / dt) + 1;
            double[] result = null;
            if (ReTask == false)
            {
                for (int i = 0; i < NT; i++)
                {
                    SolveTask(ref result);
                    Console.WriteLine("Время :" + (i * dt).ToString("F6"));
                }
            }
            else
            {
                for (int i = 0; i < NT; i++)
                {
                    SolveTaskRe(ref result);
                    Console.WriteLine("Время :" + (i * dt).ToString("F6"));
                }
            }
        }
        /// <summary>
        /// Решение одного шага по времени для задачи Навье - Стокса
        /// </summary>
        /// <param name="result"></param>
        public override void SolveTask(ref double[] result)
        {
            uint ee = 0;
            try
            {
                if (flagStart == true)
                {
                    // На первом шаге решаем задачу Стокса
                    base.SolveTask(ref result_old);
                    flagStart = false;
                    MEM.MemCopy(ref result, result_old);
                }
                else
                {
                    
                    // итераций по нелинейности на текущем шаге по времени
                    for (int nIdx = 0; nIdx < NLine; nIdx++)
                    {
                        MEM.MemCopy(ref result_cur, result);
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
                            
                            double eVx = (Vy[i0] + Vy[i1] + Vy[i2]) / 3;
                            double eVy = (Vz[i0] + Vz[i1] + Vz[i2]) / 3;
                            double mV = Math.Sqrt(eVx * eVx + eVy * eVy);
                            double Hk = Math.Sqrt(Se)/1.4;
                            double Pe = SPhysics.rho_w * mV * Hk / (2 * mu);
                            double Ax = 0;
                            double Ay = 0;
                            if (MEM.Equals(mV, 0) != true)
                            {
                                Ax = eVx / mV;
                                Ay = eVy / mV;
                            }
                            // вычисление ЛЖМ для задачи Навье - Стокса/Стокса
                            for (int ai = 0; ai < cu; ai++)
                            {
                                double N_NVx = (Vy[i0] * MM[ai][0] + Vy[i1] * MM[ai][1] + Vy[i2] * MM[ai][2]) * S[elem];
                                double N_NVy = (Vz[i0] * MM[ai][0] + Vz[i1] * MM[ai][1] + Vz[i2] * MM[ai][2]) * S[elem];
                                double L_SUPG = omega * (Hx[elem] * Ax * b[ai] + Hy[elem] * Ay * c[ai]);
                                li = cs * ai;
                                for (int aj = 0; aj < cu; aj++)
                                {
                                    lj = cs * aj;
                                    double K_ab = (b[ai] * b[aj] + c[ai] * c[aj]) * Se;
                                    double M_ab = MM[ai][aj] * Se;
                                    // конверктивные члены плоские
                                    double C_ab = N_NVx * b[aj] + N_NVy * c[aj];
                                    // конверктивные члены плоские SUPG
                                    double C_SUPG = L_SUPG * (eVx * b[aj] + eVy * c[aj]) * S[elem] / 3;
                                    //  вязкие члены плоские
                                    double additionD = mu * (b[ai] * b[aj] + c[ai] * c[aj]) * S[elem];
                                    LaplMatrix[li][lj]     =  (1 - theta) * K_ab* (1 - theta);
                                    LaplMatrix[li][lj + 1] = - (1 - theta) * M_ab * (1 - theta);
                                    LaplMatrix[li + 1][lj] = 0;
                                    LaplMatrix[li + 1][lj + 1] =  rho_w * M_ab  + dt * (1 - theta) * ((C_ab + C_SUPG) * rho_w + mu * K_ab) ;

                                    RMatrix[li][lj] = - K_ab * theta;
                                    RMatrix[li][lj + 1] =  M_ab * theta;
                                    RMatrix[li + 1][lj] = 0;
                                    RMatrix[li + 1][lj + 1] = rho_w * M_ab - dt * theta * ((C_ab + C_SUPG) * rho_w + mu * K_ab);
                                }
                            }
                            // получем значения адресов неизвестных
                            GetAdress(knots, ref adressBound);
                            // добавление вновь сформированной ЛЖМ в ГМЖ
                            algebra.AddToMatrix(LaplMatrix, adressBound);
                            // добавление вновь сформированной ЛЖМ в ГМЖ
                            Ralgebra.AddToMatrix(RMatrix, adressBound);
                        }
                        // Расчет
                        Ralgebra.getResidual(ref MRight, result_old, 0);
                        algebra.CopyRight(MRight);
                        //double Right;
                        //uint idx;
                        //for (uint belem = 0; belem < mesh.CountBoundElements; belem++)
                        //{
                        //    int mark = mesh.GetBoundElementMarker(belem);

                        //    if (mark == 2)
                        //    {
                        //        uint i1 = beKnots[belem].Vertex1;
                        //        uint i2 = beKnots[belem].Vertex2;
                        //        double Le = Math.Sqrt((X[i1] - X[i2]) * (X[i1] - X[i2]) + (Y[i1] - Y[i2]) * (Y[i1] - Y[i2]));
                        //        Right = V * Le;
                        //    }
                        //    else
                        //        Right = 0;
                        //    if (belem == 0)
                        //    {
                        //        idx = beKnots[belem].Vertex1;
                        //        VortexBC((uint)(cs * idx), ColAdress, Right);
                        //    }
                        //    idx = beKnots[belem].Vertex2;
                        //    VortexBC((uint)(cs * idx), ColAdress, Right);
                        //}
                        // Выполнение граничных условий для функции вихря
                        VortexBC();
                        // Выполнение граничных условий для функции тока
                        algebra.BoundConditions(0, bcIndex);
                        //algebra.Print();
                        // решение
                        algebra.Solve(ref result);

                        for (int i = 0; i < result.Length; i++)
                            result_cur[i] = relax * result[i] + (1 - relax) * result_cur[i];
                        for (int i = 0; i < mesh.CountKnots; i++)
                        {
                            Phi[i] = result_cur[2 * i];
                            Vortex[i] = result_cur[2 * i + 1];
                        }
                        // расчет поля скорости
                        Console.WriteLine("Vy ~ Vz:");
                        CalcVelosityPlane(Phi, ref Vy, ref Vz);
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
                    }
                    MEM.MemCopy(ref result_old, result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ee = " + ee.ToString() + " " + ex.Message);
            }
        
        }

        /// <summary>
        /// Решение одного шага по времени для задачи Навье - Стокса
        /// </summary>
        /// <param name="result"></param>
        public void SolveTaskRe(ref double[] result)
        {
            uint ee = 0;
            try
            {
                if (flagStart == true)
                {
                    // На первом шаге решаем задачу Стокса
                    base.SolveTask(ref result_old);
                    flagStart = false;
                    MEM.MemCopy(ref result, result_old);
                }
                else
                {

                    // итераций по нелинейности на текущем шаге по времени
                    for (int nIdx = 0; nIdx < NLine; nIdx++)
                    {
                        MEM.MemCopy(ref result_cur, result);
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

                            //double eVx = (Vy[i0] + Vy[i1] + Vy[i2]) / 3;
                            //double eVy = (Vz[i0] + Vz[i1] + Vz[i2]) / 3;
                            //double mV = Math.Sqrt(eVx * eVx + eVy * eVy);
                            //double Hk = Math.Sqrt(Se) / 1.4;
                            //double Pe = SPhysics.rho_w * mV * Hk / (2 * mu);
                            //double Ax = 0;
                            //double Ay = 0;
                            //if (MEM.Equals(mV, 0) != true)
                            //{
                            //    Ax = eVx / mV;
                            //    Ay = eVy / mV;
                            //}
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

                            // вычисление ЛЖМ для задачи Навье - Стокса/Стокса
                            for (int ai = 0; ai < cu; ai++)
                            {
                                //double N_NVx = (Vy[i0] * MM[ai][0] + Vy[i1] * MM[ai][1] + Vy[i2] * MM[ai][2]) * S[elem];
                                //double N_NVy = (Vz[i0] * MM[ai][0] + Vz[i1] * MM[ai][1] + Vz[i2] * MM[ai][2]) * S[elem];

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

                                    LaplMatrix[li][lj] = (1 - theta) * K_ab ;
                                    LaplMatrix[li][lj + 1] = - (1 - theta) * M_ab;
                                    LaplMatrix[li + 1][lj] = 0;
                                    LaplMatrix[li + 1][lj + 1] = rho_w * M_ab + dt * (1 - theta) * (C_agb - D_agb + eMu * K_ab);

                                    RMatrix[li][lj] = - K_ab * theta;
                                    RMatrix[li][lj + 1] =  M_ab * theta;
                                    RMatrix[li + 1][lj] = 0;
                                    RMatrix[li + 1][lj + 1] = rho_w * M_ab + dt * theta * (C_agb - D_agb + eMu * K_ab);
                                }
                            }
                            // получем значения адресов неизвестных
                            GetAdress(knots, ref adressBound);
                            // добавление вновь сформированной ЛЖМ в ГМЖ
                            algebra.AddToMatrix(LaplMatrix, adressBound);
                            // добавление вновь сформированной ЛЖМ в ГМЖ
                            Ralgebra.AddToMatrix(RMatrix, adressBound);
                        }
                        // Расчет
                        Ralgebra.getResidual(ref MRight, result_old, 0);
                        algebra.CopyRight(MRight);
                        //double Right;
                        //uint idx;
                        //for (uint belem = 0; belem < mesh.CountBoundElements; belem++)
                        //{
                        //    int mark = mesh.GetBoundElementMarker(belem);

                        //    if (mark == 2)
                        //    {
                        //        uint i1 = beKnots[belem].Vertex1;
                        //        uint i2 = beKnots[belem].Vertex2;
                        //        double Le = Math.Sqrt((X[i1] - X[i2]) * (X[i1] - X[i2]) + (Y[i1] - Y[i2]) * (Y[i1] - Y[i2]));
                        //        Right = V * Le;
                        //    }
                        //    else
                        //        Right = 0;
                        //    if (belem == 0)
                        //    {
                        //        idx = beKnots[belem].Vertex1;
                        //        VortexBC((uint)(cs * idx), ColAdress, Right);
                        //    }
                        //    idx = beKnots[belem].Vertex2;
                        //    VortexBC((uint)(cs * idx), ColAdress, Right);
                        //}
                        // Выполнение граничных условий для функции вихря
                        VortexBC();
                        // Выполнение граничных условий для функции тока
                        algebra.BoundConditions(0, bcIndex);
                        //algebra.Print();
                        // решение
                        algebra.Solve(ref result);

                        for (int i = 0; i < result.Length; i++)
                            result_cur[i] = relax * result[i] + (1 - relax) * result_cur[i];
                        for (int i = 0; i < mesh.CountKnots; i++)
                        {
                            Phi[i] = result_cur[2 * i];
                            Vortex[i] = result_cur[2 * i + 1];
                        }
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
                    }
                    // расчет поля скорости
                    Console.WriteLine("Vy ~ Vz:");
                    CalcVelosityPlane(Phi, ref Vy, ref Vz);
                    MEM.MemCopy(ref result_old, result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ee = " + ee.ToString() + " " + ex.Message);
            }

        }
        /// <summary>
        /// Решение задачи Стокса
        /// </summary>
        /// <param name="result"></param>
        public void STSolveTask(ref double[] result)
        {
            uint ee = 0;
            try
            {
                if (flagStart == true)
                {
                    // На первом шаге решаем задачу Стокса
                    base.SolveTask(ref result_old);
                    flagStart = false;
                }
                else
                {
                    // итераций по нелинейности на текущем шаге по времени
                    for (int nIdx = 0; nIdx < NLine; nIdx++)
                    {
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

                            double eVx = (Vy[i0] + Vy[i1] + Vy[i2]) / 3;
                            double eVy = (Vz[i0] + Vz[i1] + Vz[i2]) / 3;
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
                            // вычисление ЛЖМ для задачи Навье - Стокса/Стокса
                            for (int ai = 0; ai < cu; ai++)
                            {
                                double N_NVx = (Vy[i0] * MM[ai][0] + Vy[i1] * MM[ai][1] + Vy[i2] * MM[ai][2]) * S[elem];
                                double N_NVy = (Vz[i0] * MM[ai][0] + Vz[i1] * MM[ai][1] + Vz[i2] * MM[ai][2]) * S[elem];
                                double L_SUPG = omega * (Hx[elem] * Ax * b[ai] + Hy[elem] * Ay * c[ai]);
                                li = cs * ai;
                                for (int aj = 0; aj < cu; aj++)
                                {
                                    lj = cs * aj;
                                    double K_ab = (b[ai] * b[aj] + c[ai] * c[aj]) * Se;
                                    double M_ab = MM[ai][aj] * Se;
                                    // конверктивные члены плоские
                                    double C_ab = N_NVx * b[aj] + N_NVy * c[aj];
                                    // конверктивные члены плоские SUPG
                                    double C_SUPG = L_SUPG * (eVx * b[aj] + eVy * c[aj]) * S[elem] / 3;
                                    //  вязкие члены плоские
                                    double additionD = mu * (b[ai] * b[aj] + c[ai] * c[aj]) * S[elem];
                                    LaplMatrix[li][lj] = (1 - theta) * K_ab;
                                    LaplMatrix[li][lj + 1] = -(1 - theta) * M_ab;
                                    LaplMatrix[li + 1][lj] = 0;
                                    LaplMatrix[li + 1][lj + 1] = rho_w * M_ab + dt * (1 - theta) * ((C_ab + C_SUPG) * rho_w + mu * K_ab);

                                    RMatrix[li][lj] = -K_ab * theta;
                                    RMatrix[li][lj + 1] = M_ab * theta;
                                    RMatrix[li + 1][lj] = 0;
                                    RMatrix[li + 1][lj + 1] = rho_w * M_ab - dt * theta * ((C_ab + C_SUPG) * rho_w + mu * K_ab);
                                }
                            }
                            // получем значения адресов неизвестных
                            GetAdress(knots, ref adressBound);
                            // добавление вновь сформированной ЛЖМ в ГМЖ
                            algebra.AddToMatrix(LaplMatrix, adressBound);
                            // добавление вновь сформированной ЛЖМ в ГМЖ
                            Ralgebra.AddToMatrix(RMatrix, adressBound);
                        }
                        // Расчет
                        Ralgebra.getResidual(ref MRight, result_old, 0);
                        algebra.CopyRight(MRight);
                        //double Right;
                        //uint idx;
                        //for (uint belem = 0; belem < mesh.CountBoundElements; belem++)
                        //{
                        //    int mark = mesh.GetBoundElementMarker(belem);

                        //    if (mark == 2)
                        //    {
                        //        uint i1 = beKnots[belem].Vertex1;
                        //        uint i2 = beKnots[belem].Vertex2;
                        //        double Le = Math.Sqrt((X[i1] - X[i2]) * (X[i1] - X[i2]) + (Y[i1] - Y[i2]) * (Y[i1] - Y[i2]));
                        //        Right = V * Le;
                        //    }
                        //    else
                        //        Right = 0;
                        //    if (belem == 0)
                        //    {
                        //        idx = beKnots[belem].Vertex1;
                        //        VortexBC((uint)(cs * idx), ColAdress, Right);
                        //    }
                        //    idx = beKnots[belem].Vertex2;
                        //    VortexBC((uint)(cs * idx), ColAdress, Right);
                        //}
                        // Выполнение граничных условий для функции вихря
                        VortexBC();
                        // Выполнение граничных условий для функции тока
                        algebra.BoundConditions(0, bcIndex);
                        //algebra.Print();
                        // решение
                        algebra.Solve(ref result);
                        //algebra.Print();
                        for (int i = 0; i < mesh.CountKnots; i++)
                        {
                            Phi[i] = result[2 * i];
                            Vortex[i] = result[2 * i + 1];
                        }
                        // расчет поля скорости
                        Console.WriteLine("Vy ~ Vz:");
                        CalcVelosityPlane(Phi, ref Vy, ref Vz);
                        //************************************************************************************************************************
                        // Считаем невязку
                        //************************************************************************************************************************
                        double epsVortex = 0.0;
                        double normVortex = 0.0;

                        for (int i = 0; i < result.Length; i++)
                        {
                            normVortex += result[i] * result[i];
                            epsVortex += (result[i] - result_old[i]) * (result[i] - result_old[i]);
                        }
                        double residual = Math.Sqrt(epsVortex / normVortex);
                        Console.WriteLine("n {0} residual {1}", nIdx, residual);
                        if (residual < MEM.Error5)
                            break;
                    }
                    MEM.MemCopy(ref result_old, result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ee = " + ee.ToString() + " " + ex.Message);
            }

        }

    }
}



