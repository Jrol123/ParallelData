using System.Text;

namespace lab4
{
    class Program
    {

        public static async Task PostTask(Task<string> previousTask)
        {
            Console.WriteLine();
            Console.WriteLine("Пост-обработка");

            await Task.Delay(500);

            Console.WriteLine();
            Console.WriteLine("Работа завершена!");

            Console.WriteLine($"Полученная строка: {await previousTask}");
        }

        public delegate Task<string> CustomDelegate(string original, int shift);
        public delegate Task ContinueWithDelegate(Task<string> task);

        // Пример использования с ожиданием через await (основной способ)
        public static async Task Main()
        {
            string originalStr = "Hello, World!";
            Console.WriteLine($"Оригинальная строка: {originalStr}");
            int shiftStr = 3;

            CustomDelegate lambdaDelegate = async (original, shift) =>
            {
                Console.WriteLine("Шифровка началась");
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
            Task encryptionTask = lambdaDelegate(originalStr, shiftStr).ContinueWith(task => postDelegate(task)).Unwrap(); // Unwrap преобразует вложенную операцию в часть основной
            // Task encryptionTask = await lambdaDelegate(originalStr, shiftStr).ContinueWith(task => postDelegate(task)); // Переменной encryptionTask присваивается задача, возвращаемая ContinueWith (т. е. PostDelegate)
            // Task encryptionTask = lambdaDelegate(originalStr, shiftStr).ContinueWith(async task => await postDelegate(task)).Unwrap(); // ContinueWith из-за async task вернёт Task даже до того, как postDelegate закончит работу

            // А лучше всего делать так:
            // await PostTask(Task.FromResult(encrypted));

            Console.WriteLine("Идет шифрование... выполняется другая работа...");

            while (!encryptionTask.IsCompleted)
            {
                Console.Write(". ");
                await Task.Delay(100);
            }
            Console.WriteLine();
            Console.WriteLine("--Цикл завершён--");

            // await postDelegate;

            Console.WriteLine("Теперь всё готово!");
        }
    }
}