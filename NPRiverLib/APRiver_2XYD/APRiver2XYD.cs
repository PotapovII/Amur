//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 07.02.2024 Потапов И.И.
//---------------------------------------------------------------------------
//                      Сбока No Property River Lib
//              с убранными из наследников Property классами 
//---------------------------------------------------------------------------
namespace NPRiverLib.APRiver_2XYD
{
    using CommonLib;
    using CommonLib.Physics;
    using GeometryLib;
    using MemLogLib;
    using NPRiverLib.ATask;
    
    using System;
    /// <summary>
    ///         Базовый тип для плановых русловых задач
    ///  ОО: Определение класса  APRiver2XYD - расчет полей глубины расходов, 
    ///         свободной поверхности потока и напряжений в плане потока,
    ///                  приближение мелкой воды
    /// </summary>
    /// <typeparam name="TParam">параметры задачи</typeparam>
    [Serializable]
    public abstract class APRiver2XYD<TParam> : 
        APRiver<TParam> where TParam : class, ITProperty<TParam>
    {
        #region Физические параметры
        /// <summary>
        /// Турбулентная вязкость
        /// </summary>
        protected double eddyViscosityConst = 1.1;
        /// <summary>
        /// кинематическая вязкость воды
        /// </summary>
        protected double mu = SPhysics.mu;
        /// <summary>
        /// коэффициент Кармана
        /// </summary>
        protected double kappa = SPhysics.kappa_w;
        /// <summary>
        /// коэффициент Прандтля для вязкости
        /// </summary>
        protected double rho_w = SPhysics.rho_w;
        /// <summary>
        /// ускорение свободгого падения
        /// </summary>
        protected const double GRAV = SPhysics.GRAV;
        /// <summary>
        /// основание натурального лагорифма
        /// </summary>
        protected const double E = Math.E;
        #endregion


        #region Локальыне данные задачи
        /// <summary>
        /// Глубина
        /// </summary>
        public double[] _h;
        /// <summary>
        /// Расход по х
        /// </summary>
        public double[] _qx;
        /// <summary>
        /// Расход по y
        /// </summary>
        public double[] _qy;
        /// <summary>
        /// Поле напряжений T_x
        /// </summary>
        public double[] _TauX;
        /// <summary>
        /// Поле напряжений T_y
        /// </summary>
        public double[] _TauY;
        /// <summary>
        /// Отметки донной поверхности
        /// </summary>
        public double[] _Zeta;
        /// <summary>
        /// Отметки свободной поверхности
        /// </summary>
        public double[] _Eta;
        /// <summary>
        /// Вихревая вязкость
        /// </summary>
        public double[] _Mu;
        #endregion


        public APRiver2XYD(TParam p) : base(p, TypeTask.streamXY2D)
        {
        }
        /// <summary>
        /// Установка КЭ сетки и решателя задачи
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="algebra">решатель задачи</param>
        public override void Set(IMesh mesh, IAlgebra algebra = null)
        {
            this.mesh = mesh;
            this.algebra = algebra;

            MEM.Alloc(mesh.CountKnots, ref _h);
            MEM.Alloc(mesh.CountKnots, ref _qx);
            MEM.Alloc(mesh.CountKnots, ref _qy);
            MEM.Alloc(mesh.CountKnots, ref _TauX);
            MEM.Alloc(mesh.CountKnots, ref _TauY);

            MEM.Alloc(mesh.CountKnots, ref _Zeta);
            MEM.Alloc(mesh.CountKnots, ref _Eta);
            MEM.Alloc(mesh.CountKnots, ref _Mu);

            unknowns.Add(new Unknown("Глубина потока", _h, TypeFunForm.Form_2D_Triangle_L1));
            unknowns.Add(new Unknown("Расход потока по x", _qx, TypeFunForm.Form_2D_Triangle_L1));
            unknowns.Add(new Unknown("Расход потока по у", _qy, TypeFunForm.Form_2D_Triangle_L1));
            unknowns.Add(new CalkPapams("Расход потока", _qx, _qy, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new CalkPapams("Поле напряжений T_xy", _TauX, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new CalkPapams("Поле напряжений T_xz", _TauY, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new CalkPapams("Поле напряжений", _TauX, _TauY, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new CalkPapams("Отметки дна", _Zeta, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new CalkPapams("Свободная поверхность", _Eta, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new CalkPapams("Вихревая вязкость", _Mu, TypeFunForm.Form_2D_Rectangle_L1));
        }
    }
}
