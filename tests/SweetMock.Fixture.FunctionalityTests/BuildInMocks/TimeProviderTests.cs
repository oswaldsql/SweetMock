namespace SweetMock.FixtureGenerator.FunctionalityTests.BuildInMocks;

using Microsoft.Extensions.Time.Testing;

[Fixture<TimeProviderTarget>]
public class TimeProviderTests
{
    [Fact]
    public void METHOD()
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider(new DateTimeOffset(2025,12,02,00,16,54, TimeSpan.Zero));
        fakeTimeProvider.AutoAdvanceAmount = (TimeSpan.FromMinutes(1));
        
        var fixture = Fixture.TimeProviderTarget(config =>
        {
            config.provider.Initialize(fakeTimeProvider);
        });
        var sut = fixture.CreateTimeProviderTarget();

        // ACT
        var actual = sut.GetCurrentTime();
        var actual2 = sut.GetCurrentTime();

        // Assert 
        Console.WriteLine(actual);
        Console.WriteLine(actual2);

        foreach (var argumentBase in fixture.Logs.provider.All())
        {
            Console.WriteLine(argumentBase);
        }
    }
}

public class TimeProviderTarget(TimeProvider provider)
{
    public string GetCurrentTime()
    {
        var dateTimeOffset = provider.GetUtcNow();
        return dateTimeOffset.ToString("HH:mm:ss");
    }
}