namespace lab7
{
    class Program
    {
        public class StringCharPair(string text, char symbolToFind)
        {
            public string Text { get; set; } = text;
            public char SymbolToFind { get; set; } = symbolToFind;
        }
        public static void SearchCharacter(object stateInfo)
        {
            if (stateInfo is StringCharPair searchParams)
            {
                // Искусственная задержка - блокирует текущий поток пула
                Thread.Sleep(1000);

                bool found = searchParams.Text.Contains(searchParams.SymbolToFind);

                Console.WriteLine($"Поиск в строке '{searchParams.Text.Substring(0, Math.Min(5, searchParams.Text.Length))}...': " +
                                  $"Символ '{searchParams.SymbolToFind}' {(found ? "найден" : "не найден")}. " +
                                  $"Поток [ID: {Environment.CurrentManagedThreadId}]");
            }
        }
        public static void Main()
        {
            // Коллекция элементов для обработки
            var searchData = new List<StringCharPair> {
                new("Hello, World!", 'o'),
                new("Async Programming", 'z'),
                new("ThreadPool Example", 'T')
            };

            Console.WriteLine("Основной поток запущен.");


            // Console.ReadKey(); // Чтобы дать время на выполнение фоновым потокам
            int toProcess = searchData.Count;
            using (ManualResetEvent resetEvent = new(false))
            {
                foreach (var data in searchData)
                {
                    // Постановка каждой задачи в пул потоков
                    ThreadPool.QueueUserWorkItem(
                        new WaitCallback(x =>
                        {
                            SearchCharacter(x);
                            if (Interlocked.Decrement(ref toProcess) == 0)
                                resetEvent.Set();
                        }), data);
                }
                Console.WriteLine("Все задачи поставлены в очередь.");

                resetEvent.WaitOne();
            }
            // When the code reaches here, the 10 threads will be done
            Console.WriteLine("Все задачи завершены");
        }
    }
}