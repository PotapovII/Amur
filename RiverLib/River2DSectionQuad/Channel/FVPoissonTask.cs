
using System;
using System.Collections.Generic;
using System.Diagnostics;
using AlgebraLib;
using CommonLib;
using CommonLib.Physics;
using GeometryLib;
using MemLogLib;
using MeshLib;

namespace RiverLib
{
    /// <summary>
    /// Адаптер для решения задач контрольных объемов и конечных разностей на
    /// основе библиотеки линейной алгебры 
    /// </summary>
    public class FVBasePoissonTask
    {
        /// <summary>
        /// Плотность воды
        /// </summary>
        protected double rho_w = 1000;

        public bool Debug = true;
        /// <summary>
        /// Решатель для поля скорости
        /// </summary>
        public IAlgebra algebra = null;
        /// <summary>
        /// Расчетная сетка области для отрисовки полей задачи
        /// </summary>
        public ARectangleMesh mesh;
        /// <summary>
        /// количество узлов (i) по направлению X 
        /// </summary>
        public int Nx;
        /// <summary>
        /// количество узлов (j) по направлению Y
        /// </summary>
        public int Ny;
        /// <summary>
        /// количество узлов (i) по направлению X 
        /// </summary>
        public double dx;
        /// <summary>
        /// количество узлов (j) по направлению Y
        /// </summary>
        public double dy;
        double dx2;
        double dy2;
        double dx_2;
        double dy_2;
        /// <summary>
        /// адеса узлов с условиями Дирихле
        /// </summary>
        uint[] adressDirichlet = null;
        /// <summary>
        /// адеса узлов с условиями Неймана
        /// </summary>
        MeshIndex[] adressNeumann = null;
        /// <summary>
        /// граничные условия
        /// </summary>
        IBoundaryConditions BConditions = null;

        int[] Y_indexs = null;
        int NxMin = 1;
        int NyMin = 1;
        /// <summary>
        /// значение функции в узлах с условиями дирихле
        /// </summary>
        double[] BCValues = null;

        public double[][] S = null;
        double[][] Ae = null;
        double[][] Aw = null;
        double[][] An = null;
        double[][] As = null;
        double[][] Ap = null;
        double[][] Ap0 = null;
        double[][] Sc = null;
        double[] X = null;

        public FVBasePoissonTask(ARectangleMesh mesh, 
                IBoundaryConditions BConditions, 
                IAlgebra algebra,
                int[] Y_indexs = null, int NxMin = 1, int NyMin = 1)
        {
            this.mesh = mesh;
            this.NxMin = NxMin;
            this.NyMin = NyMin; 
            this.Y_indexs= Y_indexs;

            this.BConditions = BConditions;
            Nx = mesh.Nx;
            Ny = mesh.Ny;
            if (mesh.link == null)
                mesh.GetLinks();
            this.dx = mesh.dx;
            this.dy = mesh.dy;

            dx2 = 2 * dx;
            dy2 = 2 * dy;
            dx_2 = dx * dx;
            dy_2 = dy * dy;

            this.algebra = algebra;
        }
        protected void Init()
        {
            MEM.Alloc2D(Nx, Ny, ref S);
            MEM.Alloc2D(Nx, Ny, ref Ae);
            MEM.Alloc2D(Nx, Ny, ref Aw);
            MEM.Alloc2D(Nx, Ny, ref An);
            MEM.Alloc2D(Nx, Ny, ref As);
            MEM.Alloc2D(Nx, Ny, ref Ap);
            MEM.Alloc2D(Nx, Ny, ref Ap0);
            MEM.Alloc2D(Nx, Ny, ref Sc);
            MEM.Alloc(Nx * Ny, ref X);

            CalkBCAdress();
        }

        public void Start()
        {
            Init();
            algebra = algebra.Clone();
        }

        /// <summary>
        /// Расчет коэффициентов
        /// </summary>
        /// <param name="mas"></param>
        public virtual void CalkAk(double[][] mas)
        {
            if (Y_indexs == null)
            {
                for (int i = NxMin; i < Nx - 1; i++)        
                    for (int j = NyMin; j < Ny - 1; j++)
                    {
                        double Dp = mas[i][j];
                        double De = mas[i + 1][j];
                        double Dw = mas[i - 1][j];
                        double Dn = mas[i][j + 1];
                        double Ds = mas[i][j - 1];
                        Ae[i][j] = ((De + Dp) / 2.0) / (dx * dx);
                        Aw[i][j] = ((Dw + Dp) / 2.0) / (dx * dx);
                        As[i][j] = ((Ds + Dp) / 2.0) / (dy * dy);
                        An[i][j] = ((Dn + Dp) / 2.0) / (dy * dy);
                        Ap[i][j] = Ae[i][j] + Aw[i][j] + An[i][j] + As[i][j];
                    }
            }
            else
            {
                for (int i = NxMin; i < Nx - 1; i++)        
                    for (int j = Y_indexs[i] + 1; j < Ny - 1; j++)
                    {
                        double Dp = mas[i][j];
                        double De = mas[i + 1][j];
                        double Dw = mas[i - 1][j];
                        double Dn = mas[i][j + 1];
                        double Ds = mas[i][j - 1];
                        Ae[i][j] = ((De + Dp) / 2.0) / (dx * dx);
                        Aw[i][j] = ((Dw + Dp) / 2.0) / (dx * dx);
                        As[i][j] = ((Ds + Dp) / 2.0) / (dy * dy);
                        An[i][j] = ((Dn + Dp) / 2.0) / (dy * dy);
                        Ap[i][j] = Ae[i][j] + Aw[i][j] + An[i][j] + As[i][j];
                    }
            }
        }

