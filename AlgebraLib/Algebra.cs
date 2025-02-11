//---------------------------------------------------------------------------
//   Реализация библиотеки методов решения систем алгебраических уравнений
//                     - (C) Copyright 2021 -
//                       ALL RIGHT RESERVED
//                    разработка: Потапов И.И.
//---------------------------------------------------------------------------
//                        Потапов И.И.
//              member-функции класса HPackAlgebra
//                  Last Edit Data: 25.6.2001
//---------------------------------------------------------------------------
//                        Потапов И.И.
//          HPackAlgebra (C++) ==> SparseAlgebra (C#)
//                       18.04.2021 
//---------------------------------------------------------------------------
namespace AlgebraLib
{
    using MemLogLib;
    using System;
    using CommonLib;

    public enum AlgebraType
    {
        All = 0,
        Sparse1D,
        Sparse2D,
        Tape,
        Full
    }
    /// <summary>
    /// ОО: Класс матрицы в формате CRS
    /// с поддержкой интерфейса алгебры для работы с КЭ задачами
    /// </summary>
    public abstract class Algebra : IAlgebra
    {
        /// <summary>
        /// Тип алгебры
        /// </summary>
        public AlgebraType algebraType = AlgebraType.All;
        /// <summary>
        /// Результат работы вычислителя
        /// </summary>
        protected IAlgebraResult result = null;
        /// <summary>
        /// Информация о полученном результате
        /// </summary>
        public IAlgebraResult Result { get => result; }
        /// <summary>
        /// Флаг отладки
        /// </summary>
        public bool Debug = false;
        /// <summary>
        /// Точность
        /// </summary>
        public static double EPS = MEM.Error9;
        /// <summary>
        /// Название метода
        /// </summary>
        public string Name { get => name; }
        /// <summary>
        /// порядок СЛУ
        /// </summary>
        public uint N { get => FN; }
        /// <summary>
        /// порядок СЛУ
        /// </summary>
        protected uint FN;
        /// <summary>
        /// вектор правой части
        /// </summary>
        public double[] Right;
        /// <summary>
        /// имя метода
        /// </summary>
        protected string name;
        public Algebra(AlgebraResult a = null, uint FN = 1)
        {
            SetAlgebra(a, FN);
        }
        //---------------------------------------------------------------------------
        #region Переопределяемые методы
        /// <summary>
        /// Выделение рабочих массивов
        /// </summary>
        /// <param name="FN">размерность СЛАУ</param>
        public virtual void SetAlgebra(IAlgebraResult a, uint FN)
        {
            this.FN = FN;
            MEM.AllocClear(N, ref Right);
            if (a != null)
                result = a;
            else
                result = new AlgebraResult();
            result.N = N;
        }
        /// <summary>
        /// Очистка матрицы и правой части
        /// </summary>
        public virtual void Clear()
        {
            for (int i = 0; i < N; i++)
                Right[i] = 0;
        }
        #endregion 
        //---------------------------------------------------------------------------
        /// <summary>
        /// Формирование правой части
        /// </summary>
        /// <param name="LRight">Локальная правая часть КЭ аналога</param>
        /// <param name="Adress">Глобальные адреса матрицы связности для текущего конечного элемента</param>
        public virtual void AddToRight(double[] LRight, uint[] Adress)
        {
            // цикл по узлам КЭ
            for (int a = 0; a < Adress.Length; a++)
                Right[Adress[a]] += LRight[a];
        }
        /// <summary>
        /// Добавление в правую часть
        /// </summary>
        public void CopyRight(double[] CRight)
        {
            for (int a = 0; a < Right.Length; a++)
                Right[a] += CRight[a];
        }
        /// <summary>
        /// Получение правой части СЛАУ
        /// </summary>
        /// <param name="CRight"></param>
        public void GetRight(ref double[] CRight)
        {
            for (int a = 0; a < Right.Length; a++)
                CRight[a] = Right[a];
        }
        /// <summary>
        /// Скалярное произведение векторов
        /// </summary>
        protected double MultyVector(double[] a, double[] b)
        {
            double Sum = 0;
            for (uint i = 0; i < N; i++)
                Sum += a[i] * b[i];
            return Sum;
        }
        public void CheckMas(double[] MM, string mes = "")
        {
            if (Debug != true) return;
            Console.WriteLine("щаг отладки " + mes);
            for (int i = 0; i < MM.Length; i++)
                if (double.IsNaN(MM[i]) == true || double.IsInfinity(MM[i]) == true)
                    Console.WriteLine(" i =" + i.ToString());
        }
        /// <summary>
        /// Решение САУ
        /// </summary>
        /// <param name="X">Вектор в который возвращается результат решения СЛАУ</param>
        public virtual void Solve(ref double[] X)
        {
            result.Start();
            solve(ref X);
            result.X = X;
            result.Stop();
        }
        //---------------------------------------------------------------------------

        #region общие абстрактные методы
        /// <summary>
        /// решение СЛАУ
        /// </summary>
        /// <param name="X"></param>
        protected abstract void solve(ref double[] X);
        /// <summary>
        /// нормировка системы для малых значений матрицы и правой части
        /// </summary>
        protected abstract void SystemNormalization();
        /// <summary>
        /// Клонирование объекта
        /// </summary>
        /// <param name="N">порядок системы</param>
        /// <returns></returns>
        public abstract IAlgebra Clone();
        /// <summary>
        /// Сборка САУ по строкам (не для всех решателей)
        /// </summary>
        /// <param name="ColElems">Коэффициенты строки системы</param>
        /// <param name="ColAdress">Адреса коэффицентов</param>
        /// <param name="IndexRow">Индекс формируемой строки системы</param>
        /// <param name="Right">Значение правой части строки</param>
        public abstract void AddStringSystem(double[] ColElems, uint[] ColAdress, uint IndexRow, double R);
        /// <summary>
        /// Получить строку (не для всех решателей)
        /// </summary>
        /// <param name="IndexRow">Индекс получемой строки системы</param>
        /// <param name="ColElems">Коэффициенты строки системы</param>
        /// <param name="R">Значение правой части</param>
        public abstract void GetStringSystem(uint IndexRow, ref double[] ColElems, ref double R);
        /// <summary>
        /// Формирование матрицы системы
        /// </summary>
        /// <param name="LMartix">Локальная матрица</param>
        /// <param name="Adress">Глабальные адреса</param>
        abstract public void AddToMatrix(double[][] LMartix, uint[] Adress);
        /// <summary>
        /// Удовлетворение ГУ (с накопительными вызовами)
        /// </summary>
        /// <param name="Condition">Значения неизвестных по адресам</param>
        /// <param name="Adress">Глабальные адреса</param>
        abstract public void BoundConditions(double[] Conditions, uint[] Adress);
        /// <summary>
        /// Удовлетворение ГУ (с накопительными вызовами)
        /// </summary>
        abstract public void BoundConditions(double Condition, uint[] Adress);
        /// <summary>
        /// Операция определения невязки R = Matrix X - Right
        /// </summary>
        /// <param name="R">результат</param>
        /// <param name="X">умножаемый вектор</param>
        /// <param name="IsRight">знак операции = +/- 1</param>
        abstract public void getResidual(ref double[] R, double[] X, int IsRight = 1);
        /// <summary>
        /// Вывод САУ на КОНСОЛЬ
        /// </summary>
        /// <param name="flag">количество знаков мантисы</param>
        /// <param name="color">длина цветового блока</param>
        public abstract void Print(int flag = 0, int color = 1);

        #endregion

        //---------------------------------------------------------------------------
    }
}
