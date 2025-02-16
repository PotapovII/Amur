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
    //// // ////////////////////////////////не переименовывать классы Gererator и Parameter!!!! от имени зависит положение в листбоксе контрола
    public class DiffGenerator : PointsGeneratorEliz
    {
        string exception = "";
        int NX;
        int NY;
        bool flagFirstX = true;
        double[][] LeftXY, RightXY;
        OrderablePartitioner<Tuple<int, int>> rangePartitioner;
        public DiffParameter SetParameter
        {
            set
            {
                p = value;
                NX = value.Nx;
                NY = value.Ny;
            }
        }

        //
        public override string Name
        {
            get { return "Дифференциальная генерация"; }
        }
        //
        public DiffGenerator() { }
        public DiffGenerator(Parameter p)
        {
            ChangeParameters(p as DiffParameter);
        }
        public void ChangeParameters(DiffParameter p)
        {
            this.p = p;
            NX = p.Nx;
            NY = p.Ny;
            P = p.P;
            Q = p.Q;
            //
            rangePartitioner = Partitioner.Create(0, NX * NY);

        }


        

        /// <summary>
        /// метод генерации сетки
        /// </summary>
        /// <returns></returns>
        protected override void GenerateCoords(SimpleAreaProfile sArea)
        {
            // меняем NX на NX+1 для частей > 1
            NX = sArea.XBottom.Length;
            //
            TopXY = new double[2][];
            LeftXY = new double[2][];
            BottomXY = new double[2][];
            RightXY = new double[2][];
            TopXY[0] = sArea.XTop;
            TopXY[1] = sArea.YTop;
            BottomXY[0] = sArea.XBottom;
            BottomXY[1] = sArea.YBottom;
            LeftXY[0] = new double[NY];
            LeftXY[1] = new double[NY];
            RightXY[0] = new double[NY];
            RightXY[1] = new double[NY];
            //
            double[] LineStretch = Stretch(P, Q, NY);
            //
            double H = TopXY[1][0] - BottomXY[1][0];
            double dy = H / (NY - 1);
            double BottomLeft = BottomXY[1][NX - 1];
            double BottomRight = BottomXY[1][0];
            double ls = 0;
            for (int i = 0; i < NY; i++)
            {
                ls = LineStretch[NY - 1 - i];
                LeftXY[0][i] = BottomXY[0][0];
                LeftXY[1][i] = BottomLeft + ls * H;//TopLeft - i * dy;
                RightXY[0][i] = BottomXY[0][NX - 1];
                RightXY[1][i] = BottomRight + ls * H;//TopRight - i * dy;
            }
            if (Cuda)
            {
                stopAll.Restart();

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
                    resName = "DiffGrid_x64.ptx";
                else
                    resName = "DiffGrid_x64.ptx";
                string resNamespace = "MeshLib";
                string resource = resNamespace + "." + resName;
                Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
                if (stream == null) throw new ArgumentException("Kernel not found in resources.");
                CudaKernel DiffGridKernel = ctx.LoadKernelPTX(stream, "DiffGrid");
                //
                stopW1.Restart();
                //Init vars
                CudaStream[] streams = new CudaStream[nstreams];
                for (int i = 0; i < nstreams; i++)
                {
                    streams[i] = new CudaStream();
                }
                CudaDeviceVariable<float> d_X = new CudaDeviceVariable<float>(NX * NY);
                CudaDeviceVariable<float> d_Y = new CudaDeviceVariable<float>(NX * NY);
                CudaPageLockedHostMemory<float> hAligned_X = null;
                CudaPageLockedHostMemory<float> hAligned_Y = null;
                float[] h_X = null;
                float[] h_Y = null;
                AllocateHostMemory(bPinGenericMemory, ref h_X, ref hAligned_X, NX * NY * sizeof(float));
                AllocateHostMemory(bPinGenericMemory, ref h_Y, ref hAligned_Y, NX * NY * sizeof(float));
                int[] N = { NX, NY };
                CudaDeviceVariable<int> d_N = N;

                // установка граничных условий 
                //слева - справа   
                for (int i = 0; i < NY; i++)
                {
                    h_X[i * NX] = (float)LeftXY[0][i];
                    h_X[(NX - 1) + NX * i] = (float)RightXY[0][i];
                    h_Y[i * NX] = (float)LeftXY[1][i];
                    h_Y[(NX - 1) + NX * i] = (float)RightXY[1][i];
                }
                //сверху-снизу
                for (int i = 0; i < NX; i++)
                {
                    h_X[i] = (float)TopXY[0][i];
                    h_X[NX * (NY - 1) + i] = (float)BottomXY[0][i];
                    h_Y[i] = (float)TopXY[1][i];
                    h_Y[NX * (NY - 1) + i] = (float)BottomXY[1][i];
                }
                System.Runtime.InteropServices.Marshal.Copy(h_X, 0, hAligned_X.PinnedHostPointer, NX * NY);
                System.Runtime.InteropServices.Marshal.Copy(h_Y, 0, hAligned_Y.PinnedHostPointer, NX * NY);
                int N_threads = NX * NY;
                int size = N_threads / nstreams;
                for (int i = 0; i < nstreams; i++)
                {
                    hAligned_X.AsyncCopyToDevice(d_X, i * size * sizeof(float), i * size * sizeof(float), size * sizeof(float), streams[i].Stream);
                    hAligned_Y.AsyncCopyToDevice(d_Y, i * size * sizeof(float), i * size * sizeof(float), size * sizeof(float), streams[i].Stream);
                }

                ctx.Synchronize();//*/

                //hAligned_X.SynchronCopyToDevice(d_X);
                //hAligned_Y.SynchronCopyToDevice(d_Y);
                //d_X = h_X;
                //d_Y = h_Y;

                threads = new dim3(threadsPerBlock, 1);
                blocks = new dim3((N_threads + threadsPerBlock - 1) / (threadsPerBlock), 1);
                // Invoke kernel                
                DiffGridKernel.BlockDimensions = threads;
                DiffGridKernel.GridDimensions = blocks;
                //
                stopW1.Stop();
                //
                stopCalc.Restart();
                //
                for (int k = 0; k < 2 * NX * NY; k++)
                {
                    //for (int i = 0; i < nstreams; i++)
                    //DiffGridKernel.RunAsync(streams[i].Stream, d_X.DevicePointer + i * size * sizeof(float), d_Y.DevicePointer + i * size * sizeof(float), NX, NY, i, size);
                    DiffGridKernel.Run(d_X.DevicePointer, d_Y.DevicePointer, d_N.DevicePointer);
                    //ctx.Synchronize();
                }
                stopCalc.Stop();
                //
                stopW1.Start();

                for (int i = 0; i < nstreams; i++)
                {
                    hAligned_X.AsyncCopyFromDevice(d_X, i * size * sizeof(float), i * size * sizeof(float), size * sizeof(float), streams[i].Stream);
                    hAligned_Y.AsyncCopyFromDevice(d_Y, i * size * sizeof(float), i * size * sizeof(float), size * sizeof(float), streams[i].Stream);
                }
                ctx.Synchronize();//*/
                //hAligned_X.SynchronCopyToHost(d_X);
                //hAligned_Y.SynchronCopyToHost(d_Y);
                //h_X = d_X;
                //h_Y = d_Y;
                System.Runtime.InteropServices.Marshal.Copy(hAligned_X.PinnedHostPointer, h_X, 0, NX * NY);
                System.Runtime.InteropServices.Marshal.Copy(hAligned_Y.PinnedHostPointer, h_Y, 0, NX * NY);

                X = new double[NX * NY];
                Y = new double[NX * NY];

                for (int i = 0; i < NX * NY; i++)
                {
                    X[i] = Convert.ToDouble(h_X[i]);
                    Y[i] = Convert.ToDouble(h_Y[i]);
                }

                // release resources
                for (int i = 0; i < nstreams; i++)
                {
                    streams[i].Dispose();
                }

                hAligned_X.Dispose();
                hAligned_Y.Dispose();
                d_X.Dispose();
                d_Y.Dispose();
                ctx.Dispose();
                //
                stopW1.Stop();
                stopAll.Stop();
                //
                timeAll = stopAll.Elapsed;
                timeCalculate = stopCalc.Elapsed;
                timeTransrort = stopW1.Elapsed;
                // твой код вызова ядра на Cuda
                // результат должен быть записан в массивы X и Y в формате double
                // тут структура метода не такая, как в AlgGenerator, так как в этом методе можно досчитывать сетку при малых изменениях границы, а не считать с нуля каждый раз
            }
            else if (OpenCL)
            {
                ////вызов ядра для кадой координаты отдельно
                ////при первичном расчете координат задается достаточно много иетарций
                //if (flagFirstX)
                //{
                //    X = OCL_DiffGeneration(0, NX * NY * 2);
                //    Y = OCL_DiffGeneration(1, NX * NY * 2);
                //}
                //// если расчет не первичный, в памяти остаются координаты с предыдущего расчета,
                //// поэтому производится не расчет с нуля, а досчет (итераций меньше)
                ////!!!! только при part=1!!!!
                //// два массива, так как при деформации дна массив X можно не пересчитывать
                //else
                //{
                //    X = OCL_DiffGeneration(0, NX * NY * 2);
                //    Y = OCL_DiffGeneration(1, NX * NY * 2);
                //}
                //// вызов ядра для обеих координат вместе (медленнее)
                double[][] XY = OCL_DiffGeneration(NX * NY * 2);
                X = XY[0];
                Y = XY[1];
            }
            else
            {
                stopAll.Restart();
                //
                stopW1.Restart();
                //
                X = new double[NX * NY];
                Y = new double[NX * NY];
                //слева-справа
                for (int i = 0; i < NY; i++)
                {
                    X[i * NX] = LeftXY[0][i];
                    Y[i * NX] = LeftXY[1][i];
                    X[(NX - 1) + NX * i] = RightXY[0][i];
                    Y[(NX - 1) + NX * i] = RightXY[1][i];
                }
                //сверху-снизу
                for (int i = 0; i < NX; i++)
                {
                    X[i] = TopXY[0][i];
                    Y[i] = TopXY[1][i];
                    X[NX * (NY - 1) + i] = BottomXY[0][i];
                    Y[NX * (NY - 1) + i] = BottomXY[1][i];
                }
                //
                stopW1.Stop();
                stopCalc.Restart();
                //
                #region Последовательная версия
                //for (int k = 0; k < 2 * NX * NY; k++)
                //    for (int i = 0; i < NX * NY; i++)
                //    {
                //        if ((i > NX) & (i < NX * NY - NX))
                //            if ((i % NX != 0) & ((i + 1) % NX != 0))
                //            {
                //                X[i] = 0.25 * (X[i + 1] + X[i - 1] + X[i - NX] + X[i + NX]);
                //                Y[i] = 0.25 * (Y[i + 1] + Y[i - 1] + Y[i - NX] + Y[i + NX]);
                //            }
                //    }
                #endregion
                //
                #region запись в файл сетки (для тестов на кластере)
                //
                //StreamWriter sw = new StreamWriter(NX.ToString() + "x" + NY.ToString() + ".txt");
                //sw.WriteLine("Nx " + NX.ToString());
                //sw.WriteLine("Ny " + NY.ToString());
                ////
                //sw.Write("X ");
                //for (int i = 0; i < NX * NY; i++)
                //{
                //    sw.Write(X[i].ToString() + "; ");
                //}
                //sw.Write("\n");
                //sw.Write("Y ");
                //for (int i = 0; i < NX * NY; i++)
                //{
                //    sw.Write(Y[i].ToString() + "; ");
                //}
                ////
                //sw.Close();
                #endregion
                //
                // Параллельная версия средствами C#
                // данный цикл не параллелю, так как он должен полностью выполняться до сходимости
                for (int k = 0; k < 2 * NX * NY; k++)
                {
                    Parallel.ForEach(rangePartitioner,
                     (range, loopState) =>
                     {
                         for (int i = range.Item1; i < range.Item2; i++)
                         {
                             if ((i > NX) & (i < NX * NY - NX))
                                 if ((i % NX != 0) & ((i + 1) % NX != 0))
                                 {
                                     X[i] = 0.25 * (X[i + 1] + X[i - 1] + X[i - NX] + X[i + NX]);
                                     Y[i] = 0.25 * (Y[i + 1] + Y[i - 1] + Y[i - NX] + Y[i + NX]);
                                 }
                         }
                     });
                }
                stopCalc.Stop();
                stopAll.Stop();
                //
                timeAll = stopAll.Elapsed;
                timeCalculate = stopCalc.Elapsed;
                timeTransrort = stopW1.Elapsed;

            }
        }
        /// <summary>
        /// расчет координат с помощью OpenCL (включая досчет?)
        /// </summary>
        /// <param name="iteration">количество итераций расчета</param>
        /// <returns></returns>
        double[][] OCL_DiffGeneration(int iteration)
        {
            try
            {
                stopAll.Restart();
                //
                stopW1.Restart();
                //
                //!!! для part!=1
                float[] x = new float[NX * NY];
                float[] y = new float[NX * NY];
                // установка граничных условий 
                //слева - справа   
                for (int i = 0; i < NY; i++)
                {
                    x[i * NX] = (float)LeftXY[0][i];
                    x[(NX - 1) + NX * i] = (float)RightXY[0][i];
                    y[i * NX] = (float)LeftXY[1][i];
                    y[(NX - 1) + NX * i] = (float)RightXY[1][i];
                }
                //сверху-снизу
                for (int i = 0; i < NX; i++)
                {
                    x[i] = (float)TopXY[0][i];
                    x[NX * (NY - 1) + i] = (float)BottomXY[0][i];
                    y[i] = (float)TopXY[1][i];
                    y[NX * (NY - 1) + i] = (float)BottomXY[1][i];
                }
                stopW1.Stop();
                //
                #region Тест кода ядра
                //int width = 4; double ls1=0, ls2=0;
                //for (int iter = 0; iter < iteration; iter++)
                //{
                //    for (int i = 0; i < NX * NY / 4; i++)
                //    {
                //        int id = width * i;
                //        int cid;
                //        ///
                //        for (int m = 0; m < width; m++)
                //        {
                //            cid = id + m;
                //            if ((cid > NX) & (cid < NX * NY - NX))// - проверка на первую и последнюю строчку
                //                //cid = min(max((id + m), NX - 1), NX * NY - NX - 1);
                //                //
                //                if ((cid % NX != 0) & ((cid + 1) % NX != 0))
                //                {
                //                    ls1 = LineStretch[NY-1-cid/NX+1]; ls2 = LineStretch[NY-1-cid/NX-1];
                //                    xy[0][cid] = (float)(0.25 * (xy[0][cid + 1] + xy[0][cid - 1]) +  0.25*ls1 * xy[0][cid - NX] + 0.25*ls2 * xy[0][cid + NX]);
                //                    xy[1][cid] = (float)(0.25 * (xy[1][cid + 1] + xy[1][cid - 1]) + 0.25*ls1 * xy[1][cid - NX] + 0.25*ls2 * xy[1][cid + NX]);
                //                }
                //        }
                //    }
                //}
                //double[] Xx = new double[xy[k].Length];
                //for (int i = 0; i < xy[k].Length; i++)
                //    Xx[i] = Convert.ToDouble(xy[k][i]);
                ////
                //return Xx;
                //
                //
                #endregion
                //Выбор платформы расчета, создание контекста
                ComputeContextPropertyList properties = new ComputeContextPropertyList(ComputePlatform.Platforms[0]);
                ComputeContext context = new ComputeContext(ComputeDeviceTypes.All, properties, null, IntPtr.Zero);
                //Инициализация OpenCl, выбор устройства
                ComputeCommandQueue commands = new ComputeCommandQueue(context, context.Devices[0], ComputeCommandQueueFlags.None);
                //Считывание текста программы из файла
                string s = AppDomain.CurrentDomain.BaseDirectory;
                StreamReader streamReader = new StreamReader("Diff.cl");
                string clSource = streamReader.ReadToEnd();
                streamReader.Close();
                //Компиляция программы
                ComputeProgram program = new ComputeProgram(context, clSource);
                program.Build(context.Devices, "", null, IntPtr.Zero);
                //Создание ядра
                ComputeKernel kernel = program.CreateKernel("Test");
                //
                stopW1.Start();
                //
                //Создание буферов параметров
                int[] Ar = { NX, NY };
                //Выделение памяти на device(OpenCl) под переменные
                ComputeBuffer<float> XX = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, x);
                ComputeBuffer<float> YY = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, y);
                ComputeBuffer<int> ar = new ComputeBuffer<int>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, Ar);
                // установка буферов в kernel
                kernel.SetMemoryArgument(0, XX);
                kernel.SetMemoryArgument(1, YY);
                kernel.SetMemoryArgument(2, ar);
                //массив, определяющий размерность расчета (количество потоков в определенном измерении)
                long[] globalSize = new long[1];
                globalSize[0] = (long)Math.Round(NX * NY / 4.0, MidpointRounding.AwayFromZero);// тк в Diff.cl width=4
                //
                stopW1.Stop();
                //
                stopCalc.Restart();
                //
                //Повторение расчета до примерной сходимости алгоритма
                for (int i = 0; i < iteration; i++)
                {
                    //Вызов ядра
                    commands.Execute(kernel, null, globalSize, null, null);
                    //Ожидание окончания выполнения программы
                    commands.Finish();
                }
                //
                stopCalc.Stop();
                //
                stopW1.Start();
                // чтение искомой функции из буфера kernel-а
                commands.ReadFromBuffer(XX, ref x, true, null);
                commands.ReadFromBuffer(YY, ref y, true, null);
                //очищение памяти в порядке, обратном созданию
                ar.Dispose();
                YY.Dispose();
                XX.Dispose();
                kernel.Dispose();
                program.Dispose();
                commands.Dispose();
                context.Dispose();
                //конвертация координат в double
                double[][] XY = new double[2][];
                XY[0] = new double[x.Length];
                XY[1] = new double[y.Length];
                for (int i = 0; i < x.Length; i++)
                {
                    XY[0][i] = Convert.ToDouble(x[i]);
                    XY[1][i] = Convert.ToDouble(y[i]);
                }
                //
                stopW1.Stop();
                stopAll.Stop();
                //
                timeAll = stopAll.Elapsed;
                timeCalculate = stopCalc.Elapsed;
                timeTransrort = stopW1.Elapsed;
                //
                return XY;
            }
            catch (Exception ex)
            {
                exception += ex.Message.ToString();
                return null;
            }
        }
        /// <summary>
        /// расчет координат с помощью OpenCL (включая досчет?)
        /// </summary>
        /// <param name="k">параметр расчета: 0 - х-координата, 1 - у-координата</param>
        /// <param name="iteration">количество итераций расчета</param>
        /// <returns></returns>
        double[] OCL_DiffGeneration(int k, int iteration)
        {
            try
            {
                //Stopwatch sw = new Stopwatch();
                //!!! для part!=1
                float[] x = new float[NX * NY];
                // установка граничных условий 
                //слева - справа   
                for (int i = 0; i < NY; i++)
                {
                    x[i * NX] = (float)LeftXY[k][i];
                    x[(NX - 1) + NX * i] = (float)RightXY[k][i];
                }
                //сверху-снизу
                for (int i = 0; i < NX; i++)
                {
                    x[i] = (float)TopXY[k][i];
                    x[NX * (NY - 1) + i] = (float)BottomXY[k][i];
                }
                //
                #region Тест кода ядра
                //int width = 4; double ls1=0, ls2=0;
                //for (int iter = 0; iter < iteration; iter++)
                //{
                //    for (int i = 0; i < NX * NY / 4; i++)
                //    {
                //        int id = width * i;
                //        int cid;
                //        ///
                //        for (int m = 0; m < width; m++)
                //        {
                //            cid = id + m;
                //            if ((cid > NX) & (cid < NX * NY - NX))// - проверка на первую и последнюю строчку
                //                //cid = min(max((id + m), NX - 1), NX * NY - NX - 1);
                //                //
                //                if ((cid % NX != 0) & ((cid + 1) % NX != 0))
                //                {
                //                    ls1 = LineStretch[NY-1-cid/NX+1]; ls2 = LineStretch[NY-1-cid/NX-1];
                //                    xy[0][cid] = (float)(0.25 * (xy[0][cid + 1] + xy[0][cid - 1]) +  0.25*ls1 * xy[0][cid - NX] + 0.25*ls2 * xy[0][cid + NX]);
                //                    xy[1][cid] = (float)(0.25 * (xy[1][cid + 1] + xy[1][cid - 1]) + 0.25*ls1 * xy[1][cid - NX] + 0.25*ls2 * xy[1][cid + NX]);
                //                }
                //        }
                //    }
                //}
                //double[] Xx = new double[xy[k].Length];
                //for (int i = 0; i < xy[k].Length; i++)
                //    Xx[i] = Convert.ToDouble(xy[k][i]);
                ////
                //return Xx;
                //
                //
                #endregion
                //Выбор платформы расчета, создание контекста
                ComputeContextPropertyList properties = new ComputeContextPropertyList(ComputePlatform.Platforms[0]);
                ComputeContext context = new ComputeContext(ComputeDeviceTypes.All, properties, null, IntPtr.Zero);
                //Инициализация OpenCl, выбор устройства
                ComputeCommandQueue commands = new ComputeCommandQueue(context, context.Devices[0], ComputeCommandQueueFlags.None);
                //Считывание текста программы из файла
                string s = AppDomain.CurrentDomain.BaseDirectory;
                StreamReader streamReader = new StreamReader("..\\..\\..\\Mesh\\MeshLib\\OpenCL\\Diff.cl");
                string clSource = streamReader.ReadToEnd();
                streamReader.Close();
                //Компиляция программы
                ComputeProgram program = new ComputeProgram(context, clSource);
                program.Build(context.Devices, "", null, IntPtr.Zero);
                //Создание ядра
                ComputeKernel kernel = program.CreateKernel("Test");
                //Создание буферов параметров
                int[] Ar = { NX, NY };
                //Выделение памяти на device(OpenCl) под переменные
                ComputeBuffer<float> XX = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, x);
                ComputeBuffer<int> ar = new ComputeBuffer<int>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, Ar);
                // установка буферов в kernel
                kernel.SetMemoryArgument(0, XX);
                kernel.SetMemoryArgument(1, ar);
                //массив, определяющий размерность расчета (количество потоков в определенном измерении)
                long[] globalSize = new long[1];
                globalSize[0] = (long)Math.Round(NX * NY / 4.0, MidpointRounding.AwayFromZero);// тк в Diff.cl width=4
                //sw.Start();
                //Повторение расчета до примерной сходимости алгоритма
                for (int i = 0; i < iteration; i++)
                {
                    //Вызов ядра
                    commands.Execute(kernel, null, globalSize, null, null);
                    //Ожидание окончания выполнения программы
                    commands.Finish();
                }
                //sw.Stop();
                //double ms = (sw.ElapsedMilliseconds);
                // чтение искомой функции из буфера kernel-а
                commands.ReadFromBuffer(XX, ref x, true, null);
                //очищение памяти в порядке, обратном созданию
                ar.Dispose();
                XX.Dispose();
                kernel.Dispose();
                program.Dispose();
                commands.Dispose();
                context.Dispose();
                //конвертация координат в double
                double[] Xd = new double[x.Length];
                for (int i = 0; i < x.Length; i++)
                    Xd[i] = Convert.ToDouble(x[i]);
                //
                return Xd;
            }
            catch (Exception ex)
            {
                exception += ex.Message.ToString();
                return null;
            }
        }


    }
}
