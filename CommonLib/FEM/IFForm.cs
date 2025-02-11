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
//                       ПРОЕКТ  "RiverLib"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 правка  :   04.03.2021 Потапов И.И.
//---------------------------------------------------------------------------
//------------------   добавлен  FunFormHelp    -----------------------------
//                 правка  :   15.07.2022 Потапов И.И.
//---------------------------------------------------------------------------
namespace CommonLib
{
    using System;
    using MemLogLib;
    /// <summary>
    /// ID функций формы, представленных в библиотеке
    /// </summary>
    [Serializable]
    public enum TypeFunForm
    {
        /// <summary>
        /// точка на отрезке
        /// </summary>
        Form_1D_L0 = 0,
        /// <summary>
        /// функции Лагранжа 1 порядка на отрезке (2 узла) 
        /// </summary>
        Form_1D_L1,
        /// <summary>
        /// функции Лагранжа 2 порядка на отрезке (3 узла) 
        /// </summary>
        Form_1D_L2,
        /// <summary>
        /// функции Лагранжа 3 порядка на отрезке (4 узла) 
        /// </summary>
        Form_1D_L3,
        /// <summary>
        /// функции Лагранжа 1 порядка на симплексе (3 узла) 
        /// </summary>
        Form_2D_TriangleAnalitic_L1,
        /// <summary>
        /// константа на симплексе (3 узла) ***
        /// </summary>
        Form_2D_Triangle_L0,
        /// <summary>
        /// функции Лагранжа 1 порядка на симплексе (3 узла) 
        /// </summary>
        Form_2D_Triangle_L1,
        /// <summary>
        /// функции 1 порядка на симплексе (3 узла) в центах граней
        /// </summary>
        Form_2D_Triangle_L1_River,
        /// <summary>
        /// функции 1 порядка на симплексе (3 узла) в центах граней
        /// </summary>
        Form_2D_Triangle_CR,
        /// <summary>
        /// функции Лагранжа 2 порядка на симплексе (6 узлов) 
        /// </summary>
        Form_2D_Triangle_L2,
        /// <summary>
        /// функции Лагранжа 3 порядка на симплексе (10 узлов) 
        /// </summary>
        Form_2D_Triangle_L3,
        /// <summary>
        /// костанта 0 порядка в квадрате (1 узел) ***
        /// </summary>
        Form_2D_Rectangle_L0,
        /// <summary>
        /// функции Лагранжа 1 порядка в квадрате  (4 узла) 
        /// </summary>
        Form_2D_Rectangle_L1,
        /// <summary>
        /// функции Серендита 2 порядка в квадрате  (8 узлов) 
        /// </summary>
        Form_2D_Rectangle_S2,
        /// <summary>
        /// функции Серендита 3 порядка в квадрате  (12 узлов) 
        /// </summary>
        Form_2D_Rectangle_S3,
        /// <summary>
        /// функции Лагранжа 2 порядка в квадрате  (9 узла) 
        /// </summary>
        Form_2D_Rectangle_L2,
        /// <summary>
        /// функции Лагранжа 3 порядка в квадрате  (16 узла) 
        /// </summary>
        Form_2D_Rectangle_L3,
        /// <summary>
        /// функции  Кроуза-Равиарта порядка в квадрате (4 узла) в центах граней
        /// </summary>
        Form_2D_Rectangle_CR,
        /// <summary>
        /// мои функции 2 порядка в квадрате (8 узла) в центах граней
        /// </summary>
        Form_2D_Rectangle_P,
        /// <summary>
        /// внутреннии не согласованные функции Лагранжа 1 порядка в квадрате  (4 узла) 
        /// </summary>
        Form_2D_Rectangle_Area4_L1N,
        /// <summary>
        /// внутреннии не согласованные функции Лагранжа 1 порядка в квадрате  (3 узла) 
        /// </summary>
        Form_2D_Rectangle_Area3_L1N,
        /// <summary>
        ///  мод функций Кроуза-Равиарта
        /// </summary>
        Form_2D_Rectangle_CRM,
        /// <summary>
        /// функции формы Эрмита (полные) 3 порядка в квадрате  (4 узла)
        /// </summary>
        Form_2D_Rectangle_Ermit4,
        /// <summary>
        /// функции формы Эрмита (полные) 3 порядка в квадрате  (4 узла)
        /// </summary>
        Form_2D_Triangle_Ermit3,
        Form_Zerro,
        Form_Unknown
    }
    /// <summary>
    /// Порядок КЭ сетки на которой определяется функция формы
    /// </summary>
    [Serializable]
    public enum IntRange
    {
        intRange1 = 1,
        intRange2,
        intRange3,
        intRange4
    }
    
