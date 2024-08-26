//---------------------------------------------------------------------------
//                 Реализация библиотеки для моделирования 
//                  гидродинамических и русловых процессов
//---------------------------------------------------------------------------
//                Модуль BLLib для расчета донных деформаций 
//                 (учет движения только влекомых наносов)
//         по русловой модели Петрова А.Г. и Потапова И.И. от 2014 г.
//                          Потапов И.И. 27.12.19
//---------------------------------------------------------------------------
//                          08.01.24 Потапов И.И.
//                                
//---------------------------------------------------------------------------
namespace BLLib
{
    using System;
    using System.IO;
    using System.ComponentModel;

    using MeshLib;
    using MemLogLib;
    using CommonLib;
    using CommonLib.EConverter;

    /// <summary>
    /// Флаг модели лавинного обрушения
    /// </summary>
    [Serializable]
    public enum AvalancheType
    {
        [Description("Не использовать лавинную модель")]
        NonAvalanche = 0,
        [Description("Балансная лавинная модель на произвольной сетке")]
        AvalancheSimple,
        [Description("Балансная лавинная модель на регулярной сетке")]
        AvalancheSimpleOld,
        [Description("Дифференцальная лавинная модель")]
        AvalancheQuad_2006,
        [Description("Дифференцальная лавинная модель")]
        AvalancheQuad_2021
    }
    /// <summary>
    /// ОО: Параметры задачи используемые при 
    /// расчете донных деформаций
    /// </summary>
    [Serializable]
    public class BedLoadParams
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
        /// Тип граничных условий на входе в область
        /// </summary>
        [DisplayName("Граничных условия на входе")]
        [Category("Граничные условия")]
        public BoundCondition1D BCondIn { get; set; }
        /// <summary>
        /// Модель движения донного матеиала
        /// </summary>
        [DisplayName("Граничных условия на выходе")]
        [Category("Граничные условия")]
        public BoundCondition1D BCondOut { get; set; }
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
        public BedLoadParams()
        {
            isAvalanche = AvalancheType.AvalancheSimple;
            blm = TypeBLModel.BLModel_2021;
            BCondIn = new BoundCondition1D(TypeBoundCond.Neumann, 0);
            BCondOut = new BoundCondition1D(TypeBoundCond.Neumann, 0);
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
        public BedLoadParams(BedLoadParams p)
        {
            SetParams(p);
        }
        /// <summary>
        /// Установка свойств задачи
        /// </summary>
        /// <param name="p"></param>
        public virtual void SetParams(object p)
        {
            SetParams((BedLoadParams)p);
        }
        public virtual object GetParams()
        {
            return (BedLoadParams)this;
        }
        /// <summary>
        /// Установка параметров
        /// </summary>
        /// <param name="p"></param>
        public void SetParams(BedLoadParams p)
        {
            blm = p.blm;
            BCondIn = p.BCondIn;
            BCondOut = p.BCondOut;
            isAvalanche = p.isAvalanche;
            sedimentShow = p.sedimentShow;
            InitBedLoad();
        }
        public void LoadParams(string fileName = "")
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
                TypeBoundCond ta;
                double va;
                ta = (TypeBoundCond)LOG.GetInt(file.ReadLine());
                va = LOG.GetDouble(file.ReadLine());
                BCondIn = new BoundCondition1D(ta, va);
                ta = (TypeBoundCond)LOG.GetInt(file.ReadLine());
                va = LOG.GetDouble(file.ReadLine());
                BCondOut = new BoundCondition1D(ta, va);
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
        public virtual void InitParamsForMesh(int CountKnots, int CountElements) { }
    }
}
