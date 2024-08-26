//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                      07 04 2016 Потапов И.И.
//---------------------------------------------------------------------------
namespace RenderLib
{
    using System;
    using System.Collections.Generic;
    class ScaleValue
    {
        public static float[] GetScaleVector(float MinValue, float MaxValue)
        {
            List<float> list = new List<float>(11);
            float Value = MaxValue - MinValue;
            float mValue = Math.Abs(Value);
            int n = (int)Math.Log10(mValue);
            float ds = 0;
            if (n > 1)
                ds = (float)Math.Pow(10, n - 1);
            if (n >= 1 && n >= -1)
                ds = 0.1f;
            if (n < -1)
                ds = (float)Math.Pow(10, n - 1);
            for (int i = 0; i < 11; i++)
            {
                float s = ds * i;
                if (MinValue <= s && s <= MaxValue)
                    list.Add(s);
            }
            float[] mas = list.ToArray();
            return mas;
        }




        /// <summary>
        /// Авто округление чисел до второго значащего числа
        /// </summary>
        public static double ScaleConvert(double value)
        {
            double res = 0;
            if (Math.Abs(value) < 0.00000001)
                return 0;
            int sign = Math.Sign(value);
            value = Math.Abs(value);
            int n = (int)Math.Log10(value);
            if (value > 1)
            {
                double val = (double)Math.Pow(10, n);
                double dv = (double)Math.Pow(10, n - 1);
                for (int i = 1; i <= 1000; i++)
                {
                    res = val + dv * i;
                    if (res > value)
                        break;
                }
            }
            else
            {
                double val = (double)Math.Pow(10, n - 1);
                double dv = (double)Math.Pow(10, n - 2);
                for (int i = 1; i <= 10; i++)
                {
                    res = val + dv * i;
                    if (res > value)
                        break;
                }
            }
            return sign * res;
        }
    }
}
