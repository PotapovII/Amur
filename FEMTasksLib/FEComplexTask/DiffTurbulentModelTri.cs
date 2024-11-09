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
namespace FEMTasksLib.FESimpleTask
{
    using System;
    using MemLogLib;
    using CommonLib;
    using CommonLib.Mesh;
    using MeshLib;
    using CommonLib.Physics;

    /// <summary>
    /// ОО: Решение задачи Пуассона на симплекс сетке
    /// </summary>
    [Serializable]
    public class DiffTurbulentModelTri : AComplexFEMTask
    {
        #region Spalart - Allmaras константы

        protected const double kappa = SPhysics.kappa_w;
        protected const double kappa2 = kappa * kappa;
        protected const double sigma = 2 / 3.0;
        protected const double Cb1 = 0.1355;
        protected const double Cb2 = 0.622;
        protected const double Cw3_6 = 64;
        protected const double Cw3 = 2;
        protected const double Cw2 = 0.3;
        protected const double Cw1 = Cb1 / kappa2 + (1 + Cb2) / sigma;
        protected const double Cw1_3 = Cw1 * Cw1 * Cw1;
        protected const double Ct1 = 1;
        protected const double Ct2 = 2;
        protected const double Ct3 = 1.2;
        protected const double Ct4 = 0.5;
        protected const double Utrim = 0.1;
        /// <summary>
        /// функция в моделе Spalart - Allmaras 
        /// </summary>
        protected double[] f_t2;
        /// <summary>
        /// функция в моделе Spalart - Allmaras 
        /// </summary>
        protected double[] f_v2;
        /// <summary>
        /// функция в моделе Spalart - Allmaras 
        /// </summary>
        protected double[] f_w;

        #endregion
        IMWCrossSection wwMesh;
        /// <summary>
        /// Список узлов для КЭ
        /// </summary>
        protected TriElement[] eKnots;
        /// <summary>
        /// Второй инвариант тензора вихря на КЭ
        /// </summary>
        protected double[] eOmegaII = null;
        /// <summary>
        /// Второй инвариант тензора вихря в узлах
        /// </summary>
        protected double[] OmegaII = null;

        /// <summary>
        /// Задача для расчета вязкости
        /// </summary>
        protected CTransportEquationsTri taskNu = null;
        public DiffTurbulentModelTri(IMWCrossSection wMesh, 
                IAlgebra algebra, TypeTask typeTask = TypeTask.streamY1D) :
            base(wMesh, algebra, typeTask)
        {
            wwMesh = wMesh;
            eKnots = mesh.GetAreaElems();
            taskNu = new CTransportEquationsTri(wMesh, algebra, TypeTask.streamY1D);
            MEM.Alloc(mesh.CountElements, ref eOmegaII);

            MEM.Alloc(mesh.CountKnots, ref OmegaII);
            MEM.Alloc(mesh.CountKnots, ref f_t2);
            MEM.Alloc(mesh.CountKnots, ref f_v2);
            MEM.Alloc(mesh.CountKnots, ref f_w);
        }

        /// <summary>
        /// Конвертация вязкости в моделе Spalart - Allmaras 
        /// </summary>
        /// <param name="eddyViscosity"></param>
        /// <param name="eddyViscosity_t"></param>
        public void Convert(ref double[] eddyViscosity, double[] eddyViscosity_t)
        {
            double nu = SPhysics.nu;
            for (int nod = 0; nod < mesh.CountKnots; nod++)
            {
                double chi = eddyViscosity_t[nod] / nu;
                double chi3 = chi * chi * chi;
                double fv1 = chi3 / (chi3 + Cw1_3);
                eddyViscosity[nod] = fv1* eddyViscosity_t[nod];
            }
        }

        /// <summary>
        /// турбулентная вязкость модель Spalart - Allmaras
        /// </summary>
        /// <param name="eddyViscosity">турбулентная вязкость</param>
        /// <param name="Ux">скорость по X</param>
        /// <param name="Uy">скорость по Y</param>
        /// <param name="Uz">скорость по Z</param>
        public virtual void CalkEddyViscosity_SpalartAllmaras(ref double[] eddyViscosity, double[] eddyViscosity0, 
                                                              double[] Ux, double[] Uy, double[] Uz)
        {
            for (int elem = 0; elem < mesh.CountElements; elem++)
            {
                double[] b = dNdx[elem];
                double[] c = dNdy[elem];
                uint i0 = eKnots[elem].Vertex1;
                uint i1 = eKnots[elem].Vertex2;
                uint i2 = eKnots[elem].Vertex3;
                double dUx_dy = (Ux[i0] * b[0] + Ux[i1] * b[1] + Ux[i2] * b[2]) * S[elem];
                double dUx_dz = (Ux[i0] * c[0] + Ux[i1] * c[1] + Ux[i2] * c[2]) * S[elem];
                double dUz_dy = (Uz[i0] * b[0] + Uz[i1] * b[1] + Uz[i2] * b[2]) * S[elem];
                double dUy_dz = (Uy[i0] * c[0] + Uy[i1] * c[1] + Uy[i2] * c[2]) * S[elem];
                double e = dUy_dz - dUz_dy;
                eOmegaII[elem] = Math.Sqrt(2 * (dUx_dy * dUx_dy + dUx_dz * dUx_dz + e * e));
            }
            wMesh.ConvertField(ref OmegaII, eOmegaII);
            double nu = SPhysics.nu;
            double[] d = wwMesh.GetDistance();
            // расчет функций модели Spalart - Allmaras
            for (int nod = 0; nod < mesh.CountKnots; nod++)
            {
                double chi = eddyViscosity0[nod] / nu;
                double chi3 = chi * chi * chi;
                double f_v1 = chi3 / (chi3 + Cw1_3);
                f_t2[nod] = Ct3 * Math.Exp(-Ct4 * chi * chi);
                f_v2[nod] = 1 - chi / (1 + chi * f_v1);
                double Z = OmegaII[nod] * d[nod] * d[nod] * kappa2;
                double r = Math.Min(10, eddyViscosity0[nod] / Z);
                double gw = r + Cw2 * (r * r * r * r * r * r - r);
                double ar = (1 + Cw3_6) / (Math.Pow(gw, 6) + Cw3_6);
                f_w[nod] = gw * Math.Pow(ar, 1.0 / 6);
            }
        }
        /// <summary>
        /// турбулентная вязкость модель Рэя-Агарвала 
        /// </summary>
        /// <param name="eddyViscosity">турбулентная вязкость</param>
        /// <param name="Ux">скорость по X</param>
        /// <param name="Uy">скорость по Y</param>
        /// <param name="Uz">скорость по Z</param>
        public virtual void CalkEddyViscosity_WrayAgarwal2018(ref double[] eddyViscosity, double[] eddyViscosity0, 
                                                            double[] Ux, double[] Uy, double[] Uz)
        {

        }
        /// <summary>
        /// турбулентная вязкость модель Секундова Nut - 92
        /// </summary>
        /// <param name="eddyViscosity">турбулентная вязкость</param>
        /// <param name="Ux">скорость по X</param>
        /// <param name="Uy">скорость по Y</param>
        /// <param name="Uz">скорость по Z</param>
        public virtual void CalkEddyViscosity_SecundovNut92(ref double[] eddyViscosity, double[] eddyViscosity0, 
                                                            double[] Ux, double[] Uy, double[] Uz)
        {
        

        }

    }
}
