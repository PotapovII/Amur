//---------------------------------------------------------------------------
//                         ПРОЕКТ  "Hit"
//                         проектировщик:
//                кодировка C++: 7.12.98 Потапов И.И.
//---------------------------------------------------------------------------
//          реконструкция: переход на статические массивы с++
//                 кодировка : 22.12.2003 Потапов И.И.
//---------------------------------------------------------------------------
//                       ПРОЕКТ  "RiverLib"
//              Перенос на C# : 04.03.2021  Потапов И.И.
//---------------------------------------------------------------------------


namespace MeshLib
{
    using System;
    using CommonLib;
    /// <summary>
    /// ОО: Трехугольные ф.ф., 3-х узловые, 1 порядок
    /// </summary>
    [Serializable]
    public class HFForm2D_TriangleAnaliticL1 : AbFunForm
    {
        double S;
        double a0, a1, a2;
        /// <summary>
        /// ОО: Трехугольные ф.ф., 3-х узловые, 1 порядок, аналитика
        /// </summary>
        public HFForm2D_TriangleAnaliticL1()
        {
            FContinueType = FFContinueType.ContinuousAlongBorders;
            GeomType = FFGeomType.Triangle;
            id = TypeFunForm.Form_2D_TriangleAnalitic_L1;
            meshRange = IntRange.intRange1;
            Dim = 2;
            Count = 3;
            Name = "Трехугольные ф.ф., 3-х узловые, 1 порядок, аналитика";
            Init(Count);
        }
        public override void SetGeoCoords(double[] x, double[] y)
        {
            CX = x;
            CY = y;
            S =    CX[1] * CY[2] + CY[0] * CX[2] + CX[0] * CY[1]
                 - CY[1] * CX[2] - CX[0] * CY[2] - CY[0] * CX[1];

            DN_x[0] = (CY[1] - CY[2]) / S;
            DN_x[1] = (CY[2] - CY[0]) / S;
            DN_x[2] = (CY[0] - CY[1]) / S;

            DN_y[1] = (CX[0] - CX[2]) / S;
            DN_y[0] = (CX[2] - CX[1]) / S;
            DN_y[2] = (CX[1] - CX[0]) / S;

            a0 = (CX[1] * CY[2] - CX[2] * CY[1]) / S;
            a1 = (CX[2] * CY[0] - CX[0] * CY[2]) / S;
            a2 = (CX[0] * CY[1] - CX[1] * CY[0]) / S;
        }

        // вычисление значений функций формы
        public override void CalkForm(double x, double y)
        {
            N[0] = a0 + DN_x[0] * x + DN_y[0] * y;
            N[1] = a1 + DN_x[1] * x + DN_y[1] * y;
            N[2] = a2 + DN_x[2] * x + DN_y[2] * y;
        }

        /// <summary>
        /// вычисление значений функций формы ее производных
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public override void CalkDiffForm(double x, double y, double[,] BWM = null)
        {
            if (GeomFormID != HFF_MixMask)
            {
                GFForm.CalkLocalDiffForm(x, y);
                // преобразование с матрицей Якоби
                Jcob(BWM);
            }
        }

