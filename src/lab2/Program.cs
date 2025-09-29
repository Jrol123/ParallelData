namespace lab2
{
    class Program
    {
        // Асинхронный делегат с поддержкой прогресса
        public delegate Task<int> TakesAWhileDelegate(int data, int ms, IProgress<string>? progress);

        // Асинхронная реализация метода
        static async Task<int> TakesAWhileAsync(int data, int ms, IProgress<string>? progress)
        {
            progress?.Report("🏁 Запуск асинхронного метода");

            // Имитируем работу с прогрессом
            for (int i = 0; i < 5; i++)
            {
                progress?.Report($"⏳ Выполнение асинхронного метода... {i + 1}/5");
                await Task.Delay(ms / 5);
            }

            progress?.Report("✅ Остановка асинхронного метода");
            return ++data;
        }

        // Еще одна реализация для демонстрации гибкости
        static async Task<int> FastTakesAWhileAsync(int data, int ms, IProgress<string>? progress)
        {
            progress?.Report("🚀 Быстрый запуск");

            await Task.Delay(ms / 2);
            progress?.Report($"⚡ Быстрое выполнение: {data} -> {data + 2}");

            return data + 2;
        }

        // Метод для обработки результатов с прогрессом
        static async Task ProcessWithProgressAsync(TakesAWhileDelegate asyncDelegate, int data, int ms)
        {
            var progress = new Progress<string>(message =>
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
            });

            Console.WriteLine($"Начало обработки с data={data}, ms={ms}");

            // Вызываем асинхронный делегат
            Task<int> task = asyncDelegate(data, ms, progress);

            // Мониторим выполнение
            while (!task.IsCompleted)
            {
                Console.Write(". ");
                await Task.Delay(100);
            }

            int result = await task;
            Console.WriteLine($"🎯 Финальный результат: {result}");
        }

        // Метод для параллельного выполнения нескольких делегатов
        static async Task ExecuteMultipleDelegatesAsync()
        {
            var progress = new Progress<string>(message =>
            {
                Console.WriteLine($"[MULTI] {message}");
            });

            TakesAWhileDelegate dl1 = TakesAWhileAsync;
            TakesAWhileDelegate dl2 = FastTakesAWhileAsync;

            // Запускаем параллельно
            Task<int> task1 = dl1(10, 2000, progress);
            Task<int> task2 = dl2(20, 1000, progress);
            Task<int> task3 = dl1(30, 1500, progress);

            // Ждем все задачи и обрабатываем результаты
            var results = await Task.WhenAll(task1, task2, task3);

            Console.WriteLine($"📊 Все результаты: {string.Join(", ", results)}");
        }

        public static async Task Main(string[] args)
        {
            const int ms = (int)5e3;
            Console.WriteLine("=== АСИНХРОННЫЕ ДЕЛЕГАТЫ С PROGRESS ===");

            // Создаем экземпляры делегатов
            TakesAWhileDelegate asyncDelegate = TakesAWhileAsync;
            TakesAWhileDelegate fastDelegate = FastTakesAWhileAsync;
            // Лямбда-реализация делегата
            TakesAWhileDelegate lambdaDelegate = async (data, ms, progress) =>
            {
                progress?.Report($"🎭 Лямбда-делегат начал работу с data={data}");

                int steps = 3;
                for (int i = 0; i < steps; i++)
                {
                    progress?.Report($"🔨 Шаг {i + 1}/{steps}");
                    await Task.Delay(ms / steps);
                }

                int result = data * 3;
                progress?.Report($"🎭 Лямбда-делегат завершил: {data} * 3 = {result}");
                return result;
            };
            // lambdaDelegate lambdaDelegate = LambdaDelegateExample;

            // 1. Базовое использование
            Console.WriteLine("\n1. 📍 Базовое использование:");
            await ProcessWithProgressAsync(asyncDelegate, 5, ms);

            // 2. Сравнение разных делегатов
            Console.WriteLine("\n2. 🔄 Сравнение разных делегатов:");
            await ProcessWithProgressAsync(fastDelegate, 8, ms);
            await ProcessWithProgressAsync(lambdaDelegate, 3, ms);

            // 3. Мультикастинг асинхронных делегатов
            Console.WriteLine("\n3. 🔗 Мультикастинг асинхронных делегатов:");

            TakesAWhileDelegate multicastDelegate = asyncDelegate;
            multicastDelegate += fastDelegate;
            multicastDelegate += lambdaDelegate;

            // Для мультикастинга нужно обработать все задачи
            var progress = new Progress<string>(message =>
            {
                Console.WriteLine($"[MULTICAST] {message}");
            });

            // Получаем список задач из мультикаста
            var tasks = multicastDelegate.GetInvocationList();
            var taskResults = new Task<int>[tasks.Length];

            for (int i = 0; i < tasks.Length; i++)
            {
                var currentDelegate = (TakesAWhileDelegate)tasks[i];
                taskResults[i] = currentDelegate(i * 10, ms, progress);
            }

            var multicastResults = await Task.WhenAll(taskResults);
            Console.WriteLine($"📦 Результаты мультикаста: {string.Join(", ", multicastResults)}");

            // 4. Параллельное выполнение
            Console.WriteLine("\n4. ⚡ Параллельное выполнение:");
            await ExecuteMultipleDelegatesAsync();

            // 5. Обработка исключений в асинхронных делегатах
            Console.WriteLine("\n5. 🚨 Обработка исключений:");

            TakesAWhileDelegate faultyDelegate = async (data, ms, prog) =>
            {
                prog?.Report("🔧 Начало работы проблемного делегата");
                await Task.Delay(500);
                throw new InvalidOperationException("Искусственная ошибка!");
            };

            try
            {
                await faultyDelegate(1, 1000, progress);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Поймано исключение: {ex.Message}");
            }

            // 6. Отмена выполнения
            Console.WriteLine("\n6. ⏹️  Отмена выполнения:");

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                TakesAWhileDelegate cancellableDelegate = async (data, ms, prog) =>
                {
                    for (int i = 0; i < 10; i++)
                    {
                        cancellationTokenSource.Token.ThrowIfCancellationRequested();
                        prog?.Report($"🔃 Работа {i + 1}/10");
                        await Task.Delay(ms / 10);
                    }
                    return data + 100;
                };

                // Отменяем через 500ms
                cancellationTokenSource.CancelAfter(500);

                try
                {
                    await cancellableDelegate(50, 2000, progress);
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("✅ Выполнение было отменено");
                }
            }
        }
    }
}