        /// <summary>
        /// Сборка СЛАУ
        /// </summary>
        /// <param name="NxMin"></param>
        /// <param name="NyMin"></param>
        /// <param name="Y_indexs"></param>
        public void LinkAlgebra()
        {
            if (Y_indexs == null)
            {
                for (int i = NxMin; i < Nx - 1; i++)
                    for (int j = NyMin; j < Ny - 1; j++)
                        BuildSYS(i, j);
            }
            else
            {
                for (int i = NxMin; i < Nx - 1; i++)
                    for (int j = Y_indexs[i] + 1; j < Ny - 1; j++)
                        BuildSYS(i, j);
            }
        }
        /// <summary>
        /// Сборка СЛАУ для i j узла сетки
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        private void BuildSYS(int i, int j)
        {
            uint iE = (uint)mesh.map[i + 1][j];
            uint iW = (uint)mesh.map[i - 1][j];
            uint iN = (uint)mesh.map[i][j + 1];
            uint iS = (uint)mesh.map[i][j - 1];
            uint iP = (uint)mesh.map[i][j];
            uint[] knots = { iP, iE, iW, iN, iS };
            double aE = -Ae[i][j];
            double aW = -Aw[i][j];
            double aN = -An[i][j];
            double aS = -As[i][j];
            double aP = Ap[i][j];
            double[] A = { aP, aE, aW, aN, aS };
            algebra.AddStringSystem(A, knots, iP, Sc[i][j]);
        }

        #region Установка правой части СЛАУ
        /// <summary>
        /// Установка правой части СЛАУ
        /// </summary>
        /// <param name="F"></param>
        /// <param name="NxMin"></param>
        /// <param name="NyMin"></param>
        public virtual void CalkRight(double[][] F)
        {
            if (Y_indexs == null)
            {
                for (int i = NxMin; i < Nx; i++)
                    for (int j = NyMin; j < Ny; j++)
                        Sc[i][j] = F[i][j];
            }
            else
            {
                for (int i = NxMin; i < Nx; i++)
                    for (int j = Y_indexs[i]; j < Ny; j++)
                        Sc[i][j] = F[i][j];
            }
        }
        /// <summary>
        /// Установка правой части СЛАУ
        /// </summary>
        /// <param name="value"></param>
        /// <param name="NxMin"></param>
        /// <param name="NyMin"></param>
        public virtual void CalkRight(double value)
        {
            if (Y_indexs == null)
            {
                for (int i = NxMin; i < Nx; i++)
                    for (int j = NyMin; j < Ny; j++)
                        Sc[i][j] = value;
            }
            else
            {
                for (int i = NxMin; i < Nx; i++)
                    for (int j = Y_indexs[i]; j < Ny; j++)
                        Sc[i][j] = value;
            }
        }

        #endregion

