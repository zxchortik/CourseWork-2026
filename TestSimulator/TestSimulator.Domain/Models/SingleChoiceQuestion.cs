using TestSimulator.Domain.Enums;

namespace TestSimulator.Domain.Models;

public class SingleChoiceQuestion : Question
{
    public List<string> Options { get; set; } = new List<string>();
    public int CorrectOptionIndex { get; set; }

    public SingleChoiceQuestion()
    {
        Type = QuestionType.SingleChoice;
    }

    public override bool CheckAnswer(object userAnswer)
    {
        if (userAnswer is int answerIndex)
        {
            return answerIndex == CorrectOptionIndex;
        }

        return false;
    }
}