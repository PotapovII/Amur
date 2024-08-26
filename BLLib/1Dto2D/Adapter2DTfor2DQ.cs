//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2021 -
//                       ALL RIGHT RESERVED
//                        ПРОЕКТ "BLLib"
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                          27.02.21
//---------------------------------------------------------------------------
//                  добавлен контроль потери массы
//                               27.03.22
//---------------------------------------------------------------------------
namespace BLLib
{
    using System;
    using CommonLib;
    using MemLogLib;
    using MeshLib;

    /// <summary>
    /// ОО: Класс для решения плановой задачи о 
    /// расчете донных деформаций русла на симплекс сетке
    /// </summary>
    [Serializable]
    public class Adapter2DTfor2DQ : ABedLoadFEMTask2D
    {
        public override IBedLoadTask Clone()
        {
            return new Adapter2DTfor2DQ(new BedLoadParams());
        }

        /// <summary>
        /// отметки дна
        /// </summary>
        public double[][] zeta;
        public double[][] zeta0;
        protected double[][] tauX = null;
        protected double[][] tauY = null;
        protected double[][] press = null;

        Locator_TriMeshToQuad Locator = null;
        BedLoadTask2DFV bltask2D = null;
        RectFVMesh mesh2D = null;

        #region 1D 
        // массивы для конвертации 2D в 1D и обратно
        //List <HPoint> xy = new List<HPoint>();
        //List<double> x0 = new List<double>();
        //List<List<int>> link = new List<List<int>>();
        //double[] zeta01D = null;
        //double[] zeta1D = null;
        //double[] tau1D = null;
        //double[] p1D = null;
        //double[] x1D = null;
        //IMesh mesh1D = null;
        //CBedLoadTask1D bltask = null;
        #endregion 
        /// <summary>
        /// Конструктор 
        /// </summary>
        public Adapter2DTfor2DQ(BedLoadParams p) : base(p)
        {
            cu = 3;
            name = "2DQ деформация одно-фракционного дна в плане (2DT).";
        }
        /// <summary>
        /// Установка исходных данных
        /// </summary>
        /// <param name="mesh">Сетка расчетной области</param>
        /// <param name="algebra">Решатель для СЛАУ </param>
        /// <param name="BCBed">граничные условия</param>
        /// <param name="Zeta0">начальный уровень дна</param>
        /// <param name="theta">Параметр схемы по времени</param>
        /// <param name="dtime">шаг по времени</param>
        /// <param name="isAvalanche">флаг использования лавинной модели</param>
        public override void SetTask(IMesh mesh, double[] Zeta0, IBoundaryConditions BConditions)
        {
            if (mesh == null)
            {
                Logger.Instance.Info("объект mesh в методе (CBedLoadFEMTask2DTri) SetTask пуст");
                return;
            }
            taskReady = false;
            if (mesh.CountKnots == 0)
            {
                Logger.Instance.Info("объект mesh в методе (CBedLoadFEMTask2DTri) SetTask пуст");
                return;
            }
            uint NN = (uint)Math.Sqrt(mesh.CountKnots);
            base.SetTask(mesh, Zeta0, BConditions);
            InitLocal(cu);

            Locator = new Locator_TriMeshToQuad(mesh);
            
            Locator.CreateMesh();

            Locator.GetValue_2DQfrom2DT(ref zeta0, Zeta0);
            mesh2D = Locator.QMesh;
            // инициализация 2D задачи
            bltask2D = new BedLoadTask2DFV(this);
            bltask2D.SetDTime(time);
            bltask2D.SetTask(mesh2D, zeta0, BConditions);
            bltask2D.BCondIn = new BoundCondition1D(TypeBoundCond.Neumann, 0);
            bltask2D.BCondIn = new BoundCondition1D(TypeBoundCond.Neumann, 0);
            bltask2D.BCondOut = new BoundCondition1D(TypeBoundCond.Transit, 0);
            bltask2D.dtime = dtime;

            taskReady = true;
        }

        /// <summary>
        /// Модель дна: расчет коэффициентов A B C
        /// </summary>
        public override void CalkCoeff(uint elem, double mtauS, double dZx, double dZy) { }

