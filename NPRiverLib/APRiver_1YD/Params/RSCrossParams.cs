//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 12.12.2020 Потапов И.И.
//---------------------------------------------------------------------------
//               Реверс в NPRiverLib 09.02.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace NPRiverLib.APRiver_1YD.Params
{
    using System;
    using System.IO;
    using System.ComponentModel;

    using MemLogLib;
    using CommonLib;
    using CommonLib.EConverter;

    /// <summary>
    ///  ОО: Параметры для класса RiverStreamTask 
    /// </summary>
    [Serializable]
    public class RSCrossParams : ITProperty<RSCrossParams>
    {
        /// <summary>
        /// уклон свободной поверхности потока
        /// </summary>
        [DisplayName("Уклон свободной поверхности")]
        [Category("Гидрология")]
        public double J { get; set; }
        ///// <summary>
        ///// модель турбулентной вязкости
        ///// </summary>
        //[DisplayName("модель турбулентной вязкости")]
        //[Category("Гидрология")]
        //[TypeConverter(typeof(MyEnumConverter))]
        //public TurbulentViscosityModel ViscosityModel { get; set; }
        /// <summary>
        /// Вариант задачи
        /// </summary>
        [DisplayName("Вариант задачи")]
        [Category("Гидрология")]
        [TypeConverter(typeof(MyEnumConverter))]
        public TaskVariant taskVariant { get; set; }
        /// <summary>
        /// плотность воды
        /// </summary>
        [DisplayName("Ось симметрии 1 стенка 0")]
        [Category("Область расчета")]
        public int AxisSymmetry { get; set; }
        /// <summary>
        /// количество узлов по дну реки
        /// </summary>
        [DisplayName("количество узлов по смоченному периметру")]
        [Category("Сетка")]
        public int CountKnots { get; set; }
        /// <summary>
        /// количество узлов в области
        /// </summary>
        [DisplayName("количество узлов по контуру дна")]
        [Category("Сетка")]
        public int CountBLKnots { get; set; }

        public RSCrossParams()
        {
            this.J = 0.0002;
            this.CountBLKnots = 600;
            this.CountKnots = 400;
            this.AxisSymmetry = 1;
            //this.ViscosityModel = TurbulentViscosityModel.ViscosityBlasius;
            this.taskVariant = TaskVariant.flowRateFun;
        }

        public RSCrossParams(RSCrossParams p)
        {
            Set(p);
        }
        public virtual void Set(RSCrossParams p)
        {
            this.J = p.J;
            this.CountKnots = p.CountKnots;
            this.CountBLKnots = p.CountBLKnots;
           // this.ViscosityModel = p.ViscosityModel;
            this.AxisSymmetry = p.AxisSymmetry;
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
            this.J = LOG.GetDouble(file.ReadLine());
       //     this.ViscosityModel = (TurbulentViscosityModel)LOG.GetInt(file.ReadLine());
            this.AxisSymmetry = LOG.GetInt(file.ReadLine());
            int tv = LOG.GetInt(file.ReadLine());
            this.taskVariant = (TaskVariant)tv;
            this.CountKnots = LOG.GetInt(file.ReadLine());
            this.CountBLKnots = LOG.GetInt(file.ReadLine());
        }
        /// <summary>
        /// Чтение параметров задачи из файла
        /// </summary>
        /// <param name="file"></param>
        public virtual void Save(StreamReader file)
        {
            this.J = LOG.GetDouble(file.ReadLine());
           // this.ViscosityModel = (TurbulentViscosityModel)LOG.GetInt(file.ReadLine());
            this.AxisSymmetry = LOG.GetInt(file.ReadLine());
            int tv = LOG.GetInt(file.ReadLine());
            this.taskVariant = (TaskVariant)tv;
            this.CountKnots = LOG.GetInt(file.ReadLine());
            this.CountBLKnots = LOG.GetInt(file.ReadLine());
        }
        public virtual void LoadParams(string fileName)
        {
            string message = "Файл парамеров задачи - доные деформации - не обнаружен";
            WR.LoadParams(Load, message, fileName);
        }
        public RSCrossParams Clone(RSCrossParams p)
        {
            return new RSCrossParams(p);
        }
        public void SetParams(object p)
        {
            Set((RSCrossParams)p);
        }
    }
}
