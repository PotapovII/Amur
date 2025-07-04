﻿//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 11.06.2021 Потапов И.И.
//---------------------------------------------------------------------------
using System.Windows.Forms;

namespace RenderLib
{
    using CommonLib;
    using MeshLib;

    public partial class ViForm : Form
    {
        public bool CloseDO = false;
        public ViForm(ISavePoint sp = null)
        {
            InitializeComponent();
            SetSavePoint(sp);
            gdI_Control1.AddOwner(this);
        }
        /// <summary>
        /// Метод синхронизирует передачу объекта SavePoint между разными потоками
        /// (потока вычислений и потока контрола)
        /// </summary>
        /// <param name="sp"></param>
        public void SetSavePoint(ISavePoint sp)
        {
            Text = sp.Name;
            if (this.InvokeRequired == true)
            {
                SendParam d = new SendParam(gdI_Control1.SendSavePoint);
                this.Invoke(d, new object[] { sp });
            }
            else
                gdI_Control1.SendSavePoint(sp);
        }

        private void ViForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseDO = true;
        }

        public void Start1(string txt)
        {
            Text = txt;
            Show();
        }
        public static void ShowMesh(IMesh mesh)
        {

            if (mesh != null)
            {
                SavePoint data = new SavePoint();
                data.SetSavePoint(0, mesh);
                double[] xx = mesh.GetCoords(0);
                double[] yy = mesh.GetCoords(1);
                data.Add("Координата Х", xx);
                data.Add("Координата Y", yy);
                data.Add("Координаты ХY", xx, yy);
                Form form = new ViForm(data);
                form.Show();
            }

        }
    }
}
