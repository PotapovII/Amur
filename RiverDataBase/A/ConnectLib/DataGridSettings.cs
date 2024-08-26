
namespace ConnectLib
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
