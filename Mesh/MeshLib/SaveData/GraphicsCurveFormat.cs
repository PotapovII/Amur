namespace MeshLib
{
    using System;
    using MemLogLib;
    using CommonLib;
    using CommonLib.IO;
    using System.IO;
    using System.Collections.Generic;

    [Serializable]
    public class GraphicsCurveFormat : ATaskFormat<IGraphicsCurve>
    {
        /// <summary>
        /// Поддержка внешних форматов загрузки
        /// </summary>
        public override bool SupportImport { get; }
        /// <summary>
        /// Поддержка внешних форматов сохранения
        /// </summary>
        public override bool SupportExport { get; }
        public GraphicsCurveFormat()
        {
            SupportImport = true;
            SupportExport = true;
            extString = new List<string>() { ".cvs", ".fun" };
        }
        /// <summary>
        /// Прочитать файл, содержащий функцию в форматах данных  csv или fun.
        /// </summary>
        /// <param name="filename">Путь к файлу для чтения..</param>
        /// <returns>Экземпляр интерфейса<see cref="IGraphicsCurve" /> interface.</returns>
        public override void Read(string filename, ref IGraphicsCurve Curve, uint testID = 0)
        {
            var curve = Curve as GraphicsCurve;
            if (curve == null)
                throw new NotSupportedException("Не возможно сохранить выбранный формат задачи GraphicsCurve, curve == null");
            if (IsSupported(filename) == true)
            {
                try
                {
                    string ext = Path.GetExtension(filename);
                    string Name = Path.GetFileNameWithoutExtension(filename);
                    switch (ext)
                    {
                        case ".cvs":
                        case ".fun":
                            using (StreamReader file = new StreamReader(filename))
                            {
                                int Count;
                                double[] x = null, y = null;
                                if (ext == extString[1]) //".fun"
                                {
                                    string Name1 = GetLines(file, 2)[1];
                                    Count = GetInt(file);
                                }
                                if (ext == extString[0] || ext == extString[1]) //".fun"
                                {
                                    string[] X = GetLines(file, 2);
                                    string[] Y = GetLines(file, 2);
                                    MEM.Alloc(X.Length, ref x);
                                    MEM.Alloc(X.Length, ref y);
                                    for (int i = 0; i < X.Length; i++)
                                    {
                                        x[i] = double.Parse(X[i].Trim(), MEM.formatter);
                                        y[i] = double.Parse(Y[i].Trim(), MEM.formatter);
                                    }
                                }
                                Curve = new GraphicsCurve(Name, x, y);
                                Logger.Instance.Info(" Number of points = " + (curve.Count).ToString());

                                file.Close();
                                return;
                            }
                            break;
                    }
                    
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
        /// Сохраняем функцию
        /// </summary>
        /// <param name="mesh">Экземпляр интерфейса<see cref="IGraphicsCurve" /> interface.</param>
        /// <param name="filename">Путь к файлу для сохранения.</param>
        public override void Write(IGraphicsCurve Curve, string filename)
        {
            var curve = Curve as GraphicsCurve;
            if (curve == null)
                throw new NotSupportedException("Не возможно сохранить выбранный формат задачи GraphicsCurve, curve == null");
            string ext = Path.GetExtension(filename);
            using (StreamWriter file = new StreamWriter(filename))
            {
                if (ext == extString[1]) //".fun"
                {
                    file.WriteLine("Name " + curve.Name);
                    file.WriteLine("Count = " + curve.Count);
                }
                if (ext == extString[0] || ext == extString[1]) //".fun"
                {
                    for (int i = 0; i < curve.Count; i++)
                        file.Write(curve[i].x.ToString() + " ");
                    file.WriteLine();
                    for (int i = 0; i < curve.Count; i++)
                        file.Write(curve[i].y.ToString() + " ");
                }
                file.Close();
                return;
            }
            throw new NotSupportedException("Не возможно сохранить выбранный формат сетки mesh == null");
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

