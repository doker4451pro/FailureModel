using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace WorkWirhForm
{
    public partial class Model : Form
    {
        private FailureModel _model;
        public Model()
        {
            InitializeComponent();
        }

        private void PlotChart1(double[] points, int step, string line, Chart chart, string legend, string textX, string textY)
        {
            ChartArea area = new ChartArea(line);

            if (chart.ChartAreas.IsUniqueName(line)) 
            {
                chart.ChartAreas.Add(area);
                chart.ChartAreas[1].AxisX.Title = textX;
                chart.ChartAreas[1].AxisY.Title = textY;
                chart.Titles.Add(legend);
            }
            else
                chart.Series.Clear();


            Series series1 = new Series();
            series1.ChartType = SeriesChartType.Spline;
            series1.ChartArea = line;

            for (int i = 0; i < points.Length; i++)
                series1.Points.AddXY(i * step, points[i]);

            chart.Series.Add(series1);

            chart.ChartAreas[1].AxisX.Minimum = 0;//чтобы отсчет начинался с 0
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!double.TryParse(textBox1.Text, out double lambdaB))
            { MessageBox.Show("не смог преобразовать значние для lambdaB"); return; }

            if (!double.TryParse(textBox2.Text, out double resourceRY))
            { MessageBox.Show("не смог преобразовать значние для resourceRY"); return; }

            if (!double.TryParse(textBox3.Text, out double resourceNM))
            { MessageBox.Show("не смог преобразовать значние для resourceNM"); return; }

            if (!double.TryParse(textBox4.Text, out double resourceHrM))
            { MessageBox.Show("не смог преобразовать значние для resourceHrM"); return; }

            //if (!double.TryParse(textBox4.Text, out double coefficenR))
            //{ MessageBox.Show("не смог преобразовать значние для coefficenR"); return; }

            //if (!double.TryParse(textBox5.Text, out double coeffcicenF))
            //{ MessageBox.Show("не смог преобразовать значние для coeffcicenF"); return; }

            //if (!double.TryParse(textBox6.Text, out double coefficenS))
            //{ MessageBox.Show("не смог преобразовать значние для coefficenS"); return; }

            if (!double.TryParse(textBox8.Text, out double coefficenE))
            { MessageBox.Show("не смог преобразовать значние для coefficenE"); return; }

            //if (!double.TryParse(textBox7.Text, out double coefficenPR))
            //{ MessageBox.Show("не смог преобразовать значние для coefficenPR"); return; }

            if (!double.TryParse(textBox7.Text, out double coefficenT))
            { MessageBox.Show("не смог преобразовать значние для coefficenT"); return; }

            if (!uint.TryParse(textBox6.Text, out uint n))
            { MessageBox.Show("не смог преобразовать значние для n"); return; }

            var otherСoefficients = new List<double>();
            try
            {
                string[] stringOtherСoefficients;
                //удаляем все пробелы и делим по ;
                stringOtherСoefficients = textBox5.Text.Trim().Split(';');

                foreach (var item in stringOtherСoefficients)
                    otherСoefficients.Add(double.Parse(item));
            }
            catch
            {
                MessageBox.Show("не смог преобразовать значение остальных коэфиетов");
                return;
            }

            //if (!uint.TryParse(textBox12.Text, out uint t)) 
            //{ MessageBox.Show("не смог преобразовать значние для t"); return; }

            //задаем нашу модель
            _model = new FailureModel(lambdaB, resourceRY, resourceNM, resourceHrM, coefficenT, coefficenE, otherСoefficients.ToArray());


            //MessageBox.Show($"{model.LambdaE}\n{model.CoefficientV}\n{model.ExpectedValue}\n{model.Variance}\n{model.ResourceRMax}\n{model.CoefficientC}\n{model.ResourceLambda}\n{model.X1}\n{model.ResourceHrMax}\n{model.ResourceOzhMax}\n{model.CoefficientX}\n{model.X2}");

            MessageBox.Show($"Лабда Е: {_model.LambdaE}\n" +
                $"V : {_model.CoefficientV}\n" +
                $"m(tр): {_model.ExpectedValue}\n" +
                $"σ(tр):{_model.Variance}\n" +
                $"Tр.max: {_model.ResourceRMax}\n" +
                $"Tλ: {_model.ResourceLambda}\n" +
                $"Tхр.max: {_model.ResourceHrMax}\n" +
                $"Tож.max: {_model.ResourceOzhMax}\n" +
                $"χ: {_model.CoefficientX}");



            double step;
            PlotChart1(_model.GetFirstPoints(n, out step), (int)step, "F1", chart1, "плотность распределения", "Время экспулатации", "f(x)");

            PlotChart1(_model.GetSecondPoints(), (int)step, "F2", chart2, "вероятность отказов", "Время экспулатации", "F(x)");

            PlotChart1(_model.GetThirdPoints(), (int)step, "F3", chart3, "интенсивность отказов", "Время экспулатации", "λ‎(x)");

            label4.Text = "Tpmax= " + _model.ResourceRMax;
            label8.Text = "средняя наработка на отказ: " + _model.GetAvarageTime(n);

            button2.Visible = true;
        }

        private void button2_Click(object sender, EventArgs e) =>
            new Form2(_model).Show();
    }
}
