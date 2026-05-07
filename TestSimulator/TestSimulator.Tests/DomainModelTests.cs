using TestSimulator.Domain.Models;

namespace TestSimulator.Tests;

[TestFixture]
public class DomainModelTests
{
    [Test]
    public void TestResult_PercentageScore_CalculatesCorrectly()
    {
        var result = new TestResult
        {
            Score = 15,
            MaxScore = 20
        };

        Assert.That(result.PercentageScore, Is.EqualTo(75.0));
    }

    [Test]
    public void TestResult_PercentageScore_ZeroMaxScore_ReturnsZero()
    {
        var result = new TestResult
        {
            Score = 0,
            MaxScore = 0
        };

        Assert.That(result.PercentageScore, Is.EqualTo(0));
    }

    [Test]
    public void Test_TotalPoints_CalculatesSumOfQuestionPoints()
    {
        var test = new Test
        {
            Questions = new List<Question>
            {
                new OpenAnswerQuestion { Points = 5.5 },
                new SingleChoiceQuestion { Points = 4.5 },
                new MultipleChoiceQuestion { Points = 10.0 }
            }
        };

        Assert.That(test.TotalPoints, Is.EqualTo(20.0));
    }

    [Test]
    public void Test_TotalPoints_EmptyQuestions_ReturnsZero()
    {
        var test = new Test { Questions = new List<Question>() };
        Assert.That(test.TotalPoints, Is.EqualTo(0));
    }

    [Test]
    public void TestSession_IsCompleted_FalseWhenEndTimeIsNull()
    {
        var session = new TestSession { EndTime = null };
        Assert.That(session.IsCompleted, Is.False);
    }
    [Test]
    public void TestSession_IsCompleted_TrueWhenEndTimeIsSet()
    {
        var session = new TestSession { EndTime = DateTime.Now };
        Assert.That(session.IsCompleted, Is.True);
    }

    [Test]
    public void TestSearchResult_IsFound_WorksCorrectly()
    {
        var emptySearch = new TestSearchResult();
        var validSearch = new TestSearchResult { Topic = new Topic(), Test = new Test() };

        Assert.Multiple(() =>
        {
            Assert.That(emptySearch.IsFound, Is.False);
            Assert.That(validSearch.IsFound, Is.True);
        });
    }
}