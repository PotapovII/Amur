//---------------------------------------------------------------------------
//                     ПРОЕКТ  "RiverSolver"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//               кодировка : 04.06.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace NPRiverLib.APRiver2XYD.River2DSW
{
    using System;
    using GeometryLib;
    using MemLogLib;
    using MeshLib;
    /// <summary>
    /// Расчет точек интегрирования на полусмоченной границе
    /// </summary>
    [Serializable]
    public class RiverBankGaussPoints
    {
        /// <summary>
        /// Минимальная глубина русла
        /// </summary>
        double H_minGroundWater = 0.01;
        /// <summary>
        /// Учитывает затопленую часть ледового покрова
        /// </summary>
        double IceCoeff = 0.9;

        public double[][] fEtaXi;
        /// <summary>
        /// Количество вершин КЭ
        /// </summary>
        int CountElementKnots = 3;

        public RiverBankGaussPoints(double H_minGroundWater, double IceCoeff)
        {
            this.H_minGroundWater = H_minGroundWater;
            this.IceCoeff = IceCoeff;
            MEM.Alloc2D(CountElementKnots, CountElementKnots, ref fEtaXi);
            for (int i = 0; i < CountElementKnots; i++)
            {
                for (int j = 0; j < CountElementKnots; j++)
                {
                    if (i == j)
                        fEtaXi[i][j] = 2.0 / 3.0;
                    else
                        fEtaXi[i][j] = 1.0 / 6.0;
                }
            }
        }
        /// <summary>
        /// Определение точек интегрирования для полузатопленного КЭ, метод вызывается несколько раз
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="pIntegration">точки интегрирования</param>
        /// <param name="ngauss">количество уже найденных точек интегрирования </param>
        /// <returns></returns>
        public int FindGaussPoints(double[] x, double[] y, ref NumInegrationPoints pIntegration, int ngauss)
        {
            double weight;
            weight = GEO.TriangleArea(x[0], y[0], x[1], y[1], x[2], y[2]) / 3.0;
            pIntegration.ReSize(ngauss + 3);
            for (int i = ngauss; i < (ngauss + 3); i++)
            {
                pIntegration.xi[i]  = x[0] * fEtaXi[0][i - ngauss] + x[1] * fEtaXi[1][i - ngauss] + x[2] * fEtaXi[2][i - ngauss];
                pIntegration.eta[i] = y[0] * fEtaXi[0][i - ngauss] + y[1] * fEtaXi[1][i - ngauss] + y[2] * fEtaXi[2][i - ngauss];
                pIntegration.weight[i] = weight;
            }
            return (ngauss + 3);
        }
        /// <summary>
        /// Смешанные точки интегрирования для полу затопленного симплекса 
        /// </summary>
        /// </summary>
        /// <param name="currentFElement"></param>
        /// <param name="pIntegration"></param>
        /// <returns>количество точек интегрирования</returns>
        public int GetMixedGPS(RiverNode[] nodes, TriElementRiver currentFElement, ref NumInegrationPoints pIntegration, int CountElementKnots)
        {
            // количество точек интегрирования
            int ngauss = 0;

            int i, nipts, nLeft = 0, nRight = 0, n;

            RiverNode[] left = new RiverNode[CountElementKnots];
            RiverNode[] right = new RiverNode[CountElementKnots];
            RiverNode[] elemNodes = new RiverNode[CountElementKnots];

            for (i = 0; i < elemNodes.Length; i++)
                elemNodes[i] = new RiverNode();
            for (i = 0; i < left.Length; i++)
            {
                left[i] = new RiverNode();
                right[i] = new RiverNode();
            }
            double[] xl = new double[CountElementKnots];
            double[] yl = new double[CountElementKnots];
            double[] x = new double[CountElementKnots];
            double[] y = new double[CountElementKnots];

            elemNodes[0].X = 1.0;
            elemNodes[0].Y = 0.0;
            elemNodes[1].X = 0.0;
            elemNodes[1].Y = 1.0;
            elemNodes[2].X = 0.0;
            elemNodes[2].Y = 0.0;
            /// Поиск пересечений береговой линии с КЭ
            nipts = FindIntersection(nodes, currentFElement, elemNodes, ref xl, ref yl, H_minGroundWater, ref nRight, ref nLeft, ref right, ref left);
            if (nipts != 2)
            {
                // Console.WriteLine("в элементе {0} не найдено точек пересечения", currentFElement.ID);
                return (0);
            }
            switch (nRight)
            {
                case 1:
                    x[0] = xl[0];
                    y[0] = yl[0];
                    x[1] = right[0].X;
                    y[1] = right[0].Y;
                    x[2] = xl[1];
                    y[2] = yl[1];
                    ngauss = FindGaussPoints(x, y, ref pIntegration, ngauss);

                    break;

                case 2:
                    x[0] = xl[0];
                    y[0] = yl[0];
                    x[2] = xl[1];
                    y[2] = yl[1];

                    n = FindBestNode(xl[0], yl[0], right[0].X, right[0].Y, right[1].X, right[1].Y, xl[1], yl[1]);
                    x[1] = right[n].X;
                    y[1] = right[n].Y;

                    ngauss = FindGaussPoints(x, y, ref pIntegration, ngauss);

                    // теперь найди второй треугольник
                    x[1] = right[0].X;
                    x[2] = right[1].X;
                    y[1] = right[0].Y;
                    y[2] = right[1].Y;

                    if (n == 0)
                    {
                        x[0] = xl[1];
                        y[0] = yl[1];
                    }
                    else
                    {
                        x[0] = xl[0];
                        y[0] = yl[0];
                    }
                    ngauss = FindGaussPoints(x, y, ref pIntegration, ngauss);

                    break;

                case 3:
                    // качество треугольников не проверяется для первого треугольника:
                    // 1 - й треугольник:
                    x[0] = xl[1];
                    y[0] = yl[1];
                    x[1] = xl[0];
                    y[1] = yl[0];
                    x[2] = right[0].X;
                    y[2] = right[0].Y;

                    ngauss = FindGaussPoints(x, y, ref pIntegration, ngauss);

                    // 2 - й треугольник:
                    x[0] = right[0].X;
                    x[1] = right[1].X;
                    x[2] = xl[1];
                    y[0] = right[0].Y;
                    y[1] = right[1].Y;
                    y[2] = yl[1];

                    ngauss = FindGaussPoints(x, y, ref pIntegration, ngauss);

                    // 3-й треугольник:
                    x[0] = right[1].X;
                    x[1] = right[2].X;
                    x[2] = xl[1];
                    y[0] = right[1].Y;
                    y[1] = right[2].Y;
                    y[2] = yl[1];

                    ngauss = FindGaussPoints(x, y, ref pIntegration, ngauss);

                    break;
            }
            switch (nLeft)
            {
                case 1:
                    x[0] = xl[0];
                    y[0] = yl[0];
                    x[1] = xl[1];
                    y[1] = yl[1];
                    x[2] = left[0].X;
                    y[2] = left[0].Y;
                    ngauss = FindGaussPoints(x, y, ref pIntegration, ngauss);

                    break;

                case 2:
                    // найдите 1-й треугольник

                    x[0] = xl[0];
                    y[0] = yl[0];
                    x[1] = xl[1];
                    y[1] = yl[1];

                    n = FindBestNode(xl[0], yl[0], xl[1], yl[1], left[0].X, left[0].Y, left[1].X, left[1].Y);

                    x[2] = left[n].X;
                    y[2] = left[n].Y;
                    FindGaussPoints(x, y, ref pIntegration, ngauss);
                    ngauss += 3;

                    // найдите 2-й треугольник

                    x[1] = left[0].X;
                    x[2] = left[1].X;
                    y[1] = left[0].Y;
                    y[2] = left[1].Y;

                    x[0] = xl[n];
                    y[0] = yl[n];


                    FindGaussPoints(x, y, ref pIntegration, ngauss);
                    ngauss += 3;
                    break;

                case 3:
                    // качество треугольников не проверяется для первого треугольника:
                    x[0] = xl[0];
                    y[0] = yl[0];
                    x[1] = xl[1];
                    y[1] = yl[1];
                    x[2] = left[0].X;
                    y[2] = left[0].Y;

                    ngauss = FindGaussPoints(x, y, ref pIntegration, ngauss);

                    // найдите 2-й треугольник
                    x[0] = left[0].X;
                    x[1] = left[1].X;
                    x[2] = xl[0];
                    y[0] = left[0].Y;
                    y[1] = left[1].Y;
                    y[2] = yl[0];

                    ngauss = FindGaussPoints(x, y, ref pIntegration, ngauss);

                    // найдите 3-й треугольник
                    x[0] = left[1].X;
                    x[1] = left[2].X;
                    x[2] = xl[0];
                    y[0] = left[1].Y;
                    y[1] = left[2].Y;
                    y[2] = yl[0];
                    ngauss = FindGaussPoints(x, y, ref pIntegration, ngauss);
                    break;
            }
            return ngauss;
        }
        /// <summary>
        /// Поиск пересечений береговой линии с КЭ
        /// </summary>
        int FindIntersection(RiverNode[] mrNodes, TriElementRiver currentFElement, RiverNode[] elemNodes, ref double[] xl, ref double[] yl,
            double H_minGroundWater, ref int nRight, ref int nLeft, ref RiverNode[] right, ref RiverNode[] left)
        {
            // Узлов на КЭ
            int i, ncut, nlast, side, j, nr, nl;
            double mult, factor;
            ncut = 0;
            nr = 0;
            side = 0;
            left[0] = elemNodes[0];
            nl = 1;
            RiverNode[] nodes = { mrNodes[currentFElement.Vertex1],
                                  mrNodes[currentFElement.Vertex2],
                                  mrNodes[currentFElement.Vertex3] };

            for (i = 0; i < CountElementKnots; i++)
            {

                if (i == (CountElementKnots - 1))
                    nlast = 0;
                else
                    nlast = i + 1;


                mult = ((nodes[i].h0 - (nodes[i].h_ise * IceCoeff)) - H_minGroundWater) *
                       ((nodes[nlast].h0 - (nodes[nlast].h_ise * IceCoeff)) - H_minGroundWater);

                if (mult > 0.0)
                {
                    // нет пересечения
                    if (side == 0)
                    {
                        // мы на левой стороне
                        if (nr == 0)
                        {
                            // это наш первый раз на левой стороне
                            left[nl] = elemNodes[nlast];
                            nl++;
                        }
                        else
                        {
                            // возвращаемся на левую сторону. вставлять
                            if (nlast != 0)
                            {
                                for (j = nl; j > 0; j--)
                                    left[j] = left[j - 1];
                                left[0] = elemNodes[nlast];
                                nl++;
                            }
                        }
                    }
                    else
                    {
                        // мы на правой стороне
                        right[nr] = elemNodes[nlast];
                        nr++;
                    }
                }
                if (mult == 0.0)
                {
                    if ((nodes[i].h0 - (nodes[i].h_ise * IceCoeff)) == H_minGroundWater)
                    {
                        xl[ncut] = elemNodes[i].X;
                        yl[ncut] = elemNodes[i].Y;
                        ncut++;

                        side = Math.Abs(1 - side);
                        if (i == 0)
                            nl--;
                        if (nlast != 0)
                        {
                            if (side == 0)
                            {
                                if (nr == 0)
                                {
                                    left[nl] = elemNodes[nlast];
                                    nl++;
                                }
                                else
                                {
                                    if (nlast != 0)
                                    {
                                        for (j = nl; j > 0; j--)
                                            left[j] = left[j - 1];
                                        left[0] = elemNodes[nlast];
                                        nl++;
                                    }
                                }
                            }
                            else
                            {
                                // мы на правой стороне
                                right[nr] = elemNodes[nlast];
                                nr++;
                            }
                        }
                    }
                }
                if (mult < 0.0)
                {
                    double factor1 = Math.Abs((nodes[i].h0 - (nodes[i].h_ise * IceCoeff)) - H_minGroundWater);
                    double factor2 = Math.Abs((nodes[nlast].h0 - (nodes[nlast].h_ise * IceCoeff) - H_minGroundWater));
                    factor = factor1 / (factor2 + factor1);

                    xl[ncut] = elemNodes[i].X + (elemNodes[nlast].X - elemNodes[i].X) * factor;
                    yl[ncut] = elemNodes[i].Y + (elemNodes[nlast].Y - elemNodes[i].Y) * factor;

                    ncut++;
                    side = Math.Abs(side - 1);
                    if (side == 0)
                    {
                        // мы должны вернуться налево. вставлять
                        if (nlast != 0)
                        {
                            for (j = nl; j > 0; j--)
                                left[j] = left[j - 1];
                            left[0] = elemNodes[nlast];
                            nl++;
                        }
                    }
                    else
                    {
                        // мы перешли слева направо
                        right[nr] = elemNodes[nlast];
                        nr++;
                    }
                }
            }
            nRight = nr;
            nLeft = nl;
            return ncut;
        }
        /// <summary>
        /// Поиск лучшего узла
        /// </summary>
        /// <param name="xa"></param>
        /// <param name="xb"></param>
        /// <param name="ya"></param>
        /// <param name="yb"></param>
        /// <param name="x0"></param>
        /// <param name="y0"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <returns></returns>
        int FindBestNode(double xa, double xb, double ya, double yb, double x0, double y0, double x1, double y1)
        {
            double LengthAB = GEO.Length(xa, ya, xb, yb);
            double alpha0 = GEO.TriangleArea(xa, xb, ya, yb, x0, y0) / (LengthAB + GEO.Length(xb, yb, x0, y0) + GEO.Length(x0, y0, xa, ya));
            double alpha1 = GEO.TriangleArea(xa, xb, ya, yb, x1, y1) / (LengthAB + GEO.Length(xb, yb, x1, y1) + GEO.Length(x1, y1, xa, ya));
            if (alpha0 > alpha1)
                return 0;
            else
                return 1;
        }

        /// <summary>
        /// Получить точки интегрирования для полузатопленного граничного элемента 
        /// </summary>
        /// <param name="currentBoundFElement"></param>
        /// <param name="g"></param>
        /// <returns></returns>
        public int GetBoundaryGPS(RiverNode[] elemNodes, ref NumInegrationPoints bpIntegration)
        {
            int ngauss = 0;
            double[] r = new double[2];
            double gwH = H_minGroundWater;
            double a = (elemNodes[0].h - (elemNodes[0].Hice * IceCoeff)) - gwH;
            double c = (elemNodes[1].h - (elemNodes[1].Hice * IceCoeff)) - gwH;
            double ri = 2.0 * Math.Abs(a) / (Math.Abs(a) + Math.Abs(c));
            bpIntegration.ReSize(4);
            r[0] = -1.0;
            r[1] = ri - 1.0;
            ngauss = FindBoundaryGPS(r, bpIntegration, ngauss);
            r[0] = ri - 1.0;
            r[1] = 1.0;
            ngauss = FindBoundaryGPS(r, bpIntegration, ngauss);
            return (ngauss);
        }
        int FindBoundaryGPS(double[] r, NumInegrationPoints bpIntegration, int ngauss)
        {
            double[] f = new double[2];
            f[0] = 0.5 * (1.0 + 1.0 / Math.Sqrt(3.0));
            f[1] = 0.5 * (1.0 - 1.0 / Math.Sqrt(3.0));
            bpIntegration.xi[ngauss] = r[0] * f[0] + r[1] * f[1];
            bpIntegration.weight[ngauss] = (r[1] - r[0]) / 2.0;
            ngauss++;
            bpIntegration.xi[ngauss] = r[0] * f[1] + r[1] * f[0];
            bpIntegration.weight[ngauss] = (r[1] - r[0]) / 2.0;
            ngauss++;
            return (ngauss);
        }
        /// <summary>
        /// Проверка, если (глубина потока) - (толщина льда) в узле 
        /// больше минимальной глубины то 0 иначе 1
        /// если 1 то элемент смешанный вода/берег
        /// </summary>
        /// <param name="elemNodes"></param>
        /// <returns></returns>
        public int CheckRiverBankDryWet(RiverNode[] elemNodes)
        {
            double mult;
            // количетсво проверок для 3 узлового и 2 узлового КЭ
            int Count = elemNodes.Length == CountElementKnots ? CountElementKnots : 1;
            for (int i = 0; i < Count; i++)
            {
                int j = (i + 1) % elemNodes.Length;
                mult = ((elemNodes[i].h0 - (elemNodes[i].Hice * IceCoeff)) - H_minGroundWater) *
                       ((elemNodes[j].h0 - (elemNodes[j].Hice * IceCoeff)) - H_minGroundWater);
                if (mult < 0.0)
                    return 1;
            }
            return 0;
        }
    }

}
