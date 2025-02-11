//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 14.01.2025 Потапов И.И.
//---------------------------------------------------------------------------

namespace EddyViscosityLib
{
    using CommonLib;
    using CommonLib.Mesh;

    using System;
    using MemLogLib;
    using CommonLib.EddyViscosity;

    /// <summary>
    /// Базовый класс для диф. моделей вихревой вязкости
    /// </summary>
    [Serializable]
    public abstract class ADiffEddyViscosityTri
        : AbsEddyViscosityTri<BEddyViscosityParam>, IEddyViscosityTri
    {
        /// <summary>
        /// Параметр SUPG стабилизации 
        /// </summary>
        protected double omega = 0.5;
        /// <summary>
        /// Осевая скорость
        /// </summary>
        protected double[] Ux;
        /// <summary>
        /// Осевая скорость
        /// </summary>
        protected double[] Vy;
        /// <summary>
        /// Осевая скорость
        /// </summary>
        protected double[] Vz;
        /// <summary>
        /// Функция тока в створе
        /// </summary>
        protected double[] Phi;
        /// <summary>
        /// Функция вихря в створе
        /// </summary>
        protected double[] Vortex;
        /// <summary>
        /// Итераций по нелинейности
        /// </summary>
        protected int NLine;
        /// <summary>
        /// Релаксация при шаге по нелинейности
        /// </summary>
        protected double relax = 0.3;
        /// <summary>
        /// Правая часть
        /// </summary>
        protected double[] MRight = null;
        /// <summary>
        /// Перегруженный метод
        /// </summary>
        /// <returns></returns>
        public new IMesh Mesh() => mesh;
        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public ADiffEddyViscosityTri(ETurbViscType eTurbViscType, int nLine = 100) 
            : this(eTurbViscType, new BEddyViscosityParam(), nLine) { }

        public ADiffEddyViscosityTri(ETurbViscType eTurbViscType, BEddyViscosityParam p, int nLine=100)
            : base(eTurbViscType, p, TypeTask.streamY1D)
        {
            NLine = nLine;
        }

        public override void SetTask(IMesh mesh, IAlgebra algebra, IMeshWrapper wMesh = null)
        {
            base.SetTask(mesh, algebra, wMesh);
            MEM.Alloc(mesh.CountKnots, ref eddyViscosity);
        }
    }
}
