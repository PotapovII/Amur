//---------------------------------------------------------------------------
//                   ПРОЕКТ  "Русловые процессы"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 27.12.2024 Потапов И.И.
//---------------------------------------------------------------------------
//    заглушка для всех задач, не поддерживающих импорт/экспорт
//---------------------------------------------------------------------------
namespace NPRiverLib.IO
{
    using System;
    using System.Collections.Generic;

    using CommonLib;
    using CommonLib.IO;
    using NPRiverLib.ABaseTask;

    [Serializable]
    public class TaskReaderEmptyAll : ATaskFormat<IRiver>
    {
        /// <summary>
        /// Поддержка внешних форматов загрузки
        /// </summary>
        public override bool SupportImport { get; }
        /// <summary>
        /// Поддержка внешних форматов сохранения
        /// </summary>
        public override bool SupportExport { get; }
        public TaskReaderEmptyAll()
        {
            SupportImport = false;
            SupportExport = false;
            extString = new List<string>() { ".txt" };
        }
        /// <summary>
        /// Прочитать файл, содержащий речную задачу из старого River2D формата данных cdg.
        /// </summary>
        /// <param name="filename">Путь к файлу для чтения..</param>
        /// <returns>Экземпляр интерфейса<see cref="IMesh" /> interface.</returns>
        public override void Read(string filename, ref IRiver river, uint testID = 0) 
        {
            if (river as RiverEmpty1XDCircle != null)
            {
                ((RiverEmpty1XDCircle)river).DefaultCalculationDomain();
                return;
            }
            if (river as RiverEmpty1XDTest01 != null)
            {
                ((RiverEmpty1XDTest01)river).DefaultCalculationDomain();
                return;
            }
        }
        /// <summary>
        /// Сохраняем сетку на диск.
        /// </summary>
        /// <param name="mesh">Экземпляр интерфейса<see cref="IMesh" /> interface.</param>
        /// <param name="filename">Путь к файлу для сохранения.</param>
        public override void Write(IRiver river, string filename) { }
    }
}