        /// <summary>
        /// Выборка граничных узлов с условиями дирихле
        /// </summary>
        public void CalkBCAdress()
        {
            List<uint> LAdressD = new List<uint>();
            List<MeshIndex> LAdressN = new List<MeshIndex>();
            List<double> LValue = new List<double>();
            for (uint i = 0; i < mesh.BoundKnots.Length; i++)
            {
                int mark = mesh.BoundKnotsMark[i];
                if (mark == 0)
                    LAdressD.Add((uint)mesh.BoundKnots[i]);
                else
                    if (mark > 0)
                    {
                        int bi = mesh.BoundKnots[i];
                        MeshIndex link = mesh.GetNormalKnotsindex(bi);
                        LAdressN.Add(link);
                    }
            }
            adressDirichlet = LAdressD.ToArray();
            if (LAdressN.Count > 0)
                adressNeumann = LAdressN.ToArray();
            else
                adressNeumann = null;
        }
        /// <summary>
        /// Выполнение граничных условий дирихле по внутренним маркерам сетки
        /// </summary>
        /// <param name="BConditions">значения ГУ</param>
        public void DirichletBoundaryCondition(double scale = 1)
        {
            if (adressDirichlet == null)
                CalkBCAdress();
            MEM.Alloc(adressDirichlet.Length, ref BCValues);
            double[] DBCValue = BConditions.GetValueDir();
            for (uint i = 0; i < adressDirichlet.Length; i++)
            {
                int mark = mesh.BoundKnotsMark[i];
                if (mark == 0)
                {
                    BCValues[i] = DBCValue[mark]/ scale;
                }
            }
            algebra.BoundConditions(BCValues, adressDirichlet);
            //algebra.Print();
        }
        /// <summary>
        /// Выполнение граничных условий дирихле по внешним маркерам сетки
        /// </summary>
        public void SetDirichletBoundaryCondition(TypeBoundCond[] BCType)
        {
            double[] DBCValue = BConditions.GetValueDir();
            List<uint> LAdress = new List<uint>();
            List<double> LValue = new List<double>();
            for (uint i = 0; i < mesh.BoundKnots.Length; i++)
            {
                int mark = mesh.BoundKnotsMark[i];
                if (BCType[mark] == TypeBoundCond.Dirichlet)
                {
                    LAdress.Add((uint)mesh.BoundKnots[i]);
                    LValue.Add(DBCValue[mark]);
                }
            }
            uint[] adressDirichlet = LAdress.ToArray();
            double[] BCValues = LValue.ToArray();
            algebra.BoundConditions(BCValues, adressDirichlet);
        }
        /// <summary>
        /// Установк однородных ГУ Неймана
        /// </summary>
        public void NeumannBoundaryCondition()
        {
            if (adressDirichlet == null)
                CalkBCAdress();
            if (adressNeumann == null)
                return;
            MEM.Alloc(adressNeumann.Length, ref BCValues);
            
            double[] DBCValue = BConditions.GetValueNeu();
            int k = 0;
            for (uint i = 0; i < mesh.BoundKnots.Length; i++)
            {
                int mark = mesh.BoundKnotsMark[i];
                if (mark > 0)
                    BCValues[k] = DBCValue[mark];
            }

            for (int bi = 0; bi < adressNeumann.Length; bi++)
            {
                MeshIndex link = adressNeumann[bi];
                uint iP = (uint)link.i;
                uint iD = (uint)link.j;
                uint[] knots = { (uint)link.i, (uint)link.j };
                if(link.d == DirectMeshIndex.N)
                {
                    double[] A = { 1 / mesh.dy, -1 / mesh.dy };
                    algebra.AddStringSystem(A, knots, knots[0], BCValues[bi]);
                }
                else
                {
                    if (link.d == DirectMeshIndex.E)
                    {
                        double[] A = { 1 / mesh.dx, -1 / mesh.dx };
                        algebra.AddStringSystem(A, knots, knots[0], BCValues[bi]);
                    }
                    else // DirectMeshIndex.E
                    {
                        double[] A = { - 1 / mesh.dx, 1 / mesh.dx };
                        algebra.AddStringSystem(A, knots, knots[0], BCValues[bi]);
                    }
                }
            }
        }
        /// <summary>
        /// Решение СЛАУ
        /// </summary>
        /// <param name="mX"></param>
        /// <param name="X"></param>
        public void Solve(ref double[][] mX, ref double[] X)
        {
            try
            {
                if (Debug == true)
                {
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();
                    algebra.Solve(ref X);
                    mesh.Get1DValueTo2D(X, ref mX);
                    stopWatch.Stop();
                    string s = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    stopWatch.Elapsed.Hours, stopWatch.Elapsed.Minutes, stopWatch.Elapsed.Seconds,
                    stopWatch.Elapsed.Milliseconds / 10);
                    LOG.Print("Общее время расчета ", s);
                    algebra.Result.Print();
                    //LOG.Print("X",X);
                }
                else
                {
                    algebra.Solve(ref X);
                    mesh.Get1DValueTo2D(X, ref mX);
                }
            }
            catch (Exception ep)
            {
                LOG.Print("Ошибка " + ep.Message, " FVAdaptr.Solve ");
            }
        }

        /// <summary>
        /// Решение задачи Пуассона
        /// </summary>
        /// <param name="task">Задача</param>
        /// <param name="nu">вязкость</param>
        /// <param name="Sc">постоянная объемная сила</param>
        /// <param name="mX">решение на КР сетке</param>
        /// <param name="X">решение на симплекс сетке</param>
        public void PoissonTaskSolve(double[][] nu, double Sc, ref double[][] mX, ref double[] X)
        {
            Start();
            CalkAk(nu);
            CalkRight(Sc);
            LinkAlgebra();
            NeumannBoundaryCondition();
            DirichletBoundaryCondition();
            Solve(ref mX, ref X);
        }
        /// <summary>
        /// Получить градиент скалярного поля
        /// </summary>
        /// <param name="U">поле</param>
        /// <param name="dU_dx"></param>
        /// <param name="dU_dy"></param>
        public void GetGrad(double[][] U,ref double[][] dU_dx,ref double[][] dU_dy)
        {
            for (int i = NxMin; i < Nx - 1; i++)
                for (int j = Y_indexs[i] + 1; j < Ny - 1; j++)
                {
                    dU_dx[i][j] = (U[i + 1][j] - U[i - 1][j]) / dx2;
                    dU_dy[i][j] = (U[i][j + 1] - U[i][j - 1]) / dy2;
                }
        }
        /// <summary>
        /// Получить инвариант скалярного поля
        /// </summary>
        /// <param name="U">поле</param>
        /// <param name="dU_dx"></param>
        /// <param name="dU_dy"></param>
        public void GetOmega(double[][] U,ref double[][] Omega)
        {
            for (int i = NxMin; i < Nx - 1; i++)
                for (int j = Y_indexs[i] + 1; j < Ny - 1; j++)
                {
                    double Ue = U[i + 1][j];
                    double Uw = U[i - 1][j];
                    double Un = U[i][j + 1];
                    double Us = U[i][j - 1];
                    double dUdx = (Ue - Uw) / dx2;
                    double dUdy = (Un - Us) / dy2;
                    Omega[i][j] = Math.Sqrt(dUdx * dUdx + dUdy * dUdy);
                }
        }
        #region Учебная модель
        /// <summary>
        /// Решение задачи Пуассона для усеченной модели Рэя-Агарвала 
        /// </summary>
        /// <param name="task">Задача</param>
        /// <param name="nu">вязкость</param>
        /// <param name="Sc">постоянная объемная сила</param>
        /// <param name="mX">решение на КР сетке</param>
        /// <param name="X">решение на симплекс сетке</param>
        public void Nee_Kovasznay_TaskSolve(double[][] nu0, double[][] mU, ref double[][] mX, ref double[] X)
        {
            double tau = 0.01;
            Start();
            Calk_Nee_Kovasznay_TaskSolve(nu0, mU, tau, ref Sc);
            LinkAlgebra();
            NeumannBoundaryCondition();
            DirichletBoundaryCondition(rho_w);
            Solve(ref mX, ref X);
        }
        /// <summary>
        /// Расчет коэффициентов модели  Victor W. Nee, Leslie S.G. Kovasznay
        /// </summary>
        /// <param name="nu0_t"></param>

        
        public virtual void Calk_Nee_Kovasznay_TaskSolve(double[][] nu0_t, double[][] u0, double tau, ref double[][] B)
        {
            double nu = MEM.Error6;

            double btau = 1 / tau;
            double Ca = 0.1;
            double Cb = 1; 

            for (int i = NxMin; i < Nx - 1; i++)
                for (int j = Y_indexs[i] + 1; j < Ny - 1; j++)
                {
                    double nuP = nu0_t[i][j];
                    double nuE = nu0_t[i + 1][j];
                    double nuW = nu0_t[i - 1][j];
                    double nuN = nu0_t[i][j + 1];
                    double nuS = nu0_t[i][j - 1];

                    double Ue = u0[i + 1][j];
                    double Uw = u0[i - 1][j];
                    double Un = u0[i][j + 1];
                    double Us = u0[i][j - 1];

                    double dUdx = (Ue - Uw) / dx2;
                    double dUdy = (Un - Us) / dy2;
                    double dNdx = (nuE - nuW) / dx2;
                    double dNdy = (nuN - nuS) / dy2;

                    double Omegaii = Math.Sqrt(dUdx * dUdx + dUdy * dUdy);
                    double eta = nuP / mesh.d_min[i][j];

                    double Ap0 = btau - Ca * Omegaii;
                    Ae[i][j] = (nu + (nuE + nuP) / 2.0) / dx_2;
                    Aw[i][j] = (nu + (nuW + nuP) / 2.0) / dx_2;
                    As[i][j] = (nu + (nuS + nuP) / 2.0) / dy_2;
                    An[i][j] = (nu + (nuN + nuP) / 2.0) / dy_2;
                    Ap[i][j] = Ae[i][j] + Aw[i][j] + An[i][j] + As[i][j] + Ap0;
                    B[i][j] = btau * nuP - Cb * eta * eta;
                }
        }
        #endregion


