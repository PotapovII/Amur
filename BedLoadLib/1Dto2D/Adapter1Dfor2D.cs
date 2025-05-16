//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2021 -
//                       ALL RIGHT RESERVED
//                        ПРОЕКТ "BedLoadLib"
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                          27.02.21
//---------------------------------------------------------------------------
//                  добавлен контроль потери массы
//                               27.03.22
//---------------------------------------------------------------------------
//                  добавлены внешнии граничные условия
//                               24.08.22
//---------------------------------------------------------------------------
namespace BedLoadLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GeometryLib;
    using CommonLib;
    using MemLogLib;
    using MeshLib;
    using CommonLib.Geometry;
    using CommonLib.Function;
    using CommonLib.BedLoad;

    /// <summary>
    /// ОО: Класс для решения плановой задачи о 
    /// расчете донных деформаций русла на симплекс сетке
    /// </summary>
    [Serializable]
    public class Adapter1Dfor2D : ABedLoadFEM_2D
    {
        /// <summary>
        /// Загрузка полей задачи из форматного файла
        /// </summary>
        /// <param name="file">имя файла</param>
        public override void LoadData(IDigFunction[] crossFunctions = null)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Название файла параметров задачи
        /// </summary>
        public override string NameBLParams()
        {
            return "BedLoadParams2D.txt";
        }

        public override IBedLoadTask Clone()
        {
            return new Adapter1Dfor2D(new BedLoadParams2D());
        }
        // массивы для конвертации 2D в 1D и обратно
        List<HPoint> xy = new List<HPoint>();
        List<double> x0 = new List<double>();
        List<List<int>> link = new List<List<int>>();
        double[] zeta01D = null;
        double[] zeta1D = null;
        double[] tau1D = null;
        double[] p1D = null;
        double[] x1D = null;
        IMesh mesh1D = null;

        BedLoadFVM_1XD bltask = null;
        /// <summary>
        /// Конструктор 
        /// </summary>
        public Adapter1Dfor2D(BedLoadParams2D p) : base(p)
        {
            cu = 3;
            name = "1D деформация одно-фракционного дна в плане (2D).";
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
        public override void SetTask(IMesh mesh, double[] Zeta0, double[] Roughness, IBoundaryConditions BConditions)
        {
            if (mesh == null)
            {
                Logger.Instance.Info("объект mesh в методе (CBedLoadFEMTask2DTri) SetTask пуст");
                return;
            }

            
            if (mesh.CountKnots == 0)
            {
                Logger.Instance.Info("объект mesh в методе (CBedLoadFEMTask2DTri) SetTask пуст");
                return;
            }
            uint NN = (uint)Math.Sqrt(mesh.CountKnots);
            base.SetTask(mesh, Zeta0, Roughness, BConditions);
            InitLocal(cu);

            bltask = new BedLoadFVM_1XD(Params);
            // вычисление массива связей    
            Adapter_2D_to_1D_Mesh();
            // проекция дна на 1D
            GetValue1Dfrom2D(ref zeta01D, Zeta0);
            // 1D сетка

            mesh1D = new TwoMesh(x1D, zeta01D);
            BedLoadParams1D p = (BedLoadParams1D)bltask.GetParams();
            p.BCondIn = new BoundCondition1D(TypeBoundCond.Neumann, 0);
            p.BCondOut = new BoundCondition1D(TypeBoundCond.Transit, 0);
            bltask.SetParams(p);

            bltask.dtime = dtime;
            bltask.SetTask(mesh1D, zeta01D, Roughness, BConditions);

            eTaskReady = ETaskStatus.CreateMesh;
        }
        /// <summary>
        /// вычисление массива связей 2D => 1D
        /// </summary>
        protected void Adapter_2D_to_1D_Mesh()
        {
            double[] xx = mesh.GetCoords(0);
            for (int i = 0; i < mesh.CountKnots; i++)
                xy.Add(new HPoint(xx[i], i));
            xy.Sort();

            int k = 0;
            List<int> lk = new List<int>();
            x0.Add(xy[k].x);
            lk.Add((int)xy[k].y);
            link.Add(lk);
            for (int i = 1; i < xy.Count; i++)
            {
                if (MEM.Equals(x0[k], xy[i].x, 0.0001) != true)
                {
                    x0.Add(xy[i].x);
                    lk = new List<int>();
                    lk.Add((int)xy[i].y);
                    link.Add(lk);
                    k++;
                }
                else
                {
                    lk.Add((int)xy[i].y);
                }
            }
            x1D = x0.ToArray();
        }
        /// <summary>
        /// Проекция двумерного поля в одномерное
        /// </summary>
        /// <param name="value1D"></param>
        /// <param name="source2D"></param>
        protected void GetValue1Dfrom2D(ref double[] value1D, double[] source2D)
        {
            MEM.AllocClear(link.Count, ref value1D);
            for (int i = 0; i < link.Count; i++)
                value1D[i] = source2D[link[i][0]];
        }
        /// <summary>
        /// Проекция одномерного поля в двумерное
        /// </summary>
        /// <param name="value2D"></param>
        /// <param name="source1D"></param>
        protected void SetValue1Dfrom2D(ref double[] value2D, double[] source1D)
        {
            for (int i = 0; i < link.Count; i++)
            {
                double s = source1D[i];
                for (int j = 0; j < link[i].Count; j++)
                    value2D[link[i][j]] = s;
            }
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
        public override void CalkZetaFDM(ref double[] Zeta, double[] tauX, double[] tauY, double[] P = null, double[][] CS = null)
        {
            try
            {
                MEM.Alloc<double>(Zeta0.Length, ref Zeta);
                //// проекция дна на 1D
                // GetValue1Dfrom2D(ref zeta1D, Zeta);
                // проекция напряжений на 1D
                GetValue1Dfrom2D(ref tau1D, tauX);
                // проекция напряжений на 1D
                if (P != null)
                    GetValue1Dfrom2D(ref p1D, P);
                else
                    p1D = null;
                // расчет дна
                bltask.CalkZetaFDM(ref zeta1D, tau1D, null, p1D);
                // проекция дна на 1D
                SetValue1Dfrom2D(ref Zeta, zeta1D);

                var iZ0 = IntZeta(Zeta0);
                var iZ = IntZeta(Zeta);
                double errorZ = 100 * (iZ0.int_Z - iZ.int_Z) / (iZ0.int_Z + MEM.Error10);
                double errorZL2 = 100 * (iZ0.int_Z2 - iZ.int_Z2) / (iZ0.int_Z2 + MEM.Error10);

                #region Контроль баланса массы за 1 шаг по времени
                var aiZ = IntZeta(Zeta);

                double a_errorZ = 100 * (aiZ.int_Z - iZ0.int_Z) / (iZ0.int_Z + MEM.Error10);
                Logger.Instance.AddHeaderInfo("Integral mass balance control");
                Logger.Instance.AddHeaderInfo("Интегральный контроль баланса массы");
                string str = " int (Zeta0) = " + iZ0.int_Z.ToString() +
                             " int (Zeta)= " + iZ.int_Z.ToString();
                Logger.Instance.AddHeaderInfo(str);
                str = " errorZ = " + errorZ.ToString("F6") + " %" +
                      " errorZL2 = " + errorZL2.ToString("F6") + " %";
                Logger.Instance.AddHeaderInfo(str);
                str = " a_int (1) = S = " + aiZ.Area.ToString("") +
                      " a_int (Zeta) = " + aiZ.int_Z.ToString("") +
                      " a_errorZ = " + a_errorZ.ToString("F6") + " %";
                Logger.Instance.AddHeaderInfo(str);
                Logger.Instance.AddHeaderInfo("min Zeta = " + Zeta.Min() + "  max Zeta = " + Zeta.Max());
                #endregion

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
                if (link.Count >0)
                {
                    double[] tau0Elem = bltask.tau0Elem;
                    double[] tau01D = new double[tau0Elem.Length+1];
                    tau01D[0] = tau0Elem[0];
                    for (int i = 1; i < tau0Elem.Length; i++)
                        tau01D[i] = 0.5 * (tau0Elem[i - 1] + tau0Elem[i]);
                    tau01D[tau01D.Length-1] = tau0Elem[tau0Elem.Length-1];
                    double[] tau02D = null;
                    MEM.Alloc<double>(Zeta0.Length, ref tau02D);
                    // проекция дна на 1D
                    SetValue1Dfrom2D(ref tau02D, tau01D);
                    sp.Add("Критическое напряжение", tau02D);

                    // кривые
                    IGraphicsData gd = sp.graphicsData;
                    IGraphicsCurve curves1 = sp.CloneCurves("Отметки дна напряжение", x1D, zeta1D);
                    IGraphicsCurve curves2 = sp.CloneCurves("Напряжение", x1D, tau1D);
                    gd.Add(curves1);
                    gd.Add(curves2);
                }
                //double[] dw = new double[DryWet.Length];
                //for (int i = 0; i < dw.Length; i++)
                //    dw[i] = DryWet[i];
                
                //sp.Add("Поток Gx", Gx);
                //sp.Add("Поток Gy", Gy);
                //sp.Add("Поток G", Gx, Gy);
            }
        }

    }
}