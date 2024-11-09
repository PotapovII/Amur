//---------------------------------------------------------------------------
//                          ПРОЕКТ  "МКЭ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 21.06.2022 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshLib
{
    using System;
    /// <summary>
    /// Напрваление от границы области
    /// </summary>
    [Serializable]
    public enum DirectMeshIndex
    {
        P = 0,
        S = 1,
        E = 2,
        N = 3,
        W = 4
    }
    /// <summary>
    /// Связь
    /// </summary>
    [Serializable]
    public struct MeshIndex
    {
        public DirectMeshIndex d;
        public int i;
        public int j;
        public MeshIndex(int i, int j, DirectMeshIndex d = DirectMeshIndex.P)
        {
            this.i = i;
            this.j = j;
            this.d = d;
        }
        public new string ToString()
        {
            return i.ToString() + " " + j.ToString() + " " + ((int)d).ToString();
        }
    }
}
