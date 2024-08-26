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
    using System.Linq;
    using System.ComponentModel;
    /// <summary>
    /// OO: Класс для задания типа граничных условий
    /// </summary>
    [Category("Граничные условия")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Serializable]

    public class BoundaryConditionsVar : IBoundaryConditions
    {
        /// <summary>
        /// количество маркеров границы в сетке
        /// </summary>
        public int CountMark { get { return ValueDir != null ? ValueDir.Length : 0; } }
        /// <summary>
        /// Значение функции на границе (по умолчанию однородные)
        /// </summary>
        public double[] ValueDir;
        /// <summary>
        /// Значение шероховатости на границе (по умолчанию однородные)
        /// </summary>
        public double[] ks;
        /// <summary>
        /// Значение потоков на границе (по умолчанию однородные)
        /// </summary>
        public double[] ValueNeu;

        /// <summary>
        /// Значение функции на границе (по умолчанию однородные)
        /// </summary>
        public double[] GetValueDir(int index = 0) => index == 0 ? ValueDir : ks;
        /// <summary>
        /// Значение потоков на границе (по умолчанию однородные)
        /// </summary>
        public double[] GetValueNeu() => ValueNeu;

        public BoundaryConditionsVar(int Count = 1)
        {
            MEM.Alloc(Count, ref ValueDir);
            MEM.Alloc(Count, ref ValueNeu);
            MEM.Alloc(Count, ref ks);
        }
        public BoundaryConditionsVar(IMesh mesh)
        {
            int[] uniquevalues = mesh.GetBElementsBCMark().Distinct().ToArray();
            MEM.Alloc(uniquevalues.Length, ref ValueDir);
            MEM.Alloc(uniquevalues.Length, ref ValueNeu);
            MEM.Alloc(uniquevalues.Length, ref ks);
        }
        public BoundaryConditionsVar(double[] valuesDir, double[] valuesNeu, double[] mks = null)
        {
            SetValue(valuesDir, valuesNeu, mks);
        }
        public BoundaryConditionsVar(BoundCondition1D[] p)
        {
            MEM.Alloc(p.Length, ref ValueDir);
            MEM.Alloc(p.Length, ref ValueNeu);
            MEM.Alloc(p.Length, ref ks);
            for (int i = 0; i < p.Length; i++)
            {
                ValueDir[i] = 0;
                ValueNeu[i] = 0;
                if (p[i].typeBC == TypeBoundCond.Dirichlet)
                    ValueDir[i] = p[i].valueBC;
                if (p[i].typeBC == TypeBoundCond.Neumann)
                    ValueNeu[i] = p[i].valueBC;
                ks[i] = 0;
            }
        }
        public BoundaryConditionsVar(BoundaryConditionsVar p)
        {
            MEM.MemCopy(ref ValueDir, p.ValueDir);
            MEM.MemCopy(ref ValueNeu, p.ValueNeu);
            MEM.MemCopy(ref ks, p.ks);
        }
        /// <summary>
        /// Установить значения граничных условий
        /// </summary>
        /// <param name="valuesDir"></param>
        /// <param name="valuesNeu"></param>
        public void SetValue(double[] valuesDir, double[] valuesNeu, double[] mks = null)
        {
            MEM.MemCopy(ref ValueDir, valuesDir);
            MEM.MemCopy(ref ValueNeu, valuesNeu);
            if (mks != null)
                MEM.MemCopy(ref ks, mks);
        }
    }
}
