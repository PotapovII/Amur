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
//                          ПРОЕКТ  "МКЭ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 11.06.2024 Потапов И.И.
//------------------------------------------------------------------------
namespace MeshGeneratorsLib.Renumberation
{
    using MeshLib;
    using CommonLib;
    using CommonLib.Mesh;

    public abstract class ARenumberator : IFERenumberator
    {
        protected IFEMesh mesh = null;
        protected int CountKnots;
        protected double[] X;
        protected double[] Y;
        public ARenumberator()
        { }
        public ARenumberator(IFEMesh mesh)
        {
            Set(mesh);
        }
        protected void Set(IFEMesh mesh)
        {
            this.mesh = mesh;
            X = mesh.GetCoords(0);
            Y = mesh.GetCoords(1);
            CountKnots = mesh.CountKnots;
        }
        /// <summary>
        /// Фронтальный перенумератор сетки по координатаам 
        /// </summary>
        protected void DoRenumberation(ref IFEMesh NewMesh, int[] NewNumber, Direction direction = Direction.toRight)
        {
            NewMesh = new FEMesh(mesh);
            for (int i = 0; i < CountKnots; i++)
            {
                int OldKnot = i;
                int NewKnot = NewNumber[OldKnot];
                // координаты
                NewMesh.CoordsX[NewKnot] = X[i];
                NewMesh.CoordsY[NewKnot] = Y[i];
            }
            for (int i = 0; i < NewMesh.BNods.Length; i++)
            {
                int OldID = NewMesh.BNods[i].ID;
                NewMesh.BNods[i].ID = NewNumber[OldID];
            }
            for (int e = 0; e < NewMesh.CountElements; e++)
            {
                for (int i = 0; i < NewMesh.AreaElems[e].Length; i++)
                {
                    int OldID = NewMesh.AreaElems[e][i].ID;
                    NewMesh.AreaElems[e][i].ID = NewNumber[OldID];
                }
            }
            for (int e = 0; e < NewMesh.CountBoundElements; e++)
            {
                for (int i = 0; i < NewMesh.BoundElems[e].Length; i++)
                {
                    int OldID = NewMesh.BoundElems[e][i].ID;
                    NewMesh.BoundElems[e][i].ID = NewNumber[OldID];
                }
            }

        }
        public abstract void FrontRenumberation(ref IFEMesh NewMesh, IFEMesh mesh, Direction direction = Direction.toRight);

        /// <summary>
        /// Имена 
        /// </summary>
        /// <returns></returns>
        public static string[] Names()
        {
            return new string[2]
            {
                "Сеточный хеш - фронтальный перенумератор",
                "Фронтальный по координатный перенумератор "
            };
        }
        /// <summary>
        /// Получить перенумератор по коду
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static IFERenumberator GetRenumberator(int ID)
        {
            if (ID == 0)
                return new FERenumberator();
            else
                return new FERenumberatorHash();
        }
    }
}
