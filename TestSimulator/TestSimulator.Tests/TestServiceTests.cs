using TestSimulator.BLL.Service;
using TestSimulator.DAL.Interfaces;
using TestSimulator.Domain.Config;
using TestSimulator.Domain.Exceptions;
using TestSimulator.Domain.Models;

namespace TestSimulator.Tests;

public class FakeTestRepository : ITestRepository
{
    public List<Topic> Topics { get; set; } = new List<Topic>();
    public AppConfig Config { get; set; } = new AppConfig();
    public List<TestResult> SavedResults { get; set; } = new List<TestResult>();

    public List<Topic> GetAllTopics() => Topics;
    public void SaveTopic(Topic topic) { }
    public void DeleteTopic(Guid topicId) { }
    public List<TestResult> GetAllResults() => SavedResults;
    public void SaveTestResult(TestResult result) { SavedResults.Add(result); }
    public AppConfig LoadConfig() => Config;
    public void SaveConfig(AppConfig config) { Config = config; }
}

[TestFixture]
public class TestServiceTests
{
    private Guid _testId;
    private FakeTestRepository _repo;
    private TestService _service;

    [SetUp]
    public void Setup()
    {
        _testId = Guid.NewGuid();
        _repo = new FakeTestRepository
        {
            Topics = new List<Topic>
            {
                new Topic
                {
                    Tests = new List<Test>
                    {
                        new Test
                        {
                            Id = _testId,
                            Questions = new List<Question>
                            {
                                new OpenAnswerQuestion { Id = Guid.NewGuid(), Points = 1 },
                                new OpenAnswerQuestion { Id = Guid.NewGuid(), Points = 1 },
                                new OpenAnswerQuestion { Id = Guid.NewGuid(), Points = 1 }
                            }
                        }
                    }
                }
            }
        };
        _service = new TestService(_repo);
    }

    [Test]
    public void StartSession_InvalidTestId_ThrowsTestNotFoundException()
    {
        Assert.Throws<TestNotFoundException>(() => _service.StartSession(Guid.NewGuid()));
    }

    [Test]
    public void StartSession_NegativeQuestionCount_ThrowsException()
    {
        var ex = Assert.Throws<TestSimulatorException>(() => _service.StartSession(_testId, -5));
        Assert.That(ex.Message, Does.Contain("більшою за нуль"));
    }

    [Test]
    public void StartSession_TooManyQuestionsRequested_ThrowsException()
    {
        var ex = Assert.Throws<TestSimulatorException>(() => _service.StartSession(_testId, 100));
        Assert.That(ex.Message, Does.Contain("У тесті лише 3 запитань"));
    }

    [Test]
    public void StartSession_ValidQuestionCount_ReturnsSubsetOfQuestions()
    {
        var session = _service.StartSession(_testId, 2);
        Assert.That(session.SessionQuestions.Count, Is.EqualTo(2));
    }

    [Test]
    public void FinishSession_MissingAnswer_TreatedAsIncorrect()
    {
        var q1 = new OpenAnswerQuestion { Id = Guid.NewGuid(), Points = 5.0, CorrectAnswerText = "Test" };
        var session = new TestSession
        {
            TestId = _testId,
            SessionQuestions = new List<Question> { q1 },
            UserAnswers = new Dictionary<Guid, object>()
        };

        var result = _service.FinishSession(session);

        Assert.Multiple(() =>
        {
            Assert.That(result.Score, Is.EqualTo(0));
            Assert.That(result.IncorrectQuestionIds, Contains.Item(q1.Id), "Пропущене питання має вважатися неправильним");
        });
    }

    [Test]
    public void FinishSession_CalculatesScoreAndFindsErrorsCorrectly()
    {
        var q1 = new OpenAnswerQuestion { Id = Guid.NewGuid(), Points = 5.0, CorrectAnswerText = "Test" };
        var q2 = new SingleChoiceQuestion { Id = Guid.NewGuid(), Points = 10.0, CorrectOptionIndex = 1 };

        var session = new TestSession
        {
            TestId = _testId,
            SessionQuestions = new List<Question> { q1, q2 },
            UserAnswers = new Dictionary<Guid, object>
            {
                { q1.Id, "Test" },
                { q2.Id, 0 }
            }
        };

        var result = _service.FinishSession(session);

        Assert.Multiple(() =>
        {
            Assert.That(result.MaxScore, Is.EqualTo(15.0));
            Assert.That(result.Score, Is.EqualTo(5.0));
            Assert.That(result.IncorrectQuestionIds, Contains.Item(q2.Id));
            Assert.That(result.IncorrectQuestionIds, Does.Not.Contain(q1.Id));
        });
    }

    [Test]
    public void UpdateConfig_SavesToRepositoryAndUpdatesCurrent()
    {
        var newConfig = new AppConfig { PassingScorePercentage = 99 };

        _service.UpdateConfig(newConfig);
        var currentConfig = _service.GetCurrentConfig();

        Assert.Multiple(() =>
        {
            Assert.That(currentConfig.PassingScorePercentage, Is.EqualTo(99));
            Assert.That(_repo.Config.PassingScorePercentage, Is.EqualTo(99), "Конфігурація не збереглася в репозиторій");
        });
    }
}