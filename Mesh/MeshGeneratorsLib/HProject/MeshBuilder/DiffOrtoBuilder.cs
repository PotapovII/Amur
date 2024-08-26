//---------------------------------------------------------------------------
//                         проектировщик:
//                           Потапов И.И.
//                  - (C) Copyright 2000-2003
//                      ALL RIGHT RESERVED
//---------------------------------------------------------------------------
//                 кодировка : 1.02.2003 Потапов И.И.
//               ПРОЕКТ  "MixTasker' на базе "DISER"
//---------------------------------------------------------------------------
//        Перенос на C#, вариант от : 05.02.2022  Потапов И.И.
//    реализация генерации базисной КЭ сетки без поддержки полей свойств
//                         убран 4 порядок сетки 
//---------------------------------------------------------------------------
namespace MeshGeneratorsLib
{
    using System.Collections.Generic;
    using GeometryLib;
    /// <summary>
    /// ОО: Генерация координат узлов б.к.э. сетки путем решения 
    /// нелинейных эллиптических дифференциальных уравнений
    /// </summary>
    public class DiffOrtoBuilder : AMeshBuilder
    {
        /// <summary>
        /// Название строителя КЭ сетки
        /// </summary>
        public override string Name => "Решение 1D НУ для координат узлов";

        /// <summary>
        /// Генерация координат узлов б.к.э. сетки
        /// </summary>
        public override void BuilderCoords()
        {
            HIterOrtoCalcCoords FCoords = new HIterOrtoCalcCoords(MaxNodeX, MaxNodeY, Left, Right, pMap);
            FCoords.Solve();
            // ===================================================
            // Запись полученных координат и параметров
            // ===================================================
            List<double> x = new List<double>();
            List<double> y = new List<double>();
            for (int i = 0; i < MaxNodeY; i++)
                for (int j = Left[i]; j < Right[i]; j++)
                {
                    x.Add(pMap[i][j].x);
                    y.Add(pMap[i][j].y);
                }
            SMesh.CoordsX = x.ToArray();
            SMesh.CoordsY = y.ToArray();
        }
        public override void BuilderParams() { }
        /// <summary>
        /// Клонируем билдер
        /// </summary>
        /// <returns></returns>
        public override IMeshBuilder Clone()
        {
            return new DiffOrtoBuilder();
        }
    }
}
