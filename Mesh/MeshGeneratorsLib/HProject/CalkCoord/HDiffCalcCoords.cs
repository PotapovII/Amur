//---------------------------------------------------------------------------
//                          ПРОЕКТ  "H?"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                кодировка : 06.06.2006 Потапов И.И. (c++)
//---------------------------------------------------------------------------
//           кодировка : 02.04.2021 Потапов И.И. (c++=> c#)
//          при переносе не были сохранены механизмы поддержки
//          изменения параметров задачи во времени отвечающие за
//        переменные граничные условия для/на граничных сегментах 
//---------------------------------------------------------------------------
//              изменен способ подключения к IAlgebra 
//                  библиотека пока не та :(
//---------------------------------------------------------------------------
namespace MeshGeneratorsLib
{
    using System;
    using System.Collections.Generic;
    using AlgebraLib;
    using GeometryLib;
    using MemLogLib;
    /// <summary>
    ///  ОО: Вычисляем координаты по итерационному псевдоортогональному методу
    /// </summary>
    class HDiffCalcCoords : ACalcCoords
    {
        protected double RelaxMeshOrthogonality;
        public HDiffCalcCoords(int MNodeX, int MNodeY,
        List<int> _L, List<int> _R, VMapKnot[][] _pMap, double RelaxMeshOrthogonality) :
            base(MNodeX, MNodeY, _L, _R, _pMap)
        {
            if (RelaxMeshOrthogonality <= 1)
                this.RelaxMeshOrthogonality = RelaxMeshOrthogonality;
            else
                this.RelaxMeshOrthogonality = 0.05;
        }
        /// <summary>
        /// создание матрицы системы
        /// </summary>
        /// <param name="TypeAlgebra"></param>
        public override void Solve()
        {
            try
            {
                base.Solve();
                // Вычисляем координаты по итерационному псевдоортогональному методу
                CalcCoords();
            }
            catch
            {
                Console.WriteLine("Проблемы с генерацией КЭ сетки : HCalcCoords::Solve");
            }
        }
        /// <summary>
        /// Поиск параметра или координаты
        /// </summary>
        public override void CalcCoords()
        {
            IndexRow = 0;
            // параметр релаксации
            double Tay = 0.5;//2*(1-3.14159/2*MKnots)-0.2;
                             // цикл по нелинейности
            double MaxX = 1, MaxY = 1;
            //
            int ip, ie, iw, iss, inn, ien, iwn, ies, iws;

            double xp, xe, xw, xs, xn;
            double yp, ye, yw, ys, yn;
            double xen, xwn, xes, xws;
            double yen, ywn, yes, yws;
            double Ap, Ig, Alpha, Betta, Gamma, Delta;


            double xo, yo;
            double dxo, dyo, ddMax = 0, dd = 0;
            double L = pMap[MaxNodeY - 1][0].x - pMap[0][0].x + 1;
            //
            for (int nLine = 0; nLine < 100000000; nLine++)
            {
                // цикл по узлам
                for (int i = 1; i < MaxNodeY - 1; i++)
                {
                    for (int j = Left[i] + 1; j < Right[i] - 1; j++)
                    {
                        try
                        {
                            CountCol = 0;
                            R = 0;
                            // Для линейного варианта узлы помечены (..)
                            //
                            // iwn     (inn)     ien
                            //
                            //
                            // (iw)    (ip)     (ie)
                            //
                            //
                            // iws     (iss)     ies
                            //
                            // формирование индексов строки
                            ip = KnotMap[i][j];
                            ie = KnotMap[i][j + 1];
                            iw = KnotMap[i][j - 1];
                            iss = KnotMap[i + 1][j];
                            inn = KnotMap[i - 1][j];
                            //

                            xp = pMap[i][j].x / MaxX;
                            xe = pMap[i][j + 1].x / MaxX;
                            xw = pMap[i][j - 1].x / MaxX;
                            xs = pMap[i + 1][j].x / MaxX;
                            xn = pMap[i - 1][j].x / MaxX;

                            yp = pMap[i][j].y / MaxY;
                            ye = pMap[i][j + 1].y / MaxY;
                            yw = pMap[i][j - 1].y / MaxY;
                            ys = pMap[i + 1][j].y / MaxY;
                            yn = pMap[i - 1][j].y / MaxY;

                            ien = KnotMap[i - 1][j + 1];
                            if (ien < 0)
                            {
                                xen = pMap[i - 1][j + 1].x / MaxX;
                                yen = pMap[i - 1][j + 1].y / MaxY;
                            }
                            else
                            { // xws, yws
                                xen = pMap[i + 1][j - 1].x / MaxX;
                                yen = pMap[i + 1][j - 1].y / MaxY;
                            }

                            iwn = KnotMap[i - 1][j - 1];
                            if (iwn < 0)
                            {
                                xwn = pMap[i - 1][j - 1].x / MaxX;
                                ywn = pMap[i - 1][j - 1].y / MaxY;
                            }
                            else
                            { // xes, yes
                                xwn = pMap[i + 1][j + 1].x / MaxX;
                                ywn = pMap[i + 1][j + 1].y / MaxY;
                            }

                            ies = KnotMap[i + 1][j + 1];
                            if (ies < 0)
                            {
                                xes = pMap[i + 1][j + 1].x / MaxX;
                                yes = pMap[i + 1][j + 1].y / MaxY;
                            }
                            else
                            { // = xwn, ywn
                                xes = pMap[i - 1][j - 1].x / MaxX;
                                yes = pMap[i - 1][j - 1].y / MaxY;
                            }

                            // Подгибаемый угол
                            // проверяем на существование
                            iws = KnotMap[i + 1][j - 1];
                            if (iws < 0)
                            {
                                // центральная точка для правой разности
                                xws = pMap[i][j].x / MaxX;
                                yws = pMap[i][j].y / MaxY;
                            }
                            else
                            {
                                xws = pMap[i - 1][j + 1].x / MaxX;
                                yws = pMap[i - 1][j + 1].y / MaxY;
                            }

                            Alpha = 1;
                            Betta = 0;
                            Gamma = 1;
                            Delta = 0;
                            if (nLine > 10)
                            {
                                try
                                {
                                    // g22
                                    Alpha = 0.25 * ((xn - xs) * (xn - xs) + (yn - ys) * (yn - ys));
                                    // g12
                                    Betta = RelaxMeshOrthogonality * 0.25 * ((xe - xw) * (xn - xs) + (ye - yw) * (yn - ys));
                                    // g11
                                    Gamma = 0.25 * ((xe - xw) * (xe - xw) + (ye - yw) * (ye - yw));
                                    // Определитель метрического тензора
                                    Delta = 0.0625 * ((xe - xw) * (yn - ys) - (xn - xs) * (ye - yw));
                                    //
                                }
                                catch
                                {
                                    Alpha = 1;
                                    Betta = 0;
                                    Gamma = 1;
                                    Delta = 0;
                                }
                            }
                            Ig = Alpha + Gamma;
                            Ap = 1.0 / (2.0 * Ig);
                            // регуляризаторы сетки
                            double Q = 0;
                            double P = 0;
                            //
                            if (iws < 0)
                            {
                                xp = Ap * (Alpha * (xw + xe) + Gamma * (xn + xs) -
                                          0.5 * Betta * (xen - xes) +
                                          0.5 * Delta * (xe - xw) * P +
                                          0.5 * Delta * (xn - xs) * Q);

                                yp = Ap * (Alpha * (yw + ye) + Gamma * (yn + ys) -
                                          0.5 * Betta * (yen - yes) +
                                          0.5 * Delta * (ye - yw) * P +
                                          0.5 * Delta * (yn - ys) * Q);
                            }
                            else
                            {
                                xp = Ap * (Alpha * (xw + xe) + Gamma * (xn + xs) -
                                          0.5 * Betta * (xen - xwn - xes + xws) +
                                          0.5 * Delta * (xe - xw) * P +
                                          0.5 * Delta * (xn - xs) * Q);

                                yp = Ap * (Alpha * (yw + ye) + Gamma * (yn + ys) -
                                          0.5 * Betta * (yen - ywn - yes + yws) +
                                          0.5 * Delta * (ye - yw) * P +
                                          0.5 * Delta * (yn - ys) * Q);
                            }
                            // Глобальная нумерация узлов
                            xo = pMap[i][j].x;
                            yo = pMap[i][j].y;
                            pMap[i][j].x = (1 - Tay) * pMap[i][j].x + Tay * xp * MaxX;
                            pMap[i][j].y = (1 - Tay) * pMap[i][j].y + Tay * yp * MaxY;
                            dxo = pMap[i][j].x - xo;
                            dyo = pMap[i][j].y - yo;
                            dd = (dxo * dxo + dyo * dyo) / L;
                            if (ddMax < dd)
                                ddMax = dd;
                            if (double.IsNaN(dd) == true)
                                throw new Exception("Critic error CalcCoords()");
                        }
                        catch (Exception ex)
                        {
                            Logger.Instance.Exception(ex);
                            string m = " i = " + i;
                            m += " j =" + j;
                            m += " nLine = " + nLine;
                            Console.WriteLine(m);

                        }
                    }
                }
                if (Math.Sqrt(dd) < 0.00000001)
                    break;
            }
        }
    }

