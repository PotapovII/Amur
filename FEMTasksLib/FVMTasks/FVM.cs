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
//                        Потапов И.И.
//                  - (C) Copyright 2021
//                        13.11.2021
//---------------------------------------------------------------------------
namespace FEMTasksLib
{
    using AlgebraLib;
    using CommonLib;
    using MemLogLib;
    using MeshLib;
    using System;

    [Serializable]
    public abstract class AFVM : IFEMTask
    {
        /// <summary>
        /// Флаг отладки
        /// </summary>
        public int Debug { get; set; }
        /// <summary>
        /// Квадратурные точки для численного интегрирования
        /// </summary>
        protected NumInegrationPoints pIntegration;
        protected NumInegrationPoints IPointsA = new NumInegrationPoints();
        protected NumInegrationPoints IPointsB = new NumInegrationPoints();
        /// <summary>
        /// Сетка решателя
        /// </summary>
        public IMesh Mesh { get => mesh; }
        protected IFVMesh mesh;
        /// <summary>
        /// Алгебра задачи
        /// </summary>
        public IAlgebra Algebra { get => algebra; }
        protected IAlgebra algebra;
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
            this.mesh = (IFVMesh)mesh;
            this.algebra = algebra;
            IPointsA.SetInt(0 + (int)mesh.typeRangeMesh, TypeFunForm.Form_2D_Triangle_L1);
            IPointsB.SetInt(0 + (int)mesh.typeRangeMesh, TypeFunForm.Form_2D_Rectangle_L1);
        }
        /// <summary>
        /// создание/очистка ЛМЖ и ЛПЧ ...
        /// </summary>
        /// <param name="cu">количество неизвестных</param>
        public virtual void InitLocal(int cu, int cs = 2)
        {
            MEM.Alloc<double>(cu, ref x);
            MEM.Alloc<double>(cu, ref y);
            // с учетом степеней свободы
            int Count = cu * cs;
            MEM.AllocClear(Count, ref LocalRight);
            MEM.Alloc2DClear(Count, ref LaplMatrix);
        }
        /// <summary>
        /// Решение задачи
        /// </summary>
        /// <param name="result">результат решения</param>
        public abstract void SolveTask(ref double[] result);

        #region  Утилиты
        /// <summary>
        /// Получение адресов неизвестных
        /// </summary>
        /// <param name="bound">узлы опорной сетки</param>
        /// <param name="adressBound">неизвестные</param>
        /// <param name="cs">количество степеней свободы в узле</param>
        public void GetAdress(uint[] bound, ref uint[] adressBound, int cs = 2)
        {
            MEM.Alloc<uint>(bound.Length * cs, ref adressBound);
            if (cs == 1)
                adressBound = bound;
            for (int ai = 0; ai < bound.Length; ai++)
            {
                int li = cs * ai;
                for (uint i = 0; i < cs; i++)
                    adressBound[li + i] = bound[ai] * (uint)cs + i;
            }
        }
        #endregion 
    }
}
