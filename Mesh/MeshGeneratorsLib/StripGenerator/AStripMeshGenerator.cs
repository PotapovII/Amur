//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 03.03.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshGeneratorsLib.StripGenerator
{
    using System;
    using System.Collections.Generic;

    using CommonLib;
    using MemLogLib;
    using GeometryLib;
    /// <summary>
    /// ОО: Фронтальный генератор КЭ трехузловой сетки для русла реки в 
    /// створе с заданной геометрией дна и уровнем свободной поверхности
    /// </summary>
    [Serializable]
    public abstract class AStripMeshGenerator : IStripMeshGenerator
    {
        /// <summary>
        /// Опции для генерации Ленточной КЭ сетки 
        /// </summary>
        public CrossStripMeshOption Option { get; }
        /// <summary>
        /// Левая береговая точка
        /// </summary>
        public HKnot Left() => left;
        protected HKnot left = null;
        /// <summary>
        /// Правая береговая точка
        /// </summary>
        public HKnot Right() => right;
        protected HKnot right = null;
        /// <summary>
        /// флаг сухого левой берега
        /// </summary>
        public bool DryLeft() => dryLeft;
        /// <summary>
        /// флаг сухого правого берега
        /// </summary>
        public bool DryRight() => dryRight;
        /// <summary>
        /// флаг левой берега
        /// </summary>
        protected bool dryLeft = true;
        /// <summary>
        /// флаг правого берега
        /// </summary>
        protected bool dryRight = true;
        /// <summary>
        /// Кривая донной поверхности
        /// </summary>
        public TSpline spline = null;
        /// <summary>
        /// Ширина смоченного русла
        /// </summary>
        protected double width = 0;

        protected double[] zb = null;
        protected double[] xb = null;

        protected int beginLeft;
        protected int beginRight;
        protected int CountBed = 0;

        public AStripMeshGenerator(CrossStripMeshOption Option)
        {
            this.Option = Option;
        }
        
        public virtual void CalkBedFunction(ref double WetBed, 
                    double WaterLevel, double[] xx, double[] yy)
        {
            // Поиск береговых точек створа
            LookingBoundary(WaterLevel, xx, yy, out beginLeft, out beginRight);
            // Расчет характеристик живого сечения створа
            CreateBedWet(ref WetBed, WaterLevel, xx, yy, beginLeft, beginRight);
            // шаг сетки по свободной поверхности
            // количество элементов
            CountBed = beginRight - beginLeft + 1;
        }
        /// <summary>
        /// Расчет характеристик живого сечения створа
        /// </summary>
        /// <param name="WetBed">смоченный периметр</param>
        /// <param name="WaterLevel">отметка свободной поверхности реки</param>
        /// <param name="xx"></param>
        /// <param name="yy"></param>
        /// <param name="beginLeft"></param>
        /// <param name="beginRight"></param>
        protected void CreateBedWet(ref double WetBed, double WaterLevel, 
                    double[] xx, double[] yy, int beginLeft, int beginRight)
        {
            List<HKnot> pointsBed = new List<HKnot>();
            int label = 1;
            var v0 = new HKnot(left.x, left.y, label);
            pointsBed.Add(v0);
            for (int i = beginLeft + 1; i < beginRight; i++)
            {
                pointsBed.Add(new HKnot(xx[i], yy[i], label));
            }
            if(beginRight==1)
            {
                var v1 = new HKnot(right.x, right.y, label);
                pointsBed.Add(v1);
            }
            MEM.Alloc(pointsBed.Count, ref zb);
            MEM.Alloc(pointsBed.Count, ref xb);
            int kn = 0;
            foreach (var knot in pointsBed)
            {
                zb[kn] = knot.y; xb[kn++] = knot.x;
            }
            WetBed = 0;
            double dxi, dzi;
            for (int i = 1; i < xb.Length; i++)
            {
                dxi = xb[i] - xb[i - 1];
                dzi = zb[i] - zb[i - 1];
                WetBed += Math.Sqrt(dxi * dxi + dzi * dzi);
            }

            if (dryLeft == false)
                WetBed += WaterLevel - left.y;
            if (dryRight == false)
                WetBed += WaterLevel - right.y;

            //if (dryLeft == false || dryRight == false)
            //{
            //    double zbMin = zb.Min();
            //    double HH = WaterLevel - zbMin;
            //    if (dryLeft == false)
            //        WetBed += HH;
            //    if (dryRight == false)
            //        WetBed += HH;
            //}

            // Ширина смоченного русла
            width = right.x - left.x;
            //if (dryLeft == true)
            //    if (dryRight == true)
            //        width = right.x - left.x;
            //    else
            //        width = xx[xx.Length - 1] - left.x;
            //else
            //    if (dryRight == true)
            //        width = right.x - xx[0];
            //    else
            //        width = xx[xx.Length - 1] - xx[0];
            spline = new TSpline();
            spline.Set(zb, xb);
        }

        /// <summary>
        /// Поиск береговых точек створа
        /// </summary>
        /// <param name="WaterLevel"></param>
        /// <param name="xx"></param>
        /// <param name="yy"></param>
        /// <param name="N"></param>
        /// <param name="beginLeft"></param>
        /// <param name="beginRight"></param>
        protected void LookingBoundary(double WaterLevel, double[] xx, double[] yy, 
                                            out int beginLeft, out int beginRight)
        {
            int N = xx.Length - 1;
            if (yy[0] < WaterLevel) // левый берег затоплен
            {
                left = new HKnot(xx[0], yy[0], 0);
                //left = new HKnot(xx[0], WaterLevel, 0);
                dryLeft = false;
                beginLeft = 0;
            }
            else // левый берег сухой
            {
                beginLeft = 0;
                //-- нахождение "левой" точки берега-------------
                for (int i = 0; i < N; i++)
                {
                    if (((yy[i] > WaterLevel) && (yy[i + 1] <= WaterLevel)))// левый берег
                    {
                        double newX = (WaterLevel - yy[i]) / (yy[i + 1] - yy[i]) * (xx[i + 1] - xx[i]) + xx[i];
                        left = new HKnot(newX, WaterLevel, i);
                        beginLeft = i;
                        dryLeft = true;
                        break;
                    }
                }
            }
            if (yy[N] < WaterLevel) // правый берег затоплен
            {
                //right = new HKnot(xx[N], yy[N], -1);
                right = new HKnot(xx[N], WaterLevel, -1);
                dryRight = false;
                beginRight = N;
            }
            else // правый берег сухой
            {
                beginRight = N;
                //-- нахождение "правой" точки берега-------------
                for (int i = N; i > 0; i--)
                {
                    if (((yy[i] > WaterLevel) && (yy[i - 1] <= WaterLevel)))// правый берег
                    {
                        double newX = (WaterLevel - yy[i - 1]) / (yy[i] - yy[i - 1]) * (xx[i] - xx[i - 1]) + xx[i - 1];
                        right = new HKnot(newX, WaterLevel, i);
                        dryRight = true;
                        beginRight = i;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// интерполяция дна
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public double Zeta(double arg)
        {
            return spline.Value(arg);
        }
        /// <summary>
        /// Создает сетку в области живого сечения
        /// </summary>
        /// <param name="WetBed">смоченный периметр</param>
        /// <param name="WaterLevel">уровень свободной поверхности</param>
        /// <param name="xx">координаты дна по Х</param>
        /// <param name="yy">координаты дна по Y</param>
        /// <param name="Count">Количество узлов по дну</param>
        /// <returns>КЭ сетка</returns>
        public abstract IMesh CreateMesh(ref double GR, ref int[][] riverGates,
                    double WaterLevel, double[] xx, double[] yy, int Count = 0);
    }

}
