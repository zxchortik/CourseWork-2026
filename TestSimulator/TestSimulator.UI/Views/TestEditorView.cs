using TestSimulator.BLL.Service;
using TestSimulator.Domain.Config;
using TestSimulator.Domain.Exceptions;
using TestSimulator.Domain.Models;

namespace TestSimulator.UI.Views;

public class TestEditorView
{
    private readonly TestService _service;

    public TestEditorView(TestService service)
    {
        _service = service;
    }

    public void ShowList()
    {
        var topics = _service.GetAllTopics();
        if (!topics.Any())
        {
            Console.WriteLine("Немає доступних тем. Спочатку створіть їх.");
            return;
        }

        foreach (var topic in topics)
        {
            Console.WriteLine($"\nТема: {topic.Name} (ID: {topic.Id})");
            Console.WriteLine($"Опис: {topic.Description}");

            if (!topic.Tests.Any())
            {
                Console.WriteLine("Немає тестів у цій темі.");
                continue;
            }

            foreach (var test in topic.Tests)
            {
                Console.WriteLine($"Тест: {test.Title} | Питань: {test.Questions.Count} | ID: {test.Id}");

                foreach (var q in test.Questions)
                {
                    Console.WriteLine($"       - [Запитання] {q.Type}: {q.Text} (ID: {q.Id})");
                }
            }
        }
    }

    public void CreateTopic()
    {
        Console.Write("Введіть назву нової теми: ");
        var name = Console.ReadLine()?.Trim();
        Console.Write("Введіть опис теми: ");
        var desc = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(name))
        {
            throw new TestSimulatorException("Назва теми не може бути порожньою.");
        }

