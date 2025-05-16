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
//             Вычисление функций формы и их производных на КЭ распараллелено
//                    07.11.2024 Потапов И.И.
//---------------------------------------------------------------------------
//             Добавление граничных данных и ленивой функциональности
//                    07.02.2025 Потапов И.И.
//---------------------------------------------------------------------------

namespace MeshLib.Wrappers
{
    using System;
    using System.Linq;

    using MemLogLib;
    using CommonLib;
    using CommonLib.Mesh;
    using CommonLib.Geometry;

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
        /// массив массив длин граничных элементов
        /// </summary>
        public double[] Lb = null;
        /// <summary>
        /// массив нормалей к граничным элементам
        /// </summary>
        public HPoint[] bNormals;
        /// <summary>
        /// массив касательфынх к граничным элементам
        /// </summary>
        public HPoint[] bTau;
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
            MEM.MemCopy(ref Lb, m.GetLb());
            MEM.MemCopy(ref bNormals, m.GetNormals());
            MEM.MemCopy(ref bTau, m.GetTau());
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
            MEM.Alloc(mesh.CountElements, ref S, "S");
            MEM.Alloc(mesh.CountKnots, ref ElemS, "ElemS");
            MEM.Alloc(mesh.CountElements, ref Hx, "Hx");
            MEM.Alloc(mesh.CountElements, ref Hy, "Hy");
            MEM.Alloc(mesh.CountElements, cu, ref N, "N");
            MEM.Alloc(mesh.CountElements, cu, ref dNdx, "dNdx");
            MEM.Alloc(mesh.CountElements, cu, ref dNdy, "dNdx");
            MEM.Alloc(mesh.CountElements, cu, ref aN, "aN");

            TriElement[] knots = mesh.GetAreaElems();
            double[] X = mesh.GetCoords(0);
            double[] Y = mesh.GetCoords(1);
            // Определение региона распарал.

