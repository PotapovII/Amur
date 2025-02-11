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
    public static class ERR
    {
        public static bool INF_NAN(string Name, double[] M, double range = 3)
        {
            for (int i = 0; i < M.Length; i++)
            {
                if (double.IsInfinity(M[i]) == true || double.IsNaN(M[i]) == true)
                {
                    string mes = "Inf/NaN ошибка в массиве по индексам i = " + i.ToString();
                    string emes = "Inf/NaN ошибка в массиве " + Name +
                                  " по индексам i = " + i.ToString();
                    Logger.Instance.Error(mes, Name);
                    throw new Exception(emes);
                }
            }
            return true;
        }
        public static bool INF_NAN(string Name, double[][] M, double range = 3)
        {
            for (int i = 0; i < M.Length; i++)
            {
                for (int j = 0; j < M[i].Length; j++)
                {
                    if (double.IsInfinity(M[i][j]) == true ||
                        double.IsNaN(M[i][j]) == true)
                    {
                        string mes = "Inf/NaN ошибка в массиве по индексам i = "
                            + i.ToString() + " j = " + j.ToString();
                        string emes = "Inf/NaN ошибка в массиве " + Name + 
                            " по индексам i = " + i.ToString() + " j = " + j.ToString();
                        Logger.Instance.Error(mes, Name);
                        throw new Exception(emes);
                    }
                }
            }
            return true;
        }
        public static bool INF_NAN(string Name, double Value)
        {
            if (double.IsInfinity(Value) == true || double.IsNaN(Value) == true)
            {
                string mes = Name + " is inf/Nan";
                Console.WriteLine(mes);
                throw new Exception(mes);
            }
            return true;
        }
        /// <summary>
        /// Ограничитель функции
        /// </summary>
        /// <param name="M"></param>
        /// <param name=""></param>
        public static void INF_MIN(double[][] M, double value = 0)
        {
            for (int i = 0; i < M.Length; i++)
                for (int j = 0; j < M[i].Length; j++)
                    M[i][j] = Math.Max(value, M[i][j]);
        }
    }
}
