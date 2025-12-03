namespace SweetMock;

using System.Collections.Generic;
using System.Linq;

public abstract class FixtureLog_Base(CallLog callLog, string? instanceName = null)
{
    public System.Collections.Generic.IEnumerable<ArgumentBase> All() =>
        callLog.Calls.Where(t => instanceName == null || t.InstanceName == instanceName);
}
