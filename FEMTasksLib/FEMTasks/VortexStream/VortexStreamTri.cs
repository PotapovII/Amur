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
//                          21.12.24 
//---------------------------------------------------------------------------
// Тестовая задача Стокса в переменных Phi,Vortex
// две степени свободы в узле, со слабыми граничными условиями для Vortex
//---------------------------------------------------------------------------
//                         04.01.2025
// Тестовая стационарная задача Навье - Стокса в переменных Phi,Vortex
//                         SolveTaskNS
//---------------------------------------------------------------------------
namespace FEMTasksLib.FEMTasks.VortexStream
{
    using MemLogLib;
    using AlgebraLib;
    using CommonLib;
    using CommonLib.Mesh;
    using CommonLib.Physics;

    using System;
    using CommonLib.Function;

    /// <summary>
    ///  ОО: Решатель для задачи Ламе на трехузловой сетке
    /// </summary>
    [Serializable]
    public class VortexStreamTri : AWRAP_FETaskTri
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
        /// Скорость на WL
        /// </summary>
        protected double V;
        /// <summary>
        /// Количество неизвестных в ЛМЖ
        /// </summary>
        protected int Count;
        /// <summary>
        /// Количество неизвестных в САУ
        /// </summary>
        protected int CountU;
        /// <summary>
        /// локальная матрица Якоби
        /// </summary>
        private double[][] JMatrix = null;
        /// <summary>
        /// Радиальная/боковая скорости на WL
        /// </summary>
        public IDigFunction VelosityUy = null;

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
        /// <summary>
        /// функция вихря
        /// </summary>
        public double[] Vortex;
        public double[] Vortex_old;

        /// <summary>
        /// Предыдущее решение
        /// </summary>
        protected double[] result_old = null;
        #endregion
        
