namespace CPForm
{
    using ChannelProcessLib;
    using CommonLib;
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;

    public partial class FTaskManager : Form
    {

        FormLogger fLogger = null;
        TypeTask typeTask = TypeTask.streamX1D;
        ManagerRiverTask mrt = new ManagerRiverTask();
        ManagerBedLoadTask mbt = new ManagerBedLoadTask();

        List<TaskMetka> mbload = new List<TaskMetka>();
        List<TaskMetka> mriver = new List<TaskMetka>();
        public FTaskManager()
        {
            InitializeComponent();
            lb_river.SelectedIndex = 0;
            lb_bload.SelectedIndex = 0;
        }

        public void GetSetList(TypeTask tt)
        {
            lb_river.Items.Clear();
            mriver = mrt.GetStreamNameBLTask(tt);
            foreach (var e in mriver)
                lb_river.Items.Add(e.Name);
            lb_river.SelectedIndex = 0;

            lb_bload.Items.Clear();
            mbload = mbt.GetStreamNameBLTask(tt);
            foreach (var e in mbload)
                lb_bload.Items.Add(e.Name);
            lb_bload.SelectedIndex = 0;
        }

        private void FTaskManager_Load(object sender, EventArgs e)
        {
            GetSetList(typeTask);
        }

        private void rb1Y_Click(object sender, EventArgs e)
        {
            if (rb1X.Checked == true)
                typeTask = TypeTask.streamX1D;
            if (rb1Y.Checked == true)
                typeTask = TypeTask.streamY1D;
            if (rb2XY.Checked == true)
                typeTask = TypeTask.streamXY2D;
            GetSetList(typeTask);
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            int idxRiver = mriver[lb_river.SelectedIndex].id;
            int idxBload = mbload[lb_bload.SelectedIndex].id;
            if (fLogger == null)
            {
                fLogger = new FormLogger();
                fLogger.Show();
            }
            TaskForm ftask = new TaskForm(fLogger, idxRiver, idxBload, mrt, mbt);
            ftask.Show();
        }
    }
}
