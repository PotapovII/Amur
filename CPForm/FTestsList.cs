namespace CPForm
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;

    public partial class FTestsList : Form
    {
        public int GetTaskID() => lb_DefaultTasks.SelectedIndex;
        public FTestsList(List<string> TaskNames)
        {
            InitializeComponent();
            lb_DefaultTasks.Items.AddRange(TaskNames.ToArray());
            lb_DefaultTasks.SelectedIndex = 0;
        }

        private void bt_Select_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void lb_DefaultTasks_DoubleClick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            Close();
        }
    }
}
