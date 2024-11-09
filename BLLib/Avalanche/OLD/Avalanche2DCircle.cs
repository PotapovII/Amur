//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2021 -
//                       ALL RIGHT RESERVED
//                        ПРОЕКТ "BLLib"
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                          16.05.2021
//---------------------------------------------------------------------------
namespace BLLib
{
    using CommonLib;
    using MeshLib;
    using MeshLib.Locators;
    using System;
    /// <summary>
    /// ОО: Реализация класса Avalanche2DQuad вычисляющего осыпание 2D склона из 
    /// несвязного материала. Осыпание склона происходит при превышении 
    /// уклона склона угла внутреннего трения для материала 
    /// формирующего склон.
    /// </summary>
    [Serializable]
    public class Avalanche2DCircle
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

        double[] x;
        double[] y;
        double[] z;
        /// <summary>
        /// Конструктор
        /// </summary>
        public Avalanche2DCircle(IMesh mesh, uint Nx, uint Ny, uint CountAvalanche,
                                double tanPhi, double Relax = 0.25)
        {
            this.mesh = mesh;
            this.Nx = Nx;
            this.Ny = Ny;
            this.CountAvalanche = CountAvalanche;
            this.tanPhi = tanPhi;
            this.Relax = Relax;
            x = new double[9];
            y = new double[9];
            z = new double[9];
            qmesh = new TriToQuadMesh(mesh);
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
            // ^ y
            // |
            // 7---6---5
            // |       |
            // 8   0   4
            // |       |
            // 1---2---3 ---> x
            //
            x[0] = dx; x[1] = 0; x[2] = dx;
            x[3] = 2 * dx; x[4] = 2 * dx; x[5] = 2 * dx;
            x[6] = dx; x[7] = 0; x[8] = 0;

            y[0] = dy; y[1] = 0; y[2] = 0;
            y[3] = 0; y[4] = dy; y[5] = 2 * dy;
            y[6] = 2 * dy; y[7] = 2 * dy; y[8] = dy;
            // осыпание
            for (int iav = 0; iav < CountAvalanche; iav++)
            {
                for (int row = 1; row < pole.Length - 1; row++)
                {
                    for (int col = 1; col < pole[row].Length - 1; col++)
                    {
                        // ^ y
                        // |
                        // 7---6---5
                        // |       |
                        // 8   0   4
                        // |       |
                        // 1---2---3 ---> x
                        z[0] = pole[row][col];
                        z[1] = pole[row - 1][col - 1];
                        z[2] = pole[row - 1][col];
                        z[3] = pole[row - 1][col + 1];
                        z[4] = pole[row][col + 1];
                        z[5] = pole[row + 1][col + 1];
                        z[6] = pole[row + 1][col];
                        z[7] = pole[row + 1][col - 1];
                        z[8] = pole[row][col - 1];
                        LavinaCircle(ref z);
                        pole[row][col] = z[0];
                        pole[row - 1][col - 1] = z[1];
                        pole[row - 1][col] = z[2];
                        pole[row - 1][col + 1] = z[3];
                        pole[row][col + 1] = z[4];
                        pole[row + 1][col + 1] = z[5];
                        pole[row + 1][col] = z[6];
                        pole[row + 1][col - 1] = z[7];
                        pole[row][col - 1] = z[8];
                    }
                }
            }
            qmesh.pole = pole;
            // востановление данных
            qmesh.GetValuesToMeshFormat(ref Zeta);
        }
        /// <summary>
        /// Осыпание склонов
        /// </summary>
        protected void LavinaCircle(ref double[] z)
        {
            for (int nod = 1; nod < z.Length; nod++)
            {
                double dx = Math.Sqrt((x[0] - x[nod]) * (x[0] - x[nod]) +
                                      (y[0] - y[nod]) * (y[0] - y[nod]));
                double dh = dx * tanPhi;
                double dz = z[0] - z[nod];
                if (dz < 0)
                {
                    if (-dz > dh)
                    {
                        double delta = (dh + dz) * Relax;
                        z[0] -= delta;
                        z[nod] += delta;
                    }
                }
            }
        }
    }
}

