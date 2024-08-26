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
//                    21.02.2024 Потапов И.И.
//---------------------------------------------------------------------------
//             Эксперимент: прототип "Расчетная область"
//---------------------------------------------------------------------------
namespace MeshLib.CArea
{

    using MemLogLib;
    using CommonLib;
    using CommonLib.Tasks;
    
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Расчетная область, описывает только
    /// </summary>
    [Serializable]
    public abstract class ABaseCalculationDomain : IBaseCalculationDomain
    {
        /// <summary>
        /// Сетка расчетной области
        /// </summary>
        /// <returns></returns>
        public IMesh GetMesh() => mesh;
        protected IMesh mesh;
        /// <summary>
        /// Количество неизвестных в расчетной области
        /// </summary>
        /// <returns></returns>
        public uint CountUnknowns() { return (uint)unknowns.Length; }
        /// <summary>
        /// Искомые поля задачи которые позволяют сформировать на 
        /// сетке расчетной области краевые условия задачи
        /// </summary>
        public IUnknown[] Unknowns() => unknowns;
        protected IUnknown[] unknowns;
        /// <summary>
        /// Значения величин для выполнения граничных условий в узлах 
        /// для каждой неизвестной
        /// </summary>
        /// <param name="unknown">номер неизвестной</param>
        /// <returns></returns>
        public void GetBoundKnotsValues(uint unknown, ref double[] values)
        {
            CalcBoundary(unknown);
            values = Values[unknown];
        }
        protected double[][] Values;
        /// <summary>
        /// Значения величин для выполнения граничных условий в узлах 
        /// для каждой неизвестной
        /// </summary>
        /// <param name="unknown">номер неизвестной</param>
        /// <returns></returns>
        public void GetBoundKnotsIndex(uint unknown, ref int[] indexs)
        {
            indexs = Indexs[unknown];
        }
        protected int[][] Indexs;
        /// <summary>
        /// Создание расчетной области
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="unknowns"></param>
        public ABaseCalculationDomain(IMesh mesh, IUnknown[] unknowns)
        {
            this.mesh = mesh;
            this.unknowns = unknowns;
            Values = new double[unknowns.Length][];
            Indexs = new int[unknowns.Length][];
        }
        /// <summary>
        /// Установка Граничных условий для неизвестной unknown
        /// </summary>
        public virtual void Set(double[] values, int[] indexs, uint unknown)
        {
            Indexs[unknown] = indexs;
            Values[unknown] = values;
        }
        /// <summary>
        /// Начальные условия
        /// </summary>
        public abstract void InitValues();
        /// <summary>
        /// Вычисление зависимых граничных условий
        /// </summary>
        /// <param name="unknown"></param>
        public abstract void CalcBoundary(uint unknown);
    }



}
