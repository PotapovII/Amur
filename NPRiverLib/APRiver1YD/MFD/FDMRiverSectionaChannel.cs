//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//---------------------------------------------------------------------------
//                  Расчет речного потока в створе русла
//---------------------------------------------------------------------------
//               кодировка  : 19.05.2023 Потапов И.И.
//---------------------------------------------------------------------------
//                          Перенос на КЭ метод
//---------------------------------------------------------------------------

using AlgebraLib;
using CommonLib;
using CommonLib.ChannelProcess;
using CommonLib.Delegate;
using CommonLib.Function;
using CommonLib.IO;
using CommonLib.Physics;
using FDMTaskLib;
using GeometryLib;
using GeometryLib.Vector;
using MemLogLib;
using MeshLib;
using NPRiverLib.AllWallFunctions;
using NPRiverLib.APRiver1YD.Params;
using NPRiverLib.IO;
using System;
using System.Collections.Generic;
using System.IO;

namespace NPRiverLib.APRiver1YD
{
    public class FDMRiverSectionaChannel : APRiver1YD<RiverStreamQuadParams>, IRiver
    {
        [NonSerialized]
        protected SimpleProcedure CalkVelocityMethd = null;
        #region Поля
        /// <summary>
        /// Поиск вязкости по объему
        /// </summary>
        protected bool FlagLookMu = true;
        /// <summary>
        /// Корректор расхода
        /// </summary>
        protected double alphaQ = 1;
        /// <summary>
        /// шнроховатость по маркерм
        /// </summary>
        protected double[] ks = null;
        /// <summary>
        /// шнроховатость на дне по узлам
        /// </summary>
        protected double[] ksBottom = null;
        /// <summary>
        /// однородные кравевые условия
        /// </summary>
        protected IBoundaryConditions bc;
        /// <summary>
        /// однородные кравевые условия
        /// </summary>
        protected IBoundaryConditions bcMu;
        /// <summary>
        /// Плотность песка
        /// </summary>
        protected double rho_s = SPhysics.PHYS.rho_s;
        /// <summary>
        /// кинематическая вязкость воды
        /// </summary>
        protected double Mu0 = SPhysics.mu;
        /// <summary>
        /// коэффициент Кармана
        /// </summary>
        protected double kappa = SPhysics.kappa_w;
        /// <summary>
        /// Вязкость потока
        /// </summary>
        protected double Mu_w = SPhysics.mu;
        /// <summary>
        /// Пористость песка
        /// </summary>
        protected double epsilon = SPhysics.PHYS.epsilon;
        /// <summary>
        /// Вязкость песка приведенная
        /// </summary>
        protected double MuS = 10000;

        protected double Rcoef = 1;

        protected double relax = 0.25;
        protected double Area = 0;
        protected double Q = 0;

        protected double sc;
        /// <summary>
        /// FlagStartMesh - первая генерация сетки
        /// </summary>
        protected bool FlagStartMesh = false;
        /// <summary>
        /// координаты дна по оси у
        /// </summary>
        protected double[] Zeta;
        protected double[] Depth;
        protected double[] U_star;
        protected double[] MuMax;
        /// <summary>
        /// расход потока
        /// </summary>
        protected IDigFunction flowRate;
        #endregion
        #region Поля для отрисовки
        /// <summary>
        /// Поле скоростей
        /// </summary>
        public double[] U;
        /// <summary>
        /// Поле вязкостей текущее и с предыдущей итерации
        /// </summary>
        public double[] Mu;
        /// <summary>
        /// Поле напряжений dy
        /// </summary>
        public double[] TauX;
        /// <summary>
        /// Касательное придонное напряжение
        /// </summary>
        public double[] bNormalTau;
        /// <summary>
        /// Касательное придонное гидростатическое напряжение
        /// </summary>
        public double[] bNormalTauGS;
        /// <summary>
        /// Придонное давление
        /// </summary>
        public double[] bPress;
        #endregion
        #region Поля для расчета
        /// <summary>
        /// Расчетная сетка области для задачи вычисления поля скорости в створе
        /// </summary>
        protected new ARectangleMesh mesh = null;
        /// <summary>
        /// Расчетная сетка области для задачи вычисления турбулентной вязкости в створе
        /// </summary>
        protected ARectangleMesh meshMu = null;
        /// <summary>
        /// задача вычисления поля скорости в створе
        /// </summary>
        [NonSerialized]
        protected FVBasePoissonTask taskU;
        /// <summary>
        /// задача вычисления турбулентной вязкости в створе
        /// </summary>
        [NonSerialized]
        protected FVBasePoissonTask taskMu;
        /// <summary>
        /// Поле скоростей
        /// </summary>
        public double[][] mU;
        /// <summary>
        /// Поле скоростей
        /// </summary>
        public double[][] mU0;
        /// <summary>
        /// минимальное растояние от границы
        /// </summary>
        public double[][] d_min;
        /// <summary>
        /// второй инвариант тензора скоростей деформаций
        /// </summary>
        public double[][] E_II;
        /// <summary>
        /// Поле производная скорости по х
        /// </summary>
        public double[][] dU_dx;
        /// <summary>
        /// Поле производная скорости по у
        /// </summary>
        public double[][] dU_dy;
        /// <summary>
        /// Поле вязкостей текущее и с предыдущей итерации
        /// </summary>
        public double[][] mMu;
        /// <summary>
        /// Поле вязкостей текущее и с предыдущей итерации
        /// </summary>
        public double[][] mMuTilda;
        /// <summary>
        /// Поле вязкостей текущее и с предыдущей итерации
        /// </summary>
        public double[][] mMuTilda0;
        /// <summary>
        /// Поле скоростей
        /// </summary>
        public double[][] dMu_dx;
        /// <summary>
        /// Поле скоростей
        /// </summary>
        public double[][] dMu_dy;
        /// <summary>
        /// Поле правая чать для уравнения вязкости
        /// </summary>
        public double[][] Qmu;
        /// <summary>
        /// Поле вязкостей текущее и с предыдущей итерации
        /// </summary>
        public double[][] mMu1;
        /// <summary>
        /// Поле плотности 
        /// </summary>
        public double[][] mRho;
        /// <summary>
        /// Поле напряжений dz
        /// </summary>
        public double[][] mTauY;
        /// <summary>
        /// Поле напряжений dy
        /// </summary>
        public double[][] mTauX;
        /// <summary>
        /// Поле напряжений - модуль
        /// </summary>
        public double[][] mTau;
        /// <summary>
        /// Поле напряжений - на стенке
        /// </summary>
        public double[] mTauW;
        /// <summary>
        /// y^+ 
        /// </summary>
        public double[] mYplus;
        /// <summary>
        /// Поле напряжений dz
        /// </summary>
        public double[][] mTauY1;
        /// <summary>
        /// Поле напряжений dy
        /// </summary>
        public double[][] mTauX1;
        /// <summary>
        /// Поле напряжений - модуль
        /// </summary>
        public double[][] mTau1;
        /// <summary>
        /// Поле напряжений dy
        /// </summary>
        public double[][] mTauX2;
        /// <summary>
        /// Поле напряжений dy
        /// </summary>
        public double[][] mTauY2;
        /// <summary>
        /// Поле напряжений dy
        /// </summary>
        public double[][] mTauX3;
        /// <summary>
        /// Поле напряжений dy
        /// </summary>
        public double[][] mTauY3;


        #endregion
        public double[][] aP, aE, aW, aN, aS, aQ;
        protected double uBuff, bap, fe, fw, fn, fs, Ap, An, Ae, As, Aw, MaxU, Dis;
        protected int NxMin = 1, NyMin = 1;
        protected double dx;
        protected double dy;

        // сдвиг системы координат
        double Xmin = 0, Ymin = 0;
        /// <summary>
        /// длина основания трапеции
        /// </summary>
        double Lx = 3;
        /// <summary>
        /// высота  трапеции
        /// </summary>
        double Ly = 1;
        /// <summary>
        /// заложение откосов
        /// </summary>
        double m = 1;
        int Nx, Ny;
        public FDMRiverSectionaChannel(RiverStreamQuadParams p) : base(p)
        {
            Version = "TriSecRiverTask1YBase 02.01.2024";
            name = "Расчет потока в трапецеидальном створе (np)";
        }

        public override void DefaultCalculationDomain(uint testTaskID = 0)
        {
            CreateCalculationDomain();
        }

        #region Вариантные методы  трапеция / уступ

