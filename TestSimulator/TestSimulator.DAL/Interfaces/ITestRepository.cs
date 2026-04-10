using TestSimulator.Domain.Config;
using TestSimulator.Domain.Models;

namespace TestSimulator.DAL.Interfaces;

public interface ITestRepository
{
    List<Topic> GetAllTopics();
    void SaveTopic(Topic topic);
    void DeleteTopic(Guid topicId);
    List<TestResult> GetAllResults();
    void SaveTestResult(TestResult result);
    AppConfig LoadConfig();
    void SaveConfig(AppConfig config);
}