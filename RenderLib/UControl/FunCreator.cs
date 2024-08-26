//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 17.06.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace RenderLib.Fields
{
    using System;
    using System.Windows.Forms;
    using MemLogLib;
    using GeometryLib;
    using CommonLib.Function;
    using System.ComponentModel;
    using CommonLib.EConverter;
    using CommonLib.Geometry;

    public partial class FunCreator : UserControl
    {
        public FunCreator()
        {
            InitializeComponent();
            FunName = "Функция";
        }
        /// <summary>
        /// Гладкость функции
        /// </summary>
        [Description("Вид интерполяции")]
        [TypeConverter(typeof(MyEnumConverter))]
        public SmoothnessFunction FunSmoothness { get; set; } = SmoothnessFunction.linear;
        /// <summary>
        /// Параметрическая функция да или нет
        /// </summary>
        [Description("Параметрическая функция")]
        [TypeConverter(typeof(BooleanTypeConverterYN))]
        public bool FunParametric { get; set; } = false;
        /// <summary>
        /// Название функции
        /// </summary>
        [Description("Название функции")]
        public string FunName 
        { 
            get => lbName.Text;  
            set => lbName.Text = value;
        }

        private void cbEvil_CheckedChanged(object sender, EventArgs e)
        {
            if(cbEvil.Checked == true)
            {
                lblArg.Text = "Время";
                labFun.Text = "Значение";
            }
            else
            {
                lblArg.Text = "Аргумент";
                labFun.Text = "Функция";
            }
        }
        private void btAdd_Click(object sender, EventArgs e)
        {
            AddOrEdit(true);
        }
        private void btEdit_Click(object sender, EventArgs e)
        {
            AddOrEdit(false);
        }

        private void AddOrEdit(bool addFlag = true)
        {
            int mark = 0;
            double xLast = double.MinValue;
            double xNext = double.MaxValue;
            try
            {
                if (tbArg.Text != "" && tbFun.Text != "")
                {
                    string arg = tbArg.Text;
                    string fun = tbFun.Text;
                    double x = double.Parse(tbArg.Text, MEM.formatter);
                    mark++;
                    double y = double.Parse(tbFun.Text, MEM.formatter);

                    if (addFlag == true)
                    {
                        if (lbArg.Items.Count > 0)
                            xLast = double.Parse(lbArg.Items[lbArg.Items.Count - 1].ToString(), MEM.formatter);

                        if (x > xLast || FunParametric == true || lbArg.Items.Count == 0)
                        {
                            lbArg.Items.Add(arg);
                            lbFun.Items.Add(fun);
                            if (lbArg.Items.Count > 0)
                            {
                                lbArg.SelectedIndex = lbArg.Items.Count - 1;
                                lbFun.SelectedIndex = lbArg.Items.Count - 1;
                            }
                        }
                        else
                        {
                            MessageBox.Show("Добавление не возможно!", "Не корректные аргумент");
                            tbArg.Select();
                        }
                    }
                    else
                    {
                        int idx = lbArg.SelectedIndex;
                        if (idx - 1 > -1)
                            xLast = double.Parse(lbArg.Items[idx - 1].ToString(), MEM.formatter);
                        if (idx + 1 < lbArg.Items.Count - 1)
                            xNext = double.Parse(lbArg.Items[idx + 1].ToString(), MEM.formatter);
                        if ((x > xLast && x < xNext) || FunParametric == true || lbArg.Items.Count == 0)
                        {
                            //lbArg.Items[idx] = tbArg.Text;
                            //lbFun.Items[idx] = tbFun.Text;
                            //lbArg.Items.RemoveAt(idx);
                            //lbArg.Items.Insert(idx, tbArg.Text);
                            //lbFun.Items.RemoveAt(idx);
                            //lbFun.Items.Insert(idx, tbFun.Text);
                        }
                        else
                        {
                            MessageBox.Show("Добавление не возможно!", "Не корректные аргумент");
                            tbArg.Select();
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("Добавление не возможно!", "Не корректные данные в поле");
                if (mark == 0)
                    tbArg.Select();
                else
                    tbFun.Select();
            }
        }
        private void btDel_Click(object sender, EventArgs e)
        {
            int idx = lbArg.SelectedIndex;
            if (idx > -1)
            {
                lbArg.Items.Remove(lbArg.Items[idx]);
                lbFun.Items.Remove(lbFun.Items[idx]);
                if(idx!=0)
                {
                    lbArg.SelectedIndex = idx - 1;
                    lbFun.SelectedIndex = idx - 1;
                }
            }
        }
        /// <summary>
        /// Получить табличную функцию
        /// </summary>
        /// <returns></returns>
        public IDigFunction CreateFunction()
        {
            IDigFunction fun = new DigFunction(FunName, FunSmoothness, FunParametric);
            for(int i = 0; i < lbArg.Items.Count; i++)
            {
                double x = double.Parse(lbArg.Items[i].ToString(), MEM.formatter);
                double y = double.Parse(lbFun.Items[i].ToString(), MEM.formatter);
                fun.Add(x, y);
            }
            return fun;
        }
        /// <summary>
        /// Загрузка
        /// </summary>
        /// <param name="arg"></param>
        public void SetFunction(double[][] arg)
        {
            for (int i = 0; i < arg[0].Length; i++)
            {
                lbArg.Items.Add(arg[0][i].ToString());
                lbFun.Items.Add(arg[1][i].ToString());
            }
        }
        public void SetFunction(IHPoint[] arg)
        {
            for (int i = 0; i < arg.Length; i++)
            {
                lbArg.Items.Add(arg[i].X.ToString());
                lbFun.Items.Add(arg[i].Y.ToString());
            }
        }
        public void SetFunction(IFPoint[] arg)
        {
            for (int i = 0; i < arg.Length; i++)
            {
                lbArg.Items.Add(arg[i].X.ToString());
                lbFun.Items.Add(arg[i].Y.ToString());
            }
        }

        private void lbArg_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(lbArg.SelectedIndex != lbFun.SelectedIndex)
                lbFun.SelectedIndex = lbArg.SelectedIndex;
            tbArg.Text = lbArg.Items[lbArg.SelectedIndex].ToString();
            tbFun.Text = lbFun.Items[lbArg.SelectedIndex].ToString();
        }

        private void lbFun_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbArg.SelectedIndex != lbFun.SelectedIndex)
                lbArg.SelectedIndex = lbFun.SelectedIndex;
            tbArg.Text = lbArg.Items[lbArg.SelectedIndex].ToString();
            tbFun.Text = lbFun.Items[lbArg.SelectedIndex].ToString();
        }

    }
}
