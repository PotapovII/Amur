//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 15.04.2021 Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 06.01.2024 Потапов И.И.
//                 добавленно перечисление EBedErosion
//---------------------------------------------------------------------------
namespace ChannelProcessLib
{
    using MemLogLib;
    using CommonLib.EConverter;
    //
    using System;
    using System.IO;
    using System.ComponentModel;
    using CommonLib.ChannelProcess;

    /// <summary>
    /// OO: Базовые параметры задачи - русловые процессы
    /// </summary>
    [Serializable]
    public class CProcParams
    {
        /// <summary>
        /// Имя файла для сохранения результатов расчета
        /// </summary>
        [DisplayName("Имя файла для результатов")]
        [Category("Опции")]
        public string saveFileNeme { get; set; }
        /// <summary>
        /// Расчетное время в секундах
        /// </summary>
        [DisplayName("Текущее время в секундах")]
        [Category("Опции")]
        public double time { get; set; }
        /// <summary>
        /// Расчетное время в секундах
        /// </summary>
        [DisplayName("Расчетное время в секундах")]
        [Category("Опции")]
        public double timeMax { get; set; }
        /// <summary>
        /// шаг по времени
        /// </summary>
        [DisplayName("шаг по времени BL [c]")]
        [Category("Дискретизация задачи")]
        public double dtime { get; set; }
        /// <summary>
        /// Расчетное время в секундах
        /// </summary>
        [DisplayName("шаг по времени River[c]")]
        [Category("Дискретизация задачи")]
        public double dtimeRiver { get; set; }
        /// <summary>
        /// счетчик периодов сохранения задачи
        /// </summary>
        public double countRiver;
        /// <summary>
        /// Период сохранения данных
        /// </summary>
        [DisplayName("Период сохранения данных [с]")]
        [Category("Опции")]
        public double TimeSavePeriod 
        { 
            get=> timeSavePeriod; 
            set 
            {
                count = (int)(time / value) + 1;
                timeSavePeriod = value;
            }
        }
        public double timeSavePeriod;
        /// <summary>
        /// Cчетчик сохраненных периодов задачи
        /// </summary>
        [DisplayName("Cчетчик сохраненных периодов задачи")]
        [Category("Опции")]
        public double CountSavePeriod => count;
        /// <summary>
        /// счетчик периодов сохранения задачи
        /// </summary>
        public double count;
        /// <summary>
        /// Сохранять контекст задачи
        /// </summary>
        [TypeConverter(typeof(BooleanTypeConverterSave))]
        [DisplayName("Сохранять контекст задачи")]
        [Category("Опции")]
        public bool flagSave { get; set; }
        /// <summary>
        /// Сохранять контекст задачи
        /// </summary>
        [TypeConverter(typeof(BooleanTypeConverterSave))]
        [DisplayName("Сохранять кадры результатов")]
        [Category("Опции")]
        public bool flagSP { get; set; }
        /// <summary>
        /// Сохранять контекст задачи
        /// </summary>
        [DisplayName("Размыв дна")]
        [Category("Задача")]
        [TypeConverter(typeof(MyEnumConverter))]
        public EBedErosion bedErosion { get; set; }

        public CProcParams()
        {
            this.count = 1;
            this.countRiver =1;
            this.time = 0;
            this.timeMax = 3600;
            this.dtime = 0.1;
            this.dtimeRiver = dtime;
            this.timeSavePeriod = 5;
            this.flagSave = true;
            this.flagSP = true;
            this.bedErosion = EBedErosion.NoBedErosion;
            this.saveFileNeme = "Задача";

        }
        public CProcParams(CProcParams p)
        {
            SetParams(p);
        }
        public virtual void SetParams(CProcParams p)
        {
            this.count = p.count;
            this.countRiver = p.countRiver;
            this.time = p.time;
            this.timeMax = p.timeMax;
            this.dtime = p.dtime;
            this.dtimeRiver = p.dtimeRiver;
            this.timeSavePeriod = p.timeSavePeriod;
            this.saveFileNeme = p.saveFileNeme;
            this.flagSave = p.flagSave;
            this.flagSP = p.flagSP;
            this.bedErosion = p.bedErosion;
        }
        /// <summary>
        /// Установка свойств задачи
        /// </summary>
        /// <param name="p"></param>
        public void SetParams(object p)
        {
            SetParams((CProcParams)p);
        }
        public object GetParams()
        {
            return this;
        }
        public void LoadParams(string fileName)
        {
            string message = "Файл управляющих парамеров для задачи не обнаружен";
            WR.LoadParams(Load, message, fileName);
        }
        public void Load(StreamReader file)
        {
            this.count = 1;
            this.countRiver = 1;
            time = LOG.GetDouble(file.ReadLine());
            timeMax = LOG.GetDouble(file.ReadLine());
            dtime = LOG.GetDouble(file.ReadLine());
            dtimeRiver = LOG.GetDouble(file.ReadLine());
            timeSavePeriod = LOG.GetDouble(file.ReadLine());
            saveFileNeme = LOG.GetString(file.ReadLine());
            flagSave = LOG.GetBool(file.ReadLine());
            flagSP = LOG.GetBool(file.ReadLine());
            bedErosion = (EBedErosion)LOG.GetInt(file.ReadLine());
        }
    }
}
