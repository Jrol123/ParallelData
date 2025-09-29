namespace lab2
{
    class Program
    {
        public delegate Task<int> TakesAWhileDelegate(int data, int ms, IProgress<string>? progress);

        public static async Task Main(string[] args)
        {

        }
    }
}