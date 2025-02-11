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
        [DisplayName("тип задачи 0 - плоская 1 - цилиндрическая")]
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
        public double J { get; set; }
        /// <summary>
        /// Форма канала
        /// </summary>
        public SСhannelForms channelForms { get; set; }
        /// <summary>
        /// Метод определения динамической скорости
        /// </summary>
        public ECalkDynamicSpeed u_start {  get; set; }

        public BEddyViscosityParam()
        {
            this.SigmaTask = 0;
            this.RadiusMin = 1;
            this.NLine = 10;
            this.J = 0.001;
            this.u_start = ECalkDynamicSpeed.u_start_J;
            //  this.channelForms =  SСhannelForms.boxСhannelSection;
            this.channelForms = SСhannelForms.porabolic;
        }
        public BEddyViscosityParam(int NLine, int SigmaTask, double J, double RadiusMin, SСhannelForms channelForms, 
                            ECalkDynamicSpeed u_start = ECalkDynamicSpeed.u_start_J)
        {
            this.NLine = NLine;
            this.SigmaTask = SigmaTask;
            this.RadiusMin = RadiusMin;
            this.J = J;
            this.u_start = u_start;
            this.channelForms = channelForms;
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
