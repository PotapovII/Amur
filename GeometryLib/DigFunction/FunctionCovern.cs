//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 25.10.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace GeometryLib
{
    using System;
    /// <summary>
    /// ОО: Класс - для загрузки начальной геометрии русла в створе реки
    ///  Каверна
    ///  |---------------LL ------------------|
    ///  0 ---- 1                4--------5 ---     
    ///      L1 | \      L2    / |   L3       |
    ///         |  \          /  |            | H
    ///         |   \        /   |            | 
    ///         |    2------3    |          ----
    ///         | Lm1|      |Lm2 |
    ///         |____|      |____|
    ///  LL = 3*L + 2*Lm
    ///  Параметры профиля L1,L2,L3, Lm1,Lm2, H
    /// </summary>
    [Serializable]
    public class FunctionCovern : AbDigFunction
    {
        public FunctionCovern()
        {
            ///  Параметры профиля L1,L2,L3, Lm1,Lm2, H
            double[] xx = { 0, 1, 2, 3, 4, 5 };
            double[] yy = { 1, 1, 0, 0, 1, 1 };
            Init(xx[1] - xx[0], xx[2] - xx[1], xx[3] - xx[2], xx[4] - xx[3], xx[5] - xx[6], yy[2] - yy[3]);
        }
        public FunctionCovern(double L1, double Lm1, double L2, double Lm2, double L3, double H)
        {
            Init(L1, Lm1, L2, Lm2, L3, H);
        }
        public void Init(double L1, double Lm1, double L2, double Lm2, double L3, double H)
        {
            name = "Функция трапецвидной каверны";
            x0.Add(0); 
            x0.Add(L1); 
            x0.Add(Lm1 + L1); 
            x0.Add(L2 + Lm1 + L1); 
            x0.Add(Lm2 + L2 + Lm1 + L1); 
            x0.Add(L3 + Lm2 + L2 + Lm1 + L1);
            y0.Add(0); 
            y0.Add(0); 
            y0.Add(H); 
            y0.Add(H); 
            y0.Add(0); 
            y0.Add(0);
        }
    }
}