    /// <summary>
    ///  ОО: Вычисляем координаты по итерационному псевдоортогональному методу
    /// </summary>
    class HAlgCalcCoords : ACalcCoords
    {
        public HAlgCalcCoords(int MNodeX, int MNodeY,
        List<int> _L, List<int> _R, VMapKnot[][] _pMap, double RelaxMeshOrthogonality) :
            base(MNodeX, MNodeY, _L, _R, _pMap)
        {
        }
        /// <summary>
        /// создание матрицы системы
        /// </summary>
        /// <param name="TypeAlgebra"></param>
        public override void Solve()
        {
            try
            {
                base.Solve();
                // Вычисляем координаты по итерационному псевдоортогональному методу
                CalcCoords();
            }
            catch
            {
                Console.WriteLine("Проблемы с генерацией КЭ сетки : HCalcCoords::Solve");
            }
        }
        /// <summary>
        /// Поиск параметра или координаты
        /// </summary>
        public override void CalcCoords()
        {
            // цикл по узлам
            for (int i = 1; i < MaxNodeY - 1; i++)
            {
                double x0 = pMap[i][Left[i]].x;
                double xL = pMap[i][Right[i]].x;
                double L = xL - x0;
                double dx = L / (Right[i] - Left[i]);
                double y0 = pMap[i][Left[i]].y;
                double yL = pMap[i][Right[i]].y;
                double H = yL - y0;
                double dy = H / (Right[i] - Left[i]);
                for (int j = Left[i] + 1; j < Right[i] - 1; j++)
                {
                    pMap[i][j].x = x0 + dx * (j - Left[i]);
                    pMap[i][j].y = y0 + dy * (j - Left[i]);
                }
            }
        }
    }

}
