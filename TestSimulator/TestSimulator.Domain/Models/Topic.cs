namespace TestSimulator.Domain.Models;

public class Topic
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Test> Tests { get; set; } = new List<Test>();
}