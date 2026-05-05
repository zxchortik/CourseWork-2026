namespace TestSimulator.Domain.Exceptions;

public class TestSimulatorException : Exception
{
    public TestSimulatorException(string message) : base(message)
    {

    }
}

public class TestNotFoundException : TestSimulatorException
{
    public TestNotFoundException(Guid testId) : base($"Тест із ID '{testId}' не знайдено в базі даних.")
    {
        
    }
}