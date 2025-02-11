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
//                    21.01.2025 Потапов И.И.
//---------------------------------------------------------------------------
//             Эксперимент: прототип "Расчетная область"
//---------------------------------------------------------------------------

namespace FEMTasksLib.AllWallFunctions
{
    using System;

    using CommonLib;
    using CommonLib.Physics;
    using GeometryLib.Vector;

    using MemLogLib;
    using MeshLib.Wrappers;

    /// <summary>
    /// Обертка для функций стенки учитывающая tri сетку
    /// </summary>
    [Serializable]
    public class WallData
    {
        public IMesh mesh = null;
        public MeshWrapperTri wMesh;
        public MWDLinkTri mWDLink;
        protected IWallFunction wallFunction;
        /// <summary>
        /// Производные от функций форм по x
        /// </summary>
        protected double[][] dNdx = null;
        /// <summary>
        /// Производные от функций форм по y
        /// </summary>
        protected double[][] dNdy = null;
        /// <summary>
        /// координаты узлов по Х
        /// </summary>
        protected double[] X = null;
        /// <summary>
        /// координаты узлов по У
        /// </summary>
        protected double[] Y = null;
        /// <summary>
        /// Список узлов для КЭ
        /// </summary>
        protected TriElement[] elems;
        /// <summary>
        /// Граничные КЭ
        /// </summary>
        protected TwoElement[] belems;
        /// <summary>
        /// Длина "контрольного объема" для граничного узла
        /// </summary>
        protected double[] knotL = null;
        public WallData(IMesh mesh, IWallFunction wallFunction)
        :this(new MeshWrapperTri(mesh), new MWDLinkTri(mesh), wallFunction) 
        { }

        public WallData(MeshWrapperTri wMesh, MWDLinkTri mWDLink, IWallFunction wallFunction)
        {
            this.wMesh = wMesh;
            mesh = wMesh.GetMesh();
            if (mesh != mWDLink.GetMesh())
                mWDLink = new MWDLinkTri(mesh);
            else
                this.mWDLink = mWDLink;
            this.wallFunction = wallFunction;
            X = mesh.GetCoords(0);
            Y = mesh.GetCoords(1);
            belems = mesh.GetBoundElems();
            elems = mesh.GetAreaElems();
            dNdx = wMesh.GetdNdx();
            dNdy = wMesh.GetdNdy();
        }

        /// <summary>
        /// Векстора касательыне к граничным элементам
        /// </summary>
        protected Vector2[] TauBElems = null;
        /// <summary>
        /// Векстора касательыне к граничным элементам
        /// </summary>
        protected Vector2[] tau = null;
        /// <summary>
        /// Векстора касательыне к граничным элементам
        /// </summary>
        protected Vector2[] normal = null;
        /// <summary>
        /// Векстора касательыне к граничным элементам
        /// </summary>
        protected Vector2[] wall = null;

        /// <summary>
        /// Метод вычисления значений граничной вязкости для 
        /// тирбулентной модели SA
        /// </summary>
        /// <param name="Phi"></param>
        /// <param name="bMu"></param>
        public void GetBoundaryWall_Mu_SA(double[] Phi, ref double[] bMu, ref double[] tauWalls)
        {
            if (TauBElems == null)
                CreateWallData();
            CreateWallData(Phi, ref bMu, ref tauWalls);
        }
        public void CreateWallData()
        {
            MEM.Alloc(belems.Length, ref TauBElems);
            MEM.Alloc(belems.Length, ref tau);
            MEM.Alloc(belems.Length, ref normal);
            MEM.Alloc(belems.Length, ref wall);
            
            MEM.Alloc(mesh.CountKnots, ref knotL);

            for (int f = 0; f < belems.Length; f++)
            {
                int i0 = (int)belems[f].Vertex1;
                int i1 = (int)belems[f].Vertex2;

                TauBElems[f] = new Vector2(X[i1] - X[i0], Y[i1] - Y[i0]);
                tau[f] = TauBElems[f]/ mWDLink.FacetsLen[f];
                // внешняя нормаль
                //normal[f] = new Vector2(tau[f].Y, - tau[f].X);
                // внутренняя нормаль
                normal[f] = new Vector2( - tau[f].Y, tau[f].X);

                int elemID = mWDLink.boundaryFacets[f].OwnerElem;
                int e0 = (int)elems[elemID].Vertex1;
                int e1 = (int)elems[elemID].Vertex2;
                int e2 = (int)elems[elemID].Vertex3;

                Vector2 V = new Vector2((X[e0] + X[e1] + X[e2]) / 3 - X[i0], 
                                        (Y[e0] + Y[e1] + Y[e2]) / 3 - Y[i0]);
                double alpha = Vector2.Dot(tau[f], V);
                double zeta  = Math.Abs( Vector2.Dot(normal[f], V) );
                //if (zeta < 0)
                //    zeta = zeta;
                if (alpha < 0)
                    alpha = alpha;

                double sss = mWDLink.FacetsLen[f];

                wall[f] = new Vector2(alpha,zeta);
                knotL[i0] += alpha;
                knotL[i1] += mWDLink.FacetsLen[f] - alpha;
            }
        }

