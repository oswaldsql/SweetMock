namespace SweetMock;

using System;

public class NotExplicitlyMockedException(string memberName, string instanceName) : InvalidOperationException($"'{memberName}' in '{instanceName}' is not explicitly mocked.")
{
    public string MemberName => memberName;

    public string InstanceName => instanceName;
}