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
//                    09.04.2024 Потапов И.И.
//---------------------------------------------------------------------------
//             Эксперимент: прототип "Расчетная область"
//---------------------------------------------------------------------------
namespace FEMTasksLib.FESimpleTask
{
    
    using CommonLib;
    using CommonLib.Mesh;
    using MemLogLib;
    using System;

    /// <summary>
    /// ОО: Решение вспомогательных задач по интерполяции полей скорости и напряжений
    /// </summary>
    [Serializable]
    public class ATriFEMTask : AComplexFEMTask
    {
        #region Вспомогательыне поля
        /// <summary>
        /// Скорость створа
        /// </summary>
        protected double[] Ux = null;
        /// <summary>
        /// Вторичный поток по y
        /// </summary>
        protected double[] Uy = null;
        protected double[] eUy = null;
        /// <summary>
        /// Вторичный поток по z
        /// </summary>
        protected double[] Uz = null;
        protected double[] eUz = null;
        /// <summary>
        /// Напряжение 
        /// </summary>
        protected double[] TauXY = null;
        protected double[] eTauXY = null;
        /// <summary>
        /// Напряжение 
        /// </summary>
        protected double[] TauXX = null;
        protected double[] eTauXX = null;
        /// <summary>
        /// Напряжение 
        /// </summary>
        protected double[] TauXZ = null;
        protected double[] eTauXZ = null;
        /// <summary>
        /// Напряжение 
        /// </summary>
        protected double[] TauYY = null;
        protected double[] eTauYY = null;
        /// <summary>
        /// Напряжение 
        /// </summary>
        protected double[] TauYZ = null;
        protected double[] eTauYZ = null;
        /// <summary>
        /// Напряжение 
        /// </summary>
        protected double[] TauZZ = null;
        protected double[] eTauZZ = null;
        /// <summary>
        /// Вихревая вязкость + вязкость 
        /// </summary>
        protected double[] eddyViscosity;
        /// <summary>
        /// Вихревая вязкость + вязкость на КЭ
        /// </summary>
        protected double[] e_eddyViscosity;
        #endregion
        /// <summary>
        /// Количество узлов на КЭ
        /// </summary>
        protected uint cu = 3;
        /// <summary>
        /// Список узлов для КЭ
        /// </summary>
        protected TriElement[] eKnots;
        protected TwoElement[] beKnots;

        double[] S_elem = null;
        public ATriFEMTask(IMeshWrapper wMesh, IAlgebra algebra, TypeTask typeTask) :
            base(wMesh, algebra, typeTask)
        {
            eKnots = mesh.GetAreaElems();
            beKnots = mesh.GetBoundElems();
        }
        public override void SetTask(IMeshWrapper wMesh, IAlgebra algebra, TypeTask typeTask) 
        {
            base.SetTask(wMesh, algebra, typeTask);
            eKnots = mesh.GetAreaElems();
            beKnots = mesh.GetBoundElems();
        }

        protected virtual void Init()
        {
            MEM.Alloc((uint)mesh.CountKnots, ref TauXY);
            MEM.Alloc((uint)mesh.CountKnots, ref TauXZ);
            MEM.Alloc((uint)mesh.CountKnots, ref TauYY);
            MEM.Alloc((uint)mesh.CountKnots, ref TauYZ);
            MEM.Alloc((uint)mesh.CountKnots, ref TauZZ);

            MEM.Alloc((uint)mesh.CountKnots, ref Ux);
            MEM.Alloc((uint)mesh.CountKnots, ref Uy);
            MEM.Alloc((uint)mesh.CountKnots, ref Uz);
            MEM.Alloc((uint)mesh.CountKnots, ref eddyViscosity);

            MEM.Alloc((uint)mesh.CountElements, ref eTauXY);
            MEM.Alloc((uint)mesh.CountElements, ref eTauXZ);
            MEM.Alloc((uint)mesh.CountElements, ref eTauYY);
            MEM.Alloc((uint)mesh.CountElements, ref eTauYZ);
            MEM.Alloc((uint)mesh.CountElements, ref eTauZZ);

            MEM.Alloc((uint)mesh.CountElements, ref eUy);
            MEM.Alloc((uint)mesh.CountElements, ref eUz);
            MEM.Alloc((uint)mesh.CountElements, ref e_eddyViscosity);
        }