    /// <summary>
    /// Справочник по функциям формы 
    /// </summary>
    public static class FunFormHelp
    {
        public static int CheckFF(TypeFunForm ff)
        {
            if((int)ff < 4) 
                return -1;   // линии
            else 
                if((int)ff < 11) 
                    return 0;  // треугольники
                else
                    return 1;  // прямоугольники
        }

        /// <summary>
        /// Получить инфоормацию для функции формы ff
        /// </summary>
        /// <param name="ff"></param>
        /// <param name="EKnotsCount">Количество узлов на КЭ для данной Ф.Ф./param>
        /// <param name="BEKnotsCount">Количество узлов на грани КЭ для данной Ф.Ф.</param>
        /// <param name="range">Ранг сетки</param>
        /// <param name="ceil">вид элемента</param>
        public static void GetFunFormInfo(TypeFunForm ff, out int EKnotsCount, 
           out int BEKnotsCount, out TypeRangeMesh range, out TypeMesh ceil)
        {
            ceil = 0;
            EKnotsCount = 0;
            BEKnotsCount = 0;
            range = TypeRangeMesh.mRange1;
            switch (ff)
            {
                case TypeFunForm.Form_1D_L0:
                    EKnotsCount = 1;
                    BEKnotsCount = 0;
                    range = TypeRangeMesh.mRange0;
                    ceil = TypeMesh.Line;
                    break;
                case TypeFunForm.Form_1D_L1:
                    EKnotsCount = 2;
                    BEKnotsCount = 1;
                    range = TypeRangeMesh.mRange1;
                    ceil = TypeMesh.Line;
                    break;
                case TypeFunForm.Form_1D_L2:
                    EKnotsCount = 3;
                    BEKnotsCount = 1;
                    range = TypeRangeMesh.mRange2;
                    ceil = TypeMesh.Line;
                    break;
                case TypeFunForm.Form_1D_L3:
                    EKnotsCount = 4;
                    BEKnotsCount = 1;
                    range = TypeRangeMesh.mRange3;
                    ceil = TypeMesh.Line;
                    break;
                case TypeFunForm.Form_2D_TriangleAnalitic_L1:
                    EKnotsCount = 3;
                    BEKnotsCount = 2;
                    range = TypeRangeMesh.mRange1;
                    ceil = TypeMesh.Triangle;
                    break;
                case TypeFunForm.Form_2D_Triangle_L0:
                    EKnotsCount = 1;
                    BEKnotsCount = 0;
                    range = TypeRangeMesh.mRange0;
                    ceil = TypeMesh.Triangle;
                    break;
                case TypeFunForm.Form_2D_Triangle_L1:
                    EKnotsCount = 3;
                    BEKnotsCount = 2;
                    range = TypeRangeMesh.mRange1;
                    ceil = TypeMesh.Triangle;
                    break;
                case TypeFunForm.Form_2D_Triangle_L1_River:
                    EKnotsCount = 3;
                    BEKnotsCount = 2;
                    range = TypeRangeMesh.mRange1;
                    ceil = TypeMesh.Triangle;
                    break;
                case TypeFunForm.Form_2D_Triangle_CR:
                    EKnotsCount = 3;
                    BEKnotsCount = 1;
                    range = TypeRangeMesh.mRange1;
                    ceil = TypeMesh.Triangle;
                    break;
                case TypeFunForm.Form_2D_Triangle_L2:
                    EKnotsCount = 6;
                    BEKnotsCount = 3;
                    range = TypeRangeMesh.mRange2;
                    ceil = TypeMesh.Triangle;
                    break;
                case TypeFunForm.Form_2D_Triangle_L3:
                    EKnotsCount = 10;
                    BEKnotsCount = 4;
                    range = TypeRangeMesh.mRange3;
                    ceil = TypeMesh.Triangle;
                    break;
                case TypeFunForm.Form_2D_Rectangle_L0:
                    EKnotsCount = 1;
                    BEKnotsCount = 0;
                    range = TypeRangeMesh.mRange0;
                    ceil = TypeMesh.Rectangle;
                    break;
                case TypeFunForm.Form_2D_Rectangle_L1:
                    EKnotsCount = 4;
                    BEKnotsCount = 2;
                    range = TypeRangeMesh.mRange1;
                    ceil = TypeMesh.Rectangle;
                    break;
                case TypeFunForm.Form_2D_Rectangle_S2:
                    EKnotsCount = 8;
                    BEKnotsCount = 3;
                    range = TypeRangeMesh.mRange2;
                    ceil = TypeMesh.Rectangle;
                    break;
                case TypeFunForm.Form_2D_Rectangle_S3:
                    EKnotsCount = 12;
                    BEKnotsCount = 4;
                    range = TypeRangeMesh.mRange3;
                    ceil = TypeMesh.Rectangle;
                    break;
                case TypeFunForm.Form_2D_Rectangle_L2:
                    EKnotsCount = 9;
                    BEKnotsCount = 3;
                    range = TypeRangeMesh.mRange2;
                    ceil = TypeMesh.Rectangle;
                    break;
                case TypeFunForm.Form_2D_Rectangle_L3:
                    EKnotsCount = 16;
                    BEKnotsCount = 4;
                    range = TypeRangeMesh.mRange3;
                    ceil = TypeMesh.Rectangle;
                    break;

                #region Уточнить
                case TypeFunForm.Form_2D_Rectangle_CR:
                    EKnotsCount = 4;
                    BEKnotsCount = 1;
                    range = TypeRangeMesh.mRange1;
                    ceil = TypeMesh.Rectangle;
                    break;
                case TypeFunForm.Form_2D_Rectangle_P:
                    EKnotsCount = 4;
                    BEKnotsCount = 0;
                    range = TypeRangeMesh.mRange0;
                    ceil = TypeMesh.Rectangle;
                    break;
                case TypeFunForm.Form_2D_Rectangle_Area4_L1N:
                    EKnotsCount = 4;
                    range = TypeRangeMesh.mRange1;
                    BEKnotsCount = 0;
                    ceil = TypeMesh.Rectangle;
                    break;
                case TypeFunForm.Form_2D_Rectangle_Area3_L1N:
                    EKnotsCount = 3;
                    BEKnotsCount = 0;
                    range = TypeRangeMesh.mRange1;
                    ceil = TypeMesh.Rectangle;
                    break;
                case TypeFunForm.Form_2D_Rectangle_CRM:
                    EKnotsCount = 4;
                    BEKnotsCount = 2;
                    range = TypeRangeMesh.mRange1;
                    ceil = TypeMesh.Rectangle;
                    break;
                #endregion 
            }
        }

