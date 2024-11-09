namespace MeshLib.Locators
{
    using System;
    using CommonLib;
    using MemLogLib;
    public class LocatorRect
    {
        IMesh mesh = null;
        public LocatorRect(IMesh mesh)
        {
            this.mesh = mesh;
        }
        public bool CheckCreate(ref RectFVMesh qmesh)
        {
            bool flag = false;
            TriMesh m = mesh as TriMesh;
            if (m != null)
            {
                double[] x = mesh.GetCoords(0);
                double[] y = mesh.GetCoords(1);
                double Left = x[0];
                double Right = x[0];
                double Bottom = y[0];
                double Top = y[0];
                for (int i = 1; i < x.Length; i++)
                {
                    Left = Math.Min(Left, x[i]);
                    Right = Math.Min(Right, x[i]);
                    Bottom = Math.Min(Bottom, y[i]);
                    Top = Math.Max(Top, y[i]);
                }
                double[] xx = null;
                double[] yy = null;
                int CountX = MEM.CountUniqueElems(x, ref xx);
                int CountY = MEM.CountUniqueElems(y, ref yy);
                qmesh = new RectFVMesh(CountX - 1, CountY - 1, Left, Right, Bottom, Top);
            }
            else
                Logger.Instance.Error("mesh as TriMesh == null", "Locator_TriMeshToQuad.Locator_TriMeshToQuad()");
            
            return flag;
        }
    }
}
