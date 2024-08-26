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
//                  Разбит на подклассы 14 10 2022
//                             Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshGeneratorsLib
{
    using System.Collections.Generic;
    using AlgebraLib;
    using GeometryLib;
    using MemLogLib;
    abstract class ACalcCoords
    {
        //  Компонент алгебра используемый для решения САУ
        AlgebraGauss Algebra = null;
        // левая граница области
        protected List<int> Left;
        // правая граница области
        protected List<int> Right;
        protected int MaxNodeX, MaxNodeY;
        // Карта
        protected int[][] KnotMap;
        protected VMapKnot[][] pMap;
        protected double[][] Field;
        protected double[] XResult;
        protected int MKnots = 9;
        protected double[] ColElems = new double[9];
        protected uint[] ColAdress = new uint[9];
        protected uint CountCol, IndexRow;
        protected double R;
        public ACalcCoords(int MNodeX, int MNodeY,
        List<int> _L, List<int> _R, VMapKnot[][] _pMap)
        {
            pMap = _pMap;
            // левая граница области
            Left = _L;
            // правая граница области
            Right = _R;
            MaxNodeX = MNodeX;
            MaxNodeY = MNodeY;
        }
        /// <summary>
        /// создание матрицы системы
        /// </summary>
        /// <param name="TypeAlgebra"></param>
        public virtual void Solve()
        {
            // Создание карты внутренних узлов
            MEM.Alloc2DClear(MaxNodeY, MaxNodeX, ref KnotMap, -1);
            MEM.Alloc2DClear(MaxNodeY, MaxNodeX, ref Field, 0);
            // Вычисление количества узлов для расчета координат сетки
            MKnots = 0;
            for (int i = 1; i < MaxNodeY - 1; i++)
                for (int j = Left[i]; j < Right[i]; j++)
                {
                    // Глобальная нумерация узлов
                    KnotMap[i][j] = MKnots;
                    MKnots++;
                }
            //
            XResult = new double[MKnots];
            // переменные для формирование ГМЖ
            for (uint i = 0; i < 9; i++)
            {
                ColElems[i] = 0;
                ColAdress[i] = 0;
            }
        }
        /// <summary>
        /// Поиск параметра или координаты
        /// </summary>
        public abstract void CalcCoords();
    }
}
