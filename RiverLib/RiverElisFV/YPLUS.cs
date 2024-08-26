using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiverLib
{
    public class YPLUS
    {
        alglib.spline1dinterpolant bufF = null;
        double[] F = new double[] { 2, 7.89, 17.183, 29.276, 43.97, 142.7, 401.79, 696.68 };
        double[] y_p = new double[] { 2, 4, 6, 8, 10, 20, 40, 60 };
        public YPLUS()
        {
            alglib.spline1dbuildcubic(F, y_p, out bufF);
        }
        public double Yplus(double arg)
        {
            return alglib.spline1dcalc(bufF, arg);
        }
    }

}
