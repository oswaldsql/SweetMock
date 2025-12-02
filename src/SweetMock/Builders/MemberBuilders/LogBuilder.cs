namespace SweetMock.Builders.MemberBuilders;

using Generation;

public static class LogBuilder
{
    extension(CodeBuilder builder)
    {
        internal void InitializeLogging(MockContext context) =>
            builder.Region("Logging", builder => builder
                .Add("private global::SweetMock.CallLog _sweetMockCallLog = new global::SweetMock.CallLog();").BR()
                .Add("private void _log(global::SweetMock.ArgumentBase argument) => this._sweetMockCallLog.Add(argument);").BR()
                .AddToConfig(context, codeBuilder => codeBuilder
                    .Scope($"internal MockConfig GetCallLogs(out {context.Source.Name}_Logs callLog)", builder1 => builder1
                        .Add($"callLog = new {context.Source.Name}_Logs(this.target._sweetMockCallLog, this.target._sweetMockInstanceName);")
                        .Add("return this;"))
                )
            );
    }
}
