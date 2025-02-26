using GeometryLib.Vector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DelaunayGenerator
{
    internal class Class1
    {
        public bool pt_in_polygon2(Vector2 test, List<Vector2> polygon)
        {
            int[][] q_patt = { new int[2] { 0, 1 }, new int[2] { 3, 2 } };
            if (polygon.Count < 3) return false;
            Vector2 pred_pt = polygon[polygon.Count-1];
            pred_pt.X -= test.X;
            pred_pt.Y -= test.Y;
            int pred_q = q_patt[pred_pt.Y < 0 ? 1 : 0][pred_pt.X < 0 ? 1 : 0];
            int w = 0;
            for (int i = 0; i < polygon.Count; i++)
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
    }
}
