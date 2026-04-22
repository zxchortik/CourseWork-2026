using TestSimulator.DAL.Interfaces;
using TestSimulator.Domain.Config;
using TestSimulator.Domain.Models;
namespace TestSimulator.BLL.Service;

public class TestService
{
    private readonly ITestRepository _repository;
    private AppConfig _config;

    public TestService(ITestRepository repository)
    {
        _repository = repository;
        _config = _repository.LoadConfig();
    }

    public List<Topic> GetAllTopics()
    {
        return _repository.GetAllTopics();
    }

    public void SaveTopic(Topic topic)
    {
        _repository.SaveTopic(topic);
    }

    public void DeleteTopic(Guid topicId)
    {
        _repository.DeleteTopic(topicId);
    }

    public TestSession? StartSession(Guid testId)
    {
        var allTopics = _repository.GetAllTopics();
        var test = allTopics.SelectMany(t => t.Tests).FirstOrDefault(t => t.Id == testId);

        if (test == null)
        {
            return null;
        }

        var questionsForSession = test.Questions.ToList();

        if (_config.ShuffleQuestions)
        {
            ShuffleList(questionsForSession);
        }

        return new TestSession
        {
            TestId = test.Id,
            SessionQuestions = questionsForSession
        };
    }

    private void ShuffleList<T>(List<T> list)
    {
        var random = new Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public TestResult FinishSession(TestSession session)
    {
        session.EndTime = DateTime.Now;

        double totalScore = 0;
        double maxScore = 0;
        var incorrectIds = new List<Guid>();

        foreach (var question in session.SessionQuestions)
        {
            maxScore += question.Points;

            if (session.UserAnswers.TryGetValue(question.Id, out var userAnswer))
            {
                if (question.CheckAnswer(userAnswer))
                {
                    totalScore += question.Points;
                }
                else
                {
                    incorrectIds.Add(question.Id);
                }
            }
            else
            {
                incorrectIds.Add(question.Id);
            }
        }

        var allTopics = _repository.GetAllTopics();
        var test = allTopics.SelectMany(t => t.Tests).FirstOrDefault(t => t.Id == session.TestId);
        string title = test != null ? test.Title : "Невідомий тест";

        var result = new TestResult
        {
            SessionId = session.Id,
            TestTitle = title,
            Score = totalScore,
            MaxScore = maxScore,
            CompletedAt = session.EndTime.Value,
            IncorrectQuestionIds = incorrectIds
        };

        _repository.SaveTestResult(result);
        return result;
    }

    public List<TestResult> GetUserHistory()
    {
        return _repository.GetAllResults();
    }

    public void UpdateConfig(AppConfig newConfig)
    {
        _config = newConfig;
        _repository.SaveConfig(_config);
    }

    public AppConfig GetCurrentConfig()
    {
        return _config;
    }
}