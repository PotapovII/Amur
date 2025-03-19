//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 10.12.2020 Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 20.12.2023 Потапов И.И.
// + BaseColorScheme ...
// + ColorSchemeClouds ...
//---------------------------------------------------------------------------
namespace RenderLib
{
    using CommonLib.Delegate;
    using System;
    using System.Drawing;
    /// <summary>
    /// ОО: цветовая схемы задачи и опции для орисовки кривых
    /// </summary>
    public class BaseColorScheme
    {
        const int RGBMAX = 255;
        /// <summary>
        /// Инвертор цвета
        /// </summary>
        /// <param name="ColourToInvert"></param>
        /// <returns></returns>
        public static Color InvertColour1(Color ColourToInvert)
        {
            return Color.FromArgb(RGBMAX - ColourToInvert.R, RGBMAX - ColourToInvert.G, RGBMAX - ColourToInvert.B);
        }
        public static Color InvertColour(Color bg)
        {
            //const rgb = parseInt(bgColor.slice(1), 16);
            //const r = (rgb >> 16) & 0xff;
            //const g = (rgb >> 8) & 0xff;
            //const b = (rgb >> 0) & 0xff;
            //return ((r * 0.299 + g * 0.587 + b * 0.114) > 186) ? '#000000' : '#FFFFFF';
            return Color.FromArgb((int)(bg.R * 0.299), (int)(bg.G * 0.587),(int) (bg.B * 0.114));
        }


        #region Кисти
        /// <summary>
        /// Кисти для отрисовки значений у узлах координат
        /// </summary>
        protected SolidBrush brushTextReper;
        /// <summary>
        /// Кисти для узлов координат
        /// </summary>
        protected SolidBrush brushReper;
        /// <summary>
        /// Цвет поля выода
        /// </summary>
        protected Color background;
        #endregion
        #region Шрифты
        /// <summary>
        /// Шрифт для вывода значений координатных осей
        /// </summary>
        protected Font fontReper;
        #endregion
        #region Перья
        /// <summary>
        /// Перо для отрисовки координатных осей
        /// </summary>
        protected Pen penReper;
        #endregion
        /// <summary>
        /// Разраядность мантисы при выводе числовых данных
        /// </summary>
        public uint formatTextReper { get; set; }
        /// <summary>
        /// Масштаб отображаемых координат
        /// </summary>
        public int scaleCoords = 0;
        /// <summary>
        /// Кисти для узлов координат
        /// </summary>
        public SolidBrush BrushReper
        {
            get { return brushReper; }
            set
            {
                if (brushReper != null) brushReper.Dispose();
                brushReper = value;
            }
        }
        /// <summary>
        /// Кисти для отрисовки значений у узлах координат
        /// </summary>
        public SolidBrush BrushTextReper
        {
            get { return brushTextReper; }
            set
            {
                if (brushTextReper != null) brushTextReper.Dispose();
                brushTextReper = value;
            }
        }
        /// <summary>
        /// Цвет фон для контрола
        /// </summary>
        public Color Background
        {
            get { return background; }
            set { background = value; }
        }
        /// <summary>
        /// Перо для отрисовки координатных осей
        /// </summary>
        public Pen PenReper
        {
            get { return penReper; }
            set
            {
                if (penReper != null) penReper.Dispose();
                penReper = value;
            }
        }
        /// <summary>
        /// Шрифт для вывода значений координатных осей
        /// </summary>
        public Font FontReper
        {
            get { return fontReper; }
            set
            {
                if (fontReper != null) fontReper.Dispose();
                fontReper = value;
            }
        }
        /// <summary>
        /// Разраядность мантисы при выводе числовых данных
        /// </summary>
        public string FormatTextReper
        {
            get
            {
                formatTextReper = formatTextReper < 14 ? formatTextReper : 14;
                return "{0:f" + formatTextReper.ToString() + "}";
            }
        }
        public BaseColorScheme()
        {
            Background = Color.White;
            //
            brushTextReper = new SolidBrush(Color.Blue);
            brushReper = new SolidBrush(Color.Green);
            penReper = new Pen(Color.Blue, 1);
            fontReper = new Font("Arial", 14);
            formatTextReper = 3;
            scaleCoords = 0;
        }
        /// <summary>
        /// // 15 07 24 изменяемый масштаб координатных осей
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public float ScaleValue(float value)
        {
            return value * (float)Math.Pow(10, scaleCoords);
        }

