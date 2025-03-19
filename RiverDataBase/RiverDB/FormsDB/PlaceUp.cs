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
            if (tbName.Text != null && tb_X.Text != null && tb_Y.Text != null && tb_floodplain.Text != null && tb_Zerro.Text != null)
            {
                string strCom = "";
                if (id == -1)
                    strCom = "exec SP_" + TName + "_ADD '" + tbName.Text + "','" + tb_X.Text + "','" + tb_Y.Text +
                        "','" + tb_floodplain.Text + "','" + tb_Zerro.Text + "';";
                else
                    strCom = "exec SP_" + TName + "_UPD '" + id.ToString() + "','" + tbName.Text + "','" + tb_X.Text + "','" + tb_Y.Text +
                        "','"  + tb_floodplain.Text + "','" + tb_Zerro.Text + "';";
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
                tbName.Text = row["place_name"].ToString();
                tb_X.Text = row["place_x"].ToString();
                tb_Y.Text = row["place_y"].ToString();
                tb_floodplain.Text = row["place_height"].ToString();
                tb_Zerro.Text = row["place_nullheight"].ToString();
            }
            this.Text = "Форма редактирования списка участков";
        }
    }
}
