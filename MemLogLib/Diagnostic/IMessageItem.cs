//---------------------------------------------------------------------------
//              Реализация библиотеки поддержки
//                   разработка: Потапов И.И.
//                        30.01.2022 
//---------------------------------------------------------------------------
namespace MemLogLib
{
    using System;
    /// <summary>
    /// Интерфейс элемента журнала.
    /// </summary>
    public interface IMessageItem  
    {
        /// <summary>
        /// Время создания элемента
        /// </summary>
        DateTime Time { get; }
        /// <summary>
        /// Уровень сообщения
        /// </summary>
        TypeMessage LevelMessage { get; }
        /// <summary>
        /// Сообщение
        /// </summary>
        string Message { get; }
        /// <summary>
        /// Задача/подзадача сгенерировшая сообщение
        /// </summary>
        string Location { get; }
        /// <summary>
        /// Вернуть элемента журнала в формате строки
        /// </summary>
        /// <returns></returns>
        string ToString();
    }
}
