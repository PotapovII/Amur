#region License
/*
Copyright (c) 2019 - 2024  Potapov Igor Ivanovich, Khabarovsk

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion
//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 23.06.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace FEMTasksLib.FESimpleTask
{
    using System;
    using MemLogLib;
    using CommonLib;
    using CommonLib.Mesh;
    using CommonLib.Physics;
    /// <summary>
    /// ОО: Задача расчета взвешенных наносов 
    /// </summary>
    [Serializable]
    public class RiverSedimentTri_1Y : AComplexFEMTask
    {
        /// <summary>
        /// Задача для расчета взвешенных наносов
        /// </summary>
        protected CTransportEquationsTri taskSediment;
        #region Массивы для определения функции вихря на дне потока
        /// <summary>
        /// адреса донных узлов
        /// </summary>
        protected uint[] boundaryBedAdress = null;
        /// <summary>
        /// значения вихря на дне 
        /// </summary>
        protected double[] boundaryСonSediment;
        /// <summary>
        /// Заглушка правой части
        /// </summary>
        protected double[] Q;
        #endregion
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="mesh">сетка</param>
        /// <param name="algebra">линейный решатель</param>
        public RiverSedimentTri_1Y(IMeshWrapperСhannelSectionCFG wMesh, IAlgebra algebra, TypeTask typeTask) : base(wMesh, algebra, typeTask)
        {
            taskSediment = new CTransportEquationsTri(wMesh, algebra, typeTask);
            this.wMesh = wMesh;
            MEM.Alloc(mesh.CountKnots, ref Q, 0);
            /// адреса узлов на свободной поверхности потока и дне
            boundaryBedAdress = wMesh.GetBoundaryBedAdress();
            MEM.Alloc(boundaryBedAdress.Length, ref boundaryСonSediment, 0);
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="mesh">сетка</param>
        /// <param name="algebra">линейный решатель</param>
        public RiverSedimentTri_1Y(IMeshWrapperCrossCFG wMesh, IAlgebra algebra, TypeTask typeTask) 
            : this((IMeshWrapperСhannelSectionCFG)wMesh, algebra, typeTask) { }
        
        /// <summary>
        /// Задача с вынужденной конвекцией, на верхней крышке области заданна скорость
        /// </summary>
        public void CalkСonSediment(ref double[] СonSediment, 
            double[] Ux, double[] Uy, double[] Uz, double[] eddyViscosity, double[] tau)
        {
            IMeshWrapperСhannelSectionCFG wm = (IMeshWrapperСhannelSectionCFG)wMesh;
            MEM.Alloc(mesh.CountKnots, ref СonSediment, 0);
            // расчет потоковой скорости в створе
            Console.WriteLine("Concentration sediment:");
            // расчет концентрации на дне
            CalkBoundaryСonSediment(Ux, Uy, Uz, eddyViscosity, tau);
            // расчет концентрации в потоке
            taskSediment.TransportEquationsTaskSUPG(ref СonSediment, eddyViscosity, Uy, Uz, boundaryBedAdress, boundaryСonSediment, Q);
        }

        protected virtual void CalkBoundaryСonSediment(double[] Ux, double[] Uy, double[] Uz, 
                                        double[] eddyViscosity, double[] tau)
        {
            for(int i = 0; i < boundaryBedAdress.Length; i++)
                boundaryСonSediment[i]= SPhysics.PHYS.Cb(tau[i]);
        }
    }
}
