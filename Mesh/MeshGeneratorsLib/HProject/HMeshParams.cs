namespace MeshGeneratorsLib
{
    using System;
    using CommonLib;
    using CommonLib.Mesh;
    using GeometryLib;
    using MemLogLib;
    using System.IO;
    using CommonLib.Geometry;

    /// <summary>
    ///ОО: Данные для генератора КЭ сетки
    /// </summary>
    [Serializable]
    public class HMeshParams : IPropertyTask
    {
        /// <summary>
        /// Средний диаметр разбиения области в %
        /// </summary>
        public HPoint diametrFE;
        /// <summary>
        /// тип КЭ используемых для генерации сетки
        /// </summary>
        public TypeMesh meshType;
        /// <summary>
        /// порядок КЭ, используемых при генерации базисной КЭ сетки
        /// </summary>
        public TypeRangeMesh meshRange;
        /// <summary>
        /// Реалаксация ортогонализации
        /// </summary>
        public double RelaxMeshOrthogonality;
        /// <summary>
        /// Метод генерации конечно элементной сетки
        /// </summary>
        public int meshMethod;
        /// <summary>
        /// Перенумерация узлов в базовой КЭ сетке
        /// </summary>
        public int reNumberation;
        public Direction direction;
        /// <summary>
        ///  генерация сетки со средним диаметром КЭ по области
        /// </summary>
        public bool flagMidle = true;
        /// <summary>
        /// Количество параметров задачи привязанных к сетке
        /// </summary>
        public int CountParams;

        public HMeshParams()
        {
            flagMidle = true;
            meshType = TypeMesh.MixMesh;
            meshRange = TypeRangeMesh.mRange1;
            meshMethod = 0;
            reNumberation = 0;
            diametrFE = new HPoint(10,0);
            RelaxMeshOrthogonality = 0.1;
            CountParams = 0;
        }
        public HMeshParams(TypeMesh meshType, TypeRangeMesh meshRange, int meshMethod,
            HPoint diametrFE, double RelaxMeshOrthogonality, int reNumberation,
            Direction direction = Direction.toRight, int CountParams = 0, bool flagMidle= true)
        {
            this.flagMidle = flagMidle;
            this.meshType = meshType;
            this.meshRange = meshRange;
            this.meshMethod = meshMethod;
            this.reNumberation = reNumberation;
            this.direction = direction;
            this.diametrFE = diametrFE;
            this.RelaxMeshOrthogonality = RelaxMeshOrthogonality;
            this.CountParams = CountParams;
        }

        public HMeshParams(IPropertyTask p)
        {
            SetParams(p);
        }
        public HMeshParams(HMeshParams p)
        {
            Set(p);
        }
        public virtual void Set(HMeshParams p)
        {
            this.flagMidle = p.flagMidle;
            this.meshType = p.meshType;
            this.meshRange = p.meshRange;
            this.meshMethod = p.meshMethod;
            this.reNumberation = p.reNumberation;
            this.direction = p.direction;
            this.diametrFE = p.diametrFE;
            this.RelaxMeshOrthogonality = p.RelaxMeshOrthogonality;
            this.CountParams = p.CountParams;
        }

        /// <summary>
        /// Чтение параметров задачи из файла
        /// </summary>
        /// <param name="file"></param>
        public virtual void Load(StreamReader file)
        {
            this.flagMidle = LOG.GetBool(file.ReadLine());
            this.meshType =  (TypeMesh) LOG.GetInt(file.ReadLine());
            this.meshRange = (TypeRangeMesh) LOG.GetInt(file.ReadLine());
            this.meshMethod = LOG.GetInt(file.ReadLine());
            this.reNumberation = LOG.GetInt(file.ReadLine());
            this.direction =  (Direction) LOG.GetInt(file.ReadLine());
            this.diametrFE = new HPoint(0, 0);
            this.diametrFE.x = LOG.GetDouble(file.ReadLine());
            this.RelaxMeshOrthogonality = LOG.GetDouble(file.ReadLine());
            this.CountParams = LOG.GetInt(file.ReadLine());
        }
        #region Методы IPropertyTask
        /// <summary>
        /// Установка свойств задачи
        /// </summary>
        /// <param name="p"></param>
        public virtual void SetParams(object obj)
        {
            HMeshParams p = obj as HMeshParams;
            if (p != null)
                Set(p);
            else
                Logger.Instance.Error("ошибка! не корректный тип параметров задачи!", "HMeshParams.SetParams()");
        }
        /// <summary>
        /// свойств задачи
        /// </summary>
        /// <param name="p"></param>
        public virtual object GetParams() { return this; }
        /// <summary>
        /// Чтение параметров задачи из файла
        /// </summary>
        /// <param name="file"></param>
        public virtual void LoadParams(string fileName)
        {
            string message = "Файл парамеров объекта HMeshParams не обнаружен";
            WR.LoadParams(Load, message, fileName);
        }
        #endregion
    }
}
