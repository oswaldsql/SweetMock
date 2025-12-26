namespace SweetMock;

using System.ComponentModel;

[EditorBrowsable(EditorBrowsableState.Never)]
public record ArgumentBase(string Container, string MethodName, string MethodSignature, string? InstanceName);
