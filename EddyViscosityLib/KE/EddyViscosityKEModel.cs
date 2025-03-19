//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 14.01.2025 Потапов И.И.
//---------------------------------------------------------------------------
namespace EddyViscosityLib
{
    using CommonLib;
    using CommonLib.Mesh;
    using CommonLib.Physics;

    using System;
    using MemLogLib;
    using CommonLib.EddyViscosity;

    #region Классы группы моделей K-E
    /// <summary>
    /// Нестационарная векторная k - e модель  
    /// </summary>
    [Serializable]
    public class EddyViscosityKEModelTri : AEddyViscosityKETri
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
        /// Для правой части
        /// </summary>
        protected IAlgebra Ralgebra;
        /// <summary>
        /// локальная матрица часть СЛАУ
        /// </summary>
        private double[][] RMatrix = null;
        /// <summary>
        /// Предыдущее решение по нелинейности
        /// </summary>
        protected double[] result_old = null;
        /// <summary>
        /// Предыдущее решение по нелинейности
        /// </summary>
        protected double[] result = null;

        public EddyViscosityKEModelTri(ETurbViscType eTurbViscType, BEddyViscosityParam p, TypeTask tt)
            : base(eTurbViscType, tt, p.NLine) 
        {
            cs = 2;
        }
        public override void SetTask(IMesh mesh, IAlgebra algebra, IMeshWrapper wMesh = null)
        {
            base.SetTask(mesh, algebra, wMesh);
            MEM.Alloc(cs * mesh.CountKnots, ref MRight);
            MEM.Alloc(cs * mesh.CountKnots, ref result);
            MEM.Alloc(cs * mesh.CountKnots, ref result_old);
            Ralgebra = algebra.Clone();
            MEM.Alloc(cs * mesh.CountBoundKnots, ref bcIndex);
            for (int i = 0; i < Index.Length; i++)
            {
                int ai = i * cs;
                bcIndex[ai] = (uint)(Index[i] * cs);
                bcIndex[ai + 1] = (uint)(Index[i] * cs + 1);
            }
        }

        protected void CalkEddyViscosity()
        {
            for (int i = 0; i < mesh.CountKnots; i++)
            {
                Ken[i] = result_old[2 * i];
                Eps[i] = result_old[2 * i + 1];
                if (MEM.Equals(Eps[i], 0) == true)
                    eddyViscosity[i] = 0;
                else
                    eddyViscosity[i] = rho_w * C_mu * Ken[i] * Ken[i] / Eps[i];
            }
        }

