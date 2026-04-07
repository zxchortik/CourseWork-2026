using TestSimulator.Domain.Enums;

namespace TestSimulator.Domain.Models;

public class MultipleChoiceQuestion : Question
{
    public List<string> Options { get; set; } = new List<string>();
    public List<int> CorrectOptionIndices { get; set; } = new List<int>();

    public MultipleChoiceQuestion()
    {
        Type = QuestionType.MultipleChoice;
    }

    public override bool CheckAnswer(object userAnswer)
    {
        if (userAnswer is List<int> userAnswers)
        {
            if (userAnswers.Count != CorrectOptionIndices.Count)
            {
                return false;
            }

            var userSet = new HashSet<int>(userAnswers);
            return userSet.SetEquals(CorrectOptionIndices);
        }

        return false;
    }
}