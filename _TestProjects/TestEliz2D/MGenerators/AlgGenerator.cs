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
    public class AlgGenerator : PointsGeneratorEliz
    {
        OrderablePartitioner<Tuple<int, int>> rangePartitioner;
        public string exception = "";
        //
        int NX;
        int NY;
        bool flagFirstX = true;
        float[] fEta;
        //
        double[] X_old;
        public override string Name
        {
            get { return "Алгебраическая генерация"; }
        }
        //
        public AlgGenerator() { }
        public AlgGenerator(Parameter p)
        {
            ChangeParameters(p as AlgParameter);
        }
        public void ChangeParameters(AlgParameter p)
        {
            this.p = p;
            NX = p.Nx;
            NY = p.Ny;
            P = p.P;
            Q = p.Q;
            // заполнение растяжения по вертикали
            fEta = new float[NY];
            double[] Eta = Stretch(P, Q, NY);
            for (int i = 0; i < NY; i++)
                fEta[i] = (float)Eta[i];
            rangePartitioner = Partitioner.Create(0, NX);
        }
        protected override void GenerateCoords(SimpleAreaProfile sArea)
        {
            //
            TopXY = new double[2][];
            TopXY[0] = sArea.XTop;
            TopXY[1] = sArea.YTop;
            BottomXY = new double[2][];
            BottomXY[0] = sArea.XBottom;
            BottomXY[1] = sArea.YBottom;
            //
            if (flagFirstX)
            {
                //расчет координаты х - только 1 раз
                X = OCL_AlgGrid(0);
                X_old = new double[NX * NY];
                for (int i = 0; i < X.Length; i++)
                    X_old[i] = X[i];
                flagFirstX = false;
            }
            //расчет координаты y
            Y = OCL_AlgGrid(1);
            //
            for (int t = 0; t < BottomXY[1].Length; t++)
            {
                Y[(NY - 1) * NX + t] = BottomXY[1][t];
            }
            for (int i = 0; i < X.Length; i++)
                X[i] = X_old[i];
            //
            // расчет X и Y координат совместно
            //double[][] XY = OCL_AlgGrid();
            //X = XY[0];
            //Y = XY[1];
        }
        // /// <summary>
        /// Генерация координат X и Y по выбор  средствами OpenCL и  CUDA (x64) (если координаты по X не меняются, то быстрее генерировать только Y )
        /// </summary>
        /// <param name="Bottom">Значения координат на нидней границе</param>
        /// <param name="Top">Значения координат на верхней границе</param>
        /// <returns>Массив искомых координат</returns>
        double[] OCL_AlgGrid(int k)
        {
            if ((k == 1) || (k == 0))
            {
                if (Cuda)
                {
                    // -- замеряем общее время
                    stopAll.Restart();
                    //Init Cuda context               
                    //bool bPinGenericMemory = false; // we want this to be the default behavior
                    CUCtxFlags device_sync_method = CUCtxFlags.BlockingSync; // by default we use BlockingSync
                    dim3 threads, blocks; // kernel launch configuration
                    int nstreams = 4;  // number of streams for CUDA calls
                    int threadsPerBlock = 512;

                    CudaContext ctx;
                    ctx = new CudaContext(0, device_sync_method);

                    string resName;
                    if (IntPtr.Size == 8)
                        resName = "AlgGrid_x64.ptx";
                    else
                        resName = "AlgGrid_x64.ptx";
                    string resNamespace = "MeshLib";
                    string resource = resNamespace + "." + resName;
                    Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
                    if (stream == null) throw new ArgumentException("Kernel not found in resources.");
                    CudaKernel AlgGridKernel = ctx.LoadKernelPTX(stream, "AlgGrid");

                    //
                    //-- замеряем время на загрузку данных
                    stopW1.Restart();
                    //Init vars
                    CudaDeviceVariable<double> d_TopX, d_BotX, d_TopY, d_BotY, d_X, d_Y;
                    double[][] Xy = new double[2][];
                    double[] h_X = null;
                    double[] h_Y = null;
                    //CudaPageLockedHostMemory<double> hAligned_X = null;
                    //CudaPageLockedHostMemory<double> hAligned_Y = null;
                    //AllocateHostMemory(bPinGenericMemory, ref h_X, ref hAligned_X, NX * NY * sizeof(double));
                    //AllocateHostMemory(bPinGenericMemory, ref h_Y, ref hAligned_Y, NX * NY * sizeof(double));

                    int N_threads = NX * NY;

                    d_TopX = TopXY[0];
                    d_BotX = BottomXY[0];
                    d_TopY = TopXY[1];
                    d_BotY = BottomXY[1];
                    d_X = new CudaDeviceVariable<double>(NX * NY);
                    d_Y = new CudaDeviceVariable<double>(NX * NY);

                    //CudaStream[] streams = new CudaStream[nstreams];
                    //for (int i = 0; i < nstreams; i++)
                    //{
                    //    streams[i] = new CudaStream();
                    //}
                    threads = new dim3(threadsPerBlock, 1);
                    blocks = new dim3((N_threads + threadsPerBlock - 1) / (threadsPerBlock) + 0, 1);

                    // Invoke kernel                
                    AlgGridKernel.BlockDimensions = threads;
                    AlgGridKernel.GridDimensions = blocks;
                    //
                    stopW1.Stop(); // не считываем, так как еще к этому времени прибавим время на выгрузку данных из памяти
                                   //
                                   //-- замеры времени исполнения самого расчета
                    stopCalc.Restart();
                    //
                    AlgGridKernel.Run(d_TopX.DevicePointer, d_BotX.DevicePointer, d_TopY.DevicePointer, d_BotY.DevicePointer, Q, P, NX, NY, d_X.DevicePointer, d_Y.DevicePointer);
                    //
                    stopCalc.Stop();

                    //--
                    // asynchronously launch nstreams kernels, each operating on its own portion of data
                    //int size = N_threads / nstreams;
                    //for (int i = 0; i < nstreams; i++)
                    //    AlgGridKernel.RunAsync(streams[i].Stream, d_TopX.DevicePointer, d_BotX.DevicePointer, d_TopY.DevicePointer, d_BotY.DevicePointer, Q, P, NX, NY, d_X.DevicePointer + i * size * sizeof(double), d_Y.DevicePointer + i * size * sizeof(double), i, size);

                    //d_a.DevicePointer + i * n / nstreams * sizeof(int), d_c.DevicePointer, niterations);
                    //for (int i = 0; i < nstreams; i++)
                    //{
                    //   hAligned_X.AsyncCopyFromDevice(d_X, i * size * sizeof(double), i * size * sizeof(double), size * sizeof(double), streams[i].Stream);
                    //  hAligned_Y.AsyncCopyFromDevice(d_Y, i * size * sizeof(double), i * size * sizeof(double), size * sizeof(double), streams[i].Stream);
                    //}

                    //ctx.Synchronize(); //*/

                    //hAligned_X.SynchronCopyToHost(d_X);
                    //hAligned_Y.SynchronCopyToHost(d_Y);
                    //System.Runtime.InteropServices.Marshal.Copy(hAligned_X.PinnedHostPointer, h_X, 0, NX * NY);
                    //System.Runtime.InteropServices.Marshal.Copy(hAligned_Y.PinnedHostPointer, h_Y, 0, NX * NY);
                    //
                    //-- домеряем время на выгрузку данных и Dispose переменных
                    stopW1.Start();
                    h_X = d_X;
                    h_Y = d_Y;

                    Xy[0] = h_X;
                    Xy[1] = h_Y;

                    // release resources
                    //for (int i = 0; i < nstreams; i++)
                    //{
                    //    streams[i].Dispose();
                    //}

                    // Free device memory
                    if (d_TopX != null)
                        d_TopX.Dispose();

                    if (d_BotX != null)
                        d_BotX.Dispose();

                    if (d_TopY != null)
                        d_TopY.Dispose();

                    if (d_BotY != null)
                        d_BotY.Dispose();

                    if (d_X != null)
                        d_X.Dispose();

                    if (d_Y != null)
                        d_Y.Dispose();

                    if (ctx != null)
                        ctx.Dispose();
                    //
                    stopW1.Stop();
                    stopAll.Stop();
                    //
                    timeTransrort = stopW1.Elapsed;
                    timeAll = stopAll.Elapsed;
                    timeCalculate = stopCalc.Elapsed;
                    //
                    // Free host memory
                    // We have a GC for that :-)

                    // твой код вызова ядра на Cuda
                    // результат возвращается в качестве двумерного массива [2][Nx*Ny], где [0][] - Х-координаты, [1][] - Y координаты
                    //

                    return Xy[k];
                }
                else if (OpenCL)
                {
                    try
                    {
                        // -- замеряем общее время
                        stopAll.Restart();
                        //
                        stopW1.Restart();
                        // подготовка ГУ - перевод в формат float
                        float[] fBottom = new float[NX];
                        float[] fTop = new float[NX];
                        for (int i = 0; i < NX; i++)
                        {
                            fBottom[i] = (float)BottomXY[k][i];
                            fTop[i] = (float)TopXY[k][i];
                        }
                        //
                        float[] x = new float[NX * NY];
                        //
                        int[] NXYF = { NX, NY };
                        stopW1.Stop();
                        //
                        ComputeContextPropertyList properties = new ComputeContextPropertyList(ComputePlatform.Platforms[0]);
                        ComputeContext context = new ComputeContext(ComputeDeviceTypes.All, properties, null, IntPtr.Zero);
                        //Инициализация OpenCl 
                        ComputeCommandQueue commands = new ComputeCommandQueue(context, context.Devices[0], ComputeCommandQueueFlags.None);
                        //Компиляция программы
                        string s = AppDomain.CurrentDomain.BaseDirectory;
                        //
                        System.IO.StreamReader streamReader = new System.IO.StreamReader("..\\..\\Mesh\\MeshLib\\OpenCL\\AlgGrid.cl");
                        string clSource = streamReader.ReadToEnd();
                        streamReader.Close();
                        ComputeProgram program = new ComputeProgram(context, clSource);
                        //
                        program.Build(context.Devices, "", null, IntPtr.Zero);
                        //Создание ядра
                        ComputeKernel kernel = program.CreateKernel("AlgGrid");
                        //
                        stopW1.Start();
                        //
                        //Выделяем память на device(OpenCl) под переменные local int* NXYF, global float* Bottom, global float* Top, global write_only float* X
                        ComputeBuffer<int> N = new ComputeBuffer<int>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, NXYF);
                        //
                        ComputeBuffer<float> bottom = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, fBottom);
                        ComputeBuffer<float> top = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, fTop);
                        //
                        ComputeBuffer<float> XX = new ComputeBuffer<float>(context, ComputeMemoryFlags.WriteOnly | ComputeMemoryFlags.CopyHostPointer, x);
                        //
                        //Задаем их для нашего ядра  
                        kernel.SetMemoryArgument(0, N);
                        kernel.SetMemoryArgument(1, bottom);
                        kernel.SetMemoryArgument(2, top);
                        kernel.SetValueArgument<float>(3, (float)P);
                        kernel.SetValueArgument<float>(4, (float)Q);
                        kernel.SetMemoryArgument(5, XX);
                        ////двойной массив, где [0] - индекс массива
                        long[] globalSize = new long[2];
                        globalSize[0] = (long)Math.Round(NY / 4.0, MidpointRounding.AwayFromZero);// так как в AlgGrid.cl width = 4 NY;
                        globalSize[1] = NX;
                        //
                        stopW1.Stop();
                        stopCalc.Restart();
                        //
                        //Вызываем ядро количество потоков равно count
                        commands.Execute(kernel, null, globalSize, null, null);
                        commands.Finish();
                        //
                        stopCalc.Stop();
                        //
                        stopW1.Start();
                        //
                        //Читаем результат из переменной
                        commands.ReadFromBuffer(XX, ref x, true, null);
                        //очищение памяти в порядке, противоположном порядку их создания
                        XX.Dispose();
                        top.Dispose();
                        bottom.Dispose();
                        N.Dispose();
                        //
                        kernel.Dispose();
                        program.Dispose();
                        commands.Dispose();
                        context.Dispose();
                        //
                        double[] Xd = new double[x.Length];
                        for (int i = 0; i < x.Length; i++)
                            Xd[i] = Convert.ToDouble(x[i]);
                        //
                        x = null;
                        fTop = null;
                        fBottom = null;
                        //
                        for (int i = 0; i < NX; i++)
                        {
                            Xd[i] = (float)TopXY[k][i];
                            Xd[(NY - 1) * NX + i] = (float)BottomXY[k][i];
                        }
                        //
                        stopW1.Stop();
                        stopAll.Stop();
                        //
                        timeTransrort = stopW1.Elapsed;
                        timeAll = stopAll.Elapsed;
                        timeCalculate = stopCalc.Elapsed;
                        //
                        return Xd;

                    }
                    catch (Exception ex)
                    {
                        exception += ex.Message.ToString();
                        return null;
                    }
                }
                else
                {
                    double[] Xx = new double[NX * NY];

                    #region Последовательная версия
                    for (int j = 0; j < NX; j++)
                    {
                        double[] CX = new double[2];
                        double d_eta = 2.0 / (NY - 1);
                        //
                        CX[0] = BottomXY[k][j];
                        CX[1] = TopXY[k][j];
                        //
                        //double eta;
                        //double DETA = 1.0f / (NY - 1);
                        //double TQI = 1.0f / Math.Tanh(Q);
                        for (int i = 0; i < NY; i++)
                        {
                            //double AL = i;
                            //double ETA = AL * DETA;
                            //double DUM = Q * (1 - ETA);
                            //DUM = 1 - Math.Tanh(DUM) * TQI;
                            //eta = 1 - 2 * (P * ETA + (1 - P) * DUM);
                            //
                            //double[] N = new double[2];
                            //N[0] = 0.5 * (1 - eta);
                            //N[1] = 0.5 * (1 + eta);
                            //Xx[i * NX + j] = N[0] * CX[0] + N[1] * CX[1];
                            Xx[i * NX + j] = fEta[i] * CX[0] + (1 - fEta[i]) * CX[1];
                        }
                    }
                    #endregion
                    // Параллельная версия
                    Parallel.ForEach(rangePartitioner,
                     (range, loopState) =>
                     {
                         for (int j = range.Item1; j < range.Item2; j++)
                         {
                             double[] CX = new double[2];
                             double d_eta = 2.0 / (NY - 1);
                             //
                             CX[0] = BottomXY[k][j];
                             CX[1] = TopXY[k][j];
                             ////
                             //double eta;
                             //double DETA = 1.0f / (NY - 1);
                             //double TQI = 1.0f / Math.Tanh(Q);
                             for (int i = 0; i < NY; i++)
                             {
                                 //double AL = i;
                                 //double ETA = AL * DETA;
                                 //double DUM = Q * (1 - ETA);
                                 //DUM = 1 - Math.Tanh(DUM) * TQI;
                                 //eta = 1 - 2 * (P * ETA + (1 - P) * DUM);

                                 ////
                                 //double[] N = new double[2];
                                 //N[0] = 0.5 * (1 - eta);
                                 //N[1] = 0.5 * (1 + eta);
                                 //Xx[i * NX + j] = N[0] * CX[0] + N[1] * CX[1];
                                 Xx[i * NX + j] = fEta[i] * CX[0] + (1 - fEta[i]) * CX[1];
                             }
                         }
                     });
                    //
                    return Xx;
                }


            }
            else
            {
                exception += "k долюно быть равно 0 или 1-AlgGenerator";
                return null;
            }
        }
        //
        /// <summary>
        /// Генерация координат X и Y единовременно  средствами OpenCL и CUDA (x64)
        /// </summary>
        /// <param name="Bottom">Значения координат на нидней границе</param>
        /// <param name="Top">Значения координат на верхней границе</param>
        /// <returns>Массив искомых координат</returns>
        double[][] OCL_AlgGrid()
        {

            //
            if (Cuda)
            {
                // -- замеряем общее время
                stopAll.Restart();
                //Init Cuda context               
                bool bPinGenericMemory = false; // we want this to be the default behavior
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
                    resName = "AlgGrid_x64.ptx";
                else
                    resName = "AlgGrid_x64.ptx";
                string resNamespace = "MeshLib";
                string resource = resNamespace + "." + resName;
                Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
                if (stream == null) throw new ArgumentException("Kernel not found in resources.");
                CudaKernel AlgGridKernel = ctx.LoadKernelPTX(stream, "AlgGrid");

                //
                //-- замеряем время на загрузку данных
                stopW1.Restart();
                //Init vars
                CudaDeviceVariable<double> d_TopX, d_BotX, d_TopY, d_BotY, d_X, d_Y;
                double[][] Xy = new double[2][];
                double[] h_X = null;
                double[] h_Y = null;
                //CudaPageLockedHostMemory<double> hAligned_X = null;
                //CudaPageLockedHostMemory<double> hAligned_Y = null;
                //AllocateHostMemory(bPinGenericMemory, ref h_X, ref hAligned_X, NX * NY * sizeof(double));
                //AllocateHostMemory(bPinGenericMemory, ref h_Y, ref hAligned_Y, NX * NY * sizeof(double));

                int N_threads = NX * NY;

                d_TopX = TopXY[0];
                d_BotX = BottomXY[0];
                d_TopY = TopXY[1];
                d_BotY = BottomXY[1];
                d_X = new CudaDeviceVariable<double>(NX * NY);
                d_Y = new CudaDeviceVariable<double>(NX * NY);

                //CudaStream[] streams = new CudaStream[nstreams];
                //for (int i = 0; i < nstreams; i++)
                //{
                //    streams[i] = new CudaStream();
                //}
                threads = new dim3(threadsPerBlock, 1);
                blocks = new dim3((N_threads + threadsPerBlock - 1) / (threadsPerBlock) + 0, 1);

                // Invoke kernel                
                AlgGridKernel.BlockDimensions = threads;
                AlgGridKernel.GridDimensions = blocks;
                //
                stopW1.Stop(); // не считываем, так как еще к этому времени прибавим время на выгрузку данных из памяти
                //
                //-- замеры времени исполнения самого расчета
                stopCalc.Restart();
                //
                AlgGridKernel.Run(d_TopX.DevicePointer, d_BotX.DevicePointer, d_TopY.DevicePointer, d_BotY.DevicePointer, Q, P, NX, NY, d_X.DevicePointer, d_Y.DevicePointer);
                //
                stopCalc.Stop();

                //--
                // asynchronously launch nstreams kernels, each operating on its own portion of data
                //int size = N_threads / nstreams;
                //for (int i = 0; i < nstreams; i++)
                //    AlgGridKernel.RunAsync(streams[i].Stream, d_TopX.DevicePointer, d_BotX.DevicePointer, d_TopY.DevicePointer, d_BotY.DevicePointer, Q, P, NX, NY, d_X.DevicePointer + i * size * sizeof(double), d_Y.DevicePointer + i * size * sizeof(double), i, size);

                //d_a.DevicePointer + i * n / nstreams * sizeof(int), d_c.DevicePointer, niterations);
                //for (int i = 0; i < nstreams; i++)
                //{
                //   hAligned_X.AsyncCopyFromDevice(d_X, i * size * sizeof(double), i * size * sizeof(double), size * sizeof(double), streams[i].Stream);
                //  hAligned_Y.AsyncCopyFromDevice(d_Y, i * size * sizeof(double), i * size * sizeof(double), size * sizeof(double), streams[i].Stream);
                //}

                //ctx.Synchronize(); //*/

                //hAligned_X.SynchronCopyToHost(d_X);
                //hAligned_Y.SynchronCopyToHost(d_Y);
                //System.Runtime.InteropServices.Marshal.Copy(hAligned_X.PinnedHostPointer, h_X, 0, NX * NY);
                //System.Runtime.InteropServices.Marshal.Copy(hAligned_Y.PinnedHostPointer, h_Y, 0, NX * NY);
                //
                //-- домеряем время на выгрузку данных и Dispose переменных
                stopW1.Start();
                h_X = d_X;
                h_Y = d_Y;

                Xy[0] = h_X;
                Xy[1] = h_Y;

                // release resources
                //for (int i = 0; i < nstreams; i++)
                //{
                //    streams[i].Dispose();
                //}

                // Free device memory
                if (d_TopX != null)
                    d_TopX.Dispose();

                if (d_BotX != null)
                    d_BotX.Dispose();

                if (d_TopY != null)
                    d_TopY.Dispose();

                if (d_BotY != null)
                    d_BotY.Dispose();

                if (d_X != null)
                    d_X.Dispose();

                if (d_Y != null)
                    d_Y.Dispose();

                if (ctx != null)
                    ctx.Dispose();
                //
                stopW1.Stop();
                stopAll.Stop();
                //
                timeTransrort = stopW1.Elapsed;
                timeAll = stopAll.Elapsed;
                timeCalculate = stopCalc.Elapsed;
                //
                // Free host memory
                // We have a GC for that :-)

                // твой код вызова ядра на Cuda
                // результат возвращается в качестве двумерного массива [2][Nx*Ny], где [0][] - Х-координаты, [1][] - Y координаты
                //

                return Xy;
            }
            else if (OpenCL)
            {
                try
                {
                    // -- замеряем общее время
                    stopAll.Restart();
                    //
                    stopW1.Restart();
                    // подготовка ГУ - перевод в формат float
                    float[] fBottomX = new float[NX];
                    float[] fTopX = new float[NX];
                    float[] fBottomY = new float[NX];
                    float[] fTopY = new float[NX];
                    for (int i = 0; i < NX; i++)
                    {
                        fBottomX[i] = (float)BottomXY[0][i];
                        fTopX[i] = (float)TopXY[0][i];
                        fBottomY[i] = (float)BottomXY[1][i];
                        fTopY[i] = (float)TopXY[1][i];
                    }
                    //
                    float[] x = new float[NX * NY];
                    float[] y = new float[NX * NY];
                    //
                    int[] NXYF = { NX, NY };
                    stopW1.Stop();
                    //
                    ComputeContextPropertyList properties = new ComputeContextPropertyList(ComputePlatform.Platforms[0]);
                    ComputeContext context = new ComputeContext(ComputeDeviceTypes.All, properties, null, IntPtr.Zero);
                    //Инициализация OpenCl 
                    ComputeCommandQueue commands = new ComputeCommandQueue(context, context.Devices[0], ComputeCommandQueueFlags.None);
                    //Компиляция программы
                    string s = AppDomain.CurrentDomain.BaseDirectory;
                    //
                    System.IO.StreamReader streamReader = new System.IO.StreamReader("AlgGrid.cl");
                    string clSource = streamReader.ReadToEnd();
                    streamReader.Close();
                    ComputeProgram program = new ComputeProgram(context, clSource);
                    //
                    program.Build(context.Devices, "", null, IntPtr.Zero);
                    //Создание ядра
                    ComputeKernel kernel = program.CreateKernel("AlgGrid");
                    //
                    //-- замеряем время на загрузку данных
                    stopW1.Start();
                    //
                    //Выделяем память на device(OpenCl) под переменные local int* NXYF, global float* Bottom, global float* Top, global write_only float* X
                    ComputeBuffer<int> N = new ComputeBuffer<int>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, NXYF);
                    //
                    ComputeBuffer<float> bottomX = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, fBottomX);
                    ComputeBuffer<float> topX = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, fTopX);
                    ComputeBuffer<float> bottomY = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, fBottomY);
                    ComputeBuffer<float> topY = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, fTopY);
                    //
                    ComputeBuffer<float> XX = new ComputeBuffer<float>(context, ComputeMemoryFlags.WriteOnly | ComputeMemoryFlags.CopyHostPointer, x);
                    ComputeBuffer<float> YY = new ComputeBuffer<float>(context, ComputeMemoryFlags.WriteOnly | ComputeMemoryFlags.CopyHostPointer, y);
                    //
                    //Задаем их для нашего ядра  
                    kernel.SetMemoryArgument(0, N);
                    kernel.SetMemoryArgument(1, bottomX);
                    kernel.SetMemoryArgument(2, topX);
                    kernel.SetMemoryArgument(3, bottomY);
                    kernel.SetMemoryArgument(4, topY);
                    kernel.SetValueArgument<float>(5, (float)P);
                    kernel.SetValueArgument<float>(6, (float)Q);
                    kernel.SetMemoryArgument(7, XX);
                    kernel.SetMemoryArgument(8, YY);
                    ////двойной массив, где [0] - индекс массива
                    long[] globalSize = new long[2];
                    globalSize[0] = (long)Math.Round(NY / 4.0, MidpointRounding.AwayFromZero);// так как в AlgGrid.cl width = 4 NY;
                    globalSize[1] = NX;
                    //
                    stopW1.Stop();
                    stopCalc.Restart();
                    //
                    //Вызываем ядро количество потоков равно count
                    commands.Execute(kernel, null, globalSize, null, null);
                    commands.Finish();
                    //
                    stopCalc.Stop();
                    //
                    stopW1.Start();
                    //
                    //Читаем результат из переменной
                    commands.ReadFromBuffer(XX, ref x, true, null);
                    commands.ReadFromBuffer(YY, ref y, true, null);
                    //очищение памяти в порядке, противоположном порядку их создания
                    YY.Dispose();
                    XX.Dispose();
                    topY.Dispose();
                    bottomY.Dispose();
                    topX.Dispose();
                    bottomX.Dispose();
                    N.Dispose();
                    //
                    kernel.Dispose();
                    program.Dispose();
                    commands.Dispose();
                    context.Dispose();
                    //
                    double[][] XY = new double[2][];
                    XY[0] = new double[x.Length];
                    XY[1] = new double[x.Length];
                    for (int i = 0; i < x.Length; i++)
                    {
                        XY[0][i] = Convert.ToDouble(x[i]);
                        XY[1][i] = Convert.ToDouble(y[i]);
                    }
                    //
                    y = null;
                    x = null;
                    fTopX = null;
                    fBottomX = null;
                    fTopY = null;
                    fBottomY = null;
                    //
                    for (int k = 0; k < 2; k++)
                    {
                        for (int i = 0; i < NX; i++)
                        {
                            XY[k][i] = (float)TopXY[k][i];
                            XY[k][(NY - 1) * NX + i] = (float)BottomXY[k][i];
                        }
                    }
                    //
                    stopW1.Stop();
                    stopAll.Stop();
                    //
                    timeTransrort = stopW1.Elapsed;
                    timeAll = stopAll.Elapsed;
                    timeCalculate = stopCalc.Elapsed;
                    //
                    return XY;

                }
                catch (Exception ex)
                {
                    exception += ex.Message.ToString();
                    return null;
                }
            }
            else
            {
                // -- замеряем общее время
                stopAll.Restart();
                //
                double[][] Xy = new double[2][];
                Xy[0] = new double[NX * NY];
                Xy[1] = new double[NX * NY];

                #region Последовательная версия
                //for (int j = 0; j < NX; j++)
                //{
                //    double d_eta = 2.0 / (NY - 1);
                //    //
                //    double[] CX = new double[2];
                //    double[] CY = new double[2];
                //    //
                //    CX[0] = BottomXY[0][j];
                //    CX[1] = TopXY[0][j];
                //    CY[0] = BottomXY[1][j];
                //    CY[1] = TopXY[1][j];

                //    double eta;
                //    double DETA = 1.0f / (NY - 1);
                //    double TQI = 1.0f / Math.Tanh(Q);
                //    for (int i = 0; i < NY; i++)
                //    {
                //        double AL = i;
                //        double ETA = AL * DETA;
                //        double DUM = Q * (1 - ETA);
                //        DUM = 1 - Math.Tanh(DUM) * TQI;
                //        eta = 1 - 2 * (P * ETA + (1 - P) * DUM);
                //        //
                //        double[] N = new double[2];
                //        N[0] = 0.5 * (1 - eta);
                //        N[1] = 0.5 * (1 + eta);
                //        Xy[0][i * NX + j] = N[0] * CX[0] + N[1] * CX[1];
                //        Xy[1][i * NX + j] = N[0] * CY[0] + N[1] * CY[1];
                //    }
                //}
                #endregion
                // Параллельная версия
                double DETA = 1.0f / (NY - 1);
                double TQI = 1.0f / Math.Tanh(Q);
                //
                Parallel.ForEach(rangePartitioner,
                    (range, loopState) =>
                    {
                        for (int j = range.Item1; j < range.Item2; j++)
                        {
                            //
                            double CX0 = BottomXY[0][j];
                            double CX1 = TopXY[0][j];
                            double CY0 = BottomXY[1][j];
                            double CY1 = TopXY[1][j];
                            //
                            //double s;
                            //double N0 = 0, N1 = 0, ETA = 0, DUM = 0;
                            for (int i = 0; i < NY; i++)
                            {
                                //ETA = i * DETA;
                                //DUM = Q * (1 - ETA);
                                //DUM = 1 - Math.Tanh(DUM) * TQI;
                                //s = P * ETA + (1 - P) * DUM;
                                ////
                                //N0 = s;
                                //N1 = 1 - s;
                                //Xy[0][i * NX + j] = N0 * CX0 + N1 * CX1;
                                //Xy[1][i * NX + j] = N0 * CY0 + N1 * CY1;
                                Xy[0][i * NX + j] = fEta[i] * CX0 + (1 - fEta[i]) * CX1;
                                Xy[1][i * NX + j] = fEta[i] * CY0 + (1 - fEta[i]) * CY1;
                            }
                        }
                    });
                stopAll.Stop();
                timeAll = stopAll.Elapsed;
                timeCalculate = timeAll;
                return Xy;
            }

        }
    }
}
