namespace SweetMock.UnitTest;

[Mock<IVersionLibrary>]
//[Mock<RepoBaseClass>]
//[Mock<MethodExamples>]
[Mock<IMethodExamples>]
public class Tester
{
    public void IVersionLibExperiments()
    {
        Action<Version> triggerVersionAdded = null;
        var callLog = new CallLog();
        var sut = Mock.IVersionLibrary(config =>
            {
                config
                    .DownloadExists(throws:new ArgumentException())
                    .DownloadExists((string version) => true)
                    .DownloadExists(throws: new Exception("test"))
                    .DownloadLinkAsync(version => Task.FromResult(new Uri($"https://download/{version}")))
                    .CurrentVersion(get: () => new Version(1, 2, 3), set: version => { })
                    .NewVersionAdded(out triggerVersionAdded)
                    .Indexer(new Dictionary<string, Version>())
                    .Indexer(s => new Version(), (s, version) => {})
                    .LogCallsTo(callLog);
            });

        triggerVersionAdded(new Version(1, 1, 2));
        
        sut.DownloadExists("1,2,3,4");
        var actual = sut.CurrentVersion;
        var url = sut.DownloadLinkAsync("1,2,3,4");
        
        Action<Version> a = null;
        var newMock = MockOf_IVersionLibrary.Config.CreateNewMock(config => config
            .DownloadExists((string version) => true)
            .DownloadLinkAsync(version => Task.FromResult(new Uri("https://sweetmock.org")))
            .NewVersionAdded(out a));
        a(new Version());
    }
}

internal static class Ext
{
    public static MockOf_IVersionLibrary.Config DownloadExists3(this MockOf_IVersionLibrary.Config config, Exception throws)
    {
        config.DownloadExists((string _) => throw throws);
        config.DownloadExists((Version _) => throw throws);
        return config;
    }
    
    public static MockOf_IVersionLibrary.Config DownloadExists2(this MockOf_IVersionLibrary.Config config, Exception throws)
    {
        config.DownloadExists((Version _) => throw throws);
        config.DownloadExists((string  _) => throw throws);
        return config;
    }
    
    public static MockOf_IVersionLibrary.Config PrepareDownloadAsync2(this MockOf_IVersionLibrary.Config config)
    {
        config.PrepareDownloadAsync((string _) => Task.CompletedTask);
        config.PrepareDownloadAsync((Version _) => ValueTask.CompletedTask);
        // M:SweetMock.UnitTest.IVersionLibrary.PrepareDownloadAsync(System.String)
        // M:SweetMock.UnitTest.IVersionLibrary.PrepareDownloadAsync(System.Version)
        return config;
    }    
}

public class MethodExamples
{
    // Static method
    public static void StaticMethod()
    {
        Console.WriteLine("This is a static method.");
    }

    // Instance method
    public void InstanceMethod()
    {
        Console.WriteLine("This is an instance method.");
    }

    // Abstract method (must be in an abstract class)
    public abstract class AbstractClass
    {
        public abstract void AbstractMethod();
    }

    // Virtual method
    public virtual void VirtualMethod()
    {
        Console.WriteLine("This is a virtual method.");
    }

    // Override method
    public override string ToString()
    {
        return "This is an override method.";
    }

    // Async method
    public async Task AsyncMethod()
    {
        await Task.Delay(1000);
        Console.WriteLine("This is an async method.");
    }

    // Generic method
    public T GenericMethod<T>(T input)
    {
        Console.WriteLine("This is a generic method.");
        return input;
    }

    // Constructor
    public MethodExamples()
    {
        Console.WriteLine("This is a constructor.");
    }

    // Destructor
    ~MethodExamples()
    {
        Console.WriteLine("This is a destructor.");
    }

    // Operator overloading
    public static MethodExamples operator +(MethodExamples a, MethodExamples b)
    {
        Console.WriteLine("This is an operator overloading method.");
        return new MethodExamples();
    }

