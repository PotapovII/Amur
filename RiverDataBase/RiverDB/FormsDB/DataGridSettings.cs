//---------------------------------------------------------------------------
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//        кодировка : 01.10.2020 Потапов И.И. & Потапов Д.И.
//---------------------------------------------------------------------------
namespace RiverDB.FormsDB
{
    using System.Windows.Forms;
    public static class DataGridSettings
    {
        public static void DataGridViewSetup(ref DataGridView dataGridView1)
        {
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.ReadOnly = true;
        }
    }
}
