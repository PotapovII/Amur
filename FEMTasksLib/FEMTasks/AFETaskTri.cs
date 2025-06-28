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
//                    Проект "Home" dNdy++
//              Интерфейс задачи VBaseLineTask
//                  - (C) Copyright 2003
//                        Потапов И.И.
//                         14.11.03
//---------------------------------------------------------------------------
//    сильно, упрощенный перенос VBaseLineTask с dNdy++ на dNdy#,
//    не реализована концепция адресных таблиц необходтмых
//      для произвольной аппроксимации полей задачи над
//                     опорной сеткой,
//          не реализована концепция стека задач и 
//    буферов обмена полями и параметрами между задачами
//                       Потапов И.И.
//                        07.04.2021
//---------------------------------------------------------------------------
namespace FEMTasksLib
{
    using AlgebraLib;
    using CommonLib;
    using MemLogLib;
    using System;

    [Serializable]
    public abstract class AFETaskTri : IFEMTask
    {
        /// <summary>
        /// количество неизвестных в узле на конечном элементе
        /// </summary>
        public int cs = 1;
        /// <summary>
        /// количество узлов на конечном элементе
        /// </summary>
        protected int cu = 3;
        /// <summary>
        /// Флаг отладки
        /// </summary>
        public int Debug { get; set; }
        /// <summary>
        /// Сетка решателя
        /// </summary>
        public IMesh Mesh { get => mesh; }
        protected IMesh mesh;
        /// <summary>
        /// Алгебра задачи
        /// </summary>
        public IAlgebra Algebra { get => algebra; }
        [NonSerialized]
        protected IAlgebra algebra = null;
        /// <summary>
        /// Узлы КЭ
        /// </summary>
        protected uint[] knots = null;
        /// <summary>
        /// координаты узлов КЭ
        /// </summary>
        protected double[] x = null;
        protected double[] y = null;
        /// <summary>
        /// локальная матрица часть СЛАУ
        /// </summary>
        protected double[][] LaplMatrix = null;
        /// <summary>
        /// локальная правая часть СЛАУ
        /// </summary>
        protected double[] LocalRight = null;
        /// <summary>
        /// адреса ГУ
        /// </summary>
        protected uint[] adressBound = null;
        public virtual void SetTask(IMesh mesh, IAlgebra algebra)
        {
            this.mesh = mesh;
            if (algebra == null)
                algebra = new AlgebraGauss((uint)(mesh.CountKnots*cs));
             this.algebra = algebra;
        }
        /// <summary>
        /// создание/очистка ЛМЖ и ЛПЧ ...
        /// </summary>
        /// <param name="cu">количество неизвестных</param>
        public virtual void InitLocal(int cu, int cs = 2)
        {
            MEM.Alloc<double>(cu, ref x, "x");
            MEM.Alloc<double>(cu, ref y, "y");
            // с учетом степеней свободы
            int Count = cu * cs;
            MEM.AllocClear(Count, ref LocalRight);
            MEM.Alloc2DClear(Count, ref LaplMatrix);
        }
        /// <summary>
        /// Получить адреса граничных неизвестных
        /// </summary>
        /// <param name="bound"></param>
        /// <param name="adressBound"></param>
        /// <param name="cs"></param>
        //public void GetAdress(uint[] bound, ref uint[] adressBound, int cs = 2, uint shift = 0)
        //{
        //    MEM.Alloc<uint>(bound.Length * cs, ref adressBound, "adressBound");
        //    if (cs == 1)
        //        adressBound = bound;
        //    for (int ai = 0; ai < bound.Length; ai++)
        //    {
        //        int li = cs * ai;
        //        for (uint i = 0; i < cs; i++)
        //            adressBound[li + i] = bound[ai] * (uint)cs + i + shift;
        //    }
        //}
        /// <summary>
        /// Решение задачи
        /// </summary>
        /// <param name="result">результат решения</param>
        public abstract void SolveTask(ref double[] result);
    }
}
