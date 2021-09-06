using System;
using System.Web.UI.DataVisualization.Charting;

namespace WorkWirhForm
{
    public class FailureModel//модель отказов
    {
        //то что получаем как параметры 
        private double _lambdaB;//базовая интенсивность отказов в режиме работы
        private double _resourceRY;//95-процентный ресурс (измеряется в часах)
        private double _resourceNM; //минимальная наработка (измеряется в часах)
        private double _resourceHrM;//минимальный срок сохраняемости (измеряется в годах)

        private double _coefficientTX;
        private double _coefficientE;
        private double[] _coefficients;
        //переменные для расчета площади
        private double[] _column;
        private double[] _area;
        private double[] _ratio;
        private double _step;
        private double _sumTime;




        //то что нужно посчиать
        public double LambdaE { get; }//Значение интенсивности отказов в режиме работы
        public double CoefficientV { get; }//коэффициента вариации
        public double ExpectedValue { get; }//мат. ожидание    
        public double Variance { get; }//дисперсия
        public double ResourceRMax { get; }//максимального ресурса
        public double ResourceLambda { get; }//значение для перехода от экспонициального распределения к нормальному =91079
        public double X1 { get; }//значение x1
        public double ResourceHrMax { get; }
        public double ResourceOzhMax { get; }//срок сохраняемости к заданным условиям режима хранения
        public double CoefficientX { get; }//коэффицент для определения x2
        public double X2 { get; }//значение x2
        //public double AvarageTime { get; }//среднее время



        public FailureModel(double lambdaB, double resourceRY, double resourceNM, double resourceHrM, double coefficientTX, double coefficientE, params double[] coefficients)
        {
            //обьявляем наши значения из конструктора
            _lambdaB = lambdaB;
            _resourceRY = resourceRY;
            _resourceNM = resourceNM;
            _resourceHrM = resourceHrM;

            _coefficientTX = coefficientTX;
            _coefficientE = coefficientE;
            _coefficients = coefficients;

            //записываем наше значение полей, которые считаем по формулаv, которые выведены в отдельные методы
            LambdaE = SetLambdaE();
            CoefficientV = SetKoefitsentV();
            ExpectedValue = SetExpectedValue();
            Variance = SetVariance();
            ResourceRMax = SetResourceRMax();
            ResourceLambda = SetResourceLambda();
            X1 = SetX1();
            ResourceHrMax = SetResourceHrMax();
            ResourceOzhMax = SetResourceOzhMax();
            CoefficientX = SetCoefficientX();
            X2 = SetX2();
        }
        //формулы
        private double SetLambdaE()
        {
            double sum = _lambdaB * _coefficientE;

            foreach (var item in _coefficients)
                sum *= item;

            return sum;
        }
            //_lambdaB * _coefficientR * _coefficientF * _coefficientS * _coefficientE * _coefficientPR;
        private double SetKoefitsentV() =>
            (_resourceRY - _resourceNM) / (-1.645 * _resourceNM - (-3.09) * _resourceRY);
        private double SetExpectedValue() =>
            _resourceRY / (1 + CoefficientV * (-1.645));
        private double SetVariance() =>
            CoefficientV * ExpectedValue;
        private double SetResourceRMax() =>
            (1 + CoefficientV * 3.09) * ExpectedValue;
        private double SetResourceLambda()
        {
            double accuracy = 1e-8;//точность
            double min = _resourceNM;//потому что результат где-то между этими значениями
            double max = ResourceRMax;

            double x = 0;
            while (max - min > accuracy)
            {
                // Вычисляем середину интервала.
                x = (min + max) / 2;
                // Найдём новый интервал, в котором функция меняет знак.
                if (F(max) * F(x) < 0)
                    min = x;
                else
                    max = x;
            }
            return x;
        }
        //функция которую нужно посчитать преобразованная для удобства
        public double F(double x)=>
            0.5 - Math.Exp(-LambdaE * (x - _resourceNM)) -  1.002/ 2 * Erf((x - ExpectedValue) / (Variance * Math.Sqrt(2)));
        //реализация Erf она появлятся при раскрытии интеграла
        private double Erf(double x)
        {
            // constants
            double a1 = 0.254829592;
            double a2 = -0.284496736;
            double a3 = 1.421413741;
            double a4 = -1.453152027;
            double a5 = 1.061405429;
            double p = 0.3275911;

            // Save the sign of x
            int sign = 1;
            if (x < 0)
                sign = -1;
            x = Math.Abs(x);

            double t = 1.0 / (1.0 + p * x);
            double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);

            return sign * y;
        }
        private double SetX1() =>
            1-Math.Exp(-LambdaE * (ResourceLambda - _resourceNM));
        private double SetResourceHrMax() =>
            (1 + CoefficientV * 3.09) / (1 - CoefficientV * 3.09) * _resourceHrM * 24 * 365;
        private double SetResourceOzhMax() =>
            ResourceHrMax / (_coefficientTX * _coefficientE);
        private double SetCoefficientX() =>
            (ResourceOzhMax / ExpectedValue - 1) / CoefficientV;
        private double SetX2()
        {
            if (CoefficientX > 3.09)
                return 1;
            else 
                return F(CoefficientX);
        }

        private double GetRealValue(double x) 
        {

            if (x <= 0.001)
                return _resourceNM;
            if (0.001 < x && x <= X1)
                return -Math.Log(1 - x) / LambdaE + _resourceNM;
            if (X1 < x && x <= X2)
            {
                if (x > 0.9999)
                    return (new Chart()).DataManipulator.Statistics.InverseNormalDistribution(0.9998) * Variance + ExpectedValue;
                return (new Chart()).DataManipulator.Statistics.InverseNormalDistribution(x) * Variance + ExpectedValue;
            }
            return double.MaxValue;
        }

        //первый 
        public double[] GetFirstPoints(uint N, out double step) 
        {
            Random rnd = new Random();
            _column = new double[2+(int)Math.Log(N,2)];

            step = ResourceRMax /( _column.Length-1);
            uint mistake = 0;

            _step = step;

            for (int i = 0; i < N; i++)
            {
                double z = GetRealValue(rnd.NextDouble());
                if (z > ResourceRMax) 
                {
                    mistake++;
                    continue;
                }
                _sumTime += z;
                _column[1+(int)(z / step)]++;
            }
            //нормируем
            for (int i = 0; i < _column.Length; i++)
            {
                _column[i] /= (N-mistake);
            }
            return _column;

        }
        //второй график
        public double[] GetSecondPoints()
        {

            _area = new double[_column.Length];
            ////метод прямоугольников
            //for (int i = 1; i <column.Length; i++)
            //{
            //    area[i] = area[i - 1] + column[i - 1] * _step;
            //}
            for (int i = 1; i < _column.Length; i++)
            {
                _area[i] = _column[i] + _area[i - 1];
            }

            return _area;
        }

        //третий график
        public double[] GetThirdPoints()
        {
            _ratio = new double[_column.Length-1];
            for (int i = 0; i <_column.Length-1; i++)
            {
                _ratio[i] = _column[i] / (1 - _area[i]);
            }
            return _ratio;
        }

        public double GetZ1(uint t) =>
            _area[(int)(t / _step)];

        public double GetAvarageTime(uint n) =>
            _sumTime / n;

    }
}
