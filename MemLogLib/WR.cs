//---------------------------------------------------------------------------
//                 Реализация библиотеки для моделирования 
//                  гидродинамических и русловых процессов
//---------------------------------------------------------------------------
//                    - (C) Copyright 2020
//                      ALL RIGHT RESERVED
//                       проектировщик:
//                         Потапов И.И.
//                           12.07.21
//---------------------------------------------------------------------------
namespace MemLogLib
{
    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

    using MemLogLib.Delegate;
    /// <summary>
    /// IO методы
    /// </summary>
    public static class WR
    {
        public static string format1 = "F1";
        public static string format2 = "F2";
        public static string format3 = "F3";
        public static string format4 = "F4";
        public static string format5 = "F5";
        public static string format6 = "F6";
        public static string format7 = "F7";
        public static string format8 = "F8";

        public static string path = @"Data\";

        public static double FormatCut(double v,string format = "F6")
        {
            string str = v.ToString(format);
            return double.Parse(str,MEM.formatter);
        }

        public static bool LoadParams(MLoadParams Load, string message, string fileName)
        {
            ILogger<IMessageItem> logger = Logger.Instance;
            if (fileName=="")
            {
                logger.Info("Имя файла параметров не определено, " +
                                     "используется парметры по умолчанию");
                return false;
            }
            try
            {
                using (StreamReader file = new StreamReader(path + fileName))
                {
                    if (file == null)
                    {
                        Logger.Instance.Info(message);
                        logger.Error("message", "LoadParams() для файла: " + fileName);
                        return false;
                    }
                    else
                    {
                        Load(file);
                        logger.Info("Успешно загружен файл: " + fileName);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Ошибка при загрузки параметров задачи", "LoadParams() для файла: " + fileName);
                logger.Exception(ex);
                return false;
            }
        }
        /// <summary>
        /// Прочитать сериализованный файл задачи и вернуть ее ссылку
        /// </summary>
        /// <param name="filename">Путь к файлу для чтения..</param>
        public static void DeserializeTask<Tp>(string filename, ref Tp cp)
        {
            try
            {
                // создаем объект BinaryFormatter
                BinaryFormatter formatter = new BinaryFormatter();
                using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate))
                {
                    cp = (Tp)formatter.Deserialize(fs);
                }
            }
            catch (Exception ee)
            {
                Logger.Instance.Exception(ee);
            }
        }
        /// <summary>
        /// Прочитать сериализованный файл задачи на диск.
        /// </summary>
        /// <param name="filename">Путь к файлу для сохранения.</param>
        public static void SerializableTask<Tp>(Tp cp, string filename, double time = 0)
        {
            // создаем объект BinaryFormatter
            BinaryFormatter formatter = new BinaryFormatter();
            // получаем поток, куда будем записывать сериализованный объект
            using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate))
            {
                formatter.Serialize(fs, cp);
            }
        }

        /// <summary>
        /// Чтение матриц в формате *.fmsh
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public static double[][] LoadDoubleMatrix(string[] lines, ref int row)
        {
            double[][] m = null;
            int rowstart = row;
            for (; row < lines.Length; row++)
            {
                string line = lines[row];
                if (line.StartsWith("##"))
                    break;
                if (line.Trim() == "")
                    break;
            }
            int rowend = row - 1;
            int cols = lines[rowstart].Split(' ').Length;
            MEM.Alloc2D(rowend - rowstart + 1, cols, ref m);
            
            int col = 0;
            for (int r = rowstart; r <= rowend; r++)
            {
                string line = lines[r];
                var vals = line.Split(' ');
                col = 0;
                foreach (string s in vals)
                {
                    m[r - rowstart][col] = double.Parse(s.Trim(), MEM.formatter);
                    col++;
                }
            }
            return m;
        }
        public static int[][] LoadIntMatrix(string[] lines, ref int row)
        {
            int[][] m = null;
            int rowstart = row;
            for (; row < lines.Length; row++)
            {
                string line = lines[row];
                if (line.StartsWith("##"))
                    break;
                if (line.Trim() == "")
                    break;
            }
            int rowend = row - 1;
            int cols = lines[rowstart].Split(' ').Length;
            MEM.Alloc2D(rowend - rowstart + 1, cols, ref m);

            int col = 0;
            for (int r = rowstart; r <= rowend; r++)
            {
                string line = lines[r];
                var vals = line.Split(' ');
                col = 0;
                foreach (string s in vals)
                {
                    m[r - rowstart][col] = int.Parse(s.Trim());
                    col++;
                }
            }
            return m;
        }

        public static void WriteDoubleMassive(StreamWriter file, double[] mas, string Format = "F6")
        {
            file.Write(mas[0].ToString(Format));
            for (int i = 1; i < mas.Length; i++)
                file.Write(" " + mas[i].ToString(Format));
            file.WriteLine();
        }
        public static void WriteIntMassive(StreamWriter file, int[] mas)
        {
            file.Write(mas[0].ToString());
            for (int i = 1; i < mas.Length; i++)
                file.Write(" " + mas[i].ToString());
            file.WriteLine();
        }
        
        public static int[] DtoInt(double[] mas)
        {
            int[] m = null;
            MEM.Alloc(mas.Length, ref m);
            for (int i = 0; i < mas.Length; i++)
                m[i] = (int)mas[i];
            return m;
        }
        #region Преобразование с обработкой ошибок
        public static bool Parse(string Text,ref int Count)
        {
            try
            {
                Count = int.Parse(Text);
                return true;
            }
            catch(Exception ex)
            {
                Logger.Instance.Exception(ex);
                return false;
            }
        }
        public static bool Parse(string Text, ref double Count)
        {
            try
            {
                Count = double.Parse(Text);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
                return false;
            }
        }
        public static bool Parse(string Text, ref float Count)
        {
            try
            {
                Count = float.Parse(Text);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
                return false;
            }
        }
        public static bool Parse(string Text, ref bool Count)
        {
            try
            {
                Count = bool.Parse(Text);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
                return false;
            }
        }
        #endregion
    }
}
