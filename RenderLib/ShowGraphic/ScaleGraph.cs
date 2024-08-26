//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                      07 04 2016 Потапов И.И.
//---------------------------------------------------------------------------
namespace RenderLib
{
    using System.Linq;
    static class ScaleGraph
    {
        public static double[] scale = { 0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1.0 };

        public static string GetString(string name)
        {
            string[] SS = name.Split('.', ',');

            int[] flag = { 1, 1, 1, 1 };
            for (int i = flag.Length - 1; i > 0; i--)
                if (SS[1][i] == '0')
                    flag[i] = 0;
                else
                    break;

            int LS = flag.Sum();

            string SLength = SS[0] + "." + SS[1].Substring(0, LS);
            return SLength;
        }
        public static string Scale(double Length, int idx)
        {
            int i = idx % 11;
            //деления на координатных осях
            string SLengthF = (scale[i] * Length).ToString("F4");
            return GetString(SLengthF);
        }
        public static string ScaleMinMax(double Max, double Min, int idx)
        {
            int i = idx % 11;
            double Length = Max - Min;
            //деления на координатных осях
            string SLengthF = (Min + scale[i] * Length).ToString("F4");
            return GetString(SLengthF);
        }
    }
}