        protected void InitTR()
        {
            sc = rho_w * Params.J * GRAV;


            int[] Mark = { 0, 1 };

            //bool smallArea = true;
            bool smallArea = false;

            if (smallArea == true)
            {
                int MeshRang = 5;

                Xmin = 0;
                Ymin = 0;
                Lx = 0.304;
                Ly = 0.0501;
                m = 1.03792;
                Params.Nx = 77 + MeshRang * 76;
                Params.Ny = 14 + MeshRang * 13;
                Params.Qwater0 = 0.00459;
            }
            else
            {
                int MeshRang = 20;
                Xmin = 0;
                Ymin = 0;
                m = 1.0;
                Lx = 3;
                Ly = 0.5;
                Params.Nx = 6 * MeshRang + 1;
                Params.Ny = MeshRang + 1;
                Params.Qwater0 = 0.67;
            }

            mesh = new ChannelTrapezMesh(Params.Nx, Params.Ny, Lx, Ly, m, Xmin, Ymin);
            mesh.CreateMesh(Mark, null, 0);
            meshMu = new ChannelTrapezMesh(Params.Nx, Params.Ny, Lx, Ly, m, Xmin, Ymin);
            meshMu.CreateMesh(Mark, null, 0);
            dx = mesh.dx;
            dy = mesh.dy;
            Params.Nx = mesh.Nx;
            Params.Ny = mesh.Ny;
            Nx = mesh.Nx;
            Ny = mesh.Ny;
            MEM.Alloc(Params.Nx, ref bottom_x);
            MEM.Alloc(Params.Nx, ref ksBottom);

            MEM.Alloc(Params.Nx, ref Zeta);

            bc = new BoundaryConditionsVar(6);

            double DepthMax = meshMu.Ly;
            double u0 = Math.Sqrt(GRAV * Params.J * DepthMax);
            double kappa = 0.41;
            ks = new double[2] { 0.0000001, 0.0000001 };
            double[] nv = new double[2] { 0, 0 };
            double[] dv = new double[2] { 0, 0 };

            for (int i = 0; i < ks.Length; i++)
                dv[i] = rho_w * kappa * u0 * ks[i];

            for (int i = 0; i < Params.Nx; i++)
                ksBottom[i] = ks[0];

            bcMu = new BoundaryConditionsVar(dv, nv, ks);
        }
        /// <summary>
        /// Инициализация 
        /// </summary>
        protected void InitST()
        {
            /////// <summary>
            /////// Граничные условия для поля скорости
            /////// </summary>
            //TypeBoundCond[] BCType =
            //{
            //    TypeBoundCond.Dirichlet,
            //    TypeBoundCond.Neumann
            //};
            ///// <summary>
            ///// Граничные условия для поля вязкости
            ///// </summary>
            //TypeBoundCond[] BCTypeMu =
            //{
            //    TypeBoundCond.Dirichlet,
            //    TypeBoundCond.Dirichlet
            //};

            sc = rho_w * Params.J * GRAV;


            int[] Mark = { 0, 1 };

            //bool smallArea = true;
            bool smallArea = false;

            if (smallArea == true)
            {
                int MeshRang = 5;

                Xmin = 0;
                Ymin = 0;
                Lx = 0.304;
                Ly = 0.0501;
                m = 1.03792;
                Params.Nx = 77 + MeshRang * 76;
                Params.Ny = 14 + MeshRang * 13;
                Params.Qwater0 = 0.00459;
            }
            else
            {
                int MeshRang = 20;
                Xmin = 0;
                Ymin = 0;
                m = 1.0;
                Lx = 3;
                Ly = 0.5;
                Params.Nx = 6 * MeshRang + 1;
                Params.Ny = MeshRang + 1;
                Params.Qwater0 = 0.67;
            }

            mesh = new ChannelTrapezMesh(Params.Nx, Params.Ny, Lx, Ly, m, Xmin, Ymin);
            mesh.CreateMesh(Mark, null, 0);
            meshMu = new ChannelTrapezMesh(Params.Nx, Params.Ny, Lx, Ly, m, Xmin, Ymin);
            meshMu.CreateMesh(Mark, null, 0);
            dx = mesh.dx;
            dy = mesh.dy;
            Params.Nx = mesh.Nx;
            Params.Ny = mesh.Ny;
            Nx = mesh.Nx;
            Ny = mesh.Ny;
            MEM.Alloc(Params.Nx, ref bottom_x);
            MEM.Alloc(Params.Nx, ref ksBottom);

            MEM.Alloc(Params.Nx, ref Zeta);

            bc = new BoundaryConditionsVar(6);

            double DepthMax = meshMu.Ly;
            double u0 = Math.Sqrt(Params.J * GRAV * DepthMax);
            double kappa = 0.41;
            ks = new double[2] { 0.0000001, 0.0000001 };
            double[] nv = new double[2] { 0, 0 };
            double[] dv = new double[2] { 0, 0 };

            for (int i = 0; i < ks.Length; i++)
                dv[i] = rho_w * kappa * u0 * ks[i];

            for (int i = 0; i < Params.Nx; i++)
                ksBottom[i] = ks[0];


            bcMu = new BoundaryConditionsVar(dv, nv, ks);
        }


        public void GetTauContur(ref double[] sMu, ref double[] sTauX, ref double[] sTauY,
            ref double[] sArg, ref double[] sX, ref double[] sY,
            ref double[] TauW, ref double[] Yplus, ref double[] TauW2, ref double[] Yplus2)
        {
            GetTauConturTR(ref sMu, ref sTauX, ref sTauY,
            ref sArg, ref sX, ref sY, ref TauW, ref Yplus, ref TauW2, ref Yplus2);
        }
        public void GetTauConturTR(ref double[] sMu, ref double[] sTauX, ref double[] sTauY,
            ref double[] sArg, ref double[] sX, ref double[] sY,
            ref double[] TauW, ref double[] Yplus, ref double[] TauW2, ref double[] Yplus2)
        {
            WallFunction_Nguyen fw = new WallFunction_Nguyen();

            List<double> LTauX = new List<double>();
            List<double> LTauY = new List<double>();
            List<double> LsMu = new List<double>();
            List<double> LsTau = new List<double>();
            List<double> LsX = new List<double>();
            List<double> LsY = new List<double>();

            List<double> LsTauW = new List<double>();
            List<double> LsYplus = new List<double>();
            List<double> LsTauW2 = new List<double>();
            List<double> LsYplus2 = new List<double>();

            double _tau = 0, _yplus = 0;
            double _tau2 = 0, _yplus2 = 0;
            // Поиск индекса левой границы
            double Y;
            double Ks;
            double stau = 0;
            stau = 0;
            // Запись напряжения с дна
            for (int i = 1; i < Nx - 2; i++)
            {
                stau += dx;
                int j = mesh.Y_init[i];
                if (j + 1 < Ny - 1)
                {
                    LTauX.Add(mTauX[i][j]);
                    LTauY.Add(mTauY[i][j]);
                    LsMu.Add(mMu[i][j]);
                    LsTau.Add(stau);
                    LsX.Add(mesh.X[i][j]);
                    LsY.Add(mesh.Y[i][j]);
                    Ks = ksBottom[i];
                    j = mesh.Y_init[i] + 1;
                    Y = mesh.d_min[i][j];
                    double U = mU[i][j];
                    double Mu = mMu[i][j];
                    fw.Tau_Nguyen(U, Y, Ks, Mu, ref _tau, ref _yplus);
                    LsTauW.Add(_tau);
                    LsYplus.Add(_yplus);
                    WallFunction_Lutskoy.Tau(U, Y, ref _tau2, ref _yplus2);
                    LsTauW2.Add(_tau2);
                    LsYplus2.Add(_yplus2);

                }
            }


            sMu = LsMu.ToArray();
            sTauX = LTauX.ToArray();
            sTauY = LTauY.ToArray();
            sArg = LsTau.ToArray();
            sX = LsX.ToArray();
            sY = LsY.ToArray();

            TauW = LsTauW.ToArray();
            Yplus = LsYplus.ToArray();
            TauW2 = LsTauW2.ToArray();
            Yplus2 = LsYplus2.ToArray();
        }

