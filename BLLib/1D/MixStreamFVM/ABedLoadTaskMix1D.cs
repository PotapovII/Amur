//---------------------------------------------------------------------------
//                 Реализация библиотеки для моделирования 
//                  гидродинамических и русловых процессов
//---------------------------------------------------------------------------
//                Модуль BLLib для расчета донных деформаций 
//                 (учет движения только влекомых наносов)
//         по русловой модели Петрова А.Г. и Потапова И.И. от 2014 г.
//                              Потапов И.И.
//                               14.04.21
//---------------------------------------------------------------------------
namespace BLLib
{
    using AlgebraLib;
    using CommonLib;
    using CommonLib.Physics;
    using GeometryLib;
    using MemLogLib;
    using System;
    /// <summary>
    /// ОО: Абстрактный класс для 1D задач,
    /// реализует общий интерфейст задач по деформациям дна
    /// </summary>
    [Serializable]
    public abstract class ABedLoadTaskMix1D : BedMixModelParams, IBedLoadTask
    {
        #region  Свойства
        /// <summary>
        /// Название файла параметров задачи
        /// </summary>
        public string NameBLParams() => "NameMixBLParams.txt";
        /// <summary>
        /// текущее время расчета 
        /// </summary>
        public double time { get; set; }
        /// <summary>
        /// шаг по времени
        /// </summary>
        public double dtime { get; set; }
        /// <summary>
        /// наименование задачи
        /// </summary>
        public string Name { get => name; }
        protected string name;
        ///// <summary>
        ///// Модель влекомого движения донного матеиала
        ///// </summary>
        //[Browsable(false)]
        //public TypeBLModel blm { get => blm; }
        /// <summary>
        /// Тип задачи используется для выбора совместимых подзадач
        /// </summary>
        public TypeTask typeTask { get => tTask; }
        protected TypeTask tTask;
        /// <summary>
        /// версия дата последних изменений интерфейса задачи
        /// </summary>
        public string VersionData() => "ABedLoadTaskMix1D 01.09.2021";
        /// <summary>
        /// Сетка решателя
        /// </summary>
        public IMesh Mesh() => mesh;
        protected IMesh mesh;
        /// <summary>
        /// Искомые поля задачи которые позволяют сформировать на сетке расчетной области краевые условия задачи
        /// </summary>
        public IUnknown[] Unknowns() => unknowns;
        protected IUnknown[] unknowns = { new Unknown("Отметки дна", null, TypeFunForm.Form_1D_L1) };
        /// <summary>
        /// Алгебра задачи
        /// </summary>
        public IAlgebra Algebra() => algebra;
        [NonSerialized]
        protected IAlgebra algebra = null;

        /// <summary>
        /// Лавинное обрушение
        /// </summary>
        public IAvalanche GetAvalanche() => avalanche;
        /// <summary>
        /// Лавинное обрушение
        /// </summary>
        public IAvalanche avalanche;
        /// <summary>
        /// Текщий уровень дна
        /// </summary>
        public double[] CZeta { get => Zeta0; }
        #endregion 

        #region Краевые условия
        /// <summary>
        /// транзитный расход на входе
        /// </summary>
        public double Gtran_in = 0;
        /// <summary>
        /// транзитный расход на выходе
        /// </summary>
        public double Gtran_out = 0;
        #endregion

        #region Служебные переменные
        /// <summary>
        /// Количество расчетных узлов для дна
        /// </summary>
        protected int Count;
        /// <summary>
        /// Количество расчетных подобластей
        /// </summary>
        protected int N;
        /// <summary>
        /// Количество фракций дна
        /// </summary>
        public int CountFraction;
        /// <summary>
        /// текущая итерация по времени 
        /// </summary>
        protected int CountTime = 0;
        /// <summary>
        /// количество узлов по времени
        /// </summary>
        // protected int LengthTime = 200000;
        /// <summary>
        /// относительная точность при вычислении 
        /// изменения донной поверхности
        /// </summary>
        //protected double eZeta = MEM.Error6;
        /// <summary>
        /// Погрешность при вычислении коэффициентов
        /// </summary>
        //protected double ErrAE = MEM.Error9;
        /// <summary>
        /// расчетный период времени, сек 
        /// </summary>
        protected double T;
        /// <summary>
        /// расчетный шаг по времени
        /// </summary>
        protected double rdt;