        /// <summary>
        /// Вычисление изменений формы донной поверхности 
        /// на одном шаге по времени по модели 
        /// Петрова А.Г. и Потапова И.И. 2014
        /// Реализация решателя - методом контрольных объемов,
        /// Патанкар (неявный дискретный аналог ст 40 ф.3.40 - 3.41)
        /// Коэффициенты донной подвижности, определяются 
        /// как среднее гармонические величины         
        /// </summary>
        /// <param name="Zeta0">текущая форма дна</param>
        /// <param name="tau">придонное касательное напряжение</param>
        /// <param name="P">придонное давление</param>
        /// <returns>новая форма дна</returns>
        /// </summary>
        public override void CalkZetaFDM(ref double[] Zeta, double[] TauX, double[] TauY, double[] Press = null, double[][] CS = null)
        {
            try
            {
                // проекция напряжений на 2DQ
                Locator.GetValue_2DQfrom2DT(ref tauX, TauX);
                // проекция напряжений на 2DQ
                if (TauY != null)
                    Locator.GetValue_2DQfrom2DT(ref tauY, TauY);
                // проекция напряжений на 2DQ
                if (Press != null)
                    Locator.GetValue_2DQfrom2DT(ref press, Press);
                // расчет дна
                bltask2D.CalkZetaFDM(ref zeta, tauX, tauY, press);
                // проекция дна на 2DT
                MEM.Alloc<double>(Zeta0.Length, ref Zeta);
                Locator.SetValue_2DTfrom2DQ(ref Zeta, zeta);

                //var iZ0 = IntZeta(Zeta0);
                //var iZ = IntZeta(Zeta);
                //double errorZ = 100 * (iZ0.int_Z - iZ.int_Z) / (iZ0.int_Z + MEM.Error10);
                //double errorZL2 = 100 * (iZ0.int_Z2 - iZ.int_Z2) / (iZ0.int_Z2 + MEM.Error10);

                //#region Контроль баланса массы за 1 шаг по времени
                //var aiZ = IntZeta(Zeta);

                //double a_errorZ = 100 * (aiZ.int_Z - iZ0.int_Z) / (iZ0.int_Z + MEM.Error10);
                //Logger.Instance.AddHeaderInfo("Integral mass balance control");
                //Logger.Instance.AddHeaderInfo("Интегральный контроль баланса массы");
                //string str = " int (Zeta0) = " + iZ0.int_Z.ToString() +
                //             " int (Zeta)= " + iZ.int_Z.ToString();
                //Logger.Instance.AddHeaderInfo(str);
                //str = " errorZ = " + errorZ.ToString("F6") + " %" +
                //      " errorZL2 = " + errorZL2.ToString("F6") + " %";
                //Logger.Instance.AddHeaderInfo(str);
                //str = " a_int (1) = S = " + aiZ.Area.ToString("") +
                //      " a_int (Zeta) = " + aiZ.int_Z.ToString("") +
                //      " a_errorZ = " + a_errorZ.ToString("F6") + " %";
                //Logger.Instance.AddHeaderInfo(str);
                //Logger.Instance.AddHeaderInfo("min Zeta = " + Zeta.Min() + "  max Zeta = " + Zeta.Max());
                //#endregion

                // переопределение начального значения zeta 
                // для следующего шага по времени
                for (int j = 0; j < Zeta.Length; j++)
                    Zeta0[j] = Zeta[j];
            }
            catch (Exception e)
            {
                Message = e.Message;
                Logger.Instance.Exception(e);
                for (int j = 0; j < Zeta.Length; j++)
                    Zeta[j] = Zeta0[j];
            }
        }
        /// <summary>
        /// Формирование данных для отрисовки связанных с сеткой IMesh со сороны задачи
        /// </summary>
        /// <param name="sp">контейнер данных</param>
        public override void AddMeshPolesForGraphics(ISavePoint sp)
        {
            if (sp != null)
            {
                //double[] dw = new double[DryWet.Length];
                //for (int i = 0; i < dw.Length; i++)
                //    dw[i] = DryWet[i];
                //sp.Add("DryWet", dw);
                //sp.Add("Поток Gx", Gx);
                //sp.Add("Поток Gy", Gy);
                //sp.Add("Поток G", Gx, Gy);
            }
        }

    }
}