        public override void CalkLocalDiffForm(double x, double y)
        {
            DN_xi[0] = -1.0;
            DN_xi[1] = 1.0;
            DN_xi[2] = 0.0;
            DN_eta[0] = -1.0;
            DN_eta[1] = 0.0;
            DN_eta[2] = 1.0;

        }
        // вычисление  координат i узла
        public override void CalkVertex(uint IdxKnot, ref double _x, ref double _y)
        {
            double[] x = { 0.0, 1.0, 0.0 };
            double[] y = { 0.0, 0.0, 1.0 };
            _x = x[IdxKnot];
            _y = y[IdxKnot];
        }
        public override uint GetBoundFormType() { return 2; }
    }
    //---------------------------------------------------------------------------
    /// <summary>
    /// ОО: Трехугольные ф.ф., 3-х узловые, 0 порядок
    /// </summary
    [Serializable]
    class HFForm2D_TriangleConst : AbFunForm
    {
        public HFForm2D_TriangleConst()
        {
            FContinueType = FFContinueType.DisСontinuous;
            GeomType = FFGeomType.Triangle;
            id = TypeFunForm.Form_2D_Triangle_L0;
            meshRange = IntRange.intRange1;
            Dim = 2;
            Count = 1;
            Name = "Трехугольные ф.ф., 3-х узловые, 0 порядок";
            Init(Count);
        }
        // вычисление значений функций формы
        public override void CalkForm(double x, double y)
        {
            N[0] = 1;
        }
        public override void CalkLocalDiffForm(double x, double y)
        {
            DN_xi[0] = 0;
            DN_eta[0] = 0;
        }
        // вычисление  координат i узла
        public override void CalkVertex(uint IdxKnot, ref double _x, ref double _y)
        {
            _x = 1.0 / 3.0;
            _y = 1.0 / 3.0;
        }
        public override uint GetBoundFormType() { return 0; }
    }
    //---------------------------------------------------------------------------
    /// <summary>
    /// ОО: Трехугольные ф.ф., 3-х узловые, 1 порядок, Лагранж
    /// </summary>
    [Serializable]
    class HFForm2D_TriangleL1 : AbFunForm
    {
        public HFForm2D_TriangleL1()
        {
            FContinueType = FFContinueType.ContinuousAlongBorders;
            GeomType = FFGeomType.Triangle;
            id = TypeFunForm.Form_2D_Triangle_L1;
            meshRange = IntRange.intRange1;
            Dim = 2;
            Count = 3;
            Name = "Трехугольные ф.ф., 3-х узловые, 1 порядок, Лагранж";
            Init(Count);
        }
        // вычисление значений функций формы
        public override void CalkForm(double x, double y)
        {
            N[0] = 1 - x - y;
            N[1] = x;
            N[2] = y;
        }
        public override void CalkLocalDiffForm(double x, double y)
        {
            DN_xi[0] = -1.0;
            DN_xi[1] = 1.0;
            DN_xi[2] = 0.0;
            DN_eta[0] = -1.0;
            DN_eta[1] = 0.0;
            DN_eta[2] = 1.0;
        }
        // вычисление  координат i узла
        public override void CalkVertex(uint IdxKnot, ref double _x, ref double _y)
        {
            double[] x = { 0.0, 1.0, 0.0 };
            double[] y = { 0.0, 0.0, 1.0 };
            _x = x[IdxKnot];
            _y = y[IdxKnot];
        }
        public override uint GetBoundFormType() { return 2; }
    }
    //---------------------------------------------------------------------------
    /// <summary>
    /// ОО: Трехугольные ф.ф., 3-х узловые, 1 порядок, Лагранж
    /// изменен порядок нумерации функций формы для согласования с пакетом River2D
    /// </summary>
    [Serializable]
    class HFForm2D_TriangleL1_River : AbFunForm
    {
        public HFForm2D_TriangleL1_River()
        {
            FContinueType = FFContinueType.ContinuousAlongBorders;
            GeomType = FFGeomType.Triangle;
            id = TypeFunForm.Form_2D_Triangle_L1_River;
            meshRange = IntRange.intRange1;
            Dim = 2;
            Count = 3;
            Name = "Трехугольные ф.ф., 3-х узловые, 1 порядок, Лагранж для River2D#";
            Init(Count);
        }
        // вычисление значений функций формы
        public override void CalkForm(double x, double y)
        {
            N[0] = x;
            N[1] = y;
            N[2] = 1 - x - y;
        }
        public override void CalkLocalDiffForm(double x, double y)
        {
            DN_xi[0] = 1.0;
            DN_xi[1] = 0.0;
            DN_xi[2] = -1.0;

            DN_eta[0] = 0.0;
            DN_eta[1] = 1.0;
            DN_eta[2] = -1.0;
        }
        // вычисление  координат i узла
        public override void CalkVertex(uint IdxKnot, ref double _x, ref double _y)
        {
            double[] x = { 1.0, 0.0, 0.0 };
            double[] y = { 0.0, 1.0, 0.0 };
            _x = x[IdxKnot];
            _y = y[IdxKnot];
        }
        public override uint GetBoundFormType() { return 2; }
    }
    //---------------------------------------------------------------------------
    /// <summary>
    /// ОО: Трехугольные ф.ф., 3-х узловые, 1 порядок, Кроуза-Равиарта
    /// </summary>
    [Serializable]
    class HFForm2D_TriangleCR : AbFunForm
    {
        public HFForm2D_TriangleCR()
        {
            FContinueType = FFContinueType.СontinuousAtNodes;
            GeomType = FFGeomType.Triangle;
            id = TypeFunForm.Form_2D_Triangle_CR;
            meshRange = IntRange.intRange2;
            Dim = 2;
            Count = 3;
            Name = "Трехугольные ф.ф., 3-х узловые, 1 порядок, Кроуза-Равиарта";
        }
        // вычисление значений функций формы
        public override void CalkForm(double x, double y)
        {
            N[0] = 1 - 2 * x;
            N[1] = 2 * x + 2 * y - 1;
            N[2] = 1 - 2 * y;
        }
        public override void CalkLocalDiffForm(double x, double y)
        {
            DN_xi[0] = -2.0;
            DN_xi[1] = 2.0;
            DN_xi[2] = 0.0;
            DN_eta[0] = 0.0;
            DN_eta[1] = 2.0;
            DN_eta[2] = -2.0;
        }
        // вычисление  координат i узла
        public override void CalkVertex(uint IdxKnot, ref double _x, ref double _y)
        {
            double[] x = { 0.5, 0.5, 0.0 };
            double[] y = { 0.0, 0.5, 0.5 };
            _x = x[IdxKnot];
            _y = y[IdxKnot];
        }
        public override uint GetBoundFormType() { return 1; }
    }
    //---------------------------------------------------------------------------
    /// <summary>
    /// ОО: Трехугольные ф.ф., 6-х узловые, 2 порядок, Лагранж
    /// </summary>
    [Serializable]
    class HFForm2D_TriangleL2 : AbFunForm
    {
        public HFForm2D_TriangleL2()
        {
            FContinueType = FFContinueType.ContinuousAlongBorders;
            GeomType = FFGeomType.Triangle;
            id = TypeFunForm.Form_2D_Triangle_L2;
            meshRange = IntRange.intRange2;
            Dim = 2;
            Count = 6;
            Name = "Трехугольные ф.ф., 6-х узловые, 2 порядок, Лагранж";
            Init(Count);
        }
        // вычисление значений функций формы
        public override void CalkForm(double x, double y)
        {
            double L1, L2, L3;
            // ФФ первого порядка
            L1 = 1 - x - y;
            L2 = x;
            L3 = y;
            // Определяемые ФФ
            N[0] = L1 * (2 * L1 - 1);
            N[1] = 4 * L1 * L2;
            N[2] = L2 * (2 * L2 - 1);
            N[3] = 4 * L2 * L3;
            N[4] = L3 * (2 * L3 - 1);
            N[5] = 4 * L3 * L1;
        }
        public override void CalkLocalDiffForm(double x, double y)
        {
            double xi, eta;
            xi = x;
            eta = y;
            DN_xi[0] = -3.0 + 4.0 * xi + 4.0 * eta;
            DN_xi[1] = -8.0 * xi + 4.0 - 4.0 * eta;
            DN_xi[2] = 4.0 * xi - 1.0;
            DN_xi[3] = 4.0 * eta;
            DN_xi[4] = 0.0;
            DN_xi[5] = -4.0 * eta;

            DN_eta[0] = -3.0 + 4.0 * xi + 4.0 * eta;
            DN_eta[1] = -4.0 * xi;
            DN_eta[2] = 0.0;
            DN_eta[3] = 4.0 * xi;
            DN_eta[4] = 4.0 * eta - 1.0;
            DN_eta[5] = 4.0 - 4.0 * xi - 8.0 * eta;
        }
        // вычисление  координат i узла
        public override void CalkVertex(uint IdxKnot, ref double _x, ref double _y)
        {
            double[] x = { 0.0, 0.5, 1.0, 0.5, 0.0, 0.0 };
            double[] y = { 0.0, 0.0, 0.0, 0.5, 1.0, 0.5 };
            _x = x[IdxKnot];
            _y = y[IdxKnot];
        }
        public override uint GetBoundFormType() { return 3; }
    }
    //---------------------------------------------------------------------------
    /// <summary>
    /// ОО: Трехугольные ф.ф., 10-х узловые, 3 порядок, Лагранж
    /// </summary>
    [Serializable]
    class HFForm2D_TriangleL3 : AbFunForm
    {
        public HFForm2D_TriangleL3()
        {
            FContinueType = FFContinueType.ContinuousAlongBorders;
            GeomType = FFGeomType.Triangle;
            id = TypeFunForm.Form_2D_Triangle_L3;
            meshRange = IntRange.intRange3;
            Dim = 2;
            Count = 10;
            Name = "Трехугольные ф.ф., 10-х узловые, 3 порядок, Лагранж";
            Init(Count);
        }
        // вычисление значений функций формы
        public override void CalkForm(double x, double y)
        {
            double L1, L2, L3;
            // ФФ первого порядка
            L1 = 1 - x - y;
            L2 = x;
            L3 = y;
            // Определяемые ФФ
            N[0] = L1 * (3.0 * L1 - 2.0) * (3.0 * L1 - 1.0) / 2.0;
            N[1] = 9.0 * L1 * L2 * (3 * L1 - 1) / 2.0;
            N[2] = 9.0 * L2 * L1 * (3 * L2 - 1) / 2.0;
            N[3] = L2 * (3.0 * L2 - 2.0) * (3.0 * L2 - 1.0) / 2.0;
            N[4] = 9.0 * L2 * L3 * (3 * L2 - 1) / 2.0;
            N[5] = 9.0 * L3 * L2 * (3 * L3 - 1) / 2.0;
            N[6] = L3 * (3.0 * L3 - 2.0) * (3.0 * L3 - 1.0) / 2.0;
            N[7] = 9.0 * L3 * L1 * (3 * L3 - 1) / 2.0;
            N[8] = 9.0 * L1 * L3 * (3 * L1 - 1) / 2.0;
            N[9] = 27.0 * L1 * L2 * L3;
        }
        public override void CalkLocalDiffForm(double x, double y)
        {
            double xi, eta;
            xi = x;
            eta = y;
            double t1 = 1.0 - 3.0 * xi - 3.0 * eta;
            double t2 = 2.0 - 3.0 * xi - 3.0 * eta;
            double t4 = 1.0 - xi - eta;
            double t5 = t4 * t2;
            double t9 = t4 * xi;
            double t11 = 3.0 * xi - 1.0;
            double t13 = xi * t11;
            double t15 = 3.0 * xi - 2.0;
            double t20 = xi * eta;
            double t23 = eta * (3.0 * eta - 1.0);
            double t25 = eta * t4;
            DN_xi[0] = -t1 * t2 / 2 - 3.0 / 2.0 * t5 - 3.0 / 2.0 * t4 * t1;
            DN_xi[1] = -9.0 / 2.0 * xi * t2 + 9.0 / 2.0 * t5 - 27.0 / 2.0 * t9;
            DN_xi[2] = 9.0 / 2.0 * t4 * t11 - 9.0 / 2.0 * t13 + 27.0 / 2.0 * t9;
            DN_xi[3] = t15 * t11 / 2 + 3.0 / 2.0 * t13 + 3.0 / 2.0 * xi * t15;
            DN_xi[4] = 9.0 / 2.0 * eta * t11 + 27.0 / 2.0 * t20;
            DN_xi[5] = 9.0 / 2.0 * t23;
            DN_xi[6] = 0.0;
            DN_xi[7] = -9.0 / 2.0 * t23;
            DN_xi[8] = -9.0 / 2.0 * eta * t2 - 27.0 / 2.0 * t25;
            DN_xi[9] = -27.0 * t20 + 27.0 * t25;

            double t12 = xi * (3.0 * xi - 1.0);
            t13 = 3.0 * eta - 1.0;
            t15 = xi * eta;
            double t17 = 3.0 * eta - 2.0;
            double t19 = eta * t13;
            t23 = eta * t4;
            DN_eta[0] = -t1 * t2 / 2 - 3.0 / 2.0 * t5 - 3.0 / 2.0 * t4 * t1;
            DN_eta[1] = -9.0 / 2.0 * xi * t2 - 27.0 / 2.0 * t9;
            DN_eta[2] = -9.0 / 2.0 * t12;
            DN_eta[3] = 0.0;
            DN_eta[4] = 9.0 / 2.0 * t12;
            DN_eta[5] = 9.0 / 2.0 * xi * t13 + 27.0 / 2.0 * t15;
            DN_eta[6] = t17 * t13 / 2 + 3.0 / 2.0 * t19 + 3.0 / 2.0 * eta * t17;
            DN_eta[7] = 9.0 / 2.0 * t4 * t13 - 9.0 / 2.0 * t19 + 27.0 / 2.0 * t23;
            DN_eta[8] = -9.0 / 2.0 * eta * t2 + 9.0 / 2.0 * t5 - 27.0 / 2.0 * t23;
            DN_eta[9] = -27.0 * t15 + 27.0 * t9;
        }
        // вычисление  координат i узла
        public override void CalkVertex(uint IdxKnot, ref double _x, ref double _y)
        {
            double[] x = { 0.0, 1.0 / 3.0, 2.0 / 3.0, 1.0, 2.0 / 3.0, 1.0 / 3.0, 0.0, 0.0, 0.0, 1.0 / 3.0 };
            double[] y = { 0.0, 0.0, 0.0, 0.0, 1.0 / 3.0, 2.0 / 3.0, 1.0, 2.0 / 3.0, 1.0 / 3.0, 1.0 / 3.0 };
            _x = x[IdxKnot];
            _y = y[IdxKnot];
        }
        public override uint GetBoundFormType() { return 4; }
    }
}