        /// <summary>
        ///  косинус гамма - косинус угола между 
        ///  нормалью к дну и вертикальной осью
        /// </summary>
        protected double[] CosGamma = null;

        protected double[] G0 = null;
        protected double[] A = null;
        protected double[] B = null;
        protected double[] C = null;

        protected double[][] Gf = null;
        protected double[][] Af = null;
        protected double[][] Bf = null;
        public double[][] dZetaf = null;

        protected double[] CE = null;
        protected double[] CW = null;
        protected double[] CP = null;
        protected double[] S = null;

        protected double[] AE = null;
        protected double[] AW = null;
        protected double[] AP = null;
        protected double[] AP0 = null;

        protected double[] ps = null;

        protected double[] dVf = null;

        protected double[] Gs = null;

        protected double dz, dx, dp;
        protected double mtau, chi;
        /// <summary>
        /// Флаг для определения сухого-мокрого дна
        /// </summary>
        protected int[] DryWet = null;
        /// <summary>
        /// длина расчетной области
        /// </summary>
        protected double L;
        /// <summary>
        /// <summary>
        /// Флаг отладки
        /// </summary>
        public int debug = 0;
        /// <summary>
        /// Поле сообщений о состоянии задачи
        /// </summary>
        public string Message = "Ok";
        #endregion

        #region Рабочие массивы
        /// <summary>
        /// массив координаты узлов
        /// </summary>
        public double[] x = null;
        /// <summary>
        /// массив донных отметок
        /// </summary>
        public double[] Zeta = null;
        /// <summary>
        /// массив приращения донных отметок на текущем слое по времени
        /// </summary>
        public double[] dZeta = null;
        /// <summary>
        /// массив донных отметок на предыдущем слое по времени
        /// </summary>
        public double[] Zeta0 = null;
        /// <summary>
        /// массив придонных касательнывх напряжений
        /// </summary>
        public double[] tau = null;
        /// <summary>
        /// массив придонного давления
        /// </summary>
        public double[] P = null;
        /// <summary>
        /// узловые площади 
        /// </summary>
        protected double[] Area = null;
        #endregion
        /// <summary>
        /// Готовность задачи к расчету
        /// </summary>
        protected bool taskReady = false;
        /// <summary>
        /// Готовность задачи к расчету
        /// </summary>
        public bool TaskReady()
        {
            return taskReady;
        }
        /// <summary>
        /// Конструктор по умолчанию/тестовый
        /// </summary>
        public ABedLoadTaskMix1D(BedMixModelParams p) : base(p)
        {
            InitBedLoad();
            time = 0;
        }
        /// <summary>
        /// Установка текущей геометрии расчетной области
        /// </summary>
        /// <param name="mesh">Сетка расчетной области</param>
        /// <param name="Zeta0">начальный уровень дна</param>
        public virtual void SetTask(IMesh mesh, double[] Zeta0, IBoundaryConditions BConditions)
        {
            double tanphi = SPhysics.PHYS.tanphi;

            taskReady = false;
            this.mesh = mesh;
            this.x = mesh.GetCoords(0);
            this.Zeta0 = mesh.GetCoords(1);
            if (this.Zeta0 == null)
                this.Zeta0 = Zeta0;

            this.Count = x.Length;
            this.N = Count - 1;
            this.L = x[N] - x[0];
            this.isAvalanche = isAvalanche;

            MEM.Alloc<int>(Count, ref DryWet);

            MEM.Alloc<double>(Count, ref dZeta);
            MEM.Alloc<double>(Count, ref Zeta);
            MEM.Alloc<double>(Count, ref Gs);
            MEM.Alloc<double>(Count, ref ps);
            MEM.Alloc<double>(Count, ref AE);
            MEM.Alloc<double>(Count, ref AW);
            MEM.Alloc<double>(Count, ref AP);
            MEM.Alloc<double>(Count, ref AP0);
            MEM.Alloc<double>(Count, ref S);

            MEM.Alloc2D<double>(Count, CountMix, ref dZetaf);

            MEM.Alloc(Count, ref Area);
            Area[0] = 0.5 * (x[1] - x[0]);
            Area[N] = 0.5 * (x[N] - x[N - 1]);
            for (int n = 1; n < N; n++)
                Area[n] = 0.5 * (x[n + 1] - x[n - 1]);

            MEM.Alloc<double>(N, ref A);
            MEM.Alloc<double>(N, ref B);
            MEM.Alloc<double>(N, ref G0);
            MEM.Alloc<double>(N, ref CosGamma);

            MEM.Alloc2D<double>(N, CountMix, ref Af);
            MEM.Alloc2D<double>(N, CountMix, ref Bf);
            MEM.Alloc2D<double>(N, CountMix, ref Gf);

            MEM.Alloc<double>(CountMix, ref dVf);
            // инициализация массивов для расчета донных фракций
            InitParamsForMesh(Count, Count - 1);

            avalanche = new Avalanche1DX_Old(mesh, tanphi, 0.6);

            taskReady = true;
        }
        /// <summary>
        /// Вычисление изменений формы донной поверхности 
        /// на одном шаге по времени по модели 
        /// Петрова А.Г. и Потапова И.И. 2014
        /// Реализация решателя - методом контрольных объемов,
        /// Патанкар (неявный дискретный аналог ст 40 ф.3.40 - 3.41)
        /// Коэффициенты донной подвижности, определяются 
        /// как среднее гармонические величины         
        /// </summary>
        /// <param name="Zeta">>возвращаемая форма дна на n+1 итерации</param>
        /// <param name="tau">придонное касательное напряжение</param>
        /// <param name="P">придонное давление</param>
        /// <param name="GloverFlory">флаг расчета критической глуьины по Гловеру и Флори</param>
        public abstract void CalkZetaFDM(ref double[] Zeta, double[] tauX, double[] tauY = null, double[] P = null, double[][] CS = null);
        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public abstract IBedLoadTask Clone();

