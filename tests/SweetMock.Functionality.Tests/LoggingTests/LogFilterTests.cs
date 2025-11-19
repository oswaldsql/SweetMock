namespace Test.LoggingTests;

[Mock<IVersionLibrary>]
public class LogFilterTests
{
    [Fact]
    public async Task LogFilter_WhenNewVersionAdded_ShouldLogCorrectly()
    {
        // Arrange
        var logger = new CallLog();
        Dictionary<string, Version> dic = new Dictionary<string, Version>();
        Action<Version>? trigger = null;
        var sut = Mock.IVersionLibrary(config =>
        {
            config.DownloadLinkAsync(new Uri("https://github.com"));
            config.CurrentVersion(new Version(1, 2));
            config.DownloadExists(true);
            config.Indexer(dic);
            config.NewVersionAdded(out trigger);

        }, new(logger));

        
        // ACT
        await sut.DownloadLinkAsync("1.2.3.4");
        sut.CurrentVersion = new Version(1,2,3,4);
        sut.DownloadExists("2.3.4.5");
        trigger!(new Version(1,2,3,4));
        
        // Assert 
        Assert.Single(logger.DownloadLinkAsync(args => args.version == "1.2.3.4"));
        Assert.Single(logger.CurrentVersion_Set(args => args.value == new Version(1, 2, 3, 4)));
        Assert.Single(logger.DownloadExists(args => args.version?.ToString() == "2.3.4.5"));
    }
}
