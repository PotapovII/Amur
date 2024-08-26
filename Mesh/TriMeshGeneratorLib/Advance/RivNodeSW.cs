//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 11.08.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace TriMeshGeneratorLib.Advance
{
    using System;
    using System.IO;
    using CommonLib.Physics;
    /// <summary>
    /// Узел сетки для решения задачи мелкой воды
    /// </summary>
    public class RivNodeSW : RivNode
    {
        /// <summaryint
        /// шероховатость дна
        /// </summaryint
        public double Ks;
        /// <summaryint
        /// глубина потока
        /// </summaryint
        public double Depth;
        /// <summaryint
        /// расход по х
        /// </summaryint
        public double Qx;
        /// <summaryint
        /// расход по у
        /// </summaryint
        public double Qy;
        public RivNodeSW(int ID = 1,
                    double X = 1000.0,
                    double Y = 1000.0,
                    double Z = 100.0,
                    double Ks = 0.05,
                    double Depth = 1.0,
                    double Qx = 0.0,
                    double Qy = 0.0
                    ) : base(ID, X, Y, Z)
        {
            this.Ks = Ks;
            this.Depth = Depth;
            this.Qx = Qx;
            this.Qy = Qy;
        }
        public RivNodeSW(RivNodeSW p) : base(p)
        {
            Ks = p.Ks;
            Depth = p.Depth;
            Qx = p.Qx;
            Qy = p.Qy;
        }
        public RivNodeSW(StreamReader file)
        {
            
        }
        public void SetFlow(double depth, double xDis, double yDis)
        {
            Depth = depth; Qx = xDis; Qy = yDis;
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
                    return Z;
                case 1:
                    return Z;
                case 2:
                    return Ks;
                case 3:
                    return Depth;
                case 4:
                    return Qx;
                case 5:
                    return Qy;
                case 6:
                    return Z + Depth;     // уровень свободной поверхности
                case 7:
                    return Math.Sqrt(Qx * Qx + Qy * Qy) / Depth;  // скорость
                case 8: // число Фруда
                    return Math.Sqrt(Qx * Qx + Qy * Qy) / (Depth * Math.Sqrt(Math.Abs(Depth) * SPhysics.GRAV));
            }
            return 0.0;
        }
        public override double GetVariable(int i)
        {
            switch (i)
            {
                case 1:
                    return Depth;
                case 2:
                    return Qx;
                case 3:
                    return Qy;
            }
            return 0.0;
        }
        /// <summary>
        /// Интерполяция в точке для которой определены значения функций формы
        /// </summary>
        /// <param name="n">количество узлов</param>
        /// <param name="points">узлы со значениями</param>
        /// <param name="formFunctions">функций формы</param>
        public override void Interpolation(RivNode[] points, double[] formFunctions)
        {
            X = 0.0;
            Y = 0.0;
            Z = 0.0;
            Ks = 0.0;
            for (int i = 0; i < formFunctions.Length; i++)
            {
                if (points[i] != null)
                {
                    X += formFunctions[i] * points[i].Xo;
                    Y += formFunctions[i] * points[i].Yo;
                    Z += formFunctions[i] * points[i].Z;
                    Ks += formFunctions[i] * points[i].GetPapam(2);
                }
            }
            SaveLocation();
        }

        public static void Write(StreamWriter file, RivNodeSW n)
        {
            RivNode.Write(file, n);
            if (n != null)
            {
                file.Write(n.Ks.ToString("F8") + " ");
                file.Write(n.Depth.ToString("F8") + " ");
                file.Write(n.Qx.ToString("F4") + " ");
                file.Write(n.Qy.ToString("F4") + " ");
            }
        }
    }
}
