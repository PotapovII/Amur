////---------------------------------------------------------------------------
////                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
////                         проектировщик:
////                           Потапов И.И.
////---------------------------------------------------------------------------
////                 кодировка : 02.02.2024 Потапов И.И.
////---------------------------------------------------------------------------
////                      Сбока No Property River Lib
////              с убранными из наследников Property классами 
////---------------------------------------------------------------------------
//namespace NPRiverLib.TViscosityModel
//{
//    using System;
//    using MemLogLib;
//    using CommonLib;
//    using CommonLib.Physics;
//    using MeshGeneratorsLib;
   
//    using NPRiverLib.APRiver_1YD.Params;

//    using System.Linq;
//    using MeshGeneratorsLib.StripGenerator;

//    /// <summary>
//    /// Делегат по расчету алгебраической и дифференциальной турбулентной вязкости
//    /// </summary>
//    /// <param name="eddyViscosity"></param>
//    /// <param name="eddyViscosity0"></param>
//    /// <param name="U"></param>
//    public delegate void GetTurbulentViscosity(ref double[] eddyViscosity, in double[] eddyViscosity0, in double[] U);
//    /// <summary>
//    /// Фабрика алгебраических моделей турбулентности
//    /// </summary>
//    public class ViscosityModelManager
//    {
//        double g = SPhysics.GRAV;
//        double rho_w = SPhysics.rho_w;
//        double kappa = SPhysics.kappa_w;
//        double nu0 = SPhysics.nu;
//        /// <summary>
//        /// КЭ сетка
//        /// </summary>
//        protected IMesh mesh = null;
//        double[] yy = null;
//        double[] zz = null;

//        /// <summary>
//        /// Алгебра для КЭ задачи
//        /// </summary>
//        protected IAlgebra algebra = null;
//        /// <summary>
//        /// Задача Пуассона КЭ реализация
//        /// </summary>
//        protected IFEPoissonTask taskU = null;
//        /// <summary>
//        /// Смоченный периметр
//        /// </summary>
//        protected double WetBed;
//        protected double muConst;
//        protected double waterLevel;
//        protected double roughness;
//        protected RSCrossParams Params;
//        protected IStripMeshGenerator meshGenerator;
        
//        public void Set(double WetBed, double waterLevel, double roughness, RSCrossParams Params, IStripMeshGenerator meshGenerator)
//        {
//            this.WetBed = WetBed;
//            this.waterLevel = waterLevel;
//            this.roughness = roughness;
//            this.Params = Params;
//            this.meshGenerator = meshGenerator;
//        }

//        public ViscosityModelManager(IMesh mesh, IAlgebra algebra, IFEPoissonTask taskU, double muConst)
//        {
//            Set(mesh, algebra, taskU, muConst);
//        }
        
//        public void Set(IMesh mesh, IAlgebra algebra, IFEPoissonTask taskU, double muConst)
//        {
//            this.mesh = mesh;
//            this.muConst = muConst;
//            this.algebra = algebra;
//            this.taskU = taskU;
//            yy = mesh.GetCoords(0);
//            zz = mesh.GetCoords(1);
//        }

//        public void GetModel(TurbulentViscosityModel model,ref GetTurbulentViscosity fun)
//        {
//            switch (model)
//            {
//                case TurbulentViscosityModel.ViscosityBlasius:
//                    fun = ViscosityBlasius;
//                    break;
//                case TurbulentViscosityModel.ViscosityKaraushev:
//                    fun = ViscosityKaraushev;
//                    break;
//                case TurbulentViscosityModel.ViscosityWolfgangRodi:
//                    fun = ViscosityWolfgangRodi;
//                    break;
//                case TurbulentViscosityModel.ViscosityPrandtlKarman:
//                    fun = ViscosityPrandtlKarman;
//                    break;
//                default:
//                    fun = ViscosityWolfgangRodi;
//                    break;
//            }
//        }
//        /// <summary>
//        /// Вычисление 
//        /// </summary>
//        /// <param name="Area">пдощадь живого сечения</param>
//        /// <param name="Q">расход потока</param>
//        /// <param name="uStar">динамическая скорость</param>
//        /// <param name="U">скорость потока в узлах живого сечения</param>
//        protected void GetAreaQ(out double Area, out double Q, out double uStar, in double[] U)
//        {
//            Area = 0;
//            Q = taskU.SimpleRiverFlowRate(U, ref Area);
//            double H0 = Area / WetBed;
//            uStar = Math.Sqrt(g * H0 * Params.J);
//            if (MEM.Equals(Q, 0) == true)
//            {
//                double mU = uStar / SPhysics.kappa_w * Math.Log(H0 / SPhysics.PHYS.d50);
//                Q = mU * Area * 2 / 3;
//            }
//        }


