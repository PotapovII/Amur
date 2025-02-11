//---------------------------------------------------------------------------
//                   ПРОЕКТ  "Русловые процессы"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 16.11.2021 Потапов И.И.
//---------------------------------------------------------------------------
//                      Сбока No Property River Lib
//              с убранными из наследников Property классами 
//              кодировка : 24.12.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace NPRiverLib.IO
{
    using System;
    using System.IO;
    using System.Collections.Generic;

    using MeshLib;
    using MemLogLib;
    using CommonLib;
    using CommonLib.IO;
    using NPRiverLib.ABaseTask;
    using MeshAdapterLib;

    [Serializable]
    public class TaskReaderEmpty2XYD : ATaskFormat<IRiver>
    {
        /// <summary>
        /// Поддержка внешних форматов загрузки
        /// </summary>
        public override bool SupportImport { get; }
        /// <summary>
        /// Поддержка внешних форматов сохранения
        /// </summary>
        public override bool SupportExport { get; }

        const string Ext_txt = ".txt";

        public TaskReaderEmpty2XYD()
        {
            extString = new List<string>() { Ext_txt };
            SupportImport = true;
            SupportExport = true;
        }
        /// <summary>
        /// Прочитать файл, содержащий речную задачу из старого River2D формата данных cdg.
        /// </summary>
        /// <param name="filename">Путь к файлу для чтения..</param>
        /// <returns>Экземпляр интерфейса<see cref="IMesh" /> interface.</returns>
        public override void Read(string filename, ref IRiver river, uint testID = 0)
        {
            RiverEmpty2XYD river2D = river as RiverEmpty2XYD;
            if (river2D == null)
                throw new NotSupportedException("Не возможно pfuhepbnm выбранный формат задачи RiverEmptyXY2DQuad == null");
            if (IsSupported(filename) == true)
            {
                double[] zeta0 = null;
                double[] tauX = null;
                double[] tauY = null;
                double[] P = null;
                try
                {
                    TriMesh mesh = new TriMesh();
                    MeshAdapter2D ma = new MeshAdapter2D();
                    ma.LoadData(filename, ref mesh, ref zeta0, ref tauX, ref tauY, ref P);
                    river2D.Set(mesh, null);

                    double minTauX = tauX[0];
                    double maxTauX = tauX[0];
                    if (Math.Abs(maxTauX) > Math.Abs(minTauX))
                    {
                        Logger.Instance.Info("Поля напряжений не требуют инверсии");
                        for (int i = 0; i < tauX.Length; i++)
                        {
                            tauX[i] = tauX[i];
                            tauY[i] = tauY[i];
                        }
                    }
                    else
                    {
                        Logger.Instance.Info("Для полей напряжений выполнена инверсия");
                        for (int i = 0; i < tauX.Length; i++)
                        {
                            tauX[i] = tauX[i];
                            tauY[i] = tauY[i];
                        }
                    }

                }
                catch (Exception ex)
                {
                    river = new RiverEmpty2XYD(new RiverEmptyParams());
                    Logger.Instance.Info(ex.Message);
                }
            }
            else
                throw new NotSupportedException("Could not load '" + filename + "' file.");

        }

        /// <summary>
        /// Сохраняем сетку на диск.
        /// </summary>
        /// <param name="mesh">Экземпляр интерфейса<see cref="IMesh" /> interface.</param>
        /// <param name="filename">Путь к файлу для сохранения.</param>
        public override void Write(IRiver river, string filename)
        {
            RiverEmpty2XYD river2D = river as RiverEmpty2XYD;
            if (river2D == null)
                throw new NotSupportedException("Не возможно сохранить выбранный формат задачи river2D == null");
            string ext = Path.GetExtension(filename);
            if (ext == ".txt")
            {
                IMesh mesh = river2D.Mesh();
                TriElement[] elems = mesh.GetAreaElems();
                TwoElement[] belems = mesh.GetBoundElems();
                int[] bflags = mesh.GetBElementsBCMark();
                int[] bknots = mesh.GetBoundKnots();
                int[] bflagknots = mesh.GetBoundKnotsMark();
                double[] x = mesh.GetCoords(0);
                double[] y = mesh.GetCoords(1);
                using (StreamWriter file = new StreamWriter(filename))
                {

                    file.WriteLine(elems.Length);
                    for (int i = 0; i < mesh.CountElements; i++)
                    {

                        file.WriteLine(elems[i].Vertex1.ToString() + " " +
                                       elems[i].Vertex2.ToString() + " " +
                                       elems[i].Vertex3.ToString());
                    }
                    file.WriteLine(mesh.CountKnots);
                    for (int i = 0; i < mesh.CountKnots; i++)
                    {
                        file.WriteLine("{0} {1}", x[i], y[i]);
                    }
                    file.WriteLine(mesh.CountBoundElements);
                    for (int i = 0; i < mesh.CountBoundElements; i++)
                    {
                        file.WriteLine(belems[i].Vertex1.ToString() + " " +
                                       belems[i].Vertex2.ToString() + " " +
                                       bflags[i].ToString());
                    }
                    file.WriteLine(bknots.Length);
                    for (int i = 0; i < bknots.Length; i++)
                    {
                        file.WriteLine("{0} {1}",
                        bknots[i],
                        bflagknots[i]);
                    }
                    file.Close();
                }
                return;
            }
            throw new NotSupportedException("Не возможно сохранить выбранный формат сетки mesh == null");
        }
    }
}
