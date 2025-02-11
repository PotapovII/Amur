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
//                    28.12.2024 Потапов И.И.
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
    public class MWRiver : IMWRiver
    {
        IMesh mesh;
        public IMesh GetMesh() => mesh;
        public IMWRiverDistance mWRiverDistance;
        public IMWRiverCross mWRiverCross;
        /// <summary>
        /// Тип формы канала в створе потока
        /// </summary>
        public SСhannelForms channelSectionForms { get; set; }
        /// <summary>
        /// массив площадей КЭ
        /// </summary>
        public double[] S = null;
        /// <summary>
        /// Флаг отключения свободной поверхности как границы, 
        /// свободная поверхность потока считается границей
        /// (модели Великанова ...)
        /// </summary>
        protected int waterLevelMark = -1;
        public MWRiver(IMesh mesh, 
            SСhannelForms channelSectionForms = SСhannelForms.porabolic,
            double[] S = null,
            int waterLevelMark = -1)
        {
            mWRiverDistance = new MWRiverDistance(mesh, channelSectionForms);
            mWRiverCross = new MWRiverCross(mesh, channelSectionForms, S);
            this.channelSectionForms = channelSectionForms;
            this.waterLevelMark = waterLevelMark;
            this.mesh = mesh;
            Area = mWRiverCross.GetArea();
            Bottom = mWRiverCross.GetBottom();
            Width = mWRiverCross.GetWidth();
            
        }
        public MWRiver(IMWCross mWCrossTri)
        {
            mWRiverDistance = ((MWCrossTri)mWCrossTri).mWRiverDistance;
            mWRiverCross = ((MWCrossTri)mWCrossTri).mWRiverCross;
            this.channelSectionForms = mWCrossTri.channelSectionForms;
            this.waterLevelMark = ((MWCrossTri)mWCrossTri).waterLevelMark;
            Area = mWRiverCross.GetArea();
            Bottom = mWRiverCross.GetBottom();
            Width = mWRiverCross.GetWidth();
            this.mesh = mWCrossTri.GetMesh();
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
            return mWRiverCross.RiverFlowRate(U, ref Area);
        }
        /// <summary>
        /// Минимальное растояние от узла до стенки
        /// </summary>
        public double[] GetDistance() => mWRiverDistance.GetDistance();
        /// <summary>
        /// Приведенная глубина
        /// </summary>
        /// <returns></returns>
        public double[] GetHp() => mWRiverDistance.GetHp();
    }
}
