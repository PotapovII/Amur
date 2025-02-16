using System;
using MeshLib;
using ManagedCuda;
using GeometryLib;
using GeometryLib.Areas;

namespace TestEliz2D
{
    public abstract class PointsGeneratorEliz
    {
        #region Для замера скорости генерации сетки
        protected System.Diagnostics.Stopwatch stopW1 = new System.Diagnostics.Stopwatch();
        protected System.Diagnostics.Stopwatch stopCalc = new System.Diagnostics.Stopwatch();
        protected System.Diagnostics.Stopwatch stopAll = new System.Diagnostics.Stopwatch();
        public TimeSpan timeTransrort;
        public TimeSpan timeCalculate;
        public TimeSpan timeAll;
        #endregion
        //
        #region Основные параметры
        public bool OpenCL = false;
        public bool Cuda = false;
        string exception = "";
        protected double P = 1, Q = 1;
        protected double[] X, Y;
        protected double[][] TopXY, BottomXY;
        public Parameter p;

        #endregion
        //
        public abstract string Name { get; }
        //
        public PointsGeneratorEliz()
        { }
        public  PointsGeneratorEliz(Parameter p)
        {
            this.p = p;
        }
        public KsiMesh Generate(SimpleAreaProfile sArea)
        {
            GenerateCoords(sArea);
            KsiMesh tm = new KsiMesh(p.Nx, p.Ny, X, Y);
            //
            return tm;
        }
        abstract protected void GenerateCoords(SimpleAreaProfile sArea);

        //
        //
        /// <summary>
        /// функция растяжения линии
        /// </summary>
        /// <param name="P">регулятор наклона</param>
        /// <param name="Q">демпфирующий параметр</param>
        /// <returns>массив от 0 до 1 с распределением точек</returns>
        protected static double[] Stretch(double P, double Q, int CountPoints)
        {
            double[] S = new double[CountPoints];
            int AN = CountPoints - 1;
            double DETA = 1.0f / AN;
            double TQI = (double)(1.0f / Math.Tanh(Q));
            double AL, DUM, ETA;
            for (int i = 0; i < CountPoints; i++)
            {
                AL = i;
                ETA = AL * DETA;
                DUM = Q * (1 - ETA);
                DUM = 1 - Math.Tanh(DUM) * TQI;
                S[i] = P * ETA + (1 - P) * DUM;
            }
            return S;
        }
        /// <summary>
        /// Метод для реализации CUDA
        /// </summary>
        /// <param name="bPinGenericMemory"></param>
        /// <param name="pp_a"></param>
        /// <param name="pp_Aligned_a"></param>
        /// <param name="nbytes"></param>
        protected static void AllocateHostMemory(bool bPinGenericMemory, ref float[] pp_a, ref CudaPageLockedHostMemory<float> pp_Aligned_a, int nbytes)
        {
            //Console.Write("> cudaMallocHost() allocating {0:0.00} Mbytes of system memory\n", (float)nbytes / 1048576.0f);
            // allocate host memory (pinned is required for achieve asynchronicity)
            if (pp_Aligned_a != null)
                pp_Aligned_a.Dispose();

            pp_Aligned_a = new CudaPageLockedHostMemory<float>(nbytes / sizeof(float));
            pp_a = new float[nbytes / sizeof(float)];
        }
    }
}
