namespace SweetMock;

using System.Collections.Generic;

public class CallLog
{
    private readonly List<ArgumentBase> calls = [];

    public void Add(ArgumentBase argument) => this.calls.Add(argument);

    public IEnumerable<ArgumentBase> Calls => this.calls.AsReadOnly();
}
