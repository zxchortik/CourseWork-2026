namespace TestSimulator.Domain.Models;

public struct TestSearchResult
{
    public Topic? Topic { get; set; }
    public Test? Test { get; set; }

    public bool IsFound => Topic != null && Test != null;
}

public struct QuestionSearchResult
{
    public Topic? Topic { get; set; }
    public Test? Test { get; set; }
    public Question? Question { get; set; }

    public bool IsFound => Topic != null && Test != null && Question != null;
}