namespace SweetMock;

using System.Collections.Generic;

public interface ICallLog_Filter
{
    public IEnumerable<CallLogItem> Filter();
}