        public BaseColorScheme(BaseColorScheme p)
        {
            Background = p.Background;
            BrushReper = p.BrushReper;
            BrushTextReper = p.BrushTextReper;
            PenReper = p.PenReper;
            FontReper = p.FontReper;
            formatTextReper = p.formatTextReper;
        }
        #region Публичные методы
        /// <summary>
        ///  Получить сцет из списка Count цветов
        /// </summary>
        /// <param name="index">индекс цвета</param>
        /// <param name="Count">количество цветов</param>
        /// <param name="width">ширина пера</param>
        /// <returns></returns>
        public Pen GetPenGraphLine(int index, int init, int Count, int width = 1)
        {
            Color color = RGBBrush(index % Count, init, Count);
            return new Pen(color, width);
        }
        /// <summary>
        /// Получить цвет 
        /// </summary>
        /// <param name="arg">аргумент</param>
        /// <param name="PType"></param>
        /// <param name="MinV">минимальная величина аргумента</param>
        /// <param name="MaxV">максимальная величина аргумента</param>
        /// <returns></returns>
        public Color RGBBrush(double arg, double MinV, double MaxV)
        {
            Color col;
            int R, G, B;
            try
            {
                // Определение цвета
                int Base = 1;
                double MaxColors = 255 - Base;
                double mod = MaxV - MinV;
                if (mod < 1e-12) mod = 1;
                double _arg = 3.7 * (arg - MinV) / mod;
                _arg = 3.7 - _arg;
                int level = (int)(_arg);
                double value = _arg - level;
                int Arg = (int)(value * MaxColors);
                if (arg > MaxV) level = 4;
                switch (level)
                {
                    case -1:
                        B = 0;
                        R = 0;
                        G = 0;
                        break;
                    case 0:
                        R = 255;
                        G = Base + Arg;
                        B = Base;
                        break;
                    case 1:
                        R = 255 - Arg;
                        G = 255;
                        B = Base;
                        break;
                    case 2:
                        R = Base;
                        G = 255;
                        B = Base + Arg;
                        break;
                    case 3:
                        R = Base;
                        G = 255 - Arg;
                        B = 255;
                        //if(G<0) G=0;
                        break;
                    case 4:
                        B = 255;
                        R = 255;
                        G = 255;
                        break;
                    default:
                        B = 255;
                        R = 255;
                        G = 255;
                        break;
                }
            }
            catch
            {
                B = 125;
                R = 125;
                G = 125;
            }
            col = Color.FromArgb(R, G, B);
            return col;
        }
        #endregion
    }
    /// <summary>
    /// ОО: цветовая схемы задачи и опции для орисовки кривых
    /// </summary>
    public class ColorScheme : BaseColorScheme
    {
        #region Кисти
        /// <summary>
        /// Кисти для узлов сетки/кривых
        /// </summary>
        protected SolidBrush brushPoint;
        /// <summary>
        /// Кисти для отрисовки значений у узлах сетки/кривых
        /// </summary>
        protected SolidBrush brushTextValues;
        #endregion
        #region Шрифты
        ///// <summary>
        ///// Шрифт для вывода значений координатных осей
        ///// </summary>
        //protected Font fontCoord;
        /// <summary>
        /// Шрифт для вывода значений в узлах сетки/кривых
        /// </summary>
        protected Font fontValue;
        #endregion
        #region Перья
        /// <summary>
        /// Перо для отрисовки линий кривых
        /// </summary>
        protected Pen penGraphLine;
        /// <summary>
        /// Перо для отрисовки линий сетки
        /// </summary>
        protected Pen penMeshLine;
        #endregion

        /// <summary>
        /// Разраядность мантисы при выводе числовых данных
        /// </summary>
        public uint formatText;

        public SolidBrush BrushPoint
        {
            get { return brushPoint; }
            set
            {
                if (brushPoint != null) brushPoint.Dispose();
                brushPoint = value;
            }
        }
        public SolidBrush BrushTextValues
        {
            get { return brushTextValues; }
            set
            {
                if (brushTextValues != null) brushTextValues.Dispose();
                brushTextValues = value;
            }
        }

