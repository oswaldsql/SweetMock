namespace SweetMock;

public class MockOptions(CallLog? logger = null, string? instanceName = null)
{
    public static MockOptions Default => new();

    public CallLog? Logger { get; } = logger;

    public string? InstanceName { get; } = instanceName;
}
