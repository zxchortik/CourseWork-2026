using TestSimulator.BLL.Service;
using TestSimulator.DAL.Repositories;
using TestSimulator.Domain.Exceptions;
using TestSimulator.UI.Views;

namespace TestSimulator.UI;

class Program
{
    public static void Main()
    {
        string basePath = AppDomain.CurrentDomain.BaseDirectory;
        string targetDataFolder = Path.GetFullPath(Path.Combine(basePath, @"..\..\..\..\Data"));
        var repository = new JsonTestRepository(targetDataFolder);
        var testService = new TestService(repository);

        testService.OnLogMessage = (message) =>
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"\n[ЛОГ СЕРВІСУ]: {message}");
            Console.ResetColor();
        };

        var editorView = new TestEditorView(testService);
        var sessionView = new TestSessionView(testService);

        Console.WriteLine("--Тренажер тестів--");
        Console.WriteLine("Введіть 'help' для перегляду списку доступних команд.");
        Console.WriteLine("Введіть 'exit' для виходу з програми.");

        while (true)
        {
            Console.Write("\n> ");
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input))
            {
                continue;
            }

            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var command = parts[0].ToLower();

            try
            {
                ProcessCommand(command, parts, editorView, sessionView);
            }
            catch (TestSimulatorException ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Увага: {ex.Message}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Системна помилка: {ex.Message}");
                Console.ResetColor();
            }
        }
    }

    private static void ProcessCommand(string command, string[] parts, TestEditorView editor, TestSessionView session)
    {
        switch (command)
        {
            case "start": session.StartTest(parts); break;
            case "history": session.ShowHistory(); break;
            case "list": editor.ShowList(); break;
            case "create-topic": editor.CreateTopic(); break;
            case "create-test": editor.CreateTest(parts); break;
            case "create-question": editor.CreateQuestion(parts); break;
            case "edit-topic": editor.EditTopic(parts); break;
            case "edit-test": editor.EditTest(parts); break;
            case "edit-question": editor.EditQuestion(parts); break;
            case "delete-topic": editor.DeleteTopic(parts); break;
            case "delete-test": editor.DeleteTest(parts); break;
            case "delete-question": editor.DeleteQuestion(parts); break;
            case "help": ShowHelp(); break;
            case "stats": session.ShowStatistics(); break;
            case "exit":
                Console.WriteLine("Завершення роботи...");
                Environment.Exit(0);
                break;

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
        Console.WriteLine("  exit                  - Вихід із програми");
        Console.WriteLine("  help                  - Показати цю довідку");
        Console.WriteLine("\nПроходження тестів:");
        Console.WriteLine("  list                  - Показати всі доступні теми та тести");
        Console.WriteLine("  start <ID> [Count]    - Почати тест (можна вказати кількість питань)");
        Console.WriteLine("  history               - Показати історію проходжень");
        Console.WriteLine("  stats                 - Показати детальну статистику успішності");
        Console.WriteLine("\nРедагування та видалення:");
        Console.WriteLine("  create-topic          - Створити нову тему");
        Console.WriteLine("  create-test <ID>      - Додати тест до існуючої теми");
        Console.WriteLine("  create-question <ID>  - Додати запитання до тесту");
        Console.WriteLine("  edit-topic <ID>       - Редагувати тему");
        Console.WriteLine("  delete-topic <ID>     - Видалити тему");
        Console.WriteLine("  edit-test <ID>        - Редагувати тест");
        Console.WriteLine("  delete-test <ID>      - Видалити тест");
        Console.WriteLine("  edit-question <ID>    - Редагувати запитання (текст та бали)");
        Console.WriteLine("  delete-question <ID>  - Видалити запитання");
    }
}