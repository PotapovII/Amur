//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//            перенесено с правкой : 26.04.2021 Потапов И.И. 
//---------------------------------------------------------------------------
// разделен на абстрактную и производную часть : 26.02.2022 Потапов И.И. 
//---------------------------------------------------------------------------
// Решение с помощью циклического метода ГЭ задачи
// о деформировании дна под цилиндром при его обтекании
// гидродинамическим потоком
//---------------------------------------------------------------------------
namespace RiverLib
{
    using CommonLib;
    using MemLogLib;
    using System;
    using MeshAdapterLib;


    [Serializable]
    public class RiverBECircleNewMesh : RiverBEMCircle
    {
        protected IFEMesh mesh = null;
        protected double[] phi = null;
        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public override IRiver Clone()
        {
            return new RiverBECircleNewMesh(new RiverBECircleParams());
        }
        /// <summary>
        /// версия дата последних изменений интерфейса задачи
        /// </summary>
        public override string VersionData() => "RiverBECircleNewMesh 01.04.2022"; 
        /// <summary>
        /// Конструктор
        /// </summary>
        public RiverBECircleNewMesh(RiverBECircleParams p) : base(p)
        {
            name = "гидрадинамика потока под трубой (МГЭ) н.в. с сеткой";
            InitTask();
        }
        /// <summary>
        /// Метод выделения ресурсов задачи после установки свойств задачи
        /// </summary>
        public override void InitTask()
        {
            // выделение памяти
            base.InitAreaTask();
            // Определение начальной формы цилиндра и дна
            InitGeomenryTask();
            // Вычисляемые параметры геометрии
            base.ClakGeomenryParams();
        }
        /// <summary>
        /// Определение формы цилиндра и дна
        /// </summary>
        public override void InitGeomenryTask(double scale = 1)
        {
            base.InitGeomenryTask(scale);
        }
        /// <summary>
        /// Создание сетки в задачах граничных элементов для вывода полей тока....
        /// </summary>
        public override void AddMeshPolesForGraphics(ISavePoint sp)
        {
            try
            {
                mesh = MeshAdapter.CreateFEMesh(fxW, fyW, fxB, fyB, Params.HPhi, Params.LPhi);
                MEM.Alloc(mesh.CountKnots, ref phi);
                double[] xx = mesh.CoordsX;
                double[] yy = mesh.CoordsY;

                for (int i = 0; i < mesh.CountKnots; i++)
                    phi[i] = CalkPhi(xx[i], yy[i], i);

                double xmax = 0;
                double xmin = 0;
                mesh.MinMax(0, ref xmin, ref xmin);

                int[] bk = mesh.GetBoundKnots();
                int[] fbk = mesh.GetBoundKnotsMark();
                double VCF = FF[NNw + NNb];
                for (int i = 0; i < bk.Length; i++)
                {
                    bool fb = (MEM.Equals(xx[bk[i]], xmin) == true ||
                        MEM.Equals(xx[bk[i]], xmax) == true ||
                        MEM.Equals(yy[bk[i]], Params.HPhi) == true);
                    if (fb == false && fbk[i] == 1)
                        phi[bk[i]] = VCF;
                }


                sp.SetSavePoint(time, mesh, null);
                sp.Add("Функция тока " + time.ToString(), phi);
                sp.Add("Координата Х", xx);
                sp.Add("Координата Y", yy);
                base.AddMeshPolesForGraphics(sp);
            }
            catch(Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
        }
        /// <summary>
        ///  Вычисление функции тока во внешней области
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public double CalkPhi(double x, double y, int i)
        {
            double p;
            double pw = 0, pb = 0;
            //double tp;
            double pi2 = 1 / (2 * Math.PI);

            green.Set(fxW, fyW, fxW, fyW, detJW, bottomPeriod);
            for (int j = 0; j < NNw; j++)
            {
                pw += -detJW[j] / NNw * green.Get(x, y, j) * FF[j] * pi2;
            }
            green.Set(fxB, fyB, fxB, fyB, detJB, bottomPeriod);
            for (int j = 0; j < NNb; j++)
            {
                pb += -detJB[j] / NNb * green.Get(x, y, j) * FF[NNw + j] * pi2;
            }
            p = pb + pw;
            p += 0.5 * (FF[NNw + NNb] + y * Params.U_inf);
            if (double.IsInfinity(p) == true)
            {
                if (i < NNw)
                    p = FF[NNw + NNb];
                else
                    p = FF[NNw + NNb + 1];
            }
            return p;
        }
      
    }

}
