namespace Test;

using ConstructorTests;
using MethodTests;

[Mock<IVersionLibrary>]
public class UnitTest1(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void Test1()
    {
        var versions = new Dictionary<string, Version>();
        Action<Version> triggerVersionAdded = null;
        var callLog = new CallLog();
        var sut = Mock.IVersionLibrary(config =>
        {
            config
                .LogCallsTo(callLog)
                .Indexer(get : s => versions[s], set : (s, version) => versions[s] = version)
                .DownloadExists((string version) => true)
                .DownloadExists((Version version) => true)
                .DownloadLinkAsync(version => Task.FromResult(new Uri($"https://download/{version}")))
                .CurrentVersion(get: () => new Version(1, 2, 3), set: version => { })
                .NewVersionAdded(out triggerVersionAdded);
        });

        sut.DownloadExists("1,2,3,4");
        sut.CurrentVersion = new Version(2,0,0,0);
        sut["current"] = new Version(1, 2, 3);
        var current = sut["current"];
        var actual = sut.CurrentVersion;
        var url = sut.DownloadLinkAsync("1,2,3,4");
        sut.NewVersionAdded += sutOnNewVersionAdded();
        triggerVersionAdded(new Version(1, 2, 3, 4));
        sut.NewVersionAdded -= sutOnNewVersionAdded();
        
        foreach (var log in callLog)
        {
            testOutputHelper.WriteLine(log.ToString() + "[" + log.Arguments + "]");
        }

        callLog.set_CurrentVersion(t => t.value.Major == 2);
        callLog.get_CurrentVersion();
        callLog.set_Item(t => t.key == "current");
        callLog.DownloadExists(t => t.version == "new Version(1, 2, 3)");
        
        
        EventHandler<Version> sutOnNewVersionAdded()
        {
            return (sender, version) => { };
        }
    }
}
