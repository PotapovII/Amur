//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 24.12.2024 Потапов И.И.
//---------------------------------------------------------------------------
//                      Сбока No Property River Lib
//              с убранными из наследников Property классами 
//---------------------------------------------------------------------------
namespace NPRiverLib.ABaseTask
{
    using System;

    using MemLogLib;
    using GeometryLib;

    using CommonLib;
    using CommonLib.IO;
    using NPRiverLib.IO;

    /// <summary>
    /// Заглушка для плановой руслвой задачи 
    /// </summary>
    [Serializable]
    public class RiverEmpty2XYD : ARiverAnyEmpty<RiverEmptyParams>, IRiver
    {
        /// <summary>
        /// КЭ сетка
        /// </summary>
        public override IMesh Mesh() => mesh;
        /// <summary>
        ///  Сетка для расчета донных деформаций
        /// </summary>
        public override IMesh BedMesh() => mesh;

        /// <summary>
        /// Имена файлов с данными для задачи гидродинамики
        /// </summary>
        public override ITaskFileNames taskFileNemes()
        {
            ITaskFileNames fn = new TaskFileNames(GetFormater());
            fn.NameCPParams = "NameCPParams.txt";
            fn.NameBLParams = "NameBLParams.txt";
            fn.NameRSParams = "NameRSParams.txt";
            fn.NameRData = "NameRData.txt";
            return fn;
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        public RiverEmpty2XYD(RiverEmptyParams p) : base(p, TypeTask.streamXY2D)
        {
            name = "Прокси для плановая русловой задачи RiverEmpty2XD";
            Version = "River2D 24.12.2024";
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        public RiverEmpty2XYD() : this(new RiverEmptyParams()) { }
        /// <summary>
        /// Установка КЭ сетки и решателя задачи
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="algebra">решатель задачи</param>
        public override void Set(IMesh mesh, IAlgebra algebra = null)
        {
            base.Set(mesh, algebra);
            MEM.Alloc(mesh.CountKnots, ref zeta0);
            unknowns.Clear();
            unknowns.Add(new CalkPapams("Поле напряжений T_zx", tauX));
            unknowns.Add(new CalkPapams("Поле напряжений T_zy", tauY));
            unknowns.Add(new CalkPapams("Поле напряжений", tauX, tauY));
            unknowns.Add(new Unknown("Отметки дна", zeta0));
        }
        /// <summary>
        /// Получение полей придонных касательных напряжений и давления/свободной поверхности по контексту задачи
        /// усредненных на конечных элементах
        /// </summary>
        /// <param name="tauX">придонное касательное напряжение по х</param>
        /// <param name="tauY">придонное касательное напряжение по у</param>
        /// <param name="P">придонных давления/свободная поверхности потока, по контексту задачи</param>
        public override void GetTau(ref double[] tauX, ref double[] tauY, ref double[] P,
                                    ref double[][] CS, StressesFlag sf = StressesFlag.Nod)
        {
            tauX = this.tauX;
            tauY = this.tauY;
            P = this.P;
            CS = this.CS;
        }
        /// <summary>
        /// Получить шероховатость дна
        /// </summary>
        /// <param name="zeta"></param>
        public void GetRoughness(ref double[] Roughness)
        {
            Roughness = null;
        }

        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public override IRiver Clone()
        {
            return new RiverEmpty2XYD(Params);
        }
        /// <summary>
        /// Создает экземпляр класса конвертера для 
        /// загрузки и сохранения задачи не в бинарном формате
        /// </summary>
        /// <returns></returns>
        public override IOFormater<IRiver> GetFormater()
        {
            //return new TaskReaderEmpty2XYD();
            return new TaskReaderEmpty2XYDTri();
        }
    }
}