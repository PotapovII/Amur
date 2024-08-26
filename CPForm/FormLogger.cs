namespace CPForm
{
    using System;
    using System.Windows.Forms;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MemLogLib;
    using MemLogLib.Delegate;

    public partial class FormLogger : Form
    {
        /// <summary>
        /// Журнал задачи
        /// </summary>
        ILogger<IMessageItem> logger = Logger.Instance;

        public FormLogger()
        {
            InitializeComponent();
            cbLogType.SelectedIndex = 0;
            logger.Clear();
        }

        private void FormLogger_FormClosing(object sender, FormClosingEventArgs e)
        {
            //// Форму закрывает пользователь по клику на крестик, комбинацией Alt+F4 или по клику "закрыть" контекстного меню.
            //if (e.CloseReason == CloseReason.UserClosing)
            //{
            //    // Отменить закрытие формы
            //    e.Cancel = true;
            //    // Вместо закрытия — свернуть
            //    WindowState = FormWindowState.Minimized;
            //}
            //base.OnFormClosing(e);
        }

        private void FormLogger_Resize(object sender, EventArgs e)
        {
            //// Если программу свернули, то убрать ее из панели задач и показать в трее иконку
            //if (WindowState == FormWindowState.Minimized)
            //{
            //    ShowInTaskbar = false;
            //    notifyIcon.Visible = true;
            //}
            //else
            //{
            //    ShowInTaskbar = true;
            //    notifyIcon.Visible = false;
            //}
            //base.OnResize(e);
        }

        private void FormLogger_Load(object sender, EventArgs e)
        {
            if(logger.sendMessage==null)
                logger.sendMessage = SetMessageItem;
            if (logger.sendHeader == null)
                logger.sendHeader = SetHeadderIndo;
            tbHeader.Text = logger.HeaderInfo;
        }
        /// <summary>
        /// Сообщение стороннему наблюдателю
        /// </summary>
        /// <param name="mes"></param>
        public void mSendLog(IMessageItem mes)
        {
            lb_log.Items.Add(mes.ToString());
        }
        public void mSendHeader(string mes)
        {
            tbHeader.Text = mes;
        }
        /// <summary>
        /// Синхронизация потоков
        /// </summary>
        /// <param name="mes"></param>
        private void SetMessageItem(IMessageItem mes)
        {
            if (this.InvokeRequired)
            {
                SendLog d = new SendLog(mSendLog);
                this.Invoke(d, new object[] { mes });
            }
            else
                mSendLog(mes);
        }
        private void SetHeadderIndo(string mes)
        {
            if (this.InvokeRequired)
            {
                SendHeader d = new SendHeader(mSendHeader);
                this.Invoke(d, new object[] { mes });
            }
            else
                mSendHeader(mes);
        }
        private void button2_Click(object sender, EventArgs e)
        {
            lb_log.Items.Clear();
        }

        private void cbLogType_SelectedIndexChanged(object sender, EventArgs e)
        {
            lb_log.Items.Clear();
            IList<IMessageItem> log = logger.Data;
            foreach (IMessageItem mes in log)
            {
                if (mes.LevelMessage == (TypeMessage)cbLogType.SelectedIndex ||
                    cbLogType.SelectedIndex == 0)
                    lb_log.Items.Add(mes.ToString());
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }

}
