using TestSimulator.Domain.Models;

namespace TestSimulator.Tests;

[TestFixture]
public class QuestionTests
{
    [Test]
    public void SingleChoiceQuestion_CorrectAnswer_ReturnsTrue()
    {
        var question = new SingleChoiceQuestion { CorrectOptionIndex = 1 };
        Assert.That(question.CheckAnswer(1), Is.True);
    }

    [Test]
    public void SingleChoiceQuestion_IncorrectAnswer_ReturnsFalse()
    {
        var question = new SingleChoiceQuestion { CorrectOptionIndex = 1 };
        Assert.That(question.CheckAnswer(0), Is.False);
    }

    [Test]
    public void SingleChoiceQuestion_WrongDataType_ReturnsFalse()
    {
        var question = new SingleChoiceQuestion { CorrectOptionIndex = 1 };
        Assert.That(question.CheckAnswer("1"), Is.False);
    }

    [Test]
    public void MultipleChoiceQuestion_ExactCorrectAnswers_ReturnsTrue()
    {
        var question = new MultipleChoiceQuestion { CorrectOptionIndices = new List<int> { 0, 2 } };
        Assert.That(question.CheckAnswer(new List<int> { 0, 2 }), Is.True);
    }

    [Test]
    public void MultipleChoiceQuestion_CorrectAnswersInWrongOrder_ReturnsTrue()
    {
        var question = new MultipleChoiceQuestion { CorrectOptionIndices = new List<int> { 0, 2 } };
        Assert.That(question.CheckAnswer(new List<int> { 2, 0 }), Is.True);
    }

    [Test]
    public void MultipleChoiceQuestion_PartialAnswer_ReturnsFalse()
    {
        var question = new MultipleChoiceQuestion { CorrectOptionIndices = new List<int> { 0, 2 } };
        Assert.That(question.CheckAnswer(new List<int> { 0 }), Is.False);
    }
    [Test]
    public void MultipleChoiceQuestion_TooManyAnswers_ReturnsFalse()
    {
        var question = new MultipleChoiceQuestion { CorrectOptionIndices = new List<int> { 0, 2 } };
        Assert.That(question.CheckAnswer(new List<int> { 0, 1, 2 }), Is.False);
    }

    [Test]
    public void MultipleChoiceQuestion_EmptyAnswer_ReturnsFalse()
    {
        var question = new MultipleChoiceQuestion { CorrectOptionIndices = new List<int> { 0, 2 } };
        Assert.That(question.CheckAnswer(new List<int>()), Is.False);
    }

    [Test]
    public void OpenAnswerQuestion_CorrectCaseInsensitiveAnswer_ReturnsTrue()
    {
        var question = new OpenAnswerQuestion { CorrectAnswerText = "Київ" };

        Assert.Multiple(() =>
        {
            Assert.That(question.CheckAnswer("Київ"), Is.True);
            Assert.That(question.CheckAnswer("КИЇВ"), Is.True);
            Assert.That(question.CheckAnswer("київ"), Is.True);
            Assert.That(question.CheckAnswer(" Київ "), Is.True);
        });
    }

    [Test]
    public void OpenAnswerQuestion_IncorrectAnswer_ReturnsFalse()
    {
        var question = new OpenAnswerQuestion { CorrectAnswerText = "Київ" };
        Assert.That(question.CheckAnswer("Львів"), Is.False);
    }
}