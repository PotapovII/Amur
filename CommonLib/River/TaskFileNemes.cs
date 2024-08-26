#region License
/*
Copyright (c) 2019 - 2024  Potapov Igor Ivanovich, Khabarovsk

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion
//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//               кодировка : 18.09.21 Потапов И.И.
//---------------------------------------------------------------------------
namespace CommonLib
{
    using System;
    using CommonLib.IO;

    /// <summary>
    /// Имена файлов с данными для задачи гидродинамики
    /// Используется в интерфейсе IRiver
    /// </summary>
    [Serializable]
    public class TaskFileNames : ITaskFileNames
    {
        /// <summary>
        /// Наименование файла параметров руслового процесса
        /// </summary>
        public string NameCPParams { get; set; }
        /// <summary>
        /// Наименование файла параметров для расчета донных деформаций
        /// </summary>
        public string NameBLParams { get; set; }
        /// <summary>
        /// Наименование файла параметров для расчета речных потоков
        /// </summary>
        public string NameRSParams { get; set; }
        /// <summary>
        /// Наименование файла параметров для генерации сетки для расчета речных потоков
        /// </summary>
        public string NameMeshParams { get; set; }
        /// <summary>
        /// Наименование файла данных для расчета речных потоков
        /// рельеф дна/расходы/...
        /// </summary>
        public string NameRData { get; set; }
        /// <summary>
        /// расширение задачи
        /// </summary>
        public string NameEXT { get; set; }
        /// <summary>
        /// расширение задачи для импорта
        /// </summary>
        public string NameEXTImport { get; set; }
        /// <summary>
        /// расширение задачи для экспорта
        /// </summary>
        public string NameEXTExport { get; set; }

        public TaskFileNames()
        {
            NameCPParams   = "NameCPParams.txt";
            NameBLParams   = "NameBLParams.txt";
            NameRSParams   = "NameRSParams.txt";
            NameMeshParams = "NameMeshParams.txt";
            NameRData = "NameRData.txt";
            NameEXT = "(*.tsk)|*.tsk|";
            NameEXTImport = "";
            NameEXTImport = "";
        }
        public TaskFileNames(ITaskFileNames p)
        {
            NameCPParams = p.NameCPParams;
            NameBLParams = p.NameBLParams;
            NameRSParams = p.NameRSParams;
            NameMeshParams = p.NameMeshParams;
            NameRData = p.NameRData;
            NameEXT = p.NameEXT;
            NameEXTImport = p.NameEXTImport;
            NameEXTExport = p.NameEXTExport;
        }
        
        public TaskFileNames(IOFormater<IRiver> loader) : this()
        {
            NameEXT = "(*.tsk)|*.tsk|";
            if (loader.SupportExport == true)
                NameEXTImport = loader.FilterLD;
            if (loader.SupportImport == true)
                NameEXTExport = loader.FilterSD;
        }
    }
}
