//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                      Прототип River2DM C++
//---------------------------------------------------------------------------
//             кодировка : 22.06.2020 - 13.07.21 Потапов И.И.
//---------------------------------------------------------------------------
namespace RiverLib.River2D
{
    using System;

    using MeshLib;
    using MemLogLib;
    using CommonLib;
    using CommonLib.IO;
    using CommonLib.ChannelProcess;
    using RiverLib.IO;
    using RiverLib.River2D.Algebra;
    using RiverLib.River2D.RiverMesh;
    using GeometryLib;

    /// <summary>
    /// Решение плановой задачи мелкой воды методом конечных элементов
    /// </summary>
    [Serializable]
    public class River2D : ParamsRiver, IRiver
    {
        /// <summary>
        /// Готовность задачи к расчету
        /// </summary>
        public bool TaskReady() => true;

        protected GaussPoints gaussPoints = new GaussPoints();
        /// <summary>
        /// Флаг определяющий необходимость расчета взвешенных наносов
        /// </summary>
        public EBedErosion GetBedErosion() => Erosion;
        public EBedErosion Erosion;

        /// <summary>
        /// Получить граничные условия для задачи донных деформаций
        /// </summary>
        /// <returns></returns>
        public virtual IBoundaryConditions BoundCondition() => null;


        /// <summary>
        /// Имена файлов с данными для задачи гидродинамики
        /// </summary>
        public ITaskFileNames taskFileNemes()
        {
            //ITaskFileNames fn = new TaskFileNames();
            ITaskFileNames fn = new TaskFileNames(GetFormater());
            fn.NameCPParams = "NameCPParams.txt";
            fn.NameBLParams = "NameBLParams.txt";
            fn.NameRSParams = "R2DNameRSParams.txt";
            fn.NameRData = "R2DNameRData.txt";
            //fn.NameEXT =       "(*.tsk)|*.tsk|";
            //fn.NameEXTImport = "(*.cdg)|*.cdg|, (*.mrf)|*.mrf|";
            return fn;
        }
        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public IRiver Clone()
        {
            return new River2D();
        }
        /// <summary>
        /// Наименование задачи
        /// </summary>
        public string Name { get => "плановая задача мелкой воды River2D"; }
        /// <summary>
        /// Тип задачи используется для выбора совместимых подзадач
        /// </summary>
        public TypeTask typeTask { get => TypeTask.streamXY2D; }

        /// <summary>
        /// версия дата последних изменений интерфейса задачи
        /// </summary>
        public string VersionData() => "River2D 01.09.2021"; 
        /// <summary>
        /// Сетка для расчета донных деформаций
        /// </summary>
        public IMesh BedMesh() => mesh;

        /// <summary>
        /// КЭ сетка
        /// </summary>
        public IMesh Mesh() => mesh;

        /// <summary>
        /// Искомые поля задачи которые позволяют сформировать на сетке расчетной области краевые условия задачи
        /// </summary>
        public IUnknown[] Unknowns() => unknowns;
        protected IUnknown[] unknowns = { new Unknown("Удельный расход потока по х", null, TypeFunForm.Form_2D_Triangle_L1),
                                          new Unknown("Удельный расход потока по у", null, TypeFunForm.Form_2D_Triangle_L1),
                                          new Unknown("Осредненная по глубине скорость х",null, TypeFunForm.Form_2D_Triangle_L1),
                                          new Unknown("Осредненная по глубине скорость у", null, TypeFunForm.Form_2D_Triangle_L1),
                                          new Unknown("Глубина потока", null, TypeFunForm.Form_2D_Triangle_L1) };
        #region Исходные данные
        /// <summary>
        /// КЭ сетка задачи
        /// </summary>
        public TriRiverMesh mesh = new TriRiverMesh();
        /// <summary>
        /// квадратурные точки для численного интегрирования КЭ
        /// </summary>
        protected NumInegrationPoints pIntegration;
        /// <summary>
        /// квадратурные точки для численного интегрирования граничных КЭ
        /// </summary>
        protected NumInegrationPoints bpIntegration;
        /// <summary>
        /// алгебра для КЭ задачи
        /// </summary>
        [NonSerialized]
        private IAlgebra algebra = null;
        /// <summary>
        /// количество узлов на КЭ
        /// </summary>
        const int CountEKnots = 3;
        /// <summary>
        /// количество узлов на КЭ
        /// </summary>
        const int CountBoundEKnots = 2;
        /// <summary>
        /// количество неизвестных в узле
        /// </summary>
        const int CountUnknow = 3;
        /// <summary>
        /// размерность пространства для трехузлового КЭ с тремя степенями свободы
        /// </summary>
        const int CountSpace = 9;
        /// <summary>
        /// размерность пространства для двухузлового КЭ с тремя степенями свободы
        /// </summary>
        const int CountBoundSpace = 6;
        /// <summary>
        /// ускорение свободгого падения
        /// </summary>
        const double GRAV = 9.806;
        /// <summary>
        /// основание натурального лагорифма
        /// </summary>
        const double E = 2.718;
        /// <summary>
        /// вектор приращений поля h u v за одну итерацию
        /// </summary>
        private double[] result;
        #endregion
        #region Локальные переменные
        /// <summary>
        /// Узлы КЭ
        /// </summary>
        protected uint[] knots = { 0, 0, 0 };
        protected uint[] bknots = { 0, 0 };
        /// <summary>
        /// координаты узлов КЭ
        /// </summary>
        protected double[] x = { 0, 0, 0 };
        protected double[] y = { 0, 0, 0 };
        protected double[] bx = { 0, 0 };
        protected double[] by = { 0, 0 };
        /// <summary>
        /// локальная матрица жесткости
        /// </summary>
        protected double[][] KMatrix = null;
        /// <summary>
        /// локальная матрица массы
        /// </summary>
        protected double[][] SMatrix = null;
        /// <summary>
        /// локальная матрица Якоби задачи
        /// </summary>
        protected double[][] JMatrix = null;
        /// <summary>
        /// локальная правая часть СЛАУ
        /// </summary>
        protected double[] FRight = null;
        /// <summary>
        /// адреса ГУ
        /// </summary>
        protected double[] Uo = null;
        protected double[] Vo = null;
        protected double[][] Ax = null;
        protected double[][] Ay = null;
        protected double[][] A2 = null;
        protected double[][] UI = null;
        protected double[][] wx, wy, I;
        //protected double[][] um;
        protected double[][] dxx, dyx, dxy, dyy;
        protected double[][] dnf;