        /// <summary>
        /// Получить количество узлов на конечном элементе для выбранной функции формы ff
        /// </summary>
        public static int GetFEKnots(TypeFunForm ff)
        {
            int EKnotsCount;
            switch (ff)
            {
                case TypeFunForm.Form_1D_L0:
                    EKnotsCount = 1;
                    break;
                case TypeFunForm.Form_1D_L1:
                    EKnotsCount = 2;
                    break;
                case TypeFunForm.Form_1D_L2:
                    EKnotsCount = 3;
                    break;
                case TypeFunForm.Form_1D_L3:
                    EKnotsCount = 4;
                    break;
                case TypeFunForm.Form_2D_TriangleAnalitic_L1:
                    EKnotsCount = 3;
                    break;
                case TypeFunForm.Form_2D_Triangle_L0:
                    EKnotsCount = 1;
                    break;
                case TypeFunForm.Form_2D_Triangle_L1:
                    EKnotsCount = 3;
                    break;
                case TypeFunForm.Form_2D_Triangle_L1_River:
                    EKnotsCount = 3;
                    break;
                case TypeFunForm.Form_2D_Triangle_CR:
                    EKnotsCount = 3;
                    break;
                case TypeFunForm.Form_2D_Triangle_L2:
                    EKnotsCount = 6;
                    break;
                case TypeFunForm.Form_2D_Triangle_L3:
                    EKnotsCount = 10;
                    break;
                case TypeFunForm.Form_2D_Rectangle_L0:
                    EKnotsCount = 1;
                    break;
                case TypeFunForm.Form_2D_Rectangle_L1:
                    EKnotsCount = 4;
                    break;
                case TypeFunForm.Form_2D_Rectangle_S2:
                    EKnotsCount = 8;
                    break;
                case TypeFunForm.Form_2D_Rectangle_S3:
                    EKnotsCount = 12;
                    break;
                case TypeFunForm.Form_2D_Rectangle_L2:
                    EKnotsCount = 9;
                    break;
                case TypeFunForm.Form_2D_Rectangle_L3:
                    EKnotsCount = 16;
                    break;
                default:
                   throw new Exception("Не поддерживается!");
            }
            return EKnotsCount;
        }

