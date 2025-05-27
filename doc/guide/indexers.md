# Indexers

__TL;DR__

```csharp
public interface IVersionLibrary
{
    Version this[string key] { get; set; }
}

var versions = new Dictionary<string, Version> { { "current", new Version(2, 0, 0, 0) } };

var versionLibrary = Mock.IVersionLibrary(config => config
        .Indexer(get: key => new Version(2, 0, 0, 0), set: (key, value) => { }) // Overwrites the indexer getter and setter
        .Indexer(values: versions) // Provides a dictionary to retrieve and store versions
);

// Inject into system under test
```

__Please note__

- Multiple specifications for an indexer will overwrite each other with the last one taking precedence.
- Parameter-names can be omitted but make the code more readable.
- Any indexer that is not explicitly specified will throw an `InvalidOperationException` when called.
