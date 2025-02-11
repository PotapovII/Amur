// --------------------------------------------------------------
//                       - (f) 1996
//                       С++  26/11/96
//              Класс численного интегрирования
// --------------------------------------------------------------
//              Last Edit Data: 25.10.98  Потапов И.И.
// --------------------------------------------------------------
//              Last Edit Data: 10.3.2002  Потапов И.И.
// --------------------------------------------------------------
//              Перенос на C# : 03.03.2021  Потапов И.И.
// --------------------------------------------------------------
namespace MeshLib
{
    using CommonLib;
    using MemLogLib;
    using System;
    using System.Collections.Generic;
    /// <summary>
    /// ОО: Точки интегрирования и веса
    /// </summary>
    [Serializable]
    public class NumInegrationPoints
    {
        /// <summary>
        /// Тип области интегрирования
        /// </summary>
        FFGeomType Type;
        /// <summary>
        /// количество точек интегрирования в одном направлении
        /// </summary>
        int Rang;
        /// <summary>
        /// размерность пространства
        /// </summary>
        int Dim;
        /// <summary>
        /// одномерные координаты и веса
        /// </summary>
        double[] Q = { 0, 0, 0, 0, 0, 0 };
        double[] H = { 0, 0, 0, 0, 0, 0 };

        public double[] xi = null;
        public double[] eta = null;
        public double[] zeta = null;
        public double[] weight = null;
        /// <summary>
        /// Количество точек интегрирования
        /// </summary>
        public int CountIntegr
        {
            get { return xi == null ? 0 : xi.Length; }
        }

