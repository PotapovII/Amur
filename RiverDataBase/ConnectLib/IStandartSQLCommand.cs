﻿//---------------------------------------------------------------------------
//                   ПРОЕКТ  "Русловые процессы"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 11.09.2023 Потапов И.И.
//---------------------------------------------------------------------------
namespace ConnectLib
{
    /// <summary>
    /// Стандарт для генерации строки комманды
    /// </summary>
    public interface IStandartSQLCommand
    {
        string GetCommand();
    }
}