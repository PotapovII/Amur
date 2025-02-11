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
//                   28.12.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshLib.Wrappers
{
    using MemLogLib;
    using CommonLib;

    using System;
    using System.Linq;
    using CommonLib.Mesh;
    using GeometryLib.Vector;

    /// <summary>
    /// ОО: Обертка для КЭ сетки - выполняет вычисления растояние от узла до ближайшей стенки канала
    /// при решении задач методом КЭ в канале (x,z)
    /// </summary>
    [Serializable]
    public class MWRiverNormDistance :  IMWRiverDistance
    {
        /// <summary>
        /// Тип формы канала в створе потока
        /// </summary>
        public SСhannelForms channelSectionForms { get; set; }
        /// <summary>
        /// Минимальное растояние от узла до стенки
        /// </summary>
        protected double[] distance = null;
        /// <summary>
        /// Приведенная глубина
        /// Растояние до свободной поверхности потока по лучу от стенки
        /// </summary>
        protected double[] Hp = null;
        /// <summary>
        /// Приведенная глубина
        /// Растояние до свободной поверхности потока по лучу от стенки
        /// </summary>
        protected Vector2[] normals = null;
        /// <summary>
        /// Приведенная глубина
        /// Растояние до свободной поверхности потока по лучу от стенки
        /// </summary>
        protected Vector2[] tau =  null;
        /// <summary>
        /// Минимальное растояние от узла до стенки
        /// </summary>
        public double[] GetDistance() => distance;
        /// <summary>
        /// Приведенная глубина
        /// </summary>
        /// <returns></returns>
        public double[] GetHp() => Hp;
        /// <summary>
        /// Трехузловая сетка
        /// </summary>
        protected IMesh mesh;
        public MWRiverNormDistance(IMesh mesh,
        SСhannelForms channelSectionForms = SСhannelForms.porabolic)
        {
            this.channelSectionForms = channelSectionForms;
            this.mesh = mesh;
            Init();
        }
        public MWRiverNormDistance(IMWRiverDistance wMmesh)
        {
            channelSectionForms = wMmesh.channelSectionForms;
            MEM.MemCopy(ref distance, wMmesh.GetDistance());
            MEM.MemCopy(ref Hp, wMmesh.GetHp());
        }
        protected void Init()
        {
            MEM.Alloc(mesh.CountKnots, ref distance, "distance");
            MEM.Alloc(mesh.CountKnots, ref Hp, "Hp");
            CalkDistance();
        }
        /// <summary>
        /// Вычисление минимального растояния от узла до стенки
        ///  В плпнпх - хеширование узлов на масштабе глубины
        /// </summary>
        public virtual void CalkDistance()
        {
            int belem = 0;
            try
            {
                double[] X = mesh.GetCoords(0);
                double[] Y = mesh.GetCoords(1);
                TwoElement[] BoundElems = mesh.GetBoundElems();
                int[] BoundElementsMark = mesh.GetBElementsBCMark();
                int CountElemsBed = 0;
                for (belem = 0; belem < mesh.CountBoundElements; belem++)
                    if (BoundElementsMark[belem] == 0)
                        CountElemsBed++;
                MEM.Alloc(CountElemsBed, ref tau);
                MEM.Alloc(CountElemsBed, ref normals);
                int cbelem = 0;
                for (belem = 0; belem < mesh.CountBoundElements; belem++)
                    if (BoundElementsMark[belem] == 0)
                    {
                        uint i = BoundElems[belem].Vertex1;
                        uint j = BoundElems[belem].Vertex2;
                        tau[cbelem] = Vector2.Normalize(new Vector2(X[j] - X[i], Y[j] - Y[i]));
                        normals[cbelem] = tau[cbelem].GetOrtogonalLeft();
                        cbelem++;
                    }
                double Y_max = Y.Max();
                double Y_min = Y.Min();
                double Hmax = Y_max - Y_min;
                // перебор по узлам области
                for (int nod = 0; nod < mesh.CountKnots; nod++)
                {
                    belem = 0;
                    int idx = belem;
                    int cidx = belem;
                    cbelem = 0;
                    double r_min = double.MaxValue;
                    for (belem = 0; belem < mesh.CountBoundElements; belem++)
                    {
                        if (BoundElementsMark[belem] == 0)
                        {
                            uint i = BoundElems[belem].Vertex1;
                            var PA = new Vector2(X[nod] - X[i], Y[nod] - Y[i]);
                            double r_cur = Math.Abs(Vector2.Dot(PA, normals[cbelem]));
                            if (r_min > r_cur)
                            {
                                r_min = r_cur;
                                idx  = belem;
                                cidx = cbelem;
                                if (MEM.Equals(r_min, 0) == true)
                                    break;
                            }
                            cbelem++;
                        }
                    }
                    distance[nod] = r_min;
                    uint iA = BoundElems[idx].Vertex1;
                    Vector2 A = new Vector2(X[iA], Y[iA]);
                    var AP = new Vector2(X[nod] - X[iA], Y[nod] - Y[iA]);
                    Vector2 C = A + Vector2.Dot(AP, tau[cidx]) * tau[cidx];
                    double cosGamma = Math.Abs(normals[cidx].Y) + MEM.Error10;
                    Hp[nod] = Math.Max(0.01 * Hmax, Math.Min(Hmax, (Y_max - C.Y) / cosGamma));
                }
            }
            catch(Exception ex) 
            {
                Console.WriteLine(ex.Message);
            }
        
        }
    }
}