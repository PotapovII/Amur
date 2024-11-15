//---------------------------------------------------------------------------
//                    Разработка кода : Снигур К.С.
//                         релиз  от 26 06 2020
//---------------------------------------------------------------------------
//        первичная интеграция в RiverLib 15-17 08 2022 : Потапов И.И.
//---------------------------------------------------------------------------
//                      Сбока No Property River Lib
//              с убранными из наследников Property классами 
//                              23 08 2024
//---------------------------------------------------------------------------
namespace NPRiverLib.APRiver1XD.KGD_River2D
{
    using System;
    using System.IO;
    using System.Linq;
    using System.ComponentModel;
    using System.Threading.Tasks;
    using System.Collections.Concurrent;

    using Cloo;

    using MeshLib;
    using MemLogLib;
    using CommonLib;
    using CommonLib.IO;

    using AlgebraLib;
    using GeometryLib;
    using MeshGeneratorsLib;
    using CommonLib.EConverter;
    using CommonLib.ChannelProcess;
    using CommonLib.Physics;
    using CommonLib.Geometry;
    using CommonLib.Delegate;
    using NPRiverLib.APRiver_1XD;
    using NPRiverLib.IO;
    using NPRiverLib.ATask;

    /// <summary>
    /// ОО: Решение задачи гидрадинамики МКО в формулировке Елизароваой Е.
    /// реализованы модели турбулентности k-e и k-w c с различными функциями стенки
    /// </summary>
    [Serializable]
    public class KGD_Eliz2024_1XD : APRiver1XD<RGDParameters1XD>, IRiver
    {
        #region Константы моделей k-e и k-w
        public const double sigma_s = 1;
        public const double sigma_z = 3.0 / 5.0;
        public const double alpha = 13.0 / 25.0;
        public const double beta = 0.0708;
        public const double beta_z = 0.09;
        public const double sigma_d_0 = 1.0 / 8.0;
        public const double sigma_k = 1;
        public const double sigma_e = 1.3;
        public const double C_e1 = 1.44;
        public const double C_e2 = 1.92;
        public const double C_m = 0.09;
        public const double y_p_0 = 11.5;
        public const double sigma = 0.5;
        public const double g = 9.81;
        public const double cm14 = 0.54772255750516607;
        #endregion
        /// <summary>
        /// функция стенки
        /// </summary>
        protected WallFunction WFunc = null;
        /// <summary>
        /// Модель турбулентности
        /// </summary>
        protected TurbulentModel TurbModel = null;
        /// <summary>
        /// Метод расчета придонных напряжений
        /// </summary>
        protected SimpleProcedure CalkBottomTauXY = null;
        /// <summary>
        /// КЭ сетка для задачи
        /// </summary>
        protected KsiMesh mesh = new KsiMesh();
        //
        protected KsiWrapper wrapper = null;
        // k-e
        protected IUnknown[] unknownsKE = {   new Unknown("Осредненная скорость х", null, TypeFunForm.Form_2D_Triangle_L1),
                                              new Unknown("Осредненная скорость у", null, TypeFunForm.Form_2D_Triangle_L1),
                                              new Unknown("Давление ", null, TypeFunForm.Form_2D_Triangle_L1),
                                              new Unknown("Кинетичесая энергия турбулентности", null, TypeFunForm.Form_2D_Triangle_L1),
                                              new Unknown("Диссипация кинетической энергии турбулентности", null, TypeFunForm.Form_2D_Triangle_L1) };
        // k-w
        protected IUnknown[] unknownsKW = { new Unknown("Осредненная скорость х", null, TypeFunForm.Form_2D_Triangle_L1),
                                            new Unknown("Осредненная скорость у", null, TypeFunForm.Form_2D_Triangle_L1),
                                            new Unknown("Давление ", null, TypeFunForm.Form_2D_Triangle_L1),
                                            new Unknown("Кинетичесая энергия турбулентности", null, TypeFunForm.Form_2D_Triangle_L1),
                                            new Unknown("Частота затухания кинетической энергии турбулентности", null, TypeFunForm.Form_2D_Triangle_L1) };
        // Newton
        protected IUnknown[] unknownsNT = { new Unknown("Осредненная скорость х", null, TypeFunForm.Form_2D_Triangle_L1),
                                            new Unknown("Осредненная скорость у", null, TypeFunForm.Form_2D_Triangle_L1),
                                            new Unknown("Давление ", null, TypeFunForm.Form_2D_Triangle_L1) };
        /// <summary>
        /// Количество узлов в области
        /// </summary>
        protected int CountKnots;
        /// <summary>
        /// Количество элементов в области
        /// </summary>
        protected int CountElements;
        /// <summary>
        /// Координаты узлов Х 
        /// </summary>
        protected double[] X = null;
        /// <summary>
        /// Координаты узлов Y
        /// </summary>
        protected double[] Y = null;
        /// <summary>
        /// Расчет функции стенки
        /// </summary>
        [NonSerialized]
        protected YPLUS YPlus = new YPLUS();

        #region Искомые величины и объекты для работы
        public string status = "Running";
        public string err = "ok";
        public int Iter = 0;
        protected int beginIter = 0;
        /// <summary>
        /// Скорость по Х
        /// </summary>
        public double[] U = null;
        protected double[] buffU = null;
        /// <summary>
        /// Скорость по У
        /// </summary>
        public double[] V = null;
        protected double[] buffV = null;
        /// <summary>
        /// Давление
        /// </summary>
        public double[] P = null;
        protected double[] buffP = null;
        /// <summary>
        /// поправка давления
        /// </summary>
        public double[] ErrP = null;
        /// <summary>
        /// Концентрация наносов
        /// </summary>
        public double[] S = null;
        protected double[] buffS = null;
        /// <summary>
        /// кинетическая энергия турбулентности
        /// </summary>
        public double[] K = null;
        protected double[] buffK = null;
        public double[] rightK = null;
        /// <summary>
        /// диссипация энергии турбулентности
        /// </summary>
        public double[] E = null;
        protected double[] buffE = null;
        public double[] rightE = null;
        /// <summary>
        /// частота диссипации кинетической энергии турбулентности
        /// </summary>
        public double[] W = null;
        protected double[] buffW = null;
        public double[] rightW = null;
        /// <summary>
        /// турбулентная вязкость
        /// </summary>
        public double[] nuT = null;
        protected double[] buffNu = null;
        /// <summary>
        /// напряжение на дне в полуцелый узлах
        /// </summary>
        public double[] BTau = null;
        /// <summary>
        /// напряжение на дне в целых узлах
        /// </summary>
        public double[] BTauC = null;
        public double[] arg = null;//аргумент к BTau для сплайна
        /// <summary>
        /// напряжение на верхней стенке в полуцелых узлах
        /// </summary>
        public double[] TTau = null;
        /// <summary>
        ///  напряжение на верхней стенке в целых узлах
        /// </summary>
        public double[] TTauC = null;
        public double[] argT = null;

        protected double[] BV2 = null;
        protected double[] ReT = null;
        /// <summary>
        /// максимальная горизонтальная скорость
        /// </summary>
        double U_max = 0;
        //для контроля потери массы
        //перемнные для баланса массы на входе и выходе
        double Q_in = 0;
        double Q_out = 0;
        double SinJ = 0, CosJ = 0;
        /// <summary>
        /// ширина ленты
        /// </summary>
        public int BWidth = 0;
        /// <summary>
        /// Напряжение по пристеночной функции в пристеночных узлах
        /// </summary>
        double[] CV_WallTau = null;
       
        #endregion
        /// <summary>
        /// значение средней скорости по суммированию профиля
        /// </summary>
        public double Ucp = 0;
        public double dPdx = 0;
        double nu_mol = 0;
        double dt = 0.0001f;
        double Ww = 0.01;
        double rho_s = 2650;

        double d = 0.00016;
        double dy = 0;
        double[] RBC = null;
        double[] BV = null;
        double[] R = null;
        bool flag = true;
        double[] dudy;
        //----Тест        ///
        public double[] p_conv, p_kinet;
        public double[] u_press, u_kinet, u_cont;

        double kappa = SPhysics.kappa_w;

        [NonSerialized]
        public double[] Tau = null;
        [NonSerialized]
        public double[] Pk = null;
        [NonSerialized]
        public double[] y_plus;

        #region Упорядочиваемые разделители, на основе входного списка. 
        /// <summary>
        ///  Используется для балансировки нагрузки на разные потоки при параллельной обработки данных
        /// </summary>
        [NonSerialized]
        OrderablePartitioner<Tuple<int, int>> OrdPartCountKnots;
        [NonSerialized]
        OrderablePartitioner<Tuple<int, int>> OrdPart_CV;
        [NonSerialized]
        OrderablePartitioner<Tuple<int, int>> OrdPart_CV2;
        [NonSerialized]
        OrderablePartitioner<Tuple<int, int>> OrdPart_CV_Wall;
        [NonSerialized]
        OrderablePartitioner<Tuple<int, int>> OrdPartCountElems;
        #endregion
        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public KGD_Eliz2024_1XD() : this(new RGDParameters1XD()) 
        { 
            //wrapper= new KsiWrapper(mesh);
            //wrapper.TriangleGeometryCalculation(true);
            //wrapper.MakeWallFuncStructure(true);
        }
        /// <summary>
        /// Конструктор c заданными параметрами
        /// </summary>
        public KGD_Eliz2024_1XD(RGDParameters1XD p) : base(p)
        {
            name = "КГД турбулентнй поток 2D (k-e/w)";
            Version = "River2D KGD ke/w 23.08.2024";
        }
        /// <summary>
        ///  Настройка решателя, через делегаты , дабы не порождать
        ///  больших иерархий наследования и переопределения
        /// </summary>
        protected void SetModels()
        {
            
            // функции стенки
            switch (Params.wallFunctionType)
            {
                case WallFunctionType.roughWall_Lutsk:
                    WFunc = WallFuncSharpPlus;
                    break;
                case WallFunctionType.smoothWall_Snegirev:
                    WFunc = WallFuncSnegirev;
                    break;
                case WallFunctionType.smoothWall_Lutsk:
                    WFunc = WallFuncPlus;
                    break;
                case WallFunctionType.smoothWall_Volkov:
                    WFunc = WallFunc;
                    break;
                    //case WallFunctionType.smoothWall_VolkovNewton:
                    //    WFunc = WallFuncNewton;
                    //    break;
            }
            // модель турбулентности
            switch(Params.turbulentModelType)
            {
                case TurbulentModelType.Model_k_e: // Расчет K, epsilon
                    TurbModel = QHDKE;
                    unknowns.AddRange(unknownsKE);
                    break;
                case TurbulentModelType.Model_k_w: // Расчет k, omega
                    TurbModel = QHDKW;
                    unknowns.AddRange(unknownsKW);
                    break;
                case TurbulentModelType.Model_k_eG: // Расчет k-e вместе с пристеночной функцией по Жлуткову 2015
                    TurbModel = QHDKE_G;
                    unknowns.AddRange(unknownsKE);
                    break;
                case TurbulentModelType.Model_Newton: // Расчет гидрадинамики с постоянной вязкостью
                    TurbModel = QHD_Newton;
                    unknowns.AddRange(unknownsNT);
                    break;
            }
            // напряжения на дне
            switch(Params.IndexMethod)
            {
                case ModeTauCalklType.method0:
                    CalkBottomTauXY = CalkTau0;
                    break;
                case ModeTauCalklType.method1:
                    CalkBottomTauXY = CalkTau1;
                    break;
                case ModeTauCalklType.method2:
                    CalkBottomTauXY = CalkTau2;
                    break;
            }
            YPlus = new YPLUS();

            //if (mesh != null && algebra != null)
            //    Set(mesh, algebra);
        }
        /// ----------------------------------------------------------------------
        #region IRiver

        /// <summary>
        /// Имена файлов с данными для задачи гидродинамики при загрузке
        /// по умолчанию
        /// </summary>
        public override ITaskFileNames taskFileNemes()
        {
            ITaskFileNames fn = new TaskFileNames(GetFormater());
            fn.NameCPParams = "NameCPParams.txt";
            fn.NameBLParams = "NameBLParams.txt";
            fn.NameRSParams = "ElzNameParams.txt";
            fn.NameRData = "ElzNameParams.txt";
            fn.NameEXT = "(*.tsk)|*.tsk|";
            return fn;
        }
        /// <summary>
        /// флаг наала задачи
        /// </summary>
        public bool start = true;
        /// <summary>
        /// установка параметров
        /// </summary>
        /// <param name="p"></param>
        public new void SetParams(object p)
        {
            base.SetParams((RGDParameters1XD)p);
            SetModels();
        }
        /// <summary>
        ///  Сетка для расчета донных деформаций
        /// </summary>
        public override IMesh BedMesh()
        {
            double[] zX = null, zY = null;
            MEM.Alloc(mesh.CountBottom, ref zX);
            MEM.Alloc(mesh.CountBottom, ref zY);
            for (int i = 0; i < mesh.CountBottom; i++)
            {
                int id = mesh.BottomKnots[i];
                zX[i] = mesh.CoordsX[id];
                zY[i] = mesh.CoordsY[id];
            }
            return new TwoMesh(zX, zY);
        }
        /// <summary>
        /// Установка новых отметок дна и генерация новой КЭ сетки
        /// </summary>
        /// <param name="zeta"></param>
        public override void SetZeta(double[] zeta, EBedErosion bedErosion)
        {
            Erosion = bedErosion;
            if (bedErosion != EBedErosion.NoBedErosion)
            {
                mesh = GetKsiMesh();
                wrapper = GetKsiWrapper(mesh);
            }
        }
        /// <summary>
        /// Герератор заглушка на период отладки гидродинамики
        /// </summary>
        /// <returns></returns>
        protected KsiMesh GetKsiMesh()
        {
            double Lx = 10;
            double Ly = 1;
            TypeBoundCond[] TypeBC = new TypeBoundCond[4]
            {
                    TypeBoundCond.Dirichlet,
                    TypeBoundCond.Dirichlet,
                    TypeBoundCond.Neumann,
                    TypeBoundCond.Dirichlet
            };
            int[] MarkBC = { 0, 1, 2, 3 };
            double diametrFE = 0.05;
            int Nx = (int)(Lx / diametrFE);
            int Ny = (int)(Ly / diametrFE);
            // meshData
            TypeMesh meshType = TypeMesh.Triangle;
            MeshCore cmesh = SimpleMeshGenerator.GetTetrangleMesh(meshType, Nx, Ny, Lx, Ly, MarkBC, TypeBC);
            ComplecsMesh fm = (ComplecsMesh)cmesh;
            mesh = new KsiMesh(fm);
            return mesh;
        }
        //
        protected KsiWrapper GetKsiWrapper(KsiMesh mesh)
        {
           wrapper = new KsiWrapper(mesh);
           wrapper.TriangleGeometryCalculation();
           wrapper.MakeWallFuncStructure(Params.surf_flag);
           return wrapper;

        }
        #region методы предстартовой подготовки задачи
        /// <summary>
        /// Чтение входных данных задачи из файла
        /// геометрия канала по умолчанию
        /// эволюция расходов/уровней
        /// </summary>
        /// <param name="p"></param>
        public bool LoadData(string fileName)
        {
            SinJ = SinJ;
            CosJ = CosJ;
            string message = "Файл данных задачи не обнаружен";
            return WR.LoadParams(LoadData, message, fileName);
        }
        /// <summary>
        /// Чтение данных задачи из файла
        /// </summary>
        /// <param name="file"></param>
        public override void LoadData(StreamReader file)
        {
            SinJ = SinJ;
            CosJ = CosJ;
            //// инициализация задачи
            //// краевые условия
            //flowRate.Load(file);
            //// геометрия геометрия области
            //Geometry.Load(file);
            // генерация сетки в области пока заглушка !!!!!!!!!!!!!!!!!!!
            mesh = GetKsiMesh();
            Set(mesh, null);
            wrapper = GetKsiWrapper(mesh);
        }
        /// <summary>
        /// Конфигурация задачи по умолчанию (тестовые задачи)
        /// </summary>
        /// <param name="testTaskID">номер задачи по умолчанию</param>
        public override void DefaultCalculationDomain(uint testTaskID = 0)
        {
            mesh = GetKsiMesh();
            wrapper = GetKsiWrapper(mesh);
            SinJ = Math.Sin(Params.J);
            CosJ = Math.Cos(Params.J);
            //// инициализация задачи

            CountKnots = mesh.CountKnots;
            CountElements = mesh.CountElements;
            X = mesh.CoordsX;
            Y = mesh.CoordsY;
            BWidth = (int)mesh.GetWidthMatrix();

            IAlgebra a = new AlgebraGaussTape((uint)CountKnots, (uint)BWidth);
            Set(mesh, a);

            // выделение памяти под рабочии массивы
            MEM.Alloc(CountKnots, ref U, "U InitMassives");
            MEM.Alloc(CountKnots, ref V, "V InitMassives");
            MEM.Alloc(CountKnots, ref P, "P InitMassives");
            MEM.Alloc(CountKnots, ref ErrP, "ErrP InitMassives");

            MEM.Alloc(CountKnots, ref K, "K InitMassives");
            MEM.Alloc(CountKnots, ref E, "E InitMassives");
            MEM.Alloc(CountKnots, ref W, "W InitMassives");
            MEM.Alloc(CountKnots, ref S, "S InitMassives");

            MEM.Alloc(CountKnots, ref nuT, "nuT InitMassives");
            MEM.Alloc(CountKnots, ref rightK, "rightK InitMassives");
            MEM.Alloc(CountKnots, ref rightE, "rightE InitMassives");
            MEM.Alloc(CountKnots, ref rightW, "rightW InitMassives");

            MEM.Alloc(CountKnots, ref ReT, "ReT InitMassives");
            MEM.Alloc(CountKnots, ref BTauC, "BTauC InitMassives");
            MEM.Alloc(CountKnots, ref BTau, "BTau InitMassives");

            MEM.Alloc(CountKnots, ref dudy, "dudy InitMassives");
            MEM.Alloc(CountKnots, ref TTauC, "TTauC InitMassives");
            MEM.Alloc(CountKnots, ref TTau, "TTau InitMassives");
            MEM.Alloc(wrapper.CV_WallKnots.Length, ref CV_WallTau, "CV_WallTau InitMassives");
            MEM.Alloc(CountKnots, ref Tau, "Tau InitMassives");

            // активация разделителей
            OrdPartCountKnots = Partitioner.Create(0, mesh.CountKnots);
            OrdPartCountElems = Partitioner.Create(0, mesh.CountElements);
            OrdPart_CV = Partitioner.Create(0, wrapper.CVolumes.Length);
            OrdPart_CV2 = Partitioner.Create(0, wrapper.CV2.Length);
            OrdPart_CV_Wall = Partitioner.Create(0, wrapper.CV_WallKnots.Length);

            // определение граничных условий
            InitialStartConditions();
            start = false;
           
            eTaskReady = ETaskStatus.TaskReady;
        }

        #endregion
        /// <summary>
        /// Получить отметки дна
        /// </summary>
        /// <param name="zeta"></param>
        public override void GetZeta(ref double[] zeta)
        {
            MEM.Alloc(mesh.CountBottom, ref zeta);
            for (int i = 0; i < mesh.CountBottom; i++)
            {
                int id = mesh.BottomKnots[i];
                zeta[i] = mesh.CoordsY[id];
            }
        }
        /// <summary>
        /// Формирование данных для отрисовки связанных с сеткой IMesh со сороны задачи
        /// </summary>
        /// <param name="sp">контейнер данных</param>
        public override void AddMeshPolesForGraphics(ISavePoint sp)
        {
            sp.Add("Давление", P);
            sp.Add("Поправка давления", ErrP);
            sp.Add("Скорость U", U);
            sp.Add("Скорость V", V);
            sp.Add("Поле скорости", U, V);
            switch(Params.turbulentModelType)
            {
                case TurbulentModelType.Model_k_e:
                case TurbulentModelType.Model_k_eG:
                    sp.Add("Кинетическая энергия т.", K);
                    sp.Add("Диссипация т.э.", E);
                    sp.Add("Турб. вязкость", nuT);
                    break;
                case TurbulentModelType.Model_k_w:
                    sp.Add("Кинетическая энергия т.", K);
                    sp.Add("Чавстота т.э.", W);
                    sp.Add("Турб. вязкость", nuT);
                    break;
            }
            double[] tauXY = CalcTauEverywhere();
            sp.Add("Напряжение сдвига", tauXY);
        }
        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public override IRiver Clone()
        {
            return new KGD_Eliz2024_1XD(Params);
        }
        /// <summary>
        /// Создает экземпляр класса конвертера
        /// </summary>
        /// <returns></returns>
        public override IOFormater<IRiver> GetFormater()
        {
            return new TaskReaderKGD_1XD();
        }
        /// <summary>
        /// Получение полей придонных касательных напряжений и давления/свободной поверхности по контексту задачи
        /// усредненных на конечных элементах
        /// </summary>
        /// <param name="tauX">придонное касательное напряжение по х</param>
        /// <param name="tauY">придонное касательное напряжение по у</param>
        /// <param name="P">придонных давления/свободная поверхности потока, по контексту задачи</param>
        public override void GetTau(ref double[] tauX, ref double[] tauY, ref double[] Press, ref double[][] CS, StressesFlag sf = StressesFlag.Nod)
        {
            tauX = null;
            tauY = null;
            Press = null;
            Object lockThis = new Object();
            try
            {
                CalkBottomTauXY();
            }
            catch (Exception e)
            {
                err = e.Message + "ShearStressesCalculation";
            }
            MEM.MemCopy(ref tauX, BTau);
        }

        protected void CalkTau0()
        {
            // через ленту tau_l
            //!!!-- 1 ый порядок точности, для Tau в целых и полуцелых узлах надо использовать через сплайн!
            {
                //
                double[] tau_mid = new double[wrapper.BTriangles.Length];
                OrderablePartitioner<Tuple<int, int>> OrdPartitioner_Tau = Partitioner.Create(0, wrapper.BTriangles.Length);
                Parallel.ForEach(OrdPartitioner_Tau, (range, loopState) =>
                {
                    for (int i = range.Item1; i < range.Item2; i++)
                           //for (int i = 0; i < mesh.BTriangles.Length; i++)
                    {

                        double[] xL = new double[3];
                        double[] yL = new double[3];
                               //
                        uint LcurV = wrapper.BTriangles[i];
                        uint[] Knots = mesh.AreaElems[LcurV];
                               //
                        uint Lnum1 = Knots[0];
                        uint Lnum2 = Knots[1];
                        uint Lnum3 = Knots[2];
                               // получаем координаты узлов элемента
                        xL[0] = X[Lnum1]; xL[1] = X[Lnum2]; xL[2] = X[Lnum3];
                        yL[0] = Y[Lnum1]; yL[1] = Y[Lnum2]; yL[2] = Y[Lnum3];
                               // нахождение площади треугольника
                        double LS = wrapper.Sk[LcurV];
                               // скорости в вершинах треугольника
                        double LU1 = U[Lnum1];
                        double LU2 = U[Lnum2];
                        double LU3 = U[Lnum3];
                               //
                        double LV1 = V[Lnum1];
                        double LV2 = V[Lnum2];
                        double LV3 = V[Lnum3];
                               // касательный вектор (обход против часовой стрелки)
                        double Lsx = wrapper.Sx[i];
                        double Lsy = wrapper.Sy[i];
                               // нормаль (направлена во внутрь КО)
                        double Lnx = -Lsy;
                        double Lny = Lsx;
                               // производные в центре треугольника
                        double Ldu_dx = 1.0f / 2.0f / LS * (LU1 * (yL[1] - yL[2]) + LU2 * (yL[2] - yL[0]) + LU3 * (yL[0] - yL[1]));
                        double Ldu_dy = 1.0f / 2.0f / LS * (LU1 * (xL[2] - xL[1]) + LU2 * (xL[0] - xL[2]) + LU3 * (xL[1] - xL[0]));
                        double Ldv_dx = 1.0f / 2.0f / LS * (LV1 * (yL[1] - yL[2]) + LV2 * (yL[2] - yL[0]) + LV3 * (yL[0] - yL[1]));
                        double Ldv_dy = 1.0f / 2.0f / LS * (LV1 * (xL[2] - xL[1]) + LV2 * (xL[0] - xL[2]) + LV3 * (xL[1] - xL[0]));
                               // давление в центре треугольника
                        double LP_mid = (P[Lnum1] + P[Lnum2] + P[Lnum3]) / 3.0f;
                        double Mu_mid = (nuT[Lnum1] + nuT[Lnum2] + nuT[Lnum3]) / 3.0f * rho_w;
                               // составляющие тензора напряжений
                        double Tx1 = -LP_mid + 2.0f * Mu_mid * Ldu_dx;
                        double Tx2 = Mu_mid * (Ldu_dy + Ldv_dx);
                        double Ty1 = Mu_mid * (Ldu_dy + Ldv_dx);
                        double Ty2 = -LP_mid + 2.0f * Mu_mid * Ldv_dy;
                               // компоненты вектора придонного напряжения
                        double LsigX = Tx1 * Lnx + Tx2 * Lny;
                        double LsigY = Ty1 * Lnx + Ty2 * Lny;
                               //
                        tau_mid[i] = (LsigX * Lsx + LsigY * Lsy);
                    }
                });

                // Вычисление tau в узлах сетки по сглаженной методике вычисления 
                double[] Tau_all = Aproximate(tau_mid, wrapper.CBottom, wrapper.BTriangles);
                //
                //подготовка приграничных значений tau между граничными точками и координат для сплайна
                int count = mesh.BottomKnots.Length;
                BTau = new double[count];
                arg = new double[count];
                int cKnot;
                //
                for (int i = 0; i < mesh.BottomKnots.Length; i++)
                {
                    cKnot = mesh.BottomKnots[i];
                    for (int j = 0; j < wrapper.CBottom.Length; j++)
                    {
                        if (cKnot == wrapper.CBottom[j])
                        {

                            BTau[i] = Tau_all[j];
                            arg[i] = X[cKnot]; // аргумент - только X (чтобы легче проецировать на дно)
                                               //arg[i] = Math.Sqrt(X[cKnot] * X[cKnot] + Y[cKnot] * Y[cKnot]);
                            break;
                        }
                    }
                }
            }
        }
        protected void CalkTau1()
        {

            // через конечные разности -  для сильновырожденной области не годится
            //!!!-- 1 ый порядок точности, для Tau в целых и полуцелых узлах надо использовать через сплайн!
            {
                //подготовка приграничных значений tau между граничными точками и координат для сплайна
                arg = new double[mesh.BottomKnots.Length];
                //
                OrderablePartitioner<Tuple<int, int>> OrdPartitioner_Tau = Partitioner.Create(1, mesh.BottomKnots.Length - 1);
                Parallel.ForEach(OrdPartitioner_Tau, (range, loopState) =>
                {
                    for (int i = range.Item1; i < range.Item2; i++)
                            //for (int i = 1; i < mesh.BottomKnots.Length - 1; i++)
                    {
                                //
                                //           nnK(2)
                                //            |
                                //           nK(1)
                                //            |
                                //--wK(3)---cKnot(0)---eK(4)
                        int cKnot = mesh.BottomKnots[i];
                        int wK = mesh.BottomKnots[i - 1];
                        int eK = mesh.BottomKnots[i + 1];
                        int nK = 0, nnK = 0;
                        double delx = (X[eK] - X[wK]);
                        double dely = (Y[eK] - Y[wK]);
                        double ss = (double)(Math.Sqrt(delx * delx + dely * dely));
                                // компоненты касательной между граничными точками
                        double lx = 1 / ss * delx;
                        double ly = 1 / ss * dely;
                                //нормаль к поверхности между узлами wK и eK
                        double nx = -ly;
                        double ny = lx;
                                //
                        double Ldu_dx = 0;
                        double Ldu_dy = 0;
                        double Ldv_dx = 0;
                        double Ldv_dy = 0;
                        double P_c = 0;
                                //если дно почти горизонтальное, то считаем, что оно горизонтальное
                        if (lx > 0.98)
                        {
                            nK = cKnot + 1;
                            nnK = cKnot + 2;
                                    //x - это y(координата), y - это U (функция)
                            double x0 = Y[cKnot];
                            double y0 = U[cKnot];
                            double x1 = Y[nK];
                            double y1 = U[nK];
                            double x2 = Y[nnK];
                            double y2 = U[nnK];
                            Ldu_dy = 1.0f / (x0 - x1) * y0 + 1.0f / (x0 - x2) * y0 + 1.0f / (x1 - x0) * (x0 - x2) / (x1 - x2) * y1 + 1.0f / (x2 - x0) * (x0 - x1) / (x2 - x1) * y2;
                                    //функция V - это y
                            y0 = V[cKnot];
                            y1 = V[nK];
                            y2 = V[nnK];
                            Ldv_dy = 1.0f / (x0 - x1) * y0 + 1.0f / (x0 - x2) * y0 + 1.0f / (x1 - x0) * (x0 - x2) / (x1 - x2) * y1 + 1.0f / (x2 - x0) * (x0 - x1) / (x2 - x1) * y2;
                            Ldv_dx = 0;
                            Ldu_dx = 0;
                        }
                        else
                        {
                            // если дно не горизонтальное
                            double k = dy;
                            HPoint[] PointsNorm = new HPoint[2];
                            HPoint[] PointsTang = new HPoint[2];
                            double[] U12 = new double[2]; double[] V12 = new double[2];
                            double[] U34 = new double[2]; double[] V34 = new double[2];
                                    //
                            for (int j = 0; j < 2; j++)
                            {
                                        //точка на расстоянии k по направлению нормали
                                PointsNorm[j] = new HPoint(X[cKnot] + nx * k * (j + 1), Y[cKnot] + ny * k * (j + 1));
                                        //
                                int Triangle = mesh.GetTriangle(PointsNorm[j].x, PointsNorm[j].y);
                                        //
                                double s05 = 1.0f / 2.0f / wrapper.Sk[Triangle];
                                uint[] Knots = mesh.AreaElems[Triangle];
                                double x1 = X[Knots[0]]; double x2 = X[Knots[1]]; double x3 = X[Knots[2]];
                                double y1 = Y[Knots[0]]; double y2 = Y[Knots[1]]; double y3 = Y[Knots[2]];
                                double L1 = s05 * ((x2 * y3 - x3 * y2) + PointsNorm[j].x * (y2 - y3) + PointsNorm[j].y * (x3 - x2));
                                double L2 = s05 * ((x3 * y1 - x1 * y3) + PointsNorm[j].x * (y3 - y1) + PointsNorm[j].y * (x1 - x3));
                                double L3 = s05 * ((x1 * y2 - x2 * y1) + PointsNorm[j].x * (y1 - y2) + PointsNorm[j].y * (x2 - x1));
                                        //
                                U12[j] = L1 * U[Knots[0]] + L2 * U[Knots[1]] + L3 * U[Knots[2]];
                                V12[j] = L1 * V[Knots[0]] + L2 * V[Knots[1]] + L3 * V[Knots[2]];
                            }
                                    //аппроксимация по 4 точкам
                                    //double dPhids = (2 * PhiNormal[0] - 5 * PhiNormal[1] + 4 * PhiNormal[2] - PhiNormal[3]) / k / k;
                                    //аппрроксимация по трем точкам
                            Ldu_dy = (-7 * U[cKnot] + 8 * U12[0] - U12[1]) / k / k / 2.0f;
                            Ldv_dy = (-7 * V[cKnot] + 8 * V12[0] - V12[1]) / k / k / 2.0f;
                                    //
                                    //точка на расстоянии k по обоим направлениям по касательной от точки cKnot
                            PointsTang[0] = new HPoint(X[cKnot] - lx * k, Y[cKnot] - ly * k);
                            PointsTang[1] = new HPoint(X[cKnot] + lx * k, Y[cKnot] + ly * k);
                                    //
                            for (int j = 0; j < 2; j++)
                            {
                                int Triangle = mesh.GetTriangle(PointsTang[j].x, PointsTang[j].y);
                                        //если область вогнутая, то хотя бы одна точка вдоль касательной бдует лежать вне области
                                        // здесь в таком случае влияние производных U и V  вдоль касательной не учитываем
                                if (Triangle == -1)
                                {
                                    U34[0] = 0; U34[1] = 0;
                                    V34[0] = 0; V34[1] = 0;
                                    break;
                                }
                                        //
                                double s05 = 1.0f / 2.0f / wrapper.Sk[Triangle];
                                uint[] Knots = mesh.AreaElems[Triangle];
                                double x1 = X[Knots[0]]; double x2 = X[Knots[1]]; double x3 = X[Knots[2]];
                                double y1 = Y[Knots[0]]; double y2 = Y[Knots[1]]; double y3 = Y[Knots[2]];
                                double L1 = s05 * ((x2 * y3 - x3 * y2) + PointsNorm[j].x * (y2 - y3) + PointsNorm[j].y * (x3 - x2));
                                double L2 = s05 * ((x3 * y1 - x1 * y3) + PointsNorm[j].x * (y3 - y1) + PointsNorm[j].y * (x1 - x3));
                                double L3 = s05 * ((x1 * y2 - x2 * y1) + PointsNorm[j].x * (y1 - y2) + PointsNorm[j].y * (x2 - x1));
                                        //
                                U34[j] = L1 * U[Knots[0]] + L2 * U[Knots[1]] + L3 * U[Knots[2]];
                                V34[j] = L1 * V[Knots[0]] + L2 * V[Knots[1]] + L3 * V[Knots[2]];

                            }
                            Ldu_dx = (U34[1] - U34[0]) / k / 2.0f;
                            Ldv_dx = (V34[1] - V34[0]) / k / 2.0f;

                        }
                                //
                                // давление в узле
                        P_c = P[cKnot];
                        double mu_c = nuT[cKnot] * rho_w;
                                // составляющие тензора напряжений
                        double Tx1 = -P_c + 2.0f * mu_c * Ldu_dx;
                        double Tx2 = mu_c * (Ldu_dy + Ldv_dx);
                        double Ty1 = mu_c * (Ldu_dy + Ldv_dx);
                        double Ty2 = -P_c + 2.0f * mu_c * Ldv_dy;
                                // компоненты вектора придонного напряжения
                        double LsigX = Tx1 * nx + Tx2 * ny;
                        double LsigY = Ty1 * nx + Ty2 * ny;
                                // проекция вектора придонного напряжения на касательный вектор
                        BTau[i] = (LsigX * lx + LsigY * ly);
                        arg[i] = X[cKnot];
                    }
                });
            }
        }
        protected void  CalkTau2()
        {

            // через ленту тензор TT
            {
                //компоненты тензора напряжений
                double[] Tx1 = new double[wrapper.BTriangles.Length];
                double[] Tx2 = new double[wrapper.BTriangles.Length];
                double[] Ty1 = new double[wrapper.BTriangles.Length];
                double[] Ty2 = new double[wrapper.BTriangles.Length];
                double[] tau_mid = new double[wrapper.BTriangles.Length];

                OrderablePartitioner<Tuple<int, int>> OrdPartitioner_Tau = Partitioner.Create(0, wrapper.BTriangles.Length);
                Parallel.ForEach(OrdPartitioner_Tau, (range, loopState) =>
                {
                    for (int i = range.Item1; i < range.Item2; i++)
                    {

                        double[] xL = new double[3];
                        double[] yL = new double[3];
                        uint LcurV = wrapper.BTriangles[i];
                        uint[] Knots = mesh.AreaElems[LcurV];
                        uint Lnum1 = Knots[0];
                        uint Lnum2 = Knots[1];
                        uint Lnum3 = Knots[2];
                        // получаем координаты узлов элемента
                        xL[0] = X[Lnum1]; xL[1] = X[Lnum2]; xL[2] = X[Lnum3];
                        yL[0] = Y[Lnum1]; yL[1] = Y[Lnum2]; yL[2] = Y[Lnum3];
                        // нахождение площади треугольника
                        double LS = wrapper.Sk[LcurV];
                        // скорости в вершинах треугольника
                        double LU1 = U[Lnum1];
                        double LU2 = U[Lnum2];
                        double LU3 = U[Lnum3];
                        double LV1 = V[Lnum1];
                        double LV2 = V[Lnum2];
                        double LV3 = V[Lnum3];
                        // производные в центре треугольника
                        double Ldu_dx = 1.0f / 2.0f / LS * (LU1 * (yL[1] - yL[2]) + LU2 * (yL[2] - yL[0]) + LU3 * (yL[0] - yL[1]));
                        double Ldu_dy = 1.0f / 2.0f / LS * (LU1 * (xL[2] - xL[1]) + LU2 * (xL[0] - xL[2]) + LU3 * (xL[1] - xL[0]));
                        double Ldv_dx = 1.0f / 2.0f / LS * (LV1 * (yL[1] - yL[2]) + LV2 * (yL[2] - yL[0]) + LV3 * (yL[0] - yL[1]));
                        double Ldv_dy = 1.0f / 2.0f / LS * (LV1 * (xL[2] - xL[1]) + LV2 * (xL[0] - xL[2]) + LV3 * (xL[1] - xL[0]));
                        // давление в центре треугольника
                        double LP_mid = (P[Lnum1] + P[Lnum2] + P[Lnum3]) / 3.0f;
                        double Mu_mid = (nuT[Lnum1] + nuT[Lnum2] + nuT[Lnum3]) / 3.0f * rho_w;
                        // составляющие тензора напряжений
                        Tx1[i] = -LP_mid + 2.0f * Mu_mid * Ldu_dx;
                        Tx2[i] = Mu_mid * (Ldu_dy + Ldv_dx);
                        Ty1[i] = Mu_mid * (Ldu_dy + Ldv_dx);
                        Ty2[i] = -LP_mid + 2.0f * Mu_mid * Ldv_dy;
                    }
                });
                // Вычисление tau в узлах сетки по сглаженной методике вычисления 
                //double[] Tau_all = Aproximate(tau_mid, mesh.CBottom, mesh.BTriangles);
                //вычисление тензора напряжений по сглаженной методике
                double[] Tx1_all = Aproximate(Tx1, wrapper.CBottom, wrapper.BTriangles);
                double[] Tx2_all = Aproximate(Tx2, wrapper.CBottom, wrapper.BTriangles);
                double[] Ty1_all = Aproximate(Ty1, wrapper.CBottom, wrapper.BTriangles);
                double[] Ty2_all = Aproximate(Ty2, wrapper.CBottom, wrapper.BTriangles);
                //подготовка приграничных значений tau между граничными точками и координат для сплайна
                int count = mesh.BottomKnots.Length;
                BTau = new double[count];
                BTauC = new double[count];
                arg = new double[count - 1];
                int cKnot = mesh.BottomKnots[0];
                double prevTx1 = 0, prevTx2 = 0, prevTy1 = 0, prevTy2 = 0;
                //
                for (int i = 0; i < mesh.BottomKnots.Length; i++)
                {
                    cKnot = mesh.BottomKnots[i];
                    for (int j = 0; j < wrapper.CBottom.Length; j++)
                    {
                        if (cKnot == wrapper.CBottom[j])
                        {
                            if (i != 0)
                            {
                                int prevKnot = mesh.BottomKnots[i - 1];
                                double delx = (X[cKnot] - X[prevKnot]);
                                double dely = (Y[cKnot] - Y[prevKnot]);
                                double ss = (double)(Math.Sqrt(delx * delx + dely * dely));
                                // компоненты касательной между граничными точками
                                double lx = 1 / ss * delx;
                                double ly = 1 / ss * dely;
                                double nx = -ly;
                                double ny = lx;
                                //
                                double LsigX = (prevTx1 + Tx1_all[j]) / 2.0f * nx + (prevTx2 + Tx2_all[j]) / 2.0f * ny;//умножили (Tx1;Tx2) на (0;-1) - нормаль к поверхности
                                double LsigY = (prevTy1 + Ty1_all[j]) / 2.0f * nx + (prevTy2 + Ty2_all[j]) / 2.0f * ny;//умножили (Ty1;Ty2) на (0;-1) - нормаль к поверхности
                                BTau[i - 1] = (LsigX * lx + LsigY * ly);
                                arg[i - 1] = X[prevKnot] + delx / 2.0f;
                                //
                                LsigX = Tx1_all[j] * nx + Tx2_all[j] * ny;//умножили (Tx1;Tx2) на (0;-1) - нормаль к поверхности
                                LsigY = Ty1_all[j] * nx + Ty2_all[j] * ny;//умножили (Ty1;Ty2) на (0;-1) - нормаль к поверхности
                                BTauC[i] = (LsigX * lx + LsigY * ly);

                            }
                            else
                            {
                                int nextKnot = mesh.BottomKnots[i + 1];
                                double delx = (X[nextKnot] - X[cKnot]);
                                double dely = (Y[nextKnot] - Y[cKnot]);
                                double ss = (double)(Math.Sqrt(delx * delx + dely * dely));
                                // компоненты касательной между граничными точками
                                double lx = 1 / ss * delx;
                                double ly = 1 / ss * dely;
                                double nx = -ly;
                                double ny = lx;
                                //
                                double LsigX = Tx1_all[j] * nx + Tx2_all[j] * ny;//умножили (Tx1;Tx2) на (0;-1) - нормаль к поверхности
                                double LsigY = Ty1_all[j] * nx + Ty2_all[j] * ny;//умножили (Ty1;Ty2) на (0;-1) - нормаль к поверхности
                                BTauC[i] = (LsigX * lx + LsigY * ly);
                            }
                            //
                            prevTx1 = Tx1_all[j];
                            prevTx2 = Tx2_all[j];
                            prevTy1 = Ty1_all[j];
                            prevTy2 = Ty2_all[j];
                            break;
                        }
                    }
                }
                BTau[count - 1] = BTau[count - 2];
                /////
                Tx1 = new double[wrapper.TTriangles.Length];
                Tx2 = new double[wrapper.TTriangles.Length];
                Ty1 = new double[wrapper.TTriangles.Length];
                Ty2 = new double[wrapper.TTriangles.Length];
                //
                tau_mid = new double[wrapper.TTriangles.Length];
                //double e1x = 1, e1y = 0, e2x = 0, e2y = 1;//проекции
                //
                OrdPartitioner_Tau = Partitioner.Create(0, wrapper.TTriangles.Length);
                Parallel.ForEach(OrdPartitioner_Tau,
                       (range, loopState) =>
                       {
                           for (int i = range.Item1; i < range.Item2; i++)
                                       //for (int i = 0; i < mesh.BTriangles.Length; i++)
                           {

                               double[] xL = new double[3];
                               double[] yL = new double[3];
                                           //
                               uint LcurV = wrapper.TTriangles[i];
                               uint[] Knots = mesh.AreaElems[LcurV];
                                           //
                               uint Lnum1 = Knots[0];
                               uint Lnum2 = Knots[1];
                               uint Lnum3 = Knots[2];
                                           // получаем координаты узлов элемента
                               xL[0] = X[Lnum1]; xL[1] = X[Lnum2]; xL[2] = X[Lnum3];
                               yL[0] = Y[Lnum1]; yL[1] = Y[Lnum2]; yL[2] = Y[Lnum3];
                                           // нахождение площади треугольника
                               double LS = wrapper.Sk[LcurV];
                                           // скорости в вершинах треугольника
                               double LU1 = U[Lnum1];
                               double LU2 = U[Lnum2];
                               double LU3 = U[Lnum3];

                               double LV1 = V[Lnum1];
                               double LV2 = V[Lnum2];
                               double LV3 = V[Lnum3];
                                           // производные в центре треугольника
                               double Ldu_dx = 1.0f / 2.0f / LS * (LU1 * (yL[1] - yL[2]) + LU2 * (yL[2] - yL[0]) + LU3 * (yL[0] - yL[1]));
                               double Ldu_dy = 1.0f / 2.0f / LS * (LU1 * (xL[2] - xL[1]) + LU2 * (xL[0] - xL[2]) + LU3 * (xL[1] - xL[0]));
                               double Ldv_dx = 1.0f / 2.0f / LS * (LV1 * (yL[1] - yL[2]) + LV2 * (yL[2] - yL[0]) + LV3 * (yL[0] - yL[1]));
                               double Ldv_dy = 1.0f / 2.0f / LS * (LV1 * (xL[2] - xL[1]) + LV2 * (xL[0] - xL[2]) + LV3 * (xL[1] - xL[0]));
                                           // давление в центре треугольника
                               double LP_mid = (P[Lnum1] + P[Lnum2] + P[Lnum3]) / 3.0f;
                               double Mu_mid = (nuT[Lnum1] + nuT[Lnum2] + nuT[Lnum3]) / 3.0f * rho_w;
                                           // составляющие тензора напряжений
                               Tx1[i] = -LP_mid + 2.0f * Mu_mid * Ldu_dx;
                               Tx2[i] = Mu_mid * (Ldu_dy + Ldv_dx);
                               Ty1[i] = Mu_mid * (Ldu_dy + Ldv_dx);
                               Ty2[i] = -LP_mid + 2.0f * Mu_mid * Ldv_dy;
                           }
                       });
                // Вычисление tau в узлах сетки по сглаженной методике вычисления 
                //double[] Tau_all = Aproximate(tau_mid, mesh.CBottom, mesh.BTriangles);

                //вычисление тензора напряжений по сглаженной методике
                Tx1_all = Aproximate(Tx1, wrapper.CTop, wrapper.TTriangles);
                Tx2_all = Aproximate(Tx2, wrapper.CTop, wrapper.TTriangles);
                Ty1_all = Aproximate(Ty1, wrapper.CTop, wrapper.TTriangles);
                Ty2_all = Aproximate(Ty2, wrapper.CTop, wrapper.TTriangles);
                //подготовка приграничных значений tau между граничными точками и координат для сплайна
                count = mesh.TopKnots.Length;
                TTau = new double[count];
                TTauC = new double[count];
                argT = new double[count - 1];
                cKnot = mesh.TopKnots[0];
                prevTx1 = 0; prevTx2 = 0; prevTy1 = 0; prevTy2 = 0;
                //
                for (int i = 0; i < mesh.TopKnots.Length; i++)
                {
                    cKnot = mesh.TopKnots[i];
                    for (int j = 0; j < wrapper.CTop.Length; j++)
                    {
                        if (cKnot == wrapper.CTop[j])
                        {
                            if (i != 0)
                            {
                                int prevKnot = mesh.TopKnots[i - 1];
                                double delx = (X[cKnot] - X[prevKnot]);
                                double dely = (Y[cKnot] - Y[prevKnot]);
                                double ss = (double)(Math.Sqrt(delx * delx + dely * dely));
                                // компоненты касательной между граничными точками
                                double lx = 1 / ss * delx;
                                double ly = 1 / ss * dely;
                                double nx = -ly;
                                double ny = lx;
                                //
                                double LsigX = (prevTx1 + Tx1_all[j]) / 2.0f * nx + (prevTx2 + Tx2_all[j]) / 2.0f * ny;//умножили (Tx1;Tx2) на (0;-1) - нормаль к поверхности
                                double LsigY = (prevTy1 + Ty1_all[j]) / 2.0f * nx + (prevTy2 + Ty2_all[j]) / 2.0f * ny;//умножили (Ty1;Ty2) на (0;-1) - нормаль к поверхности
                                                                                                                       //в полуцелых узлах
                                TTau[i - 1] = (LsigX * lx + LsigY * ly);
                                argT[i - 1] = X[prevKnot] + delx / 2.0f;
                                //
                                LsigX = Tx1_all[j] * nx + Tx2_all[j] * ny;//умножили (Tx1;Tx2) на (0;-1) - нормаль к поверхности
                                LsigY = Ty1_all[j] * nx + Ty2_all[j] * ny;//умножили (Ty1;Ty2) на (0;-1) - нормаль к поверхности
                                TTauC[i] = (LsigX * lx + LsigY * ly);

                            }
                            else
                            {
                                int nextKnot = mesh.TopKnots[i + 1];
                                double delx = (X[nextKnot] - X[cKnot]);
                                double dely = (Y[nextKnot] - Y[cKnot]);
                                double ss = (double)(Math.Sqrt(delx * delx + dely * dely));
                                // компоненты касательной между граничными точками
                                double lx = 1 / ss * delx;
                                double ly = 1 / ss * dely;
                                double nx = -ly;
                                double ny = lx;
                                //
                                double LsigX = Tx1_all[j] * nx + Tx2_all[j] * ny;//умножили (Tx1;Tx2) на (0;-1) - нормаль к поверхности
                                double LsigY = Ty1_all[j] * nx + Ty2_all[j] * ny;//умножили (Ty1;Ty2) на (0;-1) - нормаль к поверхности
                                TTauC[i] = (LsigX * lx + LsigY * ly);
                            }
                            //
                            prevTx1 = Tx1_all[j];
                            prevTx2 = Tx2_all[j];
                            prevTy1 = Ty1_all[j];
                            prevTy2 = Ty2_all[j];
                            break;
                        }
                    }
                }
                TTau[count - 1] = TTau[count - 2];

            }

        }

        /// <summary>
        /// Аппроксимация галеркина на подмножестве КЭ
        /// </summary>
        /// <param name="MiddleFunction">Среднее значение заданной функции на треугольнике</param>
        /// <param name="GLKnots"></param>
        /// <param name="GTriangs">номера граничных треугольников</param>
        /// <returns></returns>
        protected double[] Aproximate(double[] MiddleFunction, uint[] GLKnots, uint[] GTriangs)
        {
            int Count = GLKnots.Length;
            //
            double[] ExactFunc = new double[Count];
            //
            double[][] Matrix = new double[3][];
            for (int i = 0; i < 3; i++)
                Matrix[i] = new double[3];
            //
            try
            {
                IAlgebra algebraAprox = new AlgebraGaussTape((uint)Count, (uint)BWidth);
                //SBand AlgB = new SBand();
                //AlgB.SetSystem(Count, BWidth);
                // Вычисляем локальные матрицы жесткости и производим сборку глобальной матрицы жесткости
                uint tr = 0;
                uint[] GKnots, LKnots = new uint[3];
                double S = 0; ;
                for (int k = 0; k < GTriangs.Length; k++)
                {
                    tr = GTriangs[k];
                    S = wrapper.Sk[tr];
                    //S = mesh.GetSquare(tr);
                    //переходим к локальной нумерации для СЛАУ
                    GKnots = mesh.AreaElems[tr];
                    for (int i = 0; i < 3; i++)
                    {
                        for (uint j = 0; j < Count; j++)
                        {
                            if (GKnots[i] == GLKnots[j])
                            {
                                LKnots[i] = j;
                                break;
                            }
                        }
                    }
                    //
                    // Вычисляем локальную матрицу жесткости 
                    // Расчет локальной матрицы жесткости для диффузионного члена
                    Matrix[0][0] = 1.0 / 6.0 * S;
                    Matrix[0][1] = 1.0 / 12.0 * S;
                    Matrix[0][2] = 1.0 / 12.0 * S;

                    Matrix[1][0] = 1.0 / 12.0 * S;
                    Matrix[1][1] = 1.0 / 6.0 * S;
                    Matrix[1][2] = 1.0 / 12.0 * S;

                    Matrix[2][0] = 1.0 / 12.0 * S;
                    Matrix[2][1] = 1.0 / 12.0 * S;
                    Matrix[2][2] = 1.0 / 6.0 * S;
                    // Формирование глобальной матрицы жесткости
                    algebraAprox.AddToMatrix(Matrix, LKnots);
                    //AlgB.BuildMatrix(Matrix, Knots);
                    //mesh.SaveMesh("nn");
                    //
                    double[] tmpU = { MiddleFunction[k] * S / 3.0f, MiddleFunction[k] * S / 3.0f, MiddleFunction[k] * S / 3.0f };
                    //
                    algebraAprox.AddToRight(tmpU, LKnots);
                    //AlgB.BuildRight(tmpU, Knots);
                }
                // Решение системы алгебраических уравнений
                algebraAprox.Solve(ref ExactFunc);
                //AlgGauss.Solve(AlgB);
                //ExactFunc = AlgB.GetX;
            }
            catch (Exception e)
            {
                err = e.Message + "Aproximate fell down";
            }
            return ExactFunc;
        }

        /// <summary>
        /// Расчет начальной концентрации наносов
        /// </summary>
        protected void CalkStartS()
        {
            // концентрация наносов
            if (MEM.Equals(S.Max(), 0) == true)
            {
                double y;
                double Hn = Y[mesh.CountLeft - 1] - Y[0];
                double gamma = (rho_s - rho_w) / rho_w;
                double W_Stocks = gamma * GRAV * d * d / (18 * (nuT[mesh.CountLeft / 4] + nu_mol));
                double uz = 1.4 * Math.Sqrt(GRAV * Hn * Params.J);
                double Ko = Ww / (kappa * uz);
                double S0 = 2.17 * Ww * d / (Math.Exp(0.39 * gamma * d / Hn) - 1);
                for (int i = 0; i < mesh.CountLeft; i++)
                {
                    int knot = mesh.LeftKnots[i];
                    y = (Y[knot] - Y[0]);
                    // по Раузу [Гришанин] 
                    if (y == 0)
                        S[knot] = S0;
                    else
                        //    
                        S[knot] = S0 * Math.Pow((Hn - y) * 2 * d / (y * (Hn - 2 * d)), Ko);
                }
                for (int i = mesh.CountLeft; i < CountKnots; i++)
                {
                    S[i] = S[i % mesh.CountLeft];
                }
            }
        }
        /// <summary>
        /// Один шаг вычислений задачи
        /// </summary>
        public override void SolverStep()
        {
            System.Object lockThis = new System.Object();
            MEM.Alloc(CountKnots, ref y_plus);
            MEM.Alloc(CountKnots, ref Pk);
            MEM.Alloc(CountKnots, ref ReT, "ReT SolverStep()");
            MEM.Alloc(mesh.CountRight, ref BV2, "BV2 SolverStep()");
      
            // так как поменяла имя переменной, она не хочет десериализоваться из сохраненных решений. То же с nuT[]
            if (nu_mol == 0)
                nu_mol = mu / rho_w;

            // Расчет начальной концентрации наносов
            CalkStartS();
            // максимальная порешность по давлению
            double MaxError = 0; 
            double[] Result = null;
            try
            {
                // --------------------------------------------------------------
                // цикл по нелинейности на текущем шаге по времени
                // --------------------------------------------------------------
                for (int iteration = beginIter; iteration < Params.iter; iteration++)
                {
                    #region Расчет вязкости (перенесен в метод UVKW)
                    //// OrderablePartitioner<Tuple<int, int>> rangePartitioner3 = Partitioner.Create(0, CountKnots);
                    //Parallel.ForEach(OrdPartCountKnots,
                    //        (range, loopState) =>
                    //        {
                    //            for (int i = range.Item1; i < range.Item2; i++)
                    //            //for (int i = 0; i < CountKnots; i++)
                    //            {
                    //                nuT[i] = C_m * K[i] * K[i] / (W[i] + 1e-26);
                    //                if (nuT[i] < nu_mol)
                    //                    nuT[i] = nu_mol;

                    //            }
                    //        });
                    ////ГУ справа - снос
                    //for (int i = 0; i < mesh.CountRight; i++)
                    //{
                    //    knot = mesh.RightKnots[i];
                    //    //
                    //    nuT[knot] = nuT[knot - mesh.CountRight];
                    //}

                    //
                    #endregion

                    #region Расчет давления МКЭ
                    uint[] Adress = new uint[CountKnots];
                    for (uint i = 0; i < CountKnots; i++)
                        Adress[i] = i;
                    bool Gauss = false;
                    if (Gauss)
                    {
                        #region Расчет давления
                        //выделяем масивы для локальных правых частей
                        if (iteration == 0)
                        {
                            algebra.Clear();
                            //ABand.ClearSystem();
                            // основной цикл по конечным элементам
                            // вычисляем локальные матрицы жесткости и производим сборку глобальной матрицы жесткости
                            //OrderablePartitioner<Tuple<int, int>> OrdPartitioner_Tau = Partitioner.Create(0, mesh.CountElements);
                            Parallel.ForEach(OrdPartCountElems,
                                  (range, loopState) =>
                                  {
                                      for (int fe = range.Item1; fe < range.Item2; fe++)
                                          //for (int fe = 0; fe < mesh.CountElements; fe++)
                                      {
                                              // выделяем массивы для локальных матриц жесткости
                                          double[][] M = new double[3][];
                                          for (int k = 0; k < 3; k++)
                                          {
                                              M[k] = new double[3];
                                          }
                                              //и номера его вершин
                                          uint[] LKnots = mesh.AreaElems[fe];
                                              // нахождение площади треугольника
                                          double LSk = wrapper.Sk[fe];
                                              // расчитываем геометрию элемента 
                                          double Lb1 = wrapper.b1[fe];
                                          double Lb2 = wrapper.b2[fe];
                                          double Lb3 = wrapper.b3[fe];
                                          double Lc1 = wrapper.c1[fe];
                                          double Lc2 = wrapper.c2[fe];
                                          double Lc3 = wrapper.c3[fe];
                                              // расчет локальной матрицы жесткости для диффузионного члена
                                          M[0][0] = -LSk * (Lb1 * Lb1 + Lc1 * Lc1);
                                          M[0][1] = -LSk * (Lb1 * Lb2 + Lc1 * Lc2);
                                          M[0][2] = -LSk * (Lb1 * Lb3 + Lc1 * Lc3);

                                          M[1][0] = -LSk * (Lb2 * Lb1 + Lc2 * Lc1);
                                          M[1][1] = -LSk * (Lb2 * Lb2 + Lc2 * Lc2);
                                          M[1][2] = -LSk * (Lb2 * Lb3 + Lc2 * Lc3);

                                          M[2][0] = -LSk * (Lb3 * Lb1 + Lc3 * Lc1);
                                          M[2][1] = -LSk * (Lb3 * Lb2 + Lc3 * Lc2);
                                          M[2][2] = -LSk * (Lb3 * Lb3 + Lc3 * Lc3);
                                          //int[] Knots = { (int)LKnots[0], (int)LKnots[1], (int)LKnots[2] };
                                          lock (lockThis)
                                              algebra.AddToMatrix(M, LKnots);
                                          //ABand.BuildMatrix(M, Knots);
                                      }

                                  });
                            //
                            //главные ГУ справа
                            uint[] BK = MEM.ToUInt(mesh.RightKnots);
                            
                            algebra.BoundConditions(BV2, BK);
                            //ABand.SetBoundary(BV2, mesh.RightKnots);
                            //ABand.Accept(AlgGauss, null, 1);
                        }
                        //ABand.ClearRight();
                        R = new double[CountKnots];
                        //сборка правой части
                        //OrderablePartitioner<Tuple<int, int>> rangePartitioner1 = Partitioner.Create(0, mesh.CountElements);
                        Parallel.ForEach(OrdPartCountElems,
                              (range, loopState) =>
                              {
                                  for (int fe = range.Item1; fe < range.Item2; fe++)
                                      //for(int fe=0;fe<mesh.CountElements;fe++)
                                  {

                                          //for (int fe = 0; fe < mesh.CountElem; fe++)
                                          //{
                                          // получаем текущий конечный элемент
                                      uint[] LKnots = mesh.AreaElems[fe];
                                      uint Lm1 = LKnots[0];
                                      uint Lm2 = LKnots[1];
                                      uint Lm3 = LKnots[2];
                                          // нахождение площади треугольника
                                      double LSk = wrapper.Sk[fe];
                                          // расчитываем геометрию элемента 
                                      double Lb1 = wrapper.b1[fe];
                                      double Lb2 = wrapper.b2[fe];
                                      double Lb3 = wrapper.b3[fe];
                                      double Lc1 = wrapper.c1[fe];
                                      double Lc2 = wrapper.c2[fe];
                                      double Lc3 = wrapper.c3[fe];
                                          //
                                      double LU1 = U[Lm1]; double LU2 = U[Lm2]; double LU3 = U[Lm3];
                                      double LV1 = V[Lm1]; double LV2 = V[Lm2]; double LV3 = V[Lm3];
                                          //правая часть - неразрывность
                                      double LSR = rho_w * LSk / (3 * Params.tau) * ((Lb1 * LU1 + Lb2 * LU2 + Lb3 * LU3) + (Lc1 * LV1 + Lc2 * LV2 + Lc3 * LV3));
                                      double R0 = Params.alpha_n * LSR;
                                      double R1 = Params.alpha_n * LSR;
                                      double R2 = Params.alpha_n * LSR;
                                          //правая часть поправка КГД
                                      double LBUU = (Lb1 * LU1 + Lb2 * LU2 + Lb3 * LU3) * (LU1 + LU2 + LU3);
                                      double LCUV = (Lc1 * LU1 + Lc2 * LU2 + Lc3 * LU3) * (LV1 + LV2 + LV3);
                                      double LBVU = (Lb1 * LV1 + Lb2 * LV2 + Lb3 * LV3) * (LU1 + LU2 + LU3);
                                      double LCVV = (Lc1 * LV1 + Lc2 * LV2 + Lc3 * LV3) * (LV1 + LV2 + LV3);
                                          //
                                      R0 += (double)(Params.alpha_r * rho_w * LSk / 3.0 * (Lb1 * (LBUU + LCUV) + Lc1 * (LBVU + LCVV)));
                                      R1 += (double)(Params.alpha_r * rho_w * LSk / 3.0 * (Lb2 * (LBUU + LCUV) + Lc2 * (LBVU + LCVV)));
                                      R2 += (double)(Params.alpha_r * rho_w * LSk / 3.0 * (Lb3 * (LBUU + LCUV) + Lc3 * (LBVU + LCVV)));
                                          //
                                          //для отображения невязки
                                      R[Lm1] += R0;
                                      R[Lm2] += R1;
                                      R[Lm3] += R2;
                                          //}
                                  }

                              });
                        //
                        algebra.AddToRight(R, Adress);
                        //ABand.BuildRight(R);
                        //ГУ слева
                        algebra.BoundConditions(BV, MEM.ToUInt(mesh.LeftKnots));
                        //ABand.BuildRight(BV, mesh.LeftKnots);
                        //главные ГУ справа 2
                        algebra.BoundConditions(BV2, MEM.ToUInt(mesh.RightKnots));
                        //ABand.BoundConditionsRight(BV2, mesh.RightKnots);
                        // решение системы алгебраических уравнений
                        algebra.Solve(ref Result);
                        //ABand.Accept(AlgGauss, null, 2);
                        //Result = ABand.GetX;
                        //для отладки
                        for (int i = 0; i < mesh.CountLeft; i++)
                            R[mesh.LeftKnots[i]] += BV[i];
                        for (int i = 0; i < mesh.CountRight; i++)
                            R[mesh.RightKnots[i]] = 0;

                        // релаксация решения, буферизация и вычисление погрешности
                        MaxError = 0;//масимальная ошибка в области
                        double MaxP = 0.5 *( Math.Abs( P.Max() - P.Min()) + Math.Abs(buffP.Max() - buffP.Min())) + MEM.Error10;
                        for (int i = 0; i < CountKnots; i++)
                        {
                            //релаксация
                            P[i] = (1 - Params.relaxP) * P[i] + Params.relaxP * Result[i];
                            //вычисление погрешности
                            double CurErr = Math.Abs((P[i] - buffP[i])) / MaxP;
                            if (MaxError < CurErr)
                                MaxError = CurErr;
                            ErrP[i] = CurErr;
                            //буферизация
                            buffP[i] = P[i];
                        }
                        #endregion
                    }
                    //
                    else
                    {
                        #region Расчет давления не по Гауссу или с применением ГУ Dong
                        algebra.Clear();
                        // выделение памяти под результат решения задачи
                        //Result = new double[mesh.CountKnots];
                        //выделяем масивы для локальных правых частей
                        // основной цикл по конечным элементам
                        // вычисляем локальные матрицы жесткости и производим сборку глобальной матрицы жесткости
                        // OrderablePartitioner<Tuple<int, int>> OrdPartitioner_Tau = Partitioner.Create(0, mesh.CountElements);
                        Parallel.ForEach(OrdPartCountElems,
                              (range, loopState) =>
                              {
                                  for (int fe = range.Item1; fe < range.Item2; fe++)
                                      //for (int fe = 0; fe < mesh.CountElements; fe++)
                                  {
                                          // выделяем массивы для локальных матриц жесткости
                                      double[][] M = new double[3][];
                                      for (int k = 0; k < 3; k++)
                                      {
                                          M[k] = new double[3];
                                      }
                                          //и номера его вершин
                                      uint[] LKnots = mesh.AreaElems[fe];
                                          // нахождение площади треугольника
                                      double LSk = wrapper.Sk[fe];
                                          // расчитываем геометрию элемента 
                                      double Lb1 = wrapper.b1[fe];
                                      double Lb2 = wrapper.b2[fe];
                                      double Lb3 = wrapper.b3[fe];
                                      double Lc1 = wrapper.c1[fe];
                                      double Lc2 = wrapper.c2[fe];
                                      double Lc3 = wrapper.c3[fe];
                                          // расчет локальной матрицы жесткости для диффузионного члена
                                      M[0][0] = -LSk * (Lb1 * Lb1 + Lc1 * Lc1);
                                      M[0][1] = -LSk * (Lb1 * Lb2 + Lc1 * Lc2);
                                      M[0][2] = -LSk * (Lb1 * Lb3 + Lc1 * Lc3);

                                      M[1][0] = -LSk * (Lb2 * Lb1 + Lc2 * Lc1);
                                      M[1][1] = -LSk * (Lb2 * Lb2 + Lc2 * Lc2);
                                      M[1][2] = -LSk * (Lb2 * Lb3 + Lc2 * Lc3);

                                      M[2][0] = -LSk * (Lb3 * Lb1 + Lc3 * Lc1);
                                      M[2][1] = -LSk * (Lb3 * Lb2 + Lc3 * Lc2);
                                      M[2][2] = -LSk * (Lb3 * Lb3 + Lc3 * Lc3);
                                      //int[] Knots = { (int)LKnots[0], (int)LKnots[1], (int)LKnots[2] };
                                      lock (lockThis)
                                          algebra.AddToMatrix(M, LKnots);
                                  }

                              });
                        R = new double[CountKnots];
                        //сборка правой части
                        Parallel.ForEach(OrdPartCountElems,
                                (range, loopState) =>
                                {
                                    for (int fe = range.Item1; fe < range.Item2; fe++)
                                        //for (int fe = 0; fe < mesh.CountElements; fe++)
                                    {

                                            //for (int fe = 0; fe < mesh.CountElem; fe++)
                                            //{
                                            // получаем текущий конечный элемент
                                        uint[] LKnots = mesh.AreaElems[fe];
                                        uint Lm1 = LKnots[0];
                                        uint Lm2 = LKnots[1];
                                        uint Lm3 = LKnots[2];
                                            // нахождение площади треугольника
                                        double LSk = wrapper.Sk[fe];
                                            // расчитываем геометрию элемента 
                                        double Lb1 = wrapper.b1[fe];
                                        double Lb2 = wrapper.b2[fe];
                                        double Lb3 = wrapper.b3[fe];
                                        double Lc1 = wrapper.c1[fe];
                                        double Lc2 = wrapper.c2[fe];
                                        double Lc3 = wrapper.c3[fe];
                                            //
                                        double LU1 = U[Lm1]; double LU2 = U[Lm2]; double LU3 = U[Lm3];
                                        double LV1 = V[Lm1]; double LV2 = V[Lm2]; double LV3 = V[Lm3];
                                            //правая часть - неразрывность
                                        double LSR = rho_w * LSk / (3 * Params.tau) * ((Lb1 * LU1 + Lb2 * LU2 + Lb3 * LU3) + (Lc1 * LV1 + Lc2 * LV2 + Lc3 * LV3));
                                        double R0 = Params.alpha_n * LSR;
                                        double R1 = Params.alpha_n * LSR;
                                        double R2 = Params.alpha_n * LSR;
                                            //правая часть поправка КГД
                                        double LBUU = (Lb1 * LU1 + Lb2 * LU2 + Lb3 * LU3) * (LU1 + LU2 + LU3);
                                        double LCUV = (Lc1 * LU1 + Lc2 * LU2 + Lc3 * LU3) * (LV1 + LV2 + LV3);
                                        double LBVU = (Lb1 * LV1 + Lb2 * LV2 + Lb3 * LV3) * (LU1 + LU2 + LU3);
                                        double LCVV = (Lc1 * LV1 + Lc2 * LV2 + Lc3 * LV3) * (LV1 + LV2 + LV3);
                                            //
                                        R0 += Params.alpha_r * rho_w * LSk / 3.0 * (Lb1 * (LBUU + LCUV) + Lc1 * (LBVU + LCVV));
                                        R1 += Params.alpha_r * rho_w * LSk / 3.0 * (Lb2 * (LBUU + LCUV) + Lc2 * (LBVU + LCVV));
                                        R2 += Params.alpha_r * rho_w * LSk / 3.0 * (Lb3 * (LBUU + LCUV) + Lc3 * (LBVU + LCVV));
                                            //ГУ по Фомину
                                            //RR[0] += alpha_p * rho_w * LSk / 3.0 * (1 / dt * qf);
                                            //RR[1] += alpha_p * rho_w * LSk / 3.0 * (1 / dt * qf);
                                            //RR[2] += alpha_p * rho_w * LSk / 3.0 * (1 / dt * qf);
                                            //
                                            //для отображения невязки
                                        R[Lm1] += R0;
                                        R[Lm2] += R1;
                                        R[Lm3] += R2;
                                            //}
                                    }

                                });

                        algebra.AddToRight(R, Adress);
                        //ГУ слева
                        
                        algebra.AddToRight(BV, MEM.ToUInt(mesh.LeftKnots));
                        //главные ГУ справа 2
                        //if (iteration == iter - 1)
                        //    DongBC_PE();
                        //else
                        //    DongBC_P();
                        // ГУ P=0 справа    //
                        algebra.BoundConditions(0, MEM.ToUInt(mesh.RightKnots));
                        //RBC = new double[mesh.CountRight];
                        // для Dong убрать  //
                        //Sys.SetBoundary(RBC, mesh.RightKnots);
                        //Sys.SetBoundary(BV2, mesh.RightKnots);
                        //Sys.SetBoundary(new double[] { 0 }, new int[] { mesh.RightKnots[mesh.CountRight - 1] }); // задаем ноль в правой верхней точке
                        // решение системы алгебраических уравнений
                        algebra.Solve(ref Result);
                        //для отладки
                        for (int i = 0; i < mesh.CountLeft; i++)
                            R[mesh.LeftKnots[i]] += BV[i];
                        for (int i = 0; i < mesh.CountRight; i++)
                            R[mesh.RightKnots[i]] = 0;
                        // релаксация решения, буферизация и вычисление погрешности
                        MaxError = 0;// масимальная ошибка в области
                        double MaxP = 0.5 * (Math.Abs(P.Max() - P.Min()) + Math.Abs(buffP.Max() - buffP.Min())) + MEM.Error10;
                        for (int i = 0; i < CountKnots; i++)
                        {
                            //релаксация
                            P[i] = (1 - Params.relaxP) * P[i] + Params.relaxP * Result[i];
                            //вычисление погрешности
                            double CurErr = Math.Abs((P[i] - buffP[i]) / MaxP);
                            if (MaxError < CurErr)
                                MaxError = CurErr;
                            ErrP[i] = CurErr;
                            //буферизация
                            buffP[i] = P[i];
                        }

                        #endregion
                    }
                    #endregion

                    #region Расчет скоростей МКО
                    
                    iteration = QHDUS(iteration);
                    if (err != "ok")
                        return;
                    
                    #endregion

                    // вычисление параметров турбулентной модели
                    iteration = TurbModel(iteration);
                    //выход из цикла если ошибка меньше или равно errP
                    if (MaxError <= Params.errP)
                    {
                        Iter = iteration + 1;
                        break;
                    }
                    //
                    beginIter = iteration + 1;
                    Iter = iteration + 1;
                    //расход на входе и выходе из расчетной области
                    Q_in = 0; Q_out = 0;
                    for (int k = 0; k < mesh.CountLeft - 1; k++)
                    {
                        int a = mesh.LeftKnots[k];
                        int b = mesh.LeftKnots[k + 1];
                        Q_in += (U[a] + U[b]) * 0.5f * Math.Abs(Y[a] - Y[b]);
                    }
                    //
                    for (int k = 1; k < mesh.CountRight - 1; k++)
                    {
                        int a = mesh.RightKnots[k];
                        int b = mesh.RightKnots[k + 1];
                        Q_out += (U[a] + U[b]) * 0.5f * Math.Abs(Y[a] - Y[b]);
                    }
                }
                // Буфферезация
                for (int i = 0; i < CountKnots; i++)
                {
                    buffU[i] = U[i];
                    buffV[i] = V[i];
                    buffK[i] = K[i];
                    buffW[i] = W[i];
                    buffS[i] = S[i];
                    buffNu[i] = nuT[i];
                    #region Сохранение y_plus - отключено
                    // y+ по Гришанину
                    //double delta = 0.6 * d;
                    //if (i < CountKnots - 10)
                    //{
                    //    double y1 = mesh.GetNormalDistanceBottom(i);
                    //    //double y1 = Y[i] - Y[(i / mesh.CountLeft) * mesh.CountLeft];
                    //    double f = U[i] * y1 / nu_mol;
                    //    double y_pl = 0;
                    //    if (f <= 782.4968) // ламинарный слой
                    //        y_pl = Math.Sqrt(2 * f);
                    //    else  // логарифмический слой
                    //    {
                    //        double y_i = 39.56;
                    //        double F_i = 0, u_i = 0, y_i1 = 0;
                    //        for (int h = 0; h < 7; h++)
                    //        {
                    //            F_i = 2.5 * y_i * (Math.Log(20.0855 * y_i / delta) - 1) - 683.559072;
                    //            u_i = 2.5 * Math.Log(20.0855 * y_i / delta);
                    //            y_i1 = y_i + (f - F_i) / u_i;
                    //            if (Math.Abs(y_i - y_i1) < 0.000001)
                    //            {
                    //                y_i = y_i1;
                    //                break;
                    //            }
                    //            y_i = y_i1;
                    //        }
                    //        y_pl = y_i1;
                    //    }
                    //    //
                    //    y_plus[i] = y_pl;
                    //}
                    //------------------------
                    // y+ по Волкову
                    //double kp = K[i];
                    //double Dy = Y[i] - Y[(i / mesh.CountLeft) * mesh.CountLeft];
                    //double Du = U[i];
                    //double Re = rho_w * cm14 * Math.Sqrt(Math.Abs(kp)) * Dy / mu;
                    //// tau
                    //double tauw = (rho_w * cm14 * Math.Sqrt(Math.Abs(kp)) * Du) / (1.0 / kappa * Math.Log(8.8 * Re));
                    //y_plus[i] = Dy * Math.Sqrt(tauw / rho_w) / nu_mol;
                    //-------------------------------------------------
                    // по Луцкому с буферным слоем
                    //if (i < CountKnots - 10)
                    //{
                    //    double y1 = mesh.GetNormalDistanceBottom(i);
                    //    double f = U[i] * y1 / nu_mol;
                    //    double y_pl = 0;
                    //    if (f <= 2) // ламинарный слой
                    //        y_pl = Math.Sqrt(2 * f);
                    //    else if (f >= 696.68) // логарифмический слой
                    //    {
                    //        double y_i = 60;
                    //        double F_i = 0, u_i = 0, y_i1 = 0;
                    //        for (int h = 0; h < 7; h++)
                    //        {
                    //            F_i = 2.5 * y_i * (Math.Log(y_i / 0.13) - 1) - 73.50481;
                    //            u_i = 2.5 * Math.Log(y_i / 0.13);
                    //            y_i1 = y_i + (f - F_i) / u_i;
                    //            if (Math.Abs(y_i - y_i1) < 0.000001)
                    //            {
                    //                y_i = y_i1;
                    //                break;
                    //            }
                    //            y_i = y_i1;
                    //        }
                    //        y_pl = y_i1;
                    //    }
                    //    else // буферный слой
                    //        y_pl = alglib.spline1dcalc(bufF, f);
                    //    //
                    //    y_plus[i] = y_pl;
                    //}
                    // --------------------------------------
                    // по Снегиреву
                    //double dy = Y[i] - Y[(i / mesh.CountLeft) * mesh.CountLeft];
                    //if (dy > 0)
                    //{
                    //    double y_p_plus = cm14 * Math.Sqrt(K[i]) * dy / nu_mol;
                    //    double u2_tau = 0;
                    //    if (y_p_plus > y_p_0)
                    //        u2_tau = cm14 * cm14 * K[i];
                    //    else
                    //        u2_tau = nu_mol * U[i] / (dy + 0.00000001);
                    //    double tau_w = u2_tau * rho_w;
                    //    y_plus[i] = dy * Math.Sqrt(tau_w / rho_w) / nu_mol;
                    //}
                    #endregion
                }
                // beginIter = 0;
                //Q_in = 0; Q_out = 0;
                //for (int k = 0; k < mesh.CountLeft - 1; k++)
                //{
                //    int a = mesh.LeftKnots[k];
                //    int b = mesh.LeftKnots[k + 1];
                //    Q_in += (U[a] + U[b]) * 0.5f * Math.Abs(Y[a] - Y[b]);
                //}
                ////
                //for (int k = 1; k < mesh.CountRight - 1; k++)
                //{
                //    int a = mesh.RightKnots[k];
                //    int b = mesh.RightKnots[k + 1];
                //    Q_out += (U[a] + U[b]) * 0.5f * Math.Abs(Y[a] - Y[b]);
                //}
            }
            catch (Exception ex)
            {
                err = err + "WaterTask.Run " + ex.Message;
            }
            //вычисление сдвиговых напряжений на дне через скорости по уравненям Рейнольдса
            ////ShearStresses();
            //ShearStress(mesh.BottomKnots, mesh.CBottom, mesh.BTriangles, out BTau, out BTauC, out arg);
            //ShearStress(mesh.TopKnots, mesh.CTop, mesh.TTriangles, out TTau, out TTauC, out argT);
            //
            // напряжения на дне находятся по функции стенки при вычислении k-e
        }
        
        #endregion IRiver

        #region Граничные условия
        /// <summary>
        /// Установка начальных и граничных условий ???
        /// </summary>
        /// <exception cref="Exception"></exception>
        protected void InitialStartConditions()
        {
            //Начальное условие для скорости
            double Hn = Y[mesh.LeftKnots[0]] - Y[mesh.LeftKnots[mesh.CountLeft - 1]];
            double I = 0.2;
            if (Params.surf_flag == true) 
                InitSurf_1(Hn, I); // Профиль скорости и т.д. на входе для половинки канала
            else
                InitSurf_2(Hn, I); // Профиль скорости и т.д. на входе для щелевого (полного) канала
            // средняя скорость
            Ucp = U.Sum() / mesh.CountLeft;
            //  Расчет концентрации взвешенных частиц на входе в расчетную область
            CalkS0(Hn);
            // Гу слева прописывается во всей области
            for (int i = mesh.CountLeft; i < CountKnots; i++)
            {
                U[i] = U[i % mesh.CountLeft];
                V[i] = V[i % mesh.CountLeft];
                K[i] = K[i % mesh.CountLeft];
                E[i] = E[i % mesh.CountLeft];
                W[i] = W[i % mesh.CountLeft];
                S[i] = S[i % mesh.CountLeft];
            }

            if (flag)
            {
                MEM.Alloc(CountKnots, ref buffP);
                MEM.Alloc(CountKnots, ref buffU);
                MEM.Alloc(CountKnots, ref buffV);
                MEM.Alloc(CountKnots, ref buffS);
                MEM.Alloc(CountKnots, ref buffK);
                MEM.Alloc(CountKnots, ref buffE);
                MEM.Alloc(CountKnots, ref buffW);
                MEM.Alloc(CountKnots, ref buffNu);
                flag = false;
            }
            else
            {
                // решение с предыдущего расчета
                for (int i = 0; i < CountKnots; i++)
                {
                    U[i] = buffU[i];
                    V[i] = buffV[i];
                    P[i] = buffP[i];
                    S[i] = buffS[i];
                    K[i] = buffK[i];
                    E[i] = buffE[i];
                    W[i] = buffW[i];
                    nuT[i] = buffNu[i];
                }
            }
            //ГУ на левой стенке для КЭ
            // массивы для ГУ
            MEM.Alloc(mesh.CountLeft, ref RBC);
            MEM.Alloc(mesh.CountLeft, ref BV);
            // естестненные ГУ для давления (переделать по маркеру)
            for (int i = 0; i < mesh.CountLeft - 1; i++)
            {
                double dy = Math.Abs(Y[mesh.LeftKnots[i]] - Y[mesh.LeftKnots[i + 1]]);
                double q =   dy  * (dPdx) / 2.0;
                BV[i] += q;
                BV[i + 1] += q;
            }
        }
        /// <summary>
        /// Профиль скорости и т.д. на входе для половинки канала
        /// </summary>
        /// <param name="Hn">глубина канала на выходе из области</param>
        /// <param name="I"></param>
        protected void InitSurf_1(double Hn, double I = 0.2)
        {
            double y;
            U_max = 3.0 / 2.0 * Params.Q / (2 * Hn);
            double Uav = Params.Q / Hn;
            int N = 50;
            if (Params.velocityInProfile == VelocityInProfile.PorabolicProfile)
            {
                // --- по параболическому профилю
                dPdx = -8.0 * U_max * mu / Hn / Hn;
                for (int i = 0; i < mesh.CountLeft; i++)
                {
                    int knot = mesh.LeftKnots[i];
                    //-- параболический профиль скорости
                    y = (Y[knot] - Y[0]);
                    U[knot] = (double)(1.0 / 2.0 / mu * dPdx * (y - Hn * 2) * y);
                }
            }
            else
            {
                dPdx = -(2 + N) / Hn / Hn * mu * Uav;
                for (int i = 0; i < mesh.CountLeft; i++)
                {
                    // U_left
                    int knot = mesh.LeftKnots[i];
                    y = (Y[knot] - Y[0]) - Hn;
                    U[knot] = (N + 2) / (N + 1) * Uav * (1 - Math.Pow(Math.Abs(y) / (Hn), N + 1));
                }
            }
            switch (Params.turbulentModelType)
            {
                case TurbulentModelType.Model_k_e: // Расчет K, epsilon
                case TurbulentModelType.Model_k_eG: // Расчет k-e вместе с пристеночной функцией по Жлуткову 2015
                    for (int i = 0; i < mesh.CountLeft; i++)
                    {
                        int knot = mesh.LeftKnots[i];
                        y = (Y[knot] - Y[0]);
                        //K[knot] = 0.005 * U[knot] * U[knot];
                        //// e_left  --> V
                        //E[knot] = 0.1 * K[knot] * K[knot];// Math.Pow(C_m,0.75)*Math.Pow(K[knot],1.5)/0.1/H;
                        // По Роди
                        //double Uav = Q / Hn;
                        K[knot] = 0.5e-4 * (U[knot] / Uav) * (U[knot] / Uav);
                        double dudy = 0;
                        int knot2 = 0;
                        dy = Y[1] - Y[0];
                        if (i != 0)
                        {
                            knot2 = mesh.LeftKnots[i - 1];
                            dudy = (U[knot] - U[knot2]) / dy;
                        }
                        else
                        {
                            knot2 = mesh.LeftKnots[i + 1];
                            dudy = (U[knot2] - U[knot]) / dy;
                        }
                        E[knot] = 0.1 * K[knot] * Math.Abs(dudy);
                        nuT[knot] = C_m * K[knot] * K[knot] / (E[knot] + 1.0e-15);
                    }
                    break;
                case TurbulentModelType.Model_k_w:
                    for (int i = 0; i < mesh.CountLeft; i++)
                    {
                        int knot = mesh.LeftKnots[i];
                        y = (Y[knot] - Y[0]);
                        K[knot] = 1.5 * (I * U[knot]) * (I * U[knot]);
                        if (i == mesh.CountLeft - 2)
                            W[knot] = Math.Sqrt(6.0 * K[knot] / beta) / wrapper.BWallDistance[0];
                        //W[knot] = 6.0 * nu_mol / beta / mesh.BWallDistance[0] / mesh.BWallDistance[0];
                        else
                            W[knot] = Math.Sqrt(K[knot]) / 0.1 / Hn;
                        nuT[knot] = K[knot] / (W[knot] + 1.0e-26);
                    }
                    break;
                case TurbulentModelType.Model_Newton: // Расчет гидрадинамики с постоянной вязкостью
                    for (int i = 0; i < mesh.CountLeft; i++)
                    {
                        int knot = mesh.LeftKnots[i];
                        nuT[knot] = nu;
                    }
                    break;
            }
        }
        /// <summary>
        /// Профиль скорости и т.д. на входе для щелевого (полного) канала
        /// </summary>
        /// <param name="Hn">высота щели канала на выходе из области</param>
        /// <param name="I"></param>
        protected void InitSurf_2(double Hn, double I = 0.2)
        {
            //bool parabola = false;
            //Начальное условие для скорости
            double y;
            // скользящая крышка (половинка параболы)
            // расчет профиля
            //int N = -Convert.ToInt32((Hn / 2.0) * (Hn / 2.0) / mu / Uav * dPdx - 2);
            int N = 20;
            // ---------------------------------------------
            // Профиль скорости на входе
            // ---------------------------------------------
            double Uav = Params.Q / Hn;
            if (Params.velocityInProfile == VelocityInProfile.PorabolicProfile)
            {
                U_max = 3.0 / 2.0 * Params.Q / Hn;
                // --- по параболическому профилю
                dPdx = -8.0 * U_max * mu / Hn / Hn;
                for (int i = 0; i < mesh.CountLeft; i++)
                {
                    // U_left
                    int knot = mesh.LeftKnots[i];
                    y = (Y[knot] - Y[0]);
                    U[knot] = (double)(1.0 / 2.0 / mu * dPdx * (y - Hn) * y);
                    // -- скорректированный параболический профиль
                    //y = (Y[knot] - Y[0]);
                    //U[knot] = (double)(1.0 / 2.0 / mu * dPdx * (y - Hn) * y);
                    //if (U[knot] > Uav)
                    //   U[knot] = Uav;
                }
            }
            else
            {
                //-- по степенному профилю
                dPdx = -(2 + N) / (Hn / 2.0) / (Hn / 2.0) * mu * Uav;
                for (int i = 0; i < mesh.CountLeft; i++)
                {
                    int knot = mesh.LeftKnots[i];
                    y = (Y[knot] - Y[0]) - Hn / 2.0;
                    U[knot] = (N + 2) / (N + 1) * Uav * (1 - Math.Pow(Math.Abs(y) / (Hn / 2.0), N + 1));
                }
            }
            // ---------------------------------------------
            // Профиль турбулентных параметров на входе
            // ---------------------------------------------
            for (int i = 0; i < mesh.CountLeft; i++)
            {
                int knot = mesh.LeftKnots[i];
                y = (Y[knot] - Y[0]);
                // модель турбулентности
                switch (Params.turbulentModelType)
                {
                    case TurbulentModelType.Model_k_e: // Расчет K, epsilon
                    case TurbulentModelType.Model_k_eG: // Расчет k-e вместе с пристеночной функцией по Жлуткову 2015

                        K[knot] = 0.005 * U[knot] * U[knot];
                        E[knot] = 0.1 * K[knot] * K[knot];// Math.Pow(C_m,0.75)*Math.Pow(K[knot],1.5)/0.1/H;
                        nuT[knot] = C_m * K[knot] * K[knot] / (E[knot] + 1.0e-26);

                        break;
                    case TurbulentModelType.Model_k_w: // Расчет k, omega

                        K[knot] = 1.5 * (I * U[knot]) * (I * U[knot]);
                        if ((i == 1) || (i == mesh.CountLeft - 2))
                        {
                            W[knot] = Math.Sqrt(6.0 * K[knot] / beta) / wrapper.BWallDistance[0];
                            //W[knot] = 6.0 * nu_mol / beta / mesh.BWallDistance[0] / mesh.BWallDistance[0];
                        }
                        else
                            W[knot] = Math.Sqrt(K[knot]) / 0.1 / Hn;
                        nuT[knot] = K[knot] / (W[knot] + 1.0e-26);

                        break;
                    case TurbulentModelType.Model_Newton: // Расчет гидрадинамики с постоянной вязкостью

                        nuT[knot] = 1e-6;

                        break;
                }
            }
            int idx = mesh.CountLeft / 2;
            // Коорекция градиента на входе через поля турб.модели
            switch (Params.turbulentModelType)
            {
                case TurbulentModelType.Model_k_e: // Расчет K, epsilon
                case TurbulentModelType.Model_k_eG: // Расчет k-e вместе с пристеночной функцией по Жлуткову 2015
                    
                    dPdx = -(2 + N) / Hn / Hn * C_m * K[idx] * K[idx] / (E[idx] + 1.0e-15) * rho_w * Uav;

                    break;
                case TurbulentModelType.Model_k_w:
                    //dPdx = -(2 + N) / Hn / Hn * mu * Uav;

                    dPdx = -(2 + N) / Hn / Hn * K[idx] / W[idx] * rho_w * Uav;

                    break;
                case TurbulentModelType.Model_Newton: // Расчет гидрадинамики с постоянной вязкостью

                    dPdx = -8.0 * U_max * mu / Hn / Hn;

                    break;
            }
        }
        /// <summary>
        /// Расчет концентрации взвешенных наносов на входе в расчетную область
        /// </summary>
        /// <param name="Hn"></param>
        protected void CalkS0(double Hn)
        {
            double gamma = (rho_s - rho_w) / rho_w;
            double W_Stocks = gamma * g * d * d / (18 * (nuT[mesh.CountLeft / 4] + nu_mol));
            // динамическая скорость для ровного дна [Белолипецкий]  !!!! нужно uz = sqrt(Tau/rho_w)
            double uz = 1.4 * Math.Sqrt(g * Hn * Params.J); 
            double Ko = Ww / (kappa * uz);
            // Концентрация на дне по Эйнштейну [Белолипецкий]
            double S0 = 2.17 * Ww * d / (Math.Exp(0.39 * gamma * d / Hn) - 1); 
            for (int i = 0; i < mesh.CountLeft; i++)
            {
                int knot = mesh.LeftKnots[i];
                double y = (Y[knot] - Y[0]);
                // по Раузу [Гришанин] 
                if (y == 0)
                    S[knot] = S0;
                else
                    S[knot] = S0 * Math.Pow((Hn - y) * 2 * d / (y * (Hn - 2 * d)), Ko);
            }
        }
        /// <summary>
        /// Расход на вхде
        /// </summary>
        public double Get_Q_in()
        {
            if (U != null)
            {
                Q_in = 0;
                for (int k = 0; k < mesh.CountLeft - 1; k++)
                {
                    int a = mesh.LeftKnots[k];
                    int b = mesh.LeftKnots[k + 1];
                    Q_in += (U[a] + U[b]) * 0.5f * Math.Abs(Y[a] - Y[b]);
                }
                return Q_in;
            }
            else
                return 0;
        }
        /// <summary>
        /// Расход на выхде
        /// </summary>
        public double Get_Q_out()
        {
            if (U != null)
            {
                Q_out = 0;
                for (int k = 1; k < mesh.CountRight - 1; k++)
                {
                    int a = mesh.RightKnots[k];
                    int b = mesh.RightKnots[k + 1];
                    Q_out += (U[a] + U[b]) * 0.5f * Math.Abs(Y[a] - Y[b]);
                }
                return Q_out;
            }
            else
                return 0;
        }

        /// <summary>
        /// Периодические граничные условия
        /// -----------------------------------
        /// Метод переносит профиль с выхода из расчетной области на вход для установления
        /// Справедливо только, когда количество узлов на входной и выходной стенках совпадают
        /// </summary>
        protected void OutflowProfileInflow()
        {
            int lknot = mesh.LeftKnots[0];
            int rknot = mesh.RightKnots[mesh.CountRight - 1]; // берем профиль на выходной границе
            double dx = mesh.CoordsX[mesh.BottomKnots[1]];
            //int rknot = Convert.ToInt32(0.75 / dx - 1) * mesh.CountLeft - 1;
            for (int i = 0; i < mesh.CountLeft; i++)
            {
                //lknot = mesh.LeftKnots[i];
                //rknot = mesh.RightKnots[i];
                //
                U[lknot] = U[rknot];
                V[lknot] = V[rknot];
                //P[lknot] = P[rknot];
                //
                //S[lknot] = S[rknot];
                nuT[lknot] = nuT[rknot];
                K[lknot] = K[rknot];
                //E[lknot] = E[rknot];
                W[lknot] = W[rknot];
                lknot--;
                rknot--;
            }
            // меняем dPdx_in
            //dPdx = 0;
            //int Knot = 0;
            //// -- с середины области
            //double point = 0.45;
            //for (int j = 0; j < mesh.CountBottom; j++)
            //{
            //    Knot = mesh.BottomKnots[j];
            //    if ((point - X[Knot]) < 0.0001)
            //        break;
            //}
            //for (int i = 0; i < mesh.CountLeft; i++)
            //{
            //    dPdx += (P[Knot] - P[Knot - mesh.CountLeft]) / (X[Knot] - X[Knot - mesh.CountLeft]);
            //    Knot++;

            //}
            //dPdx /= mesh.CountRight;
            ////--
            // -- с выходной границы
            //for (int i = 0; i < mesh.CountRight; i++)
            //{
            //    // с сечения на выходе
            //    Knot = mesh.RightKnots[i];
            //    dPdx += (P[Knot] - P[Knot - mesh.CountLeft]) / (X[Knot] - X[Knot - mesh.CountLeft]);

            //}
            ////--
            //dPdx /= mesh.CountRight;
            // -- с середины выходной границы
            //Knot = mesh.RightKnots[mesh.CountLeft/2];
            //dPdx = (P[Knot] - P[Knot - mesh.CountLeft]) / (X[Knot] - X[Knot - mesh.CountLeft]);
            ////
            //for (int i = 0; i < mesh.CountLeft - 1; i++)
            //{
            //    double y1 = Y[mesh.LeftKnots[i]];
            //    double y2 = Y[mesh.LeftKnots[i + 1]];
            //    //
            //    double r3 = (double)(-Math.Abs(y1 - y2) / 2.0 * (dPdx) * (-1));
            //    //
            //    BV[i] += r3;
            //    BV[i + 1] += r3;
            //}
        }

        #endregion

        #region Расчет неизвестных
        /// <summary>
        /// Расчет k и e в узлах 
        /// </summary>
        /// <param name="iteration"></param>
        /// <returns></returns>
        private int QHDKE(int iteration)
        {
            #region Расчет k и e во внутренних узлах без узлов WallKnots
            //OrderablePartitioner<Tuple<int, int>> rangePartitioner3 = Partitioner.Create(0, mesh.CV2.Length);
            Parallel.ForEach(OrdPart_CV2, (range, loopState) =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                        //for (int i = 0; i < mesh.CV2.Length; i++)
                {
                    double LsummK = 0;//потоки k 
                    double LsummE = 0;//потоки e 
                    double LLrightK = 0;//потоки k 
                    double LLrightE = 0;//потоки e 
                                        //
                    double ldudx = 0, ldudy = 0, ldvdx = 0, ldvdy = 0;
                            //
                    int p0 = wrapper.CV2[i][0];
                            // убираем из расчета узлы, в которых устанавливается WallFunc

                    int jj = wrapper.CV2[i].Length - 1;//количество КО, связанных с данным узлом
                    for (int j = 0; j < jj; j++)
                    {
                        double _lx10 = wrapper.Lx10[p0][j]; double _lx32 = wrapper.Lx32[p0][j];
                        double _ly01 = wrapper.Ly01[p0][j]; double _ly23 = wrapper.Ly23[p0][j];
                                //площадь
                        double LS = wrapper.S[p0][j];
                                //сосоедние элементы
                        int Lv1 = wrapper.CV2[i][(j + 1) % jj + 1];
                        int Lv2 = wrapper.CV2[i][j + 1];
                                //вторая точка общей грани
                        int Lp1 = wrapper.P1[p0][j];
                                //находим значения функций в центрах масс 1ого и 2ого треугольника как средние значения по элементу
                        uint[] Knots1 = mesh.AreaElems[Lv1];
                        uint Lt1 = Knots1[0]; uint Lt2 = Knots1[1]; uint Lt3 = Knots1[2];
                        double LUc1 = (U[Lt1] + U[Lt2] + U[Lt3]) / 3.0;
                        double LVc1 = (V[Lt1] + V[Lt2] + V[Lt3]) / 3.0;
                        double LPc1 = (P[Lt1] + P[Lt2] + P[Lt3]) / 3.0;
                        double LKc1 = (K[Lt1] + K[Lt2] + K[Lt3]) / 3.0;
                        double LEc1 = (E[Lt1] + E[Lt2] + E[Lt3]) / 3.0;
                        uint[] Knots2 = mesh.AreaElems[Lv2];
                        uint Lz1 = Knots2[0]; uint Lz2 = Knots2[1]; uint Lz3 = Knots2[2];
                        double LUc2 = (U[Lz1] + U[Lz2] + U[Lz3]) / 3.0;
                        double LVc2 = (V[Lz1] + V[Lz2] + V[Lz3]) / 3.0;
                        double LPc2 = (P[Lz1] + P[Lz2] + P[Lz3]) / 3.0;
                        double LKc2 = (K[Lz1] + K[Lz2] + K[Lz3]) / 3.0;
                        double LEc2 = (E[Lz1] + E[Lz2] + E[Lz3]) / 3.0;
                                //значения производных в точке пересечения граней
                        double Ls2 = 2 * LS;
                        double Ldudx = ((LUc1 - LUc2) * _ly01 + (U[Lp1] - U[p0]) * _ly23) / Ls2;
                        double Ldudy = ((LUc1 - LUc2) * _lx10 + (U[Lp1] - U[p0]) * _lx32) / Ls2;
                        double Ldvdx = ((LVc1 - LVc2) * _ly01 + (V[Lp1] - V[p0]) * _ly23) / Ls2;
                        double Ldvdy = ((LVc1 - LVc2) * _lx10 + (V[Lp1] - V[p0]) * _lx32) / Ls2;
                        double Ldpdx = ((LPc1 - LPc2) * _ly01 + (P[Lp1] - P[p0]) * _ly23) / Ls2;
                        double Ldpdy = ((LPc1 - LPc2) * _lx10 + (P[Lp1] - P[p0]) * _lx32) / Ls2;
                                //
                        double Ldkdx = ((LKc1 - LKc2) * _ly01 + (K[Lp1] - K[p0]) * _ly23) / Ls2;
                        double Ldkdy = ((LKc1 - LKc2) * _lx10 + (K[Lp1] - K[p0]) * _lx32) / Ls2;
                        double Ldedx = ((LEc1 - LEc2) * _ly01 + (E[Lp1] - E[p0]) * _ly23) / Ls2;
                        double Ldedy = ((LEc1 - LEc2) * _lx10 + (E[Lp1] - E[p0]) * _lx32) / Ls2;
                                //внешняя нормаль к грани КО (контуру КО)
                        double Lnx = wrapper.Nx[p0][j]; double Lny = wrapper.Ny[p0][j];
                                ////значение функций в точке пересечения грани КО и основной грани
                        double Lalpha = wrapper.Alpha[p0][j];
                        double LUcr = Lalpha * U[p0] + (1 - Lalpha) * U[Lp1];
                        double LVcr = Lalpha * V[p0] + (1 - Lalpha) * V[Lp1];
                        double LKcr = Lalpha * K[p0] + (1 - Lalpha) * K[Lp1];
                        double LEcr = Lalpha * E[p0] + (1 - Lalpha) * E[Lp1];
                        double LNucrT = Lalpha * nuT[p0] + (1 - Lalpha) * nuT[Lp1];
                                //длина текущего фрагмента внешнего контура КО
                        double LLk = wrapper.Lk[p0][j];
                                //расчет потоков
                        double wx = Params.tau * (LUcr * Ldudx + LVcr * Ldudy + 1.0 / rho_w * Ldpdx + 2.0 / 3.0 * Ldkdx);
                        double wy = Params.tau * (LUcr * Ldvdx + LVcr * Ldvdy + 1.0 / rho_w * Ldpdy + 2.0 / 3.0 * Ldkdy);
                        double wk = Params.tau * (LUcr * Ldkdx + LVcr * Ldkdy - (LNucrT * (2 * Ldudx * Ldudx + (Ldvdx + Ldudy) * (Ldvdx + Ldudy) + 2 * Ldvdy * Ldvdy) - LEcr));
                        double we = Params.tau * (LUcr * Ldedx + LVcr * Ldedy - (LEcr / LKcr * (C_e1 * LNucrT * (2 * Ldudx * Ldudx + (Ldvdx + Ldudy) * (Ldvdx + Ldudy) + 2 * Ldvdy * Ldvdy)) - C_e2 * LEcr));
                                //
                        double LconvK = -LUcr * LKcr * Lnx - LVcr * LKcr * Lny;
                        double LdiffK = (LNucrT / sigma_k + nu_mol) * (Ldkdx * Lnx + Ldkdy * Lny);
                        double LregK = LKcr * wx * Lnx + LKcr * wy * Lny + LUcr * wk * Lnx + LVcr * wk * Lny;
                                //
                        LsummK += (LconvK + LdiffK + LregK) * LLk;
                                //                  
                        double LconvE = -LUcr * LEcr * Lnx - LVcr * LEcr * Lny;
                        double LdiffE = (LNucrT / sigma_e + nu_mol) * (Ldedx * Lnx + Ldedy * Lny);
                        double LregE = LEcr * wx * Lnx + LEcr * wy * Lny + LUcr * we * Lnx + LVcr * we * Lny;
                                // 
                        LsummE += (LconvE + LdiffE + LregE) * LLk;
                                //
                                // компоненты производных для Pk
                        ldudx += LUcr * Lnx * LLk;
                        ldudy += LUcr * Lny * LLk;
                        ldvdx += LVcr * Lnx * LLk;
                        ldvdy += LVcr * Lny * LLk;
                                //запись в массивы
                                //ConvK[p0] += LconvK * LLk;
                                //ConvE[p0] += LconvE * LLk;
                                //DiffK[p0] += LdiffK * LLk;
                                //DiffE[p0] += LdiffE * LLk;
                                //RegK[p0] += (LUcr * wk * Lnx + LVcr * wk * Lny) * LLk;
                                //RegE[p0] += (LUcr * we * Lnx + LVcr * we * Lny) * LLk;
                                //RegK[p0] += LregK * LLk;
                                //RegE[p0] += LregE * LLk;
                    }
                            //
                    ldudx /= wrapper.S0[p0];
                    ldudy /= wrapper.S0[p0];
                    ldvdx /= wrapper.S0[p0];
                    ldvdy /= wrapper.S0[p0];
                            //double tPk = 1.4142135623730950488016887242097 * nuT[p0] * (ldudx * ldudx + (ldvdx + ldudy) * (ldvdx + ldudy) + ldvdy * ldvdy); //- 2.0 / 3.0 * K[p0];
                    double tPk = nuT[p0] * (2 * ldudx * ldudx + (ldvdx + ldudy) * (ldvdx + ldudy) + 2 * ldvdy * ldvdy);
                    Pk[p0] = tPk;//Math.Min(tPk, 10 * E[p0]);
                    LLrightK = (Pk[p0] - E[p0]);
                    LLrightE = E[p0] / K[p0] * (C_e1 * Pk[p0] - C_e2 * E[p0]);
                            //
                    K[p0] = K[p0] + dt / wrapper.S0[p0] * LsummK + dt * LLrightK;
                    E[p0] = E[p0] + dt / wrapper.S0[p0] * LsummE + dt * LLrightE;
                            ////
                            //ConvK[p0] /= mesh.S0[p0];
                            //ConvE[p0] /= mesh.S0[p0];
                            //DiffK[p0] /= mesh.S0[p0];
                            //DiffE[p0] /= mesh.S0[p0];
                            //RegK[p0] /= mesh.S0[p0];
                            //RegE[p0] /= mesh.S0[p0];
                            //
                    rightK[p0] = LLrightK;
                    rightE[p0] = LLrightE;
                    ReT[p0] = K[p0] * K[p0] / E[p0] / nu_mol;
                            //
                    if (double.IsNaN(E[p0]))
                    {
                        err = " KE в бесконечность";
                        iteration = Params.iter;
                        break;
                                //return iteration;
                    }
                            //}
                }
            });
            #endregion
            //
            #region Расчет k и e в узлах WallKnots
            //rangePartitioner3 = Partitioner.Create(0, mesh.CV_WallKnots.Length);
            Parallel.ForEach(OrdPart_CV_Wall, (range, loopState) =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                //for (int i = 0; i < mesh.CV_WallKnots.Length; i++)
                {
                    double LsummK = 0;//потоки k 
                    double LLrightK = 0;//потоки k  
                                        //
                    int p0 = wrapper.CV_WallKnots[i][0];
                    int jj = wrapper.CV_WallKnots[i].Length - 1;//количество КО, связанных с данным узлом
                                                             //

                    for (int j = 0; j < jj; j++)
                    {
                        double _lx10 = wrapper.Lx10[p0][j]; double _lx32 = wrapper.Lx32[p0][j];
                        double _ly01 = wrapper.Ly01[p0][j]; double _ly23 = wrapper.Ly23[p0][j];
                        //площадь
                        double LS = wrapper.S[p0][j];
                        //сосоедние элементы
                        int Lv1 = wrapper.CV_WallKnots[i][(j + 1) % jj + 1];
                        int Lv2 = wrapper.CV_WallKnots[i][j + 1];
                        //вторая точка общей грани
                        int Lp1 = wrapper.P1[p0][j];
                        //находим значения функций в центрах масс 1ого и 2ого треугольника как средние значения по элементу
                        uint[] Knots1 = mesh.AreaElems[Lv1];
                        uint Lt1 = Knots1[0]; uint Lt2 = Knots1[1]; uint Lt3 = Knots1[2];
                        double LUc1 = (U[Lt1] + U[Lt2] + U[Lt3]) / 3.0;
                        double LVc1 = (V[Lt1] + V[Lt2] + V[Lt3]) / 3.0;
                        double LPc1 = (P[Lt1] + P[Lt2] + P[Lt3]) / 3.0;
                        double LKc1 = (K[Lt1] + K[Lt2] + K[Lt3]) / 3.0;
                        double LEc1 = (E[Lt1] + E[Lt2] + E[Lt3]) / 3.0;
                        uint[] Knots2 = mesh.AreaElems[Lv2];
                        uint Lz1 = Knots2[0]; uint Lz2 = Knots2[1]; uint Lz3 = Knots2[2];
                        double LUc2 = (U[Lz1] + U[Lz2] + U[Lz3]) / 3.0;
                        double LVc2 = (V[Lz1] + V[Lz2] + V[Lz3]) / 3.0;
                        double LPc2 = (P[Lz1] + P[Lz2] + P[Lz3]) / 3.0;
                        double LKc2 = (K[Lz1] + K[Lz2] + K[Lz3]) / 3.0;
                        double LEc2 = (E[Lz1] + E[Lz2] + E[Lz3]) / 3.0;
                        //значения производных в точке пересечения граней
                        double Ls2 = 2 * LS;
                        double Ldudx = ((LUc1 - LUc2) * _ly01 + (U[Lp1] - U[p0]) * _ly23) / Ls2;
                        double Ldudy = ((LUc1 - LUc2) * _lx10 + (U[Lp1] - U[p0]) * _lx32) / Ls2;
                        double Ldvdx = ((LVc1 - LVc2) * _ly01 + (V[Lp1] - V[p0]) * _ly23) / Ls2;
                        double Ldvdy = ((LVc1 - LVc2) * _lx10 + (V[Lp1] - V[p0]) * _lx32) / Ls2;
                        double Ldpdx = ((LPc1 - LPc2) * _ly01 + (P[Lp1] - P[p0]) * _ly23) / Ls2;
                        double Ldpdy = ((LPc1 - LPc2) * _lx10 + (P[Lp1] - P[p0]) * _lx32) / Ls2;
                        //
                        double Ldkdx = ((LKc1 - LKc2) * _ly01 + (K[Lp1] - K[p0]) * _ly23) / Ls2;
                        double Ldkdy = ((LKc1 - LKc2) * _lx10 + (K[Lp1] - K[p0]) * _lx32) / Ls2;
                        //внешняя нормаль к грани КО (контуру КО)
                        double Lnx = wrapper.Nx[p0][j]; double Lny = wrapper.Ny[p0][j];
                        ////значение функций в точке пересечения грани КО и основной грани
                        double Lalpha = wrapper.Alpha[p0][j];
                        double LUcr = Lalpha * U[p0] + (1 - Lalpha) * U[Lp1];
                        double LVcr = Lalpha * V[p0] + (1 - Lalpha) * V[Lp1];
                        double LKcr = Lalpha * K[p0] + (1 - Lalpha) * K[Lp1];
                        double LEcr = Lalpha * E[p0] + (1 - Lalpha) * E[Lp1];
                        double LNucrT = Lalpha * nuT[p0] + (1 - Lalpha) * nuT[Lp1];
                        //длина текущего фрагмента внешнего контура КО
                        double LLk = wrapper.Lk[p0][j];
                        //расчет потоков
                        double wx = Params.tau * (LUcr * Ldudx + LVcr * Ldudy + 1.0 / rho_w * Ldpdx + 2.0 / 3.0 * Ldkdx);
                        double wy = Params.tau * (LUcr * Ldvdx + LVcr * Ldvdy + 1.0 / rho_w * Ldpdy + 2.0 / 3.0 * Ldkdy);
                        double wk = Params.tau * (LUcr * Ldkdx + LVcr * Ldkdy - (LNucrT * (2 * Ldudx * Ldudx + (Ldvdx + Ldudy) * (Ldvdx + Ldudy) + 2 * Ldvdy * Ldvdy) - LEcr));
                        //
                        double LconvK = -LUcr * LKcr * Lnx - LVcr * LKcr * Lny;
                        double LdiffK = (LNucrT / sigma_k + nu_mol) * (Ldkdx * Lnx + Ldkdy * Lny);
                        double LregK = LKcr * wx * Lnx + LKcr * wy * Lny + LUcr * wk * Lnx + LVcr * wk * Lny;
                        //
                        LsummK += (LconvK + LdiffK + LregK) * LLk;
                        //
                        //запись в массивы
                        //ConvK[p0] += LconvK * LLk;
                        //DiffK[p0] += LdiffK * LLk;
                        //RegK[p0] += (LUcr * wk * Lnx + LVcr * wk * Lny) * LLk;
                        //RegK[p0] += LregK * LLk;
                    }
                    //
                    double y_p_plus = cm14 * Math.Sqrt(K[p0]) * wrapper.CV_WallKnotsDistance[i] / nu_mol;
                    Pk[p0] = 0;
                    if (y_p_plus > y_p_0)
                    {
                        E[p0] = cm14 * cm14 * cm14 * Math.Pow(K[p0], 1.5) / kappa / wrapper.CV_WallKnotsDistance[i];
                        Pk[p0] = E[p0];
                    }
                    else
                        E[p0] = 2.0 * K[p0] / wrapper.CV_WallKnotsDistance[i] / wrapper.CV_WallKnotsDistance[i] * nu_mol;
                    //
                    LLrightK = (Pk[p0] - E[p0]);
                    //
                    K[p0] = K[p0] + dt / wrapper.S0[p0] * LsummK + dt * LLrightK;
                    //
                    //ConvK[p0] /= mesh.S0[p0];
                    //ConvE[p0] /= mesh.S0[p0];
                    //DiffK[p0] /= mesh.S0[p0];
                    //
                    rightK[p0] = LLrightK;
                    ReT[p0] = K[p0] * K[p0] / E[p0] / nu_mol;
                    //
                    if (double.IsNaN(K[p0]))
                    {
                        err = " KE WallKnots в бесконечность";
                        iteration = Params.iter;
                        break;
                        //return iteration;
                    }
                    //}

                }

            });
            #endregion

            //ГУ справа снос
            int Bknot = 0;
            for (int i = 0; i < mesh.CountRight; i++)
            {
                Bknot = mesh.RightKnots[i];
                //
                K[Bknot] = K[Bknot - mesh.CountRight];
                E[Bknot] = E[Bknot - mesh.CountRight];
            }
            //
            if (Params.surf_flag == true)
            {
                int knot;
                for (int i = 1; i < mesh.CountTop; i++)
                {
                    knot = mesh.TopKnots[i];
                    K[knot] = K[knot - 1];
                    E[knot] = E[knot - 1];
                }
            }
            // вычисление напряжения по пристеночной функции
            for (int i = 0; i < wrapper.CV_WallKnots.Length; i++)
            {
                int knot = wrapper.CV_WallKnots[i][0];
                //CV_WallTau[i] = WallFuncSharpPlus(knot, mesh.CV_WallKnotsDistance[i]);//шероховатая стенка по Луцкому, установка по Снегиреву
                //CV_WallTau[i] = WallFuncSnegirev(knot, mesh.CV_WallKnotsDistance[i]);// гладкая стенка по Снегиреву
                //CV_WallTau[i] = WallFuncPlus(knot, mesh.CV_WallKnotsDistance[i]);// гладкая стенка по Луцкому
                CV_WallTau[i] = WallFunc(knot, wrapper.CV_WallKnotsDistance[i]);// гладкая стенка по Волкову упрощ
                //CV_WallTau[i] = WallFuncNewton(knot, mesh.CV_WallKnotsDistance[i]);// гладкая стенка по Волкову Ньютон
            }
            // вычисление напряжения по пристеночной функции для отрисовки
            for (int i = 0; i < mesh.CountBottom; i++)
            {
                int knot = mesh.BottomKnots[i] + 1;
                //
                //BTauC[i] = WallFuncSharpPlus(knot, mesh.BWallDistance[i]);//шероховатая стенка по Луцкому, установка по Снегиреву
                //BTauC[i] = WallFuncSnegirev(knot, mesh.BWallDistance[i]);// гладкая стенка по Снегиреву
                //BTauC[i] = WallFuncPlus(knot, mesh.BWallDistance[i]);// гладкая стенка по Луцкому
                BTauC[i] = WallFunc(knot, wrapper.BWallDistance[i]);// гладкая стенка по Волкову упрощ
                //BTauC[i] = WallFuncNewton(knot, mesh.BWallDistance[i]);// гладкая стенка по Волкову Ньютон
            }
            for (int i = 0; i < mesh.CountBottom - 1; i++)
            {
                BTau[i] = 2 * BTauC[i] * BTauC[i + 1] / (BTauC[i] + BTauC[i + 1]);
            }
            BTau[mesh.CountBottom - 1] = BTau[mesh.CountBottom - 2];
            //
            // поправка по пристеночной функции для верхней границы
            if (Params.surf_flag!=true)
            {
                for (int i = 0; i < mesh.CountTop; i++)
                {
                    int knot = mesh.TopKnots[i] - 1;
                    //
                    //TTauC[i] = WallFuncSnegirev(knot, mesh.TWallDistance[i]);
                    TTauC[i] = WallFunc(knot, wrapper.TWallDistance[i]);// гладкая стенка по Волкову упрощ
                    //TTauC[i] = WallFuncSharpPlus(knot, mesh.TWallDistance[i]);//шероховатая стенка по Луцкому, установка по Снегиреву
                }
                for (int i = 0; i < mesh.CountTop - 1; i++)
                {
                    TTau[i] = 2 * TTauC[i] * TTauC[i + 1] / (TTauC[i] + TTauC[i + 1]);
                }
                TTau[mesh.CountTop - 1] = TTau[mesh.CountTop - 2];
            }
            //
            return iteration;
        }
        /// <summary>
        /// Расчет k-e вместе с пристеночной функцией по Жлуткову 2015 Пристеночные функции для высокорейнольдсовых расчетов в программном комплексеFlowVision
        /// Сначала ставится значение при стенке, потом идет расчет k-e равновесный (в прилегающей к границе ко не считает)
        /// очень осциллирует решение -- можно поправить по Снегиреву по технике установки ГУ
        /// </summary>
        /// <param name="iteration"></param>
        /// <returns></returns>
        private int QHDKE_G(int iteration)
        {
            //
            for (int i = 0; i < mesh.CountBottom; i++)
            {
                int knot = mesh.BottomKnots[i] + 1;
                double y = Y[knot] - Y[knot - 1];
                double dpdx = 0;
                if (i != mesh.CountBottom - 1)
                    dpdx = (P[knot + mesh.CountLeft] - P[knot]) / (X[mesh.CountLeft] - X[0]);
                else
                    dpdx = (P[knot] - P[knot - mesh.CountLeft]) / (X[mesh.CountLeft] - X[0]);
                if (dudy[i] == 0)
                    dudy[i] = (U[knot] - U[knot - 1]) / (Y[knot] - Y[knot - 1]);
                double tauw = (nu_mol + nuT[knot]) * dudy[i];
                //
                double utau = (tauw / rho_w);
                double y1_plus = rho_w * utau * y / mu;
                double l1 = 0.41 * y * (1 - Math.Exp(-y1_plus / 25.0));
                //
                double up = Math.Pow(mu / rho_w / rho_w * Math.Abs(dpdx), 1.0 / 3.0);
                double y2_plus = rho_w * up * y / mu;
                double l2 = 0.2 * y * Math.Sqrt(y2_plus) * (1 - Math.Exp(-y2_plus / 13.6));
                double ll2 = Math.Max(Math.Sign(tauw) * l1 * l1 + Math.Sign(dpdx) * l2 * l2, 0);
                //
                double mut = 1.0 / 2.0 * (-mu + mu * (Math.Sqrt(1 + 4 * rho_w * ll2 / mu / mu * (tauw + dpdx * y))));
                dudy[i] = mut / rho_w / ll2 / 2.0;

                //
                double a = 0, b = y;
                double ab = (a + b) / 2.0;
                double l1_plusa = kappa * a * (1 - Math.Exp(-a / 25.0));
                double l1_plusb = kappa * b * (1 - Math.Exp(-b / 25.0));
                double l1_plusab = kappa * ab * (1 - Math.Exp(-ab / 25.0));
                //
                double l2_plusa = kappa * a * Math.Sqrt(a) * (1 - Math.Exp(-a / 13.6));
                double l2_plusb = kappa * b * Math.Sqrt(b) * (1 - Math.Exp(-b / 13.6));
                double l2_plusab = kappa * ab * Math.Sqrt(ab) * (1 - Math.Exp(-ab / 13.6));
                //
                double ua = (-1 + Math.Sqrt(1 + 4 * l1_plusa * l1_plusa)) / 2.0 / (l1_plusa * l1_plusa + 0.000000001);
                double ub = (-1 + Math.Sqrt(1 + 4 * l1_plusb * l1_plusb)) / 2.0 / l1_plusb / l1_plusb;
                double uab = (-1 + Math.Sqrt(1 + 4 * l1_plusab * l1_plusab)) / 2.0 / l1_plusab / l1_plusab;
                double u1_plus = (b - a) / 6.0 * (ua + 4 * uab + ub);
                //
                ua = (-1 + Math.Sqrt(1 + 4 * l2_plusa * l2_plusa * a)) / 2.0 / (l2_plusa * l2_plusa + 0.000000001);
                ub = (-1 + Math.Sqrt(1 + 4 * l2_plusb * l2_plusb * b)) / 2.0 / l2_plusb / l2_plusb;
                uab = (-1 + Math.Sqrt(1 + 4 * l2_plusab * l2_plusab * ab)) / 2.0 / l2_plusab / l2_plusab;
                double u2_plus = (b - a) / 6.0 * (ua + 4 * uab + ub);
                //
                double u_plus = Math.Sign(tauw) * u1_plus + Math.Sign(dpdx) * u2_plus;
                double ux = 0;
                if (U[knot] >= 0)
                    ux = u_plus * utau;
                else
                    ux = u_plus * up;
                //
                double utaup = Math.Sqrt(Math.Max(Math.Sign(tauw) * utau * utau + Math.Sign(dpdx) * up * up * up / nu_mol * y, 0));
                double ytaup_plus = rho_w * utaup * y / mu;
                double kc = utaup * utaup / (0.3 + 1.0 / (0.003 * Math.Pow(ytaup_plus, 3.5)));
                double kc2 = utau * utau / 0.3;
                //
                double uk = cm14 * Math.Sqrt(kc);
                double yk = rho_w * uk * y / mu;
                double le = 0.41 * y * (1 - Math.Exp(-yk / 15.0));
                double ec = uk * uk * uk / le;
                //
                nuT[knot] = mut / rho_w;
                //U[knot] = ux;
                K[knot] = kc;
                E[knot] = ec;
                BTau[i] = tauw;
            }
            if (Params.surf_flag != true)
            {
                for (int i = 0; i < mesh.CountTop; i++)
                {
                    int knot = mesh.TopKnots[i] - 1;
                    double y = Y[knot + 1] - Y[knot];
                    double dpdx = 0;
                    if (i != mesh.CountBottom - 1)
                        dpdx = (P[knot + mesh.CountLeft] - P[knot]) / (X[mesh.CountLeft] - X[0]);
                    else
                        dpdx = (P[knot] - P[knot - mesh.CountLeft]) / (X[mesh.CountLeft] - X[0]);
                    if (dudy[i] == 0)
                        dudy[i] = (U[knot + 1] - U[knot]) / (Y[knot + 1] - Y[knot]);
                    double tauw = (nu_mol + nuT[knot]) * dudy[i];
                    //
                    double utau = (tauw / rho_w);
                    double y1_plus = rho_w * utau * y / mu;
                    double l1 = 0.41 * y * (1 - Math.Exp(-y1_plus / 25.0));
                    //
                    double up = Math.Pow(mu / rho_w / rho_w * Math.Abs(dpdx), 1.0 / 3.0);
                    double y2_plus = rho_w * up * y / mu;
                    double l2 = 0.2 * y * Math.Sqrt(y2_plus) * (1 - Math.Exp(-y2_plus / 13.6));
                    double ll2 = Math.Max(Math.Sign(tauw) * l1 * l1 + Math.Sign(dpdx) * l2 * l2, 0);
                    //
                    double mut = 1.0 / 2.0 * (-mu + mu * (Math.Sqrt(1 + 4 * rho_w * ll2 / mu / mu * (tauw + dpdx * y))));
                    dudy[i] = mut / rho_w / ll2 / 2.0;

                    //
                    double a = 0, b = y;
                    double ab = (a + b) / 2.0;
                    double l1_plusa = kappa * a * (1 - Math.Exp(-a / 25.0));
                    double l1_plusb = kappa * b * (1 - Math.Exp(-b / 25.0));
                    double l1_plusab = kappa * ab * (1 - Math.Exp(-ab / 25.0));
                    //
                    double l2_plusa = kappa * a * Math.Sqrt(a) * (1 - Math.Exp(-a / 13.6));
                    double l2_plusb = kappa * b * Math.Sqrt(b) * (1 - Math.Exp(-b / 13.6));
                    double l2_plusab = kappa * ab * Math.Sqrt(ab) * (1 - Math.Exp(-ab / 13.6));
                    //
                    double ua = (-1 + Math.Sqrt(1 + 4 * l1_plusa * l1_plusa)) / 2.0 / (l1_plusa * l1_plusa + 0.000000001);
                    double ub = (-1 + Math.Sqrt(1 + 4 * l1_plusb * l1_plusb)) / 2.0 / l1_plusb / l1_plusb;
                    double uab = (-1 + Math.Sqrt(1 + 4 * l1_plusab * l1_plusab)) / 2.0 / l1_plusab / l1_plusab;
                    double u1_plus = (b - a) / 6.0 * (ua + 4 * uab + ub);
                    //
                    ua = (-1 + Math.Sqrt(1 + 4 * l2_plusa * l2_plusa * a)) / 2.0 / (l2_plusa * l2_plusa + 0.000000001);
                    ub = (-1 + Math.Sqrt(1 + 4 * l2_plusb * l2_plusb * b)) / 2.0 / l2_plusb / l2_plusb;
                    uab = (-1 + Math.Sqrt(1 + 4 * l2_plusab * l2_plusab * ab)) / 2.0 / l2_plusab / l2_plusab;
                    double u2_plus = (b - a) / 6.0 * (ua + 4 * uab + ub);
                    //
                    double u_plus = Math.Sign(tauw) * u1_plus + Math.Sign(dpdx) * u2_plus;
                    double ux = 0;
                    if (U[knot] >= 0)
                        ux = u_plus * utau;
                    else
                        ux = u_plus * up;
                    //
                    double utaup = Math.Sqrt(Math.Max(Math.Sign(tauw) * utau * utau + Math.Sign(dpdx) * up * up * up / nu_mol * y, 0));
                    double ytaup_plus = rho_w * utaup * y / mu;
                    double kc = utaup * utaup / (0.3 + 1.0 / (0.003 * Math.Pow(ytaup_plus, 3.5)));
                    double kc2 = utau * utau / 0.3;
                    //
                    double uk = cm14 * Math.Sqrt(kc);
                    double yk = rho_w * uk * y / mu;
                    double le = 0.41 * y * (1 - Math.Exp(-yk / 15.0));
                    double ec = uk * uk * uk / le;
                    //
                    nuT[knot] = mut / rho_w;
                    //U[knot] = ux;
                    K[knot] = kc;
                    E[knot] = ec;
                    BTau[i] = tauw;
                }
            }
            //
            for (int i = 0; i < mesh.CountBottom - 1; i++)
            {
                BTau[i] = 2 * BTauC[i] * BTauC[i + 1] / (BTauC[i] + BTauC[i + 1]);
            }
            BTau[mesh.CountBottom - 1] = BTau[mesh.CountBottom - 2];
            //
            #region расчет k-e в области без пристенки
            Parallel.ForEach(OrdPart_CV2,
                    (range, loopState) =>
                    {
                        for (int i = range.Item1; i < range.Item2; i++)
                        //for (int i = 0; i < mesh.CV2.Length; i++)
                        {
                            double LsummK = 0;//потоки k 
                            double LsummE = 0;//потоки e 
                            double LLrightK = 0;//потоки k 
                            double LLrightE = 0;//потоки e 
                            //
                            double ldudx = 0, ldudy = 0, ldvdx = 0, ldvdy = 0;
                            //
                            int p0 = wrapper.CV2[i][0];
                            // убираем из расчета узлы, в которых устанавливается WallFunc

                            int jj = wrapper.CV2[i].Length - 1;//количество КО, связанных с данным узлом
                            for (int j = 0; j < jj; j++)
                            {
                                double _lx10 = wrapper.Lx10[p0][j]; double _lx32 = wrapper.Lx32[p0][j];
                                double _ly01 = wrapper.Ly01[p0][j]; double _ly23 = wrapper.Ly23[p0][j];
                                //площадь
                                double LS = wrapper.S[p0][j];
                                //сосоедние элементы
                                int Lv1 = wrapper.CV2[i][(j + 1) % jj + 1];
                                int Lv2 = wrapper.CV2[i][j + 1];
                                //вторая точка общей грани
                                int Lp1 = wrapper.P1[p0][j];
                                //находим значения функций в центрах масс 1ого и 2ого треугольника как средние значения по элементу
                                uint[] Knots1 = mesh.AreaElems[Lv1];
                                uint Lt1 = Knots1[0]; uint Lt2 = Knots1[1]; uint Lt3 = Knots1[2];
                                double LUc1 = (U[Lt1] + U[Lt2] + U[Lt3]) / 3.0;
                                double LVc1 = (V[Lt1] + V[Lt2] + V[Lt3]) / 3.0;
                                double LPc1 = (P[Lt1] + P[Lt2] + P[Lt3]) / 3.0;
                                double LKc1 = (K[Lt1] + K[Lt2] + K[Lt3]) / 3.0;
                                double LEc1 = (E[Lt1] + E[Lt2] + E[Lt3]) / 3.0;
                                uint[] Knots2 = mesh.AreaElems[Lv2];
                                uint Lz1 = Knots2[0]; uint Lz2 = Knots2[1]; uint Lz3 = Knots2[2];
                                double LUc2 = (U[Lz1] + U[Lz2] + U[Lz3]) / 3.0;
                                double LVc2 = (V[Lz1] + V[Lz2] + V[Lz3]) / 3.0;
                                double LPc2 = (P[Lz1] + P[Lz2] + P[Lz3]) / 3.0;
                                double LKc2 = (K[Lz1] + K[Lz2] + K[Lz3]) / 3.0;
                                double LEc2 = (E[Lz1] + E[Lz2] + E[Lz3]) / 3.0;
                                //значения производных в точке пересечения граней
                                double Ls2 = 2 * LS;
                                double Ldudx = ((LUc1 - LUc2) * _ly01 + (U[Lp1] - U[p0]) * _ly23) / Ls2;
                                double Ldudy = ((LUc1 - LUc2) * _lx10 + (U[Lp1] - U[p0]) * _lx32) / Ls2;
                                double Ldvdx = ((LVc1 - LVc2) * _ly01 + (V[Lp1] - V[p0]) * _ly23) / Ls2;
                                double Ldvdy = ((LVc1 - LVc2) * _lx10 + (V[Lp1] - V[p0]) * _lx32) / Ls2;
                                double Ldpdx = ((LPc1 - LPc2) * _ly01 + (P[Lp1] - P[p0]) * _ly23) / Ls2;
                                double Ldpdy = ((LPc1 - LPc2) * _lx10 + (P[Lp1] - P[p0]) * _lx32) / Ls2;
                                //
                                double Ldkdx = ((LKc1 - LKc2) * _ly01 + (K[Lp1] - K[p0]) * _ly23) / Ls2;
                                double Ldkdy = ((LKc1 - LKc2) * _lx10 + (K[Lp1] - K[p0]) * _lx32) / Ls2;
                                double Ldedx = ((LEc1 - LEc2) * _ly01 + (E[Lp1] - E[p0]) * _ly23) / Ls2;
                                double Ldedy = ((LEc1 - LEc2) * _lx10 + (E[Lp1] - E[p0]) * _lx32) / Ls2;
                                //внешняя нормаль к грани КО (контуру КО)
                                double Lnx = wrapper.Nx[p0][j]; double Lny = wrapper.Ny[p0][j];
                                ////значение функций в точке пересечения грани КО и основной грани
                                double Lalpha = wrapper.Alpha[p0][j];
                                double LUcr = Lalpha * U[p0] + (1 - Lalpha) * U[Lp1];
                                double LVcr = Lalpha * V[p0] + (1 - Lalpha) * V[Lp1];
                                double LKcr = Lalpha * K[p0] + (1 - Lalpha) * K[Lp1];
                                double LEcr = Lalpha * E[p0] + (1 - Lalpha) * E[Lp1];
                                double LNucrT = Lalpha * nuT[p0] + (1 - Lalpha) * nuT[Lp1];
                                //длина текущего фрагмента внешнего контура КО
                                double LLk = wrapper.Lk[p0][j];
                                //расчет потоков
                                double wx = Params.tau * (LUcr * Ldudx + LVcr * Ldudy + 1.0 / rho_w * Ldpdx);
                                double wy = Params.tau * (LUcr * Ldvdx + LVcr * Ldvdy + 1.0 / rho_w * Ldpdy);
                                double wk = Params.tau * (LUcr * Ldkdx + LVcr * Ldkdy - (LNucrT * (2 * Ldudx * Ldudx + (Ldvdx + Ldudy) * (Ldvdx + Ldudy) + 2 * Ldvdy * Ldvdy)) - LEcr);
                                double we = Params.tau * (LUcr * Ldedx + LVcr * Ldedy - (LEcr / LKcr * (C_e1 * LNucrT * (2 * Ldudx * Ldudx + (Ldvdx + Ldudy) * (Ldvdx + Ldudy) + 2 * Ldvdy * Ldvdy)) - C_e2 * LEcr));
                                //
                                double LconvK = -LUcr * LKcr * Lnx - LVcr * LKcr * Lny;
                                double LdiffK = (LNucrT / sigma_k + nu_mol) * (Ldkdx * Lnx + Ldkdy * Lny);
                                double LregK = LKcr * wx * Lnx + LKcr * wy * Lny + LUcr * wk * Lnx + LVcr * wk * Lny;
                                //
                                LsummK += (LconvK + LdiffK + LregK) * LLk;
                                //                  
                                double LconvE = -LUcr * LEcr * Lnx - LVcr * LEcr * Lny;
                                double LdiffE = (LNucrT / sigma_e + nu_mol) * (Ldedx * Lnx + Ldedy * Lny);
                                double LregE = LEcr * wx * Lnx + LEcr * wy * Lny + LUcr * we * Lnx + LVcr * we * Lny;
                                // 
                                LsummE += (LconvE + LdiffE + LregE) * LLk;
                                //
                                // компоненты производных для Pk
                                ldudx += LUcr * Lnx * LLk;
                                ldudy += LUcr * Lny * LLk;
                                ldvdx += LVcr * Lnx * LLk;
                                ldvdy += LVcr * Lny * LLk;
                                //запись в массивы
                                //ConvK[p0] += LconvK * LLk;
                                //ConvE[p0] += LconvE * LLk;
                                //DiffK[p0] += LdiffK * LLk;
                                //DiffE[p0] += LdiffE * LLk;
                                //RegK[p0] += (LUcr * wk * Lnx + LVcr * wk * Lny) * LLk;
                                //RegE[p0] += (LUcr * we * Lnx + LVcr * we * Lny) * LLk;
                                //RegK[p0] += LregK * LLk;
                                //RegE[p0] += LregE * LLk;
                            }
                            //
                            ldudx /= wrapper.S0[p0];
                            ldudy /= wrapper.S0[p0];
                            ldvdx /= wrapper.S0[p0];
                            ldvdy /= wrapper.S0[p0];
                            //double tPk = 1.4142135623730950488016887242097 * nuT[p0] * (ldudx * ldudx + (ldvdx + ldudy) * (ldvdx + ldudy) + ldvdy * ldvdy); //- 2.0 / 3.0 * K[p0];
                            double tPk = nuT[p0] * (2 * ldudx * ldudx + (ldvdx + ldudy) * (ldvdx + ldudy) + 2 * ldvdy * ldvdy);
                            double Pk = tPk;//Math.Min(tPk, 10 * E[p0]);
                            LLrightK = (Pk - E[p0]);
                            LLrightE = E[p0] / K[p0] * (C_e1 * Pk - C_e2 * E[p0]);
                            //
                            K[p0] = K[p0] + dt / wrapper.S0[p0] * LsummK + dt * LLrightK;
                            E[p0] = E[p0] + dt / wrapper.S0[p0] * LsummE + dt * LLrightE;
                            ////
                            //ConvK[p0] /= mesh.S0[p0];
                            //ConvE[p0] /= mesh.S0[p0];
                            //DiffK[p0] /= mesh.S0[p0];
                            //DiffE[p0] /= mesh.S0[p0];
                            //RegK[p0] /= mesh.S0[p0];
                            //RegE[p0] /= mesh.S0[p0];
                            //
                            rightK[p0] = LLrightK;
                            rightE[p0] = LLrightE;
                            ReT[p0] = K[p0] * K[p0] / E[p0] / nu_mol;
                            //
                            if (double.IsNaN(E[p0]))
                            {
                                err = " KE в бесконечность";
                                iteration = Params.iter;
                                break;
                                //return iteration;
                            }
                            //}
                        }
                    });
            #endregion


            //ГУ справа снос
            int Bknot = 0;
            for (int i = 0; i < mesh.CountRight; i++)
            {
                Bknot = mesh.RightKnots[i];
                //
                K[Bknot] = K[Bknot - mesh.CountRight];
                E[Bknot] = E[Bknot - mesh.CountRight];
            }
            //
            if (Params.surf_flag == true)
            {
                int knot;
                for (int i = 0; i < mesh.CountTop; i++)
                {
                    knot = mesh.TopKnots[i];
                    K[knot] = K[knot - 1];
                    E[knot] = E[knot - 1];
                }
            }

            //
            return iteration;
        }
        private int QHD_Newton(int iteration)
        {
            return iteration;
        }
        /// <summary>
        /// Расчет скоростей МКО
        /// </summary>
        /// <param name="iteration"></param>
        /// <returns></returns>
        private int QHDUS(int iteration)
        {
            int knot = 0;
            // ГУ на дне для концентрации
            double H = 0, gamma = (rho_s - rho_w) / rho_w;
            //
            for (int i = 1; i < mesh.CountBottom - 1; i++)
            {
                knot = mesh.BottomKnots[i];
                H = Y[knot + mesh.CountLeft - 1] - Y[knot];
                S[knot] = 2.17 * Ww * d / (Math.Exp(0.39 * gamma * d / H) - 1);
                //S[knot] = d * Ww / (Math.Exp(0.39 * (rho_s - rho_w) * g * d / BTauC[i]) - 1);
            }
            S[mesh.BottomKnots[mesh.CountBottom - 1]] = mesh.BottomKnots[mesh.CountBottom - 2];

            #region Расчет u, v, s во всех узлах

            if (Params.calculationType == CheckMultiThreading.singleThreaded)
            {
                for (int i = 0; i < wrapper.CV2.Length; i++)
                {
                    int p0 = CalkVelosity(i);
                    if (double.IsNaN(U[p0]))
                    {
                        err = " U в бесконечность";
                        break;
                    }
                }
            }
            else
            {
                Parallel.ForEach(OrdPart_CV, (range, loopState) =>
                {
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        int p0 = CalkVelosity(i);
                        if (double.IsNaN(U[p0]))
                        {
                            err = " U в бесконечность";
                            //iteration = iter;
                            break;
                        }
                    }
                });
            }
            
            #endregion

            #region Граничные условия TO DO привезать ГУ к маркерам и типпу !!!!!!!!!!!!!!!!!!!
            //если задача со свободной поверхностью
            //if (surf_flag)
            //    U[mesh.LeftKnots[0]] = U[mesh.TopKnots[mesh.TopKnots.Length - 2]];
            //ГУ Dong
            //if (iteration == iter - 1)
            //    DongBC_UE();
            //else
            //    DongBC_U();

            // ----------------------------------------------------------------------
            //  Периодические ГУ справа - снос (сетка нумеруется по врертикали :(
            // ----------------------------------------------------------------------
            for (int i = 0; i < mesh.CountRight; i++)
            {
                knot = mesh.RightKnots[i];
                //
                U[knot] = U[knot - mesh.CountRight];
                V[knot] = V[knot - mesh.CountRight];
                S[knot] = S[knot - mesh.CountRight];
            }
            //
            ////----------------
            //если задача со свободной поверхностью
            if (Params.surf_flag == true)
            {
                int knot1, knot2;
                for (int i = 1; i < mesh.CountTop; i++)
                {
                    knot = mesh.TopKnots[i];
                    U[knot] = U[knot - 1];
                    V[knot] = 0;
                    knot1 = knot - 1;
                    knot2 = knot - 2;
                    dy = Y[knot] - Y[knot1];
                    S[knot] = (2.0 * S[knot1] / sigma_s / dy * (3.0 * nuT[knot] - nuT[knot2]) + 4.0 * (V[knot1] - Ww * CosJ) * S[knot1] - 3.0 / 2.0 * S[knot2] / sigma_s / dy *
                        (nuT[knot] + 4.0 / 3.0 * nuT[knot1] - nuT[knot2]) - (V[knot2] - Ww * CosJ) * S[knot2]) / (1.0 / 2.0 / sigma_s / dy * (9.0 * nuT[knot] - nuT[knot2] - 4.0 * nuT[knot1]) + 3.0 * (V[knot] - Ww * CosJ));
                    //S[knot] = ((2.0 * S[knot1] / dy / sigma_s * (3.0 * nuT[knot] - nuT[knot2])) + (4.0 * (V[knot1] - Ww * CosJ) * S[knot1]) - 0.3e1 / 0.2e1 * S[knot2] / dy / sigma_s * 
                    //    (nuT[knot] + 0.4e1 / 0.3e1 * nuT[knot1] - nuT[knot2]) - (V[knot2] - Ww * CosJ) * S[knot2]) / (((9.0 * nuT[knot]) - nuT[knot2] - 0.4e1 * nuT[knot1]) / sigma_s / dy / 0.2e1 + (3.0 * V[knot]) - (3.0 * Ww * CosJ));

                    //S[knot] = S[knot - 1];
                    if (S[knot] > S[knot - 1])
                        S[knot] = S[knot - 1];
                    if (S[knot] < 0)
                        S[knot] = 0;
                }
            }
            //
            for (int i = 1; i < mesh.CountBottom; i++)
            {
                knot = mesh.BottomKnots[i];
                S[knot + 1] = S[knot];
                //
                if (S[knot + 2] <= 0)
                    S[knot + 1] = 0;
            }
            #endregion

            return iteration;
        }
        /// <summary>
        /// Расчет k, omega
        /// </summary>
        /// <param name="iteration"></param>
        /// <returns></returns>
        private int QHDKW(int iteration)
        {
            #region Расчет k и w во всех узлах 

            if (Params.calculationType == CheckMultiThreading.singleThreaded)
            {
                for (int i = 0; i < wrapper.CV2.Length; i++)
                {
                    int p0 = CalkKW(i);
                    if (double.IsNaN(K[p0]) || double.IsInfinity(K[p0]))
                    {
                        err = " K в бесконечность";
                        iteration = Params.iter;
                        break;
                    }
                    if (double.IsNaN(W[p0]) || double.IsInfinity(W[p0]))
                    {
                        err = " W в бесконечность";
                        iteration = Params.iter;
                        break;
                    }
                }
            }
            else
            {
                Parallel.ForEach(OrdPart_CV, (range, loopState) =>
                {
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        int p0 = CalkKW(i);
                        if (double.IsNaN(K[p0]) || double.IsInfinity(K[p0]))
                        {
                            err = " K в бесконечность";
                            iteration = Params.iter;
                            break;
                        }
                        if (double.IsNaN(W[p0]) || double.IsInfinity(W[p0]))
                        {
                            err = " W в бесконечность";
                            iteration = Params.iter;
                            break;
                        }
                    }
                });
            }
            #endregion
            #region расчет и установка граничных условий

            // вычисление напряжения по пристеночной функции + вычисление придонной равновесной концентрации по Эйнштейну по u* (Lin, Namin)
            for (int i = 0; i < wrapper.CV_WallKnots.Length; i++)
            {
                int knot = wrapper.CV_WallKnots[i][0];
                CV_WallTau[i] = WFunc(knot, wrapper.CV_WallKnotsDistance[i]);
            }
            // вычисление напряжения по пристеночной функции для отрисовки
            for (int i = 0; i < mesh.CountBottom; i++)
            {
                int knot = mesh.BottomKnots[i] + 1;
                BTauC[i] = WFunc(knot, wrapper.BWallDistance[i]);
            }
            for (int i = 0; i < mesh.CountBottom - 1; i++)
            {
                // осреднение напряжений - плохой алгоритм
                BTau[i] = 2 * BTauC[i] * BTauC[i + 1] / (BTauC[i] + BTauC[i + 1] + MEM.Error12);
                // поправим его
                double btau = 0.5 * (BTauC[i] + BTauC[i + 1]);
                BTau[i] = BTau[i]>btau ? btau : BTau[i];
            }
            BTau[mesh.CountBottom - 1] = BTau[mesh.CountBottom - 2];
            // поправка по пристеночной функции для верхней границы
            if (Params.surf_flag != true)
            {
                for (int i = 0; i < mesh.CountTop; i++)
                {
                    int knot = mesh.TopKnots[i] - 1;
                    TTauC[i] = WFunc(knot, wrapper.TWallDistance[i]);
                }
                for (int i = 0; i < mesh.CountTop - 1; i++)
                {
                    // осреднение напряжений - плохой алгоритм
                    TTau[i] = 2 * TTauC[i] * TTauC[i + 1] / (TTauC[i] + TTauC[i + 1] + MEM.Error12);
                    // поправим его
                    double btau = 0.5 * (TTau[i] + TTau[i + 1]);
                    TTau[i] = TTau[i] > btau ? btau : TTau[i];
                }
                TTau[mesh.CountTop - 1] = TTau[mesh.CountTop - 2];
            }
            //Wilcox2008Boundary();
            //Wilcox1988Boundary();
            WallFuncKW();
            // dwdy=0
            //W[0] = W[1]; W[mesh.CountLeft - 1] = W[mesh.CountLeft - 2];
            for (int i = 0; i < mesh.CountBottom; i++)
            {
                int knot = mesh.BottomKnots[i];
                W[knot] = W[knot + 1];
            }
            if (Params.surf_flag != true)
            {
                for (int i = 0; i < mesh.CountTop; i++)
                {
                    int knot = mesh.TopKnots[i];
                    W[knot] = W[knot - 1];
                }
            }
            //ГУ справа снос
            int Bknot = 0;
            for (int i = 0; i < mesh.CountRight; i++)
            {
                Bknot = mesh.RightKnots[i];
                //
                K[Bknot] = K[Bknot - mesh.CountRight];
                W[Bknot] = W[Bknot - mesh.CountRight];
                nuT[Bknot] = nuT[Bknot - mesh.CountRight];
            }
            //
            if (Params.surf_flag == true)
            {
                int knot, knot1;
                double dy = 0;
                for (int i = 1; i < mesh.CountTop; i++)
                {
                    knot = mesh.TopKnots[i];
                    knot1 = knot - 1;
                    K[knot] = K[knot1];
                    W[knot] = W[knot1];
                    nuT[knot] = nuT[knot1];
                }
            }
            #endregion

            return iteration;
        }
        /// <summary>
        /// Расчет скоростей U и V, концентрации S в узле
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        protected int CalkVelosity(int i)
        {

            double LsummU = 0;//потоки U скорости
            double LsummV = 0;//потоки V скорости
            double LsummS = 0;//потоки s концентрации
                              //
            int p0 = wrapper.CVolumes[i][0];
            int jj = wrapper.CVolumes[i].Length - 1;//количество КО, связанных с данным узлом
            for (int j = 0; j < jj; j++)
            {
                double _lx10 = wrapper.Lx10[p0][j];
                double _lx32 = wrapper.Lx32[p0][j];
                double _ly01 = wrapper.Ly01[p0][j];
                double _ly23 = wrapper.Ly23[p0][j];
                //площадь
                double LS = wrapper.S[p0][j];
                //сосоедние элементы
                int Lv1 = wrapper.CVolumes[i][(j + 1) % jj + 1];
                int Lv2 = wrapper.CVolumes[i][j + 1];
                //вторая точка общей грани
                int Lp1 = wrapper.P1[p0][j];
                //находим значения функций в центрах масс 1ого и 2ого треугольника как средние значения по элементу
                uint[] Knots1 = mesh.AreaElems[Lv1];
                uint Lt1 = Knots1[0]; uint Lt2 = Knots1[1]; uint Lt3 = Knots1[2];
                double LUc1 = (U[Lt1] + U[Lt2] + U[Lt3]) / 3.0;
                double LVc1 = (V[Lt1] + V[Lt2] + V[Lt3]) / 3.0;
                double LPc1 = (P[Lt1] + P[Lt2] + P[Lt3]) / 3.0;
                double LSc1 = (S[Lt1] + S[Lt2] + S[Lt3]) / 3.0;
                uint[] Knots2 = mesh.AreaElems[Lv2];
                uint Lz1 = Knots2[0]; uint Lz2 = Knots2[1]; uint Lz3 = Knots2[2];
                double LUc2 = (U[Lz1] + U[Lz2] + U[Lz3]) / 3.0;
                double LVc2 = (V[Lz1] + V[Lz2] + V[Lz3]) / 3.0;
                double LPc2 = (P[Lz1] + P[Lz2] + P[Lz3]) / 3.0;
                double LSc2 = (S[Lz1] + S[Lz2] + S[Lz3]) / 3.0;
                //значения производных в точке пересечения граней
                double Ls2 = 2 * LS;
                double Ldudx = ((LUc1 - LUc2) * _ly01 + (U[Lp1] - U[p0]) * _ly23) / Ls2;
                double Ldudy = ((LUc1 - LUc2) * _lx10 + (U[Lp1] - U[p0]) * _lx32) / Ls2;
                double Ldvdx = ((LVc1 - LVc2) * _ly01 + (V[Lp1] - V[p0]) * _ly23) / Ls2;
                double Ldvdy = ((LVc1 - LVc2) * _lx10 + (V[Lp1] - V[p0]) * _lx32) / Ls2;
                double Ldpdx = ((LPc1 - LPc2) * _ly01 + (P[Lp1] - P[p0]) * _ly23) / Ls2;
                double Ldpdy = ((LPc1 - LPc2) * _lx10 + (P[Lp1] - P[p0]) * _lx32) / Ls2;
                double Ldsdx = ((LSc1 - LSc2) * _ly01 + (S[Lp1] - S[p0]) * _ly23) / Ls2;
                double Ldsdy = ((LSc1 - LSc2) * _lx10 + (S[Lp1] - S[p0]) * _lx32) / Ls2;

                //внешняя нормаль к грани КО (контуру КО)
                double Lnx = wrapper.Nx[p0][j]; double Lny = wrapper.Ny[p0][j];
                ////значение функций в точке пересечения грани КО и основной грани
                double Lalpha = wrapper.Alpha[p0][j];
                double LUcr = Lalpha * U[p0] + (1 - Lalpha) * U[Lp1];
                double LVcr = Lalpha * V[p0] + (1 - Lalpha) * V[Lp1];
                double LPcr = Lalpha * P[p0] + (1 - Lalpha) * P[Lp1];
                double LScr = Lalpha * S[p0] + (1 - Lalpha) * S[Lp1];
                double LNucr = Lalpha * nuT[p0] + (1 - Lalpha) * nuT[Lp1] + nu_mol;
                double LKcr = Lalpha * K[p0] + (1 - Lalpha) * K[Lp1];
                double LWcr = Lalpha * W[p0] + (1 - Lalpha) * W[Lp1];
                //длина текущего фрагмента внешнего контура КО
                double LLk = wrapper.Lk[p0][j];
                //
                //
                double LKc1 = (K[Lt1] + K[Lt2] + K[Lt3]) / 3.0;
                double LKc2 = (K[Lz1] + K[Lz2] + K[Lz3]) / 3.0;
                double Ldkdx = ((LKc1 - LKc2) * _ly01 + (K[Lp1] - K[p0]) * _ly23) / Ls2;
                double Ldkdy = ((LKc1 - LKc2) * _lx10 + (K[Lp1] - K[p0]) * _lx32) / Ls2;
                //
                double wk = Params.tau * (LUcr * Ldkdx + LVcr * Ldkdy - (LNucr * (2 * Ldudx * Ldudx + (Ldvdx + Ldudy) * (Ldvdx + Ldudy) + 2 * Ldvdy * Ldvdy)) - LWcr * LKcr * beta_z);
                double wx = Params.tau * (LUcr * Ldudx + LVcr * Ldudy + 1.0 / rho_w * Ldpdx + 2.0 / 3.0 * Ldkdx);
                double wy = Params.tau * (LUcr * Ldvdx + LVcr * Ldvdy + 1.0 / rho_w * Ldpdy + 2.0 / 3.0 * Ldkdy);
                double ws = Params.tau * (LScr * (Ldudx + Ldvdy) + (LUcr + Ww * SinJ) * Ldsdx + (LVcr - Ww * CosJ) * Ldsdy);
                //
                //
                //расчет потоков
                double LpressU = -1.0 / rho_w * LPcr * Lnx - 2.0 / 3.0 * LKcr * Lnx;
                double LconvU = -LUcr * LUcr * Lnx - (LUcr * LVcr) * Lny;
                double LdiffU = (nu_mol + LNucr) * (2.0 * Ldudx * Lnx - 2.0 / 3.0 * (Ldudx + Ldvdy) * Lnx + Ldudy * Lny + Ldvdx * Lny);
                double LregU1 = 2.0 * LUcr * wx * Lnx + 2.0 / 3.0 * wk * Lnx;
                double LregU2 = (LVcr * wx + LUcr * wy) * Lny;
                double LregU = LregU1 + LregU2;
                LsummU += (LconvU + LdiffU + LregU + LpressU) * LLk;
                //                  
                double LpressV = -1.0 / rho_w * LPcr * Lny - 2.0 / 3.0 * LKcr * Lny;
                double LconvV = -(LUcr * LVcr) * Lnx - LVcr * LVcr * Lny;
                double LdiffV = (nu_mol + LNucr) * (2.0 * Ldvdy * Lny - 2.0 / 3.0 * (Ldudx + Ldvdy) * Lny + Ldvdx * Lnx + Ldudy * Lnx);
                double LregV1 = 2.0 * LVcr * wy * Lny + 2.0 / 3.0 * wk * Lny;
                double LregV2 = (LVcr * wx + LUcr * wy) * Lnx;
                double LregV = LregV1 + LregV2;
                LsummV += (LconvV + LdiffV + LregV + LpressV) * LLk;
                //
                double LconvS = -LScr * (LUcr + Ww * SinJ) * Lnx - LScr * (LVcr - Ww * CosJ) * Lny;
                double LdiffS = LNucr / sigma_s * (Ldsdx * Lnx + Ldsdy * Lny);
                double LregS1 = LScr * (wx * Lnx + wy * Lny);
                double LregS2 = (LUcr + Ww * SinJ) * ws * Lnx + (LVcr - Ww * CosJ) * ws * Lny;
                double LregS = LregS1 + LregS2; // если включаю, концентрация у поверхности начинает бесконтольно расти
                LsummS += (LconvS + LdiffS + LregS) * LLk;
                if (double.IsNaN(LsummU) || double.IsNaN(LsummV) || double.IsNaN(LsummS))
                {
                    Logger.Instance.Error("NUN U[" + p0.ToString() + "]", "CalkVelosity(" + i.ToString() + ")");
                    err = " U V S в бесконечность";
                }
                if (double.IsInfinity(LsummU) || double.IsInfinity(LsummV) || double.IsInfinity(LsummS))
                {
                    Logger.Instance.Error("IsInfinity U[" + p0.ToString() + "]", "CalkVelosity(" + i.ToString() + ")");
                    err = " U V S в бесконечность";
                }
            }
            //
            U[p0] = U[p0] + dt / wrapper.S0[p0] * LsummU;
            V[p0] = V[p0] + dt / wrapper.S0[p0] * LsummV;
            S[p0] = S[p0] + dt / wrapper.S0[p0] * LsummS;
            if (S[p0] < 0)
                S[p0] = 0;


            if (double.IsNaN(U[p0]))
            {
                Logger.Instance.Error("NUN U[" + p0.ToString() + "]", "CalkVelosity(" + i.ToString() + ")");
                err = " U в бесконечность";
            }

            if (double.IsNaN(V[p0]))
            {
                Logger.Instance.Error("NUN V[" + p0.ToString() + "]", "CalkVelosity(" + i.ToString() + ")");
                err = " V в бесконечность";
            }
            return p0;
        }
        /// <summary>
        /// Расчет K и W в узле 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        protected int CalkKW(int i)
        {
            double LsummK = 0;//потоки k 
            double LsummW = 0;//потоки e 
            double LLrightK = 0;//потоки k 
            double LLrightW = 0;//потоки e 
                                //
            double ldudx = 0, ldudy = 0, ldvdx = 0, ldvdy = 0, ldkdx = 0, ldkdy = 0, ldwdx = 0, ldwdy = 0;
            //
            int p0 = wrapper.CVolumes[i][0];
            // убираем из расчета узлы, в которых устанавливается WallFunc

            int jj = wrapper.CVolumes[i].Length - 1;//количество КО, связанных с данным узлом
            for (int j = 0; j < jj; j++)
            {
                double _lx10 = wrapper.Lx10[p0][j]; double _lx32 = wrapper.Lx32[p0][j];
                double _ly01 = wrapper.Ly01[p0][j]; double _ly23 = wrapper.Ly23[p0][j];
                //площадь
                double LS = wrapper.S[p0][j];
                //сосоедние элементы
                int Lv1 = wrapper.CVolumes[i][(j + 1) % jj + 1];
                int Lv2 = wrapper.CVolumes[i][j + 1];
                //вторая точка общей грани
                int Lp1 = wrapper.P1[p0][j];
                //находим значения функций в центрах масс 1ого и 2ого треугольника как средние значения по элементу
                uint[] Knots1 = new uint[3];
                mesh.ElementKnots((uint)Lv1, ref Knots1);
                uint Lt1 = Knots1[0]; uint Lt2 = Knots1[1]; uint Lt3 = Knots1[2];
                double LUc1 = (U[Lt1] + U[Lt2] + U[Lt3]) / 3.0;
                double LVc1 = (V[Lt1] + V[Lt2] + V[Lt3]) / 3.0;
                double LPc1 = (P[Lt1] + P[Lt2] + P[Lt3]) / 3.0;
                double LKc1 = (K[Lt1] + K[Lt2] + K[Lt3]) / 3.0;
                double LWc1 = (W[Lt1] + W[Lt2] + W[Lt3]) / 3.0;
                uint[] Knots2 = new uint[3];
                mesh.ElementKnots((uint)Lv2, ref Knots2);
                uint Lz1 = Knots2[0]; uint Lz2 = Knots2[1]; uint Lz3 = Knots2[2];
                double LUc2 = (U[Lz1] + U[Lz2] + U[Lz3]) / 3.0;
                double LVc2 = (V[Lz1] + V[Lz2] + V[Lz3]) / 3.0;
                double LPc2 = (P[Lz1] + P[Lz2] + P[Lz3]) / 3.0;
                double LKc2 = (K[Lz1] + K[Lz2] + K[Lz3]) / 3.0;
                double LWc2 = (W[Lz1] + W[Lz2] + W[Lz3]) / 3.0;
                //значения производных в точке пересечения граней
                double Ls2 = 2 * LS;
                double Ldudx = ((LUc1 - LUc2) * _ly01 + (U[Lp1] - U[p0]) * _ly23) / Ls2;
                double Ldudy = ((LUc1 - LUc2) * _lx10 + (U[Lp1] - U[p0]) * _lx32) / Ls2;
                double Ldvdx = ((LVc1 - LVc2) * _ly01 + (V[Lp1] - V[p0]) * _ly23) / Ls2;
                double Ldvdy = ((LVc1 - LVc2) * _lx10 + (V[Lp1] - V[p0]) * _lx32) / Ls2;
                double Ldpdx = ((LPc1 - LPc2) * _ly01 + (P[Lp1] - P[p0]) * _ly23) / Ls2;
                double Ldpdy = ((LPc1 - LPc2) * _lx10 + (P[Lp1] - P[p0]) * _lx32) / Ls2;
                //
                double Ldkdx = ((LKc1 - LKc2) * _ly01 + (K[Lp1] - K[p0]) * _ly23) / Ls2;
                double Ldkdy = ((LKc1 - LKc2) * _lx10 + (K[Lp1] - K[p0]) * _lx32) / Ls2;
                double Ldwdx = ((LWc1 - LWc2) * _ly01 + (W[Lp1] - W[p0]) * _ly23) / Ls2;
                double Ldwdy = ((LWc1 - LWc2) * _lx10 + (W[Lp1] - W[p0]) * _lx32) / Ls2;
                //внешняя нормаль к грани КО (контуру КО)
                double Lnx = wrapper.Nx[p0][j]; double Lny = wrapper.Ny[p0][j];
                ////значение функций в точке пересечения грани КО и основной грани
                double Lalpha = wrapper.Alpha[p0][j];
                double LUcr = Lalpha * U[p0] + (1 - Lalpha) * U[Lp1];
                double LVcr = Lalpha * V[p0] + (1 - Lalpha) * V[Lp1];
                double LKcr = Lalpha * K[p0] + (1 - Lalpha) * K[Lp1];
                double LWcr = Lalpha * W[p0] + (1 - Lalpha) * W[Lp1];
                double LNucrT = Lalpha * nuT[p0] + (1 - Lalpha) * nuT[Lp1];
                //длина текущего фрагмента внешнего контура КО
                double LLk = wrapper.Lk[p0][j];
                //расчет потоков
                double wx = Params.tau * (LUcr * Ldudx + LVcr * Ldudy + 1.0 / rho_w * Ldpdx + 2.0 / 3.0 * Ldkdx);
                double wy = Params.tau * (LUcr * Ldvdx + LVcr * Ldvdy + 1.0 / rho_w * Ldpdy + 2.0 / 3.0 * Ldkdy);
                double wk = Params.tau * (LUcr * Ldkdx + LVcr * Ldkdy - (LNucrT * (2 * Ldudx * Ldudx + (Ldvdx + Ldudy) * (Ldvdx + Ldudy) + 2 * Ldvdy * Ldvdy) - beta_z * LKcr * LWcr));
                double sigma_d = 0;
                double kwkw = Ldkdx * Ldwdx + Ldkdy * Ldwdy;
                if (kwkw > 0)
                    sigma_d = sigma_d_0;
                double ww = Params.tau * (LUcr * Ldwdx + LVcr * Ldwdy - (alpha * LWcr / LKcr * LNucrT * (2 * Ldudx * Ldudx + (Ldvdx + Ldudy) * (Ldvdx + Ldudy) + 2 * Ldvdy * Ldvdy) - beta * LWcr * LWcr + sigma_d / LWcr * kwkw));
                //
                double LconvK = -LUcr * LKcr * Lnx - LVcr * LKcr * Lny;
                double LdiffK = (nu_mol + sigma_z * LKcr / LWcr) * (Ldkdx * Lnx + Ldkdy * Lny);
                double LregK = LKcr * wx * Lnx + LKcr * wy * Lny + LUcr * wk * Lnx + LVcr * wk * Lny;
                //
                LsummK += (LconvK + LdiffK + LregK) * LLk;
                //                  
                double LconvW = -LUcr * LWcr * Lnx - LVcr * LWcr * Lny;
                double LdiffW = (nu_mol + sigma_z * LKcr / LWcr) * (Ldwdx * Lnx + Ldwdy * Lny);
                double LregW = LWcr * wx * Lnx + LWcr * wy * Lny + LUcr * ww * Lnx + LVcr * ww * Lny;
                // 
                LsummW += (LconvW + LdiffW + LregW) * LLk;
                //
                // компоненты производных для Pk
                ldudx += LUcr * Lnx * LLk;
                ldudy += LUcr * Lny * LLk;
                ldvdx += LVcr * Lnx * LLk;
                ldvdy += LVcr * Lny * LLk;
                // компоненты производных для правой части w
                ldkdx += LKcr * Lnx * LLk;
                ldkdy += LKcr * Lny * LLk;
                ldwdx += LWcr * Lnx * LLk;
                ldwdy += LWcr * Lny * LLk;
                //запись в массивы
                //ConvK[p0] += LconvK * LLk;
                //ConvE[p0] += LconvE * LLk;
                //DiffK[p0] += LdiffK * LLk;
                //DiffE[p0] += LdiffE * LLk;
                //RegK[p0] += (LUcr * wk * Lnx + LVcr * wk * Lny) * LLk;
                //RegE[p0] += (LUcr * we * Lnx + LVcr * we * Lny) * LLk;
                //RegK[p0] += LregK * LLk;
                //RegE[p0] += LregE * LLk;
            }
            //
            ldudx /= wrapper.S0[p0];
            ldudy /= wrapper.S0[p0];
            ldvdx /= wrapper.S0[p0];
            ldvdy /= wrapper.S0[p0];
            //double tPk = 1.4142135623730950488016887242097 * nuT[p0] * (ldudx * ldudx + (ldvdx + ldudy) * (ldvdx + ldudy) + ldvdy * ldvdy); //- 2.0 / 3.0 * K[p0];
            double tPk = nuT[p0] * (2 * ldudx * ldudx + (ldvdx + ldudy) * (ldvdx + ldudy) + 2 * ldvdy * ldvdy);
            Pk[p0] = tPk;//Math.Min(tPk, 10 * E[p0]);
            double rsigma_d = 0;
            double rkwkw = ldkdx * ldwdx + ldkdy * ldwdy;
            if (rkwkw > 0)
                rsigma_d = sigma_d_0;
            LLrightK = (Pk[p0] - beta_z * K[p0] * W[p0]);
            LLrightW = alpha * W[p0] / K[p0] * Pk[p0] - beta * W[p0] * W[p0] + rsigma_d / W[p0] * rkwkw;
            //
            K[p0] = K[p0] + dt / wrapper.S0[p0] * LsummK + dt * LLrightK;
            W[p0] = W[p0] + dt / wrapper.S0[p0] * LsummW + dt * LLrightW;
            double W0 = 7.0 / 8.0 * Math.Sqrt((2 * (ldudx - 1.0 / 3.0 * (ldudx + ldvdy)) * (ldudx - 1.0 / 3.0 * (ldudx + ldvdy)) + (ldvdx + ldudy) * (ldvdx + ldudy) + 2 * (ldvdx - 1.0 / 3.0 * (ldudx + ldvdy)) * (ldvdx - 1.0 / 3.0 * (ldudx + ldvdy))) / beta);
            double W_lim = Math.Max(W[p0], W0);
            nuT[p0] = K[p0] / W_lim;
            ////
            //ConvK[p0] /= mesh.S0[p0];
            //ConvE[p0] /= mesh.S0[p0];
            //DiffK[p0] /= mesh.S0[p0];
            //DiffE[p0] /= mesh.S0[p0];
            //RegK[p0] /= mesh.S0[p0];
            //RegE[p0] /= mesh.S0[p0];
            //
            rightK[p0] = LLrightK;
            rightW[p0] = LLrightW;
            ReT[p0] = K[p0] / W[p0] / nu_mol / C_m;
            //
            if (double.IsNaN(W[p0]))
            {
                err = " KE в бесконечность";
            }
            if (double.IsNaN(K[p0]))
            {
                err = " KE в бесконечность";
            }
            return p0;
        }

        /// <summary>
        /// Расчет давления
        /// </summary>
        /// <param name="MaxError"></param>
        /// <param name="Result"></param>
        /// <param name="BV2"></param>
        /// <param name="lockThis"></param>
        /// <param name="iteration"></param>
        /// <param name="Gauss"></param>
        private void PressureCalc(ref double MaxError, ref double[] Result, double[] BV2, System.Object lockThis, int iteration)
        {
            ///------Протестировано ОК
            ////////////////// МКЭ давление /////////////////////
            //if (Gauss)
            //{
            //    #region Расчет давления
            //    // выделение памяти под результат решения задачи
            //    Result = new double[mesh.CountKnots];
            //    //выделяем масивы для локальных правых частей
            //    if (iteration == 0)
            //    {
            //        ABand.ClearSystem();
            //        // основной цикл по конечным элементам
            //        // вычисляем локальные матрицы жесткости и производим сборку глобальной матрицы жесткости
            //        OrderablePartitioner<Tuple<int, int>> OrdPartitioner_Tau = Partitioner.Create(0, mesh.CountElements);
            //        Parallel.ForEach(OrdPartitioner_Tau,
            //              (range, loopState) =>
            //              {
            //                  for (int fe = range.Item1; fe < range.Item2; fe++)
            //                  //for (int fe = 0; fe < mesh.CountElements; fe++)
            //                  {
            //                      // выделяем массивы для локальных матриц жесткости
            //                      double[][] M = new double[3][];
            //                      for (int k = 0; k < 3; k++)
            //                      {
            //                          M[k] = new double[3];
            //                      }
            //                      //и номера его вершин
            //                      uint[] LKnots = mesh.AreaElems[fe];
            //                      // нахождение площади треугольника
            //                      double LSk = mesh.Sk[fe];
            //                      // расчитываем геометрию элемента 
            //                      double Lb1 = mesh.b1[fe];
            //                      double Lb2 = mesh.b2[fe];
            //                      double Lb3 = mesh.b3[fe];
            //                      double Lc1 = mesh.c1[fe];
            //                      double Lc2 = mesh.c2[fe];
            //                      double Lc3 = mesh.c3[fe];
            //                      // расчет локальной матрицы жесткости для диффузионного члена
            //                      M[0][0] = -LSk * (Lb1 * Lb1 + Lc1 * Lc1);
            //                      M[0][1] = -LSk * (Lb1 * Lb2 + Lc1 * Lc2);
            //                      M[0][2] = -LSk * (Lb1 * Lb3 + Lc1 * Lc3);

            //                      M[1][0] = -LSk * (Lb2 * Lb1 + Lc2 * Lc1);
            //                      M[1][1] = -LSk * (Lb2 * Lb2 + Lc2 * Lc2);
            //                      M[1][2] = -LSk * (Lb2 * Lb3 + Lc2 * Lc3);

            //                      M[2][0] = -LSk * (Lb3 * Lb1 + Lc3 * Lc1);
            //                      M[2][1] = -LSk * (Lb3 * Lb2 + Lc3 * Lc2);
            //                      M[2][2] = -LSk * (Lb3 * Lb3 + Lc3 * Lc3);
            //                      int[] Knots = { (int)LKnots[0], (int)LKnots[1], (int)LKnots[2] };
            //                      lock (lockThis)
            //                          ABand.BuildMatrix(M, Knots);
            //                  }

            //              });
            //        //
            //        //главные ГУ справа
            //        ABand.SetBoundary(BV2, mesh.RightKnots);
            //        ABand.Accept(AlgGauss, null, 1);

            //    }
            //    ABand.ClearRight();
            //    R = new double[CountKnots];
            //    //сборка правой части
            //    OrderablePartitioner<Tuple<int, int>> rangePartitioner1 = Partitioner.Create(0, mesh.CountElements);
            //    Parallel.ForEach(rangePartitioner1,
            //          (range, loopState) =>
            //          {
            //              for (int fe = range.Item1; fe < range.Item2; fe++)
            //              //for(int fe=0;fe<mesh.CountElements;fe++)
            //              {

            //                  //for (int fe = 0; fe < mesh.CountElem; fe++)
            //                  //{
            //                  double[] RR = new double[3];
            //                  // получаем текущий конечный элемент
            //                  uint[] LKnots = mesh.AreaElems[fe];
            //                  uint Lm1 = LKnots[0];
            //                  uint Lm2 = LKnots[1];
            //                  uint Lm3 = LKnots[2];
            //                  // нахождение площади треугольника
            //                  double LSk = mesh.Sk[fe];
            //                  // расчитываем геометрию элемента 
            //                  double Lb1 = mesh.b1[fe];
            //                  double Lb2 = mesh.b2[fe];
            //                  double Lb3 = mesh.b3[fe];
            //                  double Lc1 = mesh.c1[fe];
            //                  double Lc2 = mesh.c2[fe];
            //                  double Lc3 = mesh.c3[fe];
            //                  //
            //                  double LU1 = U[Lm1]; double LU2 = U[Lm2]; double LU3 = U[Lm3];
            //                  double LV1 = V[Lm1]; double LV2 = V[Lm2]; double LV3 = V[Lm3];
            //                  //правая часть - неразрывность
            //                  double LSR = rho_w * LSk / (3 * tau) * ((Lb1 * LU1 + Lb2 * LU2 + Lb3 * LU3) + (Lc1 * LV1 + Lc2 * LV2 + Lc3 * LV3));
            //                  RR[0] = alpha_n * LSR;
            //                  RR[1] = alpha_n * LSR;
            //                  RR[2] = alpha_n * LSR;
            //                  //правая часть поправка КГД
            //                  double LBUU = (Lb1 * LU1 + Lb2 * LU2 + Lb3 * LU3) * (LU1 + LU2 + LU3);
            //                  double LCUV = (Lc1 * LU1 + Lc2 * LU2 + Lc3 * LU3) * (LV1 + LV2 + LV3);
            //                  double LBVU = (Lb1 * LV1 + Lb2 * LV2 + Lb3 * LV3) * (LU1 + LU2 + LU3);
            //                  double LCVV = (Lc1 * LV1 + Lc2 * LV2 + Lc3 * LV3) * (LV1 + LV2 + LV3);
            //                  //
            //                  RR[0] += (double)(alpha_r * rho_w * LSk / 3.0 * (Lb1 * (LBUU + LCUV) + Lc1 * (LBVU + LCVV)));
            //                  RR[1] += (double)(alpha_r * rho_w * LSk / 3.0 * (Lb2 * (LBUU + LCUV) + Lc2 * (LBVU + LCVV)));
            //                  RR[2] += (double)(alpha_r * rho_w * LSk / 3.0 * (Lb3 * (LBUU + LCUV) + Lc3 * (LBVU + LCVV)));
            //                  //
            //                  //для отображения невязки
            //                  R[Lm1] += RR[0];
            //                  R[Lm2] += RR[1];
            //                  R[Lm3] += RR[2];
            //                  //}
            //              }

            //          });
            //    //
            //    ABand.BuildRight(R);
            //    //ГУ слева
            //    ABand.BuildRight(BV, mesh.LeftKnots);
            //    //главные ГУ справа 2
            //    ABand.BoundConditionsRight(BV2, mesh.RightKnots);
            //    // решение системы алгебраических уравнений
            //    ABand.Accept(AlgGauss, null, 2);
            //    Result = ABand.GetX;
            //    //для отладки
            //    for (int i = 0; i < mesh.CountLeft; i++)
            //        R[mesh.LeftKnots[i]] += BV[i];
            //    for (int i = 0; i < mesh.CountRight; i++)
            //        R[mesh.RightKnots[i]] = 0;

            //    // релаксация решения, буферизация и вычисление погрешности
            //    MaxError = 0;//масимальная ошибка в области
            //    for (int i = 0; i < CountKnots; i++)
            //    {
            //        //релаксация
            //        P[i] = (1 - relaxP) * P[i] + relaxP * Result[i];
            //        //вычисление погрешности
            //        double CurErr = Math.Abs((P[i] - buffP[i]) / P[i]);
            //        if (MaxError < CurErr)
            //            MaxError = CurErr;
            //        ErrP[i] = CurErr;
            //        //буферизация
            //        buffP[i] = P[i];
            //    }
            //    #endregion
            //}
            //else
            {
                #region Расчет давления
                uint[] Adress = new uint[CountKnots];
                for (uint i = 0; i < CountKnots; i++)
                    Adress[i] = i;

                algebra.Clear();
                // выделение памяти под результат решения задачи
                Result = new double[mesh.CountKnots];
                //выделяем масивы для локальных правых частей
                // основной цикл по конечным элементам
                // вычисляем локальные матрицы жесткости и производим сборку глобальной матрицы жесткости
                //OrderablePartitioner<Tuple<int, int>> OrdPartitioner_Tau = Partitioner.Create(0, mesh.CountElements);
                //Parallel.ForEach(OrdPartitioner_Tau,
                //      (range, loopState) =>
                //      {
                //          for (int fe = range.Item1; fe < range.Item2; fe++)
                for (int fe = 0; fe < mesh.CountElements; fe++)
                {
                    // выделяем массивы для локальных матриц жесткости
                    double[][] M = new double[3][];
                    for (int k = 0; k < 3; k++)
                    {
                        M[k] = new double[3];
                    }
                    //и номера его вершин
                    uint[] LKnots = mesh.AreaElems[fe];
                    // нахождение площади треугольника
                    double LSk = wrapper.Sk[fe];
                    // расчитываем геометрию элемента 
                    double Lb1 = wrapper.b1[fe];
                    double Lb2 = wrapper.b2[fe];
                    double Lb3 = wrapper.b3[fe];
                    double Lc1 = wrapper.c1[fe];
                    double Lc2 = wrapper.c2[fe];
                    double Lc3 = wrapper.c3[fe];
                    // расчет локальной матрицы жесткости для диффузионного члена
                    M[0][0] = -LSk * (Lb1 * Lb1 + Lc1 * Lc1);
                    M[0][1] = -LSk * (Lb1 * Lb2 + Lc1 * Lc2);
                    M[0][2] = -LSk * (Lb1 * Lb3 + Lc1 * Lc3);

                    M[1][0] = -LSk * (Lb2 * Lb1 + Lc2 * Lc1);
                    M[1][1] = -LSk * (Lb2 * Lb2 + Lc2 * Lc2);
                    M[1][2] = -LSk * (Lb2 * Lb3 + Lc2 * Lc3);

                    M[2][0] = -LSk * (Lb3 * Lb1 + Lc3 * Lc1);
                    M[2][1] = -LSk * (Lb3 * Lb2 + Lc3 * Lc2);
                    M[2][2] = -LSk * (Lb3 * Lb3 + Lc3 * Lc3);
                    lock (lockThis)
                        algebra.AddToMatrix(M, LKnots);
                }

                // });
                R = new double[CountKnots];
                //сборка правой части
                OrderablePartitioner<Tuple<int, int>> rangePartitioner1 = Partitioner.Create(0, mesh.CountElements);
                Parallel.ForEach(rangePartitioner1,
                      (range, loopState) =>
                      {
                          for (int fe = range.Item1; fe < range.Item2; fe++)
                          //for(int fe=0;fe<mesh.CountElem;fe++)
                          {

                              //for (int fe = 0; fe < mesh.CountElem; fe++)
                              //{
                              double[] RR = new double[3];
                              // получаем текущий конечный элемент
                              uint[] LKnots = mesh.AreaElems[fe];
                              uint Lm1 = LKnots[0];
                              uint Lm2 = LKnots[1];
                              uint Lm3 = LKnots[2];
                              // нахождение площади треугольника
                              double LSk = wrapper.Sk[fe];
                              // расчитываем геометрию элемента 
                              double Lb1 = wrapper.b1[fe];
                              double Lb2 = wrapper.b2[fe];
                              double Lb3 = wrapper.b3[fe];
                              double Lc1 = wrapper.c1[fe];
                              double Lc2 = wrapper.c2[fe];
                              double Lc3 = wrapper.c3[fe];
                              //
                              double LU1 = U[Lm1]; double LU2 = U[Lm2]; double LU3 = U[Lm3];
                              double LV1 = V[Lm1]; double LV2 = V[Lm2]; double LV3 = V[Lm3];
                              //правая часть - неразрывность
                              double LSR = rho_w * LSk / (3 * Params.tau) * ((Lb1 * LU1 + Lb2 * LU2 + Lb3 * LU3) + (Lc1 * LV1 + Lc2 * LV2 + Lc3 * LV3));
                              RR[0] = Params.alpha_n * LSR;
                              RR[1] = Params.alpha_n * LSR;
                              RR[2] = Params.alpha_n * LSR;
                              //правая часть поправка КГД
                              double LBUU = (Lb1 * LU1 + Lb2 * LU2 + Lb3 * LU3) * (LU1 + LU2 + LU3);
                              double LCUV = (Lc1 * LU1 + Lc2 * LU2 + Lc3 * LU3) * (LV1 + LV2 + LV3);
                              double LBVU = (Lb1 * LV1 + Lb2 * LV2 + Lb3 * LV3) * (LU1 + LU2 + LU3);
                              double LCVV = (Lc1 * LV1 + Lc2 * LV2 + Lc3 * LV3) * (LV1 + LV2 + LV3);
                              //
                              RR[0] += (double)(Params.alpha_r * rho_w * LSk / 3.0 * (Lb1 * (LBUU + LCUV) + Lc1 * (LBVU + LCVV)));
                              RR[1] += (double)(Params.alpha_r * rho_w * LSk / 3.0 * (Lb2 * (LBUU + LCUV) + Lc2 * (LBVU + LCVV)));
                              RR[2] += (double)(Params.alpha_r * rho_w * LSk / 3.0 * (Lb3 * (LBUU + LCUV) + Lc3 * (LBVU + LCVV)));
                              //
                              //для отображения невязки
                              R[Lm1] += RR[0];
                              R[Lm2] += RR[1];
                              R[Lm3] += RR[2];
                              //}
                          }

                      });
                algebra.AddToRight(R, Adress);
                //ГУ слева
                algebra.AddToRight(BV, MEM.ToUInt(mesh.LeftKnots));
                //главные ГУ справа 2
                algebra.BoundConditions(BV2, MEM.ToUInt(mesh.RightKnots));
                // решение системы алгебраических уравнений
                algebra.Solve(ref Result);
                //для отладки
                for (int i = 0; i < mesh.CountLeft; i++)
                    R[mesh.LeftKnots[i]] += BV[i];
                for (int i = 0; i < mesh.CountRight; i++)
                    R[mesh.RightKnots[i]] = 0;

                // релаксация решения, буферизация и вычисление погрешности
                MaxError = 0;//масимальная ошибка в области
                double MaxP = 0.5 * (Math.Abs(P.Max() - P.Min()) + Math.Abs(buffP.Max() - buffP.Min())) + MEM.Error10;
                for (int i = 0; i < CountKnots; i++)
                {
                    //релаксация
                    P[i] = (1 - Params.relaxP) * P[i] + Params.relaxP * Result[i];
                    //вычисление погрешности
                    //double CurErr = Math.Abs((P[i] - buffP[i]) / P[i]);
                    double CurErr = Math.Abs((P[i] - buffP[i]) / MaxP);
                    if (MaxError < CurErr)
                        MaxError = CurErr;
                    ErrP[i] = CurErr;
                    //буферизация
                    buffP[i] = P[i];
                }
                #endregion
            }
        }

        // 
        private int QHDUS2(int iteration)
        {
            if (CV_WallTau[0] == 0)
            {
                for (int i = 0; i < wrapper.CV_WallKnots.Length; i++)
                {
                    int knot = wrapper.CV_WallKnots[i][0];
                    CV_WallTau[i] = WallFunc(knot, wrapper.CV_WallKnotsDistance[i]);
                }
            }
            #region Расчет u, v во внутренних узлах без узлов WallKnot
            Parallel.ForEach(OrdPart_CV2, (range, loopState) =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                //for (int i = 0; i < mesh.CV2.Length; i++)
                {
                    double LsummU = 0;//потоки U скорости
                    double LsummV = 0;//потоки V скорости
                    double LsummS = 0;//потоки s концентрации
                                      //
                    int p0 = wrapper.CV2[i][0];
                    int jj = wrapper.CV2[i].Length - 1;//количество КО, связанных с данным узлом
                    for (int j = 0; j < jj; j++)
                    {
                        double _lx10 = wrapper.Lx10[p0][j]; double _lx32 = wrapper.Lx32[p0][j];
                        double _ly01 = wrapper.Ly01[p0][j]; double _ly23 = wrapper.Ly23[p0][j];
                        //площадь
                        double LS = wrapper.S[p0][j];
                        //сосоедние элементы
                        int Lv1 = wrapper.CV2[i][(j + 1) % jj + 1];
                        int Lv2 = wrapper.CV2[i][j + 1];
                        //вторая точка общей грани
                        int Lp1 = wrapper.P1[p0][j];
                        //находим значения функций в центрах масс 1ого и 2ого треугольника как средние значения по элементу
                        uint[] Knots1 = mesh.AreaElems[Lv1];
                        uint Lt1 = Knots1[0]; uint Lt2 = Knots1[1]; uint Lt3 = Knots1[2];
                        double LUc1 = (U[Lt1] + U[Lt2] + U[Lt3]) / 3.0;
                        double LVc1 = (V[Lt1] + V[Lt2] + V[Lt3]) / 3.0;
                        double LPc1 = (P[Lt1] + P[Lt2] + P[Lt3]) / 3.0;
                        double LSc1 = (S[Lt1] + S[Lt2] + S[Lt3]) / 3.0;
                        uint[] Knots2 = mesh.AreaElems[Lv2];
                        uint Lz1 = Knots2[0]; uint Lz2 = Knots2[1]; uint Lz3 = Knots2[2];
                        double LUc2 = (U[Lz1] + U[Lz2] + U[Lz3]) / 3.0;
                        double LVc2 = (V[Lz1] + V[Lz2] + V[Lz3]) / 3.0;
                        double LPc2 = (P[Lz1] + P[Lz2] + P[Lz3]) / 3.0;
                        double LSc2 = (S[Lz1] + S[Lz2] + S[Lz3]) / 3.0;
                        //значения производных в точке пересечения граней
                        double Ls2 = 2 * LS;
                        double Ldudx = ((LUc1 - LUc2) * _ly01 + (U[Lp1] - U[p0]) * _ly23) / Ls2;
                        double Ldudy = ((LUc1 - LUc2) * _lx10 + (U[Lp1] - U[p0]) * _lx32) / Ls2;
                        double Ldvdx = ((LVc1 - LVc2) * _ly01 + (V[Lp1] - V[p0]) * _ly23) / Ls2;
                        double Ldvdy = ((LVc1 - LVc2) * _lx10 + (V[Lp1] - V[p0]) * _lx32) / Ls2;
                        double Ldpdx = ((LPc1 - LPc2) * _ly01 + (P[Lp1] - P[p0]) * _ly23) / Ls2;
                        double Ldpdy = ((LPc1 - LPc2) * _lx10 + (P[Lp1] - P[p0]) * _lx32) / Ls2;
                        double Ldsdx = ((LSc1 - LSc2) * _ly01 + (S[Lp1] - S[p0]) * _ly23) / Ls2;
                        double Ldsdy = ((LSc1 - LSc2) * _lx10 + (S[Lp1] - S[p0]) * _lx32) / Ls2;

                        //внешняя нормаль к грани КО (контуру КО)
                        double Lnx = wrapper.Nx[p0][j]; double Lny = wrapper.Ny[p0][j];
                        ////значение функций в точке пересечения грани КО и основной грани
                        double Lalpha = wrapper.Alpha[p0][j];
                        double LUcr = Lalpha * U[p0] + (1 - Lalpha) * U[Lp1];
                        double LVcr = Lalpha * V[p0] + (1 - Lalpha) * V[Lp1];
                        double LPcr = Lalpha * P[p0] + (1 - Lalpha) * P[Lp1];
                        double LScr = Lalpha * S[p0] + (1 - Lalpha) * S[Lp1];
                        double LNucr = Lalpha * nuT[p0] + (1 - Lalpha) * nuT[Lp1] + nu_mol;
                        double LKcr = Lalpha * K[p0] + (1 - Lalpha) * K[Lp1];
                        double LEcr = Lalpha * E[p0] + (1 - Lalpha) * E[Lp1];
                        //длина текущего фрагмента внешнего контура КО
                        double LLk = wrapper.Lk[p0][j];
                        //
                        //
                        double LKc1 = (K[Lt1] + K[Lt2] + K[Lt3]) / 3.0;
                        double LKc2 = (K[Lz1] + K[Lz2] + K[Lz3]) / 3.0;
                        double Ldkdx = ((LKc1 - LKc2) * _ly01 + (K[Lp1] - K[p0]) * _ly23) / Ls2;
                        double Ldkdy = ((LKc1 - LKc2) * _lx10 + (K[Lp1] - K[p0]) * _lx32) / Ls2;
                        double wk = Params.tau * (LUcr * Ldkdx + LVcr * Ldkdy - (LNucr * (2 * Ldudx * Ldudx + (Ldvdx + Ldudy) * (Ldvdx + Ldudy) + 2 * Ldvdy * Ldvdy)) - LEcr);
                        //
                        //
                        //расчет потоков
                        double LpressU = -1.0 / rho_w * LPcr * Lnx - 2.0 / 3.0 * LKcr * Lnx;
                        double LconvU = -LUcr * LUcr * Lnx - (LUcr * LVcr) * Lny;
                        double LdiffU = (nu_mol + LNucr) * (2.0 * Ldudx * Lnx - 2.0 / 3.0 * (Ldudx + Ldvdy) * Lnx + Ldudy * Lny + Ldvdx * Lny);
                        double wx = Params.tau * (LUcr * Ldudx + LVcr * Ldudy + 1.0 / rho_w * Ldpdx + 2.0 / 3.0 * Ldkdx);
                        double wy = Params.tau * (LUcr * Ldvdx + LVcr * Ldvdy + 1.0 / rho_w * Ldpdy + 2.0 / 3.0 * Ldkdy);
                        double LregU1 = 2.0 * LUcr * wx * Lnx + 2.0 / 3.0 * wk * Lnx;
                        double LregU2 = (LVcr * wx + LUcr * wy) * Lny;
                        double LregU = LregU1 + LregU2;
                        LsummU += (LconvU + LdiffU + LregU + LpressU) * LLk;
                        //                  
                        double LpressV = -1.0 / rho_w * LPcr * Lny - 2.0 / 3.0 * LKcr * Lny;
                        double LconvV = -(LUcr * LVcr) * Lnx - LVcr * LVcr * Lny;
                        double LdiffV = (nu_mol + LNucr) * (2.0 * Ldvdy * Lny - 2.0 / 3.0 * (Ldudx + Ldvdy) * Lny + Ldvdx * Lnx + Ldudy * Lnx);
                        double LregV1 = 2.0 * LVcr * wy * Lny + 2.0 / 3.0 * wk * Lny;
                        double LregV2 = (LVcr * wx + LUcr * wy) * Lnx;
                        double LregV = LregV1 + LregV2;
                        LsummV += (LconvV + LdiffV + LregV + LpressV) * LLk;
                        //
                        double LconvS = -LScr * (LUcr + Ww * SinJ) * Lnx - LScr * (LVcr - Ww * CosJ) * Lny;
                        double LdiffS = LNucr / sigma_s * (Ldsdx * Lnx + Ldsdy * Lny);
                        double LregS = LScr * (wx * Lnx + wy * Lny);
                        LsummS += (LconvS + LdiffS + LregS) * LLk;
                    }
                    //
                    U[p0] = U[p0] + dt / wrapper.S0[p0] * LsummU;
                    V[p0] = V[p0] + dt / wrapper.S0[p0] * LsummV;
                    S[p0] = S[p0] + dt / wrapper.S0[p0] * LsummS;
                    //
                    if (double.IsNaN(U[p0]))
                    {
                        err = " U в бесконечность";
                        //iteration = iter;
                        break;
                    }
                }
            });
            #endregion
            #region Расчет u, v в узлах WallKnot
            Parallel.ForEach(OrdPart_CV_Wall, (range, loopState) =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                //for (int i = 0; i < mesh.CV_WallKnots.Length; i++)
                {
                    double LsummU = 0;//потоки U скорости
                    double LsummV = 0;//потоки V скорости
                    double LsummS = 0;//потоки s концентрации
                                      //
                    int p0 = wrapper.CV_WallKnots[i][0];
                    int jj = wrapper.CV_WallKnots[i].Length - 1; // количество КО, связанных с данным узлом
                    for (int j = 0; j < jj; j++)
                    {
                        double _lx10 = wrapper.Lx10[p0][j]; double _lx32 = wrapper.Lx32[p0][j];
                        double _ly01 = wrapper.Ly01[p0][j]; double _ly23 = wrapper.Ly23[p0][j];
                        //площадь
                        double LS = wrapper.S[p0][j];
                        //сосоедние элементы
                        int Lv1 = wrapper.CV_WallKnots[i][(j + 1) % jj + 1];
                        int Lv2 = wrapper.CV_WallKnots[i][j + 1];
                        //вторая точка общей грани
                        int Lp1 = wrapper.P1[p0][j];
                        //находим значения функций в центрах масс 1ого и 2ого треугольника как средние значения по элементу
                        uint[] Knots1 = mesh.AreaElems[Lv1];
                        uint Lt1 = Knots1[0]; uint Lt2 = Knots1[1]; uint Lt3 = Knots1[2];
                        double LUc1 = (U[Lt1] + U[Lt2] + U[Lt3]) / 3.0;
                        double LVc1 = (V[Lt1] + V[Lt2] + V[Lt3]) / 3.0;
                        double LPc1 = (P[Lt1] + P[Lt2] + P[Lt3]) / 3.0;
                        double LSc1 = (S[Lt1] + S[Lt2] + S[Lt3]) / 3.0;
                        uint[] Knots2 = mesh.AreaElems[Lv2];
                        uint Lz1 = Knots2[0]; uint Lz2 = Knots2[1]; uint Lz3 = Knots2[2];
                        double LUc2 = (U[Lz1] + U[Lz2] + U[Lz3]) / 3.0;
                        double LVc2 = (V[Lz1] + V[Lz2] + V[Lz3]) / 3.0;
                        double LPc2 = (P[Lz1] + P[Lz2] + P[Lz3]) / 3.0;
                        double LSc2 = (S[Lz1] + S[Lz2] + S[Lz3]) / 3.0;
                        //значения производных в точке пересечения граней
                        double Ls2 = 2 * LS;
                        double Ldudx = ((LUc1 - LUc2) * _ly01 + (U[Lp1] - U[p0]) * _ly23) / Ls2;
                        double Ldudy = ((LUc1 - LUc2) * _lx10 + (U[Lp1] - U[p0]) * _lx32) / Ls2;
                        double Ldvdx = ((LVc1 - LVc2) * _ly01 + (V[Lp1] - V[p0]) * _ly23) / Ls2;
                        double Ldvdy = ((LVc1 - LVc2) * _lx10 + (V[Lp1] - V[p0]) * _lx32) / Ls2;
                        double Ldpdx = ((LPc1 - LPc2) * _ly01 + (P[Lp1] - P[p0]) * _ly23) / Ls2;
                        double Ldpdy = ((LPc1 - LPc2) * _lx10 + (P[Lp1] - P[p0]) * _lx32) / Ls2;
                        double Ldsdx = ((LSc1 - LSc2) * _ly01 + (S[Lp1] - S[p0]) * _ly23) / Ls2;
                        double Ldsdy = ((LSc1 - LSc2) * _lx10 + (S[Lp1] - S[p0]) * _lx32) / Ls2;
                        //внешняя нормаль к грани КО (контуру КО)
                        double Lnx = wrapper.Nx[p0][j]; double Lny = wrapper.Ny[p0][j];
                        ////значение функций в точке пересечения грани КО и основной грани
                        double Lalpha = wrapper.Alpha[p0][j];
                        double LUcr = Lalpha * U[p0] + (1 - Lalpha) * U[Lp1];
                        double LVcr = Lalpha * V[p0] + (1 - Lalpha) * V[Lp1];
                        double LPcr = Lalpha * P[p0] + (1 - Lalpha) * P[Lp1];
                        double LScr = Lalpha * S[p0] + (1 - Lalpha) * S[Lp1];
                        double LNucr = Lalpha * nuT[p0] + (1 - Lalpha) * nuT[Lp1] + nu_mol;
                        //
                        //
                        double LKc1 = (K[Lt1] + K[Lt2] + K[Lt3]) / 3.0;
                        double LKc2 = (K[Lz1] + K[Lz2] + K[Lz3]) / 3.0;
                        double Ldkdx = ((LKc1 - LKc2) * _ly01 + (K[Lp1] - K[p0]) * _ly23) / Ls2;
                        double Ldkdy = ((LKc1 - LKc2) * _lx10 + (K[Lp1] - K[p0]) * _lx32) / Ls2;
                        //double wk = tau * (LUcr * Ldkdx + LVcr * Ldkdy - (LNucr * (2 * Ldudx * Ldudx + (Ldvdx + Ldudy) * (Ldvdx + Ldudy) + 2 * Ldvdy * Ldvdy)) - LEcr);
                        //
                        //
                        //длина текущего фрагмента внешнего контура КО
                        double LLk = wrapper.Lk[p0][j];
                        //расчет потоков
                        double LpressU = -1.0 / rho_w * LPcr * Lnx;
                        double LconvU = -LUcr * LUcr * Lnx - (LUcr * LVcr) * Lny;
                        double LdiffU = 0;
                        //if ((Lp1 % mesh.CountLeft == 0) || ((Lp1 + 1) % mesh.CountLeft == 0 && surf_flag)) //только для нижней границы
                        //{
                        double Tau = CV_WallTau[i];
                        LdiffU = Tau / rho_w * Lny - nu_mol * 2.0 / 3.0 * (Ldudx + Ldvdy) * Lnx;
                        //}
                        //else
                        //    LdiffU = (nu_mol + LNucr) * (2.0 * Ldudx * Lnx - 2.0 / 3.0 * (Ldudx + Ldvdy) * Lnx + Ldudy * Lny + Ldvdx * Lny) - 2.0 / 3.0 * LKcr * Lnx;
                        double wx = Params.tau * (LUcr * Ldudx + LVcr * Ldudy + 1.0 / rho_w * Ldpdx + 2.0 / 3.0 * Ldkdx);
                        double wy = Params.tau * (LUcr * Ldvdx + LVcr * Ldvdy + 1.0 / rho_w * Ldpdy + 2.0 / 3.0 * Ldkdy);
                        double LregU1 = 2.0 * LUcr * wx * Lnx;
                        double LregU2 = (LVcr * wx + LUcr * wy) * Lny;
                        double LregU = LregU1 + LregU2;
                        LsummU += (LconvU + LdiffU + LregU + LpressU) * LLk;
                        //                  
                        double LpressV = -1.0 / rho_w * LPcr * Lny;
                        double LconvV = -(LUcr * LVcr) * Lnx - LVcr * LVcr * Lny;
                        double LdiffV = 0;
                        //if ((Lp1 % mesh.CountLeft == 0) || ((Lp1 + 1) % mesh.CountLeft == 0 && surf_flag)) //только для нижней границы
                        //{
                        //double Tau = CV_WallTau[i];
                        LdiffV = Tau / rho_w * Lnx - nu_mol * 2.0 / 3.0 * (Ldudx + Ldvdy) * Lny;
                        //}
                        //else
                        //    LdiffV = (nu_mol + LNucr) * (2.0 * Ldvdy * Lny - 2.0 / 3.0 * (Ldudx + Ldvdy) * Lny + Ldvdx * Lnx + Ldudy * Lnx) - 2.0 / 3.0 * LKcr * Lny;
                        double LregV1 = 2.0 * LVcr * wy * Lny;
                        double LregV2 = (LVcr * wx + LUcr * wy) * Lnx;
                        double LregV = LregV1 + LregV2;
                        LsummV += (LconvV + LdiffV + LregV + LpressV) * LLk;
                        //
                        double LconvS = -LScr * (LUcr + Ww * SinJ) * Lnx - LScr * (LVcr - Ww * CosJ) * Lny;
                        double LdiffS = LNucr / sigma_s * (Ldsdx * Lnx + Ldsdy * Lny);
                        double LregS = LScr * (wx * Lnx + wy * Lny);
                        LsummS += (LconvS + LdiffS + LregS) * LLk;
                    }
                    //
                    U[p0] = U[p0] + dt / wrapper.S0[p0] * LsummU;
                    V[p0] = V[p0] + dt / wrapper.S0[p0] * LsummV;
                    S[p0] = S[p0] + dt / wrapper.S0[p0] * LsummS;
                    //
                    if (double.IsNaN(U[p0]))
                    {
                        err = " U в бесконечность";
                        //iteration = iter;
                        break;
                    }
                    //}

                }

            });
            #endregion
            //если задача со свободной поверхностью
            //if (surf_flag)
            //    U[mesh.LeftKnots[0]] = U[mesh.TopKnots[mesh.TopKnots.Length - 2]];
            //ГУ Dong
            //if (iteration == iter - 1)
            //    DongBC_UE();
            //else
            //    DongBC_U();
            //ГУ справа - снос
            for (int i = 0; i < mesh.CountRight; i++)
            {
                int knot = mesh.RightKnots[i];
                //
                U[knot] = U[knot - mesh.CountRight];
                V[knot] = V[knot - mesh.CountRight];
                S[knot] = S[knot - mesh.CountRight];
            }
            for (int i = 0; i < mesh.CountTop; i++)
            {
                int knot = mesh.TopKnots[i];
                S[knot] = S[knot - 1];
                knot = mesh.BottomKnots[i];
                S[knot] = S[knot + 1];
            }
            //
            ////----------------
            //если задача со свободной поверхностью
            if (Params.surf_flag == true)
            {
                for (int i = 1; i < mesh.CountTop; i++)
                {
                    int knot = mesh.TopKnots[i];
                    U[knot] = U[knot - 1];
                    V[knot] = V[knot - 1];// или =0
                }
            }
            //
            return iteration;
        }



        #endregion

        #region Функии стенки
        /// <summary>
        /// Расчет k, omega на стенке
        /// </summary>
        void WallFuncKW()
        {
            int knot = 0;
            double u_tau = 0;
            for (int i = 0; i < wrapper.CV_WallKnots.Length; i++)
            {
                knot = wrapper.CV_WallKnots[i][0];
                u_tau = Math.Sqrt(Math.Abs(CV_WallTau[i] / rho_w));
                W[knot] = u_tau / (cm14 * cm14 * kappa * wrapper.CV_WallKnotsDistance[i]);
                K[knot] = u_tau * u_tau / cm14 / cm14;
            }
        }
        void Wilcox1988Boundary()
        {
            int knot = 0;
            for (int i = 0; i < wrapper.CV_WallKnots.Length; i++)
            {
                knot = wrapper.CV_WallKnots[i][0];
                W[knot] = 6.0 * nuT[knot] / beta / wrapper.CV_WallKnotsDistance[i] / wrapper.CV_WallKnotsDistance[i];
            }
        }
        double ks = 0.68 * 0.00016;
        void Wilcox2008Boundary()
        {
            int p = -1;
            //
            for (int i = 0; i < wrapper.CV_WallKnots.Length; i++)
            {
                int knot_p = wrapper.CV_WallKnots[i][0];
                double kp = K[knot_p];
                double Dy = wrapper.CV_WallKnotsDistance[i];
                double Du = U[knot_p];
                // По Волкову
                double Re = rho_w * cm14 * Math.Sqrt(Math.Abs(kp)) * Dy / mu;
                // tau
                double tauw = (rho_w * cm14 * Math.Sqrt(Math.Abs(kp)) * Du) / (1.0 / kappa * Math.Log(8.8 * Re));
                // квадрат u_tau
                double ut2 = tauw / rho_w;
                // -- шероховатость
                double ks_plus = Math.Sqrt(ut2) * ks / nuT[knot_p];
                //
                double SR = 0;
                if (ks_plus <= 5)
                    SR = (200.0 / ks_plus) * (200.0 / ks_plus);
                else
                    SR = 100.0 / ks_plus + ((200.0 / ks_plus) * (200.0 / ks_plus) - 100.0 / ks_plus) * Math.Exp(5 - ks_plus);
                //
                if ((knot_p - 1) % mesh.CountLeft == 0)
                    p = -1;
                else
                    p = 1;
                if (SR > 1000000)
                    SR /= 100000000;
                W[knot_p] = ut2 / nu_mol * SR;
            }
        }
        /// <summary>
        /// Функция стенки
        /// </summary>
        /// <param name="knot_p"></param>
        /// <param name="dy"></param>
        /// <returns></returns>
        private double WallFuncSnegirev(int knot_p, double dy)
        {
            double y_p_plus = cm14 * Math.Sqrt(K[knot_p]) * dy / nu_mol;
            double u2_tau = 0;
            if (y_p_plus > y_p_0)
                u2_tau = cm14 * cm14 * K[knot_p];
            else
                u2_tau = nu_mol * U[knot_p] / dy;
            double tau_w = u2_tau * rho_w;
            return tau_w;
        }
        /// <summary>
        /// Функция стенки по Волкову упрощенная
        /// </summary>
        /// <param name="knot_p">узел не на границе, но ближайший к ней</param>
        /// <param name="dy">расстояние от knot_p до дна </param>
        /// <returns></returns>
        private double WallFunc(int knot_p, double dy)
        {
            double kp = K[knot_p];
            double Dy = dy;
            double Du = U[knot_p];
            double Re = rho_w * cm14 * Math.Sqrt(Math.Abs(kp)) * Dy / mu;
            // tau
            double tauw = (rho_w * cm14 * Math.Sqrt(Math.Abs(kp)) * Du) / (1.0 / kappa * Math.Log(8.8 * Re));
            // квадрат u_tau
            //double ut2 = tauw / rho_w;
            //double k = ut2 / Math.Sqrt(C_m);
            //K[knot_p] = k;
            //double eps = ut2 * Math.Sqrt(ut2) / kappa / Dy;
            //E[knot_p] = eps;
            //nuT[knot_p] = C_m * k * k / eps + nu_mol;
            return tauw;
        }
        /// <summary>
        /// Функция стенки по Луцкому с буферным слоем
        /// </summary>
        /// <param name="knot_p">узел не на границе, но ближайший к ней</param>
        /// <param name="dy">расстояние от knot_p до дна </param>
        /// <returns></returns>
        private double WallFuncPlus(int knot_p, double dy)
        {
            double y1 = dy;
            double F = Math.Abs(U[knot_p] * y1 / nu_mol);
            double y_plus = 0;
            if (F <= 2) // ламинарный слой
                y_plus = Math.Sqrt(2 * F);
            else if (F >= 696.68) // логарифмический слой
            {
                double y_i = 60;
                double F_i = 0, u_i = 0, y_i1 = 0;
                for (int i = 0; i < 7; i++)
                {
                    F_i = 2.5 * y_i * (Math.Log(y_i / 0.13) - 1) - 73.50481;
                    u_i = 2.5 * Math.Log(y_i / 0.13);
                    y_i1 = y_i + (F - F_i) / u_i;
                    if (Math.Abs(y_i - y_i1) < 0.000001)
                    {
                        y_i = y_i1;
                        break;
                    }
                    y_i = y_i1;
                }
                y_plus = y_i1;
            }
            else // буферный слой
                y_plus = YPlus.Yplus(F);
            // tau
            double utau = y_plus / y1 * nu_mol;
            double k = utau * utau / Math.Sqrt(C_m);
            //K[knot_p] = k;
            double eps = utau * utau * utau / kappa / y1;
            //E[knot_p] = eps;
            //nuT[knot_p] = C_m * k * k / eps + nu_mol;
            double tauw = Math.Sign(U[knot_p]) * utau * utau * rho_w;
            return tauw;
        }
        /// <summary>
        /// Функция стенки для шероховатой функции по алгоритму Луцкого без буферного слоя
        /// </summary>
        /// <param name="knot_p">узел не на границе, но ближайший к ней</param>
        /// <param name="dy">расстояние от knot_p до дна </param>
        /// <returns></returns>
        private double WallFuncSharpPlus(int knot_p, double dy)
        {
            //double y1 = Math.Abs(Y[knot_p] - Y[knot]);
            double delta = 0.6 * d;
            double y1 = dy;
            double F = Math.Abs(U[knot_p] * y1 / nu_mol);
            double y_plus = 0;
            if (F <= 782.4968) // ламинарный слой
                y_plus = Math.Sqrt(2 * F);
            else  // логарифмический слой
            {
                double y_i = 39.56;
                double F_i = 0, u_i = 0, y_i1 = 0;
                for (int i = 0; i < 7; i++)
                {
                    F_i = 2.5 * y_i * (Math.Log(20.0855 * y_i / delta) - 1) - 683.559072;
                    u_i = 2.5 * Math.Log(20.0855 * y_i / delta);
                    y_i1 = y_i + (F - F_i) / u_i;
                    if (Math.Abs(y_i - y_i1) < 0.000001)
                    {
                        y_i = y_i1;
                        break;
                    }
                    y_i = y_i1;
                }
                y_plus = y_i1;
            }
            // буферный слой отсутствует
            // tau
            double utau = y_plus / y1 * nu_mol;
            double k = utau * utau / Math.Sqrt(C_m);
            //K[knot_p] = k;
            double eps = utau * utau * utau / kappa / y1;
            //E[knot_p] = eps;
            //nuT[knot_p] = C_m * k * k / eps + nu_mol;
            double tauw = Math.Sign(U[knot_p]) * utau * utau * rho_w;
            return tauw;
        }
        //
        /// <summary>
        /// Функция стенки по Волкову через метод Ньютона
        /// </summary>
        /// <param name="knot_p">узел не на границе, но ближайший к ней</param>
        /// <param name="dy">расстояние от knot_p до дна </param>
        /// <returns></returns>
        private double WallFuncVolkov(int knot_p, double dy)
        {
            double kp = K[knot_p];
            double Dy = dy;
            double Du = U[knot_p];
            double Re = rho_w * cm14 * Math.Sqrt(Math.Abs(kp)) * Dy / mu;
            double B = 5.3; // и у Волкова и есть
            //
            double u_plus = 0;
            double u_c = Math.Sqrt(Re);
            double f = 0, df = 0;
            if (Re <= 140)
            {
                u_c = Math.Sqrt(Re);
                //
                for (int i = 0; i < 5; i++)
                {
                    f = u_c + (Math.Exp(kappa * u_c) - 1 - kappa * u_c - 0.5 * kappa * u_c * kappa * u_c - 1.0 / 6.0 * kappa * u_c * kappa * u_c * kappa * u_c) * Math.Exp(-kappa * B) - Re / u_c;
                    df = 1 + (kappa * Math.Exp(kappa * u_c) - kappa - kappa * kappa * u_c - 0.5 * kappa * kappa * kappa * u_c * u_c) * Math.Exp(-kappa * B) + Re / u_c / u_c;
                    u_plus = u_c - f / df;
                    u_c = u_plus;
                }
            }
            else
            {
                u_c = 1.0 / kappa * Math.Log(Re) + B;
                //
                for (int i = 0; i < 5; i++)
                {
                    f = u_c - B - 1.0 / kappa * Math.Log(Math.Exp(-kappa * B) * (1 + kappa * u_c + 1.0 / 2.0 * kappa * kappa * 
                        u_c * u_c + 1.0 / 6.0 * kappa * kappa * kappa * u_c * u_c * u_c) + Re / u_c - u_c);
                    df = 1 - (Math.Exp(-kappa * B) * (kappa + kappa * kappa * u_c + 1.0 / 2.0 * kappa * kappa * kappa * u_c * u_c) - 
                        Re / u_c / u_c - 1) / (Math.Exp(-kappa * B) * (1 + kappa * u_c + 1.0 / 2.0 * kappa * kappa * 
                        u_c * u_c + 1.0 / 6.0 * kappa * kappa * kappa * u_c * u_c * u_c) + Re / u_c - u_c) / kappa;
                    u_plus = u_c - f / df;
                    u_c = u_plus;
                }
            }



            // tau
            double tauw1 = (rho_w * cm14 * Math.Sqrt(Math.Abs(kp)) * Du) / (1.0 / kappa * Math.Log(8.8 * Re));
            //
            double tauw = Math.Sign(Du) * rho_w * Du * Du / u_plus / u_plus;
            // квадрат u_tau
            //double ut2 = tauw / rho_w;
            //double k = ut2 / Math.Sqrt(C_m);
            //K[knot_p] = k;
            //double eps = ut2 * Math.Sqrt(ut2) / kappa / Dy;
            //E[knot_p] = eps;
            //nuT[knot_p] = C_m * k * k / eps + nu_mol;
            return tauw;
        }
        #endregion

        #region Напряжения на дне и в области (основные модули в реализации методов IRiver)
        /// <summary>
        /// Расчет TauXY во всех узлах по КО
        /// </summary>
        /// <returns></returns>
        public double[] CalcTauEverywhere()
        {
            double[] tau = new double[CountKnots];
            if (OrdPart_CV == null)
                OrdPart_CV = Partitioner.Create(0, wrapper.CVolumes.Length);
            //
            #region Расчет Tau во всех узлах по КО
            // По МКО находим касательное напряжение во внутренних узлах
            int knot = 0;
            //OrderablePartitioner<Tuple<int, int>> OrdPartitioner_Tau = Partitioner.Create(0, mesh.CVolumes.Length);
            Parallel.ForEach(OrdPart_CV,
                    (range, loopState) =>
                    {
                        for (int i = range.Item1; i < range.Item2; i++)
                        //for (int i = 0; i < mesh.CVolumes.Length; i++)
                        {
                            double LsummTau = 0;//потоки касательного напряжения
                            //
                            int p0 = wrapper.CVolumes[i][0];
                            int jj = wrapper.CVolumes[i].Length - 1;//количество КО, связанных с данным узлом
                            for (int j = 0; j < jj; j++)
                            {
                                //вторая точка общей грани
                                int Lp1 = wrapper.P1[p0][j];
                                //внешняя нормаль к грани КО (контуру КО)
                                double Lnx = wrapper.Nx[p0][j]; double Lny = wrapper.Ny[p0][j];
                                ////значение функций в точке пересечения грани КО и основной грани
                                double Lalpha = wrapper.Alpha[p0][j];
                                double LUcr = Lalpha * U[p0] + (1 - Lalpha) * U[Lp1];
                                double LVcr = Lalpha * V[p0] + (1 - Lalpha) * V[Lp1];
                                //длина текущего фрагмента внешнего контура КО
                                double LLk = wrapper.Lk[p0][j];
                                //расчет потоков
                                double LTau = LUcr * Lny + LVcr * Lnx;
                                //
                                LsummTau += LTau * LLk;
                            }
                            //
                            tau[p0] = nuT[p0] * rho_w / wrapper.S0[p0] * LsummTau; // * 2
                            //
                            if (double.IsNaN(tau[p0]))
                            {
                                err = " tau в бесконечность";
                                //iteration = iter;
                                break;
                            }
                            //}

                        }

                    });
            #endregion
          
            #region Нахождение tau на границах
            
            for (int i = 0; i < mesh.CountBottom; i++)
            {
                knot = mesh.BottomKnots[i];
                tau[knot] = BTauC[i];
                knot = mesh.BottomKnots[i] + 1;
                //tau[knot] = WallFuncSharpPlus(knot, mesh.GetNormalDistanceBottom(knot));
                //tau[knot] = WallFunc(knot, mesh.GetNormalDistanceBottom(knot));
                //tau[knot] = WallFuncVolkov(knot, mesh.GetNormalDistanceBottom(knot));
            }
            // 
            for (int i = 0; i < mesh.CountLeft; i++)
            {
                knot = mesh.LeftKnots[i];
                tau[knot] = tau[knot + mesh.CountLeft];
            }
            //
            for (int i = 0; i < mesh.CountRight; i++)
            {
                knot = mesh.RightKnots[i];
                tau[knot] = tau[knot - mesh.CountLeft];
            }
            //
            if (Params.surf_flag == true)
            {
                for (int i = 0; i < mesh.CountTop; i++)
                {
                    knot = mesh.TopKnots[i];
                    tau[knot] = tau[knot - 1];
                }
            }
            else
            {
                for (int i = 0; i < mesh.CountTop; i++)
                {
                    knot = mesh.TopKnots[i];
                    tau[knot] = TTauC[i];
                }
            }
            #endregion
            //
            return tau;
        }

        /// <summary>
        /// нахождение размерного касательного напряжения на дне
        /// </summary>
        void ShearStresses()
        {
            System.Object lockThis = new System.Object();
            //
            int IndexMethod = 0;// через ленту tau_l
            IndexMethod = 2;// через ленту тензор TT
            //IndexMethod = 1;// через конечные разности -  для сильновырожденной области не годится
            try
            {
                if (IndexMethod == 2)
                {
                    //компоненты тензора напряжений
                    double[] Tx1 = new double[wrapper.BTriangles.Length];
                    double[] Tx2 = new double[wrapper.BTriangles.Length];
                    double[] Ty1 = new double[wrapper.BTriangles.Length];
                    double[] Ty2 = new double[wrapper.BTriangles.Length];
                    //
                    double[] tau_mid = new double[wrapper.BTriangles.Length];
                    //double e1x = 1, e1y = 0, e2x = 0, e2y = 1;//проекции
                    //
                    OrderablePartitioner<Tuple<int, int>> OrdPartitioner_Tau = Partitioner.Create(0, wrapper.BTriangles.Length);
                    Parallel.ForEach(OrdPartitioner_Tau,
                           (range, loopState) =>
                           {
                               for (int i = range.Item1; i < range.Item2; i++)
                               //for (int i = 0; i < mesh.BTriangles.Length; i++)
                               {

                                   double[] xL = new double[3];
                                   double[] yL = new double[3];
                                   //
                                   uint LcurV = wrapper.BTriangles[i];
                                   uint[] Knots = mesh.AreaElems[LcurV];
                                   //
                                   uint Lnum1 = Knots[0];
                                   uint Lnum2 = Knots[1];
                                   uint Lnum3 = Knots[2];
                                   // получаем координаты узлов элемента
                                   xL[0] = X[Lnum1]; xL[1] = X[Lnum2]; xL[2] = X[Lnum3];
                                   yL[0] = Y[Lnum1]; yL[1] = Y[Lnum2]; yL[2] = Y[Lnum3];
                                   // нахождение площади треугольника
                                   double LS = wrapper.Sk[LcurV];
                                   // скорости в вершинах треугольника
                                   double LU1 = U[Lnum1];
                                   double LU2 = U[Lnum2];
                                   double LU3 = U[Lnum3];
                                   //
                                   double LV1 = V[Lnum1];
                                   double LV2 = V[Lnum2];
                                   double LV3 = V[Lnum3];
                                   // производные в центре треугольника
                                   double Ldu_dx = 1.0f / 2.0f / LS * (LU1 * (yL[1] - yL[2]) + LU2 * (yL[2] - yL[0]) + LU3 * (yL[0] - yL[1]));
                                   double Ldu_dy = 1.0f / 2.0f / LS * (LU1 * (xL[2] - xL[1]) + LU2 * (xL[0] - xL[2]) + LU3 * (xL[1] - xL[0]));
                                   double Ldv_dx = 1.0f / 2.0f / LS * (LV1 * (yL[1] - yL[2]) + LV2 * (yL[2] - yL[0]) + LV3 * (yL[0] - yL[1]));
                                   double Ldv_dy = 1.0f / 2.0f / LS * (LV1 * (xL[2] - xL[1]) + LV2 * (xL[0] - xL[2]) + LV3 * (xL[1] - xL[0]));
                                   // давление в центре треугольника
                                   double LP_mid = (P[Lnum1] + P[Lnum2] + P[Lnum3]) / 3.0f;
                                   double Mu_mid = (nuT[Lnum1] + nuT[Lnum2] + nuT[Lnum3]) / 3.0f * rho_w;
                                   // составляющие тензора напряжений
                                   Tx1[i] = -LP_mid + 2.0f * Mu_mid * Ldu_dx;
                                   Tx2[i] = Mu_mid * (Ldu_dy + Ldv_dx);
                                   Ty1[i] = Mu_mid * (Ldu_dy + Ldv_dx);
                                   Ty2[i] = -LP_mid + 2.0f * Mu_mid * Ldv_dy;
                               }
                           });
                    // Вычисление tau в узлах сетки по сглаженной методике вычисления 
                    //double[] Tau_all = Aproximate(tau_mid, mesh.CBottom, mesh.BTriangles);

                    //вычисление тензора напряжений по сглаженной методике
                    double[] Tx1_all = Aproximate(Tx1, wrapper.CBottom, wrapper.BTriangles);
                    double[] Tx2_all = Aproximate(Tx2, wrapper.CBottom, wrapper.BTriangles);
                    double[] Ty1_all = Aproximate(Ty1, wrapper.CBottom, wrapper.BTriangles);
                    double[] Ty2_all = Aproximate(Ty2, wrapper.CBottom, wrapper.BTriangles);
                    //подготовка приграничных значений tau между граничными точками и координат для сплайна
                    int count = mesh.BottomKnots.Length;
                    BTau = new double[count];
                    BTauC = new double[count];
                    arg = new double[count - 1];
                    int cKnot = mesh.BottomKnots[0];
                    double prevTx1 = 0, prevTx2 = 0, prevTy1 = 0, prevTy2 = 0;
                    //
                    for (int i = 0; i < mesh.BottomKnots.Length; i++)
                    {
                        cKnot = mesh.BottomKnots[i];
                        for (int j = 0; j < wrapper.CBottom.Length; j++)
                        {
                            if (cKnot == wrapper.CBottom[j])
                            {
                                if (i != 0)
                                {
                                    int prevKnot = mesh.BottomKnots[i - 1];
                                    double delx = (X[cKnot] - X[prevKnot]);
                                    double dely = (Y[cKnot] - Y[prevKnot]);
                                    double ss = (double)(Math.Sqrt(delx * delx + dely * dely));
                                    // компоненты касательной между граничными точками
                                    double lx = 1 / ss * delx;
                                    double ly = 1 / ss * dely;
                                    double nx = -ly;
                                    double ny = lx;
                                    //
                                    double LsigX = (prevTx1 + Tx1_all[j]) / 2.0f * nx + (prevTx2 + Tx2_all[j]) / 2.0f * ny;//умножили (Tx1;Tx2) на (0;-1) - нормаль к поверхности
                                    double LsigY = (prevTy1 + Ty1_all[j]) / 2.0f * nx + (prevTy2 + Ty2_all[j]) / 2.0f * ny;//умножили (Ty1;Ty2) на (0;-1) - нормаль к поверхности
                                    //double LsigX = 2 * prevTx1 * Tx1_all[j] / (prevTx1 + Tx1_all[j]) * nx + 2 * prevTx2 * Tx2_all[j] / (prevTx2 + Tx2_all[j]) * ny;//умножили (Tx1;Tx2) на (0;-1) - нормаль к поверхности
                                    //double LsigY = 2 * prevTy1 * Ty1_all[j] / (prevTy1 + Ty1_all[j]) * nx + 2 * prevTy2 * Ty2_all[j] / (prevTy2 + Ty2_all[j]) * ny;//умножили (Ty1;Ty2) на (0;-1) - нормаль к поверхности
                                    //в полуцелых узлах
                                    BTau[i - 1] = (LsigX * lx + LsigY * ly);
                                    arg[i - 1] = X[prevKnot] + delx / 2.0f;
                                    //
                                    LsigX = Tx1_all[j] * nx + Tx2_all[j] * ny;//умножили (Tx1;Tx2) на (0;-1) - нормаль к поверхности
                                    LsigY = Ty1_all[j] * nx + Ty2_all[j] * ny;//умножили (Ty1;Ty2) на (0;-1) - нормаль к поверхности
                                    BTauC[i] = (LsigX * lx + LsigY * ly);

                                }
                                else
                                {
                                    int nextKnot = mesh.BottomKnots[i + 1];
                                    double delx = (X[nextKnot] - X[cKnot]);
                                    double dely = (Y[nextKnot] - Y[cKnot]);
                                    double ss = (double)(Math.Sqrt(delx * delx + dely * dely));
                                    // компоненты касательной между граничными точками
                                    double lx = 1 / ss * delx;
                                    double ly = 1 / ss * dely;
                                    double nx = -ly;
                                    double ny = lx;
                                    //
                                    double LsigX = Tx1_all[j] * nx + Tx2_all[j] * ny;//умножили (Tx1;Tx2) на (0;-1) - нормаль к поверхности
                                    double LsigY = Ty1_all[j] * nx + Ty2_all[j] * ny;//умножили (Ty1;Ty2) на (0;-1) - нормаль к поверхности
                                    BTauC[i] = (LsigX * lx + LsigY * ly);
                                }
                                //
                                prevTx1 = Tx1_all[j];
                                prevTx2 = Tx2_all[j];
                                prevTy1 = Ty1_all[j];
                                prevTy2 = Ty2_all[j];
                                break;
                            }
                        }
                    }
                    BTau[count - 1] = BTau[count - 2];
                    /////
                    Tx1 = new double[wrapper.TTrianglesKnots.Length];
                    Tx2 = new double[wrapper.TTrianglesKnots.Length];
                    Ty1 = new double[wrapper.TTrianglesKnots.Length];
                    Ty2 = new double[wrapper.TTrianglesKnots.Length];
                    //
                    tau_mid = new double[wrapper.TTrianglesKnots.Length];
                    //double e1x = 1, e1y = 0, e2x = 0, e2y = 1;//проекции
                    //
                    OrdPartitioner_Tau = Partitioner.Create(0, wrapper.TTrianglesKnots.Length);
                    Parallel.ForEach(OrdPartitioner_Tau,
                           (range, loopState) =>
                           {
                               for (int i = range.Item1; i < range.Item2; i++)
                               //for (int i = 0; i < mesh.BTriangles.Length; i++)
                               {

                                   double[] xL = new double[3];
                                   double[] yL = new double[3];
                                   //
                                   uint LcurV = wrapper.TTriangles[i];
                                   uint[] Knots = mesh.AreaElems[LcurV];
                                   //
                                   uint Lnum1 = Knots[0];
                                   uint Lnum2 = Knots[1];
                                   uint Lnum3 = Knots[2];
                                   // получаем координаты узлов элемента
                                   xL[0] = X[Lnum1]; xL[1] = X[Lnum2]; xL[2] = X[Lnum3];
                                   yL[0] = Y[Lnum1]; yL[1] = Y[Lnum2]; yL[2] = Y[Lnum3];
                                   // нахождение площади треугольника
                                   double LS = wrapper.Sk[LcurV];
                                   // скорости в вершинах треугольника
                                   double LU1 = U[Lnum1];
                                   double LU2 = U[Lnum2];
                                   double LU3 = U[Lnum3];
                                   //
                                   double LV1 = V[Lnum1];
                                   double LV2 = V[Lnum2];
                                   double LV3 = V[Lnum3];
                                   // производные в центре треугольника
                                   double Ldu_dx = 1.0f / 2.0f / LS * (LU1 * (yL[1] - yL[2]) + LU2 * (yL[2] - yL[0]) + LU3 * (yL[0] - yL[1]));
                                   double Ldu_dy = 1.0f / 2.0f / LS * (LU1 * (xL[2] - xL[1]) + LU2 * (xL[0] - xL[2]) + LU3 * (xL[1] - xL[0]));
                                   double Ldv_dx = 1.0f / 2.0f / LS * (LV1 * (yL[1] - yL[2]) + LV2 * (yL[2] - yL[0]) + LV3 * (yL[0] - yL[1]));
                                   double Ldv_dy = 1.0f / 2.0f / LS * (LV1 * (xL[2] - xL[1]) + LV2 * (xL[0] - xL[2]) + LV3 * (xL[1] - xL[0]));
                                   // давление в центре треугольника
                                   double LP_mid = (P[Lnum1] + P[Lnum2] + P[Lnum3]) / 3.0f;
                                   double Mu_mid = (nuT[Lnum1] + nuT[Lnum2] + nuT[Lnum3]) / 3.0f * rho_w;
                                   // составляющие тензора напряжений
                                   Tx1[i] = -LP_mid + 2.0f * Mu_mid * Ldu_dx;
                                   Tx2[i] = Mu_mid * (Ldu_dy + Ldv_dx);
                                   Ty1[i] = Mu_mid * (Ldu_dy + Ldv_dx);
                                   Ty2[i] = -LP_mid + 2.0f * Mu_mid * Ldv_dy;
                               }
                           });
                    // Вычисление tau в узлах сетки по сглаженной методике вычисления 
                    //double[] Tau_all = Aproximate(tau_mid, mesh.CBottom, mesh.BTriangles);

                    //вычисление тензора напряжений по сглаженной методике
                    Tx1_all = Aproximate(Tx1, wrapper.CTop, wrapper.TTriangles);
                    Tx2_all = Aproximate(Tx2, wrapper.CTop, wrapper.TTriangles);
                    Ty1_all = Aproximate(Ty1, wrapper.CTop, wrapper.TTriangles);
                    Ty2_all = Aproximate(Ty2, wrapper.CTop, wrapper.TTriangles);
                    //подготовка приграничных значений tau между граничными точками и координат для сплайна
                    count = mesh.TopKnots.Length;
                    TTau = new double[count];
                    TTauC = new double[count];
                    argT = new double[count - 1];
                    cKnot = mesh.TopKnots[0];
                    prevTx1 = 0; prevTx2 = 0; prevTy1 = 0; prevTy2 = 0;
                    //
                    for (int i = 0; i < mesh.TopKnots.Length; i++)
                    {
                        cKnot = mesh.TopKnots[i];
                        for (int j = 0; j < wrapper.CTop.Length; j++)
                        {
                            if (cKnot == wrapper.CTop[j])
                            {
                                if (i != 0)
                                {
                                    int prevKnot = mesh.TopKnots[i - 1];
                                    double delx = (X[cKnot] - X[prevKnot]);
                                    double dely = (Y[cKnot] - Y[prevKnot]);
                                    double ss = (double)(Math.Sqrt(delx * delx + dely * dely));
                                    // компоненты касательной между граничными точками
                                    double lx = 1 / ss * delx;
                                    double ly = 1 / ss * dely;
                                    double nx = -ly;
                                    double ny = lx;
                                    //
                                    double LsigX = (prevTx1 + Tx1_all[j]) / 2.0f * nx + (prevTx2 + Tx2_all[j]) / 2.0f * ny;//умножили (Tx1;Tx2) на (0;-1) - нормаль к поверхности
                                    double LsigY = (prevTy1 + Ty1_all[j]) / 2.0f * nx + (prevTy2 + Ty2_all[j]) / 2.0f * ny;//умножили (Ty1;Ty2) на (0;-1) - нормаль к поверхности
                                    //в полуцелых узлах
                                    TTau[i - 1] = (LsigX * lx + LsigY * ly);
                                    argT[i - 1] = X[prevKnot] + delx / 2.0f;
                                    //
                                    LsigX = Tx1_all[j] * nx + Tx2_all[j] * ny;//умножили (Tx1;Tx2) на (0;-1) - нормаль к поверхности
                                    LsigY = Ty1_all[j] * nx + Ty2_all[j] * ny;//умножили (Ty1;Ty2) на (0;-1) - нормаль к поверхности
                                    TTauC[i] = (LsigX * lx + LsigY * ly);

                                }
                                else
                                {
                                    int nextKnot = mesh.TopKnots[i + 1];
                                    double delx = (X[nextKnot] - X[cKnot]);
                                    double dely = (Y[nextKnot] - Y[cKnot]);
                                    double ss = (double)(Math.Sqrt(delx * delx + dely * dely));
                                    // компоненты касательной между граничными точками
                                    double lx = 1 / ss * delx;
                                    double ly = 1 / ss * dely;
                                    double nx = -ly;
                                    double ny = lx;
                                    //
                                    double LsigX = Tx1_all[j] * nx + Tx2_all[j] * ny;//умножили (Tx1;Tx2) на (0;-1) - нормаль к поверхности
                                    double LsigY = Ty1_all[j] * nx + Ty2_all[j] * ny;//умножили (Ty1;Ty2) на (0;-1) - нормаль к поверхности
                                    TTauC[i] = (LsigX * lx + LsigY * ly);
                                }
                                //
                                prevTx1 = Tx1_all[j];
                                prevTx2 = Tx2_all[j];
                                prevTy1 = Ty1_all[j];
                                prevTy2 = Ty2_all[j];
                                break;
                            }
                        }
                    }
                    TTau[count - 1] = TTau[count - 2];

                }
                if (IndexMethod == 0) //!!!-- 1 ый порядок точности, для Tau в целых и полуцелых узлах надо использовать через сплайн!
                {
                    //
                    double[] tau_mid = new double[wrapper.BTriangles.Length];
                    //
                    OrderablePartitioner<Tuple<int, int>> OrdPartitioner_Tau = Partitioner.Create(0, wrapper.BTriangles.Length);
                    Parallel.ForEach(OrdPartitioner_Tau,
                           (range, loopState) =>
                           {
                               for (int i = range.Item1; i < range.Item2; i++)
                               //for (int i = 0; i < mesh.BTriangles.Length; i++)
                               {

                                   double[] xL = new double[3];
                                   double[] yL = new double[3];
                                   //
                                   uint LcurV = wrapper.BTriangles[i];
                                   uint[] Knots = mesh.AreaElems[LcurV];
                                   //
                                   uint Lnum1 = Knots[0];
                                   uint Lnum2 = Knots[1];
                                   uint Lnum3 = Knots[2];
                                   // получаем координаты узлов элемента
                                   xL[0] = X[Lnum1]; xL[1] = X[Lnum2]; xL[2] = X[Lnum3];
                                   yL[0] = Y[Lnum1]; yL[1] = Y[Lnum2]; yL[2] = Y[Lnum3];
                                   // нахождение площади треугольника
                                   double LS = wrapper.Sk[LcurV];
                                   // скорости в вершинах треугольника
                                   double LU1 = U[Lnum1];
                                   double LU2 = U[Lnum2];
                                   double LU3 = U[Lnum3];
                                   //
                                   double LV1 = V[Lnum1];
                                   double LV2 = V[Lnum2];
                                   double LV3 = V[Lnum3];
                                   // касательный вектор (обход против часовой стрелки)
                                   double Lsx = wrapper.Sx[i];
                                   double Lsy = wrapper.Sy[i];
                                   // нормаль (направлена во внутрь КО)
                                   double Lnx = -Lsy;
                                   double Lny = Lsx;
                                   // производные в центре треугольника
                                   double Ldu_dx = 1.0f / 2.0f / LS * (LU1 * (yL[1] - yL[2]) + LU2 * (yL[2] - yL[0]) + LU3 * (yL[0] - yL[1]));
                                   double Ldu_dy = 1.0f / 2.0f / LS * (LU1 * (xL[2] - xL[1]) + LU2 * (xL[0] - xL[2]) + LU3 * (xL[1] - xL[0]));
                                   double Ldv_dx = 1.0f / 2.0f / LS * (LV1 * (yL[1] - yL[2]) + LV2 * (yL[2] - yL[0]) + LV3 * (yL[0] - yL[1]));
                                   double Ldv_dy = 1.0f / 2.0f / LS * (LV1 * (xL[2] - xL[1]) + LV2 * (xL[0] - xL[2]) + LV3 * (xL[1] - xL[0]));
                                   // давление в центре треугольника
                                   double LP_mid = (P[Lnum1] + P[Lnum2] + P[Lnum3]) / 3.0f;
                                   double Mu_mid = (nuT[Lnum1] + nuT[Lnum2] + nuT[Lnum3]) / 3.0f * rho_w;
                                   // составляющие тензора напряжений
                                   double Tx1 = -LP_mid + 2.0f * Mu_mid * Ldu_dx;
                                   double Tx2 = Mu_mid * (Ldu_dy + Ldv_dx);
                                   double Ty1 = Mu_mid * (Ldu_dy + Ldv_dx);
                                   double Ty2 = -LP_mid + 2.0f * Mu_mid * Ldv_dy;
                                   // компоненты вектора придонного напряжения
                                   double LsigX = Tx1 * Lnx + Tx2 * Lny;
                                   double LsigY = Ty1 * Lnx + Ty2 * Lny;
                                   //
                                   tau_mid[i] = (LsigX * Lsx + LsigY * Lsy);
                               }
                           });

                    // Вычисление tau в узлах сетки по сглаженной методике вычисления 
                    double[] Tau_all = Aproximate(tau_mid, wrapper.CBottom, wrapper.BTriangles);
                    //
                    //подготовка приграничных значений tau между граничными точками и координат для сплайна
                    int count = mesh.BottomKnots.Length;
                    BTau = new double[count];
                    arg = new double[count];
                    int cKnot;
                    //
                    for (int i = 0; i < mesh.BottomKnots.Length; i++)
                    {
                        cKnot = mesh.BottomKnots[i];
                        for (int j = 0; j < wrapper.CBottom.Length; j++)
                        {
                            if (cKnot == wrapper.CBottom[j])
                            {

                                BTau[i] = Tau_all[j];
                                arg[i] = X[cKnot]; // аргумент - только X (чтобы легче проецировать на дно)
                                //arg[i] = Math.Sqrt(X[cKnot] * X[cKnot] + Y[cKnot] * Y[cKnot]);
                                break;
                            }
                        }
                    }
                }
                if (IndexMethod == 1)//!!!-- 1 ый порядок точности, для Tau в целых и полуцелых узлах надо использовать через сплайн!
                {
                    //подготовка приграничных значений tau между граничными точками и координат для сплайна
                    arg = new double[mesh.BottomKnots.Length];
                    //
                    OrderablePartitioner<Tuple<int, int>> OrdPartitioner_Tau = Partitioner.Create(1, mesh.BottomKnots.Length - 1);
                    Parallel.ForEach(OrdPartitioner_Tau,
                           (range, loopState) =>
                           {
                               for (int i = range.Item1; i < range.Item2; i++)
                               //for (int i = 1; i < mesh.BottomKnots.Length - 1; i++)
                               {
                                   //
                                   //           nnK(2)
                                   //            |
                                   //           nK(1)
                                   //            |
                                   //--wK(3)---cKnot(0)---eK(4)
                                   int cKnot = mesh.BottomKnots[i];
                                   int wK = mesh.BottomKnots[i - 1];
                                   int eK = mesh.BottomKnots[i + 1];
                                   int nK = 0, nnK = 0;
                                   double delx = (X[eK] - X[wK]);
                                   double dely = (Y[eK] - Y[wK]);
                                   double ss = (double)(Math.Sqrt(delx * delx + dely * dely));
                                   // компоненты касательной между граничными точками
                                   double lx = 1 / ss * delx;
                                   double ly = 1 / ss * dely;
                                   //нормаль к поверхности между узлами wK и eK
                                   double nx = -ly;
                                   double ny = lx;
                                   //
                                   double Ldu_dx = 0;
                                   double Ldu_dy = 0;
                                   double Ldv_dx = 0;
                                   double Ldv_dy = 0;
                                   double P_c = 0;
                                   //если дно почти горизонтальное, то считаем, что оно горизонтальное
                                   if (lx > 0.98)
                                   {
                                       nK = cKnot + 1;
                                       nnK = cKnot + 2;
                                       //x - это y(координата), y - это U (функция)
                                       double x0 = Y[cKnot];
                                       double y0 = U[cKnot];
                                       double x1 = Y[nK];
                                       double y1 = U[nK];
                                       double x2 = Y[nnK];
                                       double y2 = U[nnK];
                                       Ldu_dy = 1.0f / (x0 - x1) * y0 + 1.0f / (x0 - x2) * y0 + 1.0f / (x1 - x0) * (x0 - x2) / (x1 - x2) * y1 + 1.0f / (x2 - x0) * (x0 - x1) / (x2 - x1) * y2;
                                       //функция V - это y
                                       y0 = V[cKnot];
                                       y1 = V[nK];
                                       y2 = V[nnK];
                                       Ldv_dy = 1.0f / (x0 - x1) * y0 + 1.0f / (x0 - x2) * y0 + 1.0f / (x1 - x0) * (x0 - x2) / (x1 - x2) * y1 + 1.0f / (x2 - x0) * (x0 - x1) / (x2 - x1) * y2;
                                       Ldv_dx = 0;
                                       Ldu_dx = 0;
                                   }
                                   // если дно не горизонтальное
                                   else
                                   {

                                       double k = dy;
                                       HPoint[] PointsNorm = new HPoint[2];
                                       HPoint[] PointsTang = new HPoint[2];
                                       double[] U12 = new double[2]; double[] V12 = new double[2];
                                       double[] U34 = new double[2]; double[] V34 = new double[2];
                                       //
                                       for (int j = 0; j < 2; j++)
                                       {
                                           //точка на расстоянии k по направлению нормали
                                           PointsNorm[j] = new HPoint(X[cKnot] + nx * k * (j + 1), Y[cKnot] + ny * k * (j + 1));
                                           //
                                           int Triangle = mesh.GetTriangle(PointsNorm[j].x, PointsNorm[j].y);
                                           //
                                           double s05 = 1.0f / 2.0f / wrapper.Sk[Triangle];
                                           uint[] Knots = mesh.AreaElems[Triangle];
                                           double x1 = X[Knots[0]]; double x2 = X[Knots[1]]; double x3 = X[Knots[2]];
                                           double y1 = Y[Knots[0]]; double y2 = Y[Knots[1]]; double y3 = Y[Knots[2]];
                                           double L1 = s05 * ((x2 * y3 - x3 * y2) + PointsNorm[j].x * (y2 - y3) + PointsNorm[j].y * (x3 - x2));
                                           double L2 = s05 * ((x3 * y1 - x1 * y3) + PointsNorm[j].x * (y3 - y1) + PointsNorm[j].y * (x1 - x3));
                                           double L3 = s05 * ((x1 * y2 - x2 * y1) + PointsNorm[j].x * (y1 - y2) + PointsNorm[j].y * (x2 - x1));
                                           //
                                           U12[j] = L1 * U[Knots[0]] + L2 * U[Knots[1]] + L3 * U[Knots[2]];
                                           V12[j] = L1 * V[Knots[0]] + L2 * V[Knots[1]] + L3 * V[Knots[2]];
                                       }
                                       //аппроксимация по 4 точкам
                                       //double dPhids = (2 * PhiNormal[0] - 5 * PhiNormal[1] + 4 * PhiNormal[2] - PhiNormal[3]) / k / k;
                                       //аппрроксимация по трем точкам
                                       Ldu_dy = (-7 * U[cKnot] + 8 * U12[0] - U12[1]) / k / k / 2.0f;
                                       Ldv_dy = (-7 * V[cKnot] + 8 * V12[0] - V12[1]) / k / k / 2.0f;
                                       //
                                       //точка на расстоянии k по обоим направлениям по касательной от точки cKnot
                                       PointsTang[0] = new HPoint(X[cKnot] - lx * k, Y[cKnot] - ly * k);
                                       PointsTang[1] = new HPoint(X[cKnot] + lx * k, Y[cKnot] + ly * k);
                                       //
                                       for (int j = 0; j < 2; j++)
                                       {
                                           int Triangle = mesh.GetTriangle(PointsTang[j].x, PointsTang[j].y);
                                           //если область вогнутая, то хотя бы одна точка вдоль касательной бдует лежать вне области
                                           // здесь в таком случае влияние производных U и V  вдоль касательной не учитываем
                                           if (Triangle == -1)
                                           {
                                               U34[0] = 0; U34[1] = 0;
                                               V34[0] = 0; V34[1] = 0;
                                               break;
                                           }
                                           //
                                           double s05 = 1.0f / 2.0f / wrapper.Sk[Triangle];
                                           uint[] Knots = mesh.AreaElems[Triangle];
                                           double x1 = X[Knots[0]]; double x2 = X[Knots[1]]; double x3 = X[Knots[2]];
                                           double y1 = Y[Knots[0]]; double y2 = Y[Knots[1]]; double y3 = Y[Knots[2]];
                                           double L1 = s05 * ((x2 * y3 - x3 * y2) + PointsNorm[j].x * (y2 - y3) + PointsNorm[j].y * (x3 - x2));
                                           double L2 = s05 * ((x3 * y1 - x1 * y3) + PointsNorm[j].x * (y3 - y1) + PointsNorm[j].y * (x1 - x3));
                                           double L3 = s05 * ((x1 * y2 - x2 * y1) + PointsNorm[j].x * (y1 - y2) + PointsNorm[j].y * (x2 - x1));
                                           //
                                           U34[j] = L1 * U[Knots[0]] + L2 * U[Knots[1]] + L3 * U[Knots[2]];
                                           V34[j] = L1 * V[Knots[0]] + L2 * V[Knots[1]] + L3 * V[Knots[2]];

                                       }
                                       Ldu_dx = (U34[1] - U34[0]) / k / 2.0f;
                                       Ldv_dx = (V34[1] - V34[0]) / k / 2.0f;

                                   }
                                   //
                                   // давление в узле
                                   P_c = P[cKnot];
                                   double mu_c = nuT[cKnot] * rho_w;
                                   // составляющие тензора напряжений
                                   double Tx1 = -P_c + 2.0f * mu_c * Ldu_dx;
                                   double Tx2 = mu_c * (Ldu_dy + Ldv_dx);
                                   double Ty1 = mu_c * (Ldu_dy + Ldv_dx);
                                   double Ty2 = -P_c + 2.0f * mu_c * Ldv_dy;
                                   // компоненты вектора придонного напряжения
                                   double LsigX = Tx1 * nx + Tx2 * ny;
                                   double LsigY = Ty1 * nx + Ty2 * ny;
                                   // проекция вектора придонного напряжения на касательный вектор
                                   BTau[i] = (LsigX * lx + LsigY * ly);
                                   arg[i] = X[cKnot];

                                   //1/(x0-x1)*y0+1/(x0-x2)*y0+1/(x1-x0)*(x0-x2)/(x1-x2)*y1+1/(x2-x0)*(x0-x1)/(x2-x1)*y2

                               }
                           });
                    //
                }
            }
            catch (Exception e)
            {
                err = e.Message + "ShearStressesCalculation";
            }

        }

        void ShearStress(int[] BottomKnots, int[] CBottom, int[] BTriangles, out double[] BTau, out double[] BTauC, out double[] arg)
        {
            BTauC = new double[mesh.BottomKnots.Length];
            BTau = new double[mesh.BottomKnots.Length];
            arg = new double[mesh.BottomKnots.Length - 1];
            try
            {
                int count = mesh.BottomKnots.Length;
                //
                double[] Tx1 = new double[wrapper.BTriangles.Length];
                double[] Tx2 = new double[wrapper.BTriangles.Length];
                double[] Ty1 = new double[wrapper.BTriangles.Length];
                double[] Ty2 = new double[wrapper.BTriangles.Length];
                //
                double[] tau_mid = new double[wrapper.BTriangles.Length];
                //double e1x = 1, e1y = 0, e2x = 0, e2y = 1;//проекции
                //
                OrderablePartitioner<Tuple<int, int>> OrdPartitioner_Tau = Partitioner.Create(0, wrapper.BTriangles.Length);
                Parallel.ForEach(OrdPartitioner_Tau,
                        (range, loopState) =>
                        {
                            for (int i = range.Item1; i < range.Item2; i++)
                            //for (int i = 0; i < mesh.BTriangles.Length; i++)
                            {

                                double[] xL = new double[3];
                                double[] yL = new double[3];
                                //
                                uint LcurV = wrapper.BTriangles[i];
                                uint[] Knots = mesh.AreaElems[LcurV];
                                //
                                uint Lnum1 = Knots[0];
                                uint Lnum2 = Knots[1];
                                uint Lnum3 = Knots[2];
                                // получаем координаты узлов элемента
                                xL[0] = X[Lnum1]; xL[1] = X[Lnum2]; xL[2] = X[Lnum3];
                                yL[0] = Y[Lnum1]; yL[1] = Y[Lnum2]; yL[2] = Y[Lnum3];
                                // нахождение площади треугольника
                                double LS = wrapper.Sk[LcurV];
                                // скорости в вершинах треугольника
                                double LU1 = U[Lnum1];
                                double LU2 = U[Lnum2];
                                double LU3 = U[Lnum3];
                                //
                                double LV1 = V[Lnum1];
                                double LV2 = V[Lnum2];
                                double LV3 = V[Lnum3];
                                // производные в центре треугольника
                                double Ldu_dx = 1.0f / 2.0f / LS * (LU1 * (yL[1] - yL[2]) + LU2 * (yL[2] - yL[0]) + LU3 * (yL[0] - yL[1]));
                                double Ldu_dy = 1.0f / 2.0f / LS * (LU1 * (xL[2] - xL[1]) + LU2 * (xL[0] - xL[2]) + LU3 * (xL[1] - xL[0]));
                                double Ldv_dx = 1.0f / 2.0f / LS * (LV1 * (yL[1] - yL[2]) + LV2 * (yL[2] - yL[0]) + LV3 * (yL[0] - yL[1]));
                                double Ldv_dy = 1.0f / 2.0f / LS * (LV1 * (xL[2] - xL[1]) + LV2 * (xL[0] - xL[2]) + LV3 * (xL[1] - xL[0]));
                                // давление в центре треугольника
                                double LP_mid = (P[Lnum1] + P[Lnum2] + P[Lnum3]) / 3.0f;
                                double Mu_mid = (nuT[Lnum1] + nuT[Lnum2] + nuT[Lnum3]) / 3.0f * rho_w;
                                // составляющие тензора напряжений
                                Tx1[i] = -LP_mid + 2.0f * Mu_mid * Ldu_dx;
                                Tx2[i] = Mu_mid * (Ldu_dy + Ldv_dx);
                                Ty1[i] = Mu_mid * (Ldu_dy + Ldv_dx);
                                Ty2[i] = -LP_mid + 2.0f * Mu_mid * Ldv_dy;
                            }
                        });
                // Вычисление tau в узлах сетки по сглаженной методике вычисления 
                //double[] Tau_all = Aproximate(tau_mid, mesh.CBottom, mesh.BTriangles);

                //вычисление тензора напряжений по сглаженной методике
                double[] Tx1_all = Aproximate(Tx1, wrapper.CBottom, wrapper.BTriangles);
                double[] Tx2_all = Aproximate(Tx2, wrapper.CBottom, wrapper.BTriangles);
                double[] Ty1_all = Aproximate(Ty1, wrapper.CBottom, wrapper.BTriangles);
                double[] Ty2_all = Aproximate(Ty2, wrapper.CBottom, wrapper.BTriangles);
                //подготовка приграничных значений tau между граничными точками и координат для сплайна

                int cKnot = mesh.BottomKnots[0];
                double prevTx1 = 0, prevTx2 = 0, prevTy1 = 0, prevTy2 = 0;
                //
                for (int i = 0; i < mesh.BottomKnots.Length; i++)
                {
                    cKnot = mesh.BottomKnots[i];
                    for (int j = 0; j < wrapper.CBottom.Length; j++)
                    {
                        if (cKnot == wrapper.CBottom[j])
                        {
                            if (i != 0)
                            {
                                int prevKnot = mesh.BottomKnots[i - 1];
                                double delx = (X[cKnot] - X[prevKnot]);
                                double dely = (Y[cKnot] - Y[prevKnot]);
                                double ss = (double)(Math.Sqrt(delx * delx + dely * dely));
                                // компоненты касательной между граничными точками
                                double lx = 1 / ss * delx;
                                double ly = 1 / ss * dely;
                                double nx = -ly;
                                double ny = lx;
                                //
                                double LsigX = (prevTx1 + Tx1_all[j]) / 2.0f * nx + (prevTx2 + Tx2_all[j]) / 2.0f * ny;//умножили (Tx1;Tx2) на (0;-1) - нормаль к поверхности
                                double LsigY = (prevTy1 + Ty1_all[j]) / 2.0f * nx + (prevTy2 + Ty2_all[j]) / 2.0f * ny;//умножили (Ty1;Ty2) на (0;-1) - нормаль к поверхности
                                //в полуцелых узлах
                                BTau[i - 1] = (LsigX * lx + LsigY * ly);
                                arg[i - 1] = X[prevKnot] + delx / 2.0f;
                                //
                                LsigX = Tx1_all[j] * nx + Tx2_all[j] * ny;//умножили (Tx1;Tx2) на (0;-1) - нормаль к поверхности
                                LsigY = Ty1_all[j] * nx + Ty2_all[j] * ny;//умножили (Ty1;Ty2) на (0;-1) - нормаль к поверхности
                                BTauC[i] = (LsigX * lx + LsigY * ly);

                            }
                            else
                            {
                                int nextKnot = mesh.BottomKnots[i + 1];
                                double delx = (X[nextKnot] - X[cKnot]);
                                double dely = (Y[nextKnot] - Y[cKnot]);
                                double ss = (double)(Math.Sqrt(delx * delx + dely * dely));
                                // компоненты касательной между граничными точками
                                double lx = 1 / ss * delx;
                                double ly = 1 / ss * dely;
                                double nx = -ly;
                                double ny = lx;
                                //
                                double LsigX = Tx1_all[j] * nx + Tx2_all[j] * ny;//умножили (Tx1;Tx2) на (0;-1) - нормаль к поверхности
                                double LsigY = Ty1_all[j] * nx + Ty2_all[j] * ny;//умножили (Ty1;Ty2) на (0;-1) - нормаль к поверхности
                                BTauC[i] = (LsigX * lx + LsigY * ly);
                            }
                            //
                            prevTx1 = Tx1_all[j];
                            prevTx2 = Tx2_all[j];
                            prevTy1 = Ty1_all[j];
                            prevTy2 = Ty2_all[j];
                            break;
                        }
                    }
                }
                BTau[count - 1] = BTau[count - 2];
            }
            catch (Exception e)
            {
                err = e.Message + "ShearStressesCalculation";
            }
        }

        #endregion

        #region Граничные условия Донга - сейчас отключены
        private void DongBC_UE(double delta = 0.05)
        {
            u_cont = new double[mesh.CountRight];
            u_kinet = new double[mesh.CountRight];
            u_press = new double[mesh.CountRight];
            //
            int knot = mesh.RightKnots[0];
            double du = 0, dv = 0;
            double dx = (X[knot] - X[knot - mesh.CountRight]);
            double dy = (Y[knot + 2] - Y[knot + 1]);
            for (int i = 1; i < mesh.CountRight - 1; i++)
            {
                knot = mesh.RightKnots[i];
                du = (U[knot] - U[knot - mesh.CountRight]) / dx;
                if (V[knot] < 0)
                    dv = (V[knot] - V[knot - 1]) / dy;
                else
                    dv = (V[knot + 1] - V[knot]) / dy;
                u_press[i] = 1.0 / mu * P[knot];
                u_kinet[i] = rho_w / 4.0 / mu * (U[knot] * U[knot] + V[knot] * V[knot]) * (1 - Math.Tanh(+U[knot] / Ucp / delta));
                u_cont[i] = du + dv;
                U[knot] = U[knot - mesh.CountRight] + dx * (u_press[i] + u_kinet[i] - u_cont[i]);
                V[knot] = V[knot - mesh.CountRight];
            }
        }

        private void DongBC_PE(double delta = 0.05)
        {
            p_conv = new double[mesh.CountRight];
            p_kinet = new double[mesh.CountRight];
            //
            int knot = 0;
            for (int i = 0; i < mesh.CountRight; i++)
            {
                RBC[i] = 0;
                knot = mesh.RightKnots[i];
                p_kinet[i] = 0.25 * rho_w * (U[knot] * U[knot] + V[knot] * V[knot]) * (1 - Math.Tanh(U[knot] / Ucp / delta));
                p_conv[i] = mu * (U[knot] - U[knot - mesh.CountRight]) / (X[knot] - X[knot - mesh.CountRight]);
                //
                RBC[i] = p_conv[i] - p_kinet[i];
            }
            RBC[0] = RBC[1];
            RBC[mesh.CountRight - 1] = RBC[mesh.CountRight - 2];
            //
            p_kinet[0] = p_kinet[1];
            p_conv[0] = p_conv[1];
            p_kinet[mesh.CountRight - 1] = p_kinet[mesh.CountRight - 2];
            p_conv[mesh.CountRight - 1] = p_conv[mesh.CountRight - 2];
        }
        //
        //----  ///
        private void DongBC_U(double delta = 0.05)
        {
            int knot = mesh.RightKnots[0];
            double du = 0, dv = 0;
            double dx = (X[knot] - X[knot - mesh.CountRight]);
            double dy = (Y[knot + 2] - Y[knot + 1]);
            for (int i = 1; i < mesh.CountRight - 1; i++)
            {
                knot = mesh.RightKnots[i];
                du = (U[knot] - U[knot - mesh.CountRight]) / dx;
                if (V[knot] < 0)
                    dv = (V[knot] - V[knot - 1]) / dy;
                else
                    dv = (V[knot + 1] - V[knot]) / dy;
                U[knot] = U[knot - mesh.CountRight] + dx / mu * (P[knot] + rho_w / 4.0 * (U[knot] * U[knot] + V[knot] * V[knot]) * (1 - Math.Tanh(+U[knot] / Ucp / delta))) - dx * (du + dv);
                V[knot] = V[knot - mesh.CountRight];
            }
        }

        private void DongBC_P(double delta = 0.05)
        {
            int knot = 0;
            for (int i = 0; i < mesh.CountRight; i++)
            {
                RBC[i] = 0;
                knot = mesh.RightKnots[i];
                RBC[i] = mu * (U[knot] - U[knot - mesh.CountRight]) / (X[knot] - X[knot - mesh.CountRight]) - 0.25 * rho_w * (U[knot] * U[knot] + V[knot] * V[knot]) * (1 - Math.Tanh(U[knot] / Ucp / delta));
            }
            RBC[0] = RBC[1];
            RBC[mesh.CountRight - 1] = RBC[mesh.CountRight - 2];
        }
        //
        public double[] GetKBed(int pp = 1)
        {
            int n1 = Convert.ToInt32(1.0 / mesh.CoordsX[mesh.CountLeft]);
            int ch = 0;
            double pw0 = 0;
            double dy = mesh.CoordsY[1] - mesh.CoordsY[0];
            for (int i = n1; i < mesh.CountBottom; i++)
            {
                if (Math.Abs(mesh.CoordsY[i * mesh.CountLeft]) < dy)
                    ch++;
                if (ch == 5)
                {
                    pw0 = P[i * mesh.CountLeft + pp];
                    break;
                }
            }
            //по Segunda
            double[] pBed = new double[mesh.CountBottom];
            int knot = 0;
            for (int i = 0; i < mesh.CountBottom; i++)
            {
                knot = mesh.BottomKnots[i] + pp;
                pBed[i] = P[knot] - pw0;
            }
            //
            return pBed;
            //
            //double dP = (P[0] - P[mesh.BottomKnots[mesh.CountBottom - 1]]) / (mesh.CountBottom - 1);
            //for (int i = 0; i < mesh.CountBottom; i++)
            //{
            //    knot = mesh.BottomKnots[i] + pp;
            //    pBed[i] = P[knot] - dP * (mesh.CountBottom - i);
            //}
            //return pBed;
            //
            //double[] kBed = new double[mesh.CountBottom];
            //int knot = 0;
            //for (int i = 0; i < mesh.CountBottom; i++)
            //{
            //    knot = mesh.BottomKnots[i]+pp;
            //    kBed[i] = K[knot];
            //}
            //return kBed;
        }

        #endregion 
        
        #region Расчет в OpenCL
        public int All_Calc_Parallel()
        {
            double MaxError = 0; // максимальная порешность по давлению
            int iteration = 0;
            //
            int knot = 0;
            double[] Result = null;
            double[] BV2 = new double[mesh.CountRight];
            Pk = new double[CountKnots];
            //
            //
            Object lockThis = new Object();
            //
            //bool Gauss = false;
            //string ss = Alg.GetType().Name;
            //if (ss == "AlgorythmGauss")
            //    Gauss = true;
            //
            if (Params.typePU == TypePU.Gpu_OpenCL)
            {
                //
                int width = 4;
                //
                int CVLength = wrapper.CV2.Length + wrapper.CV_WallKnots.Length;
                int[] Num = new int[CVLength + 1]; // хранит как плотной упаковке номера узлов КО и его соседних узлов
                int ch = 0;
                for (int i = 0; i < wrapper.CV2.Length; i++)
                {
                    Num[i] = ch;
                    ch += wrapper.CV2[i].Length;
                }
                //
                for (int i = 0; i < wrapper.CV_WallKnots.Length; i++)
                {
                    Num[wrapper.CV2.Length + i] = ch;
                    ch += wrapper.CV_WallKnots[i].Length;
                }
                Num[CVLength] = ch;
                // перевод массивов во float и в одномерный вид
                int[] OCV = new int[ch];
                int[] OP1 = new int[ch];
                float[] OSS = new float[ch];
                float[] OLx10 = new float[ch];
                float[] OLx32 = new float[ch];
                float[] OLy01 = new float[ch];
                float[] OLy23 = new float[ch];
                float[] ONx = new float[ch];
                float[] ONy = new float[ch];
                float[] OAlpha = new float[ch];
                float[] OLk = new float[ch];
                float[] OS0 = new float[CountKnots];
                //
                ch = 0;
                for (int i = 0; i < wrapper.CV2.Length; i++)
                {
                    int jj = wrapper.CV2[i].Length;
                    int p0 = wrapper.CV2[i][0];
                    //
                    for (int j = 0; j < jj; j++)
                    {
                        OCV[ch] = wrapper.CV2[i][j];
                        OP1[ch] = wrapper.P1[p0][j];
                        OSS[ch] = (float)wrapper.S[p0][j];
                        OLx10[ch] = (float)wrapper.Lx10[p0][j];
                        OLx32[ch] = (float)wrapper.Lx32[p0][j];
                        OLy01[ch] = (float)wrapper.Ly01[p0][j];
                        OLy23[ch] = (float)wrapper.Ly23[p0][j];
                        ONx[ch] = (float)wrapper.Nx[p0][j];
                        ONy[ch] = (float)wrapper.Ny[p0][j];
                        OAlpha[ch] = (float)wrapper.Alpha[p0][j];
                        OLk[ch] = (float)wrapper.Lk[p0][j];
                        ch++;
                    }
                    //
                    OS0[p0] = (float)wrapper.S0[p0];
                }
                //
                float[] OCV_Tau = new float[wrapper.CV_WallKnots.Length];
                float[] OCV_WallKnotsDistance = new float[wrapper.CV_WallKnots.Length];
                //
                for (int i = 0; i < wrapper.CV_WallKnots.Length; i++)
                {
                    int jj = wrapper.CV_WallKnots[i].Length;
                    int p0 = wrapper.CV_WallKnots[i][0];
                    //
                    for (int j = 0; j < jj; j++)
                    {
                        OCV[ch] = wrapper.CV_WallKnots[i][j];
                        OP1[ch] = wrapper.P1[p0][j];
                        OSS[ch] = (float)wrapper.S[p0][j];
                        OLx10[ch] = (float)wrapper.Lx10[p0][j];
                        OLx32[ch] = (float)wrapper.Lx32[p0][j];
                        OLy01[ch] = (float)wrapper.Ly01[p0][j];
                        OLy23[ch] = (float)wrapper.Ly23[p0][j];
                        ONx[ch] = (float)wrapper.Nx[p0][j];
                        ONy[ch] = (float)wrapper.Ny[p0][j];
                        OAlpha[ch] = (float)wrapper.Alpha[p0][j];
                        OLk[ch] = (float)wrapper.Lk[p0][j];
                        ch++;
                    }
                    //
                    OS0[p0] = (float)wrapper.S0[p0];
                    //
                    OCV_Tau[i] = (float)CV_WallTau[i];
                    OCV_WallKnotsDistance[i] = (float)wrapper.CV_WallKnotsDistance[i];
                }
                //
                ch = 0;
                int[] OAreaElems = new int[mesh.CountElements * 3];
                for (int i = 0; i < mesh.AreaElems.Length; i++)
                {
                    uint[] Knots = mesh.AreaElems[i];
                    OAreaElems[ch++] = (int)Knots[0];
                    OAreaElems[ch++] = (int)Knots[1];
                    OAreaElems[ch++] = (int)Knots[2];
                }
                //
                float[] OU = new float[CountKnots];
                float[] OV = new float[CountKnots];
                float[] OP = new float[CountKnots];
                float[] OS = new float[CountKnots];
                //
                float[] OK = new float[CountKnots];
                float[] OE = new float[CountKnots];
                float[] OnuT = new float[CountKnots];
                float[] OPk = new float[CountKnots];
                //
                OrderablePartitioner<Tuple<int, int>> rangePartitioner3 = Partitioner.Create(0, CountKnots);
                Parallel.ForEach(rangePartitioner3,
                      (range, loopState) =>
                      {
                          for (int i = range.Item1; i < range.Item2; i++)
                          {
                              OU[i] = (float)U[i];
                              OV[i] = (float)V[i];
                              OP[i] = (float)P[i];
                              OS[i] = (float)S[i];
                              //
                              OK[i] = (float)K[i];
                              OE[i] = (float)E[i];
                              OnuT[i] = (float)(C_m * K[i] * K[i] / (E[i] + 1e-26));
                          }
                      });
                //
                #region Расчет на OpenCL
                if (wrapper.CVolumes.Length / width > 2147483647)
                    width *= 2;
                //
                //д.б. < 3 999 ГБайта
                double glSize = sizeof(int) * OCV.Length + sizeof(int) * Num.Length + sizeof(float) * OLx10.Length + sizeof(float) * OLx32.Length + sizeof(float) * OLy01.Length +
                    sizeof(float) * OLy23.Length + sizeof(float) * OSS.Length + sizeof(int) * OP1.Length + sizeof(int) * OAreaElems.Length + sizeof(float) * ONx.Length
                    + sizeof(float) * ONy.Length + sizeof(float) * OAlpha.Length + sizeof(float) * OLk.Length + sizeof(float) * OS0.Length + sizeof(float) * OP.Length +
                    sizeof(float) * OU.Length + sizeof(float) * OV.Length + sizeof(float) * OS.Length + sizeof(float) * OK.Length + sizeof(float) * OE.Length + sizeof(float) * OnuT.Length;
                glSize = glSize / 8.0 / 1024.0 / 1024.0 / 1024.0;
                //д.б. < 64 КБайта
                double constSize = 8 * sizeof(float) + 2 * sizeof(int);
                constSize = constSize / 8.0 / 1024.0;
                //Выбор платформы расчета, создание контекста
                ComputeContextPropertyList properties = new ComputeContextPropertyList(ComputePlatform.Platforms[0]);
                ComputeContext context = new ComputeContext(ComputeDeviceTypes.All, properties, null, IntPtr.Zero);
                //Инициализация OpenCl, выбор устройства
                ComputeCommandQueue commands = new ComputeCommandQueue(context, context.Devices[0], ComputeCommandQueueFlags.None);
                //Считывание текста программы из файла
                string s = AppDomain.CurrentDomain.BaseDirectory;
                StreamReader streamReader = new StreamReader("UVKE_OpenCL.cl");
                string clSource = streamReader.ReadToEnd();
                streamReader.Close();
                //Компиляция программы
                ComputeProgram program = new ComputeProgram(context, clSource);
                program.Build(context.Devices, "", null, IntPtr.Zero);
                //Создание ядра
                ComputeKernel kernelUV = program.CreateKernel("UV_CV2");
                ComputeKernel kernelKE = program.CreateKernel("KE_CV2");
                ////Создание буферов параметров
                //(const int CV2Length, const int CVWLength, const int width, const float dt, const float rho_w, const float nu_mol, const float tau, const float WSinJ, const float WCosJ,
                //global read_only int* OCV, global read_only int* Num, global read_only float* OLx10, global read_only float* OLx32, global read_only float* OLy01, global read_only float* OLy23, 
                //global read_only float* OSS, global read_only int* OP1, global read_only int* OAreaElems, global read_only float* ONx, global read_only float* ONy, global read_only float* OAlpha, 
                //global read_only float* OLk, global read_only float* OS0, global read_only float* CV_Tau, global read_only float* OP, global read_only float* OK, global read_only float* OE, 
                //global read_only float* OnuT, global float* OU, global float* OV, global float* OS)
                //---

                ////Выделение памяти на device(OpenCl) под переменные
                ComputeBuffer<int> Ocv = new ComputeBuffer<int>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, OCV);
                ComputeBuffer<int> Onum = new ComputeBuffer<int>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, Num);
                ComputeBuffer<float> Olx10 = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, OLx10);
                ComputeBuffer<float> Olx32 = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, OLx32);
                ComputeBuffer<float> Oly01 = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, OLy01);
                ComputeBuffer<float> Oly23 = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, OLy23);
                ComputeBuffer<float> Oss = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, OSS);
                ComputeBuffer<int> Op1 = new ComputeBuffer<int>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, OP1);
                ComputeBuffer<int> OareaElems = new ComputeBuffer<int>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, OAreaElems);
                ComputeBuffer<float> Onx = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, ONx);
                ComputeBuffer<float> Ony = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, ONy);
                ComputeBuffer<float> Oalpha = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, OAlpha);
                ComputeBuffer<float> Olk = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, OLk);
                ComputeBuffer<float> Os0 = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, OS0);
                ComputeBuffer<float> Ocv_tau = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, OCV_Tau);
                ComputeBuffer<float> Ocv_WallKnotsDistance = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, OCV_WallKnotsDistance);
                // с CopyHostPointer быстрее
                ComputeBuffer<float> Op = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, OP);
                ComputeBuffer<float> Ok = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, OK);
                ComputeBuffer<float> Oe = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, OE);
                ComputeBuffer<float> Onut = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, OnuT);
                ComputeBuffer<float> Ou = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, OU);
                ComputeBuffer<float> Ov = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, OV);
                ComputeBuffer<float> Os = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, OS);
                ComputeBuffer<float> Opk = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, OPk);
                // установка буферов в kernel
                kernelUV.SetValueArgument<int>(0, wrapper.CV2.Length);
                kernelUV.SetValueArgument<int>(1, wrapper.CV_WallKnots.Length);
                kernelUV.SetValueArgument<int>(2, width);
                kernelUV.SetValueArgument<float>(3, Convert.ToSingle(dt));
                kernelUV.SetValueArgument<float>(4, Convert.ToSingle(rho_w));
                kernelUV.SetValueArgument<float>(5, Convert.ToSingle(nu_mol));
                kernelUV.SetValueArgument<float>(6, Convert.ToSingle(Params.tau));
                kernelUV.SetValueArgument<float>(7, Convert.ToSingle(Ww * SinJ));
                kernelUV.SetValueArgument<float>(8, Convert.ToSingle(Ww * CosJ));
                kernelUV.SetMemoryArgument(9, Ocv);
                kernelUV.SetMemoryArgument(10, Onum);
                kernelUV.SetMemoryArgument(11, Olx10);
                kernelUV.SetMemoryArgument(12, Olx32);
                kernelUV.SetMemoryArgument(13, Oly01);
                kernelUV.SetMemoryArgument(14, Oly23);
                kernelUV.SetMemoryArgument(15, Oss);
                kernelUV.SetMemoryArgument(16, Op1);
                kernelUV.SetMemoryArgument(17, OareaElems);
                kernelUV.SetMemoryArgument(18, Onx);
                kernelUV.SetMemoryArgument(19, Ony);
                kernelUV.SetMemoryArgument(20, Oalpha);
                kernelUV.SetMemoryArgument(21, Olk);
                kernelUV.SetMemoryArgument(22, Os0);
                kernelUV.SetMemoryArgument(23, Ocv_tau);
                kernelUV.SetMemoryArgument(24, Op);
                kernelUV.SetMemoryArgument(25, Ok);
                kernelUV.SetMemoryArgument(26, Oe);
                kernelUV.SetMemoryArgument(27, Onut);
                kernelUV.SetMemoryArgument(28, Ou);
                kernelUV.SetMemoryArgument(29, Ov);
                kernelUV.SetMemoryArgument(30, Os);
                //
                kernelKE.SetValueArgument<int>(0, wrapper.CV2.Length);
                kernelKE.SetValueArgument<int>(1, wrapper.CV_WallKnots.Length);
                kernelKE.SetValueArgument<int>(2, width);
                kernelKE.SetValueArgument<float>(3, Convert.ToSingle(dt));
                kernelKE.SetValueArgument<float>(4, Convert.ToSingle(rho_w));
                kernelKE.SetValueArgument<float>(5, Convert.ToSingle(nu_mol));
                kernelKE.SetValueArgument<float>(6, Convert.ToSingle(Params.tau));
                kernelKE.SetValueArgument<float>(7, Convert.ToSingle(C_e1));
                kernelKE.SetValueArgument<float>(8, Convert.ToSingle(C_e2));
                kernelKE.SetValueArgument<float>(9, Convert.ToSingle(C_m));
                kernelKE.SetValueArgument<float>(10, Convert.ToSingle(sigma_e));
                kernelKE.SetValueArgument<float>(11, Convert.ToSingle(sigma_k));
                kernelKE.SetValueArgument<float>(12, Convert.ToSingle(kappa));
                kernelKE.SetValueArgument<float>(13, Convert.ToSingle(y_p_0));
                kernelKE.SetMemoryArgument(14, Ocv);
                kernelKE.SetMemoryArgument(15, Onum);
                kernelKE.SetMemoryArgument(16, Olx10);
                kernelKE.SetMemoryArgument(17, Olx32);
                kernelKE.SetMemoryArgument(18, Oly01);
                kernelKE.SetMemoryArgument(19, Oly23);
                kernelKE.SetMemoryArgument(20, Oss);
                kernelKE.SetMemoryArgument(21, Op1);
                kernelKE.SetMemoryArgument(22, OareaElems);
                kernelKE.SetMemoryArgument(23, Onx);
                kernelKE.SetMemoryArgument(24, Ony);
                kernelKE.SetMemoryArgument(25, Oalpha);
                kernelKE.SetMemoryArgument(26, Olk);
                kernelKE.SetMemoryArgument(27, Os0);
                kernelKE.SetMemoryArgument(28, Ocv_WallKnotsDistance);
                kernelKE.SetMemoryArgument(29, Op);
                kernelKE.SetMemoryArgument(30, Ou);
                kernelKE.SetMemoryArgument(31, Ov);
                kernelKE.SetMemoryArgument(32, Ok);
                kernelKE.SetMemoryArgument(33, Oe);
                kernelKE.SetMemoryArgument(34, Onut);
                kernelKE.SetMemoryArgument(35, Opk);
                //
                //массив, определяющий размерность расчета (количество потоков в определенном измерении)
                long[] globalSize = new long[1];
                if (wrapper.CVolumes.Length / width * width != wrapper.CVolumes.Length)
                    width = 2;
                globalSize[0] = (long)Math.Round((double)(wrapper.CV2.Length / width), MidpointRounding.AwayFromZero);// тк в Diff.cl width=4
                //
                for (iteration = beginIter; iteration < Params.iter; iteration++)
                {
                    //конвертация в double
                    for (int i = 0; i < U.Length; i++)
                    {
                        U[i] = Convert.ToDouble(OU[i]);
                        V[i] = Convert.ToDouble(OV[i]);
                    }
                    //------------------ Расчет давления
                    
                    PressureCalc(ref MaxError, ref Result, BV2, lockThis, iteration);
                    // перенос данных в буффер
                    for (int i = 0; i < U.Length; i++)
                    {
                        OP[i] = Convert.ToSingle(P[i]);
                    }
                    //
                    // вычисление напряжения по пристеночной функции
                    for (int i = 0; i < wrapper.CV_WallKnots.Length; i++)
                    {
                        knot = wrapper.CV_WallKnots[i][0];
                        //CV_WallTau[i] = WallFuncSharpPlus(knot, mesh.CV_WallKnotsDistance[i]);//шероховатая стенка по Луцкому, установка по Снегиреву
                        //CV_WallTau[i] = WallFuncSnegirev(knot, mesh.CV_WallKnotsDistance[i]);// гладкая стенка по Снегиреву
                        //CV_WallTau[i] = WallFuncPlus(knot, mesh.CV_WallKnotsDistance[i]);// гладкая стенка по Луцкому
                        OCV_Tau[i] = (float)WallFunc(knot, wrapper.CV_WallKnotsDistance[i]);// гладкая стенка по Волкову упрощ
                        //CV_WallTau[i] = WallFuncNewton(knot, mesh.CV_WallKnotsDistance[i]);// гладкая стенка по Волкову Ньютон
                    }

                    commands.WriteToBuffer(OP, Op, true, null);
                    commands.WriteToBuffer(OCV_Tau, Ocv_tau, true, null);

                    //
                    if (false)
                    {
                        #region Код ядра на CPU
                        int CV2Length = wrapper.CV2.Length;
                        int CVWLength = wrapper.CV_WallKnots.Length;
                        int p0, jj, Lv1, Lv2, Lp1, Lt1, Lt2, Lt3, Lz1, Lz2, Lz3;
                        float lx10, lx32, ly01, ly23, LS, LUc1, LVc1, LPc1, LSc1, LKc1, LUc2, LVc2, LPc2, LSc2, LKc2, Ls2, Ldudx, Ldudy, Ldvdx, Ldvdy, Ldpdx, Ldpdy, Ldsdx, Ldsdy, Ldkdx, Ldkdy, Lnx, Lny, Lalpha,
                            LUcr, LVcr, LPcr, LScr, LLk, LNucr, LKcr, LEcr, LpressU, LconvU, LdiffU, LregU1, LregU2, LregU, LpressV, LconvV, LdiffV, LregV1, LregV2, LregV, LconvS, LdiffS, LregS, wx, wy, wk;
                        int k, j;
                        for (int gl = 0; gl < globalSize[0]; gl++)
                        {
                            int c = gl * width; // NX*NY / width
                            float LsummU = 0, LsummV = 0, LsummS = 0; //потоки U, V скорости, S
                            int i;
                            for (k = 0; k < width; k++)
                            {
                                i = c + k;
                                //
                                LsummU = 0;//потоки U скорости
                                LsummV = 0;//потоки V скорости
                                LsummS = 0;//потоки s концентрации
                                //
                                p0 = OCV[Num[i]];
                                jj = Num[i + 1] - Num[i] - 1; //количество КО, связанных с данным узлом
                                for (j = Num[i]; j < Num[i + 1] - 1; j++)
                                {
                                    lx10 = OLx10[j]; lx32 = OLx32[j];
                                    ly01 = OLy01[j]; ly23 = OLy23[j];
                                    //площадь
                                    LS = OSS[j];
                                    //сосоедние элементы
                                    Lv1 = OCV[(j - Num[i] + 1) % jj + Num[i] + 1];
                                    Lv2 = OCV[j + 1];
                                    //вторая точка общей грани
                                    Lp1 = OP1[j];
                                    //находим значения функций в центрах масс 1ого и 2ого треугольника как средние значения по элементу
                                    Lt1 = OAreaElems[Lv1 * 3]; Lt2 = OAreaElems[Lv1 * 3 + 1]; Lt3 = OAreaElems[Lv1 * 3 + 2];
                                    LUc1 = (OU[Lt1] + OU[Lt2] + OU[Lt3]) / 3.0f;
                                    LVc1 = (OV[Lt1] + OV[Lt2] + OV[Lt3]) / 3.0f;
                                    LPc1 = (OP[Lt1] + OP[Lt2] + OP[Lt3]) / 3.0f;
                                    LSc1 = (OS[Lt1] + OS[Lt2] + OS[Lt3]) / 3.0f;
                                    LKc1 = (OK[Lt1] + OK[Lt2] + OK[Lt3]) / 3.0f;
                                    //
                                    Lz1 = OAreaElems[Lv2 * 3]; Lz2 = OAreaElems[Lv2 * 3 + 1]; Lz3 = OAreaElems[Lv2 * 3 + 2];
                                    LUc2 = (OU[Lz1] + OU[Lz2] + OU[Lz3]) / 3.0f;
                                    LVc2 = (OV[Lz1] + OV[Lz2] + OV[Lz3]) / 3.0f;
                                    LPc2 = (OP[Lz1] + OP[Lz2] + OP[Lz3]) / 3.0f;
                                    LSc2 = (OS[Lz1] + OS[Lz2] + OS[Lz3]) / 3.0f;
                                    LKc2 = (OK[Lz1] + OK[Lz2] + OK[Lz3]) / 3.0f;
                                    //значения производных в точке пересечения граней
                                    Ls2 = 2 * LS;
                                    Ldudx = ((LUc1 - LUc2) * ly01 + (OU[Lp1] - OU[p0]) * ly23) / Ls2;
                                    Ldudy = ((LUc1 - LUc2) * lx10 + (OU[Lp1] - OU[p0]) * lx32) / Ls2;
                                    Ldvdx = ((LVc1 - LVc2) * ly01 + (OV[Lp1] - OV[p0]) * ly23) / Ls2;
                                    Ldvdy = ((LVc1 - LVc2) * lx10 + (OV[Lp1] - OV[p0]) * lx32) / Ls2;
                                    Ldpdx = ((LPc1 - LPc2) * ly01 + (OP[Lp1] - OP[p0]) * ly23) / Ls2;
                                    Ldpdy = ((LPc1 - LPc2) * lx10 + (OP[Lp1] - OP[p0]) * lx32) / Ls2;
                                    Ldsdx = ((LSc1 - LSc2) * ly01 + (OS[Lp1] - OS[p0]) * ly23) / Ls2;
                                    Ldsdy = ((LSc1 - LSc2) * lx10 + (OS[Lp1] - OS[p0]) * lx32) / Ls2;
                                    Ldkdx = ((LKc1 - LKc2) * ly01 + (OK[Lp1] - OK[p0]) * ly23) / Ls2;
                                    Ldkdy = ((LKc1 - LKc2) * lx10 + (OK[Lp1] - OK[p0]) * lx32) / Ls2;
                                    //внешняя нормаль к грани КО (контуру КО)
                                    Lnx = ONx[j]; Lny = ONy[j];
                                    ////значение функций в точке пересечения грани КО и основной грани
                                    Lalpha = OAlpha[j];
                                    LUcr = Lalpha * OU[p0] + (1 - Lalpha) * OU[Lp1];
                                    LVcr = Lalpha * OV[p0] + (1 - Lalpha) * OV[Lp1];
                                    LPcr = Lalpha * OP[p0] + (1 - Lalpha) * OP[Lp1];
                                    LScr = Lalpha * OS[p0] + (1 - Lalpha) * OS[Lp1];
                                    LNucr = Lalpha * OnuT[p0] + (1 - Lalpha) * OnuT[Lp1] + (float)nu_mol;
                                    LKcr = Lalpha * OK[p0] + (1 - Lalpha) * OK[Lp1];
                                    LEcr = Lalpha * OE[p0] + (1 - Lalpha) * OE[Lp1];
                                    //
                                    wx = (float)(Params.tau * (LUcr * Ldudx + LVcr * Ldudy + 1.0f / rho_w * Ldpdx + 2.0f / 3.0f * Ldkdx));
                                    wy = (float)(Params.tau * (LUcr * Ldvdx + LVcr * Ldvdy + 1.0f / rho_w * Ldpdy + 2.0f / 3.0f * Ldkdy));
                                    wk = (float)Params.tau * (LUcr * Ldkdx + LVcr * Ldkdy - (LNucr * (2.0f * Ldudx * Ldudx + (Ldvdx + Ldudy) * (Ldvdx + Ldudy) + 2.0f * Ldvdy * Ldvdy)) - LEcr);
                                    //длина текущего фрагмента внешнего контура КО
                                    LLk = OLk[j];
                                    //
                                    //расчет потоков
                                    LpressU = -(float)(1.0f / rho_w * LPcr * Lnx - 2.0f / 3.0f * LKcr * Lnx);
                                    LconvU = -LUcr * LUcr * Lnx - (LUcr * LVcr) * Lny;
                                    LdiffU = (float)(nu_mol + LNucr) * (2.0f * Ldudx * Lnx - 2.0f / 3.0f * (Ldudx + Ldvdy) * Lnx + Ldudy * Lny + Ldvdx * Lny);
                                    LregU1 = 2.0f * LUcr * wx * Lnx + 2.0f / 3.0f * wk * Lnx;
                                    LregU2 = (LVcr * wx + LUcr * wy) * Lny;
                                    LregU = LregU1 + LregU2;
                                    LsummU += (LconvU + LdiffU + LregU + LpressU) * LLk;
                                    //                  
                                    LpressV = (float)(-1.0f / rho_w * LPcr * Lny - 2.0f / 3.0f * LKcr * Lny);
                                    LconvV = -(LUcr * LVcr) * Lnx - LVcr * LVcr * Lny;
                                    LdiffV = (float)(nu_mol + LNucr) * (2.0f * Ldvdy * Lny - 2.0f / 3.0f * (Ldudx + Ldvdy) * Lny + Ldvdx * Lnx + Ldudy * Lnx);
                                    LregV1 = 2.0f * LVcr * wy * Lny + 2.0f / 3.0f * wk * Lny;
                                    LregV2 = (LVcr * wx + LUcr * wy) * Lnx;
                                    LregV = LregV1 + LregV2;
                                    LsummV += (LconvV + LdiffV + LregV + LpressV) * LLk;
                                    //
                                    LconvS = -(float)(LScr * (LUcr + Ww * SinJ) * Lnx - LScr * (LVcr - Ww * CosJ) * Lny);
                                    LdiffS = LNucr * (Ldsdx * Lnx + Ldsdy * Lny);
                                    LregS = LScr * (wx * Lnx + wy * Lny);
                                    LsummS += (LconvS + LdiffS + LregS) * LLk;
                                }
                                //
                                OU[p0] = OU[p0] + (float)dt / OS0[p0] * LsummU;
                                OV[p0] = OV[p0] + (float)dt / OS0[p0] * LsummV;
                                OS[p0] = OS[p0] + (float)dt / OS0[p0] * LsummS;

                            }
                            if (c + width == CV2Length)
                            {
                                for (k = 0; k < CVWLength; k++)
                                {
                                    i = c + width + k;
                                    LsummU = 0;//потоки U скорости
                                    LsummV = 0;//потоки V скорости
                                    LsummS = 0;//потоки s концентрации
                                    //
                                    p0 = OCV[Num[i]];
                                    jj = Num[i + 1] - Num[i] - 1; //количество КО, связанных с данным узлом
                                    for (j = Num[i]; j < Num[i + 1] - 1; j++)
                                    {
                                        lx10 = OLx10[j]; lx32 = OLx32[j];
                                        ly01 = OLy01[j]; ly23 = OLy23[j];
                                        //площадь
                                        LS = OSS[j];
                                        //сосоедние элементы
                                        Lv1 = OCV[(j - Num[i] + 1) % jj + Num[i] + 1];
                                        Lv2 = OCV[j + 1];
                                        //вторая точка общей грани
                                        Lp1 = OP1[j];
                                        //находим значения функций в центрах масс 1ого и 2ого треугольника как средние значения по элементу
                                        Lt1 = OAreaElems[Lv1 * 3]; Lt2 = OAreaElems[Lv1 * 3 + 1]; Lt3 = OAreaElems[Lv1 * 3 + 2];
                                        LUc1 = (OU[Lt1] + OU[Lt2] + OU[Lt3]) / 3.0f;
                                        LVc1 = (OV[Lt1] + OV[Lt2] + OV[Lt3]) / 3.0f;
                                        LPc1 = (OP[Lt1] + OP[Lt2] + OP[Lt3]) / 3.0f;
                                        LSc1 = (OS[Lt1] + OS[Lt2] + OS[Lt3]) / 3.0f;
                                        LKc1 = (OK[Lt1] + OK[Lt2] + OK[Lt3]) / 3.0f;
                                        //
                                        Lz1 = OAreaElems[Lv2 * 3]; Lz2 = OAreaElems[Lv2 * 3 + 1]; Lz3 = OAreaElems[Lv2 * 3 + 2];
                                        LUc2 = (OU[Lz1] + OU[Lz2] + OU[Lz3]) / 3.0f;
                                        LVc2 = (OV[Lz1] + OV[Lz2] + OV[Lz3]) / 3.0f;
                                        LPc2 = (OP[Lz1] + OP[Lz2] + OP[Lz3]) / 3.0f;
                                        LSc2 = (OS[Lz1] + OS[Lz2] + OS[Lz3]) / 3.0f;
                                        LKc2 = (OK[Lz1] + OK[Lz2] + OK[Lz3]) / 3.0f;
                                        //значения производных в точке пересечения граней
                                        Ls2 = 2 * LS;
                                        Ldudx = ((LUc1 - LUc2) * ly01 + (OU[Lp1] - OU[p0]) * ly23) / Ls2;
                                        Ldudy = ((LUc1 - LUc2) * lx10 + (OU[Lp1] - OU[p0]) * lx32) / Ls2;
                                        Ldvdx = ((LVc1 - LVc2) * ly01 + (OV[Lp1] - OV[p0]) * ly23) / Ls2;
                                        Ldvdy = ((LVc1 - LVc2) * lx10 + (OV[Lp1] - OV[p0]) * lx32) / Ls2;
                                        Ldpdx = ((LPc1 - LPc2) * ly01 + (OP[Lp1] - OP[p0]) * ly23) / Ls2;
                                        Ldpdy = ((LPc1 - LPc2) * lx10 + (OP[Lp1] - OP[p0]) * lx32) / Ls2;
                                        Ldsdx = ((LSc1 - LSc2) * ly01 + (OS[Lp1] - OS[p0]) * ly23) / Ls2;
                                        Ldsdy = ((LSc1 - LSc2) * lx10 + (OS[Lp1] - OS[p0]) * lx32) / Ls2;
                                        Ldkdx = ((LKc1 - LKc2) * ly01 + (OK[Lp1] - OK[p0]) * ly23) / Ls2;
                                        Ldkdy = ((LKc1 - LKc2) * lx10 + (OK[Lp1] - OK[p0]) * lx32) / Ls2;
                                        //внешняя нормаль к грани КО (контуру КО)
                                        Lnx = ONx[j]; Lny = ONy[j];
                                        ////значение функций в точке пересечения грани КО и основной грани
                                        Lalpha = OAlpha[j];
                                        LUcr = Lalpha * OU[p0] + (1 - Lalpha) * OU[Lp1];
                                        LVcr = Lalpha * OV[p0] + (1 - Lalpha) * OV[Lp1];
                                        LPcr = Lalpha * OP[p0] + (1 - Lalpha) * OP[Lp1];
                                        LScr = Lalpha * OS[p0] + (1 - Lalpha) * OS[Lp1];
                                        LNucr = Lalpha * OnuT[p0] + (1 - Lalpha) * OnuT[Lp1] + (float)nu_mol;
                                        LKcr = Lalpha * OK[p0] + (1 - Lalpha) * OK[Lp1];
                                        LEcr = Lalpha * OE[p0] + (1 - Lalpha) * OE[Lp1];
                                        //
                                        wx = (float)(Params.tau * (LUcr * Ldudx + LVcr * Ldudy + 1.0f / rho_w * Ldpdx + 2.0f / 3.0f * Ldkdx));
                                        wy = (float)(Params.tau * (LUcr * Ldvdx + LVcr * Ldvdy + 1.0f / rho_w * Ldpdy + 2.0f / 3.0f * Ldkdy));
                                        wk = (float)Params.tau * (LUcr * Ldkdx + LVcr * Ldkdy - (LNucr * (2.0f * Ldudx * Ldudx + (Ldvdx + Ldudy) * (Ldvdx + Ldudy) + 2.0f * Ldvdy * Ldvdy)) - LEcr);
                                        //длина текущего фрагмента внешнего контура КО
                                        LLk = OLk[j];
                                        //
                                        //расчет потоков
                                        LpressU = (float)(-1.0f / rho_w * LPcr * Lnx - 2.0f / 3.0f * LKcr * Lnx);
                                        LconvU = (float)(-LUcr * LUcr * Lnx - (LUcr * LVcr) * Lny);
                                        LdiffU = (float)(OCV_Tau[k] / rho_w * Lny - nu_mol * 2.0 / 3.0 * (Ldudx + Ldvdy) * Lnx);
                                        LregU1 = 2.0f * LUcr * wx * Lnx + 2.0f / 3.0f * wk * Lnx;
                                        LregU2 = (LVcr * wx + LUcr * wy) * Lny;
                                        LregU = LregU1 + LregU2;
                                        LsummU += (LconvU + LdiffU + LregU + LpressU) * LLk;
                                        //                  
                                        LpressV = (float)(-1.0 / rho_w * LPcr * Lny - 2.0 / 3.0 * LKcr * Lny);
                                        LconvV = -(LUcr * LVcr) * Lnx - LVcr * LVcr * Lny;
                                        LdiffV = (float)(OCV_Tau[k] / rho_w * Lnx - nu_mol * 2.0 / 3.0 * (Ldudx + Ldvdy) * Lny);
                                        LregV1 = (float)(2.0 * LVcr * wy * Lny + 2.0 / 3.0 * wk * Lny);
                                        LregV2 = (LVcr * wx + LUcr * wy) * Lnx;
                                        LregV = LregV1 + LregV2;
                                        LsummV += (LconvV + LdiffV + LregV + LpressV) * LLk;
                                        //
                                        LconvS = (float)(-LScr * (LUcr + Ww * SinJ) * Lnx - LScr * (LVcr - Ww * CosJ) * Lny);
                                        LdiffS = LNucr * (Ldsdx * Lnx + Ldsdy * Lny);
                                        LregS = LScr * (wx * Lnx + wy * Lny);
                                        LsummS += (LconvS + LdiffS + LregS) * LLk;
                                    }
                                    //
                                    OU[p0] = OU[p0] + (float)dt / OS0[p0] * LsummU;
                                    OV[p0] = OV[p0] + (float)dt / OS0[p0] * LsummV;
                                    OS[p0] = OS[p0] + (float)dt / OS0[p0] * LsummS;
                                }
                            }
                        }
                        //
                        //ГУ справа - снос
                        for (int i = 0; i < mesh.CountRight; i++)
                        {
                            knot = mesh.RightKnots[i];
                            //
                            OU[knot] = OU[knot - mesh.CountRight];
                            OV[knot] = OV[knot - mesh.CountRight];
                            OS[knot] = OS[knot - mesh.CountRight];
                        }
                        //если задача со свободной поверхностью
                        if (Params.surf_flag == true)
                        {
                            //ГУ на верхней стенке 
                            for (int i = 1; i < mesh.CountTop; i++)
                            {
                                knot = mesh.TopKnots[i];
                                OU[knot] = OU[knot - 1];
                                OV[knot] = OV[knot - 1];
                                OS[knot] = OS[knot - 1];
                            }

                        }
                        //

                        float LsummK = 0, LsummE = 0, LrightK = 0, LrightE = 0; //потоки K, E и правые части
                        float LEc2, ldudx, ldudy, ldvdx, ldvdy, Ldedx, Ldedy, LconvK, LdiffK, LregK, LconvE, LdiffE, LEc1, LregE, we, y_p_plus;
                        for (int gl = 0; gl < globalSize[0]; gl++)
                        {
                            int i;
                            int c = gl * width; // NX*NY / width
                            for (k = 0; k < width; k++)
                            {
                                i = c + k;
                                //
                                LsummK = 0;//потоки K скорости
                                LsummE = 0;//потоки E скорости
                                ldudx = 0; ldudy = 0; ldvdx = 0; ldvdy = 0;
                                //
                                p0 = OCV[Num[i]];
                                jj = Num[i + 1] - Num[i] - 1; //количество КО, связанных с данным узлом
                                for (j = Num[i]; j < Num[i + 1] - 1; j++)
                                {
                                    lx10 = OLx10[j]; lx32 = OLx32[j];
                                    ly01 = OLy01[j]; ly23 = OLy23[j];
                                    //площадь
                                    LS = OSS[j];
                                    //сосоедние элементы
                                    Lv1 = OCV[(j - Num[i] + 1) % jj + Num[i] + 1];
                                    Lv2 = OCV[j + 1];
                                    //вторая точка общей грани
                                    Lp1 = OP1[j];
                                    //находим значения функций в центрах масс 1ого и 2ого треугольника как средние значения по элементу
                                    Lt1 = OAreaElems[Lv1 * 3]; Lt2 = OAreaElems[Lv1 * 3 + 1]; Lt3 = OAreaElems[Lv1 * 3 + 2];
                                    LUc1 = (OU[Lt1] + OU[Lt2] + OU[Lt3]) / 3.0f;
                                    LVc1 = (OV[Lt1] + OV[Lt2] + OV[Lt3]) / 3.0f;
                                    LPc1 = (OP[Lt1] + OP[Lt2] + OP[Lt3]) / 3.0f;
                                    LKc1 = (OK[Lt1] + OK[Lt2] + OK[Lt3]) / 3.0f;
                                    LEc1 = (OE[Lt1] + OE[Lt2] + OE[Lt3]) / 3.0f;
                                    //
                                    Lz1 = OAreaElems[Lv2 * 3]; Lz2 = OAreaElems[Lv2 * 3 + 1]; Lz3 = OAreaElems[Lv2 * 3 + 2];
                                    LUc2 = (OU[Lz1] + OU[Lz2] + OU[Lz3]) / 3.0f;
                                    LVc2 = (OV[Lz1] + OV[Lz2] + OV[Lz3]) / 3.0f;
                                    LPc2 = (OP[Lz1] + OP[Lz2] + OP[Lz3]) / 3.0f;
                                    LKc2 = (OK[Lz1] + OK[Lz2] + OK[Lz3]) / 3.0f;
                                    LEc2 = (OE[Lz1] + OE[Lz2] + OE[Lz3]) / 3.0f;
                                    //значения производных в точке пересечения граней
                                    Ls2 = 2 * LS;
                                    Ldudx = ((LUc1 - LUc2) * ly01 + (OU[Lp1] - OU[p0]) * ly23) / Ls2;
                                    Ldudy = ((LUc1 - LUc2) * lx10 + (OU[Lp1] - OU[p0]) * lx32) / Ls2;
                                    Ldvdx = ((LVc1 - LVc2) * ly01 + (OV[Lp1] - OV[p0]) * ly23) / Ls2;
                                    Ldvdy = ((LVc1 - LVc2) * lx10 + (OV[Lp1] - OV[p0]) * lx32) / Ls2;
                                    Ldpdx = ((LPc1 - LPc2) * ly01 + (OP[Lp1] - OP[p0]) * ly23) / Ls2;
                                    Ldpdy = ((LPc1 - LPc2) * lx10 + (OP[Lp1] - OP[p0]) * lx32) / Ls2;
                                    Ldkdx = ((LKc1 - LKc2) * ly01 + (OK[Lp1] - OK[p0]) * ly23) / Ls2;
                                    Ldkdy = ((LKc1 - LKc2) * lx10 + (OK[Lp1] - OK[p0]) * lx32) / Ls2;
                                    Ldedx = ((LEc1 - LEc2) * ly01 + (OE[Lp1] - OE[p0]) * ly23) / Ls2;
                                    Ldedy = ((LEc1 - LEc2) * lx10 + (OE[Lp1] - OE[p0]) * lx32) / Ls2;
                                    //внешняя нормаль к грани КО (контуру КО)
                                    Lnx = ONx[j]; Lny = ONy[j];
                                    ////значение функций в точке пересечения грани КО и основной грани
                                    Lalpha = OAlpha[j];
                                    LUcr = Lalpha * OU[p0] + (1 - Lalpha) * OU[Lp1];
                                    LVcr = Lalpha * OV[p0] + (1 - Lalpha) * OV[Lp1];
                                    LNucr = Lalpha * OnuT[p0] + (1 - Lalpha) * OnuT[Lp1] + (float)nu_mol;
                                    LKcr = Lalpha * OK[p0] + (1 - Lalpha) * OK[Lp1];
                                    LEcr = Lalpha * OE[p0] + (1 - Lalpha) * OE[Lp1];
                                    //
                                    wx = (float)(Params.tau * (LUcr * Ldudx + LVcr * Ldudy + 1.0 / rho_w * Ldpdx + 2.0 / 3.0 * Ldkdx));
                                    wy = (float)(Params.tau * (LUcr * Ldvdx + LVcr * Ldvdy + 1.0 / rho_w * Ldpdy + 2.0 / 3.0 * Ldkdy));
                                    wk = (float)(Params.tau * (LUcr * Ldkdx + LVcr * Ldkdy - (LNucr * (2 * Ldudx * Ldudx + (Ldvdx + Ldudy) * (Ldvdx + Ldudy) + 2 * Ldvdy * Ldvdy)) - LEcr));
                                    we = (float)(Params.tau * (LUcr * Ldedx + LVcr * Ldedy - (LEcr / LKcr * (C_e1 * LNucr * (2 * Ldudx * Ldudx + (Ldvdx + Ldudy) * (Ldvdx + Ldudy) + 2 * Ldvdy * Ldvdy)) - C_e2 * LEcr)));
                                    //длина текущего фрагмента внешнего контура КО
                                    LLk = OLk[j];
                                    //
                                    //расчет потоков
                                    LconvK = -LUcr * LKcr * Lnx - LVcr * LKcr * Lny;
                                    LdiffK = (float)(LNucr / sigma_k + nu_mol) * (Ldkdx * Lnx + Ldkdy * Lny);
                                    LregK = LKcr * wx * Lnx + LKcr * wy * Lny + LUcr * wk * Lnx + LVcr * wk * Lny;
                                    //
                                    LsummK += (LconvK + LdiffK + LregK) * LLk;
                                    //                  
                                    LconvE = -LUcr * LEcr * Lnx - LVcr * LEcr * Lny;
                                    LdiffE = (float)(LNucr / sigma_e + nu_mol) * (Ldedx * Lnx + Ldedy * Lny);
                                    LregE = LEcr * wx * Lnx + LEcr * wy * Lny + LUcr * we * Lnx + LVcr * we * Lny;
                                    // 
                                    LsummE += (LconvE + LdiffE + LregE) * LLk;
                                    //
                                    // компоненты производных для Pk
                                    ldudx += LUcr * Lnx * LLk;
                                    ldudy += LUcr * Lny * LLk;
                                    ldvdx += LVcr * Lnx * LLk;
                                    ldvdy += LVcr * Lny * LLk;
                                }
                                //
                                ldudx /= OS0[p0];
                                ldudy /= OS0[p0];
                                ldvdx /= OS0[p0];
                                ldvdy /= OS0[p0];
                                //
                                OPk[p0] = OnuT[p0] * (2 * ldudx * ldudx + (ldvdx + ldudy) * (ldvdx + ldudy) + 2 * ldvdy * ldvdy);
                                LrightK = (OPk[p0] - OE[p0]);
                                LrightE = (float)(OE[p0] / OK[p0] * (C_e1 * OPk[p0] - C_e2 * OE[p0]));
                                //
                                OK[p0] = (float)(OK[p0] + dt / OS0[p0] * LsummK + dt * LrightK);
                                OE[p0] = (float)(OE[p0] + dt / OS0[p0] * LsummE + dt * LrightE);
                                OnuT[p0] = (float)(C_m * OK[p0] * OK[p0] / (OE[p0] + 1e-26));
                                if (OnuT[p0] < nu_mol)
                                    OnuT[p0] = (float)nu_mol;


                            }
                            if (c + width == CV2Length)
                            {
                                for (k = 0; k < CVWLength; k++)
                                {
                                    i = c + width + k;
                                    LsummK = 0;//потоки K скорости
                                    LsummE = 0;//потоки E скорости
                                    ldudx = 0; ldudy = 0; ldvdx = 0; ldvdy = 0;
                                    //
                                    p0 = OCV[Num[i]];
                                    jj = Num[i + 1] - Num[i] - 1; //количество КО, связанных с данным узлом
                                    for (j = Num[i]; j < Num[i + 1] - 1; j++)
                                    {
                                        lx10 = OLx10[j]; lx32 = OLx32[j];
                                        ly01 = OLy01[j]; ly23 = OLy23[j];
                                        //площадь
                                        LS = OSS[j];
                                        //сосоедние элементы
                                        Lv1 = OCV[(j - Num[i] + 1) % jj + Num[i] + 1];
                                        Lv2 = OCV[j + 1];
                                        //вторая точка общей грани
                                        Lp1 = OP1[j];
                                        //находим значения функций в центрах масс 1ого и 2ого треугольника как средние значения по элементу
                                        Lt1 = OAreaElems[Lv1 * 3]; Lt2 = OAreaElems[Lv1 * 3 + 1]; Lt3 = OAreaElems[Lv1 * 3 + 2];
                                        LUc1 = (OU[Lt1] + OU[Lt2] + OU[Lt3]) / 3.0f;
                                        LVc1 = (OV[Lt1] + OV[Lt2] + OV[Lt3]) / 3.0f;
                                        LPc1 = (OP[Lt1] + OP[Lt2] + OP[Lt3]) / 3.0f;
                                        LKc1 = (OK[Lt1] + OK[Lt2] + OK[Lt3]) / 3.0f;
                                        LEc1 = (OE[Lt1] + OE[Lt2] + OE[Lt3]) / 3.0f;
                                        //
                                        Lz1 = OAreaElems[Lv2 * 3]; Lz2 = OAreaElems[Lv2 * 3 + 1]; Lz3 = OAreaElems[Lv2 * 3 + 2];
                                        LUc2 = (OU[Lz1] + OU[Lz2] + OU[Lz3]) / 3.0f;
                                        LVc2 = (OV[Lz1] + OV[Lz2] + OV[Lz3]) / 3.0f;
                                        LPc2 = (OP[Lz1] + OP[Lz2] + OP[Lz3]) / 3.0f;
                                        LKc2 = (OK[Lz1] + OK[Lz2] + OK[Lz3]) / 3.0f;
                                        LEc2 = (OE[Lz1] + OE[Lz2] + OE[Lz3]) / 3.0f;
                                        //значения производных в точке пересечения граней
                                        Ls2 = 2 * LS;
                                        Ldudx = ((LUc1 - LUc2) * ly01 + (OU[Lp1] - OU[p0]) * ly23) / Ls2;
                                        Ldudy = ((LUc1 - LUc2) * lx10 + (OU[Lp1] - OU[p0]) * lx32) / Ls2;
                                        Ldvdx = ((LVc1 - LVc2) * ly01 + (OV[Lp1] - OV[p0]) * ly23) / Ls2;
                                        Ldvdy = ((LVc1 - LVc2) * lx10 + (OV[Lp1] - OV[p0]) * lx32) / Ls2;
                                        Ldpdx = ((LPc1 - LPc2) * ly01 + (OP[Lp1] - OP[p0]) * ly23) / Ls2;
                                        Ldpdy = ((LPc1 - LPc2) * lx10 + (OP[Lp1] - OP[p0]) * lx32) / Ls2;
                                        Ldkdx = ((LKc1 - LKc2) * ly01 + (OK[Lp1] - OK[p0]) * ly23) / Ls2;
                                        Ldkdy = ((LKc1 - LKc2) * lx10 + (OK[Lp1] - OK[p0]) * lx32) / Ls2;
                                        //внешняя нормаль к грани КО (контуру КО)
                                        Lnx = ONx[j]; Lny = ONy[j];
                                        ////значение функций в точке пересечения грани КО и основной грани
                                        Lalpha = OAlpha[j];
                                        LUcr = Lalpha * OU[p0] + (1 - Lalpha) * OU[Lp1];
                                        LVcr = Lalpha * OV[p0] + (1 - Lalpha) * OV[Lp1];
                                        LNucr = Lalpha * OnuT[p0] + (1 - Lalpha) * OnuT[Lp1] + (float)nu_mol;
                                        LKcr = Lalpha * OK[p0] + (1 - Lalpha) * OK[Lp1];
                                        LEcr = Lalpha * OE[p0] + (1 - Lalpha) * OE[Lp1];
                                        //
                                        wx = (float)(Params.tau * (LUcr * Ldudx + LVcr * Ldudy + 1.0 / rho_w * Ldpdx + 2.0 / 3.0 * Ldkdx));
                                        wy = (float)(Params.tau * (LUcr * Ldvdx + LVcr * Ldvdy + 1.0 / rho_w * Ldpdy + 2.0 / 3.0 * Ldkdy));
                                        wk = (float)(Params.tau * (LUcr * Ldkdx + LVcr * Ldkdy - (LNucr * (2 * Ldudx * Ldudx + (Ldvdx + Ldudy) * (Ldvdx + Ldudy) + 2 * Ldvdy * Ldvdy)) - LEcr));
                                        //длина текущего фрагмента внешнего контура КО
                                        LLk = OLk[j];
                                        //
                                        //расчет потоков
                                        LconvK = -LUcr * LKcr * Lnx - LVcr * LKcr * Lny;
                                        LdiffK = (float)(LNucr / sigma_k + nu_mol) * (Ldkdx * Lnx + Ldkdy * Lny);
                                        LregK = LKcr * wx * Lnx + LKcr * wy * Lny + LUcr * wk * Lnx + LVcr * wk * Lny;
                                        //
                                        LsummK += (LconvK + LdiffK + LregK) * LLk;
                                    }
                                    //
                                    y_p_plus = (float)(cm14 * Math.Sqrt(OK[p0]) * wrapper.CV_WallKnotsDistance[k] / nu_mol);
                                    OPk[p0] = 0;
                                    if (y_p_plus > y_p_0)
                                    {
                                        OE[p0] = (float)(cm14 * cm14 * cm14 * OK[p0] * Math.Sqrt(OK[p0]) / kappa / wrapper.CV_WallKnotsDistance[k]);
                                        OPk[p0] = OE[p0];
                                    }
                                    else
                                        OE[p0] = (float)(2.0 * OK[p0] / wrapper.CV_WallKnotsDistance[k] / wrapper.CV_WallKnotsDistance[k] * nu_mol);
                                    //
                                    LrightK = (OPk[p0] - OE[p0]);
                                    //
                                    OK[p0] = OK[p0] + (float)(dt / OS0[p0] * LsummK + dt * LrightK);
                                    OnuT[p0] = (float)(C_m * OK[p0] * OK[p0] / (OE[p0] + 1e-26));
                                    if (OnuT[p0] < nu_mol)
                                        OnuT[p0] = (float)nu_mol;


                                }
                            }
                        }
                        //
                        //ГУ справа снос
                        for (int i = 0; i < mesh.CountRight; i++)
                        {
                            knot = mesh.RightKnots[i];
                            //
                            OK[knot] = OK[knot - mesh.CountRight];
                            OE[knot] = OE[knot - mesh.CountRight];
                        }
                        //
                        if (Params.surf_flag == true)
                        {
                            for (int i = 1; i < mesh.CountTop; i++)
                            {
                                knot = mesh.TopKnots[i];
                                OK[knot] = OK[knot - 1];
                                OE[knot] = OE[knot - 1];
                            }
                        }
                        //
                        for (int i = 0; i < U.Length; i++)
                        {
                            K[i] = Convert.ToDouble(OK[i]);
                            E[i] = Convert.ToDouble(OE[i]);
                            nuT[i] = Convert.ToDouble(OnuT[i]);
                        }
                        #endregion
                    }
                    else
                    {
                        #region OpenCL
                        //-------------------- Расчет UVS
                        //stopCalc.Start();
                        ////Вызов ядра
                        commands.Execute(kernelUV, null, globalSize, null, null);
                        //Ожидание окончания выполнения программы
                        commands.Finish();

                        //// чтение искомой функции из буфера kernel-а

                        commands.ReadFromBuffer(Ou, ref OU, true, null);
                        commands.ReadFromBuffer(Ov, ref OV, true, null);
                        commands.ReadFromBuffer(Os, ref OS, true, null);

                        //ГУ справа - снос
                        for (int i = 0; i < mesh.CountRight; i++)
                        {
                            knot = mesh.RightKnots[i];
                            //
                            OU[knot] = OU[knot - mesh.CountLeft];
                            OV[knot] = OV[knot - mesh.CountLeft];
                            OS[knot] = OS[knot - mesh.CountLeft];
                        }
                        //если задача со свободной поверхностью
                        if (Params.surf_flag == true)
                        {
                            //ГУ на верхней стенке 
                            for (int i = 1; i < mesh.CountTop; i++)
                            {
                                knot = mesh.TopKnots[i];
                                OU[knot] = OU[knot - 1];
                                OV[knot] = OV[knot - 1];
                                OS[knot] = OS[knot - 1];
                            }

                        }

                        commands.WriteToBuffer(OU, Ou, true, null);
                        commands.WriteToBuffer(OV, Ov, true, null);
                        commands.WriteToBuffer(OS, Os, true, null);

                        //--------------------- Расчет K-E

                        //Вызов ядра
                        commands.Execute(kernelKE, null, globalSize, null, null);
                        //Ожидание окончания выполнения программы
                        commands.Finish();

                        //

                        commands.ReadFromBuffer(Ok, ref OK, true, null);
                        commands.ReadFromBuffer(Oe, ref OE, true, null);
                        commands.ReadFromBuffer(Onut, ref OnuT, true, null);

                        //
                        //ГУ справа снос
                        for (int i = 0; i < mesh.CountRight; i++)
                        {
                            knot = mesh.RightKnots[i];
                            //
                            OK[knot] = OK[knot - mesh.CountRight];
                            OE[knot] = OE[knot - mesh.CountRight];
                        }
                        //
                        if (Params.surf_flag == true)
                        {
                            for (int i = 1; i < mesh.CountTop; i++)
                            {
                                knot = mesh.TopKnots[i];
                                OK[knot] = OK[knot - 1];
                                OE[knot] = OE[knot - 1];
                            }
                        }
                        //
                        for (int i = 0; i < U.Length; i++)
                        {
                            K[i] = Convert.ToDouble(OK[i]);
                            E[i] = Convert.ToDouble(OE[i]);
                            nuT[i] = Convert.ToDouble(OnuT[i]);
                        }
                        //
                        commands.WriteToBuffer(OK, Ok, true, null);
                        commands.WriteToBuffer(OE, Oe, true, null);
                        #endregion
                    }
                    //
                    //выход из цикла если ошибка меньше или равно errP
                    if (MaxError <= Params.errP)
                    {
                        Iter = iteration + 1;
                        //break;
                    }
                    //
                    Iter = iteration + 1;
                }
                if (true)
                {
                    commands.ReadFromBuffer(Ou, ref OU, true, null);
                    commands.ReadFromBuffer(Ov, ref OV, true, null);
                    commands.ReadFromBuffer(Os, ref OS, true, null);
                    commands.ReadFromBuffer(Ok, ref OK, true, null);
                    commands.ReadFromBuffer(Oe, ref OE, true, null);
                    commands.ReadFromBuffer(Onut, ref OnuT, true, null);
                    commands.ReadFromBuffer(Opk, ref OPk, true, null);
                }
                //конвертация в double
                for (int i = 0; i < U.Length; i++)
                {
                    U[i] = Convert.ToDouble(OU[i]);
                    if (double.IsNaN(U[i]))
                    {
                        err += " Расчет OpenCL привел к бесконечному значению U";
                        break;
                    }
                    V[i] = Convert.ToDouble(OV[i]);
                    S[i] = Convert.ToDouble(OS[i]);
                    K[i] = Convert.ToDouble(OK[i]);
                    E[i] = Convert.ToDouble(OE[i]);
                    nuT[i] = Convert.ToDouble(OnuT[i]);
                    Pk[i] = Convert.ToDouble(OPk[i]);
                }

                ////очищение памяти в порядке, обратном созданию
                Opk.Dispose();
                Os.Dispose();
                Ov.Dispose();
                Ou.Dispose();
                Onut.Dispose();
                Oe.Dispose();
                Ok.Dispose();
                Op.Dispose();
                Ocv_WallKnotsDistance.Dispose();
                Ocv_tau.Dispose();
                Os0.Dispose();
                Olk.Dispose();
                Oalpha.Dispose();
                Ony.Dispose();
                Onx.Dispose();
                OareaElems.Dispose();
                Op1.Dispose();
                Oss.Dispose();
                Oly23.Dispose();
                Oly01.Dispose();
                Olx32.Dispose();
                Olx10.Dispose();
                Onum.Dispose();
                Ocv.Dispose();
                //
                kernelKE.Dispose();
                kernelUV.Dispose();
                program.Dispose();
                commands.Dispose();
                context.Dispose();
                #endregion
            }
            //
            for (int i = 0; i < CountKnots; i++)
            {
                buffU[i] = U[i];
                buffV[i] = V[i];
                buffK[i] = K[i];
                buffE[i] = E[i];
                buffS[i] = S[i];
                buffNu[i] = nuT[i];
                #region Сохранение y_plus - отключено
                // y+ по Гришанину
                //double delta = 0.6 * d;
                //if (i < CountKnots - 10)
                //{
                //    double y1 = mesh.GetNormalDistanceBottom(i);
                //    //double y1 = Y[i] - Y[(i / mesh.CountLeft) * mesh.CountLeft];
                //    double f = U[i] * y1 / nu_mol;
                //    double y_pl = 0;
                //    if (f <= 782.4968) // ламинарный слой
                //        y_pl = Math.Sqrt(2 * f);
                //    else  // логарифмический слой
                //    {
                //        double y_i = 39.56;
                //        double F_i = 0, u_i = 0, y_i1 = 0;
                //        for (int h = 0; h < 7; h++)
                //        {
                //            F_i = 2.5 * y_i * (Math.Log(20.0855 * y_i / delta) - 1) - 683.559072;
                //            u_i = 2.5 * Math.Log(20.0855 * y_i / delta);
                //            y_i1 = y_i + (f - F_i) / u_i;
                //            if (Math.Abs(y_i - y_i1) < 0.000001)
                //            {
                //                y_i = y_i1;
                //                break;
                //            }
                //            y_i = y_i1;
                //        }
                //        y_pl = y_i1;
                //    }
                //    //
                //    y_plus[i] = y_pl;
                //}
                //------------------------
                // y+ по Волкову
                //double kp = K[i];
                //double Dy = Y[i] - Y[(i / mesh.CountLeft) * mesh.CountLeft];
                //double Du = U[i];
                //double Re = rho_w * cm14 * Math.Sqrt(Math.Abs(kp)) * Dy / mu;
                //// tau
                //double tauw = (rho_w * cm14 * Math.Sqrt(Math.Abs(kp)) * Du) / (1.0 / kappa * Math.Log(8.8 * Re));
                //y_plus[i] = Dy * Math.Sqrt(tauw / rho_w) / nu_mol;
                //-------------------------------------------------
                // по Луцкому с буферным слоем
                //if (i < CountKnots - 10)
                //{
                //    double y1 = mesh.GetNormalDistanceBottom(i);
                //    double f = U[i] * y1 / nu_mol;
                //    double y_pl = 0;
                //    if (f <= 2) // ламинарный слой
                //        y_pl = Math.Sqrt(2 * f);
                //    else if (f >= 696.68) // логарифмический слой
                //    {
                //        double y_i = 60;
                //        double F_i = 0, u_i = 0, y_i1 = 0;
                //        for (int h = 0; h < 7; h++)
                //        {
                //            F_i = 2.5 * y_i * (Math.Log(y_i / 0.13) - 1) - 73.50481;
                //            u_i = 2.5 * Math.Log(y_i / 0.13);
                //            y_i1 = y_i + (f - F_i) / u_i;
                //            if (Math.Abs(y_i - y_i1) < 0.000001)
                //            {
                //                y_i = y_i1;
                //                break;
                //            }
                //            y_i = y_i1;
                //        }
                //        y_pl = y_i1;
                //    }
                //    else // буферный слой
                //        y_pl = alglib.spline1dcalc(bufF, f);
                //    //
                //    y_plus[i] = y_pl;
                //}
                // --------------------------------------
                // по Снегиреву
                //double dy = Y[i] - Y[(i / mesh.CountLeft) * mesh.CountLeft];
                //if (dy > 0)
                //{
                //    double y_p_plus = cm14 * Math.Sqrt(K[i]) * dy / nu_mol;
                //    double u2_tau = 0;
                //    if (y_p_plus > y_p_0)
                //        u2_tau = cm14 * cm14 * K[i];
                //    else
                //        u2_tau = nu_mol * U[i] / (dy + 0.00000001);
                //    double tau_w = u2_tau * rho_w;
                //    y_plus[i] = dy * Math.Sqrt(tau_w / rho_w) / nu_mol;
                //}
                #endregion
            }
            //Iter = iter;
            //расход на входе и выходе из расчетной области
            beginIter = 0;
            Q_in = 0; Q_out = 0;
            for (int k = 0; k < mesh.CountLeft - 1; k++)
            {
                int a = mesh.LeftKnots[k];
                int b = mesh.LeftKnots[k + 1];
                Q_in += (U[a] + U[b]) * 0.5f * Math.Abs(Y[a] - Y[b]);
            }
            //
            for (int k = 1; k < mesh.CountRight - 1; k++)
            {
                int a = mesh.RightKnots[k];
                int b = mesh.RightKnots[k + 1];
                Q_out += (U[a] + U[b]) * 0.5f * Math.Abs(Y[a] - Y[b]);
            }
            //
            return Iter;
        }

        #endregion
    }
}
