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
//                   рефакторинг : Потапов И.И.
//                          27.04.25
//---------------------------------------------------------------------------
namespace BedLoadLib
{
    using AlgebraLib;
    using CommonLib;
    using CommonLib.BedLoad;
    using CommonLib.Physics;
    using MemLogLib;
    using System;
    using System.Linq;

    /// <summary>
    /// ОО: Абстрактный класс для 2D задач,
    /// реализует общий интерфейст задач
    /// </summary>
    [Serializable]
    public abstract class ABedLoadFEM_1XD : ABedLoad<BedLoadParams1D>, IBedLoadTask
    {
        /// <summary>
        /// Параметр схемы по времени
        /// </summary>
        public double theta = 0.5;
        /// <summary>
        /// Для правой части
        /// </summary>
        [NonSerialized]
        protected IAlgebra Ralgebra = null;

        #region Служебные переменные
        /// <summary>
        ///  косинус гамма - косинус угола между 
        ///  нормалью к дну и вертикальной осью
        /// </summary>
        protected double[] CosGamma = null;
        protected double[] GammaX = null;
        protected double[] GammaY = null;
        //protected double[] G0 = null;
        protected double[] A = null;
        protected double[] B = null;
        protected double[] C = null;
        protected double[] D = null;
        protected double[] ps = null;
        protected int cu;
        protected double[] a;
        protected double[] b;
        protected double dz;
        #endregion

        #region Переменные для работы с КЭ аналогом
        /// <summary>
        /// Узлы КЭ
        /// </summary>
        protected uint[] knots = null;
        /// <summary>
        /// координаты узлов КЭ
        /// </summary>
        protected double[] x = null;
        protected double[] y = null;
        /// <summary>
        /// локальная матрица часть СЛАУ
        /// </summary>
        protected double[][] LaplMatrix = null;
        /// <summary>
        /// локальная матрица часть СЛАУ
        /// </summary>
        protected double[][] RMatrix = null;
        /// <summary>
        /// локальная правая часть СЛАУ
        /// </summary>
        protected double[] LocalRight = null;
        /// <summary>
        /// адреса ГУ
        /// </summary>
        protected uint[] adressBound = null;
        /// <summary>
        /// Вспомогательная правая часть
        /// </summary>
        protected double[] MRight = null;
        /// <summary>
        ///  косинус гамма - косинус угола между 
        ///  нормалью к дну и вертикальной осью
        /// </summary>
        protected double[] TanGamma = null;
        #endregion

        /// <summary>
        /// Конструктор по умолчанию/тестовый
        /// </summary>
        public ABedLoadFEM_1XD(BedLoadParams1D p) : base(p, TypeTask.streamX1D)
        {
            time = 0;
        }
        /// <summary>
        /// создание/очистка ЛМЖ и ЛПЧ ...
        /// </summary>
        /// <param name="cu">количество неизвестных</param>
        public virtual void InitLocal(int cu, int cs = 1)
        {
            MEM.Alloc<double>(cu, ref x);
            MEM.Alloc<double>(cu, ref y);
            MEM.Alloc<uint>(cu, ref knots);
            // с учетом степеней свободы
            int Count = cu * cs;
            MEM.AllocClear(Count, ref LocalRight);
            MEM.Alloc2DClear(Count, ref LaplMatrix);
            MEM.Alloc2DClear(Count, ref RMatrix);
        }
        protected void BaseSetTask(IMesh mesh, double[] Zeta0, double[] Roughness, IBoundaryConditions BConditions)
        {
            base.SetTask(mesh, Zeta0, Roughness, BConditions);
        }
        /// <summary>
        /// Установка текущей геометрии расчетной области
        /// </summary>
        /// <param name="mesh">Сетка расчетной области</param>
        /// <param name="Zeta0">начальный уровень дна</param>
        public override void SetTask(IMesh mesh, double[] Zeta0, double[] Roughness, IBoundaryConditions BConditions)
        {
            double tanphi = SPhysics.PHYS.tanphi;
            
            base.SetTask(mesh, Zeta0, Roughness, BConditions);

            algebra = new SparseAlgebraCG((uint)mesh.CountKnots);
            Ralgebra = algebra.Clone();

            int Count = mesh.CountElements;
            MEM.Alloc<double>(Count, ref CosGamma);
            MEM.Alloc<double>(Count, ref GammaX);
            MEM.Alloc<double>(Count, ref GammaY);
            MEM.Alloc<double>(Count, ref TanGamma);
            MEM.Alloc<double>(Count, ref A);
            MEM.Alloc<double>(Count, ref B);
            MEM.Alloc<double>(Count, ref C);
            MEM.Alloc<double>(Count, ref D);
            MEM.Alloc<double>(Count, ref tau);
            MEM.Alloc<double>(Count, ref ps);

            //avalanche = new Avalanche2DX(mesh, tanphi, DirectAvalanche.AvalancheXY, 0.3);
            avalanche = new Avalanche1DX_Old(mesh, tanphi, 0.6);
            eTaskReady = ETaskStatus.CreateMesh;
        }
        /// <summary>
        /// Установка алгебы для задачи
        /// </summary>
        /// <param name="_algebra"></param>
        public virtual void CreateAlgebra(IAlgebra _algebra = null)
        {
            if (this.algebra == null)
            {
                if (_algebra != null)
                    this.algebra = _algebra;
                else
                    this.algebra = new AlgebraGauss((uint)mesh.CountKnots);
                this.Ralgebra = this.algebra.Clone();
            }
            else
            {
                algebra.Clear();
                Ralgebra.Clear();
            }
        }

        /// <summary>
        /// Расчет коэффициентов A B C
        /// </summary>
        public abstract void CalkCoeff(uint elem, double mtauS, double dZx, double dZy);
        /// <summary>
        /// Интеграл по области x,y для донной поверхности Z
        /// используется для контроля потери массы на каждой итерации
        /// </summary>
        /// <param name="Z">поверхность</param>
        /// <returns></returns>
        public (double int_Z, double int_Z2, double Area) IntZeta(double[] Z)
        {
            double sumZ = 0;
            double sumZL2 = 0;
            double sumS = 0;
            double S;
            double[] ezeta = { 0, 0, 0 };
            for (uint elem = 0; elem < mesh.CountElements; elem++)
            {
                // площадь
                S = mesh.ElemSquare(elem);
                sumS += S;
                // выборка отметок дня для текущего КЭ
                mesh.ElemValues(Z, elem, ref ezeta);
                double midleZ = ezeta.Sum();
                sumZ += midleZ * S / 3;
                sumZL2 += Math.Abs(midleZ) * Math.Abs(S) / 3;
            }
            return (sumZ / Math.Abs(sumS), sumZL2 / Math.Abs(sumS), Math.Abs(sumS));
        }
        /// <summary>
        /// Вычисление текущих расходов и их градиентов для построения графиков
        /// </summary>
        /// <param name="Gs">возвращаемые расходы (по механизмам движения)</param>
        /// <param name="dGs">возвращаемые градиенты расходы (по механизмам движения)</param>
        /// <param name="tau">придонное касательное напряжение</param>
        /// <param name="P">придонное давление</param>
        public override void Calk_Gs(ref double[][] Gs, ref double[][] dGs, double[] tau, double[] P = null)
        {

        }
    }
}
