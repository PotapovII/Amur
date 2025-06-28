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
    using System.ComponentModel;

    using MeshLib;
    using MemLogLib;
    using CommonLib;
    using CommonLib.EConverter;
    using CommonLib.BedLoad;

    /// <summary>
    /// ОО: Параметры задачи используемые при 
    /// расчете донных деформаций
    /// </summary>
    [Serializable]
    public class BedLoadParams1D : ABedLoadParams, ITProperty<BedLoadParams1D>
    {
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
        /// Значение параметров по умолчанию
        /// </summary>
        public BedLoadParams1D() : base()
        {
            BCondIn = new BoundCondition1D(TypeBoundCond.Neumann, 0);
            BCondOut = new BoundCondition1D(TypeBoundCond.Neumann, 0);
        }
        /// <summary>
        /// Конструктор копирования
        /// </summary>
        /// <param name="p"></param>
        public BedLoadParams1D(BedLoadParams1D p)
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
            SetParams((BedLoadParams1D)p);
        }
        public override object GetParams()
        {
            return new BedLoadParams1D(this);
        }
        /// <summary>
        /// Установка параметров
        /// </summary>
        /// <param name="p"></param>
        public override void SetParams(BedLoadParams1D p)
        {
            BCondIn = p.BCondIn;
            BCondOut = p.BCondOut;
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
                TypeBoundCond ta;
                double va;
                ta = (TypeBoundCond)LOG.GetInt(file.ReadLine());
                va = LOG.GetDouble(file.ReadLine());
                BCondIn = new BoundCondition1D(ta, va);
                ta = (TypeBoundCond)LOG.GetInt(file.ReadLine());
                va = LOG.GetDouble(file.ReadLine());
                BCondOut = new BoundCondition1D(ta, va);
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
        public override BedLoadParams1D Clone(BedLoadParams1D p)
        {
            return new BedLoadParams1D(p);
        }
    }

}
