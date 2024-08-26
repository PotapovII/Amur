#region License
/*
Copyright (c) 2019 - 2024  Potapov Igor Ivanovich, Khabarovsk

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion
//---------------------------------------------------------------------------
//                 Общие делегаты проекта "Русловые процессы"
//---------------------------------------------------------------------------
//                              Потапов И.И.
//                               03.02.24
//---------------------------------------------------------------------------
namespace CommonLib.Delegate
{
    using System;
    #region Общие
    /// <summary>
    /// Шаблон функции по передачи данных (синхронизация потоков и т.д.)
    /// </summary>
    /// <param name="point"></param>
    public delegate void SendData<TArg>(TArg point);
    /// <summary>
    /// Шаблон функции
    /// </summary>
    /// <typeparam name="TArg">тип аргумента</typeparam>
    /// <typeparam name="TResult">тип результата</typeparam>
    /// <param name="x">аргумент</param>
    /// <returns>результат</returns>
    public delegate TResult Function<TResult, TArg>(TArg x);
    #endregion

    #region IRiver
    /// <summary>
    /// Шаблон простой/(пустой) функций - процедуры
    /// </summary>
    [Serializable]
    public delegate void SimpleProcedure();
    /// <summary>
    /// Шаблон функции для установки функций стенки
    /// </summary>
    /// <param name="knot_p"></param>
    /// <param name="dy"></param>
    /// <returns></returns>
    [Serializable]
    public delegate double WallFunction(int knot_p, double dy);
    /// <summary>
    /// Шаблон функции для установки турбулентных моделей
    /// </summary>
    /// <param name="idx"></param>
    /// <returns></returns>
    [Serializable]
    public delegate int TurbulentModel(int idx);
    /// <summary>
    /// Шаблон функции для установки граничных условий
    /// </summary>
    /// <param name="X"></param>
    public delegate void BProc(ref double[][] X);
    #endregion
}
