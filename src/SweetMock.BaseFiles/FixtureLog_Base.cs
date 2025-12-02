namespace SweetMock;

using System.Collections.Generic;

public abstract class FixtureLog_Base(CallLog callLog)
{
    public IEnumerable<ArgumentBase> All => callLog.Calls;
}
