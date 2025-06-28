//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2021 -
//                       ALL RIGHT RESERVED
//                        ПРОЕКТ "BedLoadLib"
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                          28.04.2021
//---------------------------------------------------------------------------

namespace BedLoadLib
{
    using AlgebraLib;
    using CommonLib;
    using MemLogLib;
    using System;
    using System.Linq;

    /// <summary>
    /// ОО: Реализация класса SAvalanche вычисляющего осыпание склона из 
    /// несвязного материала. Осыпание склона происходит при превышении 
    /// уклона склона угла внутреннего трения для материала 
    /// формирующего склон.
    /// </summary>
    [Serializable]
    class Avalanche2D
    {
        IMesh mesh;
        SparseAlgebra algebra;
        double[] ActiveKnot;
        double[] SKnot;
        double tanPhi;
        uint CountStep;
        uint[] knots = { 0, 0, 0 };
        double[] x = { 0, 0, 0 };
        double[] y = { 0, 0, 0 };


        double Relax = 1;
        /// <summary>
        /// Плошадь КЭ
        /// </summary>
        double S = 0;
        /// <summary>
        /// ненормированные производные от функций формы по х
        /// </summary>
        double[] a = { 0, 0, 0 };
        /// <summary>
        /// ненормированные производные от функций формы по у
        /// </summary>
        double[] b = { 0, 0, 0 };
        /// <summary>
        /// Отметки дна в узлах КЭ
        /// </summary>
        double[] zeta = { 0, 0, 0 };
        /// <summary>
        /// локальная матрица часть СЛАУ
        /// </summary>
        protected double[][] LaplMatrix = null;

        double dZetadX, dZetadY;
        public Avalanche2D(IMesh mesh, double tanPhi, double Relax = 0.3, uint CountStep = 3)
        {
            this.mesh = mesh;
            this.tanPhi = tanPhi;
            this.Relax = Relax;
            this.CountStep = CountStep;
        }
        /// <summary>
        /// Метод лавинного обрушения правого склонов 
        /// </summary>
        /// <param name="Z">массив донных отметок</param>
        /// <param name="ds">координаты узлов</param>
        /// <param name="tf">тангенс внутреннено трения **</param>
        /// <param name="Step">шаг остановки процесса, 
        /// при 0 лавина проходит полностью</param>
        public void Lavina(ref double[] Zeta)
        {
            if (mesh == null || Zeta == null)
                return;
            if (algebra == null)
                algebra = new SparseAlgebraCG((uint)Zeta.Length);
            MEM.Alloc2DClear(3, ref LaplMatrix);
            for (int index = 0; index < CountStep; index++)
            //for (int index = 0; index < 5; index++)
            {
                MEM.AllocClear(Zeta.Length, ref ActiveKnot);
                MEM.AllocClear(Zeta.Length, ref SKnot);
                algebra.Clear();
                for (uint elem = 0; elem < mesh.CountElements; elem++)
                {
                    // получить узлы КЭ
                    mesh.ElementKnots(elem, ref knots);
                    // координаты и площадь
                    mesh.GetElemCoords(elem, ref x, ref y);
                    // площадь
                    S = mesh.ElemSquare(elem);
                    a[0] = y[1] - y[2];
                    b[0] = x[2] - x[1];
                    a[1] = y[2] - y[0];
                    b[1] = x[0] - x[2];
                    a[2] = y[0] - y[1];
                    b[2] = x[1] - x[0];
                    // определение градиента дна по х,у
                    mesh.ElemValues(Zeta, elem, ref zeta);
                    dZetadX = 0;
                    dZetadY = 0;
                    for (int ai = 0; ai < zeta.Length; ai++)
                    {
                        dZetadX += a[ai] * zeta[ai];
                        dZetadY += b[ai] * zeta[ai];
                    }
                    dZetadX /= (2 * S);
                    dZetadY /= (2 * S);

                    double Ax = Math.Max(0, dZetadX - tanPhi);
                    double Ay = Math.Max(0, dZetadY - tanPhi);
                    double Ac = Math.Max(Ax, Ay);
                    SKnot[knots[0]] += S / 3;
                    SKnot[knots[1]] += S / 3;
                    SKnot[knots[2]] += S / 3;
                    if (Ac > 0)
                    {
                        ActiveKnot[knots[0]] += Ac * S / 3;
                        ActiveKnot[knots[1]] += Ac * S / 3;
                        ActiveKnot[knots[2]] += Ac * S / 3;
                    }

                    // вычисление ЛЖМ
                    for (int ai = 0; ai < knots.Length; ai++)
                        for (int aj = 0; aj < knots.Length; aj++)
                        {
                            // Матрица жесткости
                            LaplMatrix[ai][aj] = (a[ai] * a[aj] +
                                                  a[ai] * b[aj] +
                                                  b[ai] * a[aj] +
                                                  b[ai] * b[aj]) / (4 * S);
                        }
                    // добавление вновь сформированной ЛЖМ в ГМЖ
                    algebra.AddToMatrix(LaplMatrix, knots);
                }
                // активные узлы
                for (int i = 0; i < SKnot.Length; i++)
                    ActiveKnot[i] /= SKnot[i];
                // algebra.Print();
                double max = ActiveKnot.Max();
                if (max < MEM.Error3)
                    break;
                //Console.WriteLine();
                //Console.WriteLine(" {0} ", index);
                //for (int i = 0; i < SKnot.Length; i++)
                //{
                //    if(ActiveKnot[i]> MEM.Error3)
                //        Console.Write(" {0} {1:F4} ",i, ActiveKnot[i]);
                //}
                for (int i = 0; i < SKnot.Length; i++)
                {
                    double z1 = Zeta[i];
                    double idxAll = 0;
                    double dij = 0;
                    double elem = 1;
                    int ki;
                    if (Math.Abs(ActiveKnot[i]) > MEM.Error3)
                    {
                        //Console.WriteLine();
                        //Console.WriteLine(" {0}", i);
                        SparseRow row = algebra.Matrix[i];
                        for (int j = 0; j < row.Row.Count; j++)
                        {
                            ki = row[j].IndexColumn;
                            elem = row[j].Value;
                            //Console.Write(" {0:F4} ", elem);
                            if (ki == i)
                                dij = elem;
                            else
                                idxAll -= elem * Zeta[ki];
                        }
                        //Console.WriteLine();
                        //Console.WriteLine(" {0} ", i);
                        //for (uint j = 0; j < row.Row.Count; j++)
                        //{
                        //    ki = row[j].Knot;
                        //    Console.Write(" {0:F4} ", Zeta[ki]);
                        //}
                        //Console.WriteLine();
                        //Console.WriteLine(" {0} ", i);
                        //for (uint j = 0; j < row.Row.Count; j++)
                        //{
                        //    ki = row[j].Knot;
                        //    Console.Write(" {0:F4} ", ki);
                        //}

                        if (Math.Abs(dij) > MEM.Error5)
                        {
                            double z2 = idxAll / dij;
                            Zeta[i] = (1 - Relax) * Zeta[i] + Relax * z2;
                        }
                        //Console.WriteLine();
                        //Console.WriteLine("{0}  oldZeta = {1}   Zeta = {2}   dZ = {3}   dij = {4}   idxAll = {5} ",
                        // i, z1, Zeta[i], z1 - Zeta[i], dij, idxAll);
                    }
                    else
                        Zeta[i] = Zeta[i];
                }
            }
        }
    }
}