//        /// <summary>
//        /// алгебраических моделей турбулентности Блазиуса
//        /// </summary>
//        /// <param name="eddyViscosity">Вычесленное поле вихревой вязкости</param>
//        /// <param name="eddyViscosity0">Вычесленное поле вихревой вязкости с предыдущего слоя по времени</param>
//        /// <param name="U">поле скорости</param>
//        public void ViscosityBlasius(ref double[] eddyViscosity, in double[] eddyViscosity0, in double[] U)
//        {
//            GetAreaQ(out double Area, out double Q, out double uStar, in U);
//            double m = 23; // к-т Блазиуса (см. Розовский ст. 27)
//            double Aw = rho_w * g * Q / (2 * m); 
//            for (int node = 0; node < mesh.CountKnots; node++)
//            {
//                double zeta = meshGenerator.Zeta(yy[node]);
//                double H = waterLevel - zeta;
//                double z = (zz[node] - zeta);
//                double xi = z / H;
//                if (H <= MEM.Error10 || MEM.Equals(xi, 1))
//                    eddyViscosity[node] = SPhysics.mu;
//                else
//                {
//                    // Шези
//                    double Cs = SPhysics.PHYS.Cs(H);
//                    eddyViscosity[node] = Aw / Cs + SPhysics.mu;
//                }
//            }
//        }
//        /// <summary>
//        /// алгебраических моделей турбулентности по Караущеву
//        /// </summary>
//        /// <param name="eddyViscosity">Вычесленное поле динамической вихревой вязкости</param>
//        /// <param name="eddyViscosity0">динамическая вихревая вязкость с предыдущего слоя по времени</param>
//        /// <param name="U">поле скорости</param>
//        public void ViscosityKaraushev(ref double[] eddyViscosity, in double[] eddyViscosity0, in double[] U)
//        {
//            GetAreaQ(out double Area, out double Q, out double uStar, in U);
//            for (int node = 0; node < mesh.CountKnots; node++)
//            {
//                double zeta = meshGenerator.Zeta(yy[node]);
//                double H = waterLevel - zeta;
//                double z = (zz[node] - zeta);
//                double xi = z / H;
//                if (H <= MEM.Error10 || MEM.Equals(xi, 1))
//                    eddyViscosity[node] = SPhysics.mu;
//                else
//                    eddyViscosity[node] = rho_w * kappa * uStar * (1 - xi) * (roughness + z) + SPhysics.mu;
//            }
//        }
//        /// <summary>
//        /// алгебраических моделей турбулентности по Караущеву
//        /// </summary>
//        /// <param name="eddyViscosity">Вычесленное поле динамической вихревой вязкости</param>
//        /// <param name="eddyViscosity0">динамическая вихревая вязкость с предыдущего слоя по времени</param>
//        /// <param name="U">поле скорости</param>
//        public void ViscosityWolfgangRodi(ref double[] eddyViscosity, in double[] eddyViscosity0, in double[] U)
//        {
//            GetAreaQ(out double Area, out double Q, out double uStar, in U);
//            double H0 = Area / WetBed;
//            double Re = uStar * H0 / nu0;
//            double Pa = 0.2;
//            for (int node = 0; node < mesh.CountKnots; node++)
//            {
//                double zeta = meshGenerator.Zeta(yy[node]);
//                double H = waterLevel - zeta;
//                double z = zz[node] - zeta;
//                double xi = z / H;
//                double zplus = z * uStar / nu0;
//                if (H <= MEM.Error10 || MEM.Equals(xi, 1))
//                {
//                    eddyViscosity[node] = SPhysics.mu; 
//                }
//                else
//                {
//                    if (zplus <= 0.2 * Re)
//                    {
//                        eddyViscosity[node] = rho_w * kappa * uStar * (1 - xi) * (roughness + z) + SPhysics.mu;
//                    }
//                    if (zplus > 0.2 * Re)
//                    {
//                        eddyViscosity[node] = rho_w * kappa * uStar * (1 - xi) * (roughness + z) * H /
//                        (H + 2 * Math.PI * Math.PI * Pa * (roughness + z) * Math.Sin(Math.PI * xi)) + SPhysics.mu;
//                    }
//                }
//            }
//        }
//        /// <summary>
//        /// алгебраических моделей турбулентности по Караущеву
//        /// </summary>
//        /// <param name="eddyViscosity">Вычесленное поле динамической вихревой вязкости</param>
//        /// <param name="eddyViscosity0">динамическая вихревая вязкость с предыдущего слоя по времени</param>
//        /// <param name="U">поле скорости</param>
//        public void ViscosityPrandtlKarman(ref double[] eddyViscosity, in double[] eddyViscosity0, in double[] U)
//        {
//            GetAreaQ(out double Area, out double Q, out double uStar, in U);
//            for (int node = 0; node < mesh.CountKnots; node++)
//            {
//                double zeta = meshGenerator.Zeta(yy[node]);
//                double H = waterLevel - zeta;
//                double z = zz[node] - zeta;
//                double xi = z / H;
//                // Шези (безразмерный)
//                double Cs = SPhysics.PHYS.Cs(H);
//                if (H <= MEM.Error10)
//                    eddyViscosity[node] = SPhysics.mu;
//                else
//                    eddyViscosity[node] = rho_w * kappa * (1 - xi) * (roughness + z) * Q / Cs + SPhysics.mu;
//            }
//        }

//        protected void GetRadies()
//        {
//        }
//    }
//}