        public Pen PenGraphLine
        {
            get { return penGraphLine; }
            set
            {
                if (penGraphLine != null) penGraphLine.Dispose();
                penGraphLine = value;
            }
        }
        public Pen PenMeshLine
        {
            get { return penMeshLine; }
            set
            {
                if (penMeshLine != null) penMeshLine.Dispose();
                penMeshLine = value;
            }
        }
        public Font FontValue
        {
            get { return fontValue; }
            set
            {
                if (fontValue != null) fontValue.Dispose();
                fontValue = value;
            }
        }
        /// <summary>
        /// Формат 
        /// </summary>
        public string FormatText
        {
            get
            {
                formatText = formatText < 8 ? formatText : 8;
                return "{0:f" + formatText.ToString() + "}";
            }
        }
        public ColorScheme() : base()
        {
            background = Color.White;
            brushPoint = new SolidBrush(Color.Green);
            brushTextValues = new SolidBrush(Color.Blue);
            penGraphLine = new Pen(Color.Black);
            penMeshLine = new Pen(Color.Black, 2);
            FontReper = new Font("Arial", 14);
            fontValue = new Font("Arial", 12);
            formatText = 3;
        }
        public ColorScheme(ColorScheme p) : base(p)
        {
            BrushPoint = p.brushPoint;
            BrushTextValues = p.brushTextValues;
            PenGraphLine = p.penGraphLine;
            PenMeshLine = p.penMeshLine;
            FontValue = p.fontValue;
            formatText = p.formatText;
        }
    }
    /// <summary>
    /// ОО: цветовая схемы задачи и опции для орисовки облака
    /// и контуравми границ области
    /// </summary>
    public class ColorSchemeClouds : BaseColorScheme
    {
        /// <summary>
        /// Разраядность мантисы при выводе числовых данных
        /// </summary>
        public uint formatTextValues;
        /// <summary>
        /// Разраядность мантисы при выводе числовых данных
        /// </summary>
        public string FormatTextValues
        {
            get
            {
                formatTextValues = formatTextValues < 8 ? formatTextValues : 8;
                return "{0:f" + formatTextValues.ToString() + "}";
            }
        }
        #region Кисти
        /// <summary>
        /// Кисти для отрисовки узлов облака 
        /// </summary>
        protected SolidBrush brushNodes;
        /// <summary>
        /// Кисти для отрисовки номерации узлов облака  
        /// </summary>
        protected SolidBrush brushTextNodes;
        /// <summary>
        /// Кисти для отрисовки значений полей облака в узлах 
        /// </summary>
        protected SolidBrush brushTextValues;
        /// <summary>
        /// Кисти для отрисовки узлов контура в фокусе
        /// </summary>
        protected SolidBrush brushNodeFocusContur;
        /// <summary>
        /// Кисти для отрисовки узлов контура НЕ в фокусе
        /// </summary>
        protected SolidBrush brushNodeContur;
        #endregion

        #region Шрифты
        /// <summary>
        /// Шрифт для вывода значений в узлах облака
        /// </summary>
        protected Font fontNodesCloud;
        #endregion

        #region Перья
        /// <summary>
        /// Перо для отрисовки контура области в фокусе
        /// </summary>
        protected Pen penAreaFocusLine;
        /// <summary>
        /// Перо для отрисовки контура области 
        /// </summary>
        protected Pen penAreaLine;
        /// <summary>
        /// Перо для отрисовки сегмента контура в фокусе
        /// </summary>
        protected Pen penSegmentFocusLine;
        /// <summary>
        /// Перо для отрисовки сегмента контура 
        /// </summary>
        protected Pen penSegmentLine;
        /// <summary>
        /// Перо для отрисовки контурная линия  
        /// </summary>
        protected Pen penCounturLine;
        /// <summary>
        /// Перо для отрисовки контурная линия  
        /// </summary>
        protected Pen penSelectCounturLine;
        /// <summary>
        /// Перо для отрисовки контурная линия  
        /// </summary>
        protected Pen penLinkCounturLine;
        /// <summary>
        /// Перо для отрисовки контурная линия  
        /// </summary>
        protected Pen penSelectLinkCounturLine;
        /// <summary>
        /// Перо для отрисовки векторных полей облака
        /// </summary>
        protected Pen penVectorLine;
        /// <summary>
        /// Перо для отрисовки сегмента для входящего потока 
        /// </summary>
        protected Pen penInSegmentLine;
        /// <summary>
        /// Перо для отрисовки сегмента для исходящего потока
        /// </summary>
        protected Pen penOutSegmentLine;
        #endregion



