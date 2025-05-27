# Events

__TL;DR__

```csharp
public interface IVersionLibrary
{
    event EventHandler<Version> NewVersionAdded;
}

Action<Version> triggerNewVersionAdded = _ => { };

var versionLibrary = Mock.IVersionLibrary(config => config
        .NewVersionAdded(raise: new Version(2, 0, 0, 0)) // Raises the event right away
        .NewVersionAdded(trigger: out triggerNewVersionAdded) // Provides a trigger for when a new version is added
);

// Inject into system under test

triggerNewVersionAdded(new Version(2, 0, 0, 0));
```

Alternative to creating a action for triggering the is to use the out parameter for configuration instead. 
```csharp
var versionLibrary = Mock.IVersionLibrary(out var config);

// Inject into system under test

config.NewVersionAdded(raise: new Version(2, 0, 0, 0));
```

__Please note__

- Parameter-names can be omitted but makes the code more readable.
- Unlike other members, events does not need to be specified in order to be subscribed to by the system under test.
