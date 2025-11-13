namespace SweetMock;

using System.Collections.Generic;

public interface ICallLogFilter
{
    public IEnumerable<CallLogItem> Filter();
}
