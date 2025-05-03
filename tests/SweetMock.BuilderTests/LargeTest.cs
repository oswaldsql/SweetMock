// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable MemberCanBePrivate.Global

namespace SweetMock.BuilderTests;

using SweetMock;
using Util;

public class LargeTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void MethodWithOutArgumentTests()
    {
        var source = @"
#nullable enable

namespace Demo;
using SweetMock.BuilderTests;
using SweetMock;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xunit;

public class TestClass{

    [Mock<LargeTest.ILoveThisLibrary>]
    public async Task IsLibraryLovable()
    {
        var versions = new Dictionary<string, Version>() { { ""current"", new Version(2, 0, 0, 0) } };

        Action<Version>? triggerNewVersionAdded = default;
        var lovable = Mock.ILoveThisLibrary(config =>
            config
                .DownloadExists(returns: true) // Returns true for all versions
                .DownloadExists(throws: new IndexOutOfRangeException()) // Throws IndexOutOfRangeException for all versions
                .DownloadExists(call: s => s.StartsWith(""2.0.0"") ? true : false) // Returns true for version 2.0.0.x

                .DownloadExistsAsync(returns: Task.FromResult(true)) // Returns true for all versions
                .DownloadExistsAsync(call: s => Task.FromResult(s.StartsWith(""2.0.0"") ? true : false)) // Returns true for version 2.0.0.x
                //.DownloadExistsAsync(returns: true) // Returns true for all versions
                .DownloadExistsAsync(throws: new IndexOutOfRangeException()) // Throws IndexOutOfRangeException for all versions
                //.DownloadExistsAsync(call: s => s.StartsWith(""2.0.0"") ? true : false) // Returns true for version 2.0.0.x

                .Version(value: new Version(2, 0, 0, 0)) // Sets the initial version to 2.0.0.0
                .Version(get: () => new Version(2,0,0,0), set: version => throw new IndexOutOfRangeException()) // Overwrites the property getter and setter

                .Indexer(values: versions) // Provides a dictionary to retrieve and store versions
                .Indexer(get: s => new Version(2,0,0,0), set: (s, version) => {}) // Overwrites the indexer getter and setter

                .NewVersionAdded(trigger: out triggerNewVersionAdded) // Provides a trigger for when a new version is added
            );

        var actual = lovable.DownloadExists(""2.0.0.0"");
        Assert.True(actual);

        var actualAsync = await lovable.DownloadExistsAsync(""2.0.0.0"");
        Assert.True(actualAsync);

        var preVersion = lovable.Version;
        lovable.Version = new Version(3, 0, 0, 0);
        var postVersion = lovable.Version;
        Assert.NotEqual(postVersion, preVersion);

        var preCurrent = lovable[""current""];
        lovable[""current""] = new Version(3, 0, 0, 0);
        var postCurrent = lovable[""current""];
        Assert.NotEqual(preCurrent, postCurrent);

        lovable.NewVersionAdded += (sender, version) => Console.WriteLine($""New version added: {version}"");
        triggerNewVersionAdded?.Invoke(new Version(2, 0, 0, 0));
    }
}";

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        Assert.Empty(generate.GetErrors());
    }

    public interface ILoveThisLibrary
    {
        Version Version { get; set; }

        Version this[string key] { get; set; }

        bool DownloadExists(string version);
        Task<bool> DownloadExistsAsync(string version);

        event EventHandler<Version> NewVersionAdded;
    }
}
