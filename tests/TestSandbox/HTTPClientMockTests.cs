namespace TestSandbox;

using System.Net;
using System.Text.Json;
using SweetMock;
using Xunit.Abstractions;

[Mock<IVersionLibrary>]
public class HttpClientMockTests(ITestOutputHelper testOutputHelper)
{

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