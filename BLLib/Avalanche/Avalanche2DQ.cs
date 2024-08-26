//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2021 -
//                       ALL RIGHT RESERVED
//                        ПРОЕКТ "BLLib"
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                          19.12.2021
//---------------------------------------------------------------------------
namespace BLLib
{
    using System;
    using MemLogLib;
    using CommonLib;
    using MeshLib;
    /// <summary>
    /// ОО: Реализация класса Avalanche вычисляющего осыпание склона из 
    /// несвязного материала. Осыпание склона происходит при превышении 
    /// уклона склона угла внутреннего трения для материала 
    /// формирующего склон.
    /// Реализация на ортогональной сетке в прямоугольном массиве
    /// </summary>
    [Serializable]
    public class Avalanche2DQ : AvalancheX
    {
        RectFVMesh Mesh = null;
        double[][] Zeta = null;
        
        public DirectAvalanche directAvalanche = DirectAvalanche.AvalancheXY;
        /// <summary>
        /// длины сторон треугольных КЭ
        /// </summary>
        public Avalanche2DQ(IMesh mesh, double tf, DirectAvalanche directAvalanche = DirectAvalanche.AvalancheX, double Relax = 0.5) : 
            base(mesh, tf, Relax)
        {
            this.directAvalanche = directAvalanche;
            Mesh = mesh as RectFVMesh;
            if (Mesh == null)
            {
                Logger.Instance.Error("Ошибка в приведении типпа сетки", "Avalanche2DQ.Avalanche2DQ");
            }
        }
        /// <summary>
        /// Лавинное обрушение дна
        /// </summary>
        /// <param name="Z">дно</param>
        public override void Lavina(ref double[] Z)
        {
            if (Mesh != null)
            {
                MEM.ValueAlloc1D_to_2D(Z, ref Zeta, Mesh.Nx, Mesh.Ny);
                Lavina2D(ref Zeta);
                MEM.Value2D_to_1D(Zeta, ref Z);
            }
            else
            {
                Logger.Instance.Error("Ошибка в приведении типпа сетки", "Avalanche2DQ.Lavina");
            }
        }
        public void Lavina2D(ref double[][] Z)
        {
            if (Mesh != null)
            {
                Zeta = Z;
                int i = 0;
                for (; i < 100; i++)
                {
                    if (Lavina(ref Z) == false)
                        break;
                }
            }
            else
            {
                Logger.Instance.Error("Ошибка в приведении типпа сетки", "Avalanche2DQ.Lavina2D");
            }
        }

        /// <summary>
        /// Метод лавинного обрушения правого склона, 
        /// обрушение с правой стороны
        /// </summary>
        /// <param name="Z">массив донных отметок</param>
        /// <param name="ds">координаты узлов</param>
        /// <param name="tf">тангенс внутреннено трения **</param>
        /// <param name="Step">шаг остановки процесса, 
        /// при 0 лавина проходит полностью</param>
        public bool Lavina(ref double[][] Zeta)
        {
            if (directAvalanche == DirectAvalanche.AvalancheX ||
                directAvalanche == DirectAvalanche.AvalancheXY)
            {
                maxDz = 0;
                for (int i = 1; i < Zeta.Length; i++)
                {
                    for (int j = 0; j < Zeta[i].Length; j++)
                    {
                        double dh = (Mesh.x[i][j] - Mesh.x[i - 1][j]) * tf;
                        double dz = Zeta[i][j] - Zeta[i - 1][j];
                        if (dh < Math.Abs(dz))
                        {
                            double delta = (Math.Abs(dz) - dh) * Relax;
                            maxDz = Math.Max(maxDz, Math.Abs(delta));
                            if (Zeta[i - 1][j] < Zeta[i][j])
                            {
                                Zeta[i][j] -= delta;
                                Zeta[i - 1][j] += delta;
                            }
                            else
                            {
                                Zeta[i][j] += delta;
                                Zeta[i - 1][j] -= delta;
                            }
                        }
                    }
                }
                if (directAvalanche == DirectAvalanche.AvalancheY ||
                    directAvalanche == DirectAvalanche.AvalancheXY)
                {
                    maxDz = 0;
                    for (int i = 0; i < Zeta.Length; i++)
                    {
                        for (int j = 1; j < Zeta[i].Length; j++)
                        {
                            double dh = (Mesh.y[i][j] - Mesh.y[i][j - 1]) * tf;
                            double dz = Zeta[i][j] - Zeta[i][j - 1];
                            if (dh < Math.Abs(dz))
                            {
                                double delta = (Math.Abs(dz) - dh) * Relax;
                                maxDz = Math.Max(maxDz, Math.Abs(delta));
                                if (Zeta[i][j - 1] < Zeta[i][j])
                                {
                                    Zeta[i][j] -= delta;
                                    Zeta[i][j - 1] += delta;
                                }
                                else
                                {
                                    Zeta[i][j] += delta;
                                    Zeta[i][j - 1] -= delta;
                                }
                            }
                        }
                    }
                }
            }
            if (maxDz < MEM.Error4)
                return false;
            else
                return true;
        }
    }
}
