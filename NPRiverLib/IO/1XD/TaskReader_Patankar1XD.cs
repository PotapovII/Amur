//---------------------------------------------------------------------------
//                   ПРОЕКТ  "Русловые процессы"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 19.06.2024 Потапов И.И.
//                      форматы для канала
//---------------------------------------------------------------------------
namespace NPRiverLib.IO
{
    using System;
    using System.Collections.Generic;

    using CommonLib;
    using CommonLib.IO;
    using NPRiverLib.APRiver_1XD.River2D_FVM_ke;

    [Serializable]
    public class TaskReader_Patankar1XD : ATaskFormat<IRiver>
    {
        /// <summary>
        /// Поддержка внешних форматов загрузки
        /// </summary>
        public override bool SupportImport { get; }
        /// <summary>
        /// Поддержка внешних форматов сохранения
        /// </summary>
        public override bool SupportExport { get; }
        public TaskReader_Patankar1XD()
        {
            SupportImport = false;
            SupportExport = false;
            extString = new List<string>() { ".pfcd" };
        }
        /// <summary>
        /// Прочитать файл, содержащий речную задачу из старого River2D формата данных cdg.
        /// </summary>
        /// <param name="filename">Путь к файлу для чтения..</param>
        /// <returns>Экземпляр интерфейса<see cref="IMesh" /> interface.</returns>
        public override void Read(string filename, ref IRiver river, uint testID = 0)
        {
            CPatankarStream1XD river1D = river as CPatankarStream1XD;
            if (river1D != null)
            {
                river1D.DefaultCalculationDomain(); 
                return;
            }
            river1D = river as CPatankarStream1XD;
            if (river1D != null)
            {
                river1D.DefaultCalculationDomain(); 
                return;
            }
            if (river1D == null)
                throw new NotSupportedException("Не возможно открыть выбранный формат задачи river == null");
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
