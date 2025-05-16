//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2021 -
//                       ALL RIGHT RESERVED
//                        ПРОЕКТ "BedLoadLib"
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                          12.04.21
//---------------------------------------------------------------------------
namespace BedLoadLib
{
    using CommonLib;
    using MemLogLib;
    using System;
    /// <summary>
    /// ОО: Абстрактный класс для 2D задач,
    /// реализует общий интерфейст задач
    /// </summary>
    [Serializable]
    public abstract class ABedLoadTask_2D<TParam> : ABedLoad<TParam> where TParam : class, ITProperty<TParam>
    {
        /// <summary>
        /// Текщий уровень дна
        /// </summary>
        public double[] CZeta { get => Zeta0; }
        /// <summary>
        /// Флаг отладки
        /// </summary>
        public int debug = 0;
        /// <summary>
        /// относительная точность при вычислении 
        /// изменения донной поверхности
        /// </summary>
        protected double eZeta = 0.000001;
        /// <summary>
        /// Конструктор по умолчанию/тестовый
        /// </summary>
        public ABedLoadTask_2D(TParam p) : base(p, TypeTask.streamXY2D) { }
        /// <summary>
        /// Установка текущей геометрии расчетной области
        /// </summary>
        /// <param name="mesh">Сетка расчетной области</param>
        /// <param name="Zeta0">начальный уровень дна</param>
        public override void SetTask(IMesh mesh, double[] Zeta0, double[] Roughness, IBoundaryConditions BConditions)
        {
            base.SetTask(mesh, Zeta0, Roughness, BConditions);
            MEM.Alloc<double>(mesh.CountKnots, ref Zeta);
            eTaskReady = ETaskStatus.CreateMesh;
        }
    }
}
