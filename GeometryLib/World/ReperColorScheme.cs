//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 01.02.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace GeometryLib.World
{
    using System.Drawing;
    /// <summary>
    /// ОО: цветовая схемы задачи и опции для орисовки кривых
    /// </summary>
    public class ReperColorScheme
    {
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
        public Color Background { get; set; }
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
                formatTextReper = formatTextReper < 8 ? formatTextReper : 8;
                return "{0:f" + formatTextReper.ToString() + "}";
            }
        }
        public ReperColorScheme()
        {
            Background = Color.White;
            //
            brushTextReper = new SolidBrush(Color.Blue);
            brushReper = new SolidBrush(Color.Green);
            penReper = new Pen(Color.Blue, 1);
            fontReper = new Font("Arial", 8);
            formatTextReper = 2;
        }
        public ReperColorScheme(ReperColorScheme p)
        {
            Background = p.Background;
            brushReper = p.brushReper;
            brushTextReper = p.brushTextReper;
            penReper = p.penReper;
            fontReper = p.fontReper;
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

}
