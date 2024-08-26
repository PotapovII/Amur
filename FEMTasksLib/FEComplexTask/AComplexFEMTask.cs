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
//                    06.03.2024 Потапов И.И.
//---------------------------------------------------------------------------
//             Эксперимент: прототип "Расчетная область"
//---------------------------------------------------------------------------
namespace FEMTasksLib
{
    using CommonLib;
    using CommonLib.Mesh;
    using MemLogLib;
    using MeshLib;
    using System;

    [Serializable]
    public class AComplexFEMTask : IComplexFEMTask
    {
        /// <summary>
        /// Тип задачи
        /// </summary>
        protected TypeTask typeTask;
        /// <summary>
        /// Квадратурные точки для численного интегрирования
        /// </summary>
        protected NumInegrationPoints pIntegration;
        protected NumInegrationPoints IPointsA = new NumInegrationPoints();
        protected NumInegrationPoints IPointsB = new NumInegrationPoints();
        /// <summary>
        /// Сетка решателя
        /// </summary>
        public IMeshWrapper WMesh { get => wMesh; }
        protected IMeshWrapper wMesh;
        protected IMesh mesh;
        /// <summary>
        /// Алгебра задачи
        /// </summary>
        public IAlgebra Algebra { get => algebra; }
        [NonSerialized]
        protected IAlgebra algebra = null;
        /// <summary>
        /// Сетка решателя
        /// </summary>
        public IMesh Mesh { get => mesh; }
        /// <summary>
        /// Площадь КЭ
        /// </summary>
        protected double[] S;
        /// <summary>
        /// Производные от ФФ по Х
        /// </summary>
        protected double[][] dNdx;
        /// <summary>
        /// Производные от ФФ по У
        /// </summary>
        protected double[][] dNdy;
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
        /// координаты узлов 
        /// </summary>
        protected double[] X = null;
        protected double[] Y = null;
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

        public AComplexFEMTask(IMeshWrapper wMesh, IAlgebra algebra, TypeTask typeTask)
        {
            SetTask(wMesh, algebra, typeTask);
        }

        public virtual void SetTask(IMeshWrapper wMesh, IAlgebra algebra, TypeTask typeTask)
        {
            this.typeTask = typeTask;
            this.wMesh = wMesh; 
            mesh = wMesh.GetMesh();
            X = mesh.GetCoords(0);
            Y = mesh.GetCoords(1);
            S = wMesh.GetS();
            dNdx = wMesh.GetdNdx();
            dNdy = wMesh.GetdNdy();
            this.algebra = algebra;
            IPointsA.SetInt((int)mesh.typeRangeMesh, TypeFunForm.Form_2D_Triangle_L1);
            IPointsB.SetInt((int)mesh.typeRangeMesh, TypeFunForm.Form_2D_Rectangle_L1);
        }
        /// <summary>
        /// создание/очистка ЛМЖ и ЛПЧ ...
        /// </summary>
        /// <param name="cu">количество неизвестных</param>
        public virtual void InitLocal(int cu, int cs = 1)
        {
            MEM.Alloc<double>(cu, ref x, "x");
            MEM.Alloc<double>(cu, ref y, "y");
            // с учетом степеней свободы
            int Count = cu * cs;
            MEM.AllocClear(Count, ref LocalRight);
            MEM.Alloc2DClear(Count, ref LaplMatrix);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bound"></param>
        /// <param name="adressBound"></param>
        /// <param name="cs"></param>
        public void GetAdress(uint[] bound, ref uint[] adressBound, int cs = 2)
        {
            MEM.Alloc<uint>(bound.Length * cs, ref adressBound, "adressBound");
            if (cs == 1)
                adressBound = bound;
            for (int ai = 0; ai < bound.Length; ai++)
            {
                int li = cs * ai;
                for (uint i = 0; i < cs; i++)
                    adressBound[li + i] = bound[ai] * (uint)cs + i;
            }
        }
    }
}