        /// <summary>
        /// Получить количество узлов на граничном конечном элементе для выбранной функции формы ff
        /// </summary>
        public static int GetBoundFEKnots(TypeFunForm ff)
        {
            int BEKnotsCount;
            switch (ff)
            {
                case TypeFunForm.Form_1D_L0:
                    BEKnotsCount = 0;
                    break;
                case TypeFunForm.Form_1D_L1:
                    BEKnotsCount = 1;
                    break;
                case TypeFunForm.Form_1D_L2:
                    BEKnotsCount = 1;
                    break;
                case TypeFunForm.Form_1D_L3:
                    BEKnotsCount = 1;
                    break;
                case TypeFunForm.Form_2D_TriangleAnalitic_L1:
                    BEKnotsCount = 2;
                    break;
                case TypeFunForm.Form_2D_Triangle_L0:
                    BEKnotsCount = 0;
                    break;
                case TypeFunForm.Form_2D_Triangle_L1:
                    BEKnotsCount = 2;
                    break;
                case TypeFunForm.Form_2D_Triangle_L1_River:
                    BEKnotsCount = 2;
                    break;
                case TypeFunForm.Form_2D_Triangle_CR:
                    BEKnotsCount = 1;
                    break;
                case TypeFunForm.Form_2D_Triangle_L2:
                    BEKnotsCount = 3;
                    break;
                case TypeFunForm.Form_2D_Triangle_L3:
                    BEKnotsCount = 4;
                    break;
                case TypeFunForm.Form_2D_Rectangle_L0:
                    BEKnotsCount = 0;
                    break;
                case TypeFunForm.Form_2D_Rectangle_L1:
                    BEKnotsCount = 2;
                    break;
                case TypeFunForm.Form_2D_Rectangle_S2:
                    BEKnotsCount = 3;
                    break;
                case TypeFunForm.Form_2D_Rectangle_S3:
                    BEKnotsCount = 4;
                    break;
                case TypeFunForm.Form_2D_Rectangle_L2:
                    BEKnotsCount = 3;
                    break;
                case TypeFunForm.Form_2D_Rectangle_L3:
                    BEKnotsCount = 4;
                    break;
                default:
                    throw new Exception("Не поддерживается!");
            }
            return BEKnotsCount;
        }

        /// <summary>
        /// Получить количество 2 узловых КЭ покрывающих границу для КЭ с выбранной функцией формы ff
        /// </summary>
        public static int GetCountTwoElemes(TypeFunForm ff)
        {
            int BEKnotsCount;
            switch (ff)
            {
                case TypeFunForm.Form_2D_TriangleAnalitic_L1:
                    BEKnotsCount = 1;
                    break;
                case TypeFunForm.Form_2D_Triangle_L1:
                    BEKnotsCount = 1;
                    break;
                case TypeFunForm.Form_2D_Triangle_L1_River:
                    BEKnotsCount = 1;
                    break;
                case TypeFunForm.Form_2D_Triangle_CR:
                    BEKnotsCount = 1;
                    break;
                case TypeFunForm.Form_2D_Triangle_L2:
                    BEKnotsCount = 2;
                    break;
                case TypeFunForm.Form_2D_Triangle_L3:
                    BEKnotsCount = 3;
                    break;
                case TypeFunForm.Form_2D_Rectangle_L1:
                    BEKnotsCount = 1;
                    break;
                case TypeFunForm.Form_2D_Rectangle_S2:
                    BEKnotsCount = 2;
                    break;
                case TypeFunForm.Form_2D_Rectangle_S3:
                    BEKnotsCount = 3;
                    break;
                case TypeFunForm.Form_2D_Rectangle_L2:
                    BEKnotsCount = 2;
                    break;
                case TypeFunForm.Form_2D_Rectangle_L3:
                    BEKnotsCount = 3;
                    break;
                default:
                    throw new Exception("Не поддерживается!");
            }
            return BEKnotsCount;
        }



