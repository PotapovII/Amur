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
namespace MeshLib
{
    using System;
    using System.Runtime.CompilerServices;

    using CommonLib;
    /// <summary>
    /// ОО: Возвращает тип ф.ф., по рангу сетки
    /// </summary>
    public class HFForm1D
    {
        public static TypeFunForm GetTypeFunForm1D(int MeshRange)
        {
            switch (MeshRange)
            {
                case 1:
                    return TypeFunForm.Form_1D_L1;
                case 2:
                    return TypeFunForm.Form_1D_L2;
                case 3:
                    return TypeFunForm.Form_1D_L3;
                default:
                    return TypeFunForm.Form_1D_L0;
            }
        }
        public static TypeFunForm GetTypeFunForm1D(TypeRangeMesh MeshRange)
        {
            switch (MeshRange)
            {
                case TypeRangeMesh.mRange1:
                    return TypeFunForm.Form_1D_L1;
                case TypeRangeMesh.mRange2:
                    return TypeFunForm.Form_1D_L2;
                case TypeRangeMesh.mRange3:
                    return TypeFunForm.Form_1D_L3;
                default:
                    return TypeFunForm.Form_1D_L0;
            }
        }
    }
    /// <summary>
    /// ОО: Одномерные ф.ф., 1-х узловые, 0 порядок, Лагранж
    /// </summary>
    [Serializable]
    class HFForm1D_L0 : AbFunForm
    {
        public HFForm1D_L0()
        {
            FContinueType = FFContinueType.СontinuousAtNodes;
            GeomType = FFGeomType.Line;
            id = TypeFunForm.Form_1D_L0;
            meshRange = IntRange.intRange1;
            Dim = 1;
            Count = 2;
            Name = "Одномерные ф.ф., 1-х узловые, 0 порядок, Лагранж";
            Init(Count);
        }
        /// <summary>
        /// вычисление значений функций формы
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkForm(double x, double y)
        {
            N[0] = 0.5 * (1 - x);
            N[1] = 0.5 * (1 + x);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkLocalDiffForm(double x, double y)
        {
            DN_xi[0] = -0.5;
            DN_xi[1] = 0.5;
        }
        // вычисление  координат i узла
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkVertex(uint IdxKnot, ref double _x, ref double _y)
        {
            double[] x = { -1, 1 };
            _x = x[IdxKnot];
            _y = 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override uint GetBoundFormType() { return 0; }
    }
    //---------------------------------------------------------------------------
    /// <summary>
    /// ОО: Одномерные ф.ф., 2-х узловые, 1 порядок, Лагранж
    /// </summary>
    [Serializable]
    class HFForm1D_L1 : AbFunForm
    {
        public HFForm1D_L1()
        {
            FContinueType = FFContinueType.СontinuousAtNodes;
            GeomType = FFGeomType.Line;
            id = TypeFunForm.Form_1D_L1;
            meshRange = IntRange.intRange1;
            Dim = 1;
            Count = 2;
            Name = "Одномерные ф.ф., 2-х узловые, 1 порядок, Лагранж";
            Init(Count);
        }
        /// <summary>
        /// вычисление значений функций формы
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkForm(double x, double y)
        {
            N[0] = 0.5 * (1 - x);
            N[1] = 0.5 * (1 + x);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkLocalDiffForm(double x, double y)
        {
            DN_xi[0] = -0.5;
            DN_xi[1] = 0.5;
        }
        /// <summary>
        /// вычисление  координат i узла
        /// </summary>
        /// <param name="IdxKnot"></param>
        /// <param name="_x"></param>
        /// <param name="_y"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkVertex(uint IdxKnot, ref double _x, ref double _y)
        {
            double[] x = { -1, 1 };
            _x = x[IdxKnot];
            _y = 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override uint GetBoundFormType() { return 0; }
    }
    //---------------------------------------------------------------------------
    /// <summary>
    /// ОО: Одномерные ф.ф., 3-х узловые, 2 порядок, Лагранж
    /// </summary>
    [Serializable]
    class HFForm1D_L2 : AbFunForm
    {
        public HFForm1D_L2()
        {
            FContinueType = FFContinueType.СontinuousAtNodes;
            GeomType = FFGeomType.Line;
            id = TypeFunForm.Form_1D_L2;
            meshRange = IntRange.intRange2;
            Dim = 1;
            Count = 3;
            Name = "Одномерные ф.ф., 3-х узловые, 2 порядок, Лагранж";
            Init(Count);
        }
        // вычисление значений функций формы
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkForm(double x, double y)
        {
            N[0] = -0.5 * (1 - x) * x;
            N[1] = (1 - x * x);
            N[2] = 0.5 * (1 + x) * x;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkLocalDiffForm(double x, double y)
        {
            DN_xi[0] = -0.5 + x;
            DN_xi[1] = -2 * x;
            DN_xi[2] = 0.5 + x;
        }
        //---------------------------------------------------------------------------
        // вычисление  координат i узла
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkVertex(uint IdxKnot, ref double _x, ref double _y)
        {
            double[] x = { -1, 0, 1 };
            _x = x[IdxKnot];
            _y = 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        public override uint GetBoundFormType() { return 0; }
    }
    //---------------------------------------------------------------------------
    /// <summary>
    /// ОО: Одномерные ф.ф., 4-х узловые, 3 порядок, Лагранж
    /// </summary>
    [Serializable]
    class HFForm1D_L3 : AbFunForm
    {
        public HFForm1D_L3()
        {
            FContinueType = FFContinueType.СontinuousAtNodes;
            GeomType = FFGeomType.Line;
            id = TypeFunForm.Form_1D_L3;
            meshRange = IntRange.intRange3;
            Dim = 1;
            Count = 4;
            Name = "Одномерные ф.ф., 4-х узловые, 3 порядок, Лагранж";
            Init(Count);
        }
        //---------------------------------------------------------------------------
        // вычисление значений функций формы
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkForm(double x, double y)
        {
            double a = x * x;
            N[0] = -(1 - x) * (1 - 9.0 * a) / 16.0;
            N[1] = 9.0 * (1 - a) * (1 - 3 * x) / 16.0;
            N[2] = 9.0 * (1 - a) * (1 + 3 * x) / 16.0;
            N[3] = -(1 + x) * (1 - 9.0 * a) / 16.0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkLocalDiffForm(double x, double y)
        {
            double a = x * x;
            DN_xi[0] = (1.0 + 18.0 * x - 27.0 * a) / 16.0;
            DN_xi[1] = 9.0 * (-3 - 3 * x + 9 * a) / 16.0;
            DN_xi[2] = 9.0 * (3 - 3 * x - 9 * a) / 16.0;
            DN_xi[3] = (-1.0 - 18.0 * x - 27.0 * a) / 16.0;
        }
        // вычисление  координат i узла
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CalkVertex(uint IdxKnot, ref double _x, ref double _y)
        {
            double[] x = { -1, -1.0 / 3.0, 1.0 / 3.0, 1 };
            _x = x[IdxKnot];
            _y = 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override uint GetBoundFormType() { return 0; }
    }
}