        ///  /// <summary>
        /// Вычисление текущих расходов и их градиентов для построения графиков
        /// </summary>
        /// <param name="Gs">возвращаемые расходы (по механизмам движения)</param>
        /// <param name="dGs">возвращаемые градиенты расходы (по механизмам движения)</param>
        /// <param name="tau">придонное касательное напряжение</param>
        /// <param name="P">придонное давление</param>
        public void Calk_Gs(ref double[][] Gs, ref double[][] dGs, double[] tau = null, double[] P = null)
        {
            
            double tanphi = SPhysics.PHYS.tanphi;
            double G1 = SPhysics.PHYS.G1;
            double tau0 = SPhysics.PHYS.tau0;
            double rho_s = SPhysics.PHYS.rho_s;
            

            if (tau == null && this.tau == null)
            {
                Gs = null; dGs = null; return;
            }
            if (tau == null)
                tau = this.tau;

            if (Gs == null)
            {
                Gs = new double[4][]; // idxAll, idxTransit, zeta, press 
                dGs = new double[4][];
                for (int i = 0; i < Gs.Length; i++)
                {
                    Gs[i] = new double[tau.Length];
                    dGs[i] = new double[tau.Length];
                }
            }
            // Расчет деформаций дна от влекомых наносов
            // Давление в узлах Zeta,  Zeta0
            // Расчет коэффициентов  на грани  P--e--E
            for (int i = 0; i < N; i++)
            {
                mtau = Math.Abs(tau[i]);
                chi = Math.Sqrt(tau0 / mtau);
                dx = x[i + 1] - x[i];
                dz = (Zeta0[i + 1] - Zeta0[i]) / dx;
                // косинус гамма
                CosGamma[i] = Math.Sqrt(1 / (1 + dz * dz));
                double A = Math.Max(0, 1 - chi);
                double B = (chi / 2 + A) / tanphi;
                // Расход массовый! только для отрисовки !!! 
                // для расчетов - объемный
                double G0 = rho_s * G1 * tau[i] * Math.Sqrt(mtau) / CosGamma[i];
                Gs[idxTransit][i] = G0 * A;
                Gs[idxZeta][i] = -G0 * B * dz;
                Gs[idxAll][i] = Gs[idxTransit][i] + Gs[idxZeta][i];
            }
            for (int i = 0; i < N - 1; i++)
            {
                dx = x[i + 1] - x[i];
                dGs[idxTransit][i] = (dGs[idxTransit][i + 1] - dGs[idxTransit][i]) / dx;
                dGs[idxZeta][i] = (dGs[idxZeta][i + 1] - dGs[idxZeta][i]) / dx;
                dGs[idxAll][i] = (dGs[idxAll][i + 1] - dGs[idxAll][i]) / dx;
            }
        }

