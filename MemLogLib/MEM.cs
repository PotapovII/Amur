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
    using System.Linq;
    using System.Globalization;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    /// <summary>
    /// Работа с памятью
    /// </summary>
    public static class MEM
    {
        #region Точность
        /// <summary>
        /// Формат
        /// </summary>
        public static NumberFormatInfo formatter = new NumberFormatInfo { NumberDecimalSeparator = "." };
        /// <summary>
        /// Поле точности 10^-3
        /// </summary>
        public const double Error2 = 0.01;
        /// <summary>
        /// Поле точности 10^-3
        /// </summary>
        public const double Error3 = 0.001;
        /// <summary>
        /// Поле точности 10^-4
        /// </summary>
        public const double Error4 = 0.0001;
        /// <summary>
        /// Поле точности 10^-5
        /// </summary>
        public const double Error5 = 0.00001;
        /// <summary>
        /// Поле точности 10^-6
        /// </summary>
        public const double Error6 = 0.000001;
        /// <summary>
        /// Поле точности 10^-7
        /// </summary>
        public const double Error7 = 0.0000001;
        /// <summary>
        /// Поле точности 10^-8
        /// </summary>
        public const double Error8 = 0.00000001;
        /// <summary>
        /// Поле точности 10^-8
        /// </summary>
        public const double Error9 = 0.000000001;
        /// <summary>
        /// Поле точности 10^-10
        /// </summary>
        public const double Error10 = 0.0000000001;
        /// <summary>
        /// Поле точности 10^-11
        /// </summary>
        public const double Error11 = 0.00000000001;
        /// <summary>
        /// Поле точности 10^-12
        /// </summary>
        public const double Error12 = 0.000000000001;
        /// <summary>
        /// Поле точности 10^-13
        /// </summary>
        public const double Error13 = 0.0000000000001;
        /// <summary>
        /// Поле точности 10^-14
        /// </summary>
        public const double Error14 = 0.00000000000001;
        /// <summary>
        /// Сравнение вещественных чисел
        /// </summary>
        public static bool Equals(float a, float b, double eps = Error5)
        {
            if (Math.Abs(a - b) < eps)
                return true;
            else
                return false;
        }
        /// <summary>
        /// Сравнение вещественных чисел
        /// </summary>
        public static bool Equals(double a, double b, double eps = Error8)
        {
            if (Math.Abs(a - b) < eps)
                return true;
            else
                return false;
        }
        /// <summary>
        /// Ограничение вещественных чисел
        /// </summary>
        public static double SE(double a, double eps = Error8)
        {
            if (Math.Abs(a) < eps)
                return 0;
            else
                return a;
        }

        /// <summary>
        /// Упаковка из 2D в 1D для коротких переменных
        /// </summary>
        public static double Shift(double X, double Y)
        {
            //if (X < MEM.Error5)
            //    return 1000000 * (X + 1) + Y;
            //else
            return 1000000 * X + Y;
        }
        /// <summary>
        /// Реверс массива
        /// </summary>
        /// <typeparam name="Tp"></typeparam>
        /// <param name="y"></param>
        public static void Reverse<Tp>(ref Tp[] y)
        {
            int Count = y.Length - 1;
            for (int i = 0; i < y.Length / 2; i++)
            {
                Tp ytmp = y[i];
                y[i] = y[Count - i];
                y[Count - i] = ytmp;
            }
        }

        /// <summary>
        /// TODO выделить в отдельный класс
        /// </summary>
        /// <param name="Text"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double ParserABS(ref string Text, double a = 0, double b = double.MaxValue)
        {
            if (Text.Trim() == "")
            {
                Text = a.ToString();
                return a;
            }
            try
            {
                return double.Parse(Text, formatter);
            }
            catch (Exception e)
            {
                Logger.Instance.Exception(e);
                Text = a.ToString();
                return a;
            }
        }


        #endregion

        #region Выделение памяти
        /// <summary>
        /// Выделение памяти 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="N">размер массива</param>
        /// <param name="X">массив</param>
        public static void Alloc<T>(int N, ref T[] X, string name = "")
        {
            try
            {
                if (X == null)
                    X = new T[N];
                else
                    if (X.Length != N)
                    X = new T[N];
            }
            catch (Exception exp)
            {
                if (name != "")
                    Logger.Instance.Info("Наименование массива " + name);
                Logger.Instance.Exception(exp);
            }
        }
        #region Частные перегрузки
        public static void Alloc(int N, ref double[] X, string name = "")
        {
            try
            {
                if (X == null)
                    X = new double[N];
                else
                    if (X.Length != N)
                    X = new double[N];
            }
            catch (Exception exp)
            {
                if (name != "")
                    Logger.Instance.Info("Наименование массива " + name);
                Logger.Instance.Exception(exp);
            }
        }
        public static void Alloc(int N, ref float[] X, string name = "")
        {
            try
            {
                if (X == null)
                    X = new float[N];
                else
                    if (X.Length != N)
                    X = new float[N];
            }
            catch (Exception exp)
            {
                if (name != "")
                    Logger.Instance.Info("Наименование массива " + name);
                Logger.Instance.Exception(exp);
            }
        }
        public static void Alloc(int N, ref int[] X, string name = "")
        {
            try
            {
                if (X == null)
                    X = new int[N];
                else
                    if (X.Length != N)
                    X = new int[N];
            }
            catch (Exception exp)
            {
                if (name != "")
                    Logger.Instance.Info("Наименование массива " + name);
                Logger.Instance.Exception(exp);
            }
        }
        public static void Alloc(int N, ref uint[] X, string name = "")
        {
            try
            {
                if (X == null)
                    X = new uint[N];
                else
                    if (X.Length != N)
                    X = new uint[N];
            }
            catch (Exception exp)
            {
                if (name != "")
                    Logger.Instance.Info("Наименование массива " + name);
                Logger.Instance.Exception(exp);
            }
        }
        public static void Alloc(int N, ref bool[] X, string name = "")
        {
            try
            {
                if (X == null)
                    X = new bool[N];
                else
                    if (X.Length != N)
                    X = new bool[N];
            }
            catch (Exception exp)
            {
                if (name != "")
                    Logger.Instance.Info("Наименование массива " + name);
                Logger.Instance.Exception(exp);
            }
        }
        #endregion
        /// <summary>
        /// Выделение памяти с присвоением значения
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="N">размер массива</param>
        /// <param name="value">значение</param>
        /// <param name="X">массив</param>
        public static void VAlloc<T>(int N, T value, ref T[] X, string name = "")
        {
            try
            {
                if (X == null)
                    X = new T[N];
                else
                    if (X.Length != N)
                    X = new T[N];
                for (int i = 0; i < N; i++)
                    X[i] = value;
            }
            catch (Exception exp)
            {
                if (name != "")
                    Logger.Instance.Info("Наименование массива " + name);
                Logger.Instance.Exception(exp);
            }
        }
        /// <summary>
        /// Выделение памяти 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="N">размер массива</param>
        /// <param name="X">массив</param>
        public static void Alloc<T>(uint N, ref T[] X, string name = "")
        {
            try
            {
                if (X == null)
                    X = new T[N];
                else
                    if (X.Length != N)
                    X = new T[N];
            }
            catch (Exception exp)
            {
                if (name != "")
                    Logger.Instance.Info("Наименование массива " + name);
                Logger.Instance.Exception(exp);
            }
        }
        public static void Alloc<T>(uint N, ref double[] X, string name = "")
        {
            try
            {
                if (X == null)
                    X = new double[N];
                else
                    if (X.Length != N)
                    X = new double[N];
            }
            catch (Exception exp)
            {
                if (name != "")
                    Logger.Instance.Info("Наименование массива " + name);
                Logger.Instance.Exception(exp);
            }
        }
        /// <summary>
        /// Выделение памяти 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="N">размер массива</param>
        /// <param name="X">массив</param>
        public static void Alloc0(uint N, ref double[] X, string name = "")
        {
            try
            {
                if (X == null)
                {
                    X = new double[N];
                }
                else
                {
                    if (X.Length != N)
                    {
                        X = new double[N];
                    }
                    else
                    {
                        for (int i = 0; i < N; i++)
                            X[i] = 0;
                    }
                }
            }
            catch (Exception exp)
            {
                if (name != "")
                    Logger.Instance.Info("Наименование массива " + name);
                Logger.Instance.Exception(exp);
            }
        }

        /// <summary>
        /// Выделение памяти для 2D массива
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="N">размер массива</param>
        /// <param name="X">массив</param>
        public static void Alloc<T>(int Nx, int Ny, ref T[,] X)
        {
            if (X == null)
                X = new T[Nx, Ny];
            else
                if (X.GetLength(0) != Nx || X.GetLength(1) != Ny)
                X = new T[Nx, Ny];
        }
        /// <summary>
        /// Выделение памяти или очистка квадратной матрицы
        /// </summary>
        /// <param name="Count">размерность</param>
        /// <param name="LaplMatrix">матрица</param>
        public static void Alloc<T>(int Nx, int Ny, ref T[][] X, string name = "")
        {
            try
            {
                if (X == null)
                {
                    X = new T[Nx][];
                    for (int i = 0; i < Nx; i++)
                        X[i] = new T[Ny];
                    return;
                }
                if (X.Length != Nx)
                {
                    X = new T[Nx][];
                    for (int i = 0; i < Nx; i++)
                        X[i] = new T[Ny];
                    return;
                }
                else
                if (X[0].Length != Ny)
                {
                    for (int i = 0; i < Nx; i++)
                        X[i] = new T[Ny];
                }
            }
            catch (Exception exp)
            {
                if (name != "")
                    Logger.Instance.Info("Наименование массива " + name);
                Logger.Instance.Exception(exp);
            }


        }
        /// <summary>
        /// Выделение памяти с присвоением значения
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="N">размер массива</param>
        /// <param name="X">массив</param>
        /// <param name="value">значение</param>
        public static void Alloc<T>(int N, ref T[] X, T value)
        {
            if (X == null)
                X = new T[N];
            else
                if (X.Length != N)
                X = new T[N];
            for (int i = 0; i < N; i++)
                X[i] = value;
        }

        /// <summary>
        /// Выделение памяти или очистка квадратной матрицы
        /// </summary>
        /// <param name="Count">размерность</param>
        /// <param name="LaplMatrix">матрица</param>
        public static void Alloc2DClear(int Count, ref double[][] LaplMatrix)
        {
            if (LaplMatrix == null)
            {
                LaplMatrix = new double[Count][];
                for (int i = 0; i < Count; i++)
                    LaplMatrix[i] = new double[Count];
            }
            else
            {
                if (Count == LaplMatrix.Length)
                {
                    for (int i = 0; i < Count; i++)
                    {
                        for (int j = 0; j < Count; j++)
                            LaplMatrix[i][j] = 0;
                    }
                }
                else
                {
                    LaplMatrix = new double[Count][];
                    for (int i = 0; i < Count; i++)
                        LaplMatrix[i] = new double[Count];
                }
            }
        }
        #region Выделение памяти для массивов свойств 
        public static T[] New1D<T>(int N)
        {
            T[] mas = null;
            Alloc<T>(N, ref mas);
            return mas;
        }
        public static T[][] New2Dp<T>(int Nx, int Ny, string name = "")
        {
            T[][] mas = null;
            Alloc<T>(Nx, Ny, ref mas, name);
            return mas;
        }
        public static T[,] New2D<T>(int Nx, int Ny, string name = "")
        {
            T[,] mas = null;
            Alloc<T>(Nx, Ny, ref mas);
            return mas;
        }
        #endregion
        /// <summary>
        /// Выделение памяти или очистка квадратной матрицы
        /// </summary>
        /// <param name="Count">размерность</param>
        /// <param name="LaplMatrix">матрица</param>
        public static void Alloc2DClear(int Count, ref double[,] LaplMatrix)
        {
            if (LaplMatrix == null)
                LaplMatrix = new double[Count, Count];
            else
            {
                if (Count == LaplMatrix.GetLength(0))
                {
                    for (int i = 0; i < Count; i++)
                        for (int j = 0; j < Count; j++)
                            LaplMatrix[i, j] = 0;
                }
                else
                    LaplMatrix = new double[Count, Count];
            }
        }

        public static void Alloc2DClear(uint Count, ref double[,] LaplMatrix)
        {
            if (LaplMatrix == null)
                LaplMatrix = new double[Count, Count];
            else
            {
                if (Count == LaplMatrix.GetLength(0))
                {
                    for (int i = 0; i < Count; i++)
                        for (int j = 0; j < Count; j++)
                            LaplMatrix[i, j] = 0;
                }
                else
                    LaplMatrix = new double[Count, Count];
            }
        }

        /// <summary>
        /// Выделение памяти или очистка квадратной матрицы
        /// </summary>
        /// <param name="Count">размерность</param>
        /// <param name="LaplMatrix">матрица</param>
        public static void Alloc2DClear(int Count, ref int[][] LaplMatrix)
        {
            if (LaplMatrix == null)
            {
                LaplMatrix = new int[Count][];
                for (int i = 0; i < Count; i++)
                    LaplMatrix[i] = new int[Count];
            }
            else
            {
                if (Count == LaplMatrix.Length)
                {
                    for (int i = 0; i < Count; i++)
                    {
                        for (int j = 0; j < Count; j++)
                            LaplMatrix[i][j] = 0;
                    }
                }
                else
                {
                    LaplMatrix = new int[Count][];
                    for (int i = 0; i < Count; i++)
                        LaplMatrix[i] = new int[Count];
                }
            }
        }
        /// <summary>
        /// Выделение памяти или очистка квадратной матрицы
        /// </summary>
        /// <param name="Nx">размерность</param>
        /// <param name="Ny">размерность</param>
        /// <param name="LaplMatrix">матрица</param>
        public static void Alloc2DClear(int Nx, int Ny, ref double[][] LaplMatrix, double value = 0)
        {
            if (LaplMatrix == null)
            {
                LaplMatrix = new double[Nx][];
                for (int i = 0; i < Nx; i++)
                    LaplMatrix[i] = new double[Ny];
            }
            else
            {
                if (Nx == LaplMatrix.Length)
                {
                    if (Ny != LaplMatrix[0].Length)
                    {
                        for (int i = 0; i < Nx; i++)
                            LaplMatrix[i] = new double[Ny];
                    }
                }
                else
                {
                    LaplMatrix = new double[Nx][];
                    for (int i = 0; i < Nx; i++)
                        LaplMatrix[i] = new double[Ny];
                }
            }
            for (int i = 0; i < Nx; i++)
                for (int j = 0; j < Ny; j++)
                    LaplMatrix[i][j] = value;
        }
        /// <summary>
        /// Выделение памяти или очистка квадратной матрицы
        /// </summary>
        /// <param name="Nx">размерность</param>
        /// <param name="Ny">размерность</param>
        /// <param name="LaplMatrix">матрица</param>
        public static void Alloc2DClear(int Nx, int Ny, ref int[][] LaplMatrix, int value = 0)
        {
            if (LaplMatrix == null)
            {
                LaplMatrix = new int[Nx][];
                for (int i = 0; i < Nx; i++)
                    LaplMatrix[i] = new int[Ny];
            }
            else
            {
                if (Nx == LaplMatrix.Length)
                {
                    if (Ny != LaplMatrix[0].Length)
                    {
                        for (int i = 0; i < Nx; i++)
                            LaplMatrix[i] = new int[Ny];
                    }
                }
                else
                {
                    LaplMatrix = new int[Nx][];
                    for (int i = 0; i < Nx; i++)
                        LaplMatrix[i] = new int[Ny];
                }
            }
            for (int i = 0; i < Nx; i++)
                for (int j = 0; j < Ny; j++)
                    LaplMatrix[i][j] = value;
        }

        /// <summary>
        /// Выделение памяти или очистка квадратной матрицы
        /// </summary>
        /// <param name="Count">размерность</param>
        /// <param name="LaplMatrix">матрица</param>
        public static void Alloc2D<T>(int Nx, int Ny, ref T[,] LaplMatrix)
        {
            if (LaplMatrix == null)
                LaplMatrix = new T[Nx, Ny];
            else if (Nx != LaplMatrix.GetLength(0) || Ny != LaplMatrix.GetLength(1))
                LaplMatrix = new T[Nx, Ny];
        }
        /// <summary>
        /// Выделение памяти или очистка квадратной матрицы
        /// </summary>
        /// <param name="Nx">размерность</param>
        /// <param name="Ny">размерность</param>
        /// <param name="LaplMatrix">матрица</param>
        public static void Alloc2DClear(int Nx, int Ny, ref double[,] LaplMatrix)
        {
            if (LaplMatrix == null)
                LaplMatrix = new double[Nx, Ny];
            else
            {
                if (Nx == LaplMatrix.GetLength(0) && Ny == LaplMatrix.GetLength(1))
                {
                    for (int i = 0; i < Nx; i++)
                        for (int j = 0; j < Ny; j++)
                            LaplMatrix[i, j] = 0;
                }
                else
                    LaplMatrix = new double[Nx, Ny];
            }
        }
        public static void Alloc2DClear(uint Nx, uint Ny, ref double[,] LaplMatrix)
        {
            if (LaplMatrix == null)
                LaplMatrix = new double[Nx, Ny];
            else
            {
                if (Nx == LaplMatrix.GetLength(0) && Ny == LaplMatrix.GetLength(1))
                {
                    for (int i = 0; i < Nx; i++)
                        for (int j = 0; j < Ny; j++)
                            LaplMatrix[i, j] = 0;
                }
                else
                    LaplMatrix = new double[Nx, Ny];
            }
        }
        /// <summary>
        /// Выделение памяти или очистка квадратной матрицы
        /// </summary>
        /// <param name="Nx">размерность</param>
        /// <param name="Ny">размерность</param>
        /// <param name="LaplMatrix">матрица</param>
        public static double[][] PAlloc2DClear(int Nx, int Ny, double[][] LaplMatrix)
        {
            if (LaplMatrix == null)
            {
                LaplMatrix = new double[Nx][];
                for (int i = 0; i < Nx; i++)
                    LaplMatrix[i] = new double[Ny];
            }
            else
            {
                if (Nx == LaplMatrix.Length)
                {
                    if (Ny == LaplMatrix[0].Length)
                    {
                        Parallel.For(0, Nx, (i, state) =>
                        {
                            for (int j = 0; j < Ny; j++)
                                LaplMatrix[i][j] = 0;
                        });
                    }
                    else
                    {
                        for (int i = 0; i < Nx; i++)
                            LaplMatrix[i] = new double[Ny];
                    }
                }
                else
                {
                    LaplMatrix = new double[Nx][];
                    for (int i = 0; i < Nx; i++)
                        LaplMatrix[i] = new double[Ny];
                }
            }
            return LaplMatrix;
        }
        public static void Alloc2DClear(uint Nx, uint Ny, ref double[][] LaplMatrix)
        {
            try
            {
                if (LaplMatrix == null)
                {
                    LaplMatrix = new double[Nx][];
                    for (int i = 0; i < Nx; i++)
                        LaplMatrix[i] = new double[Ny];
                }
                else
                {
                    if (Nx == LaplMatrix.Length)
                    {
                        if (Ny == LaplMatrix[0].Length)
                        {
                            for (int i = 0; i < Nx; i++)
                                for (int j = 0; j < Ny; j++)
                                    LaplMatrix[i][j] = 0;
                        }
                        else
                        {
                            for (int i = 0; i < Nx; i++)
                                LaplMatrix[i] = new double[Ny];
                        }
                    }
                    else
                    {
                        LaplMatrix = new double[Nx][];
                        for (int i = 0; i < Nx; i++)
                            LaplMatrix[i] = new double[Ny];
                    }
                }
            }
            catch (Exception exp)
            {
                Logger.Instance.Exception(exp);
                Logger.Instance.Exception(exp);
            }
        }
        public static void Alloc2DClear(int Nx, int Ny, ref int[][] LaplMatrix)
        {
            try
            {
                if (LaplMatrix == null)
                {
                    LaplMatrix = new int[Nx][];
                    for (int i = 0; i < Nx; i++)
                        LaplMatrix[i] = new int[Ny];
                }
                else
                {
                    if (Nx == LaplMatrix.Length)
                    {
                        if (Ny == LaplMatrix[0].Length)
                        {
                            for (int i = 0; i < Nx; i++)
                                for (int j = 0; j < Ny; j++)
                                    LaplMatrix[i][j] = 0;
                        }
                        else
                        {
                            for (int i = 0; i < Nx; i++)
                                LaplMatrix[i] = new int[Ny];
                        }
                    }
                    else
                    {
                        LaplMatrix = new int[Nx][];
                        for (int i = 0; i < Nx; i++)
                            LaplMatrix[i] = new int[Ny];
                    }
                }
            }
            catch (Exception exp)
            {
                Logger.Instance.Exception(exp);
            }
        }
        public static void Alloc2D<T>(int Nx, int Ny, ref T[][] LaplMatrix)
        {
            try
            {
                if (LaplMatrix == null)
                {
                    LaplMatrix = new T[Nx][];
                    for (int i = 0; i < Nx; i++)
                        LaplMatrix[i] = new T[Ny];
                }
                else
                {
                    if (Nx == LaplMatrix.Length)
                    {
                        for (int i = 0; i < Nx; i++)
                        {
                            if (LaplMatrix[i] == null)
                                LaplMatrix[i] = new T[Ny];
                            if (Ny != LaplMatrix[i].Length)
                                LaplMatrix[i] = new T[Ny];
                        }
                    }
                    else
                    {
                        LaplMatrix = new T[Nx][];
                        for (int i = 0; i < Nx; i++)
                            LaplMatrix[i] = new T[Ny];
                    }
                }
            }
            catch (Exception exp)
            {
                Logger.Instance.Exception(exp);
            }
        }
        public static void Alloc2D(int Nx, int Ny, ref double[][] LaplMatrix)
        {
            try
            {
                if (LaplMatrix == null)
                {
                    LaplMatrix = new double[Nx][];
                    for (int i = 0; i < Nx; i++)
                        LaplMatrix[i] = new double[Ny];
                }
                else
                {
                    if (Nx == LaplMatrix.Length)
                    {
                        for (int i = 0; i < Nx; i++)
                        {
                            if (LaplMatrix[i] == null)
                                LaplMatrix[i] = new double[Ny];
                            if (Ny != LaplMatrix[i].Length)
                                LaplMatrix[i] = new double[Ny];
                        }
                    }
                    else
                    {
                        LaplMatrix = new double[Nx][];
                        for (int i = 0; i < Nx; i++)
                            LaplMatrix[i] = new double[Ny];
                    }
                }
            }
            catch (Exception exp)
            {
                Logger.Instance.Exception(exp);
            }
        }
        public static void Alloc2D(int Nx, int Ny, ref float[][] LaplMatrix)
        {
            try
            {
                if (LaplMatrix == null)
                {
                    LaplMatrix = new float[Nx][];
                    for (int i = 0; i < Nx; i++)
                        LaplMatrix[i] = new float[Ny];
                }
                else
                {
                    if (Nx == LaplMatrix.Length)
                    {
                        for (int i = 0; i < Nx; i++)
                        {
                            if (LaplMatrix[i] == null)
                                LaplMatrix[i] = new float[Ny];
                            if (Ny != LaplMatrix[i].Length)
                                LaplMatrix[i] = new float[Ny];
                        }
                    }
                    else
                    {
                        LaplMatrix = new float[Nx][];
                        for (int i = 0; i < Nx; i++)
                            LaplMatrix[i] = new float[Ny];
                    }
                }
            }
            catch (Exception exp)
            {
                Logger.Instance.Exception(exp);
            }
        }
        public static void Alloc2D(int Nx, int Ny, ref int[][] LaplMatrix)
        {
            try
            {
                if (LaplMatrix == null)
                {
                    LaplMatrix = new int[Nx][];
                    for (int i = 0; i < Nx; i++)
                        LaplMatrix[i] = new int[Ny];
                }
                else
                {
                    if (Nx == LaplMatrix.Length)
                    {
                        for (int i = 0; i < Nx; i++)
                        {
                            if (LaplMatrix[i] == null)
                                LaplMatrix[i] = new int[Ny];
                            if (Ny != LaplMatrix[i].Length)
                                LaplMatrix[i] = new int[Ny];
                        }
                    }
                    else
                    {
                        LaplMatrix = new int[Nx][];
                        for (int i = 0; i < Nx; i++)
                            LaplMatrix[i] = new int[Ny];
                    }
                }
            }
            catch (Exception exp)
            {
                Logger.Instance.Exception(exp);
            }
        }
        public static void Alloc2D(int Nx, int Ny, ref uint[][] LaplMatrix)
        {
            try
            {
                if (LaplMatrix == null)
                {
                    LaplMatrix = new uint[Nx][];
                    for (int i = 0; i < Nx; i++)
                        LaplMatrix[i] = new uint[Ny];
                }
                else
                {
                    if (Nx == LaplMatrix.Length)
                    {
                        for (int i = 0; i < Nx; i++)
                        {
                            if (LaplMatrix[i] == null)
                                LaplMatrix[i] = new uint[Ny];
                            if (Ny != LaplMatrix[i].Length)
                                LaplMatrix[i] = new uint[Ny];
                        }
                    }
                    else
                    {
                        LaplMatrix = new uint[Nx][];
                        for (int i = 0; i < Nx; i++)
                            LaplMatrix[i] = new uint[Ny];
                    }
                }
            }
            catch (Exception exp)
            {
                Logger.Instance.Exception(exp);
            }
        }

        public static T[,] NewMassRec2D<T>(int Nx, int Ny)
        {
            object obj = new object();
            try
            {
                lock (obj)
                {
                    T[,] LaplMatrix = new T[Nx, Ny];
                    return LaplMatrix;
                }
            }
            catch (Exception exp)
            {
                Logger.Instance.Exception(exp);
                return null;
            }
        }

        public static T[] NewMass<T>(int Nx)
        {
            object obj = new object();
            try
            {
                lock (obj)
                {
                    T[] mas = new T[Nx];
                    return mas;
                }
            }
            catch (Exception exp)
            {
                Logger.Instance.Exception(exp);
                return null;
            }
        }

        public static T[][] NewMass2D<T>(int Nx, int Ny)
        {
            object obj = new object();
            try
            {
                lock (obj)
                {
                    T[][] LaplMatrix = new T[Nx][];
                    for (int i = 0; i < Nx; i++)
                        LaplMatrix[i] = new T[Ny];
                    return LaplMatrix;
                }
            }
            catch (Exception exp)
            {
                Logger.Instance.Exception(exp);
                return null;
            }
        }

        public static void Alloc3D(uint N, uint NL, ref double[][][] Matrix)
        {
            try
            {
                if (Matrix == null)
                    Matrix = new double[N][][];
                if (Matrix.Length != N)
                    Matrix = new double[N][][];
                for (int i = 0; i < N; i++)
                    Alloc2DClear(NL, NL, ref Matrix[i]);
            }
            catch (Exception exp)
            {
                Logger.Instance.Exception(exp);
            }
        }
        /// <summary>
        /// Выделение памяти или очистка массива
        /// </summary>
        /// <param name="N">размерность</param>
        /// <param name="X">массив</param>
        public static void AllocClear(int N, ref double[] X)
        {
            try
            {
                if (X == null)
                    X = new double[N];
                else
                {
                    if (X.Length != N)
                        X = new double[N];
                    else
                        for (int i = 0; i < N; i++)
                            X[i] = 0;
                }
            }
            catch (Exception exp)
            {
                Logger.Instance.Exception(exp);
            }
        }
        /// <summary>
        /// Выделение памяти или очистка массива
        /// </summary>
        /// <param name="N">размерность</param>
        /// <param name="X">массив</param>
        public static void AllocClear(uint N, ref double[] X)
        {
            if (X == null)
                X = new double[N];
            else
            {
                if (X.Length != N)
                    X = new double[N];
                else
                    for (uint i = 0; i < N; i++)
                        X[i] = 0;
            }
        }
        #endregion

        #region Работа с массивами TO DO Убрать? => Distinct()
        /// <summary>
        /// Вычисление количества уникальных элементов
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static int CountUniqueElems(double[] x, ref double[] xx)
        {
            // double[] xxx = x.Distinct().ToArray();
            xx = MEM.Copy(xx, x);
            Array.Sort(xx);
            int CountX = 1;
            int k = 0;
            for (int i = 1; i < xx.Length; i++)
            {
                if (MEM.Equals(xx[k], xx[i]) == false)
                {
                    k = i;
                    CountX++;
                }
            }
            return CountX;
        }
        /// <summary>
        /// Вычисление количества уникальных элементов
        /// </summary>
        public static int CountUniqueElems(double[] x)
        {
            double[] xx = null;
            xx = MEM.Copy(xx, x);
            Array.Sort(xx);
            int CountX = 1;
            int k = 0;
            for (int i = 1; i < xx.Length; i++)
            {
                if (MEM.Equals(xx[k], xx[i]) == false)
                {
                    k = i;
                    CountX++;
                }
            }
            return CountX;
        }
        /// <summary>
        /// Получение уникальных элементов
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static void SelectUniqueElems(double[] x, ref double[] ux)
        {
            List<double> list = new List<double>();
            double[] xx = null;
            xx = MEM.Copy(xx, x);
            Array.Sort(xx);
            int k = 0;
            list.Add(xx[k]);
            for (int i = 1; i < xx.Length; i++)
            {
                if (MEM.Equals(xx[k], xx[i]) == false)
                {
                    k = i;
                    list.Add(xx[i]);
                }
            }
            ux = list.ToArray();
        }
        /// <summary>
        /// Проекция одномерного поля в двумерное
        /// </summary>
        public static void Value1D_to_2D(double[] source1D, ref double[][] value2D, int Nx, int Ny)
        {
            int k = 0;
            for (int i = 0; i < Nx; i++)
                for (int j = 0; j < Ny; j++)
                    value2D[i][j] = source1D[k++];
        }
        /// <summary>
        /// Проекция двумерного поля в одномерное
        /// </summary>
        public static void Value2D_to_1D(double[][] source2D, ref double[] value1D)
        {
            int k = 0;
            for (int i = 0; i < source2D.Length; i++)
                for (int j = 0; j < source2D[i].Length; j++)
                    value1D[k++] = source2D[i][j];
        }
        /// <summary>
        /// Проекция одномерного поля в двумерное
        /// </summary>
        public static void ValueAlloc1D_to_2D(double[] source1D, ref double[][] value2D, int Nx, int Ny)
        {
            Alloc2D(Nx, Ny, ref value2D);
            int k = 0;
            for (int i = 0; i < Nx; i++)
                for (int j = 0; j < Ny; j++)
                    value2D[i][j] = source1D[k++];
        }
        /// <summary>
        /// Проекция двумерного поля в одномерное
        /// </summary>
        public static void ValueAlloc2D_to_1D(double[][] source2D, ref double[] value1D)
        {
            int Count = 0;
            for (int i = 0; i < source2D.Length; i++)
                Count += source2D[i].Length;
            Alloc(Count, ref value1D);
            int k = 0;
            for (int i = 0; i < source2D.Length; i++)
                for (int j = 0; j < source2D[i].Length; j++)
                    value1D[k++] = source2D[i][j];
        }
        /// <summary>
        /// Транспонирование квадратной матрицы и ее запись из одномерно  массива
        /// </summary>
        public static void ValueTransposAlloc1D_to_2D(double[] source1D, ref double[][] value2D, int Nx, int Ny)
        {
            Alloc2D(Ny, Nx, ref value2D);
            int k = 0;
            for (int i = 0; i < Nx; i++)
                for (int j = 0; j < Ny; j++)
                    value2D[j][i] = source1D[k++];
        }

        /// <summary>
        /// Транспонирование квадратной матрицы
        /// </summary>
        public static void TranspositionMatrix(double[][] source2D, ref double[][] value2D)
        {
            Alloc2D(source2D[0].Length, source2D.Length, ref value2D);
            for (int i = 0; i < source2D.Length; i++)
                for (int j = 0; j < source2D[i].Length; j++)
                    value2D[j][i] = source2D[i][j];
        }
        /// <summary>
        /// Переобразование массивов
        /// </summary>
        public static int[] ToInt(uint[] mas)
        {
            int[] m = new int[mas.Length];
            for (int i = 0; i < m.Length; i++)
                m[i] = (int)mas[i];
            return m;
        }
        public static uint[] ToUInt(int[] mas)
        {
            uint[] m = new uint[mas.Length];
            for (int i = 0; i < m.Length; i++)
                m[i] = (uint)mas[i];
            return m;
        }
        public static uint[] СToInt(int[] mas)
        {
            return mas.Cast<uint>().ToArray();
        }
        public static int[] СToUInt(uint[] mas)
        {
            return mas.Cast<int>().ToArray();
        }


        #endregion

        #region Копирование данных, работа с памятью
        /// <summary>
        /// реверс поля в двумерном массиве
        /// </summary>
        /// <param name="a"></param>
        public static void MReverseOrder(double[][] a)
        {
            int Length = a.Length;
            for (int i = 0; i < Length / 2; i++)
            {
                double[] buf = a[i];
                a[i] = a[Length - 1 - i];
                a[Length - 1 - i] = buf;
            }
        }
        /// <summary>
        /// Очистка/установка массива
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="value"></param>
        public static void MemSet<T>(this T[] arr, T value)
        {
            if (arr.Length < 10000)
                for (int i = 0; i < arr.Length; i++)
                    arr[i] = value;
            else
                Parallel.For(0, arr.Length, (i, state) => arr[i] = value);
        }
        /// <summary>
        /// Копирование одномерного массива
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="source"></param>
        public static void MemCopy<T>(ref T[] arr, T[] source)
        {
            Alloc<T>(source.Length, ref arr);
            for (int i = 0; i < arr.Length; i++)
                arr[i] = source[i];
        }
        /// <summary>
        /// Копирование одномерного массива
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="source"></param>
        public static void MemCopy(ref double[] arr, double[] source)
        {
            Alloc(source.Length, ref arr);
            for (int i = 0; i < arr.Length; i++)
                arr[i] = source[i];
        }
        /// <summary>
        /// Копирование одномерного массива
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="source"></param>
        public static T[] Copy<T>(T[] arr, T[] source)
        {
            Alloc<T>(source.Length, ref arr);
            for (int i = 0; i < arr.Length; i++)
                arr[i] = source[i];
            return arr;
        }
        public static void Copy(ref double[] arr, double[] source)
        {
            Alloc(source.Length, ref arr);
            for (int i = 0; i < arr.Length; i++)
                arr[i] = source[i];
        }
        public static void Relax(ref double[] arr, double[] source, double relax = 0.3)
        {
            for (int i = 0; i < arr.Length; i++)
                arr[i] = (1 - relax) * arr[i] + relax * source[i];
        }
        

        /// <summary>
        /// Копирование двухмерного массива
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="source"></param>
        public static void MemCopy<T>(ref T[,] arr, T[,] source)
        {
            Alloc<T>(source.GetLength(0), source.GetLength(1), ref arr);
            for (int i = 0; i < source.GetLength(0); i++)
                for (int j = 0; j < source.GetLength(1); j++)
                    arr[i, j] = source[i, j];
        }
        /// <summary>
        /// Копирование двухмерного массива
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="source"></param>
        public static void MemCopy<T>(ref T[][] arr, T[,] source)
        {
            Alloc2D<T>(source.GetLength(0), source.GetLength(1), ref arr);
            for (int i = 0; i < source.GetLength(0); i++)
                for (int j = 0; j < source.GetLength(1); j++)
                    arr[i][j] = source[i, j];
        }
        /// <summary>
        /// Копирование двухмерного массива
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="source"></param>
        public static void MemCopy<T>(ref T[][] arr, T[][] source, string name = "")
        {
            if (source == null)
            {
                Logger.Instance.Error("Ошибка копирования массива " + name, "пустой исходник");
                return;
            }
            if (source.Length == 0)
            {
                Logger.Instance.Error("Ошибка копирования массива " + name, "пустой исходник");
                return;
            }
            Alloc2D(source.Length, source[0].Length, ref arr);
            for (int i = 0; i < source.Length; i++)
                for (int j = 0; j < source[0].Length; j++)
                    arr[i][j] = source[i][j];
        }
        /// <summary>
        /// Копирование двухмерного массива
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="source"></param>
        public static void MemCopyNull<T>(ref T[][] arr, T[][] source)
        {
            if (source == null)
            {
                Logger.Instance.Error("Ошибка копирования массива", "пустой исходник");
                return;
            }
            if (source.Length == 0)
            {
                Logger.Instance.Error("Ошибка копирования массива", "пустой исходник");
                return;
            }
            arr = new T[source.Length][];
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] != null)
                {
                    Alloc(source[i].Length, ref arr[i]);
                    for (int j = 0; j < source[i].Length; j++)
                        arr[i][j] = source[i][j];
                }
            }
        }
        /// <summary>
        /// Копирование двухмерного массива
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="source"></param>
        public static T[][] Copy<T>(T[][] arr, T[][] source)
        {
            if (source == null)
            {
                Logger.Instance.Error("Ошибка копирования массива", "пустой исходник");
                return null;
            }
            if (source.Length == 0)
            {
                Logger.Instance.Error("Ошибка копирования массива", "пустой исходник");
                return null;
            }
            Alloc2D(source.Length, source[0].Length, ref arr);
            for (int i = 0; i < source.Length; i++)
                for (int j = 0; j < source[0].Length; j++)
                    arr[i][j] = source[i][j];
            return arr;
        }
        /// <summary>
        /// Копирование сылок массива
        /// </summary>
        public static void MemCpy<T>(ref T[] a, T[] b, int Count = 0)
        {
            if (Count == 0 || Count != b.Length)
                Count = b.Length;
            if (a == null)
                a = new T[Count];
            if (a.Length < Count)
                a = new T[Count];
            for (int i = 0; i < Count; i++)
                a[i] = b[i];
        }
        /// <summary>
        /// Копирование двухмерного массива
        /// </summary>
        public static void MemCpy<T>(ref T[][] a, T[][] b)
        {
            Alloc2D<T>(b.Length, b[0].Length, ref a);
            for (int i = 0; i < b.Length; i++)
                for (int j = 0; j < b[0].Length; j++)
                    a[i][j] = b[i][j];
        }
        /// <summary>
        /// Копирование двухмерного массива d в одномерный
        /// </summary>
        public static void MemCpy<T>(ref T[] a, T[][] b)
        {
            MEM.Alloc(b.Length * b[0].Length, ref a);
            int k = 0;
            for (int i = 0; i < b.Length; i++)
                for (int j = 0; j < b[0].Length; j++)
                    a[k++] = b[i][j];
        }
        /// <summary>
        /// Копирование двухмерного массива d в одномерный
        /// </summary>
        public static void MemCpy<T>(int Nx, int Ny, ref T[][] b, T[] a)
        {
            Alloc2D(Nx, Ny, ref b);
            int k = 0;
            for (int i = 0; i < Nx; i++)
                for (int j = 0; j < Ny; j++)
                    b[i][j] = a[k++];
        }
        /// <summary>
        /// Удлинение массива с сохранением данных
        /// </summary>
        public static void MemResizeCopy<T>(ref T[] a, int NewCount)
        {
            if (a.Length < NewCount)
            {
                T[] tmp = null;
                MEM.Alloc(NewCount, ref tmp);
                for (int i = 0; i < a.Length; i++)
                    tmp[i] = a[i];
                a = tmp;
            }
        }
        #endregion
        /// <summary>
        /// Конвертация массива
        /// </summary>
        /// <param name="mas"></param>
        /// <returns></returns>
        public static uint[] IntToUint(int[] mas)
        {
            uint[] Vertex = null;
            MEM.Alloc(mas.Length, ref Vertex);
            for (int i = 0; i < Vertex.Length; i++)
                Vertex[i] = (uint)mas[i];
            return Vertex;
        }

        /// <summary>
        /// Вычисление максимума в двумернрм массиве
        /// </summary>
        public static double Max(double[][] b, bool modul = false)
        {
            double max = b[0][0];
            if (modul == false)
            {
                for (int i = 0; i < b.Length; i++)
                    for (int j = 0; j < b[i].Length; j++)
                        if (max < b[i][j])
                            max = b[i][j];
            }
            else
            {
                for (int i = 0; i < b.Length; i++)
                    for (int j = 0; j < b[i].Length; j++)
                        if (max < Math.Abs(b[i][j]))
                            max = Math.Abs(b[i][j]);
            }
            return max;
        }
        /// <summary>
        /// Вычисление максимума в двумернрм массиве
        /// </summary>
        public static double Max(double[,] b, bool modul = false)
        {
            double max = b[0, 0];
            if (modul == false)
            {
                for (int i = 0; i < b.GetLength(0); i++)
                    for (int j = 0; j < b.GetLength(1); j++)
                        if (max < b[i, j])
                            max = b[i, j];
            }
            else
            {
                for (int i = 0; i < b.GetLength(0); i++)
                    for (int j = 0; j < b.GetLength(1); j++)
                        if (max < Math.Abs(b[i, j]))
                            max = Math.Abs(b[i, j]);
            }
            return max;
        }
        /// <summary>
        /// Вычисление минимума в двумернрм массиве
        /// </summary>
        public static double Min(double[][] b)
        {
            double max = b[0][0];
            for (int i = 0; i < b.Length; i++)
                for (int j = 0; j < b[i].Length; j++)
                    if (max > b[i][j])
                        max = b[i][j];
            return max;
        }
    }
}
