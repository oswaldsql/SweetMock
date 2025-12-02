namespace SweetMock;

using System.Collections.Generic;

public class CallLog
{
    public void Add(ArgumentBase argument) => this.Calls.Add(argument);
    public List<ArgumentBase> Calls = [];
}