        public VortexStreamTri(double V, double[] eddyViscosity, int NL_max,
            int SigmaTask, double RadiusMin, double[] Ux = null)
        {
            cs = 2;
            this.V = V;
            this.eddyViscosity = eddyViscosity;
            this.NL_max = NL_max;
            this.SigmaTask = SigmaTask;
            this.RadiusMin = RadiusMin;
            this.Ux = Ux;
            if (SigmaTask > 0 && (MEM.Equals(RadiusMin, 0) == true || Ux == null))
                throw new Exception("Не определен радиус закругдения русла");
        }
        public VortexStreamTri(double V, double mu, int NL_max, 
            int SigmaTask, double RadiusMin, double[] Ux = null)
        {
            cs = 2;
            this.V = V;
            this.mu = mu;
            this.NL_max = NL_max;
            this.SigmaTask = SigmaTask;
            this.RadiusMin = RadiusMin;
            this.Ux = Ux;
            if (SigmaTask > 0 && (MEM.Equals(RadiusMin, 0) == true || Ux == null))
                throw new Exception("Не определен радиус закругдения русла");
        }
        public VortexStreamTri(IDigFunction VelosityUy, int NL_max,
            int SigmaTask, double RadiusMin)
        {
            cs = 2;
            this.V = 0.1;
            this.mu = 0.1;
            this.NL_max = NL_max;
            this.SigmaTask = SigmaTask;
            this.RadiusMin = RadiusMin;
            this.VelosityUy = VelosityUy;
            this.Ux = Ux;
            Ux = null;
            if (SigmaTask > 0 && (MEM.Equals(RadiusMin, 0) == true))
                throw new Exception("Не определен радиус закругдения русла");
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
            
            if (eddyViscosity == null || eddyViscosity.Length != mesh.CountKnots)
            {
                MEM.Alloc(mesh.CountKnots, ref eddyViscosity);
                for (int i = 0; i < eddyViscosity.Length; i++)
                    eddyViscosity[i] = mu;
            }
            MEM.Alloc(cs * mesh.CountKnots, ref result_old);
            
            if (algebra==null)
                algebra = new AlgebraGauss((uint)CountU);

            for (int i = 0; i < bcIndex.Length; i++)
                bcIndex[i] = (uint)(Index[i] * cs);
            MEM.Alloc(algebra.N, ref ColAdress);
            for (uint i = 0; i < ColAdress.Length; i++)
                ColAdress[i] = i;
        }
        /// <summary>
        /// Метод для диагностики задачи Стокса
        /// </summary>
        public virtual void Solve()
        {
            double[] result = null;
            SolveTask(ref result);
        }
        /// <summary>
        /// Решение уравнений Стокса
        /// </summary>
        /// <param name="result"></param>
        public override void SolveTask(ref double[] result)
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
                    GetAdress(knots, ref adressBound);

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
                        double Radius = RadiusMin + (X[i0] + X[i1] + X[i2])/3;
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
                                LaplMatrix[li][lj + 1] = - M_ab;
                                LaplMatrix[li + 1][lj] = 0;
                                LaplMatrix[li + 1][lj + 1] = D_ab_Mu + eMu * K_ab + RD_ab;
                            }
                            LocalRight[li] = 0;
                            LocalRight[li + 1] = rho_w * c[ai] * Se3 * Ux2/Radius;
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
        /// Решение уравнений  Навье - Стокса методом простой итерации
        /// </summary>
        public void SolveTaskNS()
        {
            double[] result = null;
            uint ee = 0;
            try
            {
                for (int nIdx = 0; nIdx < NL_max; nIdx++)
                {
                    if (flagStart == true)
                    {
                        // На первом шаге решаем задачу Стокса
                        SolveTask(ref result_old);
                        for (int i = 0; i < mesh.CountKnots; i++)
                        {
                            Phi[i] = result_old[2 * i];
                            Vortex[i] = result_old[2 * i + 1];
                        }
                        flagStart = false;
                    }
                    else
                    {
                        MEM.MemCopy(ref Vortex_old, Vortex);
                        // расчет функции тока и функции вихря
                        Console.WriteLine("Phi & Vortex:");
                        MEM.Alloc((int)algebra.N, ref result);
                        algebra.Clear();
                        int li, lj;
                        for (uint elem = 0; elem < mesh.CountElements; elem++)
                        {
                            ee = elem;
                            // локальная матрица часть СЛАУ
                            MEM.Alloc2DClear(Count, ref JMatrix);
                            MEM.AllocClear(Count, ref LocalRight);
                            double[] b = dNdx[elem];
                            double[] c = dNdy[elem];
                            uint i0 = eKnots[elem].Vertex1;
                            uint i1 = eKnots[elem].Vertex2;
                            uint i2 = eKnots[elem].Vertex3;
                            // получить узлы КЭ
                            uint[] knots = { i0, i1, i2 };
                            double Se = S[elem];
                            double eVx =   c[0] * Phi[i0] + c[1] * Phi[i1] + c[2] * Phi[i2];
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
                            // вычисление ЛЖМ для задачи Навье - Стокса/Стокса
                            for (int ai = 0; ai < cu; ai++)
                            {
                                li = cs * ai;
                                double L_SUPG = omega * (Hx[elem] * Ax * b[ai] + Hy[elem] * Ay * c[ai]);
                                double La = rho_w * (1 / 3.0 + L_SUPG);
                                for (int aj = 0; aj < cu; aj++)
                                {
                                    lj = cs * aj;
                                    double K_ab = (b[ai] * b[aj] + c[ai] * c[aj]) * Se;
                                    double M_ab = MM[ai][aj] * Se;
                                    double C_agb = La * ((c[0] * b[aj] - b[0] * c[aj]) * Phi[i0] +
                                                         (c[1] * b[aj] - b[1] * c[aj]) * Phi[i1] +
                                                         (c[2] * b[aj] - b[2] * c[aj]) * Phi[i2]) * Se;
                                    JMatrix[li][lj] = K_ab;
                                    JMatrix[li][lj + 1] = - M_ab;
                                    JMatrix[li + 1][lj] = 0;
                                    JMatrix[li + 1][lj + 1] = C_agb + mu * K_ab;
                                }
                            }
                            // получем значения адресов неизвестных
                            GetAdress(knots, ref adressBound);
                            // добавление вновь сформированной ЛЖМ в ГМЖ
                            algebra.AddToMatrix(JMatrix, adressBound);
                        }
                        // Выполнение граничных условий для функции вихря
                        VortexBC();
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
                        algebra.BoundConditions(0, bcIndex);
                        //algebra.Print();
                        // Определение поправки
                        algebra.Solve(ref result);
                        // Поправка решения
                        
                        for (int i = 0; i < result.Length; i++)
                            result_old[i] = relax * result[i] + (1- relax)* result_old[i];
                        for (int i = 0; i < mesh.CountKnots; i++)
                        {
                            Phi[i] = result_old[2 * i];
                            Vortex[i] = result_old[2 * i + 1];
                        }
                        //************************************************************************************************************************
                        // Считаем невязку
                        //************************************************************************************************************************
                        double epsVortex = 0.0;
                        double normVortex = 0.0;

                        for (int i = 0; i < mesh.CountKnots; i++)
                        {
                            normVortex += Vortex[i] * Vortex[i];
                            epsVortex += (Vortex[i] - Vortex_old[i]) * (Vortex[i] - Vortex_old[i]);
                        }
                        double residual = Math.Sqrt(epsVortex / normVortex);
                        Console.WriteLine("n {0} residual {1}", nIdx, residual);
                        if (residual < MEM.Error5)
                            break;
                    }
                    // расчет поля скорости
                    Console.WriteLine("Vy ~ Vz:");
                    CalcVelosityPlane(Phi, ref Vy, ref Vz);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ee = " + ee.ToString() + " " + ex.Message);
            }
        }
        /// <summary>
        /// Решение уравнений Рейнольдса методом простой Ньютона
        /// </summary>
        public void SolveTaskRe()
        {
            double[] result = null;
            uint ee = 0;
            try
            {
                for (int nIdx = 0; nIdx < NL_max; nIdx++)
                {
                    if (flagStart == true)
                    {
                        // На первом шаге решаем задачу Стокса
                        SolveTask(ref result_old);
                        for (int i = 0; i < mesh.CountKnots; i++)
                        {
                            Phi[i] = result_old[2 * i];
                            Vortex[i] = result_old[2 * i + 1];
                        }
                        flagStart = false;
                    }
                    else
                    {
                        MEM.MemCopy(ref Vortex_old, Vortex);
                        // расчет функции тока и функции вихря
                        Console.WriteLine("Phi & Vortex:");
                        MEM.Alloc((int)algebra.N, ref result);
                        algebra.Clear();
                        int li, lj;
                        for (uint elem = 0; elem < mesh.CountElements; elem++)
                        {
                            ee = elem;
                            // локальная матрица часть СЛАУ
                            MEM.Alloc2DClear(Count, ref JMatrix);
                            MEM.AllocClear(Count, ref LocalRight);
                            double[] b = dNdx[elem];
                            double[] c = dNdy[elem];
                            uint i0 = eKnots[elem].Vertex1;
                            uint i1 = eKnots[elem].Vertex2;
                            uint i2 = eKnots[elem].Vertex3;
                            // получить узлы КЭ
                            uint[] knots = { i0, i1, i2 };
                            double Se = S[elem];
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
                                li = cs * ai;
                                double L_SUPG = omega * (Hx[elem] * Ax * b[ai] + Hy[elem] * Ay * c[ai]);
                                double Lb = (1 / 3.0 + L_SUPG);
                                double La = rho_w * Lb;

                                for (int aj = 0; aj < cu; aj++)
                                {
                                    lj = cs * aj;
                                    double K_ab = (b[ai] * b[aj] + c[ai] * c[aj]) * Se;
                                    double M_ab = MM[ai][aj] * Se;
                                    double C_agb = La * ((c[0] * b[aj] - b[0] * c[aj]) * Phi[i0] +
                                                                 (c[1] * b[aj] - b[1] * c[aj]) * Phi[i1] +
                                                                 (c[2] * b[aj] - b[2] * c[aj]) * Phi[i2]) * Se;
                                    double D_agb = Lb * ((b[0] * b[aj] - c[0] * c[aj]) * eddyViscosity[i0] +
                                                         (b[1] * b[aj] - c[1] * c[aj]) * eddyViscosity[i1] +
                                                         (b[2] * b[aj] - c[2] * c[aj]) * eddyViscosity[i2]) * Se;
                                    JMatrix[li][lj] = K_ab;
                                    JMatrix[li][lj + 1] = -M_ab;
                                    JMatrix[li + 1][lj] = 0;
                                    JMatrix[li + 1][lj + 1] = C_agb - D_agb + eMu * K_ab;
                                }
                            }
                            // получем значения адресов неизвестных
                            GetAdress(knots, ref adressBound);
                            // добавление вновь сформированной ЛЖМ в ГМЖ
                            algebra.AddToMatrix(JMatrix, adressBound);
                        }
                        // Выполнение граничных условий для функции вихря
                        VortexBC();
                        // Выполнение граничных условий для функции тока
                        algebra.BoundConditions(0, bcIndex);
                        //algebra.Print();
                        // Определение поправки
                        algebra.Solve(ref result);
                        // Поправка решения

                        for (int i = 0; i < result.Length; i++)
                            result_old[i] = relax * result[i] + (1 - relax) * result_old[i];
                        for (int i = 0; i < mesh.CountKnots; i++)
                        {
                            Phi[i] = result_old[2 * i];
                            Vortex[i] = result_old[2 * i + 1];
                        }
                        //************************************************************************************************************************
                        // Считаем невязку
                        //************************************************************************************************************************
                        double epsVortex = 0.0;
                        double normVortex = 0.0;

                        for (int i = 0; i < mesh.CountKnots; i++)
                        {
                            normVortex += Vortex[i] * Vortex[i];
                            epsVortex += (Vortex[i] - Vortex_old[i]) * (Vortex[i] - Vortex_old[i]);
                        }
                        double residual = Math.Sqrt(epsVortex / normVortex);
                        Console.WriteLine("n {0} residual {1} normVortex {2} ", nIdx, residual, normVortex);
                        if (residual < MEM.Error5)
                            break;
                    }
                    // расчет поля скорости
                    Console.WriteLine("Vy ~ Vz:");
                    CalcVelosityPlane(Phi, ref Vy, ref Vz);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ee = " + ee.ToString() + " " + ex.Message);
            }
        }
        /// <summary>
        /// Решение задачи Навье - Стокса методом Ньютона - Рафоса
        /// </summary>
        public void SolveTaskNR()
        {
            double[] result = null;
            uint ee = 0;
            try
            {

                for (int nIdx = 0; nIdx < 12; nIdx++)
                {
                    //if (flagStart == true)
                    //{
                    //    // На первом шаге решаем задачу Стокса
                    //    SolveTask(ref result_old);
                    //    for (int i = 0; i < mesh.CountKnots; i++)
                    //    {
                    //        Phi[i] = result_old[2 * i];
                    //        Vortex[i] = result_old[2 * i + 1];
                    //    }
                    //    flagStart = false;
                    //}
                    //else
                    {
                        MEM.MemCopy(ref Vortex_old, Vortex);
                        // расчет функции тока и функции вихря
                        Console.WriteLine("Phi & Vortex:");
                        MEM.Alloc((int)algebra.N, ref result);
                        algebra.Clear();
                        int li, lj;
                        for (uint elem = 0; elem < mesh.CountElements; elem++)
                        {
                            ee = elem;
                            // локальная матрица часть СЛАУ
                            MEM.Alloc2DClear(Count, ref JMatrix);
                            MEM.AllocClear(Count, ref LocalRight);
                            double[] b = dNdx[elem];
                            double[] c = dNdy[elem];
                            uint i0 = eKnots[elem].Vertex1;
                            uint i1 = eKnots[elem].Vertex2;
                            uint i2 = eKnots[elem].Vertex3;
                            // получить узлы КЭ
                            uint[] knots = { i0, i1, i2 };
                            double Se = S[elem];
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
                            // вычисление ЛЖМ для задачи Навье - Стокса/Стокса
                            for (int ai = 0; ai < cu; ai++)
                            {
                                li = cs * ai;
                                double L_SUPG = omega * (Hx[elem] * Ax * b[ai] + Hy[elem] * Ay * c[ai]);
                                double La = rho_w * (1 / 3.0 + L_SUPG);
                                double Rphi = 0;
                                double Rvortex = 0;
                                for (int aj = 0; aj < cu; aj++)
                                {
                                    lj = cs * aj;
                                    double K_ab = (b[ai] * b[aj] + c[ai] * c[aj]) * Se;
                                    double M_ab = MM[ai][aj] * Se;
                                    double C_agb = La * ((c[0] * b[aj] - b[0] * c[aj]) * Phi[i0] +
                                                         (c[1] * b[aj] - b[1] * c[aj]) * Phi[i1] +
                                                         (c[2] * b[aj] - b[2] * c[aj]) * Phi[i2]) * Se;
                                    double C_abg = La * ((c[aj] * b[0] - b[aj] * c[0]) * Vortex[i0] +
                                                         (c[aj] * b[1] - b[aj] * c[1]) * Vortex[i1] +
                                                         (c[aj] * b[2] - b[aj] * c[2]) * Vortex[i2]) * Se;

                                    JMatrix[li][lj] = K_ab;
                                    JMatrix[li][lj + 1] = -M_ab;
                                    JMatrix[li + 1][lj] = C_abg;
                                    JMatrix[li + 1][lj + 1] = C_agb + mu * K_ab;
                                    Rphi += K_ab * Phi[knots[aj]] - M_ab * Vortex[knots[aj]];
                                    Rvortex += (C_agb + mu * K_ab) * Vortex[knots[aj]];
                                }
                                LocalRight[li] = 0;// Rphi;
                                LocalRight[li + 1] = Rvortex;
                            }
                            // получем значения адресов неизвестных
                            GetAdress(knots, ref adressBound);
                            // добавление вновь сформированной ЛЖМ в ГМЖ
                            algebra.AddToMatrix(JMatrix, adressBound);
                            algebra.AddToRight(LocalRight, adressBound);
                        }
                        // Выполнение граничных условий для функции вихря
                        VortexBC();
                        // Выполнение граничных условий для функции тока
                        algebra.BoundConditions(0, bcIndex);
                        //algebra.Print();
                        // Определение поправки
                        algebra.Solve(ref result);
                        // Поправка решения
                        for (int i = 0; i < result.Length; i++)
                            result_old[i] += result[i];
                        for (int i = 0; i < mesh.CountKnots; i++)
                        {
                            Phi[i] = result_old[2 * i];
                            Vortex[i] = result_old[2 * i + 1];
                        }
                        //************************************************************************************************************************
                        // Считаем невязку
                        //************************************************************************************************************************
                        double epsVortex = 0.0;
                        double normVortex = 0.0;

                        for (int i = 0; i < mesh.CountKnots; i++)
                        {
                            normVortex += Vortex[i] * Vortex[i];
                            epsVortex += (Vortex[i] - Vortex_old[i]) * (Vortex[i] - Vortex_old[i]);
                        }
                        double residual = Math.Sqrt(epsVortex / normVortex);
                        Console.WriteLine("n {0} residual {1}", nIdx, residual);
                        if (residual < MEM.Error5)
                            break;
                    }
                    // расчет поля скорости
                    Console.WriteLine("Vy ~ Vz:");
                    CalcVelosityPlane(Phi, ref Vy, ref Vz);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ee = " + ee.ToString() + " " + ex.Message);
            }
        }

        /// <summary>
        /// Выполнение граничных условий для функции вихря
        /// </summary>
        public void VortexBC()
        {
            double Right;
            uint idx;
            for (uint belem = 0; belem < mesh.CountBoundElements; belem++)
            {
                int mark = mesh.GetBoundElementMarker(belem);
                if (mark == 2)
                {
                    uint i1 = beKnots[belem].Vertex1;
                    uint i2 = beKnots[belem].Vertex2;
                    double Le = Math.Sqrt((X[i1] - X[i2]) * (X[i1] - X[i2]) + (Y[i1] - Y[i2]) * (Y[i1] - Y[i2]));
                    if (VelosityUy == null)
                        Right = V * Le;
                    else
                    {
                        double y = 0.5 * (X[i1] + X[i2]);
                        Right = Le * VelosityUy.FunctionValue(y);
                    }
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
                {
                    sumVortex += ColElems[i]; ColElems[i] = 0;
                }
            }
            ColElems[IndexRow + 1] = sumVortex;
            IndexRow++;
            algebra.AddStringSystem(ColElems, ColAdress, IndexRow, R + Right);
        }

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
                VSUtils.VelocityZerroOnBed(mesh, ref Vx, ref Vy);
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

    }
}