    // Indexer
    private string[] elements = new string[10];
    public string this[int index]
    {
        get => elements[index];
        set => elements[index] = value;
    }

    // Event
    public event EventHandler ExampleEvent;
    protected virtual void OnExampleEvent()
    {
        ExampleEvent?.Invoke(this, EventArgs.Empty);
    }

    // Method with ref parameter
    public void MethodWithRefParameter(ref int value)
    {
        value += 10;
        Console.WriteLine("This is a method with a ref parameter.");
    }

    // Method with out parameter
    public void MethodWithOutParameter(out int value)
    {
        value = 42;
        Console.WriteLine("This is a method with an out parameter.");
    }
}

public interface IMethodExamples
{
    void ReturnVoidNoParams();
    Task ReturnTaskNoParams();
    ValueTask ReturnValueTaskNoParams();

    void ReturnVoidWithParams(int param);
    Task ReturnTaskWithParams(int param);
    ValueTask ReturnValueTaskWithParams(int param);

    int ReturnVariableWithParams(int param);
    

    // Method with ref parameter
    void MethodWithRefParameter(ref int value);

    // Method with out parameter
    void MethodWithOutParameter(out int value);
}

public class FullTest
{
    public void T()
    {
        var versions = new Dictionary<string, Version> { { "current", new Version(2, 0, 0, 0) } };
        Action<Version> triggerNewVersionAdded = _ => { };

        var versionLibrary = Mock.IVersionLibrary(config => config
                .DownloadExists((string version) => versions.ContainsKey(version))
                .DownloadExists(true) // Returns true for all versions
                .DownloadExists(new IndexOutOfRangeException()) // Throws IndexOutOfRangeException for all versions
                .DownloadExists(s => s.StartsWith("2.0.0")) // Returns true for version 2.0.0.x base on a string parameter
                .DownloadExists(v => v is { Major: 2, Minor: 0, Revision: 0 }) // Returns true for version 2.0.0.x based on a version parameter
                //.DownloadExists([true, true, false]) // Returns true two times, then false
                .DownloadLinkAsync(Task.FromResult(new Uri("http://downloads/2.0.0"))) // Returns a task containing a download link for all versions
                .DownloadLinkAsync(s => Task.FromResult(s.StartsWith("2.0.0") ? new Uri("http://downloads/2.0.0") : new Uri("http://downloads/UnknownVersion"))) // Returns a task containing a download link for version 2.0.0.x otherwise a error link
                .DownloadLinkAsync(new TaskCanceledException()) // Throws IndexOutOfRangeException for all parameters
                //.DownloadLinkAsync(new Uri("http://downloads/2.0.0")) // Returns a task containing a download link for all versions
                //.DownloadLinkAsync(s => s.StartsWith("2.0.0") ? new Uri("http://downloads/2.0.0") : new Uri("http://downloads/UnknownVersion")) // Returns a task containing a download link for version 2.0.0.x otherwise a error link
                //.DownloadLinkAsync([Task.FromResult(new Uri("http://downloads/1.0.0")), Task.FromResult(new Uri("http://downloads/1.1.0")), Task.FromResult(new Uri("http://downloads/2.0.0"))]) // Returns a task with a download link
                //.DownloadLinkAsync([new Uri("http://downloads/2.0.0"), new Uri("http://downloads/2.0.0"), new Uri("http://downloads/2.0.0")]) // Returns a task with a download link
                .CurrentVersion(() => new Version(2, 0, 0, 0), version => throw new IndexOutOfRangeException()) // Overwrites the property getter and setter
                .CurrentVersion(new Version(2, 0, 0, 0)) // Sets the initial version to 2.0.0.0
                .Indexer(key => new Version(2, 0, 0, 0), (key, value) => { }) // Overwrites the indexer getter and setter
                .Indexer(versions) // Provides a dictionary to retrieve and store versions
                .NewVersionAdded(new Version(2, 0, 0, 0)) // Raises the event right away
                .NewVersionAdded(out triggerNewVersionAdded) // Provides a trigger for when a new version is added
        );
    }

}