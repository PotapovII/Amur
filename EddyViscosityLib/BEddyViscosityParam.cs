//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 03.02.2025 Потапов И.И.
//---------------------------------------------------------------------------
namespace EddyViscosityLib
{
    using System;
    using System.ComponentModel;
    using System.IO;

    using CommonLib;
    using CommonLib.Mesh;
    using CommonLib.Physics;

    /// <summary>
    /// Параметры для класса задач вихревой вязкости
    /// </summary>
    [Serializable]
    public class BEddyViscosityParam : ITProperty<BEddyViscosityParam>
    {
        /// <summary>
        /// Максимальное количество итераций по нелинейности
        /// </summary>
        [DisplayName("Количество итераций по нелинейности на текущем шаге по времени")]
        [Category("Алгоритм")]
        public int NLine { get; set; }
        /// <summary>
        /// тип задачи 0 - плоская 1 - цилиндрическая
        /// </summary>
        [DisplayName("тип задачи 0 - плоская 1 - цилиндрическая в 1Y/осесимметричная в 1X")]
        [Category("Задача")]
        public int SigmaTask { get; set; }
        /// <summary>
        /// начальынй радиус закругления канала по выпуклому берегу
        /// </summary>
        [DisplayName("начальынй радиус закругления канала по выпуклому берегу")]
        [Category("Задача")]
        public double RadiusMin { get; set; }
        /// <summary>
        /// Уклон русла в створе
        /// </summary>
        [DisplayName("Уклон свободной поверхности потока")]
        [Category("Задача")]
        public double J { get; set; }
        /// <summary>
        /// Форма канала
        /// </summary>
        [DisplayName("Форма канала")]
        [Category("Задача")]
        public SСhannelForms channelForms { get; set; }
        /// <summary>
        /// Метод определения динамической скорости
        /// </summary>
        [DisplayName("Метод определения динамической скорости")]
        [Category("Задача")]
        public ECalkDynamicSpeed u_start {  get; set; }
        /// <summary>
        /// Постоянная вихревая вязкость
        /// </summary>
        [DisplayName("Метод определения динамической скорости")]
        [Category("Задача")]
        public double mu_const { get; set; }

        public BEddyViscosityParam()
        {
            this.SigmaTask = 0;
            this.RadiusMin = 1;
            this.NLine = 10;
            this.J = 0.001;
            this.u_start = ECalkDynamicSpeed.u_start_J;
            this.channelForms = SСhannelForms.porabolic;
            //this.mu_const = 10;
            //this.mu_const = 1;
            //this.mu_const = 5/16.0;
            //this.mu_const = 0.1;
            this.mu_const = 0.01;
        }
        public BEddyViscosityParam(int NLine, int SigmaTask, double J, 
            double RadiusMin, SСhannelForms channelForms,
            ECalkDynamicSpeed u_start = ECalkDynamicSpeed.u_start_J,
            //double mu_const = 10)
        //double mu_const = 1) 
        //double mu_const = 5 / 16.0)
        //double mu_const = 0.1)
        double mu_const = 0.01)
        {
            this.NLine = NLine;
            this.SigmaTask = SigmaTask;
            this.RadiusMin = RadiusMin;
            this.J = J;
            this.u_start = u_start;
            this.channelForms = channelForms;
            this.mu_const = mu_const;
        }
        public BEddyViscosityParam(BEddyViscosityParam p)
        {
            Set(p);
        }
        public void Set(BEddyViscosityParam p)
        {
            this.NLine = p.NLine;
            this.SigmaTask = p.SigmaTask;
            this.RadiusMin = p.RadiusMin;
            this.J = p.J;
            this.channelForms = p.channelForms;
            this.u_start = p.u_start;
            this.mu_const = p.mu_const;
        }
        /// <summary>
        /// свойств задачи
        /// </summary>
        /// <param name="p"></param>
        public virtual object GetParams() { return this; }
        /// <summary>
        /// Установка свойств задачи
        /// </summary>
        /// <param name="p"></param>
        public virtual void SetParams(object p)
        {
            Set((BEddyViscosityParam)p);
        }
        public virtual void Load(StreamReader file) { }
        public virtual void Save(StreamReader file) { }
        public BEddyViscosityParam Clone(BEddyViscosityParam p)
        {
            return new BEddyViscosityParam(p);
        }
    }
}