        /// <summary>
        /// Получить функцию формы ребра для выбранной функции формы на конечном элементе ff
        /// </summary>
        public static TypeFunForm GetBoundFunFormForGeometry(TypeFunForm ff)
        {
            switch (ff)
            {
                case TypeFunForm.Form_1D_L0:
                case TypeFunForm.Form_1D_L1:
                case TypeFunForm.Form_1D_L2:
                case TypeFunForm.Form_1D_L3:
                    return TypeFunForm.Form_Zerro;

                case TypeFunForm.Form_2D_Triangle_L0:
                case TypeFunForm.Form_2D_Rectangle_L0:
                case TypeFunForm.Form_2D_Triangle_L1:
                case TypeFunForm.Form_2D_TriangleAnalitic_L1:
                case TypeFunForm.Form_2D_Triangle_L1_River:
                case TypeFunForm.Form_2D_Triangle_CR:
                case TypeFunForm.Form_2D_Rectangle_L1:
                    return TypeFunForm.Form_1D_L1;

                case TypeFunForm.Form_2D_Triangle_L2:
                case TypeFunForm.Form_2D_Rectangle_S2:
                case TypeFunForm.Form_2D_Rectangle_L2:
                    return TypeFunForm.Form_1D_L2;

                case TypeFunForm.Form_2D_Triangle_L3:
                case TypeFunForm.Form_2D_Rectangle_S3:
                case TypeFunForm.Form_2D_Rectangle_L3:
                    return TypeFunForm.Form_1D_L3;
                default:
                    throw new Exception("Не поддерживается!");
            }
        }
        /// <summary>
        /// Получить функцию формы ребра для выбранной функции формы на конечном элементе ff
        /// </summary>
        public static TypeFunForm GetBoundFunFormForAprox(TypeFunForm ff)
        {
            switch (ff)
            {
                case TypeFunForm.Form_1D_L0:
                case TypeFunForm.Form_1D_L1:
                case TypeFunForm.Form_1D_L2:
                case TypeFunForm.Form_1D_L3:
                case TypeFunForm.Form_2D_Triangle_L0:
                case TypeFunForm.Form_2D_Rectangle_L0:
                    return TypeFunForm.Form_Zerro;

                case TypeFunForm.Form_2D_Triangle_CR:
                case TypeFunForm.Form_2D_Rectangle_CRM:
                    return TypeFunForm.Form_1D_L0;

                case TypeFunForm.Form_2D_Triangle_L1:
                case TypeFunForm.Form_2D_TriangleAnalitic_L1:
                case TypeFunForm.Form_2D_Triangle_L1_River:
                case TypeFunForm.Form_2D_Rectangle_L1:

                    return TypeFunForm.Form_1D_L1;

                case TypeFunForm.Form_2D_Triangle_L2:
                case TypeFunForm.Form_2D_Rectangle_S2:
                case TypeFunForm.Form_2D_Rectangle_L2:
                    return TypeFunForm.Form_1D_L2;

                case TypeFunForm.Form_2D_Triangle_L3:
                case TypeFunForm.Form_2D_Rectangle_S3:
                case TypeFunForm.Form_2D_Rectangle_L3:
                    return TypeFunForm.Form_1D_L3;
                default:
                    throw new Exception("Не поддерживается!");
            }
        }


