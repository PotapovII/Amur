//---------------------------------------------------------------------------
//                          ПРОЕКТ  "H?"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                кодировка : 06.06.2006 Потапов И.И. (c++)
//---------------------------------------------------------------------------
//           кодировка : 02.04.2021 Потапов И.И. (c++=> c#)
//          при переносе не были сохранены механизмы поддержки
//          изменения параметров задачи во времени отвечающие за
//        переменные граничные условия для/на граничных сегментах 
//---------------------------------------------------------------------------
//              изменен способ подключения к IAlgebra 
//                  библиотека пока не та :(
//---------------------------------------------------------------------------
namespace MeshGeneratorsLib
{
    using System;
    using System.Collections.Generic;
    using AlgebraLib;
    using GeometryLib;
    class HIterOrtoCalcCoords : ACalcCoords
    {
        //  Компонент алгебра используемый для решения САУ
        AlgebraGauss Algebra = null;
        public HIterOrtoCalcCoords(int MNodeX, int MNodeY,
        List<int> _L, List<int> _R, VMapKnot[][] _pMap)
                   :base(MNodeX, MNodeY, _L, _R, _pMap)
        {
        }
        /// <summary>
        /// создание матрицы системы
        /// </summary>
        /// <param name="TypeAlgebra"></param>
        public override void Solve()
        {
            try
            {
                base.Solve();
                CalcCoords();
            }
            catch
            {
                Console.WriteLine("Проблемы с генерацией КЭ сетки : HCalcCoords::Solve");
            }
        }
        public override void CalcCoords()
        {
            Algebra = new AlgebraGauss((uint)MKnots);
            //Вычисляем координаты Х
            for (int i = 0; i < MaxNodeY; i++)
                for (int j = Left[i]; j < Right[i]; j++)
                    Field[i][j] = pMap[i][j].x;
            Calc();
            MKnots = 0;
            for (int i = 1; i < MaxNodeY - 1; i++)
                for (int j = Left[i] + 1; j < Right[i] - 1; j++)
                {
                    // Глобальная нумерация узлов
                    pMap[i][j].x = XResult[MKnots];
                    MKnots++;
                }
            //
            // Вычисляем координаты Y
            //
            // Количество неизвестных
            Algebra = new AlgebraGauss((uint)MKnots);
            for (int i = 0; i < MaxNodeY; i++)
                for (int j = Left[i]; j < Right[i]; j++)
                    Field[i][j] = pMap[i][j].y;
            Calc();
            MKnots = 0;
            for (int i = 1; i < MaxNodeY - 1; i++)
                for (int j = Left[i] + 1; j < Right[i] - 1; j++)
                {
                    // Глобальная нумерация узлов
                    pMap[i][j].y = XResult[MKnots];
                    MKnots++;
                }
        }
        // Поиск параметра или координаты
        public void Calc()
        {
            IndexRow = 0;
            for (int i = 1; i < MaxNodeY - 1; i++)
            {
                for (int j = Left[i] + 1; j < Right[i] - 1; j++)
                {
                    CountCol = 0;
                    R = 0;
                    // Для линейного варианта узлы помечены (..)
                    //
                    // iwn     (inn)     ien
                    //
                    //
                    // (iw)    (ip)     (ie)
                    //
                    //
                    // iws     (iss)     ies
                    //
                    // формирование индексов строки
                    int ip = KnotMap[i][j];
                    int ie = KnotMap[i][j + 1];
                    int iw = KnotMap[i][j - 1];
                    int iss = KnotMap[i + 1][j];
                    int inn = KnotMap[i - 1][j];
                    //
                    int ien = KnotMap[i - 1][j + 1];
                    int iwn = KnotMap[i - 1][j - 1];
                    //
                    int ies = KnotMap[i + 1][j + 1];
                    int iws = KnotMap[i + 1][j - 1];
                    // Решение задачи Лапласа
                    // формирование строки
                    ColElems[CountCol] = 4;
                    ColAdress[CountCol] = (uint)ip;
                    CountCol++;
                    // справа
                    if (ie > -1)
                    {
                        ColElems[CountCol] = -1;
                        ColAdress[CountCol] = (uint)ie;
                        CountCol++;
                    }
                    else
                    {
                        R += Field[i][j + 1];
                    }
                    // слева
                    if (iw > -1)
                    {
                        ColElems[CountCol] = -1;
                        ColAdress[CountCol] = (uint)iw;
                        CountCol++;
                    }
                    else
                    {
                        R += Field[i][j - 1];
                    }
                    // сверху
                    if (inn > -1)
                    {
                        ColElems[CountCol] = -1;
                        ColAdress[CountCol] = (uint)inn;
                        CountCol++;
                    }
                    else
                    {
                        R += Field[i - 1][j];
                    }
                    // снизу
                    if (iss > -1)
                    {
                        ColElems[CountCol] = -1;
                        ColAdress[CountCol] = (uint)iss;
                        CountCol++;
                    }
                    else
                    {
                        R += Field[i + 1][j];
                    }
                    Algebra.AddStringSystem(ColElems, ColAdress, IndexRow, R);
                    IndexRow++;
                }
            }
            Algebra.Solve(ref XResult);
        }
    }
}
