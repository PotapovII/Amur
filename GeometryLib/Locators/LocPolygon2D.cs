namespace TestsUtils
{
    using CommonLib.Areas;
    using CommonLib.Geometry;
    using GeometryLib.Areas;
    using GeometryLib.Locators;
    using GeometryLib.Vector;
    using System;
    using System.Collections.Generic;

    public static class LocPolygon2D
    {
        /// <summary>
        /// Локатор для точки в полигоне
        /// </summary>
        /// <param name="test">точка</param>
        /// <param name="polygon">массив точек полигона</param>
        /// <returns>истина - находится в полигоне</returns>
        public static bool Contains(IHPoint test, Vector2[] polygon)
        {
            int[][] q_patt = { new int[2] { 0, 1 }, new int[2] { 3, 2 } };
            if (polygon.Length < 3) 
                return false;
            Vector2 pred_pt = polygon[polygon.Length-1];
            pred_pt.X -= test.X;
            pred_pt.Y -= test.Y;
            int pred_q = q_patt[pred_pt.Y < 0 ? 1 : 0][pred_pt.X < 0 ? 1 : 0];
            int w = 0;
            for (int i = 0; i < polygon.Length; i++)
            {
                Vector2 cur_pt = polygon[i];
                cur_pt.X -= test.X;
                cur_pt.Y -= test.Y;
                int q = q_patt[cur_pt.Y < 0 ? 1 : 0][cur_pt.X < 0 ? 1 : 0];
                switch (q - pred_q)
                {
                    case -3: ++w; break;
                    case 3:  --w; break;
                    case -2:
                        if (pred_pt.X * cur_pt.Y >= pred_pt.Y * cur_pt.X) ++w;
                        break;
                    case 2:
                        if (!(pred_pt.X * cur_pt.Y >= pred_pt.Y * cur_pt.X)) --w;
                        break;
                }
                pred_pt = cur_pt;
                pred_q = q;
            }
            return w != 0;
        }

        /// <summary>
        /// Локатор для точки в полигоне
        /// </summary>
        /// <param name="test">точка</param>
        /// <param name="polygon">массив точек полигона</param>
        /// <returns>истина - находится в полигоне</returns>
        /// <param name="p_out">точка лежащая за контуром</param>
        /// <returns></returns>
        public static bool Contains(HPoint p, IHPoint[] polygon, HPoint p_out)
        {
            int crossCount = 0;
            for (int k = 0; k < polygon.Length; k++)
            {
                if (CrossLineUtils.IsCrossing(
                    (HPoint)polygon[k],
                    (HPoint)polygon[(k + 1) % polygon.Length],
                     p_out, p) == true)
                {
                    crossCount++;
                }
            }
            return crossCount % 2 == 1;
        }
        /// <summary>
        /// Локатор для точки в полигоне
        /// </summary>
        /// <param name="test">точка</param>
        /// <param name="polygon">массив точек полигона</param>
        /// <returns>истина - находится в полигоне</returns>
        /// <param name="p_out">точка лежащая за контуром</param>
        /// <returns></returns>
        public static bool Contains(IHPoint p, IHPoint p_out, List<IMPoint> polygon)
        {
            int crossCount = 0;
            for (int k = 0; k < polygon.Count; k++)
            {
                if (CrossLineUtils.IsCrossing(
                    polygon[k],
                    polygon[(k + 1) % polygon.Count],
                    p_out, p) == true)
                {
                    crossCount++;
                }
            }
            return crossCount % 2 == 1;
        }


        public static void TestsUtils()
        {
            Vector2[] ps = new Vector2[5];
            ps[0] = new Vector2(0, 0);
            ps[1] = new Vector2(4, 0);
            ps[2] = new Vector2(2, 2);
            ps[3] = new Vector2(4, 4);
            ps[4] = new Vector2(0, 4);
            Vector2 p = new Vector2(3.1, 4.5);
            if (LocPolygon2D.Contains(p, ps) == true)
                Console.WriteLine("Ok");
            else
                Console.WriteLine("No");
            p = new Vector2(0.5, 0.5);
            if (LocPolygon2D.Contains(p, ps) == true)
                Console.WriteLine("Ok");
            else
                Console.WriteLine("No");
            p = new Vector2(4.1, 3.9);
            if (LocPolygon2D.Contains(p, ps) == true)
                Console.WriteLine("Ok");
            else
                Console.WriteLine("No");
            p = new Vector2(-0.0000001, 3);
            if (LocPolygon2D.Contains(p, ps) == true)
                Console.WriteLine("Ok");
            else
                Console.WriteLine("No");
            Console.Read();
        }
    }
}
