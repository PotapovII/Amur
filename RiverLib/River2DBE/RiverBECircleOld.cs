////---------------------------------------------------------------------------
////                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
////            перенесено с правкой : 26.04.2021 Потапов И.И. 
////---------------------------------------------------------------------------
//// разделен на абстрактную и производную часть : 26.02.2022 Потапов И.И. 
////---------------------------------------------------------------------------
//// Решение с помощью циклического метода ГЭ задачи
//// о деформировании дна под цилиндром при его обтекании
//// гидродинамическим потоком
////---------------------------------------------------------------------------
//namespace RiverLib
//{
//    using AlgebraLib;
//    using CommonLib.Math;
//    using MemLogLib;
//    using System;
//    [Serializable]
//    public class RiverBECircleOld : ARiverBEM
//    {
//        /// <summary>
//        /// Создает экземпляр класса
//        /// </summary>
//        /// <returns></returns>
//        public override IRiver Clone()
//        {
//            return new RiverBECircleOld(new RiverBECircleParams());
//        }
//        /// <summary>
//        /// версия дата последних изменений интерфейса задачи
//        /// </summary>
//        public override string VersionData() => "RiverBECircleOld 01.09.2021/26.02.2022"; 
//        /// <summary>
//        /// Установка свойств задачи
//        /// </summary>
//        /// <param name="p"></param>
//        public override void SetParams(object p)
//        {
//            base.SetParams((RiverBECircleParams)p);
//            InitTask();
//        }
//        /// <summary>
//        /// Конструктор
//        /// </summary>
//        public RiverBECircleOld(RiverBECircleParams p) : base(p)
//        {
//            name = "гидрадинамика потока под трубой (МГЭ) старый вариант"; 
//            InitTask();
//        }
//        /// <summary>
//        /// Расчет полей глубины и скоростей на текущем шаге по времени
//        /// </summary>
//        public override void SolverStep()
//        {
//            try
//            {
//                bool flagGamma = true;
//                if (flagGamma == true)
//                {
//                    int NDU = 10;
//                    double GammaMin = -5;
//                    double GammaMax = 1;
//                    double dGamma = Math.Abs(GammaMax - GammaMin)/(NDU-1);
//                    double[] dG = new double[NDU];
//                    double[] dU = new double[NDU];
//                    Gamma = GammaMin;
//                    for (int ig = 0; ig < NDU; ig++)
//                    {
//                        SolverStepForGamma(Gamma);
//                        // скорости на цилиндре
//                        for (int j = 0; j < NNb; j++)
//                            VCircle[j] = FF[NNw + j];
//                         // VCircle[j] = U_inf * dXdZetaB[j] / detJB[j] - FF[NNw + j];
//                        dG[ig] = Gamma;
//                        double VCircleMax = DMath.Max(VCircle, 2 * Math.PI).yMax;
//                        double VCircleMin = DMath.Min(VCircle, 2 * Math.PI).yMin;
//                        dU[ig] = VCircleMax + VCircleMin;
//                        //Console.WriteLine("Gamma = {2}  Ua = {0}, Ub = {1}", VCircle[NNb / 2], VCircle[NNb -1], Gamma);
//                        //dU[ig] = Math.Abs(FF[NNw + NNb / 2]) + Math.Abs(FF[NNw + NNb]);
//                        //Console.WriteLine("Ua = {0}, Ub = {1}", FF[NNw + NNb / 2], FF[NNw + NNb]);
//                        Gamma += dGamma;
//                    }
//                    // найденная циркуляция
//                    Gamma =  DMath.RootFun(dG, dU).xRoot;
//                    Console.WriteLine("Gamma = {2}  Ua = {0}, Ub = {1}", VCircle[NNb / 2], VCircle[NNb - 1], Gamma);
//                }
//                else
//                {
//                    Gamma = -0.5;
//                }
//                // решение задачи для заданной циркуляции
//                SolverStepForGamma(Gamma);
//                for (int j = 0; j < NNb; j++)
//                    VCircle[j] = FF[NNw + j];
//                //  скорости на дне
//                for (int j = 0; j < NNw; j++)
//                    VC[j] = U_inf * dXdZetaW[j] / detJW[j] - FF[j];

//                // скорости на цилиндре
//                for (int j = 0; j < NNb; j++)
//                    VC[NNw + j] = U_inf * dXdZetaB[j] / detJB[j] - FF[NNw + j];

//                for (int j = 0; j < NNb; j++)
//                    VCircle[j] = U_inf * dXdZetaB[j] / detJB[j] - FF[NNw + j];

