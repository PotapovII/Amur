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
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//---------------------------------------------------------------------------
//                   12.03.2024 Потапов И.И.
//---------------------------------------------------------------------------
//             Эксперимент: прототип "Расчетная область"
//---------------------------------------------------------------------------
namespace MeshLib.CArea
{
    using MemLogLib;
    using CommonLib;

    using System;
    using System.Linq;
    using CommonLib.Mesh;
    using CommonLib.Geometry;
    using GeometryLib.Locators;

    /// <summary>
    /// ОО: Обертка для КЭ сетки - выполняет предварительные вычисления
    /// при решении задач методом КЭ в канале (x,z)
    /// </summary>
    [Serializable]
    public class MeshWrapperСhannelCFGTri : MeshWrapperTri, IMeshWrapperСhannelCFG
    {
        /// <summary>
        /// Тип формы канала в створе потока
        /// </summary>
        public СhannelSectionForms channelSectionForms { get; set; }
        /// <summary>
        /// Минимальное растояние от узла до стенки
        /// </summary>
        protected double[] distance = null;
        /// <summary>
        /// Приведенная глубина
        /// Растояние до свободной поверхности потока по лучу от стенки
        /// </summary>
        protected double[] Hp = null;
        public MeshWrapperСhannelCFGTri(IMesh mesh,
        СhannelSectionForms channelSectionForms = СhannelSectionForms.porabolicСhannelSection)
            : base()
        {
            this.channelSectionForms = channelSectionForms;
            SetMesh(mesh);
        }
        public MeshWrapperСhannelCFGTri(IMeshWrapper wMmesh,
        СhannelSectionForms channelSectionForms = СhannelSectionForms.porabolicСhannelSection
            ) : base(wMmesh)
        {
            Init();
        }
        public MeshWrapperСhannelCFGTri(IMeshWrapperСhannelCFG wMmesh) : base(wMmesh)
        {
            channelSectionForms = wMmesh.channelSectionForms;
            MEM.MemCopy(ref distance, wMmesh.GetDistance());
            MEM.MemCopy(ref Hp, wMmesh.GetHp());
        }
        /// <summary>
        /// Установка КЭ сетки
        /// </summary>
        /// <param name="mesh"></param>
        public override void SetMesh(IMesh mesh)
        {
            base.SetMesh(mesh);
            Init();
        }
        protected void Init()
        {
            MEM.Alloc(mesh.CountKnots, ref distance, "distance");
            MEM.Alloc(mesh.CountKnots, ref Hp, "Hp");
            CalkDistance();
        }
        /// <summary>
        /// Минимальное растояние от узла до стенки
        /// </summary>
        public double[] GetDistance() => distance;
        /// <summary>
        /// Приведенная глубина
        /// </summary>
        /// <returns></returns>
        public double[] GetHp() => Hp;
        /// <summary>
        /// Вычисление минимального растояния от узла до стенки
        ///  В плпнпх - хеширование узлов на масштабе глубины
        /// </summary>
        public virtual void CalkDistance()
        {
            double[] X = mesh.GetCoords(0);
            double[] Y = mesh.GetCoords(1);
            TwoElement[] BoundElems = mesh.GetBoundElems();
            double X_min = X.Min();
            double X_max = X.Max();
            double Y_min = Y.Min();
            double Y_max = Y.Max();
            double L = X_max - X_min;
            double H = Y_max - Y_min;
            int[] BoundElementsMark = mesh.GetBElementsBCMark();
            HPoint p = new HPoint();
            IHPoint pc = new HPoint();
            double dis = 0, disA, disB, disL;
            // Поиск минимальног растояния до дна
            switch (channelSectionForms)
            {
                case СhannelSectionForms.porabolicСhannelSection:
                case СhannelSectionForms.halfPorabolicСhannelSection:
                    for (int nod = 0; nod < mesh.CountKnots; nod++)
                    {
                        p.X = X[nod];
                        p.Y = Y[nod];
                        double r_min = L;
                        double Yb = 0;
                        for (int belem = 0; belem < mesh.CountBoundElements; belem++)
                        {
                            // метка дна
                            if (BoundElementsMark[belem] == 0)
                            {
                                uint i = BoundElems[belem].Vertex1;
                                uint j = BoundElems[belem].Vertex2;
                                double Xmin = Math.Min(X[i], X[j]);
                                double Xmax = Math.Max(X[i], X[j]);
                                if ((p.X >= Xmin && p.X <= Xmax) == true)
                                {
                                    double Y1, Y2;
                                    if (X[i] < X[j])
                                    {
                                        Y1 = Y[i];
                                        Y2 = Y[j];
                                    }
                                    else
                                    {
                                        Y2 = Y[i];
                                        Y1 = Y[j];
                                    }
                                    double dx = Xmax - Xmin;
                                    double N2 = (p.X - Xmin) / dx;
                                    double N1 = 1 - N2;
                                    Yb = N1 * Y1 + N2 * Y2;
                                    r_min = p.Y - Yb;
                                    break;
                                }
                            }
                        }
                        distance[nod] = r_min;
                        Hp[nod] = Math.Max(0.01 * H, Y_max - Yb);
                    }
                    break;
                case СhannelSectionForms.boxСhannelCrossSection:
                    for (int nod = 0; nod < mesh.CountKnots; nod++)
                    {
                        double mL = Math.Min(X[nod] - X_min, X_max - X[nod]);
                        double mH = Y[nod] - Y_min;
                        distance[nod] = Math.Min(mL, mH);
                        Hp[nod] = H;
                    }
                    break;
                case СhannelSectionForms.boxСhannelCrossTrapezoidSection:
                    for (int nod = 0; nod < mesh.CountKnots; nod++)
                    {
                        p.X = X[nod];
                        p.Y = Y[nod];
                        double r_min = L;
                        double Yb = 0;
                        for (int belem = 0; belem < mesh.CountBoundElements; belem++)
                        {
                            if (channelSectionForms == СhannelSectionForms.boxСhannelCrossTrapezoidSection)
                            {
                                bool flagBoundary = BoundElementsMark[belem] == 0 ||
                                                BoundElementsMark[belem] == 1 ||
                                                BoundElementsMark[belem] == 3;
                                if (flagBoundary == true)
                                {
                                    uint i = BoundElems[belem].Vertex1;
                                    uint j = BoundElems[belem].Vertex2;
                                    IHPoint p1 = new HPoint(X[i], Y[i]);
                                    IHPoint p2 = new HPoint(X[j], Y[j]);
                                    double Lab = CrossLine.LookCrossPoint(p1, p2, p, ref pc);
                                    Lab = ((HPoint)p1 - (HPoint)p2).Length();
                                    disA = ((HPoint)p1 - (HPoint)pc).Length();
                                    disB = ((HPoint)p2 - (HPoint)pc).Length();
                                    if (1.1 * Lab < disA + disB)
                                        continue;
                                    dis = ((HPoint)pc - p).Length();
                                    if (r_min > dis)
                                        r_min = dis;
                                }
                            }
                        }
                        distance[nod] = r_min;
                        Hp[nod] = Math.Max(0.01 * H, Y_max - Yb);
                    }
                    break;
            }

            //if (channelSectionForms == СhannelSectionForms.boxСhannelCrossTrapezoidSection)
            //{
            //    // Поиск минимальног растояния до дна
            //    for (int nod = 0; nod < mesh.CountKnots; nod++)
            //    {
            //        p.X = X[nod];
            //        p.Y = Y[nod];
            //        double r_min = L;
            //        double Yb = 0;
            //        for (int belem = 0; belem < mesh.CountBoundElements; belem++)
            //        {
            //            if (channelSectionForms == СhannelSectionForms.boxСhannelCrossTrapezoidSection)
            //            {
            //                bool flagBoundary = BoundElementsMark[belem] == 0 ||
            //                                BoundElementsMark[belem] == 1 ||
            //                                BoundElementsMark[belem] == 3;
            //                if (flagBoundary == true)
            //                {
            //                    uint i = BoundElems[belem].Vertex1;
            //                    uint j = BoundElems[belem].Vertex2;
            //                    IHPoint p1 = new HPoint(X[i], Y[i]);
            //                    IHPoint p2 = new HPoint(X[j], Y[j]);
            //                    double  Lab = CrossLine.LookCrossPoint(p1, p2, p, ref pc);
            //                    Lab = ((HPoint)p1 - (HPoint)p2).Length();
            //                    disA = ((HPoint)p1 - (HPoint)pc).Length();
            //                    disB = ((HPoint)p2 - (HPoint)pc).Length();
            //                    if (1.1*Lab < disA + disB)
            //                        continue;
            //                    dis = ((HPoint)pc - p).Length();
            //                    if (r_min > dis)
            //                        r_min = dis;
            //                }
            //            }
            //            else
            //            {
            //                bool flagBoundary = BoundElementsMark[belem] == 0;
            //                // метка дна
            //                if (flagBoundary == true)
            //                {
            //                    uint i = BoundElems[belem].Vertex1;
            //                    uint j = BoundElems[belem].Vertex2;
            //                    double Xmin = Math.Min(X[i], X[j]);
            //                    double Xmax = Math.Max(X[i], X[j]);
            //                    if ((p.X >= Xmin && p.X <= Xmax) == true)
            //                    {
            //                        double Y1, Y2;
            //                        if (X[i] < X[j])
            //                        {
            //                            Y1 = Y[i];
            //                            Y2 = Y[j];
            //                        }
            //                        else
            //                        {
            //                            Y2 = Y[i];
            //                            Y1 = Y[j];
            //                        }
            //                        double dx = Xmax - Xmin;
            //                        double N2 = (p.X - Xmin) / dx;
            //                        double N1 = 1 - N2;
            //                        Yb = N1 * Y1 + N2 * Y2;
            //                        r_min = p.Y - Yb;
            //                        break;
            //                    }
            //                }
            //            }
            //        }
            //        distance[nod] = r_min;
            //        Hp[nod] = Math.Max(0.01*H, Y_max - Yb);
            //    }

            //}
            //if (channelSectionForms == СhannelSectionForms.boxСhannelCrossSection)
            //{
            //    // Поиск минимальног растояния до дна
            //    for (int nod = 0; nod < mesh.CountKnots; nod++)
            //    {
            //        double mL = Math.Min(X[nod] - X_min, X_max - X[nod]);
            //        double mH = Y[nod] - Y_min;
            //        distance[nod] = Math.Min( mL, mH );
            //        Hp[nod] = H;
            //    }
            //}

        }
    }
}