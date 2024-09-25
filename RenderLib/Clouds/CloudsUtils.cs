using CommonLib;
using CommonLib.Areas;
using GeometryLib;
using GeometryLib.Vector;
using MemLogLib;
using System;
using System.Linq;

namespace RenderLib
{
    public class CloudsUtils
    {
        public static RectangleWorld GetWorld(IClouds cloud)
        {
            if (cloud == null)
                return new RectangleWorld();
            double MinX = 0, MaxX = 0, MinY = 0, MaxY = 0;
            cloud.MinMax(0, ref MinX, ref MaxX);
            cloud.MinMax(1, ref MinY, ref MaxY);
            RectangleWorld range = new RectangleWorld((float)MinX, (float)MaxX, (float)MinY, (float)MaxY);
            if(range.Width>0 && range.Height>0)
                return range;
            RectangleWorld Rdata = new RectangleWorld();
            RectangleWorld World = RectangleWorld.Extension(ref range, ref Rdata);
            return World;
        }
        /// <summary>
        /// Получить заначения для точек обласка
        /// </summary>
        /// <param name="indexPole"></param>
        /// <param name="MinV"></param>
        /// <param name="MaxV"></param>
        /// <param name="Values"></param>
        /// <param name="VX"></param>
        /// <param name="VY"></param>
        /// <param name="Dim"></param>
        /// <returns></returns>
        public static bool GetPoleMinMax(IField pole, ref double MinV, ref double MaxV, ref double[] Values, ref double[] VX, ref double[] VY, ref int Dim)
        {
            Dim = pole.Dimention;
            if (pole == null)
                return false;
            if (pole.Dimention == 1)
                Values = ((Field1D)pole).Values;
            else
            {
                Vector2[] val = ((Field2D)pole).Values;
                MEM.Alloc<double>(val.Length, ref Values);
                MEM.Alloc<double>(val.Length, ref VX);
                MEM.Alloc<double>(val.Length, ref VY);
                for (uint i = 0; i < Values.Length; i++)
                {
                    VX[i] = val[i].X;
                    VY[i] = val[i].Y;
                    Values[i] = val[i].Length();
                }
            }
            if (Values.Length > 0)
            {
                MaxV = Values.Max();
                MinV = Values.Min();
                    if (Math.Abs(MaxV - MinV) > MEM.Error9)
                        return true;
                    else
                        return false;
            }
            else
                return false;
        }
    }
}
