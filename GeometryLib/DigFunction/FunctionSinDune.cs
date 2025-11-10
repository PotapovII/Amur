//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 09.10.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace GeometryLib
{
    using MemLogLib;
    using System;
    /// <summary>
    /// ОО: Класс - для загрузки начальной геометрии русла в створе реки
    /// </summary>
    [Serializable]
    public class FunctionSinDune : FunctionSin 
    {
        /// <summary>
        /// Гауссовский холм
        /// </summary>
        public override double FunctionValue(double s)
        {
            double ss = (s - X0);
            double arg = k * ss;
            double y = 0;
            if (ss < 5*Length/8)
                y = Y0 + amplitude * Math.Sin(arg);
            else
                if (ss < 3 * Length / 4)
                    y = Y0 + amplitude * Math.Cos(arg);
            return y;
        }
        public FunctionSinDune(double amplitude = 0.05,int NN = 1) : base()
        {
            double[] xx = { 0, 1 };
            double[] yy = { 0, amplitude };
            SetFunctionData(xx, yy, "Функция Sin");
            Init(amplitude, NN);
        }
        public FunctionSinDune(double amplitude, double NN, double[] xx, double[] yy)
        {
            this.X0 = xx[0];
            this.Y0 = yy[0];
            SetFunctionData(xx, yy, "Функция Sin");
            Init(amplitude, NN);
        }
    }
}
