namespace SweetMock;

using System.Collections.Generic;

public class CallLog
{
    private List<ArgumentBase> _calls = [];

    public void Add(ArgumentBase argument) => this._calls.Add(argument);

    public IEnumerable<ArgumentBase> Calls => this._calls.AsReadOnly();
}
