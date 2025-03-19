//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 11.12.2020 Потапов И.И.
//---------------------------------------------------------------------------
//                  15 06 2024: заливка градиентной шкалы 
//---------------------------------------------------------------------------
namespace RenderLib
{
    using CommonLib;
    using MemLogLib;
    using GeometryLib;
    using MeshLib;
    using System;
    using System.Drawing;
    using System.Linq;
    using GeometryLib.Vector;
    using GeometryLib.World;
    using CommonLib.DrvGraphics;
    using CommonLib.Geometry;

    /// <summary>
    /// ОО: Класс методов для отрисовки данных
    /// </summary>
    public class TaskRendererFields : TaskRenderer
    {
        double[] Values = null;
        double MaxV = 0;
        double MinV = 0;
        double minV, maxV;
        double SumV = 0;
        double[] ValuesX = null;
        double[] ValuesY = null;
        FPoint[] VeLLines = null;
        float[] LengthLines = null;
        /// <summary>
        /// Флаги объектов отрисовки
        /// </summary>
        public RenderOptionsFields renderOptions = new RenderOptionsFields();
        /// <summary>
        /// Цветовая схема отрисовки
        /// </summary>
        public ColorSchemeFields colorScheme = new ColorSchemeFields();
        /// <summary>
        /// Данные
        /// </summary>
        SavePointData data;
        /// <summary>
        /// Сетка для отрисовки геометрии области и полей
        /// </summary>
        IRenderMesh mesh;
        /// <summary>
        /// одномерная сетка задачи в точке сохранения
        /// </summary>
        GraphicsData graphicsData;
        /// <summary>
        /// буфера для координат сетки
        /// </summary>
        double[] X;
        double[] Y;
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref = "TaskRendererFields" />.
        /// </summary>
        public TaskRendererFields(SavePointData data)
        {
            if (data == null) return;
            this.data = data;
            this.mesh = (IMesh)data.mesh;
            this.graphicsData = data.graphicsData;
            if (mesh != null)
            {
                X = mesh.GetCoords(0);
                Y = mesh.GetCoords(1);
            }
        }
        /// <summary>
        /// Визуализирует данные (сетку и поля)
        /// !!! Должно управлять визуализацией
        /// </summary>
        public void Render(Graphics g, WorldScaler zoom)
        {
            try
            {
                this.zoom = zoom;
                // заливка
                if (renderOptions.opFillValues == true)
                    NativeRenderFillField(g);
                // сетка (элементы)
                if (renderOptions.showMesh == true)
                    RenderTriangles(g);
                // граница - элементы
                if (renderOptions.showBoudary == true)
                    RenderBoundary(g);
                // граница - узлы и флаги
                if (renderOptions.showBoudaryKnots == true)
                    RenderBoundaryKnots(g);
                // граница - узлы и флаги
                if (renderOptions.showBoudaryElems == true)
                    RenderBoundaryElements(g);
                // узлы
                if (renderOptions.showKnotNamber == true)
                    RenderPoints(g);
                // числовые величины
                if (renderOptions.opValuesKnot == true)
                    RenderValuePoints(g);
                // изолинии
                if (renderOptions.opIsoLineValues == true)
                    RenderIsoLine(g);
                // изолиния 0
                if (renderOptions.opIsoLineValues0 == true)
                    RenderIsoLine(g, true);
                // изолиния с выбранным значением
                if (renderOptions.opIsoLineSelect == true)
                    RenderIsoLine(g, true, renderOptions.opIsoLineSelectValue);
                // векторые поля
                if (renderOptions.opVectorValues == true)
                    RenderVectorField(g);
                // Отрисовка функций 
                if (renderOptions.opGraphicCurve == true)
                    RenderCurve(g);
                // шкала для заливки
                if (renderOptions.opGradScale == true)
                    GradientScale(g);
                // отрисовка створа
                if (renderOptions.opTargetLine == true)
                    RenderTargetLine(g);
                // Отрисовка координатных осей
                if (renderOptions.coordReper == true)
                    RenderCoordReper(g, colorScheme);
            }
            catch (Exception exc)
            {
                //Logger.Instance.Info(exc.Message);
                    Logger.Instance.Info(exc.Message + "TaskRendererFields Render");
            }
        }
        /// <summary>
        /// отрисовка створа
        /// </summary>
        /// <param name="g"></param>
        public void RenderTargetLine(Graphics g)
        {
            PointF pa = new PointF((float)renderOptions.a.X, (float)renderOptions.a.Y);
            PointF pb = new PointF((float)renderOptions.b.X, (float)renderOptions.b.Y);
            if (pa != pb)
            {
                zoom.WorldToScreen(ref pa);
                zoom.WorldToScreen(ref pb);
                g.FillEllipse(colorScheme.BrushPoint, pa.X - 1.5f, pa.Y - 1.5f, 3, 3);
                g.FillEllipse(colorScheme.BrushPoint, pb.X - 1.5f, pb.Y - 1.5f, 3, 3);
                g.DrawLine(colorScheme.PenBoundaryLine, pa, pb);
            }
        }
        /// <summary>
        /// Отрисовка функций 
        /// </summary>
        /// <param name="g"></param>
        private void RenderCurve(Graphics g)
        {
            if (graphicsData == null)
                return;
            foreach (var curve in graphicsData.curves)
            {
                if (curve.Check == false)
                    continue;
                PointF p0, p1;
                for (int i = 0; i < curve.Count - 1; i++)
                {
                    p0 = new PointF((float)curve[i].x, (float)curve[i].y);
                    p1 = new PointF((float)curve[i + 1].x, (float)curve[i + 1].y);
                    zoom.WorldToScreen(ref p0);
                    zoom.WorldToScreen(ref p1);
                    g.DrawLine(colorScheme.PenGraphLine, p0, p1);
                }
            }
        }
        /// <summary>
        /// Отрисовка узлов сетки и ее границы
        /// </summary>
        /// <param name="g"></param>
        private void RenderPoints(Graphics g)
        {
            if (data.mesh == null) return;
            int i;
            PointF pt;
            // Нарисуйте точки в области
            for (i = 0; i < X.Length; i++)
            {
                
                pt = new PointF((float)X[i], (float)Y[i]);
                zoom.WorldToScreen(ref pt);
                g.FillEllipse(colorScheme.BrushPoint, pt.X - 1.5f, pt.Y - 1.5f, 3, 3);
                if (renderOptions.showKnotNamber == true)
                {
                    PointF pp = new PointF(pt.X + 10f, pt.Y);
                    g.DrawString(i.ToString(), colorScheme.FontKnot, colorScheme.BrushTextKnot, pp);
                }
            }
            int[] BoundKnots = data.mesh.GetBoundKnots();
            // Нарисуйте точки границы
            for (i = 0; i < data.mesh.CountBoundKnots; i++)
            {
                int k = BoundKnots[i];
                pt = new PointF((float)X[k], (float)Y[k]);
                zoom.WorldToScreen(ref pt);
                g.FillEllipse(colorScheme.BrushBoundaryPoint, pt.X - 1.5f, pt.Y - 1.5f, 3, 3);
            }
        }
        /// <summary>
        /// Отрисовка полей в узлах сетки
        /// </summary>
        /// <param name="g"></param>
        private void RenderValuePoints(Graphics g)
        {
            // Индекс поля
            if (data.mesh == null) return;
            uint indexPole = (uint)renderOptions.indexValues;
            // Поле
            IField pole = data.GetPole(indexPole);
            if (pole == null)
                return;
            int i;
            PointF pt;
            string res;
            if (pole.Dimention == 1)
            {
                ValuesX = ((Field1D)pole).Values;
                for (i = 0; i < X.Length; i++)
                {
                    pt = new PointF((float)X[i], (float)Y[i]);
                    zoom.WorldToScreen(ref pt);
                    pt.X += 10f; pt.Y = pt.Y - colorScheme.FontValue.Size - 3;
                    res = ": " + String.Format(colorScheme.FormatText, renderOptions.ScaleValue(ValuesX[i]));
                    g.DrawString(res, colorScheme.FontValue, colorScheme.BrushTextValues, pt);
                }
            }
            else
            {
                //pole.GetValue(ref ValuesX, ref ValuesY);
                Vector2[] val = ((Field2D)pole).Values;
                MEM.Alloc(val.Length, ref ValuesX);
                MEM.Alloc(val.Length, ref ValuesY);
                for (i = 0; i < X.Length; i++)
                {
                    ValuesX[i] = val[i].X;
                    ValuesY[i] = val[i].Y;
                }
                for (i = 0; i < X.Length; i++)
                {
                    pt = new PointF((float)X[i], (float)Y[i]);
                    zoom.WorldToScreen(ref pt);
                    pt.X += 10f; pt.Y = pt.Y - colorScheme.FontValue.Size - 3;
                    res = "> " + String.Format(colorScheme.FormatText, renderOptions.ScaleValue(ValuesX[i]));
                    g.DrawString(res, colorScheme.FontValue, colorScheme.BrushTextValues, pt);
                    pt.Y = pt.Y - colorScheme.FontValue.Size - 2;
                    res = "^ " + String.Format(colorScheme.FormatText, renderOptions.ScaleValue(ValuesY[i]));
                    g.DrawString(res, colorScheme.FontValue, colorScheme.BrushTextValues, pt);
                }
            }
        }
        /// <summary>
        /// Рисуем треугольники/линии КЭ (с заливкой и без нее)
        /// </summary>
        /// <param name="g"></param>
        /// <param name="fillTriangles"></param>
        private void RenderTriangles(Graphics g)
        {
            if (data.mesh == null) return;
            TwoMesh mesh2 = data.mesh as TwoMesh;
            if (data.mesh as TwoMesh != null)
            {
                TwoElement[] AreaElems = mesh2.AreaElems;
                if (AreaElems == null) return;
                TwoElement el;
                PointF p0, p1;

                for (int i = 0; i < AreaElems.Length; i++)
                {
                    el = AreaElems[i];

                    p0 = new PointF((float)X[el.Vertex1], (float)Y[el.Vertex1]);
                    p1 = new PointF((float)X[el.Vertex2], (float)Y[el.Vertex2]);

                    zoom.WorldToScreen(ref p0);
                    zoom.WorldToScreen(ref p1);

                    PointF[] line = { p0, p1 };

                    g.DrawPolygon(colorScheme.PenMeshLine, line);
                    // 
                    if (renderOptions.showElementNamber == true)
                    {
                        float cx = (p0.X + p1.X) / 2 - 5;
                        float cy = (p0.Y + p1.Y) / 2 - 15;
                        g.DrawString(i.ToString(), colorScheme.FontValue, colorScheme.BrushPoint, new PointF(cx, cy));
                    }
                }
            }
            if (MeshIsNull() == false)
            {
                TriElement[] AreaElems = data.mesh.GetAreaElems();
                if (AreaElems == null) return;
                TriElement el;
                PointF p0, p1, p2;
                for (int i = 0; i < AreaElems.Length; i++)
                {
                    el = AreaElems[i];

                    p0 = new PointF((float)X[el.Vertex1], (float)Y[el.Vertex1]);
                    p1 = new PointF((float)X[el.Vertex2], (float)Y[el.Vertex2]);
                    p2 = new PointF((float)X[el.Vertex3], (float)Y[el.Vertex3]);

                    zoom.WorldToScreen(ref p0);
                    zoom.WorldToScreen(ref p1);
                    zoom.WorldToScreen(ref p2);

                    PointF[] tri = { p0, p1, p2 };

                    g.DrawPolygon(colorScheme.PenMeshLine, tri);
                    // номер КЭ
                    if (renderOptions.showElementNamber == true)
                    {
                        float cx = (p0.X + p1.X + p2.X) / 3 - 5;
                        float cy = (p0.Y + p1.Y + p2.Y) / 3 - 5;
                        g.DrawString(i.ToString(), colorScheme.FontValue, colorScheme.BrushPoint, new PointF(cx, cy));
                    }
                }
            }
        }
        /// <summary>
        /// Отрисовка границы области
        /// </summary>
        /// <param name="g"></param>
        private void RenderBoundary(Graphics g)
        {
            if (MeshIsNull() == false)
            {
                TwoElement[] Elems = mesh.GetBoundElems();
                if (Elems == null) return;
                TwoElement el;
                PointF p0, p1, pc;
                for (uint i = 0; i < Elems.Length; i++)
                {
                    el = Elems[i];
                    p0 = new PointF((float)X[el.Vertex1], (float)Y[el.Vertex1]);
                    p1 = new PointF((float)X[el.Vertex2], (float)Y[el.Vertex2]);
                    pc = new PointF(0.5f * (p0.X + p1.X), 0.5f * (p0.Y + p1.Y));
                    zoom.WorldToScreen(ref p0);
                    zoom.WorldToScreen(ref p1);
                    g.DrawLine(colorScheme.PenBoundaryLine, p0, p1);

                    if (renderOptions.showKnotNamber == true)
                    {
                        zoom.WorldToScreen(ref pc); pc.Y -= 5;
                        int flag = mesh.GetBoundElementMarker(i);
                        string ss = flag.ToString() + " : " + i.ToString();
                        g.DrawString(ss, colorScheme.FontKnot, colorScheme.BrushTextKnot, pc);
                    }
                }
            }
        }
        /// <summary>
        /// Отрисовка граничных узлов области
        /// </summary>
        /// <param name="g"></param>
        private void RenderBoundaryKnots(Graphics g)
        {
            if (MeshIsNull() == false)
            {
                int[] bk = data.mesh.GetBoundKnots();
                int[] fbk = ((IMesh)data.mesh).GetBoundKnotsMark();

                PointF p0;
                for (int i = 0; i < bk.Length; i++)
                {
                    p0 = new PointF((float)X[bk[i]], (float)Y[bk[i]]);
                    zoom.WorldToScreen(ref p0);
                    g.FillEllipse(colorScheme.BrushPoint, p0.X - 1.5f, p0.Y - 1.5f, 3, 3);
                    //if (renderOptions.showKnotNamber == true)
                    {
                        PointF pp = new PointF(p0.X - 20f, p0.Y);
                        g.DrawString(fbk[i].ToString(), colorScheme.FontKnot, colorScheme.BrushTextKnot, pp);
                        pp = new PointF(p0.X + 10f, p0.Y);
                        g.DrawString(bk[i].ToString(), colorScheme.FontKnot, colorScheme.BrushTextKnot, pp);
                    }
                }
            }
        }

