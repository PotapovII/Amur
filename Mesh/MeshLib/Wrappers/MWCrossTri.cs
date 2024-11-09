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
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//---------------------------------------------------------------------------
//                    07.03.2024 Потапов И.И.
//---------------------------------------------------------------------------
//             Эксперимент: прототип "Расчетная область"
//---------------------------------------------------------------------------
namespace MeshLib.Wrappers
{
    using System;

    using CommonLib;
    using CommonLib.Mesh;

    /// <summary>
    /// ОО: Обертка для КЭ сетки - выполняет предварительные вычисления
    /// при решении задач методом КЭ в канале (y,z)
    /// </summary>
    [Serializable]
    public class MWCrossTri : MWDistanceTri, IMWCross
    {
        /// <summary>
        /// Флаг отключения свободной поверхности как границы, 
        /// свободная поверхность потока считается границей
        /// (модели Великанова ...)
        /// </summary>
        protected int waterLevelMark=-1;
        public MWCrossTri(IMesh mesh,
        SСhannelForms channelSectionForms = SСhannelForms.porabolicСhannelSection,
        int waterLevelMark=-1) : base(mesh, channelSectionForms)
        {
            this.waterLevelMark = waterLevelMark;
            SetMesh(mesh);
        }
        /// <summary>
        /// Установка КЭ сетки
        /// </summary>
        /// <param name="mesh"></param>
        public override void SetMesh(IMesh mesh)
        {
            base.SetMesh(mesh);
            double[] X = mesh.GetCoords(0);
            double[] Y = mesh.GetCoords(1);
            int[] BoundElementsMark = mesh.GetBElementsBCMark();
            TwoElement[] BoundElems = mesh.GetBoundElems();
            Area = 0;
            Width = 0;
            Bottom = 0;
            for (uint elem = 0; elem < mesh.CountElements; elem++)
                Area += S[elem];
            if (channelSectionForms == SСhannelForms.porabolicСhannelSection ||
                channelSectionForms == SСhannelForms.halfPorabolicСhannelSection)
            {
                for (int belem = 0; belem < mesh.CountBoundElements; belem++)
                {
                    uint i = BoundElems[belem].Vertex1;
                    uint j = BoundElems[belem].Vertex2;
                    double Lx = X[i] - X[j];
                    double Ly = Y[i] - Y[j];
                    double L = Math.Sqrt(Lx * Lx + Ly * Ly);
                    if (BoundElementsMark[belem] == 0)
                        Bottom += L;
                    if (BoundElementsMark[belem] == 1)
                        Width += L;
                }
            }
            else
            {
                for (int belem = 0; belem < mesh.CountBoundElements; belem++)
                {
                    uint i = BoundElems[belem].Vertex1;
                    uint j = BoundElems[belem].Vertex2;
                    double Lx = X[i] - X[j];
                    double Ly = Y[i] - Y[j];
                    double L = Math.Sqrt(Lx * Lx + Ly * Ly);
                    if (BoundElementsMark[belem] == 0 ||
                        BoundElementsMark[belem] == 1 ||
                        BoundElementsMark[belem] == 3)
                        Bottom += L;
                    if (BoundElementsMark[belem] == 1)
                        Width += L;
                }
            }
        }
        /// <summary>
        /// Площадь сечения
        /// </summary>
        /// <returns></returns>
        public double GetArea() => Area;
        protected double Area;
        /// <summary>
        /// Смоченный периметр живого сечения
        /// </summary>
        /// <returns></returns>
        public double GetBottom() => Bottom;
        protected double Bottom;
        /// <summary>
        /// Ширина живого сечения
        /// </summary>
        /// <returns></returns>
        public double GetWidth() => Width;
        protected double Width;
        /// <summary>
        /// Расчет интеграла по площади расчетной области для функции U 
        /// (например расхода воды через створ, если U - скорость потока в узлах)
        /// </summary>
        /// <param name="U">функции</param>
        /// <param name="Area">площадь расчетной области </param>
        /// <returns>интеграла по площади расчетной области для функции U</returns>
        /// <exception cref="Exception"></exception>  
        public double RiverFlowRate(double[] U, ref double Area)
        {
            int cu = 3;
            Area = 0;
            double riverFlowRateCalk = 0;
            TriElement[] eKnots = mesh.GetAreaElems();
            for (uint elem = 0; elem < mesh.CountElements; elem++)
            //    Parallel.For(0, mesh.CountElements, (elem, state) =>
            {
                double mU = (U[eKnots[elem].Vertex1] +
                             U[eKnots[elem].Vertex2] +
                             U[eKnots[elem].Vertex3]) / cu;
                // расход по живому сечению
                riverFlowRateCalk += S[elem] * mU;
                Area += S[elem];
            }// });
            return riverFlowRateCalk;
        }
    }
}
