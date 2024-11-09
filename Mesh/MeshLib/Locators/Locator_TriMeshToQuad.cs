//---------------------------------------------------------------------------
//                          ПРОЕКТ  "МКЭ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 21.06.2022 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshLib.Locators
{
    using System;
    using CommonLib;
    using GeometryLib;
    using MemLogLib;
    /// <summary>
    /// Класс для интерполяции полей с нерегулярной сетки источника 
    /// на регулярную квадратную сетку приемник с постоянным шагом dx, dy
    /// </summary>
    public class Locator_TriMeshToQuad
    {
        /// <summary>
        /// Сетка источник
        /// </summary>
        protected IMesh mesh = null;
        /// <summary>
        /// Сетка приемник 
        /// </summary>
        public RectFVMesh ReverseQMesh => qmesh;
        /// <summary>
        /// Сетка приемник
        /// </summary>
        RectFVMesh qmesh = null;
        /// <summary>
        /// Связи между узлами ReverseQMesh и КЭ TriMesh
        /// </summary>
        public ILink[] Lint = null;
        public int[][] linkFE = null;
        /// <summary>
        /// Координаты узлов привязанных к TriMesh
        /// </summary>
        double[] x;
        double[] y;
        /// <summary>
        /// узлы треугольника, массивы для работы
        /// </summary>
        uint[] knots = { 0, 0, 0 };
        double[] ex = { 0, 0, 0 };
        double[] ey = { 0, 0, 0 };
        /// <summary>
        /// ОО: Границы региона 
        /// </summary>
        public double Left;
        public double Right;
        public double Bottom;
        public double Top;
        double dx;
        double dy;
        double xa = 0;
        double xb = 0;
        double ya = 0;
        double yb = 0;
        public Locator_TriMeshToQuad(IMesh mesh)
        {
            this.mesh = mesh;
            TriMesh m = mesh as TriMesh;
            if (m != null)
            {
                x = mesh.GetCoords(0);
                y = mesh.GetCoords(1);
                Left = x[0];
                Right = x[0];
                Bottom = y[0];
                Top = y[0];
                for (int i=1; i<x.Length;i++)
                {
                    Left = Math.Min(Left, x[i]);
                    Right = Math.Max(Right, x[i]);
                    Bottom = Math.Min(Bottom, y[i]);
                    Top = Math.Max(Top, y[i]);
                }
            }
            else
                Logger.Instance.Error("mesh as TriMesh == null", "Locator_TriMeshToQuad.Locator_TriMeshToQuad()");
        }
        /// <summary>
        /// Построение 
        /// </summary>
        /// <param name="mesh"></param>
        public void CreateMesh(int Imax = 0, int Jmax = 0)
        {
            try
            {
                Imax = Imax == 0 ? MEM.CountUniqueElems(x) : Imax;
                Jmax = Jmax == 0 ? MEM.CountUniqueElems(y) : Jmax;
                double[] tx = mesh.GetCoords(0);
                double[] ty = mesh.GetCoords(1);
                Locator.FindMinStep(tx, ref Imax, ref xa, ref xb);
                Locator.FindMinStep(ty, ref Jmax, ref ya, ref yb);
                double Lx = xb - xa;
                double Ly = yb - ya;
                // Создание сетки
                qmesh = new RectFVMesh(Imax, Jmax, xa, xb, ya, yb);
                // Создание связей
                CreateLinks(Lx, Ly);
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
        }
        /// <summary>
        /// Метод интерпляции с нерегулярной трехузловой КЭ сетки на 
        /// регулярную секу в прямоугольнике
        /// </summary>
        /// <param name="Nx"></param>
        /// <param name="Ny"></param>
        /// <param name="Zeta"></param>
        /// <returns></returns>
        public void CreateLinks(double Lx, double Ly)
        {
            try
            {
                bool flagIn;
                bool tri = false;
                uint Imin, Imax; // для y
                uint Jmin, Jmax; // для x
                int Nx = qmesh.Nx;
                int Ny = qmesh.Ny;
                MEM.Alloc2DClear(Nx, Ny, ref linkFE, -1);
                dx = Lx / (Nx - 1);
                dy = Ly / (Ny - 1);
                TriElement[] elems = mesh.GetAreaElems();
                for (uint elem = 0; elem < mesh.CountElements; elem++)
                {
                    uint i = elems[elem].Vertex1;
                    uint j = elems[elem].Vertex2;
                    uint k = elems[elem].Vertex3;
                    // площадь
                    double S = (x[i] * (y[j] - y[k]) + x[j] * (y[k] - y[i]) + x[k] * (y[i] - y[j])) / 2.0;
                    // Координаты 
                    double maxX = Math.Max(Math.Max(x[i], x[j]), x[k]) - Left;
                    double minX = Math.Min(Math.Min(x[i], x[j]), x[k]) - Left;
                    double maxY = Math.Max(Math.Max(y[i], y[j]), y[k]) - Bottom;
                    double minY = Math.Min(Math.Min(y[i], y[j]), y[k]) - Bottom;
                    //mesh.ElemX(elem, ref ex);
                    //mesh.ElemY(elem, ref ey);
                    //// Для треугольника существует описывающий его квадрат, 
                    //double maxX = ex.Max() + Left;
                    //double minX = ex.Min() + Left;
                    //double maxY = ey.Max() + Bottom;
                    //double minY = ey.Min() + Bottom;
                    // который позволяет определить узлы, которые необходимо
                    // просмотреть на предмет попадания их в треугольник.
                    Jmin = (uint)(minY / dy);
                    Jmax = (uint)(maxY / dy) + 1;
                    //Imax = (Imax > Ny - 1) ? Ny - 1 : Imax;
                    Jmax = (Jmax > Ny) ? (uint)Ny : Jmax;
                    
                    Imin = (uint)(minX / dx);
                    Imax = (uint)(maxX / dx) + 1;
                    //Jmax = (Jmax > Nx - 1) ? Nx - 1 : Jmax;
                    Imax = (Imax > Nx) ? (uint)Nx : Imax;
                    // цикл по хешированным узлам поля
                    for (uint pi = Imin; pi < Imax; pi++)
                    {
                        double xc = dx * pi + Left;
                        for (uint pj = Jmin; pj < Jmax; pj++)
                        {
                            if (linkFE[pi][pj] > -1) continue;
                            double yc = dy * pj+ Bottom;
                            if (tri == true)
                            {
                                // Определения принадлежности точки xc, yc по площади
                                double S1 = (xc * (y[j] - y[k]) + x[j] * (y[k] - yc) + x[k] * (yc - y[j])) / 2.0;
                                double S2 = (x[i] * (yc - y[k]) + xc * (y[k] - y[i]) + x[k] * (y[i] - yc)) / 2.0;
                                double S3 = (x[i] * (y[j] - yc) + x[j] * (yc - y[i]) + xc * (y[i] - y[j])) / 2.0;
                                flagIn = (Math.Abs(S - S1 - S2 - S3) < MEM.Error8);
                            }
                            else
                            {
                                // Определения принадлежности точки xc, yc 
                                // Для того, чтобы точка xc, yc принадлежала данному треугольнику, необходимо, 
                                // чтобы псевдоскалярное(косое) произведение соответствующих векторов 
                                // было больше или же меньше нуля.
                                double n1 = (y[j] - y[i]) * (xc - x[i]) - (x[j] - x[i]) * (yc - y[i]);
                                double n2 = (y[k] - y[j]) * (xc - x[j]) - (x[k] - x[j]) * (yc - y[j]);
                                double n3 = (y[i] - y[k]) * (xc - x[k]) - (x[i] - x[k]) * (yc - y[k]);
                                flagIn = ((n1 >= 0) && (n2 >= 0) && (n3 >= 0)) || ((n1 <= 0) && (n2 <= 0) && (n3 <= 0));
                            }
                            // узел в треугольнике?
                            // Для узлов попавших треугольник выполняем интерполяцию в 
                            // них значений с поля, заданного на треугольнике.
                            if (flagIn == true)
                                linkFE[pi][pj] = (int)elem;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
        }
        /// <summary>
        /// Проекция двумерного Tri поля в двумерное Q  поля 
        /// </summary>
        /// <param name="value2D"></param>
        /// <param name="source2D"></param>
        public void GetValue_2DQfrom2DT(ref double[][] value2DQ, double[] source2DT)
        {
            if (source2DT == null) return;
            MEM.Alloc2D(qmesh.Nx, qmesh.Ny, ref value2DQ);
            TriElement[] elems = mesh.GetAreaElems();
            for (int pi = 0; pi < qmesh.Nx; pi++)
            {
                double yc = dy * pi + Bottom;
                for (int pj = 0; pj < qmesh.Ny; pj++)
                {
                    double xc = dx * pj + Left;
                    uint elem = (uint)linkFE[pi][pj];
                    uint i = elems[elem].Vertex1;
                    uint j = elems[elem].Vertex2;
                    uint k = elems[elem].Vertex3;
                    // площадь
                    double S = (x[i] * (y[j] - y[k]) + x[j] * (y[k] - y[i]) + x[k] * (y[i] - y[j])) / 2.0;
                    double N1 = (x[j] * y[k] - x[k] * y[j]) + (y[j] - y[k]) * xc + (x[k] - x[j]) * yc;
                    double N2 = (x[k] * y[i] - x[i] * y[k]) + (y[k] - y[i]) * xc + (x[i] - x[k]) * yc;
                    double N3 = (x[i] * y[j] - x[j] * y[i]) + (y[i] - y[j]) * xc + (x[j] - x[i]) * yc;
                    // интерполяция
                    value2DQ[pi][pj] = (N1 * source2DT[i] + N2 * source2DT[j] + N3 * source2DT[k]) / (2 * S);
                }
            }
        }
        /// <summary>
        /// Проекция одномерного поля в двумерное
        /// </summary>
        /// <param name="value2D"></param>
        /// <param name="source1D"></param>
        public void SetValue_2DTfrom2DQ(ref double[] value2DT, double[][] source2DQ)
        {
            for (int i = 0; i < mesh.CountKnots; i++)
            {
                double xi = x[i];
                double yi = y[i];

                int pi = (int)((xi - Left) / dx) - 1;
                int pj = (int)((yi - Bottom) / dy) - 1;
                pi = pi < 0 ? 0 : pi;
                pj = pj < 0 ? 0 : pj;

                double x0 = dx * pi + Left;
                double y0 = dy * pj + Bottom;
                
                double Nx1 = (x[i] - x0) / dx;
                double Nx0 = 1 - Nx1;
                double Ny1 = (y[i] - y0) / dy;
                double Ny0 = 1 - Ny1;

                double N1 = Nx0 * Ny0;
                double N2 = Nx1 * Ny0;
                double N3 = Nx1 * Ny1;
                double N4 = Nx0 * Ny1;
                double V1 = source2DQ[pi][pj];
                double V2 = source2DQ[pi + 1][pj];
                double V3 = source2DQ[pi + 1][pj + 1];
                double V4 = source2DQ[pi][pj + 1];
                double value = V1 * N1 + V2 * N2 + V3 * N3 + V4 * N4;
                value2DT[i] = value;
            }
        }
    }
}
