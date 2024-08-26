//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 11.08.2021 Потапов И.И.
//---------------------------------------------------------------------------
//             обновление и чистка : 01.01.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace TriMeshGeneratorLib
{
    using System;
    using System.IO;
    /// <summary>
    /// Узел сетки для решения задачи мелкой воды
    /// </summary>
    public class RVNodeShallowWater : RVNode
    {
        protected double G = 9.806;
        /// <summaryint
        /// шероховатость дна
        /// </summaryint
        public double Ks { get=>ks; set=>ks = value; }
        protected double ks;
        /// <summaryint
        /// глубина потока
        /// </summaryint
        public double Depth { get => d; set => d = value; }
        protected double d;
        /// <summaryint
        /// расход по х
        /// </summaryint
        public double Qx { get => qx; set => qx = value; }
        protected double qx;
        /// <summaryint
        /// расход по у
        /// </summaryint
        public double Qy { get => qy; set => qy = value; }
        protected double qy;
        public RVNodeShallowWater(int nm = 1,
                    double xc = 1000.0,
                    double yc = 1000.0,
                    double zc = 100.0,
                    double ks = 0.05,
                    double depth = 1.0,
                    double xDis = 0.0,
                    double yDis = 0.0
                    ) : base(nm, xc, yc, zc)
        {
            this.ks = ks;
            d =  depth;
            qx = xDis;
            qy = yDis;
        }
        public RVNodeShallowWater(StreamReader file)
        {
        }
        public void SetFlow(double depth, double xDis, double yDis)
        {
            d = depth; qx = xDis; qy = yDis;
        }

        public virtual void GetParName(int i, ref string parName)
        {
            switch (i)
            {
                case 0:
                    parName = "Bed Elevation";
                    break;
                case 1:
                    parName = "Bed Elevation";
                    break;
                case 2:
                    parName = "Bed Roughness";
                    break;
                case 3:
                    parName = "Depth";
                    break;
                case 4:
                    parName = "qx";
                    break;
                case 5:
                    parName = "qy";
                    break;
                case 6:
                    parName = "Water Surface Elev";
                    break;
                case 7:
                    parName = "Velocity";
                    break;
                case 8:
                    parName = "Froude #";
                    break;
            }
        }
        public override int CountPapams() { return 2; }
        public override int CountVariables() { return 3; }
        public override double GetPapam(int i)
        {
            switch (i)
            {
                case 0:
                    return z;
                case 1:
                    return z;
                case 2:
                    return ks;
                case 3:
                    return d;
                case 4:
                    return qx;
                case 5:
                    return qy;
                case 6:
                    return z + d;     // уровень свободной поверхности
                case 7:
                    return Math.Sqrt(qx * qx + qy * qy) / d;  // скорость
                case 8: // число Фруда
                    return Math.Sqrt(qx * qx + qy * qy) / (d * Math.Sqrt(Math.Abs(d) * G));
            }
            return 0.0;
        }
        public override double GetVariable(int i)
        {
            switch (i)
            {
                case 1:
                    return d;
                case 2:
                    return qx;
                case 3:
                    return qy;
            }
            return 0.0;
        }
        /// <summary>
        /// Интерполяция в точке для которой определены значения функций формы
        /// </summary>
        /// <param name="n">количество узлов</param>
        /// <param name="nPtrs">узлы со значениями</param>
        /// <param name="wts">функций формы</param>
        public override void Interpolation(int n, RVNode[] nPtrs, double[] wts)
        {
            x = 0.0;
            y = 0.0;
            z = 0.0;
            ks = 0.0;
            for (int i = 0; i < n; i++)
            {
                if (nPtrs[i] != null)
                {
                    x += wts[i] * nPtrs[i].Xo;
                    y += wts[i] * nPtrs[i].Yo;
                    z += wts[i] * nPtrs[i].Z;
                    ks += wts[i] * nPtrs[i].GetPapam(2);
                }
            }
            SaveLocation();
        }
    }
}
