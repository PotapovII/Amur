//---------------------------------------------------------------------------
//              Реализация библиотеки поддержки
//                   разработка: Потапов И.И.
//                        30.01.2022 
//---------------------------------------------------------------------------
namespace MemLogLib
{
    using System;
    using MemLogLib.Delegate;
    using System.Collections.Generic;

    public enum TypeMessage
    {
        All = 0,
        Tracer = 1,
        Info = 2,
        Warning = 3,
        Error = 4,
        СriticalError = 5
    }
    /// <summary>
    /// Интерфейс журнала сообщений.
    /// </summary>
    public interface ILogger<T> where T : IMessageItem
    {
        /// <summary>
        /// установка заголовка
        /// </summary>
        SendHeader sendHeader { get; set; }
        /// <summary>
        /// репортер 
        /// </summary>
        SendLog sendMessage { get; set; }
        /// <summary>
        /// Флаг направления лога (консоль / файл)
        /// </summary>
        int FlagLogger { get; set; }
        /// <summary>
        /// Информация о журналируемой задаче
        /// </summary>
        string HeaderInfo { get; }
        /// <summary>
        /// добавление информации о журналируемой задаче
        /// </summary>
        void AddHeaderInfo(string info);
        /// <summary>
        /// очистка информации о журналируемой задаче
        /// </summary>
        void ClearHeaderInfo();
        /// <summary>
        /// Запись состояния логгера
        /// </summary>
        void Write();
        /// <summary>
        /// Добавить сообщение
        /// </summary>
        /// <param name="item"></param>
        void Add(T item);
        /// <summary>
        /// Очистить журнал
        /// </summary>
        void Clear();
        /// <summary>
        /// Трассировка работы - сообщение
        /// </summary>
        /// <param name="message"></param>
        void Tracer(string message);
        /// <summary>
        /// Получить сообщение
        /// </summary>
        /// <param name="message"></param>
        void Info(string message);
        /// <summary>
        /// Получить сообщение об нештатной работе 
        /// </summary>
        /// <param name="message"></param>
        void Warning(string message, string taskName);
        /// <summary>
        /// Получить сообщение об ошибки
        /// </summary>
        /// <param name="message"></param>
        void Error(string message, string taskName);
        /// <summary>
        /// обработка исключений
        /// </summary>
        void Exception(Exception e);
        /// <summary>
        /// Получить сообщений
        /// </summary>
        IList<T> Data { get; }
        /// <summary>
        /// Получить уровень состояния задач/и
        /// </summary>
        TypeMessage TaskState { get; }
    }
}
