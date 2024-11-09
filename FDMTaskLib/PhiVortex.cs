//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//             кодировка : 04.10.24 Потапов И.И.
//---------------------------------------------------------------------------
namespace FDMTaskLib
{
    using System;
    using MemLogLib;
    using MeshLib.TaskArea;
    /// <summary>
    /// Решение задачи вынужденной конвекции в 
    /// постановке вихрь функция тока для задачи Стокса
    /// в прямоугольной области
    /// </summary>
    public class PhiVortex : RectangleArea
    {
        /// <summary>
        /// Макимальное количество итераций
        /// </summary>
        public int M = 1000;
        /// <summary>
        /// функция тока
        /// </summary>
        public double[,] Phi, Phi0;
        /// <summary>
        /// Функция вихря
        /// </summary>
        public double[,] Vortex, Vortex0;
        /// <summary>
        /// поле скоростей
        /// </summary>
        public double[,] mV, mW;
        /// <summary>
        /// Скорость крышки
        /// </summary>
        public double V = 0.5;
        public PhiVortex(int Nx, int Ny, double Lx, double Ly, double V) : base (Nx, Ny, Lx, Ly)
        {
            this.V = V;
            MEM.Alloc2D(Nx, Ny, ref Phi);
            MEM.Alloc2D(Nx, Ny, ref Vortex);
            MEM.Alloc2D(Nx, Ny, ref Phi0);
            MEM.Alloc2D(Nx, Ny, ref Vortex0);
            MEM.Alloc2D(Nx, Ny, ref mV);
            MEM.Alloc2D(Nx, Ny, ref mW);
        }

        public void Solver(bool print = false)
        {
            Console.Clear();
            for (int n = 0; n < 1000; n++)
            {
                Copy(ref Phi0, Phi);
                Copy(ref Vortex0, Vortex);
                SolverVortex();
                Relax(ref Vortex, Vortex0);
                SolverPhi();
                Relax(ref Phi, Phi0);
                if (print == true)
                {
                    LOG.Print("Phi", Phi, 6);
                    LOG.Print("Vortex", Vortex, 6);
                }
                Console.WriteLine("Итерация " + n.ToString());
                if (Error(Vortex0, Vortex) < 0.00001)
                {
                    Velocity();
                    if (print == true)
                    {
                        LOG.Print("Phi", Phi, 6);
                        LOG.Print("Vortex", Vortex, 6);
                        LOG.Print("Vx", mV, 6);
                        LOG.Print("Vy", mW, 6);
                    }
                    //Console.WriteLine("Итерация " + n.ToString());
                    break;
                }
            }
            if (print == true)
            {
                Console.WriteLine("Нажать любую клавишу");
                Console.ReadKey();
            }
        }
        public void SolverPhi()
        {
            double ErrPhi = 0, Phi_Old, Phi_Max = 0;
            for (int n = 0; n < M; n++)
            {
                ErrPhi = 0; 
                Phi_Max = 0;
                for (int i = 1; i < Nx - 1; i++)
                    for (int j = 1; j < Ny - 1; j++)
                    {
                        Phi_Old = Phi[i, j];
                        Phi[i, j] = 0.25 * (Phi[i + 1, j] + Phi[i - 1, j] + Phi[i, j + 1] + Phi[i, j - 1]) + 0.25 * dS * Vortex[i, j];
                        ErrPhi = Math.Max(ErrPhi, Math.Abs(Phi[i, j] - Phi_Old));
                        Phi_Max = Math.Max(Phi_Max, Math.Abs(Phi[i, j]));

                    }
                if (ErrPhi / (Phi_Max + MEM.Error9) < MEM.Error6)
                    break;
            }
        }
        /// <summary>
        /// ОО: RectFDMesh - базистная разностная сетка 
        /// в четырех гранной области c косым уступом
        /// 
        ///  ----0-----------------------------> y (j)
        ///      |                         |  
        ///     Lx                         |
        ///      |                         |
        ///      |                         |
        ///      |---------- Ly -----------|
        ///      |
        ///      V x (i)     
        /// 
        public void SolverVortex()
        {
            // границы 
            for (int i = 0; i < Nx; i++)
            {
                // WL
                Vortex[i, Ny-1] = - 2 * Phi[i, Ny - 2] / (dy * dy) - 2 * V / dy;
                // bed
                Vortex[i, 0]    = - 2 * Phi[i, 1] / (dy * dy);
            }
            for (int j = 0; j < Ny; j++)
            {
                Vortex[0, j]    = - 2 * Phi[1, j] / (dx * dx);
                Vortex[Nx-1, j] = - 2 * Phi[Nx - 2, j] / (dx * dx);
            }

            double ErrPhi = 0, Phi_Old, Phi_Max = 0;
            for (int n = 0; n < M; n++)
            {
                ErrPhi = 0;
                Phi_Max = 0;
                for (int i = 1; i < Nx - 1; i++)
                    for (int j = 1; j < Ny - 1; j++)
                    {
                        Phi_Old = Vortex[i, j];
                        Vortex[i, j] = 0.25 * (Vortex[i + 1, j] + Vortex[i - 1, j] + Vortex[i, j + 1] + Vortex[i, j - 1]);
                        ErrPhi = Math.Max(ErrPhi, Math.Abs(Vortex[i, j] - Phi_Old));
                        Phi_Max = Math.Max(Phi_Max, Math.Abs(Vortex[i, j]));

                    }
                if (ErrPhi / (Phi_Max + MEM.Error9) < MEM.Error6)
                    break;
            }
        }

        public void Velocity()
        {
            for (int i = 0; i < Nx; i++)
                for (int j = 1; j < Ny - 1; j++)
                {
                    mV[i, j] = (Phi[i, j + 1] - Phi[i, j - 1]) / dy /2;
                }
            for (int i = 0; i < Nx; i++)
            {
                mV[i, Ny - 1] = (Phi[i, Ny - 1] - Phi[i, Ny - 2]) / dy;
            }
            for (int i = 1; i < Nx - 1; i++)
                for (int j = 0; j < Ny; j++)
                {
                    mW[i, j] = - (Phi[i + 1, j] - Phi[i - 1, j]) / dx /2;
                }
            for (int j = 1; j < Ny - 1; j++)
            {
                mW[0, j] = -(Phi[1, j] - Phi[0, j]) / dx;
                mW[Nx - 1, j] = -(Phi[Nx - 1, j] - Phi[Nx - 2, j]) / dx;
            }
        }
    }
}
