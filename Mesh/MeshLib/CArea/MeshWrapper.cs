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
namespace MeshLib.CArea
{
    using MemLogLib;
    using CommonLib;
    using CommonLib.Geometry;
    using CommonLib.Mesh;
    using System.Linq;
    using System;

    /// <summary>
    /// ОО: Обертка для КЭ сетки - выполняет предварительные вычисления
    /// при решении задач методом КЭ
    /// </summary>
    [Serializable]
    public class MeshWrapperTri : IMeshWrapper
    {
        /// <summary>
        /// Трехузловая сетка
        /// </summary>
        protected IMesh mesh;
        /// <summary>
        /// массив площадей КЭ
        /// </summary>
        public double[] S = null;
        /// <summary>
        /// массив площадей КО
        /// </summary>
        public double[] ElemS = null;
        /// <summary>
        /// Размер КЭ по х
        /// </summary>
        public double[] Hx = null;
        /// <summary>
        /// Размер КЭ по y
        /// </summary>
        public double[] Hy = null;
        /// <summary>
        /// массив функций формы
        /// </summary>
        public double[][] N = null;
        /// <summary>
        /// массив производных для функций формы
        /// </summary>
        protected double[][] aN = null;
        /// <summary>
        /// массив производных для функций формы
        /// </summary>
        public double[][] dNdx = null;
        /// <summary>
        /// массив производных для функций формы
        /// </summary>
        public double[][] dNdy = null;

        public MeshWrapperTri() {}
        public MeshWrapperTri(IMesh mesh)
        {
            SetMesh(mesh);
        }
        public MeshWrapperTri(IMeshWrapper m)
        {
            this.mesh = m.GetMesh();
            MEM.MemCopy(ref S, m.GetS());
            MEM.MemCopy(ref ElemS, m.GetElemS());
            MEM.MemCopy(ref Hx, m.GetHx());
            MEM.MemCopy(ref Hy, m.GetHy());
            MEM.MemCopy(ref N, m.GetN());
            MEM.MemCopy(ref aN, m.GetAN());
            MEM.MemCopy(ref dNdx, m.GetdNdx());
            MEM.MemCopy(ref dNdy, m.GetdNdy());
        }
        /// <summary>
        /// Установка КЭ сетки
        /// </summary>
        /// <param name="mesh"></param>
        public virtual void SetMesh(IMesh mesh)
        {
            this.mesh = mesh;
            int cu = 3;
            MEM.Alloc(mesh.CountElements, ref S,"S");
            MEM.Alloc(mesh.CountElements, ref ElemS, "ElemS");
            MEM.Alloc(mesh.CountElements, ref Hx, "Hx");
            MEM.Alloc(mesh.CountElements, ref Hy, "Hy");
            MEM.Alloc(mesh.CountElements, cu, ref N, "N");
            MEM.Alloc(mesh.CountElements, cu, ref dNdx, "dNdx");
            MEM.Alloc(mesh.CountElements, cu, ref dNdy, "dNdx");
            MEM.Alloc(mesh.CountElements, cu, ref aN, "aN");

            TriElement[] knots = mesh.GetAreaElems();
            double[] X = mesh.GetCoords(0);
            double[] Y = mesh.GetCoords(1);

            for (int elem = 0; elem < mesh.CountElements; elem++)
            {
                uint i0 = knots[elem].Vertex1;
                uint i1 = knots[elem].Vertex2;
                uint i2 = knots[elem].Vertex3;
                //Координаты и площадь
                S[elem] =   X[i1] * Y[i2] + Y[i0] * X[i2] + X[i0] * Y[i1]
                          - Y[i1] * X[i2] - X[i0] * Y[i2] - Y[i0] * X[i1];

                double S3 = S[elem] / 3.0;
                ElemS[i0] += S3;
                ElemS[i1] += S3;
                ElemS[i2] += S3;

                double[] dxs = { Math.Abs(X[i2] - X[i1]), Math.Abs(X[i0] - X[i2]), Math.Abs(X[i1] - X[i0]) };
                Hx[elem] = dxs.Max() - dxs.Min();
                double[] dys = { Math.Abs(Y[i1] - Y[i2]), Math.Abs(Y[i2] - Y[i0]), Math.Abs(Y[i0] - Y[i1]) };
                Hy[elem] = dys.Max() - dys.Min();

                dNdx[elem][0] = (Y[i1] - Y[i2]) / S[elem];
                dNdx[elem][1] = (Y[i2] - Y[i0]) / S[elem];
                dNdx[elem][2] = (Y[i0] - Y[i1]) / S[elem];

                dNdy[elem][0] = (X[i2] - X[i1]) / S[elem];
                dNdy[elem][1] = (X[i0] - X[i2]) / S[elem];
                dNdy[elem][2] = (X[i1] - X[i0]) / S[elem];

                aN[elem][0] = (X[i1] * Y[i2] - X[i2] * Y[i1]) / S[elem];
                aN[elem][1] = (X[i2] * Y[i0] - X[i0] * Y[i2]) / S[elem];
                aN[elem][2] = (X[i0] * Y[i1] - X[i1] * Y[i0]) / S[elem];
            }
        }

