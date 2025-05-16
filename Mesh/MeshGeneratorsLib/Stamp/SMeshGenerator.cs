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
    using CommonLib.Function;
    using GeometryLib.Vector;
    using MeshLib;


    /// <summary>
    /// ОО: Фронтальный генератор КЭ трехузловой сетки для русла реки в 
    /// створе с заданной геометрией дна и уровнем свободной поверхности
    /// </summary>
    [Serializable]
    public class SMeshGenerator 
    {
        /// <summary>
        /// Опции для генерации Ленточной КЭ сетки 
        /// </summary>
        public CrossStripMeshOption Option { get; }
        /// <summary>
        /// Левая береговая точка
        /// </summary>
        protected HKnot left = null;
        /// <summary>
        /// Правая береговая точка
        /// </summary>
        protected HKnot right = null;
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
        //public TSpline spline = null;
        /// <summary>
        /// Ширина смоченного русла
        /// </summary>
        public double width = 0;
        /// <summary>
        /// Смоченный периметр
        /// </summary>
        public double WetBed = 0;

        protected double[] zb = null;
        protected double[] xb = null;

        protected double[] xx = null;
        protected double[] yy = null;

        public double[] bx = null;
        public double[] by = null;

        public double[] sx = null;
        public double[] sy = null;
        /// <summary>
        /// Профиль русла
        /// </summary>
        protected IDigFunction bed = null;
        /// <summary>
        /// Мокрое дно
        /// </summary>
        protected IDigFunction bedwet = null;
        /// <summary>
        /// Свободная поверхность
        /// </summary>
        protected double WaterLevel;

        protected int beginLeft;
        protected int beginRight;
        /// <summary>
        /// Узлов по мокрому дну
        /// </summary>
        protected int CountBed = 0;

        public SMeshGenerator() : this(new CrossStripMeshOption()) { }
        public SMeshGenerator(CrossStripMeshOption Option)
        {
            this.Option = Option;
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
        protected void LookingBoundary()
        {
            string name = "";
            bed.isParametric = true;
            bed.GetFunctionData(ref name, ref xx, ref yy);
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
        /// Расчет характеристик живого сечения створа
        /// </summary>
        /// <param name="WetBed">смоченный периметр</param>
        /// <param name="WaterLevel">отметка свободной поверхности реки</param>
        /// <param name="xx"></param>
        /// <param name="yy"></param>
        /// <param name="beginLeft"></param>
        /// <param name="beginRight"></param>
        protected void CreateBedWet()
        {
            List<HKnot> pointsBed = new List<HKnot>();
            int label = 1;
            var v0 = new HKnot(left.x, left.y, label);
            
            pointsBed.Add(v0);
            for (int i = beginLeft + 1; i < beginRight; i++)
                pointsBed.Add(new HKnot(xx[i], yy[i], label));
            
            var v1 = new HKnot(right.x, right.y, label);
            pointsBed.Add(v1);
            
            MEM.Alloc(pointsBed.Count, ref zb);
            MEM.Alloc(pointsBed.Count, ref xb);
            int kn = 0;
            foreach (var knot in pointsBed)
            {
                zb[kn] = knot.y; 
                xb[kn++] = knot.x;
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
            // Ширина смоченного русла
            width = right.x - left.x;
            //spline = new TSpline();
            //spline.Set(zb, xb);
            bedwet = new DigFunction(xb, zb, "Мокрое дно");
        }

 
        /// <summary>
        /// интерполяция дна
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public double Zeta(double arg)
        {
            return bedwet.FunctionValue(arg);
        }

        public virtual void CalkBedFunction(double WaterLevel, IDigFunction bed)
        {
            this.bed = bed;
            this.WaterLevel = WaterLevel;
            // Поиск береговых точек створа
            LookingBoundary();
            // Расчет характеристик живого сечения створа
            CreateBedWet();
            // шаг сетки по свободной поверхности
            // количество элементов
            CountBed = beginRight - beginLeft + 1;
        }

        /// <summary>
        /// Создает сетку слой вдоль дна живого сечения
        /// </summary>
        /// <param name="WetBed">смоченный периметр</param>
        /// <param name="WaterLevel">уровень свободной поверхности</param>
        /// <param name="xx">координаты дна по Х</param>
        /// <param name="yy">координаты дна по Y</param>
        /// <param name="Count">Количество узлов по дну</param>
        /// <returns>КЭ сетка</returns>
        public TriMesh CreateMeshTupeOld(double WaterLevel, IDigFunction bed, int CountBed)
        {
            this.CountBed = CountBed;
            CalkBedFunction(WaterLevel, bed);
            TriMesh mesh = new TriMesh();
            bedwet.isParametric = true;
            bedwet.Smoothness = SmoothnessFunction.linear;
            bedwet.GetFunctionData(ref bx, ref by, CountBed);
            // Создание штампа
            CreateStamp(bx, by, ref sx, ref sy);
            int CountKnots = sx.Length + bx.Length;
            int CountElems = 2 * sx.Length;
            int CountBoundElems = 2 * (sx.Length + 1);
            MEM.Alloc(CountKnots, ref mesh.CoordsX);
            MEM.Alloc(CountKnots, ref mesh.CoordsY);
            MEM.Alloc(CountElems, ref mesh.AreaElems);
            MEM.Alloc(CountBoundElems, ref mesh.BoundElems);
            MEM.Alloc(CountBoundElems, ref mesh.BoundElementsMark);
            MEM.Alloc(CountBoundElems, ref mesh.BoundKnots);
            MEM.Alloc(CountBoundElems, ref mesh.BoundKnotsMark);
            int[,] map = new int[bx.Length, 2];

            map[0, 0] = -1; map[bx.Length - 1, 0] = -1;
            map[0, 1] = 0; map[bx.Length - 1, 1] = CountKnots - 1;
            int k = 0;
            mesh.CoordsX[k] = bx[0];
            mesh.CoordsY[k++] = by[0];
            for (int i = 0; i < sx.Length; i++)
            {
                mesh.CoordsX[k] = bx[i + 1];
                mesh.CoordsY[k] = by[i + 1];
                map[i + 1, 0] = k++;
                mesh.CoordsX[k] = sx[i];
                mesh.CoordsY[k] = sy[i];
                map[i + 1, 1] = k++;
            }
            mesh.CoordsX[k] = bx[bx.Length - 1];
            mesh.CoordsY[k] = by[bx.Length - 1];
            LOG.Print("map", map);
            int elem = 0;
            int belem = 0;
            int knot = 0;
            mesh.AreaElems[elem++] = new TriElement(map[0, 1], map[1, 1], map[1, 0]);

            mesh.BoundElems[belem++] = new TwoElement(map[0, 1], map[1, 1]);
            mesh.BoundElems[belem++] = new TwoElement(map[0, 1], map[1, 0]);
            mesh.BoundKnots[knot++] = map[0, 1];
            for (int i = 1; i < sx.Length; i++)
            {
                mesh.AreaElems[elem++] = new TriElement(map[i + 1, 0], map[i, 0], map[i, 1]);
                mesh.AreaElems[elem++] = new TriElement(map[i + 1, 0], map[i, 1], map[i + 1, 1]);

                mesh.BoundElems[belem++] = new TwoElement(map[i, 0], map[i + 1, 0]);
                mesh.BoundElems[belem++] = new TwoElement(map[i, 1], map[i + 1, 1]);

                mesh.BoundKnots[knot++] = map[i, 0];
                mesh.BoundKnots[knot++] = map[i, 1];
            }
            int ie = bx.Length - 2;
            mesh.BoundElems[belem++] = new TwoElement(map[ie, 0], map[ie + 1, 1]);
            mesh.BoundElems[belem++] = new TwoElement(map[ie, 1], map[ie + 1, 1]);

            mesh.AreaElems[elem++] = new TriElement(map[ie, 0], map[ie, 1], map[ie + 1, 1]);
            mesh.BoundKnots[knot++] = map[ie + 1, 1];
            Console.WriteLine();
            for (int i = 0; i < mesh.AreaElems.Length; i++)
                Console.WriteLine(mesh.AreaElems[i].ToString());
            return mesh;
        }



        /// <summary>
        /// Создает сетку слой вдоль дна живого сечения
        /// </summary>
        /// <param name="WetBed">смоченный периметр</param>
        /// <param name="WaterLevel">уровень свободной поверхности</param>
        /// <param name="xx">координаты дна по Х</param>
        /// <param name="yy">координаты дна по Y</param>
        /// <param name="Count">Количество узлов по дну</param>
        /// <returns>КЭ сетка</returns>
        public TriMesh CreateMeshTupe(double WaterLevel, IDigFunction bed, int CountBed)
        {
            this.CountBed = CountBed;
            CalkBedFunction(WaterLevel, bed);
            TriMesh mesh = new TriMesh();
            bedwet.isParametric = true;
            bedwet.Smoothness = SmoothnessFunction.linear;
            int[,] map = null;
            bedwet.GetFunctionData(ref bx, ref by, CountBed);
            int CountElems, CountKnots;
            (CountElems, CountKnots) = CreateStamp(bx, by, ref map, ref sx, ref sy);
            //int CountKnots = sx.Length + bx.Length;
            //int CountElems = 2*sx.Length;
            int CountBoundElems = CountKnots;
            MEM.Alloc(CountKnots, ref mesh.CoordsX);
            MEM.Alloc(CountKnots, ref mesh.CoordsY);
            MEM.Alloc(CountElems, ref mesh.AreaElems);
            MEM.Alloc(CountBoundElems, ref mesh.BoundElems);
            MEM.Alloc(CountBoundElems, ref mesh.BoundElementsMark);
            MEM.Alloc(CountBoundElems, ref mesh.BoundKnots);
            MEM.Alloc(CountBoundElems, ref mesh.BoundKnotsMark);

            int k = 0;
            int si = 0;
            mesh.CoordsX[k] = bx[0];
            mesh.CoordsY[k++] = by[0];
            if (map[0, 0] != -1)
            {
                mesh.CoordsX[k] = sx[si];
                mesh.CoordsY[k++] = sy[si++];
            } 
            for (int i=1; i<bx.Length-1; i++)
            {
                mesh.CoordsX[k] = bx[i];
                mesh.CoordsY[k++] = by[i];
                mesh.CoordsX[k] = sx[si];
                mesh.CoordsY[k++] = sy[si++];
            }
            mesh.CoordsX[k] = bx[bx.Length - 1];
            mesh.CoordsY[k++] = by[bx.Length - 1];
            if (map[bx.Length - 1, 0] != -1)
            {
                mesh.CoordsX[k] = sx[si];
                mesh.CoordsY[k++] = sy[si++];
            }
          //  LOG.Print("map", map);
            int elem = 0;
            int belem = 0;
            int knot = 0;
            if (map[0, 0] != -1)
            {
                //mesh.AreaElems[elem++] = new TriElement(map[0, 1], map[1, 1], map[1, 0]);
                //mesh.AreaElems[elem++] = new TriElement(map[1, 1], map[1, 0], map[0, 0]);

                mesh.AreaElems[elem++] = new TriElement(map[1, 0], map[0, 0], map[0, 1]);
                mesh.AreaElems[elem++] = new TriElement(map[1, 0], map[0, 1], map[1, 1]);

                
                mesh.BoundElems[belem++] = new TwoElement(map[1, 0], map[0, 0]);
                mesh.BoundElems[belem++] = new TwoElement(map[0, 1], map[1, 1]);
                mesh.BoundElems[belem++] = new TwoElement(map[0, 1], map[0, 0]);

                mesh.BoundKnots[knot++] = map[0, 0];
            }
            else
            {
                mesh.AreaElems[elem++] = new TriElement(map[0, 1], map[1, 1], map[1, 0]);
                mesh.BoundElems[belem++] = new TwoElement(map[0, 1], map[1, 1]);
                mesh.BoundElems[belem++] = new TwoElement(map[0, 1], map[1, 0]);
            }
            mesh.BoundKnots[knot++] = map[0, 1];
            
            for (int i = 1; i < bx.Length-2; i++)
            {
                mesh.AreaElems[elem++] = new TriElement(map[i + 1, 0], map[i, 0], map[i, 1]);
                mesh.AreaElems[elem++] = new TriElement(map[i + 1, 0], map[i, 1], map[i + 1, 1]);

                mesh.BoundElems[belem++] = new TwoElement(map[i, 0], map[i + 1, 0]);
                mesh.BoundElems[belem++] = new TwoElement(map[i, 1], map[i + 1, 1]);
            }

            for (int i = 1; i < bx.Length - 1; i++)
            {
                mesh.BoundKnots[knot++] = map[i, 0];
                mesh.BoundKnots[knot++] = map[i, 1];
            }

            int ie = bx.Length - 2;

            if (map[bx.Length - 1, 0] != -1)
            {
                mesh.AreaElems[elem++] = new TriElement(map[ie + 1, 0], map[ie, 0], map[ie, 1]);
                mesh.AreaElems[elem++] = new TriElement(map[ie + 1, 0], map[ie, 1], map[ie + 1, 1]);

                //mesh.AreaElems[elem++] = new TriElement(map[ie, 0], map[ie, 1], map[ie + 1, 0]);
                //mesh.AreaElems[elem++] = new TriElement(map[ie, 1], map[ie+1, 1], map[ie + 1, 0]);

                mesh.BoundElems[belem++] = new TwoElement(map[ie, 1], map[ie + 1, 1]);
                mesh.BoundElems[belem++] = new TwoElement(map[ie + 1, 0], map[ie + 1, 1]);
                mesh.BoundElems[belem++] = new TwoElement(map[ie, 0], map[ie + 1, 0]);
                //mesh.BoundElems[belem++] = new TwoElement(map[ie, 0], map[ie + 1, 0]);
                mesh.BoundKnots[knot++] = map[bx.Length - 1, 0];
            }
            else
            {
                mesh.AreaElems[elem++] = new TriElement(map[ie, 0], map[ie, 1], map[ie + 1, 1]);
                mesh.BoundElems[belem++] = new TwoElement(map[ie, 0], map[ie + 1, 1]);
                mesh.BoundElems[belem++] = new TwoElement(map[ie, 1], map[ie + 1, 1]);
                
            }
            mesh.BoundKnots[knot++] = map[ie + 1, 1];




            //int ie = bx.Length - 2;

            //if (map[bx.Length - 1, 0] != -1)
            //{
            //    mesh.AreaElems[elem++] = new TriElement(map[ie + 1, 0], map[ie, 0], map[ie, 1]);
            //    mesh.AreaElems[elem++] = new TriElement(map[ie + 1, 0], map[ie, 1], map[ie + 1, 1]);

            //    //mesh.AreaElems[elem++] = new TriElement(map[ie, 0], map[ie, 1], map[ie + 1, 0]);
            //    //mesh.AreaElems[elem++] = new TriElement(map[ie, 1], map[ie+1, 1], map[ie + 1, 0]);
            //    mesh.BoundElems[belem++] = new TwoElement(map[ie, 1], map[ie + 1, 1]);
            //    mesh.BoundElems[belem++] = new TwoElement(map[ie + 1, 0], map[ie + 1, 1]);

            //    mesh.BoundKnots[knot++] = map[bx.Length - 1, 0];
            //}
            //else
            //{
            //    mesh.AreaElems[elem++] = new TriElement(map[ie, 0], map[ie, 1], map[ie + 1, 1]);
            //    mesh.BoundElems[belem++] = new TwoElement(map[ie, 1], map[ie + 1, 0]);
            //}
            //mesh.BoundElems[belem++] = new TwoElement(map[ie, 0], map[ie + 1, 0]);

            //mesh.BoundKnots[knot++] = map[ie + 1, 1];


            //Console.WriteLine();
            //for (int i = 0; i < mesh.AreaElems.Length; i++)
            //    Console.WriteLine(mesh.AreaElems[i].ToString());
            return mesh;
        }


        /// <summary>
        /// Создает сетку слой вдоль дна живого сечения и возвращает контур для генерации сетки в области
        /// </summary>
        /// <param name="WetBed">смоченный периметр</param>
        /// <param name="WaterLevel">уровень свободной поверхности</param>
        /// <param name="xx">координаты дна по Х</param>
        /// <param name="yy">координаты дна по Y</param>
        /// <param name="Count">Количество узлов по дну</param>
        /// <returns>КЭ сетка</returns>
        public TriMesh CreateMeshTupe(double WaterLevel, IDigFunction bed, int CountBed, ref HKnot[] Contur)
        {
            TriMesh mesh = CreateMeshTupe(WaterLevel, bed, CountBed);
            MEM.Alloc(2 * sx.Length - 2,ref Contur);
            for (int i = 0; i < sx.Length; i++)
                Contur[i]=new HKnot(sx[i], sy[i]);
            int e = sx.Length;
            int marker = 2;
            for (int i = 0; i < sx.Length-2; i++)
                Contur[e+i] = new HKnot(sx[e-i-2], WaterLevel, marker);
            return mesh;
        }

        public static void CreateStamp(double[] bx, double[] by, ref double[] ax, ref double[] ay)
        {
            double ds = bx[1] - bx[0];
            Vector2[] tauL = new Vector2[bx.Length - 1];
            Vector2[] normaL = new Vector2[bx.Length - 1];
            Vector2[] p = new Vector2[bx.Length - 2];
            double b = 1;
            for (int i = 0; i < tauL.Length - 1; i++)
            {
                tauL[i] = new Vector2(bx[i + 1] - bx[i], by[i + 1] - by[i]);
                normaL[i] = tauL[i].GetOrtogonalLeft();
                if (i == 0 || i == tauL.Length - 2)
                {
                    b = Math.Abs(tauL[i].Y / (Math.Abs(normaL[i].Y) + MEM.Error14));
                }
                normaL[i] *= b;
                p[i] = new Vector2(bx[i + 1], by[i + 1]) + normaL[i];
            }
            MEM.Alloc(p.Length, ref ax);
            MEM.Alloc(p.Length, ref ay);
            for (int i = 0; i < p.Length; i++)
            {
                ax[i] = p[i].X;
                ay[i] = p[i].Y;
            }
        }


        public static (int,int) CreateStamp(double[] bx, double[] by, ref int[,] map, ref double[] ax, ref double[] ay)
        {
            // Создание штампа
            map = new int[bx.Length, 2];
            double ds = Math.Sqrt( (bx[1] - bx[0])* (bx[1] - bx[0]) + (by[1] - by[0]) * (by[1] - by[0]));
            Vector2[] tauL = new Vector2[bx.Length - 1];
            Vector2[] normaL = new Vector2[bx.Length - 1];
            Vector2[] p = new Vector2[bx.Length];
            double b = 1;
            bool FL = false;
            bool FR = false;
            int ei = tauL.Length - 2;
            for (int i = 0; i < tauL.Length-1; i++)
            {
                if(i == tauL.Length - 2)
                    tauL[i] = new Vector2(bx[ei + 1] - bx[ei], by[ei + 1] - by[ei]);
                else
                    tauL[i] = new Vector2(0.5*(bx[i + 2] - bx[i]), 0.5 *(by[i + 2] - by[i]));
                normaL[i] = tauL[i].GetOrtogonalLeft();
                
                if (i == 0)
                {
                    b = Math.Abs(tauL[i].Y / (Math.Abs(normaL[i].Y) + MEM.Error14));
                    if (b > 1.2)
                    {
                        FL = true;
                        p[0] = new Vector2(bx[0] + ds, by[0]);
                        b = 1;
                        //normaL[i] *= b;
                        p[1] = new Vector2(bx[i + 1], by[i + 1]) + normaL[i];
                    }
                    else
                    {
                        normaL[i] *= b;
                        p[1] = new Vector2(bx[i + 1], by[i + 1]) + normaL[i];
                    }
                }
                else
                if (i == ei)
                {
                    b = Math.Abs(tauL[i].Y / (Math.Abs(normaL[i].Y) + MEM.Error14));
                    if (b > 1.2)
                    {
                        FR = true;
                        p[ei + 2] = new Vector2(bx[ei + 1] - 0.8*ds, by[0]);
                        b = 1;
                        //normaL[i] *= b;
                        //p[ei+1] = new Vector2(bx[i + 1], by[i + 1]) + normaL[i];
                        p[ei + 1] = 0.5 * (p[ei + 2] + p[ei]);
                    }
                    else
                    {
                        normaL[i] *= b;
                        p[ei + 1] = new Vector2(bx[i + 1], by[i + 1]) + normaL[i];
                        p[ei + 1].Y = by[0];
                    }
                }
                else
                {
                    normaL[i] *= b;
                    p[i+1] = new Vector2(bx[i + 1], by[i + 1]) + normaL[i];
                }
            }
            
            int CountKnots = 0;
            int CountElems = 0;
            if (FL == true)
            {
                map[0, 0] = CountKnots++;
                map[0, 1] = CountKnots++;
                CountElems += 2;
            }
            else
            {
                map[0, 0] = -1;
                map[0, 1] = CountKnots++;
                CountElems += 1;
            }
            for (int i = 1; i < bx.Length - 1; i++)
            {
                map[i, 0] = CountKnots++;
                map[i, 1] = CountKnots++;
                CountElems += 2;
            }
            if (FR == true)
            {
                map[bx.Length - 1, 0] = CountKnots++;
                map[bx.Length - 1, 1] = CountKnots++;
                CountElems += 2;
            }
            else
            {
                map[bx.Length - 1, 0] = -1;
                map[bx.Length - 1, 1] = CountKnots++;
                CountElems += 1;
            }
            int Count = p.Length - 2;
            if (FL == true) Count++;
            if (FR == true) Count++;
            MEM.Alloc(Count, ref ax);
            MEM.Alloc(Count, ref ay);
            int idx = 0;
            if (FL == true)
            {
                ax[idx] = p[0].X;
                ay[idx++] = p[0].Y;
            }
            for (int i = 1; i < p.Length-1; i++)
            {
                ax[idx] = p[i].X;
                ay[idx++] = p[i].Y;
            }
            if (FR == true)
            {
                ax[idx] = p[p.Length - 1].X;
                ay[idx++] = p[p.Length - 1].Y;
            }
            return (CountElems, CountKnots);
        }

    }
}
