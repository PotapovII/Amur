//---------------------------------------------------------------------------
//                         проектировщик:
//                           Потапов И.И. 
//                кодировка : 26.06.2022 Потапов И.И. 
//---------------------------------------------------------------------------
namespace MeshLib
{
    using System;
    using CommonLib.Geometry;
    using GeometryLib;
    using MemLogLib;
    /// <summary>
    /// Образ контрольного объема для произвольной сетки
    /// 
    ///     | <------hw ------->|
    ///     
    ///    x0 y0               x3 y3   
    /// ---- |---------*---------|-----> j (y)
    ///  ^   |         ^         |  ^
    ///  |   |         |         |  |
    ///  |   |        dw         |  |
    ///  |   |         |         |  |
    ///  |   |         V         |  |
    /// hs   |<--ds--->*<---dn-->|  hn
    ///  |   |         ^         |  |
    ///  |   |         |         |  |
    ///  |   |        de         |  |
    ///  |   |         |         |  |
    ///  V   |         V         |  V
    /// ---- |---------*---------|---
    ///    x1 y1                x2 y2  
    ///      |                   
    ///      | <------he ------->|
    ///      |  
    ///      V  i (x)
    ///      
    /// </summary>
    public interface IFVElem
    {
        /// <summary>
        ///  Координаты центра КО
        /// </summary>
        HPoint p { get; set; }
        /// <summary>
        /// длина грани КО ( x0 y0 -  x1 y1 )
        /// </summary>
        double hs { get; set; }
        /// <summary>
        /// длина грани КО ( x1 y1 -  x2 y2 )
        /// </summary>
        double he { get; set; }
        /// <summary>
        /// длина грани КО ( x2 y2 -  x3 y3 )
        /// </summary>
        double hn { get; set; }
        /// <summary>
        /// длина грани КО ( x3 y3 -  x0 y0 )
        /// </summary>
        double hw { get; set; }
        /// <summary>
        /// Растояние от центра КЭ к центрц грани e
        /// </summary>
        double de { get; set; }
        /// <summary>
        /// Растояние от центра КЭ к центрц грани w
        /// </summary>
        double dw { get; set; }
        /// <summary>
        /// Растояние от центра КЭ к центрц грани n
        /// </summary>
        double dn { get; set; }
        /// <summary>
        /// Растояние от центра КЭ к центрц грани s
        /// </summary>
        double ds { get; set; }
        /// <summary>
        /// Площадь КО
        /// </summary>
        double S { get; set; }
        /// <summary>
        /// Установить параметры КО
        /// </summary>
        /// <param name="fv"></param>
        void Set(IFVElem fv);
        /// <summary>
        /// Взять координаты вершин КО
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        void GetVertex(ref HPoint p0, ref HPoint p1, ref HPoint p2, ref HPoint p3);
    }
}
