namespace lab9
{

    class Program
    {
        async static Task<int[,]> GenerateRandomMatrix()
        {
            await Task.Delay(1000);
            Random rand = new();
            int rows = rand.Next(3, 8);
            int cols = rand.Next(3, 8);

            int[,] matrix = new int[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    matrix[i, j] = rand.Next(1, 101);
                }
            }

            return matrix;
        }

        static int CalculateSumOfEvenDivisibleBy4(int[,] matrix)
        {
            int sum = 0;
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    if (matrix[i, j] % 2 == 0 && matrix[i, j] % 4 == 0)
                    {
                        sum += matrix[i, j];
                    }
                }
            }
            return sum;
        }

        static int FindMaxDivisibleBy3(int[,] matrix)
        {
            int max = int.MinValue;
            bool found = false;

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    if (matrix[i, j] % 3 == 0)
                    {
                        if (!found || matrix[i, j] > max)
                        {
                            max = matrix[i, j];
                            found = true;
                        }
                    }
                }
            }

            return found ? max : -1;
        }

        static int FindMinElement(int[,] matrix)
        {
            int min = matrix[0, 0];

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    if (matrix[i, j] < min)
                    {
                        min = matrix[i, j];
                    }
                }
            }

            return min;
        }

        static void PrintMatrix(int[,] matrix)
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    Console.Write($"{matrix[i, j],3} ");
                }
                Console.WriteLine();
            }
        }

        static (int, int, int) GenerateAndPrintMatrix()
        {
            var taskMatrix = GenerateRandomMatrix();

            var postTaskPrint = taskMatrix.ContinueWith(antecedent =>
            {
                var matrix = antecedent.Result;
                Console.WriteLine($"Сгенерированная матрица: {matrix.GetLength(0)}x{matrix.GetLength(1)}");
                PrintMatrix(matrix);
            });

            var evenSumTask = taskMatrix.ContinueWith(antecedent =>
                CalculateSumOfEvenDivisibleBy4(antecedent.Result));

            var maxDivisibleBy3Task = taskMatrix.ContinueWith(antecedent =>
                FindMaxDivisibleBy3(antecedent.Result));

            var minElementTask = taskMatrix.ContinueWith(antecedent =>
                FindMinElement(antecedent.Result));

            Task.WaitAll(postTaskPrint, evenSumTask, maxDivisibleBy3Task, minElementTask);
            return (evenSumTask.Result, maxDivisibleBy3Task.Result, minElementTask.Result);
        }
        static void Main(string[] args)
        {
            int evenSumTaskRes;
            int maxDivisibleBy3TaskRes;
            int minElementTaskRes;
            (evenSumTaskRes, maxDivisibleBy3TaskRes, minElementTaskRes) = GenerateAndPrintMatrix();

            Console.WriteLine($"\nРезультаты:");
            Console.WriteLine($"Сумма чётных элементов, делящихся на 4: {evenSumTaskRes}");
            Console.WriteLine($"Максимальный элемент, делящийся на 3: {maxDivisibleBy3TaskRes}");
            Console.WriteLine($"Минимальный элемент: {minElementTaskRes}");
        }
    }
}