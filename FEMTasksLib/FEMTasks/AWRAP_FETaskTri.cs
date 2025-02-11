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
//          Абстрактная обертка для tri КЭ задач
//                       Потапов И.И.
//                        07.04.2021
//---------------------------------------------------------------------------
namespace FEMTasksLib.FEMTasks.VortexStream
{
    using System;

    using CommonLib;
    using CommonLib.Mesh;
    using MeshLib.Wrappers;

    [Serializable]
    public class AWRAP_FETaskTri : AFETaskTri
    {
        #region Локальыне переменные задачи
        /// <summary>
        /// КЭ врапер для сетки
        /// </summary>
        public IMeshWrapper WMesh() => wMesh;
        /// <summary>
        /// КЭ врапер для сетки
        /// </summary>
        public IMeshWrapper wMesh = null;
        /// <summary>
        /// Перегруженный метод
        /// </summary>
        /// <returns></returns>
        public new IMesh Mesh() => mesh;
        /// <summary>
        /// Список узлов для КЭ
        /// </summary>
        protected TriElement[] eKnots;
        /// <summary>
        /// Граничные КЭ
        /// </summary>
        protected TwoElement[] beKnots;
        /// <summary>
        /// Матрица масс
        /// </summary>
        protected double[][] MM =
        {
            new double[3] { 1 / 12.0, 1 / 24.0, 1 / 24.0 },
            new double[3] { 1 / 24.0, 1 / 12.0, 1 / 24.0 },
            new double[3] { 1 / 24.0, 1 / 24.0, 1 / 12.0 }
        };
        /// <summary>
        /// номера граничных узлов
        /// </summary>
        protected int[] Index = null;
        /// <summary>
        /// маркеры граничных узлов
        /// </summary>
        protected int[] Marker = null;
        /// <summary>
        /// адреса строки САУ
        /// </summary>
        protected uint[] ColAdress = null;
        /// <summary>
        /// адреса функии тока на границе
        /// </summary>
        protected uint[] bcIndex = null;
        /// <summary>
        /// Производные от функций форм по x
        /// </summary>
        protected double[][] dNdx = null;
        /// <summary>
        /// Производные от функций форм по y
        /// </summary>
        protected double[][] dNdy = null;
        /// <summary>
        /// площади КЭ
        /// </summary>
        protected double[] S = null;
        /// <summary>
        /// координаты узлов по Х
        /// </summary>
        protected double[] X = null;
        /// <summary>
        /// координаты узлов по У
        /// </summary>
        protected double[] Y = null;
        protected double[] Hx;
        protected double[] Hy;
        #endregion
        public virtual void SetTask(IMesh mesh, IAlgebra algebra, IMeshWrapper wMesh = null)
        {
            base.SetTask(mesh, algebra);
            X = mesh.GetCoords(0);
            Y = mesh.GetCoords(1);
            if (wMesh == null)
                wMesh = new MeshWrapperTri(mesh);
            eKnots = mesh.GetAreaElems();
            beKnots = mesh.GetBoundElems();
            Index = mesh.GetBoundKnots();
            Marker = mesh.GetBoundKnotsMark();
            dNdx = wMesh.GetdNdx();
            dNdy = wMesh.GetdNdy();
            S = wMesh.GetS();
            Hx = wMesh.GetHx();
            Hy = wMesh.GetHx();
            this.wMesh = wMesh;
        }
        /// <summary>
        /// Сокрытие абстрактного метода если классы наследник в нем ненуждаются
        /// </summary>
        /// <param name="result">результат решения</param>
        public override void SolveTask(ref double[] result) { }
    }
}
