//---------------------------------------------------------------------------
//                     Прототип: код Снигур К.С.
//                         релиз 26 06 2017 
//---------------------------------------------------------------------------
//         интеграция в MeshLib 13-22 08 2022 : Потапов И.И.
//---------------------------------------------------------------------------
//         + изменений  KsiWrapper 19 11 2024 : Королева К.С (Снигур К.С.)
//---------------------------------------------------------------------------

namespace MeshLib
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    
    using MemLogLib;
    using CommonLib;

    [Serializable]
    public class KsiMesh : ComplecsMesh
    {
        #region Поля расширения
        /// <summary>
        /// идентификатор структуры сетки: 0 - треугольная, 1 - смешанная, 2 - четырехугольная
// ==>  /// ==> замена на typeMesh tMesh, у меня реализация только triangle
        /// </summary>
        //public int StructureIndex;
        /// <summary>
        /// Координаты сетки по X
        /// </summary>
        public double[] X { get => CoordsX; set =>CoordsX = value; } 
        /// <summary>
        /// Координаты сетки по Y
        /// </summary>
        public double[] Y { get => CoordsY; set => CoordsY = value; }
        ///// <summary>
        /// <summary>
        /// Узлы левой границы
        /// </summary>
        public int[] LeftKnots;
        /// <summary>
        /// Количество узлов левой границы 
        /// </summary>
        public int CountLeft;
        /// <summary>
        /// Узлы правой границы
        /// </summary>
        public int[] RightKnots;
        /// <summary>
        /// Количество узлов правой границы 
        /// </summary>
        public int CountRight;
        /// <summary>
        /// Узлы верхней границы
        /// </summary>
        public int[] TopKnots;
        /// <summary>
        /// Количество узлов верхней границы 
        /// </summary>
        public int CountTop;
        /// Узлы нижней границы
        /// </summary>
        public int[] BottomKnots;
        /// <summary>
        /// Количество узлов нижней границы 
        /// </summary>
        public int CountBottom;
        //
        public string error = "";
        //
        //public double[] Sx, Sy;
        /// <summary>
        /// Шаблон перенумерации (для решулярной сетки)
        /// </summary>
        int[] Numbers = null;
        #endregion

        #region Конструкторы
        public KsiMesh() : base() { }
        public KsiMesh(KsiMesh m) : base(m)
        {
            CountLeft = m.CountLeft;
            CountRight = m.CountRight;
            CountTop = m.CountTop;
            CountBottom = m.CountBottom;

            
            MEM.MemCopy(ref LeftKnots, m.LeftKnots);
            MEM.MemCopy(ref TopKnots, m.TopKnots);
            MEM.MemCopy(ref RightKnots, m.RightKnots);
            MEM.MemCopy(ref BottomKnots, m.BottomKnots);

            //MEM.MemCopy(ref CPLeft, m.CPLeft, "KsiMesh.CPLeft");
            //MEM.MemCopy(ref CPRight, m.CPRight, "KsiMesh.CPRight");
            //MEM.MemCopy(ref CPTop, m.CPTop, "KsiMesh.CPTop");
            //MEM.MemCopy(ref CPBottom, m.CPBottom, "KsiMesh.CPBottom");

            //MEM.MemCopy(ref BottomTriangles, m.BottomTriangles);

        }
        public KsiMesh(ComplecsMesh m) : base(m)
        {
            bool surf_flag = false;
            bool structureChanged = true;
            List<int>[] bknots = new List<int>[4];
            for (int i = 0; i < bknots.Length; i++)
                bknots[i] = new List<int>();
            for (int i = 0; i < m.CountBoundElements; i++)
            {
                int mark = m.BoundElementsMark[i];
                bknots[mark].Add((int)m.BoundElems[i][0]);
                bknots[mark].Add((int)m.BoundElems[i][1]);
            }
            BottomKnots = bknots[0].Distinct().ToArray();
            RightKnots = bknots[1].Distinct().ToArray();
            TopKnots = bknots[2].Distinct().ToArray();
            LeftKnots = bknots[3].Distinct().ToArray();
            //
            CountBottom = BottomKnots.Length;
            CountRight = RightKnots.Length;
            CountTop = TopKnots.Length;
            CountLeft = LeftKnots.Length;
            //            
        }
        public KsiMesh(int NX, int NY, double[] X, double[] Y)
        {
            try
            {
                // количество элементов
                int NE = (NY - 1) * (NX - 1) * 2;
                // создание карты
                uint[][] map = new uint[NY][];
                uint e = 0;
                for (int i = 0; i < NY; i++)
                {
                    map[i] = new uint[NX];
                    for (int j = 0; j < NX; j++)
                        map[i][j] = e++;
                }
                AreaElemsFFType = new TypeFunForm[NE];
                // генерация треугольной сетки
               AreaElems = new uint[NE][]; // [NE, 3];
                for (int i = 0; i < NE; i++)
                {
                    AreaElems[i] = new uint[3];
                    AreaElemsFFType[i] = TypeFunForm.Form_2D_Triangle_L1;
                }
                this.X = X;
                this.Y = Y;
                
                e = 0;
                uint a, b, c, d;
                //for (int i = 0; i < NY - 3; i++)
                //!! не забыть закомменить флаг неизменности структуры сетки в Form BackgroundWork
                for (int i = 0; i < NY - 1; i++)
                    for (int j = 0; j < NX - 1; j++)
                    {
                        a = map[i][j];
                        b = map[i][j + 1];
                        c = map[i + 1][j + 1];
                        d = map[i + 1][j];

                        //double Lac = (X[a] - X[c]) * (X[a] - X[c]) +
                        //                (Y[a] - Y[c]) * (Y[a] - Y[c]);

                        //double Lbd = (X[b] - X[d]) * (X[b] - X[d]) +
                        //                (Y[b] - Y[d]) * (Y[b] - Y[d]);
                        ////
                        //if ((Lac / Lbd > 0.9999) && (Lac / Lbd < 1.0001))
                        //{
                        //    if (j % 2 == j % 2 * 2)
                        //    {
                        //        m.AreaElems[e][0] = a;
                        //        m.AreaElems[e][1] = c;
                        //        m.AreaElems[e][2] = b;
                        //        e++;
                        //        //
                        //        m.AreaElems[e][0] = a;
                        //        m.AreaElems[e][1] = d;
                        //        m.AreaElems[e][2] = c;
                        //        e++;
                        //    }
                        //    else
                        //    {
                        //        m.AreaElems[e][0] = a;
                        //        m.AreaElems[e][1] = d;
                        //        m.AreaElems[e][2] = b;
                        //        e++;
                        //        //
                        //        m.AreaElems[e][0] = b;
                        //        m.AreaElems[e][1] = d;
                        //        m.AreaElems[e][2] = c;
                        //        e++;
                        //    }
                        //}
                        //else if (Lac < Lbd)
                        //{
                        //    m.AreaElems[e][0] = a;
                        //    m.AreaElems[e][1] = c;
                        //    m.AreaElems[e][2] = b;
                        //    e++;
                        //    //
                        //    m.AreaElems[e][0] = a;
                        //    m.AreaElems[e][1] = d;
                        //    m.AreaElems[e][2] = c;
                        //    e++;
                        //}
                        //else
                        //{
                            AreaElems[e][0] = a;
                            AreaElems[e][1] = d;
                            AreaElems[e][2] = b;
                            e++;
                            //
                            AreaElems[e][0] = b;
                            AreaElems[e][1] = d;
                            AreaElems[e][2] = c;
                            e++;
                        //}
                    }
                //// сетка с регулярными КО по дну
                //for (int i = NY - 3; i < NY - 2; i++)
                //    for (int j = 0; j < NX - 1; j++)
                //    {
                //        a = map[i][j];
                //        b = map[i][j + 1]; ;
                //        c = map[i + 1][j + 1];
                //        d = map[i + 1][j];
                //        //
                //        m.AreaElems[e][0] = a;
                //        m.AreaElems[e][1] = c;
                //        m.AreaElems[e][2] = b;
                //        e++;
                //        //
                //        m.AreaElems[e][0] = a;
                //        m.AreaElems[e][1] = d;
                //        m.AreaElems[e][2] = c;
                //        e++;

                //    }
                //for (int i = NY - 2; i < NY - 1; i++)
                //    for (int j = 0; j < NX - 1; j++)
                //    {
                //        a = map[i][j];
                //        b = map[i][j + 1]; ;
                //        c = map[i + 1][j + 1];
                //        d = map[i + 1][j];
                //        //
                //        m.AreaElems[e][0] = a;
                //        m.AreaElems[e][1] = d;
                //        m.AreaElems[e][2] = b;
                //        e++;
                //        //
                //        m.AreaElems[e][0] = b;
                //        m.AreaElems[e][1] = d;
                //        m.AreaElems[e][2] = c;
                //        e++;

                //    }
                // генерация массивов граничных узлов
                LeftKnots = new int[NY];
                RightKnots = new int[NY];
                TopKnots = new int[NX];
                BottomKnots = new int[NX];
                CountLeft = NY;
                CountRight = NY;
                CountTop = NX;
                CountBottom = NX;
                for (int i = 0; i < NY; i++)
                {
                    LeftKnots[i] = (int)map[i][0];
                    RightKnots[NY - i - 1] = (int)map[i][NX - 1];
                }
                for (int j = 0; j < NX; j++)
                {
                    TopKnots[NX - j - 1] = (int)map[0][j];
                    BottomKnots[j] = (int)map[NY - 1][j];
                }
                //
                map = null;
            }
            catch (Exception ex)
            {
                error += ex.Message.ToString();
            }

        }
        #endregion
        /// <summary>
        /// Клонирование объекта сетки
        /// </summary>
        public override IMesh Clone()
        {
            return new KsiMesh(this);
        }
        /// <summary>
        /// конвертация сетки в TriMesh, по умолчанию первый порядок точности аппроксимации на элементе
        /// </summary>
        /// <returns></returns>
        public TriMesh ConvertToTriMesh()
        {
            int ch = 0, i = 0;
            TriMesh triMesh = new TriMesh();
            //
            triMesh.tRangeMesh = TypeRangeMesh.mRange1;
            triMesh.tMesh = TypeMesh.Triangle;
            // копирование координат
            MEM.MemCopy(ref triMesh.CoordsX, X);
            MEM.MemCopy(ref triMesh.CoordsY, Y);
            // копирование элементов
            triMesh.AreaElems = new TriElement[AreaElems.Length];
            for (i = 0; i < AreaElems.Length; i++)
                triMesh.AreaElems[i] = new TriElement(AreaElems[i][0], AreaElems[i][1], AreaElems[i][2]);
            //копирование граничных элементов
            triMesh.BoundElems = new TwoElement[2 * (CountBottom + CountLeft - 2)];
            triMesh.BoundElementsMark = new int[2 * (CountBottom + CountLeft - 2)];
            ch = 0;
            //
            for (i = 0; i < CountLeft - 1; i++)
            {
                triMesh.BoundElems[ch] = new TwoElement(i, i + 1);
                triMesh.BoundElementsMark[ch++] = 3;
            }
            for (i = 0; i < CountTop - 1; i++)
            {
                triMesh.BoundElems[ch] = new TwoElement(CountLeft + i * CountLeft - 1, CountLeft + (i + 1) * CountLeft - 1);
                triMesh.BoundElementsMark[ch++] = 2;
            }
            for (i = 0; i < CountRight - 1; i++)
            {
                triMesh.BoundElems[ch] = new TwoElement(CountKnots - CountLeft + i, CountKnots - CountLeft + i + 1);
                triMesh.BoundElementsMark[ch++] = 1;
            }
            for (i = 0; i < CountBottom - 1; i++)
            {
                triMesh.BoundElems[ch] = new TwoElement(i * CountLeft, (i + 1) * CountLeft);
                triMesh.BoundElementsMark[ch++] = 0;
            }
            //копирование граничных узлов
            triMesh.BoundKnots = new int[2 * (CountLeft + CountBottom)];
            triMesh.BoundKnotsMark = new int[2 * (CountLeft + CountBottom)];
            ch = 0;
            for (i = 0; i < CountLeft; i++)
            {
                triMesh.BoundKnots[ch] = i;
                triMesh.BoundKnotsMark[ch++] = 3;
            }
            for (i = 0; i < CountTop; i++)
            {
                triMesh.BoundKnots[ch] = i * CountLeft - 1;
                triMesh.BoundKnotsMark[ch++] = 2;
            }
            for (i = 0; i < CountRight; i++)
            {
                triMesh.BoundKnots[ch] = CountKnots - CountLeft + i;
                triMesh.BoundKnotsMark[ch++] = 1;
            }
            for (i = 0; i < CountBottom; i++)
            {
                triMesh.BoundKnots[ch] = i * CountLeft;
                triMesh.BoundKnotsMark[ch++] = 0;
            }
            //
            return triMesh;
        }
        #region Методы

        /// <summary>
        /// выдет номер треугольника, в который попадает заданная точка
        /// </summary>
        /// <param name="x">x-координата точки</param>
        /// <param name="y">y-координата точки</param>
        /// <returns></returns>
        public int GetTriangle(double x, double y)
        {
            for (int k = 0; k < AreaElems.Length; k++)
            {
                uint[] Knots = AreaElems[k];
                double[] Xk = { X[Knots[0]], X[Knots[1]], X[Knots[2]] };
                double[] Yk = { Y[Knots[0]], Y[Knots[1]], Y[Knots[2]] };
                //
                if ((x > Xk.Min()) && (x < Xk.Max()))
                {
                    if ((y > Yk.Min()) && (y < Yk.Max()))
                    {
                        return k;
                    }

                }
            }
            return -1;
        }
        ////----- есть аналог работающий ComplexMesh.GetWidthMatrix
        ///// <summary>
        ///// Выдает ширину ленты для ленточного решател СЛАУ по Гауссу
        ///// </summary>
        ///// <returns></returns>
        //protected int BandWidth()
        //{
        //    int width = 0;
        //    int min = 0, max = 0;
        //    for (int fe = 0; fe < CountElements; fe++)
        //    {
        //        uint[] Knots = AreaElems[fe];
        //        max = (int)Knots.Max();
        //        min = (int)Knots.Min();
        //        width = Math.Max(width, max - min);
        //    }
        //    return width + 1;
        //}

        /// <summary>
        /// Метод перенумерации сетки
        /// </summary>
        public void Renumberation()
        {
            int CountKnot = X.Length;
            //if (Numbers==null)
            //    Numbers = RenumberationTemplate();
            //
            //приводим нумерацию снизу вверх от левого нижнего угла
            //к виду нумерации слева направо от левого верхнего узла
            Numbers = new int[CountKnot];
            int ch = 0;
            for (int i = 0; i < CountLeft; i++)
                for (int j = 0; j < CountBottom; j++)
                    Numbers[ch++] = CountLeft * (j + 1) - 1 - i;

            double[] tmpX = new double[CountKnot];
            for (int i = 0; i < CountKnot; i++)
                tmpX[i] = X[i];
            //
            double[] tmpY = new double[CountKnot];
            for (int i = 0; i < CountKnot; i++)
                tmpY[i] = Y[i];
            //
            for (int i = 0; i < CountKnot; i++)
            {
                int n = Numbers[i];
                X[n] = tmpX[i];
                Y[n] = tmpY[i];
            }
            tmpX = null;
            tmpY = null;
            // изменяем нумерацию для всей остальной структуры
            StructureChanging(Numbers);
            Numbers = null;


        }
        private void StructureChanging(int[] NewNumb)
        {
            
            // **************** Создание нового массива обхода ******************
            // перебор по всем КЭ второй сетки
            for (int i = 0; i < AreaElems.Length; i++)
            {
                // перенумерация
                for (int j = 0; j < AreaElems[i].Length; j++)
                {
                    uint old = AreaElems[i][j];
                    AreaElems[i][j] = (uint) NewNumb[old];
                }
            }
            //for (int i = 0; i < BoundElems.Length; i++)
            //{
            //    // перенумерация
            //    for (int j = 0; j < BoundElems[i].Length; j++)
            //    {
            //        uint old = BoundElems[i][j];
            //        BoundElems[i][j] = (uint)NewNumb[old];
            //    }
            //}
            //for (int j = 0; j < BoundKnots.Length; j++)
            //{
            //    int old = BoundKnots[j];
            //    BoundKnots[j] = NewNumb[old];
            //}
            //****************  Граничные узлы  ***********************
            if (LeftKnots != null)
                for (int i = 0; i < LeftKnots.Length; i++)
                {
                    //BoundKnots[i].Knot = NewNumb[BoundKnots[i].Knot];
                    int old = LeftKnots[i];
                    LeftKnots[i] = NewNumb[old];
                    int ew = LeftKnots[i];
                    //
                    int old2 = RightKnots[i];
                    RightKnots[i] = NewNumb[old2];
                    int ew1 = RightKnots[i];
                }
            //
            if (BottomKnots != null)
                for (int i = 0; i < BottomKnots.Length; i++)
                {
                    //BoundKnots[i].Knot = NewNumb[BoundKnots[i].Knot];
                    int old = BottomKnots[i];
                    BottomKnots[i] = NewNumb[old];
                    int ew = BottomKnots[i];
                    //
                    int old2 = TopKnots[i];
                    TopKnots[i] = NewNumb[old2];
                    int ew1 = TopKnots[i];
                }
        }
        
        /// <summary>
        /// поблочное соединение равных четырехугольных сеток, образующее четырехугольник (четырехугольник + четырехугольник)
        /// </summary>
        /// <param name="NMesh"></param>
        public void Add(KsiMesh NMesh)
        {
            double Eps = 0.00001; // точность float 6-7 знаков после запятой
            //
            // создание временного массива для хранения
            int[] Conform = new int[NMesh.CountKnots];
            bool[] Check = new bool[NMesh.CountKnots];
            int aPoint, bPoint, idx = 0;
            for (int i = 0; i < NMesh.CountKnots; i++)
            {
                Check[i] = true; Conform[i] = i;
            }
            #region Попарное сравнивнение левых граней с правыми и верхние с нижними двух сеток
            //определение общей границы левая-правая, правая-левая
            bool LR = false, RL = false, TB = false, BT = false;
            for (int i = 0; i < CountLeft; i++)
            {
                aPoint = LeftKnots[i];
                for (int j = 0; j < NMesh.CountRight; j++)
                {
                    bPoint = NMesh.RightKnots[j];
                    //
                    if (Math.Abs(X[aPoint] - NMesh.X[bPoint]) < Eps
                        && Math.Abs(Y[aPoint] - NMesh.Y[bPoint]) < Eps)
                    {
                        Conform[bPoint] = aPoint;
                        Check[bPoint] = false;
                        idx++;
                        break;
                    }
                    if (idx > 3)
                        LR = true;
                }
            }
            if (idx < 2)
            {
                for (int i = 0; i < CountRight; i++)
                {
                    aPoint = RightKnots[i];
                    for (int j = 0; j < NMesh.CountLeft; j++)
                    {
                        bPoint = NMesh.LeftKnots[j];
                        //
                        if (Math.Abs(X[aPoint] - NMesh.X[bPoint]) < Eps
                            && Math.Abs(Y[aPoint] - NMesh.Y[bPoint]) < Eps)
                        {
                            Conform[bPoint] = aPoint;
                            Check[bPoint] = false;
                            idx++;
                            break;
                        }
                        if (idx > 3)
                            RL = true;
                    }
                }
            }
            if (idx < 2)
            {
                for (int i = 0; i < CountTop; i++)
                {
                    aPoint = TopKnots[i];
                    for (int j = 0; j < NMesh.CountBottom; j++)
                    {
                        bPoint = NMesh.BottomKnots[j];
                        //
                        if (Math.Abs(X[aPoint] - NMesh.X[bPoint]) < Eps
                            && Math.Abs(Y[aPoint] - NMesh.Y[bPoint]) < Eps)
                        {
                            Conform[bPoint] = aPoint;
                            Check[bPoint] = false;
                            idx++;
                            break;
                        }
                        if (idx > 3)
                            TB = true;
                    }
                }
            }
            if (idx < 2)
            {
                for (int i = 0; i < CountBottom; i++)
                {
                    aPoint = BottomKnots[i];
                    for (int j = 0; j < NMesh.CountTop; j++)
                    {
                        bPoint = NMesh.TopKnots[j];
                        //
                        if (Math.Abs(X[aPoint] - NMesh.X[bPoint]) < Eps
                            && Math.Abs(Y[aPoint] - NMesh.Y[bPoint]) < Eps)
                        {
                            Conform[bPoint] = aPoint;
                            Check[bPoint] = false;
                            idx++;
                            break;
                        }
                        if (idx > 3)
                            BT = true;
                    }
                }
            }
            #endregion
            //если нет общих граней, то выходим из метода
            if (idx < 2)
                return;
            //
            //Если сетка соединяется по горизонтали, то меняем нумерацию
            // чтобы она шла построчно по слитой сетке, а не по секторам
            int[] ConformL = new int[CountKnots]; // отображение для левой сетки
            int NumberCount = 0;
            // граничные КО и КЭ находятся по результирующей сетке
            //
            #region Формирование массивов граничных узлов и перенумерация структуры
            if (LR || RL)
            {
                KsiMesh MeshLeft, MeshRight;
                if (LR)
                {
                    MeshLeft = NMesh;
                    MeshRight = this;
                }
                else
                {
                    MeshLeft = this;
                    MeshRight = NMesh;
                }
                //
                // нумерация левой половины сетки
                int Nx = CountBottom + NMesh.CountBottom - 1;
                int ch = 0;
                for (int i = 0; i < CountLeft; i++)
                    for (int j = 0; j < CountBottom; j++)
                    {
                        ConformL[ch++] = Nx * i + j;
                        NumberCount++;
                    }
                //применение отображения к структуре элементов и граничных узлов
                Conforming(MeshLeft, ConformL);
                //нумерация правой половины сетки
                ch = 0;
                for (int i = 0; i < NMesh.CountLeft; i++)
                    for (int j = 0; j < NMesh.CountBottom; j++)
                    {
                        if (Check[ch] == true)
                        {
                            Conform[ch] = Nx * i + CountBottom + j - 1;
                            NumberCount++;
                        }
                        else
                        {
                            Conform[ch] = ConformL[Conform[ch]];
                        }
                        ch++;
                    }
                //применение отображения к структуре элементов и граничных узлов
                Conforming(MeshRight, Conform);
                //формирование массивов левой и правой границы
                for (int i = 0; i < CountLeft; i++)
                {
                    LeftKnots[i] = MeshLeft.LeftKnots[i];
                    RightKnots[i] = MeshRight.RightKnots[i];
                }
                // формирование массивов узлов нижней и верхней границы
                int[] tmpBottom = new int[MeshLeft.CountBottom + MeshRight.CountBottom - 1];
                int[] tmpTop = new int[MeshLeft.CountTop + MeshRight.CountTop - 1];
                for (int i = 0; i < MeshLeft.CountBottom; i++)
                {
                    tmpBottom[i] = MeshLeft.BottomKnots[i];
                }
                int e = MeshLeft.CountBottom;
                for (int i = 1; i < MeshRight.CountBottom; i++)
                {
                    tmpBottom[e++] = MeshRight.BottomKnots[i];
                }
                for (int i = 0; i < MeshRight.CountTop; i++)
                {
                    tmpTop[i] = MeshRight.TopKnots[i];
                }
                e = MeshRight.CountTop;
                for (int i = 1; i < MeshLeft.CountTop; i++)
                {
                    tmpTop[e++] = MeshLeft.TopKnots[i];
                }
                this.BottomKnots = tmpBottom;
                this.CountBottom = tmpBottom.Length;
                this.TopKnots = tmpTop;
                this.CountTop = tmpTop.Length;
                //
                // перенумерация координат
                double[] tmpX = new double[NumberCount];
                double[] tmpY = new double[NumberCount];
                for (int i = 0; i < CountKnots; i++)
                {
                    tmpX[ConformL[i]] = MeshLeft.X[i];
                    tmpY[ConformL[i]] = MeshLeft.Y[i];
                }
                int kn = CountKnots;
                for (uint i = 0; i < NMesh.CountKnots; i++)
                    if (Check[i] == true)
                    {
                        tmpX[Conform[i]] = MeshRight.X[i];
                        tmpY[Conform[i]] = MeshRight.Y[i];
                    }
                X = tmpX; Y = tmpY;
            }
            //если верхняя и нижняя границы совпадают
            if (TB || BT)
            {
                KsiMesh MeshTop, MeshBottom;
                if (BT)
                {
                    MeshTop = this;
                    MeshBottom = NMesh;
                }
                else
                {
                    MeshTop = NMesh;
                    MeshBottom = this;
                }
                //
                NumberCount = 0;
                // отображение для верхней сетки
                for (uint i = 0; i < MeshTop.CountKnots; i++)
                    ConformL[i] = NumberCount++;
                //применение отображения к структуре элементов и граничных узлов
                Conforming(MeshTop, ConformL);
                // отображение для нижней сетки
                for (uint i = 0; i < MeshBottom.CountKnots; i++)
                {
                    if (Check[i] == true)
                    {
                        Conform[i] = NumberCount;
                        NumberCount++;
                    }
                    else
                    {
                        Conform[i] = ConformL[Conform[i]];
                    }
                }
                // применение отображения к структуре элементов и граничных узлов
                Conforming(MeshBottom, Conform);
                // формирование массивов узлов верхней и нижней границы
                for (int i = 0; i < CountTop; i++)
                {
                    TopKnots[i] = MeshTop.TopKnots[i];
                    BottomKnots[i] = MeshBottom.BottomKnots[i];
                }
                // формирование массивов узлов левой и правой границы
                int[] tmpLeft = new int[MeshTop.CountLeft + MeshBottom.CountLeft - 1];
                int[] tmpRight = new int[MeshTop.CountRight + MeshBottom.CountRight - 1];
                for (int i = 0; i < MeshTop.CountLeft; i++)
                {
                    tmpLeft[i] = MeshTop.LeftKnots[i];
                }
                int e = MeshTop.CountLeft;
                for (int i = 1; i < MeshBottom.CountLeft; i++)
                {
                    tmpLeft[e++] = MeshBottom.LeftKnots[i];
                }
                for (int i = 0; i < MeshBottom.CountRight; i++)
                {
                    tmpRight[i] = MeshBottom.RightKnots[i];
                }
                e = MeshBottom.CountRight;
                for (int i = 1; i < MeshTop.CountRight; i++)
                {
                    tmpRight[e++] = MeshTop.RightKnots[i];
                }
                this.LeftKnots = tmpLeft;
                this.CountLeft = tmpLeft.Length;
                this.RightKnots = tmpRight;
                this.CountRight = tmpRight.Length;
                //
                // перенумерация координат
                double[] tmpX = new double[NumberCount];
                double[] tmpY = new double[NumberCount];
                for (int i = 0; i < CountKnots; i++)
                {
                    tmpX[i] = MeshTop.X[i];
                    tmpY[i] = MeshTop.Y[i];
                }
                int kn = CountKnots;
                for (uint i = 0; i < NMesh.CountKnots; i++)
                    if (Check[i] == true)
                    {
                        tmpX[kn] = MeshBottom.X[i];
                        tmpY[kn++] = MeshBottom.Y[i];
                    }
                X = tmpX; Y = tmpY;
            }
            #endregion
            //
            //расширение массива конечных элементов
            int fe = 0;
            uint[][] tmpAreaElems = new uint[CountElements + NMesh.CountElements][];
            for (uint i = 0; i < AreaElems.Length; i++)
            {
                tmpAreaElems[fe] = new uint[3];
                tmpAreaElems[fe][0] = AreaElems[i][0];
                tmpAreaElems[fe][1] = AreaElems[i][1];
                tmpAreaElems[fe++][2] = AreaElems[i][2];
            }
            for (uint i = 0; i < NMesh.AreaElems.Length; i++)
            {
                tmpAreaElems[fe] = new uint[3];
                tmpAreaElems[fe][0] = NMesh.AreaElems[i][0];
                tmpAreaElems[fe][1] = NMesh.AreaElems[i][1];
                tmpAreaElems[fe++][2] = NMesh.AreaElems[i][2];
            }
            AreaElems = tmpAreaElems;
            //CountElements = tmpAreaElems.Length;
            //CountKnots = X.Length;
        }
        /// <summary>
        /// Метод переопределяет нумерацию одной их сливаемых сеток
        /// </summary>
        /// <param name="NMesh">Сетка, в которой нужно поменять нумерацию</param>
        /// <param name="Conform">Массив нумерации, которая должна бытьт в итоге на сетке NMesh</param>
        private static void Conforming(KsiMesh NMesh, int[] Conform)
        {
            // перебор по всем КЭ второй сетки и исправление номеров узлов
            int CountTwoFE = NMesh.AreaElems.Length;
            for (uint i = 0; i < CountTwoFE; i++)
                for (int j = 0; j < 3; j++)
                    NMesh.AreaElems[i][j] = (uint)Conform[NMesh.AreaElems[i][j]];
            //
            //перенумерация граничных узлов
            for (int i = 0; i < NMesh.CountLeft; i++)
                NMesh.LeftKnots[i] = Conform[NMesh.LeftKnots[i]];
            for (uint i = 0; i < NMesh.CountBottom; i++)
                NMesh.BottomKnots[i] = Conform[NMesh.BottomKnots[i]];
            for (int i = 0; i < NMesh.CountRight; i++)
                NMesh.RightKnots[i] = Conform[NMesh.RightKnots[i]];
            for (int i = 0; i < NMesh.CountTop; i++)
                NMesh.TopKnots[i] = Conform[NMesh.TopKnots[i]];
        }
        //

        
        #endregion
    }
}
