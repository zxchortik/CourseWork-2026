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
}