using TestSimulator.BLL.Service;
using TestSimulator.DAL.Repositories;
using TestSimulator.Domain.Models;

namespace TestSimulator.UI;

class Program
{
    public static void Main()
    {
        string basePath = AppDomain.CurrentDomain.BaseDirectory;
        string targetDataFolder = Path.GetFullPath(Path.Combine(basePath, @"..\..\..\..\Data"));
        var repository = new JsonTestRepository(targetDataFolder);
        var testService = new TestService(repository);

        Console.WriteLine("--Тренажер тестів--");
        Console.WriteLine("Введіть 'help' для перегляду списку доступних команд.");
        Console.WriteLine("Введіть 'exit' для виходу з програми.");

        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input))
            {
                continue;
            }

            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var command = parts[0].ToLower();

            try
            {
                switch (command)
                {
                    case "create-test":
                        CreateTest(testService, parts);
                        break;
                    case "create-topic":
                        CreateTopic(testService);
                        break;
                    case "list":
                        ShowList(testService);
                        break;
                    case "help":
                        ShowHelp();
                        break;

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
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Помилка: {ex.Message}");
                Console.ResetColor();
            }
        }
    }

    private static void ShowHelp()
    {
        Console.WriteLine("Доступні команди:");
        Console.WriteLine("exit - Вихід із програми");
        Console.WriteLine("help - Показати цю довідку");
        Console.WriteLine("list - Показати всі доступні теми та тести");
        Console.WriteLine("create-topic - Створити нову тему");
        Console.WriteLine("create-test <TopicID> - Додати тест до існуючої теми");
    }

    private static void ShowList(TestService service)
    {
        var topics = service.GetAllTopics();
        if (!topics.Any())
        {
            Console.WriteLine("Немає доступних тем. Спочатку створіть їх.");
            return;
        }

        foreach (var topic in topics)
        {
            Console.WriteLine($"Тема: {topic.Name} (ID: {topic.Id})");
            Console.WriteLine($"Опис: {topic.Description}");

            if (!topic.Tests.Any())
            {
                Console.WriteLine("Немає тестів у цій темі.");
                continue;
            }

            foreach (var test in topic.Tests)
            {
                Console.WriteLine($"Тест: {test.Title} | Питань: {test.Questions.Count} | ID: {test.Id}");
            }
        }
    }

    private static void CreateTopic(TestService service)
    {
        Console.Write("Введіть назву нової теми: ");
        var name = Console.ReadLine()?.Trim();

        Console.Write("Введіть опис теми: ");
        var desc = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(name))
        {
            Console.WriteLine("Назва теми не може бути порожньою.");
            return;
        }

        var topic = new Topic
        {
            Name = name,
            Description = desc ?? string.Empty
        };

        service.SaveTopic(topic);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Тему '{name}' успішно створено, ID: {topic.Id}");
        Console.ResetColor();
    }

    private static void CreateTest(TestService service, string[] parts)
    {
        if (parts.Length < 2 || !Guid.TryParse(parts[1], out Guid topicId))
        {
            Console.WriteLine("Вкажіть коректний ID теми. Приклад: create-test 12345678-1234-... ");
            return;
        }

        var topics = service.GetAllTopics();
        var topic = topics.FirstOrDefault(t => t.Id == topicId);

        if (topic == null)
        {
            Console.WriteLine("Тему з таким ID не знайдено.");
            return;
        }

        Console.Write("Введіть назву тесту: ");
        var title = Console.ReadLine()?.Trim();

        Console.Write("Введіть опис тесту: ");
        var desc = Console.ReadLine()?.Trim();

        var test = new Test
        {
            Title = title ?? "Без назви",
            Description = desc ?? string.Empty
        };

        topic.Tests.Add(test);
        service.SaveTopic(topic);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Тест '{test.Title}' успішно додано до теми '{topic.Name}', ID тесту: {test.Id}");
        Console.ResetColor();
    }
}