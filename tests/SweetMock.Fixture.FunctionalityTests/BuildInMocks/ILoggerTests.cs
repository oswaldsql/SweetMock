namespace SweetMock.FixtureGenerator.FunctionalityTests.BuildInMocks;

using Microsoft.Extensions.Logging;

[Fixture<ILoggerTarget>]
public class ILoggerTests
{
    [Fact]
    public void LogMessagesAreWrittenToTheCallLog()
    {
        // Arrange
        var fixture = Fixture.ILoggerTarget();
        var sut = fixture.CreateILoggerTarget();

        // ACT
        sut.SomeMethod("Some name");

        // Assert 
        var intoItem = Assert.Single(fixture.Log.Log(args => args.logLevel == LogLevel.Information));
        Assert.Equal("Some method was called with the name 'Some name'", intoItem.message);
        Console.WriteLine(intoItem.state.ToString());
        
        var errorItem = Assert.Single(fixture.Log.Log(args => args.logLevel == LogLevel.Error));
        Assert.Equal("A Exception of type ArgumentException was thrown in SomeMethod", errorItem.message);
        Assert.IsType<ArgumentException>(errorItem.exception);
        Console.WriteLine(errorItem.state.ToString());
    }
    
    public class ILoggerTarget(ILogger<ILoggerTarget> logger)
    {
        public string SomeMethod(string name)
        {
            using var scope = logger.BeginScope(new {scope = "This is a scope"});
            logger.LogInformation("Some method was called with the name '{Name}'", name);
            logger.LogError(new ArgumentException(), "A Exception of type {ExceptionType} was thrown in {MethodName}", "ArgumentException", nameof(SomeMethod));
            logger.SomeCriticalMessageReason("Lunch is late");
            return name;
        }
    }
}

public static partial class LogExtensions
{
    [LoggerMessage(LogLevel.Critical, "This is some critical message {reason}")]
    public static partial void SomeCriticalMessageReason(this ILogger<ILoggerTests.ILoggerTarget> logger, string reason);
}