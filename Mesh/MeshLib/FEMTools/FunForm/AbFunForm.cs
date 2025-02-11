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
//     Добавлен Эрмитовы функции формы и весь связынный с ними функционал
//              Перенос на C# : 27.11.2024  Потапов И.И.
//     Убраны перечисления ApproxOrder поскольку задач
//     по автогенерации составных функций формы для аппроксимации и
//     использования адоесных таблиц пока не предвидится в задачах
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
        /// <summary>
        /// Не согласованные на КЭ функции формы
        /// </summary>
        DisСontinuous = 0,
        /// <summary>
        /// Согласованные в узлах КЭ функции формы
        /// </summary>
        СontinuousAtNodes,
        /// <summary>
        /// Согласованные по рубрам КЭ функции формы
        /// </summary>
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
        All,
    }
    ///// <summary>
    ///// Порядок аппроксимации (по функциям) 
    ///// </summary>
    //[Serializable]
    //public enum ApproxOrder
    //{
    //    NullOrder = 0,
    //    FirstOrder,
    //    SecondOrder,
    //    thirdOrder,
    //    fourthOrder,
    //    fifthOrder
    //}
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
        public FFContinueType FContinueType;
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
        #region C^0
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
        /// значение производных по x от функций формы в точке point
        /// </summary>
        public double[] DN_x = null;
        /// <summary>
        /// значение производных по y от функций формы в точке point
        /// </summary>
        public double[] DN_y = null;
        #endregion
        #region C^1
        /// <summary>
        /// значение лок.производных второго порядка по xi от функций формы в точке point
        /// </summary>
        public double[] DN2xi = null;
        /// <summary>
        /// значение лок.производных второго порядкапо eta от функций формы в точке point
        /// </summary>
        public double[] DN2eta = null;
        /// <summary>
        /// значение смешанных лок.производных второго порядкапо eta от функций формы в точке point
        /// </summary>
        public double[] DNXiEta = null;
        /// <summary>
        /// значение производных второго порядка по x от функций формы в точке point
        /// </summary>
        public double[] DN2x = null;
        /// <summary>
        /// значение производных второго порядка по y от функций формы в точке point
        /// </summary>
        public double[] DN2y = null;
        /// <summary>
        /// значение смешанных производных второго порядка по y от функций формы в точке point
        /// </summary>
        public double[] DNxy = null;
        #endregion
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Jcob(double[,] BWM = null)
        {
            uint i;
            if (id == TypeFunForm.Form_2D_Rectangle_Ermit4)
            {
                double a, b, c, d, x1, x2, m1, m2, y1, y2;
                //    [a     b      0         0         0 ]
                //    [                                   ]
                //    [c     d      0         0         0 ]
                //    [                                   ]
                //    [             2                   2 ]
                //    [x1    x2    a        2 a b      b  ]
                //    [                                   ]
                //    [m1    m2    a c    c b + a d    b d]
                //    [                                   ]
                //    [             2                   2 ]
                //    [y1    y2    c        2 c d      d  ]
                a = 0; b = 0; c = 0; d = 0;
                x1 = 0; x2 = 0; m1 = 0; m2 = 0; y1 = 0; y2 = 0;
                if (GeomFormID == HFF_MixMask)
                {
                    // вариант изопараметрических элементов 
                    for (i = 0; i < Count; i++)
                    {
                        a += DN_xi[i] * CX[i];
                        b += DN_xi[i] * CY[i];
                        //
                        c += DN_eta[i] * CX[i];
                        d += DN_eta[i] * CY[i];
                        //
                        x1 += DN2xi[i] * CX[i];     // d^2 x /d xi^2
                        x2 += DN2xi[i] * CY[i];     // d^2 y /d xi^2
                                                    //
                        m1 += DNXiEta[i] * CX[i];   // d^2 x /d xi d eta
                        m2 += DNXiEta[i] * CY[i];   // d^2 y /d xi d eta
                                                    //
                        y1 += DN2eta[i] * CX[i];    // d^2 x /d eta^2
                        y2 += DN2eta[i] * CY[i];    // d^2 y /d eta^2
                    }
                }
                else
                {
                    // вариант не изопараметрических элементов когда
                    // для описания геометрии КЭ используются функции формы другого вида
                    for (i = 0; i < GFForm.Count; i++)
                    {
                        a += GFForm.DN_xi[i] * CX[i];
                        b += GFForm.DN_xi[i] * CY[i];
                        c += GFForm.DN_eta[i] * CX[i];
                        d += GFForm.DN_eta[i] * CY[i];
                        //
                        x1 += GFForm.DN2xi[i] * CX[i]; // d^2 x /d xi^2
                        x2 += GFForm.DN2xi[i] * CY[i]; // d^2 y /d xi^2
                                                       //
                        m1 += GFForm.DNXiEta[i] * CX[i]; // d^2 x /d xi d eta
                        m2 += GFForm.DNXiEta[i] * CY[i]; // d^2 y /d xi d eta
                                                         //
                        y1 += GFForm.DN2eta[i] * CX[i]; // d^2 x /d eta^2
                        y2 += GFForm.DN2eta[i] * CY[i]; // d^2 y /d eta^2
                    }
                }
                // детерминант матрицы Якоби
                DetJ = a * d - b * c;
                Jcoby[0] = a;
                Jcoby[1] = b;
                Jcoby[2] = c;
                Jcoby[3] = d;
                // Умножение обратной матрицы Якоби на ЛФФ для
                // получения глобальных производных
                for (i = 0; i < Count; i++)
                {
                    double t1 = a * d;
                    double t2 = c * b;
                    double ADetJ = 1 / (t1 - t2);
                    double t15 = d * d;
                    double t16 = t15 * d;
                    double t21 = t15 * c;
                    double t23 = b * d;
                    double t24 = c * m2;
                    double t27 = b * b;
                    double t28 = d * t27;
                    double t30 = t27 * c;
                    double t33 = a * a;
                    double t34 = t33 * a;
                    double t36 = t33 * t15;
                    double t39 = c * c;
                    double t40 = t39 * t27;
                    double t43 = t39 * c;
                    double t44 = t27 * b;
                    double t47 = 1 / (t34 * t16 - 3.0 * t2 * t36 + 3.0 * t40 * t1 - t43 * t44);
                    double t50 = t15 * a;
                    double t59 = a * t27;
                    double t65 = a * c;
                    double t69 = 1 / (t36 - 2.0 * t65 * t23 + t40);
                    double t72 = t69 * DNXiEta[i];
                    double t80 = y1 * b;
                    double t83 = d * t39;
                    double t85 = c * m1;
                    double t89 = b * t39;
                    double t94 = d * t33;
                    double t98 = c * d;
                    //
                    DN_x[i] = (d * DN_xi[i] - b * DN_eta[i]) * ADetJ;
                    DN_y[i] = (-c * DN_xi[i] + a * DN_eta[i]) * ADetJ;

                    DN2x[i] = (-x1 * t16 + 2.0 * t15 * m1 * b + t21 * x2 - 2.0 * t23 * t24 - t28 * y1 + t30 * y2) * t47 * DN_xi[i]
                    - (t50 * x2 - t15 * x1 * b - 2.0 * t1 * b * m2 + 2.0 * t28 * m1 + t59 * y2 - t44 * y1) * t47 * DN_eta[i] +
                    t15 * t69 * DN2xi[i] - 2.0 * t23 * t72 + t27 * t69 * DN2eta[i];

                    DNxy[i] = -(t50 * m1 - t21 * x1 - t1 * t80 - t1 * t24 + t83 * x2 + t23 * t85 + t65 * b * y2 - t89 * m2) * t47
                    * DN_xi[i] + (-t94 * m2 + t1 * m1 * b + t98 * a * x2 - d * x1 * t2 + t33 * b * y2 - t59 * y1 - t2 * m2 * a + t30 * m1) *
                    t47 * DN_eta[i] - t98 * t69 * DN2xi[i] + (t2 + t1) * t69 * DNXiEta[i] - a * b * t69 * DN2eta[i];

                    DN2y[i] = (-t94 * y1 + 2.0 * t1 * t85 - t83 * x1 + t33 * c * y2 - 2.0 * a * t39 * m2 + t43 * x2) * t47 * DN_xi[i]
                    - (y2 * t34 - 2.0 * t24 * t33 - t80 * t33 + t39 * x2 * a + 2.0 * t2 * a * m1 - t89 * x1) * t47 * DN_eta[i] +
                    t39 * t69 * DN2xi[i] - 2.0 * t65 * t72 + t33 * t69 * DN2eta[i];
                }
            }
            else
            {
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
            DN_y = new double[MaxKnots];
            /// <summary>
            /// значение лок.производных по y от функций формы в точке point
            /// </summary>
            if (id == TypeFunForm.Form_2D_Rectangle_Ermit4)
            {
                N = new double[4*Count];
                DN2xi = new double[Count];
                DN2eta = new double[Count];
                DNXiEta = new double[Count];
                DN2x = new double[Count];
                DN2y = new double[Count];
                DNxy = new double[Count];
            }
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void CalkForm(double x, double y);
        /// <summary>
        /// вычисление значений функций формы ее производных
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void CalkVertex(uint IdxKnot, ref double _x, ref double _y);

        /// <summary>
        /// вычисление локальных производных функций формы
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void CalkLocalDiffForm(double x, double y);
        /// <summary>
        /// код функции формы на гранях КЭ
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract uint GetBoundFormType();
    }
}