        #region  модели Потапова

        /// <summary>
        /// Решение задачи Пуассона для определения вязкости со сдувом в сторону свободной поверхности
        /// по параболическому прототипу
        /// </summary>
        /// <param name="task">Задача</param>
        /// <param name="nu">вязкость</param>
        /// <param name="Sc">постоянная объемная сила</param>
        /// <param name="mX">решение на КР сетке</param>
        /// <param name="X">решение на симплекс сетке</param>
        public void Potapovs_02TaskSolve(double Q, ref double[][] mX, ref double[] X, double alphaQ)
        {
            double rho_w = 1000;
            Start();
            CalkPotapovs_02(Q, alphaQ);
            LinkAlgebra();
            NeumannBoundaryCondition();
            DirichletBoundaryCondition(rho_w);
            Solve(ref mX, ref X);
        }
        /// <summary>
        /// Расчет коэффициентов модели  
        /// </summary>
        /// <param name="nu0_t"></param>
        public virtual void CalkPotapovs_02(double Q, double alphaQ)
        {
            double velX=  0, velY = 0;

            double Pe_x = dx / 2;
            double Pe_y = dy / 2;
            velY = 0.01 * 1 / Pe_y;
            velX = 0.01 * 1 / Pe_x;

            for (int i = NxMin; i < Nx - 1; i++)        
            {
                for (int j = Y_indexs[i] + 1; j < Ny - 1; j++)
                {
                    Ae[i][j] = 1 / dx_2 + velX * mesh.d_min_dx[i][j] / dx2;
                    Aw[i][j] = 1 / dx_2 - velX * mesh.d_min_dx[i][j] / dx2;
                    An[i][j] = 1 / dy_2 + velY * mesh.d_min_dy[i][j] / dy2;
                    As[i][j] = 1 / dy_2 - velY * mesh.d_min_dy[i][j] / dy2;
                    Ap[i][j] = Ae[i][j] + Aw[i][j] + An[i][j] + As[i][j];
                    Sc[i][j] = Q * alphaQ;
                }
            }
        }

        /// <summary>
        /// Определение вязкости по формулам 
        /// </summary>
        /// <param name="task">Задача</param>
        /// <param name="nu">вязкость</param>
        /// <param name="Sc">постоянная объемная сила</param>
        /// <param name="mX">решение на КР сетке</param>
        /// <param name="X">решение на симплекс сетке</param>
        public void Potapovs_03TaskSolve(double u0, ref double[][] mMu, ref double[] Mu)
        {
            double Re = mesh.hydraulicRadius * u0 / SPhysics.nu;
            double C1 = Re / (0.46 * Re - 5.98);
            double Ca = Math.Exp(-(0.34 * Re - 11.5) / (0.46 * Re - 5.98));
            double Bf = 15;
            double h = mesh.MaxD;
            for (int i = NxMin; i < Nx - 1; i++)        
            {
                for (int j = Y_indexs[i] + 1; j < Ny - 1; j++)
                {
                    double y = mesh.d_min[i][j];
                    double nu = u0 * y * Ca * Math.Exp(-C1 * y / h);
                    double nu1 = nu * (1 - Math.Exp(-Bf * (1 - y / h)));
                    mMu[i][j] = SPhysics.rho_w * nu1;
                }
            }
            mesh.GetValueTo1D(mMu, ref Mu);
        }
 
