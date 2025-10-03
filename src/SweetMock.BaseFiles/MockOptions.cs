namespace SweetMock;

public class MockOptions
{
    public MockOptions(CallLog? logger = null, string? instanceName = null)
    {
        this.Logger = logger;
        this.InstanceName = instanceName;
    }

    public static MockOptions Default => new();

    public CallLog? Logger { get; private set; }

    public string? InstanceName { get; private set; }
}