        public ColorSchemeClouds() : base()
        {
            brushNodes = new SolidBrush(Color.Green);
            brushTextNodes = new SolidBrush(Color.Blue);
            brushTextValues = new SolidBrush(Color.Blue);
            brushNodeFocusContur = new SolidBrush(Color.Red);
            brushNodeContur = new SolidBrush(Color.Gray);

            fontNodesCloud = new Font("Arial", 8);

            penAreaLine = new Pen(Color.LightGray);
            penSegmentLine = new Pen(Color.LightGray);

            penAreaFocusLine = new Pen(Color.DarkGreen, 2);
            penSegmentFocusLine = new Pen(Color.DarkGreen, 2);
            penInSegmentLine = new Pen(Color.DarkBlue, 2);
            penOutSegmentLine = new Pen(Color.DarkOliveGreen, 2);

            penCounturLine = new Pen(Color.LightGray);
            penSelectCounturLine = new Pen(Color.DarkGray, 2);
            
            penLinkCounturLine = new Pen(Color.Red);
            penSelectLinkCounturLine = new Pen(Color.DarkRed);

            penVectorLine = new Pen(Color.DarkRed);

            

            formatTextValues = 2;
        }

        public ColorSchemeClouds(ColorSchemeClouds p) : base(p)
        {
            brushNodes = p.brushNodes;
            brushTextNodes = p.brushTextNodes;
            brushTextValues = p.brushTextValues;
            brushNodeFocusContur = p.brushNodeFocusContur;
            brushNodeContur = p.brushNodeContur;

            fontNodesCloud = p.fontNodesCloud;

            penAreaLine = p.penAreaLine;
            penSegmentLine = p.penSegmentLine;
            penInSegmentLine = p.penInSegmentLine;
            penOutSegmentLine = p.penOutSegmentLine;

            penAreaFocusLine = p.penAreaFocusLine;
            penSegmentFocusLine = p.penSegmentFocusLine;

            penCounturLine = p.penCounturLine;
            penSelectCounturLine = p.penSelectCounturLine;
            penLinkCounturLine = p.penLinkCounturLine;
            penSelectLinkCounturLine = p.penSelectLinkCounturLine;

            penVectorLine = p.penVectorLine;

            formatTextValues = p.formatTextValues;
        }

        #region Кисти
        /// <summary>
        /// Кисти для отрисовки узлов облака 
        /// </summary>
        public SolidBrush BrushNodes
        {
            get { return brushNodes; }
            set
            {
                if (brushNodes != null) brushNodes.Dispose();
                brushNodes = value;
            }
        }
        /// <summary>
        /// Кисти для отрисовки номерации узлов облака  
        /// </summary>
        public SolidBrush BrushTextNodes
        {
            get { return brushTextNodes; }
            set
            {
                if (brushTextNodes != null) brushTextNodes.Dispose();
                brushTextNodes = value;
            }
        }
        /// <summary>
        /// Кисти для отрисовки значений полей облака в узлах 
        /// </summary>
        public SolidBrush BrushTextValues
        {
            get { return brushTextValues; }
            set
            {
                if (brushTextValues != null) brushTextValues.Dispose();
                brushTextValues = value;
            }
        }
        /// <summary>
        /// Кисти для отрисовки узлов контура в фокусе
        /// </summary>
        public SolidBrush BrushNodeFocusContur
        {
            get { return brushNodeFocusContur; }
            set
            {
                if (brushNodeFocusContur != null) brushNodeFocusContur.Dispose();
                brushNodeFocusContur = value;
            }
        }
        /// <summary>
        /// Кисти для отрисовки узлов контура НЕ в фокусе
        /// </summary>
        public SolidBrush BrushNodeContur
        {
            get { return brushNodeContur; }
            set
            {
                if (brushNodeContur != null) brushNodeContur.Dispose();
                brushNodeContur = value;
            }
        }
        #endregion
        #region Шрифты
        /// <summary>
        /// Шрифт для вывода значений в узлах
        /// </summary>
        public Font FontNodesCloud
        {
            get { return fontNodesCloud; }
            set
            {
                if (fontNodesCloud != null) fontNodesCloud.Dispose();
                fontNodesCloud = value;
            }
        }
        #endregion

