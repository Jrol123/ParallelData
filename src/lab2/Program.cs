namespace lab2
{
    class Program
    {
        const short DELAY = 500;
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

        public delegate Task<bool?> AsyncDelegate(string str, char symbol, IProgress<string> progress, CancellationToken cancellationToken);

        public static async Task Main(string[] args)
        {
            // u, v
            const string text = "asjhdkajsdjkskjfldfghjghgasdhsadugkjashdjkashdjkhgdsagdhasfdgasdgsajhdtashdjashdsajdhjas;das;jkdasjkdpkjashdjklashdjhasgdhjagsdjhas;dlkjv";
            const char symbol = 'u';
            const int countParts = 4;

            string[] parts = SplitString(text, countParts);


            AsyncDelegate lambdaDelegate = async (str, symbol, progress, cancelToken) =>
            {
                progress?.Report("Start");
                char[] charArray = str.ToCharArray();
                int strLength = str.Length;
                for (int i = 0; i < strLength; i++)
                {
                    progress?.Report($"Шаг {i + 1} из {strLength} возможных");
                    if (charArray[i] == symbol)
                    {
                        Console.WriteLine("Найдено!");
                        return true;
                    }

                    await Task.Delay(DELAY, cancelToken);
                    cancelToken.ThrowIfCancellationRequested();
                }
                progress?.Report("Finish");

                return false;
            };

            using var cancellationSource = new CancellationTokenSource();
            var tasks = new List<Task<bool?>>();


            for (int i = 0; i < parts.Length; i++)
            {
                var progress = new Progress<string>(message =>
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [Процесс {i + 1}] {message}");
                });
                // Каждая часть проверяется в отдельной задаче
                tasks.Add(lambdaDelegate(parts[i], symbol, progress, cancellationSource.Token));
            }

            bool found = false;
            // Ждём, когда любая задача найдёт символ
            while (tasks.Count > 0 && found == false)
            {
                var completedTask = await Task.WhenAny(tasks);

                if ((bool)await completedTask)
                {
                    cancellationSource.Cancel(); // Остальные задачи прервутся
                    found = true;
                    break;
                }
            }


            const string HAPPYMESSAGE = "GG!";
            const string SADMESSAGE = "Not GG...";

            string endmessage;

            if (found) { endmessage = HAPPYMESSAGE; } else { endmessage = SADMESSAGE; }

            Console.WriteLine(endmessage);
        }
    }
}