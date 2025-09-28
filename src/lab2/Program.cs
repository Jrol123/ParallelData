delegate string GetAString();

class Program
{
    static void Main(string[] args)
    {
        int x = 69;
        GetAString firstStringMethod = new(x.ToString);

        Console.WriteLine("Строка равна " + firstStringMethod());
    }
}