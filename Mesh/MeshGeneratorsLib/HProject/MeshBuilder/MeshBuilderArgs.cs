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
    using CommonLib;
    using GeometryLib;
    /// <summary>
    /// Параметры генератора для одной подобласти
    /// </summary>
    public class MeshBuilderArgs
    {
        /// <summary>
        /// максимальный размер карты по х и у
        /// </summary>
        protected int MaxNodeX, MaxNodeY;
        /// <summary>
        /// ранг сетки
        /// </summary>
        protected TypeRangeMesh meshRange = TypeRangeMesh.mRange1;
        /// <summary>
        /// Тип сетки
        /// </summary>
        protected TypeMesh meshType = TypeMesh.MixMesh;
        /// <summary>
        /// карта номеров узлов
        /// </summary>
        protected int[][] Map = null;
        /// <summary>
        /// карта координат узлов
        /// </summary>
        protected VMapKnot[][] pMap = null;
        /// <summary>
        /// номер подобласти
        /// </summary>
        protected int area;
        /// <summary>
        /// флаг подобласти
        /// </summary>
        protected int flag;
        /// <summary>
        /// левая граница области
        /// </summary>
        protected List<int> Left = new List<int>();
        /// <summary>
        /// правая граница области
        /// </summary>
        protected List<int> Right = new List<int>();
        /// <summary>
        /// граничные сегменты после разбиения и ориентировки
        /// </summary>
        protected HMapSegment[] RealSegmRibs = new HMapSegment[4];

        public MeshBuilderArgs()
        {
        }
        public MeshBuilderArgs(MeshBuilderArgs p)
        {
            Set(p);
        }
        public void Set(MeshBuilderArgs p)
        {
            meshType = p.meshType;
            area = p.area;
            flag = p.flag;
            Map = p.Map;
            pMap = p.pMap;
            meshRange = p.meshRange;
            Left = p.Left;
            Right = p.Right;
            MaxNodeX = p.MaxNodeX;
            MaxNodeY = p.MaxNodeY;
            RealSegmRibs = p.RealSegmRibs;
        }
    }
}
