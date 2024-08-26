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
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//---------------------------------------------------------------------------
//                    08.03.2024 Потапов И.И.
//---------------------------------------------------------------------------
//             Эксперимент: прототип "Расчетная область"
//---------------------------------------------------------------------------
namespace CommonLib.Mesh
{
    /// <summary>
    /// Тип формы канала в створе потока
    /// </summary>
    public enum СhannelSectionForms
    {
        /// <summary>
        /// Параьолический профиль канала (2 границы)
        /// </summary>
        porabolicСhannelSection = 0,
        /// <summary>
        /// Полу параболический профиль канала (3 границы)
        /// </summary>
        halfPorabolicСhannelSection = 1,
        /// <summary>
        /// Канал с вертикальными стенками (4 границы)
        /// </summary>
        boxСhannelCrossSection = 2,
        /// <summary>
        /// Канал - щель (4 границы)
        /// </summary>
        boxСhannelSection = 3,
        /// <summary>
        /// Канал с вертикальными стенками (4 границы)
        /// </summary>
        boxСhannelCrossTrapezoidSection = 5,
    }
    /// <summary>
    /// Обертка для сетки для задач CFG в створе
    /// </summary>
    public interface IMeshWrapperСhannelCFG : IMeshWrapper
    {
        /// <summary>
        /// Тип формы канала в створе потока
        /// </summary>
        СhannelSectionForms channelSectionForms { get; set; }
        /// <summary>
        /// Минимальное растояние от узла до стенки
        /// </summary>
        double[] GetDistance();
        /// <summary>
        /// Приведенная глубина
        /// </summary>
        /// <returns></returns>
        double[] GetHp();
    }
}