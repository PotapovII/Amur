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
    using System;
    using System.ComponentModel;
    using CommonLib;
    using GeometryLib;

    /// <summary>
    /// ОО: Абстрактный класс для задач донных деформаций,
    /// реализует общий интерфейст задач по деформациям дна
    /// </summary>
    [Serializable]
    public abstract class ABedLoadTask : BedLoadParams, IBedLoadTask
    {
        #region  Свойства
        /// <summary>
        /// Название файла параметров задачи
        /// </summary>
        public string NameBLParams() => "NameBLParams.txt";
        /// <summary>
        /// текущее время расчета 
        /// </summary>
        [DisplayName("Текущее время")]
        [Category("Задача")]
        public double time { get; set; }
        /// <summary>
        /// Шаг по времени
        /// </summary>
        [DisplayName("Шаг по времени")]
        [Category("Задача")]
        public double dtime { get; set; }
        /// <summary>
        /// Наименование задачи
        /// </summary>
        [DisplayName("Наименование задачи")]
        [Category("Задача")]
        public string Name { get => name; }
        protected string name;
        /// <summary>
        /// Тип задачи используется для выбора совместимых подзадач
        /// </summary>
        [DisplayName("Профиль задачи")]
        [Category("Задача")]
        public TypeTask typeTask { get => tTask; }
        protected TypeTask tTask;
        /// <summary>
        /// версия дата последних изменений интерфейса задачи
        /// </summary>
        [DisplayName("Версия задачи")]
        [Category("Задача")]
        public string VersionData() => "ABedLoadTask 30.01.2022"; 
        /// <summary>
        /// Сетка решателя
        /// </summary>
        public IMesh Mesh() => mesh; 
        public IMesh mesh;
        /// <summary>
        /// Искомые поля задачи которые позволяют сформировать на сетке расчетной области краевые условия задачи
        /// </summary>
        public IUnknown[] Unknowns() => unknowns;
        protected IUnknown[] unknowns =  { new Unknown("Отметки дна", null, TypeFunForm.Form_1D_L1) };   
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

        #endregion
        #region Служебные переменные
        /// <summary>
        /// Количество расчетных узлов для дна
        /// </summary>
        protected int Count;
        /// <summary>
        /// текущая итерация по времени 
        /// </summary>
        protected int CountTime = 0;
        /// <summary>
        /// количество узлов по времени
        /// </summary>
        protected int LengthTime = 20000000;
        /// <summary>
        /// расчетный период времени, сек 
        /// </summary>
        protected double T;
        protected double dx, dp;
        protected double mtau, chi;
        #endregion
        #region Рабочие массивы
        /// <summary>
        /// Флаг для определения сухого-мокрого дна
        /// </summary>
        public int[] DryWet = null;
        /// <summary>
        /// массив донных отметок
        /// </summary>
        public double[] Zeta = null;
        /// <summary>
        /// массив донных отметок на предыдущем слое по времени
        /// </summary>
        public double[] Zeta0 = null;
        /// <summary>
        /// массив производных для уровня донных отметок 
        /// </summary>
        public double[] dZeta = null;
        /// <summary>
        /// массив придонных касательнывх напряжений
        /// </summary>
        public double[] tau = null;
        /// <summary>
        /// массив придонного давления
        /// </summary>
        public double[] P = null;
        /// <summary>
        /// шероховатость дна
        /// </summary>
        public double[] Roughness = null;

        #endregion
        /// <summary>
        /// Граничные условия задачи
        /// </summary>
        protected IBoundaryConditions BConditions = null;
        /// <summary>
        /// Масштаб расхода от уклона дна
        /// </summary>
        /// <param name="Gx">(d zeta/d x)/tan phi</param>
        /// <param name="Gy">(d zeta/d y)/tan phi</param>
        /// <returns></returns>
        public double DRate(double Gx, double Gy=0)
        {
            double scaleMin = 0.05;
            double scale = 1 - Gx * Gx - Gy * Gy;
            if (scale <= scaleMin) // область осыпания склона
                return 20; // 1.0 / scaleMin;
            else
                return 1.0 / scale;
        }





        // старая 05 11 2022
        //public double DRate(double Gx, double Gy = 0)
        //{
        //    double scaleMin = 0.01;
        //    double scale = 1;
        //    if (Gx > 0)
        //    {
        //        scale = 1 - Gx * Gx - Gy * Gy;
        //        if (scale > scaleMin && scale <= 1)
        //            scale = 1.0 / scale;
        //        else
        //            scale = 1.0 / scaleMin;
        //    }
        //    return scale;
        //}
        /// <summary>
        /// Конструктор по умолчанию/тестовый
        /// </summary>
        public ABedLoadTask(BedLoadParams p) : base(p)
        {
            InitBedLoad();
            tTask = TypeTask.streamXY2D;
        }
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
        /// Установка текущего шага по времени
        /// </summary>
        /// <param name="dtime"></param>
        public void SetDTime(double dtime)
        {
            this.dtime = dtime;
        }
        /// <summary>
        /// Установка текущей геометрии расчетной области
        /// </summary>
        /// <param name="mesh">Сетка расчетной области</param>
        /// <param name="Zeta0">начальный уровень дна</param>
        /// <param name="Roughness">шероховатость дна</param>
        /// <param name="BConditions">граничные условия, 
        /// количество в обзем случае определяется через маркеры 
        /// границ сетеи</param>
        public virtual void SetTask(IMesh mesh, double[] Zeta0, double[] Roughness, IBoundaryConditions BConditions)
        {
            taskReady = false;
            this.mesh = mesh;
            this.Zeta0 = Zeta0;
            this.Count = mesh.CountKnots;
            this.Roughness = Roughness;
            this.BConditions = BConditions;
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
        /// Формирование данных для отрисовки связанных с сеткой IMesh со сороны задачи
        /// </summary>
        /// <param name="sp">контейнер данных</param>
        public abstract void AddMeshPolesForGraphics(ISavePoint sp);
        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public abstract IBedLoadTask Clone();
        /// <summary>
        /// Вычисление текущих расходов и их градиентов для построения графиков
        /// </summary>
        /// <param name="Gs">возвращаемые расходы (по механизмам движения)</param>
        /// <param name="dGs">возвращаемые градиенты расходы (по механизмам движения)</param>
        /// <param name="tau">придонное касательное напряжение</param>
        /// <param name="P">придонное давление</param>
        public abstract void Calk_Gs(ref double[][] Gs, ref double[][] dGs, double[] tau, double[] P = null);
    }
}
