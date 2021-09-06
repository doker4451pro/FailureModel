using System;
using System.Numerics;

namespace ClassLibraryForFailureModels
{
    class FailureModel//модель отказов
    {
        //то что получаем как параметры 
        private double _lambdaB;//базовая интенсивность отказов в режиме работы
        private double _lambdaHrB;//базовая интенсивность отказов в режиме хранения
        private double _resourceRY;//95-процентный ресурс (измеряется в часах)
        private double _resourceNM; //минимальная наработка (измеряется в часах)
        private double _resourceHrM;//минимальный срок сохраняемости (измеряется в годах)
        private double _temperature;//максимальная температура переход (измеряется в градусах)

        private double _coefficientTX = 1.95;//
        private double _coefficientE = 1;//

        //то что нужно посчиать
        public double LambdaE { get; }//Значение интенсивности отказов в режиме работы
        public double CoefficientV { get; }//коэффициента вариации
        public double ExpectedValue { get; }//мат. ожидание    
        public double Variance { get; }//дисперсия
        public double ResourceRMax { get; }//максимального ресурса
        public double CoefficientC { get; }//нормирующий коэффициент
        public double ResourceLambda { get; } = 91079;//значение для перехода от экспонициального распределения к нормальному =91079
        public double X1 { get; }//значение x1
        public double ResourceHrMax { get; }
        public double ResourceOzhMax { get; }//срок сохраняемости к заданным условиям режима хранения
        public double CoefficientX { get; }//коэффицент для определения x2
        public double X2 { get; }//значение x2

        public FailureModel(double lambdaB, double lambdaHrB, double resourceRY, double resourceNM, double resourceHrM)
        {
            //обьявляем наши значения из конструктора
            _lambdaB = lambdaB;
            _lambdaHrB = lambdaHrB;
            _resourceRY = resourceRY;
            _resourceNM = resourceNM;
            _resourceHrM = resourceHrM;

            //записываем наше значение полей, которые считаем по формулаv, которые выведены в отдельные методы
            LambdaE = SetLambdaE();
            Console.WriteLine(LambdaE);
            CoefficientV = SetKoefitsentV();
            Console.WriteLine(CoefficientV);
            ExpectedValue = SetExpectedValue();
            Console.WriteLine(ExpectedValue);
            Variance = SetVariance();
            Console.WriteLine(Variance);
            ResourceRMax = SetResourceRMax();
            Console.WriteLine(ResourceRMax);
            //ResourceLambda = SetResourceLambda();
            Console.WriteLine(ResourceLambda);
            X1 = SetX1();
            Console.WriteLine(X1);
            ResourceHrMax = SetResourceHrMax();
            Console.WriteLine(ResourceHrMax);
            ResourceOzhMax = SetResourceOzhMax();
            Console.WriteLine(ResourceOzhMax);
            CoefficientX = SetCoefficientX();
            Console.WriteLine(CoefficientX);
            X2 = SetX2();
            Console.WriteLine(X2);
        }
        //формулы
        private double SetLambdaE() =>
            _lambdaB * 1 * 1.5 * 3 * 1 * 1;
        private double SetKoefitsentV() =>
            (_resourceRY - _resourceNM) / (-1.645 * _resourceNM - (-3.09) * _resourceRY);
        private double SetExpectedValue() =>
            _resourceRY / (1 + CoefficientV * (-1.645));
        private double SetVariance() =>
            CoefficientV * ExpectedValue;
        private double SetResourceRMax() =>
            (1 + CoefficientV * 3.09) * ExpectedValue;
        public double SetKoefitsentC() =>
            1 / 0.998;
        private double SetResourceLambda()
        {
            //значение для этого ресурса будем счиать методом дихомии

            double accuracy = 1e-8;//точность
            double min = _resourceNM;//потому что результат где-то между этими значениями
            double max = _resourceRY;
            var length = max - min;//длина интервала
            var err = length;//ошибка. Изначалана равна длине интервала

            double x = 0;
            while (err > accuracy && F(x) != 0)
            {
                // Вычисляем середину интервала.
                x = (min + max) / 2;
                // Найдём новый интервал, в котором функция меняет знак.
                if (F(min) * F(x) < 0)
                {
                    max = x;
                }
                else if (F(x) * F(max) < 0)
                {
                    min = x;
                }
                // Вычисляем новую ошибку.
                err = (max - min) / length;
                //для проверки
                Console.WriteLine(x);
            }
            return x;
        }
        //функция которую нужно посчитать
        private double F(double x)
        {
            return 1 - Math.Exp(LambdaE * (x - _resourceNM)) - (CoefficientC / (Math.Sqrt(2 * Math.PI))) * (Integra(x) - Integra(_resourceNM));
        }
        //значение интеграла, когда его раскрываем
        private double Integra(double x)
        {
            return Variance * Math.Sqrt(Math.PI / 2) * Erfi((x - ExpectedValue) / (Variance * Math.Sqrt(2)));
        }
        //реализация Erfi она появлятся при раскрытии интеграла
        private double Erfi(double x)
        {
            double a1 = 0.254829592;
            double a2 = -0.284496736;
            double a3 = 1.421413741;
            double a4 = -1.453152027;
            double a5 = 1.061405429;
            double p = 0.3275911;

            int sign = 1;
            if (x < 0)
                sign = -1;
            x = Math.Abs(x);

            Complex complexX = new Complex(0, x);
            Complex t = 1.0 / (1.0 + p * complexX);
            Complex y = 1.0 - ((((a5 * t + a4) * t + a3) * t + a2) * t + a1) * t * Math.Exp(x * x);
            return Complex.Abs(y) * sign;
        }
        private double SetX1() =>
            Math.Exp(-LambdaE * (ResourceLambda - _resourceNM));
        private double SetResourceHrMax() =>
            (1 + CoefficientV * 3.09) / (1 - CoefficientV * 3.09) * _resourceHrM * 24 * 365;
        private double SetResourceOzhMax() =>
            ResourceHrMax / (_coefficientTX * _coefficientE);
        private double SetCoefficientX() =>
            (ResourceOzhMax / ExpectedValue - 1) / CoefficientV;
        private double SetX2()
        {
            if (CoefficientX > 3.09)
                return 0;
            else return 0;//здесь будет обратная формула лапласа
        }

        //методы для построения графика
        public double[] GetAllPoints()
        {
            //мы создаем массив длины ResourceRMax и в него кладем значения x таким образом у нас в одном месте значения для построения графика
            double[] points = new double[((int)ResourceRMax) + 1];

            for (var i = 0; i <= _resourceNM; i++)
                points[i] = 1;
            for (int i = ((int)_resourceNM) + 1; i <= ResourceLambda; i++)
                points[i] = Math.Exp(-LambdaE * (i - _resourceNM));



            //делаем пока просто линию
            double c = -points[(int)ResourceLambda] / (ResourceRMax - _resourceNM);
            for (int i = ((int)ResourceLambda) + 1; i < ResourceRMax; i++)
                points[i] = c * i;

            //добавляем точку для окончания
            points[(int)ResourceRMax] = 0;

            return points;
        }
        //функция чтобы посчтитаь среднее значение ресурса
        public double GetValueX(int n)
        {
            Random rnd = new Random();
            double sum = 0;
            for (var i = 0; i < n; i++)
            {
                var value = rnd.NextDouble();
                if (value == 1)
                    sum += _resourceNM;
                if (1 > value && value >= X1)
                {

                }

            }
            return sum / n;
        }
    }
}
