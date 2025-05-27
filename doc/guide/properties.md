# Properties

__TL;DR__

```csharp
public interface IVersionLibrary
{
    Version CurrentVersion { get; set; }
}

var versionLibrary = Mock.IVersionLibrary(config => config
    .CurrentVersion(get: () => new Version(major: 2, minor: 0, build: 0, revision: 0), set: version => throw new IndexOutOfRangeException()) // Overwrites the property getter and setter
    .CurrentVersion(value: new Version(major: 2, minor: 0, build: 0, revision: 0)) // Sets the initial version to 2.0.0.0
);

// Inject into system under test

```

__Please note__ 

- Multiple specifications for a property will overwrite each other with the last one taking precedence.
- Parameter-names can be omitted but make the code more readable.
- Any property that is not explicitly specified will throw an `InvalidOperationException` when called.
- If the mocked interface or class only exposes get or set only the exposes parameter will be shown.
