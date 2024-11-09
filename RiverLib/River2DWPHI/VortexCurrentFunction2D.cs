////---------------------------------------------------------------------------
////                          ПРОЕКТ  "DISER"
////                  создано  :   9.03.2007 Потапов И.И.
////---------------------------------------------------------------------------
////                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
////            перенесено с правкой : 06.12.2020 Потапов И.И. 
////            создание родителя : 21.02.2022 Потапов И.И. 
////---------------------------------------------------------------------------
//namespace RiverLib
//{
//    using CommonLib;
//    using CommonLib.IO;
//    using MemLogLib;
//    using GeometryLib;
//    using MeshLib;
//    using System;
//    using System.IO;
//    /// <summary>
//    ///  ОО: Определение класса River2DWP - расчет полей скорости, функций вихря и тока 
//    ///  и напряжений в канале МКЭ на произвольной сетке
//    /// </summary>
//    [Serializable]
//    public class VortexCurrentFunction2D : ARiverTask 
//    {
//        #region Свойства параметров IPropertyTask
//        /// <summary>
//        /// Переопределяется в наследнике для конкретных свойств
//        /// </summary>
//        public override IPropertyTask PropertyTask { get=> pTask; set => new GidroParams(value); }
//        /// <summary>
//        /// Параметры задачи
//        /// </summary>
//        protected GidroParams pTask;

//        /// <summary>
//        /// свойств задачи
//        /// </summary>
//        /// <pTask name="p"></pTask>
//        public override object GetParams() => pTask;
//        /// <summary>
//        /// Установка свойств задачи
//        /// </summary>
//        /// <pTask name="p"></pTask>
//        public override void SetParams(object obj)=> pTask.SetParams(obj);
//        /// <summary>
//        /// Чтение параметров задачи из файла
//        /// </summary>
//        /// <pTask name="file"></pTask>
//        public override void LoadParams(string fileName) => pTask.LoadParams(fileName);
//        #endregion 

//        #region Свойства IRiver
//        /// <summary       
//        /// Тип задачи используется для выбора совместимых подзадач
//        /// </summary>
//        public override TypeTask typeTask { get => TypeTask.streamX1D; }
//        /// <summary>
//        /// версия дата последних изменений интерфейса задачи
//        /// </summary>
//        public override string VersionData() => "River2DWP 11.03.2022"; 
//        #endregion

//        #region Локальыне переменые задачи
//        /// <summary>
//        ///  Число Прандтля.
//        /// </summary>
//        //double Pr = 16.0;   // В ПАРАМЕТРЫ ЗАДАЧИ !!!!!!!!!!!!!!!!!!!!!!
//        //double Mu = 0.001;  // В ПАРАМЕТРЫ ЗАДАЧИ !!!!!!!!!!!!!!!!!!!!!!
//        //double Nu = 0.001;  // В ПАРАМЕТРЫ ЗАДАЧИ !!!!!!!!!!!!!!!!!!!!!!
//        //double Gr = 0.1;    // В ПАРАМЕТРЫ ЗАДАЧИ !!!!!!!!!!!!!!!!!!!!!!
//        /// <summary>
//        /// функция тока
//        /// </summary>
//        private double[] phi, phi_old;
//        /// <summary>
//        /// скорости
//        /// </summary>
//        private double[] vx, vy;
//        /// <summary>
//        /// Функция вихря
//        /// </summary>
//        private double[] Vortex, Vortex_old, VortexX, VortexY;
//        /// <summary>
//        /// правая часть
//        /// </summary>
//        private double[] S;
//        private double[] nodS;
//        /// <summary>
//        /// производные в области
//        /// </summary>
//        private double[][] dNdx;
//        private double[][] dNdy;
//        private double[][] N;
//        private double residual = 0.0;
//        private double relax = 0.5;
//        //Локальная матрица массы
//        double[][] MM = null; 
//        #endregion

