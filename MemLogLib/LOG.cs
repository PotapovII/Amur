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
    using System.Globalization;
    using System.IO;
    /// <summary>
    /// ОО: Класс логгер
    /// </summary>
    public static class LOG
    {
        /// <summary>
        /// Флаг направления лога (консоль / файл)
        /// </summary>
        public static int flagLogger = 0;
        /// <summary>
        /// Разделитель значимых частей строки
        /// </summary>
        public static char[] Spliter = { ' ', '\t' };
        /// <summary>
        /// Имя файла лога 
        /// </summary>
        public static string FileLogger = "FileLogger.txt";
        /// <summary>
        /// Разделитель вещественного числа по умолчанию
        /// </summary>
        public static NumberFormatInfo formatter = new NumberFormatInfo { NumberDecimalSeparator = "." };

        #region Методы чтения и разбота строки
        /// <summary>
        /// Строка => Имя +Spliter+ знакчение +Spliter+ комментарий
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static double Double(string line)
        {   try
            {
                return double.Parse(line, formatter);
            }
            catch(Exception ex)
            {
                Logger.Instance.Info(ex.Message);
                return 0;
            }
        }
        /// <summary>
        /// Чтение второго слова строки (первое слово комментарий)
        /// </summary>
        /// <param name="line">строка</param>
        /// <returns></returns>
        public static int Int(string line)
        {
            return int.Parse(line);
        }
        /// <summary>
        /// Строка => Имя +Spliter+ знакчение +Spliter+ комментарий
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static double GetDouble(string line)
        {
            string[] sline = line.Split(Spliter);
            return double.Parse(sline[1].Trim(), formatter);
        }
        /// <summary>
        /// Чтение второго слова строки (первое слово комментарий)
        /// </summary>
        /// <param name="line">строка</param>
        /// <returns></returns>
        public static int GetInt(string line)
        {
            string[] sline = line.Split(Spliter);
            return int.Parse(sline[1].Trim());
        }
        /// <summary>
        /// Чтение второго слова строки (первое слово комментарий)
        /// </summary>
        /// <param name="line">строка</param>
        /// <returns></returns>
        public static bool GetBool(string line)
        {
            string[] sline = line.Split(Spliter);
            int f = int.Parse(sline[1].Trim());
            return (f == 1) ? true : false;
        }
        /// <summary>
        /// Чтение второго слова строки (первое слово комментарий)
        /// </summary>
        /// <param name="line">строка</param>
        /// <returns></returns>
        public static string GetString(string line)
        {
            string[] sline = line.Split(Spliter);
            return sline[1].Trim();
        }
        #endregion
        /// <summary>
        /// добавление строки в файл
        /// </summary>
        /// <param name="message"></param>
        public static void sendToFile(string message)
        {
            using (StreamWriter writer = new StreamWriter(FileLogger, true))
            {
                writer.WriteLine(message);
                writer.Close();
            }
        }
        /// <summary>
        /// Очистка консоли
        /// </summary>
        /// <param name="mes"></param>
        public static void Clear()
        {
            Console.Clear();
        }
        #region Выравниватели строк в текстовых файлах
        /// <summary>
        /// Выравниватели строк в текстовых файлах с отрицатальными значениями
        /// </summary>
        /// <param name="a"></param>
        /// <param name="F"></param>
        /// <returns></returns>
        public static string DoubleToString(double a, string F)
        {
            string s = a.ToString(F);
            if (a > 0)
                s = " " + s;
            else
                if ( MEM.Equals( a, 0 , MEM.Error10) == true)
                    s = " " + s;
            return s;
        }
        public static string IntToString(int a)
        {
            string s = a.ToString();
            if (a >= 0)
                s = " " + s;
            return s;
        }
      
        /// <summary>
        /// чтение выровненных строк
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static string[] Split(string line)
        {
            line = line.Trim('(', ')', '\t');
            string[] slines = line.Split(' ', '\t');
            int k = 0;
            for (int i = 0; i < slines.Length; i++)
                if (slines[i] != "")
                    slines[k++] = slines[i];
            return slines;
        }
        #endregion
        public static void TPrint<MyType>(string Name, MyType value) where MyType : struct
        {
            string s = String.Format(Name + " = " + value.ToString());
            if (flagLogger == 0)
                Console.WriteLine(s);
            else
                sendToFile(s);
        }
        public static void Print(string Name, int value) 
        {
            string s = String.Format(Name + " = " + value.ToString());
            if (flagLogger == 0)
                Console.WriteLine(s);
            else
                sendToFile(s);
        }

        public static void Print(string Name, double value)
        {
            string s = String.Format(Name + " = " + value.ToString());
            if (flagLogger == 0)
                Console.WriteLine(s);
            else
                sendToFile(s);
        }
        public static void Print(string Name, float value)
        {
            string s = String.Format(Name + " = " + value.ToString());
            if (flagLogger == 0)
                Console.WriteLine(s);
            else
                sendToFile(s);
        }
        public static void Print(string Name, bool value)
        {
            string s = String.Format(Name + " = " + value.ToString());
            if (flagLogger == 0)
                Console.WriteLine(s);
            else
                sendToFile(s);
        }
        public static void Print(string Name, string value)
        {
            string s = String.Format(Name + " = " + value);
            if (flagLogger == 0)
                Console.WriteLine(s);
            else
                sendToFile(s);
        }
        /// <summary>
        /// Тестовая печать поля
        /// </summary>
        /// <param name="Name">имя поля</param>
        /// <param name="mas">массив пля</param>
        /// <param name="FP">точность печати</param>
        public static void PrintMas(string Name, double[] mas, int FP = 8)
        {
            string Format = " {F6}";
            if (FP != 6)
                Format = " {F" + FP.ToString() + "}";
            if (flagLogger == 0)
            {
                Console.WriteLine(Name);
                for (int i = 0; i < mas.Length; i++)
                    Console.Write(DoubleToString(mas[i], Format)+" ");
                Console.WriteLine();
            }
            else
            {
                using (StreamWriter writer = new StreamWriter(FileLogger, true))
                {
                    writer.WriteLine(Name);
                    for (int i = 0; i < mas.Length; i++)
                        writer.Write(DoubleToString(mas[i], Format) + " ");
                    writer.WriteLine();
                    writer.Close();
                }
            }
        }
        /// <summary>
        /// ф-я контрольной печати одномерного массива (си 1D формат)
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="di"></param>
        /// <param name="dm"></param>
        /// <param name="j"></param>
        public static void Print(string Name, int[] di, int dm, int j)
        { 
            int i, k; i = k = 0;
            Console.WriteLine(Name);
            for (i = 0; i < dm; i++)
            {
                Console.Write(" {0}", di[i]); 
                k++;
                if (k > j - 1) 
                { 
                    k = 0;
                    Console.WriteLine();
                }
            }
        }
        /// <summary>
        /// ф-я контрольной печати одномерного массива (си 1D формат)
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="di"></param>
        /// <param name="dm"></param>
        /// <param name="j"></param>
        public static void Print(string Name, float[] di, int dm, int j)
        {
            int i, k; i = k = 0;
            Console.WriteLine(Name);
            for (i = 0; i < dm; i++)
            {
                Console.Write(" {0}", di[i]);
                k++;
                if (k > j - 1)
                {
                    k = 0;
                    Console.WriteLine();
                }
            }
        }
        /// <summary>
        /// Печать ЛМЖ для отладки
        /// </summary>
        /// <param name="M"></param>
        /// <param name="Count"></param>
        /// <param name="F"></param>
        public static void Print(string Name, int nx, int ny, double[] M, int F = 2)
        {
            string Format = "F" + F.ToString();
            int k = 0;
            if (flagLogger == 0)
            {
                Console.WriteLine(Name);

                for (int i = 0; i < nx; i++)
                {
                    Console.Write("{0} ", i);
                    for (int j = 0; j < ny; j++)
                        Console.Write(DoubleToString(M[k++], Format) + " ");
                    Console.WriteLine();
                }
            }
            else
            {
                using (StreamWriter writer = new StreamWriter(FileLogger, true))
                {
                    writer.WriteLine(Name);
                    for (int i = 0; i < nx; i++)
                    {
                        writer.Write("{0} ", i);
                        for (int j = 0; j < ny; j++)
                            writer.Write(DoubleToString(M[k++], Format) + " ");
                        writer.WriteLine();
                    }
                    writer.Close();
                }
            }
        }
        /// <summary>
        /// Печать ЛМЖ для отладки
        /// </summary>
        /// <param name="M"></param>
        /// <param name="Count"></param>
        /// <param name="F"></param>
        public static void Print(string Name, double[][] M, int F = 3, int color = 0)
        {
            string Format = "F" + F.ToString();
            if (flagLogger == 0)
            {
                //Console.Clear();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine();
                Console.WriteLine(Name);
                Console.ForegroundColor = ConsoleColor.White;
                if (color == 0)
                {
                    for (int i = 0; i < M.Length; i++)
                    {
                        Console.Write("{0} ", i);
                        for (int j = 0; j < M[i].Length; j++)
                            Console.Write(DoubleToString(M[i][j], Format) + " ");
                        Console.WriteLine();
                    }
                }
                else
                {
                    bool r = false;
                    bool c = false;
                    for (int i = 0; i < M.Length; i++)
                    {
                        if (i % color == 0) r = !r;
                        int cl = (int)Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("{0} ", i);
                        Console.ForegroundColor = (ConsoleColor)cl;
                        for (int j = 0; j < M[i].Length; j++)
                        {
                            if (j % color == 0) c = !c;
                            if (color > 0 && MEM.Equals(M[i][j],0) == true)
                            {
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            else
                            {
                                if (MEM.Equals(M[i][j], 1) == true)
                                {
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                }
                                else
                                {
                                    if (c == r)
                                        Console.ForegroundColor = ConsoleColor.Green;
                                    else
                                        Console.ForegroundColor = ConsoleColor.Cyan;
                                }
                            }
                            Console.Write(DoubleToString(M[i][j], Format) + " ");
                        }
                        Console.WriteLine();
                    }
                }
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                using (StreamWriter writer = new StreamWriter(FileLogger, true))
                {
                    writer.WriteLine(Name);
                    for (int i = 0; i < M.Length; i++)
                    {
                        writer.Write("{0} ", i);
                        for (int j = 0; j < M[i].Length; j++)
                            writer.Write(DoubleToString(M[i][j], Format) + " ");
                        writer.WriteLine();
                    }
                    writer.Close();
                }
            }
        }
        /// <summary>
        /// Печать ЛМЖ для отладки
        /// </summary>
        /// <param name="M"></param>
        /// <param name="Count"></param>
        /// <param name="F"></param>
        public static void Print(string Name, int[][] M, int color = 1)
        {
            //Console.BackgroundColor
            if (flagLogger == 0)
            {
                Console.WriteLine(Name);
                for (int i = 0; i < M.Length; i++)
                {
                    Console.Write("{0} ", i);
                    for (int j = 0; j < M[i].Length; j++)
                        Console.Write(IntToString(M[i][j]) + " ");
                    Console.WriteLine();
                }
            }
            else
            {
                using (StreamWriter writer = new StreamWriter(FileLogger, true))
                {
                    writer.WriteLine(Name);
                    for (int i = 0; i < M.Length; i++)
                    {
                        writer.Write("{0} ", i);
                        for (int j = 0; j < M[i].Length; j++)
                            writer.Write(IntToString(M[i][j]) + " ");
                        writer.WriteLine();
                    }
                    writer.Close();
                }
            }
        }
        /// <summary>
        /// Печать ЛМЖ для отладки
        /// </summary>
        /// <param name="M"></param>
        /// <param name="Count"></param>
        /// <param name="F"></param>
        public static void Print(string Name, uint[][] M, int color = 1)
        {
            if (flagLogger == 0)
            {
                Console.WriteLine(Name);
                for (int i = 0; i < M.Length; i++)
                {
                    if(color==0) Console.Write("{0} ", i);
                    for (int j = 0; j < M[i].Length; j++)
                        Console.Write(M[i][j].ToString() + " ");
                    Console.WriteLine();
                }
            }
            else
            {
                using (StreamWriter writer = new StreamWriter(FileLogger, true))
                {
                    writer.WriteLine(Name);
                    for (int i = 0; i < M.Length; i++)
                    {
                        if (color == 0) writer.Write("{0} ", i);
                        for (int j = 0; j < M[i].Length; j++)
                            writer.Write(M[i][j].ToString() + " ");
                        writer.WriteLine();
                    }
                    writer.Close();
                }
            }
        }
        /// <summary>
        /// Печать ЛМЖ для отладки
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Name"></param>
        /// <param name="M"></param>
        //public static void Print<T>(string Name, T[][] M, int color = 1) where T : struct
        //{
        //    Console.WriteLine(Name);
        //    for (int i = 0; i < M.Length; i++)
        //    {
        //        for (int j = 0; j < M[i].Length; j++)
        //        {
        //            Console.Write(" ");
        //            Console.Write(M[i][j]);
        //        }
        //        Console.WriteLine();
        //    }
        //}
        /// <summary>
        /// Печать ЛМЖ для отладки
        /// </summary>
        /// <param name="M"></param>
        /// <param name="Count"></param>
        /// <param name="F"></param>
        public static void Print(string Name, double[,] M, int F = 2)
        {
            Console.WriteLine(Name);
            string Format = "F" + F.ToString();
            int NX = M.GetLength(0);
            for (int i = 0; i < NX; i++)
            {
                int NY = M.GetLength(1);
                for (int j = 0; j < NY; j++)
                    Console.Write(DoubleToString(M[i,j], Format) + " ");
                Console.WriteLine();
            }
        }
        /// <summary>
        /// Печать ЛМЖ для отладки
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Name"></param>
        /// <param name="M"></param>
        public static void Print<T>(string Name, T[,] M, int color = 1) where T : struct
        {
            Console.WriteLine(Name);
            for (int i = 0; i < M.GetLength(0); i++)
            {
                for (int j = 0; j < M.GetLength(1); j++)
                {
                    Console.Write(" ");
                    Console.Write(M[i, j]);
                }
                Console.WriteLine();
            }
        }

        public static void Print(string Name, double[] M, int F = 2)
        {
            Console.WriteLine(Name);
            string Format = "F" + F.ToString();
            for (int i = 0; i < M.Length; i++)
                Console.Write(DoubleToString(M[i], Format) + " ");
            Console.WriteLine();
        }
        public static void Print(string Name, double[] M, int N, int F = 2)
        {
            Console.WriteLine(Name);
            string Format = "F" + F.ToString();
            for (int i = 0; i < M.Length; i++)
            {
                if (i % N == 0)
                    Console.WriteLine();
                Console.Write(DoubleToString(M[i], Format) + " ");
            }
            Console.WriteLine();
        }
        public static void Print(string Name, int[] M)
        {
            Console.WriteLine(Name);
            for (int i = 0; i < M.Length; i++)
                Console.Write(IntToString(M[i]));
            Console.WriteLine();
        }
        public static void Print(string Name, uint[] M)
        {
            Console.WriteLine(Name);
            for (int i = 0; i < M.Length; i++)
                Console.Write(" " + M[i].ToString());
            Console.WriteLine();
        }
        /// <summary>
        /// Загрузка сетки КЭ, координат вершин и дна, придонных касательных напряжений и давления
        /// </summary>
        /// <param name="FileName">имя файла</param>
        /// <param name="V">напряжение ...</param>
        public static void LoadMasLine(string FileName, ref double[] V, int N)
        {
            using (StreamReader file = new StreamReader(FileName))
            {
                if (file == null)
                {
                    Logger.Instance.Info("штатно не открыт файл " + FileName);
                    return;
                }
                string line = file.ReadLine();
                int Count = int.Parse(line.Trim('\t'));
                Logger.Instance.Info(line);
                V = new double[N];
                for (int i = 0; i < N; i++)
                {
                    line = file.ReadLine().Trim('(', ')', '\t');
                    string[] slines = line.Split(' ', '\t');
                    V[i] = double.Parse(slines[0], formatter);
                }
                Logger.Instance.Info("Загружен файл " + FileName);
            }
        }
        /// <summary>
        /// Запись в файл одномерного массива
        /// </summary>
        /// <param name="FileName">имя файла</param>
        /// <param name="V">массив</param>
        /// <param name="FP">мантиса числе</param>
        public static void SaveMasLine(string FileName, double[] V, int FP = 8)
        {
            string Format = "F" + FP.ToString();
            using (StreamWriter file = new StreamWriter(FileName))
            {
                for (int i = 0; i < V.Length; i++)
                    file.Write(DoubleToString(V[i], Format) + " ");
                file.Close();
            }
        }
    }
}
