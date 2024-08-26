//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 12.12.2020 Потапов И.И.
//---------------------------------------------------------------------------
namespace RiverLib
{
    using MemLogLib;
    using System;
    using System.ComponentModel;
    using System.IO;
    using CommonLib;

    /// <summary>
    ///  ОО: Параметры для класса RiverStreamTask 
    /// </summary>
    [Serializable]
    public class GidroParams : IPropertyTask
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
        public TurbulentViscosityModel ViscosityModel { get; set; }
        /// <summary>
        /// Вариант задачи
        /// </summary>
        [DisplayName("Вариант задачи")]
        [Category("Гидрология")]
        public TaskVariant taskVariant { get; set; }
        /// <summary>
        /// плотность воды
        /// </summary>
        [DisplayName("Ось симметрии 1 стенка 0")]
        [Category("Область расчета")]
        public int AxisSymmetry { get; set; }
        /// <summary>
        /// плотность воды
        /// </summary>
        [DisplayName("Плотность воды (кг/м^3)")]
        [Category("Физика")]
        public double rho_w { get; set; }
        /// <summary>
        /// Вязкость потока
        /// </summary>
        [DisplayName("Вязкость потока (м^2/c)")]
        [Category("Физика")]
        public double Mu { get; set; }
        /// <summary>
        /// ускорение с.п.
        /// </summary>
        [DisplayName("Ускорение с.п. (м/c^2)")]
        [Category("Физика")]
        public double g { get; set; }
        public GidroParams()
        {
            this.Mu = 0.01;
            this.J = 0.0002;
            this.rho_w = 1000;
            this.g = 9.81;
            this.AxisSymmetry = 1;
            this.ViscosityModel = TurbulentViscosityModel.ViscosityConst;
            this.taskVariant = TaskVariant.flowRateFun;
        }
        public GidroParams(IPropertyTask p)
        {
            SetParams(p);
        }
        public GidroParams(GidroParams p)
        {
            Set(p);
        }
        public virtual void Set(GidroParams p)
        {
            this.Mu = p.Mu;
            this.J = p.J;
            this.rho_w = p.rho_w;
            this.g = p.g;
            this.ViscosityModel = p.ViscosityModel;
            this.AxisSymmetry = p.AxisSymmetry;
            this.taskVariant = p.taskVariant;
        }
        
        /// <summary>
        /// Чтение параметров задачи из файла
        /// </summary>
        /// <param name="file"></param>
        public virtual void Load(StreamReader file)
        {
            this.Mu = LOG.GetDouble(file.ReadLine());
            this.J = LOG.GetDouble(file.ReadLine());
            this.rho_w = LOG.GetDouble(file.ReadLine());
            this.ViscosityModel = (TurbulentViscosityModel)LOG.GetInt(file.ReadLine());
            this.AxisSymmetry = LOG.GetInt(file.ReadLine());
            int tv = LOG.GetInt(file.ReadLine());
            this.taskVariant = (TaskVariant)tv;
        }
        #region Методы IPropertyTask
        /// <summary>
        /// Установка свойств задачи
        /// </summary>
        /// <param name="p"></param>
        public virtual void SetParams(object obj)
        {
            GidroParams p = obj as GidroParams;
            if(p!=null)
                Set(p);
            else
                Logger.Instance.Error("ошибка! не корректный тип параметров задачи!","GidroParams.SetParams()");
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
        public virtual void LoadParams(string fileName)
        {
            string message = "Файл парамеров задачи - доные деформации - не обнаружен";
            WR.LoadParams(Load, message, fileName);
        }
        #endregion
    }
}
