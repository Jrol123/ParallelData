delegate string GetAString();

class Program
{
    static void Main(string[] args)
    {
        int x = 40;
        GetAString firstStringMethod = new(x.ToString);

        Console.WriteLine("Строка равна " + firstStringMethod());
    }
}