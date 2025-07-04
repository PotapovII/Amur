﻿namespace CommonLib.Geometry
{
    /// <summary>
    /// Ребро
    /// </summary>
    public interface IHEdge : IHLine
    {
        /// <summary>
        /// Индекс ребра
        /// </summary>
        int ID { get; set; }
    }
    /// <summary>
    /// Ребро
    /// </summary>
    public interface IHillEdge : IHEdge
    {
        /// <summary>
        /// Маркер ребра
        /// </summary>
        int mark { get; set; }
        /// <summary>
        /// количество узлов на ребре
        /// </summary>
        int Count { get; set; }
        /// <summary>
        /// Получить массив точек границ
        /// </summary>
        /// <param name="inCount"></param>
        /// <returns></returns>
        IHPoint[] GetPoints(int inCount = 5);
        /// <summary>
        /// Принадлежность линии
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        bool Contains(IHPoint p);
    }
}
