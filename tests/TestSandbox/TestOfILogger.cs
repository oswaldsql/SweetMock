namespace TestSandbox;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using SweetMock;

[Fixture<Target>]
public class TestOfILogger
{
    [Test]
    public void METHOD()
    {
        var fixture = Fixture.Target(config => config.dependency.GetName("tester"));
        var sut = fixture.CreateTarget();
        
        sut.LogSomething();

        foreach (var callLogItem in fixture.Log.GetLogs())
        {
            Console.WriteLine(callLogItem);
            Console.WriteLine(" - " + callLogItem.Arguments.ToString());
        }

        var t = fixture.Log.Log(logArgs => logArgs.logLevel == LogLevel.Information).First();
        
        //t.message
        
        Assert.That(fixture.Log.Log(logArgs => logArgs.logLevel == LogLevel.Information), Is.Not.Empty);
    }
}

public interface IDependency
{
    public string GetName(int number);
}

public class Settings{}

public class Target(IDependency dependency, ILogger<Target> logger, IOptions<Settings> settings)
{
    public void LogSomething()
    {
        dependency.GetName(12);
        using var scope = logger.BeginScope("this is a scope");
        if(logger.IsEnabled(LogLevel.Information)) logger.LogInformation("This is a test");
    }
}