//---------------------------------------------------------------------------
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//        кодировка : 04.10.2020 Потапов И.И. & Потапов Д.И.
//---------------------------------------------------------------------------
namespace RiverDB.FormsDB
{
    using System;
    using System.Data;
    using System.Windows.Forms;
    using ConnectLib;

    public partial class LevelsRiverUp : Form
    {
        public string filename;
        public string TName;
        int id;
        public LevelsRiverUp(int id = -1)
        {
            this.id = id;
            InitializeComponent();
        }

        private void bt1_Click(object sender, EventArgs e)
        {
            Catalogue book = new Catalogue("place", true);
            book.ShowDialog();
            tb2.Text = book.RowID;
            tb21.Text = book.RowName;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (tb2.Text.Trim() != "" && tb3.Text.Trim() != "" && tb4.Text.Trim() != "")
            {
                string strCom = "";
                DateTime DT = dtp1.Value;
                string FDT = DT.ToString("yyyy-MM-dd HH:mm:ss.fff");
                if (id == -1)
                    strCom = "exec SP_" + TName + "_ADD '" + tb2.Text + "','" + FDT + "','" + tb3.Text + "','" + tb4.Text + "';";
                else
                    strCom = "exec SP_" + TName + "_UPD '" + id.ToString() + "','" + tb2.Text + "','" + FDT + "','" + tb3.Text + "','" + tb4.Text + "';";
                ConnectDB.SQLCommandDo(strCom);
           }
        }

        private void LevelsRiverUp_Load(object sender, EventArgs e)
        {
            if (id != -1)
            {
                DataRow row = ConnectDB.GetDataRow(TName, id);
                tb3.Text = row["experiment_waterlevel"].ToString();
                tb4.Text = row["experiment_middlewaterlevel"].ToString();
                dtp1.Value = Convert.ToDateTime(row["Experiment_datetime"]);
                int a_id = Convert.ToInt32(row["place_id"]);
                tb2.Text = a_id.ToString();
                tb21.Text = ConnectDB.GetName("place", a_id);
            }
            this.Text = "Форма редактирования списка речных глубин по гидропостам";
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
