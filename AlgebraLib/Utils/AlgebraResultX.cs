//---------------------------------------------------------------------------
//   Реализация библиотеки методов решения систем алгебраических уравнений
//                     - (C) Copyright 2021 -
//                       ALL RIGHT RESERVED
//                   разработка: Потапов И.И.
//                          31.07.2021 
//---------------------------------------------------------------------------
namespace AlgebraLib
{
    using MemLogLib;

    public class AlgebraResultDirect : AlgebraResult
    {
        /// <summary>
        /// Ширина ленты
        /// </summary>
        public double FH { get { return (FHL + FHR + 1); } }
        /// <summary>
        /// Шириной нижней полуленты
        /// </summary>
        public int FHL;
        /// <summary>
        /// Шириной верхней полуленты 
        /// </summary>
        public int FHR;
        public AlgebraResultDirect(int FHL = 0, int FHR = 0) : base()
        {
            this.FHL = FHL;
            this.FHR = FHR;
        }
        public AlgebraResultDirect(AlgebraResultDirect a) : base(a)
        {
            FHL = a.FHL;
            FHR = a.FHR;
        }
        public AlgebraResultDirect(AlgebraResult a) : base(a)
        {
            FHL = 0;
            FHR = 0;
        }
        public override void Print()
        {
            base.Print();
            LOG.Print("Ширина ленты", FH);
            LOG.Print("ШШириной нижней полуленты", FHL);
            LOG.Print("Шириной верхней полуленты", FHR);
        }
    }
    public class AlgebraResultIterative : AlgebraResult
    {
        /// <summary>
        /// Количество итераций до сходимости
        /// </summary>
        public int Iterations;
        /// <summary>
        /// Норма ошибки sqrt( r * r )
        /// </summary>
        public double Error_L2;
        /// <summary>
        /// Норма ошибки max(| r |)
        /// </summary>
        public double Error_C;
        /// <summary>
        /// Норма ошибки sqrt(r0* r0)
        /// </summary>
        public double Error0_L2;
        /// <summary>
        /// Относительная ошибка 
        /// </summary>
        public double ratioError;
        /// <summary>
        /// Нома матрицы max | A | или max | X |
        /// </summary>
        public double normA;
        /// <summary>
        /// Флаг работы с матрицами предобуславливания
        /// </summary>
        public bool isPrecond;
        public AlgebraResultIterative() : base()
        {
            Iterations = 0;
            Error_L2 = 0;
            Error_C = 0;
            Error0_L2 = 0;
            ratioError = 0;
            normA = 1;
            isPrecond = false;
        }
        public AlgebraResultIterative(AlgebraResult a) : base(a)
        {
            Iterations = 0;
            Error_L2 = 0;
            Error_C = 0;
            Error0_L2 = 0;
            ratioError = 0;
            normA = 0;
            isPrecond = false;
        }
        public AlgebraResultIterative(AlgebraResultIterative a) : base(a)
        {
            Iterations = a.Iterations;
            Error_L2 = a.Error_L2;
            Error_C = a.Error_C;
            Error0_L2 = a.Error0_L2;
            ratioError = a.ratioError;
            normA = a.normA;
            isPrecond = a.isPrecond;
        }
        public override void Print()
        {
            base.Print();
            LOG.TPrint<double>("Количество итераций до сходимости", Iterations);
            LOG.TPrint<double>("Норма ошибки max(| r |)", Error_C);
            LOG.TPrint<double>("Норма ошибки sqrt( r * r )", Error_L2);
            LOG.TPrint<double>("Норма начальной ошибки sqrt( r0 * r0 )", Error0_L2);
            LOG.TPrint<double>("Относительная ошибка", ratioError);
            LOG.TPrint<double>("Нома матрицы max | A |", normA);
            LOG.TPrint<bool>("Предобуславливание", isPrecond);
        }
    }

}
