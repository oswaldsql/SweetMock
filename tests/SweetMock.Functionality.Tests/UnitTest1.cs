namespace Test;

[Mock<IVersionLibrary>]
public class UnitTest1(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void Test1()
    {
        Action<Version> triggerVersionAdded = null;
        var callLog = new CallLog();
        var sut = Mock.IVersionLibrary(config =>
        {
            config
                .LogCallsTo(callLog)
                .DownloadExists((string version) => true)
                .DownloadLinkAsync(version => Task.FromResult(new Uri($"https://download/{version}")))
                .CurrentVersion(get: () => new Version(1, 2, 3), set: version => { })
                .NewVersionAdded(out triggerVersionAdded);
        });

        sut.DownloadExists("1,2,3,4");
        sut.CurrentVersion = new Version(0,0,0,0);
        var actual = sut.CurrentVersion;
        var url = sut.DownloadLinkAsync("1,2,3,4");
        sut.NewVersionAdded += sutOnNewVersionAdded();
        triggerVersionAdded(new Version(1, 2, 3, 4));
        sut.NewVersionAdded -= sutOnNewVersionAdded();
        
        foreach (var log in callLog)
        {
            testOutputHelper.WriteLine(log.ToString());
        }

        EventHandler<Version> sutOnNewVersionAdded()
        {
            return (sender, version) => { };
        }
    }
}