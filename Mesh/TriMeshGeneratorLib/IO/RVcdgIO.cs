using System;
using System.IO;
using MemLogLib;
using System.Collections.Generic;

namespace TriMeshGeneratorLib
{
    /// <summary>
    /// чтение строк в cdg
    /// </summary>
    static class RVcdgIO
    {
        /// <summary>
        /// клучевые слова
        /// </summary>

        public static HashSet<string> keyWord = new HashSet<string> 
        { "Ver", "Count", "Boundary", "RVElement", "Node", "ID", "no" };

        public static int CountLines = 0;
        public static string[] GetLines(StreamReader file)
        {
            for (; ; CountLines++)
            {
                string line = file.ReadLine();
                if (line == null)
                    return null;
                string[] lines = line.Split(new char[] { ' ', '\t' }, 
                            StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length == 0) continue;
                if (keyWord.Contains(lines[0]))
                {
                    //    Logger.Instance.Info(line); 
                }
                else
                {
                    //  Logger.Instance.Info(lines.Length.ToString());
                    return lines;
                }
            }
        }
        /// <summary>
        /// поиск в строке
        /// </summary>
        public static int GetInt(string[] lines)
        {
            for (int i = 0; i < lines.Length; i++)
                if (lines[i].Trim() == "=")
                    return i + 1;
            return -1;
        }
        /// <summary>
        /// чтение строки с величиной int
        /// </summary>
        public static int GetInt(StreamReader file)
        {
            int val = 0;
            string line = file.ReadLine().Trim();
            try
            {
                string[] lines = line.Split(' ');
                int i = GetInt(lines);
                return int.Parse(lines[i].Trim());
            }
            catch (Exception e)
            {
                Logger.Instance.Exception(e);
                Logger.Instance.Info(line);
                return val;
            }
        }
        /// <summary>
        /// чтение строки с величиной double 
        /// </summary>
        public static double GetDouble(StreamReader file)
        {
            double val = 0;
            string line = file.ReadLine().Trim();
            try
            {
                string[] lines = line.Split(' ');
                int i = GetInt(lines);
                val = double.Parse(lines[i].Trim(), LOG.formatter);
                return val;
            }
            catch (Exception e)
            {
                Logger.Instance.Exception(e);
                Logger.Instance.Info(line);
                return val;
            }

        }

        public static string[] GetLines(StreamReader file, int size = 8)
        {
            for (; ; CountLines++)
            {
                string line = file.ReadLine();
                if (line == null)
                    return null;
                string[] lines = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length >= size)
                {
                    // Logger.Instance.Info(lines.Length.ToString());
                    return lines;
                }
                //else
                //   Logger.Instance.Info(line);
            }
        }

    }
}
