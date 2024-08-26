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
    /// <summary>
    /// Имена файлов с данными для задачи гидродинамики
    /// Используется в интерфейсе IRiver
    /// </summary>
    public interface ITaskFileNames
    {
        /// <summary>
        /// Наименование файла параметров руслового процесса
        /// </summary>
        string NameCPParams { get; set; }
        /// <summary>
        /// Наименование файла параметров для расчета донных деформаций
        /// </summary>
        string NameBLParams { get; set; }
        /// <summary>
        /// Наименование файла параметров для расчета речных потоков
        /// </summary>
        string NameRSParams { get; set; }
        /// <summary>
        /// Наименование файла параметров для генерации сетки для расчета речных потоков
        /// </summary>
        string NameMeshParams { get; set; }
        /// <summary>
        /// Наименование файла данных для расчета речных потоков
        /// рельеф дна/расходы/...
        /// </summary>
        string NameRData { get; set; }
        /// <summary>
        /// расширение задачи
        /// </summary>
        string NameEXT { get; set;  }
        /// <summary>
        /// расширение задачи для импорта
        /// </summary>
        string NameEXTImport { get; set; }
        /// <summary>
        /// расширение задачи для экспорта
        /// </summary>
        string NameEXTExport { get; set; }
    }
}
