//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 19.05.2023 Потапов И.И.
//---------------------------------------------------------------------------
namespace RiverLib
{
    using System;
    using System.IO;
    using System.ComponentModel;

    using MemLogLib;
    using CommonLib.EConverter;

    /// <summary>
    ///  ОО: Параметры для класса RiverSectionalQuad 
    /// </summary>
    [Serializable]
    /// <summary>
    ///  ОО: Параметры для класса RiverStreamTask 
    /// </summary>
    public class RiverStreamQuadParams
    {
        /// <summary>
        /// уклон свободной поверхности потока
        /// </summary>
        [DisplayName("Уклон свободной поверхности")]
        [Category("Гидрология")]
        public double J { get; set; }
        /// <summary>
        /// модель турбулентной вязкости
        /// </summary>
        [DisplayName("модель турбулентной вязкости")]
        [Category("Гидрология")]
        [TypeConverter(typeof(MyEnumConverter))]
        public TurbulentViscosityQuadModelOld ViscosityModel { get; set; }
        /// <summary>
        /// Вариант задачи
        /// </summary>
        [DisplayName("Вариант задачи")]
        [Category("Гидрология")]
        [TypeConverter(typeof(MyEnumConverter))]
        public TaskVariant taskVariant { get; set; }
        /// <summary>
        /// Вязкость потока
        /// </summary>
        [DisplayName("Расход потока (м^3/c)")]
        [Category("Физика")]
        public double Qwater0 { get; set; }
        /// <summary>
        /// Вязкость потока
        /// </summary>
        [DisplayName("Расход задан")]
        [Category("Физика")]
        public bool Flag_Q_Mu { get; set; }
        /// <summary>
        /// Количество узлов по длине области
        /// </summary>
        [DisplayName("Узлов по Х")]
        [Category("Расчетная сетка")]
        public int Nx { get; set; }
        /// <summary>
        /// Количество узлов по высоте области
        /// </summary>
        [DisplayName("Узлов по Y")]
        [Category("Расчетная сетка")]
        public int Ny { get; set; }
        /// <summary>
        /// Минимальная толщина дна в долях от максимальной глубины 
        /// </summary>
        [DisplayName("Коэффициент толщины дна")]
        [Description("Коэффициент толщины дна - отношение минимальная толщины дна к максимальной глубине")]
        [Category("Геометрия области")]
        public double KsDepth { get; set; }
        public RiverStreamQuadParams()
        {
            this.Nx = 61;
            this.Ny = 27;
            this.Qwater0 =  0.00459; // Akihiro Tominaga  theta = 44
            this.Flag_Q_Mu = false;
            this.J = 0.001;
            this.KsDepth = 0.25;
            this.ViscosityModel = TurbulentViscosityQuadModelOld.ViscosityConst;
            // this.ViscosityModel = TurbulentViscosityQuadModelOld.ViscosityWolfgangRodi;
            //this.ViscosityModel = TurbulentViscosityQuadModelOld.Viscosity2DXY;
            //this.ViscosityModel = TurbulentViscosityQuadModelOld.SpalartAllmarasStandard;
            //this.ViscosityModel = TurbulentViscosityQuadModelOld.SpalartAllmarasStandardQ;
            this.ViscosityModel = TurbulentViscosityQuadModelOld.WrayAgarwal2018;
            //this.ViscosityModel = TurbulentViscosityQuadModelOld.WrayAgarwal2018Q;
            //this.ViscosityModel = TurbulentViscosityQuadModelOld.ViscositySA0;
            // this.ViscosityModel = TurbulentViscosityQuadModelOld.Potapovs_00;
            // константа +
            // this.ViscosityModel = TurbulentViscosityQuadModelOld.Potapovs_01;
            //this.ViscosityModel = TurbulentViscosityQuadModelOld.Potapovs_01Q;
            //this.ViscosityModel = TurbulentViscosityQuadModelOld.Potapovs_02;
            //  this.ViscosityModel = TurbulentViscosityQuadModelOld.Potapovs_02Q;
            //this.ViscosityModel = TurbulentViscosityQuadModelOld.Potapovs_03;
            //this.ViscosityModel = TurbulentViscosityQuadModelOld.Potapovs_03Q;
            //this.ViscosityModel = TurbulentViscosityQuadModelOld.Potapovs_04Q;
            //   this.ViscosityModel = TurbulentViscosityQuadModelOld.SecundovNut92;
            //this.ViscosityModel = TurbulentViscosityQuadModelOld.SecundovNut92Q;
            //this.ViscosityModel = TurbulentViscosityQuadModelOld.Nee_Kovasznay_TaskSolve_0;
            this.taskVariant = TaskVariant.flowRateFun;
        }

        public RiverStreamQuadParams(RiverStreamQuadParams p)
        {
            Set(p);
        }
        public virtual void Set(RiverStreamQuadParams p)
        {
           // this.Mu_w = p.Mu_w;
            this.Qwater0 = p.Qwater0;
            this.Flag_Q_Mu = p.Flag_Q_Mu;
            this.J = p.J;
            this.Nx = p.Nx;
            this.Ny = p.Ny;
            this.KsDepth = p.KsDepth;
            this.ViscosityModel = p.ViscosityModel;
            this.taskVariant = p.taskVariant;
        }

        /// <summary>
        /// свойств задачи
        /// </summary>
        /// <param name="p"></param>
        public virtual object GetParams() { return this; }
        /// <summary>
        /// Чтение параметров задачи из файла
        /// </summary>
        /// <param name="file"></param>
        public virtual void Load(StreamReader file)
        {
            this.Qwater0 =  LOG.GetDouble(file.ReadLine());
            this.Flag_Q_Mu = LOG.GetBool(file.ReadLine());
            this.J = LOG.GetDouble(file.ReadLine());
            this.KsDepth = LOG.GetDouble(file.ReadLine());
            this.ViscosityModel = (TurbulentViscosityQuadModelOld)LOG.GetInt(file.ReadLine());
            int tv = LOG.GetInt(file.ReadLine());
            this.taskVariant = (TaskVariant)tv;
            this.Nx = LOG.GetInt(file.ReadLine());
            this.Ny = LOG.GetInt(file.ReadLine());
        }
        public virtual void LoadParams(string fileName)
        {
            string message = "Файл парамеров задачи - доные деформации - не обнаружен";
            WR.LoadParams(Load, message, fileName);
        }
    }

}
