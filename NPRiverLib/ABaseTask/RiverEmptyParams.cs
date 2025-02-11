//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 24.12.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace NPRiverLib.ABaseTask
{
    using System;
    using System.IO;
    using System.ComponentModel;

    using CommonLib;

    /// <summary>
    ///  ОО: Параметры для класса River2D 
    /// </summary>
    [Serializable]
    public class RiverEmptyParams : ITProperty<RiverEmptyParams>
    {
        /// <summary>
        /// Текущее время расчета
        /// </summary>
        [DisplayName("Текущее время расчета")]
        [Category("Алгоритм")]
        public double time { get; set; }
        /// <summary>
        /// Рекомендуемый шаг по времени
        /// </summary>
        [DisplayName("Рекомендуемый шаг по времени")]
        [Category("Алгоритм")]
        public double dtime { get; set; }
        /// <summary>
        /// Theta - параметр шага по премени
        /// </summary>
        [DisplayName("Theta - параметр шага по премени")]
        [Category("Алгоритм")]
        public double theta { get; set; }
        public RiverEmptyParams()
        {
            time = 0;
            dtime = 100;
            theta = 0.5;
        }
        public RiverEmptyParams(RiverEmptyParams p)
        {
            Set(new RiverEmptyParams());
        }
        public void Set(RiverEmptyParams p)
        {
            time = p.time;
            dtime = p.dtime;
            theta = p.theta;
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
            SetParams((RiverEmptyParams)p);
        }
        public virtual void Load(StreamReader file){}
        public virtual void Save(StreamReader file){}
        public RiverEmptyParams Clone(RiverEmptyParams p)
        {
            return new RiverEmptyParams(p);
        }
    }
    /// <summary>
    ///  ОО: Параметры для класса River2D 
    /// </summary>
    [Serializable]
    public class RiverEmptyParamsCircle : RiverEmptyParams, ITProperty<RiverEmptyParamsCircle>
    {
        /// <summary>
        /// Текущее время расчета
        /// </summary>
        [DisplayName("Количество узлов")]
        [Category("Алгоритм")]
        public int CountKnots  { get; set; }
        public RiverEmptyParamsCircle():base()
        {
            CountKnots = 1000;
        }
        public RiverEmptyParamsCircle(RiverEmptyParamsCircle p) : base(p)
        {
            base.Set(p);
            Set(new RiverEmptyParamsCircle());
        }
        public void Set(RiverEmptyParamsCircle p)
        {
            CountKnots = p.CountKnots;
        }
        public RiverEmptyParamsCircle Clone(RiverEmptyParamsCircle p)
        {
            return new RiverEmptyParamsCircle(p);
        }
    }
}
