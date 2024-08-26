//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//---------------------------------------------------------------------------
//                  Расчет речного потока в створе русла
//---------------------------------------------------------------------------
//               кодировка  : 19.05.2023 Потапов И.И.
//---------------------------------------------------------------------------
namespace RiverLib
{
    using System;
    using MemLogLib;
    using CommonLib;
    using MeshLib;
    using System.Collections.Generic;
    using RiverLib.AllWallFunctions;

    /// <summary>
    ///  ОО: Определение класса RiverSectionaChannelTrapez - для расчета поля скорости
    ///  в створе речного потока
    ///  
    /// </summary>
    [Serializable]
    public class RiverSectionaChannelTrapez : RiverSectionaChannel, IRiver
    {
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
        public RiverSectionaChannelTrapez(RiverStreamQuadParams p) : base(p)
        {
            Init();
        }
        /// <summary>
        /// Инициализация 
        /// </summary>
        protected override void Init()
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

            sc = rho_w * J * g;


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
                Nx = 77 + MeshRang * 76; 
                Ny = 14 + MeshRang * 13;
                this.Qwater0 = 0.00459;
            }
            else
            {
                int MeshRang = 20;
                Xmin = 0;
                Ymin = 0;
                m = 1.0;
                Lx = 3;
                Ly = 0.5;
                Nx = 6 * MeshRang + 1;
                Ny = MeshRang + 1;
                this.Qwater0 = 0.67;
            }

            mesh = new ChannelTrapezMesh(Nx, Ny, Lx, Ly, m, Xmin, Ymin);
            mesh.CreateMesh(Mark, null, 0);
            meshMu = new ChannelTrapezMesh(Nx, Ny, Lx, Ly, m, Xmin, Ymin);
            meshMu.CreateMesh(Mark, null, 0);
            dx = mesh.dx;
            dy = mesh.dy;
            Nx = mesh.Nx;
            Ny = mesh.Ny;
            MEM.Alloc(Nx, ref bottom_x);
            MEM.Alloc(Nx, ref ksBottom);

            MEM.Alloc(Nx, ref Zeta);

            bc = new BoundaryConditionsVar(6);

            double DepthMax = meshMu.Ly;
            double u0 = Math.Sqrt(g * J * DepthMax);
            double kappa = 0.41;
            ks = new double[2] { 0.0000001, 0.0000001 };
            double[] nv = new double[2] { 0, 0 };
            double[] dv = new double[2] { 0, 0 };

            for (int i = 0; i < ks.Length; i++)
                dv[i] = rho_w * kappa * u0 * ks[i];
            
            for (int i = 0; i < Nx; i++)
               ksBottom[i] = ks[0];


            bcMu = new BoundaryConditionsVar(dv, nv, ks);

            InitSerializable();
        }


        public override void GetTauContur(ref double[] sMu, ref double[] sTauX, ref double[] sTauY,
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
            double stau=0;
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

        /// <summary>
        /// Расчет напряжений в узлах МЦР
        /// </summary>
        public override void CalkTau()
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


        #region IRiver
        /// <summary>
        /// Наименование задачи
        /// </summary>
        public override string Name { get => "Расчет потока в трапецеидальном створе "; }
        /// <summary>
        /// версия дата последних изменений интерфейса задачи
        /// </summary>
        public new string VersionData() => "River2D 18.07.2023"; 
        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public override IRiver Clone()
        {
            return new RiverSectionaChannelTrapez(this);
        }
        #endregion 
    }
}
