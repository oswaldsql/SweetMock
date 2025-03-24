namespace SweetMock.UnitTest;

[Mock<IVersionLibrary>]
[Mock<IVersionLibrary>]
[Mock<Class1>]

public class Class1
{
    public string Test(int test) => test.ToString();
}

public class Tester
{
    public void t()
    {
        Action<Version> triggerVersionAdded = null;
        var callLog = new CallLog();
        var sut = Mock.IVersionLibrary(config =>
            {
                config
                    .DownloadExists((string version) => true)
                    .DownloadLinkAsync(version => Task.FromResult(new Uri($"https://download/{version}")))
                    .CurrentVersion(get: () => new Version(1, 2, 3), set: version => { })
                    .NewVersionAdded(out triggerVersionAdded)
                    .LogCallsTo(callLog);
            })
            ;

        sut.DownloadExists("1,2,3,4");
        var actual = sut.CurrentVersion;
        var url = sut.DownloadLinkAsync("1,2,3,4");
        
        foreach (var log in callLog)
        {
            Console.WriteLine(log);
        }
        
        
        Action<Version> a = null;
        var newMock = MockOf_IVersionLibrary.Config.CreateNewMock(config => config
            .DownloadExists((string version) => true)
            .DownloadLinkAsync(version => Task.FromResult(new Uri("https://sweetmock.org")))
            .NewVersionAdded(out a));
        a(new Version());
        
    }
}

public interface IVersionLibrary
{
    /// <summary> Gets or sets the current version of the library. </summary>
    Version CurrentVersion { get; set; }

    /// <summary>
    ///     Gets the versoion for the specified key.
    /// </summary>
    /// <param name="key">The version key.</param>
    Version this[string key] { get; set; }

    /// <summary> Gets a value indicating whether a download exists for the specified version. </summary>
    /// <param name="version">The version as a <c>string</c></param>
    /// <returns><c>true</c> if exists, otherwise <c>false</c></returns>
    bool DownloadExists(string version);

    /// <summary> Gets a value indicating whether a download exists for the specified version. </summary>
    /// <param name="version">The version</param>
    /// <returns><c>true</c> if exists, otherwise <c>false</c></returns>
    bool DownloadExists(Version version);

    /// <summary>
    ///     Gets the download link for the specified version.
    /// </summary>
    /// <param name="version">The version</param>
    /// <returns>The uri to the specified version</returns>
    Task<Uri> DownloadLinkAsync(string version);

    /// <summary>
    ///     Occurs when a new version is added.
    /// </summary>
    event EventHandler<Version> NewVersionAdded;
}