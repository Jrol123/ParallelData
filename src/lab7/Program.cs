namespace lab7
{
    class Program
    {
        /// <summary>
        /// Класс Текста, Символа, который надо найти и Результата
        /// </summary>
        public class StringSearchItem(string text, char symbol)
        {
            public string Text { get; set; } = text;
            public char SymbolToFind { get; set; } = symbol;
            public bool Result { get; set; }

            // Метод, который будет выполняться в потоке пула
            public void SearchSymbol(object stateInfo)
            {
                // Имитация задержки для наглядности
                Thread.Sleep(1000);

                // Логика поиска символа
                Result = Text.Contains(SymbolToFind);

                Console.WriteLine($"[Процесс {Environment.CurrentManagedThreadId}]: Поиск символа '{SymbolToFind}' в строке \"{Text}\". Результат: {Result}");
            }
        }

        public class CollectionManager
        {
            private readonly List<StringSearchItem> _items = [];

            public void AddItem(StringSearchItem item)
            {
                _items.Add(item);
            }

            public void ProcessAll()
            {
                Console.WriteLine("Запуск обработки коллекции...");

                foreach (var item in _items)
                {
                    // Ставим в очередь метод каждого элемента на выполнение в пуле потоков
                    ThreadPool.QueueUserWorkItem(item.SearchSymbol);
                }

                // Даем время на завершение фоновых операций (для консольного приложения)
                // Thread.Sleep(2000);
                Console.WriteLine("Обработка коллекции завершена.");
            }
        }

        public static void Main(string[] args)
        {
            var manager = new CollectionManager();

            // Добавляем элементы в коллекцию
            manager.AddItem(new StringSearchItem("Hello World", 'o'));
            manager.AddItem(new StringSearchItem("Parallel Processing", 'z'));
            manager.AddItem(new StringSearchItem("C# ThreadPool", '#'));

            // Запускаем обработку
            manager.ProcessAll();

            Console.WriteLine("Завершение работы основного потока.");

            Console.ReadKey();
        }
    }
}