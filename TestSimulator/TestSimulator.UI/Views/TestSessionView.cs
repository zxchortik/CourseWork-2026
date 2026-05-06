using TestSimulator.BLL.Service;
using TestSimulator.Domain.Models;

namespace TestSimulator.UI.Views;

public class TestSessionView
{
    private readonly TestService _service;

    public TestSessionView(TestService service)
    {
        _service = service;
    }

    public void StartTest(string[] parts)
    {
        if (parts.Length < 2 || !Guid.TryParse(parts[1], out Guid testId))
        {
            Console.WriteLine("Вкажіть коректний ID тесту.");
            return;
        }

        int? qCount = null;
        if (parts.Length >= 3)
        {
            if (int.TryParse(parts[2], out int parsedCount))
            {
                qCount = parsedCount;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Помилка: '{parts[2]}' не є коректним числом.");
                Console.ResetColor();
                return;
            }
        }

        var session = _service.StartSession(testId, qCount);

        Console.Clear();
        Console.WriteLine("--- Початок тесту ---");

        int qNumber = 1;
        foreach (var question in session.SessionQuestions)
        {
            Console.WriteLine($"\nЗапитання {qNumber++}/{session.SessionQuestions.Count} ({question.Points} балів):");
            Console.WriteLine(question.Text);

            object? userAnswer = AskUserForAnswer(question);
            if (userAnswer != null)
            {
                session.UserAnswers[question.Id] = userAnswer;
            }
        }

        var result = _service.FinishSession(session);
        PrintResult(result, session);
    }

    public void ShowHistory()
    {
        var history = _service.GetUserHistory();
        if (!history.Any())
        {
            Console.WriteLine("Історія порожня.");
            return;
        }

        Console.WriteLine("\n--- Ваша історія ---");
        foreach (var result in history.OrderByDescending(r => r.CompletedAt))
        {
            Console.WriteLine($"[{result.CompletedAt:g}] {result.TestTitle} | Результат: {result.Score}/{result.MaxScore} ({result.PercentageScore:F1}%)");
        }
    }

    private object? AskUserForAnswer(Question question)
    {
        if (question is SingleChoiceQuestion single)
        {
            return AskSingleChoice(single);
        }

        if (question is MultipleChoiceQuestion multi)
        {
            return AskMultiChoice(multi);
        }

        if (question is OpenAnswerQuestion open)
        {
            return AskOpenAnswer(open);
        }

        return null;
    }

    private object? AskSingleChoice(SingleChoiceQuestion q)
    {
        for (int i = 0; i < q.Options.Count; i++)
        {
            Console.WriteLine($"  {i + 1}. {q.Options[i]}");
        }

        while (true)
        {
            Console.Write("Ваша відповідь: ");
            if (int.TryParse(Console.ReadLine(), out int indx) && indx >= 1 && indx <= q.Options.Count)
            {
                return indx - 1;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Помилка: введіть коректне число від 1 до {q.Options.Count}.");
            Console.ResetColor();
        }
    }

    private object AskMultiChoice(MultipleChoiceQuestion q)
    {
        for (int i = 0; i < q.Options.Count; i++)
        {
            Console.WriteLine($"  {i + 1}. {q.Options[i]}");
        }

        while (true)
        {
            Console.Write("Ваші відповіді через кому (наприклад 1,3): ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                return new List<int>();
            }

            var answerIndices = new List<int>();
            bool hasErrors = false;

            foreach (var ans in input.Split(','))
            {
                if (int.TryParse(ans.Trim(), out int idx) && idx >= 1 && idx <= q.Options.Count)
                {
                    answerIndices.Add(idx - 1);
                }
                else
                {
                    hasErrors = true;
                    break;
                }
            }

            if (!hasErrors)
            {
                return answerIndices;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Помилка: використовуйте лише числа від 1 до {q.Options.Count}, розділені комою.");
            Console.ResetColor();
        }
    }

    private object? AskOpenAnswer(OpenAnswerQuestion q)
    {
        Console.Write("Ваша відповідь: ");
        return Console.ReadLine()?.Trim();
    }

    private void PrintResult(TestResult result, TestSession session)
    {
        Console.WriteLine("\n=====================================");
        Console.WriteLine($"Тест завершено! Ваш результат: {result.Score} / {result.MaxScore} ({result.PercentageScore:F1}%)");

        if (result.IncorrectQuestionIds.Any())
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\nПомилки у запитаннях:");
            Console.ResetColor();

            foreach (var wrongId in result.IncorrectQuestionIds)
            {
                var wrongQ = session.SessionQuestions.First(q => q.Id == wrongId);
                Console.WriteLine($"- {wrongQ.Text}");
            }
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Ідеально! Жодної помилки.");
            Console.ResetColor();
        }
        Console.WriteLine("=====================================");
    }
}