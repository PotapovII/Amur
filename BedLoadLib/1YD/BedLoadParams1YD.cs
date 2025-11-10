//---------------------------------------------------------------------------
//                 Реализация библиотеки для моделирования 
//                  гидродинамических и русловых процессов
//---------------------------------------------------------------------------
//                Модуль BedLoadLib для расчета донных деформаций 
//                 (учет движения только влекомых наносов)
//         по русловой модели Петрова А.Г. и Потапова И.И. от 2014 г.
//                          Потапов И.И. 27.12.19
//---------------------------------------------------------------------------
//                          08.01.24 Потапов И.И.
//                                
//---------------------------------------------------------------------------
namespace BedLoadLib
{
    using System;
    using System.IO;
    using MemLogLib;
    using CommonLib;
    /// <summary>
    /// ОО: Параметры задачи используемые при 
    /// расчете донных деформаций
    /// </summary>
    [Serializable]
    public class BedLoadParams1YD : BedLoadParams1D, ITProperty<BedLoadParams1YD>
    {
        /// <summary>
        /// Значение параметров по умолчанию
        /// </summary>
        public BedLoadParams1YD() : base()
        {
        }
        /// <summary>
        /// Конструктор копирования
        /// </summary>
        /// <param name="p"></param>
        public BedLoadParams1YD(BedLoadParams1YD p)
        {
            SetParams(p);
        }
        public BedLoadParams1YD(BedLoadParams1D p)
        {
            SetParams(p);
        }
        /// <summary>
        /// Установка свойств задачи
        /// </summary>
        /// <param name="p"></param>
        public override void SetParams(object p)
        {
            base.SetParams(p);
            SetParams((BedLoadParams1YD)p);
        }
        public override object GetParams()
        {
            return new BedLoadParams1YD(this);
        }
        /// <summary>
        /// Установка параметров
        /// </summary>
        /// <param name="p"></param>
        public override void SetParams(BedLoadParams1D p)
        {
            BedLoadParams1YD params1YD = (BedLoadParams1YD)p;
            base.SetParams(p);
        }
        public override void LoadParams(string fileName = "")
        {
            string message = "Файл парамеров задачи - доные деформации - не обнаружен";
            WR.LoadParams(Load, message, fileName);
        }
        /// <summary>
        /// Чтение параметров задачи из файла
        /// </summary>
        /// <param name="file"></param>
        public override void Load(StreamReader file)
        {
            try
            {
                base.Load(file);
            }
            catch (Exception ee)
            {
                Logger.Instance.Exception(ee);
                Logger.Instance.Info("Не согласованность версии программы и файла данных");
                Logger.Instance.Info("Использованы параметры по умолчанию");
            }
            // Пересчет зависимых параметров
            InitBedLoad();
        }
        public  BedLoadParams1YD Clone(BedLoadParams1YD p)
        {
            return new BedLoadParams1YD(p);
        }
    }

}
