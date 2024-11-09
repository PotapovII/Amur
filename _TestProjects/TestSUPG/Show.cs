namespace TestSUPG
{
    using MeshLib;
    using RenderLib;
    using CommonLib;
    using System.Windows.Forms;

    public class SHOW
    {
        /// <summary>
        /// Отрисовка полей задачи гидродинамики
        /// </summary>
        /// <param name="mesh">сетка задачи</param>
        /// <param name="mVx">скорость по X</param>
        /// <param name="mVy">скорость по Y</param>
        /// <param name="mVz">скорость по Z</param>
        /// <param name="meddyViscosity">вязкость</param>
        /// <param name="TauXX">напряжения ...</param>
        /// <param name="TauXY">напряжения ...</param>
        /// <param name="TauXZ">напряжения ...</param>
        /// <param name="Psi">функция тока</param>
        /// <param name="mVortex">функция вихря</param>
        /// <param name="mDistance">расстояние до стенки</param>
        public static void ShowCFD(IMesh mesh, double[] mVx, double[] mVy, double[] mVz, double[] meddyViscosity,
         double[] TauXX, double[] TauXY, double[] TauXZ, double[] Psi, double[] mVortex, double[] mDistance = null)
        {
            SavePoint data = new SavePoint();
            data.SetSavePoint(0, mesh);

            double[] x = mesh.GetCoords(0);
            double[] y = mesh.GetCoords(1);

            if (mVx != null)
                data.Add("Скорость Vx", mVx);
            if (mVy != null)
                data.Add("Скорость Vy", mVy);
            if (mVz != null)
                data.Add("Скорость Vz", mVz);
            if (mVy != null && mVz != null)
                data.Add("Скорость", mVy, mVz);
            if (meddyViscosity != null)
                data.Add("Турбулентная вязкость", meddyViscosity);

            if (Psi != null)
                data.Add("Psi", Psi);
            if (mVortex != null)
                data.Add("mVortex", mVortex);
            if (TauXX != null)
                data.Add("tauXX", TauXX);
            if (TauXY != null)
                data.Add("TauXY", TauXY);
            if (TauXZ != null)
                data.Add("TauXZ", TauXZ);
            if (TauXZ != null && TauXY != null)
                data.Add("TauX", TauXY, TauXZ);

            if (mDistance != null)
                data.Add("mDistance", mDistance);

            data.Add("Координата Х", x);
            data.Add("Координата Y", y);

            Form form = new ViForm(data);
            form.Show();
        }
    }
}
