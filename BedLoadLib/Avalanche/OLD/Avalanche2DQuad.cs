//---------------------------------------------------------------------------
//                 Реализация библиотеки для моделирования 
//                  гидродинамических и русловых процессов
//---------------------------------------------------------------------------
//                 кодировка : 16.05.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace BedLoadLib
{
    using System;
    using CommonLib;
    using MeshLib;
    using MeshLib.Locators;
    /// <summary>
    /// ОО: Реализация класса Avalanche2DQuad вычисляющего осыпание 2D склона из 
    /// несвязного материала. Осыпание склона происходит при превышении 
    /// уклона склона угла внутреннего трения для материала 
    /// формирующего склон.
    /// </summary>
    [Serializable]
    public class Avalanche2DQuad
    {
        /// <summary>
        /// Аппроксимация данных с треугольной не регулярной сетки
        /// на прямоугольную регулярную
        /// </summary>
        TriToQuadMesh qmesh;
        /// <summary>
        /// Сетка для донора данных
        /// </summary>
        IMesh mesh;
        /// <summary>
        /// Релаксация - доля обвала за один проход
        /// </summary>
        double Relax;
        /// <summary>
        /// тангенс угла внутреннено трения
        /// </summary>
        double tanPhi;
        /// <summary>
        /// Релаксация - доля обвала за один проход
        /// </summary>
        double CountAvalanche;
        /// <summary>
        /// Размер регулярной сетки по Х
        /// </summary>
        uint Nx = 0;
        /// <summary>
        /// Размер регулярной сетки по Y
        /// </summary>
        uint Ny = 0;
        double[][] pole;
        double dx, dy;
        /// <summary>
        /// Конструктор
        /// </summary>
        public Avalanche2DQuad(IMesh mesh, uint Nx, uint Ny, uint CountAvalanche,
                                double tanPhi, double Relax = 0.5)
        {
            this.mesh = mesh;
            this.Nx = Nx;
            this.Ny = Ny;
            this.CountAvalanche = CountAvalanche;
            this.tanPhi = tanPhi;
            this.Relax = Relax;
            qmesh = new TriToQuadMesh((TriMesh)mesh);
        }

        /// <summary>
        /// Метод лавинного обрушения рельефа 
        /// </summary>
        /// <param name="Z">массив донных отметок</param>
        /// <param name="ds">координаты узлов</param>
        /// <param name="tf">тангенс внутреннено трения **</param>
        /// <param name="Step">шаг остановки процесса, 
        /// при 0 лавина проходит полностью</param>
        public void Lavina(ref double[] Zeta)
        {
            // проекция нерегулярной сетки на ренулярную
            qmesh.Approx(Nx, Ny, Zeta);
            pole = qmesh.pole;
            dx = qmesh.dx;
            dy = qmesh.dy;
            // осыпание
            for (int i = 0; i < CountAvalanche; i++)
            {
                LavinaRight();
                LavinaLeft();
                LavinaUp();
                LavinaDown();
            }
            qmesh.pole = pole;
            // востановление данных
            qmesh.GetValuesToMeshFormat(ref Zeta);
        }
        /// <summary>
        /// Осыпание правого склона
        /// </summary>
        protected void LavinaRight()
        {
            for (int row = 0; row < pole.Length; row++)
                for (int i = 1; i < pole[row].Length; i++)
                {
                    double dh = dx * tanPhi;
                    double dz = pole[row][i] - pole[row][i - 1];
                    if (dz < 0)
                    {
                        if (-dz > dh)
                        {
                            double delta = (dh + dz) * Relax;
                            pole[row][i] -= delta;
                            pole[row][i - 1] += delta;
                        }
                    }
                }
        }
        protected void LavinaLeft()
        {
            for (int row = 0; row < pole.Length; row++)
                for (int i = 1; i < pole[row].Length; i++)
                {
                    double dh = dx * tanPhi;
                    int k = pole[row].Length - i - 1;
                    int kp = pole[row].Length - i;
                    double dz = pole[row][kp] - pole[row][k];
                    if (dz > 0)
                    {
                        if (dz > dh)
                        {
                            double delta = (dz - dh) * Relax;
                            pole[row][k] += delta;
                            pole[row][kp] -= delta;
                        }
                    }
                }
        }
        protected void LavinaDown()
        {
            for (int col = 0; col < pole[0].Length; col++)
                for (int i = 1; i < pole.Length; i++)
                {
                    double dh = dy * tanPhi;
                    double dz = pole[i][col] - pole[i - 1][col];
                    if (dz < 0)
                    {
                        if (-dz > dh)
                        {
                            double delta = (dh + dz) * Relax;
                            pole[i][col] -= delta;
                            pole[i - 1][col] += delta;
                        }
                    }
                }
        }
        protected void LavinaUp()
        {
            for (int col = 0; col < pole[0].Length; col++)
                for (int i = 1; i < pole.Length; i++)
                {
                    double dh = dx * tanPhi;
                    int k = pole.Length - i - 1;
                    int kp = pole.Length - i;
                    double dz = pole[kp][col] - pole[k][col];
                    if (dz > 0)
                    {
                        if (dz > dh)
                        {
                            double delta = (dz - dh) * Relax;
                            pole[k][col] += delta;
                            pole[kp][col] -= delta;
                        }
                    }
                }
        }


        protected void LavinaRightDoun()
        {

        }


    }
}
