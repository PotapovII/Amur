namespace FEMTasksLib.FEMTasks.VortexStream
{
    using CommonLib;
    using CommonLib.Mesh;
    using MemLogLib;
    using System;

    public class VSUtils
    {
        /// <summary>
        ///Расчет компонент поля скорости в узлах сетки по функции тока
        /// </summary>
        /// <param name="result">результат решения</param>
        public static void CalcVelosity(double[] Phi, ref double[] Vx, ref double[] Vy,
                         IMeshWrapper wMesh, double R_midle = 0, int Ring = 0)
        {
            try
            {
                IMesh mesh = wMesh.GetMesh();
                /// Производные 
                double[][] dNdx = wMesh.GetdNdx();
                double[][] dNdy = wMesh.GetdNdy();
                /// Список узлов для КЭ
                TriElement[] eKnots = mesh.GetAreaElems();
                double[] X = mesh.GetCoords(0);
                double[] tmpVx = null;
                double[] tmpVy = null;
                MEM.Alloc((uint)mesh.CountElements, ref tmpVx, "tmpVx");
                MEM.Alloc((uint)mesh.CountElements, ref tmpVy, "tmpVy");
                for (uint elem = 0; elem < mesh.CountElements; elem++)
                {
                    double[] b = dNdx[elem];
                    double[] c = dNdy[elem];
                    uint i0 = eKnots[elem].Vertex1;
                    uint i1 = eKnots[elem].Vertex2;
                    uint i2 = eKnots[elem].Vertex3;
                    double dPhidx = Phi[i0] * b[0] + Phi[i1] * b[1] + Phi[i2] * b[2];
                    double dPhidy = Phi[i0] * c[0] + Phi[i1] * c[1] + Phi[i2] * c[2];
                    if (Ring == 0)
                    {
                        tmpVx[elem] = dPhidy;
                        tmpVy[elem] = -dPhidx;
                    }
                    else
                    {
                        double R_elem = R_midle + (X[i0] + X[i1] + X[i1]) / 3;
                        tmpVx[elem] = dPhidy / R_elem;
                        tmpVy[elem] = -dPhidx / R_elem;
                    }
                }
                wMesh.ConvertField(ref Vx, tmpVx);
                wMesh.ConvertField(ref Vy, tmpVy);
                VelocityZerroOnBed(mesh, ref Vx, ref Vy);
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
        }
        /// <summary>
        /// Установка однородных условий на донной поверхности
        /// </summary>
        public static void VelocityZerroOnBed(IMesh mesh, ref double[] Vx, ref double[] Vy)
        {
            var bknotsMark = mesh.GetBoundKnotsMark();
            var bknots = mesh.GetBoundKnots();
            double[] X = mesh.GetCoords(0);
            for (uint bknot = 0; bknot < mesh.CountBoundKnots; bknot++)
            {
                int mark = bknotsMark[bknot];
                uint knot = (uint)bknots[bknot];
                if (mark != 2) // свободная поверхность
                {
                    Vx[knot] = 0;
                    Vy[knot] = 0;
                }
            }
        }
    }
}
