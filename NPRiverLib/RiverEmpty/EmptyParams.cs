//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 25.07.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace NPRiverLib.RiverEmpty
{
    using CommonLib;
    using MemLogLib;

    using System;
    using System.IO;
    using System.ComponentModel;

    /// <summary>
    ///  ОО: Прокси параметры  
    /// </summary>
    [Serializable]
    public class EmptyParams : ITProperty<EmptyParams>
    {
        public EmptyParams Clone(EmptyParams p)
        {
            return new EmptyParams(p);
        }
        /// количество узлов по дну реки
        /// </summary>
        [DisplayName("количество узлов")]
        [Category("Сетка")]
        public int CountKnots { get; set; }

        public EmptyParams() : base()
        {
            CountKnots = 100;
        }
        public EmptyParams(EmptyParams p)
        {
            SetParams(p);
        }
        /// <summary>
        /// Установка свойств задачи
        /// </summary>
        /// <param name="p"></param>
        public void SetParams(object p)
        {
            EmptyParams pp = p as EmptyParams;
            SetParams(pp);
        }
        public void SetParams(EmptyParams p)
        {
            CountKnots = p.CountKnots;
        }
        public object GetParams()
        {
            return this;
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
        public void Load(StreamReader file)
        {
            this.CountKnots = LOG.GetInt(file.ReadLine());
        }
    }
}
