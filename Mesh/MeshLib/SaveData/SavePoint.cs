//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 14.12.2020 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshLib
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Runtime.Serialization.Formatters.Binary;

    using CommonLib;
    using GeometryLib;
    using CommonLib.Function;
    /// <summary>
    /// ОО: Точка сохранения задачи для визуализации ее полей
    /// </summary>
    [Serializable]
    public class SavePoint : IComparable, ISavePoint
    {
        /// <summary>
        /// Наименование точки сохранения
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// расчетное время в точке сохранения
        /// </summary>
        protected double stime;
        /// <summary>
        /// графики, не привязанные к КЭ сетки задачи в точке сохранения
        /// </summary>
        protected GraphicsData gdata = new GraphicsData();
        /// <summary>
        /// сетка задачи в точке сохранения
        /// </summary>
        public IRenderMesh cload;
        /// <summary>
        /// поля задачи в точке сохранения
        /// </summary>
        public List<IField> spoles = new List<IField>();
        /// <summary>
        /// расчетное время в точке сохранения
        /// </summary>
        public double time { get { return stime; } }
        /// <summary>
        /// поля задачи в точке сохранения связанные с сеткой IMesh
        /// </summary>
        public List<IField> poles { get { return spoles; } }
        /// <summary>
        /// сетка задачи в точке сохранения
        /// </summary>
        public IRenderMesh mesh { get { return cload; } }
        /// <summary>
        /// Контейнер кривых
        /// </summary>
        public IGraphicsData graphicsData { get { return gdata; } }
        /// <summary>
        /// Получить новый экземпляр кривой
        /// </summary>
        /// <returns></returns>
        public IGraphicsCurve CloneCurves(string Name, TypeGraphicsCurve tGraphicsCurve = TypeGraphicsCurve.AreaCurve, bool Check = true)
        {
            return new GraphicsCurve(Name, tGraphicsCurve, Check);
        }
        /// <summary>
        /// Получить новый экземпляр кривой
        /// </summary>
        /// <returns></returns>
        public IGraphicsCurve CloneCurves(string Name, double[] x, double[] y, TypeGraphicsCurve tGraphicsCurve = TypeGraphicsCurve.AreaCurve, bool Check = true)
        {
            return new GraphicsCurve(Name, x, y, tGraphicsCurve, Check);
        }

        public SavePoint(string Name="", double stime = 0) 
        {
            this.Name = Name;
            this.stime = stime;
        }
        public SavePoint(SavePoint sp)
        {
            SetSavePoint(sp);
        }
        public void SetSavePoint(double t, IMesh m, IGraphicsData gdata = null)
        {
            this.stime = t;
            if (m != null)
                this.cload = m.Clone(); 
            if (gdata != null)
                this.gdata.Add(gdata);
        }
        public void SetSavePoint(SavePoint sp)
        {
            this.stime = sp.time;
            this.Name = sp.Name;
            this.cload = ((IMesh)sp.cload).Clone();
            this.gdata = new GraphicsData(sp.gdata);
        }
        public void SetGraphicsData(GraphicsData gd)
        {
            if (gd != null)
                this.gdata.Add(gd);
        }
        public void Add(string Name, double[] Value)
        {
            if (cload != null)
                poles.Add(new Field1D(Name, Value));
            else
                throw new Exception("Сетка для полей не загружена");
        }
        public void Add(Field1D pole)
        {
            if (cload != null)
                poles.Add(new Field1D(pole));
            else
                throw new Exception("Сетка для полей не загружена");
        }
        /// <summary>
        /// Добавление векторного поля привязанного к узлам сетки
        /// </summary>
        /// <param name="Name">Название поля</param>
        /// <param name="Vx">Аргументы функции поля</param>
        /// <param name="Vy">Значение функции поля</param>
        public void Add(string Name, double[] Vx, double[] Vy)
        {
            if (cload != null)
                poles.Add(new Field2D(Name, Vx, Vy));
            else
                throw new Exception("Сетка для полей не загружена");
        }
        /// <summary>
        /// Добавление кривой
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void AddCurve(string Name, double[] x, double[] y, TypeGraphicsCurve TGraphicsCurve = TypeGraphicsCurve.AreaCurve)  
        {
            GraphicsCurve curve = new GraphicsCurve(Name, TGraphicsCurve);
            for (uint i = 0; i < Math.Min(x.Length, y.Length); i++)
                curve.Add(x[i], y[i]);
            gdata.Add(curve);
        }
        /// <summary>
        /// Удаление кривой по имени
        /// </summary>
        /// <param name="Name">Название поля</param>
        public void RemoveCurve(string Name)
        {
            gdata.RemoveCurve(Name);
        }
        /// <summary>
        /// Очистить кривые по фильтру
        /// </summary>
        /// <param name="curve"></param>
        public void ClearСurve(TypeGraphicsCurve TGraphicsCurve = TypeGraphicsCurve.TimeCurve)
        {
            gdata.Clear(TGraphicsCurve);
        }

        /// <summary>
        /// Добавить кривую в контейнер кривых IDigFunction
        /// </summary>
        /// <param name="curve"></param>
        public void AddCurve(IDigFunction df)
        {
            string name = "";
            double[] x = null;
            double[] y = null;
            df.GetFunctionData(ref name, ref x, ref y);
            GraphicsCurve curve = new GraphicsCurve(name, TypeGraphicsCurve.AllCurve);
            for (uint i = 0; i < x.Length; i++)
                curve.Add(x[i], y[i]);
            gdata.Add(curve);
        }
        /// <summary>
        /// Добавить кривую в контейнер кривых IGraphicsData 
        /// </summary>
        /// <param name="curve"></param>
        public void AddCurve(IGraphicsCurve Curve)
        {
            gdata.Add(new GraphicsCurve((GraphicsCurve)Curve));
        }
        /// <summary>
        /// Добавить контейнер кривых 
        /// </summary>
        /// <param name="Name">Название поля</param>
        /// <param name="arg">Аргументы функции поля</param>
        /// <param name="Value">Значение функции поля</param>
        public void AddGraphicsData(IGraphicsData gd)
        {
            gd.Add(gd);
        }
        /// <summary>
        ///  сортировка объектов по времени (осталось от анализатора эволуции по времени (нужно востановить)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            SavePoint sp = obj as SavePoint;
            return stime.CompareTo(sp.stime);
        }
        public void SerializableSavePoint(ISavePoint sp, string NameSave)
        {
            DateTime dtime = DateTime.Now;
            string dstr = dtime.ToString();
            string str = dstr.Replace(":", ".");
            string FileName = NameSave + " " + str + ".sp";
            // создаем объект BinaryFormatter
            BinaryFormatter formatter = new BinaryFormatter();
            // получаем поток, куда будем записывать сериализованный объект
            using (FileStream fs = new FileStream(FileName, FileMode.OpenOrCreate))
            {
                formatter.Serialize(fs, sp);
            }
        }
        /// <summary>
        /// Загрузка буфера сохранения данных
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public ISavePoint LoadSavePoint(string FileName)
        {
            SavePoint sp = null;
            // создаем объект BinaryFormatter
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream fs = new FileStream(FileName, FileMode.OpenOrCreate))
            {
                sp = (SavePoint)formatter.Deserialize(fs);
            }
            return sp;
        }

        /// <summary>
        /// Сохраняем облако глубин в файл 
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="FileName"></param>
        /// <param name="shift"></param>
        public void ImportSPMesh(string FileName,int shift = 1)
        {
            using (StreamWriter sw = new StreamWriter(FileName))
            {
                string FileEXT = Path.GetExtension(FileName);
                if (FileEXT == ".bed")
                {
                    double[] H = null;
                    foreach (IField field in poles)
                    {
                        if(field.Name == "Глубина"||
                           field.Name == "Глубина потока")
                        {
                            H = ((Field1D)field).Values;
                            break;
                        }    
                    }
                    if (H == null)
                    {
                        return;
                    }
                    double ks = 0.1;
                    double[] X = mesh.GetCoords(0);
                    double[] Y = mesh.GetCoords(1);

                    for (int i = 0; i < mesh.CountKnots; i++)
                    {
                        sw.WriteLine("{0} {1} {2} {3} {4}", shift + i, X[i], Y[i], H[i], ks);
                    }
                    sw.WriteLine();
                    sw.WriteLine("no more nodes.");
                    sw.WriteLine();
                    sw.WriteLine("no more breakline segments.");
                }
                sw.Close();
            }

        }
        /// <summary>
        /// Загрузка сетки из файла 
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="FileName"></param>
        /// <param name="shift"></param>
        public void ExportSPMesh(string FileName, int shift = 1)
        {
            ISavePoint sp = null;
            string FileEXT = Path.GetExtension(FileName);
            switch(FileEXT)
            {
                case ".mesh":
                    using (StreamReader sr = new StreamReader(FileName))
                    {



                    }
                    break;
                case ".node":
                    {

                    }
                    break;
                case ".cdg":
                    {
                        //IMesh mesh = IMesh.Load(".cdg")
                        //IMesh mesh = IMesh.Save(".cdg")
                        //IRiver rtask = mrt.Clone(idxRiver);
                        //IOFormater<IRiver> loader = rtask.GetFormater();
                        //loader.Read(openFileDialog2.FileName, ref rtask);
                        //IBedLoadTask btask = mbt.Clone(idxBload);
                        //task = new ChannelProcessPro(rtask, btask);
                        //SetHeadToLogger();
                        //if (task.channelProcessError != ChannelProcessError.notError)
                        //    throw new Exception(task.GetError());
                        //this.Text = Name + " " + openFileDialog2.FileName;
                        //SetGrid();
                        //SavePoint sp = (SavePoint)task.GetSavePoint();
                    }
                    break;
            }
        }
    }
}