        /// <summary>
        /// Получить инфоормацию для функции формы ff
        /// </summary>
        /// <param name="ff"></param>
        /// <param name="EKnotsCount">Количество узлов на КЭ для данной Ф.Ф./param>
        /// <param name="BEKnotsCount">Количество узлов на грани КЭ для данной Ф.Ф.</param>
        /// <param name="range">Ранг сетки</param>
        /// <param name="ceil">вид элемента</param>
        public static int GetCountTriElement(TypeFunForm ff)
        {
            int Count;
            switch (ff)
            {
                case TypeFunForm.Form_2D_TriangleAnalitic_L1:
                    Count = 1;
                    break;
                case TypeFunForm.Form_2D_Triangle_L0:
                    Count = 1;
                    break;
                case TypeFunForm.Form_2D_Triangle_L1:
                    Count = 1;
                    break;
                case TypeFunForm.Form_2D_Triangle_L1_River:
                    Count = 1;
                    break;
                case TypeFunForm.Form_2D_Triangle_CR:
                    Count = 1;
                    break;
                case TypeFunForm.Form_2D_Triangle_L2:
                    Count = 4;
                    break;
                case TypeFunForm.Form_2D_Triangle_L3:
                    Count = 9;
                    break;
                case TypeFunForm.Form_2D_Rectangle_L0:
                    Count = 2;
                    break;
                case TypeFunForm.Form_2D_Rectangle_L1:
                    Count = 2;
                    break;
                case TypeFunForm.Form_2D_Rectangle_S2:
                    Count = 6;
                    break;
                case TypeFunForm.Form_2D_Rectangle_S3:
                    Count = 10;
                    break;
                case TypeFunForm.Form_2D_Rectangle_L2:
                    Count = 8;
                    break;
                case TypeFunForm.Form_2D_Rectangle_L3:
                    Count = 18;
                    break;
                default:
                    throw new Exception("Не поддерживается! тип "+ ff.ToString() + " в методе IFForm.GetCountTriElement");
            }
            return Count;
        }



