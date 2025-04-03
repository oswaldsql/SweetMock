namespace SweetMock.UnitTest;

[Mock<IVersionLibrary>]
//[Mock<RepoBaseClass>]
//[Mock<MethodExamples>]
//[Mock<IMethodExamples>]
public class Tester
{
    public void t()
    {
        Action<Version> triggerVersionAdded = null;
        var callLog = new CallLog();
        var sut = Mock.IVersionLibrary(config =>
            {
                config
                    .DownloadExists(throws:new ArgumentException())
                    .DownloadExists((string version) => true)
                    .DownloadLinkAsync(version => Task.FromResult(new Uri($"https://download/{version}")))
                    .CurrentVersion(get: () => new Version(1, 2, 3), set: version => { })
                    .NewVersionAdded(out triggerVersionAdded)
                    .Indexer(new Dictionary<string, Version>())
                    .Indexer(s => new Version(), (s, version) => {})
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

internal static class Ext
{
    public static MockOf_IVersionLibrary.Config DownloadExists(this MockOf_IVersionLibrary.Config config, Exception throws)
    {
        config.DownloadExists((string _) => throw throws);
        config.DownloadExists((Version _) => throw throws);
        return config;
    }
    
    public static MockOf_IVersionLibrary.Config NewVersionAdded2(this MockOf_IVersionLibrary.Config config, Version newVersion)
    {
        config.NewVersionAdded(out var trigger);
        trigger.Invoke(newVersion);
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
    // Instance method
    void InstanceMethod();

    // Async method
    Task AsyncMethod();

    // Generic method
//    T GenericMethod<T>(T input);

    // Indexer
    string this[int index] { get; set; }

    // Event
    event EventHandler ExampleEvent;

    // Method with ref parameter
    void MethodWithRefParameter(ref int value);

    // Method with out parameter
    void MethodWithOutParameter(out int value);
}