        /// <summary>
        /// Отрисовка граничных узлов области
        /// </summary>
        /// <param name="g"></param>
        private void RenderBoundaryElements(Graphics g)
        {
            if (MeshIsNull() == false)
            {
                var elems = data.mesh.GetBoundElems();
                int[] mark = data.mesh.GetBElementsBCMark();
                PointF pa;
                PointF pb;
                for (int i = 0; i < elems.Length; i++)
                {
                    var i1 = elems[i].Vertex1;
                    var i2 = elems[i].Vertex2;
                    pa = new PointF((float)X[i1], (float)Y[i1]);
                    pb = new PointF((float)X[i2], (float)Y[i2]);
                    zoom.WorldToScreen(ref pa);
                    zoom.WorldToScreen(ref pb);
                    g.FillEllipse(colorScheme.BrushPoint, pa.X - 1.5f, pa.Y - 1.5f, 3, 3);
                    g.FillEllipse(colorScheme.BrushPoint, pb.X - 1.5f, pb.Y - 1.5f, 3, 3);

                    PointF pc = new PointF(0.5f * (pa.X + pb.X), 0.5f * (pa.Y + pb.Y));
                    //if (renderOptions.showKnotNamber == true)
                    {
                        PointF pp = new PointF(pc.X - 20f, pc.Y);
                        g.DrawString(mark[i].ToString(), colorScheme.FontKnot, colorScheme.BrushTextKnot, pp);
                        pp = new PointF(pc.X + 10f, pc.Y);
                    }
                    if (renderOptions.showElementNamber == true)
                    {
                        PointF pp = new PointF(pc.X + 30f, pc.Y);
                        g.DrawString(i.ToString(), colorScheme.FontKnot, colorScheme.BrushTextKnot, pp);
                        PointF p1 = new PointF(pc.X + 30f, pc.Y-10);
                        g.DrawString(i1.ToString(), colorScheme.FontKnot, colorScheme.BrushTextKnot, p1);
                        PointF p2 = new PointF(pc.X + 30f, pc.Y+10);
                        g.DrawString(i2.ToString(), colorScheme.FontKnot, colorScheme.BrushTextKnot, p2);
                    }
                }
            }
        }

