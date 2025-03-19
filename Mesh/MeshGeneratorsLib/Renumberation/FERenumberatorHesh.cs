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
//---------------------------------------------------------------------------
namespace MeshGeneratorsLib.Renumberation
{
    using MemLogLib;
    using CommonLib;
    using CommonLib.Mesh;
    using GeometryLib;
    using System;
    public class FERenumberatorHash : ARenumberator
    {
        public FERenumberatorHash() : base() { }
        /// <summary>
        /// Фронтальный перенумератор сетки по координатам 
        /// </summary>
        public override void FrontRenumberation(ref IFEMesh NewMesh, IFEMesh mesh, Direction direction = Direction.toRight)
        {
            try
            {
                SKnot.direction = direction;
                Set(mesh);
                SKnot[] knots = null;
                MEM.Alloc(X.Length, ref knots, "knots");
                for (int i = 0; i < X.Length; i++)
                    knots[i] = new SKnot(X[i], Y[i], i);
                
                Array.Sort(knots);
                
                int[] NewNumber = null;
                MEM.VAlloc(knots.Length, -1, ref NewNumber);
                int NewIndex = 0;
                for (int i = 0; i < knots.Length; i++) // по узлам в хеше
                {
                    int Old = knots[i].ID;
                    NewNumber[Old] = NewIndex;
                    NewIndex++;
                }
                DoRenumberation(ref NewMesh, NewNumber, direction);
            }
            catch(Exception ex)
            {
                Logger.Instance.Exception(ex);
                Logger.Instance.Info("ОШИБКА: Перенумерации сетки классом FERenumberatorHash!");
            }
        }
    }
}
