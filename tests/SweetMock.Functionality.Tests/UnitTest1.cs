namespace Test;

using System.Security.Cryptography;
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

        var sequence = callLog.Sequence(log => log.DownloadExists(), log => log.CurrentVersion_Set(), log => log.CurrentVersion_Get());//,  log => log.CurrentVersion_Get(), log => log.CurrentVersion_Get(), log => log.Item_Set(), log => log.DownloadExists());
        foreach (var log in sequence)
        {
            testOutputHelper.WriteLine(log.ToString() + "[" + log.Arguments + "]");
        }
        
        callLog.CurrentVersion_Set(t => t.value.Major == 2);
        callLog.CurrentVersion_Get();
        callLog.Item_Set(t => t.key == "current");
        callLog.DownloadExists(t => t.version == "new Version(1, 2, 3)");
        
        EventHandler<Version> sutOnNewVersionAdded()
        {
            return (sender, version) => { };
        }
    }
}

public static class CallLogExtensions
{
    public static IEnumerable<CallLogItem> Sequence(this CallLog callLog, params Func<IEnumerable<CallLogItem>, IEnumerable<CallLogItem>>[] funcs)
    {
        int lastIndex = 0;
        List<CallLogItem> result = [];
        foreach (var func in funcs)
        {
            var match = func(callLog.Skip(lastIndex)).FirstOrDefault();
            if (match != null)
            {
                lastIndex = match.Index;
                result.Add(match);
            }
            else
            {
                result = [];
                continue;
            }
        }
        return result;
    }
}