//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 14.12.2020 Потапов И.И.
//---------------------------------------------------------------------------
namespace RenderLib
{
    using System;
    using System.Drawing;
    /// <summary>
    /// Установка опций отрисовки данных  задачи
    /// </summary>
    public class RenderOptions
    {
        /// <summary>
        /// Масштаб полей
        /// </summary>
        public int scaleFields = 0;
        /// <summary>
        /// Орисовка фильтра данных 
        /// </summary>
        public int showFilter = - 1;
        /// <summary>
        /// Отрисовка сетки
        /// </summary>
        public bool showMesh = false;
        /// <summary>
        /// Отрисовка координатных осей
        /// </summary>
        public bool coordReper = true;
        /// <summary>
        /// Обновление маштаба при обновлении данных
        /// </summary>
        public bool ckScaleUpdate = false;
        /// <summary>
        /// Орисовка узлов конечных элементов
        /// </summary>
        public bool showKnotNamber = false;
        /// <summary>
        /// Числовое поле в узлах
        /// </summary>
        public bool opValuesKnot = false;
        /// <summary>
        /// Индекс поля
        /// </summary>
        public int indexValues = 0;
        /// <summary>
        /// Инверсия системы координат X<=>Y
        /// </summary>
        public bool coordInv = false;
        /// <summary>
        /// Настройки для печати
        /// </summary>
        public bool printOp = false;

        public RenderOptions() { }
        public RenderOptions(RenderOptions o)
        {
            showFilter = o.showFilter;
            showMesh = o.showMesh;
            coordReper = o.coordReper;
            ckScaleUpdate = o.ckScaleUpdate;
            indexValues = o.indexValues;
            showKnotNamber = o.showKnotNamber;
            opValuesKnot = o.opValuesKnot;
            scaleFields = o.scaleFields;
            coordInv = o.coordInv;
            printOp = o.printOp;
        }
        /// <summary>
        /// 15 07 24 изменяемый масштаб полей
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public double ScaleValue(double value)
        {
            return value * Math.Pow(10, scaleFields);
        }
    }
    public class RenderOptionsCurves : RenderOptions
    {
        /// <summary>
        /// false - авто цвет
        /// false - черный цвет
        /// </summary>
        public bool opAutoColorCurves = false;
        /// <summary>
        /// авто масштаб кривых по Х
        /// false - авто масштаб
        /// true - нет авто масштаба
        /// </summary>
        public bool opAutoScaleX = false;
        /// <summary>
        /// авто масштаб кривых по Y
        /// false - авто масштаб
        /// true - нет авто масштаба
        /// </summary>
        public bool opAutoScaleY = true;
        /// <summary>
        /// собственный масштаб кривых
        /// true - общий масштаб
        /// false - свой масштаб
        /// </summary>
        //public bool opOwnScaleCurves = true;
        /// <summary>
        /// Автомасштаб 0 - нет, 1 - |max| 3 - задан
        /// </summary>
        public int opCurveScale = 1;
        /// <summary>
        /// Фиксация масштаба
        /// </summary>
        public bool opFixingScale = false;
        /// <summary>
        /// Масштаб задаваемый пользователем
        /// </summary>
        public double opScaleValue = 1;
        /// <summary>
        /// Масштаб по Х
        /// </summary>
        public double opFixingScaleX = 1;
        /// <summary>
        /// Масштаб по Y
        /// </summary>
        public double opFixingScaleY = 1;

        public RenderOptionsCurves() { }
        public RenderOptionsCurves(RenderOptionsCurves o) : base(o)
        {
            opAutoScaleX = o.opAutoScaleX;
            opAutoScaleY = o.opAutoScaleY;
            opCurveScale = o.opCurveScale;
            opFixingScaleX = o.opFixingScaleX;
            opFixingScaleY = o.opFixingScaleY;
            opScaleValue = o.opScaleValue;
            opAutoColorCurves = o.opAutoColorCurves;
        }
    }

    public class RenderOptionsFields : RenderOptions
    {
        /// <summary>
        /// Учет кривых при расчетах маштаба
        /// </summary>
        public bool ckAccountingСurves = false;
        /// <summary>
        /// Отрисовка границ по ГКЭ
        /// </summary>
        public bool showBoudary = true;
        /// <summary>
        /// Отрисовка границ по граничным узлам
        /// </summary>
        public bool showBoudaryKnots = false;
        /// <summary>
        /// Отрисовка границ по граничным элементам
        /// </summary>
        public bool showBoudaryElems = false;
        /// <summary>
        /// Орисовка номеров конечных элементов
        /// </summary>
        public bool showElementNamber = false;
        /// <summary>
        /// Заливка для поля
        /// </summary>
        public bool opFillValues = true;
        /// <summary>
        ///  15 06 2024 шкала для градиентной заливки
        /// </summary>
        public bool opGradScale = false;
        /// <summary>
        /// Выбор пределов шкалы/изолиний
        /// </summary>
        public bool cb_GradScaleLimit = false;
        /// <summary>
        /// Пределы шкалы
        /// </summary>
        public float MaxValue,MinValue = 0;
        /// <summary>
        /// Изолинии для поля
        /// </summary>
        public bool opIsoLineValues = false;
        /// <summary>
        /// Изолинии для поля
        /// </summary>
        public bool opIsoLineValues0 = false;
        /// <summary>
        /// Отрисовка значений изолинии 
        /// </summary>
        public bool opIsoLineValuesShow = false;
        /// <summary>
        /// Отрисовка изолинии с заданным занчением 
        /// </summary>
        public bool opIsoLineSelect = false;
        /// <summary>
        /// занчение заданной изолинии 
        /// </summary>
        public float opIsoLineSelectValue = 0;
        /// <summary>
        /// Векторное поле в узлах
        /// </summary>
        public bool opVectorValues = false;
        /// <summary>
        /// Независимые графики 
        /// </summary>
        public bool opGraphicCurve = true;
        /// <summary>
        /// Первая координата створа
        /// </summary>
        public PointF a = new PointF();
        /// <summary>
        /// Вторая координата створа
        /// </summary>
        public PointF b = new PointF();
        /// <summary>
        /// Отрисовка створа
        /// </summary>
        public bool opTargetLine = true;