        /// <summary>
        /// Решение задачи Пуассона для определения вязкости
        /// </summary>
        /// <param name="task">Задача</param>
        /// <param name="nu">вязкость</param>
        /// <param name="Sc">постоянная объемная сила</param>
        /// <param name="mX">решение на КР сетке</param>
        /// <param name="X">решение на симплекс сетке</param>
        public void Potapovs_04TaskSolve(double Q, double[][] mMu0, ref double[][] mX, ref double[] X, double alphaQ)
        {
            double a = 0.9;
            double rho_w = 1000;
            Start();
            CalkPotapovs_04(Q, alphaQ);
            LinkAlgebra();
            NeumannBoundaryCondition();
            DirichletBoundaryCondition(rho_w);
            Solve(ref mX, ref X);
            double maxMu = MEM.Max(mX);
            for (uint i = 0; i < Nx; i++)
                for (int j = 0; j < Ny; j++)
                    mMu0[i][j] = Math.Sqrt(mX[i][j]);
            double maxMuNew = MEM.Max(mMu0);
            for (uint i = 0; i < Nx; i++)
                for (int j = 0; j < Ny; j++)
                    mX[i][j] = ((1 - a) * Math.Sqrt(mMu0[i][j]) / maxMuNew * maxMu + a * mX[i][j]) * rho_w;
            for (uint i = 0; i < X.Length; i++)
                X[i] = ((1 - a) * Math.Sqrt(Math.Sqrt(X[i])) / maxMuNew * maxMu + a * X[i]) * rho_w;
        }
        /// <summary>
        /// Расчет коэффициентов модели  
        /// </summary>
        /// <param name="nu0_t"></param>
        public virtual void CalkPotapovs_04(double Q, double alphaQ)
        {
            for (int i = NxMin; i < Nx - 1; i++)        
            {
                int j0 = Y_indexs[i];
                for (int j = j0 + 1; j < Ny - 1; j++)
                {
                    Ae[i][j] = 1 / dx_2;
                    Aw[i][j] = 1 / dx_2;
                    As[i][j] = 1 / dy_2;
                    An[i][j] = 1 / dy_2;
                    Ap[i][j] = Ae[i][j] + Aw[i][j] + An[i][j] + As[i][j];
                    Sc[i][j] = Q * alphaQ;
                }
            }
        }


        #endregion

