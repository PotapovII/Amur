//---------------------------------------------------------------------------
//                         ПРОЕКТ  "Hit"
//                         проектировщик:
//                кодировка C++: 7.12.98 Потапов И.И.
//---------------------------------------------------------------------------
//          реконструкция: переход на статические массивы
//                 кодировка : 22.12.2003 Потапов И.И.
//---------------------------------------------------------------------------
//                       ПРОЕКТ  "RiverLib"
//              Перенос на C# : 04.03.2021  Потапов И.И.
//---------------------------------------------------------------------------
//  TO DO:
//  выполнить оптимизацию по довычислению коэффициентов в функциях формы
//---------------------------------------------------------------------------

namespace MeshLib
{
    using System;
    using System.Runtime.CompilerServices;
    using CommonLib;
    /// <summary>
    /// Четырехугольные ф.ф., 1-х узловые, 0 порядок
    /// </summary>
    [Serializable]
    class HFForm2D_RectangleConst : AbFunForm
    {
        public HFForm2D_RectangleConst()
        {
            FContinueType = FFContinueType.DisСontinuous;
            GeomType = FFGeomType.Rectangle;
            id = TypeFunForm.Form_2D_Rectangle_L0;
            meshRange = IntRange.intRange1;
            Dim = 2;
            Count = 1;
            Name = "Четырехугольные ф.ф., 1-х узловые, 0 порядок";
            Init(Count);
        }
        // вычисление значений функций формы
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkForm(double x, double y)
        {
            N[0] = 1;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkLocalDiffForm(double x, double y)
        {
            DN_xi[0] = 0;
            DN_eta[0] = 0;
        }
        //---------------------------------------------------------------------------
        // вычисление  координат i узла
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkVertex(uint IdxKnot, ref double _x, ref double _y)
        {
            _x = 0;
            _y = 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override uint GetBoundFormType() { return 0; }
    }
    //---------------------------------------------------------------------------
    /// <summary>
    /// Четырехугольные ф.ф., 4-х узловые, 1 порядок, Лагранж
    /// </summary>
    [Serializable]
    class HFForm2D_RectangleL1 : AbFunForm
    {
        double[] x0 = { -1, 1, 1, -1 };
        double[] y0 = { -1, -1, 1, 1 };
        public HFForm2D_RectangleL1()
        {
            FContinueType = FFContinueType.ContinuousAlongBorders;
            GeomType = FFGeomType.Rectangle;
            id = TypeFunForm.Form_2D_Rectangle_L1;
            meshRange = IntRange.intRange1;
            Dim = 2;
            Count = 4;
            Name = "Четырехугольные ф.ф., 4-х узловые, 1 порядок, Лагранж";
            Init(Count);
        }
        // вычисление значений функций формы
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkForm(double x, double y)
        {
            for (uint i = 0; i < Count; i++)
                N[i] = 0.25 * (1 + x0[i] * x) * (1 + y0[i] * y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkLocalDiffForm(double x, double y)
        {
            for (uint i = 0; i < Count; i++)
            {
                DN_xi[i] = 0.25 * (1 + y0[i] * x) * x0[i];
                DN_eta[i] = 0.25 * (1 + x0[i] * y) * y0[i];
            }
        }
        //---------------------------------------------------------------------------
        // вычисление  координат i узла
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkVertex(uint IdxKnot, ref double _x, ref double _y)
        {
            _x = x0[IdxKnot];
            _y = y0[IdxKnot];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override uint GetBoundFormType() { return 2; }
    }
    //---------------------------------------------------------------------------
    /// <summary>
    /// Четырехугольные ф.ф., 8-х узловые, 2 порядок, Серендип
    /// </summary>
    [Serializable]
    class HFForm2D_RectangleS2 : AbFunForm
    {
        double[] x0 = { -1, 0, 1, 1, 1, 0, -1, -1 };
        double[] y0 = { -1, -1, -1, 0, 1, 1, 1, 0 };
        public HFForm2D_RectangleS2()
        {
            FContinueType = FFContinueType.ContinuousAlongBorders;
            GeomType = FFGeomType.Rectangle;
            id = TypeFunForm.Form_2D_Rectangle_S2;
            meshRange = IntRange.intRange2;
            Dim = 2;
            Count = 8;
            Name = "Четырехугольные ф.ф., 8-х узловые, 2 порядок, Серендип";
            Init(Count);
        }
        //---------------------------------------------------------------------------
        // вычисление значений функций формы
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkForm(double x, double y)
        {
            double xi = x;
            double eta = y;
            for (uint i = 0; i < 8; i++)
                switch (i)
                {
                    case 0:
                    case 2:
                    case 4:
                    case 6:
                        N[i] = (1.0 + x0[i] * xi) * (1.0 + y0[i] * eta) *
                               (xi * x0[i] + eta * y0[i] - 1.0) * 0.25;
                        break;
                    case 1:
                    case 5:
                        N[i] = (1.0 - xi * xi) * (1.0 + y0[i] * eta) * 0.5;
                        break;
                    case 3:
                    case 7:
                        N[i] = (1.0 - eta * eta) * (1.0 + x0[i] * xi) * 0.5;
                        break;
                }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkLocalDiffForm(double xi, double eta)
        {
            for (uint i = 0; i < 8; i++)
                switch (i)
                {
                    case 0:
                    case 2:
                    case 4:
                    case 6:
                        DN_xi[i] = (1.0 + y0[i] * eta) * x0[i] * (2.0 * xi * x0[i] + y0[i] * eta) * 0.25;
                        DN_eta[i] = (1.0 + xi * x0[i]) * y0[i] * (2.0 * eta * y0[i] + xi * x0[i]) * 0.25;
                        break;
                    case 1:
                    case 5:
                        DN_xi[i] = -xi * (1.0 + eta * y0[i]);
                        DN_eta[i] = (1.0 - xi * xi) * y0[i] * 0.5;
                        break;
                    case 3:
                    case 7:
                        DN_xi[i] = (1.0 - eta * eta) * x0[i] * 0.5;
                        DN_eta[i] = -eta * (1.0 + xi * x0[i]);
                        break;
                }
        }
        //---------------------------------------------------------------------------
        // вычисление  координат i узла
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkVertex(uint IdxKnot, ref double _x, ref double _y)
        {
            _x = x0[IdxKnot];
            _y = y0[IdxKnot];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override uint GetBoundFormType() { return 3; }
    }
    //---------------------------------------------------------------------------
    /// <summary>
    /// Четырехугольные ф.ф., 12-х узловые, 3 порядок, Серендип
    /// </summary>
    [Serializable]
    class HFForm2D_RectangleS3 : AbFunForm
    {
        double[] x0 = { -1.0, -1.0 / 3.0, 1.0 / 3.0, 1.0, 1.0, 1.0, 1.0, 1.0 / 3.0, -1.0 / 3.0, -1.0, -1.0, -1.0 };
        double[] y0 = { -1.0, -1.0, -1.0, -1.0, -1.0 / 3.0, 1.0 / 3.0, 1.0, 1.0, 1.0, 1.0, 1.0 / 3.0, -1.0 / 3.0 };
        public HFForm2D_RectangleS3()
        {
            FContinueType = FFContinueType.ContinuousAlongBorders;
            GeomType = FFGeomType.Rectangle;
            id = TypeFunForm.Form_2D_Rectangle_S3;
            meshRange = IntRange.intRange3;
            Dim = 2;
            Count = 12;
            Name = "Четырехугольные ф.ф., 12-х узловые, 3 порядок, Серендип";
            Init(Count);
        }
        //---------------------------------------------------------------------------
        // вычисление значений функций формы
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkForm(double x, double y)
        {
            double e, b;
            for (uint i = 0; i < 12; i++)
            {
                e = x * x0[i]; b = y * y0[i];
                if (i == 0 || i == 3 || i == 6 || i == 9)
                    N[i] = (1.0 + e) * (1.0 + b) * (9.0 * (x * x + y * y) - 10.0) / 32.0;
                else
                  if (i == 4 || i == 5 || i == 10 || i == 11)
                    N[i] = 9.0 * (1.0 + e) * (1.0 + 9.0 * b) * (1.0 - y * y) / 32.0;
                else
                    N[i] = 9.0 * (1.0 + b) * (1.0 + 9.0 * e) * (1.0 - x * x) / 32.0;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkLocalDiffForm(double x, double y)
        {
            for (uint i = 0; i < 12; i++)
            {
                if (i == 0 || i == 3 || i == 6 || i == 9)
                {
                    DN_xi[i] = 1.0 / 32.0 * x0[i] * (1 + y * y0[i]) * (9 * (x * x + y * y) - 10)
                                      + 0.5625 * (1 + x * x0[i]) * (1 + y * y0[i]) * x;
                    DN_eta[i] = 1.0 / 32.0 * (1.0 + x * x0[i]) * y0[i] * (9 * (x * x + y * y) - 10)
                                      + 0.5625 * (1 + x * x0[i]) * (1 + y * y0[i]) * y;
                }
                else
                {
                    if (i == 4 || i == 5 || i == 10 || i == 11)
                    {
                        DN_xi[i] = 9.0 / 32.0 * x0[i] * (1 + 9 * y * y0[i]) * (1 - y * y);
                        DN_eta[i] = 81.0 / 32.0 * (1 + x * x0[i]) * y0[i] * (1 - y * y) -
                                     0.5625 * (1 + x * x0[i]) * (1 + 9 * y * y0[i]) * y;
                    }
                    else
                    {
                        DN_xi[i] = 81.0 / 32.0 * (1.0 + y * y0[i]) * x0[i] * (1 - x * x) -
                                     0.5625 * (1.0 + y * y0[i]) * (1 + 9 * x * x0[i]) * x;
                        DN_eta[i] = 9.0 / 32.0 * y0[i] * (1 + 9 * x * x0[i]) * (1 - x * x);
                    }
                }
            }
        }
        //---------------------------------------------------------------------------
        // вычисление  координат i узла
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkVertex(uint IdxKnot, ref double _x, ref double _y)
        {
            _x = x0[IdxKnot];
            _y = y0[IdxKnot];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override uint GetBoundFormType() { return 4; }
    }
    //---------------------------------------------------------------------------
    /// <summary>
    /// Четырехугольные ф.ф., 9-х узловые, 2 порядок, Лагранж 
    /// </summary>
    [Serializable]
    class HFForm2D_RectangleL2 : AbFunForm
    {
        double[] x0 = { -1, 0, 1, 1, 1, 0, -1, -1, 0 };
        double[] y0 = { -1, -1, -1, 0, 1, 1, 1, 0, 0 };
        public HFForm2D_RectangleL2()
        {
            FContinueType = FFContinueType.ContinuousAlongBorders;
            GeomType = FFGeomType.Rectangle;
            id = TypeFunForm.Form_2D_Rectangle_L2;
            meshRange = IntRange.intRange2;
            Dim = 2;
            Count = 9;
            Name = "Четырехугольные ф.ф., 9-х узловые, 2 порядок, Лагранж";
            Init(Count);
        }
        //---------------------------------------------------------------------------
        // вычисление значений функций формы
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkForm(double x, double y)
        {
            double xi = x;
            double eta = y;
            for (uint i = 0; i < 9; i++)
                switch (i)
                {
                    case 0:
                    case 2:
                    case 4:
                    case 6:
                        N[i] = 0.25 * (1 + x0[i] * xi) * (1 + y0[i] * eta) * x0[i] * xi * y0[i] * eta;
                        break;
                    case 1:
                    case 5:
                        N[i] = 0.5 * (1 - xi * xi) * (1 + y0[i] * eta) * y0[i] * eta;
                        break;
                    case 3:
                    case 7:
                        N[i] = 0.5 * (1 - eta * eta) * (1 + x0[i] * xi) * x0[i] * xi;
                        break;
                    case 8:
                        N[i] = (1 - eta * eta) * (1 - xi * xi);
                        break;
                }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkLocalDiffForm(double x, double y)
        {
            double xi = x;
            double eta = y;
            // Локальные производные
            double t1 = 1.0 - eta;
            double t3 = t1 * xi * eta;
            double t4 = 1.0 - xi;
            double t8 = 1.0 + xi;
            double t12 = eta * eta;
            double t13 = 1.0 - t12;
            double t14 = t13 * xi;
            double t17 = 1.0 + eta;
            double t19 = t17 * xi * eta;

            DN_xi[0] = -t3 / 4 + t4 * t1 * eta / 4;
            DN_xi[1] = t3;
            DN_xi[2] = -t3 / 4 - t8 * t1 * eta / 4;
            DN_xi[3] = t14 / 2 + t8 * t13 / 2;
            DN_xi[4] = t19 / 4 + t8 * t17 * eta / 4;
            DN_xi[5] = -t19;
            DN_xi[6] = t19 / 4 - t4 * t17 * eta / 4;
            DN_xi[7] = t14 / 2 - t4 * t13 / 2;
            DN_xi[8] = -2.0 * t14;

            t1 = 1.0 - xi;
            t3 = t1 * xi * eta;
            t4 = 1.0 - eta;
            t8 = xi * xi;
            double t9 = 1.0 - t8;
            double t10 = t9 * eta;
            t13 = 1.0 + xi;
            double t15 = t13 * xi * eta;
            t19 = 1.0 + eta;
            DN_eta[0] = -t3 / 4 + t4 * t1 * xi / 4;
            DN_eta[1] = t10 / 2 - t9 * t4 / 2;
            DN_eta[2] = t15 / 4 - t4 * t13 * xi / 4;
            DN_eta[3] = -t15;
            DN_eta[4] = t15 / 4 + t13 * t19 * xi / 4;
            DN_eta[5] = t10 / 2 + t9 * t19 / 2;
            DN_eta[6] = -t3 / 4 - t1 * t19 * xi / 4;
            DN_eta[7] = t3;
            DN_eta[8] = -2.0 * t10;
        }
        //---------------------------------------------------------------------------
        // вычисление  координат i узла
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkVertex(uint IdxKnot, ref double _x, ref double _y)
        {
            _x = x0[IdxKnot];
            _y = y0[IdxKnot];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override uint GetBoundFormType() { return 3; }
    }
    //---------------------------------------------------------------------------
    /// <summary>
    /// Четырехугольные ф.ф., 16-х узловые, 3 порядок, Лагранж
    /// </summary>
    [Serializable]
    class HFForm2D_RectangleL3 : AbFunForm
    {
        double[] x ={ -1.0, -1.0/3.0, 1.0/3.0, 1.0, 1.0, 1.0, 1.0, 1.0/3.0, -1.0/3.0, -1.0, -1.0, -1.0,
               -1.0/3.0, 1.0/3.0 ,1.0/3.0, -1.0/3.0 };
        double[] y ={ -1, -1, -1, -1, -1.0/3.0, 1.0/3.0, 1, 1, 1, 1, 1.0/3.0, -1.0/3.0,
               -1.0/3.0, -1.0/3.0 ,1.0/3.0, 1.0/3.0 };
        public HFForm2D_RectangleL3()
        {
            FContinueType = FFContinueType.ContinuousAlongBorders;
            GeomType = FFGeomType.Rectangle;
            id = TypeFunForm.Form_2D_Rectangle_L3;
            meshRange = IntRange.intRange3;
            Dim = 2;
            Count = 16;
            Name = "Четырехугольные ф.ф., 16-х узловые, 3 порядок, Лагранж";
            Init(Count);
        }
        // вычисление значений функций формы
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkForm(double x, double y)
        {
            double t1 = 1.0 + 3.0 * x;
            double t2 = 1.0 - 3.0 * x;
            double t3 = t1 * t2;
            double t4 = 1.0 - x;
            double t5 = t3 * t4;
            double t6 = 1.0 + 3.0 * y;
            double t7 = 1.0 - 3.0 * y;
            double t8 = t6 * t7;
            double t9 = 1.0 - y;
            double t10 = t8 * t9;
            double t12 = 1.0 + x;
            double t14 = t12 * t2 * t4;
            double t17 = t12 * t1 * t4;
            double t19 = t3 * t12;
            double t21 = 1.0 + y;
            double t23 = t21 * t7 * t9;
            double t26 = t21 * t6 * t9;
            double t28 = t8 * t21;

            N[0] = t5 * t10 / 256;
            N[1] = -9.0 / 256.0 * t14 * t10;
            N[2] = -9.0 / 256.0 * t17 * t10;
            N[3] = t19 * t10 / 256;
            N[4] = -9.0 / 256.0 * t19 * t23;
            N[5] = -9.0 / 256.0 * t19 * t26;
            N[6] = t19 * t28 / 256;
            N[7] = -9.0 / 256.0 * t17 * t28;
            N[8] = -9.0 / 256.0 * t14 * t28;
            N[9] = t5 * t28 / 256;
            N[10] = -9.0 / 256.0 * t5 * t26;
            N[11] = -9.0 / 256.0 * t5 * t23;
            N[12] = 81.0 / 256.0 * t14 * t23;
            N[13] = 81.0 / 256.0 * t17 * t23;
            N[14] = 81.0 / 256.0 * t17 * t26;
            N[15] = 81.0 / 256.0 * t14 * t26;
        }
        //---------------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkLocalDiffForm(double x, double y)
        {
            {
                double t1 = 1.0 - 3.0 * x;
                double t2 = 1.0 - x;
                double t3 = t1 * t2;
                double t4 = 1.0 + 3.0 * y;
                double t5 = 1.0 - 3.0 * y;
                double t6 = t4 * t5;
                double t7 = 1.0 - y;
                double t8 = t6 * t7;
                double t9 = t3 * t8;
                double t10 = 1.0 + 3.0 * x;
                double t11 = t10 * t2;
                double t12 = t11 * t8;
                double t13 = t10 * t1;
                double t14 = t13 * t8;
                double t16 = 1.0 + x;
                double t17 = t16 * t2;
                double t18 = t17 * t8;
                double t19 = t16 * t1;
                double t20 = t19 * t8;
                double t22 = t16 * t10;
                double t23 = t22 * t8;
                double t26 = 1.0 + y;
                double t28 = t26 * t5 * t7;
                double t29 = t19 * t28;
                double t30 = t22 * t28;
                double t31 = t13 * t28;
                double t34 = t26 * t4 * t7;
                double t35 = t19 * t34;
                double t36 = t22 * t34;
                double t37 = t13 * t34;
                double t39 = t6 * t26;
                double t40 = t19 * t39;
                double t41 = t22 * t39;
                double t42 = t13 * t39;
                double t44 = t11 * t39;
                double t45 = t17 * t39;
                double t47 = t3 * t39;
                double t50 = t3 * t34;
                double t51 = t11 * t34;
                double t53 = t3 * t28;
                double t54 = t11 * t28;
                double t56 = t17 * t28;
                double t59 = t17 * t34;
                DN_xi[0] = 3.0 / 256.0 * t9 - 3.0 / 256.0 * t12 - t14 / 256;
                DN_xi[1] = -9.0 / 256.0 * t9 + 27.0 / 256.0 * t18 + 9.0 / 256.0 * t20;
                DN_xi[2] = -9.0 / 256.0 * t12 - 27.0 / 256.0 * t18 + 9.0 / 256.0 * t23;
                DN_xi[3] = 3.0 / 256.0 * t20 - 3.0 / 256.0 * t23 + t14 / 256;
                DN_xi[4] = -27.0 / 256.0 * t29 + 27.0 / 256.0 * t30 - 9.0 / 256.0 * t31;
                DN_xi[5] = -27.0 / 256.0 * t35 + 27.0 / 256.0 * t36 - 9.0 / 256.0 * t37;
                DN_xi[6] = 3.0 / 256.0 * t40 - 3.0 / 256.0 * t41 + t42 / 256;
                DN_xi[7] = -9.0 / 256.0 * t44 - 27.0 / 256.0 * t45 + 9.0 / 256.0 * t41;
                DN_xi[8] = -9.0 / 256.0 * t47 + 27.0 / 256.0 * t45 + 9.0 / 256.0 * t40;
                DN_xi[9] = 3.0 / 256.0 * t47 - 3.0 / 256.0 * t44 - t42 / 256;
                DN_xi[10] = -27.0 / 256.0 * t50 + 27.0 / 256.0 * t51 + 9.0 / 256.0 * t37;
                DN_xi[11] = -27.0 / 256.0 * t53 + 27.0 / 256.0 * t54 + 9.0 / 256.0 * t31;
                DN_xi[12] = 81.0 / 256.0 * t53 - 243.0 / 256.0 * t56 - 81.0 / 256.0 * t29;
                DN_xi[13] = 81.0 / 256.0 * t54 + 243.0 / 256.0 * t56 - 81.0 / 256.0 * t30;
                DN_xi[14] = 81.0 / 256.0 * t51 + 243.0 / 256.0 * t59 - 81.0 / 256.0 * t36;
                DN_xi[15] = 81.0 / 256.0 * t50 - 243.0 / 256.0 * t59 - 81.0 / 256.0 * t35;
            }
            {
                double t1 = 1.0 + 3.0 * x;
                double t2 = 1.0 - 3.0 * x;
                double t3 = t1 * t2;
                double t4 = 1.0 - x;
                double t5 = 1.0 - 3.0 * y;
                double t6 = t4 * t5;
                double t7 = 1.0 - y;
                double t8 = t6 * t7;
                double t9 = t3 * t8;
                double t10 = 1.0 + 3.0 * y;
                double t11 = t4 * t10;
                double t12 = t11 * t7;
                double t13 = t3 * t12;
                double t14 = t11 * t5;
                double t15 = t3 * t14;
                double t17 = 1.0 + x;
                double t18 = t17 * t2;
                double t19 = t18 * t8;
                double t20 = t18 * t12;
                double t21 = t18 * t14;
                double t23 = t17 * t1;
                double t24 = t23 * t8;
                double t25 = t23 * t12;
                double t26 = t23 * t14;
                double t30 = t3 * t17 * t5 * t7;
                double t31 = t17 * t10;
                double t33 = t3 * t31 * t7;
                double t35 = t3 * t31 * t5;
                double t37 = 1.0 + y;
                double t38 = t17 * t37;
                double t40 = t3 * t38 * t7;
                double t42 = t3 * t38 * t5;
                double t45 = t3 * t38 * t10;
                double t48 = t6 * t37;
                double t49 = t23 * t48;
                double t50 = t11 * t37;
                double t51 = t23 * t50;
                double t53 = t18 * t48;
                double t54 = t18 * t50;
                double t56 = t3 * t48;
                double t57 = t3 * t50;
                double t60 = t4 * t37 * t7;
                double t61 = t3 * t60;
                double t64 = t18 * t60;
                double t66 = t23 * t60;
                DN_eta[0] = 3.0 / 256.0 * t9 - 3.0 / 256.0 * t13 - t15 / 256;
                DN_eta[1] = -27.0 / 256.0 * t19 + 27.0 / 256.0 * t20 + 9.0 / 256.0 * t21;
                DN_eta[2] = -27.0 / 256.0 * t24 + 27.0 / 256.0 * t25 + 9.0 / 256.0 * t26;
                DN_eta[3] = 3.0 / 256.0 * t30 - 3.0 / 256.0 * t33 - t35 / 256;
                DN_eta[4] = -9.0 / 256.0 * t30 + 27.0 / 256.0 * t40 + 9.0 / 256.0 * t42;
                DN_eta[5] = -9.0 / 256.0 * t33 - 27.0 / 256.0 * t40 + 9.0 / 256.0 * t45;
                DN_eta[6] = 3.0 / 256.0 * t42 - 3.0 / 256.0 * t45 + t35 / 256;
                DN_eta[7] = -27.0 / 256.0 * t49 + 27.0 / 256.0 * t51 - 9.0 / 256.0 * t26;
                DN_eta[8] = -27.0 / 256.0 * t53 + 27.0 / 256.0 * t54 - 9.0 / 256.0 * t21;
                DN_eta[9] = 3.0 / 256.0 * t56 - 3.0 / 256.0 * t57 + t15 / 256;
                DN_eta[10] = -9.0 / 256.0 * t13 - 27.0 / 256.0 * t61 + 9.0 / 256.0 * t57;
                DN_eta[11] = -9.0 / 256.0 * t9 + 27.0 / 256.0 * t61 + 9.0 / 256.0 * t56;
                DN_eta[12] = 81.0 / 256.0 * t19 - 243.0 / 256.0 * t64 - 81.0 / 256.0 * t53;
                DN_eta[13] = 81.0 / 256.0 * t24 - 243.0 / 256.0 * t66 - 81.0 / 256.0 * t49;
                DN_eta[14] = 81.0 / 256.0 * t25 + 243.0 / 256.0 * t66 - 81.0 / 256.0 * t51;
                DN_eta[15] = 81.0 / 256.0 * t20 + 243.0 / 256.0 * t64 - 81.0 / 256.0 * t54;
            }
        }
        //---------------------------------------------------------------------------
        // вычисление  координат i узла
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkVertex(uint IdxKnot, ref double _x, ref double _y)
        {
            _x = x[IdxKnot];
            _y = y[IdxKnot];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override uint GetBoundFormType() { return 3; }
    }
    //---------------------------------------------------------------------------
    /// <summary>
    /// Четырехугольные ф.ф., 4-х узловые, 2 порядок, Кроуза-Равиарта
    /// (1,x,y,x^2-y^2)
    /// </summary>
    [Serializable]
    class HFForm2D_RectangleCR4 : AbFunForm
    {
        double[] x = { 0.0, 1.0, 0.0, -1.0 };
        double[] y = { -1.0, 0.0, 1.0, 0.0 };
        public HFForm2D_RectangleCR4()
        {
            FContinueType = FFContinueType.СontinuousAtNodes;
            GeomType = FFGeomType.Rectangle;
            id = TypeFunForm.Form_2D_Rectangle_CR;
            meshRange = IntRange.intRange2;
            Dim = 2;
            Count = 4;
            Name = "Четырехугольные ф.ф., 4-х узловые, 2 порядок, Кроуза-Равиарта";
            Init(Count);
        }
        // вычисление значений функций формы
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkForm(double xi, double eta)
        {
            double t1 = eta *0.5;
            double t2 = xi * xi;
            double t3 = t2 * 0.25;
            double t4 = eta * eta;
            double t5 = t4 * 0.25;
            double t7 = xi *0.5;
            N[0] = 0.25 - t1 - t3 + t5;
            N[1] = 0.25 + t7 + t3 - t5;
            N[2] = 0.25 + t1 - t3 + t5;
            N[3] = 0.25 - t7 + t3 - t5;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkLocalDiffForm(double x, double y)
        {
            double xi = x, eta = y;
            double t1 = xi *0.5;
            DN_xi[0] = -t1;
            DN_xi[1] = 0.5 + xi *0.5;
            DN_xi[2] = -t1;
            DN_xi[3] = -0.5 + xi *0.5;
            double t2 = eta *0.5;
            DN_eta[0] = -0.5 + eta *0.5;
            DN_eta[1] = -t2;
            DN_eta[2] = 0.5 + eta *0.5;
            DN_eta[3] = -t2;
        }
        //---------------------------------------------------------------------------
        // вычисление  координат i узла
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkVertex(uint IdxKnot, ref double _x, ref double _y)
        {
            _x = x[IdxKnot];
            _y = y[IdxKnot];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override uint GetBoundFormType() { return 1; }
    }
    //---------------------------------------------------------------------------
    /// <summary>
    /// ОО: Четырехугольные ф.ф., 8-х узловые, 2 порядок, Полный 
    /// </summary>
    [Serializable]
    class HFForm2D_RectangleP8 : AbFunForm
    {
        double[] x = { -1, 0, 1, 1, 1, 0, -1, -1 };
        double[] y = { -1, -1, -1, 0, 1, 1, 1, 0 };
        public HFForm2D_RectangleP8()
        {
            FContinueType = FFContinueType.СontinuousAtNodes;
            GeomType = FFGeomType.Rectangle;
            id = TypeFunForm.Form_2D_Rectangle_P;
            meshRange = IntRange.intRange2;
            Dim = 2;
            Count = 8;
            Name = "Четырехугольные ф.ф., 8-х узловые, 2 порядок, Полный";
            Init(Count);
        }
        //---------------------------------------------------------------------------
        // вычисление значений функций формы
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkForm(double x, double y)
        {
            double t1 = x * x;
            double t3 = y * y;
            double t5 = x * y;
            double t6 = t5 * 0.25;
            double t7 = t1 * t3;
            double t8 = t7 *0.5;
            double t9 = t1 * y;
            double t10 = t9 * 0.25;
            double t11 = x * t3;
            double t12 = t11 * 0.25;
            N[0] = 0.25 - t1 * 0.25 - t3 * 0.25 + t6 + t8 - t10 - t12;
            N[1] = -y *0.5 + t3 *0.5 - t7 *0.5 + t9 *0.5;
            N[2] = -0.25 + t1 * 0.25 + t3 * 0.25 - t5 * 0.25 - t9 * 0.25 + t11 * 0.25;
            N[3] = 1.0 + x *0.5 - t1 *0.5 - t3 + t8 - t11 *0.5;
            N[4] = -0.75 + 0.75 * t1 + 0.75 * t3 + t6 - t8 + t10 + t12;
            N[5] = 1.0 + y *0.5 - t1 - t3 *0.5 + t8 - t9 *0.5;
            N[6] = -0.25 + t1 * 0.25 + t3 * 0.25 - t5 * 0.25 + t9 * 0.25 - t11 * 0.25;
            N[7] = -x *0.5 + t1 *0.5 - t7 *0.5 + t11 *0.5;
        }
        // вычисление значений функций формы ее производных
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkLocalDiffForm(double x, double y)
        {
            {
                double t1 = y *0.5;
                double t2 = x * 0.25;
                double t3 = x * x;
                double t4 = t3 * y;
                double t5 = t3 * 0.25;
                double t6 = x * y;
                double t7 = t6 *0.5;
                double t10 = -0.5 + y - t4 + t3 *0.5;
                DN_eta[0] = -t1 + t2 + t4 - t5 - t7;
                DN_eta[1] = t10;
                DN_eta[2] = t1 - t2 - t5 + t7;
                DN_eta[3] = -2.0 * y + t4 - t6;
                DN_eta[4] = 1.5 * y + t2 - t4 + t5 + t7;
                DN_eta[5] = -t10;
                DN_eta[6] = t1 - t2 + t5 - t7;
                DN_eta[7] = -t4 + t6;
            }
            {
                double t1 = x *0.5;
                double t2 = y * 0.25;
                double t3 = y * y;
                double t4 = x * t3;
                double t5 = x * y;
                double t6 = t5 *0.5;
                double t7 = t3 * 0.25;
                double t12 = 0.5 - x + t4 - t3 *0.5;
                DN_xi[0] = -t1 + t2 + t4 - t6 - t7;
                DN_xi[1] = -t4 + t5;
                DN_xi[2] = t1 - t2 - t6 + t7;
                DN_xi[3] = t12;
                DN_xi[4] = 1.5 * x + t2 - t4 + t6 + t7;
                DN_xi[5] = -2.0 * x + t4 - t5;
                DN_xi[6] = t1 - t2 + t6 - t7;
                DN_xi[7] = -t12;
            }
        }
        //---------------------------------------------------------------------------
        // вычисление  координат i узла
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkVertex(uint IdxKnot, ref double _x, ref double _y)
        {
            _x = x[IdxKnot];
            _y = y[IdxKnot];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override uint GetBoundFormType() { return 3; }
    }
    //---------------------------------------------------------------------------
    /// <summary>
    /// ОО: Четырехугольные ф.ф., 4-х узла в области КЭ, 1 порядок 
    /// </summary>
    [Serializable]
    class HFForm2D_RectangleInArea1 : AbFunForm
    {
        const double d = 0.577350269189626;
        double[] x = { -d, d, d, -d };
        double[] y = { -d, -d, d, d };
        public HFForm2D_RectangleInArea1()
        {
            FContinueType = FFContinueType.DisСontinuous;
            GeomType = FFGeomType.Rectangle;
            id = TypeFunForm.Form_2D_Rectangle_Area4_L1N;
            meshRange = IntRange.intRange1;
            FContinueType = 0; // нет контактных узлов
            Dim = 2;
            Count = 4;
            Name = "Четырехугольные ф.ф., 4-х узла в области КЭ, 1 порядок";
            Init(Count);
        }
        //---------------------------------------------------------------------------
        // вычисление значений функций формы
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkForm(double x, double y)
        {
            double t1 = 1 / d;
            double t2 = x * t1;
            double t3 = 1.0 - t2;
            double t4 = y * t1;
            double t5 = 1.0 - t4;
            double t8 = 1.0 + t2;
            double t11 = 1.0 + t4;
            N[0] = t3 * t5 * 0.25;
            N[1] = t8 * t5 * 0.25;
            N[2] = t8 * t11 * 0.25;
            N[3] = t3 * t11 * 0.25;
        }
        // вычисление значений локальных функций формы ее производных
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkLocalDiffForm(double x, double y)
        {
            {

                double t1 = 1 / d;
                double t2 = y * t1;
                double t5 = t1 * (1.0 - t2) * 0.25;
                double t8 = t1 * (1.0 + t2) * 0.25;
                DN_xi[0] = -t5;
                DN_xi[1] = t5;
                DN_xi[2] = t8;
                DN_xi[3] = -t8;
            }
            {
                double t1 = 1 / d;
                double t2 = x * t1;
                double t5 = t1 * (1.0 - t2) * 0.25;
                double t8 = t1 * (1.0 + t2) * 0.25;
                DN_eta[0] = -t5;
                DN_eta[1] = -t8;
                DN_eta[2] = t8;
                DN_eta[3] = t5;
            }
        }
        //---------------------------------------------------------------------------
        // вычисление  координат i узла
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkVertex(uint IdxKnot, ref double _x, ref double _y)
        {
            _x = x[IdxKnot];
            _y = y[IdxKnot];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override uint GetBoundFormType() { return 0; }
    }
    //---------------------------------------------------------------------------
    /// <summary>
    /// ОО: Четырехугольные ф.ф., 3-х узла в области КЭ, 1 порядок 
    /// </summary>
    [Serializable]
    class HFForm2D_TriangleInArea1 : AbFunForm
    {
        double[] x = { 0.1, 0.9, 0.1 };
        double[] y = { 0.1, 0.1, 0.9 };
        public HFForm2D_TriangleInArea1()
        {
            FContinueType = FFContinueType.DisСontinuous;
            GeomType = FFGeomType.Rectangle;
            id = TypeFunForm.Form_2D_Rectangle_Area3_L1N;
            meshRange = IntRange.intRange1;
            Dim = 2;
            Count = 3;
            Name = "Четырехугольные ф.ф., 3-х узла в области КЭ, 1 порядок";
            Init(Count);
        }
        //---------------------------------------------------------------------------
        // вычисление значений функций формы
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkForm(double x, double y)
        {
            N[0] = 5.0 * 0.25 - 5.0 * 0.25 * x - 5.0 * 0.25 * y;
            N[1] = -1.0 / 8.0 + 5.0 * 0.25 * x;
            N[2] = -1.0 / 8.0 + 5.0 * 0.25 * y;
        }
        // вычисление значений локальных функций формы ее производных
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkLocalDiffForm(double x, double y)
        {
            {
                DN_xi[0] = -5.0 * 0.25;
                DN_xi[1] = 5.0 * 0.25;
                DN_xi[2] = 0.0;
            }
            {
                DN_eta[0] = -5.0 * 0.25;
                DN_eta[1] = 0.0;
                DN_eta[2] = 5.0 * 0.25;
            }
        }
        //---------------------------------------------------------------------------
        // вычисление  координат i узла
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkVertex(uint IdxKnot, ref double _x, ref double _y)
        {
            _x = x[IdxKnot];
            _y = y[IdxKnot];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override uint GetBoundFormType() { return 0; }
    }
    //---------------------------------------------------------------------------
    /// <summary>
    ///ОО: Четырехугольные ф.ф., 4-х узла, 1 порядок, мод Кроуза-Равиарта
    /// </summary>
    [Serializable]
    class HFForm2D_RectangleMyReviar : AbFunForm
    {
        double a = 1.0;
        double b = -5.0 / 3.0;
        double[] x = { 0.0, 1.0, 0.0, -1.0 };
        double[] y = { -1.0, 0.0, 1.0, 0.0 };
        public HFForm2D_RectangleMyReviar()
        {
            FContinueType = FFContinueType.СontinuousAtNodes;
            GeomType = FFGeomType.Rectangle;
            id = TypeFunForm.Form_2D_Rectangle_CRM;
            meshRange = IntRange.intRange2;
            Dim = 2;
            Count = 4;
            Name = "Четырехугольные ф.ф., 4-х узла, 1 порядок, мод Кроуза-Равиарта";
            Init(Count);
        }
        //---------------------------------------------------------------------------
        // вычисление значений функций формы
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkForm(double x, double y)
        {
            double t1 = y *0.5;
            double t4 = x * x;
            double t5 = y * y;
            double t8 = t4 * t4;
            double t9 = t5 * t5;
            double t14 = 1 / (a + b) * (a * (t4 - t5) + b * (t8 - t9)) * 0.25;
            double t16 = x *0.5;
            N[0] = 0.25 - t1 - t14;
            N[1] = 0.25 + t16 + t14;
            N[2] = 0.25 + t1 - t14;
            N[3] = 0.25 - t16 + t14;
        }
        // вычисление значений локальных функций формы ее производных
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkLocalDiffForm(double x, double y)
        {
            {
                double t5 = x * x;
                double t11 = 1 / (a + b) * (2.0 * a * x + 4.0 * b * t5 * x) * 0.25;
                DN_xi[0] = -t11;
                DN_xi[1] = 0.5 + t11;
                DN_xi[2] = -t11;
                DN_xi[3] = -0.5 + t11;
            }
            {
                double t5 = y * y;
                double t11 = 1 / (a + b) * (-2.0 * a * y - 4.0 * b * t5 * y) * 0.25;
                DN_eta[0] = -0.5 - t11;
                DN_eta[1] = t11;
                DN_eta[2] = 0.5 - t11;
                DN_eta[3] = t11;
            }
        }
        //---------------------------------------------------------------------------
        // вычисление  координат i узла
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkVertex(uint IdxKnot, ref double _x, ref double _y)
        {
            _x = x[IdxKnot];
            _y = y[IdxKnot];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override uint GetBoundFormType() { return 2; }
    }
    //---------------------------------------------------------------------------
    /// <summary>
    ///ОО: Внутренний 4 - х узловой КЭ третьего порядка
    ///    Неполные полиномы Эрмита
    ///       
    ///  4 |------| 3
    ///    |      |
    ///  1 |------| 2
    ///  NN1 :=  (1/2-3/4* x+1/4* x^3)* (1/2-3/4* y+1/4* y^3):
    ///  NN2 :=  (1/2+3/4* x-1/4* x^3)* (1/2-3/4* y+1/4* y^3):
    ///  NN3 :=  (1/2+3/4* x-1/4* x^3)* (1/2+3/4* y-1/4* y^3):
    ///  NN4 :=  (1/2-3/4* x+1/4* x^3)* (1/2+3/4* y-1/4* y^3):
    ///  4 |<-du->| 3
    ///    |      |
    ///  1 |<-du->| 2
    ///  KN1 :=  (1/4 - 1/4* x-1/4* x^2+1/4* x^3)* (1/2-3/4* y+1/4* y^3):
    ///  KN2 :=  (-1/4 - 1/4* x+1/4* x^2+1/4* x^3)* (1/2-3/4* y+1/4* y^3):
    ///  KN3 :=  (-1/4 - 1/4* x+1/4* x^2+1/4* x^3)* (1/2+3/4* y-1/4* y^3):
    ///  KN4 :=  (1/4 - 1/4* x-1/4* x^2+1/4* x^3)* (1/2+3/4* y-1/4* y^3):
    ///  4 |------| 3
    ///    ^      ^
    ///    dv     dv
    ///    V      V
    ///  1 |------| 2
    /// NK1 :=  (1/2-3/4* x+1/4* x^3)* (1/4  - 1/4* y-1/4* y^2+1/4* y^3):
    /// NK2 :=  (1/2+3/4* x-1/4* x^3)* (1/4  - 1/4* y-1/4* y^2+1/4* y^3):
    /// NK3 :=  (1/2+3/4* x-1/4* x^3)* (-1/4 - 1/4* y+1/4* y^2+1/4* y^3):
    /// NK4 :=  (1/2-3/4* x+1/4* x^3)* (-1/4 - 1/4* y+1/4* y^2+1/4* y^3):
    ///
    /// </summary>
    [Serializable]
    class HFForm2D_RectangleErmit4 : AbFunForm
    {
        double[] x0 = { -1, 1, 1, -1 };
        double[] y0 = { -1, -1, 1, 1 };
        public HFForm2D_RectangleErmit4()
        {
            FContinueType = FFContinueType.СontinuousAtNodes;
            GeomType = FFGeomType.Rectangle;
            id = TypeFunForm.Form_2D_Rectangle_Ermit4;
            meshRange = IntRange.intRange3;
            Dim = 2;
            Count = 4;
            Name = "Четырехугольные ф.ф., 4-х узла, 3 порядок, полиномы Эрмита";
            Init(Count);
        }
        //---------------------------------------------------------------------------
        // вычисление значений функций формы
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkForm(double x, double y)
        {
            double t1 = 0.75 * x;
            double t2 = x * x;
            double t3 = t2 * x;
            double t4 = t3 * 0.25;
            double t5 = 0.5 - t1 + t4;
            double t6 = 0.75 * y;
            double t7 = y * y;
            double t8 = t7 * y;
            double t9 = t8 * 0.25;
            double t10 = 0.5 - t6 + t9;
            double t12 = 0.5 + t1 - t4;
            double t14 = 0.5 + t6 - t9;
            double t17 = 1.0 - x - t2 + t3;
            double t19 = -1.0 - x + t2 + t3;
            double t23 = 1.0 - y - t7 + t8;
            double t26 = -1.0 - y + t7 + t8;
           
            N[0] = t5 * t10;
            N[1] = t12 * t10;
            N[2] = t12 * t14;
            N[3] = t5 * t14;

            N[4] = t17 * t10 * 0.25;
            N[5] = t19 * t10 * 0.25;
            N[6] = t19 * t14 * 0.25;
            N[7] = t17 * t14 * 0.25;

            N[8] = t5 * t23 * 0.25;
            N[9] = t12 * t23 * 0.25;
            N[10] = t12 * t26 * 0.25;
            N[11] = t5 * t26 * 0.25;
            //N[4] = t17 * t10 * 0.25;
            //N[5] = t19 * t10 * 0.25;
            //N[6] = t19 * t14 * 0.25;
            //N[7] = t17 * t14 * 0.25;

            //N[8] = t5 * t23 * 0.25;
            //N[9] = t12 * t23 * 0.25;
            //N[10] = t12 * t26 * 0.25;
            //N[11] = t5 * t26 * 0.25;


        }
        // вычисление значений локальных функций формы ее производных
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkLocalDiffForm(double x, double y)
        {
            double t1, t2, t3, t4, t5, t6, t7, t8, t9, t10;
            double t11, t12, t13, t14, t15, t16, t17, t18, t19, t20;
            double t21, t23, t24;
            {
                t1 = x * x;
                t2 = -1.0 + t1;
                t3 = 0.75 * y;
                t4 = y * y;
                t5 = t4 * y;
                t6 = t5 * 0.25;
                t7 = 0.5 - t3 + t6;
                t10 = 0.5 + t3 - t6;
                t13 = x *0.5;
                t14 = 0.75 * t1;
                t15 = -0.25 - t13 + t14;
                t17 = -0.25 + t13 + t14;
                t21 = 1.0 - y - t4 + t5;
                t24 = -1.0 - y + t4 + t5;
                DN_xi[0] = 0.75 * t2 * t7;
                DN_xi[1] = -0.75 * t2 * t7;
                DN_xi[2] = -0.75 * t2 * t10;
                DN_xi[3] = 0.75 * t2 * t10;
                DN_xi[4] = t15 * t7;
                DN_xi[5] = t17 * t7;
                DN_xi[6] = t17 * t10;
                DN_xi[7] = t15 * t10;
                DN_xi[8] = 0.1875 * t2 * t21;
                DN_xi[9] = -0.1875 * t2 * t21;
                DN_xi[10] = -0.1875 * t2 * t24;
                DN_xi[11] = 0.1875 * t2 * t24;
            }
            {
                t1 = 0.75 * x;
                t2 = x * x;
                t3 = t2 * x;
                t4 = t3 * 0.25;
                t5 = 0.5 - t1 + t4;
                t6 = y * y;
                t7 = -1.0 + t6;
                t9 = 0.5 + t1 - t4;
                t13 = 1.0 - x - t2 + t3;
                t15 = -1.0 - x + t2 + t3;
                t19 = y *0.5;
                t20 = 0.75 * t6;
                t21 = -0.25 - t19 + t20;
                t24 = -0.25 + t19 + t20;
                DN_eta[0] = 0.75 * t5 * t7;
                DN_eta[1] = 0.75 * t9 * t7;
                DN_eta[2] = -0.75 * t9 * t7;
                DN_eta[3] = -0.75 * t5 * t7;
                DN_eta[4] = 0.1875 * t13 * t7;
                DN_eta[5] = 0.1875 * t15 * t7;
                DN_eta[6] = -0.1875 * t15 * t7;
                DN_eta[7] = -0.1875 * t13 * t7;
                DN_eta[8] = t5 * t21;
                DN_eta[9] = t9 * t21;
                DN_eta[10] = t9 * t24;
                DN_eta[11] = t5 * t24;
            }
            {
                t1 = 0.75 * y;
                t2 = y * y;
                t3 = t2 * y;
                t4 = t3 * 0.25;
                t5 = 0.5 - t1 + t4;
                t7 = 1.5 * x * t5;
                t8 = 0.5 + t1 - t4;
                t10 = 1.5 * x * t8;
                t11 = 1.5 * x;
                t12 = -0.5 + t11;
                t14 = 0.5 + t11;
                t20 = 0.375 * x * (1.0 - y - t2 + t3);
                t23 = 0.375 * x * (-1.0 - y + t2 + t3);
                DN2xi[0] = t7;
                DN2xi[1] = -t7;
                DN2xi[2] = -t10;
                DN2xi[3] = t10;
                DN2xi[4] = t12 * t5;
                DN2xi[5] = t14 * t5;
                DN2xi[6] = t14 * t8;
                DN2xi[7] = t12 * t8;
                DN2xi[8] = t20;
                DN2xi[9] = -t20;
                DN2xi[10] = -t23;
                DN2xi[11] = t23;
            }
            {
                t1 = 0.75 * x;
                t2 = x * x;
                t3 = t2 * x;
                t4 = t3 * 0.25;
                t5 = 0.5 - t1 + t4;
                t7 = 1.5 * t5 * y;
                t8 = 0.5 + t1 - t4;
                t10 = 1.5 * t8 * y;
                t13 = 0.375 * (1.0 - x - t2 + t3) * y;
                t16 = 0.375 * (-1.0 - x + t2 + t3) * y;
                t17 = 1.5 * y;
                t18 = -0.5 + t17;
                t21 = 0.5 + t17;
                DN2eta[0] = t7;
                DN2eta[1] = t10;
                DN2eta[2] = -t10;
                DN2eta[3] = -t7;
                DN2eta[4] = t13;
                DN2eta[5] = t16;
                DN2eta[6] = -t16;
                DN2eta[7] = -t13;
                DN2eta[8] = t5 * t18;
                DN2eta[9] = t8 * t18;
                DN2eta[10] = t8 * t21;
                DN2eta[11] = t5 * t21;
            }
            {
                t1 = x * x;
                t2 = -1.0 + t1;
                t3 = y * y;
                t4 = -1.0 + t3;
                t5 = 0.5625 * t2 * t4;
                t6 = -0.5625 * t2 * t4;
                t7 = x *0.5;
                t8 = 0.75 * t1;
                t9 = -0.25 - t7 + t8;
                t11 = -0.25 + t7 + t8;
                t15 = y *0.5;
                t16 = 0.75 * t3;
                t17 = -0.25 - t15 + t16;
                t20 = -0.25 + t15 + t16;
                DNXiEta[0] = t5;
                DNXiEta[1] = t6;
                DNXiEta[2] = t5;
                DNXiEta[3] = t6;
                DNXiEta[4] = 0.75 * t9 * t4;
                DNXiEta[5] = 0.75 * t11 * t4;
                DNXiEta[6] = -0.75 * t11 * t4;
                DNXiEta[7] = -0.75 * t9 * t4;
                DNXiEta[8] = 0.75 * t2 * t17;
                DNXiEta[9] = -0.75 * t2 * t17;
                DNXiEta[10] = -0.75 * t2 * t20;
                DNXiEta[11] = 0.75 * t2 * t20;
            }
        }
        //---------------------------------------------------------------------------
        // вычисление  координат i узла
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkVertex(uint IdxKnot, ref double _x, ref double _y)
        {
            _x = x0[IdxKnot];
            _y = y0[IdxKnot];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override uint GetBoundFormType() { return 2; }
    }
}
