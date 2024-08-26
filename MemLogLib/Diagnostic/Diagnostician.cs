//---------------------------------------------------------------------------
//              Реализация библиотеки поддержки
//                   разработка: Потапов И.И.
//                        28.12.2021 
//---------------------------------------------------------------------------
namespace MemLogLib
{
    using MemLogLib.Delegate;
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Результат решения задачи  
    /// </summary>
    public class Diagnostician
    {
        Stopwatch stopWatch = new Stopwatch();
        /// <summary>
        /// Время работы метода
        /// </summary>
        public TimeSpan ts { get => stopWatch.Elapsed; }
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
        /// Имя метода
        /// </summary>
        public string Name;
        /// <summary>
        /// Сообщение о решении
        /// </summary>
        public string Message = "Ok";
        /// <summary>
        /// Тестируемый метод
        /// </summary>
        public TestHandle Handle = null;

        public Diagnostician(string Name, TestHandle Handle)
        {
            this.Name = Name;
            this.Handle = Handle;
        }
        public void Start()
        {
            stopWatch.Start();
        }
        public void Stop()
        {
            stopWatch.Stop();
        }
        /// <summary>
        /// Тест
        /// </summary>
        public virtual void Test()
        {
            try
            {
                Start();
                Handle();
                Stop();
            }
            catch(Exception e)
            {
                Message = e.Message;
            }
            Print();
        }
        public virtual void Print()
        {
            LOG.Print("Название теста", Name);
            LOG.Print("Время выполнения теста", elapsedTime);
            LOG.Print("Сообщение о решении", Message);
        }
    }
}
