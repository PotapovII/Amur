using System;
using System.Windows.Forms;
namespace HelpLib
{
    public class TestHelp
    {
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new HelpForm(0));
        }
    }
}