//        public VortexCurrentFunction2D(IPropertyTask p):base(p)
//        {
//            Init();
//        }
//        /// <summary>
//        /// Инициализация задачи
//        /// </summary>
//        public override void InitTask()
//        {
//            cu = 3;
//            MEM.Alloc(mesh.CountKnots, ref vx, "vx");
//            MEM.Alloc(mesh.CountKnots, ref vy, "vy");
//            MEM.Alloc(mesh.CountKnots, ref phi, "phi");
//            MEM.Alloc(mesh.CountKnots, ref phi_old, "phi_old");
//            MEM.Alloc(mesh.CountKnots, ref Vortex, "Vortex");
//            MEM.Alloc(mesh.CountKnots, ref VortexX, "VortexX");
//            MEM.Alloc(mesh.CountKnots, ref VortexY, "VortexY");
//            MEM.Alloc(mesh.CountKnots, ref Vortex_old, "Vortex_old");
//            MEM.Alloc(mesh.CountKnots, ref nodS, "nodS");
//            MEM.Alloc(mesh.CountElements, ref S, "S");
//            MEM.Alloc2D(mesh.CountElements, cu, ref N);
//            MEM.Alloc2D(mesh.CountElements, cu, ref dNdx);
//            MEM.Alloc2D(mesh.CountElements, cu, ref dNdy);
//            // Установка начальных условий
//            BoundaryCondition.AreaFunction("Vortex",ref Vortex_old);
//            // матрица масс
//            double mm = 1.0 / 12;
//            MEM.Alloc2DClear(3, 3, ref MM, mm);
//            // матрица масс
//            MM[0][0] = 2 * mm; 
//            MM[1][1] = 2 * mm;
//            MM[2][2] = 2 * mm;
//            CalkDFF();
//        }
//        public void CalkDFF()
//        {
//            double[] x = mesh.GetCoords(0);
//            double[] y = mesh.GetCoords(1);

//            double x1, y1, x2, y2, x3, y3;
//            for (uint elem = 0; elem < mesh.CountElements; elem++)
//            {
//                mesh.ElementKnots(elem, ref knots);
//                uint node0 = knots[0];
//                uint node1 = knots[1];
//                uint node2 = knots[2];
//                x1 = x[node0];
//                x2 = x[node1];
//                x3 = x[node2];
//                y1 = y[node0];
//                y2 = y[node1];
//                y3 = y[node2];
//                // Вычисление площади КЭ  
//                S[elem] = 0.5 * (x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2));
//                double V = 1 / (2 * S[elem]);
//                // Производная от симплекс функции формы по направлению Х
//                dNdx[elem][0] = (y[1] - y[2]) * V;
//                dNdx[elem][1] = (y[2] - y[0]) * V;
//                dNdx[elem][2] = (y[0] - y[1]) * V;
//                // Element Matrix df/dy
//                dNdy[elem][0] = (x[2] - x[1]) * V;
//                dNdy[elem][1] = (x[0] - x[2]) * V;
//                dNdy[elem][3] = (x[1] - x[0]) * V;
//                // Вычисление площади КО собираем кусочки площади КЭ в узле
//                nodS[node0] += S[elem] / 3.0;
//                nodS[node1] += S[elem] / 3.0;
//                nodS[node2] += S[elem] / 3.0;
//            }



//        }
//        #region локальные методы задачи
//        /// <summary>
//        /// Вычисление производных от функции p в узлах сетки
//        /// </summary>
//        /// <param name="p"></param>
//        /// <param name="dpdx"></param>
//        /// <param name="dpdy"></param>
//        public void CalkGrad(double[] p, ref double[] dpdx, ref double[] dpdy)
//        {
//            for (uint elem = 0; elem < mesh.CountElements; elem++)
//            {
//                mesh.ElementKnots(elem, ref knots);
//                uint node0 = knots[0];
//                uint node1 = knots[1];
//                uint node2 = knots[2];
//                double eU = dNdy[elem][0] * p[node0] + dNdy[elem][1] * p[node1] + dNdy[elem][2] * p[node2];
//                double eV = dNdx[elem][0] * p[node0] + dNdx[elem][1] * p[node1] + dNdx[elem][2] * p[node2];
//                dpdx[node0] += eU * S[elem] / 3.0;
//                dpdx[node1] += eU * S[elem] / 3.0;
//                dpdx[node2] += eU * S[elem] / 3.0;
//                dpdy[node0] += eV * S[elem] / 3.0;
//                dpdy[node1] += eV * S[elem] / 3.0;
//                dpdy[node2] += eV * S[elem] / 3.0;
//            }
//            for (uint nod = 0; nod < mesh.CountKnots; nod++)
//            {
//                dpdx[nod] /= nodS[nod];
//                dpdy[nod] /= nodS[nod];
//            }
//        }

