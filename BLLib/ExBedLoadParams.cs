//---------------------------------------------------------------------------
//                 Реализация библиотеки для моделирования 
//                  гидродинамических и русловых процессов
//---------------------------------------------------------------------------
//                Модуль BLLib для расчета донных деформаций 
//                 (учет движения только влекомых наносов)
//         по русловой модели Петрова А.Г. и Потапова И.И. от 2014 г.
//                              Потапов И.И.
//                               07.08.21
//---------------------------------------------------------------------------
namespace BLLib
{
    using MemLogLib;
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;

    /// <summary>
    /// Расширение параметров задачи.  
    /// Решение задачи ищем в прямоугольной области, на регулярной прямоугольной сетке 
    /// </summary>
    [Serializable]
    public class ExBedLoadParams : BedLoadParams
    {
        [DisplayName("Код задачи")]
        [Category("Задача")]
        public int TaskIndex { get; set; }
        [DisplayName("Количество узлов по Х в прямоугольной области")]
        [Category("Сетка")]
        public int Nx { get; set; }
        [DisplayName("Количество узлов по Y в прямоугольной области")]
        [Category("Сетка")]
        public int Ny { get; set; }
        [DisplayName("Шаг сетки по Х в прямоугольной области")]
        [Category("Сетка")]
        public double dx { get; set; }
        [DisplayName("Шаг сетки по Y в прямоугольной области")]
        [Category("Сетка")]
        public double dy { get; set; }
        [DisplayName("Количество узлов по Y в прямоугольной области")]
        [Category("Сетка")]
        public int xStart { get; set; }
        [DisplayName("Количество узлов по Y в прямоугольной области")]
        [Category("Сетка")]
        public int yStart { get; set; }
        [DisplayName("Имя файла для загрузки tauX")]
        [Category("Обмен")]
        public string FNameTauX { get; set; }
        [DisplayName("Имя файла для загрузки tauX")]
        [Category("Обмен")]
        public string FNameTauY { get; set; }
        [DisplayName("Имя файла для загрузки P")]
        [Category("Обмен")]
        public string FNamePress { get; set; }
        [DisplayName("Имя файла для выгрузки zeta")]
        [Category("Обмен")]
        public string FNameZeta { get; set; }
        [DisplayName("Имя файла для выгрузки x")]
        [Category("Обмен")]
        public string FNameX { get; set; }
        [DisplayName("Имя файла для выгрузки y")]
        [Category("Обмен")]
        public string FNameY { get; set; }
        /// <summary>
        /// флаг наличия расширения
        /// </summary>
        [DisplayName("флаг наличия расширения формата")]
        [Category("Задача")]
        public bool IsExtension { get; set; }

        public ExBedLoadParams() : base()
        {
            TaskIndex = 0;
            Nx = 10;
            Ny = 20;
            dx = 0.01;
            dy = 0.01;
            //xStart = 0;
            //yStart = 0;
            FNameTauX = "";
            FNameTauY = "";
            FNamePress = "";
            FNameZeta = "";
            FNameX = "";
            FNameY = "";
            IsExtension = false;
        }

        /// <summary>
        /// Конструктор копирования
        /// </summary>
        /// <param name="p"></param>
        public ExBedLoadParams(ExBedLoadParams p)
        {
            base.SetParams(p);
            SetParams(p);
        }
        /// <summary>
        /// Установка параметров
        /// </summary>
        /// <param name="p"></param>
        public void SetParams(ExBedLoadParams p)
        {
            TaskIndex = p.TaskIndex;
            Nx = p.Nx;
            Ny = p.Ny;
            dx = p.dx;
            dy = p.dy;

            FNameTauX = p.FNameTauX;
            FNameTauY = p.FNameTauY;
            FNamePress = p.FNamePress;

            FNameZeta = p.FNameZeta;
            FNameX = p.FNameX;
            FNameY = p.FNameY;

            IsExtension = p.IsExtension;
        }
        /// <summary>
        /// Чтение параметров задачи из файла
        /// </summary>
        /// <param name="file"></param>
        public override void Load(StreamReader file)
        {
            base.Load(file);

            NumberFormatInfo formatter = new NumberFormatInfo { NumberDecimalSeparator = "." };
            string line;
            string[] sline;
            if (file != null)
                Logger.Instance.Info("Файл параметров открыт");
            else
            {
                Logger.Instance.Info("Файл параметров не найден");
                return;
            }
            line = file.ReadLine();
            if (line == null)
            {
                IsExtension = false; return;
            }

            sline = line.Split(' ');
            this.TaskIndex = int.Parse(sline[1].Trim());
            Logger.Instance.Info("обработана строка " + line);

            sline = line.Split(' ');
            this.Nx = int.Parse(sline[1].Trim());
            Logger.Instance.Info("обработана строка " + line);

            line = file.ReadLine();
            sline = line.Split(' ');
            this.Ny = int.Parse(sline[1].Trim());
            Logger.Instance.Info("обработана строка " + line);

            line = file.ReadLine();
            sline = line.Split(' ');
            this.dx = double.Parse(sline[1].Trim(), formatter);
            Logger.Instance.Info("обработана строка " + line);

            line = file.ReadLine();
            sline = line.Split(' ');
            this.dy = double.Parse(sline[1].Trim(), formatter);
            Logger.Instance.Info("обработана строка " + line);

            line = file.ReadLine();
            sline = line.Split(' ');
            this.xStart = int.Parse(sline[1].Trim());
            Logger.Instance.Info("обработана строка " + line);

            line = file.ReadLine();
            sline = line.Split(' ');
            this.yStart = int.Parse(sline[1].Trim());
            Logger.Instance.Info("обработана строка " + line);

            line = file.ReadLine();
            sline = line.Split(' ');
            FNameTauX = sline[1].Trim();
            Logger.Instance.Info("обработана строка " + line);

            line = file.ReadLine();
            sline = line.Split(' ');
            FNameTauY = sline[1].Trim();
            Logger.Instance.Info("обработана строка " + line);

            line = file.ReadLine();
            sline = line.Split(' ');
            FNamePress = sline[1].Trim();
            Logger.Instance.Info("обработана строка " + line);

            line = file.ReadLine();
            sline = line.Split(' ');
            FNameZeta = sline[1].Trim();
            Logger.Instance.Info("обработана строка " + line);

            line = file.ReadLine();
            sline = line.Split(' ');
            FNameX = sline[1].Trim();
            Logger.Instance.Info("обработана строка " + line);

            line = file.ReadLine();
            sline = line.Split(' ');
            FNameY = sline[1].Trim();
            Logger.Instance.Info("обработана строка " + line);

            IsExtension = true;
        }
    }
}
