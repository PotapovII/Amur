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
//                   ПРОЕКТ  "Русловые процессы"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 01.09.2021 Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 22.11.2023 Потапов И.И. поддержка списка форматов
//---------------------------------------------------------------------------
namespace CommonLib.IO
{
    using System;
    using System.IO;
    using System.Collections.Generic;

    using MemLogLib;
    /// <summary>
    /// ОО: Интерфейс для ввода / вывода документов.
    /// </summary>
    [Serializable]
    public abstract class ATaskFormat<Task> : IOFormater<Task>
    {
        /// <summary>
        /// Поддержка внешних форматов загрузки
        /// </summary>
        public abstract bool SupportImport { get; }
        /// <summary>
        /// Поддержка внешних форматов сохранения
        /// </summary>
        public abstract bool SupportExport { get; }

        protected int CountLines = 0;
        /// <summary>
        /// Список поддерживаемых расширений
        /// </summary>
        public List<string> Extstring => extString;
        /// <summary>
        /// Список поддерживаемых расширений
        /// </summary>
        protected List<string> extString = new List<string>();
        /// <summary>
        /// Фильтр для openSsveDialog
        /// </summary>
        public string FilterSD { 
            get 
            {
                string filter = "Сохранить в файл: ";
                foreach (var ext in extString)
                    filter += "(*" + ext + ")|*" + ext + "| ";
                filter += " All files (*.*)|*.*";
                return filter;
            } 
        }
        /// <summary>
        /// Фильтр для openSsveDialog
        /// </summary>
        public string FilterLD
        {
            get
            {
                string filter = "Загрузить из файла: ";
                foreach (var ext in extString)
                    filter += "(*" + ext + ")|*" + ext + "| ";
                filter += " All files (*.*)|*.*";
                return filter;
            }
        }

        
        /// <summary>
        /// проверка файла на наличие поддержки по расширению
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public virtual bool IsSupported(string fileName)
        {
            string ext = Path.GetExtension(fileName).ToLower();
            foreach (var format in extString)
            {
                if (format == ext)
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Проверка фала на наличие 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool Exists(string filename)
        {
            bool flag = File.Exists(filename);
            if (flag==false)
            {
                Logger.Instance.Info("Файл " + filename + "не обнаружен");
            }
            return flag;
        }
        /// <summary>
        /// Прочитать файл задачи.
        /// </summary>
        public abstract void Read(string filename, ref Task task, uint testID = 0);
        /// <summary>
        /// Сохраняем задачи на диск.
        /// </summary>
        public abstract void Write(Task task, string filename);

        #region Методы чтения из файла
        /// <summary>
        /// Считать фавйл в массив строк
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        protected string[] GetAllLines(string filename)
        {
            return File.ReadAllLines(filename);
        }
        /// <summary>
        /// считать строку из потока
        /// </summary>
        /// <param name="file">поток</param>
        /// <returns></returns>
        protected string GetLine(StreamReader file)
        {
            return file.ReadLine();
        }
        /// <summary>
        ///  считать целое число из потока после символа =
        /// </summary>
        /// <param name="file"поток</param>
        /// <returns></returns>
        protected int GetInt(StreamReader file)
        {
            string line = file.ReadLine().Trim();
            return GetInt(line);
        }
        protected int GetInt(string line)
        {
            int val = 0;
            string[] sline = line.Split(' ');
            for (int i = 0; i < sline.Length; i++)
            {
                if (sline[i].Trim() == "=")
                {
                    val = int.Parse(sline[i + 1].Trim());
                    break;
                }
            }
            return val;
        }
        /// <summary>
        ///  считать вещественное число из потока после символа =
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        protected double GetDouble(StreamReader file)
        {
            string line = file.ReadLine().Trim();
            return GetDouble(line);
        }

        protected double GetDouble(string line)
        {
            double val = 0;
            string[] sline = line.Split(' ', '\t');
            for (int i = 0; i < sline.Length; i++)
            {
                if (sline[i].Trim() == "=")
                {
                    val = double.Parse(sline[i + 1].Trim(), MEM.formatter);
                    break;
                }
            }
            return val;
        }

        public string[] GetLines(StreamReader file, int size = 8)
        {
            for (; ; CountLines++)
            {
                string line = file.ReadLine();
                if (line == null)
                    return null;
                string[] lines = line.Split(new Char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length >= size)
                {
                    return lines;
                }
            }
        }
        /// <summary>
        /// для узлов cdg формата
        /// </summary>
        /// <param name="file"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public string[] GetNodeLines(StreamReader file, int size = 8)
        {
            for (; ; CountLines++)
            {
                string line = file.ReadLine();
                if (line == null)
                    return null;
                string[] lines = line.Split(new Char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length >= size)
                {
                    return lines;
                }
            }
        }
        /// <summary>
        /// Создает список тестовых задач для загрузчика по умолчанию
        /// </summary>
        /// <returns></returns>
        public virtual List<string> GetTestsName()
        {
            List<string> list = new List<string>();
            list.Add("Основная задача - тестовая");
            return list;
        }
        #endregion

    }
}
