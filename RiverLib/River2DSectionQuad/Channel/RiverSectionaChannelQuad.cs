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

    /// <summary>
    ///  ОО: Определение класса RiverSectionaChannelQuad - для расчета поля скорости
    ///  в створе речного потока
    ///  
    /// </summary>
    [Serializable]
    public class RiverSectionaChannelQuad : RiverSectionaChannel, IRiver
    {
        public RiverSectionaChannelQuad(RiverStreamQuadParams p) : base(p)
        {
            Init();
        }
        /// <summary>
        /// Инициализация 
        /// </summary>
        protected override void Init()
        {
            ///// <summary>
            ///// Граничные условия для поля скорости
            ///// </summary>
            TypeBoundCond[] BCType =
            {
                TypeBoundCond.Dirichlet,
                TypeBoundCond.Dirichlet,
                TypeBoundCond.Neumann,
                TypeBoundCond.Dirichlet
            };
            /// <summary>
            /// Граничные условия для поля вязкости
            /// </summary>
            TypeBoundCond[] BCTypeMu =
            {
                TypeBoundCond.Dirichlet,
                TypeBoundCond.Dirichlet,
                //TypeBoundCond.Dirichlet,
                TypeBoundCond.Neumann,
                TypeBoundCond.Dirichlet
            };
            
            this.Qwater0 = 0.01;
            this.J = 0.00115;
            sc = rho_w * J * g;
            int[] Mark = { 0, 1, 2, 3 };

            //double Ly = 0.073;
            //double Lx = 4.11 * 0.073;

            double Ly = 0.14;
            double Lx = 0.3;

            //Nx = 1 * 321;
            //Ny = 1 * 81;
            Nx = 1 * 161;
            Ny = 1 * 41;


            double Xmin = 0;
            double Ymin = 0;
            
            mesh = new ChannelRectMesh(Nx, Ny, Lx, Ly, Xmin, Ymin);
            mesh.CreateMesh(Mark, null, 0);
            meshMu = new ChannelRectMesh(Nx, Ny, Lx, Ly, Xmin, Ymin);
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

            ks = new double[4] { 0.00025, 0.00005, 0.0000, 0.00005 };
            ks = new double[4] { 0.00001, 0.00001, 0.00001, 0.00001 };
            double[] nv = new double[6] { 0, 0, 0, 0, 0, 0 };
            double[] dv = new double[6] { 0, 0, 0, 0, 0, 0 };

            for (int i = 0; i < ks.Length; i++)
                dv[i] = rho_w * kappa * u0 * ks[i];
            
            for (int i = 0; i < Nx; i++)
               ksBottom[i] = ks[0];


            bcMu = new BoundaryConditionsVar(dv, nv, ks);

            InitSerializable();
        }

        #region IRiver
        /// <summary>
        /// Наименование задачи
        /// </summary>
        public override string Name { get => "Расчет потока в прямоугольном створе "; }
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
            return new RiverSectionaChannelQuad(this);
        }
        #endregion 
    }
}
