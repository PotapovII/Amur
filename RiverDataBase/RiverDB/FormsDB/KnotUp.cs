namespace RiverDB.FormsDB
{
    using System;
    using System.Data;
    using System.Windows.Forms;
    using ConnectLib;
    public partial class KnotUp : Form
    {
        public string filename;
        public string TName;
        int id;
        public KnotUp()
        {
            InitializeComponent();
        }
        public KnotUp(string TName, int id = -1)
        {
            this.id = id;
            this.TName = TName;
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (tb1.Text != null && tb2.Text != null && tb4.Text != null && tb5.Text != null && tb6.Text != null && tb7.Text != null && tb8.Text != null)
            {
                string strCom = "";
                DateTime DT = dtp1.Value;
                string FDT = DT.ToString("yyyy-MM-dd HH:mm:ss.fff");
                if (id == -1)
                    strCom = "exec SP_" + TName + "_ADD '" + tb1.Text + "','" + tb1.Text + "','" + tb2.Text + "','" + 
                              tb4.Text + "','" + tb5.Text + "','" + FDT + "','" + tb6.Text + "','"
                               + tb7.Text + "','" + tb6.Text + "','" + tb7.Text + "','" + tb8.Text + "';";
                else
                    strCom = "exec SP_" + TName + "_UPD '" + id.ToString() + "','" + tb1.Text + "','" + tb1.Text + "','" + 
                        tb2.Text + "','" + tb4.Text + "','" + tb5.Text + "','" + FDT + "','" + tb6.Text + "','"
                        + tb7.Text + "','" + tb6.Text + "','" + tb7.Text + "','" + tb8.Text + "';";
                ConnectDB.SQLCommandDo(strCom);
            }
        }

        private void KnotUp_Load(object sender, EventArgs e)
        {
            if (id != -1)
            {
                DataRow row = ConnectDB.GetDataRow(TName, id);
                tb1.Text = row["knot_fulldepth"].ToString();
                tb2.Text = row["knot_step"].ToString();
                tb4.Text = row["knot_speed"].ToString();
                tb5.Text = row["knot_course"].ToString();
                tb6.Text = row["knot_latitude"].ToString();
                tb7.Text = row["knot_longitude"].ToString();
                tb8.Text = row["knot_temperature"].ToString();
                dtp1.Value = Convert.ToDateTime(row["knot_datetime"]);
            }
            this.Text = "Форма редактирования списка узлов опорной сети";
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
