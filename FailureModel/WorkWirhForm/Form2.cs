using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WorkWirhForm
{
    public partial class Form2 : Form
    {
        FailureModel _model;
        public Form2()
        {
            InitializeComponent();
        }
        public Form2(FailureModel model) 
        {
            InitializeComponent();
            _model = model;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!uint.TryParse(textBox1.Text, out uint t)) 
            { MessageBox.Show("не смог преобразовать значние для t"); return; }

            if (t > _model.ResourceRMax) 
            { MessageBox.Show("Значение, которое вы ввели больше допустимого");return; }

            label2.Text = "вероятность отказа:" + _model.GetZ1(t);
        }
    }
}
