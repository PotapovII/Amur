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
    /// <summary>
    /// ОО: Генерация координат узлов путем инейной интерполяции координат
    /// </summary>
    public class AlgebraBuilder : AMeshBuilder
    {
        /// <summary>
        /// Название строителя КЭ сетки
        /// </summary>
        public override string Name => "Линейная интерполяция координат сетки";
        public AlgebraBuilder() { }
        /// <summary>
        /// Генерация координат узлов б.к.э. сетки
        /// </summary>
        public override void BuilderCoords()
        {
            // ===================================================
            // Запись полученных координат и параметров
            // ===================================================
            List<double> x = new List<double>();
            List<double> y = new List<double>();
            double Xmin = pMap[pMap.Length - 1][0].x;
            double Xmax = pMap[0][pMap[0].Length - 1].x;
            double Ymax = pMap[pMap.Length - 1][0].y;
            double Ymin = pMap[0][pMap[0].Length - 1].y;
            double dx = (Xmax - Xmin) / (MaxNodeX - 1);
            double dy = (Ymax - Ymin) / (MaxNodeY - 1);
            for (int i = 0; i < MaxNodeY; i++)
            {
                double yy = i * dy + Ymin;
                for (int j = Left[i]; j < Right[i]; j++)
                {
                    double xx = Xmin + j * dx;
                    x.Add(xx);
                    y.Add(yy);
                }
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
            return new AlgebraBuilder();
        }
    }
}