        /// <summary>
        /// Решение задачи
        /// </summary>
        /// <param name="result">результат решения</param>
        public override void SolveTask(ref double[] eddyViscosity, double[] Ux, double[] Vy, double[] Vz, double[] Phi, double[] Vortex, double dt)
        {
            this.Ux = Ux;
            this.Phi = Phi;
            this.Vortex = Vortex;
            this.dt = dt;
            SolveTask(ref eddyViscosity);
        }
        /// <summary>
        /// Определение вязкости  К-Е модели на текущем шаге по времени
        /// </summary>
        /// <param name="eddyViscosity"></param>
        public override void SolveTask(ref double[] eddyViscosity)
        {
            uint ee = 0;
            try
            {
                // итераций по нелинейности на текущем шаге по времени
                for (int nIdx = 0; nIdx < NLine; nIdx++)
                {
                    MEM.MemCopy(ref result_old, result);
                    // расчет функции тока и функции вихря
                    Console.WriteLine("Ken & Eps: шаг по нелинейности: " + nIdx.ToString());
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
                        MEM.AllocClear(Count, ref LocalRight);
                        double[] b = dNdx[elem];
                        double[] c = dNdy[elem];
                        uint i0 = eKnots[elem].Vertex1;
                        uint i1 = eKnots[elem].Vertex2;
                        uint i2 = eKnots[elem].Vertex3;
                        // получить узлы КЭ
                        uint[] knots = { i0, i1, i2 };
                        double Se = S[elem];

                        double eMu = (eddyViscosity[i0] + eddyViscosity[i1] + eddyViscosity[i2]) / 3.0;
                        double eVortex = (Vortex[i0] + Vortex[i1] + Vortex[i2]) / 3.0;
                        double eMu_k = eMu / Sigma_k + SPhysics.mu;
                        double eMu_e = eMu / Sigma_e + SPhysics.mu;
                        double eMu_p = eMu + Sigma_e + SPhysics.mu;

                        double eKen = (Ken[i0] + Ken[i1] + Ken[i2]) / 3.0;
                        double eEps = (Eps[i0] + Eps[i1] + Eps[i2]) / 3.0;
                        double eK_E = eKen / (eEps + MEM.Error14);
                        // Вычисление ЛПЧ от объемных сил    
                        double pk = eVortex * eVortex;

                        double eVx = c[0] * Phi[i0] + c[1] * Phi[i1] + c[2] * Phi[i2];
                        double eVy = -(b[0] * Phi[i0] + b[1] * Phi[i1] + b[2] * Phi[i2]);



                        double mV = Math.Sqrt(eVx * eVx + eVy * eVy);
                        double Hk = Math.Sqrt(Se) / 1.4;
                        // сеточное число Пекле
                        double Pe = SPhysics.rho_w * mV * Hk / (2 * eMu);
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
                            double L_SUPG = omega * (Hx[elem] * Ax * b[ai] + Hy[elem] * Ay * c[ai]);
                            li = cs * ai;
                            double Lb = (1 / 3.0 + L_SUPG);
                            double La = rho_w * Lb;

                            for (int aj = 0; aj < cu; aj++)
                            {
                                lj = cs * aj;
                                // матрица масс
                                double M_ab = MM[ai][aj] * Se;
                                // матрица масс SUPG
                                double _M_ab = rho_w * (M_ab + L_SUPG * Se / 3.0);
                                // матрица вязкости члены плоские
                                double K_ab = (b[ai] * b[aj] + c[ai] * c[aj]) * Se;
                                // матрица конверкции 
                                double C_agb = La * ((c[0] * b[aj] - b[0] * c[aj]) * Phi[i0] +
                                                     (c[1] * b[aj] - b[1] * c[aj]) * Phi[i1] +
                                                     (c[2] * b[aj] - b[2] * c[aj]) * Phi[i2]) * Se;

                                LaplMatrix[li][lj] = _M_ab + dt * (1 - theta) * (C_agb + eMu_k * K_ab);
                                LaplMatrix[li][lj + 1] = dt * (1 - theta) * _M_ab;
                                LaplMatrix[li + 1][lj] = -dt * (1 - theta) * C_e1 * C_mu * pk;
                                LaplMatrix[li + 1][lj + 1] = _M_ab + dt * (1 - theta) * (C_agb + eMu_e * K_ab + C_e2 * eK_E * _M_ab);

                                RMatrix[li][lj] = _M_ab - dt * theta * (C_agb + eMu_k * K_ab);
                                RMatrix[li][lj + 1] = -dt * theta * _M_ab;
                                RMatrix[li + 1][lj] = dt * theta * C_e1 * C_mu * pk;
                                RMatrix[li + 1][lj + 1] = _M_ab - dt * theta * (C_agb + eMu_e * K_ab + C_e2 * eK_E * _M_ab);
                            }

                            LocalRight[li] = Lb * eMu_p * pk;
                            LocalRight[li + 1] = 0;
                        }
                        // получем значения адресов неизвестных
                        GetAdress(knots, ref adressBound);
                        // добавление вновь сформированной ЛЖМ в ГМЖ
                        algebra.AddToMatrix(LaplMatrix, adressBound);
                        // добавление вновь сформированной ЛЖМ в ГМЖ
                        Ralgebra.AddToMatrix(RMatrix, adressBound);

                        algebra.AddToRight(LocalRight, adressBound);
                    }
                    // Расчет
                    Ralgebra.getResidual(ref MRight, result_old, 0);
                    algebra.CopyRight(MRight);

                    // Выполнение граничных условий для функции вихря
                    // VortexBC();
                    // Выполнение граничных условий для функции тока
                    algebra.BoundConditions(0, bcIndex);
                    //algebra.Print();
                    // решение
                    algebra.Solve(ref result);

                    for (int i = 0; i < result.Length; i++)
                        result_old[i] = relax * result[i] + (1 - relax) * result_old[i];

                    for (int i = 0; i < mesh.CountKnots; i++)
                    {
                        Ken[i] = result_old[2 * i];
                        Eps[i] = result_old[2 * i + 1];
                        if (MEM.Equals(Eps[i], 0) == true)
                            eddyViscosity[i] = 0;
                        else
                            eddyViscosity[i] = rho_w * C_mu * Ken[i] * Ken[i] / Eps[i];
                    }
                    //************************************************************************************************************************
                    // Считаем невязку
                    //************************************************************************************************************************
                    double epsVortex = 0.0;
                    double normVortex = 0.0;

                    for (int i = 0; i < result.Length; i++)
                    {
                        normVortex += result_old[i] * result_old[i];
                        epsVortex += (result_old[i] - result[i]) * (result_old[i] - result[i]);
                    }
                    double residual = Math.Sqrt(epsVortex / normVortex);
                    Console.WriteLine("n {0} residual {1}", nIdx, residual);
                    if (residual < MEM.Error5)
                        break;
                }
                // расчет поля скорости
                Console.WriteLine("Vy ~ Vz:");
                //CalcVelosityPlane(Phi, ref Vy, ref Vz);
                MEM.MemCopy(ref result_old, result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ee = " + ee.ToString() + " " + ex.Message);
            }
        }
    }

    /// <summary>
    /// Стационарная векторная  k - e модель 
    /// </summary>
    [Serializable]
    public class EddyViscosityVKEModelTri : AEddyViscosityKETri
    {
        /// <summary>
        /// локальная матрица часть СЛАУ
        /// </summary>
        private double[][] RMatrix = null;
        /// <summary>
        /// Предыдущее решение по нелинейности
        /// </summary>
        protected double[] result_old = null;
        /// <summary>
        /// Предыдущее решение по нелинейности
        /// </summary>
        protected double[] result = null;
        public EddyViscosityVKEModelTri(ETurbViscType eTurbViscType, BEddyViscosityParam p, TypeTask tt)
            : base(eTurbViscType,tt, p.NLine)
        {
            cs = 2;
        }
        public override void SetTask(IMesh mesh, IAlgebra algebra, IMeshWrapper wMesh = null)
        {
            base.SetTask(mesh, algebra, wMesh);
            MEM.Alloc(cs * mesh.CountKnots, ref MRight);
            MEM.Alloc(cs * mesh.CountKnots, ref result);
            MEM.Alloc(cs * mesh.CountKnots, ref result_old);
            MEM.Alloc(cs * mesh.CountBoundKnots, ref bcIndex);
            for (int i = 0; i < Index.Length; i++)
            {
                int ai = i * cs;
                bcIndex[ai] = (uint)(Index[i] * cs);
                bcIndex[ai + 1] = (uint)(Index[i] * cs + 1);
            }
        }

        /// <summary>
        /// Решение задачи
        /// </summary>
        /// <param name="result">результат решения</param>
        public override void SolveTask(ref double[] eddyViscosity, double[] Ux, double[] Vy, double[] Vz, double[] Phi, double[] Vortex, double dt)
        {
            this.Ux = Ux;
            this.Phi = Phi;
            this.Vortex = Vortex;
            SolveTask(ref eddyViscosity);
        }
        /// <summary>
        /// Определение вязкости  К-Е модели на текущем шаге по времени
        /// </summary>
        /// <param name="eddyViscosity"></param>
        public override void SolveTask(ref double[] eddyViscosity)
        {
            uint ee = 0;
            try
            {
                // итераций по нелинейности на текущем шаге по времени
                for (int nIdx = 0; nIdx < NLine; nIdx++)
                {
                    MEM.MemCopy(ref result_old, result);
                    // расчет функции тока и функции вихря
                    Console.WriteLine("Ken & Eps: шаг по нелинейности: " + nIdx.ToString());
                    MEM.Alloc((int)algebra.N, ref result);
                    algebra.Clear();
                    int li, lj;
                    int Count = cu * cs;
                    for (uint elem = 0; elem < mesh.CountElements; elem++)
                    {
                        ee = elem;
                        // локальная матрица часть СЛАУ
                        MEM.Alloc2DClear(Count, ref LaplMatrix);
                        MEM.Alloc2DClear(Count, ref RMatrix);
                        MEM.AllocClear(Count, ref LocalRight);
                        double[] b = dNdx[elem];
                        double[] c = dNdy[elem];
                        uint i0 = eKnots[elem].Vertex1;
                        uint i1 = eKnots[elem].Vertex2;
                        uint i2 = eKnots[elem].Vertex3;
                        // получить узлы КЭ
                        uint[] knots = { i0, i1, i2 };
                        double Se = S[elem];

                        double eMu = (eddyViscosity[i0] + eddyViscosity[i1] + eddyViscosity[i2]) / 3.0;
                        double eVortex = (Vortex[i0] + Vortex[i1] + Vortex[i2]) / 3.0;
                        double eMu_k = eMu / Sigma_k + SPhysics.mu;
                        double eMu_e = eMu / Sigma_e + SPhysics.mu;
                        double eMu_p = eMu + Sigma_e + SPhysics.mu;

                        double eKen = (Ken[i0] + Ken[i1] + Ken[i2]) / 3.0;
                        double eEps = (Eps[i0] + Eps[i1] + Eps[i2]) / 3.0;
                        double eK_E = eKen / (eEps + MEM.Error14);
                        // Вычисление ЛПЧ от объемных сил    
                        double pk = eVortex * eVortex;

                        double eVx = c[0] * Phi[i0] + c[1] * Phi[i1] + c[2] * Phi[i2];
                        double eVy = -(b[0] * Phi[i0] + b[1] * Phi[i1] + b[2] * Phi[i2]);

                        double mV = Math.Sqrt(eVx * eVx + eVy * eVy);
                        double Hk = Math.Sqrt(Se) / 1.4;
                        // сеточное число Пекле
                        double Pe = SPhysics.rho_w * mV * Hk / (2 * eMu);
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
                            double L_SUPG = omega * (Hx[elem] * Ax * b[ai] + Hy[elem] * Ay * c[ai]);
                            li = cs * ai;
                            double Lb = (1 / 3.0 + L_SUPG);
                            double La = rho_w * Lb;

                            for (int aj = 0; aj < cu; aj++)
                            {
                                lj = cs * aj;
                                // матрица масс
                                double M_ab = MM[ai][aj] * Se;
                                // матрица масс SUPG
                                double _M_ab = rho_w * (M_ab + L_SUPG * Se / 3.0);
                                // матрица вязкости члены плоские
                                double K_ab = (b[ai] * b[aj] + c[ai] * c[aj]) * Se;
                                // матрица конверкции 
                                double C_agb = La * ((c[0] * b[aj] - b[0] * c[aj]) * Phi[i0] +
                                                     (c[1] * b[aj] - b[1] * c[aj]) * Phi[i1] +
                                                     (c[2] * b[aj] - b[2] * c[aj]) * Phi[i2]) * Se;

                                LaplMatrix[li][lj] = C_agb + eMu_k * K_ab;
                                LaplMatrix[li][lj + 1] = _M_ab;
                                LaplMatrix[li + 1][lj] = -C_e1 * C_mu * pk;
                                LaplMatrix[li + 1][lj + 1] = C_agb + eMu_e * K_ab + C_e2 * eK_E * _M_ab;
                            }
                            LocalRight[li] = Lb * eMu_p * pk;
                            LocalRight[li + 1] = 0;
                        }
                        // получем значения адресов неизвестных
                        GetAdress(knots, ref adressBound);
                        // добавление вновь сформированной ЛЖМ в ГМЖ
                        algebra.AddToMatrix(LaplMatrix, adressBound);
                        // добавление вновь сформированной ЛПЧ в ГМЖ
                        algebra.AddToRight(LocalRight, adressBound);
                    }
                    // Выполнение граничных условий для функции вихря
                    // VortexBC();
                    // Выполнение граничных условий для функции тока
                    algebra.BoundConditions(0, bcIndex);
                    // решение
                    algebra.Solve(ref result);

                    for (int i = 0; i < result.Length; i++)
                        result_old[i] = relax * result[i] + (1 - relax) * result_old[i];

                    for (int i = 0; i < mesh.CountKnots; i++)
                    {
                        Ken[i] = result_old[2 * i];
                        Eps[i] = result_old[2 * i + 1];
                        if (MEM.Equals(Eps[i], 0) == true)
                            eddyViscosity[i] = 0;
                        else
                            eddyViscosity[i] = rho_w * C_mu * Ken[i] * Ken[i] / Eps[i];
                    }
                    //************************************************************************************************************************
                    // Считаем невязку
                    //************************************************************************************************************************
                    double epsVortex = 0.0;
                    double normVortex = 0.0;

                    for (int i = 0; i < result.Length; i++)
                    {
                        normVortex += result_old[i] * result_old[i];
                        epsVortex += (result_old[i] - result[i]) * (result_old[i] - result[i]);
                    }
                    double residual = Math.Sqrt(epsVortex / normVortex);
                    Console.WriteLine("n {0} residual {1}", nIdx, residual);
                    if (residual < MEM.Error5)
                        break;
                }
                // расчет поля скорости
                Console.WriteLine("Vy ~ Vz:");
                //CalcVelosityPlane(Phi, ref Vy, ref Vz);
                MEM.MemCopy(ref result_old, result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ee = " + ee.ToString() + " " + ex.Message);
            }
        }
    }

    /// <summary>
    /// Стационарная скалярная k - e модель  
    /// </summary>
    [Serializable]
    public class EddyViscositySSKEModelTri : AEddyViscosityKETri
    {
        public EddyViscositySSKEModelTri(ETurbViscType eTurbViscType, BEddyViscosityParam p, TypeTask tt)
            : base(eTurbViscType,tt, p.NLine)
        {
            cs = 1;
        }
        public override void SetTask(IMesh mesh, IAlgebra algebra, IMeshWrapper wMesh = null)
        {
            base.SetTask(mesh, algebra, wMesh);
            MEM.Alloc(mesh.CountKnots, ref MRight);
            MEM.Alloc(mesh.CountBoundKnots, ref bcIndex);
            for (int i = 0; i < Index.Length; i++)
                bcIndex[i] = (uint)(Index[i]);
        }
        /// <summary>
        /// Решение задачи
        /// </summary>
        /// <param name="result">результат решения</param>
        public override void SolveTask(ref double[] eddyViscosity, double[] Ux, double[] Vy, double[] Vz, double[] Phi, double[] Vortex, double dt)
        {
            this.Ux = Ux;
            this.Phi = Phi;
            this.Vortex = Vortex;
            SolveTask(ref eddyViscosity);
        }
        /// <summary>
        /// Определение вязкости  К-Е модели на текущем шаге по времени
        /// </summary>
        /// <param name="eddyViscosity"></param>
        public override void SolveTask(ref double[] eddyViscosity)
        {
            uint ee = 0;
            try
            {
                // итераций по нелинейности на текущем шаге по времени
                for (int nIdx = 0; nIdx < NLine; nIdx++)
                {
                    MEM.MemCopy(ref Ken_old, Ken);
                    MEM.MemCopy(ref Eps_old, Eps);
                    // расчет функции тока и функции вихря
                    Console.WriteLine("Ken : шаг по нелинейности: " + nIdx.ToString());
                    algebra.Clear();
                    for (uint elem = 0; elem < mesh.CountElements; elem++)
                    {
                        ee = elem;
                        // локальная матрица часть СЛАУ
                        MEM.Alloc2DClear(cu, ref LaplMatrix);
                        MEM.AllocClear(cu, ref LocalRight);
                        double[] b = dNdx[elem];
                        double[] c = dNdy[elem];
                        uint i0 = eKnots[elem].Vertex1;
                        uint i1 = eKnots[elem].Vertex2;
                        uint i2 = eKnots[elem].Vertex3;
                        // получить узлы КЭ
                        uint[] knots = { i0, i1, i2 };
                        double Se = S[elem];
                        double eMu = (eddyViscosity[i0] + eddyViscosity[i1] + eddyViscosity[i2]) / 3.0;
                        double eVortex = (Vortex[i0] + Vortex[i1] + Vortex[i2]) / 3.0;
                        double eMu_k = eMu / Sigma_k + SPhysics.mu;
                        double eMu_p = eMu + Sigma_e + SPhysics.mu;

                        double eKen = (Ken_old[i0] + Ken_old[i1] + Ken_old[i2]) / 3.0;
                        double eEps = (Eps_old[i0] + Eps_old[i1] + Eps_old[i2]) / 3.0;

                        double eE_K = eEps / (eKen + MEM.Error14);
                        // Вычисление ЛПЧ от объемных сил    
                        double pk = eVortex * eVortex;

                        double eVx = c[0] * Phi[i0] + c[1] * Phi[i1] + c[2] * Phi[i2];
                        double eVy = -(b[0] * Phi[i0] + b[1] * Phi[i1] + b[2] * Phi[i2]);

                        double mV = Math.Sqrt(eVx * eVx + eVy * eVy);
                        double Hk = Math.Sqrt(Se) / 1.4;
                        // сеточное число Пекле
                        double Pe = SPhysics.rho_w * mV * Hk / (2 * eMu);
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
                            double L_SUPG = omega * (Hx[elem] * Ax * b[ai] + Hy[elem] * Ay * c[ai]);
                            double Lb = (1 / 3.0 + L_SUPG);
                            double La = rho_w * Lb;
                            for (int aj = 0; aj < cu; aj++)
                            {
                                // матрица масс
                                double M_ab = MM[ai][aj] * Se;
                                // матрица масс SUPG
                                double _M_ab = rho_w * (M_ab + L_SUPG * Se / 3.0);
                                // матрица вязкости члены плоские
                                double K_ab = (b[ai] * b[aj] + c[ai] * c[aj]) * Se;
                                // матрица конверкции 
                                double C_agb = La * ((c[0] * b[aj] - b[0] * c[aj]) * Phi[i0] +
                                                     (c[1] * b[aj] - b[1] * c[aj]) * Phi[i1] +
                                                     (c[2] * b[aj] - b[2] * c[aj]) * Phi[i2]) * Se;

                                LaplMatrix[ai][aj] = C_agb + eMu_k * K_ab + eE_K * _M_ab;
                            }
                            LocalRight[ai] = Lb * eMu_p * pk;
                        }
                        // получем значения адресов неизвестных
                        GetAdress(knots, ref adressBound, cs);
                        // добавление вновь сформированной ЛЖМ в ГМЖ
                        algebra.AddToMatrix(LaplMatrix, adressBound);
                        // добавление вновь сформированной ЛЖМ в ГМЖ
                        algebra.AddToRight(LocalRight, adressBound);
                    }
                    // Выполнение граничных условий для функции тока
                    uint[] bcIndex0 = mesh.GetBoundKnotsByMarker(0);

                    uint[] bcIndex2 = mesh.GetBoundKnotsByMarker(2);
                    double[] k_wl = new double[bcIndex2.Length];
                    for (int n = 0; n < bcIndex2.Length; n++)
                    {
                        k_wl[n] = 1.5 * (0.2 * Ux[bcIndex2[n]]) * (0.2 * Ux[bcIndex2[n]]);
                    }
                    algebra.BoundConditions(0, bcIndex0);
                    algebra.BoundConditions(k_wl, bcIndex2);
                    //algebra.Print();
                    // решение
                    algebra.Solve(ref Ken);
                    MEM.Relax(ref Ken, Ken_old);


                    Console.WriteLine("Eps: шаг по нелинейности: " + nIdx.ToString());
                    algebra.Clear();
                    for (uint elem = 0; elem < mesh.CountElements; elem++)
                    {
                        ee = elem;
                        // локальная матрица часть СЛАУ
                        MEM.Alloc2DClear(cu, ref LaplMatrix);
                        MEM.AllocClear(cu, ref LocalRight);
                        double[] b = dNdx[elem];
                        double[] c = dNdy[elem];
                        uint i0 = eKnots[elem].Vertex1;
                        uint i1 = eKnots[elem].Vertex2;
                        uint i2 = eKnots[elem].Vertex3;
                        // получить узлы КЭ
                        uint[] knots = { i0, i1, i2 };
                        double Se = S[elem];

                        double eMu = (eddyViscosity[i0] + eddyViscosity[i1] + eddyViscosity[i2]) / 3.0;
                        double eVortex = (Vortex[i0] + Vortex[i1] + Vortex[i2]) / 3.0;
                        double eMu_k = eMu / Sigma_k + SPhysics.mu;
                        double eMu_e = eMu / Sigma_e + SPhysics.mu;
                        double eMu_p = eMu + Sigma_e + SPhysics.mu;

                        //double eKen = (Ken[i0] + Ken[i1] + Ken[i2]) / 3.0;
                        double eKen = (Ken[i0] + Ken[i1] + Ken[i2]) / 3.0;
                        double eEps = (Eps_old[i0] + Eps_old[i1] + Eps_old[i2]) / 3.0;

                        double eE_K = eEps / (eKen + MEM.Error14);
                        // Вычисление ЛПЧ от объемных сил    
                        double pk = eVortex * eVortex;

                        double eVx = c[0] * Phi[i0] + c[1] * Phi[i1] + c[2] * Phi[i2];
                        double eVy = -(b[0] * Phi[i0] + b[1] * Phi[i1] + b[2] * Phi[i2]);

                        double mV = Math.Sqrt(eVx * eVx + eVy * eVy);
                        double Hk = Math.Sqrt(Se) / 1.4;
                        // сеточное число Пекле
                        double Pe = SPhysics.rho_w * mV * Hk / (2 * eMu);
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
                            double L_SUPG = omega * (Hx[elem] * Ax * b[ai] + Hy[elem] * Ay * c[ai]);
                            double Lb = (1 / 3.0 + L_SUPG);
                            double La = rho_w * Lb;
                            for (int aj = 0; aj < cu; aj++)
                            {
                                // матрица масс
                                double M_ab = MM[ai][aj] * Se;
                                // матрица масс SUPG
                                double _M_ab = rho_w * (M_ab + L_SUPG * Se / 3.0);
                                // матрица вязкости члены плоские
                                double K_ab = (b[ai] * b[aj] + c[ai] * c[aj]) * Se;
                                // матрица конверкции 
                                double C_agb = La * ((c[0] * b[aj] - b[0] * c[aj]) * Phi[i0] +
                                                     (c[1] * b[aj] - b[1] * c[aj]) * Phi[i1] +
                                                     (c[2] * b[aj] - b[2] * c[aj]) * Phi[i2]) * Se;
                                LaplMatrix[ai][aj] = C_agb + eMu_e * K_ab + C_e2 * eE_K * _M_ab;
                            }
                            LocalRight[ai] = La * eKen * C_mu * C_e1 * pk;
                        }
                        // получем значения адресов неизвестных
                        GetAdress(knots, ref adressBound, cs);
                        // добавление вновь сформированной ЛЖМ в ГМЖ
                        algebra.AddToMatrix(LaplMatrix, adressBound);
                        algebra.AddToRight(LocalRight, adressBound);
                    }

                    // Вычисление граничных условий для функции диссипации
                    IAFacet[] facet = mWDLinkTri.boundaryFacets;
                    double[] BL = mWDLinkTri.FacetsLen;
                    double[] bcL = new double[mesh.CountKnots];
                    double[] bEpsKnots = new double[mesh.CountKnots];
                    uint[] bcIndx = new uint[facet.Length];
                    for (int i = 0; i < facet.Length; i++)
                    {
                        double L2 = BL[i] / 2;
                        double Eps_wall_e = 0;
                        if (facet[i].BoundaryFacetsMark == 0)
                        {
                            int elem = facet[i].OwnerElem;
                            uint i0 = eKnots[elem].Vertex1;
                            uint i1 = eKnots[elem].Vertex2;
                            uint i2 = eKnots[elem].Vertex3;
                            double eKen = (Ken[i0] + Ken[i1] + Ken[i2]) / 3.0;
                            double he = Math.Sqrt(S[elem]);
                            Eps_wall_e = C_mu * eKen * Math.Sqrt(Math.Abs(eKen)) / (SPhysics.kappa_w * cmu4 * he);
                            bcL[facet[i].Pointid1] += L2;
                            bcL[facet[i].Pointid2] += L2;
                            bEpsKnots[facet[i].Pointid1] += L2 * Eps_wall_e;
                            bEpsKnots[facet[i].Pointid2] += L2 * Eps_wall_e;
                        }
                        bcL[facet[i].Pointid1] += L2;
                        bcL[facet[i].Pointid2] += L2;
                        bEpsKnots[facet[i].Pointid1] += L2 * Eps_wall_e;
                        bEpsKnots[facet[i].Pointid2] += L2 * Eps_wall_e;
                    }
                    //uint[] bcIndex0 = mesh.GetBoundKnotsByMarker(0);
                    double[] bcEpsKnots = new double[bcIndex0.Length];
                    for (int i = 0; i < bcIndex0.Length; i++)
                        bcEpsKnots[i] = bEpsKnots[bcIndex0[i]] / bcL[bcIndex0[i]];
                    //Выполнение граничных условий для функции
                    algebra.BoundConditions(bcEpsKnots, bcIndex0);
                    //algebra.BoundConditions(0, bcIndex);
                    //algebra.Print();
                    // решение
                    algebra.Solve(ref Eps);
                    MEM.Relax(ref Eps, Eps_old);

                    for (int i = 0; i < mesh.CountKnots; i++)
                    {
                        if (MEM.Equals(Eps[i], 0) == true)
                            eddyViscosity[i] = 0;
                        else
                        {
                            eddyViscosity[i] = rho_w * C_mu * Ken[i] * Ken[i] / Eps[i];
                            if (eddyViscosity[i] < MEM.Error12)
                                eddyViscosity[i] = 0;
                        }
                    }
                    double[] bMuKnots = new double[mesh.CountKnots];
                    for (int i = 0; i < facet.Length; i++)
                    {
                        double L2 = BL[i] / 2;
                        if (facet[i].BoundaryFacetsMark == 0)
                        {
                            int elem = facet[i].OwnerElem;
                            uint i0 = eKnots[elem].Vertex1;
                            uint i1 = eKnots[elem].Vertex2;
                            uint i2 = eKnots[elem].Vertex3;
                            double eKen = (Ken[i0] + Ken[i1] + Ken[i2]) / 3.0;
                            double he = Math.Sqrt(S[elem]);
                            double z_plus = cmu4 * Math.Sqrt(Math.Abs(eKen)) * he / SPhysics.nu;
                            double mu_wall = SPhysics.nu * z_plus / (Math.Log(Eps_wall * z_plus) / SPhysics.kappa_w);
                            bMuKnots[facet[i].Pointid1] += L2 * mu_wall;
                            bMuKnots[facet[i].Pointid2] += L2 * mu_wall;
                        }
                    }
                    for (int i = 0; i < bcIndex.Length; i++)
                        eddyViscosity[bcIndex[i]] = bMuKnots[bcIndex[i]] / bcL[bcIndex[i]];



                    //************************************************************************************************************************
                    // Считаем невязку
                    //************************************************************************************************************************
                    double epsVortex = 0.0;
                    double normVortex = 0.0;

                    for (int i = 0; i < Eps_old.Length; i++)
                    {
                        normVortex += Eps_old[i] * Eps_old[i];
                        epsVortex += (Eps_old[i] - Eps[i]) * (Eps_old[i] - Eps[i]);
                    }
                    double residual = Math.Sqrt(epsVortex / normVortex);
                    Console.WriteLine("n {0} residual {1}", nIdx, residual);
                    if (residual < MEM.Error5)
                        break;
                }
                // расчет поля скорости
                Console.WriteLine("Vy ~ Vz:");

            }
            catch (Exception ex)
            {
                Console.WriteLine("ee = " + ee.ToString() + " " + ex.Message);
            }
        }

    }

    /// <summary>
    /// Нестационарная скалярная k - e модель  
    /// </summary>
    [Serializable]
    public class EddyViscositySKEModelTri : AEddyViscosityKETri
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
        /// Для правой части
        /// </summary>
        protected IAlgebra Ralgebra;
        /// <summary>
        /// локальная матрица часть СЛАУ
        /// </summary>
        private double[][] RMatrix = null;
        public EddyViscositySKEModelTri(ETurbViscType eTurbViscType, BEddyViscosityParam p, TypeTask tt)
            : base(eTurbViscType, tt, p.NLine)
        {
            cs = 1;
        }
        public override void SetTask(IMesh mesh, IAlgebra algebra, IMeshWrapper wMesh = null)
        {
            base.SetTask(mesh, algebra, wMesh);
            MEM.Alloc(mesh.CountKnots, ref MRight);
            Ralgebra = algebra.Clone();
            MEM.Alloc(mesh.CountBoundKnots, ref bcIndex);
            for (int i = 0; i < Index.Length; i++)
                bcIndex[i] = (uint)(Index[i]);
        }
        /// <summary>
        /// Решение задачи
        /// </summary>
        /// <param name="result">результат решения</param>
        public override void SolveTask(ref double[] eddyViscosity, double[] Ux, double[] Vy, double[] Vz, double[] Phi, double[] Vortex, double dt)
        {
            this.Ux = Ux;
            this.Phi = Phi;
            this.Vortex = Vortex;
            this.dt = dt;
            SolveTask(ref eddyViscosity);
        }
        /// <summary>
        /// Определение вязкости  К-Е модели на текущем шаге по времени
        /// </summary>
        /// <param name="eddyViscosity"></param>
        public override void SolveTask(ref double[] eddyViscosity)
        {
            uint ee = 0;
            try
            {
                // итераций по нелинейности на текущем шаге по времени
                for (int nIdx = 0; nIdx < NLine; nIdx++)
                {
                    MEM.MemCopy(ref Ken_old, Ken);
                    MEM.MemCopy(ref Eps_old, Eps);
                    // расчет функции тока и функции вихря
                    Console.WriteLine("Ken : шаг по нелинейности: " + nIdx.ToString());
                    //MEM.Alloc((int)algebra.N, ref Ken);
                    algebra.Clear();
                    Ralgebra.Clear();
                    for (uint elem = 0; elem < mesh.CountElements; elem++)
                    {
                        ee = elem;
                        // локальная матрица часть СЛАУ
                        MEM.Alloc2DClear(cu, ref LaplMatrix);
                        MEM.Alloc2DClear(cu, ref RMatrix);
                        MEM.AllocClear(cu, ref LocalRight);
                        double[] b = dNdx[elem];
                        double[] c = dNdy[elem];
                        uint i0 = eKnots[elem].Vertex1;
                        uint i1 = eKnots[elem].Vertex2;
                        uint i2 = eKnots[elem].Vertex3;
                        // получить узлы КЭ
                        uint[] knots = { i0, i1, i2 };
                        double Se = S[elem];
                        double eMu = (eddyViscosity[i0] + eddyViscosity[i1] + eddyViscosity[i2]) / 3.0;
                        double eVortex = (Vortex[i0] + Vortex[i1] + Vortex[i2]) / 3.0;
                        double eMu_k = eMu / Sigma_k + SPhysics.mu;
                        double eMu_p = eMu + Sigma_e + SPhysics.mu;

                        double eKen = (Ken_old[i0] + Ken_old[i1] + Ken_old[i2]) / 3.0;
                        double eEps = (Eps_old[i0] + Eps_old[i1] + Eps_old[i2]) / 3.0;

                        double eE_K = eEps / (eKen + MEM.Error14);
                        // Вычисление ЛПЧ от объемных сил    
                        double pk = eVortex * eVortex;

                        double eVx = c[0] * Phi[i0] + c[1] * Phi[i1] + c[2] * Phi[i2];
                        double eVy = -(b[0] * Phi[i0] + b[1] * Phi[i1] + b[2] * Phi[i2]);

                        double mV = Math.Sqrt(eVx * eVx + eVy * eVy);
                        double Hk = Math.Sqrt(Se) / 1.4;
                        // сеточное число Пекле
                        double Pe = SPhysics.rho_w * mV * Hk / (2 * eMu);
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
                            double L_SUPG = omega * (Hx[elem] * Ax * b[ai] + Hy[elem] * Ay * c[ai]);
                            double Lb = (1 / 3.0 + L_SUPG);
                            double La = rho_w * Lb;
                            for (int aj = 0; aj < cu; aj++)
                            {
                                // матрица масс
                                double M_ab = MM[ai][aj] * Se;
                                // матрица масс SUPG
                                double _M_ab = rho_w * (M_ab + L_SUPG * Se / 3.0);
                                // матрица вязкости члены плоские
                                double K_ab = (b[ai] * b[aj] + c[ai] * c[aj]) * Se;
                                // матрица конверкции 
                                double C_agb = La * ((c[0] * b[aj] - b[0] * c[aj]) * Phi[i0] +
                                                     (c[1] * b[aj] - b[1] * c[aj]) * Phi[i1] +
                                                     (c[2] * b[aj] - b[2] * c[aj]) * Phi[i2]) * Se;

                                LaplMatrix[ai][aj] = _M_ab + dt * (1 - theta) * (C_agb + eMu_k * K_ab + eE_K * _M_ab);
                                RMatrix[ai][aj] = _M_ab - dt * theta * (C_agb + eMu_k * K_ab + eE_K * _M_ab);
                            }
                            LocalRight[ai] = Lb * eMu_p * pk;
                        }
                        // получем значения адресов неизвестных
                        GetAdress(knots, ref adressBound, cs);
                        // добавление вновь сформированной ЛЖМ в ГМЖ
                        algebra.AddToMatrix(LaplMatrix, adressBound);
                        // добавление вновь сформированной ЛЖМ в ГМЖ
                        Ralgebra.AddToMatrix(RMatrix, adressBound);
                        algebra.AddToRight(LocalRight, adressBound);
                    }
                    // Расчет
                    Ralgebra.getResidual(ref MRight, Ken_old, 0);
                    algebra.CopyRight(MRight);
                    // Выполнение граничных условий для функции вихря
                    // VortexBC();
                    // Выполнение граничных условий для функции тока
                    algebra.BoundConditions(0, bcIndex);
                    //algebra.Print();
                    // решение
                    algebra.Solve(ref Ken);
                    algebra.Clear();
                    Ralgebra.Clear();
                    Console.WriteLine("Eps: шаг по нелинейности: " + nIdx.ToString());
                    for (uint elem = 0; elem < mesh.CountElements; elem++)
                    {
                        ee = elem;
                        // локальная матрица часть СЛАУ
                        MEM.Alloc2DClear(cu, ref LaplMatrix);
                        MEM.Alloc2DClear(cu, ref RMatrix);
                        MEM.AllocClear(cu, ref LocalRight);
                        double[] b = dNdx[elem];
                        double[] c = dNdy[elem];
                        uint i0 = eKnots[elem].Vertex1;
                        uint i1 = eKnots[elem].Vertex2;
                        uint i2 = eKnots[elem].Vertex3;
                        // получить узлы КЭ
                        uint[] knots = { i0, i1, i2 };
                        double Se = S[elem];

                        double eMu = (eddyViscosity[i0] + eddyViscosity[i1] + eddyViscosity[i2]) / 3.0;
                        double eVortex = (Vortex[i0] + Vortex[i1] + Vortex[i2]) / 3.0;
                        double eMu_k = eMu / Sigma_k + SPhysics.mu;
                        double eMu_e = eMu / Sigma_e + SPhysics.mu;
                        double eMu_p = eMu + Sigma_e + SPhysics.mu;

                        double eKen = (Ken[i0] + Ken[i1] + Ken[i2]) / 3.0;
                        double eEps = (Eps_old[i0] + Eps_old[i1] + Eps_old[i2]) / 3.0;

                        double eE_K = eEps / (eKen + MEM.Error14);
                        // Вычисление ЛПЧ от объемных сил    
                        double pk = eVortex * eVortex;

                        double eVx = c[0] * Phi[i0] + c[1] * Phi[i1] + c[2] * Phi[i2];
                        double eVy = -(b[0] * Phi[i0] + b[1] * Phi[i1] + b[2] * Phi[i2]);

                        double mV = Math.Sqrt(eVx * eVx + eVy * eVy);
                        double Hk = Math.Sqrt(Se) / 1.4;
                        // сеточное число Пекле
                        double Pe = SPhysics.rho_w * mV * Hk / (2 * eMu);
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
                            double L_SUPG = omega * (Hx[elem] * Ax * b[ai] + Hy[elem] * Ay * c[ai]);

                            double Lb = (1 / 3.0 + L_SUPG);
                            double La = rho_w * Lb;

                            for (int aj = 0; aj < cu; aj++)
                            {
                                // матрица масс
                                double M_ab = MM[ai][aj] * Se;
                                // матрица масс SUPG
                                double _M_ab = rho_w * (M_ab + L_SUPG * Se / 3.0);
                                // матрица вязкости члены плоские
                                double K_ab = (b[ai] * b[aj] + c[ai] * c[aj]) * Se;
                                // матрица конверкции 
                                double C_agb = La * ((c[0] * b[aj] - b[0] * c[aj]) * Phi[i0] +
                                                     (c[1] * b[aj] - b[1] * c[aj]) * Phi[i1] +
                                                     (c[2] * b[aj] - b[2] * c[aj]) * Phi[i2]) * Se;

                                LaplMatrix[ai][aj] = _M_ab + dt * (1 - theta) * (C_agb + eMu_e * K_ab + C_e2 * eE_K * _M_ab);
                                RMatrix[ai][aj] = _M_ab - dt * theta * (C_agb + eMu_e * K_ab + C_e2 * eE_K * _M_ab);
                            }

                            LocalRight[ai] = La * eKen * C_mu * C_e1 * pk;
                        }
                        // получем значения адресов неизвестных
                        GetAdress(knots, ref adressBound, cs);
                        // добавление вновь сформированной ЛЖМ в ГМЖ
                        algebra.AddToMatrix(LaplMatrix, adressBound);
                        // добавление вновь сформированной ЛЖМ в ГМЖ
                        Ralgebra.AddToMatrix(RMatrix, adressBound);

                        algebra.AddToRight(LocalRight, adressBound);
                    }
                    // Расчет
                    Ralgebra.getResidual(ref MRight, Eps_old, 0);
                    algebra.CopyRight(MRight);

                    // Выполнение граничных условий для функции вихря
                    // VortexBC();
                    // Выполнение граничных условий для функции тока
                    algebra.BoundConditions(0, bcIndex);
                    //algebra.Print();
                    // решение
                    algebra.Solve(ref Eps);

                    for (int i = 0; i < mesh.CountKnots; i++)
                    {
                        if (MEM.Equals(Eps[i], 0) == true)
                            eddyViscosity[i] = 0;
                        else
                        {
                            eddyViscosity[i] = rho_w * C_mu * Ken[i] * Ken[i] / Eps[i];
                            if (eddyViscosity[i] < MEM.Error12)
                                eddyViscosity[i] = 0;
                        }
                    }
                    //************************************************************************************************************************
                    // Считаем невязку
                    //************************************************************************************************************************
                    double epsVortex = 0.0;
                    double normVortex = 0.0;

                    for (int i = 0; i < Eps_old.Length; i++)
                    {
                        normVortex += Eps_old[i] * Eps_old[i];
                        epsVortex += (Eps_old[i] - Eps[i]) * (Eps_old[i] - Eps[i]);
                    }
                    double residual = Math.Sqrt(epsVortex / normVortex);
                    Console.WriteLine("n {0} residual {1}", nIdx, residual);
                    if (residual < MEM.Error5)
                        break;
                }
                // расчет поля скорости
                Console.WriteLine("Vy ~ Vz:");

            }
            catch (Exception ex)
            {
                Console.WriteLine("ee = " + ee.ToString() + " " + ex.Message);
            }
        }
    }
    #endregion
}
