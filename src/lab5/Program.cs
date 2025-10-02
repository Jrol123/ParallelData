namespace lab5
{
    class Program
    {
        // Метод для преобразования матрицы (работает в отдельном потоке)
        static void ProcessMatrixPart(int[,] matrix, int startRow, int endRow, int threadId)
        {
            Console.WriteLine($"Поток {threadId} начал обработку строк с {startRow + 1} по {endRow + 1}");

            for (int i = startRow; i <= endRow; i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    // Если элемент кратен 3, заменяем на квадрат
                    if (matrix[i, j] % 3 == 0)
                    {
                        int original = matrix[i, j];
                        matrix[i, j] = original * original;  // Аве ссылкам
                        Console.WriteLine($"Поток {threadId}: [{i},{j}] {original} -> {matrix[i, j]}");
                    }
                    else
                    {
                        // Console.WriteLine($"Поток {threadId}: [{i},{j}] {matrix[i, j]} -> {matrix[i, j]}");
                    }
                }
            }

            Console.WriteLine($"Поток {threadId} завершил работу");
        }

        // Метод для создания и запуска потоков
        static void TransformMatrixWithThreads(int[,] matrix, int threadCount)
        {
            int rows = matrix.GetLength(0);
            int rowsPerThread = rows / threadCount;

            Thread[] threads = new Thread[threadCount];

            for (int i = 0; i < threadCount; i++)
            {
                int startRow = i * rowsPerThread;
                int endRow = (i == threadCount - 1) ? rows - 1 : startRow + rowsPerThread - 1;
                int threadId = i + 1;

                // Создаем поток для обработки своей части матрицы
                threads[i] = new Thread(() => ProcessMatrixPart(matrix, startRow, endRow, threadId));
                threads[i].Start();
            }

            // Ждем завершения всех потоков
            foreach (Thread thread in threads)
            {
                thread.Join();
            }
        }

        // Вспомогательные методы
        static void PrintMatrix(int[,] matrix, string title)
        {
            Console.WriteLine($"\n{title}:");
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    Console.Write($"{matrix[i, j],4} ");
                }
                Console.WriteLine();
            }
        }

        static void FillMatrixWithRandomNumbers(int[,] matrix, int min = 1, int max = 20)
        {
            Random rand = new();
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    matrix[i, j] = rand.Next(min, max);
                }
            }
        }

        public static void Main(string[] args)
        {
            // Тут вполне можно было бы использовать Task.Run вместо потоков, которые больше используются для долгих операций / нужен foreground-поток

            const int ROWS = 5;
            const int COLS = 5;
            const int THREAD_COUNT = 3;

            // Создаем и заполняем матрицу случайными числами
            int[,] matrix = new int[ROWS, COLS];
            FillMatrixWithRandomNumbers(matrix);

            PrintMatrix(matrix, "Исходная матрица");

            // Преобразуем матрицу с использованием потоков
            Console.WriteLine("\n=== Начало преобразования матрицы ===");
            TransformMatrixWithThreads(matrix, THREAD_COUNT);
            Console.WriteLine("=== Преобразование завершено ===\n");

            PrintMatrix(matrix, "Преобразованная матрица");
        }
    }
}