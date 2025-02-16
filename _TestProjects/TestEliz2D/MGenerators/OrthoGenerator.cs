using System.Collections.Concurrent;
using System;
using System.IO;
using System.Reflection;
using Cloo;
using ManagedCuda.BasicTypes;
using ManagedCuda.VectorTypes;
using ManagedCuda;
using System.Threading.Tasks;
using GeometryLib;
using GeometryLib.Areas;

namespace TestEliz2D
{
    //
    public class OrthoGenerator : PointsGeneratorEliz
    {
        string exception = "";
        int NX;
        int NY;
        bool flagFirstX = true;
        double[][] LeftXY, RightXY;
        float[] x, y;
        float RelaxOrtho, Tay;
        //
        OrderablePartitioner<Tuple<int, int>> rangePartitioner;
        public override string Name
        {
            get { return "Ортотропная генерация"; }
        }
        public OrthoGenerator() { }
        //
        public OrthoGenerator(Parameter p)
        {
            ChangeParameters(p as OrthoParameter);
        }
        //
        public void ChangeParameters(OrthoParameter p)
        {
            this.p = p;
            NX = p.Nx;
            NY = p.Ny;
            P = p.P;
            Q = p.Q;
            RelaxOrtho = (float)p.RelaxOrtho;
            Tay = (float)p.Tay;
            x = new float[NX * NY];
            y = new float[NX * NY];
            //
            rangePartitioner = Partitioner.Create(NX + 1, NX * NY - NX - 1);
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
            //double dy = H / (NY - 1);
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
            //
            //при первичном расчете координат задается достаточно много иетарций
            if (flagFirstX)
            {
                OCL_OrthoGenerator(NX * NY * 2, out X, out Y);
                flagFirstX = false;
            }
            // если расчет не первичный, в памяти остаются координаты с предыдущего расчета,
            // поэтому производится не расчет с нуля, а досчет (итераций меньше)
            else
                OCL_OrthoGenerator(NX * NY * 2, out X, out Y);
        }