        /// <summary>
        /// Формирование данных для отрисовки связанных с сеткой IMesh со сороны задачи
        /// </summary>
        /// <param name="sp">контейнер данных</param>
        public void AddMeshPolesForGraphics(ISavePoint sp)
        {
            if (sp != null)
            {
                double[][] Gs = null;
                double[][] dGs = null;
                Calk_Gs(ref Gs, ref dGs);
                if (Gs != null)
                {
                    double[] X = new double[x.Length - 1];
                    for (int i = 0; i < X.Length; i++)
                        X[i] = 0.5 * (x[i + 1] + x[i]);

                    
                    sp.AddCurve("Отметки дна", X, Zeta, TypeGraphicsCurve.FuncionCurve);

                    sp.AddCurve("Транзитный расход наносов", X, Gs[idxTransit], TypeGraphicsCurve.FuncionCurve);
                    sp.AddCurve("Гравитационный расход наносов", X, Gs[idxZeta], TypeGraphicsCurve.FuncionCurve);
                    sp.AddCurve("Полный расход наносов", X, Gs[idxAll], TypeGraphicsCurve.FuncionCurve);

                    sp.AddCurve("Градиент транзитного расхода наносов", X, dGs[idxTransit], TypeGraphicsCurve.FuncionCurve);
                    sp.AddCurve("Градиент гравитационного расхода наносов", X, dGs[idxZeta], TypeGraphicsCurve.FuncionCurve);
                    sp.AddCurve("Градиент полного расхода наносов", X, dGs[idxAll], TypeGraphicsCurve.FuncionCurve);
                }

                // -------------- поля
                //for (int ff = 0; ff < CountMix; ff++)
                //{
                //    double[] data = new double[x.Length];
                //    for (int i = 0; i < data.Length; i++)
                //        data[i] = FractionSurface[i][ff];
                //    sp.Add("Доля фракции " + ff.ToString() + " в активном слое", data);
                //}
                //for (int ff = 0; ff < CountMix; ff++)
                //{
                //    double[] data = new double[x.Length];
                //    for (int i = 0; i < data.Length; i++)
                //        data[i] = FractionBedLoad[i][ff];
                //    sp.Add("Доля фракции " + ff.ToString() + " в несущем слое", data);
                //}
                //for (int ff = 0; ff < CountMix; ff++)
                //{
                //    double[] data = new double[x.Length];
                //    for (int i = 0; i < data.Length; i++)
                //        data[i] = FractionSubSurface[i][ff];
                //    sp.Add("Доля фракции " + ff.ToString() + " в пасивном слое", data);
                //}
                // ------------ Кривые
                for (int ff = 0; ff < CountMix; ff++)
                {
                    double[] data = new double[x.Length];
                    for (int i = 0; i < data.Length; i++)
                        data[i] = FractionSurface[i][ff];
                    sp.AddCurve("Доля фракции " + ff.ToString() + " в активном слое", x, data);
                }
                for (int ff = 0; ff < CountMix; ff++)
                {
                    double[] data = new double[x.Length];
                    for (int i = 0; i < data.Length; i++)
                        data[i] = FractionBedLoad[i][ff];
                    sp.AddCurve("Доля фракции " + ff.ToString() + " в несущем слое", x, data);
                }
                //for (int ff = 0; ff < CountMix; ff++)
                //{
                //    double[] data = new double[x.Length];
                //    for (int i = 0; i < data.Length; i++)
                //        data[i] = FractionSubSurface[i][ff];
                //    sp.Add("Доля фракции " + ff.ToString() + " в пасивном слое", x , data);
                //}

                //for (int ff = 0; ff < CountMix; ff++)
                //    sp.Add("Процент фракции " + ff.ToString() + " в активном слое", PercentFinerSurface[ff]);

                //for (int ff = 0; ff < CountMix; ff++)
                //    sp.Add("Процент фракции " + ff.ToString() + " в активном слое", PercentFinerBedLoad[ff]);

                //for (int ff = 0; ff < CountMix; ff++)
                //    sp.Add("Процент фракции " + ff.ToString() + " в пасивном слое", PercentFinerSubSurface[ff]);
            }
        }