        #region Массивы для метода по обновлению скоростей
        /// <summary>
        /// глобальная диагональная матрица массы
        /// </summary>
        protected double[] M = null;
        /// <summary>
        /// глобальные правые части
        /// </summary>
        protected double[] FH = null;
        protected double[] FU = null;
        protected double[] FV = null;
        protected double[][] ME = null;
        protected double[] FHE = null;
        protected double[] FUE = null;
        protected double[] FVE = null;
        #endregion
        #endregion
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="mesh">КЭ сетка</param>
        /// <param name="p">параметры задачи</param>
        /// <param name="algebra">алгебра задачи</param>
        public River2D(IMesh mesh = null, ParamsRiver p = null) : base(p)
        {
            Set(mesh);
            if (p != null)
                base.SetParams(p);

            MEM.Alloc2DClear(CountEKnots, ref A2);
            MEM.Alloc2DClear(CountEKnots, ref Ax);
            MEM.Alloc2DClear(CountEKnots, ref Ay);
            MEM.Alloc2DClear(CountEKnots, ref UI);

            MEM.Alloc2DClear(CountEKnots, ref wx);
            MEM.Alloc2DClear(CountEKnots, ref wy);
            //MEM.Alloc2DClear(CountEKnots, ref um);
            MEM.Alloc2DClear(CountEKnots, ref dnf);

            MEM.Alloc2DClear(CountEKnots, ref dxx);
            MEM.Alloc2DClear(CountEKnots, ref dyx);
            MEM.Alloc2DClear(CountEKnots, ref dxy);
            MEM.Alloc2DClear(CountEKnots, ref dyy);

            MEM.Alloc2DClear(CountEKnots, ref I);

            MEM.AllocClear(CountEKnots, ref Uo);
            MEM.AllocClear(CountEKnots, ref Vo);

            MEM.Alloc(CountEKnots, ref FHE);
            MEM.Alloc(CountEKnots, ref FUE);
            MEM.Alloc(CountEKnots, ref FVE);
            MEM.Alloc(CountEKnots, CountEKnots, ref ME);
        }
        // =====================================================================
        //                       Реализация    IRiver2D
        // =====================================================================
        #region Реализация    IRiver2D
        /// <summary>
        /// Чтение входных данных задачи из файла
        /// геометрия канала
        /// эволюция расходов/уровней
        /// </summary>
        /// <param name="p"></param>
        public bool LoadData(string fileName) { return false;  }
        public void Set(IMesh _mesh, IAlgebra a = null)
        {
            if (_mesh == null) return;
            if (_mesh != null)
                this.mesh = (TriRiverMesh)_mesh;
            pIntegration = new NumInegrationPoints();
            bpIntegration = new NumInegrationPoints();
            pIntegration.SetInt(0 + (int)mesh.typeRangeMesh, mesh.First);
            bpIntegration.SetInt(1 + (int)mesh.typeRangeMesh, TypeFunForm.Form_1D_L1);
            algebra = new AlgebraRiver(mesh);
        }
        public void SetZeta(double[] zeta, EBedErosion bedErosion)
        {
            Erosion = bedErosion;
            if (zeta != null)
                for (int i = 0; i < mesh.CountKnots; i++)
                    mesh.nodes[i].zeta = zeta[i];
        }
        /// <summary>
        /// Получить отметки дна
        /// </summary>
        /// <param name="zeta"></param>
        public void GetZeta(ref double[] zeta)
        {
            MEM.AllocClear(mesh.CountKnots, ref zeta);
            for (int i = 0; i < mesh.CountKnots; i++)
                zeta[i] = mesh.nodes[i].zeta;
        }
        /// <summary>
        /// расчет изменений полей (h,u,v) за один шаг по времени 
        /// </summary>
        /// <param name="result"></param>
        public void SolverStep()
        {
            try
            {

                if (mesh == null) return;
                if (algebra == null)
                    algebra = new AlgebraRiver(mesh);
                MEM.AllocClear((int)algebra.N, ref result);
                // ограничители
                double[,] BWM = new double[CountUnknow, 2];

                if (mesh == null) return;
                // формирование ГМЖ ГПЧ ГММ для всех конечных элементов 
                time += dtime;
                // обновление граничных условий
                BoundaryConditionUpdate();
                // формирование ГМЖ ГПЧ ГММ для всех конечных элементов 
                #region сборка ГМЖ и ГПЧ
                algebra.Clear();
                // ===========================================================================
                //                      сборка КЭ матриц и правх частей
                // ===========================================================================
                #region цикл по конечным элементам

                AbFunForm FunN = FunFormsManager.CreateKernel(mesh.First);
                AbFunForm FunL = FunFormsManager.CreateKernel(mesh.First);

                for (uint elem = 0; elem < mesh.CountElements; elem++)
                {
                    // очистка массивов ЛМЖ ММ...
                    ClearLocal();

                    RiverNode[] elemNodes = { mesh.nodes[mesh.AreaElems[elem].Vertex1],
                                         mesh.nodes[mesh.AreaElems[elem].Vertex2],
                                         mesh.nodes[mesh.AreaElems[elem].Vertex3] };
                    // получить узлы КЭ
                    mesh.ElementKnots(elem, ref knots);
                    // Получить координаты узлов
                    mesh.GetElemCoords(elem, ref x, ref y);
                    // установка координат узлов в функции формы
                    FunN.SetGeoCoords(x, y);
                    FunL.SetGeoCoords(x, y);

                    if (CheckMixedRB(elemNodes) == 1)
                    {
                        TriElementRiver currentFElement = mesh.AreaElems[elem];
                        // получить точки интегрирования для полузатопленного КЭ
                        GetMixedGPS(currentFElement, ref pIntegration);
                    }
                    else
                    {
                        // количество точек Гаусса(в данном случае 3)
                        pIntegration.SetInt(1 + (int)mesh.typeRangeMesh, mesh.First);
                    }
                    // флаг для отметки точек интегрирования попадающих на берег
                    for (int i = 0; i < pIntegration.CountIntegr; i++)
                        mesh.AreaElems[elem].gpcode[i] = 1;
                    // цикл по точкам интегрирования
                    for (int pi = 0; pi < pIntegration.CountIntegr; pi++)
                    {
                        // вычисление глоб. производных от функции формы
                        FunN.CalkDiffForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                        FunN.CalkForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                        // береговые ограничители
                        for (uint nod = 0; nod < CountEKnots; nod++)
                        {
                            BWM[nod, 0] = FunctionLimiters(elemNodes, nod, 0);
                            BWM[nod, 1] = FunctionLimiters(elemNodes, nod, 1);
                        }
                        // функции формы с усеченными по ограничителям производными 
                        FunL.CalkDiffForm(pIntegration.xi[pi], pIntegration.eta[pi], BWM);
                        FunL.CalkForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                        double SumWeight = 0.0;
                        for (int i = 0; i < pIntegration.CountIntegr; i++)
                            SumWeight += pIntegration.weight[i];
                        // формирование матриц (жесткости, масс, Якоби) и правой части
                        // в точках интегрирования
                        СomputationFiniteElementWetRiverBed(pi, elem, FunN, FunL, elemNodes, SumWeight);
                    }
                    #region реализация веменной схемы и метода Ньютона (1 итерация)
                    // похож на огрызок от нелинейной схемы
                    double tdt = theta; // * dt ;
                    double mtdt = (1.0 - theta); // * dt ;
                    for (int i = 0; i < CountSpace; i++)
                    {
                        for (int j = 0; j < CountEKnots; j++)
                        {
                            for (int jj = 0; jj < CountUnknow; jj++)
                            {
                                int k = j * CountUnknow + jj;
                                SMatrix[i][k] /= dtime;
                                FRight[i] += (SMatrix[i][k] - mtdt * KMatrix[i][k]) * elemNodes[j].uo[jj];
                                SMatrix[i][k] += tdt * KMatrix[i][k];
                            }
                        }
                    }
                    for (int i = 0; i < CountSpace; i++)
                    {
                        for (int j = 0; j < CountEKnots; j++)
                        {
                            for (int jj = 0; jj < CountUnknow; jj++)
                            {
                                int k = j * CountUnknow + jj;
                                FRight[i] -= SMatrix[i][k] * elemNodes[j].u[jj];
                                SMatrix[i][k] += tdt * JMatrix[i][k];
                            }
                        }
                    }
                    #endregion
                    // добавление вновь сформированной ЛЖМ в ГМЖ
                    algebra.AddToMatrix(SMatrix, knots);
                    // добавление вновь сформированной ЛПЧ в ГПЧ
                    algebra.AddToRight(FRight, knots);
                }
                #endregion
                // ===========================================================================
                //                          сборка граничных КЭ матриц
                // ===========================================================================
                #region цикл по граничным конечным элементам
                double dx, dy, Jaa, Jab;
                AbFunForm BFunN = FunFormsManager.CreateKernel(TypeFunForm.Form_1D_L1);
                for (uint belem = 0; belem < mesh.CountBoundElements; belem++)
                {
                    // очистка массивов ЛМЖ ММ...
                    ClearLocal();

                    BoundElementRiver bElement = mesh.BoundElems[belem];
                    // получить узлы КЭ
                    mesh.ElementBoundKnots(belem, ref bknots);
                    //
                    RiverNode[] elemNodes = { mesh.nodes[bknots[0]],
                                          mesh.nodes[bknots[1]] };

                    if (CheckMixedRB(elemNodes) == 1)
                    {
                        // получить точки интегрирования для полузатопленного КЭ
                        GetBoundaryGPS(elemNodes, ref bpIntegration);
                    }
                    else
                        bpIntegration.SetInt(1 + (int)mesh.typeRangeMesh, TypeFunForm.Form_1D_L1);

                    // цикл по точкам интегрирования
                    for (int pi = 0; pi < bpIntegration.CountIntegr; pi++)
                    {
                        // вычисление глоб. производных от функции формы
                        BFunN.CalkLocalDiffForm(bpIntegration.xi[pi], 0);
                        BFunN.CalkForm(bpIntegration.xi[pi], 0);
                        Jaa = 0.0;
                        Jab = 0.0;
                        for (int i = 0; i < BFunN.Count; i++)
                        {
                            Jaa += BFunN.DN_xi[i] * elemNodes[i].X;
                            Jab += BFunN.DN_xi[i] * elemNodes[i].Y;
                        }
                        dx = bpIntegration.weight[pi] * Jaa;
                        dy = bpIntegration.weight[pi] * Jab;
                        // Расчет локальынх матриц жесткости и правой части для ГРАНИЧНОГО КЭ в точке интегрирования
                        СomputationBoundaryFiniteElement(bElement, elemNodes, BFunN, dx, dy); // update_BKe
                    }
                    for (int i = 0; i < CountBoundSpace; i++)
                    {
                        for (int j = 0; j < CountBoundSpace; j++)
                        {
                            SMatrix[i][j] /= dtime;
                            SMatrix[i][j] += theta * KMatrix[i][j];
                        }
                    }
                    for (int i = 0; i < CountBoundSpace; i++)
                    {
                        for (int j = 0; j < CountBoundEKnots; j++)
                        {
                            for (int jj = 0; jj < CountUnknow; jj++)
                            {
                                int k = j * CountUnknow + jj;
                                FRight[i] -= SMatrix[i][k] * elemNodes[j].u[jj];
                                SMatrix[i][k] += theta * JMatrix[i][k];
                            }
                        }
                    }
                    // Сборка глобальных матриц жесткости(левой и правой) и глобальной правой части
                    // добавление вновь сформированной ЛЖМ в ГМЖ
                    algebra.AddToMatrix(SMatrix, bknots);

                    // добавление вновь сформированной ЛПЧ в ГПЧ
                    algebra.AddToRight(FRight, bknots);
                }
                #endregion
                // ===========================================================================
                //                          отработка проблемных узлов
                // ===========================================================================
                #region отработка проблемных узлов
                // анализ правой части системы
                algebra.GetRight(ref result);
                double resL2 = 0.0;
                for (int i = 0; i < result.Length; i++)
                {
                    double res = result[i];
                    resL2 += res * res;
                }
                double resLine = 0.5 * Math.Sqrt(resL2);
                // --------------------------------------------------------------------------
                // отработка проблемных узлов/элементов
                // --------------------------------------------------------------------------
                // счетчик проблемных узлов
                int nProbElms = 0;
                // цикл по КЭ
                for (uint elem = 0; elem < mesh.CountElements; elem++)
                {
                    // очистка массивов ЛМЖ ММ...
                    ClearLocal();
                    // получить узлы КЭ
                    mesh.ElementKnots(elem, ref knots);
                    RiverNode[] elemNodes = { mesh.nodes[mesh.AreaElems[elem].Vertex1],
                                          mesh.nodes[mesh.AreaElems[elem].Vertex2],
                                          mesh.nodes[mesh.AreaElems[elem].Vertex3] };
                    // цикл по узлам КЭ
                    for (int iN = 0; iN < CountEKnots; iN++)
                    {
                        // цикл по степеням свободы КЭ
                        for (int iV = 0; iV < CountUnknow; iV++)
                        {
                            if (result[knots[iN] * CountUnknow + iV] > resLine)
                            {
                                // Получить координаты узлов
                                mesh.GetElemCoords(elem, ref x, ref y);
                                // установка координат узлов в функции формы
                                FunN.SetGeoCoords(x, y);
                                FunL.SetGeoCoords(x, y);
                                // флаг для отметки точек интегрирования попадающих на берег
                                for (int i = 0; i < pIntegration.CountIntegr; i++)
                                    mesh.AreaElems[elem].gpcode[i] = 1;
                                double dtmin = 1000000.0;
                                // цикл по точкам интегрирования
                                for (int pi = 0; pi < pIntegration.CountIntegr; pi++)
                                {
                                    // вычисление глоб. производных от функции формы
                                    FunN.CalkDiffForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                                    FunN.CalkForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                                    // береговые ограничители
                                    for (uint nod = 0; nod < CountEKnots; nod++)
                                    {
                                        BWM[nod, 0] = FunctionLimiters(elemNodes, nod, 0);
                                        BWM[nod, 1] = FunctionLimiters(elemNodes, nod, 1);
                                    }
                                    // функции формы с усеченными по ограничителям производными 
                                    FunL.CalkDiffForm(pIntegration.xi[pi], pIntegration.eta[pi], BWM);
                                    FunL.CalkForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                                    double SumWeight = 0.0;
                                    for (int i = 0; i < pIntegration.CountIntegr; i++)
                                        SumWeight += pIntegration.weight[i];
                                    // формирование матриц (жесткости, масс, Якоби) и правой части
                                    // в точках интегрирования
                                    double dt = СomputationFiniteElementWetRiverBed(pi, elem, FunN, FunL, elemNodes, SumWeight);
                                    if (dt < dtmin)
                                        dtmin = dt;
                                }
                                for (int i = 0; i < CountSpace; i++)
                                {
                                    FRight[i] = 0.0;
                                    for (int j = 0; j < CountEKnots; j++)
                                    {
                                        for (int jj = 0; jj < CountUnknow; jj++)
                                        {
                                            SMatrix[i][j * CountUnknow + jj] /= dtmin;
                                        }
                                    }
                                }
                                // добавление вновь сформированной ЛЖМ в ГМЖ
                                algebra.AddToMatrix(SMatrix, knots);
                                // добавление вновь сформированной ЛПЧ в ГПЧ
                                algebra.AddToRight(FRight, knots);
                                //
                                iV = CountUnknow;
                                iN = CountEKnots;
                                nProbElms++;
                            }
                        }
                    }
                }
                #endregion

                #endregion сборка ГМЖ и ГПЧ
                // ===========================================================================
                //                              решение САУ
                //                      нахождение поправок решения
                // ===========================================================================
                algebra.Solve(ref result);
                // ---------------------------------------------------------------------------
                //                  анализ полученного решения
                // ---------------------------------------------------------------------------
                double varsum = 0.0;
                // норма поправок
                double uchange = 0.0;
                for (int i = 0; i < mesh.CountKnots; i++)
                {
                    mesh.nodes[i].h += result[i * CountUnknow];
                    mesh.nodes[i].qx += result[i * CountUnknow + 1];
                    mesh.nodes[i].qy += result[i * CountUnknow + 2];
                    varsum += mesh.nodes[i].h * mesh.nodes[i].h +
                              mesh.nodes[i].qx * mesh.nodes[i].qx +
                              mesh.nodes[i].qy * mesh.nodes[i].qy;
                }
                for (int i = 0; i < mesh.CountKnots; i++)
                    uchange += result[i] * result[i];
                if (varsum > 0.0)
                    uchange = Math.Sqrt(uchange / varsum);
                else
                    uchange = 0.0;
                // если изменение слишком велико, отклонить итерацию и записать информацию в файл журнала
                if (uchange > 1.25 * maxSolСorrection)
                {
                    Logger.Instance.Info("отклонить итерацию  t = " + time.ToString());
                    // откат по времени
                    time -= dtime;
                    // уменьшаем шаг по времени
                    dtTrend = 0.5 * maxSolСorrection / uchange;
                    // сбрасываем значения в узлах к значениям
                    // на предыдущем временном шаге
                    for (int i = 0; i < mesh.CountKnots; i++)
                    {
                        mesh.nodes[i].h = mesh.nodes[i].h0;
                        mesh.nodes[i].qx = mesh.nodes[i].qx0;
                        mesh.nodes[i].qy = mesh.nodes[i].qy0;
                    }
                }
                else
                {
                    // делаем сдвиг полученного решение на предыдущий слой
                    RollbackToOldValues();
                    // обновление скоростей
                    UpdateVelocities();
                    // коэффициент = (по умолчанию 0,05) / uchange
                    dtTrend = maxSolСorrection / uchange;
                    if (dtTrend > 1.5)
                        dtTrend = 1.5;
                }
                // обновление шага по времени
                dtime *= dtTrend;
                if (dtime > dtmax)
                    dtime = dtmax;
            }
            catch(Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
        }
        ///// <summary>
        ///// Получение полей глубины и скоростей на текущем шаге по времени
        ///// </summary>
        ///// <param name="h">глубины</param>
        ///// <param name="u">скорость по х</param>
        ///// <param name="v">скорость по у</param>
        public void Get(ref double[] zeta, ref double[] h, ref double[] u, ref double[] v)
        {
            MEM.AllocClear(mesh.CountKnots, ref zeta);
            MEM.AllocClear(mesh.CountKnots, ref h);
            MEM.AllocClear(mesh.CountKnots, ref u);
            MEM.AllocClear(mesh.CountKnots, ref v);
            for (int i = 0; i < mesh.CountKnots; i++)
            {
                zeta[i] = mesh.nodes[i].zeta;
                h[i] = mesh.nodes[i].h;
                u[i] = mesh.nodes[i].qx;
                v[i] = mesh.nodes[i].qy;
            }
        }
        /// <summary>
        /// Получение полей придонных касательных напряжений и давления/свободной поверхности по контексту задачи
        /// </summary>
        /// <param name="tauX">придонное касательное напряжение по х</param>
        /// <param name="tauY">придонное касательное напряжение по у</param>
        /// <param name="P">придонных давления/свободная поверхности потока, по контексту задачи</param>
        public void GetTau(ref double[] tauX, ref double[] tauY, ref double[] P, ref double[][] CS, StressesFlag stressFlag)
        {
            double[] Area = null;
            if (StressesFlag.Nod == stressFlag)
            {
                MEM.AllocClear(mesh.CountKnots, ref tauX);
                MEM.AllocClear(mesh.CountKnots, ref tauY);
                MEM.AllocClear(mesh.CountKnots, ref P);
                Area = new double[mesh.CountKnots];
            }
            else
            {
                MEM.AllocClear(mesh.CountElements, ref tauX);
                MEM.AllocClear(mesh.CountElements, ref tauY);
                MEM.AllocClear(mesh.CountElements, ref P);
            }
            for (uint elem = 0; elem < mesh.CountElements; elem++)
            {
                // получить узлы КЭ
                mesh.ElementKnots(elem, ref knots);
                RiverNode[] elemNodes = { mesh.nodes[mesh.AreaElems[elem].Vertex1],
                                          mesh.nodes[mesh.AreaElems[elem].Vertex2],
                                          mesh.nodes[mesh.AreaElems[elem].Vertex3] };
                double ks = 0, h = 0, qx = 0, qy = 0, zeta = 0, hice = 0, ksIce = 0;
                for (uint k = 0; k < CountEKnots; k++)
                {
                    ks += elemNodes[k].ks;
                    ksIce += elemNodes[k].KsIce;
                    zeta += elemNodes[k].zeta;
                    hice += elemNodes[k].Hice;
                    h += elemNodes[k].h;
                    qx += elemNodes[k].qx;
                    qy += elemNodes[k].qy;
                }
                ks /= CountEKnots;
                ksIce /= CountEKnots;
                h /= CountEKnots;
                zeta /= CountEKnots;
                qx /= CountEKnots;
                qy /= CountEKnots;
                hice /= CountEKnots;
                double h_clear = h - hice;
                double Ks = ks;
                double fac = 1;
                if (hice > 0)
                {
                    Ks = Math.Pow((Math.Pow(ks, 0.25) + Math.Pow(ksIce, 0.25)) / 2, 4);
                    fac = 2;
                }
                double U = 0, V = 0;
                if (h > H_minGroundWater)
                {
                    U = qx / h_clear;
                    V = qy / h_clear;
                }
                double A = 12.0 * h_clear / (fac * Ks);
                double e2 = E * E;
                double Cs;
                if (A > e2)
                    Cs = 2.5 * Math.Log(A);
                else
                    Cs = 2.5 + 2.5 / e2 * A;
                double Cs2 = Cs * Cs;
                double mU = Math.Sqrt(U * U + V * V);

                double tx = rho_w * U * mU / Cs2;
                double ty = rho_w * V * mU / Cs2;
                if (StressesFlag.Nod == stressFlag)
                {
                    double S = mesh.ElemSquare(elem) / 3;
                    Area[knots[0]] += S;
                    Area[knots[1]] += S;
                    Area[knots[2]] += S;

                    tauX[knots[0]] += tx * S;
                    tauX[knots[1]] += tx * S;
                    tauX[knots[2]] += tx * S;

                    tauY[knots[0]] += ty * S;
                    tauY[knots[1]] += ty * S;
                    tauY[knots[2]] += ty * S;

                    P[knots[0]] = (h + zeta) * S;
                    P[knots[1]] = (h + zeta) * S;
                    P[knots[2]] = (h + zeta) * S;
                }
                else
                {
                    tauX[elem] = tx;
                    tauY[elem] = ty;
                    //свободная поверхность
                    P[elem] = h + zeta;
                }
            }
            if (StressesFlag.Nod == stressFlag)
            {
                for (int nod = 0; nod < tauX.Length; nod++)
                {
                    tauX[nod] /= Area[nod];
                    tauY[nod] /= Area[nod];
                    P[nod] /= Area[nod];
                }
            }
        }

        /// <summary>
        /// Формирование данных для отрисовки связанных с сеткой IMesh со сороны задачи
        /// </summary>
        /// <param name="sp">контейнер данных</param>
        public void AddMeshPolesForGraphics(ISavePoint sp)
        {
            double[] zeta = null, h = null, qx = null, qy = null;
            Get(ref zeta, ref h, ref qx, ref qy);

            sp.Add("Расход ", qx, qy);
            sp.Add("Расход qx", qx);
            sp.Add("Расход qy", qy);

            sp.Add("Отметки дна", zeta);
            sp.Add("Глубина", h);

            double[] tauX = null, tauY = null, P = null;
            double[][] CS = null;
            GetTau(ref tauX, ref tauY, ref P, ref CS, StressesFlag.Nod);

            sp.Add("Напряжения ", tauX, tauY);
            sp.Add("Напряжения tauX", tauX);
            sp.Add("Напряжения tauY", tauY);
            sp.Add("Уровень свободной повверхности eta", P);
        }
        #endregion

        #region Вычисление ЛМЖ и ПЧ
        /// <summary>
        /// создание/очистка ЛМЖ и ЛПЧ ...
        /// </summary>
        /// <param name="cu">количество неизвестных</param>
        protected virtual void ClearLocal()
        {
            MEM.Alloc2DClear(CountSpace, ref KMatrix);
            MEM.Alloc2DClear(CountSpace, ref SMatrix);
            MEM.Alloc2DClear(CountSpace, ref JMatrix);
            MEM.AllocClear(CountSpace, ref FRight);
        }
        /// <summary>
        /// Расчет локальынх матриц жесткости и правой части для ГРАНИЧНОГО КЭ в точке интегрирования
        /// update_BKe
        /// </summary>
        public int СomputationBoundaryFiniteElement(BoundElementRiver bElement,
                    RiverNode[] elemNodes, AbFunForm BFunN, double dx, double dy)
        {
            int i, j, l, m;
            double Qx, Qy, H, U, V, Qn, Qt, dS, Ha, Qxa, Qya, qn, zb, K;
            double[][] AK = null;
            double[][] AJK = null;
            double[][] A = null;
            double[] cbcc = new double[CountUnknow];
            double[] bcc = new double[CountUnknow];
            double[] AF = new double[CountUnknow];
            double[] phi = new double[CountUnknow];
            double[] phin = new double[CountUnknow];
            double[] Un = new double[CountUnknow];
            double[] Vn = new double[CountUnknow];


            MEM.Alloc2DClear(CountUnknow, CountUnknow, ref A);
            MEM.Alloc2DClear(CountUnknow, CountUnknow, ref AK);
            MEM.Alloc2DClear(CountUnknow, CountUnknow, ref AJK);


            double Qxnew, Qynew, Hnew, Unew, Vnew, tice;
            double D0, D1, qf, dQn;//, qn0, qn1;
            double ex;

            // Вычислить параметры здесь
            // Примечание: Ha, Qxa и Qya - это переменные, которые используются
            // только для вычисления элементов в матрице A.

            Qxnew = Qynew = Hnew = Unew = Vnew = Ha = Qxa = Qya = zb = tice = 0.0;
            dS = Math.Sqrt(dx * dx + dy * dy);

            for (i = 0; i < CountBoundEKnots; i++)
                tice += BFunN.N[i] * elemNodes[i].Hice;

            if ((elemNodes[0].Hice > 0) && (elemNodes[1].Hice > 0))
                tice = iceCoeff * tice; //submerged depth
            else
                tice = 0.0;

            for (i = 0; i < CountBoundEKnots; i++)
            {
                if ((elemNodes[i].h - (elemNodes[i].Hice * iceCoeff)) > H_minGroundWater)
                {
                    Un[i] = elemNodes[i].qx / (elemNodes[i].h - tice);
                    Vn[i] = elemNodes[i].qy / (elemNodes[i].h - tice);
                }
                else
                {
                    Un[i] = 0.0;
                    Vn[i] = 0.0;
                }
            }

            for (i = 0; i < CountBoundEKnots; i++)
            {
                zb +=    BFunN.N[i] * elemNodes[i].zeta;
                Hnew +=  BFunN.N[i] * elemNodes[i].h;
                Qxnew += BFunN.N[i] * elemNodes[i].qx;
                Qynew += BFunN.N[i] * elemNodes[i].qy;
                Unew +=  BFunN.N[i] * Un[i];
                Vnew +=  BFunN.N[i] * Vn[i];
            }
            double cos = dx / dS;
            double sin = dy / dS;

            qn = - Qxnew * sin + Qynew * cos;
            H = bElement.Eta - zb;
            Qn = bElement.Qn;
            if (bElement.boundCondType == 5)
                Qn = 0.0;
            if (bElement.boundCondType == 1)
            {
                D0 = elemNodes[0].h - (elemNodes[0].Hice * iceCoeff);
                D1 = elemNodes[1].h - (elemNodes[1].Hice * iceCoeff);
                if ((D0 > 0.0) && (D1 > 0.0))
                {
                    qf = Math.Pow(D0, 1.67) / Math.Pow(D1, 1.67);
                    dQn = (qf - 1.0) / (qf + 1.0) * Qn;
                    Qn = ((Qn + dQn) * BFunN.N[0] + (Qn - dQn) * BFunN.N[1]);
                }
                else if ((D0 > 0.0) && (D1 <= 0.0))
                {
                    Qn *= 2.0 * BFunN.N[1];
                }
                else if ((D0 <= 0.0) && (D1 > 0.0))
                {
                    Qn *= 2.0 * BFunN.N[0];
                }
                else
                {
                    Qn = 0.0;
                }
            }
            Qt = bElement.Qt;
            Qx = -Qn * sin - Qt * cos;
            Qy =  Qn * cos - Qt * sin;
            phi[0] = H;
            phi[1] = Qx;
            phi[2] = Qy;
            double Hwater = Hnew - tice;
            if (Hwater > H_minGroundWater)
            {
                Unew = Qxnew / Hwater;
                Vnew = Qynew / Hwater;
            }
            else
            {
                Unew = 0.0;
                Vnew = 0.0;
            }

            phin[0] = Hnew;
            phin[1] = Qxnew;
            phin[2] = Qynew;

            int boundCondType = bElement.boundCondType;
            if (boundCondType == 3 && ((H - tice) < H_minGroundWater))
            {
                boundCondType = 0;
            }
            // глубина больше минимальной
            if (Hwater > H_minGroundWater)
            {
                // скорость больше критической скорости "звука"
                if (qn / Hwater > Math.Sqrt(Hwater * GRAV))
                {
                    if (boundCondType == 1) // тип граничных УСЛОВИЙ 1 - заданна глубина
                    {
                        H = Hnew;
                        boundCondType = 1;
                    }
                    if (boundCondType == 3) // тип граничных УСЛОВИЙ 2  - заданн расход (на выходе - по глубине)
                    {
                        if ((H - tice) > H_minGroundWater)
                        {
                            Qx = -(H - tice) * Math.Sqrt((H - tice) * GRAV) * dy / dS;
                            Qy = (H - tice) * Math.Sqrt((H - tice) * GRAV) * dx / dS;
                        }
                        else
                        {
                            Qx = 0.0;
                            Qy = 0.0;
                        }
                        // цикл по узлам (функциям формы) - установка ГЛАВНЫХ граничных условий методом ШТРАФА
                        for (i = 0; i < CountBoundEKnots; i++)
                        {
                            Ha = bElement.Eta - elemNodes[i].zeta;
                            // диагональные элементы матрицы
                            KMatrix[i * CountUnknow][i * CountUnknow] += 1.0E12;
                            // правая часть
                            FRight[i * CountUnknow] += 1.0E12 * Ha;
                            KMatrix[i * CountUnknow + 1][i * CountUnknow + 1] += 1.0E12;
                            KMatrix[i * CountUnknow + 2][i * CountUnknow + 2] += 1.0E12;
                            if ((Ha - tice) > H_minGroundWater)
                            {
                                FRight[i * CountUnknow + 1] += 1.0E12 * (-(Ha - tice) * Math.Sqrt((Ha - tice) * GRAV) * dy / dS);
                                FRight[i * CountUnknow + 2] += 1.0E12 * ((Ha - tice) * Math.Sqrt((Ha - tice) * GRAV) * dx / dS);
                            }
                            else
                            {
                                FRight[i * CountUnknow + 1] += 0.0;
                                FRight[i * CountUnknow + 2] += 0.0;
                            }
                        }
                    }
                }
                // Сверхкритический отток из области
                if (qn / Hwater < -Math.Sqrt((Hwater) * GRAV))
                    boundCondType = 0;
            }
            switch (boundCondType)
            {
                case 0:
                    bcc[0] = 0.0;
                    bcc[1] = 1.0;
                    bcc[2] = 1.0;
                    break;

                case 1:
                    bcc[0] = 0.0;
                    bcc[1] = 1.0;
                    bcc[2] = 1.0;
                    break;

                case 2:
                    bcc[0] = 1.0;
                    bcc[1] = 1.0;
                    bcc[2] = 1.0;
                    break;

                case 3:
                    bcc[0] = 1.0;
                    bcc[1] = 0.0;
                    bcc[2] = 0.0;
                    break;

                case 4:
                    bcc[0] = 0.0;
                    bcc[1] = 0.0;
                    bcc[2] = 0.0;
                    break;

                case 5:
                    bcc[0] = 0.0;
                    bcc[1] = 0.0;
                    bcc[2] = 0.0;
                    break;
            }

            for (i = 0; i < CountUnknow; i++)
                cbcc[i] = Math.Abs(bcc[i] - 1.0);

            Ha =  bcc[0] * H  + cbcc[0] * Hnew;
            Qxa = bcc[1] * Qx + cbcc[1] * Qxnew;
            Qya = bcc[2] * Qy + cbcc[2] * Qynew;

            if (((Ha - tice) < H_minGroundWater))
            {
                if (bElement.boundCondType == 3)
                {
                    for (i = 0; i < CountBoundEKnots; i++)
                    {
                        Ha = bElement.Eta - elemNodes[i].zeta;
                        if (((Ha - tice) < H_minGroundWater))
                        {
                            KMatrix[i * CountUnknow][i * CountUnknow] += 1.0E12;
                            FRight[i * CountUnknow] += 1.0E12 * Ha;
                        }
                    }
                }
                return (0);
            }

            if ((Ha - tice) >= H_minGroundWater)
            {
                if (bElement.boundCondType > 5)
                {
                    U = Unew;
                    V = Vnew;
                }
                else
                {
                    U = Qxa / (Ha - tice);
                    V = Qya / (Ha - tice);
                }
            }
            else
            {
                U = 0.0;
                V = 0.0;
            }

            AF[0] = 0.0;
            AF[1] = 0.0;
            AF[2] = 0.0;

            // A are associated with known boundary values

            A[0][0] = 0.0;
            A[0][1] = dy;
            A[0][2] = -dx;
            A[1][0] = GRAV * Ha / 2.0 * dy;
            A[2][0] = -GRAV * Ha / 2.0 * dx;
            A[1][1] = (U * dy - V * dx);
            A[1][2] = 0.0;
            A[2][1] = 0.0;
            A[2][2] = (U * dy - V * dx);

            // AK are associated with unknown boundary values

            AK[0][0] = 0.0;
            AK[0][1] = dy;
            AK[0][2] = -dx;
            AK[1][0] = GRAV * Ha / 2.0 * dy;
            AK[2][0] = -GRAV * Ha / 2.0 * dx;
            if (qn < 0.0)
            {
                AK[1][1] = (U * dy - V * dx);
                AK[1][2] = 0.0;
                AK[2][1] = 0.0;
                AK[2][2] = (U * dy - V * dx);
            }
            else
            {
                AK[1][1] = (U * dy);
                AK[1][2] = (V * dy);
                AK[2][1] = (-U * dx);
                AK[2][2] = (-V * dx);
            }
            // AJK - это вклад граничных членов в Якобиан
            AJK[0][0] = 0.0;
            AJK[0][1] = 0.0;
            AJK[0][2] = 0.0;
            AJK[1][0] = (GRAV * Ha / 2.0 - U * U) * dy + (U * V) * dx;
            AJK[1][1] = U * dy;
            AJK[2][0] = -(GRAV * Ha / 2.0 - V * V) * dx - (U * V) * dy;
            AJK[2][2] = -V * dx;
            if (qn < 0.0)
            {
                AJK[1][2] = -U * dx;
                AJK[2][1] = V * dy;
            }
            else
            {
                AJK[1][2] = V * dy;
                AJK[2][1] = -U * dx;
            }

            if ((bElement.boundCondType == 5) && ((Ha - tice) > H_minGroundWater))
            {
                ex = bElement.Qn;
                K = bElement.Eta * Math.Pow((Ha - tice), ex - 1.0);

                AK[0][0] = K * dS;
                AK[0][1] = 0.0;
                AK[0][2] = 0.0;

                AK[1][0] = (GRAV / 2 * Ha) * dy;
                AK[1][1] = K * dS;
                AK[1][2] = 0.0;

                AK[2][0] = -(GRAV / 2 * Ha) * dx;
                AK[2][1] = 0.0;
                AK[2][2] = K * dS;

                AJK[0][0] = K * (ex - 1.0) * dS;
                AJK[0][1] = 0.0;
                AJK[0][2] = 0.0;

                AJK[1][0] = (GRAV / 2 * Ha) * dy + (ex - 1.0) * K * U * dS;
                AJK[1][1] = 0.0;
                AJK[1][2] = 0.0;

                AJK[2][0] = -(GRAV / 2 * Ha) * dx + (ex - 1.0) * K * V * dS;
                AJK[2][1] = 0.0;
                AJK[2][2] = 0.0;

                AF[0] = K * tice * dS;
            }
            for (i = 0; i < CountBoundEKnots; i++)
            {
                for (l = 0; l < CountUnknow; l++)
                    FRight[i * CountUnknow + l] += AF[l] * BFunN.N[i];
                for (j = 0; j < CountBoundEKnots; j++)
                {
                    for (l = 0; l < CountUnknow; l++)
                    {
                        for (m = 0; m < CountUnknow; m++)
                        {
                            KMatrix[i * CountUnknow + l][j * CountUnknow + m] += AK[l][m] * cbcc[m] * BFunN.N[i] * BFunN.N[j];
                            FRight[i * CountUnknow + l] -= A[l][m] * bcc[m] * phi[m] * BFunN.N[i] * BFunN.N[j];

                            JMatrix[i * CountUnknow + l][j * CountUnknow + m] += AJK[l][m] * BFunN.N[i] * BFunN.N[j] * cbcc[m];
                        }
                    }
                }
            }
            return (0);
        }
        /// <summary>
        /// Расчет локальных матриц и правой части для диффузионного движения грунтовых вод
        /// вычисление конечного элемента в точке интегрирования попавшей на участок сухого русла реки
        /// update_gwKe
        /// </summary>
        /// <param name="FunN">функции формы симплекса</param>
        /// <param name="elemNodes">узлы КЭ</param>
        /// <param name="FunNDetJ">детерминант матрицы якоби умноженный на вес в точке интегрирования</param>
        void СalculationFiniteElementDryRiverBed(AbFunForm FunN, RiverNode[] elemNodes, double FunNDetJ)
        {
            double Sox = 0.0, Soy = 0.0;
            // расчет уклонов
            for (int i = 0; i < CountEKnots; i++)
            {
                Sox += -FunN.DN_x[i] * elemNodes[i].zeta;
                Soy += -FunN.DN_y[i] * elemNodes[i].zeta;
            }
            double gwd = filtrСoeff;
            double ff = rho_w * gwd;
            for (int i = 0; i < CountEKnots; i++)
            {
                for (int j = 0; j < CountEKnots; j++)
                {
                    KMatrix[i * CountUnknow][j * CountUnknow] += gwd * (FunN.DN_x[i] * FunN.DN_x[j] + FunN.DN_y[i] * FunN.DN_y[j]) * FunNDetJ;
                    SMatrix[i * CountUnknow][j * CountUnknow] += droundWaterCoeff * (FunN.N[i] * FunN.N[j]) * FunNDetJ;
                    if (i == j)
                    {
                        KMatrix[i * CountUnknow + 1][j * CountUnknow + 1] += ff * FunN.N[i] * FunN.N[j] * FunNDetJ;
                        KMatrix[i * CountUnknow + 2][j * CountUnknow + 2] += ff * FunN.N[i] * FunN.N[j] * FunNDetJ;
                    }
                }
                FRight[i * CountUnknow] += gwd * (FunN.DN_x[i] * Sox + FunN.DN_y[i] * Soy) * FunNDetJ;
            }
        }
        /// <summary>
        /// Расчет коэффициентов ЛМЖ и ЛПЧ
        /// вычисление конечного элемента для мокрого русла реки
        /// update_PGKe
        /// </summary>
        /// <param name="pi">номер точки интегрирования</param>
        /// <param name="elem">номер КЭ</param>
        /// <param name="FunN">функции формы симплекса</param>
        /// <param name="FunL">функции формы симплекса усеченного на границах</param>
        /// <param name="elemNodes">узлы КЭ</param>
        /// <param name="SumWeight">сумма весов схемы интегрирования</param>
        /// <returns></returns>
        double СomputationFiniteElementWetRiverBed(int pi, uint elem, AbFunForm FunN, AbFunForm FunL, RiverNode[] elemNodes, double SumWeight)
        {
            double UV, cx, cy, cs, us, vs;
            double Cstar, Qxnew, Qynew, Hnew, Unew, Vnew, Sox, Soy, ff, root, nu, Htemp;
            double Qxold, Qyold, Hold, Uold, Vold;
            double dHdx, dHdy, dQXdx, dQXdy, dQYdx, dQYdy;
            double dnudH = 0, dnudQX = 0, dnudQY = 0, SXi = 0, SYi = 0;
            double dSXdHj, dSXdQXj, dSXdQYj, dSYdHj, dSYdQXj, dSYdQYj;
            double dMXdHj, JMXdH = 0, dMYdHj, JMYdH = 0;
            double dMXdQXj, JMXdQX = 0, dMYdQXj, JMYdQX = 0;
            double dMXdQYj, JMXdQY = 0, dMYdQYj, JMYdQY = 0;
            double dUdx, dUdy, dVdx, dVdy, Ph;
            double c, c2, c22, U2, V2, e2;
            double Ud, Vd, Hd;
            double se1, se2, se3, Iu, IIu, IIIu, Ic, IIc, IIIc, div;
            double cs2, dCdH;
            double kice;            // underice roughness	
            double tice;            // submerged ice thickness
            double fac;             // fac = 1.0 when open water, fac = 2.0 when ice covered
            double ticeCheck, rdetJ, rSWE = 0, rGWE = 0;
            double UWTemp;
            double fCoriolis, dtNatural;
            kice = tice = ticeCheck = 0.0;
            Qxnew = Qynew = Hnew = Unew = Vnew = Sox = Soy = ff = Cstar = 0.0;
            Qxold = Qyold = Hold = Uold = Vold = 0.0;
            Ud = Vd = Hd = 0.0;
            dHdx = dHdy = dQXdx = dQXdy = dQYdx = dQYdy = 0.0;
            dUdx = dUdy = dVdx = dVdy = 0.0;
            for (int i = 0; i < CountUnknow; i++)
                I[i][i] = 1;
            // расчет глубин и толщины льда
            for (int i = 0; i < CountEKnots; i++)
            {
                Hnew += FunN.N[i] * elemNodes[i].h;
                tice += FunN.N[i] * elemNodes[i].Hice;
            }
            tice = iceCoeff * tice;
            ticeCheck = tice;
            double H_clear = Hnew - tice; // H_clear
            double weight_pi = pIntegration.weight[pi];
            double FunNDetJ = FunN.DetJ * weight_pi;
            if (H_clear < 2.0 * H_minGroundWater)
            {
                // если глубина потока меньше критической, то флаг точки интегрирования равен 0
                mesh.AreaElems[elem].gpcode[pi] = 0;
                if (H_clear <= H_minGroundWater)
                {
                    rSWE = 0.0;
                    rGWE = 1.0;
                }
                else
                {
                    rSWE = ((H_clear) - H_minGroundWater) / H_minGroundWater;
                    //  Hermite cubic blending function
                    // Функция кубического смешивания Эрмита
                    rSWE = (3.0 - 2.0 * rSWE) * rSWE * rSWE;
                    rGWE = 1.0 - rSWE;
                }
            }
            // если точка интегрирования попадает на берег выполняем расчет диффузионной матрицы жесткости
            if (mesh.AreaElems[elem].gpcode[pi] == 0)
            {
                rdetJ = FunNDetJ;
                FunNDetJ *= rGWE;
                // расчет локальных матриц и правой части для диффузионного движения грунтовых вод
                СalculationFiniteElementDryRiverBed(FunN, elemNodes, FunNDetJ);
                if (rSWE <= 0.0)
                {
                    dtNatural = FunNDetJ / weight_pi * SumWeight / 4.0 / filtrСoeff * droundWaterCoeff;
                    return (dtNatural);
                }
                else
                    FunNDetJ = rdetJ * rSWE;
            }

            if ((Hnew - ticeCheck) < H_minGroundWater)
                tice = 0.0;

            for (int i = 0; i < CountEKnots; i++)
            {
                Uo[i] = 0.0;
                Vo[i] = 0.0;
                if ((elemNodes[i].h0 - tice) > H_minGroundWater)
                {
                    double WaterHice = elemNodes[i].Hice * iceCoeff;
                    double Hwater = elemNodes[i].h0 - WaterHice;
                    if (Hwater > H_minGroundWater)
                    {
                        Uo[i] = elemNodes[i].qx0 / Hwater;
                        Vo[i] = elemNodes[i].qy0 / Hwater;
                    }
                }
            }
            // вычисление функции в точке интегрирования
            for (int i = 0; i < CountEKnots; i++)
            {
                Qxnew += FunN.N[i] * elemNodes[i].qx;
                Qynew += FunN.N[i] * elemNodes[i].qy;
                Hold += FunN.N[i] * elemNodes[i].h0;
                Qxold += FunN.N[i] * elemNodes[i].qx0;
                Qyold += FunN.N[i] * elemNodes[i].qy0;
                ff += FunN.N[i] * elemNodes[i].ks;
                Sox += -FunN.DN_x[i] * elemNodes[i].zeta;
                Soy += -FunN.DN_y[i] * elemNodes[i].zeta;
                dHdx += FunN.DN_x[i] * elemNodes[i].h;
                dHdy += FunN.DN_y[i] * elemNodes[i].h;
                dQXdx += FunN.DN_x[i] * elemNodes[i].qx;
                dQXdy += FunN.DN_y[i] * elemNodes[i].qx;
                dQYdx += FunN.DN_x[i] * elemNodes[i].qy;
                dQYdy += FunN.DN_y[i] * elemNodes[i].qy;
                dUdx += FunN.DN_x[i] * Uo[i];
                dUdy += FunN.DN_y[i] * Uo[i];
                dVdx += FunN.DN_x[i] * Vo[i];
                dVdy += FunN.DN_y[i] * Vo[i];
                kice += FunN.N[i] * elemNodes[i].KsIce;
                Hd += FunN.N[i] * elemNodes[i].hd;
                Ud += FunN.N[i] * elemNodes[i].udx;
                Vd += FunN.N[i] * elemNodes[i].udy;
            }

            for (int i = 0; i < CountEKnots; i++)
            {
                for (int j = 0; j < CountEKnots; j++)
                {
                    wx[i][j] = 0.0;
                    wy[i][j] = 0.0;
                    dxx[i][j] = 0.0;
                    dxy[i][j] = 0.0;
                    dyx[i][j] = 0.0;
                    dyy[i][j] = 0.0;
                    //um[i][j] = 0.0;
                    Ax[i][j] = 0.0;
                    Ay[i][j] = 0.0;
                    A2[i][j] = 0.0;
                    UI[i][j] = 0.0;
                }
            }
            UWTemp = UpWindCoeff;
            if ((ticeCheck > 0.0) && ((Hnew - ticeCheck) < H_minGroundWater))
                UpWindCoeff = 0.0;

            for (int i = 0; i < CountEKnots; i++)
            {
                FunL.DN_x[i] *= (UpWindCoeff * Math.Sqrt(FunNDetJ / weight_pi * SumWeight / 4.0));
                FunL.DN_y[i] *= (UpWindCoeff * Math.Sqrt(FunNDetJ / weight_pi * SumWeight / 4.0));
            }

            if ((elemNodes[0].Hice > 0) && (elemNodes[1].Hice > 0) && (elemNodes[2].Hice > 0))
            {
                ff = Math.Pow((Math.Pow(kice, 0.25) + Math.Pow(ff, 0.25)) / 2, 4);
                fac = 2.0;
            }
            else
            {
                tice = 0.0;
                kice = 0.0;
                fac = 1.0;
                ticeCheck = 0.0;
            }
            Unew = Qxnew / H_clear;
            Vnew = Qynew / H_clear;
            Uold = Qxold / (Hold - tice);
            Vold = Qyold / (Hold - tice);

            if ((ticeCheck > 0.0) && ((Hnew - ticeCheck) < H_minGroundWater))
            {
                Unew = 0.0;
                Vnew = 0.0;
            }

            UV = Unew * Vnew;
            cx = GRAV * H_clear - Unew * Unew;
            cy = GRAV * H_clear - Vnew * Vnew;
            U2 = 2.0 * Unew;
            V2 = 2.0 * Vnew;

            c22 = GRAV * Hnew / 2.0;

            Htemp = 12.0 * H_clear / (fac * ff);
            e2 = E * E;
            if (Htemp > e2)
            {
                Cstar = 2.5 * Math.Log(Htemp);
                dCdH = 2.5 / H_clear;
            }
            else
            {
                Cstar = 2.5 + 2.5 / e2 * Htemp;
                dCdH = 30.0 / e2 / ff;
            }

            root = Math.Sqrt(Qxnew * Qxnew + Qynew * Qynew);
            cs2 = Cstar * Cstar;
            ff = fac * root / H_clear / H_clear / cs2;
            double ff1 = fac * root / (H_clear * H_clear * cs2);
            if ((ticeCheck > 0.0) && ((Hnew - ticeCheck) < H_minGroundWater))
                ff = 100;

            fCoriolis = 0.000146 * Math.Sin(latitudeArea * 0.0174533);

            double CF = fac / root / H_clear / H_clear / cs2;
            double CFF = fac / (root * H_clear * H_clear * cs2);
            double CF1 = fac * root / H_clear / H_clear / H_clear / cs2 * (1.0 + H_clear * dCdH / Cstar);
            double CF2 = fac * root / (H_clear * H_clear * H_clear * cs2) * (1.0 + H_clear * dCdH / Cstar);
            dnf[0][0] = 0.0;
            dnf[0][1] = 0.0;
            dnf[0][2] = 0.0;
            dnf[1][0] = -2.0 * Qxnew * CF1;
            dnf[2][0] = -2.0 * Qynew * CF1;
            if (root == 0.0)
            {
                dnf[1][1] = 0.0;
                dnf[1][2] = 0.0;
                dnf[2][1] = 0.0;
                dnf[2][2] = 0.0;
            }
            else
            {
                dnf[1][1] = Qxnew * Qxnew * CF;
                dnf[1][2] = Qxnew * Qynew * CF;
                dnf[2][1] = Qxnew * Qynew * CF;
                dnf[2][2] = Qynew * Qynew * CF;
            }

            if (UpWindCoeff > 0.0)
            {
                c = Math.Sqrt(Math.Abs((Hold - tice) * GRAV));
                cs = c * c;
                us = Uold * Uold;
                vs = Vold * Vold;
                c2 = 2.0 * c;

                Ax[0][0] = 0.0;
                Ax[0][1] = 1.0;
                Ax[0][2] = 0.0;

                Ax[1][0] = cs - us;
                Ax[1][1] = 2.0 * Uold;
                Ax[1][2] = 0.0;

                Ax[2][0] = -Uold * Vold;
                Ax[2][1] = Vold;
                Ax[2][2] = Uold;


                Ay[0][0] = 0.0;
                Ay[0][1] = 0.0;
                Ay[0][2] = 1.0;

                Ay[1][0] = -Uold * Vold;
                Ay[1][1] = Vold;
                Ay[1][2] = Uold;

                Ay[2][0] = cs - vs;
                Ay[2][1] = 0.0;
                Ay[2][2] = 2.0 * Vold;

                // сумма квадратов матриц A2 = Ax^2 + Ay^2
                for (int row = 0; row < CountUnknow; row++)
                    for (int col = 0; col < CountUnknow; col++)
                        A2[row][col] = CalkDotMatrix(Ax, Ax, row, col, CountUnknow) + CalkDotMatrix(Ay, Ay, row, col, CountUnknow);

                // собственные числа
                se1 = Math.Sqrt(us + vs + cs);
                se2 = Math.Sqrt(3.0 * cs + 2.0 * us + 2.0 * vs + c * Math.Sqrt(cs + 16.0 * us + 16.0 * vs)) / Math.Sqrt(2.0);
                se3 = Math.Sqrt(3.0 * cs + 2.0 * us + 2.0 * vs - c * Math.Sqrt(cs + 16.0 * us + 16.0 * vs)) / Math.Sqrt(2.0);
                // коэффициенты полинома (se1+x)(se3+x)(se3+x)
                Iu = se1 + se2 + se3;
                IIu = se1 * se2 + se2 * se3 + se3 * se1;
                IIIu = se1 * se2 * se3;
                // коэффициенты полинома (se1^2+x)(se3^2+x)(se3^2+x)
                Ic = se1 * se1 + se2 * se2 + se3 * se3;
                IIc = se1 * se1 * se2 * se2 + se2 * se2 * se3 * se3 + se3 * se3 * se1 * se1;
                IIIc = IIIu * IIIu;

                div = IIIu * IIIu * (IIIu + Iu * Ic) + Iu * Iu * (Iu * IIIc + IIIu * IIc);
                for (int row = 0; row < CountUnknow; row++)
                {
                    for (int col = 0; col < CountUnknow; col++)
                        UI[row][col] = (Iu * (Iu * IIu - IIIu) * CalkDotMatrix(A2, A2, row, col, CountUnknow) -
                                   (Iu * IIu - IIIu) * (Iu * Ic + IIIu) * A2[row][col] +
                                   (IIu * IIIu * (Iu * Ic + IIIu) + Iu * Iu * (IIu * IIc + IIIc)) * I[row][col]) / div;
                }
                for (int row = 0; row < CountUnknow; row++)
                    for (int col = 0; col < CountUnknow; col++)
                    {
                        wx[row][col] = CalkDotMatrix(UI, Ax, row, col, CountUnknow);
                        wy[row][col] = CalkDotMatrix(UI, Ay, row, col, CountUnknow);
                    }

                #region Нигде не используется
                // цикл по функциям формы
                //for (int i = 0; i < CountEKnots; i++)
                //{
                //    // циклы по узлам
                //    for (int l = 0; l < CountUnknow; l++)
                //        for (int m = 0; m < CountUnknow; m++)
                //            um[l][m] = wx[l][m] * FunL.DN_x[i] + wy[l][m] * FunL.DN_y[i];
                //}
                #endregion
            }
            UpWindCoeff = UWTemp;
            // diffusion terms 
            Ph = Math.Sqrt(2.0 * dUdx * dUdx + (dUdy + dVdx) * (dUdy + dVdx) + 2.0 * dVdy * dVdy);

            //dec = dHdx * Qxnew + dHdy * Qynew;
            if (Cstar > 1.0)
                nu = turbulentVisCoeff * root / Cstar + MEM.Error8 * Hold * Hold * Ph;
            else
                nu = turbulentVisCoeff * root / 1.0 + MEM.Error8 * Hold * Hold * Ph;
            if ((ticeCheck > 0.0) && ((Hnew - ticeCheck) < H_minGroundWater))
                nu = 0.0;

            dxx[1][0] = -Unew * (2.0 * nu);
            dxx[1][1] = 2.0 * nu;
            dxx[2][0] = -Vnew * nu;
            dxx[2][2] = 1.0 * nu;

            dyx[2][0] = -Unew * nu;
            dyx[2][1] = nu;
            
            dxy[1][0] = -Vnew * nu;
            dxy[1][2] = nu;
            
            dyy[1][0] = -Unew * nu;
            dyy[1][1] = nu;
            dyy[2][0] = -Vnew * (2.0 * nu);
            dyy[2][2] = 2.0 * nu;
            dnudH = -nu / Cstar * dCdH;
            if (root > 0.0)
            {
                dnudQX = nu / root / root * Qxnew;
                dnudQY = nu / root / root * Qynew;
            }
            else
            {
                dnudQX = 0.0;
                dnudQY = 0.0;
            }
            JMXdH = ((GRAV + 2 * Unew * Unew / H_clear) * dHdx
                      - 2 * Unew / H_clear * dQXdx
                      + 2 * Unew * Vnew / H_clear * dHdy
                      - Vnew / H_clear * dQXdy
                      - Unew / H_clear * dQYdy) + dnf[1][0];
            JMYdH = ((GRAV + 2 * Vnew * Vnew / H_clear) * dHdy
                      - 2 * Vnew / H_clear * dQYdy + 2 * Unew * Vnew / H_clear * dHdx
                      - Unew / H_clear * dQYdx - Vnew / H_clear * dQXdx) + dnf[2][0];
            JMXdQX = -2 * Unew / H_clear * dHdx + 2 / H_clear * dQXdx - Vnew / H_clear * dHdy + dQYdy / H_clear + dnf[1][1];
            JMYdQX = -Vnew / H_clear * dHdx + dQYdx / H_clear + dnf[2][1];
            JMXdQY = -Unew / H_clear * dHdy + dQXdy / H_clear + dnf[1][2];
            JMYdQY = -2 * Vnew / H_clear * dHdy + 2 / H_clear * dQYdy - Unew / H_clear * dHdx + dQXdx / H_clear + dnf[2][2];
            // цикл по функциям формы
            for (int i = 0; i < CountEKnots; i++)
            {
                SXi = FunN.DN_x[i] * 2 * (dQXdx - dHdx * Unew)
                    + FunN.DN_y[i] * (-Unew * dHdy - Vnew * dHdx + dQXdy + dQYdx);
                SYi = FunN.DN_y[i] * 2 * (dQYdy - dHdy * Vnew)
                    + FunN.DN_x[i] * (-Vnew * dHdx - Unew * dHdy + dQYdx + dQXdy);
                // цикл по функциям формы
                for (int j = 0; j < CountEKnots; j++)
                {
                    // матрица жесткости
                    KMatrix[i * CountUnknow][j * CountUnknow] += K00(FunL, FunN, i, j, cx, cy, UV, Sox, Soy, FunNDetJ);
                    KMatrix[i * CountUnknow][j * CountUnknow + 1] += K01(FunL, FunN, i, j, U2, Vnew, ff, fCoriolis, FunNDetJ);
                    KMatrix[i * CountUnknow][j * CountUnknow + 2] += K02(FunL, FunN, i, j, Unew, V2, ff, fCoriolis, FunNDetJ);
                    KMatrix[i * CountUnknow + 1][j * CountUnknow] += K10(FunL, FunN, i, j, c22, cx, cy, UV, Sox, Soy, tice, FunNDetJ);
                    KMatrix[i * CountUnknow + 1][j * CountUnknow + 1] += K11(FunL, FunN, i, j, Unew, Vnew, U2, ff, fCoriolis, FunNDetJ);
                    KMatrix[i * CountUnknow + 1][j * CountUnknow + 2] += K12(FunL, FunN, i, j, Unew, V2, ff, fCoriolis, FunNDetJ);
                    KMatrix[i * CountUnknow + 2][j * CountUnknow] += K20(FunL, FunN, i, j, c22, cx, cy, UV, Sox, Soy, tice, FunNDetJ);
                    KMatrix[i * CountUnknow + 2][j * CountUnknow + 1] += K21(FunL, FunN, i, j, U2, Vnew, ff, fCoriolis, FunNDetJ);
                    KMatrix[i * CountUnknow + 2][j * CountUnknow + 2] += K22(FunL, FunN, i, j, Unew, Vnew, V2, ff, fCoriolis, FunNDetJ);

                    dMXdHj = JMXdH * FunN.N[j];
                    dMYdHj = JMYdH * FunN.N[j];
                    dMXdQXj = JMXdQX * FunN.N[j];
                    dMYdQXj = JMYdQX * FunN.N[j];
                    dMXdQYj = JMXdQY * FunN.N[j];
                    dMYdQYj = JMYdQY * FunN.N[j];
                    dSXdHj = (SXi * dnudH + ((2 * FunN.DN_x[i] * dHdx + FunN.DN_y[i] * dHdy) * Unew / Hnew
                                            + FunN.DN_y[i] * dHdx * Vnew / Hnew) * nu) * FunN.N[j];
                    dSYdHj = (SYi * dnudH + ((2 * FunN.DN_y[i] * dHdy + FunN.DN_x[i] * dHdx) * Vnew / Hnew
                                            + FunN.DN_x[i] * dHdy * Unew / Hnew) * nu) * FunN.N[j];
                    dSXdQXj = (SXi * dnudQX - (2 * FunN.DN_x[i] * dHdx + FunN.DN_y[i] * dHdy) / Hnew * nu) * FunN.N[j];
                    dSYdQXj = (SYi * dnudQX - (FunN.DN_x[i] * dHdy) / Hnew * nu) * FunN.N[j];
                    dSXdQYj = (SXi * dnudQY - (FunN.DN_y[i] * dHdx) / Hnew * nu) * FunN.N[j];
                    dSYdQYj = (SYi * dnudQY - (2 * FunN.DN_y[i] * dHdy + FunN.DN_x[i] * dHdx) / Hnew * nu) * FunN.N[j];
                    // матрица Якоби
                    JMatrix[i * CountUnknow][j * CountUnknow] += J00(FunL, FunN, i, j, dMXdHj, dMYdHj, FunNDetJ);
                    JMatrix[i * CountUnknow][j * CountUnknow + 1] += J01(FunL, FunN, i, j, Hnew, Unew, Vnew, ff, dMXdQXj, dMYdQXj, FunNDetJ);
                    JMatrix[i * CountUnknow][j * CountUnknow + 2] += J02(FunL, FunN, i, j, Hnew, Unew, Vnew, ff, dMXdQYj, dMYdQYj, FunNDetJ);
                    JMatrix[i * CountUnknow + 1][j * CountUnknow] += J10(FunL, FunN, i, j, Hnew, Unew, Vnew, ff, nu, dMXdHj, dMYdHj, dSXdHj, FunNDetJ);
                    JMatrix[i * CountUnknow + 1][j * CountUnknow + 1] += J11(FunL, FunN, i, j, Hnew, Unew, Vnew, ff, nu, dMXdQXj, dMYdQXj, dSXdQXj, FunNDetJ);
                    JMatrix[i * CountUnknow + 1][j * CountUnknow + 2] += J12(FunL, FunN, i, j, Hnew, Unew, Vnew, ff, nu, dMXdQYj, dMYdQYj, dSXdQYj, FunNDetJ);
                    JMatrix[i * CountUnknow + 2][j * CountUnknow] += J20(FunL, FunN, i, j, Hnew, Unew, Vnew, ff, nu, dMXdHj, dMYdHj, dSYdHj, FunNDetJ);
                    JMatrix[i * CountUnknow + 2][j * CountUnknow + 1] += J21(FunL, FunN, i, j, Hnew, Unew, Vnew, ff, nu, dMXdQXj, dMYdQXj, dSYdQXj, FunNDetJ);
                    JMatrix[i * CountUnknow + 2][j * CountUnknow + 2] += J22(FunL, FunN, i, j, Hnew, Unew, Vnew, ff, nu, dMXdQYj, dMYdQYj, dSYdQYj, FunNDetJ);
                }
                FRight[i * CountUnknow] += filtrСoeff * (FunN.DN_x[i] * Sox + FunN.DN_y[i] * Soy) * FunNDetJ -
                                                              (CalkUW(0, 1, i, FunL) * GRAV * tice * Sox +
                                                               CalkUW(0, 2, i, FunL) * GRAV * tice * Soy) * FunNDetJ;
                FRight[i * CountUnknow + 1] += -(FunN.N[i] * GRAV * tice * Sox +
                                                 CalkUW(1, 1, i, FunL) * GRAV * tice * Sox +
                                                 CalkUW(1, 2, i, FunL) * GRAV * tice * Soy) * FunNDetJ;
                FRight[i * CountUnknow + 2] += -(FunN.N[i] * GRAV * tice * Soy +
                                                 CalkUW(2, 1, i, FunL) * GRAV * tice * Sox +
                                                 CalkUW(2, 2, i, FunL) * GRAV * tice * Soy) * FunNDetJ;
            }
            // Элементы матрицы жесткости + матрица масс для глубины
            for (int i = 0; i < CountEKnots; i++)           // цикл по узлам
            {
                for (int l = 0; l < CountUnknow; l++)     // цикл по степеням свободы
                {
                    int ii = i * CountUnknow + l;
                    for (int j = 0; j < CountEKnots; j++)       // цикл по узлам
                    {
                        for (int m = 0; m < CountUnknow; m++) // цикл по степеням свободы
                        {
                            int jj = j * CountUnknow + m;
                            SMatrix[ii][jj] += (wx[l][m] * FunL.DN_x[i] + wy[l][m] * FunL.DN_y[i]) * FunN.N[j] * FunNDetJ;
                            if (l == m)
                                SMatrix[ii][jj] += FunN.N[i] * FunN.N[j] * FunNDetJ;
                        }
                    }
                }
            }
            dtNatural = Math.Sqrt(FunNDetJ / weight_pi * SumWeight / 4.0) / (Math.Abs(Vnew) + Math.Sqrt(Math.Abs(GRAV * Hnew)));
            return (dtNatural);
        }
        /// <summary>
        /// Весовая функция формы
        /// </summary>
        /// <param name="l"></param>
        /// <param name="m"></param>
        /// <param name="i"></param>
        /// <param name="FunL"></param>
        /// <returns></returns>
        double CalkUW(int l, int m, int i, AbFunForm FunL)
        {
            return wx[l][m] * FunL.DN_x[i] + wy[l][m] * FunL.DN_y[i];
        }
        double K00(AbFunForm FunL, AbFunForm FunN, int i, int j, double cx, double cy, double UV, double Sox, double Soy, double FunNDetJ)
        {
            double t3, t4, t5, t6, t;
            t3 = (cx * CalkUW(0, 1, i, FunL) - UV * CalkUW(0, 2, i, FunL)) * FunN.DN_x[j];
            t4 = (cy * CalkUW(0, 2, i, FunL) - UV * CalkUW(0, 1, i, FunL)) * FunN.DN_y[j];
            t5 = (-CalkUW(0, 1, i, FunL) * GRAV * Sox - CalkUW(0, 2, i, FunL) * GRAV * Soy);
            t6 = filtrСoeff * (FunN.DN_x[i] * FunN.DN_x[j] + (FunN.DN_y[i] * FunN.DN_y[j]));
            t = (t3 + t4 + t5 * FunN.N[j] + t6) * FunNDetJ;
            return (t);
        }
        double K01(AbFunForm FunL, AbFunForm FunN, int i, int j, double U2, double V, double ff, double fCoriolis, double FunNDetJ)
        {
            double t1, t3, t4, t6, t7, t;
            t1 = -FunN.DN_x[i];
            t3 = (CalkUW(0, 0, i, FunL) + U2 * CalkUW(0, 1, i, FunL) + V * CalkUW(0, 2, i, FunL)) * FunN.DN_x[j];
            t4 = V * CalkUW(0, 1, i, FunL) * FunN.DN_y[j];
            t6 = /*CalkDotMatrix(um,nf,0,1,3)*/ CalkUW(0, 1, i, FunL) * ff;
            t7 = CalkUW(0, 2, i, FunL) * fCoriolis;
            t = ((t1 + t6 + t7) * FunN.N[j] + t3 + t4) * FunNDetJ;
            return (t);
        }
        double K02(AbFunForm FunL, AbFunForm FunN, int i, int j, double U, double V2, double ff, double fCoriolis, double FunNDetJ)
        {
            double t2, t3, t4, t6, t7, t;
            t2 = -FunN.DN_y[i];
            t3 = U * CalkUW(0, 2, i, FunL) * FunN.DN_x[j];
            t4 = (CalkUW(0, 0, i, FunL) + U * CalkUW(0, 1, i, FunL) + V2 * CalkUW(0, 2, i, FunL)) * FunN.DN_y[j];
            t6 = CalkUW(0, 2, i, FunL) * ff;
            t7 = -fCoriolis * CalkUW(0, 1, i, FunL);
            t = ((t2 + t6 + t7) * FunN.N[j] + t3 + t4) * FunNDetJ;
            return (t);
        }
        double K10(AbFunForm FunL, AbFunForm FunN, int i, int j, double c22, double cx, double cy, double UV, double Sox, double Soy, double ts, double FunNDetJ)
        {
            double t1, t2, t3, t4, t5, t6, t7, t8, t;
            t1 = -c22 * FunN.DN_x[i];
            t2 = -GRAV * ts * FunN.N[i] * FunN.DN_x[j];
            t3 = (cx * CalkUW(1, 1, i, FunL) - UV * CalkUW(1, 2, i, FunL)) * FunN.DN_x[j];
            t4 = (cy * CalkUW(1, 2, i, FunL) - UV * CalkUW(1, 1, i, FunL)) * FunN.DN_y[j];
            t5 = -GRAV * Sox * FunN.N[i];
            t6 = (-CalkUW(1, 1, i, FunL) * GRAV * Sox - CalkUW(1, 2, i, FunL) * GRAV * Soy);
            t7 = dxx[1][0] * FunN.DN_x[i] * FunN.DN_x[j] + dxy[1][0] * FunN.DN_y[i] * FunN.DN_x[j];
            t8 = dyy[1][0] * FunN.DN_y[i] * FunN.DN_y[j];
            t = ((t1 + t5 + t6) * FunN.N[j] + t2 + t3 + t4 + t7 + t8) * FunNDetJ;
            return (t);
        }
        double K11(AbFunForm FunL, AbFunForm FunN, int i, int j, double U, double V, double U2, double ff, double fCoriolis, double FunNDetJ)
        {
            double t1, t2, t3, t4, t5, t6, t8, t9, t10, t;
            t1 = -U * FunN.DN_x[i];
            t2 = -V * FunN.DN_y[i];

            t3 = (CalkUW(1, 0, i, FunL) + U2 * CalkUW(1, 1, i, FunL) + V * CalkUW(1, 2, i, FunL)) * FunN.DN_x[j];
            t4 = V * CalkUW(1, 1, i, FunL) * FunN.DN_y[j];
            t5 = ff * FunN.N[i];
            t6 = CalkUW(1, 1, i, FunL) * ff;
            t8 = dxx[1][1] * FunN.DN_x[i] * FunN.DN_x[j];
            t9 = dyy[1][1] * FunN.DN_y[i] * FunN.DN_y[j];
            t10 = fCoriolis * CalkUW(1, 2, i, FunL);

            t = ((t1 + t2 + t5 + t6 + t10) * FunN.N[j] + t3 + t4 + t8 + t9) * FunNDetJ;
            return (t);
        }
        double K12(AbFunForm FunL, AbFunForm FunN, int i, int j, double U, double V2, double ff, double fCoriolis, double FunNDetJ)
        {
            double t3, t4, t6, t8, t9, t10, t;

            t3 = U * CalkUW(1, 2, i, FunL) * FunN.DN_x[j];
            t4 = (CalkUW(1, 0, i, FunL) + U * CalkUW(1, 1, i, FunL) + V2 * CalkUW(1, 2, i, FunL)) * FunN.DN_y[j];
            t6 = CalkUW(1, 2, i, FunL) * ff;
            t8 = dxy[1][2] * FunN.DN_y[i] * FunN.DN_x[j];
            t9 = -fCoriolis * FunN.N[i] * FunN.N[j];
            t10 = -fCoriolis * CalkUW(1, 1, i, FunL);

            t = ((t6 + t10) * FunN.N[j] + t3 + t4 + t8 + t9) * FunNDetJ;
            return (t);
        }
        double K20(AbFunForm FunL, AbFunForm FunN, int i, int j, double c22, double cx, double cy, double UV, double Sox, double Soy, double ts, double FunNDetJ)
        {
            double t1, t2, t3, t4, t5, t6, t7, t8, t;

            t1 = -c22 * FunN.DN_y[i];
            t2 = -GRAV * ts * FunN.N[i] * FunN.DN_y[j];
            t3 = (cx * CalkUW(2, 1, i, FunL) - UV * CalkUW(2, 2, i, FunL)) * FunN.DN_x[j];
            t4 = (cy * CalkUW(2, 2, i, FunL) - UV * CalkUW(2, 1, i, FunL)) * FunN.DN_y[j];
            t5 = -GRAV * Soy * FunN.N[i];
            t6 = (-CalkUW(2, 1, i, FunL) * GRAV * Sox - CalkUW(2, 2, i, FunL) * GRAV * Soy);
            t7 = dxx[2][0] * FunN.DN_x[i] * FunN.DN_x[j] + dyx[2][0] * FunN.DN_x[i] * FunN.DN_y[j];
            t8 = dyy[2][0] * FunN.DN_y[i] * FunN.DN_y[j];

            t = ((t1 + t5 + t6) * FunN.N[j] + t2 + t3 + t4 + t7 + t8) * FunNDetJ;
            return (t);
        }
        double K21(AbFunForm FunL, AbFunForm FunN, int i, int j, double U2, double V, double ff, double fCoriolis, double FunNDetJ)
        {
            double t3, t4, t6, t8, t9, t10, t;

            t3 = (CalkUW(2, 0, i, FunL) + U2 * CalkUW(2, 1, i, FunL) + V * CalkUW(2, 2, i, FunL)) * FunN.DN_x[j];
            t4 = V * CalkUW(2, 1, i, FunL) * FunN.DN_y[j];
            t6 = CalkUW(2, 1, i, FunL) * ff;
            t8 = dyx[2][1] * FunN.DN_x[i] * FunN.DN_y[j];
            t9 = fCoriolis * FunN.N[i] * FunN.N[j];
            t10 = fCoriolis * CalkUW(2, 2, i, FunL);

            t = ((t6 + t10) * FunN.N[j] + t3 + t4 + t8 + t9) * FunNDetJ;
            return (t);
        }
        double K22(AbFunForm FunL, AbFunForm FunN, int i, int j, double U, double V, double V2, double ff, double fCoriolis, double FunNDetJ)
        {
            double t1, t2, t3, t4, t5, t6, t8, t9, t10, t;

            t1 = -U * FunN.DN_x[i];
            t2 = -V * FunN.DN_y[i];
            t3 = U * CalkUW(2, 2, i, FunL) * FunN.DN_x[j];
            t4 = (CalkUW(2, 0, i, FunL) + U * CalkUW(2, 1, i, FunL) + V2 * CalkUW(2, 2, i, FunL)) * FunN.DN_y[j];
            t5 = ff * FunN.N[i];
            t6 = CalkUW(2, 2, i, FunL) * ff;
            t8 = dxx[2][2] * FunN.DN_x[i] * FunN.DN_x[j];
            t9 = dyy[2][2] * FunN.DN_y[i] * FunN.DN_y[j];
            t10 = -fCoriolis * CalkUW(2, 1, i, FunL);

            t = ((t1 + t2 + t5 + t6 + t10) * FunN.N[j] + t3 + t4 + t8 + t9) * FunNDetJ;
            return (t);
        }
        #region  Якобиан
        double J00(AbFunForm FunL, AbFunForm FunN, int i, int j, double dMXdHj, double dMYdHj, double FunNDetJ)
        {
            double t3, t4, t;
            t3 = CalkUW(0, 1, i, FunL) * dMXdHj;
            t4 = CalkUW(0, 2, i, FunL) * dMYdHj;
            t = (t3 + t4) * FunNDetJ;
            return (t);
        }
        double J01(AbFunForm FunL, AbFunForm FunN, int i, int j, double H, double U, double V, double ff, double dMXdQX, double dMYdQX, double FunNDetJ)
        {
            double t3, t4, t;
            t3 = CalkUW(0, 1, i, FunL) * dMXdQX;
            t4 = CalkUW(0, 2, i, FunL) * dMYdQX;
            t = (t3 + t4) * FunNDetJ;
            return (t);
        }
        double J02(AbFunForm FunL, AbFunForm FunN, int i, int j, double H, double U, double V, double ff, double dMXdQY, double dMYdQY, double FunNDetJ)
        {
            double t3, t4, t;
            t3 = CalkUW(0, 1, i, FunL) * dMXdQY;
            t4 = CalkUW(0, 2, i, FunL) * dMYdQY;
            t = (t3 + t4) * FunNDetJ;
            return (t);
        }
        double J10(AbFunForm FunL, AbFunForm FunN, int i, int j, double H, double U, double V, double ff, double nu, double dMXdHj, double dMYdHj, double dSXdHj, double FunNDetJ)
        {
            double t1, t2, t3, t4, t5, d8, t;
            t1 = -(GRAV * H / 2.0 - U * U) * FunN.DN_x[i];
            t2 = U * V * FunN.DN_y[i];
            t3 = CalkUW(1, 1, i, FunL) * dMXdHj;
            t4 = CalkUW(1, 2, i, FunL) * dMYdHj;
            t5 = dnf[1][0] * FunN.N[i];
            d8 = dSXdHj;
            t = ((t1 + t2 + t5) * FunN.N[j] + (t3 + t4) + d8) * FunNDetJ;
            return (t);
        }
        double J11(AbFunForm FunL, AbFunForm FunN, int i, int j, double H, double U, double V, double ff, double nu, double dMXdQX, double dMYdQX, double dSXdQXj, double FunNDetJ)
        {
            double t1, t3, t4, t5, d8, t;
            t1 = -U * FunN.DN_x[i];
            t3 = CalkUW(1, 1, i, FunL) * dMXdQX;
            t4 = CalkUW(1, 2, i, FunL) * dMYdQX;
            t5 = dnf[1][1] * FunN.N[i];
            d8 = dSXdQXj;
            t = ((t1 + t5) * FunN.N[j] + (t3 + t4) + d8) * FunNDetJ;
            return (t);
        }
        double J12(AbFunForm FunL, AbFunForm FunN, int i, int j, double H, double U, double V, double ff, double nu, double dMXdQY, double dMYdQY, double dSXdQYj, double FunNDetJ)
        {
            double t2, t3, t4, t5, d8, t;
            t2 = -U * FunN.DN_y[i];
            t3 = CalkUW(1, 1, i, FunL) * dMXdQY;
            t4 = CalkUW(1, 2, i, FunL) * dMYdQY;
            t5 = dnf[1][2] * FunN.N[i];
            d8 = dSXdQYj;
            t = ((t2 + t5) * FunN.N[j] + (t3 + t4) + d8) * FunNDetJ;
            return (t);
        }
        double J20(AbFunForm FunL, AbFunForm FunN, int i, int j, double H, double U, double V, double ff, double nu, double dMXdHj, double dMYdHj, double dSYdHj, double FunNDetJ)
        {
            double t1, t2, t3, t4, t5, d8, t;
            t1 = U * V * FunN.DN_x[i];
            t2 = -(GRAV * H / 2.0 - V * V) * FunN.DN_y[i];
            t3 = CalkUW(2, 1, i, FunL) * dMXdHj;
            t4 = CalkUW(2, 2, i, FunL) * dMYdHj;
            t5 = dnf[2][0] * FunN.N[i];
            d8 = dSYdHj;
            t = ((t1 + t2 + t5) * FunN.N[j] + (t3 + t4) + d8) * FunNDetJ;
            return (t);
        }
        double J21(AbFunForm FunL, AbFunForm FunN, int i, int j, double H, double U, double V, double ff, double nu, double dMXdQX, double dMYdQX, double dSYdQXj, double FunNDetJ)
        {
            double t1, t3, t4, t5, d8, t;
            t1 = -V * FunN.DN_x[i];
            t3 = CalkUW(2, 1, i, FunL) * dMXdQX;
            t4 = CalkUW(2, 2, i, FunL) * dMYdQX;
            t5 = dnf[2][1] * FunN.N[i];
            d8 = dSYdQXj;
            t = ((t1 + t5) * FunN.N[j] + (t3 + t4) + d8) * FunNDetJ;
            return (t);
        }
        double J22(AbFunForm FunL, AbFunForm FunN, int i, int j, double H, double U, double V, double ff, double nu, double dMXdQY, double dMYdQY, double dSYdQYj, double FunNDetJ)
        {
            double t2, t3, t4, t5, d8, t;
            t2 = -V * FunN.DN_y[i];
            t3 = CalkUW(2, 1, i, FunL) * dMXdQY;
            t4 = CalkUW(2, 2, i, FunL) * dMYdQY;
            t5 = dnf[2][2] * FunN.N[i];
            d8 = dSYdQYj;
            t = ((t2 + t5) * FunN.N[j] + (t3 + t4) + d8) * FunNDetJ;
            return (t);
        }
        #endregion

        /// <summary>
        /// умножение l строки матрицы А с m - м столбцом матрицы B
        /// </summary>
        double CalkDotMatrix(double[][] A, double[][] B, int row, int col, int n)
        {
            double t = 0.0;
            for (int i = 0; i < n; i++)
                t += A[row][i] * B[i][col];
            return (t);
        }
        #endregion

        /// <summary>
        /// Учет граничных узлов 
        /// </summary>
        /// <param name="elemNodes">узлы КЭ</param>
        /// <param name="nod">индекс локального узла</param>
        /// <param name="i">коррдината x: i=0, y: i=1</param>
        /// <returns></returns>
        protected double FunctionLimiters(RiverNode[] elemNodes, uint nod, uint i)
        {
            // получить функции формы
            AbFunForm funforms = FunFormsManager.CreateKernel(mesh.First);
            double mult, r1 = 0, r2 = 0, s1 = 0, s2 = 0;
            uint nbn = 3, l1, l2;
            // количество узлов для КЭ заданного типа
            mult = 1.0;
            if (elemNodes[nod].fxc == FixedFlag.fixednode)
            {
                if (nod == 0 && (elemNodes[1].fxc == FixedFlag.fixednode))
                {
                    l1 = 0;
                    l2 = 1;
                    funforms.CalkVertex(l1, ref r1, ref s1);
                    funforms.CalkVertex(l2, ref r2, ref s2);
                    if (r1 == r2)
                    {
                        if (i == 0) mult = 0.0;
                    }
                    if (s1 == s2)
                    {
                        if (i == 1) mult = 0.0;
                    }

                }

                if (nod == 0 && (elemNodes[nbn - 1].fxc) == FixedFlag.fixednode)
                {
                    l1 = 0;
                    l2 = nbn - 1;
                    funforms.CalkVertex(l1, ref r1, ref s1);
                    funforms.CalkVertex(l2, ref r2, ref s2);
                    if (r1 == r2)
                    {
                        if (i == 0) mult = 0.0;
                    }
                    if (s1 == s2)
                    {
                        if (i == 1) mult = 0.0;
                    }

                }

                if (nod == (nbn - 1) && elemNodes[nbn - 2].fxc == FixedFlag.fixednode)
                {
                    l1 = nbn - 1;
                    l2 = nbn - 2;
                    funforms.CalkVertex(l1, ref r1, ref s1);
                    funforms.CalkVertex(l2, ref r2, ref s2);
                    if (r1 == r2)
                    {
                        if (i == 0) mult = 0.0;
                    }
                    if (s1 == s2)
                    {
                        if (i == 1) mult = 0.0;
                    }

                }

                if (nod == (nbn - 1) && elemNodes[0].fxc == FixedFlag.fixednode)
                {
                    l1 = nbn - 1;
                    l2 = 0;
                    funforms.CalkVertex(l1, ref r1, ref s1);
                    funforms.CalkVertex(l2, ref r2, ref s2);
                    if (r1 == r2)
                    {
                        if (i == 0) mult = 0.0;
                    }
                    if (s1 == s2)
                    {
                        if (i == 1) mult = 0.0;
                    }

                }

                if (nod != 0 && nod != (nbn - 1))
                {
                    if (elemNodes[nod + 1].fxc == FixedFlag.fixednode)
                    {
                        l1 = nod;
                        l2 = nod + 1;
                        funforms.CalkVertex(l1, ref r1, ref s1);
                        funforms.CalkVertex(l2, ref r2, ref s2);
                        if (r1 == r2)
                        {
                            if (i == 0) mult = 0.0;
                        }
                        if (s1 == s2)
                        {
                            if (i == 1) mult = 0.0;
                        }

                    }
                    if (elemNodes[nod - 1].fxc == FixedFlag.fixednode)
                    {
                        l1 = nod;
                        l2 = nod - 1;
                        funforms.CalkVertex(l1, ref r1, ref s1);
                        funforms.CalkVertex(l2, ref r2, ref s2);
                        if (r1 == r2)
                        {
                            if (i == 0) mult = 0.0;
                        }
                        if (s1 == s2)
                        {
                            if (i == 1) mult = 0.0;
                        }
                    }
                }
            }
            return (mult);
        }
        /// <summary>
        /// set_bvalues() == >
        /// Определение потоков и глубин на граничных конечных элементаъ по данным с сегментов
        /// </summary>
        public void BoundaryConditionUpdate() // update_BCs
        {
            int sID = 0, eID = 0;
            int dryFlag;
            // цикл по граничным элементам и обнуление массивов bcs и p
            for (int i = 0; i < mesh.CountBoundElements; i++)
                mesh.BoundElems[i].Clear();
            // цикл по граничным сегментам
            for (int i = 0; i < mesh.CountSegment; i++)
            {
                mesh.boundSegment[i].Length = 0;
                mesh.boundSegment[i].Dcoef = 0;
                // поиск граничных элементов сегмента
                mesh.GetBConditionID(i, ref sID, ref eID);
                // уровень свободной поверхности в живом сечении потока
                double Eta_SrossSection = 0.0;
                // вычисление среднего уровня свободной поверхности в живом сечении потока
                for (uint be = (uint)sID; ; be++)
                {
                    // длина граничного элемента
                    double Length = mesh.GetBoundElemLength(be);
                    mesh.BoundElems[be].Length = Length;
                    // вычисление длины створа
                    mesh.boundSegment[i].Length += Length;
                    // вычисление среднего между узлами уровня свободной поверхности
                    double wse = mesh.FreeFlowSurfaceElement(be);
                    Eta_SrossSection += wse * Length;
                    if (be >= eID)
                        break;
                }
                // средний уровень свободной поверхности
                double freeFlowSurface = Eta_SrossSection / mesh.boundSegment[i].Length;
                double fac = 1;
                // Проходим снова через bsegment, оценивая относительную емкость каждого ГЭ 
                for (int be = sID; ; be++)
                {
                    mesh.BoundElems[be].segmentID = mesh.boundSegment[i].ID;
                    mesh.BoundElems[be].boundCondType = mesh.boundSegment[i].boundCondType;
                    mesh.BoundElems[be].H = 0;
                    mesh.BoundElems[be].Zeta = 0;
                        dryFlag = 0;
                    uint Vertex1 = mesh.BoundElems[be].Vertex1;
                    uint Vertex2 = mesh.BoundElems[be].Vertex2;
                    if ((mesh.nodes[Vertex1].h - mesh.nodes[Vertex1].Hice * iceCoeff) < H_minGroundWater)
                        dryFlag = 1;
                    if ((mesh.nodes[Vertex2].h - mesh.nodes[Vertex2].Hice * iceCoeff) < H_minGroundWater)
                        dryFlag = 1;
                    // Вычисляем среднюю по граничному элементу глубину
                    mesh.BoundElems[be].H =  // p[3]
                         (mesh.nodes[Vertex1].h + mesh.nodes[Vertex2].h) * 0.5;
                    // Вычисляем среднюю по граничному элементу отметку дна
                    mesh.BoundElems[be].Zeta =  // p[4]
                        (mesh.nodes[Vertex1].zeta + mesh.nodes[Vertex2].zeta) * 0.5; // дно
                    double ice1 = mesh.nodes[mesh.BoundElems[be].Vertex1].Hice;
                    double ice2 = mesh.nodes[mesh.BoundElems[be].Vertex2].Hice;
                    double tice = (ice1 + ice2) * 0.5;
                    // глубина
                    
                    mesh.BoundElems[be].H = freeFlowSurface - mesh.BoundElems[be].Zeta;
                    if ((ice1 > 0) && (ice2 > 0))
                    {
                        mesh.BoundElems[be].H -= tice; // выичтаем из глубины толщину льда
                        fac = 1.0 / 1.5874; // ?
                    }
                    
                    mesh.BoundElems[be].Eta = mesh.boundSegment[i].Hn;
                    mesh.BoundElems[be].Qn = mesh.boundSegment[i].Qn;
                    
                    if (((mesh.BoundElems[be].H) <= H_minGroundWater) || dryFlag == 1)
                    {
                        // гкэ сухой, нет потока через границу
                        mesh.BoundElems[be].H = 0.0;
                    }
                    if (be >= eID)
                        break;
                }
                // Снова пропустите через bsegment, уменьшая пропускную способность Гр.Элем., 
                // которые примыкают к берегу с нулевым потоком.
                if ((mesh.boundSegment[i].boundCondType == 1) || (mesh.boundSegment[i].boundCondType == 2))
                {
                    for (int be = sID; ; be++)
                    {
                        if (be != sID)
                        {
                            if (mesh.BoundElems[be - 1].H < MEM.Error3)
                                mesh.BoundElems[be].H /= 3.0;
                        }
                        if (be != eID)
                        {
                            if (mesh.BoundElems[be + 1].H < MEM.Error3)
                                mesh.BoundElems[be].H /= 3.0;
                        }
                        mesh.boundSegment[i].Dcoef += fac * Math.Pow(mesh.BoundElems[be].H, (5.0 / 3.0)) * mesh.BoundElems[be].Length;
                        if (mesh.BoundElems[be].H > 0)
                            mesh.BoundElems[be].H = mesh.BoundElems[be].H;
                        if (be >= eID)
                            break;
                    }
                }
                // Наконец, вычисляем q для всех частей этого bsegment
                if ((mesh.boundSegment[i].boundCondType == 1) || (mesh.boundSegment[i].boundCondType == 2))
                {
                    for (int be = sID; ; be++)
                    {
                        if (mesh.BoundElems[be].boundCondType == 1 || (mesh.BoundElems[be].boundCondType == 2))
                        {
                            double Hice1 = mesh.nodes[mesh.BoundElems[be].Vertex1].Hice;
                            double Hice2 = mesh.nodes[mesh.BoundElems[be].Vertex2].Hice;
                            if ((Hice1 > 0) && (Hice2 > 0))
                                fac = 1.0 / 1.5874;
                            else
                                fac = 1.0;
                            if (mesh.boundSegment[i].Dcoef > 0.0)
                            {
                                // распределение расхода в узлы пропорционально глубине потока
                                double normDcoef_i = fac * Math.Pow(mesh.BoundElems[be].H, (5.0 / 3.0)) / mesh.boundSegment[i].Dcoef;
                                mesh.BoundElems[be].Qn = normDcoef_i * mesh.boundSegment[i].Qn;
                                mesh.BoundElems[be].Qn = fac * Math.Pow(mesh.BoundElems[be].H, (5.0 / 3.0))
                                    * mesh.boundSegment[i].Qn / mesh.boundSegment[i].Dcoef;
                            }
                            else
                            {
                                // равномерное распределение расхода в узлы  
                                mesh.BoundElems[be].Qn = mesh.boundSegment[i].Qn / mesh.boundSegment[i].Length;
                                mesh.BoundElems[be].Qt = 0;
                            }
                        }
                        if (be >= eID)
                            break;
                    }
                }
            }
        }
        /// <summary>
        /// Перекидывает решение из центров КЭ в узлы КЭ
        /// </summary>
        public void UpdateVelocities() // Called by transient() Вызывается transient ()
        {
            MEM.Alloc(mesh.CountKnots, ref M,  0);
            MEM.Alloc(mesh.CountKnots, ref FH, 0);
            MEM.Alloc(mesh.CountKnots, ref FU, 0);
            MEM.Alloc(mesh.CountKnots, ref FV, 0);

            int i, j, iRow;
            double qx, qy, H, tice, depth;//, curvature;
            // получить функции формы
            AbFunForm FF = FunFormsManager.CreateKernel(mesh.First);
            // цикл по конечным элементам
            for (uint elem = 0; elem < mesh.CountElements; elem++)
            {
                RiverNode[] elemNodes = { mesh.nodes[mesh.AreaElems[elem].Vertex1],
                                          mesh.nodes[mesh.AreaElems[elem].Vertex2],
                                          mesh.nodes[mesh.AreaElems[elem].Vertex3] };
                mesh.GetElemCoords(elem, ref x, ref y);
                // установка координат узлов в функции формы
                FF.SetGeoCoords(x, y);
                // инициализируем матрицы элементов
                for (i = 0; i < CountEKnots; i++)
                {
                    for (j = 0; j < CountEKnots; j++)
                    {
                        ME[i][j] = 0.0;
                    }
                    FUE[i] = 0.0;
                    FVE[i] = 0.0;
                    FHE[i] = 0.0;
                }
                // получаем расположение и веса точек интегрирования Гаусса в
                // зависимости от глубины и льда
                if (CheckMixedRB(elemNodes) == 1)
                {
                    TriElementRiver currentFElement = mesh.AreaElems[elem];
                    // плучить точки интегрирования для полузатопленного КЭ
                    GetMixedGPS(currentFElement, ref pIntegration);
                }
                else
                {
                    // количество точек Гаусса(в данном случае 3)
                    pIntegration.SetInt(1 + (int)mesh.typeRangeMesh, mesh.First);
                }
                // цикл по точкам интегрирования
                for (int pi = 0; pi < pIntegration.CountIntegr; pi++)
                {
                    // вычисление глоб. производных от функции формы
                    FF.CalkDiffForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                    FF.CalkForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                    double DWJ = FF.DetJ * pIntegration.weight[pi];
                    H = tice = qx = qy = 0;// curvature = 0;
                    for (j = 0; j < CountEKnots; j++)
                    {
                        RiverNode nodeP = elemNodes[j];
                        H += FF.N[j] * nodeP.h;
                        tice += FF.N[j] * nodeP.Hice;
                        qx += FF.N[j] * nodeP.qx;
                        qy += FF.N[j] * nodeP.qy;
                    }
                    tice = iceCoeff * tice;
                    // вычисляем вклады в матрицу элементов для этой точки Гаусса
                    for (i = 0; i < CountEKnots; i++)
                    {
                        for (j = 0; j < CountEKnots; j++)
                        {
                            ME[i][j] += FF.N[i] * FF.N[j] * DWJ;// gfp.detJ;
                        }
                        depth = H - tice;
                        // фильтрация скоростей по текущей глубине
                        if (depth > H_minGroundWater)
                        {
                            FHE[i] += FF.N[i] * depth * DWJ;
                            FUE[i] += FF.N[i] * qx / depth * DWJ;
                            FVE[i] += FF.N[i] * qy / depth * DWJ;
                        }
                    }
                }
                // собирать матрицы элементов в глобальные векторы,
                // объединяя матрицу ME в глобальный вектор M
                for (i = 0; i < CountEKnots; i++)
                {
                    iRow = elemNodes[i].i;
                    // собираем матрицу масс в диагональную матрицу 
                    for (j = 0; j < CountEKnots; j++)
                    {
                        M[iRow] += ME[i][j];
                    }
                    FH[iRow] += FHE[i];
                    FU[iRow] += FUE[i];
                    FV[iRow] += FVE[i];
                }
            }
            //  обновить узловые глубины и скорости,
            //  решая диагональные системы [M] {U} = {FU}
            for (int iNode = 0; iNode < mesh.CountKnots; iNode++)
            {
                RiverNode nodeP = mesh.nodes[iNode];
                nodeP.hd =  FH[iNode] / M[iNode];
                nodeP.udx = FU[iNode] / M[iNode];
                nodeP.udy = FV[iNode] / M[iNode];
            }
        }
        /// <summary>
        /// Проверка, если (глубина потока) - (толщина льда) в узле 
        /// больше минимальной глубины то 0 иначе 1
        /// если 1 то элемент смешанный вода/берег
        /// </summary>
        /// <param name="elemNodes"></param>
        /// <returns></returns>
        int CheckMixedRB(RiverNode[] elemNodes)
        {
            double mult;
            // количетсво проверок для 3 узлового и 2 узлового КЭ
            int Count = elemNodes.Length == CountEKnots ? CountEKnots : 1;
            for (int i = 0; i < Count; i++)
            {
                int j = (i + 1) % elemNodes.Length;
                mult = ((elemNodes[i].h0 - (elemNodes[i].Hice * iceCoeff)) - H_minGroundWater) *
                       ((elemNodes[j].h0 - (elemNodes[j].Hice * iceCoeff)) - H_minGroundWater);
                if (mult < 0.0)
                    return 1;
            }
            return 0;
        }
        /// <summary>
        /// Получить точки интегрирования для полузатопленного граничного элемента 
        /// </summary>
        /// <param name="currentBoundFElement"></param>
        /// <param name="g"></param>
        /// <returns></returns>
        public int GetBoundaryGPS(RiverNode[] elemNodes, ref NumInegrationPoints bpIntegration)
        {
            int ngauss = 0;
            double[] r = new double[2];
            double gwH = H_minGroundWater;
            double a = (elemNodes[0].h - (elemNodes[0].Hice * iceCoeff)) - gwH;
            double c = (elemNodes[1].h - (elemNodes[1].Hice * iceCoeff)) - gwH;
            double ri = 2.0 * Math.Abs(a) / (Math.Abs(a) + Math.Abs(c));
            bpIntegration.ReSize(4);
            r[0] = -1.0;
            r[1] = ri - 1.0;
            ngauss = FindBoundaryGPS(r, bpIntegration, ngauss);
            r[0] = ri - 1.0;
            r[1] = 1.0;
            ngauss = FindBoundaryGPS(r, bpIntegration, ngauss);
            return (ngauss);
        }
        public int FindBoundaryGPS(double[] r, NumInegrationPoints bpIntegration, int ngauss)
        {
            double[] f = new double[2];
            f[0] = 0.5 * (1.0 + 1.0 / Math.Sqrt(3.0));
            f[1] = 0.5 * (1.0 - 1.0 / Math.Sqrt(3.0));
            bpIntegration.xi[ngauss] = r[0] * f[0] + r[1] * f[1];
            bpIntegration.weight[ngauss] = (r[1] - r[0]) / 2.0;
            ngauss++;
            bpIntegration.xi[ngauss] = r[0] * f[1] + r[1] * f[0];
            bpIntegration.weight[ngauss] = (r[1] - r[0]) / 2.0;
            ngauss++;
            return (ngauss);
        }
        /// <summary>
        /// Смешанные точки интегрирования для полу затопленного симплекса 
        /// </summary>
        /// </summary>
        /// <param name="currentFElement"></param>
        /// <param name="pIntegration"></param>
        /// <returns>количество точек интегрирования</returns>
        int GetMixedGPS(TriElementRiver currentFElement, ref NumInegrationPoints pIntegration)
        {
            // количество точек интегрирования
            int ngauss = 0;

            int i, nipts, nLeft = 0, nRight = 0,  n;

            RiverNode[] left = new RiverNode[CountEKnots];
            RiverNode[] right = new RiverNode[CountEKnots];
            RiverNode[] elemNodes = new RiverNode[CountEKnots];

            for (i = 0; i < elemNodes.Length; i++)
                elemNodes[i] = new RiverNode();
            for (i = 0; i < left.Length; i++)
            {
                left[i] = new RiverNode();
                right[i] = new RiverNode();
            }
            double[] xl = new double[CountEKnots];
            double[] yl = new double[CountEKnots];
            double[] xTriangle = new double[CountEKnots];
            double[] yTriangle = new double[CountEKnots];
            double level = H_minGroundWater;

            elemNodes[0].X = 1.0;
            elemNodes[0].Y = 0.0;
            elemNodes[1].X = 0.0;
            elemNodes[1].Y = 1.0;
            elemNodes[2].X = 0.0;
            elemNodes[2].Y = 0.0;

            nipts = FindIntersection(currentFElement, elemNodes, ref xl, ref yl, level, ref nRight, ref nLeft, ref right, ref left);
            if (nipts != 2)
            {
                // Console.WriteLine("в элементе {0} не найдено точек пересечения", currentFElement.ID);
                return (0);
            }
            switch (nRight)
            {
                case 1:
                    xTriangle[0] = xl[0];
                    yTriangle[0] = yl[0];
                    xTriangle[1] = right[0].X;
                    yTriangle[1] = right[0].Y;
                    xTriangle[2] = xl[1];
                    yTriangle[2] = yl[1];
                    ngauss = FindGaussPoints(xTriangle, yTriangle, ref pIntegration, ngauss);

                    break;

                case 2:
                    xTriangle[0] = xl[0];
                    yTriangle[0] = yl[0];
                    xTriangle[2] = xl[1];
                    yTriangle[2] = yl[1];

                    n = FindBestNode(xl[0], yl[0], right[0].X, right[0].Y, right[1].X, right[1].Y, xl[1], yl[1]);
                    xTriangle[1] = right[n].X;
                    yTriangle[1] = right[n].Y;

                    ngauss = FindGaussPoints(xTriangle, yTriangle, ref pIntegration, ngauss);
                    
                    // теперь найди второй треугольник
                    xTriangle[1] = right[0].X;
                    xTriangle[2] = right[1].X;
                    yTriangle[1] = right[0].Y;
                    yTriangle[2] = right[1].Y;

                    if (n == 0)
                    {
                        xTriangle[0] = xl[1];
                        yTriangle[0] = yl[1];
                    }
                    else
                    {
                        xTriangle[0] = xl[0];
                        yTriangle[0] = yl[0];
                    }
                    ngauss = FindGaussPoints(xTriangle, yTriangle, ref pIntegration, ngauss);

                    break;

                case 3:
                    //качество треугольников не проверяется для первошо треугольника:
                    
                    xTriangle[0] = xl[1];
                    yTriangle[0] = yl[1];
                    xTriangle[1] = xl[0];
                    yTriangle[1] = yl[0];
                    xTriangle[2] = right[0].X;
                    yTriangle[2] = right[0].Y;

                    ngauss = FindGaussPoints(xTriangle, yTriangle, ref pIntegration, ngauss);

                    /*2nd triangle:*/
                    xTriangle[0] = right[0].X;
                    xTriangle[1] = right[1].X;
                    xTriangle[2] = xl[1];
                    yTriangle[0] = right[0].Y;
                    yTriangle[1] = right[1].Y;
                    yTriangle[2] = yl[1];

                    ngauss = FindGaussPoints(xTriangle, yTriangle, ref pIntegration, ngauss);

                    /*3rd triangle:*/
                    xTriangle[0] = right[1].X;
                    xTriangle[1] = right[2].X;
                    xTriangle[2] = xl[1];
                    yTriangle[0] = right[1].Y;
                    yTriangle[1] = right[2].Y;
                    yTriangle[2] = yl[1];

                    ngauss = FindGaussPoints(xTriangle, yTriangle, ref pIntegration, ngauss);

                    break;
            }
            switch (nLeft)
            {
                case 1:
                    xTriangle[0] = xl[0];
                    yTriangle[0] = yl[0];
                    xTriangle[1] = xl[1];
                    yTriangle[1] = yl[1];
                    xTriangle[2] = left[0].X;
                    yTriangle[2] = left[0].Y;
                    ngauss = FindGaussPoints(xTriangle, yTriangle, ref pIntegration,  ngauss);

                    break;

                case 2:
                    xTriangle[0] = xl[0];
                    yTriangle[0] = yl[0];
                    xTriangle[1] = xl[1];
                    yTriangle[1] = yl[1];

                    n = FindBestNode(xl[0], yl[0], xl[1], yl[1], left[0].X, left[0].Y, left[1].X, left[1].Y);

                    xTriangle[2] = left[n].X;
                    yTriangle[2] = left[n].Y;
                    FindGaussPoints(xTriangle, yTriangle, ref pIntegration, ngauss);
                    ngauss += 3;

                    /*now find the 2nd triangle*/

                    xTriangle[1] = left[0].X;
                    xTriangle[2] = left[1].X;
                    yTriangle[1] = left[0].Y;
                    yTriangle[2] = left[1].Y;

                    xTriangle[0] = xl[n];
                    yTriangle[0] = yl[n];


                    FindGaussPoints(xTriangle, yTriangle, ref pIntegration, ngauss);
                    ngauss += 3;
                    break;

                case 3:
                    /*quality of triangles is not checked
					//first triangle:*/
                    xTriangle[0] = xl[0];
                    yTriangle[0] = yl[0];
                    xTriangle[1] = xl[1];
                    yTriangle[1] = yl[1];
                    xTriangle[2] = left[0].X;
                    yTriangle[2] = left[0].Y;

                    ngauss = FindGaussPoints(xTriangle, yTriangle, ref pIntegration, ngauss);

                    /*2nd triangle:*/
                    xTriangle[0] = left[0].X;
                    xTriangle[1] = left[1].X;
                    xTriangle[2] = xl[0];
                    yTriangle[0] = left[0].Y;
                    yTriangle[1] = left[1].Y;
                    yTriangle[2] = yl[0];

                    ngauss = FindGaussPoints(xTriangle, yTriangle, ref pIntegration, ngauss);

                    /*3rd triangle:*/
                    xTriangle[0] = left[1].X;
                    xTriangle[1] = left[2].X;
                    xTriangle[2] = xl[0];
                    yTriangle[0] = left[1].Y;
                    yTriangle[1] = left[2].Y;
                    yTriangle[2] = yl[0];
                    ngauss = FindGaussPoints(xTriangle, yTriangle, ref pIntegration, ngauss);
                    break;
            }
            return ngauss;
        }
        /// <summary>
        /// Поиск пересечений береговой линии с КЭ
        /// </summary>
        int FindIntersection(TriElementRiver currentFElement, RiverNode[] elemNodes, ref double[] xl, ref double[] yl, 
            double level, ref int nRight, ref int nLeft, ref RiverNode[] right, ref RiverNode[] left)
        {
            // Узлов на КЭ
            int i, ncut, nlast, side, j, nr, nl;
            double mult, factor;
            ncut = 0;
            nr = 0;
            side = 0;
            left[0] = elemNodes[0];
            nl = 1;
            RiverNode[] nodes = { mesh.nodes[currentFElement.Vertex1],
                                  mesh.nodes[currentFElement.Vertex2],
                                  mesh.nodes[currentFElement.Vertex3] };

            for (i = 0; i < CountEKnots; i++)
            {
                
                if (i == (CountEKnots - 1))
                    nlast = 0;
                else
                    nlast = i + 1;


                mult = ((nodes[i].h0 - (nodes[i].h_ise * iceCoeff)) - level) *
                       ((nodes[nlast].h0 - (nodes[nlast].h_ise * iceCoeff)) - level);

                if (mult > 0.0)
                {
                    // нет пересечения
                    if (side == 0)
                    {
                        // мы на левой стороне
                        if (nr == 0)
                        {
                            // это наш первый раз на левой стороне
                            left[nl] = elemNodes[nlast];
                            nl++;
                        }
                        else
                        {
                            // возвращаемся на левую сторону. вставлять
                            if (nlast != 0)
                            {
                                for (j = nl; j > 0; j--)
                                    left[j] = left[j - 1];
                                left[0] = elemNodes[nlast];
                                nl++;
                            }
                        }
                    }
                    else
                    {
                        // мы на правой стороне
                        right[nr] = elemNodes[nlast];
                        nr++;
                    }
                }
                if (mult == 0.0)
                {
                    if ((nodes[i].h0 - (nodes[i].h_ise * iceCoeff)) == level)
                    {
                        xl[ncut] = elemNodes[i].X;
                        yl[ncut] = elemNodes[i].Y;
                        ncut++;

                        side = Math.Abs(1 - side);
                        if (i == 0)
                            nl--;
                        if (nlast != 0)
                        {
                            if (side == 0)
                            {
                                if (nr == 0)
                                {
                                    left[nl] = elemNodes[nlast];
                                    nl++;
                                }
                                else
                                {
                                    if (nlast != 0)
                                    {
                                        for (j = nl; j > 0; j--)
                                            left[j] = left[j - 1];
                                        left[0] = elemNodes[nlast];
                                        nl++;
                                    }
                                }
                            }
                            else
                            {
                                // мы на правой стороне
                                right[nr] = elemNodes[nlast];
                                nr++;
                            }
                        }
                    }
                }
                if (mult < 0.0)
                {
                    double factor1 = Math.Abs((nodes[i].h0 - (nodes[i].h_ise * iceCoeff)) - level);
                    double factor2 = Math.Abs((nodes[nlast].h0 - (nodes[nlast].h_ise * iceCoeff) - level));
                    factor = factor1 / (factor2 + factor1);

                    xl[ncut] = elemNodes[i].X + (elemNodes[nlast].X - elemNodes[i].X) * factor;
                    yl[ncut] = elemNodes[i].Y + (elemNodes[nlast].Y - elemNodes[i].Y) * factor;

                    ncut++;
                    side = Math.Abs(side - 1);
                    if (side == 0)
                    {
                        // мы должны вернуться налево. вставлять
                        if (nlast != 0)
                        {
                            for (j = nl; j > 0; j--)
                                left[j] = left[j - 1];
                            left[0] = elemNodes[nlast];
                            nl++;
                        }
                    }
                    else
                    {
                        // мы перешли слева направо
                        right[nr] = elemNodes[nlast];
                        nr++;
                    }
                }
            }
            nRight = nr;
            nLeft = nl;
            return ncut;
        }
        /// <summary>
        /// Определение точек интегрирования для полузатопленного КЭ, метод вызывается несколько раз
        /// </summary>
        /// <param name="xTriangle"></param>
        /// <param name="yTriangle"></param>
        /// <param name="pIntegration">точки интегрирования</param>
        /// <param name="ngauss">количество уже найденных точек интегрирования </param>
        /// <returns></returns>
        int FindGaussPoints(double[] xTriangle, double[] yTriangle, ref NumInegrationPoints pIntegration, int ngauss)
        {
            double weight;
            double[][] fEtaXi = gaussPoints.fEtaXi;
            weight = GetTriArea(xTriangle[0], yTriangle[0], xTriangle[1], yTriangle[1], xTriangle[2], yTriangle[2]) / 3.0;
            pIntegration.ReSize(ngauss + 3);
            for (int i = ngauss; i < (ngauss + 3); i++)
            {
                pIntegration.xi[i] = xTriangle[0] * fEtaXi[0][i - ngauss] + xTriangle[1] * fEtaXi[1][i - ngauss] + xTriangle[2] * fEtaXi[2][i - ngauss];
                pIntegration.eta[i] = yTriangle[0] * fEtaXi[0][i - ngauss] + yTriangle[1] * fEtaXi[1][i - ngauss] + yTriangle[2] * fEtaXi[2][i - ngauss];
                pIntegration.weight[i] = weight;
            }
            return (ngauss + 3);
        }
        /// <summary>
        /// площадь треугольника 
        /// </summary>
        double GetTriArea(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            double ar = 0.5 * ((x2 - x1) * (y3 - y1) - (y2 - y1) * (x3 - x1));
            return (ar);
        }
        /// <summary>
        /// Длина отрезка
        /// </summary>
        double GetLength(double x1, double y1, double x2, double y2)
        {
            double d;
            d = Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
            return (d);
        }

        int FindBestNode(double xa, double xb, double ya, double yb, double x0, double y0, double x1, double y1)
        {
            double LengthAB = GetLength(xa, ya, xb, yb);
            double alpha0 = GetTriArea(xa, xb, ya, yb, x0, y0) / ( LengthAB + GetLength(xb, yb, x0, y0) + GetLength(x0, y0, xa, ya));
            double alpha1 = GetTriArea(xa, xb, ya, yb, x1, y1) / (LengthAB + GetLength(xb, yb, x1, y1) + GetLength(x1, y1, xa, ya));
            if (alpha0 > alpha1)
                return 0;
            else
                return 1;
        }
        /// <summary>
        /// функция для установки старых значений(uo, uoo)
        /// </summary>
        public void RollbackToOldValues()
        {
            for (int i = 0; i < mesh.CountKnots; i++)
            {
                RiverNode elemNodes = mesh.nodes[i];
                // порядок сохранения глубины и расходов различный
                elemNodes.h00 = elemNodes.h0;// h
                elemNodes.h0 = elemNodes.h;  // h
                elemNodes.qx00 = elemNodes.qx0;// qx
                elemNodes.qx0 = elemNodes.qx;  // qx
                elemNodes.qy00 = elemNodes.qy0;// qy
                elemNodes.qy0 = elemNodes.qy;  // qy
            }
        }
        /// <summary>
        /// Создает экземпляр класса конвертера для 
        /// загрузки и сохранения задачи не в бинарном формате
        /// </summary>
        /// <returns></returns>
        public IOFormater<IRiver> GetFormater()
        {
            return new RiverFormatReader2DCdg();
        }
        /// <summary>
        /// Тест
        /// </summary>
        public static void Test()
        {
            string filename = "Spin400gr.cdg";
            //string filename = "Soni-steady.cdg";
            IRiver river2D = new River2D();
            // river2D.Load(filename);
            IOFormater<IRiver> loader = new RiverFormatReader2DCdg();
            loader.Read(filename, ref river2D);
            river2D.SolverStep();
        }
        public static void Main()
        {
            River2D.Test();
        }

        #region
        public double GetWsElev(ref BoundElementRiver inSegP, ref BoundElementRiver outSegP)
        {
            int countOut = 0;
            double elOut = 0.0;
            for(int i = 0; i < mesh.CountBoundElements; i++)
            {
                BoundElementRiver elem = mesh.BoundElems[i];
                if ((elem.boundCondType == 3) || (elem.boundCondType == 5))
                {
                    outSegP = elem;
                    countOut += 1;
                    elOut += elem.Qn;
                }
                else if (elem.boundCondType == 1)
                {
                    inSegP = elem;
                }
            }
            if (countOut > 0)
                return elOut / countOut;
            else
                return 0.0;
        }
        /// <summary>
        /// функция для поиска соседнего элемента для каждого элемента
        /// </summary>
        /// <returns></returns>
        public int find_neighbour()
        {
            TriElementRiver element;
            BoundElementRiver boundElement;
            int i, j, k, l, found, n;

            int CountBoundElemNods = 2;
            int CountElemNods = 3;
            //boundElement = gp.BoundElements;
            for (l = 0; l < mesh.CountBoundElements; l++)
            {
                boundElement = mesh.BoundElems[l];
                found = 0;
                int idx_e = 0;
                for (i = 0; i < mesh.CountElements; i++)
                {
                    element = mesh.AreaElems[idx_e];
                    for (k = 0; k < CountElemNods; k++)
                    {
                        if (boundElement.Vertex1 == element.nodes[k])
                        {
                            found = 1;
                            for (j = 1; j < CountBoundElemNods; j++)
                            {
                                if ((k + j) > 2)
                                    n = k + j - 3;
                                else
                                    n = k + j;
                                if (boundElement.nodes[j] != element.nodes[n])
                                {
                                    found = 0;
                                    break;
                                }
                            }
                            if (found == 1)
                            {
                                boundElement.elementID = i;
                                break;
                            }
                        }
                    }
                    if (found == 1)
                        break;
                    else
                        idx_e++;
                }
                if (found == 0)
                {
                    Console.WriteLine("Ошибка, соседа для ГКЭ # {0} не найдено", boundElement.ID);
                    return (-1);
                }
            }
            //for (i = 0; i < mesh.CountBoundElements; i++)
            //{
            //    boundElement = mesh.BoundElems[l];
            //    Console.WriteLine("belement = {0} telement={1}", boundElement.ID, boundElement.elementID);
            //}
            return (0);
        }

        /// <summary>
        /// функция для поиска соседнего элемента для каждого элемента
        /// </summary>
        /// <returns></returns>
        public int FindNeighbour()
        {
            TriElementRiver element;
            BoundElementRiver boundElement;
            int i, j, k, l, found, n;

            int CountBoundElemNods = 2;
            int CountElemNods =3;
            //boundElement = gp.BoundElements;
            for (l = 0; l < mesh.CountBoundElements; l++)
            {
                boundElement = mesh.BoundElems[l];
                found = 0;
                int idx_e = 0;
                for (i = 0; i < mesh.CountElements; i++)
                {
                    element = mesh.AreaElems[idx_e];
                    for (k = 0; k < CountElemNods; k++)
                    {
                        if (boundElement.Vertex1 == element.nodes[k])
                        {
                            found = 1;
                            for (j = 1; j < CountBoundElemNods; j++)
                            {
                                if ((k + j) > 2)
                                    n = k + j - 3;
                                else
                                    n = k + j;
                                if (boundElement.nodes[j] != element.nodes[n])
                                {
                                    found = 0;
                                    break;
                                }
                            }
                            if (found == 1)
                            {
                                boundElement.elementID = i;
                                break;
                            }
                        }
                    }
                    if (found == 1)
                        break;
                    else
                        idx_e++;
                }
                if (found == 0)
                {
                    Console.WriteLine("Ошибка, соседа для ГКЭ # {0} не найдено", boundElement.ID);
                    return (-1);
                }
            }
            //for (i = 0; i < mesh.CountBoundElements; i++)
            //{
            //    boundElement = mesh.BoundElems[l];
            //    Console.WriteLine("belement = {0} telement={1}", boundElement.ID, boundElement.elementID);
            //}
            return (0);
        }

        #endregion

    }
}
