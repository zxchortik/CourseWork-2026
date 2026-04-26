using TestSimulator.BLL.Service;
using TestSimulator.DAL.Repositories;

namespace TestSimulator.UI;

class Program
{
    public static void Main()
    {
        var repository = new JsonTestRepository();
        var testService = new TestService(repository);

        Console.WriteLine("--Тренажер тестів--");
        Console.WriteLine("Введіть 'help' для перегляду списку доступних команд.");
        Console.WriteLine("Введіть 'exit' для виходу з програми.");

        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine()?.Trim().ToLower();

            if (string.IsNullOrEmpty(input))
            {
                continue;
            }

            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var command = parts[0];

            switch (command)
            {
                case "exit":
                    Console.WriteLine("Завершення роботи...");
                    return;

                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Невідома команда: '{command}'. Введіть 'help' для довідки.");
                    Console.ResetColor();
                    break;
            }
        }
    }
}