//        #endregion
//        #region переопределенные методы задачи
//        /// <summary>
//        /// Установка новых отметок дна - перегенерация КЭ сетки и преренос данных на нее
//        /// </summary>
//        /// <pTask name="zeta"></pTask>
//        public override void SetZeta(double[] zeta, EBedErosion bedErosion)
//        {
//              Erosion = bedErosion;
//            if (zeta != null)
//            {
//                // FlagStartMesh - первая генерация сетки
//                // bedErosion - генерация сетки при размывах дна
//                if (FlagStartMesh == false || bedErosion == true)
//                {
//                    // Получение новой границы области и формирование сетки
//                    //qmesh.CreateNewQMesh(zeta);
//                    //// конвертация ReverseQMesh в сетку задачи
//                    //ConvertMeshToMesh();
//                    FlagStartMesh = true;
//                }
//            }
//            else
//            {
//                // Получение текущих донных отметок
//                //qmesh.GetBed(ref zeta);
//            }
//        }
//        /// <summary>
//        /// Расчет полей глубины и скоростей на текущем шаге по времени
//        /// </summary>
//        public override void SolverStep()
//        {
//            int n;
//            //for (n = 0; n < 10; n++)
//            {
//                MEM.MemCopy(ref phi_old, phi);
//                MEM.MemCopy(ref Vortex_old, Vortex);
//                // ----------------------------
//                // 1. Решается задача для функции тока
//                algebra.Clear();
//                for (uint elem = 0; elem < mesh.CountElements; elem++)
//                {
//                    // получить узлы КЭ
//                    mesh.ElementKnots(elem, ref knots);
//                    // получить детерминант
//                    double detJ = 2 * Math.Abs(S[elem]);
//                    // вычисление ЛЖМ
//                    for (int ai = 0; ai < cu; ai++)
//                        for (int aj = 0; aj < cu; aj++)
//                            LaplMatrix[ai][aj] = (dNdx[elem][ai] * dNdx[elem][aj] + dNdy[elem][ai] * dNdy[elem][aj]) * S[elem];

