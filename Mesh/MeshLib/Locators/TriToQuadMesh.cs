//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 15.05.2021 Потапов И.И.
//---------------------------------------------------------------------------

namespace MeshLib.Locators
{
    using System;
    using System.Linq;
    using CommonLib;
    using MemLogLib;

    /// <summary>
    /// Аппроксимация данных с треугольной не регулярной сетки
    /// на прямоугольную регулярную
    /// </summary>
    [Serializable]
    public class TriToQuadMesh
    {
        /// <summary>
        /// Сетка TriMesh
        /// </summary>
        IMesh mesh;
        /// <summary>
        /// ОО: Границы региона 
        /// </summary>
        public double Left;
        public double Right;
        public double Bottom;
        public double Top;
        /// <summary>
        /// узлов сетки по координате Х
        /// </summary>
        uint Nx;
        /// <summary>
        /// узлов сетки по координате У
        /// </summary>
        uint Ny;
        /// <summary>
        /// шаг сетки
        /// </summary>
        public double dx, dy;
        /// <summary>
        /// Поле на сетке
        /// </summary>
        public double[][] pole;
        /// <summary>
        /// Флаг для поля
        /// </summary>
        public bool[,] flagPole;
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
        double[] Value = { 0, 0, 0 };

        public double Width
        {
            get { return this.Right - this.Left; }
        }
        public double Height
        {
            get { return this.Top - this.Bottom; }
        }
        public TriToQuadMesh(IMesh mesh)
        {
            this.mesh = mesh;
            mesh.MinMax(0, ref Left, ref Right);
            mesh.MinMax(1, ref Bottom, ref Top);
            x = mesh.GetCoords(0);
            y = mesh.GetCoords(1);
        }

