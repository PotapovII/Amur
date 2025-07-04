﻿//---------------------------------------------------------------------------
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
    using System.ComponentModel;

    using MemLogLib;
    using CommonLib.EConverter;
    using CommonLib.BedLoad;

    /// <summary>
    /// ОО: Параметры задачи используемые при 
    /// расчете донных деформаций
    /// </summary>
    [Serializable]
    public class ABedLoadParams 
    {
        /// <summary>
        /// Учет лавинного осыпания дна
        /// </summary>
        [DisplayName("Учет лавинного осыпания дна")]
        [Category("Настройка физической модели")]
        [TypeConverter(typeof(MyEnumConverter))]
        public AvalancheType isAvalanche { get; set; }

        /// <summary>
        /// Модель движения донного матеиала
        /// </summary>
        [DisplayName("Модель движения донного матеиала")]
        [Category("Настройка физической модели")]
        [TypeConverter(typeof(MyEnumConverter))]
        public TypeBLModel blm { get; set; }
        /// <summary>
        /// Сохранять расход наносов
        /// </summary>
        [DisplayName("Сохранять расход наносов")]
        [Description("Сохранять расход наносов")]
        [Category("Управление выводом")]
        [TypeConverter(typeof(BooleanTypeConverterSave))]
        public bool sedimentShow { get; set; }
        /// <summary>
        /// Расход наносов по механизмам движения донного материала
        /// </summary>
        protected const int idxTransit = 0, idxZeta = 1, idxAll = 2, idxPress = 3;
        /// <summary>
        /// Значение параметров по умолчанию
        /// </summary>
        public ABedLoadParams()
        {
            isAvalanche = AvalancheType.AvalancheSimple;
            blm = TypeBLModel.BLModel_2021;
            sedimentShow = false;
        }

        #region Вычисляемые параметры задачи
        /// <summary>
        /// Пересчет зависимых параметров задачи
        /// </summary>
        public virtual void InitBedLoad()
        {
        }
        #endregion
        /// <summary>
        /// Конструктор копирования
        /// </summary>
        /// <param name="p"></param>
        public ABedLoadParams(ABedLoadParams p)
        {
            SetParams(p);
        }
        /// <summary>
        /// Установка свойств задачи
        /// </summary>
        /// <param name="p"></param>
        public virtual void SetParams(object p)
        {
            SetParams((ABedLoadParams)p);
        }
        public virtual object GetParams()
        {
            return (ABedLoadParams)this;
        }
        /// <summary>
        /// Установка параметров
        /// </summary>
        /// <param name="p"></param>
        public virtual void SetParams(BedLoadParams1D p)
        {
            blm = p.blm;
            isAvalanche = p.isAvalanche;
            sedimentShow = p.sedimentShow;
            InitBedLoad();
        }
        public virtual void LoadParams(string fileName = "")
        {
            string message = "Файл парамеров задачи - доные деформации - не обнаружен";
            WR.LoadParams(Load, message, fileName);
        }
        /// <summary>
        /// Чтение параметров задачи из файла
        /// </summary>
        /// <param name="file"></param>
        public virtual void Load(StreamReader file)
        {
            try
            {
                isAvalanche = (AvalancheType)LOG.GetInt(file.ReadLine());
                blm = (TypeBLModel)LOG.GetInt(file.ReadLine());
                sedimentShow = LOG.GetBool(file.ReadLine());
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
        public virtual BedLoadParams1D Clone(BedLoadParams1D p)
        {
            return new BedLoadParams1D(p);
        }
        public virtual void InitParamsForMesh(int CountKnots, int CountElements) { }
    }
}
