namespace Test.LoggingTests;

[Mock<IVersionLibrary>]
public class LogFilterTests
{
    [Fact]
    public async Task LogFilter_WhenNewVersionAdded_ShouldLogCorrectly()
    {
        // Arrange
        var dic = new Dictionary<string, Version>();
        Action<Version>? trigger = null;
        IVersionLibrary_Logs log = null!;
        var sut = Mock.IVersionLibrary(config => config
            .DownloadLinkAsync(new Uri("https://github.com"))
            .CurrentVersion(new Version(1, 2))
            .DownloadExists(true)
            .Indexer(dic)
            .NewVersionAdded(out trigger)
            .GetCallLogs(out log)
        );
        
        // ACT
        await sut.DownloadLinkAsync("1.2.3.4");
        sut.CurrentVersion = new Version(1,2,3,4);
        sut.DownloadExists("2.3.4.5");
        trigger!(new Version(1,2,3,4));

        
        log.DownloadLinkAsync(args => args is { InstanceName: "terst", version: "1.2.3.4" });

        // Assert 
        Assert.Single(log.DownloadLinkAsync(args => args.version == "1.2.3.4"));
        Assert.Single(log.CurrentVersion(args => args.value == new Version(1, 2, 3, 4)));
        Assert.Single(log.DownloadExists(args => args.version?.ToString() == "2.3.4.5"));
    }
}