        /// <summary>
        /// Метод интерпляции с нерегулярной трехузловой КЭ сетки на 
        /// регулярную секу в прямоугольнике
        /// </summary>
        /// <param name="Nx"></param>
        /// <param name="Ny"></param>
        /// <param name="Zeta"></param>
        /// <returns></returns>
        public void Approx(uint Nx, uint Ny, double[] Zeta)
        {
            bool flagIn;
            bool tri = false;
            uint Imin, Imax; // для y
            uint Jmin, Jmax; // для x
            this.Nx = Nx;
            this.Ny = Ny;
            dx = Width / (Nx - 1);
            dy = Height / (Ny - 1);
            flagPole = new bool[Ny, Nx];
            MEM.Alloc2DClear((int)Ny, (int)Nx, ref pole);
            TriElement[] elems = mesh.GetAreaElems();
            for (uint elem = 0; elem < mesh.CountElements; elem++)
            {
                mesh.ElementKnots(elem, ref knots);
                uint i = elems[elem].Vertex1;
                uint j = elems[elem].Vertex2;
                uint k = elems[elem].Vertex3;
                // площадь
                double S = (x[i] * (y[j] - y[k]) + x[j] * (y[k] - y[i]) + x[k] * (y[i] - y[j])) / 2.0;

                mesh.ElemValues(Zeta, elem, ref Value);

                //Координаты 
                mesh.GetElemCoords(elem, ref ex, ref ey);
                

                // Для треугольника существует описывающий его квадрат, 
                double maxX = ex.Max() - Left;
                double minX = ex.Min() - Left;
                double maxY = ey.Max() - Bottom;
                double minY = ey.Min() - Bottom;

                // который позволяет определить узлы, которые необходимо
                // просмотреть на предмет попадания их в треугольник.
                Imin = (uint)(minY / dy);
                Imax = (uint)(maxY / dy) + 1;
                //Imax = (Imax > Ny - 1) ? Ny - 1 : Imax;
                Imax = (Imax > Ny) ? Ny : Imax;
                Jmin = (uint)(minX / dx);
                Jmax = (uint)(maxX / dx) + 1;
                //Jmax = (Jmax > Nx - 1) ? Nx - 1 : Jmax;
                Jmax = (Jmax > Nx) ? Nx : Jmax;

                // цикл по хешированным узлам поля
                for (uint pi = Imin; pi < Imax; pi++)
                {
                    double yc = dy * pi + Bottom;
                    for (uint pj = Jmin; pj < Jmax; pj++)
                    {
                        if (flagPole[pi, pj] == true) continue;
                        double xc = dx * pj + Left;
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
                        {
                            // вычисление функций формы для узла
                            double N1 = (x[j] * y[k] - x[k] * y[j]) + (y[j] - y[k]) * xc + (x[k] - x[j]) * yc;
                            double N2 = (x[k] * y[i] - x[i] * y[k]) + (y[k] - y[i]) * xc + (x[i] - x[k]) * yc;
                            double N3 = (x[i] * y[j] - x[j] * y[i]) + (y[i] - y[j]) * xc + (x[j] - x[i]) * yc;
                            // интерполяция
                            pole[pi][pj] = (N1 * Value[0] + N2 * Value[1] + N3 * Value[2]) / (2 * S);
                            flagPole[pi, pj] = true;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Перенос данных с прямоугольной сетки на нерегулярную сетку
        /// </summary>
        /// <param name="Zeta"></param>
        public void GetValuesToMeshFormat(ref double[] Zeta)
        {
            MEM.Alloc<double>(mesh.CountKnots, ref Zeta);
            for (uint nod = 0; nod < mesh.CountKnots; nod++)
            {
                double xc = x[nod];
                double yc = y[nod];

                uint i = (uint)((yc - Bottom) / dy);
                uint j = (uint)((xc - Left) / dx);
                // проверка на границу
                i = (i == Ny - 1) ? i - 1 : i;
                j = (j == Nx - 1) ? j - 1 : j;
                uint count = 0;
                double[] tx = { 0, 0, 0, 0 };
                double[] ty = { 0, 0, 0, 0 };
                double[] p = { 0, 0, 0, 0 };
                if (flagPole[i, j] == true)
                {
                    p[count] = pole[i][j];
                    tx[count] = j * dx + Left;
                    ty[count] = i * dy + Bottom;
                    count++;
                }
                if (flagPole[i, j + 1] == true)
                {
                    p[count] = pole[i][j + 1];
                    tx[count] = (j + 1) * dx + Left;
                    ty[count] = i * dy + Bottom;
                    count++;
                }
                if (flagPole[i + 1, j + 1] == true)
                {
                    p[count] = pole[i + 1][j + 1];
                    tx[count] = (j + 1) * dx + Left;
                    ty[count] = (i + 1) * dy + Bottom;
                    count++;
                }
                if (flagPole[i + 1, j] == true)
                {
                    p[count] = pole[i + 1][j];
                    tx[count] = j * dx + Left;
                    ty[count] = (i + 1) * dy + Bottom;
                    count++;
                }
                if (count == 4)
                {
                    double Lx1 = (xc - tx.Min()) / dx;
                    double Lx0 = 1 - Lx1;
                    double Ly1 = (yc - ty.Min()) / dy;
                    double Ly0 = 1 - Ly1;

                    double N0 = Lx0 * Ly0;
                    double N1 = Lx1 * Ly0;
                    double N2 = Lx1 * Ly1;
                    double N3 = Lx0 * Ly1;
                    Zeta[nod] = N0 * p[0] + N1 * p[1] + N2 * p[2] + N3 * p[3];
                }
                else
                    if (count == 3)
                {
                    // вычисление функций формы для узла
                    // площадь
                    double S = (tx[0] * (ty[1] - ty[2]) + tx[1] * (ty[2] - ty[0]) + tx[2] * (ty[0] - ty[1])) / 2.0;
                    double N1 = (tx[1] * ty[2] - tx[2] * ty[1]) + (ty[1] - ty[2]) * xc + (tx[2] - tx[1]) * yc;
                    double N2 = (tx[2] * ty[0] - tx[0] * ty[2]) + (ty[2] - ty[0]) * xc + (tx[0] - tx[2]) * yc;
                    double N3 = (tx[0] * ty[1] - tx[1] * ty[0]) + (ty[0] - ty[1]) * xc + (tx[1] - tx[0]) * yc;
                    // интерполяция
                    Zeta[nod] = (N1 * p[0] + N2 * p[1] + N3 * p[2]) / (2 * S);
                }
                else
                if (count == 2) // нужно сделать экстраполяцию с других подобластей, нет времени :(
                {
                    Zeta[nod] = 0.5 * (p[0] + p[1]);
                }
                else
                    Zeta[nod] = 0.5 * p[0];
            }
        }
        public void PrintPole()
        {

        }

    }
}


