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
    using AlgebraLib;

    using CommonLib;
    using CommonLib.Mesh;
    using CommonLib.Physics;
    using CommonLib.Function;
    using FEMTasksLib.FEMTasks.Utils;

    /// <summary>
    ///  ОО: Решатель для задачи Рейнольдс на трехузловой сетке
    ///  для решения задач движения потока в канале, 
    ///  на входе канала задаются значения функции тока,
    ///  на выходе из канала задаются значения функции тока/...
    /// </summary>
    [Serializable]
    public class ReynoldsVortexStream1XDTri : AWRAP_FETaskTri
    {
        /// <summary>
        /// Маркер дырок
        /// </summary>
        protected int MarkHoles = 4;
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
        /// скорости на крышках
        /// </summary>
        public IDigFunction[] VelosityUy = null;
        /// <summary>
        /// функция тока на входе и выходе
        /// </summary>
        public IDigFunction[] bcPhi = null;
        /// <summary>
        /// адреса функии тока на границе
        /// </summary>
        protected double[] bcValuePhi = null;
        /// <summary>
        /// ГУ на выходе 0 - дирехле однород. 1 - нейман однород.
        /// </summary>
        protected int bcOut = 0;
        /// <summary>
        /// адреса функии тока на границе
        /// </summary>
        protected double[] tmpBcValuePhi = null;
        /// <summary>
        /// Параметр схемы по времени
        /// </summary>
        protected double theta = 0.5;
        /// <summary>
        /// Итераций по нелинейности на текущем шаге по времени
        /// </summary>
        protected int NLine;
        #region массивы работают при скольжении по WL
        /// <summary>
        /// ГУ на WL 0 - скольжение 1 - прилипание
        /// </summary>
        protected int bcTypeOnWL;
        /// <summary>
        /// адреса функии тока на границе
        /// </summary>
        protected uint[] bcIndexVortex = null;
        ///// <summary>
        ///// адреса функии тока на границе
        ///// </summary>
        //protected double[] bcValueVortex = null;
        #endregion
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
        /// <summary>
        /// Конствруктор
        /// </summary>
        /// <param name="NLine">иераций по нелинейности</param>
        /// <param name="bcTypeOnWL">ГУ на WL 0 - скольжение 1 - прилипание</param>
        /// <param name="bcOut">ГУ на выходе 0 - дирехле однород. 1 - нейман однород.</param>
        /// <param name="VelosityUy">скорости на входк и выходе</param>
        /// <param name="bcPhi">функция тока на на входк и выходе</param>
        /// <param name="theta">параметр неявности для схемы по времени</param>
        public ReynoldsVortexStream1XDTri(int NLine, int bcTypeOnWL, int bcOut,
            IDigFunction[] VelosityUy, IDigFunction[] bcPhi, double theta = 0.5) 
        {
            cs = 2;
            this.theta = theta;
            this.NLine = NLine;
            this.NL_max = NLine;
            this.VelosityUy = VelosityUy;
            this.bcPhi = bcPhi;
            this.bcOut = bcOut;
            this.bcTypeOnWL = bcTypeOnWL;
            flagStart = true;
        }
        public override void SetTask(IMesh mesh, IAlgebra algebra, 
                                     IMeshWrapper wMesh = null)
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
            //MEM.Alloc(mesh.CountBoundKnots, ref bcIndex);
            MEM.Alloc(mesh.CountKnots, ref tmpBcValuePhi);
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
            
            int[] bkm = mesh.GetBoundKnotsMark();

            int CountBK = 0;
            // исключаем дырки и узлы на выходе из области
            for (int i = 0; i < Marker.Length; i++)
                if (bcOut == 1 && Marker[i] != 1 || bcOut == 0)
                    if (Marker[i] < MarkHoles)
                        CountBK++;

            MEM.Alloc(CountBK, ref bcIndex);
            MEM.Alloc(CountBK, ref bcValuePhi);
            
            int k = 0;
            for (int i = 0; i < Marker.Length; i++)
                if (bcOut == 1 && Marker[i] != 1 || bcOut == 0)
                    if (Marker[i] < MarkHoles)
                        bcIndex[k++] = (uint)(Index[i] * cs);

            MEM.Alloc(algebra.N, ref ColAdress);
            for (uint i = 0; i < ColAdress.Length; i++)
                ColAdress[i] = i;
            
            Ralgebra = algebra.Clone();
            if (bcTypeOnWL == 0)
            {
                uint i1;
                uint i2;
                int CountElVortexWL = 0;
                for (uint belem = 0; belem < mesh.CountBoundElements; belem++)
                {
                    int mark = mesh.GetBoundElementMarker(belem);
                    if (mark == 2)
                        CountElVortexWL++;
                }
                CountElVortexWL++;
                MEM.Alloc(CountElVortexWL, ref bcIndexVortex);
                CountElVortexWL = -1;
                k = 0;
                for (uint belem = 0; belem < mesh.CountBoundElements; belem++)
                {
                    int mark = mesh.GetBoundElementMarker(belem);
                    if(mark == 2)
                    {
                        if (CountElVortexWL == -1)
                        {
                            i1 = beKnots[belem].Vertex1;
                            bcIndexVortex[k++] = (uint)(i1 * cs + 1);
                            CountElVortexWL = 0;
                        }
                        i2 = beKnots[belem].Vertex2;
                        bcIndexVortex[k++] = (uint)(i2 * cs + 1);
                    }
                }
            }
        }
        /// <summary>
        /// решаем задачу Стокса (Stokes problem)
        /// </summary>
        public void SolveStokesTask(ref double[] _Phi, ref double[] _Vortex, double[] eddyViscosity)
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
                            LaplMatrix[li][lj] = K_ab;
                            LaplMatrix[li][lj + 1] = - M_ab;
                            LaplMatrix[li + 1][lj] = 0;
                            LaplMatrix[li + 1][lj + 1] = eMu * K_ab - D_agb;
                        }
                    }
                    // получем значения адресов неизвестных
                    FEMUtils.GetAdress(knots, ref adressBound);
                    // добавление вновь сформированной ЛЖМ в ГМЖ
                    algebra.AddToMatrix(LaplMatrix, adressBound);
                }
                // Выполнение граничных условий для функции вихря
                VortexBC();
                // Выполнение граничных условий для функции тока
                algebra.BoundConditions(bcValuePhi, bcIndex);
                // Выполнение однородных граничных условий для функции вихря
                // на свободной поверхности
                if (bcTypeOnWL == 0)
                    algebra.BoundConditions(0, bcIndexVortex);
                // algebra.Print(1);
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
        public void SolveReynoldsTask(ref double[] _Phi, ref double[] _Vortex, double[] eddyViscosity)
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
                        SolveStokesTask(ref _Phi, ref _Vortex, eddyViscosity);
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
                                    LaplMatrix[li + 1][lj + 1] = C_agb - D_agb + eMu * K_ab;
                                }
                            }
                            // получем значения адресов неизвестных
                            FEMUtils.GetAdress(knots, ref adressBound);
                            // добавление вновь сформированной ЛЖМ в ГМЖ
                            algebra.AddToMatrix(LaplMatrix, adressBound);
                        }
                        testflag = 1;
                        // Выполнение граничных условий для функции вихря
                        VortexBC();
                        testflag = 2;
                        // Выполнение граничных условий для функции тока
                        algebra.BoundConditions(bcValuePhi, bcIndex);
                        // Выполнение однородных граничных условий для функции вихря
                        // на свободной поверхности
                        if (bcTypeOnWL == 0)
                            algebra.BoundConditions(0, bcIndexVortex);

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
        public void SolveReynoldsTask(ref double[] _Phi, ref double[] _Vortex,  double[] eddyViscosity, double dt)
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
                        SolveStokesTask(ref _Phi, ref _Vortex, eddyViscosity);
                        MEM.MemCopy(ref result_old, result);
                        flagStart = false;
                        continue;
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
                                    LaplMatrix[li + 1][lj + 1] = rho_w * _M_ab + dt * (1 - theta) * (C_agb - D_agb + eMu * K_ab);

                                    RMatrix[li][lj] = 0;
                                    RMatrix[li][lj + 1] = 0;
                                    RMatrix[li + 1][lj] = 0;
                                    RMatrix[li + 1][lj + 1] = rho_w * _M_ab - dt * theta * (C_agb - D_agb + eMu * K_ab);
                                }
                            }
                            // получем значения адресов неизвестных
                            FEMUtils.GetAdress(knots, ref adressBound);
                            // добавление вновь сформированной ЛЖМ в ГМЖ
                            algebra.AddToMatrix(LaplMatrix, adressBound);
                            // добавление вновь сформированной ЛЖМ в ГМЖ
                            Ralgebra.AddToMatrix(RMatrix, adressBound);
                        }
                    }
                    testflag = 1;
                    // Расчет
                    Ralgebra.GetResidual(ref MRight, result_old, 0);
                    testflag = 2;
                    algebra.CopyRight(MRight);
                    testflag = 3;
                    // Выполнение граничных условий для функции вихря
                    VortexBC();
                    testflag = 4;
                    // Выполнение граничных условий для функции тока
                    algebra.BoundConditions(bcValuePhi, bcIndex);
                    // Выполнение однородных граничных условий для функции вихря
                    // на свободной поверхности
                    if (bcTypeOnWL == 0)
                        algebra.BoundConditions(0, bcIndexVortex);

                    testflag = 5;
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
                // расчет поля скорости
                Console.WriteLine("Vy ~ Vz:");
                CalcVelosityPlane(Phi, ref Vy, ref Vz);
                MEM.MemCopy(ref result_old, result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ee = " + ee.ToString() + " " + ex.Message + " этап: " + testflag.ToString());
            }
        }
        /// <summary>
        /// Выполнение граничных условий для функции вихря
        /// </summary>
        public void VortexBC()
        {
            uint idx;
            double Right, bPhi1, bPhi2, y;
            double[] LBElem = wMesh.GetLb();
            
            for (uint belem = 0; belem < mesh.CountBoundElements; belem++)
            {
                int mark = mesh.GetBoundElementMarker(belem);
                switch (mark)
                {
                    case 1: // выход
                        {
                            uint i1 = beKnots[belem].Vertex1;
                            uint i2 = beKnots[belem].Vertex2;
                            double x = 0.5 * (X[i1] + X[i2]);
                            //double y = 0.5 * (Y[i1] + Y[i2]);

                            Right = 0; // VelosityUy[1].FunctionValue(y);
                            y = Y[i1];
                            bPhi1 = bcPhi[1].FunctionValue(y);
                            y = Y[i2];
                            bPhi2 = bcPhi[1].FunctionValue(y);
                        }
                        break;
                    case 3: // вход
                        {
                            uint i1 = beKnots[belem].Vertex1;
                            uint i2 = beKnots[belem].Vertex2;
                            double x = 0.5 * (X[i1] + X[i2]);
                            //double y = 0.5 * (Y[i1] + Y[i2]);
                            Right = 0;// VelosityUy[0].FunctionValue(y);
                            y = Y[i1];
                            bPhi1 = bcPhi[0].FunctionValue(y);
                            y = Y[i2];
                            bPhi2 = bcPhi[0].FunctionValue(y);

                        }
                        break;
                    case 2: // границы крышки
                        {
                            Right = 0;
                            bPhi1 = bcPhi[0].Ymax;
                            bPhi2 = bPhi1;
                        }
                        break;
                    default: // границы дна
                        Right = 0;
                        bPhi1 = 0;
                        bPhi2 = 0;
                        break;
                }
                if (belem == 0)
                {
                    idx = beKnots[belem].Vertex1;
                    VortexBC((uint)(cs * idx), ColAdress, Right);
                    tmpBcValuePhi[idx] = bPhi1;
                }
                idx = beKnots[belem].Vertex2;
                VortexBC((uint)(cs * idx), ColAdress, Right);
                tmpBcValuePhi[idx] = bPhi2;
            }

            int ii = 0;
            for (int i = 0; i < Index.Length; i++)
                if (bcOut == 1 && Marker[i] != 1 || bcOut == 0)
                    if (Marker[i] < MarkHoles)
                        bcValuePhi[ii++] = tmpBcValuePhi[Index[i]];
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
                //VSUtils.VelocityZerroOnBed(mesh, ref Vx, ref Vy);
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
                    Vx[elem] =  dPhidz;
                    Vy[elem] = -dPhidy;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
        }
    }
}



