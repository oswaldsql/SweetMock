namespace SweetMock.FixtureGenerator.FunctionalityTests.BuildInMocks;

using Microsoft.Extensions.Logging;

[Fixture<ILoggerTarget>]
public class ILoggerTests(ITestOutputHelper  outputHelper)
{
    [Fact]
    public void LogMessagesAreWrittenToTheCallLog()
    {
        // Arrange
        var fixture = Fixture.ILoggerTarget(config => config
            .logger.SetLogLevel(LogLevel.Warning)
        );
        var sut = fixture.CreateILoggerTarget();

        // ACT
        sut.SomeMethod("Some name");

        var logs = fixture.Calls.logger;
        foreach (var logArguments in logs.Log())
        {
            outputHelper.WriteLine(logArguments.ToString());
        }
        
        // Assert 
        var intoItem = Assert.Single(logs.Log(args => args.logLevel == LogLevel.Information));
        Assert.Equal("Some method was called with the name 'Some name'", intoItem.message);
        
        var errorItem = Assert.Single(logs.Log(args => args.logLevel == LogLevel.Error));
        Assert.Equal("A Exception of type ArgumentException was thrown in SomeMethod", errorItem.message);
        Assert.IsType<ArgumentException>(errorItem.exception);
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