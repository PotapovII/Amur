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
    using AlgebraLib;
    /// <summary>
    ///  ОО: Решатель для задачи Рейнольдс на трехузловой сетке,
    ///  для задачи (вынужденной конвекции/с учетом центробежных сил) в
    ///  створе канала с учетом скорости Ux или каверных с подвижной крышкой Ux = { 0,...,0 }
    /// </summary>
    [Serializable]
    public class ReynoldsVortexStream1YDTri : AWRAP_FETaskTri
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
        protected double relax = 0.9;
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
        /// Радиальная/боковая скорости на WL
        /// </summary>
        public IDigFunction VelosityUy = null;
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
        public ReynoldsVortexStream1YDTri(int NLine, int SigmaTask, double RadiusMin, IDigFunction VelosityUy, double theta = 0.5) 
        {
            cs = 2;
            this.theta = theta;
            this.NLine = NLine;
            this.NL_max = NLine;
            this.SigmaTask = SigmaTask;
            this.RadiusMin = RadiusMin;
            this.VelosityUy = VelosityUy;
            flagStart = true;
            if (SigmaTask > 0 && (MEM.Equals(RadiusMin, 0) == true))
                throw new Exception("Не определен радиус закругдения русла");
        }
        public override void SetTask(IMesh mesh, IAlgebra algebra, IMeshWrapper wMesh = null)
        {
            if(this.mesh == null)
                flagStart = true;
            else
            {
                if (this.mesh.CountKnots == mesh.CountKnots &&
                    this.mesh.CountBoundKnots == mesh.CountBoundKnots &&
                    this.mesh.CountBoundElements == mesh.CountBoundElements)
                    flagStart = false;
                else
                    flagStart = true;
            }
            base.SetTask(mesh, algebra, wMesh);

            Count = cs * cu;
            CountU = cs * mesh.CountKnots;
            MEM.Alloc(mesh.CountBoundKnots, ref bcIndex);
            MEM.Alloc(mesh.CountKnots, ref Vy);
            MEM.Alloc(mesh.CountKnots, ref Vz);
            MEM.Alloc(mesh.CountKnots, ref Phi);
            MEM.Alloc(mesh.CountKnots, ref Vortex);
            MEM.Alloc(cs * mesh.CountKnots, ref result_old);
            MEM.Alloc(cs * mesh.CountKnots, ref result_old);
            MEM.Alloc(cs * mesh.CountKnots, ref MRight);
            MEM.Alloc(cs * mesh.CountKnots, ref result_cur);

            if (algebra == null)
                algebra = new AlgebraGauss((uint)CountU);

            for (int i = 0; i < bcIndex.Length; i++)
                bcIndex[i] = (uint)(Index[i] * cs);
            MEM.Alloc(algebra.N, ref ColAdress);
            for (uint i = 0; i < ColAdress.Length; i++)
                ColAdress[i] = i;

            Ralgebra = algebra.Clone();
            
        }
        /// <summary>
        /// решаем задачу Стокса (Stokes problem)
        /// </summary>
        public void SolveStokesTask(ref double[] _Phi, ref double[] _Vortex, double[] eddyViscosity, double[] Ux)
        {
            uint ee = 0;
            try
            {
                // расчет функции тока и функции вихря
                Console.WriteLine("Calk Phi & Vortex in Stokes problem");
                MEM.Alloc((int)algebra.N, ref result);
                algebra.Clear();
                int li, lj;
                double eMu = mu;
                double D_agb = 0;
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
                    double Se3 = Se / 3;
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

                                // матрица масс
                                double M_ab = MM[ai][aj] * Se;
                                // матрица жосткости - вязкие члены плоские
                                double K_ab = (b[ai] * b[aj] + c[ai] * c[aj]) * Se;
                                if (eddyViscosity != null)
                                {
                                    // конверктивные члены - вязкости
                                    D_agb = ((c[0] * b[aj] - b[0] * c[aj]) * eddyViscosity[i0] +
                                             (c[1] * b[aj] - b[1] * c[aj]) * eddyViscosity[i1] +
                                             (c[2] * b[aj] - b[2] * c[aj]) * eddyViscosity[i2]) * Se3;
                                    eMu = (eddyViscosity[i0] + eddyViscosity[i0] + eddyViscosity[i0]) / 3;
                                }
                                LaplMatrix[li][lj]          = K_ab;
                                LaplMatrix[li][lj + 1]      = - M_ab;
                                LaplMatrix[li + 1][lj]      = 0;
                                LaplMatrix[li + 1][lj + 1]  = eMu * K_ab - D_agb;
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
                        for (int ai = 0; ai < cu; ai++)
                        {
                            li = cs * ai;
                            for (int aj = 0; aj < cu; aj++)
                            {
                                lj = cs * aj;
                                // матрица масс
                                double M_ab = MM[ai][aj] * Se;
                                // матрица жосткости - вязкие члены плоские
                                double K_ab = (b[ai] * b[aj] + c[ai] * c[aj]) * Se;
                                if (eddyViscosity != null)
                                {
                                    // конверктивные члены - вязкости
                                    D_agb = ((c[0] * b[aj] - b[0] * c[aj]) * eddyViscosity[i0] +
                                               (c[1] * b[aj] - b[1] * c[aj]) * eddyViscosity[i1] +
                                               (c[2] * b[aj] - b[2] * c[aj]) * eddyViscosity[i2]) * Se3;
                                    eMu = (eddyViscosity[i0] + eddyViscosity[i0] + eddyViscosity[i0]) / 3;
                                }
                                double RD_ab = eMu * b[ai] * Se3 / Radius;

                                LaplMatrix[li][lj]          = K_ab + Se3 * b[aj] / Radius;
                                LaplMatrix[li][lj + 1]      = - M_ab * Radius;
                                LaplMatrix[li + 1][lj]      = 0;
                                LaplMatrix[li + 1][lj + 1]  = eMu * K_ab - D_agb + RD_ab;
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
                Console.WriteLine("Calk Vy & Vz:");
                CalcVelosityPlane(Phi, ref Vy, ref Vz);
                MEM.MemCopy(ref result_cur, result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ee = " + ee.ToString() + " " + ex.Message);
            }
            MEM.Copy(ref _Phi, Phi);
            MEM.Copy(ref _Vortex, Vortex);
        }
        /// <summary>
        /// Решение стационарной задачи Рейнольдса
        /// </summary>
        /// <param name="result"></param>
        public void SolveReynoldsTask(ref double[] _Phi, ref double[] _Vortex, double[] eddyViscosity, double[] Ux)
        {
            uint ee = 0;
            uint testflag = 0;
            try
            {
                // итераций по нелинейности на текущем шаге по времени
                for (int nIdx = 0; nIdx < NLine; nIdx++)
                {
                    if (flagStart == true)
                    {
                        // На первом шаге решаем задачу Стокса
                        SolveStokesTask(ref _Phi, ref _Vortex, eddyViscosity, Ux);
                        flagStart = false;
                    }
                    else
                    {
                        testflag = 0;
                        // расчет функции тока и функции вихря

                        Console.WriteLine("Calk Phi & Vortex in st Reynolds problem: шаг по нелинейности: " + nIdx.ToString());
                        MEM.Alloc((int)algebra.N, ref result);
                        algebra.Clear();
                        int li, lj;
                        int Count = cu * cs;
                        for (uint elem = 0; elem < mesh.CountElements; elem++)
                        {
                            ee = elem;
                            // локальная матрица часть СЛАУ
                            MEM.Alloc2DClear(Count, ref LaplMatrix);
                            double[] b = dNdx[elem];
                            double[] c = dNdy[elem];
                            uint i0 = eKnots[elem].Vertex1;
                            uint i1 = eKnots[elem].Vertex2;
                            uint i2 = eKnots[elem].Vertex3;
                            // получить узлы КЭ
                            uint[] knots = { i0, i1, i2 };
                            double Se = S[elem];
                            double eMu = (eddyViscosity[i0] + eddyViscosity[i1] + eddyViscosity[i2]) / 3.0;
                            double Ax = 0;
                            double Ay = 0;
                            if (SigmaTask == 0) // Плоский створ
                            {
                                // Скорости через phi 
                                double eVx =  c[0] * Phi[i0] + c[1] * Phi[i1] + c[2] * Phi[i2];
                                double eVy = -(b[0] * Phi[i0] + b[1] * Phi[i1] + b[2] * Phi[i2]);
                                double mV = Math.Sqrt(eVx * eVx + eVy * eVy);
                                double Hk = Math.Sqrt(Se) / 1.4;
                                double Pe = rho_w * mV * Hk / (2 * mu);
                                if (MEM.Equals(mV, 0) != true)
                                {
                                    Ax = eVx / mV;
                                    Ay = eVy / mV;
                                }
                                // вычисление ЛЖМ для задачи Рейнольдса
                                for (int ai = 0; ai < cu; ai++)
                                {
                                    li = cs * ai;
                                    double L_SUPG = omega * (Hx[elem] * Ax * b[ai] + Hy[elem] * Ay * c[ai]);
                                    double La = (1 / 3.0 + L_SUPG);

                                    for (int aj = 0; aj < cu; aj++)
                                    {
                                        lj = cs * aj;
                                        // матрица масс
                                        double M_ab = MM[ai][aj] * Se;
                                        // матрица жосткости - вязкие члены плоские
                                        double K_ab = (b[ai] * b[aj] + c[ai] * c[aj]) * Se;

                                        // конверктивные члены - скорости
                                        double C_agb = La * rho_w * ((c[0] * b[aj] - b[0] * c[aj]) * Phi[i0] +
                                                                     (c[1] * b[aj] - b[1] * c[aj]) * Phi[i1] +
                                                                     (c[2] * b[aj] - b[2] * c[aj]) * Phi[i2]) * Se;

                                        // конверктивные члены - вязкости
                                        double D_agb = La * ((b[0] * b[aj] + c[0] * c[aj]) * eddyViscosity[i0] +
                                                             (b[1] * b[aj] + c[1] * c[aj]) * eddyViscosity[i1] +
                                                             (b[2] * b[aj] + c[2] * c[aj]) * eddyViscosity[i2]) * Se;

                                        LaplMatrix[li][lj] = K_ab;
                                        LaplMatrix[li][lj + 1] = -M_ab;
                                        LaplMatrix[li + 1][lj] = 0;
                                        LaplMatrix[li + 1][lj + 1] = C_agb - D_agb + eMu * K_ab;
                                    }
                                }
                            }
                            else // цилиндрический створ
                            {
                                // Скорости через phi 
                                double Radius = RadiusMin + (X[i0] + X[i1] + X[i2]) / 3;
                                double eUx = (Ux[i0] + Ux[i1] + Ux[i2]) / 3;
                                double Ux2 = eUx * eUx;
                                double Se3 = Se / 3;
                                double eVy =  (c[0] * Phi[i0] + c[1] * Phi[i1] + c[2] * Phi[i2]) / Radius;
                                double eVz = -(b[0] * Phi[i0] + b[1] * Phi[i1] + b[2] * Phi[i2]) / Radius;
                                double mV = Math.Sqrt(eVy * eVy + eVz * eVz);
                                double Hk = Math.Sqrt(Se) / 1.4;
                                double Pe = rho_w * mV * Hk / (2 * mu);
                                if (MEM.Equals(mV, 0) != true)
                                {
                                    Ax = eVy / mV;
                                    Ay = eVz / mV;
                                }
                                // вычисление ЛЖМ для задачи Рейнольдса
                                for (int ai = 0; ai < cu; ai++)
                                {
                                    double L_SUPG = omega * (Hx[elem] * Ax * b[ai] + Hy[elem] * Ay * c[ai]);
                                    li = cs * ai;
                                    double Lb = (1 / 3.0 + L_SUPG);
                                    for (int aj = 0; aj < cu; aj++)
                                    {
                                        lj = cs * aj;
                                        // матрица масс
                                        double M_ab = MM[ai][aj] * Se;
                                        // матрица масс SUPG L_alha
                                        double _M_ab = M_ab + L_SUPG * Se3;
                                        // матрица жосткости - вязкие члены плоские
                                        double K_ab = (b[ai] * b[aj] + c[ai] * c[aj]) * Se;
                                        // цилиндрическая добавка к матрице K_ab
                                        double RD_ab = eMu * b[ai] * Se3 / Radius  - rho_w  * eVy * M_ab / Radius;
                                        // конверктивные члены - скорости
                                        double C_agb = rho_w * Lb * ((c[0] * b[aj] - b[0] * c[aj]) * Phi[i0] +
                                                                     (c[1] * b[aj] - b[1] * c[aj]) * Phi[i1] +
                                                                     (c[2] * b[aj] - b[2] * c[aj]) * Phi[i2]) * Se / Radius;
                                        // конверктивные члены - вязкости
                                        double D_agb = Lb * ((b[0] * b[aj] + c[0] * c[aj]) * eddyViscosity[i0] +
                                                             (b[1] * b[aj] + c[1] * c[aj]) * eddyViscosity[i1] +
                                                             (b[2] * b[aj] + c[2] * c[aj]) * eddyViscosity[i2]) * Se;

                                        LaplMatrix[li][lj] = K_ab + Se3 * b[aj] / Radius;
                                        LaplMatrix[li][lj + 1] = - M_ab * Radius;
                                        LaplMatrix[li + 1][lj] = 0;
                                        LaplMatrix[li + 1][lj + 1] = C_agb - D_agb + eMu * K_ab + RD_ab;
                                    }
                                    LocalRight[li] = 0;
                                    LocalRight[li + 1] = rho_w * c[ai] * Se3 * Ux2 / Radius;
                                }
                                algebra.AddToRight(LocalRight, adressBound);
                            }
                            // получем значения адресов неизвестных
                            GetAdress(knots, ref adressBound);
                            // добавление вновь сформированной ЛЖМ в ГМЖ
                            algebra.AddToMatrix(LaplMatrix, adressBound);
                        }
                        testflag = 1;
                        // Выполнение граничных условий для функции вихря
                        VortexBC();
                        testflag = 2;
                        // Выполнение граничных условий для функции тока
                        algebra.BoundConditions(0, bcIndex);
                        testflag = 3;
                        //algebra.Print();
                        // решение
                        algebra.Solve(ref result);
                        testflag = 4;

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
            }
            catch (Exception ex)
            {
                Console.WriteLine("ee = " + ee.ToString() + " " + ex.Message + " этап: " + testflag.ToString());
            }
        }

        /// <summary>
        /// Решение одного шага по времени для задачи Рейнольдса
        /// </summary>
        /// <param name="result"></param>
        public void SolveReynoldsTask(ref double[] _Phi, ref double[] _Vortex,  double[] eddyViscosity, double[] Ux, double dt)
        {
            uint ee = 0;
            uint testflag = 0;
            uint NLine10 = 10;
            try
            {
                MEM.MemCopy(ref result_cur, result_old);
                // итераций по нелинейности на текущем шаге по времени
                for (int nIdx = 0; nIdx < NLine10; nIdx++)
                {
                    if (flagStart == true)
                    {
                        // На первом шаге решаем задачу Стокса
                        SolveStokesTask(ref _Phi, ref _Vortex, eddyViscosity, Ux);
                        MEM.MemCopy(ref result_old, result);
                        flagStart = false;
                    }
                    else
                    {
                        testflag = 0;
                        // расчет функции тока и функции вихря
                        Console.WriteLine("Calk Phi & Vortex in nst Reynolds problem: шаг по нелинейности: " + nIdx.ToString());
                        MEM.Alloc((int)algebra.N, ref result);
                        algebra.Clear();
                        Ralgebra.Clear();
                        int li, lj;
                        int Count = cu * cs;
                        //
                        bool old = false;
                        //
                        if (old == true) // Старый вариант
                        {
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
                                double eMu = (eddyViscosity[i0] + eddyViscosity[i1] + eddyViscosity[i2]) / 3.0;
                                double Ax = 0;
                                double Ay = 0;
                                if (SigmaTask == 0) // Плоский створ
                                {
                                    // Скорости через phi 
                                    double eVx = c[0] * Phi[i0] + c[1] * Phi[i1] + c[2] * Phi[i2];
                                    double eVy = -(b[0] * Phi[i0] + b[1] * Phi[i1] + b[2] * Phi[i2]);
                                    double mV = Math.Sqrt(eVx * eVx + eVy * eVy);
                                    double Hk = Math.Sqrt(Se) / 1.4;
                                    double Pe = rho_w * mV * Hk / (2 * mu);
                                    if (MEM.Equals(mV, 0) != true)
                                    {
                                        Ax = eVx / mV;
                                        Ay = eVy / mV;
                                    }
                                    // вычисление ЛЖМ для задачи Рейнольдса
                                    for (int ai = 0; ai < cu; ai++)
                                    {
                                        li = cs * ai;
                                        double L_SUPG = omega * (Hx[elem] * Ax * b[ai] + Hy[elem] * Ay * c[ai]);
                                        double La = (1 / 3.0 + L_SUPG);
                                        for (int aj = 0; aj < cu; aj++)
                                        {
                                            lj = cs * aj;
                                            // матрица масс
                                            double M_ab = MM[ai][aj] * Se;
                                            // матрица жосткости - вязкие члены плоские
                                            double K_ab = (b[ai] * b[aj] + c[ai] * c[aj]) * Se;

                                            // конверктивные члены - скорости
                                            double C_agb = La * rho_w * ((c[0] * b[aj] - b[0] * c[aj]) * Phi[i0] +
                                                                         (c[1] * b[aj] - b[1] * c[aj]) * Phi[i1] +
                                                                         (c[2] * b[aj] - b[2] * c[aj]) * Phi[i2]) * Se;
                                            // конверктивные члены - вязкости
                                            double D_agb = La * ((b[0] * b[aj] + c[0] * c[aj]) * eddyViscosity[i0] +
                                                                 (b[1] * b[aj] + c[1] * c[aj]) * eddyViscosity[i1] +
                                                                 (b[2] * b[aj] + c[2] * c[aj]) * eddyViscosity[i2]) * Se;


                                            LaplMatrix[li][lj] = K_ab;
                                            LaplMatrix[li][lj + 1] = -M_ab;
                                            LaplMatrix[li + 1][lj] = 0;
                                            LaplMatrix[li + 1][lj + 1] = rho_w * M_ab + dt * (1 - theta) * (C_agb - D_agb + eMu * K_ab);

                                            RMatrix[li][lj] = 0;
                                            RMatrix[li][lj + 1] = 0;
                                            RMatrix[li + 1][lj] = 0;
                                            RMatrix[li + 1][lj + 1] = rho_w * M_ab - dt * theta * (C_agb - D_agb + eMu * K_ab);

                                        }
                                    }
                                }
                                else // цилиндрический створ
                                {
                                    // Скорости через phi 
                                    double Radius = RadiusMin + (X[i0] + X[i1] + X[i2]) / 3;
                                    double eUx = (Ux[i0] + Ux[i1] + Ux[i2]) / 3;
                                    double Ux2 = eUx * eUx;
                                    double Se3 = Se / 3;
                                    double eVy = (c[0] * Phi[i0] + c[1] * Phi[i1] + c[2] * Phi[i2]) / Radius;
                                    double eVz = -(b[0] * Phi[i0] + b[1] * Phi[i1] + b[2] * Phi[i2]) / Radius;
                                    double mV = Math.Sqrt(eVy * eVy + eVz * eVz);
                                    double Hk = Math.Sqrt(Se) / 1.4;
                                    double Pe = rho_w * mV * Hk / (2 * mu);
                                    if (MEM.Equals(mV, 0) != true)
                                    {
                                        Ax = eVy / mV;
                                        Ay = eVz / mV;
                                    }
                                    // вычисление ЛЖМ для задачи Рейнольдса
                                    for (int ai = 0; ai < cu; ai++)
                                    {
                                        li = cs * ai;
                                        double L_SUPG = omega * (Hx[elem] * Ax * b[ai] + Hy[elem] * Ay * c[ai]);
                                        double La = (1 / 3.0 + L_SUPG);
                                        for (int aj = 0; aj < cu; aj++)
                                        {
                                            lj = cs * aj;
                                            // матрица масс
                                            double M_ab = MM[ai][aj] * Se;
                                            // матрица жосткости - вязкие члены плоские
                                            double K_ab = (b[ai] * b[aj] + c[ai] * c[aj]) * Se;
                                            // цилиндрическая добавка к матрице K_ab
                                            double RD_ab = eMu * b[ai] * Se3 / Radius - eVy * M_ab / (Radius * Radius);
                                            // конверктивные члены - скорости
                                            double C_agb = La * rho_w * ((c[0] * b[aj] - b[0] * c[aj]) * Phi[i0] +
                                                                         (c[1] * b[aj] - b[1] * c[aj]) * Phi[i1] +
                                                                         (c[2] * b[aj] - b[2] * c[aj]) * Phi[i2]) * Se / Radius;
                                            // конверктивные члены - вязкости
                                            double D_agb = La * ((b[0] * b[aj] + c[0] * c[aj]) * eddyViscosity[i0] +
                                                                 (b[1] * b[aj] + c[1] * c[aj]) * eddyViscosity[i1] +
                                                                 (b[2] * b[aj] + c[2] * c[aj]) * eddyViscosity[i2]) * Se;

                                            LaplMatrix[li][lj] = K_ab + Se3 * b[aj] / Radius;
                                            LaplMatrix[li][lj + 1] = -M_ab * Radius;
                                            LaplMatrix[li + 1][lj] = 0;
                                            LaplMatrix[li + 1][lj + 1] = rho_w * M_ab + dt * (1 - theta) * (C_agb - D_agb + eMu * K_ab + RD_ab);

                                            RMatrix[li][lj] = 0;
                                            RMatrix[li][lj + 1] = 0;
                                            RMatrix[li + 1][lj] = 0;
                                            RMatrix[li + 1][lj + 1] = rho_w * M_ab - dt * theta * (C_agb - D_agb + eMu * K_ab + RD_ab);
                                        }
                                        LocalRight[li] = 0;
                                        LocalRight[li + 1] = rho_w * c[ai] * Se3 * Ux2 / Radius;
                                    }
                                    algebra.AddToRight(LocalRight, adressBound);
                                }
                                // получем значения адресов неизвестных
                                GetAdress(knots, ref adressBound);
                                // добавление вновь сформированной ЛЖМ в ГМЖ
                                algebra.AddToMatrix(LaplMatrix, adressBound);
                                // добавление вновь сформированной ЛЖМ в ГМЖ
                                Ralgebra.AddToMatrix(RMatrix, adressBound);
                            }
                        }
                        else // Новый уточненный вариант
                        {
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
                                double Se3 = Se / 3;
                                double eMu = (eddyViscosity[i0] + eddyViscosity[i1] + eddyViscosity[i2]) / 3.0;
                                double Ax = 0;
                                double Ay = 0;
                                double Radius = 1;
                                if (SigmaTask == 1) // Плоский створ
                                    Radius = RadiusMin + (X[i0] + X[i1] + X[i2]) / 3;

                                double eUx = (Ux[i0] + Ux[i1] + Ux[i2]) / 3;
                                double Ux2 = eUx * eUx;
                                double eVy = (c[0] * Phi[i0] + c[1] * Phi[i1] + c[2] * Phi[i2]) / Radius;
                                double eVz = -(b[0] * Phi[i0] + b[1] * Phi[i1] + b[2] * Phi[i2]) / Radius;
                                double mV = Math.Sqrt(eVy * eVy + eVz * eVz);
                                double Hk = Math.Sqrt(Se) / 1.4;
                                double Pe = rho_w * mV * Hk / (2 * mu);
                                if (MEM.Equals(mV, 0) != true)
                                {
                                    Ax = eVy / mV;
                                    Ay = eVz / mV;
                                }
                                // вычисление ЛЖМ для задачи Рейнольдса
                                for (int ai = 0; ai < cu; ai++)
                                {
                                    li = cs * ai;
                                    double L_SUPG = omega * (Hx[elem] * Ax * b[ai] + Hy[elem] * Ay * c[ai]);
                                    double La = (1 / 3.0 + L_SUPG);
                                    // правая часть SUPG : \int (L_alpha) d Omega
                                    double _Fa = Se * La;
                                    for (int aj = 0; aj < cu; aj++)
                                    {
                                        lj = cs * aj;
                                        // матрица масс
                                        double M_ab = MM[ai][aj] * Se;
                                        // матрица масс SUPG : \int (L_alpha * N_beta) d Omega
                                        double _M_ab = M_ab + L_SUPG * Se3;
                                        // матрица жосткости - вязкие члены плоские
                                        double K_ab = (b[ai] * b[aj] + c[ai] * c[aj]) * Se;
                                        // цилиндрическая добавка к матрице K_ab
                                        double RD_ab = (eMu * b[ai] * Se3 / Radius - rho_w * eVy * _M_ab / Radius) * SigmaTask;
                                        // конверктивные члены - скорости
                                        double C_agb = _Fa * rho_w * ((c[0] * b[aj] - b[0] * c[aj]) * Phi[i0] +
                                                                      (c[1] * b[aj] - b[1] * c[aj]) * Phi[i1] +
                                                                      (c[2] * b[aj] - b[2] * c[aj]) * Phi[i2]) / Radius;
                                        // конверктивные члены - вязкости
                                        double D_agb = _Fa * ((b[0] * b[aj] + c[0] * c[aj]) * eddyViscosity[i0] +
                                                              (b[1] * b[aj] + c[1] * c[aj]) * eddyViscosity[i1] +
                                                              (b[2] * b[aj] + c[2] * c[aj]) * eddyViscosity[i2]);

                                        LaplMatrix[li][lj] = K_ab + Se3 * b[aj] / Radius;
                                        LaplMatrix[li][lj + 1] = -M_ab * Radius;
                                        LaplMatrix[li + 1][lj] = 0;
                                        LaplMatrix[li + 1][lj + 1] = rho_w * _M_ab + dt * (1 - theta) * (C_agb - D_agb + eMu * K_ab + RD_ab);

                                        RMatrix[li][lj] = 0;
                                        RMatrix[li][lj + 1] = 0;
                                        RMatrix[li + 1][lj] = 0;
                                        RMatrix[li + 1][lj + 1] = rho_w * _M_ab - dt * theta * (C_agb - D_agb + eMu * K_ab + RD_ab);
                                    }
                                    LocalRight[li] = 0;
                                    LocalRight[li + 1] = (rho_w * c[ai] * Se3 * Ux2 / Radius) * SigmaTask;
                                }
                                algebra.AddToRight(LocalRight, adressBound);

                                // получем значения адресов неизвестных
                                GetAdress(knots, ref adressBound);
                                // добавление вновь сформированной ЛЖМ в ГМЖ
                                algebra.AddToMatrix(LaplMatrix, adressBound);
                                // добавление вновь сформированной ЛЖМ в ГМЖ
                                Ralgebra.AddToMatrix(RMatrix, adressBound);
                            }
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
                        //for (int i = 0; i < result.Length; i++)
                        //    result_cur[i] = relax * result[i] + (1 - relax) * result_cur[i];
                        for (int i = 0; i < mesh.CountKnots; i++)
                        {
                            Phi[i] = result[2 * i];
                            _Phi[i] = Phi[i];
                            Vortex[i] = result[2 * i + 1];
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
                            normVortex += result[i] * result[i];
                            epsVortex += (result_cur[i] - result[i]) * (result_cur[i] - result[i]);
                        }
                        double residual = Math.Sqrt(epsVortex / normVortex);
                        Console.WriteLine("n {0} residual {1}", nIdx, residual);
                        if (residual < MEM.Error4)
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
                Console.WriteLine("ee = " + ee.ToString() + " " + ex.Message +" этап: " + testflag.ToString());
            }
        }
        /// <summary>
        /// Выполнение граничных условий для функции вихря
        /// </summary>
        public void VortexBC()
        {
            double Right;
            uint idx;
            double[] LBElem = wMesh.GetLb();
            double Radius;
            for (uint belem = 0; belem < mesh.CountBoundElements; belem++)
            {
                int mark = mesh.GetBoundElementMarker(belem);
                if (mark == 2 && VelosityUy != null)
                {
                    uint i1 = beKnots[belem].Vertex1;
                    uint i2 = beKnots[belem].Vertex2;
                    double y = 0.5 * (X[i1] + X[i2]);
                    if (SigmaTask == 0)
                        Radius = 1;
                    else
                        Radius = RadiusMin + y;
                    Right = LBElem[belem] * VelosityUy.FunctionValue(y) * Radius;
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
            MEM.VAlloc((int)algebra.N, 0, ref ColElems);
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
                    double dPhidy = Phi[i0] * b[0] + Phi[i1] * b[1] + Phi[i2] * b[2];
                    double dPhidz = Phi[i0] * c[0] + Phi[i1] * c[1] + Phi[i2] * c[2];
                    if (SigmaTask == 0)
                    {
                        Vx[elem] =  dPhidz;
                        Vy[elem] = -dPhidy;
                    }
                    else
                    {
                        double Radius = RadiusMin + (X[i0] + X[i1] + X[i2]) / 3;
                        Vx[elem] =   dPhidz / Radius;
                        Vy[elem] = - dPhidy / Radius;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
        }
    }
}



