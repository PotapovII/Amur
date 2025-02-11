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
//                   12.03.2024 Потапов И.И.
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
    /// при решении задач методом КЭ в канале (x,z)
    /// </summary>
    [Serializable]
    public class MWDistanceTri : MeshWrapperTri, IMWDistance
    {
        public IMWRiverDistance mWRiverDistance;
        /// <summary>
        /// Тип формы канала в створе потока
        /// </summary>
        public SСhannelForms channelSectionForms { get; set; }
        public MWDistanceTri(IMesh mesh,
        SСhannelForms channelSectionForms = SСhannelForms.porabolic)
            : base()
        {
            mWRiverDistance = new MWRiverDistance(mesh, channelSectionForms);
            this.channelSectionForms = channelSectionForms;
            SetMesh(mesh);
        }
        public MWDistanceTri(IMeshWrapper wMmesh,
        SСhannelForms channelSectionForms = SСhannelForms.porabolic
            ) : base(wMmesh)
        {
            mWRiverDistance = new MWRiverDistance(mesh, channelSectionForms);
        }
        public MWDistanceTri(IMWDistance wMmesh) : base(wMmesh)
        {
            channelSectionForms = wMmesh.channelSectionForms;
            mWRiverDistance = wMmesh;
        }
        /// <summary>
        /// Установка КЭ сетки
        /// </summary>
        /// <param name="mesh"></param>
        public override void SetMesh(IMesh mesh)
        {
            base.SetMesh(mesh);
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