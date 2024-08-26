//---------------------------------------------------------------------------
//              Реализация библиотеки поддержки
//                   разработка: Потапов И.И.
//                        30.01.2022 
//---------------------------------------------------------------------------
namespace MemLogLib
{
    using System;
    /// <summary>
    /// элемент журнала сообщений
    /// </summary>
    public class MessageItem : IMessageItem
    {
        /// <summary>
        /// Время создания элемента
        /// </summary>
        DateTime time;
        /// <summary>
        /// Уровень сообщения
        /// </summary>
        TypeMessage levelMessage;
        /// <summary>
        /// Сообщение
        /// </summary>
        string message;
        /// <summary>
        /// Задача/подзадача сгенерировшая сообщение
        /// </summary>
        string location;
        public DateTime Time
        {
            get { return time; }
        }
        public TypeMessage LevelMessage
        {
            get { return levelMessage; }
        }
        public string Message
        {
            get { return message; }
        }
        public string Location
        {
            get { return location; }
        }
        public MessageItem(TypeMessage levelMessage, string message, string location)
        {
            this.time = DateTime.Now;
            this.levelMessage = levelMessage;
            this.message = message;
            this.location = location;
        }
        /// <summary>
        /// Вернуть элемента журнала в формате строки
        /// </summary>
        /// <returns></returns>
        public new string ToString()
        {
            string mes = "Time :" + time.ToString() +
                " Level: " + levelMessage.ToString() +
                " MES: " + message +
                " LOC: " + location;
            return mes;
        }
    }
}