        List<double> Xi = new List<double>();
        List<double> Eta = new List<double>();
        List<double> Zeta = new List<double>();
        List<double> Weight = new List<double>();
        //
        public NumInegrationPoints()
        {
        }
        public NumInegrationPoints(int _Rang, TypeFunForm ff)
        {
            SetInt(_Rang, ff);
        }
        // ----------------------------------------------------------------------------
        void Clear()
        {
            Xi.Clear();
            Eta.Clear();
            Weight.Clear();
        }
        // ----------------------------------------------------------------------------
        public void SetInt(int _Rang, FFGeomType _Type)
        {
            Rang = _Rang;
            Type = _Type;
            if (FFGeomType.Line != _Type)
                Dim = 2;
            else
                Dim = 1;
            Set();
        }
        public void SetInt(int _Rang, TypeFunForm ff)
        {
            Rang = _Rang;
            if (ff > TypeFunForm.Form_1D_L3)
            {
                Dim = 2;
                if (ff > TypeFunForm.Form_2D_Triangle_L3)
                    Type = FFGeomType.Rectangle;
                else
                    Type = FFGeomType.Triangle;
            }
            else
            {
                Dim = 1;
                Type = FFGeomType.Line;
            }
            Set();
        }
        void Set()
        {
            Clear();
            if (Type == FFGeomType.Triangle) //трехугольная область
                IntTriang();
            else  //  прямоугольная или одномерная область
                IntRectang();
            xi = new double[Xi.Count];
            weight = new double[Xi.Count];
            if (Dim > 1)
            {
                eta = new double[Xi.Count];
                if (Dim > 2)
                    zeta = new double[Xi.Count];
            }
            for (int i = 0; i < xi.Length; i++)
            {
                xi[i] = Xi[i];
                weight[i] = Weight[i];
                if (Dim > 1)
                {
                    eta[i] = Eta[i];
                    if (Dim > 2)
                        zeta[i] = Zeta[i];
                }
            }
            Clear();
        }
        // ----------------------------------------------------------------------------
        void IntRectang()
        {
            switch (Rang)
            {
                case 1:
                    Q[0] = 0.0000000000000000;
                    H[0] = 2.0000000000000000;
                    break;
                case 2:
                    Q[0] = -0.577350269189626;
                    Q[1] = 0.577350269189626;

                    H[0] = 1.000000000000000;
                    H[1] = 1.000000000000000;

                    break;
                case 3:                       // 3 точки интегрирования
                    Q[0] = -0.774596669241483;
                    Q[1] = 0.000000000000000;
                    Q[2] = 0.774596669241483;

                    H[0] = 0.555555555555556;
                    H[1] = 0.888888888888889;
                    H[2] = 0.555555555555556;
                    break;
                case 4:                       // 4 точки интегрирования
                    Q[0] = -0.861136311594053;
                    Q[1] = -0.339981043584856;
                    Q[2] = 0.339981043584856;
                    Q[3] = 0.861136311594053;

                    H[0] = 0.347854845137454;
                    H[1] = 0.652145154862546;
                    H[2] = 0.652145154862546;
                    H[3] = 0.347854845137454;
                    break;
                case 5:                       // 5 точек интегрирования
                    Q[0] = -0.906179845938664;
                    Q[1] = -0.538469310105683;
                    Q[2] = 0.000000000000000;
                    Q[3] = 0.538469310105683;
                    Q[4] = 0.906179845938664;

                    H[0] = 0.236926885056189;
                    H[1] = 0.478628670499366;
                    H[2] = 0.569888888888889;
                    H[3] = 0.478628670499366;
                    H[4] = 0.236926885056189;
                    break;
                case 6:                      // 6 точек интегрирования
                    Q[0] = -0.932469514203152;
                    Q[1] = -0.661209386466264;
                    Q[2] = -0.238619186083197;
                    Q[3] = 0.238619186083197;
                    Q[4] = 0.661209386466264;
                    Q[5] = 0.932469514203152;

                    H[0] = 0.171324492379170;
                    H[1] = 0.360761573048138;
                    H[2] = 0.467913934572691;
                    H[3] = 0.467913934572691;
                    H[4] = 0.360761573048138;
                    H[5] = 0.171324492379170;
                    break;
            }
            int i, j, k;
            switch (Dim)
            {
                case 1:
                    for (i = 0; i < Rang; i++)
                    {
                        Xi.Add(Q[i]);
                        Weight.Add(H[i]);
                    }
                    break;
                case 2:
                    for (i = 0; i < Rang; i++)
                        for (j = 0; j < Rang; j++)
                        {
                            Xi.Add(Q[i]);
                            Eta.Add(Q[j]);
                            Weight.Add(H[i] * H[j]);
                        }
                    break;
                case 3:
                    for (i = 0; i < Rang; i++)
                        for (j = 0; j < Rang; j++)
                            for (k = 0; k < Rang; k++)
                            {
                                Xi.Add(Q[i]);
                                Eta.Add(Q[j]);
                                Zeta.Add(Q[k]);
                                Weight.Add(H[i] * H[j] * H[k]);
                            }
                    break;
            }
        }
        // ----------------------------------------------------------------------------
        void IntTriang()
        {
            // схемы интегрирования плоскких трехугольных КЭ
            switch (Rang)
            {
                case 1: // первого порядка
                        // координаты интегрирования по xi
                    Xi.Add(1.0 / 3.0);
                    // координаты интегрирования по eta
                    Eta.Add(1.0 / 3.0);
                    // координаты интегрирования по zeta
                    Zeta.Add(0);
                    // весовые коэффициенты
                    Weight.Add(0.5);
                    break;
                case 2:           //
                    Xi.Add(0.166666666666667);
                    Xi.Add(0.666666666666667);
                    Xi.Add(0.166666666666667);
                    // координаты интегрирования по eta
                    Eta.Add(0.166666666666667);
                    Eta.Add(0.166666666666667);
                    Eta.Add(0.666666666666667);
                    // координаты интегрирования по zeta
                    Zeta.Add(0);
                    Zeta.Add(0);
                    Zeta.Add(0);
                    // весовые коэффициенты
                    Weight.Add(0.166666666666667);
                    Weight.Add(0.166666666666667);
                    Weight.Add(0.166666666666667);
                    break;
                case 3:           // третьего порядка
                    //// координаты интегрирования по xi
                    //Xi.Add(0.5);
                    //Xi.Add(0.0);
                    //Xi.Add(0.5);
                    //// координаты интегрирования по eta
                    //Eta.Add(0.5);
                    //Eta.Add(0.5);
                    //Eta.Add(0.0);
                    //// координаты интегрирования по zeta
                    //Zeta.Add(0);
                    //Zeta.Add(0);
                    //Zeta.Add(0);
                    //// весовые коэффициенты
                    //Weight.Add(1.0 / 6.0);
                    //Weight.Add(1.0 / 6.0);
                    //Weight.Add(1.0 / 6.0);
                    //break;
                case 4:           //
                case 5:           //
                case 6:           //
                case 7:           // седьмого порядка
                    Xi.Add(1.0 / 3.0);
                    Xi.Add(0.05971587);
                    Xi.Add(0.4701421);
                    Xi.Add(0.4701421);
                    Xi.Add(0.797427);
                    Xi.Add(0.1012865);
                    Xi.Add(0.1012865);

                    Eta.Add(1.0 / 3.0);
                    Eta.Add(0.4701421);
                    Eta.Add(0.05971587);
                    Eta.Add(0.4701421);
                    Eta.Add(0.1012865);
                    Eta.Add(0.797427);
                    Eta.Add(0.1012865);
                    //
                    for (int i = 0; i < 7; i++)
                        Zeta.Add(0.0);
                    //
                    Weight.Add(0.1125);
                    Weight.Add(0.06619708);
                    Weight.Add(0.06619708);
                    Weight.Add(0.06619708);
                    Weight.Add(0.06296959);
                    Weight.Add(0.06296959);
                    Weight.Add(0.06296959);
                    break;
            }
        }

        public void ReSize(int Count)
        {
            MEM.MemResizeCopy<double>(ref xi, Count);
            MEM.MemResizeCopy<double>(ref weight, Count);
            if (Dim == 2)
                MEM.MemResizeCopy<double>(ref eta, Count);
            if (Dim == 3)
                MEM.MemResizeCopy<double>(ref zeta, Count);
        }
    }
}
