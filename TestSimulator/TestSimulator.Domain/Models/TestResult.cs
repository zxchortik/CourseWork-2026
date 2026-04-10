namespace TestSimulator.Domain.Models;

public class TestResult
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SessionId { get; set; }
    public string TestTitle { get; set; } = string.Empty;
    public double Score { get; set; }
    public double MaxScore { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.Now;
    public List<Guid> IncorrectQuestionIds { get; set; } = new List<Guid>();
    public double PercentageScore => MaxScore > 0 ? (Score / MaxScore) * 100 : 0;
}