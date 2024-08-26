using System.Windows.Forms;
using CommonLib;

namespace RenderLib
{
    public static class ClientRender
    {
        public static void Show(ISavePoint data)
        {
            Form form = new ViForm(data);
            form.Show();
        }
    }
}
