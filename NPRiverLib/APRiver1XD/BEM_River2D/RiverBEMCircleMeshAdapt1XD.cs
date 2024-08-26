//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//            перенесено с правкой : 26.04.2021 Потапов И.И. 
//---------------------------------------------------------------------------
// разделен на абстрактную и производную часть : 26.02.2022 Потапов И.И. 
//---------------------------------------------------------------------------
// Решение с помощью циклического метода ГЭ задачи
// о деформировании дна под цилиндром при его обтекании
// гидродинамическим потоком
//---------------------------------------------------------------------------

namespace NPRiverLib.APRiver1XD.BEM_River2D
{
    using System;
    using System.Collections.Generic;

    using MemLogLib;
    using GeometryLib;
    
    using CommonLib;
    using CommonLib.Physics;

    [Serializable]
    public class RiverBEMCircleMeshAdapt1XD : RiverBEMCircleMesh1XD
    {
        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public override IRiver Clone()
        {
            return new RiverBEMCircleMeshAdapt1XD(new RiverBEMParams1XD());
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        public RiverBEMCircleMeshAdapt1XD(RiverBEMParams1XD p) : base(p)
        {
            name = "поток идеальной жидкости под трубой с 0 циркуляцией (МГЭ) 2XD";
            Version = "RiverBEMCircleMeshAdapt1XD 24.07.2024";
        }
        /// <summary>
        /// Метод выделения ресурсов задачи после установки свойств задачи
        /// </summary>
        public override void InitTask()
        {
            // выделение памяти
            base.InitAreaTask();
            // Определение начальной формы цилиндра и дна
            InitGeomenryTask();
            // Вычисляемые параметры геометрии
            base.ClakGeomenryParams();
        }
        /// <summary>
        /// Определение формы цилиндра и дна
        /// </summary>
        public override void InitGeomenryTask(double scale=1)
        {
            InitBattomGeomenry();
            double x0 = Params.XC - 3* Params.RC;
            double x1 = Params.XC + 3* Params.RC;
            double minR = 9 * Params.RC * Params.RC;
            int minIndex =-1;
            for (int i=0; i<fxW.Length; i++)
            {
                if(fxW[i]>x0 && fxW[i] < x1)
                {
                    double r = (Params.XC - fxW[i]) * (Params.XC - fxW[i]) +
                               (Params.YC - fyW[i]) * (Params.YC - fyW[i]);
                    if(minR > r)
                    {
                        minR = r;
                        minIndex = i;
                    }
                }
            }
            if (minIndex > -1)
            {
                double h = Params.YC - fyW[minIndex];
                Params.Alpha = Math.Acos(h / Math.Sqrt(minR))/Math.PI*180;
            }
            else
                Params.Alpha = 0;
            base.InitGeomenryTask();
        }

        public void SetBodyGeometry(double scale = 1)
        {
            InitBodyGeomenry(scale);
            initB(fxB, fyB);
        }

        /// <summary>
        /// Расчет полей глубины и скоростей на текущем шаге по времени
        /// </summary>
        public override void SolverStep()
        {
            try
            {
                double Lambda = SPhysics.PHYS.Lambda;
                InitGeomenryTask();
                ClakGeomenryParams();
                Params.Alpha = 0;
                int CountScale = 40;
                double scale = 0.3;
                double Dscale = 0.4;
                List<double> dSS = new List<double>();
                List<double> dGG = new List<double>();
                // цикл по масштабу отрывной зоны
                for (int idx=0; idx< CountScale; idx++)
                {
                    SetBodyGeometry(scale);
                    Gamma = LookingGamma();
                    dSS.Add(scale);
                    dGG.Add(Gamma);
                    if (Gamma > 0)
                        break;
                    scale += Dscale;
                }
                // найденный маштаб для нулевой циркуляции
                double SCASLE = 1;
                if(dSS.Count>3)
                    SCASLE = DMath.RootFun(dSS.ToArray(), dGG.ToArray()).xRoot;
                SetBodyGeometry(SCASLE);
                Gamma = 0;
                //CountGamma++;
                // решение задачи для заданной циркуляции
                SolverStepForGamma(Gamma);
                ////  скорости на дне
                for (int j = 0; j < NNw; j++)
                {
                    VC[j] = -FF[j];
                    SV[j] = -FF[j];
                }
                if (Params.FlagFlexibility == true)
                {
                    // Фильтрация скоростей
                    alglib.spline1dinterpolant c = new alglib.spline1dinterpolant();
                    alglib.spline1dfitreport rep = new alglib.spline1dfitreport();
                    double[] arg = fxW;
                    MEM.AllocClear(Params.NW, ref SV);
                    MEM.AllocClear(Params.NW, ref VCC);
                    for (int j = 0; j < Params.NW; j++)
                        VCC[j] = VC[j];
                    int j0 = Params.NW / 10 + 1;
                    for (int j = 0; j < j0; j++)
                        VCC[j] = VC[j0];
                    j0 = Params.NW - j0;
                    for (int j = j0; j < Params.NW; j++)
                        VCC[j] = VC[j0];
                    int info = 0;
                    //аппроксимируем V и сглаживаем по кубическому сплайну
                    alglib.spline1dfitpenalized(arg, VCC, arg.Length,
                        Params.Flexibility, Params.Hardness, out info, out c, out rep);
                    for (int i = 0; i < Params.NW; i++)
                        SV[i] = (float)alglib.spline1dcalc(c, arg[i]);
                }
                for (int j = 0; j < Params.NW; j++)
                    tauX[j] = SPhysics.rho_w * Lambda * SV[j] * SV[j] / 2.0;
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
            time += dtime;
        }
    }

}
