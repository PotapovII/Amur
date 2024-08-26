using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using ConnectLib;

namespace RiverDB
{
    public partial class FConnectPath : Form
    {
        public List<string> list = new List<string>();
        public FConnectPath()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            string Message = "";
            string connectString = listBox1.SelectedItem.ToString();
            if (ConnectDB.ConnectStringTest(connectString, ref Message))
            {
                ConnectPath.connectString = listBox1.SelectedItem.ToString();
                textBox1.Text = ConnectPath.connectString;
                listBox1.Items.Clear();
                File.WriteAllText("pathconfig.txt", String.Empty);
                using (StreamWriter sw = new StreamWriter("pathconfig.txt"))
                {
                    string line = ConnectPath.connectString;
                    sw.WriteLine(line);

                    foreach (string l in list)
                    {
                        if (l != ConnectPath.connectString)
                        {
                            sw.WriteLine(l);
                        }
                    }
                }
                list.Clear();
                Close();
            }
            else
            {
                textBox1.Text = Message;
            }
        }




        private void FConnectPath_Load(object sender, EventArgs e)
        {
            using (StreamReader sr = new StreamReader($"LinkStr\\pathconfig.txt"))
            {
                listBox1.Items.Clear();
                string s;
                while ((s = sr.ReadLine()) != null)
                {
                    listBox1.Items.Add(s);
                    list.Add(s);
                }
                sr.Close();
                listBox1.SelectedIndex = 0;
                ConnectPath.connectString = listBox1.SelectedItem.ToString();
                textBox1.Text = ConnectPath.connectString;
            }
        }
    }
}
