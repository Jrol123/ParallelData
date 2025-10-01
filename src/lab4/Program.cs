using System.Text;

namespace lab4
{
    class Program
    {

        public static void PostTask(Task<string> previousTask)
        {
            // await Task.Run(() =>
            // {
            Console.WriteLine();
            Console.WriteLine("Пост-обработка");
            // await Task.Delay(1000); // Останавливает только функцию, в корневом потоке цикл прерывается и даётся команда о завершении задачи.
            Task.Delay(1000).Wait(); // Команда о завершении задачи не даётся
            Console.WriteLine();
            Console.WriteLine("Работа завершена!");
            Console.WriteLine($"Полученная строка: {previousTask.Result}");
            // });
            // await Task.Delay(2000);
        }

        public delegate Task<string> CustomDelegate(string original, int shift);
        public delegate void ContinueWithDelegate(Task<string> task);

        // Пример использования с ожиданием через await (основной способ)
        public static async Task Main()
        {
            string originalStr = "Hello, World!";
            Console.WriteLine($"Оригинальная строка: {originalStr}");
            int shiftStr = 3;

            CustomDelegate lambdaDelegate = async (original, shift) =>
            {
                await Task.Delay(1000);
                // Task.Run используется для выноса CPU-bound операции в пул потоков
                StringBuilder encrypted = new();
                foreach (char c in original)
                {
                    // Простой алгоритм шифра Цезаря для символов английского алфавита
                    if (char.IsLetter(c))
                    {
                        char baseChar = char.IsUpper(c) ? 'A' : 'a';
                        char shifted = (char)(((c - baseChar + shift) % 26) + baseChar);
                        encrypted.Append(shifted);
                    }
                    else
                    {
                        encrypted.Append(c); // Не-буквы оставляем как есть
                    }
                }
                return encrypted.ToString();

            };

            // Механизм "обратного вызова" реализован через продолжение кода после await
            string encrypted = await lambdaDelegate(originalStr, shiftStr);
            Console.WriteLine($"Зашифрованная строка: {encrypted}");

            ContinueWithDelegate postDelegate = PostTask;

            // Демонстрация использования с ContinueWith (явное продолжение)
            Task encryptionTask = lambdaDelegate(originalStr, shiftStr).ContinueWith(task => postDelegate(task));

            // Можно выполнять другую работу здесь, пока задача выполняется
            Console.WriteLine("Идет шифрование... выполняется другая работа...");

            while (!encryptionTask.IsCompleted)
            {
                Console.Write(". ");
                await Task.Delay(100);
            }
            Console.WriteLine();
            Console.WriteLine("--Цикл завершён--");

            Console.WriteLine("Теперь всё готово!");
        }
    }
}