//                // Фильтрация скоростей
//                alglib.spline1dinterpolant c = new alglib.spline1dinterpolant();
//                alglib.spline1dfitreport rep = new alglib.spline1dfitreport();
//                double[] arg = fxW;
//                MEM.AllocClear(NW, ref SV);
//                MEM.AllocClear(NW, ref VCC);
//                for (int j = 0; j < NW; j++)
//                    VCC[j] = VC[j];
//                int j0 = NW / 10 + 1;
//                for (int j = 0; j < j0; j++)
//                    VCC[j] = VC[j0];
//                j0 = NW - j0;
//                for (int j = j0; j < NW; j++)
//                    VCC[j] = VC[j0];

//                int info = 0;
//                //аппроксимируем V и сглаживаем по кубическому сплайну
//                alglib.spline1dfitpenalized(arg, VCC, arg.Length, Flexibility, Hardness, out info, out c, out rep);
//                for (int i = 0; i < NW; i++)
//                    SV[i] = (float)alglib.spline1dcalc(c, arg[i]);

//                for (int j = 0; j < NW; j++)
//                    tauX[j] = Rho * Lambda * SV[j] * SV[j] / 2.0;
//            }
//            catch(Exception ex)
//            {
//                Logger.Instance.Exception(ex);
//            }
//        }

//        /// <summary>
//        /// Расчет полей глубины и скоростей на текущем шаге по времени
//        /// </summary>
//        public override void SolverStepForGamma(double Gamma)
//        {
//            MEM.Alloc<double>(Count, ref VC);
//            MEM.Alloc<double>(Count, ref SV);
//            MEM.Alloc<double>(Count, ref tauX);

//            // вычисление матрицы системы
//            Aww = CalkMatrixWW();
//            Awb = CalkMatrixWB();
//            Abw = CalkMatrixBW();
//            Abb = CalkMatrixBB();

//            Bww = CalkMatrixF_WW();
//            Bwb = CalkMatrixF_WB();
//            Bbw = CalkMatrixF_BW();
//            Bbb = CalkMatrixF_BB();

//            // Сборка Matrix и R
//            for (int i = 0; i < NNw; i++)
//                for (int j = 0; j < NNw; j++)
//                    Matrix[i][j] = Aww[i, j];
//            for (int i = 0; i < NNw; i++)
//                for (int j = 0; j < NNb; j++)
//                    Matrix[i][j + NNw] = Awb[i, j];
//            for (int i = 0; i < NNb; i++)
//                for (int j = 0; j < NNw; j++)
//                    Matrix[i + NNw][j] = Abw[i, j];
//            for (int i = 0; i < NNb; i++)
//                for (int j = 0; j < NNb; j++)
//                    Matrix[i + NNw][j + NNw] = Abb[i, j];
//            for (int i = 0; i < NNw; i++)
//            {
//                double sum = 0;
//                for (int j = 0; j < NNb; j++)
//                    sum += Bwb[i, j];
//                Matrix[i][NNw + NNb] = -Math.PI - sum;
//                Matrix[i][NNw + NNb + 1] = sum;
//            }
//            for (int i = 0; i < NNw; i++)
//            {
//                double sumW = 0;
//                for (int j = 0; j < NNw; j++)
//                    sumW += Bww[i, j] * fyW[j];
//                double sumB = 0;
//                for (int j = 0; j < NNb; j++)
//                    sumB += Bwb[i, j] * fyB[j];
//                R[i] = U_inf * (-Math.PI * fyW[i] + sumW + sumB);
//            }
//            // Уравнения : точка на теле
//            for (int i = 0; i < NNb; i++)
//            {
//                double sum = 0;
//                for (int j = 0; j < NNw; j++)
//                    sum += Bbw[i, j];
//                Matrix[NNw + i][NNw + NNb] = sum;
//                Matrix[NNw + i][NNw + NNb + 1] = -Math.PI - sum;
//            }
//            for (int i = 0; i < NNb; i++)
//            {
//                double sumW = 0;
//                for (int j = 0; j < NNw; j++)
//                    sumW += Bbw[i, j] * fyW[j];
//                double sumB = 0;
//                for (int j = 0; j < NNb; j++)
//                    sumB += Bbb[i, j] * fyB[j];
//                R[NNw + i] = U_inf * (-Math.PI * fyB[i] + sumW + sumB);
//            }
//            // Предпоследнее уравнение : интеграл d Psi /d n по дну должен дать 0
//            for (int j = 0; j < NNw; j++)
//                Matrix[NNw + NNb][j] = detJW[j] / NNw;

//            for (int j = 0; j < NNb; j++)
//                Matrix[NNw + NNb + 1][NNw + j] = detJB[j] / NNb;

//            R[NNw + NNb] = - Gamma;
//            R[NNw + NNb + 1] =   Gamma;

//            if (algebra == null)
//                algebra = new AlgebraGauss(N);
//            else
//                algebra.Clear();
//            algebra.AddToMatrix(Matrix, knots);
//            algebra.AddToRight(R, knots);
//            algebra.Solve(ref FF);
//        }
//    }
//}
