
namespace HelpLib
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Windows.Forms;
    public partial class HelpForm : Form
    {
        string[] FileTexts = { "H1.txt" };
        string[] FileFigs = { "F1.png" };
        int ID;
        public HelpForm(int HelpID)
        {
            InitializeComponent();
            ID = HelpID;
        }

        private void HelpForm_Load(object sender, EventArgs e)
        {
            try
            {
                string filePathT = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "HelTexts", FileTexts[ID]);
                string filePathF = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "HelpFigs", FileFigs[ID]);
                richTextBox1.Text = File.ReadAllText(filePathT);
                richTextBox1.Font = new Font("Times New Roman", 14);
                richTextBox1.Enabled = false;
                pictureBox1.Load(filePathF);
            }
            catch(Exception ex) 
            { 
                Console.WriteLine(ex.Message);            
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
