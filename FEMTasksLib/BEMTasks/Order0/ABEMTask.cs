#region License
/*
Copyright (c) 2019 - 2024  Potapov Igor Ivanovich, Khabarovsk

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion
//---------------------------------------------------------------------------
//                  Библиотека  метода граничных элементов
//                              Потапов И.И.
//                        - () Copyright 2014 -
//                          ALL RIGHT RESERVED
//                              07.10.14
//---------------------------------------------------------------------------
//                              Потапов И.И.
//                               12.05.21
//---------------------------------------------------------------------------
namespace FEMTasksLib
{
    using AlgebraLib;
    using CommonLib;
    using MemLogLib;
    using System;

    /// <summary>
    /// ОО: А класс для метода граничных элементов 0 порядка
    /// </summary>
    [Serializable]
    public abstract class ABEMTask : IBEMTask
    {
        /// <summary>
        /// граничные условия
        /// </summary>
        protected BoundLabel[] boundLabels;
        /// <summary>
        /// Флаг отладки
        /// </summary>
        public int Debug { get; set; }
        /// <summary>
        /// Сетка решателя
        /// </summary>
        public IMesh Mesh { get => mesh; }
        protected IMesh mesh;
        /// <summary>
        /// Алгебра задачи
        /// </summary>
        public IAlgebra Algebra { get => algebra; }
        protected IAlgebra algebra;
        /// <summary>
        /// Количество ГЭ
        /// </summary>
        protected int N;
        /// <summary>
        /// Количество КЭ
        /// </summary>
        protected int M;
        /// <summary>
        /// Координаты узлов (Х)
        /// </summary>
        protected double[] x = null;
        /// <summary>
        /// Координаты узлов (У)
        /// </summary>
        protected double[] y = null;
        /// <summary>
        /// Узлы КЭ
        /// </summary>
        protected uint[] knots = { 0, 0, 0 };
        /// <summary>
        /// Граничные элементы сетки
        /// </summary>
        protected TwoElement[] belems;
        /// <summary>
        /// потонциальное поле 
        /// </summary>
        public double[] Phi = null;
        /// <summary>
        /// поле потока
        /// </summary>
        public double[] U = null;
        /// <summary>
        /// Получить потонциальное поле 
        /// </summary>
        /// <returns></returns>
        public double[] GetPhi() { return Phi; }
        /// <summary>
        /// Получить поле потока
        /// </summary>
        /// <returns></returns>
        public double[] GetU() { return U; }

        #region Локальныен переменные класса
        /// <summary>
        /// Масштаб ядра - функции Грина
        /// </summary>
        public double Radius0 = 1.0;
        /// <summary>
        /// Alpha = -1.0 / (2.0 * Math.PI);
        /// Alpha - масштабируется в наследниках на дифузионный коэффициент Mu
        /// Alpha = -1.0 / (2.0 * Math.PI)/ Mu;
        /// </summary>
        public double Alpha = 0;
        /// <summary>
        /// угол тетта от цента наблюдения до первого узла грани влияния
        /// </summary>
        public double thetaA = 0;
        /// <summary>
        /// угол тетта от цента наблюдения до второго узла грани влияния
        /// </summary>
        public double thetaB = 0;
        /// <summary>
        /// длина радиус вектора от цента наблюдения до первого узла грани влияния
        /// </summary>
        public double rA = 0;
        /// <summary>
        /// длина радиус вектора от цента наблюдения до второго узла грани влияния
        /// </summary>
        public double rB = 0;
        /// <summary>
        /// растояние  по нормали от точки наблюдения до линии влияния
        /// </summary>
        public double h = 0;
        #endregion
        public ABEMTask(IMesh mesh, IAlgebra algebra, BoundLabel[] boundLabels)
        {
            this.boundLabels = boundLabels;
            Debug = 0;
            SetTask(mesh, algebra);
        }
        /// <summary>
        /// Установка сетки задачи и решателя СЛУ
        /// </summary>
        /// <param name="mesh">сетки задачи</param>
        /// <param name="algebra">решатель СЛУ</param>
        public virtual void SetTask(IMesh mesh, IAlgebra algebra)
        {
            // Alpha мосштабируется в наследниках на дифузионный коэффициент
            Alpha = -1.0 / (2.0 * Math.PI);
            this.mesh = mesh;
            belems = mesh.GetBoundElems();
            // Количество ГЭ
            N = mesh.CountBoundElements;
            // Количество КЭ
            M = mesh.CountElements;
            x = mesh.GetCoords(0);
            y = mesh.GetCoords(1);
            Phi = new double[mesh.CountKnots];
            U = new double[mesh.CountKnots];
            if (algebra == null)
                algebra = new AlgebraGauss((uint)N);
            this.algebra = algebra;
        }
        #region методы для реализации BEM
        /// <summary>
        /// Интеграл от функции грина int_{Delta S}( G(x^p,xi^q) ) dS формула (3.50) 
        /// Расчет потенциала, создаваемого в точке наблюдения Хр равномерно распределеннным вдоль грани 
        /// длинной h источником единичной интенсивности
        /// </summary>
        /// <param name="thetaA">угол тетта от цента наблюдения до первого узла грани влияния</param>
        /// <param name="thetaB">угол тетта от цента наблюдения до второго узла грани влияния</param>
        /// <param name="rA">длина радиус вектора от цента наблюдения до первого узла грани влияния</param>
        /// <param name="rB">длина радиус вектора от цента наблюдения до второго узла грани влияния</param>
        /// <param name="h">растояние  по нормали от точки наблюдения до линии влияния</param>
        /// <returns>Вычесленный потенциал</returns>
        public double GreenBoundElem(double thetaA, double thetaB, double rA, double rB, double h)
        {
            double gs = Alpha *
                ((rB * Math.Sin(thetaB) * (Math.Log(Math.Abs(rB / Radius0)) - 1) + thetaB * h)
               - (rA * Math.Sin(thetaA) * (Math.Log(Math.Abs(rA / Radius0)) - 1) + thetaA * h));
            return gs;
        }

        /// <summary>
        /// Интеграл от функции грина int_{Delta A}( G(x^p,xi^q) ) dS формула (3.55) 
        /// Расчет потенциала, создаваемого в точке наблюдения Хр равномерно распределеннным вдоль грани 
        /// длинной h источником единичной интенсивности
        /// </summary>
        /// <param name="thetaA">угол тетта от цента наблюдения до первого узла грани влияния</param>
        /// <param name="thetaB">угол тетта от цента наблюдения до второго узла грани влияния</param>
        /// <param name="rA">длина радиус вектора от цента наблюдения до первого узла грани влияния</param>
        /// <param name="rB">длина радиус вектора от цента наблюдения до второго узла грани влияния</param>
        /// <param name="h">растояние  по нормали от точки наблюдения до линии влияния</param>
        /// <returns>Вычесленный потенциал</returns>
        public double GreenAreaElem(double thetaA, double thetaB, double rA, double rB, double h)
        {
            return h * h * Alpha * 0.5 *
                   ((Math.Tan(thetaB) * (Math.Log(Math.Abs(rB / Radius0)) - 1.5) + thetaB) -
                    (Math.Tan(thetaA) * (Math.Log(Math.Abs(rA / Radius0)) - 1.5) + thetaA));
        }
        /// <summary>
        /// Функция Грина для двух граничыных элементов i и j
        /// </summary>
        public double GreenBound(int i, int j)
        {
            if (i == j)
            {
                double h2 = mesh.GetBoundElemLength((uint)i) / 2.0;
                return 2 * Alpha * h2 * (Math.Log(h2 / Radius0) - 1);
            }
            else
            {
                double xc, yc;
                // Узлы ГЭ влияния
                uint jdxA = belems[j].Vertex1;
                uint jdxB = belems[j].Vertex2;
                // координаты точки ГЭ наблюдения
                GetMidleXY(i, out xc, out yc);
                // расчет геометрии  (углов, расстояний, размера)
                GetRelation(xc, yc, jdxA, jdxB, out thetaA, out thetaB, out rA, out rB, out h);
                // расчет матрицы Грина (fs)
                // расчет матрицы потенциалов (fs)
                return GreenBoundElem(thetaA, thetaB, rA, rB, h);
            }
        }
        /// <summary>
        /// Нормальная производная от функции Грина для двух граничыных элементов i и j
        /// Интеграл от функции грина int_{Delta S}( F(x^p,xi^q) ) dS формула (3.53) 
        /// </summary>
        public double DiffGreenBound(int i, int j)
        {
            if (i == j)
            {

                return -0.5;
            }
            else
            {
                double xc, yc;
                // Узлы ГЭ влияния
                uint jdxA = belems[j].Vertex1;
                uint jdxB = belems[j].Vertex2;
                // координаты точки ГЭ наблюдения
                GetMidleXY(i, out xc, out yc);
                // расчет геометрии  (углов, расстояний, размера)
                GetRelation(xc, yc, jdxA, jdxB, out thetaA, out thetaB, out rA, out rB, out h);
                // расчет матрицы потенциалов (fs)
                return (thetaB - thetaA) / (2 * Math.PI);
            }
        }
        public double DiffGreenBound(double xc, double yc, uint jdxA, uint jdxB)
        {
            // расчет геометрии  (углов, расстояний, размера)
            GetRelation(xc, yc, jdxA, jdxB, out thetaA, out thetaB, out rA, out rB, out h);
            // расчет матрицы потенциалов (fs)
            return (thetaB - thetaA) / (2 * Math.PI);
        }
        /// <summary>
        /// Лаплас от функции Грина для двух граничыных элементов i и j
        /// !!!!!!!!!!!!!!!!!!!!! метод недоделан нет нормалей!!!
        /// формула (3.53)
        /// </summary>
        public double Diff2GreenBound(int i, int j, double Mu)
        {
            double xc, yc;
            double n1A = 0, n2A = 0, n1B = 0, n2B = 0;
            //// Узлы ГЭ влияния
            uint jdxA = belems[j].Vertex1;
            uint jdxB = belems[j].Vertex2;
            // координаты точки ГЭ наблюдения
            GetMidleXY(i, out xc, out yc);
            // расчет геометрии  (углов, расстояний, размера)
            GetRelation(xc, yc, jdxA, jdxB, out thetaA, out thetaB, out rA, out rB, out h);
            //// расчет матрицы потенциалов (H)
            return -Alpha * Mu * Mu / (4 * h) * ((n1B * (2 * thetaB - Math.Sin(2 * thetaB)) + n2B * Math.Cos(2 * thetaB)) -
                                                 (n1A * (2 * thetaA - Math.Sin(2 * thetaA)) + n2A * Math.Cos(2 * thetaA)));
        }

        /// <summary>
        /// Вычисление функции тока в k - м узле 
        /// </summary>
        /// <returns>функция тока</returns>
        protected double CalkPhi(uint k, double[] bePhi, double[] beU)
        {
            //double[] phiQ = { 0, 0, 0 };
            //uint[] jdxGA = { 0, 0, 0 };
            //uint[] jdxGB = { 0, 0, 0 };
            double xc, yc;
            // координаты точки ГЭ наблюдения
            xc = x[k]; yc = x[k];
            double phi = 0;
            // вычисление влияния со стороны других элементов
            for (int be = 0; be < N; be++)
            {
                // Узлы ГЭ влияния
                uint jdxA = belems[be].Vertex1;
                uint jdxB = belems[be].Vertex2;
                // расчет геометрии  (углов, расстояний, размера)
                GetRelation(xc, yc, jdxA, jdxB, out thetaA, out thetaB, out rA, out rB, out h);
                // расчет матрицы Грина (fs)
                double Fs = (thetaB - thetaA) / (2 * Math.PI);
                // расчет матрицы потенциалов (fs)
                double Gs = GreenBoundElem(thetaA, thetaB, rA, rB, h);
                phi += Fs * bePhi[be] + Gs * beU[be];
            }
            // Цикл по КЭ в области
            //for (int elem = 0; elem < M; elem++)
            //{
            //    // получить узлы КЭ
            //    mesh.ElementKnots((uint)elem, ref knots);

            //    jdxGA[0] = knots[0];
            //    jdxGB[0] = knots[1];
            //    jdxGA[1] = knots[1];
            //    jdxGB[1] = knots[2];
            //    jdxGA[2] = knots[2];
            //    jdxGB[2] = knots[0];

            //    // Вычисляем потенциал действующий на ГЭ наблюдения со стороны каждой грани элемента
            //    for (int knot = 0; knot < 3; knot++)
            //    {
            //        // расчет геометрии  (углов, расстояний, размера)
            //        GetRelation(xc, yc, jdxGA[knot], jdxGB[knot], out thetaA, out thetaB, out rA, out rB, out h);
            //        // расчет потенциала грани (fs)
            //        phiQ[knot] = GreenAreaElem(thetaA, thetaB, rA, rB, h);
            //        // коррекция коэффициентов    
            //        phiQ[knot] = Math.Abs(phiQ[knot]) > MEM.Error6 ? phiQ[knot] : 0;
            //    }
            //    // Вычисляем вклад каждой грани текущего КЭ на суммарный потенциал от объемного источника (суперпозиция)
            //    //for (int knot = 0; knot < 3; knot++)
            //    //{
            //    //    double X, Y;
            //    //    // если узел ГЭ не принадлежит элементу в области, то вычисляем потонциал как
            //    //    if (!PointInTriangle(i, elem))
            //    //    {
            //    //        uint jdxA = jdxGA[knot];            // узел связанный с точкой наблюдения - луч
            //    //        uint jdxAS = jdxGA[(knot + 1) % 3];  // первый узел грани
            //    //        uint jdxBS = jdxGB[(knot + 1) % 3];  // второй узел грани
            //    //                                             // определяем точку пересечения луча наблюдения с граню треугольника
            //    //        int res = GetRaySegmentIntersection(xc, yc, x[jdxA], y[jdxA], x[jdxAS], y[jdxAS], x[jdxBS], y[jdxBS], out X, out Y);
            //    //        //int res = mesh.Crossing(xc, yc, mesh.x[jdxA], mesh.y[jdxA], mesh.x[jdxAS], mesh.y[jdxAS], mesh.x[jdxBS], mesh.y[jdxBS], out X, out Y);
            //    //        if (res == 1)
            //    //        {
            //    //            double Length1 = LengthElem(xc, yc, x[jdxA], y[jdxA]);
            //    //            double Length2 = LengthElem(xc, yc, X, Y);
            //    //            // сложение и вычитание секторов определяющих потенциал
            //    //            if (Length1 > Length2)
            //    //                phiQ[(knot + 1) % 3] *= -1.0;
            //    //            else
            //    //            {
            //    //                phiQ[knot] *= -1.0;
            //    //                phiQ[(knot + 2) % 3] *= -1.0;
            //    //            }
            //    //            break;
            //    //        }
            //    //    }
            //    //}
            //    // заносим результат в матрицу объемного влияния 
            //    phi += Psi * phiQ.Sum();
            //}
            return phi;
        }
        #endregion
        #region Геометрия и алгебра
        /// <summary>
        /// Расстояние м/ж узлами
        /// </summary>
        /// <param name="X">координата Х первого узла</param>
        /// <param name="Y">координата У первого узла</param>
        /// <param name="dNdx">номер второго узела</param>
        /// <returns></returns>
        public double LengthElem(double X, double Y, uint dNdx)
        {
            return Math.Sqrt((X - x[dNdx]) * (X - x[dNdx]) + (Y - y[dNdx]) * (Y - y[dNdx]));
        }
        /// <summary>
        /// Расстояние м/ж узлами
        /// </summary>
        /// <param name="x1">координата Х первого узла</param>
        /// <param name="y1">координата У первого узла</param>
        /// <param name="x2">координата Х второго узла</param>
        /// <param name="y2">координата У второго узла</param>
        /// <returns></returns>
        public double LengthElem(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        }
        /// <summary>
        /// Расчет углов и растояний между двумя ГЭ
        /// </summary>
        /// <param name="xc">координата х центра ГЭ наблюдения</param>
        /// <param name="yc">координата у центра ГЭ наблюдения</param>
        /// <param name="jdxA">индекс начального узла ГЭ влияния</param>
        /// <param name="jdxB">индекс конечного узла Г влияния</param>
        /// <param name="thetaA">угол тетта от цента наблюдения до первого узла грани влияния</param>
        /// <param name="thetaB">угол тетта от цента наблюдения до второго узла грани влияния</param>
        /// <param name="rA">длина радиус вектора от цента наблюдения до первого узла грани влияния</param>
        /// <param name="rB">длина радиус вектора от цента наблюдения до второго узла грани влияния</param>
        /// <param name="h">растояние  по нормали от точки наблюдения до линии влияния</param>
        public void GetRelation(double xc, double yc, uint jdxA, uint jdxB, out double thetaA, out double thetaB, out double rA, out double rB, out double h)
        {
            double gammaEta = 0;
            // расчет растояния по нормали от точки наблюдения до линии влияния 
            GetHAndEta(xc, yc, jdxA, jdxB, out h, out gammaEta);
            if (h > MEM.Error6)
            {
                double gammaA = GetGamma(xc, yc, x[jdxA], y[jdxA]) - gammaEta;
                double gammaB = GetGamma(xc, yc, x[jdxB], y[jdxB]) - gammaEta;
                thetaA = NormAngle(gammaA);
                thetaB = NormAngle(gammaB);
            }
            else
            {
                h = 0.0;
                thetaA = Math.PI / 2.0;
                thetaB = Math.PI / 2.0;
            }
            rA = LengthElem(xc, yc, jdxA);
            rB = LengthElem(xc, yc, jdxB);
            if (rB < rA && h <= MEM.Error6)
            {
                thetaA *= -1.0;
                thetaB *= -1.0;
            }
            if (thetaB < thetaA)
            {
                double tmp = thetaA;
                thetaA = thetaB;
                thetaB = tmp;
                tmp = rA;
                rA = rB;
                rB = tmp;
            }
        }
        /// <summary>
        /// расчет растояния (h) по нормали от точки наблюдения до линии влияния и угла (gammaEta)
        /// </summary>
        /// <param name="xc">х точки наблюдения</param>
        /// <param name="yc">у точки наблюдения</param>
        /// <param name="jdxA">индекс начального узла ГЭ влияния</param>
        /// <param name="jdxB">индекс конечного узла Г влияния</param>
        /// <param name="h">растояние  по нормали от точки наблюдения до линии влияния</param>
        /// <param name="gammaEta"></param>
        public void GetHAndEta(double xc, double yc, uint jdxA, uint jdxB, out double h, out double gammaEta)
        {
            // интервал м/д узлами по Х
            double dx = x[jdxB] - x[jdxA];
            // если он нулевой то элемент расположен вертикально
            if (Math.Abs(dx) < MEM.Error6)
            {
                h = Math.Abs(x[jdxA] - xc);
                if (x[jdxA] < xc)
                    gammaEta = Math.PI;
                else
                    gammaEta = 0;
            }
            else
            {   // угол наклона элемента
                double tanPhi = (y[jdxB] - y[jdxA]) / dx;
                double Yx = y[jdxA] - tanPhi * x[jdxA];
                double dX = yc * tanPhi + xc;
                // определение пересечения нормали из точки наблюдения до линии ГЭ
                double hx = (dX - Yx * tanPhi) / (1 + tanPhi * tanPhi);
                double hy = tanPhi * hx + Yx;
                // определение длины нормали
                h = LengthElem(xc, yc, hx, hy);
                // вычисления угла коррекции
                gammaEta = GetGamma(xc, yc, hx, hy);
            }
        }
        /// <summary>
        /// Угол между между двумя точками и осью Х (в стандартной СК)
        /// </summary>
        /// <param name="xc">х точки наблюдения</param>
        /// <param name="yc">у точки наблюдения</param>
        /// <param name="xj">х текущей точки</param>
        /// <param name="yj">у текущей точки</param>
        /// <returns>угол</returns>
        public double GetGamma(double xc, double yc, double xj, double yj)
        {
            if (Math.Abs(xj - xc) < MEM.Error6)
            {
                if (yj < yc)
                    return -Math.PI / 2.0;
                else
                    return Math.PI / 2.0;
            }
            double gamma = Math.Atan((yj - yc) / (xj - xc));
            if (xj < xc)
            {
                if (yj < yc)
                    gamma -= Math.PI;
                else
                    gamma += Math.PI;
            }
            return gamma;
        }
        /// <summary>
        /// Расчет нормального угла 
        /// </summary>
        /// <param name="ang">угол на входе</param>
        /// <returns>угол на выходе</returns>
        public double NormAngle(double ang)
        {
            if (Math.Abs(ang) < Math.PI)
                return ang;
            if (ang > 0)
                return Math.Abs(ang) - (2 * Math.PI);
            return (2 * Math.PI) - Math.Abs(ang);
        }
        /// <summary>
        /// Определения принадлежности точки (центра ГЭ) к одной из граней треугольников КЭ сетки
        /// </summary>
        /// <param name="idxA">индекс ГЭ наблюдения</param>
        /// <param name="elem">индекс конечного элемента</param>
        /// <returns>результат проверки</returns>
        public bool PointInTriangle(int idx, int elem)
        {
            // координаты средней точки ГЭ
            double xc, yc;
            GetMidleXY(idx, out xc, out yc);
            // получить номера узлов треугольника КЭ
            mesh.ElementKnots((uint)elem, ref knots);
            uint i = knots[0];
            uint j = knots[1];
            uint k = knots[2];
            double n1 = (y[j] - y[i]) * (xc - x[i]) - (x[j] - x[i]) * (yc - y[i]);
            double n2 = (y[k] - y[j]) * (xc - x[j]) - (x[k] - x[j]) * (yc - y[j]);
            double n3 = (y[i] - y[k]) * (xc - x[k]) - (x[i] - x[k]) * (yc - y[k]);
            return ((n1 >= 0) && (n2 >= 0) && (n3 >= 0)) || ((n1 <= 0) && (n2 <= 0) && (n3 <= 0));
        }
        /// <summary>
        /// Вычисление координат середины ГЭ
        /// </summary>
        /// <param name="i">номер ГЭ</param>
        protected void GetMidleXY(int i, out double xc, out double yc)
        {
            xc = 0.5 * (x[belems[i].Vertex1] + x[belems[i].Vertex2]);
            yc = 0.5 * (y[belems[i].Vertex1] + y[belems[i].Vertex2]);
        }
        /// <summary>
        /// Определение точки пересечения двух отрезков (метод Бориса) (переделать - избавится от inf..)
        /// </summary>
        /// <param name="x1">координаты узлов</param>
        /// <param name="y1">координаты узлов</param>
        /// <param name="x2">координаты узлов</param>
        /// <param name="y2">координаты узлов</param>
        /// <param name="x3">координаты узлов</param>
        /// <param name="y3">координаты узлов</param>
        /// <param name="x4">координаты узлов</param>
        /// <param name="y4">координаты узлов</param>
        /// <param name="X">координата Y точки пересечения</param>
        /// <param name="Y">координата Х точки пересечения</param>
        /// <returns>признак пересечения 0 - отрезки коллинеарны, 1 - пересечение есть, -1 - пересечения нет</returns>
        public int GetRaySegmentIntersection(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4, out double X, out double Y)
        {
            double aK = (y2 - y1) / (x2 - x1);
            double bK = (y4 - y3) / (x4 - x3);
            X = 0; Y = 0;
            if (aK != bK)
            {
                if (double.IsInfinity(aK))
                {
                    X = x1;
                    Y = bK * X + y3 - bK * x3;
                }
                else
                    if (double.IsInfinity(bK))
                {
                    X = x3;
                    Y = aK * X + y1 - aK * x1;
                }
                else
                {
                    X = (y3 - bK * x3 - (y1 - aK * x1)) / (aK - bK);
                    Y = aK * X + y1 - aK * x1;
                }

                double segH = LengthElem(x3, y3, x4, y4);
                double haR = LengthElem(X, Y, x3, y3);
                double hbR = LengthElem(X, Y, x4, y4);

                double criteria = segH / (haR + hbR);
                if (criteria > (1.0 - MEM.Error6))
                    return 1;
                if (haR < MEM.Error6 || hbR < MEM.Error6)
                    return 1;
            }
            return -1;
        }
        #endregion
        /// <summary>
        /// Решение задачи дискретизации задачи и ее рещателя
        /// </summary>
        /// <param name="result">результат решения</param>
        public abstract void SolveTask(ref double[] result);
    }
}
