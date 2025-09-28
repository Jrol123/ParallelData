namespace lab1
{
    class Program
    {
        /// <summary>
        /// Кастомный делегат
        /// </summary>
        /// <param name="intGetter">Получатель цифры</param>
        /// <param name="floatList">Дефолтные цифры</param>
        public delegate void CustomDelegate(Func<int> intGetter, List<float> floatList);

        static int GetRandomNumber()
        {
            Random rnd = new();
            return rnd.Next(1, 100);
        }

        static int GetFixedNumber()
        {
            return 42;
        }

        static int GetUserInput()
        {
            Console.Write("Введите число: ");
            string? input = Console.ReadLine();
            try
            {
#pragma warning disable CS8604
                return int.Parse(input);
#pragma warning restore CS8604
            }
            catch (System.Exception)
            {
                const int default_number = 17;
                Console.WriteLine($"В следующий раз используйте цифры!\nВаш ввод был заменён на: {default_number}");
                return default_number;
            }

        }

        /// <summary>
        /// Добавление чисел в список
        /// </summary>
        /// <param name="numberGenerator">Генератор цифр</param>
        /// <param name="numbers">Массив дефолтных фицр</param>
        static void ProcessList(Func<int> numberGenerator, List<float> numbers)
        {
            Console.WriteLine("=== Обработка списка ===");

            // Генерируем число с помощью переданного делегата
            int generatedNumber = numberGenerator();
            Console.WriteLine($"Сгенерировано число: {generatedNumber}");

            // Добавляем преобразованное число в список
            numbers.Add((float)generatedNumber / 2);
            numbers.Add((float)generatedNumber * 1.5f);

            // Выводим содержимое списка
            Console.WriteLine("Содержимое списка:");
            foreach (var num in numbers)
            {
                Console.WriteLine($"- {num:F2}");
            }
        }

        /// <summary>
        /// Анализатор списка
        /// </summary>
        /// <param name="numberGenerator">Генератор цифр</param>
        /// <param name="numbers">Массив дефолтных фицр</param>
        static void AnalyzeList(Func<int> numberGenerator, List<float> numbers)
        {
            Console.WriteLine("=== Анализ списка ===");

            int baseValue = numberGenerator();
            Console.WriteLine($"Базовое значение: {baseValue}");

            if (numbers.Count > 0)
            {
                float sum = 0;
                foreach (var num in numbers)
                {
                    sum += num;
                    Console.WriteLine($"Элемент: {num:F2}");
                }

                float average = sum / numbers.Count;
                Console.WriteLine($"Сумма: {sum:F2}, Среднее: {average:F2}");
            }
            else
            {
                Console.WriteLine("Список пуст!");
            }
        }

        /// <summary>
        /// Очистка и заполнение списка
        /// </summary>
        /// <param name="numberGenerator">Генератор цифр</param>
        /// <param name="numbers">Массив дефолтных фицр</param>
        static void ClearAndFillList(Func<int> numberGenerator, List<float> numbers)
        {
            Console.WriteLine("=== Очистка и заполнение ===");

            numbers.Clear();

            for (int i = 0; i < 3; i++)
            {
                int value = numberGenerator() + i;
                numbers.Add(value);
                Console.WriteLine($"Добавлено: {value}");
            }
        }

        static void Main(string[] args)
        {
            CustomDelegate myDelegate;

            // Дефолтные цифры
            List<float> numbers = [1.5f, 2.8f, 3.2f];

            Console.WriteLine("Демонстрация работы делегатов:");
            Console.WriteLine("===============================");

            // 1. Используем делегат с разными методами

            // Первый вызов
            myDelegate = ProcessList;
            Console.WriteLine("\n1. Вызов ProcessList с GetRandomNumber:");
            myDelegate(GetRandomNumber, new List<float>(numbers));

            // Второй вызов
            myDelegate = AnalyzeList;
            Console.WriteLine("\n2. Вызов AnalyzeList с GetFixedNumber:");
            myDelegate(GetFixedNumber, new List<float>(numbers));

            // Третий вызов
            myDelegate = ClearAndFillList;
            Console.WriteLine("\n3. Вызов ClearAndFillList с GetRandomNumber:");
            myDelegate(GetRandomNumber, new List<float>(numbers));

            // 2. Демонстрация multicast делегата
            Console.WriteLine("\n4. Multicast делегат:");
            CustomDelegate multicastDelegate = ProcessList;
            multicastDelegate += AnalyzeList;
            multicastDelegate += ClearAndFillList;

            multicastDelegate(GetFixedNumber, new List<float>(numbers));

            // 3. Использование встроенного Action
            Console.WriteLine("\n5. Использование встроенного Action:");
            // Встроенный делегат вида Action
            Action<Func<int>, List<float>> builtInAction = ProcessList;
            builtInAction(GetUserInput, numbers);
        }
    }
}