        public virtual void GetTauConturST(ref double[] sMu, ref double[] sTauX, ref double[] sTauY,
        ref double[] sArg, ref double[] sX, ref double[] sY,
        ref double[] TauW, ref double[] Yplus, ref double[] TauW2, ref double[] Yplus2)
        {
            WallFunction_Nguyen fw = new WallFunction_Nguyen();

            List<double> LTauX = new List<double>();
            List<double> LTauY = new List<double>();
            List<double> LsMu = new List<double>();
            List<double> LsTau = new List<double>();
            List<double> LsX = new List<double>();
            List<double> LsY = new List<double>();

            List<double> LsTauW = new List<double>();
            List<double> LsYplus = new List<double>();
            List<double> LsTauW2 = new List<double>();
            List<double> LsYplus2 = new List<double>();

            double _tau = 0, _yplus = 0;
            double _tau2 = 0, _yplus2 = 0;
            // Поиск индекса левой границы

            int idxI = 1;
            int idxJ = 2;
            double stau = -dy;

            // Запись напряжения с левой границы
            for (int j = Ny - 1; j > mesh.Y_init[idxI]; j--)
            {
                stau += dy;
                LTauX.Add(mTauX[idxI][j]);
                LTauY.Add(mTauY[idxI][j]);
                LsMu.Add(mMu[idxI][j]);
                LsTau.Add(stau);
                LsX.Add(mesh.X[idxI][j]);
                LsY.Add(mesh.Y[idxI][j]);

                double Y = dx;
                double U = mU[idxI][j];
                double Mu = mMu[idxI][j];
                double Ks = ks[ks.Length - 1];

                fw.Tau_Nguyen(U, Y, Ks, Mu, ref _tau, ref _yplus);
                LsTauW.Add(_tau);
                LsYplus.Add(_yplus);

                WallFunction_Lutskoy.Tau_LutskySeverin(U, Y, ref _tau2, ref _yplus2);
                LsTauW2.Add(_tau2);
                LsYplus2.Add(_yplus2);
            }

            // Запись напряжения с дна
            for (int i = idxI; i < Nx - 2; i++)
            {
                stau += dx;
                int j = mesh.Y_init[i] + idxJ;
                LTauX.Add(mTauX[i][j]);
                LTauY.Add(mTauY[i][j]);
                LsMu.Add(mMu[i][j]);
                LsTau.Add(stau);
                LsX.Add(mesh.X[i][j]);
                LsY.Add(mesh.Y[i][j]);

                j = mesh.Y_init[i] + 1;
                double Ks;
                double Y = dy;
                double U = mU[i][j];
                double Mu = mMu[i][j];

                Ks = ksBottom[i];
                if (mesh.Y_init[i] != mesh.Y_init[i - 1] || mesh.Y_init[i] != mesh.Y_init[i + 1])
                    Y = Math.Sqrt(dx * dx + dy * dy) / 2;
                else
                    Y = dy;

                fw.Tau_Nguyen(U, Y, Ks, Mu, ref _tau, ref _yplus);
                LsTauW.Add(_tau);
                LsYplus.Add(_yplus);
                WallFunction_Lutskoy.Tau(U, Y, ref _tau2, ref _yplus2);
                LsTauW2.Add(_tau2);
                LsYplus2.Add(_yplus2);

            }

            // Запись напряжения с правой границы
            for (int j = 1; j < Ny; j++)
            {
                stau += dy;
                LTauX.Add(mTauX[Nx - idxI - 1][j]);
                LTauY.Add(mTauY[Nx - idxI - 1][j]);
                LsMu.Add(mMu[Nx - idxI - 1][j]);
                LsTau.Add(stau);
                LsX.Add(mesh.X[Nx - idxI - 1][j]);
                LsY.Add(mesh.Y[Nx - idxI - 1][j]);

                double Y = dx;
                double U = mU[Nx - 2][j];
                double Mu = mMu[Nx - 2][j];
                double Ks = ks[3];
                fw.Tau_Nguyen(U, Y, Ks, Mu, ref _tau, ref _yplus);
                LsTauW.Add(_tau);
                LsYplus.Add(_yplus);
                WallFunction_Lutskoy.Tau(U, Y, ref _tau2, ref _yplus2);
                LsTauW2.Add(_tau2);
                LsYplus2.Add(_yplus2);
            }
            sMu = LsMu.ToArray();
            sTauX = LTauX.ToArray();
            sTauY = LTauY.ToArray();
            sArg = LsTau.ToArray();
            sX = LsX.ToArray();
            sY = LsY.ToArray();

            TauW = LsTauW.ToArray();
            Yplus = LsYplus.ToArray();
            TauW2 = LsTauW2.ToArray();
            Yplus2 = LsYplus2.ToArray();
        }

        /// <summary>
        /// Расчет напряжений в узлах МЦР
        /// </summary>
        public void CalkTau()
        {
            int i, j;

            for (i = 1; i < Nx - 1; i++)
                for (j = 1; j < Ny - 1; j++)
                {
                    mTauX[i][j] = 0.5 * (0.5 * (mMu[i][j] + mMu[i + 1][j]) * (mU[i + 1][j] - mU[i][j]) / dx +
                                         0.5 * (mMu[i][j] + mMu[i - 1][j]) * (mU[i][j] - mU[i - 1][j]) / dx);
                    mTauY[i][j] = 0.5 * (0.5 * (mMu[i][j] + mMu[i][j + 1]) * (mU[i][j + 1] - mU[i][j]) / dy +
                                         0.5 * (mMu[i][j] + mMu[i][j - 1]) * (mU[i][j] - mU[i][j - 1]) / dy);
                }

            for (i = 0; i < Nx; i++)
            {
                j = mesh.Y_init[i];
                if (j + 2 < Ny - 1)
                {
                    mTauX[i][j] = 2 * mTauX[i][j + 1] - mTauX[i][j + 2];
                    mTauY[i][j] = 2 * mTauY[i][j + 1] - mTauY[i][j + 2];
                }
            }
            for (i = 0; i < Nx; i++)
            {
                j = mesh.Y_init[i];
                if (Ny - 3 >= j)
                {
                    mTauX[i][Ny - 1] = 2 * mTauX[i][Ny - 2] - mTauX[i][Ny - 3];
                    mTauY[i][Ny - 1] = 2 * mTauY[i][Ny - 2] - mTauY[i][Ny - 3];
                }
            }
        }

        /// <summary>
        /// Расчет напряжений в узлах МЦР
        /// </summary>
        //public virtual void CalkTau()
        //{
        //    int i, j;

        //    for (i = 1; i < Nx - 1; i++)
        //        for (j = 1; j < Ny - 1; j++)
        //        {
        //            mTauX[i][j] = 0.5 * (0.5 * (mMu[i][j] + mMu[i + 1][j]) * (mU[i + 1][j] - mU[i][j]) / dx +
        //                                 0.5 * (mMu[i][j] + mMu[i - 1][j]) * (mU[i][j] - mU[i - 1][j]) / dx);
        //            mTauY[i][j] = 0.5 * (0.5 * (mMu[i][j] + mMu[i][j + 1]) * (mU[i][j + 1] - mU[i][j]) / dy +
        //                                 0.5 * (mMu[i][j] + mMu[i][j - 1]) * (mU[i][j] - mU[i][j - 1]) / dy);
        //        }
        //    // напряжения на границах
        //    for (j = Ny - 1; j > mesh.Y_init[0]; j--)
        //    {
        //        mTauX[0][j] = 2 * mTauX[1][j] - mTauX[2][j];
        //        mTauY[0][j] = 2 * mTauY[1][j] - mTauY[2][j];
        //    }
        //    for (i = 0; i < Nx; i++)
        //    {
        //        j = mesh.Y_init[i];
        //        mTauX[i][j] = 2 * mTauX[i][j + 1] - mTauX[i][j + 2];
        //        mTauY[i][j] = 2 * mTauY[i][j + 1] - mTauY[i][j + 2];
        //    }
        //    for (j = 0; j < Ny; j++)
        //    {
        //        mTauX[Nx - 1][j] = 2 * mTauX[Nx - 2][j] - mTauX[Nx - 3][j];
        //        mTauY[Nx - 1][j] = 2 * mTauY[Nx - 2][j] - mTauY[Nx - 3][j];
        //    }
        //    for (i = 0; i < Nx; i++)
        //    {
        //        mTauX[i][Ny - 1] = 2 * mTauX[i][Ny - 2] - mTauX[i][Ny - 3];
        //        mTauY[i][Ny - 1] = 2 * mTauY[i][Ny - 2] - mTauY[i][Ny - 3];
        //    }

        //    // коррекция по напряжениям
        //    //for (i = 0; i < Nx; i++)
        //    //    for (j = 0; j < Ny; j++)
        //    //    {
        //    //        mTauX[i][j] /= alphaQ;
        //    //        mTauY[i][j] /= alphaQ;
        //    //    }
        //}
        #endregion

