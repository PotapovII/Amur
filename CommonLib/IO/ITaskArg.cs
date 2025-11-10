#region License
/*
Copyright (c) 2019 - 2025  Potapov Igor Ivanovich, Khabarovsk

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
//                   ПРОЕКТ  "Русловые процессы"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 02.09.2025 Потапов И.И.
//---------------------------------------------------------------------------
namespace CommonLib.IO
{
    using GeometryLib;
    using CommonLib.Function;
    using System.Collections.Generic;
    using System;

    public interface ITaskArgs
    {
        int Count {  get; }
        double[] GetValues(int idx);
    }

    public interface IDTaskArgs 
    {
        void GetDigFunction(int idx, IDigFunction function);
        void AddDigFunction(int idx, IDigFunction function);
    }
    public interface IFTaskArgs
    {
        void GetField(int idx, IField field);
        void AddField(int idx, IField field);
    }

    /// <summary>
    /// Простой буффер обмена
    /// </summary>
    public class TaskArgs : ITaskArgs
    {
        /// <summary>
        /// Массивы 
        /// </summary>
        public List<(string name, double[] V)> Values = new List<(string, double[])>();
        /// <summary>
        /// Скалярные параметры
        /// </summary>
        public List<(string name, double S)> SValues = new List<(string, double)>();

        public int Count { get=> Values.Count; }

        public void Clear()
        {
            Values.Clear();
            SValues.Clear();
        }

        public double[] GetValues(int idx)
        {
            if( idx<Values.Count )
                return Values[idx].V;
            return null;
        }
        public void Add(string name, double[] V)
        {
            int f = 0;
            for (int i = 0; i < Values.Count; i++)
                if (name == Values[i].name)
                {
                    f++;
                    Values[i] = (name,V);
                    break;
                }
            if (f == 0)
                Values.Add((name, V));
        }
        public void Add(string name, double S)
        {
            int f = 0;
            for (int i = 0; i < SValues.Count; i++)
                if (name == SValues[i].name)
                {
                    f++;
                    SValues[i] = (name, S);
                    break;
                }
            if (f == 0)
                SValues.Add((name, S));
        }
        public double[] GetValues(string name)
        {
            for(int i=0; i<Values.Count; i++)
            if (name == Values[i].name)
                return Values[i].V;
            return null;
        }
        public double GetSValues(string name)
        {
            for (int i = 0; i < Values.Count; i++)
                if (name == SValues[i].name)
                    return SValues[i].S;
            throw new Exception("Парамер "+ name + " не определен В буфер обмена");
        }
    }
}
