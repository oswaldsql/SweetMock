namespace SweetMock;

public class CallLogItem
{
    public int Index { get; set; }

    public string? MethodSignature { get; set; }

    public Arguments Arguments { get; set; } = Arguments.Empty;

    public override string ToString() => this.Index.ToString("0000") + " : " + this.MethodSignature;
}

public record ArgumentBase(string Container, string MethodName, string MethodSignature, string? InstanceName);