        /// <summary>
        /// Перенос значений с КЭ  в узлы КЭ
        /// </summary>
        /// <param name="Y">узловые значения</param>
        /// <param name="X">значения на КЭ</param>
        public void ConvertField(ref double[] Y, double[] X)
        {
            try
            {
                MEM.Alloc(mesh.CountKnots, ref Y,  0);
                TriElement[] eKnots = mesh.GetAreaElems();
                for (uint elem = 0; elem < mesh.CountElements; elem++)
                {
                    double S3 = S[elem] / 3;
                    Y[eKnots[elem].Vertex1] += X[elem] * S3;
                    Y[eKnots[elem].Vertex2] += X[elem] * S3;
                    Y[eKnots[elem].Vertex3] += X[elem] * S3;
                }
                for (int i = 0; i < Y.Length; i++)
                    Y[i] = Y[i] / ElemS[i];
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
        }


        public IMesh GetMesh() => mesh;

        /// <summary>
        /// Размер КЭ по х
        /// </summary>
        public double[] GetHx() => Hx;
        /// <summary>
        /// Размер КЭ по y
        /// </summary>
        public double[] GetHy() => Hy;
        /// <summary>
        /// массив площадей КЭ
        /// </summary>
        public double[] GetS()=>S;
        /// <summary>
        /// массив площадей КО
        /// </summary>
        public double[] GetElemS()=>ElemS;
        /// <summary>
        /// массив функций формы
        /// </summary>
        public double[][] GetN()=>N;
        /// <summary>
        /// массив 
        /// </summary>
        public double[][] GetAN()=>aN;
        /// <summary>
        /// массив производных для функций формы
        /// </summary>
        public double[][] GetdNdx()=>dNdx;
        /// <summary>
        /// массив производных для функций формы
        /// </summary>
        public double[][] GetdNdy()=>dNdy;
        /// <summary>
        ///  вычисление значений функций формы в точке
        /// </summary>
        /// <param name="elem">номер элемента</param>
        /// <param name="x">координата х</param>
        /// <param name="y">координата Y</param>
        /// <param name="N">массив функций формы</param>
        public void CalkForm(int elem, double x, double y, ref double[] N)
        {
            N[0] = aN[elem][0] + dNdx[elem][0] * x + dNdy[elem][0] * y;
            N[1] = aN[elem][1] + dNdx[elem][1] * x + dNdy[elem][1] * y;
            N[2] = aN[elem][2] + dNdx[elem][2] * x + dNdy[elem][2] * y;
        }
        /// <summary>
        ///  вычисление значений функций формы в точке
        /// </summary>
        /// <param name="elem">номер элемента</param>
        /// <param name="p">точка</param>
        /// <param name="N">массив функций формы</param>
        public void CalkForm(int elem, IHPoint p, ref double[] N)
        {
            N[0] = aN[elem][0] + dNdx[elem][0] * p.X + dNdy[elem][0] * p.Y;
            N[1] = aN[elem][1] + dNdx[elem][1] * p.X + dNdy[elem][1] * p.Y;
            N[2] = aN[elem][2] + dNdx[elem][2] * p.X + dNdy[elem][2] * p.Y;
        }
    }
}