        void OCL_OrthoGenerator(int iteration, out double[] Xd, out double[] Yd)
        {
            // Выделение памяти под выходные массивы координат
            Xd = new double[NX * NY];
            Yd = new double[NX * NY];
            //Stopwatch sw = new Stopwatch();
            //
            if (Cuda)
            {
                stopAll.Restart();
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
                    resName = "OrthoGrid_x64.ptx";
                else
                    resName = "OrthoGrid_x64.ptx";
                string resNamespace = "MeshLib";
                string resource = resNamespace + "." + resName;
                Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
                if (stream == null) throw new ArgumentException("Kernel not found in resources.");
                CudaKernel OrthoGridKernel = ctx.LoadKernelPTX(stream, "OrthoGrid");

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
                float[] Ar = { RelaxOrtho, Tay };
                int[] N = { NX, NY };
                CudaDeviceVariable<float> d_Ar = Ar;
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
                OrthoGridKernel.BlockDimensions = threads;
                OrthoGridKernel.GridDimensions = blocks;
                //sw.Start();
                stopW1.Stop();
                //
                stopCalc.Restart();
                //
                for (int k = 0; k < 2 * NX * NY; k++)
                {
                    //for (int i = 0; i < nstreams; i++)
                    //DiffGridKernel.RunAsync(streams[i].Stream, d_X.DevicePointer + i * size * sizeof(float), d_Y.DevicePointer + i * size * sizeof(float), NX, NY, i, size);
                    OrthoGridKernel.Run(d_X.DevicePointer, d_Y.DevicePointer, d_Ar.DevicePointer, d_N.DevicePointer);
                    //ctx.Synchronize();
                }
                //
                stopCalc.Stop();
                //
                stopW1.Start();
                //sw.Stop();
                //double ms = (sw.ElapsedMilliseconds);

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

                for (int i = 0; i < NX * NY; i++)
                {
                    Xd[i] = Convert.ToDouble(h_X[i]);
                    Yd[i] = Convert.ToDouble(h_Y[i]);
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
                // твой код вызова ядра на Cuda
                // результат должен быть записан в массивы Xd и Yd в формате double
                stopW1.Stop();
                stopAll.Stop();
                //
                timeCalculate = stopCalc.Elapsed;
                timeTransrort = stopW1.Elapsed;
                timeAll = stopAll.Elapsed;
            }
            else if (OpenCL)
            {
                stopAll.Restart();
                //
                stopW1.Restart();
                x = new float[NX * NY];
                y = new float[NX * NY];
                // установка граничных условий 
                // слева - справа   
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
                try
                {
                    //Создание контекста, выбор платформы расчета
                    ComputeContextPropertyList properties = new ComputeContextPropertyList(ComputePlatform.Platforms[0]);
                    ComputeContext context = new ComputeContext(ComputeDeviceTypes.All, properties, null, IntPtr.Zero);
                    //Инициализация OpenCl
                    ComputeCommandQueue commands = new ComputeCommandQueue(context, context.Devices[0], ComputeCommandQueueFlags.None);
                    //Чтение текста программы из файла
                    string s = AppDomain.CurrentDomain.BaseDirectory;
                    StreamReader streamReader = new StreamReader("Ortho.cl");
                    //D:\Dropbox\работа\NoOpenCL\RiverComplex\MeshLibrary\OpenCL
                    string clSource = streamReader.ReadToEnd();
                    streamReader.Close();
                    //Компиляция программы
                    ComputeProgram program = new ComputeProgram(context, clSource);
                    program.Build(context.Devices, "", null, IntPtr.Zero);
                    //Создание ядра
                    ComputeKernel kernel = program.CreateKernel("Test1");
                    //
                    stopW1.Start();
                    //
                    // Создание массивов под аргументы расчета
                    float[] Ar = { RelaxOrtho, Tay };
                    int[] N = { NX, NY };
                    //Создание буферов переменных расчета
                    ComputeBuffer<float> XX = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, x);
                    ComputeBuffer<float> YY = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, y);
                    ComputeBuffer<float> ar = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, Ar);
                    ComputeBuffer<int> NN = new ComputeBuffer<int>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, N);
                    //Установка буферов в kernel
                    kernel.SetMemoryArgument(0, XX);
                    kernel.SetMemoryArgument(1, YY);
                    kernel.SetMemoryArgument(2, ar);
                    kernel.SetMemoryArgument(3, NN);
                    //Массив, указывающий размерность расчета (количество потоков)
                    long[] globalSize = new long[1];
                    globalSize[0] = (long)Math.Round((NX * NY - 2 * NX) / 4.0, MidpointRounding.AwayFromZero);// так как в Ortho.cl width = 4
                    //
                    stopW1.Stop();
                    //
                    stopCalc.Restart();
                    //
                    // Повторение расчета до примерной сходимости алгоритма
                    for (int i = 0; i < iteration; i++)
                    {
                        commands.Execute(kernel, null, globalSize, null, null);
                        commands.Finish();
                    }
                    //
                    stopCalc.Stop();
                    //
                    stopW1.Start();
                    //
                    //Чтение результата из переменной
                    commands.ReadFromBuffer(XX, ref x, true, null);
                    commands.ReadFromBuffer(YY, ref y, true, null);
                    //Очищение памяти в порядке, противоположном создания
                    NN.Dispose();
                    ar.Dispose();
                    YY.Dispose();
                    XX.Dispose();
                    //
                    kernel.Dispose();
                    program.Dispose();
                    commands.Dispose();
                    context.Dispose();
                }
                catch (Exception ex)
                {
                    exception += ex.Message.ToString();
                }
                finally
                {
                    //Конвертация координат из float в double и запись в переменные вывода
                    for (int i = 0; i < NX * NY; i++)
                    {
                        Xd[i] = Convert.ToDouble(x[i]);
                        Yd[i] = Convert.ToDouble(y[i]);
                    }
                    //
                    stopW1.Stop();
                    stopAll.Stop();
                    //
                    timeAll = stopAll.Elapsed;
                    timeCalculate = stopCalc.Elapsed;
                    timeTransrort = stopW1.Elapsed;
                }

            }
            else
            {
                stopAll.Restart();
                //
                stopW1.Restart();
                double[] Xdl = new double[NX * NY]; // массивы для работы в Parallel, так как метод не работает с out и ref массивами
                double[] Ydl = new double[NX * NY];
                // установка граничных условий 
                // слева - справа   
                for (int i = 0; i < NY; i++)
                {
                    Xdl[i * NX] = LeftXY[0][i];
                    Xdl[(NX - 1) + NX * i] = RightXY[0][i];
                    Ydl[i * NX] = LeftXY[1][i];
                    Ydl[(NX - 1) + NX * i] = RightXY[1][i];
                }
                //сверху-снизу
                for (int i = 0; i < NX; i++)
                {
                    Xdl[i] = TopXY[0][i];
                    Xdl[NX * (NY - 1) + i] = BottomXY[0][i];
                    Ydl[i] = TopXY[1][i];
                    Ydl[NX * (NY - 1) + i] = BottomXY[1][i];
                }
                stopW1.Stop();
                //
                #region Последовательная версия
                //double xp = 0; double xe = 0; double xw = 0; double xs = 0; double xn = 0;
                //double yp = 0; double ye = 0; double yw = 0; double ys = 0; double yn = 0;
                //double xen = 0; double xwn = 0; double xes = 0; double xws = 0;
                //double yen = 0; double ywn = 0; double yes = 0; double yws = 0;
                //double Ap = 0; double Ig = 0; double Alpha = 0; double Betta = 0; double Gamma = 0; double Delta = 0;
                //double RelaxOrt = RelaxOrtho;
                //double Tau = Tay;
                //
                //for (int k = 0; k < 2 * NX * NY; k++)
                //    for (int i = NX + 1; i < NX * NY - NX - 1; i++)
                //    {
                //        if ((i % NX != 0) & ((i + 1) % NX != 0))
                //        {
                //            xp = Xd[i];
                //            xe = Xd[i + 1];
                //            xw = Xd[i - 1];
                //            xs = Xd[i - NX];
                //            xes = Xd[i - NX + 1];
                //            xws = Xd[i - NX - 1];

                //            xn = Xd[i + NX];
                //            xen = Xd[i + NX + 1];
                //            xwn = Xd[i + NX - 1];

                //            yp = Yd[i];
                //            ye = Yd[i + 1];
                //            yw = Yd[i - 1];
                //            ys = Yd[i - NX];
                //            yes = Yd[i - NX + 1];
                //            yws = Yd[i - NX - 1];

                //            yn = Yd[i + NX];
                //            yen = Yd[i + NX + 1];
                //            ywn = Yd[i + NX - 1];

                //            /// g22
                //            Alpha = 0.25 * ((xn - xs) * (xn - xs) + (yn - ys) * (yn - ys));
                //            /// g12
                //            Betta = RelaxOrt * 0.25 * ((xe - xw) * (xn - xs) + (ye - yw) * (yn - ys));
                //            /// g11
                //            Gamma = 0.25 * ((xe - xw) * (xe - xw) + (ye - yw) * (ye - yw));
                //            /// чтобы не вылетать за float
                //            if ((Alpha + Gamma) < 0.000001)
                //            {
                //                Alpha = 1;
                //                Gamma = 1;
                //                Betta = 0;
                //            }
                //            //
                //            Ig = Alpha + Gamma;
                //            Ap = 1.0 / (2 * Ig);

                //            xp = Ap * (Alpha * (xw + xe) + Gamma * (xn + xs) - 0.5 * Betta * (xen - xwn - xes + xws));

                //            yp = Ap * (Alpha * (yw + ye) + Gamma * (yn + ys) - 0.5 * Betta * (yen - ywn - yes + yws));

                //            Xd[i] = (1 - Tau) * Xd[i] + Tau * xp;
                //            Yd[i] = (1 - Tau) * Yd[i] + Tau * yp;
                //        }

                //    }
                #endregion
                //
                stopCalc.Restart();
                //
                // Параллельная версия
                double RelaxOrt = RelaxOrtho;
                double Tau = Tay;
                //
                for (int k = 0; k < iteration; k++)
                {
                    Parallel.ForEach(rangePartitioner,
                    (range, loopState) =>
                    {
                        for (int i = range.Item1; i < range.Item2; i++)
                        {
                            if ((i % NX != 0) & ((i + 1) % NX != 0))
                            {
                                double xp = Xdl[i];
                                double xe = Xdl[i + 1];
                                double xw = Xdl[i - 1];
                                double xs = Xdl[i - NX];
                                double xes = Xdl[i - NX + 1];
                                double xws = Xdl[i - NX - 1];

                                double xn = Xdl[i + NX];
                                double xen = Xdl[i + NX + 1];
                                double xwn = Xdl[i + NX - 1];

                                double yp = Ydl[i];
                                double ye = Ydl[i + 1];
                                double yw = Ydl[i - 1];
                                double ys = Ydl[i - NX];
                                double yes = Ydl[i - NX + 1];
                                double yws = Ydl[i - NX - 1];

                                double yn = Ydl[i + NX];
                                double yen = Ydl[i + NX + 1];
                                double ywn = Ydl[i + NX - 1];

                                /// g22
                                double Alpha = 0.25 * ((xn - xs) * (xn - xs) + (yn - ys) * (yn - ys));
                                /// g12
                                double Betta = RelaxOrt * 0.25 * ((xe - xw) * (xn - xs) + (ye - yw) * (yn - ys));
                                /// g11
                                double Gamma = 0.25 * ((xe - xw) * (xe - xw) + (ye - yw) * (ye - yw));
                                /// чтобы не вылетать за float
                                if ((Alpha + Gamma) < 0.000001)
                                {
                                    Alpha = 1;
                                    Gamma = 1;
                                    Betta = 0;
                                }
                                //
                                double Ig = Alpha + Gamma;
                                double Ap = 1.0 / (2 * Ig);

                                xp = Ap * (Alpha * (xw + xe) + Gamma * (xn + xs) - 0.5 * Betta * (xen - xwn - xes + xws));

                                yp = Ap * (Alpha * (yw + ye) + Gamma * (yn + ys) - 0.5 * Betta * (yen - ywn - yes + yws));

                                Xdl[i] = (1 - Tau) * Xdl[i] + Tau * xp;
                                Ydl[i] = (1 - Tau) * Ydl[i] + Tau * yp;
                            }

                        }
                    });
                }
                stopCalc.Stop();
                //
                stopW1.Start();
                // метод не работает с out и ref массивами Xd и Yd
                for (int i = 0; i < NX * NY; i++)
                {
                    Xd[i] = Xdl[i];
                    Yd[i] = Ydl[i];
                }
                stopW1.Stop();
                stopAll.Stop();
                //
                timeAll = stopAll.Elapsed;
                timeCalculate = stopCalc.Elapsed;
                timeTransrort = stopW1.Elapsed;
            }

        }

    }
}
