//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 04.03.2024 Потапов И.И.
//---------------------------------------------------------------------------
//                      Сбока No Property River Lib
//              с убранными из наследников Property классами 
//---------------------------------------------------------------------------
namespace MeshGeneratorsLib.StripGenerator
{
    using System;
    using CommonLib;
    using CommonLib.Mesh;

    /// <summary>
    /// Маркировка границ в простой области
    /// </summary>
    [Serializable]
    public enum SimpleMarkerArea
    {
        /// <summary>
        /// Речной створ, определен дном (bed) и свободной поверхностью (WL)
        /// Маркеры: bed 0, WL 2 
        /// </summary>
        crossSectionRiver = 0,
        /// <summary>
        /// Трапециидальный канал с боковыми стенками/берегами 
        /// Маркеры: bed 0, right 0, WL 2 , left 0
        /// </summary>
        crossSectionTrapezoid = 1,
        /// <summary>
        /// С правой вертикальной стенкой
        /// Маркеры: bed 0, right wall 1, WL 2 
        /// </summary>
        crossSectionRiverLeft = 2,
        /// <summary>
        /// С левой вертикальной стенкой
        /// Маркеры: bed 0,  WL 2 , left wall 3,
        /// </summary>
        crossSectionRiverRight = 3,
        /// <summary>
        /// Проточный канал с вертикальными боковыми стенками 
        /// Маркеры: bed 0, right 1, WL 2 , left 3
        /// </summary>
        boxCrossSection = 4,
    }

    /// <summary>
    /// Опции для генерации Ленточной КЭ сетки 
    /// </summary>
    [Serializable]
    public class CrossStripMeshOption
    {
        /// <summary>
        /// Тип формы канала в створе потока
        /// </summary>
        [Obsolete("Не используйте в новом коде, флаг устарел !!!", false)]
        public SСhannelForms channelSectionForms = SСhannelForms.porabolic;
        /// <summary>
        /// Ось симметрии
        /// </summary>
        [Obsolete("Не используйте в новом коде, флаг устарел !!!", false)]
        public bool AxisOfSymmetry;
        /// <summary>
        /// Тип сетки
        /// </summary>
        public TypeMesh typeMesh;
        /// <summary>
        /// Способ замыкания крутого берега
        /// </summary>
        public int BoundaryClose;
        /// <summary>
        /// Способ маркировки
        /// </summary>
        public SimpleMarkerArea markerArea;

        public CrossStripMeshOption(SimpleMarkerArea markerArea, TypeMesh typeMesh, int boundaryClose)
        {
            this.typeMesh = typeMesh;
            this.BoundaryClose = boundaryClose;
            this.markerArea = markerArea;

            this.AxisOfSymmetry = false;
            this.channelSectionForms =  SСhannelForms.porabolic;
        }

        public CrossStripMeshOption(SimpleMarkerArea markerArea, TypeMesh typeMesh, int boundaryClose, SСhannelForms channelSectionForms, bool axisOfSymmetry)
        {
            this.typeMesh = typeMesh;
            this.BoundaryClose = boundaryClose;
            this.markerArea = markerArea;

            this.AxisOfSymmetry = axisOfSymmetry;
            this.channelSectionForms = channelSectionForms;
        }
        public CrossStripMeshOption() : this(SimpleMarkerArea.crossSectionRiver, TypeMesh.Triangle, 0, SСhannelForms.porabolic, false) { }
    }
}
