namespace SweetMock;

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class FixtureLog_Base(CallLog callLog, string? instanceName = null)
{
    public IEnumerable<ArgumentBase> All() =>
        callLog.Calls.Where(t => instanceName == null || t.InstanceName == instanceName);
}
