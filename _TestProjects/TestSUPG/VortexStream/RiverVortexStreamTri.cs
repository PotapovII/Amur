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
namespace TestSUPG
{
    using MemLogLib;
    using CommonLib;

    using System;
    using CommonLib.Physics;
    using CommonLib.Mesh;
    using CommonLib.Function;
    using FEMTasksLib.FEMTasks.VortexStream;
    using NPRiverLib.APRiver2XYD.River2DSW;
    using FEMTasksLib.FEMTasks.Utils;

    [Serializable]
    public delegate void CalkTask(ref double[] X);
    /// <summary>
    ///  ОО: Решатель для задачи Ламе на трехузловой сетке
    /// </summary>
    [Serializable]
    public class RiverVortexStreamTri : AWRAP_FETaskTri
    {
        /// <summary>
        /// Скорость по оси х
        /// </summary>
        protected double[] Ux;
        /// <summary>
        /// тип задачи 0 - плоская 1 - цилиндрическая
        /// </summary>
        protected int SigmaTask;
        /// <summary>
        /// радиус изгиба русла
        /// </summary>
        protected double RadiusMin;
        /// <summary>
        /// Релаксация при шаге по нелинейности
        /// </summary>
        protected double relax = 0.3;
        /// <summary>
        /// Итераций по нелинейности
        /// </summary>
        protected int NL_max;
        /// <summary>
        /// Флаг сборки рекурсивной системы
        /// </summary>
        protected bool flagStart = true;
        /// <summary>
        /// Параметр SUPG стабилизации 
        /// </summary>
        protected double omega = 0.5;
        /// <summary>
        /// Плотность воды
        /// </summary>
        protected double rho_w = SPhysics.rho_w;
        /// <summary>
        /// Турбулентная вязкость воды
        /// </summary>
        protected double mu = 1;
        /// <summary>
        /// вихревая вязкость
        /// </summary>
        public double[] eddyViscosity = null;
        /// <summary>
        /// Количество неизвестных в ЛМЖ
        /// </summary>
        protected int Count;
        /// <summary>
        /// Количество неизвестных в САУ
        /// </summary>
        protected int CountU;
        /// <summary>
        /// Нормальная скорость на WL
        /// </summary>
        public IDigFunction VelosityUx = null;
        /// <summary>
        /// Радиальная/боковая скорости на WL
        /// </summary>
        public IDigFunction VelosityUy = null;
        /// <summary>
        /// Параметр схемы по времени
        /// </summary>
        protected double theta = 0.5;
        /// <summary>
        /// Шаг по времени
        /// </summary>
        protected double dt;
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
        protected double[][] RMatrix = null;
        /// <summary>
        /// Решатель
        /// </summary>
        protected CalkTask solver = null;
        #region Поля результатов
        /// <summary>
        /// горизонтальая скорость в створе
        /// </summary>
        public double[] Vy;
        /// <summary>
        /// вертикальная скорость в створе
        /// </summary>
        public double[] Vz;
        /// <summary>
        /// Функция тока
        /// </summary>
        public double[] Phi;
        protected double[] Phi_old;
        /// <summary>
        /// функция вихря
        /// </summary>
        public double[] Vortex;
        public double[] Vortex_old;
        /// <summary>
        /// Текущее решение
        /// </summary>
        protected double[] result = null;
        /// <summary>
        /// Предыдущее решение по времени
        /// </summary>
        protected double[] result_old = null;
        /// <summary>
        /// Предыдущее решение по нелинейности
        /// </summary>
        protected double[] result_cur = null;
        /// <summary>
        /// Тип задачи уравнения Рейнольдса или уравнения Навье-Стокса 
        /// </summary>
        protected int ReTask;
        #endregion
        public RiverVortexStreamTri(){}
        /// <summary>
        /// Получение граничных условий
        /// </summary>
        /// <param name="VelosityUx"></param>
        /// <param name="VelosityUy"></param>
        public void SetBCVelosity(IDigFunction VelosityUx, IDigFunction VelosityUy,
            int NLine, int SigmaTask, double RadiusMin, int ReTask = 0)
        {
            this.VelosityUx = VelosityUx;
            this.VelosityUy= VelosityUy;
            this.NLine = NLine;
            this.SigmaTask = SigmaTask;
            this.RadiusMin = RadiusMin;
            this.ReTask = ReTask;
            switch (ReTask)
            {
                case 0: // задача Стокса
                    solver = SolveStokesTask;
                    break;
                case 1:
                    solver = SolveNavierStokes;
                    break;
                case 2:
                    solver = SolveReynolds;
                    break;
            }
        }
        public override void SetTask(IMesh mesh, IAlgebra algebra, IMeshWrapper wMesh = null)
        {
            base.SetTask(mesh, algebra, wMesh);
            Count = cs * cu;
            CountU = cs * mesh.CountKnots;
            MEM.Alloc(mesh.CountBoundKnots, ref bcIndex);
            MEM.Alloc(mesh.CountKnots, ref Vy);
            MEM.Alloc(mesh.CountKnots, ref Vz);
            MEM.Alloc(mesh.CountKnots, ref Phi);
            MEM.Alloc(mesh.CountKnots, ref Vortex);
            MEM.Alloc(mesh.CountKnots, ref Phi_old);
            MEM.Alloc(CountU, ref MRight);
            MEM.Alloc(CountU, ref result);
            MEM.Alloc(CountU, ref result_cur);
            MEM.Alloc(CountU, ref result_old);

            for (int i = 0; i < bcIndex.Length; i++)
                bcIndex[i] = (uint)(Index[i] * cs);
            MEM.Alloc(algebra.N, ref ColAdress);
            for (uint i = 0; i < ColAdress.Length; i++)
                ColAdress[i] = i;
            Ralgebra = algebra.Clone();
        }
        /// <summary>
        /// один шаг расчета по времени
        /// </summary>
        /// <param name="eddyViscosity"></param>
        /// <param name="dt"></param>
        /// <param name="Ux"></param>
        public void Solve(double[] eddyViscosity, double dt, double[] Ux = null)
        {
            this.dt = dt;
            this.eddyViscosity = eddyViscosity;
            this.Ux = Ux;
            solver(ref result);
        }
        /// <summary>
        /// Решение уравнений Стокса
        /// </summary>
        /// <param name="result"></param>
        public void SolveStokesTask(ref double[] result)
        {
            uint ee = 0;
            try
            {
                // расчет функции тока и функции вихря
                Console.WriteLine("Phi & Vortex:");
                MEM.Alloc((int)algebra.N, ref result);
                algebra.Clear();
                int li, lj;
                for (uint elem = 0; elem < mesh.CountElements; elem++)
                {
                    ee = elem;
                    // локальная матрица часть СЛАУ
                    MEM.Alloc2DClear(Count, ref LaplMatrix);
                    MEM.AllocClear(Count, ref LocalRight);
                    double[] b = dNdx[elem];
                    double[] c = dNdy[elem];
                    uint i0 = eKnots[elem].Vertex1;
                    uint i1 = eKnots[elem].Vertex2;
                    uint i2 = eKnots[elem].Vertex3;
                    // получить узлы КЭ
                    uint[] knots = { i0, i1, i2 };
                    double Se = S[elem];

                    // получем значения адресов неизвестных
                    FEMUtils.GetAdress(knots, ref adressBound);

                    if (SigmaTask == 0)
                    {
                        // вычисление ЛЖМ для задачи Стокса
                        for (int ai = 0; ai < cu; ai++)
                        {
                            li = cs * ai;
                            for (int aj = 0; aj < cu; aj++)
                            {
                                lj = cs * aj;
                                LaplMatrix[li][lj] = Se * (b[ai] * b[aj] + c[ai] * c[aj]);
                                LaplMatrix[li][lj + 1] = -Se * MM[ai][aj];
                                LaplMatrix[li + 1][lj] = 0;
                                LaplMatrix[li + 1][lj + 1] = Se * (b[ai] * b[aj] + c[ai] * c[aj]);
                            }
                        }
                    }
                    else
                    {
                        //RadiusMin
                        // вычисление ЛЖМ для задачи Навье - Стокса/Стокса
                        double Radius = RadiusMin + (X[i0] + X[i1] + X[i2]) / 3;
                        double eUx = (Ux[i0] + Ux[i1] + Ux[i2]) / 3;
                        double Ux2 = eUx * eUx;
                        double Se3 = Se / 3;
                        for (int ai = 0; ai < cu; ai++)
                        {
                            li = cs * ai;
                            for (int aj = 0; aj < cu; aj++)
                            {
                                lj = cs * aj;
                                double eMu = mu;
                                double D_ab_Mu = 0;
                                if (eddyViscosity != null)
                                {
                                    D_ab_Mu = ((c[0] * b[aj] - b[0] * c[aj]) * eddyViscosity[i0] +
                                               (c[1] * b[aj] - b[1] * c[aj]) * eddyViscosity[i1] +
                                               (c[2] * b[aj] - b[2] * c[aj]) * eddyViscosity[i2]) * Se3;
                                    eMu = (eddyViscosity[i0] + eddyViscosity[i0] + eddyViscosity[i0]) / 3;
                                }
                                double RD_ab = eMu * b[ai] * Se3 / Radius;
                                double K_ab = (b[ai] * b[aj] + c[ai] * c[aj]) * Se;
                                double M_ab = MM[ai][aj] * Se;

                                LaplMatrix[li][lj] = K_ab + Se3 * b[aj] / Radius;
                                LaplMatrix[li][lj + 1] = -M_ab;
                                LaplMatrix[li + 1][lj] = 0;
                                LaplMatrix[li + 1][lj + 1] = D_ab_Mu + eMu * K_ab + RD_ab;
                            }
                            LocalRight[li] = 0;
                            LocalRight[li + 1] = rho_w * c[ai] * Se3 * Ux2 / Radius;
                        }
                        algebra.AddToRight(LocalRight, adressBound);
                    }
                    // добавление вновь сформированной ЛЖМ в ГМЖ
                    algebra.AddToMatrix(LaplMatrix, adressBound);
                }
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
        public void SolveNavierStokes(ref double[] result)
        {
            uint ee = 0;
            try
            {
                if (flagStart == true)
                {
                    // На первом шаге решаем задачу Стокса
                    SolveStokesTask(ref result_old);
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
                            FEMUtils.GetAdress(knots, ref adressBound);
                            // добавление вновь сформированной ЛЖМ в ГМЖ
                            algebra.AddToMatrix(LaplMatrix, adressBound);
                            // добавление вновь сформированной ЛЖМ в ГМЖ
                            Ralgebra.AddToMatrix(RMatrix, adressBound);
                        }
                        // Расчет
                        Ralgebra.GetResidual(ref MRight, result_old, 0);
                        algebra.CopyRight(MRight);
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
        /// Решение задачи Рейнольдса
        /// </summary>
        /// <param name="result"></param>
        public void SolveReynolds(ref double[] result)
        {
            uint ee = 0;
            try
            {
                if (flagStart == true)
                {
                    // На первом шаге решаем задачу Стокса
                    SolveStokesTask(ref result_old);
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
                            FEMUtils.GetAdress(knots, ref adressBound);
                            // добавление вновь сформированной ЛЖМ в ГМЖ
                            algebra.AddToMatrix(LaplMatrix, adressBound);
                            // добавление вновь сформированной ЛЖМ в ГМЖ
                            Ralgebra.AddToMatrix(RMatrix, adressBound);
                        }
                        // Расчет
                        Ralgebra.GetResidual(ref MRight, result_old, 0);
                        algebra.CopyRight(MRight);
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
        /// <summary>
        /// Заглушка
        /// </summary>
        /// <param name="result"></param>
        public override void SolveTask(ref double[] result) { }
        #region TO DO : копия из VortexStreamTri позже абстрагировать 
        /// <summary>
        /// Расчет местных скоростей в узлах сетки
        /// </summary>
        /// <param name="Phi"></param>
        /// <param name="Vx"></param>
        /// <param name="Vy"></param>
        public void CalcVelosityPlane(double[] Phi, ref double[] Vx, ref double[] Vy)
        {
            try
            {
                double[] eVx = null;
                double[] eVy = null;
                CalcVelosity(ref eVx, ref eVy, Phi);
                wMesh.ConvertField(ref Vx, eVx);
                wMesh.ConvertField(ref Vy, eVy);
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
        }
        /// <summary>
        /// Расчет компонент поля скорости на КЭ по функции тока на КЭ
        /// </summary>
        /// <param name="Vx"></param>
        /// <param name="Vy"></param>
        /// <param name="Phi"></param>
        public void CalcVelosity(ref double[] Vx, ref double[] Vy, double[] Phi)
        {
            try
            {
                MEM.Alloc((uint)mesh.CountElements, ref Vx, "eVx");
                MEM.Alloc((uint)mesh.CountElements, ref Vy, "eVy");
                for (uint elem = 0; elem < mesh.CountElements; elem++)
                {
                    double[] b = dNdx[elem];
                    double[] c = dNdy[elem];
                    uint i0 = eKnots[elem].Vertex1;
                    uint i1 = eKnots[elem].Vertex2;
                    uint i2 = eKnots[elem].Vertex3;
                    double dPhidx = Phi[i0] * b[0] + Phi[i1] * b[1] + Phi[i2] * b[2];
                    double dPhidy = Phi[i0] * c[0] + Phi[i1] * c[1] + Phi[i2] * c[2];
                    Vx[elem] = dPhidy;
                    Vy[elem] = -dPhidx;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
        }

        /// <summary>
        /// Выполнение граничных условий для функции вихря
        /// </summary>
        public void VortexBC()
        {
            double Right;
            uint idx;
            var bLength =  wMesh.GetLb();
            for (uint belem = 0; belem < mesh.CountBoundElements; belem++)
            {
                int mark = mesh.GetBoundElementMarker(belem);
                if (mark == 2) // свободная поверхность
                {
                    uint i1 = beKnots[belem].Vertex1;
                    uint i2 = beKnots[belem].Vertex2;
                    double y = 0.5 * (X[i1] + X[i2]);
                    Right = VelosityUy.FunctionValue(y) * bLength[belem]; 
                }
                else
                    Right = 0;
                if (belem == 0)
                {
                    idx = beKnots[belem].Vertex1;
                    VortexBC((uint)(cs * idx), ColAdress, Right);
                }
                idx = beKnots[belem].Vertex2;
                VortexBC((uint)(cs * idx), ColAdress, Right);
            }
        }
        /// <summary>
        /// Выполнение ГУ
        /// </summary>
        /// <param name="IndexRow"></param>
        /// <param name="ColAdress"></param>
        /// <param name="Right"></param>
        protected void VortexBC(uint IndexRow, uint[] ColAdress, double Right)
        {
            double R = 0;
            double[] ColElems = null;
            MEM.Alloc(algebra.N, ref ColElems);
            algebra.GetStringSystem(IndexRow, ref ColElems, ref R);
            double sumVortex = 0;
            for (int i = 0; i < ColElems.Length; i++)
            {
                if (i % cs == 1)
                    sumVortex += ColElems[i]; ColElems[i] = 0;
            }
            ColElems[IndexRow + 1] = sumVortex;
            IndexRow++;
            algebra.AddStringSystem(ColElems, ColAdress, IndexRow, R + Right);
        }

        #endregion
    }
}



