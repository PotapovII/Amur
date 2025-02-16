using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeometryLib.Areas;
using MemLogLib;
using MeshLib;
using GeometryLib;

namespace TestEliz2D
{
    public class MeshBuilderProfile2D
    {
        /// <summary>
        /// Итоговая сетка
        /// </summary>
        KsiMesh _FinalMesh;
        //
        /// <summary>
        ///Генератор области 
        /// </summary>
        PointsGeneratorEliz[] pg = null;
        public KsiMesh FinalMesh
        {
            get { return _FinalMesh; }
        }
        //
        public TimeSpan time = new TimeSpan();
        public TimeSpan timeTransport = new TimeSpan();
        public TimeSpan timeCalculate = new TimeSpan();
        public TimeSpan timeAll = new TimeSpan();
        System.Diagnostics.Stopwatch stopW = new System.Diagnostics.Stopwatch();
        /// <summary>
        /// Флаг расчета сетки на GPU с использованием технологии OpenCL
        /// </summary>
        public bool OpenCL = false;
        /// <summary>
        /// Флаг расчета сетки на GPU с использованием технологии CUDA
        /// </summary>
        public bool Cuda = false;
        /// <summary>
        /// Составная область
        /// </summary>
        AreasProfile2D Area;
        //
        /// <summary>
        /// Метод генерации всей области
        /// </summary>
        /// <param name="subareas">массив подобластей расчетной области</param>
        /// <param name="Method">метод разбиения сетки</param>
        public MeshBuilderProfile2D(AreasProfile2D area, PointsGeneratorEliz[] pg)
        {
            Area = area;
            this.pg = pg;
        }
        public void ChangeArea(AreasProfile2D area)
        {
            Area = area;
        }
        public KsiMesh GenerateMesh(bool structureChanged = true)
        {
            // формирование массива под сетки простых областей
            
            //
            SimpleAreaProfile[] sa = Area.GetSimpleAreas();
            //
            KsiMesh[] Meshes = new KsiMesh[pg.Length];
            for (int i = 0; i < Meshes.Length; i++)
            {
                if (sa[i]!=null)
                    Meshes[i] = new KsiMesh();
            }
            //
            for (int i = 0; i < pg.Length; i++)
            {
                if (pg[i] != null)
                {
                    Meshes[i] = pg[i].Generate(sa[i]);
                    //
                    timeCalculate += pg[i].timeCalculate;
                    timeAll += pg[i].timeAll;
                    timeTransport += pg[i].timeTransrort;
                }
            }
            //
            if (Meshes[0] != null)
            {
                Meshes[0].Add(Meshes[1]);
                Meshes[1] = Meshes[0];
            }
            //
            if (Meshes[2] != null)
                Meshes[1].Add(Meshes[2]);

            KsiMesh prevMesh = null;
            if (!structureChanged)
                prevMesh = _FinalMesh;
            //
            _FinalMesh = Meshes[1];
            _FinalMesh.Renumberation();
            //
            stopW.Reset();
            stopW.Start();
            //
            // на метод выше в KsiWrapper
            //if (!structureChanged)
            //    _FinalMesh.TransportCVstructure(prevMesh);
            //_FinalMesh.TriangleGeometryCalculation(structureChanged);
            //_FinalMesh.MakeWallFuncStructure(surf_flag);
            //
            time = stopW.Elapsed;
            stopW.Stop();
            return _FinalMesh;
        }

       

    }

}