            for (int elem = 0; elem < mesh.CountElements; elem++)
            {
                uint i0 = knots[elem].Vertex1;
                uint i1 = knots[elem].Vertex2;
                uint i2 = knots[elem].Vertex3;
                //Координаты и площадь
                double Se2 = (  X[i1] * Y[i2] + Y[i0] * X[i2] + X[i0] * Y[i1]
                           - Y[i1] * X[i2] - X[i0] * Y[i2] - Y[i0] * X[i1] );

                double[] dxs = { Math.Abs(X[i2] - X[i1]), Math.Abs(X[i0] - X[i2]), Math.Abs(X[i1] - X[i0]) };
                Hx[elem] = dxs.Max() - dxs.Min();
                double[] dys = { Math.Abs(Y[i1] - Y[i2]), Math.Abs(Y[i2] - Y[i0]), Math.Abs(Y[i0] - Y[i1]) };
                Hy[elem] = dys.Max() - dys.Min();

                dNdx[elem][0] = (Y[i1] - Y[i2]) / Se2;
                dNdx[elem][1] = (Y[i2] - Y[i0]) / Se2;
                dNdx[elem][2] = (Y[i0] - Y[i1]) / Se2;

                dNdy[elem][0] = (X[i2] - X[i1]) / Se2;
                dNdy[elem][1] = (X[i0] - X[i2]) / Se2;
                dNdy[elem][2] = (X[i1] - X[i0]) / Se2;

                aN[elem][0] = (X[i1] * Y[i2] - X[i2] * Y[i1]) / Se2;
                aN[elem][1] = (X[i2] * Y[i0] - X[i0] * Y[i2]) / Se2;
                aN[elem][2] = (X[i0] * Y[i1] - X[i1] * Y[i0]) / Se2;

                // поправкк от 23 03 2025
                S[elem] = Se2 / 2.0;
                double S3 = S[elem] / 3.0;
                ElemS[i0] += S3;
                ElemS[i1] += S3;
                ElemS[i2] += S3;
            }
            TwoElement[] BoundElems = mesh.GetBoundElems();
            MEM.Alloc(mesh.CountBoundElements, ref Lb, "Lb");
            MEM.Alloc<HPoint>(mesh.CountBoundElements, ref bTau, "bTau");
            MEM.Alloc<HPoint>(mesh.CountBoundElements, ref bNormals, "bNormals");
            for (int belem = 0; belem < mesh.CountBoundElements; belem++)
            {
                uint i = BoundElems[belem].Vertex1;
                uint j = BoundElems[belem].Vertex2;
                HPoint bl = new HPoint(X[j] - X[i], Y[j] - Y[i]);
                Lb[belem] = bl.Length();
                bTau[belem] = bl/ Lb[belem];
                bNormals[belem] = bTau[belem].GetOrtogonalLeft();
            }
            Area = -1;
            Bottom = -1;
            Width = -1;
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
                    Y[i] = Y[i] /ElemS[i];
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
        /// массив длин граничных элементов
        /// </summary>
        public double[] GetLb()=>Lb;
        /// <summary>
        /// массив нормалей к граничным элементам
        /// </summary>
        public HPoint[] GetNormals()=>bNormals;
        /// <summary>
        /// массив касательфынх к граничным элементам
        /// </summary>
        public HPoint[] GetTau()=>bTau;
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
        //                    07.02.2025 Потапов И.И.
        #region Дополнительная ленивая функциональность
        /// <summary>
        /// Вычисление минимального растояния от узла до стенки
        /// В плпнпх - хеширование узлов на масштабе глубины
        /// </summary>
        public void CalkDistance(ref double[] distance, ref double[] Hp)
        {
            try
            {
                MEM.Alloc(mesh.CountKnots, ref distance, "distance");
                MEM.Alloc(mesh.CountKnots, ref Hp, "Hp");
                double[] X = mesh.GetCoords(0);
                double[] Y = mesh.GetCoords(1);
                TwoElement[] BoundElems = mesh.GetBoundElems();
                int[] BoundElementsMark = mesh.GetBElementsBCMark();

                double Y_max = Y.Max();
                double Y_min = Y.Min();
                double Hmax = Y_max - Y_min;
                // перебор по узлам области
                for (int nod = 0; nod < mesh.CountKnots; nod++)
                {
                    int idx = -1;
                    double r_min = double.MaxValue;

                    for (int belem = 0; belem < mesh.CountBoundElements; belem++)
                    {
                        if (BoundElementsMark[belem] != 2)
                        {
                            uint i = BoundElems[belem].Vertex1;
                            uint j = BoundElems[belem].Vertex2;
                            var PA = new HPoint(X[nod] - X[i], Y[nod] - Y[i]);
                            double r_cur = Math.Abs(HPoint.Dot(PA, bNormals[belem]));
                            double l_cur = Math.Abs(HPoint.Dot(PA, bTau[belem]));
                            if (r_min > r_cur && l_cur <= Lb[belem])
                            {
                                r_min = r_cur;
                                idx = belem;
                                if (MEM.Equals(r_min, 0) == true)
                                    break;
                            }
                        }
                    }
                    if (r_min > Hmax)
                    {
                        //double r_minC = double.MaxValue;
                        //for (int belem = 0; belem < mesh.CountBoundElements; belem++)
                        //{
                        //    if (BoundElementsMark[belem] != 2)
                        //    {
                        //        uint i = BoundElems[belem].Vertex1;
                        //        uint j = BoundElems[belem].Vertex2;
                        //        var Pl = new HPoint(X[nod] - X[i], Y[nod] - Y[i]);
                        //        var Pp = new HPoint(X[nod] - X[j], Y[nod] - Y[j]);
                        //        var minE = Math.Min(Pl.Length(), Pp.Length());
                        //        if (r_minC > minE)
                        //        {
                        //            r_minC = minE;
                        //            idx = belem;
                        //            if (MEM.Equals(r_minC, 0) == true)
                        //                break;
                        //        }
                        //    }
                        //}
                        //uint ii = BoundElems[idx].Vertex1;
                        //var PA = new HPoint(X[nod] - X[ii], Y[nod] - Y[ii]);
                        //r_min = Math.Abs(HPoint.Dot(PA, bNormals[idx]));
                        double r_minC = double.MaxValue;
                        int belemLast = 0;
                        for (int belem = 0; belem < mesh.CountBoundElements; belem++)
                        {
                            if (BoundElementsMark[belem] != 2)
                            {
                                uint i = BoundElems[belem].Vertex1;
                                var Pl = new HPoint(X[nod] - X[i], Y[nod] - Y[i]);
                                var minE = Pl.Length();
                                belemLast = belem;
                                if (r_minC > minE)
                                {
                                    r_minC = minE;
                                    idx = belem;
                                    if (MEM.Equals(r_minC, 0) == true)
                                        break;
                                }
                            }
                        }
                        {
                            uint i = BoundElems[belemLast].Vertex1;
                            var Pl = new HPoint(X[nod] - X[i], Y[nod] - Y[i]);
                            var minE = Pl.Length();
                            if (r_minC > minE)
                            {
                                r_minC = minE;
                                idx = belemLast;
                                if (MEM.Equals(r_minC, 0) == true)
                                    break;
                            }
                        }
                        r_min = r_minC;
                    }
                    distance[nod] = r_min;
                    uint iA = BoundElems[idx].Vertex1;
                    HPoint A = new HPoint(X[iA], Y[iA]);
                    var AP = new HPoint(X[nod] - X[iA], Y[nod] - Y[iA]);
                    HPoint C = A + HPoint.Dot(AP,bTau[idx]) * bTau[idx];
                    double cosGamma = Math.Abs(bNormals[idx].Y) + MEM.Error14;
                    Hp[nod] = Math.Max(0.01 * Hmax, Math.Min(Hmax, (Y_max - C.Y + MEM.Error8) / cosGamma));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// Площадь сечения
        /// </summary>
        /// <returns></returns>
        public double GetArea()
        {
            if (Area < 0)
                Area = S.Sum();
            return Area;
        }
        protected double Area = - 1;
        /// <summary>
        /// Смоченный периметр живого сечения
        /// </summary>
        /// <returns></returns>
        public double GetBottom()
        {
            if (Bottom < 0)
            {
                int[] BoundElementsMark = mesh.GetBElementsBCMark();
                Bottom = 0;
                for (int belem = 0; belem < mesh.CountBoundElements; belem++)
                    if (BoundElementsMark[belem] != 2)
                        Bottom += Lb[belem];
            }
            return Bottom;
        }
        protected double Bottom = -1;
        /// <summary>
        /// Ширина живого сечения
        /// </summary>
        /// <returns></returns>
        public double GetWidth()
        {
            if (Width < 0)
            {
                int[] BoundElementsMark = mesh.GetBElementsBCMark();
                Width = 0;
                for (int belem = 0; belem < mesh.CountBoundElements; belem++)
                    if (BoundElementsMark[belem] == 2)
                        Width += Lb[belem];
            }
            return Width;
        }
        protected double Width = -1;
        /// <summary>
        /// Расчет интеграла по площади расчетной области для функции U 
        /// (например расхода воды через створ, если U - скорость потока в узлах)
        /// </summary>
        /// <param name="U">функции</param>
        /// <param name="Area">площадь расчетной области </param>
        /// <returns>интеграла по площади расчетной области для функции U</returns>
        /// <exception cref="Exception"></exception>  
        public double RiverFlowRate(double[] U, ref double Area)
        {
            double bcu = 1.0/3.0;
            Area = 0;
            double riverFlowRateCalk = 0;
            TriElement[] eKnots = mesh.GetAreaElems();
            for (uint elem = 0; elem < mesh.CountElements; elem++)
            {
                double mU = (U[eKnots[elem].Vertex1] +
                             U[eKnots[elem].Vertex2] +
                             U[eKnots[elem].Vertex3]) * bcu;
                // расход по живому сечению
                riverFlowRateCalk += S[elem] * mU;
                Area += S[elem];
            }
            return riverFlowRateCalk;
        }
        #endregion

    }
}
