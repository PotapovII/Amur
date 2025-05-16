namespace GeometryLib
{
    using System;
    [Serializable]
    public class TSpline
    {
        protected double[] a = null;
        protected double[] b = null;
        protected double[] c = null;
        protected double[] d = null; //
        protected double[] fun = null;        // адрес значений переменной
        protected uint N = 0;           //
        protected double[] f = null;
        protected double[] x = null;
        protected double fMin = 0, fMax = 0, xMin = 0, xMax = 0;
        //---------------------------------------------------------------------------
        public void Set(double[] f, double[] x, uint n = 0)
        {
            try
            {
                this.f = f;
                this.x = x;
                GetMinMax(f, out fMin, out fMax);
                if (n == 0)
                    N = (uint)x.Length;
                else
                    N = n;
                a = new double[N];
                b = new double[N];
                c = new double[N];
                d = new double[N];
                fun = new double[N];
                //
                double[] P = new double[N];
                double[] Q = new double[N];

                P[0] = Q[0] = 0;
                for (uint i = 1; i < N - 1; i++)
                {
                    double h_i = x[i] - x[i - 1];
                    double h_i1 = x[i + 1] - x[i];
                    double k = 6.0 * ((f[i] - f[i - 1]) / h_i - (f[i + 1] - f[i]) / h_i1);

                    P[i] = h_i1 / (-2.0 * (h_i + h_i1) - h_i * P[i - 1]);
                    Q[i] = (k + h_i * Q[i - 1]) / (-2.0 * (h_i + h_i1) - h_i * P[i - 1]);
                }
                c[N - 1] = 0;
                for (int i = (int)N - 2; i >= 0; i--) c[i] = P[i] * c[i + 1] + Q[i];
                for (uint i = 1; i < N; i++)
                {
                    double h = x[i] - x[i - 1];
                    d[i] = (c[i] - c[i - 1]) / h;
                    a[i] = f[i];
                    b[i] = h * c[i] / 2.0 - h * h * d[i] / 6.0 + (f[i] - f[i - 1]) / h;
                }

                for (uint i = 0; i < N; i++) fun[i] = x[i];

                if (N > 0)
                {
                    xMin = Value(f[0]);
                    xMax = Value(f[0]);
                }

                for (uint i = 1; i < N; i++)
                {
                    if (Value(f[i]) < xMin) xMin = Value(f[i]);
                    if (Value(f[i]) > xMax) xMax = Value(f[i]);
                }
            }
            catch
            {
                Console.WriteLine("Разрушение сплайн функции");
            }
        }

        //  значение функции
        public double Value(double arg)
        {
            bool ck = true;
            uint i = 1;
            while (arg > fun[i] && ck)
            {
                i++;
                if (i >= N)
                {
                    i = N - 1; ck = false;
                }
            }
            double rez = arg - fun[i];
            return (a[i] + rez * (b[i] + rez * (c[i] * 0.5 + d[i] * rez / 6.0)));
        }
        public double GetF(int Index)
        {
            return f[Index];
        }
        public double GetX(int Index)
        {
            return x[Index];
        }
        void GetMinMax(double[] arr, out double Min, out double Max)
        {
            Min = 0;
            Max = 0;

            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] < Min) Min = arr[i];
                if (arr[i] > Max) Max = arr[i];
            }
        }
        public double GetMinF()
        {
            return fMin;
        }
        public double GetMinX()
        {
            return xMin;
        }
        public double GetMaxF()
        {
            return fMax;
        }
        public double GetMaxX()
        {
            return xMax;
        }
    }
}
