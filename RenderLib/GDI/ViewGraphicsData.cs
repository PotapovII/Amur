using System.Windows.Forms;

namespace RenderLib
{
    using CommonLib;
    /// <summary>
    /// Менеджер отрисовки данных
    /// </summary>
    public partial class FVCurves : Form
    {
        public FVCurves(ISavePoint sp = null)
        {
            InitializeComponent();
            SetSavePoint(sp);
        }
        /// <summary>
        /// Метод синхронизирует передачу объекта SavePoint между разными потоками
        /// (потока вычислений и потока контрола)
        /// </summary>
        /// <param name="sp"></param>
        private void SetSavePoint(ISavePoint sp)
        {
            Text = sp.Name;
            if (this.InvokeRequired)
            {
                SendParam d = new SendParam(gdI_Curves_Control1.SendSavePoint);
                this.Invoke(d, new object[] { sp });
            }
            else
                gdI_Curves_Control1.SendSavePoint(sp);
        }

    }
}
