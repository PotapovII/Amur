//---------------------------------------------------------------------------
//                   ПРОЕКТ  "Русловые процессы"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 16.11.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace RiverLib.IO
{
    using System;
    using System.IO;
    using System.Collections.Generic;

    using MeshLib;
    using MemLogLib;
    using CommonLib;
    using CommonLib.IO;
    using GeometryLib;
    using MeshAdapterLib;
    using RiverLib.River2D;
    using MeshGeneratorsLib;
    using CommonLib.Mesh;
    using MeshGeneratorsLib.Renumberation;

    [Serializable]
    public class RiverFormatReader2DQuad : ATaskFormat<IRiver>
    {
        /// <summary>
        /// Поддержка внешних форматов загрузки
        /// </summary>
        public override bool SupportImport { get; }
        /// <summary>
        /// Поддержка внешних форматов сохранения
        /// </summary>
        public override bool SupportExport { get; }
        string fileNameMeshParams;
        public RiverFormatReader2DQuad(string NameMeshParams)
        {
            SupportImport = true;
            SupportExport = true;
            this.fileNameMeshParams = NameMeshParams;
            extString = new List<string>() { ".txt" };
        }
        /// <summary>
        /// Прочитать файл, содержащий речную задачу из старого River2D формата данных cdg.
        /// </summary>
        /// <param name="filename">Путь к файлу для чтения..</param>
        /// <returns>Экземпляр интерфейса<see cref="IMesh" /> interface.</returns>
        public override void Read(string filename, ref IRiver river, uint testID = 0)
        {
            RiverEmptyXY2DQuad river2D = river as RiverEmptyXY2DQuad;
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
                    river = new RiverEmptyXY2D(new RiverStreamParams());
                    Logger.Instance.Info(ex.Message);
                }
            }
            else
                throw new NotSupportedException("Could not load '" + filename + "' file.");

        }


        public IFEMesh CreateFEMesh(HMeshParams mp, double xMin, double xMax, double yMin, double yMax)
        {
            HMapSegment[] mapSegment = new HMapSegment[4];

            // количество параметров на границе (задан 1)
            double[] param = { 5 };

            VMapKnot p0 = new VMapKnot(xMin, yMin, param);
            VMapKnot p1 = new VMapKnot(xMax, yMin, param);
            VMapKnot p2 = new VMapKnot(xMax, yMax, param);
            VMapKnot p3 = new VMapKnot(xMin, yMax, param);

            List<VMapKnot> Knots0 = new List<VMapKnot>() { new VMapKnot(p0), new VMapKnot(p1) };
            List<VMapKnot> Knots1 = new List<VMapKnot>() { new VMapKnot(p1), new VMapKnot(p2) };
            List<VMapKnot> Knots2 = new List<VMapKnot>() { new VMapKnot(p2), new VMapKnot(p3) };
            List<VMapKnot> Knots3 = new List<VMapKnot>() { new VMapKnot(p3), new VMapKnot(p0) };

            // определение сегментов
            mapSegment[0] = new HMapSegment(Knots0);
            mapSegment[1] = new HMapSegment(Knots1);
            mapSegment[2] = new HMapSegment(Knots2);
            mapSegment[3] = new HMapSegment(Knots3);

            // область для генерации КЭ сетки
            HMapSubArea subArea = new HMapSubArea();
            HMapFacet[] facet = new HMapFacet[4];
            for (int i = 0; i < 4; i++)
            {
                facet[i] = new HMapFacet();
                facet[i].Add(mapSegment[i]);
                subArea.Add(facet[i]);
            }
            HMeshParams md = new HMeshParams();
            IHTaskMap mapMesh = new HTaskMap(subArea);
            IFERenumberator Renumberator = new FERenumberator();
            //mapMesh.Add(subArea);
            DirectorMeshGenerator mg = new DirectorMeshGenerator(null, mp, mapMesh, Renumberator);
            // генерация КЭ сетки
            IFEMesh feMesh = mg.Create();
            return feMesh;
        }
        /// <summary>
        /// Сохраняем сетку на диск.
        /// </summary>
        /// <param name="mesh">Экземпляр интерфейса<see cref="IMesh" /> interface.</param>
        /// <param name="filename">Путь к файлу для сохранения.</param>
        public override void Write(IRiver river, string filename)
        {
            River2D river2D = river as River2D;
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
