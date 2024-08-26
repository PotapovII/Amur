//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2021 -
//                       ALL RIGHT RESERVED
//                        ПРОЕКТ "BLLib"
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                          12.04.21
//---------------------------------------------------------------------------
namespace BLLib
{
    using AlgebraLib;
    using CommonLib;
    using MemLogLib;
    using System;

    /// <summary>
    /// ОО: Абстрактный класс для 2D задач,
    /// реализует общий интерфейст задач
    /// </summary>
    [Serializable]
    public abstract class ABedLoadTask2D : ABedLoadTask, IBedLoadTask
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
        /// Поле сообщений о состоянии задачи
        /// </summary>
        public string Message = "Ok";
        
        /// <summary>
        /// относительная точность при вычислении 
        /// изменения донной поверхности
        /// </summary>
        protected double eZeta = 0.000001;
        /// <summary>
        /// Конструктор по умолчанию/тестовый
        /// </summary>
        public ABedLoadTask2D(BedLoadParams p) : base(p)
        {
            InitBedLoad();
            time = 0;
        }
        public void ReInitBedLoad(BedLoadParams p)
        {
            SetParams(p);
            InitBedLoad();
            time = 0;
        }
        /// <summary>
        /// Установка текущей геометрии расчетной области
        /// </summary>
        /// <param name="mesh">Сетка расчетной области</param>
        /// <param name="Zeta0">начальный уровень дна</param>
        public override void SetTask(IMesh mesh, double[] Zeta0, IBoundaryConditions BConditions)
        {
            taskReady = false;
            base.SetTask(mesh, Zeta0, BConditions);
            MEM.Alloc<double>(mesh.CountKnots, ref Zeta);
            taskReady = true;
        }
    }
}
