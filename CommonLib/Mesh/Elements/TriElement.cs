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
//                         ПРОЕКТ  "RiverLib"
//                 правка  :   06.12.2020 Потапов И.И.
//---------------------------------------------------------------------------
namespace CommonLib
{
    using System;
    using System.Runtime.InteropServices;
    /// <summary>
    /// ОО: Структура TriElement определяет индекс трех
    /// вершины в массиве pVertex в функции GradientFill.
    /// Эти три вершины образуют один треугольник,
    /// фиксация порядка полей при экспорте в с++
    /// </summary>
    [StructLayout(LayoutKind.Sequential)] 
    [Serializable]
    public struct TriElement
    {
        /// <summary>
        /// Первая точка треугольника
        /// </summary>
        public uint Vertex1;
        /// <summary>
        /// Вторая точка треугольника
        /// </summary>
        public uint Vertex2;
        /// <summary>
        /// Третья точка треугольника
        /// </summary>
        public uint Vertex3;
        /// <summary>
        /// Итератор
        /// </summary>
        /// <param name="i"></param>
        public uint this[int i]
        {
            get
            {
                if (i == 0)
                    return Vertex1;
                else 
                    if (i == 1)
                        return Vertex2;
                    else
                        if (i == 2)
                            return Vertex3;
                        else
                            throw new Exception("bad vector index");
            }
            set
            {
                if (i == 0)
                    Vertex1 = value;
                else
                    if (i == 1)
                        Vertex2 = value; 
                    else
                        if (i == 2)
                            Vertex3 = value; 
                        else
                            throw new Exception("bad vector index");
            }
        }
        public TriElement(uint vertex1, uint vertex2, uint vertex3)
        {
            this.Vertex1 = vertex1;
            this.Vertex2 = vertex2;
            this.Vertex3 = vertex3;
        }
        public TriElement(int vertex1, int vertex2, int vertex3)
        {
            this.Vertex1 = (uint)vertex1;
            this.Vertex2 = (uint)vertex2;
            this.Vertex3 = (uint)vertex3;
        }
        /// <summary>
        /// Выборка поля в узлах элемента
        /// </summary>
        /// <param name="Values"></param>
        /// <param name="knots"></param>
        /// <param name="elementValue"></param>
        public static void TriElemValues(double[] Values, TriElement knots, ref double[] elementValue)
        {
            elementValue[0] = Values[knots.Vertex1];
            elementValue[1] = Values[knots.Vertex2];
            elementValue[2] = Values[knots.Vertex3];
        }
    }
}