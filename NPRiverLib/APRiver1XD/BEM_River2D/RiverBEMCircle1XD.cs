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
//               Реверс в NPRiverLib 24.07.2024 Потапов И.И.
//---------------------------------------------------------------------------

namespace NPRiverLib.APRiver1XD.BEM_River2D
{
    using System;

    using MemLogLib;
    using AlgebraLib;
    using GeometryLib;
    using NPRiverLib.IO;

    using CommonLib;
    using CommonLib.IO;
    using CommonLib.Physics;

    [Serializable]
    public class RiverBEMCircle1XD : ARiverBEM1XD
    {
        /// <summary>
        /// период пересчета циркуляции
        /// </summary>
        protected int CountGamma = 0;
        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public override IRiver Clone()
        {
            return new RiverBEMCircle1XD(new RiverBEMParams1XD());
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        public RiverBEMCircle1XD(RiverBEMParams1XD p) : base(p)
        {
            name = "Поток идеальной жидкости под трубой (МГЭ) 2XD";
            Version = "RiverBEMCircle1XD 24.07.2024";
        }
        /// <summary>
        /// Расчет полей глубины и скоростей на текущем шаге по времени
        /// </summary>
        public override void SolverStep()
        {
            try
            {
                if (CountGamma % 5 == 0)
                {
                    if (Params.flagGamma == true)
                    {
                        Gamma = LookingGamma();
                        Console.WriteLine("Gamma = {2}  Ua = {0}, Ub = {1}", VCircle[NNb / 2], VCircle[NNb - 1], Gamma);
                    }
                    else
                    {
                        //Gamma = 0;
                        Gamma = 0 * Params.U_inf * Params.RC;
                    }
                    gammaList.Add(Gamma);
                    timeList.Add(time);
                }
                // CountGamma++;
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
                double Lambda = SPhysics.PHYS.Lambda;
                for (int j = 0; j < Params.NW; j++)
                    tauX[j] = SPhysics.rho_w * Lambda * SV[j] * SV[j] / 2.0;
            }
            catch(Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
            time += dtime;
        }
        /// Создает экземпляр класса конвертера
        /// </summary>
        /// <returns></returns>
        public override IOFormater<IRiver> GetFormater()
        {
            return new TaskReaderBEM_1XD();
        }

        #region Локальные методы
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
        public override void InitGeomenryTask(double scale = 1)
        {
            InitBattomGeomenry();
            InitBodyGeomenry(scale);
        }
        /// <summary>
        ///  вычисление геометрии дна
        /// </summary>
        public override void InitBattomGeomenry()
        {
            // равномерная сетка
            if (Params.MeshType == true)
            {
                double dx = Params.LW * 1.0 / NNw;
                for (uint i = 0; i < Params.NW; i++)
                {
                    double x = dx * (i + 1);
                    fxW[i] = x;
                    double arg = Params.AMPCowern * (x - Params.XC) / Params.RC;
                    fyW[i] = -Params.AlphaCowern * Math.Exp(-arg * arg);
                }
            }
            else
            {
                for (uint i = 0; i < Params.NW; i++)
                {
                    double xi = 1.0 * i / (NNw - 1);
                    fxW[i] = Params.LW * FS(xi);
                    double arg = Params.AMPCowern * (fxW[i] - Params.XC);
                    fyW[i] = -Params.AlphaCowern * Math.Exp(-arg * arg);
                }
            }
        }
        /// <summary>
        ///  вычисление геометрии обтекаемого тела
        /// </summary>
        public override void InitBodyGeomenry(double scale = 1)
        {
            double dx = 1.0 / NNb;
            // форма обтекаемого тела
            for (uint i = 0; i < Params.NB / 2; i++)
            {
                double x = dx * (i + 1);
                fxB[i] = Params.RC * Math.Cos(-2 * Math.PI * x - Math.PI / 2);
                fyB[i] = Params.RC * Math.Sin(-2 * Math.PI * x - Math.PI / 2);
            }
            double RE = scale * Params.EllipsRC;
            for (uint i = (uint)(Params.NB / 2); i < Params.NB; i++)
            {
                double x = dx * (i + 1);
                fxB[i] = RE * Math.Cos(-2 * Math.PI * x - Math.PI / 2);
                fyB[i] = Params.RC * Math.Sin(-2 * Math.PI * x - Math.PI / 2);
            }
            double alpha = Params.Alpha / 180.0 * Math.PI;
            double cos = Math.Cos(alpha);
            double sin = Math.Sin(alpha);
            // поворот обтекаемого тела
            for (uint i = 0; i < Params.NB; i++)
            {
                double x = fxB[i];
                double y = fyB[i];
                fxB[i] = x * cos - y * sin;
                fyB[i] = x * sin + y * cos;
            }
            // сдвиг
            for (uint i = 0; i < Params.NB; i++)
            {
                fxB[i] += Params.XC;
                fyB[i] += Params.YC;
            }
        }
        /// <summary>
        /// Поиск циркуляции
        /// </summary>
        /// <param name="Gamma"></param>
        public override double LookingGamma()
        {
            int NDU = 10;
            double GammaMin = -2;
            double GammaMax = 1;
            double dGamma = Math.Abs(GammaMax - GammaMin) / (NDU - 1);
            double[] dG = new double[NDU];
            double[] dU = new double[NDU];
            double gamma = GammaMin;
            for (int ig = 0; ig < NDU; ig++)
            {
                SolverStepForGamma(gamma);
                // скорости на цилиндре
                for (int j = 0; j < NNb; j++)
                    VCircle[j] = FF[NNw + j];
                dG[ig] = gamma;
                var rootMax = DMath.Max(VCircle, 2 * Math.PI);
                double VCircleMax = rootMax.yMax;
                var rootMin = DMath.Min(VCircle, 2 * Math.PI);
                double VCircleMin = rootMin.yMin;
                dU[ig] = VCircleMax + VCircleMin;
                gamma += dGamma;
            }
            // найденная циркуляция
            gamma = DMath.RootFun(dG, dU).xRoot;
            return gamma;
        }
        /// <summary>
        /// Расчет полей глубины и скоростей на текущем шаге по времени
        /// </summary>
        public override void SolverStepForGamma(double Gamma)
        {

            CalkMatrixWW();
            CalkMatrixWB();
            CalkMatrixBW();
            CalkMatrixBB();

            // Сборка Matrix и R
            for (int i = 0; i < NNw; i++)
                for (int j = 0; j < NNw; j++)
                    Matrix[i][j] = Aww[i, j];
            for (int i = 0; i < NNw; i++)
                for (int j = 0; j < NNb; j++)
                    Matrix[i][j + NNw] = Awb[i, j];
            for (int i = 0; i < NNb; i++)
                for (int j = 0; j < NNw; j++)
                    Matrix[i + NNw][j] = Abw[i, j];
            for (int i = 0; i < NNb; i++)
                for (int j = 0; j < NNb; j++)
                    Matrix[i + NNw][j + NNw] = Abb[i, j];

            for (int i = 0; i < NNw; i++)
            {
                Matrix[i][NNw + NNb] = -Math.PI;
                Matrix[i][NNw + NNb + 1] = 0;
            }
            for (int i = 0; i < NNw; i++)
            {
                R[i] = -Math.PI * fyW[i] * Params.U_inf;
            }
            // Уравнения : точка на теле
            for (int i = 0; i < NNb; i++)
            {
                Matrix[NNw + i][NNw + NNb] = Math.PI;
                Matrix[NNw + i][NNw + NNb + 1] = -2 * Math.PI;
            }
            for (int i = 0; i < NNb; i++)
            {
                R[NNw + i] = -Math.PI * fyB[i] * Params.U_inf;
            }
            // Предпоследнее уравнение : интеграл d Psi /d n по дну должен дать 0
            for (int j = 0; j < NNw; j++)
                Matrix[NNw + NNb][j] = -detJW[j] / NNw;

            for (int j = 0; j < NNb; j++)
                Matrix[NNw + NNb + 1][NNw + j] = -detJB[j] / NNb;

            R[NNw + NNb] = bottomPeriod * Params.U_inf + Gamma;
            R[NNw + NNb + 1] = -Gamma;

            if (algebra == null)
                algebra = new AlgebraGauss(N);
            else
                algebra.Clear();
            algebra.AddToMatrix(Matrix, knots);
            algebra.AddToRight(R, knots);
            algebra.Solve(ref FF);
        }

        #endregion
    }
}
