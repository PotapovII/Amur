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
//---------------------------------------------------------------------------
//                   рефакторинг : Потапов И.И.
//                          27.04.25
//---------------------------------------------------------------------------
namespace BedLoadLib
{
    using System;
    using System.IO;
    using CommonLib;
    using MemLogLib;
    /// <summary>
    /// ОО: Параметры задачи используемые при 
    /// расчете донных деформаций
    /// </summary>
    [Serializable]
    public class BedLoadParams2D : BedLoadParams1D, ITProperty<BedLoadParams2D>
    {
        /// <summary>
        /// Значение параметров по умолчанию
        /// </summary>
        public BedLoadParams2D():base(){}

        /// <summary>
        /// Конструктор копирования
        /// </summary>
        /// <param name="p"></param>
        public BedLoadParams2D(BedLoadParams2D p)
        {
            SetParams(p);
        }
        /// <summary>
        /// Установка параметров
        /// </summary>
        /// <param name="p"></param>
        public void SetParams(BedLoadParams2D p)
        {
            base.SetParams(p);
            InitBedLoad();
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
                base .Load(file);
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
        public BedLoadParams2D Clone(BedLoadParams2D p)
        {
            return new BedLoadParams2D(p);
        }
    }
}
