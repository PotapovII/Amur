//---------------------------------------------------------------------------
//                         ПРОЕКТ  "Hit"
//                         проектировщик:
//                кодировка C++: 7.12.98 Потапов И.И.
//---------------------------------------------------------------------------
//          реконструкция: переход на статические массивы
//                 кодировка : 22.12.2003 Потапов И.И.
//---------------------------------------------------------------------------
//            изменения: добавлены элементы Кроуза-Равиарта
//                 кодировка : 12.02.2004 Потапов И.И.
//---------------------------------------------------------------------------
//                       ПРОЕКТ  "RiverLib"
//              Перенос на C# : 03.03.2021  Потапов И.И.
//     Убраны Эрмитовы функции формы и весь связынный с ними функционал
//---------------------------------------------------------------------------
namespace MeshLib
{
    using CommonLib;
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Тип функций формы
    /// </summary>
    [Serializable]
    public enum FFContinueType
    {
        DisСontinuous = 0,
        СontinuousAtNodes,
        ContinuousAlongBorders
    }
    /// <summary>
    /// Тип геометрии КЭ 
    /// </summary>
    [Serializable]
    public enum FFGeomType
    {
        NonForm = 0,
        Line,
        Triangle,
        Rectangle,
        All
    }
    /// <summary>
    /// Порядок аппроксимации (по функциям) 
    /// </summary>
    [Serializable]
    public enum ApproxOrder
    {
        NullOrder = 0,
        FirstOrder,
        SecondOrder,
        thirdOrder
    }
    /// <summary>
    /// ОО: функции формы
    /// </summary>
    [Serializable]
    public abstract class AbFunForm : IFForm
    {
        /// <summary>
        /// Порядок сетки на которой работает функция формы
        /// </summary>
        protected IntRange meshRange;
        /// <summary>
        /// Порядок сетки на которой работает функция формы
        /// </summary>
        public IntRange MeshRange { get { return meshRange; } }
        /// <summary>
        /// уникальный идентификатор функции формы
        /// </summary>
        protected TypeFunForm id;
        /// <summary>
        /// идентификатор функции формы
        /// </summary>
        public TypeFunForm ID { get { return id; } }
        /// <summary>
        /// зарезервированная функция формы
        /// </summary>
        protected uint HFF_MixMask = 0;
        /// <summary>
        /// код функции формы используемой для описания геометрии КЭ
        /// </summary>
        protected uint GeomFormID;
        /// <summary>
        /// координаты х КЭ
        /// </summary>
        protected double[] CX;
        /// <summary>
        /// координаты y КЭ
        /// </summary>
        protected double[] CY;
        /// <summary>
        /// Функция формы для базистной геометрии, используемтся при решении
        /// задач со многими неизвестными, каждая из которых может 
        /// использовать свои функции формы
        /// </summary>
        protected AbFunForm GFForm = null;
        /// <summary>
        /// тип функций формы
        /// дискретные 0
        /// непрерывные в узлах 1
        /// непрерывные по границам 2
        /// </summary>
        protected FFContinueType FContinueType;
        /// <summary>
        /// тип геометрии 0, 1,3,4 - количество граней
        /// </summary>
        public FFGeomType GeomType;
        /// <summary>
        /// размерность пространства (1 или 2)
        /// </summary>
        public uint Dim = 1;
        // public uint count { get { return Count; } }
        /// <summary>
        /// количестов узлов в функции формы
        /// </summary>
        public uint Count;
        /// <summary>
        /// название функции формы
        /// </summary>
        public string Name;
        /// <summary>
        /// значение функций формы
        /// </summary>
        public double[] N = null;
        /// <summary>
        /// значение лок.производных по xi от функций формы в точке point
        /// </summary>
        public double[] DN_xi = null;
        /// <summary>
        /// значение лок.производных по eta от функций формы в точке point
        /// </summary>
        public double[] DN_eta = null;
        /// <summary>
        /// значение лок.производных по x от функций формы в точке point
        /// </summary>
        public double[] DN_x = null;
        /// <summary>
        /// значение лок.производных по y от функций формы в точке point
        /// </summary>
        public double[] DN_y = null;
        /// <summary>
        /// величина детерминанта матрицы Якоби
        /// </summary>
        public double DetJ;
        /// <summary>
        /// матрица Якоби
        /// </summary>
        public double[] Jcoby = new double[4];
        /// <summary>
        /// вычисление матрицы Якоби  
        /// </summary>
        protected void Jcob(double[,] BWM = null)
        {
            uint i;

            // преобразование с матрицей Якоби
            switch (Dim)
            {
                case 1:
                    {
                        double a, b;
                        // Вычисление коэффициентов матрицы Якоби
                        //   | a 0 |
                        //   | 0 0 |
                        a = 0; b = 0;
                        if (GeomFormID == HFF_MixMask)
                        {
                            for (i = 0; i < Count; i++)
                            {
                                a += DN_xi[i] * CX[i];
                                b += DN_xi[i] * CY[i];
                            }
                        }
                        else
                        {
                            for (i = 0; i < GFForm.Count; i++)
                            {
                                a += GFForm.DN_xi[i] * CX[i];
                                b += GFForm.DN_xi[i] * CY[i];
                            }
                        }
                        // детерминант матрицы Якоби
                        DetJ = Math.Sqrt(a * a + b * b);
                        for (i = 0; i < Count; i++)
                        {
                            DN_x[i] = (DN_xi[i] / DetJ);
                        }
                    }
                    break;
                case 2:
                    {
                        double a, b, c, d;
                        // Вычисление коэффициентов матрицы Якоби
                        //   | a b |
                        //   | c d |
                        a = 0; b = 0; c = 0; d = 0;
                        if (GeomFormID == HFF_MixMask)
                            for (i = 0; i < Count; i++)
                            {
                                a += DN_xi[i] * CX[i];
                                b += DN_xi[i] * CY[i];
                                c += DN_eta[i] * CX[i];
                                d += DN_eta[i] * CY[i];
                            }
                        else
                            for (i = 0; i < GFForm.Count; i++)
                            {
                                a += GFForm.DN_xi[i] * CX[i];
                                b += GFForm.DN_xi[i] * CY[i];
                                c += GFForm.DN_eta[i] * CX[i];
                                d += GFForm.DN_eta[i] * CY[i];
                            }
                        DetJ = a * d - b * c; // детерминант матрицы Якоби
                                              // Умножение обратной матрицы Якоби на ЛФФ для
                                              // получения глобальных производных
                                              //   | d -b |
                                              //   | -c a |
                        if (BWM == null)
                        {
                            for (i = 0; i < Count; i++)
                            {
                                DN_x[i] = (d * DN_xi[i] - b * DN_eta[i]) / DetJ;
                                DN_y[i] = ((-c * DN_xi[i] + a * DN_eta[i]) / DetJ);
                            }
                        }
                        else
                        {
                            for (i = 0; i < Count; i++)
                            {
                                DN_x[i] = (d * DN_xi[i] * BWM[i, 0] - b * DN_eta[i] * BWM[i, 1]) / DetJ;
                                DN_y[i] = ((-c * DN_xi[i] * BWM[i, 0] + a * DN_eta[i] * BWM[i, 1]) / DetJ);
                            }
                        }
                    }
                    break;
            }
        }
        /// <summary>
        /// Выделение памяти под рабочие массивы
        /// </summary>
        /// <param name="MaxKnots"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Init(uint MaxKnots)
        {
            /// <summary>
            /// значение функций формы
            /// </summary>
            N = new double[MaxKnots];
            /// <summary>
            /// значение лок.производных по xi от функций формы в точке point
            /// </summary>
            DN_xi = new double[MaxKnots];
            /// <summary>
            /// значение лок.производных по eta от функций формы в точке point
            /// </summary>
            DN_eta = new double[MaxKnots]; //  
            /// <summary>
            /// значение лок.производных по x от функций формы в точке point
            /// </summary>
            DN_x = new double[MaxKnots];
            /// <summary>
            /// значение лок.производных по y от функций формы в точке point
            /// </summary>
            DN_y = new double[MaxKnots];
        }

