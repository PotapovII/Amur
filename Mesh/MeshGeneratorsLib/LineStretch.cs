//---------------------------------------------------------------------------
//               Класс сплайн используется для аппроксимации 
//                              Потапов И.И.
//                        - (C) Copyright 2014 -
//                          ALL RIGHT RESERVED
//                              05.09.14
//---------------------------------------------------------------------------
//     Реализация для библиотеки решающих задачу обтекания тел 
//                     метода граничных элементов
//---------------------------------------------------------------------------

namespace MeshLib
{
    using System;
    /// <summary>
    /// Класс - одномерная функция растяжения и сгущения
    ///     По К. Флетчеру ВМ в ДЖ т. 2 ст. 123
    ///         (С) Потапов И.И. 23.11.2014
    /// </summary>
    [Serializable]
    public class LineStretch
    {
        /// <summary>
        /// демпфирующий параметр
        /// </summary>
        double Q = 2;
        /// <summary>
        /// наклон распределения P меньше 1 в лево, P больше 1 в право
        /// </summary>
        double P = 1;
        /// <summary>
        /// Локальное разбиение со сгущением сетки
        /// </summary>
        double[] s = null;

        public LineStretch()
        {
            int N = 20;
            s = new double[N];
            double ds = 1.0 / (N - 1);
            double tQ = Math.Tanh(Q);
            for (int i = 0; i < N; i++)
            {
                double x = ds * i;
                double txQ = Math.Tanh(Q * (1 - x));
                s[i] = P * x + (1 - P) * (1 - txQ / tQ);
            }
        }
        public LineStretch(int N, double q = 2, double p = 1)
        {
            if (Math.Abs(q) < 0.0001)
                Q = 0.0001;
            else
                Q = q;
            P = p;

            s = new double[N];
            double ds = 1.0 / (N - 1);
            double tQ = Math.Tanh(Q);
            for (int i = 0; i < N; i++)
            {
                double x = ds * i;
                double txQ = Math.Tanh(Q * (1 - x));
                s[i] = P * x + (1 - P) * (1 - txQ / tQ);
            }
        }

        public double[] GetCoords(double xw, double xe)
        {
            double[] x = new double[s.Length];
            for (int i = 0; i < x.Length; i++)
                x[i] = xw * s[i] + xe * (1 - s[i]);
            return x;
        }

        public double[,] GetCoords(double xk, double yk, double alpha)
        {
            double tf = 1;
            double[,] x = new double[2, s.Length];
            if (Math.Abs(tf) > 1.0e5)
            {
                for (int i = 0; i < x.Length; i++)
                {
                    x[0, i] = xk;
                    x[1, i] = 0 * (1 - s[i]) + yk * s[i];
                }
            }
            else
            {
                //double delta = yk * tf;
                //double xr = xk + delta;
                //double R = Math.Sqrt(yk * yk + delta * delta);
                //double Lx = Math.Sign(tf) * R - xr;
                //double phi = Math.Sign(tf)*(Math.PI/2*(1-alpha));
                //double d_phi = phi/()
                //for (int i = 0; i < x.Length; i++)
                //{
                //    //x[0, i] = xe * (1 - s[i]) + xw * s[i];
                //    //x[1, i] = ye * (1 - s[i]) + yw * s[i];
                //}
            }
            return x;
        }


        public void Test()
        {
            LineStretch t = new LineStretch(12, 3, 0.5);
            double[] xx = t.GetCoords(1, 3.5);
        }
    }
}