        #region Модель Секундова Nut - 92
        /// <summary>
        /// Решение задачи Пуассона для модели  Секундова Nut - 92
        /// </summary>
        /// <param name="task">Задача</param>
        /// <param name="nu">вязкость</param>
        /// <param name="Sc">постоянная объемная сила</param>
        /// <param name="mX">решение на КР сетке</param>
        /// <param name="X">решение на симплекс сетке</param>
        public void SecundovNut92TaskSolve(double[][] mNu, double[][] mU, ref double[][] mX, ref double[] X, double alphaQ = 1)
        {
            double tau = 0.0001;
            double rho_w = 1000;
            Start();
            CalkSecundovNut92(mNu, mU, tau, ref Sc, alphaQ);
            //CalkRight(Sc);
            LinkAlgebra();
            NeumannBoundaryCondition();
            DirichletBoundaryCondition(rho_w);
            Solve(ref mX, ref X);
        }
        /// <summary>
        /// Коррекция растояния до ближайшей стенки
        /// </summary>
        public void ChangeD_min()
        {
            double[] Ks = BConditions.GetValueDir(1);
            MEM.Alloc(mesh.BoundKnots.Length, ref mks);
            for (uint i = 0; i < mesh.BoundKnots.Length; i++)
            {
                int mark = mesh.BoundKnotsMark[i];
                mks[i] = Ks[mark];
            }
            for (uint n = 0; n < mesh.BoundKnots.Length; n++)
            {
                int bi = mesh.BoundKnots[n];
                int i = mesh.link[bi].i;
                int j = mesh.link[bi].j;
                mesh.d_min[i][j] = mesh.d_min[i][j] + 0.01 * mks[n];
            }
        }
        double[] mks = null;
        double[][] G_1 = null;
        double[][] G_2 = null;
        double[][] N_1 = null;
        /// <summary>
        /// Расчет коэффициентов модели  Секундова Nut - 92
        /// </summary>
        /// <param name="nu0_t"></param>
        public virtual void CalkSecundovNut92(double[][] mNu, double[][] u0, double tau, ref double[][] B, double alphaQ =1)
        {
            if(G_2 == null)
            {
                MEM.Alloc(Nx, Ny, ref G_1);
                MEM.Alloc(Nx, Ny, ref G_2);
                MEM.Alloc(Nx, Ny, ref N_1);
                // Коррекция растояния до ближайшей стенки
                ChangeD_min();
            }
            double nu = MEM.Error6;
            double nu7 = 7 * nu;
            double nuWall = 0;
            double rho = 1000;
            #region Секундова Nut - 92  константы
            double A1 = -0.5;
            double A2 = 4.0;

            double C0 = 0.8;
            double C1 = 1.6;
            double C2 = 0.1;
            double C3 = 4.0;
            //double C4 = 0.35;
            //double C5 = 3.5;
            double C6 = 2.9;
            double C7 = 31.5;
            double C8 = 0.1;
            #endregion
            double btau = 1 / tau;
            for (int i = NxMin; i < Nx - 1; i++)        
                for (int j = Y_indexs[i] + 1; j < Ny - 1; j++)
                {
                    double Up = u0[i][j];
                    double Ue = u0[i + 1][j];
                    double Uw = u0[i - 1][j];
                    double Un = u0[i][j + 1];
                    double Us = u0[i][j - 1];
                    double dUdx = (Ue - Uw) / dx2;
                    double dUdy = (Un - Us) / dy2;

                    G_1[i][j] = Math.Sqrt(dUdx * dUdx + dUdy * dUdy);
                    G_2[i][j] = Math.Abs((Ue - 2 * Up + Uw) / dx_2 + (Un - 2 * Up + Us) / dy_2);

                    double nuE = mNu[i + 1][j];
                    double nuW = mNu[i - 1][j];
                    double nuN = mNu[i][j + 1];
                    double nuS = mNu[i][j - 1];

                    double dNdx = (nuE - nuW) / dx2;
                    double dNdy = (nuN - nuS) / dy2;
                    N_1[i][j] = Math.Sqrt(dNdx * dNdx + dNdy * dNdy);
                }

            for (int i = NxMin; i < Nx - 1; i++)
            {
                int j = mesh.Y_init[i];
                if (j + 2 < Ny - 1 && Ny - 3 >= j)
                {
                    N_1[i][Ny - 1] = 2 * N_1[i][Ny - 2] - N_1[i][Ny - 3];
                    N_1[i][j] = 2 * N_1[i][j + 1] - N_1[i][j + 2];
                }
            }
            for (int j = Y_indexs[0]; j < Ny; j++)
                N_1[0][j] = 2 * N_1[1][j] - N_1[2][j];

            int j0 = mesh.Y_init[Nx - 1];
            for (int j = j0; j < Ny; j++)
                N_1[Nx - 1][j] = 2 * N_1[Nx - 2][j] - N_1[Nx - 3][j];
            
            for (int i = NxMin; i < Nx - 1; i++)        
                for (int j = Y_indexs[i] + 1; j < Ny - 1; j++)
                {
                    double nuP = mNu[i][j];
                    double nuE = mNu[i + 1][j];
                    double nuW = mNu[i - 1][j];
                    double nuN = mNu[i][j + 1];
                    double nuS = mNu[i][j - 1];

                    double N_1p = N_1[i][j];
                    double N_1e = N_1[i + 1][j];
                    double N_1w = N_1[i - 1][j];
                    double N_1n = N_1[i][j + 1];
                    double N_1s = N_1[i][j - 1];

                    double dN_1dx = (N_1e - N_1w) / dx2;
                    double dN_1dy = (N_1n - N_1s) / dy2;

                    double N_2 = dN_1dx * dN_1dx + dN_1dy * dN_1dy;
                    double D2nu = (nuE - 2 * nuP + nuW) / dx_2 + (nuN - 2 * nuP + nuS) / dy_2;

                    double Ap0;
                    double N1_2 = N_1[i][j] * N_1[i][j];
                    double dw = mesh.d_min[i][j];
                    double F1 = ( N_1[i][j] * dw + 0.4*C8*nuP ) / (nuP + C8 *nu + nuWall);
                    double chi = nuP / nu7;
                    double z2 = (chi * chi - 1.3 * chi + 1.0);
                    z2 = Math.Abs( z2 ) > MEM.Error14 ? z2 : MEM.Error14;
                    double F2 = (chi * chi + 1.3 * chi + 0.2)/ z2;

                    double Pv0 = C2 * F2 * (nuP * G_1[i][j] + A1 * Math.Pow(nuP, 4.0 / 3) * Math.Pow(G_2[i][j], 1.0 / 3));
                    double Pv1 = C2 * F2 * A2 * N_1p * Math.Sqrt((nu + nuP) * G_1[i][j]);
                    //double Pv2 = C3 * nuP * (D2nu + N_2);
                    double Pv2_1 = C3 * nuP * (D2nu);
                    double Pv2_2 = C3 * nuP * (N_2);
                    double Pv2 = 0;// Pv2_1;// + Pv2_2;
                    double Pv = Pv0 + Pv1 + Pv2;
                    double Dv = (C6 * nuP * (N_1p * dw + nuWall) + C7 * F1 * nu * nuP) / (dw* dw);

                    Ap0 =      rho * btau;
                    double P_D = rho * (btau * nuP + ((C1 - C0) * N1_2 + Pv - Dv) * alphaQ);
                    B[i][j] =  P_D;
                    Ae[i][j] = rho * (nu + C0 * (nuE + nuP) / 2.0) / dx_2;
                    Aw[i][j] = rho * (nu + C0 * (nuW + nuP) / 2.0) / dx_2;
                    As[i][j] = rho * (nu + C0 * (nuS + nuP) / 2.0) / dy_2;
                    An[i][j] = rho * (nu + C0 * (nuN + nuP) / 2.0) / dy_2;
                    Ap[i][j] = Ae[i][j] + Aw[i][j] + An[i][j] + As[i][j] + Ap0;
                }
        }
        #endregion