        #region Перья
        /// <summary>
        /// Перо для отрисовки контура области в фокусе
        /// </summary>
        public Pen PenAreaFocusLine
        {
            get { return penAreaFocusLine; }
            set
            {
                if (penAreaFocusLine != null) penAreaFocusLine.Dispose();
                penAreaFocusLine = value;
            }
        }
        /// <summary>
        /// Перо для отрисовки контура области 
        /// </summary>
        public Pen PenAreaLine
        {
            get { return penAreaLine; }
            set
            {
                if (penAreaLine != null) penAreaLine.Dispose();
                penAreaLine = value;
            }
        }
        /// <summary>
        /// Перо для отрисовки сегмента контура в фокусе
        /// </summary>
        public Pen PenSegmentFocusLine
        {
            get { return penSegmentFocusLine; }
            set
            {
                if (penSegmentFocusLine != null) penSegmentFocusLine.Dispose();
                penSegmentFocusLine = value;
            }
        }
        /// <summary>
        /// Перо для отрисовки сегмента контура
        /// </summary>
        public Pen PenSegmentLine
        {
            get { return penSegmentLine; }
            set
            {
                if (penSegmentLine != null) penSegmentLine.Dispose();
                penSegmentLine = value;
            }
        }
        /// <summary>
        /// Контурная линия  
        /// </summary>
        public Pen PenCounturLine
        {
            get { return penCounturLine; }
            set
            {
                if (penCounturLine != null) penCounturLine.Dispose();
                penCounturLine = value;
            }
        }
        /// <summary>
        /// Выделенная контурная линия  
        /// </summary>
        public Pen PenSelectCounturLine
        {
            get { return penSelectCounturLine; }
            set
            {
                if (penSelectCounturLine != null) penSelectCounturLine.Dispose();
                penSelectCounturLine = value;
            }
        }
        public Pen PenLinkCounturLine
        {
            get { return penLinkCounturLine; }
            set
            {
                if (penLinkCounturLine != null) penLinkCounturLine.Dispose();
                penLinkCounturLine = value;
            }
        }
        public Pen PenSelectLinkCounturLine
        {
            get { return penSelectLinkCounturLine; }
            set
            {
                if (penSelectLinkCounturLine != null) penSelectLinkCounturLine.Dispose();
                penSelectLinkCounturLine = value;
            }
        }
        
        /// <summary>
        /// Вектор линия для полей облака
        /// </summary>
        public Pen PenVectorLine
        {
            get { return penVectorLine; }
            set
            {
                if (penVectorLine != null) penVectorLine.Dispose();
                penVectorLine = value;
            }
        }

        /// <summary>
        /// Перо для отрисовки сегмента для входящего потока 
        /// </summary>
        public Pen PenInSegmentLine
        {
            get { return penInSegmentLine; }
            set
            {
                if (penInSegmentLine != null) penInSegmentLine.Dispose();
                penInSegmentLine = value;
            }
        }
        /// <summary>
        /// Перо для отрисовки сегмента для исходящего потока
        /// </summary>
        public Pen PenOutSegmentLine
        {
            get { return penOutSegmentLine; }
            set
            {
                if (penOutSegmentLine != null) penOutSegmentLine.Dispose();
                penOutSegmentLine = value;
            }
        }
        #endregion
    }
    
    /// <summary>
    /// ОО: цветовая схемы задачи и опции для орисовки полей
    /// </summary>
    public class ColorSchemeFields : ColorScheme
    {
        /// <summary>
        /// Кисти для граничных узлов сетки/кривых
        /// </summary>
        protected SolidBrush brushBoundaryPoint;
        /// <summary>
        /// Кисти для вывода текста
        /// </summary>
        protected SolidBrush brushTextKnot;
        /// <summary>
        /// Шрифт для вывода значений в узлах
        /// </summary>
        protected Font fontKnot;

        protected Pen penBoundaryLine;
        protected Pen penIsoLine;
        protected Pen penVectorLine;

        public int CountIsoLine;
        public int MinIsoLine;
        public int MaxIsoLine;

        public ColorSchemeFields() : base()
        {
            brushBoundaryPoint = new SolidBrush(Color.Peru);
            brushTextKnot = new SolidBrush(Color.Black);

            penBoundaryLine = new Pen(Color.DarkGray, 2);
            penIsoLine = new Pen(Color.DarkGreen, 2);
            penVectorLine = new Pen(Color.DarkRed);
            penMeshLine = new Pen(Color.LightGray);

            fontKnot = new Font("Arial", 8);
            fontValue = new Font("Arial", 16);
            CountIsoLine = 10;
            MinIsoLine = 0;
            MaxIsoLine = 100;
        }
        #region Публичные свойства
        public PointF MinMaxIsoLine
        {
            get
            {
                return new PointF(MinIsoLine / 100f, MaxIsoLine / 100f);
            }
        }