        /// <summary>
        /// Интерполяция поля МКЭ
        /// </summary>
        public virtual void Interpolation(ref double[] Value, double[] eValue)
        {
            algebra.Clear();
            //Parallel.For(0, mesh.CountElements, (elem, state) =>
            for (uint elem = 0; elem < mesh.CountElements; elem++)
            {
                // локальная матрица часть СЛАУ
                double[][] LaplMatrix = new double[3][]
                {
                       new double[3]{ 0,0,0 },
                       new double[3]{ 0,0,0 },
                       new double[3]{ 0,0,0 }
                };
                // локальная правая часть СЛАУ
                double[] LocalRight = { 0, 0, 0 };
                uint[] knots = {
                    eKnots[elem].Vertex1, eKnots[elem].Vertex2, eKnots[elem].Vertex3
                };
                //подготовка ЛЖМ
                for (int ai = 0; ai < cu; ai++)
                    for (int aj = 0; aj < cu; aj++)
                    {
                        if (ai == aj)
                            LaplMatrix[ai][aj] = 2.0 * S[elem] / 12.0;
                        else
                            LaplMatrix[ai][aj] = S[elem] / 12.0;
                    }
                //добавление вновь сформированной ЛЖМ в ГМЖ
                algebra.AddToMatrix(LaplMatrix, knots);
                //подготовка ЛПЧ
                for (int j = 0; j < cu; j++)
                    LocalRight[j] = eValue[elem] * S[elem] / 3;
                //добавление вновь сформированной ЛПЧ в ГПЧ
                algebra.AddToRight(LocalRight, knots);
            }// });
            algebra.Solve(ref Value);
        }
        /// <summary>
        /// Расчет интеграла по площади расчетной области для функции U 
        /// (например расхода воды через створ, если U - скорость потока в узлах)
        /// </summary>
        /// <param name="U">функции</param>
        /// <param name="Area">площадь расчетной области </param>
        /// <returns>интеграла по площади расчетной области для функции U</returns>
        /// <exception cref="Exception"></exception>
        public virtual double RiverFlowRate(double[] U,ref double Area)
        {
            double area = 0;
            double riverFlowRateCalk = 0;
            for (uint elem = 0; elem < mesh.CountElements; elem++)
            //    Parallel.For(0, mesh.CountElements, (elem, state) =>
            {
                double mU = (U[eKnots[elem].Vertex1] +
                             U[eKnots[elem].Vertex2] +
                             U[eKnots[elem].Vertex3]) / cu;
                // расход по живому сечению
                riverFlowRateCalk += S[elem] * mU;
                area += S[elem];
            }// });
            Area = area;
            if (double.IsNaN(riverFlowRateCalk) == true)
                throw new Exception("riverFlowRateCalk NaN");
            return riverFlowRateCalk;
        }
        /// <summary>
        /// Нахождение полей напряжений и их сглаживание по методу Галеркина
        /// </summary>
        /// <param name="Ux">скорость в створе</param>
        /// <param name="eddyViscosity">вязкость в створе</param>
        /// <param name="Local">вид интерполяции с елементов в узлы</param>
        public void Calk_TauXY_TauXZ(bool Local = true)
        {
            try
            {
                // Parallel.For(0, mesh.CountElements, (elem, state) =>
                for (uint elem = 0; elem < mesh.CountElements; elem++)
                {
                    double[] b = dNdx[elem];
                    double[] c = dNdy[elem];
                    uint i0 = eKnots[elem].Vertex1;
                    uint i1 = eKnots[elem].Vertex2;
                    uint i2 = eKnots[elem].Vertex3;
                    double eddyVis = (eddyViscosity[i0] + eddyViscosity[i1] + eddyViscosity[i2]) / 3;
                    eTauXY[elem] = eddyVis * (Ux[i0] * b[0] + Ux[i1] * b[1] + Ux[i2] * b[2]);
                    eTauXZ[elem] = eddyVis * (Ux[i0] * c[0] + Ux[i1] * c[1] + Ux[i2] * c[2]);
                }//);
                if (Local == true)
                {
                    wMesh.ConvertField(ref TauXY, eTauXY);
                    wMesh.ConvertField(ref TauXZ, eTauXZ);
                }
                else
                {
                    Interpolation(ref TauXY, eTauXY);
                    Interpolation(ref TauXZ, eTauXZ);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
        }

        /// <summary>
        /// Нахождение полей напряжений и их сглаживание по методу Галеркина
        /// </summary>
        /// <param name="Ux">скорость в створе</param>
        /// <param name="eddyViscosity">вязкость в створе</param>
        /// <param name="Local">вид интерполяции с елементов в узлы</param>
        public void Calk_tensor_deformations(ref double[][] tDef, ref double[] E2)
        {
            try
            {
                IMeshWrapperСhannelSectionCFG wm = (IMeshWrapperСhannelSectionCFG)wMesh;
                double R_midle = wm.GetR_midle();
                int Ring = wm.GetRing();
                int NE = 5 + Ring;
                MEM.Alloc(mesh.CountKnots, NE, ref tDef);
                MEM.Alloc(mesh.CountKnots, ref E2);
                if (Ring == 1)
                {
                    MEM.Alloc(mesh.CountKnots, ref TauXX);
                    MEM.Alloc(mesh.CountElements, ref eTauXX);
                    for (uint elem = 0; elem < mesh.CountElements; elem++)
                    {
                        double[] b = dNdx[elem];
                        double[] c = dNdy[elem];
                        uint i0 = eKnots[elem].Vertex1;
                        uint i1 = eKnots[elem].Vertex2;
                        uint i2 = eKnots[elem].Vertex3;

                        double R_elem = (Y[i0] + Y[i1] * Y[i2]) / 3 + R_midle;
                        eTauXX[elem] = (Uy[i0] + Uy[i1] * Uy[i2]) / (3 * R_elem);
                        double U_Y = (Ux[i0] + Ux[i1] * Ux[i2]) / (3 * R_elem);

                        eTauXY[elem] = 0.5 * ((Ux[i0] * b[0] + Ux[i1] * b[1] + Ux[i2] * b[2]) - U_Y);
                        eTauXZ[elem] = 0.5 * (Ux[i0] * c[0] + Ux[i1] * c[1] + Ux[i2] * c[2]);

                        eTauYZ[elem] = 0.5 * Uz[i0] * b[0] + Uz[i1] * b[1] + Uz[i2] * b[2] +
                                             Uy[i0] * c[0] + Uy[i1] * c[1] + Uy[i2] * c[2];

                        eTauYY[elem] = Uy[i0] * b[0] + Uy[i1] * b[1] + Uy[i2] * b[2];
                        eTauZZ[elem] = Uz[i0] * c[0] + Uz[i1] * c[1] + Uz[i2] * c[2];
                    }
                    wMesh.ConvertField(ref TauXX, eTauXX);
                }
                else
                {
                    for (uint elem = 0; elem < mesh.CountElements; elem++)
                    {
                        double[] b = dNdx[elem];
                        double[] c = dNdy[elem];
                        uint i0 = eKnots[elem].Vertex1;
                        uint i1 = eKnots[elem].Vertex2;
                        uint i2 = eKnots[elem].Vertex3;

                        eTauXY[elem] = 0.5 * (Ux[i0] * b[0] + Ux[i1] * b[1] + Ux[i2] * b[2]);
                        eTauXZ[elem] = 0.5 * (Ux[i0] * c[0] + Ux[i1] * c[1] + Ux[i2] * c[2]);

                        eTauYZ[elem] = 0.5 * Uz[i0] * b[0] + Uz[i1] * b[1] + Uz[i2] * b[2] +
                                             Uy[i0] * c[0] + Uy[i1] * c[1] + Uy[i2] * c[2];
                        eTauYY[elem] = Uy[i0] * b[0] + Uy[i1] * b[1] + Uy[i2] * b[2];
                        eTauZZ[elem] = Uz[i0] * c[0] + Uz[i1] * c[1] + Uz[i2] * c[2];
                    }
                }
                wMesh.ConvertField(ref TauXY, eTauXY);
                wMesh.ConvertField(ref TauXZ, eTauXZ);
                wMesh.ConvertField(ref TauYZ, eTauYZ);
                wMesh.ConvertField(ref TauYY, eTauYY);
                wMesh.ConvertField(ref TauZZ, eTauZZ);

                for (uint k = 0; k < mesh.CountKnots; k++)
                {
                    tDef[k][0] = TauXY[k];
                    tDef[k][1] = TauXZ[k];
                    tDef[k][2] = TauYZ[k];
                    tDef[k][3] = TauYY[k];
                    tDef[k][4] = TauZZ[k];
                    if (Ring == 1)
                        tDef[k][5] = TauXX[k];
                    E2[k] = 0;
                    for (uint j = 0; j < NE; j++)
                        E2[k] += tDef[k][j] * tDef[k][j];
                    E2[k] = Math.Sqrt(2 * E2[k]);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
        }

        /// <summary>
        /// Нахождение полей напряжений и их сглаживание по методу Галеркина
        /// </summary>
        /// <param name="Ux">скорость в створе</param>
        /// <param name="eddyViscosity">вязкость в створе</param>
        /// <param name="Local">вид интерполяции с елементов в узлы</param>
        public void Calk_TauYY_TauZZ_TauYZ(bool Local = true)
        {
            try
            {
                // Parallel.For(0, mesh.CountElements, (elem, state) =>
                for (uint elem = 0; elem < mesh.CountElements; elem++)
                {
                    double[] b = dNdx[elem];
                    double[] c = dNdy[elem];
                    uint i0 = eKnots[elem].Vertex1;
                    uint i1 = eKnots[elem].Vertex2;
                    uint i2 = eKnots[elem].Vertex3;
                    double eddyVis = (eddyViscosity[i0] + eddyViscosity[i1] + eddyViscosity[i2]) / 3.0;
                    eTauYZ[elem] = 0.5 * eddyVis * (Uz[i0] * b[0] + Uz[i1] * b[1] + Uz[i2] * b[2] +
                                                    Uy[i0] * c[0] + Uy[i1] * c[1] + Uy[i2] * c[2]);
                    eTauYY[elem] = eddyVis * (Uy[i0] * b[0] + Uy[i1] * b[1] + Uy[i2] * b[2]);
                    eTauZZ[elem] = eddyVis * (Uz[i0] * c[0] + Uz[i1] * c[1] + Uz[i2] * c[2]);
                }//);
                if (Local == true)
                {
                    wMesh.ConvertField(ref TauYZ, eTauYZ);
                    wMesh.ConvertField(ref TauYY, eTauYY);
                    wMesh.ConvertField(ref TauZZ, eTauZZ);
                }
                else
                {
                    Interpolation(ref TauYZ, eTauYZ);
                    Interpolation(ref TauYY, eTauYY);
                    Interpolation(ref TauZZ, eTauZZ);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
        }
        /// <summary>
        /// Расчет компонент поля скорости по функции тока
        /// </summary>
        /// <param name="result">результат решения</param>
        public void CalcVelosity(double[] Phi, bool Local = true)
        {
            try
            {
                for (uint elem = 0; elem < mesh.CountElements; elem++)
                {
                    double[] b = dNdx[elem];
                    double[] c = dNdy[elem];
                    uint i0 = eKnots[elem].Vertex1;
                    uint i1 = eKnots[elem].Vertex2;
                    uint i2 = eKnots[elem].Vertex3;
                    double dPhidx = Phi[i0] * b[0] + Phi[i1] * b[1] + Phi[i2] * b[2];
                    double dPhidy = Phi[i0] * c[0] + Phi[i1] * c[1] + Phi[i2] * c[2];
                    eUy[elem] = - dPhidy;
                    eUz[elem] =   dPhidx;
                }
                if (Local == true)
                {
                    wMesh.ConvertField(ref Uy, eUy);
                    wMesh.ConvertField(ref Uz, eUz);
                }
                else
                {
                    Interpolation(ref Uy, eUy);
                    Interpolation(ref Uz, eUz);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
        }
    }
}
