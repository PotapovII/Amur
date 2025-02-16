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
//              задачи механики сплошных сред
//                - (C) Copyright 2025
//                       Потапов И.И.
//                        08.02.2025
//---------------------------------------------------------------------------
namespace FEMTasksLib
{
    using System;
    using MemLogLib;
    using CommonLib;
    using CommonLib.Mesh;
    /// <summary>
    /// Сплошная среда
    /// </summary>
    [Serializable]
    public class СontinuumMechanicsTri
    {
        /// <summary>
        /// КЭ сетка
        /// </summary>
        protected IMesh mesh;
        /// <summary>
        /// Врапер КЭ сетки
        /// </summary>
        protected IMeshWrapper wMesh;
        /// <summary>
        /// флаг задачи 0 - плоская 1 - цилиндрическая 
        /// </summary>
        protected int Sigma;
        /// <summary>
        /// минимальный радиус в случае цилиндрическая постановки
        /// </summary>
        protected double R_min;
        /// <summary>
        /// Деформации в узлах КЭ
        /// </summary>
        public double[] E_xx = null, E_xy = null, E_xz = null, E_yy = null, E_yz = null, E_zz = null;
        /// <summary>
        /// Деформации на КЭ
        /// </summary>
        public double[] eE_xx = null, eE_xy = null, eE_xz = null, eE_yy = null, eE_yz = null, eE_zz = null;

