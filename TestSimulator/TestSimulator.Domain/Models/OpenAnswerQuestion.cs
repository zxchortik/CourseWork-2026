using TestSimulator.Domain.Enums;

namespace TestSimulator.Domain.Models;

public class OpenAnswerQuestion : Question
{
    public string CorrectAnswerText { get; set; } = string.Empty;

    public OpenAnswerQuestion()
    {
        Type = QuestionType.OpenAnswer;
    }

    public override bool CheckAnswer(object userAnswer)
    {
        if (userAnswer is string userText)
        {
            return string.Equals(userText.Trim(), CorrectAnswerText.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }
}