        /// <summary>
        /// Аппроксимация фракций с узлов КЭ сетки в элементы
        /// </summary>
        /// <param name="knot"></param>
        /// <param name="elem"></param>
        public void KnotToElement(double[][] knot, ref double[][] elem)
        {
            for (int i = 0; i < elem.Length; i++)
                for (int fr = 0; fr < CountMix; fr++)
                    elem[i][fr] = 0.5 * (knot[i + 1][fr] + knot[i][fr]);
        }
        /// <summary>
        /// Аппроксимация фракций с элементов в узлы КЭ сетки
        /// </summary>
        /// <param name="elem"></param>
        /// <param name="knot"></param>
        public void ElementToKnot(double[][] elem, ref double[][] knot)
        {
            for (int fr = 0; fr < CountMix; fr++)
            {
                knot[0][fr] = elem[0][fr];
                knot[elem.Length][fr] = elem[elem.Length - 1][fr];
            }
            for (int i = 1; i < knot.Length - 1; i++)
            {
                double dxe = x[i + 1] - x[i];
                double dxw = x[i] - x[i - 1];
                for (int fr = 0; fr < CountMix; fr++)
                    knot[i][fr] = (elem[i][fr] * dxe + elem[i - 1][fr] * dxw) / (dxw + dxe);
            }
        }
        #region Методы отладки
        /// <summary>
        /// Тестовая печать поля
        /// </summary>
        /// <param name="Name">имя поля</param>
        /// <param name="mas">массив пля</param>
        /// <param name="FP">точность печати</param>
        public void PrintMas(string Name, double[] mas, int FP = 8)
        {
            string Format = " {0:F6}";
            if (FP != 6)
                Format = " {0:F" + FP.ToString() + "}";

            Console.WriteLine(Name);
            for (int i = 0; i < mas.Length; i++)
            {
                Console.Write(Format, mas[i]);
            }
            Console.WriteLine();
        }
        /// <summary>
        /// Тестовая печать поля
        /// </summary>
        /// <param name="Name">имя поля</param>
        /// <param name="mas">массив пля</param>
        /// <param name="FP">точность печати</param>
        public void PrintMatrix(int FP = 8)
        {
            string Format = " {0:F6}";
            if (FP != 6)
                Format = " {0:F" + FP.ToString() + "}";

            for (int i = 0; i < AP.Length; i++)
            {
                for (int j = 0; j < AP.Length; j++)
                {
                    double a = 0;
                    if (i == j + 1)
                        a = AW[i];
                    if (i == j)
                        a = AP[i];
                    if (i == j - 1)
                        a = AE[i];
                    Console.Write(Format, a);
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }
        public void PrintKnotFr(string Name, double[][] mas, int FP = 8)
        {
            Console.WriteLine(Name);
            string Format = " {0:F6}";
            if (FP != 6)
                Format = " {0:F" + FP.ToString() + "}";

            for (int fr = 0; fr < mas[0].Length; fr++)
            {
                Console.WriteLine("Фракция: " + fr.ToString() +
                    " диаметр " + (1000 * SandDiam[fr]).ToString() + " мм");
                for (int n = 0; n < mas.Length; n++)
                {
                    Console.Write(Format, mas[n][fr]);
                }
                Console.WriteLine();
            }
        }

        #endregion
    }
}
