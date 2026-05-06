using TestSimulator.BLL.Service;
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

        Console.Write($"Нові бали (було: {search.Question.Points}): ");
        var pointsInput = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(pointsInput) && double.TryParse(pointsInput, out double points))
        {
            search.Question.Points = points;
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

        Console.Write("Кількість варіантів відповіді: ");
        if (!int.TryParse(Console.ReadLine(), out int count) || count <= 0)
        {
            return single;
        }

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

        return single;
    }

    private MultipleChoiceQuestion BuildMultipleChoiceQuestion(string text, double points)
    {
        var multi = new MultipleChoiceQuestion { Text = text, Points = points };

        Console.Write("Кількість варіантів відповіді: ");
        if (!int.TryParse(Console.ReadLine(), out int count) || count <= 0)
        {
            return multi;
        }

        for (int i = 0; i < count; i++)
        {
            Console.Write($"Варіант {i + 1}: ");
            multi.Options.Add(Console.ReadLine()?.Trim() ?? string.Empty);
        }

        Console.Write("Введіть номери правильних варіантів через кому (наприклад: 1,3): ");
        var answersInput = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(answersInput))
        {
            return multi;
        }

        foreach (var ans in answersInput.Split(','))
        {
            if (int.TryParse(ans.Trim(), out int indx))
            {
                multi.CorrectOptionIndices.Add(indx - 1);
            }
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
}