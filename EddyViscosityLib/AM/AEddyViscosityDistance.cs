//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 14.01.2025 Потапов И.И.
//---------------------------------------------------------------------------
namespace EddyViscosityLib
{
    using System;

    using CommonLib;
    using CommonLib.Mesh;
    using CommonLib.EddyViscosity;
    /// <summary>
    /// Профиль турбулентной вязкости Прандтль 1934
    /// </summary>
    [Serializable]
    public abstract class AEddyViscosityDistance : AlgebraEddyViscosityTri
    {
        public double[] Hp = null;
        public double[] Distance = null;
        /// <summary>
        /// Конструктор 
        /// </summary>
        public AEddyViscosityDistance(ETurbViscType eTurbViscType, BEddyViscosityParam p, TypeTask tt)
            : base(eTurbViscType, p, tt) {}
        public override void SetTask(IMesh mesh, IAlgebra algebra, IMeshWrapper wMesh = null)
        {
            base.SetTask(mesh, algebra, wMesh);
            wMesh.CalkDistance(ref Distance,ref Hp); 
        }
    }
}
