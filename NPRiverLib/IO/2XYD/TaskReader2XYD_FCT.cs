//---------------------------------------------------------------------------
//                   ПРОЕКТ  "Русловые процессы"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 16.11.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace NPRiverLib.IO
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;

    using MeshLib;
    using MemLogLib;
    using CommonLib;
    using CommonLib.IO;
    using NPRiverLib.APRiver2XYD.River2DFST;

    [Serializable]
    public class TaskReader2XYD_FCT : ATaskFormat<IRiver>
    {
        /// <summary>
        /// Поддержка внешних форматов загрузки
        /// </summary>
        public override bool SupportImport { get; }
        /// <summary>
        /// Поддержка внешних форматов сохранения
        /// </summary>
        public override bool SupportExport { get; }

        const string Ext_mrf = ".mrf";
        public TaskReader2XYD_FCT()
        {
            extString = new List<string>() { Ext_mrf };
            SupportImport = true;
            SupportExport = true;
        }
        /// <summary>
        /// Прочитать файл, содержащий речную задачу из старого River2D формата данных cdg.
        /// </summary>
        /// <param name="filename">Путь к файлу для чтения..</param>
        /// <returns>Экземпляр интерфейса<see cref="IMesh" /> interface.</returns>
        public override void Read(string filename, ref IRiver river, uint testID = 0)
        {
            string FileEXT = Path.GetExtension(filename).ToLower();
            try
            {
                switch (FileEXT)
                {
                    case Ext_mrf:
                        Import_mrf(filename, ref river);
                        return;
                }
                DefaultRead(filename, ref river);
            }
            catch (Exception ex)
            {
                Logger.Instance.Info("Формат файла не корректен: " + ex.Message);
                Logger.Instance.Info(ex.Message);
            }
        }
        public void Import_mrf(string filename, ref IRiver river)
        {
            ARiverSWE_FCT_2XYD river2D = null;
            if (river as RiverSWE_FCT_2XYD != null)
            {
                river2D = river as RiverSWE_FCT_2XYD;
            }
            else
                if (river as GasDynamics_FCT != null)
            {
                river2D = river as GasDynamics_FCT;
            }
            if (river2D == null)
                throw new NotSupportedException("Не возможно загрузить выбранный объект задачи river_1YD == null");
            river2D.DefaultCalculationDomain();
        }
        /// <summary>
        /// Прочитать файл, речной формат данных 1DX для створа
        /// </summary>
        /// <param name="filename">Путь к файлу для чтения..</param>
        /// <returns>Экземпляр интерфейса<see cref="IMesh" /> interface.</returns>
        public void DefaultRead(string filename, ref IRiver river, uint testID = 0)
        {
            ARiverSWE_FCT_2XYD river2D = null;
            if (river as RiverSWE_FCT_2XYD != null)
            {
                river2D = river as RiverSWE_FCT_2XYD;
            }
            else
                if (river as GasDynamics_FCT != null)
            {
                river2D = river as GasDynamics_FCT;
            }
            if (river2D == null)
                throw new NotSupportedException("Не возможно загрузить выбранный объект задачи river_1YD == null");
            river2D.DefaultCalculationDomain();
        }

        /// <summary>
        /// Сохраняем сетку на диск.
        /// </summary>
        /// <param name="mesh">Экземпляр интерфейса<see cref="IMesh" /> interface.</param>
        /// <param name="filename">Путь к файлу для сохранения.</param>
        public override void Write(IRiver river, string filename)
        {
        }
    }
}
