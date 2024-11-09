//---------------------------------------------------------------------------
//                          ПРОЕКТ  "МКЭ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 15.07.2022 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MemLogLib;
    using CommonLib;
    /// <summary>
    /// ОО: Транспортный класс базисной конечно-элементной сетки общего назначений
    /// Назначение: 
    /// - Передача данных
    /// - Чтение/запись
    /// - Обмен между сетками различных форматов
    /// - Генерация сеток
    /// </summary>
    [Serializable]
    public class MeshCore
    {
        #region базисные массывы данных
        /// <summary>
        /// id пообласти 
        /// </summary>
        public int ID;
        /// <summary>
        /// Координаты узлов
        /// </summary>
        public double[][] points = null;
        /// <summary>
        /// Массив связности (узлы конечно - элементной сетки)
        /// следующие элементы столбца - номера узлов 
        /// хвост столбца (-1)  если сетка смешанная
        /// </summary>
        public int[][] elems = null;
        /// <summary>
        /// индекс типа - функции формы 
        /// </summary>
        public int[] fform = null;
        /// <summary>
        /// Массив границ (количество строк == количеству узлов на граничном элементе)
        /// </summary>
        public int[][] boundary = null;
        /// <summary>
        /// Массив маркеров границ 
        /// 1 строка - метка границы
        /// 2 строка - индекс типа граничного условя
        /// </summary>
        public int[][] boundaryMark = null;
        /// <summary>
        /// Значения в узлах - контексно
        /// </summary>
        public double[][] Params = null;
        #endregion
        public MeshCore() { ID = 0; }
        public MeshCore(MeshCore mesh)
        {
            ID = mesh.ID;
            Set(mesh.points, mesh.elems, mesh.fform, mesh.boundary, mesh.boundaryMark, mesh.Params);
        }
        public MeshCore(double[][] points, int[][] elems,int[] fform, int[][] boundary, int[][] boundaryMark, double[][] Params,int ID = 0)
        {
            this.ID = ID;
            Set(points, elems, fform, boundary, boundaryMark, Params);
        }
        public void Set(double[][] _points, int[][] _elems, int[] _fform, int[][] _boundary, int[][] _boundaryMark, double[][] _fields)
        {
            MEM.MemCopy(ref points, _points);
            MEM.MemCopy(ref elems, _elems);
            MEM.MemCopy(ref fform, _fform);
            MEM.MemCopy(ref boundary, _boundary);
            MEM.MemCopy(ref boundaryMark, _boundaryMark);
            if( _fields != null )
                MEM.MemCopy(ref Params, _fields);
        }
        public MeshCore(FVComMesh mesh)
        {
            ID = 0;
            Set(mesh);
        }
        public void Set(FVComMesh mesh)
        {
            MEM.Alloc2D(2, mesh.CoordsX.Length, ref points);
            MEM.MemCopy(ref points[0], mesh.CoordsX);
            MEM.MemCopy(ref points[1], mesh.CoordsY);
            var ffs = mesh.AreaElems.Select(x => (uint)x.TFunForm).ToArray();
            var dffs = ffs.Distinct().ToArray();
            int cu = dffs.Length == 1 ? 3 : 4;
            /// всегда 4
            cu = 4;
            MEM.Alloc2DClear(cu, mesh.AreaElems.Length, ref elems, -1);
            MEM.Alloc(mesh.AreaElems.Length, ref fform);
            for (int j = 0; j < mesh.AreaElems.Length; j++)
            {
                for (int i = 0; i < mesh.AreaElems[j].Nodes.Length; i++)
                    elems[i][j] = mesh.AreaElems[j].Nodes[i];
                fform[j] = (int)mesh.AreaElems[j].TFunForm;
            }
            MEM.Alloc2D(2, mesh.BoundaryFacets.Length, ref boundary);
            MEM.Alloc2D(2, mesh.BoundaryFacets.Length, ref boundaryMark);
            for (int j = 0; j < mesh.BoundaryFacets.Length; j++)
            {
                boundary[0][j] = mesh.BoundaryFacets[j].Pointid1;
                boundary[1][j] = mesh.BoundaryFacets[j].Pointid2;
                boundaryMark[0][j] = (int)mesh.BoundaryFacets[j].BoundaryFacetsMark;
            }
        }
        public MeshCore(IFEMesh mesh)
        {
            Set(mesh);
        }
        public void Set(IFEMesh mesh)
        {
            ID = mesh.ID;
            MEM.Alloc2D(2, mesh.CoordsX.Length, ref points);
            MEM.MemCopy(ref points[0], mesh.CoordsX);
            MEM.MemCopy(ref points[1], mesh.CoordsY);
            TypeFunForm[] ffs = mesh.AreaElems.Select(x => x.TFunForm).ToArray();
            TypeFunForm[] dffs = ffs.Distinct().ToArray();
            int cu = FunFormHelp.GetFEKnots(dffs[0]);
            if (dffs.Length == 2)
                cu = Math.Max(cu, FunFormHelp.GetFEKnots(dffs[1]));
            MEM.Alloc2DClear(cu, mesh.AreaElems.Length, ref elems, -1);
            MEM.Alloc(mesh.AreaElems.Length, ref fform);
            for (int j = 0; j < mesh.AreaElems.Length; j++)
            {
                for (int i = 0; i < mesh.AreaElems[j].Nods.Length; i++)
                    elems[i][j] = mesh.AreaElems[j].Nods[i].ID;
                fform[j] = (int)mesh.AreaElems[j].TFunForm;
            }
            TypeFunForm ffb = FunFormHelp.GetBoundFunFormForGeometry(dffs[0]);
            int Count = FunFormHelp.GetFEKnots(ffb);
            MEM.Alloc2D(Count, mesh.CountBoundElements, ref boundary);
            MEM.Alloc2D(2, mesh.CountBoundElements, ref boundaryMark);
            for (int j = 0; j < mesh.CountBoundElements; j++)
            {
                for(int i = 0; i < Count; i++)
                    boundary[i][j] = mesh.BoundElems[j].Nods[i].ID;
                boundaryMark[0][j] = mesh.BoundElems[j].MarkBC;
            }
            if (mesh.Params != null)
                if( mesh.Params.Length > 0)
                    MEM.MemCopy(ref Params, mesh.Params);
        }
        public MeshCore(ComplecsMesh mesh)
        {
            ID = 0;
            Set(mesh);
        }
        public void Set(ComplecsMesh mesh)
        {
            MEM.Alloc2D(2, mesh.CoordsX.Length, ref points);
            MEM.MemCopy(ref points[0], mesh.CoordsX);
            MEM.MemCopy(ref points[1], mesh.CoordsY);
            var dffs = mesh.AreaElemsFFType.Distinct().ToArray();
            int cu = FunFormHelp.GetFEKnots(dffs[0]);
            if (dffs.Length == 2)
                cu = Math.Max(cu, FunFormHelp.GetFEKnots(dffs[1]));
            MEM.Alloc2DClear(cu, mesh.AreaElems.Length, ref elems, -1);
            MEM.Alloc(mesh.AreaElems.Length, ref fform);
            for (int j = 0; j < mesh.AreaElems.Length; j++)
            {
                for (int i = 0; i < mesh.AreaElems[j].Length; i++)
                    elems[i][j] = (int) mesh.AreaElems[j][i];
                fform[j] = (int)mesh.AreaElemsFFType[j];
            }
            int[] boundaryFacetsMark = mesh.GetBElementsBCMark();
            TypeFunForm ffb = FunFormHelp.GetBoundFunFormForGeometry(dffs[0]);
            int Count = FunFormHelp.GetFEKnots(ffb);
            MEM.Alloc2D(Count, mesh.CountBoundElements, ref boundary);
            MEM.Alloc2D(2, mesh.CountBoundElements, ref boundaryMark);
            for (int j = 0; j < mesh.CountBoundElements; j++)
            {
                for (int i = 0; i < Count; i++)
                    boundary[i][j] = (int)mesh.BoundElems[j][i];
                boundaryMark[0][j] = boundaryFacetsMark[j];
            }
        }
        /// <summary>
        /// Любой тип сетки конвертируем к тругольной 
        /// если не найдем других производных типпов
        /// </summary>
        /// <param name="mesh"></param>
        public MeshCore(IMesh mesh)
        {
            ID = 0;
            FVComMesh amesh = mesh as FVComMesh;
            if (amesh != null)
            {
                Set(amesh);
                return;
            }
            FEMesh femesh = mesh as FEMesh;
            if (femesh != null)
            {
                Set(femesh);
                return;
            }
            ComplecsMesh cmesh = mesh as ComplecsMesh;
            if(cmesh != null)
            {
                Set(cmesh);
                return;
            }
            Set(mesh);
        }
        public void Set(IMesh mesh)
        {
            double[] CoordsX = mesh.GetCoords(0);
            double[] CoordsY = mesh.GetCoords(1);
            MEM.Alloc2D(2, CoordsX.Length, ref points);
            MEM.MemCopy(ref points[0], CoordsX);
            MEM.MemCopy(ref points[1], CoordsY);

            TriElement[] AreaElems = mesh.GetAreaElems();
            int cu = 3;
            MEM.Alloc2DClear(cu, AreaElems.Length, ref elems, -1);
            MEM.Alloc(AreaElems.Length, ref fform);
            for (int j = 0; j < AreaElems.Length; j++)
            {
                elems[0][j] = (int)AreaElems[j].Vertex1;
                elems[1][j] = (int)AreaElems[j].Vertex2;
                elems[2][j] = (int)AreaElems[j].Vertex3;
                fform[j] = (int)TypeFunForm.Form_2D_Triangle_L1;
            }
            TwoElement[] BoundElems = mesh.GetBoundElems();
            int[] boundaryFacetsMark = mesh.GetBElementsBCMark();

            MEM.Alloc2D(2, mesh.CountBoundElements, ref boundary);
            MEM.Alloc2D(2, mesh.CountBoundElements, ref boundaryMark);
            for (int j = 0; j < mesh.CountBoundElements; j++)
            {
                boundary[0][j] = (int)BoundElems[j].Vertex1;
                boundary[1][j] = (int)BoundElems[j].Vertex2;
                boundaryMark[0][j] = boundaryFacetsMark[j];
            }
        }
        public MeshCore(TriMesh mesh)
        {
            Set(mesh);
        }
        /// <summary>
        /// Приведение произвольной КЭ сетки к симплекс сетки
        /// </summary>
        /// <param name="a"></param>
        public static explicit operator FEMesh(MeshCore m)
        {
            List<int> knots = new List<int>();
            int[] nods = null;
            int MarkBC = 0;
            TypeFunForm ff = TypeFunForm.Form_2D_Triangle_L1;
            FEMesh mesh = new FEMesh();
            mesh.ID = m.ID;
            mesh.AreaElems = new IFElement[m.elems[0].Length];
            for(int e = 0; e < m.elems[0].Length; e++)
            {
                for (int k = 0; k < m.elems.Length; k++)
                {
                    if (m.elems[k][e] > -1)
                        knots.Add(m.elems[k][e]);
                    else
                        break;
                }
                nods = knots.ToArray(); knots.Clear();
                ff = (TypeFunForm)m.fform[e];
                mesh.AreaElems[e] = new FElement(nods, e, MarkBC, ff);
            }
            mesh.BoundElems = new IFElement[m.boundary[0].Length];
            int cu = m.boundary.Length;
            TypeFunForm ffb = FunFormHelp.GetBoundFunFormForGeometry(ff);
            for (int e = 0; e < m.boundary[0].Length; e++)
            {
                for (int k = 0; k < m.boundary.Length; k++)
                {
                    if (m.boundary[k][e] > -1)
                        knots.Add(m.boundary[k][e]);
                    else
                        break;
                }
                nods = knots.ToArray(); knots.Clear();
                MarkBC = m.boundaryMark[0][e];
                mesh.BoundElems[e] = new FElement(nods, e, MarkBC, ffb);
            }
            mesh.BNods = new IFENods[(m.boundary.Length-1) * m.boundary[0].Length];
            int n = 0;
            for (int e = 0; e < m.boundary[0].Length; e++)
            {
                for (int k = 0; k < m.boundary.Length-1; k++)
                {
                    mesh.BNods[n++] = new FENods(m.boundary[k][e], m.boundaryMark[0][e]);
                }
            }
            mesh.coordsX = new double[m.points[0].Length];
            mesh.coordsY = new double[m.points[0].Length];
            for (n = 0; n < m.points[0].Length; n++)
            {
                mesh.coordsX[n] = m.points[0][n];
                mesh.coordsY[n] = m.points[1][n];
            }
            if (m.Params != null)
                if (m.Params[0].Length > 0)
                {
                    double[][] p = null;
                    MEM.MemCopy(ref p, m.Params);
                    mesh.Params = p;
                }
            //mesh.Print();
            return mesh;
        }

        public static explicit operator ComplecsMesh(MeshCore m)
        {
            ComplecsMesh mesh = new ComplecsMesh();
            TypeFunForm ff = TypeFunForm.Form_2D_Triangle_L1;
            int CountElems = m.elems[0].Length;
            MEM.Alloc(CountElems, ref mesh.AreaElemsFFType);
            mesh.AreaElems = new uint[CountElems][];
            for (int e = 0; e < m.elems[0].Length; e++)
            {
                List<uint> knots = new List<uint>();
                for (int k = 0; k < m.elems.Length; k++)
                {
                    if (m.elems[k][e] > -1)
                        knots.Add((uint)m.elems[k][e]);
                    else
                        break;
                }
                uint[] nods = knots.ToArray(); knots.Clear();
                ff = (TypeFunForm)m.fform[e];
                mesh.AreaElems[e] = nods;
                mesh.AreaElemsFFType[e] = ff;
            }
            int CountBoundElems = m.boundary[0].Length;
            int cu = m.boundary.Length;
            MEM.Alloc2D(CountBoundElems, cu, ref mesh.BoundElems);
            MEM.Alloc(CountBoundElems, ref mesh.BoundElementsMark);
            TypeFunForm ffb = FunFormHelp.GetBoundFunFormForGeometry(ff);
            for (int e = 0; e < CountBoundElems; e++)
            {
                List<uint> knots = new List<uint>();
                for (int k = 0; k < cu; k++)
                {
                    if (m.boundary[k][e] > -1)
                        knots.Add((uint)m.boundary[k][e]);
                    else
                        break;
                }
                uint[] nods = knots.ToArray(); knots.Clear();
                mesh.BoundElementsMark[e] = m.boundaryMark[0][e];
                mesh.BoundElems[e] = nods;
            }
            int mCu = cu - 1;
            MEM.Alloc(CountBoundElems * mCu, ref mesh.BoundKnotsMark);
            MEM.Alloc(CountBoundElems * mCu, ref mesh.BoundKnots);
            int n = 0;
            for (int e = 0; e < CountBoundElems; e++)
            {
                for (int k = 0; k < mCu; k++)
                {
                    mesh.BoundKnots[n] = m.boundary[k][e];
                    mesh.BoundKnotsMark[n] = m.boundaryMark[0][e];
                    n++;
                }
            }
            MEM.MemCopy(ref mesh.CoordsX, m.points[0]);
            MEM.MemCopy(ref mesh.CoordsY, m.points[1]);
            return mesh;
        }
        public void Print()
        {
            LOG.Print(" Координаты ", points);
            LOG.Print(" Элементы ", elems);
            LOG.Print(" Типы функцуий форм для элементов ", fform);
            LOG.Print(" Граничные элементы ", boundary);
            LOG.Print(" Метки граничных элементов ", boundaryMark);
            if (Params != null)
                if (Params[0].Length > 0)
                    LOG.Print(" Поля ", Params);
        }
    }
}