        #region  модели Spalart - Allmaras
        /// <summary>
        /// Решение задачи Пуассона
        /// </summary>
        /// <param name="task">Задача</param>
        /// <param name="nu">вязкость</param>
        /// <param name="Sc">постоянная объемная сила</param>
        /// <param name="mX">решение на КР сетке</param>
        /// <param name="X">решение на симплекс сетке</param>
        public void SpalartAllmarasTaskSolve(double[][] nuTilda0, double[][] mU,  ref double[][] mX, ref double[] X,
            ref double[][] dU_dx, ref double[][] dU_dy, ref double[][] dMu_dx, ref double[][] dMu_dy,
            double alphaQ)
        {
            double tau = 0.01;
            double rho_w = 1000;
            Start();
            CalkSpalartAllmaras(nuTilda0, mU, tau, ref Sc, ref dU_dx, ref dU_dy, ref dMu_dx, ref dMu_dy, alphaQ);
            //CalkRight(Sc);
            LinkAlgebra();
            NeumannBoundaryCondition();
            DirichletBoundaryCondition(rho_w);
            Solve(ref mX, ref X);
        }
        /// <summary>
        /// Расчет коэффициентов модели  Spalart - Allmaras
        /// </summary>
        /// <param name="nu0_t"></param>
        public virtual void CalkSpalartAllmaras(double[][] nu0_t, 
            double[][] u0, double tau, ref double[][] B,
            ref double[][] dU_dx, ref double[][] dU_dy, 
            ref double[][] dMu_dx, ref double[][] dMu_dy, 
            double alphaQ)
        {
            double nu = MEM.Error6;
            #region Spalart - Allmaras константы
            double sigma = 2 / 3.0;
            double Cb1 = 0.1355;
            double Cb2 = 0.622;
            double kappa = 0.4;
            double kappa2 = kappa * kappa;
            double Cw3_6 = 64;
            //double Cw3 = 2;
            double Cw2 = 0.3;
            double Cw1 = Cb1 / kappa2 + (1 + Cb2) / sigma;
            //double Ct1 = 1;
            //double Ct2 = 2;
            double Ct3 = 1.2;
            double Ct4 = 0.5;
            //double Utrim = 0.1;
            #endregion

            double[][] d_m = mesh.d_min;
            double n16 = 1 / 6.0;
            double btau = 1 / tau;

            for (int i = NxMin; i < Nx - 1; i++)        
                for (int j = Y_indexs[i] + 1; j < Ny - 1; j++)
                {
                    double nuP = nu0_t[i][j];
                    double nuE = nu0_t[i + 1][j];
                    double nuW = nu0_t[i - 1][j];
                    double nuN = nu0_t[i][j + 1];
                    double nuS = nu0_t[i][j - 1];
                    double Ue = u0[i + 1][j];
                    double Uw = u0[i - 1][j];
                    double Un = u0[i][j + 1];
                    double Us = u0[i][j - 1];
                    double dp = d_m[i][j];

                    double chi = nuP / nu;
                    double chi3 = chi * chi * chi;
                    double fv1 = chi3 / (chi3 + 357.911);
                    double fv2 = 1 - chi / (1 + chi * fv1);

                    double dUdx = (Ue - Uw) / dx2;
                    double dUdy = (Un - Us) / dy2;
                    double dNdx = (nuE - nuW) / dx2;
                    double dNdy = (nuN - nuS) / dy2;
                    
                    dU_dx[i][j] = dUdx;
                    dU_dy[i][j] = dUdy;
                    dMu_dx[i][j] = dNdx;
                    dMu_dy[i][j] = dNdy;

                    double kd = kappa * dp;
                    double kd2 = kd * kd;

                    double Omegaii = Math.Sqrt(dUdx * dUdx + dUdy * dUdy);
                    double Nuii = Cb2 * (dNdx * dNdx + dNdy * dNdy) / sigma;
                    double Eii = Omegaii + fv2 * nuP / kd2;
                    double r = Math.Min(nuP / (Eii * kd2), 10);
                    double gw = r + Cw2 * (Math.Pow(r, 6) - r);
                    double ag = (1 + Cw3_6) / (Math.Pow(gw, 6) + Cw3_6);
                    double fw = gw * Math.Pow(ag, n16);
                    double ft2 = Ct3 * Math.Exp(-Ct4 * chi * chi);

                    double Ap0 = btau 
                    + (Cw1 * fw - Cb1 * ft2 / kappa2) * nuP / (dp * dp) * alphaQ 
                    - Cb1 * (1 - ft2) * Eii / alphaQ;

                    Ae[i][j] = (nu + (nuE + nuP) / 2.0 / sigma) / dx_2;
                    Aw[i][j] = (nu + (nuW + nuP) / 2.0 / sigma) / dx_2;
                    As[i][j] = (nu + (nuS + nuP) / 2.0 / sigma) / dy_2;
                    An[i][j] = (nu + (nuN + nuP) / 2.0 / sigma) / dy_2;
                    Ap[i][j] = Ae[i][j] + Aw[i][j] + An[i][j] + As[i][j] + Ap0;
                    B[i][j] = btau * nuP + Nuii * alphaQ; 
                }
        }

        #endregion