        public RenderOptionsFields() : base() { }
        public RenderOptionsFields(RenderOptionsFields o) : base(o)
        {
            showBoudary = o.showBoudary;
            showBoudaryKnots = o.showBoudaryKnots;
            showElementNamber = o.showElementNamber;
            opFillValues = o.opFillValues;
            opIsoLineValues = o.opIsoLineValues;
            opIsoLineValuesShow = o.opIsoLineValuesShow;
            opIsoLineSelect = o.opIsoLineSelect;
            opIsoLineSelectValue = o.opIsoLineSelectValue;
            opIsoLineValues0 = o.opIsoLineValues0;
            opVectorValues = o.opVectorValues;
            opGraphicCurve = o.opGraphicCurve;
            opGradScale = o.opGradScale;
            cb_GradScaleLimit = o.cb_GradScaleLimit;
            MaxValue = o.MaxValue;
            MinValue = o.MinValue;
            a = o.a;
            b = o.b;
            opTargetLine = o.opTargetLine;
        }
    }

    public class RenderOptionsEdit : RenderOptions
    {
        /// <summary>
        /// Учет кривых при расчетах маштаба
        /// </summary>
       // public bool ckAccountingСurves = false;
        /// <summary>
        /// Отрисовка границ по ГКЭ
        /// </summary>
        public bool showBoudary = true;
        /// <summary>
        /// Отрисовка границ по граничным узлам
        /// </summary>
        public bool showBoudaryKnots = false;
        /// <summary>
        /// Отрисовка границ по граничным элементам
        /// </summary>
        public bool showBoudaryElems = false;
        /// <summary>
        /// Орисовка номеров конечных элементов
        /// </summary>
        public bool showElementNamber = false;
        /// <summary>
        /// Заливка для поля
        /// </summary>
        public bool opFillValues = true;
        /// <summary>
        ///  15 06 2024 шкала для градиентной заливки
        /// </summary>
        public bool opGradScale = false;
        /// <summary>
        /// Изолинии для поля
        /// </summary>
       // public bool opIsoLineValues = false;
        /// <summary>
        /// Изолинии для поля
        /// </summary>
       // public bool opIsoLineValues0 = false;
        /// <summary>
        /// Отрисовка значений изолинии 
        /// </summary>
      //  public bool opIsoLineValuesShow = false;
        /// <summary>
        /// Отрисовка изолинии с заданным занчением 
        /// </summary>
      //  public bool opIsoLineSelect = false;
        /// <summary>
        /// занчение заданной изолинии 
        /// </summary>
     //   public float opIsoLineSelectValue = 0;
        /// <summary>
        /// Векторное поле в узлах
        /// </summary>
     //   public bool opVectorValues = false;
        /// <summary>
        /// Независимые графики 
        /// </summary>
       // public bool opGraphicCurve = true;
        /// <summary>
        /// Первая координата створа
        /// </summary>
        //public PointF a = new PointF();
        ///// <summary>
        ///// Вторая координата створа
        ///// </summary>
        //public PointF b = new PointF();
        /// <summary>
        /// Отрисовка створа
        /// </summary>
    //    public bool opTargetLine = true;


        public RenderOptionsEdit() : base() { }
        public RenderOptionsEdit(RenderOptionsEdit o) : base(o)
        {
            showBoudary = o.showBoudary;
            showBoudaryKnots = o.showBoudaryKnots;
            showElementNamber = o.showElementNamber;
            opFillValues = o.opFillValues;
            //opIsoLineValues = o.opIsoLineValues;
            //opIsoLineValuesShow = o.opIsoLineValuesShow;
            //opIsoLineSelect = o.opIsoLineSelect;
            //opIsoLineSelectValue = o.opIsoLineSelectValue;
            //opIsoLineValues0 = o.opIsoLineValues0;
            //opVectorValues = o.opVectorValues;
            //opGraphicCurve = o.opGraphicCurve;
            opGradScale = o.opGradScale;
            //a = o.a;
            //b = o.b;
            //opTargetLine = o.opTargetLine;
        }
    }

}
