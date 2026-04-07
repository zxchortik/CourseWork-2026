namespace TestSimulator.Domain.Models;

public class Test
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Question> Questions { get; set; } = new List<Question>();
    public double TotalPoints => Questions.Sum(q => q.Points);
}