//                    for (int ai = 0; ai < cu; ai++)
//                    {
//                        for (int aj = 0; aj < cu; aj++)
//                            LocalRight[ai] += MM[elem][aj] * Vortex[knots[aj]];
//                        LocalRight[ai] *= - S[elem];
//                    }
//                    // добавление вновь сформированной ЛЖМ в ГМЖ
//                    algebra.AddToMatrix(LaplMatrix, knots);
//                    algebra.AddToRight(LocalRight, knots);
//                }
//                // удовлетворение ГУ для расчета функции вихря
//                BordersCondition(algebra, "phi");
//                // решение слау
//                algebra.Solve(ref phi);
//                // ----------------------------
//                // 2.Определяется поле скорости
//                CalkGrad(phi, ref vx, ref vy);
//                // ----------------------------
//                // 3.Рассчитывается функция-- распределение вихря на границе --как решение задачи
//                // Vortex = diff(phi,x,2)+diff(phi,y,2)
//                // с естественным граничным условием diff(phi,x) n_x + diff(phi,y) n_y
//                algebra.Clear();
//                for (uint elem = 0; elem < mesh.CountElements; elem++)
//                {
//                    // получить узлы КЭ
//                    mesh.ElementKnots(elem, ref knots);
//                    // получить детерминант
//                    double detJ = 2 * Math.Abs(S[elem]);
//                    // вычисление ЛЖМ
//                    for (int ai = 0; ai < cu; ai++)
//                        for (int aj = 0; aj < cu; aj++)
//                            LaplMatrix[ai][aj] = MM[elem][aj] * S[elem];
//                    for (int ai = 0; ai < cu; ai++)
//                        for (int aj = 0; aj < cu; aj++)
//                            LocalRight[ai] += (dNdx[elem][ai] * dNdx[elem][aj] + dNdy[elem][ai] * dNdy[elem][aj]) * phi[knots[aj]] * S[elem];
//                    // добавление вновь сформированной ЛЖМ в ГМЖ
//                    algebra.AddToMatrix(LaplMatrix, knots);
//                    algebra.AddToRight(LocalRight, knots);
//                }
//                algebra.Solve(ref Vortex);
//                // ----------------------------
//                // 4. Решается задача для завихренности
//                algebra.Clear();
//                for (uint elem = 0; elem < mesh.CountElements; elem++)
//                {
//                    // получить узлы КЭ
//                    mesh.ElementKnots(elem, ref knots);
//                    // получить детерминант
//                    double detJ = 2 * Math.Abs(S[elem]);
//                    // вычисление ЛЖМ
//                    for (int ai = 0; ai < cu; ai++)
//                        for (int aj = 0; aj < cu; aj++)
//                            LaplMatrix[ai][aj] = (MM[elem][aj] - dtime * pTask.Mu * (dNdx[elem][ai] * dNdx[elem][aj] + dNdy[elem][ai] * dNdy[elem][aj])) * S[elem];
//                    // Проверить в maple
//                    for (int ai = 0; ai < cu; ai++)
//                        for (int aj = 0; aj < cu; aj++)
//                        {
//                            LocalRight[ai] += (vx[knots[0]] * MM[ai][0] + vx[knots[1]] * MM[ai][1] + vx[knots[2]] * MM[ai][2]) * Vortex[knots[ai]] * dNdx[elem][ai] * detJ;
//                            LocalRight[ai] += (vy[knots[0]] * MM[ai][0] + vy[knots[1]] * MM[ai][1] + vy[knots[2]] * MM[ai][2]) * Vortex[knots[ai]] * dNdy[elem][ai] * detJ;
//                            LocalRight[ai] += MM[ai][aj] * Vortex[knots[aj]] * S[elem];
//                        }
//                    // добавление вновь сформированной ЛЖМ в ГМЖ
//                    algebra.AddToMatrix(LaplMatrix, knots);
//                    algebra.AddToRight(LocalRight, knots);
//                }
//                 algebra.Solve(ref Vortex);
//            }
//        }
//        /// <summary>
//        /// Формирование данных для отрисовки связанных с сеткой IMesh со сороны задачи
//        /// </summary>
//        /// <pTask name="sp">контейнер данных</pTask>
//        public override void AddMeshPolesForGraphics(ISavePoint sp)
//        {

//        }
//        /// <summary>
//        /// Получение полей придонных касательных напряжений и давления/свободной поверхности по контексту задачи
//        /// усредненных на конечных элементах
//        /// </summary>
//        /// <pTask name="tauX">придонное касательное напряжение по х</pTask>
//        /// <pTask name="tauY">придонное касательное напряжение по у</pTask>
//        /// <pTask name="P">придонных давления/свободная поверхности потока, по контексту задачи</pTask>
//        public override void GetTau(ref double[] _tauX, ref double[] _tauY, ref double[] P, ref double[][] CS, StressesFlag sf = StressesFlag.Nod)
//        {
//            tauY = null;
//            double[] yp = null;
//            //GetForce(ref tauX, ref P, ref yp);
//        }
//        /// <summary>
//        /// Создает экземпляр класса
//        /// </summary>
//        /// <returns></returns>
//        public override IRiver Clone()
//        {
//            return new VortexCurrentFunction2D(pTask);
//        }
//        #endregion
//    }
//}
