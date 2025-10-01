namespace lab3
{
    class Program
    {
        const short DELAY_ms = 500;
        const short TIMESPAN_s = 5;
        public static string[] SplitString(string input, int countParts)
        {
            int partLength = input.Length / (countParts - 1);
            string[] parts = new string[countParts];
            int currentPart = 0;
            for (int i = 0; i < input.Length; i += partLength)
            {
                int charsCount = 0;
                int currentIndex = i;

                // Подсчет реальных символов с учетом Unicode
                while (charsCount < partLength && currentIndex < input.Length)
                {
                    char c = input[currentIndex];
                    charsCount += char.IsSurrogate(c) ? 2 : 1;
                    currentIndex++;
                }

                parts[currentPart] = input.Substring(i, currentIndex - i);
                currentPart++;
            }
            return parts;
        }

        static async Task<(bool found, bool timeoutOccurred)> CheckTasks(TimeSpan overallTimeout, CancellationTokenSource cancellationSource, List<Task<bool?>> tasks, bool found, bool timeoutOccurred)
        {
            foreach (Task<bool?> task in tasks)
            {
                try
                {
                    var result = await task;
                    if (result == true)
                    {
                        cancellationSource.Cancel();
                        found = true;
                        break;
                    }
                }
                catch (OperationCanceledException) when (cancellationSource.Token.IsCancellationRequested)
                {
                    timeoutOccurred = true;
                    Console.WriteLine($"\nВнимание: Превышено общее время выполнения ({overallTimeout.TotalSeconds} сек.).");
                    break;
                }
            }

            return (found, timeoutOccurred);
        }

        public delegate Task<bool?> AsyncDelegate(string str, char symbol, IProgress<string> progress, CancellationToken cancellationToken);

        public static async Task Main(string[] args)
        {
            const string text = "asjhdkajsdjkskjfldfghjghgasdhsadugkjashdjkashdjkhgdsagdhasfdgasdgsajhdtashdjashdsajdhjas;das;jkdasjkdpkjashdjklashdjhasgdhjagsdjhas;dlkjv";
            // u, v
            const char symbol = ']';
            const int countParts = 20;
            TimeSpan overallTimeout = TimeSpan.FromSeconds(TIMESPAN_s);

            string[] parts = SplitString(text, countParts);

            AsyncDelegate lambdaDelegate = async (str, symbol, progress, cancelToken) =>
            {
                progress?.Report("Start");
                char[] charArray = str.ToCharArray();
                int strLength = str.Length;
                for (int i = 0; i < strLength; i++)
                {
                    cancelToken.ThrowIfCancellationRequested();
                    progress?.Report($"Шаг {i + 1} из {strLength} возможных");
                    if (charArray[i] == symbol)
                    {
                        progress?.Report("Найдено!");
                        return true;
                    }

                    await Task.Delay(DELAY_ms, cancelToken);
                }
                progress?.Report("Finish");

                return false;
            };

            using var cancellationSource = new CancellationTokenSource();
            cancellationSource.CancelAfter(overallTimeout);

            var tasks = new List<Task<bool?>>();

            for (int i = 0; i < parts.Length; i++)
            {
                int processId = i + 1;
                var progress = new Progress<string>(message =>
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [Процесс {processId}] {message}");
                });
                tasks.Add(lambdaDelegate(parts[i], symbol, progress, cancellationSource.Token));
            }

            // Найден ли символ
            bool found = false;
            // Флаг для отслеживания тайм-аута
            bool timeoutOccurred = false;
            (found, timeoutOccurred) = await CheckTasks(overallTimeout, cancellationSource, tasks, found, timeoutOccurred);

            const string HAPPYMESSAGE = "GG!";
            const string SADMESSAGE = "Not GG...";

            // Вывод финального сообщения
            string endmessage = found ? HAPPYMESSAGE : (timeoutOccurred ? SADMESSAGE + " (Тайм-аут)" : SADMESSAGE);
            Console.WriteLine(endmessage);
        }
    }
}