        /// <summary>
        /// Адаптер для отрисовки. 
        /// Массив конечных элементов приводится массиву трехузловых конечных элементов 
        /// </summary>
        public static void GetTriElem(TypeFunForm ff, uint[] Knots, ref TriElement[] triElements)
        {
            int CountTriFE = FunFormHelp.GetCountTriElement(ff);
            int Count = FunFormHelp.GetFEKnots(ff);
            MEM.Alloc(CountTriFE, ref triElements);
            CountTriFE = 0;
            switch (ff)
            {
                case TypeFunForm.Form_2D_TriangleAnalitic_L1:
                case TypeFunForm.Form_2D_Triangle_L0:
                case TypeFunForm.Form_2D_Triangle_L1:
                case TypeFunForm.Form_2D_Triangle_L1_River:
                case TypeFunForm.Form_2D_Triangle_CR:
                    triElements[CountTriFE++] = new TriElement(Knots[0], Knots[1], Knots[2]);
                    break;
                case TypeFunForm.Form_2D_Triangle_L2:
                    triElements[CountTriFE++] = new TriElement(Knots[0], Knots[1], Knots[5]);
                    triElements[CountTriFE++] = new TriElement(Knots[1], Knots[3], Knots[5]);
                    triElements[CountTriFE++] = new TriElement(Knots[1], Knots[2], Knots[3]);
                    triElements[CountTriFE++] = new TriElement(Knots[5], Knots[3], Knots[4]);
                    break;
                case TypeFunForm.Form_2D_Triangle_L3:
                    triElements[CountTriFE++] = new TriElement(Knots[0], Knots[1], Knots[8]);
                    triElements[CountTriFE++] = new TriElement(Knots[1], Knots[9], Knots[8]);
                    triElements[CountTriFE++] = new TriElement(Knots[1], Knots[2], Knots[9]);
                    triElements[CountTriFE++] = new TriElement(Knots[2], Knots[4], Knots[9]);
                    triElements[CountTriFE++] = new TriElement(Knots[2], Knots[3], Knots[4]);

                    triElements[CountTriFE++] = new TriElement(Knots[8], Knots[9], Knots[7]);
                    triElements[CountTriFE++] = new TriElement(Knots[9], Knots[5], Knots[7]);
                    triElements[CountTriFE++] = new TriElement(Knots[9], Knots[4], Knots[5]);

                    triElements[CountTriFE++] = new TriElement(Knots[7], Knots[5], Knots[6]);
                    break;
                case TypeFunForm.Form_2D_Rectangle_L0:
                case TypeFunForm.Form_2D_Rectangle_L1:
                    triElements[CountTriFE++] = new TriElement(Knots[0], Knots[1], Knots[2]);
                    triElements[CountTriFE++] = new TriElement(Knots[0], Knots[2], Knots[3]);
                    break;
                case TypeFunForm.Form_2D_Rectangle_S2:
                    triElements[CountTriFE++] = new TriElement(Knots[0], Knots[1], Knots[7]);
                    triElements[CountTriFE++] = new TriElement(Knots[1], Knots[2], Knots[3]);
                    triElements[CountTriFE++] = new TriElement(Knots[3], Knots[4], Knots[5]);
                    triElements[CountTriFE++] = new TriElement(Knots[5], Knots[6], Knots[7]);
                    triElements[CountTriFE++] = new TriElement(Knots[7], Knots[1], Knots[5]);
                    triElements[CountTriFE++] = new TriElement(Knots[1], Knots[3], Knots[5]);
                    break;
                case TypeFunForm.Form_2D_Rectangle_S3:
                    triElements[CountTriFE++] = new TriElement(Knots[0], Knots[1], Knots[11]);
                    triElements[CountTriFE++] = new TriElement(Knots[2], Knots[3], Knots[4]);
                    triElements[CountTriFE++] = new TriElement(Knots[5], Knots[6], Knots[7]);
                    triElements[CountTriFE++] = new TriElement(Knots[8], Knots[9], Knots[10]);

                    triElements[CountTriFE++] = new TriElement(Knots[11], Knots[1], Knots[10]);
                    triElements[CountTriFE++] = new TriElement(Knots[10], Knots[1], Knots[8]);

                    triElements[CountTriFE++] = new TriElement(Knots[1], Knots[7], Knots[8]);
                    triElements[CountTriFE++] = new TriElement(Knots[1], Knots[2], Knots[7]);

                    triElements[CountTriFE++] = new TriElement(Knots[2], Knots[4], Knots[5]);
                    triElements[CountTriFE++] = new TriElement(Knots[2], Knots[5], Knots[7]);
                    break;
                case TypeFunForm.Form_2D_Rectangle_L2:
                    triElements[CountTriFE++] = new TriElement(Knots[0], Knots[1], Knots[7]);
                    triElements[CountTriFE++] = new TriElement(Knots[1], Knots[8], Knots[7]);
                    triElements[CountTriFE++] = new TriElement(Knots[1], Knots[2], Knots[3]);
                    triElements[CountTriFE++] = new TriElement(Knots[3], Knots[8], Knots[1]);
                    triElements[CountTriFE++] = new TriElement(Knots[8], Knots[3], Knots[5]);
                    triElements[CountTriFE++] = new TriElement(Knots[3], Knots[4], Knots[5]);
                    triElements[CountTriFE++] = new TriElement(Knots[7], Knots[8], Knots[5]);
                    triElements[CountTriFE++] = new TriElement(Knots[7], Knots[5], Knots[6]);
                    break;
                case TypeFunForm.Form_2D_Rectangle_L3:
                    triElements[CountTriFE++] = new TriElement(Knots[0], Knots[1], Knots[11]);
                    triElements[CountTriFE++] = new TriElement(Knots[1], Knots[12], Knots[11]);
                    triElements[CountTriFE++] = new TriElement(Knots[1], Knots[2], Knots[12]);
                    triElements[CountTriFE++] = new TriElement(Knots[2], Knots[13], Knots[12]);
                    triElements[CountTriFE++] = new TriElement(Knots[2], Knots[3], Knots[13]);
                    triElements[CountTriFE++] = new TriElement(Knots[3], Knots[4], Knots[13]);

                    triElements[CountTriFE++] = new TriElement(Knots[11], Knots[12], Knots[10]);
                    triElements[CountTriFE++] = new TriElement(Knots[12], Knots[15], Knots[10]);
                    triElements[CountTriFE++] = new TriElement(Knots[12], Knots[13], Knots[14]);
                    triElements[CountTriFE++] = new TriElement(Knots[12], Knots[14], Knots[15]);
                    triElements[CountTriFE++] = new TriElement(Knots[13], Knots[4], Knots[14]);
                    triElements[CountTriFE++] = new TriElement(Knots[4], Knots[5], Knots[14]);

                    triElements[CountTriFE++] = new TriElement(Knots[10], Knots[15], Knots[9]);
                    triElements[CountTriFE++] = new TriElement(Knots[15], Knots[8], Knots[9]);
                    triElements[CountTriFE++] = new TriElement(Knots[15], Knots[14], Knots[8]);
                    triElements[CountTriFE++] = new TriElement(Knots[14], Knots[7], Knots[8]);
                    triElements[CountTriFE++] = new TriElement(Knots[14], Knots[5], Knots[7]);
                    triElements[CountTriFE++] = new TriElement(Knots[5], Knots[6], Knots[7]);
                    break;
                default:
                    throw new Exception("Не поддерживается!");
            }
        }
        
