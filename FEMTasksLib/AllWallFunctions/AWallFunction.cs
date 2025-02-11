//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//               Реверс в NPRiverLib 20.01.2025 Потапов И.И.
//---------------------------------------------------------------------------
namespace FEMTasksLib.AllWallFunctions
{
    using CommonLib.Physics;
    using System;
    /// <summary>
    /// Функция стенки по Волков К.Н. 2007. Теплофизика и аэромеханика, 2007, том 14. № 1
    /// </summary>
    [Serializable]
    public abstract class AWallFunction : IWallFunction
    {
        /// <summary>
        /// кинематическая вязкость воды
        /// </summary>
        public double nu = SPhysics.nu;
        /// <summary>
        /// кинематическая вязкость воды
        /// </summary>
        public double mu = SPhysics.mu;
        /// <summary>
        /// плотность воды
        /// </summary>
        public double rho = SPhysics.rho_w;
        /// <summary>
        /// параметр Кармана
        /// </summary>
        public double kappa = SPhysics.kappa_w;

        protected double kappa2;
        protected double kappa3;
        protected double bkappa;
        public AWallFunction()
        {
            kappa2 = kappa * kappa;
            kappa3 = kappa2 * kappa;
            bkappa = 1.0 / kappa;
        }

        /// <summary>
        /// Расчет касательного напряжения 
        /// </summary>
        /// <param name="Ux">скорость в ближайшем узле</param>
        /// <param name="y">растояние до узла</param>
        /// <returns>полученное касательное напряжение на стенке</returns>
        public abstract void Tau_wall(ref double y_plus, ref double u_plus, ref double tau, double Ux, double y, double ks = 0);
        public abstract void tau_wall_test();
    }
}
