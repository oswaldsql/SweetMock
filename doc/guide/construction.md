# Construction

__TL;DR__

```csharp
[Fact]
[Mock<IVersionLibrary>] // Signals Sweetmock to build a mock object for IVersionLibrary.
public void MockInitialization()
{
    // Mock without anything mocked used to satisfy dependencies not used in the tests execution path
    var emptyMock = Mock.IVersionLibrary();

    // Mock with inline configuration useful for most setup scenarios
    var inlineMock = Mock.IVersionLibrary(config => config
        .DownloadExists(true)
    );

    // Mock with external configuration useful for more complex scenarios like testing events and modifying mock behaviour.
    var externalMock = Mock.IVersionLibrary(out var config);
    config.DownloadExists(true);

    // Direct access to the mock implementation.
    var implementationMock = new MockOf_IVersionLibrary(config => config
        .DownloadExists(true)
    );
}
```
