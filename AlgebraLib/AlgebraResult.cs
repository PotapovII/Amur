//---------------------------------------------------------------------------
//   Реализация библиотеки методов решения систем алгебраических уравнений
//                     - (C) Copyright 2021 -
//                       ALL RIGHT RESERVED
//                   разработка: Потапов И.И.
//                          31.07.2021 
//---------------------------------------------------------------------------
namespace AlgebraLib
{
    using MemLogLib;
    using System;
    using System.Diagnostics;
    using CommonLib;


    /// <summary>
    /// Результат решения задачи
    /// </summary>
    public class AlgebraResult : IAlgebraResult
    {
        Stopwatch stopWatch = new Stopwatch();
        /// <summary>
        /// Время работы метода
        /// </summary>
        public TimeSpan ts => stopWatch.Elapsed; 
        /// <summary>
        /// Время работы метода в строковом формате
        /// </summary>
        public string elapsedTime
        {
            get
            {
                string s = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
                return s;
            }
        }
        /// <summary>
        /// Порядок системы
        /// </summary>
        public uint N { get; set; }
        /// <summary>
        /// Имя метода
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Сообщение о решении
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// тип ошибки
        /// </summary>
        public ErrorType errorType { get; set; }
        /// <summary>
        /// решение СЛАУ
        /// </summary>
        public double[] X { get; set; }
        /// <summary>
        /// число обусловленности
        /// </summary>
        public double conditionality { get; set; }

        public AlgebraResult()
        {
            this.N = 0;
            this.Name = "";
            this.Message = "респект";
            this.errorType = ErrorType.notError;
            this.X = null;
            conditionality = 0;
        }
        public AlgebraResult(AlgebraResult a)
        {
            N = a.N;
            Name = a.Name;
            Message = a.Message;
            errorType = a.errorType;
            X = a.X;
            conditionality = a.conditionality;
        }
        public void Start()
        {
            stopWatch.Start();
        }
        public void Stop()
        {
            stopWatch.Stop();
        }
        public virtual void Print()
        {
            LOG.Print("Время решения СЛАУ", elapsedTime);
            LOG.Print("Порядок СЛАУ", N);
            LOG.Print("Имя метода", Name);
            LOG.Print("Сообщение о решении", Message);
            LOG.Print("Число обусловленности", conditionality);
            LOG.TPrint("Тип ошибки", (int)errorType);
            LOG.Print("==дополнительная ==", "== информация ======");
        }
    }
}
