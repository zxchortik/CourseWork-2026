namespace TestSimulator.Domain.Config;

public class AppConfig
{
    public int SessionDurationMinutes { get; set; } = 30;
    public bool ShowCorrectAnswersImmediately { get; set; } = false;
    public double PassingScorePercentage { get; set; } = 60.0;
    public bool ShuffleQuestions { get; set; } = true;
}