        protected void InitSerializable()
        {
            for (int i = 0; i < Nx; i++)
            {
                bottom_x[i] = mesh.X[i][mesh.Ny - 1];
                Zeta[i] = mesh.Y[i][mesh.Y_init[i]];
            }

            switch (Params.ViscosityModel)
            {
                case TurbulentViscosityQuadModelOld.ViscosityConst:
                case TurbulentViscosityQuadModelOld.ViscosityWolfgangRodi:
                    CalkVelocityMethd = CalkVelocity;
                    //CalkVelocityMethd = CalkVelocityOld;
                    break;

                case TurbulentViscosityQuadModelOld.Potapovs_01:
                case TurbulentViscosityQuadModelOld.Potapovs_01Q:
                    // постоянная правая часть
                    CalkVelocityMethd = CalkTaskPotapovs_01;
                    break;
                case TurbulentViscosityQuadModelOld.Potapovs_02:
                case TurbulentViscosityQuadModelOld.Potapovs_02Q:
                    // пораболическая правая часть
                    CalkVelocityMethd = CalkTaskPotapovs_02;
                    break;
                case TurbulentViscosityQuadModelOld.Potapovs_03:
                case TurbulentViscosityQuadModelOld.Potapovs_03Q:
                    // пораболическая правая часть с вихревой генерацией
                    CalkVelocityMethd = CalkTaskPotapovs_03;
                    break;
                case TurbulentViscosityQuadModelOld.Potapovs_04:
                case TurbulentViscosityQuadModelOld.Potapovs_04Q:
                    // пораболическая правая часть с вихревой генерацией
                    CalkVelocityMethd = CalkTaskPotapovs_04;
                    break;

                case TurbulentViscosityQuadModelOld.SecundovNut92:
                    CalkVelocityMethd = CalkTaskSecundovNut92;
                    break;
                case TurbulentViscosityQuadModelOld.SecundovNut92Q:
                    CalkVelocityMethd = CalkTaskSecundovNut92_Q;
                    break;

                case TurbulentViscosityQuadModelOld.SpalartAllmarasStandard:
                case TurbulentViscosityQuadModelOld.SpalartAllmarasStandardQ:
                    CalkVelocityMethd = CalkTaskSpalartAllmaras;
                    break;
                case TurbulentViscosityQuadModelOld.WrayAgarwal2018:
                case TurbulentViscosityQuadModelOld.WrayAgarwal2018Q:
                    CalkVelocityMethd = CalkTaskWrayAgarwal2018;
                    break;
                // модель турбулентности Ни–Коважного
                case TurbulentViscosityQuadModelOld.Nee_Kovasznay_TaskSolve_0:
                    CalkVelocityMethd = CalkTaskNee_Kovasznay1969;
                    break;

            }

            SetAlgebra();

            taskU = new FVBasePoissonTask(mesh, bc, algebra, mesh.Y_init);
            taskMu = new FVBasePoissonTask(meshMu, bcMu, algebra, meshMu.Y_init);
        }

        protected void SetAlgebra()
        {
            uint N = (uint)mesh.CoordsX.Length;
            int keyAlgebra = 6;
            switch (keyAlgebra)
            {
                case 0:
                    algebra = new AlgebraGauss(N);
                    break;
                case 1:
                    //algebra = new SparseAlgebraGMRES(N, 10, true);
                    algebra = new SparseAlgebraGMRES(N, 30, true);
                    break;
                case 2:
                    algebra = new SparseAlgebraBeCG(N, true);
                    break;
                case 3:
                    algebra = new AlgebraGaussTape(N, (uint)Ny + 5);
                    break;
                case 4:
                    algebra = new AlgebraCholetsky(N);
                    break;
                case 5:
                    algebra = new SparseAlgebraCG(N, true);
                    break;
                case 6:
                    //algebra = new SparseAlgebraGMRES(N, 10, true);
                    algebra = new SparseAlgebraGMRES_P(N, 30, true);
                    break;


            }
        }


        #region IRiver 
        /// <summary>
        /// Получить шероховатость дна
        /// </summary>
        /// <param name="zeta"></param>
        public void GetRoughness(ref double[] Roughness)
        {
            Roughness = null;
        }

        #region перегруженные абстрактные методы

