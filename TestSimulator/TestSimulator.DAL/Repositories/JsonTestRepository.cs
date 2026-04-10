using System.Text.Json;
using TestSimulator.DAL.Interfaces;
using TestSimulator.Domain.Config;
using TestSimulator.Domain.Models;

namespace TestSimulator.DAL.Repositories;

public class JsonTestRepository : ITestRepository
{
    private readonly string _dataFolder = "Data";
    private readonly string _topicsFile;
    private readonly string _resultsFile;
    private readonly string _configFile;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public JsonTestRepository()
    {
        if (!Directory.Exists(_dataFolder))
        {
            Directory.CreateDirectory(_dataFolder);
        }

        _topicsFile = Path.Combine(_dataFolder, "topics.json");
        _resultsFile = Path.Combine(_dataFolder, "results.json");
        _configFile = Path.Combine(_dataFolder, "config.json");
    }

    public void DeleteTopic(Guid topicId)
    {
        var topics = GetAllTopics();
        var topicToRemove = topics.FirstOrDefault(t => t.Id == topicId);

        if (topicToRemove != null)
        {
            topics.Remove(topicToRemove);
            string json = JsonSerializer.Serialize(topics, _jsonOptions);
            File.WriteAllText(_topicsFile, json);
        }
    }

    public List<TestResult> GetAllResults()
    {
        if (!File.Exists(_resultsFile))
        {
            return new List<TestResult>();
        }

        string json = File.ReadAllText(_resultsFile);
        return JsonSerializer.Deserialize<List<TestResult>>(json) ?? new List<TestResult>();
    }

    public List<Topic> GetAllTopics()
    {
        if (!File.Exists(_topicsFile))
        {
            return new List<Topic>();
        }

        string json = File.ReadAllText(_topicsFile);
        return JsonSerializer.Deserialize<List<Topic>>(json) ?? new List<Topic>();
    }

    public AppConfig LoadConfig()
    {
        if (!File.Exists(_configFile))
        {
            return new AppConfig();
        }

        string json = File.ReadAllText(_configFile);
        return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
    }

    public void SaveConfig(AppConfig config)
    {
        string json = JsonSerializer.Serialize(config, _jsonOptions);
        File.WriteAllText(_configFile, json);
    }

    public void SaveTestResult(TestResult result)
    {
        var results = GetAllResults();
        results.Add(result);

        string json = JsonSerializer.Serialize(results, _jsonOptions);
        File.WriteAllText(_resultsFile, json);
    }

    public void SaveTopic(Topic topic)
    {
        var topics = GetAllTopics();

        var existing = topics.FirstOrDefault(t => t.Id == topic.Id);
        if (existing != null)
        {
            topics.Remove(existing);
        }

        topics.Add(topic);

        string json = JsonSerializer.Serialize(topics, _jsonOptions);
        File.WriteAllText(_topicsFile, json);
    }
}