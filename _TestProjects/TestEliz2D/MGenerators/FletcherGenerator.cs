using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Cloo;
using ManagedCuda;
using ManagedCuda.BasicTypes;
using ManagedCuda.VectorTypes;
using GeometryLib;
using GeometryLib.Areas;

namespace TestEliz2D
{
    // // ////////////////////////////////не переименовывать классы Gererator и Parameter!!!! от имени зависит положение в листбоксе контрола
    public class FletcherGenerator : PointsGeneratorEliz
    {
        string exception = "";
        //точные координаты дна
        double[] Eta;
        //точные координаты свободной поверхности
        double[] Z;
        // примерные координаты 4х слоев области
        double[] XS;
        double[] YS;
        //координаты 6ти граничных точек области (Флетчер, том 2, рис.13.25)
        double[] XB;
        double[] YB;
        //расстояние по AB
        double[] XA;
        double[] RA;
        //расстояние по FD
        double[] XF;
        double[] RF;
        // функции сгущения для граней
        double[] sAC;
        double[] sFD;
        double[] sAF;
        double[] sCD;
        // весовые функции
        double[] sH;
        //количество узлов сетки
        int N;
        // количество узлов по нижней границе по zeta
        int Nx = 51;
        // количество узлов по левой границе по eta
        int Ny = 51;
        // предварительные интерполяционные параметры для внутренних поверхностией Z2, Z3 области
        double S2 = 0.5f;
        double S3 = 0.5f;
        //параметр однородности внутренних точек
        double AW = 0.5f;
        //параметры сгущения
        double PAC = 1.0f;
        double QAC = 0.5f;
        double PFD = 1.0f;
        double QFD = 0.5f;
        double PAF = 1.0f;
        double QAF = 0.5f;
        double PCD = 1.0f;
        double QCD = 0.5f;
        //счетчик для ячеек криволинейной поверхности
        int l = 0;
        //
        double RAB = 0.0f;
        //
        OrderablePartitioner<Tuple<int, int>> rangePartitioner;
        ComputeContext context;
        ComputeCommandQueue commands;
        ComputeProgram program;
        ComputeKernel kernelSurch;
        ComputeKernel kernelIntGid;
        public override string Name
        {
            get { return "Генерация по Флетчеру"; }
        }
        //
        public FletcherGenerator() { }
        public FletcherGenerator(Parameter fp)
        {
            FletcherParameter p = fp as FletcherParameter;
            this.p = p;
            Nx = p.Nx;
            Ny = p.Ny;
            N = Nx * Ny;
            S2 = p.S2;
            S3 = p.S3;
            AW = p.AW;
            PAC = p.PAC;
            QAC = p.QAC;
            PFD = p.PFD;
            QFD = p.QFD;
            PAF = p.PAF;
            QAF = p.QAF;
            PCD = p.PCD;
            QCD = p.QCD;
            //   
            rangePartitioner = Partitioner.Create(0, Nx);
        }
        void InitialValues()
        {
            //
            XA = new double[Nx];
            RA = new double[Nx];
            //
            XB = new double[4];
            YB = new double[4];
            //
            X = new double[Nx * Ny];
            Y = new double[Nx * Ny];
            XS = new double[4 * Nx];
            YS = new double[4 * Nx];
            //
            sAC = new double[Nx];
            sFD = new double[Nx];
            sAF = new double[Ny];
            sCD = new double[Ny];
            sH = new double[4];
        }

