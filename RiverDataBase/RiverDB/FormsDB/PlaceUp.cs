//---------------------------------------------------------------------------
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//        кодировка : 04.10.2020 Потапов И.И. & Потапов Д.И.
//--------------------------------------------------------------------------
namespace RiverDB.FormsDB
{
    using ConnectLib;
    using System;
    using System.Data;
    using System.Windows.Forms;

    public partial class PlaceUp : Form
    {
        string TName;
        int id;
        public PlaceUp(string TName,int id = -1)
        {
            this.id = id;
            this.TName = TName; 
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (tb1.Text != null && tb3.Text != null && tb4.Text != null && tb7.Text != null && tb8.Text != null)
            {
                string strCom = "";
                if (id == -1)
                    strCom = "exec SP_" + TName + "_ADD '" + tb1.Text + "','" + tb3.Text + "','" + tb4.Text +
                        "','" + tb7.Text + "','" + tb8.Text + "';";
                else
                    strCom = "exec SP_" + TName + "_UPD '" + id.ToString() + "','" + tb1.Text + "','" + tb3.Text + "','" + tb4.Text +
                        "','"  + tb7.Text + "','" + tb8.Text + "';";
                ConnectDB.SQLCommandDo(strCom);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void PlaceUp_Load(object sender, EventArgs e)
        {
            if (id != -1)
            {
                DataRow row = ConnectDB.GetDataRow(TName, id);
                tb1.Text = row["place_name"].ToString();
                tb3.Text = row["place_x"].ToString();
                tb4.Text = row["place_y"].ToString();
                tb7.Text = row["place_height"].ToString();
                tb8.Text = row["place_nullheight"].ToString();
            }
            this.Text = "Форма редактирования списка участков";
        }
    }
}