        var topic = new Topic { Name = name, Description = desc ?? string.Empty };
        _service.SaveTopic(topic);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Тему '{name}' успішно створено, ID: {topic.Id}");
        Console.ResetColor();
    }

    public void CreateTest(string[] parts)
    {
        if (parts.Length < 2 || !Guid.TryParse(parts[1], out Guid topicId))
        {
            throw new TestSimulatorException("Вкажіть коректний ID теми.");
        }

        var topic = _service.GetAllTopics().FirstOrDefault(t => t.Id == topicId);
        if (topic == null)
        {
            throw new TestSimulatorException("Тему з таким ID не знайдено.");
        }

        Console.Write("Введіть назву тесту: ");
        var title = Console.ReadLine()?.Trim();
        Console.Write("Введіть опис тесту: ");
        var desc = Console.ReadLine()?.Trim();

        var test = new Test { Title = title ?? "Без назви", Description = desc ?? string.Empty };
        topic.Tests.Add(test);
        _service.SaveTopic(topic);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Тест '{test.Title}' успішно додано до теми '{topic.Name}', ID тесту: {test.Id}");
        Console.ResetColor();
    }

    public void CreateQuestion(string[] parts)
    {
        if (parts.Length < 2 || !Guid.TryParse(parts[1], out Guid testId))
        {
            throw new TestSimulatorException("Вкажіть коректний ID тесту.");
        }

        var search = FindTestById(_service.GetAllTopics(), testId);
        if (!search.IsFound)
        {
            throw new TestSimulatorException("Тест з таким ID не знайдено.");
        }

        Console.WriteLine("Оберіть тип запитання:");
        Console.WriteLine("1 - з однією правильною відповіддю");
        Console.WriteLine("2 - з кількома правильними відповідями");
        Console.WriteLine("3 - відкрита відповідь");
        Console.Write("Ваш вибір (1-3): ");

        int typeChoice = ReadValidInt("Ваш вибір (1-3): ", 1, 3);

        Console.Write("Введіть текст запитання: ");
        var text = Console.ReadLine()?.Trim() ?? "Без тексту";

        double points = ReadValidDouble("Введіть кількість балів за це запитання: ", 0.1);

        Question? newQuestion = typeChoice switch
        {
            1 => BuildSingleChoiceQuestion(text, points),
            2 => BuildMultipleChoiceQuestion(text, points),
            3 => BuildOpenAnswerQuestion(text, points),
            _ => null
        };

        if (newQuestion == null)
        {
            throw new TestSimulatorException("Невідомий тип запитання.");
        }

        search.Test!.Questions.Add(newQuestion);
        _service.SaveTopic(search.Topic!);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Запитання успішно додано до тесту '{search.Test.Title}', ID: {newQuestion.Id}");
        Console.ResetColor();
    }

    public void DeleteTopic(string[] parts)
    {
        if (parts.Length < 2 || !Guid.TryParse(parts[1], out Guid id))
        {
            throw new TestSimulatorException("Вкажіть коректний ID теми.");
        }

        _service.DeleteTopic(id);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Тему успішно видалено (якщо вона існувала).");
        Console.ResetColor();
    }

    public void DeleteTest(string[] parts)
    {
        if (parts.Length < 2 || !Guid.TryParse(parts[1], out Guid id))
        {
            throw new TestSimulatorException("Вкажіть коректний ID тесту.");
        }

        var search = FindTestById(_service.GetAllTopics(), id);
        if (!search.IsFound)
        {
            throw new TestSimulatorException("Тест із таким ID не знайдено.");
        }

        search.Topic!.Tests.Remove(search.Test!);
        _service.SaveTopic(search.Topic);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Тест успішно видалено.");
        Console.ResetColor();
    }

    public void DeleteQuestion(string[] parts)
    {
        if (parts.Length < 2 || !Guid.TryParse(parts[1], out Guid id))
        {
            throw new TestSimulatorException("Вкажіть коректний ID запитання.");
        }

        var search = FindQuestionById(_service.GetAllTopics(), id);
        if (!search.IsFound)
        {
            throw new TestSimulatorException("Запитання з таким ID не знайдено.");
        }

        search.Test!.Questions.Remove(search.Question!);
        _service.SaveTopic(search.Topic!);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Запитання успішно видалено.");
        Console.ResetColor();
    }

    public void EditTopic(string[] parts)
    {
        if (parts.Length < 2 || !Guid.TryParse(parts[1], out Guid id))
        {
            throw new TestSimulatorException("Вкажіть коректний ID теми.");
        }

        var topic = _service.GetAllTopics().FirstOrDefault(t => t.Id == id);
        if (topic == null)
        {
            throw new TestSimulatorException("Тему з таким ID не знайдено.");
        }

        Console.Write($"Нова назва (залиште порожнім, щоб залишити '{topic.Name}'): ");
        var name = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(name))
        {
            topic.Name = name;
        }

        Console.Write($"Новий опис (залиште порожнім, щоб залишити '{topic.Description}'): ");
        var desc = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(desc))
        {
            topic.Description = desc;
        }

        _service.SaveTopic(topic);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Тему успішно оновлено.");
        Console.ResetColor();
    }

    public void EditTest(string[] parts)
    {
        if (parts.Length < 2 || !Guid.TryParse(parts[1], out Guid id))
        {
            throw new TestSimulatorException("Вкажіть коректний ID тесту.");
        }

        var search = FindTestById(_service.GetAllTopics(), id);
        if (!search.IsFound)
        {
            throw new TestSimulatorException("Тест із таким ID не знайдено.");
        }

        Console.Write($"Нова назва (залиште порожнім, щоб залишити '{search.Test!.Title}'): ");
        var title = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(title))
        {
            search.Test.Title = title;
        }

        Console.Write($"Новий опис (залиште порожнім, щоб залишити '{search.Test.Description}'): ");
        var desc = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(desc))
        {
            search.Test.Description = desc;
        }

        _service.SaveTopic(search.Topic!);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Тест успішно оновлено.");
        Console.ResetColor();
    }

    public void EditQuestion(string[] parts)
    {
        if (parts.Length < 2 || !Guid.TryParse(parts[1], out Guid id))
        {
            throw new TestSimulatorException("Вкажіть коректний ID запитання.");
        }

        var search = FindQuestionById(_service.GetAllTopics(), id);
        if (!search.IsFound)
        {
            throw new TestSimulatorException("Запитання з таким ID не знайдено.");
        }

        Console.Write($"Новий текст запитання (було: '{search.Question!.Text}'): ");
        var text = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(text))
        {
            search.Question.Text = text;
        }

        double? newPoints = ReadOptionalValidDouble($"Нові бали (було: {search.Question.Points}) - натисніть Enter, щоб не змінювати: ", 0.1);

        if (newPoints.HasValue)
        {
            search.Question.Points = newPoints.Value;
        }

        _service.SaveTopic(search.Topic!);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Запитання успішно оновлено.");
        Console.ResetColor();
    }

    private TestSearchResult FindTestById(List<Topic> topics, Guid testId)
    {
        foreach (var topic in topics)
        {
            var test = topic.Tests.FirstOrDefault(t => t.Id == testId);
            if (test != null)
            {
                return new TestSearchResult { Topic = topic, Test = test };
            }
        }
        return new TestSearchResult();
    }

    private QuestionSearchResult FindQuestionById(List<Topic> topics, Guid questionId)
    {
        foreach (var topic in topics)
        {
            foreach (var test in topic.Tests)
            {
                var q = test.Questions.FirstOrDefault(x => x.Id == questionId);
                if (q != null)
                {
                    return new QuestionSearchResult { Topic = topic, Test = test, Question = q };
                }
            }
        }
        return new QuestionSearchResult();
    }

    private SingleChoiceQuestion BuildSingleChoiceQuestion(string text, double points)
    {
        var single = new SingleChoiceQuestion { Text = text, Points = points };

        int count = ReadValidInt("Кількість варіантів відповіді: ", 1, 20);

        for (int i = 0; i < count; i++)
        {
            Console.Write($"Варіант {i + 1}: ");
            single.Options.Add(Console.ReadLine()?.Trim() ?? string.Empty);
        }

        int correctIndx = ReadValidInt("Введіть номер правильного варіанту: ", 1, count);
        single.CorrectOptionIndex = correctIndx - 1;

        return single;
    }

    private MultipleChoiceQuestion BuildMultipleChoiceQuestion(string text, double points)
    {
        var multi = new MultipleChoiceQuestion { Text = text, Points = points };

        int count = ReadValidInt("Кількість варіантів відповіді: ", 1, 20);

        for (int i = 0; i < count; i++)
        {
            Console.Write($"Варіант {i + 1}: ");
            multi.Options.Add(Console.ReadLine()?.Trim() ?? string.Empty);
        }

        var correctIndices = ReadValidIntList("Введіть номери правильних варіантів через кому (наприклад: 1,3): ", 1, count);

        foreach (var idx in correctIndices)
        {
            multi.CorrectOptionIndices.Add(idx - 1);
        }

        return multi;
    }

    private OpenAnswerQuestion BuildOpenAnswerQuestion(string text, double points)
    {
        var open = new OpenAnswerQuestion { Text = text, Points = points };
        Console.Write("Введіть правильну відповідь (текст): ");
        open.CorrectAnswerText = Console.ReadLine()?.Trim() ?? string.Empty;
        return open;
    }

    private int ReadValidInt(string prompt, int min, int max)
    {
        while (true)
        {
            Console.Write(prompt);
            if (int.TryParse(Console.ReadLine(), out int result) && result >= min && result <= max)
            {
                return result;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Помилка: введіть ціле число від {min} до {max}.");
            Console.ResetColor();
        }
    }

    private double ReadValidDouble(string prompt, double min)
    {
        while (true)
        {
            Console.Write(prompt);
            if (double.TryParse(Console.ReadLine(), out double result) && result >= min)
            {
                return result;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Помилка: введіть число більше або дорівнює {min}.");
            Console.ResetColor();
        }
    }

    private List<int> ReadValidIntList(string prompt, int min, int max)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Помилка: введення не може бути порожнім.");
                Console.ResetColor();
                continue;
            }

            var indices = new List<int>();
            bool hasErrors = false;

            foreach (var ans in input.Split(','))
            {
                if (int.TryParse(ans.Trim(), out int idx) && idx >= min && idx <= max)
                {
                    indices.Add(idx);
                }
                else
                {
                    hasErrors = true;
                    break;
                }
            }

            if (!hasErrors && indices.Any())
            {
                return indices;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Помилка: використовуйте лише числа від {min} до {max}, розділені комою.");
            Console.ResetColor();
        }
    }

    private double? ReadOptionalValidDouble(string prompt, double min)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input))
            {
                return null;
            }

            if (double.TryParse(input, out double result) && result >= min)
            {
                return result;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Помилка: введіть число більше або дорівнює {min}, або просто натисніть Enter.");
            Console.ResetColor();
        }
    }

    private int? ReadOptionalValidInt(string prompt, int min)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input))
            {
                return null;
            }

            if (int.TryParse(input, out int result) && result >= min)
            {
                return result;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Помилка: введіть ціле число більше або дорівнює {min}, або просто натисніть Enter.");
            Console.ResetColor();
        }
    }

    public void Settings()
    {
        var config = _service.GetCurrentConfig();

        while (true)
        {
            Console.WriteLine("\n=== НАЛАШТУВАННЯ ЗАСТОСУНКУ ===");
            Console.WriteLine($"1. Перемішувати питання та варіанти: {(config.ShuffleQuestions ? "Так" : "Ні")}");
            Console.WriteLine($"2. Прохідний бал для тестів: {config.PassingScorePercentage}%");
            Console.WriteLine($"3. Тривалість сесії (хвилин): {config.SessionDurationMinutes}");
            Console.WriteLine($"4. Показувати правильну відповідь одразу: {(config.ShowCorrectAnswersImmediately ? "Так" : "Ні")}");
            Console.WriteLine("0. Повернутися назад");
            Console.Write("Що хочете змінити? (1-4 або 0): ");

            var choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1": ToggleShuffle(config); break;
                case "2": UpdatePassingScore(config); break;
                case "3": UpdateSessionDuration(config); break;
                case "4": ToggleShowAnswers(config); break;
                case "0": return;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Помилка: Невідомий пункт меню. Будь ласка, введіть 0-4.");
                    Console.ResetColor();
                    break;
            }
        }
    }

    private void ToggleShuffle(AppConfig config)
    {
        config.ShuffleQuestions = !config.ShuffleQuestions;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Налаштування змінено! Перемішування: {(config.ShuffleQuestions ? "Так" : "Ні")}");
        Console.ResetColor();
        _service.UpdateConfig(config);
    }

    private void ToggleShowAnswers(AppConfig config)
    {
        config.ShowCorrectAnswersImmediately = !config.ShowCorrectAnswersImmediately;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Налаштування змінено! Показувати одразу: {(config.ShowCorrectAnswersImmediately ? "Так" : "Ні")}");
        Console.ResetColor();
        _service.UpdateConfig(config);
    }

    private void UpdatePassingScore(AppConfig config)
    {
        double? newScore = ReadOptionalValidDouble("Введіть новий прохідний бал у % (від 0 до 100, або Enter щоб скасувати): ", 0);
        if (!newScore.HasValue) return;

        if (newScore.Value > 100)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Помилка: Прохідний бал не може перевищувати 100%.");
            Console.ResetColor();
            return;
        }

        config.PassingScorePercentage = newScore.Value;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Налаштування змінено! Прохідний бал: {config.PassingScorePercentage}%");
        Console.ResetColor();
        _service.UpdateConfig(config);
    }

    private void UpdateSessionDuration(AppConfig config)
    {
        int? newDuration = ReadOptionalValidInt("Введіть тривалість сесії у хвилинах (від 1 до 180, або Enter щоб скасувати): ", 1);
        if (!newDuration.HasValue) return;

        if (newDuration.Value > 180)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Помилка: Тривалість не може перевищувати 180 хвилин.");
            Console.ResetColor();
            return;
        }

        config.SessionDurationMinutes = newDuration.Value;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Налаштування змінено! Тривалість сесії: {config.SessionDurationMinutes} хв.");
        Console.ResetColor();
        _service.UpdateConfig(config);
    }
}