        double[] belemMu = null;
        double[] bknotMu = null;
        double[] bknotSig = null;
        /// <summary>
        /// Получить касательыне к ганице скорости
        /// </summary>
        /// <param name="Phi">функция тока</param>
        /// <param name="bMu">вихревая вязкость сиенки в узлах границы</param>
        /// <param name="tauWalls">узловое придонное касательное напряжене</param>
        public void CreateWallData(double[] Phi, ref double[] bMu, ref double[] tauWalls)
        {
            double ks = 0;
            TwoElement[] belems = mesh.GetBoundElems();
            int[] knots = mesh.GetBoundKnots();
            MEM.VAlloc(knots.Length, 0, ref bMu);
            MEM.VAlloc(knots.Length, 0, ref tauWalls);
            MEM.VAlloc(belems.Length, 0, ref belemMu);
            MEM.VAlloc(mesh.CountKnots, 0, ref bknotMu);
            MEM.VAlloc(mesh.CountKnots, 0, ref bknotSig);
            double chi;
            double Muw;
            for (int f = 0; f < belems.Length; f++)
            {
                int elemID = mWDLink.boundaryFacets[f].OwnerElem;
                int i0 = (int)belems[f].Vertex1;
                int i1 = (int)belems[f].Vertex2;
                double[] b = dNdx[elemID];
                double[] c = dNdy[elemID];
                uint e0 = elems[elemID].Vertex1;
                uint e1 = elems[elemID].Vertex2;
                uint e2 = elems[elemID].Vertex3;
                double eVx = c[0] * Phi[e0] + c[1] * Phi[e1] + c[2] * Phi[e2];
                double eVy = -(b[0] * Phi[e0] + b[1] * Phi[e1] + b[2] * Phi[e2]);
                Vector2 Vel = new Vector2(eVx, eVy);
                double Utau = Vector2.Dot(tau[f], Vel);
                double y_plus = 0;
                double u_plus = 0;
                double tauWall = 0;
                double zeta = wall[f].Y;
                tauWall = 0;
                Muw = 0;
                if (MEM.Equals(Utau, 0) == false)
                {
                    wallFunction.Tau_wall(ref y_plus, ref u_plus, ref tauWall, Utau, zeta, ks);
                    if (u_plus > 0)
                    {
                        chi = SPhysics.kappa_w * y_plus;
                        Muw = SPhysics.nu * chi;
                    }
                }
                //
                

                bknotMu[i0] += wall[f].X * Muw;
                bknotMu[i1] += (mWDLink.FacetsLen[f] - wall[f].X) * Muw;

                bknotSig[i0] += wall[f].X * tauWall;
                bknotSig[i1] += (mWDLink.FacetsLen[f] - wall[f].X) * tauWall;
            }
            for (int k = 0; k < knots.Length; k++)
            {
                int i = knots[k];
                bknotMu[i] /= knotL[i];
                bMu[k] = bknotMu[i];
                bknotSig[i] /= knotL[i];
                tauWalls[k] = bknotSig[i];
            }    
        }
    }
}
