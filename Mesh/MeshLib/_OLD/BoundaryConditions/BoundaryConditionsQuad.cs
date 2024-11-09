//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2022 -
//                       ALL RIGHT RESERVED
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                          22.07.22
//---------------------------------------------------------------------------
namespace MeshLib
{
    using System;
    using CommonLib;
    using MemLogLib;
    using System.ComponentModel;
    /// <summary>
    /// OO: Класс для задания типа граничных условий
    /// </summary>
    [Category("Граничные условия")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Serializable]
    public class BoundaryConditionsQuad : IBoundaryConditions
    {
        const int countMark = 4;
        /// <summary>
        /// количество маркеров границы в сетке
        /// </summary>
        public int CountMark => countMark;
        /// <summary>
        /// Значение функции на границе (по умолчанию однородные)
        /// </summary>
        public double[] ValueDir = new double[countMark];
        /// <summary>
        /// Значение потоков на границе (по умолчанию однородные)
        /// </summary>
        public double[] ValueNeu = new double[countMark];
        /// <summary>
        /// Значение шероховатости на границе (по умолчанию однородные)
        /// </summary>
        public double[] ks;
        /// <summary>
        /// Значение функции на границе (по умолчанию однородные)
        /// </summary>
        public double[] GetValueDir(int index = 0) => index == 0 ? ValueDir : ks;
        /// <summary>
        /// <summary>
        /// Значение потоков на границе (по умолчанию однородные)
        /// </summary>
        public double[] GetValueNeu() => ValueNeu;

        public BoundaryConditionsQuad()
        {
            double[] Dir = { 0, 0, 0, 0 };
            double[] Neu = { 0, 0, 0, 0 };
            double[] mks = { 0, 0, 0, 0 };
            SetValue(Dir, Neu, mks);
        }
        public BoundaryConditionsQuad(double[] valuesDir, double[] valuesNeu, double[] mks = null)
        {
            SetValue(valuesDir, valuesNeu, mks);
        }
        public BoundaryConditionsQuad(BoundaryConditionsVar p)
        {
            SetValue(p.ValueDir, p.ValueNeu, p.ks);
        }
        /// <summary>
        /// Установить значения граничных условий
        /// </summary>
        /// <param name = "valuesDir" ></ param >
        /// <param name="valuesNeu"></param>
        public void SetValue(double[] valuesDir, double[] valuesNeu, double[] mks = null)
        {
            MEM.MemCopy(ref ValueDir, valuesDir);
            MEM.MemCopy(ref ValueNeu, valuesNeu);
        }
    }
}