        /// <summary>
        /// Вычисление произведение значений в узлах на функцию формы в точке интегрирования
        /// </summary>
        /// <param name="ff"></param>
        /// <param name="mas"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double FFValue(double[] mas)
        {
            double sum = 0;
            for (int i = 0; i < mas.Length; i++)
                sum += N[i]*mas[i];
            return sum;
        }

        /// <summary>
        /// очистка массивов
        /// </summary>
        protected void Clear()
        {
            for (uint i = 0; i < N.Length; i++)
            {
                N[i] = 0;  //  значение функций формы
                DN_xi[i] = 0;  //  значение лок.производных от
                DN_eta[i] = 0;  //  функций формы в точке point
                DN_x[i] = 0;  //  значение глоб.производных от
                DN_y[i] = 0;  //  функций формы в точке point
            }
        }
        /// <summary>
        /// вычисление глобальных координат по локальым координатам
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="Gx"></param>
        /// <param name="Gy"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CalkGlobaloCoords(double x, double y, ref double Gx, ref double Gy)
        {
            Gx = 0;
            Gy = 0;
            CalkForm(x, y);
            for (uint i = 0; i < Count; i++)
            {
                Gx += N[i] * CX[i];
                Gy += N[i] * CY[i];
            }
        }
        //---------------------------------------------------------------------------
        public void GetJcoby(ref double[] Jcoby)
        {
            for (uint i = 0; i < 4; i++)
                Jcoby[i] = this.Jcoby[i];
        }
        //---------------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetGeometry(uint _Dim, double[] _x, double[] _y, uint type)
        {
            Dim = _Dim;
            CX = _x;
            CY = _y;
            // вычисление кононических (локальных) координат узла
            if (type == 0)
            {
                double x = 0, y = 0;
                for (uint knot = 0; knot < Count; knot++)
                {
                    CalkVertex(knot, ref x, ref y);
                    CX[knot] = x;
                    CY[knot] = y;
                }
            }
            GeomFormID = HFF_MixMask;
        }
        // получение локальных координат узлов
        public void GetGeoCoords(ref double[] gx, ref double[] gy)
        {
            double x = 0, y = 0;
            for (uint knot = 0; knot < Count; knot++)
            {
                CalkVertex(knot, ref x, ref y);
                gx[knot] = x;
                gy[knot] = y;
            }
        }
        /// <summary>
        /// передача глобальных координат узлов
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public virtual void SetGeoCoords(double[] x, double[] y)
        {
            CX = x;
            CY = y;
        }
        /// <summary>
        /// установка геометрии КЭ - реализовать после менеджера функций форм
        /// </summary>
        /// <param name="_Dim"></param>
        /// <param name="_ID"></param>
        /// <param name="_x"></param>
        /// <param name="_y"></param>
        /// <param name="type"></param>
        public void SetIndependGeometry(uint _Dim, uint GFormID, double[] _x, double[] _y, uint type)
        {
            Dim = _Dim;
            CX = _x;
            CY = _y;
            // установка ф.ф. для аппроксимации геометрии
            //if (!GGetFForm == 0)
            //{
            //    GGetFForm = Zaplata;
            //}
            //if (GeomFormID != _ID)
            //{
            //    GeomFormID = _ID;
            //    GFForm = GGetFForm.CreateElem(GeomFormID);
            //}
            //// вычисление кононических (локальных) координат узла
            //if (type == 0 && GFForm == null)
            //{
            //    double x, y;
            //    for (uint knot = 0; knot < GFForm.Count; knot++)
            //    {
            //        GFForm.CalkVertex(knot, &x, &y);
            //        CX[knot] = x;
            //        CY[knot] = y;
            //    }
            //}
            //if (!GFForm == 0) 
            //    GeomFormID = HFF_MixMask;
        }
        /// <summary>
        /// установка геоиетрии КЭ  - реализовать после менеджера функций форм
        /// </summary>
        /// <param name="GFormID"></param>
        /// <param name="dim"></param>
        public void SetGeoOnly(uint GFormID, uint dim)
        {
            Dim = dim;
            GeomFormID = GFormID;
            //// установка менеджера ф.ф. для аппроксимации геометрии
            //if (!GGetFForm == null) GGetFForm = &Zaplata;
            //// получение функции формы для описания геометрии КЭ
            //GFForm = GGetFForm.CreateElem(GeomFormID);
            //// вычисление кононических (локальных) координат узла
            //if (GFormID != HFF_MixMask && GFForm == 0)
            //{
            //    GeomFormID = HFF_MixMask;
            //    // ошибка ??
            //}
        }
        //---------------------------------------------------------------------------
        /// <summary>
        /// вычисление значений функций формы
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public abstract void CalkForm(double x, double y);
        /// <summary>
        /// вычисление значений функций формы ее производных
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public virtual void CalkDiffForm(double x, double y, double[,] BWM = null)
        {
            // преобразование с матрицей Якоби
            if (GeomFormID != HFF_MixMask)
            {
                GFForm.CalkLocalDiffForm(x, y);
                Jcob(BWM);
            }
            else
            {
                CalkLocalDiffForm(x, y);
                Jcob(BWM);
            }
        }

        /// <summary>
        /// вычисление  координат i узла
        /// </summary>
        /// <param name="IdxKnot"></param>
        /// <param name="_x"></param>
        /// <param name="_y"></param>
        public abstract void CalkVertex(uint IdxKnot, ref double _x, ref double _y);

        /// <summary>
        /// вычисление локальных производных функций формы
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public abstract void CalkLocalDiffForm(double x, double y);
        /// <summary>
        /// код функции формы на гранях КЭ
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public abstract uint GetBoundFormType();
    }
}