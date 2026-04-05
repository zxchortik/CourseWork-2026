using TestSimulator.Domain.Enums;

namespace TestSimulator.Domain.Models;

public abstract class Question
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Text { get; set; } = string.Empty;
    public QuestionType Type { get; protected set; }
    public double Points { get; set; } = 1.0;

    public abstract bool CheckAnswer(object userAnswer);
}