//---------------------------------------------------------------------------
//              Реализация библиотеки поддержки
//                   разработка: Потапов И.И.
//                        30.01.2022 
//---------------------------------------------------------------------------
namespace MemLogLib
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Collections.Generic;
    using MemLogLib.Delegate;
    /// <summary>
    /// Простой логгер, который записывает сообщения в список и/или в консоль
    /// </summary>
    public sealed class Logger : ILogger<IMessageItem>
    {
        /// <summary>
        /// установка заголовка
        /// </summary>
        public SendHeader sendHeader { get; set; }
        /// <summary>
        /// репортер 
        /// </summary>
        public SendLog sendMessage { get; set; }
        /// <summary>
        /// Флаг направления лога (консоль / файл)
        /// </summary>
        public int FlagLogger { get; set; }
        /// <summary>
        /// Информация о журналируемой задаче
        /// </summary>
        public string HeaderInfo { get => headerInfo; }
        string headerInfo;
        /// <summary>
        /// Запись состояния логгера
        /// </summary>
        public void AddHeaderInfo(string info)
        {
            if(headerInfo=="")
                headerInfo += info;
            else
                headerInfo += " && " + info;
            if (sendHeader != null)
                sendHeader(headerInfo);
        }
        /// <summary>
        /// очистка информации о журналируемой задаче
        /// </summary>
        public void ClearHeaderInfo()
        {
            headerInfo = "";
            if (sendHeader != null)
                sendHeader("");
        }
        /// <summary>
        /// Журнал сообщений
        /// </summary>
        private List<IMessageItem> logger = null;
        /// <summary>
        /// Максимальный уровень сообщений
        /// </summary>
        private TypeMessage typeMessage;

        #region Патерн синглетон
        public static object locObject = new Object();
        private static Logger instance = null; 
        private Logger() 
        {
            FlagLogger = 0;
            logger = new List<IMessageItem>();
            typeMessage = TypeMessage.Info;
            sendMessage = null;
            sendHeader = null;
            headerInfo = "";
        }
        public static ILogger<IMessageItem> Instance
        {
            get
            {
                lock (locObject)
                {
                    if (instance == null)
                        instance = new Logger();
                }
                return instance;
            }
        }
        #endregion
        public void Add(IMessageItem item)
        {
            //if (FlagLogger == 1 && item.LevelMessage!= TypeMessage.Tracer)
                logger.Add(item);
            if (FlagLogger == 0) 
                Console.WriteLine(item.ToString());
            if (sendMessage != null)
                sendMessage(item);
        }
        public void Clear()
        {
            logger.Clear();
        }
        public void Tracer(string message)
        {
            Add(new MessageItem(TypeMessage.Tracer, message, ""));
        }

        public void Info(string message)
        {
            Add(new MessageItem(TypeMessage.Info, message, ""));
        }

        public void Warning(string message, string location)
        {
            Add(new MessageItem(TypeMessage.Warning, message, location));
            if(typeMessage < TypeMessage.Warning)
                typeMessage = TypeMessage.Warning;
        }

        public void Error(string message, string location)
        {
            Add(new MessageItem(TypeMessage.Error, message, location));
            if (typeMessage < TypeMessage.Error)
                typeMessage = TypeMessage.Error;
        }
        public void СriticalError(string message, string location)
        {
            Add(new MessageItem(TypeMessage.Error, message, location));
            typeMessage = TypeMessage.СriticalError;
        }
        /// <summary>
        /// обработка исключений
        /// </summary>
        public void Exception(Exception e)
        {
            // сообщение
            string message = e.Message;
            //  Имя приложения или объекта, вызвавшего ошибку
            string Source = e.Source;
            // описание метода вызвавшего исключение
            MethodBase TargetSite = e.TargetSite;
            // Имя метода
            string Name = TargetSite.Name;
            IMessageItem mes = new MessageItem(TypeMessage.СriticalError, "Name: " + Name + " Mes: " + message, Source);
            Add(mes);
            typeMessage = TypeMessage.СriticalError;
        }
        public IList<IMessageItem> Data
        {
            get { return logger; }
        }
        /// <summary>
        /// Получить уровень состояния задач/и
        /// </summary>
        public TypeMessage TaskState
        {
            get { return typeMessage; }
        }
        /// <summary>
        /// Запись состояния логгера
        /// </summary>
        public void Write()
        {
            if (FlagLogger == 0)
            {
                Console.Clear();
                Console.WriteLine(HeaderInfo);
                foreach (IMessageItem m in logger)
                    Console.WriteLine(m.ToString());
            }
            else
            {
                string filename = "LOG " + DateTime.Now.ToString() + ".log";
                using (StreamWriter file = new StreamWriter(filename))
                {
                    Console.WriteLine(HeaderInfo);
                    foreach (IMessageItem m in logger)
                        file.WriteLine(m.ToString());
                    file.Close();
                }
            }
        }
    }
}
