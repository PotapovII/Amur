//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                    создание поля IField<T>
//                 кодировка : 14.12.2020 Потапов И.И.
//---------------------------------------------------------------------------

namespace GeometryLib
{
    using System;
    using MemLogLib;
    [Serializable]
    public class FieldSquare
    {
        public int CountI;
        public int CountJ;
        int NI;
        int NJ;
        double dx;
        double dy;
        /// <summary>
        /// Полу ширина области
        /// </summary>
        double halfWidth;
        /// <summary>
        /// Полувысота области
        /// </summary>
        double halfHeight;
        /// <summary>
        /// Значения опорного поля
        /// </summary>
        double[][] Values;
        //               
        //               
        // |--------------|--------------| ----
        // |  3           |            2 |   ^
        // |              |              |   |  
        // |              | <-halfWidth->|  halfHeight
        // |              |              |   | 
        // |              |              |   V   x (i)
        // |-----------------------------| ------>
        // |              |              |
        // |              |              |  
        // |              |              |
        // |              |              |  
        // |  0           |            1 |
        // |--------------|--------------|
        //                |
        //                V y (j) 
        //
        public FieldSquare(double halfWidth, double halfHeight, double[][] Values)
        {
            if (Values == null) throw new Exception(" Источник пуст : FieldSquare");
            MEM.MemCopy(ref this.Values, Values);
            this.halfHeight = halfHeight;
            this.halfWidth = halfWidth;
            Set();
        }
        public FieldSquare(double halfWidth, double halfHeight, double[,] Values)
        {
            if (Values == null) throw new Exception(" Источник пуст : FieldSquare");
            MEM.MemCopy(ref this.Values, Values);
            this.halfHeight = halfHeight;
            this.halfWidth = halfWidth;
            Set();
        }

        public void Set()
        {
            CountJ = Values.Length;
            CountI = Values[0].Length;
            NI = CountI - 2;
            NJ = CountJ - 2;
            dx = 2 * halfWidth / (CountI - 1);
            dy = 2 * halfHeight / (CountJ - 1);
        }

        int indexX(double x)
        {
            int i = (int)((x + halfWidth) / dx); 
            i = i < 0 ? 0 : i;
            i = i > NI ? NI : i;
            return i;
        }
        int indexY(double y)
        {
            int i = (int)((y + halfHeight) / dy);
            i = i < 0 ? 0 : i;
            i = i > NJ ? NJ : i;
            return i;
        }
        /// <summary>
        /// Для паралелльной обработки данных
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public double Value(double x, double y)
        {
            int i = indexX(x);
            int j = indexY(y);
            double x0 = - halfWidth + i * dx; 
            double y0 = - halfHeight + j * dy;

            double NX2 = (x - x0) / dx;
            double NX1 = 1 - NX2;
            double NY2 = (y - y0) / dy;
            double NY1 = 1 - NY2;

            double N0 = NX1 * NY2;
            double N1 = NX2 * NY2;
            double N2 = NX2 * NY1;
            double N3 = NX1 * NY1;

            double V0 = Values[j + 1][i    ];
            double V1 = Values[j + 1][i + 1];
            double V2 = Values[j    ][i + 1];
            double V3 = Values[j    ][i    ];
            double V = N0 * V0 + N1 * V1 + N2 * V2 + N3 * V3;
            return V;
        }

        int i,j;
        double x0,y0;
        double NX2,NX1,NY2, NY1;
        double N0,N1,N2,N3;
        double V0,V1,V2,V3,V;
        public double SValue(double x, double y)
        {
             i = indexX(x);
             j = indexY(y);
             x0 = -halfWidth + i * dx;
             y0 = -halfHeight + j * dy;

             NX2 = (x - x0) / dx;
             NX1 = 1 - NX2;
             NY2 = (y - y0) / dy;
             NY1 = 1 - NY2;

             N0 = NX1 * NY2;
             N1 = NX2 * NY2;
             N2 = NX2 * NY1;
             N3 = NX1 * NY1;

             V0 = Values[j + 1][i];
             V1 = Values[j + 1][i + 1];
             V2 = Values[j][i + 1];
             V3 = Values[j][i];
             V = N0 * V0 + N1 * V1 + N2 * V2 + N3 * V3;
            return V;
        }
    }
}