        #region WrayAgarwal2018 модели Рэя-Агарвала 
        /// <summary>
        /// Решение задачи Пуассона для модели Рэя-Агарвала 
        /// </summary>
        /// <param name="task">Задача</param>
        /// <param name="nu">вязкость</param>
        /// <param name="Sc">постоянная объемная сила</param>
        /// <param name="mX">решение на КР сетке</param>
        /// <param name="X">решение на симплекс сетке</param>
        public void WrayAgarwal2018TaskSolve(double[][] nu0, double[][] mU, ref double[][] mX, ref double[] X,
            ref double[][] dU_dx, ref double[][] dU_dy, ref double[][] dMu_dx, ref double[][] dMu_dy, double alphaQ)
        {
            double tau = 0.01;
            double rho_w = 1000;
            Start();
            CalkWrayAgarwal2018(nu0, mU, tau, ref Sc, ref dU_dx, ref dU_dy, ref dMu_dx, ref dMu_dy, alphaQ);
            LinkAlgebra();
            NeumannBoundaryCondition();
            DirichletBoundaryCondition(rho_w);
            Solve(ref mX, ref X);
        }
        /// <summary>
        /// Расчет коэффициентов модели Рэя-Агарвала 
        /// </summary>
        /// <param name="R"></param>
        public virtual void CalkWrayAgarwal2018(double[][] R, 
            double[][] u0, double tau, ref double[][] B,
            ref double[][] dU_dx, ref double[][] dU_dy, 
            ref double[][] dMu_dx, ref double[][] dMu_dy, 
            double alphaQ)
        {
            double nu = MEM.Error6;
            #region  Рэя-Агарвала  константы
            double C1ke  = 0.1284;
            double C1kw = 0.0829;
            double sigma_ke = 1.0;
            double sigma_kw = 0.72;
            double kappa = 0.41;
            double kappa2 = kappa*kappa;
            double C2ke = C1ke / kappa2 + sigma_ke;
            double C2kw = C1kw / kappa2 + sigma_kw;
            double Cw = 8.54;
            double Cw3 = Cw * Cw * Cw;
            double Cw3nu3 = Cw3 * nu*nu*nu;
            // double Cmu = 0.09;
            double Cm = 8.0;
            #endregion
            double btau = 1 / tau;

            for (int i = NxMin; i < Nx - 1; i++)        
                for (int j = Y_indexs[i] + 1; j < Ny - 1; j++)
                {
                    double Ue = u0[i + 1][j];
                    double Uw = u0[i - 1][j];
                    double Un = u0[i][j + 1];
                    double Us = u0[i][j - 1];
                    double dUdx = (Ue - Uw) / dx2;
                    double dUdy = (Un - Us) / dy2;
                    dU_dx[i][j] = dUdx;
                    dU_dy[i][j] = dUdy;
                    S[i][j] = Math.Sqrt(dUdx * dUdx + dUdy * dUdy);
                }

            for (int i = NxMin; i < Nx - 1; i++)        
                for (int j = Y_indexs[i] + 1; j < Ny - 1; j++)
                {
                    double RP = R[i][j];
                    double nuE = R[i + 1][j];
                    double nuW = R[i - 1][j];
                    double nuN = R[i][j + 1];
                    double nuS = R[i][j - 1];
                    double Sp = S[i][j];

                    Sp = Math.Max(Sp, MEM.Error10);
                    double Se = S[i + 1][j];
                    double Sw = S[i - 1][j];
                    double Sn = S[i][j + 1];
                    double Ss = S[i][j - 1];

                    double dNdx = (nuE - nuW) / dx2;
                    double dNdy = (nuN - nuS) / dy2;

                    dMu_dx[i][j] = dNdx;
                    dMu_dy[i][j] = dNdy;

                    double dSdx = (Se - Sw) / dx2;
                    double dSdy = (Sn - Ss) / dy2;

                    double dSdx2 = dSdx * dSdx + dSdy * dSdy;
                    double Nuii = dNdx * dNdx + dNdy * dNdy;
                    double dRS = dSdx * dNdx + dSdy * dNdy;

                    double RP3 = RP * RP * RP;
                    double arg = 0.5 * (nu + RP) * (RP3 + Cw3nu3) / (RP3 * RP);
                    double f1 = Math.Tanh(arg * arg * arg * arg);
                    double C1 = f1 * (C1kw - C1ke) + C1ke;
                    double SigmaR = f1 * (sigma_kw - sigma_ke) + sigma_ke;

                    double Fii = C2ke * RP * RP * dSdx2 / (Sp * Sp);
                    double Ap0;

                    Ap0 = btau - (C1 * S[i][j] + f1 * C2kw * dRS / Sp);
                    B[i][j] = btau * RP - (1 - f1) * Math.Min(Fii, Cm * Nuii);

                    Ae[i][j] = (nu + SigmaR * (nuE + RP) / 2.0) / dx_2;
                    Aw[i][j] = (nu + SigmaR * (nuW + RP) / 2.0) / dx_2;
                    As[i][j] = (nu + SigmaR * (nuS + RP) / 2.0) / dy_2;
                    An[i][j] = (nu + SigmaR * (nuN + RP) / 2.0) / dy_2;
                    Ap[i][j] = Ae[i][j] + Aw[i][j] + An[i][j] + As[i][j] + Ap0;
                }
        }

        #endregion

        /// <summary>
        /// Установка постоянного значения в массив
        /// </summary>
        /// <param name="mas"></param>
        /// <param name="weight"></param>
        public void SetValue(ref double[][] mas, double value)
        {
            for (uint i = 0; i < Nx; i++)
                for (int j = 0; j < Ny; j++)
                    mas[i][j] = value;
        }
        /// <summary>
        /// Установка постоянного значения в массив
        /// </summary>
        /// <param name="mas"></param>
        /// <param name="weight"></param>
        public void MultValue(ref double[][] mas, double value)
        {
            for (uint i = 0; i < Nx; i++)
                for (int j = 0; j < Ny; j++)
                    mas[i][j] *= value;
        }
    }
}