        public static void GetBoundTwoElems(TypeFunForm ffb, uint[] Knots, ref TwoElement[] BElements)
        {
            switch (ffb)
            {
                case TypeFunForm.Form_1D_L1:
                    {
                        MEM.Alloc(1, ref BElements);
                        BElements[0] = new TwoElement(Knots[0], Knots[1]);
                        return;
                    }
                case TypeFunForm.Form_1D_L2:
                    {
                        MEM.Alloc(2, ref BElements);
                        BElements[0] = new TwoElement(Knots[0], Knots[1]);
                        BElements[1] = new TwoElement(Knots[1], Knots[2]);
                        return;
                    }
                case TypeFunForm.Form_1D_L3:
                    {
                        MEM.Alloc(3, ref BElements);
                        BElements[0] = new TwoElement(Knots[0], Knots[1]);
                        BElements[1] = new TwoElement(Knots[1], Knots[2]);
                        BElements[2] = new TwoElement(Knots[2], Knots[3]);
                        return;
                    }
                default:
                    throw new Exception("Не поддерживается!");
            }
        }


        public static int GetCountBoundTwoElems(TypeFunForm ffb)
        {
            switch (ffb)
            {
                case TypeFunForm.Form_1D_L1:
                        return 1;
                case TypeFunForm.Form_1D_L2:
                        return 2;
                case TypeFunForm.Form_1D_L3:
                        return 3;
                default:
                    throw new Exception("Не поддерживается!");
            }
        }

    }

    /// <summary>
    /// ОО:  Функции формы КЭ
    /// </summary>
    public interface IFForm
    {
        /// <summary>
        /// Рекомендуемый порядок интегрирования функции формы
        /// </summary>
        IntRange MeshRange { get; }
        /// <summary>
        /// идентификатор функции формы
        /// </summary>
        TypeFunForm ID { get; }
        /// <summary>
        /// вычисление значений функций формы
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        void CalkForm(double x, double y);
        /// <summary>
        /// вычисление значений функций формы ее производных
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        void CalkDiffForm(double x, double y, double[,] BWM = null);
        /// <summary>
        /// вычисление  координат i узла
        /// </summary>
        /// <param name="IdxKnot"></param>
        /// <param name="_x"></param>
        /// <param name="_y"></param>
        void CalkVertex(uint IdxKnot, ref double _x, ref double _y);
        /// <summary>
        /// вычисление локальных производных функций формы
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        void CalkLocalDiffForm(double x, double y);
        /// <summary>
        /// Тип функций формы на гранях КЭ
        /// </summary>
        /// <returns></returns>
        uint GetBoundFormType();
        /// <summary>
        /// Вычисление произведение значений в узлах на функцию формы в точке интегрирования
        /// </summary>
        /// <param name="ff"></param>
        /// <param name="mas"></param>
        /// <returns></returns>
        double FFValue(double[] mas);
    }

    /// <summary>
    /// ОО:  Функции Лагранжа
    /// </summary>
    public interface IFunLagrange1D
    {
        /// <summary>
        /// вычисление значений функций формы
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        double CalkForm(double xp, double Ua, double Ub, double Uc = 0);
        /// <summary>
        /// вычисление значений функций формы
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        double CalkForm(double xp, double[] U);
    }
}

