//---------------------------------------------------------------------------
//                   ПРОЕКТ  "Русловые процессы"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 03.03.2024 Потапов И.И.
//                форматы ( RvX ) для створа реки
//---------------------------------------------------------------------------
namespace NPRiverLib.IO
{
    using System;
    using System.IO;
    using System.Collections.Generic;

    using MemLogLib;
    using CommonLib;
    using CommonLib.IO;
    using NPRiverLib.APRiver1YD;
    using NPRiverLib.APRiver1YD.Params;

    [Serializable]
    public class TaskReader1YD_RvY : ATaskFormat<IRiver>
    {
        /// <summary>
        /// Поддержка внешних форматов загрузки
        /// </summary>
        public override bool SupportImport { get; }
        /// <summary>
        /// Поддержка внешних форматов сохранения
        /// </summary>
        public override bool SupportExport { get; }

        //const string Ext_RvY = ".RvY";
        const string Ext_RvY = ".rvy";
        public TaskReader1YD_RvY()
        {
            extString = new List<string>() { Ext_RvY };
            SupportImport = true;
            SupportExport = true;
        }
        public override void Read(string filename, ref IRiver river, uint testID = 0)
        {
            // расширение файла
            string FileEXT = Path.GetExtension(filename).ToLower();
            switch (FileEXT)
            {
                case Ext_RvY:
                    Read_RvY(filename, ref river);
                    break;
            }
        }
        /// <summary>
        /// Сохраняем сетку на диск речной формат данных 1DX для створа
        /// </summary>
        /// <param name="mesh">Экземпляр интерфейса<see cref="IMesh" /> interface.</param>
        /// <param name="filename">Путь к файлу для сохранения.</param>
        public override void Write(IRiver river, string filename)
        {
            // расширение файла
            string FileEXT = Path.GetExtension(filename).ToLower();
            switch (FileEXT)
            {
                case Ext_RvY:
                    Write_RvY(river, filename);
                    break;
            }
        }
        public void Write_RvY(IRiver river, string filename)
        {
            //River2D river_1YD = river as River2D;
            //if (river_1YD == null)
            //    throw new NotSupportedException("Не возможно сохранить выбранный формат задачи river_1YD == null");
            //try
            //{
            //    using (StreamWriter file = new StreamWriter(filename))
            //    {
            //        file.WriteLine(" RVTransient analysis = 1");
            //        file.WriteLine(" Mesh type = 1");
            //        file.WriteLine(" Number of Time Steps = 1");
            //        file.WriteLine(" Delta t Acceleration Factor = " + river_1YD.dtTrend);
            //        file.WriteLine(" Time = " + river_1YD.time.ToString());
            //        file.WriteLine(" Delta t = " + river_1YD.dtime.ToString());
            //        file.WriteLine(" Theta = " + river_1YD.theta.ToString());
            //        file.WriteLine(" UW = " + river_1YD.UpWindCoeff.ToString());
            //        file.WriteLine(" Eddy Viscosity Bed Shear Parameter = " + river_1YD.turbulentVisCoeff.ToString());
            //        file.WriteLine(" Latitude  = " + river_1YD.latitudeArea.ToString());
            //        file.WriteLine(" Groundwater Storativity = " + river_1YD.droundWaterCoeff.ToString());
            //        file.WriteLine(" Diffusive wave Solution = 0 zero for fully dynamic only");
            //        file.WriteLine(" UW Jacobian terms included = 0  zero for not included");
            //        file.WriteLine(" Plot Code = 2 zero for xsec one for contour two for velocity and three for threeD");
            //        file.WriteLine(" RVTransient Boundary Condition = 0  zero for Steady BCs");
            //        file.WriteLine(" Maximum Number of Iterations = 9");
            //        file.WriteLine(" Small Depths Occur = 1 zero for no small depth calculations");
            //        file.WriteLine(" Jacobian Terms included = 1 zero for not included");
            //        file.WriteLine(" Eddy Viscosity Constant = 0");
            //        file.WriteLine(" Minimum Depth for Groundwater Flow Calculation = " + river_1YD.H_minGroundWater.ToString());
            //        file.WriteLine(" Eddy Viscosity Horizontal Shear Paramter = 0");
            //        file.WriteLine(" Groundwater Transmissivity = " + river_1YD.filtrСoeff.ToString());
            //        file.WriteLine(" Dimensions = 2");
            //        file.WriteLine(" Number of Variables = 3");
            //        file.WriteLine(" [K] governing equation numbers");
            //        for (int i = 0; i < 3; i++)
            //        {
            //            for (int ii = 0; ii < 4; ii++)
            //                file.Write(" 1");
            //            file.WriteLine();
            //        }
            //        file.WriteLine(" Number of Parameters = 3");
            //        file.WriteLine(" Number of Boundary Parameters = 7");
            //        file.WriteLine(" Number of Nodes = " + river_1YD.mesh.CountKnots.ToString());
            //        file.WriteLine(" Number of Elements = " + river_1YD.mesh.CountElements.ToString());
            //        file.WriteLine(" Number of  Boundary Elements = " + river_1YD.mesh.CountBoundElements.ToString());
            //        file.WriteLine(" Number of  Boundary Segments = " + river_1YD.mesh.CountSegment.ToString());

            //        file.WriteLine("\n\n Node Information ");
            //        file.WriteLine("\n Node #, Coordinates, Parameters, Variables \n");

            //        // запись узловых полей
            //        for (int i = 0; i < river_1YD.mesh.CountKnots; i++)
            //            file.WriteLine(river_1YD.mesh.nodes[i].ToStringCDG());
            //        file.WriteLine("\n Element Information");
            //        file.WriteLine("\n Element #, vtype, gtype, CountNodes\n\n");
            //        // запись КЭ 
            //        for (int i = 0; i < river_1YD.mesh.CountElements; i++)
            //            file.WriteLine(river_1YD.mesh.AreaElems[i].ToStringCDG());
            //        file.WriteLine("\n Boundary Element #, vtype, gtype, CountNodes, boundary condition codes\n");
            //        // запись ГКЭ 
            //        for (int i = 0; i < river_1YD.mesh.CountBoundElements; i++)
            //            file.WriteLine(river_1YD.mesh.BoundElems[i].ToStringCDG());
            //        file.WriteLine("\n Boundary Seg #,Boundary type,stage,QT,start node #,end node #\n\n");
            //        // запись сегментов
            //        for (int i = 0; i < river_1YD.mesh.CountSegment; i++)
            //            file.WriteLine(river_1YD.mesh.boundSegment[i].ToStringCDG());
            //        file.WriteLine("\nno more breakline segments.\n\n");

            //        file.Close();

            //    }
            //}
            //catch (Exception ex)
            //{
            //    Logger.Instance.Info(ex.Message);
            //}
            return;
        }
        /// <summary>
        /// Прочитать файл, речной формат данных 1DX для створа
        /// </summary>
        /// <param name="filename">Путь к файлу для чтения..</param>
        /// <returns>Экземпляр интерфейса<see cref="IMesh" /> interface.</returns>
        public void Read_RvY(string filename, ref IRiver river)
        {
            APRiverFEM1YD river_1YD = river as APRiverFEM1YD;
            if (river_1YD == null)
                throw new NotSupportedException("Не возможно загрузить выбранный объект задачи river_1YD == null");
            if (IsSupported(filename) == true)
            {
                try
                {
                    using (StreamReader file = new StreamReader(filename))
                    {
                        var value = LOG.GetInt(file.ReadLine());  // Версия
                        if (value != 1)
                            throw new Exception("Не согласованность версий");
                        RSCrossParams p = new RSCrossParams();
                        p.Load(file);
                        river_1YD = new TriSroSecRiverTask1YD(p);
                        river_1YD.LoadData(file);
                        file.Close();
                    }
                    river = river_1YD;
                }
                catch (Exception ex)
                {
                    Logger.Instance.Info(ex.Message);
                }
            }
            else
                throw new NotSupportedException("Could not load '" + filename + "' file.");
        }
        /// <summary>
        /// Создает список тестовых задач для загрузчика по умолчанию
        /// </summary>
        /// <returns></returns>
        public override List<string> GetTestsName()
        {
            List<string> strings = new List<string>();
            strings.Add("Основная задача - тестовая");
            return strings;
        }

    }
}
