﻿#region License
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
//                          ПРОЕКТ  "МКЭ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 10.08.2022 Потапов И.И.
//---------------------------------------------------------------------------
namespace CommonLib
{
    /// <summary>
    /// Класс ребро КО сетки 1 порядка (геометрические параметры ребра и ссылки)
    /// </summary>
    public interface IAFacet : ISFacet
    {
        /// <summary>
        /// Новер грани
        /// </summary>
        int Id { get; set; }
        /// <summary>
        /// Владелец
        /// </summary>
        int OwnerElem { get; set; }
        /// <summary>
        /// Совладелец
        /// </summary>
        int NbElem { get; set; }
    }
}