        public override void SolverStep()
        {
            int flagErr = 0;
            try
            {
                if (CalkVelocityMethd == null)
                    InitSerializable();
                CalkVelocityMethd();
                flagErr++;
                //// Расчет напряжений в узлах МЦР                                     
                CalkTau();
                flagErr++;
                //// Расчет гидростатических скоростей
                // CalkHydroStaticVelosity();
                time += dtime;
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
        }
        ///// <summary>
        ///// Расчет полей глубины и скоростей на текущем шаге по времени
        ///// </summary>
        //public void SolverStep()
        //{
        //    int flagErr = 0;
        //    try
        //    {
        //        if (CalkVelocityMethd == null)
        //            InitSerializable();
        //        CalkVelocityMethd();
        //        flagErr++;
        //        //// Расчет напряжений в узлах МЦР                                     
        //        CalkTau();
        //        flagErr++;
        //        //// Расчет гидростатических скоростей
        //        // CalkHydroStaticVelosity();
        //        time += dtime;
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Instance.Exception(ex);
        //    }
        //}


        protected override void InitTask()
        {
            InitTR();
        }
        /// <summary>
        /// Создать расчетную область
        /// </summary>
        /// <param name="right"></param>
        /// <param name="left"></param>
        protected override void CreateCalculationDomain()
        {
            
        }
        /// <summary>
        /// Изменение уравня свободной поверхности
        /// </summary>
        /// <returns></returns>
        /// <summary>
        /// Расчет уровня свободной поверхности реки, 
        /// </summary>
        /// <returns>true - изменение, false - изменений небыло</returns>
        protected override bool CalkWaterLevel()
        {
            double waterLevel0 = waterLevel;
            if (Params.taskVariant == TaskVariant.flowRateFun)
            {
                riverFlowRate = FlowRate.FunctionValue(time);
                if (time > 0)
                {
                    double Ck = 0.1;
                    double Umax = 5;
                    // расчет расхода
                    mesh.GetAreaAndRate(Ux, ref Area, ref riverFlowRateCalk);
                    //riverFlowRateCalk = taskPoisson.SimpleRiverFlowRate(Ux, ref Area);
                    double deltaH = (riverFlowRateCalk - riverFlowRate) / riverFlowRate;
                    double W = bottom_x[bottom_x.Length - 1] - bottom_x[0];
                    double H = riverFlowRateCalk / (W * Umax) * deltaH;
                    double dH = Ck * dtime * H;
                    if (Math.Abs(dH) > 0.005)
                        dH = Math.Sign(dH) * 0.005;
                    waterLevel = waterLevel - dH;
                }
                else
                {
                    riverFlowRateCalk = riverFlowRate;
                }
            }
            else
            {
                waterLevel = WaterLevels.FunctionValue(time);
            }
            if (MEM.Equals(waterLevel0, waterLevel, 0.001) != true)
                return true;
            else
                return false;
        }

        
        protected override void CalkTauBed(ref double[] tauBed){}
        #endregion

        /// <summary>
        /// граничные условия для поля скорости
        /// </summary>
        protected IBoundaryConditions BoundConditionQuadU = null;
        /// <summary>
        /// Получить граничные условия для задачи донных деформаций
        /// </summary>
        /// <returns></returns>
        public virtual IBoundaryConditions BoundCondition() => BoundConditionQuadU;



        /// <summary>
        ///  Сетка для расчета донных деформаций
        /// </summary>
        public override IMesh BedMesh()
        {
            return new TwoMesh(bottom_x, Zeta);
        }
        /// <summary>
        /// КЭ сетка
        /// </summary>
        public override IMesh Mesh() => mesh.Clone();
        /// <summary>
        /// Искомые поля задачи которые позволяют сформировать на сетке расчетной области краевые условия задачи
        /// </summary>
        public IUnknown[] Unknowns() => unknowns;
        protected IUnknown[] unknowns =
        {
            new Unknown("Cкорость потока",null, TypeFunForm.Form_2D_Rectangle_L1)
        };
        public virtual void CalkVelocity()
        {
            try
            {
                SetMuAndRho();
                taskU.PoissonTaskSolve(mMu, sc, ref mU, ref U);
            }
            catch (Exception ep)
            {
                Console.WriteLine("Ошибка " + ep.Message);
            }
        }


        #region Простая дифференциальная модель турбулентной вязкости
        /// <summary>
        /// Экспериментальная модель турбулентной вязкости
        /// </summary>
        //public virtual void CalkTaskPotapovs_00()
        //{
        //    try
        //    {
        //        Area = 0;
        //        Q = 0;
        //        if (start == true)
        //        {
        //            CalkTaskPotapovs_01();
        //            MEM.MemCopy(ref mU0, mU);
        //            MEM.Alloc2D(Nx, Ny, ref nuTilda0);
        //            for (int i = 0; i < Nx; i++)        
        //                for (int j = mesh.Y_init[i]; j < Ny; j++)
        //                    nuTilda0[i][j] = mU0[i][j] / rho_w;
        //            start = false;

        //        }
        //        // Находим источник турбулентности по модели Спаларта – Аллмареса 
        //        for (int index = 0; index < 2; index++)
        //        {
        //            for (int indexMu = 0; indexMu < 3; indexMu++)
        //            {
        //                // Расчет правой части для вычисления вязкости
        //                taskMu.Potapovs_00TaskSolve(nuTilda0, mU, ref nuTilda, ref Mu, alphaQ);
        //                RelaxMu(ref nuTilda, nuTilda0, relax, taskMu.mesh.Y_init);
        //            }
        //            double nu = MEM.Error6;
        //            for (uint i = 0; i < Nx; i++)
        //                for (int j = 0; j < Ny; j++)
        //                {
        //                    double chi = nuTilda[i][j] / nu;
        //                    double chi3 = chi * chi * chi;
        //                    double fw = chi3 / (chi3 + 622.835864);
        //                    mMu[i][j] = rho_w * fw * nuTilda[i][j];
        //                }
        //            // расчет скорости
        //            taskU.PoissonTaskSolve(mMu, sc, ref mU, ref U);
        //            RelaxMu(ref mU, mU0, relax, taskMu.mesh.Y_init);
        //            // расчет расхода
        //            taskU.mesh.GetAreaAndRate(U, ref Area, ref Q);
        //            if (ViscosityModel == TurbulentViscosityQuadModelOld.WrayAgarwal2018)
        //                alphaQ = 1;
        //            else
        //                //alphaQ = Q / Qwater0 / 2 ;
        //                alphaQ = 0.3; // false
        //                              // alphaQ = 0.18; // true
        //        }
        //    }
        //    catch (Exception ep)
        //    {
        //        Console.WriteLine("Ошибка " + ep.Message);
        //    }
        //}
        /// <summary>
        /// Простая дифференциальная модель 1.1 турбулентной вязкости
        /// </summary>
        public virtual void CalkTaskPotapovs_01()
        {
            try
            {
                if (Params.ViscosityModel == TurbulentViscosityQuadModelOld.Potapovs_01)
                {
                    LookAlphaQPotapovs_01(1);
                }
                else
                {
                    alphaQ = DMath.RootBisectionMethod(LookAlphaQPotapovs_01, 0.00001, 50000, MEM.Error3);
                    Console.WriteLine("==========================================");
                    Console.WriteLine("alphaQ = " + alphaQ.ToString("F4"));
                    Console.WriteLine("==========================================");
                }
            }
            catch (Exception ep)
            {
                Console.WriteLine("Ошибка " + ep.Message);
            }
        }
        /// <summary>
        /// Функция вычисления вязкости для заданного расхода, уровня свободной поверхности и геометрии
        /// </summary>
        /// <param name="root_mu">искомый корень для mu = const</param>
        /// <returns></returns>
        public double LookAlphaQPotapovs_01(double alpha)
        {
            alphaQ = alpha;
            double kappa = 0.41;
            double R = mesh.hydraulicRadius;
            //double R = mesh.Ly;
            double u0 = Math.Sqrt(GRAV * R * Params.J);
            double Sn = 2 * rho_w * u0 * kappa / R * alphaQ;

            taskMu.SetValue(ref mMuTilda, 1);
            // расчет вязкости
            taskMu.PoissonTaskSolve(mMuTilda, Sn, ref mMu, ref Mu);
            // расчет скорости
            taskU.PoissonTaskSolve(mMu, sc, ref mU, ref U);
            // расчет расхода
            mesh.GetAreaAndRate(U, ref Area, ref Q);
            double errQ = (Q - Params.Qwater0) / Params.Qwater0;
            return errQ;
        }
        public virtual void CalkTaskPotapovs_02()
        {
            try
            {
                if (Params.ViscosityModel == TurbulentViscosityQuadModelOld.Potapovs_02)
                {
                    LookAlphaQPotapovs_02(1);
                }
                else
                {
                    // вычисление приведенной вязкости и согласованной скорости
                    alphaQ = DMath.RootBisectionMethod(LookAlphaQPotapovs_02, 0.0000001, 50000, MEM.Error3);
                    Console.WriteLine("==========================================");
                    Console.WriteLine("alphaQ = " + alphaQ.ToString("F4"));
                    Console.WriteLine("==========================================");
                }
            }
            catch (Exception ep)
            {
                Console.WriteLine("Ошибка " + ep.Message);
            }
        }
        /// <summary>
        /// Функция вычисления вязкости для заданного расхода, уровня свободной поверхности и геометрии
        /// </summary>
        /// <param name="root_mu">искомый корень для mu = const</param>
        /// <returns></returns>
        public double LookAlphaQPotapovs_02(double alpha)
        {
            alphaQ = alpha;
            double kappa = 0.41;
            double u0 = Math.Sqrt(GRAV * mesh.hydraulicRadius * Params.J);
            double Sn = 2 * rho_w * u0 * kappa / mesh.hydraulicRadius;
            // расчет вязкости
            taskMu.Potapovs_02TaskSolve(Sn, ref mMu, ref Mu, alphaQ);
            // расчет скорости
            taskU.PoissonTaskSolve(mMu, sc, ref mU, ref U);
            // расчет расхода
            mesh.GetAreaAndRate(U, ref Area, ref Q);
            double errQ = (Q - Params.Qwater0) / Params.Qwater0;
            return errQ;
        }
        public virtual void CalkTaskPotapovs_03()
        {
            try
            {
                LookAlphaQPotapovs_03();
            }
            catch (Exception ep)
            {
                Console.WriteLine("Ошибка " + ep.Message);
            }
        }
        public double LookAlphaQPotapovs_03()
        {
            double u0 = Math.Sqrt(GRAV * mesh.hydraulicRadius * Params.J);
            // расчет вязкости
            taskMu.Potapovs_03TaskSolve(u0, ref mMu, ref Mu);
            // расчет скорости
            taskU.PoissonTaskSolve(mMu, sc, ref mU, ref U);
            // расчет расхода
            mesh.GetAreaAndRate(U, ref Area, ref Q);
            double errQ = (Q - Params.Qwater0) / Params.Qwater0;
            return errQ;
        }
        public virtual void CalkTaskPotapovs_04()
        {
            try
            {
                if (Params.ViscosityModel == TurbulentViscosityQuadModelOld.Potapovs_03)
                {
                    LookAlphaQPotapovs_04(1);
                }
                else
                {
                    // вычисление приведенной вязкости и согласованной скорости
                    alphaQ = DMath.RootBisectionMethod(LookAlphaQPotapovs_04, MEM.Error12, 50000, MEM.Error3);
                    Console.WriteLine("==========================================");
                    Console.WriteLine("alphaQ = " + alphaQ.ToString("F4"));
                    Console.WriteLine("==========================================");
                }
            }
            catch (Exception ep)
            {
                Console.WriteLine("Ошибка " + ep.Message);
            }
        }
        public double LookAlphaQPotapovs_04(double alpha)
        {
            alphaQ = alpha;

            double u0 = Math.Sqrt(GRAV * mesh.hydraulicRadius * Params.J);
            double Q = 2 * u0 * kappa;
            // расчет вязкости
            taskMu.Potapovs_04TaskSolve(Q, nuTilda0, ref mMu, ref Mu, alphaQ);
            // расчет скорости
            taskU.PoissonTaskSolve(mMu, sc, ref mU, ref U);
            // расчет расхода
            mesh.GetAreaAndRate(U, ref Area, ref Q);
            double errQ = (Q - Params.Qwater0) / Params.Qwater0;
            return errQ;
        }
        /// <summary>
        /// Простая дифференциальная модель 1.2
        /// </summary>
        //public virtual void CalkTaskPotapovs_01()
        //{
        //    try
        //    {
        //        MEM.MemSet(ref mMu, 1);
        //        // расчет вязкости
        //        taskMu.PoissonTaskSolve(mMu, alphaQ * sc, ref mMu, ref Mu);
        //        // расчет скорости
        //        taskU.PoissonTaskSolve(mMu, sc, ref mU, ref U);
        //        // расчет расхода
        //        taskU.mesh.GetAreaAndRate(U, ref Area, ref Q);

        //        if (d80 * d50 * d10 > 0)
        //            alphaQ = 2 * Q / Qwater0;
        //        else
        //            alphaQ = Q / Qwater0;

        //        //bool flag = false;
        //        bool flag = true;
        //        if (flag == true)
        //        {
        //            for (uint i = 0; i < Nx; i++)
        //                for (int j = 0; j < Ny; j++)
        //                    mMuTilda[i][j] = mMu[i][j] + Mu_w;
        //            // расчет вязкости
        //            taskMu.PoissonTaskSolve(mMuTilda, alphaQ * sc, ref mMu, ref Mu);
        //        }
        //        else
        //        {
        //            taskMu.SetValue(ref mMu, 1);
        //            // расчет вязкости
        //            taskMu.PoissonTaskSolve(mMu, alphaQ * sc, ref mMu, ref Mu);
        //        }
        //        // расчет скорости
        //        taskU.PoissonTaskSolve(mMu, sc, ref mU, ref U);
        //        // расчет расхода
        //        //mesh.GetAreaAndRate(U, ref Area, ref Q);

        //    }
        //    catch (Exception ep)
        //    {
        //        Console.WriteLine("Ошибка " + ep.Message);
        //    }
        //}
        #endregion

        #region Модель турбулентной вязкости Спаларта-Аллмараса
        bool start = true;
        double[][] nuTilda0, nuTilda;
        /// <summary>
        /// Модель турбулентности Спаларта-Аллмараса
        /// </summary>
        public virtual void CalkTaskSpalartAllmaras()
        {
            try
            {
                Area = 0;
                Q = 0;
                if (start == true)
                {
                    //CalkTaskPotapovs_01();
                    LookAlphaQPotapovs_01(1);
                    MEM.MemCopy(ref mU0, mU);
                    alphaQ = 1;
                    start = false;
                    MEM.Alloc2D(Nx, Ny, ref nuTilda0);
                    for (int i = 0; i < Nx; i++)
                        for (int j = mesh.Y_init[i]; j < Ny; j++)
                            nuTilda0[i][j] = mU0[i][j] / rho_w;
                }

                // Находим источник турбулентности по модели Спаларта – Аллмареса 
                for (int index = 0; index < 1; index++)
                {
                    for (int indexMu = 0; indexMu < 1; indexMu++)
                    {
                        // Расчет правой части для вычисления вязкости
                        taskMu.SpalartAllmarasTaskSolve(nuTilda0, mU, ref nuTilda, ref Mu,
                            ref dU_dx, ref dU_dy, ref dMu_dx, ref dMu_dy, alphaQ);

                        RelaxMu(ref nuTilda, nuTilda0, relax, taskMu.mesh.Y_init);
                        //LOG.Print("mMuTilda", mMuTilda, 3);
                    }
                    double nu = MEM.Error6;
                    for (uint i = 0; i < Nx; i++)
                        for (int j = 0; j < Ny; j++)
                        {
                            double chi = nuTilda[i][j] / nu;
                            double chi3 = chi * chi * chi;
                            double fw = chi3 / (chi3 + 357.911);
                            mMu[i][j] = rho_w * fw * nuTilda[i][j];
                        }
                    // расчет скорости
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine(" ==== начат рачет скорости ====");
                    taskU.PoissonTaskSolve(mMu, sc, ref mU, ref U);

                    RelaxMu(ref mU, mU0, relax, taskMu.mesh.Y_init);
                    Console.WriteLine(" ==== завершон рачет скорости ====");
                    Console.ForegroundColor = ConsoleColor.White;
                    if (Params.ViscosityModel == TurbulentViscosityQuadModelOld.SpalartAllmarasStandard)
                    {
                        alphaQ = 1;
                    }
                    else
                    {
                        taskU.mesh.GetAreaAndRate(U, ref Area, ref Q);
                        alphaQ = Q / Params.Qwater0;
                    }
                }
            }
            catch (Exception ep)
            {
                Console.WriteLine("Ошибка " + ep.Message);
            }
        }
        public virtual void CalkTaskWrayAgarwal2018_()
        {
            try
            {
                if (start == true)
                {
                    LookAlphaQPotapovs_01(1);
                    MEM.MemCopy(ref mU0, mU);
                    MEM.Alloc2D(Nx, Ny, ref nuTilda0);
                    for (int i = 0; i < Nx; i++)
                        for (int j = mesh.Y_init[i]; j < Ny; j++)
                            nuTilda0[i][j] = mU0[i][j] / rho_w;
                    start = false;
                }
                // Находим источник турбулентности по модели Спаларта – Аллмареса 
                // Расчет правой части для вычисления вязкости
                taskMu.WrayAgarwal2018TaskSolve(nuTilda0, mU, ref nuTilda, ref Mu,
                    ref dU_dx, ref dU_dy, ref dMu_dx, ref dMu_dy, alphaQ);
                //RelaxMu(ref nuTilda, nuTilda0, relax, taskMu.mesh.Y_init);
                double nu = MEM.Error6;
                for (uint i = 0; i < Nx; i++)
                    for (int j = 0; j < Ny; j++)
                    {
                        double chi = nuTilda[i][j] / nu;
                        double chi3 = chi * chi * chi;
                        double fw = chi3 / (chi3 + 622.835864);
                        mMu[i][j] = rho_w * fw * nuTilda[i][j];
                    }
                // расчет скорости
                taskU.PoissonTaskSolve(mMu, sc, ref mU, ref U);
                // вычисление приведенной вязкости и согласованной скорости
                //alphaQ = DMath.RootBisectionMethod(LookAlphaQ_WrayAgarwal2018, 0.1, 30, MEM.Error3);
                //Console.WriteLine("==========================================");
                //Console.WriteLine("alphaQ = " + alphaQ.ToString("F4"));
                //Console.WriteLine("==========================================");
            }
            catch (Exception ep)
            {
                Console.WriteLine("Ошибка " + ep.Message);
            }
        }
        #endregion 
        #region Модель турбулентности Ни–Коважного
        public virtual void CalkTaskNee_Kovasznay1969()
        {

            try
            {
                if (start == true)
                {
                    LookAlphaQPotapovs_01(1);
                    MEM.MemCopy(ref mU0, mU);
                    MEM.Alloc2D(Nx, Ny, ref nuTilda0);
                    for (int i = 0; i < Nx; i++)
                        for (int j = mesh.Y_init[i]; j < Ny; j++)
                            nuTilda0[i][j] = mU0[i][j] / rho_w;
                    start = false;

                }
                // Находим источник турбулентности по модели Спаларта – Аллмареса 
                for (int index = 0; index < 2; index++)
                {
                    for (int indexMu = 0; indexMu < 3; indexMu++)
                    {
                        // Расчет правой части для вычисления вязкости
                        taskMu.Nee_Kovasznay_TaskSolve(nuTilda0, mU, ref nuTilda, ref Mu);
                        RelaxMu(ref nuTilda, nuTilda0, relax, taskMu.mesh.Y_init);
                    }
                    double nu = MEM.Error6;
                    for (uint i = 0; i < Nx; i++)
                        for (int j = 0; j < Ny; j++)
                        {

                            //double chi = nuTilda[i][j] / nu;
                            //double chi3 = chi * chi * chi;
                            //double fw = chi3 / (chi3 + 622.835864);
                            //mMu[i][j] = rho_w * fw * nuTilda[i][j];
                            mMu[i][j] = rho_w * nuTilda[i][j];
                        }
                    //// расчет скорости
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine(" ==== начат рачет скорости ====");
                    taskU.PoissonTaskSolve(mMu, sc, ref mU, ref U);
                    RelaxMu(ref mU, mU0, relax, taskMu.mesh.Y_init);
                    Console.WriteLine(" ==== завершон рачет скорости ====");
                    Console.ForegroundColor = ConsoleColor.White;

                }
            }
            catch (Exception ep)
            {
                Console.WriteLine("Ошибка " + ep.Message);
            }
        }

        #endregion
        #region Модель турбулентной вязкости Рэя - Агарвала
        public virtual void CalkTaskWrayAgarwal2018()
        {

            try
            {
                if (start == true)
                {
                    LookAlphaQPotapovs_01(1);
                    MEM.MemCopy(ref mU0, mU);
                    MEM.Alloc2D(Nx, Ny, ref nuTilda0);
                    for (int i = 0; i < Nx; i++)
                        for (int j = mesh.Y_init[i]; j < Ny; j++)
                            nuTilda0[i][j] = mU0[i][j] / rho_w;
                    start = false;

                }
                // Находим источник турбулентности по модели 
                {
                    for (int indexMu = 0; indexMu < 3; indexMu++)
                    {
                        // Расчет правой части для вычисления вязкости
                        taskMu.WrayAgarwal2018TaskSolve(nuTilda0, mU, ref nuTilda, ref Mu,
                            ref dU_dx, ref dU_dy, ref dMu_dx, ref dMu_dy, alphaQ);
                        RelaxMu(ref nuTilda, nuTilda0, relax, taskMu.mesh.Y_init);
                    }
                    double nu = MEM.Error6;
                    for (uint i = 0; i < Nx; i++)
                        for (int j = 0; j < Ny; j++)
                        {
                            double chi = nuTilda[i][j] / nu;
                            double chi3 = chi * chi * chi;
                            double fw = chi3 / (chi3 + 622.835864);
                            mMu[i][j] = rho_w * fw * nuTilda[i][j];
                        }
                    //// расчет скорости
                    //taskU.PoissonTaskSolve(mMu, sc, ref mU, ref U);
                    //RelaxMu(ref mU, mU0, relax, taskMu.mesh.Y_init);

                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine(" ==== начат рачет скорости ====");
                    taskU.PoissonTaskSolve(mMu, sc, ref mU, ref U);
                    RelaxMu(ref mU, mU0, relax, taskMu.mesh.Y_init);
                    Console.WriteLine(" ==== завершон рачет скорости ====");
                    Console.ForegroundColor = ConsoleColor.White;


                    //// расчет расхода
                    //taskU.mesh.GetAreaAndRate(U, ref Area, ref Q);
                    //if (ViscosityModel == TurbulentViscosityQuadModelOld.WrayAgarwal2018)
                    //    alphaQ = 1.0;
                    //else
                    //    alphaQ = 0.12; // false
                    //alphaQ = 0.37;// 0.14; // true

                    if (Params.ViscosityModel == TurbulentViscosityQuadModelOld.WrayAgarwal2018)
                    {
                        alphaQ = 1;
                    }
                    else
                    {
                        // расчет расхода
                        taskU.mesh.GetAreaAndRate(U, ref Area, ref Q);
                        alphaQ = Q / Params.Qwater0;
                    }

                }
            }
            catch (Exception ep)
            {
                Console.WriteLine("Ошибка " + ep.Message);
            }

        }
        /// <summary>
        /// Модель Рэя - Агарвала для створа без циркуляционного канала 
        /// </summary>
        public double LookAlphaQ_WrayAgarwal2018(double alpha)
        {
            alphaQ = alpha;
            // Находим источник турбулентности по модели Спаларта – Аллмареса 
            // Расчет правой части для вычисления вязкости
            taskMu.WrayAgarwal2018TaskSolve(nuTilda0, mU, ref nuTilda, ref Mu,
                ref dU_dx, ref dU_dy, ref dMu_dx, ref dMu_dy, alphaQ);
            //RelaxMu(ref nuTilda, nuTilda0, relax, taskMu.mesh.Y_init);
            double nu = MEM.Error6;
            for (uint i = 0; i < Nx; i++)
                for (int j = 0; j < Ny; j++)
                {
                    double chi = nuTilda[i][j] / nu;
                    double chi3 = chi * chi * chi;
                    double fw = chi3 / (chi3 + 622.835864);
                    mMu[i][j] = rho_w * fw * nuTilda[i][j];
                }
            // расчет скорости
            taskU.PoissonTaskSolve(mMu, sc, ref mU, ref U);
            //RelaxMu(ref mU, mU0, relax, taskMu.mesh.Y_init);
            // расчет расхода
            taskU.mesh.GetAreaAndRate(U, ref Area, ref Q);
            double errQ = (Q - Params.Qwater0) / Params.Qwater0;
            return errQ;
        }
        #endregion

        #region Модель турбулентной вязкости Секундова Nut - 92
        public virtual void CalkTaskSecundovNut92()
        {
            try
            {
                Area = 0;
                Q = 0;
                if (start == true)
                {
                    //CalkTaskPotapovs_01();
                    LookAlphaQPotapovs_01(10);
                    MEM.MemCopy(ref mU0, mU);
                    MEM.Alloc2D(Nx, Ny, ref nuTilda0);
                    for (int i = 0; i < Nx; i++)
                        for (int j = mesh.Y_init[i]; j < Ny; j++)
                            nuTilda0[i][j] = mU0[i][j] / rho_w;
                    start = false;
                    alphaQ = 1;
                }
                // Находим источник турбулентности по модели Спаларта – Аллмареса 
                for (int index = 0; index < 2; index++)
                {
                    for (int indexMu = 0; indexMu < 3; indexMu++)
                    {
                        // Расчет правой части для вычисления вязкости
                        taskMu.SecundovNut92TaskSolve(nuTilda0, mU, ref nuTilda, ref Mu, alphaQ);
                        RelaxMu(ref nuTilda, nuTilda0, relax, taskMu.mesh.Y_init);
                    }
                    for (uint i = 0; i < Nx; i++)
                        for (int j = 0; j < Ny; j++)
                        {
                            mMu[i][j] = rho_w * nuTilda[i][j];
                        }
                    // расчет скорости
                    taskU.PoissonTaskSolve(mMu, sc, ref mU, ref U);
                    RelaxMu(ref mU, mU0, relax, taskMu.mesh.Y_init);
                    if (Params.ViscosityModel == TurbulentViscosityQuadModelOld.SecundovNut92)
                    {
                        alphaQ = 1;
                    }
                    else
                    {
                        // расчет расхода
                        taskU.mesh.GetAreaAndRate(U, ref Area, ref Q);
                        alphaQ = Q / Params.Qwater0;
                    }
                }
            }
            catch (Exception ep)
            {
                Console.WriteLine("Ошибка " + ep.Message);
            }
        }
        public virtual void CalkTaskSecundovNut92_Q()
        {
            try
            {
                alphaQ = DMath.RootBisectionMethod(LookAlphaQ_SecundovNut92, 0.1, 10, MEM.Error3);
                Console.WriteLine("==========================================");
                Console.WriteLine("alphaQ = " + alphaQ.ToString("F4"));
                Console.WriteLine("==========================================");
            }
            catch (Exception ep)
            {
                Console.WriteLine("Ошибка " + ep.Message);
            }
        }

        /// <summary>
        /// Модель Рэя - Агарвала для створа без циркуляционного канала 
        /// </summary>
        public double LookAlphaQ_SecundovNut92(double alpha)
        {
            alphaQ = alpha;
            // Находим источник турбулентности по модели Спаларта – Аллмареса 
            // Расчет правой части для вычисления вязкости
            taskMu.SecundovNut92TaskSolve(nuTilda0, mU, ref nuTilda, ref Mu, alphaQ);
            //RelaxMu(ref nuTilda, nuTilda0, relax, taskMu.mesh.Y_init);
            double nu = MEM.Error6;
            for (uint i = 0; i < Nx; i++)
                for (int j = 0; j < Ny; j++)
                    mMu[i][j] = rho_w * nuTilda[i][j];
            // расчет скорости
            taskU.PoissonTaskSolve(mMu, sc, ref mU, ref U);
            //RelaxMu(ref mU, mU0, relax, taskMu.mesh.Y_init);
            // расчет расхода
            taskU.mesh.GetAreaAndRate(U, ref Area, ref Q);
            double errQ = (Q - Params.Qwater0) / Params.Qwater0;
            return errQ;
        }
        #endregion


        /// <summary>
        /// Наложение штрафа на поле
        /// </summary>
        /// <param name="mas"></param>
        /// <param name="weight"></param>
        protected void RelaxMu(ref double[][] mas, double[][] mas0, double relax, int[] Y_indexs)
        {
            for (uint i = 0; i < Nx; i++)
                for (int j = Y_indexs[i]; j < Ny; j++)
                {
                    mas[i][j] = mas[i][j] * relax + (1 - relax) * mas0[i][j];
                    if (mas[i][j] <= 0)
                        mas[i][j] = Mu_w;
                    mas0[i][j] = mas[i][j];
                }
        }
        /// <summary>
        /// Чтение входных данных задачи из файла
        /// геометрия канала по умолчанию
        /// эволюция расходов/уровней
        /// </summary>
        /// <param name="p"></param>
        public bool LoadData(string fileName)
        {
            string message = "Файл данных задачи не обнаружен";
            return WR.LoadParams(LoadData, message, fileName);
        }
        /// <summary>
        /// Чтение данных задачи из файла
        /// </summary>
        /// <param name="file"></param>
        public void LoadData(StreamReader file)
        {
            //// геометрия дна
            //Geometry.Load(file);
            //// свободная поверхность
            //WaterLevels.Load(file);
            //// расход потока
            //flowRate.Load(file);
            // инициализация задачи
            InitTask();
        }
        /// <summary>
        /// Установка объектоа КЭ сетки и алгебры
        /// </summary>
        /// <param name="_mesh"></param>
        /// <param name="algebra"></param>
        public void Set(IMesh mesh, IAlgebra algebra = null)
        {

        }
        /// <summary>
        /// Установка новых отметок дна
        /// </summary>
        /// <param name="zeta"></param>
        public void SetZeta(double[] zeta, EBedErosion bedErosion)
        {
            Erosion = bedErosion;
            if (zeta != null)
                Zeta = zeta;
        }
        /// <summary>
        /// Получить отметки дна
        /// </summary>
        /// <param name="zeta"></param>
        public void GetZeta(ref double[] zeta)
        {
            zeta = Zeta;

        }
        /// <summary>
        /// Формирование данных для отрисовки связанных с сеткой IMesh со сороны задачи
        /// </summary>
        /// <param name="sp">контейнер данных</param>
        public virtual void AddMeshPolesForGraphics(ISavePoint sp)
        {
            mesh.GetValueTo1D(mU, ref U);

            sp.Add("U", U);
            mesh.GetValueTo1D(mMu, ref Mu);
            sp.Add("Mu", Mu);
            mesh.GetValueTo1D(mesh.d_min, ref Mu);
            sp.Add("d_min", Mu);

            mesh.GetValueTo1D(mTauX, ref TauX);
            sp.Add("TauX", TauX);
            mesh.GetValueTo1D(mTauY, ref TauY);
            sp.Add("TauY", TauY);
            // векторное поле на сетке
            sp.Add("Tau", TauX, TauY);


            taskU.GetGrad(mU, ref dU_dx, ref dU_dy);

            mesh.GetValueTo1D(dU_dx, ref TauX);
            sp.Add("dU_dx", TauX);
            mesh.GetValueTo1D(dU_dy, ref TauY);
            sp.Add("dU_dy", TauY);
            sp.Add("|dU|", TauX, TauY);

            taskU.GetGrad(mMu, ref dMu_dx, ref dMu_dy);
            mesh.GetValueTo1D(dMu_dx, ref TauX);
            sp.Add("dMu_dx", TauX);
            mesh.GetValueTo1D(dMu_dy, ref TauY);
            sp.Add("dMu_dy", TauY);
            // векторное поле на сетке
            sp.Add("|dNu|", TauX, TauY);

            GetBTau();

            sp.AddCurve("", mesh.X[0], bNormalTau);
            sp.AddCurve("Касательные придонные напряжения", mesh.X[0], bNormalTau);
            sp.AddCurve("Касательные напряжения гидростатика", mesh.X[0], bNormalTauGS);
            sp.AddCurve("Придонное давление", mesh.X[0], bPress);
            sp.AddCurve("Донных профиль", mesh.X[0], Zeta);

            double[] sMu = null, sTauX = null, sTauY = null, sArg = null, sTauV = null;
            double[] sX = null, sY = null;
            double[] TauW = null; double[] Yplus = null;
            double[] TauW2 = null; double[] Yplus2 = null;

            // Получение напряжений на контуре
            GetTauContur(ref sMu, ref sTauX, ref sTauY, ref sArg, ref sX, ref sY,
                ref TauW, ref Yplus, ref TauW2, ref Yplus2);

            sp.AddCurve("Касательные придонные напряжения по Х", sArg, sTauX);
            sp.AddCurve("Касательные придонные напряжения по Y", sArg, sTauY);
            sp.AddCurve("Донных профиль C", sX, sY);
            MEM.Alloc(sMu.Length, ref sTauV, "sTauV");

            for (int i = 0; i < sTauX.Length; i++)
                sTauV[i] = Math.Sqrt(sTauX[i] * sTauX[i] + sTauY[i] * sTauY[i]);


            sp.AddCurve("Касательные придонные напряжения", sArg, sTauV);
            sp.AddCurve("Донная вязкость", sArg, sMu);

            sp.AddCurve("КН стенки 1", sArg, TauW);
            sp.AddCurve("Игрек плюс 1", sArg, Yplus);
            sp.AddCurve("КН стенки 2", sArg, TauW2);
            sp.AddCurve("Y+ Lutsky Severin", sArg, Yplus2);
        }


        /// <summary>
        /// Получение полей придонных касательных напряжений и давления/свободной поверхности по контексту задачи
        /// усредненных на конечных элементах
        /// </summary>
        /// <param name="tauX">придонное касательное напряжение по х</param>
        /// <param name="tauY">придонное касательное напряжение по у</param>
        /// <param name="P">придонных давления/свободная поверхности потока, по контексту задачи</param>
        public void GetTau(ref double[] tauX, ref double[] tauY, ref double[] P, ref double[][] CS, StressesFlag sf = StressesFlag.Nod)
        {
            MEM.Alloc(mesh.CountKnots, ref tauX);
            MEM.Alloc(mesh.CountKnots, ref P);
            tauY = null;
            GetBTau();
            for (uint j = 0; j < Ny; j++)
            {
                tauX[j] = bNormalTau[j];
                P[j] = bPress[j];
            }
        }

        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public override IRiver Clone()
        {
            return new FDMRiverSectionaChannel(Params);
        }
        /// <summary>
        /// Создает экземпляр класса конвертера
        /// </summary>
        /// <returns></returns>
        public override IOFormater<IRiver> GetFormater()
        {
            return null;
            //return new RiverFormatReaderSection_1YD();
        }
        #endregion

        #region Вспомогательные функции
        /// <summary>
        /// Определяем значения вязкости и плотности в узлах расчетной области, 
        /// исходя из расположения донных отметок   
        /// </summary>
        public virtual void SetMuAndRho() // #3
        {
            waterLevel = mesh.Ly;
            for (int i = 0; i < Zeta.Length; i++)
            {
                Depth[i] = waterLevel - Zeta[i];
                if (Depth[i] > 0)
                    U_star[i] = Math.Sqrt(Params.J * GRAV * Depth[i]);
                else
                    U_star[i] = 0;
            }
            switch (Params.ViscosityModel)
            {
                case TurbulentViscosityQuadModelOld.ViscosityConst:
                    {
                        for (int i = 0; i < Zeta.Length; i++)
                            MuMax[i] = 0;
                        // Mu_w = 0.21185;
                        Mu_w = 0.3732;
                        CalkMuAndRho();
                    }
                    break;
                case TurbulentViscosityQuadModelOld.ViscosityWolfgangRodi:
                    {
                        for (int i = 0; i < Zeta.Length; i++)
                            MuMax[i] = rho_w * Depth[i] * U_star[i] * Rcoef;
                        CalkMuAndRho();
                    }
                    break;
            }
        }

        public void CalkMuAndRho()
        {
            for (uint i = 0; i < Nx; i++)
            {
                double Depth2 = Depth[i] * Depth[i] / 4;
                for (int j = mesh.Y_init[i]; j < Ny; j++)
                {
                    double y = mesh.Y[i][j] - Zeta[i];
                    if (Depth2 > MEM.Error10)
                        mMu[i][j] = (Mu_w + MuMax[i] * ((Depth[i] - y) * y) / Depth2);
                    else
                        mMu[i][j] = Mu_w;
                    mRho[i][j] = rho_w;
                }
            }
        }

        /// <summary>
        /// #9 Выполняем расчет придонных касательных напряжений на e - х интервалах
        /// </summary>
        /// <param name="MainTau"></param>
        public void GetBTau()
        {
            double dy = mesh.dy;
            double dx = mesh.dx;
            for (int j = NxMin; j < Nx; j++)
            {
                //eta = waterLevel;
                double xC = dx * (j + 0.5f);
                double zetaC = 0.5 * (Zeta[j] + Zeta[j - 1]);
                double dzeta = Zeta[j] - Zeta[j - 1];
                if (zetaC + dy / 2 < waterLevel)
                {
                    float sigma = (float)Math.Min(dx, dy);
                    // вектор касательный к дну
                    Vector2 S = new Vector2(dx, dzeta);
                    // вектор нормальный к дну
                    Vector2 Ns = new Vector2(-S.Y, S.X);
                    // вектор - единичная нормаль к дну
                    Vector2 Normals = Vector2.Normalize(Ns);
                    // радиус вектор центра интервала дна
                    Vector2 C = new Vector2(xC, (float)zetaC);
                    // радиус вектор центра придонной точки
                    Vector2 P = C + sigma * Normals;
                    // Скорость в центре донного интервала C
                    double Ucc = mesh.CalkCeilValue(ref C, mU);
                    // Скорость в придонной точке P
                    double Upp = mesh.CalkCeilValue(ref P, mU);
                    // Приведенная вязкость
                    double Mu_p = mesh.CalkCeilValue(ref P, mMu);
                    // Приведенная вязкость
                    double rho_p = mesh.CalkCeilValue(ref P, mMu);
                    // Нормальное придонное касательное напряжение
                    //bNormalTau[j] = Mu_p * (Upp - Ucc) / sigma;
                    bNormalTau[j] = Mu_w * (Upp - Ucc) / sigma;
                    bPress[j] = GRAV * rho_p * (Depth[j] + Depth[j - 1]) / 2;
                    // гидростатическое придонное касательное напряжение
                    bNormalTauGS[j] = bPress[j] * Params.J;
                }
                else
                {
                    bNormalTau[j] = 0;
                    bPress[j] = 0;
                    bNormalTauGS[j] = 0;
                }
            }
        }
        #endregion

    }
}
