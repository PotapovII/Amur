//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 07.02.2024 Потапов И.И.
//---------------------------------------------------------------------------
//                      Сбока No Property River Lib
//              с убранными из наследников Property классами 
//---------------------------------------------------------------------------
namespace NPRiverLib.APRiver_1XD
{
    using System;
    using System.Collections.Generic;
    using CommonLib;
    using NPRiverLib.ATask;

    [Serializable]
    public abstract class APRiver1XD<TParam> : APRiver<TParam> where TParam : class, ITProperty<TParam>
    {
        public APRiver1XD(TParam p) : base(p, TypeTask.streamX1D) { }
    }
}