        /// <summary>
        /// метод генерации сетки
        /// </summary>
        protected override void GenerateCoords(SimpleAreaProfile sArea)
        {
            // меняем NX на NX+1 для частей > 1
            Nx = sArea.XBottom.Length;
            //
            InitialValues();
            try
            {

                // по сравнению с алгоритмом из книги, верхняя и нижняя границы поменяны местами, 
                //чтобы нумерация узлов шла от верхнего левого угла к правому нижнему
                XB[0] = sArea.XTop[0];
                XB[1] = sArea.XTop[Nx - 1];
                XB[2] = sArea.XBottom[Nx - 1];
                XB[3] = sArea.XBottom[0];
                //
                YB[0] = sArea.YTop[0];
                YB[1] = sArea.YTop[Nx - 1];
                YB[2] = sArea.YBottom[Nx - 1];
                YB[3] = sArea.YBottom[0];
                //
                Eta = sArea.YTop;
                Z = sArea.YBottom;
                //определение растяжения узлов на всех границах
                sAC = Stretch(PAC, QAC, Nx);
                sFD = Stretch(PFD, QFD, Nx);
                sAF = Stretch(PAF, QAF, Ny);
                sCD = Stretch(PCD, QCD, Ny);
                //длина криволинейного участка в натуральном размере
                RAB = Foil0(Eta, out XA, out RA);  // для постоянного dx по Eta
                //расстояние нижней и верхней границы
                double RAC = RAB;
                double RFD = Foil0(Z, out XF, out RF);// для постоянного dx по Z
                //алгоритм
                double RAC_sj = 0, RFD_s = 0;
                //интерполированные координаты профиля для foil (были обезразмеренные, стали размерные)
                double XD = 0;
                double YD = 0;
                l = 2;
                for (int j = 0; j < Nx; j++)
                {
                    RAC_sj = sAC[j] * RAC;
                    // получаем координаты X Y на криволинейной грани AB через dx по x
                    // учтывается сгущение узлов
                    Foil1(RAC_sj, ref XD, ref YD, XA, RA, Eta);
                    XS[j] = XD;
                    YS[j] = YD;
                    ////сгущение узлов по дну не учитывается
                    //XS[j] = XA[j];
                    //YS[j] = Z[j];
                    RFD_s = sFD[j] * RFD;
                    // при закрытом канале
                    if (Z == null)
                    {
                        XS[3 * Nx + j] = XB[3] + (RFD_s) * (XB[2] - XB[3]) / (RFD);
                        YS[3 * Nx + j] = YB[3];
                    }
                    // при свободной поверхности
                    else
                    {
                        Foil1(RFD_s, ref XD, ref YD, XF, RF, Z);
                        XS[3 * Nx + j] = XD;
                        YS[3 * Nx + j] = YD;
                    }
                }
                //
                if ((YS[3 * Nx - 1] == 0) && (YS[Nx - 1] == 0))
                {
                    if ((XS[3 * Nx - 1] == 0) && (XS[Nx - 1] == 0))
                    {
                        YS[4 * Nx - 1] = YS[4 * Nx - 2];
                        YS[Nx - 1] = YS[Nx - 2];
                        XS[4 * Nx - 1] = XS[4 * Nx - 2];
                        XS[Nx - 1] = XS[Nx - 2];
                    }
                }
                if (Cuda)
                {
                    stopAll.Restart();
                    //
                    //
                    //Init Cuda context
                    bool bPinGenericMemory = true; // we want this to be the default behavior
                    CUCtxFlags device_sync_method = CUCtxFlags.BlockingSync; // by default we use BlockingSync
                    dim3 threads, blocks; // kernel launch configuration
                    int nstreams = 4;  // number of streams for CUDA calls
                    int threadsPerBlock = 512;

                    CudaContext ctx;
                    if (bPinGenericMemory)
                        ctx = new CudaContext(0, device_sync_method | CUCtxFlags.MapHost);
                    else
                        ctx = new CudaContext(0, device_sync_method);

                    string resName;
                    if (IntPtr.Size == 8)
                        resName = "FletcherGrid_x64.ptx";
                    else
                        resName = "FletcherGrid_x64.ptx"; //change in future for x32
                    string resNamespace = "MeshLib";
                    string resource = resNamespace + "." + resName;
                    Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
                    if (stream == null) throw new ArgumentException("Kernel not found in resources.");
                    CudaKernel SurchKernel = ctx.LoadKernelPTX(stream, "Surch");
                    CudaKernel FletcherGridKernel = ctx.LoadKernelPTX(stream, "InternalGrid");
                    //
                    stopW1.Restart();
                    //
                    float[] SS2 = { (float)S2, (float)S3 };
                    int[] N = { Nx, Ny };
                    float[] fXS = new float[XS.Length];
                    float[] fYS = new float[XS.Length];

                    for (int i = 0; i < XS.Length; i++)
                    {
                        fXS[i] = (float)XS[i];
                        fYS[i] = (float)YS[i];
                    }

                    CudaDeviceVariable<int> d_N = N;
                    CudaDeviceVariable<float> d_SS2 = SS2;
                    CudaDeviceVariable<float> d_XS = fXS;
                    CudaDeviceVariable<float> d_YS = fYS;
                    int N_threads = Nx;
                    threads = new dim3(threadsPerBlock, 1);
                    blocks = new dim3((N_threads + threadsPerBlock - 1) / (threadsPerBlock), 1);
                    SurchKernel.BlockDimensions = threads;
                    SurchKernel.GridDimensions = blocks;
                    //
                    stopW1.Stop();
                    //
                    stopCalc.Restart();
                    // Run Kernel
                    SurchKernel.Run(d_N.DevicePointer, d_SS2.DevicePointer, d_XS.DevicePointer, d_YS.DevicePointer);
                    //
                    stopCalc.Stop();
                    //
                    stopW1.Start();
                    //
                    fXS = d_XS;
                    fYS = d_YS;
                    // конвертация результатов в double
                    for (int i = 0; i < XS.Length; i++)
                    {
                        XS[i] = Convert.ToDouble(fXS[i]);
                        YS[i] = Convert.ToDouble(fYS[i]);
                    }

                    // 2nd kernel 
                    X = new double[Nx * Ny];
                    Y = new double[Nx * Ny];
                    float[] Ar = { (float)AW };
                    //конвертируем массивы в float
                    float[] Xf = new float[Nx * Ny];
                    float[] Yf = new float[Nx * Ny];
                    //float[] fXS = new float[XS.Length];
                    //float[] fYS = new float[YS.Length];
                    float[] fsCD = new float[sCD.Length];
                    float[] fsAF = new float[sAF.Length];
                    for (int i = 0; i < sCD.Length; i++)
                    {
                        fsCD[i] = (float)sCD[i];
                        fsAF[i] = (float)sAF[i];
                    }
                    CudaDeviceVariable<float> d_X = Xf;
                    CudaDeviceVariable<float> d_Y = Yf;
                    CudaDeviceVariable<float> d_sCD = fsCD;
                    CudaDeviceVariable<float> d_sAF = fsAF;
                    CudaDeviceVariable<float> d_Ar = Ar;
                    N_threads = Nx * Ny;
                    threads = new dim3(threadsPerBlock, 1);
                    blocks = new dim3((N_threads + threadsPerBlock - 1) / (threadsPerBlock), 1);
                    FletcherGridKernel.BlockDimensions = threads;
                    FletcherGridKernel.GridDimensions = blocks;
                    //
                    stopW1.Stop();
                    //
                    stopCalc.Start();
                    // Run Kernel
                    FletcherGridKernel.Run(d_X.DevicePointer, d_Y.DevicePointer, d_XS.DevicePointer, d_YS.DevicePointer, d_sCD.DevicePointer, d_sAF.DevicePointer, d_Ar.DevicePointer, d_N.DevicePointer);
                    //
                    stopCalc.Stop();
                    //
                    stopW1.Start();
                    //
                    Xf = d_X;
                    Yf = d_Y;
                    // конвертация результаты в double
                    for (int i = 0; i < X.Length; i++)
                    {
                        X[i] = Convert.ToDouble(Xf[i]);
                        Y[i] = Convert.ToDouble(Yf[i]);
                    }
                    // выравнивание накопившейся ошибки на правой грани расчетной области по x
                    for (int i = 1; i < Ny + 1; i++)
                        X[i * Nx - 1] = XB[1];

                    // Clean up after you!!!
                    d_N.Dispose();
                    d_SS2.Dispose();
                    d_XS.Dispose();
                    d_YS.Dispose();
                    d_X.Dispose();
                    d_Y.Dispose();
                    d_sCD.Dispose();
                    d_sAF.Dispose();
                    d_Ar.Dispose();
                    ctx.Dispose();
                    //
                    stopW1.Stop();
                    stopAll.Stop();
                    //
                    timeAll = stopAll.Elapsed;
                    timeCalculate = stopCalc.Elapsed;
                    timeTransrort = stopW1.Elapsed;

                }
                if (OpenCL)
                {
                    stopAll.Restart();
                    //
                    //Выбор платформы расчета, создание контекста
                    ComputeContextPropertyList properties = new ComputeContextPropertyList(ComputePlatform.Platforms[0]);
                    context = new ComputeContext(ComputeDeviceTypes.All, properties, null, IntPtr.Zero);
                    //Инициализация OpenCl, выбор устройства
                    commands = new ComputeCommandQueue(context, context.Devices[0], ComputeCommandQueueFlags.None);
                    //Считывание текста программы из файла
                    string s = AppDomain.CurrentDomain.BaseDirectory;
                    StreamReader streamReader = new StreamReader("F1.cl");
                    string clSource = streamReader.ReadToEnd();
                    streamReader.Close();
                    //Компиляция программы
                    program = new ComputeProgram(context, clSource);
                    program.Build(context.Devices, "", null, IntPtr.Zero);
                    //Создание ядра
                    kernelSurch = program.CreateKernel("Surch");
                    kernelIntGid = program.CreateKernel("InternalGrid");
                    //
                    stopW1.Restart();
                    // копирование массивов, использующихся в обоих kernel-ах
                    float[] fXS;
                    float[] fYS;
                    //построение 2х внутренних граней области и получение всех координат
                    // по линейной интерполяции без учента вертикального сгущения (только по горизонтали)
                    OCL_Surch(out fXS, out fYS);
                    //генерация внутренней сетки с учетом поправки на вертикальное сгущение
                    OCL_InternalGrid(out X, out Y, fXS, fYS);
                    //
                    stopW1.Stop();
                    stopAll.Stop();
                    //
                    timeAll = stopAll.Elapsed;
                    timeCalculate = stopCalc.Elapsed;
                    timeTransrort = stopW1.Elapsed;
                }
                else
                {
                    stopAll.Restart();
                    //
                    #region Surch
                    #region Последовательная версия
                    //for (int j = 0; j < Nx; j++)
                    //{
                    //    int jMap = Nx - 1;
                    //    double EM1, EM2, EM3, EM4;
                    //    double X2, Y2, X3, Y3;
                    //    double STJM, SJJM;
                    //    double XS2;
                    //    double YS2;
                    //    double XS3;
                    //    double YS3;
                    //    //
                    //    double DXS = XS[3 * Nx + j] - XS[0 * Nx + j];
                    //    double DYS = YS[3 * Nx + j] - YS[0 * Nx + j];
                    //    XS[1 * Nx + j] = XS[0 * Nx + j] + S2 * DXS;
                    //    YS[1 * Nx + j] = YS[0 * Nx + j] + S2 * DYS;
                    //    XS[2 * Nx + j] = XS[0 * Nx + j] + S3 * DXS;
                    //    YS[2 * Nx + j] = YS[0 * Nx + j] + S3 * DYS;
                    //    //
                    //    if ((j > 1) & (j < jMap))
                    //    {
                    //        if (Math.Abs(XS[0 * Nx + j + 1] - XS[0 * Nx + j - 1]) > 0.000001)
                    //            EM1 = (YS[0 * Nx + j + 1] - YS[0 * Nx + j - 1]) / (XS[0 * Nx + j + 1] - XS[0 * Nx + j - 1]);
                    //        else
                    //            EM1 = 1.0E+06f * (YS[0 * Nx + j + 1] - YS[0 * Nx + j - 1]);
                    //        if (Math.Abs(XS[1 * Nx + j] - XS[1 * Nx + j - 1]) > 0.000001)
                    //            EM2 = (YS[1 * Nx + j] - YS[1 * Nx + j - 1]) / (XS[1 * Nx + j] - XS[1 * Nx + j - 1]);
                    //        else
                    //            EM2 = 1000000 * (YS[1 * Nx + j] - YS[1 * Nx + j - 1]);
                    //        X2 = (EM1 * (YS[0 * Nx + j] - YS[1 * Nx + j] + EM2 * XS[1 * Nx + j]) + XS[0 * Nx + j]) / (1 + EM1 * EM2);
                    //        Y2 = YS[1 * Nx + j] + EM2 * (X2 - XS[1 * Nx + j]);
                    //        STJM = Math.Sqrt((X2 - XS[1 * Nx + j - 1]) * (X2 - XS[1 * Nx + j - 1]) + (Y2 - YS[1 * Nx + j - 1]) * (Y2 - YS[1 * Nx + j - 1]));
                    //        SJJM = Math.Sqrt((XS[1 * Nx + j] - XS[1 * Nx + j - 1]) * (XS[1 * Nx + j] - XS[1 * Nx + j - 1]) + (YS[1 * Nx + j] - YS[1 * Nx + j - 1]) * (YS[1 * Nx + j] - YS[1 * Nx + j - 1]));
                    //        if (STJM < SJJM)
                    //        {
                    //            XS2 = X2;
                    //            YS2 = Y2;
                    //        }
                    //        else
                    //        {
                    //            if (Math.Abs(XS[1 * Nx + j + 1] - XS[1 * Nx + j]) > 0.000001)
                    //                EM2 = (YS[1 * Nx + j + 1] - YS[1 * Nx + j]) / (XS[1 * Nx + j + 1] - XS[1 * Nx + j]);
                    //            else
                    //                EM2 = 1000000 * (YS[1 * Nx + j + 1] - YS[1 * Nx + j]);
                    //            X2 = (EM1 * (YS[0 * Nx + j] - YS[1 * Nx + j] + EM2 * XS[1 * Nx + j]) + XS[0 * Nx + j]) / (1 + EM1 * EM2);
                    //            Y2 = YS[1 * Nx + j] + EM2 * (X2 - XS[1 * Nx + j]);
                    //            XS2 = X2;
                    //            YS2 = Y2;
                    //        }

                    //        if (Math.Abs(XS[3 * Nx + j + 1] - XS[3 * Nx + j - 1]) > 0.000001)
                    //            EM4 = (YS[3 * Nx + j + 1] - YS[3 * Nx + j - 1]) / (XS[3 * Nx + j + 1] - XS[3 * Nx + j - 1]);
                    //        else
                    //            EM4 = 1000000 * (YS[3 * Nx + j + 1] - YS[3 * Nx + j - 1]);
                    //        if (Math.Abs(XS[2 * Nx + j] - XS[2 * Nx + j - 1]) > 0.000001)
                    //            EM3 = (YS[2 * Nx + j] - YS[2 * Nx + j - 1]) / (XS[2 * Nx + j] - XS[2 * Nx + j - 1]);
                    //        else
                    //            EM3 = 1000000 * (YS[2 * Nx + j] - YS[2 * Nx + j - 1]);
                    //        //
                    //        X3 = (EM4 * (YS[3 * Nx + j] - YS[2 * Nx + j] + EM3 * XS[2 * Nx + j]) + XS[3 * Nx + j]) / (1 + EM3 * EM4);
                    //        Y3 = YS[2 * Nx + j] + EM3 * (X3 - XS[2 * Nx + j]);
                    //        STJM = Math.Sqrt((X3 - XS[2 * Nx + j - 1]) * (X3 - XS[2 * Nx + j - 1]) + (Y3 - YS[2 * Nx + j - 1]) * (Y3 - YS[2 * Nx + j - 1]));
                    //        SJJM = Math.Sqrt((XS[2 * Nx + j] - XS[2 * Nx + j - 1]) * (XS[2 * Nx + j] - XS[2 * Nx + j - 1]) + (YS[2 * Nx + j] - YS[2 * Nx + j - 1]) * (YS[2 * Nx + j] - YS[2 * Nx + j - 1]));
                    //        //
                    //        if (STJM > SJJM)
                    //        {
                    //            if (Math.Abs(XS[2 * Nx + j + 1] - XS[2 * Nx + j]) > 0.000001)
                    //                EM3 = (YS[2 * Nx + j + 1] - YS[2 * Nx + j]) / (XS[2 * Nx + j + 1] - XS[2 * Nx + j]);
                    //            else
                    //                EM3 = 1000000 * (YS[2 * Nx + j + 1] - YS[2 * Nx + j]);
                    //            X3 = (EM4 * (YS[3 * Nx + j] - YS[2 * Nx + j] + EM3 * XS[2 * Nx + j]) + XS[3 * Nx + j]) / (1 + EM3 * EM4);
                    //            Y3 = YS[2 * Nx + j] + EM3 * (X3 - XS[2 * Nx + j]);
                    //        }
                    //        //
                    //        XS3 = X3;
                    //        YS3 = Y3;

                    //        XS[1 * Nx + j] = XS2;
                    //        YS[1 * Nx + j] = YS2;
                    //        XS[2 * Nx + j] = XS3;
                    //        YS[2 * Nx + j] = YS3;

                    //    }
                    //}
                    #endregion
                    // Параллельная версия
                    Parallel.ForEach(rangePartitioner,
                    (range, loopState) =>
                    {
                        for (int j = range.Item1; j < range.Item2; j++)
                        {
                            int jMap = Nx - 1;
                            double EM1, EM2, EM3, EM4;
                            double X2, Y2, X3, Y3;
                            double STJM, SJJM;
                            double XS2;
                            double YS2;
                            double XS3;
                            double YS3;
                            //
                            double DXS = XS[3 * Nx + j] - XS[0 * Nx + j];
                            double DYS = YS[3 * Nx + j] - YS[0 * Nx + j];
                            XS[1 * Nx + j] = XS[0 * Nx + j] + S2 * DXS;
                            YS[1 * Nx + j] = YS[0 * Nx + j] + S2 * DYS;
                            XS[2 * Nx + j] = XS[0 * Nx + j] + S3 * DXS;
                            YS[2 * Nx + j] = YS[0 * Nx + j] + S3 * DYS;
                            //
                            if ((j > 1) & (j < jMap))
                            {
                                if (Math.Abs(XS[0 * Nx + j + 1] - XS[0 * Nx + j - 1]) > 0.000001)
                                    EM1 = (YS[0 * Nx + j + 1] - YS[0 * Nx + j - 1]) / (XS[0 * Nx + j + 1] - XS[0 * Nx + j - 1]);
                                else
                                    EM1 = 1.0E+06f * (YS[0 * Nx + j + 1] - YS[0 * Nx + j - 1]);
                                if (Math.Abs(XS[1 * Nx + j] - XS[1 * Nx + j - 1]) > 0.000001)
                                    EM2 = (YS[1 * Nx + j] - YS[1 * Nx + j - 1]) / (XS[1 * Nx + j] - XS[1 * Nx + j - 1]);
                                else
                                    EM2 = 1000000 * (YS[1 * Nx + j] - YS[1 * Nx + j - 1]);
                                X2 = (EM1 * (YS[0 * Nx + j] - YS[1 * Nx + j] + EM2 * XS[1 * Nx + j]) + XS[0 * Nx + j]) / (1 + EM1 * EM2);
                                Y2 = YS[1 * Nx + j] + EM2 * (X2 - XS[1 * Nx + j]);
                                STJM = Math.Sqrt((X2 - XS[1 * Nx + j - 1]) * (X2 - XS[1 * Nx + j - 1]) + (Y2 - YS[1 * Nx + j - 1]) * (Y2 - YS[1 * Nx + j - 1]));
                                SJJM = Math.Sqrt((XS[1 * Nx + j] - XS[1 * Nx + j - 1]) * (XS[1 * Nx + j] - XS[1 * Nx + j - 1]) + (YS[1 * Nx + j] - YS[1 * Nx + j - 1]) * (YS[1 * Nx + j] - YS[1 * Nx + j - 1]));
                                if (STJM < SJJM)
                                {
                                    XS2 = X2;
                                    YS2 = Y2;
                                }
                                else
                                {
                                    if (Math.Abs(XS[1 * Nx + j + 1] - XS[1 * Nx + j]) > 0.000001)
                                        EM2 = (YS[1 * Nx + j + 1] - YS[1 * Nx + j]) / (XS[1 * Nx + j + 1] - XS[1 * Nx + j]);
                                    else
                                        EM2 = 1000000 * (YS[1 * Nx + j + 1] - YS[1 * Nx + j]);
                                    X2 = (EM1 * (YS[0 * Nx + j] - YS[1 * Nx + j] + EM2 * XS[1 * Nx + j]) + XS[0 * Nx + j]) / (1 + EM1 * EM2);
                                    Y2 = YS[1 * Nx + j] + EM2 * (X2 - XS[1 * Nx + j]);
                                    XS2 = X2;
                                    YS2 = Y2;
                                }

                                if (Math.Abs(XS[3 * Nx + j + 1] - XS[3 * Nx + j - 1]) > 0.000001)
                                    EM4 = (YS[3 * Nx + j + 1] - YS[3 * Nx + j - 1]) / (XS[3 * Nx + j + 1] - XS[3 * Nx + j - 1]);
                                else
                                    EM4 = 1000000 * (YS[3 * Nx + j + 1] - YS[3 * Nx + j - 1]);
                                if (Math.Abs(XS[2 * Nx + j] - XS[2 * Nx + j - 1]) > 0.000001)
                                    EM3 = (YS[2 * Nx + j] - YS[2 * Nx + j - 1]) / (XS[2 * Nx + j] - XS[2 * Nx + j - 1]);
                                else
                                    EM3 = 1000000 * (YS[2 * Nx + j] - YS[2 * Nx + j - 1]);
                                //
                                X3 = (EM4 * (YS[3 * Nx + j] - YS[2 * Nx + j] + EM3 * XS[2 * Nx + j]) + XS[3 * Nx + j]) / (1 + EM3 * EM4);
                                Y3 = YS[2 * Nx + j] + EM3 * (X3 - XS[2 * Nx + j]);
                                STJM = Math.Sqrt((X3 - XS[2 * Nx + j - 1]) * (X3 - XS[2 * Nx + j - 1]) + (Y3 - YS[2 * Nx + j - 1]) * (Y3 - YS[2 * Nx + j - 1]));
                                SJJM = Math.Sqrt((XS[2 * Nx + j] - XS[2 * Nx + j - 1]) * (XS[2 * Nx + j] - XS[2 * Nx + j - 1]) + (YS[2 * Nx + j] - YS[2 * Nx + j - 1]) * (YS[2 * Nx + j] - YS[2 * Nx + j - 1]));
                                //
                                if (STJM > SJJM)
                                {
                                    if (Math.Abs(XS[2 * Nx + j + 1] - XS[2 * Nx + j]) > 0.000001)
                                        EM3 = (YS[2 * Nx + j + 1] - YS[2 * Nx + j]) / (XS[2 * Nx + j + 1] - XS[2 * Nx + j]);
                                    else
                                        EM3 = 1000000 * (YS[2 * Nx + j + 1] - YS[2 * Nx + j]);
                                    X3 = (EM4 * (YS[3 * Nx + j] - YS[2 * Nx + j] + EM3 * XS[2 * Nx + j]) + XS[3 * Nx + j]) / (1 + EM3 * EM4);
                                    Y3 = YS[2 * Nx + j] + EM3 * (X3 - XS[2 * Nx + j]);
                                }
                                //
                                XS3 = X3;
                                YS3 = Y3;

                                XS[1 * Nx + j] = XS2;
                                YS[1 * Nx + j] = YS2;
                                XS[2 * Nx + j] = XS3;
                                YS[2 * Nx + j] = YS3;

                            }
                        }
                    });
                    #endregion
                    //
                    #region Internal grid
                    X = new double[Nx * Ny];
                    Y = new double[Nx * Ny];
                    //
                    #region Последоватеьная версия
                    //for (int j = 0; j < Nx; j++)
                    //    for (int k = 0; k < Ny; k++)
                    //    {
                    //        double A1 = 2.0f / (3.0f * AW - 1);
                    //        double A2 = 2.0f / (3.0f * (1 - AW) - 1);
                    //        double AJM = Nx - 1;
                    //        double DZI = 1.0f / AJM;
                    //        //
                    //        double AJ, ZI, S;
                    //        double[] sH = new double[4];
                    //        // 
                    //        AJ = j - 1;
                    //        ZI = AJ * DZI;
                    //        S = sAF[k] + ZI * (sCD[k] - sAF[k]);
                    //        //
                    //        sH[0] = (1 - S) * (1 - S) * (1 - A1 * S);
                    //        sH[1] = (1 - S) * (1 - S) * S * (A1 + 2);
                    //        sH[2] = (1 - S) * S * S * (A2 + 2);
                    //        sH[3] = S * S * (1 - A2 * (1 - S));
                    //        //поправка координат
                    //        for (int L = 0; L < 4; L++)
                    //        {
                    //            X[j + k * Nx] = X[j + k * Nx] + sH[L] * XS[L * Nx + j];
                    //            Y[j + k * Nx] = Y[j + k * Nx] + sH[L] * YS[L * Nx + j];
                    //        }
                    //    }
                    #endregion
                    // Параллельная версия
                    Parallel.ForEach(rangePartitioner,
                   (range, loopState) =>
                   {
                       for (int j = range.Item1; j < range.Item2; j++)
                       {
                           for (int k = 0; k < Ny; k++)
                           {
                               double A1 = 2.0f / (3.0f * AW - 1);
                               double A2 = 2.0f / (3.0f * (1 - AW) - 1);
                               double AJM = Nx - 1;
                               double DZI = 1.0f / AJM;
                               //
                               double AJ, ZI, S;
                               double[] sH = new double[4];
                               // 
                               AJ = j - 1;
                               ZI = AJ * DZI;
                               S = sAF[k] + ZI * (sCD[k] - sAF[k]);
                               //
                               sH[0] = (1 - S) * (1 - S) * (1 - A1 * S);
                               sH[1] = (1 - S) * (1 - S) * S * (A1 + 2);
                               sH[2] = (1 - S) * S * S * (A2 + 2);
                               sH[3] = S * S * (1 - A2 * (1 - S));
                               //поправка координат
                               for (int L = 0; L < 4; L++)
                               {
                                   X[j + k * Nx] = X[j + k * Nx] + sH[L] * XS[L * Nx + j];
                                   Y[j + k * Nx] = Y[j + k * Nx] + sH[L] * YS[L * Nx + j];
                               }
                           }
                       }
                   });
                    #endregion
                    //
                    stopAll.Stop();
                    //
                    timeAll = stopAll.Elapsed;
                    timeCalculate = timeAll;
                    //
                    // выравнивание накопившейся ошибки для корректной сшивки
                    for (int j = 0; j < Nx; j++)
                    {
                        X[j] = sArea.XTop[j];
                        Y[j] = sArea.YTop[j];
                        //
                        X[Nx * (Ny - 1) + j] = sArea.XBottom[j];
                        Y[Nx * (Ny - 1) + j] = sArea.YBottom[j];
                    }
                    //
                }
            }
            catch (Exception ex)
            {
                exception += ex.Message.ToString();
            }
        }
        void OCL_Surch(out float[] fXS, out float[] fYS)
        {
            fXS = new float[XS.Length];
            fYS = new float[XS.Length];
            try
            {

                //подготовка массивов для расчета в OpenCL
                float[] SS2 = { (float)S2, (float)S3 };
                int[] N = { Nx };
                //
                for (int i = 0; i < XS.Length; i++)
                {
                    fXS[i] = (float)XS[i];
                    fYS[i] = (float)YS[i];
                }
                //Выделение памяти на device(OpenCl) под переменные
                ComputeBuffer<int> NN = new ComputeBuffer<int>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, N);
                ComputeBuffer<float> ss = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, SS2);
                ComputeBuffer<float> xs = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, fXS);
                ComputeBuffer<float> ys = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, fYS);
                // установка буферов в kernel 
                kernelSurch.SetMemoryArgument(0, NN);
                kernelSurch.SetMemoryArgument(1, ss);
                kernelSurch.SetMemoryArgument(2, xs);
                kernelSurch.SetMemoryArgument(3, ys);
                //массив, определяющий размерность расчета (количество потоков в определенном измерении)
                long[] globalSize = new long[1];
                globalSize[0] = Nx;
                //
                stopW1.Stop();
                //
                stopCalc.Restart();
                //Вызов ядра
                commands.Execute(kernelSurch, null, globalSize, null, null);
                //Ожидание окончания выполнения программы
                commands.Finish();
                //
                stopCalc.Stop();
                stopW1.Start();
                // чтение искомой функции из буфера kernel-а
                commands.ReadFromBuffer(xs, ref fXS, true, null);
                commands.ReadFromBuffer(ys, ref fYS, true, null);
                //очищение памяти в порядке, обратном созданию
                ys.Dispose();
                xs.Dispose();
                ss.Dispose();
                NN.Dispose();
                //
                kernelSurch.Dispose();
            }
            catch (Exception ex)
            {
                exception += ex.Message.ToString();
            }

        }
        private void OCL_InternalGrid(out double[] Xd, out double[] Yd, float[] fXS, float[] fYS)
        {
            Xd = new double[Nx * Ny];
            Yd = new double[Nx * Ny];
            try
            {
                //подготовка массивов для OpenCL
                float[] Ar = { (float)AW };
                int[] N = { Nx };
                //конвертируем массивы в float
                float[] Xf = new float[Nx * Ny];
                float[] Yf = new float[Nx * Ny];
                float[] fsCD = new float[sCD.Length];
                float[] fsAF = new float[sAF.Length];
                //
                for (int i = 0; i < sCD.Length; i++)
                {
                    fsCD[i] = (float)sCD[i];
                    fsAF[i] = (float)sAF[i];
                }
                //Создание буферов параметров
                ComputeBuffer<float> XX = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, Xf);
                ComputeBuffer<float> YY = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, Yf);
                //
                ComputeBuffer<float> xs = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, fXS);
                ComputeBuffer<float> ys = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, fYS);
                //
                ComputeBuffer<float> SCD = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, fsCD);
                ComputeBuffer<float> SAF = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, fsAF);
                //
                ComputeBuffer<float> aw = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, Ar);
                ComputeBuffer<int> NN = new ComputeBuffer<int>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, N);
                //
                // установка буферов в kernel 
                kernelIntGid.SetMemoryArgument(0, XX);
                kernelIntGid.SetMemoryArgument(1, YY);
                kernelIntGid.SetMemoryArgument(2, xs);
                kernelIntGid.SetMemoryArgument(3, ys);
                kernelIntGid.SetMemoryArgument(4, SCD);
                kernelIntGid.SetMemoryArgument(5, SAF);
                kernelIntGid.SetMemoryArgument(6, aw);
                kernelIntGid.SetMemoryArgument(7, NN);
                //массив, определяющий размерность расчета (количество потоков в определенном измерении)
                long[] globalSize = new long[2];
                globalSize[0] = (long)Math.Round(Nx / 4.0, MidpointRounding.AwayFromZero);// так как width = 4 в F1.cl
                globalSize[1] = Ny;
                //
                stopW1.Stop();
                //
                stopCalc.Start();
                //Вызов ядра
                commands.Execute(kernelIntGid, null, globalSize, null, null);
                //Ожидание окончания выполнения программы
                commands.Finish();
                //
                stopCalc.Stop();
                //
                stopW1.Start();
                //
                // чтение искомой функции из буфера kernel-а
                commands.ReadFromBuffer(XX, ref Xf, true, null);
                commands.ReadFromBuffer(YY, ref Yf, true, null);
                //очищение памяти в порядке, обратном созданию
                NN.Dispose();
                aw.Dispose();
                SAF.Dispose();
                SCD.Dispose();
                ys.Dispose();
                xs.Dispose();
                YY.Dispose();
                XX.Dispose();
                //
                kernelIntGid.Dispose();
                program.Dispose();
                commands.Dispose();
                context.Dispose();
                // конвертация результаты в double
                for (int i = 0; i < X.Length; i++)
                {
                    Xd[i] = Convert.ToDouble(Xf[i]);
                    Yd[i] = Convert.ToDouble(Yf[i]);
                }
                // выравнивание накопившейся ошибки на правой грани расчетной области по x
                for (int i = 1; i < Ny + 1; i++)
                    Xd[i * Nx - 1] = XB[1];
                //

            }
            catch (Exception ex)
            {
                exception += ex.Message.ToString();
            }
        }

        //определение X и длины (криволиейные координаты) на криволинейном участке дна
        double Foil0(double[] Z, out double[] X, out double[] R)
        {
            R = new double[Z.Length];
            X = new double[Z.Length];
            X[0] = XB[0];
            //
            double dx = (float)((XB[1] - XB[0]) / (Nx - 1));
            //
            for (int i = 1; i < Nx; i++)
            {
                //пока равномерная сетка
                X[i] = XB[0] + dx * i;
                R[i] = R[i - 1] + Math.Sqrt(dx * dx + (Z[i] - Z[i - 1]) * (Z[i] - Z[i - 1]));
            }
            return R[Z.Length - 1];
        }
        // определение положения узлов на криволинейном участке дна с учетом сгущения
        void Foil1(double Val, ref double XD, ref double YD, double[] X, double[] R, double[] Z)
        {
            XD = 0;
            YD = 0;
            //определяем на каком интервале находится длина Val
            for (int i = l - 1; i < Nx; i++)
            {
                if (Val <= R[i])
                {
                    //линейная интерполяция координаты X по кривой
                    XD = X[i - 1] + (X[i] - XA[i - 1]) * (Val - R[i - 1]) / (R[i] - R[i - 1]);
                    if (XD < 1.0E-06)
                        XD = 1.0E-06f;
                    //линейная интерполяция координаты Y по кривой
                    YD = Z[i - 1] + (Z[i] - Z[i - 1]) * (Val - R[i - 1]) / (R[i] - R[i - 1]);
                    return;
                }
                l++;
            }
        }

    }
}