        public Pen PenIsoLine
        {
            get { return penIsoLine; }
            set
            {
                if (penIsoLine != null) penIsoLine.Dispose();
                penIsoLine = value;
            }
        }
        public Pen PenVectorLine
        {
            get { return penVectorLine; }
            set
            {
                if (penVectorLine != null) penVectorLine.Dispose();
                penVectorLine = value;
            }
        }
        public Pen PenBoundaryLine
        {
            get { return penBoundaryLine; }
            set
            {
                if (penBoundaryLine != null) penBoundaryLine.Dispose();
                penBoundaryLine = value;
            }
        }

        public SolidBrush BrushBoundaryPoint
        {
            get { return brushBoundaryPoint; }
            set
            {
                if (brushBoundaryPoint != null) brushBoundaryPoint.Dispose();
                brushBoundaryPoint = value;
            }
        }
        public SolidBrush BrushTextKnot
        {
            get { return brushTextKnot; }
            set
            {
                if (brushTextKnot != null) brushTextKnot.Dispose();
                brushTextKnot = value;
            }
        }

        public Font FontKnot
        {
            get { return fontKnot; }
            set
            {
                if (fontKnot != null) fontKnot.Dispose();
                fontKnot = value;
            }
        }
        #endregion
    }
    /// <summary>
    /// ОО: цветовая схемы задачи и опции для орисовки полей
    /// </summary>
    public class ColorSchemeEdit : ColorScheme
    {
        /// <summary>
        /// Кисти для граничных узлов сетки/кривых
        /// </summary>
        protected SolidBrush brushBoundaryPoint;
        /// <summary>
        /// Кисти для вывода текста
        /// </summary>
        protected SolidBrush brushTextKnot;
        /// <summary>
        /// Шрифт для вывода значений в узлах
        /// </summary>
        protected Font fontKnot;
        
        protected Pen penBoundaryLine;
        protected Pen penIsoLine;
        protected Pen penVectorLine;
        //public int CountIsoLine;
        //public int MinIsoLine;
        //public int MaxIsoLine;

        public ColorSchemeEdit() : base()
        {
            brushBoundaryPoint = new SolidBrush(Color.Peru);
            brushTextKnot = new SolidBrush(Color.Black);

            penBoundaryLine = new Pen(Color.DarkGray, 2);
            penIsoLine = new Pen(Color.DarkGreen, 2);
            penVectorLine = new Pen(Color.DarkRed);
            penMeshLine = new Pen(Color.LightGray);

            fontKnot = new Font("Arial", 8);
            fontValue = new Font("Arial", 16);
            //CountIsoLine = 10;
            //MinIsoLine = 0;
            //MaxIsoLine = 100;
        }
        #region Публичные свойства
        //public PointF MinMaxIsoLine
        //{
        //    get
        //    {
        //        return new PointF(MinIsoLine / 100f, MaxIsoLine / 100f);
        //    }
        //}
        public Pen PenIsoLine
        {
            get { return penIsoLine; }
            set
            {
                if (penIsoLine != null) penIsoLine.Dispose();
                penIsoLine = value;
            }
        }
        public Pen PenVectorLine
        {
            get { return penVectorLine; }
            set
            {
                if (penVectorLine != null) penVectorLine.Dispose();
                penVectorLine = value;
            }
        }
        public Pen PenBoundaryLine
        {
            get { return penBoundaryLine; }
            set
            {
                if (penBoundaryLine != null) penBoundaryLine.Dispose();
                penBoundaryLine = value;
            }
        }
        public SolidBrush BrushBoundaryPoint
        {
            get { return brushBoundaryPoint; }
            set
            {
                if (brushBoundaryPoint != null) brushBoundaryPoint.Dispose();
                brushBoundaryPoint = value;
            }
        }
        public SolidBrush BrushTextKnot
        {
            get { return brushTextKnot; }
            set
            {
                if (brushTextKnot != null) brushTextKnot.Dispose();
                brushTextKnot = value;
            }
        }
        public Font FontKnot
        {
            get { return fontKnot; }
            set
            {
                if (fontKnot != null) fontKnot.Dispose();
                fontKnot = value;
            }
        }
        #endregion
    }
}