        public СontinuumMechanicsTri(IMeshWrapper wMesh, int Sigma = 0, double R_min = 0) 
        { 
            this.Sigma = Sigma;
            this.R_min = R_min;
            this.wMesh = wMesh;
            mesh = wMesh.GetMesh();

            MEM.Alloc(mesh.CountKnots, ref E_xx);
            MEM.Alloc(mesh.CountKnots, ref E_xy);
            MEM.Alloc(mesh.CountKnots, ref E_xz);
            MEM.Alloc(mesh.CountKnots, ref E_yy);
            MEM.Alloc(mesh.CountKnots, ref E_yz);
            MEM.Alloc(mesh.CountKnots, ref E_zz);

            MEM.Alloc(mesh.CountElements, ref eE_xx);
            MEM.Alloc(mesh.CountElements, ref eE_xy);
            MEM.Alloc(mesh.CountElements, ref eE_xz);
            MEM.Alloc(mesh.CountElements, ref eE_yy);
            MEM.Alloc(mesh.CountElements, ref eE_yz);
            MEM.Alloc(mesh.CountElements, ref eE_zz);
        }
        /// <summary>
        /// Нахождение полей деформаций на КЭ и их разнесение в узлы
        /// </summary>
        /// <param name="tDef"></param>
        /// <param name="E2"></param>
        /// <param name="Vx"></param>
        /// <param name="Vy"></param>
        /// <param name="Vz"></param>
        public void Calk_tensor_deformations(ref double[][] tDef, ref double[] E2, double[] Ux, double[] Vy, double[] Vz)
        {
            try
            {
                MEM.Alloc(mesh.CountKnots, 6, ref tDef, "tDef");
                var eKnots = mesh.GetAreaElems();
                var dNdx = wMesh.GetdNdx();
                var dNdy = wMesh.GetdNdy();
                var X = mesh.GetCoords(0);
                var Y = mesh.GetCoords(1);
                int NE = 5 + Sigma;
                MEM.Alloc(mesh.CountKnots, ref E2);
                if (Sigma == 1)
                {
                    MEM.Alloc(mesh.CountKnots, ref E_xx);
                    MEM.Alloc(mesh.CountElements, ref eE_xx);
                    for (uint elem = 0; elem < mesh.CountElements; elem++)
                    {
                        double[] b = dNdx[elem];
                        double[] c = dNdy[elem];
                        uint i0 = eKnots[elem].Vertex1;
                        uint i1 = eKnots[elem].Vertex2;
                        uint i2 = eKnots[elem].Vertex3;

                        double R_elem = (Y[i0] + Y[i1] * Y[i2]) / 3 + R_min;
                        eE_xx[elem] = (Vy[i0] + Vy[i1] * Vy[i2]) / (3 * R_elem);
                        double U_Y = (Ux[i0] + Ux[i1] * Ux[i2]) / (3 * R_elem);

                        eE_xy[elem] = 0.5 * ((Ux[i0] * b[0] + Ux[i1] * b[1] + Ux[i2] * b[2]) - U_Y);
                        eE_xz[elem] = 0.5 * (Ux[i0] * c[0] + Ux[i1] * c[1] + Ux[i2] * c[2]);

                        eE_yz[elem] = 0.5 * Vz[i0] * b[0] + Vz[i1] * b[1] + Vz[i2] * b[2] +
                                             Vy[i0] * c[0] + Vy[i1] * c[1] + Vy[i2] * c[2];

                        eE_yy[elem] = Vy[i0] * b[0] + Vy[i1] * b[1] + Vy[i2] * b[2];
                        eE_zz[elem] = Vz[i0] * c[0] + Vz[i1] * c[1] + Vz[i2] * c[2];
                    }
                    wMesh.ConvertField(ref E_xx, eE_xx);
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

                        eE_xy[elem] = 0.5 * (Ux[i0] * b[0] + Ux[i1] * b[1] + Ux[i2] * b[2]);
                        eE_xz[elem] = 0.5 * (Ux[i0] * c[0] + Ux[i1] * c[1] + Ux[i2] * c[2]);

                        eE_yz[elem] = 0.5 * Vz[i0] * b[0] + Vz[i1] * b[1] + Vz[i2] * b[2] +
                                             Vy[i0] * c[0] + Vy[i1] * c[1] + Vy[i2] * c[2];
                        eE_yy[elem] = Vy[i0] * b[0] + Vy[i1] * b[1] + Vy[i2] * b[2];
                        eE_zz[elem] = Vz[i0] * c[0] + Vz[i1] * c[1] + Vz[i2] * c[2];
                    }
                }
                wMesh.ConvertField(ref E_xy, eE_xy);
                wMesh.ConvertField(ref E_xz, eE_xz);
                wMesh.ConvertField(ref E_yz, eE_yz);
                wMesh.ConvertField(ref E_yy, eE_yy);
                wMesh.ConvertField(ref E_zz, eE_zz);

                for (uint k = 0; k < mesh.CountKnots; k++)
                {
                    tDef[k][0] = E_xy[k];
                    tDef[k][1] = E_xz[k];
                    tDef[k][2] = E_yz[k];
                    tDef[k][3] = E_yy[k];
                    tDef[k][4] = E_zz[k];
                    if (Sigma == 1)
                        tDef[k][5] = E_xx[k];
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
        /// Нахождение полей деформаций на КЭ и их разнесение в узлы
        /// </summary>
        /// <param name="tDef"></param>
        /// <param name="E2"></param>
        /// <param name="Vx"></param>
        /// <param name="Vy"></param>
        /// <param name="Vz"></param>
        public void CalkElementsDeformations(double[] Ux, double[] Vy, double[] Vz)
        {
            try
            {
                var eKnots = mesh.GetAreaElems();
                var dNdx = wMesh.GetdNdx();
                var dNdy = wMesh.GetdNdy();
                var X = mesh.GetCoords(0);
                var Y = mesh.GetCoords(1);
                int NE = 5 + Sigma;
                if (Sigma == 1)
                {
                    for (uint elem = 0; elem < mesh.CountElements; elem++)
                    {
                        double[] b = dNdx[elem];
                        double[] c = dNdy[elem];
                        uint i0 = eKnots[elem].Vertex1;
                        uint i1 = eKnots[elem].Vertex2;
                        uint i2 = eKnots[elem].Vertex3;

                        double R_elem = (Y[i0] + Y[i1] * Y[i2]) / 3 + R_min;
                        eE_xx[elem] = (Vy[i0] + Vy[i1] * Vy[i2]) / (3 * R_elem);
                        double U_Y = (Ux[i0] + Ux[i1] * Ux[i2]) / (3 * R_elem);

                        eE_xy[elem] = 0.5 * ((Ux[i0] * b[0] + Ux[i1] * b[1] + Ux[i2] * b[2]) - U_Y);
                        eE_xz[elem] = 0.5 * (Ux[i0] * c[0] + Ux[i1] * c[1] + Ux[i2] * c[2]);

                        eE_yz[elem] = 0.5 * Vz[i0] * b[0] + Vz[i1] * b[1] + Vz[i2] * b[2] +
                                             Vy[i0] * c[0] + Vy[i1] * c[1] + Vy[i2] * c[2];

                        eE_yy[elem] = Vy[i0] * b[0] + Vy[i1] * b[1] + Vy[i2] * b[2];
                        eE_zz[elem] = Vz[i0] * c[0] + Vz[i1] * c[1] + Vz[i2] * c[2];
                    }
                    wMesh.ConvertField(ref E_xx, eE_xx);
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

                        eE_xy[elem] = 0.5 * (Ux[i0] * b[0] + Ux[i1] * b[1] + Ux[i2] * b[2]);
                        eE_xz[elem] = 0.5 * (Ux[i0] * c[0] + Ux[i1] * c[1] + Ux[i2] * c[2]);

                        eE_yz[elem] = 0.5 * Vz[i0] * b[0] + Vz[i1] * b[1] + Vz[i2] * b[2] +
                                             Vy[i0] * c[0] + Vy[i1] * c[1] + Vy[i2] * c[2];
                        eE_yy[elem] = Vy[i0] * b[0] + Vy[i1] * b[1] + Vy[i2] * b[2];
                        eE_zz[elem] = Vz[i0] * c[0] + Vz[i1] * c[1] + Vz[i2] * c[2];
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
        }
        /// <summary>
        /// Нахождение полей деформаций на КЭ и их разнесение в узлы
        /// </summary>
        /// <param name="tDef"></param>
        /// <param name="E2"></param>
        /// <param name="Vx"></param>
        /// <param name="Vy"></param>
        /// <param name="Vz"></param>
        public void CalkKnotsDeformations()
        {
            wMesh.ConvertField(ref E_xy, eE_xy);
            wMesh.ConvertField(ref E_xz, eE_xz);
            wMesh.ConvertField(ref E_yz, eE_yz);
            wMesh.ConvertField(ref E_yy, eE_yy);
            wMesh.ConvertField(ref E_zz, eE_zz);
        }
    }
}
