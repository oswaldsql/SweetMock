# Methods

__TL;DR__

```csharp
public interface IVersionLibrary
{
        bool DownloadExists(string version);
        bool DownloadExists(Version version);
        Task<Uri> DownloadLinkAsync(string version);
 }

var versionLibrary = Mock.IVersionLibrary(config => config
        .DownloadExists(returns: true) // Returns true for all overloads
        .DownloadExists(throws: new IndexOutOfRangeException()) // Throws IndexOutOfRangeException for all overloads
        .DownloadExists(call: s => s.StartsWith(value: "2.0.0")) // Returns true for version 2.0.0.x base on a string parameter
        .DownloadExists(call: v => v is { Major: 2, Minor: 0, Revision: 0 }) // Returns true for version 2.0.0.x based on a version parameter
        .DownloadExists(returnValues: [true, true, false]) // Returns true two times, then false

        .DownloadLinkAsync(returns: Task.FromResult(result: new Uri(uriString: "http://downloads/2.0.0"))) // Returns a task containing a download link for all versions
        .DownloadLinkAsync(call: s => Task.FromResult(result: s.StartsWith(value: "2.0.0") ? new Uri(uriString: "http://downloads/2.0.0") : new Uri(uriString: "http://downloads/UnknownVersion"))) // Returns a task containing a download link for version 2.0.0.x otherwise a error link
        .DownloadLinkAsync(throws: new TaskCanceledException()) // Throws IndexOutOfRangeException for all parameters
        .DownloadLinkAsync(returns: new Uri(uriString: "http://downloads/2.0.0")) // Returns a task containing a download link for all versions
        .DownloadLinkAsync(call: s => s.StartsWith(value: "2.0.0") ? new Uri(uriString: "http://downloads/2.0.0") : new Uri(uriString: "http://downloads/UnknownVersion")) // Returns a task containing a download link for version 2.0.0.x otherwise a error link
        .DownloadLinkAsync(returnValues: [Task.FromResult(result: new Uri(uriString: "http://downloads/1.0.0")), Task.FromResult(result: new Uri(uriString: "http://downloads/1.1.0")), Task.FromResult(result: new Uri(uriString: "http://downloads/2.0.0"))]) // Returns a task with a download link
        .DownloadLinkAsync(returnValues: [new Uri(uriString: "http://downloads/2.0.0"), new Uri(uriString: "http://downloads/2.0.0"), new Uri(uriString: "http://downloads/2.0.0")]) // Returns a task with a download link
);

// Inject into system under test
```

__Please note__

- Multiple specifications for a method will overwrite each other with the last one taking precedence.
- Parameter-names can be omitted but makes the code more readable.
- Any method that is not explicitly specified will throw a `InvalidOperationException` when called.

## Common scenarios

__Call lambda expression or method__  to specify what should happen based on the input parameter. 
This can be done by using a lambda expression or a method and offers flexibility and control over what happens when the method is called, but also requires more code.

```csharp
.DownloadExists(call: s => s.StartsWith("2.0.0")) // Returns true for version 2.0.0.x based on a string parameter
```

```csharp
.DownloadExists(call: MockDownloadExists); 
      
private bool MockDownloadExists(Version version)
{
    return version is { Major: 2, Minor: 0, Revision: 0 };
}
```

__Return a fixed value__ for any call to the method gives a quick and easy way to specify the return value when you don't care about the input parameters.

```csharp
.DownloadExists(returns: true) // Returns true for all parameters
```

__Return multiple values__ for a method when you need to wary the result for each call. The first value is returned for the first call, the second for the second call, and so on.
When the last value is reached an exception is thrown.

  ```csharp
  .DownloadExists(returnValues: [true, true, false]) // Returns true two times, then false
  ```

__Methods that return void__ can be mocked by not specifying any parameters.

```csharp
.LogDownloadRequest()
```

__Throwing exceptions__ can be done by specifying the exception to throw for any call to the method.

```csharp
.DownloadExists(throws: new IndexOutOfRangeException()) // Throws IndexOutOfRangeException for all versions
```

## Overloaded methods

When a method is overloaded, you can specify the return value for one specific overload based on the input parameter.

```csharp
.DownloadExists(call: v => v is { Major: 2, Minor: 0, Revision: 0 }) // Returns true for version 2.0.0.x based on a version parameter
.DownloadExists(call: s => s.StartsWith("2.0.0")) // Returns true for version 2.0.0.x base on a string parameter
```

For overloaded methods returning identical values all overloaded methods return values will be set using the return and return values parameters.

Specifying the throw parameter will throw the exception for all overloaded methods.

Returning multiple values for overloaded methods will handle each overload as a separate instance.

## Async methods

Methods returning a `Task` or `Task<T>` are supported by the common scenarios but also supports specifying the return value or values without the Task.

```csharp
.DownloadExistsAsync(call: s => s.StartsWith("2.0.0")) // Returns true for version 2.0.0.x that will be wrapped in a task
.DownloadExistsAsync(returns: true) // Returns true for any parameter that will be wrapped in a task
.DownloadExistsAsync(returnValues: [true, false, true]) // Returns true, false, true for the first, second, and third call that will be wrapped in a task
.LogDownloadRequestAsync() // Returns a completed task for all parameters
```
