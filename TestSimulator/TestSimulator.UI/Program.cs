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
                ProcessCommand(command, parts, testService);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Помилка: {ex.Message}");
                Console.ResetColor();
            }
        }
    }

    private static void ProcessCommand(string command, string[] parts, TestService testService)
    {
        switch (command)
        {
            case "create-question":
                CreateQuestion(testService, parts);
                break;
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

    private static void ShowHelp()
    {
        Console.WriteLine("Доступні команди:");
        Console.WriteLine("exit - Вихід із програми");
        Console.WriteLine("help - Показати цю довідку");
        Console.WriteLine("list - Показати всі доступні теми та тести");
        Console.WriteLine("create-topic - Створити нову тему");
        Console.WriteLine("create-test <TopicID> - Додати тест до існуючої теми");
        Console.WriteLine("create-question <TestID> - Додати запитання до існуючого тесту");
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

    private static void CreateQuestion(TestService service, string[] parts)
    {
        if (parts.Length < 2 || !Guid.TryParse(parts[1], out Guid testId))
        {
            Console.WriteLine("Вкажіть коректний ID тесту. Приклад: create-question 12345678-1234-... ");
            return;
        }

        var (targetTopic, targetTest) = FindTestById(service.GetAllTopics(), testId);
        if (targetTopic == null || targetTest == null)
        {
            Console.WriteLine("Тест з таким ID не знайдено.");
            return;
        }

        Console.WriteLine("Оберіть тип запитання:");
        Console.WriteLine("1 - з однією правильною відповіддю");
        Console.WriteLine("2 - з кількома правильними відповідями");
        Console.WriteLine("3 - відкрита відповідь");
        Console.Write("Ваш вибір (1-3): ");

        var inputType = Console.ReadLine()?.Trim();

        Console.Write("Введіть текст запитання: ");
        var text = Console.ReadLine()?.Trim() ?? "Без тексту";

        Console.Write("Введіть кількість балів за це запитання: ");
        if (!double.TryParse(Console.ReadLine(), out double points))
        {
            points = 1.0;
        }

        Question? newQuestion = inputType switch
        {
            "1" => BuildSingleChoiceQuestion(text, points),
            "2" => BuildMultipleChoiceQuestion(text, points),
            "3" => BuildOpenAnswerQuestion(text, points),
            _ => null
        };

        if (newQuestion == null)
        {
            Console.WriteLine("Невідомий тип. Створення скасовано.");
            return;
        }

        targetTest.Questions.Add(newQuestion);
        service.SaveTopic(targetTopic);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Запитання успішно додано до тесту '{targetTest.Title}', ID: {newQuestion.Id}");
        Console.ResetColor();
    }

    private static (Topic? topic, Test? test) FindTestById(List<Topic> topics, Guid testId)
    {
        foreach (var topic in topics)
        {
            var test = topic.Tests.FirstOrDefault(t => t.Id == testId);
            if (test != null)
            {
                return (topic, test);
            }
        }

        return (null, null);
    }

    private static SingleChoiceQuestion BuildSingleChoiceQuestion(string text, double points)
    {
        var single = new SingleChoiceQuestion { Text = text, Points = points };
        Console.Write("Кількість варіантів відповіді ");
        if (int.TryParse(Console.ReadLine(), out int count) && count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                Console.Write($"Варіант {i + 1}: ");
                single.Options.Add(Console.ReadLine()?.Trim() ?? string.Empty);
            }
            Console.Write("Введіть номер правильного варіанту: ");
            if (int.TryParse(Console.ReadLine(), out int correctIndx))
            {
                single.CorrectOptionIndex = correctIndx - 1;
            }
        }

        return single;
    }

    private static MultipleChoiceQuestion BuildMultipleChoiceQuestion(string text, double points)
    {
        var multi = new MultipleChoiceQuestion { Text = text, Points = points };
        Console.Write("Кількість варіантів відповіді ");
        if (int.TryParse(Console.ReadLine(), out int count) && count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                Console.Write($"Варіант {i + 1}: ");
                multi.Options.Add(Console.ReadLine()?.Trim() ?? string.Empty);
            }
            Console.Write("Введіть номери правильних варіантів через кому (наприклад: 1,3): ");
            var answersInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(answersInput))
            {
                foreach (var ans in answersInput.Split(','))
                {
                    if (int.TryParse(ans.Trim(), out int indx))
                    {
                        multi.CorrectOptionIndices.Add(indx - 1);
                    }
                }
            }
        }

        return multi;
    }

    private static OpenAnswerQuestion BuildOpenAnswerQuestion(string text, double points)
    {
        var open = new OpenAnswerQuestion { Text = text, Points = points };
        Console.Write("Введіть правильну відповідь (текст): ");
        open.CorrectAnswerText = Console.ReadLine()?.Trim() ?? string.Empty;

        return open;
    }
}