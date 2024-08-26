//---------------------------------------------------------------------------
//            Делегаты диагностики проекта "Русловые процессы"
//---------------------------------------------------------------------------
//                              Потапов И.И.
//                               03.02.24
//---------------------------------------------------------------------------
using System.IO;

namespace MemLogLib.Delegate
{

    #region Диагностика
    /// <summary>
    /// Сообщение стороннему наблюдателю
    /// </summary>
    /// <param name="mes"></param>
    public delegate void SendLog(IMessageItem mes);
    /// <summary>
    /// Сообщение стороннему наблюдателю
    /// </summary>
    /// <param name="mes"></param>
    public delegate void SendHeader(string mes);
    /// <summary>
    /// Тестируемый метод
    /// </summary>
    public delegate void TestHandle();
    #endregion
    #region
    /// <summary>
    /// Тип функций для загрузки параметров/файлов
    /// </summary>
    /// <param name="file"></param>
    public delegate void MLoadParams(StreamReader file);
    #endregion
}
