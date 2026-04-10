namespace TestSimulator.Domain.Models;

public class TestSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TestId { get; set; }
    public List<Question> SessionQuestions { get; set; } = new List<Question>();
    public Dictionary<Guid, object> UserAnswers { get; set; } = new Dictionary<Guid, object>();
    public DateTime StartTime { get; set; } = DateTime.Now;
    public DateTime? EndTime { get; set; }
    public bool IsCompleted => EndTime.HasValue;
}