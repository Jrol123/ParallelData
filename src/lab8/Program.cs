namespace lab8
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    class Program
    {
        // Метод генерации случайного вектора
        static double[] GenerateRandomVector(int size, double minValue = 0, double maxValue = 10)
        {
            Random random = new();
            double[] vector = new double[size];

            for (int i = 0; i < size; i++)
            {
                vector[i] = minValue + (random.NextDouble() * (maxValue - minValue));
            }

            return vector;
        }

        // Синхронная версия скалярного произведения
        static double DotProductSync(double[] vector1, double[] vector2)
        {
            if (vector1.Length != vector2.Length)
                throw new ArgumentException("Векторы должны иметь одинаковую длину");

            double result = 0;
            for (int i = 0; i < vector1.Length; i++)
            {
                result += vector1[i] * vector2[i];
            }
            return result;
        }

        // Асинхронная версия с использованием Task
        static async Task<double> DotProductAsync(double[] vector1, double[] vector2)
        {
            if (vector1.Length != vector2.Length)
                throw new ArgumentException("Векторы должны иметь одинаковую длину");

            // Запускаем вычисление в отдельной задаче
            return await Task.Run(() =>
            {
                double result = 0;
                for (int i = 0; i < vector1.Length; i++)
                {
                    result += vector1[i] * vector2[i];
                }
                return result;
            });
        }

        // Параллельная версия с разбиением на части
        static async Task<double> DotProductParallelAsync(double[] vector1, double[] vector2, int partitions = 4)
        {
            if (vector1.Length != vector2.Length)
                throw new ArgumentException("Векторы должны иметь одинаковую длину");

            int vectorLength = vector1.Length;
            int partitionSize = vectorLength / partitions;

            // Создаем задачи для каждой части векторов
            var tasks = new Task<double>[partitions];

            for (int i = 0; i < partitions; i++)
            {
                int start = i * partitionSize;
                int end = (i == partitions - 1) ? vectorLength : start + partitionSize;

                // Захватываем переменные для каждой итерации
                int localStart = start;
                int localEnd = end;

                tasks[i] = Task.Run(() =>
                {
                    double partialSum = 0;
                    for (int j = localStart; j < localEnd; j++)
                    {
                        partialSum += vector1[j] * vector2[j];
                    }
                    return partialSum;
                });
            }

            // Ждем завершения всех задач и суммируем результаты
            double[] partialResults = await Task.WhenAll(tasks);
            return partialResults.Sum();
        }

        public static async Task Main(string[] args)
        {
            const int vectorSize = 1000000; // Большой размер для демонстрации

            Console.WriteLine("Генерация случайных векторов...");
            double[] vector1 = GenerateRandomVector(vectorSize);
            double[] vector2 = GenerateRandomVector(vectorSize);

            Console.WriteLine($"Размер векторов: {vectorSize} элементов");

            // Тестирование синхронной версии
            Console.WriteLine("\n1. Синхронное вычисление:");
            var syncResult = DotProductSync(vector1, vector2);
            Console.WriteLine($"Результат: {syncResult:F6}");

            // Тестирование асинхронной версии
            Console.WriteLine("\n2. Асинхронное вычисление:");
            var asyncResult = await DotProductAsync(vector1, vector2);
            Console.WriteLine($"Результат: {asyncResult:F6}");

            // Тестирование параллельной версии
            Console.WriteLine("\n3. Параллельное вычисление (4 части):");
            var parallelResult = await DotProductParallelAsync(vector1, vector2, 4);
            Console.WriteLine($"Результат: {parallelResult:F6}");

            // Сравнение производительности
            Console.WriteLine("\n4. Сравнение производительности:");

            var watch = System.Diagnostics.Stopwatch.StartNew();
            syncResult = DotProductSync(vector1, vector2);
            watch.Stop();
            Console.WriteLine($"Синхронная версия: {watch.ElapsedMilliseconds} мс");

            watch.Restart();
            asyncResult = await DotProductAsync(vector1, vector2);
            watch.Stop();
            Console.WriteLine($"Асинхронная версия: {watch.ElapsedMilliseconds} мс");

            watch.Restart();
            parallelResult = await DotProductParallelAsync(vector1, vector2, 4);
            watch.Stop();
            Console.WriteLine($"Параллельная версия: {watch.ElapsedMilliseconds} мс");

            // Демонстрация с меньшими векторами для проверки правильности
            Console.WriteLine("\n5. Проверка правильности (малые векторы):");
            double[] small1 = [1, 2, 3];
            double[] small2 = [4, 5, 6];

            var testResult = await DotProductAsync(small1, small2);
            Console.WriteLine($"Вектор 1: [{string.Join(", ", small1)}]");
            Console.WriteLine($"Вектор 2: [{string.Join(", ", small2)}]");
            Console.WriteLine($"Скалярное произведение: {testResult} (ожидалось: 32)");

            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}