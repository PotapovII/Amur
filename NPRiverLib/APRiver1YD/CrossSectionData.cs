//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//---------------------------------------------------------------------------
//                 кодировка : 03.09.2025 Потапов И.И.
//---------------------------------------------------------------------------
namespace NPRiverLib.APRiver1YD
{
    using System;
    using System.IO;

    using GeometryLib;
    using CommonLib.Function;
    /// <summary>
    /// Данные о задаче в створе канала
    /// </summary>
    [Serializable]
    public class CrossSectionData
    {
        /// <summary>
        /// Количество полей
        /// </summary>
        public int Count = 8;
        /// <summary>
        ///  начальная геометрия русла
        /// </summary>
        public IDigFunction Geometry;
        /// <summary>
        /// уровни(нь) свободной поверхности потока во времени
        /// </summary>
        public IDigFunction WaterLevels;
        /// <summary>
        /// расход потока во времени
        /// </summary>
        public IDigFunction FlowRate;
        /// <summary>
        /// скорости на сободной поверхности потока
        /// </summary>
        public IDigFunction VelocityX;
        /// <summary>
        /// радиальные скорости на сободной поверхности потока
        /// </summary>
        public IDigFunction VelocityY;
        /// <summary>
        /// шероховатость дна
        /// </summary>
        public IDigFunction Roughness;
        /// <summary>
        /// Угол естественного откоса
        /// </summary>
        public IDigFunction TanPhi;
        /// <summary>
        /// Напряжния удельного сцепления
        /// </summary>
        public IDigFunction Tau_Adhesion;
        /// <summary>
        /// Данные о створе
        /// </summary>
        public IDigFunction[] crossFunctions = null;
        public CrossSectionData(int Count = 8) 
        {
            if (Count != 8) Count = 3;
            WaterLevels = new DigFunction();
            Geometry = new DigFunction();
            FlowRate = new DigFunction();
            Roughness = new DigFunction();
            VelocityX = new DigFunction();
            VelocityY = new DigFunction();
            Roughness = new DigFunction();
            TanPhi = new DigFunction();
            Tau_Adhesion = new DigFunction();
            UpDate();
        }

        public double[] GetArg2()
        {
            string name = "";
            double[] x = null;
            double[] y = null;
            Geometry.GetFunctionData(ref name, ref x, ref y);
            double[] arg = new double[2] { x[0], x[x.Length-1] };
            return arg;
        }

        public CrossSectionData(IDigFunction[] funs)
        {
            Geometry = funs[0];
            WaterLevels = funs[1];
            FlowRate = funs[2];
            Count = 3;
            if (funs.Length > 3)
            {
                VelocityX = funs[3];
                VelocityY = funs[4];
                Roughness = funs[5];
                TanPhi = funs[6];
                Tau_Adhesion = funs[7];
                Count = 8;
            }
            UpDate();
        }

        public void UpDate()
        {
            if(Count == 3)
            {
                crossFunctions = new IDigFunction[3]
                {
                    Geometry, WaterLevels, FlowRate
                };
            }
            else
            {
                crossFunctions = new IDigFunction[8]
                {
                Geometry, WaterLevels, FlowRate, VelocityX, VelocityY, Roughness, TanPhi, Tau_Adhesion
                };
            }
        }

        public CrossSectionData(CrossSectionData p)
        {
            this.Geometry = new DigFunction(p.Geometry);
            this.WaterLevels = new DigFunction(p.WaterLevels);
            this.FlowRate = new DigFunction(p.FlowRate);
            this.VelocityX = new DigFunction(p.VelocityX);
            this.VelocityY = new DigFunction(p.VelocityY);
            this.Roughness = new DigFunction(p.Roughness);
            this.TanPhi = new DigFunction(p.TanPhi);
            this.Tau_Adhesion = new DigFunction(p.Tau_Adhesion);
            UpDate();
        }
        public void Load(StreamReader file)
        {
            int i = 0;
            try
            {
                for (i = 0; i < crossFunctions.Length; i++)
                    crossFunctions[i].Load(file);
                Count = 8;
            }
            catch 
            {
                Count = 3;
                if (Count != i)
                    throw new Exception("Несогласованность данных в файле");
            }
        }
        public void Save(StreamWriter file)
        {
            for (int i = 0; i < crossFunctions.Length; i++)
                crossFunctions[i].Save(file);
        }
    }
}