        private bool MeshIsNull()
        {
            mesh = (IMesh)data.mesh;
            if (data.mesh as TwoMesh != null)
                return true;
            else
                return false;
        }


        /// <summary>
        /// Отрисовка изолиний
        /// </summary>
        private void RenderIsoLine(Graphics g, bool flagOne = false, float ValueISO = 0)
        {
            if (MeshIsNull()) return;
            // Индекс поля
            int Dim = 0;
            int indexPole = renderOptions.indexValues;
            double MaxV0 = 0;
            double MinV0 = 0;
            double AreaV = 0;
            if (data.GetPoleMinMax(indexPole, ref MinV0, ref MaxV0, ref SumV, ref AreaV, ref Values, ref ValuesX, ref ValuesY, ref Dim) == false)
                return;

            int IsoLine = colorScheme.CountIsoLine;
            if (flagOne == true)
                IsoLine = 1;

            double[] Xn = { 0, 0, 0 };
            double[] Yn = { 0, 0, 0 };

            double pmin, pmax;
            int[] ScalNum = new int[IsoLine];
            double[] IsoValue = new double[IsoLine];


            if (flagOne == true)
            {
                IsoValue[0] = ValueISO;
            }
            else
            {
                if (renderOptions.cb_GradScaleLimit == true)
                {
                    MinV = renderOptions.MinValue;
                    MaxV = renderOptions.MaxValue;
                }
                else
                {
                    MinV = MinV0 + (MaxV0 - MinV0) * colorScheme.MinIsoLine / 100f;
                    MaxV = MinV0 + (MaxV0 - MinV0) * colorScheme.MaxIsoLine / 100f;
                }
                // шаг изолиний
                double DV = (MaxV - MinV) / (IsoLine + 1);
                for (uint i = 0; i < IsoLine; i++)
                {
                    double Value = MinV + DV * (i + 1);
                    IsoValue[i] = Value;
                }
            }

            double[] Fn = { 0, 0, 0 };
            // цикл по КЭ
            TriElement[] elems = mesh.GetAreaElems();
            double[] x = mesh.GetCoords(0);
            double[] y = mesh.GetCoords(1);
            uint CountVert = 3;
            for (uint elem = 0; elem < elems.Length; elem++)
            {
                TriElement.TriElemValues(x, elems[elem], ref Xn);
                TriElement.TriElemValues(y, elems[elem], ref Yn);
                TriElement.TriElemValues(Values, elems[elem], ref Fn);

                pmin = Fn.Min();
                pmax = Fn.Max();
                // построение изолиний
                double pt, pa, pb, xN, xE, yN, yE;
                double[] xline = { 0, 0, 0, 0 };
                double[] yline = { 0, 0, 0, 0 };
                // цикл по изолиниям
                uint liz;
                for (uint isoline = 0; isoline < IsoLine; isoline++)
                {
                    // значение изолинии
                    pt = IsoValue[isoline];
                    // условие наличия текущей изолинии в области КЭ
                    if (pmax >= pt && pmin <= pt)
                    {
                        liz = 0;
                        // ----- цикл по граням текущего элемента -----------
                        for (uint m = 0; m < CountVert; m++)
                        {
                            pa = Fn[m]; pb = Fn[(m + 1) % CountVert];
                            xN = Xn[m]; xE = Xn[(m + 1) % CountVert];
                            yN = Yn[m]; yE = Yn[(m + 1) % CountVert];

                            if (Math.Abs(pa - pb) < 0.00000000001) continue;
                            // --- условие прохождения изолинии через грань
                            if ((pa >= pt && pt >= pb) || (pb >= pt && pt >= pa))
                            {
                                // работа с пропорцией
                                double xt = (2.0 * pt - pa - pb) / (pb - pa);
                                xline[liz] = 0.5 * ((1.0 - xt) * xN + (1.0 + xt) * xE);
                                yline[liz] = 0.5 * ((1.0 - xt) * yN + (1.0 + xt) * yE);
                                liz++;
                            }
                        }
                        // -- конец цикла по граням --------------------
                        // отрисовка изолинии
                        PointF p0 = new PointF((float)xline[0], (float)yline[0]);
                        PointF p1 = new PointF((float)xline[1], (float)yline[1]);

                        if (zoom.ViewportContains(p0) == false ||
                            zoom.ViewportContains(p1) == false)
                            continue;

                        // Масштабирование
                        zoom.WorldToScreen(ref p0);
                        zoom.WorldToScreen(ref p1);
                        // отрисовка изолиний
                        g.DrawLine(colorScheme.PenIsoLine, p0, p1);
                        if (renderOptions.opIsoLineValuesShow == true)
                        {
                            if (ScalNum[isoline] == 0)
                            {
                                double v = renderOptions.ScaleValue(pt);
                                string res = String.Format(colorScheme.FormatText, v);
                                g.DrawString(res, colorScheme.FontValue, colorScheme.BrushTextValues, p1);
                                ScalNum[isoline] = 1;
                            }
                        }
                    }
                }
            }

        }
        /// <summary>
        /// Векторное поле
        /// </summary>
        /// <param name="g"></param>
        private void RenderVectorField(Graphics g)
        {
            if (data.mesh == null) return;
            if (MeshIsNull()) return;
            // Индекс поля
            int Dim = 0;
            double AreaV = 0;
            if (data.GetPoleMinMax(renderOptions.indexValues, ref MinV, ref MaxV, ref SumV, ref AreaV, ref Values, ref ValuesX, ref ValuesY, ref Dim) == false)
                return;
            if (Dim == 1)
                return;
            if (ValuesY == null)
                return;
            double LValue = MaxV - MinV;
            MEM.Alloc(Values.Length, ref VeLLines);
            MEM.Alloc(Values.Length, ref LengthLines);
            for (int i = 0; i < Values.Length; i++)
            {
                float dt = 15;
                float dxV = (float)(ValuesX[i] / LValue);
                float dyV = (float)(ValuesY[i] / LValue);
                VeLLines[i] = new FPoint(dxV * dt, dyV * dt);
                LengthLines[i] = VeLLines[i].Length();
            }
            float max = LengthLines.Max();
            PointF ps = new PointF(max,max);
            zoom.WorldToScreen(ref ps);
            float maxPS = ((FPoint)ps).Length();
            int line = 50;
            int PixelVel = 100;
            float scale = PixelVel / maxPS;


            PointF pt, pv,pL,pR,pp;
            float a = 0.15f;
            float b = 0.7f;
            for (int i = 0; i < Values.Length; i++)
            {
                pt = new PointF((float)X[i], (float)Y[i]);
                zoom.WorldToScreen(ref pt);
               // g.FillEllipse(colorScheme.BrushPoint, pt.X - 1.5f, pt.Y - 1.5f, 3, 3);
                
              //  if (MEM.Equals(LengthLines[i], 0) == true) continue;
              //  pv = (PointF)VeLLines[i];
              //  zoom.WorldToScreen(ref pv);
              //  pv.X= pv.X * scale + pt.X; pv.Y = pv.Y * scale + pt.Y;
              //  g.DrawLine(colorScheme.PenVectorLine, pt, pv);

              ////  Console.WriteLine(pv.ToString());
              //  // основание наконечника
              //  pp = new PointF(b * pt.X + (1 - b) * pv.X, b * pt.Y - (1 - b) * pv.X);
              //  FPoint norm = VeLLines[i].GetOrt();
              //  FPoint L =  a * norm * PixelVel;
              //  FPoint R = -L;
              //  pL = new PointF(pp.X + L.X, pp.Y + L.Y);
                // pR = new PointF(pp.X + R.X, pp.Y + R.Y);

                //  g.DrawLine(colorScheme.PenVectorLine, pL, pv);
                // g.DrawLine(colorScheme.PenVectorLine, pR, pv);



                float dxv = (float)(line * ValuesX[i] / LValue);
                float dyv = (float)(line * ValuesY[i] / LValue);
                FPoint norm = new FPoint(-dyv, dxv);
                FPoint L = norm * a;
                FPoint R = -L;


                pv = new PointF(pt.X + dxv, pt.Y - dyv);
                pp = new PointF(pt.X + b * dxv, pt.Y - b * dyv);
                pL = new PointF(pp.X + L.X, pp.Y + L.Y);
                pR = new PointF(pp.X + R.X, pp.Y + R.Y);
                g.DrawLine(colorScheme.PenVectorLine, pt, pv);
                g.DrawLine(colorScheme.PenVectorLine, pL, pv);
                g.DrawLine(colorScheme.PenVectorLine, pR, pv);
            }
        }
        /// <summary>
        /// Заливка КЭ
        /// </summary>
        /// <param name="g"></param>
        private void NativeRenderFillField(Graphics g)
        {
            // Индекс поля
            if (data.mesh == null) return;
            if (MeshIsNull()) return;
            TriElement[] AreaElems = data.mesh.GetAreaElems();
            if (AreaElems == null)
                return;
            double MSumV = 0;
            double AreaV = 0;
            int Dim = 0;
            if (data.GetPoleMinMax(renderOptions.indexValues, ref MinV, ref MaxV, ref MSumV, ref AreaV, ref Values, ref ValuesX, ref ValuesY, ref Dim) == false)
                return;

            TriVertex[] triVertex = new TriVertex[Values.Length];
            for (uint i = 0; i < triVertex.Length; i++)
            {
                Color col = colorScheme.RGBBrush(Values[i], MinV, MaxV);
                PointF pt = new PointF((float)X[i], (float)Y[i]);
                zoom.WorldToScreen(ref pt);
                triVertex[i].SetTriVertex(ref pt, ref col);
            }
            var hdc = g.GetHdc();
            FillMethods.GradientFill(hdc, triVertex, (uint)triVertex.Length,
                AreaElems, (uint)AreaElems.Length, GradientFillMode.GRADIENT_FILL_TRIANGLE);
            g.ReleaseHdc(hdc);
        }
        /// <summary>
        /// Заливка градиентной шкалы + 15 06 2024
        /// </summary>
        /// <param name="g"></param>
        private void GradientScale(Graphics g)
        {
            //   3------2
            //   |  1  /|
            //   |   /  | 
            //   | /  0 |
            //   0 ---- 1
            // Индекс поля
            if (data.mesh == null) return;
            if (MeshIsNull()) return;
            TriElement[] AreaElemsSacale = { new TriElement(0, 1, 2), new TriElement(2, 3, 0) };
            
                // очистка квадрата
                int[] cX0 = { 90, 300, 300, 90 };
                int[] cY0 = { 10, 10, 240, 240 };

                TriVertex[] cLTriVertex = new TriVertex[4];
                for (uint i = 0; i < cLTriVertex.Length; i++)
                {
                    Color col = Color.White;
                    PointF pt = new PointF(cX0[i], cY0[i]);
                    cLTriVertex[i].SetTriVertex(ref pt, ref col);
                }
            //   По одному квадрату на каждый цве R G B
            //   7------6 210
            //   |  5  /|
            //   |   /  | 
            //   | /  4 |
            //   5------4 140
            //   |  3  /|
            //   |   /  | 
            //   | /  2 |
            //   3------2 70
            //   |  1  /|
            //   |   /  | 
            //   | /  0 |
            //   0 ---- 1 0
            // Индекс поля
            // Заливка квадрата
            TriElement[] AreaElemsColor = { new TriElement(0, 1, 2), new TriElement(2, 3, 0),
                                            new TriElement(3, 2, 4), new TriElement(4, 5, 3),
                                            new TriElement(5, 4, 6), new TriElement(6, 7, 5) };
            GetScaleLimit(ref minV, ref maxV);
            int[] cX = { 100, 150, 150, 100, 150, 100, 150, 100 };
            int[] cY = { 20, 20, 90, 90, 160, 160, 230, 230 };
            double v13 = 2 * minV / 3 + maxV / 3;
            double v23 = minV / 3 + 2 * maxV / 3;
            double[] clearValue = { minV, minV, v13, v13, v23, v23, maxV, maxV };
            TriVertex[] triVertex = new TriVertex[8];
            for (uint i = 0; i < triVertex.Length; i++)
            {
                Color col = colorScheme.RGBBrush(clearValue[i], minV, maxV);
                PointF pt = new PointF(cX[i], cY[i]);
                triVertex[i].SetTriVertex(ref pt, ref col);
            }
            var hdc = g.GetHdc();
                    FillMethods.GradientFill(hdc, cLTriVertex, (uint)cLTriVertex.Length,
                        AreaElemsSacale, (uint)AreaElemsSacale.Length, GradientFillMode.GRADIENT_FILL_TRIANGLE);

                    FillMethods.GradientFill(hdc, triVertex, (uint)triVertex.Length,
                    AreaElemsColor, (uint)AreaElemsColor.Length, GradientFillMode.GRADIENT_FILL_TRIANGLE);
                g.ReleaseHdc(hdc);

            int[] fY0 = { 20, 70, 120, 170, 220 };
            double dy = 1.0 / (fY0.Length - 1);
            PointF pf;
            for (int i = 0; i < fY0.Length; i++)
            {
                pf = new PointF((float)X[i], (float)Y[i]);
                pf.X = cX[1] + 5f; 
                pf.Y = fY0[i] - colorScheme.FontValue.Size/2-1;
                double v = minV * (1 - i * dy) + maxV * i * dy;
                double vv = renderOptions.ScaleValue(v);
                string res = String.Format(colorScheme.FormatText, vv);
                g.DrawString(res, colorScheme.FontValue, colorScheme.BrushTextValues, pf);
            }

        }
        /// <summary>
        /// Выбор пределов для, заливки, шкалы и изолиний
        /// </summary>
        /// <param name="minV"></param>
        /// <param name="maxV"></param>
        public void GetScaleLimit(ref double minV, ref double maxV)
        {
            if (renderOptions.cb_GradScaleLimit == true)
            {
                minV = renderOptions.MinValue;
                maxV = renderOptions.MaxValue;
            }
            else
            {
                minV = MinV;
                maxV = MaxV;
            }
        }
    }
}



